# R11-02 双版本比较当前 main 复核

日期：2026-10-14；分支 `main`；源基线 `3a2d116353945dc41e026ca993c59ec5276bd2d3`。任务实现已存在，本批仅复核当前基线并补可复查证据，没有修改生产或测试源码。

## 当前实现和行为

- 复用 `BackupVersionDto`、`BackupCompareRequestDto`、`MessageTypes.CompareBackups`、`FileManifestDiffService` 和 `SaveCenterView`，没有新增比较/恢复服务或选择 DTO。
- 生产比较区的 A/B `ComboBox` 绑定 `CompareLeftBackup` / `CompareRightBackup`，显示稳定比较标签；完整值经 Tooltip 与 Automation HelpText 提供。比较、交换按钮分别绑定 `CompareBackupCommand` / `SwapCompareBackupCommand`，A→B 提示明确说明新增属于 B、删除属于 A。
- `R11VersionComparisonBehaviorTests` 使用合成 manifest 调用真实 `FileManifestDiffService`，验证方向交换会反转新增/删除、修改方向与字节增量。第二项测试加载真实 `SaveCenterView` 于隔离 STA `Window`，实际检查 A/B 选中项、完整 Tooltip/HelpText、摘要方向及比较/交换命令入口与执行。将选择改成同一 BackupId 后，按钮禁用、`CanExecute` 为 false，用户提示明确为“不会发起比较或恢复”。
- 生产 `DashboardViewModel.CompareBackupAsync` 在调用 Worker `RequestAsync<BackupDiffDto>` 前先运行 `TryGetComparisonSelection`；空、缺稳定 ID 或相同 ID 会清理旧比较结果并返回提示。该路径只发送 `MessageTypes.CompareBackups`，不调用 `RestoreCommand` 或 `RestoreExecute`。同版本防护的生产方法经源码检查；本次隔离 STA UI 测试使用 fake DataContext/命令，不启动 Worker，也不调用真实归档或恢复。

## 本机验证

- 本机 SDK `9.0.302`，VSTest `17.14.1`，testhost `.NET Framework 4.8.9181.0`。仓库 pin `8.0.423` 本机未安装；所有以下结果不替代 CI SDK `8.0.423` 或 `10.0.401`。
- 采用已有 NuGet restore assets，逐项构建 Contracts、Core、HostAuditSeeder、Playnite（`net462`）和 Playnite.Tests（`net472`）；各项目以 `--no-restore -p:BuildProjectReferences=false` Release build，按顺序复用刚编好的依赖，每项均 `0 warning / 0 error`。初次直接构建测试项目在 SDK9 项目引用框架发现阶段失败，诊断列出缺少的 workload locator SDK 目录但没有编译错误；因此未把它归因为源码问题，也没有报告完整 solution build。构建期间仅临时选择已安装 SDK，随后按原始字节恢复 `global.json`。
- `R11VersionComparisonBehaviorTests` 独立串行 `2/2`；同一构建下 `R09FocusOutlineBehaviorTests` `2/2`，0 失败、0 跳过。R09 结果另存于[外机失败复核包](../CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md)。
- `scripts/check-xaml.ps1`：`24/24`；`python scripts/validate-source.py`：通过。`git diff --check` 在提交前执行。没有生产视觉改动，不生成 RenderHarness 或截图。

## 身份与结果文件

| 文件 | SHA-256 / 构建身份 |
| --- | --- |
| `DashboardViewModel.cs` | `6C9A40DDC8AD762491CB3DD36FEA955AA957E69C1B6EA35E87FF350259377544` |
| `SaveCenterView.xaml` | `4A86836BB5FB4CDA36DD2E4085F43A345EB9F2ABDBFDD1C7C96CE5CA8DC718F9` |
| `WpfUiProduction.xaml` | `7CFCBEE2720F3C8960FA9B54D8B86562C87D17D0E44F679FCF846F05ACDCEB2B` |
| `FileManifestDiffService.cs` | `40E40520737E7D0C6A9860069B6F0AFE82C597550B27C1DA14EB981BBD59E329` |
| `R11VersionComparisonBehaviorTests.cs` | `FEFE9F7C563F46DFC213918E90CBB61AEFAD32266E896DB37EB4D5B61A720444` |
| `GameSaveCenter.Playnite.dll` / `.pdb` | `11D0D875657355094B69DA24C7DFA11279434255A82DC62F066A514E6D565B79` / `9BF4849E61CBEB31A87204D2CBFAB464543CDF2EA93FF0EA22B0C7C8601D4623` |
| `GameSaveCenter.Playnite.Tests.dll` / `.pdb` | `51E2FBF104D2C32B473E85989BBE897EF41A3A7D67B575E2D5CFD1277A176881` / `4F079B19B153591A6EC95963EDD7721E1FB89F601EC76913292537E45EC57749` |
| `R11VersionComparisonBehaviorTests-sdk9302-main-3a2d1163.trx` | `EAAB81A75DAC7F905D1A3E83ACF6669969A3CDBC9B96184E14BF61195D95DB9D` |

两套程序集 ProductVersion 均为 `0.6.73+3a2d116353945dc41e026ca993c59ec5276bd2d3`。TRX 已去除本机绝对路径。

## CI 与边界

当前公开 Actions 页面有加载错误，只显示旧快照中的 #972（head `8546aca`）In progress；另一次旧抓取显示 #979。匿名 API 曾返回 HTTP 403，页面中没有 `58090d99` 或 `3a2d1163` 的 run，无法确认它们是否创建或终态。SDK9 本机结果不替代 SDK8/SDK10 CI。

全部数据为合成 manifest/DTO 与 fake 命令；没有访问或修改用户存档、媒体、设置或云端，没有触发 Worker、恢复、对外诊断或真实 Playnite。未验证真实 UIA/读屏、物理 DPI/跨屏和 Playnite 最终呈现。

下一项：R11-03 当前 main 盘点；先查已有差异类型/路径筛选、有限加载、计数与完整路径复制，再按实际缺口决定是否改代码。
