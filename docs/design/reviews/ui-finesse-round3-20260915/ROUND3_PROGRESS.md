## 2026-10-16 R13-04 当前 main 双 SDK CI 收口与外机 R09 测试源对照

- R13-04 本机 SDK8.0.423 定向 Worker/Core/Playnite 为 `4/4`、`1/1`、`24/24`；R09 同隔离输出 `2/2`。代码 `ccdbacb6` 与证据提交 `a81e7abd` 已推送，当前 HEAD 与 `origin/main` 一致。Actions #1039/run `38872163664` 两条 job 终态成功：SDK8 `116878566799`、SDK10.0.401 `116878566530`；Core `125`、Worker `362`、source `108` 类/WPF `124` 类全通过；SDK8 Render QA、静态审查及扩展包生成成功。详见[R13-04证据包](evidence/R13-04-CURRENT-MAIN-RECHECK-20261016/README.md)。
- 外机 R09 附件显示 source `111` 类/WPF `116` 类，line151 仍断言 Chrome BorderThickness `2 DIP`；当前源 line151 是错误色、line152 Chrome `1 DIP`、line154 独立 ValidationStroke `2 DIP`。断言形态与旧 `3f7d0b30` 一致，`f13b9e05` 已拆分。附件路径 `gsc-b/fd7672ef` 是随机目录，不是 commit；粘贴文本缺 sourceCommit、dirty paths、DLL/PDB SHA 与实际插件 DLL 身份，故确认输入版本不一致，但具体来源仍未知。当前 Actions 通过不覆盖外机失败。
- GitHub 连接补取 #992/#993 完整日志：SDK8.0.423 与 SDK10.0.401 两 lane 均失败于 R08 入场动效测试夹具（`opacity=1; loaded=True`，无动画 clock），后续 TextServices COM 异常发生在失败后收尾；这些旧失败与 R10-06/R09 厚度断言无关。后续夹具调整已在 main，当前 #1039 双 SDK 全量 WPF 通过。
- CLOSE-WRAP-01、CLOSE-OPTICAL-01、CLOSE-FOCUS-01 的受控门禁已由 #1039 收口；R07-05/06/07/08 在同一全套 WPF `124` 类通过，但真实 Playnite、触控板、窗口拖动仍待验，当前 main 复核报告链接保留在各表格行。R00/R01 freshness `14/14` 无需重跑、0 登记源码路径命中，package identity 未提供。
- 下一批继续根据未完成项与依赖选择 Q/R 任务；R05-03“翻转无闪屏”仍缺真实呈现帧，离屏截图/打开后几何不签收该条件。

## 2026-10-16 R13-03 当前 main 行为复核与外机 R09 对照

- R13-03 的生产实现已在 `680ea83a` 落地；本轮提交 `ad2e6716` 只增强行为夹具，没有重建命令、DTO、Worker 服务或 XAML。当前隔离 SDK8 Playnite `R13CloudTransferStageBehaviorTests 14/14`；Worker 全套 `360 passed / 1 Named Pipe 环境 skip`，包含复用已有本地副本的失败云端重试用例；Core `125/125`。R00/R01 freshness 在 `ad2e6716` 为 `14/14 fresh / 0 stale / 0 source matches`、比较 `2,866` 路径，package identity 未提供。源码、隔离输出 test DLL/PDB identity 和 TRX 见[R13-03 当前 main 复核](evidence/R13-03-CURRENT-MAIN-RECHECK-20261016/README.md)。
- 锁定 SDK `8.0.423`、MSBuild `17.11.48`、VSTest `17.11.1` 的完整隔离 Release build 为 `0 warning / 0 error`，XAML `24/24`。完整 Playnite 扫描在 WPF 类 `45/124` 中因 R05 Tooltip fixture 的 `GetCursorPos` 返回 Win32 error 5 停止；该桌面权限阻塞下不能记作 Playnite 全量通过。之后从同一隔离输出 `--no-build` 单独运行 R09 焦点轮廓类 `2/2`、R13-03 类 `14/14`。没有移动系统指针或尝试绕过输入权限。
- 用户本次粘贴的外机 R09 日志仍是 `Expected 2 / Actual 1`，堆栈 line 151，但该行在当前源（及祖先提交 `983fee5`）检查错误色，`ValidationStroke 2 DIP` 位于 line 154；外机 WPF 类总数是 `116`，当前干净隔离程序集发现 `124`。这证明日志映射/测试集不是本次当前 main 复核身份，无法确定外机 HEAD、源/DLL/PDB 或根因；没有更改模板或放宽断言。R09 TRX 与身份见[R09 外机复核记录](evidence/CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md)。
- Actions #1037/run `38859916493` 的 SDK8 和 SDK10.0.401 jobs 均成功，但运行源码是推送基线 `09173448`，不是 `ad2e6716`。待本轮文档提交并推送后，继续跟踪当前提交的两个 SDK job 至明确终态。完整一键脚本其余限制、证据 SHA 与边界见[R13-03 当前 main 复核包](evidence/R13-03-CURRENT-MAIN-RECHECK-20261016/README.md)。下一项 `R13-04 暂停与允许时段`。

## 2026-10-16 R13-02 当前 main 下次重试时间行为补证

- R13-02 的生产实现早已存在于 `6fd22892`，本轮先核对 `CloudTransferStatusDto.NextAttemptUtc/NextAttemptLocal`、详情绑定与页面生命周期；没有改生产 DTO、XAML 或 Worker。提交 `1e7ff4d8` 只把测试补强并并入既有生产 `MaintenanceView` STA 窗口行为用例。
- 本机 SDK `8.0.423`、MSBuild `17.11.48`、VSTest `17.11.1`：Core 重试边界 `2/2`，Playnite `R13CloudTransferStageBehaviorTests 14/14`，均为 Release 定向项目构建/测试；source validation、diff check 通过。生产维护页实际详情显示相对重试时间、完整本地/UTC辅助时间、Automation HelpText 和同条记录的失败原因；到期记录重新选择后显示“可立即重试”，不出现负数。没有逐行 DispatcherTimer，故无需为每条有限列表项目常驻计时。
- R00/R01 freshness 在提交 `1e7ff4d8d121f46869c87d8315d466056344a1ae` 为 `14/14 fresh`、`0 stale`、`0` 个登记源路径命中，比较 `2,861` 路径；package identity `not-provided`。TRX、源码/程序集 SHA 与实现边界见 [R13-02 当前 main 复核](evidence/R13-02-CURRENT-MAIN-RECHECK-20261016/README.md)。R13-02 继续标记“已满足”，没有重建既有功能。
- R12-05 #1035/run `38855405119` 已由 SDK10.0.401 job `116825613833` 与 SDK8 job `116825614090` 全部成功。R13-01 #1036/run `38857050764` 的 SDK10 job `116830769335` 成功；SDK8 job `116830769619` 已过编译/测试、离屏 QA、WPF 静态审查，正在打包。两次运行都不包含 `1e7ff4d8`；新测试/文档提交推送后，继续跟踪新 Actions 至终态。
- “时钟变化”以已过期时间戳重新求值模拟，不调用 OS 改时 API或证明正在显示的绑定会因系统时钟事件立即刷新。窗口为逻辑 `1440×1000 DIP` 隔离 STA，不是 Playnite/package-host、物理 DPI/跨屏、最终呈现帧、OS UIA/读屏/IME、ETW 或宿主性能证据。只用合成 DTO，未触及真实存档、媒体、云端或外发诊断。Demo 原目录不可用，沿用恢复生产基线。
- 下一项 `R13-03 手动重试范围`：先盘点已有命令入口、幂等请求身份及部分成功语义，再按条件补正负行为证据。

## 2026-10-16 R13-01 当前 main 阶段行为补证与 R12 CI 跟踪

- R12-04 提交 `86ba18a5` 的 Actions #1034/run `38846116072` 已成功：SDK8 job `116796913269` 与 SDK10.0.401 job `116796913109` 均通过。R12-05 提交 `241bfb16` 对应 #1035/run `38855405119` 最近读取仍为 `in_progress`；SDK10 job `116825613833` 与 SDK8 job `116825614090` 均在 Release 编译/测试步骤。本机结果不代替两个 CI lane。
- R12-05 复用既有 staging service、TaskStatusDto 与 MaintenanceView；修复取消投影丢失 Worker 清理结果，并补锁文件残留负例及生产维护页进度/取消/残留提示行为。其定向本机证据见 [R12-05 当前 main 复核](evidence/R12-05-CURRENT-MAIN-RECHECK-20261016/README.md)。本轮 SDK8.0.423 全解方案 Release build `0 warning / 0 error`、XAML `24/24`，覆盖 R12 修正与 R13 测试夹具。
- R13-01 最新 main 已具有 `QueuePhaseDisplay`、保证级别与队列投影。本机 SDK8 Core `40/40`、Worker `14/14`、Playnite `21/21`；新增生产 `MaintenanceView` 隔离 STA 行为测试，读取实际 DataGrid 单元格和详情绑定，确认“上传成功/等待验证”不冒充“远端校验成功”。状态机新增的 Worker 用例在隔离 SQLite 中执行，不连接远端。源码、程序集 SHA 与脱敏 TRX 见 [R13-01 当前 main 复核](evidence/R13-01-CURRENT-MAIN-RECHECK-20261016/README.md)。R13-01 状态保持“已满足”，未重建生产服务。
- 提交 R13 测试与证据后继续跟踪对应 Actions；R12 #1035 失败则先取得完整日志按实际错误处理。R00/R01 在 R12 源码提交 `241bfb16` 上为 `14 fresh / 0 stale / 0 matched paths`，package identity `not-provided`；R13 本批提交后重跑 freshness。下一项 R13-02 下次重试时间。真实远端、真实存档、Playnite host、独立 OS UIA/IME、物理 DPI/跨屏、最终呈现、ETW 与宿主性能仍未验。

## 2026-10-15 R05-03 Popup 复核与外机 R09 断言映射

- 当前 `main` 源码检查点 `91bc88ad2f865c760469c8ad68fc6bf061c4a97b` 上，本机 SDK `9.0.302` / VSTest `17.14.1` 定向 Release 构建 Playnite 测试项目及引用；R05 Popup 边界与选框虚拟化 `7/7`，R09 焦点描边 `2/2`。本机缺仓库锁定的 SDK `8.0.423`，因此这不是 SDK8、SDK10.0.401 CI 或完整 solution build。脱敏 TRX、源码/程序集/PDB SHA 与行为边界见 [R05-03](evidence/R05-03-CURRENT-MAIN-RECHECK-20261015/README.md) 和 [R09](evidence/CI-R09-CURRENT-MAIN-RECHECK-20261015/README.md)。
- R05-03 现有测试证明 Popup 最终几何在当前桌面工作区内、最大高度与滚动生效、选定项可见；未捕获呈现帧，“翻转无闪屏”仍未验，任务保持部分满足。前次隔离 Playnite profile 的宿主进程现在已退出；确认 profile 位于仓库 `.tmp`、审计 marker 匹配、无重解析点与冲突进程后，清理精确目录并回收 `134,054,425` bytes。本轮没有重试真实宿主。
- 用户新贴的外机 R09 日志仍是 `Expected 2 / Actual 1`、stack line 151。当前源码 line 151 检查错误色、line 152 检查 Chrome `1 DIP`、line 154 检查独立 `ValidationStroke 2 DIP`；历史 `3f7d0b30` line 151 是旧 Chrome `2 DIP` 断言。当前 main 本机 SDK9 同输出 `2/2`。外机实际 HEAD、工作树、DLL 与 PDB 未知，因此旧测试源/二进制映射只是证据线索，根因未确认；没有更改生产样式或放宽断言。`libpng iCCP` 不在断言调用栈，COM teardown 出现在断言失败之后。
- 下一独立工作继续按 Round3/当前 backlog 选依赖已满足的 Q/R；R05-03 只在得到真实呈现帧证据后再验收。新文档推送后分别跟踪 SDK8 与 SDK10.0.401 Actions 至明确终态。

## 2026-10-15 R07-08 当前 main resize 压力复核

- 复核 R07-08 的五阶段正常→窄→短→宽→恢复序列。旧 `552b8168` 基线之后共享 `WpfUiProduction.xaml`、`Redesign.xaml` 和 DataGrid 输入接线更新，虽 TaskCenter 页面/测试源未改，仍按当前 main 隔离重建。源码 `d190f7eac4d85e81193080a2ff4f73105cec9b45`。
- 本机仓库锁定 SDK `8.0.423` / VSTest `17.11.1`，Playnite 测试项目与引用 Release build `0 warning / 0 error`，XAML `24/24`。R07 resize 行为 `1/1`，Task 响应式相邻类 `8/8`，合计 `9/9`；TRX、程序集和源哈希见[R07-08 当前 main 复核包](evidence/R07-08-CURRENT-MAIN-RECHECK-20261015/README.md)。不是完整 solution build 或 CI SDK10.0.401 结果。
- 生产 TaskCenter 详情通过按钮 routed Click 打开；Task 选择、详情、GamePicker 与搜索焦点在五阶段均保留。Window 尺寸 `1366×900 → 960×700 → 960×560 → 1440×900 → 1366×900 DIP`，PageHost 最终恢复到 `1038.667×759.333`，基线误差不超过 `1 DIP`。窄/短详情 `136 DIP` 且 MaxHeight 收紧为 `136`，回到宽窗后取消有限 MaxHeight；任务表非空且 MaxHeight 不残留，shell 接收 `11` 次 SizeChanged。
- 每步经 Dispatcher `ApplicationIdle` 与 `UpdateLayout` 后测量。它证明受控布局状态和恢复几何，不证明真实鼠标拖动帧时间、Playnite、物理 DPI/跨屏、UIA、presented frame、ETW 或宿主性能。退出时 WPF TextServices 清理输出 `InvalidComObjectException`，VSTest `9/9` exit `0`，原因未知。
- d190f7ea 已推送；当前网络通道无法读取 GitHub Actions run ID/终态。#1012/run `38640644812` 对父提交 `2b423737` 上次观察为 In progress，不推断 d190f7ea 状态。SDK8 本机结果不替代 SDK10 CI；新证据提交后继续追踪 Actions。下一项 R10-02 当前 main 复核。

## 2026-10-15 R07-07 当前 main 受控滚动复核

- 按任务表的细滚、反向、加速边界、末端、Layout 与容器数据条件，复跑生产 DataGrid/ScrollViewer 行为和相邻几何/响应式回归；没有改生产或测试源码。源码身份 `2b42373771e950d087f393c0894380530d5eb227`。
- 本机仓库锁定 SDK `8.0.423` / VSTest `17.11.1`，Playnite 测试项目及引用 Release build `0 warning / 0 error`，XAML `24/24`。六类隔离 WPF 用例 `21/21`：细滚 `2/2`、滚动归属 `2/2`、Media geometry `3/3`、响应断点 `5/5`、Task responsive `8/8`、横向滚轮相邻回归 `1/1`。这不是完整 solution build 或 SDK10.0.401 CI 结果。
- 实测：12×`-30` routed wheel 令 offset `0→36 DIP` 单调、每步 `+3`；反向 `33→30→27`；末端 `154/154`；20 个事件的 `LayoutUpdated=17`，5 个无位移末端事件增量 `0`，DataGridRow `6–9`；嵌套 `-360` 后外层 `32→80/432`、内层 `628→628`、布局 `1`、可见容器 `2`。数字取当前 TRX，测试还断言边界、终点和布局预算。
- 事件是合成 WPF routed wheel，不是真实触控板/OS 输入；逻辑 DIP 和布局次数不代表真实 Playnite、呈现帧、物理 DPI/跨屏或宿主性能。没有访问真实存档、媒体或云端。
- 证据包含六份脱敏 TRX、程序集和源码 SHA：[R07-07 当前 main 复核](evidence/R07-07-CURRENT-MAIN-RECHECK-20261015/README.md)。同次用 SDK8 重建的 R09 当前 main 对照 `2/2` 记录于[R09 外机复核](evidence/CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md)；外机 DLL/PDB/HEAD 仍未知。
- #1010/#1011 已 Failure，完整日志 API 403、实际根因未知；#1012/run `38640644812` 的上次观察是 In progress，当前读取被本机代理拒绝和页面 cache miss 阻断，终态未确认。新证据 commit 推送后继续核验 Actions；不能以本机测试替代 CI。下一项 R07-08 当前 main resize 压力序列复核。

## 2026-10-13 Actions #965 布局测试宿主校正

- 用户提供的 `GameSaveCenter-build-diagnostics (2).zip` 哈希与 #965/run `38342488530` artifact `11788460821` 匹配；CI 源码 `d0dbaea00dc620545524236c55444748d43d6540`、SDK `8.0.423`、VSTest `17.11.1`。Release solution 构建成功 `0/0`，R08Numeric `5/5`；布局类 `50/54`，失败是 `CompactInboxKeepsBatchButtonsCompactAndTheGridInsideItsFrameRow` 和 `SettingsHeaderAndPathActionsStayAnchoredToTheirLabelsAndEachOther` 的浅/深主题实例。
- 原始值：收件箱按钮中心对全局目标为 `42/42/0 DIP`；Settings 宽布局实测 shell `952 DIP`，旧断言要求 `1320–1360 DIP`。诊断没有记录 Window.ActualWidth 或 runner 分辨率，不能据此断言生产 UI 缺陷或具体系统限宽；测试将请求窗口尺寸当成逻辑布局尺寸。
- 两项测试现使用显式尺寸 Grid 逻辑宿主，Window 只提供 PresentationSource。没有改生产 UI。隔离 SDK `8.0.423` Release solution `0/0`，`ReportedWorkspaceLayoutBehaviorTests 54/54`；SDK9.0.302 对照 `54/54`。SDK8本机 VSTest `17.11.1`，测试宿主 .NET Framework `4.8.9181.0`，CI 为 `4.8.9345.0`。测试源码、程序集身份、脱敏 TRX 与完整范围见 [#965 布局宿主复核](evidence/CI-965-LAYOUT-HOST-CURRENT-MAIN-20261013/README.md)。
- 本阶段提交推送后必须检查新 Actions 到终态；成功后重跑 freshness，先复核 R00/R01 再推进依赖满足的 Q/R。当前逻辑几何不代表屏幕最终呈现、真实 Playnite或物理 DPI/跨屏。

## 2026-10-13 R06-05 当前 main 列头说明复核

- 当前生产源码基线 main `d60e472de8c1f52c9eedc348f0274562c06d7e53`。没有改生产 XAML、视图模型、命令或 DTO；先盘点既有 `DataGridColumnHeaderHelpBehavior`、共享 header 模板、容量格式化及各生产表列宽，再增强此前保留的 R06 测试改动。
- 本机隔离 SDK `8.0.423`、Windows `10.0.22000`；Release solution `0 warning / 0 error`、XAML `24/24`、Playnite `net462`、Tests `net472`。首次误从项目旧 `bin` 运行测试，`TestRepositoryContext` 因程序集 commit `a2f8883c` 与源码 HEAD `d60e472d` 不一致而 fail-closed；改用隔离输出属性后 `R06ColumnHeaderHelpTests 2/2` 通过。首次阻断不是产品或测试断言失败，未计入结果。
- 测试实例化生产 `DataGridColumnHeader` 样式，行为验证 Tooltip/Automation HelpText、Header string 保持、排序/重排能力、无嵌套 ButtonBase、左右 resize Thumb、长表头 Wrap/None 实际高度，以及两次原生列头 Click 后升/降序和 `ListCollectionView` 行顺序。容量单位沿用 B/KiB/MiB/GiB 的 1024 进制；当前列均有具体业务语义，未找到无值占位列。
- 当前 main RenderHarness `render-qa OK`，Light/Dark 共 372 PNG；报告 commit `d60e472d`，`WorkingTreeClean=False`（R06 测试改动与已有其他任务未跟踪证据仍在树上），离屏 `DpiScale=1.00`。Save/Task/Media/Maintenance 四页 1040×700 双主题图人工检查；Save 7、Task 7、Media 5、Maintenance Findings 3、Audit Findings 4、Audit Log 3 列 header contract 均 `resize=true/sort-arrow=visible`。摘要、8 张精简图、脱敏 TRX、source/DLL SHA 见[R06-05 当前 main 复核包](evidence/R06-05-CURRENT-MAIN-RECHECK-20261013/README.md)。
- R00/R01 证据校正提交 `a2f8883c` 对应 #954（run `38313448857`）终态失败；完整日志已归档于[CI/R07-08证据包](evidence/CI-R07-08-RESIZE-FAILURE-20261013/run-38313448857-complete-logs.zip)，SHA-256 `CAB266AD735E3E145B4A31C4AA50A641F2FC4DD4371EDA30AF29972A7AE82807`，CI SDK `8.0.423`。日志只确认旧 R07 `IsVisible` 初始断言失败，未输出实际可见性/祖先/尺寸数据，不能归因 R00/R01。#960（run `38327521241`，commit `d60e472d`）最新公开页面当前为 `In progress`，与之前记录的终态失败相冲突；曾关联附件 `11781543273`、580 KB、SHA-256 `113893ceea28078586cad28dc98fff77bcb402d08457a979b862d4a227bdfc51`。公开页要求登录查看日志，匿名 API HTTP403、artifact 页 HTTP404；当前状态、CI SDK、失败步骤与根因待可信终态和完整日志确认，不据旧状态或退出码盲改。
- 本地 R06-05 行为/离屏视觉条件满足；对应测试/证据/记忆/日志随本阶段提交推送后，必须跟踪 Actions 到终态。未验真实 Playnite、真实鼠标悬停/键盘/OS UIA、拖拽列宽、读屏/IME、物理 DPI/跨屏、presented frame、ETW或宿主性能；无真实存档、媒体、云端操作。下一项依赖可独立推进 R06-06 行内进度稳定。

## 2026-10-13 R07-08 / R08 #959 CI 失败定位与动效前提修正

- Actions #959（run `38324971243`，commit `be2881e19cfca549b39c17732119aca129044c59`，CI SDK `8.0.423`）Release 构建及先前测试通过；R07 resize 类 `1/1` 通过。随后 R08 `SidebarRapidReversalUsesLatestTargetAndReleasesOldClock` 失败。完整日志显示原生策略为 `HighContrast=False; ClientAreaAnimation=False; expected=False`，失败夹具观测 `running=False; base=72; actual=72`。生产按系统偏好禁用动画并归一到 72 DIP，日志指向动画正例夹具没有显式控制系统输入，而非生产故障。完整日志 SHA-256 `6ABB957DDDE4AF6C6BED8909387D172325B39E97A66936495A50EF07C9ABB428`；diagnostic artifact ID `11781132337`、590,570 bytes、SHA-256 `0A17ACE44C551EF65FE236FB7D433F517DFFB843959EC38A1790424EFDDA58CB`，均归档于[证据包](evidence/CI-R07-08-RESIZE-FAILURE-20261013/README.md)。
- `R08MotionReverseBehaviorTests` 现同时固定 `MotionEnabledProvider` 与 `SystemMotionEnabledProviderForAudit` 为 true，并在开始采样前断言 `SidebarMotionEnabledForAudit`，确保在系统关闭客户端动画的 runner 上仍测试真实动画正/反向生命周期；原生系统偏好行为仍由 `ProductionShellChromeSourceTests` 的独立矩阵检查。无生产代码改动。
- 本机隔离 SDK `8.0.423` Release 重建 `0 warning / 0 error`、XAML `24/24`；R08 `2/2`、原生动效偏好组 `22/22`、R07 `1/1`。R08/TRX/源/DLL SHA 及完整 CI 日志已入包。R07 Window/PageHost 实测宽度修正由 #959 单项测试通过；R07-08/R08 整体阶段需待本修正推送后的 Actions 成功再收口。
- 没有改生产 XAML/视图模型/命令/DTO，也未切换操作系统偏好。受控 STA WPF 逻辑时间/布局不是真实 Playnite 最终呈现、物理 DPI/跨屏或 ETW 证据。下一步仅提交本阶段测试、日志与事实同步，推送并持续检查新 Actions；新 run 成功后独立核对 R06-05。

## 2026-10-13 R07-08 实测窗口宽度断言校正

- Actions #958（run `38322407969`，commit `1c08196d`，CI SDK `8.0.423`）确认紧凑详情按钮 Click、抽屉显示及开/关按钮互换通过；随后旧宽度判据失败：测试计算 `RequestedWidth - ShellActualWidth` 并要求不超过 `32 DIP`，runner 实测差值 `336 DIP`。完整日志 ZIP SHA-256 `84d55e58c4bec5c34b10d9e2e1eec569c912b99b96529bae4bbf395414918dce` 已归档。失败发生在测试把请求尺寸当作实际窗口尺寸，不是生产控件失败。
- 测试现记录 `Window.ActualWidth/Height`、`PageHost.ActualWidth` 和生产 detail breakpoint 状态。窗口到 Shell 的 inset 以实测窗口尺寸计算；宽屏/紧凑预期依实测 PageHost 宽度和带迟滞阈值判断；详情 MaxHeight 断言依实际 compact 状态，不按请求标签推断。
- 本机从隔离路径 `D:\GameSaveCenterTools\dotnet-sdk-8.0.423` 临时前置 PATH，使用官方 `scripts/build.ps1 -Configuration Release -SkipTests`：Release solution `0 warning / 0 error`、XAML `24/24`；定向 `R07ResizeStressBehaviorTests 1/1`。五次样本实测 PageHost `1038.667 / 632.667 / 632.667 / 1112.667 / 1038.667 DIP`，前后进入宽屏、窄/短进入紧凑；详情持续可见，搜索焦点与 picker 可达，shell `SizeChanged=11`。脱敏 TRX、源/DLL SHA 见[证据包](evidence/CI-R07-08-RESIZE-FAILURE-20261013/README.md)。测试退出前仍输出 TextServicesHost COM cleanup 异常，成因未知，测试进程 exit `0`。
- 当前只改 R07 行为夹具和阶段记录，未改生产 XAML/VM/命令/DTO。合成任务、受控 STA 窗口与逻辑 DIP 不证明真实 Playnite 最终呈现、OS UIA/IME、物理 DPI/跨屏、ETW 或宿主性能。新测试修正待提交/推送，需追踪新 Actions 到终态；成功后再单独处理 R06-05。

## 2026-10-13 R07-08 compact 详情抽屉交互校正

- Actions #957（run `38319864960`，commit `063550b6`）CI SDK `8.0.423`：源码检查、Release build、Core/Worker 测试通过，R07 单项失败。完整日志中的状态为详情 `Collapsed/0×0`，TaskView/Window 可见，`TaskGrid.SelectedItem` 与绑定 `SelectedTask` 同为 `r07-resize-task`；日志 ZIP SHA-256 `36491ec7689e9483521124edb4af470d4255e771e40893fc8ac8ee2a5bc22576` 已归档。它否定了“只需修复选择绑定身份”的前一夹具假设。
- 对照生产 `TaskCenterView`：`OnTaskSelectionChanged` 将 `taskInspectorOpen=false`；compact 模式详情面板保持折叠，直到“查看任务详情”按钮的 Click 处理器翻转开关。夹具之前在挂载前已把 `SelectedTask` 设为 DTO，加载后再次给 `TaskGrid.SelectedItem` 赋相同对象，既没有覆盖未选中→选中流程，也没有执行 compact open action。
- 当前工作树已改为从 `SelectedTask=null` 加载；在 `960×700` 窗口的已加载 DataGrid 实际选中合成 DTO，验证 compact 按钮可用/详情折叠，再路由生产 Button.Click 并检查详情有面积、open 按钮隐藏、close 按钮显示；之后再跑宽→窄→短→宽→恢复序列。没有改生产 UI、命令或 DTO。
- 本机官方 SDK `8.0.423` 最终 Release `0 warning / 0 error`、XAML `24/24`；R07 `1/1`。TRX 确认 `compactDetailsAction=clicked`、初始详情 `632.667×136 DIP`；五个 resize 样本详情为 `360×476`、`632.667×136`、`632.667×136`、`360×476`、`360×476` DIP。WPF `TextServicesHost.InvalidComObjectException` cleanup 在 VSTest 结束后出现，原因未知，进程仍 exit `0`。源码/DLL/TRX SHA 与日志见[证据包](evidence/CI-R07-08-RESIZE-FAILURE-20261013/README.md)。
- 当前交互修正还未提交/推送，故无对应远端 CI 结果；R07-08 未收口。这里证明的是受控 STA WPF compact drawer 与 resize 布局，不证明 Playnite 真实呈现、物理 DPI/跨屏、OS UIA、presented frame、ETW 或宿主性能。下一步提交此 R07 夹具与证据、推送 main、追踪 Actions 终态，再推进 R06-05。

## 2026-10-13 R07-08 CI 失败定位与本机判据修正

- 用户指定检查点 #920（run `36677346317`，commit `77de7f450431d267026ce2169aa30525b18bf528`）已终态失败，完整日志确认 CI SDK `10.0.401`；#919 同样失败。两项均在旧侧栏过渡测试的启动断言失败，后续 main 已改源码；当前 SDK8 测试结果不能替代该旧提交的 SDK10 结果。
- #952/#953/#954（runs `38309632982`、`38312234186`、`38313448857`）完整日志确认 CI SDK `8.0.423`，均在旧 `IsVisible` 判据失败；旧日志没有记录当时详情区域的本地 Visibility、祖先可见性或实际布局尺寸。#956（run `38317512412`，commit `494e7a6e`）同为 SDK `8.0.423`，源码检查和 build 成功，R07 失败日志记录详情 `Collapsed/0×0`，而 TaskView 与 Window 可见；测试当时没有记录 grid/view-model 的选择身份，不能据此归因生产缺陷。七份完整日志 ZIP 与 SHA-256 已归档到[CI/R07-08 证据包](evidence/CI-R07-08-RESIZE-FAILURE-20261013/README.md)。
- R07 夹具现先显示并布局窗口，再经已加载的 DataGrid 选择合成任务、滚入视口，并断言 `SelectedItem` 与绑定 `SelectedTask` 是同一对象；随后检查详情可见性/正尺寸，在宽→窄→短→宽→恢复五个阶段记录详情尺寸。保留 picker、搜索焦点、任务表非空、MaxHeight 预算与 shell resize 传播断言。SDK `8.0.423` 官方 Release build `0 warning / 0 error`、XAML `24/24`；最终夹具定向行为 `1/1`。证明受控 STA WPF 布局，不证明实际呈现帧、物理跨屏或真实 Playnite。
- 修正提交 `55d3df19` 对应 Actions #955（run `38317008652`）已终态 failure，CI SDK `8.0.423`。完整日志显示失败发生于“源码结构检查”：更新 CURRENT_STATE 时漏掉校验器要求的 `0.6.73` current-facts 标记，编译/测试/渲染/打包全部跳过；日志 ZIP SHA `c022325198c2c1f4245b7462fc0e5111ac7502fd0832dbfa70d1c25f52acd3e3` 已归档。标记已补回，当前 `python scripts/validate-source.py` 通过。#956 后对 R07 夹具的 loaded-grid 选择修正尚无远端结果；R07-08 未收口，待新 Actions 终态后再推进 R06-05。

## 2026-10-13 R00-03 / R00-04 当前 main freshness 复核

- 本轮前 freshness 为12 fresh / 2 stale。R00-03 关联的 DashboardView 变化加入了默认路径不启用的测试构造/动效 override；R00-04 命中的 Shell XAML 变化仅为 Tooltip BetweenShowDelay `100`→`0`，搜索代码未变。仍按审查要求复核行为，不把不相关差异改写为产品缺陷。
- main `5762c874ef9c1ee2e92692a61c71d17cc9c0f371`：官方 Release build SDK `8.0.423`，0 warning/0 error、XAML 24/24、Playnite net462、测试 net472。动画/overlay/Dashboard 焦点及搜索定向组共25/25；2000项合成搜索30/30不同查询改变结果集，超时负例通过，p50/p95/max为46/63/75 ms。六份脱敏TRX与原始测量见[R00-03/R00-04当前复核包](evidence/R00-03-04-CURRENT-MAIN-RECHECK-20261013/README.md)。
- 更新两条 baseline sourceCommit 后 freshness 为14/14。其余12项和R01没有命中本轮源路径，继续保留此前证据；package identity仍未提供，freshness不表示完整宿主验收。
- 本节复核时 Actions #953（docs-only commit `5762c87`）仍为 In progress；随后 #953/#954 均已终态失败，完整日志与判据修正见本文件前面的 R07-08 记录。#952 也已取得完整日志。下一项审查R06-05现有列头实现和行为证据，再按依赖推进Q/R。
## 2026-10-13 最新 main/CI 检查点
- Actions #952（run 38309632982，commit 4572118f，job 115011915461）已终态失败；job 用时12m07s，“编译与测试”仅有exit1 annotation。诊断artifact 11774606363（556 KB，SHA-256 9686cc633488c860c9dd8021690986abfff69129f1420ced7b433ff062490a3a）；日志要求登录，匿名artifact API 403限流、匿名下载路由404，未读取内容。本次runner SDK和失败根因未知，不盲改。
- main `5762c874` 的 R00-03/R00-04 定向行为复核为25/25；baseline freshness现为14 fresh / 0 stale。其他12条记录未命中本轮源路径，原证据保持不变；下一项审查R06-05。R05-07/R06-04仍待CI失败根因。

- 最近完整 clean Release 源码身份为 `4d166f017dcba8fbb07980ec21b690267b9ff377`。SDK `8.0.423` 全量 Release `1560 passed / 57 skipped / 0 failed` 仍绑定该源码身份；后续增量验证按对应源码/测试范围记录，不冒充全量 clean Release。
- 当前身份 RenderHarness 构建 `0/0`、Light/Dark `372` PNG `render-qa OK`、`WorkingTreeClean=True`、offscreen logical DPI `1.00`；R02-08 Save 1040×700 两主题动作标签图与 CloudQueue 页面上下文图已归档。[R02-08 当前 main 复核](evidence/R02-08-CURRENT-MAIN-RECHECK-20261012/README.md)
- 推送 R03-01 证据的 `32c0e1e2` 对应 Actions run `38265390606`（#937，job `114866141338`）终态 failure，8m06s 后“编译与测试”exit1；Node.js 20→24 弃用警告。artifact `GameSaveCenter-build-diagnostics` 478 KB、SHA-256 `31fa1b2bf29661b435ede1c749f854dc358e0e2c2279eb0c72d0a51eb50cd47e`。logs 与 artifacts API均HTTP403，job页要求登录；没有 artifact ID/完整日志，失败根因未知，不盲改。当前 workflow 指向 global.json8.0.423；任务补充中的CI10.0.401未由该run日志证实。
- R04-02 证据提交 `e952c414` 对应 Actions run `38270923668`（#939，job `114884678634`）终态 failure，7m11s 后“编译与测试”exit1；Node.js 20→24 弃用警告。诊断 artifact `GameSaveCenter-build-diagnostics` 478 KB，SHA-256 `660a09c2217195c1e1db44d96887d4f3c674c32f0c02bb3a0385e03dcafc29dc`。run/job logs 与 artifact API HTTP403，job 页要求登录；没有完整日志，失败根因和实际 CI SDK 未证实，不盲改。用户任务补充指定 CI SDK10.0.401；本机 SDK8.0.423 单独记录，不替代该门禁。
- 前一 run #934（`38249983091`，commit `3df542af`）同样失败于“编译与测试”，logs HTTP403、artifact HTTP401；本次不得沿用其当时的 in-progress 状态。历史失败仍需完整日志，不依据 SDK8 结果推断 SDK10 根因。
- R02-06 继续外部阻塞：Playnite 宿主持有原生菜单 UI，ENV-001 条件未变，本轮不重试启动。R02-07 与 R02-08 插件可控行为均已满足；R02-07 复用 `c55f7a9` 下既有 `4/4`，R02-08 当前 main 行为类 `4/4`。详见[R02-06/07复核](evidence/R02-06-07-CURRENT-MAIN-REVIEW-20261012/README.md)。
- run `38274288024`（#940，commit `746eaf6a`）现为终态 `failure`：job `114895863386` 耗时10m30s，步骤“编译与测试”失败，其余前置源码检查、诊断自测、SDK基线成功，后续渲染/静态审查/打包跳过。完整日志 API HTTP403 `Must have admin rights to Repository.`，根因未知，不依据 exit code 猜测修复。
- run `38279606113`（#941，commit `1c39e909`）现为终态 failure，耗时 `8m31s`；run `38281199307`（#942，commit `903525be`）现为终态 failure，耗时 `9m00s`。两页只显示需登录查看 logs；匿名 logs/artifacts API 均返回 HTTP403 rate limit。失败步骤、实际 runner SDK 与根因未确认，不凭摘要或耗时猜测修复。本机 SDK `8.0.423` 与用户要求追踪的 CI SDK `10.0.401` 分开记录。
- main `fbf504d2` 对应 Actions #943（run `38284177732`，job `114928442567`）终态 failure，总耗时 `6m13s`；公开摘要显示“编译与测试”exit1。artifact `11762189156` 为 477 KB，SHA-256 `bb2ffc75a4f111adffa061f46440af303131d14bfd6b52f2d6642b6b9a76176d`。job 页要求登录；匿名 logs/artifacts API HTTP403，失败根因与实际 runner SDK未知，不盲改。[run #943](https://github.com/Nikilua/GameSaveCenter/actions/runs/38284177732)
- main `0fcb44f8` 对应 Actions #944（run `38286603033`，job `114936366123`）终态 failure，总耗时 `7m22s`；公开页面的 workflow job `编译、测试与打包` annotation 为 exit1，artifact `11763647566` 的摘要 SHA-256 为 `575c33f021e762ac35c66c8946733b8fb20acc2ef841d78d60ddbdc91e94a103`。job 页要求登录；匿名 logs/artifacts API HTTP403，具体失败步骤、CI 实际 SDK 与根因未知，不盲改。[run #944](https://github.com/Nikilua/GameSaveCenter/actions/runs/38286603033)
- docs/evidence 提交 `90454586` 对应 Actions #947（run `38295749414`，job `114966280669`）截至 2026-10-12 19:24 UTC 仍为 `In progress`，触发约15分钟；公开页要求登录查看日志，实际 runner SDK/失败步骤均未知。用户指定 CI SDK `10.0.401` 与本机 `8.0.423` 分开记录；未把运行时长当作失败证据。
- 用户补充的历史检查点 `77de7f450431d267026ce2169aa30525b18bf528` 对应 run `36677346317`（#920），公开页面现为终态 `Failure`：7m04s 后“编译、测试与打包”exit1。job 日志要求登录；此前完整日志下载 HTTP403，失败根因仍未知，不盲改。该提交早于当前 main `746eaf6a`，状态不能替代 #940 或后续 run。
- 用户另指定优先查看的失败 run `36675309812`（#919，commit `a6b6dfc`）公开页现为 `Failure`，总耗时13m02s；job 摘要确认“编译与测试”步骤 exit1。展开 job 只显示加载失败/登录提示，完整日志下载 HTTP403；失败根因未知，在取得日志前不据此盲改。
- R03-01 GlyphRun、R04-01 当前 main 输入行为 `37/37`、R04-02 错误导航 `5/5`、R04-03 草稿关闭分支 `12/12` 均已归档。R05-01/02 `11/11`；R05-03 `7/7` 但“翻转无闪屏”无呈现帧、部分满足；R05-04 `3/3`；R05-05 无批量三态 CheckBox 消费点、不适用；R05-06 `28/28`；R05-07 Tooltip/Popup/焦点/源契约当前 main 定向 `34/34`，快速划过负例在前一 tooltip 已打开后仍无重复打开；局部 timing scope 覆盖默认子 owner，显式设置值保留。详见[R05-07当前 main 复核包](evidence/R05-07-CURRENT-MAIN-RECHECK-20261013/README.md)。R06-01 当前 main `afade3cd` 定向 `6/6`，见[当前复核包](evidence/R06-01-CURRENT-MAIN-RECHECK-20261013/README.md)。真实 Playnite、Dashboard 全页宿主、OS UIA/Narrator、物理 DPI/多屏、呈现帧、ETW/宿主性能仍未验。R05-08、R06-01 均保持“已满足”，下一项 R06-02 排序提示与稳定性。

## 2026-10-13 R05-01/02 当前 main 提交后复核

- 已推送 main commit `1c39e909856faa736b32224ee196ef0bac6ca460`。隔离 SDK `8.0.423` 下 Release 定向重建插件 `net462`、测试 `net472`；`R05FocusBoundaryBehaviorTests`、`R05OptionVirtualizationBehaviorTests`、`R21FocusVisualRegressionBehaviorTests` 合计 `11 passed / 0 failed / 0 skipped`。脱敏 TRX SHA-256 `DE45C3F9462F44665BDCD2F068E00630377C413D8669B54F4944F3E988312554`；测试程序集 SHA-256 `9A509F12BBA20BD826ECC3D900DFD21752F0AAA498B9E10A7BE1CF683C52F063` / MVID `8c8895de-7c96-4fe3-bdcb-fc0455c400e4`；插件 SHA-256 `8EB2BCE95E6959F3F4F794B26E1CE565CD59ADEF86F4A5B4EEB29133615E7F33` / MVID `0477b734-38ce-4897-8ea4-6bb904b4d2ec`。ProductVersion 均对应完整 SHA。
- Dashboard 最小 STA WPF fixture 调用生产确认处理器，并以 routed Click 驱动生产取消处理器；验证焦点进入取消按钮、Tab/Shift+Tab 在 overlay 内循环、减少动效即时关闭和 240ms 动画关闭期间 overlay/焦点仍留在层内、关闭后恢复合成触发器；请求完成值为取消。XML 结构检查另核对生产 XAML 的取消/确认 Click 接线，源码结构断言只作接线证据。
- `python scripts/validate-source.py` 通过，`git diff --check` 通过（提交前 PowerShell 显示测试文件 CRLF 提示）。VSTest exit `0`；进程收尾仍打印三条 WPF TextServicesHost/TextStore `InvalidComObjectException` cleanup 消息，原因未知。
- 该 fixture 不加载完整生产 Dashboard XAML、不启动 Playnite，也不证明宿主输入、物理 DPI/跨屏、IME、UIA/Narrator、最终呈现帧、ETW 或宿主性能。#940 已失败且完整日志 HTTP403、根因未知；#941 仍运行，待其终态后更新证据记录并推送，再推进 R05-04。

## 2026-10-13 R05-03 当前 main 边界复核

- 在 main `1c39e909856faa736b32224ee196ef0bac6ca460` 上，隔离 SDK `8.0.423` Release 定向运行 `R05PopupBoundaryBehaviorTests` 与 `R05OptionVirtualizationBehaviorTests`，`7/7`。脱敏 TRX SHA-256 `B3BF918708BF18D6AA8B15A6DDE8962953D2E1084892CF8DA261198C93459427`。
- 当前生产实现实测 720×360 DIP 短窗选框边界、Auto 滚动和选中项视口可见；共享 ComboBox 在当前桌面右下打开后 Popup 起止点处于 `SystemParameters.WorkArea`，高度受限且选定项仍可见。没有改生产代码；未生成 RenderHarness 截图。
- R05-03 明确完成条件还包括“翻转无闪屏”。当前套件只断言 Popup 打开后的工作区几何与列表状态，没有采集翻转动画/呈现帧，因此状态从“已满足”校正为“部分满足”。真实 Playnite、多物理屏/物理 DPI、presented frame 和 ETW/宿主性能仍未验。详见[当前 main 复核包](evidence/R05-03-CURRENT-MAIN-RECHECK-20261013/README.md)。下一项为 R05-04 多选摘要；呈现帧无闪烁仍单独待验。

## 2026-10-13 R05-04 当前 main 多选摘要复核

- 在代码基线 `903525becdb679e9a4aae7bb04e73c98c1cb25dc` 上核对已有 `MediaCenterView` 生产实现。原清空测试只比较 ComboBox 文本，未将生产处理器读取的 `DashboardViewModel.MediaInboxMode` 接入 fixture，也未检查其他模式的稳定 ID 是否保留；本次只补测试夹具与负例，没有改生产 UI/VM/命令/DTO。
- 本机隔离 SDK `8.0.423`，Release solution build `0 warning / 0 error`，Playnite 插件 `net462`、Tests `net472`。定向 `R05MultiSelectionSummaryBehaviorTests` `3/3`：0/1/2 项摘要、隐藏 ID 与当前窗口可操作数、当前“已忽略”模式清空且保留“待归类”选择、筛选模式不变。`validate-source.py` 和 `git diff --check` 通过。
- 脱敏 TRX SHA、测试源码 SHA、插件/测试程序集 SHA/MVID 与完整边界见[R05-04 当前 main 复核包](evidence/R05-04-CURRENT-MAIN-RECHECK-20261013/README.md)。未改生产视觉，无需重跑 RenderHarness；旧双主题截图仍仅属于原 `52900815` 阶段。
- #941（`38279606113`，`1c39e909`）失败、8m31s；#942（`38281199307`，`903525be`）失败、9m00s。job 页要求登录查看日志，匿名 logs 与 artifacts API 403 rate limit；失败步骤、CI 实际 SDK 与根因未知。本地 SDK8 结果不替代用户要求跟踪的 CI SDK10.0.401；不根据这两个 run 盲改。
- 没有真实 Playnite、OS UIA/Narrator/IME、物理 DPI/多屏、最终呈现帧、ETW/宿主性能验证；全部 DTO 与媒体路径为合成夹具。下一项先复核 R05-05 是否仍无实际批量三态复选框消费点。

## 2026-10-13 R05-05 当前 main 消费点复核

- 在 main `fbf504d2a7aabcd322c3c735959a31c7e1679fc9` 检查生产 `MediaCenterView`、共享 CheckBox 模板及仓库三态消费点。媒体收件箱仍是 Extended DataGrid + `SelectedItems` 命令参数；唯一显式 `IsThreeState=True` 样例在开发校对页。共享 `IndeterminateMark` 与 Q10-02 半选视觉不构成批量集合或页/全部结果作用域，故 R05-05 维持“不适用”。
- 本机隔离 SDK `8.0.423`，官方 `scripts/build.ps1 -Configuration Release -SkipTests` 流程：XAML `24/24`、solution `0 warning / 0 error`；`UiFinesseRound2ControlSourceTests` `24/24`，脱敏 TRX SHA-256 `E6BD5F4FDF559CA1589ED45385538908D04052F9314EBFD6C2AB32D0AB7D9BAE`。其中 `SharedCheckboxExposesAnExplicitIndeterminateMark` 是源字符串断言；没有把它写成 Space、UIA 或集合行为证据。各源文件 SHA 与测试边界见[R05-05 当前 main 复核包](evidence/R05-05-CURRENT-MAIN-RECHECK-20261013/README.md)。
- #943（run `38284177732`）已失败，摘要显示“编译与测试”exit1；完整日志与 artifact API 返回 HTTP403，job 页面提示登录查看。实际失败命令/断言、CI SDK 与根因未知，故无盲目生产修复。本机 SDK8 结果不替代待核 CI SDK10.0.401。
- 未运行真实 Playnite或 OS Space/UIA/Narrator；未做物理 DPI/多屏、呈现帧、ETW 或宿主性能验证。没有触碰真实存档、媒体、云端或用户配置。下一项：R05-06 当前 main 开关模型/视觉/持久化/回滚行为复核。

## 2026-10-13 R05-06 当前 main 开关保存语义复核

- 在 main 生产代码基线 `0fcb44f851257504d867401dab92a74b3807899a` 复用既有设置 DTO、设置页、ToggleSwitch 模板、Portable JSON 与 R04-08 保存反馈状态机；生产实现无需重建。本轮只在 `R05TogglePersistenceBehaviorTests` 补同值重复赋值负例：真实开关改为关闭产生一次 `PropertyChanged`；再次赋相同 false 不增加计数、不改变 Track Brush 或 dirty 提示；`CancelEdit` 改回 true 后计数只增加一次且提示恢复已保存。
- 本机隔离 SDK `8.0.423`，官方脚本 Release solution `0 warning / 0 error`、XAML `24/24`；`R05TogglePersistenceBehaviorTests 1/1`、`SettingsSaveFeedbackTests 2/2`、`SettingsDraftLifecycleBehaviorTests 1/1`、`UiFinesseRound2ControlSourceTests 24/24`，总计 `28/28`。当前脱敏 TRX、源码/程序集 SHA 和测试身份边界见[R05-06 当前 main 复核包](evidence/R05-06-CURRENT-MAIN-RECHECK-20261013/README.md)。
- 受控 WPF 用例覆盖值/显示/Portable JSON 快照/回滚、依赖面板启停、写入失败与 Worker apply 失败原因状态、视图 detach/reattach、关闭“否/是”与焦点保持。现有双主题 RenderHarness 仍绑定 `cf9a250f`，没有冒充当前 main 像素证据；本次只验证当前默认主题 Track Brush 状态，不声称像素或最终呈现帧。
- Actions #944（run `38286603033`）已终态 failure，公开摘要指向“编译与测试”exit1；日志和 artifact API HTTP403，失败命令/断言、CI SDK和根因未知。本机 SDK8 不替代待核 CI SDK10.0.401，不盲目改生产代码。
- 合成设置/隔离临时路径，无真实配置写入；未运行真实 Playnite/原生 MessageBox/Worker 故障注入，OS UIA/Narrator/IME、物理 DPI/多屏、呈现帧、ETW与宿主性能仍待验。下一项 R05-07 Tooltip 时序当前 main 复核。

## 2026-10-12 R00/R01 当前证据复核

- 在 main 提交 `f13b9e05de0cf502ac61e97b0fbad8433b339b3f` 上复核了 10 条 stale 源码关联记录：定向 TRX `40/40`，RenderHarness 构建 `0 warning / 0 error`；浅/深主题对比和数字可读性、工具栏正常/超宽/不可达负例、Media Inbox 双主题几何、R00-04 30 个不同查询与超时负例均有当前身份的实际证据。原始 TRX、探针和性能样本见 [当前 R00/R01 复核包](evidence/R00-R01-CURRENT-RECHECK-20261012/README.md)。
- `UI_EVIDENCE_BASELINE.json` 的 freshness 现为 `14 fresh / 0 stale`：10 条命中当前源码变化的记录指向本复核包；4 条没有关联源码路径命中，保留其原证据身份。包未生成，package identity `not-provided`；freshness 不表示包、真实宿主或整项 192 任务已验收。
- 对应 Actions run `38222818374`（commit `f13b9e05`，SDK `10.0.401`）终态为 failure，步骤“编译与测试”失败。本机 SDK `8.0.423` 与 CI 基线分别记录。workflow logs 下载 HTTP 403 `Must have admin rights to Repository.`，诊断 artifact 下载 HTTP 401；根因未知，不根据退出码猜测或盲改。真实 Playnite、Media Inbox 同进程滚动、IME/UIA、物理 DPI/跨屏、ETW、最终呈现帧和 package-host 仍未验。
- `CLOSE-EVID-01` 的源码证据复核与校正已收口；CI/SDK、宿主和发布身份仍各自开放。下一步在保留 CI 日志权限限制的同时，推进活动队列中不依赖这份日志的 Q/R 小批量。

## 2026-10-12 R02-03 当前 main WPF/UIA 复验

- R02-03 原有生产状态提示、命令门禁、共享样式和 Save 紧凑检查器布局仍在。只加强了测试：Focus 后核对 `Keyboard.FocusedElement`，并从 UIA peer 读取 Name/HelpText；实际生产 `SaveCenterView` 宽态/紧凑态提示可见性行为一并覆盖。定向 `5/5`，Release `0 warning / 0 error`、XAML `24/24`，本机 SDK `8.0.423`。本次没有生产 UI/VM/命令改动，账本 R02-03 状态保持“已满足”。
- RenderHarness 另建隔离输出，Light/Dark 通用全量 `render-qa OK`、372 PNG、logical DPI `1.00`。该默认样例没有强制禁用原因状态，故不把其截图当作 R02-03 disabled-state 直接视觉证明。WPF 静态审查 `36 XAML / 0 errors / 29 warnings / 177 info`；`validate-source.py` 通过。测试 TRX、dirty 测试文件 SHA、DLL SHA/MVID 与范围边界见 [R02-03 当前复验包](evidence/R02-03-CURRENT-RECHECK-20261012/README.md)。
- 推送提交 `da6388e8` 的 run `38226955461`（run931）终态 failure 于“编译与测试”，CI SDK `10.0.401`。完整日志 API 返回403，artifact `11735869371` 下载401；根因未知。没有真实 Playnite、package-host、OS UIA/Narrator/IME、物理 DPI/跨屏、ETW 或最终呈现帧验证。下一步 R02-04 当前 main 图文光学对齐复验。

## 2026-10-12 R02-04 当前 main 图文光学对齐复验

- 当前 main `a21440834865551cdb6894830548af51ad1d8e78` 沿用已有生产共享按钮修复，本阶段未改生产 UI。Release `0/0`、XAML `24/24`；`R02OpticalAlignmentTests 7/7`。真实生产 ResourceDictionary 的 STA 几何输出：16/20 DIP图标与数字 gap 各8 DIP、中心差各0；中英文字基线、三种对齐切换、文字/视觉内容往返通过。本机SDK8.0.423。
- 当前身份 RenderHarness双主题全量 `render-qa OK`，372 PNG、logical DPI `1.00`；已人工对照 Save 1040×700 Light/Dark，两张截图随[当前复验包](evidence/R02-04-CURRENT-RECHECK-20261012/README.md)归档。`validate-source.py`通过，WPF静态审查36 XAML/0 errors/29 warnings/177 info。离屏逻辑DIP不代表真实宿主或物理DPI。
- run `38244963620` 对应当时提交，已失败于“编译与测试”；步骤注释仅 `Process completed with exit code 1`，CI SDK10.0.401。未认证API rate-limit 403，artifact `11745496487` 下载404；失败根因未知。R02-04仍“已满足”，CLOSE-OPTICAL-01整体门禁/CI未收口。R02-05当前main复验后，下一项R02-06菜单宿主边界。

## 2026-10-12 R02-05 当前 main 命中区与间距复验

- 当前 main `90099a09104677708a5829cf7e6a5d22b6b414ff` 使用既有共享密度资源，没有改生产UI。Release `0/0`、XAML `24/24`；`R02HitAreaSpacingTests 2/2`。STA实际视觉树HitTest确认复制/删除中心落在各自Button，6 DIP空隙不命中且两个矩形不重叠；82 DIP窄WrapPanel中复制/删除保持同排、第三动作换行，三个中心各命中对应Button。
- 当前身份RenderHarness双主题 `render-qa OK`、372 PNG、logical DPI `1.00`。已检查并归档Media 1040×700 Light/Dark；`validate-source.py`通过，WPF静态审查36 XAML/0 errors/29 warnings/177 info。截图只证明离屏布局，未验OS输入、Playnite或物理DPI。详细TRX/身份见[R02-05当前复验包](evidence/R02-05-CURRENT-RECHECK-20261012/README.md)。
- run `38247748056`（#933）是该源码复验提交的推送结果，已失败于“编译与测试”，完整日志未取得；最新 docs-only HEAD `3df542af` 对应run `38249983091`（#934），状态见本页顶部。根因未知。R02-05继续“已满足”。R02-06/07的当前源边界见顶部复核包。

## 2026-10-05 完成度复核（历史统计）

正式8列表192个唯一R ID，表前10条R13/R14摘要不重复计数。按本轮行状态校正，状态归纳为：受控满足104、环境待验83、用户失败未定位1（R08-01）、外部阻塞1（R02-06）、部分满足2（R05-03、R23-05）、不适用1（R05-05）。旧106/83/1/1/1把R08-01归入受控满足；本轮继续单列用户故障。R05-03 的几何/选定项行为有证据，“翻转无闪屏”呈现帧边界尚未验证。

该日期核算时 R00/R01 freshness 为 9 fresh/5 stale；已由 2026-10-12 的当前源码复核更新为 14 fresh/0 stale。历史状态汇总不证明证据新鲜或整项宿主验收。当前活动队列见 [活动积压清单](../../../AUTONOMOUS_BACKLOG.md)，统计与实际构建失败见 [完成度评估](../../../FEATURE_COMPLETION_ASSESSMENT.md)；下方阶段“下一项”为历史记录。

## 2026-09-30 窄窗换行行距上调

- 生产提交 `af915a2d637dc4901e1c81e2e2cdd097c958a002` 将实际折行操作组间距从 `16` 加到 `20 DIP`，只在窄窗折行时启用，恢复宽态原 margin；当前确认组包括存档历史、媒体批量/次级动作、任务筛选预设和 shell 动作。
- 提交后 `ReportedWorkspaceLayoutBehaviorTests 46/46`、`Q14ToolbarAlignmentBehaviorTests 1/1`、R00-06 几何/锚点 `13/13`；Release solution `0 warning / 0 error`、XAML `24/24`、source validation 通过，RenderHarness clean-tree `render-qa OK`。R00-06 因 `MediaCenterView.xaml.cs` 命中重新测量并更新了当前证据入口。
- [间距、截图、脱敏 TRX 和程序集身份](evidence/USER-REPORTED-COMPACT-ROW-GAP-20260930/README.md)。离屏逻辑 DIP / 合成 WPF 不代表 Playnite 或物理 DPI；Media Inbox 实机滚动偏移仍待安全宿主同进程诊断。下一项继续检查可见折行组与有数据时的冗余说明。

## 2026-09-30 Trainer 可下载版本页说明精简

- 提交 `22c55647799e732a770e6b16a93af4a9e83056f6` 缩短搜索结果副标题，移除已加载版本标题旁的常驻实现说明，并把操作提示放入 Tooltip/UIA HelpText。空态仅在零版本且非加载时出现；数量、真实命令、DTO 和列表虚拟化保持不变。
- 当前提交 Release `0/0`、XAML `24/24`；关联 WPF/辅助功能行为 `58 通过 / 1 跳过 / 0 失败`，其中布局行为 `46/46`；UiAuditSource `6/6`、source validation 通过。`render-qa OK`，截图显示“可下载版本 · 8 个版本”标题行与已加载列表。[证据](evidence/USER-REPORTED-TRAINER-COPY-CURRENT-MAIN-RECHECK-20260930/README.md)
- TrainerCenterView 命中 R00-07 来源路径后，按当前身份重新跑了正常表单、同祖先超宽工具栏、不可达工具栏三场景 toolbarprobe，结果 `OK`。更新 R00-07 baseline 后 freshness `14/14 FRESH`；package identity 未提供。受控离屏逻辑 DIP，不代表真实 Playnite 或物理 DPI。
- 下一项继续逐页检查有数据时重复的常驻说明。Media Inbox 同宿主滚动几何和 R08-01 用户侧失败原始断言/堆栈仍待验证；192 项状态不变。

## 2026-09-30 活跃状态颜色语义校正

- 按实际 DTO 状态核对并修复三类通用 Info 蓝：Overview 云队列 tone 按认证/校验失败/上传失败、重试等待、传输/远端校验、已上传/验证和普通待办区分；Maintenance 环境检查总摘要与单项 glyph 按真实检查结果区分；Save Center 比较精度标签改为精确中性、估算警示、无效 Manifest 错误。
- 不改 DTO、命令、队列/环境/比较业务流程。Release `0 warning / 0 error`，XAML `24/24`，source validation 通过；实际 Light/Dark WPF 行为 `ReportedWorkspaceLayoutBehaviorTests 44/44`，状态 resolver + Typography diagnostics `13/13`；技能静态检查 `0 errors / 30 warnings / 177 info`；R00/R01 freshness `14/14 FRESH`，freshness 自测通过。
- 提交后 RenderHarness `render-qa OK`，报告记录 `e27eab0465714dbfc9b75aa5006916460bfcfce7`、`WorkingTreeClean=True`、离屏逻辑 DPI `1.00`；插件/测试/RenderHarness 程序集身份、精选截图、脱敏 TRX 和完整报告见[复核证据](evidence/USER-REPORTED-STATUS-TONE-CURRENT-MAIN-RECHECK-20260930/README.md)。离屏结果不是实际 Playnite、用户安装 DLL 或物理 DPI 验证；此批不改变 192 项总数/状态。
- 不将颜色样例扩展成 R03-02 或 R22-08 整组完成，不改 192 项总数/状态。没有真实 Playnite、用户 DLL 或物理 DPI 复核。仍待用户同进程 Media Inbox 滚动诊断；R08-01 用户机失败需原始方法/断言/堆栈/DLL 身份。
- 下一可执行项：按数据态清单继续缩短与空态/标题重复的常驻说明。

## 2026-09-30 用户缩窗布局与 R00/R01 证据复核

- 生产代码提交 `4ebbf81f63882a0b1c1e9e02ee214069bb552596` 将已确认会折行的存档/媒体/任务/shell 操作组间距统一为 `16 DIP`，只在紧凑区间启用并恢复宽态 margin；shell 单列标题断点由 `<1280` 调至 `<1440 DIP`。休眠的折叠 `DashboardDemoShell` 不计入当前可见区域。
- Task 队列的加载/更新时间移至动作行下方、移除重复筛选摘要；Light/Dark 实测主行 `36 DIP`，三行摘要加长后不变，重试/重置为 `30/36 DIP`。Cloud Transfers 标题/计数胶囊相对行中心偏差 `≤0.33 DIP`。
- 提交后 Release solution `0 warning / 0 error`、XAML `24/24`、source validation 通过。隔离生产 WPF 行为 `ReportedWorkspaceLayout 42/42`、Q14 toolbar `1/1`、Responsive coordinator `5/5`、Task retry `2/2`；R00-04 搜索 `5/5`（30/30 结果集合改变，p50/p95/max `47/49/49 ms`）、R00-06 几何/锚点 `13/13`、R00-08 键盘/焦点 `11/11`、R01-05 负例 `1/1`。R00/R01 freshness `14/14 FRESH`，旧证据身份按本次确切测试和程序集刷新。
- 提交身份 `4ebbf81f...` RenderHarness clean-tree 双主题全矩阵 `render-qa OK`；离屏逻辑 DPI `1.00`，不是真实 Playnite/物理 DPI。TRX、精选截图、DLL SHA/MVID、报告和未验边界：[紧凑布局当前 main 复核](evidence/USER-REPORTED-COMPACT-LAYOUT-CURRENT-MAIN-RECHECK-20260930/README.md)。
- 用户提到的蓝色说明现以主题次级/Muted 资源显示，语义 Info 状态仍使用 Info 色；参考 Microsoft Windows Color 指引。此批不表示全产品颜色审计完成。没有安装/启动真实 Playnite，Media Inbox 实际滚动错位仍需同一隔离宿主 `[GSC-GRID-DIAGNOSTIC]` 前后日志；192 项状态不变。
- 下一可执行项：继续逐页核对有数据时冗余说明和活跃 InfoBrush 的语义，随后推进依赖已满足的 Q/R 小批；Media Inbox 同宿主诊断、R08-01 用户失败原始 log/TRX 保持独立待验。

## 2026-09-30 R08-01 当前 main 反向动画复测

- 生产源码身份 `dc7f97cf` 的 Release `R08MotionReverseBehaviorTests` 本机复测 `2/2`，0 failed/skip、VSTest exit `0`；当前复测未重现用户 `1/2`。TRX 成功后含 1 条 `TextServicesHost.OnUnregisterTextStore InvalidComObjectException` 清理记录，根因未知。
- 失败机器仍未提供失败方法、断言、堆栈和对应 DLL 身份；不据本机通过宣称用户问题已修复。插件/测试 SHA、MVID、命令和边界见[证据](evidence/R08-01-CURRENT-MAIN-RECHECK-20260930/README.md)。
- 下一阶段处理用户最新的跨页面紧凑窗口审计；真实 Playnite/DPI 与 Media Inbox 滚动诊断的既有边界不变。
## 2026-09-30 R08-02 当前 main 热关闭动画复核

- 生产源码 `dc7f97cf`；当前 Release `R08MotionHotChangeBehaviorTests 1/1`、exit `0`，Light/Dark RenderHarness 生产侧栏运行中关动画 probe 均通过。应用选项关/开验证入口状态复原且不重播；侧栏动画可即时停止、禁用态重入直接到终点。
- Dashboard/Settings 已订阅 `SystemParameters.StaticPropertyChanged`，更新主题并归一化活动动效；本次不改变 Windows 系统动画偏好，也不声称真实系统通知/Playnite 宿主已验。TRX 成功后有 8 条 TextServices COM 清理输出，根因未知。详细身份、探针读数与 6 张精选状态图：[R08-02 当前复核](evidence/R08-02-CURRENT-MAIN-RECHECK-20260930/README.md)。
- 下一可执行工作：按用户 2026-09-30 截图继续跨页审计紧凑换行行距、缩窄标题、辅助说明密度和主题次级文字。R08-01 本机复测未复现用户 `1/2`；仍需失败机器原始 log/TRX。Media Inbox 滚动错位仍需同一安全 Playnite 进程诊断。
## 2026-09-30 R00/R01 当前审计证据与账本校正

- 发现进度表中 R00-06、R00-07、R01-03、R01-06 仍指向 2026-09-28 的 `103 warnings / 0 HIGH / 0 MEDIUM`，与 2026-09-30 当前 audit 不符；已依据 `dc7f97cf` clean-tree 归档把这些行校正为 `168 snapshots / 110 warnings / 0 HIGH / 7 MEDIUM / 0 Fidelity / 0 failed routes`。实际 7 条 MEDIUM 工具栏垂直扩展提示保留。
- 按已提交的 post-commit TRX/原始样本刷新 R00-04/06/08 与 R01-05/06/07 行的身份和证据入口；R01-03/06 当前索引 `20/20`，`UiAuditSourceTests 6/6`，R00/R01 freshness `14/14 FRESH`。不改 192 项状态或总数。
- 归档 `UI_MANIFEST.json` 的绝对仓库根路径和 `audit-metadata.json` 指向已清理 `.tmp` 的 ZipPath 已移除，保持 repo-relative 源码引用和当前提交身份；归档文档明确保留逻辑 DIP/真实宿主未验边界。
- 这是证据/账本纠正，无生产代码改变，也未重新声称 Playnite/物理 DPI 已验。下一项核对 R08-02 关闭系统/应用动效开关时的运行时契约；R08-01 用户侧失败仍待原始 log/TRX。
## 2026-09-30 用户窄窗行距与紧凑标题单列

- `dc7f97cfa49724778c4987224c9f736c744b3631` 已推送 main：所有确认会换行的存档/媒体/任务/shell 操作组采用 `12 DIP` 行距，只在 compact 时启用并在宽态恢复；compact shell `<1280 DIP` 标题行在上、全局游戏选择/操作在下并横跨同一列。用户主备份图标与按钮文字统一使用 on-accent 色。
- Release `0/0`、XAML `24/24`、source validation 通过；定向布局 `13/13`。R00-04/06/08 与 R01-03/05 stale path 关联行为总计 `55/55`；baseline evidence/source 身份更新后 freshness `14/14 FRESH`、自测通过。当前 clean commit RenderHarness 多主题/多尺寸 `render-qa OK`；audit 索引 `20/20`，摘要仍保留 `7 MEDIUM` 垂直扩展提示。
- 独立证据：[紧凑布局与颜色](evidence/USER-REPORTED-COMPACT-SPACING-STACKED-HEADER-20260930/README.md)、[R01-03/06 当前 audit](evidence/R01-06-controlled-audit-20260930/README.md)。现有说明密度、任务按钮高度与传输 pill 在同批测试复核，但不表示所有页面文案或色板完成。
- TRX 在 Media Inbox 几何/锚点与 Q14筛选测试后记录 `InvalidComObjectException` 清理噪声，分别 `2`/`18` 条；xUnit 仍 `13/13` 与 `1/1`、exit `0`，原因未知。离屏/受控 STA 逻辑 DIP 不是 Playnite、物理 DPI、OS 输入或最终呈现验证；Media Inbox 滚动偏移依旧等安全同进程 `[GSC-GRID-DIAGNOSTIC]`。Demo DesignShell 源目录缺失，沿用恢复生产基线；192 项账本未改。
- 下一项继续逐页审计剩余可见 WrapPanel 行距和有数据时辅助文案，之后推进依赖已满足的 Q/R 小批。

## 2026-09-30 Overview 首页云端队列语义色

- 提交 `74ab0203f77b59ca4c36e749025884d7f2dcc253` 将 `OverviewCloudQueueValue` 数量指标改为 `GscAccentBrush`；`Cloud` 优先事项明确代表失败、认证或重试等待，改走 warning；云端状态胶囊保留 info。按钮命令、Tooltip/HelpText、DTO 和队列行为没有改动。
- Light/Dark 生产 WPF 合成 `FailedCount=4` 行为 `2/2`：可见计数 `4`、priority title/warning、cloud status/info 与队列命令均逐项验证；对比度最低值 Light `4.13/4.13/4.72:1`、Dark `4.77/4.77/7.66:1`。Release solution `0/0`，XAML `24/24`，UiAuditSource `6/6`，source validation 与 diff check 通过。
- 提交后 RenderHarness 双主题完整矩阵 `WorkingTreeClean=True` / 离屏逻辑 DPI `1.00` / `render-qa OK`；R00/R01 freshness `14/14 FRESH`。插件/测试 DLL SHA、MVID、TRX、截图及限制：[证据](evidence/USER-REPORTED-OVERVIEW-CLOUD-SEMANTIC-COLOR-20260930/README.md)。最终隔离测试没有观察到 WPF `InvalidComObjectException` 清理噪声。
- 本批使用合成队列状态，没有联网、读写真实存档或启动 Playnite；没验证用户安装 DLL、物理 DPI 或真实宿主渲染。这只覆盖 R03-02/R22-08 的一个用例，不改变 192 项状态。下一项逐实例审查活跃 InfoBrush，先分清语义状态与装饰色；Media Inbox 表格滚动错位继续等待安全同进程诊断。

## 2026-09-30 Maintenance 云端队列摘要精简与计数主题色

- 统计卡删去“待上传与排队中的本地任务”“只读远端校验正在执行”“认证、失败或等待重试”三条常显说明；原信息移至标题 Tooltip/UIA HelpText，远端只读/不覆盖本地存档语义保留。待处理和校验数字改用主题紫色强调，需处理仍用警示色。
- Release `0 warning / 0 error`、XAML `24/24`、WPF 数据/主题/高度行为 `1/1`、RenderHarness 全矩阵 `render-qa OK`、source validation 通过，R00/R01 freshness `14/14 FRESH`。[双主题截图、TRX、程序集 SHA/MVID](evidence/USER-REPORTED-CLOUD-SUMMARY-COLOR-DENSITY-20260930/README.md)
- 这是云端队列一个界面实例的修整，不签收 R03-02/R22-08 整组且不改 192 项计数。未启动真实 Playnite或验证物理 DPI。下一项逐页审查剩余数据态静态说明与状态色；Media Inbox 实际滚动空白仍须安全宿主同进程几何日志。

## 2026-09-29 用户表格排序崩溃与选中行复核

- 用户提供的 `crash.zip` 日志显示 `ListCollectionView.Refresh` 经 `DataGridStableSortController.ApplyCurrentSort/ToggleSort` 到真实列头点击时发生 `NullReferenceException`；包内没有 DLL/SHA/MVID，原运行实例身份未知。已有 `f8a82469` 修复 detached CollectionView 排序路径。
- 当前 `10626efc` Release `0 warning / 0 error`、XAML `24/24`；排序 `7/7`、选中行几何 `2/2`、Save/Task/Media/Maintenance 四网格浅深主题状态 `1/1`，合计 `10/10`。当前插件/测试 DLL 身份、TRX 和限制见[复核证据](evidence/USER-REPORTED-DATAGRID-CRASH-ROW-STATE-RECHECK-20260929/README.md)。没有改源码或 192 项状态。
- 本次只验证当前合成 STA WPF 路径，未启动 Playnite 或还原用户原 DLL/主题/物理 DPI。下一项继续跨页面审计有数据时冗余说明与短窗口控件密度；Media Inbox 滚动空白仍需安全宿主同进程诊断。

## 2026-09-29 用户报告：Maintenance 诊断空态文案

- `4a834135` 把诊断页和异常审计页重复的两行空态收成“暂无待处理诊断项。”，避免零条问题被当作备份和媒体健康证明；零条仍显示，有数据时折叠。Light/Dark 生产 WPF 两处各做 `Visible → Collapsed → Visible`，TRX `2/2`。[证据](evidence/USER-REPORTED-MAINTENANCE-EMPTY-DIAGNOSTIC-COPY-20260929/README.md)
- 当前提交 Release solution `0/0`、XAML `24/24`、source validation 通过；R00/R01 freshness `14/14`，未命中登记源路径。恢复保护、刷新/错误/离线和取消语义未改；不改 192 项完成数。
- 未启动真实 Playnite、未验物理 DPI或用户截图 DLL。下一项逐项检查存档/维护页的数据态辅助说明，保留安全和故障解释；Media Inbox 滚动偏移仍需宿主同进程日志。

## 2026-09-29 用户报告：Media Inbox 有数据时收起策略说明

- `1e64347d` 将策略说明缩为“归属不明的媒体会留在待归类中，不会自动猜测。”；只在空的待归类列表显示。有数据或切到“已忽略”时折叠，完整说明转挂到常驻标题的 Tooltip/HelpText。Light/Dark WPF 实测说明行 `0/18/0 DIP` 往返；信息带总高仍由数量胶囊主导，变化仅 `0.67 DIP`，未将其误记成整卡缩高。
- 提交后 Release `0/0`、XAML `24/24`；策略状态 `2/2`、MediaInboxGeometry `3/3`、MediaWindowAnchor `10/10` 共 `15/15`。RenderHarness 全矩阵绑定当前提交 `1e64347d`，工作树干净，逻辑离屏 DPI `1.00`、`0 PROBLEM`、`render-qa OK`。包含数据的 `1040×700 DIP` 屏幕图和 TRX：[本次证据](evidence/USER-REPORTED-MEDIA-INBOX-POLICY-COPY-20260929/README.md)。
- `MediaCenterView.xaml` 命中 R00-06 基线路径；提交后刷新只剩该条需复核，完成相关 15 项复测后把 R00-06 `sourceCommit` 更新到本提交，最终 `14 FRESH / 0 STALE`。没有改变 R00-06 的“已满足”状态或 192 项计数。
- TRX 有两条测试完成后的 WPF `TextServicesHost.OnUnregisterTextStore` `InvalidComObjectException` 清理噪声；xUnit `15/15`、exit `0`，根因未知。未启动真实 Playnite、未验物理 DPI；用户截图运行 DLL/MVID 仍未知。真实滚动行偏移继续等待安全宿主同进程 `[GSC-GRID-DIAGNOSTIC]`。
- 下一项按用户的全页文案要求，核对存档/维护页数据存在时的解释文字；保留恢复保护、错误、取消与危险操作的必要安全语义。
## 2026-09-29 用户报告：Trainer 导入确认空态文案

## 2026-09-29 用户报告：Media Inbox 局部游戏选择器（已满足）

- 核对当前代码确认已有 `4f778e9b` 实现：待归类批量操作只读显示全局 `SelectedGame`，没有重复 ComboBox；批量命令使用同一目标 Playnite ID。现存摘要 `220×36 DIP` 用于确认上下文，不创建第二份可编辑状态。
- 当前最近源码构建 `f88bcb4d` 的 Light/Dark 生产 WPF 行为 `2/2`：全局目标切换、清空、恢复同步到操作区与 Inspector；按钮/摘要 `36 DIP` 高且垂直中心差 `0 DIP`。`src/` 与 `tests/` 自该源码身份至当前 HEAD 无差异。[证据](evidence/USER-REPORTED-MEDIA-INBOX-GLOBAL-TARGET-20260929/README.md)
- 未启动 Playnite；用户截图运行 DLL/主题/DPI 未知。该确认不覆盖 Media Inbox 滚动行偏移，它仍待同进程前后诊断。
- 下一项继续其它页面的紧凑动作行距/静态辅助说明审计，不改 192 项计数。

## 2026-09-29 用户报告：Task Center 提示和按钮（已满足）

- 对照当前生产 `TaskCenterView` 与已有能力后确认无需重建：运行时显示短句，空闲时折叠；完整后台任务不中断/恢复提示保留在 UI Automation HelpText。任务列表按钮已采用固定垂直尺寸，摘要换行不再撑开按钮。
- 当前身份 `f88bcb4d` Release `0 warning / 0 error`、XAML `24/24`、行为 `4/4`。Light/Dark 摘要高 `58 DIP`；按钮高 `30/36 DIP`、垂直中心差 `0/0.33 DIP`；折叠说明后高度不变。辅助文字解析为次级文字色，非 Info 蓝。[证据](evidence/USER-REPORTED-TASK-CENTER-HELPER-20260929/README.md)
- 先前 `98800f1a` 测试程序集被源码身份门禁拒绝，已作废；本结果来自当前 `f88bcb4d` checkout 的新 Release 构建。未安装/启动真实 Playnite，用户截图 DLL/主题/物理 DPI 身份未知。
- 下一项核验 Media Inbox 批量操作是否仍有重复游戏选框；不改变 192 项状态。Media Inbox 真实滚动空白仍需同进程诊断。

- 源码提交 `98800f1a7cb27bcb6f92c190c7feb5cf3d388c94` 将静态“没有待确认项目”提示绑定为空态：选择待确认 EXE 后提示收起，选择器、选项、确认/取消按钮和真实命令不变。Light/Dark 生产视图行为 `3/3`。
- 提交后 Release `0 warnings / 0 errors`、XAML `24/24`、`UiAuditSourceTests 6/6`、source validation 通过。compact `904×520 DIP` 与 wide `1596×840 DIP` 几何数据记录工具栏 `44 DIP`、可达且无溢出；R00-07 freshness 更新后 `14 FRESH / 0 STALE`，package identity 未提供。
- 离屏几何、TRX、插件/测试程序集身份与限制：[Trainer import copy](evidence/USER-REPORTED-TRAINER-IMPORT-COPY-20260929/README.md)。没有启动真实 Playnite、测试物理 125%/150% DPI 或最终呈现；192 项账本计数和状态未变。
- 下一项继续用户要求的页面文案审查：核对 Task Center 中离页任务提示与队列说明；保留后台任务继续执行的安全语义，只减少重复辅助文字和非语义信息色。Media Inbox 同进程滚动诊断仍未验。
## 2026-09-29 概览空状态文案与语义颜色

- “今日工作台”未加载时显示短提示；快照加载后自动收起，状态变回未加载时恢复。生产 WPF Light/Dark 回归测得 header `112→92 DIP`，三条刷新/备份/同步命令仍绑定。
- 首页“已管理游戏”由信息蓝改成主题强调色；维护动作分类由信息蓝改成主题次级文字。对比度夹具按实际主题表面采样，浅/深色分别为 `4.13/4.77:1`（大号指标，门槛 3:1）、`6.43/7.86:1`（普通文字，门槛 4.5:1）。
- Release solution `0/0`、XAML `24/24`、定向 WPF `8/8`、source validation 通过；RenderHarness 双主题全矩阵 `render-qa OK`。证据和图片：[Overview Copy/Color](evidence/USER-REPORTED-OVERVIEW-COPY-COLOR-20260929/README.md)。没有启动 Playnite 或验证物理 DPI/最终呈现；Media Inbox 同进程滚动诊断仍未完成。
- 没有改动 192 项账本状态或计数。下一项盘点其他可见页面的数据态静态说明，只隐藏冗余行，不删必要动态说明和真实空状态。

## 2026-09-29 Q14-03 Task Center 搜索宽度

- 当前源码提交 `5559b0fbc9eba2c14207660628860a6f44b30571` 将紧凑筛选断点调整为 `<1216 DIP`，修复 980–1215 DIP 搜索 viewport 和状态筛选布局争用；保留现有更多筛选折叠入口、绑定、清除与刷新行为。
- 双主题 7 档搜索尺寸、旧布局 1040 DIP 负例、完整搜索/状态/刷新边界均通过；测试 DLL 内含真实生产 `TaskCenterView`，三个 Release TRX `18/18`。RenderHarness 全矩阵 `render-qa OK`。证据：[Q14-03 搜索宽度](evidence/Q14-03-SEARCH-WIDTH-20260929/README.md)。
- R00/R01 freshness 对当前代码仍为 `14 FRESH / 0 STALE`，本次 TaskCenter 源/测试路径未命中既有记录。真实 Playnite/物理 DPI/OS 输入/屏幕呈现未验证，Round2 Q14-03 保留外部阻塞/未完成。
- 下一项按用户要求继续审计跨页面窄窗 WrapPanel 行距，避免对未触发行换行的组做机械修改。Media Inbox 宿主滚动错位的同进程诊断仍未满足安全启动条件。

## 2026-09-29 当前 R00/R01 复核收口

- 对 clean main 源码身份 `9c906cc0772aad06143bdf3237255effd417e2de` 完成首轮 freshness 命中复测：R00-01/02、04、05、06、08 与 R01-03、05 的关联证据 `39/39` 通过；Release `0 warning / 0 error`、XAML `24/24`，双主题 RenderHarness `render-qa OK`。
- 重新生成的 R01-03/R01-06 audit 在 `R01-06-controlled-audit-20260929`，索引 validator `20/20`。当前 audit 168 snapshots、110 warnings（7 HIGH / 4 MEDIUM）、0 Fidelity、0 failed route。风险继续保留。
- `R01-07` 扫描 14 个登记项为 `14 FRESH / 0 STALE`，报告中 source `9c906cc...` 和独立 package 输入 `fc58264...` 分开；R01-08 未登记于 freshness baseline，本轮不冒称重跑全量 skip 分类。
- R00-06 的 WPF testhost 自报 DPI `1.5×1.5`；测试 RenderTransform `1.25/1.5` 仅为布局尺度。真实 Playnite 内 Media Inbox 滚动错位仍需同进程 DLL/MVID、DPI/主题/窗口 DIP 与滚动前后行几何；系统跟踪/进程访问被拒绝时不绕过。
- 本次 R00/R01 收口后的下一项为 Q14-03；该项现已完成本地行为补证与最小修复，见本文件上方记录。

## 2026-09-29 用户报告：多页紧凑窗口行距、说明密度与控件对齐

- 补充处理存档历史、媒体待归类和外壳动作 WrapPanel 的 compact 行距：运行 WPF 实际测量两种主题下净行距 `8 DIP`，宽态还原原 margin。compact 外壳标题与全局游戏选择/操作同一行，隐藏副标题并将完整摘要保留在 Automation HelpText。
- Task Queue 实测 `58 DIP` 摘要不再拉伸 `30/36 DIP` 的重试/重置动作按钮；文字摘要从信息蓝改用动态次级文字，仅在有运行任务时显示离页提示；传输明细 pill/text 中心差 `0.33 DIP`。安全、命令、过滤和绑定语义不变。
- 定向 WPF 几何 `8/8`、离页语义 `2/2`；Release build `0 warning / 0 error`、XAML `24/24`、source validation 通过；WPF skill checker `0 error / 30 warning / 177 info`，本批无新增 error。RenderHarness 的旧第二行假设更新为一行/矩形交叠门禁后，完整矩阵 `render-qa OK`、`PROBLEM=0`。[证据、TRX 和截图](evidence/USER-REPORTED-COMPACT-LAYOUT-20260929/README.md)。
- clean `fc58264e` 隔离 Release package/install 通过：XAML `24/24`、solution `0/0`、Core `125/125`、Worker `356/357`（1 个既有进程重启 skip）、Playnite source `111` 类/WPF 隔离 `113/113` 类；插件和 Worker 的 SHA/MVID 记录在本批证据中。文件只安装到仓库 `.tmp`，没有启动真实 Playnite。此补充阶段不对应 Round2 的单一 Q 行或 R03 全组签收，Q/R ledger 数字保持不变；125%/150% 物理 DPI、OS 输入或用户资料目录写入未做。
- 下一阶段按最新目标先在当前 main 核对 R00/R01 freshness，仅对命中本批路径的项目补证或标记“已满足”；之后推进依赖已满足的 Q/R 任务。Media Inbox 滚动问题继续等待同进程 `[GSC-GRID-DIAGNOSTIC]`。

## 2026-09-29 用户报告：Media Inbox 次级动作行距续作

- `ce12c68da11069da21cf014a2f8a6f1840fa4603` 给 `MediaInboxSecondaryActions` 增加窄窗动态行距：Light/Dark 下 `520–576 DIP` 两行净距从 `4` 提到 `8 DIP`；`577 DIP` 起恢复单行和作者原 `4 DIP` margin。保留现有任务/媒体筛选行距行为。
- 当前提交 Release solution/XAML `0 warning / 0 error`、XAML `24/24`；相关 WPF 独立进程 `2/2 + 3/3 + 10/10 = 15/15`。行距扫描覆盖 `520–800 DIP` 断点和 `576→577→576` 往返；MediaInboxGeometry 覆盖 `520×600` footer 页级滚动可达及 `700×600` 四行下限/页级 fallback。完整证据和 TRX：[媒体次级动作行距](evidence/USER-REPORTED-MEDIA-SECONDARY-ROW-GAP-20260929/README.md)。
- freshness 更新至 `ce12c68d` 后仍为 `14 FRESH / 0 STALE`，自动门禁通过；package identity 未提供。真实 Playnite、物理 DPI、OS 输入和最终屏幕呈现未验；Media Inbox 滚动空白仍需安全宿主同进程诊断，不从本次布局测试推断修复。
- 下一项：继续实测 Save current-rule、Overview header 和 Dashboard game-header 的换行行距；随后按用户要求精简非必要静态说明、核对次级文字主题色与紫色主题一致性。

## 2026-09-29 用户报告：剩余可见操作组换行审计

- 在 `451164f2241f9a13a7d974c72410bcdd4528acb2` 上为实际生产 `SaveCenterView`“路径与校验”页的 `SaveCurrentRuleActions` 和 `OverviewView` 的 `OverviewHomeToolbarActions` 增加双主题 WPF 几何扫描。两组在全部采样尺寸均是单行，三个按钮均有非零可见尺寸；没有触发行距不足，因此不改生产布局。
- 存档操作组覆盖 520–900 DIP；工作台操作组覆盖 520–900 DIP，含 699/700/720/760 DIP 的响应式断点。隔离 Release/XAML `0/0`、`24/24`；精确隔离测试 DLL VSTest `2/2`，零失败/跳过。DLL SHA 与完整采样见 [剩余操作组行几何](evidence/USER-REPORTED-ACTION-WRAP-AUDIT-20260929/README.md)。
- DashboardView 的旧 `GameHeaderActions` 位于 `DashboardDemoShell` 的 `Visibility=Collapsed` 兼容树下，不是当前可见工作区，不按源码存在与否推导用户界面问题。未更改 192 项状态与计数。
- 首次默认 `bin` test invocation 因程序集身份旧于 checkout 被 `TestRepositoryContext` 拦截并停止，未计入；最终测试直接运行同一隔离 Release 输出 DLL。
- 下一项：精简 Overview 顶部与已加载数据重复的静态说明，再分类当前生产页 `GscInfoBrush` 用法，只调整非语义装饰/统计色，并检查浅深主题对比。真实 Playnite与物理 DPI仍待验。

## 2026-09-29 Q14-02 Task Center 筛选标签和紧凑换行

- 将任务筛选标签与控件配成完整组，统一 4 DIP 标签间距/全角标点；窄窗用 WrapPanel 整组换行。实测发现动态换行组原有行距为 0 DIP，现仅在紧凑区增加 8 DIP 底边距，并在返回主行时复位，避免改变 36 DIP 主工具栏高度。
- 同次 WPF 布局回归发现筛选控件被重排时可能写回首项；修复时保存/恢复现有选择，不改变业务命令或过滤语义。Release solution `0 warning / 0 error`、XAML `24/24`、source validation 通过；4 个隔离测试类 `13/13`，新几何 Fact 覆盖 Light/Dark × 5 个尺寸共 10 场景，窄窗换行净距 `≥7.25 DIP`。
- `render-qa` 完成且 `PROBLEM=0`；证据、四份 TRX 和完整报告见 [Q14-02 筛选标签](evidence/Q14-02-FILTER-LABELS-20260929/README.md)。测试 host 的 DPI 为 `1.5×1.5`，退出有 TextServices `InvalidComObjectException` 清理噪声、原因未知。离屏报告是逻辑 DPI `1.0`，没有真实 Playnite/物理 DPI/OS 输入验收，Q14-02 在 Round2 账本继续保持外部阻塞/未完成。
- 下一任务先处理用户补充的窗口紧凑布局：跨页面换行行距、存档中心顶栏、任务队列辅助说明与动作按钮高度、传输明细计数标记对齐；同时检查说明文字和状态色是否来自浅/深主题语义资源。Media Inbox 用户滚动问题继续等待安全宿主同进程诊断。

## 2026-09-29 Q14-01 Task Center 工具栏同高实测补证

- 当前代码身份 `591f07be` Release build `0 warning / 0 error`、XAML `24/24`，source validation 通过；新增的生产视图 WPF 几何 Fact `1/1`。Light/Dark × `980×640`、`1040×700`、`1280×720`、`1600×900`、`760×640 DIP` 共 10 组，参与主行/预设行控件高均为 `36 DIP`，主行框中心差 `≤0.75 DIP`，预设内容中心差 `0.67 DIP`。校验错误、键盘焦点、禁用刷新和忙碌状态均无超过 `0.75 DIP` 布局变化。没有生产 UI 改动。[几何、TRX、构建与限制](evidence/Q14-01-TOOLBAR-ALIGNMENT-20260929/README.md)
- WPF testhost 实测 `1.5×1.5`；xUnit/TRX 明确 `1/1` 且 VSTest exit `0`，关闭时出现 21 条 TextServicesHost `InvalidComObjectException` 清理噪声，根因未知。没有在真实 Playnite 或物理 125% DPI/OS 输入下验证。Round2 Q14-01 保持外部阻塞/未完成；后续 Q14-02 已补齐本地行为证据，见本文件上方。

## 2026-09-29 Q06 本机失败报告复核（先于 Q14-01 实测）

- 用户新提供的 one-click 日志构建与当前 main 身份一致（`dc712445`）；原日志未保留断言/堆栈。复用日志中的同一测试 DLL，三次独立 VSTest 和一轮项目级 `dotnet test --no-build` 均 `1/1` 通过；双主题五态探针 `10/10`，未复现、未改生产按钮，也不据此称故障已解决。[Q06 重检证据](evidence/Q06-BUTTON-STATE-RECHECK-DC712445-20260929/README.md)
- 隔离 runner 失败时现在会输出 normal verbosity 断言和堆栈；当时下一小批为 Q14-01，现已补入受控几何证据。Playnite 实机和物理输入/其他 DPI 仍不得由 STA 合成窗口代替。

## 2026-09-29 Q13-08 稳定分页锚点与总数证据复核

- 当前身份 `2333361e` 的 Release solution build 成功，0 warnings / 0 errors，XAML structural checks `24/24`；源码验证通过。Playnite 与 Worker 测试程序集 SHA、MVID、构建日志、5 份 TRX 和 console 均归档在 [Q13-08 证据](evidence/Q13-08-PAGING-ANCHOR-20260929/README.md)。
- 复跑 `MediaInboxScrollBehaviorTests 5/5`、`MediaWindowAnchorContractTests 10/10`、`R07SelectionAnchorBehaviorTests 4/4`、`MediaPageAccumulatorTests 6/6`、`MediaQueryPersistenceTests 4/4`，共 `29/29`。已有行为覆盖加载更多时稳定 ID/视口恢复、裁剪锚点时邻行回退、有界 2,000 项缓存去重与选中保留，以及隔离 fake 数据库中的稳定游标/实际总数。
- 没有生产实现或断言修改。“返回最新”按既有产品语义回到顶部；组件测试未覆盖真实 DashboardViewModel/fake service 刷新/删项后的绑定总数端到端，也没有 Playnite 宿主真实分页操作。Q13-08 继续保持外部阻塞/未完成；RenderTransform 缩放没有冒充物理 DPI。
- 下一项 Q14-01 工具栏同高，按既有共享样式和行为证据逐条对账。Media Inbox 用户滚动空白仍需真实 Playnite 的运行 DLL 身份、DPI/主题和同坐标系滚动前后诊断；WMI Access Denied 不绕过。R08 用户侧 `1/2` 失败仍缺方法、断言/堆栈与原始构建身份。

## 2026-09-29 Q13-05 共享 ScrollBar Thumb 行为复核

- 复用 `DesignTokens.xaml` 已有双向模板：纵/横轨道边距 4 DIP、有效 Thumb 下限 36 DIP、每个端帽由一个圆角形状绘制，Hover 刷从当前主题动态资源读取；没有改视觉资源、滚动单位或滚动系统。
- 提交 `ae35be70` 新增生产控件 STA WPF 双主题行为 `Q13ScrollBarThumbBehaviorTests 1/1`，覆盖纵/横向轨道两端、剩余边隙、最小长度、端帽边距/单形状和 Hover 进入/离开/恢复。精确身份 Release `0 warning / 0 error`、XAML `24/24`。测试与 DLL SHA：[Q13-05 证据](evidence/Q13-05-SCROLLBAR-THUMB-20260929/README.md)。
- 同身份 Media Inbox 合成滚动回归 `MediaInboxScrollBehaviorTests 3/3`：输出尺度 `1/1.25/1.5`，仍是 RenderTransform，不是物理 DPI；TextServicesHost COM 清理异常只作噪声记录，TRX 明确通过、exit `0`。现有虚拟化、Item ScrollUnit 和页尾断言未变。
- 当日真实宿主只读前置检查：冲突进程 `0`，`Assert-GscProcessCommandLineInspectionAvailable` 返回 Access Denied，runner 按既有安全协议拒绝启动。无实际 DLL/MVID、window DIP、DPI/主题或滚动日志；用户报告的表头后空白依然未复现、未修复。详情与诊断数据清单：[Media Inbox 宿主复核](evidence/MEDIA-INBOX-HOST-RECHECK-20260929/README.md)。
- 下一独立代码项 Q13-06 表格滚动条交角/末行末列避让；Media Inbox 保持高优先，隔离启动条件恢复后先复现并按 presenter/首行坐标差定位，不能据合成夹具猜根因。

## 2026-09-29 Q13-04 表格列宽调整与 Auto 恢复

- 复核生产共享 Header 模板和布局控制器后发现，双击原生 resize gripper 虽会将列宽改为 `Auto`，旧 Pixel 持久化键仍留在设置中，下一次布局会再次覆盖用户动作。提交 `c513be3f` 只移除当前视图/当前列的旧键并复用既有延迟保存；没有新增持久化模型或改共享视觉模板。
- 精确提交 Release solution/XAML `0 warning / 0 error`、XAML `24/24`。真实生产 WPF Header/Thumb 双主题 STA 回归 `Q13ColumnResizeBehaviorTests 1/1`；既有列宽持久化 `6/6` 与排序 `7/7` 均通过。修复前红测与修复后 TRX、程序集 SHA 和用例范围见[Q13-04 证据](evidence/Q13-04-COLUMN-RESIZE-20260929/README.md)。
- 测试通过真实 WPF Thumb 的路由拖动事件，不是 OS 物理鼠标；本阶段没有启动真实 Playnite，也没有核验有效 DPI 或最终屏幕呈现。Q13-04 宿主栏及最终状态保持未完成。R08 用户侧 `1/2` 仍缺失败详情，当前隔离 `2/2` 不能定位该故障。
- Q13-05 Thumb 端点/Hover 自动行为后来已由 `ae35be70` 补证；物理输入与宿主限制见当前阶段记录。

## 2026-09-29 Q13-03 进程映射行内删除目标

- 盘点生产 `MaintenanceView` 后发现行内删除按钮漏传行 DTO，而 DashboardViewModel 读取独立的 `SelectedProcessMapping`；现已由提交 `256a40f2` 传入当前行 DTO，并让命令拒绝缺少目标的调用，实际 Worker 请求仍复用现有 `DeleteProcessMapping` 与 DTO。
- 生产 WPF 行为测试在 Light/Dark 下保留 A 为选中详情、对 B 行按钮做实际 WPF HitTest 并由 AutomationPeer 执行，捕获目标为 B；Inspector 动作仍指向 A。`ProcessMappingInlineActionBehaviorTests 1/1`，源绑定/既有 Inspector 目标契约 `1/1`。Release solution/XAML `0 warning / 0 error`、XAML `24/24`，source validation/diff check 通过。
- 未启动真实 Playnite、未发送 OS 鼠标输入或调用 Worker 删除；真实桌面行命中仍属宿主边界，Q13-03 最终状态保持未完成。完整证据：[Q13-03 行内删除目标](evidence/Q13-03-PROCESS-MAPPING-ROW-ACTION-20260929/README.md)。同一提交身份 R08 动效 `2/2`，但用户报告的失败方法/堆栈仍缺失，COM 清理噪声另记于[R08 复核](evidence/R08-CURRENT-RECHECK-20260929/README.md)。
- Q13-04 列宽调整热区随后以 `c513be3f` 修正 Auto 恢复持久化并补足 STA WPF 行为；物理鼠标/真实 Playnite 边界详见上方当前阶段。

## 2026-09-29 Q13-02 多选反馈：已有计数行为与宿主手势边界

- 盘点确认 Media/Task/Save/Maintenance 主表已使用 `SelectionMode=Extended`；Media 批量动作复用 `SelectedItems`，稳定 ID/模式范围与隐藏选择摘要已存在。复用 `R05MultiSelectionSummaryBehaviorTests 3/3` 的 0/1/2 计数、隐藏项、清空且保留模式，以及 `R22BatchCountBehaviorTests 3/3` 的隐藏计数/空选择负例；没有重建功能。`GscCheckBox` 的 tri-state 只是视觉状态，当前批量 DataGrid 无全选/半选消费者，R05-05 已判不适用。
- 现有测试没有实际 Ctrl/Shift 鼠标修饰键序列；源码 `SelectionMode=Extended` 声明不作为手势通过证据。隔离宿主仍受 WMI Access Denied/CEF `platform_channel 0x5` 限制，本轮不使用 SendInput 抢占桌面，也不绕过门禁；Q13-02 宿主及最终状态保持外部阻塞/未完成。具体真实 host 复核和负例步骤：[Q13-02](evidence/Q13-02-MULTISELECT-GESTURE-BOUNDARY-20260929.md)。
- Q13-03 行内删除目标已完成可控修复与行为验证，结论及宿主边界见上文。

## 2026-09-29 Q13-01 失焦选中：已有共享实现当前 main 复核

- 没有重做样式或增加产品代码。当前身份 `d46d6be5` Release solution/XAML `0 warning / 0 error`、XAML `24/24`；`R06SelectionStateBehaviorTests 2/2` 实际 WPF 测量活动选中→外部失焦后的 inactive brush/muted outline，`R23ProductionResourceStateBehaviorTests.EachProductionPageGridKeepsSelectedFocusAndDisabledRowStatesAcrossThemes 1/1` 在 Task/Media/Save/Maintenance 四表及 Light/Dark 比较状态下 cell/TextBlock 几何，门限 `<=0.25 DIP`。
- 复用 `R06-03` Light/Dark 357 PNG `render-qa OK` 与 9/24 几何复核；Media Inbox 212 DIP 门禁已有证据。当前自动/视觉条件已有支持，但真实 Playnite OS 失焦输入、UIA、物理 DPI 和 presented frame 未验，Q13-01 最终状态仍为“未完成”。详细复核与 TRX：[Q13-01](evidence/Q13-01-RECHECK-20260929/README.md)。
- 下一项 Q13-02 多选反馈：盘点 Extended DataGrid、计数与半选绑定，补实际 Ctrl/Shift WPF 手势及负例。

## 2026-09-29 Q11-08 快速导航过渡

- 当前生产导航本来就同步替换缓存 PageHost 页面、标题和 RadioButton 选中状态，不使用全页入场动画。本阶段只补行为验证，无生产代码修改。提交 `8e3cc914` 的 `R08PageSwitchBehaviorTests 4/4` 中新增七次 Overview/Tasks 快速 Checked 路由；每次实际 WPF 状态核对 workspace、正确页引用、标题、唯一选中项和仍聚焦的导航项，布局刷新后终点一致。观测 `routes=7; final=Tasks; title=任务中心; focus=NavTasks; pageEffects=none`。
- 同身份导航来源/历史 `PurposeNavigationSourceTests + R10NavigationBehaviorTests 6/6`；用户前条报告的 `R08MotionReverseBehaviorTests 2/2`。三组共 `12/12`，0 failed/skipped。Release solution `0 warning / 0 error`、XAML `24/24`。R08 testhost 有一条 TextServicesHost `InvalidComObjectException` 退出清理噪声，根因未知，测试和进程均成功。证据：[Q11-08](evidence/Q11-08-NAV-TRANSITION-20260929/README.md)。
- 受控 `1366×900 DIP`、透明 STA Window 和 WPF focus API 结果不等于 OS 输入、无旧帧的屏幕级验证或 Playnite host；主题/物理 DPI/真实呈现仍待验，Round2 Q11-08 最终未完成。下一项检查 Q13 已登记的交互任务，先确认现有测试覆盖和缺失负例。

## 2026-09-29 R08 用户失败报告：本机当前身份 8 轮复核

- 用户提供 one-click 摘要 `R08MotionReverseBehaviorTests 1/2`，无方法/断言/TRX；checkout 内同名日志最后更新于 9 月 24 日，是宿主审计记录，不含该失败。
- `c50de56ae9d54995e5bc4834c6bf6d664097bb07` 的独立 Release solution build 成功：XAML `24/24`，0 warning/error。R08 两个真实 WPF 动效行为方法在 8 个分开的 VSTest 进程逐轮 `2/2`，总 `16/16`、0 failed/skipped、exit `0`。每轮有 TextServicesHost `InvalidComObjectException` shutdown 噪声，根因未知；TRX 与程序集 SHA：[R08 复核](evidence/R08-CURRENT-RECHECK-20260929/README.md)。
- 当前未重现用户机器失败；保留其问题打开，等待完整失败日志/TRX 以识别断言。未改生产动画。下一可执行项 Q11-08 导航过渡。

## 2026-09-29 Q11-07 工作区导航状态往返

- 生产页面缓存与双向筛选/页签绑定已存在；`f9fa47f6` 仅补 STA WPF 回归。TaskCenter 搜索/四筛选改成非默认值、Media/Save/Maintenance 索引设为 `2/3/5`，切过其它页面再回时保留；96 个合成 Task 的第 55 项选择与滚动 `18→18 DIP` 保留；Shell DashboardViewModel 引用及 page/DataContext fixture 引用不变，合成刷新命令 `0` 次。`R08PageSwitchBehaviorTests 3/3`，源码映射及路由栈 `6/6`。详情及 no-service 边界：[Q11-07 evidence](evidence/Q11-07-WORKSPACE-STATE-RETURN-20260929.md)。
- Release Playnite.Tests `0 warning / 0 error`。WPF testhost `1366×900 DIP`、opacity 0.01；没有连接业务查询服务，故只能断言合成刷新命令/集合稳定；没有屏幕截图、真实 Playnite、OS 输入、主题切换或 effective DPI。Q11-07 自动行为通过，最终视觉/宿主待验；下一项 Q11-08。

## 2026-09-29 Q11-06 页签溢出行为 / R08 完整 Release 复核

- `1c4c21d537e78ffb9595f5d278410a7d97ca504f` 新增窄窗工作区页签 STA WPF 回归，不改生产 XAML。四个生产页面实际页签数 `4/4/6/4`；Light/Dark 下检查滚动条可见、Tab 键进入、Right/Left 每项可聚焦且滚入视口、PageRight/PageLeft 鼠标滚动路径和页签选择；宽窗六项 Maintenance 的横条应折叠。R23 类 `7/7`，TRX 与夹具限制见 [Q11-06 evidence](evidence/Q11-06-WORKSPACE-TAB-OVERFLOW-20260929.md)。
- 同提交完整 Release `0 exit`：XAML `24/24`，solution `0 warning / 0 error`，Core `125/125`，Worker `356/1 existing skip`，Playnite source `111` classes、WPF isolated `108` classes 全过；此前用户报告失败的 R08 类在 `[54/108]` 通过。日志 `artifacts/q11-06-release-20260929/full-release-build-1c4c21d5.log`。
- Q11-06 只获受控 WPF 行为证据，透明 STA 夹具不是 Playnite 呈现；FollowPlaynite、OS 物理输入、有效 per-monitor DPI 和截图未验，最终视觉/宿主栏仍待验。下一项 Q11-07；Media Inbox 等用户报告问题继续等待同进程 `[GSC-GRID-DIAGNOSTIC]`。

## 2026-09-28 Q11-05 页签内容 Stretch / R08-01 采样稳定

- 当前代码身份 `e467c3b2fe080965c3946f1f3101f707c731472b` 的 Q11-05 受控 STA WPF `380×240 DIP` 三类 Header、浅/深主题均验证内容根元素与 `PART_SelectedContentHost` x/y 对齐、宽高差 `<=0.25 DIP`；R23 类 `6/6`。现有共享 Stretch 实现满足窄页面行为，没有生产 XAML 更改。证据：[Q11-05](evidence/Q11-05-TAB-CONTENT-STRETCH-20260928.md)。
- 用户提供 R08 一键构建失败摘要 `1/2`；本地隔离曾复现一次 `SidebarRapidReversalUsesLatestTargetAndReleasesOldClock` 的间歇失败，但用户机器完整堆栈不可见。将固定 100ms 读值修为有界等待真实动画时钟/中段几何，保留反向和终态断言；修复后 8 个独立 testhost 全通过，后续隔离构建中的 R08 类 `[54/108]` 也通过。证据：[R08-01 采样稳定性](evidence/R08-01-REVERSAL-TEST-STABILITY-20260928.md)。
- 修正提交 `4d0ff11c` 的 clean identity 全套 Release 最终通过：XAML `24/24`，solution `0 warning/0 error`，Core `125/125`，Worker `356 passed / 1 existing skip / 0 failed`；Playnite source `111` 类组与 WPF isolated `108` 类组全部通过。构建脚本最终报告全部成功，日志见 `artifacts/r08-motion-flake-20260928/full-release-build-4d0ff11c.log`。滑杆旧 22-DIP 结构断言已按 32-DIP 实测命中面更新，定向资源 `1/1`、三主题命中/键盘行为 `3/3`；完整运行中的 R08 类也通过。R08 单类详细输出中的 TextServicesHost `InvalidComObjectException` 是测试退出清理噪声，xUnit/VSTest 结果为成功。
- Q11-05 自动 Stretch 行为仍未验真实 Playnite/物理 DPI；R08 用户故障机的实际 assertion 仍待其完整 log。下一独立 Q 项为 `Q11-06` 页签溢出/窄窗键盘鼠标可达；Media Inbox host geometry 继续按高优先级等待同进程证据。

## 2026-09-28 当前续作：Q11-04 页签内边距和标签切换稳定性

- 最终代码/测试身份 `9804f494c77f97c6a38a81e56c861f9918a4b9ea` 的 `R23ProductionResourceStateBehaviorTests 5/5` 通过。短中文、长英文及计数 Header 在共享工作区 TabItem 真实 WPF 模板中不裁切/无重叠，均落在圆角安全边界内；切换 Tab 后 selected content width 变化 `<=0.25 DIP`，跨浅深主题 slot 差同样受限。仅测试变化，无生产样式调整；证据：[Q11-04](evidence/Q11-04-TAB-HEADER-PADDING-20260928.md)。
- Q11-04 仍缺 Playnite 真实页面/物理 DPI/OS 输入，不标最终完成。下一独立任务 `Q11-06` TabControl 窄窗溢出与可达性；Media Inbox 主问题待同进程宿主诊断或安全隔离环境恢复。

## 2026-09-28 当前续作：Q11-03 收起导航入口状态

- `50865efcd67e3e17bfe32445ef7eab85b298d871` 只补实际 WPF collapse 后的 label/icon/Tooltip/UIA 名称行为，没有生产样式变化。`ProductionShellChromeSourceTests 13/13` 通过；TRX 记录 1 条 WPF TextServicesHost COM 清理噪声，根因未知。
- 七项收起布局、Tooltip 完整文本与 Automation Name 受控通过；未弹 Popup/驱动 OS 鼠标，因此延迟/定位/边缘翻转/Playnite host 仍未验证，Q11-03 保持未完成。证据见 [Q11-03](evidence/Q11-03-COLLAPSED-NAV-TOOLTIPS-20260928.md)，Round2 账本随本阶段同步。
- 当前下一独立项 `Q11-04` 页签内边距；Media Inbox 真宿主滚动问题优先等待用户同进程日志或安全隔离前置条件恢复。第三轮 R ledger 192 项统计不变。

## 2026-09-28 当前续作：Q11-02 状态组合与 Media Inbox 宿主门禁

- Q11-02 新增 `AcrylicNavItem` 真实 WPF selected+keyboard focus 重叠行为，提交 `9987dbb10f14519f3da2614215ab298410320149`，`R23ProductionResourceStateBehaviorTests 4/4` 通过；不改生产样式。Q11-02 保持未完成，OS mouse hover 和 Playnite host 未验；证据见 [Q11-02](evidence/Q11-02-SELECTED-FOCUS-20260928.md)。Round2 账本只更新该项，不改变第三轮 192 行 R 统计。
- Media Inbox 用户空白问题按真实宿主流程再次检查：Playnite 进程为 0，找到的 Desktop EXE 文件资源版本为 `1.0.0.0`，无法充当运行身份；隔离 runner 的命令行可用性门返回 Access Denied。本轮没有启动/改真实 profile，旧失败隔离记录发生在 GSC 安装前。用户缺陷未复现、未修，数据模板/虚拟化保持不动；详见 [Media Inbox 宿主复核](evidence/MEDIA-INBOX-HOST-RECHECK-20260928.md)。
- 当前独立下一项 Round2 `Q11-03` 收起导航 Tooltip 行为；取得同次真实宿主诊断或隔离前置恢复后，优先返回 Media Inbox 根因分流。

## 2026-09-28 Q10-07 设置滑杆实际可操作性

- 盘点生产设置 `GlassStrengthSlider` 和共享 `GscSlider`：旧证据只量得开发夹具 150 DIP 可见，新增生产 WPF 用例在旧模板 Light/Dark `0/2` 复现轨道上下命中为 `none`。`e007a339` 用透明命中面承载原 4 DIP 可见轨道，18 DIP 可见圆形 Thumb 保持，命中扩大到 32 DIP；键盘小/大步长固定为 1/10。
- 精确提交 Release solution `0 warning/0 error`、XAML `24/24`、source validation 通过。Light/Dark/FollowPlaynite 实际设置视图 `3/3`，受控点击、键盘、百分比绑定、禁用/UIA 拒绝和窄短窗口均有行为断言；关联类按 WPF 类隔离 `13/13 + 3/3 + 24/24`。合并类 testhost 的 R09 全局资源污染曾导致 `1` 项失败，独立 R09 `3/3`，没有删去该失败事实。
- 同身份生产设置页 `1040×700 DIP` 浅/深色 RenderHarness 图与报告可见滑杆/值标签，属于离屏组件视觉。Playnite/package-host、物理鼠标/键盘、125%/200% DPI、屏幕呈现仍未验；Round2 Q10-07 最终保持未完成，192 项 R 台账不变。细节见 [Q10-07 evidence](evidence/Q10-07-SETTINGS-SLIDER-BEHAVIOR-20260928.md)。下一独立任务 Q10-08；Media Inbox 真实宿主滚动空白仍优先等待安全隔离日志。

## 2026-09-28 Q10-06 Toggle 过程状态行为补证

- `9ec0ab83` 在共享 Toggle 上复用现有 Binding/设置模型，增加按元素拥有的 Thumb 位移动效与关闭动效/高对比度回退；不新增服务、DTO、命令或存档字段。提交后 Release solution `0 warning / 0 error`、XAML `24/24`，source validation 通过；WPF 静态审查 `0 errors / 30 warnings / 177 info`。
- 精确构建身份 `0.6.73+9ec0ab8300e3c2ca90d7aaaaf2e3b6eadaa7dd5a`，Playnite DLL SHA-256 `F0C628AEF9292AC2FE81BDD1EBFC18DB604C81DD69948E41F5185AF09C91A6BA`。定向隔离 WPF 回归 `5/5`：Q10 行为 `1/1`、R05 持久化 `1/1`、R08 动效反转 `2/2`、共享 Toggle 资源契约 `1/1`。Q10 实测两主题下源更新拒绝、禁用焦点/UIA 负例、连续反转中间值与最新目标、关闭动效即时收敛并释放时钟；TRX 和边界见 [Q10-06 evidence](evidence/Q10-06-TOGGLE-STATE-20260928.md)。
- 没有把本次程序集安装到用户 Playnite 目录或启动真实宿主。STA WPF 动画及进程内 UIA provider 不代表 package-host、物理 DPI、OS 输入或最终呈现；因此 Q10-06 自动行为已补证，真实宿主视觉项仍待验，Round2 最终状态保持未完成。R ledger 仍 192 项、`106/83/1/1/1`。
- 当前下一项是用户新报告的 Media Inbox 滚动后表头与数据行错位/空白：先核验隔离宿主构建身份和尺寸/DPI/主题，再补同坐标头部/视口/首行与外层滚动偏移日志；在真实样本区分共享 DataGrid/Page 测量与行锚点原因前不改生产布局。

## 2026-09-28 R06-02 用户排序崩溃修复的 clean-main 复核

- `f8a82469` 修复 detached `ListCollectionView` 在结束 `DeferRefresh` 时触发 `NullReferenceException`；原 TRX 在提交前运行且身份为父提交 `ade4b937`，现用干净当前 main `3d83d38f9592c01522240fe7f462bf2e52ff0cb3` 重建 Release 并复跑排序类，`7/7`、0 fail/skip、exit 0。
- 行为覆盖真实 WPF `DataGridColumnHeader` 受控 Click 升序/降序/箭头，以及 detach 后点击不抛错、活动排序状态不变。solution `0 warning/0 error`、XAML `24/24`。真实 OS 鼠标、Playnite/package-host、用户包和最终呈现仍待验。
- 详见[排序 clean-main 复核](evidence/R06-SORTING-CLEAN-MAIN-RECHECK-20260928.md)和[脱敏 TRX](evidence/R06-SORTING-CLEAN-MAIN-20260928.trx)。

## 2026-09-28 R00/R01 freshness、SettingsState 夹具根路径修正

- 源码身份 `59afd8f4bd81039e1e8150d0ccc35e608b357113` 修正 RenderHarness SettingsState 探针：隔离输出路径下固定回退五层会落到 `.tmp`，正常/dirty 夹具因此得到空 Worker 路径并误报；现复用已有 `RepositoryRoot`。只改 RenderHarness，不改生产设置校验或 XAML。
- Release solution `0 warning/0 error`、XAML `24/24`；完整 clean-tree 双主题 RenderHarness `373` 个输出（`312` 张 PNG）、normal/dirty/invalid 三态正确、`render-qa OK`。`UiAuditSourceTests 6/6`、RepositoryIdentity `2/2`、BuildIdentity `3/3`。
- 当前 UI Audit `168` 个运行时快照、`103` 条警告、`0 HIGH / 0 MEDIUM / 0 Fidelity / 0 失败路由`；索引 `20/20` 引用、完整代码身份、样本和边界通过。旧审计 `7 HIGH/4 MEDIUM` 为前一身份历史，不再描述当前身份。
- freshness 复核刷新 7 条 R00/R01 源码基线；`0` 条需重跑、`0` 条匹配失效源码路径。R00-01/02、R00-05、R00-06 的测试身份为 `53e61175`，其后唯一代码差异是未命中这些任务 sourcePaths 的 RenderHarness 根路径修正；R00-07 与 R01-01/03/06 使用当前 `59afd8f4` 证据。Package identity `not-provided`。
- 详细根因、记录身份、TRX、完整 render-qa 文本及 5 张合成页面图见 [R00/R01 当前复核](evidence/R00-R01-CURRENT-RECHECK-20260928.md) 和 [R01-06 当前归档](evidence/R01-06-controlled-audit-20260928/README.md)。离屏 logical DIP 不代表真实 Playnite、物理 DPI、跨屏、presented frame、ETW 或宿主性能。R ledger 仍 192 项、`106/83/1/1/1`；R23-08 无 READY/IN_PROGRESS 产品代码项。

## 2026-09-28 main：R18-04 精确组成、复采与全局媒体目标行为补证

- 按跨设备续接请求将本地 `main` fast-forward 至 `331a57137f755897f309664064a8f8e1adbbd58f`。远端已含媒体 Inbox 全局游戏目标的生产实现；本批只在 `ReportedWorkspaceLayoutBehaviorTests` 增加实际 WPF Binding 状态变化断言，没有改 XAML/ViewModel/业务代码。
- Release solution `0 warnings / 0 errors`、XAML `24/24`、source validation 与 diff check 通过。Reported workspace `10/10` clean exit；R14 分类选择 `4/4`；R18 专测 `1/1`。关联测试精确为 MediaPageAccumulator `6`、MediaWindowAnchorContract `10`、MediaInboxGeometry `3`、R07SelectionAnchor `4`，独立 testhost 合计 `23/23`，无失败/skip；纯数据/几何/选择 `10`、源码契约 `7`、STA WPF `6`。
- 本次 R18 STA 采样 Task 三档均 `9` 个实现容器/最多 `7` 行可见；Media Inbox 此专测视窗 `7/7` 且后端增至 20k 仍只缓存 2k。最大 ScrollTo+布局同步时长 Task `63.899/32.968/27.796 ms`、Media `0.027/0.217/0.463 ms`。用户先前给出的共享模板/窗口读数 Media `14` 和约 `0.03 ms` 属于不同样本上下文，分开保存，不冒充真实宿主帧性能。
- TRX 保留 R18 `TextServicesHost.OnUnregisterTextStore InvalidComObjectException` 清理噪声 6 次、MediaWindowAnchor 2 次；各 testhost 结果明确全通过且退出 `0`，根因未知。双主题切换/清空/恢复的全局游戏摘要验证、23 项逐项组成、采样和原始 TRX 见[完整证据](evidence/USER-REPORTED-MEDIA-TARGET-AND-R18-RECHECK-20260928.md)。未安装或启动 Playnite，无真实宿主/物理 DPI/presented frame/ETW 结论。
- R ledger 仍 192 项、`106/83/1/1/1`；R23-08 当前准入仍无 READY/IN_PROGRESS 产品代码项。下一步等待新的可复现产品问题或明确环境门槛变化；不从这组离屏采样自行创建性能缺陷。

## 2026-09-28 Q10-03 复选标签受控行为补证与远程身份错配

- 共享生产 `GscCheckBox` 模板新增字符串 `AccessText` 换行呈现和透明根 hit-test 面；没有变更业务绑定/命令。隔离 Release 双主题行为 `4/4`，包括长标签换行、方框/标签/间隔几何命中、每次 Click/Space 只切换一次，以及禁用后的焦点/UIA Toggle 负例。完整范围和 TRX 见 [Q10-03 证据](evidence/Q10-03-CHECKBOX-LABEL-HIT-20260928.md)。Q10-03 仍缺 Playnite 宿主中的实际鼠标与视觉验收。
- 用户提供的 one-click 日志中 Core `125/125`、Worker `357/357`、编译成功；`KeyboardFocusSourceTests` 的 3 条失败在 `TestRepositoryContext` 源身份门，程序集 commit `6618de22` 对应源码根 HEAD `8a7a56b5`，另两条 WPF 行为通过。当前 checkout 重建后同类 `5/5`（[复核 TRX](evidence/REMOTE-IDENTITY-MISMATCH-KEYBOARD-FOCUS-CLEAN-RECHECK-20260928.trx)）；原因确认是程序集/源码身份不一致，旧身份程序集如何进入该输出路径仍待定位，不放宽 fail-closed 身份检查。
- 本次没有启动 Playnite，也没有写真实用户数据。离屏逻辑 DIP/STA 和进程内 UIA provider 不代表物理鼠标、Narrator/Inspect、宿主呈现或 DPI。R ledger 仍 192 项、`106/83/1/1/1`；R23-08 的 R 类 READY/IN_PROGRESS 结论不变。后续按 Q10-06 的具体条件先盘点现有开关绑定及拒绝/禁用/连续切换路径，再选择可受控验证的小批量。

# 2026-09-26 SaveHistory summary/action separation

- `ReportedWorkspaceLayoutBehaviorTests` 在 Light/Dark 独立复现摘要正文与动作区 gap `1.6 DIP`（既有目标 `8–14 DIP`）。将紧凑动作区 top margin 从 2 改为 10 DIP 后，两主题实测均为 `9.6 DIP`，断言与行为未放宽/替换。
- Release Playnite.Tests/RenderHarness 隔离构建 `0/0`，布局回归 `2/2`；完整 RenderHarness `render-qa OK`、0 PROBLEM；1040×700 Save 表格 `4/4` 行。Source validation、XAML `24/24`、diff check 通过。R ledger 192 项、`106/83/1/1/1` 不变。
- 证据：[SaveHistory action separation](evidence/SAVE-HISTORY-ACTION-SEPARATION-20260926.md)。复用既有 restore assets、未启动 Playnite；离屏 DIP 不代替真实 host/物理 DPI。Settings 与本阶段分别提交，用户批准后已普通快进推送至 origin/main（功能提交范围 `b85e53ed..15a5fe22`）；推送状态的文档同步也已提交并推送。
- `DEVELOPMENT_HANDOFF.md` 中更早的阶段问题计数是历史快照，最新全量 Render QA 已无 PROBLEM；不从历史快照重新打开已完成页面。

# 2026-09-26 Settings short-window viewport repair

- Settings 阶段收口 Task 后的 8 个 RenderHarness findings：5 种宽度的 560-DIP 样本中 category rail 可视区 `174.4 DIP`、滚到末项后边界 `165.6 DIP`；正文从 128/144 提高为 `185.6 DIP`。恢复默认说明与命令没有移除，短高自动折叠、正常高度恢复；同一断点内保留用户切换。
- 修复类别门禁单位：ListBox `CanContentScroll=True` 时 `ScrollViewer.ViewportHeight` 是 logical item count，不是 DIP。现在使用 `ScrollContentPresenter.ActualHeight` 和同坐标系末项边界。完整双主题/多尺寸 RenderHarness `render-qa OK`、0 PROBLEM、退出码 0；Release Harness 和 Playnite.Tests/依赖 `0 warnings/0 errors`；Source validation、XAML `24/24`、diff check 通过。R 账本仍 192 项、`106/83/1/1/1`。
- 证据：[Settings 矮窗视口修复](evidence/SETTINGS-SHORT-WINDOW-VIEWPORT-20260926.md)。这是离屏 WPF/DIP 结论，不代替 Playnite host、用户包 identity、物理 DPI/呈现。普通 restore 被 NuGet.Config ACL 阻止，采用现有资产/no-restore 和 `.tmp` 输出隔离。
- 后续扩展测试发现的 SaveHistory `1.6 DIP` 间距已在下一独立阶段修复为 `9.6 DIP`，断言范围保留；见顶部独立证据。

# 2026-09-26 Overview empty recent-access viewport

- 当前 main 起点 `daa6ecb2` 上的空集合 fixture 将 `OverviewRecentAccessList` 实测为 `2 DIP`；Auto 行测量没有从 `MaxHeight` 获得任何最小高度。将本列表最小高度设为 `236 DIP`，保留空状态内容、最大高度、Recycling virtualization 和内部滚动。
- 修复后 RenderHarness 11 个常用/大窗口样本全部 `236 DIP`、`items=0`；Light/Dark 页面均无 Overview PROBLEM。总 gate 从 33 项降至 13 项，Task 和 Settings 问题仍开放。
- Release build `0/0`，Overview baseline source test class `16/16`。`render-qa.ps1` 常规 restore/build 受 NuGet.Config ACL 限制；用现有 assets `--no-restore` 构建后直接运行同一 harness。未启动 Playnite，离屏验证不作为实际宿主/DPI验收。证据：[Overview empty viewport](evidence/OVERVIEW-EMPTY-RECENT-ACCESS-VIEWPORT-20260926.md)。

# 2026-09-26 Save history compact viewport recheck

- 在当前 main 起始身份 `0e340c08` 上复核旧 UI gate，发现 Save history 在窄窗中垂直空间不足；修正后 1040×700 DIP Render QA 样本表视口 `260 DIP`、完整可读 `4/4` 行，三个操作按钮各 `36 DIP`。窄布局保留可发现的恢复可用性说明，不占底部数据行空间。
- 离屏 RenderHarness 168 个快照：HIGH/MEDIUM/Fidelity/失败路由均为 0；完整 `render-qa` 仍有 33 个 PROBLEM，限于 Overview 最近访问列表 2 DIP、Task 行数/compact Inspector 越界、Settings 矮窗类别/主体视口不足。项目级 gate 未通过；后续应按这三组继续，而不是依据旧 R23-08 快照称无代码工作。
- 目标回归 `3/3`、RenderHarness build `0/0`、XAML `24/24`、source validator 与 WPF 静态检查通过。本批不改 R 账本（192 项，`106/83/1/1/1`），未启动 Playnite/真实宿主。完整细节：[Save history compact viewport recheck](evidence/SAVE-HISTORY-COMPACT-VIEWPORT-RECHECK-20260926.md)。

## 2026-09-24 R23-04 隔离宿主启动边界更新

- `42884321` 首次 host run 只观察到通用 Startup Error/safe-mode prompt，manifest、`summary.json` 与 GSC 侧栏均未出现，`cef.log` 是空文件。runner 强杀缺陷与旧 profile 的 `safestart.flag` 已发现，但不能唯一归因该次报错。
- `08a10da9` 全新 profile bootstrap 在扩展/seeder 安装前直接记录 `cef.log` 的 CEF `platform_channel.cc:108` FATAL “拒绝访问 (0x5)”，Playnite 未正常退出并留下 `safestart.flag`；没有 GSC 加载、manifest、Dashboard、UIA 页面或第二次启动。相同系统权限状态不重试，不删除标记，不绕过限制。失败 catch 在 `910480c7` 提交，先写 `runner-metadata.json` 和 `host-startup-blocker.json` 后停止；离线正/负日志 fixture 分类通过。该提交 clean Release build `0 errors/2` 既有 warning，审计源用例 `7/7`，用户四页行为 `8/8`。
- 当前 Settings 最新截图与 source 的受控几何仍不匹配；当前源设置页 Light/Dark `1254×800 DIP` 行为、搜索左锚点、标题图标行和 reset/path 控件已测，但截图加载 identity 与 Playnite 父容器未知。不得写成用户窗口已修复。详细日志/边界见 [R23-04 bootstrap 证据](evidence/R23-04-BOOTSTRAP-SAFE-START-ROOT-CAUSE-20260924-42884321.md) 与 [用户布局复核](evidence/USER-REPORTED-LAYOUT-CURRENT-MAIN-RECHECK-20260924.md)。
- `910480c7` clean rebuild 后 source validator、PowerShell AST、`git diff --check` 通过；WPF 静态审查 `0 errors/30 warnings/177 info`。用户四页 Light/Dark 行为 `8/8`，最终 identity TRX 已留档、无 COM 清理噪声；R23-04 仍未满足。

## 2026-09-24 R23-04 合成非空库夹具准备

- 按 R23-04 当前缺口补充测试专用 Playnite SDK seeder：显式开关才安装至隔离 Extensions；数据路径强制在仓库 `.tmp/`，拒绝 reparse point；默认 64 条有固定合成前缀的未安装游戏，无安装目录，不改正式插件。
- Release solution `0 errors`，两条既有 `MediaCenterView.xaml.cs:703 CS8602` warnings；source validator、XAML `24/24`、PowerShell parse 通过。目录边界/稳定测试 `4/4`，宿主证据契约 `6/6`，用户四页几何 `8/8`。包脚本支持隔离审计跳过版本化 `.pext/.zip` 覆盖。
- 此阶段未运行 Playnite；R23-04 状态不变，真实数据库 manifest 与 Embedded/UIA 仍待验。下一步提交后，在 `.tmp` 新 profile 按证据页命令仅做一次种子启动。CEF 若仍 `platform_channel 0x5`，保留真实阻塞与 seed 是否执行事实，不重试同状态。
- 证据：[R23-04 合成库夹具准备](evidence/R23-04-SYNTHETIC-LIBRARY-SEEDER-PREP-20260924.md)。

## 2026-09-24 ROUND3 状态与临时目录审计

- 核对规范任务表：正文表格 192 项、ID 唯一；表前 R13/R14 的 10 行是阶段摘要，不计入 192 项。逐项状态以任务表为准，R02-06 仍是 Playnite 原生菜单外部阻塞，R23-04 仍待正常宿主环境，R23-05 仍待真实呈现帧/宿主性能验收；本轮未启动 Playnite，也未重试此前受 CEF `platform_channel 0x5` 阻挡的宿主流程。
- 清理仓库 `.tmp/` 内已完成且无文档/脚本引用的旧构建与布局 QA 目录共 26 个，释放约 1.81 GiB。保留被 R00 当前证据引用的 `.tmp/r00-current-refresh`，也未清理归属不确定的旧目录；未改动 `artifacts/`、跟踪文件或其他任务文件。
- 本轮仅作账本与工作区审计，无源代码、测试或验收结果变化。下一步仍从任务表中选择依赖已满足的小批量推进；宿主相关项等待正常隔离宿主可运行，R23-05 等待合规跟踪权限。真实 Playnite 渲染、用户窗口截图及物理 DPI 仍未验。

## 2026-09-24 Settings 窗口化截图追加复核

- 用户再次报告 Settings 顶部图标、搜索框和操作控件错位。`3a1dadd8` 已将搜索框锚定标题列并约束宽度；当前 `c866c027` Release 构建的 `ReportedWorkspaceLayoutBehaviorTests` 四页 Light/Dark 共 `8/8`，Settings 两主题各 `1/1`，覆盖近似 `1254×800 DIP` 窗口、Loaded/SizeChanged 及搜索居中错位负例。当前源无复现，未改生产代码。
- 当前候选包为 `0.6.73+c866c027a2c7a2232028e9a20f1e2060bcf027cd`，SHA-256 `17B5C51CA502C0C2F119DFCBF98BF720AC43C56F899CB6BC3A6C909923498CAA`，未安装。原始 TRX 为 `evidence/USER-REPORTED-LAYOUT-CURRENT-MAIN-RECHECK-20260924-C866C027.trx`。本机旧 DLL `7a4ba2a9` 可能解释截图差异但未关联；当前身份隔离宿主仍因 CEF `platform_channel 0x5` 仅打开 Startup Error，不能记为 Playnite 视觉通过。
- 192 项账本状态不变。具体尺寸、包与宿主边界见[设置页窗口化复核](evidence/SETTINGS-HEADER-WINDOWED-1254x800-CURRENT-MAIN-20260924.md)和[用户四页布局复核](evidence/USER-REPORTED-LAYOUT-CURRENT-MAIN-RECHECK-20260924.md)。下一项继续做不依赖 CEF/ETW 的 Q/R 小批量；R23-04 仅在正常隔离宿主可用时复验。

## 2026-09-24 R23-07 当前 main 状态复算

- 正式 8 列表头之后的任务表为 `192/192` 个唯一 ID；表前 10 行 R13/R14 摘要排除。当前分组：已满足/当前 main 受控复核 `106`、已满足但待明确环境门禁 `83`、外部阻塞 `1`（R02-06）、部分满足 `1`（R23-05）、不适用 `1`（R05-05）。没有待开始、实施中或代码完成待验收行。9 月 22 日的 `112/29/45/1/3/1/1` 保留作历史快照，不作为当前数字。
- 第二轮 Q 正式表仍为 `208/208` 唯一 ID，最终结论 `5` 已验收、`203` 未完成；受控自动/视觉通过不冒充最终验收。R23-07 已满足，本次只补当前 main 账本状态复算，不重跑逐项行为门禁。
- 当前 R23-04 正常 Playnite 主窗体/UIA 仍受 CEF `platform_channel 0x5` 阻挡；R23-05 真实呈现帧/宿主性能等权限与设备边界仍未验；Q24-03 等待第二物理显示器。下一可执行动作是宿主能正常显示主窗体时用当前候选身份复跑 R23-04；证据见 [R23-07 当前 main 状态复核](evidence/R23-07-CURRENT-MAIN-STATUS-RECHECK-20260924.md)。

## 2026-09-23 R00/R01 证据收口与设置截图行为复核

- 本次 freshness JSON 的 main source identity 为 HEAD 8e4f3194227afb28640754f12ab0889cb8bb71ce；14 条 R00/R01 记录均 needsRerun=false、matchedSourcePaths=0，documentationOnlyChange=false，package identity not-provided。R00-01/02、R00-05 历史证据身份仍保持 3a1dadd8。
- 当前阶段 Q06-08 已在 `8846d712` 完成并推送；按钮五态 Light/Dark 行为与证据已同步。R18-04 复采、23 项组成、TextServicesHost 清理噪声和用户四页问题的边界继续见各自证据；现有用户截图布局行为覆盖为 Light/Dark `8/8`，不等于当前安装包宿主截图已验。
- 设置视图的 Loaded/SizeChanged 自动布局测试 Light/Dark 2/2；覆盖 1280×840、1880×1200、560、恢复 1280×840 DIP。标题/搜索左差 0 DIP，居中负例 287.33 DIP，图标/标题横向间距 12 DIP；顶部恢复默认下拉框与两个按钮均 36 DIP、中心差 0，路径组合框与四按钮同为 36 DIP。8e4f3194 上 Release 解决方案构建 0 errors/2 条既有 CS8602，设置几何用例再跑 2/2；当前包为 0.6.73+8e4f3194，SHA-256 和 TRX 见设置证据页。
- 2026-09-24 本机扩展目录只读 identity 为 `0.6.73+7a4ba2a9`、DLL SHA `5E02…50E22`；该身份早于 `3a1dadd8` 设置搜索修正、`da91bd68` 窗口/媒体滚动修正。**推断：**旧安装可能解释新截图与源码差异，但没有运行中进程关联截图，不能确认呈现源。
- 新截图近似 `1254×800 DIP` 复核 Light/Dark 各 `1/1`、exit `0`：icon/title 顶差 `11.33`、search/title 左差 `0`、search/icon 顶间隔 `72.67`，恢复/路径操作高度与中心线一致。基于 `e83d8ba9` 的可审阅包已生成，身份 `0.6.73+e83d8ba9`、SHA `75A8…B73E`、未安装。具体几何、包构建边界见[窗口化设置截图复核](evidence/SETTINGS-HEADER-WINDOWED-1254x800-CURRENT-MAIN-20260924.md)。受控源码几何通过仍不等于当前用户屏幕已修复；正常 Playnite host、真实 DPI 未验，CEF platform_channel 0x5 限制未绕过。下一项 R08-05。

## 2026-09-24 R08-05 当前 main 页面切换复核

- 当前 main/测试身份 `13c38754` Release solution 构建成功，XAML `24/24`、0 errors，保留两条既有 `MediaCenterView.xaml.cs:703 CS8602` warnings；源校验、XAML 校验与 diff check 通过。
- R08PageSwitch `2/2`、R02 busy `4/4`、R08 business feedback `4/4`、ProductionShellChrome `12/12`、Foundation `9/9`，共 `31/31`、0 failed/skipped，五个 VSTest 进程均 exit `0`。页面滚动 `10→10`；任务→媒体与返回的实际 shell Measure/Arrange 均为 `1/1`；同页重入 `0/0` 为精确断言。
- 壳层 TRX 仅在 testhost 收尾含一段 WPF TextServicesHost `InvalidComObjectException`；结果明确通过，根因未知。其余四份 TRX 无该异常文本。
- 证据：[R08-05 当前 main 复核](evidence/R08-05-CURRENT-MAIN-RECHECK-20260924.md)及五份 TRX。仅合成 DTO/隔离 STA WPF/逻辑 DIP，不表示正常 Playnite、物理呈现或宿主性能通过。用户 Media/Task/Save/Settings 问题继续待候选包身份与正常宿主对照。
- 下一项按用户当前要求核对 R18-04 相关 `23` 项测试组成。

## 2026-09-24 main：R18-04 当前复采与用户四页布局复核

- 当前 Release/test assembly `abd7927b`：XAML `24/24`、0 errors、两条既有 CS8602 warning。R18 专测 `1/1`；四个关联类精确组成 `6+10+3+4=23/23`，均 exit `0`。其中为 10 项纯数据/几何/选择行为、7 项源代码结构契约、6 项隔离 STA WPF 行为，不能写成 23 项真实宿主交互。
- 后续 `b5c7a6d4`/`da91bd68` 调整了 Inbox footer 高度预算并令窄窗 DataGrid 始终保持有限高度；当前 R18 窗口下 Inbox 为 `7/7/7`，不同于较早 `922501e7` 的 `14/14/14`。Task 三档仍为最大实现/可见 `9/7`；Media UI 窗口 2,000 项；当前 Media 最大滚动样本 `0.089/0.031/0.026 ms`。完整样本与 23 项精确方法表见 [R18-04 当前复核](evidence/R18-04-CURRENT-MAIN-RECHECK-20260924.md)。R18/TRX COM 清理文本 6 次，锚点类 2 次，所有结果仍通过，根因未知。
- 对用户四页窗口布局，Light/Dark `ReportedWorkspaceLayoutBehaviorTests 8/8`：Media 动作各 `36 DIP` 且 scrollbar 留在有限 grid；Task 失败行 0 位置误差/0 状态不匹配；Save action row `36 DIP`、间距 `10 DIP`/尾白 `9.33 DIP`；Settings 搜索与标题左差 `0 DIP`、图标/标题顶差 `11.33 DIP`，重置/路径动作同高同中心。结果见 [四页当前 main 复核](evidence/USER-REPORTED-LAYOUT-CURRENT-MAIN-RECHECK-20260924.md)。本机旧 DLL `7a4ba2a9` 早于上述修正，截图可能来自旧包但进程未关联；候选 `e83d8ba9` 包未安装，不能写成实际窗口已修复。
- 外部边界：正常 Playnite host 仍被 CEF `platform_channel 0x5` 阻挡；物理 DPI/presented frame/ETW 与宿主性能未验。下一项回到可执行的 R23-05/其他依赖满足的 Q/R 小批量；不绕过 host 或系统跟踪权限。

## 2026-09-24 R08-04 当前 main 业务完成节奏复核

- 当前生产源码 identity `09ec3132`；Release solution XAML `24/24`、0 errors，保留两条既有 `MediaCenterView.xaml.cs:703 CS8602` warnings。R08-04 实现/测试文件自 `43141399` 后未变。
- 定向行为按类串行 `R02BusyStateTests 4/4` + `R08BusinessFeedbackBehaviorTests 4/4` = `8/8`，0 fail/skip、VSTest exit `0`；两份 TRX 无 COM 清理异常文本。
- 慢任务在 action 完成前保持忙态，按钮尺寸/内容/焦点稳定；快速完成 `150ms` 后 spinner 仍折叠；重复请求门禁、失败/取消恢复通过。成功/失败/取消 DTO 与 Toast AutomationPeer 反馈一致。
- 仅合成 DTO/fake、隔离 STA WPF/逻辑 DIP；真实 Playnite/Worker 长任务、Narrator/真实 UIA、物理 DPI/呈现帧/ETW/宿主性能未验。
- 证据：[R08-04 当前 main 复核](evidence/R08-04-CURRENT-MAIN-RECHECK-20260924.md) 与两份 TRX。下一项 R08-05 页面切换轻量化。

## 2026-09-24 R08-03 当前 main 离屏与隐藏停机复核

- 当前生产源码 identity `378ceb13`；完整 Release solution 构建 XAML `24/24`、0 errors，保留两条既有 `MediaCenterView.xaml.cs:703 CS8602` warnings。补充行为断言后 Playnite 测试项目构建 0 warning/error，Playnite `net462`、测试 `net472`。
- 原有行为已验证真实模板 storyboard 隐藏/最小化时暂停、恢复后继续，但没有直接断言业务忙碌态不变。本次在隐藏、最小化、等待与恢复节点补 `IsIndeterminate == true` 断言。五类当前程序集串行行为 `25/25`：Offscreen `1`、HotChange `1`、Reverse `2`、ShellChrome `12`、Foundation `9`；均 0 fail/skip、VSTest exit `0`。
- 可见位移 `>0.5 DIP`；隐藏 Tab 与最小化各等待 `360ms`，Transform 位移保持至小数后三位；恢复后均继续。HotChange/Reverse/ShellChrome 共 10 段 WPF TextServicesHost COM 清理噪声（8/1/1），Offscreen/Foundation 无；根因未知。
- 仅隔离 STA WPF/合成 Tab/逻辑 DIP；正常 Playnite 页面或嵌入窗口、物理 DPI/跨屏、UIA、呈现帧、ETW/宿主性能未验。旧资源字典宽回归的 Save 时间列源码期望失败仍按原证据保留，不纳入本项。
- 证据：[R08-03 当前 main 复核](evidence/R08-03-CURRENT-MAIN-RECHECK-20260924.md) 与五份 TRX。下一项 R08-04 业务完成节奏。

## 2026-09-24 R08-02 当前 main 热关闭动画复核

- 当前 `main` 全解决方案 Release 构建/程序集身份 `9002668c`：XAML `24/24`、0 errors，保留两条既有 `MediaCenterView.xaml.cs:703 CS8602` warning；Playnite `net462`、测试 `net472`。
- 按类串行复核 HotChange `1/1`、Reverse `2/2`、ShellChrome `12/12`、MotionFoundation `9/9`，共 `24/24`、均 exit `0`。设置关闭动画后 SettingsShell opacity/Y 动画立即清理并归中性值；重新打开不重放入场。侧栏/Translate 反向从有效中点接管到最新目标。
- 前一程序集 `4b7f0a34` 被仓库 identity gate 拦下 7 条源码检查；在当前 checkout 重建后全部通过，记录为过期测试程序集门禁。3 份 WPF TRX 有总计 10 段 TextServicesHost COM 清理日志，根因未知。证据见 [R08-02 当前 main 复核](evidence/R08-02-CURRENT-MAIN-RECHECK-20260924.md) 及四份 TRX。下一项 R08-03 离屏与隐藏停机。

## 2026-09-24 R08-01 当前 main 动画反向与设置顶栏复核

- 当前 main Release solution 构建成功：XAML `24/24`、0 errors、两条既有 `MediaCenterView.xaml.cs:703 CS8602` warning；Playnite `net462`、测试 `net472`。生产代码身份 `0c0869a2`，本次 `4b7f0a34` 仅提交证据/测试。
- R08-01 动画行为 `2/2`；Translate 从当前采样 `9.287` 连续反向到 `-8`，侧栏从 `184` 连续扩展到 `270`，动画时钟清理。相邻壳层/基础动画/设置几何 `23/23`（Chrome `12`、Foundation `9`、Settings `2`）。两个 TRX 都有 TextServicesHost COM 清理异常，根因未知，但结果 exit `0`。
- 用户最新设置截图继续显示搜索框和图标偏移；当前生产设置视图隔离 STA WPF Light/Dark `2/2`：标题/搜索左差 `0 DIP`、居中负例 `287.33 DIP`；恢复默认和路径控件高度/中心差为 `0`。用户包身份、真实 Playnite 父容器与物理 DPI 未核实，不能声明用户屏幕问题已关闭。详见 [R08-01 当前 main 复核](evidence/R08-01-CURRENT-MAIN-RECHECK-20260924.md)、[设置顶栏最新截图复核](evidence/SETTINGS-HEADER-CURRENT-MAIN-RECHECK-20260924.md) 及对应 TRX。
- 下一项 R08-02 热关闭动画；用户截图差异继续作为待验边界，若 package identity/正常 host 暂不可得，不阻塞独立 Q/R 小批量。

## 2026-09-24 R07-05 当前 main 复核

- `main` 源码/测试构建身份 `ec9b0a21`；Release 隔离构建 XAML `24/24`、0 errors、保留 `MediaCenterView.xaml.cs:703` 两条既有 `CS8602` warning。
- R07-05 与相邻当前行为共 `17/17`、0 failed/skip：断点协调器 `5`、详情保持 `1`、滚动所有权 `2`、Task 响应 `7`、详情预算 `2`。旧 `8e4f3194` 测试程序集先被源码身份保护拦下 4 个源码读取用例；按当前 `ec9b0a21` 重建后完整复跑通过，不计为产品失败。
- TRX 收尾包含 WPF TextServicesContext `InvalidComObjectException`，VSTest exit `0`，根因未知。测试是隔离 STA WPF/合成 DTO/逻辑 DIP，不表示 Playnite shell 窗口拖动或物理 DPI 通过。
- [R07-05 当前 main 复核](evidence/R07-05-CURRENT-MAIN-RECHECK-20260924.md) 与 [TRX](evidence/R07-05-CURRENT-MAIN-RECHECK-20260924.trx) 已归档。下一项：R07-06 当前 main 状态横幅预算复核。

## 2026-09-24 R07-06 当前 main 复核

- 当前生产源码身份 `2e591b9f6217c1dcf7ac5d0494a55a7e6e230c82`；隔离 Release 构建 XAML `24/24`、0 errors、保留两条 `MediaCenterView.xaml.cs:703 CS8602` warning。测试类与 fake 命令行为补强后重建无警告/错误。
- R07-06 状态横幅与相邻空态/Task/详情/滚动门禁共 `17/17`，0 failed/skip。新增 actual WPF AutomationPeer `IInvokeProvider` 激活，计数验证五个重试/恢复命令；横幅只保留明确按钮，Task 状态恢复后收起。没有执行真实业务命令。
- TRX 有 9 段 WPF TextServicesHost/TextStore `InvalidComObjectException` 清理输出，原因未知；xUnit/VSTest 17/17、exit `0`。隔离 STA WPF/合成 DTO/DIP，不代表 Playnite UIA/宿主/物理呈现。
- [R07-06 当前 main 复核](evidence/R07-06-CURRENT-MAIN-RECHECK-20260924.md) 和 [TRX](evidence/R07-06-CURRENT-MAIN-RECHECK-20260924.trx)；下一项 R07-07 当前 main 触控板增量复核。

## 2026-09-24 R07-08 当前 main 复核

- 当前 `main` 源码身份 `0c0869a2a95a9e949af5dc257bf7c7b4da2f80b6`；Release solution 构建 XAML `24/24`、0 errors，保留两条既有 `MediaCenterView.xaml.cs:703 CS8602` warning。最终测试项目重建 0 warning/error。
- R07 resize 压力序列及相邻断点、Task、滚动、Media 几何 `21/21`，0 fail/skip、VSTest exit `0`。测试现在只调外层 Window 尺寸并监听生产 shell `SizeChanged`，不再手动调用布局私有路径；宽/窄/短/恢复都保持详情、选择器和搜索焦点，详情限高与选择器预算随尺寸正确变化。
- [R07-08 当前 main 复核](evidence/R07-08-CURRENT-MAIN-RECHECK-20260924.md)/[TRX](evidence/R07-08-CURRENT-MAIN-RECHECK-20260924.trx)。TRX 有 6 段 WPF 文本服务 `InvalidComObjectException` 收尾噪声，根因未知且 exit 0。证据是隔离 STA WPF/合成任务/逻辑 DIP，不是正常 Playnite/物理拖动/跨屏/DPI/最终帧/ETW/宿主性能。下一项 R08-01 当前 main 中途反向连续复核。

## 2026-09-24 R07-07 当前 main 复核

- 当前 `main` 身份 `d46fc76e`；细滚动路由实现未变，但共享 WPF 生产资源和 DataGrid/Task 相关布局已变，故按当前控件重新测量。Release solution XAML `24/24`、0 errors，保留两条既有 `MediaCenterView.xaml.cs:703 CS8602` warning。
- R07 细滚动 `2/2`、滚动所有权 `2/2`、Media Inbox geometry `3/3`、响应断点 `5/5`、Task responsive `7/7`，共 `19/19`、0 fail/skip。offset/布局/容器数字与旧样本一致：单次 -30 为 +3 DIP、反向 +120 单调、末端 `154/154`、20 事件布局 `17`、末端无效事件新增布局 `0`、行容器 `6–9`；嵌套 -360 单次布局 `1`。
- [R07-07 当前 main 复核](evidence/R07-07-CURRENT-MAIN-RECHECK-20260924.md)/[TRX](evidence/R07-07-CURRENT-MAIN-RECHECK-20260924.trx)。合成 routed wheel 与隔离 WPF，不等同真实触控板/Playnite/物理跨屏或宿主帧性能。下一项 R07-08 当前 main resize 压力序列复核。

# 第三轮任务账本

初始规划日期：2026-09-15。任务源：[192 项实施文件](../../UI_FINESSE_ROUND3_192_TASKS_2026-09-15.md)；依据：[质量审查](../../UI_FINESSE_QUALITY_REVIEW_2026-09-15.md)。审查基线 35b8276，实施时核对当前分支最新提交。初始全部待开始，不清零旧 Q 账本。

> 2026-09-23 R16-07 配置导入预览定向复核：本批没有新增生产代码，在 `01e83d8a` 上复用 `451195ad` 的 portable settings v1、detached 预览、确认后应用、未知字段说明、设备身份保护和异常回滚。`R16SettingsImportPreviewBehaviorTests 4/4`、源码 `1/1`、既有 `PortableSettingsTests 10/10`，合计 `15/15`；当前提交隔离 Release solution `0 errors/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；`validate-source.py`、XAML `24/24`、diff check、WPF `0/28/162` 通过。未验真实 Playnite/package-host 文件选择器/MessageBox/保存取消、UIA/焦点、DPI/跨屏、presented frame、ETW 或宿主性能；Demo 原目录不可用。详见 [R16-07 配置导入预览定向复核](evidence/R16-07-SETTINGS-IMPORT-PREVIEW-RECHECK-20260923.md)。下一项推进 R16-08。

> 2026-09-23 R16-08 保存冲突处理定向复核：本批没有新增生产代码，在 `afe80186` 上复用 `ee6b37c9` 的三方字段合并、冲突事件/异常、草稿取消基线和保存失败分流。`R16SettingsConflictBehaviorTests 3/3`、源码 `1/1`、脱离视图 WPF `1/1`、保存反馈 `2/2`、既有 `PortableSettingsTests 10/10`，合计 `17/17`；当前提交隔离 Release solution `0 errors/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；`validate-source.py`、XAML `24/24`、diff check、WPF `0/28/162` 通过。未验真实 Playnite 双设置窗口/共享 settings 竞态、宿主错误呈现、最终呈现、DPI/UIA/IME、presented frame、ETW 或宿主性能；合成 detached/fake/隔离目录，Demo 原目录不可用。详见 [R16-08 保存冲突处理定向复核](evidence/R16-08-SETTINGS-CONFLICT-RECHECK-20260923.md)。下一项推进 R17-01。

> 2026-09-23 R17-01 健康结果分层定向复核：本批没有新增生产代码，在 `3f9d8e2b` 上复用 `eb033251` 的 `resolved=0` 开放队列、健康巡检 resolve、证据时间、跨来源去重和三档影响分组。Playnite `R17FindingTriageBehaviorTests + R17FindingTriageSourceTests 5/5`、Worker 隔离 SQLite `R17FindingPersistenceTests 2/2`，合计 `7/7`；当前提交隔离 Release solution `0 errors/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；`validate-source.py`、XAML `24/24`、diff check、WPF `0/28/162` 通过。未验真实 Playnite/package-host、多来源生产标题规范、最终呈现、DPI/UIA/IME、presented frame、ETW 或宿主性能；合成 DTO/fake/隔离 SQLite，Demo 原目录不可用。详见 [R17-01 健康结果分层定向复核](evidence/R17-01-HEALTH-RESULT-LAYERS-RECHECK-20260923.md)。下一项推进 R17-02。

> 2026-09-23 R17-02 诊断包预览定向复核：本批没有新增生产代码，在 `837528bc` 上复用 `2b6e9051` 的只读预览、类别/脱敏/排除清单、确认前置和生成后结果展示。Playnite R17 定向 `7/7`；Worker 诊断预览与既有真实 ZIP 脱敏/上限 `2/2`，R17-01 SQLite 回归 `2/2`，相关合计 `11/11`；当前提交隔离 Release solution `0 errors/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；`validate-source.py`、XAML `24/24`、diff check、WPF `0/28/162` 通过。未验真实 Playnite/package-host 确认框/Explorer/权限/日志并发、最终呈现、DPI/UIA/IME、presented frame、ETW 或宿主性能；合成请求/fake/隔离 SQLite/临时目录，Demo 原目录不可用。详见 [R17-02 诊断包预览定向复核](evidence/R17-02-DIAGNOSTICS-PACKAGE-PREVIEW-RECHECK-20260923.md)。下一项推进 R17-03。

> 2026-09-23 R17-03 检查进度预算定向复核：本批没有新增生产代码，在 `e0b5cd15` 上复用 `87473bc3` 的持久化游标、时间预算、运行/操作锁门禁、延后候选和取消/失败终态。Worker `HealthInspectionServiceTests 12/12`；Playnite R17 合并回归 `10/10`，含 R17-03 DTO/维护页 `3/3`；当前提交隔离 Release solution `0 errors/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；`validate-source.py`、XAML `24/24`、diff check、WPF `0/28/162` 通过。未验真实 Playnite/package-host、真实游戏/锁/超时竞态、最终呈现、DPI/UIA/IME、presented frame、ETW 或宿主性能；合成会话/fake 锁/隔离 SQLite/归档，Demo 原目录不可用。详见 [R17-03 检查进度预算定向复核](evidence/R17-03-INSPECTION-PROGRESS-BUDGET-RECHECK-20260923.md)。下一项推进 R17-04。

> 2026-09-23 R16-06 生效条件说明定向复核：本批没有新增生产代码，在 `9cef273b` 上复用 `83e7c745` 的四类生效提示和真实消费链标注。`R16SettingsEffectSourceTests 2/2` 加 R16-05 路径回归 `6/6` 通过；当前提交隔离 Release solution `0 errors/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；`validate-source.py`、XAML `24/24`、diff check、WPF `0/28/162` 通过。源契约范围已明确为文案/链路/负例，不冒充真实宿主时序；未验真实 Playnite/package-host、Worker 重启/轮询、UIA/焦点、DPI/跨屏、presented frame、ETW 或宿主性能；Demo 原目录不可用。详见 [R16-06 生效条件说明定向复核](evidence/R16-06-SETTINGS-EFFECT-CONDITIONS-RECHECK-20260923.md)。下一项推进 R16-07。

> 2026-09-23 R16-05 路径编辑一致定向复核：本批没有新增生产代码，在 `3121d337` 上复用 `955dc52e` 的统一路径编辑卡片、只读探测、当前字段 Binding 和剪贴板重试。`R16SettingsPathEditorBehaviorTests + R16SettingsPathEditorSourceTests + SettingsPathValidationTests 6/6` 通过；当前提交隔离 Release solution `0 errors/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；`validate-source.py`、XAML `24/24`、diff check、WPF `0/28/162` 通过。有效/缺失/文件误作目录、保存校验和远端目标排除均有行为或契约证据。未验真实 Playnite/package-host、Explorer/文件夹对话框、UIA/焦点、DPI/跨屏、presented frame、ETW 或宿主性能；Demo 原目录不可用。详见 [R16-05 路径编辑一致定向复核](evidence/R16-05-PATH-EDITOR-RECHECK-20260923.md)。下一项推进 R16-06。

> 2026-09-23 R16-03 模板应用范围定向复核：本批没有新增生产代码，在 `1f968819` 上复用 `52fbf5de` 的批量模板预览、稳定 ID 目标集合、逐项 Worker 执行和失败重试。Core `PolicyTemplateBatchPreviewTests 3/3`、Worker 持久化 `2/2`、Playnite 批量模板源契约 `1/1`；`net462` 定向构建无错误，仅保留 `MediaCenterView.xaml.cs:706` 两条既有 warning；`validate-source.py`、XAML `24/24`、diff check 通过。显式勾选/排除、100 个上限、空选择、逐项结果、取消和稳定 ID 均有行为或契约证据，R16-03 校正为“已满足，待环境验证”。未验真实 Playnite 批量点击/筛选/UIA/焦点、DPI/跨屏、presented frame、ETW 或宿主性能；Demo 原目录不可用。详见 [R16-03 模板应用范围定向复核](evidence/R16-03-POLICY-TEMPLATE-BATCH-RECHECK-20260923.md)。下一项推进 R16-04。

> 2026-09-23 R16-04 恢复默认粒度定向复核：本批没有新增生产代码，在 `666c60bb` 上复用 `2b194461` 的设置恢复目录和 Playnite 草稿取消语义。`R16SettingsDefaultsBehaviorTests 2/2`、`R16SettingsDefaultsSourceTests 1/1` 通过；当前提交隔离 Release solution `0 errors/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；`validate-source.py`、XAML `24/24`、diff check 通过。首次直接复用旧程序集触发 `1f968819`/`666c60bb` 身份保护，随后从当前 checkout 重建后通过，记录为测试输出边界而非功能失败。R16-04 校正为“已满足，待环境验证”。未验真实 Playnite/package-host 点击/最终呈现、DPI/UIA/IME、presented frame、ETW 或宿主性能；Demo 原目录不可用。详见 [R16-04 恢复默认粒度定向复核](evidence/R16-04-RESET-GRANULARITY-RECHECK-20260923.md)。下一项推进 R16-05。

> 2026-09-23 R16-02 策略差异预览定向复核：本批没有新增生产代码，在 `e63c62c7` 上复用 `b327d5ef` 的策略 DTO、模板目录、13 字段差异比较和保存/取消链。Core `BackupPolicyTemplateCatalogTests 5/5`，Playnite 策略源契约 `2/2`；`net462` 定向构建无错误，仅保留 `MediaCenterView.xaml.cs:706` 两条既有 warning；`validate-source.py`、XAML `24/24`、diff check 通过。已保存/显式草稿/模板覆盖差异、复制回退、取消不发 Worker 请求和未保存草稿禁用模板均有行为/负例证据，R16-02 校正为“已满足，待环境验证”。未验真实 Playnite 差异卡片、浅深主题、UIA/读屏/IME、DPI/跨屏、presented frame、ETW 或宿主性能；Demo 原目录不可用。详见 [R16-02 策略差异预览定向复核](evidence/R16-02-POLICY-DIFF-RECHECK-20260923.md)。下一项推进 R16-03。

> 2026-09-23 R16-01 设置搜索定位定向复核：本批没有新增生产代码，在 `ae9aedbc` 上复用 `a4e35578` 的设置字段索引和原有控件绑定。`R16SettingsSearchBehaviorTests 1/1`、`SettingsValidationSourceTests 1/1`、验证导航 `1/1`、草稿生命周期 `1/1` 均在独立 testhost 通过；Playnite `net462` 定向构建无错误，仅保留 `MediaCenterView.xaml.cs:706` 两条既有 warning；`validate-source.py`、XAML `24/24`、diff check 通过。搜索命中/清空恢复、可编辑性、配置不写入和 pending edit 不变均有行为证据，R16-01 校正为“已满足，待环境验证”。合并 WPF testhost 的 `Application` 多实例冲突作为环境边界记录。未验真实 Playnite 最终布局、浅深主题、UIA/读屏/IME、DPI/跨屏、presented frame、ETW 或宿主性能；Demo 原目录不可用。详见 [R16-01 设置搜索定位定向复核](evidence/R16-01-SETTINGS-SEARCH-RECHECK-20260923.md)。下一项推进 R16-02。

> 2026-09-23 R15-08 清理历史范围定向复核：本批没有新增生产代码，在 `569fa1d8` 上复用既有 Retention Simulation、保护门禁和隔离账本。Worker `RetentionSimulationServiceTests + RetentionQuarantineRecoveryTests 17/17`，Playnite 维护页/账本契约 `6/6`；`net462` 定向构建无错误，仅保留 `MediaCenterView.xaml.cs:706` 两条既有 warning；`validate-source.py`、XAML `24/24`、diff check 通过。预览句柄、二次确认、运行中/锁定/健康/PreRestore 保护、共享锁忙碌跳过和账本恢复均有行为证据，R15-08 保持“已满足”。未验真实 Playnite 清理呈现、UIA/读屏/IME、DPI/跨屏、presented frame、ETW 或宿主性能；Demo 原目录不可用。详见 [R15-08 清理历史范围定向复核](evidence/R15-08-HISTORY-CLEANUP-SCOPE-RECHECK-20260923.md)。下一项推进 R16-01。

> 2026-09-23 R15-07 失败结果复制定向复核：本批没有新增生产代码，在 `d9dc4317` 上复用 `37dd4a03` 的任务复制、统一脱敏、有限高详情和重试语义。Playnite `R15TaskFailureCopyTests` 加相邻剪贴板/恢复/任务详情套件 `21/21`，`net462` 定向构建无错误，仅保留 `MediaCenterView.xaml.cs:706` 两条既有 warning；R15-07 校正为“已满足，待环境验证”。短摘要、密码负例、完整脱敏 payload、只读可选择详情、第三次成功和四次失败均有行为证据。未验真实系统剪贴板/Playnite UIA/读屏/IME、DPI/跨屏、presented frame、ETW 或宿主性能；Demo 原目录不可用。详见 [R15-07 失败结果复制定向复核](evidence/R15-07-TASK-FAILURE-COPY-RECHECK-20260923.md)。下一项推进 R15-08。

> 2026-09-23 R15-06 耗时与吞吐定向复核：本批没有新增生产代码，在 `d9dc4317` 上复用 `6f65638e` 的单调采样和可靠总量门控。Worker 采样/查询/广播/失败链 `22/22`，Playnite 进度/快照/耗时套件 `5/5`；Playnite `net462` 定向构建无错误，仅保留 `MediaCenterView.xaml.cs:706` 两条既有 warning；`validate-source.py`、XAML `24/24`、diff check 通过。两个推进样本、未知总量、等待确认、10 秒停顿和普通阶段清空均有行为证据，R15-06 校正为“已满足，待环境验证”。未验真实 Playnite Task Center 呈现、UIA/读屏/IME、DPI/跨屏、presented frame、ETW 或宿主性能；Worker 全量既有失败和 Demo 原目录边界不变。详见 [R15-06 耗时与吞吐定向复核](evidence/R15-06-TASK-THROUGHPUT-RECHECK-20260923.md)。下一项推进 R15-07。

> 2026-09-23 R15-05 任务来源定位定向复核：本批没有新增生产代码，在 `ba4d624b` 上复用 `0d1ff346` 的稳定来源引用与精确解析。Playnite `R15TaskSourceNavigationTests 3/3`，Worker `TaskQueryPersistenceTests + TaskEventBroadcasterTests + TaskCoordinatorFailureTests 21/21`；Playnite `net462` 定向构建无错误，仅保留 `MediaCenterView.xaml.cs:706` 两条既有 warning。已删除游戏不跳同名游戏、已删除版本不选邻近版本、来源 clone 保留稳定诊断身份，R15-05 校正为“已满足，待环境验证”。未验真实 Playnite 来源卡片、UIA/读屏/IME、DPI/跨屏、presented frame、ETW 或宿主性能；Demo 原目录不可用。详见 [R15-05 来源定位定向复核](evidence/R15-05-TASK-SOURCE-LOCATION-RECHECK-20260923.md)。下一项推进 R15-06。

> 2026-09-23 R23-05 壳层断点几何复测：本批只扩展 RenderHarness 的实际 WPF 行为探针，提交 `b5e7fca0` 覆盖 `1200/1279/1280/1366 DIP` 的 compact/expanded 边界，检查标题/动作区的真实分行或不重叠及 `HeaderSurface` 内边界。提交后 clean `shellqa` exit `0`，`WorkingTreeClean=True`；`ResponsiveLayoutCoordinatorTests` + `MediaInboxGeometryTests` `8/8`。R23-05 仍为“部分满足，待宿主性能验收”，不把离屏、Rendering/Stopwatch 或逻辑 DIP 写成 presented frame/物理刷新率结论。详见 [R23-05 壳层断点几何复测](evidence/R23-05-HEADER-BREAKPOINT-RECHECK-20260923.md)。下一项不重复几何探针，转依赖已满足的 Q/R 行为小批量。

> 2026-09-24 R21-05 当前身份复测校正：实现已在当前历史提交 `380234e2` 中存在，本批没有重建生产代码；D 盘 `codex/ui-finesse-round2` 以 `1f69b803` 重新生成隔离 Release 产物，实际 `R21DisabledHiddenBehaviorTests 2/2` 通过。当前 Playnite `net462` 构建为 `0 error`，保留 `MediaCenterView.xaml.cs:706` 两条既有 `CS8602` warning。证据只覆盖实际 `SaveCenterView`/共享按钮/隔离 STA WPF 的 Collapsed 隐藏、可读解释、恢复命令和禁用 Invoke 负例；真实 Playnite/UIA/读屏、OS 输入/IME、DPI/跨屏、呈现帧、ETW 和宿主性能仍待验。下一可执行项保留 R23-04 正常可枚举宿主会话；阻塞时转依赖已满足的小批量。

每个 ID 分别记录实现与验收。状态不能由自动通过推导为已验收；不需要的维度写“不适用+原因”。每阶段更新本表，不以重复的聚合报告替代具体条件。可在行后链接详细证据，保持表格简短。

> 2026-09-21 接续点校正：用户指出最近交接应以 R12-04 为准。核对后 R12-04 已由 `e4e42f40` 满足，R12-05 至 R12-08 也已有独立历史证据；本轮不重做、不回滚，也不抹去已完成的后续事实。当前按真实账本继续处理首个未完成项。

> 2026-09-22 R13/R14 定向复核校正：`8a9a052e` 只补 RenderHarness 合成 DTO，不改生产服务/DTO/命令/绑定。Core `40/40`、Worker `32/32`、Playnite R13/R14 定向 `19/19`；clean-tree RenderHarness 报告绑定 `8a9a052e`，Maintenance 无 R13 专属问题，Media 重复组 `2` 组/组内 `4` 项、来源试运行 `4` 项且无 R14 专属问题。全报告仍因既有 Overview/Settings/Task/Save 基线以 `FAILED` 结束，不能写成全局 `render-qa OK`；本批真实 Playnite、网络/外部工具、UIA/IME、物理 DPI/跨屏、presented frame、ETW 和宿主性能仍未验。详见 [R13/R14 定向复核](evidence/R13-R14-TARGETED-RECHECK-20260922.md)。R13-07/R13-08/R14-01～R14-08 的“定向夹具未执行”历史字段以本条和证据为当前事实；状态仅在受控范围内补齐，不扩大成真实宿主通过。下一项转 R23-05 独立几何/性能边界或等待可枚举 Playnite 宿主会话。

> 2026-09-22 R22-01 本地镜像时间摘要校正：`b45e31db` 只复用已有 `LastSyncRelativeDisplay`/`LastSyncFullDisplay`，将 Worker 状态摘要和维护页紧凑镜像时间行从旧 `LastSyncDisplay` 改为相对时间，未改同步、命令、滚动、镜像删除保护或兼容投影。Worker 镜像 `6/6`、Playnite `R22TimeDisplayBehaviorTests 30/30`；clean `shellqa` 和全量 RenderHarness 绑定 `b45e31db`、`WorkingTreeClean=True`，Maintenance 浅/深主题目标尺寸无本批新增问题。全量 RenderHarness 仍因 Overview/Settings/Task/Save 既有基线 `FAILED`，不写成全局通过；详见 [R22-01 本地镜像时间摘要](evidence/R22-01-LOCAL-MIRROR-TIME-20260922.md)。真实 Playnite/UIA/读屏、OS 输入/IME、DPI/跨屏、呈现帧、ETW 和宿主性能仍未验；下一项回到 R23-04 UIA/Controlled host。

> 2026-09-22 R23-04 窗口暴露证据校正：`c6d65b08`/`2d327d4b`/`eba374c5` 为真实宿主 runner 增加 `host-window-exposure.json`、显式跳过安装前测试开关和隔离 profile 配置自举；`DiagnosticsEvidenceSourceTests 2/2`，当前提交 Release `-SkipTests` 构建 XAML `24/24`、`0` errors，保留既有两条 `MediaCenterView.xaml.cs:699 CS8602`。隔离 Playnite PID `31920` 的可见窗口实际为 `Startup Error`，`MainWindowHandle=0x19F08B4`、顶层窗口 `5` 个、UIA 侧栏未确认，`cef.log` 为 `platform_channel ... 拒绝访问 (0x5)`；runner 自行从隔离备份恢复配置，无 `summary.json`，仍按 `[PARTIAL]` 结束。窗口存在不等于正常主窗体，更不等于 UIA/Controlled/键盘/读屏通过；详见 [R23-04 窗口暴露边界复测](evidence/R23-04-UIA-WINDOW-EXPOSURE-RECHECK-20260922.md)。单屏 Q24-03、ETW/最终呈现帧/宿主性能和 Demo 原目录边界不变；下一可执行任务是取得正常可枚举 Playnite 会话，若继续受阻则转 R23-05 独立几何小批量。

> 2026-09-23 R00/R01 当前身份与证据校正：代码修复提交 `b5c7a6d4` 将 Media Inbox 的窄宽/紧凑高度切回页级滚动，并把包含 footer 的表格预算改为 `360 DIP`；`MediaInboxGeometryTests 3/3`、`MediaWindowAnchorContractTests 10/10`、Light/Dark `mediageometryprobe`、`toolbarprobe`、`shellqa` 和 `scaleprobe` 均在同一干净隔离构建上完成。完整审计索引 `20/20` 可追溯，但真实 summary 为 `168` 快照、`0` Fidelity、`0` 失败路由、`7` 条 HIGH、`4` 条 MEDIUM；已修正文档中旧的 0 风险摘要。动画 `UiFinesseFoundationTests 9/9` 通过，额外 motion probes 在当前 Dispatcher 环境未稳定通过，保留为待验边界，不改动生产动效。freshness 报告已绑定当前完整 SHA，`14/14 fresh`、包身份 `not-provided`。下一项继续 R23-04 可枚举宿主会话；宿主受阻时推进独立的 R23-05/相关 Q/R 小批量。

> 2026-09-23 R23-04 runner UIA 候选探测：`f551359c` 复用 Win32 顶层窗口枚举，逐窗建立 UIA 根节点并把候选/匹配动作写入 `host-window-exposure.json`；真实隔离 Playnite PID `3556` 的可见窗口仍为 `Startup Error`，顶层窗口 `5`，60 秒/30 次探测建立 5 个 UIA 根节点但未找到 `GameSaveCenter` 侧栏，`summary.json` 未生成，`cef.log` 为 `platform_channel ... 拒绝访问 (0x5)`。不把窗口/根节点存在写成 UIA/键盘/读屏/Controlled host 通过；详见 [R23-04 UIA 窗口候选复测](evidence/R23-04-UIA-WINDOW-EXPOSURE-RECHECK-20260923.md)。下一项是正常可枚举宿主会话，条件不变则推进依赖已满足的 Q/R。

> 2026-09-23 R22-01 残余入口复核：本批没有生产代码变更；在 `df6bee9d` 上扫描生产 `Views`/`ViewModels`/`Contracts` 的旧时间入口，未发现生产 XAML 绑定到旧本地直显投影。现有兼容/报告/日志/内部属性保留，相对时间、完整本地时间、原始 UTC 和未知语义已有实际行为证据。`R22TimeDisplayBehaviorTests 30/30`、`R10RecentAccessBehaviorTests 2/2`，隔离 Release XAML `24/24`、Playnite `net462`、`0 error`，仅保留 `MediaCenterView.xaml.cs:706` 两条既有 `CS8602` warning。详见 [R22-01 残余时间入口复核](evidence/R22-01-RESIDUAL-ENTRY-RECHECK-20260923.md)。不把生产源码负向审计写成真实 Playnite/UIA/读屏、OS 输入/IME、DPI/跨屏、presented frame、ETW 或宿主性能通过；下一项保留 R23-04 正常可枚举宿主会话，若受阻则推进独立 Q/R。

> 2026-09-23 R00/R01 动效探针时序复核：本批只修正 RenderHarness 采样夹具，没有生产动效代码变更；`0e468873` 先完成深色主题响应式布局，再以有界活动/完成态等待和实际观察值收口 Dispatcher 首帧晚到误判。当前 RenderHarness Release Playnite `net462` 构建 `0 error`；`motionprobe`、`motionreentryprobe`、`motionhotprobe`、`motioncycleprobe` Light/Dark 均 exit `0`，`UiFinesseFoundationTests 9/9`。R00-03 与 R01-04 从“探针待稳定复核”校正为“已满足”；受控窗口/离屏 logical DIP 不等价真实 Playnite 输入、UIA/读屏、DPI/跨屏、presented frame、ETW 或宿主性能。详见 [R00/R01 动效探针时序复核](evidence/R00-R01-MOTION-PROBE-RECHECK-20260923.md)。下一项 R23-04 正常可枚举宿主会话，受阻则推进独立 Q/R。

> 2026-09-24 R14-06 批量目标防误选定向复核：本批没有生产代码变更；在 `89f07445` 上复用 `cfbb1279` 的 IconPath、IdentityDisplay、Games/SelectedItem/TargetPlayniteId 和现有选框过滤保护，新增同名对象按 Playnite ID 保持区分的行为夹具。`GamePickerViewModelTests 22/22`、`R14ClassificationSelectionTests 4/4`、`GamePickerKeyboardBehaviorTests 6/6`，隔离 Release XAML `24/24`、solution `0 error/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning、Playnite `net462` 编译通过；既有 `Shell-Media-1040x700` 截图只证明图标/平台/稳定 ID 的 offscreen 存在性，不证明同名下拉运行时、真实 Playnite 呈现或宿主性能。详见 [R14-06 目标防误选定向复核](evidence/R14-06-TARGETED-RECHECK-20260924.md)。真实 Playnite/UIA/读屏/IME、DPI/跨屏、presented frame、ETW 和宿主性能仍待验；下一项推进 R14-07。

> 2026-09-24 R14-07 媒体详情浏览定向复核：本批没有生产代码变更；当前 `89f07445` 重新生成隔离产物后，详情导航/锚点/缩略图/视频回退/异步 generation 选定套件 `46/46` 通过，Release XAML `24/24`、solution `0 error/2` 条既有 warning、Playnite `net462` 编译通过。既有 `Media-1040x700-tab1` 仅显示媒体网格和“查看媒体详情”入口，未打开详情面板，不把它写成详情呈现证据。详见 [R14-07 媒体详情浏览定向复核](evidence/R14-07-TARGETED-RECHECK-20260924.md)。跨页导航、真实视频编解码、Playnite/UIA/读屏/IME、DPI/跨屏、presented frame、ETW 和宿主性能仍待验；下一项核对 R14-08。

> 2026-09-24 R14-08 来源规则试运行定向复核：本批没有生产代码变更；当前 `6d426f6e` 隔离 Release 构建为 XAML `24/24`、solution `0 error/2` 条既有 warning、Playnite `net462`，Worker `MediaSyncServiceTests 20/20`、Playnite `R14SourceRulePreviewTests 1/1` 通过。既有 `Media-1600x900-tab3` 截图显示合成试运行 `4` 项、命中 `2`、排除 `2`、原因/大小/隔离路径和有限列表滚动。真实目录权限、超大目录、Playnite 最终呈现、DPI/UIA/IME、presented frame、ETW 和宿主性能仍待验。详见 [R14-08 来源规则试运行定向复核](evidence/R14-08-TARGETED-RECHECK-20260924.md)。下一项推进 R15-01。

> 2026-09-24 R15-01 任务阶段可读定向复核：本批没有生产代码变更；当前 `9a63e7e2` 的 Worker 任务阶段套件合计 `16/16`、Playnite `R15TaskStageTests 2/2` 通过，隔离 Release XAML `24/24`、solution `0 error/2` 条既有 warning、Playnite `net462` 编译通过。既有 `Task-1600x900` 截图显示阶段列、正常阶段与“阶段未知”负例及详情区，但不证明真实宿主事件覆盖或最终呈现。详见 [R15-01 任务阶段可读定向复核](evidence/R15-01-TARGETED-RECHECK-20260924.md)。下一项推进 R15-02。

> 2026-09-24 R15-02 取消过程展示定向复核：本批没有生产代码变更；当前 `c879cf43` 的 Worker 取消套件 `16/16`、Playnite `R15TaskCancellationTests + R15TaskStageTests + R06TaskProgressBehaviorTests 8/8` 通过，隔离 Release XAML `24/24`、solution `0 error/2` 条既有 warning、Playnite `net462` 编译通过。既有 `Task-1600x900` 显示合成“已取消”终态和详情动作区，但未捕获“正在取消”过渡帧。真实长任务取消、Playnite/UIA/读屏/IME、DPI/跨屏、presented frame、ETW 和宿主性能仍待验。详见 [R15-02 取消过程展示定向复核](evidence/R15-02-TARGETED-RECHECK-20260924.md)。下一项推进 R15-03。

> 2026-09-24 R15-03 任务详情时间线定向复核：当前实现 `fe0c05a9` 已提供 `OccurredUtc`、UTC/本地双显示、缺失时间/事件的未知语义和有限时间线窗口；本批 `b6170ec9` 只补乱序事件按 UTC、同刻按序号稳定排序及跨任务过滤行为测试，没有生产代码变更。Worker `TaskCoordinatorFailureTests + TaskEventBroadcasterTests 12/12`、Playnite `R15TaskTimelineTests + TaskEventUiBatcherTests + R15TaskStageTests + R15TaskCancellationTests + R06TaskProgressBehaviorTests 15/15` 通过；隔离 Release XAML `24/24`、solution `0 error/2` 条既有 warning、Playnite `net462`。既有 Task 截图没有打开时间线卡，不作时间线呈现证据；真实 Worker 重启历史、Playnite/UIA/读屏/IME、DPI/跨屏、presented frame、ETW 和宿主性能仍待验。详见 [R15-03 任务详情时间线定向复核](evidence/R15-03-TARGETED-RECHECK-20260924.md)。下一项推进 R15-04。

> 2026-09-24 R15-04 重复通知归并定向复核：当前实现 `a67d371e` 复用 `TaskNotificationDeduper`、`SessionNotificationAccumulator`、通知等级策略和 Task Center 历史；本批 `6510ccf6` 只补“错误码和错误消息均缺失、仅未知详情变化”的去重行为，没有生产代码变更。Playnite 核心通知/会话行为 `10/10`，相邻通知反馈/时间线/UI 批处理/R13 夹具合计 `31/31`，Core 通知策略/摘要 `7/7`；隔离 Release XAML `24/24`、solution `0 error/2` 条既有 warning、Playnite `net462`。源码契约不冒充真实 Toast；真实 Playnite 通知时序、UIA/读屏/IME、DPI/跨屏、presented frame、ETW 和宿主性能仍待验。详见 [R15-04 重复通知归并定向复核](evidence/R15-04-TARGETED-RECHECK-20260924.md)。下一项推进 R15-05。

| 本轮 ID | 当前受控复核状态 | 当前自动/视觉事实 |
| --- | --- | --- |
| R13-07 | 已满足，待环境验证 | Core/Worker/Playnite 定向 `40/40`、`32/32`、`19/19`；Maintenance `FindingsGrid` `8` 行、`5/4` 可读行，无 R13 专属 RenderHarness 问题 |
| R13-08 | 已满足，待环境验证 | 认证/空间/远端不存在/校验差异/限流和未知错误负例已由 Core/Worker/Playnite 定向覆盖；受控错误摘要可读 |
| R14-01 | 已满足，待环境验证 | `R14ClassificationEvidenceTests 1/1`；高置信结构化依据和无依据待判断项均在合成样本中出现 |
| R14-02 | 已满足，待环境验证 | `R14ClassificationSelectionTests 4/4` 与 Worker `MediaSyncServiceTests 20/20`；稳定 ID、目标覆盖和排除语义通过 |
| R14-03 | 已满足，待环境验证 | `MediaSyncServiceTests 20/20` 覆盖逐项失败、成功项不重复、取消/恢复；有限列表和失败结果入口有受控样本 |
| R14-04 | 已满足，待环境验证 | Worker `MediaSyncServiceTests 20/20` 覆盖撤销后人工变化冲突、归档副本恢复和不覆盖后来决定 |
| R14-05 | 已满足，待环境验证 | Worker/Playnite 重复识别行为通过；clean-tree Media 重复页为 `2` 组、组内 `4` 项，有限视口 `330 DIP` |
| R14-06 | 已满足，待环境验证 | `GamePickerViewModelTests 22/22`、`R14ClassificationSelectionTests 4/4`、`GamePickerKeyboardBehaviorTests 6/6`；同名目标按 Playnite ID 区分，过滤隐藏选择保持；现有截图仅为 offscreen 视觉证据，真实宿主仍待验 |
| R14-07 | 已满足，待环境验证 | 选定 Playnite 套件 `46/46`；详情导航/列表锚点/缩略图尺寸/取消与失败回退通过，既有截图只证明入口和媒体列表，真实详情呈现、文件编解码与宿主仍待验 |
| R14-08 | 已满足，待环境验证 | Worker `MediaSyncServiceTests 20/20`、Playnite `R14SourceRulePreviewTests 1/1`；clean-tree 试运行样本 `4` 项、命中 `2`/排除 `2`，受控只读/限额/取消边界通过 |

| ID | 任务 | 状态 | 实现 commit | 自动验证 | 视觉/交互 | 宿主/性能 | 证据与下一步 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| R00-01 | 按压合成算法 | 已满足 | a95e900 / e216e9b / 38d5b7b2 / d752424e | `88b8eb57`、SDK 8.0.423：Release solution `0/0`，XAML `24/24`；UiDiagnosticsExporterTests `11/11`、UiFinesseFoundationTests `9/9`；source validation 通过 | 当前 R09 生产资源矩阵和算法正负例通过；仍是受控 WPF/逻辑 DIP，不代替真实按压和屏幕帧 | 不适用真实宿主性能签收；真实 Playnite 与物理呈现未验 | [2026-10-15 当前 main 复核与精简审计](evidence/R00-R01-CURRENT-MAIN-RECHECK-20261015/README.md)；受控条件/风险与外部未验边界见复核包 |
| R00-02 | 组合缩放复用 | 已满足 | a95e900 / 5fbfc869 / d752424e | `88b8eb57`、SDK 8.0.423：UiFinesseFoundationTests `9/9`；`ScaleTransformIsReusedInAStableCompositeTree` 连续 1000 次复用后节点/深度稳定，平移旋转和实例隔离通过 | 受控 STA WPF；真实宿主呈现仍未验 | 变换树辅助不适用库性能签收；不宣称物理呈现；可变共享隔离留给 R08-08 | [2026-10-15 当前 main 复核](evidence/R00-R01-CURRENT-MAIN-RECHECK-20261015/README.md)；ScaleTransform 1000 次复用与树稳定由同一 UiFinesseFoundationTests `9/9` 覆盖；外部未验边界见复核包 |
| R00-03 | 动画结束基值 | 已满足 | 4414f05 / cda168c / 0e468873 / 5fbfc869 | `88b8eb57`、SDK 8.0.423：Release solution `0/0`；UiFinesseFoundationTests `9/9`、R09HighContrastBehaviorTests `3/3`；XAML `24/24` | 受控 WPF 动画与资源状态通过；未把离屏帧写成真实屏幕帧 | 真实 Playnite 生命周期、OS 输入、ETW/物理呈现未验 | [2026-10-15 当前 main 复核与精简审计](evidence/R00-R01-CURRENT-MAIN-RECHECK-20261015/README.md)；受控条件/风险与外部未验边界见复核包 |
| R00-04 | 搜索基准真实性 | 已满足 | df884b0 / 5fbfc869 / dc7f97cf；`a78c46b5` 当前 main 复核 | `LargeLibraryPerformanceTests 5/5`；预热5次、30个不同查询均改变可见 ID 集合；`SearchText -> FilteredCount` p50/p95/max=`45/60/60 ms`；超时负例通过 | 合成 2000 项样本，记录原始30项；不等同真实 Playnite 输入、IME 或屏幕帧 | 受控 STA WPF 输入到可见容器20样本派生 p50/p95/max=`46.214/75.782/76.334 ms`，不作为 presented frame；R18-01 保留独立大库/连续输入数据 | [历史基准](evidence/R00-04-SEARCH-BENCHMARK-20260916.md)；[当前 main 热态响应复核](evidence/Q25-01-CURRENT-MAIN-HOT-RESPONSE-RECHECK-20261014/README.md) |
| R00-05 | 上下文禁用透明度 | 已满足 | aebcefc / 5fbfc869 / d752424e | `88b8eb57`、SDK 8.0.423：WpfUiResourceDictionaryTests `139 passed / 39 skipped / 0 failed`；XAML `24/24`，skip 不计通过 | 实测生产派生样式资源与几何；未验真实宿主像素 | 样式无独立库性能影响；命令、绑定和安全语义未改 | [2026-10-15 当前 main 复核与精简审计](evidence/R00-R01-CURRENT-MAIN-RECHECK-20261015/README.md)；受控条件/风险与外部未验边界见复核包 |
| R00-06 | 媒体四行门禁 | 已满足 | 7d57575 / db5d483 / e8fe1aab / 5fbfc869 / dc7f97cf / af915a2d（当前身份复核） | 当前 `R00-06` 几何/锚点定向用例 `13/13`；R18-04 专测历史 `1/1` | 短窗页级回退、有效视口、锚点和 footer 可达由受控 STA WPF 覆盖；这些用例不替代真实宿主滚动诊断 | synthetic DTO/offscreen logical DIP；TRX 在 xUnit `13/13`、exit `0` 后有 2 条 TextServices 清理 `InvalidComObjectException`，根因未知；真实 Playnite 仍未验。当前整体审计风险见 R01-06 | [历史门禁](evidence/R00-06-MEDIA-FOUR-ROWS-20260916.md)、[当前行距/锚点复核](evidence/USER-REPORTED-COMPACT-ROW-GAP-20260930/README.md)、[当前 R01-06 审计](evidence/R00-R01-CURRENT-MAIN-RECHECK-20261015/README.md) |
| R00-07 | 审计排除项收窄 | 已满足，审计风险另记 | 42f9dca / 5fbfc869 / dc7f97cf / 22c55647（当前身份复核） | `22c55647` 的 `UiAuditSourceTests 6/6`；toolbarprobe 正常表单、同祖先超宽、不可达工具栏三场景通过 | 正常表单按输入流理由排除；超宽溢出和不可达负例仍保留为 action-toolbar 告警 | 当前 audit `168` snapshots、`116` warnings、`0 HIGH / 13 MEDIUM / 0 Fidelity / 0 failed routes`；13 条工具栏自然换行风险仍保留 | [R00-07 规则和负例](evidence/R00-07-TOOLBAR-EXCLUSION-20260916.md)、[当前身份复核](evidence/USER-REPORTED-TRAINER-COPY-CURRENT-MAIN-RECHECK-20260930/README.md)、[当前 R01-06 审计](evidence/R00-R01-CURRENT-MAIN-RECHECK-20261015/README.md) |
| R00-08 | 搜索框 Enter/IME | 受控行为已满足；真实宿主/OS 输入待验 | `55ed3a17f14087859319b350297132c7fcb3f79c`（当前身份复核） | `88b8eb57`、SDK 8.0.423：GamePickerKeyboardBehaviorTests `7/7`；旧选择/IME 合成输入/候选确认/取消回焦行为已在当前源复核 | 无可见结果保留旧选择且不确认；合成 composition/方向键与有效候选 Enter；Escape/清空命令焦点返回；8 个真实入口正反向 Tab 可达；帮助弹层当前命令列表与禁用命令空列表 | 隔离 STA Window 和合成输入；没有真实 Playnite、OS IME 候选/实体键盘、UIA/读屏或最终呈现证据；物理 DPI/跨屏、ETW/宿主性能未验 | [2026-10-15 当前 main 复核与精简审计](evidence/R00-R01-CURRENT-MAIN-RECHECK-20261015/README.md)；受控条件/风险与外部未验边界见复核包 |
| R01-01 | 测试源码根绑定 | 已满足 | abb5589 / a5219c09 / 38d5b7b2 | 当前身份 `RepositoryIdentityTests 2/2`、`BuildIdentityTests 3/3`；隔离 Release solution XAML `24/24`、`0 warning/0 error` | 构建门禁阻断错根/旧程序集；旧 C 盘仓库未读取 | 只验证隔离 testhost/构建身份，未启动 Playnite，不宣称宿主输入/DPI/帧率 | [R01-01 证据](evidence/R01-01-REPOSITORY-IDENTITY-20260916.md)、[2026-09-28 R00/R01 复核](evidence/R00-R01-CURRENT-RECHECK-20260928.md) |
| R01-02 | 数字单元格裁切 | 已满足 | acfe1ea / 5fbfc869 | `NumericCellReadabilityTests 2/2`；Light/Dark 探针 `expected=4 realized=4 horizontalFit=4 verticalFit=4 allReadable=True`；窄列负例 `HorizontalFit=False / VerticalFit=True / must-fail=passed` | `1,024 / 99,999`、长负数、容量与 TiB 在 offscreen logical DIP 中无横/竖裁切；旧 `110 DIP` 曾有末位裁切 | 真实宿主/屏幕帧未验；诊断 helper 不宣称宿主性能 | [R01-02 证据](evidence/R01-02-NUMERIC-CELL-READABILITY-20260916.md)；当前可控条件已满足 |
| R01-03 | 每项证据直达 | 已满足，风险另记 | 3875f88 / 2eb4c46 / 591deae / f6a3209 / 9d5146d / 3a1dadd8 / dc7f97cf | `88b8eb57`、SDK 8.0.423 RenderHarness Release `0/0`；UiAuditSourceTests `6/6`；E01–E20 索引校验 `20/20` | 20 项均直达报告、源码位置、身份、样本与未验边界；当前风险 `0 HIGH / 13 MEDIUM`，未写成清零 | 受控 WPF 报告与索引；不启动 Playnite或执行任何业务写入，不宣称真实宿主呈现 | [2026-10-15 当前 main 复核与精简审计](evidence/R00-R01-CURRENT-MAIN-RECHECK-20261015/README.md)；受控条件/风险与外部未验边界见复核包 |
| R01-04 | 动效行为替代字符串 | 已满足 | 74abb10 / 0e468873 / 5fbfc869 | `UiFinesseFoundationTests 9/9`；四个 Light/Dark 动效探针通过；最终 build `0 errors/2` 既有 warning | STA/Dispatcher 覆盖重入当前值、完成清钟、reduced-motion 与 Loaded/Unloaded 清理 | offscreen logical DIP；真实 Playnite、Windows 偏好通知、UIA、物理 DPI、呈现帧、ETW/宿主帧率未验 | [R01-04 证据](evidence/R01-04-MOTION-BEHAVIOR-20260916.md)、[时序复核](evidence/R00-R01-MOTION-PROBE-RECHECK-20260923.md) |
| R01-05 | 负例注册表 | 已满足 | 5e6d64a / 5fbfc869 / dc7f97cf（当前身份复核） | `ae8f3be9` 当前 post-commit `UiNegativeFixtureRegistryTests 1/1`；N01–N05 五类检测器逐项捕获预期负例 | detector 的 expected-failure 不混入生产入口；N02/N03/N05 使用受控 WPF Window，N04 用隔离目录检查真实报告文件 | 合成颜色/DTO/WPF/offscreen logical DIP；真实宿主、OS 输入/IME/DPI/呈现帧/ETW 未验 | [当前 R00/R01 复核包](evidence/R00-R01-Q24-07-CURRENT-MAIN-RECHECK-20261014/README.md) |
| R01-06 | 宿主证据保全 | 已满足，当前摘要含风险 | 3929ed7 / 3a1dadd8 / dc7f97cf（当前 audit 源码） | `88b8eb57`、SDK 8.0.423 RenderHarness Release `0/0`；audit `168` snapshots、`116` warnings、`0 HIGH / 13 MEDIUM / 0 Fidelity / 0 failed routes` | clone 后可查阅源码身份、摘要、manifest、layout、route/fidelity 和证据索引；归档 JSON 已去除机器专属 RepositoryRoot 与 ZipPath | 截图只代表受控 WPF 离屏 logical DIP；不宣称真实 Playnite/UIA、用户主题、物理 DPI、OS 输入/IME、presented frame、ETW 或宿主性能 | [2026-10-15 当前 main 复核与精简审计](evidence/R00-R01-CURRENT-MAIN-RECHECK-20261015/README.md)；受控条件/风险与外部未验边界见复核包 |
| R01-07 | 基线失效规则 | 已满足，包身份待宿主 | e1324fe / baseline 当前条目 `dc7f97cf` / freshness 自测；latest check `b1e7b652` | R01-07 freshness 自测在 `b1e7b652` 为 `14/14 FRESH / 0 stale`；`packageIdentity=not-provided`；没有把 fresh 结果解释成 package 或 host 已验证 | 代码源与 package identity 分开报告；freshness 只说明关联源码路径未变，不代表重建、重装或宿主复测；latest pass 对比区间包含 2,848 个源码、文档与证据路径，不能简化为 docs-only | Playnite/package host、UIA、物理呈现、ETW/宿主性能未验 | [当前 freshness 运行结果](evidence/R00-R01-CURRENT-MAIN-RECHECK-20261016/README.md)；[基线规则复核](evidence/R01-07-EVIDENCE-FRESHNESS-20260916.md) |
| R01-08 | 跳过测试说明 | 已满足 | d31fe13 / 当前复验 72be494d | 当前隔离输出：Core 83/0/0；Playnite R01 gated 7/0/0（含 NamedPipe 6/6）；Worker 重启 gated 1/0/0；legacy 清单 0/0/57；XAML 24/24；不把非本项全量 WPF 失败改写为 skip 或通过 | 57 条为已撤销 UI 基线；6 条 IPC NamedPipe gated 与 1 条 Worker 重启 gated 在当前环境实际通过；当前全量 Playnite 中的非 R01 WPF/布局失败保留为独立边界 | Playnite 计划中的 63 = 57 legacy + 6 IPC；本机包/宿主未启动，IPC/Worker 仅隔离能力验证，真实宿主、物理 DPI、OS 输入/IME、presented frame、ETW 与宿主性能保留未验 | [R01-08 证据](evidence/R01-08-SKIP-INVENTORY-20260916.md)；R02-01～R02-05 已有证据，R02-06 外部阻塞；下一项 R07-05 详情断点稳定 |
| R02-01 | 动作优先级 | 已满足 | 89c9cc3 / cdfd2788（行为补测） | 最终隔离 Release XAML `24/24`、`0 errors/2` 条既有 `CS8602` warning；`R02ActionPriorityTests 3/3`；结构/危险样式两项原测试保留，新增生产 MediaCenter 视图的 ComboBox 来回切换行为测试 | Overview 工具栏、当前游戏卡片、Inbox 批量栏各一个主动作；Apply/Restore 在待归类↔已忽略切换中通过双向绑定与实际 DataTrigger 始终互斥，活动动作恰为一个 Primary；恢复、来源删除、策略模板删除使用各自危险样式 | 合成模式状态/隔离 STA WPF/逻辑 DIP；不执行归类/恢复/删除；真实 Playnite、用户屏幕/读屏、物理 DPI、IME、presented frame、ETW/宿主性能未验；当前独立审计 `7 HIGH/4 MEDIUM` 风险保留 | [R02-01 当前行为复核](evidence/R02-01-ACTION-PRIORITY-20260916.md)；下一项 R02-02 忙碌宽度稳定 |
| R02-02 | 忙碌宽度稳定 | 已满足 | `eaee1d20`（探针复核） | 当前隔离 Release：XAML `24/24`、`0 errors/2` 条既有 `CS8602` warning、Playnite `net462`；`R02BusyStateTests 4/4`；相邻媒体批量忙态 `1/1`；RenderHarness 双主题探针 `OK` | 复用共享 `IsBusy` 与 `BusyOperationCoordinator`；真实 STA WPF 状态验证 `180→180 DIP`、文本/焦点稳定、延迟指示器及快完成负例；fake 失败/取消/重复进入复位或拒绝。探针经隔离 Window 验证未加载时隐藏、150ms 后显示、`20×2.67 DIP` 且动画未暂停 | 受控 fake、STA WPF、逻辑 DIP；离屏 PNG 未证明 spinner 对比度/动画像素或真实呈现。未验真实 Playnite/Worker 时序、物理 DPI、OS 输入、presented frame、ETW 或宿主性能 | [R02-02 当前复核](evidence/R02-02-BUSY-WIDTH-20260916.md)；下一项 R02-03 禁用原因可达 |
| R02-03 | 禁用原因可达 | 已满足 | 627f864；当前复验 main `da6388e8` | 历史 Release `0/0`、R02 定向 `4/4`；2026-10-12 当前 main STA 行为/生产紧凑布局 `5/5`，Release `0/0`、XAML `24/24`；WPF 静态审查 `0 errors/29 warnings/177 info` | Restore、Media Inbox、Cloud Transfer、Remote Restore 继续有状态说明；当前 STA 用例核对 Focus 后的 `Keyboard.FocusedElement`，并读取 Automation peer 的 Name/HelpText；Save 宽/紧凑提示位置变化覆盖；危险命令门禁未改。通用双主题 render 不作为禁用态截图证明 | 受控 fake/STA WPF/offscreen logical DIP；真实 Playnite、屏幕阅读器、物理 DPI、OS 输入/IME、package-host、presented frame、ETW/宿主性能仍未验 | [原始 R02-03 证据](evidence/R02-03-ACTION-AVAILABILITY-20260916.md)；[当前 main 复验](evidence/R02-03-CURRENT-RECHECK-20261012/README.md)；下一项 R02-04 图文光学居中 |
| R02-04 | 图文光学居中 | 已满足 | 历史 `d296ce0`；当前 main `a2144083` 复验 | 当前 Release `0/0`、XAML `24/24`；R02 定向 `7/7`，16/20 DIP gap各8、中心差0；Light/Dark RenderHarness 372 PNG `OK`；源码校验通过 | 同一共享文字模板的中文四字/英文双词基线差≤0.5 DIP；16/20 DIP 图标与数字间距8 DIP、中心差≤1.5 DIP；当前 Save 1040×700 两主题截图人工对照 | 合成 STA WPF、Geometry、离屏 logical DIP；WPF静态审查36 XAML/0 errors/29 warnings/177 info；CI run932失败但完整日志未取得；未验真实 Playnite/物理 DPI/屏幕阅读器/OS 输入/IME/presented frame/ETW/宿主性能 | [原始 R02-04 证据](evidence/R02-04-OPTICAL-ALIGNMENT-20260917.md)；[当前 main 复验](evidence/R02-04-CURRENT-RECHECK-20261012/README.md)；下一项 R02-06 菜单宿主边界 |
| R02-05 | 命中区与间距 | 已满足 | 历史 `f03b4dd`；当前 main `90099a09` 复验 | 当前 Release `0/0`、XAML `24/24`；STA R02 定向 `2/2`；Light/Dark RenderHarness 372 PNG `OK`；源码校验通过 | 当前实际视觉树HitTest：34×34 DIP复制/删除独立命中、6 DIP空隙负例、82 DIP窄WrapPanel同排/换行/无重叠；Media 1040×700浅/深截图复核 | 合成按钮/图标、隔离STA Window、offscreen logical DIP；CI run `38247748056` 失败且完整日志未取得；未验真实Playnite/物理DPI/OS鼠标/屏幕阅读器/OS输入/IME/presented frame/ETW/宿主性能；未写真实数据 | [原始 R02-05 证据](evidence/R02-05-HIT-AREA-SPACING-20260917.md)；[当前 main 复验](evidence/R02-05-CURRENT-RECHECK-20261012/README.md)；下一项 R02-06 菜单宿主边界 |
| R02-06 | 菜单状态完整 | 外部阻塞 | 8e48ac8 | Release 构建 `0/0`；Core `83/83`、Worker `311/311`、Playnite `539/596` 通过、`57` 跳过、`0` 失败；R02 定向 `2/2`；XAML `24/24`；源码校验通过 | 当前插件可控的 `GameMenuItem` 契约已测：空选择 0 项；合成多选按固定顺序生成 6 项、统一 `GameSaveCenter` 分组且 Action 非空；没有执行 Action。WPF 菜单状态、键盘、翻转和快捷键列不在插件 visual tree | 当前无 WPF `ContextMenu/MenuItem`，Playnite 宿主拥有禁用/勾选/危险/子菜单/快捷键/定位与关闭；需隔离宿主会话补验，不能以本地静态测试替代；未写真实数据 | [R02-06 证据](evidence/R02-06-MENU-HOST-BOUNDARY-20260917.md)；[当前 main 源码边界复核](evidence/R02-06-07-CURRENT-MAIN-REVIEW-20261012/README.md)；下一项 R02-07 异步菜单上下文 |
| R02-07 | 异步菜单上下文 | 已满足 | 4d4bb93 | Release 构建 `0/0`；Core `83/83`、Worker `311/311`、Playnite `541` 通过/`57` 跳过/`0` 失败；R02 定向 `4/4`；XAML `24/24`；源码校验通过 | 菜单生成保存目标 Guid 快照；五个目标相关异步 Action 在 Worker/Upsert 前按当前数据库重新解析并保持原顺序；刷新替换对象命中新引用；删除目标返回缺失 ID、明确提示并中止，不改用当前选中行 | 只验证插件可控的合成对象和隔离程序集；Playnite 宿主菜单打开后的刷新/删除时序、宿主输入和菜单视觉仍未验；没有执行 Action 或写真实数据 | [R02-07 证据](evidence/R02-07-MENU-ASYNC-CONTEXT-20260917.md)；[当前 main 源码边界复核](evidence/R02-06-07-CURRENT-MAIN-REVIEW-20261012/README.md)；下一项 R02-08 动作文案动词化 |
| R02-08 | 动作文案动词化 | 已满足 | `43ea843` / 当前复核 `4d166f01` | 当前 main clean Release：SDK `8.0.423`、XAML `24/24`、0 warning/error、全量 `1560 passed / 57 skipped / 0 failed`；`R02ActionCopyTests 4/4`；source validation 通过 | 当前 XAML/DTO/Worker 保留重新加载、重新校验、远端校验、重试上传、隔离下载、快照恢复的动词边界；RenderHarness 当前 main Light/Dark `372` 图 `render-qa OK`，已查 Save 1040×700 两主题与 CloudQueue 页面上下文图 | 测试读取当前生产 XAML/DTO/消息，但未触发云端或恢复命令；RenderHarness 是 offscreen logical DPI `1.00`，未启动真实 Playnite、未验宿主字体、OS 输入/UIA、物理 DPI、presented frame、ETW 或宿主性能；CloudQueue 图未显示验证/重试按钮，按钮精确词由当前行为测试确认 | [原实现证据](evidence/R02-08-ACTION-COPY-20260917.md)；[当前 main 复核](evidence/R02-08-CURRENT-MAIN-RECHECK-20261012/README.md)；R03-01 当前 main 复核已归档；下一项 R04-01 当前 main behavior recheck |
| R03-01 | 真实落字证据 | 已满足 | `855e727` / 当前复核 `8e71feb4` | main `8e71feb4` 当前身份重建 RenderHarness `0 warning / 0 error`、`TypographyDiagnosticsTests 11/11`；Light/Dark `finesseprobe` exit0。最近完整 clean Release `4d166f0`（source/tests 与当前无改动）：XAML24/24、0/0、1560 passed/57 skipped/0 failed | 复用现有字体链；WPF `TextFormatter.GetIndexedGlyphRuns()` 逐项区分候选/最终 Typeface。中文、英文、数字、𠮷、组合重音均 `GlyphRunCaptured`；𠮷 候选 unresolved，但最终 `MingLiU-ExtB` 含该码点且非 `.notdef`。截图和报告绑定当前 main；`WorkingTreeClean=False` 为探针运行时本证据目录已在工作树的实际记录 | 受控 STA WPF、合成样本、offscreen logical DIP；未验真实 Playnite frame、用户/宿主字体替换、物理 DPI/跨屏、OS 输入/IME、读屏、ETW 或宿主性能。旧 `f89b160` 测试程序集先触发 checkout 身份保护；当前身份重建后全类11/11，不把旧程序集失败算成功 | [R03-01 原始证据](evidence/R03-01-GLYPHRUN-20260917.md)；[当前 main 复核包](evidence/R03-01-CURRENT-MAIN-RECHECK-20261012/README.md)；下一项 R04-01 当前 main recheck |
| R03-02 | 阅读层级校准 | 已满足 | e889b04 | Release XAML `24/24`；构建 `0/0`；Core `83/83`、Worker `311/311`、Playnite `547/604` 通过、`57` 跳过、`0` 失败；Typography 定向 `10/10`；源码校验通过 | 生产 Views/Settings 的 12.5/10.5/9.5 字面字号由 `GscBodyFontSize=14` 或 `GscCaptionFontSize=12` 接管；Light/Dark 1040×700 `render-qa OK`，人工检查 Overview、Media、Save、Maintenance 代表图无字号挤压 | 受控 WPF RenderHarness、offscreen logical DIP；未验真实 Playnite presented frame、宿主字体替换、物理 DPI/跨屏、OS 输入/IME、读屏、ETW 或宿主性能；未写真实存档/媒体/云端 | [R03-02 证据](evidence/R03-02-TYPOGRAPHY-HIERARCHY-20260917.md)；下一项 R03-03 双语基线 |
| R03-03 | 双语基线 | 已满足 | 466c2f5 | Release XAML `24/24`；构建 `0/0`；Core `83/83`、Worker `311/311`、Playnite `548` 通过/`57` 跳过/`0` 失败；Typography 定向 `11/11`；源码校验通过 | 复用现有字体链与生产排版；四组 Save Center/日期/容量/中文标点混排在 Light/Dark `finesseprobe` 均为实际 runs/glyphs、baseline spread `0`、`stable=True`；生产 1040×700 双主题 `render-qa OK`，人工抽查 Overview/Media 未见挤压 | 受控 STA WPF `TextFormatter`、合成文本、offscreen logical DIP；未验真实 Playnite presented frame、宿主字体替换、物理 DPI/跨屏、OS 输入/IME、读屏、ETW 或宿主性能 | [R03-03 证据](evidence/R03-03-BILINGUAL-BASELINE-20260917.md)；下一项 R03-04 数字列对齐 |
| R03-04 | 数字列对齐 | 已满足 | 9fa68ef | Release XAML `24/24`；构建 `0/0`；Core `83/83`、Worker `311/311`、Playnite `558/615` 通过、`57` 跳过、`0` 失败；R03-04 定向 `10/10`；源码校验通过 | 真实 STA WPF 生产资源中 `9/10/99/100` 右边界均稳定在 `119.5～120.5 DIP`；排队/负数未知统一为 `—`，有效百分比和超界钳制通过；Light/Dark 1040×700 生产页面 `render-qa OK` 并人工抽查 Task/Save/Media | 受控隔离 WPF/offscreen logical DIP；未验真实 Playnite presented frame、宿主字体/物理 DPI/跨屏、OS 输入/IME、读屏、ETW 或宿主性能；保留 picker/滚动条/命令绑定/取消错误/恢复保护/net462 | [R03-04 证据](evidence/R03-04-NUMERIC-ALIGNMENT-20260917.md)；下一项 R03-05 长路径分层 |
| R03-05 | 长路径分层 | 已满足 | 2b8612f | Release XAML `24/24`；构建 `0/0`；Core `83/83`、Worker `311/311`、Playnite `561/618` 通过、`57` 跳过、`0` 失败；R03-05 定向 `3/3`；源码校验通过 | 共享中间省略预览保留盘符/文件名尾部；Save、Media、Trainer 详情均提供原始路径只读选择框和完整复制命令；超过 260 字符在 `340 DIP` 有限宿主内不撑宽，选择结果与原始值完全一致；Light/Dark RenderHarness `render-qa OK`，最终报告 `WorkingTreeClean=True` | 合成长路径、真实 STA WPF 生产资源、offscreen logical DIP；未验真实 Playnite presented frame、OS 剪贴板/输入/IME、屏幕阅读器、宿主字体、物理 DPI/跨屏、ETW 或宿主性能；Maintenance 摘要路径仍是 Tooltip/只读摘要，未冒充独立复制详情 | [R03-05 证据](evidence/R03-05-LONG-PATH-20260917.md)；下一项 R03-06 双语长度压力 |
| R03-06 | 双语长度压力 | 已满足 | 52527b6 | Release XAML `24/24`；构建 `0/0`；Core `83/83`、Playnite `563/620` 通过、`57` 跳过、`0` 失败；R03 定向 `2/2`；源码校验通过 | 共享文字动作模板在窄槽改为完整换行、不做歧义省略；标题继续省略但保留完整 Tooltip。`BilingualLengthStress` 合成 profile 同时覆盖英文长句、中文长游戏名；clean commit 的 Light/Dark `overviewedges` 在 820/1040/1600 DIP 均 `surfaces=6/6`、英文长句可见、动作 2 个可测、无横向溢出；统一 `render-qa` `WorkingTreeClean=True`、297 PNG、`render-qa OK` | 实际 STA WPF 生产模板与 offscreen logical DIP；未验真实 Playnite presented frame、宿主字体替换、屏幕阅读器、OS 输入/IME、物理 DPI/跨屏、ETW 或宿主性能；Worker 本阶段未改，311/311 为同分支最近完整基线；未写真实存档/媒体/云端 | [R03-06 证据](evidence/R03-06-BILINGUAL-LENGTH-20260917.md)；下一项 R03-07 文案标点统一 |
| R03-07 | 文案标点统一 | 已满足 | 5a07eda | Release XAML `24/24`；构建 `0/0`；Core `83/83`、Worker `311/311`、Playnite `566/623` 通过、`57` 跳过、`0` 失败；R03 定向 `3/3`；源码校验通过 | 任务筛选四个生产标签统一全角冒号；失败详情使用“错误码：…；…”展示分隔，空错误码不生成孤立标签；整库失败聚合同步使用中文分隔。错误码、错误正文和路径原值保持不变，单位/英文缩写按现有术语表保留。clean commit 双主题 `render-qa OK`、`WorkingTreeClean=True`、357 PNG；Task 1040/1100/1366 DIP 均通过页面与滚动探针，人工抽查 Light/Dark Task | 实际 STA WPF、合成错误正文/路径、offscreen logical DIP；未验真实 Playnite presented frame、宿主字体替换、物理 DPI/跨屏、OS 输入/IME、屏幕阅读器、ETW 或宿主性能；路径复制具体行为沿用 R03-05 证据；未写真实存档/媒体/云端 | [R03-07 证据](evidence/R03-07-COPY-SAFE-PUNCTUATION-20260917.md)；下一项 R03-08 用户文本缩放 |
| R03-08 | 用户文本缩放 | 已满足 | 981b600 | Release XAML `24/24`；构建 `0/0`；Core `83/83`、Worker `311/311`、Playnite `568/625` 通过、`57` 跳过、`0` 失败；R03 定向 `2/2`；源码校验通过 | 生产 `TextBox`/文本按钮去掉硬高，表头去掉 `ColumnHeaderHeight` 并保留 `MinHeight` + Wrap/None；实际 `24/20 DIP` DynamicResource 覆盖的 STA WPF 输入、按钮、表头不裁切；双主题 RenderHarness `render-qa OK`、357 PNG，人工抽查 Task/Media/Save | 受控隔离 WPF、合成文本、offscreen logical DIP；保留 picker/滚动条/命令绑定/取消错误/恢复保护/有限列表/net462；未验真实 Playnite 宿主字体替换、用户文本设置、物理 DPI/跨屏、OS 输入/IME、读屏、presented frame、ETW 或宿主性能 | [R03-08 证据](evidence/R03-08-TEXT-SCALE-20260917.md)；下一项 R04-01 组合输入状态 |
| R04-01 | 组合输入状态 | 已满足（插件可控范围）；系统 IME/宿主待验 | `40c7f9a` / 当前复核 `32c0e1e2` | Playnite net472 Release 定向测试 `37/37`：GamePickerKeyboardBehavior `6/6`、GamePickerViewModel `22/22`、DebouncedRefresh `4/4`、R05OptionVirtualization `5/5`；source/tests 自完整源码身份 `4d166f0` 未变 | 当前生产 Shell WPF routed-event 行为：组合 start/update 期间 Enter 不提交旧游戏；commit 后 Enter 选择可见候选并关闭；无结果、ImeProcessed/方向键、Escape/清空焦点正负例；复用现有本地筛选、短 debounce、ItemsView 与 2k 虚拟化 | 合成 TextCompositionEventArgs 与隔离 STA Window，不是 Windows TSF/真实候选窗；未断言真实候选翻页期间的文本/FilteredCount；未验真实 Playnite 输入链、物理键盘英文端到端反馈、屏幕阅读器、物理 DPI/跨屏、presented frame、ETW/宿主性能 | [R04-01原始证据](evidence/R04-01-IME-COMPOSITION-20260917.md)；[当前 main 复核包](evidence/R04-01-CURRENT-MAIN-RECHECK-20261012/README.md)；下一项 R04-02 错误摘要导航
| R04-02 | 错误摘要导航 | 已满足（插件可控导航/验证范围）；宿主朗读待验 | `6524f94` / 当前复核 `32c0e1e2` | 当前 main 的 Playnite net472 定向复核 `5/5`：SettingsValidationSource `1`、SettingsPathValidation `3`、SettingsValidationNavigationBehavior `1`；禁用巡检字段非法旧值负例通过 | 复用 VerifySettings、字段 Validation 模板、页头摘要、SettingsSectionTabs 与 SettingsScroller。隔离 STA Window 实际激活错误摘要 Hyperlink 后选择自动化 Tab、让目标字段可见并取得键盘焦点、滚动偏移增加；Automation Name/HelpText 含原因。路径普通文件拒绝、缺失子目录不自动创建均有负例 | 合成设置/fake Worker/隔离目录；结构检查仅作补充，不代替交互证据。未执行 Playnite Settings API 保存或 Worker 应用过程；未验真实 Playnite 嵌入、屏幕阅读器朗读、物理 DPI/跨屏、presented frame、ETW 或宿主性能 | [R04-02 原始证据](evidence/R04-02-VALIDATION-NAVIGATION-20260917.md)；[当前 main 复核包](evidence/R04-02-CURRENT-MAIN-RECHECK-20261012/README.md)；下一项 R04-03 未保存离开保护 |
| R04-03 | 未保存离开保护 | 已满足（插件可控关闭分支；原生宿主仍待验） | 原始实现 `58e1734` / 当前补证基于 `e952c414` 工作树 | 原始实现 clean Release/XAML `24/24`；当前补充定向 `12/12`（SettingsDraftLifecycle `1`、PortableSettings `10`、SettingsValidationSource `1`），实际 STA WPF `Window.Closing` 分支、标签切换草稿保留、分离/重挂焦点恢复、CancelEdit 回滚 | 复用 Playnite `ISettings` 编辑克隆与既有取消逻辑；返回“否”时真实窗口保持打开、草稿为 `60`、原输入框回焦；返回“是”时才关闭并回滚至 `30`。内部 test seam 仅代替确认结果；生产默认仍调用原生 `MessageBox.Show`。当前 `validate-source.py` 通过；R00/R01 freshness 14 条记录中该路径 0 命中、0 条重跑 | 测试程序集 ProductVersion 指向基线 `e952c414`，当时工作树包含本阶段源码/测试修改；合成设置/fake Worker、隔离目录、STA WPF、offscreen logical DIP。未操作真实原生 MessageBox、Playnite 保存/Worker 应用、UIA/Narrator、物理 DPI/跨屏、presented frame、ETW 或宿主性能；未写真实存档/媒体/云端 | [原始 R04-03 证据](evidence/R04-03-DRAFT-LIFECYCLE-20260917.md)；[当前 main 补充证据](evidence/R04-03-CURRENT-MAIN-RECHECK-20261012/README.md)；下一项 R05-01 当前 main 证据复核 |
| R04-04 | 粘贴标准化 | 已满足 | d7e92f1 | clean Release XAML `24/24`、构建 `0/0`、源码校验通过；R04-04 定向 `8/8`，覆盖路径/模式/端口类型规则、真实 WPF Pasting/Undo、多行拒绝负例和生产 XAML 接入 | 共享 attached behavior 按输入类型去除外层空白/成对引号及单值尾随 shell 换行；内部多行直接拒绝，字段不变、剪贴板不改写；标准化写入保留原生 Ctrl+Z，Tooltip/Automation HelpText 说明前后结果。设置 6 个本地路径 + 云端目标、Media 目录 + 文件模式已接入；当前没有真实可编辑端口或独立 Exclude 字段。clean RenderHarness 双主题、297 PNG、`WorkingTreeClean=True`、`render-qa OK`，人工抽查 Settings/Media 1040×700 | 合成文本/设置/fake Worker、隔离目录、STA WPF、offscreen logical DIP；未验真实 Playnite 剪贴板/输入链、IME、读屏、物理 DPI/跨屏、presented frame、ETW 或宿主性能；没有端口/排除字段可做宿主验证，未写真实存档/媒体/云端 | [R04-04 证据](evidence/R04-04-PASTE-NORMALIZATION-20260917.md)；下一项 R04-05 数字输入边界 |
| R04-05 | 数字输入边界 | 已满足 | 8d56eb5 | clean Release XAML `24/24`、构建 `0/0`、源码校验通过；R04-05 相关定向 `20/20`，含实际 STA WPF 滚轮合法步进、边界不改值/不吞事件、溢出与非法文本负例，以及 Save/Trainer 生产接入 | 共享 `IntegerRangeValidationRule` 处理空值、非整数、上下界和 `Int32` 溢出；`GscNumericTextBox` 的滚轮只按同一绑定规则步进，非法当前值或越界候选不静默 clamp，现有页面滚动继续生效。间隔 `1–1440`、启动延迟 `0–300`、保留周期 `0–2147483647`；未新增不存在的重试/端口/容量字段。clean RenderHarness 双主题、297 PNG、`WorkingTreeClean=True`、`render-qa OK`，人工抽查 Save `1040×700` | 合成设置/fake 服务、隔离目录、STA WPF、offscreen logical DIP；未验真实 Playnite 系统键盘/剪贴板/滚轮、IME、读屏、物理 DPI/跨屏、presented frame、ETW 或宿主性能；未写真实存档/媒体/云端 | [R04-05 证据](evidence/R04-05-NUMERIC-INPUT-20260917.md)；下一项 R04-06 清空与撤销 |
| R04-06 | 清空与撤销 | 已满足 | 0d3f71a | clean Release XAML `24/24`、构建 `0/0`、源码校验通过；R04-06 STA WPF 行为 `3/3`，覆盖搜索清空焦点、Esc 关闭/回焦和标准 SelectAll/Undo/Redo 选择区 | 复用现有各页搜索清空处理和 WPF 原生编辑命令；补齐 Dashboard 空状态“清除搜索”入口的 Tag/Click 接线并标记事件已处理，保留 `GamePicker.ClearSearchCommand`。游戏选框 Esc 不触发业务危险命令；clean RenderHarness 双主题、297 PNG、`WorkingTreeClean=True`、`render-qa OK`，人工抽查 Task/Shell `1040×700` | 合成搜索数据、隔离 STA WPF、offscreen logical DIP；未验真实 Playnite 系统键盘/剪贴板/IME/读屏、物理 DPI/跨屏、presented frame、ETW 或宿主性能；未写真实存档/媒体/云端 | [R04-06 证据](evidence/R04-06-CLEAR-UNDO-20260917.md)；下一项 R04-07 异步校验竞态 |
| R04-07 | 异步校验竞态 | 已满足 | b1d5b68f / df6f8083 | clean Release XAML `24/24`、构建 `0/0`、源码校验通过；R04-07 定向 `7/7`，含慢 A 晚于 B 返回不覆盖、卸载取消不回写、隔离路径服务负例和源码接线 | 复用 `GameSaveCenterSettings.VerifySettings` 的完整同步规则；编辑期用不可变路径快照 + `SettingsPathValidationService` 后台校验，`LatestAsyncValidationCoordinator` 以递增版本和取消令牌抑制旧成功/失败回调；设置页 DataContext/提交/回滚/卸载均失效旧结果，失败转为可修复提示，保存提示显示校验进行中；clean RenderHarness 双主题、297 PNG、`WorkingTreeClean=True`、`render-qa OK`，人工抽查设置 normal/dirty/invalid | 合成设置、隔离目录、TaskCompletionSource、offscreen logical DIP；未验真实 Playnite 宿主输入/关闭时序、慢网络共享、系统 IME/剪贴板、读屏、物理 DPI/跨屏、presented frame、ETW、宿主线程/帧率性能；不可中断的单次 OS 文件系统调用只能取消结果应用，未写真实存档/媒体/云端 | [R04-07 证据](evidence/R04-07-ASYNC-VALIDATION-20260917.md)；下一项 R04-08 保存反馈闭环 |
| R04-08 | 保存反馈闭环 | 已满足 | c735905a | clean Release XAML `24/24`、构建 `0/0`、源码校验与 diff check 通过；R04-08 相关选择集 `20/20` | 复用 Playnite `ISettings.EndEdit`、现有 Worker live apply 和设置页稳定提示面；增加保存中、已写入 Playnite/应用 Worker、Worker 应用失败、保存失败四类闭环，失败不伪报已保存，本地写入失败保留编辑克隆，保存/应用期间重复 `EndEdit` 被抑制。当前生产字段没有仅重启生效项，因此不虚构“需重启”态。clean RenderHarness 绑定完整 SHA、双主题、297 PNG、`WorkingTreeClean=True`、`render-qa OK`，Settings normal/dirty/invalid `1040×700` 已抽查 | 合成设置/fake 服务、隔离目录、状态机与 STA WPF/offscreen logical DIP；未在真实 Playnite 宿主注入本地写入或 Worker 失败，未验真实保存/取消/关闭时序、宿主输入、IME/剪贴板、读屏、物理 DPI/跨屏、presented frame、ETW 或宿主性能；未写真实存档/媒体/云端 | [R04-08 证据](evidence/R04-08-SAVE-FEEDBACK-20260917.md)；下一项 R05-01 弹层焦点范围 |
| R05-01 | 弹层焦点范围 | 已满足（Shell/ComboBox 与 Dashboard 受控取消关闭插件行为）；真实宿主和完整 Dashboard XAML 加载待验 | 当前 main `1c39e909856faa736b32224ee196ef0bac6ca460` | 精确提交 Release 定向 R05Focus + R05Option + R21 合计 `11/11`，测试程序集/插件 SHA 与 MVID 已记录 | 生产 Shell 选框在 640/320 DIP 下焦点进入、Tab/Shift+Tab 循环、Esc 回焦；ComboBox 项目不可 Tab 聚焦且 Tab 回到表单；R21 2000 项焦点可见；Dashboard fixture 验证焦点进入、Tab 环绕、即时/动画关闭期间焦点留层内、取消后回焦触发器；XML 解析断言核对生产 XAML Cancel/Confirm Click 接线 | 合成 STA WPF、逻辑 DIP；fixture 不加载完整 Dashboard XAML、不运行 Playnite。真实宿主/物理键盘/IME/UIA/Narrator、物理 DPI/跨屏、presented frame、ETW、宿主性能未验；VSTest exit0 但有三条 TextServicesHost `InvalidComObjectException` cleanup 消息，原因未知 | [R05-01 原始实现证据](evidence/R05-01-FOCUS-BOUNDARY-20260917.md)；[当前 main 复核包](evidence/R05-01-CURRENT-MAIN-RECHECK-20261013/README.md)；R05-02 本次一并复核；其 CI run #941 后续失败且日志不可读；R05-03 随后完成复核 |
| R05-02 | 选项虚拟化焦点 | 已满足（当前 main 插件可控复核） | 原实现 `7a4ede84`；当前 main `1c39e909856faa736b32224ee196ef0bac6ca460` | 当前 Release `R05OptionVirtualizationBehaviorTests 5/5`，与 R05-01/R21 回归共 `11/11` | 2000 项生产 Shell 列表验证方向键、PageDown、Home/End，活动项滚入可见 DIP 视口且导航不关闭弹层；过滤隐藏保留有效选择，已有恢复命令可恢复；删除选中项回退首个有效项；保留 Auto 滚动与 Recycling 虚拟化 | 合成 STA WPF/逻辑 DIP；真实 Playnite/OS 键盘与 IME、读屏、物理 DPI/跨屏、presented frame、ETW 或宿主性能未验；本机测试不是 CI 门禁 | [R05-02 证据](evidence/R05-02-OPTION-VIRTUALIZATION-20260917.md)；[当前 main 复核包](evidence/R05-02-CURRENT-MAIN-RECHECK-20261013/README.md)；CI #941 后续失败且日志不可读 |
| R05-03 | 弹层边缘适配 | 部分满足（边界/滚动/选定项通过；无闪屏未验；隔离宿主预检阻塞） | 原实现 `cbfacd20`；当前 main `1c39e909856faa736b32224ee196ef0bac6ca460` | 当前 main Release 定向 `R05PopupBoundaryBehaviorTests` + R05-02 虚拟化回归 `7/7`；2026-10-13 host bootstrap 在安装前按保护规则退出；TRX/隔离启动原始日志见复核包 | 生产 Picker 在 720×360 DIP 短窗不越 overlay，列表 Auto 滚动且选定项可见；共享 ComboBox Popup 在当前桌面工作区右下翻转后起止点在工作区内，最大高度/滚动条/选定项通过。历史 420×220 约 `92×92 DIP` 负例说明无有效列表视口 | 当前套件断言打开后几何，未采集翻转过程/实际呈现帧，故“翻转无闪屏”未验；当前桌面不代表多屏或物理 DPI；隔离 Playnite bootstrap 未完成、未安装插件；真实 Playnite/IME/UIA/Narrator/ETW/宿主性能未验 | [R05-03 原始实现证据](evidence/R05-03-POPUP-EDGE-20260917.md)；[当前 main 复核包](evidence/R05-03-CURRENT-MAIN-RECHECK-20261013/README.md)；下一项 Round2 Q23-06 目录编辑器视觉复核 |
| R05-04 | 多选摘要 | 已满足 | 原实现 `52900815`；当前行为夹具基线 `903525be`，测试源 SHA 见复核包 | 当前本机 SDK `8.0.423` Release solution `0/0`；`R05MultiSelectionSummaryBehaviorTests 3/3`。只补测试夹具/负例，没有生产代码变更 | 生产 MediaCenterView/STA WPF DataGrid 验证 0/1/2 选择摘要；隐藏 ID 与当前窗口可操作数区分；“已忽略”模式清空后该模式 ID 与当前 DataGrid 选择归零、`待归类`模式的保留 ID 不变，模式值仍为“已忽略”。详细 TRX/程序集身份见当前复核包 | 合成 DTO 与隔离路径；未改视觉，未把历史 RenderHarness 截图冒充当前 main 视觉证据。真实 Playnite、OS 输入、IME、UIA/Narrator、物理 DPI/多屏、最终呈现帧、ETW/宿主性能仍未验。CI #941/#942/#943 失败且日志不可读，根因未知 | [原始实现证据](evidence/R05-04-MULTI-SELECTION-SUMMARY-20260917.md)；[当前 main 复核包](evidence/R05-04-CURRENT-MAIN-RECHECK-20261013/README.md)；下一项 R05-06 当前 main 开关保存行为复核 |
| R05-05 | 复选框三态 | 不适用 | 当前 main `fbf504d2a7aabcd322c3c735959a31c7e1679fc9`；无生产代码变更 | SDK `8.0.423` 官方脚本 Release build `0 warning / 0 error`、XAML `24/24`；`UiFinesseRound2ControlSourceTests 24/24`，脱敏 TRX SHA 见复核包 | 媒体批量仍由 Extended DataGrid + `SelectedItems` 表达；三态样例在开发校对页，共享 `IndeterminateMark` 仅为控件视觉；源测试本身仅核对资源/Trigger 字符串，不作为行为签收 | 当前产品无对应的批量三态集合控件，Space/UIA/分页三态无业务消费点；集合筛选/追加由 R05-04 行为证据覆盖。未启动真实 Playnite/OS 输入，未验物理 DPI/跨屏/presented frame/ETW/宿主性能。#943 失败日志/API 403，根因未知 | [历史边界证据](evidence/R05-05-CHECKBOX-THREESTATE-20260917.md)；[当前 main 复核包](evidence/R05-05-CURRENT-MAIN-RECHECK-20261013/README.md)；下一项 R05-06 当前 main 开关保存行为复核 |
| R05-06 | 开关保存语义 | 已满足（当前 main 行为复核） | 初始实现 `cf9a250f`；当前生产代码基线 `0fcb44f851257504d867401dab92a74b3807899a`；仅补重复值通知负例 | SDK `8.0.423` 官方脚本 Release build `0 warning / 0 error`、XAML `24/24`；定向 `1/2/1/24`，总计 `28/28`；改动测试源 SHA、TRX 与程序集 SHA 见复核包 | 当前生产设置页/STA WPF 验证模型、ToggleSwitch、Track Brush、dirty 提示、Portable JSON 快照和 `CancelEdit` 回滚对齐；同值重复赋值不重复发属性变化通知；媒体来源/巡检依赖面板按值禁用/恢复；保存与 Worker apply 失败反馈区分。没有生产代码改动 | current-main 行为测试读取当前 XAML/资源，但不生成像素；既有双主题 RenderHarness 绑定 `cf9a250f`，不得写成当前截图。隔离合成设置/临时路径；真实 Playnite 写入/Worker 故障注入、原生对话框、宿主输入/IME/UIA、物理 DPI/跨屏、presented frame、ETW、宿主性能未验。#944 CI failure 日志/API 403，根因未知 | [初始实现/历史双主题证据](evidence/R05-06-TOGGLE-SAVE-20260917.md)；[当前 main 复核包](evidence/R05-06-CURRENT-MAIN-RECHECK-20261013/README.md)；下一项 R05-07 Tooltip 时序当前 main 复核 |
| R05-07 | Tooltip 时序 | 已满足（当前 main 行为通过；#968 SDK8/SDK10 CI 全门禁通过；#952 历史根因未知） | 当前 main 4572118f；生产 Tooltip 未改，仅修测试夹具 | SDK 8.0.423 Release 0/0、XAML 24/24；Tooltip 2/2 两轮；CI #952 于“编译与测试”exit1（12m07s） | 真实 STA 系统指针快速划过取消、WPF hover 到达后驻留开启、长路径 Wrap/Esc 与焦点行为均有本机证据 | #952 完整日志/artifact 内容不可读，runner 实际 SDK与根因未知，不盲改；真实 Playnite、OS UIA/Narrator、多屏物理 DPI、presented frame、ETW/宿主性能未验 | [当前 main 复核包](evidence/R05-07-CURRENT-MAIN-RECHECK-20261013/README.md)；日志可读后按真实失败复现；当前先复核 R00/R01 |
| R05-08 | 弹层资源热切换 | 已满足 | 599a8fd9 / bebde2fe | clean Release XAML `24/24`、编译 `0/0`；最终标准产物定向 R05-08 `1/1`、Tooltip `1/1`、Popup `2/2`、焦点 `3/3`、开关 `1/1`、选项虚拟化 `3/3`、源契约 `24/24` | 真实生产 Settings Popup 与 Tooltip 在 Light→Dark 切换后仍打开且背景随活动主题更新；父窗 `Hide()` 后视图不可见并关闭两类附属层；行为只在局部范围绑定并在卸载时解除显式 Tooltip 处理器，无静态主题订阅；保留现有命令、绑定、滚动条、恢复/错误语义与 net462 | clean RenderHarness 绑定 `bebde2fe`，Light/Dark、357 PNG、`WorkingTreeClean=True`、`render-qa OK`；Settings 1040×700 开面探针与 Light/Dark Popup/Tooltip 图已抽查；`DpiScale=1.00` 仅离屏 logical DIP。全量 WPF 并行夹具仍有环境性失败，未宣称真实 Playnite/宿主关闭、泄漏 profiler、物理 DPI/跨屏、presented frame、ETW 或宿主性能 | [R05-08 证据](evidence/R05-08-POPUP-LIFECYCLE-20260918.md)；下一项 R06-01 列宽用户记忆 |
| R06-01 | 列宽用户记忆 | 已满足（当前 main 行为复核） | 初始实现 `75a6e6d8`；当前复核 `afade3cd39eab31de97f204aa76e9dc590ba3083` | 初始 clean Release XAML `24/24`、行为 `5/5`；当前 main SDK `8.0.423` Release `0/0`、XAML `24/24`，`R06ColumnWidthPersistenceBehaviorTests 6/6`（零失败/跳过）；当前 source 和 DLL SHA、脱敏 TRX 见当前复核包 | 四个生产 DataGrid 按稳定视图/列键保存有效 Pixel 宽度；控制器重建恢复与视图区隔离；版本/未知键忽略、最小宽度、响应式布局保留、重置/持久化和窄窗横向滚动通过；第六条测试核对 Task 列键与生产列一致 | 当前复核仅为行为/构建证据，没有重跑 RenderHarness；双主题357 PNG与代表图仍绑定初始实现 `75a6e6d8`。合成设置和DataGrid，不等价真实 Playnite 进程重启/用户拖拽；物理DPI/跨屏、UIA/Narrator、presented frame、ETW、宿主性能仍未验。Maintenance 表格不纳入此功能 | [原实现/视觉证据](evidence/R06-01-COLUMN-WIDTH-PERSISTENCE-20260918.md)；[当前 main 复核包](evidence/R06-01-CURRENT-MAIN-RECHECK-20261013/README.md)；下一项 R06-02 |
| R06-02 | 排序提示与稳定性 | 已满足（当前 main 行为复核） | 实现 `c577afc5` / detached-view 修复 `f8a82469`；clean-main `3d83d38f`；当前复核 `90454586d702f6a2b413bfcc386ac599424f8bc1` | 当前 main SDK `8.0.423` Release `0/0`、XAML `24/24`；`R06SortingBehaviorTests 7/7`，0 fail/skip；当前 source/DLL SHA、MVID、脱敏 TRX 见复核包 | Save History 时间/数值、同值次键、未知末尾与乱序重加；Task 数值进度/未知阶段；Media 未知来源升降序末尾；真实 DataGrid 列头 Click 双向切换和箭头同步；失效视图点击负例；Save Candidates 生产 profile 接线已核对 | 历史 clean RenderHarness 双主题 `357` 图和代表截图绑定 `c577afc5`，本次没有冒充当前截图。合成 DTO/隔离 CollectionView/STA Window；未验真实 Playnite、UIA/读屏、物理 DPI/跨屏、presented frame、ETW/宿主性能；未接触真实数据 | [R06-02 原证据](evidence/R06-02-SORT-STABILITY-20260918.md)；[clean-main 复核](evidence/R06-SORTING-CLEAN-MAIN-RECHECK-20260928.md)；[当前 main 复核包](evidence/R06-02-CURRENT-MAIN-RECHECK-20261013/README.md)；下一项 R06-03 |
| R06-03 | 选中焦点区分 | 已满足（当前 main 行为复核） | 实现 `d83c7378`；当前 main parent `fdeeec8a` + 当前测试差异 | SDK `8.0.423` Release `0 warning / 0 errors`、XAML `24/24`；R06Selection `2/2`、R23 四表双主题选择/焦点/禁用几何 `1/1`、Task 双主题 hover/徽章/取消 UIA 行为 `1/1`、共享资源契约 `2/2`；脱敏 TRX 与程序集身份见当前复核包 | 复用共享 DataGridRow/ListBoxItem 状态；Task 生产页浅/深主题受控 MouseDevice hover 进出，失败行保留 error pill 且 `InputHitTest` 命中徽章，选中运行任务的取消按钮可通过 UIA Invoke 执行 fake 命令；四生产表状态几何不变 | 合成 DTO/fake 命令、隔离 STA WPF Window；hover 为受控 WPF MouseDevice 状态转换，不代表真实 OS 指针或 Playnite 宿主；本轮未生成当前截图，历史 RenderHarness 图仍绑定 `d83c7378`。真实宿主/UIA/Narrator、物理 DPI/跨屏、最终呈现、ETW/性能未验 | [历史实现证据](evidence/R06-03-SELECTION-FOCUS-20260918.md)；[当前 main 行为复核](evidence/R06-03-CURRENT-MAIN-RECHECK-20261013/README.md)；下一项 R06-04 复制单元格与整行 |
| R06-04 | 复制单元格与整行 | 已满足（当前 main 行为通过；#968 SDK8/SDK10 CI 全门禁通过；#952 历史根因未知） | 初始实现 afe4aa55；Q14校正2f546d06；Tooltip夹具修正4572118f | 本机SDK8.0.423 Release 0/0、XAML24/24；Q14 1/1、ProductionShellChromeSource22/22、R06Clipboard5/5、R22 2/2；CI #952 编译与测试exit1，根因未知 | 实际生产DataGrid PreviewKeyDown验证Ctrl+C行复制、Ctrl+Shift+C单元格复制及错误修饰键/解绑/无选择负例 | #952日志需登录，artifact 11774606363匿名内容不可读；本次Q14 CI状态、runner SDK未确认。本机fake clipboard/隔离路由不代表真实OS剪贴板/Playnite；真实UIA、IME、DPI/跨屏、呈现帧、ETW/性能未验 | [当前复核包](evidence/R06-04-CURRENT-MAIN-RECHECK-20261013/README.md)；CI根因未明期间继续独立R00/R01 |
| R06-05 | 列头说明 | 已满足（当前 main 行为与视觉复核；#968 Actions 全门禁通过） | `d60e472d` + R06 test source `9391298E` | 隔离 SDK `8.0.423` Release `0/0`、XAML `24/24`、当前输出程序集 `R06ColumnHeaderHelpTests 2/2`；当前 main RenderHarness 双主题 372 PNG、`render-qa OK`，WorkingTreeClean=False 的工作树身份已注明 | 生产列头行为实际验证 Tooltip/HelpText、string header、没有嵌套 Button、左/右 resize Thumb、长 header Wrap/None 与测量高度；列头 Click 两次造成升/降序且 collection 顺序改变。当前生产四页 8 张代表图检查；单位复用既有 1024 格式，没发现无信息值占位列 | 合成数据、隔离 STA WPF/offscreen logical DIP；无生产 UI变化。非真实 Playnite/OS pointer/UIA/键盘/拖拽/读屏/IME/物理 DPI/跨屏/presented frame/ETW/宿主性能 | [当前 main 复核包](evidence/R06-05-CURRENT-MAIN-RECHECK-20261013/README.md)；推送后等待 Actions 终态，再推进 R06-06 |
| R06-06 | 行内进度稳定 | 已满足（当前 main 行为复核；#968 SDK8/SDK10 CI 全门禁通过） | 实现 `72fd6d9a` / `fd9326d7`；当前复核源码及程序集身份 `73d0f6e6` | SDK `8.0.423` Release `0 warning / 0 error`、XAML `24/24`；隔离 Playnite `34/34`、Worker SQLite 查询 `9/9`；`R06TaskProgressBehaviorTests 4/4`。CI SDK 不由本机结果替代；本轮复核后的远端 run 尚待提交触发/终态 | 80 行合成任务的实际 DataGrid 进度行更新产生单个 `Replace`、没有 `Reset`；选择行 ID/索引、行数及逻辑滚动偏移不变；未知/排队进度占位、有效 0%、钳制、取消/成功状态边界通过；生产 `TaskCenterView` 取消状态 TextBlock 实际折叠→可见；Task 总数继续使用服务端 `TaskSummary`，Worker 合成分页查询覆盖摘要与页数据分离。绑定源码测试只作为补充，不单独证明行为 | 当前身份无新截图；历史 RenderHarness 绑定 `72fd6d9a`、Light/Dark 357 PNG，只作历史离屏 logical DIP 证据。当前复核使用合成 DTO、隔离 STA WPF 和 SQLite；混合非隔离试次 `22/23` 的一个失败停在 `FindScrollViewer(grid)` 为空，未计入逐类结果且未确定根因。真实 Playnite/真实取消竞争、UIA/读屏、物理 DPI/跨屏、最终呈现帧、ETW/宿主性能未验 | [历史实现证据](evidence/R06-06-TASK-PROGRESS-STABILITY-20260918.md)；[当前 main 复核包](evidence/R06-06-CURRENT-MAIN-RECHECK-20261013/README.md)；下一项复核已实现的 R06-07 空表保留结构 |
| R06-07 | 空表保留结构 | 自动行为已满足（当前 main；本提交 CI 待终态） | 实现 `5046bf8f`；当前 main `cd27c8c0aba8616331867a58af13376f44e958f0` | SDK `9.0.302` / VSTest `17.14.1`；测试项目及引用 Release build `0/0`；`R06EmptyStateBehaviorTests 2/2`、`WorkspaceStatePresenterBehaviorTests 8/8`、`MediaWorkspaceStateCacheTests 7/7`，共 `17/17`；当前源/DLL/TRX SHA 见复核包 | Save 表头 7/4 列保留，Loading/首次空/完成/Error 区分，失败不伪装无数据；重试/覆盖层键盘与点击行为及禁用负例；缓存上下文、旧值和取消语义 | 本轮没有改生产布局或截图；历史 RenderHarness 图绑定旧实现提交。当前行为为合成/隔离 WPF，不等价真实 Playnite/UIA/IME、物理 DPI/跨屏、presented frame、ETW/宿主性能 | [原实现证据](evidence/R06-07-EMPTY-TABLE-STRUCTURE-20260918.md)；[当前 main 复核包](evidence/R06-07-08-Q24-06-R09-CURRENT-MAIN-RECHECK-20261015/README.md)；下一项核对 R07-04 当前源差异 |
| R06-08 | 详情与行高预算 | 自动行为已满足（当前 main；本提交 CI 待终态） | 初始实现 `07376adb`；当前 main `cd27c8c0aba8616331867a58af13376f44e958f0` | SDK `9.0.302` / VSTest `17.14.1`；测试项目及引用 Release build `0/0`；`R06DetailsBudgetBehaviorTests 2/2`、`MediaInboxGeometryTests 3/3`、`TaskCenterViewResponsiveTests 8/8`、`DetailsDisclosureSourceTests 1/1`，共 `14/14`；当前源/DLL/TRX SHA 见复核包 | 实际 STA 窗口的详情随选择更新且旧项不残留；独立详情滚动、160 段诊断、行高≤60 DIP、至少四行完整可见；Media 不足四行的页面滚动回退；Save 按稳定 Playnite ID+Path 恢复选中 | 当前行为实测已更新到 HEAD；未生成当前 main 截图或呈现帧。沿用恢复生产基线；真实 Playnite、UIA/读屏、物理 DPI/跨屏、ETW/宿主性能仍待验 | [原实现证据](evidence/R06-08-DETAIL-ROW-BUDGET-20260918.md)；[当前 main 复核包](evidence/R06-07-08-Q24-06-R09-CURRENT-MAIN-RECHECK-20261015/README.md)；下一项核对 R07-04 当前源差异 |
| R07-01 | 滚动所有权 | 已满足 | 9c878b9c | `R07ScrollOwnershipBehaviorTests 2/2`；相邻 Task 响应式、Media 四行、详情 disclosure、R06 详情 `14/14`；Release XAML `24/24`、编译 `0 warning/0 error`、源码校验与 diff check 通过 | 复用现有页面/表格/详情 ScrollViewer 所有权；共享行为只在内层到方向边界时把滚轮转给最近可滚动外层，DataGrid 解析内部 `DG_ScrollViewer`，不改变 `CanContentScroll`、虚拟化或滚动条；移除未接线固定三行旧处理器 | clean RenderHarness `.tmp/r07-01-scroll-final/render-qa-report.txt` 绑定 `9c878b9c...`，`WorkingTreeClean=True`、Light/Dark、多尺寸和 resize 均 `render-qa OK`；1040×700 Save/Media/Maintenance/Task 至少 `4/4`，1366 Task `4/4`；已查看 Task/Media/Maintenance 图。实际轮滚边界仅由隔离 STA WPF 夹具证明，不等价真实宿主/物理设备 | [R07-01 证据](evidence/R07-01-SCROLL-OWNERSHIP-20260918.md)；下一项 R07-02 锚点删除回退，先盘点稳定 ID 与视口恢复负例 |
| R07-02 | 锚点删除回退 | 已满足 | 7acb61a5 / ef53a748 | `R07SelectionAnchorBehaviorTests 4/4`；最终相邻回归 `20/20`、0 skipped；Release XAML `24/24`、编译 `0 warning/0 error`；源码校验、diff check 通过 | Task、Media 主库/Inbox、Findings、进程映射、云端队列、Save 历史/候选均先按稳定键恢复；锚点消失按旧行位夹到邻近项；任务导航目标和云端一致性 pending key 保持既有优先级 | clean RenderHarness `.tmp/r07-02-anchor-final/render-qa-report.txt` 绑定 `ef53a748...`、`WorkingTreeClean=True`、双主题、多尺寸/滚动/resize、50/400/2000/4468 合成数据量 `render-qa OK`；1040×700 Save/Media/Maintenance/Task 为 `4/4`、`6/4`、`5/4`、`5/4`，1366 Task `4/4`；已查看 Task/Media/Maintenance 图。行为仍是隔离 STA WPF/offscreen logical DIP，不等价真实 Playnite/设备输入/物理 DPI/跨屏/presented frame/ETW/宿主性能 | [R07-02 证据](evidence/R07-02-SELECTION-ANCHOR-20260918.md)；下一项 R07-03 短窗底栏可达 |
| R07-03 | 短窗底栏可达 | 已满足 | d19e848b / 447ac07e | `shortwindowprobe OK`：Light/Dark、1040×700/560 共 16 个实际页面组合；Media/Save/Task/Maintenance 分别验证底部加载更多、保存策略、取消任务和云端加载更多的真实可见矩形；相关相邻回归 `20/20`；Release XAML `24/24`、编译 `0 warning/0 error`；源码校验、diff check 通过 | 复用固定 36 DIP `FooterSurface` 与主内容行，未改游戏选框、滚动条、命令/Binding；提示条、页面 ScrollViewer、DataGrid/详情滚动各保留原有所有权；Save 探针以真实按钮 `BringIntoView` 验证短窗可达，并保留首次最大滚动假设的负例修正 | clean `.tmp/r07-03-short-window/shortwindowprobe-report.txt` 绑定 `447ac07e...`、`WorkingTreeClean=True`、`DpiScale=1.00`；合成 stale banner/加载更多和隔离 STA WPF/offscreen logical DIP，图像仅作布局抽查，不等价真实 Playnite/物理 DPI/跨屏/presented frame/ETW/宿主性能 | [R07-03 证据](evidence/R07-03-SHORT-WINDOW-20260918.md)；下一项 R07-04 横向滚动端点，先核对现有横向滚动和端点负例 |
| R07-04 | 横向滚动端点 | 受控输入与端点已满足；真实设备/宿主待验；当前提交 CI 失败原因未知 | 原探针 `8f682fcb`；当前补强基于 main `62672c94` / commit `aa49a569` | SDK `9.0.302` / VSTest `17.14.1`；Playnite 测试项目/引用 Release build `0/0`；新输入行为与相邻滚动 `8/8`；RenderHarness `0/0`、双主题四页面端点 `8/8`；XAML `24/24` | Shift+wheel 水平路由；Ctrl 与 Ctrl+Shift 不被 Shift 转换处理；合成 `WM_MOUSEHWHEEL` 小增量/命中负例/两端夹断；真实生产 DataGrid 模板下末列、末单元格及滚动条 viewport 几何；左右回程无偏移；Media 虚拟化设置保持 | 合成 2,000 条 Media 数据、STA WPF、离屏 logical DIP；真实硬件、OS 消息分发、Playnite host、最终呈现或宿主性能仍待验。#1010 SDK8/SDK10 job 失败，但日志 403，根因未知，不得当作 CI 通过 | [原始端点证据](evidence/R07-04-HORIZONTAL-ENDPOINT-20260918.md)；[当前 main 输入与端点复核](evidence/R07-04-CURRENT-MAIN-RECHECK-20261015/README.md)；下一项 R07-08 当前 main resize 压力复核

## 2026-10-15 R07-04 当前 main 横向输入补强

- 重读表格条件后发现，原八组合 `horizontalprobe` 直接设置 ScrollViewer offset，并未覆盖 Ctrl/Shift 或触控板输入；旧证据曾将程序化左端诊断标签写成“Ctrl+Shift/触控板等效”。本轮将该标签更正为“水平左端程序化复位”，不把 offset 变更说成物理输入。
- 逐项复用生产 DataGrid 的共享模板与 `DG_ScrollViewer`，在共享 `GscRedesignWorkspaceDataGrid` 及 Dashboard 的显式 `GscDataGrid` 样式接入 `DataGridHorizontalWheelBehavior`。Shift 转换、Control 负例、合成 `WM_MOUSEHWHEEL` 处理、表格外负例和左右端点均使用实际 STA WPF 表格/滚动视口观察；没有改变滚动条模板、选框、命令、DTO、业务服务或 Media 虚拟化。
- 本机 SDK `9.0.302` / VSTest `17.14.1`：Playnite 测试项目及引用 Release build `0 warning / 0 error`；RenderHarness Release `0/0`；新行为 `1/1`、相邻滚动行为 `7/7`；XAML `24/24`、source validation 与 `git diff --check` 通过。非完整 solution build，SDK8/SDK10 Actions 仍单独追踪。
- 新行为测得 2,000 条合成 Media 仅实现 6 个行容器；偏移样本为 Shift `0→48→0`、合成高精度横向 delta `4.8 DIP`、端点 `0→680→0`；Control/Control+Shift/无修饰转换不移动水平 offset，表格外消息不处理。Light/Dark × Save/Task/Media/Maintenance 的真实生产模板端点探针 `8/8`，Media `Standard/Item/column=False` 保持。详细 TRX SHA、探针范围及限制见[R07-04 当前 main 复核包](evidence/R07-04-CURRENT-MAIN-RECHECK-20261015/README.md)。
- 当前是合成 `WM_MOUSEHWHEEL` payload 直达生产窗口 hook 方法、隔离 STA WPF 与 offscreen logical DIP；未注入操作系统输入，也未连接触控板/鼠标硬件或 Playnite。物理设备、OS 消息分发、真实 host、UIA/读屏、物理 DPI/跨屏、presented frame、ETW 与宿主性能仍待验。自动收尾另输出 WPF TextServices COM 异常；测试计数为通过，不能把该收尾日志作为本阶段失败根因。
| R07-05 | 详情断点稳定 | 当前 main 受控行为已复核；Actions #1039 双 SDK 全量 WPF 通过；真实宿主待验 | 生产实现 `0daca6f0`；main 当前复核 `aa49a569` | SDK `9.0.302` / VSTest `17.14.1`；Playnite 测试项目及引用 Release build `0 warning / 0 error`（net462/net472）；Coordinator `5/5`、详情断点 `1/1`、滚动所有权 `2/2`、Task 响应 `8/8`、R06 详情 `2/2`，共 `18/18`；XAML `24/24` | 生产 TaskCenter STA WPF 夹具检查 `1000→979→981→971→979→988 DIP`，选择对象、详情焦点与垂直 offset 保持，紧凑态详情可见；滞回带内无重复切换 | 合成 DTO/fake、隔离窗口、offscreen logical DIP；没有真实 Playnite 拖窗、物理 DPI/跨屏、设备输入、UIA/读屏、presented frame、ETW 或宿主性能签收；WPF 收尾 COM 异常根因未知 | [首次验收](evidence/R07-05-DETAIL-BREAKPOINT-20260918.md)；[2026-09-24复核](evidence/R07-05-CURRENT-MAIN-RECHECK-20260924.md)/[TRX](evidence/R07-05-CURRENT-MAIN-RECHECK-20260924.trx)；[当前 main 复核](evidence/R07-05-CURRENT-MAIN-RECHECK-20261015/README.md)/[TRX](evidence/R07-05-CURRENT-MAIN-RECHECK-20261015/tests/R07-05-current-main-sdk9302.trx)；Actions #1039/run `38872163664` 当前候选 WPF124类全通过；下一项 R07-07 当前 main 复核
| R07-06 | 状态横幅预算 | 当前 main 受控行为已复核；Actions #1039 双 SDK 全量 WPF 通过；真实宿主待验 | 实现 `3551de81`；main 当前复核 `85609395` | SDK `9.0.302` / VSTest `17.14.1`；Playnite 测试项目及引用 Release build `0 warning / 0 error`（net462/net472）；XAML `24/24`；状态横幅 `4/4` + 空态 `2/2` + Task 响应 `8/8` + R06 详情 `2/2` + R07 滚动 `2/2`，共 `18/18`，0 fail/skip，exit `0` | Task 失败保留旧记录、空列表失败 presenter、Save/Maintenance Stale 非零 viewport 与表头；重试按钮 AutomationPeer 实际 Invoke 到 fake 命令计数各 1 次；安全模式只可执行明确恢复动作 | 合成 DTO/fake、生产页面控件、隔离 STA WPF/DIP；`IInvokeProvider` 不是 Playnite UIA 客户端；未验主题截图、真实 Playnite、物理 DPI/跨屏、OS 输入、presented frame、ETW/宿主性能；收尾有 WPF COM 异常，根因未知 | [初次验收](evidence/R07-06-STATUS-BANNER-BUDGET-20260918.md)；[2026-09-24复核](evidence/R07-06-CURRENT-MAIN-RECHECK-20260924.md)/[TRX](evidence/R07-06-CURRENT-MAIN-RECHECK-20260924.trx)；[当前 main 复核](evidence/R07-06-CURRENT-MAIN-RECHECK-20261015/README.md)/[五份脱敏 TRX](evidence/R07-06-CURRENT-MAIN-RECHECK-20261015/tests/)；Actions #1039/run `38872163664` 当前候选 WPF124类全通过；下一项 R07-08 当前 main resize 压力复核
| R07-07 | 触控板小增量 | 当前 main 受控行为 `21/21`；Actions #1039 双 SDK 全量 WPF 通过；真实触控板/宿主待验 | 初始实现 `98e8b244`；当前 main 复核源 `2b423737` | 锁定 SDK `8.0.423` / VSTest `17.11.1`；Playnite 测试项目及引用 Release build `0 warning / 0 error`（net462/net472）；XAML `24/24`；六类测试共 `21/21`，0 fail/skip | 12×`-30` offset `0→36 DIP` 单调，每步 `+3`；反向 `33→30→27`；末端 `154/154`；20 个 routed wheel `LayoutUpdated=17`，末端 5 个无位移事件增量 `0`，可见行 `6–9`；嵌套 `-360` 外层 `32→80/432`、内层 `628→628`、布局 `1`、可见容器 `2` | 合成 routed wheel/生产 DataGrid 与 ScrollViewer/隔离 STA WPF/逻辑 DIP；不是实际触控板、OS 输入、Playnite host、物理 DPI/跨屏、presented frame、ETW 或宿主性能；边界、末端和布局预算已有实际行为断言 | [初始实现](evidence/R07-07-FINE-SCROLL-20260918.md)；[2026-09-24历史复核](evidence/R07-07-CURRENT-MAIN-RECHECK-20260924.md)/[TRX](evidence/R07-07-CURRENT-MAIN-RECHECK-20260924.trx)；[当前 main 复核](evidence/R07-07-CURRENT-MAIN-RECHECK-20261015/README.md)；Actions #1039/run `38872163664` 当前候选 WPF124类全通过；下一项 R07-08 当前 main resize 压力复核 |
| R07-08 | resize 压力序列 | 当前 main 受控行为 `9/9`；Actions #1039 双 SDK 全量 WPF 通过；真实宿主/拖动待验 | 测试实现 `5fcd6856`；原源码基线 `552b8168`；当前复核 `d190f7ea` | 锁定 SDK `8.0.423` / VSTest `17.11.1`；Playnite 测试项目/引用 Release build `0/0`；XAML `24/24`；R07Resize `1/1` + Task responsive `8/8`；测试项目 ProductVersion `0.6.73+d190f7ea` | 详情按钮 Click 打开实际 TaskCenter 详情；五阶段 `1366×900→960×700→960×560→1440×900→1366×900 DIP`；Picker/详情/搜索焦点连续可达，任务表有高度；恢复 PageHost 回到 `1038.667×759.333`、误差≤1 DIP；窄/短详情 MaxHeight `136`，宽窗恢复 `∞`；shell SizeChanged `11` | 隔离 STA WPF、合成任务、logical DIP 和 dispatcher idle；不代表真实拖动稳定帧、Playnite、物理 DPI/跨屏、UIA/读屏、presented frame/ETW/宿主性能。COM cleanup 异常在测试成功后输出，原因未知。d190f7ea 的 Actions run ID/终态尚未读到；#1012 对父提交上次 In progress；SDK8本机不是SDK10 CI。#960 旧失败根因仍未知 | [当前 main SDK8复核包](evidence/R07-08-CURRENT-MAIN-RECHECK-20261015/README.md)；[2026-10-14复核](evidence/R07-08-CURRENT-MAIN-RECHECK-20261014/README.md)；[历史CI日志记录](evidence/CI-R07-08-RESIZE-FAILURE-20261013/README.md)；Actions #1039/run `38872163664` 当前候选 WPF124类全通过；下一项 R10-02 当前 main 复核 |
| R08-01 | 中途反向连续 | 当前 main 按钮 hover/press 自动行为已补足并通过；用户失败待原始身份定位 | 生产动画实现沿用；当前源码基线 066b9a6；测试源 SHA 278CB3D2...E07DAFA | SDK 9.0.302 Release 0/0；R08 3/3、Q06 按钮状态 1/1、source validation、XAML 24/24、diff check 通过 | Translate 中途反向、侧栏最后输入终点与时钟释放；生产 GscWpfUiPrimaryButton hover/press 切换瞬间连续、朝新目标推进、最终目标与最后输入一致 | 真实共享模板、隔离 STA、合成鼠标/按压状态；不是物理输入/Playnite。用户 1/2 失败方法/断言/堆栈/DLL 尚未核实；SDK9 不替代 SDK8/CI SDK10；无截图/呈现帧/ETW | [2026-10-14 当前 main 复核](evidence/R08-01-CURRENT-MAIN-RECHECK-20261014/README.md)；[外机 R09 断言复核](evidence/CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md)；下一项 R08-02 热关闭动画 |
| R08-02 | 热关闭动画 | 当前 main 受控行为已满足；SDK8/SDK10 CI 与真实 Windows 设置通知待确认 | 生产逻辑沿用 `0243c108`；当前 main `31d64e42`，仅加实例级测试注入入口与行为夹具 | SDK `9.0.302` 隔离 Release build `0/0`；应用热关闭 `1/1`、系统策略回调 `1/1`、生产侧栏 reduced-motion `2/2`、回调接线/卸载门禁 `1/1`；源码校验、XAML `24/24`、diff check 通过 | 活动 SettingsShell 入场在应用开关或受控系统策略关闭时清理两条时钟并回到 Opacity/Y=`1/0`；重新开启等待原周期不重播。实际生产侧栏活动过渡关闭后抵达稳定收起态，无活动时钟/迟到写入 | 系统策略只由实例级 provider 模拟；调用真实 OnSystemParametersChanged 回调路径，不触发静态事件或修改 Windows 偏好。源门禁检查 Dashboard/Settings 订阅与卸载。WPF STA/隔离窗口；TextServices COM shutdown 异常根因未知。新一轮 SDK8/SDK10 Actions 尚未在公开页核实；真实 Playnite、系统通知、物理 DPI/最终呈现/ETW/宿主性能未验 | [2026-10-14 当前 main 复核包](evidence/R08-02-CURRENT-MAIN-RECHECK-20261014/README.md)；旧复核保留为对应轮次历史事实 |
| R08-03 | 离屏与隐藏停机 | 当前 main 受控行为已满足；SDK8/SDK10 CI 与真实宿主性能待确认 | 生产 `4a18fe0c`；当前 main `31d64e42` 复核 | R08-02 同一隔离 Release build（`0/0`）；当前 main hash-identified test assembly 单类串行 `1/1`，0 fail/skip、exit `0`；`validate-source.py`、XAML `24/24`、diff check 通过 | 生产共享 ProgressBar 模板与 storyboard：可见时实测移动 `>0.5 DIP`；隐藏 Tab 和最小化各等待 `360ms`、X 保持到小数后三位且 `IsIndeterminate` 仍为 true；恢复后动画重新移动；未改生产代码 | 合成 Tab/ProgressBar、隔离 STA Window、逻辑 DIP；提交后新的全量重建尝试因 MSBuild project-reference target 无诊断失败，未冒称通过；行为测试复用同源码哈希的已编译程序集。真实 Playnite、物理最小化输入、呈现帧、UIA/读屏、ETW/宿主性能未验 | [R08-03 当前 main 复核](evidence/R08-03-CURRENT-MAIN-RECHECK-20261014/README.md)；[旧复核](evidence/R08-03-CURRENT-MAIN-RECHECK-20260924.md) 与[初始实现证据](evidence/R08-03-OFFSCREEN-HIDDEN-20260918.md)；R08-04 本机受控条件已复核，下一项 R08-05 |
| R08-04 | 业务完成节奏 | 本机受控条件已满足；CI/宿主待验 | 生产 `3bf90a00/43141399`；测试补充于 `37bd3c97` 工作树 | 本机 SDK `9.0.302` 隔离 Release solution `0 warning / 0 error`；R02 忙态 `5/5` + R08-04 反馈 `4/4`，共 `9/9`、0 fail/skip；Source validation 通过 | 未完成的成功请求保持忙态直至 action 放行，完成回调恰好一次；失败错误码/详情覆盖阶段文本，成功/失败/取消状态与详情精确匹配；按钮尺寸/内容/焦点稳定，慢 spinner 显示、快任务 `150ms` 后仍折叠；AutomationPeer Name/HelpText 与终态一致 | 合成 DTO/fake、隔离 STA WPF Window、offscreen logical DIP；未验真实 Playnite/Worker 长请求、Narrator/外部 UIA、自动通知宿主时序、物理 DPI、呈现帧、ETW 或宿主性能 | [R08-04 当前 main 复核包](evidence/R08-04-CURRENT-MAIN-RECHECK-20261014/README.md)；CI SDK8/SDK10.0.401 终态待核；下一项 R08-05 |
| R08-05 | 页面切换轻量化 | 当前 main 受控条件已满足；SDK8/SDK10 CI 与真实 Playnite 待验 | 页面缓存/测量审计沿用 `ccd71c27`；焦点行为沿用 `8e3cc914`；当前源码 `552b8168`，本次无生产代码改动 | SDK `9.0.302` 隔离 Release solution `0 warning / 0 error`；R08PageSwitch `4/4` + ShellChrome `22/22` + Foundation `9/9` + R02 Busy `5/5` + R08 feedback `4/4`，共 `44/44`；source validation、XAML `24/24`、diff check 通过 | 真实生产 shell/page 的 STA 夹具：Task→Media/返回 Measure/Arrange 各 `1/1`；DataGrid offset `10→10`，选中项/DataContext/96 项集合同引用；同页重入 `0/0`；筛选与 Media/Save/Maintenance 页签 `2/3/5` 保留、刷新命令 `0` 次；七次导航后标题、选中项与键盘焦点一致；PageHost 无 Blur/整页入场动画 | 合成 `TaskStatusDto` 与隔离状态对象、真实生产 WPF 页面、STA Window/offscreen logical DIP；未验真实 Playnite/Worker、物理 DPI/跨屏、presented frame、UIA/读屏、ETW 或宿主性能；Demo 原始目录不可用，沿用恢复生产基线。ShellChrome `22/22` 后有 TextServices COM 清理异常输出但 exit 0，根因未知 | [2026-10-14 当前 main 复核](evidence/R08-05-CURRENT-MAIN-RECHECK-20261014/README.md)与五份脱敏 TRX；CI SDK8/SDK10.0.401 终态待查；R07-08 当前 main resize 复核已补；下一项 R10-02 当前 main 稳定 ID 行为复核 |
| R08-06 | 数字变化动效 | 已满足（本机行为 5/5；#968 SDK8/SDK10 CI 全门禁通过） | 原实现 `acbfe7e0 / e264acff`；当前基线 `83fbee97`，修正源 hash/MVID 见当前复核包 | 隔离 SDK `8.0.423` Release solution `0 warning / 0 error`；R08Numeric `5/5`、`validate-source.py`、`git diff --check` 通过；Playnite `net462`、Tests `net472`；未改 XAML | 六个概览数字使用实际 `ScaleTransform` 动画并按 `420ms` 节流；99→100 后实际时钟、连发仅一次、`NormalizeAll()` 归一及固定 `96 DIP` 相邻位置；系统关闭动效与应用 reduced-motion 两个负例即时更新且无动画；默认路径跟随 `GscMotion.IsEnabled(true)`；任务进度/技术文本仍排除 | 合成数据、真实生产 Overview、隔离 STA WPF/offscreen logical DIP；当前无新截图。Demo 原目录缺失，沿用恢复生产基线；未验真实 Playnite/Worker、物理 DPI/跨屏、presented frame、UIA/读屏、ETW 或宿主性能 | [原始 R08-06 证据](evidence/R08-06-NUMERIC-CHANGE-20260918.md)；[当前 main / CI 门控复核](evidence/R08-06-CURRENT-MAIN-RECHECK-20261013/README.md)；下一项 R08-07 对话框遮罩同步 |
| R08-07 | 对话框遮罩同步 | 已满足 | afa845a6 | 当前身份隔离 Release：XAML `24/24`、构建 `0 warning / 0 error`；R08 相关类隔离运行 `16/16`；Core `83/83`；Worker `310/311`（1 skip）；源校验、diff check 通过 | 合成 fake 请求与隔离 STA WPF 对话框：Opening/Open/Closing/Closed 状态、重复完成/关闭期重入、遮罩退场后折叠、卡片透明度与位移归一化、焦点 scope/tab cycle 契约均通过 | 合成数据、隔离 STA/offscreen logical DIP；单类 testhost `16/16`，合并 R08 testhost 的 `6` 项失败记录为 WPF Dispatcher/视觉资源污染边界；未验真实 Playnite、物理 DPI/跨屏、键盘/IME、UIA/读屏、presented frame、ETW 或宿主性能；C: worktree 直接 WPF 构建受 `_wpftmp.csproj` Access denied 阻塞，D: 隔离源目录用于构建核验；Demo 原始目录不可用，沿用恢复生产基线 | [R08-07 证据](evidence/R08-07-DIALOG-OVERLAY-20260918.md)；下一项 R08-08 变换所有权，先核对 `GscMotion` 共用可变/冻结 `Freezable` 与其他变换实例的所有权边界 |
| R08-08 | 变换所有权 | 已满足 | 194a16fe | 当前提交干净隔离 Release/XAML `24/24`、解决方案 `0 warning / 0 error`；Foundation `9/9`、R08 相关串行回归 `12/12`；源码校验、diff check 通过 | 两个控件共享未冻结直接/组合变换时，helper 首次使用分别克隆并登记所有权；单控件 1000 次复用保持节点/深度稳定，旋转/平移几何保留；不宣称真实宿主呈现 | 合成 Border、STA WPF、隔离逻辑 DIP；未验真实 Playnite、物理 DPI/跨屏、presented frame、UIA/读屏、ETW 或宿主性能 | [R08-08 变换所有权证据](evidence/R08-08-TRANSFORM-OWNERSHIP-20260919.md)；下一项 R09-01 主题转换闪白 |
| R09-01 | 主题转换闪白 | 已满足 | fe048952 | 当前提交定向 STA WPF 主题切换 `1/1`；干净隔离 Release/XAML `24/24`、解决方案 `0 warning / 0 error`；源码校验、diff check 通过 | Light→Dark 在下一 Render 优先级更新 Popup surface、Path 图标、缩略图占位和文本资源；局部字典切换不留旧颜色；不宣称真实 presented frame 零闪 | host 字典 Magenta 哨兵保持原实例；主题资源只写插件局部字典；真实 Playnite/系统主题/物理屏幕与宿主性能未验 | [R09-01 主题切换证据](evidence/R09-01-THEME-SWITCH-20260919.md)；下一项 R09-02 图标语义统一 |
| R09-02 | 图标语义统一 | 已满足 | c7c7ae0d | 当前提交精确隔离 Release/XAML `24/24`、解决方案 `0 warning / 0 error`；R09-02 新夹具 `2/2`，相邻图标/动作回归 `3/3`；源码校验、diff check 通过 | 共享 Geometry 覆盖备份/恢复/上传/校验/归类/忽略；动作图标统一 `16x16`；实际禁用 Button 仍保留 IconData、可见性和 16x16 布局尺寸；不宣称真实宿主呈现 | 合成 STA WPF 控件、隔离资源字典、逻辑 DIP；未验真实 Playnite/物理 DPI/跨屏/presented frame/UIA/读屏/IME/ETW/宿主性能；Demo 原始目录不可用，沿用恢复生产基线；package-host `not-provided` | [R09-02 图标语义证据](evidence/R09-02-ICON-SEMANTICS-20260919.md)；下一项 R09-03 一像素描边 |
| R09-03 | 一像素描边 | 已满足 | 083b7a22 | 当前提交精确 Release/XAML `24/24`、solution `0 warning / 0 error`；R09-03 行为夹具 `2/2`，图标与共享资源相邻回归合计 `6/6`；源码校验、diff check 通过 | 实际 STA WPF 生产 TabItem/RadioButton 选中 Chrome 保持 1 DIP 边框、`SnapsToDevicePixels=True`、`UseLayoutRounding=True`；1 DIP 分隔线/圆角边框在 `100/125/150/175/200%` 明确缩放模拟下带宽随 scale 受控，圆角连接不方化；未改游戏选框、滚动条、命令/绑定和安全语义 | 五档均为离屏 `RenderTargetBitmap` 的 96-DPI 显式缩放模拟，不等价真实物理 DPI；未验真实 Playnite/跨屏/presented frame/UIA/读屏/IME/ETW/宿主性能；Demo 原始目录不可用，沿用恢复生产基线；package-host `not-provided` | [R09-03 一像素描边证据](evidence/R09-03-PIXEL-STROKE-20260919.md)；下一项 R09-04 阴影层次预算 |
| R09-04 | 阴影层次预算 | 已满足 | 3ad61099 | 当前提交精确 Release/XAML `24/24`、solution `0 warning / 0 error`；R09-04 行为夹具 `3/3`，R09-02/R09-03/共享资源相邻回归合计 `9/9`；源码校验、diff check 通过 | 运行时已有六个有限阴影角色层级；浅/深主题的冻结 DropShadowEffect 参数和层级通过；`glassEnabled=false` 返回真实 null、透明洗色回退；三个实际带阴影卡片的 ScrollViewer `ExtentHeight` 不因 Effect 改变；未重建阴影体系 | 验证为合成 palette、真实 WPF 资源与 ScrollViewer；未切换真实系统 High Contrast（归 R09-06），未验真实 Playnite/呈现帧/物理 DPI/跨屏/UIA/读屏/IME/ETW/宿主性能；Demo 原始目录不可用，沿用恢复生产基线；package-host `not-provided` | [R09-04 阴影层次预算证据](evidence/R09-04-SHADOW-BUDGET-20260919.md)；下一项 R09-05 焦点轮廓合成 |
| R09-05 | 焦点轮廓合成 | 当前 main SDK8 完整构建后定向行为 2/2；外机版本身份待核 | 生产实现沿用；错误态断言修正于 f13b9e05；当前代码基线 965fe1b2（src/tests 与 552b8168 相同） | 当前 main `6b946540`（仅文档提交，源码父项 `88b8eb57`）隔离 SDK8.0.423 完整 Release `0/0`；R09FocusOutlineBehaviorTests `2/2`，新旧 `2 DIP/1 DIP` 外机报告仍待实际 DLL/PDB 身份确认 | Primary Button focus overlay 保持圆角；共享 focus visual 圆角13、厚度2、非命中；错误 TextBox 的 Chrome 1 DIP + ValidationStroke 2 DIP；修复输入后错误描边隐藏 | 隔离 STA 与合成验证值；未验真实 Playnite/OS 输入/物理 DPI/读屏/呈现帧。外机第151行及 2→1 值吻合旧 Chrome 厚度断言；无外机 HEAD/DLL/PDB，错配仍为推断 | [历史证据](evidence/R09-FOCUS-OUTLINE-20260918.md)；[6b946540 当前 main SDK8 同输出复核](evidence/CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md)；外机根因未确认 |
| R09-06 | 高对比真实配色 | 已满足 | 1f2eac4a | 当前提交正式 Release/XAML `24/24`、solution `0 warning / 0 error`；R09-06 与相邻 R09/高对比源码门禁 `13/13`；源码校验、diff check 通过 | 实际 DynamicResource 绑定的 ProgressBar、Path 图标、禁用文字验证系统语义色；高对比关闭玻璃/阴影/Popup transparency/动画/游戏背景；同一资源字典切回普通 palette 后 ambient/button material 恢复；未宣称真实 OS 切换或呈现帧 | 生产 palette/ResourceDictionary 与隔离 STA WPF；未切换 Windows High Contrast，未验真实 Playnite/系统方案/presented frame/物理 DPI/跨屏/UIA/读屏/IME/ETW/宿主性能/package-host；Demo 原始目录不可用，沿用恢复生产基线；DEV-INSTALL-008 main 安装失败仍单列 | [R09-06 高对比配色证据](evidence/R09-06-HIGH-CONTRAST-20260919.md)；下一项 R09-07 缩略图占位一致 |
| R09-07 | 缩略图占位一致 | 已满足 | 72a1a07b | 当前提交正式 Release/XAML `24/24`、solution `0 warning / 0 error`；R09-07 行为夹具 `1/1`，缩略图/缓存/转换回归 `9/9`，R09 定向回归 `12/12`；源码校验、XAML、diff check 通过 | 真实 STA WPF 固定媒体卡片：无图、录像、缺失、损坏、成功各有状态文字；`96 x 96` 预览槽位保持尺寸，操作行位于 `96 DIP` 之后；成功图片迟到不改变行高；详情录像继续由 MediaElement 承载，状态文字不覆盖播放器 | 合成临时文件、真实生产缩略图控件、隔离 STA Window/offscreen logical DIP；未验真实 Playnite presented frame、物理 DPI/跨屏、UIA/读屏、IME、ETW、宿主性能和 package-host；Demo 原目录不可用，沿用恢复生产基线；DEV-INSTALL-008 main 全量安装失败仍单列，未在 dirty main 上覆盖或重跑安装器 | [R09-07 缩略图占位证据](evidence/R09-07-THUMBNAIL-PLACEHOLDER-20260919.md)；下一项 R09-08 主题背景压力 |
| R09-08 | 主题背景压力 | 已满足 | 7de5c3de | 当前提交正式 Release/XAML `24/24`、solution `0 warning / 0 error`；R09-08 背景压力夹具 `2/2`，主题/材质相邻回归 `8/8`，高对比/主题/阴影回归 `5/5`，R09 定向 `14/14`；源码校验、XAML、diff check 通过 | 四种合成宿主背景下读取实际 backdrop/ambient/glass 资源；透明 stop 未被不透明层伪装，按 backdrop→ambient→surface 合成后的正文全部达到 `4.5`，故意失败负例被拒绝；保留正常主题玻璃、高对比回退和原四参数主题工厂入口 | 合成 host ResourceDictionary、真实生产 palette/guard、隔离 STA/WPF、逻辑 DIP；未验真实 Playnite presented frame、物理 DPI/跨屏、UIA/读屏、IME、ETW、宿主性能和 package-host；Demo 原目录不可用，沿用恢复生产基线；DEV-INSTALL-008 main 全量安装失败仍单列，未在 dirty main 上覆盖或重跑安装器 | [R09-08 主题背景压力证据](evidence/R09-08-BACKGROUND-PRESSURE-20260919.md)；下一项 R10-01 上下文返回 |
| R10-01 | 上下文返回 | 已满足 | 97770ed4 | 当前隔离 Release/XAML `24/24`、solution `0 warning / 0 error`；R10-01 行为/负例与相邻告警路由 `13/13`；source/XAML/diff check 通过 | 真实生产 ViewModel/命令接线：告警→任务/游戏详情与任务→游戏详情均用稳定 PlayniteId；返回恢复工作区、页签、筛选、任务/诊断选择和实际 DataGrid ScrollViewer 偏移；原对象消失时不替换其他游戏/任务并给出状态说明 | 隔离 fake/合成状态、STA WPF 控件事件和逻辑 DIP；未验真实 Playnite presented frame、物理 DPI/跨屏、UIA/读屏、IME、ETW、宿主性能、package-host；Demo 原目录不可用，沿用恢复生产基线；DEV-INSTALL-008 main 全量仍为 `73 failed / 588 passed / 57 skipped`，未在 dirty main 覆盖或重跑 | [R10-01 上下文返回证据](evidence/R10-01-CONTEXT-RETURN-20260919.md)；下一项 R10-02 |
| R10-02 | 定位当前游戏 | 已满足（当前 main 受控行为与 Actions #1039 双 SDK 全量 WPF 通过） | `13704e75` 原实现；`6eac7b90` 当前生产源码；本次仅补 `GamePickerKeyboardBehaviorTests` | SDK `8.0.423` / VSTest `17.11.1`；Playnite `net462` / Tests `net472` Release 定向 build `0/0`；R10 context `5/5` + 邻接导航 `20/20` + Production GamePicker Shell `7/7`，共 `32/32`；XAML `24/24` | 任务命令按 `GameId` 精确路由同名游戏；目标缺失时保留当前对象/工作区并提示刷新；生产 Shell 顶部按钮打开选框，选择第二个同名项后对象与 ID 一致、选框关闭并还焦点；ViewModel 缺失 ID 不回退到同名项 | 合成 DTO、隔离 STA WPF、工作区加载审计回调；真实 Playnite/媒体后端、物理键鼠/IME、UIA/读屏、呈现帧、DPI/跨屏、ETW/宿主性能未验；本机 SDK8 不替代 CI SDK10；旧 #990 日志 API 403、根因未知；新 run 未确认 | [R10-02 当前 main 复核包](evidence/R10-02-CURRENT-MAIN-RECHECK-20261015/README.md)；Actions #1039/run `38872163664` SDK8 `116878566799` + SDK10.0.401 `116878566530` 全量 WPF `124/124` 类成功；下一项 R10-03 |
| R10-03 | 搜索快捷键 | 已满足（当前 main 受控行为与 Actions #1039 双 SDK 全量 WPF 通过） | 实现 `563e6862`；当前源码基线 `95109c22` | SDK8 `8.0.423` / VSTest `17.11.1`；Playnite `net462` / Tests `net472` Release build `0/0`；R10 邻接组 `13/13`；R09 `2/2`；source validation/diff check 通过 | 实际 PreviewKeyDown routed event 调用生产 Shell 搜索命令并聚焦当前 Media 页真实 `MediaSearchTextBox`；对话框/游戏选框/紧凑浏览器和 Ctrl+Z/C/普通 F 负例保持原焦点且未处理；其他工作区映射沿用既有路径 | 合成修饰键、隔离 STA WPF Window；没有改 XAML/模板/命令策略。未验真实 Playnite 全局热键、物理键盘/IME、UIA/读屏、presented frame、物理 DPI/跨屏、ETW/宿主性能；SDK8 不替代 CI SDK10.0.401。外机 R09 仍缺 HEAD/DLL/PDB，旧源/产物错配只是推断 | [当前 main 行为复核包](evidence/R10-03-CURRENT-MAIN-RECHECK-20261015/README.md)；R09 当前构建对照见[外机失败复核](evidence/CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md)；Actions #1039/run `38872163664` SDK8 `116878566799` + SDK10.0.401 `116878566530` 全量 WPF `124/124` 类成功；下一项 R10-04 |
| R10-04 | 快捷键帮助 | 已满足（当前 main 受控行为与 Actions #1039 双 SDK 全量 WPF 通过） | `19be9f12` 原实现；当前 main 对照 `6cf27923` | SDK9.0.302/VSTest17.14.1 全解方案 Release build `0/0`；R10 帮助及相邻行为 `18/18`，R09 `2/2`；XAML `24/24`、source validation/diff check 通过 | Shell帮助正例实际打开/关闭并展示当前媒体页Ctrl+F；生产Shell命令CanExecute=false时点击实际帮助按钮后Popup打开但不列快捷键；helper fake ICommand正/负例通过 | 本机仅有 SDK9，仓库 global.json 固定 SDK8.0.423；隔离 STA/testhost 和合成状态，真实 Playnite/UIA/IME/物理DPI/呈现/ETW/宿主性能未验。历史 Actions 日志在前次复核时不可读；当前 #1039 SDK8/SDK10.0.401 全量 WPF 门禁均通过；SDK9 本机结果仍单独标记 | [历史实现证据](evidence/R10-04-KEYBOARD-HELP-20260919.md)；[当前main复核](evidence/R10-04-CURRENT-MAIN-RECHECK-20261015/README.md)；R09 对照见[evidence](evidence/R09-FOCUS-CURRENT-MAIN-RECHECK-20261015/README.md)；Actions #1039/run `38872163664` SDK8 `116878566799` + SDK10.0.401 `116878566530` 全量 WPF `124/124` 类成功；下一项 R10-05 |
| R10-05 | 筛选预设 | 已满足（当前 main 受控行为与 Actions #1039 双 SDK 全量 WPF 通过） | `005dc2c5` 原实现；复核构建 `e2353aa3` + 新测试源 SHA | SDK9.0.302/VSTest17.14.1 全解方案 Release build `0/0`；R10预设 `6/6`、任务响应式 `8/8`、工具栏 `1/1`，共 `15/15`；XAML `24/24`、source validation/diff check通过 | 生产快照/应用方法行为验证只存标量筛选值并恢复搜索/状态/游戏/类型；动态选项失效安全回退“全部”；非法旧配置与重复ID回退/去重；真实重命名/删除方法取消后集合、对象、顺序、选择、草稿不变 | 合成设置/筛选值、空任务集合、隔离STA/net472；取消路径未调用设置持久化；未验真实Playnite确认UI/设置存盘、presented frame、DPI/跨屏、UIA/读屏、IME、ETW/宿主性能。Q14测试断言通过且exit0，随后testhost报告COM teardown异常。历史 #991 失败日志仍不可读、根因未知；当前 Actions #1039 SDK8/SDK10.0.401 全量 WPF 门禁均通过 | [历史证据](evidence/R10-05-FILTER-PRESETS-20260919.md)；[当前main复核](evidence/R10-05-CURRENT-MAIN-RECHECK-20261015/README.md)；Actions #1039/run `38872163664` SDK8 `116878566799` + SDK10.0.401 `116878566530` 全量 WPF `124/124` 类成功；下一项 R10-06 |
| R10-06 | 筛选来源提示 | 已满足（当前 main 受控行为；Actions #1039 双 SDK 通过） | 实现 `5403d797`；本轮基线 `a2fdb4f6` + 当前测试源 SHA | SDK9.0.302 Release net462/net472 项目及引用构建；`R10FilterSourceBehaviorTests 3/3`、R09 focus 相邻复核 `2/2`；source validator/diff check 通过 | 真实 TaskCenterView 按钮执行生产 DashboardViewModel 命令；清除导航来源后 banner 折叠、CanExecute 禁用、合成任务列表恢复第二条匹配任务；活动 LatestRequestCoordinator token 取消；搜索/状态/游戏/类型/历史草稿值保留 | 未运行真实任务服务/Worker；仅验证活动请求协调器 token 失效，没有实际服务请求；隔离 STA/net472 合成状态，不等于真实 Playnite/UIA/读屏/IME/物理 DPI/跨屏/presented frame/ETW/宿主性能/package-host。#992 jobs `115852859775`/`115852860137` 与 #993 jobs `115872658151`/`115872658675` 均 exit1；logs API 403，#993 job 页面要求登录，无完整日志/根因；SDK9 不替代 SDK8.0.423/SDK10.0.401 | [当前 main 行为复核与 CI 日志](evidence/R10-06-CURRENT-MAIN-RECHECK-20261014/README.md)；本机 R10FilterSourceBehaviorTests `3/3`；#992/#993 已由 GitHub 连接读取完整日志，实际失败在独立 R08 motion fixture，不属于此项；Actions #1039/run `38872163664` SDK8/SDK10.0.401 全量 WPF 通过 |
| R10-07 | 侧栏信息密度 | 已满足（当前 main 受控行为与 Actions #1039 双 SDK 全量 WPF 通过） | 本阶段补证（生产实现既有能力） | Release `ProductionShellChromeSourceTests 22/22`；XAML `24/24`；WPF 静态审查 0 errors；source/XAML/diff check 通过 | 当前生产 Shell STA 行为覆盖折叠/展开后选中态、7 个入口图标/Tooltip/Automation 名称/Tab 键入口、72/270 DIP 宽度、长标签主区空间、Reduced Motion 与时钟清理 | 隔离 STA WPF 和逻辑 DIP；未验真实 Playnite/package-host、物理 DPI/跨屏、presented frame、外部 UIA/读屏、OS 键盘/IME、ETW、宿主性能；Demo 原目录不可用，沿用恢复生产基线 | [历史侧栏证据](evidence/R10-07-SIDEBAR-DENSITY-20260919.md)；[当前 main 复核](evidence/R10-07-CURRENT-MAIN-RECHECK-20261014/README.md)；Actions #1039/run `38872163664` SDK8 `116878566799` + SDK10.0.401 `116878566530` 全量 WPF `124/124` 类成功；下一项 R10-08 |
| R10-08 | 最近操作续接 | 已满足（当前 main 受控行为、Render QA 与 Actions #1039 双 SDK 全量 WPF 通过） | 既有记录/清理实现；本轮基线 `e0a72c9`，修正 GSC-058 后卡片不可达 | SDK9.0.302 Release 重建；`R10RecentAccessBehaviorTests 2/2`、R09 focus `2/2`、侧栏 `22/22`；XAML `24/24`；RenderHarness 双主题 `render-qa OK`、0 PROBLEM；source/XAML/WPF static/diff checks 通过 | 同栏切换最近任务/最近访问，默认任务模式不增加高度；真实 STA Window 点击两侧切换按钮并分别取键盘焦点；检查列表独立、隐藏态不可见和访问行命令单次执行；最近记录最多8条、稳定 ID、可清理移除对象且不序列化本地路径 | 合成记录与fake命令，不访问用户设置/Playnite/Worker；offscreen RenderHarness 只截图默认任务模式，切换后列表由真实 STA 行为验证；真实 Playnite/package-host、外部 UIA/读屏、OS 输入/IME、物理 DPI/跨屏、最终呈现帧、ETW/宿主性能未验；SDK9不替代SDK8/SDK10.0.401 | [历史实现](evidence/R10-08-RECENT-ACCESS-20260919.md)；[当前 main 入口/行为/视觉证据](evidence/R10-08-CURRENT-MAIN-RECHECK-20261014/README.md)；Actions #1039/run `38872163664` SDK8 `116878566799` + SDK10.0.401 `116878566530` 全量 WPF `124/124` 类成功；下一项 R11-01 当前 main 复核 |
| R11-01 | 版本信息摘要 | 已满足（当前 main 复核 17/17） | 本阶段实现；本轮当前 main 基线 58090d99 | SDK9.0.302 Release 重建 Playnite net462/测试 net472 0 warning/0 error；R11VersionSummaryBehaviorTests 2/2，Save 空态/排序/列宽相邻 15/15（总计17/17）；XAML 24/24；source validation/diff check 通过 | 实际 SaveCenterView/STA Window 加载合成历史行与 7 个生产 DataGrid 列；来源空值显示未知设备；未提供 readiness 与锁定态保持中性；Ready 才显示可恢复；固定列宽/详情ToolTip不推宽；复用现有 DTO 与命令 | 合成 DTO 与逻辑 DIP；未验真实 Playnite/package-host、归档服务、最终呈现帧、物理 DPI/跨屏、外部 UIA/读屏/IME、ETW/宿主性能；没有真实存档/用户设置读写；代码已在 58090d99 推送，SDK9不替代SDK8/CI SDK10.0.401；该提交 Actions 当前读取受限 | [历史实现证据](evidence/R11-01-VERSION-SUMMARY-20260919.md)；[当前 main 复核](evidence/R11-01-CURRENT-MAIN-RECHECK-20261014/README.md)；下一项 R11-02 当前 main 复核 |
| R11-02 | 双版本对比选择 | 已满足 | 本阶段实现 | `R11VersionComparisonBehaviorTests 2/2`；R11-02 与 R11-01、Save 页面 R06 相邻回归 `15/15`；定向 Release/net462 编译 `0 warning / 0 error`；XAML `24/24`；source/XAML/diff check 通过 | 比较页显式提供 A（基准）/B（对照）两个真实 ComboBox；默认仍是上一版本→当前版本；可交换 A/B 并重新使用现有 CompareBackups；界面明确新增属于 B、删除属于 A；同一版本禁用比较且提示不发起比较或恢复；Core 反向 manifest 反例覆盖新增/删除/修改/大小增量 | 合成 manifest、真实 SaveCenterView/隔离 STA WPF 和逻辑 DIP；未验真实 Worker IPC/归档读取、Playnite/package-host、最终呈现、物理 DPI/跨屏、UIA/读屏、IME、ETW、宿主性能；Demo 原目录不可用，沿用恢复生产基线；main DEV-INSTALL-008 `73/588/57` 与安装器退出 1 仍单列 | [R11-02 双版本对比选择证据](evidence/R11-02-VERSION-COMPARISON-20260919.md)；下一项 R11-03 差异列表搜索 |
| R11-03 | 差异列表搜索 | 已满足 | 本阶段实现 | `R11DiffListSearchBehaviorTests 2/2`；R11-01/R11-02/R11-03 SaveCenter WPF 夹具串行组合 `6/6`；R06 存档页相邻回归 `11/11`；定向 Release/net462 编译 `0 warning / 0 error`；XAML `24/24`；source/XAML/diff check 通过 | 比较页按新增/修改/删除和路径片段筛选；匹配计数独立于当前显示窗口；每类初始最多 `120` 条并可逐步加载；完整相对路径可选择并通过现有复制命令复制；未变化与非精确比较的未知差异独立提示；修正 A/B 比较卡片行位重叠 | 合成 DTO、真实 SaveCenterView/隔离 STA WPF 和逻辑 DIP；未验真实 Worker IPC/归档读取、大型真实清单呈现、Playnite/package-host、最终呈现、物理 DPI/跨屏、UIA/读屏、IME、ETW、宿主性能；Demo 原目录不可用，沿用恢复生产基线；main DEV-INSTALL-008 `73/588/57` 与安装器退出 1 仍单列 | [R11-03 差异列表搜索证据](evidence/R11-03-DIFF-LIST-20260919.md)；下一项 R11-04 版本说明编辑 |
| R11-04 | 版本说明编辑 | 已满足 | 本阶段补齐取消入口并补证既有持久化能力 | `R11VersionNoteBehaviorTests 3/3`；R11-01/R11-02/R11-03/R11-04 SaveCenter 串行组合 `9/9`；Worker SQLite 重启可读 `1/1`；R06 存档页相邻回归 `11/11`；D 盘隔离 Release solution `0 warning / 0 error`；XAML `24/24`；source validation/diff check 通过 | 真实 STA `SaveCenterView` 绑定探针：版本备注草稿可取消并恢复原值；重复版本说明仍按稳定 `BackupId` 选择；编辑请求只含备注/锁定，不改归档文件名 | 合成 DTO、隔离 SQLite/STA WPF；未验真实 Ludusavi IPC/归档读取和真实 Worker 进程重启、Playnite/package-host、最终呈现、物理 DPI/跨屏、UIA/读屏、真实键盘/IME、ETW、宿主性能；Demo 原目录不可用，沿用恢复生产基线；C 盘空间不足改用 D 盘隔离输出；main DEV-INSTALL-008 `73/588/57` 与安装器退出 1 仍单列 | [R11-04 版本说明编辑证据](evidence/R11-04-VERSION-NOTE-20260919.md)；下一项 R11-05 保护操作解释 |
| R11-05 | 保护操作解释 | 已满足（当前 main `884e826e`：18/18） | 复用现有保护 DTO/保留规划/Worker 路径；补生产 cell template 绑定行为证据 | SDK `8.0.423` / VSTest `17.11.1`：Playnite `R11ProtectionBehaviorTests 3/3`、Core `RetentionPlannerTests 3/3`、Worker `RetentionSimulationServiceTests 12/12`；相关测试项目及引用 Release 构建通过；source validation/diff check 通过 | 从真实 `SaveCenterView` 取生产“状态”列 DataTemplate 并加载绑定：锁定与普通状态 glyph/文案/解释一致，Ready 无内容负例不显示保护；Core 解锁正例和其它保护排除负例；Worker 合成预览/应用只保留未保护候选 | 合成 DTO、隔离 SQLite/测试归档与 STA WPF；cell template 行为不等于最终像素；未验真实 Ludusavi IPC、Playnite/package-host、presented frame、物理 DPI/跨屏、OS UIA/读屏/IME、ETW/宿主性能。未运行完整 solution build；未触及用户数据。CI SDK8/SDK10.0.401 待推送后追踪 | [R11-05 当前 main 复核包](evidence/R11-05-CURRENT-MAIN-RECHECK-20261015/README.md)；下一项 R11-06 备份前变更摘要 |
| R11-06 | 备份前变更摘要 | 本机复核通过；Actions 待终态 | `40af39c8`（仅测试） | SDK `8.0.423` / VSTest `17.11.1`：Playnite `R11BackupPreviewBehaviorTests 4/4`；Worker `BackupPreviewBehaviorTests 2/2`；相关项目/引用 Release 构建通过；XAML `24/24`；source validation/diff check 通过；非完整 solution build | 真实 `SaveCenterView` Ready 卡片显示合成摘要/路径；Automation Peer 执行预览命令且立即备份命令保持未执行；Empty 卡片收起，Loading 不冒充 0 路径，NoData 明确表示扫描已完成；当前代码的 preview 跳过备份目录创建，立即备份重新读取游戏/匹配并使用非 preview 参数 | WPF 使用合成 DTO 和隔离 STA；Worker JSON 解析用合成结果，未执行真实 orchestration/IPC/Ludusavi/归档目录验证。未验真实 Playnite/package-host、系统 UIA/读屏/IME、presented frame、物理 DPI/跨屏、ETW/宿主性能；未触碰用户存档/媒体/云端；Demo 原目录不可用 | [R11-06 当前 main 复核](evidence/R11-06-CURRENT-MAIN-RECHECK-20261015/README.md)；R00-08 已复核，freshness `14/14`；下一项 R12-01 |
| R11-07 | 备份结果分层 | 已满足 | 02860571 | `R11BackupPreviewBehaviorTests 3/3`；Worker 分层/终态/实时事件 `9/9`，云状态相邻组合 `20/20`；完整 R11 Playnite 定向 `14/14`；D 盘隔离 Release `0 warning / 0 error`；XAML `24/24`；source/XAML/diff check 通过 | 真实 `SaveCenterView`/隔离 STA 夹具验证本地成功+云端排队显示和单独重试按钮；`Uploaded` 显示待远端校验且负例不显示上传重试；不因云端失败隐藏本地历史 | 合成 DTO/fake TaskCoordinator/隔离 WPF；未验真实 Ludusavi/rclone/Worker IPC、Playnite/package-host、presented frame、物理 DPI/跨屏、UIA/读屏、IME、ETW、宿主性能；Demo 原目录不可用；main DEV-INSTALL-008 `73/588/57`/退出 1 仍独立记录 | [R11-07 备份结果分层证据](evidence/R11-07-BACKUP-RESULT-LAYERS-20260919.md)；当前分支已推送；下一项按用户顺序先做 R00/R01 问题修复与证据校正，再回到 R11-08 |
| R11-08 | 历史时间导航 | 当前工作树 SDK9 定向 `6/6`；#1043 SDK8 暴露 fake host 保存回调空引用，已隔离修复，待新 CI 双 SDK 终态 | 复用既有日期范围/CollectionView/命令；补本地日期、DST/UTC 排序与真实控件行为，测试隔离 Playnite 设置宿主 | 本机 SDK9.0.302/VSTest17.14.1 新隔离 Release build：R11 `6/6`；R09 `2/2`、R06 `6/6`、独立 Q13 `1/1`；source validation/freshness/diff check 通过 | 生产 SaveCenterView 日期筛选、清空、最近/较早导航、空范围禁用；列宽持久化路径另由 R06/Q13 行为测试覆盖；合成 host 的 debounce save 不触真实 Plugin API | 本机 SDK9 不替代仓库 SDK8.0.423/CI SDK10.0.401；#1043 SDK8失败修复尚待新 run；未验 Playnite/package host、最终帧、物理 DPI/跨屏、OS UIA/IME、ETW/宿主性能 | [历史时间交互证据](evidence/R11-08-CURRENT-MAIN-INTERACTION-20261016/README.md)；[当前宿主隔离复核](evidence/R11-08-CURRENT-MAIN-RECHECK-20261016/README.md)；新 CI 终态后推进依赖满足项 |
| R12-01 | 恢复分步摘要 | 本机行为复核通过；Actions 待终态 | `e1a8da0c` 生产实现；`3f6b355f` 仅补测试 | SDK `8.0.423` / VSTest `17.11.1`：`R12RestoreWorkflowBehaviorTests 8/8`、`R12RestoreConflictExplanationBehaviorTests 4/4`、`R12RestoreRevalidationBehaviorTests 2/2`；合计 `14/14`；相关测试项目/引用 Release build `0/0`；XAML `24/24`；source validation/diff check 通过；非完整 solution build | 真实生产 `SaveCenterView` 在隔离 STA 中显示四阶段绑定：目标核对失败及错误细节、执行仍 pending 且明确未写入；切换集合后显示 readiness warning、目标/执行 active 和 `PreRestore`；模型正负例覆盖无检查、目标失败、执行回滚、PreRestore 中止、成功完成 | 合成 DTO/选中版本和隔离 WPF Window；不是 Playnite/package-host/Worker IPC/真实恢复；未写真实用户存档；未验 presented frame、物理 DPI/跨屏、OS UIA/读屏/IME、ETW 或宿主性能。Demo 原目录不可用，本轮无生产视觉变更，沿用恢复生产基线 | [R12-01 当前 main 复核包](evidence/R12-01-CURRENT-MAIN-RECHECK-20261015/README.md)；下一项 R12-02 校验结果解释 |
| R12-02 | 校验结果解释 | 本机行为复核通过；Actions 待终态 | `6cc3a618` 生产实现；`d6640887` 仅补测试 | SDK `9.0.302` / VSTest `17.14.1`：Worker `RestoreReadinessTests 15/15`；Core `UiDisplayMappingTests 40/40`；Playnite R12 readiness view/workflow + R11 summary `11/11`；合计 `66/66`；各测试项目及引用 Release 构建通过；XAML `24/24`、source validation/diff check 通过；非完整 solution build | 生产 `SaveCenterView` 隔离 STA 实际呈现未验证/无哈希、部分覆盖且过期、损坏三态；核对 `1/2` 覆盖、文件/大小计数、旧结果说明、时间 Tooltip 与 Automation HelpText。Worker 合成 ZIP 负例继续区分未提供哈希、部分/失败哈希、损坏/缺失归档、取消与清理；不把可读取归档等同于哈希全覆盖 | 合成 DTO/ZIP/Manifest、fake/隔离 SQLite/WPF；未验真实 Playnite/package-host、Worker IPC/用户存档写入、最终呈现帧、物理 DPI/跨屏、OS UIA/IME、ETW 或宿主性能。Demo 原目录不可用；无生产视觉/安全路径变更 | [R12-02 当前 main 复核包](evidence/R12-02-CURRENT-MAIN-RECHECK-20261015/README.md)；R12-03 当前 main 行为复核见 [证据](evidence/R12-03-CURRENT-MAIN-RECHECK-20261015/README.md)；下一项 Q24-04 完整业务状态键盘行为补证 |
| R12-03 | 目标路径核对 | 当前 main 受控行为复核通过；Actions 待终态 | `c0adb1b7` 既有生产实现；本批只补 Playnite 行为测试 | SDK `9.0.302` / VSTest `17.14.1`：Playnite `R12PathRemapBehaviorTests 3/3`、Worker `PathRemapServiceTests 4/4`；定向 Release 测试项目/引用构建通过；XAML `24/24`、source validation、diff check 通过；非完整 solution build | 生产 `MaintenanceView` 隔离 STA 实际加载；80 条路径下 `260 DIP` 有限视口与 Recycling 虚拟化生效；210 字符段原/目标路径可完整选择，Copy 命令处于可执行状态；Worker 负例验证异盘/相似根辨别、未确认拒绝、缺失目标跳过，数据库保持原路径且目标文件不存在 | 合成路径/fake/隔离 SQLite 和 STA WPF；未执行 Copy 或路径迁移命令，不写系统剪贴板或用户目录；未验真实 Playnite/package-host、UIA/IME、物理 DPI/跨屏、presented frame、ETW/宿主性能；Demo 原目录不可用，沿用恢复生产基线 | [R12-03 当前 main 行为复核](evidence/R12-03-CURRENT-MAIN-RECHECK-20261015/README.md)；R12-04 至 R12-08 已有独立满足证据，下一项转 Q24-04 完整业务状态键盘行为补证 |
| R12-04 | 恢复保护备份 | 已满足（当前 main 复核） | e4e42f40；本轮只补测试负例 | SDK8.0.423：Worker `RestoreOrchestratorTests + GameOperationLockTests 26/26`，Playnite `R12RestoreWorkflowBehaviorTests 8/8`；项目引用 Release build 成功，XAML `24/24`；source validation、diff check 通过；NuGet audit 源不可达有 `NU1900` 警告 | 保护失败不触发目标 Restore/Edit；同一 `GameOperationLock` 上重试读取并锁定最新 fake 状态 `A-latest`；持有同游戏 Backup 锁时 Restore 返回 `GAME_OPERATION_BUSY`，PreRestore/Restore/Edit 调用均为 0 | 合成/fake/隔离 SQLite、STA WPF 和逻辑 DIP；未验真实存档、Playnite/package-host、物理 DPI/跨屏、presented frame、外部 UIA/IME、ETW、宿主性能；Demo 原目录不可用，沿用恢复生产基线 | [当前 main 复核与 TRX](evidence/R12-04-CURRENT-MAIN-RECHECK-20261016/README.md)；[原实现证据](evidence/R12-04-RESTORE-PROTECTION-20260919.md)；新锁负例随本次提交追踪 Actions |
| R12-05 | 远端下载进度 | 已满足 | 23e5b9d4（原实现） | 原 Playnite `3/3`、Worker `22/22` 与隔离 Debug solution `0/0`；当前 main SDK8 Worker `RemoteBackupStagingSafetyTests + TaskQueryPersistenceTests 32/32`、Playnite `R12RemoteStageProgressBehaviorTests + R15TaskSourceNavigationTests 8/8`；后续完整 Release solution `0/0`、XAML `24/24`；source validation、diff check 通过 | 维护页绑定现有 TaskCoordinator 事件流，区分准备/下载到隔离区/一致性校验/版本确认/等待恢复；取消按钮只在活动态出现；成功不写成恢复完成；取消、失败和隔离清理结果分别投影。当前 main 修正投影以保留 Worker 原始取消清理结果，负例验证锁文件残留可见、释放后仅清理隔离目录 | 合成 DTO/fake/隔离目录、net462 Playnite 测试程序集；后续完整 build 在 R13 当前 main 验证中运行，含本修正及测试夹具；无 XAML 变更故未跑 Render QA；未验真实 Playnite/package-host、真实远端、物理 DPI/跨屏、独立 UIA/IME、presented frame、ETW、宿主性能；Demo 原目录不可用，沿用恢复生产基线 | [R12-05 当前 main 复核](evidence/R12-05-CURRENT-MAIN-RECHECK-20261016/README.md)；[原阶段证据](evidence/R12-05-REMOTE-STAGE-PROGRESS-20260919.md)；R13-01 当前 main 复核与 R12 CI 状态见本节 |
| R13-01 | 队列阶段展示 | 已满足 | 803470b8 | Core `UiDisplayMappingTests 40/40`；Worker `CloudTransferStateTests 14/14`；Playnite `R13CloudTransferStageBehaviorTests + MaintenanceCloudTransferResolverTests 21/21`；SDK8.0.423 完整 Release solution `0 warning / 0 error`；XAML `24/24`、source validation、diff check 通过 | 当前 main DTO 对待队列/等待网络/普通重试/上传中/校验中/等待验证/已验证分别映射；Worker 隔离 SQLite 状态机证明只有完成远端检查后才显示 Verified；实际生产 `MaintenanceView` DataGrid 与详情 STA 行为显示上传和已校验的不同阶段/保证级别 | 合成 DTO、fake、隔离 SQLite 与逻辑 DIP STA 窗口；无生产代码或 XAML 改动；未验真实 Playnite/package-host、真实远端、物理 DPI/跨屏、独立 UIA/IME、presented frame、ETW、宿主性能；Demo 原目录不可用，沿用恢复生产基线 | [R13-01 当前 main 复核与 TRX / SHA](evidence/R13-01-CURRENT-MAIN-RECHECK-20261016/README.md)；[历史实现证据](evidence/R13-01-CLOUD-QUEUE-STAGES-20260919.md)；下一项 R13-02 下次重试时间 |
| R12-06 | 恢复冲突说明 | 已满足 | 423856b2 | Playnite R12 恢复流程/冲突说明 `11/11`；Worker `RestoreOrchestratorTests 12/12`；隔离 Debug solution `0 warning / 0 error`；XAML `24/24`；source validation、diff check 通过 | 游戏运行、操作锁、磁盘空间、目标权限和未知写入失败分别给出处理步骤；行为测试验证阶段状态、前置失败不越过执行、权限负例与 XAML HelpText 绑定；不建议无依据关闭安全机制 | 合成 DTO/fake、隔离目录、net462 Playnite 测试程序集；未验真实 Playnite/package-host、物理 DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能；Demo 原目录不可用，沿用恢复生产基线；main dirty R08 文件和 `src.zip` 未触碰 | [R12-06 恢复冲突说明证据](evidence/R12-06-RESTORE-CONFLICT-EXPLANATION-20260919.md)；当前分支已推送，下一项 R12-07 预览失效重验 |
| R12-07 | 预览失效重验 | 已满足 | 00724e62 | Playnite R12 `13/13`；Worker 恢复就绪/编排合计 `27/27`；隔离 Debug solution `0 warning / 0 error`；XAML `24/24`；source validation、diff check 通过 | 确认返回后重新比较游戏/版本身份，切换对象不提交旧确认；Worker 按最新映射和精确 BackupId 重验目标，实际写入前后保留预览；同大小不同内容的归档按 SHA-256 识别失效 | 合成 Manifest/归档、fake Worker、隔离目录和 net462 Playnite 测试程序集；未验真实 Playnite/package-host、物理 DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能；Demo 原目录不可用，沿用恢复生产基线；main dirty R08 文件和 `src.zip` 未触碰 | [R12-07 预览失效重验证据](evidence/R12-07-RESTORE-REVALIDATION-20260919.md)；当前分支已推送，下一项 R12-08 恢复结果报告 |
| R12-08 | 恢复结果报告 | 已满足 | fd4756ea | Playnite R12 `15/15`；Worker `RestoreReadinessTests\|RestoreOrchestratorTests\|TaskQueryPersistenceTests 34/34`；隔离 Debug solution `0 warning / 0 error`；XAML `24/24`；source validation、diff check 通过 | 复用既有 RestoreOrchestrator/TaskCoordinator/任务详情滚动容器；报告展示目标版本、任务 ID、预览文件范围、PreRestore 保护、失败阶段与完成/回滚/人工介入/取消/失败结果；任务最近/活动/分页查询持久化回读；复制命令只复制脱敏报告 | 合成 DTO/fake Worker/隔离 SQLite 与 net462 Playnite 测试程序集；当前 linked worktree WPF 临时项目仍 Access denied，最终构建使用当前分支外部源码副本和独立输出根；未验真实 Playnite/package-host、物理 DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能；Demo 原目录不可用，沿用恢复生产基线；main dirty R08 文件和 `src.zip` 未触碰、未合并 | [R12-08 恢复结果报告证据](evidence/R12-08-RESTORE-RESULT-REPORT-20260919.md)；当前分支已推送，下一项 R13-01 队列阶段展示 |
| R13-01 | 队列阶段展示 | 已满足 | 803470b8 | Core 阶段映射 `27/27`；Playnite `R13CloudTransferStageBehaviorTests\|MaintenanceCloudTransferResolverTests 7/7`；Worker `CloudTransferStateTests 11/11`；隔离 Debug solution `0 warning / 0 error`；XAML `24/24`；source validation、diff check 通过 | 复用既有云端状态机和队列摘要；维护页显示等待队列/等待网络/等待重试/上传中/验证中/等待验证/已验证；认证失败不冒充网络等待；详情保留 GuaranteeDisplay，上传成功不冒充远端校验 | 合成 DTO/fake/隔离 Worker 与 net462 Playnite 程序集；通过显式 GIT_DIR 绑定当前 linked worktree 身份；未验真实 Playnite/package-host、真实远端、物理 DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能；Demo 原目录不可用，沿用恢复生产基线；main dirty R08 文件和 `src.zip` 未触碰、未合并 | [R13-01 队列阶段展示证据](evidence/R13-01-CLOUD-QUEUE-STAGES-20260919.md)；当前分支已推送，下一项 R13-02 下次重试时间 |
| R13-02 | 下次重试时间 | 已满足（当前 main 行为补证） | 6fd22892 | 原 Core `29/29`、Playnite `2/2`；当前 main SDK8 Core 重试边界 `2/2`、Playnite `R13CloudTransferStageBehaviorTests 14/14`；Release 定向构建、source validation、diff check 通过；R00/R01 `14/14 fresh / 0 stale / 0 source matches` | 复用 NextAttemptUtc/Local；实际生产维护页详情展示有界相对时间、绝对时间/UTC辅助文本、Automation HelpText 与同记录错误原因；已到期重新求值显示可立即重试且无负倒计时；无逐行计时器 | 合成 DTO 与逻辑 `1440×1000 DIP` STA 窗口；到期戳重新求值模拟时钟快进后的边界，不证明 OS 时钟变更事件触发即时绑定刷新；未验真实 Playnite/package-host、真实远端、物理 DPI/跨屏、OS UIA/IME、presented frame、ETW、宿主性能；Demo 原目录不可用，沿用恢复生产基线 | [R13-02 当前 main 复核包](evidence/R13-02-CURRENT-MAIN-RECHECK-20261016/README.md)；[原实现证据](evidence/R13-02-RETRY-TIMING-20260919.md)；下一项 R13-03 手动重试范围 |
| R13-03 | 手动重试范围 | 已满足（当前 main 行为补证） | `680ea83a`（实现）/ `ad2e6716`（当前行为证据） | 历史 Playnite `8/8`、Core `29/29`、Worker/IPC 幂等门禁通过；当前 SDK8 Core `125/125`、Worker `360 passed / 1 Named Pipe skip`、隔离 Release solution build `0/0`、XAML `24/24`；同一隔离程序集 Playnite R13 类 `14/14`；R00/R01 freshness `14 fresh / 0 stale / 0 source matches` | 真实 BusyOperationCoordinator/RelayCommand 门控首个未完成传输，重复 Execute 不提交第二次；失败/排队选择项可重试，传输中/已上传/已验证为负例；已有部分成功/队列筛选分页与 RequestId replay protection 沿用；Worker fake-cloud 失败验证复用已完成的本地副本，不再次运行 Ludusavi | R05 Tooltip 真实系统指针 fixture 在本机 GetCursorPos `ERROR_ACCESS_DENIED (5)`，完整 Playnite 类隔离扫描 `45/124` 中止，不能记全量通过；R09 焦点轮廓同一隔离 DLL/PDB 当前 `2/2`。R09 外机日志缺 HEAD/DLL/PDB，显示 WPF `116` 类且 line151 数值断言与当前源码映射不符，根因未确认。真实 Playnite/package-host、远端、DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能仍待验；只用合成/隔离数据，Demo 原目录不可用，沿用恢复生产基线 | [R13-03 当前 main 复核](evidence/R13-03-CURRENT-MAIN-RECHECK-20261016/README.md)；[原实现证据](evidence/R13-03-MANUAL-RETRY-SCOPE-20260919.md)；推送后查 SDK8 与 SDK10.0.401 Actions，下一项 R13-04 |
| R13-04 | 暂停与允许时段 | 已满足 | cca3f052、ccdbacb6 | 原实现 `cca3f052`：Core `1/1`、Worker `3/3`、Playnite R13 `9/9` 与 PortableSettings `10/10`、隔离 Debug solution `0/0`、XAML `24/24`。当前 main 持久化补证：Worker `4/4`、Core `1/1`、Playnite R13/设置迁移 `24/24`、同输出 R09 `2/2`；定向项目 build 通过，SDK `8.0.423`。| 摘要明确区分用户暂停、允许时段外、队列空闲和运行中；设置页明确恢复入口、下一轮 Worker 检查生效且不取消已开始上传 | 合成 DTO、现有 Worker 策略/fake 设置和 net462 Playnite 程序集；未验真实 Playnite/package-host、真实远端、物理 DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能 | [R13-04证据](evidence/R13-04-CURRENT-MAIN-RECHECK-20261016/README.md)；Actions #1039/run `38872163664` SDK8/SDK10.0.401 均成功，source108类/WPF124类全通过，SDK8 Render QA/静态审查/扩展包成功；真实 Playnite、物理 DPI/跨屏、UIA/IME、最终呈现帧、ETW/宿主性能仍未验 |
| R13-05 | 远端证据详情 | 已满足 | e280cf1c | Core 定向 `2/2`；Worker 云状态映射 `1/1`；Playnite `R13CloudTransferStageBehaviorTests 10/10`；Release 隔离 solution `0 warning / 0 error`，Playnite `net462`；XAML `24/24`；source validation、diff check 通过 | 维护页现有详情滚动容器/共享卡片样式接入远端对象、来源设备、最后尝试和最后成功校验；实际 DTO/Worker 映射和认证参数负例通过；未运行 RenderHarness/真实呈现 | 只使用合成 DTO、fake/隔离 SQLite、外部源码/输出；远端路径只生成脱敏显示值；历史成功校验没有持久化时保持未知；未验真实 Playnite/package-host、真实云端、物理 DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能；main 用户改动和 `src.zip` 未触碰 | [R13-05 远端证据详情](evidence/R13-05-REMOTE-EVIDENCE-20260919.md)；当前分支已推送，下一项 R13-06 离线恢复反馈 |
| R13-06 | 离线恢复反馈 | 已满足 | e4244329（当前 main 行为复核见下） | 当前 SDK9.0.302 定向项目 build；Core `1/1`、Worker `CloudRetryPersistenceTests 12/12`、Playnite `R13CloudTransferStageBehaviorTests 14/14`、通知去重 `6/6`；另有 R09 对照 `2/2`；source validation/diff check 通过 | 真实 `MaintenanceView` STA 选择网络重试→恢复传输→认证失败；绑定文字随选择变化，退避/`2/6`/批次 `10` 明确，认证负例为空 | 合成 DTO/fake/隔离 SQLite；Worker 每轮 30 秒、最多 10 项、逐项 await；未运行真实网络/Playnite 通知/RenderHarness/宿主性能 | [原始 R13-06 证据](evidence/R13-06-OFFLINE-RECOVERY-20260919.md)；[当前 main 行为复核](evidence/R13-06-CURRENT-MAIN-BEHAVIOR-RECHECK-20261016/README.md)；下一项 R13-07 队列筛选与汇总 |
| R13-07 | 队列筛选与汇总 | 受控行为已满足；#1045 双 SDK 均因过期源码断言失败，校正待新 CI | d6c2af90（原功能；当前复核输入 ea1e1aea + 本轮源改动） | SDK9.0.302/VSTest17.14.1 隔离 Release：Playnite R13 `16/16`、校正后 `WorkspaceStateSourceTests 9/1/0`（通过/跳过/失败）、Worker `CloudTransferStateTests 14/14`；source validation/diff check 通过；freshness 14 条、0 需重跑、0 匹配源路径 | 生产维护页五个筛选控件绑定值实际变化，当前/全局摘要正例和空筛选负例；Worker 合成数据覆盖游戏/状态/设备/时间/全局数；生产分页追加验证大小写不敏感去重、无空 Reset；刷新稳定选择、后页 pending、删除回退均有正负例 | 仅合成 DataContext/Worker store 与隔离 testhost；未通过真实插件 IPC 执行过滤刷新。SDK9 非完整 solution build，不替代 SDK8.0.423/CI SDK10.0.401；真实 Playnite/package-host、UIA/IME、物理 DPI/跨屏、最终呈现、ETW/宿主性能仍未验 | [R13-07 当前 main 行为复核](evidence/R13-07-CURRENT-MAIN-BEHAVIOR-RECHECK-20261016/README.md)、[R13/R14 历史复核](evidence/R13-R14-TARGETED-RECHECK-20260922.md)、[R13-07 原始证据](evidence/R13-07-QUEUE-FILTER-SUMMARY-20260919.md)；新 CI 待终态；[SDK 源码断言校正](evidence/CI-R13-07-SDK-SOURCE-ASSERTION-RECHECK-20261016.md) |
| R13-08 | 失败分类帮助 | 受控行为已满足；新测试待 CI | 96a4c6a9；行为补证为本次测试改动 | 本机 SDK9.0.302/VSTest17.14.1 Playnite Release `net462/net472` 定向 build 后 R13 类 `16/16`；source validation/diff check 通过；对测试改动路径 freshness `14/14 fresh / 0 stale / 0 matched`。SDK8/SDK10.0.401 新提交 Actions 尚未运行 | 真实生产 MaintenanceView/DataGrid 中选择已识别认证失败，帮助可见且文案准确；通过 ExpandCollapse provider 验证原始码/文本展开和收起；选择未知错误后帮助隐藏、分类/下一步为空但原始诊断仍能展开，收起则不可见 | 合成 DTO、隔离 STA 与 fake context；只改测试，无 XAML/生产逻辑变化。外机 R09 另有真实 1/2 失败且总流程 exit 1；其 line151/Thickness 映射与当前源不匹配，具体 DLL/PDB/checkout 身份待同次 summary。未验真实 Playnite/package-host、网络/rclone、UIA/IME、物理呈现、ETW 或宿主性能 | [R13-08 当前 main 行为补证](evidence/R13-08-CURRENT-MAIN-FAILURE-HELP-RECHECK-20261016/README.md)；#1046 仅覆盖父提交 `e30a2976`，推送后必须追踪新 SDK8/SDK10 lanes；外机诊断见[R09 复核](evidence/CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md) |
| R14-01 | 归类建议解释 | 已满足，待环境验证 | 7735cd7c | `R14ClassificationEvidenceTests 1/1`；Core `UiDisplayMappingTests`、Worker `MediaSyncServiceTests` 与 Playnite R13/R14 定向夹具包含在 R13/R14 复核合计 `40/40`、`32/32`、`19/19`；XAML `24/24`、Release `0 error/2` 条既有 warning、source validation/diff check 通过 | 复用现有建议预览、来源规则/会话/进程映射和文件名匹配；预览卡逐条显示候选依据，多候选保留各候选证据；无依据显示“待判断”且不生成目标。受控 RenderHarness 样本覆盖有依据/无依据两类；全报告既有基线仍为 `FAILED`，不扩大为全局 render-qa 通过 | 仅合成 DTO、fake/隔离 Worker/Playnite testhost 和 offscreen logical DIP；未验真实 Playnite/package-host、物理 DPI/跨屏、Windows UIA/读屏/IME、presented frame、ETW 或宿主性能；Demo 原目录不可用，沿用恢复生产基线 | [R13/R14 定向复核](evidence/R13-R14-TARGETED-RECHECK-20260922.md)、[R14-01 归类建议解释](evidence/R14-01-CLASSIFICATION-EVIDENCE-20260919.md)；受控条件已满足，真实宿主边界待验，下一项 R14-02 |
| R14-02 | 预览选择编辑 | 已满足，待环境验证 | 69cf2a72 | `R14ClassificationSelectionTests 4/4`；Worker `MediaSyncServiceTests 20/20` 纳入稳定 ID、目标覆盖和排除语义；Playnite R13/R14 定向夹具包含在 `19/19`；XAML `24/24`、Release `0 error/2` 条既有 warning、source validation/diff check 通过 | 预览卡支持按稳定 MediaId 排除条目；高置信建议可在当前游戏目录中调整目标，汇总显示纳入/排除/可应用数量；排除项不提交，全部排除时命令门禁同步禁用。受控 Media 样本保留 `2` 组/组内 `4` 项和只读选择语义；不把离屏报告写成真实宿主呈现 | 仅合成 DTO、fake/隔离 Worker/Playnite testhost 和 offscreen logical DIP；未验真实 Playnite/package-host、物理 DPI/跨屏、Windows UIA/读屏/IME、presented frame、ETW 或宿主性能；Demo 原目录不可用，沿用恢复生产基线；真实存档、媒体和云端未触碰 | [R13/R14 定向复核](evidence/R13-R14-TARGETED-RECHECK-20260922.md)、[R14-02 预览选择编辑](evidence/R14-02-CLASSIFICATION-SELECTION-20260920.md)；受控条件已满足，真实宿主边界待验，下一项 R14-03 |
| R14-03 | 部分成功处理 | 已满足，待环境验证 | aef251b1 | Worker `MediaSyncServiceTests 20/20`，含逐项失败保留与成功项不重复；Playnite `R14ClassificationSelectionTests 4/4`、`MediaWindowAnchorContractTests 10/10`；XAML `24/24`、Release `0 error/2` 条既有 warning、source validation/diff check 通过 | 批量归类/忽略/恢复保留逐项失败 ID 与原因；失败列表为 `MaxHeight=128`、Recycling 有限列表，重试确认只提交上次失败项，成功项不会再次执行。Playnite 源契约与滚动锚点行为已补证，不把离屏/源契约写成真实宿主呈现 | 仅合成数据、fake/隔离 SQLite 和隔离 WPF testhost；未验真实 Playnite/package-host、RenderHarness 新失败样本、物理 DPI/跨屏、Windows UIA/读屏/IME、presented frame、ETW 或宿主性能；Demo 原目录不可用，沿用恢复生产基线 | [R14-03 定向复核](evidence/R14-03-TARGETED-RECHECK-20260923.md)、[R14-03 部分成功处理](evidence/R14-03-PARTIAL-RETRY-20260920.md)；受控条件已满足，真实宿主边界待验，下一项 R14-04 |
| R14-04 | 撤销边界说明 | 已满足，待环境验证 | 1c0c5a37、a7c39922、03521991 | Worker `MediaSyncServiceTests 20/20`；含正常撤销归档副本恢复与人工收藏/备注变化后的 `UndoneWithConflicts` 负例；XAML `24/24`、Release `0 error/2` 条既有 warning、source validation/diff check 通过，Playnite 目标编译为 `net462` | 历史页提供上次/所选可回退入口；撤销前重新核对批次目标与当前媒体，目标/路径/状态/元数据变化进入冲突，保留后来决定且不恢复移动。受控证据来自 fake/隔离 SQLite，不把编译或离屏结果写成真实宿主行为 | 未验真实 Playnite/package-host、RenderHarness 新呈现、物理 DPI/跨屏、Windows UIA/读屏/IME、presented frame、ETW 或宿主性能；未触碰真实存档、媒体、云端；Demo 原目录不可用，沿用恢复生产基线 | [R14-04 定向复核](evidence/R14-04-TARGETED-RECHECK-20260923.md)、[R14-04 撤销边界说明](evidence/R14-04-UNDO-BOUNDARY-20260920.md)；受控条件已满足，真实宿主边界待验，下一项 R14-05 |
| R14-05 | 重复媒体识别视图 | 已满足，待环境验证 | 136285d5 | Worker `MediaSyncServiceTests 20/20`；Playnite `R14ClassificationSelectionTests 4/4`；XAML `24/24`、Release `0 error/2` 条既有 warning、source validation/diff check 通过。Worker 合成样本为疑似组 `1` 组/组内 `2` 项；既有 clean RenderHarness 报告在 `1040×700` 为 `2` 组/组内 `4` 项、组内列表 `330 DIP` | 复用现有 SHA-256 入库去重能力；当前游戏范围内区分确定哈希组与同类型/文件名/大小疑似组；只读选择查看；扫描上限 `5000`、组上限 `100`、组内展示上限 `24`，列表有限/Recycling；重复页不提供删除或重新归类命令 | 受控证据来自合成 DTO、fake/隔离 SQLite、隔离 WPF testhost 和 offscreen logical DIP；RenderHarness 全报告仍因既有 Overview/Settings/Task/Save 基线为 `FAILED`，不写成全局 render-qa 通过；未验真实 Playnite/package-host、超大真实媒体库、物理 DPI/跨屏、Windows UIA/读屏/IME、presented frame、ETW 或宿主性能；Demo 原目录不可用，沿用恢复生产基线 | [R14-05 定向复核](evidence/R14-05-TARGETED-RECHECK-20260924.md)、[R14-05 重复媒体识别视图](evidence/R14-05-DUPLICATE-INSPECTION-20260920.md)；受控条件已满足，真实宿主边界待验，下一项 R14-06 |
| R14-06 | 批量目标防误选 | 已满足，待环境验证 | cfbb1279 | `GamePickerViewModelTests 22/22`、`R14ClassificationSelectionTests 4/4`、`GamePickerKeyboardBehaviorTests 6/6`；隔离 Release XAML `24/24`、solution `0 error/2` 条既有 warning、Playnite `net462` 编译通过；source/XAML/diff 门禁通过 | 复用现有游戏描述/状态 DTO 与选框筛选/选择；本地 Playnite 图标缺失安全回退；归类目标显示名称、平台、图标和稳定 Playnite ID；新增行为夹具确认同名对象可区分且按 Playnite ID 选中；过滤不改写已选目标，不使用 SelectedIndex。既有 `Shell-Media-1040x700` 只作 offscreen 视觉证据 | 未验真实 Playnite/RenderHarness 新呈现、宿主呈现、物理 DPI/跨屏、UIA/IME、presented frame、ETW 或宿主性能；Demo 原目录不可用，沿用恢复生产基线；IconPath 只读本地已有引用；main 用户改动和 src.zip 未碰、未合并 | [R14-06 目标防误选定向复核](evidence/R14-06-TARGETED-RECHECK-20260924.md)、[R14-06 初始实现](evidence/R14-06-TARGET-GUARD-20260920.md)；受控条件已满足，真实宿主边界待验，下一项 R14-07 |
| R14-07 | 媒体详情浏览 | 已满足，待环境验证 | c17d9bc7 | 选定 Playnite 套件 `46/46`（详情导航、锚点、缩略图 loader/image、语义值、generation/失败回退）；隔离 Release XAML `24/24`、solution `0 error/2` 条既有 warning、Playnite `net462`；source/XAML/diff 门禁通过 | 当前已加载媒体窗口内上一项/下一项；按 MediaId 保持列表行锚点；类型、来源、大小、采集时间和截图 PixelWidth/PixelHeight 可见；视频缺失/格式不支持/MediaFailed 有回退；既有 `Media-1040x700-tab1` 只显示详情入口，未把未打开详情截图当呈现证据 | 未验跨页导航、真实 Playnite/RenderHarness 详情呈现、真实视频编解码、物理 DPI/跨屏、UIA/IME、presented frame、ETW 或宿主性能；Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 src.zip 未碰、未合并 | [R14-07 定向复核](evidence/R14-07-TARGETED-RECHECK-20260924.md)、[R14-07 初始实现](evidence/R14-07-MEDIA-DETAIL-20260920.md)；受控条件已满足，真实宿主边界待验，下一项 R14-08 |
| R14-08 | 来源规则试运行 | 已满足，待环境验证 | 89141528 | Worker `MediaSyncServiceTests 20/20`、Playnite `R14SourceRulePreviewTests 1/1`；隔离 Release XAML `24/24`、solution `0 error/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning、Playnite `net462`；source/XAML/diff 门禁通过 | 复用现有来源 DTO、媒体扩展名和文件模式匹配；样本显示命中/排除、大小、路径和原因；试运行只读，不保存规则、移动文件或写媒体记录；列表 `MaxHeight=240`、Recycling、有限 viewport；既有 `Media-1600x900-tab3` 显示 `4` 项/命中 `2`/排除 `2` 的合成摘要与样本 | Worker 时间预算 `100–5000ms`、扫描预算 `1–5000`、样本预算 `1–200`，并保留取消语义；未验真实来源权限/超大目录、真实 Playnite/RenderHarness/最终呈现、DPI/UIA/IME/presented frame/ETW/宿主性能；Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 src.zip 未碰、未合并 | [R14-08 定向复核](evidence/R14-08-TARGETED-RECHECK-20260924.md)、[R14-08 初始实现](evidence/R14-08-SOURCE-RULE-PREVIEW-20260920.md)；受控条件已满足，真实宿主边界待验，下一项 `R15-01 任务阶段可读` |
| R15-01 | 任务阶段可读 | 已满足，待环境验证 | 4e7ac33a | Worker `TaskCoordinatorFailureTests` + `TaskQueryPersistenceTests` `16/16`；Playnite `R15TaskStageTests 2/2`；隔离 Release XAML `24/24`、solution `0 error/2` 条既有 warning、Playnite `net462`；source/XAML/diff 门禁通过 | 复用已有 TaskCoordinator/任务事件；共享阶段解析和 `StageMessage` 显示扫描、校验、索引、上传、下载、恢复、清理等可读阶段；未知进度显示 `—`；终态错误与最后阶段分离；SQLite 迁移保留旧库读取；既有 `Task-1600x900` 仅作 offscreen 阶段列/负例视觉证据 | 未验真实 Playnite/RenderHarness/最终呈现、各类真实宿主阶段事件全覆盖、DPI/UIA/IME、presented frame、ETW 或宿主性能；Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰、未合并 | [R15-01 定向复核](evidence/R15-01-TARGETED-RECHECK-20260924.md)、[R15-01 初始实现](evidence/R15-01-TASK-STAGES-20260920.md)；受控条件已满足，真实宿主边界待验，下一项 `R15-02 取消过程展示` |
| R15-02 | 取消过程展示 | 已满足，待环境验证 | 9c8241fb | Worker `TaskCoordinatorFailureTests` + `TaskQueryPersistenceTests` `16/16`；Playnite `R15TaskCancellationTests` + `R15TaskStageTests` + `R06TaskProgressBehaviorTests` `8/8`；隔离 Release XAML `24/24`、solution `0 error/2` 条既有 warning、Playnite `net462`；source/XAML/diff 门禁通过 | 复用现有 TaskCoordinator、取消 IPC、TaskStatusDto 和 Task Center；持久化取消阶段，重复取消只发一次请求，`Requested → Finalizing → Cancelled` 收敛；成功/取消竞争和取消后失败分别落到稳定终态；SQLite 旧库通过迁移默认空值兼容；既有 `Task-1600x900` 仅作合成终态视觉证据 | 未验真实不响应取消令牌的长任务、真实 Playnite/RenderHarness/最终呈现、DPI/UIA/IME、presented frame、ETW 或宿主性能；Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰、未合并 | [R15-02 定向复核](evidence/R15-02-TARGETED-RECHECK-20260924.md)、[R15-02 初始实现](evidence/R15-02-TASK-CANCELLATION-20260920.md)；受控条件已满足，真实宿主边界待验，下一项 `R15-03 任务详情时间线` |
| R15-03 | 任务详情时间线 | 已满足，待环境验证 | fe0c05a9 / b6170ec9 | `validate-source.py`、XAML `24/24`、`git diff --check`；Worker `TaskCoordinatorFailureTests + TaskEventBroadcasterTests 12/12`；Playnite `R15TaskTimelineTests + TaskEventUiBatcherTests + R15TaskStageTests + R15TaskCancellationTests + R06TaskProgressBehaviorTests 15/15`；Playnite Release `net462` | 复用现有 TaskChangeEventDto/TaskCoordinator/TaskEventBroadcaster/Task Center；`OccurredUtc`、UTC/本地双显示、缺失时间/事件未知和不臆测重试均有行为证据；新增乱序 UTC、同刻序号稳定排序、跨任务过滤负例；事件窗口最多 64 条/任务、200 个任务，详情时间线 `220 DIP` 有界滚动，广播 clone 保留阶段与取消状态 | 当前隔离 Release XAML `24/24`、solution `0 error/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；截图未打开时间线卡，不作时间线呈现签收；未验 Worker 重启后历史、真实 Playnite/RenderHarness/presented frame、DPI/UIA/IME、ETW 或宿主性能；Demo 原目录不可用，沿用恢复生产基线 | [R15-03 任务详情时间线定向复核](evidence/R15-03-TARGETED-RECHECK-20260924.md)、[R15-03 初始实现](evidence/R15-03-TASK-TIMELINE-20260920.md)；下一项 R15-04 重复通知归并 |
| R15-04 | 重复通知归并 | 已满足，待环境验证 | a67d371e / 6510ccf6 | `validate-source.py`、XAML `24/24`、`git diff --check`；Playnite 核心通知/会话行为 `10/10`，相邻通知反馈/时间线/UI 批处理/R13 夹具合计 `31/31`；Core 通知策略/摘要 `7/7`；Playnite Release `net462`，solution `0 error/2` 条既有 warning | 复用既有 BoundedTaskIdSet、SessionNotificationAccumulator、NotificationLevelPolicy、Task Center 历史和 Dashboard 通知链；进度不领取通知键；相同任务/终态/失败证据只通知一次；不同错误码或未知详情保留；摘要后新失败/取消不静音；Task Center 历史继续保留完整错误。`NotificationFeedbackSourceTests` 只作源码契约边界，不作真实 Toast 签收 | 当前隔离 Release solution 构建通过；WPF 静态检查 `0/28/162`；未验真实 Playnite/package-host Toast 时序、最终呈现/DPI/UIA/IME、ETW 或宿主性能；本批只使用合成 DTO/fake/隔离 testhost，Demo 原目录不可用，沿用恢复生产基线；未触碰真实存档、媒体、云端或诊断 | [R15-04 重复通知归并定向复核](evidence/R15-04-TARGETED-RECHECK-20260924.md)、[R15-04 初始实现](evidence/R15-04-TASK-NOTIFICATION-DEDUPE-20260920.md)；下一项 R15-05 任务来源定位 |
| R15-05 | 任务来源定位 | 已满足，待环境验证 | 0d1ff346 / ba4d624b | `validate-source.py`、XAML `24/24`、`git diff --check`；Playnite `R15TaskSourceNavigationTests 3/3`；Worker 来源/查询/广播/失败路径 `21/21`；Playnite `net462` 定向构建 `0 error/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning | 任务详情来源卡片复用共享上下文按钮；游戏维持原入口，版本/媒体批次/云队列按稳定 ID 精确定位；删除或缺失对象保留诊断且不跳同名/邻近对象；clone 保留稳定来源身份 | 未验真实 Playnite/package-host、删除/重命名后的宿主呈现、UIA/读屏、IME、物理 DPI/跨屏、presented frame、ETW 或宿主性能；Demo 原目录不可用，沿用恢复生产基线；本批未触碰真实存档、媒体、云端或诊断 | [R15-05 来源定位定向复核](evidence/R15-05-TASK-SOURCE-LOCATION-RECHECK-20260923.md)、[R15-05 初始实现](evidence/R15-05-TASK-SOURCE-LOCATION-20260920.md)；下一项 `R15-06 耗时与吞吐`，先核对可靠采样和未知总量语义 |
| R15-06 | 耗时与吞吐 | 已满足，待环境验证 | 6f65638e / d9dc4317 | `validate-source.py`、XAML `24/24`、`git diff --check`；Worker 采样/查询/广播/失败链 `22/22`；Playnite 进度/快照/耗时套件 `5/5`；Playnite `net462` 定向构建 `0 error/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning | 复用现有 `StartedUtc/FinishedUtc` 耗时和 `TaskProgress`；明确总量才显示平滑速率/ETA，至少两个推进样本，10 秒停顿隐藏、15 秒无推进重置；普通阶段、等待确认、未知总量、远端 rclone/恢复写入不猜测 | Worker 全量门禁保留真实失败：`342 passed/1 skipped/1 failed/344 total`，失败为既有 `MediaSyncService.cs:570`；未验真实 Playnite/package-host、Task Center 最终呈现、DPI/UIA/IME、presented frame、ETW 或宿主性能；Demo 原目录不可用，沿用恢复生产基线；本批未触碰真实存档、媒体、云端或诊断 | [R15-06 耗时与吞吐定向复核](evidence/R15-06-TASK-THROUGHPUT-RECHECK-20260923.md)、[R15-06 初始实现](evidence/R15-06-TASK-THROUGHPUT-20260920.md)；下一项 `R15-07 失败结果复制`，先核对现有摘要、错误码、脱敏详情和剪贴板失败重试 |
| R15-07 | 失败结果复制 | 已满足，待环境验证 | 37dd4a03 / d9dc4317 | `validate-source.py`、XAML `24/24`、`git diff --check`；Playnite 失败复制/剪贴板/恢复/任务详情套件 `21/21`；Playnite `net462` 定向构建 `0 error/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning | 复用现有任务复制命令和脱敏入口；失败卡显示 240 字符以内脱敏首行摘要与错误码；技术详情默认收起，使用有限高、只读可选择 TextBox；剪贴板瞬时失败最多 4 次，失败不清除当前选中任务 | 未验真实 Playnite/package-host、系统剪贴板时序、最终呈现、物理 DPI/跨屏、UIA/读屏、IME、ETW 或宿主性能；本批使用合成 DTO/fake 剪贴板/隔离 STA，Demo 原目录不可用，沿用恢复生产基线；Worker 全量既有 `MediaSyncService.cs:570` 失败保留 | [R15-07 失败结果复制定向复核](evidence/R15-07-TASK-FAILURE-COPY-RECHECK-20260923.md)、[R15-07 初始实现](evidence/R15-07-TASK-FAILURE-COPY-20260920.md)；下一项 `R15-08 清理历史范围`，先核对运行中任务保护、恢复账本和日期/状态预览 |
| R15-08 | 清理历史范围 | 已满足 | a07f0518、88bde5de、a841e42c、77d5f346 / 569fa1d8 | Worker 清理/隔离账本回归 `17/17`；Playnite 维护页/账本契约 `6/6`；Playnite `net462` 定向构建 `0 error/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；XAML `24/24`、source/diff 门禁通过 | 复用已有 Retention Simulation、日期/原因/影响摘要和持久化隔离账本；预览句柄/过期/状态变化校验，运行中备份/恢复/媒体共享锁忙碌跳过，锁定/健康/PreRestore 保护，应用二次确认；失败账本保留恢复入口 | 未验真实 Playnite/package-host、真实清理呈现、物理 DPI/跨屏、UIA/读屏、IME、ETW 或宿主性能；Demo 原目录不可用，沿用恢复生产基线；本批使用合成/fake/隔离 SQLite/目录，没有删除真实历史；Worker 全量既有 `MediaSyncService.cs:570` 失败保留 | [R15-08 清理历史范围定向复核](evidence/R15-08-HISTORY-CLEANUP-SCOPE-RECHECK-20260923.md)、[R15-08 初始证据](evidence/R15-08-HISTORY-CLEANUP-SCOPE-20260920.md)；下一项 `R16-01 设置搜索定位`，先核对现有设置页定位能力 |
| R16-01 | 设置搜索定位 | 已满足，待环境验证 | a4e35578 / ae9aedbc | `validate-source.py`、XAML `24/24`、`git diff --check`；搜索 `1/1`、源契约 `1/1`、验证导航/草稿各独立 `1/1`；Playnite `net462` 定向构建 `0 error/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；WPF 静态沿用既有 `0/28/177` | 受控 STA WPF 实际输入搜索词，匹配字段可见且可编辑；清空恢复进入搜索前分类；搜索不改配置、不产生 pending edit；联合筛选的 Application 多实例冲突已拆分 testhost 并保留为环境边界 | 未验真实 Playnite/package-host、浅深主题最终呈现、RenderHarness presented frame、DPI/UIA/IME、ETW 或宿主性能；Demo 原目录不可用，沿用恢复生产基线；本批未触碰真实存档、媒体、云端或诊断 | [R16-01 设置搜索定位定向复核](evidence/R16-01-SETTINGS-SEARCH-RECHECK-20260923.md)、[R16-01 初始证据](evidence/R16-01-SETTINGS-SEARCH-20260920.md)；下一项 `R16-02 策略差异预览` |
| R16-02 | 策略差异预览 | 已满足，待环境验证 | b327d5ef / e63c62c7 | `validate-source.py`、XAML `24/24`、`git diff --check`；Core `BackupPolicyTemplateCatalogTests 5/5`；Playnite 策略源契约 `2/2`；Playnite `net462` 定向构建 `0 error/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；WPF 静态沿用既有 `0/28/177` | 保存前按 13 个字段展示已保存基线/显式草稿与模板覆盖差异；复制回退保留实际值；取消不发 Worker 请求；未保存游戏草稿时模板应用禁用；模板为一次性复制，不建立隐式继承 | 未验真实 Playnite/package-host、最终浅深主题呈现、DPI/UIA/IME、ETW、宿主性能和 presented frame；Demo 原目录不可用，沿用恢复生产基线；本批未触碰真实存档、媒体、云端或诊断 | [R16-02 策略差异预览定向复核](evidence/R16-02-POLICY-DIFF-RECHECK-20260923.md)、[R16-02 初始证据](evidence/R16-02-POLICY-DIFF-20260920.md)；下一项 `R16-03 模板应用范围` |
| R16-03 | 模板应用范围 | 已满足，待环境验证 | 52fbf5de / 1f968819 | `validate-source.py`、XAML `24/24`、`git diff --check`；Core 批量预览 `3/3`；Worker 策略持久化 `2/2`；Playnite 批量模板源契约 `1/1`；Playnite `net462` 定向构建 `0 error/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；WPF 静态沿用既有 `0/28/177` | 复用现有模板/策略 DTO、归一化和 Worker IPC；明确勾选目标、排除项、变更字段数、筛选后稳定 ID 保留、100 个上限、逐项失败/重试；单项仍按游戏锁和审计路径处理，取消不发送批量请求 | 未验真实 Playnite/package-host、批量点击/筛选/焦点/UIA、最终浅深主题呈现、DPI/IME/跨屏、presented frame、ETW 或宿主性能；Demo 原目录不可用，沿用恢复生产基线；本批未触碰真实存档、媒体、云端或诊断 | [R16-03 模板应用范围定向复核](evidence/R16-03-POLICY-TEMPLATE-BATCH-RECHECK-20260923.md)、[R16-03 初始证据](evidence/R16-03-POLICY-TEMPLATE-BATCH-20260920.md)；下一项 `R16-04 恢复默认粒度`，先核对设置恢复入口和敏感连接字段保护 |
| R16-04 | 恢复默认粒度 | 已满足，待环境验证 | 2b194461 / 666c60bb | 当前提交隔离 Release solution `0 errors/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；行为/取消 `2/2`、源码接线 `1/1`；`validate-source.py`、XAML `24/24`、`git diff --check`；首次旧程序集身份不一致后从当前 checkout 重建通过 | 单字段、单分类、全部默认三个范围；确认文案列出影响；全部默认只重置安全标量/界面偏好，保留 Worker/Ludusavi/Rclone、存档/媒体/镜像路径、云端目标和设备身份；同一 Playnite 草稿重绑不结束编辑，取消仍恢复原草稿 | 未验真实 Playnite/package-host、最终浅深主题呈现、DPI/UIA/IME、RenderHarness presented frame、ETW 或宿主性能；使用 D 盘隔离构建目录，Demo 原目录不可用；本批未触碰真实存档、媒体、云端或用户配置 | [R16-04 恢复默认粒度定向复核](evidence/R16-04-RESET-GRANULARITY-RECHECK-20260923.md)、[R16-04 初始实现](evidence/R16-04-RESET-GRANULARITY-20260920.md)；下一项 `R16-05 路径编辑一致`，先核对路径浏览/校验/打开/复制和权限/网络/不存在负例 |
| R16-05 | 路径编辑一致 | 已满足，待环境验证 | 955dc52e / 3121d337 | 当前提交隔离 Release solution `0 errors/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；R16-05 定向行为/源码/路径回归 `6/6`；`validate-source.py`、XAML `24/24`、`git diff --check`；WPF `0/28/162` | 六个本地工具/目录字段共用当前字段编辑模式；浏览按类型选择，单字段只读探测区分有效/缺失/网络不可达/无权限；打开只处理当前有效路径，复制复用脱敏与剪贴板重试；远端目标排除；取消浏览不改草稿 | 未验真实 Playnite/package-host、WinForms 文件夹对话框归属、Explorer 动作、最终浅深主题呈现、DPI/UIA/IME、RenderHarness presented frame、ETW 或宿主性能；未用真实网络共享/用户 ACL/剪贴板；Demo 原目录不可用，沿用恢复生产基线；本批未触碰真实存档、媒体、云端或诊断 | [R16-05 路径编辑一致定向复核](evidence/R16-05-PATH-EDITOR-RECHECK-20260923.md)、[R16-05 初始实现](evidence/R16-05-PATH-EDITOR-20260920.md)；下一项 `R16-06 生效条件说明`，先查设置字段实际消费点和保存/应用/重启边界 |
| R16-06 | 生效条件说明 | 已满足，待环境验证 | 83e7c745 / 9cef273b | 当前提交隔离 Release solution `0 errors/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；R16-06 链路/负例 `2/2` 与 R16-05 路径回归 `6/6`；`validate-source.py`、XAML `24/24`、`git diff --check`；WPF `0/28/162` | 按真实 `EndEdit → NotifyVisualSettingsChanged → settings.update → WorkerOptions.Apply/SyncPlan` 链路标注外观即时预览、下一任务/轮询边界和下一次 Playnite 启动；明确没有笼统的重启要求 | 未验真实 Playnite/package-host 的保存后呈现、Worker 重启/轮询实际时序、最终主题、DPI/UIA/IME、RenderHarness presented frame、ETW 或宿主性能；源契约测试不替代宿主交互；Demo 原目录不可用，沿用恢复生产基线；本批未触碰真实存档、媒体、云端或用户配置 | [R16-06 生效条件说明定向复核](evidence/R16-06-SETTINGS-EFFECT-CONDITIONS-RECHECK-20260923.md)、[R16-06 初始实现](evidence/R16-06-SETTINGS-EFFECT-CONDITIONS-20260920.md)；下一项 `R16-07 配置导入预览`，先查现有导入报告/版本/未知字段/凭据边界 |
| R16-07 | 配置导入预览 | 已满足，待环境验证 | 451195ad / 01e83d8a | 当前提交隔离 Release solution `0 errors/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；行为 `4/4`、源码 `1/1`、既有 portable 回归 `10/10`，合计 `15/15`；`validate-source.py`、XAML `24/24`、`git diff --check`；WPF `0/28/162` | 设置导入先展示架构版本、兼容性、差异字段和未知字段，再由原生 Yes/No 确认；取消、旧架构、坏值不改草稿；未知字段忽略，设备身份保留；应用异常回滚当前草稿 | 未验真实 Playnite/package-host 文件选择器/MessageBox/保存取消、最终呈现、DPI/UIA/IME、RenderHarness presented frame、ETW 或宿主性能；使用合成 JSON/detached settings/隔离目录，不触碰真实配置/存档/媒体/云端；Demo 原目录不可用；本批未触碰真实用户配置 | [R16-07 配置导入预览定向复核](evidence/R16-07-SETTINGS-IMPORT-PREVIEW-RECHECK-20260923.md)、[R16-07 初始实现](evidence/R16-07-SETTINGS-IMPORT-PREVIEW-20260920.md)；下一项 `R16-08 保存冲突处理`，先核对编辑基线与后台更新的字段级冲突/拒绝边界 |
| R16-08 | 保存冲突处理 | 已满足，待环境验证 | ee6b37c9 / afe80186 | 当前提交隔离 Release solution `0 errors/2 条既有 warning`；冲突行为 `3/3`、源码 `1/1`、脱离视图 WPF `1/1`、保存反馈 `2/2`、Portable `10/10`，合计 `17/17`；source、XAML `24/24`、diff；WPF `0/28/162` | 基线变化先做三方字段合并；非冲突后台字段合入，冲突字段显示明确提示、保留草稿并阻断保存；取消基线转到最新外部值 | 未验真实 Playnite 双设置窗口/后台共享对象竞态/宿主错误呈现、最终呈现、DPI/UIA/IME、RenderHarness presented frame、ETW 或宿主性能；合成 detached/fake/隔离目录，不触碰真实配置/存档/媒体/云端；Demo 原目录不可用 | [R16-08 保存冲突处理定向复核](evidence/R16-08-SETTINGS-CONFLICT-RECHECK-20260923.md)；下一项 `R17-01 健康结果分层`，先核对健康结果、已解决项与跨来源去重时间证据 |
| R17-01 | 健康结果分层 | 已满足，待环境验证 | eb033251 / 3f9d8e2b | 当前提交隔离 Release solution `0 errors/2 条既有 warning`；Playnite `5/5`、Worker `2/2`，合计 `7/7`；source、XAML `24/24`、diff；WPF `0/28/162` | 复用 `resolved=0` 开放队列和健康巡检 resolve；补证据时间；维护页按需立即处理/建议处理/信息项分层，按游戏+稳定代码+问题标题去重，错误优先；原问题表、选择、详情、复制/导航和滚动保留 | 未验真实 Playnite/package-host、多来源生产标题规范、最终呈现、DPI/UIA/IME、ETW 或宿主性能；合成 DTO、fake/隔离 SQLite；Demo 原目录不可用 | [R17-01 健康结果分层定向复核](evidence/R17-01-HEALTH-RESULT-LAYERS-RECHECK-20260923.md)；下一项 `R17-02 诊断包预览`，先核对类别预览、脱敏范围和生成后大小/位置 |
| R17-02 | 诊断包预览 | 已满足，待环境验证 | 2b6e9051 / 837528bc | 当前提交隔离 Release solution `0 errors/2 条既有 warning`；Playnite R17 `7/7`；Worker 诊断预览与既有 ZIP `2/2`、R17-01 SQLite 回归 `2/2`，相关合计 `11/11`；source、XAML `24/24`、diff；WPF `0/28/162` | 复用既有有限诊断 ZIP、统一脱敏和结果路径/大小；生成前列明类别、可选日志、脱敏范围、上限与明确排除项；取消确认不生成，确认后沿用原创建/打开路径 | 未验真实 Playnite/package-host 确认框、Explorer/权限、日志并发、最终呈现、DPI/UIA/IME、ETW 或宿主性能；`database.json` 仅摘要不等于真实 DB；合成请求、fake/隔离 SQLite/临时目录；Demo 原目录不可用 | [R17-02 诊断包预览定向复核](evidence/R17-02-DIAGNOSTICS-PACKAGE-PREVIEW-RECHECK-20260923.md)；下一项 `R17-03 检查进度预算`，先核对巡检范围、暂停原因、最近成功/下轮计划和取消结束状态 |
| R17-03 | 检查进度预算 | 已满足，待环境验证 | 87473bc3 / e0b5cd15 | 当前提交隔离 Release solution `0 errors/2 条既有 warning`；Worker `12/12`、Playnite R17 `10/10`（其中 R17-03 `3/3`）；source、XAML `24/24`、diff；WPF `0/28/162` | 复用既有游标/预算/会话与操作锁/延后表；摘要记录索引总数、需检查数、延后数、候选数和未读取归档；游戏运行/锁占用/全候选延后给出原因；取消/预算/异常不伪装整库完成；维护页显示当前候选、最近完成/成功和下轮计划 | 未验真实 Playnite/package-host、最终浅深主题、DPI/UIA/IME、焦点/滚动、presented frame、ETW、宿主性能和真实游戏/锁/超时竞态；只用合成/fake/隔离 SQLite/目录；Demo 原目录不可用 | [R17-03 检查进度预算定向复核](evidence/R17-03-INSPECTION-PROGRESS-BUDGET-RECHECK-20260923.md)；下一项 `R17-04 保留预览对比`，先核对保留候选、保护项、隔离账本和执行前预览过期边界 |
> 2026-09-23 R17-04 保留预览对比定向复核：本批没有新增生产代码，在 `503c2119` 上复用 `3c73b498` 的保留预览、保护计数、隔离账本、Apply 指纹/时效/操作锁和删除失败恢复。Worker `RetentionSimulationServiceTests 12/12`；Playnite R17 合并回归 `11/11`（含保留源码/绑定 `1/1`），布局 `20 passed/11 skipped`；当前提交隔离 Release solution `0 errors/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；`validate-source.py`、XAML `24/24`、diff check、WPF `0/28/162` 通过。未验真实 Playnite/package-host、Explorer/权限、真实锁/文件故障/重启恢复、最终呈现、DPI/UIA/IME、presented frame、ETW 或宿主性能；合成策略/fake/隔离 SQLite trigger/目录，Demo 原目录不可用。详见 [R17-04 保留预览对比定向复核](evidence/R17-04-RETENTION-PREVIEW-RECHECK-20260923.md)。下一项推进 R17-05。

> 2026-09-23 R17-05 隔离账本入口定向复核：本批没有新增生产代码，在 `d01ab7ea` 上复用 `3002a8dc` 的分页隔离账本、原/隔离路径展示、完整 Tooltip、EntryId 定向恢复和冲突停止。Worker `RetentionQuarantineRecoveryTests 5/5`；Playnite R17 合并 `12/12`，含隔离账本源码/绑定 `2/2`；当前提交隔离 Release solution `0 errors/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；`validate-source.py`、XAML `24/24`、diff check、WPF `0/28/162` 通过。未验真实 Playnite/package-host、Explorer/权限、重启恢复、最终呈现、DPI/UIA/IME、presented frame、ETW 或宿主性能；合成 DTO/fake IPC/隔离 SQLite/临时目录，Demo 原目录不可用。详见 [R17-05 隔离账本入口定向复核](evidence/R17-05-QUARANTINE-LEDGER-RECHECK-20260923.md)。下一项推进 R17-06。

> 2026-09-23 R17-06 存储分析导航定向复核：本批没有新增生产代码，在 `1077a7ee` 上复用 `51cae6b9` 的存储分析、稳定游戏/版本导航和缺失目标负例；按当前源码更正旧证据数量，Worker `StorageAnalysisServiceTests 4/4`、Playnite R17-06 `3/3`、完整 R17 `15/15`。当前提交隔离 Release solution `0 errors/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；`validate-source.py`、XAML `24/24`、diff check、WPF `0/28/162` 通过。未验真实 Playnite/package-host、Explorer/权限、真实文件系统占用时序、最终呈现、DPI/UIA/IME、presented frame、ETW 或宿主性能；合成 DTO/fake/隔离 SQLite/目录，Demo 原目录不可用。详见 [R17-06 存储分析导航定向复核](evidence/R17-06-STORAGE-ANALYSIS-NAVIGATION-RECHECK-20260923.md)。下一项推进 R17-07。

> 2026-09-23 R17-07 检查项一键定位定向复核：本批没有新增生产代码，在 `280c839e` 上复用 `e8d581c6` 的稳定 Finding/Health/Task 来源、健康诊断 `PlayniteId + BackupId` 精确路由、缺失版本负例和维护返回栈；Worker 迁移/健康/Finding `18/18`，Playnite `FindingNavigationResolverTests 11/11`、完整 R17 `15/15`。当前提交隔离 Release solution `0 errors/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；`validate-source.py`、XAML `24/24`、diff check、WPF `0/28/162` 通过。未验真实 Playnite/package-host、Explorer/权限、最终呈现、DPI/UIA/IME、焦点/滚动、presented frame、ETW 或宿主性能；合成 DTO/fake/隔离 SQLite/临时目录，Demo 原目录不可用。详见 [R17-07 检查项一键定位定向复核](evidence/R17-07-FINDING-NAVIGATION-RECHECK-20260923.md)。下一项推进 R17-08。

> 2026-09-23 R17-08 维护报告可读性定向复核：本批没有新增生产代码，在 `fc8dffa5` 上复用 `59d4190b` 的结构化报告、统一生成时间、身份透传和脱敏器；按当前源码校正数量，Worker `MaintenanceReportServiceTests 3/3`，Playnite `MaintenanceReportSourceTests 4/4`（报告 IPC/复制/导出 `2/2`，同类维护回归 `2/2`），完整 R17 `15/15`。当前提交隔离 Release solution `0 errors/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；`validate-source.py`、XAML `24/24`、diff check、WPF `0/28/162` 通过。未验真实 Playnite/package-host、实际导出目录/剪贴板/对话框、最终呈现、DPI/UIA/IME、屏幕阅读器、presented frame、ETW 或宿主性能；合成 DTO/fake/隔离 SQLite/临时目录，Demo 原目录不可用。详见 [R17-08 维护报告可读性定向复核](evidence/R17-08-MAINTENANCE-REPORT-RECHECK-20260923.md)。下一项推进 R18-01。

> 2026-09-23 R18-01 连续输入基准定向复核：本批没有新增生产代码，在 `0173364e` 上复用 `10bc5789` 的 GamePicker 本地过滤、20ms 防抖、IME 已提交查询和内部性能诊断；`R18ContinuousInputBenchmarkTests 1/1`，当前 `GamePicker|DebouncedRefreshTests` 宽筛选 `50/50`（包含已存在的 R18-02 受控窗口类及其他相邻回归，R18-02 不因宽筛选提前签收）。本次 2,000/10,000 项各 30 次样本 p95/max 为 `2.350/3.825 ms`、`7.297/9.657 ms`，过滤评估 `60,000/300,000`，托管堆增量代理峰值 `303,024/1,448,112 bytes`；隔离 Release `0 errors/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；源校验、XAML `24/24`、diff、WPF `0/28/162` 通过。未验真实 Playnite/package-host、Windows IME 候选 UI、最终 presented frame、物理 DPI/跨屏、UIA/读屏、ETW 或宿主性能；合成内存列表，Demo 原目录不可用。详见 [R18-01 连续输入基准定向复核](evidence/R18-01-CONTINUOUS-INPUT-RECHECK-20260923.md)。下一项推进 R18-02。

> 2026-09-23 R18-02 真实 Dispatcher 基准定向复核：本批没有新增生产代码，在 `23d70d65` 上复用 `59468b37` 的 STA WPF 受控窗口两段测量和 `5b28b0c3` 的来源标注；`R18DispatcherVisibilityBenchmarkTests 1/1`。本次 20 次样本 VM p95/最大 `38.259/64.967 ms`，VM→可见容器增量 p95/最大 `29.245/37.615 ms`，可见计数 `20/20=1`，容器 `476×19.24 DIP`；当前宽筛选 `GamePicker|DebouncedRefreshTests 50/50`，不将宽筛选数量提前签收其他 R18 项。隔离 Release `0 errors/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；源校验、XAML `24/24`、diff、WPF `0/28/162` 通过。未验真实 Playnite/package-host、DWM presented frame、60fps、物理 DPI/跨屏、UIA/读屏、真实 IME、ETW 或宿主性能；受控 Window/合成 DTO/隔离 STA，Demo 原目录不可用。详见 [R18-02 真实 Dispatcher 基准定向复核](evidence/R18-02-DISPATCHER-VISIBILITY-RECHECK-20260923.md)。下一项推进 R18-03。

> 2026-09-23 R18-03 缩略图滚动预算定向复核：本批没有新增生产代码，在 `93b115f4` 上复用 `e54d514e`/`18c5073f` 的 3 路后台解码、96 项 LRU、取消和 generation 迟到结果保护；`R18ThumbnailBudgetTests 1/1`、`AsyncThumbnailLoaderTests + AsyncThumbnailImageTests 8/8`，合计 `9/9`。本次 120 请求/解码开始/成功均为 `120/120/120`，峰值活动 `3`，每轮归零，缓存 `12…96` 封顶 `96/96`，托管堆代理峰值 `112,456 bytes`，预取消 `1`，迟到失败替换后最终 `Ready`；隔离 Release `0 errors/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；源校验、XAML `24/24`、diff、WPF `0/28/162` 通过。未验真实 Playnite/package-host 快速滚动、真实媒体尺寸、DPI/UIA/IME、显存、presented frame、ETW 或宿主性能；合成 PNG/隔离目录/受控 STA，Demo 原目录不可用。详见 [R18-03 缩略图滚动预算定向复核](evidence/R18-03-THUMBNAIL-BUDGET-RECHECK-20260923.md)。下一项推进 R18-04。

| R17-04 | 保留预览对比 | 已满足，待环境验证 | 3c73b498 / 503c2119 | 当前提交隔离 Release solution `0 errors/2 条既有 warning`；Worker Retention `12/12`；Playnite R17 `11/11`（含保留源码/绑定 `1/1`）；布局 `20 passed/11 skipped`；source、XAML `24/24`、diff；WPF `0/28/162` | 既有预览展示候选/保护项/预计释放/隔离占用；Apply 校验句柄、十分钟时效、策略/候选/归档指纹并执行前重算；SQLite 删除失败负例证明恢复原路径、保留账本且 `MovedBytes/FreedBytes` 不计真实释放 | 未验真实 Playnite/package-host、最终浅深主题、DPI/UIA/IME、焦点/滚动、Explorer/权限、真实锁/文件故障/重启恢复、presented frame、ETW 或宿主性能；只用合成/fake/隔离 SQLite/目录；Demo 原目录不可用 | [R17-04 保留预览对比定向复核](evidence/R17-04-RETENTION-PREVIEW-RECHECK-20260923.md)；下一项 `R17-05 隔离账本入口`，先核对分页隔离列表、原/隔离路径、状态和受控恢复入口 |
| R17-05 | 隔离账本入口 | 已满足，待环境验证 | 3002a8dc / d01ab7ea | 当前提交隔离 Release solution `0 errors/2 条既有 warning`；Worker 隔离账本 `5/5`；Playnite R17 `12/12`（隔离账本源码/绑定 `2/2`）；source、XAML `24/24`、diff；WPF `0/28/162` | 维护行动项显式显示原路径、隔离路径和状态；路径只对隔离账本项展开并支持完整 Tooltip；“受控恢复”沿用逐条确认、EntryId 定向 IPC 和冲突停止，不默认删除残留 | 未验真实 Playnite/package-host、最终主题/DPI/UIA/IME/焦点滚动、Explorer/权限、重启恢复时序、presented frame、ETW 或宿主性能；合成/fake/隔离 SQLite/临时目录；Demo 原目录不可用 | [R17-05 隔离账本入口定向复核](evidence/R17-05-QUARANTINE-LEDGER-RECHECK-20260923.md)；下一项 `R17-06 存储分析导航`，先核对已有存储统计、来源记录和维护页跳转能力 |
| R17-06 | 存储分析导航 | 已满足，待环境验证 | 51cae6b9 / 1077a7ee | Worker 存储分析 `4/4`；Playnite R17-06 `3/3`、完整 R17 `15/15`；隔离 Release solution `0 errors/2 条既有 warning`；`validate-source.py`、XAML `24/24`、`git diff --check`；WPF `0/28/162` | Demo 卡片明确分离 SQLite 逻辑索引体积、备份目录文件实测与卷剩余；失联/空路径单独提示，不把失联体积按 0；TopGames 按稳定 `PlayniteId`/最新 `BackupId` 精确打开游戏或版本，缺失目标不回退，返回维护中心复用已有导航 | 未验真实 Playnite/package-host、最终主题/DPI/UIA/IME/焦点滚动、Explorer/权限、真实文件系统占用时序、presented frame、ETW 或宿主性能；合成/fake/隔离 SQLite/目录；Demo 原目录不可用，main 用户改动和 src.zip 未碰、未合并 | [R17-06 存储分析导航定向复核](evidence/R17-06-STORAGE-ANALYSIS-NAVIGATION-RECHECK-20260923.md)；下一项 `R17-07 检查项一键定位`，先核对稳定 Finding/Health/Task 来源和返回状态 |
| R17-07 | 检查项一键定位 | 已满足，待环境验证 | e8d581c6 / 280c839e | Worker 迁移/健康/Finding `18/18`；Playnite `FindingNavigationResolverTests 11/11`、完整 R17 `15/15`；隔离 Release solution `0 errors/2 条既有 warning`；`validate-source.py`、XAML `24/24`、`git diff --check`；WPF `0/28/162` | 复用既有 Finding/Health/Task 入口和维护返回栈；健康诊断新增 `PlayniteId + BackupId` 精确版本路由，历史标题前缀兼容；目标缺失保留诊断、不选择邻居，无版本身份不回落到失败任务；维护筛选、选中项和滚动恢复保持 | 未验真实 Playnite/package-host、最终主题/DPI/UIA/IME/焦点滚动、Explorer/权限、重启时序、presented frame、ETW 或宿主性能；只用合成/fake/隔离 SQLite/临时目录；Demo 原目录不可用 | [R17-07 检查项一键定位定向复核](evidence/R17-07-FINDING-NAVIGATION-RECHECK-20260923.md)；下一项 `R17-08 维护报告可读性`，先核对报告 DTO/导出服务、分组和脱敏器 |
| R17-08 | 维护报告可读性 | 已满足，待环境验证 | 59d4190b / fc8dffa5 | Worker `MaintenanceReportServiceTests 3/3`；Playnite `MaintenanceReportSourceTests 4/4`（报告 IPC/复制/导出 `2/2`，同类维护回归 `2/2`）；完整 R17 `15/15`；隔离 Release solution `0 errors/2 条既有 warning`；`validate-source.py`、XAML `24/24`、`git diff --check`；WPF `0/28/162` | 复用原报告采集、IPC、复制/导出；按软件身份、摘要、待处理、已验证、未知组织；统一生成时间与计数；脱敏 URL 参数/凭据及 Windows 用户路径，保留安全主体；无条目明确说明采集范围 | 未验真实 Playnite/package-host、实际导出目录/剪贴板/保存对话框、最终主题/DPI/UIA/IME/读屏/物理跨屏、presented frame、真实权限、ETW 或宿主性能；只用合成/fake/隔离 SQLite/临时目录；Demo 原目录不可用 | [R17-08 维护报告可读性定向复核](evidence/R17-08-MAINTENANCE-REPORT-RECHECK-20260923.md)；下一项 `R18-01 连续输入基准`，先核对游戏选框搜索、DebouncedRefresh、IME 和大库基准 |
| R18-01 | 连续输入基准 | 已满足，待环境验证 | 10bc5789 / 0173364e | `R18ContinuousInputBenchmarkTests 1/1`；当前 `GamePicker\|DebouncedRefreshTests` 宽筛选 `50/50`（含 R18-02 受控窗口类及相邻回归）；2,000/10,000 项 p95/max `2.350/3.825 ms`、`7.297/9.657 ms`；过滤评估 `60,000/300,000`、托管堆增量代理峰值 `303,024/1,448,112 bytes`；Release 隔离 solution `0 errors/2` 条既有 warning；source、XAML `24/24`、diff；WPF `0/28/162` | 复用现有游戏选框本地缓存、取消与同步过滤；两档各 30 次连续英文输入，粘贴/删除/已提交中文查询按结果断言，连续快速输入最终只刷新 1 次；现有 STA WPF composition/Enter 正负例仍保留；本阶段无 XAML/视觉资源改动 | 仅受控 net472 testhost/合成内存列表；`GC.GetTotalMemory(false)` 差值为可复算托管堆代理，不替代 ETW/真实分配栈、presented frame 或宿主性能；未验真实 Playnite/package-host、Windows IME、DPI/UIA/读屏、物理跨屏；Demo 原目录不可用 | [R18-01 连续输入基准定向复核](evidence/R18-01-CONTINUOUS-INPUT-RECHECK-20260923.md)；下一项 `R18-02 真实 Dispatcher 基准`，区分 VM 完成与窗口可见反馈 |
| R18-02 | 真实 Dispatcher 基准 | 已满足，待环境验证 | 59468b37 / 5b28b0c3 / 23d70d65 | `R18DispatcherVisibilityBenchmarkTests 1/1`；当前 `GamePicker\|DebouncedRefreshTests` 宽筛选 `50/50`（含 R18-01、R18-02 及相邻回归）；20 次 VM p95/最大 `38.259/64.967 ms`、VM→可见容器增量 `29.245/37.615 ms`；可见计数 `20/20=1`、容器 `476×19.24 DIP`；Release 隔离 solution `0 errors/2` 条既有 warning；`validate-source.py`、XAML `24/24`、`git diff --check`；WPF `0/28/162` | 实际 STA WPF `Window` 中分离 SearchText→VM `RefreshCount + LastSearchText` 和 VM→可见 `ListBoxItem` 容器增量；不以 `FilteredCount` 代替画面延迟，来源标签已校正；20 次原始时间戳与容器几何保留 | 受控 `DispatcherSynchronizationContext`、真实 `Window.Show()`/`IsVisible` 与布局容器检查；不等于 presented frame、DWM/60fps、Playnite/package-host、物理 DPI/跨屏、UIA/读屏、真实 IME、ETW 或宿主性能；Demo 原目录不可用 | [R18-02 真实 Dispatcher 基准定向复核](evidence/R18-02-DISPATCHER-VISIBILITY-RECHECK-20260923.md)；下一项 `R18-03 缩略图滚动预算`，先查活动请求、取消、缓存上限和迟到结果保护 |
| R18-03 | 缩略图滚动预算 | 已满足，待环境验证 | e54d514e / 18c5073f / 93b115f4 | `R18ThumbnailBudgetTests 1/1`；`AsyncThumbnailLoaderTests + AsyncThumbnailImageTests 8/8`，合计 `9/9`；Release 隔离 solution `0 errors/2` 条既有 warning；`validate-source.py`、XAML `24/24`、`git diff --check`；WPF `0/28/162` | 复用现有 3 路后台解码、96 项 LRU、不可见/卸载取消和 generation 双侧迟到保护；120 个合成请求/10 窗口实测峰值活动 `3`、活动逐轮归零、缓存 `96/96`、托管堆代理峰值 `112,456 bytes`、预取消 `1`；实际 STA 替换旧 Missing 后最终 Ready 且无 Missing/Failed；当前 `PreviewWidth=96` 的 `96×96` 尺寸断言 | 合成 PNG、隔离目录、真实生产 loader/control、STA WPF 窗口；托管堆为 `GetTotalMemory(false)` 代理，不等于显存/ETW/presented frame；未验真实 Playnite 快速滚动、DPI/UIA/IME、宿主性能；Demo 原目录不可用 | [R18-03 缩略图滚动预算定向复核](evidence/R18-03-THUMBNAIL-BUDGET-RECHECK-20260923.md)；下一项 `R18-04 表格容器预算`，在 2k/10k/20k 数据下记录虚拟化容器上限 |
| R18-04 | 表格容器预算 | 已满足，待宿主环境验证 | 64843642 / f55dce61 / 922501e7 / d752424e / `3a1dadd8` / 当前复测 `cdd28fd2` | 当前 main Release/XAML `24/24`、0 errors/2 既有 CS8602 warnings；专测 `1/1`、四个关联行为类 `23/23`（6+10+3+4），0 fail/skip、均 exit `0` | 当前固定窗口复测 Task 三档均 `9/7`；Media Inbox 固定 `1280×720 DIP` 为 `7/7/7`，UI cache `2,000`；代理滚动最大 Task `80.839/33.607/28.492 ms`、Media `0.040/0.052/0.025 ms`。用户另报更大窗口下 Media `14` 行、约 `0.03 ms`，没有原始 TRX/尺寸，单独标记未复现摘要 | 保留 Task Recycling 与 Media Standard/Item/禁列虚拟化例外；23 项精确方法/理论参数见新证据，覆盖 10 项纯逻辑、7 项源码契约、6 项隔离 STA WPF 行为；不是宿主交互。当前 R18 专测 TRX 出现 6 次、Anchor TRX 出现 2 次 `InvalidComObjectException` 清理噪声，xUnit/VSTest 都明确通过、根因未知。隔离 STA/DIP/Stopwatch 不等于 Playnite、物理 DPI/UIA/IME、呈现帧、ETW 或宿主性能 | [R18-04 当前 main cdd28fd2 复测与 23 项精确组成](evidence/R18-04-CURRENT-MAIN-RECHECK-20260924-CDD28FD2.md)及五份 TRX；旧身份详细采样见[前次复核](evidence/R18-04-CURRENT-MAIN-RECHECK-20260924.md)；下一项处理用户报告的排序崩溃和两处表格布局，再继续依赖满足的 Q/R；R23-04/05 外部门禁保留 |
| R18-05 | 后台事件合并 | 已满足，待环境验证 | 91947336 | `TaskEventUiBatcher 3/3`；Worker `TaskEventBroadcaster 5/5`；相关 Playnite `19/19`；`validate-source.py`、XAML `24/24`、`git diff --check`；Release `0 errors/2 existing MediaCenter nullable warnings` | Playnite pending `128`、batch `32`，TaskId 合并进度；终态 DataBind 优先；Worker 固定 `128` 容量优先淘汰非终态；卸载释放批处理器；相关任务进度/时间线/索引/通知行为通过 | 受控 fake Dispatcher、合成事件、Worker Channel、隔离 testhost；终态压力样本保留 Failed；durable change feed/快照为断线和极端终态压力的恢复来源；不等于 presented frame、真实 Playnite、DPI/UIA/读屏、ETW 或宿主性能；Demo 原目录不可用；`.tmp/r18-05-solution` 已清理 | [R18-05 后台事件合并证据](evidence/R18-05-BACKGROUND-EVENT-BATCHING-20260920.md)；下一项 R18-06 页面重访成本，复用生命周期、generation、取消和缓存快照 |
| R18-06 | 页面重访成本 | 已满足，待环境验证 | 1b19fd4c | `WorkspaceRevisitLoadGate`/source `7/7`；页面生命周期、状态 presenter、请求协调、忙状态相邻回归 `31/31`；`validate-source.py`、XAML `24/24`、`git diff --check`；Release `0 errors/2 existing MediaCenter nullable warnings` | 复用六页 registry、同页 `PageHost.Content` 实例和既有页面级读取；15 秒热态门按 workspace/稳定游戏/Media 筛选上下文隔离；失败、取消、卸载失效和晚返回不记新鲜度；壳层记录 binding、首次/重访 attach/reuse/layout，VM 记录 read/skip/outcome/load；显式 `LoadDetails` 继续强制读取 | 仅合成时间/context、source/fake、隔离 net472/WPF testhost；日志覆盖生产同步委托/布局路径，不等于真实 Playnite 首次/重访样本、presented frame、DPI/UIA/读屏、ETW 或宿主性能；Demo 原目录不可用，main 用户改动和 `src.zip` 未碰、未合并；`.tmp/r18-06-*` 已清理 | [R18-06 页面重访成本](evidence/R18-06-WORKSPACE-REVISIT-20260920.md)；下一项 `R18-07 长时资源曲线`，先核对既有耐久脚本和句柄/订阅/时钟/工作集/缓存指标 |
| R18-07 | 长时资源曲线 | 已满足，待环境验证 | 56d8e1b7 / c5c33e18 | 原始序列 `177` 样本；1800 秒操作 + 30 秒静置，`2769` 周期、`8537` 动作、`0` 失败、`enduranceprobe OK`；相关源测试 `31/31`、Release `0 errors/2 existing warnings`、source/diff 通过 | 隔离 STA WPF Window、Light/Dark、六工作区；停止输入后周期稳定、`timers=0`，私有字节/工作集回落并尾段稳定；不代替真实屏幕/宿主呈现 | handles `1170–1547`、threads `22–42`、subscriptions `1`、animated owners `0`、thumbnail cache `0/96`；资源指标是受控窗口观察，非无界泄漏证明；真实 Playnite 长时/ETW 未验 | [R18-07 长时资源曲线证据](evidence/R18-07-LONG-RUN-RESOURCE-CURVE-20260920.md)；下一项 R18-08 低性能降级触发 |
| R18-08 | 低性能降级触发 | 已满足，待环境验证 | 3bfe3d3c | clean-tree lowcostprobe `24/24`；直接相关回归 `37/37`；Release `0 errors/2 existing warnings`；source/diff 通过；联合 R14 筛选另有 `2` 条既有旧断言失败，未写成全绿 | 显式模拟 `glass=false/motion=false` 下六工作区、双主题、1040×700/1600×900：`visibleEffects=0`、文本 `4–147`、非输入框 `unexpectedOverflow=0`；代表图已查看 | 真实 Material null/opaque fallback、PopupAnimation None、有限列表和横向边界通过；未验硬件 Render Tier、真实低 Tier、Playnite host、DPI/UIA/ETW/屏幕帧 | [R18-08 低性能回退证据](evidence/R18-08-LOW-PERFORMANCE-FALLBACK-20260920.md)；第三轮 R18 组完成，下一项 R19-01 异步竞态与故障恢复 |
| R19-01 | 旧请求晚返回 | 已满足，待环境验证 | ed107c50 | `30 passed / 1 skipped / 31 total`；clean-tree Release `0 errors/2 existing MediaCenter nullable warnings`；`validate-source.py`、XAML `24/24`、`git diff --check` | 详情请求绑定启动 workspace、generation、game ID；工作区切换立即失效旧详情/媒体代际；A 慢成功不能覆盖 B 新失败，标题/数据/选择/更新时间同属 B；保留游戏选框、滚动条、命令绑定、取消/错误/恢复和 net462 | 合成 surface、fake/request coordinator、MediaWorkspaceStateCache、隔离 net472/WPF testhost；未验真实 Playnite IPC 延迟与跨线程宿主时序、最终 presented frame、DPI/UIA/读屏、ETW/宿主性能；Demo 原目录不可用，main 用户改动未碰未合并 | [R19-01 旧请求晚返回证据](evidence/R19-01-LATE-REQUEST-CONTEXT-20260920.md)；下一项 `R19-02` 刷新失败保留草稿，先核对只读刷新与编辑对象分离和未保存字段保护 |
| R19-02 | 刷新失败保留草稿 | 已满足，待环境验证 | 7b2d3afa | 生产 VM 编辑同步行为 `2/2`；R11 备注/工作区状态相邻回归 `14 passed / 1 skipped / 15 total`；clean-tree Release `0 errors/2 existing MediaCenter nullable warnings`；source/XAML/diff 通过 | 复用 dirty 标记与稳定 ID：失败不清集合/草稿；同 ID 成功只覆盖干净字段，用户备注/锁定/收藏草稿保留；取消命令仍恢复服务端原值；未新增第二套 Draft 服务 | 合成 DTO、隔离生产 VM、R11 WPF/testhost；未验真实 Playnite/Worker 断连和慢刷新呈现时序、最终 presented frame、DPI/UIA/读屏、ETW/宿主性能；Demo 原目录不可用，main 用户改动未碰未合并 | [R19-02 刷新失败保留草稿证据](evidence/R19-02-DRAFT-REFRESH-20260920.md)；下一项 `R19-03` 重复执行幂等，先核对 request ID、重试恢复和未知写结果边界 |
| R19-03 | 重复执行幂等 | 已满足，待环境验证 | f7bc3bf0 | Worker ledger/消息边界 `16/16`；生产 Busy 原子门与按钮 `2/2`；XAML `24/24`；`validate-source.py`、`git diff --check` 通过；未新增写服务 | 已有 RequestId、replay-protected 分类、durable ledger、同 envelope 复核与 `REQUEST_IN_PROGRESS`/`REQUEST_INTERRUPTED` 明确结果；第二次 UI 触发不执行 | 合成请求、fake/隔离 testhost、本地已有 Release 产物；Named Pipe 客户端因环境禁止创建跳过；linked `_wpftmp.csproj` Access denied，外部副本 restore `NU1301`；真实 Playnite/Worker 管道、presented frame、DPI/UIA/IME、ETW/宿主性能未验；Demo 原目录不可用 | [R19-03 重复执行幂等证据](evidence/R19-03-IDEMPOTENCY-20260920.md)；下一项 `R19-04` 取消关闭顺序，先核对关闭/取消/Worker 断开和重开恢复的确定性清理 |
| R19-04 | 取消关闭顺序 | 已满足，待环境验证 | e856938a | `LatestRequestCoordinator` + Busy `5/5`；`TaskReconcileService` `1/1`；取消/重启相邻 Worker `13 passed / 1 skipped / 14 total`；XAML `24/24`、source/diff 通过；本项无生产代码变更 | Dashboard 卸载取消和 generation、插件 lifetime/owned Worker 停止、Worker durable reconcile 已有确定顺序；迟到回写拒绝、Busy 恢复、任务终态保留和重复协调均有实际行为证据 | 合成/fake/隔离 SQLite/testhost；独立 Worker 硬重启因 Named Pipe 权限跳过；WPF shutdown 源测试因旧 net472 产物缺 `GscBuildCommit` 被身份校验拦截；真实 Playnite 关闭/重开/同刻断管、presented frame、DPI/UIA/IME、ETW/宿主性能未验；Demo 原目录不可用 | [R19-04 取消关闭顺序证据](evidence/R19-04-CLOSE-CANCEL-ORDER-20260920.md)；下一项 `R19-05` Worker 重启恢复，先在可用 Named Pipe/Worker 进程环境复跑硬重启、进度订阅和重连去重 |
| R19-05 | Worker 重启恢复 | 已满足，待环境验证 | a4b23ccb | Worker event/reconcile/cloud `18/18`；Playnite `TaskEventUiBatcher 3/3`；XAML `24/24`、source/diff 通过；无生产代码变更 | Worker 启动 reconcile、中断任务真实状态、整库/云队列恢复、事件 pipe 每连接单订阅、指数退避和 durable change feed/SQLite 修复均已有实现 | 合成事件、隔离 SQLite/fake/testhost；独立 Worker 硬重启因 Named Pipe 权限 `1 skipped`；相邻 subscription 一条因旧 net472 产物缺 `GscBuildCommit` 被身份门拦截；真实 Worker/Playnite 重启/断管/重开、presented frame、DPI/UIA/IME、ETW/宿主性能未验；Demo 原目录不可用 | [R19-05 Worker 重启恢复证据](evidence/R19-05-WORKER-RESTART-RECOVERY-20260920.md)；下一项 `R19-06` 分页快照变化，先核对 durable cursor、末页变化和稳定选择 ID |
| R19-06 | 分页快照变化 | 已满足，待环境验证 | 6d1a401b | 当前 main/test identity `9b3dd2f1` clean Release：Worker 查询 `13/13`、Playnite 分页/索引 `10/10`、选择锚点 `4/4`，合计 `27/27`；相邻 R06 排序/源码身份 `5/5`；XAML `24/24`、0 errors/2 条既有 `CS8602`；`validate-source.py` 与 `git diff --check` | Worker 任务 `(created_utc, task_id)`、媒体 `(captured_utc, media_id)` 稳定游标和 limit+1 末页策略；Playnite reset/generation/当前 cursor 明确快照重载；MediaPageAccumulator 按 `MediaId` 去重更新、窗口 `2000`、稳定选择 ID；任务按 `TaskId` 合并。后续耗时字段映射已单独复核，不改变游标语义 | 仅合成/fake/隔离 SQLite/testhost；当前 fresh build 复跑使旧 `GscBuildCommit` 产物失败不再适用。真实并发新增/删除/排序时序、Playnite/package-host、presented frame、DPI/UIA/IME、ETW、宿主性能未验；Demo 原目录不可用；本阶段无生产代码变更 | [R19-06 分页快照变化](evidence/R19-06-PAGED-SNAPSHOT-20260920.md)及 Worker/Playnite/selection/sorting 四份当前 TRX；下一项 `R19-07` 外部文件变化，先核对移动/占用/损坏回退和重新定位 |
| R19-07 | 外部文件变化 | 已满足，待环境验证 | 9760c648 | 当前 main Release：XAML `24/24`、0 errors/2 既有 CS8602；Worker `15/15`、移动缩略图/恢复 `2/2`、相邻媒体/恢复行为 `16/16`；四页用户布局双主题 `8/8`；source/diff 通过 | 复用既有媒体 Missing/Failed/刷新、generation 与取消保护、父路径回退、隔离备份重检和 BackupId 诊断；锁定归档与媒体移动/恢复补了实际行为负例 | 全部为合成文件/隔离目录/STA WPF、逻辑 DIP；无真实 Playnite 外部移动/占用/损坏时序、Explorer/播放器/呈现帧/DPI/UIA/IME/ETW/宿主性能。设置截图对应的实际加载 DLL 未绑定，记录到的本机旧 identity 早于 `3a1dadd8`；CEF `platform_channel 0x5` 阻挡宿主复核。不改真实存档/媒体/云端/诊断；本阶段无生产代码变更 | [R19-07 外部文件变化](evidence/R19-07-EXTERNAL-FILE-CHANGE-20260920.md)；四页几何见 [用户布局复核](evidence/USER-REPORTED-LAYOUT-20260923.md)；下一项 `R19-08` 慢调用可取消，先核对取消/超时/UI 解锁和未知写结果 |
| R19-08 | 慢调用可取消 | 已满足，待环境验证 | 6618de22 | 当前 main Release：XAML `24/24`、0 errors/2 既有 CS8602；Worker 进程/云队列/ledger/Rclone `37/37`，IPC 内存流边界 `10/10`；Busy、旧请求协调、取消反馈 `12/12`；Named Pipe 客户端 `1 passed / 6 skipped / 0 failed`；source/diff 通过 | 复用 `ExternalProcessRunner` 的 timeout/token 与稳定错误码；云端状态按实际 `ProcessResult` 变化；RequestId ledger 对中断写保留 Interrupted，客户端取消只同 envelope 有限复核；Busy finally 释放；本次新增的是当前 main 证据，不改生产服务 | 本地 ping loopback/PowerShell 进程、fake/隔离 SQLite/内存流/STA WPF；6 个 Named Pipe 客户端时序因明确权限探测跳过，未绕过。真实 Playnite/Worker pipe、Ludusavi/Rclone、网络延迟/远端云写、presented frame、DPI/UIA/IME/ETW/宿主性能未验；设置截图实际加载包/host 仍未绑定；本项无生产代码改动 | [R19-08 慢调用可取消](evidence/R19-08-CANCELLABLE-SLOW-CALLS-20260920.md)；下一项 `R20-01` 概览下一步，核对无游戏/未匹配/可备份/失败待处理及真实导航 |
| R20-01 | 概览下一步 | 已满足，待环境验证 | 1a43f474 | 当前 main Release：解析器 `15/15`、选框过滤 `22/22`、真实 WPF 概览主按钮/路由 `4/4`、Shell 选框 `4/4`，四页布局 Light/Dark `8/8`；`validate-source.py`、XAML `24/24`、diff 通过 | 失败任务→任务中心失败命令、空库→既有刷新命令、未匹配/可备份→实际点击概览主按钮后分别打开选框筛选；工作区请求一次；可备份不调用全库备份；优先级负例保持 | 合成 DTO、隔离 STA WPF/逻辑 DIP；真实 Playnite/package-host/程序集关联、物理 DPI/UIA/IME、呈现/ETW/宿主性能待验；正常隔离 host 受 CEF `platform_channel 0x5` 阻断，不绕过；未写真实存档/媒体/云端/诊断 | [当前 main 证据](evidence/R20-01-OVERVIEW-NEXT-ACTION-CURRENT-MAIN-20260924.md) 与五份 TRX；下一项 `R20-02` 指标统计范围 |
| R20-02 | 指标统计范围 | 已满足，待环境验证 | 当前 main 复核 `cdd28fd2` | Release solution/XAML `24/24`，0 errors/2 既有 CS8602；核心定向 `48/48`，四页双主题行为 `8/8`，合计 `56/56`，0 fail/skip；source-copy build 成功 | 复用 snapshot `GeneratedUtc` 与现有计数；全库/当前游戏范围与时间、未加载 `—`、合法零 `0`、无时间未知均有 formatter/VM 投影和行为测试；没有重建 DTO/服务 | 合成 DTO、隔离 testhost/STA WPF；TRX 无 COM 清理噪声。正常 Playnite/package-host、Worker/外部工具、呈现、DPI/UIA/IME、ETW、宿主性能未验，CEF `platform_channel 0x5` 不绕过 | [R20-02 当前 main 复核](evidence/R20-02-CURRENT-MAIN-RECHECK-20260924.md)与六份 TRX；接续用户报告的排序崩溃、选中行位移、待归类冗余游戏选框；之后继续 `R20-03` |
| R20-03 | 首次配置引导 | 已满足，待环境验证 | 1a90cd06 | Worker 环境检查 `1/1`；Playnite 当前身份定向 `3 passed / 1 skipped / 0 failed / 4 total`；Playnite `net462` / Tests `net472` 隔离编译 `0 errors / 2 既有 warnings`；source/XAML/diff 通过 | 复用既有维护页 `EnvironmentCheckCard`、`EnvironmentCheckService`、`OnboardingCompleted` 和真实手动备份管道；检查、完成、跳过、返回均有明确语义，不自动写用户配置 | 合成/fake/隔离目录；`LegacyProductionUiBaselineFact` 1 条跳过；真实 Playnite/package-host、外部工具、存档/云端、呈现、DPI/UIA/IME、ETW、宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并；本阶段无生产代码变更 | [R20-03 首次配置引导](evidence/R20-03-FIRST-USE-ONBOARDING-20260920.md)；下一项 `R20-04` 零结果恢复，先核对筛选摘要、清除条件与离线错误边界 |
| R20-04 | 零结果恢复 | 已满足，待环境验证 | cf09f3f6 | 定向行为 `26 passed / 0 failed / 0 skipped / 26 total`；受影响 UI/源码套件 `171 passed / 3 failed / 50 skipped / 224 total`（3 条既有基线）；隔离 Playnite `0 errors / 2` 既有 warning；source/XAML/diff 通过 | 游戏选框、媒体和云端队列显示当前筛选条件；清除只改筛选并复用现有刷新/滚动；云端读取异常明确显示失败而不是零结果；任务中心复用既有摘要与清除命令 | 合成/fake/隔离 testhost 和目录；3 条更宽套件失败为未修改的设置字段、空态覆盖层、下拉模板基线；真实 Playnite/package-host、云端/媒体、呈现、DPI/UIA/IME、ETW、宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并 | [R20-04 零结果恢复](evidence/R20-04-ZERO-RESULT-RECOVERY-20260920.md)；下一项 `R20-05` Stale 可理解，先核对旧数据时间、失败原因和重试入口 |
| R20-05 | Stale 可理解 | 已满足，待环境验证 | c492bbc2 | 定向回归 `37 passed / 1 failed / 0 skipped / 38 total`；唯一失败为未修改的任务详情断言漂移；隔离 Playnite `0 errors / 2` 既有 warning；source/XAML/diff 通过 | 任务/存档/媒体/维护页复用已有 stale 时间、错误和重试；云端补最近成功时间、失败原因、旧数据保留提示和重试入口；读取失败不清除旧记录 | 合成/fake/隔离 testhost 和目录；唯一失败 `FailedTaskDetailsPutUserReasonBeforeCollapsedTechnicalDetails` 为既有基线；真实 Playnite/package-host、云端/媒体、呈现、DPI/UIA/IME、ETW、宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并 | [R20-05 Stale 可理解](evidence/R20-05-STALE-UNDERSTANDABLE-20260921.md)；下一项从 Q/R 依赖清单选择依赖已满足的小批量 |
| R20-06 | 部分可用状态 | 已满足，待环境验证 | e32ed599 | R02/R07/Workspace 定向 `17 passed / 0 failed / 1 skipped / 18 total`；隔离 Release Playnite `net462` / Tests `net472` `0 errors / 2` 既有 warning；source/XAML `24/24`/diff 通过 | `ActionAvailabilityHints` 按恢复、媒体收件箱、云端和远端恢复分别解释禁用原因；`WorkspaceStatePresenter` 保留 Loading/Empty/Error/Degraded/Offline 独立区域；局部故障不把整页伪装成不可用 | 合成/fake/隔离 testhost 与 source-copy；初跑发现并修正云端过期横幅 `Style.BasedOn` 的真实 WPF `XamlParseException`；1 条旧架构事实跳过，关闭阶段有已知 TextServices COM 噪声但测试成功；真实 Playnite/package-host、Worker/云端/外部工具、呈现、DPI/UIA/IME、ETW、宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并 | [R20-06 部分可用状态](evidence/R20-06-PARTIAL-AVAILABILITY-20260921.md)；下一项 `R20-07` 最近活动密度，先核对事件摘要、重复进度合并、展开详情和真实计数 |
| R20-07 | 最近活动密度 | 已满足，待环境验证 | ee327e80（复用现有实现） | Core 活动映射 `4/4`；Playnite 定向 `11/11`（批处理 `3/3`、时间线 `3/3`、首页活动 `1/1`、选择/详情 `4/4`）；隔离 Release Playnite `net462` / Tests `net472` `0 errors / 2` 既有 warning；source/XAML `24/24`/diff 通过 | `TaskEventUiBatcher` 按 TaskId 合并进度、限制批次并让终态即时落地；首页最近任务保留最近 8 项和 Recycling/本地滚动；TaskCenter 失败/完成详情、技术 Expander 与有界时间线可追溯 | 合成/fake/隔离 testhost；组合 WPF host 首次出现既有选择绑定时序失败，R06 详情类单独隔离 `2/2`，未改业务实现；真实 Playnite/package-host、Worker 实时流、呈现、DPI/UIA/IME、ETW、宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并 | [R20-07 最近活动密度](evidence/R20-07-RECENT-ACTIVITY-DENSITY-20260921.md)；下一项 `R20-08` 状态语气统一，先核对加载/失败/空/完成/需操作文案模板 |
| R20-08 | 状态语气统一 | 已满足，待环境验证 | 176183ec | 定向 `30/30`；相关较宽套件 `25 passed / 1 skipped / 1 failed / 27 total`（1 条既有任务详情源断言基线失败）；隔离 Release Playnite `net462` / Tests `net472` `0 errors / 2` 既有 warning；source/XAML `24/24`/diff 通过 | 复用 `WorkspaceStatePresenter`、`ActionAvailabilityHints`、`OverviewPriorityResolver`；Shell 副标题随优先状态更新；主状态正向/负例覆盖，不以 Worker/Rclone 替代解释 | 合成/fake/隔离 testhost/source-copy；已知任务详情断言漂移与 legacy skip；真实 Playnite/package-host、Worker/工具/云端、呈现、DPI/UIA/IME、ETW、宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并 | [R20-08 状态语气统一](evidence/R20-08-STATE-TONE-20260921.md)；下一项 `R21-01` 八入口纯键盘，先核对 Q24/UIA 键盘行为 |
| R21-01 | 八入口纯键盘 | 已满足，待环境验证 | c3459ebd | R21 新增 `2/2`；相关焦点/键盘/无障碍/生产壳层回归 `31/31`；隔离 Release `0 errors / 2` 既有 warning；source/XAML/diff 通过 | 八个生产入口在 STA WPF host 中有前/反向实际焦点轨迹，全部留在入口范围并命中安全命令/导航名；现有 R05/GamePicker 夹具覆盖方向键、Enter/Esc | 未运行真实 Playnite/package-host、OS 输入、UIA/读屏、IME、物理 DPI/跨屏或宿主性能；不以离屏窗口替代呈现 | [R21-01 八入口纯键盘](evidence/R21-01-KEYBOARD-TRACE-20260921.md)；下一项 `R21-02` 控件名称与值 |
| R21-02 | 控件名称与值 | 已满足，待环境验证 | efb42b7b / 6b56a467 / 03f8d33f | `R21AutomationValueBehaviorTests 21/21`；相关定向 `35/35`；隔离 Release Playnite `net462` / Tests `net472` `0 errors / 2` 既有 warning；source/XAML/diff 通过；WPF `0/27/162` | 图标按钮、复合选择器、开关和进度条均有实际 WPF peer 名称/状态/值证据；覆盖 Invoke、选值、无选中 `null`、Off/On/Indeterminate、正常/未知/越界进度，关联 HelpText 明确 | 未运行真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、presented frame、ETW 或宿主性能；不以离屏 peer 替代这些验证 | [R21-02 收口](evidence/R21-02-CLOSEOUT-20260921.md)；下一项 `R21-03` 验证错误播报 |
| R21-03 | 验证错误播报 | 已满足，待环境验证 | 6e94195d（复用现有实现） | 设置错误导航 `1/1`；R09 错误视觉恢复 `2/2`；异步验证/源审计/数值边界 `16/16`；隔离 Release Playnite `net462` / Tests `net472` `0 errors / 2` 既有 warning；source/XAML/diff 与 WPF `0/27/162` 通过 | 复用合并验证摘要、字段 HelpText、错误详情 Hyperlink 和聚焦目标；不按每次键入重复播报；空错误集合折叠旧摘要，字段修复/错误视觉恢复有实际 WPF 负例 | 合成/fake/隔离 testhost 与目录；未运行真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、DPI/跨屏、presented frame、ETW 或宿主性能；Demo 原目录不可用，main 用户改动未碰、未合并 | [R21-03 验证错误播报](evidence/R21-03-VALIDATION-ANNOUNCEMENT-20260921.md)；下一项 `R21-04` 异步完成播报 |
| R21-04 | 异步完成播报 | 已满足，待环境验证 | f6c9619c / a67d371e / 882638ea | 新增行为 `2/2`；相邻定向 `22/22`；Core `5/5`；Worker `5/5`；隔离 Release `0/0`；XAML `24/24`；source/diff 通过；WPF `0/27/162` | 终态 Toast 实际不抢焦点且可由 Automation Name/HelpText 回读；Task Center 加载态实际变为最近更新时间；进度合并、终态旁路、失败证据去重和会话摘要均有行为负例 | 合成/fake/隔离 STA WPF；未验真实 Playnite/package-host、UIA/读屏、OS 输入、IME、物理 DPI/跨屏、presented frame、ETW、宿主性能；Demo 原目录不可用 | [R21-04 异步完成播报](evidence/R21-04-ASYNC-COMPLETION-ANNOUNCEMENT-20260921.md)；相邻旧套件另有 `28/1` 基线失败，未改写；下一项 `R21-05` 禁用与隐藏区别 |
| R21-05 | 禁用与隐藏区别 | 已满足，待环境验证 | 380234e2（复测 1f69b803） | 当前 `R21DisabledHiddenBehaviorTests 2/2`；隔离 Release XAML `24/24`、Playnite `net462` `0 error`，保留 `MediaCenterView.xaml.cs:706` 两条既有 CS8602 warning；source/XAML/diff 门禁通过 | 实际 `SaveCenterView` 验证 Collapsed 隐藏分支不入可见树/焦点，说明文本可聚焦且 Name/HelpText 可读；满足条件后同一命令恢复可执行；共享禁用动作保持可见，UIA Invoke 被拒绝且无副作用 | 合成/fake/隔离 WindowHost；未验真实 Playnite/package-host、Windows UIA/读屏、OS 输入/IME、DPI/跨屏、呈现帧、ETW 或宿主性能；Demo 原目录不可用 | [R21-05 当前复测](evidence/R21-05-TARGETED-RECHECK-20260924.md)、[R21-05 初始证据](evidence/R21-05-DISABLED-HIDDEN-BEHAVIOR-20260921.md)；下一可执行项 R23-04 正常可枚举宿主会话 |
| R21-06 | 可选择技术文本 | 已满足，待环境验证 | 866ceecd（复用既有实现并补版本入口） | R21-06 与 R03/R11/R15 行为 `13/13`；Release 测试项目 `0 错误 / 2` 条既有 CS8602 warning；source/XAML/diff 通过 | 路径与技术详情实际可全选复制；Dashboard/AcrylicProductionShell 两条插件版本入口使用共享只读 TextBox，实际可聚焦、全选，HelpText 提示 Ctrl+C；不改变版本赋值、页面导航或复制命令 | 合成路径/错误 DTO、实际 WPF TextBox、SaveCenterView、隔离 WindowHost；未验真实剪贴板/读屏、Playnite/package-host、选择像素、DPI/跨屏/呈现帧/ETW/宿主性能 | [R21-06 可选择技术文本](evidence/R21-06-SELECTABLE-TECHNICAL-TEXT-20260921.md)；按“已满足，待环境验证”收口，后续进入 R21-07 焦点可视回归 |
| R21-07 | 焦点可视回归 | 已满足，待环境验证 | b20f1a4b | R21 新增 `1/1`；R09 焦点环 `2/2`；R05 主题弹层 `1/1`；串行合计 `4/4`；Release 测试项目 `0 错误 / 2` 条既有 CS8602 warning；source/XAML/diff 通过 | 生产 Shell 中 2,000 项合成列表请求末项实现，搜索框焦点保持可见；浅/深主题资源切换后焦点样式仍在；选择器关闭后回焦可见游戏选框；保留生产选框、滚动条、Binding 和命令语义 | 合成/fake/隔离 STA WPF；隔离隐藏宿主不把 VerticalOffset/物理裁剪当作最终呈现证据；未验真实 Playnite/package-host、Windows UIA/读屏、OS 输入/IME、DPI/跨屏、像素焦点环、presented frame、ETW 或宿主性能；Demo 原目录不可用；旧 R05 选择器初始化夹具失败未改写 | [R21-07 焦点可视回归](evidence/R21-07-FOCUS-VISUAL-20260921.md)；按“已满足，待环境验证”收口，下一项 `R21-08` 单屏与跨屏分账 |
| R21-08 | 单屏与跨屏分账 | 已满足，待环境验证 | 8a31421a（复用既有实现） | `R05PopupBoundaryBehaviorTests 2/2`；`R09PixelStrokeBehaviorTests 2/2`；`R09ThemeSwitchBehaviorTests 1/1`；拓扑/源码契约 `26/26`；Release 测试项目 `0 errors / 2` 条既有 CS8602 warning；real-host 脚本语法通过 | 当前主机真实拓扑仅 `\\.\DISPLAY1`，Bounds `1707×960`、WorkArea `1707×912`；生产 Popup 在当前工作区内边界/滚动通过；1.00–2.00 仅逻辑 DPI 像素模拟；已准备第二物理屏迁移、Popup/字体/焦点/资源释放的可执行场景清单 | 未运行真实双屏 Playnite/package-host，未把离屏 DPI 模拟当物理跨屏；Popup 跨屏位置、字体清晰度、焦点与资源释放、呈现帧、Windows UIA/读屏、OS 输入/IME、ETW、宿主性能仍待验；Demo 原目录不可用；main 用户改动未碰、未合并 | [R21-08 单屏与跨屏分账](evidence/R21-08-SINGLE-CROSS-SCREEN-20260921.md)；当前单屏条件阻塞，下一可执行任务为后续依赖已满足的 Q/R 小批量 |
| R22-01 | 时间显示统一 | 已满足，待环境验证 | 1250aaad / 538fcae9 / b53ab44f / 2be8627d / c211a04f / abe9369e / e4389a09 / a799317b / eb6a8682 / 0db4abe7 / 058ca8ff / 308c2b06 / 6ea7bf9a / 44023ad6 / 0df6c69e / 6a27ef42 / f1e568d0 / 47bc1d00 / 9d319296 / 4e85d665 / 391d28b8 / 0c9870fb / ea010656 / 1bac9090 / 3f535cb2 / 494a911f / c96130a2 / 60db7534 / f39b8ef1 / c5dc47fc / df689dc1 / 2bf95267 / 48db9ee5 / 2484f132 / b1afb004 / 814d3e7a / c0885757 / 74d3dcdc / db2ba3c0 / ed03c49e / df6bee9d（残余复核，无生产改动） | 云端 stale banner、媒体缓存标题、游戏来源诊断、Trainer 版本发布时间、概览云端队列摘要和维护云端状态详情均复用 `TimeDisplayFormatter`；相对/完整本地 UTC、未知时间负例、昨天/刚刚边界、UTC 排序和单调任务时长均有实际行为证据；`R22TimeDisplayBehaviorTests 30/30`、`R22TaskDurationBehaviorTests 2/2`、本次 `R10RecentAccessBehaviorTests 2/2`；生产 `Views/*.xaml` 旧本地投影/云端详情绑定负向审计及相对/完整入口正向审计通过；隔离 Release `0 error`、主分支复核 `0 error / 2` 条既有 warning；source validation、diff check 通过 | 报告/复制列/日志稳定完整时间入口按设计保留，不再作为页面缺陷；真实 Playnite/package-host、UIA/读屏、OS 输入/IME、DPI/跨屏、最终呈现、ETW 或宿主性能仍未验；`scripts/validate_wpf_ui.py` 当前不存在，Demo 原目录不可用 | 待真实宿主验证；具体限制见前列 | [R22-01 时间审计收口](evidence/R22-01-TIME-AUDIT-CLOSEOUT-20260922.md)、[R22-01 残余时间入口复核](evidence/R22-01-RESIDUAL-ENTRY-RECHECK-20260923.md)、[Trainer 发布时间](evidence/R22-01-TRAINER-PUBLICATION-TIME-20260922.md)、[游戏来源诊断时间](evidence/R22-01-GAME-DISCOVERY-TIME-20260922.md)、[概览云端摘要时间](evidence/R22-01-OVERVIEW-CLOUD-SUMMARY-TIME-20260922.md)、[维护云端详情时间](evidence/R22-01-CLOUD-DETAIL-TIME-20260922.md)；下一项 R23-04 UIA/Controlled host 收口 |
| R22-02 | 容量单位统一 | 已满足，待环境验证 | 86b72e1e | `R22CapacityUnitBehaviorTests 13/13`；相关 Core `53/53`；`MaintenanceReportServiceTests 3/3`；隔离 Release XAML `24/24`、Contracts/Playnite net462、Tests net472、Worker `0 errors / 2` 条既有 CS8602 warning；source/diff 通过；WPF `0/27/177` | 统一 1024 进制 `B/KiB/MiB/GiB` 与最多两位小数，小于 1 KiB 直接显示 B；Backup 概览/列表/详情 DTO、媒体/Trainer/诊断/元数据灾备及 Worker 维护报告复用同一口径；1 字节隔离镜像报告实际为 `1 B`；比较页 `0 B` 与 IPC `+0 B` 差值语义保留 | 合成 DTO、fake/隔离 Worker、隔离目录；未验真实 Playnite/package-host、实际窗口/UIA/读屏、OS 输入/IME、DPI/跨屏、呈现帧、ETW 或宿主性能；Demo 原目录不可用；main 用户改动未碰、未合并 | [R22-02 容量单位统一](evidence/R22-02-CAPACITY-UNITS-20260921.md)；下一项 `R22-03` 复制反馈轻量 |
| R22-03 | 复制反馈轻量 | 已满足，待环境验证 | 5a21e18e | `R22CopyFeedbackBehaviorTests 2/2`；R06 `4/4`；R15 `6/6`；R16 `1/1`；R21 两个独立 STA 场景 `1/1 + 1/1`；提交身份 Release Playnite net462/Tests net472 无错误、既有 `MediaCenterView.xaml.cs:671 CS8602` 两条 warning；source/diff 通过；WPF `0/27/177` | DataGrid、设置页路径、Dashboard 复制通知和通知详情复用同一非模态 Popup；成功/空内容/剪贴板占用失败均有反馈，失败含重试信息；Popup 定位原控件、不进布局、不抢焦点；连续复制复用/合并而不堆 Toast；FeedbackToast Automation Name/HelpText 可读 | fake 剪贴板、合成 DTO、隔离 STA Window；未验真实 Playnite/package-host、真实剪贴板、Windows UIA/读屏、OS 输入/IME、DPI/物理跨屏、呈现帧、ETW 或宿主性能；Demo 原目录不可用；main 用户改动未碰、未合并 | [R22-03 复制反馈轻量](evidence/R22-03-COPY-FEEDBACK-20260921.md)；下一项 `R22-05` 批量数量防歧义 |
| R22-04 | 打开路径失败 | 已满足，待环境验证 | a43a8968（复用 R16 实现） | `R16SettingsPathEditorBehaviorTests` + 源码边界 `3/3`；Release 测试项目 `0 errors / 2` 条既有 CS8602 warning；source/XAML/diff 通过 | 隔离临时目录实际区分有效文件、有效目录、缺失目录和文件冒充目录；设置页无效 Probe 在打开前显示原因并保留完整路径复制；Dashboard 失效本地路径由 RunLocal/ReportDashboardFailure 捕获 | 未启动真实 Explorer，未验真实 Playnite/package-host、网络共享/ACL/占用导致的系统级打开失败、呈现、UIA/读屏、OS 输入/IME、DPI/跨屏、ETW 或宿主性能；Demo 原目录不可用；main 用户改动未碰、未合并 | [R22-04 打开路径失败](evidence/R22-04-OPEN-PATH-FAILURE-20260921.md)；下一项按依赖选择 R22-01 时间显示统一或其他可独立 Q/R 小批量 |
| R22-05 | 批量数量防歧义 | 已满足，待环境验证 | 501cb715 | `R22BatchCountBehaviorTests 3/3`；R05 收件箱 `3/3`；R16 策略模板 + TaskRetry 合计 `3/3`；隔离 Release XAML `24/24`、Playnite net462/Tests net472/Worker `0 errors`、既有 MediaCenter CS8602 两条 warning；source/diff 通过；WPF `0/27/177` | 当前游戏媒体批量动作同时显示已选、当前结果、当前窗口可操作和隐藏选择；策略模板批量同时显示已选、当前筛选结果、隐藏选择、排除和变更字段；保留 R05-04 收件箱按模式稳定 ID 汇总；任务中心按当前结果批量，不伪造复选选择 | 合成 DTO/稳定 ID、隔离 STA WPF ListBox、隔离 `.tmp`；未验真实 Playnite/package-host、Windows UIA/读屏、实际键盘筛选分页切换、DPI/物理跨屏、呈现帧、ETW 或宿主性能；Demo 原目录不可用；main 用户改动未碰、未合并 | [R22-05 批量数量防歧义](evidence/R22-05-BATCH-COUNT-20260921.md)；下一项 `R22-06` 长任务离页提示 |
| R22-06 | 长任务离页提示 | 已满足，待环境验证 | ef671288 | `R22LongTaskLeavePageBehaviorTests 2/2`；R21 异步完成播报 `2/2`；R08 缓存工作区切换第一场景通过；隔离 Release Playnite net462/Tests net472 `0 errors`、既有 MediaCenter CS8602 两条 warning；source/diff 通过；WPF `0/27/177` | TaskCenter 明确提示离开页面不会取消 Worker 后台任务，返回后从任务记录恢复阶段/进度；现有事件订阅、快照/历史轮询、终态 Toast、缓存页面和显式取消命令保持，不伪造实时进度 | 合成/fake WPF、隔离 Window、隔离 `.tmp`；未验真实 Playnite/package-host、Worker 长任务、实机离页/返回、Windows UIA/读屏、DPI/物理跨屏、休眠/重启恢复、呈现帧、ETW 或宿主性能；Demo 原目录不可用；main 用户改动未碰、未合并；R08 另一个旧 Shell 文本契约断言失败未改写 | [R22-06 长任务离页提示](evidence/R22-06-LONG-TASK-LEAVE-20260921.md)；下一项 `R22-07` 确认框信息结构 |
| R22-07 | 确认框信息结构 | 已满足，待环境验证 | 3ac6d16a | `R22ConfirmationStructureBehaviorTests 4/4`；相关对话框/焦点/恢复/远端回归合计 `34/34`；隔离 Release XAML `24/24`、Playnite `net462` / Tests `net472` / Worker `0 errors`，既有 `MediaCenterView.xaml.cs:699 CS8602` 两条 warning；source/diff 通过；WPF `0/27/177` | 真实确认调用已核对对象/范围/后果与动作按钮文案；沿用独立可滚动详情区；共享策略显式区分危险/普通/三选一/结果键盘语义，危险确认取消为安全选项、确认不成为 Enter 默认；本地/撤销/远端恢复补 `isDangerous: true`，保留 PreRestore、当前选择复核、取消和 Worker 语义 | 合成请求/DTO、隔离 STA WPF、隔离 `.tmp`；native fallback 仍受 Playnite Yes/No API 限制；未验真实 Playnite/package-host、Windows UIA/读屏、OS 输入/IME、DPI/物理跨屏、呈现帧、ETW 或宿主性能；Demo 原目录不可用，main 用户改动未碰、未合并；未读写真实存档/媒体/云端或外发诊断 | [R22-07 确认框信息结构](evidence/R22-07-CONFIRMATION-STRUCTURE-20260922.md)；下一项 `R22-08` 状态样式一致索引 |
| R22-08 | 状态样式一致索引 | 已满足，待环境验证 | d919179a | `R22StatusStyleIndexBehaviorTests 7/7`；状态/图标/Workspace 回归 `32 passed / 1 skipped`（既有 legacy source skip）；隔离 Release XAML `24/24`、Playnite `net462` / Tests `net472` / Worker `0 errors`，既有 `MediaCenterView.xaml.cs:699 CS8602` 两条 warning；source/diff 通过；WPF `0/27/177` | 复用现有 `StatusGlyphConverter` 与主题资源；补齐运行/进行中/传输中/处理中/校验中信息符号、暂停/未知警示符号；成功/错误保持既有符号，未知不借用成功；TaskCenter/Maintenance 共享同一转换器 | 合成状态字符串、现有 DTO/转换器、隔离 testhost；未验真实跨页颜色/图标呈现、Playnite/package-host、Windows UIA/读屏、OS 输入/IME、DPI/物理跨屏、呈现帧、ETW 或宿主性能；Demo 原目录不可用，main 用户改动未碰、未合并；未读写真实存档/媒体/云端或外发诊断 | [R22-08 状态样式一致索引](evidence/R22-08-STATUS-STYLE-INDEX-20260922.md)；下一项按依赖选择 `R23-01` 每组可审阅交付 |
| R23-01 | 每组可审阅交付 | 已满足 | 553b38a9（审计基线，无新增业务代码） | R00-R22 共 23 组、184 项逐行核对；每组 8 项；184/184 行有非占位提交字段和 evidence 路径；当前分支工作树干净且 HEAD/远端同为 553b38a9 | 账本逐项保留实现、自动/视觉/宿主门禁、边界与下一步；R02-06 外部阻塞、R05 不适用、R22-01 部分满足和 R13-R22 环境待验未被聚合覆盖 | 本项只验证交付可追溯性，不替代各项真实 Playnite/package-host、UIA/读屏、OS 输入/IME、DPI/跨屏、呈现帧、ETW 或宿主性能；main 用户改动未碰，生成目录已清理 | [R23-01 每组可审阅交付](evidence/R23-01-GROUP-DELIVERY-20260922.md)；下一项 `R23-02` 生产资源状态矩阵 |
| R23-02 | 生产资源状态矩阵 | 已满足，待真实宿主环境验证 | a18cb43e（共享样式与行为） | 提交后隔离 Release：XAML `24/24`、solution `0 errors/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning，Playnite `net462`/测试 `net472`；`R23ProductionResourceStateBehaviorTests 3/3`；关联旧门禁复跑 R00-01/02 `5/5`、R00-05 `2/2` | Light/Dark 下分别实例化实际 `AcrylicNavItem`、由 `GscSettingsSectionTabs` 生成的 `GscSettingsSectionTabItem`，以及 Task/Media Inbox/Save History/Maintenance 四个派生 DataGrid；观察选中/失选、键盘焦点/失焦、禁用外观、选择不变量与拒焦负例。hover 仅核对模板声明，未驱动真实鼠标；无独立 pressed chrome 的控件按不适用记录 | R23-02 原记录时 freshness 为 `12/14`；当前 `57d96bbb` R01-07 复核为 `14/14 fresh`、0 stale、`matchedSourcePaths=0`，但 freshness 不会重跑旧 WPF 探针，也不表示已安装包。受控 STA/合成行不代表鼠标 hover、Playnite/UIA/读屏、OS 输入/IME、DPI/跨屏、最终呈现帧、ETW 或宿主性能；Demo 原目录不可用 | [R23-02 生产资源状态矩阵](evidence/R23-02-PRODUCTION-RESOURCE-STATE-MATRIX-20260922.md)、[R01-07 当前 freshness](evidence/R01-07-freshness-report-20260924-current.json)；下一项 `R23-03` 代表页面终审 |
| R23-03 | 代表页面终审 | 已满足，待真实宿主环境验证 | 16f4dee6（既有盘点）/ 7a4ba2a9（当前复核） | 当前身份隔离 Release XAML `24/24`、solution `0 errors/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning；六类页面状态/操作抽样测试 `38 passed/0 failed/1 skipped`，skip 明确是撤销架构断言 | 终审覆盖概览、存档、媒体、工具、任务、维护、设置、壳层八个当前 XAML 入口；分别复核空/错/加载或业务例外、实际命令/Automation 路由、滚动/选择/安全边界及 Demo-first 对应；未拿基类样例或旧截图代替生产页面事实 | 合成/fake、生产 XAML 与隔离 STA WPF；不宣称 Playnite package-host、Windows UIA/读屏、OS 输入、物理 DPI/跨屏、当前真实像素、ETW 或宿主性能。R23-04 CEF `platform_channel` `0x5` 是正常宿主边界；Demo 原目录不可用 | [R23-03 代表页面终审](evidence/R23-03-REPRESENTATIVE-PAGE-FINAL-20260922.md)；下一项 `R23-04` 正常可枚举隔离宿主会话（仍受阻则推进依赖已满足的 Q/R） |
| R23-04 | 非空隔离宿主 | 已满足，待宿主环境验证 | 8a9eead2（代码）/ f457a7a0、0157ace6、c6d65b08、2d327d4b、eba374c5、f551359c（复测与窗口证据）；910480c7（启动失败证据/runner fix） | 收口 7 列 TaskGrid 的列宽/stage 排序契约，并修复真实嵌入截图暴露的共享按钮复合内容字符串化：`UIElement`/`FrameworkContentElement` 直接呈现，字符串继续走文本模板；新增 STA 正/负行为测试；窗口探测 runner 逐窗记录 Win32/UIA，能自举隔离 profile。已修 bootstrap 正常关闭与 theme manifest ID 查找；新加失败证据分类/写盘。clean Release build `0 errors/2` 既有 CS8602；XAML `24/24`，`DiagnosticsEvidenceSourceTests 7/7`，离线 CEF/一般失败正负例通过 | `08a10da9` 全新 profile 在扩展安装前直接记录 `platform_channel.cc:108` 拒绝访问 `0x5`，bootstrap 未正常退出并留 `safestart.flag`；无扩展/seeder、run manifest、summary、Dashboard、UIA 侧栏或截图。相同状态不重试；此前 `42884321` 的空 CEF Startup Error 不被追溯归因。 | 正常 Playnite host/UIA/Controlled 仍待环境；不删标记、不绕过 CEF 权限。四页布局当前 Light/Dark `8/8` 只属隔离 WPF/逻辑 DIP，用户 Settings 新截图 identity/父容器未确认。Q24-03 单屏阻塞；真实 presented frame、物理跨屏、ETW/PresentMon、宿主性能和 Demo 原目录仍未验 | [R23-04 启动失败证据与校正](evidence/R23-04-BOOTSTRAP-SAFE-START-ROOT-CAUSE-20260924-42884321.md)、[bootstrap evidence 910480c7 测试](evidence/R23-04-BOOTSTRAP-FAILURE-EVIDENCE-20260924-910480C7.trx)、[用户四页布局 910480c7 测试](evidence/USER-REPORTED-LAYOUT-CURRENT-MAIN-RECHECK-20260924-910480C7.trx)、[R23-04 任务表宿主初始化修复与复合按钮](evidence/R23-04-TASK-GRID-HOST-FIX-20260922.md)；下一项继续依赖满足且不需 CEF/ETW 的 Q/R 小批量 |
| R23-05 | 帧性能证据闭环 | 部分满足，待宿主性能验收 | 130ba48a / b5c7a6d4 / b5e7fca0 | 当前 clean `shellqa`：单次 `30` 回调/p95 `27.1ms`/最大 `87.4ms`，快速二次 `46`/`14.3ms`/`39.4ms`，无动画 `4`/`21.6ms`；本次断点 clean 复测 `WorkingTreeClean=True`；XAML `24/24`、solution `0 errors/2` 既有 warning | Media 1040/1100/1366 的 `gridTopGap=142 DIP`，footer/历史/次级动作可达；新增真实 WPF shellqa 覆盖 1200/1279/1280/1366 断点，标题/动作区关系与 HeaderSurface 边界通过；当前报告明确是 offscreen WPF Rendering 代理，不是真实 presented frame | ETW/WPR/xperf 真实采集仍受系统权限拒绝；不把代理数字、离屏截图或 Stopwatch 写成 DWM/PresentMon/物理刷新率结论；真实 Playnite 宿主性能仍待验 | [R23-05 帧性能证据闭环](evidence/R23-05-FRAME-PERFORMANCE-EVIDENCE-20260922.md)、[R23-05 壳层断点几何复测](evidence/R23-05-HEADER-BREAKPOINT-RECHECK-20260923.md)；R23-04 正常宿主仍阻塞，下一项转依赖已满足的 Q/R 行为小批量 |
| R23-06 | 安装与回退可核查 | 已满足，待宿主环境验证 | 4f778e9b | 当前 main Release：XAML `24/24`、Core `125/125`、Worker 非进程级 `356/356`、受影响 Playnite 类 `179 passed/40 skipped`；source validator 通过、WPF 静态 `0 errors/28 warnings/177 info`；六份程序集身份均为 `0.6.73+4f778e9b…`；zip/pext 同 SHA-256 `82A615DA…6B24DF1`、各 `45,482,846` bytes；synthetic profile 安装及旧候选逐文件回退核对通过 | 合并 Playnite testhost 无输出超过 11 分钟后停止，分类隔离测试通过；Worker process-restart 类未纳入本轮非进程级套件。仅操作仓库 `.tmp` 合成 profile，未写真实 Extensions/存档/媒体/云端；未把包安装事实扩大成真实宿主呈现或恢复通过 | 待真实宿主验证；具体限制见前列 | [R23-06 当前 main 安装与回退复核](evidence/R23-06-CURRENT-MAIN-ROLLBACK-20260926.md)、[R23-06 历史复核](evidence/R23-06-PACKAGE-ROLLBACK-20260922.md)；按 R23-08 准入清单继续依赖已满足的 Q/R，R23-04/05 外部宿主和呈现边界不重试或绕过 |
| R23-07 | 任务去重与收尾 | 已满足 | 91a008fb（原审计）/ e0a17e3a（main 状态复算，仅文档） | R 表正式部分 `192/192` 唯一 ID：106 已满足/受控复核、83 环境门禁待验、1 外部阻塞、1 部分满足、1 不适用；Q 表 `208/208`，5 已验收、203 未完成 | 不新增视觉/交互实现，不把等待宿主/权限的项并入已验收；逐项证据沿用现有证据链接 | R23-04 正常宿主/ UIA、R23-05 真实 presented frame 与宿主性能、R02-06 原生菜单、Q24-03 物理跨屏仍各自保留边界 | [R23-07 原审计](evidence/R23-07-TASK-DEDUPE-CLOSURE-20260922.md)；[当前 main 状态复算](evidence/R23-07-CURRENT-MAIN-STATUS-RECHECK-20260924.md)；下一宿主条件满足时复跑 R23-04 |
| R23-08 | 下一轮准入清单 | 已满足 | 4ad8bd17（准入基线）/ 6ac276d1（当前 main 复核，仅文档） | 当前复核 R23-06 包安装/回退已完成；R22-01 残余入口无新增生产绑定缺口；R 表仍为 192 项且无待开始/实施中行 | 仅保留有复现或明确用户价值的工作；无当前代码缺口时不自创功能；CEF/物理跨屏/ETW 门禁按条件恢复，不用离屏或 synthetic 证据冒充宿主 | 本机当前桌面仅枚举到 `DISPLAY21`；Playnite/Worker 未运行；R23-04 不在相同 CEF `0x5` 状态重试；Q24-03 等第二活动显示器；R23-05 等合规跟踪权限 | [R23-08 当前准入复核](evidence/R23-08-CURRENT-MAIN-ADMISSION-20260926.md)、[原始准入清单](evidence/R23-08-NEXT-ROUND-ADMISSION-20260922.md)、[R23-06 当前 main 安装与回退](evidence/R23-06-CURRENT-MAIN-ROLLBACK-20260926.md)；下一阶段需等待上述环境条件变化或用户提供新缺陷/功能范围 |

## 2026-09-19 Round3 R12-01 恢复流程分步摘要

- `e1a8da0c` 复用已有恢复 readiness/task/command 能力，把选择版本、可恢复性检查、目标核对和执行结果映射为固定四阶段摘要；真实恢复确认、取消、错误传播、PreRestore 保护和回滚语义保持不变。
- 当前提交隔离 Release solution `0 warning / 0 error`、XAML `24/24`；R12 行为 `6/6`；R11/R06 相邻回归合计 `17/17`；资源字典类 `137/39/0`；WPF 静态审查 `0/24/177`。阶段负例检查目标未进入执行、失败详情和回滚文本，不是字符串存在性签收。
- 当前提交绑定的 render-qa 真实退出 `1`，报告只命中既有 Overview 空列表/Task/Save 可读行/Settings 状态/Shell header/Media 几何基线；不把离屏 harness 写成 Playnite 实机或安装通过。Demo 原目录不可用，继续沿用恢复生产基线。
- main 的 DEV-INSTALL-008 失败事实仍为 Release `0/0`、Core `83/83`、Worker `311/311`、Playnite `73 failed / 588 passed / 57 skipped`、安装器退出 `1`；main 用户改动未触碰。下一可执行小批量：R12-02 校验结果解释。

## 2026-09-19 Round3 R12-02 校验结果解释

- 6cc3a618 复用已有 readiness DTO/service/SQLite JSON 持久化和 SaveCenter 卡片，补齐哈希覆盖数、可覆盖总数、状态文案和旧结果提示。有效 Manifest 无哈希现在是 Warning，不再把“未提供哈希”显示成校验成功；部分覆盖显示比例并说明未覆盖文件不能视为已校验。
- 合成恢复校验 13/13、Core 显示 19/19；隔离 build/test 全流程为 XAML 24/24、Release 0/0、Core 85/85、Worker 324/324、Playnite source 组和 84 个 WPF 隔离类全部返回 0；资源字典类 137/39/0。
- WPF 静态审查 0 error / 24 warning / 177 info；warning/info 为既有外层布局、Canvas、颜色令牌提示。没有真实 Playnite/package-host、物理 DPI/跨屏、presented frame、UIA/IME、ETW 或宿主性能证据。Demo 原目录不可用，沿用恢复生产基线；main 用户改动未触碰。
- 下一可执行小批量：R12-03 目标路径核对；当前分支代码已推送，main 尚未合并。

## 2026-09-19 Round3 R12-03 目标路径核对

- `c0adb1b7` 复用已有 `PathRemapService.PreviewAsync`、预览 DTO、IPC 和设置写回；Maintenance 页面增加原路径/重映射路径对照、目标存在状态和完整可复制文本。旧根/新根改变时清除旧预览，不展示过期目标。
- 预览 DataGrid 的 `MaxHeight=260`、`Tag=FiniteViewport`、Recycling 虚拟化和只读 TextBox 保留有限列表性能与路径复制能力；服务仍只改索引/Worker 设置，不移动或删除文件。
- 行为证据为 Worker `4/4`、Playnite `2/2`、源代码契约 `24/24`；完整隔离构建 XAML `24/24`、Release `0/0`、Core `85/85`、Worker `325/325`，source `68` 类/WPF `84` 类隔离进程全返回 0；资源字典 `137/39/0`；WPF 静态审查 `0/25/177`。
- 长路径、不同盘符、相似根负例和目标不写入使用合成数据、fake、隔离 SQLite/目录验证；未验真实 Playnite/package-host、物理 DPI/跨屏、presented frame、UIA/IME、ETW 或宿主性能。Demo 原目录不可用，沿用恢复生产基线。
- 最新 main 一键命令在安装前的 `DEV-INSTALL-008` 仍因 `DangerousConfirmationKeepsCancelAsTheInitialFocusTarget` 失败（`273 passed / 18 skipped / 1 failed / 292 total`）；main dirty R08 实现与跟踪测试源断言不一致。本分支对应源测试已 `24/24`，未覆盖 main 用户改动。下一可执行小批量：R12-04 恢复保护备份。

## 2026-09-19 Round3 R12-04 恢复保护备份

- `e4e42f40` 复用已有 RestoreOrchestrator、TaskCoordinator、GameOperationLock 和 PreRestore 回滚链路。PreRestore 返回失败、无法确认版本、无法锁定或本地保护索引保存失败时统一返回 `RESTORE_PRERESTORE_FAILED`，危险恢复在写入前中止。
- SaveCenter 执行摘要把保护备份作为当前子阶段；成功任务消息包含“执行前保护快照已创建并锁定”，失败显示“保护备份阶段失败，危险恢复已中止”并保留任务详情。没有新增命令、DTO、选框或滚动体系。
- fake Worker 负例先让保护备份失败并确认没有目标恢复调用，再把当前状态改成 `A-latest` 重试，确认重试重新取得同游戏操作门、用最新状态建立锁定快照并完成目标恢复；Worker `12/12`，Playnite `7/7`。
- 完整隔离构建 XAML `24/24`、Release `0/0`、Core `85/85`、Worker `326/326`，source `68` 类/WPF `84` 类隔离进程全返回 0；资源字典类 `137/39/0`。证据使用合成/fake/隔离目录和 STA WPF/offscreen logical DIP，不代表真实 Playnite/package-host、呈现帧、物理 DPI/跨屏、UIA/IME、ETW 或宿主性能；Demo 原目录不可用。
- 最新 main 一键命令仍在安装前因 `DangerousConfirmationKeepsCancelAsTheInitialFocusTarget` 失败（`273/18/1`）；本分支未覆盖 main dirty R08 文件。下一可执行小批量：R12-05 远端下载进度。

## 2026-09-19 Round3 R12-05 远端下载进度

- `23e5b9d4` 先复用现有 `TaskCoordinator`、任务事件流、remote staging/校验服务和维护页状态 DTO；StageRemote 任务按准备、下载到隔离区、哈希一致性校验、Ludusavi 版本确认、写入隔离清单和等待恢复确认发布进度。下载完成只代表隔离区已校验，当前存档仍未恢复。
- 取消时 Worker 先删除本次隔离目录并保留“已清理”或“清理失败仍有残留”的终态消息；普通失败同样报告清理失败，不再静默吞掉残留。维护页取消按钮与进度条只在活动任务显示，非远端任务事件不会改变其状态。
- 最终 D 盘隔离 Debug 构建 `0 warning / 0 error`、XAML `24/24`；Playnite `R12RemoteStageProgressBehaviorTests 3/3`、Worker `RemoteBackupStagingSafetyTests 22/22`；`validate-source.py`、XAML、`git diff --check` 通过。
- 全量隔离 Playnite 门禁在同一轮既有 `R07ResizeStressBehaviorTests.ResizeSequenceKeepsOpenTaskDetailsAndPickerFocusReachable` 处返回失败并按脚本停止；该失败不属于 R12-05，未被改写为 skip 或通过，也未声称全量 WPF 绿色。未运行真实 Playnite/package-host、远端 rclone/Ludusavi、物理 DPI/跨屏、UIA/IME、presented frame、ETW 或宿主性能；未写真实存档、媒体、用户云端或外发诊断。
- Demo 原目录不可用，继续沿用恢复生产基线；main dirty R08 文件和 `src.zip` 未触碰、未合并。下一可执行小批量为 `R12-06 恢复冲突说明`，先复用现有恢复冲突/任务结果投影，再补成功与负例行为证据。

## 2026-09-19 Round3 R12-06 恢复冲突说明

- `423856b2` 复用现有 `RestoreReadinessDto`、`TaskStatusDto`、错误码和四阶段 `RestoreWorkflowProgress`，新增原因分类：游戏运行、操作锁、磁盘空间、目标权限和未知原因。SaveCenter 只在原流程卡片中显示“处理步骤”，没有新增恢复服务/DTO/IPC，也没有改变写入、取消、回滚或 PreRestore 保护。
- 游戏运行提示退出游戏/启动器/MOD 管理器；操作锁提示等待任务中心并禁止并发重试；磁盘空间提示清理或迁移后重新验证；权限提示核对 Playnite/Worker 账户 ACL。无明确依据的泛化写入失败保持未知原因，所有分类都明确不通过关闭安全机制解决。
- 新增行为测试 `R12RestoreConflictExplanationBehaviorTests 4/4`，并与既有 `R12RestoreWorkflowBehaviorTests 7/7` 合并为 Playnite `11/11`；包含游戏运行、操作锁、磁盘空间、权限正例与泛化写入失败负例，另验证 `ResolutionDisplay`/Automation HelpText 绑定。
- 最终隔离 Debug 构建 XAML `24/24`、solution `0 warning / 0 error`；Worker `RestoreOrchestratorTests 12/12`；`python scripts/validate-source.py` 与 `git diff --check` 通过。未运行全量 WPF，未把既有 R07 resize/focus 边界改写为通过。
- 证据只来自合成 DTO/fake、隔离输出、net462 Playnite 测试程序集；未运行真实 Playnite/package-host、物理 DPI/跨屏、UIA/IME、presented frame、ETW 或宿主性能，未写真实存档/媒体/云端/诊断。Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未触碰、未合并。下一可执行小批量为 `R12-07 预览失效重验`。

## 2026-09-19 Round3 R12-07 预览失效重验

- `00724e62` 先复用现有恢复确认、映射、readiness 和 PreRestore 链路：确认对话框返回后重新比较当前游戏/版本身份，变化时清空流程并拒绝旧确认；Worker 仍按最新映射和精确 `BackupId` 解析目标。
- 新增行为证据覆盖确认守卫正负例、同大小归档内容变化和恢复调用顺序。隔离 Worker 记录 `preview → write → post-validation` 为 `true → false → true`；Manifest SHA-256 变化返回 `Corrupted/Failed`，不以大小相同视为有效。
- 最终隔离 Debug solution `0 warning / 0 error`、XAML `24/24`、Playnite `13/13`、Worker `27/27`；`python scripts/validate-source.py` 与 `git diff --check` 通过。无 XAML/视觉资源改动，选框、滚动条、命令绑定、取消/错误和恢复保护保持。
- 证据只来自合成 Manifest/归档、fake Worker、隔离目录和 net462 程序集；未运行真实 Playnite/package-host、全量 WPF、物理 DPI/跨屏、UIA/IME、presented frame、ETW 或宿主性能，未写真实存档/媒体/云端/诊断。Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未触碰、未合并。下一可执行小批量为 `R12-08 恢复结果报告`。

## 2026-09-19 Round3 R12-08 恢复结果报告

- `fd4756ea` 复用既有 `RestoreOrchestrator`、`TaskCoordinator`、`TaskStatusDto`、PreRestore 和任务详情滚动容器，新增无凭据 `RestoreReportDto`。报告记录执行目标、目标版本、执行前预览确认的文件范围、保护快照、当前阶段、稳定任务 ID 和完成/回滚/人工介入/取消/失败结果。
- Worker 把报告写入任务 SQLite，并让最近任务、活动任务和分页查询回读；TaskCenter 在既有详情滚动区显示目标版本、任务 ID、文件范围、保护备份和失败阶段。复制命令在有报告时只复制脱敏摘要，不包含路径、诊断详情或凭据。
- 行为证据为 Playnite R12 `15/15`、Worker 定向 `34/34`；最终隔离 Debug solution `0 warning / 0 error`、XAML `24/24`；`validate-source.py`、XAML check、`git diff --check` 通过。测试曾捕获分页 JSON 选项缺口，修复后才签收，未用 Assert.Contains 代替行为验证。
- 当前 linked worktree 直接构建 WPF 临时项目仍有 Access denied，最终构建使用当前分支外部隔离源码副本和独立输出根；没有运行真实 Playnite/package-host、全量 WPF、物理 DPI/跨屏、UIA/IME、presented frame、ETW 或宿主性能，也未写真实存档、媒体、云端或诊断。Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未触碰、未合并。`.tmp/r12-07-build-final` 仍待后续启动重试精确清理。
- 下一可执行小批量为 `R13-01 队列阶段展示`：先核对现有 `CloudTransferCoordinator`/维护页阶段投影及 Q22 依赖，再补上传成功不冒充远端校验的负例。

## 2026-09-19 Round3 R13-01 队列阶段展示

- `803470b8` 复用现有 `CloudTransferStatusDto.State`、`CloudTransferSummaryDto.QueueControlDisplay`、Worker 队列状态和维护页；新增 `QueuePhaseDisplay`，将等待队列、网络退避、普通重试、上传中、远端验证、等待验证和已验证分别显示。维护页详情保留 `GuaranteeLevelDisplay`，上传成功仍不冒充远端校验。
- Core 阶段映射 `27/27`、Playnite `7/7`、Worker `CloudTransferStateTests 11/11`；隔离 Debug solution `0 warning / 0 error`、XAML `24/24`；`validate-source.py` 与 `git diff --check` 通过。认证失败重试的负例保持“等待重试”，未写成“等待网络”。
- 证据使用合成 DTO/fake/隔离 Worker 和 net462 程序集；通过显式 `GIT_DIR` 绑定当前 linked worktree 身份，外部副本已清理。未验真实 Playnite/package-host、真实远端、物理 DPI/跨屏、UIA/IME、presented frame、ETW 或宿主性能；Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰、未合并。
- 下一可执行小批量为 `R13-02 下次重试时间`：先核对既有 `NextAttemptUtc/NextAttemptLocal`、系统时钟变化和页面生命周期，再补负倒计时与常驻计时器负例。

## 2026-09-19 Round3 R13-02 下次重试时间

- `6fd22892` 复用 `NextAttemptUtc/NextAttemptLocal`，新增 `RetryTimingDisplay`，在维护页详情同时显示本地绝对时间和有界相对提示；未来时间显示约 N 分钟/小时/天后，到期显示可立即重试，无时间显示无自动重试。
- Core `29/29`、Playnite `2/2`、Worker `CloudTransferStateTests 11/11`；最终隔离 Debug solution `0 warning / 0 error`、XAML `24/24`；`validate-source.py` 与 `git diff --check` 通过。没有新增常驻计时器，因此没有关闭页面后的计时器生命周期问题。
- 证据使用合成 DTO/fake/隔离 Worker 和 net462 程序集；未验真实 Playnite/package-host、真实远端、物理 DPI/跨屏、UIA/IME、presented frame、ETW 或宿主性能，未写真实云端/存档/媒体/诊断。Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰、未合并。
- 下一可执行小批量为 `R13-03 手动重试范围`：先核对单项/媒体入口、幂等 requestId 和部分成功后的重试范围，再补重复点击负例。

## 2026-09-19 Round3 R13-03 手动重试范围

- `680ea83a` 先复用现有单项/媒体重试入口、任务中心批量入口和 IPC ledger；`CloudTransferStatusDto.CanManuallyRetry` 只允许选中行处于 `Failed` 或 `RetryScheduled`，维护页详情明确只重试当前选中云端上传，不重新执行本地备份。传输中、已上传、已校验由状态负例说明不会重复提交。
- 既有任务中心批量入口仍只处理当前加载且符合筛选的失败/取消任务，按任务类型和游戏 ID 去重；云端上传重试使用已保留的本地备份/媒体归档。`RetryCloudUpload` 与 `RetryMediaCloudUpload` 保持 replay protection，超时重放沿用原 RequestId。
- Playnite `8/8`、Core `29/29`、Worker 云状态/部分成功 `19/19`、Worker IPC ledger `6/6`；隔离 Debug solution `0 warning / 0 error`、XAML `24/24`，source validation/diff check 通过。新增行为测试不是只查文案：实际构造状态 DTO，并用 `RelayCommand.CanExecute` 验证忙态第二次点击提交次数仍为 `1`。
- 证据仅使用合成 DTO、fake/隔离 SQLite、真实命令门控和外部隔离构建；Playnite named-pipe 行为类为 `1 passed / 6 skipped / 0 failed`，跳过项未写成真实 IPC 通过。未验真实 Playnite/package-host、真实远端、物理 DPI/跨屏、UIA/IME、presented frame、ETW 或宿主性能；Demo 原目录不可用，沿用恢复生产基线；main 用户改动与 `src.zip` 未碰、未合并。
- 下一可执行小批量为 `R13-04 暂停与允许时段`：先核对 `CloudUploadQueuePaused`、允许时段、持久化队列状态和进行中上传不被意外取消的边界。

## 2026-09-19 Round3 R13-04 暂停与允许时段

- `cca3f052` 先复用 `WorkerOptions.CloudUploadQueuePaused`、允许时段字段、`CloudRetryService` 和现有设置页开关；没有另建队列状态或替换调度器。`CloudTransferSummaryDto.QueueControlDisplay` 现在按用户暂停、允许时段外、无条目空闲、存在条目运行中依次投影，避免空队列误报运行中。
- 暂停语义保持为只暂停后台自动重试，恢复开关后继续处理已保存队列；允许时段变更提示在下一轮 Worker 检查生效，已经开始的上传不会被取消。源代码复核确认暂停跳过自动 sweep、时段外持久化 defer；本阶段未把该静态边界写成真实云端运行时证明。
- Core 定向队列摘要 `1/1`；Worker `CloudUploadWindowPolicyTests` 两项及 `CloudQueueWindowSettingsAreClampedAndRoundTrip` 合计 `3/3`；Playnite `R13CloudTransferStageBehaviorTests 9/9`，既有 `PortableSettingsTests 10/10`。隔离 Debug solution `0 warning / 0 error`、XAML `24/24`；`validate-source.py` 与 `git diff --check` 通过。
- 证据只使用合成 DTO、现有 Worker 策略和 fake/隔离设置、net462 程序集及外部隔离构建；未写真实云端、存档、媒体或诊断。未验真实 Playnite/package-host、真实远端/进行中上传时序、物理 DPI/跨屏、UIA/IME、presented frame、ETW 或宿主性能；Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰、未合并；`.tmp/r12-07-build-final` 仍因 Access denied 暂留，未扩大清理范围。
- 2026-10-16 当前 main 补证：代码提交 `ccdbacb6` 新增隔离配置目录的 WorkerOptions 落盘/重载测试，暂停及允许时段恢复前后行为 `4/4`；Core 摘要 `1/1`，Playnite R13 与设置迁移 `24/24`，目标 net462；同一 Playnite 隔离输出 R09 `2/2`。测试只证明持久化数据重载与当前源码程序集行为，不启动真实 Worker Service、不取消真实进行中上传。SDK8.0.423 / MSBuild17.11.48 / VSTest17.11.1 定向项目构建；未跑完整 solution/完整 Playnite 全类扫描，且没有 XAML 变更。R00/R01 对 `ccdbacb6` freshness `14/14` 无需重跑、0 源路径命中；详见[R13-04 当前 main 复核包](evidence/R13-04-CURRENT-MAIN-RECHECK-20261016/README.md)。本阶段证据待推送后由该代码候选的 SDK8/SDK10.0.401 Actions 终态收口。
- R13-05/06/07/08 与 R14-01..08 已有当前 main 受控证据；后续依积压清单活动队列继续 `CLOSE-WRAP-01`，不因历史段落里的旧“下一项”重做已满足任务。

## 2026-09-19 Round3 R13-05 远端证据详情

- `e280cf1c` 先复用现有 `CloudTransferStatusDto`、`CloudTransferStateService` 的队列状态/远端布局和维护页详情入口，没有新建上传服务或改变上传、校验、取消语义。新增 display-only `CloudRemoteDisplay`，对 URI 用户信息、query/key-value secret、Bearer 值统一显示 `[已隐藏]`；Worker 按现有备份/媒体远端相对路径生成安全远端对象显示值，并补来源设备、最后尝试时间和最后成功校验时间。
- DTO 正负行为实际验证：空远端/设备/时间显示为“未知/未知设备”；带 URI 密码、token 和 Bearer 的远端对象不泄漏原值；Worker `RemoteVerified` 行以当前 `UpdatedUtc` 作为可证实的成功校验时间，转为 `Uploaded` 后历史时间恢复“未知”，不伪造队列未持久化的历史校验记录。既有 `CopyDiagnosticsCommand` 仍经 `ClipboardValueSanitizer`，认证参数负例实际不进入复制文本。
- 维护页在既有 `GscInspectorScrollViewer`、`GscRedesignSectionCard` 和表格滚动模型内增加四个绑定字段，没有改游戏选框、滚动条、命令/绑定、取消/错误/恢复保护、有限列表或 net462。WPF 质量检查仅按 Demo-first 约束复核共享样式、详情容器、换行/省略和 Tooltip；本阶段没有把离屏或静态结果写成真实视觉呈现通过。
- 验证：Core `2/2`；Worker 云状态映射 `1/1`；Playnite `R13CloudTransferStageBehaviorTests 10/10`；Release 隔离 solution `0 warning / 0 error`、Playnite `net462`；XAML `24/24`；`python scripts/validate-source.py`、`git diff --check` 通过。代码提交已推送 `origin/codex/ui-finesse-round2`。
- 证据仅来自合成 DTO、fake/隔离 SQLite、脱敏负例和外部隔离构建；未写真实云端、存档、媒体或诊断，未运行真实 Playnite/package-host、RenderHarness、物理 DPI/跨屏、UIA/IME、presented frame、ETW 或宿主性能。Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未触碰、未合并；本批 `.tmp/r13-05-*` 已清理，旧 `.tmp/r12-07-build-final` 仍因 Access denied 暂留且未扩大清理范围。
- 下一可执行小批量为 `R13-06 离线恢复反馈`：先查现有 Worker/维护页离线状态、恢复入口和错误分类，再补网络恢复前后状态变化与真实未验边界。

## 2026-09-19 Round3 R13-06 离线恢复反馈

- `e4244329` 复用现有 `CloudRetryPolicy` 与 `CloudRetryService`，没有重建队列或改变上传、校验、取消、错误和恢复语义；`CloudTransferStatusDto` 增加 display-only `NetworkRecoveryDisplay`，维护页沿用原有详情滚动容器和共享卡片样式。
- 网络失败且处于 `RetryScheduled` 时显示“等待网络恢复；按退避时间重试”，同时展示已用自动重试次数和本轮最多 10 项；进入 `Transferring` 时显示“网络已恢复；按批次上传中”。认证失败不套用网络恢复文案，避免把不可恢复错误误报成在线恢复。
- 源码与既有 Worker 测试确认：`CloudRetryPolicy` 的退避为 1/5/15/60/240/720 分钟、最多 6 次；`CloudRetryService` 每 30 秒检查、每轮最多取 10 项并顺序处理，且没有逐条 `ShowTaskNotification`。因此本阶段没有把恢复写成瞬时并发冲击，也没有新增旧失败逐条通知。
- 验证：Core 定向 `1/1`；Worker `CloudRetryPersistenceTests 10/10`；Playnite `R13CloudTransferStageBehaviorTests 11/11`；Release 外部隔离 solution `0 warning / 0 error`、Playnite `net462`；XAML `24/24`；`python scripts/validate-source.py`、`git diff --check` 通过。代码提交已推送 `origin/codex/ui-finesse-round2`。
- 证据只来自合成 DTO、fake/隔离 SQLite、源码边界测试和外部隔离构建；未运行真实网络/rclone/Playnite/package-host、RenderHarness、最终呈现、物理 DPI/跨屏、UIA/IME、presented frame、ETW 或宿主性能。Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未触碰、未合并；本批 `.tmp/r13-06-*` 已清理，旧 `.tmp/r12-07-build-final` 仍因 Access denied 暂留且未扩大清理范围。
- 下一可执行小批量为 `R13-07 队列筛选与汇总`：先核对现有状态筛选、全局计数、分页和选中项联动，再补筛选负例与汇总证据。

## 2026-09-19 Round3 R13-07 队列筛选与汇总

- `d6c2af90` 先复用现有状态/类型筛选、查询一致性 token、分页追加、`existingKeys` 去重和 `SelectionAnchorResolver.Restore`，再补游戏名称/Playnite ID 片段、来源设备和最近时间窗口筛选。Worker 同一候选集合同时计算当前筛选 `TotalCount` 与未筛选 `GlobalTotalCount`，没有改变上传、校验、取消、错误、恢复保护或有限列表语义。
- 维护页摘要明确“当前筛选 X/Y 项”与“全局 Y 项”，文本筛选使用既有 debounce，时间/状态/类型仍走既有选择变更入口；筛选栏改为可收缩列并给文本框保留最小宽度。RenderHarness 合成 ViewModel 同步新绑定，避免夹具缺属性。
- 已加入 Worker 合成 SQLite 行为夹具（游戏/设备/时间、错误设备负例、全局计数）和 Playnite 源行为夹具（绑定、摘要、分页去重、全局计数），但本阶段尚未执行这些新增定向测试。
- 验证：`python scripts/validate-source.py` 通过；XAML `24/24`；`git diff --check` 通过。尝试隔离 restore 时，本机仅有 .NET SDK `9.0.302`，`global.json` 的 `8.0.100` 向上滚动命中缺失的 Workload resolver 目录，Worker restore 退出 `1` 且没有 `project.assets.json`；linked `obj` 另有 Access denied。因此不把 Worker/Playnite build/test 或 Release/net462 写成通过。
- 证据只来自源码、XAML 结构门禁和新增合成夹具；未运行真实网络/rclone、Playnite/package-host、RenderHarness、最终呈现、物理 DPI/跨屏、UIA/IME、presented frame、ETW 或宿主性能。Demo 原目录不可用，沿用恢复生产基线；未写真实云端/存档/媒体/诊断；main 用户改动和 `src.zip` 未碰、未合并。`r13-07-build` 已清理，`r13-07-source` 首次清理时短暂被占用，阶段末已删除。
- 下一可执行小批量仍为 R13-07/R13-08 验证收口：在可用 SDK/Workload 的隔离输出目录重跑两项 Worker/Core/Playnite 定向夹具与相关回归；通过后再推进 `R14-01 归类建议解释`。

## 2026-09-19 Round3 R13-08 失败分类帮助

- `96a4c6a9` 复用现有 `RcloneFailureClassifier` 和稳定错误码，新增无空间与限流分类；限流仍走既有有界退避，未改变上传、取消、恢复保护、本地副本保留或通知语义。
- `CloudFailureExplanation` 只为已识别的认证、空间不足、远端不存在、校验差异和限流提供不同下一步；未知错误保持空解释。维护页把已识别帮助放入共享诊断提示，把原始错误码/详情放进默认折叠的“原始诊断”，保留可访问名称和审计信息。
- 已加入 Core 正例/未知错误负例、Worker 无空间/限流分类和 Playnite 详情绑定源行为夹具；验证：源码校验通过、XAML `24/24`、diff check 通过。受当前唯一 SDK `9.0.302` 缺失 Workload resolver 目录影响，Core/Worker/Playnite 定向测试、Release/net462、RenderHarness 尚未执行，不写成通过。
- 证据只来自源码、XAML 结构门禁和新增合成夹具；未运行真实 rclone/网络限流/远端配额、Playnite/package-host、最终呈现、物理 DPI/跨屏、UIA/IME、presented frame、ETW 或宿主性能。Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰、未合并。`r13-07-source` 首次清理时短暂被占用，阶段末已删除。
- 下一可执行小批量：在可用 SDK/Workload 环境同时补跑 R13-07/R13-08 定向测试和相关回归；通过后推进 `R14-01 归类建议解释`。

## 2026-09-19 Round3 R14-01 归类建议解释

- `7735cd7c` 先复用现有 `MediaSyncService.BuildClassificationSuggestion` 和本地证据源，新增 `MediaClassificationEvidenceDto`；来源规则、游戏会话、进程映射和文件名匹配都保留具体依据，多候选保留候选游戏，不生成目标；无依据显示“待判断”。
- Media 预览沿用既有 Inspector、有限高度、Recycling 虚拟化、滚动和命令确认/应用/撤销链。RenderHarness 合成数据加入有依据/无依据样本；Worker/Core/Playnite 源行为夹具覆盖正例与负例。本批没有移动、删除媒体或写真实存档/云端。
- 验证：`python scripts/validate-source.py`、XAML `24/24`、`git diff --check`、Contracts/Core Release 隔离构建 `0 warning / 0 error`。Core 定向测试未进入 testhost（项目引用目标框架评估退出 `1`），Worker restore 退出 `1`，Playnite/RenderHarness 未执行。
- 证据只来自合成/fake/隔离数据；未验真实 Playnite/package-host、最终呈现、物理 DPI/跨屏、UIA/IME、ETW 或宿主性能。Demo 原目录不可用，沿用恢复生产基线；R14 隔离构建目录已清理，此前 `.tmp/r13-verify-source` 曾短暂被占用，阶段末已精确删除，未强杀未知进程；main 用户改动未触碰、未合并。
- 下一可执行小批量：在可用 SDK/Workload 环境补跑 R13-07/R13-08/R14-01 定向夹具与相关回归，通过后推进 `R14-02 预览选择编辑`。

## 2026-09-20 Round3 R14-02 预览选择编辑

- `69cf2a72` 复用现有预览批次、稳定 `MediaId`、Worker 重验、冲突/取消/恢复和撤销链；新增 `IsIncluded`、高置信目标编辑和 `MediaClassificationTargetOverrideDto`。应用请求只携带当前纳入项的稳定 ID，排除项不会执行。
- 目标编辑只对已有高置信建议开放，并通过当前游戏目录的 `PlayniteId` 解析；Worker 仅在批次项目仍为 `Pending` 时写入覆盖目标和原因，非法目标跳过并保持未归类。目标原因写入批次记录后再进入原应用链，撤销仍可读取同一批次。
- 已加入 Core 选择状态、Worker 选中稳定 ID/合法目标行为和 Playnite XAML/命令绑定源契约夹具。`validate-source.py`、XAML `24/24`、`git diff --check`、Contracts/Core Release 隔离构建 `0 warning / 0 error` 通过；Core 测试项目在当前 SDK 下未产出可运行结果，Worker/Playnite/RenderHarness 未执行。
- 本批只用合成 DTO、fake 服务和隔离目录；没有写真实存档、媒体、云端或诊断。Demo 原目录不可用，沿用恢复生产基线；游戏选框、滚动条、命令绑定、取消/错误语义、恢复保护、有限列表和 net462 路径保持。`.tmp/r14-02-build` 已精确清理，main 用户改动和 `src.zip` 未碰、未合并。
- 下一可执行小批量：在可用 SDK/Workload 环境补跑 R13-07/R13-08/R14-01/R14-02 定向夹具和相关回归，确认目标覆盖与排除项的运行时行为后推进 `R14-03 部分成功处理`。

## 2026-09-20 Round3 R14-03 部分成功处理

- `aef251b1` 复用现有 `MediaSyncService.ReassignBatchAsync`、`IgnoreBatchAsync`、`RestoreIgnoredBatchAsync` 的逐项 best-effort 结果，不改变成功项的归类/忽略/恢复或媒体副本语义。ViewModel 保存失败项的稳定 `MediaId` 和逐项错误原因。
- Media 收件箱新增有限高度、Recycling 的失败明细列表和“仅重试失败项”命令；重试使用上次失败集合与原归类目标，不读取当前列表选择，不把已成功项重新提交。当前批次失败为零时提示区隐藏，避免伪造待处理状态。
- 已加入 Worker 合成 SQLite/fake 夹具，验证一个有效忽略项成功、一个缺失稳定 ID 项保留失败原因且成功项状态已更新；Playnite 源契约夹具覆盖失败集合、命令绑定和有限列表。源码校验、XAML `24/24`、diff check 通过；Worker/Playnite 测试未执行。
- 本批只使用合成/fake/隔离目录，未写真实存档、媒体、云端或诊断。Demo 原目录不可用，沿用恢复生产基线；游戏选框、滚动条、取消/错误、恢复保护、有限列表性能和 net462 路径保持；main 用户改动和 `src.zip` 未碰、未合并。
- 下一可执行小批量：在可用 SDK/Workload 环境补跑 R13-07/R13-08/R14-01/R14-02/R14-03 定向夹具和相关回归，确认真实 Worker/Playnite 行为后推进 `R14-04 撤销边界说明`。

## 2026-09-20 Round3 R14-04 撤销边界说明

- R14-04 先核对既有媒体归类批次历史和撤销链，确认实现已具备，因此只补边界行为证据，没有重建服务。历史页已有“撤销上次建议批次”和“撤销所选可回退批次”，`IsUndoable` 只允许仍有已应用项的 Applied/AppliedWithConflicts 批次。
- Worker 撤销逐项重新读取当前媒体并匹配应用后快照；目标、分类状态、归档路径或元数据变化即记录 Conflict，不执行恢复移动。持久层撤销提交还用当前目标、Assigned 状态、应用后归档路径和 Applied 批次项做条件更新，覆盖竞态失败进入冲突/恢复分支。
- 既有隔离夹具覆盖应用后重启 Store 的正常撤销。本阶段 `03521991` 增加“应用后人工修改收藏/备注再撤销”的负例，断言 `UndoneWithConflicts`、人工决定和应用后归档路径均保留。`validate-source.py`、XAML `24/24`、diff check 通过；定向 Worker testhost 当前长时间无输出，未获得运行时汇总，不冒充通过。
- 只用合成/fake/隔离目录，未触碰真实存档、媒体、云端或诊断；Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰、未合并。证据见 [R14-04 撤销边界说明](evidence/R14-04-UNDO-BOUNDARY-20260920.md)。
- 下一可执行小批量：先核对重复检测已有 hash/元数据能力，再推进 `R14-05 重复媒体识别视图`；R14-04 Worker/Playnite 运行时复跑仍待可用 SDK/Workload 环境。

## 2026-09-20 Round3 R14-05 重复媒体识别视图

- 先核对发现现有入库路径已经按 SHA-256 去重，media.sha256 有唯一约束，但没有用户可查看的重复组视图。本阶段复用现有 MediaItemDto/GetMediaAsync，新增当前游戏范围的有界只读重复检查。
- Worker 将非空 SHA-256 完全一致作为“确定重复”（兼容历史/异常数据），将同类型、文件名和大小一致且未被确定组占用的项目作为“疑似重复”；结果显示依据、稳定组 ID 和有限组内条目，不触发删除、移动或重新归类。
- Media 中心新增“重复识别”Tab，组与组内媒体均使用有限高度、FiniteViewport、Recycling；请求使用取消/generation 和重载命令，失败不阻塞主媒体详情。Worker/Playnite 隔离夹具已加入，源码校验、XAML 24/24、diff check 通过；linked obj Access denied/SDK-Workload 阻塞构建和运行时测试。
- 只用合成/fake/隔离数据，Demo 原目录不可用，沿用恢复生产基线；未触碰真实存档、媒体、云端或诊断，main 用户改动和 src.zip 未碰、未合并。证据见 R14-05 重复媒体识别视图（evidence/R14-05-DUPLICATE-INSPECTION-20260920.md）。
- 下一可执行小批量：先在可用 SDK/Workload 环境补跑 R14-04/R14-05 定向验证，再推进 R14-06 批量目标防误选。

## 2026-09-20 Round3 R14-06 批量目标防误选

- 先核对现有能力：全局 GamePickerViewModel 已按 PlayniteId 保留被搜索/筛选隐藏的选择并提供恢复入口；媒体归类目标已使用 Games、SelectedItem 和预览项的 TargetPlayniteId，没有重建选框或命令链。
- GameDescriptorDto/GameStatusDto 增加只读本地 IconPath 和 IdentityDisplay。Playnite 适配器复用现有 Database.GetFullFilePath(game.Icon) 解析存在的本地图标，Worker Dashboard、策略克隆和快照比较同步该字段；路径缺失或异常为空。
- 全局选框显示图标，缺失时回退首字母；媒体批量归类、预览目标覆盖和单项重新归类共用名称/平台/Playnite ID 模板，仍按对象/稳定 ID 选择，未使用索引选择。
- 新增 PickerItemsExposeStableIdentityAndOptionalPlayniteIcon 及同名对象按 Playnite ID 选择的行为夹具，并复跑 R14 目标显示/绑定契约夹具。`GamePickerViewModelTests 22/22`、`R14ClassificationSelectionTests 4/4`、`GamePickerKeyboardBehaviorTests 6/6`；隔离 Release XAML `24/24`、solution `0 error/2` 条既有 warning、Playnite `net462` 编译通过。
- 只用合成/fake/隔离源码，不下载图标，不读写真实存档、媒体、云端或诊断；Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 src.zip 未碰、未合并。证据见 R14-06 批量目标防误选（evidence/R14-06-TARGET-GUARD-20260920.md）。
- 下一可执行小批量：R14-06 受控条件已满足，继续保留真实宿主/UIA/呈现边界，推进 R14-07 媒体详情浏览。

## 2026-09-20 Round3 R14-07 媒体详情浏览

- 先查到现有 `Media` 已有稳定分页窗口、`SelectedMedia` 和选择/滚动锚点，`AsyncThumbnailImage` 已有取消、不可见/卸载取消以及 Missing/Unavailable/Failed 状态；本批只补详情导航、可见行回滚、视频失败回退和截图尺寸证据。
- 上一项/下一项命令只在当前已加载 `Media` 集合内按索引相邻移动，导航后由 `MediaGrid.ScrollIntoView`/容器 `BringIntoView` 恢复列表位置；不把“可加载更多”伪装成已实现的跨页详情导航。
- 详情显示已有媒体 DTO 的类型、来源、大小、采集时间；异步截图把解码位图的 PixelWidth/PixelHeight 显示为尺寸。视频本地路径/扩展名先验失败或 `MediaFailed` 时显示回退，不让详情错误撕裂列表。
- 当前 `89f07445` 重新生成身份一致的隔离产物后，选定 Playnite 套件 `46/46` 通过；隔离 Release XAML `24/24`、solution `0 error/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning，Playnite `net462` 编译通过。`validate-source.py`、XAML、diff check 通过；只用合成/fake/隔离路径，Demo 原目录不可用，沿用恢复生产基线。证据见 [R14-07 定向复核](evidence/R14-07-TARGETED-RECHECK-20260924.md) 与 R14-07 媒体详情浏览（evidence/R14-07-MEDIA-DETAIL-20260920.md）。
- 下一可执行小批量：R14-07 受控条件已满足，继续保留真实宿主/编解码/呈现边界，核对 R14-08 的既有实现与依赖。


## 2026-09-20 Round3 R14-08 来源规则试运行

- `89141528` 复用现有 `MediaSourceRuleDto`、媒体扩展名和 `MatchesIncludePattern`，新增只读来源规则草稿试运行 DTO、IPC 入口和 Worker 扫描。样本逐项给出命中/排除、大小、路径和原因；试运行不调用来源保存、媒体入库、移动或归类链。
- Worker 使用 linked cancellation token、100–5000ms 时间预算、1–5000 扫描项预算和 1–200 样本预算；数量截断/超时返回 `Partial`，不会把部分结果伪装成完整扫描。UI 试运行按钮只提交当前草稿，保存仍由原“添加来源”命令完成。
- 来源规则页沿用当前滚动、命令绑定、主题和选框系统，新增 `MaxHeight=240`、Recycling 的有限样本列表；同时把重复组卡片的两个 Border 子级收进单一 Grid，修复 WPF Border 单子级编译门禁。
- `validate-source.py`、XAML `24/24`、diff check 通过；Worker Release 隔离构建 `0/0`，Worker 行为测试 `1/1`，Playnite Release `net462` 契约测试 `1/1`。Playnite 构建仍有 `MediaCenterView.xaml.cs:664` 的 2 条既有 nullable warning。WPF 静态检查为 `0/28/177`；render-qa 的 linked `obj` 权限阻塞未写成呈现通过。
- 只用合成/fake/隔离目录和 SQLite，没有写真实存档、媒体、云端或诊断。Demo 原目录不可用，沿用恢复生产基线；main dirty 改动和 `src.zip` 未碰、未合并。证据见 [R14-08 来源规则试运行](evidence/R14-08-SOURCE-RULE-PREVIEW-20260920.md)。
- 下一可执行小批量：`R15-01 任务阶段可读`，先查现有 TaskCoordinator/任务事件阶段 DTO；R14-08 的真实 Playnite/RenderHarness、权限拒绝目录、超大目录和呈现边界仍待验。

## 2026-09-20 Round3 R15-01 任务阶段可读

- `4e7ac33a` 先核对现有 `TaskCoordinator`、任务 DTO、SQLite 查询和 `TaskCenterView`，复用已有 Worker 阶段事件；新增 Contracts 层共享 `TaskStageResolver`，将已有扫描、校验、索引、上传、下载、恢复、清理等阶段文案转换为可读阶段。
- `TaskStatusDto.StageMessage` 保存最后一个真实阶段事件，失败/取消时不被终态 `Message` 覆盖；任务详情单独保留错误和技术详情。运行中无可靠进度时显示 `—`，不以 0% 冒充真实百分比。
- SQLite `tasks.stage_message` 通过现有迁移入口补列，新增/更新/最近任务/分页查询均保留该字段；Task Center 增加阶段列和阶段详情，原任务命令、取消、滚动和有限列表边界未改。
- `validate-source.py`、XAML `24/24`、diff check 通过；Worker Release 隔离定向测试 `12/12`，Playnite Release `net462` 定向测试 `2/2`。Playnite 构建仍有 `MediaCenterView.xaml.cs:664` 的既有 nullable warning 2 条；WPF 静态检查 `0/28/177`。
- 只使用合成 DTO、fake/隔离持久层和隔离构建输出，未写真实存档、媒体、云端或诊断。Demo 原目录不可用，沿用恢复生产基线；未宣称真实 Playnite/RenderHarness、最终呈现、DPI/UIA/IME、ETW 或宿主性能通过；main 用户改动和 `src.zip` 未碰、未合并。证据见 [R15-01 任务阶段可读](evidence/R15-01-TASK-STAGES-20260920.md)。
- 下一可执行小批量：`R15-02 取消过程展示`，先核对取消请求、取消中状态和成功/取消竞争的现有终态收敛；真实宿主阶段事件全覆盖仍待验。

## 2026-09-20 Round3 R15-02 取消过程展示

- `9c8241fb` 先核对现有 `TaskCoordinator.Cancel`、取消 IPC、`TaskStatusDto` 和 Task Center 取消提示；保留原有 `TaskState`、命令绑定、取消入口、滚动系统、恢复/错误语义和 `net462` 路径。
- Contracts 增加共享取消阶段。运行时以任务闸门串行化取消请求和终态：第一次请求持久化/发布 `Requested` 后只调用一次令牌取消，再发布 `Finalizing`；已接受的成功/取消竞争收敛为 `Cancelled`，取消后业务失败收敛为 `NotInterruptible`，终态不会保留取消中状态。Task Center 详情增加取消状态卡，SQLite 查询、快照比较和任务复制列同步取消/阶段字段。
- `validate-source.py`、XAML `24/24`、diff check 通过；Worker Release 隔离项目定向测试 `14/14`；Playnite Release `net462` 构建 0 错误、定向测试 `8/8`。Playnite 构建保留 `MediaCenterView.xaml.cs:664` 的 2 条既有 nullable warning；WPF 静态检查为 `0/28/162`。
- 完整 solution 脚本在 linked worktree 生成 WPF 临时项目时遇到 `Access denied`，不记为完整 solution 通过；Worker 与 Playnite 项目已分别实际构建，四个本批隔离输出目录已清理。只用合成/fake/隔离目录，未写真实存档、媒体、云端或诊断；Demo 原目录不可用，沿用恢复生产基线。main 用户改动和 `src.zip` 未碰、未合并。证据见 [R15-02 取消过程展示](evidence/R15-02-TASK-CANCELLATION-20260920.md)。
- 下一可执行小批量：`R15-03 任务详情时间线`，先核对已有任务事件缓存、阶段字段和详情滚动容器；真实 Playnite/RenderHarness、最终呈现、DPI/UIA/IME、presented frame、ETW 和宿主性能仍待验。

## 2026-09-20 Round3 R15-03 任务详情时间线

- `fe0c05a9` 先核对现有 `TaskChangeEventDto`、`TaskCoordinator`、Worker 事件广播和 Task Center 详情，复用已有任务阶段/取消字段；没有另建历史服务，也没有把刷新快照当作重试记录。
- 变更事件增加 Worker 观察到的 `OccurredUtc`；Task Center 使用 `TaskTimelineBuilder` 整理已知创建、开始、阶段、取消和结束事件，按 UTC 再按序号稳定排序，同时显示本地时间与 UTC。缺少事件或时间显示“时间未知”，没有可关联的重试记录就不生成重试条目。
- Dashboard 只保留最多 64 条/任务、最多 200 个任务的运行期事件窗口；详情时间线卡为有限高度 `220 DIP`，沿用现有外层滚动条、选框、命令/绑定、取消/错误/恢复保护和 net462 路径。广播 clone 同步阶段和取消字段，避免实时事件丢失前两项真实状态。
- `validate-source.py`、XAML `24/24`、diff check 通过；Worker Release 隔离项目定向测试 `11/11`；Playnite Release `net462` 构建 0 错误、R06 取消回归 + R15-01/R15-02/R15-03 定向测试 `11/11`。Playnite 构建保留 `MediaCenterView.xaml.cs:664` 的 2 条既有 nullable warning；WPF 静态检查 `0/28/162`。
- 本批隔离输出目录已清理。只用合成 DTO、fake/内存事件和隔离构建，未写真实存档、媒体、云端或诊断；Demo 原目录不可用，沿用恢复生产基线；未宣称完整 solution、RenderHarness、真实 Playnite、重启后持久时间线、最终呈现、DPI/UIA/IME、ETW 或宿主性能通过。main 用户改动和 `src.zip` 未碰、未合并。证据见 [R15-03 任务详情时间线](evidence/R15-03-TASK-TIMELINE-20260920.md)。
- 下一可执行小批量：`R15-04 重复通知归并`，先核对现有任务通知、会话摘要和失败历史入口；保持重要新失败可见并为重复进度事件补屏幕通知负例。

## 2026-09-20 Round3 R15-04 重复通知归并

- `a67d371e` 先核对并复用既有 `BoundedTaskIdSet`、`SessionNotificationAccumulator`、`NotificationLevelPolicy`、Dashboard Toast 和 Task Center 历史；新增 `TaskNotificationDeduper`，不另建通知服务。
- 进度事件和非终态不领取通知键；终态按任务 ID、状态和失败证据归并。相同任务的相同失败证据只显示一次，不同错误码/错误消息仍分别保留；成功后又取消等不同终态不被错误吞并。摘要后的新失败/取消在重要级别也不静音，完整错误继续保留在 Task Center 历史。
- `validate-source.py`、XAML `24/24`、diff check 通过；Playnite Release `net462` 外部源码副本项目构建 0 errors、2 条 `MediaCenterView.xaml.cs:664` 既有 nullable warning；通知/会话/R15 时间线/R13 相邻夹具 `28/28`；WPF 静态检查 `0/28/177`。
- 本批未改 Worker；linked WPF 临时项目仍因 `Access denied` 未能直接构建，未宣称完整 solution/RenderHarness/真实宿主；未验真实 Toast/OS 通知、最终呈现、DPI/UIA/IME、ETW 或宿主性能。只用合成/fake/隔离数据，Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰、未合并。证据见 [R15-04 重复通知归并](evidence/R15-04-TASK-NOTIFICATION-DEDUPE-20260920.md)。
- 下一可执行小批量：`R15-06 耗时与吞吐`，先查已有可靠耗时/进度采样字段和未知总量语义；R15-05 仍待真实 Playnite 宿主验证来源卡片布局、键盘/UIA 焦点和删除对象提示。

## 2026-09-20 Round3 R15-06 耗时与吞吐

- `6f65638e` 复用现有 `TaskProgress`、`TaskStatusDto`、SQLite 任务查询和 Task Center 详情；新增明确工作量总量的可选采样。最多保留 5 个推进样本，至少两个推进样本才显示速率/ETA；15 秒没有推进重置窗口，10 秒没有新推进隐藏速率/ETA。普通阶段报告会清除旧采样，避免跨阶段推算。
- 只给整库备份的游戏数、游戏专属媒体候选文件数和已知总字节下载接入采样；远端 rclone、恢复写入和未知总量保持未知。详情中的“可靠进度采样”卡片按 DTO 条件折叠，已有耗时继续基于开始/结束时间显示；刷新比较器、广播 clone、SQLite 旧库迁移均保留新增字段。
- 最终提交外部隔离 Release solution 到达 Playnite `net462`，`0 errors/2` 条既有 nullable warning；Core `106/106`；Worker 定向 `20/20`；Playnite R15 `11/11`；source/XAML/diff 门禁通过；WPF 静态 `0/28/162`。Worker 全量门禁仍为 `342 passed/1 skipped/1 failed/344 total`，失败为既有 `MediaSyncServiceTests.cs:570`，未改写为通过。
- 只使用合成/fake/隔离 SQLite/目录；未验真实 Playnite/package-host、RenderHarness、最终呈现、DPI/UIA/IME、presented frame、ETW 或宿主性能；linked `obj` 仍有 `Access denied`，Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰、未合并。证据见 [R15-06 耗时与吞吐](evidence/R15-06-TASK-THROUGHPUT-20260920.md)。
- 下一可执行小批量：`R15-07 失败结果复制`，先核对现有任务摘要、错误码、脱敏详情和剪贴板失败语义；R15-06 保留真实宿主呈现与 Worker 全量既有失败边界。

## 2026-09-20 Round3 R15-07 失败结果复制

- `37dd4a03` 先核对并复用既有 `CopyTaskErrorCommand`、任务错误字段、恢复报告脱敏文本和剪贴板入口；新增共享 Contracts 脱敏器、短摘要 `FailureSummary`、安全技术详情 `SafeDetailMessage`、可测试的完整复制格式化器和 4 次瞬时占用重试。完整复制继续保留 `ErrorMessage`、`ErrorCode`、`DetailMessage` 和任务 ID，不因视觉去重削弱诊断。
- Task Center 失败卡现在先显示脱敏首行摘要和错误码；技术详情仍默认收起，改用生产 TextBox 样式的只读有限高选择区，避免长堆栈撑开任务页；游戏选框、滚动条、命令/Binding、取消/错误/恢复保护和 net462 不变。
- `R15TaskFailureCopyTests` `6/6`，R06/R12/R15 相邻回归 `14/14`；外部隔离 Release solution `7 warnings/0 errors`（均为离线 NuGet `NU1900`），Playnite `net462` 定向构建保留 2 条既有 `MediaCenterView.xaml.cs:664` warning；XAML `24/24`、source/diff、WPF `0/28/162` 通过。R06 旧测试夹具先按旧列顺序失败，已同步当前“阶段”列后重跑通过。
- 只用合成 DTO、fake 剪贴板 setter、隔离 STA WPF 和外部源码副本，未写真实存档、媒体、云端、诊断或系统剪贴板；Demo 原目录不可用，沿用恢复生产基线。未验真实 Playnite/package-host、RenderHarness presented frame、物理 DPI/跨屏、UIA/IME、ETW 或宿主性能；linked `obj` 的 `Access denied` 仍是环境边界。main 用户改动和 `src.zip` 未碰、未合并。证据见 [R15-07 失败结果复制](evidence/R15-07-TASK-FAILURE-COPY-20260920.md)。
- 下一可执行小批量：`R15-08 清理历史范围`，先核对已有历史清理命令、运行中任务保护、恢复账本以及日期/状态预览与真实执行集合的一致性。

## 2026-09-20 Round3 R15-08 清理历史范围

- 先核对既有 `RetentionSimulationService`、预览 DTO、维护页和 SQLite 隔离账本；没有重建清理服务。全局预览已有 `PreviewId`、生成时间、现有/保留/候选数量、预计释放、锁定/健康恢复点/PreRestore 影响统计和隔离账本占用，候选明细保留日期、路径和原因，维护页实际绑定日期/原因/摘要并将列表限制为最多 200 条、有限高 240。
- 应用要求明确确认且只接受 Worker 持有的预览句柄；预览过期、时间不一致、候选/策略/文件身份变化都会拒绝。执行按游戏取得与备份、恢复、媒体共用的 `GameOperationKind.Retention` 锁，忙碌游戏跳过；清理仅处理备份根目录内 ZIP，保护版本和必要恢复账本不被删除，隔离账本支持重启后逐条协调或人工确认。
- `RetentionSimulationServiceTests` + `RetentionQuarantineRecoveryTests` 在当前提交外部隔离副本 `16/16`；Playnite `MaintenanceReportSourceTests` `3/3`；`validate-source.py`、XAML `24/24`、diff check 通过。本批没有生产 XAML 改动，沿用上一批 WPF `0/28/162` 静态结果。一次陈旧剪贴板源码断言已按 R15-07 现行重试入口修正并重跑通过。
- 未验真实 Playnite/package-host、RenderHarness presented frame、DPI/跨屏、UIA/IME、ETW 或宿主性能；隔离副本已清理，未写真实存档/媒体/云端/诊断/系统剪贴板，Demo 原目录不可用；main 用户改动和 `src.zip` 未碰、未合并。证据见 [R15-08 清理历史范围](evidence/R15-08-HISTORY-CLEANUP-SCOPE-20260920.md)。
- 下一可执行小批量：`R16-01 设置搜索定位`，先查现有设置页筛选/分组/滚动和命令绑定，再决定是否需要实现；R15-08 的真实宿主呈现和性能仍待验。

## 2026-09-20 Round3 R16-01 设置搜索定位

- `a4e35578` 在现有设置页上复用原分类、字段控件和 Binding，增加轻量搜索框、结果摘要以及 `SearchTerms` 附加属性；构造时登记五个分类的匹配宿主，不复制设置服务或 DTO。搜索只改变匹配字段/分类的可见性，首次输入记录原分类，清空后恢复原分类和字段；验证错误定位会先清空搜索再沿用已有焦点/滚动路径。
- 受控 STA WPF 行为 `R16SettingsSearchBehaviorTests` `1/1`：合成设置和隔离目录中，搜索“恢复巡检间隔”后自动化分类和可编辑数值字段可见，Worker 字段隐藏，原配置路径不变；清空后回到原分类、字段恢复且无 pending edit。源契约 `1/1`，既有验证导航/草稿夹具分别独立 `1/1`。
- 外部隔离副本 Release solution 单节点构建 `0 errors/10 warnings`；警告仅为离线 NuGet `NU1900` 和既有 `MediaCenterView.xaml.cs:664` nullable warning。`validate-source.py`、XAML `24/24`、diff 通过；WPF 静态 `0/28/177`。联合 WPF 筛选因既有 Application 多实例生命周期夹具出现 `2 passed/2 failed`，已拆成独立 testhost，未写成联合通过。
- 保留游戏选框、滚动条、命令/Binding、保存取消、错误/恢复保护、有限列表性能和 net462；未验真实 Playnite/package-host、最终呈现、浅深主题截图、DPI/UIA/IME、ETW 或宿主性能。只用合成/fake/隔离目录，Demo 原目录不可用，linked `obj` 仍 `Access denied`，main 用户改动和 `src.zip` 未碰、未合并。证据见 [R16-01 设置搜索定位](evidence/R16-01-SETTINGS-SEARCH-20260920.md)。
- 下一可执行小批量：`R16-02 策略差异预览`，先查现有策略/模板 DTO、继承与显式覆盖来源，限定为只读差异、取消无写入和保存字段范围；R16-01 的真实宿主呈现和联合 WPF 夹具边界继续保留。

## 2026-09-20 Round3 R16-02 策略差异预览

- `b327d5ef` 复用现有策略/模板 DTO、模板目录归一化、Worker IPC 和保存基线，新增 13 字段有界差异服务与 `BackupPolicyDto` 字段通知；保存卡分开展示已保存基线和显式草稿，模板卡展示应用前覆盖差异。模板仍为一次性复制，不建立实时继承。
- “取消未保存修改”只把保存基线复制回当前游戏草稿，不调用 `RequestAsync`；保存继续走现有 `SavePolicyAsync`。未保存游戏草稿存在时模板应用命令禁用，防止刷新/应用覆盖本地草稿。
- 外部隔离 Release solution `0 errors/7 warnings`（警告均为离线 NuGet `NU1900`）；策略差异/复制/通知核心 `5/5`，Playnite 源契约 `1/1`；`validate-source.py`、XAML `24/24`、diff 通过；WPF `0/28/177`。未把源契约断言当作真实交互或宿主呈现通过。
- 保留游戏选框、滚动条、命令/Binding、取消/错误/恢复保护、有限列表和 net462；未验真实 Playnite/package-host、浅深主题最终呈现、RenderHarness presented frame、DPI/UIA/IME、ETW 或宿主性能。只用合成/fake/隔离目录，Demo 原目录不可用，linked `obj` 仍 `Access denied`；main 用户改动和 `src.zip` 未碰、未合并。证据见 [R16-02 策略差异预览](evidence/R16-02-POLICY-DIFF-20260920.md)。
- 下一可执行小批量：`R16-03 模板应用范围`，先核对现有模板 DTO、应用命令和一次性复制边界，再补目标范围/取消与负例证据。

## 2026-09-20 Round3 R16-03 模板应用范围

- `52fbf5de` 复用现有 `BackupPolicyTemplateDto`、`BackupPolicyDto`、模板目录归一化、Worker IPC 和单游戏应用边界，增加批量请求/结果 DTO 与 `policy.template.batch.apply` IPC；客户端只发送明确勾选的稳定 Playnite ID，最多 100 个，空选择和超限均拒绝，不把筛选结果或空列表解释为全库。
- Save 页面模板卡增加有限高目标清单、名称/稳定 ID 筛选、目标/排除/预计变更字段数摘要。筛选只改变可见项，已勾选项按稳定 ID 保留；应用前二次确认列出目标、排除数量与变更数。Worker 每个目标单独取得游戏操作锁、写策略和审计；非取消异常落在逐项结果中继续处理，失败项可单独重试，取消仍向上传播。
- 合成核心测试覆盖稳定 ID 去重、排除项、空选择不全选、变更字段计数和超过 100 个不静默截断；Playnite 源契约覆盖绑定、命令、结果/重试入口、显式目标路径与 Worker 逐项失败结构。外部隔离 Release solution `0 errors/2 条既有 warning`；R16-03 Core `3/3`、Playnite `1/1`、Worker `2/2`；`validate-source.py`、XAML `24/24`、diff 通过，WPF 静态 `0/28/177`。
- 本批保留游戏选框、现有滚动条系统、命令/Binding、取消/错误/恢复保护、有限列表性能、模板一次性复制和 Playnite `net462`。没有把源契约测试写成真实交互或视觉通过；未验真实 Playnite/package-host、最终浅深主题、DPI/UIA/IME、RenderHarness presented frame、ETW 或宿主性能。Demo 原目录不可用，fresh restore 在隔离副本无诊断退出，构建复核使用现有 `obj` 资产和 `--no-restore`；main 用户改动和 `src.zip` 未碰、未合并。证据见 [R16-03 模板应用范围](evidence/R16-03-POLICY-TEMPLATE-BATCH-20260920.md)。
- 下一可执行小批量：`R16-04 恢复默认粒度`，先核对设置页/设置服务是否已有单字段、单分类、全部默认入口和敏感连接字段保护，再决定复用、补行为测试或记录“已满足”。

## 2026-09-20 Round3 R16-04 恢复默认粒度

- `2b194461` 核对后确认原设置页只有 Playnite 整体保存/取消和导入校验，没有单字段、单分类或全部默认入口；新增 `SettingsResetCatalog`，把安全标量字段分为常规与目录、备份与恢复、外观与可访问性、自动化与媒体四类。
- 设置页新增单字段下拉、四个分类级“恢复本类默认”和“恢复全部默认”。确认文案分别列出字段/分类/全部影响；全部默认额外清理筛选预设、列宽、最近访问等本地 UI 偏好。Worker/Ludusavi/Rclone 可执行文件、存档/媒体/镜像路径、Rclone 云端目标和设备身份不在可重置字段中，避免敏感连接或路径被无意清空。
- 重置直接修改现有 Playnite 编辑草稿，随后重绑同一 settings 实例刷新自动属性，但不调用 `EndEdit`、不保存、不启动 Worker。行为测试验证全部默认保留连接字段且 Playnite 取消恢复原草稿；源码测试验证三个范围的 UI 接线和编辑生命周期保护。
- 外部隔离 Release solution `0 errors/2 条既有 warning`；R16-04 行为/取消 `2/2`、源码接线 `1/1`；`validate-source.py`、XAML `24/24`、diff 通过，WPF 静态 `0/28/177`。本阶段使用隔离副本已有 `obj` 的 `--no-restore`，未把 fresh restore 写成通过；Demo 原目录不可用，真实 Playnite/package-host、最终呈现、DPI/UIA/IME、RenderHarness presented frame、ETW 和宿主性能仍待验；main 用户改动和 `src.zip` 未碰、未合并。证据见 [R16-04 恢复默认粒度](evidence/R16-04-RESET-GRANULARITY-20260920.md)。
- 下一可执行小批量：`R16-05 路径编辑一致`，先查现有路径浏览、校验、打开、复制事件与 `SettingsPathValidationService`，确认网络/不存在/无权限负例后再决定实现或“已满足”。

## 2026-09-20 Round3 R16-05 路径编辑一致

- `955dc52e` 核对确认原设置页虽有全量异步 `SettingsPathValidationService`、粘贴标准化和导入/导出，但没有统一的当前路径浏览、单字段校验、打开和复制入口；新增六个本地工具/目录字段共用的“统一路径编辑”卡片，Rclone 云端目标保留远端文本语义并排除在本地打开之外。
- 浏览按可执行文件/目录分别使用文件或文件夹选择器；取消选择不改草稿。`SettingsPathEditorService` 先用只读属性探测区分有效、缺失、网络/磁盘不可达、类型错误和无权限，再做只读目录枚举；打开必须通过当前字段有效探测，不回退到父目录；复制复用 `ClipboardValueSanitizer` 与 `ClipboardRetry`，失败保留字段并反馈。
- 合成隔离行为、既有路径回归和源契约定向 `6/6`；外部隔离 Release solution `0 errors/2 warnings`，warning 为既有 `MediaCenterView.xaml.cs:664` nullable；`validate-source.py`、XAML `24/24`、diff、WPF `0/28/162` 通过。未用真实网络共享、用户 ACL、剪贴板或 Explorer；未验真实 Playnite/package-host、最终呈现、DPI/UIA/IME、RenderHarness、ETW 或宿主性能。Demo 原目录不可用，main 用户改动和 `src.zip` 未碰、未合并。证据见 [R16-05 路径编辑一致](evidence/R16-05-PATH-EDITOR-20260920.md)。
- 下一可执行小批量：`R16-06 生效条件说明`，先查设置字段实际消费点、保存/应用/重启边界，再补不笼统的生效提示和负例证据。

## 2026-09-20 Round3 R16-06 生效条件说明

- `83e7c745` 追踪现有设置消费链：Playnite `GameSaveCenterSettings.EndEdit` 保存后触发视觉变更并调用 `ApplySettingsAsync`，插件通过 `settings.update` 发送 `ToWorkerSettings`，Worker `UpdateSettings` 应用运行时选项并重算健康巡检计划。没有新增另一个配置源或要求重启的泛化规则。
- 设置页四个分类标题旁标注实际边界：外观当前页即时预览、保存后已打开页面即时重建；工具/目录和备份参数从下一次新任务读取；轮询/队列/健康计划在下一轮边界读取；随 Playnite 启动 Worker、下次安全模式在下一次启动判断。已有云端时段文案继续保留“下一轮 Worker 检查”。
- 当前提交重新编译后的链路/负例与 R16-05 路径回归 `8/8`；外部隔离 Release solution `0 errors/2 warnings`，warning 为既有 `MediaCenterView.xaml.cs:664` nullable；source/XAML/diff、WPF `0/28/162` 通过。未验真实宿主保存后时序、最终呈现、DPI/UIA/IME、RenderHarness、ETW 或宿主性能；Demo 原目录不可用，main 用户改动和 `src.zip` 未碰、未合并。证据见 [R16-06 生效条件说明](evidence/R16-06-SETTINGS-EFFECT-CONDITIONS-20260920.md)。
- 下一可执行小批量：`R16-07 配置导入预览`，先复用现有 `ImportPortableJson`/导入报告能力，核对版本、未知字段、凭据和失败回退边界。

## 2026-09-20 Round3 R16-07 配置导入预览

- `451195ad` 复用现有 `ExportPortableJson`、`ImportPortableJson` 与缺失路径报告，新增 detached `PreviewPortableJson` 和确认后的 `ApplyPortableJson`；预览展示架构版本、兼容性、实际变化字段、未知字段和安全说明，设备身份不被导入。
- 设置页按读取→预览→Yes/No→应用→报告执行；取消、旧架构、坏值不写入当前草稿，应用前快照在复制/报告异常时恢复。导出仍不包含当前 DTO 的凭据字段，测试锁定 `RclonePassword`、Password/Secret/Token 文本不出现在可分享 JSON。
- 最终 HEAD 定向 `15/15`，隔离 Release solution `0 errors/2 warnings`（既有 `MediaCenterView.xaml.cs:664` nullable），`validate-source.py`、XAML `24/24`、diff、WPF `0/28/162` 通过。未验真实宿主文件对话框/确认框/保存取消、最终呈现、DPI/UIA/IME、RenderHarness、ETW/性能；Demo 原目录不可用，main 用户改动和 `src.zip` 未碰、未合并。证据见 [R16-07 配置导入预览](evidence/R16-07-SETTINGS-IMPORT-PREVIEW-20260920.md)。
- 下一可执行小批量：`R16-08 保存冲突处理`，先查 Playnite 编辑基线、后台设置更新和字段级冲突/拒绝边界。

## 2026-09-20 Round3 R16-08 保存冲突处理

- `ee6b37c9` 复用 Playnite `BeginEdit`/`CancelEdit` 基线和 fingerprint，新增 `SettingsConflictResolver` 三方字段合并；仅最新持久化变化的字段并入草稿，同字段不同值列为冲突，不做部分覆盖。
- `EndEdit` 保存前读取最新 settings；无冲突继续原保存、视觉通知和 Worker 应用；冲突触发 `SettingsConflictDetected`，设置页显示字段名与“当前草稿未写入”，并抛出 `SettingsConflictException` 阻断保存，取消基线保留最新外部值。
- 最终 HEAD 定向 `17/17`，隔离 Release solution `0 errors/2 warnings`（既有 `MediaCenterView.xaml.cs:664` nullable），`validate-source.py`、XAML `24/24`、diff、WPF `0/28/162` 通过。未验真实双设置窗口、共享对象竞态、宿主错误呈现、最终呈现、DPI/UIA/IME、RenderHarness、ETW/性能；Demo 原目录不可用，main 用户改动和 `src.zip` 未碰、未合并。证据见 [R16-08 保存冲突处理](evidence/R16-08-SETTINGS-CONFLICT-20260920.md)。
- 下一可执行小批量：`R17-01 健康结果分层`，先查健康检查结果、已解决项和跨来源去重时间证据。

## 2026-09-19 R00/R01 当前提交复核

- 当前续作分支的干净源码身份为 `3354fd82400df6659165a688b8fcb1eb87116ca4`；对应复核证据为 [R00/R01 当前复核](evidence/R00-R01-CURRENT-RECHECK-20260919.md)，新鲜度报告为 [R01-07-freshness-report-20260919.json](evidence/R01-07-freshness-report-20260919.json)。
- 曾发现的旧证据身份 `c3e67cb4` 与隔离输出目录误认主工作树问题已修正；14 条 freshness 记录当前为 `14 fresh / 0 stale`，没有改变 R00/R01 的已满足状态，也没有将 `not-provided` 包身份写成安装或宿主通过。
- 本阶段实际验证包含当前提交的 Release/XAML、受影响源码行为、浅/深主题合成探针、审计索引和 freshness 负例；R00-07、R01-01、R01-07 原本已 fresh，未为了统一时间而重复构建。
- 真实 Playnite、物理 DPI/跨屏、IME/UIA、presented frame、ETW、宿主性能和 package-host 仍未验；Demo 原目录仍不可用，继续以恢复生产资源基线为视觉依据。main 工作树现有用户改动未合并、未覆盖。
- 下一可执行小批量：R08-08，检查并收口 `GscMotion` 的变换所有权与 Freezable 生命周期。

## 2026-10-12 CLOSE-PICKER 本机全量复验

- 对原删除回退 `Game 0000` 期望/`Game 0020` 实际的全量失败新增确定性 STA Dispatcher 负例；旧 `GamePickerViewModel` 在无 `SynchronizationContext` 时确实把 WPF 刷新执行在线程池。修复回投所属 Dispatcher，并让立即刷新、状态刷新、Dispose 取消 pending debounce；20ms 合并、选择回退、虚拟化和键盘路径保留。picker 类最终 `5/5`，完整隔离 WPF `114` 类中该类通过。
- R09-05 行为测试和生产模板的实际结构已对齐：错误描边是独立 `ValidationStroke` 两 DIP overlay，输入 `Chrome` 保持一 DIP。测试检查 overlay 的显示、厚度、命中状态及清除错误后的隐藏，不要求 Chrome 改两 DIP。原失败与最终通过 TRX 分别保留在 [CLOSE-PICKER 证据](../../../ai/evidence/close-picker-20261012/README.md)。R09-05 状态与 192 项总数不变。
- 完整 Release `0 warnings / 0 errors`、XAML `24/24`；Core `125/125`，Worker `356 passed / 1 skipped`，Playnite source `461 passed / 27 skipped`，WPF `608 passed / 39 skipped`、114 类全过；总计 `1,550 passed / 67 skipped / 0 failed`，脚本 exit0。本机 SDK8.0.423；Worker Named Pipe skip 保持环境限制。
- 测试程序集是 `e12119b` 加三个 dirty 源码/测试文件构建，摘要保留 dirty 状态与程序集 SHA/MVID，不冒充 clean post-commit。没有真实 Playnite、物理 DPI/跨屏、IME/UIA、ETW或最终帧验证；Demo 原目录不可用，继续沿用恢复生产基线。
- 最近 e121 基线 freshness 是 `4 fresh / 10 stale`，baseline 本轮未改；修复提交后重跑 freshness，并按 R00/R01 关联行为/负例逐项更新。推送后 Actions 新 run 未核验前不可报告远端阶段完成；旧 run `36677346317` 本次不可读，`36675309812` 日志仍403。Round3 任务状态/总数未变。
- 下一可执行批次：CLOSE-EVID-01，对 R00/R01 十条 stale sourcePaths 做精确补证和证据直达校正；真实宿主滚动、CI SDK10 与用户Q06/R08原始失败仍单列。

## 2026-09-19 R00/R01 合并后门禁纠偏

- main DEV-INSTALL-008 的真实结果仍是 Release `0/0`、Core `83/83`、Worker `311/311`、Playnite `73 failed / 588 passed / 57 skipped`、安装器退出 `1`，未进入打包/安装。首个 `SaveWorkspaceKeepsAllPrimaryCommandsReachableAtHighDpi` 失败来自列帮助属性插入后过期的连续字符串断言，不把它写成命令实际不可达。
- 当前 continuation 分支提交 `c975e16d` 只收口测试门禁、断言漂移和一个导航 namescope 早期空值保护：XAML 元素关系断言、最新状态源/字体/布局契约、Dispatcher 有界行为等待，以及按 WPF 类独立 testhost 的隔离脚本；未切换设计体系或覆盖 main。
- D 盘隔离 Release 构建 `0 warning / 0 error`、XAML `24/24`；source `65` 类组和 WPF `84` 类进程全部通过；资源字典类 `137/39/0`，Core `84/0/0`，Worker `322/1/0`；source validation、XAML check、diff check 通过。证据：[R00/R01 门禁纠偏](evidence/R00-R01-TEST-GATE-CORRECTION-20260919.md)。
- R00/R01 账面状态不因这次校正自动改写；本阶段只补真实失败原因、当前分支证据与隔离门禁结果。仍未验 Playnite/package-host 安装、物理 DPI/跨屏、UIA/IME、presented frame、ETW 和宿主性能；Demo 原目录不可用，继续沿用恢复生产基线。
- 提交已推送 `origin/codex/ui-finesse-round2`；main 用户文件未触碰。下一可执行任务：R11-08 历史时间导航，先查已有实现和 Q 依赖后再做小批量。

## 2026-09-19 Round3 R11-08 历史时间导航

- 复用 `BackupVersionDto.CreatedUtc/CreatedLocal/BackupId` 和现有历史排序，新增本地日历范围过滤、清除范围、最近/更早跳转；独立 `CollectionViewSource` 只作用于历史表，保留 `Backups` 原集合供 A/B 比较选择与既有绑定使用。
- 行为覆盖本地今天/昨天/近 7 天、未知时间正负例、清除恢复全部、同秒稳定排序和独立视图契约。隔离 Release `0 warning / 0 error`、XAML `24/24`；定向组合 `148 passed / 39 skipped / 0 failed`。
- R11-08 已由 `8cc329e4` 提交并推送；证据：[R11-08 历史时间导航](evidence/R11-08-HISTORY-TIME-NAVIGATION-20260919.md)。main 安装失败事实和未验的 Playnite/package-host、呈现帧、物理 DPI/跨屏、UIA/IME、ETW、宿主性能边界不变。
- 下一可执行小批量：R12-01 恢复分步摘要。

## 2026-09-20 Round3 R20-01 概览下一步

- 先核对已有 `OverviewPriorityResolver`、快照、`GamePickerViewModel`、生产 Shell 和任务工作区；没有重建服务或 DTO，也没有把全库备份写命令接到概览英雄动作。
- 实现四类首屏状态的真实入口：失败任务设置任务中心“失败”筛选并显式刷新；空库沿用刷新；未匹配打开游戏选框“未匹配”；工具可用且无本地备份的已匹配游戏打开“可备份”。优先级由快照稳定字段计算，未匹配优先于可备份，无关警告计数变化不跳状态。
- `OverviewPriorityResolverTests` + `GamePickerViewModelTests` `32/32`，Overview/Shell 交互 `5/5`；Release 隔离 source-copy 编译 Playnite `net462` / Tests `net472` 通过，仅有既有 `MediaCenterView.xaml.cs:671` nullable warning。`validate-source.py`、XAML `24/24`、`git diff --check` 通过。
- 链接工作树写 obj/WPF 临时项目遇到 `Access denied`，未绕过权限；未运行真实 Playnite/Worker/外部工具、呈现、DPI/UIA/IME、ETW 或宿主性能，未写真实存档/媒体/云端。Demo 原目录不可用，main 用户改动未碰且未合并。证据见 [R20-01 概览下一步](evidence/R20-01-OVERVIEW-NEXT-ACTION-20260920.md)。
- 下一可执行小批量：`R20-02 指标统计范围`，先盘点概览数字的来源字段、更新时间与当前游戏/全库/未知未加载表达。

## 2026-09-20 Round3 R20-02 指标统计范围

- 先核对确认 `DashboardSnapshotDto` 已提供 `GeneratedUtc`、全库计数、云端/媒体计数和当前游戏 DTO；没有重建服务、DTO 或点击入口。新增 `OverviewSnapshotDisplay` 和 `DashboardViewModel` 投影属性，概览统计条显示“全库 · Playnite 游戏库 · 更新时间”，当前游戏卡片显示“当前游戏 · 与全库快照同步 · 更新时间”。
- 快照未成功返回时，概览数字、当前游戏摘要、最近访问/任务计数显示 `—`，并明确写“尚未加载；数字显示 —，不代表 0”；成功快照中的合法零保留为 `0`，默认生成时间显示“更新时间未知”。依赖快照的比例条和状态胶囊在未加载时隐藏，既有零分母隐藏规则与真实 Snapshot 来源保持。
- `OverviewSnapshotDisplayTests`、R20-01 核心行为与交互定向组合 `40/40`；概览相关 4 条 XAML/布局断言在隔离宿主条件下 `0 passed / 0 failed / 4 skipped`。更宽筛选 `194 passed / 3 failed / 50 skipped / 247 total`，失败为未改动的 `SettingsUsesSharedResponsiveFieldGroupsWithoutShrinkingNumericInputs`、`EmptyDataSurfacesExplainNextStepsWithoutBreakingLocalScrolling`、`FiniteWidthComboBoxesUseTheSharedLongTextTemplate`，不计为本项行为失败。
- Release 隔离 source-copy 编译 Playnite `net462` / Tests `net472` 通过，`0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671` `CS8602`；`validate-source.py`、XAML `24/24`、`git diff --check` 通过。链接工作树仍受 obj/WPF 临时项目 `Access denied` 阻塞，未绕过；未运行真实 Playnite、呈现、DPI/UIA/IME、ETW 或宿主性能，未写真实存档/媒体/云端。Demo 原目录不可用，main 用户改动未碰且未合并。证据见 [R20-02 指标统计范围](evidence/R20-02-METRIC-SCOPE-20260920.md)。
- 下一可执行小批量：`R20-03 首次配置引导`，先查现有环境检查、外部工具设置入口、已完成标记和取消/返回语义，确保不自动更改用户配置。

## 2026-09-20 Round3 R20-03 首次配置引导

- 追溯确认当前分支已包含 `1a90cd06` 的首次使用环境引导实现；没有重建新页面、DTO 或服务。维护页 `EnvironmentCheckCard` 提供最小步骤、逐项检查和最近检查时间；`OnboardingCompleted` 提供已完成标记，完成后卡片回到普通“环境检查”语气。
- `EnvironmentCheckService` 的已有非破坏性检查覆盖 Worker、数据/存档/媒体目录、SQLite、Playnite 游戏库、Ludusavi、可选 Rclone 和磁盘空间；完成设置要求最近检查且无失败，跳过会明确持久化“之后可重新运行”。检查卡不强制导航，维护工作区可返回；测试备份必须用户明确点击且复用 `BackupSelectedAsync`，不自动改配置或启动写入。
- Worker 隔离合成 `EnvironmentCheckServiceTests` `1/1`；Playnite 当前身份定向 `3 passed / 1 skipped / 0 failed / 4 total`，跳过项为 `LegacyProductionUiBaselineFact`。Playnite `net462` / Tests `net472` source-copy 构建 `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671` warning，Worker.Tests 构建 `0/0`；source/XAML `24/24`/diff 通过。
- 首次直接复用旧工作树产物的 3 条 Playnite 源测试被 `GscBuildCommit` 身份门拒绝，未计为行为失败；固定当前 `31784686` 后隔离重建并通过。未运行真实 Playnite/package-host、真实外部工具、存档/云端、呈现、DPI/UIA/IME、ETW 或宿主性能，未写真实用户目录；Demo 原目录不可用，main 用户改动未碰且未合并。证据见 [R20-03 首次配置引导](evidence/R20-03-FIRST-USE-ONBOARDING-20260920.md)。
- 下一可执行小批量：`R20-04 零结果恢复`，先核对任务/媒体/游戏选框的筛选状态摘要、清除条件和离线错误不能伪装零结果的路径。

## 2026-09-20 Round3 R20-04 零结果恢复

- 先盘点确认任务中心已有 `TaskActiveFiltersSummary`、`ClearTaskFiltersCommand` 和 `FilterEmpty` 状态；本阶段复用它们，没有重建任务筛选服务或 DTO。游戏选框新增共享 `FilterConditionSummary` 绑定，媒体空态复用 `WorkspaceDataState` 和 `ClearMediaFiltersCommand`，云端队列补齐条件摘要、清除命令及读取失败分流。
- 游戏选框空结果显示搜索/状态/平台条件并提供清除搜索、清除筛选；媒体空结果显示搜索/类型条件，清除只修改筛选字段；云端清除只重置五类队列筛选并重新加载。云端请求异常设置失败状态，空列表明确说明“这不是零结果”，保留刷新入口。
- `FilterConditionSummaryTests` + `GamePickerViewModelTests` `26/26`；受影响 UI/源码套件 `171 passed / 3 failed / 50 skipped / 224 total`，3 条失败为未修改的设置字段、空态覆盖层和受限下拉模板既有基线。Release 隔离 source-copy 编译 Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671` warning；source/XAML `24/24`/diff 通过。
- 只用合成/fake/隔离 testhost 和隔离目录，没有写真实存档、媒体、云端或诊断；未运行真实 Playnite/package-host、呈现、物理 DPI/跨屏、UIA/IME、ETW 或宿主性能。Demo 原目录不可用，main 用户改动未碰且未合并。证据见 [R20-04 零结果恢复](evidence/R20-04-ZERO-RESULT-RECOVERY-20260920.md)。
- 下一可执行小批量：`R20-05 Stale 可理解`，先核对任务、媒体、云端和维护页最后成功时间、失败原因、重试入口及 stale 时已有只读数据是否仍可见。

## 2026-09-21 Round3 R20-05 Stale 可理解

- 复核确认任务中心、存档、媒体和维护页已经提供 stale 状态、最后成功时间、失败原因与重试入口；缺口在云端队列：有旧记录时读取异常只保留表格，没有明确旧数据边界。本阶段只补云端状态提示。
- 云端队列记录最近成功读取时间和错误原因，新增 stale 横幅、失败摘要和重试命令；失败不清除旧记录、不改队列数据，也不遮挡现有只读详情。没有旧记录时继续使用 R20-04 的“不是零结果”失败空态。
- `FilterConditionSummary.StaleStateDetail` 定向及 R20-04 相关回归 `37 passed / 1 failed / 0 skipped / 38 total`；唯一失败是未改动的 `TaskCenterViewResponsiveTests.FailedTaskDetailsPutUserReasonBeforeCollapsedTechnicalDetails` 断言漂移。Release 隔离 source-copy 编译 Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671` warning；source/XAML `24/24`/diff 通过。
- 只用合成/fake/隔离 testhost 和隔离目录，没有写真实存档、媒体、云端或诊断；未运行真实 Playnite/package-host、呈现、物理 DPI/跨屏、UIA/IME、ETW 或宿主性能。Demo 原目录不可用，main 用户改动未碰且未合并。证据见 [R20-05 Stale 可理解](evidence/R20-05-STALE-UNDERSTANDABLE-20260921.md)。
- 下一可执行小批量：从最新 Q/R 依赖账本选择依赖已满足且未被既有基线阻塞的任务，先复用已有服务/DTO，再补行为负例与证据。

## 2026-09-21 Round3 R20-06 部分可用状态

- 先盘点确认现有 `ActionAvailabilityHints` 已按恢复、媒体收件箱、云端队列和隔离远端恢复分别共享命令前置条件；`WorkspaceStatePresenter` 已提供 Loading、Empty、Error、Degraded、Offline 等独立投影。没有重建服务或 DTO，也没有把新增表格方向误判成当前缺失。
- R02/R07/Workspace 定向复跑 `18 total / 17 passed / 0 failed / 1 skipped`；跳过项是已撤销旧今日工作台架构的 `LegacyProductionUiBaselineFact`。首次复跑捕获 Maintenance 云端过期横幅 `Style.BasedOn` 的真实 WPF `XamlParseException`，改为 `StaticResource` 后恢复通过，提交 `e32ed599`。
- 外部隔离 Release 构建显式绑定当前分支身份：Playnite `net462` / Tests `net472`，`0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671` `CS8602`；source/XAML `24/24`/diff 通过。测试关闭阶段仍有已知 TextServices COM 清理噪声，但 testhost 最终成功退出。
- 证据来自合成/fake/隔离 testhost/source-copy；真实 Playnite/package-host、Worker/Named Pipe、Ludusavi/Rclone/云端、最终呈现、物理 DPI/跨屏、UIA/IME/ETW/宿主性能仍待验。Demo 原目录不可用，main 用户改动未碰、未合并。证据见 [R20-06 部分可用状态](evidence/R20-06-PARTIAL-AVAILABILITY-20260921.md)。
- 下一可执行小批量：`R20-07 最近活动密度`，先核对已有任务事件摘要、重复进度合并、完成/失败详情展开和真实计数来源。

## 2026-09-21 Round3 R20-07 最近活动密度

- 先盘点确认 Worker 已将 active/recent 同一 `TaskId` 去重并稳定排序，`ActivityTimelineMapper` 将审计映射为有限字段摘要；Playnite `TaskEventUiBatcher` 按 TaskId 合并高频进度、限制 128/32 队列与批次，成功/失败/取消终态即时落地。
- 首页只显示最近 8 个任务，使用 Recycling 和有限本地滚动，计数绑定实际 `OverviewTasks.Count`；TaskCenter 通过 `SelectedTask` 保留失败摘要/错误码、技术详情 Expander、复制/重试和 220 DIP 有界时间线。没有新增状态模型或动画刷屏路径。
- Core 活动映射 `4/4`；Playnite 定向 `11/11`（TaskEvent `3/3`、TaskTimeline `3/3`、首页活动 `1/1`、详情/选择 `4/4`）。首次组合 WPF testhost 的 R06 详情选择失败在独立类进程复跑 `2/2`，记录为现有 testhost 时序边界，未把它写成产品失败。
- 隔离 Release 构建显式绑定 `ee327e80`：Playnite `net462` / Tests `net472`，`0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671` `CS8602`；source/XAML `24/24`/diff 通过。证据来自合成/fake/隔离 testhost/source-copy，真实宿主、Worker 实时流、最终呈现、DPI/UIA/IME/ETW/宿主性能仍待验。Demo 原目录不可用，main 用户改动未碰、未合并。证据见 [R20-07 最近活动密度](evidence/R20-07-RECENT-ACTIVITY-DENSITY-20260921.md)。
- 下一可执行小批量：`R20-08 状态语气统一`，先核对现有加载、失败、空、完成和需要操作文案的来源/模板，补正向与负例证据。

## 2026-09-21 Round3 R20-08 状态语气统一

- 先核对已有能力：`WorkspaceStatePresenter`、`ActionAvailabilityHints` 和 `OverviewPriorityResolver` 已覆盖本项状态投影、动作前置与概览优先级；没有重建服务或 DTO。实际修正集中在 Shell 概览副标题和主状态文案，不改变命令、Binding、取消/错误、恢复保护、选框、滚动条与有限列表。
- `176183ec` 让概览副标题随 `OverviewPriorityTitle` 更新，不再固定“一切运行正常”；加载、失败、空、完成和需要操作的主状态统一说明当前事实与下一步，并补 `Worker`/`Rclone` 不应出现在主解释中的负例。技术诊断区仍保留必要的内部细节。
- 定向 `30 passed / 0 failed / 0 skipped / 30 total`；相关较宽套件 `25 passed / 1 skipped / 1 failed / 27 total`，唯一失败为未修改的任务详情旧绑定源断言。隔离 Release Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 warning；source/XAML/diff 通过。
- 证据仅来自合成/fake/隔离 testhost/source-copy/目录；真实 Playnite/package-host、Worker/工具/云端、呈现、物理 DPI/跨屏、UIA/IME、ETW、宿主性能仍待验。Demo 原目录不可用，main 用户改动未碰、未合并。证据见 [R20-08 状态语气统一](evidence/R20-08-STATE-TONE-20260921.md)。
- 下一可执行小批量：`R21-01` 八入口纯键盘，先核对 Q24/UIA 键盘行为和现有入口能力，继续补行为与负例证据。

## 2026-09-21 Round3 R21-01 八入口纯键盘

- 先复核 Q24 与现有键盘/焦点能力；最新实现已经提供八个生产入口的导航与安全动作。本阶段只补 TrainerCenter 工具栏四个已有命令的稳定 Automation 名称，并新增受控 WPF 焦点轨迹测试，没有新造服务、DTO、命令或导航模型。
- `R21KeyboardNavigationTraceTests` 在 STA WPF `Window` 中逐页创建 Dashboard Shell、Overview、SaveCenter、TrainerCenter、MediaCenter、TaskCenter、Maintenance、Settings，实际走前向/反向焦点遍历，记录范围内停靠点并检查安全命令/导航名称；R21 新增 `2/2`。
- 相关焦点/键盘/无障碍/生产壳层回归在显式构建身份下 `31/31`；R05 和 GamePicker 既有夹具覆盖弹层/ComboBox 的方向键以及游戏选框的 Enter/Esc 和焦点返回。隔离 Release `net462/net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671` warning；`validate-source.py`、XAML `24/24`、diff 通过。
- 证据来自生产 WPF 视图、合成宿主资源、fake/隔离 testhost/source-copy；真实 Playnite/package-host、OS 输入、UIA/读屏、IME、物理 DPI/跨屏、最终呈现和宿主性能仍待验。Demo 原目录不可用，main 用户改动未碰、未合并。证据见 [R21-01 八入口纯键盘](evidence/R21-01-KEYBOARD-TRACE-20260921.md)。
- 下一可执行小批量：`R21-02` 控件名称与值，先盘点图标按钮、复合选择器、开关和进度条的 UIA 名称/状态/值，并补负例。

## 2026-09-21 Round3 R21-02 控件名称与值（部分收口）

- 先查现有 UIA/Automation 接线：Shell、媒体历史、任务预设等多数入口已有语义名称。本阶段只补明确可关联的进度值和任务筛选器名称，不改变数值、命令或 Binding：Dashboard 后台操作进度、Overview 三个进度条、Maintenance 存储占用、Trainer 下载进度，以及 TaskCenter 状态/类型/游戏筛选。
- `R21AutomationValueBehaviorTests` 以真实 WPF AutomationPeer 验证动作名不退化为 glyph，ToggleSwitch `Off → On` 状态可读，ProgressBar 当前/最小/最大值可读；新增 `2/2`。与 R21-01 相关套件合计 `33/33`。
- 隔离 Release `net462/net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671` warning；`validate-source.py`、XAML `24/24`、diff 通过。证据来自生产 XAML、合成 WPF 控件、隔离 source-copy/testhost；真实宿主/UIA/读屏/OS 输入/IME/物理 DPI/跨屏/呈现/性能仍待验。Demo 原目录不可用，main 用户改动未碰、未合并。证据见 [R21-02 控件名称与值](evidence/R21-02-AUTOMATION-VALUE-20260921.md)。
- 当前不签收整项 R21-02：本段记录的初批之后，SaveCenter 外置标签开关和复合选择器已在续作小批量补证；TaskCenter DataGrid 内重复进度条、Maintenance 远端备份进度及其状态/值负例仍待继续。
- 下一可执行小批量：继续 `R21-02`，先处理 TaskCenter/Maintenance 进度条名称和值与负例；完成后再进入 `R21-03` 验证错误播报。

## 2026-09-21 Round3 R21-02 SaveCenter 控件名称与值（续作小批量）

- `efa9614f` 复用 SaveCenter 现有 ToggleSwitch、ComboBox、CheckBox、命令和 Binding，为四个存档策略开关、异常保护等级、策略模板、锁定所选版本和两个策略动作补稳定 Automation 名称；未新增服务、DTO、命令或数值语义。
- `R21AutomationValueBehaviorTests` 当前 `4/4`；受控 STA WPF peer 实际检查四个 ToggleSwitch 的名称和 `Off → On`、异常保护 ComboBox 的名称和选中值变化；相关 R21 键盘/焦点/无障碍/生产壳层回归 `35/35`。
- 隔离 source-copy Release Playnite `net462` / Tests `net472` 无错误；首次编译只出现 `MediaCenterView.xaml.cs:671` 的 2 条既有 `CS8602` warning，后续 no-restore build summary `0/0`。`validate-source.py`、XAML `24/24`、diff 通过；WPF 静态检查 `0/27/177`。
- 证据来自生产 XAML、绑定关联契约、合成控件和隔离 testhost；真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现和宿主性能仍待验。Demo 原目录不可用，main 用户改动未碰、未合并；source-copy/build 目录已清理。证据见 [R21-02 SaveCenter 控件名称与值](evidence/R21-02-SAVECENTER-AUTOMATION-20260921.md)。
- 在本批开始时不签收整项 R21-02：当时 TaskCenter DataGrid 重复进度条、Maintenance 远端备份进度、其余复合选择器和逐控件状态/值负例仍待继续；该边界已由后续进度控件小批量继续处理，历史记录不代表当前待办。

## 2026-09-21 Round3 R21-02 进度控件名称与值（续作小批量）

- `74b559e2` 继续复用 TaskCenter 既有 `ProgressValue`/`ProgressDisplay` 和 Maintenance 既有 `RemoteBackupStageProgress`/`IsRemoteBackupStageActive`：为 DataGrid 行进度、所选任务详情进度和远端隔离下载进度补稳定 Automation 名称；HelpText 分别关联可读进度文本或远端百分比，没有改变 DTO、状态投影、命令、取消或恢复保护。
- `R21AutomationValueBehaviorTests` 仍为 `4/4`；相关组合筛选 `52/52` 通过，包含既有 R06 的未知/排队/运行零值/越界钳制/取消/成功终态和既有 R12 的远端下载、校验、成功等待恢复、取消/失败负例，未把源码字符串断言冒充为运行时行为。
- 当前 source-copy Release 构建为 Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602`；`validate-source.py`、XAML `24/24`、`git diff --check` 通过；WPF 静态检查 `0/27/177`，未见新增 warning。
- 证据来自生产 XAML、现有 DTO/投影行为、合成 WPF peer、fake/隔离 testhost/source-copy；真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现和宿主性能仍待验。Demo 原目录不可用，main 用户改动未碰、未合并；本批临时 source-copy/build 已清理。证据见 [R21-02 进度控件名称与值](evidence/R21-02-PROGRESS-AUTOMATION-20260921.md)。
- 当前不签收整项 R21-02：剩余复合选择器及逐控件状态/值负例仍需逐项核对。下一可执行小批量是继续盘点这些控件；完成公共门禁后再进入 `R21-03` 验证错误播报。

## 2026-09-21 Round3 R21-02 TrainerCenter 导入控件名称（续作小批量）

- `c327f92a` 复用 TrainerCenter 两个已有“导入确认”呈现路径、`ImportEntryCandidates`/`SelectedImportEntryCandidate` Binding、确认命令和取消命令，为选择器及确认/取消动作补稳定 Automation 名称；没有改变导入、取消或文件语义。
- `R21AutomationValueBehaviorTests` 当前 `5/5`；新增测试实际创建 WPF ComboBox/Button peer，检查名称并验证候选值从 A 切换到 B；相关组合筛选 `53/53` 通过。
- 隔离 source-copy Release Playnite `net462` / Tests `net472` 为 `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning；`validate-source.py`、XAML `24/24`、diff 通过；WPF 静态检查 `0/27/177`，未见本批新增 warning。
- 证据来自生产 TrainerCenter XAML、合成 peer、fake/隔离 testhost/source-copy；真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现和宿主性能仍待验。Demo 原目录不可用，main 用户改动未碰、未合并；本批临时 source-copy/build 已清理。证据见 [R21-02 Trainer 导入控件名称](evidence/R21-02-TRAINER-IMPORT-AUTOMATION-20260921.md)。
- 当前不签收整项 R21-02：下一可执行小批量是 Trainer 工具设置中的版本/风险/运行策略选择器与开关，再补逐控件状态值负例；完成公共门禁后进入 `R21-03`。

## 2026-09-21 Round3 R21-02 TrainerCenter 工具设置名称与状态（续作小批量）

- `5d9cb81c` 复用 TrainerCenter 既有工具设置 Binding、`CanTrackProcess` 禁用条件和 ToggleSwitch 实现，为版本、已有实例处理方式、风险类别三个选择器及四个工具设置开关补稳定 Automation 名称；没有改变工具启动、进程跟踪、保存或风险判定。
- `R21AutomationValueBehaviorTests` 当前 `6/6`；新增测试实际检查三个 ComboBox peer、四个 ToggleSwitch peer、Toggle `Off → On` 和版本选中值变化；相关组合筛选 `54/54` 通过。
- 隔离 source-copy Release Playnite `net462` / Tests `net472` 为 `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning；`validate-source.py`、XAML `24/24`、diff 通过；WPF 静态检查 `0/27/177`，未见本批新增 warning。
- 证据来自生产 TrainerCenter XAML、合成 peer、fake/隔离 testhost/source-copy；真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现和宿主性能仍待验。Demo 原目录不可用，main 用户改动未碰、未合并；本批临时 source-copy/build 已清理。证据见 [R21-02 Trainer 设置名称与状态](evidence/R21-02-TRAINER-SETTINGS-AUTOMATION-20260921.md)。
- 当前不签收整项 R21-02：下一可执行小批量是继续盘点其他复合选择器和逐控件状态/值负例；完成公共门禁后进入 `R21-03`。

## 2026-09-21 Round3 R21-02 MediaCenter 批量动作空选择保护（续作小批量）

- `f7b664f1` 先复核已有 `UpdateMediaMetadataBatchAsync` 的保护顺序：`null` 与空 `IList` 在 IPC/媒体元数据写入前抛出既有“请先在媒体列表中选择一个或多个项目。”；不改生产命令、Binding、取消/错误语义或写入路径。
- `R21AutomationValueBehaviorTests` 当前 `16/16`；新增测试通过反射等待真实生产私有异步方法，分别覆盖 `null` 和空 `ArrayList` 两条负例；相关组合筛选 `64/64` 通过。
- 提交身份 `GscBuildCommit=f7b664f1` 的 D 盘源码副本 Release 构建为 Playnite `net462` / Tests `net472`，`0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning；链接工作树 `_wpftmp.csproj` 写入遇 `Access denied`，按既有流程改用 D 盘源码副本；`validate-source.py`、XAML `24/24`、diff 通过；WPF 静态检查 `0/27/177`。
- 该证据只覆盖生产私有方法的空选择错误边界，不宣称真实 `ICommand.CanExecute`/UI Enabled、Playnite/package-host、媒体写入、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现和宿主性能。Demo 原目录不可用，main 用户改动未碰、未合并；本批 source-copy/build 已清理。证据见 [R21-02 MediaCenter 批量动作空选择保护](evidence/R21-02-MEDIA-BATCH-EMPTY-GUARD-20260921.md)。
- 当前不签收整项 R21-02：下一可执行小批量是核对批量动作忙碌态/CanExecute，再继续其他逐控件状态/值边界；完成公共门禁后进入 `R21-03`。

## 2026-09-21 Round3 R21-02 MediaCenter 备注与元数据动作（续作小批量）

- `b20dac99` 先查已有 `MediaComment` Binding、`UpdateMediaMetadataCommand` 和 `ReassignMediaCommand`，只补备注 TextBox、保存元数据和移动归类三个控件的稳定 Automation 名称；不改样式、Binding、命令、布局或媒体写入语义。
- `R21AutomationValueBehaviorTests` 当前 `14/14`；新增行为测试实际检查三个 WPF peer 名称，验证 TextBox Value 从“原备注”到“更新备注”，以及两个隔离 Button 的 Invoke 通道；相关组合筛选 `62/62` 通过。
- 提交身份 `GscBuildCommit=b20dac99` 的 D 盘源码副本 Release 构建为 Playnite `net462` / Tests `net472`，`0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning；链接工作树 `_wpftmp.csproj` 写入遇 `Access denied`，按既有流程改用 D 盘源码副本；`validate-source.py`、XAML `24/24`、diff 通过；WPF 静态检查 `0/27/177`。
- Invoke 只证明隔离 WPF 控件调用通道，不代表真实 ICommand、Playnite/package-host 或媒体写入；证据来自生产 MediaCenter XAML、合成 WPF peer、fake/隔离 testhost/源码副本。真实 Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现和宿主性能仍待验。Demo 原目录不可用，main 用户改动未碰、未合并；本批 source-copy/build 已清理。证据见 [R21-02 MediaCenter 备注与元数据动作](evidence/R21-02-MEDIA-METADATA-CONTROLS-20260921.md)。
- 当前不签收整项 R21-02：下一可执行小批量是继续盘点 MediaCenter 批量动作和其他逐控件状态/值负例；完成公共门禁后进入 `R21-03`。

## 2026-09-21 Round3 R21-02 MediaCenter 批量动作名称与 Invoke（续作小批量）

- `9fd7223c` 先查已有三个批量命令和两处操作条，补“收藏所选媒体”“取消收藏所选媒体”“为所选媒体应用当前备注”三类按钮名称；保留 Content、Command、CommandParameter、样式和布局。
- `R21AutomationValueBehaviorTests` 当前 `15/15`；新增行为测试确认三个名称各出现两次，并以实际 WPF `IInvokeProvider` 调用三个隔离动作处理器；相关组合筛选 `63/63` 通过。
- 提交身份 `GscBuildCommit=9fd7223c` 的 D 盘源码副本 Release 构建为 Playnite `net462` / Tests `net472`，`0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning；链接工作树 `_wpftmp.csproj` 写入遇 `Access denied`，按既有流程改用 D 盘源码副本；`validate-source.py`、XAML `24/24`、diff 通过；WPF 静态检查 `0/27/177`。
- Invoke 只证明隔离 WPF 控件调用通道，不代表真实 ICommand、批量选择集合或媒体写入；证据来自生产 MediaCenter XAML、合成 WPF peer、fake/隔离 testhost/源码副本。真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现和宿主性能仍待验。Demo 原目录不可用，main 用户改动未碰、未合并；本批 source-copy/build 已清理。证据见 [R21-02 MediaCenter 批量动作](evidence/R21-02-MEDIA-BATCH-ACTIONS-20260921.md)。
- 当前不签收整项 R21-02：下一可执行小批量是补批量动作禁用/空选择负例，再继续其他逐控件状态/值边界；完成公共门禁后进入 `R21-03`。

## 2026-09-21 Round3 R21-02 MediaCenter 收藏开关名称与状态（续作小批量）

- `42f5744d` 先查已有 `MediaFavorite` Binding、ToggleSwitch 和共享样式，只补当前媒体收藏开关缺失的 `AutomationProperties.Name="收藏当前媒体"`；不改开/关内容、命令、服务、DTO、布局或媒体写入语义。
- `R21AutomationValueBehaviorTests` 当前 `13/13`；新增行为测试实际检查 WPF ToggleSwitch peer 名称，并验证 `Off → On → Off` 回切负例；相关组合筛选 `61/61` 通过。
- 提交身份 `GscBuildCommit=42f5744d` 的 D 盘源码副本 Release 构建为 Playnite `net462` / Tests `net472`，`0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning；链接工作树 `_wpftmp.csproj` 写入遇 `Access denied`，按既有流程改用 D 盘源码副本；`validate-source.py`、XAML `24/24`、diff 通过；WPF 静态检查 `0/27/177`。
- 首次未提交 source-copy 测试因 `working-tree` 与源码根 HEAD 不一致被身份夹具拒绝，提交后统一身份重建并通过；证据来自生产 MediaCenter XAML、合成 WPF peer、fake/隔离 testhost/源码副本。真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现和宿主性能仍待验。Demo 原目录不可用，main 用户改动未碰、未合并；本批 source-copy/build 已清理。证据见 [R21-02 MediaCenter 收藏开关](evidence/R21-02-MEDIA-FAVORITE-TOGGLE-20260921.md)。
- 当前不签收整项 R21-02：下一可执行小批量是继续盘点 MediaCenter 备注/元数据动作和其他逐控件状态/值负例；完成公共门禁后进入 `R21-03`。

## 2026-09-21 Round3 R21-02 MediaCenter 选择器名称与值（续作小批量）

- `da16043d` 复用 MediaCenter 三个已有选择器的 Games/MediaFilterOptions 数据源、SelectedItem Binding 和后续命令，为收件箱确认归类目标、当前游戏媒体类型筛选、所选媒体重新归类目标补稳定 Automation 名称；没有改变媒体归类、筛选、移动或文件语义。
- `R21AutomationValueBehaviorTests` 当前 `7/7`；新增测试实际检查三个 ComboBox peer 的名称并验证合成选中值变化；相关组合筛选 `55/55` 通过。
- 隔离 source-copy Release Playnite `net462` / Tests `net472` 为 `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning；`validate-source.py`、XAML `24/24`、diff 通过；WPF 静态检查 `0/27/177`，未见本批新增 warning。
- 证据来自生产 MediaCenter XAML、合成 peer、fake/隔离 testhost/source-copy；真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现和宿主性能仍待验。Demo 原目录不可用，main 用户改动未碰、未合并；本批临时 source-copy/build 已清理。证据见 [R21-02 MediaCenter 选择器名称与值](evidence/R21-02-MEDIA-SELECTORS-AUTOMATION-20260921.md)。
- 当前不签收整项 R21-02：下一可执行小批量是继续盘点其他复合选择器和逐控件状态/值负例；完成公共门禁后进入 `R21-03`。

## 2026-09-21 Round3 R21-02 Dashboard 选框与策略控件名称/状态（续作小批量）

- `6a8cae16` 复用 Dashboard 游戏选框的既有状态/平台/排序 Binding、同步路径和当前游戏策略编辑器，为三个筛选器、六个策略开关和保存策略动作补稳定 Automation 名称；没有改变筛选、排序、同步、命令或策略语义。
- `R21AutomationValueBehaviorTests` 当前 `8/8`；新增行为测试实际创建三个 WPF ComboBox peer、六个 ToggleSwitch peer 和保存按钮，检查名称、选项切换及 Toggle `Off → On`；相关组合筛选 `56/56` 通过。
- 隔离 source-copy Release Playnite `net462` / Tests `net472` 为 `0 errors / 0 warnings`；`validate-source.py`、XAML `24/24`、diff 通过；WPF 静态检查 `0/27/177`，未见本批新增诊断。
- 证据来自生产 Dashboard XAML、合成 WPF peer、fake/隔离 testhost/source-copy；真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现和宿主性能仍待验。Demo 原目录不可用，main 用户改动未碰、未合并；本批临时 source-copy/build 已清理。证据见 [R21-02 Dashboard 选框与策略控件](evidence/R21-02-DASHBOARD-AUTOMATION-20260921.md)。
- 当前不签收整项 R21-02：下一可执行小批量是继续盘点其他复合选择器和逐控件状态/值负例；完成公共门禁后进入 `R21-03`。

## 2026-09-21 Round3 R21-02 Maintenance 进程映射选择器名称与值（续作小批量）

- `915ac77c` 复用 Maintenance 映射编辑器已有 `Games` 数据源、`ProcessMappingTargetGame` `SelectedItem` Binding、游戏名称模板和“绑定”命令，为目标游戏选择器补稳定 Automation 名称；没有改变进程映射保存、删除或进程识别语义。
- `R21AutomationValueBehaviorTests` 当前 `9/9`；新增行为测试实际创建 WPF ComboBox peer，检查“进程映射目标游戏”名称并验证合成选项从“游戏 A”切换到“游戏 B”；相关组合筛选 `57/57` 通过。
- 隔离 source-copy Release Playnite `net462` / Tests `net472` 为 `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning；`validate-source.py`、XAML `24/24`、diff 通过；WPF 静态检查 `0/27/177`，未见本批新增诊断。
- 证据来自生产 Maintenance XAML、合成 WPF peer、fake/隔离 testhost/source-copy；真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现和宿主性能仍待验。Demo 原目录不可用，main 用户改动未碰、未合并；本批临时 source-copy/build 已清理。证据见 [R21-02 Maintenance 进程映射选择器](evidence/R21-02-MAINTENANCE-PROCESS-MAPPING-20260921.md)。
- 当前不签收整项 R21-02：下一可执行小批量是继续盘点其他复合选择器和逐控件状态/值负例；完成公共门禁后进入 `R21-03`。

## 2026-09-21 Round3 R21-02 MediaCenter 额外选择器名称与值（续作小批量）

- `eac4276f` 没有修改生产 XAML，只复用 MediaCenter 已有的 `媒体收件箱视图`、`媒体筛选预设`、`媒体归类批次状态筛选` 和 `调整归类建议目标` 名称、选项来源与 Binding，补实际 WPF peer 证据。
- `R21AutomationValueBehaviorTests` 当前 `10/10`；新增行为测试实际检查四个 ComboBox peer 名称并验证选值从第一项切换到第二项；相关组合筛选 `58/58` 通过。
- 隔离 source-copy Release Playnite `net462` / Tests `net472` 为 `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning；`validate-source.py`、XAML `24/24`、diff 通过；WPF 静态检查 `0/27/177`，未见本批新增诊断。
- 证据来自已有生产 MediaCenter XAML、合成 WPF peer、fake/隔离 testhost/source-copy；真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现和宿主性能仍待验。Demo 原目录不可用，main 用户改动未碰、未合并；本批临时 source-copy/build 已清理。证据见 [R21-02 MediaCenter 额外选择器](evidence/R21-02-MEDIA-EXTRA-SELECTORS-20260921.md)。
- 当前不签收整项 R21-02：下一可执行小批量是继续盘点其他复合选择器和逐控件状态/值负例；完成公共门禁后进入 `R21-03`。

## 2026-09-21 Round3 R21-02 Maintenance 云端队列筛选器名称与值（续作小批量）

- `f3eecad0` 没有修改生产 XAML，只复用 Maintenance 已有 `云端队列状态筛选`、`云端队列类型筛选`、`云端队列时间筛选` 名称、选项来源与 Binding，补实际 WPF peer 证据。
- `R21AutomationValueBehaviorTests` 当前 `11/11`；新增行为测试实际检查三个 ComboBox peer 名称并验证选值从“全部/全部时间”切换到“待处理/媒体/最近一天”；相关组合筛选 `59/59` 通过。
- 提交身份 `GscBuildCommit=f3eecad0` 的 D 盘源码副本 Release 构建为 Playnite `net462` / Tests `net472`，`0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning；链接工作树 `_wpftmp.csproj` 写入遇 `Access denied`，按既有流程改用 D 盘源码副本；`validate-source.py`、XAML `24/24`、diff 通过；WPF 静态检查 `0/27/177`。
- 证据来自已有生产 Maintenance XAML、合成 WPF peer、fake/隔离 testhost/源码副本；真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现和宿主性能仍待验。Demo 原目录不可用，main 用户改动未碰、未合并；本批源码副本/build 已清理。证据见 [R21-02 Maintenance 云端队列筛选器](evidence/R21-02-MAINTENANCE-CLOUD-SELECTORS-20260921.md)。
- 当前不签收整项 R21-02：下一可执行小批量是继续盘点其他复合选择器和逐控件状态/值负例；完成公共门禁后进入 `R21-03`。

## 2026-09-21 Round3 R21-02 SaveCenter 策略控件外置标签与状态（续作小批量）

- `e1fadaab` 没有修改生产 XAML，只复用 SaveCenter 已有四个策略 ToggleSwitch、异常保护/策略模板 ComboBox 和锁定版本 CheckBox 的标签、Binding 与 Automation 名称，补实际 WPF peer 证据。
- `R21AutomationValueBehaviorTests` 当前 `12/12`；新增行为测试实际检查四个 ToggleSwitch、两个 ComboBox 和一个 CheckBox 的名称，验证 Toggle/CheckBox `Off → On` 及两个选值变化；相关组合筛选 `60/60` 通过。
- 提交身份 `GscBuildCommit=e1fadaab` 的 D 盘源码副本 Release 构建为 Playnite `net462` / Tests `net472`，`0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning；链接工作树 `_wpftmp.csproj` 写入遇 `Access denied`，按既有流程改用 D 盘源码副本；`validate-source.py`、XAML `24/24`、diff 通过；WPF 静态检查 `0/27/177`。
- 证据来自已有生产 SaveCenter XAML、合成 WPF peer、fake/隔离 testhost/源码副本；真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现和宿主性能仍待验。Demo 原目录不可用，main 用户改动未碰、未合并；本批源码副本/build 已清理。证据见 [R21-02 SaveCenter 策略控件 peer](evidence/R21-02-SAVECENTER-POLICY-PEER-20260921.md)。
- 当前不签收整项 R21-02：下一可执行小批量是继续盘点其他复合选择器和逐控件状态/值负例；完成公共门禁后进入 `R21-03`。

## 2026-09-21 Round3 R21-02 MediaCenter 批量动作忙碌态刷新（续作小批量）

- `e0805624` 复用现有 `IsBusy`、三个批量命令和 `RelayCommand`，修复 `RaiseCommandStatesCore` 漏列 `FavoriteSelectedMediaCommand`、`UnfavoriteSelectedMediaCommand`、`CommentSelectedMediaCommand` 的问题；没有改变命令参数、绑定、选择集合、取消/错误或媒体写入语义。
- `R21AutomationValueBehaviorTests` 当前 `17/17`；新增 WPF `Button.Command` 行为探针验证批量动作 `可用 → 忙碌禁用 → 恢复可用`，相关组合筛选 `65/65` 通过；空选择 null/空集合负例继续通过。
- 提交身份 `GscBuildCommit=e0805624` 的 D 盘源码副本 Release 构建为 Playnite `net462` / Tests `net472`，`0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning；`validate-source.py`、XAML `24/24`、diff 通过；WPF 静态检查 `0/27/162`，未见本批新增诊断。
- 证据来自生产 DashboardViewModel 刷新列表、合成 `RelayCommand`/WPF Button、fake/隔离 testhost/source-copy；不代表真实 Playnite/package-host、媒体 IPC/写入、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现或宿主性能。链接工作树 `_wpftmp.csproj` 写入遇 `Access denied` 未绕过；Demo 原目录不可用，main 用户改动未碰、未合并；本批 source-copy/build 已清理。证据见 [R21-02 MediaCenter 批量动作忙碌态](evidence/R21-02-MEDIA-BATCH-BUSY-CANEXECUTE-20260921.md)。
- 当前不签收整项 R21-02：下一可执行小批量是继续其他逐控件状态/值边界；完成公共门禁后进入 `R21-03`。

## 2026-09-21 Round3 R21-02 MediaCenter 列表语义与选择状态（续作小批量）

- `c419fc98` 复用 `MediaGrid` 已有扩展选框、虚拟化、滚动条、`SelectedMedia` Binding 和 `SelectionChanged`，只补列表级 `AutomationProperties.Name="当前游戏媒体列表"`，没有更换控件或选择/滚动语义。
- `R21AutomationValueBehaviorTests` 当前 `18/18`；新增实际 WPF `ListBox` peer 验证列表名称、`ISelectionProvider.CanSelectMultiple`、选中项存在和“媒体 A”→“媒体 B”变化；相关组合筛选 `66/66` 通过。
- 提交身份 `GscBuildCommit=c419fc98` 的 D 盘源码副本 Release 构建为 Playnite `net462` / Tests `net472`，`0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning；`validate-source.py`、XAML `24/24`、diff 通过；WPF 静态检查 `0/27/162`，未见本批新增诊断。
- 证据来自生产 MediaCenter XAML、合成 WPF `ListBox` peer、fake/隔离 testhost/source-copy；不代表真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现或宿主性能。链接工作树 `_wpftmp.csproj` 写入遇 `Access denied` 未绕过；Demo 原目录不可用，main 用户改动未碰、未合并；本批 source-copy/build 已清理。证据见 [R21-02 MediaCenter 媒体列表](evidence/R21-02-MEDIA-LIST-SELECTION-20260921.md)。
- 当前不签收整项 R21-02：下一可执行小批量是继续其他逐控件状态/值边界；完成公共门禁后进入 `R21-03`。

## 2026-09-21 Round3 R21-02 MediaCenter 媒体详情位置值与导航边界（续作小批量）

- `806d5a27` 先复用生产 `MediaDetailNavigationDisplay`、`CanNavigatePreviousMedia` 和 `CanNavigateNextMedia`，没有修改服务、DTO、命令、Binding 或媒体写入语义；新增证据覆盖未选择、首项 `1 / 2`、末项 `2 / 2` 及前后导航不可用负例。
- `R21AutomationValueBehaviorTests` 当前 `19/19`；新增行为测试用合成 `MediaItemDto` 和隔离 ViewModel 验证三态，再用实际 WPF `TextBlock` peer 验证“媒体详情位置”名称与值；本批相关组合筛选 `22/22` 通过。
- 提交身份 `GscBuildCommit=806d5a27` 的 D 盘源码副本 Release 构建为 Playnite `net462` / Tests `net472`，`0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning；`validate-source.py`、XAML `24/24`、diff 通过；WPF 静态检查 `0/27/162`，未见本批新增诊断。
- 证据来自生产 MediaCenter XAML、合成 DTO/隔离 ViewModel 和 WPF peer；不代表真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现或宿主性能。链接工作树 `_wpftmp.csproj` 写入遇 `Access denied` 未绕过；Demo 原目录不可用，main 用户改动未碰、未合并；本批 source-copy/build 已清理。证据见 [R21-02 MediaCenter 媒体详情位置](evidence/R21-02-MEDIA-NAVIGATION-VALUE-20260921.md)。
- 当前不签收整项 R21-02：下一可执行小批量是继续其他逐控件状态/值负例；完成公共门禁后进入 `R21-03`。

## 2026-09-21 Round3 R21-02 TaskCenter 任务进度 UIA 值与状态边界（续作小批量）

- `6b56a467` 复用既有 `TaskStatusDto.ProgressValue`、`ProgressDisplay` 和 TaskCenter 生产 Binding，只新增实际 WPF `ProgressBar`/`AutomationPeer` 行为证据；正常 `42`、未知 `-1`、越界 `120` 分别验证 UIA `RangeValue`、`0–100` 范围和 HelpText，未知保持 `—`，没有伪装成有效百分比，也没有修改生产代码。
- `R21AutomationValueBehaviorTests` 当前 `20/20`；`R21AutomationValueBehaviorTests | R06TaskProgressBehaviorTests | R03NumericAlignmentTests` `34/34`。提交后 D 盘 source-copy Release 构建 Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning；`validate-source.py`、XAML `24/24`、`git diff --check`、WPF `0/27/162` 通过。
- 证据来自生产 TaskCenter XAML、既有 DTO/投影、合成 WPF `ProgressBar` peer、fake/隔离 testhost/source-copy；真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现和宿主性能仍待验。链接工作树 `_wpftmp.csproj` 写入遇 `Access denied` 未绕过；Demo 原目录不可用，main 用户改动未碰、未合并；本批临时 source-copy/build 已清理。证据见 [R21-02 TaskCenter 任务进度 UIA](evidence/R21-02-TASK-PROGRESS-PEER-20260921.md)。
- 当前不签收整项 R21-02：剩余复合选择器及逐控件状态/值负例仍需逐项核对；下一可执行小批量继续这些边界，公共门禁完成后再进入 `R21-03` 验证错误播报。

## 2026-09-21 Round3 R21-02 控件名称与值收口

- 按 R21-02 完成条件完成控件族核对：图标/动作按钮、复合选择器、开关和进度条均有生产契约与实际 WPF peer 证据；无选中、未知、越界、忙碌、空选择和三态边界均有明确结果。
- 最新定向回归为 `R21AutomationValueBehaviorTests 21/21`、相关 `35/35`；提交 `efb42b7b` 的隔离 Release Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 warning，source/XAML/diff 与 WPF `0/27/162` 通过。
- 因真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、DPI/跨屏、最终呈现、ETW 和宿主性能未运行，本项按“已满足，待环境验证”收口，不把离屏 peer 当成宿主签收。Demo 原目录不可用，main 用户改动未碰、未合并。本阶段临时 source-copy/build 已清理。证据见 [R21-02 收口](evidence/R21-02-CLOSEOUT-20260921.md)。
- 下一可执行小批量：进入 `R21-03` 验证错误播报，先复用现有验证错误 Binding、状态 presenter 和错误摘要，不新造通知体系。

## 2026-09-21 Round3 R21-02 选择器无选中与开关三态边界（续作小批量）

- `efb42b7b` 复用生产 ComboBox、`ToggleSwitch` 和共享状态模板，只新增实际 WPF peer 负例：无选中 ComboBox 的 `ISelectionProvider.GetSelection()` 返回 `null`，选中“失败”后为单项；三态开关按 `Indeterminate → Off → On` 可读，没有修改生产 XAML、Binding 或业务设置语义。
- `R21AutomationValueBehaviorTests` 当前 `21/21`；相关组合筛选 `35/35`。提交后 D 盘 source-copy Release 构建 Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning；`validate-source.py`、XAML `24/24`、`git diff --check`、WPF `0/27/162` 通过。
- 证据来自生产控件类型/共享模板、合成选项、实际 WPF `ComboBox`/`ToggleSwitch` peer、fake/隔离 testhost/source-copy；三态不代表当前业务 Binding 会产生 Indeterminate。真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现和宿主性能仍待验。链接工作树 `_wpftmp.csproj` 写入遇 `Access denied` 未绕过；Demo 原目录不可用，main 用户改动未碰、未合并；本批临时 source-copy/build 已清理。证据见 [R21-02 选择器与开关负例](evidence/R21-02-SELECTOR-TOGGLE-NEGATIVE-20260921.md)。
- 当前不签收整项 R21-02：剩余复合选择器及逐控件状态/值负例仍需逐项核对；下一可执行小批量继续这些边界，公共门禁完成后再进入 `R21-03` 验证错误播报。

## 2026-09-21 Round3 R21-03 验证错误播报（已有实现审计收口）

- 先查现有实现而非重建通知体系：设置页已有 `QueueValidationSummaryUpdate` 合并输入变化，`RefreshValidationSummaryCore` 合并模型/路径/字段错误并按目标去重；字段 `AutomationProperties.HelpText`、详情错误链接和聚焦目标由同一条验证目标维护，空集合会折叠旧摘要/详情。
- 真实 WPF `SettingsValidationNavigationBehaviorTests` 验证错误链接切到自动化分类、字段可见并取得键盘焦点、字段 HelpText 关联错误和滚动容器移动（`1/1`）；既有 R09 真实控件测试验证越界 `9` 出现错误视觉、合法 `2` 后 `Validation.GetHasError=false` 且焦点样式恢复（`2/2`）。异步旧请求/离页取消、源审计和数值边界 `16/16` 通过；没有把 `Assert.Contains` 当作交互签收。
- 干净 D 盘 source-copy Release 构建 Playnite `net462` / Tests `net472` 为 `0 errors`，仅 2 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning；`validate-source.py`、XAML `24/24`、`git diff --check`、WPF `0/27/162` 通过。本批无生产代码变更，临时 source-copy/build 已清理。
- 真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、DPI/物理跨屏、最终呈现、ETW 和宿主性能仍待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据见 [R21-03 验证错误播报](evidence/R21-03-VALIDATION-ANNOUNCEMENT-20260921.md)。下一可执行小批量：`R21-04` 异步完成播报，先盘点完成/失败/列表加载通知和重复进度去重。

## 2026-09-21 Round3 R21-04 异步完成播报

- 先查现有能力：`TaskEventUiBatcher` 合并运行中进度并限制待处理队列，终态即时旁路；`TaskNotificationDeduper` 只领取终态且对不同失败证据保留可见性；Dashboard Toast 和 Task Center 历史提供最终状态/详情回读，不新造通知服务或 DTO。
- 新增 `R21AsyncCompletionAnnouncementBehaviorTests` `2/2`：真实 STA WPF 窗口验证终态 Toast 的 Name/HelpText、非焦点属性与焦点不变；实际 `TaskCenterView` 验证加载态文本更新为最近更新时间。相邻通知/批处理/会话/旧任务页行为合计 `22/22`，另 Core `5/5`、Worker `5/5`。
- 隔离 Release solution `0/0`、XAML `24/24`、source/diff 和 WPF `0/27/162` 通过。合并相邻旧测试时仍有 `TaskCenterViewResponsiveTests.FailedTaskDetailsPutUserReasonBeforeCollapsedTechnicalDetails` 单条旧源码断言失败（`28/1`），本批未绕过、未改写。
- 证据只来自合成 DTO、fake/隔离 testhost、隔离目录和 STA WPF；真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、DPI/跨屏、presented frame、ETW、宿主性能仍待验。Demo 原目录不可用，main 用户改动未碰、未合并。证据见 [R21-04 异步完成播报](evidence/R21-04-ASYNC-COMPLETION-ANNOUNCEMENT-20260921.md)。下一可执行小批量：`R21-05` 禁用与隐藏区别。

## 2026-09-21 Round3 R21-05 禁用与隐藏区别

- 先查现有 `ActionAvailabilityHints`、SaveCenter 维护跳转条件、`GscWpfUiContextButton` 和共享 Automation 绑定；本阶段没有新增服务、DTO、命令或视觉体系，也没有改变选框、滚动条、取消/错误或恢复保护。
- `R21DisabledHiddenBehaviorTests` 使用实际 `SaveCenterView` 和生产资源字典验证：前置条件不满足时维护跳转按钮为 `Visibility.Collapsed`、不可见且不能取焦点，说明文本仍可聚焦并读回 Name/HelpText；条件满足时同一按钮恢复可见并保留原命令绑定；共享上下文动作在 `CanExecute=false` 时保持可见，UIA `Invoke` 被拒绝且无副作用。新增行为 `2/2`。
- 提交身份 `GscBuildCommit=380234e20c127277d3655a07a11e1e29ee23f2cd` 的 Release 测试项目编译 `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning；显式身份下邻接组合 `31/31`；`validate-source.py`、XAML `24/24`、`git diff --check` 通过。完整 Release 脚本已编译解决方案，但 Worker 仍有未修改的 `MediaSyncServiceTests.ClassificationApplyUsesSelectedStableIdsAndValidatedTargetOverride` 在 `MediaSyncServiceTests.cs:570` 的 `NullReferenceException`（350 通过、1 失败、351 总计）。
- 证据来自合成 hint/命令、fake/隔离 WindowHost 和实际 WPF AutomationPeer；不把源码包含断言当作交互签收。真实 Playnite/package-host、Windows UIA/读屏、OS 输入/IME、DPI/跨屏、最终呈现、ETW、宿主性能仍待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据见 [R21-05 禁用与隐藏行为](evidence/R21-05-DISABLED-HIDDEN-BEHAVIOR-20260921.md)。下一可执行小批量：`R21-06` 可选择技术文本，先盘点现有 TextBlock/选择器与复制语义。

## 2026-09-21 Round3 R21-06 可选择技术文本

- 先查现有路径/诊断/错误码/版本入口：`GscWpfUiPathDetailTextBox`、`CopyPathCommand`、TaskCenter `SafeDetailMessage` 和 `TaskFailureClipboardFormatter` 已提供可选择/复制能力；本批只新增共享 `GscWpfUiTechnicalTextBox`，并把两个侧栏插件版本入口从 `TextBlock` 改为只读可选 `TextBox`，保留原版本赋值代码和导航。
- `R03LongPathTests`、`R11DiffListSearchBehaviorTests`、`R15TaskFailureCopyTests` 加上 `R21SelectableTechnicalTextBehaviorTests` 合计 `13/13`：长路径、SaveCenter 完整路径、脱敏技术详情/错误码/任务 ID 和两条版本入口均由实际 WPF 控件承载，版本框可聚焦全选且提示 Ctrl+C。
- 提交身份 `GscBuildCommit=866ceecde2425c1b5ec159f5012c42023bfae841` 的 Release 测试项目为 `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning；`validate-source.py`、XAML `24/24`、`git diff --check` 通过。
- 该批不宣称真实 Playnite/package-host、系统 UIA/读屏、真实剪贴板、DPI/跨屏、最终选择像素、ETW 或宿主性能；Demo 原目录不可用，main 用户改动未碰、未合并。R21-06 按“已满足，待环境验证”收口，下一项 `R21-07` 焦点可视回归。证据见 [R21-06 可选择技术文本](evidence/R21-06-SELECTABLE-TECHNICAL-TEXT-20260921.md)。

## 2026-09-21 Round3 R21-07 焦点可视回归

- 先复用生产 `GscSharedFocusVisual`、AcrylicProductionShell 的 `PickerOverlay`/`PickerList`、既有滚动条与 `ClosePickerAndRestoreFocus`；没有重建焦点、滚动或主题体系，也没有改变游戏选框、命令、Binding、取消/错误和恢复语义。
- 新增 `R21FocusVisualRegressionBehaviorTests.ProductionPickerKeepsVisibleFocusAcrossScrollThemeSwitchAndClose`：真实生产 Shell 加合成 2,000 项，打开并完成布局后让搜索框实际取得焦点；请求 `ListBox.ScrollIntoView` 后末项容器实际实现且保留焦点样式；浅/深主题运行时资源切换后强调色变化、焦点仍在搜索框；关闭选择器后 Overlay 折叠并回焦可见游戏选框。没有通过 `SelectedIndex` 提交选择来制造假滚动。
- 已推送提交 `b20f1a4b` 的精确身份 Release 构建 Playnite `net462` / Tests `net472` 为 `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning。串行窄回归：R21 新增 `1/1`、R09 焦点环 `2/2`、R05 主题弹层 `1/1`，合计 `4/4`；`validate-source.py`、XAML `24/24`、`git diff --check` 通过。
- 一次把三组 WPF 测试并行放入同一 testhost 时，多个隐藏窗口互相争抢桌面焦点，产生离屏时序失败；拆成独立 testhost 后均通过，不把它写成产品失败。旧 R05 选择器/焦点边界夹具仍有手动注入 VM 后未走最新 Shell 初始化路径的既有失败，本批没有改写旧断言。
- 隔离隐藏宿主不能把 `ScrollViewer.VerticalOffset`、末项实际像素裁剪或 presented frame 作为物理滚动/最终呈现证据；真实 Playnite/package-host、Windows UIA/读屏、OS 输入/IME、DPI/跨屏、像素焦点环、ETW 和宿主性能仍待验。Demo 原目录不可用，main 用户改动未碰、未合并。本批只使用合成游戏 DTO、隔离 STA 窗口和生产资源，没有访问真实存档、媒体目录、云端或外发诊断。证据见 [R21-07 焦点可视回归](evidence/R21-07-FOCUS-VISUAL-20260921.md)。
- 下一可执行小批量：`R21-08` 单屏与跨屏分账，先记录当前单屏/DPI 可测边界与 Popup/字体/焦点的真实跨屏待验场景，不把离屏模拟写成物理跨屏结论。

## 2026-09-21 Round3 R21-08 单屏与跨屏分账

- 先核对已有 Q24-03 真实宿主前置：`scripts/real-host-audit.ps1` 使用 `System.Windows.Forms.Screen.AllScreens` 记录显示器边界/工作区，并在单屏时把 `Q24_03PhysicalCrossScreen.Status` 置为 `blocked-single-display`；没有新造 Popup 定位、独立窗口或 DPI 体系。
- 当前 Windows 主机真实枚举只有 `\\.\DISPLAY1`，Bounds 为 `0,0 1707×960`，WorkArea 为 `0,0 1707×912`；因此没有执行跨屏迁移，也没有把离屏逻辑 DPI 模拟写成物理跨屏通过。双屏场景已写成可执行清单：Popup 位置、字体/文本度量、焦点、主题资源/资源释放、回迁和前后呈现证据。
- 现有实际证据：`R05PopupBoundaryBehaviorTests 2/2`、`R09PixelStrokeBehaviorTests 2/2`（1.00–2.00 逻辑尺度模拟）、`R09ThemeSwitchBehaviorTests 1/1`、拓扑/源码契约 `26/26`；当前提交身份 Release 测试项目 `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning，real-host 脚本语法通过。
- R21-08 按“已满足，待环境验证”收口。真实双屏 Playnite/package-host、Popup 跨屏像素位置、字体清晰度、焦点和资源释放、呈现帧、Windows UIA/读屏、OS 输入/IME、ETW、宿主性能仍待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据见 [R21-08 单屏与跨屏分账](evidence/R21-08-SINGLE-CROSS-SCREEN-20260921.md)。
- 下一可执行任务：进入后续依赖已满足的 Q/R 小批量；若获得第二个物理显示器，优先按本证据的双屏清单运行隔离 Playnite 宿主回放。

## 2026-09-21 Round3 R22-04 打开路径失败

- 先复用 R16 的 `SettingsPathEditorService`、设置页当前字段编辑器、完整路径复制和 Dashboard `RunLocal` 错误边界；没有新增文件服务、权限修改、Explorer 绕过或用户数据写入。
- `R16SettingsPathEditorBehaviorTests` 在隔离临时目录中实际验证有效可执行文件、有效目录、缺失目录、文件冒充目录四种结果；设置目录选项保持 6 个本地字段，远端目标不进入本地打开动作。源码边界测试确认无效 Probe 在打开前返回、打开方法不做父目录兜底；合计 `3/3`。
- 当前提交身份 Release Playnite `net462` / Tests `net472` 构建 `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning；`validate-source.py`、XAML `24/24`、`git diff --check` 通过。
- R22-04 按“已满足，待环境验证”收口。真实 Explorer/Playnite host 的启动失败、网络共享/ACL/占用、最终呈现、UIA/读屏、OS 输入/IME、DPI/跨屏、ETW 和宿主性能仍待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据见 [R22-04 打开路径失败](evidence/R22-04-OPEN-PATH-FAILURE-20260921.md)。
- 下一可执行任务：按依赖选择 `R22-01` 时间显示统一或其他独立 Q/R 小批量；优先继续查已有时间格式化/任务时长能力，避免只以源码包含断言签收。
## 2026-09-24 R23-04 隔离 runner 打包参数修正

- 首次 seed runner 在 Release build 后、package 前失败：`dev-install-run.ps1` 对 `package.ps1` 使用参数字符串数组 splat，导致 `-Configuration` 被位置绑定。失败前没有部署正式扩展或 seeder，没有启动 Playnite，也没有 manifest；不记为 CEF/宿主结果。
- 修为 hashtable 具名 splat，新增 source guard。Release solution `0 errors`、两条既有 CS8602；定向 guard test `1/1`，XAML `24/24`、source validator、PowerShell AST、diff check 通过。深 `.tmp` 的首个 test invocation 被 net472 260 字符路径限制挡住；短 `.tmp/r3b24` 重建后测试通过。
- 证据：[R23-04 runner 参数修正](evidence/R23-04-RUNNER-PACKAGE-ARGUMENT-FIX-20260924.md)。下一步提交后对未曾启动的同一隔离 seed 流程重试一次，检查 run ID manifest 与实际 Playnite/CEF；不触碰真实 profile，不绕过 CEF。R23-04 保持开放。

## 2026-09-30 用户报告：设置页分组色彩语义

- `c770d37c` 将静态 Settings 分组“存档格式与历史版本”和“自动化与安全”标题的 Info/Success 装饰色统一为主题 Accent；真实状态色未做全局替换。浅/深主题实际生产 WPF 资源行为 `2/2`，复位按钮名称和启用状态保持；规范 Release solution `0 warning / 0 error`、XAML `24/24`。
- OffscreenRenderHarness 全矩阵 Light/Dark `render-qa OK`，Settings `1040×700` 两主题通过；只是逻辑离屏 `1.00 DPI`，并非 Playnite 最终帧。R00/R01 freshness `14 FRESH / 0 STALE`。程序集 SHA/MVID、TRX、截图和官方色彩依据：[证据包](evidence/USER-REPORTED-SETTINGS-SEMANTIC-COLOR-20260930/README.md)。没有真实 host/package/物理 DPI，192 项任务状态与计数不变。
- 下一小批核对 Dashboard 游戏备份统计和主操作图标的 InfoBrush 是否为状态语义，再继续跨页文案/色彩审计。Media Inbox 滚动问题仍要求安全隔离宿主同进程 `[GSC-GRID-DIAGNOSTIC]`。
# 2026-09-24 用户反馈：R06 表格排序崩溃修复

- 用户日志实际命中 `DataGridStableSortController.ApplyCurrentSort()` 的 `ListCollectionView.DeferRefresh` 清理路径。已在当前 main 修复 stale/detached `SourceCollection` 检查，防止失效 view 刷新，并恢复 DataGrid 先行清除后的 controller-owned 箭头。
- 同一隔离 Release build XAML `24/24`、0 errors、两条既有 Media `CS8602` warning；真实 WPF 列头排序升/降序 + detached view 负例 `7/7`。证据：[R06 排序 detached-view 复核](evidence/R06-SORTING-DETACHED-VIEW-CURRENT-MAIN-20260924.md)。
- 这只是针对用户崩溃路径的行为修复，不签收完整 `R06-02`；真实 Playnite/package-host 鼠标输入、用户加载包身份与宿主呈现仍待验。首次默认 `dotnet test` 被旧默认 `bin` assembly 的 identity gate 拦截，最终改运行 `.tmp/sortfix` 同 checkout 隔离 Release assembly 后测试通过；不把入口身份不一致记录成产品回归。
- 下一可执行小批量：共享 DataGrid row selected/focus chrome 的几何修复，先添加能比较真实 cell/content 选择前后的行为探针；再移除 Media Inbox 重复游戏目标选择并改用顶部全局 `SelectedGame`。
# 2026-09-24 用户反馈：DataGrid 选中态行内容几何

- 共享 `GscRoundedDataGridRowTemplate` 原先将 `SelectiveScrollingGrid` 包进会改 Margin 的 `RowChrome`，选中/焦点状态因此会重排行内容。现在 RowBackground/行内容/RowChrome 是模板 Grid 的 sibling；状态 Margin 保留在背景/描边层，细节滚动、虚拟化、圆角和 scrollbar 安全 inset 保持。禁用透明度改由 DataGridRow 统一应用，保留文字/内容淡化。
- 实际 WPF DataGrid 比较未选、失焦选中、键盘焦点选中时每个 cell/TextBlock 的 x/y/宽/高；四个生产表格在 Light/Dark 均由 `<=0.25 DIP` 门限覆盖。再检查 disabled opacity 与失败 badge；不是源码包含断言代替几何。
- Release/XAML `24/24`、0 errors/两条既有 Media CS8602；R06Selection `2/2`、R23 四表主题状态+几何 `1/1`、ReportedWorkspace `8/8`、模板结构 `1/1`、R06排序回归 `7/7`，总 `19/19`。source validator/diff check 通过。详情与五份 TRX：[DataGrid 选中/焦点几何](evidence/R06-DATAGRID-SELECTION-GEOMETRY-CURRENT-MAIN-20260924.md)。
- 仅证明隔离生产 WPF 窗口逻辑 DIP；Playnite/package-host 最终呈现和用户安装身份未验，CEF `platform_channel 0x5` 不绕过。此为 `R06-03` 几何定点修复，不签收完整任务。下一项 Media Inbox 移除冗余目标下拉并使用全局 `SelectedGame`，保持确认/取消/错误与空目标行为。

## 2026-09-26 ENV-001 启动进程身份稳定性复核

- runner 的启动快照现只接受一个带引号的 `--userdatadir`，规范化后与期望绝对路径完全相同；拒绝 sibling 前缀、重复参数，以及首次 CIM 快照前后退出。首次快照后等待 500 ms 并确认 PID、exe 与完整命令行未变，以降低把 Playnite 单实例转发短命进程误认成隔离宿主的风险。
- Release solution `--no-restore` build `0 warning/0 error`；XAML `24/24`、Core `125/125`、Worker `357/357`、Playnite `DiagnosticsEvidenceSourceTests 8/8`、Playnite source-only group 111 类通过；helper tests、4 个 PowerShell AST、diff check 通过。105 类 WPF 逐进程全量套件未完成，不作通过声明。
- 当前 WMI `Win32_Process.CommandLine` Access Denied 与旧 CEF `platform_channel 0x5` 条件未变化，本轮未启动 Playnite；稳定窗口不消除 preflight→launch TOCTOU，也不能证明 OS 级隔离或无副作用。ENV-001 保持 `BLOCKED_ENVIRONMENT`。
- R 主账本复核仍是 `192/192` 唯一 ID、`106/83/1/1/1`，本轮不新增、重分类或签收任务。证据：[ENV-001 启动进程身份复核](evidence/ENV-001-PROCESS-START-IDENTITY-20260926.md)。
# 2026-09-26 Task queue four-row viewport / compact inspector

- After `741f81b8`, Render QA showed the 52-DIP desktop task rows could fit only three records in a 236-DIP viewport. Desktop minimum is now 264 DIP; compact 36-DIP rows reserve 200 DIP. On a short compact page with the inspector open, the secondary metric strip yields its space and returns when the inspector closes. Short-height decisions use the measured Task page viewport; the stacked inspector is capped at 136 DIP with a 2-DIP bottom inset.
- RenderHarness `Task/step1:1100x720` now reads `4/4` rows at 200 DIP. Compact shell samples at 1040×700 and 1100×720 each retain four opened rows and keep the inspector within the Task surface. No Task PROBLEM remains; the complete report is down from 13 to 8 PROBLEMs, all eight in Settings at 560 DIP height.
- Release RenderHarness build `0/0`; Task responsive/inspector tests `9/9`; source validator, XAML `24/24`, WPF static `0 errors/28 warnings/177 info`, and diff check passed. Offscreen logical DIP only; Playnite was not started. The 192-ID R ledger and `106/83/1/1/1` counts are unchanged. Details: [Task viewport evidence](evidence/TASK-FOUR-ROW-INSPECTOR-VIEWPORT-20260926.md).

# 2026-09-26 Overview empty recent-access viewport

## 2026-09-29 R08-01 one-click 间歇失败报告复核（未复现）

- 用户本次摘要仍只说明 `R08MotionReverseBehaviorTests` `1/2`，没有失败方法、断言或堆栈；checkout 内同名安装日志时间为 9/24，无法代表此轮。当前 `97a9b712` Release solution/XAML build `0 warning / 0 error`、`24/24`。
- 从同一隔离输出以独立 VSTest 进程串行跑 R08 类 12 轮：每轮两项均通过，共 `24/24`、所有进程 exit `0`。各轮有 `InvalidComObjectException` 清理输出，根因未知；没有修改动画实现或测试断言。逐轮 TRX：[R08-RECHECK-97A9B712](evidence/R08-RECHECK-97A9B712-20260929/README.md)。
- 用户侧失败未定位，R08 报告保持打开，不更改账本状态。稍后在 `8e4c32a7` 下再次做 12 个独立进程复跑，仍无失败机器断言可用于定位；最新证据见 [R08 复核](evidence/R08-RECHECK-8E4C32A7-20260929/README.md)。Media Inbox 真实宿主问题仍按安全边界待验。

## 2026-09-29 Q13-07 媒体 Inspector 与页面滚动边界

- 盘点后复用现有生产 `ScrollBoundaryRoutingBehavior`、`MediaInboxPageScrollViewer`、`MediaInboxInspectorScrollViewer` 和 R07 通用滚动测试；缺口是它们没有在生产 MediaCenter 页面和真实 compact-details 开启流程中组合断言。本阶段只在 `8e4c32a7` 增加 `ProductionInboxInspectorTransfersWheelOnlyAtItsScrollBoundary`，不改生产布局或路由代码。
- Release solution/XAML `0 warning / 0 error`、`24/24`，source validation 通过。生产 `MediaInboxScrollBehaviorTests 5/5`，包含 Inspector 中段不转移、底部向下转移、顶部向上转移及双方顶端消费负例；`R07ScrollOwnershipBehaviorTests 2/2`、`R07FineScrollBehaviorTests 2/2` 分别独立运行通过。窗口 `860×620 DIP`；合成 `PreviewMouseWheel` 从 Inspector 边界转给页面恰好 `16 DIP`，详情保持其 `0` 或 `1502/1502 DIP` 端点。完整程序集身份、行为表和 TRX：[Q13-07](evidence/Q13-07-MEDIA-INBOX-SCROLL-CHAIN-20260929/README.md)。
- 合成 routed event 不代表触控板惯性、OS 物理滚轮或 Playnite 宿主。Q13-07 自动行为栏已有证据；宿主/触控体验仍外部待验，账本状态保留“外部阻塞 / 未完成”。Media Inbox 用户报告的真实表格滚动空白仍需安全隔离宿主及同进程诊断，不能由本次 Inspector 测试推断根因。下一项按账本核对 Q13-08 稳定分页锚点证据。

## 2026-09-29 R08 one-click 复发报告复核（未复现）

- 用户随后再次报告 R08 one-click `1/2` 失败，但只有计数摘要，没有失败方法、断言或堆栈。本机当前 `8e4c32a7` Release solution/XAML build `0 warning / 0 error`、`24/24`；12 个新 VSTest 进程都 `2/2`，总 `24/24`、exit `0`。每轮有 `TextServicesHost.OnUnregisterTextStore InvalidComObjectException` 清理输出，根因未知；没有修改动画或测试代码。原始 TRX/console：[R08 `8e4c32a7`](evidence/R08-RECHECK-8E4C32A7-20260929/README.md)。这不能解释其他机器上的失败；需要该轮完整日志或 TRX。

## 2026-09-29 Q13-06 DataGrid 滚动交角与末行末列行为

- 先检查已有共享 `GscRedesignDataGridTemplate` 和 `MediaInboxScrollBehaviorTests`：生产模板已有独立 3×3 横纵条布局，旧 2,000 项行为测试覆盖虚拟化/末行完整，但未直接量测两滚动条交角，也没确认最后一列完整位于 viewport。故只补实际生产 DataGrid 的 STA WPF 行为回归，没有改模板。
- `b98ea5a2` Release solution/XAML `0 warning / 0 error`、`24/24`；Media Inbox 滚动类 `4/4`、Q13 Thumb 双主题类 `1/1`。顶/中/右下实测 Header bottom 与 Presenter top `82 DIP`、Presenter right 与竖条 left `608 DIP`、Presenter bottom 与横条 top `388 DIP`；两条滚动条同样只共边。滚到底/最右后末行 1999 完整，五个 cell 均有内容、最右单元格在视口内；列表保持 2,000 项、Item scroll 与行虚拟化。完整身份/边界/TRX：[Q13-06](evidence/Q13-06-DATAGRID-SCROLLBAR-CORNER-20260929/README.md)。
- Round2 Q13-06 自动行为栏有本身份实际证据；因真实 Playnite 宿主/屏幕像素未验，仍保持 external blocker / incomplete。Media Inbox 用户报告的滚动错位未由此证明已经修复；下一项先核对 Q13-07 既有滚动链行为覆盖。

## 2026-09-29 用户紧凑窗口：筛选预设换行行距

- `4121a47e` 修复两组实测为零行距的生产控件：Task Center preset `<657 DIP`、Media Center preset `<720 DIP`。两者窄态补 `8 DIP` 行距，宽态恢复初始 margin；其他 WrapPanel 暂不机械修改，逐组实测后再决定。
- Release/XAML `0/0`、XAML `24/24`；Q14 toolbar `1/1`、两主题行距/resize `2/2`、R00-06 MediaInboxGeometry `3/3`（场景从 760 改为 `700×600 DIP`）和 MediaWindowAnchorContract `10/10`，隔离总计 `16/16`。R00/R01 freshness `14 FRESH / 0 STALE`，自动 freshness gate 通过。
- R00-06 freshness source identity 已更新到完整 SHA `4121a47e28aad0701b314c4a81accfc8ef5283e3`。700×600 下表格可读高度 floor、四行完整可见或页级回退、footer 可达门禁通过；本次没有重跑完整 RenderHarness 矩阵。证据及 TRX：[窄窗行距与 R00-06 当前复核](evidence/USER-REPORTED-WRAP-ROW-SPACING-20260929/README.md)。
- 本地 STA WPF/合成尺寸，不是 Playnite 最终呈现或物理 DPI。下一项：测 Media Inbox secondary actions、Save current-rule、Overview header 和 Dashboard game-header 实际换行净距；再按用户请求精简静态说明、核对次级文本色与紫色主题。Media Inbox 用户滚动空白仍需安全真实宿主同次 `[GSC-GRID-DIAGNOSTIC]`。

## 2026-10-13 R00/R01 freshness 与 #968 CI 收口

- #967 SDK8 源码结构门禁的失败已在本机以同一 `validate-source.py` guard 复现：PowerShell helper 缺 UTF-8 BOM。补 BOM 后校验通过并提交在 `4aa1ec7`；用户给的 SDK10 ZIP 对应 #967 绿色 SDK10 job，不是 SDK8 原始日志。
- `4aa1ec7` 的 #968 / run `38366158855` 终态成功：SDK8 `8.0.423` job `115191682311` 与 SDK10 `10.0.401` job `115191682644` 均成功。SDK8 job 完成 build/test、Render QA、WPF 静态审查、扩展包及诊断上传；SDK10 build/test 与诊断上传成功。没有从 #968 artifact 提取按类计数。
- 当前 freshness `14 fresh / 0 stale`，source commit `4aa1ec7`，package identity `not-provided`；summary JSON 与 artifact 元数据在 [R00/R01 当前 main 复核包](evidence/R00-R01-CURRENT-MAIN-RECHECK-20261013/README.md)。未下载/安装 86.1 MB Windows preview artifact。
- R00/R01 已有行为实现和证据没有因本次 helper 变更而失效；R00-08 OS IME/Playnite 输入链、真实 Playnite 最终呈现、物理 DPI/跨屏、UIA/Narrator、ETW/宿主性能仍待验。Demo 原目录不可用，沿用恢复生产基线。
- 下一项 R05-03：通过已授权的隔离宿主流程检查弹层快速翻转的真实呈现帧；先执行 fail-closed 宿主预检，若仍受环境限制则记录具体阻塞并推进下一个可隔离验证的 Q/R 项。

## 2026-10-13 #969 后续证据校正与 Q23-06 队列

- #967 用户 ZIP 的 SHA-256 `A4D13BB58AC343DFDCFFA6FB4DA31257080F1E5128AF151C946B0AEB0E3BC409` 绑定 SDK10 job `115178885908`、artifact `11795654027`、SDK `10.0.401`；其中 R08-01 单类 TRX `2/2` 已提取并按哈希归档。它不包含 workflow 中 SDK8 源码结构失败的日志，用户机 `1/2` 故障仍无原始材料。
- #968/run `38366158855` 对代码 `4aa1ec7` 的 SDK8 `8.0.423` 和 SDK10 `10.0.401` jobs 均成功；#969/run `38377673615` 对当前文档 HEAD `d56d23e` 也成功。R00/R01 freshness `14 fresh / 0 stale`，package identity 未提供。
- R05-03 隔离 Playnite bootstrap 在扩展安装前停止。到 `2026-10-13T10:35:50Z`，PID `34512` 和 4 个 CefSharp 子进程仍占用本次 profile；不强杀、不删目录、不重跑。没有 Worker、popup 或视觉/呈现帧证据。隔离记录见 [R05-03 当前复核](evidence/R05-03-CURRENT-MAIN-RECHECK-20261013/README.md)。
- #960/run `38327521241` 已确认为 terminal failure，job `115069541561` “编译与测试” exit 1、13m54s；日志 endpoint 404/API 403，SDK 和根因仍未知。
- R08-01 的 SDK10 单类自动证据是当前 source-identical 复核；用户故障保持独立待验。Round3 中其余自动行为已满足，真实宿主/最终呈现/OS 输入、物理跨屏和 ETW/宿主性能边界仍按原状态保留。下一项执行 Round2 Q23-06 目录编辑器视觉复核，使用合成数据和现有 RenderHarness。

## 2026-10-13 Q23-06 目录编辑器复核与 #967 诊断确认

- 用户这次提供的 SDK10 diagnostics ZIP SHA-256 为 `A4D13BB58AC343DFDCFFA6FB4DA31257080F1E5128AF151C946B0AEB0E3BC409`，对应 #967 SDK10 job `115178885908`、artifact `11795654027`、源码提交 `983fee5`。`summary.json` 记录 SDK `10.0.401`、`outcome=passed`，恢复、编译及全部记录的测试步骤 exit 0。截图中的 workflow 红灯属于独立 SDK8 job `115178885652` 源码结构检查；其原始日志仍不可读。这个附件没有提供 #983 的日志，#968 已验证 SDK helper BOM 修复后的 SDK8/SDK10 两 job 成功。
- Q23-06 复用现有 `SettingsPathEditorService` 与字段/命令契约。RenderHarness 发现选中设置路径时共享 ComboBox 显示 DTO 类型名；内部 DTO 的 `Key`/`DisplayName` getter 改为 public，模板改用公开 `ItemTemplate` API，并按 `DisplayMemberPath` 显示 ComboBox.Text，保留自定义 ItemTemplate。
- 本机 SDK `8.0.423`，RenderHarness/依赖 Release build `0/0`。最终 synthetic `settingspathprobe` 在 Light/Dark、`1040×700`/`720×640 DIP`、有效/172 字符缺失路径下 `8/8`；完整输入和尾部滚动、错误说明尺寸、状态变化时四动作无位移、短窗滚动后动作完全可达均有运行时断言。合成哨兵和占位 Worker 内容/时间戳/目录清单不变，缺失目录未创建；未打开 Browse/Explorer、Open、Clipboard 或 Save。
- Playnite 定向回归 `209 total / 170 passed / 0 failed`；控制台标记 `39 skipped`，未计为通过。`validate-source.py`、XAML `24/24` 通过；WPF 静态审查 `0 errors / 29 warnings / 177 info`；完整 Render QA `render-qa OK`，报告 `0 PROBLEM` 行。截图、probe 报告、TRX、全量 Render QA 报告与 SHA 见[Q23-06 当前主线复核包](../ui-finesse-round2-20260913/evidence/Q23-06-CURRENT-MAIN-RECHECK-20261013/README.md)。
- Round2 Q23-06 视觉栏更新为“通过”；宿主仍“外部阻塞”，最终仍“未完成”。离屏 `1.00 DIP` 不是 Playnite/OS 呈现；真实 ACL 拒绝/网络共享、Explorer/剪贴板、宿主键盘/UIA/IME、物理 DPI/跨屏尚未验。Demo 源目录仍不可用，沿用恢复生产基线。
- 截至 `2026-10-13 11:44 UTC`，前置文档提交 #970/run `38386326043` 页面仍显示 `In progress`。Q23-06 变更尚待中文提交/push，推送后需另跟对应 Actions 至终态，区分本机 SDK8 与 CI SDK10。下一项可执行任务为 Round2 Q23-08 底部动作栏：先核对现有保存/取消/恢复默认/导入导出命令、短窗可达和危险/迁移确认，再决定是否只补当前证据。

## 2026-10-13 Q23-08 设置底部动作当前 main 复核

- 先核对现有实现：Playnite 管 Save/Cancel；`SettingsResetCatalog` 管单字段/分类/全部默认；portable settings 已有 preview/confirm/apply；导入/导出与确认、错误语义已存在。本批没有增造保存/取消按钮或替换服务/DTO。
- 只把 Reset All 确认接到共用提示方法，为实际按钮加测试名，并以内部 delegate 驱动原生 Yes/No 分支。实际按钮Click的No不改fingerprint；Yes仅修改安全草稿、路径和云端目标不动；`CancelEdit`能恢复进入本轮前的未提交草稿。导入提示No/Yes通过同一生产确认方法验证；已有source/preview测试锁定preview→confirm→apply。
- 隔离 SDK `8.0.423`，Playnite `net462`、tests `net472`。测试项目 Release build `0 warning / 0 error`；相关 filter `10/10`、取消回归`1/1`，共`12/12`；XAML `24/24`、`validate-source.py`、`git diff --check`通过。合成 RenderHarness Light/Dark × `1040×700`/`720×640 DIP` 四态均验证Reset All展开、四类重置动作滚动可达和迁移导入/导出完整可达。证据：[Q23-08复核包](evidence/Q23-08-CURRENT-MAIN-RECHECK-20261013/README.md)。
- 代码提交 main `本阶段代码提交哈希见证据同步记录` 的首次提交后重跑曾传错 `GSC_BUILD_COMMIT`，源码身份守卫阻止3项源码测试（其余7项通过）；使用精确HEAD重建后，10/10 filter、确认1/1、取消1/1通过。WPF静态审查0 errors/29 warnings/177 info。README记录拦截和精确构建身份，当前TRX替换旧身份结果。
- Round2 Q23-08 视觉栏已更新为通过；宿主 external、final incomplete。真实 Playnite Save/Cancel footer、原生文件对话框、实际导入/导出、physical DPI/跨屏、OS UIA/IME、呈现帧未验。Demo 源目录不可用，沿用恢复生产基线；没有使用真实设置或用户路径。
- 用户表示目前只能提供 #967，已给包对应成功 SDK10 job。之后 #970 SDK10 workflow job失败，#971 SDK8 主 job和SDK10 job均失败；只有exit code 1，匿名API 403、job页需登录且没有artifact。未获详细日志，实际选中SDK/根因仍未知，不据摘要盲改。推送Q23-08代码后继续跟新Actions到terminal，并分别记录SDK8/SDK10。
- 下一可执行项 Q24-01：查现有 fractional-DPI 字形/边线/图标能力，做有边界的逻辑缩放行为检查；真实物理屏与跨屏维持独立待验。保留其他任务未跟踪文件，当前证据目录的每张截图/TRX均由README索引；旧`.tmp/`清理受安全执行器拒绝，本轮不绕过。

## 2026-10-13 #970/#971 诊断与 R00/R01 freshness 当前复核

- 用户提供 #970 SDK10 ZIP (`155612D3…43EAE`) 绑定源码 `c9f9179`、CI SDK `10.0.401`：Release build `0/0`，Worker 取消终态测试期望 callback 1、实测 0。根因是测试未等待 token callback 注册完毕；测试改为使用 registration-ready 握手，生产 TaskCoordinator 未变。#970 SDK8 ZIP (`598C2CA6…64A8B6`) 同源 `c9f9179`、SDK `8.0.423`，两个完整 solution-build summary 通过。
- 用户提供 #971 SDK8 ZIP (`6AC19F26…ADA4C`) 绑定 source `5a6647c`、SDK `8.0.423`：Q14 toolbar 几何测试把 `TaskFilterPreset` 对象 ItemsSource 错换成 strings，`DisplayMemberPath=Name` 因而选中文字为空并量到隐藏/零尺寸 presenter。测试现在使用真实 preset DTO、“默认”选中项与可见且有尺寸的文本/presenter；生产布局未改。#971 SDK10 diagnostics ZIP 仍未提供。
- `599d2fd` 精确身份本机隔离 SDK `8.0.423`：Worker 完整 `357/357`、Q14 `1/1`；R00/R01 受影响类 `UiFinesseFoundationTests 9/9`、`WpfUiResourceDictionaryTests 139 passed / 39 skipped / 0 failed`、`UiAuditSourceTests 6/6`。39 个 skip 未计入通过。
- 当前 RenderHarness audit `168 snapshots / 116 warnings / 0 HIGH / 13 MEDIUM / 0 Fidelity / 0 failed routes`，E01–E20 引用/身份/样本/边界 `20/20`。13 条 MEDIUM 为窄布局自然换行与垂直扩展提示，作为真实风险保留。因 shared `WpfUiProduction.xaml` 与 `RenderHarness/Program.cs` 变更而 stale 的四个 R00/R01 记录，已在当前精确复跑后更新 baseline 到 `599d2fd`；freshness `14/14`。详见[R00/R01 当前复核包](evidence/R00-R01-CURRENT-MAIN-RECHECK-20261013/README.md)与[CI 诊断映射](evidence/CI-RUN-970-971-TEST-RECHECK-20261013/README.md)。
- #971 SDK10 job 的最终状态与失败根因仍待取证；用户到明天 09:00 无法回应期间，独立任务继续推进。下一项 Q24-01：核查 fractional-scale 字形/边线现有能力和任务条件，再做受控逻辑 scale 验证；physical DPI/跨屏继续待验。

## 2026-10-14 Q24-01 分数 DPI 受控离屏复核

- 先确认现有 Typography/ThemeAwareIcon/图标 Geometry 和 `RenderTargetBitmap` 能力；没有改生产 UI。新增 `Q24FractionalDpiTypographyBehaviorTests`：生产资源下用 STA WPF Visual 检查中英文字、16 DIP 图标与 1 DIP 描边，对目标 96/120/144/168/192 DPI 原生栅格化；正例检查文本 fit、目标 Bitmap DPI/像素尺寸与文字/图标/边线像素，负例覆盖裁切与永久 1.25 倍 RenderTransform。
- 代码提交 `3ca41ec9a5a3f3c6888db72ba3a6254b046dcd40` 后，以精确 HEAD 使用本机隔离 SDK `8.0.423` Release 重跑 `ProductionTypographyIconAndOneDipBorderRenderAtFiveExplicitTargetDpis`，结果 `1 passed / 0 failed / 0 skipped`；Playnite `net462`、测试 `net472`，VSTest `17.11.1`。当前工作台 Windows 11 `10.0.22000`，系统 DPI 144（150%）、只枚举 `DISPLAY1`。
- 五档 PNG 人工检查均可见中英文字、生产图标和边线，无明显裁字/消失；TRX、每张 PNG SHA、程序集 SHA 和计数见[Q24-01 证据包](evidence/Q24-01-CURRENT-MAIN-LOGICAL-DPI-RECHECK-20261013/README.md)。控件 Visual 继承源 DPI `1.5`；目标 DPI 是离屏 `RenderTargetBitmap` 参数，并非真实物理显示器切换，也不是缩放原尺寸截图。
- 本项受控视觉栏可记“受控通过”，Host 仍外部阻塞，最终仍未完成。未验证真实 Playnite、物理显示器 DPI、Popup/跨屏、OS 字体缩放、最终呈现帧、UIA/IME/读屏；无真实配置/存档/媒体/云端操作。#972/run `38421480496`（前置 SHA `8546acab`）现已终态成功：SDK8 `8.0.423` 与 SDK10 `10.0.401` jobs 均通过。Q24 代码 `3ca41ec9` 未包含在 #972 中；推送本批后仍需跟新 SDK8/SDK10 jobs 到终态。
- 下一可执行小批 Q24-02：复核当前 main 的真实 PageHost 短窗尺寸、首尾可达与宽窗恢复，记录内容区而非外层请求尺寸；沿用生产基线，不因离屏结果改换设计体系。

## 2026-10-14 Q24-02 PageHost 短窗/滚动与 Dispatcher 回归

- `5fcd68562ee79ef819af054fb14a189c2610d890` 补测生产 PageHost 的实际宽高。1366×900 DIP 窗口中 PageHost `1038.667×759.333`；960×700 为 `632.667×559.333`；960×560 为 `632.667×419.333`；1440×900 为 `1112.667×759.333`；恢复 1366×900 后与初始差异不超过 `1 DIP`。详情、选框和焦点保持可见。
- 合并运行重现 Tooltip behavior 遍历其他 STA Dispatcher 的 PresentationSource root 后抛 `VerifyAccess`。`74815b2b` 让 Tooltip/Popup 扫描只访问当前 scope dispatcher，并新增双 STA `HwndSource` 回归。隔离 SDK `8.0.423`、Release、Playnite `net462`/tests `net472` 精确身份 `9/9`：MediaInboxScroll `5`、R07 resize `1`、R05 Tooltip timing `3`。2,000 个合成收件箱条目保持虚拟化/有限视口，thumb-end 最后一行完整，页追加锚点和缩窗页尾动作可达。证据、TRX SHA 和实际 DIP 见[Q24-02 证据包](evidence/Q24-02-CURRENT-MAIN-PAGEHOST-RECHECK-20261014/README.md)。
- Popup/焦点/选框扩展过滤 `8/16`：Tooltip timing `3/3`、Popup lifecycle `1/1`；其余 8 项的 Popup 或焦点对象未建立，失败现场在[精确身份 TRX](evidence/Q24-02-CURRENT-MAIN-PAGEHOST-RECHECK-20261014/tests/R05-transient-surface-regression-74815b2b-exact.trx)。该组不视作通过，未确认失败根因，也未归因到 Dispatcher 修复。
- 本机使用隔离 SDK `8.0.423`；系统 SDK `9.0.302` 未用于本次验证。#973/run `38429051152` 对先前推送的 `bee29749` 检查时仍为 `In progress`。`5fcd6856`/`74815b2b` 本地提交与证据尚待该 run 终态后推送。
- 受控几何和 WPF 行为不等于真实 Playnite、第二物理显示器、UIA/IME/Narrator、最终 presented frame 或 ETW/宿主性能。#970/#971 三个补传 ZIP 经 SHA 确认是已审内容的相同副本；#971 SDK10 原始 diagnostics 仍缺，用户离线期间继续不依赖它的工作。
- 下一项：Q24-04 复核现有键盘次序/焦点路径。Q24-03 物理跨屏需要第二显示器，本机单屏条件下保持外部待验。

## 2026-10-14 Q24-04 键盘顺序行为补证

- 先检查已有行为：GamePicker 生产键盘处理与 R21 八入口真实 WPF `MoveFocus` 轨迹已存在。原断言只保证每入口至少两个焦点点、前向命中一个预期名称并且焦点留在页面，不能证明全部当前可见控件可达或反向顺序正确。
- 提交 `8707090901a3ee1e424c8bced9c79a170b9631bc` 仅增强 `R21KeyboardNavigationTraceTests`：对 Dashboard Shell、Overview、Save、Trainer、Media、Task、Maintenance、Settings 的 STA 窗口，逐一验证所有可见、启用、可 Tab 的 Button/TextBox/Selector 都在 Tab 与 Shift+Tab 轨迹内；反向交互控件顺序须与前向互逆；成功轨迹写入 TRX。没有生产 UI/命令/布局变化。
- 本机隔离 SDK `8.0.423`、VSTest `17.11.1`、Playnite `net462`/tests `net472`，精确提交身份 Release 定向 `8/8`：`GamePickerKeyboardBehaviorTests 6/6`、`R21KeyboardNavigationTraceTests 2/2`。八入口实际记录 90 个当前可见交互目标，两方向全部可达；TRX、程序集 SHA 与断言边界见[Q24-04 证据包](evidence/Q24-04-CURRENT-MAIN-KEYBOARD-RECHECK-20261014/README.md)。`validate-source.py` 与 `git diff --check` 通过。
- 因 `*Keyboard*.cs` 是 R00-08 登记源码，本次按精确身份重跑 GamePicker 和焦点轨迹后只更新 R00-08 baseline 到 `87070909`；freshness 重算 `14 records / 0 needsRerun`。其他 R00/R01 source commit 未改变，package identity 仍为 `not-provided`。
- 这只证明受控 STA 窗口里当前默认内容的 WPF 键盘焦点遍历，不证明完整数据状态下每一行/详情命令、按屏幕阅读顺序是否正确、物理 Tab 输入或 Playnite 嵌入。Q24-04 视觉栏仍待验，真实六页走查与宿主验收未完成。#973/run `38429051152` 在本次检查仍 `In progress`；本批代码与前两项代码提交尚待推送并跟 SDK8/SDK10 CI 至终态。
- 下一可执行项 Q24-05：核对现有 Focus/Selected/Current 状态与弹层关闭后的焦点返回；复用现有状态服务与 WPF 行为夹具，真实宿主仍单独标记。

## 2026-10-15 Q24-04 失败任务状态键盘路径补证

- 核对已有 `TaskCenterView`、筛选器、DataGrid、技术详情和页尾命令后，仅补 `R21KeyboardNavigationTraceTests`，没有新建业务服务/DTO或生产 UI。合成成功、运行中、失败 `TaskStatusDto` 中选择失败项，加载生产视图、展开技术详情，并执行 Tab/Shift+Tab 轨迹与反向顺序核对；Retry 可用、Cancel 不可用，命令 fixture 为 no-op 且未执行。
- 基于 `5838bb51` 的本机源工作树定向 Release 构建：SDK `9.0.302`、VSTest `17.14.1`、Playnite `net462`、tests `net472`，测试项目与引用构建成功，非完整 solution build。R21 `3/3`、R06 `2/2`、R23 `9/9`，共 `14/14`。测试源 SHA、DLL ProductVersion/SHA、脱敏 TRX 及行为边界见[Q24-04 业务状态键盘证据](evidence/Q24-04-BUSINESS-STATE-KEYBOARD-RECHECK-20261015/README.md)。
- 不改生产 XAML/VM/服务/DTO，无截图或真实呈现声明。Q24-04 的受控业务状态自动行为已补证；真实 Playnite、OS 键盘、完整页面阅读顺序、UIA/Narrator、视觉验收仍未完成，因此 Round2 总体状态保持未完成。R00-08 baseline 已绑定 `e221feb3`；freshness `14 records / 14 fresh / 0 stale`，package identity 未提供。run `38624032938` 的 SDK8.0.423 与 SDK10.0.401 jobs 仍在运行，未报本阶段远端完成。
- 外机 R09 用户称已从 Git 拉取最新，仍报 Expected `2,2,2,2` / Actual `1,1,1,1`、line151；片段缺少 HEAD/source/DLL/PDB identity。当前 main 同名测试第151行是颜色断言；旧断言或符号映射错配仍未确诊，不改生产模板/放宽断言。
- Q24-05 已在当前 main 有受控焦点语义证据；下一依赖项为 Q24-06 UI Automation，先复用并核对现有 Name/HelpText/LabeledBy/AutomationPeer。
## 2026-10-14 Q24-05 当前 main 焦点语义复核

- 用户新附加的 #970/#971 三份 ZIP 经 SHA-256 核对为已审包的完全相同副本：#970 SDK10、#970 SDK8、#971 SDK8；没有新增 #971 SDK10 diagnostics。独立工作不等待缺失包。
- 核对已有实现后复用 b20f1a4b 的 R21-07 回归及现有生产状态模板。当前 main 精确身份 cb31947b1148195e2b8750dbfc75ae0269b1978a；没有生产 UI/VM/XAML 变更。
- 本机隔离 SDK 8.0.423，插件 net462、tests net472、VSTest 17.11.1；Release build 0 warning / 0 error。R21FocusVisualRegressionBehaviorTests 1/1、R09FocusOutlineBehaviorTests 2/2、R23 navigation/settings tabs/production grids 4/4，合计 7/7。程序集 ProductVersion 与源 SHA 相符；TRX、MVID、source hashes 见 [Q24-05 证据包](evidence/Q24-05-CURRENT-MAIN-FOCUS-RECHECK-20261014/README.md)。
- 实际覆盖当前导航页选中态在焦点移出后保留、表格选中行和键盘焦点分离、浅/深主题、失焦/禁用及错误恢复负例；游戏选择层以 2,000 条合成游戏在滚动末端/主题切换后保留可定位焦点，关闭后回到可见触发按钮。因没有生产视觉变更，无截图或 RenderHarness 结果。
- 首个 STA WPF 测试进程关闭时输出 TextServicesContext/TextStore InvalidComObjectException 清理异常；测试进程 exit 0，TRX 通过，根因未知，未归因于产品。
- Q24-05 当前 main 受控行为记为已满足。真实 Playnite、Windows UIA/读屏、OS 级键盘、焦点环最终像素、物理 DPI/跨屏、presented frame、ETW/宿主性能仍待验。
- CI 检查：#973/run 38429051152 页面仍 In progress；#974/run 38436999880 的 SDK8 job 115433489365 与 SDK10-labeled job 115433489872 页均显示 Re-running jobs，logs 需要登录，实际 runner SDK 与终态未知。新提交后继续跟踪所有相关 Actions。
- 下一可执行项 Q24-06：复核当前 main UIA Name/HelpText/LabeledBy/原生 Pattern；Narrator 和真实 Playnite 另列外部待验。Q24-03 仍受第二物理显示器阻塞。

## 2026-10-14 Q24-06 UI Automation 当前 main 复核

- 先复核已有能力：R21 有 RangeValue/Toggle/Selector peer 行为，但生产 DataGrid 只在合成 ListBox 上验证 Selection pattern；11 个具名生产 DataGrid 中多个没有 Name/HelpText。没有发现任何生产 `AutomationProperties.LabeledBy`，现有输入控件使用直接 Name。没有为满足属性清单虚构表格标签关联。
- 提交 `af8ed430373c5e52dba77af534c689f6727bb91f` 给四页 11 个 DataGrid 增加语义 Name，并给原先无帮助文本的维护表增加简短 HelpText；原复制提示、命令、绑定、选中模式和滚动/虚拟化未动。新测试从实际生产 UserControl 获取所有表格 peer，运行时验证 Name/HelpText、Grid/Table/Selection Pattern、RowMajor；对生产媒体收件箱改变所选合成行并通过 Selection provider 读回新行名称。实际 Task 页图标关闭动作 peer 名称也通过。
- 进度行为复用现有绑定测试并增加 `AutomationLiveSetting.Off`、未知/越界值和更新时键盘焦点不转移负例。它证明受控 WPF 属性/焦点行为，不等于 Narrator 实际播报。
- 本机精确身份隔离 SDK `8.0.423`；Playnite net462 / Tests net472、VSTest 17.11.1、.NET Framework testhost 4.8.9181.0。Release build `0 warning / 0 error`，R21AutomationValueBehaviorTests `24/24`；R00-06 关联 MediaWindowAnchorContractTests + MediaInboxGeometryTests `13/13`。`validate-source.py`、`git diff --check` 通过；WPF 静态审查 `0 errors / 29 warnings / 177 info`。DLL SHA/MVID、脱敏 TRX 和具体范围见[Q24-06 证据包](evidence/Q24-06-CURRENT-MAIN-UI-AUTOMATION-20261014/README.md)。
- 改动命中 `MediaCenterView.xaml` 后，完整 freshness 初检只有 R00-06 需要复跑；其余 R00/R01 记录未命中。完成对应 `13/13` 后，将 R00-06 baseline 源身份更新至 `af8ed430` 并引用本证据；完整 freshness 应为 `14 records / 0 needsRerun`。package identity 未提供。
- Q24-06 状态为“代码完成待验收”：用户验收条件中的实际 Narrator/Playnite 读屏还未做，当前只验证了进程内 WPF peers。准确待验步骤、合成数据边界和 SDK8/CI SDK10 分账见证据 README。无布局改动，不跑 RenderHarness/截图。
- 用户 #970/#971 新附 ZIP 与此前已审包 SHA 一致；#971 SDK10 原始日志仍缺，不等待材料。推送后查新 SDK8/SDK10 Actions 至终态；本机 SDK8 不能代表 SDK10.0.401。
- 下一项 Q24-07 高对比组合；Q24-06 实际读屏/Playnite 留作外部待验。Q24-03 仍需第二物理显示器。

## 2026-10-14 Q24-07 高对比组合与动效行为复核

- 复用现有 `AdaptiveThemePaletteFactory`、Light/Dark/Follow 模式、资源字典和生产 Button/ComboBox/Disclosure 模板；没有新增服务、DTO 或命令。为生产根节点补充可继承的高对比/有效动效状态，设置页与 Dashboard 将状态传到各自生产视觉树。高对比压制玻璃/透明 Popup/阴影和动画；系统语义色应用于按钮、禁用选项、焦点与选中状态；恢复后按原主题、玻璃与动效设置恢复。
- 修正 Q06 行为夹具初始化新继承动效属性，避免 fixture 与真实 Dashboard/Settings 根状态不一致。R09 行为矩阵先确认正常模式动画时钟，再切换高对比检查按钮/Disclosure 时钟清除、稳定终态与恢复；另覆盖 Follow/Light/Dark、宿主深浅色、玻璃/动效请求开关、禁用/选中/焦点/Popup 正负例。WPF 直接构造的 KeyUp 不更新 KeyboardDevice，测试改为 routed KeyDown 正例及明确标注的受控释放态探针，不声称物理键盘输入。
- 提交 `0243c108eaa7f3d63430475dd87d292cfcfeb950`。本机隔离 SDK `8.0.423`，官方 `scripts/build.ps1 -SkipTests` Release solution `0 warning / 0 error`、XAML `24/24`；SDK 的隔离安装路径、VSTest `17.11.1`、net462/net472 与程序集 SHA 见[Q24-07 证据包](evidence/Q24-07-CURRENT-MAIN-HIGH-CONTRAST-RECHECK-20261014/README.md)。七个类逐类隔离合计 `13/13`，无失败/跳过。
- 相邻的 R05 Tooltip 类在提交身份下单独执行 `2/3`：唯一失败发生于显示合成窗口前，`GetCursorPos` 无法保存系统指针位置；没有启动 Tooltip 行为断言。失败 TRX 已单列，未绕过系统限制。它不纳入 Q24-07 的 13/13 分母。
- freshness 检查 `14` 项中 `4 needsRerun`：R00-01-02、R00-03、R00-05、R01-05；其他 10 项未命中，下一小批先重验并校正这四条。
- 受控 WPF 结果不是 Playnite 真实渲染/视觉签收。未真实切换 Windows 高对比、未验证原生输入、UIA/读屏、物理 DPI/跨屏、最终呈现帧或宿主性能；Demo 原目录不可用，沿用恢复生产基线。未接触真实用户数据。CI SDK8/SDK10 需随推送新提交持续查到终态；当前 Q24-07 为代码完成待 CI/外部验收。

## 2026-10-14 R00/R01 freshness 校正与 #978 CI 终态

- `ae8f3be9969fe8baa649d7ae39bb88f7c6498484` 上重跑受 Q24-07 共享主题/动效改动影响的 R00/R01 行为：官方本机 SDK `8.0.423` Release `0 warning / 0 error`、XAML `24/24`；5 个隔离 WPF 类合计 `163 passed / 39 skipped / 0 failed`。比较原断言覆盖后修正两项 XML 属性顺序假设和一项过时的动效源字符串断言；未改生产行为。TRX、SHA 与范围见[R00/R01 当前 main 复核包](evidence/R00-R01-Q24-07-CURRENT-MAIN-RECHECK-20261014/README.md)。
- baseline 只更新四条实际受源路径影响的记录：R00-01-02、R00-03、R00-05、R01-05。freshness 从 `4 needsRerun` 校正为 `14 fresh / 0 stale`；package identity 仍为 `not-provided`，不代表真实宿主/包验收。
- 用户新附的 #970/#971 三个 ZIP 哈希与此前已审材料完全一致：#970 SDK10 Worker cancellation 测试竞态、#970 SDK8 build summary、#971 SDK8 Q14 夹具 DTO/可见性错误；修复已在先前提交，归档映射见[诊断复核包](evidence/CI-RUN-970-971-TEST-RECHECK-20261013/README.md)。没有新增 #971 SDK10 日志。
- #978 / run `38509928015`（commit `ae8f3be9`）已终态失败。SDK8 命名 job `115675846585` 的“编译与测试”及 SDK10.0.401 命名 job `115675846886` 的隔离 Release 编译/测试步骤均失败；CI 页面显示诊断包 digest `11108fca…637e64` 与 `9a722711…eb5d18`。job 页要求登录，完整日志/包内容尚未取得，实际 runner SDK 与根因未知；Node.js 20 弃用告警不作为根因。未凭摘要盲改，本机 SDK8 不替代 SDK10。
- 本地 R00/R01 证据与 freshness 校正已齐；因 #978 CI 失败且根因未取得，代码阶段仍未通过远端门禁。受控证据不代表 Playnite 最终呈现。Demo 原目录仍不可用，未验真实高对比切换、OS UIA/Narrator/IME、物理 DPI/跨屏、presented frame、ETW/宿主性能；真实存档、媒体、云端均未访问。用户 2026-10-15 09:00 前无法回复，不等待其补件；下一项按依赖推进 Q24-08 长文案与辅助输入，先查现有绑定/焦点/复制能力并复用。

## 2026-10-14 Q24-08 长文案与辅助输入当前 main 复核

- 先盘点并复用现有生产 `GscCheckBox`、游戏选择 WPF composition/keyboard 路径、`DataGridClipboardBehavior` 和 `GscLongTextCell` 样式；未加服务/DTO/生产代码。只加强 `Q10CheckBoxHitBehaviorTests` 的双主题长标签/命令/UIA 正负例，以及 `R06ClipboardBehaviorTests` 的窄列长原值复制。
- 构建源基线 `ae3c565ed9cee48c2b8bbdf19bb20610c3a7d068` 加两份本批测试源 SHA；本机隔离 SDK `8.0.423`、MSBuild `17.11.48`、VSTest `17.11.1`，插件 `net462`/测试 `net472`；Release `0 warning / 0 error`，XAML `24/24`，四类隔离行为 `15/15`，`validate-source.py` 与 `git diff --check` 通过。程序集身份、TRX SHA 和覆盖边界见[Q24-08 复核包](evidence/Q24-08-CURRENT-MAIN-LONG-TEXT-ASSISTIVE-INPUT-RECHECK-20261014/README.md)。
- 受控覆盖：双主题长标签换行及完整 AutomationPeer Name/HelpText、Space 单次翻转/命令及参数、禁用态拒绝；合成 WPF TextComposition 的中文“中”提交/Enter 前后路径；82 DIP 列宽下 fake clipboard 收到完整原值，以及无选择/禁用/不支持输入负例。共享 DataGrid 样式测试仍仅作 XAML/resource 契约，不冒充 Tooltip 运行时视觉。
- 没有生产 UI 改动，不生成截图。未验真实 Playnite、Windows 中文 IME、Narrator/OS UIA、系统剪贴板、真实 Tooltip 像素、物理 DPI/跨屏、presented frame、ETW/宿主性能。Demo 目录缺失，沿用恢复生产基线；仅用合成数据，未访问真实用户配置、存档、媒体或云端。
- Q24-08 当前本机自动行为证据补齐，视觉与最终状态维持待验/未完成，宿主为外部阻塞。当前下一可执行项是检查 Q25-01 已有热态响应实现和证据在 main 上是否仍匹配；按现有门禁补证，不重建已存在功能。#978 两 lane 失败根因仍未知且日志需登录；#979 为前一文档提交的 Actions，须与本次代码提交的 SDK8/SDK10 新 run 分开跟踪到终态。

## 2026-10-14 Q25-01 当前 main 热态响应复核

- 旧专项包绑定 `1a58e4b`；当前 main 的 `LargeLibraryPerformanceTests` 后续已有 30 个唯一查询、每个结果集必须改变的行为门禁。本次只复核现有实现，不改性能代码。精确提交 `a78c46b59836bb4dfb408e4bdd467b6e1597e1e4` 本机隔离 SDK `8.0.423`、Release `0 warning / 0 error`、XAML `24/24`；四类隔离 `13/13`：LargeLibrary `5/5`、R18 continuous `1/1`、R18 Dispatcher visibility `1/1`、GamePicker keyboard `6/6`。
- Q25-01 主 2000 项输入到 `FilteredCount` 基准先预热5次、测30次，30个不同查询全部改变可见 ID 集合；p50/p95/max=`45/60/60ms`，p95≤100ms 门禁通过，超时负例通过。R18 2k/10k 连续查询 p95/max=`2.466/7.788ms`、`7.009/11.059ms`，过滤评估 `60,000/300,000`；没有将 `GC.GetTotalMemory(false)` 代理称为 ETW allocation。
- 同一提交下受控 WPF Window 20 个搜索样本从 `SearchText` 到可见 `ListBoxItem` 的配对总时间 p50/p95/max=`46.214/75.782/76.334ms`；VM 与容器增量分项在证据包。窗口可见容器不是 Playnite 宿主或屏幕 presented frame。GamePicker 键盘/IMEProcessed/组合输入/焦点回返类 `6/6`，其中中文 TextComposition 是合成事件，不是真实 OS IME。
- 证据、源码/程序集 SHA、TRX 与原始样本见[Q25-01 当前 main 证据包](evidence/Q25-01-CURRENT-MAIN-HOT-RESPONSE-RECHECK-20261014/README.md)。Round2 Q25-01 自动行为为通过、实现记“已满足”；本轮无视觉变化（视觉不适用），真实 Playnite、物理输入、呈现帧/低 Tier/ETW 不在本机结果内，Host external、Final incomplete。
- `a78c46b5` commit 页面确认已推送，但最近公开 Actions 列表仍停留在 #979、没有显示本提交 run；commit checks 页为 404，匿名 API 不可访问。当前 CI 状态未知，SDK8 本机不替代 SDK10.0.401。下一项检查 R08-01 当前 main 的已有中途反向输入行为与证据；用户报告的原始失败日志仍未提供，不凭未知根因改代码。

## 2026-10-14 #970/#971 CI 诊断归因与当前 main 复核

- 用户重附 ZIP 均与已审副本 SHA-256 一致。#970 SDK10.0.401 包确认旧 Worker cancellation 测试在 callback 注册前发出开始信号，导致测试竞态；#971 所附 SDK8 包确认旧 Q14 fixture 用字符串替换生产 preset 对象集合、测量了错误/隐藏文本。两个夹具已由 `599d2fd3` 修正，未动生产 `TaskCoordinator` 或 UI。完整 run、SDK、附件 SHA 与错误摘要见[诊断复核包](evidence/CI-970-971-DIAGNOSTIC-TRIAGE-20261014/README.md)。
- 当前 main 精确身份 `a78c46b59836bb4dfb408e4bdd467b6e1597e1e4` 上，取消回调和 Q14 toolbar 几何定向测试在本机 SDK `9.0.302` 分别 `1/1` 通过。当前环境缺 `8.0.423`，本结果不替代仓库基线 SDK8 或 CI SDK `10.0.401`。Q14 runner 通过后仍打印 TextServicesContext/TextStore `InvalidComObjectException` 收尾噪声，根因未知。
- 公开 Actions 状态：#970/run `38386326043` Failure；#971/run `38393317198` 仍 In progress，附件只含 SDK8 诊断，未提供 SDK10 lane 日志。#972 的成功属于更早 commit；当前 `a78c46b5` CI 在公开列表中尚未出现。持续查询所有当前提交 SDK lanes 至终态，终态前不签阶段完成。
- 用户 2026-10-15 09:00 前无法回复；没有等待新材料。下一可执行项 R08-01：检查当前 main 已有中途反向输入实现与正/负行为路径；用户报告故障没有方法/断言/堆栈时不凭空修复。

## 2026-10-14 R08-01 当前 main 按钮 hover/press 反向行为补证

- 先核对现有覆盖：原 R08 类覆盖 Translate 与生产侧栏的中途反向，没有覆盖生产共享按钮模板 hover/press 的中途反向。仅增加行为测试，未改生产模板、动画、命令、绑定或业务代码。
- 隔离 Release build（本机 SDK 9.0.302、MSBuild 17.14.13、VSTest 17.14.1、.NET Framework 4.8.9181.0）0 warning/0 error；R08 行为 3/3，Q06 生产按钮五状态 Light/Dark 1/1；source validation、XAML 24/24、diff check 通过。实际样本覆盖退出/重新进入、释放/再次按压的当前值连续、目标方向和最终状态。测试采用 WPF 私有状态探针，不代表物理鼠标/键盘。
- 生产侧栏和按钮动画由真实共享模板驱动。动作样本、连续容差、源/DLL SHA/MVID、脱敏 TRX 见 [R08-01 证据包](evidence/R08-01-CURRENT-MAIN-RECHECK-20261014/README.md)。VSTest 成功退出后仍有 TextServices COM shutdown 清理噪声，根因未知。没有视觉改动，未生成截图。
- 用户另贴外机 R09 失败：旧测试预期 Chrome 边框 2 DIP、实际 1 DIP。提交 f13b9e05 已将断言校正为 Chrome 1 DIP、ValidationStroke 2 DIP；当前 main R09 类在 SDK9 复核 2/2。外机 HEAD/DLL/PDB 未提供，具体是否为旧分支/旧二进制未确认；证据单列于 [R09 外机失败复核](evidence/CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md)。
- CI：#971/run 38393317198 最新公开页仍为 In Progress，所附为 SDK8 诊断且没有 SDK10 lane 日志；#978/run 38509928015 为 Failure，完整日志需要登录，执行 SDK/根因未知。SDK9 本机结果不替代 SDK8 或 CI SDK10.0.401；推送后需跟新 run 到明确终态。
- 用户原报告的 R08 1/2 仍缺失败方法、断言、堆栈和 DLL 身份，当前自动通过不等于用户故障已修复。下一可执行项 R08-02 热关闭动画；真实 Playnite、系统/物理输入、物理 DPI/跨屏、最终呈现帧、ETW/宿主性能仍未验。

## 2026-10-14 R08-02 当前 main 应用/系统动效热关闭复核

- 先复用现有 `SystemParameters.StaticPropertyChanged` 订阅、Dispatcher 合并与 `GscMotion.NormalizeAll`，未重建动画管理服务。Settings 视图新增仅内部测试用的实例级 motion-policy provider；生产默认值为空，继续读取应用设置和 WPF 系统参数。
- 隔离 SDK `9.0.302` Release 构建插件 `net462` 与测试 `net472`，0 warning/0 error；应用 ToggleSwitch 热关闭 `1/1`，实际 SettingsShell 走系统参数处理回调的受控 policy-off/on `1/1`，生产侧栏 reduced-motion 两行为 `2/2`，Dashboard/Settings 回调订阅/卸载保护门禁 `1/1`。source validation、XAML `24/24`、diff check 通过。各 WPF 类分进程串行运行。
- 两类 SettingsShell 测试都在活动中的入场动画关闭时检查 Opacity/Translate 时钟立即清理并归一到 `1/0`，再开启策略并等待超过原周期，确认没有重播旧入场。系统策略用实例 provider 模拟，并调用真实 `OnSystemParametersChanged` 回调；没有修改 Windows 动画设置，也没有人为触发真实静态系统事件。
- 程序集身份、脱敏 TRX、源码与证据 SHA 见 [R08-02 当前 main 复核包](evidence/R08-02-CURRENT-MAIN-RECHECK-20261014/README.md)。测试成功后 TextServices COM 清理输出仍在，根因未知。无视觉改动，不生成截图；真实 Playnite、Windows 设置通知、物理 DPI/跨屏、呈现帧、UIA/读屏、ETW/宿主性能仍待验。
- 推送后分别跟 SDK8 与 SDK10.0.401 Actions 到终态。R08-03 当前 main 复核见下节；下一项 R08-04 业务完成节奏。R08-01 用户原始 `1/2` 故障仍缺外机身份与堆栈，不阻塞其他独立工作。

## 2026-10-14 R08-03 当前 main 隐藏/最小化停机复核

- R08-03 已有 `IndeterminateProgressBehavior` 与共享 ProgressBar 模板。因 `0243c108` 以来样式、页面 XAML 与测试源有变化，核对当前 SHA 后刷新行为证据；没有改生产代码或重建现有服务。
- 当前 main `31d64e42` 上隔离 STA 测试 `IndeterminateProgressPausesForHiddenTabAndMinimizedWindowThenResumes` `1/1` 通过：真实共享模板的 indicator 可见时移动超过 `0.5 DIP`；切换 Tab/最小化各等待 `360ms` 后 X 位移保持到小数后三位，忙态仍为 `IsIndeterminate=true`；返回可见/还原窗口后位移再次变化。
- R08-02 隔离 Release build 在提交前对相同源码字节完成，SDK `9.0.302`、0 warning/0 error；ProductVersion 标记保留父 commit `d3cb0b2d`，当前 source/assembly SHA、MVID、脱敏 TRX 在[R08-03 当前 main 证据包](evidence/R08-03-CURRENT-MAIN-RECHECK-20261014/README.md)。提交后新隔离重建在 MSBuild ProjectReference graph 阶段 exit1，完整诊断只显示 `_GenerateRestoreProjectPathWalk` / `_GetProjectReferenceTargetFrameworkProperties` 失败及 `0 warning / 0 error`，无 MSB/NU 根因；不把重建写成通过，也不据此改代码。
- CI 公开页当前只可见 #979 (`ae3c565`) queued；#971/run `38393317198` In Progress，#978/run `38509928015` Failure 且完整日志需登录；没有可核实的 `31d64e42` run。SDK8/SDK10.0.401 仍待各自终态。受控逻辑 DIP 不代表真实 Playnite 呈现、物理最小化、宿主性能、UIA/读屏或 ETW；无视觉改动，不生成截图。
- R08-03 受控行为已满足。下一项 R08-04：先盘点现有 DTO/真实任务状态和反馈路径，再补明确缺少的行为/负例；R08-01 用户原始 `1/2` 故障仍缺外机方法/断言/DLL，不阻塞该独立任务。

## 2026-10-14 R08-04 当前 main 业务完成节奏复核

- 核对并复用 `BusyOperationCoordinator`、共享生产按钮 `IsBusy`、`FeedbackToast` AutomationPeer、`DashboardView.ApplyToastAutomation` 与 `TaskStatusDto`；生产命令、绑定、忙态/取消/错误语义及模板均未改。只补一条成功慢请求生命周期行为，并把成功/失败/取消 DTO 的状态/详情由宽泛包含断言改为精确值断言；失败样例验证错误码与失败详情优先于阶段消息。
- 本机只有 SDK `9.0.302`，仓库 `global.json` 要求 `8.0.423`；隔离 Release solution build `0 warning / 0 error`（Playnite `net462`、tests `net472`）。`R02BusyStateTests 5/5`、`R08BusinessFeedbackBehaviorTests 4/4`，合计 `9/9`、0 skip/fail；source validation、`git diff --check` 通过。程序集 SHA/MVID/ProductVersion、源 SHA 和脱敏 TRX 见[R08-04 当前 main 复核包](evidence/R08-04-CURRENT-MAIN-RECHECK-20261014/README.md)。初次未设置 `GSC_BUILD_COMMIT` 的隔离测试被源码身份门禁拒绝；按仓库 build 脚本约定重新构建后两类均通过，不计初次运行。
- WPF 技能静态检查 `0 errors/29 warnings/177 info`，扫描包含未改动的 `.tmp/r12-04-stage` 历史布局 warning；本阶段未改 XAML。自动证据为合成任务与隔离 WPF 行为，不证明真实 Playnite/Worker 长任务、Narrator/外部 UIA、通知宿主时序、物理 DPI/最终呈现、ETW 或宿主性能。
- 公开 Actions 列表仍只显示到 #979 queued；#971/run `38393317198` 仍 In progress，#978/run `38509928015` Failure 且日志需登录。最新 R08 提交和本阶段 SDK8/SDK10.0.401 结果未从公共页核实；本机 SDK9 不替代两条 CI lane，CI 终态前不记门禁完成。下一项 R08-05 页面切换轻量化；R09 外机的断言/PDB 与当前源码行号不一致仍待外机 HEAD/DLL/PDB 证据，独立不阻塞。

## 2026-10-14 R09-05 外机失败当前 main 复核

- 当前 main 965fe1b2 使用隔离 SDK 9.0.302、显式提交与源码根身份构建 Playnite 测试项目及引用项目；R09FocusOutlineBehaviorTests 2/2。此次是定向项目构建，不是全解决方案 build；552b8168 同代码源此前全量 SDK9 Release 0/0。测试源与 WpfUiProduction.xaml 哈希、程序集 SHA/MVID/ProductVersion、PDB SHA 和脱敏 TRX 见外机失败复核包。
- 外机日志第151行及 Expected 2 / Actual 1 对应历史 3f7d0b30 的 Chrome 2 DIP 断言；当前第151行是颜色断言，第154行检查 ValidationStroke 2 DIP。旧测试源码/程序集或 PDB 错配最符合现有证据，因外机 HEAD、源文件、DLL/PDB 身份缺失仍未证实。
- libpng iCCP 与断言无关；TextServices COM 异常出现在断言失败后的 WPF 收尾。当前 main 未改生产样式或降低断言。真实 Playnite/OS 输入/物理 DPI/读屏/最终呈现未验。
- Actions 当前公开快照仍到 #979 queued，#971 In progress、#978 Failure 且日志需登录；没有可核实的 965fe1b2 run，按 SHA 查公开 REST API仍受 HTTP 403 rate limit。SDK9 结果不替代 SDK8/SDK10.0.401 CI。用户至 2026-10-15 09:00 前无法回复，外机身份待补但不阻塞其他任务。

## 2026-10-14 R08-05 页面切换当前 main 复核

- 现有生产页缓存与 PageHost 行为已满足任务方向，没有重建页面服务或动画体系。本次没有生产代码/XAML 改动，只复核真实 shell/page 行为并刷新当前 main 证据。
- 隔离 SDK 9.0.302 Release solution build 0 warning/0 error；R08PageSwitch 4/4、ProductionShellChrome 22/22、UiFinesseFoundation 9/9、R02 Busy 5/5、R08 business feedback 4/4，共 44/44。source validation 通过，XAML 结构 24/24，文档同步后的 diff check 另行校验。详细 SHA/MVID/TRX 在 R08-05-CURRENT-MAIN-RECHECK-20261014。
- 真实 WPF 页面切换保留 DataGrid offset 10→10、任务引用、选中项与 DataContext；两次不同页面切换各 Measure/Arrange 1/1，同页重入 0/0；96 项集合和筛选/页签状态恢复，refresh command 0 次。七次快速路由后导航标题/焦点一致；PageHost 无 Blur/整页入场动画。
- ShellChrome 22 项均通过、TRX exit 0；testhost 收尾出现 TextServices COM 异常输出，根因未知。误用类名的零测试筛选器未计入 44/44。
- 本机 SDK9 不替代仓库 SDK8.0.423 或 CI SDK10.0.401。公开 Actions 仍只显示 #979（ae3c565），#971 In progress，#978 Failure 且日志需登录；推送的 R09 文档提交 8dda9995 尚未显示 run。真实 Playnite、物理 DPI/跨屏、presented frame、UIA/读屏、ETW/宿主性能仍待验。
- R08-05 当前 main 受控条件已满足，视觉不适用，不生成截图。下一可执行任务 R07-08 当前 main resize 复核；先查现有实现/测试及 #960/#968 的提交边界，不凭旧失败摘要猜修。

## 2026-10-15 R06-07/R06-08 当前 main、Q24-06 与 R09 复核

- 本轮复用既有 Save 空态/恢复操作、WorkspaceStatePresenter、Media 状态缓存、详情选择/滚动、四行预算、生产 DataGrid Automation peers 与共享焦点描边；没有改生产服务、DTO、XAML、命令或滚动系统。
- main `cd27c8c0aba8616331867a58af13376f44e958f0`，本机 SDK `9.0.302`、VSTest `17.14.1`，Playnite `net462`/测试 `net472` 定向 Release 测试项目及引用 build `0 warning / 0 error`。R06-07 `17/17`、R06-08 `14/14`、Q24-06 `24/24`、R09 `2/2`，共 `57/57`。`validate-source.py`、XAML `24/24`、`git diff --check` 通过；非完整 solution build。精确源码、XAML、DLL/PDB SHA 与九份脱敏 TRX 见[当前 main 复核包](evidence/R06-07-08-Q24-06-R09-CURRENT-MAIN-RECHECK-20261015/README.md)。
- 用户提供的 R09 报告仍缺机器实际 HEAD/工作树/测试源/DLL/PDB 身份。当前同名测试 line151 核对错误色，line152 核对 Chrome 1 DIP，line154 核对独立 ValidationStroke 2 DIP；外机 line151 和 2 对 1 的厚度值形态与历史断言吻合，但不能证明实际源码/符号版本。当前 main 行为 `2/2`。不把旧源/符号错配说成已确认，不改模板或降低断言。libpng warning不在断言栈；COM异常在断言失败后的WPF清理。
- #962/#963 所附包的失败属于 R08 数字动效测试，#967 SDK10 包中 R09 `2/2`；#970 SDK10 Worker 取消回调注册时序与 #971 SDK8 Q14 可见 presenter 夹具错误都已在 `599d2fd3` 修复。用户提供的 #971 SDK10 lane 日志仍缺；不得以 SDK8/SDK9 替代。
- #1006/run `38624032938`（代码提交 `e221feb3`）和 #1007/run `38625232841`（文档提交 `cd27c8c0`）的 SDK8.0.423 与 SDK10.0.401 lane 均 Failure。SDK8 job `116058522011`/`116062580362` 失败于“编译与测试”，SDK10 job `116058522588`/`116062580995` 失败于隔离 Release 编译/测试步骤；当前匿名 API 403、step-log路由404，诊断 ZIP没有取得，根因未知。artifact 元数据和 SHA 见[当前复核包](evidence/R06-07-08-Q24-06-R09-CURRENT-MAIN-RECHECK-20261015/README.md)。本机 SDK9 不替代 CI。受控 STA/AutomationPeer 不等于真实 Playnite、Narrator、OS 输入、物理 DPI/跨屏、最终呈现帧、ETW 或宿主性能。无截图、未访问用户存档/媒体/云端。
- 下一项按用户要求先复核 R00/R01 问题与证据；随后继续推进依赖已满足的 Q/R，确认已有能力后补当前身份，不重建功能。

## 2026-10-15 R10-02 当前 main 定位游戏复核

- 当前生产实现沿用 `OpenSelectedTaskGameCommand`、`TaskSourceNavigationResolver`、`GamePickerViewModel` 与全局 Shell 选框；只补选框 UI 行为测试。SDK8.0.423/VSTest17.11.1 下 Release 项目/引用 build `0 warning / 0 error`，XAML `24/24`，R10 context `5/5`、相邻导航 `20/20`、生产选框 STA 行为 `7/7`，合计 `32/32`。
- 生产 Shell 从顶部按钮打开选框并聚焦搜索；同名两项中选择 `playnite-b` 后检查实际对象、稳定 ID、关闭状态、返回焦点和当前名称绑定。任务 ID 缺失负例仍保留当前游戏/工作区、不建立返回路线、不请求加载并给刷新提示。
- 定向构建不是完整 solution；测试使用合成 DTO、隔离 testhost 和合成路由输入，不代表真实 Playnite、媒体后端、物理键鼠/IME、UIA/读屏、最终呈现、物理 DPI/跨屏或宿主性能。详细程序集身份、脱敏 TRX 与外机 R09 边界见 [证据包](evidence/R10-02-CURRENT-MAIN-RECHECK-20261015/README.md)。
- 本机 SDK8 不替代 SDK10.0.401。`6eac7b90` 与证据提交的 Actions run ID/终态待查询；#990 旧失败日志 API 403、根因未知。外机 R09 仍无 HEAD/DLL/PDB，不能据 `Expected 2 / Actual 1` 断定其程序集来源。下一项 R10-03 搜索快捷键当前 main 复核。

## 2026-10-15 R10-03 当前 main 搜索快捷键行为复核

- 在现有 Ctrl+F 策略/路由基础上仅提取共享行为入口，并增加实际 routed-event 与当前 Media 页焦点验证；没有重建搜索功能或更换视觉体系。SDK8.0.423 Release 定向 build `0/0`、R10 相关组 `13/13`，R09 当前源码对照 `2/2`。
- 本机 source validation 与 diff check 通过；WPF 扫描 `0 error`，warning/info 来自现有无关 XAML/临时目录。行为、程序集身份和未验边界见 [R10-03 证据包](evidence/R10-03-CURRENT-MAIN-RECHECK-20261015/README.md)。
- `95109c2` 对应 run `38645600070` 上次观察为 In progress；当前代理连接拒绝、`gh` 不存在、浏览器读取超时，状态未复核。新阶段提交的 Actions run 尚待推送后读取，SDK8 本机不能替代 SDK10.0.401。外机 R09 仍未取得 HEAD/DLL/PDB；下一项 R10-04 快捷键帮助可独立推进。

## 2026-10-15 Q24-02 当前 main transient Popup / 焦点 / 选框复核

- 当前推送提交 `6b946540` 是文档提交，生产源码父项为 `88b8eb57`。本机 SDK `8.0.423`、MSBuild `17.11.48`、VSTest `17.11.1` 完整 Release solution build `0 warning / 0 error`，XAML `24/24`；Playnite `net462`、测试 `net472`。
- 旧 `74815b2b` 失败涉及的三个目标类按仓库正式的“每个 WPF 类独立 VSTest 进程”方式重跑：Popup boundary `2/2`、focus boundary `5/5`、option virtualization `5/5`，合计 `12/12`。Popup lifecycle `1/1`。原始 Popup/焦点/虚拟化失败在当前这三组隔离运行中未复现；测试源码文件与 `74815b2b` 版本相同，生产实现源码后来有更新。没有据旧结果改模板或放宽断言。
- 补充同进程诊断运行 16 项时为 `11 passed / 5 failed`：4 个 focus/window activation 断言与 1 个系统鼠标 Tooltip 夹具前提失败。单类隔离时 focus 类 `5/5` 通过；Tooltip 类独立运行 `2/3`，其 `QuickMousePassesCancelTooltipAndAStationaryDwellOpensIt` 在打开测试窗口之前因 Win32 `GetCursorPos` 返回 false 而停止。保留此桌面输入限制，未绕过系统鼠标 API，也未把合并运行失败归因为产品根因。具体脱敏 TRX、DLL/PDB 与源 SHA 见[Q24-02 当前 main 复核包](evidence/Q24-02-CURRENT-MAIN-RERUN-20261015/README.md)。
- 同一 `6b946540` 完整构建输出中的外机 R09 焦点描边类另行 `2/2` 通过；当前第151行颜色断言与外机的 `Expected 2 / Actual 1` 仍不匹配。外机 HEAD/DLL/PDB 未知，根因未确认；见[R09 外机对照](evidence/CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md)。
- `6b946540` 对应 GitHub Actions run ID/终态未知：公开列表报加载错误且展示不同 SHA 的缓存项 `8546aca`，匿名 API 返回 `403 rate limit`。SDK8 本机结果不替代 CI SDK10.0.401。没有视觉代码改动；真实 Playnite、系统鼠标/键盘输入、UIA/Narrator、物理 DPI/跨屏、presented frame、ETW 与宿主性能仍未验。Q24-03 等第二物理屏，Q25 真实呈现/跟踪权限不绕过。
- 下一可执行动作：可用交互桌面恢复后单独重跑 Tooltip 系统鼠标测试；用户明早可回复后，对照外机同一次失败的 `HEAD`、工作树、测试源码与 DLL/PDB SHA。当前没有证据支持进一步生产模板或行为修改。


## 2026-10-15 CLOSE-WRAP 当前 main SDK8 全量门禁复核

- 基线 main b04660e0；生产 UI 没有变更。唯一源码改动将 WorkspaceTreeGovernanceTests 中 Trainer/Media 的过时 getter 字符串检查更新为通用 registry callback 与 workspace 路由。实际 PageHost、导航、焦点测试8/8。
- 本机隔离 SDK8.0.423、MSBuild17.11.48、VSTest17.11.1。Release solution build 0 warning/0 error，XAML24/24；Core125/125、Worker356 passed/1 skipped、Playnite source455 passed/27 skipped。WPF122类648 passed/39 skipped/1 failed，121类无失败。类结果表见 [manifest](evidence/CLOSE-WRAP-CURRENT-MAIN-RECHECK-20261015/tests/wpf-class-results.json)。
- Q14 654 DIP 单行无额外gap当前1/1通过，旧 backlog 的复现描述过期。唯一失败R05TooltipTimingBehaviorTests.QuickMousePassesCancelTooltipAndAStationaryDwellOpensIt在调用window.Show()前GetCursorPos返回false；没有实际显示窗口或执行光标/tooltip动作。其余78类隔离为481 passed/39 skipped/0 failed。全体计数1584 passed/67 skipped/1 failed，本机门禁未全绿。
- 不改Win32 API、不把该项记skip、不写成通过；需交互式桌面重新执行真实系统指针用例。SDK8结果不能替代SDK10.0.401。用户附加CI ZIP确认与既有归档相符，详细失败修正见R08-06、CI-965、CI-SDK10-BUILD-VERIFY与CI-970-971证据。
- 下一可执行动作：本阶段提交推送后查询两条Actions至终态；并按授权在当前main重新核对R00/R01 freshness，然后继续CLOSE-OPTICAL-01。未验证Playnite宿主、物理DPI/跨屏、呈现帧和ETW权限。

## 2026-10-15 `db073461` R00/R01 freshness 与外机 R09 续核

- 当前 source commit `db073461994062d6b1849948f105f97f3ff15f53` 上重跑 R00/R01 freshness：14 条全部 `needsRerun=false`，package identity `not-provided`；完整报告为 `evidence/R00-R01-CURRENT-MAIN-RECHECK-20261015/ui-evidence-freshness-db073461.json`。这是源路径 freshness 结论，不是重新构建或宿主验收。
- 用户新贴的外机 R09 日志仍是 line151 `Expected 2 / Actual 1`。当前源 line151 检查错误色，line152 Chrome `1 DIP`，line154 独立 ValidationStroke `2 DIP`；当前测试源与 PDB 映射不符的线索仍不能确定外机加载物或根因。
- 锁定 SDK8.0.423 / MSBuild17.11.48 / VSTest17.11.1 使用本机现有 assets 做隔离 Release 重建，0 warnings / 0 errors；Playnite net462、测试 net472，R09 单类 `2/2`。测试/插件 DLL、PDB、MVID、TRX 与脱敏日志见[R09 外机复核包](evidence/CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md)。没有改生产模板或放宽断言。
- #1023/run `38671542992` 对应 `db073461`，SDK8 job `116215565491` 与 SDK10.0.401 job `116215565679` 均失败于编译/测试步骤；公开页只有 exit code，匿名日志 API 返回403，完整 artifact 未取得，根因未知。本机 SDK8 结果不代替 CI SDK10。当前 CLOSE-WRAP 完整门禁另有系统 `GetCursorPos=false` 前提失败，真实宿主/物理跨屏/呈现帧/ETW 仍未验。
- 下一独立 Q/R 项 R12-04：复用恢复前保护备份流程与 DTO，用 fake service/隔离目录验证保护备份失败确实阻断危险恢复、重试/取消遵守同游戏锁和最新状态；不访问真实存档。

## 2026-10-15 R08-02 CI 失败诊断与夹具修正

- #1024/run `38677622085` 对应 `c30b89e2`，SDK8 job `116234717488` 与 SDK10.0.401 job `116234717135` 都在相同 R08 app motion 行为测试失败；完整日志通过已配置 GitHub 插件取得。失败位置报告 `opacity=1; loaded=True`，不是断言之后的 TextServices COM 清理异常导致。
- 初次 Settings 主题应用依赖真实 Windows 动效策略并会重写 duration 资源；两个测试夹具现在在初次显示/主题应用后再注入确定时长。生产代码未改。锁定 SDK8.0.423 Release test project build `0/0`；R08 应用设置和系统偏好行为各 `1/1`；source validation、diff check 通过。源/产物 SHA 与 TRX 见[本阶段证据包](evidence/CI-R08-MOTION-POLICY-FIX-20261015/README.md)。
- 新提交的 SDK8/SDK10.0.401 Actions 仍须查询并跟到终态；本机 SDK8 结果不替代 CI。真实 Windows motion notification、Playnite host、物理 DPI/跨屏、呈现帧、UIA/读屏、ETW/宿主性能未验。

## 2026-10-15 R08-02 system-motion 加载时序复核

- #1025/run `38687483121` 已终态失败。SDK10.0.401 job `116265550450` 完整 log 指向 system-motion 测试第56行 opacity animation clock 未启动；`Show/UpdateLayout` 后仍有 Loaded handler 与测试时长设置的时序竞争。测试现在先 drain dispatcher、重做 layout，再设置测试时长并触发实际动画，然后通过生产 `OnSystemParametersChanged` 路径验证关停和不重播。只改测试夹具，不改生产代码，不弱化断言。
- SDK8.0.423 job `116265550259` 同 run 在 R09HighContrast 行为矩阵报告第195行 Assert.True（Expected True / Actual False）；完整 log 无定制状态上下文，本机当前源码同 SDK 类测试 `3/3`，没有复现，根因未证实，不能将其记为通过或归因于此轮改动。
- 锁定 SDK8.0.423/VSTest17.11.1 Release test project build `0/0`；R08 system/app-motion `1/1` 各通过；R09 high-contrast `3/3`。source validation/diff check 通过；TRX 与 #1025 artifacts 的身份见[本阶段证据包](evidence/CI-R08-MOTION-POLICY-FIX-20261015/README.md)。
- 下一步提交推送当前夹具修复并追踪新 SDK8/SDK10.0.401 Actions 至终态。R08 CI 收口后按用户顺序更新 R00/R01 freshness，再推进 CLOSE-OPTICAL-01。外机 R09 `Expected 2 / Actual 1` 仍缺同次 HEAD/DLL/PDB；真实 Playnite、OS 输入/UIA、物理 DPI/跨屏、presented frame、ETW/宿主性能仍待验。

## 2026-10-15 Actions #1026 Worker 取消竞态收口

- 旧提交 `8e651385` 的 SDK10.0.401 完整日志确认 `TaskCoordinator.CancelAsync` 访问已释放的 `SemaphoreSlim Gate`；根因是运行时从字典移除后并发取消者仍持有引用。修复移除提前 `Dispose`，并保留重复取消幂等快返；没有改变业务取消或错误语义。
- 本机 SDK8.0.423 Release solution build `0 warning / 0 error`；Core `125/125`、Worker `356 passed / 1 Named Pipe 环境 skip / 0 failed`、取消目标 `1/1`、R09 高对比类 `3/3`。Playnite 122 类扫描仅完成 6 类后中止，未全量验收。实际 TRX：[证据目录](evidence/CI-WORKER-CANCEL-RACE-FIX-20261015/README.md)。
- 新提交 Actions 未执行/未验，需待推送后查询 SDK8 与 SDK10.0.401 job 至终态。后续按用户顺序重查 R00/R01 freshness 并推进其行为证据，再处理依赖已满足的 Q/R；CI 失败先读完整日志。真实 Playnite、物理跨屏、最终呈现、OS UIA/IME、ETW/宿主性能仍未验。
- 本轮删除已完成且未被证据引用的 `.tmp`，净释放约 588 MB；保留 SDK 和明确被历史 README 引用的产物。

## 2026-10-15 main 5862856e R00/R01 freshness recheck

- `scripts/check-ui-evidence-freshness.ps1` 对 `5862856e508fdb0abf2478fbbc54c34b34075692` 复查 14 条登记记录：`14 FRESH / 0 STALE`，0 条命中 source path，package identity `not-provided`。JSON 与 SHA 见[R00/R01 当前 main 复核包](evidence/R00-R01-CURRENT-MAIN-RECHECK-20261015/README.md)。这一步只证明既有证据未因登记源文件变化而失效，不是行为重跑或真实宿主验收。
- R00/R01 表中“已满足”状态依据各自已有构建、行为、负例和审计证据保持不变；没有因 freshness 自动签收缺失条件，也没有重建已存在功能。
- 最新 Worker 提交 `5862856e` 的 Actions run ID/终态当前未从 GitHub 插件状态接口取得，公开 Actions 页缓存只到 #979 并报加载错误；CI 门禁保持未确认，后续继续跟踪。
- 下一项按已记录依赖推进 R12-04：先查已有 PreRestore 保护备份 service/DTO 和 game lock；使用 fake service、隔离目录/合成状态验证失败阻断、重试、取消与锁，不触碰真实存档。

## 2026-10-15 R09 高对比按压状态 CI 修正

- GitHub 插件取得 #1027/run `38698959702` 与 #1028/run `38701408503` 的 SDK8/SDK10 完整 logs。两次两条 SDK lane 都在 `R09HighContrastBehaviorTests.HighContrastCombinationsKeepDisabledSelectedFocusAndPopupReadableAndRecoverMotion` 报 `Assert.True` Expected True/Actual False，栈为测试源 line 195；Core `125/125` 与 Worker `357/357` 通过。失败后出现的 WPF TextServices COM shutdown 异常也会在本机 pass 后出现，不是断言根因。
- 失败发生在用 `RaiseSpaceKey` 构造的合成 routed `KeyDown` 后。夹具改用现有 `HighContrastStateProbeButton` 确定驱动 `IsPressed`，仍检查生产模板 press/release、overlay、focus ring、动效状态、disabled/selected/popup 对比。未改生产样式。Q06 独立类仍检查合成 Space routed state；物理键盘仍未验。
- 本机 SDK8.0.423 / MSBuild17.11.48 / VSTest17.11.1 Release 测试项目 build 成功；R09 `3/3`，Q06 `1/1`。TRX、源/产物 SHA 与 #1027/#1028 artifact ID/SHA 在[证据包](evidence/CI-R09-PRESSED-STATE-RECHECK-20261015/README.md)。没有在工作区保留诊断 ZIP、DLL/PDB 或离屏图。
- 修正提交推送后仍须追踪对应 SDK8 与 SDK10.0.401 CI 至成功/失败终态；本机 SDK8 不可替代 SDK10。
- 下一项：R12-04 的 ledger 行与 `e4e42f40` 证据已经标为“已满足”。先核对当前 main 相关源码与测试是否发生变化；无变化时沿用已有 fake/隔离证据并补 freshness 结论，若当前依赖改变则只针对实际缺口复验。真实存档不触碰。

## 2026-10-15 R21 UIA Selection 子树复核（新 CI 待验）

- Actions #1030/run `38712426496` 的 SDK8 和 SDK10.0.401 jobs 均因父级 `GetSelection()` 返回空集合失败。两个 job 的完整日志确认先前补入的 SelectedItem、唯一 SelectedItems、DataGridRow 检查都通过；根因尚未确认。详见[evidence](evidence/CI-R21-SELECTION-PEER-ENUMERATION-20261015/README.md)。
- 新夹具实际从 DataGrid peer 子树按行名取得 `DataGridItemAutomationPeer`，用 `ISelectionItemProvider.IsSelected` 验证首行选中及切换后的旧行取消/新行选中，并继续验证父级 Selection pattern 的返回项。只改测试，没有变更生产控件。
- 本机 SDK8.0.423 / MSBuild17.11.48 Release build `0/0`；R21 `24/24`，TRX 已归档。代码修订需 push 后确认 SDK8/SDK10.0.401 Actions 终态；未完成 CI 不签收阶段。
- `.tmp` 旧隔离输出约 `374 MiB`。自动审查两次拦截对列出的旧目录进行递归删除，理由包含目录内日志/TRX及不明 PID `32408`；未清理、未绕过。用户提出希望清理无用文件；待用户明日可回复时申请这些具体路径的确认，当前继续主任务。

## 2026-10-16 R21 UIA 客户端 Selection 查询复核

- #1031/run `38834172655`（推送提交 `2252fad7`）SDK8.0.423 job `116759418158` 与 SDK10.0.401 job `116759417854` 均已终态失败：`ProductionMediaInboxSelectionPatternTracksTheSelectedRow` 的父级 `ISelectionProvider.GetSelection()` 返回空集合。完整日志显示两条 lane 的 Release build `0/0`、Core `125/125`、Worker `357/357`；SelectedItem/SelectedItems/DataGridRow 与行 peer 选中状态前置断言通过，R21 其余 `23/24`。COM teardown 噪声在 xUnit 失败之后。失败/ZIP 身份详见[evidence](evidence/CI-R21-SELECTION-PEER-ENUMERATION-20261015/README.md)。
- 测试现在保留 FullRow、WPF 状态和真实行 peer 负例，改由 UIA 客户端 `AutomationElement.FromHandle` 定位命名生产 DataGrid，并用 `SelectionPattern` 查询 A→B 单选切换、旧项取消及新项 IsSelected。没有改生产视觉、绑定、命令或模型。此处覆盖 UIA 客户端 API 的 STA 行为，不代表独立外部进程、Narrator 或真实 Playnite。
- 隔离 SDK8.0.423 / MSBuild17.11.48 / VSTest17.11.1，Release Playnite `net462` / 测试 `net472` build `0 warning / 0 error`；目标行为 `1/1`，R21 `24/24`；source validation 与 diff check 通过。源/DLL/MVID/TRX 身份见[本机复核包](evidence/CI-R21-SELECTION-UIA-CLIENT-20261016/README.md)。默认并行 `dotnet test` 首次超过两分钟无输出且无结果目录；结束本次精确 SDK 调用后以 `-m:1` 重跑成功，不把停滞试次算成测试结果。
- 新夹具待中文 commit/push；必须查对应 SDK8 和 SDK10.0.401 Actions 到终态，失败先取得完整日志。完成后按用户要求先复核 R00/R01 问题与 freshness，再审查 R12-04 既有保护路径与证据，随后推进依赖已满足的 Q/R。真实 Playnite、Windows 独立 UIA/读屏、OS 输入/IME、物理 DPI/跨屏、presented frame、ETW/宿主性能仍待验。

## 2026-10-15 R21 UIA 行选择前置条件与 #1029 双 SDK 失败

- Actions #1029/run `38706481548` 对提交 `171ac4a6` 的 SDK8.0.423 与 SDK10.0.401 jobs 都在 `R21AutomationValueBehaviorTests.ProductionMediaInboxSelectionPatternTracksTheSelectedRow` 的 UIA `GetSelection()` 返回空集合处失败。完整 log 未记录 WPF `SelectedItems` 或容器状态，不能区分实际选择丢失、行未实现或 .NET Framework UIA peer 差异。失败详情、job/artifact 身份见[证据包](evidence/CI-R09-PRESSED-STATE-RECHECK-20261015/README.md)。
- 测试夹具增加 `ScrollIntoView`、布局、SelectedItem/SelectedItems 及实际 `DataGridRow` 前置断言，仍运行原 UIA Selection pattern 项数和名称检查；生产源未改。本机 SDK8.0.423 / MSBuild17.11.48 / VSTest17.11.1 Release project/reference build `0 warning / 0 error`，R21 `24/24`，TRX 及 SHA 在证据包。当前 .NET Framework testhost `4.8.9181.0`，CI 为 `4.8.9345.0`，本机结果不能替代新 CI。
- 当前测试改动待中文 commit/push；跟踪 SDK8 与 SDK10.0.401 新 Actions 至明确终态后再定 R21 证据状态。失败须先读取对应完整 logs。真实 Playnite、OS UIA/读屏/IME、物理 DPI/跨屏、presented frame、ETW/宿主性能仍待验。
- `.tmp` 约 `1087.8 MiB`；SDK8.0.423 约 `713.8 MiB`，旧隔离输出约 `374 MiB`。D盘可用 `8.93 GiB`。批量删除旧目录被自动审查拒绝（存在未归档 TRX/日志、未知 dotnet PID `32408`）；拒绝后未尝试旁路删除，未删除任何目录。后续先补齐归档并确认进程占用再清理；工作继续进行。

## 2026-10-16 R21 SelectionItem 子树复核及临时目录清理

- 本机隔离 SDK8.0.423 / MSBuild17.11.48 / VSTest17.11.1 的 Release 项目与引用构建 `0 warning / 0 error`，R21 行选择 UIA 类 `24/24`。测试先核对生产 `SelectionUnit.FullRow`，再覆盖真实 DataGrid AutomationPeer 子树、两行 SelectionItem 正反状态与父级 Selection pattern 查询；源码 SHA `E13552DA73AF1FE8D72715CE0D9A0C6D5FC1B36DB0564C359701C58226D77D5C`、DLL SHA `840ABF60DE81DEC74744168F78397085D4CD9B2B3A3FE7AABFE1F3C12EE67CB4`、脱敏 TRX SHA `E33A440A00897E7DC830CA35232B67B0441170F03D81643980E63242EC330550`。证据见[复核包](evidence/CI-R21-SELECTION-PEER-ENUMERATION-20261015/README.md)。新提交 SDK8/SDK10.0.401 Actions 仍须到终态后才可签收。
- 用户明确授权后移除五个旧 `.tmp` 阶段输出（q24-07、r11-06、r09-current-main-retry、r00-08、r08-01 build），释放 `373.7 MiB`。SDK8工具链及当前 R21 复现目录保留；`.tmp` 现为 `744.9 MiB`，D盘可用 `9.26 GiB`。
- `python scripts/validate-source.py` 与 `git diff --check` 通过；该批只改测试夹具，无 XAML/视觉变更。
## 2026-10-16 R13-05 当前 main 远端证据复核

- R13-05 原始实现后的 DTO、维护页、Worker 映射和测试都在后续 R13 阶段继续变化，不能直接沿用 9 月身份。本次确认相关六个源/测试文件在成功验证的 `964f325d` 到当前 main `72b2cbb7` 间无变化；R13 行为用例现有 `14/14` 与真实生产 `MaintenanceView` 隔离绑定、未知态、凭据脱敏及诊断复制负例见[R13-05 当前 main 复核包](evidence/R13-05-CURRENT-MAIN-RECHECK-20261016/README.md)。
- Actions #1040/run `38880099277` 的 SDK8.0.423 job `116904483831`、SDK10.0.401 job `116904484037` 均成功；主 lane 完成全套 WPF `124/124` 类。该 run 验证 R13-05 当前代码但不含 R11-08。此工作环境只装 SDK9.0.302，未用其冒充仓库锁定 SDK8；本轮无代码或 XAML 变化。
- R13-05 继续“已满足”。未验真实 Playnite、rclone、物理 DPI/跨屏、独立 OS UIA/IME、presented frame 或宿主性能。下一项 R13-06：检查离线状态/恢复反馈相关路径是否有后续改动，再复用已有行为证据。

## 2026-10-16 R13-06 当前 main 离线恢复行为复核

- 现有 `CloudTransferStatusDto.NetworkRecoveryDisplay`、`MaintenanceView` 绑定、Worker 30 秒轮询/每轮最多 10 项/顺序 await 与通知去重已先行盘点。没有重建服务，也没有改 DTO、XAML、Worker、业务命令或通知语义；只扩展 `ProductionMaintenanceQueueRendersUploadedAndVerifiedAsDifferentStates` 的生产视图 STA 行为步骤。
- 用同一真实 `MaintenanceView` 实例和 DataGrid 选中操作验证 `RetryScheduled` 网络错误文案、2/6 次退避与最多 10 项；选到 `Transferring` 网络错误项后文本转为“网络已恢复；按批次上传中”；认证失败项文本为空，随后可恢复原网络等待项。Core 映射 `1/1`、Worker `CloudRetryPersistenceTests 12/12`、Playnite `R13CloudTransferStageBehaviorTests 14/14`、`TaskNotificationDeduperTests 6/6`。所有业务数据都是合成数据；未启动真 Worker/Playnite、网络或通知宿主。
- 本机 SDK `9.0.302` / VSTest `17.14.1`，仓库锁定 SDK `8.0.423` 未安装。Playnite `net462`/tests `net472`、Worker `net8.0-windows10.0.19041.0`、Core `net8.0` 各在新建隔离输出中定向 Release 构建；不是完整 solution build。当前 R09 类同一 Playnite 输出 `2/2`，source validation 与 diff check 通过。源码、DLL/PDB、脱敏 TRX SHA 见[R13-06 复核包](evidence/R13-06-CURRENT-MAIN-BEHAVIOR-RECHECK-20261016/README.md)。
- 对本轮唯一测试源变化运行 freshness：R00/R01 `14/14 FRESH / 0 STALE / 0 matched source paths`，package `not-provided`；这只证明登记范围没有受该路径影响。JSON 与 SHA 在 R13-06 复核包内。
- 附件外机 run `20261016T052146719Z-d9d99ac4` 仍报告旧 Chrome 2 DIP 断言；当前 main 第151行已是颜色断言，第152/154行为 Chrome 1 DIP / ValidationStroke 2 DIP。本机干净隔离 R09 `2/2` 通过；没有外机 `sourceCommit`、dirty paths、测试源/DLL/PDB 哈希，故不判定具体根因。详见[R09 外机复核](evidence/CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md)。
- 本机证据来自 SDK9，不替代 SDK8/SDK10.0.401 CI。新测试提交推送后需等其两个 Actions lane 终态再收口；未运行真实网络、真实通知监视器、Playnite/package-host、RenderHarness、物理 DPI/跨屏、独立 OS UIA/IME、presented frame、ETW 或宿主性能。下一可执行项为 R13-07 队列筛选与汇总。

## 2026-10-16 R13-07 当前 main 队列筛选与分页复核

- 复用既有 `CloudTransferStatusRequestDto`、Worker 查询、生产维护页筛选和汇总、分页 consistency token、`BatchObservableCollection` 与 `SelectionAnchorResolver`。仅把现有页追加与选中恢复代码抽成生产路径实际调用的小 helper，并共用生产筛选计数格式 helper；没有新增服务、DTO、命令或状态语义。
- Playnite 生产 `MaintenanceView` 的隔离 STA 操作状态、类型、时间、游戏和设备筛选控件，断言五个绑定值更新及生产汇总 helper 渲染“当前筛选 5/8 项”“全局 8 项”，另测空结果“当前筛选 0/8 项”。分页追加测试覆盖跨页/页内大小写不敏感重复、已有实例保留、一次 Reset；全重复页没有多余 Reset。刷新测试覆盖稳定 key 恢复、未加载后页 pending 和目标删除后的边界回退。
- 本机 SDK `9.0.302` / VSTest `17.14.1`、Playnite `net462` / 测试 `net472`：Release 定向项目与引用 build 成功，R13 `16/16`；Worker `net8.0-windows10.0.19041.0` 的 `CloudTransferStateTests 14/14`；source validation 和 diff check 通过。Worker 数据为合成隔离状态。首轮独立 STA 筛选测试导致后续 WPF 用例在关闭的 `Application.Current` Dispatcher 上找不到表格；将筛选操作放入已有 STA 维护页测试后完整 R13 类 `16/16` 通过，已归档最终成功 TRX。
- 仓库锁定 SDK8.0.423 未安装，本机 SDK9 不是完整 solution build，也不替代 SDK8/CI SDK10.0.401。R00/R01 freshness `14` 条、`0` 需重跑、`0` 源路径命中，package identity `not-provided`。详情和哈希见[R13-07复核包](evidence/R13-07-CURRENT-MAIN-BEHAVIOR-RECHECK-20261016/README.md)。
- 当前 #1044/run `38894241925` 基于 `ea1e1aea`，SDK10 job `116950776709` 已成功；SDK8 job `116950777158` 编译/测试步骤成功，离屏渲染 QA 仍运行。该 run 不含 R13-07 本轮源码。R13-07 推送会生成新 run，必须等两条 lane 到终态并读取失败日志后才能签收。外机 R09 身份材料不阻断本阶段；真实插件 IPC 刷新、Playnite/package-host、OS UIA/IME、物理 DPI/跨屏、最终帧、ETW/宿主性能仍未验。下一项 `R13-08 失败分类帮助`。

### 2026-10-16 R11-08 CI 失败定位与宿主隔离

#1043/run `38889453677` 的 SDK8 job 在 R11 历史筛选 UI 行为测试中异常退出；完整日志指向合成页面的列宽防抖计时器使用未初始化 Playnite Plugin 保存设置。只改测试宿主边界，生产默认持久化路径保留。新的 SDK9 隔离 Release 输出 R11 `6/6`、R06 `6/6`、独立 Q13 `1/1`、R09 `2/2`；已保留 TRX、源码/程序集 SHA 与 R00/R01 freshness。#1043 SDK10成功，旧 SDK8失败仍需由新 commit 的双 SDK Actions验证，不签收本阶段。

## 2026-10-16 R13-07 SDK 源码测试断言校正

- #1045/run `38897153757` 的 SDK10.0.401 job `116960249686` 与 SDK8.0.423 job `116960249845` 均终态失败；完整日志显示 `WorkspaceStateSourceTests` 的 `Assert.Contains` 仍查找云分页旧变量名。两边结果均为 462 项、443 通过、1 失败、18 跳过。`R13CloudTransferStageBehaviorTests` 已直接执行新 helper 的选择恢复行为。`WorkspaceStateSourceTests` 现改为明确的源码接线测试名称与 helper 调用检查；生产功能不变。
- 本机锁定 SDK8.0.423 不可用；首次 SDK9 定向测试因 MSB4276 提前终止，随后关闭 Workload resolver 并以全新隔离输出重跑，`WorkspaceStateSourceTests 9/1/0`（通过/跳过/失败）。这是定向项目验证，不是完整 solution/source group，也不替代 SDK8/10 CI。校正后新 Actions 尚未生成，推送后双 lane 都须终态。证据：[R13-07 SDK CI 断言校正](evidence/CI-R13-07-SDK-SOURCE-ASSERTION-RECHECK-20261016.md)。
