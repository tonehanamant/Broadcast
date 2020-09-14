# Beware : The references in this script are very relative.
# Move anything and it may break...

param
(    
    [switch] $SkipIncrementVersion
)

$repositoryPath = '..\..\Solutions\BroadcastComposerWeb\packages'
$targetProjectFile = '..\..\Tam\Maestro\Services\Broadcast\Services.Broadcast\Services.Broadcast.csproj'

Write-Host "Beginning nuget package creation for 'Services.Broadcast'."

$targetProjectPropertiesFile = '..\..\Tam\Maestro\Services\Broadcast\Services.Broadcast\Properties\AssemblyInfo.cs'
$targetProjectNuspecFile = '..\..\Tam\Maestro\Services\Broadcast\Services.Broadcast\Services.Broadcast.nuspec'

Write-Host "Resolving paths..."

$fullRepositoryPath = Resolve-Path -Path "$repositoryPath"
$fullTargetProjectFilePath = Resolve-Path -Path "$targetProjectFile"

if ($SkipIncrementVersion.IsPresent -eq $false)
{
    Write-Host "Detecting version information..."

    $assemblyVersionSplits=$null
    $nugetSpecVersion=$null

    Get-Content $targetProjectPropertiesFile | ForEach-Object {    
        $splits = ([string]$_).Split('"');
        if ($splits[0] -eq "[assembly: AssemblyVersion(")
        {     
            $assemblyVersionSplits = $($splits[1]).Split('.')
        }    
    }

    Get-Content $targetProjectNuspecFile | ForEach-Object {        
        if ($_ -like "*<version>*</version>")
        {
            $raw = $($($_ -replace ' ', '') -replace '<version>', '') -replace '</version>', ''
            $nugetSpecVersion = $raw.Split('.');        
        }    
    }

    $sectorOneAndTwoMatch = $($assemblyVersionSplits[0] -eq $nugetSpecVersion[0]) -and $($assemblyVersionSplits[1] -eq $nugetSpecVersion[1])
    if ($sectorOneAndTwoMatch -ne $true)
    {
        $newPackageVersion = "{0}.{1}.{2}" -f $assemblyVersionSplits[0], $assemblyVersionSplits[1], $assemblyVersionSplits[2]
    }
    else
    {    
        $newPatchNumber = [int]$nugetSpecVersion[2] + 1        
        $newPackageVersion = "{0}.{1}.{2}" -f $nugetSpecVersion[0], $nugetSpecVersion[1], $newPatchNumber
    }

    Write-Host "Changing the package version to '$newPackageVersion'"
    $newPackageVersionString="    <version>{0}</version>" -f $newPackageVersion

    $newNuspeclines = [System.Collections.ArrayList]@()
    Get-Content $targetProjectNuspecFile | ForEach-Object {
        $toWrite = $_
        if ($toWrite -like "*<version>*")
        {
            $toWrite = $newPackageVersionString
        }
        $newNuspeclines += $toWrite
    }

    Set-Content $targetProjectNuspecFile $newNuspeclines
}

Write-Host "Redirecting package source repo to '$fullRepositoryPath'"
.\nuget.exe config -Set repositoryPath="$fullRepositoryPath"

Write-Host "Building our package..."
.\nuget.exe pack $targetProjectFile -IncludeReferencedProjects -Build -properties Configuration=Release

Write-Host "All done"
