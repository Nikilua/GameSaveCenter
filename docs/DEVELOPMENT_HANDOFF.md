# GameSaveCenter 当前交接

更新：2026-10-14；分支 main。R10-06 来源提示既有实现已补实际生产页面/命令行为，SDK9.0.302 `3/3`；R09 焦点类当前 main `2/2`，外机 HEAD/DLL/PDB 未知。父提交 `a2fdb4f6` 对应 #992/run `38564273828` 已终态失败：SDK10.0.401 job `115852859775` 的“SDK 10.0.401 编译与测试复核”和默认 build job `115852860137` 的“编译、测试与打包”均 exit1。完整 logs 与两个 artifact API 返回403，匿名下载404；失败根因未知，不能按摘要猜修。artifact ID/SHA、当前本机测试身份与日志限制见 [R10-06 当前 main 复核包](design/reviews/ui-finesse-round3-20260915/evidence/R10-06-CURRENT-MAIN-RECHECK-20261014/README.md)。SDK9不替代SDK8.0.423/SDK10.0.401。

SDK9 本地结果不替代仓库 SDK8.0.423 或 CI SDK10.0.401。用户到 2026-10-15 09:00 前无法回复；#991/#992 完整日志仍受访问限制，没有日志时不猜根因，继续推进独立任务。每个阶段仍需中文提交、推送并跟踪对应 Actions 至终态。

本地下一项 R10-07：先核对侧栏折叠/展开和入口可达性现有证据，补当前 main 适用性，不重建已有能力。本轮 R10-06 本机门禁后提交推送并跟踪新 Actions 至成功或失败终态；失败优先取完整日志，访问受限时不猜修并继续依赖已满足的任务。外机 R09 HEAD/DLL/PDB 和 Q24-03 第二物理显示器仍待验。真实 Playnite、OS UIA/Narrator/IME、物理 DPI/跨屏、最终呈现、ETW/宿主性能仍待验。

## 2026-10-13 #965 CI 布局测试宿主修正

- 用户提供的 ZIP 哈希与 Actions artifact 一致。CI 实际 SDK `8.0.423`，构建 `0 warning / 0 error`；R08Numeric `5/5`。整次失败在两项 WPF 布局测试的 Light/Dark 实例（收件箱中心差 `42 DIP`；Settings 壳宽实测 `952 DIP`，测试却要求 `1320–1360 DIP`）。完整观察与摘要见上述证据包。
- 两项测试现以显式 Grid 逻辑宿主设置实际测量尺寸，保留原有行为与负例；本机隔离 SDK8.0.423 下 Release solution `0/0`、布局类 `54/54`。真实 host render、窗口物理面积、物理 DPI/跨屏未验。
- 下一步：提交此阶段（仅测试、证据、当前事实文档），推送 main，追踪对应 Actions 到成功或失败终态；随后先重跑 freshness 并复核 R00/R01。若 CI 再失败，先读完整日志再改。

## 2026-10-13 R00-03 / R00-04 当前 main freshness 复核

- Release SDK `8.0.423` 构建 `0 warning / 0 error`、XAML `24/24`，Playnite `net462`、测试 `net472`；定向动画、overlay、Dashboard焦点与2000项搜索行为共 `25/25`。搜索30/30不同查询改变结果集，p50/p95/max `46/63/75 ms`，超时负例通过。TRX、原始查询与freshness报告见[R00-03/R00-04复核包](design/reviews/ui-finesse-round3-20260915/evidence/R00-03-04-CURRENT-MAIN-RECHECK-20261013/README.md)。
- R00-03命中源变化为Dashboard测试构造/override seam（默认生产路径仍使用设置值）；R00-04命中源变化仅是Shell XAML Tooltip BetweenShowDelay。R01及其余12项没有命中本轮路径，本次未重跑。baseline freshness `14/14`；package identity未提供。
- Actions #953 docs-only run 最近仍在运行；#952代码run exit1但日志/根因不可读，不据此盲改。下一项先审查R06-05现有能力与完成条件。真实Playnite、OS UIA/Narrator/IME、物理DPI/多屏、最终呈现帧、ETW和宿主性能未验。
## 2026-10-13 R06-04 当前 main 复制快捷键行为复核

- 复用 `afe4aa55` 已实现的共享 `DataGridClipboardBehavior`、五类 profile 和凭据脱敏。本轮以 main `e277a804bd4774cd32ad0bde0dfe2b8f76bc3308` 为源码基线，仅添加 modifiers 测试 seam 与一项 WPF routed-event 行为测试。
- 本机隔离 SDK `8.0.423`；官方 Release build `0 warning / 0 error`、XAML `24/24`，Playnite `net462`、测试 `net472`。`R06ClipboardBehaviorTests 5/5`、`R22CopyFeedbackBehaviorTests 2/2`，0 失败/跳过；`python scripts/validate-source.py` 与 `git diff --check` 通过。
- 对实际生产 DataGrid 触发 `PreviewKeyDown` routed event，验证 Ctrl+C 复制选中行、Ctrl+Shift+C 复制当前单元格，并测试 Ctrl+Alt+C、Ctrl+X、解绑后 Ctrl+C 与无选择负例。既有测试继续覆盖显示顺序、稳定 TSV、稳定 ID 去重、完整原值和凭据脱敏。详情、源码/DLL SHA/MVID 与脱敏 TRX 见[R06-04 当前 main 复核包](design/reviews/ui-finesse-round3-20260915/evidence/R06-04-CURRENT-MAIN-RECHECK-20261013/README.md)。
- modifiers 经内部测试 seam 注入，clipboard 使用 fake setter；受控 WPF 路由不代表真实 OS 键盘、系统剪贴板或 Playnite 输入。未改布局、主题、DTO 或业务动作；历史双主题 RenderHarness 357 PNG 仍绑定 `afe4aa55`。真实 OS 剪贴板占用、宿主快捷键冲突、IME、UIA/Narrator、物理 DPI/跨屏和宿主性能仍待验。
- #951（`38306094571`，commit `2f546d06`）完整 log SHA-256 `96BCAC239C2E6BB25DA54E725319FA9AB6EC6964C3662D2C4AFAAB0A266406DE`，CI SDK `8.0.423`、Windows `10.0.26100`；Q14 类通过，后续 R05 Tooltip dwell 测试 expected1/actual0。已定位本机旧夹具会出现“系统指针到位但 WPF 未命中”；新测试夹具置顶/激活并有界等待实际 hover，本机连续两轮 `2/2`。当前测试修改与脱敏证据待 commit/push，下一 run 需跟到终态；成功后先按用户要求复核 R00/R01，再继续 Q/R。

## 2026-10-13 R06-03 当前 main 选中/焦点行为复核

- 复核基线为 main `fdeeec8a9d3a7742a63ccbee21c55a50752326c8`，复用共享 `DataGridRow`、`ListBoxItem`、现有 DTO 与绑定。本轮只增加一项生产 Task 页面行为测试；未改生产资源、命令或 DTO。
- 本机隔离 SDK `8.0.423`，官方 `scripts/build.ps1 -Configuration Release -SkipTests` Release `0 warning / 0 error`、XAML `24/24`，Playnite `net462`、测试 `net472`。R06Selection `2/2`；四个生产表格 Light/Dark 选中/焦点/禁用几何 `1/1`；Task Light/Dark hover 进出、错误徽章命中与取消命令 UIA Invoke `1/1`；共享 DataGrid/ListBox 资源契约 `2/2`。
- 新行为探针使用 fake Cancel 命令与合成 Failed/Running Task DTO；通过受控 `MouseDevice.ChangeMouseOver` 产生 WPF hover/leave 状态，检查生产 RowBackground 颜色变化；selected Failed row 实际测 `InputHitTest` 仍落到状态徽章，并确认终态取消按钮禁用；选中 Running row 后用 AutomationPeer Invoke 触发真实 Button Click 与 fake command。没有用源码断言代替状态/事件行为。
- 测试源码 SHA-256 `663FF65E6EDF569D05D51D002C45C74D4D125AA2F281617F02BD8081AA84B3FC`；插件 DLL SHA/MVID `A486B9B34CFB5614E24797F39E2D09CEBFAE51E251299F0F4C5EC8A67AA1BF50` / `354dc807-4006-4a7d-b9ef-59dc034f3420`；测试 DLL SHA/MVID `CB3F3FB068EDAF7E880F146AF892769D4384448F34AEB0DA1AB62FD3AAC0B148` / `d3eadb88-2bc6-470c-8ab5-6d0053b8c5fd`。四份脱敏 TRX、SHA 和限制见[R06-03复核包](design/reviews/ui-finesse-round3-20260915/evidence/R06-03-CURRENT-MAIN-RECHECK-20261013/README.md)。
- WPF hover 是受控 MouseDevice 状态，不代表实际 OS 鼠标；一次 `SetCursorPos` 尝试未令测试行进入 `IsMouseOver`，未作为通过证据，原因未知。没有重跑 RenderHarness 或新截图；历史截图仍绑定 `d83c7378`。真实 Playnite 输入、UIA/Narrator、物理 DPI/跨屏、最终呈现帧、ETW/宿主性能未验。
- 后续 #947–#950 均已终止失败，其中 #949/#950 的完整日志确认 Q14 compact 断言 Expected False/Actual True；完整证据与本机修正见下一节 R06-04。用户指定 #919/#920 实际运行 SDK `10.0.401`，两项侧栏 transition 测试失败；当前 main 对应类本机 SDK8 `22/22`，尚无当前分支 SDK10 复核。

## 2026-10-13 R06-02 当前 main 排序行为复核

- R06-02 实现及测试源码当前与 clean-main `3d83d38f` 无差异；本轮复核身份为 `90454586d702f6a2b413bfcc386ac599424f8bc1`，无生产/测试源码改动。
- 隔离 SDK `8.0.423` Release rebuild `0 warning / 0 error`、XAML `24/24`，定向 `R06SortingBehaviorTests 7/7`。首次复用 `afade3cd` DLL 被源码身份保护拦下，随后从当前 HEAD 重建；失败身份试次不纳入通过数。排序/unknown/刷新/实际列头/失效视图负例、源/DLL SHA/MVID 和 TRX 见[R06-02复核包](design/reviews/ui-finesse-round3-20260915/evidence/R06-02-CURRENT-MAIN-RECHECK-20261013/README.md)。
- 既有双主题 RenderHarness 图绑定 `c577afc5`；当前只补当前源码行为证据，没有重跑渲染或将旧图冒充新截图。合成 DTO/隔离 CollectionView/STA；真实 Playnite、UIA/Narrator、物理 DPI/多屏、presented frame、ETW/宿主性能未验。
- Actions #947（`38295749414`，commit `90454586`）截至2026-10-12 19:24 UTC 仍为 In progress；job 页提示登录查看日志，实际 runner SDK未知。CI目标 SDK `10.0.401` 与本机 `8.0.423` 分开记录。此前 #946 failure 日志/artifact匿名HTTP403、artifact内容不可读，根因未知，不按摘要盲改。
- 当时下一项：R06-03 选中焦点区分（现已完成当前 main 行为复核）。

## 2026-10-13 R06-01 列宽用户记忆当前 main 复核

- 当前 main `afade3cd39eab31de97f204aa76e9dc590ba3083` 已包含 R06-01 实现；复用既有 `GameSaveCenterSettings.DataGridColumnWidths` 和 `DataGridColumnLayoutController`，本轮无生产代码或测试改动。
- 本机 SDK `8.0.423` 官方 Release rebuild `0 warning / 0 error`、XAML `24/24`；当前 `R06ColumnWidthPersistenceBehaviorTests 6/6`。第六条验证 Task Queue 稳定列键逐项匹配生产 DataGrid 列定义。当前 source/DLL SHA、TRX 和边界见[R06-01当前复核包](design/reviews/ui-finesse-round3-20260915/evidence/R06-01-CURRENT-MAIN-RECHECK-20261013/README.md)。
- 原双主题 RenderHarness/代表图仍绑定实现提交 `75a6e6d8`，本轮没有重跑渲染。测试仅用隔离 STA/合成 DataGrid/设置对象；不等价真实 Playnite 进程重启、用户拖拽、物理 DPI/跨屏、UIA/Narrator、presented frame、ETW或宿主性能。
- Actions #946 对 `afade3cd` 终态 failure；公共页面只给出“编译与测试”exit1和Node.js弃用warning，完整日志与artifact API匿名 HTTP403，artifact内容不可读。失败原因与CI SDK未知，不凭退出码推断。
- 下一项：R06-02 排序提示与稳定性；先检查当前 main 相关实现/测试身份，复用已有代码与历史证据。

## 2026-10-13 R05-07 Tooltip 时序当前 main 复核

- 发现根节点 `ToolTipService` 附加值不会自动应用到其子 owner；此外 `BetweenShowDelay=100` 允许刚显示过 tooltip 后快速移动到下一项时再次立即打开。复用并扩展现有 `GscToolTipBehavior`，通过继承 timing scope 对默认后代应用根级 `350/0`，保留 owner 显式值；壳层、设置页、Dashboard 根值统一 `350/0`。
- 当前工作树 Release build (SDK `8.0.423`) `0 warning / 0 error`、XAML `24/24`；Tooltip `2/2`、Popup lifecycle `1/1`、Popup boundary `2/2`、focus boundary `5/5`、source contracts `24/24`，总计 `34/34`。源码 validator、`git diff --check` 通过；WPF 静态审查 `0 errors/29 warnings/177 info`。长路径 Wrap、Esc 关闭保留焦点、实际鼠标快速扫过正负例、动态 owner 和显式时序覆盖见[R05-07证据包](design/reviews/ui-finesse-round3-20260915/evidence/R05-07-CURRENT-MAIN-RECHECK-20261013/README.md)。
- 用户指定 run #920（`36677346317`）现为 failure；提交 `afade3cd` 对应 run #946（`38294361061`）亦 failure，4m18s；公开步骤“编译与测试”exit1，artifact 239 KB / SHA-256 `083b3f72bcacb1fdd36c413517d075fe5f27a921d1fb79024ad13882e3cc2cf7`。匿名 run/log/artifact API HTTP403，完整日志和artifact内容不可读；失败命令、根因与实际 runner SDK未知，不凭退出码盲修。本机 SDK8不替代目标 CI SDK10.0.401。
- 没有启动真实 Playnite或加载完整 Dashboard。OS UIA/Narrator、物理 DPI/多屏边缘放置、呈现帧、ETW、宿主性能未验；全量 WPF 尝试中途停止在第19/114类，不计通过。下一项 R06-02 排序提示与稳定性，先核对当前 main 排序实现及测试。

## 2026-10-13 R05-04 当前 main 多选摘要复核

- 在 main 父基线 `903525becdb679e9a4aae7bb04e73c98c1cb25dc` 上复用已有生产多选摘要，没有改生产 UI/VM/命令/DTO。原清空测试没有实际挂接 `MediaInboxMode`，本次补强 fixture：生产清空处理器清理当前“已忽略”可见与隐藏 ID、保留“待归类”集合，ViewModel 与 ComboBox 模式保持原值。
- 固定本机 SDK `8.0.423` 下完整 Release solution build `0/0`，定向 `R05MultiSelectionSummaryBehaviorTests 3/3`；源校验与 diff check 通过。确切 TRX、源码与程序集身份见[R05-04 当前复核包](design/reviews/ui-finesse-round3-20260915/evidence/R05-04-CURRENT-MAIN-RECHECK-20261013/README.md)。没有生产视觉变化，未重跑 RenderHarness。
- #941 (`38279606113`, `1c39e909`) failure，8m31s；#942 (`38281199307`, `903525be`) failure，9m00s。job 页面显示登录后可看 logs；匿名 logs/artifacts API 403 rate limit。根因、失败步骤、实际 CI SDK 未知；不得按 exit code 猜修。本机 SDK8不替代用户要求单独跟踪的CI SDK10.0.401。新测试 fixture 与上述 CI 根因无关，未据其失败状态更改生产实现。
- 未运行真实 Playnite，也未访问真实媒体/存档/云端。OS UIA/Narrator/IME、物理DPI/多屏、呈现帧、ETW/宿主性能仍未验证。下一项 R05-05 当前 main 适用性核验。

## 2026-10-13 R05-05 当前 main 三态复选框适用性复核

- main `fbf504d2a7aabcd322c3c735959a31c7e1679fc9` 上，媒体批量仍由 Extended DataGrid + `SelectedItems` 表达；显式三态样例只存在于开发校对页。共享模板的 `IndeterminateMark` 和源字符串测试不是集合/Space/UIA 行为证据，R05-05 维持不适用。XAML `24/24`、SDK8 Release build `0/0`、源契约 `24/24`；脱敏 TRX 与源文件 SHA 见[R05-05 复核包](design/reviews/ui-finesse-round3-20260915/evidence/R05-05-CURRENT-MAIN-RECHECK-20261013/README.md)。
- #943 终态 failure；摘要标明“编译与测试”exit1，run `38284177732`、job `114928442567`。artifact `11762189156` 477 KB，SHA-256 `bb2ffc75a4f111adffa061f46440af303131d14bfd6b52f2d6642b6b9a76176d`。页面提示登录查看完整日志；匿名 logs/artifacts API HTTP403，CI SDK和根因仍未知，不凭摘要猜修。用户要求跟踪的CI SDK10.0.401与本机8.0.423分开记录。
- 没有启动真实 Playnite、OS Space 输入或触碰真实数据；UIA/Narrator、物理 DPI/跨屏、presented frame、ETW、宿主性能仍待验。下一项 R05-06 当前 main 开关模型、视觉、持久化和回滚行为复核。

## 2026-10-13 R05-06 当前 main 开关保存语义复核

- main `0fcb44f851257504d867401dab92a74b3807899a` 已有完整设置值/显示/保存/回滚实现；本轮复用现有 DTO、ToggleSwitch、Portable JSON 与 R04-08 反馈状态机，只补 `R05TogglePersistenceBehaviorTests` 同值重复赋值负例，不改生产实现。
- 本机隔离 SDK `8.0.423` 官方 Release build `0/0`、XAML `24/24`；开关、保存反馈、草稿生命周期与源契约定向 `28/28`。实际覆盖和源码/程序集/TRX SHA 见[R05-06 当前 main 复核包](design/reviews/ui-finesse-round3-20260915/evidence/R05-06-CURRENT-MAIN-RECHECK-20261013/README.md)。
- #944（run `38286603033`，commit `0fcb44f8`）failure，7m22s；公开页面 annotation 只有 job exit1，需登录看完整日志，匿名日志 API HTTP403。失败命令/断言、实际 CI SDK和根因未证实；页面 artifact 摘要可见，但内容不可读。不凭摘要盲改，本机 SDK8 不替代待核 CI SDK10.0.401。
- 合成设置/隔离路径；没有真实 Playnite、原生 MessageBox、Worker 故障注入、OS UIA/Narrator/IME、物理 DPI/多屏、presented frame、ETW或宿主性能证据。下一项 R05-07 Tooltip 时序当前 main 行为复核。

## 2026-10-13 R05-01/02 当前 main 提交后复核与 Actions 检查点

- 推送的 main commit 为 `1c39e909856faa736b32224ee196ef0bac6ca460`。隔离 SDK `8.0.423` 下 Release 测试构建插件 `net462` 与测试 `net472`；`R05FocusBoundaryBehaviorTests`、`R05OptionVirtualizationBehaviorTests`、`R21FocusVisualRegressionBehaviorTests` 合计 `11/11`，VSTest exit0。TRX SHA-256 `DE45C3F9462F44665BDCD2F068E00630377C413D8669B54F4944F3E988312554`；测试 DLL SHA/MVID `9A509F12BBA20BD826ECC3D900DFD21752F0AAA498B9E10A7BE1CF683C52F063` / `8c8895de-7c96-4fe3-bdcb-fc0455c400e4`，插件 DLL SHA/MVID `8EB2BCE95E6959F3F4F794B26E1CE565CD59ADEF86F4A5B4EEB29133615E7F33` / `0477b734-38ce-4897-8ea4-6bb904b4d2ec`；测试宿主 .NET Framework `4.8.9181.0`。
- Dashboard 行为证据由最小 STA WPF fixture 调用生产确认处理器并通过 routed Click 走生产取消处理器；覆盖焦点进入、Tab/Shift+Tab 环绕、减少动效即时关闭、240ms 动画关闭期间焦点仍留在 overlay、取消后回到合成触发器。XML 检查另核对生产 XAML 的 Cancel/Confirm Click 接线。fixture 没有加载完整 Dashboard XAML 或 Playnite 宿主。
- `python scripts/validate-source.py` 通过，`git diff --check` 通过（仍打印测试源码 CRLF 提示）。进程退出时输出三条 WPF TextServicesHost/TextStore `InvalidComObjectException` 清理异常；测试为 `11/11`、exit0，异常根因未知。用 PowerShell 清理本轮试验 TRX 的命令被执行策略拒绝，相关文件保留在 evidence 本地目录，未尝试其他删除路径。
- Actions #940 (`38274288024`, commit `746eaf6a`) 终态 failure，job `114895863386` 耗时10m30s；日志 API 返回403 `Must have admin rights to Repository.`，根因未知。#941 和 #942 当前都已失败，具体状态见本交接顶部 R05-04 段；当时实际 runner SDK均未由可读日志证实。用户跟踪 CI SDK `10.0.401`；本机 SDK `8.0.423` 分开记录。真实 Playnite、完整 Dashboard XAML 加载、IME/UIA/Narrator、物理 DPI/跨屏、翻转呈现帧与宿主性能仍待验。
- 用户补充的旧检查点 `77de7f450431d267026ce2169aa30525b18bf528` 对应 Actions `36677346317`（#920），现已终态 failure，7m04s 后“编译、测试与打包”exit1；job 页要求登录，完整日志下载 HTTP403，根因未知。此状态属于早于当前 main `746eaf6a` 的历史提交，不替代 #940 或本轮新 run。
- 用户要求优先取得的旧 run `36675309812`（#919，commit `a6b6dfc`）也已终态 failure，总耗时13m02s，“编译与测试”步骤exit1。公开 job 展开仅有加载失败/登录提示，完整日志下载 HTTP403；根因未知，没有日志前不基于 exit code 猜测修复。

## 2026-10-12 R04-03 本机复核与最新已推送 CI

- 最近已推送基线为 main `e952c414a3941491543ecf558f6f6ab2a68bbf6e`。R04-03 测试工作树基于该源码/测试基线并含本阶段关闭确认 test seam 与行为测试；插件 net462、测试 net472 定向构建及 `SettingsDraftLifecycleBehaviorTests + PortableSettingsTests + SettingsValidationSourceTests` 为 `12/12`。详细 TRX、程序集身份和范围见[R04-03 当前 main 补充证据](design/reviews/ui-finesse-round3-20260915/evidence/R04-03-CURRENT-MAIN-RECHECK-20261012/README.md)。未跑完整 Release/RenderHarness，也未修改 XAML/布局。
- 最新已推送 run `38270923668`（#939，commit `e952c414`，job `114884678634`）终态 failure：7m11s 后“编译与测试”exit 1；Node.js 20→24 弃用警告；artifact `GameSaveCenter-build-diagnostics` 478 KB、SHA-256 `660a09c2217195c1e1db44d96887d4f3c674c32f0c02bb3a0385e03dcafc29dc`。GitHub job 页面要求登录，run/job logs 与 artifact API 返回 HTTP403，未取得完整日志或根因，不凭退出码盲修。
- 本机隔离 SDK `8.0.423` 与用户任务补充要求跟踪的 CI SDK `10.0.401` 分开记录；#939 实际 SDK 尚未通过日志核实。仓库 workflow 通过 global.json 选择 `8.0.423`，与用户补充信息有待核差，本机结果不替代 CI。
- R04-03 的“否”路径通过实际 STA WPF `Window.Closing` 事件保留草稿并把键盘焦点恢复到编辑框；“是”路径才调用现有 `CancelEdit` 并关闭。test seam 默认仍走原生 MessageBox。真实 Playnite 原生确认框、保存/Worker 应用与宿主关闭时序仍待验。R04-04 至 R04-08 已有账本证据，下一项 R05-01 当前 main 复核。
- 使用合成设置、fake Worker 和隔离目录；未碰真实存档/媒体/云端。Narrator/UIA、物理 DPI/跨屏、最终 presented frame、ETW 与宿主性能仍未验证。

## 2026-10-12 早期检查点（前序记录）

- R02-08 当前 main 行为测试 `4/4`，source TRX、Light/Dark Save 动作文案截图及 CloudQueue 页面上下文图已归档；RenderHarness `372` 图 `render-qa OK`。插件文案行为满足，真实 Playnite 与云端/恢复操作未执行。[证据包](design/reviews/ui-finesse-round3-20260915/evidence/R02-08-CURRENT-MAIN-RECHECK-20261012/README.md)
- 本轮 build diagnostics 及 `D:\GameSaveCenterTools` 下 build/render 输出已请求按精确路径清理，但执行器拒绝 PowerShell `Remove-Item`；它们仍留在本机且未纳入提交，没有尝试其他删除路径。
- picker Dispatcher 修复提交 `f13b9e05de0cf502ac61e97b0fbad8433b339b3f` 已推送 `main`。无 `SynchronizationContext` 的 STA 旧实现线程负例失败；修复后的 R05 picker 类 `5/5`。前一候选完整 Release/WPF 脚本在本机 SDK `8.0.423` 下 exit `0`（XAML24/24、0 warning/error、合计1550 passed/67 skipped/0 failed），其构建诊断来自 e12119b 加三项 dirty 源码/测试；不能称为 clean post-commit 全量身份。
- 本次对确切提交 f13b9e05 的 R00/R01 定向复核 `40/40`，RenderHarness 0 warning/error；媒体几何与工具栏负例探针均通过，证据身份、SHA/MVID、原始30搜索样本见[R00/R01复核包](design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-CURRENT-RECHECK-20261012/README.md)。freshness 已校正为14 fresh/0 stale；包未构建，package identity 未提供。
- 当前 Actions run `38222818374`（run930）已终态 failure，失败步骤“编译与测试”。CI workflow SDK10.0.401与本机 SDK8.0.423分开记录。下载完整日志API返回403 `Must have admin rights to Repository.`；artifact列表可读但下载返回401，根因未知。不得按退出码猜测或盲改；需取得有权限的完整日志后复现。
- 文档证据提交 `da6388e814309825141fb530ecda702cb7e11eab` 对应 run `38226955461`（run931）已终态 failure，失败步骤仍为“编译与测试”，SDK10.0.401。完整日志 HTTP403；diagnostic artifact ID `11735869371`（488,867 bytes）下载 HTTP401。根因未知，尚无失败命令/断言/堆栈，不凭 exit code 盲改。
- R02-03 复验已提交 `a2144083`；随后当前 main `a21440834865551cdb6894830548af51ad1d8e78` 的 Release 构建 SDK8.0.423 `0/0`、XAML24/24，R02-04 STA 几何测试 `7/7`，16/20 DIP 图标间距各8 DIP、中心差0；双主题 RenderHarness `372` 图 `render-qa OK`，Save 1040×700 两张截图已核对并归档。`validate-source.py` 通过，WPF静态审查36 XAML `0 errors/29 warnings/177 info`。该复验未改生产 UI。
- `a2144083` 对应 Actions run `38244963620` 已失败于“编译与测试”，页面仅显示 `Process completed with exit code 1`。CI SDK10.0.401；未认证 API rate limit HTTP403，GitHub job 页完整日志要求登录，diagnostic artifact `11745496487`（478 KB，SHA-256 `11de81935473ce3da60739d481c768eba2af589d2c76cc819cf3977fd4daea1e`）下载路由404。根因未知，不按退出码修复。
- `90099a09` 对应最新 Actions run `38247748056` 也终态 failure 于“编译与测试”，页面只有 `Process completed with exit code 1`。CI SDK10.0.401；未认证 Jobs/log API rate-limit HTTP403；diagnostic artifact `11747310690`（479 KB，SHA-256 `8586bcace066ca779c72dbf9fbea1843c19a49133d619b9a6000e6e8dc6e79c5`）下载路由404。失败命令、断言、堆栈未知，不按退出码猜修。
- docs/evidence commit `3df542af` 的 Actions run `38249983091`（#934）已终态 failure 于“编译与测试”；SDK安装、结构、诊断自测及SDK基线步骤成功，渲染/静态审查/打包跳过。CI SDK10.0.401；日志endpoint HTTP403；artifact `11747469284`（488,428 bytes，SHA-256 `662aba958e53e7a6d8ccd837a319fd93e8df91d2309f7977b0af1a84935f79fa`）下载HTTP401，根因未知。[run #934](https://github.com/Nikilua/GameSaveCenter/actions/runs/38249983091)
- main `4d166f017dcba8fbb07980ec21b690267b9ff377` 对应 run `38253621689`（#935，job `114825925174`）已终态 failure，耗时9m39s，失败步骤“编译与测试”exit1。CI SDK10.0.401；job logs HTTP403，artifact `11750360097`（478 KB，SHA-256 `805b8eff2797816cf4b8f0fded68de024da80f3dc585993c02373be29e8ef46c`）下载HTTP401。失败命令/断言/堆栈不可读，根因未知，不盲改。[run #935](https://github.com/Nikilua/GameSaveCenter/actions/runs/38253621689)
- 当前文档/evidence提交 `8e71feb41aeb20738aa80f0cbb5d05d0c6aeb0af` 对应 run `38262296773`（#936，job `114855606547`）终态 failure，耗时6m19s，“编译与测试”exit1。CI SDK10.0.401；logs API403 rate limit，job页要求登录；artifact `11752913387`、476 KB、SHA-256 `d78fb86cee5d81520fa81cebe65434cded562c18bd6fda46af0a91b8081bb021`，匿名suite路由404。根因未知，不能盲改。[run #936](https://github.com/Nikilua/GameSaveCenter/actions/runs/38262296773)
- 安装前本机只发现SDK9.0.302；仓库要求SDK8.0.423且 `rollForward=disable`。对 `3df542af` 的本机构建在sdk-baseline预检停止，未编译/测试；`validate-source.py`通过。2026-10-12后由官方 `dotnet-install.ps1` 将 SDK8.0.423 安装到隔离目录，临时前置进程PATH，系统PATH未改；clean main `4d166f01` Release 全量 `1560 passed/57 skipped/0 failed`。这不替代CI SDK10。
- R02-05 当前 main `90099a09104677708a5829cf7e6a5d22b6b414ff` 已重建 Release `0/0`、XAML24/24；生产资源 STA 几何命中测试 `2/2`，包含6 DIP间距负例、中心命中独立按钮及82 DIP换行；双主题RenderHarness `372` 图 `render-qa OK`，已归档Media 1040×700浅/深截图。源码门禁通过，WPF静态审查36 XAML `0 errors/29 warnings/177 info`。没有生产UI变更。
- R02-08 当前 main `4d166f01` 文案行为 `4/4`，全量 Release `1560/57/0`、双主题 RenderHarness `372` 图通过，TRX/截图/身份见当前复验包。R03-01 当前 main `8e71feb4` 的 GlyphRun 行为证据和 `TypographyDiagnosticsTests 11/11` 已补；探针/测试程序集身份及旧 DLL 负例见复核包。下一项 R04-01 当前 main 行为类 `6/6` 已复跑，证据待同步。R02-06 仅在 ENV-001 前置变化后重跑 fail-closed 宿主预检；真实 Windows IME 候选 UI/Playnite 输入链仍未验；Q06/R08用户原始失败日志与DLL identity仍缺。
- `.tmp/r02-03-current-recheck-20261012`、`.tmp/r02-04-current-main-20261012`、`.tmp/r02-05-current-main-20261012` 和 `artifacts/build-diagnostics` 下本轮诊断目录均已确认是仓库临时输出，所需证据已复制至 docs；递归清理请求被执行器拒绝，暂留本机。最新SDK预检诊断目录为 `artifacts/build-diagnostics/20261012T123814433Z-853a4720`，未提交。SDK下载中断后留下的空工具目标 `D:\GameSaveCenterTools\dotnet-sdk-8.0.423`，其精确非递归清理也被执行器拒绝。没有改走其他删除路径。
- Actions 权限受限期间继续不依赖该日志的独立 Q/R 小批次，不把 CI 标绿。CLOSE-HOST-01 仍受 ENV-001 限制；Q06/R08 用户原始失败日志与 DLL identity 仍缺。真实 Playnite、物理 DPI、IME/UIA、ETW 和最终呈现帧均未验。

## 2026-10-07共享UI阶段检查点（历史记录）

输入错误覆盖描边、实际Arrange行距和共享复合按钮对齐已修复；联合专项64/64、焦点可见获焦/紧凑负例4/4、完整render/source/XAML通过，Core125/Worker357/source470 passed/18原有skip。前两轮全量R02/R05失败、旧DLL/同candidate对照及干预均保留，未放宽原断言。

第三轮完整入口前36类WPF含Q14/R02/R05焦点通过，第37类R05OptionVirtualizationBehaviorTests删除回退期望Game 0000、实际Game 0020，2 passed/1 failed，后77类未执行。旧clean c35与同candidate独立均3/3；新增CLOSE-PICKER-01，先受控核对排序/20ms防抖刷新线程及集合重建，尚不归因具体修改或改断言。UI阶段源码/证据单独commit/push，完整门禁和任务仍IN_PROGRESS；下一阶段构建必须使用提交后的真实HEAD，不能把c35 dirty DLL冒充新提交。

c35 runner两项自测/SDK8.0.423已实际验证，失败artifact已取得。独立render-qa未注入env时MSBuild身份unknown已实际查询，当前render显式注入c35并核对DLL；后续修脚本注入/恢复。共享模板另命中R00-01-02/R00-05，共10项补证，baseline不改。证据：[共享UI阶段](ai/evidence/close-wrap-20261007/README.md)。
## 本轮完成

- global.json与CI固定SDK8.0.423/disable，C#12.0，构建和独立runner验证实际选择；PS5.1/7真实缺失旧SDK失败、原latestMajor对照返回8.0.423通过。Release0/0、Core125/Worker357/source470通过；完整WPF、runner与包门禁仍须按最新终态签收，SDK/CI任务不关闭。
- run37565955357/37566483862已取得终态与always上传产物；SDK10侧栏22/22，默认偏好ClientAreaAnimation=false；实际Q14错误态36→37 DIP失败。STA测量确认错误边框参与布局，独立双主题654 DIP边距失败和620/660正例均已取得。
- 补修诊断器多个PATH同名程序拼接导致启动失败；两个不同原生程序夹具在PS5.1/7验证首路径优先级，finally恢复PATH。前后诊断保留；无业务/UI修复或版本变化。

## 前阶段已记录事实（历史）

- CI诊断已接入实际native退出码、独立console/TRX、SDK/源码hash/DLL身份及always上传；PS5.1/7受控失败、stderr、缺失程序和脱敏自测通过。生产默认系统动效策略保留，实例内测试输入覆盖8组合；侧栏22/22，source470/18，Core125/125。
- 本轮WPF前23类通过，第24类Q14先在错误态高度36→37 DIP失败，后89类未执行；没有运行到原654 DIP边距断言。CI恢复仍未验，不关闭CLOSE-CI-01。新增受影响R00-03/R01-01/R01-04须跟原5项一并补证。
- CLOSE-IPC-01已完成：独立管道、握手与失败清理先排除夹具干扰，再受控复现生产取消dispose的IOException/EOF竞争；修复取消分类，保留RequestId、有限重放和未知提交语义。
- 同一最终DLL专项20轮200/200；source470通过/18原有跳过/零失败，Core125/Worker357通过，SDK8 Release 0 warning/error、XAML24/24。源码/DLL SHA、MVID、复现和负例见 [本轮证据](ai/evidence/close-ipc-20261005/README.md)。提交前dirty构建身份明确分账。
- 全量WPF在第24类Q14失败：654 DIP单行预设控件残留20 DIP bottom margin，同DLL独立复核再失败。前23类通过，后89类未执行；新增CLOSE-WRAP-01，整体门禁不通过。
- 取得最新CI run37329446647完整失败步骤：实际SDK10.0.401，两条ProductionShellChromeSourceTests活动动画断言失败，与IPC分开；旧run日志403仍是历史事实。

## 下一项如何开始

1. 保持 `main`；保留其他任务的 R04/R05/R07 未跟踪 TRX 和证据，不把它们并入本阶段。
2. R08 fixture 修正已经 push；#960 run `38327521241` 最新公开页面显示 `In progress`，与之前终态失败记录冲突，日志与 CI SDK不可见。R00/R01 校正 #954 的完整日志已归档，确认失败在旧 R07 初始 `IsVisible` 断言，未记录实际几何值；#954 不指向 R00/R01 问题。用户指定的 #919/#920 SDK `10.0.401` 日志已在证据包归档并核验 SHA，绑定旧源码。
3. R06-05 当前 main 本机复核已完成：Release `0/0`、XAML `24/24`、行为 `2/2`，当前源码长表头换行、双向排序和四页双主题代表图见证据包；其 #961 页面仍为 `In progress`，不能收口。
4. R06-06 当前 main 本机复核为 Release `0/0`、XAML `24/24`、逐类隔离行为 `43/43`，详见本轮复核包。证据/账本文档已准备提交；推送后跟踪新 Actions 到终态。随后复核已实现的 R06-07，不重建已有功能。
5. 真实 Playnite、OS UIA/IME、物理 DPI/跨屏、presented frame、ETW/宿主性能仍独立待验；R05-07/R06-04 需当前远端门禁覆盖，R02-06 仍需 ENV-001 条件解除。
6. 本机临时输出尚未清理：已确认绝对路径均在仓库的 `.tmp/` 或 `artifacts/build-diagnostics/`，被 `.gitignore` 忽略且无已跟踪文件；执行器安全策略拒绝 `Remove-Item -LiteralPath -Recurse -Force`，没有改用其他删除方式。以下四个生成目录仍存在、不可提交：`.tmp/r06-05-main-20261013`、`.tmp/r06-05-render-current-20261013`、`artifacts/build-diagnostics/20261013T005136220Z-e660c089`、`.tmp/r08-postcommit-d60e472d`。

## 验证边界

本轮没有启动真实Playnite、改变Windows动画偏好、安装到用户Extensions或触及真实存档/媒体/云端；没有验证物理DPI/读屏/最终呈现帧。SDK8本机与SDK10 CI分别记录。测试期间只终止已核对命令行归属本轮隔离目录的测试进程；被终止的运行不记为通过。

每独立阶段编译、测试、同步文档并commit/push当前main。历史d57a613f包的clean构建/IPC/结构及六个程序集身份已记录，未安装；不能签收当前源码。当前freshness6/8，baseline未改，完整SDK/CI门禁仍受Q14阻塞。本地包不冒称稳定发布，当前阶段中间产物归档后按仓库规则清理。

## 2026-10-13 当前恢复点：R06-07

- 当前分支 `main`，当前基线 `87d27592086bbef355b9426a00614b5bb9856e8c`。R06-07 源/测试与 `73d0f6e6` 完全相同；本机 SDK `8.0.423` 用同源 Release/net472 输出隔离执行当前行为用例 `16/16`，详情及 TRX SHA 在 [R06-07 当前 main 证据包](design/reviews/ui-finesse-round3-20260915/evidence/R06-07-CURRENT-MAIN-RECHECK-20261013/README.md)。这是行为/结构复核，不包含新截图。
- 对应 Actions #962 / run `38335989087` 已 Failure。用户提供诊断包确认 Release build 通过，CI SDK `8.0.423`、VSTest `17.11.1`；失败在无关的 `R08NumericChangeBehaviorTests` 反馈计数断言（0/期望1），详见 [R08-06 CI 门控复核](design/reviews/ui-finesse-round3-20260915/evidence/R08-06-CURRENT-MAIN-RECHECK-20261013/README.md)。
- 当前 R06-07 本机行为结论不变；CI 失败原因已定位到未固定系统动效输入的 R08 测试夹具，不归因 R06-07。原 Demo `DesignShellView.xaml`/`Pages` 路径缺失，沿用恢复生产基线。旧 `.tmp/r06-07-emptytables` 不可用，旧报告绑定 `5046bf8f`，不作为当前截图。真实 Playnite、UIA/读屏、OS 输入/IME、物理 DPI/跨屏、presented frame、ETW/宿主性能未验。
- R06-08 已按当前 main 完成本机 14/14 行为复核。下一步先提交/推送 R08-06 的系统动效测试输入修正并等新 Actions 终态，再按当前续跑顺序复核 R00/R01 问题与证据，之后推进依赖已满足的 Q/R。保留其他任务 R04/R05/R07 未跟踪 TRX。
- 磁盘清理：四个旧生成目录仍在仓库 `.tmp/` 或 `artifacts/build-diagnostics/` 且被忽略；PowerShell 精确递归删除被安全策略拒绝，未尝试改走其他删除接口。不要提交生成物。新增本阶段证据仅是三份脱敏 TRX 与 README。

## 2026-10-13 当前恢复点：R06-08

- 最新代码阶段在 main `dc8f26ee` 基线完成；R06-08 只改行为夹具，SDK8.0.423 Release `0 warning / 0 error`、XAML `24/24`，定向 WPF/geometry/source suites `14/14`。长诊断展开后 10 条合成任务中至少 4 行完整可见；用户选中项、详情内容和独立详情滚动保持有效。测试源 hash、DLL SHA/MVID 与脱敏 TRX 见 [当前 main 证据包](design/reviews/ui-finesse-round3-20260915/evidence/R06-08-CURRENT-MAIN-RECHECK-20261013/README.md)。本轮没有 production UI 改动或新渲染矩阵。
- #963/run `38338090417` 同样在 `R08NumericChangeBehaviorTests` 反馈计数断言失败；用户提供的完整诊断包和 #962 的 CI SDK均为 `8.0.423`。#964/run `38339686245`（commit `83fbee97`，job `115107636301`）终态失败于“编译与测试”，公开匿名 artifact partial 404，未取得 #964 自身完整日志；修正尚未推送。
- R06-07 与 R06-08 本机行为条件已复核；R06-08 当前无截图证据。原 Demo 目录缺失，沿用恢复生产基线。真实 Playnite/Worker、UIA/读屏、OS 输入/IME、物理 DPI/跨屏、presented frame、ETW/宿主性能和真实服务失败时序未验。
- 推送 R08-06 修正后，先等对应 Actions 终态；随后按当前续跑顺序先处理 R00/R01 问题与证据校正，再推进依赖已满足的 Q/R 任务。保留其他任务未跟踪 R04/R05/R07 证据。
- 清理：本阶段沿用 `.tmp/r06-05-main-20261013` 构建输出以避免新建大型输出；已生成证据仅四份脱敏 TRX 与 README。旧 `.tmp`/`artifacts` 目录的递归删除仍被安全执行器拒绝，保持原状，不绕过。

## 2026-10-13 当前恢复点：R08-06 CI 动效门控修正

- main HEAD 仍为 `83fbee9712195afe454f6f36f07e130ecc236398`；本轮本机已用隔离 SDK `8.0.423` 完成 Release solution `0 warning / 0 error`、`R08NumericChangeBehaviorTests 5/5`、`validate-source.py` 和 diff check。当前未提交代码修改仅 `NumericChangeFeedback.cs` 与 `R08NumericChangeBehaviorTests.cs`；已新增当前证据包及同步文档，其他 R04/R05/R07 未跟踪文件仍保留。
- #962/#963 用户上传诊断包已核验：CI SDK `8.0.423`，两次 Release 构建成功，失败是 R08 数字反馈计数 `0`/期望`1`；此前 #959 日志证明同 runner `ClientAreaAnimation=False`。修正增加按 TextBlock 隔离的内部审计 provider，默认生产行为仍服从真实系统动效偏好；正例强制开、系统负例关、另验默认路径。
- #964/run `38339686245`（提交 `83fbee97`）在“编译与测试”失败，job `115107636301`，artifact partial 404；该提交早于当前修正。推送当前小批后继续查询新 Actions，直到成功或失败并按实际日志处理。完成 CI 门禁后先复核 R00/R01，再推进依赖已满足的 Q/R 小批。
- 未验真实 Playnite、UIA/读屏、OS 输入、物理 DPI/跨屏、最终呈现帧、ETW/宿主性能；未触及真实数据。清理仍受先前安全执行器拒绝限制，不绕过；未提交 `artifacts/`、`.tmp/` 或其他任务 TRX。

## 2026-10-13 当前恢复点：#968 成功与 R00/R01 校正

- 当前代码检查点 main `4aa1ec7ef9bafa95758027a9c9ea4f0c43be8c80`；#968/run `38366158855` 已终态成功。SDK8 job `115191682311` 实际 `8.0.423`，SDK10 job `115191682644` 实际 `10.0.401`。详细步骤、artifact 元数据与证据边界见 [R00/R01 复核包](design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-CURRENT-MAIN-RECHECK-20261013/README.md)。
- R00/R01 freshness summary 为 `14 fresh / 0 stale`；没有按类重跑；package identity `not-provided`。#967 SDK8 日志仍受匿名 HTTP403/404 限制，上传 ZIP 只代表绿色 SDK10 job；helper BOM 修正已在 `4aa1ec7`。
- 下一项 R05-03：从已授权隔离 Playnite的 fail-closed 预检开始，只有通过后才采集真实弹层翻转帧；如果预检阻塞，保存精确错误与后续手工步骤并转其他独立 Q/R。
- 真实 Playnite/Worker、package identity、物理 DPI/跨屏、OS 输入/UIA/IME、最终呈现帧、ETW 与宿主性能均未验。保留其他任务未跟踪 R04/R05/R07 TRX，不纳入提交；不下载或安装 86.1 MB workflow preview artifact，也不触碰真实存档/媒体/云端。

## 2026-10-13 当前恢复点：#969 文档门禁与 R05-03 bootstrap 阻塞

- HEAD `d56d23e` 对应 #969/run `38377673615` 已成功；最新代码阶段仍为 `4aa1ec7`，#968 的 SDK8/SDK10 jobs 都通过。R00/R01 `14 fresh / 0 stale` 未因文档同步变化。
- #967 ZIP 是 SDK10 job `115178885908`，不是 workflow 的 SDK8 失败 job。对应 R08-01 TRX 已提取并复核 `2/2`；用户机原始 `1/2` 故障仍未定位。
- R05-03 runner 在 Playnite 扩展安装前因隔离启动进程未优雅退出而停止。到 `2026-10-13T10:35:50Z`，PID `34512` 与 4 个 CefSharp 子进程仍使用 `.tmp/r05-03-popup-profile-20261013`。不得强杀、删除该 profile 或在进程活跃时重跑；没有产生 popup/呈现证据。详见 R05-03 README 与 `host-preflight-20261013.json`。
- #960/run `38327521241` 为 terminal failure、job `115069541561` exit 1；完整 log 404、API 403，SDK/根因未知。下一项 Round2 Q23-06：用 RenderHarness 在生产基线上检查目录编辑器长路径、只读检测、错误说明和 Browse 按钮布局，合成数据仅用隔离路径，不写真实目录；离屏结果不代表真实宿主/物理跨屏/最终呈现。

## 2026-10-13 Q23-06 当前 main 交接

- 代码提交 `9e84dba305a252fea116f1c5e918f7cf15f2c89f`：修复设置路径 ComboBox 选中项显示并加入双主题/窄窗合成路径探针。证据：[Q23-06 复核包](design/reviews/ui-finesse-round2-20260913/evidence/Q23-06-CURRENT-MAIN-RECHECK-20261013/README.md)。
- 本机 SDK `8.0.423`：RenderHarness Release `0/0`、probe `8/8`、Playnite 定向 `170 passed / 0 failed / 39 skipped`、source/XAML 检查通过、WPF 静态 `0 errors / 29 warnings / 177 info`、full Render QA `0 PROBLEM`。
- #967 用户 ZIP 对应 SDK10 `10.0.401` 的单 job 通过包；整次红灯为另一 SDK8 job 源码门禁，已由 `4aa1ec7` 修复且 #968 双 job 成功。这不替代本次 Q23 代码的后续 CI。
- #970/run `38386326043` 和 #971/run `38393317198` 的用户 ZIP 已补齐部分失败材料：#970 SDK10 取消测试竞态、#971 SDK8 Q14 fixture 类型错误；已仅修测试并在 `599d2fd` 本机分别全量 Worker `357/357` 与 Q14 `1/1` 通过。SDK8/SDK10 CI 终态和 #971 SDK10 根因仍待复核。保持其他任务 R04/R05/R07 未跟踪文件。
- Q23-08 本机复核详见下方交接段；代码 `c377ab2`、测试修正 `599d2fd`。代码与证据推送后检查新 Actions 两个 SDK lane 到终态。

## 2026-10-13 当前恢复点：Q23-08 底部动作栏

- 本轮改动：`GameSaveCenterSettingsView.xaml` 只加 Reset All 测试定位名；视图代码将已有 reset/import 确认转到共享方法，并用内部测试 delegate 代替测试里的原生模态框；新增 Reset All/导入确认实际行为回归和 `settingsactionsprobe`。业务重置目录、portable DTO、导入/导出文件流程、Playnite 保存/取消没有重建。
- 本机 SDK `8.0.423`；Playnite `net462`/tests `net472` Release `0 warning / 0 error`；`599d2fd` 精确身份下 Q23 行为 `12/12`；R00/R01 受影响类 `154 passed / 39 skipped / 0 failed`，XAML `24/24`、`validate-source.py`、WPF static `0 errors / 29 warnings / 177 info`、diff check通过。R00/R01 freshness `14/14 FRESH`，current audit `168 snapshots / 116 warnings / 13 MEDIUM / 0 HIGH / 0 failed routes`，E01–E20 `20/20`。证据见对应 Q23 与 R00/R01 README。
- 用户 #970/#971 ZIP 已复核并记录在[CI 诊断及本机复核](design/reviews/ui-finesse-round3-20260915/evidence/CI-RUN-970-971-TEST-RECHECK-20261013/README.md)：#970 SDK10 cancel test race、#971 SDK8 Q14 fixture 类型错误均是测试问题；`599d2fd` 的 SDK8 本机复跑通过。#971 SDK10 diagnostics 仍缺；公开匿名日志/API 无法确认 workflow 终态，推送后检查本次新 run。
- 未验证真实 Playnite host Save/Cancel footer、原生文件对话框/文件操作、物理 DPI/跨屏、OS UIA/IME、presented frame。Demo 原目录不可用，本阶段沿用恢复生产基线。证据中的合成窗口未执行操作；未修改真实配置或用户数据。
- 下一项可执行任务：Q24-01 分数 DPI 字形。先查现有能力与验收条件，再测有限逻辑 scale；物理 DPI/跨屏单列待验。其他任务 R04/R05/R07 未跟踪文件需保留。旧 `.tmp/` 清理命令先前被拒是历史事实；本轮只清理本轮自己生成且不被证据引用的临时目录。

## 2026-10-14 当前恢复点：Q24-01 受控 DPI 验证

- main 代码提交 `3ca41ec9a5a3f3c6888db72ba3a6254b046dcd40` 新增 `Q24FractionalDpiTypographyBehaviorTests`，没有生产代码或视觉改动。隔离 SDK `8.0.423`、Playnite `net462`、测试 `net472`，提交后精确身份定向结果 `1/1`；五个目标 DPI PNG、TRX、源码/程序集 hash 在[证据包](design/reviews/ui-finesse-round3-20260915/evidence/Q24-01-CURRENT-MAIN-LOGICAL-DPI-RECHECK-20261013/README.md)。
- 五档只由 RenderTargetBitmap 的目标 DPI 栅格化；源 WPF Visual 继承本机 150% (`DpiScale=1.5`)，系统 DPI API `144`，物理桌面只枚举 `DISPLAY1`。因此只更新 Q24-01 视觉为受控通过；真实物理 DPI/窗口迁移、Playnite/Popup 与最终呈现仍待验，Host external、最终未完成。
- #972/run `38421480496` 对前置 SHA `8546acab` 已终态成功，SDK8 `8.0.423` job `115379639786` 与 SDK10 `10.0.401` job `115379639572` 均通过。该 run 不含 Q24 代码 `3ca41ec9`；推送 Q24 本批后分别跟踪新 SDK8/SDK10 jobs 到终态，CI 未结束前不报告代码阶段完成。
- 下一可执行小批 Q24-02：用当前 main 生产 `PageHost` 记录短窗真实内容几何、首尾可达和恢复宽窗；不以窗口请求尺寸代替内容区。用户至明早 09:00 不可回复，独立任务继续推进，不发问题等待。
- 保持游戏选框/滚动条、Playnite `net462`、命令/取消/错误/恢复保护和大列表性能语义。其他任务 R04/R05/R07 未跟踪 evidence 原样保留；本轮生成的 PNG/TRX 均由 README 引用，未产生新 `.tmp` 目录。

## 2026-10-14 当前恢复点：Q24-05 焦点语义已满足

- 当前分支 main，代码与证据推送于 cb31947b1148195e2b8750dbfc75ae0269b1978a；对应 origin/main。Q24-02 的 5fcd6856/74815b2b 与 Q24-04 的 87070909 均已包含在此 HEAD。
- Q24-05 复用 b20f1a4b 以来的生产实现；当前 main 隔离 SDK 8.0.423 Release 构建 0 warning / 0 error，R21 picker、R09 focus outline、R23 navigation/settings tabs/production grids 共 7/7。证据、TRX 与程序集身份见 Q24-05 复核包。
- Q24-05 受控行为已满足：Current/Selected 与键盘焦点分离、失焦/禁用负例、双主题状态，以及游戏选择层滚动/切主题/关闭回焦。真实 Playnite、Windows UIA/读屏、OS 输入、焦点环最终像素与宿主呈现仍待验。
- #970/#971 新附件均为已有 ZIP 的相同 SHA；#971 SDK10 诊断仍缺，不等待用户补充。#973/run 38429051152 页面仍 In progress；#974/run 38436999880 的 SDK8/SDK10 job 页仍显示 Re-running jobs，日志要求登录，实际 SDK/终态未核实。新提交后继续跟旧/新 run 直到状态明确。
- 用户明日 09:00 前无法回复；继续独立任务。下一项 Q24-06 当前 main UIA Name/HelpText/LabeledBy/Pattern 复核；Q24-03 仍需第二物理屏。
- Q24-04 之前自己产生且未被证据引用的 TRX 仍在 Q24-02 证据 tests 目录；.tmp/q24-04-current-main-20261014 也曾登记为本轮临时目录。精确清理请求被执行策略拒绝，保留原状，不用其他删除接口。R04/R05/R07 等其它任务的未跟踪证据保持原样。
- C 盘隔离临时构建约 46 MB。按精确路径清理时执行策略拒绝 Remove-Item，文件仍留在 C:\Users\lopmatu\AppData\Local\Temp\gsc-q24-05-build-20261014；未尝试其他删除接口。新 TRX 已脱敏并由 README 引用，其他未跟踪证据保持原样。

## 2026-10-14 当前恢复点：R08-02

- 当前分支 main，本批基线 066b9a6c01057c044ddc0d7c63ff70ce3dd7a6b3。R08-01 仅新增生产按钮 hover/press 中途反向行为测试；本机 SDK9.0.302 Release 0/0，R08 3/3、Q06 1/1、source validation、XAML 24/24、diff check 通过。详细输入与采样证据在 [R08-01 当前 main 包](design/reviews/ui-finesse-round3-20260915/evidence/R08-01-CURRENT-MAIN-RECHECK-20261014/README.md)。
- R08-01 用户机 1/2 失败仍没有具体方法、断言、堆栈/DLL；当前行为通过不能代替该问题定位。下一项 R08-02：先检查运行中的热关闭已有服务/模板与正负行为覆盖，再决定是否只补行为证据；保留系统/应用取消与错误语义、Playnite/net462 及动画时钟释放。
- 用户报告另一台机器的 R09 失败为 Expected 2 / Actual 1。当前模板分离 Chrome 1 DIP 与 ValidationStroke 2 DIP；旧测试期望 Chrome 2 已在 f13b9e05 修正；本机当前 R09 2/2。未知外机 HEAD/PDB，因此把使用旧测试源/程序集或分支记为待确认而不是已证实；材料见 [R09 外机复核](design/reviews/ui-finesse-round3-20260915/evidence/CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md)。
- CI：#971/run 38393317198 公开状态仍 In progress（所附 ZIP 只有 SDK8 job）；#978/run 38509928015 Failure，完整日志登录限制、根因未知；#979 当前 queued。main 066b9a6 对应 run 尚未在公开列表显示。推送后逐一查 SDK8 与 SDK10.0.401 至终态；日志未取到时不盲改，继续独立 Q/R。
- 保留他任务未跟踪证据与 WPF 临时项目；只清理本轮 .tmp/r08-01-production-button-sdk9-20261014，且在 TRX/身份已复制和哈希核对后执行。真实 Playnite、物理鼠标/键盘、OS UIA/读屏/IME、物理 DPI/跨屏、presented frame、ETW/宿主性能仍未验。

## 2026-10-14 当前恢复点：R08-02 已推送，R08-03 当前 main 已复核

- 工作树在 main，当前提交 `31d64e42` 已推送。R08-02 本地 SDK `9.0.302` Release 构建 `0 warning / 0 error`；应用开关测试 `1/1`、真实 SettingsShell 系统参数处理回调的受控策略模拟 `1/1`、生产侧栏 reduced-motion `2/2`、回调订阅/卸载保护门禁 `1/1`。详细 TRX、source/assembly identity 和边界见 [R08-02 证据包](design/reviews/ui-finesse-round3-20260915/evidence/R08-02-CURRENT-MAIN-RECHECK-20261014/README.md)。
- 系统模拟使用实例级内部 provider；未修改 Windows 动画偏好，也未触发 `SystemParameters.StaticPropertyChanged`。真实设置通知待隔离宿主；UIA/读屏、Playnite、物理 DPI/呈现帧、ETW/宿主性能仍未验。R08-02 测试 exit 0 后仍有 TextServices COM shutdown 清理异常，根因未知。
- 外机 R09 最新日志的第 151 行对应历史 `3f7d0b30` 中 `Chrome.BorderThickness == 2`；当前 main 第 151 行断言错误边框颜色，第 154 行才断言 `ValidationStroke.BorderThickness == 2`。当前 main R09 `2/2`，日志中的 `libpng iCCP` 是独立 warning；没有外机 HEAD/DLL/PDB，不能具体归因旧分支、旧二进制或 PDB 错配。
- 公开 Actions 页面仍只列到 #979 (`ae3c565`) queued；#971/run `38393317198` In Progress，#978/run `38509928015` Failure 且完整日志需登录。`31d64e42` 对应 run 尚未在公开页出现。继续查 SDK8 与 SDK10.0.401 两条 lane 到终态；SDK9 结果不替代 CI。
- R08-03 已按当前 main 刷新行为证据并满足受控条件；下一项 R08-04 业务完成节奏，先核查 DTO 与现有取消/成功/失败时序再补缺口。保留他任务未跟踪证据与 WPF 临时项目；只清理本轮已验明归属的 `.tmp`。

## 2026-10-14 R08-03 当前 main 隐藏/最小化 spinner 复核

- 生产 `IndeterminateProgressBehavior` 与共享 ProgressBar 模板已存在；核对 `0243c108` 之后有共享主题/布局变化，故刷新实际行为测试而未复建服务。隐藏 Tab、窗口最小化各等待 `360ms` 时 `TranslateTransform.X` 保持三位小数不变，`IsIndeterminate` 一直 true；可见/还原均恢复移动。测试 `1/1`，脱敏 TRX 与源/程序集 SHA 见 [R08-03 当前 main 证据包](design/reviews/ui-finesse-round3-20260915/evidence/R08-03-CURRENT-MAIN-RECHECK-20261014/README.md)。
- 当前测试程序集 ProductVersion 是 `0.6.73+d3cb0b2d`（在提交前构建），源哈希与当前 `31d64e42` 一致。提交后新隔离 build 两次失败于 MSBuild ProjectReference 发现/目标阶段，exit1、无 warning/error 诊断；不将其声称为新 build pass。R08-02 代码阶段的隔离 Release build 对相同源码内容通过 `0/0`。当前 Actions 公共页只见 #979 queued、#971 In progress、#978 Failure（完整日志需登录），无当前提交 run 可核；CI 未收口。下一项 R08-04，保留真实 Playnite、Windows 动效通知、呈现帧/物理最小化、UIA/读屏、ETW 与宿主性能边界。

## 2026-10-14 当前恢复点：R08-04 本机验证完成，CI 待核

- checkout 在 `main`，阶段基线 `37bd3c97`。只新增/加强两个行为测试与对应证据/记忆，没有修改生产逻辑；完整 Release solution 在本机唯一 SDK `9.0.302` 下隔离构建 `0 warning / 0 error`。`R02BusyStateTests 5/5`、`R08BusinessFeedbackBehaviorTests 4/4`，共 `9/9`；源校验与 diff check 通过。构建和 TRX 对应文件 SHA/MVID 见 [R08-04 证据包](design/reviews/ui-finesse-round3-20260915/evidence/R08-04-CURRENT-MAIN-RECHECK-20261014/README.md)。global.json SDK `8.0.423` 未安装；SDK8/SDK10.0.401 CI 门禁尚未核实。
- WPF 静态检查 `0 errors/29 warnings/177 info`，本阶段没有 XAML 变化；检查扫到未改动的 `.tmp/r12-04-stage` 一条历史布局 warning。隔离 WPF 窗口/合成 DTO 不是 Playnite host、真实 Worker 长请求、Narrator/外部 UIA、自动通知宿主时序、呈现帧或宿主性能证明。
- 用户提供外机 R09 log 的期望/实际吻合历史 Chrome 2 DIP 断言；当前源码同一行是颜色检查，独立 `ValidationStroke` 才期望 2 DIP。`libpng iCCP` 不在失败调用栈，`TextServicesContext` COM 异常发生于失败后的 WPF 收尾；外机 HEAD/DLL/PDB 未提供，具体错配仍未证实，当前 main 未改样式/降低断言。
- 公共 Actions 页最新可见仍为 #979 queued；#971/run `38393317198` 页面仍 In progress；#978/run `38509928015` 为 Failure，完整 logs 需登录。没有 `gh` CLI；匿名浏览拿不到新提交 SDK8/SDK10 终态。用户至 2026-10-15 09:00 无法回复，不等新材料，继续独立任务并在有访问权限时再查 CI；不因 SDK9 本机通过称 CI 收口。
- 本轮生成的 `.tmp/r08-04-current-main-sdk9302` 在脱敏 TRX/程序集身份校验并归档后清理；只清理已确认归属本轮的 R08 `.tmp`，保留其他任务未跟踪证据与 WPF 临时项目。下一项 R08-05 页面切换轻量化；先复用现有页面 transition/绑定/滚动实现，分析布局与容器重建行为后再补小批行为验证。

## 2026-10-14 R09-05 外机失败当前 main 复核

- 当前 main 552b8168 的隔离 SDK 9.0.302 Release solution build 为 0 warning/0 error，R09FocusOutlineBehaviorTests 2/2。用户所附 SDK10.0.401 ZIP（commit 983fee5）与 SDK8.0.423 ZIP（commit c9f9179）内对应 R09 结果也为 2/2；commit 5a6647c 的另一 ZIP 不含 R09 TRX。
- 当前 test source SHA 为 A5AAB9E…B2CBC98；插件/测试 DLL SHA、MVID、ProductVersion 以及脱敏 TRX 已记录在 CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014。没有更改生产模板或降低断言。
- 外机日志第 151 行、Expected 2/Actual 1 对应历史 Chrome 2 DIP 断言；当前第 151 行是错误色、第 154 行读取 ValidationStroke 2 DIP。旧测试程序集/分支或 PDB 与源码错配是推断，缺少外机 HEAD/DLL/PDB 身份，不能确认。libpng iCCP 与该断言无关；TextServices COM 异常发生在失败后的 WPF 收尾。
- #971/run 38393317198 公开页仍为 In progress，日志需登录；#978/run 38509928015 为 Failure，完整日志需登录。Actions 列表仅到 #979（ae3c565），未显示当前 HEAD run；SDK9 本机结果不替代 SDK8/SDK10.0.401 CI。
- 用户暂时不可回复，所需的外机提交、工作树、测试 DLL SHA/ProductVersion 与源码 145–160 行记录为待验，不阻塞当前 R08-05。
## 2026-10-14 R08-05 页面切换当前 main 复核

- 现有生产页缓存与 PageHost 行为已满足任务方向，没有重建页面服务或动画体系。本次没有生产代码/XAML 改动，只复核真实 shell/page 行为并刷新当前 main 证据。
- 隔离 SDK 9.0.302 Release solution build 0 warning/0 error；R08PageSwitch 4/4、ProductionShellChrome 22/22、UiFinesseFoundation 9/9、R02 Busy 5/5、R08 business feedback 4/4，共 44/44。source validation 通过，XAML 结构 24/24，文档同步后的 diff check 另行校验。详细 SHA/MVID/TRX 在 R08-05-CURRENT-MAIN-RECHECK-20261014。
- 真实 WPF 页面切换保留 DataGrid offset 10→10、任务引用、选中项与 DataContext；两次不同页面切换各 Measure/Arrange 1/1，同页重入 0/0；96 项集合和筛选/页签状态恢复，refresh command 0 次。七次快速路由后导航标题/焦点一致；PageHost 无 Blur/整页入场动画。
- ShellChrome 22 项均通过、TRX exit 0；testhost 收尾出现 TextServices COM 异常输出，根因未知。误用类名的零测试筛选器未计入 44/44。
- 本机 SDK9 不替代仓库 SDK8.0.423 或 CI SDK10.0.401。公开 Actions 仍只显示 #979（ae3c565），#971 In progress，#978 Failure 且日志需登录；推送的 R09 文档提交 8dda9995 尚未显示 run。真实 Playnite、物理 DPI/跨屏、presented frame、UIA/读屏、ETW/宿主性能仍待验。
- R08-05 当前 main 受控条件已满足，视觉不适用，不生成截图。下一可执行任务 R07-08 当前 main resize 复核；先查现有实现/测试及 #960/#968 的提交边界，不凭旧失败摘要猜修。
## 2026-10-14 R07-08 当前 main 复核与继续点

- d7f6fc42 已推送到 main；R08-05 SDK9 隔离 solution 0/0、行为 44/44，CI 公共页尚未列出该提交 run。R07-08 复用同源码基线 552b8168 的 Release test assembly，当前精确 R07 行为 1/1，TRX、DLL SHA/MVID/ProductVersion、源 SHA 和实际 PageHost 尺寸见 docs/design/reviews/ui-finesse-round3-20260915/evidence/R07-08-CURRENT-MAIN-RECHECK-20261014/README.md。
- R07 当前本机受控行为覆盖生产详情打开、GamePicker、任务表、焦点与五次 resize；不证明真实 Playnite 拖动、物理 DPI/呈现帧/宿主耗时。#960/run 38327521241 已失败但完整日志 API 403/404，CI SDK与内部根因未知；不能用 #968 历史双 lane 或 SDK9 结果替代当前 CI。
- Actions 页面最近可见 #979（ae3c565）；按 d7f6fc42 head SHA 查询公开 REST API 返回 HTTP 403 rate limit。R08/R07 当前推送 SDK8.0.423 与 SDK10.0.401 终态未确认，不能据此判断 run 不存在。继续查到成功/失败后按完整日志处理；CI 运行时不宣称阶段完成。其他任务的未跟踪证据保留。
- 下一可执行项 R10-02。R09-05 外机提交、工作树和 DLL/PDB 身份待用户 2026-10-15 09:00 后补充；期间继续独立工作。

## 2026-10-14 R10-02 当前游戏定位与 CI 继续点

- 代码提交 `13704e75234e7f8d943e15ae8380a728a277375e`。复用现有 `TaskSourceNavigationResolver`、DTO、选框与命令；测试通过实际任务命令覆盖同名 ID 正例和过期 ID 安全负例，并验证缺失 ID 的同名选框不回退。`WorkspaceLoadSchedulerOverrideForAudit` 默认 null，仅允许 fixture 拦截 Worker 加载启动，正常生产路径不变。
- SDK `9.0.302` 本机编译 Playnite `net462` / tests `net472`；R10 behavior `5/5`，相邻导航/来源组 `25/25`；source validator、diff check 通过。TRX、DLL SHA/MVID、源身份与边界见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R10-02-CURRENT-MAIN-RECHECK-20261014/README.md`。本机 SDK9 不替代 SDK8.0.423/CI SDK10.0.401。
- 用户指定 run #920（`36677346317`，`77de7f4`）当前 Failure：7m04s、无 artifact，公开页面仅显示“编译、测试与打包”exit1；logs 需登录，实际 SDK 与失败根因未知。当前公共列表最新 #972（`8546aca`）为 In progress；本阶段代码尚未推送。推送后逐一跟踪 SDK8 和 SDK10.0.401 到终态；失败先尝试取得完整 job log，不能凭摘要盲修。
- R10-02 本机条件完成但阶段仍等本次 CI 终态；未运行真实 Playnite/媒体 Worker、未验证真实呈现、物理 DPI/跨屏、OS UIA/读屏、ETW/宿主性能。CI 收口后下一可执行任务 R10-03 搜索快捷键；先核对搜索绑定、焦点、弹层/dialog 作用域并补实际正/负行为。R09 外机 HEAD/DLL/PDB 暂待用户明早回复，不阻塞。

## 2026-10-14 R10-06 筛选来源清除行为复核

- 生产来源提示与清除命令沿用 `5403d797`。本轮补实际 `TaskCenterView` 按钮调用生产 ViewModel 命令，验证来源条件清除、提示/命令状态、第二条合成任务恢复、其余任务草稿保留和当前页请求 token 取消。Release 项目构建成功；R10-06 定向 `3/3`，相邻 R09 焦点 `2/2`，本机 SDK9.0.302/VSTest17.14.1。详细源/程序集身份见 `evidence/R10-06-CURRENT-MAIN-RECHECK-20261014/README.md`。
- 最新前序 run #992（父提交 `a2fdb4f6`）Failure。SDK10.0.401 job `115852859775` 的“SDK 10.0.401 编译与测试复核”与默认 build job `115852860137` 的“编译、测试与打包”均 exit1；完整 logs/API 403，两个 artifact API 403、网页下载404，根因未知。此 run 不包含本轮 R10-06 测试。不得按通用 exit code 猜修；本轮提交触发的 Actions 必须另行跟到终态。
- 用户到明日 09:00 前无法回复。即便 logs 暂不可获取，推送后跟完新 Actions，再继续 R10-07 侧栏信息密度；等待外机 R09 HEAD/DLL/PDB 不阻塞主队列。真实 Playnite、任务服务、UIA/读屏/IME、物理 DPI/跨屏、presented frame、ETW/宿主性能仍未验。
