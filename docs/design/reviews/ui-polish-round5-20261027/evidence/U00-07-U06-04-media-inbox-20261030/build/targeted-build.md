# SDK8 定向编译记录

- Source content: the final production/test edits later committed as 64f26f58153efdb4a634bf8a0aa7c832898a581d.
- SDK: 8.0.423; Configuration: Release.
- Command: dotnet build tests/GameSaveCenter.Playnite.Tests/GameSaveCenter.Playnite.Tests.csproj -c Release --no-restore -m:1 -nodeReuse:false -p:NuGetAudit=false -p:MSBuildEnableWorkloadResolver=false -p:GscBuildOutputRoot=<isolated stage output root>
- Exit: 0; 0 warnings; 0 errors.
- The compile/test invocation preceded the source commit. Its GSC_BUILD_COMMIT metadata still named parent 40fd0bc438338e4aef45e16ee03df5bd83768b9c; do not cite it as a post-commit local identity check. Exact 64f26f5 validation is supplied by render-qa and both completed GitHub jobs.
