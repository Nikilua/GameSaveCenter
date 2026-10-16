# GameSaveCenter 当前事实入口

更新：2026-10-16；分支 `main`，当前已推送 HEAD `72b2cbb77e4be53e9550983fd685d82ca9a31f88`；最近产品代码提交 `5157d8719110514aec857f2c0397ec4943b926e4` 为 R11-08。Actions #1041/run `38885898033` 对 `5157d871`：SDK10.0.401 job `116923431523` 成功，SDK8 job `116923431709` 最近已到扩展包生成、仍未终态，故 R11-08 CI 未收口。文档提交 #1042/run `38888296980` 两条 job 最近也仍在编译/测试。旧 #1040/run `38880099277` 对 `964f325d` 双 SDK成功，不含 R11-08 新代码；这些 run 分开记录。R11-08 本机 SDK9.0.302 定向 `6/6` 不替代仓库 SDK8/CI SDK10.0.401。证据见[R11-08](../design/reviews/ui-finesse-round3-20260915/evidence/R11-08-CURRENT-MAIN-INTERACTION-20261016/README.md)。

外机 R09 输出确实失败：`Expected 2,2,2,2 / Actual 1,1,1,1` 与历史 `3f7d0b30` 的 Chrome `2 DIP` 断言一致。`f13b9e05` 已拆为 Chrome `1 DIP` 和独立 `ValidationStroke 2 DIP`；截图对应祖先 `983fee5` 也包含新测试源码，而粘贴日志的 line 151 仍对应旧厚度断言、WPF 类数为 `116`（当前 main 为 `124`）。因此日志测试源码/PDB映射与该当前源码不一致；尚缺同次 `sourceCommit`、dirty paths、测试 DLL/PDB SHA，不能判断是旧测试程序集、另一个 checkout 还是其他构建输入。需诊断目录 `artifacts/build-diagnostics/20261014T052440239Z-3fc9f5f1` 的 `summary.json`/TRX 和 DLL/PDB 身份才能精确归因；未改生产模板或降低断言。见[R09 外机复核](../design/reviews/ui-finesse-round3-20260915/evidence/CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md)。

R00/R01 对当前 source HEAD `5157d871` 的只读 freshness 复核为 `14/14 fresh / 0 stale / 0 source-path matches`，比较 `2,883` 条路径；package identity `not-provided`。这只说明登记源码路径未变化，不代表安装包或宿主验证。简要证据见[R00/R01 当前 freshness](../design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-CURRENT-MAIN-RECHECK-20261016/README.md)。

.tmp 中旧目录与 R11-08 隔离输出已按授权清理，其他任务未跟踪文件保留。R00/R01 在 `5157d871` freshness `14/14 fresh / 0 stale / 0 source matches`；审计摘要已校正为 168 snapshots/116 warnings/0 HIGH/13 MEDIUM，13 条工具栏换行风险仍开。R13-05 当前 main 源与双 SDK #1040 成功身份一致，复核见[R13-05](../design/reviews/ui-finesse-round3-20260915/evidence/R13-05-CURRENT-MAIN-RECHECK-20261016/README.md)，状态“已满足”。下一步跟踪 R11-08 Actions 至终态，同时按依赖推进 R13-06。真实 Playnite、OS UIA/读屏/IME、物理 DPI/跨屏、最终呈现、ETW/宿主性能仍未验。



更新：2026-10-16；分支 `main`，最新推送基线 `8e6513856500505d4608f76ef098236c64e1b5b4`。Actions #1026/run `38690846810` 已终态失败：SDK8.0.423 job `116276140253` 的 R09 高对比行为断言失败，SDK10.0.401 job `116276140402` 的取消幂等测试因 `SemaphoreSlim` 已释放失败；两份完整日志已读取。Worker 竞态修复在本机 SDK8.0.423 已经 Release 全解方案构建 0 warning/0 error，Core 125/125，Worker 356 passed/1 个 Named Pipe 环境 skip，目标取消测试 1/1，高对比类 3/3；完整 Playnite 扫描只完成 6/122 个隔离类后中止，不能当作全量通过。新提交 Actions 尚待推送与跟踪。证据：[本阶段复核包](../design/reviews/ui-finesse-round3-20260915/evidence/CI-WORKER-CANCEL-RACE-FIX-20261015/README.md)。

更新：2026-10-16；分支 `main`，当前已推送源码 `676be4f3349d439198e834aa28341b3ca2883040`。本轮继续修正 R08 system-motion 测试的 Loaded/Dispatcher 时序；仅改测试夹具，不改生产 UI。本机 SDK `8.0.423` / VSTest `17.11.1` Release 测试项目 build `0 warning / 0 error`，R08 app/system 动效行为各 `1/1`，R09 高对比类定向复测 `3/3`，source validation 与 diff check 通过。Actions #1025/run `38687483121` 已终态 Failure：SDK10.0.401 job `116265550450` 在 R08SystemMotion 入场 opacity 动画观察失败；SDK8 job `116265550259` 在 R09HighContrast 行为矩阵失败（日志第195行 Assert.True，Expected True/Actual False）。两份完整日志已用 GitHub 插件取得；SDK8失败在本机同 SDK 定向 `3/3` 通过，当前根因未证实。新的 R08 夹具修复已完成本机验证，待提交推送并跟踪新 Actions。R00/R01 最近 source freshness 为 `14/14 fresh`、package identity `not-provided`；R08 CI 收口后须在新 source HEAD 重核。外机 R09 `Expected 2 / Actual 1` 仍缺该次 HEAD/DLL/PDB，根因未确认。后续待真实 Playnite、OS UIA/Narrator/IME、物理 DPI/跨屏、presented frame、ETW/宿主性能验证。

历史基线：R07-07 `2b423737` 与 R07-08 `d190f7ea` 的滚动/缩放复核证据分别见[R07-07](../design/reviews/ui-finesse-round3-20260915/evidence/R07-07-CURRENT-MAIN-RECHECK-20261015/README.md)、[R07-08](../design/reviews/ui-finesse-round3-20260915/evidence/R07-08-CURRENT-MAIN-RECHECK-20261015/README.md)；R10-02 `6eac7b90` 的锁定 SDK8 复核及同名选框行为见[R10-02](../design/reviews/ui-finesse-round3-20260915/evidence/R10-02-CURRENT-MAIN-RECHECK-20261015/README.md)。上面旧 CI/#1012 状态仅作历史记录；当前 run ID/终态以本页顶部的续跑状态为准。真实 Playnite host/UIA、物理DPI/跨屏、presented frame、ETW/宿主性能仍待验。

更新：2026-10-15；版本 0.6.73-development-preview；分支 main。R12-02 生产实现沿用 `6cc3a618`；`d6640887` 只补生产 `SaveCenterView` 运行时状态行为。当前主机仅安装 SDK `9.0.302` / VSTest `17.14.1`，Worker `15/15`、Core `40/40`、Playnite `11/11`，共 `66/66`；真实详情卡隔离 STA 显示无哈希、部分覆盖/过期、损坏三个状态和完整时间辅助文本。定向 Release 测试项目/引用均成功构建，非完整 solution build；XAML `24/24`、source validation/diff check 通过。SDK8/SDK10 CI 独立追踪，Actions 终态尚未核实。证据见[R12-02 当前 main 复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R12-02-CURRENT-MAIN-RECHECK-20261015/README.md)。

R12-03 生产路径映射与预览能力沿用 `c0adb1b7`；当前 main 只补测试。SDK9.0.302/VSTest17.14.1 下 Playnite `3/3`、Worker `4/4`；实际生产 `MaintenanceView` 隔离 STA 核对长路径全选、Copy CanExecute、80 项 Recycling 有限视口，Worker 合成负例核对相似根/异盘、不确认拒绝、缺失目标跳过且不写目标/索引。XAML `24/24`、source validation/diff check 通过，非完整 solution build。证据见[R12-03 当前 main 复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R12-03-CURRENT-MAIN-RECHECK-20261015/README.md)。本次未改生产实现或系统剪贴板；真实 Playnite、剪贴板、UIA/IME、物理 DPI/跨屏和呈现帧仍待验。

Q24-04 继续复用既有 Task Center 控件与命令，只增强 `R21KeyboardNavigationTraceTests`。本机 SDK9.0.302/VSTest17.14.1 定向 Release 项目构建后，R21 `3/3`、R06 `2/2`、R23 `9/9`，共 `14/14`；用合成成功/运行中/失败任务在生产视图中验证筛选、表格、技术详情、页尾控件正向和反向焦点路径，以及失败态 Retry/Cancel 启用状态。no-op fixture 命令未执行；非完整 solution build、无生产 UI 改动。证据见[Q24-04 业务状态键盘补证](../design/reviews/ui-finesse-round3-20260915/evidence/Q24-04-BUSINESS-STATE-KEYBOARD-RECHECK-20261015/README.md)。R00-08 baseline 已按提交 `e221feb3` 更新；Q24-05 焦点语义已复核；Q24-06 当前 main UI Automation 已复跑 24/24，真实 Narrator/Playnite UIA 仍待验。R00/R01 freshness 在 source HEAD `4eb778e9` 重查为 `14/14 fresh`；R00-01..07 与 R01-01..08 保留“已满足”，R00-08 仅受控行为已满足，真实宿主/OS 输入仍待验。R07-04 横向输入与端点证据已补，下一项 R07-05 详情断点当前 main 复核。

R00-08 旧状态 `55ed3a17` 的 GamePicker `6/6`、R21 `2/2`、R10 帮助 `5/5` 保留；本轮仅 R21 keyboard source 变化并复跑 `3/3`，当前行为合计 `14/14`。baseline sourceCommit=`e221feb3`、freshness `14/14`；测试为隔离 STA 与合成输入，不代表真实 Playnite/OS IME。证据见[R00-08 当前 main 复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R00-08-CURRENT-MAIN-RECHECK-20261015/README.md)。

外机 R09 再次报 Expected 2/2/2/2、Actual 1/1/1/1、stack line 151；用户称该机已从 Git 拉取最新，但片段不含实际 HEAD、工作树、测试源/DLL/PDB 身份。失败断言特征与历史 `3f7d0b30` 的旧 Chrome 2 DIP 检查吻合；`f13b9e05` 已拆为 Chrome 1 DIP 与独立 ValidationStroke 2 DIP，提交 `983fee5` 已包含新测试源码。当前 main 第 151 行检查错误色，第 152/154 行检查厚度。因此旧测试程序集/源映射仍只是推断，根因未确认。`libpng iCCP` 不在失败栈；WPF COM 异常发生在断言失败后的收尾。没有更改生产模板或放宽断言，详见[R09 外机复核包](../design/reviews/ui-finesse-round3-20260915/evidence/CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md)。

CI：暂停检查点 #920/run 36677346317 与 #919/run 36675309812 的 SDK 10.0.401 失败日志已归档，失败为旧测试夹具没有固定系统动效策略，不代表后续提交 CI。Q24-04 代码提交 e221feb3 对应 #1006/run 38624032938：SDK8.0.423 job 116058522011“编译与测试”失败，SDK10.0.401 job 116058522588“隔离工作树并运行 Release 编译与测试”失败。文档提交 cd27c8c0 对应 #1007/run 38625232841：SDK8 job 116062580362 与 SDK10 job 116062580995 相同步骤失败。匿名 API 403、step-log 路由 404，artifact ZIP 未取得，根因未知。#1008/run 38630493740 与 #1009/run 38631425713 于 2026-10-15 均 Failure；对应 job IDs 为 116080557876/116080558142 与 116083742041/116083741622。公开页面不含完整 step log，API 403、step-log 路由 404、gh CLI 不可用；用户附加的 #970/#971 ZIP 是更早 source commit，不是这两次 run 的日志。根因未知，先取完整日志再定位；本机 SDK9 不能替代 SDK8/SDK10。
本页只保留当前结论与下一步，不再累加阶段日志。9 月 30 日及以前完整原文移至 [历史快照](CURRENT_STATE_HISTORY_THROUGH_20260930.md)，旧记录中的“当前”“下一项”只代表当时状态。

## 当前判断

核心功能和六个工作区已实现，当前属于功能较完整、尚未完成发布验收的开发预览版。不能用测试数量、Round3 的“已满足”或旧评估的 95%/98% 推导正式稳定版完成度。详见 [完成度评估](../FEATURE_COMPLETION_ASSESSMENT.md)。

| 维度 | 当前可核查事实 |
| --- | --- |
| 最近完整 Release | clean source identity `4d166f017dcba8fbb07980ec21b690267b9ff377`、隔离 SDK `8.0.423`、`0 warning / 0 error`、XAML `24/24`；main `8e71feb4` 之后只有文档/证据变化，source/tests 不变 |
| 最近完整脚本测试 | clean source identity `4d166f0` 上 `1560 passed / 57 skipped / 0 failed`：Core `125/125`、Worker `357/357`、Playnite source `470 passed / 18 skipped`、114 个 WPF 隔离类 `608 passed / 39 skipped`。Named Pipe 环境 skip 与其余 `NotExecuted` 未计通过 |
| 当前实现阶段 | `CLOSE-PICKER-01` 本机代码和自动化门禁已实现；新增 STA Dispatcher 线程负例证明旧实现线程错误，删除回退/虚拟化全量通过。R09 测试已校正为检查独立 `ValidationStroke` 两 DIP 覆盖层，不错误断言 Chrome 改为两 DIP |
| R02-03 当前复验 | 生产实现未变；加强后的 STA 提示焦点/UIA peer行为与Save紧凑布局定向 `5/5`。本机 SDK `8.0.423` Release `0 warning / 0 error`、XAML `24/24`；WPF 静态审查 `0 errors/29 warnings/177 info`；RenderHarness 通用双主题 `render-qa OK`，未作为禁用提示截图证明 |
| R02-04 当前复验 | 当前 main Release `0/0`、XAML `24/24`；生产资源 STA 几何 `7/7`，16/20 DIP 图标间距各 `8 DIP`、中心差 `0`；Light/Dark RenderHarness `372` 图 `render-qa OK`，已人工对照并归档 Save 1040×700 两主题截图。未改生产 UI |
| R02-05 当前复验 | 当前 main Release `0/0`、XAML `24/24`；生产资源 STA 命中/间距/82 DIP 换行 `2/2`；Light/Dark RenderHarness `372` 图 `render-qa OK`，已检查并归档 Media 1040×700 两主题截图。未改生产 UI |
| R03-01 当前复核 | main `8e71feb4`；RenderHarness Release `0 warning / 0 error`、`TypographyDiagnosticsTests 11/11`、双主题 `finesseprobe` exit 0；中文/英文/数字/𠮷/组合重音均有实际 GlyphRun。报告 `WorkingTreeClean=False` 是证据目录已在工作树的真实记录；自 `4d166f0` 以来 src/tests 无变化。截图/报告与 identity negative 见[R03-01 当前复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R03-01-CURRENT-MAIN-RECHECK-20261012/README.md) |
| R04-02 当前复核 | main 源码/测试身份 `32c0e1e2`；Playnite net472 定向行为 `5/5`，覆盖摘要链接跨 Tab 后字段可见、键盘焦点、HelpText/滚动，以及禁用巡检字段旧非法值和路径负例。结构检查仅作补充；没有执行真实 Playnite 设置保存/Worker 应用。详见[R04-02 当前复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R04-02-CURRENT-MAIN-RECHECK-20261012/README.md) |
| R04-03 当前复核 | `e952c414` 工作树上的插件 net462 / 测试 net472 定向构建，隔离 SDK `8.0.423`，`SettingsDraftLifecycleBehaviorTests + PortableSettingsTests + SettingsValidationSourceTests` 为 `12/12`；关闭事件实际选择“否”保留草稿并恢复焦点、“是”后才关闭/回滚。真实 Playnite 原生确认框、宿主保存和 Worker 应用仍待验。详见[R04-03 当前 main 补充证据](../design/reviews/ui-finesse-round3-20260915/evidence/R04-03-CURRENT-MAIN-RECHECK-20261012/README.md) |
| R05-01/02 当前 main | commit `1c39e909856faa736b32224ee196ef0bac6ca460`，隔离 SDK `8.0.423`，Playnite 插件 `net462`、测试 `net472`；R05Focus/R05Option/R21 定向合计 `11/11`。TRX SHA/MVID、插件 SHA/MVID 已归档。Dashboard 受控 STA fixture 覆盖焦点进入、Tab 环绕、即时/动画关闭和取消回焦；XML 解析断言核对真实 XAML Click 接线。完整 Dashboard XAML/Playnite 宿主未加载；VSTest 收尾有三条 TextServicesHost `InvalidComObjectException` 消息但 exit0，原因未知 |
| R05-03 当前 main | 同一 commit 和隔离 SDK 下 `R05PopupBoundaryBehaviorTests + R05OptionVirtualizationBehaviorTests 7/7`。短窗 Picker 边界、当前桌面 ComboBox Popup 翻转后的工作区几何/MaxHeight/滚动/选定项可见通过。隔离 Playnite bootstrap 于插件安装前因 graceful shutdown 未完成而被保护策略阻止，未取得 popup/呈现证据；“翻转无闪屏”、420×220 负例、真实 Playnite/多屏仍未验 |
| R05-04 当前工作树 | 基于 main `903525be`，测试源 SHA `B6D804AD...`；本机 SDK `8.0.423` Release solution `0/0`，`R05MultiSelectionSummaryBehaviorTests 3/3`。验证 0/1/2 摘要、隐藏 ID 例外、清空“已忽略”选择同时保留“待归类”选择并保持模式；未改生产视觉。身份/边界见当前复核包 |
| R05-05 当前 main | `fbf504d2a7aabcd322c3c735959a31c7e1679fc9`；媒体批量选择用 Extended DataGrid + `SelectedItems`；唯一显式三态样例在开发校对页。XAML `24/24`、Release `0/0`、源契约 `24/24`；源测试为字符串断言，不覆盖 Space/UIA/集合行为。维持不适用，见[复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R05-05-CURRENT-MAIN-RECHECK-20261013/README.md) |
| R05-06 当前 main | 生产基线 `0fcb44f8`，只补同值重复赋值负例；隔离 SDK `8.0.423`、官方 Release build `0/0`、XAML `24/24`，定向 `28/28`。当前行为、TRX 与源码/程序集身份见[复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R05-06-CURRENT-MAIN-RECHECK-20261013/README.md) |
| R05-07 当前 main | 生产 Tooltip 行为未改；测试夹具在 main 4572118f 置顶/激活临时窗口，并先确认 WPF hover 再计驻留。本机 SDK 8.0.423 Release 0/0、XAML 24/24；Tooltip 2/2 两轮。#952 于“编译与测试”失败，12m07s；job 日志要求登录，artifact 11774606363（556 KB，SHA-256 9686cc633488c860c9dd8021690986abfff69129f1420ced7b433ff062490a3a）内容不可读，实际 runner SDK/根因未知；不凭摘要盲改。详情见[复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R05-07-CURRENT-MAIN-RECHECK-20261013/README.md) |
| R06-01 当前 main | 同一 `afade3cd` 最新 main 重建 SDK `8.0.423` Release `0/0`、XAML `24/24`；`R06ColumnWidthPersistenceBehaviorTests 6/6`，含 Task 列键/生产列定义一致性；当前代码和程序集 SHA、TRX 见[复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R06-01-CURRENT-MAIN-RECHECK-20261013/README.md)。旧双主题截图不绑定当前 commit |
| R06-02 当前 main | `90454586` 隔离 SDK `8.0.423` Release `0/0`、XAML `24/24`；`R06SortingBehaviorTests 7/7`。实际 WPF 列头双向点击/箭头、乱序重加稳定次键、未知值和失效视图负例见[复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R06-02-CURRENT-MAIN-RECHECK-20261013/README.md)。历史 RenderHarness 图绑定 `c577afc5`，未当作当前截图 |
| R06-03 当前 main | 生产实现沿用 `d83c7378`；本轮以 `fdeeec8` 生产源加单项测试差异构建，SDK `8.0.423` Release `0/0`、XAML `24/24`；R06Selection `2/2`、四表双主题状态几何 `1/1`、Task 双主题受控 hover/错误徽章 InputHitTest/Failed 终态禁用与 Running 取消按钮 UIA Invoke `1/1`、共享资源契约 `2/2`。测试仅证明隔离 WPF 状态与逻辑 DIP，不是实际 OS 指针或 Playnite 呈现。详情见[复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R06-03-CURRENT-MAIN-RECHECK-20261013/README.md) |
| R06-04 当前 main | 复用 afe4aa55 已有 DataGridClipboardBehavior 和五类 profile；生产行为复核 5/5，复制反馈 2/2。本机 SDK 8.0.423 Release 0/0、XAML 24/24，Q14 1/1、侧栏 22/22、R06Clipboard 5/5、R22 2/2。#952 完整日志显示其失败发生在旧 R07 resize 初始断言；本轮修正尚无 CI 结果，R06-04 不因本机定向通过签收。详情见[当前复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R06-04-CURRENT-MAIN-RECHECK-20261013/README.md) |
| R06-05 当前 main | 复用现有共享列头说明、排序与单位格式；本机 SDK `8.0.423` Release `0/0`、XAML `24/24`、`R06ColumnHeaderHelpTests 2/2`。当前身份 RenderHarness `372 PNG`、`render-qa OK`；代表图和 header contract 见[复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R06-05-CURRENT-MAIN-RECHECK-20261013/README.md)。本轮未改生产 UI；受控 STA 与离屏逻辑 DIP 不等价真实 Playnite/OS 输入 |
| R06-06 当前 main | 复用 `TaskStatusDto`、`TaskIndexedCollection`、现有事件合批与 `TaskSummary`；SDK `8.0.423` Release `0/0`、XAML `24/24`，Playnite 隔离 `34/34`、Worker 合成 SQLite `9/9`；真实 DataGrid 验证 80 行更新只发生 `Replace`、选择/行数/滚动偏移保持，未知/0/钳制进度、取消状态与服务端分页总数各有行为证据。TRX 哈希/程序集身份见[R06-06 当前 main 复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R06-06-CURRENT-MAIN-RECHECK-20261013/README.md)。本轮无当前身份截图，旧图仍绑定 `72fd6d9a`。混合非隔离试次 `22/23` 的单一失败在滚动 viewer 查找，已披露且未混入通过数。#961 是旧 run；当前代码检查点 #968 的 SDK8/SDK10 全门禁已成功；真实 Playnite/取消竞争、UIA、物理跨屏、presented frame、ETW 和宿主性能未验 |
| R07-08 / R08 当前 main | #959 CI 的 R07 resize `1/1` 通过，R08 fixture 的系统动效前提已在 `d60e472d` 修正；本机 SDK `8.0.423` Release `0/0`、XAML `24/24`、R07 `1/1`、R08 `2/2`、native motion policy `22/22`。#960/run `38327521241` 已确认 terminal failure、job `115069541561` “编译与测试” exit 1；完整日志 endpoint 404/API 403，SDK/根因未知。#968 当前源码自动门禁成功；真实宿主仍待验。历史日志/证据见[证据包](../design/reviews/ui-finesse-round3-20260915/evidence/CI-R07-08-RESIZE-FAILURE-20261013/README.md) |
| Q23-08 当前 main | 复用现有 ResetCatalog、导入预览/确认、Playnite Save/Cancel；SDK `8.0.423` 本机 build `0/0`、行为 `12/12`、XAML `24/24`、source validation 通过；Light/Dark × `1040×700`/`720×640 DIP` 的四场景动作可达性通过。视觉栏通过，宿主外部阻塞、最终未完成。证据见[复核包](../design/reviews/ui-finesse-round3-20260915/evidence/Q23-08-CURRENT-MAIN-RECHECK-20261013/README.md) |
| Q24-01 当前 main | `3ca41ec9` 新增受控分数 DPI 行为验证；SDK `8.0.423`，Release 定向测试 `1/1`。生产资源字体/图标/1 DIP 描边在 RenderTargetBitmap 的 96/120/144/168/192 DPI 目标栅格可见，负例覆盖裁切和永久缩放；受控离屏视觉通过，物理 DPI/Playnite Host 仍待验，最终未完成。详见[证据包](../design/reviews/ui-finesse-round3-20260915/evidence/Q24-01-CURRENT-MAIN-LOGICAL-DPI-RECHECK-20261013/README.md) |
| Q24-02 当前 main | `5fcd6856` 记录生产 PageHost 实际宽高；`74815b2b` 修复 Tooltip/Popup roots 跨 Dispatcher 遍历并增加双 STA 回归。SDK `8.0.423` 定向 `9/9`，覆盖 PageHost 窄/短/宽恢复、2,000 条合成收件箱首尾可达/虚拟化及 Tooltip 行为；扩展 Popup/焦点组 `8/16`，失败边界未验收。真实 Playnite、物理跨屏和最终呈现未验。详见[证据包](../design/reviews/ui-finesse-round3-20260915/evidence/Q24-02-CURRENT-MAIN-PAGEHOST-RECHECK-20261014/README.md) |
| Q24-04 当前 main | `87070909` 加强 R21 键盘轨迹测试；SDK `8.0.423`、Playnite `net462`/tests `net472` 定向 `8/8`。八入口 90 个当前可见交互控件双向可达、反向顺序互逆，选框行为 6/6。没有完整业务行态或真实 Playnite/OS 键盘；视觉/宿主/最终仍未验。详见[证据包](../design/reviews/ui-finesse-round3-20260915/evidence/Q24-04-CURRENT-MAIN-KEYBOARD-RECHECK-20261014/README.md) |
| Q24-05 当前 main | 复用 b20f1a4b 及现有状态模板；隔离 SDK 8.0.423 Release 构建 0/0，焦点/选中/当前态与选框关闭回焦行为 7/7。选中与键盘焦点区分、双主题及禁用/失焦负例通过；真实 Playnite、OS UIA/读屏、物理焦点像素仍待验。详见[Q24-05 证据包](../design/reviews/ui-finesse-round3-20260915/evidence/Q24-05-CURRENT-MAIN-FOCUS-RECHECK-20261014/README.md) |
| Q24-06 当前 main | `af8ed430` 更新 11 个生产 DataGrid 的 Name/HelpText；进程内 peer 验证 Grid/Table/Selection pattern 与选中行变化，Task 图标按钮名称、进度 Off live setting/焦点负例通过。Release 0/0，R21Automation 24/24，R00-06 freshness 行为 13/13，freshness 14/14。实际 Narrator/Playnite/OS UIA 未验，因此代码完成待验收。详见[Q24-06 证据包](../design/reviews/ui-finesse-round3-20260915/evidence/Q24-06-CURRENT-MAIN-UI-AUTOMATION-20261014/README.md) |
| Q24-07 当前 main | `0243c108` 将高对比与有效动效状态沿视觉树继承；系统语义色、禁用/焦点/选中/Popup、活动动画时钟清理与恢复行为通过。SDK `8.0.423`，Release `0/0`，XAML `24/24`，WPF `13/13`。代码待 CI/外部验收。真实 Playnite、Windows 高对比切换、OS 键鼠/UIA/Narrator、物理 DPI/跨屏和最终呈现未验。R05 Tooltip 相邻类 `2/3` 的失败在 GetCursorPos 前置阶段，TRX 单列。详见[Q24-07 证据包](../design/reviews/ui-finesse-round3-20260915/evidence/Q24-07-CURRENT-MAIN-HIGH-CONTRAST-RECHECK-20261014/README.md) |
| Q24-08 当前 main | 复用现有长文本、TextComposition、键盘与复制实现；本机 SDK `8.0.423` Release `0/0`、XAML `24/24`、定向 `15/15`。双主题长标签/AutomationPeer/Space command、合成中文 composition 和 82 DIP 长值复制负例通过；无生产 UI 改动。真实 Windows IME、Playnite/Narrator、系统剪贴板及运行时 Tooltip 仍待验。详见[Q24-08 证据包](../design/reviews/ui-finesse-round3-20260915/evidence/Q24-08-CURRENT-MAIN-LONG-TEXT-ASSISTIVE-INPUT-RECHECK-20261014/README.md) |
| Q25-01 当前 main | 复用既有搜索/可见列表实现；SDK `8.0.423` Release `0/0`、XAML `24/24`、4 类 `13/13`。2000项5次预热+30个不同查询 p50/p95/max=`45/60/60ms`，受控 Window 20次输入到可见容器=`46.214/75.782/76.334ms`，GamePicker 键盘路径6/6。受控行为可满足；不等于真实 Playnite/物理输入/呈现帧。详见[Q25-01 证据包](../design/reviews/ui-finesse-round3-20260915/evidence/Q25-01-CURRENT-MAIN-HOT-RESPONSE-RECHECK-20261014/README.md) |
| 远端 CI | 当前推送候选 `a81e7abd` 的 Actions #1039/run `38872163664` 已终态成功：SDK8 `8.0.423` 与 SDK10.0.401 两 lane 均通过；完整 Playnite WPF 124类/Source108类、Core125、Worker362成功，SDK8 Render QA/静态审查/扩展包成功。旧 #992/#993 完整日志已取回：两次 SDK8与SDK10都失败于 R08 motion 测试夹具 `opacity=1; loaded=True`，非R09/R10-06；后续夹具调整后 #1039 通过。外机 R09 当前附日志与main测试源不一致，具体设备输入身份仍待补。|
| R00/R01 证据 | source HEAD b1e7b652 的 freshness 为 14 fresh / 0 stale，0 条命中登记源路径；比较区间 2,848 个变更路径并非 docs-only。R00/R01 登记源码未变；保留受影响行为与审计证据，不将 freshness 等同于重跑。R00-07 audit 最新摘要 168 snapshots / 116 warnings / 0 HIGH / 13 MEDIUM / 0 Fidelity / 0 failed routes，13 条工具栏换行风险保持打开；package identity not-provided。详见[R00/R01 当前 main 复核](../design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-CURRENT-MAIN-RECHECK-20261016/README.md)。|
| 真实宿主 | 未启动真实 Playnite；Media Inbox 同进程滚动、物理 DPI/跨屏、IME、Narrator/UIA、ETW 与最终呈现帧均未验，`MANUAL QA REQUIRED` |

仓库锁定 SDK `8.0.423` 已由官方安装脚本安装到 `D:\GameSaveCenterTools\dotnet-sdk-8.0.423`；通过临时进程 PATH 前缀选择，系统 SDK/PATH 未改。历史 SDK/CI/IPC 诊断见[SDK阶段证据](evidence/close-sdk-20261007/README.md)、[CI证据](evidence/close-ci-20261007/README.md)和[IPC证据](evidence/close-ipc-20261005/README.md)。R05 新确定性负例与修复正例已记录；旧的第三轮失败保留为归因证据，不能用旧clean DLL独立通过覆盖。run930、run931、run932、#934、#935 与 #936 的失败原因均未取得完整日志；访问受限期间不盲修。

## 当前执行顺序

用户明确要求继续 Round3；截至 2026-10-16 09:00（Asia/Shanghai）无法回复，不因缺少输入停住独立工作。

1. R21 UIA 选择修正已完成本机验证、中文 commit/push，Actions #1032/run 38838015414 两个 SDK lane 均终态成功；不沿用此前 #1031 的失败状态。
2. R00/R01 当前 freshness 已在源码 b1e7b652 复核为 14/14 fresh、0 stale、0 条命中登记源路径；R00-07 当前账本的 audit 摘要已从旧的 110/7 校正为 116 warnings / 13 MEDIUM。13 条工具栏换行风险继续打开；package identity 未提供。行为/视觉旧证据因源路径未变而保持原状。
3. R12-04 当前 main 的本机证据已补齐：Worker restore/lock `26/26`、Playnite 阶段行为 `8/8`；测试改动只新增锁冲突负例。推送后待双 SDK Actions 终态；之后核对 R12-05 现有 remote staging/取消证据，不访问真实存档。

本机 R21 使用 SDK8.0.423；远端 SDK8.0.423 与 SDK10.0.401 两条 CI 均通过。Playnite 全量本机扫描未完成（6/122 类），真实 Playnite、OS UIA/Narrator/IME、物理 DPI/跨屏、最终呈现帧、ETW/宿主性能仍未验。Demo 原目录不可用；视觉沿用已恢复生产基线。

## 架构与生产 UI

- Playnite 插件 `net462`，Core/Contracts `netstandard2.0`，Worker `net8.0-windows10.0.19041.0`，Playnite 测试 `net472`。插件与 Worker 经 Named Pipe 协作，SQLite 由 Worker 持久化。
- 生产外壳为 `AcrylicProductionShellView`；局部共享入口 `AcrylicProductionResources.xaml`，配合 `DesignTokens.xaml`、`WpfUiProduction.xaml`、`Redesign.xaml` 等资源。六工作区：Overview、Save、Trainer、Media、Task、Maintenance；另有 Settings。
- Demo-first 唯一主要视觉基准仍为 `GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design/`；该目录当前工作区不存在。按既有授权沿用已恢复的生产基线，不伪造 Demo 对比结果。WPF 技能只负责质量检查。
- 游戏状态/排序选择沿用已修复的 `DropDownClosed` 提交路径；动态选项、用户选择、真实命令、取消/错误与滚动/虚拟化契约必须保留。页面结构可按当前 AGENTS 授权重构，不沿用旧历史记录中的页面冻结禁令。
- `DashboardViewModel` 主文件约 6480 行，已存在 16 个 partial 文件；这是维护风险，不是未实现功能或已测出的性能瓶颈。

## 验证与交付规则

- 行为矩阵入口 `scripts/e01-behavior-matrix.ps1`；构建 `scripts/build.ps1 -Configuration Release`，可用 `-OutputRoot` 隔离；XAML/source/freshness/RenderHarness 各自只证明其覆盖范围。
- 不把离屏 logical DIP、合成 WPF、进程内 UIA provider 当成 Playnite、物理 DPI、OS 输入或最终呈现帧。
- 每个独立阶段同步文档、记录构建/测试实际结果，单独 commit 并 push 当前远端分支；本次不安装到用户真实 Extensions，不操作真实存档、媒体或云端。
- 后续依次阅读 [PROJECT_MEMORY.md](PROJECT_MEMORY.md)、[WORKLOG.md](WORKLOG.md)、[DEVELOPMENT_HANDOFF.md](../DEVELOPMENT_HANDOFF.md) 与 Git 状态。只在追溯具体问题时读历史大文件。

## 历史：2026-10-13 SDK 10.0.401 CI 门禁实现（后续由 #968 收口）

- #965 用户诊断包确认 CI SDK `8.0.423`；布局类 54 项中 4 项失败，均已在 `5eaa12bf` 以显式逻辑 Grid 宿主修正。#966/run `38352437420` 对该提交已终态成功；其 SDK 仍为 `8.0.423`。
- 已新增 SDK `10.0.401` 独立 Actions job 和隔离 worktree 验证脚本。仓库 `global.json` 与原 SDK8 job 保持不变。本机 SDK `9.0.302` 全量 Release 编译成功；Core `125/125`、Worker `357/357`、Playnite `1566` 通过/0失败；另有 57 条旧 UI 架构断言以既有规则 NotExecuted。布局类 `54/54`。
- 该门禁后来在 #968 对 SDK8 `8.0.423` 与 SDK10 `10.0.401` 两个 job 验证成功；#967 SDK8 源码结构检查的 BOM 修正随 `4aa1ec7` 提交。此处 SDK9 本机记录只作历史交叉检查；当前执行顺序见本页上方。
- 保留其他任务 R04/R05/R07 未跟踪证据。真实 Playnite、OS UIA/IME、物理 DPI/跨屏、presented frame、ETW及宿主性能未验；Demo 原目录不可用，沿用恢复的生产基线。

## 2026-10-15 当前恢复点：R10-03 本机行为通过，Actions 待复核

- `main` 源码基线 `95109c221ec85a52c61c51c0a119a2f3d97efacf`。R10-03 使用已存在的搜索策略、Dashboard 路由、Shell 命令和工作区搜索框映射；只提取可测路由/目标解析入口并增加 WPF routed-event 行为用例。SDK `8.0.423` / VSTest `17.11.1`，Playnite `net462` / tests `net472` Release 定向 build `0/0`，R10 相关组 `13/13`。当前媒体页焦点实际移到 `MediaSearchTextBox`；弹层占用和编辑按键负例保留焦点。
- 当前 R09 外机断言对照在本机 `2/2` 通过；#967 附件的 SDK10.0.401/#983fee5 包也含 R09 `2/2`，但都不能确认外机所用源码/DLL/PDB。旧源或产物错配仍是推断，见[R09 复核](../design/reviews/ui-finesse-round3-20260915/evidence/CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md)。
- `95109c2` 的 run `38645600070` 上次观察为 In progress；本轮 API 连接被拒绝、`gh` CLI 不存在、Actions 浏览器读取超时，当前终态未确认。新阶段提交推送后要继续跟踪两条 CI lane 至明确终态；SDK8 本机结果不替代 SDK10.0.401。下一项 R10-04 快捷键帮助。


## 2026-10-15 当前恢复点：CLOSE-WRAP / SDK8 全量门禁

- 当前 main 基线为 b04660e0af2d17daae8c7dec426598e90de738b2。唯一 tracked 源改动是 WorkspaceTreeGovernanceTests 中 Trainer/Media 的旧 typed getter 断言，已更新为通用 workspace registry 路由；Production PageHost、缓存导航和焦点路由由行为测试覆盖，定向 8/8。没有生产 UI 改动。
- 隔离 SDK 8.0.423 / VSTest 17.11.1：完整 Release solution build 0 warning / 0 error、XAML 24/24；Core 125/125，Worker 356 passed / 1 skipped，Playnite source 455 passed / 27 skipped。WPF 122 类 648 passed / 39 skipped / 1 failed；Q14 654 DIP 行距当前 1/1。详见 CLOSE-WRAP 当前 main 证据包。
- 唯一失败是 R05TooltipTimingBehaviorTests.QuickMousePassesCancelTooltipAndAStationaryDwellOpensIt 在窗口显示前调用 GetCursorPos 返回 false；该类其余 2/2 通过。全体合计 1584 passed / 67 skipped / 1 failed；本机门禁未全绿。系统输入限制保持未验，不伪装为通过。
- 新提供的 #962/#963/#965/#967/#970/#971 ZIP 与既有归档 SHA/提交/SDK 记录相符；其中旧测试修正有单独证据，不替代当前提交 Actions。推送后分别追踪 SDK8 与 SDK10.0.401 到终态。下一步重查当前 main 的 R00/R01 freshness，再推进 CLOSE-OPTICAL-01。真实 Playnite、物理指针/DPI/跨屏、呈现帧、ETW/宿主性能仍未验。
