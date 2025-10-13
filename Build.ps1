# PowerShell Build Script for Project Overmind
# This script attempts to find RimWorld and build the mod

Write-Host "=== Project Overmind Build Script ===" -ForegroundColor Cyan
Write-Host ""

# Common RimWorld installation paths
$possiblePaths = @(
    "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Managed",
    "C:\Program Files\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Managed",
    "D:\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Managed",
    "E:\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Managed",
    "C:\GOG Games\RimWorld\RimWorldWin64_Data\Managed",
    "D:\GOG Games\RimWorld\RimWorldWin64_Data\Managed"
    "D:\Game\Rimworld\RimWorldWin64_Data\Managed"
)

# Find RimWorld installation
$rimworldPath = $null
foreach ($path in $possiblePaths) {
    if (Test-Path "$path\Assembly-CSharp.dll") {
        $rimworldPath = $path
        Write-Host "Found RimWorld at: $rimworldPath" -ForegroundColor Green
        break
    }
}

if ($null -eq $rimworldPath) {
    Write-Host "ERROR: Could not automatically find RimWorld installation!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please manually update the assembly paths in:" -ForegroundColor Yellow
    Write-Host "  Source\ProjectOvermind\ProjectOvermind.csproj" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Or enter your RimWorld Managed folder path:" -ForegroundColor Yellow
    $manualPath = Read-Host "Path"
    if (Test-Path "$manualPath\Assembly-CSharp.dll") {
        $rimworldPath = $manualPath
    } else {
        Write-Host "Invalid path. Exiting." -ForegroundColor Red
        exit 1
    }
}

# Update .csproj file with correct paths
Write-Host ""
Write-Host "Updating assembly references..." -ForegroundColor Cyan

$csprojPath = "Source\ProjectOvermind\ProjectOvermind.csproj"
$csprojContent = Get-Content $csprojPath -Raw

# Replace paths
$csprojContent = $csprojContent -replace '<HintPath>.*?Assembly-CSharp\.dll</HintPath>', "<HintPath>$rimworldPath\Assembly-CSharp.dll</HintPath>"
$csprojContent = $csprojContent -replace '<HintPath>.*?UnityEngine\.CoreModule\.dll</HintPath>', "<HintPath>$rimworldPath\UnityEngine.CoreModule.dll</HintPath>"
$csprojContent = $csprojContent -replace '<HintPath>.*?UnityEngine\.dll</HintPath>', "<HintPath>$rimworldPath\UnityEngine.dll</HintPath>"
$csprojContent = $csprojContent -replace '<HintPath>.*?UnityEngine\.IMGUIModule\.dll</HintPath>', "<HintPath>$rimworldPath\UnityEngine.IMGUIModule.dll</HintPath>"
$csprojContent = $csprojContent -replace '<HintPath>.*?UnityEngine\.TextRenderingModule\.dll</HintPath>', "<HintPath>$rimworldPath\UnityEngine.TextRenderingModule.dll</HintPath>"

Set-Content $csprojPath -Value $csprojContent

Write-Host "Assembly references updated successfully!" -ForegroundColor Green

# Build the project
Write-Host ""
Write-Host "Building mod..." -ForegroundColor Cyan
Push-Location "Source\ProjectOvermind"

try {
    dotnet build -c Release
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "=== BUILD SUCCESSFUL ===" -ForegroundColor Green
        Write-Host ""
        Write-Host "Compiled assembly: Assemblies\ProjectOvermind.dll" -ForegroundColor Green
        Write-Host ""
        Write-Host "The mod is ready to use!" -ForegroundColor Green
        Write-Host "Copy the entire Project_Overmind folder to your RimWorld Mods directory:" -ForegroundColor Cyan
        Write-Host "  Steam: ...\Steam\steamapps\common\RimWorld\Mods\" -ForegroundColor Yellow
        Write-Host "  GOG: ...\RimWorld\Mods\" -ForegroundColor Yellow
    } else {
        Write-Host ""
        Write-Host "=== BUILD FAILED ===" -ForegroundColor Red
        Write-Host "Please check the error messages above." -ForegroundColor Yellow
    }
} catch {
    Write-Host ""
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
} finally {
    Pop-Location
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
