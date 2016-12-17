; CairoShell.nsi
;
; It's time to celebrate!  Cairo will go out to the masses =)

;--------------------------------
!include "MUI.nsh"

; The name of the installer
Name "Cairo Desktop Environment"

; The file to write
OutFile "CairoShellSetup_64bit.exe"

; The default installation directory
InstallDir "$PROGRAMFILES64\Cairo Shell"

; Registry key to check for directory (so if you install again, it will 
; overwrite the old one automatically)
InstallDirRegKey HKLM "Software\CairoShell" "Install_Dir"
!define MUI_ABORTWARNING

;--------------------------------

; Pages

; Page components
; Page directory
; Page instfiles

; UninstPage uninstConfirm
; UninstPage instfiles
  !define MUI_ICON inst_icon.ico
  !define MUI_HEADERIMAGE
  !define MUI_HEADERIMAGE_RIGHT
  !define MUI_HEADERIMAGE_BITMAP header_img.bmp
  !define MUI_UNICON inst_icon.ico
  !define MUI_WELCOMEFINISHPAGE_BITMAP left_img.bmp
  !define MUI_FINISHPAGE_RUN "$INSTDIR\CairoDesktop.exe"
  !define MUI_FINISHPAGE_RUN_TEXT "Start Cairo Desktop Environment"
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

!insertmacro MUI_LANGUAGE "English"

; The stuff to install
Section "Cairo Shell (required)" cairo

  SectionIn RO

  ; Get .NET directory so we can ngen, error if not found
  Push "v4.0"
  Call GetDotNetDir
  Pop $R0
  StrCmpS "" $R0 err_dot_net_not_found
  
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  
  ; Put file there
  File "..\Cairo Desktop\Build\x64\Release\CairoDesktop.exe"
  File "..\Cairo Desktop\Build\x64\Release\CairoDesktop.exe.config"
  File "..\Cairo Desktop\Build\x64\Release\Cairo.WindowsHooks.dll"
  File "..\Cairo Desktop\Build\x64\Release\Cairo.WindowsHooksWrapper.dll"
  File "..\Cairo Desktop\Build\x64\Release\CairoDesktop.AppGrabber.dll"
  File "..\Cairo Desktop\Build\x64\Release\CairoDesktop.Interop.dll"
  File "..\Cairo Desktop\Build\x64\Release\Interop.IWshRuntimeLibrary.dll"
  File "..\Cairo Desktop\Build\x64\Release\Interop.Shell32.dll"
  File "..\Cairo Desktop\Build\x64\Release\WPFToolkit.dll"
  File "..\Cairo Desktop\Build\x64\Release\SearchAPILib.dll"
  File "..\Cairo Desktop\Build\x64\Release\UnhandledExceptionFilter.dll"
  File "..\Cairo Desktop\Build\x64\Release\CairoDesktop.WindowsTasks.dll"
  File "..\Cairo Desktop\Build\x64\Release\PostSharp.Core.dll"
  File "..\Cairo Desktop\Build\x64\Release\PostSharp.Laos.dll"
  File "..\Cairo Desktop\Build\x64\Release\PostSharp.Public.dll"
  File "..\Cairo Desktop\Build\x64\Release\PostSharp.Core.XmlSerializers.dll"
  File "..\Cairo Desktop\Build\x64\Release\White.xaml"

  ;Reboot just in case, some people seem to have issues for some reason.
  ; SetRebootFlag true

  ; Start menu shortcuts
  createShortCut "$SMPROGRAMS\Cairo Desktop.lnk" "$INSTDIR\CairoDesktop.exe"
  
  ; Write the installation path into the registry
  WriteRegStr HKLM SOFTWARE\CairoShell "Install_Dir" "$INSTDIR"
  
  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\CairoShell" "DisplayName" "Cairo Desktop Environment"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\CairoShell" "DisplayIcon" '"$INSTDIR\RemoveCairo.exe"'
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\CairoShell" "DisplayVersion" "0.2"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\CairoShell" "UninstallString" '"$INSTDIR\RemoveCairo.exe"'
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\CairoShell" "URLInfoAbout" "http://cairoshell.scj.me"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\CairoShell" "Publisher" "Cairo Development Team"
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\CairoShell" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\CairoShell" "NoRepair" 1
  WriteUninstaller "RemoveCairo.exe"
  
  ; ngen to improve first start performance
  SetDetailsPrint both
  DetailPrint "Generating native image..."
  SetDetailsPrint none
  ExecWait '"$R0\ngen.exe" install "$INSTDIR\CairoDesktop.exe"'
  SetDetailsPrint both

  Return
 
  err_dot_net_not_found:
    Abort "Cairo requires .NET Framework 4.5.2 or higher to be installed. Please install .NET Framework 4.5.2 or greater from Microsoft and try again."

SectionEnd

; The wallpaper contest winner deserves to have their wallpaper put out in to the real world, don't you think?
; Section "Wallpaper" wall

 ; SetOutPath "C:\Windows\Web\Wallpaper\"
 ; File "CairoWallpaper1.jpg"
 ; File "CairoWallpaper2.jpg"
 ; WriteRegStr HKCU "Control Panel\Desktop" "WallPaper" "C:\Windows\Web\Wallpaper\CairoWallpaper2.jpg"
  
; SectionEnd

; Replace explorer.
Section /o "Set as shell (current user)" startupCU
  
  WriteRegStr HKCU "Software\Microsoft\Windows NT\CurrentVersion\Winlogon" "Shell" "$INSTDIR\CairoDesktop.exe"
  DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "CairoShell"
  
SectionEnd

  ;Language strings
  LangString DESC_cairo ${LANG_ENGLISH} "Installs Cairo and its required components."
  ; LangString DESC_wall ${LANG_ENGLISH} "Sets the Cairo wallpaper as your default desktop background."
  LangString DESC_startupCU ${LANG_ENGLISH} "Makes Cairo start up when you log in."

  ;Assign language strings to sections
  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${cairo} $(DESC_cairo)
   ; !insertmacro MUI_DESCRIPTION_TEXT ${wall} $(DESC_wall)
    !insertmacro MUI_DESCRIPTION_TEXT ${startupCU} $(DESC_startupCU)
  !insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------

; Uninstaller


Section "Uninstall"

  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\CairoShell"
  DeleteRegKey HKLM SOFTWARE\CairoShell
  DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "CairoShell"
  DeleteRegValue HKCU "Software\Microsoft\Windows NT\CurrentVersion\Winlogon" "Shell"

  ; Remove files and uninstaller
  Delete "$INSTDIR\CairoDesktop.exe"
  Delete "$INSTDIR\CairoDesktop.exe.config"
  Delete "$INSTDIR\Cairo.WindowsHooks.dll"
  Delete "$INSTDIR\Cairo.WindowsHooksWrapper.dll"
  Delete "$INSTDIR\CairoDesktop.AppGrabber.dll"
  Delete "$INSTDIR\CairoDesktop.Interop.dll"
  Delete "$INSTDIR\Interop.IWshRuntimeLibrary.dll"
  Delete "$INSTDIR\Interop.Shell32.dll"
  Delete "$INSTDIR\WPFToolkit.dll"
  Delete "$INSTDIR\SearchAPILib.dll"
  Delete "$SMPROGRAMS\Cairo Desktop.lnk"
  DELETE "$INSTDIR\UnhandledExceptionFilter.dll"
  Delete "$INSTDIR\CairoDesktop.WindowsTasks.dll"
  Delete "$INSTDIR\PostSharp.Core.dll"
  Delete "$INSTDIR\PostSharp.Public.dll"
  Delete "$INSTDIR\PostSharp.Laos.dll"
  Delete "$INSTDIR\PostSharp.Core.XmlSerializers.dll"
  Delete $INSTDIR\RemoveCairo.exe
  Delete "$INSTDIR\White.xaml"

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

  ; 64bit, lets do registry inserts to the right place
  SetRegView 64

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