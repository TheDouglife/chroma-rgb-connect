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

[Files]
Source: "{#PublishDir}\**"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{group}\{#Product}"; Filename: "{app}\ChromaControl.App.exe"; WorkingDir: "{app}"
Name: "{commonstartup}\{#Product}"; Filename: "{app}\ChromaRGBConnect.Service.exe"; WorkingDir: "{app}"

[Run]
Filename: "{app}\ChromaRGBConnect.Service.exe"; WorkingDir: "{app}"; StatusMsg: "Starting Chroma RGB Connect service..."; Flags: runhidden nowait
Filename: "{app}\ChromaControl.App.exe"; WorkingDir: "{app}"; Description: "Start Chroma RGB Connect"; Flags: postinstall nowait skipifsilent

[UninstallRun]
Filename: "{sys}\taskkill.exe"; Parameters: "/F /IM ChromaControl.App.exe"; StatusMsg: "Stopping Chroma RGB Connect..."; Flags: runhidden
Filename: "{sys}\taskkill.exe"; Parameters: "/F /IM ChromaRGBConnect.Service.exe"; StatusMsg: "Stopping Chroma RGB Connect service..."; Flags: runhidden

[Code]
const
  VCRedistUrl = 'https://aka.ms/vs/17/release/vc_redist.x64.exe';
  WebView2BootstrapperUrl = 'https://go.microsoft.com/fwlink/p/?LinkId=2124703';

function InstallPrerequisite(const Url, FileName, Parameters, Description: String): Boolean;
var
  InstallerPath: String;
  DownloadSize: Int64;
  ResultCode: Integer;
begin
  Result := True;
  InstallerPath := ExpandConstant('{tmp}\' + FileName);
  DownloadSize := DownloadTemporaryFile(Url, FileName, '', nil);

  if DownloadSize < 0 then
  begin
    MsgBox('Unable to download ' + Description + '. Check your internet connection and try again.', mbError, MB_OK);
    Result := False;
    exit;
  end;

  if not Exec(InstallerPath, Parameters, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    MsgBox('Unable to start the ' + Description + ' installer.', mbError, MB_OK);
    Result := False;
    exit;
  end;

  { 1638 means a compatible or newer VC++ runtime is already installed.
    3010 means installation succeeded and a reboot is recommended. }
  if (ResultCode <> 0) and (ResultCode <> 1638) and (ResultCode <> 3010) then
  begin
    MsgBox('Unable to install ' + Description + '. Installer exit code: ' + IntToStr(ResultCode) + '.', mbError, MB_OK);
    Result := False;
  end;
end;

function InstallPrerequisites: Boolean;
begin
  Result := InstallPrerequisite(
    VCRedistUrl,
    'vc_redist.x64.exe',
    '/install /quiet /norestart',
    'the Microsoft Visual C++ runtime');

  if Result then
  begin
    Result := InstallPrerequisite(
      WebView2BootstrapperUrl,
      'MicrosoftEdgeWebview2Setup.exe',
      '/silent /install',
      'Microsoft Edge WebView2 Runtime');
  end;
end;

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
  StopProcess('ChromaRGBConnect.Service.exe');
  StopProcess('ChromaControl.OpenRGB.exe');
  StopService('WinRing0x64');
  StopService('WinRing0_1_2_0');

  Result := InstallPrerequisites;
end;
