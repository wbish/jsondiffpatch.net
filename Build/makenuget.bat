@ECHO OFF

:parse
IF "%~1"=="" GOTO endparse

..\Tools\NuGet.exe pack .\JsonDiffPatchDotNet.nuspec -OutputDirectory . -BasePath . -Version %~1
GOTO end

:endparse
ECHO Usage: makenuget.bat ^<version^>

:end