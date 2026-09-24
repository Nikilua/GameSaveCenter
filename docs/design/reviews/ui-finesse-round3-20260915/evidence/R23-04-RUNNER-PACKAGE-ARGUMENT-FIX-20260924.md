# R23-04 隔离 runner 打包参数修正（2026-09-24）

## 问题与修正

首次执行合成库隔离流程时，Release solution 已构建成功，但流程在打包前停止。`dev-install-run.ps1` 用字符串数组 splat 调用 `package.ps1`，PowerShell 将 `-Configuration` 作为位置参数传入，触发 `ValidateSet` 失败。该次没有安装 GameSaveCenter 或 seeder、没有启动 Playnite、没有合成库 manifest；不能作为宿主或 CEF 结果。

现在改为哈希表具名参数：`Configuration`、`SkipBuild`、`BuildOutputRoot`，隔离审计另传 `SkipPackageArchives`。`DiagnosticsEvidenceSourceTests` 增加参数形状防回归断言。隔离审计的 transcript 与安装报告仍指向本轮 `Output`，避免覆盖通用安装日志。

## 验证

- `scripts/build.ps1 -Configuration Release -OutputRoot .tmp/r3b24 -SkipTests`：solution 成功，0 errors；有两条既有 `MediaCenterView.xaml.cs:703 CS8602` warning。
- 对 `RenderAndHostAuditEntriesDeclareEvidenceBoundariesAndTimingFields` 的定向 VSTest：`1/1` 通过，exit `0`。首次在更深的 `.tmp` 输出根遇到 .NET Framework 260 字符适配器路径限制，迁至较短的 `.tmp/r3b24` 后通过；首轮不是断言失败。
- `scripts/validate-source.py`、XAML `24/24`、两个相关 PowerShell AST parse 与 `git diff --check` 通过。

## 后续门禁

接下来仅重试一次原先未到达启动阶段的隔离 seed runner，核对同一 run ID 的 manifest 和 Playnite/CEF 结果。首次调用没有启动宿主，因此不受“宿主启动失败后不在相同状态下重试”的限制。所有写入继续限定仓库 `.tmp/` 隔离 profile 与本轮审计输出；不安装到真实用户 profile，不改用户存档、媒体、云端或诊断数据。
