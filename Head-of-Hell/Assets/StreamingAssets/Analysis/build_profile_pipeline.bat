@echo off
echo ========================================
echo BUILDING PROFILE PIPELINE EXE
echo ========================================

REM Πήγαινε στο directory του script
cd /d %~dp0

REM Καθάρισε παλιά builds
if exist build rmdir /s /q build
if exist dist rmdir /s /q dist
if exist __pycache__ rmdir /s /q __pycache__

REM Φτιάξε exe
pyinstaller ^
    --onefile ^
    --clean ^
    runtime_profile_pipeline.py ^
    --name profile_pipeline

echo ========================================
echo BUILD COMPLETE
echo ========================================

pause