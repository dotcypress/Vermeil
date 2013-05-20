$env:Path = $env:Path + ";C:\Windows\Microsoft.NET\Framework\v4.0.30319"

MSBuild.exe Vermeil.sln /p:Configuration=Release /p:Platform="Any CPU" /target:Rebuild

.\.nuget\NuGet.exe pack .\Vermeil\Vermeil.nuspec
.\.nuget\NuGet.exe pack .\Vermeil.Core\Vermeil.Core.nuspec