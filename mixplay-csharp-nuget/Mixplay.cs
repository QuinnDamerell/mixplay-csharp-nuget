using Newtonsoft.Json;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Mixer
{
    // From mixer_result_code
    public enum MixerResultCode
    {
        MIXER_OK,
        MIXER_ERROR,
        MIXER_ERROR_AUTH,
        MIXER_ERROR_AUTH_DENIED,
        MIXER_ERROR_AUTH_INVALID_TOKEN,
        MIXER_ERROR_BUFFER_SIZE,
        MIXER_ERROR_CANCELLED,
        MIXER_ERROR_HTTP,
        MIXER_ERROR_INIT,
        MIXER_ERROR_INVALID_CALLBACK,
        MIXER_ERROR_INVALID_CLIENT_ID,
        MIXER_ERROR_INVALID_OPERATION,
        MIXER_ERROR_INVALID_POINTER,
        MIXER_ERROR_INVALID_PROPERTY_TYPE,
        MIXER_ERROR_INVALID_VERSION_ID,
        MIXER_ERROR_JSON_PARSE,
        MIXER_ERROR_METHOD_CREATE,
        MIXER_ERROR_NO_HOST,
        MIXER_ERROR_NO_REPLY,
        MIXER_ERROR_OBJECT_NOT_FOUND,
        MIXER_ERROR_PROPERTY_NOT_FOUND,
        MIXER_ERROR_TIMED_OUT,
        MIXER_ERROR_UNKNOWN_METHOD,
        MIXER_ERROR_UNRECOGNIZED_DATA_FORMAT,
        MIXER_ERROR_WS_CLOSED,
        MIXER_ERROR_WS_CONNECT_FAILED,
        MIXER_ERROR_WS_DISCONNECT_FAILED,
        MIXER_ERROR_WS_READ_FAILED,
        MIXER_ERROR_WS_SEND_FAILED,
        MIXER_ERROR_NOT_CONNECTED,
        MIXER_ERROR_OBJECT_EXISTS,
        MIXER_ERROR_INVALID_STATE,
        MIXER_SDK_ERROR,
        None
    }

    public class MixPlayException : Exception
    {
        // Indicates is the exception is a mixer error code or http.
        public bool HasMixerResultCode;

        // The error code returned by MixerPlay.
        public MixerResultCode MixerErrorCode = MixerResultCode.None;

        // The http error code returned.
        public int HttpErrorCode = 0;

        // This is called if the C# SDK has an error.
        public MixPlayException(string str)
            : base(str)
        {
            HasMixerResultCode = true;
            MixerErrorCode = MixerResultCode.MIXER_SDK_ERROR;
        }

        public MixPlayException(int code) 
            : base(CreateErrorMessage(code))
        {
            if(IsMixerResultCode(code))
            {
                HasMixerResultCode = true;
                MixerErrorCode = (MixerResultCode)code;
            }
            else
            {
                HasMixerResultCode = false;
                HttpErrorCode = code;
            }
        }

        private static bool IsMixerResultCode(int code)
        {
            // If the code is > results, it's usually an http error code.
            if (code > Enum.GetNames(typeof(MixerResultCode)).Length)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static string CreateErrorMessage(int code)
        {
            if(IsMixerResultCode(code))
            {
                return $"MixPlay returned the error: {Enum.GetName(typeof(MixerResultCode), code)} ({code})";
            }
            else
            {
                return $"MixPlay returned the Http Error Code:{code}";
            }
        }
    }

    // Represents the result of the Get Short Code call
    public class MixPlayAuthShortCode
    {
        public string ShortCode;
        public string ShortCodeAuthUrl;
    }

    class MixPlayAuthContext
    {
        public string RefreshToken;
        public string AuthToken;
    }

    public class MixPlay
    {
        const string c_mixerShortCodeUrl = "https://www.mixer.com/go?code=";
        const int c_shortCodeLen = 20;
        const int c_shortCodeHnadleLen = 1024;
        const int c_authRefreshTokenLen = 1024;
        const int c_authTokenLen = 1024;

        #region DllInports

        [DllImport("MixPlay.dll")]
        static extern int mixplay_auth_get_short_code(string clientId, string clientSecret, StringBuilder shortCode, ref UInt64 shortCodeLength, StringBuilder shortCodeHandle, ref UInt64 shortCodeHandleLength);

        [DllImport("MixPlay.dll")]
        static extern int mixplay_auth_wait_short_code(string clientId, string clientSecret, string shortCodeHandle, StringBuilder refreshToken, ref UInt64 refreshTokenLen);

        [DllImport("MixPlay.dll")]
        static extern int mixplay_auth_parse_refresh_token(string token, StringBuilder authorization, ref UInt64 authorizationLen);

        [DllImport("MixPlay.dll")]
        static extern int mixplay_auth_is_token_stale(string token, ref bool isStale);

        [DllImport("MixPlay.dll")]
        static extern int mixplay_auth_refresh_token(string clientId, string clientSecret, string staleToken, StringBuilder refreshToken, ref UInt64 refreshTokenLength);

        #endregion

        // General
        string m_clientId;
        string m_clientSecret;

        // Auth
        string m_shortCodeHandle = null;
        MixPlayAuthContext m_authContext = null;

        public MixPlay(string clientId, string clientSecret)
        {
            if(String.IsNullOrWhiteSpace(clientId))
            {
                throw new MixPlayException("A client id is required!");
            }
            m_clientId = clientId;
            m_clientSecret = clientSecret;
        }

        #region Auth

        // Called get short code for auth. This short code should be presented to the user
        // so they can log in via the mixer website.
        // Returns the short code and URL or an error.
        public MixPlayAuthShortCode GetAuthShortCode()
        {
            // Clear the current handle.
            m_shortCodeHandle = null;

            // Get the new code.
            UInt64 shortLen = c_shortCodeLen;
            UInt64 shortHandleLen = c_shortCodeHnadleLen;
            StringBuilder shortCode = new StringBuilder(c_shortCodeLen);
            StringBuilder shortCodeHandle = new StringBuilder(c_shortCodeHnadleLen);
            int ret = mixplay_auth_get_short_code(m_clientId, m_clientSecret, shortCode, ref shortLen, shortCodeHandle, ref shortHandleLen);
            if(ret != 0)
            {
                throw new MixPlayException(ret);
            }

            // Capture the short code handle.
            m_shortCodeHandle = shortCodeHandle.ToString();

            // Return.
            return new MixPlayAuthShortCode()
            {
                ShortCode = shortCode.ToString(),
                ShortCodeAuthUrl = c_mixerShortCodeUrl + shortCode.ToString()
            };
        }

        // Should be called after the short code has been give to the user to log in.
        // This function will block the calling thread until the auth has been done.
        // Throws an exception if an error occurs.
        public void WaitForShortCodeAuthComplete()
        {
            if(String.IsNullOrWhiteSpace(m_shortCodeHandle))
            {
                throw new MixPlayException("You must call GetAuthShortCode first!");
            }

            UInt64 tokenLen = c_authRefreshTokenLen;
            StringBuilder refreshToken = new StringBuilder(c_authRefreshTokenLen);
            int ret = mixplay_auth_wait_short_code(m_clientId, m_clientSecret, m_shortCodeHandle, refreshToken, ref tokenLen);

            // Clear the handle
            m_shortCodeHandle = null;

            if (ret != 0)
            {
                throw new MixPlayException(ret);
            }

            // Setup the auth token
            ImportRefreshToken(refreshToken.ToString());  
        }

        // Returns the current authorization token string.
        // This string should be serialized to disk and can be given to the SDK to keep the user logged in.
        public string GetAuthTokenString()
        {
            if(m_authContext == null)
            {
                return "";
            }
            return JsonConvert.SerializeObject(m_authContext);
        }

        // Called to give the SDK a previously created auth token. This will allow the user to remain logged in. 
        public void SetAuthTokenString(string authToken)
        {
            MixPlayAuthContext context = JsonConvert.DeserializeObject<MixPlayAuthContext>(authToken);
            if(String.IsNullOrEmpty(context.AuthToken) || String.IsNullOrWhiteSpace(context.RefreshToken))
            {
                throw new MixPlayException("Invalid Auth Token Given!");
            }
            m_authContext = context;

            // We can check if the token is stale with mixplay_auth_is_token_stale, but we will always just refresh it to be safe.
            RefreshAuthToken();
        }

        // Assuming the client is already authed, this forces the token to be refreshed.
        public void RefreshAuthToken()
        {
            MixPlayAuthContext oldContext = m_authContext;
            if(oldContext == null)
            {
                throw new MixPlayException("No previous token was found to refresh.");
            }
     
            UInt64 refreshTokenLen = c_authRefreshTokenLen;
            StringBuilder newRefreshToken = new StringBuilder(c_authRefreshTokenLen);
            int ret = mixplay_auth_refresh_token(m_clientId, m_clientSecret, oldContext.RefreshToken, newRefreshToken, ref refreshTokenLen);
            if (ret != 0)
            {
                throw new MixPlayException(ret);
            }

            // Handle the new token
            ImportRefreshToken(newRefreshToken.ToString());            
        }

        private void ImportRefreshToken(string refreshToken)
        {
            if(String.IsNullOrEmpty(refreshToken))
            {
                throw new MixPlayException("Refresh Token was null!");
            }

            UInt64 authTokenLen = c_authTokenLen;
            StringBuilder authToken = new StringBuilder(c_authTokenLen);
            int ret = mixplay_auth_parse_refresh_token(refreshToken, authToken, ref authTokenLen);
            if(ret != 0)
            {
                throw new MixPlayException(ret);
            }

            m_authContext = new MixPlayAuthContext()
            {
                RefreshToken = refreshToken,
                AuthToken = authToken.ToString()
            };
        }

        #endregion
    }
}
