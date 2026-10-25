; ArchSkin Installer - Inno Setup Script
; Requires Inno Setup 6.x

#define MyAppName "ArchSkin"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "ArchSkin Contributors"
#define MyAppExeName "ArchSkin.Shell.exe"
#define MyServiceName "ArchSkin.Service.exe"
#define MySettingsExeName "ArchSkin.Settings.exe"

[Setup]
AppId={{ARCHSKIN-GUID-1234-5678-ABCD-EF0123456789}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=..\LICENSE
OutputDir=..\artifacts
OutputBaseFilename=ArchSkin-Setup-{#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
UninstallDisplayIcon={app}\{#MyAppName}.exe

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "..\src\ArchSkin.Shell\bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\src\ArchSkin.Shell\bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\src\ArchSkin.Settings\bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\{#MySettingsExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\src\ArchSkin.Settings\bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\src\ArchSkin.Service\bin\Release\net10.0-windows\win-x64\publish\{#MyServiceName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\src\ArchSkin.Service\bin\Release\net10.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\src\ArchSkin.ThemeSync\bin\Release\net10.0-windows\win-x64\publish\archskin.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ATTRIBUTION.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{#MyAppName} Settings"; Filename: "{app}\{#MySettingsExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\{#MyServiceName}"; Parameters: "install"; Description: "Enable autostart"; Flags: runhidden

[UninstallRun]
Filename: "{app}\{#MyServiceName}"; Parameters: "uninstall"; Flags: runhidden waituntilterminated

[Code]
function InitializeSetup(): Boolean;
begin
  Result := true;
end;
