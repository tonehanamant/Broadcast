# Beware : The references in this script are very relative.
# Move anything and it may break...

param
(    
    [switch] $SkipVersionIncrement,
    [switch] $SkipProjectBuild
)

$targetProjectName = 'Services.Broadcast'
$targetProjectDirectory = '..\..\Tam\Maestro\Services\Broadcast\Services.Broadcast'

Write-Host "Beginning nuget package creation for '$targetProjectName'." -ForegroundColor Green

Write-Host "Resolving paths..." -ForegroundColor Green 

$targetProjectFilePath = [System.Io.Path]::Combine($targetProjectDirectory, $("{0}.csproj" -f $targetProjectName))
$targetProjectNuspecFilePath = [System.Io.Path]::Combine($targetProjectDirectory, $("{0}.nuspec" -f $targetProjectName))

$fullTargetProjectFilePath = Resolve-Path -Path "$targetProjectFilePath"
$fullTargetProjectNuspecFilePath = Resolve-Path -Path "$targetProjectNuspecFilePath"

if ($SkipVersionIncrement.IsPresent -eq $false)
{
    Write-Host "Detecting version information..." -ForegroundColor Green
    
    $nugetSpecVersion = $null
    $toWrite = $null
    $newNuspeclines = [System.Collections.ArrayList]@()

    Get-Content $fullTargetProjectNuspecFilePath | ForEach-Object {        
        $toWrite = $_
        if ($toWrite -like "*<version>*</version>")
        {
            $raw = $($($toWrite  -replace ' ', '') -replace '<version>', '') -replace '</version>', ''
            $nugetSpecVersion = $raw.Split('.');

            $newPatchNumber = [int]$nugetSpecVersion[2] + 1        
            $newPackageVersion = "{0}.{1}.{2}" -f $nugetSpecVersion[0], $nugetSpecVersion[1], $newPatchNumber
            Write-Host "Changing the package version to '$newPackageVersion'" -ForegroundColor Green
            
            $newPackageVersionString = "    <version>{0}</version>" -f $newPackageVersion
            $toWrite  = $newPackageVersionString
        }    
        $newNuspeclines += $toWrite
    }    

    Set-Content $fullTargetProjectNuspecFilePath $newNuspeclines
}
else
{
    Write-Host "Skipping package version incrementation per switch." -ForegroundColor Yellow
}

Write-Host "Building our package..." -ForegroundColor Green
if ($SkipProjectBuild.IsPresent -eq $false)
{
    .\nuget.exe pack $fullTargetProjectFilePath -IncludeReferencedProjects -Build -properties Configuration=Release
}
else 
{
    Write-Host "Skipping project build per switch." -ForegroundColor Yellow
    .\nuget.exe pack $fullTargetProjectFilePath -IncludeReferencedProjects -properties Configuration=Release
}

invoke-item .

Write-Host "Package built!!!" -ForegroundColor Green

if ($SkipVersionIncrement.IsPresent -eq $true)
{
    Write-Host "Skipped package version incrementation per switch." -ForegroundColor Yellow
}

if ($SkipProjectBuild.IsPresent -eq $true)
{
    Write-Host "Skipped project build per switch." -ForegroundColor Yellow
}

Write-Host "All done" -ForegroundColor Green