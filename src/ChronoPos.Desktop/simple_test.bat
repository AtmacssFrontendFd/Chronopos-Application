@echo off
cd /d "C:\Users\adars\Desktop\pos-software"
echo Testing ChronoPos Application...
echo.
dotnet run --project src/ChronoPos.Desktop --verbosity detailed 2>&1
echo.
echo Application test completed.
pause
