@ECHO OFF

:parse
IF "%~1"=="" GOTO endparse

..\Tools\NuGet.exe pack ..\Src\JsonDiffPatchDotNet.nuspec -OutputDirectory . -BasePath ..\Src -Version %~1
GOTO end

:endparse
ECHO Usage: makenuget.bat ^<version^>

:end