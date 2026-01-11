; Harmony Community Manager Installer Script
; NSIS (Nullsoft Scriptable Install System) installer

;--------------------------------
; Includes

!include "MUI2.nsh"
!include "FileFunc.nsh"
!include "LogicLib.nsh"

;--------------------------------
; General

; Version define - can be passed via /DVERSION="x.y.z" command line argument
!ifndef VERSION
    !define VERSION "1.0.0"
!endif

; Name and file
Name "Harmony Community Manager"
OutFile "Harmony-Setup-${VERSION}.exe"
Icon "..\artwork\HarmonyIcon.ico"
Unicode True

; Default installation folder
InstallDir "$PROGRAMFILES64\Harmony"

; Get installation folder from registry if available
InstallDirRegKey HKCU "Software\Harmony" ""

; Request application privileges for Windows Vista/7/8/10/11
RequestExecutionLevel admin

; Version information
; VERSION format: "x.y.z" -> convert to "x.y.z.0" for VIProductVersion (4-part version required)
; Simple approach: append ".0" to the version string
!define VI_PRODUCT_VERSION "${VERSION}.0"

VIProductVersion "${VI_PRODUCT_VERSION}"
VIAddVersionKey "ProductName" "Harmony Community Manager"
VIAddVersionKey "ProductVersion" "${VERSION}"
VIAddVersionKey "FileVersion" "${VI_PRODUCT_VERSION}"
VIAddVersionKey "CompanyName" "Harmony"
VIAddVersionKey "FileDescription" "Harmony Community Manager Installer"
VIAddVersionKey "LegalCopyright" "Copyright 2025 Mark van de Veerdonk - Licensed under Apache License 2.0"

;--------------------------------
; Interface Settings

!define MUI_ABORTWARNING
!define MUI_ICON "..\artwork\HarmonyIcon.ico"
!define MUI_UNICON "..\artwork\HarmonyIcon.ico"

; Finish page settings
!define MUI_FINISHPAGE_RUN "$INSTDIR\Harmony.Web.exe"
!define MUI_FINISHPAGE_RUN_TEXT "Launch Harmony Community Manager"

;--------------------------------
; Pages

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "LICENSE.txt"
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

;--------------------------------
; Languages

!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_LANGUAGE "Dutch"

;--------------------------------
; Installer Sections

Section "Harmony Application" SecMain
    SectionIn RO
    
    SetOutPath "$INSTDIR"
    
    ; Install all files from the publish directory
    ; Publish output should be in: src/Harmony.Web/bin/Release/net9.0/win-x64/publish/
    ; Note: This path is relative to where makensis is executed (installer directory)
    ; NSIS will fail at compile time if these files don't exist, so ensure you run
    ; the build script or publish the application before building the installer
    File /r "..\src\Harmony.Web\bin\Release\net9.0\win-x64\publish\*.*"
    
    ; Store installation folder
    WriteRegStr HKCU "Software\Harmony" "" $INSTDIR
    
    ; Create uninstaller
    WriteUninstaller "$INSTDIR\Uninstall.exe"
    
    ; Add to Add/Remove Programs
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Harmony" \
        "DisplayName" "Harmony Community Manager"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Harmony" \
        "UninstallString" "$\"$INSTDIR\Uninstall.exe$\""
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Harmony" \
        "DisplayIcon" "$\"$INSTDIR\Harmony.Web.exe$\""
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Harmony" \
        "Publisher" "Harmony"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Harmony" \
        "DisplayVersion" "${VERSION}"
    WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Harmony" \
        "NoModify" 1
    WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Harmony" \
        "NoRepair" 1
    
    ; Create database directory if it doesn't exist
    CreateDirectory "$APPDATA\Harmony"
    
SectionEnd

Section "Desktop Shortcut" SecDesktop
    CreateShortcut "$DESKTOP\Harmony Community Manager.lnk" "$INSTDIR\Harmony.Web.exe" \
        "" "$INSTDIR\Harmony.Web.exe" 0 SW_SHOWNORMAL \
        "" "Harmony Community Manager"
SectionEnd

Section "Start Menu Shortcuts" SecStartMenu
    CreateDirectory "$SMPROGRAMS\Harmony"
    CreateShortcut "$SMPROGRAMS\Harmony\Harmony Community Manager.lnk" "$INSTDIR\Harmony.Web.exe" \
        "" "$INSTDIR\Harmony.Web.exe" 0 SW_SHOWNORMAL \
        "" "Harmony Community Manager"
    CreateShortcut "$SMPROGRAMS\Harmony\Uninstall.lnk" "$INSTDIR\Uninstall.exe" \
        "" "$INSTDIR\Uninstall.exe" 0
SectionEnd

;--------------------------------
; Descriptions

; Language strings
LangString DESC_SecMain ${LANG_ENGLISH} "Install Harmony Community Manager application files."
LangString DESC_SecMain ${LANG_DUTCH} "Installeer Harmony Community Manager applicatiebestanden."

LangString DESC_SecDesktop ${LANG_ENGLISH} "Create a desktop shortcut for Harmony Community Manager."
LangString DESC_SecDesktop ${LANG_DUTCH} "Maak een bureaublad snelkoppeling voor Harmony Community Manager."

LangString DESC_SecStartMenu ${LANG_ENGLISH} "Create Start Menu shortcuts for Harmony Community Manager."
LangString DESC_SecStartMenu ${LANG_DUTCH} "Maak Start Menu snelkoppelingen voor Harmony Community Manager."

; Assign language strings to sections
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${SecMain} $(DESC_SecMain)
    !insertmacro MUI_DESCRIPTION_TEXT ${SecDesktop} $(DESC_SecDesktop)
    !insertmacro MUI_DESCRIPTION_TEXT ${SecStartMenu} $(DESC_SecStartMenu)
!insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------
; Uninstaller Section

Section "Uninstall"
    ; Remove files and uninstaller
    Delete "$INSTDIR\Uninstall.exe"
    RMDir /r "$INSTDIR"
    
    ; Remove shortcuts
    Delete "$DESKTOP\Harmony Community Manager.lnk"
    RMDir /r "$SMPROGRAMS\Harmony"
    
    ; Remove registry keys
    DeleteRegKey HKCU "Software\Harmony"
    DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Harmony"
    
    ; Note: We don't remove the database directory ($APPDATA\Harmony) 
    ; to preserve user data during uninstallation
    
SectionEnd

