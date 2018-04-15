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
