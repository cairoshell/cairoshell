// Windows System Hooks library
// Developed for Cairo Shell, December 2008
//
// Provides native hooks into the system to handle SysTray messages and Task bar hook notification.
// Allowing the hooks to be setup and callbacks sent via the .NET framework (Platform invocation)


#include "stdafx.h"
#include <ShellAPI.h>
#include <Winuser.h>
#include "WindowsHooks.h"

	// Callbacks for the delegates
	CALLBACK_NOTIFYICON_FUNCTION pSystrayFunction;
	CALLBACK_TASK_FUNCTION pTaskFunction;
	
	// Member variables&
	WNDCLASS	   m_TrayClass;
 	HWND		   m_hWndTray;
 	WNDCLASS	   m_NotifyClass;
 	HWND		   m_hWndNotify;
	HINSTANCE	   m_hInstance;
	HHOOK		   m_ShellHook;

	// Forward declaration for WndProc
	extern "C" 
	{
		LRESULT CALLBACK WndProc(HWND, UINT, WPARAM, LPARAM);
		LRESULT CALLBACK ShellProc(int, WPARAM, LPARAM);
	}

BOOL WINAPI DllMain(HINSTANCE hInstDll, DWORD dwReason, LPVOID reserved)
{
	m_hInstance = hInstDll;
	return TRUE;
}

void SetSystrayCallback(LPVOID theCallbackFunctionAddress)
{
	pSystrayFunction = (CALLBACK_NOTIFYICON_FUNCTION)theCallbackFunctionAddress;
	ODS("Systray callback set.\n");
}

void SetTaskCallback(LPVOID taskCallbackAddress)
{
	pTaskFunction = (CALLBACK_TASK_FUNCTION)taskCallbackAddress;
	ODS("Task callback set.\n");
}

void InitializeSystray()
{
	memset(&m_TrayClass, 0, sizeof(m_TrayClass));
	
	m_TrayClass.lpszClassName = L"Shell_TrayWnd";
	m_TrayClass.lpfnWndProc = WndProc;
	m_TrayClass.style = CS_DBLCLKS;
	m_TrayClass.hInstance = m_hInstance;

	RegisterClass( &m_TrayClass );

	m_hWndTray = CreateWindowEx(	WS_EX_TOPMOST | WS_EX_TOOLWINDOW,
								TEXT("Shell_TrayWnd"),
								TEXT(""),
								WS_POPUP,
								0, 0,
								0, 0,
								NULL,
								NULL,
								m_hInstance,
								NULL);

	ODS("Shell_TrayWnd Created\n");

	memset(&m_NotifyClass,0, sizeof(m_NotifyClass));
     m_NotifyClass.lpszClassName = TEXT("TrayNotifyWnd");
     m_NotifyClass.lpfnWndProc = WndProc;
     //m_NotifyClass.cbSize = sizeof(WNDCLASSEX);
     m_NotifyClass.hInstance = m_hInstance;
     m_NotifyClass.style = CS_DBLCLKS;
     
     RegisterClass(&m_NotifyClass);
        
	 m_hWndNotify = CreateWindowEx( 0,
              TEXT("TrayNotifyWnd"), 
              NULL,
              WS_CHILD,
              0, 0, 0, 0,
              m_hWndTray, NULL,
              m_hInstance,
              NULL);

	 ODS("TrayNotifyWnd Created\n");
}


void InitializeTask()
{
	m_ShellHook = SetWindowsHookEx(WH_SHELL, (HOOKPROC)ShellProc, m_hInstance, 0);
	if(m_ShellHook != NULL)
	{
		ODS("Shell hook created\n");
	}
}
void Run()
{
	SendNotifyMessage(HWND_BROADCAST, RegisterWindowMessage(L"TaskbarCreated"), 0, 0);
	ODS("Sent TaskbarCreated message.\n");
}

void ShutdownAll()
{
	ODS("Shutting down SysTray and Tasks.\n");
	ShutdownSystray();
	ShutdownTask();
}

void ShutdownSystray()
{
	if(m_hWndTray != NULL)
	{
		DestroyWindow(m_hWndTray);
		UnregisterClass(L"TrayNotifyWnd", m_hInstance);
		ODS("TrayNotifyWnd destroyed.\n");
	}

	if(m_hWndNotify != NULL)
	{
		DestroyWindow(m_hWndNotify);
		UnregisterClass(L"Shell_TrayWnd", m_hInstance);
		ODS("Shell_TrayWnd destroyed.\n");
	}
}


void ShutdownTask()
{
	if(m_ShellHook != NULL)
	{
		UnhookWindowsHookEx(m_ShellHook);
		ODS("ShellHook unhooked.");
	}
}

BOOL CallSystrayDelegate(int message, NOTIFYICONDATA nicData)
{
	if(pSystrayFunction != NULL)
	{
		ODS("Calling Systray Delegate");
		return (pSystrayFunction)(message, nicData);	
	}
	else
	{
		ODS("Attempted to call the Systray Delegate, however the pointer is null");
		return FALSE;
	}
}

BOOL CallTaskDelegate(int nCode, TASKINFORMATION* taskInfo)
{
	if(pTaskFunction != NULL)
	{
		ODS("Calling Task Delegate");
		return (pTaskFunction)(nCode, taskInfo);
	}
	else
	{
		ODS("Attempted to call the Task Delegate, however the pointer is null");
		return FALSE;
	}

}

LPWSTR GetWindowName(HWND windowHandle)
{
	LPWSTR windowTitle = new wchar_t[128];
	GetWindowText(windowHandle, windowTitle, 128);

	return windowTitle;
}

HICON GetWindowIcon(HWND windowHandle)
{
	DWORD_PTR hIco = 0;
	int HICON1 = -14;
	int HICONSM = -34;

	SendMessageTimeout(windowHandle, 0x007f, 2, 0, 2, 200, (PDWORD_PTR)hIco);

	if (hIco == 0)
	{
		SendMessageTimeout(windowHandle, 0x007f, 0, 0, 2, 200, (PDWORD_PTR)hIco);
	}

	if (hIco == 0)
	{
		hIco = GetClassLongPtr(windowHandle, HICONSM);
	}

	if (hIco == 0)
	{
		hIco = GetClassLongPtr(windowHandle, HICON1);
	}

	return (HICON)hIco;
	//DWORD_PTR dwReturn = GetClassLongPtr(windowHandle, (-14));
	//return (HICON)dwReturn;
}

void GetTaskinformationForWindowAction(TASKINFORMATION* taskInfo, int nCode, WPARAM wParam, LPARAM lParam)
{
		taskInfo->WindowName = GetWindowName((HWND)wParam);
		taskInfo->WindowHandle = (HWND)wParam;
		taskInfo->SystemMenu = GetSystemMenu(taskInfo->WindowHandle, TRUE);
		taskInfo->WindowIcon = GetWindowIcon((HWND)wParam);
		if(nCode == HSHELL_WINDOWREPLACED)
		{
			taskInfo->NewWindowHandle = (HWND)lParam;
		}
}

LRESULT CALLBACK ShellProc(int nCode, WPARAM wParam, LPARAM lParam)
{
	TASKINFORMATION *taskInfo = new TASKINFORMATION;

	switch(nCode)
	{
	case HSHELL_WINDOWACTIVATED:
		GetTaskinformationForWindowAction(taskInfo, nCode, wParam, lParam);
		taskInfo->WindowAction = Activated;
		break;

	case HSHELL_WINDOWCREATED:
		GetTaskinformationForWindowAction(taskInfo, nCode, wParam, lParam);
		taskInfo->WindowAction = Created;
		break;

	case HSHELL_WINDOWDESTROYED:
		GetTaskinformationForWindowAction(taskInfo, nCode, wParam, lParam);
		taskInfo->WindowAction = Destroyed;
		break;

	case HSHELL_WINDOWREPLACED:
		GetTaskinformationForWindowAction(taskInfo, nCode, wParam, lParam);
		taskInfo->WindowAction = Replaced;
		break;
	}

	if(CallTaskDelegate(nCode, taskInfo))
	{
		delete taskInfo;
		return 0;
	}
	else
	{
		delete taskInfo;
		return CallNextHookEx(m_ShellHook, nCode, wParam, lParam);
	}
}

LRESULT CALLBACK WndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	ODS("WndProc called.\n");
	switch(msg)
	{
	case WM_COPYDATA:
		{
			COPYDATASTRUCT *copyData = (COPYDATASTRUCT *)lParam;
			if(copyData == NULL)
			{
				ODS("CopyData is null. Returning.");
				return DefWindowProc(hWnd, msg, wParam, lParam);
			}

			switch(copyData->dwData)
			{
			case 0:
				// This is supposed to just 'pass it along' to the default handler, but it doesn't
				// Currently i'm just setting up the app bar before we initialize this, but we need to research this
				// further and find a final solution.. (broc)
				return FALSE;
				break;
			case 1:
				int offset = 8;

				if (sizeof(void*) == 8)
				{
					offset = -4;
				}

				NOTIFYICONDATA *nicData = (NOTIFYICONDATA *)(((BYTE *)copyData->lpData) + offset);
				int TrayCmd = *(INT *) (((BYTE *)copyData->lpData) + 4);

				BOOL result = CallSystrayDelegate(TrayCmd, *nicData);
				if(result) OutputDebugString(L"Result is true.");
				else OutputDebugString(L"Result is false");
				return 0;
			}
		}
	}
	return DefWindowProc(hWnd, msg, wParam, lParam);
}
