// NamedPipeWrapper.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"

#include "NamedPipeWrapper.h"

const int BUF_SIZE = 1024;
__declspec(thread) static wchar_t GBL_errorMessage[BUF_SIZE] = L"";

const wchar_t* PIPE_PREFIX = L"\\\\.\\pipe\\";

const int PIPE_BUF_SIZE = 102400;

static void saveErrorMessage(wchar_t* funcName) {
    StringCbPrintf(GBL_errorMessage, sizeof(GBL_errorMessage), L"%s failed with %lu", funcName, GetLastError());
}

JNIEXPORT jstring JNICALL Java_npw_NamedPipeWrapper_getErrorMessage(JNIEnv* pEnv, jclass cls) {
    return pEnv->NewString((const jchar*)GBL_errorMessage, wcslen(GBL_errorMessage);
}

JNIEXPORT jlong JNICALL Java_npw_NamedPipeWrapper_createPipe(JNIEnv* pEnv, jclass cls, jstring javaName, jboolean userLocal) {
    HANDLE pipeHandle = NULL;
    const jchar* pipeName = NULL;
    SECURITY_ATTRIBUTES* pSA = NULL;
    wchar_t fullPipeName[256];
    try {
        pipeName = pEnv->GetStringChars(javaName, NULL);
        if (pipeName == NULL) {
            wsprintf(GBL_errorMessage, L"JNI out of memory");
            throw 1;
        }
        if (userLocal) {
            wchar_t userName[200];
            unsigned long nameBufSize = sizeof(userName);
            if (!GetUserName(userName, &nameBufSize)) {
                saveErrorMessage(L"GetUserName");
                throw 1;
            }
            HRESULT hr = StringCbPrintf(fullPipeName, sizeof(fullPipeName), L"%s%s\\%s", PIPE_PREFIX, userName, pipeName);
            if (FAILED(hr)) {
                wsprintf(GBL_errorMessage, L"When combined with user name and prefix, pipe name is too long");
                throw 1;
            }
            pSA = new SECURITY_ATTRIBUTES;
            if (pSA == NULL) {
                wsprintf(GBL_errorMessage, L"Out of memory");
                throw 1;
            }
            pSA->nLength = sizeof(SECURITY_ATTRIBUTES);
            pSA->bInheritHandle = FALSE;
            pSA->lpSecurityDescriptor = NULL;
            wchar_t* sddl = L"A;;GA;;;CO";
            if (!ConvertStringSecurityDescriptorToSecurityDescriptor(sddl, SDDL_REVISION_1, &(pSA->lpSecurityDescriptor), NULL)) {
                saveErrorMessage(L"ConvertStringSecurityDescriptorToSecurityDescriptor");
                throw 1;
            }
        } else {
            HRESULT hr = StringCbPrintf(fullPipeName, sizeof(fullPipeName), L"%s%s", PIPE_PREFIX, pipeName);
            if (FAILED(hr)) {
                wsprintf(GBL_errorMessage, L"When combined with prefix, pipe name is too long");
                throw 1;
            }
        }

        pipeHandle = CreateNamedPipe(fullPipeName, PIPE_ACCESS_DUPLEX | FILE_FLAG_FIRST_PIPE_INSTANCE,
                PIPE_TYPE_MESSAGE | PIPE_READMODE_MESSAGE | PIPE_WAIT | PIPE_REJECT_REMOTE_CLIENTS,
                PIPE_UNLIMITED_INSTANCES, PIPE_BUF_SIZE, PIPE_BUF_SIZE, 0, pSA);
        if (pipeHandle == INVALID_HANDLE_VALUE) {
            saveErrorMessage(L"CreateNamedPipe");
            throw 1;
        }
             
    } catch (...) {}

    if (pipeName != NULL ){
        pEnv->ReleaseStringChars(javaName, pipeName);
    }
    if (pSA != NULL) {
        if (pSA->lpSecurityDescriptor != NULL) {
            LocalFree(pSA->lpSecurityDescriptor);
        }
        delete pSA;
    }
    return (jlong)pipeHandle;
}

JNIEXPORT jboolean JNICALL Java_npw_NamedPipeWrapper_closePipe(JNIEnv* pEnv, jclass cls, jlong pipeHandle) {
    if (! CloseHandle((HANDLE)pipeHandle)) {
        saveErrorMessage(L"CloseHandle");
        return FALSE;
    }
    return TRUE;
}

JNIEXPORT jboolean JNICALL Java_npw_namedPipeWrapper_connectPipe(JNIEnv* pEnv, jclass cls, jlong pipeHandle) {
    if (! ConnectNamedPipe((HANDLE)pipeHandle, NULL)) {
        saveErrorMessage(L"ConnectNamedPipe");
        return FALSE;
    }
    return TRUE;
}

JNIEXPORT jstring JNICALL Java_npw_namedPipeWrapper_readPipe(JNIEnv* pEnv, jclass cls, jlong pipeHandle) {
    std::vector<unsigned char> buf;

    bool done = false;
    bool error = false;
    while (!done) {
        const int READ_BUF_SIZE = 25 * 1024;
        char readBuffer[READ_BUF_SIZE];
        unsigned long bytesRead = 0;
        BOOL result = ReadFile((HANDLE)pipeHandle, readBuffer, sizeof(readBuffer), &bytesRead, NULL);
        DWORD dwError = GetLastError();
        if (result || (!result && dwError == ERROR_MORE_DATA)) {

