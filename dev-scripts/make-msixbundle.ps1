$ProjectFile = "ZedCommandPalette/ZedCommandPalette.csproj"

$xml = [xml](Get-Content $ProjectFile)
$Version = $xml.Project.PropertyGroup.AppxPackageVersion | Select-Object -First 1

dotnet build ZedCommandPalette/ZedCommandPalette.csproj --configuration Release -p:GenerateAppxPackageOnBuild=true -p:Platform=ARM64 -p:SelfContained=true -p:RuntimeIdentifier=win-arm64 -p:AppxPackageDir="../AppPackages/ARM64/"
dotnet build ZedCommandPalette/ZedCommandPalette.csproj --configuration Release -p:GenerateAppxPackageOnBuild=true -p:Platform=x64 -p:SelfContained=true -p:RuntimeIdentifier=win-x64 -p:AppxPackageDir="../AppPackages/x64/"

$x64Msix = Get-ChildItem -Path AppPackages/x64 -Recurse -Filter *.msix | Select-Object -First 1
$arm64Msix = Get-ChildItem -Path AppPackages/ARM64 -Recurse -Filter *.msix | Select-Object -First 1
if (-not $x64Msix -or -not $arm64Msix) { throw "Could not find MSIX files" }

$mappingFile = [System.IO.Path]::GetTempFileName()
@"
[Files]
"$($x64Msix.FullName)" "ZedCommandPalette_${Version}_x64.msix"
"$($arm64Msix.FullName)" "ZedCommandPalette_${Version}_arm64.msix"
"@ | Set-Content -Path $mappingFile -Encoding ascii

$BundlePath = "ZedCommandPalette_${Version}_Bundle.msixbundle"
makeappx bundle /v /f $mappingFile /p $BundlePath
