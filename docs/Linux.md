## Сборка проекта без сборки corefx для Linux

Для сборки проекта необходимо выполнить следующие действия:

1. Установить КриптоПро CSP 5.0. Убедиться что введена действующая лицензия.

2. Установить последнюю версию [core 3.1 sdk и runtime](https://dotnet.microsoft.com/download).

3. Задать переменную среды DOTNET_MULTILEVEL_LOOKUP=0.

4. Скачать архив [packages](https://ci.appveyor.com/project/CryptoProLLC/corefx/build/artifacts) и разархивировать его в папку packages по некоторому пути `packages_PATH`.

5. Скачать архив [runtime](https://ci.appveyor.com/project/CryptoProLLC/corefx/build/artifacts) и и разархивировать его в папку runtime по некоторому пути `runtime_PATH`.

6. Изменить файл `~/.nuget/NuGet/NuGet.Config`, добавив в начало узла `packageSources` источник `<add key="local coreclr" value="packages_PATH/Debug/NonShipping" />`.

Пример:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="local coreclr" value="~/git/packages/Debug/NonShipping" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
```

7. Скопировать сборки NetStandard из репозитория [NetStandard.Library](https://github.com/CryptoProLLC/NetStandard.Library/tree/master/nugetReady/netstandard.library) в локальную директорию nuget (`~.nuget/packages/`) с заменой.

Пример sh скрипта, выполняющего данную операцию
```sh
git clone https://github.com/CryptoProLLC/NetStandard.Library
mkdir -p ~/.nuget/packages/netstandard.library
cp -r ./NetStandard.Library/nugetReady/netstandard.library ~/.nuget/packages/
```

8. Выкачать локально репозиторий текущего проекта.
```sh
git clone https://github.com/CryptoProLLC/DotnetCoreSampleProject
```

9. Изменить файл DotnetSampleProject.csproj, указав правильные пути до сборок `System.Security.Cryptography.Pkcs.dll` и `System.Security.Cryptography.Xml.dll`, указав в качестве пути
`runtime_PATH`. Изменить `RuntimeIdentifier` на `linux-x64`

Пример:
```xml
﻿<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp3.1</TargetFramework>
    <RuntimeIdentifier>linux-x64</RuntimeIdentifier>
    <!-- make self-contained -->
    <PackageConflictPreferredPackages>Microsoft.Private.CoreFx.NETCoreApp;runtime.win-x64.Microsoft.Private.CoreFx.NETCoreApp;runtime.linux-x64.Microsoft.Private.CoreFx.NETCoreApp;$(PackageConflictPreferredPackages)</PackageConflictPreferredPackages>
  </PropertyGroup>
    <ItemGroup>
        <PackageReference Include="Microsoft.Private.CoreFx.NETCoreApp" Version="4.7.0-dev.20111.1" />
    </ItemGroup>
    <ItemGroup>
      <Reference Include="System.Security.Cryptography.Pkcs">
        <HintPath>../runtime/netcoreapp-Linux-Debug-x64/System.Security.Cryptography.Pkcs.dll</HintPath>
      </Reference>
      <Reference Include="System.Security.Cryptography.Xml">
        <HintPath>../runtime/netcoreapp-Linux-Debug-x64/System.Security.Cryptography.Xml.dll</HintPath>
      </Reference>
    </ItemGroup>
</Project>
```

10. Перейти в папку проекта. Восстановить зависимости и собрать проект. Запустить полученный проект.
```sh
cd DotnetCoreSampleProject
dotnet restore
dotnet build
dotnet run
```