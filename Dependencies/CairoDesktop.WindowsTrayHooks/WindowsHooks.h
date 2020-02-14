// The type of window action received by WH_SHELL
enum WindowActions
{
	Activated,
	Created,
	Destroyed,
	Replaced
};

// Holds the data relevent to the application/window which the shell hook has been received for.
typedef struct _TASKINFORMATION 
{
	wchar_t* WindowName;
	HWND WindowHandle;
	HWND NewWindowHandle;
	HICON WindowIcon;
	WindowActions WindowAction; // Enum
	HMENU SystemMenu;

} TASKINFORMATION, *PTASKINFORMATION;

// Shell_NotifyIconGetRect sends this data struct
typedef struct _WINNOTIFYICONIDENTIFIER
{
	DWORD dwMagic;
	DWORD dwMessage;
	DWORD cbSize;
	DWORD dwPadding;
	HWND hWnd;
	UINT uID;
	GUID guidItem;
} WINNOTIFYICONIDENTIFIER, *CAIROWINNOTIFYICONIDENTIFIER;

typedef struct _APPBARDATAV2
{
	UINT cbSize;
	UINT hWnd;
	UINT uCallbackMessage;
	UINT uEdge;
	RECT rc;
	int lParam;
} APPBARDATAV2, *PAPPBARDATAV2;

typedef struct _SHELLTRAYDATA
{
	DWORD dwUnknown;
	DWORD dwMessage;
	NOTIFYICONDATA nid;
} SHELLTRAYDATA, *PSHELLTRAYDATA;

// Data sent with AppBar Message
typedef struct _SHELLAPPBARDATA {
	APPBARDATAV2 abd;
	DWORD lparamExtensionTo64bit; // The cbSize of APPBARDATA is incremented by 4
	DWORD dwMessage;
	DWORD unused1; // random (often 0), probably a leak from a calling process
	DWORD dataAtom;
	DWORD unknown1; // always zero during my tests
	DWORD destinationProcessId;
	DWORD unused2;  // random (often 0), probably a leak from the calling process
} SHELLAPPBARDATA, *PSHELLAPPBARDATA;

// Type definitions for the callback signatures
typedef BOOL (__stdcall *CALLBACK_NOTIFYICON_FUNCTION)(UINT, NOTIFYICONDATA);
typedef BOOL (__stdcall *CALLBACK_NOTIFYICONID_FUNCTION)(CAIROWINNOTIFYICONIDENTIFIER);
typedef BOOL (__stdcall *CALLBACK_TASK_FUNCTION)(INT, TASKINFORMATION*);

// Forward declaration of methods.
void SetSystrayCallback(LPVOID);
void SetIconDataCallback(LPVOID);
void SetTaskCallback(LPVOID);
HWND InitializeSystray(int, float);
void Run();
void ShutdownSystray();
BOOL CallSystrayDelegate(int, NOTIFYICONDATA);
LRESULT CallIconDataDelegate(CAIROWINNOTIFYICONIDENTIFIER);
LPWSTR GetWindowName(HWND);
HICON GetWindowIcon(HWND);

// Shortcut for debug text
#define ODS(data) OutputDebugString(TEXT(data))
