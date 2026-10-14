# R11-03 差异列表当前 main 复核

日期：2026-10-14；分支 `main`；当前基线提交 `fd853380ec888ef94c112e255996b53106a0ddea`。任务实现已存在，本轮只补生产 ViewModel 行为证据，没有修改生产实现、DTO、XAML 或滚动系统。

## 当前实现与原有覆盖

- 复用 `BackupDiffDto`、`BackupDiffPathFilter`、`CopyPathCommand`、`BatchObservableCollection` 和 SaveCenter 现有有限详情滚动。新增/修改/删除按类型和不区分大小写的路径片段筛选；每类窗口最多显示 `diffPathVisibleLimit` 条，初始 120，生产“加载更多”每次增加 120。
- 三个匹配计数来自当前筛选完整结果，显示数来自有限窗口投影；摘要分别显示总匹配和当前显示量。“未变化”单独读取 `UnchangedCount`；`Exact` 显示精确比较，估算/无效质量另显示未知差异，空比较状态独立说明尚未比较。
- 原 `R11DiffListSearchBehaviorTests` 的纯 filter 用例覆盖类型/路径匹配、有限窗口及 HasMore 正反例；真实 `SaveCenterView` 的 STA 用例覆盖只读路径可选文本、复制按钮完整 CommandParameter 与执行 fake 命令，但没有驱动生产 DashboardViewModel 的搜索/加载行为。

## 新增的生产行为验证

新增第三个测试 `ProductionDashboardFiltersLoadsMoreAndKeepsUnknownSeparateFromZeroChanges`，通过隔离的未初始化 DashboardViewModel 调用生产 `RefreshDiffPathResults`、`LoadMoreDiffPaths`、`ClearDiffPathFilters` 和属性 setter；只注入合成差异、独立 `BatchObservableCollection` 与测试字段，不创建插件、不启动 Worker。

- 123 条新增、1 条修改、1 条删除输入，路径过滤后计数为 `123/1/1`，初始窗口 2 条/类；首屏共显示 4 条且仍有更多。
- 每次生产加载方法增加窗口后重新检查可见集合、总匹配计数和 HasMore；加载至末尾后再调用加载方法保持 125 条，不产生额外结果。
- 类型“删除”加路径 `slot/old` 后只保留一条删除结果，其他两类计数归零且没有更多；清除筛选恢复全部路径和有限窗口。
- `Estimated + UnchangedCount=0` 同时显示“未知差异”及“零变化：0 条”；`Exact + UnchangedCount=0` 显示精确比较且无“未知差异”；无比较结果使用独立占位状态。

`R11VersionComparisonBehaviorTests` 仍实际加载生产 `SaveCenterView` 并检查 A/B、方向交换与同版本禁用；`R11VersionSummaryBehaviorTests` 继续检查现有状态摘要。因此该组当前 main 串行回归为 R11Diff `3/3`、R11VersionComparison `2/2`、R11VersionSummary `2/2`，合计 `7/7`。同一构建的 R09 焦点类另为 `2/2`。

## 构建与检查

- 本机 SDK `9.0.302`、VSTest `17.14.1`；仓库 SDK pin `8.0.423` 本机未安装，SDK9 不能替代 CI SDK8/SDK10.0.401。
- 修改测试后 Release 重建 Playnite（`net462`）与 Playnite.Tests（`net472`），均 `0 warning / 0 error`。使用已有 restore assets，逐项构建并设 `BuildProjectReferences=false`；Contracts/Core/HostAuditSeeder 复用 R11-02 当前 main 中刚构建的输出，其源文件自该基线未变。本轮没有运行完整 solution build。
- `scripts/check-xaml.ps1`：`24/24`；`python scripts/validate-source.py` 通过；`git diff --check` 通过。没有视觉源码变更，不生成截图。

## 身份与脱敏结果

| 文件 | SHA-256 / 构建身份 |
| --- | --- |
| `R11DiffListSearchBehaviorTests.cs` | `73A2C8A184E5BFAFBCDC92583F74D94715CF9143164F133BEF6EA032F3276EF6` |
| `BackupDiffPathFilter.cs` | `11E1294DFC21AD434C964E584376815318368B56E6E9C00E92AEA20A6D7E5DF0` |
| `DashboardViewModel.cs` | `6C9A40DDC8AD762491CB3DD36FEA955AA957E69C1B6EA35E87FF350259377544` |
| `SaveCenterView.xaml` / `WpfUiProduction.xaml` | `4A86836BB5FB4CDA36DD2E4085F43A345EB9F2ABDBFDD1C7C96CE5CA8DC718F9` / `7CFCBEE2720F3C8960FA9B54D8B86562C87D17D0E44F679FCF846F05ACDCEB2B` |
| `GameSaveCenter.Playnite.dll` / `.pdb` | `D71DD6CDC3B96A9A3B9E89BABC5ACBED66F17F73CB5BB02A9FCFB4E9145357C2` / `E3257DEEE2F5DF7112A736494BBBB2F1FFD61657F49DA85F13A19671D15CA015` |
| `GameSaveCenter.Playnite.Tests.dll` / `.pdb` | `EFB50B83FCC5FA0E999209B6ED0C9AABD4ED5F2231E5E7652B1466E2CEA179B0` / `67D2915B312E009D42AC71B9767587CC8085EFB508E3B4DF76FF76A684916509` |
| `R11DiffListSearchBehaviorTests` TRX | `EC1A73B6AA7E4378800051488833ECB55254C110DF50B8EBB586C84CE9C60B99` |
| `R11VersionComparisonBehaviorTests` TRX | `645720AE610A4B28187B503748C41516623139C1D2CA4B0D8649339ADEE2AE6D` |
| `R11VersionSummaryBehaviorTests` TRX | `B504AD796CFB6D0198B2D4322CFC27C68982483F07336CD90AB14523DF348D86` |

Both current Playnite and test assemblies have ProductVersion `0.6.73+fd853380ec888ef94c112e255996b53106a0ddea`; the test source hash above identifies the added worktree test that was compiled. TRX files replace the local checkout path with `[repo]`.

## Boundary and next step

The copy-button STA fixture uses synthetic path text and a fake command so it does not write to the user's system clipboard; it verifies the complete bound value reaches the command. Production clipboard retry/sanitization against a live OS clipboard, real Playnite presentation, Worker archive reads, UIA/Narrator, physical DPI/cross-screen and final presented frames were not exercised. The new production-method test invokes the actual loading/clear methods through reflection but does not invoke a live Playnite dispatcher or Worker.

Next: R11-04 current main review; inspect the existing version note/metadata path, cancellation behavior and stable BackupId selection before deciding whether implementation is already sufficient.
