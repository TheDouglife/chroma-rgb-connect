#include "CodeDependencies.iss"

[Setup]
AppId=ChromaControl
AppName={#Product}
AppVersion={#Version}
AppPublisher={#Authors}
WizardStyle=modern
DefaultDirName={autopf}\{#Product}
DefaultGroupName={#Product}
UninstallDisplayName={#Product}
UninstallDisplayIcon={app}\ChromaControl.App.exe
Compression=lzma2
SolidCompression=yes
OutputDir={#ObjDir}
OutputBaseFileName=ChromaControlSetup-{#Version}
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
CloseApplications=force
SetupIconFile={#PublishDir}\wwwroot\favicon.ico
LicenseFile={#LicenseFile}
MissingRunOnceIdsWarning=no
WizardImageFile=images\WizardImage0.bmp,images\WizardImage1.bmp
WizardSmallImageFile=images\WizardSmallImage0.bmp,images\WizardSmallImage1.bmp,images\WizardSmallImage2.bmp,images\WizardSmallImage3.bmp,images\WizardSmallImage4.bmp,images\WizardSmallImage5.bmp,images\WizardSmallImage6.bmp

[Files]
Source: "{#PublishDir}\**"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{group}\{#Product}"; Filename: "{app}\ChromaControl.App.exe"; WorkingDir: "{app}"
Name: "{commonstartup}\{#Product}"; Filename: "{app}\ChromaControl.Service.exe"; WorkingDir: "{app}"

[Run]
Filename: "{app}\ChromaControl.Service.exe"; WorkingDir: "{app}"; StatusMsg: "Starting Chroma Control Service..."; Flags: runhidden nowait
Filename: "{app}\ChromaControl.App.exe"; WorkingDir: "{app}"; Description: "Start Chroma Control"; Flags: postinstall nowait skipifsilent

[UninstallRun]
Filename: "{sys}\taskkill.exe"; Parameters: "/F /IM ChromaControl.App.exe"; StatusMsg: "Stopping Chroma Control..."; Flags: runhidden
Filename: "{sys}\taskkill.exe"; Parameters: "/F /IM ChromaControl.Service.exe"; StatusMsg: "Stopping Chroma Control Service..."; Flags: runhidden

[Code]
procedure StopProcess(const ImageName: String);
var
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{sys}\taskkill.exe'), '/F /IM ' + ImageName, '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
end;

procedure StopService(const ServiceName: String);
var
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{sys}\sc.exe'), 'stop ' + ServiceName, '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
end;

function InitializeSetup: Boolean;
begin
  StopProcess('ChromaControl.App.exe');
  StopProcess('ChromaControl.Service.exe');
  StopProcess('ChromaControl.OpenRGB.exe');
  StopService('WinRing0x64');
  StopService('WinRing0_1_2_0');

  Dependency_AddVC2015To2022;
  Dependency_AddWebView2;
  Result := True;
end;
