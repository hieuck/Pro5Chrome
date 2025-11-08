@echo off
REM setup_and_run.bat - create venv, install requirements and run Pro5Chrome
REM Place this file in the project root and run it (double-click or from cmd)

SETLOCAL

:: Ensure we run from the script directory
CD /D "%~dp0"

:: Check Python availability
where python >nul 2>&1
if errorlevel 1 (
    echo Python not found in PATH. Please install Python 3 and add it to PATH.
    pause
    exit /b 1
)

:: Create virtual environment if missing
if not exist """%~dp0venv\Scripts\activate.bat""" (
    echo Creating virtual environment in "%~dp0venv" ...
    python -m venv "%~dp0venv"
    if errorlevel 1 (
        echo Failed to create virtual environment.
        pause
        exit /b 1
    )
) else (
    echo Virtual environment already exists at "%~dp0venv"
)

echo Activating virtual environment...
call "%~dp0venv\Scripts\activate.bat"

echo Upgrading pip...
python -m pip install --upgrade pip

if exist "%~dp0requirements.txt" (
    echo Installing packages from requirements.txt ...
    pip install -r "%~dp0requirements.txt"
) else (
    echo requirements.txt not found in "%~dp0". Skipping package installation.
)

echo Starting Pro5Chrome application...
if exist "%~dp0Pro5Chrome.py" (
    python "%~dp0Pro5Chrome.py"
) else (
    echo Pro5Chrome.py not found in "%~dp0". Exiting.
)

echo Done.
pause

ENDLOCAL
