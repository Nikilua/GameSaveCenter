# Trainer 版本页说明精简与 R00-07 复核（2026-09-30）

## 改动范围

提交 `22c55647799e732a770e6b16a93af4a9e83056f6`：搜索结果标题下的长句缩为“选择结果查看版本。”；可下载版本标题移除常驻的按需读取实现说明，把用户操作提示放到标题 Tooltip 和 `AutomationProperties.HelpText`。零版本时原空态仍出现，载入版本后收起；版本数量继续显示。未改变真实目录/版本 DTO、命令、下载、绑定或列表 Recycling 虚拟化。

## 当前提交验证

- `scripts/build.ps1 -Configuration Release -SkipTests`：XAML `24/24`，解决方案 Release `0 warning / 0 error`。
- `ReportedWorkspaceLayoutBehaviorTests`：`46/46`，Light/Dark。新增实际 WPF 用例读取标题 Tooltip 和 `TextBlockAutomationPeer.GetHelpText()`，验证标题无实现说明、空态随版本数从 `0` 到 `1` 收起、计数从“0 个版本”更新为“1 个版本”。
- 关联 `WorkspaceStateSourceTests`、`AccessibilitySourceTests`：总计 `12 通过 / 1 跳过 / 0 失败`。唯一跳过项 `SharedWorkspaceStatePresenterExistsAndIsUsedAcrossPages` 明确属于已撤销的旧今日工作台 UI 架构。合并筛选的 TRX 总计 `58 通过 / 1 跳过 / 0 失败`，均为提交 `22c55647` 的 Release 测试程序集。
- `UiAuditSourceTests`：`6/6`；`python scripts/validate-source.py` 通过；WPF 扫描 `31 XAML / 0 errors / 29 warnings / 177 info`。TrainerCenterView 中仍有两条既有“ScrollViewer 可能位于 StackPanel”的静态提示，本批没有更改该滚动布局。

测试记录：[TrainerCopyFinalBehaviorTests.trx](TrainerCopyFinalBehaviorTests.trx)、[UiAuditSourceTests.trx](UiAuditSourceTests.trx)。TRX 中机器仓库路径已脱敏为 `REPO`，本机名、账户名和部署目录标识已移除。

## 已重新验证 R00-07

因 `TrainerCenterView.xaml` 命中 R00-07 的来源路径，先前 freshness 将该记录标成 stale。当前身份复跑了 `UiAuditSourceTests 6/6` 和 [RenderHarness `toolbarprobe`](R00-07-toolbarprobe-report.txt)，三个实际 STA WPF 场景均符合分类条件：

- 正常输入长表单：按 `settings-form` 理由排除，可达且没有水平溢出。
- 同一滚动祖先下的 700 DIP 超宽工具栏：保留为 `action-toolbar`，检测到 360 DIP 可用宽度下的横向溢出。
- 同一滚动祖先下不可达工具栏：保留为 `action-toolbar`，可见区域为 `0x0` 并标记不可达。

探针结果 `toolbarprobe OK`，仅代表合成 WPF 几何。R00-07 基线来源身份更新为 `22c55647799e732a770e6b16a93af4a9e83056f6`；R00/R01 freshness `14/14 FRESH`，报告见 [ui-evidence-freshness.json](ui-evidence-freshness.json)。package identity 是 `not-provided`，不代表已安装或 Playnite 宿主验证。

## 离屏视觉复核

[render-qa-report.txt](render-qa-report.txt) 为当前提交的完整 Light/Dark 多页矩阵，记录 `render-qa OK`、`WorkingTreeClean=True`、`DpiScale=1.00`。精选图 [Trainer-1366x768-tab3.png](Trainer-1366x768-tab3.png) 使用合成数据显示 8 个版本：标题行只保留“可下载版本”和数量，列表正常显示。该图来自 `OffscreenRenderHarness`，不是用户 Playnite 截图。

## Release 程序集身份

所有程序集 `InformationalVersion=0.6.73+22c55647799e732a770e6b16a93af4a9e83056f6`；测试和渲染用的插件副本 SHA/MVID 一致。

| 程序集 | SHA256 | MVID |
|---|---|---|
| `GameSaveCenter.Playnite.dll` | `BA7FE26BD7321F0E0A26177E027D450D42EA80F1663C4C68EBB9AAB97339D2B7` | `cfc45589-0f59-4f93-b3a6-fe0691fb53e8` |
| `GameSaveCenter.Playnite.Tests.dll` | `2EA6975F61FDAEF969E7A72D18BD00C9EF91FD1FF0255091D7055A588CAC44C4` | `165d4a69-41dd-4a68-a3cf-7ddafe6ac52f` |
| `GameSaveCenter.RenderHarness.exe` | `9CD2CB2FC8747ADB0FFEF0A158EF6A12CB675543DD292E54AD3E6947E1FFB379` | `e66b11e0-cc70-4cd2-bcad-f0a53b1e62dd` |

## 未验边界与下一项

本批未启动真实 Playnite，也未核对用户安装 DLL、物理 125%/150% DPI 或最终屏幕帧；离屏 `DpiScale=1.00` 不代表物理 DPI。下一项继续审查有数据时重复出现的说明文字，保留安全、错误、取消和后台任务语义。Media Inbox 滚动偏移仍待同一 Playnite 进程的 `[GSC-GRID-DIAGNOSTIC]` 前后数据；R08-01 用户报告的 `1/2` 失败仍缺原始方法、断言/堆栈和 DLL 身份。
