# GameSaveCenter 持续维护交接与开发入口

## 2026-09-26 用户备份诊断/乱码/复制问题已修复并复验

从既有项目任务找回用户原始报告。修复提交 `3f42de43` 已在当前 main 历史中：Ludusavi stdout/stderr 按 UTF-8 解码，失败诊断保留 raw output，任务详情复制对剪贴板占用短暂重试并在持续失败时反馈。以产品代码基线 `d1559fc9` 重建的 Worker 回归 `5/5`、Playnite 相关复制/反馈回归 `21/21` 通过。原始备份失败根因是当时 GitHub manifest 网络下载超时；不把外部网络恢复说成代码修复。未验真实 Playnite/系统剪贴板。详见 [当前 main 复核证据](docs/design/reviews/ui-finesse-round3-20260915/evidence/USER-REPORTED-BACKUP-UTF8-COPY-CURRENT-MAIN-RECHECK-20260926.md)。

## 2026-09-26 当前续接：ENV-001 runner 加固完成，真实宿主继续阻塞

已完成 ENV-001 的本地 runner 安全阶段：隔离目录、marker/reparse、进程观察、数据库/output 范围与安装时不终止现有进程均 fail-closed。Release solution `--no-restore` build `0/0`、XAML `24/24`、Core `125/125`、Worker `357/357`；Playnite 源码组 `465 passed/18 skipped`，105 个 WPF 类隔离通过。源码 validator、PowerShell AST、runner helper tests、diff check 通过。R 基线仍为 192，状态数 `106/83/1/1/1`。

真实宿主验收仍 `BLOCKED_ENVIRONMENT`：Playnite 在 `D:\software\Playnite\Playnite\Playnite.DesktopApp.exe` 且签名有效，但当前执行身份无权查询 `Win32_Process` 命令行；审计 runner 在任何目录副作用前拒绝。另有 2026-09-24 同机 CEF `platform_channel 0x5` 的 fresh-profile 启动拒绝记录；系统状态未改变，本轮没有再次启动。不要声称已验证实际用户 AppData/库零写入、扩展加载、UIA 或 Playnite 呈现；不要绕过进程/CEF 保护。完整证据：[ENV-001 runner evidence](docs/design/reviews/ui-finesse-round3-20260915/evidence/ENV-001-ISOLATED-RUNNER-20260926.md)。

2026-09-26 只读核对[Playnite 官方命令行文档](https://api.playnite.link/docs/manual/advanced/cmdlineArguments.html)：`--userdatadir` 只重定向数据目录，未列出并行独立实例参数；`--shutdown` 会关闭已有实例，不能作为隔离方案。此前 `--userdatadir` 实测仍命中全局单实例行为；目前无安全替代启动方案，不猜参数、不在 CEF 状态未变时重试。

## 2026-09-26 当前准入状态：产品实现项已收口，环境门禁待变化

R23-06 当前 main 包安装/回退复核完成后，已校准 R23-08 的旧执行顺序。192 项唯一 R 账本仍为 `106` 项受控满足、`83` 项待环境、`1` 外部阻塞、`1` 部分满足、`1` 不适用；没有待开始或实施中的代码项。R22-01 残余时间入口亦没有发现新的生产绑定缺口。

本机目前只枚举到一个活动桌面显示器 `\\.\DISPLAY21`，Playnite/Worker 未运行。下一步只有在条件变化后继续相应实测：正常隔离宿主可启动且 CEF 状态改变后做 R23-04；第二活动屏可用后做 Q24-03；获得合规系统跟踪权限后做 R23-05。不要在 CEF `platform_channel 0x5` 状态不变时重复启动或绕过权限，也不要用逻辑模拟替代物理跨屏/真实 presented frame。若要继续源码开发，请提供新的可复现缺陷或明确功能范围；没有证据时不自创业务/UI改动。

详细准入：[R23-08 当前 main 准入复核](docs/design/reviews/ui-finesse-round3-20260915/evidence/R23-08-CURRENT-MAIN-ADMISSION-20260926.md)。

## 2026-09-26 当前续接：R23-06 当前 main 安装/回退门禁

R23-06 已按当前 main `4f778e9bd7e305cc878e83671f372b6b954b32e8` 重建当前 Release 包并完成仓库 `.tmp` 合成 profile 安装/回退：XAML `24/24`、solution `0 errors/2` 条既有 Media `CS8602` warnings，Core `125/125`、Playnite 隔离相关类 `179 passed/40 skipped`、Worker 非进程级 `356/356`；source validator 通过，WPF static `0 errors/28 warnings/177 info`。zip/pext 同 SHA-256 `82A615DA72E55527266E9CA7A7B1A67A5926472485FEC0E57DD234F8F6B24DF1`。回退后的六份 DLL 与旧候选逐文件一致。

本批没有触碰真实用户 Extensions/profile、存档、媒体或云端，也没有启动 Playnite。合并 Playnite testhost 超 11 分钟无结果后按类隔离运行通过；`WorkerProcessRestartTests` 未纳入当前 Worker 命令范围。CEF `platform_channel 0x5`、真实 UIA/读屏、呈现帧/ETW/宿主性能仍是环境边界，不重试同状态或绕过权限。R 总基线保持 192 项唯一任务。证据：[R23-06 当前 main 安装与回退复核](docs/design/reviews/ui-finesse-round3-20260915/evidence/R23-06-CURRENT-MAIN-ROLLBACK-20260926.md)。

下一步以 [R23-08 当前准入复核](docs/design/reviews/ui-finesse-round3-20260915/evidence/R23-08-CURRENT-MAIN-ADMISSION-20260926.md) 为准：当前没有可在本机直接领取的产品代码项。只有新用户复现缺陷/明确范围，或 R23-04、Q24-03、R23-05 的环境前置发生可观察变化后再开新批次；不得从旧“下一项”历史记录自动推导任务。新 WPF 源码改动前仍须先读 Demo-first design gate 与仓库 `wpf-apple-desktop-ui` skill。

## 2026-09-26 当前续接：Media Inbox 全局目标改动已完成

用户要求的 Media Inbox 代码批次已完成：删除重复的 `InboxTargetGame` 目标状态与 ComboBox，批量/单项归类统一使用页面顶部全局 `SelectedGame`；compact Inspector 和批量栏只读展示当前全局游戏。目标 DTO/ID/名称在异步确认前捕获，避免确认期间切换选择器造成目标漂移；空目标禁用、无媒体禁用、取消、错误和批量重试语义均保留。

本批 Release solution/XAML `24/24`、0 errors、两条既有 `MediaCenterView.xaml.cs:703 CS8602`；Core `125/125`，受影响 Playnite `219`（179 passed/40 skipped），Worker 非进程级 `356/356`。全量脚本的 `WorkerProcessRestartTests` 在当前机真实进程夹具超过 12 分钟无结果后停止，需作为环境夹具单独复验，不能写成生产代码失败。`python`/`py` 不可用，Python validator 未执行；真实 Playnite/package-host 和最终呈现仍受 CEF `platform_channel 0x5` 边界影响。

R 表仍以 192 项唯一任务为总基线，本批不改计数。旧的 R20-03 指针已由后续 R23-08 当前准入复核取代；当前没有可据以修改产品行为的开放项。等待新用户复现缺陷/明确范围或环境门禁变化；任何后续 WPF 改动继续保持 Demo-first、Playnite/net462、命令/绑定、虚拟化、键盘/UIA 和安全语义。

## 2026-09-24 当前续接：用户报告的 DataGrid 问题

R20-02/R18-04 先前复核在 `cdd28fd2` 收口；之后 R06 排序崩溃修复已在当前 main 完成：真实 WPF 列头升/降序与 detached-view 负例 `7/7`，隔离 Release solution/XAML `24/24`、0 errors/两条既有 Media `CS8602`。详见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R06-SORTING-DETACHED-VIEW-CURRENT-MAIN-20260924.md`。

R06 排序崩溃和选中行内容缩进两处已定点修复。当前只剩一个用户反馈代码批次：删除 Media Inbox 冗余 `InboxTargetGame` 下拉，归类命令复用全局 `SelectedGame`，维持空目标禁用、确认目标快照和取消/错误语义并覆盖行为。行几何修复见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R06-DATAGRID-SELECTION-GEOMETRY-CURRENT-MAIN-20260924.md`，包含生产四表双主题的 cell/content 实测。

主线的 Settings/Save/Task/Media 截图布局不因四页隔离逻辑 DIP 通过而宣称真实 Playnite 已验。CEF `platform_channel 0x5` 不绕过；Demo-first、Playnite/net462、命令/恢复/取消/虚拟化和现有滚动系统仍有效。Inbox 改动完成后继续 `R20-03` 或其他依赖满足的 Q/R，不必逐阶段询问。

## 2026-09-24 R19-06 当前身份验证结果

当前 main/test identity `9b3dd2f1` Release build 的 XAML `24/24`、0 errors；R19-06 Worker 查询 `13/13`、Playnite 分页/索引 `10/10`、稳定选择 `4/4`，合计 `27/27`；相邻 R06 sorting/identity `5/5`。旧合并运行的 stale `GscBuildCommit` 失败不再复现。R19-06 保持“已满足，待环境验证”，生产并发变更时序与正常 Playnite/package-host 仍待验。细节和 TRX 见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R19-06-PAGED-SNAPSHOT-20260920.md`。

Settings 截图已在同 identity Light/Dark 四页套件再复核 `8/8`，当前本机旧 extension DLL `7a4ba2a9` 早于 `3a1dadd8` 布局修正；用户截图没有模块身份关联，候选未安装。不要把受控几何写成用户窗口已修复。下一任务为 R19-07 外部文件变化。

## 2026-09-24 最新：R23-04 外部启动阻塞

`42884321` 首次 Startup Error 的 `cef.log` 为空；强杀/safe-start 是已找到的 runner 缺陷，但不能断言它是首次报错唯一原因。`08a10da9` 全新隔离 profile 在安装扩展前直接记录 CEF `platform_channel.cc:108` 拒绝访问 `0x5`，Playnite bootstrap 不正常退出并留下 `safestart.flag`；没有扩展/seeder、manifest、summary、Dashboard 或 UIA 页面。当前权限/系统状态不重试，不删标记，不绕过权限。失败时写出 `runner-metadata.json` 与 `host-startup-blocker.json` 的逻辑已在 `910480c7` 提交。clean Release build `0 errors/2` 条既有 CS8602；source test `7/7`、用户四页行为 `8/8`、离线 synthetic log 正/负分类通过。最终身份 TRX 与本阶段 evidence 已归档。详细证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R23-04-BOOTSTRAP-SAFE-START-ROOT-CAUSE-20260924-42884321.md`。

Settings 最新截图仍与当前 source 的隔离 WPF 几何不符。`ReportedWorkspaceLayoutBehaviorTests` 对当前候选是 Light/Dark `8/8`，其中搜索/标题锚点、图标行、reset/path 控件行为均实测；但截图载入包身份与 Playnite 父容器未捕获。候选包未安装，真实 Settings 窗口问题仍开放；正常宿主复验受上述 CEF 阻塞。

## 2026-09-24 继续 R23-04 runner 收口

修复隔离安装 runner 的 `package.ps1` 参数传递：必须用具名 hashtable splat。首次执行在构建后、打包前失败，因此没有安装/启动宿主，也没有 seed manifest；修正的 Release build 与定向 evidence test `1/1` 已通过。证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R23-04-RUNNER-PACKAGE-ARGUMENT-FIX-20260924.md`。

当前 checkout 是 `main`；`codex/ui-finesse-round2` 没有已挂载 worktree，且生产代码停留在 `8a08863b` 基点（远端后续唯一提交只改 RenderHarness）。main 已含这之后的生产布局与审计收口。本批沿最新 main 修复，未将旧 main 内容覆盖回 round2 分支；后续如切回 round2，先逐项对照其 RenderHarness 独有提交和 main 之后的生产变更，不能 reset/force。

前述旧 profile 留下了 `safestart.flag`，没有复用。之后已在 `.tmp/r23-04-seeded-host-20260924-08a10da9` 全新目录做过一次 bootstrap 尝试；启动在扩展安装前被 CEF `platform_channel 0x5` 阻断，safe-start 标记仍留在隔离 profile。细节与不重试边界见顶部最新交接；同状态不再启动。

## 2026-09-24 当前接续：R23-04 合成非空库夹具

R23-04 以前缺少可复现的非空 Playnite 库。新增独立测试 seeder 和 `real-host-audit.ps1 -SeedSyntheticLibrary` opt-in，数据限定在 repo `.tmp/` 隔离 profile、固定合成前缀、未安装且没有安装目录；用本次 run ID manifest 证明是否真正导入。包归档覆盖在隔离宿主流程中关闭。

夹具准备阶段曾完成 Release build、catalog `4/4`、宿主 evidence `6/6` 与四页几何 `8/8`，当时尚未启动 Playnite。后续唯一全新 profile bootstrap 结果及 CEF 直接阻塞已在顶部最新交接和 [R23-04 当前证据](docs/design/reviews/ui-finesse-round3-20260915/evidence/R23-04-BOOTSTRAP-SAFE-START-ROOT-CAUSE-20260924-42884321.md)追加；R23-04 仍未验收。

## 2026-09-24 当前接续：Settings 截图后续与候选包
## 2026-09-24 当前接续：Settings 截图后续与候选包

用户再次报告窗口化 Settings 顶部图标、搜索框及操作控件错位。该布局修正在 `3a1dadd8` 已进入当前 main；`c866c027` Release 当前四页 Light/Dark `ReportedWorkspaceLayoutBehaviorTests` 为 `8/8`，Settings 1254×800 DIP Loaded/SizeChanged 几何 Light/Dark 各通过。原始 TRX 与度量见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/SETTINGS-HEADER-WINDOWED-1254x800-CURRENT-MAIN-20260924.md`。

审阅包 [GameSaveCenter-0.6.73-main-c866c027.pext](../artifacts/current-main/GameSaveCenter-0.6.73-main-c866c027.pext) identity `0.6.73+c866c027a2c7a2232028e9a20f1e2060bcf027cd`、SHA-256 `17B5C51CA502C0C2F119DFCBF98BF720AC43C56F899CB6BC3A6C909923498CAA`，未装入真实用户 profile。现有本机 Extension 是较早的 `7a4ba2a9`，只能作可能解释；截图进程未关联。新身份隔离 host 因 CEF `platform_channel 0x5` 启动失败，用户真实窗口仍需正常 host 验证。

本轮没有布局源码变更。继续选择不依赖 CEF/ETW 的 Q/R 小批量；宿主正常后再完成 R23-04。不要把 STA WPF 几何或候选包构建写成真实 Playnite 呈现。

## 2026-09-24 当前接续：R00/R01 freshness 校正

当前 main `57d96bbba75c413269ed723b71c2d2ec26cb500d` 的 R01-07 freshness 复算为 `14/14 fresh`、0 stale、0 source-path 匹配；三个失效负例通过。`R23-02` 记录中的 `12/14` 是当时快照，ROUND3_PROGRESS 已增加当前 14/14 与 R01-07 JSON 引用。`documentationOnlyChange=false` 仍应保留，因为这些 evidence baseline 以来确有源码变化；当前 package identity 未提供。

这是路径 freshness 校验，不替代 R00/R01 行为用例、RenderHarness 或真实 Playnite host。证据 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-CURRENT-RECHECK-20260923.md` 和 `R01-07-freshness-report-20260924-current.json`。下一项继续不依赖 CEF/ETW 的 Q/R；正常隔离 Playnite 恢复后再跑 R23-04。

## 2026-09-24 当前接续：R23-07 账本状态复算

当前 main 的 R23-07 保持“已满足”。正式第三轮任务表 `192/192` 唯一 ID，状态为 106 已满足/受控复核、83 等待明确环境门禁、1 外部阻塞、1 部分满足、1 不适用；旧 9 月 22 日统计仅作历史。第二轮 `208/208` 唯一 Q ID 中 5 已验收、203 未完成。细节见 `design/reviews/ui-finesse-round3-20260915/evidence/R23-07-CURRENT-MAIN-STATUS-RECHECK-20260924.md`。

下一宿主动作：隔离 Playnite 能正常显示主窗体时再跑 R23-04 UIA/Controlled。最近证据只得到 `Startup Error` 窗口，CEF `platform_channel 0x5` 拒绝访问；条件不变时不重试，不绕过。R23-05 等合规系统跟踪与真实呈现帧；R02-06 等 Playnite 原生菜单输入；Q24-03 等第二物理显示器。用户四页当前 main 隔离几何 `8/8` 和候选包身份见下方交接，仍不能替代真实用户窗口复验。

## 2026-09-24 当前接续：ROUND3 账本审计

Round3 规范任务表已核对为 192 个唯一 ID；前置 10 行 R13/R14 是阶段摘要，不计入任务总数。`.tmp/` 中 26 个明确已完成且无引用的旧目录已清理（约 1.81 GiB）；保留 `.tmp/r00-current-refresh` 和归属不明的目录。本轮没有代码或测试变更。

下一步从 192 项任务中继续选择依赖已满足的 Q/R 小批量，先查当前服务/DTO和最近提交，再按实现、构建/测试、适用行为验证、证据/记忆/日志和中文提交推进。R02-06 仍需正常隔离 Playnite 宿主来验证原生菜单输入；R23-04 等待正常宿主环境；R23-05 仍需合规系统跟踪权限与真实呈现帧证据。此前 CEF `platform_channel 0x5` 阻断不通过绕过解决。没有运行中的 Playnite 宿主，因此本轮没有新增宿主结论；四页隔离 STA 几何仍不能代替用户窗口复验。

## 2026-09-24 当前接续：R23-04 / R23-05 外部验收边界

最近的 main 行为复核已完成：R08-05 当前身份 `13c38754` 的页面切换回归 `31/31`；R18-04 当前代码/test identity `abd7927b` 的专测 `1/1`、相关行为 `23/23`，用户四页 Light/Dark 布局 `8/8`。Release XAML `24/24`、0 errors，保留两条既有 `MediaCenterView.xaml.cs:703 CS8602` warning；相关证据和 TRX 已提交。

用户报告的 Media Inbox 动作高、列表滚入 footer，Task 失败行框偏移，Save 窗口化动作周边空白，Settings 顶栏错位，在当前生产视图的隔离 STA WPF 行为几何中均未复现。新样本 Media Inbox `7/7/7`（早期 `14/14/14` 属旧布局预算），按钮 36 DIP，滚动条留在 DataGrid。Task 行 chrome 位置与错误状态误差均 0；Save action row 36 DIP、尾白 9.33 DIP；Settings search/title 左差 0，reset/path controls 36 DIP 同中心。测试/TRX：`docs/design/reviews/ui-finesse-round3-20260915/evidence/USER-REPORTED-LAYOUT-CURRENT-MAIN-RECHECK-20260924.md`。

本机 Playnite Extension identity `0.6.73+7a4ba2a9` 早于 screenshot 修正 commits，是实际差异的可能解释；截图无法关联到运行中 DLL，因果仍未确认。候选 `[GameSaveCenter-0.6.73.pext](../artifacts/GameSaveCenter-0.6.73.pext)` identity `0.6.73+e83d8ba9`，SHA `75A8E5AD1841C7EE6898CCB63BEEF9C216ABFBB8C8CF57693602B667DEB8B73E`，只作审阅、未安装。实际宿主复核仍被 Playnite CEF `platform_channel 0x5` 阻挡；R23-05 真正呈现帧采样仍受 ETW/WPR/xperf 权限限制，不绕过。

可继续的 Q/R 源码/隔离行为门禁已逐项满足；当前仅剩的未完成条件是 R02-06 原生菜单宿主输入、R23-04 正常 Playnite/Controlled host、R23-05 真实 presented frame/宿主性能。下一执行点：在隔离 Playnite runner/Cef host 正常可用时用候选身份复核四页；取得合规系统跟踪权限时再补真实 frame 证据。不可用时继续未依赖宿主的其他小批量任务，不把离屏几何/代理时间冒充用户屏幕通过。

## 2026-09-24 当前接续：R08-02

R08-01 已在当前 main 身份复核：Release XAML `24/24`、0 errors、保留两条既有 `MediaCenterView.xaml.cs:703 CS8602` warning；动画专测 `2/2`，相邻 shell chrome/motion/settings geometry `23/23`，均 VSTest exit `0`。生产 source identity `0c0869a2`，目前测试/文档提交 `4b7f0a34`。Translate 和侧栏中途反向都从实际值连续接管至最新目标并清理动画 clocks。证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R08-01-CURRENT-MAIN-RECHECK-20260924.md`。TRX 收尾各有一段 TextServicesHost COM 清理异常，原因未知。

下一项 R08-02 热关闭动画，先查已有关闭/退场实现。用户最新设置页截图仍与隔离 WPF 结果不符；当前源 Light/Dark 几何为 `2/2`（标题/搜索左差 0 DIP，icon-title 横向 12 DIP，重置及路径控件 36 DIP 同中心），但 package identity、正常 Playnite 父容器和物理 DPI 尚未核实。CEF `platform_channel 0x5` 曾阻断隔离 host；继续按真实限制记录，不宣称实机已经修复。已有 0.6.73+8e4f3194 包仅作身份对照，未安装。

## 2026-09-24 当前接续：R07-08

当前 `main` 为 `0c0869a2a95a9e949af5dc257bf7c7b4da2f80b6`。resize 压力序列通过当前受控验收：Release XAML `24/24`、0 errors，保留两条既有 `MediaCenterView.xaml.cs:703 CS8602` warning；最终测试项目 0 warning/error；resize 和相邻响应布局/Task/滚动/Media 几何行为 `21/21`。测试从外层 Window 尺寸事件实际进入 production shell `SizeChanged`，不直接调用布局方法。证据在 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R07-08-CURRENT-MAIN-RECHECK-20260924.md`。TRX 有 6 段 WPF 文本服务 `InvalidComObjectException` 收尾日志，根因未知，VSTest exit `0`。

下一项 R08-01 当前 main 中途反向连续复核。隔离 STA WPF/合成任务/逻辑 DIP 不代表 Playnite 正常宿主、物理窗口拖动/跨屏 DPI、真实 UIA、呈现帧或宿主性能。设置页几何代码已有受控 Light/Dark 行为证据，但用户最新截图的加载包身份和正常 Playnite 宿主布局仍未核对，CEF `platform_channel 0x5` 先前阻挡隔离 host；继续保留该问题与 Media/Task/Save 截图边界，不宣称真实用户环境已修复。

## 2026-09-24 当前接续：R07-07

当前 `main` 为 `d46fc76e8da76814a6f4870880f98f153c2581cd`。由于 98e8 后共享 WPF 生产资源和 DataGrid/Task 布局变化，已按当前 Release 构建复跑细滚动/滚动所有权/Media geometry/响应断点/Task 页面共 `19/19`，0 fail/skip；XAML `24/24`、0 errors，保留两条既有 `CS8602` warning。重点指标：20 个 -30/+120 等 routed wheel 产生 `17` 次 LayoutUpdated，末端 5 个 no-op 产生 0 布局增量，行容器 `6–9`；嵌套 -360 的单次边界传播 layout `1`。证据在 `R07-07-CURRENT-MAIN-RECHECK-20260924.md`。

下一项 R07-08 resize 压力序列当前 main 复核。触控板样本为合成 WPF routed wheel，不是物理设备/OS 输入；隔离 STA 窗口不代表正常 Playnite、物理 DPI/跨屏、UIA/读屏、presented frame 或宿主性能。继续保留既有滚动条与有限列表行为。

## 2026-09-24 当前接续：R07-06

当前生产源码身份 `2e591b9f6217c1dcf7ac5d0494a55a7e6e230c82`。状态横幅与相邻空态/Task/详情/滚动当前行为 `17/17`，Release XAML `24/24`、0 errors，保留两条既有 `MediaCenterView.xaml.cs:703 CS8602` warning；改进的 Playnite 测试项目构建 0 warning/error。用 WPF AutomationPeer `IInvokeProvider` 调用实际按钮后，计数型 fake 验证重试/安全恢复命令各执行一次；Stale 横幅没有泛化关闭按钮，Task 成功状态按绑定收起。TRX 有 9 段 TextServicesHost/TextStore COM 清理噪声，根因未知、exit 0。

证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R07-06-CURRENT-MAIN-RECHECK-20260924.md`。下一项 R07-07 当前 main 触控板小增量复核；先比较实现和上一轮提交，确认变化关联后再运行现有行为/滚动测量。不可将隔离 WPF AutomationPeer 当成 Playnite/系统 UIA，不把逻辑 DIP 或 Stopwatch 推断为真实设备输入、物理跨屏/最终帧/宿主性能。正常 Playnite host 与用户包 identity 仍未核实；不操作真实存档、媒体、云端或诊断。

## 2026-09-24 当前接续

当前 `main` 与 `origin/main` 起点为 `ec9b0a21`；R07-05 在同身份 Release 隔离构建后复跑断点和相邻回归 `17/17`、0 failed/skip，XAML `24/24`、0 errors，保留两条 `MediaCenterView.xaml.cs:703 CS8602` warning。旧身份测试程序集被 checkout 身份门禁拦下 4 个源码读取用例，重建后通过。TRX 收尾的一条 WPF TextServicesContext `InvalidComObjectException` 如实记录，原因未知、VSTest exit 0。证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R07-05-CURRENT-MAIN-RECHECK-20260924.md`。

下一项：R07-06 状态横幅预算当前 main 复核；先盘点现有行为断言，再跑本项与相邻空态/详情/滚动门禁，按当前 checkout 构建以遵守源码身份保护。测试仅限合成数据、隔离 STA WPF 与逻辑 DIP。正常 Playnite host、用户 package identity、物理 DPI/跨屏、真实设备输入、UIA/读屏、presented frame、ETW 与宿主性能仍待验；不接触真实存档、媒体、云端或诊断。

## 2026-09-23 当前接续

Q06-08 已追加生产按钮 normal/hover/pressed/focus/disabled 五态双主题受控截图与自动行为检查，定向 Release `1/1`/exit 0。证据在 `docs/design/reviews/ui-finesse-round2-20260913/evidence/q04-q12/q06-08-states-20260923/Q06-08-BUTTON-STATES-20260923.md`；Hover、Space 和程序化焦点均属受控 WPF，不是物理输入或 Playnite 真实呈现，任务最终仍未完成。此前四处截图布局问题、R18-04 与审计的历史身份和边界仍见下方历史记录及 Round3 对应 evidence。

下一可执行项：复现用户新增的设置窗口截图，核对标题图标、搜索输入框与操作按钮在窗口化高度/宽度下的相对位置；现有 96-DPI 受控几何测试与用户截图不一致。先用当前源码和隔离宿主区分尺寸、DPI 与加载版本，再决定生产修正。Demo 原目录不可用，遵守 Demo-first 并沿用恢复生产基线。


> 历史交接（已由上方本轮 main 接续更新）：工作分支 `main`，R18-04 当前源码身份 `d752424ee46c861e080a8e57b01f90f19c3a7872`；Release XAML `24/24`、0 errors，保留两条既有 `MediaCenterView.xaml.cs:706 CS8602` warning，Playnite `net462`。R18 专测 `1/1`，四个关联行为类 `6+10+3+4=23/23`、0 失败/跳过，五个 VSTest 进程 exit 0；精确用例、当前原始滚动样本与 6/2 行 `InvalidComObjectException` testhost 清理输出见 `evidence/R18-04-TABLE-CONTAINER-BUDGET-RECHECK-20260923.md`。Task 最大容器/可见 `9/7`；Media Inbox `14/14/14`，UI 上限 `2,000`；本次 20k Media 样本有一个 `0.917 ms`，保留为单独观察。下一步先完成 R00-01/02 和 R00-05 已跑完探针/行为测试的证据及 freshness 同步，然后继续依赖已满足的 Q/R 小批量。真实 Playnite/package-host、UIA/读屏、OS 输入/IME、物理 DPI/跨屏、presented frame、ETW 和宿主性能未验；Demo 原目录不可用。没有触碰真实存档、媒体、用户云端或外发诊断。

> 2026-09-23 R18-03 缩略图滚动预算定向复核：D 盘 `D:\workplace\github\GameSaveCenter` 的 `codex/ui-finesse-round2` 当前复核基线为 `93b115f4`；没有新增生产代码，复用 `e54d514e`/`18c5073f` 的 `AsyncThumbnailLoader` 3 路解码、96 项 LRU、取消和 generation 迟到结果保护。`R18ThumbnailBudgetTests 1/1`；loader/image 回归 `8/8`，合计 `9/9`；120 请求/解码开始/成功 `120/120/120`，峰值活动 `3`，缓存 `96/96`、活动逐轮归零、取消 `1`、迟到失败最终 `Ready`，托管堆代理峰值 `112,456 bytes`；隔离 Release `0 errors/2 条既有 warning`，均为 `MediaCenterView.xaml.cs:706` 的 `CS8602`；`validate-source.py`、XAML `24/24`、diff、WPF `0/28/162` 通过。未验真实 Playnite/package-host 快速滚动、显存、presented frame、DPI/UIA/IME、ETW 或宿主性能；只用合成 PNG/隔离目录/受控 STA，Demo 原目录不可用。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R18-03-THUMBNAIL-BUDGET-RECHECK-20260923.md`。下一可执行任务：`R18-04 表格容器预算`，先核对 DataGrid/ListBox 虚拟化和 2k/10k/20k 容器上限。

> 2026-09-23 R18-02 真实 Dispatcher 基准定向复核：D 盘 `D:\workplace\github\GameSaveCenter` 的 `codex/ui-finesse-round2` 当前复核基线为 `23d70d65`；没有新增生产代码，复用 `59468b37` 的 STA WPF 受控窗口两段测量和 `5b28b0c3` 的来源标注。`R18DispatcherVisibilityBenchmarkTests 1/1`；20 次 VM p95/最大 `38.259/64.967 ms`、VM→可见容器增量 `29.245/37.615 ms`；可见计数 `20/20=1`、容器 `476×19.24 DIP`；隔离 Release `0 errors/2 条既有 warning`，均为 `MediaCenterView.xaml.cs:706` 的 `CS8602`；`validate-source.py`、XAML `24/24`、diff、WPF `0/28/162` 通过。未验真实 Playnite/package-host、DWM presented frame、60fps、物理 DPI/跨屏、UIA/读屏、真实 IME、ETW 或宿主性能；仅受控 Window/合成 DTO/隔离 STA，Demo 原目录不可用。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R18-02-DISPATCHER-VISIBILITY-RECHECK-20260923.md`。下一可执行任务：`R18-03 缩略图滚动预算`，先核对活动请求、取消、缓存上限和迟到结果保护。

> 2026-09-23 R18-01 连续输入基准定向复核：D 盘 `D:\workplace\github\GameSaveCenter` 的 `codex/ui-finesse-round2` 当前复核基线为 `0173364e`；没有新增生产代码，复用 `10bc5789` 的 GamePicker 本地过滤、20ms 防抖、IME 已提交查询和内部性能诊断。`R18ContinuousInputBenchmarkTests 1/1`；当前 `GamePicker|DebouncedRefreshTests` 宽筛选 `50/50`，其中含已存在的 R18-02 受控窗口类及相邻回归，不提前签收 R18-02。2,000/10,000 项 p95/max `2.350/3.825 ms`、`7.297/9.657 ms`；过滤评估 `60,000/300,000`、托管堆代理峰值 `303,024/1,448,112 bytes`；隔离 Release `0 errors/2 条既有 warning`，均为 `MediaCenterView.xaml.cs:706` 的 `CS8602`；`validate-source.py`、XAML `24/24`、diff、WPF `0/28/162` 通过。未验真实 Playnite/package-host、Windows IME 候选 UI、最终 presented frame、物理 DPI/跨屏、UIA/读屏、ETW 或宿主性能；只用合成内存列表，Demo 原目录不可用。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R18-01-CONTINUOUS-INPUT-RECHECK-20260923.md`。下一可执行任务：`R18-02 真实 Dispatcher 基准`，区分 VM 完成与窗口可见反馈。

> 2026-09-23 R17-08 维护报告可读性定向复核：D 盘 `D:\workplace\github\GameSaveCenter` 的 `codex/ui-finesse-round2` 当前复核基线为 `fc8dffa5`；没有新增生产代码，复用 `59d4190b` 的结构化报告、统一生成时间、身份透传和脱敏器。按当前测试实际数量校正旧摘要：Worker `MaintenanceReportServiceTests 3/3`；Playnite `MaintenanceReportSourceTests 4/4`（报告 IPC/复制/导出 `2/2`，同类维护回归 `2/2`）；完整 R17 `15/15`；隔离 Release `0 errors/2 条既有 warning`，均为 `MediaCenterView.xaml.cs:706` 的 `CS8602`；`validate-source.py`、XAML `24/24`、diff、WPF `0/28/162` 通过。未验真实 Playnite/package-host、实际导出目录/剪贴板/保存对话框、最终呈现、DPI/UIA/IME、屏幕阅读器、presented frame、ETW 或宿主性能；只用合成 DTO/fake/隔离 SQLite/临时目录，Demo 原目录不可用。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R17-08-MAINTENANCE-REPORT-RECHECK-20260923.md`。下一可执行任务：`R18-01 连续输入基准`，先核对搜索/防抖/IME 和大库合成基准。

> 2026-09-23 R17-07 检查项一键定位定向复核：D 盘 `D:\workplace\github\GameSaveCenter` 的 `codex/ui-finesse-round2` 当前复核基线为 `280c839e`；没有新增生产代码，复用 `e8d581c6` 的稳定 Finding/Health/Task 来源、健康诊断 `PlayniteId + BackupId` 精确路由、缺失版本负例和维护返回栈。Worker 迁移/健康/Finding `18/18`；Playnite `FindingNavigationResolverTests 11/11`、完整 R17 `15/15`；隔离 Release `0 errors/2 条既有 warning`，均为 `MediaCenterView.xaml.cs:706` 的 `CS8602`；`validate-source.py`、XAML `24/24`、diff、WPF `0/28/162` 通过。未验真实 Playnite/package-host、Explorer/权限、最终呈现、DPI/UIA/IME、焦点/滚动、presented frame、ETW 或宿主性能；只用合成 DTO/fake/隔离 SQLite/临时目录，Demo 原目录不可用。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R17-07-FINDING-NAVIGATION-RECHECK-20260923.md`。下一可执行任务：`R17-08 维护报告可读性`，先核对报告 DTO/导出服务、分组、时间/计数一致性和脱敏负例。

> 2026-09-23 R17-06 存储分析导航定向复核：D 盘 `D:\workplace\github\GameSaveCenter` 的 `codex/ui-finesse-round2` 当前复核基线为 `1077a7ee`；没有新增生产代码，复用 `51cae6b9` 的逻辑/物理存储指标、稳定游戏/版本导航和缺失目标负例。旧证据的 Playnite R17-06 `4/4` 按当前源码更正为 `3/3`；Worker `4/4`、完整 R17 `15/15`；隔离 Release `0 errors/2 条既有 warning`，均为 `MediaCenterView.xaml.cs:706` 的 `CS8602`；`validate-source.py`、XAML `24/24`、diff、WPF `0/28/162` 通过。未验真实 Playnite/package-host、Explorer/权限、真实文件系统占用时序、最终呈现、DPI/UIA/IME、presented frame、ETW 或宿主性能；只用合成 DTO/fake/隔离 SQLite/临时目录，Demo 原目录不可用。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R17-06-STORAGE-ANALYSIS-NAVIGATION-RECHECK-20260923.md`。下一可执行任务：`R17-07 检查项一键定位`，先核对稳定 Finding/Health/Task 来源、返回目标和入口。

> 2026-09-23 R17-05 隔离账本入口定向复核：D 盘 `D:\workplace\github\GameSaveCenter` 的 `codex/ui-finesse-round2` 当前复核身份为 `d01ab7ea`；没有新增生产代码，复用 `3002a8dc` 的分页账本、原/隔离路径、完整 Tooltip、EntryId 定向恢复和冲突停止。Worker `RetentionQuarantineRecoveryTests 5/5`；Playnite R17 合并 `12/12`（隔离账本源码/绑定 `2/2`）；隔离 Release `0 errors/2 条既有 warning`，均为 `MediaCenterView.xaml.cs:706` 的 `CS8602`；`validate-source.py`、XAML `24/24`、diff、WPF `0/28/162` 通过。未验真实 Playnite/package-host、Explorer/权限、重启恢复、最终呈现、DPI/UIA/IME、presented frame、ETW 或宿主性能；只用合成 DTO/fake IPC/隔离 SQLite/临时目录，Demo 原目录不可用。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R17-05-QUARANTINE-LEDGER-RECHECK-20260923.md`。下一可执行任务：`R17-06 存储分析导航`，先核对已有存储统计、来源记录和维护页跳转能力，再决定是否需要代码。

> 2026-09-23 R17-04 保留预览对比定向复核：D 盘 `D:\workplace\github\GameSaveCenter` 的 `codex/ui-finesse-round2` 当前复核身份为 `503c2119`；没有新增生产代码，复用 `3c73b498` 的保留预览、保护计数、隔离账本、Apply 指纹/时效/操作锁和删除失败恢复。Worker `RetentionSimulationServiceTests 12/12`；Playnite R17 合并 `11/11`（含保留源码/绑定 `1/1`），布局 `20 passed/11 skipped`；隔离 Release `0 errors/2 条既有 warning`，均为 `MediaCenterView.xaml.cs:706` 的 `CS8602`；`validate-source.py`、XAML `24/24`、diff、WPF `0/28/162` 通过。未验真实 Playnite/package-host、Explorer/权限、真实锁/文件故障/重启恢复、最终呈现、DPI/UIA/IME、presented frame、ETW 或宿主性能；只用合成策略/fake/隔离 SQLite trigger/目录，Demo 原目录不可用。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R17-04-RETENTION-PREVIEW-RECHECK-20260923.md`。下一可执行任务：`R17-05 隔离账本入口`，先核对分页隔离列表、原/隔离路径、状态和受控恢复入口，不默认删除残留。

> 2026-09-23 R17-03 检查进度预算定向复核：D 盘 `D:\workplace\github\GameSaveCenter` 的 `codex/ui-finesse-round2` 当前复核身份为 `e0b5cd15`；没有新增生产代码，复用 `87473bc3` 的持久化游标、时间预算、运行/操作锁门禁、延后候选和取消/失败终态。Worker `HealthInspectionServiceTests 12/12`；Playnite R17 合并回归 `10/10`（R17-03 `3/3`）；隔离 Release `0 errors/2 条既有 warning`，均为 `MediaCenterView.xaml.cs:706` 的 `CS8602`；`validate-source.py`、XAML `24/24`、diff、WPF `0/28/162` 通过。未验真实 Playnite/package-host、真实游戏/锁/超时竞态、最终呈现、DPI/UIA/IME、presented frame、ETW 或宿主性能；只用合成会话/fake 锁/隔离 SQLite/归档，Demo 原目录不可用。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R17-03-INSPECTION-PROGRESS-BUDGET-RECHECK-20260923.md`。下一可执行任务：`R17-04 保留预览对比`，先核对保留候选、保护项、隔离账本和执行前预览过期边界。

> 2026-09-23 R17-02 诊断包预览定向复核：D 盘 `D:\workplace\github\GameSaveCenter` 的 `codex/ui-finesse-round2` 当前复核身份为 `837528bc`；没有新增生产代码，复用 `2b6e9051` 的只读预览、类别/脱敏/排除清单、确认前置和生成后结果展示。Playnite R17 `7/7`；Worker 诊断预览与既有真实 ZIP `2/2`，R17-01 SQLite 回归 `2/2`，相关合计 `11/11`；隔离 Release `0 errors/2 条既有 warning`，均为 `MediaCenterView.xaml.cs:706` 的 `CS8602`；`validate-source.py`、XAML `24/24`、diff、WPF `0/28/162` 通过。未验真实 Playnite/package-host 确认框/Explorer/权限/日志并发、最终呈现、DPI/UIA/IME、presented frame、ETW 或宿主性能；只用合成请求/fake/隔离 SQLite/临时目录，Demo 原目录不可用。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R17-02-DIAGNOSTICS-PACKAGE-PREVIEW-RECHECK-20260923.md`。下一可执行任务：`R17-03 检查进度预算`，先核对巡检范围、暂停/延后原因、最近成功时间、下轮计划和取消/结束状态。

> 2026-09-23 R17-01 健康结果分层定向复核：D 盘 `D:\workplace\github\GameSaveCenter` 的 `codex/ui-finesse-round2` 当前复核身份为 `3f9d8e2b`；没有新增生产代码，复用 `eb033251` 的 `resolved=0` 开放队列、健康巡检 resolve、证据时间、跨来源去重和三档影响分组。Playnite `R17FindingTriageBehaviorTests + R17FindingTriageSourceTests 5/5`、Worker 隔离 SQLite `R17FindingPersistenceTests 2/2`，合计 `7/7`；隔离 Release `0 errors/2 条既有 warning`，均为 `MediaCenterView.xaml.cs:706` 的 `CS8602`；`validate-source.py`、XAML `24/24`、diff、WPF `0/28/162` 通过。未验真实 Playnite/package-host、多来源生产标题规范、最终呈现、DPI/UIA/IME、presented frame、ETW 或宿主性能；只用合成 DTO/fake/隔离 SQLite，Demo 原目录不可用。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R17-01-HEALTH-RESULT-LAYERS-RECHECK-20260923.md`。下一可执行任务：`R17-02 诊断包预览`，先核对类别预览、脱敏范围和生成后大小/位置。

> 2026-09-23 R16-08 保存冲突处理定向复核：D 盘 `D:\workplace\github\GameSaveCenter` 的 `codex/ui-finesse-round2` 当前复核身份为 `afe80186`；没有新增生产代码，复用 `ee6b37c9` 的三方字段合并、冲突事件/异常、取消基线和保存失败分流。R16-08 冲突行为 `3/3`、源码 `1/1`、脱离视图 WPF `1/1`、保存反馈 `2/2`、Portable `10/10`，合计 `17/17`；隔离 Release `0 errors/2 条既有 warning`，均为 `MediaCenterView.xaml.cs:706` 的 `CS8602`；`validate-source.py`、XAML `24/24`、diff、WPF `0/28/162` 通过。未验真实 Playnite 双设置窗口/共享 settings 竞态、宿主错误呈现、DPI/UIA/IME、presented frame、ETW 或宿主性能；只用合成 detached/fake/隔离目录，Demo 原目录不可用。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R16-08-SETTINGS-CONFLICT-RECHECK-20260923.md`。下一可执行任务：`R17-01 健康结果分层`，先核对健康结果、已解决项和跨来源去重时间证据。

> 2026-09-23 R16-07 配置导入预览定向复核：D 盘 `D:\workplace\github\GameSaveCenter` 的 `codex/ui-finesse-round2` 当前复核身份为 `01e83d8a`；没有新增生产代码，复用 `451195ad` 的 portable settings v1、detached 预览、确认后应用、未知字段/凭据/设备身份保护和异常回滚。R16-07 行为 `4/4`、源码 `1/1`、既有 portable 回归 `10/10`，合计 `15/15`；隔离 Release `0 errors/2 条既有 warning`，均为 `MediaCenterView.xaml.cs:706` 的 `CS8602`；`validate-source.py`、XAML `24/24`、diff、WPF `0/28/162` 通过。未验真实 Playnite/package-host 文件选择器/MessageBox/保存取消、DPI/UIA/IME、presented frame、ETW 或宿主性能；Demo 原目录不可用。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R16-07-SETTINGS-IMPORT-PREVIEW-RECHECK-20260923.md`。下一可执行任务：`R16-08 保存冲突处理`，先核对编辑基线、后台更新通知和字段级合并/拒绝边界。

> 2026-09-23 R16-06 生效条件说明定向复核：D 盘 `D:\workplace\github\GameSaveCenter` 的 `codex/ui-finesse-round2` 当前复核身份为 `9cef273b`；没有新增生产代码，复用 `83e7c745` 的四类生效提示与 `EndEdit → NotifyVisualSettingsChanged → settings.update → WorkerOptions.Apply/SyncPlan` 消费链。`R16SettingsEffectSourceTests 2/2` 加 R16-05 路径回归 `6/6`，隔离 Release `0 errors/2 条既有 warning`，均为 `MediaCenterView.xaml.cs:706` 的 `CS8602`；`validate-source.py`、XAML `24/24`、diff、WPF `0/28/162` 通过。源契约测试只覆盖文案/链路/负例，不冒充真实 Playnite 保存后时序；未验真实宿主、Worker 重启/轮询、DPI/UIA/IME、presented frame、ETW 或宿主性能，Demo 原目录不可用。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R16-06-SETTINGS-EFFECT-CONDITIONS-RECHECK-20260923.md`。下一可执行任务：`R16-07 配置导入预览`，先核对导入报告、版本/未知字段、凭据和失败回退边界。

> 2026-09-23 R16-05 路径编辑一致定向复核：D 盘 `D:\workplace\github\GameSaveCenter` 的 `codex/ui-finesse-round2` 当前复核身份为 `3121d337`；没有新增生产代码，复用 `955dc52e` 的六字段路径编辑卡片、只读探测、当前字段 Binding、严格打开门禁和剪贴板重试。相关行为/源码/路径回归 `6/6`，隔离 Release `0 errors/2 条既有 warning`，均为 `MediaCenterView.xaml.cs:706` 的 `CS8602`；`validate-source.py`、XAML `24/24`、diff、WPF `0/28/162` 通过。未验真实 Playnite/package-host、WinForms 文件夹对话框/Explorer、UIA/焦点、DPI/跨屏、presented frame、ETW 或宿主性能；Demo 原目录不可用。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R16-05-PATH-EDITOR-RECHECK-20260923.md`。下一可执行任务：`R16-06 生效条件说明`，先核对设置字段实际消费点和保存/应用/重启边界。

> 2026-09-23 R16-04 恢复默认粒度定向复核：D 盘 `D:\workplace\github\GameSaveCenter` 的 `codex/ui-finesse-round2` 当前复核身份为 `666c60bb`；没有新增生产代码，复用 `2b194461` 的安全默认字段目录、单字段/分类/全部入口和 Playnite 草稿取消保护。行为 `2/2`、源码契约 `1/1`，当前提交隔离 Release solution `0 errors/2 条既有 warning`，均为 `MediaCenterView.xaml.cs:706` 的 `CS8602`；`validate-source.py`、XAML `24/24`、diff check 通过。直接复用旧输出曾触发 `1f968819`/`666c60bb` 身份保护，当前 checkout 重建后通过。未验真实 Playnite/package-host 点击与最终呈现、DPI/UIA/IME、presented frame、ETW 或宿主性能；Demo 原目录不可用。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R16-04-RESET-GRANULARITY-RECHECK-20260923.md`。下一可执行任务：`R16-05 路径编辑一致`，先核对路径浏览/校验/打开/复制和权限/网络/不存在负例。

> 2026-09-23 R16-03 模板应用范围定向复核：D 盘 `D:\workplace\github\GameSaveCenter` 的 `codex/ui-finesse-round2` 当前复核身份为 `1f968819`；没有新增生产代码，复用 `52fbf5de` 的批量模板预览、稳定 ID 目标集合、逐项 Worker 执行和失败重试。Core 批量预览 `3/3`、Worker 策略持久化 `2/2`、Playnite 源契约 `1/1`，Playnite `net462` 定向构建无错误，保留 `MediaCenterView.xaml.cs:706` 两条既有 `CS8602` warning；`validate-source.py`、XAML `24/24`、diff check 通过。显式勾选/排除、100 个上限、空选择、逐项结果、取消和稳定 ID 均有证据，R16-03 校正为“已满足，待环境验证”。真实 Playnite 批量点击/筛选/UIA/焦点、DPI/跨屏、presented frame、ETW 和宿主性能仍未验；Demo 原目录不可用。下一可执行项为 R16-04 恢复默认粒度。

证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R16-03-POLICY-TEMPLATE-BATCH-RECHECK-20260923.md`。

> 2026-09-23 R16-02 策略差异预览定向复核：D 盘 `D:\workplace\github\GameSaveCenter` 的 `codex/ui-finesse-round2` 当前复核身份为 `e63c62c7`；没有新增生产代码，复用 `b327d5ef` 的策略 DTO、模板目录、13 字段差异比较和保存/取消链。Core `BackupPolicyTemplateCatalogTests 5/5`、Playnite 策略源契约 `2/2`，Playnite `net462` 定向构建无错误，保留 `MediaCenterView.xaml.cs:706` 两条既有 `CS8602` warning；`validate-source.py`、XAML `24/24`、diff check 通过。已保存/显式草稿/模板覆盖差异、复制回退、取消不发 Worker 请求和未保存草稿禁用模板均有行为/负例证据，R16-02 校正为“已满足，待环境验证”。真实 Playnite 差异卡片、浅深主题、UIA/读屏/IME、DPI/跨屏、presented frame、ETW 和宿主性能仍未验；Demo 原目录不可用。下一可执行项为 R16-03 模板应用范围。

证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R16-02-POLICY-DIFF-RECHECK-20260923.md`。

> 2026-09-23 R16-01 设置搜索定位定向复核：D 盘 `D:\workplace\github\GameSaveCenter` 的 `codex/ui-finesse-round2` 当前复核身份为 `ae9aedbc`；没有新增生产代码，复用 `a4e35578` 的设置字段索引、分类切换和原有控件绑定。搜索 `1/1`、源契约 `1/1`、验证导航 `1/1`、草稿生命周期 `1/1` 均在独立 testhost 通过；Playnite `net462` 定向构建无错误，保留 `MediaCenterView.xaml.cs:706` 两条既有 `CS8602` warning；`validate-source.py`、XAML `24/24`、diff check 通过。搜索命中/清空恢复、可编辑性、配置不写入和 pending edit 不变均有证据；合并 WPF testhost 的 `Application` 多实例冲突作为环境边界记录。真实 Playnite 设置页、浅深主题、UIA/读屏/IME、DPI/跨屏、presented frame、ETW 和宿主性能仍未验，Demo 原目录不可用。下一可执行项为 R16-02 策略差异预览。

证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R16-01-SETTINGS-SEARCH-RECHECK-20260923.md`。

> 2026-09-23 R15-08 清理历史范围定向复核：D 盘 `D:\workplace\github\GameSaveCenter` 的 `codex/ui-finesse-round2` 当前复核身份为 `569fa1d8`；没有新增生产代码，复用既有 Retention Simulation、预览句柄、保护门禁和隔离账本。Worker 清理/恢复账本 `17/17`、Playnite 维护页/账本契约 `6/6`，`net462` 定向构建无错误，保留 `MediaCenterView.xaml.cs:706` 两条既有 `CS8602` warning；`validate-source.py`、XAML `24/24`、diff check 通过。预览过期/状态变化、二次确认、运行中/共享锁忙碌跳过、锁定/健康/PreRestore 保护和账本恢复均有行为证据，R15-08 保持“已满足”。真实 Playnite 清理呈现、UIA/读屏/IME、DPI/跨屏、presented frame、ETW 和宿主性能仍未验；Demo 原目录不可用，Worker 全量既有 `MediaSyncService.cs:570` 失败不变。下一可执行项为 R16-01 设置搜索定位。

证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R15-08-HISTORY-CLEANUP-SCOPE-RECHECK-20260923.md`。

> 2026-09-23 R15-07 失败结果复制定向复核：D 盘 `D:\workplace\github\GameSaveCenter` 的 `codex/ui-finesse-round2` 当前复核身份为 `d9dc4317`；没有新增生产代码，复用 `37dd4a03` 的任务复制、统一脱敏、有限高详情和剪贴板重试。Playnite 失败复制/剪贴板/恢复/任务详情套件 `21/21`，`net462` 定向构建无错误，保留 `MediaCenterView.xaml.cs:706` 两条既有 `CS8602` warning。短摘要、密码负例、完整脱敏 payload、只读可选择详情、第三次成功和四次失败均有行为证据，账本校正为“已满足，待环境验证”。真实系统剪贴板/Playnite UIA/读屏/IME、DPI/跨屏、presented frame、ETW 和宿主性能仍未验；Demo 原目录不可用，Worker 全量既有 `MediaSyncService.cs:570` 失败不变。下一可执行项为 R15-08 清理历史范围。

证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R15-07-TASK-FAILURE-COPY-RECHECK-20260923.md`。

> 2026-09-23 R15-06 耗时与吞吐定向复核：D 盘 `D:\workplace\github\GameSaveCenter` 的 `codex/ui-finesse-round2` 当前复核身份为 `d9dc4317`；没有新增生产代码，复用 `6f65638e` 的单调采样、可靠总量门控和 Task Center 字段。Worker 采样/查询/广播/失败链 `22/22`，Playnite 进度/快照/耗时套件 `5/5`，Playnite `net462` 定向构建无错误，保留 `MediaCenterView.xaml.cs:706` 两条既有 `CS8602` warning；`validate-source.py`、XAML `24/24`、diff check 通过。未知总量、等待确认、10 秒停顿、普通阶段清空和两个推进样本门槛均有行为证据，账本校正为“已满足，待环境验证”。Worker 全量既有 `MediaSyncService.cs:570` 失败未改写，真实 Playnite Task Center、UIA/读屏/IME、DPI/跨屏、presented frame、ETW 和宿主性能仍未验；Demo 原目录不可用。下一可执行项为 R15-07 失败结果复制。

证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R15-06-TASK-THROUGHPUT-RECHECK-20260923.md`。

> 2026-09-23 R15-05 任务来源定位定向复核：D 盘 `D:\workplace\github\GameSaveCenter` 的 `codex/ui-finesse-round2` 当前复核身份为 `ba4d624b`；没有新增生产代码，复用 `0d1ff346` 的稳定来源引用与精确导航。Playnite `R15TaskSourceNavigationTests 3/3`、Worker 查询/广播/失败路径 `21/21` 通过，Playnite `net462` 定向构建无错误，保留 `MediaCenterView.xaml.cs:706` 两条既有 `CS8602` warning。已删除游戏不跳同名游戏，已删除版本不选邻近版本，来源 clone 保留稳定诊断身份；账本校正为“已满足，待环境验证”。真实 Playnite 来源卡片、删除/重命名后的 UIA/读屏/IME、DPI/跨屏、presented frame、ETW 和宿主性能仍未验，Demo 原目录不可用；未碰真实存档、媒体、云端或诊断。下一可执行项为 R15-06 耗时与吞吐。

证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R15-05-TASK-SOURCE-LOCATION-RECHECK-20260923.md`。

> 2026-09-23 R23-05 壳层断点几何复测：D 盘 `D:\workplace\github\GameSaveCenter` 的 `codex/ui-finesse-round2` 提交 `b5e7fca0` 只扩展 RenderHarness 的实际 WPF 断点探针，没有新增生产 UI/业务代码。提交后 clean `shellqa` exit `0`、`WorkingTreeClean=True`：1200/1279 为 compact，1280/1366 为 expanded，标题/动作区分行或不重叠且动作区在 HeaderSurface 内；Media 1040/1100/1366 `gridTopGap=142 DIP`，页尾可达；`ResponsiveLayoutCoordinatorTests + MediaInboxGeometryTests 8/8`，XAML `24/24`，solution `0 errors/2` 条既有 warning。R23-05 仍“部分满足，待宿主性能验收”，不把离屏/Rendering/Stopwatch 写成 presented frame；R23-04 CEF `0x5` 与 ETW 权限边界仍保留。下一项为依赖已满足的 Q/R 行为小批量，不重复当前几何探针。

证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R23-05-HEADER-BREAKPOINT-RECHECK-20260923.md`。

> 2026-09-23 R00/R01 动效探针时序复核：当前 D 盘 `D:\workplace\github\GameSaveCenter` 的 `codex/ui-finesse-round2` 身份为 `0e468873`；本批只修正 RenderHarness 采样，不改生产动效。深色资源先完成响应式布局，活动/完成态均以有界 Dispatcher 等待和实际观察值校正首帧/完成回调晚到误判。RenderHarness Release Playnite `net462` `0 error`，四个 Light/Dark motion probes 全部 exit `0`，`UiFinesseFoundationTests 9/9`，仅保留 `MediaCenterView.xaml.cs:706` 两条既有 `CS8602` warning。R00-03/R01-04 按受控证据改为“已满足”；真实 Playnite 输入、UIA/读屏、Windows 偏好通知、DPI/跨屏、presented frame、ETW、宿主性能和 Demo 原目录仍未验。下一项为 R23-04 正常可枚举宿主会话，若 CEF/窗口暴露继续阻塞则转依赖已满足的独立 Q/R。

证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-MOTION-PROBE-RECHECK-20260923.md`。

> 2026-09-23 R22-01 残余时间入口复核：D 盘 `D:\workplace\github\GameSaveCenter` 是唯一有效工作区，当前开发分支 `codex/ui-finesse-round2`，复核身份 `df6bee9d`；本批没有生产代码变更。生产 `Views/*.xaml` 未发现旧本地直显时间绑定，兼容/报告/复制/日志/内部入口保留；`R22TimeDisplayBehaviorTests 30/30`、`R10RecentAccessBehaviorTests 2/2`，隔离 Release XAML `24/24`、Playnite `net462`、`0 error`，仅保留 `MediaCenterView.xaml.cs:706` 两条既有 `CS8602` warning。真实 Playnite/package-host、UIA/读屏、OS 输入/IME、DPI/跨屏、presented frame、ETW、宿主性能和 Demo 原目录仍未验。下一可执行项为 R23-04 正常可枚举宿主会话；若 CEF/窗口暴露继续阻塞，转依赖已满足的独立 Q/R 小批量。

证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R22-01-RESIDUAL-ENTRY-RECHECK-20260923.md`。

> 2026-09-24 R21-05 当前身份复测：D 盘 `D:\workplace\github\GameSaveCenter` 是唯一有效工作区，当前开发分支 `codex/ui-finesse-round2`；C 盘旧仓库与用户删除的 `src.zip` 均不存在。R21-05 实现已在 `380234e2`，本次只以 `1f69b803` 复测，不重复实现。实际 `R21DisabledHiddenBehaviorTests 2/2`，隔离 Release XAML `24/24`、Playnite `net462`、`0 error`，仅保留 `MediaCenterView.xaml.cs:706` 两条既有 `CS8602` warning。下一可执行任务是 R23-04 正常可枚举 Playnite 主窗体/UIA 会话；若 CEF/窗口暴露仍阻塞，转依赖已满足的独立 Q/R 小批量。不绕过 ETW/系统跟踪拒绝，不把离屏/代理/AutomationPeer 结果写成真实宿主呈现或性能通过。

证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R21-05-TARGETED-RECHECK-20260924.md`。

> 2026-09-24 R15-03 任务详情时间线定向复核：实现 `fe0c05a9` 已提供 `OccurredUtc`、UTC/本地双显示、缺失事件/时间未知和有界详情时间线；本批 `b6170ec9` 只新增乱序 UTC、同刻序号稳定排序、跨任务过滤行为测试。Worker `12/12`、Playnite 时间线/批处理/阶段/取消/进度套件 `15/15`，Release XAML `24/24`、solution `0 error/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning，Playnite `net462`。既有 Task 截图没有打开时间线卡，不作时间线呈现证据；真实 Worker 重启历史、Playnite/UIA/读屏/IME、DPI/跨屏、presented frame、ETW 和宿主性能仍未验；Demo 原目录不可用。下一项 R15-04 重复通知归并。

证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R15-03-TARGETED-RECHECK-20260924.md`。

> 2026-09-24 R15-04 重复通知归并定向复核：实现 `a67d371e` 已复用任务终态去重、会话累加、通知等级策略和 Task Center 历史；本批 `6510ccf6` 只新增未知失败详情变化行为测试。Playnite 核心通知/会话 `10/10`，相邻通知反馈/时间线/UI 批处理/R13 夹具 `31/31`，Core 通知策略/摘要 `7/7`；Release XAML `24/24`、solution `0 error/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning，Playnite `net462`。源码契约不冒充真实 Toast；真实 Playnite 通知时序、UIA/读屏/IME、DPI/跨屏、presented frame、ETW 和宿主性能未验。下一项 R15-05 任务来源定位。

证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R15-04-TARGETED-RECHECK-20260924.md`。

> 2026-09-24 R15-02 取消过程展示定向复核：本批没有生产代码变更；当前 `c879cf43` 的 Worker 取消套件 `16/16`、Playnite `R15TaskCancellationTests + R15TaskStageTests + R06TaskProgressBehaviorTests 8/8` 通过，隔离 Release XAML `24/24`、solution `0 error/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning，Playnite `net462` 编译通过。覆盖 `Requested → Finalizing → Cancelled`、重复取消幂等、完成后晚到取消、成功/取消终态和未知进度；`Task-1600x900` 仅为合成“已取消”终态证据。真实长任务取消、Playnite、UIA/读屏/IME、DPI/跨屏、presented frame、ETW 和宿主性能未验；Demo 原目录不可用。下一项 R15-03 任务详情时间线。

证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R15-02-TARGETED-RECHECK-20260924.md`。

> 2026-09-24 R15-01 任务阶段可读定向复核：本批没有生产代码变更；当前 `9a63e7e2` 的 Worker `TaskCoordinatorFailureTests` + `TaskQueryPersistenceTests` 合计 `16/16`、Playnite `R15TaskStageTests 2/2` 通过，隔离 Release XAML `24/24`、solution `0 error/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning，Playnite `net462` 编译通过。阶段消息与终态错误保持分离，未知总量不伪造百分比；`Task-1600x900` 仅为合成 offscreen 阶段列/未知阶段负例证据。真实宿主阶段事件、任务历史、DPI/UIA/IME、presented frame、ETW 和宿主性能未验；Demo 原目录不可用。下一项 R15-02 取消过程展示。

证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R15-01-TARGETED-RECHECK-20260924.md`。

> 2026-09-24 R14-08 来源规则试运行定向复核：本批没有生产代码变更；当前 `6d426f6e` 的 Worker `MediaSyncServiceTests 20/20`、Playnite `R14SourceRulePreviewTests 1/1` 通过，隔离 Release XAML `24/24`、solution `0 error/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning，Playnite `net462` 编译通过。合成隔离来源样本扫描 `4` 项、命中 `2`、排除 `2`，显示原因/大小/路径，试运行不保存规则、不入库、不移动；既有 `Media-1600x900-tab3` 仅为 offscreen 证据。真实权限拒绝、超大目录、Playnite、DPI/UIA/IME、presented frame、ETW 和宿主性能未验；Demo 原目录不可用。下一项 R15-01 任务阶段可读。

证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R14-08-TARGETED-RECHECK-20260924.md`。

> 2026-09-24 R14-07 媒体详情浏览定向复核：本批没有生产代码变更；当前 `89f07445` 重新生成身份一致的隔离产物后，选定 Playnite 套件 `46/46` 通过，覆盖当前已加载窗口导航、MediaId 列表锚点、缩略图尺寸/取消/缺失、视频回退和 generation 保护。隔离 Release XAML `24/24`、solution `0 error/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning，Playnite `net462` 编译通过；既有 `Media-1040x700-tab1` 只显示详情入口与媒体列表，未打开详情面板。跨页导航、真实视频编解码、真实 Playnite、UIA/读屏/IME、DPI/跨屏、presented frame、ETW 和宿主性能未验；Demo 原目录不可用。下一项 R14-08 来源规则试运行。

证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R14-07-TARGETED-RECHECK-20260924.md`。

> 2026-09-24 R14-06 批量目标防误选定向复核：本批没有生产代码变更；在 `89f07445` 上复用 `cfbb1279` 的 IconPath、IdentityDisplay、现有 Games/SelectedItem/TargetPlayniteId 和过滤后隐藏选择保护，新增同名对象按 Playnite ID 区分的行为夹具。`GamePickerViewModelTests 22/22`、`R14ClassificationSelectionTests 4/4`、`GamePickerKeyboardBehaviorTests 6/6`，隔离 Release XAML `24/24`、solution `0 error/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning、Playnite `net462` 编译通过。`Shell-Media-1040x700` 仅为合成 offscreen 图标/平台/稳定 ID 视觉证据，不证明同名下拉运行时、真实 Playnite、UIA/读屏/IME、DPI/跨屏、presented frame、ETW 或宿主性能。Demo 原目录不可用；下一项 R14-07 媒体详情浏览，继续复用现有 `SelectedMedia`/分页/稳定 `MediaId`。

证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/R14-06-TARGETED-RECHECK-20260924.md`。

> 2026-09-24 R14-05 重复媒体识别定向复核：无生产代码变更，复用 `136285d5` 的只读重复查询/DTO/IPC/页面；Worker `MediaSyncServiceTests 20/20`、Playnite `R14ClassificationSelectionTests 4/4`，当前 Release XAML `24/24`、solution `0 error/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning。合成行为覆盖 SHA-256 确定组、元数据疑似组和删除/移动门禁；既有 RenderHarness `1040×700` 重复页为 `2` 组/组内 `4` 项、`330 DIP`，全报告其他页面仍有基线失败。真实 Playnite/package-host、UIA/读屏/IME、DPI/跨屏、presented frame、ETW、宿主性能和超大真实媒体库未验。下一项 R14-06，保留现有游戏选框与稳定 ID 系统。

> 2026-09-23 R14-04 撤销边界定向复核：无生产代码变更，Worker `MediaSyncServiceTests 20/20` 通过；正常撤销恢复归档副本并保留原始文件，应用后人工收藏/备注变化进入 `UndoneWithConflicts`，后来决定和应用后归档副本保留，不重建 Inbox 副本。合并主分支 Release XAML `24/24`、solution `0 error/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning，Playnite `net462` 编译通过。仅合成/fake/隔离 SQLite/目录；真实 Playnite/package-host、RenderHarness 新呈现、UIA/读屏/IME、DPI/跨屏、presented frame、ETW 和宿主性能未验。下一项 R14-05 重复媒体只读分组。

> 2026-09-23 R14-03 部分成功处理定向复核：本批无生产代码变更，复用 `aef251b1` 的逐项 best-effort 和失败重试命令；Worker `MediaSyncServiceTests 20/20`，Playnite `R14ClassificationSelectionTests 4/4`、`MediaWindowAnchorContractTests 10/10`，隔离 Release XAML `24/24`、solution `0 error/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning。失败列表仍为 `MaxHeight=128`、Recycling，重试只提交上次失败稳定 ID，成功项不重复执行。仅合成/fake/隔离 testhost；真实 Playnite/package-host、UIA/读屏/IME、DPI/跨屏、presented frame、ETW、宿主性能和 RenderHarness 新失败样本未验。下一项 R14-04 撤销冲突/恢复保护。

> 2026-09-23 R14-01/R14-02 受控证据账本校正：本阶段只更新 `ROUND3_PROGRESS.md` 两个历史详细行，没有生产代码变更；以 `8a9a052e` 的 R13/R14 定向复核为当前事实，R14-01 `R14ClassificationEvidenceTests 1/1`、R14-02 `R14ClassificationSelectionTests 4/4`、Worker `MediaSyncServiceTests 20/20`，当前隔离 Release XAML `24/24`、solution `0 errors/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning。受控证据仅来自合成/fake/隔离 testhost/offscreen logical DIP，不扩大为真实 Playnite、UIA/读屏/IME、DPI/跨屏、presented frame、ETW 或宿主性能通过；下一项为 R14-03，真实媒体/云端仍未触碰。

> 2026-09-23 R23-04 runner UIA 候选探测：代码提交 `f551359c58142e43bfc9dbe4dd3dff3987111244` 复用已有 Win32 顶层窗口枚举，逐窗建立 UIA 根节点，保留 MainWindowHandle 回退，并把候选窗口/根节点类型/匹配动作写入 `host-window-exposure.json`；没有修改生产 UI、命令/Binding、选框、滚动条或安全语义。真实隔离 Playnite PID `3556` 的可见窗口仍是 `Startup Error`，句柄 `0x40B24`，顶层窗口 `5`；60 秒/30 次逐窗探测建立 5 个 UIA 根节点但没有 GameSaveCenter 侧栏，`summary.json` 未生成，`cef.log` 仍为 `platform_channel` 拒绝访问 `0x5`，按 `[PARTIAL]` 收口。证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R23-04-UIA-WINDOW-EXPOSURE-RECHECK-20260923.md` 和 `artifacts/ui-host-audit-r23-04-uia-candidates-20260923`。隔离 PID 已停止；下一可执行项是正常可枚举 Playnite UIA 会话，若继续阻塞则推进依赖已满足的 Q/R 小批量。真实 UIA/键盘/读屏、presented frame、DPI/跨屏、ETW/宿主性能仍未验。

> 2026-09-23 R00/R01 当前证据收口：D 盘 `D:\workplace\github\GameSaveCenter` 是唯一开发工作区，当前分支 `codex/ui-finesse-round2`，代码提交 `b5c7a6d4`。Media Inbox footer 增加后的旧表格预算已修正为窄宽/紧凑高度页级滚动与 `360 DIP` footer 预算；XAML `24/24`、solution `0 errors/2` 条既有 `MediaCenterView.xaml.cs:706` nullable warning。Media 几何/锚点、审计源、身份、数字、负例、大库和动效行为定向证据分别通过 `3/3`、`10/10`、`6/6`、`2/2`、`1/1`、`5/5`、`9/9`。

> 当前同身份 RenderHarness 的 Light/Dark finesse、scale、media geometry、toolbar、shell 夹具通过；审计索引 `20/20`，summary 实际为 `168` 快照、`7 HIGH/4 MEDIUM`，不能写成 0 风险。motion reentry/hot probes 在当前 Dispatcher 环境重跑未稳定通过，已记录为待验而未改生产动效。freshness 报告 `R01-07-freshness-report-20260923.json` 为 `14/14 fresh`，包身份 `not-provided`。

> 下一可执行项：先寻找能稳定暴露 Playnite 主窗体/UIA 的隔离会话；当前宿主 CEF `platform_channel` 访问拒绝、`Startup Error`/无 summary、ETW/真实 presented frame/物理跨屏/宿主性能仍是边界。若外部条件不变，继续 R23-05 或依赖已满足的 Q/R 小批量；提交前完成当前分支 push、合并 `main`、main 构建复核并清理未被证据引用的 `.tmp`/`artifacts`。

> 2026-09-22 当前 R23-04 窗口证据校正：D 盘 `D:\workplace\github\GameSaveCenter`、分支 `codex/ui-finesse-round2`；`c6d65b08`/`2d327d4b`/`eba374c5` 为 `real-host-audit.ps1` 增加 `host-window-exposure.json`、显式 `-SkipInstallTests` 和全新隔离 profile 配置自举。当前提交 Release `-SkipTests` 构建 XAML `24/24`、`0` errors，`DiagnosticsEvidenceSourceTests 2/2`，既有两条 `MediaCenterView.xaml:699 CS8602` 保留。真实隔离 Playnite PID `31920` 的可见窗口是 `Startup Error`，句柄 `0x19F08B4`，顶层窗口 `5`，UIA 侧栏未确认；runner metadata 为 `restored-from-isolated-backup`，`cef.log` 为 CEF `platform_channel` 拒绝访问 `0x5`，没有 `summary.json`，不宣称 UIA/键盘/读屏/Controlled host 或最终呈现通过。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R23-04-UIA-WINDOW-EXPOSURE-RECHECK-20260922.md` 和 `artifacts/ui-host-audit-r23-04-profile-bootstrap-20260922`。下一可执行任务：取得正常可枚举宿主会话；若仍阻塞则进入 R23-05 几何小批量。单屏、ETW/宿主性能、Demo 原目录边界继续有效。

> 2026-09-22 当前 R22-01 本地镜像时间摘要：代码提交 `b45e31db` 复用现有相对/完整时间投影，修正 Worker `LocalMirrorService.Message` 和维护页紧凑镜像时间行，不改同步、命令、滚动、镜像删除保护或兼容投影。Worker `6/6`、Playnite `R22TimeDisplayBehaviorTests 30/30`；clean shell/full RenderHarness 绑定当前身份，Maintenance Light/Dark 目标尺寸无本批新增问题；全量报告仍保留 Overview/Settings/Task/Save 基线失败，未宣称全局 render-qa。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R22-01-LOCAL-MIRROR-TIME-20260922.md`。D 盘仍是唯一工作区，C 盘旧库不存在；真实 Playnite/UIA/读屏、OS 输入/IME、DPI/跨屏、presented frame、ETW/宿主性能未验。下一可执行项：回到 R23-04 UIA/Controlled host，阻塞时继续独立 Q/R 小批量。

> 2026-09-22 当前 R13/R14 复核：D 盘唯一工作区 `D:\workplace\github\GameSaveCenter`、分支 `codex/ui-finesse-round2`、夹具提交 `8a9a052e`。只补 RenderHarness 合成重复组/来源试运行 DTO 与清空态复位；Core `40/40`、Worker `32/32`、Playnite R13/R14 `19/19`。clean-tree 双主题报告中 Maintenance 无 R13 专属问题，Media 重复组 `2` 组/组内 `4` 项、来源试运行 `4` 项；全报告仍因既有 Overview/Settings/Task/Save 基线失败，未宣称全局 render-qa 或真实宿主通过。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R13-R14-TARGETED-RECHECK-20260922.md`。真实网络/媒体、Playnite UIA/IME、物理跨屏、presented frame、ETW/宿主性能未验。下一可执行项：R23-05 独立几何/性能边界，或取得可枚举 Playnite 主窗体后重跑 R02-06/R23-04。

> 2026-09-22 当前宿主窗口边界：D 盘 `D:\workplace\github\GameSaveCenter`、分支 `codex/ui-finesse-round2`、复测身份 `0157ace6`。真实 Playnite 隔离 profile 加载了 GameSaveCenter 并生成 `EmbeddedPlaynite` 结构/视觉树/资源/滚动证据；完整 runner 门禁为 XAML/Core/Worker/Playnite source/WPF `24/24`、`125/125`、`355/355`、`111`、`101`，Release `0 error / 2` 条既有 `CS8602` warning。插件日志有主窗体创建，但 PID `39900` 的 `MainWindowHandle=0`，Win32/UIA 按 PID 均为 `0`，约 90 秒未生成 `summary.json`。因此不宣称 UIA/键盘/读屏/Controlled host；单屏、presented frame、ETW/宿主性能、Demo 原目录和 WPF 静态脚本仍未验。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R23-04-UIA-WINDOW-EXPOSURE-RECHECK-20260922.md`，审计输出保留、隔离临时目录已清理。下一可执行项：寻找可稳定暴露 Playnite 窗口的隔离桌面会话；若仍阻塞则推进 R23-05 几何小批量。

> 2026-09-22 当前宿主复测：D 盘 `D:\workplace\github\GameSaveCenter`、分支 `codex/ui-finesse-round2`、身份 `f457a7a0`；真实 Playnite 隔离 profile 已加载当前 GameSaveCenter 并生成 `EmbeddedPlaynite` 证据，XAML/Core/Worker/Playnite source/WPF 门禁分别为 `24/24`、`125/125`、`355/355`、`111`、`101`，Release `0 error / 2` 条既有 `CS8602` warning。UIA 未找到侧栏项，约 90 秒未生成 `summary.json`，所以不宣称 UIA/键盘/Controlled host；单屏、presented frame、ETW/宿主性能、Demo 原目录和 WPF 静态脚本仍未验。隔离 profile/临时目录已清理，当前审计目录保留作证据。下一可执行项：继续 R23-04 runner UIA/Controlled 收口，若环境仍阻塞则进入 R23-05 几何小批量。

> 2026-09-22 当前收口：R22-01 时间显示统一已由 `ed03c49e` 补齐生产 `Views/*.xaml` 旧投影负向/相对完整入口正向审计，`R22TimeDisplayBehaviorTests 30/30`、`R22TaskDurationBehaviorTests 2/2`；主分支合并为 `002c2c63`，完整 Release `0 error / 2` 条既有 warning。R22-01 账本现为“已满足，待环境验证”。D 盘 `D:\workplace\github\GameSaveCenter` 是唯一工作区，C 盘旧 worktree 已删除。真实 Playnite/package-host、UIA/读屏、OS 输入/IME、DPI/跨屏、最终呈现、ETW/宿主性能、Demo 原目录和缺失的 WPF 静态脚本仍未验。下一可执行项：R23-04 UIA/Controlled host 收口；报告/复制列/日志继续保留稳定完整时间语义。

> 2026-09-22 当前开发批次：`db2ba3c0` 收口 Maintenance 云端队列表格/选中详情的相对重试时间与完整 Tooltip/HelpText，保留 Worker 报告兼容时间属性。证据/账本提交 `6cb2e53c` 已由 `3801184d` 合并到 `main` 并推送；提交后 R22 时间定向 `29/29`、XAML `24/24`，主分支 Release `0 error / 2` 条既有 `MediaCenterView.xaml.cs:699 CS8602` warning。真实 Playnite/package-host、UIA/读屏、DPI/跨屏、最终呈现、ETW/宿主性能未验。下一可执行项：继续盘点 `DashboardViewModel`/Contracts stale/缓存时间入口。

> 2026-09-22 当前开发批次：D 盘唯一工作区 `D:\workplace\github\GameSaveCenter`，分支 `codex/ui-finesse-round2`；`74d3dcdc` 完成概览云端队列相对/完整时间 Tooltip 与 HelpText，证据/账本提交 `1292b296` 已由 `6aed472b` 合并到 `main` 并推送。提交后 R22 时间定向 `28/28`、XAML `24/24`，主分支 Release `0 error / 2` 条既有 `MediaCenterView.xaml.cs:699 CS8602` warning。下一可执行项仍是继续盘点 `DashboardViewModel`/Contracts stale/缓存时间入口；真实 Playnite/package-host、UIA/读屏、DPI/跨屏、最终呈现、ETW/宿主性能未验。

> 2026-09-22 当前接续点：D 盘 `D:\workplace\github\GameSaveCenter` 是唯一开发工作区，当前开发分支为 `codex/ui-finesse-round2`；C 盘旧 worktree 已删除并完成 Git prune。R22-01 Trainer 版本发布时间批次代码提交为 `c0885757`，证据/交接文档为 `9efc54eb`，已由 `811b4046` 合并到 `main` 并推送。主分支 Release 构建 `0 error / 2` 条既有 `MediaCenterView.xaml.cs:699 CS8602` warning；本批仍未验证真实 Playnite/package-host、UIA/读屏、DPI/跨屏、最终呈现、ETW/宿主性能。下一可执行项：继续按实际绑定盘点 `DashboardViewModel`/Contracts 的 stale/缓存时间入口，并保留报告/复制列/日志的稳定完整时间语义。

> 2026-09-21 接续点校正：用户指出最近交接应以 R12-04 为准；账本核对显示 R12-04（`e4e42f40`）以及 R12-05 至 R12-08 均已有独立事实记录，本轮不回滚、不重做。当前分支随后按真实账本收口 R21-04：新增终态播报不抢焦点、任务页加载结束状态可读的实际 WPF 行为证据 `2/2`，相邻定向 `22/22`、Core `5/5`、Worker `5/5`，隔离 Release `0/0`；真实 Playnite/package-host、UIA/读屏、OS 输入、IME、DPI/跨屏、呈现、ETW、宿主性能和 Demo 原目录未验。相邻旧套件的 `TaskCenterViewResponsiveTests.FailedTaskDetailsPutUserReasonBeforeCollapsedTechnicalDetails` 仍为既有单条失败，未改写。证据见 `design/reviews/ui-finesse-round3-20260921/evidence/R21-04-ASYNC-COMPLETION-ANNOUNCEMENT-20260921.md`。下一可执行任务：R21-05 禁用与隐藏区别。

> 2026-09-22 R22-01 媒体缓存时间显示：D 盘 `GameSaveCenter` 已切换到 `codex/ui-finesse-round2` 作为后续开发目录；`main` 上原有对话框未提交改动已保存为 `2e071b3e`，用户删除的 `src.zip` 未纳入。`48db9ee5`/`2484f132`/`b1afb004` 将媒体收件箱 Offline/Stale 缓存标题接入相对时间和完整本地/UTC Tooltip/Automation HelpText，并补 RenderHarness 契约与 Stale 行为负例。最终 `MediaWorkspaceStateCacheTests 7/7`、`WorkspaceStateSourceTests 9/9 + 1 skip`，隔离 Release `0 error / 2` 条既有 warning，XAML/source/WPF 门禁通过。C 盘旧 worktree 已删除并完成 Git prune，D 盘是当前有效工作区。下一项继续盘点 `DashboardViewModel` 其余 stale/缓存时间入口；真实 Playnite/UIA/呈现/ETW/宿主性能边界保持未验。

> 2026-09-21 R21-03 验证错误播报已按“已有实现优先”收口为“已满足，待环境验证”：复用设置页现有合并验证摘要、字段 HelpText/错误链接、可聚焦目标和清空旧状态逻辑；实际 WPF 错误导航 `1/1`、越界到合法值的错误视觉恢复 `2/2`，异步验证/源审计/数值边界 `16/16`。干净隔离 Release Playnite `net462` / Tests `net472` 构建 `0 errors`，仅 2 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning，source/XAML/diff 与 WPF `0/27/162` 通过；本批无生产代码变更。真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、DPI/跨屏、最终呈现、ETW 和宿主性能未验；Demo 原目录不可用，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R21-03-VALIDATION-ANNOUNCEMENT-20260921.md`。下一可执行任务：R21-04 异步完成播报。

> 2026-09-21 R21-02 控件名称与值已收口为“已满足，待环境验证”：图标/动作按钮、复合选择器、开关和进度条均有生产契约与实际 WPF peer 行为证据，包含 Invoke、选值、无选中 `null`、Off/On/Indeterminate、正常/未知/越界进度与 HelpText。`R21AutomationValueBehaviorTests 21/21`，相关 `35/35`；最新隔离 Release Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 warning，source/XAML/diff 与 WPF `0/27/162` 通过。真实 Playnite/package-host、系统 UIA/读屏、OS 输入、IME、DPI/跨屏、最终呈现、ETW 和宿主性能仍未验；Demo 原目录不可用，main 用户改动未碰、未合并。收口证据见 `design/reviews/ui-finesse-round3-20260921/evidence/R21-02-CLOSEOUT-20260921.md`。下一可执行任务：进入 R21-03 错误播报。

> 2026-09-21 第三轮 R21-02 选择器无选中与开关三态边界已由 `efb42b7b` 完成小批量：复用 ComboBox、`ToggleSwitch` 和共享状态模板，实际 WPF peer 验证无选中 `GetSelection()==null`、选中后单项，以及 `Indeterminate → Off → On`；无生产代码变更。`R21AutomationValueBehaviorTests 21/21`，相关筛选 `35/35`；提交后 D 盘 source-copy Release 构建 Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 warning，source/XAML/diff 通过，WPF `0/27/162`。三态只代表共享控件承载能力，不代表当前业务 Binding 会产生 Indeterminate；不宣称真实 Playnite host、UIA/读屏、呈现、DPI/IME 或性能。Demo 原目录不可用，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R21-02-SELECTOR-TOGGLE-NEGATIVE-20260921.md`。下一可执行任务：继续 R21-02 剩余复合选择器/逐控件状态值负例，之后进入 R21-03 错误播报。

> 2026-09-21 第三轮 R21-02 TaskCenter 任务进度 UIA 值与状态边界已由 `6b56a467` 完成小批量：复用既有 `TaskStatusDto.ProgressValue`、`ProgressDisplay` 和 TaskCenter 生产 Binding，实际 WPF `ProgressBar`/`AutomationPeer` 验证正常 `42`、未知 `-1`、越界 `120` 的名称、RangeValue、范围和 HelpText；本批无生产代码变更。`R21AutomationValueBehaviorTests 20/20`，相关筛选 `34/34`；提交后 D 盘 source-copy Release 构建 Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 warning，source/XAML/diff 通过，WPF `0/27/162`。不宣称真实 Playnite host、Windows UIA/读屏、OS 输入、IME、DPI/跨屏、呈现或性能；链接 `_wpftmp.csproj` 的 `Access denied` 未绕过。Demo 原目录不可用，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R21-02-TASK-PROGRESS-PEER-20260921.md`。下一可执行任务：继续 R21-02 剩余复合选择器/逐控件状态值负例，之后进入 R21-03 错误播报。

> 2026-09-21 第三轮 R21-02 MediaCenter 媒体详情位置值与导航边界已由 `806d5a27` 完成小批量：复用 `MediaDetailNavigationDisplay`、`CanNavigatePreviousMedia` 和 `CanNavigateNextMedia`，实际合成 ViewModel 与 WPF `TextBlock` peer 验证未选择、`1 / 2`、`2 / 2` 三态、前后导航负例及“媒体详情位置”名称；未修改生产媒体业务语义。`R21AutomationValueBehaviorTests 19/19`，本批相关组合筛选 `22/22`；提交身份 D 盘源码副本 Release Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning，source/XAML/diff 通过，WPF 静态检查 `0/27/162`，未见本批新增诊断。不宣称真实 Playnite host、UIA/读屏、OS 输入、IME、DPI/跨屏、呈现或性能；链接工作树 `_wpftmp.csproj` 仍受 `Access denied`，未绕过。Demo 原目录不可用，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R21-02-MEDIA-NAVIGATION-VALUE-20260921.md`。下一可执行任务：继续 R21-02 其他状态值边界，之后进入 R21-03 错误播报。

> 2026-09-21 第三轮 R21-02 MediaCenter 批量动作忙碌态刷新已由 `e0805624` 完成小批量：复用现有 `IsBusy`、三个批量命令和 `RelayCommand`，修复三项漏列于 `RaiseCommandStatesCore` 的刷新列表；实际 WPF `Button.Command` 验证 `可用 → 忙碌禁用 → 恢复可用`。`R21AutomationValueBehaviorTests 17/17`，相关进度/焦点/键盘/无障碍/生产壳层 `65/65`；提交身份 D 盘源码副本 Release Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning，source/XAML/diff 通过，WPF 静态检查 `0/27/162`，未见本批新增诊断。不宣称真实 Playnite host、媒体 IPC/写入、UIA/读屏、OS 输入、IME、DPI/跨屏、呈现或性能；链接工作树 `_wpftmp.csproj` 仍受 `Access denied`，未绕过。Demo 原目录不可用，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R21-02-MEDIA-BATCH-BUSY-CANEXECUTE-20260921.md`。下一可执行任务：继续 R21-02 其他状态值边界，之后进入 R21-03 错误播报。

> 2026-09-21 第三轮 R21-02 MediaCenter 批量动作空选择保护续作已由 `f7b664f1` 完成小批量：复核既有 `UpdateMediaMetadataBatchAsync` 在 null/空 `IList` 下于 IPC 前抛出“请先在媒体列表中选择一个或多个项目。”，只新增实际反射等待行为证据，未改命令、Binding、取消/错误或媒体写入语义。`R21AutomationValueBehaviorTests 16/16`，相关进度/焦点/键盘/无障碍/生产壳层 `64/64`；提交身份 D 盘源码副本 Release Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning，source/XAML/diff 通过，WPF 静态检查 `0/27/177`。不宣称真实 CanExecute/UI Enabled、Playnite host 或媒体写入；链接工作树 `_wpftmp.csproj` 仍受 `Access denied`，未绕过。真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、呈现和宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R21-02-MEDIA-BATCH-EMPTY-GUARD-20260921.md`。下一可执行任务：核对批量动作忙碌态/CanExecute，再继续其他状态值边界，之后进入 R21-03 错误播报。

> 2026-09-21 第三轮 R21-02 MediaCenter 批量动作续作已由 `9fd7223c` 完成小批量：复用三个现有批量命令和两处操作条，仅补“收藏所选媒体”“取消收藏所选媒体”“为所选媒体应用当前备注”三类稳定 Automation 名称；实际 WPF peer 验证名称各出现两次及三个隔离 `IInvokeProvider` 通道。`R21AutomationValueBehaviorTests 15/15`，相关进度/焦点/键盘/无障碍/生产壳层 `63/63`；提交身份 D 盘源码副本 Release Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning，source/XAML/diff 通过，WPF 静态检查 `0/27/177`。Invoke 仅证明隔离控件通道，不代表真实 ICommand/批量选择或媒体写入；链接工作树 `_wpftmp.csproj` 仍受 `Access denied`，未绕过。真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、呈现和宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R21-02-MEDIA-BATCH-ACTIONS-20260921.md`。下一可执行任务：补批量动作禁用/空选择负例，再继续其他逐控件状态/值边界，之后进入 R21-03 错误播报。

> 2026-09-21 第三轮 R21-02 MediaCenter 备注与元数据动作续作已由 `b20dac99` 完成小批量：复用 `MediaComment` Binding、`UpdateMediaMetadataCommand` 和 `ReassignMediaCommand`，仅补备注、保存元数据、移动归类三个控件的稳定 Automation 名称；实际 WPF peer 验证名称、备注 Value 更新和两个隔离 Button Invoke 通道。`R21AutomationValueBehaviorTests 14/14`，相关进度/焦点/键盘/无障碍/生产壳层 `62/62`；提交身份 D 盘源码副本 Release Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning，source/XAML/diff 通过，WPF 静态检查 `0/27/177`。Invoke 仅证明隔离控件通道，不代表真实 ICommand/Playnite 写入；链接工作树 `_wpftmp.csproj` 仍受 `Access denied`，未绕过。真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、呈现和宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R21-02-MEDIA-METADATA-CONTROLS-20260921.md`。下一可执行任务：继续盘点 MediaCenter 批量动作和其他逐控件状态/值负例，之后进入 R21-03 错误播报。

> 2026-09-21 第三轮 R21-02 MediaCenter 收藏开关续作已由 `42f5744d` 完成小批量：复用 `MediaFavorite` Binding、现有 ToggleSwitch 与共享样式，仅补 `AutomationProperties.Name="收藏当前媒体"`；实际 WPF peer 验证名称及 `Off → On → Off` 回切负例。`R21AutomationValueBehaviorTests 13/13`，相关进度/焦点/键盘/无障碍/生产壳层 `61/61`；提交身份 D 盘源码副本 Release Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning，source/XAML/diff 通过，WPF 静态检查 `0/27/177`。未提交身份初跑被源码身份门拒绝，统一提交哈希后重跑通过；链接工作树 `_wpftmp.csproj` 仍受 `Access denied`，未绕过。真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、呈现和宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R21-02-MEDIA-FAVORITE-TOGGLE-20260921.md`。下一可执行任务：继续盘点 MediaCenter 备注/元数据动作和其他逐控件状态/值负例，之后进入 R21-03 错误播报。

> 2026-09-21 第三轮 R21-02 SaveCenter 策略控件续作已由 `e1fadaab` 补证并推送：不改生产 XAML，复用已有四个策略 ToggleSwitch、异常保护/策略模板 ComboBox、锁定版本 CheckBox 的标签、Binding 和 Automation 名称，`R21AutomationValueBehaviorTests 12/12`，相关进度/焦点/键盘/无障碍/生产壳层 `60/60`，实际 WPF peer 验证名称、Toggle/CheckBox `Off → On` 和选值切换。提交身份 D 盘源码副本 Release Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning，source/XAML/diff 通过，WPF 静态检查 `0/27/177`；链接工作树 `_wpftmp.csproj` 仍受 `Access denied`，未绕过。真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、呈现和宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260921/evidence/R21-02-SAVECENTER-POLICY-PEER-20260921.md`。下一可执行任务：继续盘点其他复合选择器和逐控件状态值负例，之后进入 R21-03 错误播报。

> 2026-09-21 第三轮 R21-02 Maintenance 云端队列筛选器续作已由 `f3eecad0` 补证并推送：不改生产 XAML，复用已有 `云端队列状态筛选`、`云端队列类型筛选`、`云端队列时间筛选` 名称、选项来源和 Binding，`R21AutomationValueBehaviorTests 11/11`，相关进度/焦点/键盘/无障碍/生产壳层 `59/59`，实际 WPF peer 验证三个筛选器名称及选值切换。提交身份 D 盘源码副本 Release Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning，source/XAML/diff 通过，WPF 静态检查 `0/27/177`；链接工作树 `_wpftmp.csproj` 仍受 `Access denied`，未绕过。真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、呈现和宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260921/evidence/R21-02-MAINTENANCE-CLOUD-SELECTORS-20260921.md`。下一可执行任务：继续盘点其他复合选择器和逐控件状态值负例，之后进入 R21-03 错误播报。

> 2026-09-21 第三轮 R21-02 MediaCenter 额外选择器续作未改生产 XAML，由 `eac4276f` 补证并推送：复用已有“媒体收件箱视图”“媒体筛选预设”“媒体归类批次状态筛选”“调整归类建议目标”的名称、选项来源和 Binding，`R21AutomationValueBehaviorTests 10/10`，相关进度/焦点/键盘/无障碍/生产壳层 `58/58`，实际 WPF peer 验证四个选择器名称和选项切换。source-copy Release Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning，source/XAML/diff 通过，WPF 静态检查 `0/27/177`。真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、呈现和宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260921/evidence/R21-02-MEDIA-EXTRA-SELECTORS-20260921.md`。下一可执行任务：继续盘点其他复合选择器和逐控件状态值负例，之后进入 R21-03 错误播报。

> 2026-09-21 第三轮 R21-02 MediaCenter 选择器续作已由 `da16043d` 收口并推送：复用收件箱归类目标、当前游戏媒体类型筛选、所选媒体重新归类目标的现有数据源、Binding 和后续命令，补稳定 Automation 名称；`R21AutomationValueBehaviorTests 7/7`，相关进度/焦点/键盘/无障碍/生产壳层 `55/55`，实际 WPF peer 验证三个选择器名称与选中值变化。source-copy Release Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning，source/XAML/diff 通过，WPF 静态检查 `0/27/177`。真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、呈现和宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R21-02-MEDIA-SELECTORS-AUTOMATION-20260921.md`。下一可执行任务：继续盘点其他复合选择器和逐控件状态值负例，之后进入 R21-03 错误播报。

> 2026-09-21 第三轮 R21-02 TrainerCenter 工具设置续作已由 `5d9cb81c` 收口并推送：复用现有工具设置 Binding、`CanTrackProcess` 禁用条件和 ToggleSwitch 实现，为版本/已有实例处理方式/风险类别三个选择器及四个开关补稳定 Automation 名称；`R21AutomationValueBehaviorTests 6/6`，相关进度/焦点/键盘/无障碍/生产壳层 `54/54`，实际 WPF peer 验证三个选择器、四个 ToggleSwitch、Toggle `Off/On` 和版本值变化。source-copy Release Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning，source/XAML/diff 通过，WPF 静态检查 `0/27/177`。真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、呈现和宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R21-02-TRAINER-SETTINGS-AUTOMATION-20260921.md`。下一可执行任务：继续盘点其他复合选择器和逐控件状态值负例，之后进入 R21-03 错误播报。

> 2026-09-21 第三轮 R21-02 TrainerCenter 导入控件续作已由 `c327f92a` 收口并推送：复用两个已有导入确认路径、候选 Binding、确认/取消命令，为 ComboBox 与动作补稳定 Automation 名称；`R21AutomationValueBehaviorTests 5/5`，相关进度/焦点/键盘/无障碍/生产壳层 `53/53`，实际 WPF peer 验证候选值 A→B。source-copy Release Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning，source/XAML/diff 通过，WPF 静态检查 `0/27/177`。真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、呈现和宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260921/evidence/R21-02-TRAINER-IMPORT-AUTOMATION-20260921.md`。下一可执行任务：继续 R21-02 Trainer 工具设置版本/风险/运行策略选择器与开关，再补其他状态值负例，之后进入 R21-03 错误播报。

> 2026-09-21 第三轮 R21-02 进度控件续作已由 `74b559e2` 收口并推送：复用 TaskCenter `ProgressValue`/`ProgressDisplay` 与 Maintenance 远端阶段值，为列表行、所选任务详情和远端隔离下载进度补稳定 Automation 名称与 HelpText；无新增服务、DTO、命令或状态语义。`R21AutomationValueBehaviorTests 4/4`，相关进度/焦点/键盘/无障碍/生产壳层 `52/52`，其中既有 R06/R12 负例继续覆盖未知、排队、零值、越界、取消、成功和远端阶段终态。source-copy Release Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning，source/XAML/diff 通过，WPF 静态检查 `0/27/177`。真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、呈现和宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R21-02-PROGRESS-AUTOMATION-20260921.md`。下一可执行任务：继续 R21-02 剩余复合选择器及逐控件状态/值负例，之后进入 R21-03 错误播报。

> 2026-09-21 第三轮 R21-02 SaveCenter 续作小批量已由 `efa9614f` 收口并推送：复用现有 ToggleSwitch、ComboBox、CheckBox、命令和 Binding，为四个存档策略开关、异常保护等级、策略模板、锁定所选版本及两个策略动作补语义 Automation 名称。`R21AutomationValueBehaviorTests 4/4`，相关键盘/焦点/无障碍/生产壳层回归 `35/35`；隔离 Release 无错误，首次编译仅有 2 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning，后续 no-restore `0/0`；source/XAML/diff 和 WPF 静态检查通过。TaskCenter DataGrid 进度条、Maintenance 远端备份进度及状态/值负例仍待继续，未把 R21-02 写成完成。真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、呈现和宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R21-02-SAVECENTER-AUTOMATION-20260921.md`。下一可执行任务仍为 R21-02 剩余进度条和负例，之后才进入 R21-03。

> 2026-09-21 第三轮 R21-02 控件名称与值完成部分收口，提交 `1ca2d01d`：补 Dashboard/Overview/Maintenance/Trainer 明确进度条与 TaskCenter 三个筛选器的语义名；AutomationPeer 行为 `2/2`，与 R21-01 相关回归 `33/33`。隔离 Release Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671` warning，source/XAML/diff 通过。SaveCenter 外置标签开关、更多复合选择器、DataGrid 进度条及状态/值负例仍待继续，未把部分覆盖写成整项完成。真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、呈现和宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R21-02-AUTOMATION-VALUE-20260921.md`。下一可执行任务仍为 `R21-02` 剩余控件，之后才进入 `R21-03`。

> 2026-09-21 第三轮 R21-01 八入口纯键盘已由 `c3459ebd` 收口到受控 WPF 证据：补上 TrainerCenter 默认工具页四个已有工具栏命令的 Automation 名称，新增八入口前/反向焦点轨迹，R21 新增 `2/2`；相关焦点/键盘/无障碍/生产壳层回归在显式构建身份下 `31/31`。隔离 Release Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671` warning，source/XAML/diff 通过。方向键、Enter/Esc 来自既有 R05/GamePicker 受控夹具；真实逐页 OS 输入、Playnite/package-host、UIA/读屏、IME、物理 DPI/跨屏、呈现和宿主性能仍待验。Demo 原目录不可用，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R21-01-KEYBOARD-TRACE-20260921.md`。下一可执行任务：`R21-02` 控件名称与值，先盘点图标按钮、复合选择器、开关和进度条的 UIA 名称/状态/值与负例。

> 2026-09-21 第三轮 R20-08 状态语气统一已由 `176183ec` 收口：复用 `WorkspaceStatePresenter`、`ActionAvailabilityHints`、`OverviewPriorityResolver`，修正 Shell 概览副标题固定“一切运行正常”的陈旧状态，并统一加载/失败/空/完成/需操作主状态的事实与下一步表达；主状态不再以 Worker/Rclone 替代解释，命令、Binding、取消/错误、安全、选框/滚动条和有限列表保持。定向 `30/30`；相关较宽套件 `25 passed / 1 skipped / 1 failed / 27 total`，唯一失败为未修改的任务详情旧绑定源断言；隔离 Release Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 warning，source/XAML/diff 通过。证据来自合成/fake/隔离 testhost/source-copy；真实 Playnite/package-host、Worker/工具/云端、最终呈现、DPI/UIA/IME、ETW、宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R20-08-STATE-TONE-20260921.md`。下一可执行任务：`R21-01` 八入口纯键盘，先核对 Q24/UIA 键盘行为与现有入口实现。

> 2026-09-21 第三轮 R20-07 最近活动密度已按现有能力收口：Worker active/recent 任务按 `TaskId` 去重并稳定排序，活动审计映射为有限摘要；Playnite `TaskEventUiBatcher` 合并高频进度、限制 `128/32`、终态即时落地，Overview 仅显示最近 8 项并保留虚拟化/本地滚动；TaskCenter 选中项有失败摘要、技术详情 Expander、复制/重试和 220 DIP 时间线。Core `4/4`、Playnite `11/11`；隔离 Release Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 warning，source/XAML/diff 通过。组合 WPF testhost 的详情选择时序首轮失败，单独类进程 `2/2`，未改生产代码。真实 Playnite/package-host、Worker 实时流、最终呈现、DPI/UIA/IME、ETW、宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R20-07-RECENT-ACTIVITY-DENSITY-20260921.md`。下一可执行任务：`R20-08` 状态语气统一，先核对加载/失败/空/完成/需操作文案模板。

> 2026-09-21 第三轮 R20-06 部分可用状态已由 `e32ed599` 收口：复核确认 `ActionAvailabilityHints` 分别覆盖恢复、媒体收件箱、云端和隔离远端恢复的禁用原因，`WorkspaceStatePresenter` 保留 Loading/Empty/Error/Degraded/Offline 独立区域；R02/R07/Workspace 定向 `17 passed / 0 failed / 1 skipped / 18 total`。首轮真实 WPF 回归捕获云端过期横幅 `Style.BasedOn` 的 `DynamicResource` 解析异常，已改为 `StaticResource`；隔离 Playnite `net462` / Tests `net472` 构建 `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671` warning，source/XAML/diff 通过。testhost 关闭阶段有已知 TextServices COM 清理噪声但最终成功。真实 Playnite/package-host、Worker/Named Pipe、Ludusavi/Rclone/云端、最终呈现、DPI/UIA/IME、ETW、宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R20-06-PARTIAL-AVAILABILITY-20260921.md`。下一可执行任务：`R20-07` 最近活动密度，先核对事件摘要、重复进度合并、展开详情和计数来源。

> 2026-09-21 第三轮 R20-05 Stale 可理解已由 `c492bbc2` 实现并完成受控证据：任务/存档/媒体/维护复用已有 stale 状态；云端队列新增最近成功读取时间、失败原因、旧数据保留摘要和重试横幅，读取失败不清除旧记录。定向 `37 passed / 1 failed / 0 skipped / 38 total`，唯一失败为未修改的任务详情断言漂移；隔离 Playnite/Tests `0 errors / 2` 条既有 warning，source/XAML/diff 通过。真实 Playnite/package-host、云端/媒体、呈现、DPI/UIA/IME、ETW、宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R20-05-STALE-UNDERSTANDABLE-20260921.md`。下一可执行任务：从 Q/R 依赖账本选择依赖已满足的小批量。

> 2026-09-20 第三轮 R20-04 零结果恢复已由 `cf09f3f6` 实现并完成受控证据：任务中心复用既有筛选摘要/清除状态；游戏选框、媒体中心、云端队列显示条件摘要并提供只改筛选的恢复动作；云端读取异常明确区别于零结果。定向 `26 passed / 0 failed / 0 skipped / 26 total`；受影响套件 `171 passed / 3 failed / 50 skipped / 224 total` 的 3 条为未修改基线；隔离 Playnite/Tests `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671` warning，source/XAML/diff 通过。真实 Playnite/package-host、媒体/云端、呈现、DPI/UIA/IME、ETW、宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260920/evidence/R20-04-ZERO-RESULT-RECOVERY-20260920.md`。下一可执行任务：从 Q/R 依赖账本选择依赖已满足的小批量。

> 2026-09-20 第三轮 R20-03 首次配置引导已确认由祖先提交 `1a90cd06` 完整实现，本阶段无生产代码变更：维护页环境检查卡、非破坏性 Worker 检查、`OnboardingCompleted` 完成/跳过状态、可返回工作区和显式测试备份入口均已存在。Worker 隔离 `1/1`；Playnite 当前身份定向 `3 passed / 1 skipped / 0 failed / 4 total`，legacy 宿主事实 1 条跳过；Playnite `net462` / Tests `net472` source-copy `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671` warning，Worker.Tests `0/0`，source/XAML/diff 通过。旧产物缺 `GscBuildCommit` 的身份拒绝未计行为失败，固定 `31784686` 后重建复测。真实 Playnite/package-host、外部工具、存档/媒体/云端、呈现、DPI/UIA/IME、ETW、宿主性能待验；Demo 原目录不可用，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R20-03-FIRST-USE-ONBOARDING-20260920.md`。下一可执行任务：`R20-04` 零结果恢复，先核对筛选条件摘要、清除动作和离线错误边界。

> 2026-09-20 第三轮 R20-02 指标统计范围已实现并完成受控证据：复用 `DashboardSnapshotDto` 既有生成时间/计数与当前游戏字段，新增 Overview 全库/当前游戏范围和更新时间显示，快照未加载显示 `—` 而不是默认零，合法零保留；未加载时隐藏依赖快照的比例条/状态胶囊。核心定向 `40/40`，概览相关 4 条宿主断言跳过；更宽筛选 `194 passed / 3 failed / 50 skipped / 247 total`，3 条为未改动的设置字段、空态覆盖层、下拉模板既有基线。Release 隔离 source-copy 编译 Playnite `net462` / Tests `net472` `0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671` warning；source/XAML/diff 通过。链接工作树 obj/WPF 临时项目 `Access denied` 未绕过；真实 Playnite/Worker/外部工具、存档/媒体/云端、呈现、DPI/UIA/IME、ETW、宿主性能待验。Demo 原目录不可用，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R20-02-METRIC-SCOPE-20260920.md`。下一可执行任务：`R20-03` 首次配置引导，先核对环境检查、外部工具设置入口、完成标记与可返回/不自动改配置边界。

> 2026-09-20 第三轮 R20-01 概览下一步已实现并完成受控证据：`OverviewPriorityResolverTests` + `GamePickerViewModelTests` `32/32`，Overview/Shell 交互定向 `5/5`；失败任务、空库、未匹配、可备份分别进入真实任务/选框/刷新入口，可备份不直接启动全库写入，优先级负例已覆盖。Release 隔离 source-copy 编译 Playnite `net462` / Tests `net472` 通过，仅有既有 `MediaCenterView.xaml.cs:671` nullable warning；source/XAML/diff 通过。链接工作树 obj/WPF 临时项目写入受 `Access denied` 阻塞，未绕过权限。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R20-01-OVERVIEW-NEXT-ACTION-20260920.md`。真实 Playnite/Worker/外部工具、存档/云端写入、呈现、DPI/UIA/IME、ETW、宿主性能待验；Demo 原目录不可用，main 用户改动未碰，未合并 main。下一可执行任务：`R20-02` 指标统计范围，先核对数字来源、更新时间与当前游戏/全库范围。

> 2026-09-20 第三轮 R19-08 慢调用可取消已补齐受控证据：外部工具均有显式 timeout/token，超时返回稳定码并终止进程树，IPC 写取消保留 RequestId/可能已接收语义且不以新 ID 重试，Busy 在 finally 解锁，云端只在明确成功时记 Uploaded。Worker `36/36`、Rclone runner `1/1`；Playnite 可执行子集 `7 passed/6 skipped/0 failed/13 total`，Named Pipe 真实时序 6 条因系统权限跳过；另 2 条旧 net472 产物缺 `GscBuildCommit` 身份门失败。无生产代码变更；source/XAML/diff 通过。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R19-08-CANCELLABLE-SLOW-CALLS-20260920.md`。真实 Playnite/Worker 管道、Ludusavi/Rclone/远端写入、网络延迟、呈现、DPI/UIA/IME、ETW、宿主性能待验；Demo 原目录不可用，main 用户改动、`src.zip` 和未跟踪对话框文件未碰，未合并 main。下一可执行任务：`R20-01` 概览下一步，先核对四种首屏状态和真实导航入口。

> 2026-09-20 第三轮 R19-07 外部文件变化已补齐受控证据：媒体缺失/损坏/录像失败有固定占位和迟到保护，打开失败只通知不改路径，打开目录在目标消失时回退到真实父目录；备份详情复用隔离 ZIP/Manifest/哈希恢复校验，失败版本阻止恢复，重新验证后刷新详情。Worker `14/14`，Playnite 纯行为 `7/7`；组合 `17 passed/4 failed/21 total` 中 4 条为旧 net472 产物缺 `GscBuildCommit` 身份门。无生产代码变更；source/XAML/diff 通过。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R19-07-EXTERNAL-FILE-CHANGE-20260920.md`。真实 Playnite/package-host 外部移动/占用/损坏时序、Explorer/播放器、呈现、DPI/UIA/IME、ETW、宿主性能待验；Demo 原目录不可用，main 用户改动、`src.zip` 和未跟踪对话框文件未碰，未合并 main。下一可执行任务：`R19-08` 慢调用可取消，先核对取消、超时、UI 解锁和未知写结果。

> 2026-09-20 第三轮 R19-06 分页快照变化已补齐受控证据：Worker 任务/媒体按稳定时间+ID 游标分页，limit+1 结束；Playnite reset/generation/当前 cursor、媒体 ID 去重窗口和选择锚点保持稳定。Worker `12/12`，Playnite 分页/索引 `10/10`、选择锚点 `4/4`；合并筛选中 1 条旧 R06 测试因旧 net472 产物缺 `GscBuildCommit` 身份门失败，已拆出。无生产代码变更；source/XAML/diff 通过，沿用 `6d1a401b` Release `0 errors/2 warnings`。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R19-06-PAGED-SNAPSHOT-20260920.md`。只覆盖合成/fake/隔离 SQLite/testhost，真实并发变更、Playnite/package-host、呈现、DPI/UIA/IME、ETW、宿主性能待验；Demo 原目录不可用，main 用户改动、`src.zip` 和未跟踪对话框文件未碰，未合并 main。下一可执行任务：`R19-07` 外部文件变化，先核对媒体/备份详情的移动、占用、损坏回退和重新定位。

> 2026-09-20 第三轮 R19-05 Worker 重启恢复已补齐现有能力证据：Worker 启动 reconcile 旧 Queued/Running、整库备份/云端队列恢复，事件 pipe 每连接单独 bounded subscription，Playnite 单连接指数退避并用 change feed/SQLite 修复断线。Worker event/reconcile/cloud `18/18`，Playnite TaskEventUiBatcher `3/3`，XAML/source/diff 通过；独立 Worker 硬重启因 Named Pipe 权限 `1 skipped`，相邻 subscription 一条因旧 net472 产物缺 `GscBuildCommit` 在身份门退出。未新增生产代码。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R19-05-WORKER-RESTART-RECOVERY-20260920.md`。真实 Worker/Playnite 重启、断管、重开、呈现、DPI/UIA/IME、ETW、宿主性能待验，Demo 原目录不可用；main 用户改动、`src.zip` 和未跟踪对话框文件未碰，未合并 main。下一可执行任务：`R19-06` 分页快照变化，先核对 durable cursor、末页变化和稳定选择 ID。

> 2026-09-20 第三轮 R19-04 取消关闭顺序已补齐受控证据：生产 Dashboard 卸载/取消、VM generation、插件 lifetime/Worker owned-process 停止和 Worker durable task reconcile 已形成确定收尾；`LatestRequestCoordinator` + Busy `5/5`，`TaskReconcileService` `1/1`，取消/重启相邻 Worker `13 passed / 1 skipped / 14 total`。未新增生产代码。独立 Worker 硬重启因 Named Pipe 权限跳过；WPF shutdown 源测试因复用旧 net472 产物缺 `GscBuildCommit` 在身份门退出。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R19-04-CLOSE-CANCEL-ORDER-20260920.md`。真实 Playnite 关闭/重开、同刻断管、呈现、DPI/UIA/IME、ETW、宿主性能仍待验，Demo 原目录不可用；main 用户改动、`src.zip` 和未跟踪对话框文件未碰，未合并 main。下一可执行任务：`R19-05` Worker 重启恢复，先在可用 Named Pipe/Worker 进程环境复跑硬重启、进度订阅和重连去重。

> 2026-09-20 第三轮 R19-03 重复执行幂等已核对现有实现并补齐证据：`IpcEnvelope.RequestId`、replay-protected 分类、Worker 持久化 ledger、同 envelope 复核、`REQUEST_ID_CONFLICT`/`REQUEST_IN_PROGRESS`/`REQUEST_INTERRUPTED` 和 Busy 原子门共同保证响应丢失不以新 ID 重复执行，未知结果明确提示且不自动删除/恢复。Worker ledger/消息边界 `16/16`，生产 Busy/按钮 `2/2`，XAML `24/24`、source/diff 通过；真实 Named Pipe 客户端因环境限制跳过。linked WPF `_wpftmp` 构建 Access denied，外部副本 restore 为 `NU1301`，未绕过权限或冒充全量构建。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R19-03-IDEMPOTENCY-20260920.md`。只用合成/fake/隔离数据和本地已有产物，Demo 原目录不可用；真实 Playnite/Worker 管道、呈现、DPI/UIA/IME、ETW、宿主性能待验。main 用户改动、`src.zip` 和未跟踪对话框文件未碰，未合并 main。下一可执行任务：`R19-04` 取消关闭顺序，先核对窗口关闭、请求取消、Worker 断开和重开恢复的确定性清理。

> 2026-09-20 第三轮 R19-02 刷新失败保留草稿已由 `7b2d3afa` 完成证据补齐并推送到 `codex/ui-finesse-round2`：复用生产 `DashboardViewModel` 现有存档/媒体 dirty 标记和稳定 ID 编辑同步，确认失败只更新状态、不清编辑字段，成功返回只覆盖干净字段。新增生产 VM 隔离边界回归 `2/2`，叠加 R11 WPF 备注取消和工作区状态回归 `14 passed / 1 skipped / 15 total`；clean-tree Release `0 errors/2 条既有 warning`，source/XAML/diff 通过。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R19-02-DRAFT-REFRESH-20260920.md`。只用合成 DTO、隔离 VM/WPF testhost，真实 Playnite/Worker 断连时序、presented frame、DPI/UIA/读屏、ETW/宿主性能仍待验；Demo 原目录不可用。main 用户改动、`src.zip` 和未跟踪对话框文件未碰，未合并 main。下一可执行任务：`R19-03` 重复执行幂等，先核对 request ID、重试恢复和未知写结果边界。

> 2026-09-20 第三轮 R19-01 旧请求晚返回已由 `ed107c50` 完成并推送到 `codex/ui-finesse-round2`：复用已有请求取消/代际、`MediaWorkspaceStateCache` 和页面状态投影，修正详情请求在跨游戏/工作区/筛选切换时的启动上下文绑定。工作区切换立即失效详情/媒体代际；`LoadDetailsAsync` 在开始、成功、取消、失败回写前统一校验 workspace、generation、game ID；A 慢成功不能覆盖 B 新失败，B 的标题、数据、选择、更新时间和失败保持一致。定向回归 `30 passed / 1 skipped / 31 total`，clean-tree Release `0 errors/2 条既有 warning`，source/XAML/diff 通过。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R19-01-LATE-REQUEST-CONTEXT-20260920.md`。只用合成/fake/隔离 testhost，真实 Playnite IPC 延迟、宿主时序、presented frame、DPI/UIA/读屏、ETW/宿主性能仍待验；Demo 原目录不可用。main 的用户改动、`src.zip` 和未跟踪对话框文件未碰，当前未合并 main。下一可执行任务：`R19-02` 刷新失败保留草稿，先核对只读刷新与编辑对象的分离和未保存字段保护。

> 2026-09-20 第三轮 R18-08 低性能降级触发已由 `3bfe3d3c` 完成并待文档提交：复用已有无玻璃/无动画回退和 `lowcostprobe`，修复 Media Inspector 归类建议 ListBox 横向 Auto 导致的真实可见溢出，并将同构历史列表设为水平 Disabled；垂直有限列表、Recycling、路径 TextBox 内容滚动、游戏选框、滚动条和命令绑定保留。clean-tree RenderHarness 明确模拟 `glass=false/motion=false`，24/24 组合通过，Effects null、PopupAnimation None、visibleEffects=0、文本 4–147、非输入框 unexpectedOverflow=0；直接相关回归 37/37，Release `0 errors/2 条既有 warning`，source/diff 通过。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R18-08-LOW-PERFORMANCE-FALLBACK-20260920.md`，原始报告保留在 `.tmp/r18-08-lowcost-final-clean/`。这不是真实 Render Tier/GPU、Playnite host、物理 DPI、UIA/ETW 或 presented frame 证据；Demo 原目录不可用。下一可执行任务：`R19-01` 异步竞态与故障恢复入口。

> 2026-09-20 第三轮 R18-07 长时资源曲线已由 `56d8e1b7` + `c5c33e18` 完成并待文档提交：复用既有 `RunEnduranceProbe`，保留原始时间序列并新增 managed/private/working set、threads/handles、探针可见 timers、反射可见托管事件委托、动画属性持有者代理与 thumbnail cache 诊断；1800 秒操作后停止输入静置 30 秒，最终 `1830.2/1830s`、177 样本、2769 周期、8537 动作、0 失败、`enduranceprobe OK`。静置四点周期保持 2769、timers=0，private/working set 回落并尾段稳定。相关源测试 `31/31`，Release `0 errors/2 条既有 warning`，source/diff 通过。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R18-07-LONG-RUN-RESOURCE-CURVE-20260920.md`，原始报告保留在 `.tmp/r18-07-endurance-final/`。报告生成时 working tree 仍含未提交静置 patch，已随后独立提交，未伪造 clean-tree。只用合成/fake/隔离 WPF Window，不触碰真实数据；真实 Playnite/package-host、物理 DPI/跨屏、UIA/读屏、presented frame、ETW/宿主性能仍待验，Demo 原目录不可用。下一可执行任务：`R18-08 低性能降级触发`，先盘点无玻璃/无动画回退与渲染 Tier。

> 2026-09-20 第三轮 R18-06 页面重访成本已由 `1b19fd4c` 实现并待文档提交：复用生产六页 registry 和同页 `PageHost.Content`，增加 15 秒、按页面/稳定游戏/媒体筛选上下文隔离的热态读取门；失败、取消、卸载失效和晚返回不生成新鲜缓存，显式 LoadDetails 仍强制读取。壳层记录页面创建 binding、首次/重访 attach/reuse/layout，VM 记录页面 read/skip/outcome/load；`WorkspaceRevisitLoadGate`/source `7/7`，相关回归 `31/31`，Release 隔离 solution `0 errors/2 条既有 warning`，source/XAML/diff 通过，`.tmp/r18-06-*` 已清理。只用合成/fake/隔离 testhost，无真实数据写入；真实 Playnite 首次/重访耗时、最终呈现、DPI/UIA/读屏、ETW/宿主性能仍待验，Demo 原目录不可用。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R18-06-WORKSPACE-REVISIT-20260920.md`。下一可执行任务：`R18-07 长时资源曲线`，先核对既有耐久脚本和句柄/订阅/时钟/工作集/缓存指标。

> 2026-09-20 第三轮 R18-05 后台事件合并已由 `91947336` 实现并推送到 `codex/ui-finesse-round2`：复用已有 TaskEventBroadcaster、durable change feed、TaskId 索引、批量 ObservableCollection 和 Dashboard 卸载取消。Playnite 进度按 TaskId 合并，pending=`128`、batch=`32`；完成/失败/取消用 DataBind 优先投递，旧进度不会迟到覆盖终态；Worker 固定 128 容量优先淘汰非终态，终态压力样本保留失败事件。`TaskEventUiBatcher 3/3`、Worker `5/5`、相关 Playnite `19/19`，source/XAML/diff 通过，Release `0 errors/2 条既有 warning`；临时输出已清理。未验真实 Playnite 卸载/重载时序、最终呈现、DPI/UIA/读屏、ETW/宿主性能，Demo 原目录不可用；main 用户改动、`src.zip` 和未跟踪对话框文件未碰，未合并 main。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R18-05-BACKGROUND-EVENT-BATCHING-20260920.md`。下一可执行任务：`R18-06 页面重访成本`。

> 2026-09-20 第三轮 R18-04 表格容器预算已由 `64843642` 实现并推送到 `codex/ui-finesse-round2`：复用生产 Task/Media DataGrid、共享模板和 MediaPageAccumulator；正常高度 Media Inbox 使用有限 `Height/MaxHeight`，外层页级滚动只在短页/stale fallback 开启，保留 Media `Standard/Item/禁列虚拟化`、选框、滚动条、绑定、选择和锚点语义。实际 STA WPF 2k/10k/20k 合成规模下，Task 最大 `9` 容器/7 行视口，Media 最大 `9` 容器/9 行视口；Media UI 窗口保持 `2,000`。8 次滚动 p95/最大 Task=`52.846/52.846`、`28.365/28.365`、`34.807/34.807ms`，Media=`0.132/0.132`、`0.289/0.289`、`0.153/0.153ms`。R18-04 `1/1`，相关媒体分页/锚点/几何/滚动 `23/23`，Release `0 errors/2 条既有 warning`，source/XAML/diff 通过；`.tmp/r18-04-solution` 已清理。阶段还修正 c17 媒体详情 `DynamicResource` BasedOn 解析错误，两个此前失败锚点测试恢复通过。首轮先 Show 后布局的 2,000 全量容器负例和最终首次 measure 前布局边界已记录；真实 Playnite 首次 Loaded/宿主时序、物理呈现、DPI/UIA/读屏、ETW/宿主性能仍待验，Demo 原目录不可用，main 用户改动和 `src.zip` 未碰，未合并 main。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R18-04-TABLE-CONTAINER-BUDGET-20260920.md`。下一可执行任务：`R18-05 后台事件合并`。

> 2026-09-20 第三轮 R18-03 缩略图滚动预算已由 `e54d514e` 实现证据、`18c5073f` 补全原始样本并推送到 `codex/ui-finesse-round2`：复用现有 3 路后台解码、96 项 LRU、取消和 generation 迟到结果保护；120 个合成图片请求分 10 个滚动窗口，峰值活动 `3`、缓存封顶 `96/96`、每轮活动归零、托管堆增量代理最大 `90,072 bytes`，预取消 `1`，旧 Missing 结果不改写替换后的 Ready 行。R18-03 `1/1`，AsyncThumbnailLoader/Image 回归 `9/9`，Release 隔离 solution `0 errors/2 条既有 warning`，source/XAML/diff 通过；c17 的 `64×64` 旧断言按当前 PreviewWidth=96 实测校正为 `96×96`，生产 loader 未改。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R18-03-THUMBNAIL-BUDGET-20260920.md`。真实 Playnite 快速滚动、DPI/UIA/读屏、presented frame、ETW 和宿主性能仍待验；Demo 原目录不可用，main 用户改动和 `src.zip` 未碰，未合并 main；`.tmp/r18-03-solution` 已清理。下一可执行任务：R18-04 表格容器预算。

> 2026-09-20 第三轮 R18-02 真实 Dispatcher 基准已由 `59468b37` 实现、`5b28b0c3` 校正来源并推送到 `codex/ui-finesse-round2`：实际 STA WPF `Window` 分别记录 SearchText→VM 刷新完成和 VM 完成→可见 `ListBoxItem` 容器的增量；20 次 p95/最大为 `52.272/63.581ms`、`28.343/49.942ms`，可见计数均为 1，容器几何 `476×19.24 DIP`。R18-02 `1/1`，R18/选框/键盘/IME/防抖合并 `47/47`，source/XAML `24/24`、Release 隔离 solution `0 errors/2 条既有 warning`、diff 通过；无 XAML 变更，WPF 静态沿用 `0/27/162`。测试显式安装 `DispatcherSynchronizationContext`，首轮缺失的真实负例已修正并记录。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R18-02-DISPATCHER-VISIBILITY-20260920.md`。这不是 presented frame、物理 DPI、60fps、Playnite 宿主或 ETW 证据；Demo 原目录不可用，main 用户改动和 `src.zip` 未碰，未合并 main；`.tmp/r18-02-solution` 已清理。下一可执行任务：R18-03 缩略图滚动预算。

> 2026-09-20 第三轮 R18-01 连续输入基准已由 `10bc5789` 完成并推送到 `codex/ui-finesse-round2`：复用现有游戏选框本地缓存、取消和 20ms 防抖路径，增加内部诊断与 2,000/10,000 项合成回放；两档各 30 次连续英文输入，p95/最大同步过滤更新 `4.908/6.121ms`、`12.115/13.813ms`，粘贴/删除/已提交中文 IME 查询和最终单次刷新均通过。R18 基准 `1/1`，选框/键盘/IME/防抖相邻回归 `46/46`，source/XAML `24/24`、Release 隔离 solution `0 errors/2 条既有 warning`、diff 通过；无 XAML 变更，WPF 静态沿用 `0/27/162`。托管堆值是 net472 `GC.GetTotalMemory(false)` 的可复算代理，不是 ETW/真实宿主分配或呈现；原始样本见 `design/reviews/ui-finesse-round3-20260915/evidence/R18-01-CONTINUOUS-INPUT-20260920.md`。Demo 原目录不可用，main 用户改动、`src.zip` 和未跟踪对话框文件未碰，未合并 main；`.tmp/r18-01-solution` 已清理。下一可执行任务：R18-02 真实 Dispatcher 基准，记录 VM 完成到受控窗口可见反馈的两段延迟。

> 2026-09-20 第三轮 R17-08 维护报告可读性已由 `59d4190b` 完成并推送到 `codex/ui-finesse-round2`：复用原维护报告采集、IPC、复制/导出链，增加插件/Worker/Playnite 软件身份，按摘要/待处理/已验证/未知分段，并以同一生成时间和计数输出；统一脱敏 URL 参数、凭据和 Windows 用户路径。Worker R17-08 `2/2`、Playnite `4/4`，合并相关回归 `5/5`/`19/19`，隔离 Release solution `0 errors/2 条既有 warning`，source/XAML/diff、WPF `0/27/162` 通过；`.tmp/r17-08-solution` 已清理。未验真实 Playnite/package-host 文件导出/剪贴板、最终主题/DPI/UIA/IME/读屏/物理跨屏、presented frame、ETW 或宿主性能；只用合成/fake/隔离数据，Demo 原目录不可用。main 用户改动、`src.zip` 和未跟踪对话框文件未碰，当前不合并 main。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R17-08-MAINTENANCE-REPORT-20260920.md`。下一可执行任务：`R18-01 连续输入基准`，先核对 GamePicker 搜索、DebouncedRefresh、IME 和大库合成夹具。

> 2026-09-20 第三轮 R17-07 检查项一键定位已由 `e8d581c6` 完成并推送到 `codex/ui-finesse-round2`：复用既有 Finding/Health/Task 路由和维护返回栈，为健康诊断补 `PlayniteId + BackupId` 精确版本定位；旧 findings 表增量迁移，历史标题前缀兼容，目标缺失不邻近回退，无版本身份不回落到失败任务。Worker 迁移/健康/Finding `18/18`、Playnite R17 `15/15`，隔离 Release solution `0 errors/2 条既有 warning`，source/XAML/diff、WPF `0/27/162` 通过；`.tmp/r17-07-solution` 已清理。未验真实 Playnite/package-host、最终主题/DPI/UIA/IME/焦点滚动、Explorer/权限、presented frame、ETW 或宿主性能；只用合成/fake/隔离数据，Demo 原目录不可用。main 用户改动、`src.zip` 和未跟踪对话框文件未碰，当前不合并 main。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R17-07-FINDING-NAVIGATION-20260920.md`。下一可执行任务：`R17-08 维护报告可读性`，先核对现有报告导出、分组和脱敏器。

> 2026-09-20 第三轮 R17-06 存储分析导航已由 `51cae6b9` 完成并推送到 `codex/ui-finesse-round2`：复用既有逻辑索引、目录实测、TopGames 和稳定 ID 解析；维护页 Demo 卡片区分 SQLite 逻辑体积、备份目录物理实测和磁盘剩余空间，失联路径单独说明且不按零占用处理；游戏/版本入口仅按稳定 `PlayniteId`/`BackupId` 精确跳转，缺失目标不回退。Worker `4/4`、Playnite R17-06 `4/4`、R17 `15/15`，隔离 Release solution `0 errors/2 条既有 warning`，source/XAML/diff、WPF `0/27/162` 通过；`.tmp/r17-06-solution` 已清理。未验真实 Playnite/package-host、最终呈现、DPI/UIA/IME/焦点滚动、Explorer/权限、真实文件系统占用时序、presented frame、ETW 或宿主性能；只用合成/fake/隔离数据，Demo 原目录不可用。main 用户改动、`src.zip` 和未跟踪对话框文件未碰，当前不合并 main。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R17-06-STORAGE-ANALYSIS-NAVIGATION-20260920.md`。下一可执行任务：`R17-07 检查项一键定位`，先核对 Finding/Health/Task 稳定来源和导航返回状态。

> 2026-09-20 第三轮 R17-05 隔离账本入口已由 `3002a8dc` 完成并推送到 `codex/ui-finesse-round2`：复用既有隔离账本 DTO、分页接口、状态和定向恢复命令，在维护行动项中显式显示原路径与隔离路径，并将逐条操作标为“受控恢复”；确认、`EntryId`、冲突停止、残留保留和不默认删除语义未改。Worker 隔离账本 `5/5`、Playnite R17 `12/12`，隔离 Release solution `0 errors/2 条既有 warning`，source/XAML/diff、WPF `0/27/162` 通过；`.tmp/r17-05-solution` 已清理。未验真实 Playnite/package-host、最终呈现、DPI/UIA/IME、Explorer/权限、重启恢复时序、presented frame、ETW 或宿主性能；只用合成/fake/隔离数据，Demo 原目录不可用。main 用户改动、`src.zip` 和未跟踪对话框文件未碰，当前不合并 main。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R17-05-QUARANTINE-LEDGER-20260920.md`。下一可执行任务：`R17-06 存储分析导航`，先核对已有存储统计、来源记录和维护页跳转能力。

> 2026-09-20 第三轮 R17-04 保留预览对比已满足并由 `3c73b498` 补齐失败证据：复用既有 RetentionSimulation 预览、保护项、预计释放、隔离占用、二次确认、预览十分钟时效、live 重算和归档指纹门禁；新增隔离 SQLite 删除失败负例，证明归档恢复原路径、恢复账本保留且 `MovedBytes/FreedBytes` 不计真实释放。最终 Worker Retention `12/12`、Playnite R17 `10/10`、维护源码 `3/3`，隔离 Release solution `0 errors/2 条既有 warning`，source/XAML/diff、WPF `0/28/162` 通过；布局回归 `20 passed/11 skipped` 未写成全绿。未验真实 Playnite/package-host、最终呈现、DPI/UIA/IME、Explorer/权限、真实锁/故障/重启恢复、presented frame、ETW 或宿主性能；只用合成/fake/隔离 SQLite/目录，Demo 原目录不可用。main 用户改动、`src.zip` 和未跟踪对话框文件未碰，当前不合并 main。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R17-04-RETENTION-PREVIEW-20260920.md`。下一可执行任务：`R17-05 隔离账本入口`，先核对分页隔离列表、原/隔离路径、状态和受控恢复入口。

> 2026-09-20 第三轮 R17-03 检查进度预算已由 `87473bc3` 完成并推送到 `codex/ui-finesse-round2`：复用既有健康巡检游标、单次预算、会话/操作锁、延后表和取消/失败终态，在 `LastSummary` 中记录本轮索引范围、需检查/延后/候选数量和未读取归档边界；维护页新增当前/最近候选、最近完成、最近成功与下轮计划/预算显示。游戏运行、锁占用和全候选延后显示暂停原因，取消/预算耗尽/异常结束不伪装整库完成。最终 Worker 健康巡检 `12/12`、Playnite R17 `10/10`，隔离 Release solution `0 errors/2 条既有 warning`，source/XAML/diff、WPF `0/28/162` 通过。未验真实 Playnite/package-host、最终呈现、DPI/UIA/IME、焦点/滚动、presented frame、ETW 或宿主性能；只用合成/fake/隔离 SQLite/目录，Demo 原目录不可用。main 用户改动、`src.zip` 和未跟踪对话框文件未碰，当前不合并 main。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R17-03-INSPECTION-PROGRESS-BUDGET-20260920.md`。下一可执行任务：`R17-04 保留预览对比`，先核对 RetentionSimulation 的候选、保护项、隔离账本和执行前预览过期边界。

> 2026-09-20 第三轮 R17-02 诊断包预览已由 `2b6e9051` 完成：复用既有有限诊断 ZIP、统一脱敏、2 MiB/日志上限和生成结果，新增只读预览清单，列出摘要类别、可选日志、脱敏范围、上限与明确排除项。`database.json` 只含数据库摘要探针，不含真实 SQLite 文件或表内容；存档、媒体、Rclone 凭据和自动上传均排除。Playnite 先确认，取消不生成；确认后显示完整位置/大小并沿用原打开路径动作。最终 Playnite R17 `7/7`、Worker `3/3`，隔离 Release solution `0 errors/2 条既有 warning`，source/XAML/diff、WPF `0/28/162` 通过。未验真实 Playnite/package-host 确认框、Explorer/权限、日志并发、最终呈现、DPI/UIA/IME、ETW 或宿主性能；只用合成/fake/隔离 SQLite，Demo 原目录不可用。main 用户改动、`src.zip` 和未跟踪对话框文件未碰，当前不合并 main。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R17-02-DIAGNOSTICS-PACKAGE-PREVIEW-20260920.md`。下一可执行任务：`R17-03 检查进度预算`，先核对巡检范围、延后原因、最近成功、下轮计划和取消/结束状态。

> 2026-09-20 第三轮 R17-01 健康结果分层已由 `eb033251` 完成：复用既有开放 finding/健康巡检解决语义，补充 `created_utc` 到 DTO 和维护详情证据时间；维护页按真实影响显示需立即处理、建议处理、信息项，并在展示边界合并同游戏/代码/问题标题的重复来源，同游戏不同健康备份保持独立。原问题表、滚动、命令绑定、选中详情和导航保留。最终 Playnite R17 `5/5`、Worker `2/2`，隔离 Release solution `0 errors/2 条既有 warning`，source/XAML/diff、WPF `0/28/162` 通过。未验真实 Playnite/package-host、多来源生产标题、最终呈现、DPI/UIA/IME、ETW 或宿主性能；只用合成/fake/隔离 SQLite，Demo 原目录不可用。main 用户改动、`src.zip` 和未跟踪对话框文件未碰，当前不合并 main。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R17-01-HEALTH-RESULT-LAYERS-20260920.md`。下一可执行任务：`R17-02 诊断包预览`，先核对 DiagnosticsPackage 的类别、脱敏清单和生成后文件结果。

> 2026-09-20 第三轮 R16-08 保存冲突处理已由 `ee6b37c9` 提交并推送到 `codex/ui-finesse-round2`：复用 Playnite 编辑基线/fingerprint，新增三方字段合并；后台仅改动字段自动并入，用户与最新持久化同时改动的字段列出冲突并阻止保存。设置页显示字段级“当前草稿未写入”提示，取消基线转到最新外部值；原保存/视觉/Worker 应用链保留。最终 HEAD 定向 `17/17`，隔离 Release solution `0 errors/2 条既有 warning`，source/XAML/diff、WPF `0/28/162` 通过。未验真实 Playnite 双设置窗口、后台共享对象竞态、宿主错误呈现、最终呈现、DPI/UIA/IME、RenderHarness、ETW 或宿主性能；只用合成 detached/fake/隔离目录，Demo 原目录不可用。main 用户改动、`src.zip` 和未跟踪对话框文件未碰，当前不合并 main。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R16-08-SETTINGS-CONFLICT-20260920.md`。下一可执行任务：`R17-01 健康结果分层`，先核对健康结果、已解决项和跨来源去重时间证据。

> 2026-09-20 第三轮 R16-07 配置导入预览已由 `451195ad` 提交并推送到 `codex/ui-finesse-round2`：复用既有设置导入/导出、架构 v1、缺失路径报告和设备身份保护，新增 detached 预览、版本/兼容性/字段差异/未知字段说明与 Yes/No 确认；未知字段忽略，凭据不进入可分享导出，确认取消/旧架构/坏值不写入，应用前快照支持异常恢复。最终 HEAD 定向 `15/15`，隔离 Release solution `0 errors/2 条既有 warning`，source/XAML/diff、WPF `0/28/162` 通过。未验真实 Playnite/package-host 文件选择器、MessageBox、保存取消、最终呈现、DPI/UIA/IME、RenderHarness、ETW 或宿主性能；只用合成 JSON/detached settings/隔离目录，Demo 原目录不可用。main 用户改动、`src.zip` 和未跟踪对话框文件未碰，当前不合并 main。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R16-07-SETTINGS-IMPORT-PREVIEW-20260920.md`。下一可执行任务：`R16-08 保存冲突处理`，先核对编辑基线、后台更新和字段级冲突/拒绝边界。

> 2026-09-20 第三轮 R16-06 生效条件说明已由 `83e7c745` 提交并推送到 `codex/ui-finesse-round2`：复核 `EndEdit → NotifyVisualSettingsChanged → settings.update → WorkerOptions.Apply/SyncPlan` 后，在常规、备份、外观、自动化四个分类标题旁标注外观即时预览、下一任务/轮询边界和下一次 Playnite 启动；没有笼统要求所有修改重启，保留云端时段与安全模式的已有边界。提交后当前身份定向链路/负例与 R16-05 路径回归 `8/8`，外部隔离 Release `0 errors/2 条既有 warning`，source/XAML/diff、WPF `0/28/162` 通过。未验真实 Playnite/package-host 保存后时序、Worker 重启/轮询呈现、最终主题、DPI/UIA/IME、ETW 或宿主性能；Demo 原目录不可用，main 用户改动和 `src.zip` 未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R16-06-SETTINGS-EFFECT-CONDITIONS-20260920.md`。下一可执行任务：`R16-07 配置导入预览`，先核对现有导入报告、版本/未知字段、凭据和失败回退。

> 2026-09-20 第三轮 R16-05 路径编辑一致已由 `955dc52e` 提交并推送到 `codex/ui-finesse-round2`：核对现有全量异步路径校验、粘贴标准化和导入/导出后，新增六个本地工具/目录字段共用的统一路径编辑卡片，按类型浏览，当前字段单独校验，复制复用脱敏/剪贴板重试，打开严格要求当前路径有效且可读；Rclone 云端目标排除本地打开。权限/缺失/网络不可达由只读探测分层反馈，浏览取消不改草稿。外部隔离 Release `0 errors/2 条既有 warning`，R16-05 定向行为/源码/路径回归 `6/6`，source/XAML/diff、WPF `0/28/162` 通过。未用真实网络共享、用户 ACL、剪贴板或 Explorer；未验真实 Playnite/package-host、文件夹对话框归属、最终呈现、DPI/UIA/IME、ETW 或宿主性能。Demo 原目录不可用，main 用户改动和 `src.zip` 未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R16-05-PATH-EDITOR-20260920.md`。下一可执行任务：`R16-06 生效条件说明`，先查设置字段实际消费点和保存/应用/重启边界。

> 2026-09-20 第三轮 R16-04 恢复默认粒度已由 `2b194461` 提交并推送到 `codex/ui-finesse-round2`：确认现有设置只有整体保存/取消、首次路径补全和导入校验后，新增单字段、单分类和全部默认入口。确认文案列出影响；全部默认只重置安全标量/本地 UI 偏好，明确保留 Worker/Ludusavi/Rclone、存档/媒体/镜像路径、云端目标和设备身份。重置只修改当前 Playnite 草稿并重绑同一对象，不结束编辑、不保存、不启动 Worker，取消仍恢复原草稿。外部隔离 Release `0 errors/2 条既有 warning`，行为 `2/2`、源码 `1/1`，source/XAML/diff、WPF `0/28/177` 通过；使用隔离 `obj` 的 `--no-restore`，未宣称 fresh restore。未验真实 Playnite/package-host、最终呈现、DPI/UIA/IME、ETW 或宿主性能；Demo 原目录不可用，main 用户改动和 `src.zip` 未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R16-04-RESET-GRANULARITY-20260920.md`。下一可执行任务：`R16-05 路径编辑一致`，先核对路径浏览、校验、打开、复制和权限/网络/不存在负例。

> 2026-09-20 第三轮 R16-03 模板应用范围已由 `52fbf5de` 提交并推送到 `codex/ui-finesse-round2`：确认既有代码只有单目标模板应用后，复用模板/策略 DTO、归一化、游戏操作锁、持久化和审计，新增最多 100 个明确稳定 Playnite ID 的批量应用。Save 页面展示目标、排除、变更字段数和跨筛选稳定选择；空选择不全选；Worker 逐项返回结果，失败可单独重试，取消仍传播。外部隔离 Release solution `0 errors/2 条既有 warning`，Core `3/3`、Playnite 源契约 `1/1`、Worker `2/2`，source/XAML/diff、WPF `0/28/177` 通过。fresh restore 在外部副本无诊断退出，构建复核使用现有隔离 `obj` 和 `--no-restore`，未写成全新还原通过；临时副本已清理。未验真实 Playnite/package-host、最终呈现、DPI/UIA/IME、ETW 或宿主性能；Demo 原目录不可用，main 用户改动和 `src.zip` 未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R16-03-POLICY-TEMPLATE-BATCH-20260920.md`。下一可执行任务：`R16-04 恢复默认粒度`，先核对设置恢复入口、敏感连接字段保护和取消保留草稿。

> 2026-09-20 第三轮 R16-02 策略差异预览已由 `b327d5ef` 推送到 `codex/ui-finesse-round2`：复用现有策略/模板 DTO、模板归一化和 Worker IPC，新增 13 字段差异预览与字段通知；Save 页面分开显示已保存基线、显式游戏草稿和模板覆盖值。取消未保存游戏策略只本地恢复基线、不发 `RequestAsync`；有未保存游戏草稿时模板应用禁用，模板仍是一次性复制而非实时继承。外部隔离 Release solution `0 errors/7 warnings`（离线 `NU1900`），核心差异/复制/通知 `5/5`，Playnite 源契约 `1/1`，XAML `24/24`、source/diff、WPF `0/28/177` 通过。未验真实 Playnite/package-host、最终浅深主题呈现、DPI/UIA/IME、ETW、宿主性能或 presented frame；Demo 原目录不可用，linked `obj` 仍 `Access denied`，只用合成/fake/隔离目录。main 用户改动和 `src.zip` 未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R16-02-POLICY-DIFF-20260920.md`。下一可执行任务：`R16-03 模板应用范围`，先核对现有模板 DTO、应用命令和一次性复制边界，再补目标范围/取消与负例证据。

> 2026-09-20 第三轮 R16-01 设置搜索定位已由 `a4e35578` 提交：复用设置页五个分类、原控件和 Binding，新增轻量搜索、`SearchTerms` 匹配登记和结果摘要；搜索仅切换匹配字段/分类可见性，清空恢复进入搜索前分类，验证错误定位先清空搜索并保留已有滚动/焦点路径。外部隔离 Release solution 单节点 `0 errors/10 warnings`（离线 `NU1900` 与既有 `MediaCenterView.xaml.cs:664` nullable），R16 行为 `1/1`、源契约 `1/1`，验证导航/草稿分别独立 `1/1`，XAML `24/24`、source/diff、WPF `0/28/177` 通过。联合 WPF 筛选的 `2 passed/2 failed` 是既有 AppDomain 多 Application 夹具冲突，已拆分 testhost 验证。未验真实 Playnite/package-host、最终浅深主题呈现、RenderHarness presented frame、DPI/UIA/IME、ETW/宿主性能；Demo 原目录不可用，linked `obj` 仍 `Access denied`，只用合成/fake/隔离目录，main 用户改动和 `src.zip` 未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R16-01-SETTINGS-SEARCH-20260920.md`。下一可执行任务：`R16-02 策略差异预览`，先核对现有策略/模板 DTO、继承值与显式覆盖值。

> 2026-09-20 第三轮 R15-08 清理历史范围已确认由既有 `88bde5de`、`a841e42c`、`77d5f346` 实现，`a07f0518` 补齐日期/原因/影响负例与维护页契约证据：全局预览句柄、生成时间、保留/候选/保护统计、预计释放和持久化隔离账本齐全；应用二次确认、预览过期/状态/归档身份校验、运行中备份/恢复/媒体共享锁忙碌跳过、锁定/健康/PreRestore 保护均保留，清理链路不删任务记录。当前外部隔离副本 Worker 清理/隔离账本 `16/16`，Playnite `net462` 维护页 `3/3`，source/XAML `24/24`、diff 通过；本批无生产 XAML 改动，WPF 静态沿用 `0/28/162`。未验真实 Playnite/package-host、RenderHarness presented frame、DPI/UIA/IME、ETW/宿主性能；Demo 原目录不可用，外部临时副本已清理，main 用户改动未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R15-08-HISTORY-CLEANUP-SCOPE-20260920.md`。下一可执行任务：`R16-01 设置搜索定位`，先核对现有设置页定位能力。

> 2026-09-20 第三轮 R15-07 失败结果复制已由 `37dd4a03` 完成代码提交：复用已有任务复制命令、错误字段、恢复报告脱敏文本和剪贴板入口，新增共享 `ClipboardTextSanitizer`、`FailureSummary`、`SafeDetailMessage`、完整复制格式化器与最多 4 次瞬时失败重试；完整复制字段保留但统一脱敏。Task Center 使用脱敏短摘要、错误码和默认收起的只读可选择有限高详情框，选框、滚动条、Binding、取消/错误/恢复保护和 net462 保留。R15 定向 `6/6`，R06/R12/R15 相邻回归 `14/14`，完整外部 Release solution `7 warnings/0 errors`（离线 NuGet `NU1900`），定向 Playnite 的 2 条 warning 为既有 `MediaCenterView.xaml.cs:664`；XAML/source/diff 和 WPF `0/28/162` 通过。未验真实 Playnite/package-host、RenderHarness、最终呈现、DPI/UIA/IME、ETW、宿主性能；linked `obj` 仍 `Access denied`，只用合成/fake/隔离验证，Demo 原目录不可用。main 用户改动和 `src.zip` 未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R15-07-TASK-FAILURE-COPY-20260920.md`。下一可执行任务：`R15-08 清理历史范围`，先核对日期/状态预览、运行中任务保护和恢复账本边界。

> 2026-09-20 第三轮 R15-06 耗时与吞吐已由 `6f65638e` 完成代码提交：复用现有 TaskProgress、TaskStatusDto、SQLite 任务查询和 Task Center 详情，新增明确工作量总量的平滑采样。整库游戏数、媒体专属候选文件数和已知下载总字节才展示速率/ETA；未知总量、远端 rclone、恢复写入、长时间停顿和普通阶段保持未知。采样最多 5 个推进样本，15 秒无推进重置，10 秒无新推进隐藏速率/ETA；旧 SQLite 自动迁移，广播/快照比较器/分页查询保留字段。最终外部隔离 Release solution 到达 Playnite `net462`，`0 errors/2` 条既有 `MediaCenterView.xaml.cs:664` warning，Core `106/106`，Worker 定向 `20/20`，Playnite R15 `11/11`，XAML `24/24`、source/diff 门禁通过，WPF 静态 `0/28/162`。Worker 全量脚本真实结果为 `342 passed/1 skipped/1 failed/344 total`，失败为既有 `MediaSyncServiceTests.cs:570`，未写成全量通过。未验真实 Playnite/package-host、RenderHarness、最终呈现、DPI/UIA/IME、presented frame、ETW 或宿主性能；linked `obj` 仍 `Access denied`，使用外部源码副本。只用合成/fake/隔离 SQLite/目录，Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R15-06-TASK-THROUGHPUT-20260920.md`。下一可执行任务：`R15-07 失败结果复制`，先核对现有任务摘要、错误码、脱敏详情和剪贴板失败语义，并保留本批全量 Worker 失败和真实宿主呈现边界。

> 2026-09-20 第三轮 R15-05 任务来源定位已由 `0d1ff346` 完成代码提交：复用现有任务 DTO、TaskCoordinator、Worker 广播、SQLite 查询、云队列入口和媒体历史 BatchId 恢复，新增稳定来源引用与任务详情来源卡片。版本、媒体批次和云队列按稳定 ID 查找；对象消失时保留诊断并说明未跳转，不按同名/邻近对象兜底；游戏继续使用原关联游戏入口。当前分支外部源码副本 solution Release `0 errors/2 条既有 nullable warning`，Playnite R15 `10/10`，Worker 来源/任务回归 `18/18`，XAML `24/24`、源码/diff 门禁通过，WPF 静态 `0/28/162`。linked WPF 临时项目仍有 Access denied 边界；未验真实 Playnite/package-host、来源卡片最终呈现、DPI/UIA/IME、ETW 或宿主性能。只用合成/fake/隔离 SQLite，Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R15-05-TASK-SOURCE-LOCATION-20260920.md`。下一可执行任务：`R15-06 耗时与吞吐`，先核对已有可靠采样和未知总量语义。

> 2026-09-20 第三轮 R15-04 重复通知归并已由 `a67d371e` 完成代码提交：复用既有 `BoundedTaskIdSet`、会话摘要、通知级别策略、Dashboard Toast 和 Task Center 历史，新增按任务/终态/失败证据归并；进度不领取通知键，相同失败证据只通知一次，不同失败保留，摘要后的新失败/取消不静音，完整错误仍可从历史读取。source/XAML/diff 门禁通过；Playnite Release `net462` 外部源码副本项目构建 0 errors、2 条 `MediaCenterView.xaml.cs:664` 既有 nullable warning；通知/会话/R15 时间线/R13 相邻夹具 `28/28`；WPF 静态 `0/28/177`。linked WPF 临时项目仍 `Access denied`，未宣称完整 solution/RenderHarness/真实宿主；未验 Toast/OS 通知、最终呈现、DPI/UIA/IME、ETW 或性能。只用合成/fake/隔离数据，Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R15-04-TASK-NOTIFICATION-DEDUPE-20260920.md`。下一可执行任务：`R15-05 任务来源定位`，先核对稳定对象身份、已删除对象诊断和同名对象误跳负例。

> 2026-09-20 第三轮 R15-03 任务详情时间线已由 `fe0c05a9` 完成代码提交：复用现有任务变更 DTO、TaskCoordinator、Worker 广播和 Task Center，事件带 Worker 观察到的 `OccurredUtc`；`TaskTimelineBuilder` 按 UTC/序号稳定整理创建、开始、阶段、取消和结束记录，同时显示本地时间与 UTC，缺失事件/时间显示未知，不猜测重试。Dashboard 运行期窗口最多 64 条/任务、200 个任务，详情时间线卡有限高 220 DIP，广播 clone 保留阶段与取消状态。Worker Release 隔离定向 `11/11`，Playnite Release `net462` 构建 0 错误、R06 取消回归 + R15-01/R15-02/R15-03 `11/11`，XAML `24/24`、源码/diff 门禁通过，WPF 静态检查 `0/28/162`；Playnite 保留 `MediaCenterView.xaml.cs:664` 的 2 条既有 nullable warning。未宣称完整 solution、RenderHarness、真实 Worker 重启持久时间线、真实 Playnite/最终呈现、DPI/UIA/IME、ETW 或宿主性能；只用合成/fake/隔离数据，Demo 原目录不可用，沿用恢复生产基线。main 用户改动和 `src.zip` 未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R15-03-TASK-TIMELINE-20260920.md`。下一可执行任务：`R15-04 重复通知归并`，先核对通知、会话摘要和失败历史入口。


> 2026-09-20 第三轮 R15-02 取消过程展示已由 `9c8241fb` 完成代码提交：复用现有 TaskCoordinator、取消 IPC、任务 DTO、SQLite 查询和 Task Center；新增 `Requested`、`Finalizing`、`Cancelled`、`NotInterruptible` 持久化阶段，闸门保证连点取消只发一次，成功/取消竞争、取消后失败和晚到请求都有稳定终态。Task Center 增加取消状态卡，快照比较和任务复制列同步阶段字段；原有命令绑定、滚动、恢复/错误语义和 net462 路径未改。当前提交身份 Worker 定向 `14/14`，Playnite Release `net462` 构建 0 错误、R06 取消回归 + R15 阶段/取消 `8/8`，XAML `24/24`、源码/diff 门禁通过，WPF 静态检查 `0/28/162`；Playnite 保留 `MediaCenterView.xaml.cs:664` 的 2 条既有 nullable warning。完整 solution 脚本在 linked worktree 生成 WPF 临时项目时遇到 `Access denied`，未写成全 solution 通过；Worker/Playnite 项目已分别实际构建。只用合成/fake/隔离数据，Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R15-02-TASK-CANCELLATION-20260920.md`。下一可执行任务：`R15-03 任务详情时间线`，先核对任务事件缓存、阶段字段和详情滚动容器；真实 Playnite/RenderHarness、最终呈现、DPI/UIA/IME、presented frame、ETW 和宿主性能仍待验。

> 2026-09-20 第三轮 R15-01 任务阶段可读已由 `4e7ac33a` 完成代码提交：复用现有 TaskCoordinator、任务 DTO、SQLite 查询和 Task Center；新增共享阶段解析与 `StageMessage`，保留最后真实阶段，终态错误/取消独立显示，未知进度显示 `—`，SQLite 旧库通过迁移默认空值兼容。Worker 定向测试 `12/12`、Playnite Release `net462` 定向测试 `2/2`、XAML `24/24`、源码/diff 门禁通过；Playnite 构建有 `MediaCenterView.xaml.cs:664` 的既有 nullable warning 2 条，WPF 静态检查 `0/28/177`。真实 Playnite/RenderHarness、宿主各类阶段事件全覆盖、最终呈现、DPI/UIA/IME/ETW/性能仍待验。只用合成/fake/隔离数据，Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R15-01-TASK-STAGES-20260920.md`。下一可执行任务：`R15-02 取消过程展示`，先核对取消请求、取消中状态和成功/取消竞争的终态收敛。

> 2026-09-20 第三轮 R14-08 来源规则试运行已由 `89141528` 完成代码提交：复用现有来源规则 DTO、媒体扩展名/文件模式匹配和 IPC，新增只读草稿试运行。样本显示命中/排除、大小、路径和原因；Worker 使用 linked cancellation token、时间/扫描/样本预算，试运行不保存规则、不写媒体记录、不移动文件。来源页新增 `MaxHeight=240`、Recycling 的有限样本列表；同时修复重复组 Border 双子级 XAML 编译结构并保留其原有滚动/虚拟化。`validate-source.py`、XAML `24/24`、diff check、Worker Release 隔离构建 `0/0`、Worker 行为 `1/1`、Playnite Release `net462` 契约 `1/1` 通过；Playnite 构建有 `MediaCenterView.xaml.cs:664` 的既有 nullable warning 2 条，WPF 静态检查 `0/28/177`。render-qa linked `obj` 权限阻塞未写成呈现通过；真实 Playnite/RenderHarness、权限拒绝/超大目录、DPI/UIA/IME/presented frame/ETW/宿主性能仍待验。只用合成/fake/隔离目录，Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R14-08-SOURCE-RULE-PREVIEW-20260920.md`。下一可执行任务：`R15-01 任务阶段可读`，先核对 TaskCoordinator/任务事件阶段 DTO。

> 2026-09-20 第三轮 R14-07 媒体详情浏览已在 `codex/ui-finesse-round2` 完成代码批次：复用现有媒体分页、`SelectedMedia`、稳定 `MediaId` 和滚动锚点，增加当前已加载窗口的上一项/下一项及位置摘要；导航后显式将选中项滚回 `MediaGrid` 可见行。详情复用媒体 DTO 显示类型、来源、大小、采集时间，异步截图显示实际 `PixelWidth × PixelHeight`；视频缺失、不支持格式或 `MediaFailed` 显示回退，既有取消/generation/失败状态保留。`validate-source.py`、XAML `24/24`、diff check 通过；Playnite Tests Release build 退出 1，仅 0 警告/0 错误且无诊断，未写成构建/testhost 通过。跨页导航、真实视频解码、Release/net462、真实 Playnite/RenderHarness、宿主呈现、DPI/UIA/IME、presented frame、ETW、性能仍待验。Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 src.zip 未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R14-07-MEDIA-DETAIL-20260920.md`。下一可执行任务：补跑 R14-04/R14-05/R14-06/R14-07 定向验证，再核对 R14-08。

> 2026-09-20 第三轮 R14-06 批量目标防误选已由 cfbb1279 推送到 codex/ui-finesse-round2：复用现有游戏描述/状态 DTO、Playnite 本地图标解析、Games/SelectedItem 和 TargetPlayniteId，增加只读 IconPath/IdentityDisplay。全局选框保留搜索/平台/状态过滤后隐藏目标和恢复入口；媒体批量归类、预览目标覆盖、重新归类目标共用显示名称、平台、图标（缺失时平台/首字母占位）和稳定 Playnite ID 的模板，没有改成 SelectedIndex。新增 GamePicker 行为夹具及目标模板契约检查；validate-source.py、XAML 24/24、diff check 通过。定向 Playnite testhost 长时间无输出后仅终止当前会话，未写成测试/构建通过；Release/net462、真实 Playnite 呈现、DPI/UIA/IME/ETW/宿主性能仍待验。IconPath 只读本地已有引用，不下载、不写真实数据；Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 src.zip 未碰、未合并。证据见 design/reviews/ui-finesse-round3-20260915/evidence/R14-06-TARGET-GUARD-20260920.md。下一可执行任务：补跑 R14-04/R14-05/R14-06 定向验证，再推进 R14-07 媒体详情浏览。

> 2026-09-20 第三轮 R14-05 重复媒体识别视图已由 `136285d5` 推送到 `codex/ui-finesse-round2`：先核对现有扫描入库已按 SHA-256 去重且 `media.sha256` 有唯一约束，再复用 `MediaItemDto`/`GetMediaAsync` 新增当前游戏范围的有界只读重复检查；相同非空哈希显示“确定重复”，同类型/文件名/大小一致且排除确定组显示“疑似重复”。Media 新 Tab 的组与组内列表使用有限高度、FiniteViewport、Recycling，只支持选择查看/重新识别，没有删除、移动或重新归类命令；请求有取消/generation，失败不阻塞主媒体详情。Worker/Playnite 夹具已加入，源码校验、XAML `24/24`、diff check 通过；linked `obj` Access denied/SDK-Workload 阻塞构建和运行时测试，未写成通过。只用合成/fake/隔离目录，Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R14-05-DUPLICATE-INSPECTION-20260920.md`。下一可执行任务：补跑 R14-04/R14-05 定向验证，再推进 `R14-06 批量目标防误选`。

> 2026-09-20 第三轮 R14-04 撤销边界说明已收口：复用既有 `1c0c5a37`/`a7c39922` 媒体归类历史与撤销链，确认历史页提供最近批次/所选可回退入口，撤销前重新核对应用后快照，冲突项目不覆盖后来人工决定。`03521991` 新增应用后人工修改收藏/备注再撤销的隔离负例，断言 `UndoneWithConflicts`、人工状态和应用后归档路径保留；源码校验、XAML `24/24`、diff check 通过。当前定向 Worker testhost 长时间无输出，未写成运行时通过；Release/net462、Playnite/RenderHarness、真实宿主呈现、DPI/UIA/IME/ETW/性能仍待验。只用合成/fake/隔离目录，Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未碰、未合并。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R14-04-UNDO-BOUNDARY-20260920.md`。下一可执行任务：`R14-05 重复媒体识别视图`，先核对既有 hash/元数据重复检测和只读边界；R14-04 运行时复跑待可用 SDK/Workload。

> 2026-09-20 第三轮 R14-03 部分成功处理已由 `aef251b1` 推送到 `codex/ui-finesse-round2`：复用现有媒体批量归类/忽略/恢复的逐项 best-effort 结果，UI 保留失败稳定 MediaId 和逐项原因；新增有限高度/Recycling 失败列表与“仅重试失败项”确认，重试只提交上次失败集合，成功项不会再次执行。Worker/Playnite 夹具已加入，`validate-source.py`、XAML `24/24`、diff check 通过；Worker/Playnite 定向运行、Release/net462、RenderHarness 未执行。只用合成/fake/隔离目录，Demo 原目录不可用，沿用恢复生产基线；没有新的 artifacts/.tmp，main 用户改动和 `src.zip` 未改、未合并。下一可执行任务：在可用 SDK/Workload 环境补跑 R13-07/R13-08/R14-01/R14-02/R14-03 定向测试与相关回归，再推进 R14-04 撤销边界说明。

> 2026-09-20 第三轮 R14-02 预览选择编辑已由 `69cf2a72` 推送到 `codex/ui-finesse-round2`：复用现有媒体归类预览批次和稳定 `MediaId`，新增预览排除、纳入/排除汇总以及高置信目标覆盖；应用命令按 `SelectedHighConfidenceCount` 门禁，排除项不会提交。Worker 按当前游戏目录和批次 `Pending` 状态校验目标，合法覆盖写回原因后继续原有应用/冲突/取消/恢复保护/撤销链，非法目标跳过并保持未归类。`validate-source.py`、XAML `24/24`、diff check、Contracts/Core Release 隔离构建 `0/0` 通过；当前 Core testhost 未产出可运行结果，Worker/Playnite/RenderHarness 未执行。只用合成/fake/隔离目录，Demo 原目录不可用，沿用恢复生产基线；`.tmp/r14-02-build` 已清理；main 用户改动和 `src.zip` 未改、未合并。下一可执行任务：在可用 SDK/Workload 环境补跑 R13-07/R13-08/R14-01/R14-02 定向测试与相关回归，再推进 R14-03 部分成功处理。

> 2026-09-19 第三轮 R14-01 归类建议解释已由 `7735cd7c` 推送到 `codex/ui-finesse-round2`：先复用现有 `MediaSyncService` 归类建议算法，将来源规则、游戏会话、进程映射和文件名证据结构化显示；多候选逐候选展示依据，无依据显示“待判断”，不伪造目标/置信度。保留 Media Inspector、有限列表、Recycling、滚动、命令绑定、确认/应用/撤销与媒体安全语义。`validate-source.py`、XAML `24/24`、diff check、Contracts/Core Release 隔离构建 `0/0` 通过；Core testhost 因项目引用目标框架评估退出 `1`，Worker restore 退出 `1`，Playnite/RenderHarness 未执行。只用合成/fake/隔离数据，Demo 原目录不可用，沿用恢复生产基线；R14 构建目录已清理，此前 `.tmp/r13-verify-source` 曾短暂被占用，阶段末已精确删除，未强杀未知进程；main 用户改动和 `src.zip` 未改、未合并。下一可执行任务：在可用 SDK/Workload 环境补跑 R13-07/R13-08/R14-01 定向测试，再推进 R14-02。

> 2026-09-19 第三轮 R13-08 失败分类帮助已由 `96a4c6a9` 推送到 `codex/ui-finesse-round2`：复用稳定错误码，新增无空间/限流分类，display-only 区分认证、空间、远端不存在、校验差异、限流下一步；未知错误不猜测；维护详情新增共享提示和默认折叠“原始诊断”，限流继续走有限退避。源码校验、XAML `24/24`、diff check 通过；Core/Worker/Playnite 定向夹具已加入但未执行。当前 SDK `9.0.302` 缺少 Workload resolver 目录，Worker restore 退出 `1`，不宣称 build/test、Release/net462 或真实 rclone/宿主通过。只用合成/fake/隔离数据，Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未改、未合并。`r13-07-source` 首次清理时短暂被占用，阶段末已精确删除，未强杀未知进程。下一可执行任务是在可用 SDK/Workload 环境同时补跑 R13-07/R13-08 定向测试，再推进 R14-01。

> 2026-09-19 第三轮 R13-07 队列筛选与汇总代码已由 `d6c2af90` 推送到 `codex/ui-finesse-round2`：复用现有状态/类型筛选、查询一致性 token、分页追加、去重和选中项恢复，新增游戏/Playnite ID、来源设备、时间窗口筛选与 `GlobalTotalCount`，维护页摘要区分当前筛选与全局计数，筛选栏使用可收缩列；合成 RenderHarness 绑定同步。源码校验通过、XAML `24/24`、diff check 通过；Worker/Playnite 定向夹具已加入但未执行。当前唯一 SDK `9.0.302` 缺少 Workload resolver 目录，Worker restore 退出 `1`，不宣称 build/test 或 Release/net462 通过。只用合成/fake/隔离数据，未写真实云端/存档/媒体/诊断；Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未改、未合并。`r13-07-build` 已清理，`r13-07-source` 首次清理时短暂被占用，阶段末已精确删除。下一可执行任务是在可用 SDK/Workload 环境重跑 R13-07/R13-08 定向测试，验证后再推进 R14-01。

> 2026-09-19 第三轮 R13-06 离线恢复反馈已由 `e4244329` 推送到 `codex/ui-finesse-round2`：复用现有 `CloudRetryPolicy`/`CloudRetryService` 的 1/5/15/60/240/720 分钟退避、最多 6 次自动重试、30 秒轮询、每轮最多 10 项顺序处理和无逐条旧失败通知；新增共享 `NetworkRecoveryDisplay`，等待态解释退避与批次边界，恢复态显示按批次上传，认证失败不误报为网络恢复。Core `1/1`、Worker `10/10`、Playnite R13 `11/11`，Release 外部隔离 solution `0 warning / 0 error`、Playnite `net462`、XAML `24/24`、source validation/diff check 通过。只使用合成/fake/隔离数据，未运行真实网络/rclone、真实 Playnite/package-host、RenderHarness、最终呈现、物理 DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能；未写真实云端/存档/媒体/诊断。Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未改、未合并。本批 `.tmp/r13-06-*` 已清理，`.tmp/r12-07-build-final` 仍因 Access denied 暂留。下一可执行任务为 `R13-07 队列筛选与汇总`，先查现有状态筛选、全局计数、分页和选中项联动。

> 2026-09-19 第三轮 R13-05 远端证据详情已由 `e280cf1c` 推送到 `codex/ui-finesse-round2`：复用现有云端队列/远端布局/维护页详情和复制诊断链，新增 `CloudRemoteDisplay` 及远端对象、来源设备、最后尝试、最后成功校验四项 display-only 证据；URI 用户信息、query/key-value secret、Bearer 值统一脱敏，`RemoteVerified` 以当前 `UpdatedUtc` 显示可证实成功时间，其他状态保持未知。Core `2/2`、Worker `1/1`、Playnite R13 `10/10`，Release 外部隔离 solution `0 warning / 0 error`、Playnite `net462`、XAML `24/24`、source validation/diff check 通过。只使用合成/fake/隔离数据，未运行 RenderHarness、真实 Playnite/package-host、真实远端、最终呈现、物理 DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能；未写真实云端/存档/媒体/诊断。Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未改、未合并。本批 `.tmp/r13-05-*` 已清理，`.tmp/r12-07-build-final` 仍因 Access denied 暂留。下一可执行任务为 `R13-06 离线恢复反馈`，先查现有 Worker/维护页离线状态、恢复入口和错误分类。

> 2026-09-19 第三轮 R13-04 暂停与允许时段已由 `cca3f052` 推送到 `codex/ui-finesse-round2`：复用 `CloudUploadQueuePaused`、允许时段持久化、`CloudRetryService` 与设置页入口，队列摘要实际区分用户暂停、允许时段外、队列空闲和运行中；设置页明确恢复已保存队列、下一轮 Worker 检查生效且不取消已开始上传。Core 定向 `1/1`、Worker `3/3`、Playnite `9/9`、Portable `10/10`，隔离 Debug `0 warning / 0 error`、XAML `24/24`、source validation/diff check 通过。证据见 [`R13-04-PAUSE-WINDOW-20260919.md`](design/reviews/ui-finesse-round3-20260915/evidence/R13-04-PAUSE-WINDOW-20260919.md)。只使用合成/fake/隔离数据和外部构建，未触碰真实云端/存档/媒体/诊断；源代码复核未冒充真实云端运行时验证，未验真实 Playnite/package-host、进行中上传时序、物理 DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能。Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未改、未合并。`.tmp/r12-07-build-final` 仍因 Access denied 暂留，未扩大清理范围。下一可执行任务为 `R13-05 远端证据详情`，先核对远端验证状态、脱敏和已上传/已验证区分。

> 2026-09-19 第三轮 R13-03 手动重试范围已由 `680ea83a` 推送到 `codex/ui-finesse-round2`：复用现有维护页选中项、任务中心单项/批量和 IPC ledger，DTO 现在明确失败/排队才可重试，维护页明确只复制当前选中云端项、不重新执行本地备份；传输中、已上传、已校验负例不会重复提交。实际 `RelayCommand.CanExecute` 忙态第二次点击提交数为 `1`；任务中心批量继续按当前筛选结果和任务类型/游戏去重。Playnite `8/8`、Core `29/29`、Worker 云状态/部分成功 `19/19`、IPC ledger `6/6`，隔离 Debug `0 warning / 0 error`、XAML `24/24`、source validation/diff check 通过。Playnite named-pipe 类为 `1 passed / 6 skipped / 0 failed`，跳过未写成真实 IPC 通过。证据见 [`R13-03-MANUAL-RETRY-SCOPE-20260919.md`](design/reviews/ui-finesse-round3-20260915/evidence/R13-03-MANUAL-RETRY-SCOPE-20260919.md)。只使用合成/fake/隔离数据和外部构建，未触碰真实云端/存档/媒体/诊断；未验真实 Playnite/package-host、物理 DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能。Demo 原目录不可用，沿用恢复生产基线；main 用户改动和 `src.zip` 未改、未合并。`.tmp/r12-07-build-final` 仍因 Access denied 暂留，未强杀未知进程。下一可执行任务为 `R13-04 暂停与允许时段`，先核对 `CloudUploadQueuePaused`、允许时段、持久化队列状态和进行中上传边界。

> 2026-09-19 第三轮 R13-02 下次重试时间已由 `6fd22892` 推送到 `codex/ui-finesse-round2`：复用 `NextAttemptUtc/NextAttemptLocal`，详情同时显示绝对时间和有界相对提示；未来、到期、无自动重试分别可读，过期不出现负倒计时，也没有新增每行常驻计时器。Core `29/29`、Playnite `2/2`、Worker `11/11`，隔离 Debug `0 warning / 0 error`、XAML `24/24`，source validation/diff check 通过。外部隔离源码/输出已清理；未验真实 Playnite/package-host、真实远端、物理 DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能，Demo 原目录不可用，沿用恢复生产基线。main dirty R08 文件和 `src.zip` 未改、未合并。下一可执行任务为 `R13-03 手动重试范围`，先核对单项/媒体重试入口、幂等 requestId 和部分成功语义。

> 2026-09-19 第三轮 R13-01 队列阶段展示已由 `803470b8` 推送到 `codex/ui-finesse-round2`：复用现有云端状态机、队列摘要和维护页，在 DTO 派生显示中区分等待队列、等待网络、等待重试、上传中、验证中、等待验证和已验证；认证失败不冒充网络等待，详情继续保留上传与远端校验的 Guarantee 区分。Core `27/27`、Playnite `7/7`、Worker `11/11`，隔离 Debug `0 warning / 0 error`、XAML `24/24`，source validation/diff check 通过。外部隔离源码/输出已清理；未验真实 Playnite/package-host、真实远端、物理 DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能，Demo 原目录不可用，沿用恢复生产基线。main dirty R08 文件和 `src.zip` 未改、未合并。下一可执行任务为 `R13-02 下次重试时间`，先核对 NextAttemptUtc/NextAttemptLocal、时钟变化和页面生命周期。

> 2026-09-19 第三轮 R12-08 恢复结果报告已由 `fd4756ea` 推送到 `codex/ui-finesse-round2`：复用既有恢复编排、任务状态、PreRestore 和任务详情滚动容器，报告覆盖执行目标、预览文件范围、保护备份、失败阶段、完成/回滚/人工介入/取消/失败结果和稳定任务 ID；SQLite 最近/活动/分页查询可回读，复制命令输出脱敏摘要。最终外部隔离 Debug `0 warning / 0 error`、XAML `24/24`，Playnite R12 `15/15`，Worker 定向 `34/34`，source validation/diff check 通过。当前 linked worktree 直接构建 WPF 临时项目仍 Access denied，最终使用当前分支外部源码副本；未验真实 Playnite/package-host、全量 WPF、物理 DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能，Demo 原目录不可用，沿用恢复生产基线。main dirty R08 文件和 `src.zip` 未改、未合并；`.tmp/r12-07-build-final` 仍待下一启动重试清理。下一可执行任务为 `R13-01 队列阶段展示`，先核对 CloudTransferCoordinator/维护页现有阶段投影和 Q22 依赖。

> 2026-09-19 第三轮 R12-07 预览失效重验已由 `00724e62` 推送到 `codex/ui-finesse-round2`：复用既有恢复确认、映射、readiness 和 PreRestore 链路，确认返回后若游戏/版本变化则拒绝旧确认；Worker 在最新映射下于写入前重预览、写入后做结果校验；同大小不同内容的归档由 Manifest SHA-256 识别失效。隔离 Debug `0 warning / 0 error`、XAML `24/24`，Playnite R12 `13/13`，Worker `27/27`，source/diff check 通过。无 XAML/视觉资源改动，选框、滚动条、命令绑定、取消/错误、恢复保护和 net462 保持。证据仅来自合成/fake/隔离目录；真实 Playnite/package-host、全量 WPF、物理 DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能仍未验；Demo 原目录不可用，沿用恢复生产基线。main dirty R08 文件和 `src.zip` 未改、未合并。`.tmp/r12-07-build-final` 已尝试精确清理但 VB/C# 编译器服务器拒绝 shutdown，未强杀未知进程；下一次启动先重试清理，再执行 `R12-08 恢复结果报告`。

> 2026-09-19 第三轮 R12-06 恢复冲突说明已由 `423856b2` 推送到 `codex/ui-finesse-round2`：复用既有 readiness/task/error 和四阶段恢复投影，为游戏运行、操作锁、磁盘空间、目标权限及未知写入失败分别提供处理步骤；权限证据不足时不误分类，且不建议关闭安全机制。SaveCenter 仅增加现有流程卡片的处理步骤与 Automation HelpText，命令/绑定、取消/错误、PreRestore/回滚、选框、滚动和 net462 保持。隔离 Debug 构建 `0 warning / 0 error`、XAML `24/24`，Playnite R12 `11/11`，Worker 恢复编排 `12/12`，源码校验/diff check 通过。全量 WPF 未运行，真实 Playnite/package-host、物理 DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能仍未验；Demo 原目录不可用，沿用恢复生产基线。main dirty R08 文件和 `src.zip` 未改、未合并。下一可执行任务为 `R12-07 预览失效重验`，先核对已有 preview/readiness 缓存失效与重验路径。

> 2026-09-19 第三轮 R12-05 远端下载进度已由 `23e5b9d4` 推送到 `codex/ui-finesse-round2`：复用现有 `TaskCoordinator`/任务事件流和 remote staging 服务，维护页能区分准备、下载到隔离区、校验、版本确认和等待恢复确认；成功不显示为恢复完成。取消/失败清理本次隔离目录，终态准确保留已清理或清理失败残留；原游戏选框、滚动条、命令/绑定、取消/错误、PreRestore 保护和 net462 保持。最终 Debug 隔离构建 `0 warning / 0 error`、XAML `24/24`，R12 Playnite `3/3`、Worker `22/22`，source/XAML/diff check 通过。全量隔离 WPF 在既有 `R07ResizeStressBehaviorTests.ResizeSequenceKeepsOpenTaskDetailsAndPickerFocusReachable` 失败处按门禁停止，未写成全量通过。Demo 原目录不可用，沿用恢复生产基线；真实 Playnite/package-host、rclone/Ludusavi、物理 DPI/跨屏、UIA/IME、presented frame、ETW、宿主性能仍未验。main dirty R08 文件和 `src.zip` 未改、未合并。下一可执行任务为 `R12-06 恢复冲突说明`，先核对已有冲突/恢复结果 DTO 与负例行为。

> 2026-09-18 第三轮 R07-03 短窗底栏可达已满足当前可控范围：`d19e848b`、`447ac07e` 核对并复用 shell 固定 36 DIP `FooterSurface`、主内容行及各页既有提示条/页面/表格/详情滚动；仅在隔离 RenderHarness 增加实际 WPF 短窗探针和合成 Media page-more 状态。Light/Dark、1040×700/560 共 16 个组合的 Media `LoadMore`、Save `保存策略`、Task 详情 `取消任务`、Maintenance 云端 `加载更多` 均通过实际矩形、可见性、祖先与 ScrollViewer offset 检查，`shortwindowprobe OK`；Save 还记录了最大滚动不能证明中间按钮可达的负例并改用 `BringIntoView`。相邻回归 `20/20`、XAML `24/24`、编译 `0 warning/0 error`、源码校验/diff check 通过。clean 报告 `.tmp/r07-03-short-window/shortwindowprobe-report.txt` 绑定 `447ac07e...`、`WorkingTreeClean=True`。Demo 原始目录仍不存在，沿用恢复生产基线；证据只覆盖合成数据、隔离 STA WPF/offscreen logical DIP，不等价真实 Playnite/Worker、设备输入、UIA/读屏、物理 DPI/跨屏、presented frame、ETW 或宿主性能，未写真实存档/媒体/云端/诊断数据。下一可执行任务为 R07-04 横向滚动端点，证据见 [`R07-03-SHORT-WINDOW-20260918.md`](design/reviews/ui-finesse-round3-20260915/evidence/R07-03-SHORT-WINDOW-20260918.md)。

> 2026-09-18 第三轮 R07-02 锚点删除回退已满足当前可控范围：`7acb61a5` 新增共享稳定键/旧索引恢复器，接入 Task、Media 主库/Inbox、Findings、进程映射、云端队列和 Save 历史/候选；对象删除或筛选后选邻近项，不跳首行或错选同索引对象，任务导航目标和云端一致性 pending key 保持既有优先级。`ef53a748` 补 Save 候选删除负例。`R07SelectionAnchorBehaviorTests 4/4`，最终相邻回归 `20/20`、0 skipped；清理 Release 输出后 XAML `24/24`、编译 `0 warning/0 error`、源码校验/diff check 通过。clean RenderHarness `.tmp/r07-02-anchor-final/render-qa-report.txt` 绑定 ef53a748、`WorkingTreeClean=True`、Light/Dark、多尺寸/滚动/resize、50/400/2000/4468 合成数据量 `render-qa OK`，Task/Media/Maintenance 代表图已查看。Demo 原始目录仍不存在，沿用恢复生产基线；真实 Playnite/Worker、后端刷新竞态、设备输入、UIA/读屏、物理 DPI/跨屏、presented frame、ETW、宿主性能仍未验，未写真实存档/媒体/云端/诊断数据。下一可执行任务为 R07-03 短窗底栏可达：在 1040×700 及更短内容区核对顶部提示条叠加后的保存、取消、加载更多可达性。证据见 [`R07-02-SELECTION-ANCHOR-20260918.md`](design/reviews/ui-finesse-round3-20260915/evidence/R07-02-SELECTION-ANCHOR-20260918.md)。

> 2026-09-18 第三轮 R07-01 滚动所有权已满足当前可控范围：`9c878b9c` 先复用 `GscPageScrollViewer` 页面主滚动、DataGrid 模板 `DG_ScrollViewer` 表格滚动和 `GscInspectorScrollViewer` 详情滚动，保持现有滚动条、`CanContentScroll`、虚拟化、游戏选框、命令/Binding、取消/错误/恢复保护、有限列表和 net462。新增 `ScrollBoundaryRoutingBehavior` 接入两个共享 ScrollViewer 样式与共享 DataGrid 样式：内层还能向当前方向滚动时不抢事件，到边界才转给最近可滚动外层；实际 DataGrid 模板内部 ScrollViewer、普通嵌套详情上下边界由 `R07ScrollOwnershipBehaviorTests 2/2` 验证。最终提交重新构建后相邻 Task 响应式/Media 四行/详情 disclosure/R06 详情回归 `14/14`，Release XAML `24/24`、编译 `0 warning/0 error`、源校验/diff check 通过。clean RenderHarness `.tmp/r07-01-scroll-final/render-qa-report.txt` 绑定完整 SHA，`WorkingTreeClean=True`、Light/Dark、多尺寸和 resize `render-qa OK`；1040×700 工作页四行门禁与 1366 Task `4/4` 通过，Task/Media/Maintenance 图已查看。Demo 原始目录仍不存在，沿用恢复生产基线；未验真实 Playnite/Worker、设备滚轮/触控板轨迹、UIA/读屏、OS 输入/IME、物理 DPI/跨屏、presented frame、ETW、宿主性能，未写真实存档/媒体/云端/诊断数据。下一可执行任务为 R07-02 锚点删除回退：先核对刷新、删除、筛选、加载更多的稳定 ID 和视口恢复路径。证据见 [`R07-01-SCROLL-OWNERSHIP-20260918.md`](design/reviews/ui-finesse-round3-20260915/evidence/R07-01-SCROLL-OWNERSHIP-20260918.md)。

> 2026-09-18 第三轮 R06-08 详情与行高预算已满足当前可控范围：`07376adb` 先复用 Task/Media/Save/Maintenance 既有详情区、选中绑定、独立滚动和紧凑行高预算，只修 Save 刷新后候选选中总是回到首个 Pending 的缺口，按 `PlayniteId + Path` 恢复旧候选并保留 Pending/首项回退。`R06DetailsBudgetBehaviorTests 2/2` 验证 Save 稳定选择和真实生产 TaskCenter 长诊断切换、详情对象更新、技术详情展开、详情滚动与列表行高；相邻 Task 响应式/Media 几何/详情 disclosure `10/10`。Release XAML `24/24`、编译 `0 warning/0 error`、源校验/diff check 通过。clean RenderHarness `.tmp/r06-08-render-final/render-qa-report.txt` 绑定完整 SHA，`WorkingTreeClean=True`、Light/Dark、多尺寸和 resize `render-qa OK`；Task 最窄 `4/4`，紧凑详情 `160 DIP`，宽布局 `360×516` 侧栏，Save/Media/Maintenance 四行门禁保留；Task/Save 代表图已查看。Demo 原始目录仍不存在，沿用恢复生产基线，保留选框、滚动条、命令/Binding、取消/错误/恢复保护、有限列表和 net462。证据限于合成数据、隔离 STA WPF/offscreen logical DIP，真实 Playnite/Worker、UIA/读屏、物理 DPI/跨屏、presented frame、ETW、宿主性能和真实服务失败时序仍未验，未写真实存档/媒体/云端/诊断数据。下一可执行任务为 R07-01 滚动所有权：先盘点页面、表格、详情、弹层 ScrollViewer 的所有权和事件边界。证据见 [`R06-08-DETAIL-ROW-BUDGET-20260918.md`](design/reviews/ui-finesse-round3-20260915/evidence/R06-08-DETAIL-ROW-BUDGET-20260918.md)。

> 2026-09-18 第三轮 R06-07 空表保留结构已满足当前可控范围：`5046bf8f` 先核对 Task/Media/Maintenance 已有状态 presenter 与恢复命令，只修 Save 原先 `IsBusy + Count == 0` 无法区分加载/首次空/读取失败的缺口。Save 详情现在区分 Loading、Empty、Ready、Error 和已有数据失败时的 Stale；历史/候选表头、列宽、排序、滚动和 `LoadDetailsCommand` 保持，加载中隐藏空文案，首次历史为空与候选处理完成/本次扫描无新结果分开表达，失败可重试，旧数据失败保留行并显示降级提示。`R06EmptyStateBehaviorTests 2/2`、`WorkspaceStateSourceTests 9 passed / 1 intentional legacy skip`、`TaskCenterViewResponsiveTests 7/7`、`R06TaskProgressBehaviorTests 4/4`，Release XAML `24/24`、编译 `0/0`、源码校验/diff check 通过。clean RenderHarness `.tmp/r06-07-emptytables/emptytables-report.txt` 绑定完整 SHA，`WorkingTreeClean=True`、Light/Dark、1040×700/1600×900 `emptytables OK`；Save/Task/Media/Maintenance 空表/列表结构保留，Save Light 历史与 Dark 路径核验图已查看。Demo 原始目录仍不存在，沿用恢复生产基线；保留游戏选框、滚动条、命令/Binding、取消/错误/恢复、安全语义、有限列表和 net462。范围为合成数据、fake 服务、隔离 STA WPF/offscreen logical DIP；真实 Playnite/Worker 时序、UIA/读屏、OS 输入/IME、物理 DPI/跨屏、presented frame、ETW、宿主性能和真实服务失败时序仍未验，未写真实存档/媒体/云端/诊断数据。证据见 [`R06-07-EMPTY-TABLE-STRUCTURE-20260918.md`](design/reviews/ui-finesse-round3-20260915/evidence/R06-07-EMPTY-TABLE-STRUCTURE-20260918.md)。下一可执行任务为 R06-08 详情与行高预算，先核对现有详情区、选中对象同步和四行门禁。

> 2026-09-18 第三轮 R06-06 行内进度稳定已满足当前可控范围：`72fd6d9a` 先复核 `TaskStatusDto` 的进度显示层、`SnapshotComparers.Task`、`TaskIndexedCollection.Merge` 和分页 `TaskSummary`；实时事件对已有任务只发 `Replace`，不发全表 `Reset`，总数继续来自服务端汇总。TaskCenter 新增局部 `TaskCancellationStatusText`，按既有 `IsCancellingTask` 显示“正在取消…”并提供兼容 net462 的 HelpText，不改变取消确认、Worker 请求、终态或滚动模型；`fd9326d7` 补实际生产视图可见性测试。`R06TaskProgressBehaviorTests 4/4`，TaskIndexed `4/4`、Batch `3/3`、R03 数值 `10/10`、R06 选中焦点 `2/2`、排序 `4/4`，Release XAML `24/24`、编译 `0/0`、源码校验/diff check 通过。clean RenderHarness 绑定生产代码 `72fd6d9a`，双主题 357 PNG、任务页多尺寸/滚动/虚拟化/resize、`WorkingTreeClean=True`、`render-qa OK`，Light/Dark Task 1040×700 图已抽查，报告为 `.tmp/r06-06-render-final/render-qa-report.txt`。保留游戏选框、滚动条、命令/Binding、取消/错误/恢复、安全语义、有限列表和 net462；未验真实 Playnite 长任务/取消竞争、UIA/读屏、物理 DPI/跨屏、presented frame、ETW、宿主性能；未写真实存档/媒体/云端/诊断数据。证据见 [`R06-06-TASK-PROGRESS-STABILITY-20260918.md`](design/reviews/ui-finesse-round3-20260915/evidence/R06-06-TASK-PROGRESS-STABILITY-20260918.md)。下一可执行任务为 R06-07 空表保留结构，先盘点首次空、筛选空、全部处理完和读取失败的现有 presenter 与恢复命令。

> 2026-09-18 第三轮 R06-05 列头说明已满足当前可控范围：`7527e638` 先核对共享列头的 Wrap/None、22 DIP 排序箭头槽、透明列宽拖拽 Thumb 和已有 `B/KiB/MiB/GiB` 1024 进制格式化，再新增 `DataGridColumnHeaderHelpBehavior`。说明挂在 `DataGridColumn`，生成 header 同步 Tooltip 与 `AutomationProperties.HelpText`；Header 保持字符串，不加入按钮或独立点击路由，排序、重排、拖拽和 `DataGridStableSortController` 不变。Save/Task/Media/Maintenance Findings 的时间、容量、百分比、类型/来源、状态/等级和详情列已接入。`R06ColumnHeaderHelpTests 2/2`、R06-04 复制 `3/3`、R06-03 选中焦点 `2/2`、R06-02 + R06-01 `9/9`，Release XAML `24/24`、编译 `0/0`、源校验/diff check 通过。clean RenderHarness 绑定完整 SHA，双主题 357 PNG、生产主要表格 header contract `resize=true/sort-arrow=visible`、50/400/2000/4468 数据量、滚动/虚拟化/resize、`WorkingTreeClean=True`、`render-qa OK`，Save/Task/Maintenance Light/Dark 图已抽查，报告为 `.tmp/r06-05-render-final/render-qa-report.txt`。保留游戏选框、滚动条、命令/Binding、取消/错误/恢复、安全语义、有限列表和 net462；未验真实 Playnite 悬停/排序/拖拽、UIA/读屏、物理 DPI/跨屏、presented frame、ETW、宿主性能；未写真实存档/媒体/云端/诊断数据。下一可执行任务为 R06-06 行内进度稳定，先核对任务进度值/显示/刷新路径与选择滚动保持。

> 2026-09-18 第三轮 R06-04 复制单元格与整行已满足当前可控范围：`afe4aa55` 先复用既有路径/任务错误/诊断/维护报告复制命令、`CopyTextWithRetryAsync` 和生产 DTO，再新增共享 `DataGridClipboardBehavior`。五类生产表约定 `Ctrl+C` 复制 Extended 选中行、`Ctrl+Shift+C` 复制当前单元格，按显示顺序输出稳定 TSV/CRLF，稳定 ID 去重，技术字段保留完整原值，凭据语法输出 `[已隐藏]`；公共旧复制入口同步脱敏。`R06ClipboardBehaviorTests 3/3`、R06-03 选中焦点 `2/2`、R06-02 + R06-01 `9/9`，Release XAML `24/24`、编译 `0/0`、源校验/diff check 通过。全量 Playnite 合跑观察值为 `573/679` 通过、`57` 跳过、`49` 既有 WPF 环境性失败，未写成全量绿色。clean RenderHarness 绑定完整 SHA，双主题 357 PNG、50/400/2000/4468 数据量、滚动/虚拟化/resize、`WorkingTreeClean=True`、`render-qa OK`，Task/Media/Maintenance 图已抽查，报告为 `.tmp/r06-04-render-final/render-qa-report.txt`。保留游戏选框、滚动条、命令/Binding、取消/错误/恢复、安全语义、有限列表和 net462；未验真实 Playnite 选择、OS 剪贴板、UIA/读屏、IME、物理 DPI/跨屏、presented frame、ETW、宿主性能；未写真实存档/媒体/云端/诊断数据。下一可执行任务为 R06-05 列头说明，先盘点表头、单位、Tooltip/Automation 和共享资源。

> 2026-09-18 第三轮 R06-03 选中焦点区分已满足当前可控范围：`d83c7378` 在共享 `WpfUiProduction.xaml`/主题调色板中区分 active selected、keyboard focus `2 DIP`、hover、inactive selection 和 `TaskState.Failed` 错误行，保持 DataGridCell 透明内容面；Media Inbox 移除会覆盖共享状态的本地 hover/selected trigger。原始 Demo `DesignShellView.xaml`/`Pages` 当前 checkout 不存在，已记录事实并沿用恢复生产基线。`R06SelectionStateBehaviorTests 2/2`、资源字典 `137/137`（39 skip、0 fail）、R06-02 排序 `4/4`、R06-01 列宽 `5/5`，Release XAML `24/24`、编译 `0/0`、源校验/diff check 通过。clean RenderHarness 绑定该 SHA，Light/Dark 357 PNG、`WorkingTreeClean=True`、`render-qa OK`，Task/Media/Save 代表图已抽查，报告为 `.tmp/r06-03-render-final/render-qa-report.txt`。证据见 [`R06-03-SELECTION-FOCUS-20260918.md`](design/reviews/ui-finesse-round3-20260915/evidence/R06-03-SELECTION-FOCUS-20260918.md)。保留游戏选框、滚动条、命令/Binding、取消/错误/恢复、安全语义、有限列表和 net462；未验真实 Playnite 输入/UIA/读屏、物理 DPI/跨屏、IME、presented frame、ETW、宿主性能；未写真实存档/媒体/云端或诊断数据。下一可执行任务为 R06-04 复制单元格与整行，先盘点现有复制命令、DTO/诊断字段和隔离剪贴板能力。

> 2026-09-18 第三轮 R06-02 排序提示与稳定性已满足当前可控范围：`c577afc5` 新增统一 `DataGridStableSortController`，接入 Save History/Candidates、Task Queue、Media Inbox；原始时间/数字/枚举/文本按 profile 排序，未知值升降序均末尾，同值使用稳定次键，真实列头 Sorting 与箭头状态共用切换路径。`R06SortingBehaviorTests 4/4`、R06-01 独立回归 `5/5`，Release XAML `24/24`、编译 `0/0`、源校验/diff check 通过。clean RenderHarness 绑定完整 SHA，Light/Dark 357 PNG、`WorkingTreeClean=True`、`render-qa OK`，Save/Task/Media 1040×700 图已抽查，报告覆盖 50/400/2000/4468 数据量、滚动和 2560×1440 ↔ 1100×720 resize。组合 WPF 测试的 7 项失败来自同一 AppDomain/Application、隐藏 Window/布局夹具环境假设，未写成绿色。证据见 [`R06-02-SORT-STABILITY-20260918.md`](design/reviews/ui-finesse-round3-20260915/evidence/R06-02-SORT-STABILITY-20260918.md)。未验真实 Playnite 多线程刷新/点击、UIA/读屏、物理 DPI/跨屏、OS 输入/IME、presented frame、ETW、宿主性能；Maintenance 排序 profile 未扩展；未写真实存档、媒体、云端或诊断数据。下一可执行任务为 R06-03 选中焦点区分，先盘点共享 selected、keyboard focus、hover、失焦和错误行资源。

> 2026-09-18 第三轮 R06-01 列宽用户记忆已满足当前可控范围：`75a6e6d8` 为 Save History/Candidates、Task Queue、Media Inbox 接入版本化 `v1/{view}/{column}` 列宽偏好，按视图区隔离有效 Pixel 宽度，忽略 Star/初始化/响应式默认值，四个页面均提供重置并保留最小列宽与现有横向滚动。`R06ColumnWidthPersistenceBehaviorTests 5/5`，clean Release XAML `24/24`、编译 `0/0`、源校验通过；R05 独立回归 Popup `1/1`、Tooltip `1/1`、边界 `2/2`、焦点 `3/3`、开关 `1/1`、选项虚拟化 `3/3`、源契约 `24/24`。clean RenderHarness 绑定完整 SHA，Light/Dark 357 PNG、`WorkingTreeClean=True`、`render-qa OK`，报告覆盖滚动、多数据量和 2560×1440 ↔ 1100×720 resize，Save/Task/Media `1040×700` 图已抽查。证据见 [`R06-01-COLUMN-WIDTH-PERSISTENCE-20260918.md`](design/reviews/ui-finesse-round3-20260915/evidence/R06-01-COLUMN-WIDTH-PERSISTENCE-20260918.md)。恢复测试是隔离设置对象上的控制器重建，不等价真实 Playnite 进程重启或用户拖拽；未验真实宿主、UIA/读屏、物理 DPI/跨屏、OS 输入/IME、presented frame、ETW、宿主性能；Maintenance 表格本批未接入列宽记忆；未写真实存档、媒体、云端或诊断数据。下一可执行任务为 R06-02 排序提示与稳定性，先核对当前排序实现、未知值规则和刷新路径。

> 2026-09-18 第三轮 R05-08 弹层资源热切换已满足当前可控范围：`599a8fd9` 补齐 `AdaptiveThemePalette` 到旧 Acrylic 资源键的活动 Demo 主题别名，并扩展局部 `GscToolTipBehavior` 的打开/主题同步与卸载清理；`bebde2fe` 稳固隔离 WPF 测试夹具。真实生产 Settings Popup 与 Tooltip 在 Light→Dark 后保持打开且背景切换，父窗 `Hide()` 后两层关闭；R05-08 `1/1`，相邻 Tooltip `1/1`、Popup `2/2`、焦点 `3/3`、开关 `1/1`、选项虚拟化 `3/3`、源契约 `24/24`。最终标准 Release XAML `24/24`、编译 `0/0`；clean RenderHarness 绑定 `bebde2fe`，Light/Dark 357 PNG、`WorkingTreeClean=True`、`render-qa OK`，Settings 开面及 Popup/Tooltip 双主题图已抽查。实现提交上的全量记录为 Core `83/83`、Worker `311/311`、Playnite `573/665`、57 跳过、35 条并行 WPF 环境性失败；不写成全量绿色。证据见 [`R05-08-POPUP-LIFECYCLE-20260918.md`](design/reviews/ui-finesse-round3-20260915/evidence/R05-08-POPUP-LIFECYCLE-20260918.md)。真实 Playnite 关闭/重建、泄漏 profiler、物理 DPI/跨屏、OS 输入/IME、读屏/UIA、presented frame、ETW、宿主性能仍未验；未写真实存档、媒体、云端或诊断数据。下一可执行任务为 R06-01 列宽用户记忆，先核对既有列布局和设置持久化能力。

> 2026-09-17 第三轮 R05-07 Tooltip 时序已满足当前可控范围：`2fc8c0d6` 修复实际生产 Tooltip 覆盖样式，使 `DesignTokens.xaml` 与 `AcrylicReferenceControls.xaml` 都明确不可聚焦、Mouse placement、420 DIP 上限及字符串换行/不省略；Shell 与独立 Settings 接入局部 Esc 关闭行为。真实生产 STA WPF `R05TooltipTimingBehaviorTests 1/1` 验证 350/100ms、长合成路径完整换行、Tooltip 不横向撑宽、Esc 关闭且原 TextBox 焦点不变；源契约 `24/24`，R05 Popup `2/2`、焦点 `3/3`、开关 `1/1` 回归通过。标准 Release XAML `24/24`、编译 `0/0`、Core `83/83`；Worker 全集为 `309/311`，两条既有健康状态断言在隔离环境实际返回 Warning。clean RenderHarness 绑定完整 SHA，Light/Dark 297 PNG、`WorkingTreeClean=True`、`render-qa OK`，Settings Popup/Tooltip 开面探针与 Settings/Overview 图已抽查。证据见 [`R05-07-TOOLTIP-TIMING-20260917.md`](design/reviews/ui-finesse-round3-20260915/evidence/R05-07-TOOLTIP-TIMING-20260917.md)。真实 Playnite 快速悬停/ShowDuration 消失、屏幕边缘翻转与遮挡、物理 DPI/跨屏、OS 输入/IME、读屏/UIA、presented frame、ETW、宿主性能仍未验；未写真实存档、媒体、云端或诊断数据。下一可执行任务为 R05-08 弹层资源热切换，继续先核对已有资源隔离能力。

> 2026-09-17 第三轮 R05-06 开关保存语义已满足当前可控范围：`cf9a250f` 修复 `GameSaveCenterSettings` 布尔属性在 `CopyFrom`/`CancelEdit`/导入时不通知 WPF 的绑定缺口，改为字段 + 去重 `SetBoolean`；Playnite 持久化协议、Worker live apply、命令绑定、取消/错误、安全、游戏选框和滚动条均保持。实际生产 `GameSaveCenterSettingsView`/STA WPF 行为 `1/1` 验证开关模型、`ToggleSwitch.IsChecked`、Track 视觉态、脏保存提示、隔离 `ExportPortableJson` 快照、CancelEdit 回滚以及媒体来源/巡检依赖面板关闭恢复；`SettingsSaveFeedbackTests 2/2`、草稿 `1/1`、Q 控件源契约 `24/24`。提交后 clean Release XAML `24/24`、构建 `0/0`、RenderHarness 双主题 297 PNG、`WorkingTreeClean=True`、`render-qa OK`，Settings 外观/自动化代表图已抽查。证据见 [`R05-06-TOGGLE-SAVE-20260917.md`](design/reviews/ui-finesse-round3-20260915/evidence/R05-06-TOGGLE-SAVE-20260917.md)。真实 Playnite 保存/Worker 失败注入、宿主保存取消关闭、OS 输入/IME/读屏/UIA、物理 DPI/跨屏、presented frame、ETW、宿主性能仍未验，未写真实存档/媒体/云端。下一可执行任务为 R05-07 Tooltip 时序，继续先核对现有 Tooltip/输入焦点能力。

> 2026-09-17 第三轮 R05-05 复选框三态记为不适用：核对当前生产后确认媒体批量选择是 Extended DataGrid + `SelectedItems`，没有“当前页/全部结果”的批量全选/半选 CheckBox；设置项/锁定 CheckBox 只承载标量布尔值，开发夹具三态不等价产品功能。共享 `GscCheckBox`/`GscDataGridCheckBox` 的 `IndeterminateMark` 已由 Q10-02 既有夹具记录 `mark=visible`，但不伪造真实批量集合。标准 Release 重新绑定当前文档 HEAD 后 XAML `24/24`、构建 `0/0`、`UiFinesseRound2ControlSourceTests 24/24`；此前失败是旧程序集与文档 HEAD 身份不一致。证据见 [`R05-05-CHECKBOX-THREESTATE-20260917.md`](design/reviews/ui-finesse-round3-20260915/evidence/R05-05-CHECKBOX-THREESTATE-20260917.md)。未来若新增当前页/全部结果复选框，需重新补 Space/UIA、筛选/分页/部分失败行为；当前未验真实 Playnite/OS 输入/读屏/UIA/物理 DPI/跨屏/presented frame/ETW/宿主性能，未写真实存档/媒体/云端。下一可执行任务为 R05-06 开关保存语义。

> 2026-09-17 第三轮 R05-04 多选摘要已满足当前媒体收件箱可控范围：`52900815` 复用已有 Extended DataGrid、按模式媒体 ID 选择集合、加载更多恢复和批量去重/无效项能力；摘要现在区分总选择数、当前窗口可操作数和暂不可见例外，增加“清空选择”且只清当前模式选择，不改变 `已忽略` 视图或媒体搜索/类型筛选。实际生产 `MediaCenterView`/STA WPF 行为 `3/3`，R05-01/02/03 回归合计 `11/11`；clean Release XAML `24/24`、构建 `0/0`；clean RenderHarness 双主题 357 PNG、`WorkingTreeClean=True`、`render-qa OK`，Media `1040×700` 双主题图已抽查。证据见 [`R05-04-MULTI-SELECTION-SUMMARY-20260917.md`](design/reviews/ui-finesse-round3-20260915/evidence/R05-04-MULTI-SELECTION-SUMMARY-20260917.md)。边界：跨窗口测试使用隔离保留 ID 夹具和生产 DataGrid 路由，不等价真实 Playnite 分页/后端删除竞态；未验真实 OS 输入/IME、读屏/UIA、物理 DPI/跨屏、presented frame、ETW、宿主性能；未写真实存档、媒体或云端。下一可执行任务为 R05-05 复选框三态。

> 2026-09-17 第三轮 R05-03 弹层边缘适配已满足当前可控范围：`cbfacd20` 复现并修复生产游戏选框固定 460/500 在短壳层越界的问题，为现有 `PickerPanel` 绑定 `PickerOverlay` ActualWidth/ActualHeight 最大约束；共享 ComboBox Popup 保留现有 Bottom placement、MaxDropDownHeight 和 Auto 滚动。R05 定向 `2/2`：720×360 短壳层面板不越界、列表可滚动且选定项可见；当前桌面右下边缘 Popup 实际翻转回工作区，最大高度、滚动条、选定项可见通过。clean Release XAML `24/24`、构建 `0/0`；R05-02 回归 `3/3`；clean RenderHarness 双主题、297 PNG、`WorkingTreeClean=True`、`render-qa OK`，Shell/Settings 图已抽查。420×220 极端壳层仅约 `92×92 DIP`，面板不越界但列表没有有效视口，留作最小尺寸/紧凑布局边界。证据见 [`R05-03-POPUP-EDGE-20260917.md`](design/reviews/ui-finesse-round3-20260915/evidence/R05-03-POPUP-EDGE-20260917.md)。Popup 几何不等价 presented frame 无闪屏；真实 Playnite/多屏/物理 DPI、OS 输入/IME、读屏、UIA、ETW、宿主性能仍未验；未写真实存档、媒体或云端。下一可执行任务为 R05-04 多选摘要。

> 2026-09-17 第三轮 R05-02 选项虚拟化焦点已满足当前可控范围：`7a4ede84` 先复现生产 Shell 游戏选框第一次 Down 会误关闭弹层，再为方向键、PageUp/PageDown、Home/End 增加短暂键盘导航保护；鼠标点击清除保护并保留点击选择提交。实际 2000 项 STA WPF 行为 `3/3`，活动项进入可见 DIP 视口，过滤隐藏选择可恢复，删除选中项回退首个有效项，既有 Auto 滚动/Recycling 虚拟化保留。clean Release XAML `24/24`、构建 `0/0`；artifact R05-02 `3/3`、R05-01 `3/3`、既有选框键盘 `6/6`；clean RenderHarness 双主题、297 PNG、`WorkingTreeClean=True`、`render-qa OK`，Shell/Settings/Task 图已抽查。证据见 [`R05-02-OPTION-VIRTUALIZATION-20260917.md`](design/reviews/ui-finesse-round3-20260915/evidence/R05-02-OPTION-VIRTUALIZATION-20260917.md)。真实 Playnite/OS 键盘和 IME、读屏、物理 DPI/跨屏、presented frame、ETW、宿主性能仍未验；离屏 `DpiScale=1.00` 只代表 logical DIP；未写真实存档、媒体或云端。下一可执行任务为 R05-03 弹层边缘适配。

> 2026-09-17 第三轮 R05-01 弹层焦点范围已满足当前可控范围：`00049995` 先复现生产 Shell 游戏选框第一次 Tab 落到遮挡层后的 `RadioButton`，再为 `PickerOverlay` 和 Dashboard `DialogOverlay` 补本地 focus scope、Tab 循环和方向导航边界；打开选框进入搜索框，关闭回焦 `GameContextButton`。共享 `ComboBoxItem` 设为非 Tab 停靠点，保留方向键/Enter/Escape 与滚动。Dashboard 关闭时恢复打开触发器。R05 定向 `3/3`，clean Release XAML `24/24`、构建 `0/0`，clean RenderHarness 双主题、297 PNG、`WorkingTreeClean=True`、`render-qa OK`，设置 normal/dirty 与 Overview 图已抽查。证据见 [`R05-01-FOCUS-BOUNDARY-20260917.md`](design/reviews/ui-finesse-round3-20260915/evidence/R05-01-FOCUS-BOUNDARY-20260917.md)。真实 Dashboard Playnite 宿主模态事件顺序、OS 输入/IME、读屏、物理 DPI/跨屏、presented frame、ETW、宿主性能仍未验；`DpiScale=1.00` 只表示离屏 logical DIP；未写真实存档、媒体或云端。下一可执行任务为 R05-02 选项虚拟化焦点。

> 2026-09-17 第三轮 R04-08 保存反馈闭环已满足当前可控设置范围：`c735905a` 复用 `ISettings.EndEdit`、现有 Worker live apply 和 `SettingsSaveHintText`，增加原子保存闸门及保存开始/应用开始/应用完成/保存失败事件；插件异步应用回调接回设置页。设置页稳定提示区分保存中、已写入 Playnite/正在应用 Worker、Worker 应用失败、本地保存失败、校验中、校验错误、脏草稿和已保存；重复 `EndEdit` 不并发，本地写入失败保留编辑克隆，Worker 失败不伪报已保存。当前字段均 live apply，没有仅重启生效项，未虚构重启态。相关选择集 `20/20`，clean Release XAML `24/24`、构建 `0/0`、源码校验/diff check 通过；RenderHarness 绑定完整 SHA、双主题、297 PNG、`WorkingTreeClean=True`、`render-qa OK`，Settings normal/dirty/invalid 已抽查。证据见 [`R04-08-SAVE-FEEDBACK-20260917.md`](design/reviews/ui-finesse-round3-20260915/evidence/R04-08-SAVE-FEEDBACK-20260917.md)。边界为合成设置/fake 服务/隔离目录/状态机/STA WPF/offscreen logical DIP；真实 Playnite 故障注入、保存/取消/关闭时序、宿主输入、IME/剪贴板、读屏、物理 DPI/跨屏、presented frame、ETW、宿主性能仍未验，未写真实存档、媒体或云端。下一可执行任务为 R05-01 弹层焦点范围。

> 2026-09-17 第三轮 R04-07 异步校验竞态已满足当前设置路径范围：`b1d5b68f` 新增不可变路径快照、后台路径规则服务和递增版本协调器；`GameSaveCenterSettingsView` 在最新字段版本上启动校验，旧请求晚返回、页面 DataContext 替换、提交/回滚或 Unloaded 后均不回写。完整 `VerifySettings` 保留，编辑期只在 UI 线程做轻量值校验；校验进行中/失败状态有明确提示。`df6f8083` 修复 RenderHarness 对验证摘要无参数反射入口的兼容问题。clean Release XAML `24/24`、构建 `0/0`、源码校验通过；R04-07 定向 `7/7`；RenderHarness 绑定完整 SHA、双主题、297 PNG、`WorkingTreeClean=True`、`render-qa OK`，Settings normal/dirty/invalid 已抽查。证据见 [`R04-07-ASYNC-VALIDATION-20260917.md`](design/reviews/ui-finesse-round3-20260915/evidence/R04-07-ASYNC-VALIDATION-20260917.md)。边界为合成设置/隔离目录/TaskCompletionSource/STA WPF/offscreen logical DIP；真实 Playnite 宿主输入/关闭时序、慢网络共享、IME/剪贴板、读屏、物理 DPI/跨屏、presented frame、ETW、宿主线程/帧率性能仍未验；不可中断的单次文件系统调用只取消结果应用，未写真实存档、媒体或云端。下一可执行任务为 R04-08 保存反馈闭环。

> 2026-09-17 第三轮 R04-06 清空与撤销已满足当前生产范围：`0d3f71af` 复用 Shell、Dashboard、Trainer、Task、Media 的搜索清空与游戏选框 Escape 路由；Dashboard 空状态“清除搜索”保留 `GamePicker.ClearSearchCommand`，补目标 TextBox Tag/Click/Automation Name，清空后恢复输入焦点并阻止事件冒泡。普通 TextBox 继续使用 WPF 标准 SelectAll/Undo/Redo 与粘贴链，撤销恢复选区，重做可用；Esc 只关闭选框并回焦上下文按钮，不触发危险命令。clean Release XAML `24/24`、构建 `0/0`、源码校验通过；R04-06 STA WPF 行为 `3/3`；RenderHarness 绑定完整 SHA、双主题、297 PNG、`WorkingTreeClean=True`、`render-qa OK`，Task/Shell `1040×700` 图已抽查。证据见 [`R04-06-CLEAR-UNDO-20260917.md`](design/reviews/ui-finesse-round3-20260915/evidence/R04-06-CLEAR-UNDO-20260917.md)。范围是合成搜索数据、隔离 STA WPF/offscreen logical DIP；真实 Playnite 系统输入/剪贴板/IME/读屏、物理 DPI/跨屏、presented frame、ETW、宿主性能仍未验，未写真实存档、媒体或云端。下一可执行任务为 R04-07 异步校验竞态。

> 2026-09-17 第三轮 R04-05 数字输入边界已满足当前生产字段范围：`8d56eb5` 复用共享 `GscNumericTextBox`/`IntegerRangeValidationRule` 和服务侧安全边界，拒绝空值、非整数、上下界外值与 `Int32` 溢出；Save 策略间隔/保留模板和 Trainer 启动延迟接入 `1–1440`、`0–2147483647`、`0–300` 规则。滚轮按同一绑定规则 ±1 步进，非法当前值/越界候选不改值、不静默 clamp，越界事件不吞以保留现有页面滚动；键盘/粘贴仍走原有 binding validation。没有新增当前不存在的重试/端口/容量字段，也没有覆盖 main 旧实现。相关定向 `20/20`，clean Release XAML `24/24`、构建 `0/0`、源码校验通过；RenderHarness 绑定完整 SHA、双主题、297 PNG、`WorkingTreeClean=True`、`render-qa OK`，Save `1040×700` 图已抽查。证据见 [`R04-05-NUMERIC-INPUT-20260917.md`](design/reviews/ui-finesse-round3-20260915/evidence/R04-05-NUMERIC-INPUT-20260917.md)。证据来自合成数据/fake 服务/隔离目录/STA WPF/offscreen logical DIP；真实 Playnite 系统输入、IME、读屏、物理 DPI/跨屏、presented frame、ETW、宿主性能仍未验，未写真实存档、媒体或云端。下一可执行任务为 R04-06 清空与撤销。

> 2026-09-17 第三轮 R04-04 粘贴标准化已满足当前生产输入范围：`d7e92f1` 先核对最新代码，复用设置页真实本地路径/云端目标和 MediaCenter 自定义媒体目录/文件模式；当前没有可编辑端口或独立 Exclude 字段。新增共享 `PasteNormalization` attached behavior，按 Path/RemoteTarget/Port/ExcludePattern 标识，单值粘贴去除外层空白/成对引号/尾随 shell 换行，剩余内部换行直接拒绝，字段与剪贴板不变；`SelectedText` 写入保留原生 Ctrl+Z，Tooltip/Automation HelpText 解释标准化前后。生产 XAML 接入设置 6 个路径、云端目标、媒体目录和文件模式。定向 `8/8`，clean Release XAML `24/24`、构建 `0/0`、源码校验通过；RenderHarness 绑定完整 SHA、`WorkingTreeClean=True`、双主题、297 PNG、`render-qa OK`，Settings/Media 1040×700 图已抽查。证据见 [`R04-04-PASTE-NORMALIZATION-20260917.md`](design/reviews/ui-finesse-round3-20260915/evidence/R04-04-PASTE-NORMALIZATION-20260917.md)。范围是合成数据、fake Worker、隔离目录、STA WPF/offscreen logical DIP；未验真实 Playnite 剪贴板/输入链、IME、屏幕阅读器、物理 DPI/跨屏、presented frame、ETW、宿主性能，也没有真实端口/排除字段可做宿主验证。未写真实存档、媒体或云端。下一可执行任务为 R04-05 数字输入边界。

> 2026-09-17 第三轮 R04-03 未保存离开保护已满足当前受控范围：`58e1734` 复用 Playnite `ISettings` 编辑克隆和设置指纹，明确 `CancelEdit`/`EndEdit` 的编辑缓冲生命周期；设置页对有效脏草稿挂接宿主 `Window.Closing`，复用现有原生消息框，“是”放弃并关闭，“否”取消关闭并恢复原分类/字段焦点，确认异常保守保留窗口/草稿。实际 STA WPF 分离/重挂测试和模型回滚合计 `12/12`，clean Release XAML `24/24`、构建 `0/0`、源码校验通过；RenderHarness 绑定完整 SHA，双主题、多尺寸、设置三态和既有滚动/虚拟化/Shell/resize `render-qa OK`、357 PNG，设置状态图已抽查。证据见 [`R04-03-DRAFT-LIFECYCLE-20260917.md`](design/reviews/ui-finesse-round3-20260915/evidence/R04-03-DRAFT-LIFECYCLE-20260917.md)。未自动操作真实 Playnite 原生关闭确认/UIA，真实宿主保存/取消/关闭时序、屏幕阅读器、物理 DPI/跨屏、presented frame、ETW、宿主性能仍未验；未写真实存档、媒体或云端。下一可执行任务为 R04-04 粘贴标准化。

> 2026-09-17 第三轮 R04-02 错误摘要导航已满足当前可控范围：`6524f94` 复用现有 `VerifySettings`、字段 Validation、页头摘要/详情、分类导航和 `SettingsScroller`，为路径/数量/外观/自动化字段建立目标映射；详情错误变为 Hyperlink，点击后选择对应 Tab、滚动到字段并聚焦，字段和链接的 Automation 文本包含具体原因。`HealthInspectionEnabled=false` 时隐藏巡检间隔/有效期不阻止保存校验。clean Release XAML `24/24`、构建 `0/0`、源码校验通过；R04-02 定向 `5/5`，含隔离目录模型负例和真实 STA WPF Window 跨 Tab/焦点/滚动行为。RenderHarness 绑定完整 SHA，`WorkingTreeClean=True`，双主题、多尺寸、滚动/虚拟化、Shell/resize `render-qa OK`、357 PNG，人工抽查 Settings 明暗图。证据见 [`R04-02-VALIDATION-NAVIGATION-20260917.md`](design/reviews/ui-finesse-round3-20260915/evidence/R04-02-VALIDATION-NAVIGATION-20260917.md)。同代码全量 Playnite testhost 两次复跑有 18/23 个既有 WPF 环境性失败，未计为通过；真实屏幕阅读器、Playnite 嵌入、物理 DPI/跨屏、OS 输入、presented frame、ETW、宿主性能仍未验。未写真实存档、媒体或云端。下一可执行任务为 R04-03 未保存离开保护。

> 2026-09-17 第三轮 R04-01 组合输入状态已满足当前受控范围：`40c7f9a` 复核并复用 R00-08 的可见生产游戏选框行为，为 `GameSearchTextBox` 接入 WPF `TextComposition` start/update/预览与冒泡 commit 路由；组合期间不让选择变化进入业务提交、不处理 Enter，commit 后恢复有效 `ItemsView` 候选确认、关闭弹层和焦点回返，卸载时清理状态。`GamePickerKeyboardBehaviorTests` `4/4`；clean Release XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `569/626`（57 跳过、0 失败），源码校验通过。首次未提交工作树全量出现 2 条一次性失败，复跑及 clean commit 全量均 0 失败，未放宽门禁。RenderHarness clean commit 双主题、多尺寸、滚动/虚拟化、Shell/resize `render-qa OK`、`WorkingTreeClean=True`、357 PNG，人工抽查 Task/Shell。证据见 [`R04-01-IME-COMPOSITION-20260917.md`](design/reviews/ui-finesse-round3-20260915/evidence/R04-01-IME-COMPOSITION-20260917.md)。范围为合成 DTO、隔离 STA WPF/offscreen logical DIP；真实 Windows IME 候选 UI、物理键盘/候选翻页、Playnite 嵌入输入链、屏幕阅读器、物理 DPI/跨屏、presented frame、ETW、宿主性能和连续输入/IME/debounce 基准仍未验；未改 picker、滚动条、命令/Binding、取消错误、恢复保护、有限列表和 net462，未写真实存档、媒体、云端。下一可执行任务为 R04-02 错误摘要导航。

> 2026-09-17 第三轮 R03-08 用户文本缩放已满足当前受控范围：`981b600` 复用既有 DynamicResource 字体链和尺寸 token，移除生产 TextBox、文本按钮、只读路径框与 Overview 保护按钮的硬高度；共享 DataGrid 表头改为 `GscTableHeaderHeight` 最小高度加 `Wrap + Trimming=None` 模板，页面重复 `ColumnHeaderHeight` 覆盖同步移除。`R03TextScaleTests` 在 `24/20 DIP` 字号覆盖的真实生产资源链 STA WPF 中 `2/2`，输入/按钮/表头均按自然需求测量且内容宿主不裁切；全量隔离 Release XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `568/625`（57 跳过、0 失败），源码校验通过。RenderHarness clean commit 双主题、多尺寸与滚动/虚拟化、Shell/resize `render-qa OK`、`WorkingTreeClean=True`、357 PNG，人工抽查 Task/Media/Save 代表图。证据见 [`R03-08-TEXT-SCALE-20260917.md`](design/reviews/ui-finesse-round3-20260915/evidence/R03-08-TEXT-SCALE-20260917.md)。范围为合成文本、受控 WPF/offscreen logical DIP；RenderHarness 默认字号不等价真实用户文本设置/宿主字体替换，真实 Playnite presented frame、物理 DPI/跨屏、OS 输入/IME、读屏、ETW、宿主性能仍未验；未改 picker、滚动条、命令/Binding、取消错误、恢复保护、有限列表和 net462，未写真实存档、媒体、云端。下一可执行任务为 R04-01 组合输入状态。

> 2026-09-17 第三轮 R03-07 文案标点统一已满足受控部分：`5a07eda` 盘点最新生产文案后，将 Task 状态/类型/范围/时间四个标签统一为全角冒号；失败详情和整库聚合统一生成“错误码：…；…”/“游戏：…；…”分隔。错误码、错误正文、用户名称和路径原值保持不变，空错误码不生成孤立标签；R03-05 的路径复制直传语义未改。`R03CopySafePunctuationTests` `3/3`；全量 Release XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `566/623`（57 跳过、0 失败）；RenderHarness clean commit 双主题 `render-qa OK`、`WorkingTreeClean=True`、357 PNG，人工抽查 Task `1040×700`/`1366×768` 明暗图。证据见 [`R03-07-COPY-SAFE-PUNCTUATION-20260917.md`](design/reviews/ui-finesse-round3-20260915/evidence/R03-07-COPY-SAFE-PUNCTUATION-20260917.md)。范围为合成 DTO、受控 WPF/offscreen logical DIP，不等价真实 Playnite presented frame、宿主字体替换、物理 DPI/跨屏、OS 输入/IME、读屏、ETW 或宿主性能；真实剪贴板未验，未写真实存档、媒体、云端。下一可执行任务为 R03-08 用户文本缩放。

> 2026-09-17 第三轮 R03-06 双语长度压力已满足受控部分：`52527b6` 复用现有标题省略/Tooltip 和长文本策略，修复共享动作文字模板的窄槽行为（`Wrap + Trimming=None`），新增英文长句、中文长游戏名和 820 DIP 窄宽度合成 profile。`R03BilingualLengthTests` `2/2`；clean commit 的 Light/Dark `overviewedges` 在 820/1040/1600 DIP 均 `surfaces=6/6`、标题完整 Tooltip 可达、英文长句可见、2 个当前游戏动作可测、无横向溢出；统一 `render-qa` 297 PNG、`WorkingTreeClean=True`、`render-qa OK`。全量 Release XAML `24/24`、构建 `0/0`、Core `83/83`、Playnite `563/620` 通过、57 跳过、0 失败；Worker 未改，沿用同分支最近完整基线 `311/311`。证据见 [`R03-06-BILINGUAL-LENGTH-20260917.md`](design/reviews/ui-finesse-round3-20260915/evidence/R03-06-BILINGUAL-LENGTH-20260917.md)。范围是合成数据、受控 WPF/offscreen logical DIP，不等价真实 Playnite presented frame、宿主字体替换、物理 DPI/跨屏、OS 输入/IME、读屏、ETW 或宿主性能；未写真实存档、媒体、云端。下一可执行任务为 R03-07 文案标点统一。

> 2026-09-17 第三轮 R03-04 数字列对齐已满足受控部分：`9fa68ef` 复用已有 Tabular 数字能力，新增共享数字/时间/百分比单元格样式并接入 Save、Media、Maintenance、Task；`TaskStatusDto` 保留原始 `ProgressPercent`，只新增显示层钳制与未知占位。实际 STA WPF 生产资源 120 DIP 列中 `9/10/99/100` 右边界均在 `119.5～120.5 DIP`，8 个进度边界用例通过，R03 定向 `10/10`。隔离 Release XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `558/615` 通过、57 跳过、0 失败；RenderHarness Light/Dark 1040×700 `render-qa OK`，人工抽查 Task/Save/Media。证据见 [`R03-04-NUMERIC-ALIGNMENT-20260917.md`](design/reviews/ui-finesse-round3-20260915/evidence/R03-04-NUMERIC-ALIGNMENT-20260917.md)。范围是合成数据、受控 WPF/offscreen logical DIP，不等价真实 Playnite presented frame、宿主字体替换、物理 DPI/跨屏、OS 输入/IME、读屏、ETW 或宿主性能；未写真实存档、媒体、云端，下一可执行任务为 R03-05 长路径分层。

> 2026-09-17 第三轮 R03-05 长路径分层已满足受控部分：`2b8612f` 先核对现有 `GscPathText` 与复制重试，再新增中间省略预览、共享只读可选择完整路径框和 `CopyPathCommand`；预览与复制 payload 分离。Save 候选、Media 待归类/已归档详情、Trainer 选中版本都保留盘符/文件名重点并提供完整复制。`R03LongPathTests` `3/3` 实际验证超过 260 字符在 340 DIP 内不撑宽，选择文本与原始路径完全一致；隔离 Release XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `561/618` 通过、57 跳过、0 失败，源码校验通过。RenderHarness 绑定该提交且 `WorkingTreeClean=True`，Light/Dark 多尺寸 `render-qa OK`，人工查看 Save/Media/Trainer 1366×768 详情。证据见 [`R03-05-LONG-PATH-20260917.md`](design/reviews/ui-finesse-round3-20260915/evidence/R03-05-LONG-PATH-20260917.md)。范围是合成路径、受控 WPF/offscreen logical DIP；未验真实 Playnite presented frame、OS 剪贴板/键盘/IME、读屏、宿主字体、物理 DPI/跨屏、ETW 或宿主性能；Maintenance 摘要路径仍是 Tooltip/只读摘要，未宣称独立复制详情。下一可执行任务为 R03-06 双语长度压力。

> 2026-09-17 第三轮 R03-03 双语混排基线已满足受控部分：`466c2f5` 核对现有 Save Center/存档中心、日期容量与中文标点资源后，没有引入新字体体系或生产行级偏移；新增 WPF `TextFormatter.GetIndexedGlyphRuns()` 混排基线证据与行为门禁。Light/Dark 四组样本均有实际 glyph、baseline spread `0`、`stable=True`；`TypographyDiagnosticsTests` `11/11`。隔离 Release XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `548/605`（57 跳过、0 失败），源码校验通过；RenderHarness 双主题 `finesse-fixture OK`，生产 1040×700 双主题 `render-qa OK`，人工抽查 Overview/Media 图。证据来自合成文本、受控 WPF/offscreen logical DIP，不等价真实 Playnite presented frame、宿主字体替换、物理 DPI/跨屏、OS 输入/IME、读屏、ETW 或宿主性能；未写真实存档、媒体或云端。下一可执行任务为 R03-04 数字列对齐。

> 2026-09-17 第三轮 R03-02 阅读层级校准已满足受控部分：`e889b04` 复用现有 `Typography.xaml`/`Redesign.xaml`，将生产 Views/Settings 中 5 处 12.5、9 处 10.5、2 处 9.5 微字号归入正文 14 或 Caption 12；技术路径保持 Code/Path 及复制、Tooltip 语义。`ProductionTypographyUsesSharedHierarchyWithoutMicroSizeDrift` 最终定向 `10/10`；隔离 Release XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `547/604`（57 跳过、0 失败），源码校验通过。Light/Dark 1040×700 生产窄窗 `render-qa OK`，人工查看 Overview、Media、Save、Maintenance 代表图无字号挤压。证据来自受控 WPF/offscreen logical DIP，不等价真实 Playnite presented frame、宿主字体替换、物理 DPI/跨屏、OS 输入/IME、读屏、ETW 或宿主性能；未写真实存档、媒体或云端。下一可执行任务为 R03-03 双语基线。

> 2026-09-17 第三轮 R03-01 真实落字证据已满足受控部分：`855e727` 在既有字体链上用 WPF `TextFormatter.GetIndexedGlyphRuns()` 逐样本区分 `FontCandidate` 与最终 `GlyphRunEvidence`。中文、英文、数字、`𠮷`、组合重音在 Light/Dark clean `finesseprobe` 均为 `GlyphRunCaptured`；`𠮷` 候选 unresolved 但最终 Typeface 为 `MingLiU-ExtB`。实际 STA 定向 `1/1`，`TypographyDiagnosticsTests` 类 `9/9`；最终隔离 Release XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `546/603`（57 跳过、0 失败），源码校验通过。证据范围是生产字体资源、受控 WPF/offscreen logical DIP，不等价真实 Playnite 最终 frame、宿主字体替换、物理 DPI、OS 输入/IME、读屏、presented frame、ETW 或宿主性能；未改生产 XAML/命令/绑定，未写真实数据。下一可执行任务为 R03-02 阅读层级校准。

> 2026-09-17 第三轮 R02-08 动作文案动词化已满足：`43ea843` 按真实命令绑定统一“重新加载详情”“重新校验”“下载到隔离区并校验”“快照并恢复”“校验远端内容”“重试上传”，并清理生产 DTO、Playnite、Worker 失败/状态消息中的英文 `远端 check` 混用；协议内部 `Check*` 状态名和 rclone 参数未改。`R02ActionCopyTests` `4/4`，相关可用性/焦点定向合计 `13/13`；最终干净隔离 Release 为 XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `545/602`（57 跳过、0 失败），源码校验通过。RenderHarness 绑定该提交，Light/Dark 全场景 `render-qa OK`，人工查看 Save/CloudQueue 1040×700 双主题代表图。本阶段未改命令、Binding、取消/错误、安全流程，未启动真实 Playnite 或执行外部云端/备份操作；真实宿主字体/输入、物理 DPI、IME、读屏、presented frame、ETW 和宿主性能仍未验。下一可执行任务为 R03-01 真实落字证据。

> 2026-09-17 第三轮 R02-07 异步菜单上下文已满足插件可控部分：`4d4bb93` 让 `GameMenuItem` 生成时固定目标 Guid，五个目标相关异步 Action 在 Worker/Upsert 前按当前 Playnite 数据库重解析；列表刷新使用新对象并保持原顺序，目标删除/无法确认时明确提示并中止，不改用当前选中行。`R02MenuActionContextTests` + `R02MenuHostContractTests` 定向 `4/4`；最终干净隔离 Release 为 XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `541/598`（57 跳过、0 失败），源码校验通过。首轮发现的过期源码断言已独立校准并纳入该干净提交。本阶段没有生产 XAML/样式改动，未启动真实 Playnite、未执行菜单 Action、未写真实数据；宿主菜单打开后的刷新/删除事件时序、菜单视觉/输入、物理 DPI、IME、读屏、presented frame、ETW 和宿主性能仍未验。下一可执行任务为 R02-08 动作文案动词化。

> 2026-09-17 第三轮 R02-06 菜单状态完整已记录为外部宿主阻塞：当前生产没有 WPF `ContextMenu/MenuItem`，只有 Playnite `GameMenuItem` 入口；`8e48ac8` 新增实际插件契约测试，空选择为 0 项，合成多选按固定顺序生成 6 个 `GameSaveCenter` 分组动作，定向 `2/2`，不执行 Action。最终隔离 Release 为 XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `539/596`（57 跳过、0 失败），源码校验通过。禁用/勾选/危险/子菜单/快捷键列、键盘上下左右/Enter/Esc、边缘翻转、点外关闭和焦点返回由 Playnite 宿主控制，当前工作区没有可测菜单树，不能宣称完成；未启动真实宿主、未写真实数据。下一可执行任务为 R02-07 异步菜单上下文。

> 2026-09-17 第三轮 R02-05 已满足：核对确认当前生产共享图标按钮链已经提供 `34×34 DIP` 紧凑命中区、6 DIP 邻间距和 36 DIP 工具条变体；`f03b4dd` 只新增真实 STA WPF 命中/布局行为门禁。复制/删除中心分别命中各自 Button，间隔不命中；窄 `82 DIP` WrapPanel 下复制/删除同行、第三动作换行且无正面积重叠，定向 `2/2`。最终隔离 Release 为 XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `537/594`（57 跳过、0 失败），源码校验通过；双主题 RenderHarness 56 场景均 `OK`，人工对照 Media 1040×700 双主题图。证据来自真实生产 WPF/合成图标/隔离 Window/offscreen logical DIP；未验真实 Playnite、物理 DPI、读屏、OS 输入/IME、presented frame、ETW/宿主性能，未写真实存档、媒体或云端。下一可执行任务为 R02-06 菜单状态完整。

> 2026-09-17 第三轮 R02-04 已满足：核对确认生产共享文字模板、统一 Center 对齐和复合图标内容路径已存在，`d296ce0` 只补真实 STA WPF 几何门禁。同样式中文四字/英文双词基线差 `<0.5 DIP`，16/20 DIP 图标与数字间距 `8 DIP`、中心差 `≤1.5 DIP`，定向 `3/3`；最终隔离 Release 为 XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `535/592`（57 跳过、0 失败），双主题 RenderHarness 56 场景均 `OK`。首次全量的一条既有 IPC 取消时序用例失败，单项 `1/1` 复跑后完整脚本通过，事实已记录。证据来自生产 WPF/合成 Geometry/offscreen logical DIP；未验真实 Playnite、物理 DPI、读屏、OS 输入/IME、presented frame、ETW、宿主性能。下一可执行任务为 R02-05 命中区与间距。

> 2026-09-16 第三轮 R02-03 已满足：`627f864` 在既有命令门禁基础上增加 Restore、Media Inbox、Cloud Transfer、Remote Restore 的相邻禁用原因说明；说明可聚焦，Automation Name/HelpText 与文案一致，需要维护时复用 `OpenMaintenanceCommand`，未放宽危险/未校验恢复命令。R02 定向 `4/4`，隔离 Release 为 XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `532/589`（57 跳过、0 失败）；双主题 RenderHarness 56 个视图/尺寸场景均 `OK`，源码校验通过。证据来自合成 fake、真实生产 WPF/offscreen logical DIP；未验真实 Playnite/屏幕阅读器/物理 DPI/OS 输入/IME/presented frame/ETW/宿主性能，未写真实存档/媒体/云端。下一可执行任务为 R02-04 图文光学居中。

> 2026-09-16 第三轮 R02-02 已满足：`473cf3a` 在已有 `4947539` 忙态模板基础上，将共享 `GscWpfUiButton` 的 `IsBusy` 接到 Acrylic 壳层/工作区页面的真实 `DataContext.IsBusy`，并以 `BusyOperationCoordinator` 为 `DashboardViewModel.RunAsync` 增加原子防重入；原 Worker、取消/错误、Trainer 清理和排队刷新语义保留。R02 定向 `3/3`，隔离 Release 为 XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `528/585`（57 跳过、0 失败）；Light/Dark 忙态探针均 `180→180`、文本/焦点稳定、indeterminate 指示器可见。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R02-02-BUSY-WIDTH-20260916.md`。证据来自 fake 生命周期、真实生产 WPF/offscreen logical DIP，不等价真实 Playnite/Worker 时序、物理 DPI、OS 输入/IME、presented frame、ETW 或宿主性能；未写真实存档/媒体/云端。下一可执行任务为 R02-03 禁用原因可达。

> 2026-09-16 第三轮 R02-01 已满足：`89c9cc3` 在当前 `codex/ui-finesse-round2` 接入共享 DangerAction/ContextDanger 变体，收口 Overview 三个动作区域的单一 Primary 角色；Save 恢复和 Media 来源移除明确使用危险语义；Media Inbox 待归类/已忽略批量主动作按真实 DataTrigger 互斥。Release 为 XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `525/582`（57 跳过、0 失败），R02 定向 `2/2`；RenderHarness 为 161 快照、0 Fidelity、0 失败路由、0 HIGH/0 MEDIUM。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R02-01-ACTION-PRIORITY-20260916.md`。证据是受控 WPF/offscreen logical DIP，不等价真实 Playnite、用户主题、物理 DPI/IME、presented frame、ETW 或宿主性能；未改 picker、滚动条、命令绑定、取消/错误、恢复保护、有限列表和 net462。下一可执行任务为 R02-02 忙碌宽度稳定。

> 2026-09-16 第三轮 R01-08 已满足：隔离 Release 为 Core 83/83/0、Worker 311/311/0、Playnite 523/580 通过、57 跳过、0 失败；XAML 24/24、构建 0/0。Playnite 计划中的 63 条是 57 条撤销 UI 基线 + 6 条 NamedPipe gated，本机 Named Pipe 可用所以 6 条 IPC/取消/恢复实际通过；Worker 重启 gated 1 条也通过。成功/失败/跳过、补测步骤和真实宿主边界已写入 R01-08 证据，不把能力限制改成通过；下一可执行任务为 R02-01 动作优先级。

> 2026-09-16 第三轮 R01-07 已满足：e1324fe 新增 freshness 脚本、三分支 smoke 和版本化 UI_EVIDENCE_BASELINE.json；当前源码扫描 14 条记录为 7 stale / 7 fresh，明确列出需重跑的旧证据及关联 scopes。纯文档变更为 0 重跑/0 重装；共享 Redesign.xaml 变更命中 R00-01-02/R00-05 的 shared-controls/all-pages；源码与包身份分开输出，当前真实包身份为 not-provided，合成 mismatch 只验证重装分支。源码校验和 smoke 已通过，证据见 design/reviews/ui-finesse-round3-20260915/evidence/R01-07-EVIDENCE-FRESHNESS-20260916.md；下一可执行任务为 R01-08 跳过测试说明。

> 2026-09-16 第三轮 R01-06 已满足：受控 RenderHarness 审计绑定 3929ed7，构建 0/0，运行时 161 快照、0 Fidelity、0 失败路由、0 HIGH/0 MEDIUM；关键 manifest、summary、metadata、路由/交互矩阵、布局报告、20 行具体索引和 6 张精选图已归档到 design/reviews/ui-finesse-round3-20260915/evidence/R01-06-host-evidence-20260916/。完整图集不入 Git，由归档 README 在固定 commit 上重现；metadata 不再只依赖绝对临时路径。归档前人工查看壳层、首页、维护诊断代表图。范围是受控 WPF 离屏 logical DIP，不等价真实 Playnite 嵌入、用户主题、物理 DPI、OS 输入/IME、presented frame、ETW 或宿主性能；下一可执行任务为 R01-07 基线失效规则。

> 2026-09-16 第三轮 R01-05 已满足：`5e6d64a` 新增测试侧负例注册表，N01～N05 分别覆盖对比度、裁切、焦点、层级、状态；实际检测结果均 `detected=True`，没有把夹具放进生产入口。完整 Release 构建 `0/0`，Core `83/83`、Worker `311/311`、Playnite `523/580`（57 跳过、0 失败），注册表定向 `1/1`，源码校验通过。代码已推送 `origin/codex/ui-finesse-round2`。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R01-05-NEGATIVE-REGISTRY-20260916.md`。范围是合成数据、受控 WPF Window 和 offscreen logical DIP，不等价真实 Playnite、OS 输入/IME、物理 DPI、presented frame、ETW 或宿主性能；下一可执行任务为 R01-06 宿主证据保全。

> 2026-09-16 第三轮 R01-04 已满足受控行为门禁：`74abb10` 新增真实 STA WPF 重入后清钟基值测试，并将动效源码断言收窄为结构约束；Y/Opacity 基值写回的两次隔离突变均按预期使行为测试失败。以完整提交身份构建时 XAML `24/24`、构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `522/579`（57 跳过、0 失败），`EntranceMotion` 定向 `4/4`。代码已推送 `origin/codex/ui-finesse-round2`。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R01-04-MOTION-BEHAVIOR-20260916.md`。范围是合成 WPF/隔离 Window/offscreen logical DIP，不等价真实 Playnite、物理 DPI、用户输入、presented frame、ETW 或宿主性能；下一可执行任务为 R01-05 负例注册表。

> 2026-09-16 第三轮 R01-01 已收口：`abb5589` 为 Playnite 源码型测试嵌入 `GscSourceRoot`/`GscBuildCommit`，统一 37 个重复 root helper 与 6 个直接 reader；`TestRepositoryContext` 校验源码根、Git HEAD 与程序集身份，错根直接失败。当前 worktree 的隔离 OutputRoot 全流程构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `518` 通过/`57` 跳过/`0` 失败；输出移到 main `.tmp` 复跑相同，main 源码和 `src.zip` 未改，临时目录已清理。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R01-01-REPOSITORY-IDENTITY-20260916.md`。下一可执行任务为 R01-02 数字单元格裁切；真实 Playnite 宿主、物理 DPI、IME、presented frame 和性能仍未验。

> 2026-09-16 第三轮 R01-03 已收口代码：`3875f88` 接入共享审计报告 `EVIDENCE_INDEX.md`，`2eb4c46`/`591deae` 校准为生产 DataGrid 优先、跨页面控件分散抽样并补源码文件回退，`f6a3209` 增加逐项完整性校验器，`9d5146d` 校准 Windows PowerShell BOM、源码身份参数和当前 `ButtonChrome=0.72` 源码守卫。最新完整受控审计绑定 `9d5146d4a4d604b2779f933f3dee00e730c68b71`，20 项索引为 13 个生产 DataGrid + 7 个跨页面控件；结果/源码入口/40 位代码身份/样本/未验边界均 `20/20`，审计 Fidelity `0`、失败路由 `0`。隔离 Release 构建 `0/0`，Core `83/83`、Worker `311/311`、Playnite `521` 通过/`57` 跳过/`0` 失败，源码契约测试 `6/6`，源码校验通过。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R01-03-EVIDENCE-INDEX-20260916.md`。范围为静态 manifest、运行时离屏 WPF 和逻辑 DIP；静态-only 不等价实际呈现，真实 Playnite 嵌入、物理 DPI、IME、presented frame、ETW 与宿主性能仍未验。下一可执行任务为 R01-04 动效行为替代字符串。

> 2026-09-16 第三轮 R01-02 已收口代码：`acfe1ea` 新增真实 WPF 数字单元格横/纵可读性检测，校对夹具数值列由 `110 DIP` 调整为 `160 DIP`，覆盖 `1,024 / 99,999`、长负数、容量与 TiB；旧基线图像曾显示首行末位裁切，原门禁没有水平完整性检查。实际 STA WPF 定向测试 `2/2`，双主题 clean-tree `finesseprobe` 均为 `expected=4 realized=4 horizontalFit=4 verticalFit=4 allReadable=True`，窄列长负数“行高通过但列宽失败”负例通过；完整 Release 为构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `520` 通过/`57` 跳过/`0` 失败。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R01-02-NUMERIC-CELL-READABILITY-20260916.md`。范围是合成 DTO、受控 WPF Window 和 offscreen logical DIP，不等价真实 Playnite 嵌入、物理 DPI、IME、presented frame、ETW 或宿主性能；下一可执行任务为 R01-03 每项证据直达。

> 2026-09-16 第三轮 R00-08 已收口代码：`8435d80` 修正游戏选框 Enter/IME 路由，Enter 只确认仍在 `ItemsView` 的当前可见候选，无结果不确认旧游戏，`Key.ImeProcessed` 和方向键不关闭，Escape/有效 Enter 返回 `GameContextButton` 焦点。当前分支 `codex/ui-finesse-round2` 已完成 Release Playnite `0/0` 和相关 STA WPF 测试 `28/28`，文档证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R00-08-PICKER-ENTER-IME-20260916.md`。测试使用合成 DTO、隔离 Window 和最小 Dashboard 状态承载；真实 Windows OS IME、Playnite 嵌入、物理键盘/DPI、presented frame、ETW 与宿主性能仍未验。下一可执行任务为 R01-01 测试源码根绑定。

> 2026-09-16 第三轮 R00-07 已收口：`42f9dca` 将工具栏审计从 `TrainerToolsSettingsScrollViewer` 整棵排除改为动作/表单/内容流分类，报告保留排除理由与几何/滚动可达数据；当前分支 `codex/ui-finesse-round2` clean-tree，已推送。RenderHarness 构建 `0/0`、审计源/既有精修定向 `29/29`；`toolbarprobe` 三场景通过；全量审计 161 快照、0 Fidelity、0 失败路由、0 HIGH/0 MEDIUM。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R00-07-TOOLBAR-EXCLUSION-20260916.md`。

> R00-07 校准边界：首轮 `Rect.Empty.Width=-∞` 误报已在同阶段修正，最终横向溢出只按有效需求宽度与可用宽度判断；隐藏生产详情父级不再产生动作栏 HIGH。探针使用合成 WPF 面板和 offscreen logical DIP，不能替代真实 Playnite 嵌入、物理 DPI、用户输入/滚轮、ETW、presented frame 或宿主帧率。生产命令、绑定、滚动、安全语义和 net462 未改；下一可执行任务为 R00-08 搜索框 Enter/IME。

> 2026-09-16 第三轮 R00-06 已收口：`7d57575` 将媒体表格 `212 DIP` 固定门禁改为运行时实际几何公式与行交集判定，`db5d483` 补齐几何探针元数据；当前分支 `codex/ui-finesse-round2` clean-tree SHA 为 `db5d483d8ac6460ac7c3a07fe64cec5c7fe417d3`，已推送。Playnite 定向构建 `0/0`、R00-06 定向 `4/4`、RenderHarness 构建 `0/0`；双主题几何探针 10/10、完整审计 161 快照且 0 Fidelity/0 失败路由/0 HIGH/0 MEDIUM、Shell QA exit 0。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R00-06-MEDIA-FOUR-ROWS-20260916.md`。

> 交接边界：证据来自合成媒体数据、真实生产 WPF 视图、隔离输出目录和 offscreen logical DIP；短窗页级回退与父级裁剪 HIGH 负例已覆盖，但真实 Playnite 嵌入 Dashboard、用户主题/物理 DPI、鼠标滚轮与键盘、presented frame、ETW 和宿主帧率仍未验。保留现有游戏选框、滚动条系统、命令/绑定、取消/错误/恢复保护和 net462 兼容。临时 `.tmp/r00-06-*` 输出在文档提交后清理；下一可执行任务为 R00-07 审计排除项收窄。

> 2026-09-16 第三轮 R00-04 已收口：`df884b0` 修正搜索基准为 30 个不同查询，并等待真实可见 ID 集合；不可能结果的独立超时负例通过。当前隔离 worktree Release 构建 `0/0`、定向测试 `2/2`，合成 2,000 项集合变化 `30/30`，p50/p95/max=`45/60/60ms`。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R00-04-SEARCH-BENCHMARK-20260916.md`。受控 fake/Dispatcher 证据不等价真实 Playnite 连续输入、IME、物理 DPI、屏幕帧或 ETW；下一可执行任务为 R00-05，R18-01 继续补连续输入/debounce 分配边界。

> 2026-09-16 第三轮 R00-05 已收口：`aebcefc` 删除 ContextButton 外层 `0.48`，统一复用共享模板 `ButtonChrome=0.72`；Light/Dark 的 Context、RemoteRestore、MediaBatch 真实 WPF 派生样式测试 `2/2`，启用/禁用高度差 `<0.01 DIP`，复合标签/图标/解释文字非透明，最低受控对比度 `3.0`。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R00-05-CONTEXT-DISABLED-20260916.md`。这是受控 Window/逻辑合成，不等价真实 Playnite/物理 DPI/屏幕像素；下一可执行任务为 R00-06 媒体四行门禁。

> 2026-09-16 第三轮 R00-01/R00-02 已在 `codex/ui-finesse-round2` 收口：`a95e900` 修正整组 chrome opacity 对比度合成并复用组合 ScaleTransform，`e216e9b` 增加非等距 stop 负例。当前 clean-tree `e216e9b` 定向 WPF `5/5`，双主题 RenderHarness `88` 状态样本均无 violation；证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R00-01-02-CONTRAST-SCALE-20260916.md`。这是受控 offscreen logical DIP 结果，不等价真实 Playnite/IME/物理 DPI/屏幕帧；下一项执行 R00-03 的完成、取消、卸载和重入 Dispatcher 复核，R08-08 可变共享 Freezable 仍待单独验证。

> 2026-09-16 第三轮 R00-03 已收口：`4414f05` 的现有生产终态实现由 `cda168c` 补齐真实 Dispatcher 行为测试；当前 clean-tree `cda168ca410bee0a4b0416664452010c9246db96` 的完成/重入/取消/卸载定向测试 `5/5`，`motionreentryprobe` 与 `motionhotprobe` 双主题均通过。证据见 `design/reviews/ui-finesse-round3-20260915/evidence/R00-03-MOTION-LIFECYCLE-20260916.md`。受控窗口仍不等价真实 Playnite/Windows 偏好通知/ETW/物理呈现，下一项为 R00-04 搜索基准真实性。

> 2026-09-14 最新精修入口：[第一轮独立复核](design/UI_FINESSE_REVIEW_2026-09-13.md) 与 [第二轮 208 项](design/UI_FINESSE_ROUND2_208_TASKS_2026-09-13.md)。旧 52 项不是全部完成（14 已验收、1 已满足、31 待验收、6 阻塞，且有需重开项）；暗色夹具黑字仍报告 OK、状态小字对比与字体/完整行证据缺口优先进入 Q00。新任务先读复核，再从 208 行分维度账本持续实现，不沿用“待开始为零=全部完成”的口径。本轮独立构建测试零失败，未修改生产 UI 或安装宿主；新 Luna/max 任务使用干净 worktree，主目录 src.zip 保留。

> 2026-09-13 UI 精修账本收口续跑：本轮代码基线为 `6c3c238`，源码门禁修正为 `592d7a0`，最终 Render QA 报告对应提交 `cacdaff`；已完成 P03、P05～P09 的共享实现/受控证据复核，并完成 P06-03、P06-04、P08-02、P09-02、P11-02、P11-04 的受控签收；最终 RenderHarness、状态/网格/缩略图/壳层探针均为 `OK`。P08-03、P10-01/02/04、P11-03 仍外部阻塞，其他依赖真实宿主的项为代码完成待验收。最终一键链与真实宿主审计已在文档提交后重试，均因唯一的 `?? src.zip` 在安装/采集前按 clean-tree 安全策略停止；构建与测试仍通过（Core `76/76`、Worker `311/311`、Playnite `455/512`，57 skip）。真实宿主没有 `summary.json`，专用窗口不能替代嵌入 Dashboard；根目录用户未跟踪 `src.zip` 不能擅自处理。详细证据见 [`docs/design/reviews/ui-finesse-20260913/PROGRESS.md`](design/reviews/ui-finesse-20260913/PROGRESS.md) 和 `BASELINE.md`。

> 2026-09-13 UI 精修规划入口：[12 阶段、52 项实施提示词](design/UI_FINESSE_IMPLEMENTATION_PROMPTS_2026-09-13.md)。按用户最新关注点覆盖中英文排版、色彩材质、共享控件、动画中断/终态、响应性能、逐页精修、DPI/键盘和真实宿主验收。本轮仅文档交付，52 项未开始；建议从 P00 建立基线，复用已完成 U12 工作。源码新复核确认 MotionTokens 220/300ms 与 GscMotion 200/320ms 尚有差异，Caption Muted 色叠加 0.65 Opacity 需测量，不能继续泛称动效完全统一或小字对比已全部达标。详细事实见 CURRENT_STATE，实施与签收使用新提示词包。

> 2026-09-13 开发提示词包全量门禁复核：用户提供的 Phase 0～12 及最终验收要求已逐项对照当前生产实现；最新 RenderHarness `render-qa OK`（双主题、多尺寸、状态夹具、滚动/缩放恢复、性能探针），静态审查 0 error。报告 `WorkingTreeClean=False` 仅由现有未跟踪根目录 `src.zip` 导致，未纳入提交。Playnite 已完成最新包安装并确认加载 `GameSaveCenter 0.6.73`；真实宿主审计仍因 `MainWindowHandle=0` 未捕获嵌入 Dashboard，用户主题/DPI/物理键盘滚轮和大库帧率仍待可见宿主会话。

> 2026-09-13 GPT UI 契约与发布包复核：在不改 MVVM/业务层的前提下，补齐现有共享 DesignTokens/Typography/Redesign/ButtonStyles 的间距、字体、玻璃、按钮语义别名；统一表格为 52 DIP 行、42 DIP 表头，维护页最小表格 260 DIP，任务/存档/维护状态胶囊接入 `StatusGlyphConverter` 的 `✓/×/⚠` 显示，Motion Normal/Slow 校准为 220/300ms。验证为 Release 0 warning/error、Core `76/76`、Worker `310/311`（1 skip）、Playnite `447/510`（63 skip）、RenderHarness `render-qa OK`；干净 HEAD `b470bf9` 已重新生成 `.pext/.zip`，六份程序集身份统一为 `0.6.73+b470bf97f53ec0012da165cb4eb954d6b2e18e6f`，包 SHA-256 为 `9E6F4BB11B812DB824D8E3EA4EBFCD45A2490FEA5C907996301ABD9F21A55F8A`。任务页 236 DIP 紧凑视口是有意保留的页面级契约。真实 Playnite 嵌入 Dashboard、宿主 DPI/主题、物理键盘/滚轮仍以紧随其后的真实宿主审计边界为准；当前运行中的无窗口宿主需先关闭后才能安全安装最新包。

> 2026-09-13 真实宿主审计复跑：在 HEAD `d59a6a6` 运行 `scripts/real-host-audit.ps1`，Release 构建 `0 warning / 0 error`，Core `76/76`、Worker `311/311`、Playnite `444 passed / 57 skipped / 0 failed`；当前用户 Playnite 扩展目录已安装并加载 `GameSaveCenter 0.6.73`。审计目录为 [`artifacts/ui-host-audit-live-20260913`](../artifacts/ui-host-audit-live-20260913)，受控矩阵覆盖 1366×768、1600×1000、最大化和浅/深主题，metadata 记录 150% DPI 且 `RealFixedLayoutOverflow=[]`。UIAutomation 未找到 GameSaveCenter 侧栏，宿主 `MainWindowHandle=0`，未生成 `summary.json`；因此截图均为 `DedicatedAuditWindow`，不能替代真实嵌入 Dashboard、物理滚轮/键盘和宿主主题验收。

> 2026-09-12 UI 排期入口：后续 UI 工作优先读取 [`ai/UI_CONSOLIDATED_BACKLOG_2026-09-12.md`](ai/UI_CONSOLIDATED_BACKLOG_2026-09-12.md)。它已将项目 D12 显示复核任务与用户提供的 GPT 建议整合为 U12-00～U12-10，并明确已有共享 UI 系统仅审计/补齐、Demo-first 和真实业务边界；旧 32 项计划仍用于稳定性、发布与宿主验收依赖。

> 2026-09-10 实际宿主安装核对：用户反馈按钮无变化后，比较发现 Playnite 扩展目录的旧 DLL 与源码哈希不同，且目录内 `icon.png` 仍是旧资产。已用 `scripts/dev-install-run.ps1 -Configuration Release -NoStart` 从 HEAD `c757ffe` 重新构建并原子替换 `C:\Users\lopmatu\AppData\Roaming\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`；安装后插件/Worker/共享程序集/清单/icon 与包 staging 逐项哈希一致，Playnite `440/497`（57 skip，0 fail）。当前没有运行 Playnite，用户需要完全启动/重启后才能看到新按钮和侧栏图标。

> 2026-09-10 按钮与插件侧栏图标实际接入：确认上一轮只增加语义别名、没有影响现有页面；本轮在共享 `GscWpfUiButton` 基础模板启用 `GscGlassFillBrush` / `GscGlassStrokeBrush`，所以既有 `GscWpfUi...` 页面按钮会直接获得玻璃表面，Primary/Danger 继续跟随主题效果资源。`SidebarItem.Icon` 改为 ZIP 中 `plugin-main.svg` 的 WPF Path，并将 Stroke 绑定 Playnite `GlyphBrush`，由宿主主题自动切换黑/白；`icon.png` 已替换为 `plugin-main-256.png` 透明线稿。代码提交 `83dc1c4`；全量构建/测试无失败，干净离屏报告 [`.tmp/icon-button-qa-clean-20260910/render-qa-report.txt`](../.tmp/icon-button-qa-clean-20260910/render-qa-report.txt) 为 `render-qa OK`。真实 Playnite/FusionX、DPI、用户主题和侧栏截图仍需宿主人工验收。

> 2026-09-10 主题图标包接入：用户提供的 `GameSaveCenter_IconPack_v2_round-flat.zip` 已转换为共享 `ThemeAwareIcon` + `GscIconPack.xaml` 原生 Geometry/Path 资源，并补齐设置页 `section-general-directory`、`section-migration` 映射。生产导航、设置、首页状态、媒体/维护/修改器/任务局部图标和兼容 Dashboard 图标均改为透明底、继承 `Foreground` 的线稿，真实命令/Binding/虚拟化/自动化语义保持。压缩包 plugin PNG 未直接覆盖清单 `icon.png`，因为透明导出不适合 Playnite 清单显示；plugin-main 仅作为界面 fallback。Release 构建 0 warning/0 error，Playnite `432/495`（63 skip，0 fail），双主题 RenderHarness 为 `render-qa OK`；真实 Playnite 用户主题、Follow、DPI 和清单图标仍需人工验收。

> 2026-09-10 统一 Glass 按钮组件语义：复用现有 `ui:Button` + `GscWpfUiButton` 生产模板，新增 `Themes/ButtonStyles.xaml` 语义别名（`GlassButton`、`PrimaryGlassButton`、`DangerGlassButton`、`IconButton`、`SegmentedButton`），并补充 Danger 状态和模板实例级 `0.97` 按压缩放。颜色继续跟随 `DesignTokens`/`AdaptiveThemePalette`，未引入逐按钮 Blur 或修改业务/布局；Release 构建、全量测试、源码/XAML/WPF 门禁和提交后 RenderHarness 均通过。真实 Playnite、用户主题、DPI 与安装路径仍需宿主验收。

> 2026-09-10 设置主题跟随与导航对齐：FollowPlaynite 主题资源解析现在优先读取实际承载设置页的 Window/Owner，再落到控件局部资源和 Application，避免设置窗口误用旧深色值；`AcrylicNavItem` 及七个生产导航内容组补齐图标/文字垂直居中。定向 Playnite WPF 测试 `130 passed / 39 skipped / 0 failed`；真实 Playnite 设置窗口、用户主题和 DPI 截图仍待宿主验收。

> 2026-09-10 表格内容视口继续修正：共享 `GscRedesignDataGridTemplate` 的 `PART_ScrollContentPresenter` 与 `ItemsPresenter` 均固定为横向拉伸、纵向顶对齐，避免短窗口/滑块端点把已实现行重新放到列头下方中部；Media `Standard` 行虚拟化、`Item` 滚动和关闭列虚拟化保持不变。定向 `MediaWindowAnchorContractTests` `10/10`，离屏报告 [`.tmp/qa-table-scroll-20260910-final/render-qa-report.txt`](../.tmp/qa-table-scroll-20260910-final/render-qa-report.txt) 为 `render-qa OK`；真实 Playnite、物理 DPI 和窗口化现场仍待验收。

> 2026-09-10 Media 待归类表格空白区继续修正：共享 `GscRedesignDataGridTemplate` 的外层 `DG_ScrollViewer` 与内层 `PART_ScrollContentPresenter` 均已补上 `HorizontalContentAlignment` / `VerticalContentAlignment` 绑定，修复 `VerticalContentAlignment=Top` 未贯穿到真实内容视口的问题。Media 的大数据例外（`Standard` 行虚拟化、`Item` 滚动、关闭列虚拟化、禁用 `DataGridStarFill`）保持不变；本阶段定向契约测试 `10/10`，两个视觉修复完成后统一更新全量回归结果，真实宿主物理滚动仍待人工验收。

> 2026-09-10 页面外围矩形继续修正：用户图二中各页面都有的长方形来自页面级 `AmbientMaterialLayer`，不是需要重复绘制的图片。页面层现在通过 `GscAmbientPageOpacity=0` 保持兼容挂载但透明；Shell 的单一 `ShellAmbientMaterialLayer` 通过 `IsShellLayer="True"` 继续绘制跨侧栏、右侧页面和 footer 的统一环境材质。Release 构建/全量测试、源码/XAML/WPF 门禁和 RenderHarness 均通过；干净报告 [`.tmp/qa-table-ambient-20260910/render-qa-report.txt`](../.tmp/qa-table-ambient-20260910/render-qa-report.txt) 为 `fc3c7d5`、`WorkingTreeClean: True`、`render-qa OK`。真实 Playnite/FusionX、DPI、物理滚动仍待宿主人工验收。

> 2026-09-10 生产壳层背景去缝：图二长方形是选中游戏背景 `ImageBrush`，其图片层/ambient 层保持跨两列两行；已去掉 `DemoShell` 外边距和外框、侧栏右侧 6 DIP 缝以及 footer 独立描边/边距，让侧栏与右侧页面成为连续整页背景。离屏双主题与 Shell Media 几何通过，真实 Playnite 图片资源/FusionX/DPI 仍待宿主人工验收。

> 连续实施入口：[32 项、8 阶段计划](ai/CONTINUOUS_DEVELOPMENT_PLAN_2026-09-08.md)。用户要求减少逐项确认；接手后按依赖连续实施并逐项交付，已满足任务跳过，外部阻塞不妨碍独立任务。Q6 质量报告仍是首四项问题依据。

> 2026-09-10 当前 HEAD 候选包：从 `8657654` 重新运行发布链，六份程序集身份统一为 `0.6.73+8657654310d99e349c120f3bb0484b65ac9f4dc2`；`.pext/.zip` 均 `43,837,912` 字节，SHA-256 为 `E136B5C6465A5A8933C72B0F9707CFF64D91A02D35115B9A007BF6A749B62AB0`。Core `76/76`、Worker `310/311`（1 skip）、Playnite `428/491`（63 skip），0 失败，构建 0 warning/0 error；本次只增加测试覆盖，生产 XAML 仍来自 `248d28e`。当前离屏报告为 [`.tmp/render-qa-head-clean-20260910/render-qa-report.txt`](../.tmp/render-qa-head-clean-20260910/render-qa-report.txt)，`render-qa OK`。候选包未安装真实 Playnite，FusionX、用户主题、DPI、物理点击、视频和滚动矩阵仍待 L31 宿主条件。

> 2026-09-10 最新候选包：生产源提交 `248d28e` 补齐活动游戏选择器复合按钮的 `ContentTemplate={x:Null}` 防护，并增加样式契约断言。候选 `.pext/.zip` 六份程序集身份为 `0.6.73+248d28eff8c595516a803f8db356952cef54c166`，均 `43,837,966` 字节，SHA-256 为 `628F34B01C478CD30A26260650703C8B77F558561E103D78B24C8A390949DDD3`；Core `76/76`、Worker `310/311`、Playnite `427/490`，0 失败。干净 RenderHarness 报告为 [`.tmp/render-qa-gamecontext-clean-20260910/render-qa-report.txt`](../.tmp/render-qa-gamecontext-clean-20260910/render-qa-report.txt)，`WorkingTreeClean: True`、`render-qa OK`。真实 Playnite/FusionX、用户主题、DPI、物理点击和视频仍待 L31 宿主条件。

> 2026-09-10 游戏选择器运行时测试补强：新增 STA WPF 测试实际套用 `GscRedesignGameContextButton` 模板并确认 `ContentPresenter` 保留原始 `Grid`，定向 `1/1`；当前 main 全量 Release 回归为 Core `76/76`、Worker `310/311`（1 skip）、Playnite `428/491`（63 skip），0 失败。本次只增加测试覆盖，未改变 `248d28e` 候选包的生产程序集；真实 Playnite/FusionX、用户主题、DPI、物理点击和视频仍待 L31 宿主条件。

> 2026-09-10 顶部复合按钮修复与候选包更新：提交 `63f4b2d` 为 Dashboard 顶部图标+文字按钮增加视觉内容专用样式，避免共享文本模板显示 `System.Windows.Controls.Grid`；定向 `1/1`、全量 Playnite `427/490`（63 skip、0 fail）、Release 构建 `0 warning/0 error`。从该提交生成的 `.pext/.zip` 六份程序集身份为 `0.6.73+63f4b2d54d0ff69f824a167ac3d62c070c32bf37`，均 `43,837,982` 字节，SHA-256 为 `83FC0C96476B58FD5DC8AA3E97E850B12BB77EC06A5739DE361EA96ACDC17869`。真实 Playnite/FusionX、用户主题、DPI、物理点击和视频仍待 L31 宿主条件。

> 2026-09-10 L30/L32 继续推进：从生产源提交 `fa9af0a` 重新生成候选包，六份程序集身份为 `0.6.73+fa9af0ad36db07551bd1c2985258d72eecb9c8e4`，`.pext/.zip` 均 `43,837,868` 字节，SHA-256 为 `631615AB7695C46F943D9546A53369C69ADA49F347C4F7CE33D96A33C3831249`。Core `76/76`、Worker `310/311`、Playnite `427/490`，0 失败；临时 staging 已清理。真实 Playnite/FusionX、用户主题、DPI、物理点击和视频仍待 L31 宿主条件。

> 2026-09-09 首页交互测试补齐：`4001d9d` 新增 STA WPF `OverviewInteractionTests`，实际布局验证全局活动行保留 `Border` 可视树，云端队列整卡通过 `ButtonBase.OnClick` 只执行一次 `OpenCloudQueueCommand`。Release 构建 `0 warning / 0 error`，Playnite 全量 `427/490`（63 skip、0 fail）；随后已从生产源提交 `fa9af0a` 重新生成当前候选包。真实 Playnite/FusionX、用户主题、DPI、物理点击和视频仍待宿主验收。

> 2026-09-09 Media 离屏门禁收口：`d777e65` 将主表视口验收改为实际 DataGrid 行容器的完整可见数（有足够数据时至少 `4/4` 行），并修正 resize 探针向 Media 传入 PageHost `contentH`；Inspector 内 2/3 条的预览/历史列表不再被误当主表。干净报告 [`.tmp/render-qa-media-gate-clean-20260909/render-qa-report.txt`](../.tmp/render-qa-media-gate-clean-20260909/render-qa-report.txt) 为 `WorkingTreeClean: True`、`render-qa OK`，Media resize `300 DIP/readableRows=6/4`。该结果仍是离屏证据，真实宿主/FusionX 与视频操作待验收。
> 2026-09-09 当前交接基线：文档与证据边界同步提交为 `580a70f`。它没有继续修改生产表格模板；当前 Computer Use 仍返回 `apps: []`，没有新的真实 Playnite 截图、录屏或滚动诊断。`render-qa OK` 仍只能作为离屏门禁证据，L31 真实宿主矩阵保持外部阻塞。
> 2026-09-09 首页修复后的顺序 Release 复验：`dotnet build GameSaveCenter.sln -c Release --no-restore -m:1` 为 `0 warning / 0 error`；Core `76/76`、Worker `310/311`（1 skip）、Playnite `426/489`（63 skip）均无失败。源码/XAML/WPF 静态门禁通过；RenderHarness 干净报告为 `.tmp/render-qa-overview-cloud-20260909/render-qa-report.txt`，详细过程见 `ai/WORKLOG.md`。
> 2026-09-09 云端队列浅色控件树证据：RenderHarness 新增真实 `MaintenanceView` 云端队列页探针，双主题两个筛选 ComboBox 均找到可见选中文本；Light 前景 `#F21B1F27`、Dark 前景 `#FFF2F4F8`。完整报告和截图在 `.tmp/render-qa-cloud-filter-probe-20260909/`，仍属于离屏证据，真实 FusionX 宿主待验收。
> 2026-09-09 壳层背景结构探针：同一报告检查 `ShellAmbientMaterialLayer` 在浅/深主题均跨完整壳层两列与 footer 两行，并保持 `UseSelectedGameBackground=False`；离屏没有真实 Playnite 背景位图，因此只能作为图片方框结构回归证据，不能替代宿主截图。

> 2026-09-09 回归与离屏夹具更新：`c0197e5` 隔离了两个共享 `AsyncThumbnailLoader` 静态状态的 xUnit 测试，修复并发套件中 `120` 被污染为 `122` 的失败；定向 `1/1`，Playnite 全量 `425/488`（63 skip、0 fail）。`559d64f` 修正 Settings 临时目录、Sidebar rapid-toggle 调度和 Media PageHost 高度探针。干净报告见 [`.tmp/render-qa-harness-clean-20260909/render-qa-report.txt`](../.tmp/render-qa-harness-clean-20260909/render-qa-report.txt)：Settings/Sidebar 门禁已通过，生产壳层 Media 1040/1100 表格为 `300 DIP` 且页尾可达；完整 render-qa 仍只剩 Media 预览/独立表格离屏尺寸门禁。该报告不替代真实 Playnite/FusionX 与视频式宿主验收。

> 2026-09-09 浅色主题视觉修复：生产壳层 `ShellAmbientMaterialLayer` 已改为跨完整壳层且不再重复读取选中游戏背景，避免侧栏右侧/页面最右侧出现图片方框；维护中心及 Dashboard 的 `GscComboBoxLongText` 已显式使用 `GscPrimaryTextBrush`，修复浅色云端队列下拉文字不可见。定向测试 `2/2`、源/XAML 门禁和 WPF 静态审计通过；后续 `559d64f` 已排除 Settings/Sidebar 的离屏夹具误报，但完整 render-qa 仍有 Media 离屏尺寸门禁。真实 Playnite/FusionX 浅色主题截图尚未取得，不能写成宿主已验收。

> 2026-09-09 L42/L41 真实宿主入口边界：L41 受限启动记录 CEF `拒绝访问 (0x5)`；L42 提升权限后 Playnite 进程仍无可绑定主窗口（`MainWindowHandle=0`），UI Automation 找不到 GameSaveCenter 侧栏，最终没有 `summary.json` 或新的表格 replay。L42 构建/回归为 Core `76/76`、Worker `311/311`、Playnite `431/488（57 skip）`，详见 [`ai/L42_REAL_HOST_SCROLL_REPLAY_2026-09-09.md`](ai/L42_REAL_HOST_SCROLL_REPLAY_2026-09-09.md)。这不是表格根因证据；当前有效宿主端点证据仍为 L39，视频 B 类问题仍待宿主人工验收。

> 2026-09-09 L40 真实宿主复核：c84107b 包在隔离 Playnite 中启动并修正了遗留旧 Worker 路径，但最终 `EmbeddedDashboardCaptured=false`，只得到受控 Dashboard；该轮没有新的真实嵌入表格 replay JSON，不能当作滚动通过。b100913 的 RangeValue provider 异常已由 c84107b 在开发审计路径中隔离，生产滚动逻辑未改。最新 Core/Worker/Playnite 回归为 `76/76`、`310/311（1 skip）`、`425/488（63 skip）`，详见 [`ai/L40_REAL_HOST_SCROLL_REPLAY_2026-09-09.md`](ai/L40_REAL_HOST_SCROLL_REPLAY_2026-09-09.md)。

> 2026-09-09 L39 真实宿主回放：提交 `a9b8bce` 的包已安装到隔离扩展目录并在真实 Playnite/FusionX 中运行。媒体表 400 条、任务表 50 条各 47 个样本，底部各 21 个、上下端点往返 20 次；Media `394/394`、Task `40/40` 到达末端，最后加载行分别在 Presenter `0,36,604×279.33` 与 `0,36,637.33×456` 内完整可见，cell/visual/text 为 `5/5/5`、`6/6/6`。空正文、大间隙、末端水平条覆盖、选中内容缺失和尾项不完整均为 0。详细证据见 [`ai/L39_REAL_HOST_SCROLL_REPLAY_2026-09-09.md`](ai/L39_REAL_HOST_SCROLL_REPLAY_2026-09-09.md)。

> L39 没有凭程序化端点回放猜改生产模板、ScrollUnit、Margin 或虚拟化；代码只补强诊断尾项完整性和开发审计覆盖任务表。任务审计跳过加载更多，因为当前任务命令替换 50 条页面但保持 `HasMore=true`，重复调用会污染回放。

> L39 仍不是视频式人工验收：Computer Use 原生应用返回 `apps: []`，未完成物理滑块拖动、滚轮/PageUp/PageDown/Ctrl+End、DPI/主题矩阵或真实录屏。视频中的 B 类问题必须继续标为“宿主人工待验收”，不能写成已解决；若人工复现，使用同一诊断字段与视频时刻对齐后再决定生产修复范围。

> 2026-09-09 L36 已获得真实 Playnite 嵌入采集：当前提交 `99bc976473d13a92c12d6992dff11dbf807e42b5` 在隔离只读数据目录中由生产扩展完成 EmbeddedPlaynite Dashboard/设置页采集，证据见 [`artifacts/ui-host-audit-isolated-l36/summary.json`](../artifacts/ui-host-audit-isolated-l36/summary.json) 和 [`diagnostics-summary.md`](../artifacts/ui-host-audit-isolated-l36/diagnostics-summary.md)。本次没有修改用户 FusionX 文件、Playnite 全局样式或用户数据目录。

> L36 诊断结论：真实 `MediaInboxGrid`/`TaskGrid` 的行滚动器均为 `ScrollViewer`，`IScrollInfo=ScrollContentPresenter|DataGridRowsPresenter`，`CanContentScroll=True`、`ScrollUnit=Item`；水平条出现时 Presenter 实际矩形已缩小。真实日志出现短暂 `visual>0,text=0` 后约 32ms 恢复，但没有持续 `blank=True`、`gap=True`、`hOverlap=True` 或行漂移，因此不能凭本次运行认定视频根因。媒体表本次 `Items.Count=200`，底层快照 `media=4645`，分页不能混写成全量。

> L36 尚未完成视频式人工回归：当前 CUA 无可识别原生 Playnite 窗口，未完成滑块往返 20 次、滚轮/PageUp/PageDown/Ctrl+End、尾部选择、水平条显隐、缩放矩阵和真实录屏；`CapturedAndValidated` 的滚动面清单也不能替代人工确认 MediaInboxGrid 已加载末项完整可见。继续交接时必须保留“待宿主人工验收”，不得写成视频问题已解决。

> 2026-09-09 当前候选包已按 `1fdd15e` 重新生成：六份程序集身份同源为 `0.6.73+1fdd15eedfa64bb34292b85cb0e4d14bbfa9dd81`；Worker `win-x64` self-contained；`.pext`/`.zip` 均为 `43,837,799` 字节，SHA-256 均为 `FD91FB0E0B12ABA2A73F29F76F1E3F90255D6FA4D30798EBA2FEFB53FAD120F9`。Core/Worker/Playnite 为 `76/76`、`310/311`、`426/489`（均无失败），包含首页活动内容与云端整卡导航修复，未安装真实 Playnite。

> L30 回退边界：当前数据库迁移是幂等增量，不承诺旧包直接读取已升级 schema；升级前要备份完整隔离配置/状态库，回退先停止宿主、保留失败目录、恢复升级前副本，再安装旧包。详细清单见 `ai/L30_PACKAGE_CHECKLIST_2026-09-09.md`；真实宿主加载、FusionX、DPI、Worker 回收和视频仍待 L31。

> 2026-09-09 L31 外部阻塞：PowerShell 确认 `D:\software\Playnite\Playnite.DesktopApp.exe` 路径，但本次继续核查时 Playnite 进程已退出，Windows Computer Use 仍返回 `apps: []`，没有可绑定窗口；本轮未启动、停止或重装宿主，未生成真实宿主截图/录屏/滚动诊断。详见 `ai/L31_REAL_HOST_BLOCKER_2026-09-09.md`，不能把 L30 离线包证据写成视频问题已解决。

> 2026-09-09 表格诊断补强：诊断器已优先绑定包含 `DataGridRowsPresenter` 的实际表格 `ScrollViewer`；Media 锚点记录请求、执行、完成、代际跳过、重试和失败原因。提交 `862742a` 新增隐藏 WPF `Window` 对照：插件模板从顶部 `ScrollIntoView(最后一项)` 到 `1992/1992` 且最后行完整，20 次往返和 `PageDown/PageUp/Ctrl+End` 通过；标准模板直接滑块/Ctrl+End 末行完整，但 deferred `ScrollIntoView` 仍为 `offscreen-baseline-inconclusive`。随后提交 `85b1aeb` 只读加载本机 FusionX `2.1.1` 的 `DefaultControls/DataGrid.xaml`：直接滑块/Ctrl+End 到 `1987/1987`、末行 `1999` 完整，deferred 定位仍基线不确定；提交 `5198c6c` 补充 700×640 DIP / 1100 DIP 列宽的水平条场景，`Visible/17.33` 时 Presenter 为 `678.67x582.67`，末行仍完整；提交 `8775809` 修正报告的干净工作树标记，canonical 报告为 `WorkingTreeClean: True`；提交 `f31711c` 让锚点捕获/恢复使用同一实际表格滚动器选择规则，定向 `10/10`；未修改用户主题文件。这只能缩小离线模板范围，不能替代真实宿主证据。详见 `.tmp/l32-scrollprobe/scaleprobe-report.txt`。
> 2026-09-09 表格诊断补强：诊断器已优先绑定包含 `DataGridRowsPresenter` 的实际表格 `ScrollViewer`；Media 锚点记录请求、执行、完成、代际跳过、重试和失败原因。提交 `862742a` 新增隐藏 WPF `Window` 对照：插件模板从顶部 `ScrollIntoView(最后一项)` 到 `1992/1992` 且最后行完整，20 次往返和 `PageDown/PageUp/Ctrl+End` 通过；标准模板直接滑块/Ctrl+End 末行完整，但 deferred `ScrollIntoView` 仍为 `offscreen-baseline-inconclusive`。随后提交 `85b1aeb` 只读加载本机 FusionX `2.1.1` 的 `DefaultControls/DataGrid.xaml`：直接滑块/Ctrl+End 到 `1987/1987`、末行 `1999` 完整，deferred 定位仍基线不确定；提交 `5198c6c` 补充 700×640 DIP / 1100 DIP 列宽的水平条场景，`Visible/17.33` 时 Presenter 为 `678.67x582.67`，末行仍完整；提交 `8775809` 修正报告的干净工作树标记，canonical 报告为 `WorkingTreeClean: True`；提交 `f31711c` 让锚点捕获/恢复使用同一实际表格滚动器选择规则，定向 `10/10`；未修改用户主题文件。这只能缩小离线模板范围，不能替代真实宿主证据。详见 `.tmp/l32-scrollprobe/scaleprobe-report.txt`。

> L32 证据索引：`ai/EVIDENCE_INDEX_2026-09-09.md` 汇总当前候选包、离线滚动/回归证据、skip 账本和真实宿主缺口；不要把 `scaleprobe` 或 `ScrollIntoView` 离屏结果写成 FusionX/视频通过。

> 2026-09-09 L28 已完成持续更新分页与选择恢复的离线收口：云端队列和媒体归类历史的 revision 令牌继续阻止变化期间拼接 offset 页；云端/归类 VM 在重置前捕获稳定选择 ID，第一页重建后按 `HasMore` 加载到后页恢复，删除或到达末页才清理选择。连续第二次重置会停止自动递归，保留待恢复 ID，并在摘要/状态消息中提示使用已有刷新按钮。

> L28 证据：云端/媒体历史分页定向 `26/26`，新增 Playnite 分页状态契约 `1/1`；Release 构建 `0 warning/0 error`，Core `76/76`、Worker `310/311`（1 项真实重启测试沙箱跳过）、Playnite `420/483`（63 项 UI/宿主条件 skip），源校验、XAML 和差异检查通过。真实 Playnite/FusionX、DPI、用户主题和原视频矩阵仍待宿主验收，不能写成视频问题已解决。

> 2026-09-09 L29 已完成全量回归与 skip 账本：Core `76/76`；Worker `310/311`（1 项 Worker 进程重启受当前 Named Pipe 沙箱条件跳过）；Playnite `420/483`，明确拆分为 57 项旧“今日工作台”架构断言与 6 项 Named Pipe IPC 行为测试。详细原因、替代证据和剩余风险见 [`ai/SKIP_LEDGER_2026-09-09.md`](ai/SKIP_LEDGER_2026-09-09.md)。

> L29 另修正 E01 行为矩阵的 `-SkipBuild` 路径：不向未构建的隔离输出目录发测试请求。修复后实际执行 `151` 项，`144` 通过、`7` 跳过；空日志/`0/0` 不再被接受。真实 Playnite/FusionX、Named Pipe 进程间时序、Worker 硬重启、DPI、用户主题和原视频仍待宿主验收。

> 2026-09-09 L27 已完成配置、路径与外部文件变化的离线收口：设置目录校验只读地进行区分，可创建缺失叶目录、文件目标和不可达盘/共享分别处理；用户媒体来源及附加存档探测根目录不再把消失/拒绝访问伪装成空扫描。新增稳定错误码 `MEDIA_SOURCE_UNAVAILABLE`、`MEDIA_FILE_UNAVAILABLE`、`SAVE_PATH_ROOT_UNAVAILABLE`，占用媒体源文件不会被删除。

> L27 证据：设置/便携导入定向 `11/11`，媒体/存档路径定向 `16/16`（含 Unicode/长文件名、占用文件、缺失来源和缺失附加根）；Release 构建 `0 warning/0 error`，Core `76/76`，Worker `308/309`（1 项真实进程重启测试在沙箱跳过），Playnite `419/482`（63 项 UI/宿主条件 skip），源/XAML/差异门禁通过。当前没有真实 Playnite 保存失败、网络共享 ACL、外置盘断开、DPI 或用户视频证据；本阶段仍标为宿主待验收。

> 2026-09-09 L22 已完成缩略图加载、取消与缓存边界收口：`AsyncThumbnailImage` 只为已加载且可见的卡片启动任务，不可见/卸载取消并清空旧图，generation/token 防止旧路径结果回写；`AsyncThumbnailLoader` 保持 `OnLoad` 冻结、3 路并发、96 项 LRU 和预期读取失败空占位。

> 2026-09-09 L23 已完成搜索、刷新与重复 IPC 的代码边界收口：任务历史页和 Dashboard 快照请求使用 `LatestRequestCoordinator` 做取消、generation 和提交前检查；任务 debounce 回调经 UI 投递，忙时保留最新查询，旧响应不能覆盖当前筛选。日志只记录代际、请求号、数量/尺寸和 `hasMore`，不记录搜索文本或文件内容；未修改写请求协议。

> L23 证据：连续搜索夹具 `a→ab→abc` 最终只回调 1 次；协调器覆盖取消、替换、CTS 释放后 Token 状态和 20 次快速替换仅最后作用域可提交。独立 Playnite `417` 通过、`62` 跳过、`0` 失败；Core `76/76`、Worker `303/304`（1 跳过），Release 构建无警告/错误。整套解决方案的一次并行 Playnite 重跑有 8 个既有 WPF `PackagePart.CleanUpRequestedStreamsList` 构造失败，独立重跑通过，保留为测试宿主偶发问题。真实 Playnite/FusionX、DPI、用户视频与实际 IPC 日志计数仍待宿主验收。

> 2026-09-09 L24 已完成隔离账本有限加载和动效预算采样：UI IPC 使用 SQLite `COUNT + LIMIT/OFFSET` 分页，默认/上限 100 条；维护页首载一页，显示 durable 总量与已加载量，继续加载按 `EntryId` 追加并保留原单条确认/恢复动作。溢出列表使用共享 Recycling ListBox 和 `MaxHeight=360` 有限视口，未关闭虚拟化或加入定时布局刷新。

> L24 证据：205 条 Worker 夹具返回 `100/100/5`，总量 `205`，稳定 ID 去重 `205`；RenderHarness `shellqa` 单次收起 `288.0ms/layout 46/maxGap 53.2ms`、快速往返 `424.8ms/layout 58/maxGap 31.1ms`、关闭动画终态 `59.6ms/layout 3/maxGap 17.1ms`，均 settled。Release 构建无警告/错误；Core `76/76`、Worker `304/305`（1 跳过）、Playnite `417/479`（62 跳过）；源校验、XAML `19/19`、WPF 静态审查和差异检查通过。报告来自离屏逻辑 DIP，真实 Playnite/FusionX、真实主题/DPI、视频操作和 60fps 仍待宿主验收，不能写成已解决。

> 2026-09-09 L25 已完成 IPC 取消、超时和旧响应复核收口：破坏性请求超时/断管后用同一 `RequestId` 做 ledger 复核，回执丢失不生成新请求；复核阶段继承调用方 Token，取消时保留“可能已接受”语义，宿主关闭与调用方取消分开报告。L23 的 VM generation/提交门继续阻止旧响应覆盖页面。

> L25 证据：本地 Named Pipe 行为矩阵 `6/6` 通过，Worker `IpcRequestLedgerTests` `6/6` 通过，Release 构建无警告/错误。真实 Playnite 页面关闭、Worker 实际启动/中断/恢复、宿主弹窗时序和用户视频仍待验收；Worker 重启总套件的 1 项历史环境 skip 保持不伪造为通过。

> 2026-09-09 L26 已完成 Worker 断连/重启和旧构建的隔离验收记录：`WorkerLauncher` 保留协议/版本/构建身份分类、同路径进程健康宽限、新进程 30 秒就绪截止和只回收本插件持有进程；Worker 初始化恢复旧 IPC ledger，任务硬重启状态标为 `WORKER_RESTARTED_RETRYABLE`，不盲目重放旧写请求。

> L26 证据：独立临时 Worker 真实 Named Pipe 启动→硬停止→重启→durable task 恢复 `1/1` 通过；Worker 全套 `305/305`、0 skip；Playnite 全套 `423/480`（57 个 UI/宿主条件 skip）通过。没有真实 Playnite/FusionX 窗口内启动失败、运行中断连、恢复刷新、DPI 或多实例录屏，仍待宿主验收，不能写成真实安装环境全通过。

> L22 证据为 `.tmp/l22-thumbnailprobe-final/thumbnailprobe-report.txt` 及 Playnite STA 测试：120 项初次窗口 `120` 成功、峰值并发 `3`、缓存 `96/96`；16 项保留窗口全命中；破损/缺失均为空；预取消可观测；100 次 12 项窗口往返后活动解码 `0`；旧 800×800 图替换为新 64×64 图后最终像素标记为新图，输出宽度 `96`。这是合成文件和隐藏 STA Window 证据，不是实际 Playnite/FusionX、DPI、用户目录或视频录屏验收。

> 2026-09-09 L21 已完成列表虚拟化与滚动规模实测：证据先于参数调整。原当前游戏 `MediaGrid` 使用普通 `WrapPanel`，在 200/2000/10000 后端夹具中生成 200/2000/2000 个卡片；修复仅接回已有 `VirtualizingWrapPanel`，保留 164×154 卡片、选择和 ListBox 滚动契约。修复后顶部→底部→顶部均约 20 个容器，任务表与媒体收件箱的 DataGrid 诊断/20 次滑块往返/语义滚动/resize/选择保持通过。

> L21 证据与边界：最新离屏报告为 `.tmp/l21-scaleprobe-final/scaleprobe-report.txt` 和 `.tmp/l21-render-qa-final/render-qa-report.txt`；全量 Render QA 的卡片回滚探针已通过，仍有既有 Media 小视口/预览列表与 Sidebar rapid-toggle 失败。DataGrid 直接 `ScrollIntoView(最后一项)` 在插件模板和标准 WPF 对照模板中都呈现离屏 deferred，记录为 `offscreen-inconclusive`，不能作为 FusionX 根因。无真实 Playnite/FusionX、DPI 或用户视频复测，视频中的 DataGrid 空白、漂移、选框与文字分离、最后行完整可见仍必须在宿主按验收矩阵复测。

> 2026-09-09 L20 已完成修改器导入与下载结果反馈：导入检测/确认与 FLiNG 下载均捕获稳定游戏/目录/版本 ID；切换游戏不会把晚到结果写回错误上下文。下载状态区分排队、进度、成功、重复绑定、站点拒绝、格式拒绝、版本解析失败、取消和通用失败，并复用 Worker `CancelTask`；下载文件不自动运行，来源/签名/安全解压/临时文件清理由 Worker 保持。真实 Playnite/FusionX、在线 403/离线、真实取消时序、DPI 和视频仍待宿主验收。

> L20 验证：Release 构建 `0 warning/0 error`；Core `76/76`、Worker `303/304`（1 跳过）、Playnite `405/467`（62 跳过）；RenderHarness Release 构建、`validate-source.py`、XAML `19/19`、WPF 静态审查 `0 errors/21 warnings/172 info`、差异检查通过。完整 render-qa 仍只有既有 Media 小视口/预览列表与 Sidebar rapid-toggle 失败，未新增 Trainer/Save 失败。

> 2026-09-09 L19 已完成存档版本识别、比较与恢复信息：版本详情明确显示来源设备、系统、锁定状态、恢复就绪状态和检查时间；缺少事实时显示未知/尚未检查，不从时间推断健康。比较页明确“当前版本与上一版本”，补充未变化数量、带符号大小变化和文件分组；选择变化/无上一版本会清掉旧差异，响应返回前校验当前游戏与左右版本 ID。恢复确认前捕获目标游戏/版本稳定 ID 与事实摘要，确认后请求不读取可变选择；PreRestore、锁定当前快照、关闭游戏确认、Worker 安全校验和撤销保持原语义。

> L19 验证：Release 构建 0 warning/error；Core `76/76`、Worker `303/304`（1 跳过）、Playnite `404/466`（62 跳过）；`validate-source.py`、XAML `19/19`、WPF 静态审查 `0 errors/21 warnings/172 info`、差异检查通过。离屏 Save 历史/比较截图与双主题、多尺寸探针没有本轮新增问题；完整 render-qa 仍受既有 Media 小视口/预览列表和 Sidebar rapid-toggle 失败影响。当前无可用真实 Playnite/CUA/FusionX 宿主，因此不同恢复状态、确认期间选择变化、DPI、真实滚动和录屏仍待宿主验收。

> 2026-09-09 L17 已完成常用筛选与工作区状态记忆：任务状态/游戏/类型/搜索/历史范围/时间范围统一持久化；动态筛选等任务选项重建后恢复，当前 Playnite 游戏库已删除的游戏回退“全部”，仍存在但暂时不在最近任务窗口的游戏不会因分页误判丢失；非默认历史范围重启会重新激活服务端历史分页。旧/非法历史范围归一化，恢复只查询不自动执行批量动作。媒体当前游戏页新增真实的“清除媒体筛选”命令，取消旧防抖、失效详情代际并复用现有分页保护。

> 2026-09-09 L18 已完成批量动作提交前摘要：媒体批量归类、忽略、恢复、归类预览在入口捕获去重后的稳定 MediaId；归类目标和应用建议捕获目标 PlayniteId/BatchId，确认期间列表选择变化不会改写提交范围。任务批量重试从当前 TasksView 结果按游戏/任务类型去重，缺少稳定 TaskId 的记录计入跳过，确认后按不可变任务快照执行。结果摘要区分计划、成功、失败、冲突、跳过/未返回，不把部分成功说成全部成功；未新增 Worker 协议或批量删除。

> L18 验证：Release 构建 0 warning/error；全量 Core `72/72`、Worker `303/304`（1 跳过）、Playnite `404/466`（62 跳过）；`validate-source.py`、XAML `19/19`、WPF 静态审查 `0 errors/21 warnings/172 info`、差异检查通过。本轮未修改 XAML，未重复 render-qa。当前无可用真实 Playnite/CUA/FusionX 宿主，因此批量确认框、选择在确认后变化、部分失败结果和录屏仍必须写“宿主待验收”。

> L17 验证：Release 构建 0 warning/error；Core `72/72`、Worker `303/304`（1 跳过）、Playnite `403/465`（62 跳过）；持久化/迁移定向 `10/10`；源码校验、XAML `19/19`、WPF 静态审查 `0 errors/21 warnings/172 info`、差异检查通过。完整 render-qa 仍有既有直接 Media 小视口/预览列表与 Sidebar rapid-toggle 失败，本轮未新增筛选工具栏或主题/resize 问题；真实 Playnite/FusionX 重启恢复、删除游戏、DPI、键盘和录屏仍待宿主验收。

> 2026-09-09 L16 已完成媒体收件箱操作可达性收口：批量栏在 1040 DIP 内容宽度不再因摘要/目标控件过宽而换行；生产壳层 PageHost 高度低于 620 DIP 时由 `MediaInboxPageScrollViewer` 承接页面内容滚动，DataGrid 仍保持有限视口、Item 滚动、Recycling 和行列虚拟化。1040×700/1100×720 壳层表格均为 300 DIP，页面滚到底后 footer、批次历史和次级操作完整可见；没有固定底部补偿、定时 `UpdateLayout`、强制回顶或关闭虚拟化。

> 本轮新增 RenderHarness 末尾滚动探针，并以实际 `offset/scrollable` 和 footer/history/secondary 的视口几何作为证据。全量构建/测试通过：Core `72/72`、Worker `303/304`（1 跳过）、Playnite `402/464`（62 跳过）；源校验、XAML `19/19`、差异检查通过。完整 render-qa 仍剩直接 Media 小视口/预览列表门禁和侧栏 rapid-toggle，不宣称全量通过；当前没有可用 CUA/真实 FusionX 宿主录屏，因此视频式拖动、真实模板链、DPI 和真实 Playnite 仍必须标为“宿主待验收”。

> 2026-09-09 L15 已完成任务页查错与范围说明：顶部同时说明运行中、排队/等待确认、当前结果可重试、失败/取消；宽屏队列标题区直接显示活跃筛选摘要，批量重试明确只处理当前已加载且符合筛选的结果。服务端分页、`TaskIndexedCollection`、按游戏/任务类型去重、完整错误详情、复制和安全重试语义均保留。Release 构建/全量测试通过（Core `72/72`、Worker `303/304`（1 跳过）、Playnite `402/464`（62 跳过）），任务定向 `36/42`（6 跳过），源码/XAML/WPF 静态检查通过；离屏 Task 在 1040×700 保留 6 行首屏，双主题/resize 通过。完整 render-qa 的稳定失败仍是既有媒体小视口/媒体壳表格高度，首轮另有一次侧栏 rapid-toggle 未稳定；真实 Playnite/FusionX、DPI、键盘和长历史操作仍待验收。

> 2026-09-09 L14 已完成运维总览按处理顺序组织：维护动作按人工处理、等待自动重试、例行巡检分组；每组首屏最多 3 条，剩余记录通过显式展开器显示，空组不生成空卡片。总量与当前摘要分开，长标题、风险说明和具体 `TransferKey`/`EntryId` 动作均保留，未增加批量自动修复。Release 构建/全量测试通过（Core `72/72`、Worker `303/304`（1 跳过）、Playnite `402/464`（62 跳过）），源码/XAML/WPF 静态检查通过；离屏维护页双主题、多尺寸、resize 通过。完整 render-qa 的稳定失败是既有媒体小视口/媒体壳表格高度，首轮另有一次侧栏 rapid-toggle 未稳定，单独 shellqa 重跑已稳定；真实 Playnite/FusionX、DPI、键盘和宿主模板仍待验收。

> 2026-09-09 L13 已完成首页优先级与活动上下文收口：Hero 继续使用唯一优先级解析，新增空库状态，云端失败沿用 `AttentionCount`；全局活动带稳定游戏 ID，活动行可键盘触发并路由到对应工作区，已加载游戏会恢复同一选择。全量 Release 构建/测试通过（Core `72/72`、Worker `303/304`（1 跳过）、Playnite `401/463`（62 跳过）），源码/XAML/差异检查通过。离屏 render-qa 的 Overview 场景通过，媒体小视口/媒体壳表格高度仍是既有门禁；真实 Playnite/FusionX 活动点击、主题/DPI 和录屏仍待宿主验收。

> 2026-09-09 L11 已完成通知与长错误反馈收口：通知事件分离短摘要/完整详情，成功信息摘要更短；任务失败详情保留错误码和任务 ID；错误/长消息可打开可滚动详情并复制，取消任务使用 Warning 语义。Release 构建 0 warning/error；Core `72/72`、Worker `303/304`（1 跳过）、Playnite `398/460`（62 跳过）；源码/XAML/差异校验通过。完整离屏 render-qa 仍有既有媒体小视口/侧栏快速切换问题，真实 Playnite/FusionX 多任务通知、长错误复制和宿主回退路径仍待验收。

> 2026-09-09 L12 已完成详情展开与选中上下文收口：媒体收件箱换选或切换待归类/已忽略时不再自动重开旧预览/批次历史；任务、媒体、存档、维护的选择清理语义保持一致；维护云端选择事件移除重复订阅。全量构建/测试通过（Core `72/72`、Worker `303/304`（1 跳过）、Playnite `399/461`（62 跳过）），源码/XAML/差异检查通过。未重复完整 render-qa，真实 Playnite/FusionX 的对象删除、快速换选和窄宽循环仍待验收。

> 2026-09-08 L10 已完成设置编辑状态与错误定位：校验摘要旁可用键盘/鼠标定位到首个错误分类；TextBox/ComboBox/CheckBox/ToggleSwitch/Slider 的编辑都会更新脏状态；导入保留原保存基线，DeviceId 不计入用户指纹，取消仍由 Playnite `CancelEdit` 契约恢复。三态与隐藏分类离线探针通过；真实宿主保存失败、取消按钮和 FusionX/DPI 键盘轨迹仍待验收。

> 2026-09-08 L09 已完成设置首屏与状态收口：重复全宽说明折叠、Hero 副标题压缩，核心字段在小窗口更早出现；修正校验错误分支把保存胶囊误标“已保存”的反向布尔值。RenderHarness 同画布 normal/dirty/invalid 三态与多尺寸布局通过；全量构建/测试通过。完整 render-qa 的媒体小视口/侧栏动效问题仍是既有独立门禁，真实 Playnite/FusionX 设置窗口、DPI 和保存取消仍待宿主验收。

> 2026-09-08 L08 已完成可重复诊断入口：RenderHarness 报告带场景/来源/提交/窗口 DIP/主题/数据量，布局与渲染耗时分开；真实宿主运行器写入 `runner-metadata.json`，诊断包系统元数据补充场景、来源、窗口 DIP、已加载条目数、数据量和查询耗时。`gridprobe OK`，Release 构建和全量测试通过；离屏报告不伪造真实 DPI/请求耗时，真实 Playnite/FusionX 录屏与性能采样仍待宿主验收。

> 2026-09-08 L07 键盘/焦点代码收口：媒体、任务、存档、维护、工具页的紧凑详情抽屉已接 Esc；打开进入真实 inspector，关闭回到原入口，Inspector 不进入 Tab 顺序，入口按钮保留自动化名称。新增 STA WPF 控件检查与五页源契约；Playnite `394/456`（62 跳过），构建/源码/XAML/差异校验通过。没有真实 Playnite/FusionX 键盘录屏，必须写“宿主待验收”。

> 2026-09-08 L06 已完成代码与离线行为收口：诊断到存档路径必须命中同一快照游戏；失败任务使用临时目标上下文，不覆盖无关筛选并在请求完成后恢复目标行；目标缺失、名称兜底和无结果均有明确状态。普通侧栏复用已创建页面实例，媒体/存档/维护标签的双向 `SelectedIndex` 保留会话内位置。Playnite 全量 `392/454`，真实 Playnite/FusionX 快速连续入口、隐藏目标和人工录屏仍待宿主验收。

> 2026-09-08 L05 已完成离屏夹具收口：`statefixtures` 直接渲染媒体/维护生产页，Fake 覆盖六态和三条运维动作，双主题四尺寸报告为 `statefixtures OK`。运行时发现并修复了 Stale banner 导致媒体 DataGrid 零高的布局边界：提示条可见时启用外层页面滚动，内部仍是有限 DataGrid 视口和虚拟化。当前证据仍不是 FusionX/真实 Playnite 证据，必须写“宿主待验收”。

> 2026-09-08 用户截图问题已完成代码修复，Worker 部分已完成真实安装/握手/数据请求验收：最终包、安装 DLL 和握手身份必须由最后一次打包输出核对为同一 HEAD；真实 `media.inbox.page` 返回成功且 `totalCount=4615`；受控停止唯一安装路径 Worker 后管道不可达，宿主恢复后握手成功，安装后没有重复启动循环。媒体页已改为有限 PageHost、工具栏/表格/底部操作分行、左右独立滚动、有限详情与离线非零假状态。

> 真实 Playnite 的窗口内视觉验收尚未完成：当前会话没有可用 CUA 端点，尚未取得用户要求的真实同尺寸前后截图，也未完成宽→窄→宽、无选择/有选择/长历史的人工操作和实际尺寸/滚动范围记录。RenderHarness 截图只能作为代码级辅助证据，不能替代宿主截图。32 项扩展计划按依赖继续推进。

> 2026-09-08 上轮独立复核：先读 [Q4/Q5/X2 质量评估与 Q6 计划](ai/QUALITY_REVIEW_2026-09-08.md)。基线 97131f0 的构建/现有测试/离屏复核已通过；之后完成两项代码修复并完成 Worker 真实安装链路，媒体页面真实宿主视觉验收仍待完成。

> 2026-09-08 Q6-01 已完成代码与离屏行为收口：三处失败/离线重试面板移除父级 `IsHitTestVisible=false`，共享状态模板改用 `RetryCommand` 属性触发器，Loading 隐藏重试并阻挡底层；STA 真实模板测试 `5/5`、双主题状态探针 `stateprobe OK`。Release 全量 Core `72/72`、Worker `303/304`（1 跳过）、Playnite `376/438`（62 跳过）。完整 render-qa 本次仍有 25 个媒体小视口/侧栏快速切换问题，真实 Playnite 重播和录屏仍待完成；下一独立项是 Q6-02 状态按游戏/模式隔离。

> 2026-09-08 Q6-02 已完成代码与离线测试：媒体详情状态按游戏 ID+筛选+搜索隔离，收件箱状态按待归类/已忽略隔离；同上下文刷新失败仍为 Stale，新上下文首失败为 Error，旧请求晚到不能覆盖当前代际。筛选/搜索/选中游戏变化会立即推进媒体请求代际并取消旧请求。Release 全量 Core `72/72`、Worker `303/304`（1 跳过）、Playnite `380/442`（62 跳过），构建 0 warning/error。未运行真实 Playnite 故障注入或视频式录屏，后续仍必须写“宿主待验收”，不能把本项代码测试当作视频问题已解决。

> 2026-09-08 Q6-03 已完成代码与离线测试：维护云端状态先按 `TransferKey`/`UpdatedUtc` 合并全部来源，再筛告警；新 Uploaded/RemoteVerified 能覆盖旧 Failed，上传尝试显示为“上次尝试”，隔离账本显示“账本更新”。Release 全量 Core `72/72`、Worker `303/304`（1 跳过）、Playnite `385/447`（62 跳过），构建 0 warning/error。真实 Playnite 分页刷新、故障注入和维护页录屏仍待宿主验收。

> 2026-09-08 Q6-04 隔离发布链已验收：旧 Worker 缺少构建身份、或任一侧为 `+unknown` 时，`WorkerLauncher` 按版本/协议继续兼容并记录“身份未验证”；只有两个已知身份不一致才拒绝复用。定向构建身份测试 `3/3` 通过；正例包六个程序集同源，旧插件/unknown/脏树/无 Git 负例均在成功提示前停止，环境变量恢复已验证；最终文档提交后的 HEAD 已重新打包。真实 Playnite 安装和录屏不在本阶段范围内。

> 2026-09-08 X2-03 构建身份底座及本轮真实安装已完成：公共版本仍为 0.6.73；打包脚本会把当前 Git HEAD 写入程序集 InformationalVersion，并在包前读取实际 DLL 做同源校验。旧构建/unknown/不可达分类由 WorkerLauncher 代码与回归契约覆盖，真实不可达也已通过受控停止验证；最终身份必须以最后一次打包输出为准。

> 2026-09-07 X2-02 运维总览已完成：维护页诊断概览统一显示巡检、云端关注项和清理隔离账本的上次验证、下次尝试与逐项动作；云端动作可回到真实队列记录，隔离账本再次协调需明确确认且按 EntryId 单条执行。Release 构建、全量 Core/Worker/Playnite 测试、XAML/WPF 静态检查和双主题多尺寸 `render-qa` 均已通过。真实 Playnite、DPI/高对比度、完整键盘和真实故障注入仍待用户环境验收。

> 2026-09-07 X2-01 工作区状态体验已完成：媒体当前列表、媒体收件箱和维护诊断共享 Loading/Ready/Empty/Stale/Error/Offline 状态；刷新失败保留旧数据、选择和编辑草稿，并提供过期详情与真实重试入口。Release 构建、全量 Core/Worker/Playnite 测试、XAML/WPF 静态检查和双主题多尺寸 `render-qa` 均已通过。真实 Playnite、DPI/高对比度和完整键盘仍待用户环境验收。

> 2026-09-07 设置页操作反馈已完成：保存状态由编辑指纹和校验结果驱动，矮窗口不再隐藏状态；Playnite 保存/取消边界保持不变。下一步进入动效一致性与真实宿主/DPI/键盘验收。

> 2026-09-07 收尾更新：Q4-04 动态分页一致性已完成；云端队列和归类历史已接入持久化修订号与 stale-token reset。后续按 [Q4-00～04 与 UI 优化阶段](ai/QUALITY_REVIEW_2026-09-07.md) 进入设置页与动效/真实宿主验收，不重复实现已完成的分页入口。

> 这是 GameSaveCenter 的跨电脑、跨模型持续维护入口。任何新的 agent、模型或开发者接手前，先完整读取本文件，再读取项目记忆、开发进度和 UI 规则。不要只依赖聊天记录。

> 新会话短入口：先读 [`docs/ai/CURRENT_STATE.md`](ai/CURRENT_STATE.md)。本文下方的历史交接按时间保留；除顶部最新阶段和明确标注的覆盖关系外，旧条目只用于追溯，不得覆盖当前事实入口。

## 2026-09-07 动效门控已收口，真实宿主仍待验收

- 关闭动画设置或 Windows 动画偏好后，Dashboard 会在刷新视觉资源时清理自身已知过渡；生产 Shell 的 `NormalizeMotionIfDisabled()` 会取消侧栏活动时钟并立即应用最终宽度/透明度/位移。
- 动效门控定向测试 `7/7`，其中包含真实 STA WPF `Window` 的侧栏收起行为；全量 Core `72/72`、Worker `300/301`（1 跳过）、Playnite `363/425`（62 跳过），构建 0 错误，保留 1 个 NuGet `NU1900` 网络警告。
- 不要将离屏测试或一次 Dispatcher 测量写成真实宿主流畅度结论。最近一次 `render-qa` 已全量通过，且最新 `0.6.73.pext` 已重新打包并校验，但临时渲染输出已清理、包未安装。下一步仍需在 Playnite 中验证主题切换、侧栏/详情展开、关闭动画、高对比度、DPI、键盘焦点和实际帧率。

## 2026-09-07 Q4-00 媒体分页锚点行为已收口

- `MediaCenterView` 的延迟恢复已使用上下文代际；游戏、收件箱模式、ViewModel、页面生命周期或新的集合/选择上下文变化都会使旧回调失效。旧回调不能显示过期锚点提示，也不能覆盖当前选择或滚动状态。
- `selectionRestoreQueued` 在恢复重试期间保持锁定，只在恢复成功、显示不可恢复提示或统一失效清理时释放；Attach/Detach 同步维护 ViewModel 的 `PropertyChanged` 订阅。
- 已增加真实 STA WPF `Window` 行为测试，覆盖上下文失效后的旧回调和锚点被窗口裁掉的提示/解锁行为；媒体锚点定向测试 `5/5` 通过。
- 当前全量验证：Core `72/72`、Worker `300/301`（1 跳过）、Playnite `362/424`（62 跳过），Release 构建 0 错误，源码校验/XAML `19/19`/差异检查通过。真实 Playnite、DPI/高对比度、完整键盘和大库连续滚动仍需用户环境验收；本阶段没有 XAML 改动，不伪造渲染证据。

## 2026-09-07 Q5-01 设置页操作反馈已收口

- `GameSaveCenterSettingsView` 的 `SettingsSaveHintText` 始终显示状态：校验失败优先，其次是未保存修改，最后是已保存；Tooltip 明确保存/取消仍由 Playnite 设置宿主处理。
- `GameSaveCenterSettings` 用排除 `DeviceId` 的编辑指纹识别脏状态，`SettingsCommitted`/`SettingsReverted` 在 Playnite `EndEdit`/`CancelEdit` 后更新视图基线；Portable 导入不会因 DataContext 重新绑定而伪造“已保存”。
- RenderHarness 双主题及多尺寸通过，矮窗口状态可见；完整测试与构建通过（Core `72/72`、Worker `300/301`、Playnite `360/422`），只有无法访问 nuget.org 漏洞服务的既有 `NU1900`。不要把该离屏证据描述成真实 Playnite 渲染。

## 2026-09-07 Q4-04 动态分页一致性已收口

- `query_revisions` 在 Worker 初始化中幂等创建，触发器覆盖两类分页实际参与排序/聚合的数据源；持久化 token 不依赖动态 CTE 生成的当前时间。
- `CloudTransferStatusDto`、`MediaClassificationHistoryDto` 的请求/响应已携带 `ConsistencyToken`、`PageResetRequired`、`PageResetReason`。Worker 对读前 stale token 和读期间修订变化都返回 reset，避免错误地继续 OFFSET。
- Dashboard 翻页 reset 会清空过期窗口、提示“列表已发生变化”并自动从第一页重载一次；选择仍只按稳定 TransferKey/BatchId 恢复。新增 Worker stale-token 和旧库迁移回归已通过。
- 验证：Core `72/72`、Worker `300/301`（1 跳过）、Playnite `358/420`（62 跳过），构建无错误、XAML 19/19、源码/XAML/WPF 检查通过。真实 Playnite、跨进程写入和人工键盘仍需用户环境验收。

## 2026-09-07 Q4-03 紧凑维护页已收口

- 维护页诊断/进程映射在紧凑 PageHost 默认只显示主列表；详情由“查看详情 ›”按钮显式展开，详情拥有有限高度和单一滚动容器。选中项变化会关闭详情，Esc 可收起，宽屏继续并排显示。
- `RenderHarness` 的 `RunProductionShellMaintenanceProbe` 使用真实 Production Shell 的 PageHost 几何，检查 1040×700、1100×720、1366×768 的可见完整行数、关闭/打开状态和截图。离屏证据已通过，不能替代真实 Playnite 宿主验收。
- Q4-03 已由 `0fc31ce` 本地提交；`.tmp/q4-03-render-final` 等临时渲染目录已清理。不要恢复旧的自动展开详情。

## 2026-09-07 Q4-01/Q4-02 媒体重试与目标标签导航

- 媒体云端重试已独立为 `media.cloud.upload.retry`，请求 `MediaCloudRetryRequestDto`，Worker 返回 `MediaCloudRetryResultDto`；不要恢复此前对媒体调用 `SyncMedia` 的实现，也不要把媒体请求接到备份专用 `cloud.upload.retry`。
- 重试仅复制已有媒体归档。策略关闭返回 `PausedByPolicy`；安全模式、全局云端关闭、Rclone 未配置或后台失败/取消返回 `CannotSubmit`/准确任务信息；只有 `Submitted` 才能显示已提交。已有自动重试 `RetryCloudUploadAsync` 和备份路径保持不变。
- `MediaCenterView` 的 TabControl 绑定 `MediaTabIndex`，`SaveCenterView` 绑定 `SaveTabIndex`。首页待归类入口先设 0，存档路径诊断入口先设 1，再切换工作区；不要用模拟点击或延时。
- 阶段验证：Release 构建 0/0；Worker 媒体定向 10/10；Playnite 媒体重试/锚点契约定向 4/4。真实 Playnite、Rclone 远端、DPI、高对比度和用户数据仍未验收。

## 2026-09-07 UI3-07 缓存窗口翻页与滚动锚点

- 当前游戏媒体和媒体收件箱的“加载更多”按钮会在命令执行前捕获可见首项、偏移和多选 ID；`MediaCenterView.xaml.cs` 在集合 Reset 后恢复 `VirtualizingWrapPanel`/DataGrid 的对应滚动模型和仍保留的选择。编辑对象/草稿仍由 Dashboard ViewModel 持有。
- 2000 项窗口裁剪掉锚点时显示“列表窗口已前移，当前位置不可恢复”和“返回最新”，由两个新的 reload 命令重新载入首批；不把旧位置或已裁掉的 ID 当作仍可用。
- 批量操作语义已固定为当前保留窗口，未归类/已忽略模式有各自的选中 ID 集合；窗口外选中项不参与当前批量命令。不要改回“全选覆盖已淘汰项”或在页面层偷偷请求全部历史项目。
- 验证：构建 0 warning/0 error，Core `72/72`、Worker `296/297`（1 跳过）、Playnite `355/417`（62 跳过），XAML 19/19，源码校验/WPF 0 error/RenderHarness `render-qa OK`/差异检查通过。真实 Playnite、DPI/高对比度和大库连续滚动尚未运行。

## 2026-09-07 UI3 完成质量评估与后续计划

- 先读 [质量报告](ai/QUALITY_REVIEW_2026-09-07.md)：冻结复核 `b0aa85a`，UI3-07 审阅时仍在并发编辑，待提交后另验收。
- 已有 UI3 功能主体无需重做；后续按报告先收口重试/导航语义、维护紧凑列表与动态分页，再做任务密度、设置首屏和动效。文档内给出文件、复现边界和验收条件。
- 隔离 Release 与现有全量测试通过（Core 72；Worker 296+1 跳过；Playnite 352+62 跳过），原始截图/报告已保留；这不代表真实 Playnite 和 UI3-07 已验收。

## 2026-09-07 UI3-06 存档与维护详情可读性和受控入口

- 存档历史表已采用月日+时分，Tooltip 保留完整时间；类型/状态列在窄屏有优先宽度，备注仍为弹性列。版本 Inspector 先呈现恢复可用性和隔离校验风险，再呈现备注、锁定和恢复动作，现有安全恢复流程未改变。
- 维护诊断表由五列收敛为等级/游戏/问题三列；详情和建议处理在 Inspector 中可滚动查看。`FindingNavigationResolver` 只为可识别诊断提供一个动态按钮：`进入存档路径确认`、`查看失败任务` 或 `打开云队列`，并传递游戏身份/失败筛选语义；不识别时不展示按钮。
- 显示映射已收口：云端汇总区分校验失败/上传失败/混合失败，云端和媒体归类未知状态回退中文，设备页移除固定“2 台设备”文案。枚举存储值和 IPC 契约没有改动。
- 回归证据：`dotnet test GameSaveCenter.sln -c Release --no-restore -m:1` 通过，Core `72/72`、Worker `296/297`（1 跳过）、Playnite `352/414`（62 跳过）；XAML 19/19、源码门禁、WPF 静态审查 0 error、RenderHarness `render-qa OK`、差异检查均通过。真实 Playnite 宿主尚未运行，人工边界仍包括目标机 DPI、高对比度、真实大库和实际用户数据。
- 下一阶段按 `UI_REVIEW_V3_2026-09-06.md` 进入 UI3-07 缓存窗口与滚动锚点；不要恢复旧诊断五列断言或把离屏报告当作真实宿主验收。

## 2026-09-07 UI3-05 首页优先级与明确下一步动作

- 首页 Hero 已从固定警告文案改为真实快照驱动的单一最高优先级状态。`OverviewPriorityResolver` 的顺序是 Worker 不可用、首次环境准备、云端异常队列、待归类媒体、游戏关注、健康刷新；每个状态有明确主按钮和导航命令。
- `OpenMediaWorkspaceCommand` 进入媒体工作区；云端使用现有队列明细；维护/关注路径保留既有命令和事件。首页工具栏和当前游戏卡不再重复放关注入口，风险卡保留一个“打开维护中心”上下文动作。
- RenderHarness 已同步 `Snapshot.CloudTransfers`，首页 1040×700 与 1366×768 实际显示“4 项云端任务需要处理 / 查看云端队列”，当前游戏卡只有“立即备份 / 刷新详情”。
- 验证基线：定向优先级测试 `5/5`；全量 Core `65/65`、Worker `296/297`（1 跳过）、Playnite `348/410`（62 跳过）；Release 构建 0 warning/0 error，XAML `19/19`、源码校验、WPF 静态审查、`render-qa OK` 和差异检查通过。未运行真实 Playnite 宿主。
- 下一阶段按 UI3-06 收口存档/维护页面文本、入口和分页后的上下文锚点；继续保留真实宿主、DPI/高对比度和实际用户数据操作作为人工验收边界。

## 2026-09-07 UI3-03 媒体归类批次历史与可找回撤销

- Worker 新增只读 `media.classification.history`：SQLite 分页聚合批次状态与条目计数，支持状态筛选，返回 `IsUndoable` 和 `LastError`；没有新增业务写入或改变文件安全边界。
- 客户端历史加载支持取消/代际、刷新、继续加载和选中项恢复。预览不再污染最后应用批次；应用结果建立撤销候选，撤销结果会刷新历史，部分冲突以 `UndoneWithConflicts` 留存。
- `MediaCenterView` 检查器现包含归类批次历史和筛选/加载/撤销入口，列表有限视口下限 236 DIP；RenderHarness 已覆盖代表性历史状态。静态/WPF/Render QA 均通过，离屏证据不等同真实 Playnite 宿主。
- 本阶段测试应记录为 Core `65/65`、Worker `296/297`（1 跳过）、Playnite `342/404`（62 跳过）若全量结果保持当前基线；真实 Playnite、跨重启宿主交互、DPI/高对比度和用户大库仍为人工复核项。
- 下一步优先按 UI3 方案继续任务中心密度/可见状态复查；不要重复 UI3-02 的云队列入口，也不要把 `render-qa OK` 写成真实宿主验收。

## 2026-09-07 UI3-04 任务中心紧凑筛选与主表密度

- 紧凑态主工具栏只保留搜索、状态、刷新；类型、游戏、历史范围和时间范围由 `TaskMoreFiltersExpander` 承载。`TaskCenterView.xaml.cs` 通过可逆 reparent 使用同一组真实控件，宽度恢复后必须回到 `TaskFiltersPanel`，不能新增重复 Binding 控件。
- 收起时 `TaskActiveFiltersSummary` 显示当前生效条件，`ClearTaskFiltersCommand` 始终可找；任务时间列使用 `MM-dd HH:mm` + 完整 Tooltip，关键状态/进度列最小宽度不可被详情列挤压。堆叠模式仅使用页面专用 36 DIP 行样式，宽屏恢复共享样式。
- `FakeDashboardData` 已补齐任务统计、范围、分页和详情操作命令；响应式测试覆盖紧凑/宽屏来回切换。验证基线：构建 0 warning/0 error，Core `65/65`、Worker `296/297`（1 跳过）、Playnite `343/405`（62 跳过）、XAML `19/19`、源码/WPF 检查和 `render-qa OK`。
- 离屏最终结果：1040×700 紧凑页三行完整可读并保留详情按钮，1366×768 宽屏首屏五行；这不是实际 Playnite 宿主验收。下一步按 UI3-05 处理首页最高优先级状态与明确下一步动作，之后再做 UI3-06 存档/维护文本和入口收口。

## 2026-09-07 UI3-02 云端队列明细分页与用户操作入口

- 维护中心新增云端队列页，概览卡的“查看明细”进入该页；生产 VM 已接 `GetCloudTransferStatus`、`VerifyCloudTransfer` 和按内容类型区分的重试路径。摘要来自全量聚合，明细每页最多 100 条，支持状态/类型筛选和继续加载。
- UI 事实边界：`Uploaded` 只表示上传命令成功，`RemoteVerified` 才表示远端 check 成功；check 为只读操作；认证失败保持“认证需处理”，不能通过 UI 误变为普通自动重试。媒体记录不能调用只针对备份的 `RetryCloudUpload`，必须走媒体同步上传路径。
- 取消/竞态边界：云队列请求有代际和独立 CTS，筛选/刷新会让旧页失效，离开 Maintenance 会取消请求；宽屏 Inspector 与表格并排，紧凑屏由“查看详情”按钮打开堆叠详情。RenderHarness 已调整旧维护页 Tab 索引并覆盖新页。
- 当前验证：`dotnet test GameSaveCenter.sln -c Release --no-restore -m:1` 全部通过（Core 65，Worker 295/296，Playnite 342/404）；XAML 19/19，`validate-source.py`、WPF 静态审查、`render-qa OK`、`git diff --check` 通过。未运行真实 Playnite 宿主、真实 Rclone 远端、DPI/高对比度或用户大库。
- 下一阶段：UI3-03 媒体归类批次历史与恢复入口。不要重复实现 UI3-02，也不要把离屏 RenderHarness 结果写成真实宿主 1:1 验收。

## 2026-09-07 UI3-00/01 媒体收件箱可见性与紧凑布局

- 媒体收件箱已完成首轮可见性修复：共享页面滚动承载页内容，`MediaInboxGrid` 保持有限高度和 Recycling 虚拟化；选择/视图、目标/主操作、次级动作分层，Inspector 在窄宽度下由“查看预览与归类”按钮打开，业务命令与安全语义未改。
- RenderHarness 现在把真实 PageHost 测量高度传入响应式布局，并用生产 `AcrylicProductionShellView` 验证 1040×700、1100×720、1366×768；布局审计检查祖先裁剪后的实际可见交集、表格首屏高度和动作控件命中区域，而非只检查元素存在。
- 当前交付证据：[`docs/design/reviews/2026-09-07-ui3/`](design/reviews/2026-09-07-ui3/) 的同夹具前后截图；RenderHarness `render-qa OK`，UI audit 无 HIGH/Fidelity/失败路由，源码门禁和 Playnite 测试通过。静态审查的既有 warning/info 不等于本阶段新增错误。
- 尚未完成：真实 Playnite 宿主渲染、DPI/高对比度、用户大媒体库连续滚动；下一阶段按 UI3-02/03 接云队列明细分页入口和可找回归类批次。不要把离屏壳层夹具描述成真实宿主验收。

## 2026-09-06 修复 FLiNG 后台下载 403

- FLiNG 详情页可访问但下载文件返回 403；Worker 原请求没有复用详情页 Cookie，也没有携带详情页 Referer。现已在 `FlingTrainerCatalogSource` 中维护 CookieContainer，发送浏览器风格请求头，先预热对应官方 `/trainer/` 详情页，再按同一会话下载。
- 下载请求继续执行 FLiNG HTTPS 域名白名单和重定向最终地址校验；403 转换为 `FLING_DOWNLOAD_FORBIDDEN`，任务页可给出明确站点拒绝提示。没有放宽非 FLiNG 地址、HTTPS 或文件大小限制。
- `GameToolService` 现在从下载开始到安全解压/工具绑定结束统一 finally 清理临时文件，下载阶段失败不再遗留 `.download` 文件。
- 回归已验证两步请求顺序、Referer、浏览器 User-Agent 和文件落盘；Release 全量通过：构建 0 warning/0 error，Core `65/65`、Worker `295/296`（1 跳过）、Playnite `341/403`（62 跳过）、XAML `19/19`，源码/XAML/差异门禁通过。
- 未执行真实 Worker 在线下载；如果 FLiNG 后续出现 JS/验证码挑战，需要用户在浏览器完成验证或使用浏览器下载，不能通过后台绕过站点安全措施。

## 2026-09-06 接下来优先可见 UI

- 最新审阅入口：[UI_REVIEW_V3_2026-09-06.md](ai/UI_REVIEW_V3_2026-09-06.md)，基线 `0ac0e39`；含截图、代码依据、实施顺序与验收。此次只改文档，UI3 任务未实施。
- 首先修渲染夹具/可见性门禁和媒体收件箱窄屏；随后把云队列分页与归类批次找回接成生产入口，再改任务/首页/存档/维护。不要将任务夹具空字段误当成生产绑定故障，也不要把 `render-qa OK` 当作截图无问题。
- Release 全量构建无警告/错误，Core 65/65、Worker 294 通过/1 跳过、Playnite 341 通过/62 跳过，离屏报告 OK，真实宿主仍待复核。原始代表截图与报告随文档保存，没有操作用户存档或安装插件。

## 2026-09-06 V2-07 媒体多页增量累积与有界窗口

- 媒体页面首个游标页替换集合，后续页使用 `MediaPageAccumulator` 的不区分大小写 ID 索引进行增量更新/追加；重复 ID 只保留最新数据，避免每页重建已加载集合。
- 主媒体、未归类和已忽略列表各自默认保留 2000 项，并在裁剪时保留当前选中项；分页总数、游标和继续加载语义不变，摘要显式展示当前保留窗口。
- 批量集合更新每页只发一次 Reset 通知；新增 250 页压力回归和重叠更新回归，覆盖 5 万条输入、窗口上限、选中项保留和无重复更新。
- Release 全量通过：构建 0 warning/0 error，Core `65/65`、Worker `294/295`（1 跳过）、Playnite `341/403`（62 跳过）、XAML `19/19`；源码校验、XAML 检查和差异检查通过。
- 仍待人工：真实 Playnite 大媒体库首屏与连续滚动、虚拟化回收、主题/DPI、目标机内存/帧率和跨进程并发；本阶段没有写入用户数据。下一步可按需要进入 X2 扩展或宿主手工验收。

## 2026-09-06 V2-06 云队列全量摘要与独立分页

- 云端摘要查询在 SQLite 内统一合并新队列、旧备份重试表和游戏/媒体基础状态，按 `transfer_key` 去重且新队列优先；总量、各状态计数和最早重试时间不再受最近 1000 行窗口影响。
- 明细接口接受 `Page/PageSize/State/Kind`，每页最多 100 项，并返回 `LoadedCount/HasMore`。无参数调用仍返回首页，Dashboard 和维护报告无需改变调用方式；按状态过滤可定位未出现在首页的失败记录。
- 状态排序保持认证、校验失败、上传失败、重试和运行中优先，重试按下次尝试时间排序；旧 `cloud_retry_queue` 与新 durable 行同键时只保留新行。
- V2-06 回归覆盖 1000+ 队列、旧新同键、最早重试、末页和过滤；Release 全量通过：Core `65/65`、Worker `294/295`（1 跳过）、Playnite `339/401`（62 跳过）、XAML `19/19`，源码门禁和差异检查通过。
- 真实 Playnite、大库 UI 和跨进程并发仍待人工复核；下一项按复查包进入 V2-07。

## 2026-09-06 V2-05 云端校验终态与代际保护

- `cloud_transfer_queue` 记录 `Upload`/`Verify` 操作类型、操作 ID 和校验前快照。校验在等待全局传输闸门前持久化为 `Verifying`，取消、异常和工具失败均恢复原上传状态或留下明确校验失败，避免永久 `Transferring`。
- 校验收尾通过操作 ID CAS 保护；较新的上传接管后，迟到校验返回 `CLOUD_CHECK_SUPERSEDED`，不覆盖上传状态，也不触发任何本地/远端删除或上传。
- Worker 启动恢复仅处理 Upload 型 `Pending/Transferring`；Verify 型 `Verifying` 恢复快照或 `CheckFailed`，不会把纯校验恢复为上传任务。游戏行云端状态投影失败只记日志，不反向修改已确认的队列终态。
- V2-05 回归覆盖取消、工具异常、check 失败、上传代际竞争、重启恢复和旧库迁移；Release 全量通过：Core `65/65`、Worker `293/294`（1 跳过）、Playnite `339/401`（62 跳过）、XAML `19/19`，源码门禁和差异检查通过。
- 真实 Playnite、真实云端凭据/断网、硬杀和跨进程并发仍待人工复核；下一项按复查包进入 V2-06。

## 2026-09-06 V2-04 IPC 请求身份与重放指纹

- `ipc_request_ledger` 保存协议版本和规范化 JSON 负载指纹；同一个 `RequestId` 必须同时匹配 type、协议版本和 payload，属性顺序变化仍可重放。
- ID 内容不一致返回 `REQUEST_ID_CONFLICT`，缺少 ID 的受保护写请求返回 `REQUEST_ID_REQUIRED`；两者均不执行也不重放旧响应。旧账本没有指纹时按冲突处理。
- 迁移自动补齐 `protocol_version`、`payload_hash`；完成行 7 天后按 256 行批次清理，中断行保留 30 天；Named Pipe 服务每小时低频执行终态维护。
- IPC/迁移定向 `20/20`；Release 全量通过：Core `65/65`、Worker `288/289`（1 跳过）、Playnite `339/401`（62 跳过）、XAML `19/19`，源码门禁和差异检查通过。
- 真实 Playnite、跨版本组合和硬杀端到端重放仍待人工复核；下一项按复查包进入 V2-05。

## 2026-09-06 V2-03 健康巡检候选公平与恢复游标

- 健康巡检候选按 `PlayniteId`、`CreatedUtc`、`BackupId` 稳定排序，以完整游戏/备份身份轮转；持久化 in-flight 候选会在重启后优先恢复。
- 候选身份在会话检查、游戏锁和归档读取前先通过执行状态写入落盘。写入失败不会开始归档检查，也不会将归档标为 Failed 或创建健康 finding。
- 被运行中游戏或操作锁推迟的候选写入 `health_inspection_deferred_candidates`，包含独立的下次尝试时间；本轮可切换到其他候选。所有候选新鲜时返回 `UpToDate`。
- 定向健康巡检 `11/11`；隔离 Release 全量通过：Core `65/65`、Worker `285/286`（1 跳过）、Playnite `339/401`（62 跳过）、XAML `19/19`，源码门禁和差异检查通过。
- 真实 Playnite、硬杀/断电、跨进程并发和长时调度仍待人工复核；下一项按复查包进入 V2-04。

## 2026-09-06 V2-02 健康巡检调度与计划写入

- 后台健康巡检在手动巡检持有 `_runGate` 时不再紧循环；争锁失败等待 250ms，外围调度/存储异常等待 1s 后继续，停止令牌可以结束等待。
- 计划字段与执行字段不再共用整行写回：`UpdateHealthInspectionPlanAsync` 只改配置和 `NextDueUtc`，`SaveHealthInspectionExecutionStateAsync` 只改状态、游标、计数和时间；完成时以最新计划计算下次巡检时间。
- 定向健康巡检测试 `6/6`，隔离 Release 全量构建/测试通过：Core `65/65`、Worker `280/281`（1 跳过）、Playnite `339/401`（62 跳过）、XAML `19/19`，源码门禁和差异检查通过。
- 真实宿主长时调度和目标机资源压力仍不能由本阶段自动证据替代。

## 2026-09-06 V2-01 媒体归类提交与恢复协调

- 媒体归类/撤销新增 `media_classification_operations` 文件意图账本，记录源/目标路径、内容哈希、源是否为原始捕获文件和目标是否预存；移动后媒体行、批次条目和账本状态在一个 SQLite 事务内提交，持久化提交点明确为 `Committed`。
- `WorkerInitializationService` 在任务恢复前调用媒体操作协调。启动会处理 `Planned`/`Moved`/`RecoveryRequired`：业务已提交则补齐账本，业务仍未提交且文件可验证则恢复原布局并标记 `Aborted`，混合或哈希不一致则只保留 `RecoveryRequired`。
- 审计失败不再触发文件回滚；结果会提示审计写入失败并记录日志。用户取消后的文件补偿使用独立令牌，原始捕获文件只作为复制源，不能被归类/撤销流程删除或移动。
- 回归已覆盖批次更新故障、审计故障、移动后取消及原有正常应用/撤销/冲突；隔离 Release 全量验证为 Core `65/65`、Worker `278/279`（1 跳过）、Playnite `339/401`（62 跳过）、XAML `19/19`，源码门禁和差异检查通过。
- 未完成项仍包括真实 Playnite、真实用户媒体目录、进程硬杀/断电和跨进程并发；不要把本阶段临时 SQLite 触发器测试写成宿主或真实文件系统验收。

## 2026-09-06 下一阶段复查任务入口

- 读 [第二轮复查与实施包](ai/FOLLOWUP_REVIEW_2026-09-06.md)。原 R/U/F/E 主体已有实现，新任务使用 V2/X2 编号；本轮仅文档，应用仍为 0.6.73。
- 优先处理媒体归类数据库提交后的文件补偿、巡检后台争锁/候选落盘、云端 check 的取消终态；方案区分源码事实与待复现行为。CURRENT_STATE 顶部说明覆盖此前过强的恢复保证。
- 本轮 Release 0 warning/0 error、Core 65/65、Worker 275 通过/1 跳过、Playnite 339 通过/62 跳过、XAML 19/19；独立 Worker 重启/5 项 Named Pipe 行为本次未执行通过，历史 09-05 证据不改写，真实宿主仍待验收。

## 2026-09-05 UI-134 任务中心搜索文字垂直裁切修复

- 用户报告任务中心搜索框输入后文字不可见。根因是生产 `GscWpfUiTextBoxTemplate` 将 TextBox Padding 同时用于外层 Chrome 和原生内容视口；上下 `7 DIP` 被重复扣除，36 DIP 输入框的 `PART_ContentHost.ViewportHeight` 只有约 `5 DIP`。
- 已移除生产模板外层 `Padding="{TemplateBinding Padding}"`，保留任务搜索框现有 `Padding="30,7,38,7"`、高度 `GscButtonHeight=36`、搜索 Binding/占位提示/清除命令以及 Foreground 和 ContentHost 对齐绑定。不要把外层 Padding 加回去，也不要通过增加输入框高度规避。
- 新增真实生产资源加载的 STA 回归测试；Release 定向测试 `126 通过/39 跳过`，源码校验、XAML 检查、WPF 静态审查通过。RenderHarness 双主题、多尺寸、resize `render-qa OK`，修复后任务 viewport 为 `19 DIP`、文字 extent 为 `18 DIP`。
- 以上仍是离屏/WPF 自动证据，不等同真实 Playnite 宿主；真实主题、DPI、键盘输入和焦点需用户复核。

## 2026-09-05 E01 规模性能基线

- 新增 `scripts/e01-scale-baseline.ps1`，仅在显式指定 `full` 或 `stress` 时创建大规模隔离 SQLite 夹具；脚本独立还原/Release 构建 Worker 测试，保存测试日志、`worker-scale.json` 和 `baseline.md`，不会碰当前用户 Worker 或 Playnite 数据。
- full 使用 2,000 游戏/20,000 备份/10,000 任务/5,000 媒体/500 工具，seed `105999 ms`、模拟 `427 ms`；stress 使用 10,000 游戏/20,000 备份/10,000 任务/50,000 媒体/500 工具，seed `242832 ms`、模拟 `1529 ms`。full 游戏查询首查/热查 `17/14 ms`、任务页 `92/3/3 ms`、媒体页 `3/1/0 ms`；stress 为 `83/84 ms`、`93/3/2 ms`、`4/0/1 ms`。两档资源增长、搜索/分页行数、事件订阅残留和原子写临时文件残留均通过。
- 结果只代表隔离 Worker/SQLite 夹具；没有将其描述为真实 Playnite 冷/热首屏、UI 分配、帧间隔或用户目录性能。真实宿主大库、主题/DPI 和目标机仍需人工复核。

## 2026-09-05 E01 行为证据矩阵与独立 Worker 重启验证

- `scripts/e01-behavior-matrix.ps1` 默认在 `.tmp` 生成按业务、IPC、WPF/STA、故障/Soak 分组的测试日志和 Markdown/JSON 汇总；故障/Soak 已包含 `WorkerProcessRestartTests`，`-IncludeRender` 可额外接入 RenderHarness。构建和分组测试均使用隔离输出，不改用户数据。
- 受控真实 Windows 执行结果：业务 `44/44`、IPC `22/22`、WPF/STA `45/45`、故障/Soak `4/4`，Release 0 warning/0 error；完整套件 Core `65/65`、Worker `276/276`、Playnite `343/400`（57 项跳过）。跳过项和离屏证据在报告中保持可见，没有合并为通过。
- 独立进程测试已启动真实 Worker，在随机管道/独立 Mutex/临时 SQLite 中硬停止后重启，确认未完成 Backup 在启动时变为 `WORKER_RESTARTED_RETRYABLE`。另已在不停止用户 Worker 的前提下对当前真实管道执行只读 `system.ping` 并返回成功。双选择器及 Playnite 主题/DPI/睡眠唤醒/退出重启仍需隔离宿主；在此之前状态保持 `MANUAL QA REQUIRED`。

## 2026-09-05 E02 当前事实入口与模块边界文档治理

- 已建立 `docs/ai/CURRENT_STATE.md`，固定当前版本、生产可见 Shell、六个工作区、Worker/Contracts/SQLite 入口、实际主题资源和缺失外部 Demo 的验证边界。新会话先读该文件，再进入历史记忆和阶段日志。
- `AGENTS.md`、根 `docs/PROJECT_MEMORY.md` 和本交接文件已互相链接并声明覆盖关系；旧版 AcrylicFork 路径、已撤销布局和过时绑定建议保留作历史证据，但不能覆盖短入口和最新代码。筛选、虚拟化、命令/Binding、安全/取消和 Playnite 兼容性边界已明确。
- `validate-source.py` 已增加短入口完整性检查。E02 没有生产业务改动，不改变版本或安装状态；自动验证继续不能替代真实宿主。
- 阶段验证：Release 0 warning/0 error；Core `65/65`、Worker `275/275`、Playnite `338/400`（62 跳过）、XAML `19/19`，源码校验与 `git diff --check` 通过。真实 Playnite、DPI、长时压力和目标机媒体目录仍需人工复核。

## 2026-09-05 F03 媒体归类建议与可撤销批量操作

- 已完成媒体 Inbox 的可解释归类预览与持久化可撤销批次。建议输入为来源目录规则、游戏会话时间范围、进程映射和唯一文件名候选；重叠会话、规则冲突、未知时间不会生成可自动应用的高置信目标。
- SQLite `media_classification_batches` / `media_classification_batch_items` 记录预览有效期、原始快照、建议、应用后快照和逐项结果。应用默认只确认 High 建议，并在移动归档副本和更新状态前后做快照校验；手工覆盖或并发变化显示冲突，不覆盖用户状态。
- 应用/撤销只处理归档副本，原始媒体和真实存档不删除；撤销要求当前条目仍保持应用后的 `Pending` 状态，允许 Worker 重启后执行，也允许预览过期后撤销。文件或数据库失败会逐项记录并回滚已移动副本。
- MediaCenter 新入口为“生成归类建议”“应用高置信建议”“撤销上次建议批次”，预览列表有限视口并启用 Recycling 虚拟化；既有手工归类、忽略恢复和媒体分页保持兼容。首版明确不做图像删除、OCR 或云端 AI。
- 阶段验证：Release 0 warning/0 error；Core `65/65`、Worker `275/275`、Playnite `338/400`（62 跳过）、XAML `19/19`，源码校验、WPF 质量审计、`git diff --check` 与 RenderHarness `render-qa OK` 通过。真实 Playnite、真实用户媒体目录、长时大批量和多进程并发仍需人工复核。

## 2026-09-05 F02 云端队列可见与传输策略

- 已完成备份/媒体统一持久化云端队列：`cloud_transfer_queue` 以类型和 Playnite ID 去重，同时兼容旧备份重试表；启动恢复 `Pending/Transferring`，后台按 30 秒扫描两类到期任务，游戏忙时保留原状态等待下一轮。
- Dashboard/维护报告/游戏状态显示待上传、传输中、下次尝试、认证需处理、已上传、已校验、失败等状态及次数/原因/下次时间。`Uploaded` 不等同远端校验，显式 `rclone check` 成功才进入 `RemoteVerified`；认证失败停止自动重试，校验不会删除或覆盖本地副本。
- 设置页新增后台队列暂停、按本地分钟配置的允许时段和跨午夜规则，默认行为为不暂停、全天允许；手动复制/校验不受后台策略限制。未实现带宽上限和运行中游戏限速，原因是当前未锁定可验证的 Rclone 参数版本；仍只允许 copy/check 等安全路径。
- 阶段验证：Release 0 warning/0 error；Core `65/65`、Worker `272/272`、Playnite `338/400`（62 跳过）、XAML `19/19`、源码校验、`git diff --check` 和 RenderHarness 多尺寸/双主题/resize/侧栏探针通过。真实云端凭据、断网恢复、Worker 硬重启和目标 Playnite 宿主仍需人工复核。

## 2026-09-05 F01 备份健康巡检与隔离恢复演练

- 已把现有 `RestoreReadinessService` 组成可配置的周期巡检：SQLite 记录计划、游标、in-flight 标记、结果和最近成功验证时间；启动后发现未完成状态会从当前候选重试，按新备份/过期验证优先，每轮只读一个版本。
- 运行中的游戏和同游戏已有备份/恢复/媒体操作会推迟；归档只在 Worker 自有 `RestoreReadiness` 隔离目录解包、核对 Manifest/哈希和路径，空间预检及隔离目录清理结果可追踪，真实恢复命令不受该入口触发。
- 设置页和维护页均有用户可见入口，失败会进入关注项并写审计；维护报告显示巡检状态。成功只表示该归档在当前读取器中完成隔离校验，不表示游戏内进度或真实恢复成功。
- 真实恢复前会读取目标版本最近的隔离校验结果；已知 `Corrupted/Failed` 返回 `RESTORE_READINESS_FAILED` 并在任何 Ludusavi 调用前拦截，未验证或旧的非 ZIP 版本仍沿用既有显式恢复流程。
- 验证：Release 构建 0 warning/0 error；Core `65/65`、Worker `264/264`、Playnite `338/400`（62 跳过）、XAML `19/19`，`validate-source.py`、`git diff --check`、RenderHarness 多尺寸/双主题/resize/侧栏探针通过。真实 Playnite、硬中断恢复、真实磁盘耗尽和多分卷归档仍需人工验收。

## 2026-09-05 U02 侧栏动画成本与快速操作终态

- 侧栏仍使用现有 Demo 认可的 210ms 宽度过渡与 190ms 内容淡入；动画期间再次点击会从当前实际宽度切换到最新目标，不再静默丢弃操作。旧动画 `Completed` 通过代际令牌失效，禁用动画和卸载会清理所有时钟并立即落到最终 72/270 DIP 状态。
- RenderHarness 新增 2000×1100 离屏布局探针，记录 Measure/Arrange、布局更新和帧间隔，并与无动画原子切换对比。独立 shell 结果为单次 Arrange 约 `9.9ms`、快速往返最终宽度 `270`、原子切换约 `4.0ms`；成本可控，未删除已有过渡。完整多尺寸/双主题/resize RenderHarness 为 `render-qa OK`。
- 真实 Playnite 长列表帧率、2000 游戏/5000 媒体压力、DPI、高对比度和卸载时序仍需人工复核；离屏探针不能替代宿主证据。

## 2026-09-05 U01 任务页视口与状态试点

- 第一阶段只落地 TaskCenter：任务页新增加载中、无数据、筛选无结果、无旧数据失败、保留旧数据失败/刷新中等明确状态。已有任务在失败或刷新时继续显示，并展示更新时间；可直接重试，筛选无结果可清除筛选。
- 低高度布局将次级任务摘要从 `84` DIP 收紧到 `64` DIP，任务列表保持 `236` DIP 最小高度；宽屏列表+Inspector、窄屏紧凑详情入口、列表内部滚动和 Recycling 虚拟化保持不变，未改真实业务命令和绑定。
- 阶段验证：Release 0 warning/0 error；Core `65/65`、Worker `260/260`、Playnite `337/399`（62 跳过）、XAML `19/19`、源码门禁通过；RenderHarness 在浅/深主题、1040×700、1366×768、2560×1440 与 resize transition 均为 `render-qa OK`。尚未取得真实 Playnite 重载后的像素/键盘证据，后续仍需人工复核高对比度、100/125/150/200% DPI、长错误详情和大数据量。

## 2026-09-05 U03 游戏目录来源与新鲜度诊断

- 已完成按 Playnite ID 的只读诊断链：Playnite 插件叠加当前来源存在性，Worker 返回脱敏的描述是否存在、原始安装标志、安装来源、描述同步时间、匹配/最后尝试、备份数量/最近备份及安装目录存在信号；完整本地路径不进入诊断 DTO。
- 新增 `games.diagnostic.get`、`games.descriptor.sync`、`games.match.retry`。同步描述只写一个当前 descriptor，不触发 Ludusavi；重试只处理一个已同步的 Worker 游戏。来源缺失仅显示“来源缺失”，备份和历史保持不变。
- `GamePickerViewModel` 的诊断原因与过滤谓词共用判断；维护页和选择器空状态均可清除搜索/状态/平台筛选，排序不变。普通 `Refresh` 仍走既有全库同步门控，不会因诊断入口扩大全库昂贵工作。
- 阶段验证：Release 0 warning/0 error；Core `65/65`、Worker `260/260`、Playnite `335/397`（62 跳过）、XAML `19/19`、源码校验通过。已跳过项是既有 UI 环境/Named Pipe 能力边界，不代表失败；真实 Playnite 目标机需人工复核来源缺失、Worker 离线、无效路径和安装状态变化。

## 2026-09-05 R07 IPC 取消与写请求结果追踪

- 客户端请求入口增加调用者/宿主生命周期令牌，连接、写入、读取可及时停止等待；取消区分用户取消与宿主退出，超时/管道断开单独报告。详情和媒体分页已接入页面请求取消，旧响应继续由 generation/选中 ID 丢弃。
- 破坏性 IPC 使用持久化 RequestId 账本：同 ID 的已完成请求重放原响应，处理中不二次执行，Worker 重启将未完成记录标为 Interrupted；备份/恢复任务写入 RequestId，任务页可按 RequestId 查 TaskId。账本保留 7 天。
- 阶段验证：Release 0 warning/0 error；Core `65/65`、Worker `258/258`、Playnite `333/395`（62 跳过）、XAML `19/19`、源码校验通过。真实 Named Pipe 5 项行为测试因当前沙箱禁止客户端连接而能力探测跳过；完整 Windows/Playnite 环境应复核取消和重放。

## 后续执行顺序

- R01～R07、U03、U01 任务页试点、U02 侧栏动画试点、F01、F02、F03 和 E02 已完成；E01 自动证据也已完成。真实宿主、云端凭据/断网、Worker 硬中断、长任务断线和媒体大数据量仍是人工验收项，后续不应把这些人工项重复写成自动通过。

## 2026-09-05 R06 媒体按需分页与稳定查询

- 新增 `MediaQueryDto`/`MediaPageDto` 和三类媒体分页 IPC；Worker 采用 `(captured_utc, media_id)` 稳定游标，服务端支持类型、收藏、关键词过滤，`TotalCount` 独立于当前游标，旧列表消息继续兼容。
- 当前游戏、待归类、已忽略列表首批均为 200 条；ViewModel 分别保存模式游标/总数，搜索和筛选重取首屏，提供加载更多，保留选中 ID、批量操作、Item 滚动和 Recycling 虚拟化。SQLite 复合索引有查询计划测试。
- 阶段验证：Release 构建 0 warning/0 error；Core `65/65`、Worker `255/255`、Playnite `333/390`（57 跳过）、XAML `19/19`、源码校验和 `git diff --check` 通过。真实 Playnite 宿主、50,000 项压力、4 MiB 消息边界和长时 UI 性能仍待执行。

## 2026-09-05 R05 游戏筛选下拉框 STA 行为验证

- 新增真实 WPF STA 行为回归：程序化修改关闭状态下的游戏筛选 ComboBox 不会写回共享状态；窗口托管下打开→选择→关闭会通过 `DropDownClosed` 提交最终值。当前无需修改生产提交策略。
- 生产 Shell 与兼容 Dashboard 继续使用 OneWay 显示绑定、`UiFilterSelection.Synchronize` 和 `DropDownClosed` 唯一写回；真实 Playnite UI Automation、字符搜索、Esc、双实例、主题/DPI 仍需人工复核。

## 2026-09-05 R04 任务统计口径与完整历史查询

- Worker 新增全量任务查询契约与 `tasks.page` IPC：状态、游戏、类型、关键词、创建时间半开区间均可服务端过滤，使用 `(created_utc, task_id)` 游标分页；独立摘要聚合不再被最近任务窗口截断。
- Dashboard 云端待处理数来自全量摘要，今日完成按本地日换算 UTC 半开区间统计；任务页增加最近/全部历史、时间范围、加载更多和已加载/总数提示，保留现有任务命令、详情 Inspector、虚拟化和滚动。
- `finished_utc,state` 索引有 `EXPLAIN QUERY PLAN` 自动证据；Release 0 warning/0 error，Core `65/65`、Worker `251/251`、Playnite `331/388`（57 跳过）、XAML `19/19`、源码门禁均通过。真实 Playnite 宿主、长历史性能和跨午夜人工验收仍待执行。

## 2026-09-05 下一轮开发任务包

- 新增 [项目完善实施方案](ai/IMPROVEMENT_ROADMAP_2026-09-05.md)，按 R01～R07、U01～U03、F01～F03、E01～E02 提供 15 个可交接任务。优先处理任务锁释放、全局清理确认/互斥、清理隔离账本，再推进统计、分页、交互与功能增强。
- 该节记录最初的源码审阅交接；R01～R07 已在其后完成，应用版本继续为 0.6.73。现有自动化通过不等于真实宿主行为已经验收。
- Release 0 warning/0 error；Core 65/65、Worker 258/258、Playnite 333 通过/62 跳过；未新增真实 Playnite 或渲染验收。新 AI 应先检查后续提交，避免重复实施。

## 2026-09-04 Worker 描述缓存安装状态陈旧

- 0.6.72 只修复了 WPF 选择器的程序化筛选抢写，用户实测仍有“全部/已匹配/有备份可见、已安装不可见”。
- 0.6.73 已修复真正的数据链：`GameCatalogService` 不再把匹配输入缓存当成完整描述缓存；`IsInstalled`、`InstallDirectory`、Actions 等非匹配字段变化会独立写入 SQLite，不会使已有 Ludusavi 匹配失效。
- 本阶段已通过 Release 0 warning/0 error、Core `65/65`、Worker `235/235`、Playnite `331/388`（57 跳过）、XAML `19/19`、源码/WPF 门禁；0.6.73 已安装到本机 Playnite，日志确认 `GameSaveCenter 0.6.73.0 loaded`。
- 目标机器复核步骤：完全退出 Playnite 和 Worker，安装 [GameSaveCenter-0.6.73.pext](../artifacts/GameSaveCenter-0.6.73.pext) 或对应 Release 包，重新打开后执行一次刷新，再在“已安装”搜索“死亡空间”。本机样本只有 3 个游戏，没有目标条目。

## 2026-09-03 游戏选择器用户选择写回竞态补强

- 用户确认目标游戏行信息已显示“已安装”，但“全部”可以搜索、“已安装”无法搜索。0.6.71 的 `SelectionChanged` 写回仍会把程序化初始化/绑定刷新当成用户选择。
- 0.6.72 已将生产 Shell 和兼容 Dashboard 的筛选写回入口改为 `DropDownClosed`；程序化 `SelectedItem` 对齐不会再反向修改共享筛选状态，并新增“已安装 + 死亡空间搜索”回归测试。
- 交付门槛已完成：Release 0 warning/0 error；Core `65/65`、Worker `234/234`、Playnite `331/388`（57 跳过）；XAML `19/19`、源码/WPF 门禁和 Render QA 通过；本机 Playnite 日志已确认 `GameSaveCenter 0.6.72.0 loaded`。目标游戏当前不在本机三条样本数据中，真实目标机器仍需用户安装 0.6.72 后复核。

## 2026-09-03 游戏选择器双向绑定残留与安装判定补强

- 用户在真实使用中复核到上一轮后仍有“全部/已安装找不到、已匹配/有备份/需处理可见”。这说明仅在加载后同步 ComboBox 仍不足以覆盖 WPF 两套选择器副本的 ItemsSource 重建竞态。
- 生产 Shell 与兼容 Dashboard 的状态、平台、排序 ComboBox 现在只从 `GamePickerViewModel` 单向显示；用户实际选择通过 `SelectionChanged` 明确写回，所有静态/动态首项都不再隐式写回共享状态。
- Playnite 安装状态增加有效本地 Play action/working directory 兜底，Steam URI 不会被当成本地文件；版本升为 `0.6.71`，安装后应在 `extensions.log` 确认 `GameSaveCenter 0.6.71.0 loaded`，并重启 Playnite 使旧 0.6.70 DLL 卸载。
- 本轮交付门槛已完成：Release 编译 0 warning/0 error，全量自动化为 Core `65/65`、Worker `234/234`、Playnite `330/387`（57 跳过），XAML `19/19`、源码/WPF 门禁和 Render QA 通过；本机安装目录与打包暂存 DLL 哈希一致，`extensions.log` 已确认 `GameSaveCenter 0.6.71.0 loaded`。真实嵌入 Playnite Dashboard 和目标游戏仍需用户复核，不能把 ControlledAuditWindow 当成宿主真机证据。

## 2026-09-03 游戏选择器合法过期选中值收口

- 在上一轮移除状态/排序静态 `SelectedIndex="0"` 后，仍需防御 WPF 两套共享选择器各自留下的合法旧选中值；否则控件可能显示“全部”，但 `GamePicker.StatusFilter` 仍是“已安装”。
- 已在 `UiFilterSelection` 增加 `Synchronize`，生产 Shell 与兼容 Dashboard 的加载和动态平台列表恢复均以共享 ViewModel 值同步状态、平台和排序，用户改变选项后仍由 ViewModel 作为唯一状态源。新增 STA 回归测试。
- 自动验证：Core `65/65`、Worker `234/234`、Playnite `329/386`（57 跳过）、Release 0 warning/0 error、源码/XAML/WPF 门禁和 Render QA 全部通过；0.6.70 包已安装到本机 Playnite且 DLL 哈希与隔离构建一致。真实宿主审计未能通过 UI Automation 进入 Dashboard，不能替代用户第二台机器复核。

## 2026-09-03 设置页持久化选择状态收口

- 设置页的备份格式、压缩方式和主题模式此前同时设置静态 `SelectedIndex="0"` 与持久化 `SelectedValue` 双向绑定；WPF 初始化时可能把已保存的 `Simple`、`Deflate` 或深色主题抢回第一项。
- 已移除三处局部静态首项，显式使用 `Mode=TwoWay` 与 `UpdateSourceTrigger=PropertyChanged`，并补充 ToolTip 与 UI Automation 名称；设置模型原有保存/导入契约不变。
- 自动证据：Release 0 warning/0 error、Core `65/65`、Worker `234/234`、Playnite `328/385`（57 跳过）和源码校验通过。当前阶段没有改变页面布局，因此未重复执行渲染截图；真实 Playnite 重新打开设置、切换主题并重启后的配置恢复仍需人工复核。

## 2026-09-03 设备冲突状态与媒体筛选绑定收口

- 设备冲突详情的人工决策和媒体中心的类型筛选不再同时使用静态 `SelectedIndex="0"` 与双向 ViewModel 绑定，已保存的“以本机为准/以远端为准”和上次媒体筛选不会在加载时被重置。
- 设备详情新增远端备份隔离状态卡，持续显示“尚未下载”或已校验的游戏、设备、备份 ID 及有效时间；决策、下载、恢复控件补充 ToolTip 和 UI Automation 名称，真实恢复/覆盖安全语义不变。
- 自动证据：Release 0 warning/0 error、Core `65/65`、Worker `234/234`、Playnite `327/384`（57 跳过）、源码校验和 WPF 静态审计通过。真实多设备、媒体筛选恢复和宿主无障碍树仍需人工复核。

## 2026-09-03 任务中心批量安全重试

- TaskCenter 新增“重试可恢复”按钮；从当前最近任务中筛选失败/取消且已有安全重试路径的任务，同一游戏与任务类型只取最新一条，`BackupAll`/`MediaInbox` 各只执行一次。
- 批量操作先二次确认，逐项调用既有 Backup、MediaSync、CloudUpload 或 MediaInbox 重试分流；单项失败会继续处理其他项，最后汇总成功/失败并刷新任务列表，不扩大 Worker/IPC 协议。
- 自动证据：Release 0 warning/0 error、Core `65/65`、Worker `234/234`、Playnite `326/383`（57 跳过）、源码校验和 WPF 静态审计通过。真实 Playnite 任务中心命中、批量确认和长任务行为仍需人工复核。

## 2026-09-03 Playnite 游戏菜单补充媒体同步

- Playnite 游戏右键菜单新增“同步媒体”，单选和多选均可用；执行前会同步当前游戏描述，复用现有 Worker `media.sync`、媒体开关和云端上传设置。
- 媒体归档关闭时只提示用户，不提交请求；该入口没有新增业务实现或 IPC 协议，仍沿用现有任务和通知链路。
- 自动证据：Playnite `325/382`（57 跳过）和源码校验通过；Release 构建与 Core/Worker 全量回归需在本阶段收口，真实 Playnite 菜单命中仍需人工复核。

## 2026-09-03 游戏级云端状态汇总媒体上传

- Dashboard 的 `GetDashboardGameRecordsAsync` 现在在一次 SQLite 媒体聚合中计算已归类媒体的 `Failed`、`RetryScheduled`、`Pending` 和 `Synced/Uploaded` 数量，并与存档上传状态合并。
- 游戏级状态优先级为失败、等待重试、待上传、已上传；因此存档已上传但媒体失败时，游戏选择器、Overview 和详情页会显示“上传失败”。Inbox/Ignore 媒体不参与游戏级状态。
- 自动证据：新增 Worker 持久化回归后 Worker `234/234`；Release 构建、Core/Playnite 自动化、源码校验和 WPF 静态审计通过。真实 Rclone 远端断网恢复、媒体长传输和宿主显示仍需人工复核。

## 2026-09-02 游戏选择器“全部”状态被抢写

- 用户复核上一轮跨机器修复时发现：某款游戏在“已匹配/有备份”能看到，但切换到“全部”或“已安装”后消失。
- 已确认数据层不是按状态分别查询：Worker 的 Dashboard 聚合 SQL 从 `games g` 全量返回，ViewModel 的“全部”分支也不会排除未安装条目。实际问题是生产 Shell 与隐藏兼容 Dashboard 各有一套绑定同一 `GamePicker` 的 ComboBox；状态和排序静态列表上的 `SelectedIndex="0"` 与双向 `SelectedItem` 绑定在初始化时存在抢写，界面显示值和 ViewModel 实际筛选值可能不一致。
- 已修复：两套 XAML 的状态、排序 ComboBox 删除强制 `SelectedIndex="0"`，以持久化/用户当前的共享 ViewModel 值为唯一来源；动态平台列表仍保留索引 0 和 Loaded 恢复。新增 ViewModel 和 XAML 源码回归测试。
- 自动验证已通过：Release 0 warning/0 error；Core `65/65`、Worker `233/233`、Playnite `325/382`（57 跳过）；源码门禁和 WPF 静态审计通过。需要在真实第二台 Playnite 中安装新包，确认首次打开、选择“全部/已安装”及重新打开面板后目标游戏均可见。

## 2026-09-02 跨机器 Steam 游戏搜索不到

- 根因已确认：GameSaveCenter 的搜索数据来自 Playnite `Database.Games` 和 Worker SQLite 快照，不会直接枚举 Steam 客户端；旧逻辑对 500+ 游戏库在 Dashboard 打开时跳过自动目录同步，第二台机器的空/旧 Worker 缓存因此不会出现新游戏；同时默认“已安装”筛选只看 Playnite `IsInstalled`。
- 已修复：Dashboard 首屏仍缓存优先，但打开后立即同步 Playnite 目录描述；Worker 先持久化整批描述，昂贵的 Ludusavi 匹配继续由节流后台队列处理；适配器在 Playnite 标志为 false 但 `InstallDirectory` 实际存在时按已安装处理。大库回调在 Dashboard 打开后也不再被超大库门禁吞掉。
- 自动证据：Core `65/65`、Worker `233/233`、Playnite `323/380`（57 跳过），Release 0 warning/0 error，源码校验通过，WPF 静态审计 0 error/18 warning/172 info。真实 Playnite 宿主尚未在本机运行，安装新包后需在第二台电脑重启 Playnite/插件并打开一次 Dashboard 复核。
- 边界：若游戏没有被 Playnite 的 Steam 集成导入到 `Database.Games`，本插件仍没有可同步的来源；应先确认 Playnite 本身能看到该游戏，再检查插件的“全部”筛选或重新打开 Dashboard。

## 2026-09-02 媒体收件箱旧加载继续分页

- `DashboardViewModel` 的媒体收件箱加载现在把 `mediaInboxLoadGeneration` 传入分页读取；每一页 IPC 发起前和返回后都会检查代际。
- 页面卸载或切换“待归类/已忽略”后，旧请求最多完成当前已发出的单页，不再继续请求剩余分页，也不会再进入收件箱集合和 UI 状态回写；公共 IPC 的既有客户端超时语义不变。
- 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `232/232`、Playnite `321/378`（57 跳过），源码校验和 WPF 静态审计通过。真实 Playnite 快速切换、卸载和 Worker 重启仍需人工观察。

## 2026-09-01 IPC 长连接读取器的取消等待对象累积

- `BoundedIpcLineReader` 原先用 `Task.Delay(Timeout.Infinite, token)` 与每次底层读取竞争；读取先完成时，该 Delay 和取消注册会一直保留到整个事件监听器取消。
- 现在用 `TaskCompletionSource` 配合一次性 `CancellationToken.Register`，在底层读取完成或取消竞争结束后释放注册；IPC 协议、4 MiB 行上限和超大消息丢弃语义不变。
- 自动证据：阻塞读取取消与可释放注册回归通过；Release 0 warning/0 error，Core `65/65`、Worker `232/232`、Playnite `321/378`（57 跳过），源码门禁通过。真实宿主长时间任务通知连接仍需人工观察。

## 2026-09-01 初始同步取消的令牌源释放竞态

- `DashboardViewModel.CancelInitialSynchronization` 取消页面初始同步时，现在容忍后台同步任务已在并发窗口中先行释放 `CancellationTokenSource` 的情况；`ObjectDisposedException` 只表示任务已经结束，不再影响页面卸载。
- 仍由初始同步任务的 `finally` 负责释放令牌源，避免卸载线程与 `Task.Delay` 注册竞争；代际号失效和正常取消语义保持不变。
- 自动证据：定向 Playnite 源码回归通过；Release 0 warning/0 error，Core `65/65`、Worker `230/230`、Playnite `320/377`（57 跳过），源码门禁通过。真实宿主快速打开/关闭与 Worker 启动竞态仍需人工复核。

## 2026-09-01 原生确认框 UI 线程边界

- `GameSaveCenterPlugin.ConfirmAsync` 的 Playnite 原生对话兜底现在也通过 `TryInvokeUi` 在宿主 UI Dispatcher 内执行；游戏停止、快捷操作等后台续体不会直接从线程池调用 `PlayniteApi.Dialogs.ShowMessage`。
- UI Dispatcher 已关闭或调用失败时按未确认处理，危险动作保持拒绝；嵌入式确认事件和正常确认语义不变。
- 自动证据：定向 Playnite 源码回归通过；Release 0 warning/0 error，Core `65/65`、Worker `230/230`、Playnite `319/376`（57 跳过），源码门禁通过。真实宿主后台事件触发确认框与关闭竞态仍需人工复核。

## 2026-09-01 自动修改器审计异常隔离

- `GameToolService.LaunchAfterDelayAsync` 现在通过 `TryAppendAutoStartAuditAsync` 统一写入跳过/成功/失败审计；审计失败只记录 Debug，不会从脱离任务继续抛出。
- Worker 停止令牌已取消时，自动启动失败按停机取消记录 Debug；正常业务启动失败仍按 Error 记录，审计写入是 best-effort，不影响任务主流程。
- 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `230/230`、Playnite `319/376`（57 跳过），源码门禁通过；真实自动修改器启动、进程权限和 Worker 关闭竞态仍需人工复核。

## 2026-09-01 自动修改器审计写入退出期取消

- `GameToolService.LaunchAfterDelayAsync` 的跳过、成功和失败审计写入现在使用延迟启动任务的取消令牌，不再使用 `CancellationToken.None`；停机取消会被现有取消分支观察。
- 这补齐了 STAB-015 对自动修改器启动链的最后一段生命周期边界；真正的启动失败仍记录 Error 并尝试写审计，正常 Worker 停止不再补写。
- 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `230/230`、Playnite `319/376`（57 跳过），源码门禁通过；真实自动修改器与 Worker 停机竞态仍需人工复核。

## 2026-09-01 游戏会话自动化退出期取消

- `GameSessionCoordinator` 的脱离请求后台任务现在统一使用 `ApplicationStopping`：自动修改器延迟启动、退出备份、退出媒体同步、游玩中定时备份和定时媒体同步均会随 Worker 停止取消。
- `RunSafeAsync` 将 Worker 停止期间的 `OperationCanceledException` 记为 Debug，不再把正常停机写成业务失败；真正的异常仍按原有错误日志处理。会话停止请求本身仍使用调用方令牌完成必要的状态收口。
- 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `229/229`、Playnite `319/376`（57 跳过），源码门禁通过；真实 Worker 忙碌时退出与外部进程取消仍需人工复核。

## 2026-09-01 会话快照退出期取消

- `SavePathDetectionService.BeginSessionCapture` 仍然不等待短暂 IPC 请求，但现在使用 Worker 的 `IHostApplicationLifetime.ApplicationStopping` 作为后台扫描令牌；Worker 停止时会取消存档路径快照，避免 SQLite/文件存储释放后继续回写。
- 取消异常由既有完成回调观察，正常停止不会升级为未观察任务异常；真实 Worker 忙碌时重启、文件扫描取消时机仍需人工复核。
- 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `228/228`、Playnite `319/376`（57 跳过），源码门禁通过。

## 2026-09-01 FLiNG 下载进度写入收口

- 下载进度回调由 `IProgress` 改为可等待异步回调；GameToolService 按百分比变化节流任务进度，避免大文件产生大量并发 SQLite 写入和未观察异常。
- 只有进度通知机制改变，FLiNG 域名校验、下载大小限制、临时文件清理、归档安全解包和工具绑定流程保持不变。
- 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `227/227`、Playnite `319/376`（57 跳过），源码门禁通过；真实 FLiNG 下载速度、网络断开和取消场景仍需人工复核。

## 2026-09-01 公共 IPC 入口的退出期保护

- 插件公共 `RequestAsync` 现在在 `lifetimeCancellation` 取消后返回取消任务；页面操作、右键快捷操作和其他异步续体即使跨过多个 await，也不会在 Playnite 关闭阶段创建新的 Worker 请求。
- 这是 STAB-011 的第二道边界：启动/游戏事件/任务轮询/目录同步有自己的退出检查，公共 IPC 入口负责拦截未被这些专用路径覆盖的调用方。已发出的单个 IPC 不强行中断，晚返回结果仍由各自流程处理或取消。
- 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `226/226`、Playnite `319/376`（57 跳过），源码门禁通过；真实宿主关闭中操作、Worker 重启和 Dispatcher 关闭需人工复核。

## 2026-09-01 Playnite 退出阶段后台生命周期收口

- 应用停止时先取消插件生命周期并停止任务通知计时器，再停止插件自己启动的 Worker；退出后的 Playnite 回调、Fire-and-forget、游戏会话写入、任务长轮询和目录同步不会再创建新 IPC 或 UI 回写。
- 目录同步的信号等待、任务通知的 IPC 返回和终态任务逐项处理均有生命周期检查；正在进行的单个 IPC 请求不能被现有客户端强行中断，但其晚返回结果不会继续驱动退出期状态。
- 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `226/226`、Playnite `319/376`（57 跳过），源码门禁通过。需要人工验证：真实 Playnite 退出/重启、Worker 正在忙时关闭宿主、Dispatcher 关闭竞态。

## 2026-09-01 异步操作返回上下文保护

- 媒体/存档元数据保存会捕获原游戏、原条目和请求时编辑值；返回期间切换游戏或条目时，不会更新当前新列表、清理新编辑器的 dirty 状态或覆盖媒体摘要。
- 重新归类、备份、校验、恢复准备、策略模板、比较/保留预览和进程映射等异步流程的提示与回写均绑定原始上下文；新增的请求只会更新原请求仍可见的页面。
- 自动证据：Release 0 warning/0 error，Core `65/65`、Worker `226/226`、Playnite `318/375`（57 跳过），源码门禁通过。真实宿主快速切换和输入保持仍需人工回归。

## 2026-09-01 详情编辑草稿与游戏摘要刷新保护

- 存档备注/锁定、媒体备注/收藏现在有独立 dirty 状态；详情刷新重新绑定同一条目时保留用户未保存的字段，不会被旧 DTO 覆盖，其他未编辑字段仍会同步最新服务端值。
- 新条目、清空选择和成功保存按预期重置 dirty 状态；媒体批量收藏/备注只清除本次实际更新的对应字段。游戏策略未保存编辑也会在快照刷新时保留。
- 游戏选择器同一条目更新会向 Dashboard/生产壳层转发 `SelectedGame` 通知，标题和平台摘要可随快照更新。
- 策略保存请求会捕获原游戏的 ID、名称和策略副本；保存期间切换游戏不会把基线或成功提示写到新选中的游戏。
- 需人工验证：真实 Playnite 中打开存档/媒体详情或备份策略，编辑输入后触发自动刷新、任务完成刷新和页面切换，确认输入保持且保存后能回读服务端值。

## 2026-09-01 媒体收件箱与详情刷新一致性

- 媒体收件箱切换现在保留最后一次模式请求；若切换发生在其他操作进行期间，会在 `RunAsync` 结束后补加载。请求响应通过模式代际号校验，旧响应不会覆盖当前收件箱集合。
- 收件箱媒体、目标游戏、存档版本和当前游戏媒体在异步刷新后保留当前 ID 选择，找不到原条目时才回退第一项；“已忽略”模式的刷新会实际读取忽略列表。
- Media Inbox 新增 Worker 离线状态面板，离线时不再显示与状态冲突的空列表文案。自动证据：Release 构建 0 warning/0 error，Core `65/65`、Worker `226/226`、Playnite `318/375`（57 跳过），源码/WPF 门禁和 Render QA 均通过；真实 Playnite 快速切换、Worker 重启和 DPI 仍需人工回归。

## 2026-09-01 修改器目录加载状态反馈交接

- 修改器中心的 FLiNG 搜索结果、目录同步和可下载版本读取现在分别有真实加载状态；结果为空时只有在请求完成后才显示空状态，加载期间显示共享加载面板。
- `DashboardViewModel` 的命令、自动读取版本、下载绑定和错误传播未改变；`TrainerCenterView.xaml` 只补充状态呈现与空态条件，未改变页面导航、列表虚拟化或数据契约。
- 自动证据：Release 构建 `0 warning/0 error`，Core `65/65`、Worker `226/226`、Playnite `313/370`（57 跳过），源码校验通过，WPF `0 error/18 warning/172 info`，RenderHarness 双主题/多尺寸/Resize/Shell QA `render-qa OK`。用户已确认当前版本能编译并正常运行于 Playnite；本轮新增状态尚未由 Agent 在真实宿主重新操作，仍需随发布包做一次人工观察。

## 2026-09-01 修改器版本读取竞态与逐项操作交接

- FLiNG 目录结果行的“读取版本”按钮现在带有当前行作为参数；非选中行也会正确切换选择并读取对应版本。
- 版本请求增加代际号、目录 ID 校验和忙碌期间最新选择排队；旧的慢响应只结束自身加载状态，不再写入新选择的 `TrainerReleases`。全局命令保护、下载校验、安全解压和绑定行为未变。
- 自动证据：Release 构建 `0 warning/0 error`，Core `65/65`、Worker `226/226`、Playnite `314/371`（57 跳过），源码校验通过，WPF `0 error/18 warning/172 info`，RenderHarness 双主题/多尺寸/Resize/Shell QA `render-qa OK`。用户已确认当前版本可在 Playnite 正常运行；本轮新增竞态场景仍需在实际宿主中快速切换目录项观察一次。

## 2026-09-01 IPC 媒体边界与任务通知缓存优化

- 当前游戏媒体 `ListMedia` 在 Worker 分发层新增 1–1000 条页大小夹限，避免未来异常请求绕过 IPC 响应边界；现有 UI 的 1000 条读取上限不变，Inbox/Ignored 的 500 条分页规则不变。
- 任务通知去重使用容量 4096 的线程安全 FIFO 集合，替代插件生命周期无界的任务 ID 字典；淘汰旧 ID 只影响极少数历史 feed 重放的重复提示风险，不影响任务、备份或媒体数据。
- 已完成 Release 构建（`0 warning/0 error`）、全量测试：Core `65/65`、Worker `226/226`、Playnite `312/369`（57 跳过）；源码校验通过，WPF 静态审计为 `0 error/18 warning/172 info`。真实 Playnite/DPI/宿主重启和 Rclone 仍是人工验证边界。

## 2026-09-01 FLiNG 目录解析收口

- FLiNG 在线目录和详情页已使用独立纯解析方法，支持绝对、相对和协议相对链接；统一 HTML 解码、HTTPS/FLiNG 主域边界、预期路径校验和规范 URL 去重。
- 目录链接移除追踪查询参数，下载链接保留查询参数但去除 fragment；详情页解析以实际页面 URL 为相对链接基准。外站链接不会进入缓存或下载列表。
- Worker `222/222`、Core `65/65`、Playnite `310/367`（57 跳过）及源码门禁通过。真实 FLiNG 站点、下载、安全软件和 Playnite 宿主仍需人工回归。

## 2026-08-26 UI-335 完整圆角交接

- 标题栏 `GscRedesignHeaderSurface` 已从仅顶部圆角改为四角 18 DIP 圆角、完整 1 DIP 描边和 `ClipToBounds=True`；普通页面共享卡片 `GscRedesignSectionCard` 也统一启用裁剪，避免内部背景造成方形角泄漏。
- 变更只影响共享视觉容器，不改变标题/副标题、顶部按钮命令、Binding、页面数据、滚动或虚拟化；对应源码回归断言已更新。
- 自动证据：源码校验、XAML 19/19、WPF 静态审计、Release 0 warning/0 error，Core `59/59`、Worker `210/210`、Playnite `310/367`（57 跳过），`.tmp/ui-qa-rounded-surfaces-v1/render-qa-report.txt` 为 `render-qa OK`。
- RenderHarness 只覆盖页面内容，不覆盖外层 Shell 标题栏；真实 Playnite、DPI 和宿主截图仍未运行，Phase 4 按用户要求跳过。

## 2026-08-26 UI-334 按钮状态层覆盖交接

- 生产共享 `GscWpfUiButton` 已改为整按钮状态覆盖：`ButtonChrome` 不再承载内容 Padding，Hover/Pressed 层覆盖整个圆角表面，内容间距由 `ContentPresenter Margin={TemplateBinding Padding}` 保持。
- 键盘焦点新增整按钮 `FocusOverlay`，Primary 使用 on-accent 覆盖资源，并保留共享 `GscSharedFocusVisual` 焦点环；命令、Binding、按钮尺寸、文字省略、滚动、虚拟化和主题契约未改。
- 自动证据：`validate-source.py`、XAML 检查、WPF 静态审计、Release 构建/全量测试通过；Core `59/59`、Worker `210/210`、Playnite `310/367`（57 跳过）、`.tmp/ui-qa-button-focus-v1/render-qa-report.txt` 为 `render-qa OK`。
- RenderHarness 不模拟真实 Playnite 外壳或所有键盘交互状态；真实 Playnite、DPI 和焦点实机验收未运行。Phase 4 仍按用户要求跳过。

## 2026-08-26 UI-333 生产标题栏圆角交接

- `AcrylicProductionShellView.HeaderSurface` 已接入共享 `GscRedesignHeaderSurface`；顶部两角为 16 DIP 圆角并裁剪内部材质，底边保持直线，避免标题栏与 PageHost 之间出现尖锐矩形边界或额外布局缝隙。
- 标题、副标题、游戏选择器、刷新/同步/备份命令和响应式布局未变；变更只涉及 `Redesign.xaml`、`AcrylicProductionShellView.xaml` 与 `ProductionShellChromeSourceTests.cs`。
- 自动证据：Release 0 warning/0 error，Core `59/59`、Worker `210/210`、Playnite `310/367`（57 跳过），源码/XAML/WPF 门禁通过，`.tmp/ui-qa-header-corner-v1/render-qa-report.txt` 为双主题、多尺寸、Resize `render-qa OK`。
- 页面 RenderHarness 不包含外层 Shell 标题栏，因此未将页面 PNG 误称为标题栏实机截图；真实 Playnite 未运行，Phase 4 仍为用户明确跳过的人工验证项。

## 2026-08-26 UI-332 按钮组与页面环境材质交接

- 生产页面已收口本轮用户指出的三类视觉问题：远端恢复按钮使用 `GscWpfUiRemoteRestoreButton`，媒体中心当前游戏媒体两处批量按钮使用 `GscWpfUiMediaBatchButton`，修改器下载版本提示使用中性 `GscDiagnosticHintBubble` 与 `GscDiagnosticHintText`。
- `AmbientMaterialLayer` 的页面环境材质现在由共享圆角 `MaterialChrome` 裁剪，默认页面圆角 16 DIP；`AcrylicProductionShellView` 的 Shell 环境层显式为 0 DIP，避免在完整 Shell 内形成额外内凹边界。
- 自动证据：Release 0 warning/0 error，Core `59/59`、Worker `210/210`、Playnite `309/366`（57 跳过），源码/XAML/WPF 门禁通过，`.tmp/ui-qa-button-material-v1/render-qa-report.txt` 为双主题、多尺寸、Resize `render-qa OK`。
- 本轮只改共享样式、控件材质容器和对应页面样式引用，未改命令、Binding、数据契约、滚动、虚拟化或动画。真实 Playnite 未运行，Phase 4 仍为用户明确跳过的人工验证项。

## 2026-08-26 STAB-007 性能测量交接

- Worker 全量数据规模已测完：2,000 游戏、20,000 备份、10,000 任务、30,000 媒体、500 工具，约 3m03s，managedGrowth 0 MiB、handles +0、threads +0；2,000 游戏合成集合基准为 55/2/15ms SetItems、215/196ms 搜索、1/0ms ReplaceAll。
- Blur 20/78/100 DIP 已加入回归；没有改默认 Blur、动画、滚动模型、虚拟化或增加效果。离屏 Render QA 253 样本平均 219.01ms，报告 `render-qa OK`。
- 这些是 Worker/SQLite、合成集合或离屏数据。真实 Playnite 初次打开/切页/侧栏/主题/背景内存/DPI/大库滚动仍是 `MANUAL QA REQUIRED`，Phase 4 依用户要求跳过，禁止用离屏证据替代。

## 2026-08-26 STAB-006 页面树与响应式协调交接

- Phase 5 已完成静态双树治理：可见业务实例来自 `ProductionShellView.PageHost`；旧 Dashboard 页面树保留为兼容面，详情见 `docs/ai/WORKSPACE_TREE_INVENTORY.md`。业务导航、搜索焦点、维护定位、任务动画和生产页面布局不得重新引用旧实例。
- Phase 6 已将现有响应式宽高状态集中到 `ResponsiveLayoutCoordinator`，保持所有既有断点和尺寸；`AcrylicProductionShellView` 的导航、侧栏切换、Resize 延迟回调都统一调用 `ApplyResponsiveLayout`。不可在未测量前修改 Blur、动画、滚动模型或虚拟化。
- 自动证据：Release 0 warning/0 error，Core 59/59、Worker 210/210、Playnite 309/366（57 跳过），源校验/XAML/WPF 门禁通过，`.tmp/phase6-responsive-coordinator-render/render-qa-report.txt` 为双主题与连续 Resize `render-qa OK`。
- Phase 4 由用户明确跳过；不要把上述离屏证据称为真实 Playnite 验收。宿主安装、DPI、键盘焦点、Worker 重启/日志、性能和旧树删除安全性仍是 `MANUAL QA REQUIRED`。

## 2026-08-26 STAB-005 媒体 Inbox 响应超限交接

- 用户实机两端均为 `0.6.70.0`；Worker 在 `11:04:21` 报告 Named Pipe 响应超过 `4194304` 字节，Playnite 随后把 `MESSAGE_TOO_LARGE` 显示成“操作失败”。已确认根因是媒体 Inbox 列表旧路径一次请求 5000 条，用户库有 4615 条 Inbox 媒体。
- 当前修复在 Worker 端把未分配/已忽略媒体限制为 500 条并支持 `Offset`，在 Playnite 端分页读取最多 5000 条；数据库查询按 `captured_utc DESC, media_id DESC` 稳定排序。服务端超限日志新增 `RequestId`、`Type`、`ResponseBytes`、`PayloadBytes`，不能简单调高 4 MiB 上限。
- 代码没有改 XAML、布局、动画或页面数据应用路径；自动验证为 Core `59/59`、Worker `210/210`、Playnite `302/359`（57 跳过）、Release 0 warning/0 error、RenderHarness `render-qa OK`。WPF 静态审计仍为 0 error/18 warning/172 info。
- 用户明确允许本修复跳过 Phase 4。当前未覆盖真实 Playnite 安装后的补丁 Worker 验收：只读探针对现有 Worker 的 500 条请求测得约 745990 字节，隔离补丁 Worker 因全局互斥未启动成功；没有停止用户 Worker 或修改其数据。
- 下一步：用本次构建产物安装到用户实际扩展目录后，打开 Media Inbox/Ignore 页面，确认没有 IPC 超限；若仍有超限，按新增 Worker 日志中的 `Type`、请求 ID和字节数继续定位其他接口。

## 2026-08-26 STAB-003A IPC 真实管道烟测补充

- 隔离 Release Worker 真实启动后，`4194778` 字节超限请求返回 `MESSAGE_TOO_LARGE`；同一 Named Pipe 连接的后续 `system.ping` 成功，证明超限行消费和连接复用路径可工作。
- 受限执行上下文的 Named Pipe 连接会 `Access is denied`，但提升权限后的同一隔离烟测通过；报告为 `.tmp/phase3-ipc-runtime-escalated/runtime-smoke-report.txt`。
- 仅测试结束时强制停止临时 Worker，退出码 `-1` 属于清理动作；没有修改 Playnite 用户数据。Phase 4 真实 Playnite 矩阵仍被缺少宿主环境阻塞。

## 2026-08-26 STAB-004 真实 Playnite 验证阻塞

- 当前环境没有可执行的 Playnite、运行中的宿主或注册表安装入口；`D:\software\Playnite\Playnite\Playnite.DesktopApp.exe` 等历史候选路径也不存在。Phase 4 状态为 `BLOCKED_ENVIRONMENT`。
- 不要用离屏 RenderHarness、静态 WPF 审计或历史截图宣称真实宿主通过；也不要未经隔离证明运行 `real-host-audit.ps1`/`dev-install-run.ps1`，因为它们可能安装扩展、关闭宿主或写入用户目录。
- 下一步如果要完成真实宿主验收，需要用户提供隔离 Playnite 安装/便携目录、独立数据根、唯一进程边界和扩展目录；在用户已明确跳过 Phase 4 的前提下，Phase 5/6 的静态治理可以继续，但不能补写真实宿主通过结论。
- Phase 0–3 的自动验证已提交推送，最新 IPC commit 为 `9d53b1d`；Phase 4 没有源码改动。

## 2026-08-26 STAB-003 IPC 健壮性交接

- 三个 Named Pipe 方向共用 Contracts 的有状态 `BoundedIpcLineReader`；按 4 MiB 字节上限消费消息，超限返回/发送 `MESSAGE_TOO_LARGE`，服务端响应和事件写出同样先检查，不能恢复 `ReadLineAsync` 后再检查。
- 读取器必须按连接实例化以保存 4 KiB 缓冲剩余数据；否则一块内多条消息会被错误丢弃。请求服务端并行槽位 32、事件服务端槽位 8，`CurrentUserOnly`、管道名、协议版本和消息类型保持原值。
- Playnite 请求超时仍向上暴露 `TimeoutException("Worker response timed out.")`；事件取消、JSON 错误和客户端断开继续走原有重连/忽略路径。
- 本轮验证：IPC 定向 `3/3`，全量 Core `59/59`、Worker `201/201`、Playnite `302/359`（57 跳过）、Release 0 warning/0 error，WPF 0 error/18 warning/172 info，最终 RenderHarness `.tmp/phase3-ipc-boundary-render-final/render-qa-report.txt` 为 `render-qa OK`。
- 下一阶段是 Phase 4 真实 Playnite 隔离宿主验证；若环境无法完成，必须明确写 `MANUAL QA REQUIRED` 或 `BLOCKED_ENVIRONMENT`，不能把离屏 RenderHarness 当作宿主证据。

## 2026-08-26 STAB-002 外部进程输出交接

- `ExternalProcessRunner` 每个 stdout/stderr 流最多保留 4 MiB；超限后继续消费但丢弃，返回 `PROCESS_OUTPUT_LIMIT_EXCEEDED`，不能恢复无界 `ReadToEndAsync`。
- `ProcessResult` 保留退出码、标准输出/错误和稳定 `ErrorCode`；`PROCESS_TIMED_OUT` 仍返回超时结果，调用者取消仍抛出取消异常。普通进程失败仍使用原退出码和输出。
- `RcloneClient.RunSafeAsync` 不再有 `workingDirectory` 参数，始终传 null standardInput 给 Runner；不要改变 Runner 既有的可执行文件目录或 Rclone 参数顺序。
- 本轮验证：定向 Worker `6/6`、全量 Core `59/59`、Worker `201/201`、Playnite `302/359`（57 跳过）、Release 0 warning/0 error，WPF 0 error/18 warning/172 info，RenderHarness `render-qa OK`。
- 下一阶段处理 Named Pipe 统一消息上限和边界读取；真实 Rclone/Ludusavi 大输出、Worker 重启和 Playnite 宿主日志仍需人工复核。

## 2026-08-26 STAB-001 Dashboard 事件生命周期交接

- `DashboardViewModel` 的 `PlayniteGameStarted` 订阅由 `PlayniteGameStartedSubscription` 持有，必须通过 `StartPlayniteGameStartedSubscription` / `StopPlayniteGameStartedSubscription` 管理；生产 View 的 Loaded/Unloaded 已成对调用，禁止重新放回构造函数永久订阅。
- handler 在排队 UI 调度前后都检查订阅状态，卸载后的旧回调不能写入页面；pending auto-select 的原有时序和游戏列表未到达时的保留行为不变。
- 本轮没有改 XAML 或页面视觉；验证为生命周期定向 `6/6`、Release Core `59/59`、Worker `201/201`、Playnite `302/359`（57 跳过）、0 warning/0 error，RenderHarness `render-qa OK`。
- 下一阶段应处理 `ExternalProcessRunner` 的 stdout/stderr 有限累积及 `RcloneClient` 参数语义；完成后继续同步本文件、`docs/ai/WORKLOG.md` 与 `docs/ai/PROJECT_MEMORY.md`。
- 真实 Playnite 重载、事件触发、DPI、主题和焦点仍是 `MANUAL QA REQUIRED`，不能用离屏渲染报告代替。

## 2026-08-25 UI-331 共享控件与流畅度交接

- 历史实现曾把样式 Padding 应用到外层 Chrome；当前有效实现由原生 ContentHost 消费 TextBox Padding，外层 Chrome 保持无 Padding。ContentHost 必须保持零 Margin/零 Padding，避免输入文字高度和左右内边距因模板重复计算而漂移。TextBox/ComboBox 保留 `SnapsToDevicePixels` 与 `UseLayoutRounding`。
- ComboBox 选中内容和下拉项统一传递字体族、字号、字重，并使用明确的左对齐；相邻筛选下拉框的 Items、Binding、默认值和命令未改动。
- 侧栏边界控制仍是无文字、32×32 的底部集成控件。宽度 270↔72 DIP 由 210ms `GridLengthAnimation` 驱动；内容层的 190ms 淡入/4 DIP 位移只用于平滑视觉过渡。完成、非动画布局和卸载必须停止/清理动画。
- 游戏背景仍只有 Shell 的一个静态图片层，现增加 `BitmapCache`；不要为了追求“玻璃感”给卡片、文字、表格、列表和 ScrollViewer 挂 BlurEffect。
- 设置路径输入校验已改为 Background Dispatcher 合并通知，避免 `File.Exists` 在每个键盘字符同步执行；验证语义不变。
- 证据：定向 124/39、全量 Playnite 297/57、Release 0/0、WPF 0 error/18 warning/172 info，`.tmp/ui-qa-polish-v1/render-qa-report.txt` 为 `render-qa OK`。真实 Playnite 宿主仍需复核实际点击动画、DPI、帧率与焦点。

## 2026-08-25 UI-330 毛玻璃强度交接

- `GlassStrengthSlider` 仍是 20–100；`AdaptiveThemePalette.BlurRadiusForStrength` 现在直接把百分比映射为 Blur DIP：20→20、默认 78→78、100→100。后续不要再使用 12–34 或 16–34 的压缩范围，否则滑块会再次出现“100 像 20”的观感。
- 真实 Blur 只允许出现在游戏背景图片层和设置页环境层；卡片、文字、表格、列表和滚动区域继续使用共享半透明材质，不挂 BlurEffect。
- 保留关闭玻璃、高对比度、无图/关闭游戏背景跟随时的 null/透明/不透明回退，并且强度变化不能启动新的背景解码。
- 验证证据：WPF 资源定向 119 通过/39 跳过，全量 Playnite 297 通过/57 跳过，Release 0 警告/0 错误，`.tmp/ui-qa-glass-strength-v1/render-qa-report.txt` 为 `render-qa OK`。真实 Playnite 的 100% 帧率、DPI 和视觉观感仍需安装后确认。

## 2026-08-25 UI-329 刷新与布局流畅度交接

- Dashboard 自动刷新不得在选中游戏未变化时重复刷新 Icon 或 Background；`EnsureSelectedGameBackgroundLoaded` 只在页面重新显示且当前背景确实缺失时做一次恢复。不要把背景解码放回普通快照轮询。
- 背景取色只读取五个 1×1 像素，不能恢复整张位图的临时 `byte[]` 复制；如果未来增加采样点，应先评估切换游戏时的分配、GC 和取消语义。
- 生产壳连续 `SizeChanged` 通过 `DispatcherPriority.Render` 合并，筛选平台集合变化通过单个 `Loaded` 调度恢复默认值；不要在拖拽窗口时同步执行全页面响应式重排，也不要重复排队 `DataBind + Loaded` 两套同一恢复工作。
- 本轮保留真实命令、绑定、滚动/虚拟化、侧栏宽度动画和主题材质；卸载时必须清理 pending 标记。`render-qa` 通过不等于 Playnite 宿主已完成帧率、内存、DPI 和实际缩放验收。
- 验证证据：定向 Playnite 26/26，全量 297 通过/57 跳过，Release 0 警告/0 错误，WPF 0 error/18 warning/172 info，`.tmp/ui-qa-performance-v1/render-qa-report.txt` 为 `render-qa OK`。

## 2026-08-25 UI-328 游戏背景跟随开关交接

- 设置新增 `FollowSelectedGameBackground`，默认开启，位于“外观与动态效果”。关闭后不是只隐藏图片：ViewModel 会取消封面加载、清理已解码图像和采样 Brush，重新开启后只加载当前选中游戏。
- 游戏背景资源入口必须保留共享回退：只有开关开启、采样材质存在、毛玻璃可用且非高对比度时才显示底层图片、tint 和 BlurEffect；其他情况恢复主题/Demo 中性材质。
- `AmbientMaterialLayer` 必须以 `UseSelectedGameBackground && HasSelectedGameBackgroundAmbientMaterial` 决定隐藏主题洗色，避免关闭开关后主题环境光消失。设置保存通过既有 `EndEdit -> NotifyVisualSettingsChanged` 链路通知已打开的 Dashboard。
- 验证证据：`validate-source.py`、`check-xaml.ps1`、WPF 静态审查 0 error/18 warning/172 info、Release 0 warning/0 error、Playnite 296 通过/57 跳过、`.tmp/ui-qa-game-background-v1/render-qa-report.txt` 为 `render-qa OK`。真实 Playnite 重载后仍需确认开关即时生效、浅色/深色/Follow、高对比度、DPI 和性能。

## 2026-08-25 UI-327 字体清晰度与维护提示交接

- 共享生产文字排版已从 `Display` 改为 `Ideal`，由生产壳、首页、设置页和 DataGrid 样式统一传递；继续保留 `ClearType`、`Fixed` hinting、`UseLayoutRounding` 和 `SnapsToDevicePixels`。这是针对用户反馈的大号中文笔画像素感，不是把文字做模糊。
- 修改器确认导入页的“主程序”标签和 ComboBox 已放在同一 Grid 行；按钮仍绑定原确认/取消命令。后续若再做窄宽度适配，应保持该标签/输入行的基线关系，不要恢复纵向 StackPanel。
- 维护中心摘要采用 `GscDiagnosticHintBubble` / `GscDiagnosticHintText`，用于诊断、存储、保留策略、镜像和任务摘要；气泡使用信息色低透明度，正文常规次级文字，Severity pill 的状态语义不变。
- 验证证据：`.tmp/ui-qa-font-bubbles-v1/render-qa-report.txt` 为 `render-qa OK`；Release 0 警告/0 错误，Core 59、Worker 199、Playnite 295 通过/57 跳过。未在真实 Playnite 宿主执行本轮人工 DPI/字体像素检查。

## 2026-08-25 UI-326 设置窗口与侧栏折叠交互交接

- 设置页根控件已移除 `MinWidth=1180/MinHeight=760`，避免窗口缩小时被 UserControl 强行撑大。`GameSaveCenterSettingsView.OnLoaded` 会通过 `Window.GetWindow(this)` 对真实宿主执行一次 `EnsureHostWindowSize`：优先约 1280×840，受当前工作区上限约束，设置 `SizeToContent=Manual` 和 Stretch 对齐；后续缩放不再强制回弹，继续由 `ApplyResponsiveLayout` 处理紧凑模式。没有 owner Window 的 RenderHarness 不会触发该逻辑。
- 生产壳侧栏控件位于 `SidebarLayout` 底部行 `SidebarCollapseArea`，共享样式为 `AcrylicSidebarBoundaryButton`。它是透明 32×32 边界控制，不带“收起侧栏”等文字、不覆盖导航内容；`‹` 表示收起，`›` 表示展开，悬停/按下只显示轻量 tint。
- `GridLengthAnimation` 位于 `src/GameSaveCenter.Playnite/Controls/GridLengthAnimation.cs`，210ms、`CubicEase.EaseOut`，驱动 `SidebarColumn.Width` 从 270 到 72 DIP 或反向变化。动画关闭时保持同步直接切换；页面通过 Grid 列变化自动跟随，不使用 Canvas。
- `GameSaveCenterSettings.SidebarCollapsed` 由 `DashboardView` 注入生产壳读写并立即保存，首次加载恢复用户选择。命令、导航、设置入口、主题切换、滚动与虚拟化未改动。
- 本轮验证：Debug 隔离构建 0 warning/0 error；Core 59/59、Worker 199/199、Playnite 295 通过/57 跳过；`validate-source.py`、`check-xaml.ps1`、WPF 静态审查（0 error/18 warning/172 info）、`git diff --check` 和 `.tmp/ui-qa-sidebar-boundary-v1/render-qa-report.txt`（`render-qa OK`）均通过。真实 Playnite 宿主尺寸、动画点击、DPI/焦点仍需重载扩展后复核。

## 2026-08-25 UI-319 Dune 浅色主题 FollowPlaynite 修复

- 用户当前 Playnite Desktop 主题是 Dune。该主题通过 `ThemeDarkStyle=False` 表示浅色，并同时发布 `WindowBackgroundBrush`/`DarkWindowBackgroundBrush`；Follow 不能只依赖默认主题的历史 `WindowBackgourndBrush`。
- `AdaptiveThemePaletteFactory` 现在先读取 `ThemeDarkStyle`，按主题模式选择浅色或深色窗口资源；无该标志的主题才使用背景与 `TextBrush`/`TextBrushDark` 的一致性推断。正确拼写的 `WindowBackgroundBrush` 排在历史拼写之前。
- 本轮没有改变强制浅色/深色、设置绑定、保存按钮、命令、虚拟化或 Playnite 兼容性；新增回归测试锁定 Dune 两套资源同时存在时的 Follow 结果。
- UI-319 验证：定向 2/2，Playnite 290 通过/57 跳过/0 失败，Release 0 警告/0 错误，`validate-source.py` 和 `check-xaml.ps1` 通过。真实 Playnite 重载后的 Follow 浅色设置窗口仍需人工确认，不能把离屏测试写成宿主像素验收。

## 2026-08-25 UI-318 Follow Playnite 主题读取修复

- Playnite Desktop 的实际背景资源键是 `WindowBackgourndBrush`；Follow 解析已加入该键，并兼容其他常见背景键。设置窗口还会显式检查 owner Window 和 `Application.Current` 资源。
- 当主题只提供文本资源时，使用 `TextBrush`/`TextBrushDark` 明暗推断安全背景；强制浅色/深色行为保持不变。
- 设置页主题 ComboBox 会先显式写回 `CurrentSettings.ThemeMode` 再刷新材质，避免从深色切换 Follow 时使用旧枚举值。
- UI-318 已通过定向/全量测试、Release 构建、源码/XAML/WPF 门禁和 `.tmp/ui-qa-settings-follow-v8/render-qa-report.txt`；真实 Playnite 重启后需确认实际浅色宿主窗口。

## 2026-08-25 UI-317 壳体圆角玻璃与 Follow Playnite 主题

- 生产壳导航当前由 `SidebarSurface` 的真实圆角 Border 裁切，材质使用动态 `GscSidebarMaterialBrush`；页脚由 `FooterSurface` 提供四边圆角玻璃面。不要恢复让内部 Grid 直接绘制导航背景的结构。
- 设置页不跟随游戏图片，但使用 `SettingsAmbientLayer` 的环境渐变/BlurEffect 和低 alpha 的外壳、分类栏、卡片、内容材质；BlurEffect 不得提升到文字、表格、列表或滚动区域。
- Follow Playnite 解析优先级已调整为宿主发布的背景资源，再回退视觉树背景，避免插件自身深色回退资源抢先把 Playnite 浅色主题识别成深色。
- RenderHarness 设置页 Light/Dark 审计必须经过 `ApplyThemeForAudit`；最新证据为 `.tmp/ui-qa-settings-glass-v7/render-qa-report.txt`，真实宿主逐像素、DPI、高对比度仍待人工确认。

## 2026-08-25 UI-314 当前交接：游戏背景真实模糊

- 当前游戏背景仍由 Shell 的唯一跨壳 `ImageBrush` 绘制；UI-314 只在背景实际加载时给这个矩形挂 `GscGameBackgroundEffect`，不要把 BlurEffect 加到卡片、文字、页面根、列表或滚动器。
- `GscGameBackgroundEffect` 由 `AdaptiveThemePalette` 按毛玻璃强度直接生成 20–100 DIP 的冻结 `BlurEffect`，默认设置为 78 DIP，`RenderingBias=Performance`。图片仍保持 `UniformToFill` 居中、不平铺和原有透明度/tint。
- 没有背景图、关闭毛玻璃、高对比度时必须返回 null；XAML 的 `HasSelectedGameBackgroundAmbientMaterial` DataTrigger 负责避免无图时保留大面积效果视觉。
- UI-314 已通过源码门禁、WPF 静态审查、Release 全量测试和多主题多尺寸 `render-qa OK`。真实 Playnite 没有可控窗口，安装后需人工确认模糊强度和帧率。

## 2026-08-24 UI-313 当前交接：背景图单层居中与底部接缝

- 截图中的矩形接缝来自重复材质层：生产 `ShellAmbientMaterialLayer` 与各工作区内部的 `AmbientMaterialLayer` 不能同时绘制 `SelectedGameBackgroundAmbientBrush`。共享控件的 `UseSelectedGameBackground` 默认关闭，只有 Shell 实例设置为 `True`。
- Shell 真实背景使用一个跨两行两列的 `ImageBrush`，显式 `Stretch="UniformToFill"`、`AlignmentX/Y="Center"`、`TileMode="None"`；图片和 tint 均覆盖页脚行。不要恢复多个 Image/ImageBrush 或页面局部游戏背景层。
- 页面局部环境层在有游戏背景时保持透明，因此游戏颜色仍能从 Shell 单一环境层透出，同时不再叠加固定绿色宽域渐变。无背景、关闭毛玻璃和高对比度的主题回退保持不变。
- UI-313 已通过源码门禁、WPF 静态审查、Release 全量测试和多主题多尺寸 `render-qa OK`；当前 Playnite 没有可控窗口，真实宿主仍需安装后切换不同宽高比游戏背景人工确认。

## 2026-08-24 UI-312 当前交接：卡片表面与游戏背景自适应

- Today 卡必须直接使用共享 `GscRedesignSectionCard`，不要在卡片内部放整面背景 Border；此前的方形内层来自 WPF 圆角卡片里嵌套第二层材质。
- 背景链路现在由 `PlayniteGameBackgroundProvider.LoadVisualAsync` 同时返回真实图片和图片采样环境色。`AmbientMaterialLayer` 有真实背景时隐藏固定主题宽域洗色，使用采样色；无背景时才使用主题默认宽域材质，因此不能重新把 success 绿色固定叠到所有游戏上。
- 背景图仍位于 Shell 底层，卡片/导航/文字保持功能层级；深色/浅色图片透明度为 0.48/0.40，主题 tint alpha 为 0x52/0x66。远程下载 5 秒、12 MB、后台解码、缓存、取消和 generation 约束必须保留。
- UI-312 已通过源码门禁、WPF 静态审查、Release 构建、背景提供器 5 项定向测试和多主题多尺寸 `render-qa OK`。当前 Playnite 进程没有可控窗口，因此下一次安装后需要人工切换两款有不同背景图的游戏确认真实宿主颜色跟随。

## 当前 UI 重构方向（2026-08-17）

用户已明确要求对页面进行完全大改。现在允许重做页面布局、信息架构、导航容器、Tab/Segmented 结构、控件类型、共享模板、滚动实现和视觉层级。下方 UI-221 及更早条目中的“不要恢复/不要替换/明确不迁移”是上一轮样板迁移的历史交接信息，不再构成页面结构或控件实现禁令。

新实现仍需以真实业务行为为底线：命令、Binding、数据、安全确认、错误/取消、可访问性、可扩展性能和 Playnite 兼容性不能被无意删除。如果新方案有意改变这些能力，应在当前阶段明确说明并配套测试；不得以“旧实现必须保留”为理由阻止整页重构。

## 2026-08-24 UI-311 当前交接：Today 圆角与游戏背景切换

- Today 卡内部宽域材质使用带 12 DIP 圆角的 Border；不要改回整面 Rectangle。外层 `ClipToBounds` 不能模拟 WPF 的圆角裁切。
- 背景切换链路仍是 `GamePickerViewModel.SelectedItem` → `DashboardViewModel.RefreshSelectedGameBackground` → `AcrylicProductionShellView` 的 `SelectedGameBackground` Binding。提供器优先读 Playnite 本地缓存/数据库文件，也支持当前选中游戏的 HTTP/HTTPS 背景直链；远程路径必须保持 5 秒超时、12 MB 上限、后台解码、取消和 generation 防串图。
- 毛玻璃本轮只做小幅增强，不增加 Shell/页面/表格/滚动器的 BlurEffect：背景图透明度和宽域渐变稍微提高，主题 tint 稍微变透明；关闭毛玻璃和高对比度仍必须回退到透明背景图层。
- 验证：`scripts/build.ps1 -Configuration Release` 完整通过（Core 59、Worker 199、Playnite 288 通过，57 跳过），XAML/source/WPF 门禁通过，`.tmp/ui-qa-current/render-qa-report.txt` 为 `render-qa OK`。本轮未在可识别 Playnite 宿主中实际切换游戏截图，不能把真实宿主背景切换宣称为已人工复核。

## 最新视觉优先级：Demo-first（2026-08-20）

后续迁移统一以 `GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design/` 下的 `DesignShellView.xaml`、`Pages/*.xaml`、`DesignTokens.xaml`、`DesignColorsLight.xaml`、`DesignColorsDark.xaml` 和 `DesignControls.xaml` 为主要且唯一视觉基准。Demo 与旧生产页面、UiLab、历史计划或 `wpf-apple-desktop-ui` 的通用 Apple-inspired 设计建议冲突时，以 Demo 的整体页面结构和视觉层级为准。

`wpf-apple-desktop-ui` 仅用于质量检查，不再限制 Demo-first 的页面选择；必须继续保护真实数据、命令、Binding、错误/取消/安全语义、虚拟化、可访问性和 Playnite 兼容性。当前游戏选框与现有滚动条系统按总目标保留，Demo 的 Mock 数据和演示行为不迁移。

## 2026-08-24 UI-310 当前游戏背景图环境材质

- 生产 Shell 通过 `DashboardViewModel.SelectedGameBackground` 使用 Playnite `Game.BackgroundImage` 的本地缓存/数据库文件；UI-311 已补齐当前选中游戏的受限 HTTP/HTTPS 异步下载，其他无法解析的引用仍回退主题默认背景。
- 背景图片低透明度绘制在 Shell 底层，之上仍是主题 tint、宽域多色材质、导航和页面卡片；它不是对整页加 BlurEffect。无背景图、关闭毛玻璃或高对比度时，`GscGameBackgroundOpacity` 必须为 0。
- 默认背景确实跟随当前主题：`AdaptiveThemePalette` 根据 Playnite 主题/用户的浅深色模式生成 `GscBackdropBrush`、背景 tint 和宽域材质。不要让某个游戏背景图替换主题文字/控件对比度。
- `PlayniteGameBackgroundProvider` 使用后台解码、1920 宽度上限、6 张缓存和取消/generation 保护；保持这些性能边界。
- UI-310 已完成构建、全量测试、WPF 静态检查和多主题多尺寸 render QA；本轮未强制关闭正在运行的 Playnite，因此真实宿主背景图显示需更新安装后复核。

## 2026-08-24 UI-309 全局宽域多色玻璃材质

- 主页 Today 卡片、Settings 环境层、兼容 Dashboard 和共享 `AmbientMaterialLayer` 统一使用 `GscAmbientWideWashBrush` 的整面矩形；不再恢复装饰性径向渐变、椭圆光斑或 `GscAmbientBlurEffect`。
- 宽域材质由动态 accent/info/teal/success/中性表面组成六段线性渐变，颜色在大范围内缓慢过渡；透明度受 `GlassStrength` 限制，关闭毛玻璃/高对比度必须透明。状态小圆点和图标语义色仍保留。
- 不要给 Shell、导航栏、页面根、表格、列表或滚动器挂大面积 `BlurEffect`；Playnite 嵌入视图没有安全的宿主桌面像素 backdrop blur，这里采用低成本整面渐变模拟。
- 本轮不改变真实命令、Binding、数据契约、虚拟化、滚动条或 Playnite 兼容性；源码/XAML/WPF 门禁、Release 构建、全量测试和多主题多尺寸 `render-qa` 均已通过，当前用户 Playnite 扩展已核对为 `0.6.70.0`。由于 Computer Use 当前只返回 `EmptyWindowAutomationPeer`，不要把本轮描述为真实宿主页面点击/截图已复核。

## 2026-08-24 UI-308 导航栏与宽域材质交接

- `AcrylicProductionShellView.xaml` 的 `SidebarLayout` 统一使用 `GscSidebarMaterialBrush`；标题/导航两个子 Border 必须保持透明，不能恢复成各自绘制材质，否则会造成渐变断层。
- 导航栏材质使用全区域对角线性渐变，不做透明边缘渐隐，不新增右侧硬边或光柱。它是 Playnite 嵌入视图下的低成本玻璃模拟，不是对宿主桌面像素的真正 backdrop blur。
- `AmbientMaterialLayer.xaml` 第一层为 `GscAmbientWideWashBrush` 的 `Rectangle`，用于大范围非圆形环境洗色；旧的固定椭圆实现已由 UI-309 移除。不要给 `SidebarLayout`、页面根、表格、列表或滚动器增加 BlurEffect。
- `AdaptiveThemePaletteFactory.ApplyMaterialResources` 必须在关闭毛玻璃/高对比度时提供透明宽域渐变；资源测试已锁定这一降级契约。
- UI-308 已完成 Release 构建、全量测试、多主题/多尺寸 render QA，并在真实 Playnite `0.6.70.0` 中查看了导航栏和首页；详情见 `docs/ai/WORKLOG.md`。

## 2026-08-24 UI-307 首页圆角与导航材质交接

- Today 卡片的装饰椭圆已全部移入卡片内部。不要依赖 `ClipToBounds` 裁切圆角，也不要恢复负边距光源；WPF 这里按矩形裁切，负边距会在圆角外产生直角色块。
- `AmbientMaterialLayer` 的 `ShowLeftGlow` 默认开启，生产 Shell 必须关闭它；页面局部环境光仍可保留。这样主内容列起点不会出现竖向光柱。
- 导航栏必须使用完整宽度的中性 `GscSidebarMaterialBrush`，当前不使用 `SidebarSeamMaterial`、`GscSidebarSeamBrush`、透明渐隐边缘或右侧 1 DIP 硬边。导航玻璃感由整栏低对比度材质提供，不能通过边缘高亮增强。
- 本轮没有改变命令、Binding、页面结构、DataGrid/ListBox 虚拟化、滚动条或安全语义。真实 Playnite `0.6.70.0` 已安装并截图复核：Today 左上角圆角正常、导航分界无光柱。
- 证据：Release 构建 0 warning/0 error；Core 59/59、Worker 199/199、Playnite 283/283（57 跳过）；多主题、多尺寸、滚动及 resize transition `render-qa OK`。

## 2026-08-24 UI-306 历史记录（已由 UI-307 覆盖）：Shell 毛玻璃与导航栏硬边

- `AcrylicProductionShellView.xaml` 的 `ShellAmbientMaterialLayer` 使用正向 Z 层级覆盖整个 Shell 两列，位于导航/页面表面下方；不要放回负 Z 层，也不要把 Blur 挂到根 Shell 或页面内容。
- 导航栏两个表面不再绘制右侧 1 DIP 边框；`SidebarSeamMaterial` 是 42 DIP、不可交互的渐隐过渡，`GscSidebarSeamBrush` 和 `GscShellAmbientOpacity` 由运行时主题/毛玻璃强度提供。不要恢复硬直线或新建布局列。
- 本轮未改动真实导航命令、页面 Binding、DataGrid/ListBox 虚拟化、滚动条或页面布局契约。共享固定装饰 Blur 仍为性能模式，关闭毛玻璃/高对比度必须保持真实 null/0 降级。
- 证据：`artifacts/ui-qa/shell-glass-final2/render-qa-report.txt` 为 `render-qa OK`；Release 构建 0 warning/0 error，Core 59、Worker 199、Playnite 283 通过，57 跳过；生产扩展 `0.6.70.0` 已安装并在真实 Playnite 查看首页/存档/修改器页面。若沙箱中重现安装 Access denied，应使用与 Playnite 相同用户权限的安装流程，不要据此判断旧 DLL 的视觉结果。

## 2026-08-24 UI-305 任务表格底部滚动交接

- 用户截图中的任务表最后一行被水平滚动条覆盖。生产 `TaskDataGrid` 当前仅增加 `Padding="0,0,0,12"` 底部安全区，不能关闭横向滚动、不能关闭共享 `DataGridStarFill`，也不要改动真实列宽、Binding、命令、Item scrolling 或 Recycling。
- RenderHarness 使用 500 条任务数据执行到底→回顶→再到底→中段→回顶；底部检查比较最后一行底部和水平滚动条顶部，同时检查回拉后无空白/无效行。修改滚动链路时必须保留该方向性探针。
- 当前离屏证据在 `artifacts/ui-qa/task-bottom-final/render-qa-report.txt`，结果为 `render-qa OK`；这不等同真实 Playnite 宿主逐像素验收，用户仍需在本机滚到任务列表底部确认最后一行完整。

## 2026-08-24 UI-303 任务中心输入文字垂直裁切修复交接

- 根因已确认：TextBox 模板把 `TextBox.Padding` 同时用于 `PART_ContentHost.Margin`，WPF TextBox/Playnite 宿主又对内容宿主应用 Padding，任务搜索框上下 `7 DIP` 重复扣除，`PART_ContentHost.ViewportHeight` 仅约 `5 DIP`。不要恢复 `Margin="{TemplateBinding Padding}"`，也不要通过增加 TextBox 高度修复。
- `WpfUiProduction.xaml`、`DesignTokens.xaml` 的 `PART_ContentHost` 当前使用 `Margin="0"`、`Padding="0"`、`BorderThickness="0"`；TextBox 的 Padding 由原生内容视口消费，负责上下可视空间和左右输入边距。内容宿主显式继承 TextBox 的 Foreground、FontFamily、FontSize、FontWeight，避免宿主主题重新改变文字度量或颜色。
- 当前验证：源码门禁、XAML 19/19、WPF 静态审查 0 error/20 warnings/165 info、Release 0 warning/0 error、Core 59/59、Worker 199/199、Playnite 282/282（57 跳过）通过；输入态离屏 viewport `5→19 DIP` 且文字完整显示，render QA 通过；生产安装已核验清单 `0.6.70`、DLL `0.6.70.0`。真实 Playnite 逐像素截图仍需用户复核。

## 2026-08-24 UI-302 任务中心搜索框可见性与图标间距修复交接

- TaskCenter 的“搜索任务…”和 Dashboard 游戏库搜索框属于带图标输入，当前输入 Padding 为 `30,7,38,7`，对应占位提示从 `30` DIP 起始；右侧 `38` DIP 继续为清除按钮保留空间。不要把这两处重新收紧到 `20`，否则放大镜会贴住提示文字。
- `WpfUiProduction.xaml` 和 `DesignTokens.xaml` 的 TextBox 模板必须把 `TextElement.Foreground` 绑定到 `Foreground`，并保留 `PART_ContentHost` 的现有对齐绑定；这是 Playnite 宿主下真实输入文字可见性的关键。数值输入的专用对齐、搜索清除按钮、Binding、命令、焦点和无障碍语义保持。
- 当前验证：`validate-source.py`、XAML 19/19、WPF 静态审查 0 error/20 warnings/165 info、Release 0 warning/0 error、Core 59/59、Worker 199/199、Playnite 282/282（57 跳过），`artifacts/ui-qa/ui302-task-search-v1/render-qa-report.txt` 为 `render-qa OK`；受控生产安装已核验扩展清单 `0.6.70`、DLL `0.6.70.0`，用户宿主逐像素截图仍需用户复核。

## 2026-08-24 UI-301 表格右侧安全边距与搜索输入起点微调交接

- 共享及 Dashboard 本地选中行模板的 `RowChrome` Margin 已统一为 `4,2,12,2`；这只是为了避开 Playnite 宿主垂直滚动轨道，不要改动 `SelectiveScrollingGrid`、滚动方式、排序或 Recycling。
- UI-301 曾将 TaskCenter 和 Dashboard 游戏库的带搜索图标输入框左侧 Padding/提示 Margin 收紧为 `20`；UI-302 已修正为 `30`，右侧清除按钮预留 `38` 保持；普通 TextBox、数值输入、清除按钮、Binding 和键盘焦点语义不变。
- 当前验证：`validate-source.py`、XAML 19/19、WPF 静态审查 0 error/20 warnings/165 info、Release 0 warning/0 error、Core 59/59、Worker 199/199、Playnite 282/282（57 跳过），`artifacts/ui-qa/ui301-spacing-v1/render-qa-report.txt` 为 `render-qa OK`；受控 `scripts/dev-install-run.ps1 -Configuration Release -NoStart` 已安装生产扩展 `0.6.70.0`。真实 Playnite 宿主逐像素边距仍由用户复核。

## 2026-08-24 UI-300 / FUNC-004 表格、输入框与 FLiNG 归档修复交接

- 共享 `WpfUiProduction.xaml`、`DesignTokens.xaml` 的普通 TextBox 现在显式使用左对齐内容和文本；模板把 `HorizontalContentAlignment` 传给 `PART_ContentHost`，因此插入光标从输入框左侧内边距开始。数值输入仍由专用样式覆盖为右对齐或居中，不改变原有编辑语义。
- 游戏、媒体、任务和 FLiNG 搜索框均增加共享清除按钮：有内容时显示 `×`，清空后保留键盘焦点；Dashboard 原有游戏搜索行为保持不变。按钮使用 AutomationProperties 名称，右侧输入内边距为清除按钮预留命中区。
- 共享 `GscRoundedDataGridRowTemplate` 与 Dashboard 本地兼容行模板把选中描边收进右侧安全区，避免 Playnite 宿主垂直滚动轨道覆盖右边圆角；Save/Task/Media/Maintenance 仍复用同一选中态、滚动、排序和 Recycling 契约。
- Overview 风险区两个真实命令按钮统一使用工具栏按钮模板、固定共享高度和垂直居中，命令、Automation 名称和安全行为不变。
- FLiNG 归档按实际站点链路处理：归档站点的字母/子目录列表中的 `.zip`、`.rar`、`.7z` 直链进入现有目录搜索；下载后识别 RAR/7z 签名，由 SharpCompress 流式解包，并复用越界路径、条目数、单文件和总展开体积限制；不执行压缩包中的 EXE。官方归档临时不可用时仍不阻塞在线目录刷新。
- 当前阶段验证：`validate-source.py`、XAML 19/19、WPF 静态审查 0 error/20 warnings/165 info、Release 0 warning/0 error、Core 59/59、Worker 199/199、Playnite 282/282（57 项按既有环境规则跳过），`artifacts/ui-qa/ui300-input-fling-v1/render-qa-report.txt` 为 `render-qa OK`。`scripts/dev-install-run.ps1 -Configuration Release -NoStart` 已在受控 Windows 权限下完成生产扩展安装，清单 `0.6.70`、DLL `0.6.70.0`；尚未从官方归档实际下载并运行修改器，安全软件拦截仍需用户环境复核。

## 2026-08-23 FUNC-002 媒体收件箱忽略恢复交接

- 本轮继续按用户要求只补功能块，不重排媒体页面。`MediaInboxBatchActionRow` 内增加“待归类/已忽略”视图切换；已忽略列表按需加载，默认待归类流程仍只请求原有消息。Inspector 在已忽略模式只显示恢复动作，原有归类、忽略、批量选择和 DataGrid 滚动/虚拟化保持不变。
- Worker/SQLite 新增 `ListIgnoredMedia`、`RestoreIgnoredMediaBatch`、`GetIgnoredMediaAsync` 和 `RestoreMediaToInboxAsync`。恢复最多 500 个去重 ID，文件先做现有归档/原始来源选择和目标冲突哈希校验，再移动回 `_Inbox\\Pending`；原始文件不删除，数据库回写 `Inbox`/`NotApplicable`/“用户撤销忽略，待重新归类”，每项追加审计，部分失败返回明细。
- 恢复完成后 Playnite 刷新两个缓存，防止切回待归类时显示旧数据。当前模式不持久化，重新打开页面默认待归类；不要在旧 Worker 兼容路径中预加载已忽略查询。
- 当前证据：`validate-source.py`、XAML 19/19、WPF 静态审查 0 error；Release 0 warning/0 error；Core 59/59、Worker 196/196、Playnite 280 通过/57 跳过；`artifacts/ui-qa/media-inbox-restore-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖双主题、多尺寸、滚动和 resize transition。真实 Playnite 未执行会改变用户媒体数据的恢复操作；宿主 DPI、高对比度、键盘焦点和真实点击命中仍需人工验收。

## 2026-08-23 FUNC-001 媒体收件箱批量操作交接

- 本轮按用户要求只做功能块，不重排页面。`MediaCenterView.xaml` 在待归类表格上方增加轻量 `MediaInboxBatchActionRow`：多选收件箱行、选择目标游戏、批量归类或忽略；原 Inspector、单条操作和表格滚动/虚拟化不变。
- Worker 新增 `media.reassign.batch` / `media.inbox.ignore.batch`，每个请求最多 500 个去重 ID；Playnite 更大选择自动分批。服务端复用现有文件移动、归档副本、SQLite 索引和审计逻辑，返回成功项与失败明细；忽略仍保留归档副本，取消继续传播。
- `AcrylicProductionShellView.xaml` 的 Worker/Ludusavi 状态改为真实 DataTrigger 文案和颜色，禁止恢复原始布尔文本。
- 当前证据：源码校验、XAML 19/19、Release 构建 0 warning/0 error、Core 59/59、Worker 194/194、Playnite 280 通过/57 跳过，`artifacts/ui-qa/media-inbox-batch-v1/render-qa-report.txt` 为 `render-qa OK`，WPF 审查 0 error。没有在真实 Playnite 执行会改动用户数据的批量操作；宿主 DPI、高对比度和真实点击命中仍需人工验收。

## 2026-08-22 UI-296 任务/媒体摘要条实际宿主交接

- 用户提供的 Playnite 截图证明实际宿主仍在显示旧的“四列全等宽 + Rectangle 占用第 2/3/4 个统计列”布局：第一块后缺线、最后一块后多线。此前源码的七列修复没有同步到 00:07 安装的 DLL，不能只看离屏 RenderHarness 就认为宿主已经更新。
- `TaskCenterView.xaml` 与 `MediaCenterView.xaml` 现在统一使用 `* / Auto / * / Auto / * / Auto / *`；四个统计块为 `0/2/4/6`，三条竖线为 `1/3/5`，没有尾线。测试锁定精确列序列、3 条分隔线及仅允许奇数列放置 Rectangle。
- UI-296 已通过 `validate-source.py`、WPF 静态审查、Release 构建/测试和 `artifacts/ui-qa/summary-divider-layout-fix/render-qa-report.txt` 双主题多尺寸回归；构建为 0 warning/0 error，Core 59、Worker 194、Playnite 277/57/0。
- 已运行 `scripts/dev-install-run.ps1 -Configuration Release -NoStart`，标准 Playnite 扩展目录验证为 `0.6.70.0`。本轮没有启动 Playnite 也没有声称真实嵌入像素已通过；用户启动后应重新查看 Task/Media 两页确认宿主截图。

## 2026-08-22 UI-297 页面激活运行中游戏同步交接

- 旧自动定位依赖首次 Worker Dashboard 快照中的 `IsRunning` 和页面存活期间的 `PlayniteGameStarted`；Worker 在游戏已运行后才启动时，进程检测首轮基线不会创建会话，因而选框可能保持上次游戏。
- `GameSaveCenterPlugin.TryGetCurrentlyRunningPlayniteGameIds()` 只读 Playnite SDK `Game.IsRunning`。`DashboardView` 在 `Loaded`、重新可见和快照应用后调用 `DashboardViewModel.SelectCurrentlyRunningGameOnViewActivation()`；它只覆盖 UI DTO 状态并按现有 resolver 规则切换，不发送 Worker IPC、不创建会话、不触发备份，也不改变停止后保留选择的语义。
- `GamePickerItem` 的 `INotifyPropertyChanged` 仅用于不替换缓存对象时刷新运行状态；大库筛选/排序/虚拟化和真实命令/Binding 不变。
- 验证：`validate-source.py` 通过；隔离 Release XAML 19/19、0 warning/0 error；专项测试 1/1 通过；WPF 审查 0 error、21 warnings、164 info。完整 Playnite 测试当前分支 240 通过/62 跳过/19 既有 Demo/布局断言失败；真实 Playnite 反复打开页面、多游戏同时运行、主题/DPI 尚未验证。

## 2026-08-22 UI-295 媒体摘要分隔线交接（已由 UI-296 统一为 Auto 分隔槽）

- `MediaCenterView.xaml` 的 `MediaSummaryPanel` 使用 `* / Auto / * / Auto / * / Auto / *` 七列；四个统计块位于 `0/2/4/6`，三条分隔线位于 `1/3/5`，最后一块右侧不能再增加 Rectangle。
- 本轮只修正摘要条几何，真实 MediaSummary/Snapshot OneWay Binding、媒体 Tab、DataGrid/ListBox 虚拟化、Inspector、命令和滚动模型均未改变。
- UI-295 已通过源码校验、WPF 静态审查、Release 构建/测试和 `artifacts/ui-qa/media-summary-divider-fix/render-qa-report.txt` 双主题多尺寸回归；真实 Playnite 宿主 DPI/连续缩放仍需人工验收。

## 2026-08-21 BUILD-002 跨电脑构建与测试交接

- `9b19dbd` 的 Release 编译本身通过；此前另一台机器的 5 个失败均是视觉对照测试读取 `D:\workplace\Github\GameSaveCenter.AcrylicFork` 导致的 `DirectoryNotFoundException`。
- 已删除 `AcrylicForkDesignSource.cs`、`AcrylicForkDesignFactAttribute.cs` 及五个测试中的外部 Demo 读取；这些测试现在用普通 `[Fact]` 验证仓库内生产资源契约。不要重新引入 Demo 目录解析、绝对路径或“缺失时跳过”的默认逻辑。
- 普通跨电脑构建不需要 `GSC_ACRYLICFORK_ROOT`、`GSC_REQUIRE_ACRYLICFORK_BASELINE` 或 `GameSaveCenter.AcrylicFork` 兄弟目录。Demo 是视觉设计来源，不是生产编译/测试输入。
- Worker 默认 Soak 规模已设为慢盘可接受的边界值；完整压力测试使用 `GSC_SOAK_DATA_SCALE=1`，稳定性周期可用 `GSC_SOAK_ITERATIONS` 覆盖。验证脚本继续使用 `-m:1 -nodeReuse:false`，避免 SDK 多节点恢复阶段长时间无输出。
- 本阶段只修复测试环境可移植性，未回退或重写此前 Demo-first UI、功能、绑定、命令和业务行为。

## 2026-08-21 UI-287 表格表头与列宽交互交接

- 共享 `WpfUiProduction.xaml` 的 DataGrid 表头现在必须包含 `PART_LeftHeaderGripper` 和 `PART_RightHeaderGripper`；不要为了改箭头而删掉这两个 WPF 模板部件。第一列左 Thumb 由 WPF 自动折叠属于正常行为，其他可调整边界必须有有效命中区。
- 共享 DataGrid 设置 `CanUserResizeColumns=True`、`MinColumnWidth=64`、`CanUserSortColumns=True`；排序箭头使用独立 22 DIP 列，避免窄列裁剪。`DashboardView.xaml` 的本地兼容表头也保持同样的 22 DIP 箭头列和两个 resize 部件。
- 不要为单个页面复制另一套表头模板；Save、Media、Task、Maintenance 使用共享表格契约，真实绑定、排序、选中态、虚拟化和现有滚动条系统不变。
- `RenderHarness` 的 `VerifyDataGridHeaderInteractionContract` 会检查真实模板部件和排序态箭头布局；运行 `scripts/render-qa.ps1` 时若表格回退到没有 Thumb 或箭头宽度为 0 的模板，必须视为失败。
- 当前证据：`artifacts/gsc-b/ui287-table-header-v2` 构建/测试通过；`artifacts/ui-qa/ui287-table-header-v2/render-qa-report.txt` 为 `render-qa OK`。真实 Playnite 宿主仍需在可见窗口中实际拖动至少一张 Save/Media/Task 表格列边界确认命中体验，不能把离屏证据写成宿主已验证。

## 2026-08-21 UI-286 修改器导入与历史归档交接

- `GameToolService` 已修复文件名误判：含版本文字 `Update` 的修改器 EXE 不再被过滤；只有明确的 `unins*`、`uninstall`、`update`、`updater`、`setup` 辅助入口在目录/ZIP候选列表中排除。显式单文件导入必须保留其入口。
- 拖放导入的候选集合、选中项、清理和导入后工具选择均通过 `DashboardViewModel.ApplyOnUi`，防止 Worker/IPC continuation 直接修改 WPF `CollectionView`。不要将 `Replace(ImportEntryCandidates,...)` 或 `ImportEntryCandidates.Clear()` 移回后台 continuation。
- FLiNG 目录刷新现在可选读取 `https://archive.flingtrainer.com/` 的静态目录；历史 ZIP/RAR/7z/EXE 作为“FLiNG 归档”搜索结果，读取版本后走现有下载/安全解压/入口选择流程。归档失败是可降级的，不得让当前在线目录刷新失败；RAR/7z 由 SharpCompress 解包并继续执行同一安全限制。
- 当前证据：`artifacts/gsc-b/ui286-trainer-import-v2` 为 XAML 18/18、Release 0 warning/0 error、Core 59/59、Worker 194/194、Playnite 273 通过/60 跳过/0 失败；`artifacts/ui-qa/ui286-trainer-import-v1/render-qa-report.txt` 为 `render-qa OK`；source 0 error，WPF UI 0 error、20 warnings、164 info。真实 Playnite 宿主和用户提供的 EXE 均未执行/未验证，不能把本阶段写成真实宿主验收或总迁移完成。

## 2026-08-21 UI-285 媒体待归类反向滚动空白交接

- `MediaInboxGrid` 已局部关闭 `infra:DataGridStarFill.Enabled`。它仍保留有限 Grid 中的原生星号列、Standard 行虚拟化、Item 滚动和列虚拟化关闭；不要恢复共享 star-fill，也不要关闭全部虚拟化。
- 根因是有限视口中的 star-fill 像素重分配会在大数据反向滚动时触发行/列呈现器重测量，出现滚动条有效但行容器不再显示。真实 `UnassignedMedia`、选中媒体、预览 Inspector、归类/忽略入口和安全语义均未改动。
- RenderHarness 已加入多次 `0→100→0→100→50→0→100→0` 的 4468 行收件箱回退探针，并把 star-fill 开关纳入门禁；最新 `render-qa-report.txt` 为 `render-qa OK`，构建/测试和源码校验通过。
- 这是代码级和离屏验证结果；真实 Playnite 宿主仍需用户在可见窗口拖动到底再回翻确认，不能把本阶段写成 Playnite 生产渲染已验证。

## 2026-08-21 UI-284 主题前景色与页面控件对齐交接

- 共享 `GscWpfUiButtonTextTemplate` 已把 Button 的动态前景色和字体属性传给内部文字；这是修复深色主题 Primary 按钮黑字的根因，后续页面不要复制 Button 模板。
- 首页风险操作区固定两个按钮的高度并水平对齐；需关注事项继续使用真实 `AttentionFindings` 的 Demo 分隔列表，`SuggestedAction` 仍是右侧文字，不得换回 checkbox、逐项按钮或空的自定义卡片；底部维护入口使用带背景/描边的 Secondary 样式。
- 设置分类栏已从 `LabSegmented` 切换到 `GscSettingsSectionTabs`，背景、选中态、文字和 Hover 均来自当前主题动态资源，浅色/深色主题都不能写死灰色填充。
- 比较页质量气泡使用固定高度与内边距，并保持标题行垂直居中；同时保留比较/保留页横向画布、真实差异/保留绑定和安全说明。
- 当前证据：`artifacts/ui-qa/ui284-theme-v1/render-qa-report.txt` 为 `render-qa OK`，构建和测试通过，WPF 静态校验 0 error；本阶段未运行真实 Playnite 宿主，后续仍需在可见宿主补验主题、DPI 和交互命中。

## 2026-08-21 UI-283 媒体待归类大数据滚动修复交接

- 用户反馈待归类收件箱在约 4468 条数据向下滚动时出现表头下空白表格。当前 `MediaDataGrid` 必须保留 `Item` 滚动和行虚拟化，但局部覆盖为 `VirtualizingPanel.VirtualizationMode=Standard`、`EnableColumnVirtualization=False`；这是针对星号文本列与大数据量的 WPF 呈现稳定性例外。
- 不要把该例外恢复为共享 `Recycling`/列虚拟化，也不要通过关闭整个 DataGrid 虚拟化、复制滚动条或删除 Inspector 来规避。`UnassignedMedia`、`SelectedInboxMedia`、预览、目标游戏选择、归类/忽略命令及安全语义均已保留；Inspector 继续位于表格滚动面之外。
- `RenderHarness` 媒体探针固定 4468 条数据，并显式验证媒体为 Standard/关闭列虚拟化，其余工作区仍验证共享 Recycling/列虚拟化。最新报告：`artifacts/ui-qa/ui283-media-inbox-v5/render-qa-report.txt` 为 `render-qa OK`；构建/测试：`artifacts/gsc-b/ui283-media-inbox-v1`。
- 本阶段完成了代码级回归修复和离屏验证，但没有新的可识别 Playnite 真实 Dashboard 逐页截图；后续仍需用户在可见宿主中确认实际拖动滚动条、预览/归类按钮和主题/DPI 行为，不能宣布总 Demo-first 迁移完成。

## 2026-08-21 UI-279 Trainer 导入工具栏窄宽交接

- `TrainerCenterView` 标题区新增 `TrainerToolsToolbar` 与 `TrainerToolsDropHint` 的独立布局行；`ApplyResponsiveLayout` 在 `<980 DIP` 时将四个真实导入命令置于标题下方，拖放提示再下一行，解决 1040×700 / 744 DIP 工作区最后一个按钮被边界裁切的问题。
- 继续保留项目 `TrainerTabControl` / `TrainerTabItem`，不得用隐藏按钮、横向溢出或 Demo 外层 segmented 解决；工具列表、导入确认、工具编辑 Inspector、真实绑定/命令、ScrollViewer 和回收虚拟化不变。
- 当前证据：`artifacts/ui-qa/ui279-trainer-toolbar-v1/render-qa-report.txt` 为 `render-qa OK`；Light/Dark Trainer 1040×700 已抽查，四个导入按钮均在可视区域；`artifacts/ui-audit-ui279-trainer-toolbar-v1/AUDIT_SUMMARY.md` 无 HIGH、无 Fidelity、无失败路由。静态审计的 MEDIUM `TOOLBAR_VERTICAL_EXPANSION` 只来自 Inspector 内必要的五项设置换行。

## 2026-08-21 UI-278 RenderHarness 主题背景交接

- RenderHarness 的页面宿主背景不能写死为深色；`CreateHarnessBackground(view)` 必须优先读取页面当前主题的 `GscBackdropBrush`，否则强制浅色截图会把正确的深色文字错误压到深色画布上，产生假低对比。
- 本轮覆盖主题 QA、Tab 页面、单页页面、滚动/布局探针和 resize 审计宿主；未主题化的旧探针保留原深色 fallback。生产 View、真实命令/绑定、滚动、虚拟化和项目 Tab chrome 均未改动。
- 当前证据：`artifacts/ui-qa/ui278-themed-host-v1/render-qa-report.txt` 为 `render-qa OK`，浅色 Trainer 1040×700、浅色 Save 1366×768、深色 Trainer 1040×700 已抽查。真实 Playnite Dashboard 主题与操作证据仍需用户在可见宿主中补验。

## 2026-08-21 UI-277 折叠栏表面交接

- 共享 `GscDisclosureCardExpander` 已为 Header 提供 `GscControlFillBrush` 表面，并把 Expander 的背景/边框绑定到 Header ToggleButton 与 `HeaderChrome`；这修复了 Task 1040×700 窄宽下“更多筛选”只剩箭头、标题无法识别的问题。
- 后续页面继续使用共享 `GscDisclosureCard`，不要通过局部颜色或隐藏 Header 解决对比度；`LabDisclosure` 对应的整行点击、Hover/Expanded tint、Chevron 动效、真实筛选命令和键盘焦点都必须保留。
- 当前证据：`artifacts/gsc-b/ui-277-disclosure-surface-v1` Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 266 通过/62 跳过/0 失败；`artifacts/ui-qa/ui277-disclosure-surface-v1/render-qa-report.txt` 为 `render-qa OK`，已抽查 Task 浅/深主题 1040×700。真实 Playnite Dashboard 的折叠栏命中与键盘行为仍需用户在可见宿主中补验。

## 2026-08-21 UI-276 媒体当前页操作区交接

- `MediaCenterView` 当前游戏媒体页的批量操作与紧凑 Inspector 入口已拆成 `MediaCurrentActionRow`：宽屏同一行，窄屏提示独占首行、批量操作与“查看媒体详情”分列第二行，避免 Playnite 工作区约 744 DIP 时按钮互相覆盖。
- 真实媒体选择、收藏/取消收藏/应用备注命令、异步预览、Recycling ListBox、Inspector 滚动和紧凑详情抽屉均未迁移；后续不要通过隐藏批量操作或删除详情入口来修复窄宽布局。
- 当前证据：`artifacts/gsc-b/ui-276-media-actions-v1` Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 266 通过/62 跳过/0 失败；`artifacts/ui-qa/ui276-media-actions-v1/render-qa-report.txt` 为 `render-qa OK`，已抽查媒体浅/深主题 1040×700 与浅色 1366×768。真实 Playnite Dashboard 的媒体命中、键盘焦点和 DPI 仍需用户在可见宿主中补验。

## 2026-08-21 UI-275 滑杆几何交接

- 共享 `GscSlider` 已对齐 Demo `LabSlider` 的 22 DIP 高度、4 DIP 轨道和 18 DIP 滑块；唯一 Settings 使用点、真实值绑定、`ValueChanged` 事件、键盘焦点和生产主题阴影继续保留。
- 不要在设置页局部复制滑杆模板，也不要用删除滚动内容来解决低高度布局；RenderHarness 已覆盖 Settings 双主题、多尺寸、滚动和 resize，但真实 Playnite 中的拖动/键盘调节仍需验收。
- 当前证据：`artifacts/gsc-b/ui-275-slider-v1` Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 265 通过/62 跳过/0 失败；`artifacts/ui-qa/ui275-slider-v1/render-qa-report.txt` 为 `render-qa OK`。

## 2026-08-21 UI-274 输入框与下拉状态交接

- 共享 `GscWpfUiTextBoxTemplate` 已对齐 Demo 聚焦填充与 Accent 边框，`GscWpfUiComboBox` 的隐式选项已对齐 Demo 字体、悬停/选中 tint、Medium 字重和 Hand 光标；验证错误、Popup、键盘导航和真实选择绑定均保留。
- `GscControlFocusFillBrush` 有默认令牌、普通主题 Demo 核心覆盖和高对比度 WPF 适配路径。后续页面不要局部复制 TextBox/ComboBox 模板，也不要用隐藏下拉项或改变滚动模型来解决布局问题。
- 当前证据：`artifacts/gsc-b/ui-274-input-combo-v1` Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 264 通过/62 跳过/0 失败；`artifacts/ui-qa/ui274-input-combo-v1/render-qa-report.txt` 为 `render-qa OK`。真实 Playnite Dashboard 仍需用户在可见宿主中打开 GameSaveCenter 后补证。

## 2026-08-21 UI-273 按钮与开关共享状态交接

- `Themes/WpfUiProduction.xaml` 的 `GscWpfUiButton` 已加入 Demo `LabBtn` 的悬停/按下覆盖层和过渡；`GscWpfUiToggleSwitch` 已对齐 Demo `LabToggle` 的 40×23 DIP 几何与 140ms 滑块位移动效。覆盖层不可命中，所有真实内容、命令、绑定和焦点语义保持。
- 不要为了局部页面效果复制按钮/开关模板，也不要把本阶段误扩展为项目工作区 Tab、当前游戏选框或生产滚动条迁移；Trainer/Save/Media/Maintenance 外层仍使用项目 Tab chrome。
- 当前证据：`artifacts/gsc-b/ui-273-shared-button-toggle-v1` Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 263 通过/62 跳过/0 失败；`artifacts/ui-qa/ui273-shared-button-toggle-v1/render-qa-report.txt` 为 `render-qa OK`。真实 Playnite Dashboard 仍需用户在可见宿主中打开 GameSaveCenter 后补证。

## 2026-08-21 UI-272 修改器中心 Tab chrome 回滚交接

- 用户明确要求项目 Tab 栏优先于 Demo Tab UI；已回滚 `TrainerCenterView` 在 `a03accf` 引入的 `TrainerSegmentTabs` + `LabSegmented`，恢复 `TrainerTabControl` / `TrainerTabItem` 项目样式和四个真实 `TabItem`。
- 这只回滚外层导航容器，不回滚页面内容或业务：工具列表、导入确认、FLiNG 目录、发行版本、Inspector、拖拽导入、命令/绑定、回收虚拟化、ScrollViewer 和响应式布局均保留。Settings 左侧五项 `LabSegmented` 分类栏继续保留，这是 Demo 目标明确要求的 Settings 信息架构，不属于项目工作区 Tab chrome。
- 继续禁止把修改器中心外层导航恢复成 Demo `LabSegmented`；Save、Media、Maintenance 及 Dashboard 的项目 Tab chrome 也不得因 Demo-first 页面迁移被替换。
- 最新验证：`artifacts/gsc-b/ui-272-trainer-tab-rollback-v2` 为 XAML 18/18、Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 262 通过/62 跳过/0 失败；source/WPF/diff 门禁通过；`artifacts/ui-qa/ui272-trainer-tab-rollback-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖七页双主题、多尺寸、各 Tab、滚动和 resize。离屏截图已抽查 Trainer 浅/深主题；真实宿主证据仍未补齐。

## 2026-08-21 UI-271 真实 Playnite 宿主审计边界交接

- `scripts/real-host-audit.ps1 -Configuration Release -Output artifacts/ui-host-audit-ui271` 已完成 Release 构建、安装和 Playnite 启动；`summary.json` 为 `EmbeddedSettingsCaptured=true`、`ControlledDashboardCaptured=true`、`EmbeddedDashboardCaptured=false`、`ProductionVisualSourceOfTruthAvailable=false`。
- Settings 的 `settings/embedded-current/viewport/settings.png`、视觉树和资源快照确实来自 `EmbeddedPlaynite`，可用于 Settings 的宿主复核。Dashboard 自动 UI Automation 没有找到左侧 GameSaveCenter 入口，最终 90 秒超时并保留 `gates/REAL_EMBEDDED_DASHBOARD_NOT_CAPTURED.json`；Controlled Dashboard 只能作为受控布局证据。
- Computer Use 观察到 Playnite 主窗口返回 `EmptyWindowAutomationPeer`，没有把 Codex/其他窗口画面当作 Playnite Dashboard，也没有停止不属于本轮的旧 Worker。下次优先让用户在可见 Playnite 左侧手动打开 GameSaveCenter，再重跑审计。
- 本轮基线：XAML 18/18；Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 262 通过、62 跳过、0 失败。真实 Dashboard 七页的像素、DPI、键盘焦点、命中区域、主题切换和真实操作仍未收口，总 Demo-first 目标不能宣布完成。

## 2026-08-21 UI-271 共享表格字阶交接

- 生产 `DesignTokens.xaml` 的 `GscBodyFontSize=13.5`、`GscCaptionFontSize=12` 对齐 Demo `SizeBody`/`SizeCaption`；共享 `DataGrid` 和 `DataGridColumnHeader` 使用 UI 字体链、正文/表头字阶和 Medium 表头字重。
- 不要为解决表格文字大小在页面局部加另一套字号；`44 DIP` 行高、`36 DIP` 表头、排序箭头、列宽调整、选中态、内部滚动和 Recycling 虚拟化继续由共享生产样式负责。
- 最新验证：`artifacts/gsc-b/ui-271-table-typography-v1` Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 262 通过/62 跳过/0 失败；source/WPF/diff 门禁通过；`artifacts/ui-qa/ui271-table-typography-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖七页双主题、多尺寸、滚动和 resize。已抽查 Save/Task/Maintenance 表格截图，但真实 Playnite 宿主的字号、DPI、键盘焦点和列宽拖动仍需验收。

## 2026-08-21 UI-270 共享折叠栏动效交接

- `Themes/DesignTokens.xaml` 的 `GscDisclosureCardExpander` 已按 Demo `LabDisclosure` 增加 Chevron 150ms 展开/收起旋转，`GscDisclosureCard` 仍是页面统一使用的共享入口；不要在单个页面复制另一套 Expander 模板。
- 本阶段保留整行点击、键盘焦点、真实 `IsExpanded` 绑定、内容显隐、页面滚动和生产 ScrollBar；没有改动业务命令、数据、虚拟化或当前游戏选框。
- 最新验证：`artifacts/gsc-b/ui-270-disclosure-animation-v1` Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 261 通过/62 跳过/0 失败；source/WPF/diff 门禁通过；`artifacts/ui-qa/ui270-disclosure-animation-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖七页双主题、多尺寸、滚动和 resize。动效时间曲线仍需在可识别 Playnite 宿主中用鼠标/键盘验收。

## 2026-08-21 UI-269 Demo 核心主题配色交接

- `AdaptiveThemePaletteFactory.ApplyDemoCoreResources` 现在由生产 Shell 和 Settings 共用，普通浅/深色主题固定 Demo 的画布渐变、卡片/侧栏/顶栏、输入框、正文层级、表格、分段控件、滚动条、遮罩及成功/警告/错误/信息状态关系；宿主只继续影响非核心 Accent/focus 交互。
- 高对比度仍绕过 Demo 核心覆盖并使用系统自适应资源。不要把宿主 `Background`/`Foreground` 中性刷重新应用到迁移页面核心表面，也不要为配色修复替换生产 Tab chrome、当前游戏选框或滚动条交互。
- 当前验证：`artifacts/gsc-b/ui-269-demo-palette-v2` Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 260 通过/62 跳过/0 失败；source/WPF/diff 门禁通过；`artifacts/ui-qa/ui269-demo-palette-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖七页双主题、多尺寸、滚动和 resize。已抽查浅/深色代表截图，但仍需可识别 Playnite 宿主的逐页像素、DPI、键盘焦点、主题切换和真实操作验收。

## 2026-08-21 UI-267 工作区表格测量与几何审计交接

- 媒体当前游戏媒体头部的操作区使用 `MinWidth=300`，搜索输入使用 `MinWidth=160`，所以标准及窄工作区不会把搜索框压成不可用的窄条；媒体搜索、筛选、卡片、预览 Inspector 和批量命令仍是真实绑定/命令。
- 存档历史页在工作区宽度 `<1240 DIP` 时采用紧凑历史列宽，以适配表格与 `360 DIP` Inspector 并列时的真实宿主内容宽度；状态列仍可通过 DataGrid Auto 横向滚动到达。不要通过隐藏状态列、移除 Inspector 或替换项目滚动条来解决此类宽度问题。
- `tests/GameSaveCenter.Playnite.Tests/OvernightV4SaveFormTests.cs` 的 `SharedWorkspaceBreakpointsKeepSearchAndHistoryEssentialsReadable` 是当前空间回归契约。生产 Tab chrome 是明确例外，当前游戏选框、共享 DataGrid 的排序/列宽拖动/Recycle 虚拟化、页面滚动和真实命令没有迁移。
- `tests/GameSaveCenter.RenderHarness/UiAudit/UiLayoutAnalyzer.cs` 现在按主控件直接 Grid 行计算填充，认可 Overview 有限活动视口和表格内部卡片布局，并把有实际横向滚动能力的列压缩记录为 `EXPECTED_HORIZONTAL_SCROLL`。最新审计 `artifacts/ui-audit-ui267-fix3/AUDIT_SUMMARY.md` 为 Fidelity 0、HIGH 0、MEDIUM 0、失败路由 0。
- 最新验证：`artifacts/gsc-b/ui-audit-layout-fix-v1` Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 259 通过/62 跳过/0 失败；source 门禁、WPF 校验（0 error、19 warnings、161 info）和 diff 门禁通过；`artifacts/ui-qa/ui267-layout-audit-fix-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖双主题、多尺寸、滚动和 resize。
- 总 Demo-first 迁移仍未宣布完成：自动/离屏证据不能替代可识别 Playnite 生产宿主中的逐页像素、DPI、键盘焦点、主题和真实操作验收；下一阶段继续针对目标文件逐页复核并收集宿主证据。

## 2026-08-21 UI-268 标题字体阶关系交接

- 生产共享令牌现在有三条明确字体链：`GscUiFontFamily` 使用 `Segoe UI Variable Text`，`GscDisplayFontFamily` 使用 `Segoe UI Variable Display`，`GscCodeFontFamily` 使用 `Cascadia Mono`；三者均带 `Segoe UI`/`Microsoft YaHei UI` 回退。
- `GscRedesignHeroTitle`、`GscRedesignFeedbackDialogTitle`、`GscPageTitleStyle`、生产 Shell 标题和 Dashboard 回退标题使用 Display；分区标题仍使用正文族，避免把 Demo 的 `LabSection` 错误提升为 Display。
- 本阶段没有改变用户指定的生产 Tab chrome、当前游戏选框、生产滚动条、表格/列表虚拟化、真实命令或 Binding。验证已通过 Release 构建/测试、source/WPF 门禁、双主题多尺寸 RenderHarness；证据目录为 `artifacts/gsc-b/ui-268-display-font-v1` 与 `artifacts/ui-qa/ui268-display-font-v1`。
- 总目标继续未完成：仍需按目标文件逐项收口七页信息架构、Demo 颜色/控件/状态和可访问性证据，并在可识别 Playnite 生产宿主中完成逐页像素、DPI、键盘焦点、主题和真实操作验收。

## 2026-08-20 UI-266 存档维护指标阅读节奏交接

- 存档“比较与保留”页的新增/修改/删除差异指标，以及维护页的保留、容量、趋势、保留模拟、保护状态和本地镜像指标，已统一为 Demo 的“数值 → 标签 → 补充说明”节奏；真实绑定和只读/安全语义没有改变。
- `LastBackupDiff`、Snapshot、保留模拟、存储趋势和本地镜像等真实状态继续作为唯一数据来源；没有迁移 Demo Mock 数据，也没有改变任何命令、Inspector、DataGrid/列表虚拟化、滚动或生产 Tab chrome。
- 最新验证：XAML 18/18，Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过/0 失败；source/WPF/diff 门禁通过；`artifacts/ui-qa/metrics-rhythm-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖双主题、多尺寸、滚动和 resize。仍需在可识别的 Playnite 生产宿主中完成逐页像素、DPI、键盘焦点及真实操作验收。

## 2026-08-20 UI-265 维护诊断概览环境健康区交接

- 维护页诊断概览已按 Demo 顺序把六项真实健康卡前置到 `DiagnosticHealthCard`，随后是 `EnvironmentCheckCard` 和 `MaintenanceDiagnosticsActionCard`；不要恢复到“更多维护操作”内部。
- `DiagnosticHealthPanel` 继续绑定 Worker/Ludusavi/Rclone、数据/媒体目录、待归类媒体和设备状态；环境检查展开项、诊断复制/导出、自检、目录日志、索引重建、任务协调、元数据灾备、路径迁移和安全模式入口均保留。宽屏 4 列、中等 2 列、窄屏 1 列由现有响应式代码管理。
- 最新验证：XAML 18/18，Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过/0 失败；source/WPF/diff 门禁通过；`artifacts/ui-qa/maintenance-health-order-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖双主题、多尺寸、滚动和 resize。生产 Tab chrome 未动，仍需在可识别的 Playnite 生产宿主中完成逐页像素、DPI、键盘焦点及真实操作验收。

## 2026-08-20 UI-264 首页统计条连续结构交接

- `OverviewView` 顶部 `OverviewStatStrip` 已按 Demo 恢复为一个连续统计条：六个等宽指标、五条分隔线、26 DIP 数值且数字位于标签上方；不要恢复六张独立 metric card 或旧的列数响应式逻辑。
- 六项统计仍绑定真实 `Snapshot`，匹配率与风险率进度条、空游戏库隐藏保护和健康明细均保留；今日工作台、当前游戏选框、“立即备份/全部备份”、活动列表滚动/虚拟化及真实命令没有因视觉迁移被删除。
- 最新验证：XAML 18/18，Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过/0 失败；source/WPF/diff 门禁通过；`artifacts/ui-qa/overview-summary-strip-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖双主题、多尺寸、滚动和 resize。生产 Tab chrome 仍保持当前项目实现，这是用户明确的例外；仍需在可识别的 Playnite 生产宿主中完成逐页像素、DPI、键盘焦点及真实操作验收。

## 2026-08-20 UI-263 任务统计条连续结构交接

- `TaskCenterView` 顶部 `TaskSummaryPanel` 已按 Demo 恢复为一个连续统计条：四个等宽指标、三条分隔线和 26 DIP 数值；生产 Tab/页签 chrome 例外规则不受本阶段影响。
- 计数仍来自真实 `Tasks.Count`、`RunningTaskCount`、`RetryableTaskCount`、`CompletedTaskCount`；任务筛选、更多筛选、任务队列 DataGrid、右侧详情 Inspector、复制错误/重试/取消命令没有迁移或删除。后台命名元素类型同步为 `Border`，摘要不再通过 `Columns` 响应式重排。
- 最新验证：XAML 18/18，Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过/0 失败；source/WPF/diff 门禁通过；`artifacts/ui-qa/task-summary-strip-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖双主题、多尺寸、滚动和 resize。仍需在可识别的 Playnite 生产宿主中完成逐页像素、DPI、键盘焦点及真实操作验收。

## 2026-08-20 UI-262 媒体统计条连续结构交接

- `MediaCenterView` 顶部 `MediaSummaryPanel` 已按 Demo 恢复为一个连续统计条：四个等宽指标、三条分隔线和共享 `GscRedesignSectionCard`；生产 Tab 页签仍保留项目当前 Tab chrome，这是用户明确要求的例外。
- 统计值继续来自真实 `MediaSummary`/`Snapshot` 绑定；待归类 DataGrid、媒体预览、Inspector、目标游戏选择、归类/忽略/保留副本等入口没有迁移或删除。`MediaSummaryPanelElement` 后台类型同步为 `Border`，来源规则的 `MediaSourceFields` 仍是独立 `UniformGrid`。
- 最新验证：XAML 18/18，Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过/0 失败；source/WPF/diff 门禁通过；`artifacts/ui-qa/media-summary-strip-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖双主题、多尺寸、滚动和 resize。仍需在可识别的 Playnite 生产宿主中完成逐页像素、DPI、键盘焦点及真实操作验收。

## 2026-08-20 UI-253 修改器中心分段结构交接

- 当前 `TrainerCenterView` 已按 Demo 恢复 `TrainerSegmentTabs` + `LabSegmented` 顶部分段导航，四个真实面板为 `PanelTools`、`PanelImport`、`PanelCatalog`、`PanelReleases`；旧 `TabControl/TabItem` 不再是页面主导航。
- 面板切换由 `OnTrainerSegmentChanged` 管理，只改可见性，不改业务数据或命令。必须保留工具导入、待确认 EXE 选择、FLiNG 搜索/刷新、版本加载/下载、工具编辑 Inspector 和紧凑详情入口。
- `TrainerToolsList`、目录结果、发行版本仍保留项目 ScrollBar、`CanContentScroll`、回收虚拟化和现有 `ApplyResponsiveLayout`；Demo segmented 导航仅包含 4 个标签，不应为了源码门禁给它添加大列表虚拟化要求。
- 构造期 `SelectionChanged` 的空保护是必要的：XAML 的 `SelectedIndex=0` 可能在四个面板字段完全生成前触发事件。
- 最新验证：Release 构建 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 252/252 通过、61 跳过；XAML/source/WPF 静态门禁通过；RenderHarness `render-qa OK`，证据为 `artifacts/ui-qa/trainer-segmented-final`，覆盖四个分段、双主题、多尺寸和 resize。尚无新的可识别 Playnite 宿主逐页像素证据。

## 2026-08-20 UI-252 存档历史页操作卡交接

- 默认“历史版本”页已补回 Demo 的 `SaveHistorySummaryCard`：真实版本数、当前规则/健康摘要，以及“立即扫描 / 重新校验 / 刷新详情”入口均位于历史表上方；对应命令仍是 `DetectPathsCommand`、`ValidateCommand`、`LoadDetailsCommand`。
- 700 DIP 以下摘要操作区转为第二行，正常宿主保持横向布局；历史 DataGrid 的列宽拖动、排序、虚拟化和现有滚动条未改变。
- 当前验证：XAML 18 个文件通过，Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 251 通过/61 跳过/0 失败，RenderHarness `render-qa OK`。仍需在可识别的 Playnite 生产宿主中逐页核对，不得以离屏 PNG 代替真机像素证据。

## 2026-08-17 UI-221 AcrylicFork 整页视觉迁移收口交接

- 本轮以 `GameSaveCenter.AcrylicFork` @ `b09cba6` 复核了 Overview/Save/Trainer/Media/Task/Maintenance/Settings 的生产骨架，并补齐 Media、Trainer、Save、Maintenance 分段导航右侧的样板说明与真实状态/计数。这些信息必须继续绑定真实数据，不能写死 demo 数字。
- 不要创建 `AcrylicParity.xaml` 这类独立 Lab 别名字典：独立 ResourceDictionary 的 `BasedOn` 无法引用父级合并字典的 Gsc 样式，页面构造会直接抛 `StaticResourceHolder` 异常。共享视觉键统一用生产 `Gsc*`。
- 生产滚动条、圆角表头、Item scrolling/Recycling 虚拟化、真实命令/绑定保持优先；右上角色板、预览徽标、Mock 数据和 demo 滚动条不迁移。
- 本轮验证：Core 59/59、Worker 191/191、Playnite 304/304；Release 0 warning/0 error；`render-qa OK` 证据在 `artifacts/ui-qa/acrylic-full-migration-v1`；真实宿主 0.6.70 安装加载成功，审计证据在 `artifacts/ui-host-audit-acrylic-v1`，但 `EmbeddedDashboardCaptured=false`，需要用户真实点击 Playnite 侧栏后再重跑 `real-host-audit.ps1` 才能补齐嵌入像素真值。

## 2026-08-16 UI-220 UiLab 几何对齐与媒体回滚修复交接

- `GscRedesignWorkspaceTabItem` 的 `HorizontalContentAlignment`/`VerticalContentAlignment` 必须保持 `Stretch`；标题的居中由模板内的 `HeaderContent` 完成。不要把标题对齐属性重新绑定给页面内容。
- `MaintenanceDiagnosticsSubTabs` 现在是 `ListBox` segmented control，不是 `TabControl`；内容由 `MaintenanceDiagnosticsFindingsPanel` 和 `MaintenanceDiagnosticsOverviewPanel` 切换。维护二级导航两项文字都必须保持可见。
- `VirtualizingWrapPanel.ResolveViewportHeight` 和 deferred recovery 是媒体网格滚动回顶的必要保护；继续保留生产虚拟化和滚动条，不要替换成 UiLab 的非虚拟化样例实现。
- 首页 `OverviewHeroColumn`/`OverviewCurrentGameColumn` 的宽度比例为 `1.35*:1`；744 DIP 左右的紧凑视口由 `ApplyResponsiveWidth` 堆叠，不能用旧的等宽比例恢复。
- 验证基线：Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 303/303；`render-qa OK`（双主题、多尺寸、resize、媒体滚动回顶）。WPF UI 校验无 error，但保留既有布局/主题资源 warnings。当前仍不能声称完成 Playnite 宿主逐页像素验收，因 Computer Use 无法稳定激活窗口。

## 2026-08-16 UI-219 UiLab 分段页面骨架直迁交接

- 本轮已把媒体、存档、修改器、维护四个生产页的主导航从旧 `TabControl/TabItem` 改成 UiLab 的 segmented navigation + named panel host；真实数据/命令/绑定、Inspector、DataGrid/ListBox 虚拟化和生产滚动条没有被 demo 样例替换。维护诊断/审计中的嵌套页签仍是页面内部层级。
- 新共享资源为 `GscRedesignSegmented`、`GscRedesignSegmentedItem`、`GscSegmentFillBrush`、`GscSegmentItemFillBrush`、`GscSegmentItemStrokeBrush`。继续修复时优先修改共享模板/令牌，不要对单个页面复制另一套圆角参数；UiLab 右上演示色板/窗口按钮/样例滚动条不属于生产迁移范围。
- 直接迁移后曾出现页面构造期 `SelectionChanged` 空引用，四个 `On*SegmentChanged` 已加入初始化期空保护；媒体虚拟网格的 `VirtualizingWrapPanel` 也已修复 generator 插入索引越界回退。若再次改动 XAML 面板顺序，必须先构造所有页面再运行 Playnite 测试。
- 证据：`validate-source.py`、WPF UI 校验、Release 构建通过；Core 59/59、Worker 191/191、Playnite 303/303；`DEV-INSTALL-008` 已安装 `0.6.70.0`，`extensions.log` 记录加载且无本轮新增崩溃。当前 Computer Use 只能捕获黑色 Playnite 窗口并返回 `EmptyWindowAutomationPeer`，激活失败；因此真实宿主逐页截图/像素对照仍需用户可交互窗口，不能把此次日志通过写成视觉真机验收。

## 2026-08-16 UI-218 UiLab 页面骨架迁入交接

- 本阶段把 `GameSaveCenter.UiLab` 的页面层级迁入生产工作区：页面头部保留唯一游戏上下文，取消 Dashboard 外层重复选中游戏卡、全局操作行和恢复安全横幅；安全提示归入策略/比较页面内部。
- 已收口的重点：备份策略三栏、比较指标窄宽可换行、修改器工具栏/拖入区分行、任务中心筛选分两行；真实数据、绑定、命令、Inspector、虚拟化和生产滚动条均保留。
- 明确不迁移 UiLab 右上演示色板、窗口按钮、样例数据和样例滚动条。用户指定的五类重叠只做防重叠修正，不能借机恢复生产页外壳或替换滚动模型。
- 验证：Release 0 warning/0 error，Core 59/59、Worker 191/191、Playnite 303/303；真实 Playnite 约 1303×673 视口人工检查首页、备份、修改器、任务页通过，任务筛选与修改器拖入区不重叠。自动审计仍以 `EmbeddedDashboardCaptured=false` 为准，不能把 Computer Use 截图写成自动嵌入证据。

## 2026-08-16 UI-217 真实 Playnite 人工视觉复核

- `81fde54` Release 安装包已在真实 Playnite 中加载；人工进入 GameSaveCenter 后确认首页和任务中心的主要 UiLab 层级已出现在实际宿主，且保留真实数据、命令、绑定、虚拟化和生产滚动条。
- 任务中心在约 1303×673 宿主视口中显示四项统计带、筛选区、圆角表头 DataGrid、状态胶囊和进度条；Inspector 由真实 `SelectedTask` 驱动，默认无选择时隐藏，不能把样板默认选中的失败任务详情当成生产必显内容。
- 本轮手动观察不改变 `real-host-audit` 的事实边界：自动侧栏定位仍失败，`EmbeddedDashboardCaptured=false` 继续有效；后续如需可复现像素证据，应在用户实际打开侧栏后重跑审计。

## 2026-08-16 UI-215 Task 统计栏当前基线

- 任务中心顶部现在是单一 `TaskSummaryBand`，四个真实计数位于同一圆角阅读面，三条 `GscDividerBrush` 分隔；不要恢复四张独立指标卡或 `TaskSummaryPanel.Columns` 换列逻辑。
- 生产任务队列的真实筛选、详情 Inspector、DataGrid 表头/滚动条、Item scrolling 和虚拟化均保留。窄于 760 DIP 时，`TaskGameFilterHost` 整体（标签 + 下拉框）进入“更多筛选”，避免孤立标签错位。
- 当前证据：`artifacts/ui-qa/task-summary-band-v2`，双主题、多尺寸和 resize 均 `render-qa OK`；真实 Playnite Dashboard 仍未自动捕获，离屏证据不能替代宿主视觉验收。
- `TaskGameFilterHost` 的父子层级测试已同步更新；如果后续继续把筛选项成组移入“更多筛选”，应验证组容器移动和内部真实 ComboBox，而不是检查 ComboBox 的直接父级。

## 2026-08-16 UI-214 Overview 单卡统计栏当前基线

- 首页 Overview 的六项统计已收敛成 UiLab 风格的单一圆角 `OverviewStatBand`，内部是六个真实 `Snapshot` 指标和五条分隔线；不要恢复六个独立卡片或为指标恢复悬停位移动画。
- Dashboard 根层只保留一个 `GscAmbientAccentBrush` 磨玻璃晕影；右上角演示色板/窗口控制和 UiLab 滚动条仍明确不迁移，生产滚动条、页面滚动、虚拟化、命令与绑定继续作为事实来源。
- 当前证据：`artifacts/ui-qa/overview-single-band-v1`，覆盖双主题、多尺寸与 resize transition，`render-qa OK`。真实宿主 Dashboard 仍没有自动捕获到嵌入像素，不能把离屏图当作 Playnite 视觉真值。

## 2026-08-16 UI-213 真实宿主审计交接

- 当前 `420483f` 已执行 Release 真实宿主审计：构建、Core 59/59、Worker 191/191、Playnite 303/303、安装与 Playnite 启动通过；人工进入真实 Playnite 后确认 GameSaveCenter Settings 宿主窗口可见。
- `artifacts/ui-host-audit/summary.json`：`EmbeddedSettingsCaptured=true`、`EmbeddedDashboardCaptured=false`、`ControlledDashboardCaptured=true`、`ProductionVisualSourceOfTruthAvailable=false`。因此 Settings 的 `EmbeddedPlaynite` 截图有效，Controlled Dashboard 截图只能作为辅助布局证据，不能宣称为生产嵌入视觉真值。
- 自动 UIAutomation 仍未找到 Playnite 左侧 GameSaveCenter 入口；Media 缩略图网格尚未完成真实宿主视觉验收。后续人工 QA 优先进入真实 Media 页检查 164×142 卡片、实际缩略图、滚动和选中/Inspector 行为，再检查 DPI、主题、键盘焦点和连续缩放。

## 2026-08-16 UI-205-ACRYLIC-PARITY 当前开发基线

- 权威参考是 `D:\workplace\github\GameSaveCenter.AcrylicFork` @ `b09cba6`，不是 UiLab；其页面视觉与结构已迁入生产共享资源。AcrylicFork 演示数据、右上角颜色/主题按钮和样例滚动条明确不迁移，七组顶部色值仅用于主题参考。
- 生产保留真实命令、绑定、虚拟化和现有滚动条；Overview 普通状态不再显示重复全局命令卡，Settings 常用宽度使用左侧分类栏，低于 700/620 DIP 进入紧凑布局。
- `DEV-INSTALL-008` 已修复外来扩展 Worker 误杀：只处理当前生产目录，陌生但可读路径保留运行，无法读取路径仍 fail-closed。
- 自动验证已通过：Core 59/59、Worker 191/191、Playnite 302/302、Release 0 warning/0 error、source/XAML 门禁和 `render-acrylic-correction`；一键安装已成功启动 Playnite/生产 Worker。真实宿主最终主题/DPI/键盘/连续缩放仍需人工确认。

## 2026-08-16 UI-205-REAL-HOST-MIGRATION-FIX 当前开发基线

- 已修复生产 XAML 对外部 Contracts 程序集的 BAML 直接解析风险：Settings/Media/Trainer 使用 `GameSaveCenter.Playnite.XamlValues` 本地枚举包装，Dashboard 删除未使用的外部 namespace；不要恢复 `assembly=GameSaveCenter.Contracts` 的 XAML 直接引用。
- Dashboard 选中游戏头部现在以最终实际 viewport 宽度约束，窄屏操作行固定可用宽度并换行；WPF Grid 重排后在 ApplicationIdle 做二次响应式布局，避免 1600/1366/1280/1024 DIP 首次测量残留越界。
- Real Host 审计在 Render 后等待 ApplicationIdle，干净轮次清理旧 `CHILD_LAYOUT_OVERFLOW`；最终受控矩阵 `RealFixedLayoutOverflow=[]`。自动 UIAutomation 仍找不到真实侧栏，必须保留 `EmbeddedDashboardCaptured=false`，不要把 Controlled Host 截图宣称为生产视觉真值。
- 当前基线：Core 59/59、Worker 191/191、Playnite 303/303，Release 0 warning/0 error；安装验证 `0.6.70` / DLL `0.6.70.0`，生产日志无新的 XamlParseException。

## 2026-08-16 UI-REAL-HOST-AUDIT-BLOCKERS-FIX 实施完成

- Real Host Audit 阻塞项已收口：CommitSha 可追踪、SafeFileName 修复、overflow 分类 gate、resize 稳定截图、manifest Scope 隔离、内部滚动器过滤、Embedded 身份显式判定。
- headless 无人点击时 summary 诚实写 `EmbeddedDashboardCaptured=false` + HIGH gate；用户在 Playnite 点击 GameSaveCenter 后重跑 `scripts/real-host-audit.ps1` 即可得到真实 embedded 证据。
- 报告：`docs/ai/REAL_HOST_AUDIT_BLOCKERS_FIX_REPORT.md`。
- 基线：Playnite 302/302、Worker 191/191、Core 59/59；Release 0 warning/0 error。

## 2026-08-16 UI-HOST-AUDIT-TRUTHFULNESS-FIX 实施完成

- Real Host Audit 已可信化：Sidebar View 不再调用 `Activated`，等待用户真实打开；origin 显式（EmbeddedPlaynite/ControlledAuditWindow）；manifest 按 session/scope 隔离；DataGrid 逻辑滚动器不生成像素长图；`summary.json` 硬门禁 + `CHILD_LAYOUT_OVERFLOW` gate。
- 报告：`docs/ai/HOST_AUDIT_TRUTHFULNESS_FIX_REPORT.md`。
- 基线：Playnite 294/294、Worker 191/191、Core 59/59；Release 0 warning/0 error。

## 2026-08-16 UI-REAL-HOST-CAPTURE-COMPLETENESS-FIX 实施完成

- Real Host Audit 已按 Capture Contract 重构：embedded-current / controlled-host-window / ScrollSurfaceFull 三类输出，`capture-manifest.json` + gates + 完整性断言。
- Controlled host 用无边框窗口，profile 即 client size，Dashboard Stretch 且不写死 Width/Height；embedded 模式绝不 resize Dashboard 或覆盖主题。
- 关键文件：`RealHostUiAuditService.cs`、`UiDiagnosticsExporters.cs`、`UiAuditCaptureContractTests.cs`；报告 `docs/ai/REAL_HOST_CAPTURE_COMPLETENESS_FIX_REPORT.md`。
- 基线：Playnite 287/287、Worker 191/191、Core 59/59；Release 0 warning/0 error。

## 2026-08-15 LUDUSAVI-DIAGNOSTICS-FIX 实施完成

- 已修复：外部进程输出 UTF-8 解码；Ludusavi 失败时保留 `RawOutput`；剪贴板复制重试与失败降级（`CopyTextWithRetry`）。
- 备份失败根因常为 Ludusavi manifest 下载超时，重试即可；插件保留原始输出便于诊断。
- 基线：Worker 191/191、Playnite 281/281；Release 0 warning/0 error。

## 2026-08-15 UI-REAL-HOST-AUDIT-NESTED-TABS-THEMES 实施完成

- 真机审计已覆盖嵌套 Tab（如“异常与审计”→“审计记录”）、浅色/深色双主题、5 档窗口尺寸；整页截图渲染完整 Dashboard 外壳并按内容高度撑高。
- 关键实现：递归捕获嵌套 TabControl（ApplicationIdle 等待 + 视觉树/Content 双路径）；`DashboardView`/`GameSaveCenterSettingsView` 提供 `ApplyThemeForAudit`；设置兜底注入真实 Settings DataContext。
- 最新产物：`artifacts/ui-host-audit/screenshots/<size>/<light|dark>/` 与 `artifacts/GameSaveCenter-ui-host-audit.zip`。

## 2026-08-15 UI-REAL-HOST-AUDIT-MULTI-SIZE 实施完成

- 真机审计现覆盖 5 档窗口尺寸（maximized + 1600x1000/1366x768/1280x720/1024x768），每档包含 6 个工作区、全部内层 Tab、窗口截图与 Settings 5 分类；产物在 `artifacts/ui-host-audit/`。
- 修复要点：Playnite DPI-unaware，窗口尺寸用 `SystemParameters.WorkArea` + `SizeToContent.Manual`；内容按逻辑分辨率输出防 OOM；多尺寸扫描不做全页滚动拼接。
- 文件清理规则已加入 AGENTS.md 与本文档：每轮完成后清理旧构建/旧审计/旧 zip 与 `.tmp` 一次性目录，只保留当前安装目录和审计证据。

## 2026-08-15 UI-REAL-HOST-AUDIT-FULL-COVERAGE 实施完成

- 真实宿主审计修复收口：Dashboard 6 个 workspace + 全部内层 Tab + 完整窗口截图 + 整页拼接；Settings 5 个分类全部截图。
- 修复要点：无窗口会话使用专用 1440×900 兜底窗口；Settings 兜底使用缓存输出根 + Dashboard UI Dispatcher；设置分类名从 Header 视觉树提取；zip 占用时写唯一文件名。
- 最新证据：`artifacts/ui-host-audit/` 与 `artifacts/GameSaveCenter-ui-host-audit.zip`；DPI 1.5。
- 回归：Release 0 warning/0 error；`validate-source.py`、`check-xaml.ps1`、WPF UI 校验 0 errors；Core 59/59、Worker 190/190、Playnite 281/281。
- 剩余人工项：用户实际 Playnite 窗口、第三方主题、连续缩放下的最终视觉确认。

## 2026-08-15 UI-REAL-HOST-PARITY-CLOSURE 实施完成

- 计划：`docs/ai/REAL_HOST_UI_PARITY_CLOSURE_PLAN.md`；报告：`docs/ai/REAL_HOST_UI_PARITY_CLOSURE_REPORT.md`。
- Tier A 离屏审计与 Tier B 真实宿主审计分层；`scripts/real-host-audit.ps1` 从真实 Dashboard 捕获证据。
- `RealHostUiAuditService`、`UiDiagnosticsExporters`、`AdaptiveThemePaletteContrastGuard` 已加入。
- 本机真实宿主证据已生成；离屏更漂亮的根因是 fallback vs runtime adaptive palette 等环境差异。
- 协定：每轮开发完成后由 Agent 自己 commit 并 push（AGENTS.md 已同步）。
- 基线：Playnite `281/281`；render-qa 全绿；Offscreen UI Audit 0 HIGH/0 MEDIUM/0 fidelity/0 failed routes。

## 2026-08-15 UI-AUDIT11-RESIDUAL-CLOSURE 实施完成

- 计划：`docs/ai/UI_AUDIT11_RESIDUAL_UI_CLOSURE_PLAN.md`；报告：`docs/ai/UI_AUDIT11_RESIDUAL_UI_CLOSURE_REPORT.md`。
- SaveHistory 大小列不再 ellipsis（116 DIP + `SaveSizeValue`）；Device Inspector 改为 Compact/Narrow 详情切换，展开 viewport >= 180 DIP。
- Audit 新增 `SHORT_SEMANTIC_VALUE_TRIMMING` / `INTERACTIVE_INSPECTOR_USABILITY` 失败门禁。
- 基线：Playnite `276/276`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 fidelity/0 failed routes。
- 真实 Playnite 宿主主题/DPI 125%/150%/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FIDELITY-CLOSURE-AUDIT10 实施完成

- 计划：`docs/ai/UI_FIDELITY_CLOSURE_AUDIT10_PLAN.md`；报告：`docs/ai/UI_FIDELITY_CLOSURE_AUDIT10_REPORT.md`。
- Maintenance 中间列表头恢复渲染；Media 搜索框可伸展；Settings 分类自动 scroll-into-view；Save History narrow 状态列保留。
- UI Audit 新增 4 类 fidelity 失败门禁，修复前 92 个失败 → 修复后 0。
- 基线：Playnite `273/273`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 failed routes/0 fidelity。
- 真实 Playnite 宿主主题/DPI 125%/150%/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-POST-TYPOGRAPHY-GEOMETRY-CLOSURE 实施完成

- 计划：`docs/ai/UI_POST_TYPOGRAPHY_GEOMETRY_CLOSURE_PLAN.md`；报告：`docs/ai/UI_POST_TYPOGRAPHY_GEOMETRY_CLOSURE_REPORT.md`。
- 诊断“等级”列 72 → `GscSeverityColumnWidth` 92 DIP；异常审计同列统一 token。
- UI Audit 新增 Text-Fit 检测并作为失败门禁；visual-tree exporter 修复后 175 个 JSON 非空。
- 基线：Playnite `268/268`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 failed routes/0 TEXT-FIT。
- 真实 Playnite 宿主主题/DPI 125%/150%/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FINAL-TYPOGRAPHY-RESPONSIVE-CLOSURE 实施完成

- 计划：`docs/ai/UI_TYPOGRAPHY_RESPONSIVE_CLOSURE_PLAN.md`；报告：`docs/ai/UI_TYPOGRAPHY_RESPONSIVE_CLOSURE_REPORT.md`。
- 字体链统一：`GscUiFontFamily`（含 Microsoft YaHei UI fallback）；普通 UI 不再硬编码 Segoe 组合；图标/代码字体保留；按钮默认字重 Medium，Primary SemiBold。
- Settings Compact/Narrow header 收紧，正文 viewport 不再只剩几十 DIP；Save Compare Narrow 主比较区直接可见；Compact Inspector 详情按钮独立操作行；Media 待归类底栏对齐并保留换行。
- 基线：Playnite `266/266`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 失败路由。
- 真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FINAL-POLISH-V7.1 实施完成

- v7.1 计划：`docs/ai/UI_FINAL_POLISH_PLAN_V7_1.md`；报告：`docs/ai/UI_FINAL_POLISH_REPORT_V7_1.md`。
- 首页活动行五列、chip 独立列组、Time 右留白；全量 `POSSIBLE_CLIPPING=0`。
- 基线：Playnite `263/263`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；最终 Audit 0 HIGH/0 MEDIUM/0 失败路由。
- 提交：`702b0d5`、`f6f17a8`；Audit ZIP `artifacts/GameSaveCenter-ui-audit.zip`。

## 2026-08-15 UI-FINAL-CLOSURE-V7 实施完成

- v7 计划：`docs/ai/UI_FINAL_CLOSURE_PLAN_V7.md`；报告：`docs/ai/UI_FINAL_CLOSURE_REPORT_V7.md`。
- Audit 子路由可信、六张表 2K 列填率 1.00、Task/Media 纵向 fill 1.00、Maintenance 表头白块清零、Progress 对比与单行 TextBox 指标清零、Task 1040 outer scroll=0。
- 基线：Playnite `263/263`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；最终 Audit 0 HIGH/0 MEDIUM/0 失败路由。
- 最终 Audit ZIP：`artifacts/GameSaveCenter-ui-audit.zip`；提交 `5cd0226`、`58191d5`、`494b402`、`87d0553`、`7eaaacd`。
- 真实 Playnite 主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FEEDBACK-GLOBAL-ACTIVITY-CHIP-CENTER

- 首页“全局活动”的“备份成功”等结果气泡文字已居中；宽/窄两套 Kind/Result chip 的 TextBlock 统一三向居中，回归断言已锁定。
- 提交：`d962b4d`；Playnite `263/263`、XAML/source 门禁通过，截图 `artifacts/ui-qa/v6-2-shots/`。

## 2026-08-15 UI-TABLE-AND-CHIP-CLOSURE-V6.2 实施完成

- v6.2 计划：`docs/ai/UI_TABLE_AND_CHIP_CLOSURE_PLAN_V6_2.md`；报告：`docs/ai/UI_TABLE_AND_CHIP_CLOSURE_REPORT_V6_2.md`。
- Chip 改圆角矩形（CornerRadius 7）、时间列右留白 20 DIP、Overview 六列 `40|150|*|96|84|112`、SaveCandidate 可信度真实 ProgressBar。
- Maintenance 四个主表取消 460 DIP 上限，Device/Process 布局改 Stretch；2K/4K fill ratio 见报告。
- 基线：Playnite `263/263`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/8 EXPECTED INFO。
- v6.2 截图：`artifacts/ui-qa/v6-2-shots/`；命令 `scripts/capture-v6-2-shots.ps1`。
- 提交：`c58b359`、`6a68a59`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-OVERNIGHT-CLOSURE-V6 实施完成

- v6 计划：`docs/ai/UI_OVERNIGHT_CLOSURE_PLAN_V6.md`；报告：`docs/ai/UI_OVERNIGHT_CLOSURE_REPORT_V6.md`。
- 页面历史为 Playnite 会话级；Tasks/Maintenance 保持不显示 GamePicker；数字输入根模板修复；全局活动六列表格；Overview/Maintenance/Media 真实父子滚动清零。
- 基线：Playnite `261/261`；render-qa 全绿；UI Audit 0 HIGH/0 MEDIUM/8 INFO/0 TRUE_PARENT_CHILD_SCROLL_CONFLICT。
- 截图：`artifacts/ui-qa/v6-shots/`；命令 `scripts/capture-v6-shots.ps1`。
- 提交：`baa8f72` 及后续提交见 `git log`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-OVERNIGHT-FIX-V4 实施完成

- v4 计划：`docs/ai/UI_OVERNIGHT_FIX_PLAN_V4.md`；报告：`docs/ai/UI_OVERNIGHT_FIX_REPORT_V4.md`。
- 折叠统一：`GscDisclosureCard` 独立 chevron 图标区，全项目无旧 `GscExpander`、无尾部 `>`、无折叠体内滚动。
- 维护中心：诊断页二级 Tab（默认 `问题列表`，次项 `诊断概览`），FindingsGrid 独占主工作区。
- 存档中心：备份自动化与模板数值输入全部带 label/unit/helper；首页活动行紧凑化。
- 基线：Core `59/59`、Worker `190/190`、Playnite `255/255`；render-qa 全绿；UI Audit 0 HIGH/0 MEDIUM。
- 截图：`artifacts/ui-qa/v4-shots/`；命令 `scripts/capture-v4-shots.ps1`。
- 提交：`3015182`、`5131e4d`、`0201615`、`5196f4a`、`fc86ecc`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-14 UI-VISUAL-REWORK-V3 实施完成

- v3 计划：`docs/ai/UI_VISUAL_REWORK_PLAN_V3.md`；五张用户截图对应问题已全部关闭（Overview 三按钮/保护三层/活动轻量表、Save 状态 Badge、Maintenance 折叠与五列表头）。
- 所有 Expander/Disclosure 统一到 `GscDisclosureCard`，Chevron 独立、整行可点、无尾部 `>`；Media/Save/Task 旧 `GscExpander` 引用已全部替换；Maintenance 主 Disclosure 使用内部有限滚动。
- 功能保真：REMOVE=0，命令/绑定/DataGrid 列/虚拟化/GamePicker HARD LOCK 未改。
- render-qa 新增按钮对齐探针；Overview Hero/当前游戏列按 1:1 布局，1536×864 下三按钮仍同排同高。
- 自动化基线：Release 0 warning/0 error；Core `59/59`、Worker `190/190`、Playnite `253/253`；render-qa 10 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/32 INFO/0 失败路由。
- 提交：`5c3bdae`（计划）、`9ee3660`（Overview）、`e8b8c31`（Save/Maintenance），最终补强提交见 `git log`；v3 截图 `artifacts/ui-qa/v3-shots/`（脚本 `scripts/capture-v3-shots.ps1`），Audit `artifacts/ui-audit/v3-final/AUDIT_SUMMARY.md`。
- 真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`，不得冒充已人工验收。

## 2026-08-14 UI-AUDIT-001 开发专用 UI 自动审计

- 一条命令：`.\scripts\capture-ui-audit.ps1` 或双击 `GameSaveCenter-UI-Audit.cmd`。
- 输出：`artifacts/ui-audit/`（README、UI_MANIFEST、UI_ROUTE_MAP、UI_FIDELITY_MATRIX、LAYOUT_REPORT、AUDIT_SUMMARY、visual-tree、layout、screenshots）与 `artifacts/GameSaveCenter-ui-audit.zip`。
- 静态盘点自动发现未来新增页面/Tab/控件；运行时整页截图覆盖页面级滚动容器，DataGrid/ListBox 内部滚动也从头到底拼接。
- 当前基线：Core `59/59`、Worker `190/190`、Playnite `250/250`；source/XAML/WPF 门禁与 10 档 render-qa 通过；扩档审计 161 快照、0 HIGH/0 MEDIUM/0 失败路由。
- Commit：见当前 `git log -1`。

## 2026-08-14 UI-REFACTOR-V1 扩档验证与 Audit 高度对齐

- `render-qa` 矩阵已覆盖 1040×700、1100×720、1280×720、1366×768、1536×864、1600×900、1707×960、1920×1080、2048×1152、2560×1440，10 档全部通过。
- UI Audit 新增 `2k` 与 `narrow-1100` 尺寸；工作区 `ApplyResponsiveLayout` 现在接收窗口高度（与生产 Dashboard 和 render-qa 一致），不再使用内容高度造成 1100×720 维护设备/进程表 240 DIP 的误报。
- 扩档后最终审计：HIGH `0`、MEDIUM `0`、运行时警告 39、失败路由 `0`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-14 UI-REFACTOR-V1 强制 Light/Dark 主题 QA

- RenderHarness 新增 `RunThemeQa`，复用生产 `AdaptiveThemePaletteFactory` 对 7 个工作区 × 1040×700 / 1100×720 / 1366×768 / 2560×1440 × Light/Dark 渲染，共 56 个离屏场景；调色板、主表视口和页面滚动面断言全部通过。
- `GameSaveCenter.Playnite.csproj` 仅向 RenderHarness 开放 `InternalsVisibleTo`，生产 UI 与业务未改。
- 像素采样确认主题切换有效：Light 背景约 `239,240,243`，Dark 约 `54,57,67`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。
- render-qa 与主题 QA 同时加入页面级横向溢出门禁：`*ScrollSurface` / `SettingsScroller` 必须 `hbar=Disabled` 且无横向溢出；DataGrid 内部列滚动允许。10 档尺寸与 56 主题场景全部通过。
- `render-qa` 新增 Resize 恢复探针（2560×1440 → 1100×720 → 2560×1440 同实例布局快照对比），修复 Save/Task/Trainer Inspector 宽窗不恢复缺陷，并新增 3 条回归测试。
- 实施包验收审计已落盘到 `docs/ai/UI_REFACTOR_ACCEPTANCE_AUDIT.md`；真实 Playnite 宿主主题/DPI/连续缩放与大数据滚动仍为 `MANUAL QA REQUIRED`。
- 真实宿主 reload 已验证：`dev-install-run.ps1 -Configuration Release` 成功，Playnite 加载 `GameSaveCenter 0.6.70`，扩展日志确认 `0.6.70.0 loaded`，Worker 从当前扩展目录运行，`18:10` 后无 ERROR/Exception/crash。
- Visual Correction v2 已实施：Overview 单滚动、风险卡去内滚、Disclosure、活动行响应式、Save 卡片、Diagnostics 单滚动、Audit 二级切换；新增 OV/SAVE/MAINT 断言，最终 Audit 0 HIGH/0 MEDIUM/33 运行时警告。
- Visual Correction v2 真实宿主 reload 已验证：`20:39:57` 插件加载、`20:39:59` 扩展初始化、Worker 从当前扩展目录运行，之后无 ERROR/Exception/crash。

## 2026-08-14 Final Code Gap Closure 与最终 Epic 状态

- 当前基线：Core `59/59`、Worker `190/190`、Playnite `235/235`；Release 构建 0 warnings / 0 errors；source、XAML、WPF 静态门禁和 `render-qa` 通过；fault-injection 与 1000 轮 soak 通过。
- 已交付：多设备强内容指纹/保守分歧判断、可取消的流式 Restore Readiness、按实际目录分盘检查、重复 Manifest 防护、结构化且集中脱敏的诊断包、`ImportantOnly/Summary/Verbose` 通知级别、带启动失败自动请求与恢复正常按钮的安全模式、覆盖孤儿归档/Manifest/磁盘空间/四态结果的完整性自检、旧版数据库 Fixture 升级 Harness、自身元数据灾备 ZIP 与安全恢复流程、带只读预览与确认的备份仓库索引重建、带预览/目标缺失策略/自动灾备的批量路径迁移、按 Worker 会话与任务类型分类的中断任务协调、带显式兼容矩阵的单游戏操作锁、带 AppVersion 与能力列表的 IPC protocol handshake、覆盖设置/媒体/元数据恢复/启动计数且旧文件完整保留的共享原子写入器、数据规模与资源监控 Soak Harness、覆盖 ZIP/数据库/原子写/外部进程/任务/广播/锁的 15 类故障注入 Harness、通知级别正式收口与同 Session 去重测试、未分类自定义启动项反作弊授权语义、Onboarding 测试备份真实生产链路审计、只读备份存储分析、全局保留策略模拟器、第二本地镜像、首页全局活动时间线、Playnite 游戏右键快捷操作、修改器中心拖拽导入、合理 UI 状态持久化、键盘快捷搜索与自动化名称、统一页面状态控件、设置即时验证摘要、用户可读维护健康报告（复制/导出 TXT/Markdown）、本轮四项缺口（Repository Rebuild 空库灾难恢复、元数据灾备含 Playnite 插件设置、Workspace 状态控件覆盖存档/工具/媒体/维护、Local Mirror SHA256 内容校验），以及 Metadata 跨进程原子回滚与状态控件 TargetType 崩溃修复。
- 真实开发安装已通过：Playnite 扩展 `0.6.70` 加载，Worker 从当前扩展目录启动并记录 `Application started`。安装器仍默认不请求管理员权限；沙箱无法写入 Playnite 扩展目录时，需在正常用户环境或获授权外部环境运行安装器。
- 当前阶段 commit：`cbd0cdb`、`c6bfda2`、通知级别/安全模式/完整性自检/迁移 Harness/元数据灾备/索引重建/路径迁移/任务协调/操作锁/握手提交见紧随其后的 commit；原子写入提交为 `f1eda41`，稳定性测试提交为 `8132419`，故障注入提交为 `f91992e`，A-HARDEN 提交为 `fbbdb90/8d6c36c/5666944`，B01+B02 升级提交为 `17e88b6`，B03 提交为 `a064108`，B04 提交为 `55f532f`，B05 提交为 `97b06b5`，B06 提交为 `dff0cf4`，B07 提交为 `bddcfdd`，B08 提交为 `47685bd`，B09 提交为 `7dba09c`，B10 提交为 `7ee738d`，B11 提交为 `eed946c`，B12 提交为 `c9e053a`，B13 提交为 `738f339`，Layer B Audit 提交为 `25b191f`，C01 提交为 `2ef8f13`，C02 提交为 `a27f430`，C03 提交为 `387149f`，C04 提交为 `7f38b15`，C05 提交为 `15ab1d7`，C06 提交为 `5d113c3`，C07 提交为 `e4513cb`，C08 提交为 `c907983`，C09 提交为 `f21cba4`，C10 提交为 `4614bb8`，C11 提交为 `9d465d2`；Final Code Gap Closure 提交为 `e58714c`、`0bcdce2`、`84f74fc`、`5a12d1e`；崩溃修复与 Metadata 原子回滚提交为 `a417b7c`、`13f21a5`。Layer B 13 项与 Layer C 11 项已全部交付；`PRODUCT_HARDENING_LAYER_C_AUDIT.md` 与 `PRODUCT_HARDENING_EPIC_FINAL_AUDIT.md` 已生成。
- 整体 Epic 状态：`PARTIALLY COMPLETED / MANUAL QA REQUIRED`；按 `docs/ai/FINAL_MANUAL_QA_CHECKLIST.md` 执行真实 Restore/Undo、Rclone、双设备、外置镜像、启动项、反作弊、主题/DPI/连续缩放与 1000+ 游戏库人工验收。
- `AUTO VERIFIED` 与 `MANUAL QA REQUIRED` 必须分开记录。真实 Restore/Undo、Rclone 断网、双设备、EXE/LNK/BAT/PS1、1000+ 游戏库、主题/DPI/连续缩放仍需人工验收。

## 2026-08-13 UI-QA-REAL-006 设置分类 Tab 实际裁切修复

- 上一轮分类栏底部安全区只改变了外层 extent，未改变 `TabItem` Chrome 贴住 `TabPanel` 布局槽的问题；真实截图中的每张分类卡底边仍像被一条水平边界切平。
- 共享 `GscRedesignSettingsTabItem` 现使用不裁切的 `TabItemRoot` + 独立圆角 Chrome，Chrome 顶部对齐并留出 2 DIP 底部安全距离，移除 Chrome 的 `ClipToBounds=True`。
- `GscRedesignSettingsTabControl` 现用 `SettingsHeaderBottomSafetyZone` 真实占位元素增加分类滚动内容 extent；顶部横向模式折叠占位元素。RenderHarness 会检查最后 Tab 与 Chrome 的底部几何及安全距离。
- 已验证：XAML/source 门禁、Playnite `210/210`、设置页五种窗口 `render-qa OK`；仍需真实 Playnite 宿主主题/DPI/连续缩放人工验收。

## 接手时的最短指令

以后可以直接对新的 agent 说：

```text
请先读取 GameSaveCenter 项目的 docs/DEVELOPMENT_HANDOFF.md，按照其中的读取顺序、不可丢失约束和验证要求恢复项目上下文。当前代码侧已收口：除非用户提供新的真实问题、日志或明确新需求，否则不要主动开启新的 UI 重构或性能优化；不要重置或覆盖已有改动，先检查 `git status`，完成后更新项目记忆与工作日志并提交 commit。
```

## 2026-08-13 DEV-INSTALL-007：兼容未发现 Playnite 路径

- 另一台机器的一键安装日志显示，失败发生在构建前：Playnite 使用自定义/便携安装路径时没有发现任何 `Playnite.DesktopApp.exe`，空候选数组绑定到 `TrustedPlayniteExecutables` 后被 PowerShell 拒绝；不是编译失败。
- 当前安装器增加 App Paths、PATH 和规范化路径发现，并允许可信 Playnite 候选为空。没有运行中的 Playnite 时继续构建、打包和安装；安装完成后只提示无法自动启动 Playnite。可使用 `-PlayniteExecutable "D:\\实际路径\\Playnite.DesktopApp.exe"` 支持自定义路径。
- 如果 Playnite 仍在运行但路径不可确认，安装器仍安全停止并要求手动退出或显式指定路径，不按进程名强杀未知进程。
- 根目录入口修订号为 `DEV-INSTALL-007`；真实另一台机器的便携版、自定义路径和完全未启动 Playnite 场景仍需用户手工验证。

## 2026-08-13 UI-QA-REAL-005：首页与设置页几何兼容性修复

- 首页宽屏右侧 `今日概览` 已在 XAML 与响应式代码双重设为顶端对齐；离屏几何探针在 1600/1920 DIP 宽屏下 `OverviewSecondaryTopDelta=0`。
- Hero/当前游戏宽屏比例调整为 `1.1* + 0.9*`，离屏报告当前游戏卡相对 Hero 约 `0.82`，改善 4K/大窗口下按钮与指标的拥挤；没有改变业务命令、Binding 或窄屏堆叠阈值。
- 设置分类共享模板增加 `SettingsHeaderItemsHost` 底部安全区，且分类 Chrome 使用像素对齐和布局取整；末项滚动到底部后仍完整落在 viewport 内，五个分类都保持可见/可滚动。
- RenderHarness 在捕获设置图前将 `SettingsShell.Opacity` 设为 1，避免入口动画未由真实宿主触发时生成空白图；新增三类几何门禁。
- 当前验证基线：Release 隔离构建 0 warnings/0 errors；Core `42/42`、Worker `117/117`、Playnite `210/210`；五种常用窗口的 `render-qa` 全绿。真实 Playnite 宿主 Light/Dark/Follow、高对比度、100%–200% DPI 和连续缩放仍需人工验收。

## 2026-08-13 DEV-INSTALL-006：无窗口 Playnite 残留回收

- 真实复现 PID 48188：Playnite 已无主窗口但进程残留，`CloseMainWindow()` 无法请求退出，旧安装器等待 20 秒后中止。这不是管理员权限问题。
- 当前安装器仍先请求正常退出；超时后仅当进程属于当前会话、路径与本次发现的 Playnite 可执行文件完全一致且 `MainWindowHandle=0` 时，才结束该残留实例。路径不可确认、跨会话或仍有主窗口时继续拒绝强制结束；PID 自然退出竞态不再误报。
- `DEV-INSTALL-006` 已在原失败场景完整通过：Release 0 警告/0 错误，Core 42/42、Worker 117/117、Playnite 203/203，普通用户安装并启动；当前安装 DLL 标识 `0.6.70+4125a5448b1d903c1122d6ba596b8ca31597a714`。Playnite 11:14:33 日志确认插件加载，Worker 11:14:38 日志确认初始化并启动。

## 2026-08-13 DEV-INSTALL-005：构建隔离与真实宿主验证

- 用户日志中的 Contracts 编译成功；失败发生在后续测试覆盖标准 `bin\Release` 时，原因是旧 `dotnet/testhost` 或 Worker 文件锁。当前一键入口 `GameSaveCenter-Run.cmd` 对应 `DEV-INSTALL-005`，每次构建使用唯一 `artifacts\dev-build\<Configuration>\<guid>`，不清理标准输出，也不默认请求 UAC。
- `scripts/dev-install-run.ps1 -Configuration Release -NoStart -SkipClean` 已在正常桌面文件权限下完整通过；当前受限 Codex 沙箱对 Playnite 用户扩展目录的写入失败不代表脚本失败。遇到旧残留时应先正常关闭 Playnite，让插件回收其所属 Worker；不要按进程名强杀，也不要把管理员提权作为默认修复。
- 真实启动证据：`C:\Users\lopmatu\AppData\Roaming\Playnite\playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`；插件自身日志记录 `GameSaveCenter.Playnite 0.6.70.0`；`C:\Users\lopmatu\AppData\Local\GameSaveCenter\Logs\worker-launch.log` 记录 Worker 存储初始化后进入 `Application started`。安装报告为 `artifacts\last-dev-install.txt`。
- 当前自动化基线：Core `37/37`、Worker `81/81`、Playnite `202/202`，Release 构建 0 warnings / 0 errors。真实主题/DPI/键盘、Rclone/多设备、游戏恢复/撤销、EXE/LNK/BAT/PS1 和 900+ 游戏库仍需人工 QA。

## 必须读取的资料

按以下顺序读取：

1. `AGENTS.md`
2. `docs/DEVELOPMENT_HANDOFF.md`（本文件）
3. `docs/PROJECT_MEMORY.md`：长期不可丢失约束、已完成 UI 决策和性能边界
4. `docs/DEVELOPMENT_PROGRESS.md`：按 UI 编号排列的实施历史和下一步线索
5. `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md`：总体设计方向
6. `docs/design/UI_CHANGE_GATE.md`：每次 UI 变更的门禁与验收标准
7. `.codex/skills/wpf-apple-desktop-ui/SKILL.md`：WPF/Playnite UI 专项技能，随仓库提交；本机同时安装于 `%USERPROFILE%\.codex\skills\wpf-apple-desktop-ui`。做任何 WPF/XAML 改动前先完整读取，并按任务需要读取 `references/` 中的对应文档
8. `C:\Users\lopmatuse\.codex\attachments\1b6b382f-30ed-44c7-a9ce-6c580fefbe83\pasted-text.txt`：用户提供的完整任务提示词附件；如果新电脑不存在该路径，以本文件和仓库内文档为准
9. `D:\workplace\Github\GameSaveCenter.WpfUiDemo.v3.1`：WPF Demo 模板，比较布局层级、节奏、控件尺寸和交互表面，不复制 Demo 假数据或业务实现；当前本机实际可用副本为 `D:\workplace\VSCode\GameSaveCenter.WpfUiDemo.v3.1`

如果附件路径发生变化，先在当前对话的附件中找到同一份完整提示词；不能因为附件不可用而跳过仓库内的规则和约束。

## Codex 2026-08-11/12 阶段补充（性能与自定义启动项）

## Codex 2026-08-13 阶段补充（环境准备与 GameTool 安全策略）

ONBOARDING-001 已在既有 Maintenance 诊断页加入首次环境准备入口；GAME-TOOL-003/004 已在既有 TrainerCenter Inspector 加入 CustomExecutable 的已有实例策略和风险分类。当前测试基线为 Worker 74/74、Playnite 200/200；Worker/Playnite Release 构建均为 0 警告、0 错误，WPF 静态校验与离屏 render QA 通过。

GameTool 的安全边界不可丢失：不能按进程名直接关闭程序；只在完整 EXE 路径一致且重新确认 PID 路径后执行 Restart；无法读取路径时保守拒绝。反作弊风险游戏中只允许 `GeneralUtility` 自定义工具自动启动，Unknown/Trainer/CT/GameModification 必须阻止并审计。真实 Playnite 覆盖安装、扩展扫描和 Worker/IPC 加载仍受 PID 3896 文件锁限制，清理 PID 后需要重新执行一键安装并检查 `playnite.log` 与 `extensions.log`。

## Codex 2026-08-13 阶段补充（SMART-PROTECT-001/002）

SMART-PROTECT 已完成第一版智能保护闭环：游戏停止请求等待现有存档识别，识别到候选/接受候选/Ludusavi 匹配后在既有 Dashboard 对话框显示“启用推荐策略 / 以后再说 / 不再提醒”三选一；没有识别到存档只写审计，不打扰用户。提示状态与最近识别结果保存在 SQLite，`Deferred` 冷却 7 天，`Enabled` 和 `Dismissed` 不再重复提示。因为识别最长约 2 分钟，Playnite 的 `session.stopped` 请求使用 3 分钟专用超时。

Overview 既有最近保护卡已扩展为最近窗口游戏状态列表：`已保护`、`未匹配`、`存档未保护`、`风险`。已保护项不可选，其余项可多选，通过 `protection.recommended.apply` 批量启用退出后与游玩中自动保护，并写入审计后刷新快照。不要把这个能力移到新主导航页，也不能把“启用推荐”实现成自动恢复或删除操作。

本阶段验证：Core 29/29、Worker 76/76、Playnite 202/202；Worker/Playnite Release 构建 0 警告、0 错误；`validate-source.py`、`check-xaml.ps1`、WPF UI 契约测试与 `render-qa.ps1` 全部通过。测试使用 `artifacts/smart-test/final-*` 隔离输出；标准测试目录仍被早先宿主锁住时，不要删除或强杀不属于本次任务的进程。

验证边界不变：当前会话无法完成真实最终包的 Playnite 扩展扫描；真实宿主日志仍需用户复核。开发安装器不默认请求管理员权限，而是先正常关闭 Playnite、等待插件回收其 Worker，再按扩展路径处理残留。隔离启动/离屏渲染只能证明应用层启动和布局，不得写成“真实宿主加载成功”。

## Codex 2026-08-13 阶段补充（NOTIFY-001 / MULTI-DEVICE-001 / RCLONE-RELIABILITY-001）

退出任务的 `TaskStatusDto.SessionId` 是通知聚合的唯一业务关联；备份与媒体任务必须使用退出会话的 SessionId，Playnite 只在该会话预期任务全部进入终态后发一条摘要。摘要不能另造成功判断，必须复用任务中心终态；云端失败时保留“本地备份完成”事实并给出重试提示。

多设备摘要的 `ParentBackupId` 用于识别共同基线：同父版本的两个不同子版本标记 `DivergedFromCommonBase`，线性父子关系不标记冲突。`PreferLocal`、`PreferRemote`、`KeepBoth` 只持久化用户决定；不得自动合并、自动选择、删除远端或绕过 PreRestore → Restore → Validate → Rollback。

Rclone 可靠性仍是单向安全适配器，只允许 `copy`、`check`、`lsf`、`cat`、`version`。`RcloneFailureClassifier` 将认证、权限、远端不存在、网络和不完整传输转换为稳定错误码；只有网络/不完整传输进入有限退避，凭据/权限类错误不能无限重试。本地历史不得因远端缺失而删除。

当前自动验证基线：Core 35/35、Worker 81/81、Playnite 202/202；三者 Release 构建均 0 警告/0 错误；source、XAML、WPF 静态门禁通过。真实 Rclone、两台设备、真实恢复和最终 Playnite 宿主加载仍为 `MANUAL QA REQUIRED`。开发安装的 Playnite/Worker 回收链路已改为普通权限正常关闭优先。

新的 AI/Codex 长期记忆入口已建立：先读 `docs/ai/PROJECT_MEMORY.md` 与 `docs/ai/WORKLOG.md`，再读本文件。

本轮已完成并推送：

- PERF-004：`[PERF]` 性能基线日志 + `docs/ai/PERFORMANCE_BASELINE.md`。
- PERF-005：Snapshot 无变化 0 CollectionChanged（`SnapshotComparers` + `BatchObservableCollection.ReplaceAll`）。
- PERF-006：Task/Media 搜索 180ms 防抖（`DebouncedRefresh`）。
- GAME-TOOL-001/002：自定义启动项 EXE/LNK/BAT/CMD/PS1，外部路径引用不复制文件；`GameToolLauncher` 按类型启动；`GameToolSessionTracker` 只按 Session/PID/StartTime 关闭本会话进程。
- PERF-007：媒体缩略图异步化（`AsyncThumbnailLoader` 3 并发 + LRU + Freeze，`AsyncThumbnailImage`）。
- UI-QA-REAL-001：隔离 Playnite 真机冒烟通过，截图在 `artifacts/ui-qa/real/playnite-real.png`；主题/DPI/键盘/缩放与自定义启动项真机流程仍待用户复核。
- UI-206 初始方案（SUPERSEDED）：提交 `962a6b0` 曾把共享与关键 DataGrid Style 改为 `VirtualizingPanel.ScrollUnit=Pixel`，并把 Maintenance/Task 表格由强制 `Height` 改为 `Height=double.NaN + MaxHeight`；该 Pixel 方案已由真实 Playnite A/B 验证为回归并撤回，仅保留历史记录，不作为当前方案。
- DataGrid 最终结论（`d9cd82f`/`0ce3388`/`4564c8f`）：Pixel ScrollUnit 经真实 Playnite A/B 验证会回归，已撤回；当前采用 `Item` + `GscStableDataGridRow` 稳定行样式 + geometry probe（gap ≤4 DIP、末行完整、Recycling 保持）。不要重新改回 Pixel。

本机最近两个本地提交（未推送）：`e86e461 docs: record UI-207 revalidation baseline`、`d45f65c feat: harden restore readiness and recovery drills`。UI-207 的设置布局、运行中游戏自动定位、上次选择恢复、GamePicker 新用户默认“已安装”和当前游戏真实 Icon 已由 `d2662e3` 收口；Restore Readiness 的严格 Manifest 校验与恢复灾难演练已由 `d45f65c` 收口。

当前测试基线：Core 27、Worker 67、Playnite 197；本阶段 Worker Release 构建 0 警告/0 错误，Restore Readiness/恢复演练 67/67；上一阶段 render-qa 全绿，源码验证与技能静态审查通过。下一阶段为 POLICY-001；不要重复打开已完成的 UI-207、RELIABILITY-RESTORE-001、HEALTH-001、PROTECTION-001、PERF-004～010 与 GAME-TOOL-001/002。

## 项目目标

这是已有的 Playnite 插件项目 GameSaveCenter 的持续 UI 重构，不是新建功能，也不是只改某一个 `Margin` 或颜色。

生产 UI 使用现有的 WPF/C# 技术栈，最终视觉和信息层级应接近 `GameSaveCenter.WpfUiDemo.v3.1`。Demo 只是 UI 模板；生产页面必须继续使用真实数据、真实命令和真实状态。

必须长期保持：

- 不改插件 ID、业务服务、Worker、IPC、数据库、持久化和任务协议，除非用户明确要求并单独批准。
- 保留现有 ViewModel、命令、Binding、`x:Name`、真实状态流、错误反馈和 Playnite 生命周期。
- 修复共享资源、样式、ControlTemplate 和页面结构，不用局部补丁掩盖共享控件问题。
- 保留大型列表的有限测量、内部滚动、键盘访问、UI Automation 和虚拟化。
- 不使用 HTML、WebView、Electron、Avalonia、WinUI 或截图替代原生 WPF。
- 不使用 Demo 假数据，不用 `Task.Delay` 模拟业务成功，不把视觉状态伪造成业务结果。

## 用户明确提出的 UI 细节

这些细节不是可选的视觉偏好，而是持续验收标准：

- 同类按钮的宽度/高度、文字方向、文字垂直和水平位置必须一致。
- 文本不能因为共享模板硬编码对齐而偏离调用方意图；需要修复共享模板的 `TemplateBinding`。
- 下拉框初始必须显示明确的默认值。例如初始筛选是“全部”，控件就必须显示“全部”。
- 对于依赖真实上下文的动态下拉框，空值可能代表“等待真实上下文”，不能为了视觉而强行选择第一项。
- 页面在 100%/125%/150%/175%/200% DPI、不同窗口尺寸、Light/Dark/Follow Playnite/高对比度下不能出现重叠、裁切、文字不可读或操作入口消失。

## 每次开发的固定流程

1. 先执行 `git status`、`git branch --show-current` 和最近提交检查；不得 `reset --hard`、`checkout --` 或覆盖别的电脑留下的未提交改动。
2. 读取本文件、`PROJECT_MEMORY.md`、`DEVELOPMENT_PROGRESS.md` 中与目标页面相关的最新条目。
3. 搜索目标控件的全部共享样式、模板、资源和调用点，先判断根因属于信息架构、布局测量、模板状态、可读性、可访问性、性能还是宿主兼容性。
4. 按 UI Change Gate 实施小范围、可验证的 UI 改动；优先共享资源和结构修复，保持业务合同不变。
5. 至少运行适用的静态校验（含 `python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .`）、`git diff --check`、源码验证、WPF 结构测试、Debug/Release 构建和相关单元测试。
6. 真实 Playnite、主题、DPI、键盘或宿主渲染没有实际运行时，必须明确写“尚未验证”，不能声称已经验证。
7. 完成一轮后同步更新：
   - `docs/PROJECT_MEMORY.md`：新增不可丢失的结构/行为约束
   - `docs/DEVELOPMENT_PROGRESS.md`：记录 UI 编号、修改文件、保留的命令/绑定、验证结果和未完成的宿主验证
   - 本文件的“当前交接基线”和“下一步方向”
8. 每次有实际开发改动都必须创建一个清晰的 Git commit。提交前确认工作区没有意外文件。

## 合并后当前交接基线（2026-08-11）

- 分支：`main`
- 当前 UI 交接基线：本轮本地提交 `界面：完善设置布局与当前游戏上下文`（hash 以 `git log -1` 为准；本任务不 push）；生产 UI 最近相关提交 `0ce3388`（DataGrid 稳定行样式 + geometry probe）与 `4564c8f`（诊断摘要非裁剪）。DataGrid 最终采用 `Item + GscStableDataGridRow`，Pixel 已真机验证并撤回。
- UI-207（本轮）：设置页 Header 不裁剪、分类栏宽/窄滚动与选中 BringIntoView；打开 Dashboard 自动定位运行中游戏（否则恢复上次选择/首个已安装），GameStarted 事件驱动、普通刷新不抢回；GamePicker 新用户默认“已安装”；当前游戏显示真实 Playnite Icon（UI-only provider，LRU 48，失败 fallback 手柄 glyph）。当前复验基线为 Core 27 / Worker 59 / Playnite 197、render-qa 全绿；真实设置页/自动定位/Icon 与 1080p/4K/DPI 人工验收标记 BLOCKED_ENVIRONMENT。
- 上一合并提交：`e87e2af`（`merge: reconcile local and cross-machine UI migration`）
- 合并共同基线：`9cdd975`；本机 UI-173～UI-181 与 `origin/main` 的 UI-181～UI-183、交接文档线均已保留，没有删除任一方共同基线后的提交。
- 本机额外 WIP 已先由 `e61d0fc` 固化后纳入合并；本机的长期约束已追加到 `docs/PROJECT_MEMORY.md` 的 `MERGE-001`，远端既有记忆条目保持原文。
- UI-184 已将 Overview 的 Demo 层级落地为“今日工作台动作卡 → Hero/当前游戏双列（受限宽度堆叠）→ 六项指标 → 最近活动”，具体约束见 `docs/PROJECT_MEMORY.md` 与 `docs/DEVELOPMENT_PROGRESS.md`。
- UI-185 已将 SaveCenter 的候选路径页补齐为“当前规则与校验 → 候选表/Inspector”，页签改为“路径与校验”；共享 Dashboard 游戏上下文不重复渲染，真实 `SelectedGame` 状态和扫描/校验/刷新命令保持绑定，具体约束见 `docs/PROJECT_MEMORY.md` 与 `docs/DEVELOPMENT_PROGRESS.md`。
- UI-186 已将 TaskCenter 摘要卡改为 Demo 的“任务总数 / 运行中 / 需要重试 / 已完成”四项真实任务状态计数，宽屏四列、窄屏按两列/单列收缩；任务筛选、全局视角、详情 Inspector 和恢复命令未改变。
- UI-187 已将 Maintenance 诊断页顶部摘要改为 Demo 的六项真实健康卡（Worker、Ludusavi、Rclone、数据与备份目录、媒体目录、设备状态），并将响应式列数收口为宽屏 3 列、中屏 2 列、窄屏 1 列；诊断操作、表格、Inspector、审计、完整摘要和原有命令/绑定均保留。
- UI-188 已将 TaskCenter 任务队列补齐 Demo 的搜索输入框，真实搜索任务 ID、类型、游戏、详情和错误，并与状态/游戏/类型筛选叠加；未新增 Worker/IPC 请求，任务计数、Inspector、恢复命令和虚拟化保持不变。
- UI-189 已将 TaskCenter 顶部四项任务摘要改为 Demo 的“标题 → 30px 数值 → 副文案”三行阅读卡，仍绑定真实任务计数并保持四列/两列/单列响应式逻辑；搜索、筛选、Inspector、命令和虚拟化未改变。
- UI-190 已修复常用窗口尺寸下 Overview 底部内容被截断、Media 表格只剩一行和 Maintenance 诊断下方内容不可达的问题：Overview 只滚动上方工作台内容并让最近活动保持有限 Grid/ListBox 视口；Media 与 Maintenance 使用明确命名的页面滚动面承载下方内容，主表/主列表由 code-behind 保持 236–460 DIP 有限高度和内部虚拟化滚动。真实命令、Binding、Inspector 和业务层未改变。
- UI-191 已修复 SaveCenter/TrainerCenter 窄宽度或低高度堆叠 Inspector 时主列表被挤成一行的问题：历史版本/候选路径 DataGrid、已安装工具/FLiNG 搜索结果/可下载版本 ListBox 区域保持 236 DIP 最小视口；Inspector 高度按实际布局剩余空间计算并继续使用自身滚动。真实命令、Binding、选中项、导入确认、虚拟化和 Recycling 未改变。
- UI-192 已修复 TaskCenter 窄宽度或低高度堆叠详情 Inspector 时任务主表被挤压的问题：任务队列保留 236 DIP 最小视口，详情高度按摘要区、筛选区和主表剩余空间计算，搜索/筛选、真实计数、取消/重试/复制命令和虚拟化未改变。
- UI-193 已修复 Maintenance 设备状态、异常审计和进程映射页窄宽度/低高度堆叠 Inspector 时主表被挤压的问题：诊断、设备、审计发现和进程映射表统一保留 236 DIP 最小视口；设备/审计 Inspector 按实际布局剩余空间限高并继续内部滚动，进程映射表接入显式结构标识和共享表头主题加载，命令、Binding、选中项、审计日志和虚拟化未改变。
- UI-194 已将共享页面级滚动契约与 Demo 对齐：`GscPageScrollViewer` 默认垂直 `Auto`，设置页、存档策略、维护保留策略、侧栏导航和 Overview 辅助页面不再用 `Hidden` 掩盖溢出；Overview 宽屏/堆叠的有限内部滚动仍由 code-behind 分工控制，表格/列表虚拟化未改变。
- UI-195 已补齐 Maintenance“设备状态 / 异常与审计 / 进程映射”三个 Tab 的命名页面滚动面；对应三张主表由 code-behind 使用 236–460 DIP 有限 Height，继续保留 DataGrid 内部滚动、虚拟化、Inspector 滚动、真实命令和 Binding，源码门禁同步识别该结构。
- UI-196 已补齐 TaskCenter 的命名页面滚动面 `TaskPageScrollSurface`；任务表由 code-behind 使用 236–460 DIP 有限 Height，堆叠 Inspector 按页面实际视口计算剩余高度，摘要、筛选、任务表、详情和底部恢复操作在短高度常用窗口下均可通过明确滚动访问，真实搜索/筛选/计数/Binding/命令/虚拟化未改变。
- UI-197 已修复截图暴露的常用窗口可见性问题：TaskCenter 动态游戏/类型筛选的 WPF 集合刷新空选中在 DataBind 优先级恢复为 `全部`，MediaGrid 通过共享 ListBox 顶部内容契约避免少量媒体卡沉到有限视口底部，Overview 工作台按钮在窄主列第二行横向自动换行，宽屏右侧摘要/风险列保留有限 Auto 滚动以确保“打开维护中心”可达；真实命令、Binding、Worker、IPC、数据库、持久化、虚拟化、键盘访问和 Automation 未改变。
- UI-198 已修复 Overview 在常用窗口和窄窗口下的主工作区被 sibling 行挤压问题：工作台、Hero/当前游戏、六项指标和最近活动统一进入 `OverviewPrimaryScrollSurface`，窄布局再由 `OverviewStackScrollSurface` 统一承载主列与右侧摘要，避免 980 DIP 下主列高度变成 0；宽布局仍保留主列/摘要列独立有限滚动。`OverviewActivityList` 继续使用有限高度、ListBox Recycling 和自身滚动，真实命令、Binding、SelectedTask、键盘访问和 Automation 未改变。已按 1600/1366/1280/1100/980 DIP 与 900/768/720/700/640 DIP 运行隔离生产离屏渲染，源码验证通过，生产插件 Release 构建 0 警告/0 错误，隔离测试 149/149 通过；未运行真实 Playnite 宿主、主题切换、DPI 真机和连续缩放流畅性验证。
- UI-199（代码提交 `5cbd512`）已修复工作区由程序化导航、恢复状态或离屏渲染直接切换时顶栏仍显示“首页”的语义不同步：`DashboardView.UpdateWorkspacePresentation()` 与侧栏点击共同调用 `UpdateWorkspaceHeader`，媒体/维护/任务等页面标题和副标题始终跟随当前可见工作区。MediaCenter 的摘要卡响应式断点改为逻辑 DIP 的 `>=760` 四列、`>=520` 两列、其余单列，使 Dashboard 在常用 1080p/2K/4K 窗口下保持 Demo 四卡横排并为主表保留可见行；表格有限视口、内部滚动、虚拟化、Inspector、真实命令和 Binding 未改变。源码验证通过；生产插件 Release 构建 0 警告/0 错误；隔离 WPF 测试 150/150 通过；生产离屏渲染覆盖 1600/1366/1280/1100/980 DIP 与 900/768/720/700/640 DIP 并返回 `render-prod OK`。Render harness 自身仍有 3 个 FakeApi 未使用事件警告；真实 Playnite 宿主、主题切换、DPI 真机和连续缩放流畅性尚未验证。
- UI-200（代码提交 `f11e9b7`）已将 Demo 的 `MinWidth=1040`、`MinHeight=700` DIP 固化为生产外壳的常用最小窗口：Dashboard `>=1040` 保留带文字侧栏和单行顶栏，低于该值才进入图标紧凑壳；同时按外壳扣除侧栏后的约 700 DIP 页面宽度校准 Media `>=700` 四列、Task `>=900` 四列/`>=680` 两列、Maintenance `>=980` 三列/`>=680` 两列。1040×700 离屏结果为 Media 四卡并显示两行表格、Task 2×2 摘要并显示队列、Maintenance 两列健康卡；1366×768 仍为完整多列，页面级滚动、表格/列表有限视口、内部滚动、虚拟化、真实命令/Binding 和业务层未改。源码验证通过；生产插件 Release 构建 0 警告/0 错误；隔离 WPF 测试 151/151 通过；生产离屏渲染覆盖 1600/1366/1280/1100/1040/980 DIP 与 900/768/720/700/640 DIP 并返回 `render-prod OK`。Render harness 自身仍有 3 个 FakeApi 未使用事件警告；真实 Playnite 宿主、主题切换、DPI 真机和连续缩放流畅性尚未验证。
- SKILL-001（本轮）：`wpf-apple-desktop-ui` 技能已随仓库提交到 `.codex/skills/wpf-apple-desktop-ui/`，并安装到本机 `%USERPROFILE%\.codex\skills\wpf-apple-desktop-ui`；AGENTS.md、DEVELOPMENT_HANDOFF.md、UI_CHANGE_GATE.md、PROJECT_MEMORY.md 与 DEVELOPMENT_PROGRESS.md 已同步仓库内技能路径，UI 门禁新增 `python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .` 静态审查。
- QA-001（本轮）：新增可复用的离屏渲染 QA：`tests/GameSaveCenter.RenderHarness`（假数据，不启动 Worker/IPC）与 `scripts/render-qa.ps1`，覆盖 1040×700、1280×720、1366×768、1600×900、1920×1080 逻辑窗口，输出 PNG 与 `artifacts/ui-qa/render/render-qa-report.txt`（页面滚动面、DataGrid/ListBox 有限视口尺寸、可滚动性）。1040×700 复核结果：Media 待归类/当前游戏媒体主表 350 DIP 高、6 行；Task 队列 350 DIP 高、8 行；Maintenance 各主表 350 DIP 高、8 行；所有页面滚动面为 `Auto` 且内容超限时 `scrollable=True`；Overview 堆叠模式由页面滚动承载，风险区内容完整（496 DIP）。本机系统 SDK 9.0.302 在多节点构建时会因 SDK locator 目录缺失在 `GetTargetFrameworks` 静默失败，`render-qa.ps1` 已固化 `-m:1 -nodeReuse:false -p:NuGetAudit=false`。本机 C 盘空间耗尽时，需先把 `TEMP/TMP` 指到仓库 `.tmp/qa-temp` 再运行脚本。真实 Playnite 宿主、主题、DPI 和连续缩放流畅性仍未验证。
- UI-201（本轮）：TaskCenter 堆叠详情 Inspector 的最小高度从 96 提高到 160 DIP，解决 1040×700/1280×720/1366×768 常用窗口下详情条带过矮、基本无法阅读的问题；`TaskGrid` 的 236–460 DIP 有限视口、内部滚动、虚拟化、真实搜索/筛选/计数/命令/Binding 均未改变。离屏 QA 复核：1040×700/1280×720/1366×768 下 `TaskDetailScrollViewer` 均为 160 DIP 且内部 `Auto` 滚动，Task 队列仍为 350/360/384 DIP 高、8 行；1600×900/1920×1080 仍保持 Demo 式右栏 Inspector（360 宽）。源码验证通过；真实 Playnite 宿主、主题、DPI 和连续缩放流畅性仍未验证。
- UI-202（本轮）：Overview 首页按 Demo `HomeView` 的单列阅读流收口：Dashboard 在内容区 `<1200` DIP（1280×720、1366×768 等常用窗口化逻辑宽度）时把 Overview 切为堆叠单列页面流，右侧“今日概览/风险与提醒”下移到主内容之后，避免主工作区只剩 550–600 DIP 宽导致 Hero 与“当前游戏”被挤成上下堆叠；Hero/当前游戏同行的堆叠阈值同步从 760 降到 700，使 1040×700 最小窗口也保持 Demo 的“Hero + 当前游戏”并排。离屏 QA 复核：1040×700 Hero 444px、当前游戏 266px 同行；1280×720 Hero 576px、当前游戏 346px 同行；1366×768 Hero 630px、当前游戏 378px 同行；所有尺寸均无重叠，最近活动由 `OverviewStackScrollSurface` Auto 滚动可达；1600×900/1920×1080 继续使用宽屏双列。`OverviewActivityList` 有限视口、ListBox Recycling、真实命令/Binding 和页面滚动分工未改变；`render-qa.ps1` 的 Overview 堆叠阈值同步为 1200。源码验证通过；真实 Playnite 宿主、主题、DPI 和连续缩放流畅性仍未验证。
- UI-203（本轮）：存档中心与修改器中心的堆叠 Inspector 最小高度统一从 96 提高到 160 DIP（`SaveHistoryActionsScrollViewer`、`SaveCandidateInspectorScrollViewer`、`TrainerToolsSettingsScrollViewer`、`TrainerReleaseInfoScrollViewer`），与 UI-201 的 Task 规则一致；`SaveHistoryGrid`/`SaveCandidateGrid`、修改器主表与可下载版本面板仍保持 236 DIP 最小视口、内部滚动和虚拟化。离屏 QA 已扩展覆盖 SaveCenterView/TrainerCenterView（含假数据）：1040×700/1280×720/1366×768 下四个堆叠 Inspector 均为 160 DIP 且内部 Auto 滚动，主表 236 DIP 高、8 行；1600×900/1920×1080 保持右栏 360 宽。源码验证通过；真实 Playnite 宿主、主题、DPI 和连续缩放流畅性仍未验证。
- QA-002（本轮）：离屏渲染 QA 覆盖补齐全部工作区与设置页：Overview、Save、Trainer、Media、Maintenance、Task、Settings 均在 1040×700/1280×720/1366×768/1600×900/1920×1080 渲染并输出 PNG/报告。设置页依赖 Playnite 宿主 `BaseTextBlockStyle`，harness 在 Application.Resources 预置中性 fallback 后可在无宿主环境解析；1040×700 下 Settings 四个 Tab 的 `SettingsScroller` 均为 Auto 且内容超限时 `scrollable=True`。
- QA-003（本轮）：`render-qa.ps1` 增加自动失败门禁：任何命名工作区主表/主列表（排除 `MaintenanceAuditLogGrid` 审计条带与 `OverviewActivityList` 最近活动）在任一常用窗口下的有限视口 `<236` DIP，或命名页面滚动面（`*ScrollSurface`/`SettingsScroller`）内容超限却使用 Hidden 滚动条，render-qa 将以退出码 1 失败并在报告中列出 `PROBLEM`。当前 7 页面 × 5 尺寸全绿。
- QA-004（本轮）：在 C 盘恢复可用空间并重定向 `TEMP/TMP` 到 `.tmp/qa-temp`、测试输出隔离到 `artifacts/ui-qa/*-tests` 后，完整隔离测试重跑通过：Core 13/13、Worker 23/23、Playnite 151/151。Playnite 源码结构断言同步更新为 UI-201/202/203 的新阈值（`Math.Max(160, ...)`、`workspaceContentWidth < 1200`、`primaryWidth < 700`）。
- QA-005（本轮）：`scripts/dev-install-run.ps1 -Configuration Release -NoStart` 一键构建安装成功：解决方案 Release 构建 0 警告/0 错误，Core 13/13、Worker 23/23、Playnite 151/151 全部通过，已打包并安装到 `C:\Users\lopmatu\AppData\Roaming\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`（extension.yaml 0.6.70、DLL 0.6.70.0），未自动启动 Playnite。真实 Playnite 宿主内的 Light/Dark/Follow/高对比度、DPI、键盘与连续缩放仍需用户手工验收。
- UI-207-FOLLOWUP（2026-08-12）：Settings 的页面滚动所有权已收口到共享 TabControl 模板：宽屏 `SettingsHeaderScroller` 为 232 DIP 左侧导航，紧凑布局为顶部横向 Auto，`SettingsScroller` 仅承载当前内容；5 个分类在 760/880/920/1100/1400 × 560/700/900 探针中可见。当前游戏 Icon 统一由共享样式复用到 Dashboard/Overview/Save/Trainer/Media，GamePicker 列表不加载 Icon；筛选隐藏当前选择时保留选择并提供恢复操作。隔离 Release 构建 0 警告/0 错误，Core 13/13、Worker 51/51、Playnite 197/197，render-qa 全绿；真实 Playnite 宿主主题/DPI/键盘/连续缩放仍未运行。
- UI-204（本轮）：修复真实 Playnite 中任务中心筛选下拉框在异步集合重建后显示为空的问题。新增共享 `UiFilterSelection.RestoreDefault`（`src/GameSaveCenter.Playnite/Infrastructure/UiFilterSelection.cs`），TaskCenter 的 状态/游戏/类型 三个筛选下拉框在 `Loaded`、`DataContextChanged`、游戏/类型选项集合 `CollectionChanged` 时恢复逻辑默认值（全部）；Dashboard GamePicker 的 状态/平台/排序 三个下拉框在打开选择器与平台选项重建时同样恢复，且只在 `SelectedItem == null` 时恢复，不覆盖用户真实选择。离屏 QA 记录命名 ComboBox 的 `selected/index/items`，空选择会触发 `PROBLEM`；render-qa 全绿，Playnite 151/151 通过。
- UI-205（本轮）：针对真实 Playnite 中动态下拉框 Items 物化晚于 DataBind 导致游戏/类型筛选仍为空的问题，TaskCenter 增加 200ms `DispatcherTimer` 短周期重试（最多 25 次），在集合重建、加载、DataContext 变化后持续恢复默认选中直到三个下拉框都有值；GamePicker 平台选项重建与面板打开时增加 `DispatcherPriority.Loaded` 二次恢复。render-qa 与 Playnite 152/152 通过。
- PERF-001（本轮）：性能优化第一刀——大列表集合批量通知。新增共享 `BatchObservableCollection<T>`（`src/GameSaveCenter.Playnite/Infrastructure/BatchObservableCollection.cs`），`ReplaceAll` 只在内容真正变化时发一次 `Reset`，避免 `Clear()+Add()` 对 Games/Tasks/Media/Findings/Backups/GameTools/TrainerCatalogResults 等大集合逐条触发 WPF 布局；`DashboardViewModel.Replace` 自动路由到 `ReplaceAll`。render-qa 新增每页 `render_ms` 计时基线；render-qa 全绿，Playnite 152/152 通过。
- PERF-002（本轮）：任务中心每次完整快照都会重建 游戏/类型 筛选选项（O(n log n)）。新增 `ComputeTaskFilterFingerprint`，当 `Tasks` 的任务 ID 顺序与数量指纹未变化时直接跳过 `RebuildTaskFilters`，用户筛选仍由属性 setter 直接 `TasksView.Refresh()`；render-qa 全绿，Playnite 152/152 通过。
- PERF-003（本轮）：GamePicker 每次快照都会 `Distinct+OrderBy` 重建平台筛选选项。新增 `ComputePlatformFingerprint`（平台名顺序 + 数量），`Items` 未变化时跳过 `RebuildPlatformOptions`；render-qa 全绿，Playnite 152/152 通过。
- PERF-004（本轮）：GamePicker 每次快照都为整库新建 `GamePickerItem`，大库下分配多且选中引用会漂移。改为按 `PlayniteId` 缓存复用 `GamePickerItem`，快照时只 `UpdateGame` 更新内部 `GameStatusDto` 引用（缓存超过 `max(1024, 2*游戏数+100)` 才清空重建）；减少大库分配并让选中对象身份跨快照稳定。render-qa 全绿，Playnite 152/152 通过。
- 新增的响应式门禁要求：1080p、2K、4K 不能只按物理分辨率判断，必须按 DPI 换算后的逻辑 DIP 尺寸检查全屏、窗口化和最大化；常用窗口下首屏下方真实内容不得被页脚或工作区边界遮住，主表/主列表应保留约四行可读视口，页面滚动与列表内部虚拟化滚动必须分工明确。具体门禁见 `docs/design/UI_CHANGE_GATE.md`。
- 本轮工作区：`0c6f143` 提交后干净；后续 agent 仍须先运行 `git status`、`git log -5 --oneline --decorate` 和 `git branch --show-current`。
- 验证：`python scripts/validate-source.py` 通过；`scripts/render-qa.ps1` 覆盖 Overview/Save/Trainer/Media/Maintenance/Task/Settings × 1040×700/1280×720/1366×768/1600×900/1920×1080 全部通过（含自动失败门禁）；技能静态审查 0 error；`scripts/dev-install-run.ps1 -NoStart` 一键构建安装成功（解决方案 Release 0 警告/0 错误，Core 13/13、Worker 23/23、Playnite 151/151），最新扩展已安装到本机 Playnite Extensions，未自动启动。真实 Playnite 宿主、主题、DPI、窗口化截图和连续缩放运行时渲染仍需用户在 Playnite 内手工验收。

以下原有的远端交接基线保留为历史记录，便于追溯另一台机器的 UI-183 上下文：

## 原远端交接基线（2026-08-10）

- 分支：`main`
- 当前 UI 基线：`2db4336`（`重构：维护中心诊断操作区对齐 Demo`，UI-183）
- 版本：`0.6.70-development-preview`
- 当前工作区：本轮交接文档提交完成后应干净
- 相对 `origin/main`：请运行 `git rev-list --count origin/main..HEAD` 实时确认；UI-183 实现提交为 `2db4336`，后续交接文档提交不改变生产 UI
- 最近已完成的重点：
  - UI-177：修改器中心无选中工具时释放空 Inspector 和固定右栏
  - UI-178：媒体中心无选中媒体时释放空 Inspector 和堆叠行
  - UI-179：设置页窄屏标题提示换行，以及 ZIP/Zstandard/跟随 Playnite 默认值显示
  - UI-180：首页阅读顺序调整为“工作台/今日状态 → 当前游戏 → 六项指标 → 最近活动”
  - UI-181：维护中心五张 DataGrid 的首列显式使用 `MaintenanceFirstColumnHeader`，末列继续使用显式维护表头主题
  - UI-182：维护中心进程映射编辑器宽屏使用 EXE `*`、目标游戏 240 DIP、绑定按钮的 Demo 对齐 Grid，窄于 720 DIP 时目标和按钮换到第二行
  - UI-183：维护中心诊断页顶部改为 Demo 式“诊断操作”阅读卡，刷新诊断提升为主操作，其余五个只读入口保留在第二行操作带
- 当前已完成的自动化基线：Core 13、Worker 23、Playnite 142 测试通过；Release 构建 0 警告/0 错误；XAML 结构检查 13/13；WPF 源码验证通过。静态测试不等同于真实 Playnite 宿主、主题和 DPI 渲染验证。

## 下一步开发方向

继续以 Demo 对齐为目标，对生产页面做页面级收口和真实宿主验收，优先顺序如下：

1. 以 `scripts/render-qa.ps1`（7 页面 × 5 种常用逻辑窗口）为离屏回归基线；继续检查 Overview、SaveCenter、TrainerCenter、TaskCenter、Maintenance、MediaCenter、Settings 的层级、按钮尺寸、文字对齐、默认选择和空状态。
2. 检查共享 `Button`、`ComboBox`、`TextBox`、`ListBox`、`DataGrid`、Tab 和 Inspector 资源，发现同类问题时修共享模板。
3. 每次页面级改动后运行 `scripts/render-qa.ps1`（C 盘满时先设 `TEMP/TMP` 到仓库 `.tmp/qa-temp`）；按 980/1040/1100/1280/1366/1600 DIP 宽度、640/720/900/1080 DIP 高，以及 1080p/2K/4K 在 100%/125%/150%/175%/200% DPI 下的常用窗口化逻辑尺寸，复核窄屏堆叠、首屏内容可见性、有限滚动和长文本；不把 4K 通过当作 1080p 通过。
4. 在可用环境中运行 Playnite 宿主，验证 Light/Dark/Follow Playnite/高对比度、键盘焦点、真实数据加载和窗口关闭生命周期；若环境不可用，保留明确的手工验收清单。
5. 发现问题后继续使用新的 UI 编号记录，不要删除历史记录或把未验证事项标成完成。
6. 在可用的干净环境中重跑完整 Release 构建与隔离测试；本机曾因测试输出 DLL 残留句柄占用和 C 盘 0 可用空间而无法在本次重跑。

## 跨电脑、跨模型规则

代码和文档是交接的真实来源，模型记忆不是。切换电脑前应先把当前提交推送到远端：

每一轮开发完成后，由 Agent 自己 commit 并 push 到当前远端分支（默认 `main`），不要等用户提醒；这是项目长期协定，不需要用户每次重复。

每轮开发完成后，还要及时清理不再使用的本地中间产物：`artifacts/` 下的旧 `dev-build`、`ui-audit-build`、`phase*`、`audit*` 目录与旧 zip，以及 `.tmp/` 下的一次性审计目录。只保留当前安装包/打包目录与当前审计证据；删除前确认路径在仓库 `artifacts/` 或 `.tmp/` 内，并用 PowerShell `Remove-Item -LiteralPath` 执行。

```powershell
git push origin main
```

切换后应先拉取同一分支，再执行：

```powershell
git status
git log -5 --oneline --decorate
git branch --show-current
```

如果另一台电脑有未 push 的提交或未提交改动，先比较 `git status`、分支和提交历史，再合并；禁止直接覆盖。DeepSeek、Claude 或其他模型可以根据本文件、源码和测试继续工作，但不会自动继承原聊天中的隐含上下文、工具状态或审批状态，因此必须先读取本文件并按流程重新建立上下文。

## 2026-08-16 UI-206 Overview 页面级迁移交接

- 生产首页已按 `GameSaveCenter.UiLab/Pages/OverviewPage.xaml` 的阅读骨架重排：Hero/当前游戏与六项指标占满顶部，最近活动之后才分左主区与右侧风险/关注栏。
- 宽屏右栏固定 330 DIP，与 `OverviewRecentActivityCard` 同行并且离屏探针偏移 0 DIP；窄窗口由 `OverviewStackScrollSurface` 页面滚动承载，右栏下移。生产滚动条模板保持现有版本，未迁移 demo 滚动条或右上角颜色演示按钮。
- 真实功能保留：Snapshot、RecentProtection、AttentionFindings、OpenProtection/OpenAttention 命令、选择与虚拟化列表均仍绑定生产 ViewModel；共享活动表头已做 8 DIP 圆角、低对比度填充和安全边距。
- 本阶段验证：`validate-source.py`、`check-xaml.ps1`、WPF 静态审查通过；Playnite 测试 303/303；生产 Release 0 warning / 0 error；Overview 多尺寸、Light/Dark 与 resize transition 离屏审计通过。全量 render harness 仍仅剩 Save/Media 窄尺寸 `<236 DIP` 历史表格门禁，不要误称全量通过。
- 后续人工 QA：真实 Playnite 2K/DPI、Follow/高对比度、键盘焦点、长中文文案和连续缩放；优先检查 Overview 宽屏侧栏与窄屏下移，再继续其他页面的页面级迁移。

## 2026-08-16 UI-208 Overview 全局活动交接

- 首页“全局活动”已从生产旧的表头 + 图标列改为 UiLab 业务列表四列：类型、对象/事件、结果、时间；真实 `Activities` 绑定与虚拟化保持不变。
- `OverviewActivityTimelineList` 继续使用页面级滚动契约，窄窗通过 `ActivityKindColumn`/`ActivityTimeColumn` 的响应式收紧解决空间问题，不得引入 UiLab 的滚动条模板。
- 本阶段验证：`validate-source.py`、`check-xaml.ps1`、WPF 静态审查、Playnite 303/303、Release 0 warning/0 error、RenderHarness v6/v6.2 宽/窄首页截图均通过。
- 下一阶段继续对照 UiLab 的 Saves/Trainer/Media/Tasks/Maintenance/Settings 页面，优先收口共享按钮、Tab、DataGrid 表头和 Inspector 卡片；全量 render harness 仍需单独处理 Save/Media 窄表历史门禁。
- 真实 Playnite 2K/DPI、Follow/高对比度、键盘焦点与连续缩放仍为 `MANUAL QA REQUIRED`，不能用离屏截图替代。

## 2026-08-16 UI-209 共享表头交接

- `WpfUiProduction.xaml` 的 `DataGridColumnHeadersPresenter` 已改为透明；列头圆角由每个 `DataGridColumnHeader` 自己呈现，避免整行连续底色掩盖圆角。
- 生产滚动条、Item scrolling、行/列虚拟化、绑定和 Playnite 兼容性未改变；后续页面迁移不要直接复制 UiLab 的 `LabScrollViewer`。
- 验证已完成：`validate-source.py`、`check-xaml.ps1`、Release 构建、Playnite 303/303、RenderHarness v6/v6.2。
- 下一阶段继续对照 UiLab 的 Media/Save/Trainer/Task/Maintenance/Settings 页面，优先处理页面层级和窄窗列宽；真实宿主仍需人工 QA。

## 2026-08-16 UI-210 Media 交接

- Media 统计带已改成 UiLab 的单卡四段结构，默认当前媒体标签；真实媒体、待归类、来源规则绑定和命令保持不变。
- Media 保留生产 `VirtualizingStackPanel` 行列表与滚动条，不直接复制 UiLab 的 `WrapPanel` 缩略图网格；后续若要进一步做缩略图网格，必须先提供大媒体库虚拟化方案和滚动回归证据。
- 已修复 Media 700-720 DIP 视口过短以及窄→宽 Inspector 不恢复的问题。重建 RenderHarness 后 Media Light/Dark、多尺寸和 resize transition 通过。
- 全量 render-qa 的剩余门禁仅为 Save 候选表在部分窄尺寸低于 236 DIP；下一阶段优先处理 Save，而不是回退 Media 的布局修复。

## 2026-08-16 UI-211 Save 交接

- Save 表格视口高度公式已从 `height - 520` 调整为 `height - 464`，常规 700-720 DIP 窗口保持 236 DIP；短窗下限为 180 DIP。
- Save DataGrid、Inspector 抽屉、现有滚动条、虚拟化、命令与绑定均保持生产实现。
- 全量 RenderHarness 已通过：`render-qa OK`，包含 Light/Dark、7 页面、1040/1100/1366/2560 和 resize transition；真实 Playnite 宿主仍需人工 DPI/主题/键盘验收。

## 2026-08-16 UI-212 Media 网格交接

- Media 当前游戏主体已从生产横向信息行迁移为 UiLab 风格的固定 164×142 DIP 缩略图卡片；`VirtualizingWrapPanel` 负责可见项生成与 `IScrollInfo`，生产 `ListBox` 的 ScrollViewer/ScrollBar 继续负责实际滚动。
- 卡片只使用真实 MediaItemDto 字段和 `AsyncThumbnailImage`，Inspector、批量操作、Extended selection、窄屏详情抽屉及现有命令/绑定不变；不要直接复制 UiLab 的普通 `WrapPanel` 或演示滚动条。
- 自动验证完成：`validate-source.py`、`check-xaml.ps1`、WPF 静态审查、Release 构建、Playnite 303/303、全量 RenderHarness `render-qa OK`；证据目录为 `artifacts/ui-qa/media-grid-migration-v3`。
- 真实宿主中的实际缩略图文件、超大媒体库滚动、最终 DPI、Follow/高对比度、键盘焦点和连续缩放仍是 `MANUAL QA REQUIRED`。

## 用户原话（必须保留）

> 在继续之前你最好是能够搞一个文件，能够指引去哪里读取获得开发方向等等。这样我直接说你读取xx文件就可以了，他就知道后续怎么开发了。连我这段话你也要放进去，省得我每次都说了（这样每次开发他们都会维护这个项目）。

这句话代表长期维护要求：后续每次开发都必须继续维护本项目，并同步维护本交接文件、项目记忆、开发进度和 Git commit。

## 2026-08-20 UI-254 任务与设置页交接

- 设置页已经完成 Demo-first 页面壳迁移：`GameSaveCenterSettingsView.xaml` 使用左侧五项 `LabSegmented` 分类栏和右侧 `SettingsScroller` 单一内容滚动区；真实字段、校验、保存语义、导入/导出入口均保留。
- 设置页分类面板名称固定为 `SettingsGeneralPanel`、`SettingsBackupPanel`、`SettingsAppearancePanel`、`SettingsAutomationPanel`、`SettingsMigrationPanel`；代码通过 `OnSettingsTabSelectionChanged` 做可见性切换，不能恢复旧 TabControl 的横向 TabStrip 假设。
- 任务中心当前已符合 Demo 的“指标 → 筛选 → 更多筛选 → 队列/进度 → Inspector”阅读顺序；后续修改必须继续保留真实 `TasksView`、`SelectedTask`、`RetryTaskCommand`、`CancelTaskCommand`、DataGrid 虚拟化和项目滚动条。
- RenderHarness 设置审计已改为识别 `SettingsSectionTabs` ListBox；当前证据为 `artifacts/ui-qa/task-settings-final/render-qa-report.txt`，内容为 `render-qa OK`，涵盖 Light/Dark、1040/1100/1366/2560、多分类和 resize transition。
- 本阶段验证：XAML 18 文件通过；源码门禁通过；Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 253 通过/61 跳过/0 失败；WPF 静态审查 0 error、20 warnings、161 info。
- 真实 Playnite 宿主逐页截图仍是未完成的人工验收边界；不能把 RenderHarness PNG、测试或安装清单写成 Playnite 1:1 视觉通过。总 Demo-first 迁移目标尚未完成，下一阶段继续按目标文件对照未收口页面和宿主验证。

## 2026-08-20 UI-255 工作区表格共享契约交接

- `src/GameSaveCenter.Playnite/Themes/Redesign.xaml` 新增 `GscRedesignWorkspaceDataGrid`。Save、Media、Maintenance、Task 的页面 DataGrid 样式必须基于它；页面可以覆盖状态行、媒体表头和背景，但不要复制回虚拟化/滚动/排序/列宽 setter。
- 共享契约保留真实页面滚动条和有限 Grid 视口：`CanContentScroll=True`、`VirtualizingPanel.ScrollUnit=Item`、`VirtualizationMode=Recycling`、行/列虚拟化、列宽可调整、排序可用、FullRow 单选。当前离屏表格截图确认无表头、选中行、Inspector 或滚动条遮挡。
- 所有业务折叠区仍使用 `GscDisclosureCard`；Mono 文本应使用 `GscCodeFontFamily`，其首选链是 `Cascadia Mono, Consolas, Microsoft YaHei UI`。
- 新证据目录为 `artifacts/ui-qa/shared-grid-contract-final`，报告为 `render-qa OK`，覆盖七页、多尺寸、双主题和 resize transition。源码/XAML/Release/Playnite 门禁已通过。
- 下一阶段继续按目标文件检查按钮、辅助界面和真实操作路径，并优先在同一可识别 Playnite 宿主取得逐页截图；不要把上述离屏证据写成 Playnite 1:1 视觉通过。

## 2026-08-20 UI-256 反馈层与按钮契约交接

- `Themes/Redesign.xaml` 已新增共享 `GscRedesignFeedbackToastCard`、`GscRedesignFeedbackDialogCard`、Dialog 遮罩和反馈文字资源；Dashboard 的 Toast/Dialog 必须继续引用这些资源，不要把圆角、背景、阴影、标题字号重新写回 `DashboardView.xaml.cs` 或单页 XAML。
- Dashboard 的原生 `Button` 已回到 `DesignTokens.xaml` 的 `GscButtonBase`/`GscPrimaryButton`；页面内原有 `ui:Button` 语义样式仍是 WPF-UI 的共享入口。修改按钮时优先修共享资源，保留最小 38 DIP 高度、焦点视觉、禁用态和可读的文本截断。
- Toast 的真实通知来源仍是 `GameSaveCenterPlugin.UiNotificationRequested`，确认/选择仍由 `UiConfirmationRequested`/`UiChoiceRequested` 和 `TaskCompletionSource` 完成；不要改成只显示静态 Demo 提示或删除错误详情、Escape、计时器清理、重复 Dialog 取消语义。
- 设置页导出成功、导入报告和错误继续走原生 `MessageBox`，不要在 Playnite 共享 Window 中重新注册 WPF-UI `ContentDialogHost`；如果未来要改成页面内反馈，必须先补充 Window 宿主、焦点、模态、取消和真实操作验证。
- UI-256 已完成 XAML/source/Release/三组测试/离屏 render-qa；真实 Playnite 宿主 Light/Dark/Follow、高对比度、DPI、键盘焦点、Toast/Dialog 实际触发仍是人工验收边界，总 Demo-first 目标未完成。

## 2026-08-20 UI-257 首页当前游戏卡片响应式交接

- 首页根滚动面仍然是 `OverviewStackScrollSurface`，横向滚动必须保持禁用；`OverviewLayoutGrid` 已绑定 `{Binding ViewportWidth, ElementName=OverviewStackScrollSurface}`，并以 `HorizontalAlignment=Left` 避免无限横向测量造成卡片和按钮右侧裁切。
- 生产真实绑定、当前游戏选择器、`BackupSelectedCommand`、`LoadDetailsCommand`、`OpenAttentionCenterCommand`、首页全部备份/同步媒体/刷新入口未改变。RenderHarness 报告 `artifacts/ui-qa/overview-responsive-ui257/render-qa-report.txt` 已覆盖双主题、多逻辑尺寸和 resize transition；1366/1600 代表截图中的当前游戏卡片与三个按钮均完整可见。
- 最终验证为 XAML 18/18、Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 256 通过/62 跳过、source validation 通过、WPF 0 error。新增源码回归 `OverviewFlowUsesTheFiniteViewportWhenHorizontalScrollingIsDisabled` 当前属于既有 `LegacyProductionUiBaselineFact` 门禁组，因此在默认测试运行中按基线跳过，但随 Playnite 测试程序集完成编译并锁定源码契约。
- 真实宿主验证边界：`real-host-audit.ps1` 三次安装并加载生产 0.6.70.0，最新受控证据为 `artifacts/ui-host-audit-ui257-final`；日志确认生产插件真实读取 3 games/50 tasks/100 findings/30 media。由于 Playnite 主窗口返回 `EmptyWindowAutomationPeer`，未能抓取嵌入式导航后的逐页像素截图。受控 `DashboardView` 截图只用于确认本次裁切修复，不得宣称七页已完成 Playnite 1:1 验收。

## 2026-08-20 UI-258 真实 Playnite 七页逐页复核交接

- 已在实际 Playnite 生产窗口 `GameSaveCenter 生产版` 中人工打开七页：Overview、Save、Media、Task、Trainer、Maintenance，以及从游戏右键 `GameSaveCenter → 打开设置` 打开的 Settings。
- 实际看到的关键数据/入口：当前游戏 `Bongo Cat`；Media 30 项、5.76 MiB、待归类 4468 项；Task 50 条、运行中 0、需关注 16、今日完成 34；Trainer 有 Wo Long 与 Yakuza 3；Maintenance 的诊断与进程映射均可进入；Settings 的“常规与目录”及 Worker/Ludusavi/存档目录字段可见。
- Media Inbox 已选中真实待处理截图，滚动右侧独立详情后确认预览、归类选择器、“确认归类”和“忽略并保留副本”可达。本次未点击归类/忽略，数据未改变。
- 这次是人工真实嵌入复核，补足了 UIAutomation 不能点击侧栏的缺口；`real-host-audit.ps1` 仍会因 `EmptyWindowAutomationPeer` 无法生成 `summary.json`，因此不要把人工截图写成自动审计通过。剩余边界是不同 DPI、Follow/高对比度、键盘焦点，以及备份/归类/忽略等真实操作回归。

### 后续启动协议补充

下一轮继续读取本文件、`docs/ai/PROJECT_MEMORY.md`、`docs/ai/WORKLOG.md` 和用户指定的目标文件；先检查 `git status` 与最近提交。页面继续以 `GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design/` Demo 为唯一视觉基准，保留当前游戏选择器、生产滚动条、真实命令/绑定、虚拟化和 Playnite 兼容性。优先补做真实备份/媒体归类操作的安全回归与不同 DPI/主题/键盘焦点检查；自动审计若仍返回 `EmptyWindowAutomationPeer`，如实记录边界，不能用 RenderHarness 替代。

## 2026-08-20 UI-259 媒体收件箱共享虚拟化交接

- `MediaCenterView.xaml` 的 `MediaInboxGrid` 已移除页面级 `Standard` 行虚拟化和列虚拟化关闭覆盖，统一继承 `GscRedesignWorkspaceDataGrid` 的 `Recycling`、Item scrolling、行/列虚拟化、排序和列宽调整；真实绑定与媒体 Inspector 操作不变。
- RenderHarness 已为 Media Inbox 提供 60 项夹具、五档高度和 0/25/50/75/100% 滚动位置探针，并将列虚拟化纳入 `ProbeGrid` 门禁；当前报告 `artifacts/ui-qa/media-virtualization-fix/render-qa-report.txt` 为 `render-qa OK`。
- UI-259 正式门禁：XAML 18/18；source validation 通过；Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 256 通过/62 跳过；WPF 0 error、19 warnings、146 info。
- 本轮没有重新安装真实 Playnite；UI-258 的真实七页人工嵌入复核仍是最近宿主事实。后续优先补做不同 DPI、Follow/高对比度、键盘焦点及真实备份/媒体归类操作的安全回归，若自动审计继续遇到 `EmptyWindowAutomationPeer` 必须如实记录。

## 2026-08-20 UI-260/261 存档页示例文案清理与工作区 Tab 样式回滚交接

- `AcrylicProductionShellView.xaml.cs` 的存档副标题已从硬编码 Demo 游戏名改为 `SelectedGame.Name`，空选择显示“未选择游戏”；页头在工作区切换和 `SelectedGame` 变化时都会刷新。对应源码契约已覆盖 Elden Ring 残留防回归。
- 生产页工作区 Tab 栏是当前项目视觉的明确例外：`Themes/Redesign.xaml` 中 `GscRedesignWorkspaceTabControl`/`GscRedesignWorkspaceTabItem` 已回滚为项目现有透明 header 带、独立圆角页签和横向滚动，不得恢复 Demo 的外层连续分段胶囊。Save/Media/Maintenance 的真实 Tab 结构、绑定、命令、内容 Stretch 和嵌套页签保持不变。
- RenderHarness 的重复模板部件名度量已修复；最新 `artifacts/ui-qa/project-tab-chrome-rollback/render-qa-report.txt` 为 `render-qa OK`，并已人工查看 Save/Media/Maintenance 代表截图。源码/XAML/差异检查及定向契约 15/15 均通过。
- UI-260/261 的 Release 安装验证已通过：XAML 18/18、Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过、安装 0.6.70/DLL 0.6.70.0；WPF validator 0 error、19 warnings、161 info。现在可以进入提交/推送前的最终工作树检查。
- 真实 Playnite 重启后的 Computer Use 当前仍可能返回 `foreground window did not report a process id`；不能用离屏截图代替重装后宿主 Tab 像素证据。剩余人工边界仍为 125%/150% DPI、窗口缩放、Follow/高对比度、键盘焦点和真实备份/媒体归类操作。

## 2026-08-25 UI-315 共享卡片/表格自适应毛玻璃交接

- 用户已要求所有卡片、表格、容器尽量使用毛玻璃，并希望颜色跟随当前游戏背景图。实现已集中在 `AdaptiveThemePaletteFactory.ApplyGameBackgroundGlassResources`，不要回到逐页写固定背景色的方式。
- 生产卡片和表格通常通过 `GscGlassFillBrush` / `GscGlassStrongBrush`，表头通过 `GscTableHeaderBrush`，浮层通过 `GscPopupBrush`；修改材质优先改这些共享资源和 `Redesign.xaml`，不要逐个页面加局部 Brush。
- `DashboardView` 主题刷新后以及当前游戏背景采样属性变化时，会把材质同步到 Dashboard、`AcrylicProductionShellView` 和所有生产 workspace 的 ResourceDictionary。没有游戏背景、禁用玻璃或高对比度时必须走中性回退。
- 底层游戏图 BlurEffect 的职责与卡片颜色分离：不能把 BlurEffect 直接挂到卡片，否则会把卡片自己的文字/表格一起模糊；也不能把采样渐变设成完全不透明，否则失去玻璃效果。
- UI-315 验证：source validation 通过；Release 0 warning/0 error；Core 59/59、Worker 199/199、Playnite 289 通过/57 跳过；WPF validator 0 error/18 warnings/166 info；多主题、多尺寸和 resize transition 的 render-qa 为 OK。
- 本轮没有新增真实 Playnite 重启后的逐页像素证据；后续若继续调透明度，应在真实宿主中复核 Bongo Cat 等不同主色背景、Follow/高对比度和 125%/150% DPI，并继续保留真实备份/媒体归类操作边界。

## 2026-08-25 UI-316 设置页毛玻璃交接

- 设置页不使用游戏图片背景，但必须保留整页玻璃层。运行时由 `ApplySettingsMaterialResources` 生成主题驱动的环境渐变、外壳/分类栏/表单/内容四级材质，以及仅作用于 `SettingsAmbientLayer` 的 BlurEffect。
- 需要继续保持 `SettingsScroller` 的现有滚动模型、五个分类、字段绑定、保存/取消和设置导入导出语义；不要为了加玻璃再包裹一层改变响应式代码定位或滚动通道。
- `tests/GameSaveCenter.RenderHarness/Program.cs` 已在 Settings 离屏渲染前调用 `ApplyThemeForAudit(FollowPlaynite)`，后续截图回归要保留这一步，否则会误测静态 DesignTokens。
- UI-316 验证：source validation、XAML 结构检查通过；Release 0 warning/0 error；Core 59/59、Worker 199/199、Playnite 289 通过/57 跳过；render-qa OK；WPF validator 0 error、18 warnings、172 info。
- 当前没有新的真实 Playnite 重启后设置窗口截图；下一轮若继续调材质，优先用真实宿主复核 Follow/高对比度、不同 DPI、关闭玻璃回退和键盘焦点，不能将 `.tmp/ui-qa-settings-glass-v5` 当作 Playnite 1:1 证据。

## 2026-08-25 UI-320 游戏选择器交接

- `AcrylicProductionShellView.xaml` 的生产游戏弹层使用共享 `ListBoxItem` 圆角模板；不要重新添加未基于共享样式的局部 `ListBoxItem`，否则选中和预选会回到 Playnite 的矩形默认视觉。
- 生产壳状态/平台/排序筛选默认值是“已安装 / 全部 / 名称”。平台列表异步重建后必须通过 `UiFilterSelection.RestoreDefault` 恢复显示；真实 `GamePicker` 绑定和选中命令不可改成静态选项。
- `DashboardView.xaml` 游戏列表 Row 的 `ClipToBounds` 是圆角状态契约；如果继续调整游戏行，请同时检查选中、鼠标预选、键盘焦点和高对比度边框。
- UI-320 已完成源码/XAML/Release/定向 Playnite 测试与离屏 render-qa。真实 Playnite 游戏弹层尚未由本轮自动截图验证，后续如能取得可识别宿主窗口，应优先确认选中/预选圆角和中间“全部”显示。

## 2026-08-25 UI-321 平台筛选显示时序交接

- 仅设置 `SelectedIndex="0"` 不足以保证生产游戏弹层的中间框显示“全部”：弹层初始隐藏时 ItemsSource 可能尚未生成。
- 维护 `AcrylicProductionShellView` 的游戏弹层时，平台默认恢复必须保留 `Loaded` 事件、弹层打开时的 `DataBind/Loaded` 调度，以及 `PlatformFilterOptions.CollectionChanged` 监听。
- `UiFilterSelection.RestoreDefault` 的有效选中保护不能删除；用户已经选择具体平台时，集合仍包含该平台就必须保留它。
- UI-321 已完成源码/XAML/Release/定向测试；真实 Playnite 重载后的中间框“全部”像素仍待确认。

## 2026-08-25 UI-322 底部状态栏与侧栏折叠交接

- `AcrylicProductionShellView.xaml` 的 `FooterSurface` 已跨越根 Grid 两列，`FooterStatusPanel` 显示真实 Worker/Ludusavi 状态；不要把状态灯恢复到侧栏，或改成不绑定 `Snapshot` 的静态文案。
- 侧栏品牌区右上角的 `SidebarCollapseButton` 是 26×26 小按钮；生产壳默认仍为 236 DIP 展开态，代码中的 78 DIP 是折叠态。折叠只隐藏文字并将导航项收成图标，不改变 `Nav*` 的真实工作区事件与设置入口。
- `ApplySidebarLayout` 必须在切换后调用既有 `ApplyHeaderLayout`/`ApplyPageLayout`，以便页面按新可用宽度重新布局；不要为折叠状态复制一套页面布局或改变滚动/虚拟化。
- 本阶段 `validate-source.py`、`check-xaml.ps1`、Release 构建、Playnite 294/351 和 RenderHarness `render-qa OK` 均已通过。离屏渲染没有覆盖真实 Playnite 中点击折叠按钮的像素结果；后续应在可识别宿主中复核展开/折叠、Light/Dark/Follow、125%/150% DPI、键盘焦点和导航 Tooltip。

## 2026-08-25 UI-323 状态栏、版本气泡与设置尺寸交接

- `FooterSurface` 的两个状态灯现在靠右，产品名和生产版说明已移除；不要把版本说明重新塞回底部栏。`SidebarProductionBadge` 保留在品牌行并显示程序集版本，折叠按钮位于标题下方的 `SidebarUtilityStrip`，避免按钮覆盖或挤压版本气泡。
- 如果继续修复折叠按钮，优先检查 `SidebarCollapseButton` 的 Click 路径和 `ApplySidebarLayout`，不要再把它放回品牌行；默认展开宽度仍为 236 DIP，折叠为 78 DIP。
- `GameSaveCenterSettingsView.xaml` 根 UserControl 的 `MinWidth=1180`、`MinHeight=760` 用于让 Playnite 设置宿主默认打开更大；`SettingsShell` 的 MaxWidth、原有分类/滚动/保存按钮语义不变。若真实宿主仍忽略最小尺寸，再单独调查 Playnite 设置窗口宿主，不要用页面内部硬编码宽度强行撑破窗口。
- 本阶段源码/XAML/Release/Playnite 295/352 和 RenderHarness `render-qa OK` 均已通过；尚未取得重载后真实 Playnite 的按钮点击/窗口尺寸像素证据，后续需复核折叠、展开、设置窗口、Follow/浅色、DPI 和键盘焦点。

## 2026-08-25 UI-324 侧栏折叠书签与动画交接

- `AcrylicProductionShellView.xaml` 的折叠入口现在是贴在侧栏右边、靠近底部的 `SidebarCollapseButton`，使用 `AcrylicSidebarBookmarkButton` 书签模板；不要再把它放回品牌标题行或新增一个大号导航按钮。
- 品牌名称和 `SidebarProductionBadge` 保持原位置，版本气泡继续显示程序集版本；`SidebarContentLayer` 只负责标题和真实导航内容，书签位于独立覆盖层，不改变导航项的测量和事件。
- 展开/收起仍使用 `ApplySidebarLayout` 的 236/78 DIP 宽度和页面响应式重算；动画只淡出/淡入并做 4 DIP `TranslateTransform`，时长 110/170ms。`MotionEnabledProvider` 连接到 `DashboardView.MotionEnabled`，关闭动画、系统禁用动画和高对比度时必须同步切换。
- 本阶段 `validate-source.py`、`check-xaml.ps1`、WPF 静态审查（0 error）、Release、Playnite 295 通过/57 跳过和 RenderHarness `render-qa OK` 均已通过；真实 Playnite 重载后的书签点击、键盘焦点、浅色/深色/Follow 与 DPI 仍需人工复核。

## 2026-08-25 UI-325 侧栏折叠控件纠偏交接

- UI-324 的字面“书签”实现已被否定并替换：不要恢复 `AcrylicSidebarBookmarkButton` 或 Path 丝带。用户参考图要求的是侧栏底部的一体式普通圆角控制。
- 当前使用共享 `AcrylicSidebarCollapseButton`：展开态约 168×34 DIP，左侧图标、中间“收起侧栏”、右侧箭头；折叠态约 40×34 DIP，只显示居中的展开图标。控件位于侧栏底部，不挤压品牌名称和版本气泡。
- `ApplySidebarLayout` 显式设置折叠按钮尺寸/边距、按钮内容居中、`SidebarCollapseLabel`/箭头可见性，以及 `NavOverviewContent` 等导航内部 StackPanel 的折叠态居中；原有 236/78 DIP 侧栏宽度、真实导航、动画和页面重排保持不变。
- 质量边界：源码/XAML/Release/Playnite 契约及 `.tmp/ui-qa-sidebar-control-v1` 离屏 QA 需要保持通过；真实 Playnite 重载后仍需人工确认点击、键盘焦点、Light/Dark/Follow、125%/150% DPI，不能把 RenderHarness 当作宿主像素证据。

## 2026-08-25 UI-326 折叠态图标对齐与首页右侧卡片间距

- 用户最新反馈集中在三处：折叠后品牌/导航/设置图标不在同一中心线；风险卡圆点离标题太近；“需关注事项”卡高度偏紧。
- 当前实现已在 `AcrylicProductionShellView.xaml` 为七个导航图标设置 `TextAlignment="Center"`，并在 `ApplySidebarLayout` 中将折叠态品牌区和导航内容统一到 26 DIP 居中槽；不要仅修改某一个图标的 Margin。
- 首页 `OverviewView.xaml` 的风险卡首列为 14 DIP；关注事项滚动视口 `MaxHeight` 为 220 DIP，仍保留有限内部滚动、页面根滚动和真实 `OpenAttentionCenterCommand`。
- 本轮验证：source/XAML/差异门禁通过，WPF 静态审查 0 error、18 warnings、172 info，Release 0 warning/0 error，Core 59/59、Worker 199/199、Playnite 295 通过/57 跳过，`.tmp/ui-qa-sidebar-icons-v1/render-qa-report.txt` 为 `render-qa OK`。
- 交付前仍需保持真实宿主边界说明：本轮未重新取得 Playnite 重启后的逐像素折叠截图；不得把 RenderHarness 结果扩写为真实 Playnite 的 Light/Dark/Follow、DPI、键盘焦点或 Tooltip 已验收。
# 历史交接（2026-09-22）

- 最新开发提交 `c0885757` 已完成 Trainer 发布时间小批量；行为 `27/27`、Release `0/0`，文档提交和 main 合并仍需在本阶段完成。
- 唯一开发工作区为 `D:\workplace\github\GameSaveCenter`，当前分支 `codex/ui-finesse-round2`；C 盘旧 worktree 已删除并完成 `git worktree prune`。
- 包含本次 `814d3e7a`/`412a7628` 的 `main` 合并提交 `8b3ebf33` 已在 D 盘以 Release 构建成功并推送；开发继续在 `codex/ui-finesse-round2`。
- 当前未验边界：真实 Playnite/package-host、UIA/读屏、OS 输入/IME、DPI/物理跨屏、最终呈现帧、ETW、宿主性能；Demo 原目录不可用。当前仓库不存在 `scripts/validate_wpf_ui.py`，不得把 WPF 静态审查写成已复跑。
- 下一可执行任务：继续核对 `DashboardViewModel`/Contracts 的真实 stale/缓存时间入口；保留报告/复制列/日志的稳定完整时间语义。

## 阶段交接（2026-09-23 Q06-06）

- 当前唯一工作区 `D:\workplace\github\GameSaveCenter`，当前分支 `main`；上一阶段四项用户布局反馈与 R00/R01/R18-04 证据校正均已提交、推送，详见 `docs/ai/CURRENT_STATE.md` 顶部和 `docs/design/reviews/ui-finesse-round3-20260915/evidence/USER-REPORTED-LAYOUT-20260923.md`。
- Q06-06 复用 WorkspaceStatePresenter 生产重试按钮：定向 Release 行为测试 `8/8`。Enter/Space 合成路由各调用命令一次，Space KeyDown 可观测按压态；CanExecute=false 键盘与 WPF 点击派发负例均不调用命令。
- 点击探针仅反射触发 `ButtonBase.OnClick`，不等于实际鼠标输入；合成 KeyUp 不更新 KeyboardDevice；真实鼠标、像素动画、Playnite host、UIA/读屏、物理 DPI/跨屏、presented frame、ETW 和宿主性能仍未验。Demo 原目录不可用，延续恢复生产基线；保持现有游戏选框/滚动条与 net462 兼容。
- 下一可执行任务：Q06-07 高频连续操作。只验证真实按钮命令重入/动画最终状态，复用 CanExecute 和现有安全语义，不新建全局禁用。

## 当前交接（2026-09-23 Q06-07）

- 当前工作区 `D:\workplace\github\GameSaveCenter`，分支 `main`；Q06-06 的单次键盘命令/负例已在 `caa60994` 提交并推送。
- Q06-07 复用现有忙态协调器、RelayCommand 与 GscMotion 逆转；本阶段增加一个受控生产按钮命令派发负例。测试分别按当前 `GscBuildCommit` 编译身份运行，合计 `8/8`；Release Playnite `net462` 成功。
- 真实鼠标高频序列、像素/动画屏幕反馈、真实 Playnite/UIA、物理 DPI/跨屏、presented frame、ETW、宿主性能仍未验。R08 testhost 的 WPF TextServicesHost 清理异常曾输出，但测试为 `2/2`、进程 exit 0，根因未知。
- 下一可执行任务：Q06-08 状态序列录证。先核对现有 normal/hover/pressed/focus/disabled 覆盖，再补可靠行为探针和失败负例；不把合成输入、离屏截图当真实鼠标或宿主呈现。

## 当前交接（2026-09-23 R00/R01 freshness 与设置宿主差异）

- 当前仓库在 `D:\workplace\github\GameSaveCenter` 的 `main`；最新 production source HEAD `e9bebee860c3a1374fba671bfd933c3cd681b543`，Q06-08 代码提交为 `8846d712`，之后只有证据/交接文档提交。不要使用 `codex/ui-finesse-round2` 分支。
- R00/R01 freshness current report 绑定 e9bebee8，有 14 条记录，均 `needsRerun=false`、`matchedSourcePaths=0`；R00-01/02 与 R00-05 仍绑定 `3a1dadd8` 证据身份。freshness self-test 在 e9bebee8 exit 0；全局 `documentationOnlyChange=false`。package identity `not-provided`。该结果不代表这批重跑过 R00 测试、RenderHarness 或重新安装。
- 用户最新设置截图仍见搜索框偏右、顶部图标下移；current main 的设置视图已包含左对齐与锚点修正，双主题行为几何测试过去为 `8/8`，但它是在 STA/DPI 1.0 离屏窗口。不要宣称用户当前宿主问题已解决。上一轮隔离 Playnite host 因 CEF `platform_channel` `0x5` 没有正常进入插件视图。
- 下一可执行任务：在截图窗口尺寸换算出的逻辑尺寸/系统 DPI 下检查当前设置视图布局，确认 `ApplyResponsiveLayout` 的 SizeChanged 路由，采集当前待测包身份，并对比 header 图标 top、标题/搜索框 left、状态提示和路径编辑按钮位置。如果 current checkout 复现，则修共享布局并补正/负行为验证；否则记录包身份或 host 差异待核，不关闭问题，然后继续依赖已满足的 Q/R 小批量。
- 已有边界：没有真实 Playnite 当前 package identity、正常宿主呈现、物理 DPI/跨屏、presented frame、ETW/宿主性能；不写用户存档、媒体、云端或外发诊断。Demo 原目录不可用，沿用恢复生产基线。

## 当前交接（2026-09-23 设置截图行为复核）

- 保持在 `main`。R00/R01 freshness 报告采样于 source HEAD `e9bebee8`，14/14 records 无需重跑、sourcePath 命中 0；freshness 自测通过，package identity 未提供。
- 用户新设置截图的搜索框居中/顶部图标下移与当前 production settings layout 不同。本批在 `ReportedWorkspaceLayoutBehaviorTests.SettingsHeaderAndPathActionsStayAnchoredToTheirLabelsAndEachOther` 删除私有布局方法手动调用，改由实际隔离 WPF Window 的 Loaded/SizeChanged 路由；宽 `1880×1200`、初始/恢复 `1280×840`、紧凑 `560`，Light/Dark `2/2`。title/search Δ0、center negative Δ287.33 DIP、compact width392/no overflow、save hint row `1→0`、paths buttons/combo 36 DIP/center Δ0。证据 `evidence/settings-header-responsive-20260923/`。
- Release Playnite `net462` / tests `net472`；build metadata `GscBuildCommit=62b17b0c`；保留 `MediaCenterView.xaml.cs:703 CS8602` warning。共享输出第一次被另一 `testhost.net472` 锁定；改用 `.tmp/settings-header-event-20260923/bin` 后 `2/2` 成功，未停止其他进程。`.tmp` 通过归档 TRX 后清理。
- 现有源码路径自动行为通过，但仍没有当前用户 package identity、正常 Playnite package-host 或物理 DPI 结果；之前隔离 host CEF `platform_channel 0x5` 阻挡。下一项若能使用既有隔离流程核对当前包身份/host 则继续；否则保留未验事实并推进独立 Q/R。不要把用户截图标为已解决。
## 当前交接（2026-09-23 设置页事件链复核）

- 工作分支 main；freshness JSON 采样 HEAD 62b17b0c，14 条 R00/R01 记录 needsRerun=false、matchedSourcePaths=0，documentationOnlyChange=false，package identity not-provided。
- 设置页生产源码没有改；行为测试实际经历 WPF Loaded/SizeChanged，不再手动反射调用 ApplyResponsiveLayout。双主题 2/2，标题/搜索左差 0 DIP，居中错位负例 287.33 DIP，窗口尺寸序列含 1280×840、1880×1200、560、恢复；路径编辑控件 36 DIP。证据 settings-header-responsive-20260923/README.md 与 TRX。
- 用户截图和正常 Playnite host 未绑定到当前 package identity；隔离 host 之前的 CEF platform_channel 0x5 阻挡仍未消除，不宣称用户宿主已修复。下一项先核对 Settings 父容器/宿主布局约束，再按现有隔离流程复现；受阻则继续可独立执行的 Q/R 小批任务。
## 当前交接（2026-09-23 设置页顶栏控件几何补测）

- 主分支在 13442aa4；当前未提交修改只扩展 ReportedWorkspaceLayoutBehaviorTests 并更新该行为证据/交接。
- Light/Dark 2/2；图标/标题横向间距 12 DIP，标题/搜索左差 0 DIP，顶部恢复默认下拉框与两个按钮均 36 DIP、中心差 0；路径编辑控件同为 36 DIP。完整几何输出见 settings-header-responsive-20260923/README.md 和 settings-header-controls-geometry.trx。
- Playnite net462/test net472 隔离构建通过；NU1900 来自 NuGet advisory 源不可达，既有 CS8602 保留。未返回的单独 logger 尝试无结果；诊断 console+TRX 双 logger 复跑成功 2/2。
- 用户实际 package identity、正常 Playnite 父容器和物理 DPI 仍未确认，不宣称用户截图问题已修复。下一步核对当前 Settings 父容器/布局映射，宿主受阻时推进依赖已满足的 Q/R。

## 当前交接（2026-09-23 main Release 设置包复验）

- main HEAD 8e4f3194227afb28640754f12ab0889cb8bb71ce；完整隔离 Release solution XAML 24/24、0 errors、两条既有 CS8602；设置页 Loaded/SizeChanged几何测试当前构建 Light/Dark 2/2。TRX 在 evidence/settings-header-responsive-20260923/settings-header-controls-main-8e4f3194.trx。
- 新包 artifacts/GameSaveCenter-0.6.73-main-8e4f3194.pext 的六个插件/Worker 构建身份一致，identity 0.6.73+8e4f3194227afb28640754f12ab0889cb8bb71ce，SHA-256 B6602DB38D98CDE9B11B8B0B414F43337B00AA021A11C001BBCB542912D9B3B0。包未安装；现有同版本 artifacts 文件保留。
- R00/R01 freshness 在 HEAD 8e4f3194 采样：14/14 fresh、0 source match、documentationOnlyChange=false、package not-provided；自测三类通过。
- 用户截图仍与当前受控源/离屏图不同，当前 user package identity、正常 Playnite host、物理 DPI 尚待核对；CEF platform_channel 0x5 是实际宿主阻挡。等待用户给出加载包版本/构建 identity；同时继续下一项依赖已满足的 Q/R，不把准备好的包或隔离图写成用户屏幕验证。

## 当前交接（2026-09-24 R19-07 与设置截图复核）

- 在 main 上收口 R19-07。提交/测试身份 `9760c648` 的完整 Release solution 成功（XAML `24/24`、0 errors、两条既有 Media `CS8602`）；Worker `15/15`、媒体移动/显式刷新 `2/2`、相关 Playnite 行为 `16/16`。
- 同一构建的 `ReportedWorkspaceLayoutBehaviorTests` 四页/双主题 `8/8`：Media 控件高度/表格滚动边界，Task 失败行框，Save 窗口化操作行，Settings 图标/标题/搜索/输入按钮几何均通过。TRX 在 round3 evidence 目录，R19-07 文档已有本次数字和测试夹具负例细节。
- 用户设置截图尚未绑定运行进程。读取到的本机 DLL identity `7a4ba2a9` 早于 `3a1dadd8` 修正，只是线索；隔离 Playnite 受 CEF `platform_channel 0x5` 阻挡，不能宣称当前宿主截图修复。保留真实宿主/物理 DPI边界。
- 本次变更包含两个测试和阶段文档，没有生产代码改变；提交后已用新身份重跑全部关键用例，TRX 与账本均刷新到 `9760c648`。`.tmp/r19d`、`.tmp/r19e` 已清理。
- 下一可执行任务：R19-08 慢调用可取消。先核对现有外部工具与 IPC 取消/超时/未知写结果语义，再增加 fake 慢服务的真实行为/负例；不进行真实云端写入。

## 当前交接（2026-09-24 R19-08）

- R19-08 已在当前 main `6618de22` 复核；完整 Release solution 成功，XAML `24/24`、0 errors、保留两条既有 `MediaCenterView.xaml.cs:703 CS8602`。
- 行为门 `60 passed / 6 skipped / 0 failed / 66 total`：Worker进程/云状态/ledger `37/37`、内存流 IPC `10/10`、Playnite Busy `4/4`、最新请求取消 `4/4`、取消反馈 `4/4`；NamedPipe 客户端仅非 pipe 隔离名称用例 `1/1`，其余六条因当前环境禁止客户端 pipe 而 skip。无权限绕过。
- 当前源码已有明确超时、同 RequestId 有限复核、可能已接收未知结果、finally 复位；真实 Playnite/Worker pipe、真实 Ludusavi/Rclone 与远端云写仍未验。设置截图的实际加载包/真实宿主呈现也仍未验。
- TRX 已按 `6618de22` 放入 R19-08 evidence；六份原始结果均已归档，`.tmp/r19-08a` 已清理。本阶段无生产代码变更。
- 下一可执行任务：R20-01 概览下一步。复用 `OverviewPriorityResolver`、状态投影与游戏选框命令，使用合成状态覆盖无游戏/未匹配/可备份/失败待处理；绝不启动真实存档或云写。
# 当前交接（2026-09-24 R06 排序与行几何）

- 当前仓库 `D:\workplace\github\GameSaveCenter`，分支 `main`，从当前最新生产代码继续；不要切回无独立 worktree 的 `codex/ui-finesse-round2`，也不要用 main 的旧实现覆盖当前源。
- `crash.zip` 的真实 DataGrid 列头排序 NRE 已在 `DataGridStableSortController` 修复：排序前确认 `ItemsSource` view 仍有 `SourceCollection`，失效视图不刷新，已消费的无效列头事件不会流入 WPF 默认反射排序；detached 后点击可恢复 controller-owned sort arrow。
- 当前隔离 Release solution/XAML `24/24` 成功、0 errors/2 条既有 `MediaCenterView.xaml.cs:703 CS8602`；同 checkout 的真实 WPF 列头与 detached-view 行为 `7/7`。证据和 TRX 位于 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R06-SORTING-DETACHED-VIEW-CURRENT-MAIN-20260924.md`。不签收真实 Playnite/package-host 输入和整项 R06-02。
- 下一可执行小批量：修 Media Inbox 重复游戏目标选择，复用全局 `SelectedGame`，保留命令可执行条件、确认目标快照、取消/错误语义。排序和行几何的隔离行为证据已分别归档；真实宿主边界照旧。
- 用户此前指认的媒体/任务/存档/设置窗口问题，不可因 STA 离屏几何通过而标成真实宿主已修复；现行隔离 Playnite仍受 CEF `platform_channel 0x5` 阻挡，不重试绕过。保留真实宿主、最终呈现帧、物理 DPI/跨屏、Windows UIA/IME、ETW 与宿主性能未验边界。Demo 原目录不可用，沿用已恢复生产基线。
