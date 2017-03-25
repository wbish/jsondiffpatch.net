@ECHO OFF

:parse
IF "%~1"=="" GOTO endparse

..\Tools\NuGet.exe pack ..\Src\JsonDiffPatchDotNet\JsonDiffPatchDotNet.nuspec -OutputDirectory . -BasePath ..\Src\JsonDiffPatchDotNet -Version %~1
GOTO end

:endparse
ECHO Usage: makenuget.bat ^<version^>

:end