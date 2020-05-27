## Сборка проекта без сборки corefx для Windows

Для сборки проекта необходимо выполнить следующие действия:

1. Установить КриптоПро CSP 5.0. Убедиться что введена действующая лицензия.

2. Установить [core 3.1 sdk и runtime](https://dotnet.microsoft.com/download/dotnet-core/3.1). 
Рекомендуются версии runtime 3.1.3 и 3.1.4 и sdk 3.1.300.

Установить [Распространяемый пакет Visual C++ для Visual Studio 2015](https://www.microsoft.com/ru-ru/download/details.aspx?id=48145).

3. Задать переменную среды DOTNET_MULTILEVEL_LOOKUP=0.

4. Скачать архив [package_windows_debug.zip](https://github.com/CryptoProLLC/corefx/releases) и разархивировать его в папку packages по некоторому пути `packages_PATH`.

5. Скачать архив [runtime-debug-windows.zip](https://github.com/CryptoProLLC/corefx/releases) и и разархивировать его в папку runtime по некоторому пути `runtime_PATH`.

6. Изменить файл `%appdata%\NuGet\NuGet.Config`, добавив в начало узла `packageSources` источник `<add key="local coreclr" value="packages_PATH" />`.

Пример:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="local coreclr" value="C:\packages" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
```

7. Скопировать сборки NetStandard из репозитория [NetStandard.Library](https://github.com/CryptoProLLC/NetStandard.Library/tree/master/nugetReady/netstandard.library) в локальную директорию nuget (`$env:userprofile\.nuget\packages\`) с заменой.

Пример powershell скрипта, выполняющего данную операцию
```powershell
git clone https://github.com/CryptoProLLC/NetStandard.Library
New-Item -ItemType Directory -Force -Path "$env:userprofile\.nuget\packages\netstandard.library"
Copy-Item -Force -Recurse ".\NetStandard.Library\nugetReady\netstandard.library" -Destination "$env:userprofile\.nuget\packages\"
```

8. Выкачать локально репозиторий текущего проекта.
```powershell
git clone https://github.com/CryptoProLLC/DotnetCoreSampleProject
```

9. Изменить файл `DotnetSampleProject.csproj`, указав правильные пути до сборок `System.Security.Cryptography.Pkcs.dll` и `System.Security.Cryptography.Xml.dll`, указав в качестве пути
`runtime_PATH`.

Пример:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp3.1</TargetFramework>
    <RuntimeFrameworkVersion>3.1.3</RuntimeFrameworkVersion>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <!-- make self-contained -->
    <PackageConflictPreferredPackages>Microsoft.Private.CoreFx.NETCoreApp;runtime.win-x64.Microsoft.Private.CoreFx.NETCoreApp;runtime.linux-x64.Microsoft.Private.CoreFx.NETCoreApp;$(PackageConflictPreferredPackages)</PackageConflictPreferredPackages>
  </PropertyGroup>
    <ItemGroup>
        <PackageReference Include="Microsoft.Private.CoreFx.NETCoreApp" Version="4.7.0-dev.20163.1" />
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

10. Перейти в папку проекта. Восстановить зависимости и собрать проект. Запустить полученный проект.
```powershell
cd DotnetCoreSampleProject
dotnet restore
dotnet build
dotnet run
```

В случае возникновения ошибки или предупреждения о несовпадения найденой и указанной весии `Microsoft.Private.CoreFx.NETCoreApp` изменить версию в файле `DotnetSampleProject.csproj`.

## Сборка проекта со сборкой corefx для Windows

1. Выполнить шаги 1-3 из "Сборка проекта без сборки corefx".

2. Выкачать репозиторий [corefx](https://github.com/CryptoProLLC/corefx/).

3. Выполнить сборку corefx, перейдя в локальную папку репозитория и выполнив `build.cmd`.

4. Выполнить шаги 6-10 из "Сборка проекта без сборки corefx", использовав в качестве пути 
`packages_PATH` путь вида `corefx_PATH\artifacts\packages\Debug\NonShipping`, в качестве пути `runtime_PATH` путь вида `corefx_PATH\artifacts\bin\runtime\netcoreapp-Windows_NT-Debug-x64`, где `corefx_PATH` путь до локальной папки репозитория corefx.

В случае внесения изменений в репозиторий corefx, перед его сборкой необходимо очистить папки 
`%userprofile%\.nuget\packages\microsoft.private.corefx.netcoreapp`, `%userprofile%\.nuget\packages\runtime.win-x64.microsoft.private.corefx.netcoreapp`, `corefx_PATH\artifacts\packages`
после чего завершить все процессы dotnet core. 

Пример скрипта выполняющего данное действие 
```powershell
taskkill /im dotnet.exe /f
rmdir /S /Q %userprofile%\.nuget\packages\microsoft.private.corefx.netcoreapp
rmdir /S /Q %userprofile%\.nuget\packages\runtime.win-x64.microsoft.private.corefx.netcoreapp
rmdir /S /Q artifacts\packages
build
```

Дополнительную информацию можно получить тут:

- https://github.com/dotnet/corefx/blob/master/Documentation/project-docs/dogfooding.md

- https://github.com/dotnet/corefx/blob/master/Documentation/project-docs/developer-guide.md

- https://github.com/dotnet/corefx/blob/master/Documentation/building/windows-instructions.md
