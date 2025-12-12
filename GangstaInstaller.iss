[Setup]
AppName=Gangsta
AppVersion=4.1
AppPublisher=patricus productions
AppCopyright=Copyright (C) 2025 patricus productions, under MIT
DefaultDirName={autopf}\Gangsta
DefaultGroupName=Gangsta Simulator
AllowNoIcons=yes
LicenseFile=license
OutputDir=.
OutputBaseFilename=GangstaSetup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Copy EVERYTHING from release folder into {app}
Source: "bin\Release\net10.0-windows\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Dirs]
Name: "{app}\Mods"
Name: "{app}\savegames"

[Icons]
Name: "{autoprograms}\Gangsta Simulator"; Filename: "{app}\gangsta.exe"
Name: "{autodesktop}\Gangsta Simulator"; Filename: "{app}\gangsta.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\gangsta.exe"; Description: "{cm:LaunchProgram,Gangsta Simulator}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}\Mods"
Type: filesandordirs; Name: "{app}\savegames"
