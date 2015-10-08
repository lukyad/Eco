@echo off

REM Run this script from the root of Eco (the current directory should
REM contain Eco.sln). It will build the whole solution and push new
REM versions of Eco to nuget.org.

call "%VS140COMNTOOLS%vsvars32.bat" || goto :error

devenv /rebuild Release Eco.sln || goto :error

cd Eco || goto :error
if exist *.nupkg (del *.nupkg || goto :error)
nuget pack Eco.csproj -IncludeReferencedProjects -Prop Configuration=Release || goto :error
nuget push *.nupkg || goto :error
cd .. || goto :error

goto :EOF
:error
echo Failed with error #%errorlevel%.
exit /b %errorlevel%
