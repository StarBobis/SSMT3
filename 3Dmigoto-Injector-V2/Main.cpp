// Injector.cpp : Defines the entry point for the console application.
//


#include <windows.h>
#include <stdio.h>
#include <tlhelp32.h>
#include <set>
#include <string>

#include "D3dxIniUtils.hpp"

static void wait_keypress(std::string msg)
{
	puts(msg.c_str());
	getchar();
}

static void wait_exit(int code = 0, std::string msg = "\nPress enter to close...\n")
{
	wait_keypress(msg);
	exit(code);
}

static void exit_usage(const char* msg)
{
	//                                                          80 column limit --------> \n
	printf("The Loader is not configured correctly. Please copy the 3DMigoto d3d11.dll\n"
		"and d3dx.ini into this directory, then edit the d3dx.ini's [Loader] section\n"
		"to set the target executable and 3DMigoto module name.\n"
		"\n"
		"%s", msg);

	wait_exit(EXIT_FAILURE);
}

static bool verify_injection(PROCESSENTRY32* pe, const wchar_t* module, bool log_name)
{
	HANDLE snapshot;
	MODULEENTRY32 me;
	const wchar_t* basename = wcsrchr(module, '\\');
	bool rc = false;
	static std::set<DWORD> pids;
	wchar_t exe_path[MAX_PATH], mod_path[MAX_PATH];

	if (basename)
		basename++;
	else
		basename = module;

	do {
		snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, pe->th32ProcessID);
	} while (snapshot == INVALID_HANDLE_VALUE && GetLastError() == ERROR_BAD_LENGTH);

	if (snapshot == INVALID_HANDLE_VALUE) {
		DWORD lastError = GetLastError();

		/*
			Genshin Impact and Naraka Bladepoint will refuse us to read it's information
			but they allow us to inject the dll, so in this case it's successfully injected,
			but access it's information will give us a error, so we just ignore this error here.
			so the Injector itself can close automatically.

			If we don't use this method, it will keep trying to access the process 
			the Genshin Impact's anti cheat will think we are trying to do something bad
			and then it will hang our injector process, so the injector can't close itself.

			It will make user feel bad, so we just ignore this error here, to make sure it can close itself,
			so user will feel good.
		*/
		if (lastError == ERROR_ACCESS_DENIED) {
			if (!pids.count(pe->th32ProcessID)) {
				printf("%d: target process found, but it don't want us to inject, whatever, we don't care. :)\n", pe->th32ProcessID);
				pids.insert(pe->th32ProcessID);
			}
			return true; // 假设注入成功
		}

		printf("%S (%d): Unable to verify if 3DMigoto was successfully loaded: %d\n",
			pe->szExeFile, pe->th32ProcessID, lastError);
		return false;
	}

	me.dwSize = sizeof(MODULEENTRY32);
	if (!Module32First(snapshot, &me)) {
		DWORD lastError = GetLastError();

		// 同样处理Module32First的访问拒绝
		if (lastError == ERROR_ACCESS_DENIED) {
			if (!pids.count(pe->th32ProcessID)) {
				printf("%d: Unable to verify 3DMigoto loading status due to access denied - assuming success :)\n", pe->th32ProcessID);
				pids.insert(pe->th32ProcessID);
			}
			CloseHandle(snapshot);
			return true; // 假设注入成功
		}

		printf("%S (%d): Unable to verify if 3DMigoto was successfully loaded: %d\n",
			pe->szExeFile, pe->th32ProcessID, lastError);
		goto out_close;
	}

	// First module is the executable, and this is how we get the full path:
	if (log_name)
		printf("Target process found (%i): %S\n", pe->th32ProcessID, me.szExePath);
	wcscpy_s(exe_path, MAX_PATH, me.szExePath);

	rc = false;
	while (Module32Next(snapshot, &me)) {
		if (_wcsicmp(me.szModule, basename))
			continue;

		if (!_wcsicmp(me.szExePath, module)) {
			if (!pids.count(pe->th32ProcessID)) {
				printf("%d: 3DMigoto loaded :)\n", pe->th32ProcessID);
				pids.insert(pe->th32ProcessID);
			}
			rc = true;
		}
		else {
			wcscpy_s(mod_path, MAX_PATH, me.szExePath);
			wcsrchr(exe_path, L'\\')[1] = '\0';
			wcsrchr(mod_path, L'\\')[1] = '\0';
			if (!_wcsicmp(exe_path, mod_path)) {
				printf("\n\n\n"
					"WARNING: Found a second copy of 3DMigoto loaded from the game directory:\n"
					"%S\n"
					"This may crash - please remove the copy in the game directory and try again\n\n\n",
					me.szExePath);
				wait_exit(EXIT_FAILURE);
			}
		}
	}

out_close:
	CloseHandle(snapshot);
	return rc;
}

static bool check_for_running_target(wchar_t* target, const wchar_t* module)
{
	// https://docs.microsoft.com/en-us/windows/desktop/ToolHelp/taking-a-snapshot-and-viewing-processes
	HANDLE snapshot;
	PROCESSENTRY32 pe;
	bool rc = false;
	wchar_t* basename = wcsrchr(target, '\\');
	static std::set<DWORD> pids;

	if (basename)
		basename++;
	else
		basename = target;

	snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
	if (snapshot == INVALID_HANDLE_VALUE) {
		printf("Unable to verify if 3DMigoto was successfully loaded: %d\n", GetLastError());
		return false;
	}

	pe.dwSize = sizeof(PROCESSENTRY32);
	if (!Process32First(snapshot, &pe)) {
		printf("Unable to verify if 3DMigoto was successfully loaded: %d\n", GetLastError());
		goto out_close;
	}

	do {
		if (_wcsicmp(pe.szExeFile, basename))
			continue;

		rc = verify_injection(&pe, module, !pids.count(pe.th32ProcessID)) || rc;
		pids.insert(pe.th32ProcessID);
	} while (Process32Next(snapshot, &pe));

out_close:
	CloseHandle(snapshot);
	return rc;
}

static void wait_for_target(const char* target_a, const wchar_t* module_path, bool wait, int delay, bool launched)
{
	wchar_t target_w[MAX_PATH];

	if (!MultiByteToWideChar(CP_UTF8, 0, target_a, -1, target_w, MAX_PATH))
		return;

	for (int seconds = 0; wait || delay == -1; seconds++) {
		if (check_for_running_target(target_w, module_path) && delay != -1)
			break;
		Sleep(1000);

		if (launched && seconds == 3) {
			printf("\nStill waiting for the game to start...\n"
				"If the game does not launch automatically, leave this window open and run it manually.\n"
				"You can also adjust/remove the [Loader] launch= option in the d3dx.ini as desired.\n\n");
		}
	}

	for (int i = delay; i > 0; i--) {
		printf("Shutting down loader in %i...\r", i);
		Sleep(1000);
		check_for_running_target(target_w, module_path);
	}
	printf("\n");
}


wchar_t* deduce_working_directory(wchar_t* setting, wchar_t dir[MAX_PATH])
{
	DWORD ret;
	wchar_t* file_part = NULL;

	ret = GetFullPathName(setting, MAX_PATH, dir, &file_part);
	if (!ret || ret >= MAX_PATH)
		return NULL;

	ret = GetFileAttributes(dir);
	if (ret == INVALID_FILE_ATTRIBUTES)
		return NULL;

	if (!(ret & FILE_ATTRIBUTE_DIRECTORY) && file_part)
		*file_part = '\0';

	printf("Using working directory: \"%S\"\n", dir);

	return dir;
}

int main()
{
	wchar_t setting_w[MAX_PATH], working_dir[MAX_PATH], * working_dir_p = NULL;
	wchar_t module_full_path[MAX_PATH];
	int rc = EXIT_FAILURE;
	HMODULE module;
	int hook_proc;
	FARPROC fn;
	HHOOK hook;
	bool launch;

	CreateMutexA(0, FALSE, "Local\\3DMigotoLoader");
	if (GetLastError() == ERROR_ALREADY_EXISTS)
		wait_exit(EXIT_FAILURE, "ERROR: Another instance of the 3DMigoto Loader is already running. Please close it and try again\n");

	printf("\n------------------------------- 3DMigoto Loader V2 ------------------------------\n\n");
	printf("This program is modified by NicoMico based on the 3Dmigoto source code and is exclusively included in the SSMT Release for free distribution. It is completely free! If you paid any amount of money to obtain it, you have definitely been scammed!\n\n");
	printf("\n----------------------------------------------------------------------------------\n\n");

	/*
		In China, resell open source tool is very common issue,
		but most of the scene is resell the compiled binary directly without any modification,
		so add a tip here is necessary.
		
		Most of people who can compile C++ code by themselves will not resell the tool in most case.
		So the tip is important to prevent some annoying people, and also let people know this tool is free.

		Notice some computer can't show Chinese character, so we just use English here.
	*/


	D3dxIniUtils d3dxIniUtils(L"d3dx.ini");
	
	if (d3dxIniUtils.parse_error != L"") {
		wait_exit(EXIT_FAILURE, d3dxIniUtils.ToByteString(d3dxIniUtils.parse_error).c_str());
	}


	module = LoadLibraryA(d3dxIniUtils.ToByteString(d3dxIniUtils.module).c_str());
	if (!module) {
		printf("Unable to load 3DMigoto \"%s\"\n", d3dxIniUtils.ToByteString(d3dxIniUtils.module).c_str());
		wait_exit(EXIT_FAILURE);
	}

	GetModuleFileName(module, module_full_path, MAX_PATH);
	printf("Loaded %S\n\n", module_full_path);


	fn = GetProcAddress(module, "CBTProc");
	if (!fn) {
		wait_exit(EXIT_FAILURE, "Module does not support injection method\n"
			"Make sure this is a recent 3DMigoto d3d11.dll\n");
	}

	//We don't need to read it ,we just use WH_CBT.
	//hook_proc = find_ini_int_lite(ini_section, "hook_proc", WH_CBT);

	hook_proc = WH_CBT;

	//WH_SHELL is also works good,but since we always use WH_CBT, we will keep use WH_CBT until some thing happens.
	//hook_proc = WH_SHELL;

	hook = SetWindowsHookEx(hook_proc, (HOOKPROC)fn, module, 0);
	if (!hook)
		wait_exit(EXIT_FAILURE, "Error installing hook\n");

	rc = EXIT_SUCCESS;


	launch = d3dxIniUtils.launch != L"";
	if (launch) {
        // 直接使用 d3dxIniUtils.launch（wstring）作为可执行路径

        wchar_t run_path_wchar_t[MAX_PATH];
        wcsncpy_s(run_path_wchar_t, MAX_PATH, d3dxIniUtils.launch.c_str(), _TRUNCATE); 

        wchar_t run_args_wchar_t[MAX_PATH];
        wcsncpy_s(run_args_wchar_t, MAX_PATH, d3dxIniUtils.launch_args.c_str(), _TRUNCATE);

		CoInitializeEx(NULL, COINIT_APARTMENTTHREADED | COINIT_DISABLE_OLE1DDE);

		//if (!MultiByteToWideChar(CP_UTF8, 0, d3dxIniUtils.ToByteString(d3dxIniUtils.launch).c_str(), -1, setting_w, MAX_PATH))
		//	wait_exit(EXIT_FAILURE, "Invalid launch setting\n");

		working_dir_p = deduce_working_directory(run_path_wchar_t,working_dir);

		ShellExecute(NULL, NULL, run_path_wchar_t, run_args_wchar_t, working_dir_p, SW_SHOWNORMAL);
	}
	else {
		printf("3DMigoto ready - Now run the game.\n");
	}

	wait_for_target(d3dxIniUtils.ToByteString(d3dxIniUtils.target).c_str(), module_full_path, true, 5, launch);

	UnhookWindowsHookEx(hook);

	return rc;
}

