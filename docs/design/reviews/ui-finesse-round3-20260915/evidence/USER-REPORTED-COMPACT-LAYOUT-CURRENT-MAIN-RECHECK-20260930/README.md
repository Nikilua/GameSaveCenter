# 紧凑窗口行距、任务栏摘要与传输标题复核

日期：2026-09-30。生产代码提交：`4ebbf81f63882a0b1c1e9e02ee214069bb552596`（`改善紧凑窗口布局与摘要层级`）。

## 本批处理

- 对真实生产视图中确认会折行的操作组使用共享 `16 DIP` 紧凑行距：存档历史、媒体筛选预设、媒体待归类批量和次级动作、任务筛选预设与 shell 操作。只在各自已有的紧凑断点启用；回到单行时恢复 XAML 原始 margin。未机械修改表单/开关类 WrapPanel。休眠的 `DashboardDemoShell` 仍为折叠状态，未把它计入当前可见页面修复。
- shell 紧凑断点从 `<1280 DIP` 调整到 `<1440 DIP`。因此 1280–1439 DIP 的窄窗口将页面标题放在第一行，游戏选择和操作放在第二行，并隐藏重复副标题；1440 DIP 恢复宽态结构。生产 WPF 行为覆盖 720、1040、1320、1440、1500 DIP 及缩放往返。
- Task Center 将加载量/更新时间放到独立的按钮行下方，避免摘要行高改变“重试可恢复 / 重置列宽”按钮几何；删除筛选条件的重复摘要行，筛选仍保留在筛选控件和“更多筛选”区域；摘要使用主题 muted brush。保留命令、绑定、Automation 名称与任务恢复行为。
- Cloud Transfers 的“传输明细”标题行改为对称上下边距，并使标题、计数胶囊及胶囊文字相对该行垂直居中。

## 提交后验证

Release 正式构建使用 `scripts/build.ps1 -Configuration Release -SkipTests`：XAML `24/24`，`0 warning / 0 error`；Playnite 插件目标 `net462`。`python scripts/validate-source.py` 通过。

各类独立 VSTest 的结果分别归档于 [`trx/`](trx/)；TRX 中机器本地 `codeBase` 已替换为仓库占位符 `REPO`，测试结果与计数未修改：

| 验证组 | 结果 | 覆盖 |
| --- | ---: | --- |
| `ReportedWorkspaceLayoutBehaviorTests` | `42/42` | Light/Dark 操作组真实换行间距、宽窄恢复；1320 DIP shell 单列与 1440 DIP 断点往返；任务摘要加长时按钮行不变高；传输标题/胶囊中心线 |
| `Q14ToolbarAlignmentBehaviorTests` | `1/1` | Task 筛选预设换行、16 DIP 实测净距和宽态恢复 |
| `ResponsiveLayoutCoordinatorTests` | `5/5` | 紧凑 shell 1280/1439 DIP 与 1440 DIP 宽态边界 |
| `TaskRetrySourceTests` | `2/2` | 重试命令/说明、队列摘要结构和筛选入口仍在 |
| `WpfUiResourceDictionaryTests` | `139 passed / 39 skipped` | 共享资源回归；39 项显式跳过，因其断言针对已撤销的今日工作台 UI 架构，不适用于恢复后的 AcrylicFork 生产页；没有失败 |
| R00-04 `LargeLibraryPerformanceTests` | `5/5` | 2000 项集合更新；30 个不同游戏搜索每次改变结果集合 `30/30`；有限超时负例 |
| R00-06 `MediaInboxGeometryTests` + `MediaWindowAnchorContractTests` | `13/13` | 四行/短窗回退、footer 可达、有限页滚动锚点契约 |
| R00-08 `GamePickerKeyboardBehaviorTests` + `KeyboardFocusSourceTests` | `11/11` | 无结果 Enter、IME 路由、候选确认、Esc、焦点回返 |
| R01-05 `UiNegativeFixtureRegistryTests` | `1/1` | N01–N05 五种预期失败检测器可捕获各自反例 |

2000 项合成游戏的 30 个热搜索样本：p50 `47 ms`、p95 `49 ms`、最大 `49 ms`；具体查询与采样见 [large-library.txt](large-library.txt)。定向测试之间不合并计数。

具体几何：存档与 Media Inbox 已测折行组的行净距为 `16 DIP`，回到单行时恢复原始 margin；Light/Dark Task 队列主行固定 `36 DIP`，三行摘要扩展不改变主行高度，重试/重置按钮为 `30/36 DIP`、中心差 `0.33/0 DIP`。Cloud Transfers 标题和胶囊相对标题行的中心差不超过 `0.33 DIP`，胶囊文字/标题差 `0.67 DIP`。普通说明与离页提示解析为主题次级/Muted 资源，语义 Info 色仍保留给信息状态。

RenderHarness 以相同提交和干净工作树执行：`render-qa OK`，Light/Dark，全矩阵，`WorkingTreeClean=True`；报告记录的 `DpiScale=1.00` 是离屏逻辑 DIP。选择的生产页面图：[shell 1366×768](screenshots/Shell-1366x768.png)、[存档历史 1040×700](screenshots/Save-1040x700-tab0.png)、[媒体待归类 1040×700](screenshots/Media-1040x700-tab0.png)、[任务中心 1040×700](screenshots/Task-1040x700.png)、[云端队列 1040×700](screenshots/Maintenance-1040x700-tab1.png)。完整报告：[render-qa-report.txt](render-qa-report.txt)；构建与 freshness 摘要：[build-summary.txt](build-summary.txt)、[ui-evidence-freshness.txt](ui-evidence-freshness.txt)。

## 程序集身份与边界

- Playnite 插件 `GameSaveCenter.Playnite.dll`（`net462`）：SHA-256 `098330E62345E1C27CD534AF92321515CED2BAF83AEFE95E87CCDEBA279FDC5E`，MVID `81ed3268-19e8-402a-aca2-a3d76ab7117d`。
- Playnite 测试 `GameSaveCenter.Playnite.Tests.dll`（`net472`）：SHA-256 `BB34E32CEA964B814AE0DCC07FA1EB441029175B992DEB6706F8F4E52804E25D`，MVID `437709e3-3d5b-43ce-8729-4a14b11b94f6`，程序集 `GscBuildCommit` 与本页提交一致。
- 颜色继续按主题资源表达：主题 accent 用于交互/重点，Info/Warning/Success 等语义色只用于对应真实状态，普通说明使用次级/Muted 文本；本批未作全产品色板替换。此做法与 [Windows Color 指引](https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/color)中克制使用颜色强调内容、主题适配的建议一致。此前用户看到的后台任务离页安全提示保留，但使用主题次级文字色。
- 这些结果来自隔离 STA WPF、合成数据和 OffscreenRenderHarness。没有安装/启动真实 Playnite，没有确认用户截图所用 DLL，没有切换物理 125%/150% DPI，也没有取得用户所要求的同一真实宿主 `[GSC-GRID-DIAGNOSTIC]` 滚动前后日志。因此 Media Inbox 实际滚动后列头/数据行偏移仍未验，不能以本证据关闭。
- 本轮没有写用户存档、媒体、云端或诊断数据。Demo 原设计目录在当前 checkout 不可用，继续沿用恢复的生产视觉基线；192 项总数与任务状态未因本批重新计数。
