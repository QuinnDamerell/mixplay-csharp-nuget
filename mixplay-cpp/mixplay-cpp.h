#pragma once

#include "interactivity.h"

#define WIN32_LEAN_AND_MEAN 
#include <windows.h>

extern "C" {

    __declspec(dllexport) int __cdecl mixplay_auth_get_short_code(const char* clientId, const char* clientSecret, char* shortCode, size_t* shortCodeLength, char* shortCodeHandle, size_t* shortCodeHandleLength)
    {
        return interactive_auth_get_short_code(clientId, clientSecret, shortCode, shortCodeLength, shortCodeHandle, shortCodeHandleLength);
    }

    __declspec(dllexport) int __cdecl mixplay_auth_wait_short_code(const char* clientId, const char* clientSecret, const char* shortCodeHandle, char* refreshToken, size_t* refreshTokenLength)
    {
        return interactive_auth_wait_short_code(clientId, clientSecret, shortCodeHandle, refreshToken, refreshTokenLength);
    }   

    __declspec(dllexport) int __cdecl mixplay_auth_parse_refresh_token(const char* token, char* authorization, size_t* authorizationLength)
    {
        return interactive_auth_parse_refresh_token(token, authorization, authorizationLength);
    }

    __declspec(dllexport) int __cdecl mixplay_auth_is_token_stale(const char* token, bool* isStale)
    {
        return interactive_auth_is_token_stale(token, isStale);
    }

    __declspec(dllexport) int __cdecl mixplay_auth_refresh_token(const char* clientId, const char* clientSecret, const char* staleToken, char* refreshToken, size_t* refreshTokenLength)
    {
        return interactive_auth_refresh_token(clientId, clientSecret, staleToken, refreshToken, refreshTokenLength);
    }
}

