# DotnetCoreSampleProject
������ ������� Dotnet Core � ����������� ���� ������������.

## ������ ������� ��� ������ corefx ��� Windows

��� ������ ������� ���������� ��������� ��������� ��������:

1. ���������� ��������� CSP 5.0. ��������� ��� ������� ����������� ��������.

2. ���������� ��������� ������ [core 3.1 sdk � runtime](https://dotnet.microsoft.com/download) .

3. ������ ���������� ����� DOTNET_MULTILEVEL_LOOKUP=0.

4. ������� ����� [packages](https://ci.appveyor.com/project/CryptoProLLC/corefx/build/artifacts) � ��������������� ��� � ����� packages �� ���������� ���� `packages_PATH`.

5. ������� ����� [runtime](https://ci.appveyor.com/project/CryptoProLLC/corefx/build/artifacts) � � ��������������� ��� � ����� runtime �� ���������� ���� `runtime_PATH`.

6. �������� ���� %appdata%\NuGet\NuGet.Config, ������� � ������ ���� `packageSources` �������� `<add key="local coreclr" value="packages_PATH\Debug\NonShipping" />`.

������:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="local coreclr" value="C:\packages\Debug\NonShipping" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
```

7. ����������� ������ NetStandard � ��������� ���������� nuget (`$env:userprofile\.nuget\packages\`) � �������.

������ powershell �������, ������������ ������ ��������
```powershell
git clone https://github.com/CryptoProLLC/NetStandard.Library
New-Item -ItemType Directory -Force -Path "$env:userprofile\.nuget\packages\netstandard.library"
Copy-Item -Force -Recurse ".\NetStandard.Library\nugetReady\netstandard.library" -Destination "$env:userprofile\.nuget\packages\"
```

8. �������� �������� ����������� �������� �������.
```powershell
git clone https://github.com/CryptoProLLC/DotnetCoreSampleProject
```

9. �������� ���� DotnetSampleProject.csproj, ������ ���������� ���� �� ������ `System.Security.Cryptography.Pkcs.dll` � `System.Security.Cryptography.Xml.dll`, ������ � �������� ����
`packages_PATH`.

������:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp3.1</TargetFramework>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <!-- make self-contained -->
    <PackageConflictPreferredPackages>Microsoft.Private.CoreFx.NETCoreApp;runtime.win-x64.Microsoft.Private.CoreFx.NETCoreApp;$(PackageConflictPreferredPackages)</PackageConflictPreferredPackages>
  </PropertyGroup>
    <ItemGroup>
        <PackageReference Include="Microsoft.Private.CoreFx.NETCoreApp" Version="4.7.0-dev.20065.1" />
    </ItemGroup>
    <ItemGroup>
      <Reference Include="System.Security.Cryptography.Pkcs">
        <HintPath>C:\runtime\System.Security.Cryptography.Pkcs.dll</HintPath>
      </Reference>
      <Reference Include="System.Security.Cryptography.Xml">
        <HintPath>C:\runtime\System.Security.Cryptography.Xml.dll</HintPath>
      </Reference>
    </ItemGroup>
</Project>
```

10. ������� � ����� �������. ������������ ����������� � ������� ������. ��������� ���������� ������.
```powershell
cd DotnetCoreSampleProject
dotnet restore
dotnet build
dotnet run
```

## ������ ������� �� ������� corefx ��� Windows

1. ��������� ���� 1-3 �� "������ ������� ��� ������ corefx".

2. �������� ����������� [corefx](https://github.com/CryptoProLLC/corefx/).

3. ��������� ������ corefx, ������� � ��������� ����� ����������� � �������� `build.cmd`.

4. ��������� ��� 6 �� "������ ������� ��� ������ corefx", ����������� � �������� ���� 
`packages_PATH` ���� ���� `corefx_PATH\artifacts\packages`, ��� `corefx_PATH` ���� �� ��������� ����� ����������� corefx.

5. ��������� ���� 7-10 �� "������ ������� ��� ������ corefx", ����������� � �������� ���� 
`packages_PATH` ���� ���� `corefx_PATH\artifacts\packages`, ��� `corefx_PATH` ���� �� ��������� ����� ����������� corefx.

� ������ �������� ��������� � ����������� corefx, ����� ��� ������� ���������� �������� ����� 
`%userprofile%\.nuget\packages\microsoft.private.corefx.netcoreapp`, `%userprofile%\.nuget\packages\runtime.win-x64.microsoft.private.corefx.netcoreapp`, `corefx_PATH\artifacts\packages`
����� ���� ��������� ��� �������� dotnet core. 

������ ������� ������������ ������ �������� 
```powershell
taskkill /im dotnet.exe /f
rmdir /S /Q %userprofile%\.nuget\packages\microsoft.private.corefx.netcoreapp
rmdir /S /Q %userprofile%\.nuget\packages\runtime.win-x64.microsoft.private.corefx.netcoreapp
rmdir /S /Q artifacts\packages
build
```


�������������� ���������� ����� �������� ���:

- https://github.com/dotnet/corefx/blob/master/Documentation/project-docs/dogfooding.md

- https://github.com/dotnet/corefx/blob/master/Documentation/project-docs/developer-guide.md

- https://github.com/dotnet/corefx/blob/master/Documentation/building/windows-instructions.md