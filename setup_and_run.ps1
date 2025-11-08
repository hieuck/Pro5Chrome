<#
.\setup_and_run.ps1
Create a virtual environment in the project folder (venv), install requirements
and run Pro5Chrome.py using the venv Python executable.

Usage (PowerShell):
# If ExecutionPolicy blocks running scripts for you, open an elevated PowerShell and run:
#   Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned
# Then run:
#   .\setup_and_run.ps1
# Or run once without changing policy:
#   powershell -ExecutionPolicy RemoteSigned -File .\setup_and_run.ps1
#
# The script uses the system Python (python on PATH) to create the venv, then
# uses venv\Scripts\python.exe to install requirements and run the app. This
# avoids needing to 'dot-source' Activate.ps1 inside this script and works
# when the script is run from varying environments.
#>

Set-StrictMode -Version Latest

$scriptDir = Split-Path -Parent $PSCommandPath
Push-Location $scriptDir

Write-Host "Working directory: $scriptDir"

# Find python on PATH
$pyCmd = Get-Command python -ErrorAction SilentlyContinue
if (-not $pyCmd) {
    $pyCmd = Get-Command python3 -ErrorAction SilentlyContinue
}

if (-not $pyCmd) {
    Write-Error "Python not found in PATH. Please install Python 3 and add it to PATH." -ErrorAction Stop
}

$venvPath = Join-Path $scriptDir 'venv'
$venvPython = Join-Path $venvPath 'Scripts\python.exe'

if (-not (Test-Path $venvPython)) {
    Write-Host "Creating virtual environment in: $venvPath"
    & $pyCmd.Source -ArgumentList '-m','venv',$venvPath
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to create virtual environment (exit code $LASTEXITCODE)." -ErrorAction Stop
    }
} else {
    Write-Host "Virtual environment already exists at: $venvPath"
}

if (-not (Test-Path $venvPython)) {
    Write-Error "venv python executable not found at $venvPython" -ErrorAction Stop
}

Write-Host "Using venv python: $venvPython"

Write-Host "Upgrading pip..."
& $venvPython -m pip install --upgrade pip

if (Test-Path (Join-Path $scriptDir 'requirements.txt')) {
    Write-Host "Installing packages from requirements.txt..."
    & $venvPython -m pip install -r (Join-Path $scriptDir 'requirements.txt')
} else {
    Write-Warning "requirements.txt not found. Skipping pip install -r requirements.txt"
}

if (Test-Path (Join-Path $scriptDir 'Pro5Chrome.py')) {
    Write-Host "Launching Pro5Chrome.py using venv Python..."
    & $venvPython (Join-Path $scriptDir 'Pro5Chrome.py')
} else {
    Write-Warning "Pro5Chrome.py not found in $scriptDir"
}

Pop-Location

Write-Host "Done."
