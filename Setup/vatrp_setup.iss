; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

[Setup]
#define vatrpVersion GetEnv('vatrpVersion') 
#if vatrpVersion == "" 
	#define vatrpVersion "0.0.0.0" 
#endif
#define vatrpAppName "ACE"
AppName={#vatrpAppName}
AppVerName={#vatrpAppName} {#vatrpVersion}
AppPublisher=
DefaultDirName={pf}\{#vatrpAppName}
DefaultGroupName={#vatrpAppName}
Compression=zip/9
DisableStartupPrompt=true
OutputBaseFilename=ACE_{#vatrpVersion}_x86
; uncomment the following line if you want your installation to run on NT 3.51 too.
; MinVersion=4,3.51
OutputDir=Out
AppVersion={#vatrpVersion}
UninstallDisplayIcon={app}\{#vatrpAppName}.exe,1
UninstallDisplayName={#vatrpAppName} 
LicenseFile=license.txt
VersionInfoVersion={#vatrpVersion}
VersionInfoDescription={#vatrpAppName}
VersionInfoTextVersion={#vatrpVersion}
VersionInfoCopyright=Copyright (C) 2015
VersionInfoProductName={#vatrpAppName}
VersionInfoProductVersion={#vatrpVersion}
AppendDefaultGroupName=false
UsePreviousAppDir=false
DirExistsWarning=no
UsePreviousGroup=false

[Tasks]
Name: desktopicon; Description: Create a &desktop icon; GroupDescription: Additional icons:; MinVersion: 4,4

[Files]

Source: ..\VATRP.App\bin\Release\ACE.exe; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\ACE.Core.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\ACE.LinphoneWrapper.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\avcodec-53.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\avutil-51.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\HockeyApp.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\HockeyAppPCL.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\intl.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\libantlr3c.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\libbellesip-0.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\libbzrtp-0.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\libgcc_s_dw2-1.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\liblinphone-7.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\libmediastreamer_base-5.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\libmediastreamer_voip-5.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\libogg-0.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\libopus-0.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\libortp-9.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\libpolarssl-0.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\libspeex-1.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\libspeexdsp-1.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\libsqlite3-0.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\libstdc++-6.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\libtheora-0.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\libxml2-2.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\log4net.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\log4net.config; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\Microsoft.Threading.Tasks.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\Microsoft.Threading.Tasks.Extensions.Desktop.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\Microsoft.Threading.Tasks.Extensions.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\pthreadGC2.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\SQLite.Interop.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\swscale-2.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\System.Data.SQLite.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\System.IO.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\System.Runtime.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\System.Threading.Tasks.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\zlib1.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\VATRP.App\bin\Release\openh264-1.4.0-win32msvc.dll; DestDir: {app}; Flags: ignoreversion

[Icons]
Name: {group}\ACE; Filename: {app}\ACE.exe; IconIndex: 0
Name: {group}\Uninstall ACE; Filename: {app}\unins000.exe; WorkingDir: {app}
Name: {userdesktop}\ACE; Filename: {app}\ACE.exe; Tasks: desktopicon; IconIndex: 0

[Run]
Filename: {app}\ACE.exe; Description: Launch ACE; Flags: nowait postinstall skipifsilent
