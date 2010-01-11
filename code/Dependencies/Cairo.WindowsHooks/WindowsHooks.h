// The type of window action received by WH_SHELL
typedef enum WindowActions
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

// Type definitions for the callback signatures
typedef BOOL (__stdcall *CALLBACK_NOTIFYICON_FUNCTION)(UINT, NOTIFYICONDATA);
typedef BOOL (__stdcall *CALLBACK_TASK_FUNCTION)(INT, TASKINFORMATION*);

// Forward declaration of methods.
void SetSystrayCallback(LPVOID);
void SetTaskCallback(LPVOID);
void InitializeSystray();
void InitializeTask();
void Run();
void ShutdownAll();
void ShutdownSystray();
void ShutdownTask();
BOOL CallSystrayDelegate(int, NOTIFYICONDATA);
BOOL CallTaskDelegate(int, TASKINFORMATION);
LPWSTR GetWindowName(HWND);
HICON GetWindowIcon(HWND);
void GetTaskinformationForWindowAction(TASKINFORMATION *, int, WPARAM, LPARAM);

// Shortcut for debug text
#define ODS(data) OutputDebugString(TEXT(data))
