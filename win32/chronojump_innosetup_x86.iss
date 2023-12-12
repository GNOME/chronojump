; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define ProductName "Chronojump"
#define ProductVersion "1.0"
#define Arch "x86"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{69EC15A7-5D66-4D24-A15B-1C23C0F621E1}
AppName={#ProductName}
AppVerName={#ProductName} {#ProductVersion}  
AppPublisher=Chronojump
AppPublisherURL=http://www.chronojump.org/
AppSupportURL=http://www.chronojump.org/
AppUpdatesURL=http://www.chronojump.org/
DefaultDirName={commonpf}\{#ProductName}
DefaultGroupName={#ProductName}
LicenseFile=.\gpl-2.0.txt
OutputDir=.
OutputBaseFilename={#ProductName}-win-{#Arch}-{#ProductVersion}
SetupIconFile=.\chronojump_icon_install.ico
UninstallDisplayIcon={app}\share\chronojump\images\chronojump_icon.ico
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "catalan"; MessagesFile: "compiler:Languages\Catalan.isl"
Name: "czech"; MessagesFile: "compiler:Languages\Czech.isl"
Name: "danish"; MessagesFile: "compiler:Languages\Danish.isl"
Name: "dutch"; MessagesFile: "compiler:Languages\Dutch.isl"
Name: "finnish"; MessagesFile: "compiler:Languages\Finnish.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "hebrew"; MessagesFile: "compiler:Languages\Hebrew.isl"
Name: "hungarian"; MessagesFile: "compiler:Languages\Hungarian.isl"
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"
Name: "norwegian"; MessagesFile: "compiler:Languages\Norwegian.isl"
Name: "polish"; MessagesFile: "compiler:Languages\Polish.isl"
Name: "portuguese"; MessagesFile: "compiler:Languages\Portuguese.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "slovenian"; MessagesFile: "compiler:Languages\Slovenian.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]                                                                                                       
Name: "install_dotnet7"; Description: "Install .NET Runtime 7.0.14"; GroupDescription: "Install .NET Runtime 7.0.14";
Name: "install_python3"; Description: "Install Python 3.12.0"; GroupDescription: "Install Python 3.12.0";    
Name: "install_R4"; Description: "Install R 4.3.2"; GroupDescription: "Install R 4.3.2";
Name: "install_driver"; Description: "Install Chronopic driver"; GroupDescription: "Install Chronopic driver";
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}";
; Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "../butterworth/Data/*"; DestDir: "{app}\butterworth\Data\"; Flags: recursesubdirs createallsubdirs         
Source: "../encoder/*"; DestDir: "{app}\encoder\"; Flags: recursesubdirs createallsubdirs      
Source: "./dist.chronojump-x86/*"; Excludes: ".gitignore"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs    
Source: "../butterworth/Sample/bin/Release/*"; Excludes: ".gitignore"; DestDir: "{app}\butterworth\"; Flags: recursesubdirs createallsubdirs
Source: "../glade/*"; DestDir: "{app}\glade\"; Flags: recursesubdirs createallsubdirs
Source: "../images/*"; DestDir: "{app}\images\"; Flags: recursesubdirs createallsubdirs   
Source: "../po/*"; Excludes: "*.in"; DestDir: "{app}\po\"; Flags: recursesubdirs createallsubdirs
Source: "./chronojump_icon.ico"; DestDir: "{app}\share\chronojump\images\"
Source: "./logchronojump.bat"; DestDir: "{app}"
; NOTE: Don't use "Flags: ignoreversion" on any shared system files
Source: "./deps/*"; Excludes: ".gitignore"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs
Source: "./xbuild_files/*"; DestDir: "{app}\xbuild_files\"; Flags: recursesubdirs createallsubdirs
Source: "../manual/*.pdf"; DestDir: "{app}\share\doc\chronojump\"    
Source: "./installers/dotnet-runtime-7.0.14-win-x86.exe"; DestDir: "{app}\installers\"
Source: "./installers/python-3.12.0.exe"; DestDir: "{app}\installers\"
Source: "./installers/R-4.3.2-win.exe"; DestDir: "{app}\installers\"     
Source: "./gtk3/*"; DestDir: "{app}\"; Flags: recursesubdirs createallsubdirs
Source: "../build/data/locale/*"; DestDir: "{app}\share\locale\"; Flags: recursesubdirs createallsubdirs

[Icons]                                                                        
Name: "{group}\Install Chronopic driver"; Filename: "{app}\drivers\CDM212364_Setup.exe"; WorkingDir: "{app}\drivers"
Name: "{group}\Chronojump"; Filename: "{app}\Chronojump.exe"; WorkingDir: "{app}"
Name: "{group}\Chronojump debug"; Filename: "{app}\logchronojump.bat"; WorkingDir: "{app}"     
Name: "{group}\{cm:UninstallProgram,Chronojump}"; Filename: "{uninstallexe}"
; Name: "{group}\Chronojump mini"; Filename: "{app}\rxvt.exe"; Parameters:"-e ./Chronojump_mini.exe" ; WorkingDir: "{app}" ; IconFileName: "{app}\share\chronojump\images\chronojump_icon.ico"
; Name: "{group}\Chronopic Firmware Recorder "; Filename: "{app}\bin\chronopic-firmwarecord-dir\chronopic-firmwarecord.exe"; WorkingDir: "{app}\bin\chronopic-firmwarecord-dir"
Name: "{commondesktop}\Chronojump"; Filename: "{app}\Chronojump.exe"; WorkingDir: "{app}"; Tasks: desktopicon
Name: "{commondesktop}\Chronojump debug"; Filename: "{app}\logchronojump.bat"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\installers\dotnet-runtime-7.0.14-win-x86.exe"; StatusMsg: "Install .NET Runtime 7.0.14"; Tasks: install_dotnet7; Flags: skipifsilent
Filename: "{app}\installers\python-3.12.0.exe"; StatusMsg: "Install Python 3.12.0"; Tasks: install_python3; Flags: skipifsilent
Filename: "{app}\installers\R-4.3.2-win.exe"; StatusMsg: "Install R 4.3.2"; Tasks: install_R4; Flags: skipifsilent
Filename: "{app}\drivers\CDM212364_Setup.exe"; StatusMsg: "Installing Chronopic driver"; Tasks: install_driver; Flags: skipifsilent
Filename: "{app}\Chronojump.exe"; Description: "{cm:LaunchProgram,ChronoJump}"; Flags: nowait postinstall skipifsilent
; Filename: "{app}\bin\logchronojump.bat"; Description: "{cm:LaunchProgram,ChronoJump}"; Flags: nowait postinstall skipifsilent

