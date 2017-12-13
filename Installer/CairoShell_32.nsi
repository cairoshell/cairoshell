;--------------------------------
; CairoShell_32.nsi

!include "MUI.nsh"

; The name of the installer
Name "Cairo Desktop Environment"

; The file to write
OutFile "CairoSetup_32bit.exe"

; The default installation directory
InstallDir "$PROGRAMFILES\Cairo Shell"

; Registry key to check for directory (so if you install again, it will 
; overwrite the old one automatically)
InstallDirRegKey HKLM "Software\CairoShell" "Install_Dir"
!define MUI_ABORTWARNING

;--------------------------------
; Pages

  !define MUI_ICON inst_icon.ico
  !define MUI_HEADERIMAGE
  !define MUI_HEADERIMAGE_RIGHT
  !define MUI_HEADERIMAGE_BITMAP header_img.bmp
  !define MUI_UNICON inst_icon.ico
  !define MUI_COMPONENTSPAGE_SMALLDESC
  !define MUI_WELCOMEFINISHPAGE_BITMAP left_img.bmp
  !define MUI_WELCOMEPAGE_TEXT "This installer will guide you through the installation of Cairo.\r\n\r\nBefore installing, please ensure .NET Framework 4.5.2 or higher is installed, and that any running instance of Cairo is ended.\r\n\r\nClick Next to continue."
  !define MUI_FINISHPAGE_TITLE "Cairo is now installed"
  !define MUI_FINISHPAGE_TEXT "To start Cairo, open it from your Start menu."
  !insertmacro MUI_PAGE_WELCOME
  !insertmacro MUI_PAGE_LICENSE "License.txt"
  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_INSTFILES
  !insertmacro MUI_PAGE_FINISH
  
  !insertmacro MUI_UNPAGE_WELCOME
  !define MUI_UNCONFIRMPAGE_TEXT_TOP "Please be sure that you have closed Cairo before uninstalling to ensure that all files are removed."
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  !insertmacro MUI_UNPAGE_FINISH

;--------------------------------
; Components

!insertmacro MUI_LANGUAGE "English"

; The stuff to install
Section "Cairo Desktop (required)" cairo

  SectionIn RO

  ; Get .NET directory so we can ngen, error if not found
  Push "v4.0"
  Call GetDotNetDir
  Pop $R0
  StrCmpS "" $R0 noDotNetFound
  
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR

  System::Call 'kernel32::OpenMutex(i 0x100000, b 0, t "CairoShell") i .R1'
  IntCmp $R1 0 notRunning
    System::Call 'kernel32::CloseHandle(i $R1)'
    MessageBox MB_OK|MB_ICONEXCLAMATION "Cairo is currently running. Please exit Cairo from the Cairo menu and run this installer again." /SD IDOK
    Quit
  
  notRunning:
  
  ; Put file there
  File "..\Cairo Desktop\Build\Release\CairoDesktop.exe"
  File "..\Cairo Desktop\Build\Release\CairoDesktop.WindowsTray.dll"
  File "..\Cairo Desktop\Build\Release\CairoDesktop.WindowsTrayHooks.dll"
  File "..\Cairo Desktop\Build\Release\CairoDesktop.Common.dll"
  File "..\Cairo Desktop\Build\Release\CairoDesktop.AppGrabber.dll"
  File "..\Cairo Desktop\Build\Release\CairoDesktop.Configuration.dll"
  File "..\Cairo Desktop\Build\Release\CairoDesktop.Interop.dll"
  File "..\Cairo Desktop\Build\Release\CairoDesktop.Localization.dll"
  File "..\Cairo Desktop\Build\Release\CairoDesktop.UWPInterop.dll"
  File "..\Cairo Desktop\Build\Release\Interop.IWshRuntimeLibrary.dll"
  File "..\Cairo Desktop\Build\Release\SearchAPILib.dll"
  File "..\Cairo Desktop\Build\Release\CairoDesktop.WindowsTasks.dll"
  File "..\Cairo Desktop\Build\Release\White.xaml"
  File "..\Cairo Desktop\Build\Release\WinSparkle.dll"

  ; Set shell context to All Users
  SetShellVarContext all

  ; Start menu shortcuts
  createShortCut "$SMPROGRAMS\Cairo Desktop.lnk" "$INSTDIR\CairoDesktop.exe"
  
  ; Write the installation path into the registry
  WriteRegStr HKLM SOFTWARE\CairoShell "Install_Dir" "$INSTDIR"
  
  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\CairoShell" "DisplayName" "Cairo Desktop Environment"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\CairoShell" "DisplayIcon" '"$INSTDIR\RemoveCairo.exe"'
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\CairoShell" "DisplayVersion" "0.3"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\CairoShell" "UninstallString" '"$INSTDIR\RemoveCairo.exe"'
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\CairoShell" "URLInfoAbout" "http://cairodesktop.com"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\CairoShell" "Publisher" "Cairo Development Team"
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\CairoShell" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\CairoShell" "NoRepair" 1
  WriteUninstaller "RemoveCairo.exe"
  
  ; ngen to improve first start performance
  ;SetDetailsPrint both
  ;DetailPrint "Generating native image..."
  ;SetDetailsPrint none
  ;ExecWait '"$R0\ngen.exe" install "$INSTDIR\CairoDesktop.exe"'
  ;SetDetailsPrint both

  Return
 
  noDotNetFound:
    MessageBox MB_OK|MB_ICONSTOP "Cairo requires Microsoft .NET Framework 4.5.2 or higher. Please install this from the Microsoft web site and install Cairo again." /SD IDOK
    Quit

SectionEnd

; Run at startup
Section "Run at startup (current user)" startupCU
  
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "CairoShell" "$INSTDIR\CairoDesktop.exe"
  
SectionEnd

; Replace explorer
Section /o "Advanced users only: Replace Explorer (current user)" shellCU
  
  WriteRegStr HKCU "Software\Microsoft\Windows NT\CurrentVersion\Winlogon" "Shell" "$INSTDIR\CairoDesktop.exe"
  DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "CairoShell"
  
SectionEnd

  ;Language strings
  LangString DESC_cairo ${LANG_ENGLISH} "Installs Cairo and its required components."
  LangString DESC_startupCU ${LANG_ENGLISH} "Makes Cairo start up when you log in."
  LangString DESC_shellCU ${LANG_ENGLISH} "Run Cairo instead of Windows Explorer. Note this also disables many new features in Windows."

  ;Assign language strings to sections
  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${cairo} $(DESC_cairo)
    !insertmacro MUI_DESCRIPTION_TEXT ${startupCU} $(DESC_startupCU)
    !insertmacro MUI_DESCRIPTION_TEXT ${shellCU} $(DESC_shellCU)
  !insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------
; Uninstaller


Section "Uninstall"

  ; Set shell context to All Users
  SetShellVarContext all

  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\CairoShell"
  DeleteRegKey HKLM SOFTWARE\CairoShell
  DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "CairoShell"
  DeleteRegValue HKCU "Software\Microsoft\Windows NT\CurrentVersion\Winlogon" "Shell"

  ; Remove files and uninstaller. Includes historical files
  Delete "$INSTDIR\CairoDesktop.exe"
  Delete "$INSTDIR\CairoDesktop.exe.config"
  Delete "$INSTDIR\CairoDesktop.Configuration.dll.config"
  Delete "$INSTDIR\Cairo.WindowsHooks.dll"
  Delete "$INSTDIR\Cairo.WindowsHooksWrapper.dll"
  Delete "$INSTDIR\CairoDesktop.WindowsTray.dll"
  Delete "$INSTDIR\CairoDesktop.WindowsTrayHooks.dll"
  Delete "$INSTDIR\CairoDesktop.Common.dll"
  Delete "$INSTDIR\CairoDesktop.AppGrabber.dll"
  Delete "$INSTDIR\CairoDesktop.Configuration.dll"
  Delete "$INSTDIR\CairoDesktop.Interop.dll"
  Delete "$INSTDIR\CairoDesktop.Localization.dll"
  Delete "$INSTDIR\CairoDesktop.UWPInterop.dll"
  Delete "$INSTDIR\Interop.IWshRuntimeLibrary.dll"
  Delete "$INSTDIR\Interop.Shell32.dll"
  Delete "$INSTDIR\WPFToolkit.dll"
  Delete "$INSTDIR\SearchAPILib.dll"
  Delete "$SMPROGRAMS\Cairo Desktop.lnk"
  Delete "$INSTDIR\UnhandledExceptionFilter.dll"
  Delete "$INSTDIR\CairoDesktop.WindowsTasks.dll"
  Delete "$INSTDIR\PostSharp.Core.dll"
  Delete "$INSTDIR\PostSharp.Public.dll"
  Delete "$INSTDIR\PostSharp.Laos.dll"
  Delete "$INSTDIR\PostSharp.Core.XmlSerializers.dll"
  Delete "$INSTDIR\RemoveCairo.exe"
  Delete "$INSTDIR\White.xaml"
  Delete "$INSTDIR\WinSparkle.dll"

  ; Remove directories used
  RMDir "$INSTDIR"

SectionEnd

;--------------------------------
; Functions

; Given a .NET version number, this function returns that .NET framework's
; install directory. Returns "" if the given .NET version is not installed.
; Params: [version] (eg. "v2.0")
; Return: [dir] (eg. "C:\WINNT\Microsoft.NET\Framework\v2.0.50727")
Function GetDotNetDir
  Exch $R0 ; Set R0 to .net version major
  Push $R1
  Push $R2

  ; set R1 to minor version number of the installed .NET runtime
  EnumRegValue $R1 HKLM \
    "Software\Microsoft\.NetFramework\policy\$R0" 0
  IfErrors getdotnetdir_err
 
  ; set R2 to .NET install dir root
  ReadRegStr $R2 HKLM \
    "Software\Microsoft\.NetFramework" "InstallRoot"
  IfErrors getdotnetdir_err
 
  ; set R0 to the .NET install dir full
  StrCpy $R0 "$R2$R0.$R1"
 
  getdotnetdir_end:
    Pop $R2
    Pop $R1
    Exch $R0 ; return .net install dir full
    Return
 
  getdotnetdir_err:
    StrCpy $R0 ""
    Goto getdotnetdir_end
 
FunctionEnd