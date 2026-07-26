[Setup]
AppId=ChromaConnect
AppName={#Product}
AppVersion={#Version}
AppPublisher={#Authors}
WizardStyle=modern
DefaultDirName={autopf}\{#Product}
DefaultGroupName={#Product}
UninstallDisplayName={#Product}
UninstallDisplayIcon={app}\ChromaConnect.App.exe
Compression=lzma2
SolidCompression=yes
OutputDir={#ObjDir}
OutputBaseFileName=ChromaRGBConnectSetup-{#Version}
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
CloseApplications=force
SetupIconFile={#PublishDir}\wwwroot\favicon.ico
LicenseFile={#LicenseFile}
MissingRunOnceIdsWarning=no

[Files]
Source: "{#PublishDir}\**"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion; Excludes: "VC_redist.x64.exe,VC_redist.x86.exe"
Source: "{#PublishDir}\VC_redist.x64.exe"; DestName: "vc_redist.x64.exe"; Flags: dontcopy
Source: "{#PublishDir}\VC_redist.x86.exe"; DestName: "vc_redist.x86.exe"; Flags: dontcopy

[Icons]
Name: "{group}\{#Product}"; Filename: "{app}\ChromaConnect.App.exe"; WorkingDir: "{app}"
Name: "{commonstartup}\{#Product}"; Filename: "{app}\ChromaRGBConnect.Service.exe"; WorkingDir: "{app}"

[Run]
Filename: "{app}\ChromaRGBConnect.Service.exe"; WorkingDir: "{app}"; StatusMsg: "Starting Chroma RGB Connect service..."; Flags: runhidden nowait
Filename: "{app}\ChromaConnect.App.exe"; WorkingDir: "{app}"; Description: "Start Chroma RGB Connect"; Flags: postinstall nowait skipifsilent

[UninstallRun]
Filename: "{sys}\taskkill.exe"; Parameters: "/F /IM ChromaConnect.App.exe"; StatusMsg: "Stopping Chroma RGB Connect..."; Flags: runhidden
Filename: "{sys}\taskkill.exe"; Parameters: "/F /IM ChromaRGBConnect.Service.exe"; StatusMsg: "Stopping Chroma RGB Connect service..."; Flags: runhidden

[Code]
const
  WebView2BootstrapperUrl = 'https://go.microsoft.com/fwlink/p/?LinkId=2124703';
  VCRedistRegistryKeyX64 = 'SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64';
  VCRedistRegistryKeyX86 = 'SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x86';

function IsVCRedistInstalled(const RegistryKey: String): Boolean;
var
  Installed: Cardinal;
begin
  Result := RegQueryDWordValue(HKLM, RegistryKey, 'Installed', Installed) and (Installed = 1);
end;

function InstallBundledPrerequisite(const FileName, Parameters, Description: String): Boolean;
var
  InstallerPath: String;
  ResultCode: Integer;
begin
  Result := True;

  try
    ExtractTemporaryFile(FileName);
  except
    MsgBox('Unable to extract ' + Description + ' from the installer package.', mbError, MB_OK);
    Result := False;
    exit;
  end;

  InstallerPath := ExpandConstant('{tmp}\' + FileName);

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

function InstallDownloadedPrerequisite(const Url, FileName, Parameters, Description: String): Boolean;
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

  if (ResultCode <> 0) and (ResultCode <> 1638) and (ResultCode <> 3010) then
  begin
    MsgBox('Unable to install ' + Description + '. Installer exit code: ' + IntToStr(ResultCode) + '.', mbError, MB_OK);
    Result := False;
  end;
end;

function InstallPrerequisites: Boolean;
begin
  Result := True;

  if not IsVCRedistInstalled(VCRedistRegistryKeyX64) then
  begin
    Result := InstallBundledPrerequisite(
      'vc_redist.x64.exe',
      '/install /quiet /norestart',
      'the Microsoft Visual C++ x64 runtime');
  end;

  if Result and (not IsVCRedistInstalled(VCRedistRegistryKeyX86)) then
  begin
    Result := InstallBundledPrerequisite(
      'vc_redist.x86.exe',
      '/install /quiet /norestart',
      'the Microsoft Visual C++ x86 runtime');
  end;

  if Result then
  begin
    Result := InstallDownloadedPrerequisite(
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
  StopProcess('ChromaConnect.App.exe');
  StopProcess('ChromaRGBConnect.Service.exe');
  StopProcess('ChromaConnect.OpenRGB.exe');
  StopService('WinRing0x64');
  StopService('WinRing0_1_2_0');

  Result := InstallPrerequisites;
end;
