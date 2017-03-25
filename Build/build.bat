@ECHO OFF

"%ProgramFiles(x86)%\MSBuild\14.0\Bin\msbuild.exe" /p:Configuration=Debug ..\Src\JsonDiffPatchDotNet\JsonDiffPatchDotNet.csproj
"%ProgramFiles(x86)%\MSBuild\14.0\Bin\msbuild.exe" /p:Configuration=Release ..\Src\JsonDiffPatchDotNet\JsonDiffPatchDotNet.csproj
"%ProgramFiles(x86)%\MSBuild\14.0\Bin\msbuild.exe" /p:Configuration=Debug ..\Src\JsonDiffPatchDotNet\JsonDiffPatchDotNet.Portable45.csproj
"%ProgramFiles(x86)%\MSBuild\14.0\Bin\msbuild.exe" /p:Configuration=Release ..\Src\JsonDiffPatchDotNet\JsonDiffPatchDotNet.Portable45.csproj
