#include <Windows.h>
#include <iostream>
#include <fstream>
#include <dbghelp.h>

HRESULT GenerateCrashDump(MINIDUMP_TYPE flags, EXCEPTION_POINTERS *seh);

DWORD WINAPI DllMain(DWORD dontCare1, DWORD fdwReason, DWORD dontCare2)
{
	return TRUE;
}

const char* GetEntityTypeString(WORD Type)
{
	if (Type > 20)
	{
		return "Event";
	}
	else
	{
		return (const char*)*(DWORD*)(0x00B6CB48 + (Type * sizeof(const char*)));
	}
}

const char*(*Unk_GetEntityClass)(DWORD Address, DWORD Two) = (const char*(*)(DWORD Address, DWORD Two))0x0043CF90;
const char*(*SL_ConvertToString)(int stringValue, int scriptInstance) = (const char*(*)(int stringValue, int scriptInstance))0x00687530;

const int* numGEntities = (const int*)0x01C0314C;
const WORD* check = (WORD*)0x023A5648;
const DWORD EntityArrayBase = 0x01A7981C;

typedef struct
{
	DWORD Base;
	DWORD WriteTo;
	DWORD CurrentSize;
	DWORD MaxSize;
} InfoString;

bool AffixString(InfoString* ReturnString, const char* toAffix)
{
	int length = strlen(toAffix);

	if (length + ReturnString->CurrentSize > ReturnString->MaxSize)
	{
		return false;
	}

	ReturnString->CurrentSize += length;

	strcpy_s((char*)ReturnString->WriteTo, ReturnString->MaxSize - ReturnString->CurrentSize, toAffix);
	
	ReturnString->WriteTo = ReturnString->WriteTo + length;

	return true;
}

extern "C" __declspec(dllexport) DWORD GetEntityCount()
{
	DWORD CurrentEntity = EntityArrayBase;
	int EntityCounter = 0;
	DWORD LiveEntities = 0;

	do
	{
		if (*((BYTE*)CurrentEntity - 71))
		{
			LiveEntities++;
		}

		CurrentEntity += 844;
		EntityCounter++;
	} while (EntityCounter < *numGEntities);

	return LiveEntities;
}

const char* ResolveExceptionCode(DWORD Exception) //dumb
{
	switch (Exception)
	{
	case EXCEPTION_ACCESS_VIOLATION:
		return "EXCEPTION_ACCESS_VIOLATION";
	case EXCEPTION_ARRAY_BOUNDS_EXCEEDED:
		return "EXCEPTION_ARRAY_BOUNDS_EXCEEDED";
	case EXCEPTION_BREAKPOINT:
		return "EXCEPTION_BREAKPOINT";
	case EXCEPTION_DATATYPE_MISALIGNMENT:
		return "EXCEPTION_DATATYPE_MISALIGNMENT";
	case EXCEPTION_FLT_DENORMAL_OPERAND:
		return "EXCEPTION_FLT_DENORMAL_OPERAND";
	case EXCEPTION_FLT_DIVIDE_BY_ZERO:
		return "EXCEPTION_FLT_DIVIDE_BY_ZERO";
	case EXCEPTION_FLT_INEXACT_RESULT:
		return "EXCEPTION_FLT_INEXACT_RESULT";
	case EXCEPTION_FLT_INVALID_OPERATION:
		return "EXCEPTION_FLT_INVALID_OPERATION";
	case EXCEPTION_FLT_OVERFLOW:
		return "EXCEPTION_FLT_OVERFLOW";
	case EXCEPTION_FLT_STACK_CHECK:
		return "EXCEPTION_FLT_STACK_CHECK";
	case EXCEPTION_FLT_UNDERFLOW:
		return "EXCEPTION_FLT_UNDERFLOW";
	case EXCEPTION_ILLEGAL_INSTRUCTION:
		return "EXCEPTION_ILLEGAL_INSTRUCTION";
	case EXCEPTION_IN_PAGE_ERROR:
		return "EXCEPTION_IN_PAGE_ERROR";
	case EXCEPTION_INT_DIVIDE_BY_ZERO:
		return "EXCEPTION_INT_DIVIDE_BY_ZERO";
	case EXCEPTION_INT_OVERFLOW:
		return "EXCEPTION_INT_OVERFLOW";
	case EXCEPTION_INVALID_DISPOSITION:
		return "EXCEPTION_INVALID_DISPOSITION";
	case EXCEPTION_NONCONTINUABLE_EXCEPTION:
		return "EXCEPTION_NONCONTINUABLE_EXCEPTION";
	case EXCEPTION_PRIV_INSTRUCTION:
		return "EXCEPTION_PRIV_INSTRUCTION";
	case EXCEPTION_SINGLE_STEP:
		return "EXCEPTION_SINGLE_STEP";
	case EXCEPTION_STACK_OVERFLOW:
		return "EXCEPTION_STACK_OVERFLOW";
	default:
		return "UNKNOWN";
	}
}

LPTOP_LEVEL_EXCEPTION_FILTER OldExceptionFilter = 0x0;

LONG WINAPI SuperMegaUnhandledExceptionFilter(_EXCEPTION_POINTERS *ExceptionInfo)
{
	char buffer[2048];

	HRESULT CrashDumpResult = GenerateCrashDump((MINIDUMP_TYPE)(MiniDumpNormal | MiniDumpWithHandleData | MiniDumpWithUnloadedModules | MiniDumpWithProcessThreadData | MiniDumpWithThreadInfo | MiniDumpWithTokenInformation), ExceptionInfo);

	MessageBeep(MB_ICONERROR);
	sprintf_s(buffer, "Exception Code: 0x%X (%s) ocurred at address: 0x%X\n\nA crash dump has been saved in the games folder.", ExceptionInfo->ExceptionRecord->ExceptionCode, ResolveExceptionCode(ExceptionInfo->ExceptionRecord->ExceptionCode), ExceptionInfo->ExceptionRecord->ExceptionAddress);
	MessageBoxA(NULL, buffer, "Exception Caught", MB_OK);

	if (CrashDumpResult != S_OK)
	{
		sprintf_s(buffer, "There was an error writing the crash log.\n\nError: 0x%X", CrashDumpResult);
		MessageBoxA(NULL, buffer, "Error", MB_OK);
	}

	return EXCEPTION_EXECUTE_HANDLER;
}

extern "C" __declspec(dllexport) bool EnableSEHHook()
{
	if (OldExceptionFilter == NULL)
	{
		//Hasn't been set
		OldExceptionFilter = SetUnhandledExceptionFilter(SuperMegaUnhandledExceptionFilter);

		return true;
	}
	else
	{
		//Has already been set!
		return false;
	}
}

extern "C" __declspec(dllexport) bool DisableSEHHook()
{
	if (OldExceptionFilter == NULL)
	{
		//Hasn't been set
		return false;
	}
	else
	{
		//Has already been set!
		SetUnhandledExceptionFilter(OldExceptionFilter);

		OldExceptionFilter = 0;

		return true;
	}
}

extern "C" __declspec(dllexport) DWORD GetEntities()
{
	InfoString ReturnString;
	ReturnString.MaxSize = 65536;
	ReturnString.CurrentSize = 0;
	ReturnString.Base = (DWORD)malloc(ReturnString.MaxSize);
	ReturnString.WriteTo = ReturnString.Base;

	if (!ReturnString.Base)
	{
		return 0;
	}

	ZeroMemory((void*)ReturnString.Base, ReturnString.MaxSize);

	DWORD CurrentEntity = EntityArrayBase;
	int EntityCounter = 0;
	char buffer[256];

	if (*numGEntities == 0)
	{
		return 0;
	}

	do
	{
		if (*((BYTE*)CurrentEntity - 71))
		{
			const char* EntityType = GetEntityTypeString(*((WORD*)CurrentEntity - 51));
			const char* EntityClass = Unk_GetEntityClass(*((WORD*)CurrentEntity + 28), 0);
			const char* EntityName = "-";
			const char* EntityTarget = "-";
			const char* EntityModel = "-";

			DWORD HasName = *((WORD *)CurrentEntity + 32);

			if (HasName)
			{
				EntityName = SL_ConvertToString(HasName, 0);
			}

			DWORD HasTarget = *((WORD*)CurrentEntity + 31);

			if (HasTarget)
			{
				EntityTarget = SL_ConvertToString(HasTarget, 0);
			}

			if (*check == *((WORD*)CurrentEntity + 28))
			{
				DWORD ModelIndex = *((WORD*)CurrentEntity + 24);
				if (ModelIndex)
				{
					EntityModel = (const char*)**(DWORD**)(0x01CFF7E0 + (ModelIndex * sizeof(int)));
				}
			}

			//sprintf_s(buffer, "[%d] Type: %s, Class: %s, Name: %s, Target: %s, Model: %s", EntityCounter, EntityType, EntityClass, EntityName, EntityTarget, EntityModel);
			sprintf_s(buffer, "%d:%s:%s:%s:%s:%s;", EntityCounter, EntityType, EntityClass, EntityName, EntityTarget, EntityModel);
			AffixString(&ReturnString, buffer);
		}
		//else
		//{
			//sprintf_s(buffer, "%d::-::-::-::-::-;;;", EntityCounter);
			//AffixString(&ReturnString, buffer);

			//sprintf_s(buffer, "[%d] Entity not valid", EntityCounter);
			//con(buffer);
		//}

		CurrentEntity += 844;
		EntityCounter++;
	} while (EntityCounter < *numGEntities);

	return ReturnString.Base;
}

//From: https://blogs.msdn.microsoft.com/joshpoley/2008/05/19/prolific-usage-of-minidumpwritedump-automating-crash-dump-analysis-part-0/
HRESULT GenerateCrashDump(MINIDUMP_TYPE flags, EXCEPTION_POINTERS *seh)
{
	HRESULT error = S_OK;
	// get the time
	SYSTEMTIME sysTime = { 0 };
	GetSystemTime(&sysTime);

	// build the filename: APPNAME_COMPUTERNAME_DATE_TIME.DMP
	char path[MAX_PATH] = { 0 };

	sprintf_s(path, ARRAYSIZE(path), "BlackOpsBOEM_%04u-%02u-%02u_%02u-%02u-%02u_%08X.dmp", sysTime.wYear, sysTime.wMonth, sysTime.wDay, sysTime.wHour, sysTime.wMinute, sysTime.wSecond, GetCurrentThreadId());

	// open the file
	HANDLE hFile = CreateFileA(path, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_DELETE | FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);

	if (hFile == INVALID_HANDLE_VALUE)
	{
		error = GetLastError();
		error = HRESULT_FROM_WIN32(error);
		return error;
	}

	// get the process information
	HANDLE hProc = GetCurrentProcess();
	DWORD procID = GetProcessId(hProc);

	// if we have SEH info, package it up
	MINIDUMP_EXCEPTION_INFORMATION sehInfo = { 0 };
	MINIDUMP_EXCEPTION_INFORMATION *sehPtr = NULL;

	if (seh)
	{
		sehInfo.ThreadId = GetCurrentThreadId();
		sehInfo.ExceptionPointers = seh;
		sehInfo.ClientPointers = FALSE;
		sehPtr = &sehInfo;
	}

	// generate the crash dump
	BOOL result = MiniDumpWriteDump(hProc, procID, hFile, flags, sehPtr, NULL, NULL);

	if (!result)
	{
		error = (HRESULT)GetLastError(); // already an HRESULT
	}

	// close the file
	CloseHandle(hFile);

	return error;
}