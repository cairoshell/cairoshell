// Windows System Hooks library
// Developed for Cairo Shell, December 2008
//
// Provides native hooks into the system to handle SysTray messages and Task bar hook notification.
// Allowing the hooks to be setup and callbacks sent via the .NET framework (Platform invocation)


#include "stdafx.h"
#include <ShellAPI.h>
#include <Winuser.h>
#include <tchar.h>
#include <stdio.h>
#include "WindowsHooks.h"

	// Callbacks for the delegates
	CALLBACK_NOTIFYICON_FUNCTION pSystrayFunction;
	CALLBACK_NOTIFYICONID_FUNCTION pIconDataFunction;
	
	// Member variables&
	WNDCLASS	   m_TrayClass;
 	HWND		   m_hWndTray;
 	WNDCLASS	   m_NotifyClass;
 	HWND		   m_hWndNotify;
	HINSTANCE	   m_hInstance;

	HWND			m_FwdHwnd;
	UINT			m_FwdMsg;
	WPARAM			m_FwdWParam;
	LPARAM			m_FwdLParam;
	LRESULT			m_FwdResult;

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

void SetIconDataCallback(LPVOID theCallbackFunctionAddress)
{
	pIconDataFunction = (CALLBACK_NOTIFYICONID_FUNCTION)theCallbackFunctionAddress;
	ODS("IconData callback set.\n");
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
								width, static_cast<int>(23*scale),
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
              0, 0, width, static_cast<int>(23 * scale),
              m_hWndTray, NULL,
              m_hInstance,
              NULL);

	 ODS("TrayNotifyWnd Created\n");

	 return m_hWndTray;
}


void Run()
{
	// move to top of z-order
	SetWindowPos(m_hWndTray, 0, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOACTIVATE | SWP_NOSIZE);

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
		ODS("Calling Systray Delegate\n");
		return (pSystrayFunction)(message, nicData);
	}
	else
	{
		ODS("Attempted to call the Systray Delegate, however the pointer is null\n");
		return FALSE;
	}
}

LRESULT CallIconDataDelegate(CAIROWINNOTIFYICONIDENTIFIER iconData)
{
	if (pIconDataFunction != NULL)
	{
		ODS("Calling IconData Delegate\n");
		return (pIconDataFunction)(iconData);
	}
	else
	{
		ODS("Attempted to call the IconData Delegate, however the pointer is null\n");
		return FALSE;
	}
}

BOOL CALLBACK fwdProc(HWND hWnd, LPARAM lParam)
{
	if (hWnd != m_hWndTray && hWnd != m_FwdHwnd)
	{
		TCHAR className[256];
		GetClassName(hWnd, className, 256);

		if (_tcscmp(className, _T("Shell_TrayWnd")) == 0)
		{
			m_FwdResult = SendMessage(hWnd, m_FwdMsg, m_FwdWParam, m_FwdLParam);
		}
	}
	
	return true;
}

LRESULT appBarMessageAction(PSHELLAPPBARDATA abmd)
{
	// only handle ABM_GETTASKBARPOS, send other AppBar messages to default handler
	switch (abmd->dwMessage)
	{
	case ABM_GETTASKBARPOS:
		APPBARDATAV2& abd = abmd->abd;
		abd.rc = { 0, 0, 1920, 23 };
		abd.uEdge = ABE_TOP;
		ODS("Responded to GetTaskbarPos\n");
		return 0;
		break;
	}

	return 1;
}

LRESULT CALLBACK WndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
		case WM_COPYDATA:
		{
			COPYDATASTRUCT *copyData = (COPYDATASTRUCT *)lParam;
			if (copyData == NULL)
			{
				ODS("CopyData is null. Returning.\n");
				return DefWindowProc(hWnd, msg, wParam, lParam);
			}

			switch (copyData->dwData)
			{
				case 0:
				{
					// AppBar message
					/*switch (copyData->cbData)
					{
						default :
						{
							PSHELLAPPBARDATA sbd = (PSHELLAPPBARDATA)copyData->lpData;
							
							if (appBarMessageAction(sbd) == 0)
								return 0;
						}
						break;

					}*/
				}
				break;
				case 1:
				{
					PSHELLTRAYDATA pstData = (PSHELLTRAYDATA)(copyData->lpData);
					NOTIFYICONDATA nicData = pstData->nid;
					int TrayCmd = pstData->dwMessage;
					
					BOOL result = CallSystrayDelegate(TrayCmd, nicData);
					if (!result) ODS("Ignored notify icon message\n");
					return result;
				}
				break;
				case 3:
				{
					CAIROWINNOTIFYICONIDENTIFIER iconData = (CAIROWINNOTIFYICONIDENTIFIER)copyData->lpData;

					return CallIconDataDelegate(iconData);
				}
				break;
				default:
				{
					// do nothing
				}
			}
		}
		break;
		case WM_WINDOWPOSCHANGED:
		{
			// remove WS_VISIBLE if gremlins added it.
			// Nvidia Surround seems to add this, because it does 'things' to the default Windows taskbar, which is normally a Shell_TrayWnd with WS_VISIBLE.
			WINDOWPOS* wpos;
			wpos = (WINDOWPOS*)lParam;
			if (wpos->flags & SWP_SHOWWINDOW) {
				LONG lStyle = GetWindowLong(m_hWndTray, GWL_STYLE);
				lStyle &= ~(WS_VISIBLE);
				SetWindowLong(m_hWndTray, GWL_STYLE, lStyle);
				ODS("Shell_TrayWnd became visible, hiding\n");
			}
		}
	}

	if (msg == WM_COPYDATA || msg == WM_ACTIVATEAPP)
	{
		ODS("Forwarding message to all Shell_TrayWnd\n");
		m_FwdLParam = lParam;
		m_FwdMsg = msg;
		m_FwdWParam = wParam;
		m_FwdHwnd = hWnd;

		EnumWindows(fwdProc, NULL);
		return m_FwdResult;
	}

	return DefWindowProc(hWnd, msg, wParam, lParam);
}
