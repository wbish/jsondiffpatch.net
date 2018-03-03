@ECHO OFF

"%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\msbuild.exe" /p:Configuration=Debug ..\Src\JsonDiffPatchDotNet\JsonDiffPatchDotNet.csproj
"%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\msbuild.exe" /p:Configuration=Release ..\Src\JsonDiffPatchDotNet\JsonDiffPatchDotNet.csproj