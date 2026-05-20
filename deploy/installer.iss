; Inno Setup script for Lamour Desktop App
; Requirements: Inno Setup 6.x — https://jrsoftware.org/isdl.php
;
; HOW TO USE:
;   1. Run publish-wpf.bat first to generate .\publish\wpf\
;   2. Open this file in Inno Setup Compiler
;   3. Click Build > Compile (or Ctrl+F9)
;   4. Installer .exe will be created in .\deploy\output\

#define AppName "Lamour"
#define AppVersion "1.0.0"
#define AppPublisher "Lamour"
#define AppExeName "DesktopLamour.exe"
#define SourceDir "..\publish\wpf"
#define OutputDir ".\output"

[Setup]
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
OutputDir={#OutputDir}
OutputBaseFilename=LamourSetup-{#AppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "vietnamese"; MessagesFile: "compiler:Languages\Vietnamese.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Tạo shortcut trên Desktop"; GroupDescription: "Shortcut:"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{group}\Gỡ cài đặt {#AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Khởi động {#AppName}"; Flags: nowait postinstall skipifsilent
