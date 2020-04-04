// Shell_NotifyIconGetRect sends this data struct
typedef struct _WINNOTIFYICONIDENTIFIER
{
	DWORD dwMagic;
	DWORD dwMessage;
	DWORD cbSize;
	DWORD dwPadding;
	UINT hWnd;
	UINT uID;
	GUID guidItem;
} WINNOTIFYICONIDENTIFIER, *PWINNOTIFYICONIDENTIFIER;

// AppBar message structs come to us with 32bit values on 64bit systems, so some of these data types are different than typical
typedef struct _APPBARDATAV2
{
    DWORD cbSize;
	UINT hWnd;
    UINT uCallbackMessage;
    UINT uEdge;
    RECT rc;
    LONG lParam;
    DWORD dw64BitAlign;
} APPBARDATAV2, *PAPPBARDATAV2;

typedef struct _APPBARMSGDATAV3
{
    APPBARDATAV2 abd;
    DWORD  dwMessage;
    DWORD  dwPadding1;
	UINT hSharedMemory;
    DWORD  dwPadding2;
    DWORD  dwSourceProcessId;
    DWORD  dwPadding3;
} APPBARMSGDATAV3, *PAPPBARMSGDATAV3;

typedef struct _SHELLTRAYDATA
{
	DWORD dwUnknown;
	DWORD dwMessage;
	NOTIFYICONDATA nid;
} SHELLTRAYDATA, *PSHELLTRAYDATA;

typedef struct _MENUBARSIZEDATA
{
	RECT rc;
	int edge;
} MENUBARSIZEDATA, *PMENUBARSIZEDATA;

// Type definitions for the callback signatures
typedef BOOL(__stdcall* CALLBACK_NOTIFYICON_FUNCTION)(UINT, NOTIFYICONDATA);
typedef BOOL(__stdcall* CALLBACK_NOTIFYICONID_FUNCTION)(DWORD, UINT, UINT, GUID);
typedef MENUBARSIZEDATA(__stdcall* CALLBACK_MENUBARSIZE_FUNCTION)();

// Forward declaration of methods.
void __cdecl SetSystrayCallback(LPVOID);
void __cdecl SetIconDataCallback(LPVOID);
void __cdecl SetMenuBarSizeCallback(LPVOID);
HWND __cdecl InitializeSystray(int, float);
void __cdecl Run();
void __cdecl ShutdownSystray();
BOOL CallSystrayDelegate(int, NOTIFYICONDATA);
LRESULT CallIconDataDelegate(PWINNOTIFYICONIDENTIFIER);
LPWSTR GetWindowName(HWND);
HICON GetWindowIcon(HWND);

// Shortcut for debug text
#define ODS(data) OutputDebugString(TEXT(data))
