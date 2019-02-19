using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Mixer;

namespace mixplay_csharp_playground
{
    class Program
    {
        static string ClientId      = "f0d20e2d263b75894f5cdaabc8a344b99b1ea6f9ecb7fa4f";
        static string ShareCode     = "xe7dpqd5";
        static string InteractiveId = "135704";

        static void Main(string[] args)
        {
            // Start by creating a mix play object with the client creds.
            MixPlay mixplay = new MixPlay(ClientId, null);

            //
            // Handle Auth.
            try
            {
                // Try to read a cached auth token so we don't have to sign in if we already did.
                string authToken = ReadAuth();
                if(String.IsNullOrWhiteSpace(authToken))
                {
                    // Get a short code for auth.
                    MixPlayAuthShortCode shortCode = mixplay.GetAuthShortCode();

                    // Launch the browser to let the user login (a real program can do this however it wants)
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {shortCode.ShortCodeAuthUrl}"));

                    // Wait for the user to login.
                    mixplay.WaitForShortCodeAuthComplete();

                    // Now that auth is complete, cache the auth token so the user doesn't have to sign in again.
                    //WriteAuth(mixplay.GetAuthTokenString());
                }
                else
                {
                    // Try to use the auth token
                    mixplay.SetAuthTokenString(authToken);
                }           
            } 
            catch (MixPlayException e)
            {
                if(e.HasMixerResultCode)
                {
                    Console.WriteLine("Auth failed due to MixPlay code "+ e.MixerErrorCode);
                }
                else
                {
                    Console.WriteLine("Auth failed due to http error "+ e.HttpErrorCode);
                }
                return;
            }

            //
            // Do other stuff.
            try
            {
                mixplay.OpenSession();

                mixplay.Connect(InteractiveId, ShareCode, true);
            }
            catch (MixPlayException e)
            {
                if (e.HasMixerResultCode)
                {
                    Console.WriteLine("Auth failed due to MixPlay code " + e.MixerErrorCode);
                }
                else
                {
                    Console.WriteLine("Auth failed due to http error " + e.HttpErrorCode);
                }
                return;
            }

            Thread.Sleep(50000);

        }

        public static string ReadAuth()
        {
            try
            {
                return File.ReadAllText("Auth.txt");
            }
            catch(Exception)
            { }
            return null;
        }

        public static void WriteAuth(string auth)
        {
            // Note! This should be protected, anyone who reads this file can login to this 
            // interactive experience.
            File.WriteAllText("Auth.txt", auth);
        }
    }


}
