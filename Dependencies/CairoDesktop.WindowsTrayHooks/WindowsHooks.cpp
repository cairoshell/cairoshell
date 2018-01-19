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
	
	// Member variables&
	WNDCLASS	   m_TrayClass;
 	HWND		   m_hWndTray;
 	WNDCLASS	   m_NotifyClass;
 	HWND		   m_hWndNotify;
	HINSTANCE	   m_hInstance;

	// Forward declaration for WndProc
	extern "C" 
	{
		LRESULT CALLBACK WndProc(HWND, UINT, WPARAM, LPARAM);
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

HWND InitializeSystray(int width, float scale)
{
	ShutdownSystray();

	memset(&m_TrayClass, 0, sizeof(m_TrayClass));
	
	m_TrayClass.lpszClassName = L"Shell_TrayWnd";
	m_TrayClass.lpfnWndProc = WndProc;
	m_TrayClass.style = CS_DBLCLKS;
	m_TrayClass.hInstance = m_hInstance;

	RegisterClass( &m_TrayClass );

	m_hWndTray = CreateWindowEx(	WS_EX_TOPMOST | WS_EX_TOOLWINDOW,
								L"Shell_TrayWnd",
								NULL,
								WS_POPUP | WS_CLIPCHILDREN | WS_CLIPSIBLINGS,
								0, 0,
								width*scale, 23*scale,
								NULL,
								NULL,
								m_hInstance,
								NULL);

	ODS("Shell_TrayWnd Created\n");
	
	memset(&m_NotifyClass,0, sizeof(m_NotifyClass));
     m_NotifyClass.lpszClassName = L"TrayNotifyWnd";
     m_NotifyClass.lpfnWndProc = WndProc;
     //m_NotifyClass.cbSize = sizeof(WNDCLASSEX);
     m_NotifyClass.hInstance = m_hInstance;
     m_NotifyClass.style = CS_DBLCLKS;
     
     RegisterClass(&m_NotifyClass);
        
	 m_hWndNotify = CreateWindowEx( 0,
              L"TrayNotifyWnd", 
              NULL,
              WS_CHILD | WS_CLIPCHILDREN | WS_CLIPSIBLINGS,
              0, 0, width*scale, 23*scale,
              m_hWndTray, NULL,
              m_hInstance,
              NULL);

	 ODS("TrayNotifyWnd Created\n");

	 return m_hWndTray;
}


void Run()
{
	SendNotifyMessage(HWND_BROADCAST, RegisterWindowMessage(L"TaskbarCreated"), 0, 0);
	ODS("Sent TaskbarCreated message.\n");
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
				NOTIFYICONDATA *nicData = (NOTIFYICONDATA *)(((BYTE *)copyData->lpData) + 8);
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
