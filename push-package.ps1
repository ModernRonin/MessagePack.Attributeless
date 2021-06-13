param ($version)

if (-not $nugetApiKey) {$nugetApiKey= $Env:NUGETAPIKEY}
if (-not $nugetApiKey) 
{
	Write-Host "You need to either set the powershell variable nugetApiKey or the environment variable NUGETAPIKEY before executing this script" 
	exit
}

if (-not $version) 
{
	[xml]$xml= Get-Content .\MessagePack.Attributeless\release.history
	$version= $xml.Project.PropertyGroup.Version.ToString()
}
if (-not $version) 
{
	Write-Host "Something went wrong, could not read version from release.history"
	exit 
}

dotnet pack --configuration Release --include-symbols -p:ContinuousIntegrationBuild=true
dotnet nuget push .\MessagePack.Attributeless\nupkg\MessagePack.Attributeless.$version.nupkg -s https://api.nuget.org/v3/index.json --api-key $nugetApiKey