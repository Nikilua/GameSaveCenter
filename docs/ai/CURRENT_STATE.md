## 当前事实入口（Round6 V01–V20，2026-11-14）
V19中断/Reduced Motion行为及V20实际WPF属性采样已提交在 `9097e6c86a8d5a8808eaab03b8b92c1a318b6ece`。SDK8.0.423 Release定向3/3、0失败、0跳过；147条采样含136次CompositionTarget.Rendering回调和11次状态快照，带UTC采集ID、elapsed时间与有效可见性。可交互播放器、CSV和TRX见[V19–V20证据](../design/reviews/ui-redesign-round6-20261113/evidence/V19-V20/README.md)。这是受控属性轨迹，不是录屏；详情轨迹的IsVisible=false，真实Playnite前台/物理显示仍待验。该SHA GitHub run #1576（43588654627）和前一候选run #1575（43587564816）仍在进行，需核对所有适用双SDK jobs。

V16–V18源码改造：任务/媒体详情切换只处理最新绑定对象；Toast仅重排受影响卡片且不调用同步UpdateLayout；缩略图真实Loading持续180ms才呈现提示，快响应不闪且业务时序不延迟。定向行为7/7通过；SDK8 Release全量在最后两处Toast优化之前为0 warning/0 error，Core157/157、Worker407/407、Playnite2048项中1990通过、0失败、58既有skip、157个WPF类完成；最终改动随后重新编译并定向通过。CI #1574曾在旧SHA暴露三处过期动效断言，已同步修正。详见V16–V18证据索引。

V01–V04基线和设计规格、V05–V08存档页重构及V12前后评审已完成；V09–V11共享字阶/图标、按钮和浮层样式已实现并通过本地全门禁。Demo-first依据是同级目录的 `GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design/`：生产字体现在优先采用 Demo 的 Segoe UI Variable Text/Display，保留 Windows 与可选中文回退；共享 action icon 18px、图文间距4px，按钮控制角10px；ComboBox/Tooltip浮层统一走共享主题资源。页面命令、Binding、忙碌占位、键盘/UIA、虚拟化、当前游戏选框、滚动条及恢复安全语义均保留。

V09–V11截图以相同合成数据、UTC时间、主题、窗口和DIP缩放对比V05–V08后的生产XAML。8组覆盖浅深主题、1040×700与1600×900、常规和详情打开；受控运行在WinSta0私有桌面，输入桌面保持Default。客户区像素为744×460与1276×660；图像差异和逐项评审见[V09–V11证据](../design/reviews/ui-redesign-round6-20261113/evidence/V09-V11/README.md)。这些截图只说明受控WPF渲染，不表示真实Playnite前台、物理DPI、实体输入或帧率。

SDK8.0.423 / Release：全构建0 warning/0 error、exit0；Core 157/157，Worker 407/407，Playnite 2,044项中1,986通过、0失败、58个既有skip，157/157个WPF隔离类。`python scripts/validate-source.py`通过；WPF静态审查27个生产XAML、0 error/26 warning/164 info；全量Render QA 334 PNG、0 `PROBLEM`、`render-qa OK`。本阶段未运行真实Playnite宿主；用户数据、配置、存档和云端均未访问/写入。

V13–V15已实现微交互令牌、PageHost中断续接和紧凑存档详情淡入/淡出；本地反向行为测试与Render QA通过。预提交渲染报告的SHA仍指向父提交，不能作为候选SHA证据；本批提交后的精确Render QA和GitHub CI仍待验。详情帧测试验证选中版本保持与中间值连续，不代表非激活测试窗口验证了真实键盘焦点，也没有覆盖真实Playnite。

下一批按每批2–4项推进V21–V24，推广任务、媒体、概览、设置页面；同时把V19–V20精确CI核完。最终V25–V28冻结候选后运行中文根入口安装链与独立scripts/build.cmd，复核精确提交上的所有适用GitHub CI job。清理限制和未触碰的其他任务TRX/用户文件沿用下方历史记录。

## Round5 / P1 历史交接基线（2026-11-13）

当前冻结候选与实际门禁身份为 `008b3d5dde4440534fe35a563422c60817681058`；它相对产品实现提交 `b667e22445fbf57aa591f39f3d968f5c166c7cb6` 只增加文档，`src/`、`tests/`、`scripts/` 与资源无差异。中文根入口 `GameSaveCenter-一键构建安装运行.cmd` → `GameSaveCenter-Run.cmd` 使用SDK `8.0.423` / Release / `SkipTests=false` 完成build/test/publish/package/隔离安装及启动；直接捕获 cmd PID `36560` 的退出码为 `0`。160份TRX共2,043项：1,985通过、0失败、58跳过（不计通过），157/157个WPF隔离类。独立`scripts/build.cmd`同一 SHA、实际SDK `8.0.423` / Release、直接退出码 `0`，计数相同。它的辅助退出记录里 `sdkVersion=9.0.302` 是外层环境的系统PATH SDK；本次实际选择的SDK以构建日志与诊断摘要为准，均为8.0.423。

GitHub run #1564 / `43391873169` 精确验证该 SHA：基线 job `131773490954` 与 SDK 10.0.401 job `131773490438` 的全部步骤均 completed/success。build preview artifact `14024746736` / digest `481bfc1d97991f5a4cac6f575ae53c665bcc282b2f77104799acc9c7ee9591c7`；基线诊断 artifact `14024666949` / digest `9c3e0fc66b7c0f2d9fba822327b3c9f97b103daccdea85fd12a54cda22556d0a`；SDK10诊断 artifact `14022865985` / digest `ea5e51001bc6170ddca7bc07ca52478c539bcb4a5a3913f06c40b80dfe0d0fa2`。required-checks/ruleset名单仍是既有HTTP403，权限未变未重试；两job成功不代表保护规则名单已验证。

A01/A02/A05/A06/A07/A08/A11本地或既有证据范围已收口；A03并入Round5 U09-04，A04仍等真实可见宿主、物理输入与呈现帧。A09分段安全覆盖已核但同会话宿主闭环待条件；A10后台full规模已核，真实宿主代表规模待条件。按钮下沿忙碌提示的共享绑定修复在提交 `b667e224`；本次用户截图的运行程序集身份未知，不宣称已由真实宿主确认修复。以上阶段未访问真实Playnite profile/配置/游戏库/存档，未执行恢复、删除或云端写入。计划状态以[16项执行账本](../PROJECT_ASSESSMENT_2026-11-06.md)和[P1本地证据](../design/reviews/project-assessment-2026-11-06/evidence/P1-close-b667e224-20261113/README.md)为准。

此处记录 Round5/P1最终候选的历史门禁事实；此后Round6阶段状态以本文件顶部交接和当前Git HEAD为准。纯文档提交不产生新的产品安装身份。

## 历史：2026-11-13 P1隔离宿主验收状态（c2fcc804）

冻结源码候选为 `c2fcc80477b936bafbaf46e983c266365480fb5c`（实现提交：`隔离一键安装启动验收环境`）。候选入口、缺口和收益在实现前记录于`56d3f30e17d8d5924710ed8fc62c7c31fd3c4cfd`。中文根入口SDK8.0.423 / Release在新标记的`.tmp` Playnite profile完成build/test/publish/package/install/start，成功分支退出码0；160份TRX共2,042项，1,984通过、58 `NotExecuted`、0失败，157/157个WPF隔离类。程序集身份与源码SHA一致，包hash和进程参数见[本机验收记录](../design/reviews/project-assessment-2026-11-06/evidence/P1-host-c2fcc804-20261113/README.md)。

Playnite进入首次配置后日志记录应用启动；没有观察到GameSaveCenter Worker进程，因此未验插件运行态、真实库数据或UI Automation。云上传开关在子进程中强制为false；没有读取真实配置/数据库/存档，没有恢复、删除或云端写入。独立`scripts/build.cmd`同一SHA退出1：编译0 warning/0 error，完成44份TRX共1,351项（1,333通过、18未执行、0测试失败），WPF 41/157类；输入桌面探针在`run-playnite-tests-isolated.ps1:282`失败，不能把剩余类别计为通过。[原始结果和下一步](../design/reviews/project-assessment-2026-11-06/evidence/P1-host-c2fcc804-20261113/scripts-build-cmd-result.json)。

GitHub run #1551 / `43356307416` 精确验证源码SHA：基线15/15、SDK10.0.401 8/8均成功；required-checks/ruleset名单仍因既有HTTP403未验证，权限未变未重试。输入桌面可被读取后仅复验独立`scripts/build.cmd`；不重启真实profile、不执行真实恢复/删除/云写。P1隔离入口候选结果不是16项报告的确认缺陷，也不表示全部宿主UI验收完成。

## 2026-11-12 冻结候选门禁结果（历史）

本条门禁记录的冻结产品源码候选为 `52a57db9a4c8dacb1678fbf6fca4cac9b00e87e0`；之后提交的状态文档未修改产品源文件。GitHub run #1545（`43161957758`）验证的是该源码候选：基线 job `130995009638` 和 SDK 10.0.401 job `130995009279` 均成功；job/artifact身份见[精确CI记录](../design/reviews/project-assessment-2026-11-06/evidence/A07-A08-A11-20261112/ci-run-43161957758.json)。required-check/ruleset名单此前HTTP 403，仍未验证，也未重复请求。当前分支HEAD以Git log为准。

本机 `GameSaveCenter-一键构建安装运行.cmd` → `GameSaveCenter-Run.cmd` 使用SDK8.0.423完成编译、测试、Worker发布、身份校验和打包，160份TRX共2,042项：1,974通过、0失败、68跳过。扩展目录安装写入被拒，根入口exit 1；候选没有安装或启动。Playnite与Worker在脚本后均未运行，旧安装未重启。独立 `scripts/build.cmd` 使用同一SDK，测试记录1,829项：1,800通过、0失败、29跳过，但输入桌面检查在WPF类`UiDiagnosticsExporterTests`失败，只有148/157个WPF类完成，命令exit 1。详情与摘要hash：[根入口结果](../design/reviews/project-assessment-2026-11-06/evidence/A07-A08-A11-20261112/root-entry-result.json)、[独立构建结果](../design/reviews/project-assessment-2026-11-06/evidence/A07-A08-A11-20261112/scripts-build-cmd-result.json)。

A05/A06/A07/A08/A11的本地可处理代码与测试范围已收口，不能据此声称宿主安装/启动已验；A03/U09-04和A04保持宿主待验，P2/P3未开始。没有提升权限、操作真实存档、删除用户数据或写入云端。当前必要包、诊断和失败日志保留；清理预检说明见[清理复核](../design/reviews/project-assessment-2026-11-06/evidence/A07-A08-A11-20261112/cleanup-review-20261112.json)。

## 2026-11-12 当前入口事实与计划接续

启动时本地 `main`/`origin/main` 为 `9b26680040ce174f986d34037a83c697175f7554`，对应GitHub run #1542（`43132918400`）双job成功。后续A05/A06提交 `06b88bf95e7d3646ad905a4333938369891fca1b` 的run #1544（`43150017597`）也已终态成功：基线job `130956727575` 15/15、SDK 10.0.401 job `130956727381` 8/8。artifact ID与digest见[精确CI记录](../design/reviews/project-assessment-2026-11-06/evidence/A05-A06-20261112/ci-run-43150017597.json)。required-checks/ruleset读取此前HTTP 403；运行job成功不代表保护规则已验证，不重复请求。

Round5用户手工走查接受此前71项待验体验项，当前78项验收完成；U09-04动画性能量化仍进行中，U09-06权限核验仍远端待验。用户反馈不代表逐项执行Narrator、IME、物理DPI、性能计量，也不授权真实存档恢复、媒体删除或云端写入。Round4 S账本为40已验收/2已满足/21代码完成待验/1外部阻塞；T账本32项为24已验收/3已满足/2代码完成待验/3实施中。

16项计划已经与Round5、S/T去重。A01引用同源全链和双SDK CI，A02由用户体验验收覆盖，A03并入U09-04，A04保持宿主边界。A05/A06与A07/A08/A11的本地可处理代码/测试范围已收口；冻结候选 `52a57db9a4c8dacb1678fbf6fca4cac9b00e87e0` 的CI双job成功。中文根入口在安装写入处退出1，独立 `scripts/build.cmd` 在输入桌面检查处退出1，详见上方门禁记录。计划本身不证明16个缺陷；A03/A04仍等待宿主条件，P2/P3未启动。详见[计划执行账本](../PROJECT_ASSESSMENT_2026-11-06.md)。

当前分支在第二批开始时为 `06b88bf95e7d3646ad905a4333938369891fca1b`；A07/A08/A11待提交候选仅含本批源码测试、脚本和文档，精确SHA需提交后回填。未跟踪历史TRX、`TestResults/`和Round5证据目录继续保留。

## 2026-11-12 A05/A06 Worker收口

补齐 `AtomicFileWriter` 同目标并发写和临时文件创建后取消边界，并在Worker恢复任务时同步持久化 `RestoreReport` 的人工介入终态，保留失败阶段和PreRestore身份。首轮12项定向测试10过/2失败/0跳过；首轮日志/TRX保留在[证据目录](../design/reviews/project-assessment-2026-11-06/evidence/A05-A06-20261112/README.md)。修正后定向12/12通过；Worker全套406项中405通过、0失败、1项既有Named Pipe环境skip（不计通过）。SDK8.0.423，`validate-source.py`与`git diff --check`通过。精确提交`06b88bf95e7d3646ad905a4333938369891fca1b`的run #1544基线15/15、SDK10 8/8步成功。后续冻结候选门禁已运行：根入口构建/测试/打包成功但安装写入被拒；独立 `scripts/build.cmd` 因输入桌面检查未完成全部WPF类。见上方最新记录。

## 2026-11-12 A07/A08/A11 第二批

A07去重为已有RequestId/ledger、取消歧义、重启协调与任务阶段覆盖充分；真实宿主UI恢复仍未验。A08仅新增合成客户端磁盘满HResult在部分写入后的回滚用例，Release定向测试SDK8.0.423 `1/1 pass, 0 fail, 0 skip`，TRX及源码/程序集SHA见[A07/A08/A11证据](../design/reviews/project-assessment-2026-11-06/evidence/A07-A08-A11-20261112/README.md)。A11增加提交前暂存证据路径和重复大blob检查，PowerShell夹具4/4；历史长路径保留且不追溯阻断。

该批提交已冻结为 `52a57db9a4c8dacb1678fbf6fca4cac9b00e87e0`，run #1545基线与SDK10 jobs均成功。完整根入口在安装权限处exit 1，独立 `scripts/build.cmd` 在输入桌面检查处exit 1；TRX、跳过数及原始诊断见[A07/A08/A11门禁记录](../design/reviews/project-assessment-2026-11-06/evidence/A07-A08-A11-20261112/README.md)。没有权限提升或重试，也未运行真实存档恢复、删除、云写入或宿主权限操作；A03/U09-04与A04继续等待真实宿主条件。

## 2026-11-10 U09-05 编译/检查隐藏启动跟进

为 Codex 的编译/检查调用补强 `scripts/hide-console.ps1`：新开的控制台只在 Explorer 或不共享控制台的 PowerShell/Codex 父链隐藏；与交互终端共享控制台的 PowerShell保持可见。新增5项策略测试；三入口退出码夹具9/9、策略5/5。SDK8.0.423 Release `scripts/build.cmd` 短路径完整运行exit0：2032项/1964通过/0失败/68既有skip，157个WPF类私有桌面隔离；157/157桌面身份合格且Default输入桌面未变。长路径首跑的3个恢复断言失败诊断保留，缩短路径后通过；没有跳过或删除断言。Source Validation与git diff --check exit0。记录见 `../design/reviews/ui-polish-round5-20261027/evidence/U09-05-console-hide-20261110/README.md`。

本批cmd、PowerShell、dotnet进程观察到主窗口句柄0，启动时隐藏并重定向日志；这不能证明用户交互桌面无瞬时窗口，也未取得反馈黑窗的HWND/PID。无XAML/资源改动，不跑Render QA；未启动安装/真实Playnite。U09-05仍代码完成待验收。最新C盘4,145,061,888 bytes、D盘19,418,931,200 bytes，完整入口仍差149,905,408 bytes。清理清单/空间快照见证据 `cleanup.json`，没有清理缓存、用户或他任务TestResults，也未触碰盘符/卷/映射/存档。整轮计数7/71/1/1；U09-04仍进行中，U09-06远端待验。

精确SHA `28b42cf612af9667732a5e9483f11cba9643d221` 的 GitHub Actions run #1533 / `42783454951` 已完成：基线 job `129718789544` 的15步、SDK10.0.401 job `129718789885` 的8步全部成功；预览包与两份诊断artifact身份、大小及digest见 Round5 `evidence/U09-05-console-hide-20261110/ci-run-42783454951.json`。此次完整链预检复测 C盘4,125,560,832 bytes、D盘19,415,154,688 bytes，C盘仍少169,406,464 bytes；不运行安装启动入口。required checks/ruleset仍因先前403远端待验；提交状态汇总未返回独立status check。

## 2026-11-10 Round5 U09-04 源码归因复核

U09-04仍进行中。U00与当前产品的任务表滚动保存方法、滚动诊断实现和120ms共享动效令牌未变；当前任务详情新增动效，但离屏关闭P95约250ms、Rendering回调P95为125–149ms，不能据此断定真实Playnite输入/呈现回退。关闭后立即滚动的组合样本仍未锁定源代码原因；不盲目改动生产代码或时长。下一步需真实Playnite呈现帧、物理输入响应和该面板分配量数据。源码证据：[U09-04归因记录](../design/reviews/ui-polish-round5-20261027/evidence/U09-04-source-attribution-20261110.md)。本批产品源未变，根入口/独立build已对相同源通过；C/D分别有4,454,600,704 / 19,438,219,264 bytes空闲，没有清理或盘符操作。

## 2026-11-09 U03-01/02/03 按钮与图标任务去重复核

生产共享按钮五态、延迟忙碌层和17个图标操作的UIA Name/Command/Click/键盘焦点已有覆盖，没有新增源码缺口。定向TRX 13/13、0失败/skip，10张双主题状态图和报告的SHA/大小全匹配；`8c927a93` 的 GitHub #1325 / run `40561836233` 基线与SDK10 jobs及所有步骤success，artifact身份在 `../design/reviews/ui-polish-round5-20261027/evidence/U03-01-03/ci-run-40561836233.json`。当前产品SHA `e570f7b7`完整本地build/Render QA/双SDKCI另见U09-05证据。三项保持代码完成待验收：真实鼠标、Playnite、UIA客户端/屏幕阅读器及物理DPI未验。

本批无生产源码或渲染输出，未删除文件。起始C盘空闲3,223,830,528 bytes、D盘20,201,070,592 bytes；完整根入口仍未达C盘4GiB要求。只保留对应审计证据，不碰未知Temp/缓存、用户TestResults、存档或任何盘符/卷映射。

## 2026-11-09 U02-01/02/03/04 主题层次与状态语义复核

共享主题分层、强调色配额和六态语义图标/文字/底色与高对比回退无新增源码缺口；本批不重复改色或跑渲染。历史修复候选 `f59fe4c8` 的 workflow #1322 / run `40555566776` 基线job `122370590922` 和SDK10 job `122370591760` 均completed/success，包及诊断artifact摘要见 `../design/reviews/ui-polish-round5-20261027/evidence/U02-01-04/ci-run-40555566776.json`。此前 `bf7540e2` run `40545849867` 双job失败诊断也已补齐artifact身份；当前产品SHA `e570f7b7` 独立完整build、双SDKCI与Render QA成功，见U09-05证据。U02四项保持代码完成待验收，真实Playnite高对比、UIA/键盘/IME、物理DPI仍待；required-checks保持远端待验（既有403，不重复尝试）。

本批读取的U02临时build/render输出目录均已不存在，无删除。C盘阶段首末观察3,231,997,952→3,228,778,496 bytes，距离根入口4 GiB仍差1,066,188,800 bytes；D盘20,203,831,296→20,202,524,672 bytes。磁盘变化不归因于本批，保留归档证据，不清系统/NuGet缓存、用户TestResults/配置、存档或未知文件，也不改盘符/卷/映射。

## 2026-11-09 U01-01/02 混排与标题层级去重

逐项核对 13 张归档截图和4份报告的大小/SHA-256，均匹配；被进度引用的两个 TRX 为37/37与1/1，0失败、0跳过。实现与修正提交7e61bba3、bb984a07在当前分支历史中；当前产品SHA e570f7b7已完成独立完整build、双SDK CI和394图Render QA。U01-01/02保持“代码完成待验收”，缺可选字体未安装回退及真实Playnite/UIA/键盘IME/物理DPI验收；本批无源码改动、不重跑渲染或测试。

旧cleanup列出的四个U01临时路径当前都不存在，本批没有删除或认领空间回收。C盘阶段首末观察3,234,779,136→3,231,997,952 bytes，D盘20,204,027,904→20,203,831,296 bytes；C盘距完整根入口4 GiB仍差1,062,969,344 bytes。后续优先仅清理可确认属于本项目且无证据用途的文件；不触碰NuGet/系统缓存、用户TestResults、配置、存档、卷或盘符。完整安装启动链继续等C盘空间达标。

## 2026-11-09 U01-03/04/05/06 去重核验

复核四项共享排版/图标任务的既有增量和TRX清单：U01-03/04/05/06相关6份记录文件大小与SHA-256均匹配；当前产品SHA e570f7b7 的baseline/SDK10 CI已全成功，故U01-05旧实现提交查询不可读不再作为当前源码阻塞。U01-03/04/06原状态保持已满足，U01-05仍代码完成待验收（真实宿主/UIA/物理DPI）。没有改产品源码或重复生成截图。旧U01-05-06产物目录当前不存在，本批未删除且未归因空间回收。C盘 3216539648 bytes、根入口4 GiB差1078427648；D盘20205613056 bytes。

## 2026-11-09 U00-01 盘点与去重复核

现有 U00-01 基线已完成四页各五条观察和旧任务去重映射；16张配套PNG哈希逐一复核全部匹配。PROGRESS 将该项由代码完成待验收更正为已满足，整轮计数6/70/3/1。旧U00临时目录目前不存在，但本次没有删除它也不归因释放量。C盘本次实测 3222917120 bytes，根入口4 GiB门槛尚差 1072050176 bytes；D盘 20212924416 bytes。项目专属C盘剩余内容仅当前审计文件747 bytes和空临时目录，不足以安全补齐；不清理用户/未知数据。纯文档校验完成后提交，完整构建仍由冻结候选最终链负责。

## 2026-11-09 U09-05 精确SHA完整build与CI通过；中文根链待C盘空间

当前冻结源码为 `e570f7b7e0d5019b7f57c411237de3abb55a6e89`。独立 `scripts/build.cmd -Configuration Release` 使用 SDK 8.0.423 完整 exit 0：160份TRX、2032项、1964通过、0失败，157个WPF隔离类；68个既有skip按撤销架构、Named Pipe环境和默认交互指针原因记录，不计通过。完整日志/TRX在本机 `artifacts/build-diagnostics/round5-u09-05-detached-e570f7b7/20261109T085813954Z-7831612a/`，摘要见 `../design/reviews/ui-polish-round5-20261027/evidence/U09-05-full-build-e570f7b7-20261109/`。

GitHub run #1500 / `42579208247` 对同一SHA的基线与SDK10.0.401 job均success；required-checks/ruleset仍因既有403未能读取，标“远端待验”。完整本地build监测WinSta0默认桌面33分45秒，show/hide为0；这不归因用户报告的黑窗，也不覆盖最终Playnite入口。当前根入口要求C盘4GiB空闲，实际3,238,178,816 bytes，差1,056,788,480 bytes；专属临时测试目录为空，没有确认可清理的项目文件补足差额。D盘空闲19,902,840,832 bytes。Playnite/Worker都未运行。不要降低空间门槛或清理未知数据；等待C盘达到门槛后按中文入口做安装身份和启动验收。

## 2026-11-09 U09-05 选框修正及Render QA全量复验通过

a74287e7暴露共享DataGrid选框离视口仅1 DIP。当前源修正普通/非活动框为视口减8 DIP、左边距4；2 DIP键盘框为视口减7 DIP、左边距3，内容8 DIP裁切/水平滚动不变。SDK8.0.423 Release隔离重编0 warning/error，五类定向几何/状态/资源测试157通过、0失败；39个既有“今日工作台”历史结构断言NotExecuted不计通过。Source Validation exit0，WPF静态398 XAML/0 errors。Render QA首轮在289图OOM后，通过仅测试Harness的WPF/WIC每8图回收修正完成全量重跑：SDK8.0.423，exit0，394 PNG/0 PROBLEM；Harness在WinSta0私有desktop，输入Default保持不变。源码SHA/报告SHA在 Round5 `right-inset-regression-20261109/`。下一步先提交并push候选，独立跑精确SHA `scripts/build.cmd` 和新SHA workflow全部适用job，冻结后通过中文包装器/英文根入口验收完整安装启动和身份。用户确认闪窗在编译/检查时发生；本批工具进程创建时隐藏并重定向，不代表已确认黑窗原因。未操作盘符、卷、映射、存档、媒体或云写入；required checks仍因先前403远端待验，不重试。
## 2026-11-09 U09-05 Render QA OOM，资源释放候选待复验

当前共享选框源代码定向测试已验证：SDK8.0.423 Release隔离Rebuild exit0/0 warnings/0 errors，5个适用WPF类157通过、0失败；39个旧“今日工作台”断言NotExecuted不计通过。Source Validation exit0，WPF静态398 XAML、0 errors/218 warnings/1440 info。Render QA Harness编译成功后，在WinSta0私有桌面生成289图，于 `RenderTargetBitmap.Render` OOM并exit1；不是通过，未到完整394图。Default输入桌面保持Default。已给Harness加仅测试端每8图回收候选；不改生产UI或覆盖点，需全量复验。证据/失败摘要/清理记录在 `../design/reviews/ui-polish-round5-20261027/evidence/U09-05-selection-boundary-20261109/right-inset-regression-20261109/`。用户已确认黑窗出现在编译/检查期间，本批外层进程从启动隐藏并重定向；无用户窗口身份，保持未归因。冻结SHA后的 `scripts/build.cmd`、Render QA、中文入口完整安装启动/身份和双SDK CI仍待。
## 2026-11-09 U09-05 选框留白已修正，冻结候选门禁待跑

基线 `a74287e7ecd240a0fbd4fa394a9befb8fd796748` 的共享选框在Task/Light非活动状态下仅距视口1 DIP，本机独立 `scripts/build.cmd` 与 GitHub #1499 的基线及SDK10.0.401 job均复现R23失败。未删断言或放宽skip。工作树修正共享普通/非活动框为视口宽减8 DIP、左边距4；2 DIP键盘框为视口宽减7、左边距3，并同步资源测试。SDK8.0.423 Release隔离Rebuild exit0、0警告/错误；5类定向行为/资源测试157通过、0失败、39个旧架构断言NotExecuted（不计通过）。证据清单20项SHA-256核对通过，见Round5 `right-inset-regression-20261109/`。

用户确认闪窗出现在编译/检查时。本轮至今启动的构建检查均从隐藏进程启动并重定向；WPF测试使用WinSta0私有非输入桌面。现有桌面监听仅覆盖上一失败构建的中后段，不能证明完整无闪或归因用户窗口；最终记录保持未归因。下一步提交并推送修正候选，独立执行完整 `scripts/build.cmd`，核对新SHA工作流所有适用job；门禁完成后冻结跟踪文件，再由中文包装器转交英文根入口做完整构建/测试/发布/打包/安装身份和启动验收。入口前重查Playnite/Worker并按正常关闭流程处理；不强杀任意进程。required checks/ruleset先前403，U09-06继续远端待验，不重复探测。未操作卷、盘符、映射或用户数据。
## 2026-11-09 U09-05：R10 旧边距预期已修正；新候选门禁待跑

`3527fb1d9c2c701539ca727a05b2ab6157cd94c3` 的本机完整构建（SDK 8.0.423）与 GitHub workflow #1498 / run `42553664674` 双 SDK job均失败在同一个旧测试预期：Overview selected `TaskItemChrome`仍期待旧 4-DIP 右边距。工作树已只将 Overview 选框预期改为 0；内容不位移断言及共享ListBox 4-DIP断言保留。重新编译并运行R10整类，SDK8.0.423、WinSta0私有非输入桌面，3/3通过、0失败/skip、子进程exit0，input desktop前后Default。证据见`../design/reviews/ui-polish-round5-20261027/evidence/U09-05-selection-boundary-20261109/focused-r10-fix/README.md`及`diagnostics/sdk8-r10-stale-margin/`。

完整构建的旧失败摘要是1518项/1488通过/1失败/29既有skip，执行到第93个WPF类；尚未对新修正SHA做全量构建。#1498两个job IDs及失败artifact哈希见`../design/reviews/ui-polish-round5-20261027/evidence/U09-05-selection-boundary-20261109/diagnostics/sdk8-r10-stale-margin/ci-run-1498.json`；修正SHA的双SDK CI仍需push后查询。当前旧Playnite PID24248与Worker PID42704在11-09 03:40 UTC后的进程快照中均不再存在；未结束进程。执行中文完整入口前重新查宿主状态。用户闪屏仍无同一时段HWND/PID/desktop归因；本地验证全从隐藏启动，WPF测试使用私有desktop。未修改驱动器、盘符、卷映射，未操作用户存档、媒体或云端数据。U09-05/U09-08仍进行中，U09-06 required-checks继续“远端待验”。

## 2026-11-09 U09-05 选框右缘修复；完整宿主验收待做

用户截图中的 Overview 最近任务时间、Save History/Task Grid 行内容未完全落在选中框内。当前未提交候选已移除 Overview 内容独立视口宽度，并将共享 DataGrid 选中/非活动/焦点框右缘扩至视口内1/2 DIP；保持行内容8 DIP裁切及原滚动系统。SDK8.0.423 Release `scripts/build.cmd -SkipTests` exit0；适用WPF行为11/11、共享资源类140 pass/39旧架构skip、Source Validation/WPF静态审查exit0、Render QA 394 PNG/0问题。命令启动时隐藏、输出重定向，并对Default输入桌面做全程事件监听，SHOW/HIDE均0。证据见 `../design/reviews/ui-polish-round5-20261027/evidence/U09-05-selection-boundary-20261109/README.md`。真实宿主、物理输入/DPI未验；用户黑窗没有HWND/PID归因。

Playnite PID24248与Worker PID42704仍运行且无主窗口；没有关闭或强杀，最终中文完整安装入口尚不能运行。推送新候选后的双SDKCI、完整独立构建和根入口/安装身份仍待验。80项计数/边界以Round5 `PROGRESS.md` 为准，U09-05和U09-08继续，U09-06 required checks仍“远端待验”。未动驱动器或用户存档/媒体/云数据。

## 2026-11-09 U09-05 选框像素复核；闪屏继续未归因

基于已推送 `3dc59152fee9e2e7e18872c13cadaee2e1c28dc8`，扩大 Overview/Task Grid/Save History 长列表窄视口回归，并为任务详情长文增加可选离屏截图。SDK8.0.423 Release `scripts/build.cmd -SkipTests` exit0、0 warning/error；四个私有WPF测试类8/8通过、0失败、0 skip；Source Validation通过，WPF静态26 XAML/0 errors/26 warnings/164 info。7张RenderTargetBitmap显示选框内时间、任务状态/进度、存档设备字样都在框内，长详情会换行；本批未改生产XAML或资源。详细日志、TRX、桌面身份、图片哈希见 `../design/reviews/ui-polish-round5-20261027/evidence/U09-05-frame-pixel-check-20261109/README.md`。

用户仍报告闪屏。附件没有HWND/PID/desktop，与11月6日图片相同；本批WPF窗口全在WinSta0私有非输入桌面，未执行完整安装启动入口、没有重启Playnite。此受控结果不能证明用户桌面无闪；U09-05继续进行中。 入口隐藏助手只在 Explorer -> cmd 链路实际隐藏父控制台；Codex/PowerShell链路会保留其交互终端，是本代理直接启动命令时的潜在闪窗路径。后续代理构建从启动时使用隐藏进程并重定向日志；这不识别用户截图中的黑窗来源。精确候选CI待push后核对；Round5最终中文根入口链、真实宿主页和required-checks均未因本批结项。未操作盘符/卷/映射、存档、媒体或云数据。

## 2026-11-09 U09-04/U09-05 继续：任务详情探针与闪屏未归因

本批从已推送基线 `0258ab3513c7d1bc95b8b622eead42a0fab30a76` 继续。RenderHarness新增双主题任务详情开合探针及显式构建身份守卫；U00/current快照的Harness编译均SDK8.0.423 Release、0 warning/0 error，探针12轮/主题完成。当前受控窗口p95和Rendering回调不是实际呈现帧，且未测分配，U09-04继续进行中。Render QA重跑394 PNG、无问题；代码没有生产XAML改动。证据见 `../design/reviews/ui-polish-round5-20261027/evidence/U09-04-task-inspector-motion-20261109/README.md`。

用户再次报告仍闪屏；本次图片SHA与11月6日相同、文件原始时间11月6日且无HWND/PID。只读Computer Use插件初始化失败，无法读取当前可见窗口；本批探针与Render QA在WinSta0私有桌面，输入Default未变，但不能由此称用户桌面“无闪”。本批没有运行安装启动根入口或重启Playnite。U09-05保持进行中。

精确旧基线 `0258ab35` 的 GitHub #1494 已 completed/success：baseline job `128830946846`、SDK10.0.401 job `128830946943` 均通过；artifact详情见 `../design/reviews/ui-polish-round5-20261027/evidence/U09-05-renderqa-private-20261109/ci-run-1494.json`。required-checks/ruleset仍因既往403远端待验。当前修改尚待提交、push及新SHA CI；本轮80项最终冻结候选的中文→英文根入口完整安装/启动链仍待一次性执行。未操作盘符/卷、存档、媒体或云端数据。

## 2026-11-09 U09-05 Render QA 隔离已推送；根入口最终复验待做

本地 `main` 已推送提交 `0258ab3513c7d1bc95b8b622eead42a0fab30a76`。Render QA 的 WPF宿主改在 `WinSta0\GscRenderQA_<guid>` 运行，SDK按项目基线初始化并核验父子/输入desktop身份；Source Validation加入守卫。精确提交 `scripts/build.cmd` exit0：2030项、1962通过、0失败、68 skip，157个WPF类隔离；Render QA exit0、394 PNG、无问题行。位于WinSta0\Default的WinEvent监听在Render QA全程与WPF测试监听就绪后的中后段目标SHOW/HIDE均为0（最早几个类未监听）；截图附件仍是11月6日同一文件，黑窗身份未确认。证据见 [U09-05 本批记录](../design/reviews/ui-polish-round5-20261027/evidence/U09-05-renderqa-private-20261109/README.md)。
逐项核验完整构建的157份 WPF 测试桌面结果：157个唯一 WinSta0 私有桌面、Default 输入桌面前后未变、157个退出码为0；清单见 [逐类桌面身份](../design/reviews/ui-polish-round5-20261027/evidence/U09-05-renderqa-private-20261109/wpf-desktop-identities.json)。
源码层面仍有一个未复现的入口竞态：从 Explorer 直接打开 `.cmd` 时，控制台可能先创建、再由隐藏辅助进程隐藏；这不能认定为用户所见黑窗来源。后续本机验收从非输入桌面发起，并继续保留黑窗未归因状态。

GitHub run #1494 / `42513349534` 的两个job `128830946846`（基线）和 `128830946943`（SDK10.0.401）已 `completed/success`，job/artifact身份见本轮证据。U09-05不结项；整轮80项最终冻结候选仍需中文根入口完整安装/启动验收及该候选新SHA CI。每批局部构建继续用隔离测试路径，不要把安装器关闭/重启宿主误当普通编译。required-checks/ruleset权限之前403，不重复试探；真实用户黑窗仍需新鲜窗口身份或实际宿主时段确认。未改盘符/卷、用户存档、媒体或云数据。
## 2026-11-08 U09-05 根入口复核后仍收到闪屏反馈

精确 `626a3ad90cd5b0cf72d1fbe9d8d6f566e0744ddf` 的中文完整入口成功完成 build/test、Worker发布、打包、安装身份核验并到达Playnite启动成功分支：2030项、1972通过、0失败、58 skip，SDK8.0.423，ZIP/PEXT一致 SHA 与六个安装程序集身份见 [根入口证据](../design/reviews/ui-polish-round5-20261027/evidence/U09-05-no-flash-20261107/root-entry-626a3ad9-20261108/README.md)。但外层监控因输出文件锁退出125，没有捕获根命令进程最终码；不能记录为观察到的 exit 0。

用户指出仍闪屏。`Default` 监控目标事件为0，但初始枚举省略270个窗口；当前 CUA 应用清单也只是时点快照。该完整入口会关闭并重启 Playnite，是潜在可见变化，尚无 HWND/PID/class 可将其认定为用户黑窗来源。Playnite/Worker 当前只读进程快照仍存活且没有主窗口句柄，未强杀。小批继续仅用独立 `scripts/build.cmd` 和隔离行为检查；不重复运行完整安装入口。U09-05保持进行中，U09-06 required-checks/ruleset权限此前403仍远端待验。

当前 SHA `626a3ad` 的 GitHub run #1491 / `42467986641` 已 completed/success：baseline job `128691845992` 15/15步，SDK10 job `128691846130` 8/8步；artifact identity见上述证据 `ci-run-1491.json`。未修改盘符、卷或映射；未操作存档、媒体、云端数据。

## 2026-11-08 Round5 U09-05 黑窗复核与 R08 本机采样修正

用户再次报告编译仍闪屏。附件文件记录时间为 2026-11-06 13:59:27，没有 HWND/PID/桌面身份；U09-05 保持进行中。`b573fea7210d810eef221721395b007da96d7458` 的 10 分钟 `WinSta0\Default` 顶层 `EVENT_OBJECT_SHOW/HIDE` 监听为 0 次，覆盖该次独立全测后段；不能外推至手动 Explorer 启动或真实 Playnite 页面。本次只读桌面清单没有 Playnite 窗口，旧进程 23488/30516 无 MainWindowHandle；不重复启动、不强杀。

该 SHA 的本机独立完整 `scripts/build.cmd` 在 R08 按压入口中间帧断言失败（第77/157个WPF类）；测试未被删弱。相同旧程序集的单用例1/1、类4/4通过后，当前只调整测试夹具：PressedOverlay本地时窗1600ms，探针超时有界覆盖，生产XAML/动效资源和断言未改。SDK8.0.423 Release `scripts/build.cmd -SkipTests` exit0、0 warning/0 error、XAML26/26；Source Validation、diff-check通过；当前精确测试程序集SHA `BF037240E27CF8631C74A0C621EFFA061EAD679B578F783EC244D888EBD2C4B0` 下定向1/1运行三次均通过、0 skip，三次均在私有非输入桌面且Default不变。证据见[本批目录](../design/reviews/ui-polish-round5-20261027/evidence/U09-05-no-flash-20261107/r08-press-capture-20261108/README.md)。更新源码的独立完整测试、中文安装启动链、新 SHA 双 SDK CI仍待做；当前未声称黑窗已解决。

`b573fea` CI #1489 / run `42349851252` 两个适用 job成功：SDK基线 `128335451147`、SDK10.0.401 `128335451280`；required checks/ruleset仍因先前403远端待验。Playnite实际UI、实体键盘/IME/UIA/物理DPI未验；未操作盘符或映射，未做真实恢复/媒体删除/云写入。

## 2026-11-07 Round5 U09-05 WPF 测试窗隔离更新：0feff185

提交 `0feff18501a5ec971e3d7fb2ba212e8d0d28e0ac` 已推送。交互式本机的每个 WPF 测试类现运行在 WinSta0 私有非输入桌面；SDK8.0.423 的独立 `scripts/build.cmd` 完整链为 2030 项 / 1962 通过 / 0 失败 / 68 既有 skip，157 个 WPF 类隔离，Default 桌面事件监测 0 次显示。Q10 定向动画 1/1；源检查、PowerShell 语法、WPF 静态审查均通过。精确 SHA 的 Windows CI #1486 双 SDK job 均成功。细节见[Round5 进度](../design/reviews/ui-polish-round5-20261027/PROGRESS.md)及[本批证据](../design/reviews/ui-polish-round5-20261027/evidence/U09-05-hidden-wpf-20261107/README.md)。

中文根入口输出 `[OK]`，完整摘要 2030/1972/0/58，安装版本和六个程序集 SHA 已核对。入口明确成功分支最终 `exit /b 0`；外层 PowerShell 日志捕获因启动后的 Playnite/Worker 保留重定向句柄没有记录自身最终退出，因此不伪称该包装器退出状态已观察。官方 `--shutdown` 等待超时，未强杀。Default 桌面监测事件为 0，但原截图无 HWND/PID，不能确认窗口身份或推广为所有手动启动绝不闪现；U09-05 仍进行中。U09-04、U09-08 继续，U09-06 required-check 权限沿用此前 403 并保持远端待验。未触碰盘符或映射，也未做真实存档恢复、媒体删除或云端写入。

## 2026-11-07 U09-05 再次黑窗反馈：修订未验收

用户再次报告黑窗/闪屏。历史截图没有 HWND/PID，无法确认窗口身份。此前移除 pause 和隐藏 PowerShell 子进程只处理控制台遗留问题；更直接的验证风险已确认：本机曾把 Q10 WPF 动效测试放在输入桌面，WinEvent 捕获 8 个无标题 testhost.net472.exe 窗口显示事件。测试夹具使用 Opacity=0.01，会在用户桌面留下极暗的 UI 画面；这足以解释本轮验证会闪，但不能追溯认定截图中的 HWND 就是这些窗口。

当前工作树增加 scripts/hide-console.ps1，并接入中文入口、英文入口和独立 scripts/build.cmd。它仅尝试隐藏 Explorer 双击命令链自有控制台；从现有终端启动时保持终端可见。既有 true→false 原生控制台探针早于父链保护逻辑，不能作当前保护分支的验收；本候选的无控制台 smoke、Source Validation、git diff --check、未知参数/退出码以及入口参数传播 9/9 通过。控制台隐藏不隔离 WPF 测试 HWND。

本候选尚未提交/冻结。后续所有本机 WPF 构建、Render QA 和 Playnite 启动均需从 WinSta0 的非活动 desktop 执行，并监测输入 Default；不再把 WPF 用例放到用户前台。独立完整 scripts/build.cmd、中文入口完整安装启动链和新 SHA Windows CI 均待验；U09-05 保持进行中。详见[Round5进度](../design/reviews/ui-polish-round5-20261027/PROGRESS.md)与[黑窗反馈诊断](../design/reviews/ui-polish-round5-20261027/evidence/U09-05-no-flash-20261107/followup-user-feedback.md)。

## 2026-11-07 Round5 U09-05 黑窗复核与冻结验收

精确 SHA `76e5670f02a3bf85432c312a27d1ae51fc379678`：中文真实入口完整构建/测试/发布/打包/安装身份/启动链 exit 0，独立 `scripts/build.cmd` exit 0，SDK 8.0.423 / Release。根链2030项/1972通过/0失败/58跳过；独立构建2030项/1962通过/0失败/68跳过；两轮各157个隔离WPF类，skip均保留原因且不计通过。ZIP/PEXT各45,739,053 bytes、同 SHA-256；6个安装程序集与包内项逐一匹配。

25 ms `Default` 桌面监测覆盖根链两次尝试和独立构建，无目标窗口出现/消失事件；成功根链的可见Playnite HWND属于非活动桌面。监测不能排除短于25 ms的窗口，也没有回溯识别用户截图HWND。`DEV-INSTALL-010`修正不经Shell的Playnite进程创建；未来验收继续以非活动桌面运行，不能将隔离采样说成绝对无闪保证。源码run #1481双SDK通过。U09-05已满足；状态6/71/2/1，U09-04、U09-08进行中，U09-06因required-check端点先前403仍远端待验。详见[Round5进度](../design/reviews/ui-polish-round5-20261027/PROGRESS.md)及[窗口/安装/CI证据](../design/reviews/ui-polish-round5-20261027/evidence/U09-05-no-flash-20261107/BASELINE.md)。

## 2026-11-06 Round5 当前冻结候选与真实宿主边界

冻结源码 f0d2b8f1 在非活动桌面通过独立 build.cmd 及中文根入口完整链，两个入口均退出 0。Windows CI #1473 双 SDK 通过。当前状态以 Round5 总表为准，6 项已满足、71 项代码完成待验收、2 项进行中、1 项远端待验。 [Round5 进度](../design/reviews/ui-polish-round5-20261027/PROGRESS.md)

本机安装启动留下 Playnite 与 Worker 实例。真实宿主审计脚本在启动前要求没有现存实例，并需要读取进程命令行以验证隔离。下一步在已核实无冲突时，用独立测试资料和 Worker 数据运行审计。不要向日常桌面发出输入或启动第二个实例。实体输入、DPI 与呈现帧仍需单独验证。

## 2026-11-06 综合评估与后续16项计划

新增[综合评估与计划](../PROJECT_ASSESSMENT_2026-11-06.md)，基线4719a59c。结论是优先真实体验、性能归因和稳定性收口；10项质量任务、3项维护治理、3项可选功能发现。A01先核对最新焦点修复门禁，其余按依赖去重推进；不重复Round5/S/T实现，不解除旧宿主与权限待验。此次仅文档抽查与规划，未运行产品或查询实时CI，不宣称新漏洞或功能缺失已证实。构建、CI和安全清理要求见计划。

## 2026-11-06 Round5 U09-04 同条件离屏复测更新

- U00 `5d0a2448d6315af61571d0d3fbecfd902e5344fc` 与冻结候选 `a13a7dbbe27f4cb90d37057163e1481aa8abf355` 已在统一 Harness、固定样本时间、1040×700 DIP、双主题和60秒+30秒时长下完成正序/反序配对。托管累计分配降低约12.6%–13.4%；全局P95方向不一致，滚动与详情收起代理两轮有回退信号，不能冒充实际帧/物理输入测量。
- Task页综合滚动代理较慢，未记录滚动器身份；TaskGrid内部滚动专项持平；逐页小样本和Dispatcher长尾未锁定单一源码因果。动效探针保留一次深色中点失败和681.6ms回调长尾；另一反转轮正常。全部窗口隐藏、屏幕外且Opacity 0.01；没有为测试打开可见窗口，也没有更改C/D/R盘符、SUBST或映射。
- 原始报告、哈希、失败中点图、CI身份和清理明细见 `docs/design/reviews/ui-polish-round5-20261027/evidence/U09-04-aligned-20261106/`。唯一阶段根`.tmp/round5-u09-04-aligned-20261106`已按literal路径清理20,405文件/1,010,046,290 bytes；D可用空间净增1,058,009,088 bytes，C变化-266,240 bytes（同期波动，不归因）。其它artifacts/.tmp/TestResults/配置/存档均未触碰。
- U09-04仍进行中，因没有Playnite前台ETW/DWM帧或物理输入测量，不把离屏回调当真实呈现；U09-06 required-check名单仍远端待验。文档提交 `5cb4a68c0eb843c3c6657fb6cdc84cfac9aade93` 的CI #1462（run 42127707158）completed/success：SDK8 job127596045500 15/15步、SDK10 job127596045343 8/8步，artifact身份见该复测证据目录 `ci-run-42127707158.json`。状态计数7项已满足、71项代码完成待验收、U09-04进行中、U09-06远端待验。

## 2026-11-06 Round5 最终入口门禁与当前未验项：a13a7dbb

- 冻结源码 `a13a7dbbe27f4cb90d37057163e1481aa8abf355`。本机 SDK 8.0.423 / Release：独立 `scripts/build.cmd` exit0，160 TRX / 2030项 / 1962通过 / 0失败 / 68跳过；中文根入口完整编译、测试、Worker发布、打包、安装身份和启动链 exit0，160 TRX / 2030项 / 1972通过 / 0失败 / 58跳过。跳过原因、320份TRX、包/安装SHA见 `reviews/ui-polish-round5-20261027/evidence/U09-05-final-a13a7dbb/`。
- UI_CHANGE_GATE：Source Validation与XAML结构检查通过；WPF静态审查26 XAML/0 error/26 warnings/164 info；精确候选Render QA 394张/0 PROBLEM。用户要求的时间/选框边缘文字没有回归；R08/Q10行为TRX见上述证据。
- 精确源码run #1461 / 42110142797 completed/success：baseline SDK8 job127535228072 15/15步，SDK10.0.401 job127535227511 8/8步，artifact身份见 `windows-ci.json`。required-check/ruleset读取已有403，不重试，U09-06远端待验。
- Playnite日志证实 `0.6.73.0` 已加载并观察3个游戏，但本次进程窗口句柄为0，没有显示实际插件页；页面视觉、外部UIA、实体键盘/IME、物理DPI/跨屏仍待真实宿主验收。未执行真实存档恢复、媒体删除或云端写入。本轮未触碰驱动器映射。
- 状态：7项已满足、71项代码完成待验收、U09-04进行中、U09-06远端待验。继续补足U00同条件帧时/累计分配来源或可比复测，不用逻辑proxy冒充呈现帧；旧S/T宿主/权限待验不阻断可独立工作。

## 2026-11-04 Round5 U09-05/U09-08 本机最终验收：292f3bc6

- 精确候选 `292f3bc63a8ab08c57505d50ac5ba68b6c0d431a`：独立 `scripts/build.cmd` exit 0，中文实际根入口 exit 0；SDK 8.0.423 / Release。根安装run `20261104T060044161Z-375720ee`，诊断run `20261104T060046800Z-ba7cae6d`。
- 两入口各160 TRX/2030项/0失败。独立build为1962通过、68跳过；完整链1972通过、58跳过；跳过原因和包/安装哈希见 `reviews/ui-polish-round5-20261027/evidence/U09-05-final-292f3bc6/`。根入口完整安装身份与源码SHA一致。
- Playnite PID 50956响应，扩展日志确认 `0.6.73.0` 加载。窗口在非活动隐藏桌面，没有切换桌面或捕获实机页面；本轮不宣称实际页面视觉验收。该进程仍运行，未强杀或重复启动。
- 源码 run #1454 双SDK成功；required-check名单因既有403仍远端待验。U09-05/U09-08完成，状态7/71/1/1；U09-04真实帧/分配数据继续进行，U09-06远端待验。真实恢复、媒体删除和云端写入未执行。
- 用户要求的R: SUBST映射已解除，底层C盘目录仍在；C/D盘符未变。旧构建输出清理记录见同证据目录 `cleanup.json`。
## 2026-11-04 Round5 当前框外文字修复候选：292f3bc6

- 用户新图的问题是 Overview 时间、Save History 最右列文字与 Task 队列状态/详情落在选框右缘外。源码 `4ded0f52` 修复共享视口裁切和选框几何，`3e98148e` 增加没有祖先滚动绑定时的配置边距回退，`292f3bc6` 对齐 Overview 框线；三次中文提交均已推送。
- 精确候选 SDK8.0.423 独立 `scripts/build.cmd` 2030项、1962通过、0失败、68 skip；Source Validation、WPF static 26 XAML/0 errors、XAML结构检查、Render QA 394 PNG/0 PROBLEM均通过。证据在 `reviews/ui-polish-round5-20261027/evidence/U06R5-2911/`，含六张双主题关键图与160 TRX。
- 同 SHA 中文完整链在编译前因 C 盘 4 GiB 预检失败（exit1）。安全清理task-owned isolated checkout artifacts/.tmp释放285,892,608 bytes；2026-11-04 00:58 UTC复测仍比预检门槛少330,522,624 bytes。用户确认能释放至少463 MB，待空间实际达标后重跑。尚未安装、核对安装身份或启动本候选Playnite；U09-05与U09-08进行中。
- 精确源码 GitHub run #1454 / 41742480073（SHA 292f3bc6）已 completed/success：SDK 8.0.423 job126287170658 15/15步、SDK10.0.401 job126287170119 8/8步通过；编译测试、Render QA、WPF审查、打包及上传均通过。三份artifact ID/大小/digest见`reviews/ui-polish-round5-20261027/evidence/U06R5-2911/ci-run-41742480073.json`。required-check/ruleset读取此前403不重试，名单仍远端待验。状态5/71/3/1；真实Playnite页面、UIA、实体键盘IME、物理DPI与真实业务写入边界不变。
## 2026-11-04 Round5 证据归档修复与 CI 验收：f203f15f

- 文档提交967e07e1的run #1451两job在Windows checkout因TRX长路径失败，源/测试/渲染/打包未运行且无artifact；原始job日志及归因见`reviews/ui-polish-round5-20261027/evidence/U06-02-3aa-20261104/ci-run-41681642551-failure.json`。
- 修复提交f203f15f18bf8ce5f84c38ae7e3f03a407ab3a3e缩短归档路径，320份全链/独立build TRX及6份focused TRX字节与SHA-256不变。run #1452 / 41684052918 completed/success：baseline job126084964467 15/15步、SDK10 job126084964958 8/8步；编译测试、Render QA、WPF审查、打包和artifact上传全通过。artifact ID/digest见`reviews/ui-polish-round5-20261027/evidence/U06-02-3aa-20261104/ci-run-41684052918.json`。
- 源码候选仍为3aaae534。required-check/ruleset接口先前403，不重试，U09-06仍远端待验；真实Playnite页面/UIA/键盘IME/物理DPI与真实业务写操作边界不变。Round5状态7满足、71项代码完成待验收、U09-04进行中、U09-06远端待验。
## 2026-11-04 Round5 当前候选与用户框外文字反馈：3aaae534

- 用户附图中的三个目标是 Overview 最近任务时间、Save History 末列、Task 队列状态标签。U00-01 视觉基线和旧任务去重已经记录在 reviews/ui-polish-round5-20261027/evidence/U00-01/BASELINE.md，本批沿用而不重复。
- b85f397c/28ff5e0f 行内容裁切候选曾将 DataGrid 选框 width 参数改为4，R23证实破坏4 DIP滚动轨道。3aaae534修复后恢复 DataGrid/Dashboard 参数8、焦点框7，并保留RowContent向内裁切。R06 SaveHistory夹具固定740 DIP以复现截图窄表横向滚动；R06/R10/R23目标回归已在独立build和完整中文链运行通过。
- 3aaae534已推送main。SDK8.0.423 / MSBuild17.11.48；中文实际入口exit0，160份TRX 2028项/1970通过/0失败/58既有skip；独立scripts/build.cmd exit0，2028/1960/0/68。所有skip按TRX保留原因且不计通过。
- 精确CI run #1450 / 41659230554 completed/success：基线job125997757253 15步、SDK10.0.401 job125997757096 8步、全部适用steps成功；artifact ID/digest见同Round5 evidence目录ci-run JSON。required-check/ruleset只读接口此前403，不重试；U09-06仍远端待验。
- 中文入口安装0.6.73，ZIP/PEXT SHA一致，六个安装程序集带候选SHA，Playnite/Worker进程响应并记录插件加载；未观察实际插件页面。屏幕页、UIA、实体键盘/IME、物理DPI仍待验；不执行真实恢复、媒体删除或云写入。
- Round5状态7满足、71代码完成待验收、U09-04进行中、U09-06远端待验。详细测试/包/安装/CI身份见reviews/ui-polish-round5-20261027/evidence/U06-02-3aa-20261104/BASELINE.md。
## 2026-11-03 Round5 当前源码与验收：6cd0f9ec

精确源码6cd0f9ec55ada37f03a88d24bc46690ed1837a06已推送。共享ListBox与生产DataGrid选框右侧inset收至约4 DIP，修复最近任务时间、存档末列及任务状态看起来在框外的问题；选框overlay不挤文本/图标。用户图所指出的位置已通过Overview/Save/Task/Shell修复后Render QA图和R06/R10/R23测试复核。Render QA 334图、0条PROBLEM；Source Validation及WPF静态扫描88 XAML/0错误。

实际中文入口GameSaveCenter-一键构建安装运行.cmd转交GameSaveCenter-Run.cmd，SDK8.0.423完整Release编译、测试、发布、打包、安装身份核验和Playnite启动exit0；8项目0警告/错误，158份TRX、2026项、1968通过、0失败、58 skip。独立scripts/build.cmd按职责exit0，2026项/1958通过/0失败/68 skip；skip不计通过。安装DLL和ZIP/PEXT均对应精确源码SHA。第一次根入口调用只在D盘空间预检不足时退出；记录并安全清理6个旧阶段输出后完整重跑成功。

精确push run [41613860203（#1446）](https://github.com/Nikilua/GameSaveCenter/actions/runs/41613860203) completed/success：SDK基线job 125842266009的15步和SDK10.0.401 job 125842265806的8步通过；三个artifact ID/digest及全部测试/安装证据见Round5 evidence/U06-02-frame-wrap-6cd0f9ec-20261103/。required-check读取此前403，不重试；U09-06仍远端待验。

本轮计数7项已满足、71项代码完成待验收、U09-04进行中、U09-06远端待验。Playnite启动/安装身份不证明页面布局；实际插件页、外部UIA、实体键盘/IME和物理DPI未验证。真实存档恢复、媒体删除、云端写入未运行。U09-04仍缺U00原始逐帧/累计分配及真实宿主呈现。

## 历史记录：2026-11-03 Round5 源码与门禁状态 de6f7f82

源码候选 `de6f7f82038c59c5fbdbb9b366c3e280354ffc5f` 已提交并推送。最近活动的蓝色圆角选框保留，但不推移整行或图标；共享焦点圈和DataGrid兼容行选框使用独立overlay。选框几何回归R10类3/3通过，生产资源状态R23类12/12通过。

实际中文根入口 `GameSaveCenter-一键构建安装运行.cmd` 经 `GameSaveCenter-Run.cmd` 对精确候选执行完整SDK8.0.423 Release编译、测试、Worker发布、打包、安装核验和Playnite启动，exit0。8项目、0 warning/error，158份TRX、2026项、1968通过、0失败、58既有skip（不计通过）；155个WPF隔离类。独立`scripts/build.cmd`在`eebef009`按职责exit0，`src/`、`tests/`、`scripts/`树与de6一致。安装清单0.6.73、六个DLL程序集身份均含de6 SHA；Playnite日志确认插件加载，但没有观察实际插件页面。完整本机证据：Round5 `evidence/U09-05-final-root-de6f7f82-20261103/`。

精确push run [41547554497（#1441）](https://github.com/Nikilua/GameSaveCenter/actions/runs/41547554497) completed/success。基线job `125626741394` 的15步和SDK10.0.401 job `125626741153` 的8步均通过，包括Render QA、WPF静态审查、打包及上传；三artifact ID/digest见同证据目录 `ci-run-41547554497.json`。required-check/规则集读取此前返回403；不重试同一拒绝，U09-06仍“远端待验”，由有读取权限的维护者核对规则集与required-check名单。

U09-05和U09-08已满足；80项状态7项已满足、71项代码完成待验收、U09-04进行中、U09-06远端待验。真实Playnite页面、外部UIA、实体键盘/IME、物理DPI/跨屏仍待宿主验收；计算机控制运行时缺路径两次初始化失败，没有发送UI输入。U09-04的U00原始逐帧/累计分配及Playnite呈现数据仍缺。真实恢复、媒体删除和云端写入未运行。

## 2026-11-03 Round5 前一冻结候选34d8a99f历史入口门禁与选中态复核

候选源码 SHA `34d8a99faf7ec827b2fea8303baf8c18a8edbefd`（source tree `9a632fb67999d43ecc1eba689f14f1e7d6c8ed0e`、tests tree `749635df57d0c7ab0a86f8a4a9664a40e85978b0`、scripts tree `efbde3774901e75ad253a2d77cd90f83796e2931`）已按真实入口验证。中文 `GameSaveCenter-一键构建安装运行.cmd` 经英文 `GameSaveCenter-Run.cmd` 完成 SDK 8.0.423 Release build/test/publish/package/install identity/Playnite launch，最终退出码 0；独立 `scripts/build.cmd` 编译和测试职责退出码 0。包 0.6.73 的ZIP/PEXT SHA-256一致，安装 DLL 均带候选 SHA。完整身份与TRX计数见 `docs/design/reviews/ui-polish-round5-20261027/evidence/U09-05-final-gates-20261103/`。

中文链2025项、1967通过、0失败、58 skip；独立build.cmd 2025项、1957通过、0失败、68 skip。build.cmd多出的10项依赖本地Named Pipe，在当次受限进程被既有环境守卫跳过，不计通过；中文完整链同SHA成功运行这些用例。两链共有57项撤销“今日工作台”架构旧断言和1项默认不移动系统指针测试，skip不计通过，未扩大skip。

用户反馈的首页最近活动选框保留蓝色圆角框但不可推移内容。生产修复 `f9d09cb2` 移除了 `TaskItemChrome` 的 `4,2` DIP margin；公共 ListBox 模板内容层独立。R10三态几何测试3/3，R23四种生产表格跨Light/Dark的单元格/文字几何测试通过。其它选择模板盘点显示 DataGridRow margin 仅应用于背景/框线overlay，媒体卡与Overview保护项也不因选择态位移。

Source Validation退出0；本地WPF扫描218 XAML、0 error，含保留.tmp快照的208 warning/1327 info；精确SHA的GitHub Render QA和WPF静态审查步骤成功。GitHub run41477919033(#1434)下基线及SDK10.0.401 job和所有步骤completed/success，3个artifact绑定同一SHA。required-check API先前403，不重复请求，U09-06仍远端待验；Playnite进程启动但未观察插件页面。

U09-05已满足；U09-04/U09-08进行中，U09-06远端待验。其余71项代码完成待真实宿主验收。真实恢复、媒体删除和云端写入未执行。

## 2026-11-02 Round5 U06-02/U08-08 选框移除内容偏移

当前生产源码提交为`f9d09cb2b7665355a74926e2cd4a801567c60baa`（`对齐最近活动选框`），已推送`origin/main`。按用户补充的首页最近活动反馈，Overview专用模板已去掉选中态`TaskItemChrome.Margin=4,2`；蓝色边框/底色和14 DIP圆角仍保留，框线不再造成内容偏移。公共隐式ListBoxItem选择/焦点框原已与内容层分开、Margin=0。现有DataGridRow和媒体卡为独立覆盖层，Overview保护项/ComboBox只变色，不为统一而重写无偏移样式。

`R10RecentAccessBehaviorTests`精确候选3/3；Release完整测试底层脚本直接调用exit0，2025项/1967通过/0失败/58既有skip（57条撤销的“今日工作台”断言与1条系统鼠标指针opt-in，均不计通过），155个WPF类隔离完成。Source Validation exit0；WPF静态审查26 XAML、0错误/26既有warning/164 info；固定时间Light/Dark Render QA 394 PNG、0 PROBLEM、offscreen 1.00逻辑DPI。当前ZIP/PEXT各45,737,931 bytes、SHA-256 `CE8AE7BBD4E49E1215B6151FD10317F02B22A728BCC966E442BE025D1F81BE29`；安装插件程序集身份`0.6.73+f9d09cb2b7665355a74926e2cd4a801567c60baa`，Playnite进程已启动。报告、哈希和截图见Round5 `evidence/U06-02-selection-frame-align-20261102/`。

实际中文root `.cmd`与独立`scripts/build.cmd`仍是U09-05硬门禁：此前在未变化的包装器/脚本版本`0067525f`受PowerShell ConstrainedLanguage dot-source拒绝并exit1；不重复试探该拒绝，当前SHA没有精确入口成功记录。底层脚本exit0不代替cmd结果，因此U09-05、U09-08继续进行中。GitHub精确run41449095659（#1432）completed/success：baseline job125289540888 15/15步、SDK10 job125289540494 8/8步均成功；preview和两份diagnostics artifact均绑定`f9d09cb2`，身份见Round5 evidence。required-check保护接口沿用先前403，不重复查询，U09-06仍远端待验。当前总状态为5项已满足、71项代码完成待验收、U09-04/05/08三项进行中、U09-06一项远端待验。

真实Playnite插件页面、外部UIA、实体键盘/IME和物理DPI未验；本批未执行真实存档恢复、媒体删除或云端写入。当前直接安装脚本在后台启动的Playnite进程不等于宿主视觉验收。

## 2026-11-02 Round5 U06-02/U08-08 整行选中槽位守护

用户反馈首页最近活动被选中后蓝色圆角框仍可保留，但整行不能向右下移动以致图标裁切。生产提交`cb7b78fd`的Overview与共享隐式`ListBoxItem`模板已经将选框与内容分层；本批仅修改`R10RecentAccessBehaviorTests.cs`，在未选中、选中、键盘焦点状态检查图标及整行容器几何，锁定相对ListBox的bounds、Margin和Render/LayoutTransform不变。SDK8.0.423 Release下按仓库WPF隔离方式运行R10类，3/3通过、0失败/skip、exit0；源码校验与diff check通过。本批无生产XAML差异，复用cb7对应静态审查/394图Render QA，未把离屏结果称作Playnite人工验收。

证据位于`docs/design/reviews/ui-polish-round5-20261027/evidence/U06-02-selection-slot-guard-20261102/`，含源/程序集身份、两份TRX、日志、取消/诊断尝试及清理清单。首次还原停滞与并行目标运行在测试启动前取消；单方法通过时发生TestHost COM收尾诊断，最终结论只采用隔离完整类结果。阶段专属`.tmp`已归档后清理，5个输出13,862 bytes；空间值说明见`cleanup.json`。

当前本地分支HEAD为`770a0a417870413976708a70f2052f6fd727ac87`（文档提交），测试源码还有未提交的槽位几何守护；下一步完成Source Validation/链接检查，提交当前候选，再冻结跟踪文件运行中文根入口完整编译/测试/发布/打包/安装与启动、独立`scripts/build.cmd`及双SDK CI。新测试源使U09-05/U09-08重新进行中：轮次5项已满足、71项代码完成待验收、3项进行中（U09-04/05/08）、1项远端待验（U09-06）。U06-02/U08-08仍待Playnite宿主、外部UIA、实体输入/IME和物理DPI验收。

## 2026-11-02 Round5 U09-04 同条件时长性能代理

当前仓库HEAD从`cb7b78fd`起仅增加文档，`src/`、`tests/`、`scripts/`未变。当前预编译RenderHarness SHA `D8EB122A...61736`及生产程序集`E719E3C3...1FF688`的两次60s+30s受控WPF代理运行均exit0：workspace切页布局p95 426.38/411.29ms，详情打开35.69/34.73ms，滚动33.81/31.45ms；U00同为60s+30s代理对应803.59、125.58、36.33ms。合成搜索当前p95较高但每轮仅14样本，不能判断退化。该探针不是物理帧/实体输入测量；托管堆差不是累计分配。原始报告和SDK限制、清理边界见Round5 `evidence/U09-04-u00-aligned-20261102/`。Source Validation exit0，本批267个相对文档链接0缺失；全仓扫描报157条历史缺失链接，主要是旧`.tmp`/`artifacts`路径，未依赖本批临时输出。U09-04继续进行中；无本轮完整构建（仅文档与预编译探针直接运行）。接下来按用户最新反馈给U06-02共享/页面选中态回归补充“容器相对列表槽位不变”断言，保护行整体无位移。

## 2026-11-02 Round5 80项状态表校准与CI身份

当前轮次状态为7项已满足、71项代码完成待验收、U09-04进行中、U09-06远端待验。对文档提交`21ba511c357f9525c617cc8734d1d1afecbb28d9`复核的run `41413816012`（#1428）在`.github/workflows/windows-build.yml`下两项适用job全部通过：baseline job `125172732996` 15/15步，SDK10.0.401 job `125172732641` 8/8步；artifact身份见Round5 `evidence/U09-final-cb7b78fd-20261102/ci-run-41413816012.json`。required-check保护API既有403，未重复请求，仍待有权限维护者只读核实。

## 2026-11-02 Round5 U09-05/U09-08 当前候选门禁结案

生产UI源码为`cb7b78fd514e7d6eaa2a34e341340d5d06610508`；文档冻结候选`0b61ee51ff0d78bf2f0f7c428cf358721753e558`与之源码、测试、脚本、资源树一致。冻结SHA的实际中文根入口与独立`scripts/build.cmd`均在SDK8.0.423 Release退出0；每次2025项/1967通过/0失败/58既有skip，155个WPF隔离类完成。安装包、程序集、日志/TRX哈希见Round5 `evidence/U09-final-cb7b78fd-20261102/local-gates.json`。GitHub run41403537450两个适用job（SDK基线、SDK10.0.401）均success，三artifact身份见同目录`ci-run-41403537450.json`。U09-05/U09-08已满足；总状态7/71/1/1，U09-04仍进行中，U09-06因既有403远端待验。清理仅删除本批专属D盘TEMP目录，细节见`cleanup.json`；真实Playnite页面、外部UIA/物理输入/IME/DPI未验，也未进行真实恢复、媒体删除或云写入。

## 2026-11-02 Round5 U06-02/U08-08 选框几何修复

生产提交`cb7b78fd514e7d6eaa2a34e341340d5d06610508`已推送。首页最近任务的选中圆角框移至独立覆盖层，保留4×2 DIP内缩与蓝色外观；共享隐式ListBox选择/焦点线也不再挤压Presenter内距。真实WPF回归验证图标在未选中/选中/焦点三态坐标不变。SDK8.0.423 Release 0 warning/0 error，R10行为3/3，Source Validation 0，精确SHA Render QA双主题394图/0 PROBLEM。证据：`docs/design/reviews/ui-polish-round5-20261027/evidence/U06-02-selection-overlay-20261102/`。

GitHub run 41392841712 (#1425)对精确cb7b78fd的SDK8基线job 125107766820与SDK10.0.401 job 125107767536均全步骤success；preview artifact 13120801010、基线诊断13119839919、SDK10诊断13118807409均绑定该SHA。活动job日志一度BlobNotFound，终态不受影响。U06-02/U08-08继续代码完成待验收；真实Playnite、外部UIA、物理输入/IME、跨屏DPI待验。U09-05/U09-08已由文档冻结候选完整链结案，计数7/71/1/1；U09-04仍进行中，U09-06仍远端待验。真实恢复、媒体删除、云写入不在本批。

同时补查U06-03同步悬停夹具候选`3ecacdc0dc5376e45a6460b2af2668a1df57f328`的精确run `40647037308`：SDK8.0.423与SDK10.0.401 jobs均success，三artifact身份见Round5 `evidence/U06-03-04/ci-run-40647037308.json`。旧失败仍保留历史，真实输入/宿主未验。

## 2026-11-02 U00-07/U06-04媒体收件箱批次

源码提交64f26f58153efdb4a634bf8a0aa7c832898a581d已推送main。工具栏在1040×700壳层从两行收为一行，低频清空/重置移至原滚动页脚；DataGrid上移76 DIP，四个完整数据行保持可见。SDK8.0.423定向行为218通过/0失败/39既有skip（skip不计通过）；精确提交的GitHub run41041454086中baseline job124014997522与SDK10.0.401 job124014997301均成功，3个artifact均绑定该SHA。Source Validation、生产Views WPF静态审查及post-commit Render QA通过，细节见Round5 evidence/U00-07-U06-04-media-inbox-20261030/。

两个任务仍“代码完成待验收”；真实Playnite、外部UIA、实体输入/IME、物理DPI未验。本次未执行恢复、媒体删除或云端写入。GitHub required-check清单此前读取返回403，不重复探测，仍由有权限维护者只读查询。总状态7/71/1/1；继续独立任务，U09-04仍进行中，U09-06仍远端待验。

## 2026-10-30 U09-04 U00源码重建对照

从U00产品源码提交`90bf0405`重建固定时钟/固定周期RenderHarness，与冻结候选`5c0c57b0`同协议比较；两侧相关Release探针构建及运行退出0。主题应用p95 448.77→325.97 ms，候选主题长尾及活动期AppDomain托管分配改善；内容布局p95 87.52→97.49 ms且两侧均无>300 ms样本。任务DataGrid滚动动作两组10样本对照均显示候选较慢，动作包含`UpdateLayout()`；源差异尚未定位为原因。U09-04继续进行中，不改生产源码。详细证据：`docs/design/reviews/ui-polish-round5-20261027/evidence/U09-04-correlation-20261030/u00-reconstructed-20261030/`。状态7/71/1/1；U09-06仍远端待验。

## 2026-10-30 U09-01 U08-05/06配对更新

冻结主线`5040ac555c3d202b144c4ca9730a87978d139252`的U08-05/06相关测试5/5、0 skip；历史配对截图与Render报告见`docs/design/reviews/ui-polish-round5-20261027/evidence/U09-01-U08-05-06-pair-20261030/BASELINE.md`。U08-05忙碌状态和U08-06维护分组保持代码完成待验收；U09-01、U09-04进行中，U09-06 required-check仍远端待验。5040ac5的run40958239784已completed/success，SDK8/SDK10 jobs及三个artifact均绑定此SHA；required-check名单仍因既有403远端待验。当前状态计数6/71/2/1。旧完整QA的Media/侧栏问题保留，Demo树和真实宿主/实体输入/物理DPI仍待验；未执行真实恢复、媒体删除或云写入。
## 2026-10-30 f8feee81 GitHub CI远端待验

`f8feee813c9aaebecec5ff3f644c8038a599685c`已推送`main`。`windows-build.yml`适用`build`与`sdk10-build`；GitHub连接器HTTP429、公开Actions API未认证速率限制，本机无`gh`，Computer Use kernel初始化失败，所以暂时没有run/job/artifact身份，不能记通过。限流恢复后使用认证API按head SHA查run，再核两job与artifact digest；见`docs/design/reviews/ui-polish-round5-20261027/PROGRESS.md`。

## 2026-10-30 U09-01 U08-03/04历史配对

历史生产快照`c5b03e576edddfe7a5f06172e081407eb9a5ee62`→`18481e6d8d3415ffc31362764980614e76445848`的媒体缩略占位及任务长文本UIA已补浅/深色同数据证据。两侧SDK8.0.423 Release Render QA各394 PNG/0 PROBLEM/exit0；差异4张大窗Media tab1图，390张一致；聚焦卡片探针验证Missing/Video实际文字恢复。任务HelpText不改变像素，后续主线行为基线15/15且目标源文件无后续变化。证据与SHA见`docs/design/reviews/ui-polish-round5-20261027/evidence/U09-01-U08-03-04-pair-20261030/BASELINE.md`。Demo树不在工作树，真实Playnite/外部UIA/实体输入IME/物理DPI未验；U08-03/04仍代码完成待验收，U09-01继续。

## 2026-10-30 U09-01补U08-01/02同数据配对

历史快照`18481e6d8d3415ffc31362764980614e76445848`→`fd2d1316be9ff2d65b0851ac57c45e40064e50ac`的概览与存档页面已完成同数据Light/Dark配对：两侧真实Worker构建成功，Render QA各394图/0 PROBLEM/exit0；固定UTC和运行任务ElapsedSeconds后38图变化、356图一致，after行为回归1/1通过。当前生产子树静态审查26 XAML/0 error/26 warning；Overview两条为标题StackPanel检测启发式误报。Demo-first文档所指Demo树不在本工作树，未声称直接视觉对照。未改生产源码。U08-01/02仍代码完成待验收，U09-01继续盘点其它批次。RenderHarness经验：同时固定假数据UTC与运行任务ElapsedSeconds，并从历史快照构建真实Worker；net472长路径用`.tmp`短junction只改输出目录。证据与清理见`docs/design/reviews/ui-polish-round5-20261027/evidence/U09-01-U08-pair-20261030/`。

## 2026-10-30 U09-04当前源码固定轮次性能补测

当前冻结候选源码等同于`5c0c57b0`，SDK8.0.423 Release临时探针构建exit0。三轮均70周期/297操作；活动托管分配约8.878GB，和e6约8.877GB持平；合成搜索/滚动延迟及工作区中位数接近，但Rendering回调和工作区逻辑布局p95、逐页最大布局样本偏高。旧e6报告相同SampleTimeUtc但`pinned=False`，当前为`pinned=True`；同探针重建e6因便携SDK缺workload locator目录exit1，保留日志，没有换SDK或关闭检查。结果为离屏尾部信号，不能归因于页签模板或动画。U00原始帧/累计分配和真实Playnite/实体输入仍未验，U09-04继续进行。证据在`docs/design/reviews/ui-polish-round5-20261027/evidence/U09-04-currentperf-20261030/`；阶段临时目录仅含本批产物，删除和空间记录见`cleanup.json`。

## 2026-10-30 U04-05～08冻结状态复核

U04-05～08的进度已对齐冻结源`5c0c57b06aed27dc46c41ef3d655ec20dafa5ec7`：夹具修复`c5c86558`与状态实现`4225eb17`均在其祖先中；中文根链、独立`scripts/build.cmd`及run40900374673双SDK jobs均成功，旧Q14失败保留历史。四项仍需真实IME/Playnite UIA、CanExecute取消、窗口失活与长期宿主内存复核。

## 2026-10-30 U04-01～04冻结状态复核

进度行中的旧“待跑”状态已按冻结源修正：`631b218e`、`4225eb17`、`c5c86558`、`388c46ea`均为`5c0c57b06aed27dc46c41ef3d655ec20dafa5ec7`祖先。该SHA的中文完整根链与独立`scripts/build.cmd`均exit0，GitHub run40900374673的SDK8/SDK10 job均success；旧Q14失败记录保留。U04-01～04仍代码完成待验收，真实Playnite/实体输入/物理DPI未验。细节见对应PROGRESS行与U09-final-5c0c57b0证据。

## 2026-10-30 U09-01补U07-07/08历史配对

离屏生产快照对照：设置 `a76de89f`→`c5b03e57`，任务 `17d883ab`→`0aff7f32`。SDK8.0.423 Release四次构建/捕获均exit0；八张Light/Dark前后图已复核，固定任务时钟，验证说明在视口内及UIA HelpText与卡片正文一致。首轮小窗截图裁切且入场动画未收敛，已排除。源/探针/程序集身份和SHA、构建/渲染日志见`docs/design/reviews/ui-polish-round5-20261027/evidence/U07-07-08/historical-render-pair-20261030/`。生产源码未改，U07-07/08仍代码完成待验收；Playnite宿主、外部UIA、实体输入/IME、物理DPI未验，真实恢复/删除/云写入未执行。仅清理已核验的阶段`.tmp/u07-visual-pairs-20261030`目录（2,708文件/125,652,193字节），前后空间与预检见证据`cleanup.json`。当前6项已满足、71项代码完成待验收、2项进行中、1项远端待验。接续U09-01其余历史配对盘点与U09-04独立性能工作。

## 2026-10-30 U09-05冻结门禁通过

5c0c57b06aed27dc46c41ef3d655ec20dafa5ec7 SDK8.0.423；中文包装器完整链exit0、独立scripts/build.cmd exit0。两入口各158 TRX/2020项：1962通过、0失败、58既有skip，skip不计通过；PEXT 45,737,311 bytes，SHA256 `bce032ef9d6c95aa145d0c99a69ee3be16ef7d8dda59e8964db10bc2eb076ee6`，六个包内/安装DLL逐字节相同。Playnite启动PID30324并响应，实际页面未观察。精确SHA的GitHub run40900374673，两项适用job均success，三artifact的head SHA/ID/digest已核。required-check API既有403未重探，仍待有权限维护者查询。soak一次File.Move访问拒绝完整诊断已归档；新门禁通过但旧失败未查明唯一原因。证据：[冻结本地与CI身份](../design/reviews/ui-polish-round5-20261027/evidence/U09-final-5c0c57b0/BASELINE.md)。

Computer Use依仓库Skill使用node_repl/@oai/sky，本轮初始化返回kernel assets路径不存在(os error 3)，此前也出现同错；停止自动化。插件可视页面/外部UIA/物理输入DPI未验，相关项继续标代码完成待验收；真实恢复、媒体删除、云写入未执行。required-check接口下一步为有权限维护者执行`gh api repos/Nikilua/GameSaveCenter/branches/main/protection/required_status_checks`。

## 2026-10-29 U09-01补U04-06/08中断与复用对照

两侧历史生产src逐字节核对未改，SDK8.0.423单节点编译退出0（NU1900网络审计警告保留）。真实披露卡双主题：旧版中途禁用仍保留动画时钟，新版立即回到合法展开角度且无时钟；重新启用不重播。主题重应用前后都替换Transform，因此100次开合按主题完成后的新基准测量，两侧Header/Path/Transform身份均稳定，不虚构复用改善。最新20图及三轮CSV、源码与探针身份已归档；真实取消/窗口失活、Playnite及物理输入DPI仍待验，当前生产源码未改，U09-01保持进行中。

## 2026-10-29 U09 Worker soak失败待定因

2c4ad8c3冻结中文根链在Worker原子移动时访问拒绝，退出1：Core157 pass、Worker400 pass/1 fail，未进入Playnite测试、独立build.cmd及新安装。原夹具已清理且异常缺少路径；同DLL同临时目录定向1/1通过，不能宣称根因已修复。只补路径/轮次/属性/HRESULT诊断，原异常继续抛出，未重试或放宽门禁。补诊断后完整Worker400 pass/1动态Named Pipe受限skip/0 fail，该skip不计通过。生产源码未改；新候选仍需冻结完整链及双SDK CI。当前5已满足、71代码完成待验收、3进行中、1远端待验。

## 2026-10-29 U09渲染帧取样修正候选

392854b3完整根链通过对话框类后，R08首次hover未采到中间值，退出1；局部1445 pass/1 fail/19 skip，独立build.cmd未启动。改用Rendering回调观察实际计算帧，保留原谓词、500/250ms超时及已有fixture时长；finally回收事件/timer，宿主补UIA名称。SDK8类4/4、0 skip，进入/退出/重入2/2/3帧、重置0。本轮未复现旧失败，不宣称已证明唯一根因；生产源码与392/388相同。新候选仍须独立冻结全链及双SDK CI，旧失败不擦除。当前5已满足、71代码完成待验收、3进行中、1远端待验。

## 2026-10-29 U09对话框计时断言修正候选

388c46ea中文根链退出1，失败于R08对话框进入动画的UTC耗时下限；局部1440 pass/1 fail/19 skip（73/155 WPF类），独立build.cmd未启动。原失败缺少实际耗时，未确定唯一根因。只把测试经过时间/1500ms期限改用Stopwatch，保留100ms下限与所有断言/skip，增加1秒资源校验和双时钟诊断；为宿主补UIA名称。最终类3/3、0 skip。生产源码与388相同，不重做已有394图Render QA；新测试候选仍须提交后冻结全链及双SDK CI，不以旧通过或定向通过冒充。当前5已满足、71代码完成待验收、3进行中、1远端待验。

## 2026-10-29 U04页签提示条避让与状态保持候选

U09-01补齐U04-01/04/07原始生产快照的同数据双主题图与实际时间采样，发现提示条覆盖标题；当前e6基线的共同坐标几何断言也准确失败。共享Redesign模板将提示条移入既有左留白，保留旧7×16 DIP测量下限，内部页签也保留至少3 DIP读区间距；禁用0.42透明度改在共同视觉根层生效，保持旧整体变暗而不令提示条全亮。新增状态回归先捕获初版透明度偏差，再修正。最终聚焦2/2、完整R23类12/12、0 skip；源码校验通过，生产26 XAML静态0 error，最新Render QA394图/0 PROBLEM/退出0。前后样图已复核。本候选仍未提交冻结及跑自身完整门禁，不能以e6通过替代。

此前e6e7f3b9冻结中文根链和独立build.cmd均退出0，各1961 pass/58既有skip；六个安装DLL与PEXT字节一致，Playnite新进程及加载日志已核。run40851428895的SDK8 job123371589585及SDK10 job123371590197均success，三artifact同SHA与digest已归档。两轮重建补测分配约8.88GB，比未优化后快照低29.1%，比历史前快照低20.2%；不代表实体输入/呈现帧，也不替代缺失的U00原始基线。旧Q06间歇失败仍不作唯一根因断言。

新候选提交后必须重新冻结跑中文根链、独立build.cmd与双SDK CI；真实宿主/物理输入DPI及required-check既有403边界保留，不重复权限拒绝。当前5已满足、71代码完成待验收、3进行中、1远端待验；U09-01其它批次配对、U09-04实际宿主性能和U09-05新源门禁仍未结项。已删除470张本阶段旧PNG，保留当前394图、有效配对、全部TRX/日志及未知文件，哈希/字节/空间清单已归档。

## 2026-10-29 U09主题资源发布优化候选

性能拆分确认主题重应用是主成本（前10.572GB、后11.754GB）；新增DynamicResource回归旧代码4/4失败，证实每次2～3次重复通知。生产管线改为无宿主字典归并后逐键发布最终值；修复首版漏空值导致的高对比阴影回归，七个可空效果键显式保留。最终五类150 pass/0 fail，39既有撤销架构skip；源码与WPF静态通过（0 error），补建真实Worker后render-qa退出0、394图、0 PROBLEM。缺构建身份/缺Worker和首版高对比失败均归档，没有改断言或门禁。此为候选，尚未性能复测或冻结全链/双SDK CI；U09-04/05保持进行中，Q06间歇失败不宣称已修复。当前5已满足、71代码完成待验收、3进行中、1远端待验。

## 2026-10-29 U09性能重建补测发现分配回退

固定Fake时钟的历史0447237a→acebe07d生产快照，双主题七页1040×700 DIP、70周期297操作、30秒settle，前/后/后/前四轮及一对逐页诊断均退出0。后快照活动期托管分配约12.52GB，前约11.12GB，稳定增加约12.58%；七页都增加，任务页增量最大。U09-04仍进行中，下一步拆分主题资源重应用与布局成本；不能把Rendering回调间隔当物理呈现帧、把重建数据充当原始U00或把退出0充当性能通过。当前生产源码未改。Q06三次定向诊断通过但未复现旧失败，U09-05仍进行中；不追绿重跑整链。当前5已满足、71代码完成待验收、3进行中、1远端待验。

## 2026-10-29 Q06按压恢复待诊断

2d248548中文根链退出1：已通过媒体滚动类，但第30类Q06按钮五态的按压恢复断言失败；部分1321项为1302 pass/1 fail/18 skip，独立build.cmd未启动。失败TRX和根诊断已归档。仅给原断言补状态、缩放、覆盖层、焦点、窗口激活信息，所有谓词不变；一次有明确目的的定向复现通过，但未复现原失败，不能视为修复或完整链通过。下一步区分合成Space、原生捕获与焦点干扰，不无依据重跑整链。U09-05仍进行中，源码/完整链/CI门禁均不以旧通过替代。

## 2026-10-29 U09-02/03媒体STA清理候选

f1eb8f8f在C盘真实中文根链退出1：MediaInboxScrollBehaviorTests的WPF TextStore窗口移动回调跨STA访问导致宿主崩溃。该类没有发布用例结果，Sequence标记outputScale1.25未完成；已完成部分1227 pass/18 skip不能代表完整通过，独立build.cmd未启动。诊断及Sequence已用短路径归档。

新增保留IME启用的真实TextBox生命周期回归，旧夹具因Dispatcher未关闭准确失败。仅在共享RunSta finally中于所有者线程InvokeShutdown，清理异常亦返回失败，未关闭IME或改滚动/锚点/虚拟化断言、三种输出缩放或skip。按正式隔离配置连续三轮7/7，共21/21、0 skip。缺失清理已被直接证明，旧崩溃唯一因果不作过度归因；新候选仍须中文根链、独立build.cmd及双SDK CI，U09-05继续进行中。

## 2026-10-29 U09归档路径兼容修正

隔离工作树首次检出因两批归档路径过长失败：长WPF类名在目录及TRX文件名重复。已改为每个run内trx/短编号.trx，清单保留原路径、原始SHA、旧副本SHA及新副本SHA；逐份校验测试名称/结果相同，文本仅归一换行及行尾空白。未修改Git全局配置、源码、workflow或SDK。首次检出失败不记构建通过；下一候选仍需完整链与双SDK CI。旧清理权限拒绝未重试，旧构建发现测试数据库后保留未删。

## 2026-10-29 U04-02/03采样截止修正候选

acebe07d中文根链SDK8.0.423退出0，157份TRX为1955 pass/58既有skip，六个安装DLL与PEXT逐字节相同；新宿主进程68964及14:07:12日志确认加载GameSaveCenter0.6.73，但未验真实页面或运行时模块SHA。独立build.cmd在第75类R08 hover重新进入未采到中间帧失败（局部1463项：1443 pass/1 fail/19 skip），完整交付门禁未通过；原始诊断归档，未追绿重跑旧链。

新增有限Render队列复现：旧1ms采样截止等到40次Render全部结束才返回，准确失败。仅将测试采样截止计时器改为Send优先级，保留生产时长、所有原断言、超时和skip。三轮直接运行12/12断言通过，但额外Test AppDomain有既有COM卸载异常，日志保留、不记为无异常门禁；按仓库正式DisableAppDomain=true复核4/4、0 skip且无该异常。此复现证明采样器可被饿死，旧hover失败未记录迟到耗时，不宣称回溯了唯一根因。

生产资源未改。新候选提交后须重新完整构建安装链、独立build.cmd和双SDK CI；D盘空间不足时以同SHA的C盘隔离工作树执行，不降低空间预算或替换SDK，不重试旧权限拒绝。当前5项已满足、71项代码完成待验收、3项进行中、1项远端待验；U09-05重新进行中，U09-01其它配对及U09-04重建补测继续。required-check名单仍按既有403远端待验，不复探。

## 2026-10-29 U03-04禁用原生文字修正候选

清理说明：当前保留SDK位于 .tmp/round5-u09-01-03/dotnet-sdk8；已删除逐文件SHA相同的旧SDK副本、四份已由cd062531构建1a593ccc替代的Round5输出及四份过期渲染副本。旧文字中的当前输出仅代表当时状态，现以本条和清理清单为准。原始失败诊断、最新包/构建与当前审计继续保留；D盘由约2.32GB恢复至4.33GB，未降低门禁。

历史同状态对照发现共享TextBox只改变PART_ContentHost附加Foreground，原生编辑文字仍读取控件Foreground；新增实际属性断言在修改前准确失败（预期#FF9AA4B5、实际#FFF2F4F8）。共享模板改为设置TextBox本身Foreground，沿原TemplateBinding传给内容宿主；补重新启用恢复主文字色断言，错误描边/HelpText和尺寸语义保留。

SDK8.0.423 Release定向8/8、0 skip；首次扩展运行因未传构建身份导致6个源码测试被门禁拦截，补真实HEAD元数据后通过，失败TRX保留。双主题同状态图片已检查；源码校验、XAML26/26、WPF静态42文件/0 errors（既有26 warning/179 info）及完整Render QA394 PNG/0 PROBLEM/exit0通过。当前为源码候选；提交冻结后还须本候选中文根入口完整链、独立build.cmd及双SDK CI，不能沿用cd062531通过结果。真实Playnite、实体输入/IME/UIA及物理DPI继续待验。

## 2026-10-29 U09-01补U03-04/05输入状态对照

重建f59fe4c8→d1fc1308（实际父88e1e8f3与f59生产src树相同），Fake固定UTC且源码相同，src逐文件与ZIP核对未改。SDK8.0.423 Release Worker/Harness及探针重编译均0警告0错误，两侧原RunThemeQa补充矩阵0 problems/exit0；双主题六种输入组合尺寸均480×42，After错误HelpText可读，输入探针两版均exit0。v1说明文字资源键错误导致标签不可见，v2已修正探针并保留首轮诊断；两侧双主题状态图及任务/媒体搜索页图已保存。

视觉复核发现禁用态原生文字仍接近启用态主文字色；现有“PART_ContentHost附加Foreground”断言不能证明原生编辑文字实际前景色。本批只补历史证据，不将这一点记为验收通过，下一批核查当前共享TextBox。未改当前生产源码；真实Playnite、实体输入/IME/UIA及物理DPI仍待验，U09-01保持进行中。

## 2026-10-29 U09-01补U02-05/06圆角与线宽对照

以已归档504562a7为前图（与实际父提交3f710530的src树相同），重建417bed51圆角/裁切与f59fe4c8线宽令牌两侧，分别保存两个变化边界。Fake源码无差异，时钟固定为2026-10-28T09:00:00Z；ZIP注释/哈希及src逐文件核对证明生产源码未改。两侧SDK8.0.423 Release Worker/Harness编译均0警告0错误，原RunThemeQa补充矩阵和四次双主题finesseprobe均exit0，矩阵0 problems。共享控件双主题图已视觉检查；线宽令牌收敛允许终态外观相同，不虚构视觉差异。

本批只补历史静态证据，不改当前生产资源，不冒充完整Render QA、安装链或物理DPI验收。U02-07/08原批只有专项复核、无生产资源变化，继续引用其原行为证据。U09-01仍进行中，真实Playnite、实体输入/IME/UIA、U00性能原始基线及required-check权限边界保留。

## 2026-10-29 U09-01补U02-01～04主题/状态对照

原始813adb09→504562a7源码快照，Fake时间固定2026-10-28T09:00:00Z；实际父提交4448b718与813的src树完全相同。ZIP注释/哈希及src逐文件核对证明生产源码未改。两侧SDK8.0.423 Release编译0警告0错误，调用原RunThemeQa均exit0/0 problems，After双主题finesseprobe均exit0。此为补充主题矩阵，不冒充新完整394图Render QA或安装链。

生产ListBox模板/容器样式的受控投影完整展示六状态，两主题共12行均实现；After图标前景色与对应语义资源一致。真实“最近访问”按钮验证卡片Visible、任务卡片Collapsed（实际是卡片切换，不是浮层）；两侧同数据卡片图展示普通图标由强调色改为次级文字色，命令为无业务副作用的夹具。初轮整页夹具未填充无关摘要，因此最终只捕获实际卡片，初轮状态CSV作为方法诊断保留。六状态和最近访问双主题均已视觉检查。

历史“今日工作台”只存在原始源码对照，当前生产架构未回退。当前生产源码未改，物理DPI、高对比实机、Playnite、实体输入/IME/UIA及真实恢复/媒体删除/云端写入未验；不据此宣称性能验收。U09-01其它视觉批次仍缺配对，保持进行中。

## 2026-10-29 U09-01补U01四项历史对照

重建0447237a→7e61bba3→813adb09三个源码快照，分别补U01-01/02标题/混排及U01-05/06禁用文字/图标批次。Fake时间同为2026-10-28T09:00:00Z，生产wall-clock未改；src逐文件与原始ZIP核对未改。三侧SDK8.0.423 Release Worker/Harness编译均0警告0错误，各394 PNG；标题和禁用文字两侧完整Render QA exit0。最早Before因Sidebar rapid-toggle未收敛exit1，失败报告保留，未重跑追绿；它只用于静态前图，不记整体验证通过。

六次原有finesseprobe双主题exit0；Shell标题和控件禁用态样图已视觉检查。42个页面/控件主题引用以内容哈希去重保存，包含存档、任务、媒体、工具、通知及Shell；两组变更边界分开记录。仓库Git报告会读取父仓库，历史身份以ZIP注释/哈希及源码核对证明。本批不改当前生产源码，不新增真实恢复/媒体删除/云写入；实际Playnite、物理DPI、缺失可选字体、实体输入/IME/UIA仍待验。U09-01其它视觉批次对照仍缺，保持进行中。

## 2026-10-29 U09-01补U06-07/08候选表配对

原始3ecacdc0→ac0afe72隔离快照，固定Fake数据UTC为2026-10-28T09:00:00Z；ZIP注释/哈希及src逐文件核对证明生产源码未改。SDK8.0.423 Release两侧编译0警告0错误，补充截图探针exit0。复用历史RenderTabs/SavePng路径，保存1040×700及3840×2160逻辑窗口双主题候选页前后配对；实际内容744×460与3544×1920。两主题窄窗表高310 DIP不变，高窗由560扩展至1664.67 DIP，8条同样本候选。主题、表格、详情和滚动区域已视觉检查。

首两轮原生Window探针虽然尺寸断言通过，但图片有未呈现区域，因此不作为验收图；诊断CSV/示例图保留。复用原始独立Grid捕获后完整呈现，具体原生窗口/DPI根因未证明。本批只补静态候选页对照，不冒充新的完整394图Render QA、大列表性能或真实Playnite/物理4K验证。U06-08行为证据仍按原批次范围引用；U09-01其它批次配对仍缺，继续进行中。

## 2026-10-29 U05-01/02动态时间采样补齐

在原始前后源码隔离快照中加入同一诊断探针，通过生产NavigateTo和详情按钮采样；逐文件核对src/与原始git archive完全一致，未修改生产动画时长。两侧SDK8.0.423 Release编译0警告0错误、探针双主题exit0。CSV分开记录同步action_ms和返回后实际时间；调度目标不等于实际时间，包含布局/截图开销，不用于p95、长帧或累计分配验收。v1/v2方法诊断和首次页面进入数据保留。

After双主题缓存页及详情进/退均采到中间透明度/位移，1600ms时归一并释放时钟；详情退出可见期间关闭命中，随后折叠。首入浅色页面因布局开销未充分采到中间帧，不隐去。已检查缓存页和详情退出指定时点截图。U09-01中的U05-01/02已具有同数据双主题静态配对及动态时间样本，整项仍因其它批次配对缺口进行中；真实Playnite、实体输入、物理DPI仍待验。

## 2026-10-29 U09-01补U05-01/02历史配对

从原始提交48f69413与237e993f重建隔离快照，ZIP注释SHA与文件哈希证明源码身份；仅Fake夹具时钟固定为2026-10-28T09:00:00Z，生产资源未改。两侧SDK8.0.423 Release Worker/Harness编译均0警告0错误，完整Render各394 PNG、0 PROBLEM、exit0。1040×700双主题任务页前后文件SHA相同，已视觉检查；每主题仅存一份图并由两侧引用。历史报告Commit字段沿父目录读取当前仓库，不能作为历史源码身份，详见新证据pair.json。

首次快照渲染因未先编译Worker.exe导致4项SettingsState失败；补齐真实历史Worker编译依赖后通过，未改断言或校验。保留失败报告。该证据只补静态终态配对，不证明页面/详情动画中间状态；U09-01仍进行中，下一步补U05时间采样。历史快照仍在使用，后续归档核对后清理冗余渲染目录；未执行真实恢复/媒体删除/云端写入。

## 2026-10-29 通知来源批次产物清理

核对321份归档SHA全部一致后，仅删除本批`.tmp/round5-u07-05-source-20261029/`下10个明确目录，共323,762,007 bytes；路径边界、重解析链接及独占打开检查均通过，D盘前后实测增加330,719,232 bytes。保留标准根构建、根诊断、最新包、SDK及其它任务TRX/TestResults；清单见Round5通知来源证据`final-gates/cleanup.json`。U07-05代码完成待验收，U09-01配对/U09-04基线/U09-06保护名单边界继续保留。

## 2026-10-29 U07-05冻结候选门禁

源码提交 `cd06253158c0e35d43de84e037548a764629749a` 已推送main。SDK8.0.423 Release中文根入口完整构建、测试、发布、打包、安装身份核验及启动调用退出0（run `20261028T235825476Z-d43f8012`）；独立 `scripts/build.cmd` 规范输出树全套退出0。两次各2013项、1955通过、0失败、58个既有skip（57个撤销架构断言、1个opt-in系统指针测试），skip不计通过。

GitHub run [40794111982](https://github.com/Nikilua/GameSaveCenter/actions/runs/40794111982)的baseline job `123186245250` 与SDK10.0.401 job `123186245559`均成功；baseline源码、诊断自测、SDK基线负例、构建测试、Render QA、WPF静态、打包、上传均成功。artifact IDs为`12865861325`（baseline诊断）、`12865448641`（preview包）、`12863324535`（SDK10诊断），digest见本批证据 `final-gates/ci.json`。保护required-check名单此前403，本批未复探，U09-06仍远端待验。

首次受限build.cmd多出10项Named Pipe/Worker环境skip；提权临时输出树一次Q10复选框合成空格事件2项失败。保留失败TRX，未修改断言或skip。规范输出树完整复跑通过，但根因尚未证明，不能将路径认定为已证实原因。根入口完成启动调用，后续进程查询未见Playnite，因此持续运行、插件已加载及真实页面视觉仍待验；外部UIA、实体IME/键盘、物理DPI未验，真实存档恢复/媒体删除/云端写入未执行。

U07-05更新为代码完成待验收；U09-01仍需其它视觉批次同数据双主题配对，U09-04仍缺U00逐帧/累计分配基线。当前计数6项已满足、71项代码完成待验收、2项进行中、1项远端待验。本批临时产物已归档核验并清理，见最新清理记录。

## 2026-10-29 Round5 U07-05通知来源局部候选

当前源码工作树加入显式通知来源字段：任务来源来自稳定 `TaskTypeDisplay`，会话摘要来源来自会话事件；SQLite加列兼容旧表，旧数据保留空来源并显示“来源未记录”。列表标题下增加来源行、详情增加来源字段，通知排序/复制/UIA/虚拟化/焦点/滚动系统保留。

SDK8.0.423 Release定向Worker 13/13、Playnite 7/7，无skip；源码检查通过、XAML结构26/26、WPF静态42 XAML/0 errors/26 warnings/179 info；双主题Render QA 394 PNG、0 PROBLEM。实现和对照证据在Round5 `evidence/U07-05-notification-source-20261029/`。当前候选尚未提交与冻结；随后须按真实中文入口跑完整构建/测试/发布/打包/安装身份核验/启动，并单独验证 `scripts/build.cmd`，推送后查询baseline和SDK10 CI全部job/artifact。

真实Playnite来源数据刷新、外部UIA、实体IME/键盘及物理DPI未验。其他Round5边界保持：U09-01仍缺其它视觉批次配对，U09-04缺U00逐帧/累计分配基线，U09-06保护required-check名单权限此前403，按用户要求不复探；真实存档恢复/媒体删除/云端写入未执行。

## 2026-10-29 Round5 U09-05/06/07/08 最终候选门禁

源码候选 `5f0eb6209ccfb33c67efac36aee68aee204de614` 已推送。测试专用修正避免键盘/UIA列菜单用例触发无关的 `StylusWisp` Popup 捕获崩溃；生产程序集、XAML、资源与业务命令未改。中文根入口完整Release构建/测试/发布/打包/安装身份/启动及独立 `scripts/build.cmd` 均退出0。两次SDK8.0.423运行各2012项、1954通过、0失败、58个明确skip（57个已撤销“今日工作台”断言、1个默认opt-in系统指针测试）；不将skip计为通过。

精确SHA的 GitHub run `40768381673` / #1374 success；baseline job `123098598311` 和 SDK10.0.401 job `123098597735` 均成功，Render QA/WPF静态/包与上传全过。当前包SHA-256 `F80780FE18D890610C10B881D1B21D7D9967CE7EFFEBF265B3EDF3DB4D514DBD`，安装主插件DLL SHA-256 `220BE2409571A14902F1350BBD48655950EF974C94E857D1C7CE5E1EB0859F02`；Playnite PID42296已启动。证据见[U09-05最终候选](../design/reviews/ui-polish-round5-20261027/evidence/U09-05-final-5f0eb620/BASELINE.md)。这不代表真实页面视觉、实体输入/IME、外部UIA、物理DPI或真实存档/媒体/云端业务已验。

本地阶段临时构建/测试/离屏图已安全清理，当前文件与前后空间见同目录 `cleanup.json`。保留 `.tmp/round5-u09-01-03/dotnet-sdk8` 及安装脚本，因为系统只装SDK9.0.302、global.json要求8.0.423；剩余阶段失败TRX已脱敏归档。required-check/branch protection读取先前403，未重复探测，故U09-06仅对保护规则名单标“远端待验”。80项复核：6项已满足、71项代码完成待验收、U09-01/U09-04进行中、U09-06远端待验；U09-08审查已完成但不代表整轮80项全验收。详见Round5 `PROGRESS.md`。

## 2026-10-29 Round5 U07-07/08 恢复说明批

推送源码提交 `0aff7f32aab4fb31940fd791c1486c67d36ebdbd`。SDK8.0.423 Release精确提交定向10/10、0 skip；源码校验通过，WPF静态42 XAML/0错误/26既有warning/179 info。固定UTC双主题Render QA候选394 PNG/0 PROBLEM/exit0；其报告记录前一HEAD `17d883ab` 和dirty候选，精确SHA渲染由当前CI覆盖。run `40749256118` baseline job `123031214603`与SDK10 job `123031215214`查询时均进行中、暂无artifact；前一个17d run `40744264627`的SDK10 job成功/artifact `12843205835`，baseline仍在打包。required-checks曾403，权限与保护状态继续标“远端待验”。U07-07/08更新为代码完成待验收；证据在Round5 `evidence/U07-07-08-RECOVERY/BASELINE.md`。真实Playnite、外部UIA、实体键盘/IME和物理DPI未验，未执行真实恢复/媒体删除/云端写入。

## 2026-10-29 Round5 U08-05/06 当前工作树

工具页为启动请求新增局部 busy+状态文本，在 `finally` 还原；原命令、风险与进程跟踪门控保持。维护诊断页新增1160×520 DIP真实WPF窗口的摘要底部滚动、完整长摘要内部滚动和复制命令可达回归。SDK8.0.423 Release 5/5、0 skip；source validation通过；WPF静态42 XAML/0错误/26 warning/179 info；Render QA 394 PNG/0 PROBLEM。真实Playnite、工具实际启动、Narrator/外部UIA/物理DPI及本批新SHA CI待验。证据 `docs/design/reviews/ui-polish-round5-20261027/evidence/U08-05-06-current-main-d6d940e7/BASELINE.md`。

## 2026-10-29 Round5 U08-03/04 当前主线复核

生产源码基线`8ac477b9fad108d714e94a7da202b5be22ef6b30`：任务页状态替换/阶段进度/长文本UIA及稳定行高新增真实WPF视图回归；媒体页占位各状态恢复、虚拟列表滚动锚点和选中焦点边界覆盖。SDK8.0.423 Release组合15/15、0 skip，source validation退出0，WPF静态42 XAML/0错误/26既有warning/179 info。仅改测试与review证据，无生产XAML/资源变化。两项标为代码完成待验收；Playnite、实体UIA/输入、物理DPI与本批新SHA CI待验。证据见`docs/design/reviews/ui-polish-round5-20261027/evidence/U08-03-04-current-main-8ac477b9/BASELINE.md`。完整TRX SHA-256 `316B0C4A11F464BFB0F21B98DF3D3E3A04393BC251F1648E788832606024D1D8`。

## 2026-10-29 Round5 U07-05/U08-07 焦点补充

通知清除筛选焦点回归现在以空态实际可见的生产WPF窗口断言按钮到搜索框焦点交接；到达通知不抢焦点/不替换旧选中行继续覆盖。SDK8.0.423通知视图类4/4、0 skip；source validation exit0；WPF静态42 XAML/0错误/26既有warning/179 info。仅修改测试，无生产XAML和资源差异。首次失败为测试夹具把无结果空态折叠，TRX已作为诊断归档。U08-07代码完成待验收；真实Playnite导航、Worker刷新、物理输入和UIA待验。证据见Round5 `evidence/U07-05-08/BASELINE.md`。

## 2026-10-29 Round5 U07-04/05 补充验证

在 `a89b29db` 后检查真实列选择菜单的稳定次序、必需列UIA说明、可用项以及ItemsPresenter Cycle配置，SDK8.0.423定向5/5；真实通知收件箱在列表首部加入新项不抢搜索焦点并保持旧项选中，定向4/4。新增测试与证据，不改生产XAML/资源/DTO。程序化Popup焦点探针失败是因为调用Focus仍保留在拥有按钮，原TRX已存档，不作为产品缺陷结论。DTO无来源字段，不推断来源，U07-05保持进行中；物理键盘、Playnite、真实数据刷新待验。详见Round5 `evidence/U07-04-05/BASELINE.md`。

## 2026-10-29 Round5 U07-03/06 补充行为覆盖

在已推送生产基线 `842557f6` 上新增 Tooltip 屏幕工作区底边实际翻转与 Toast 五条上限/最新优先/退出中不重排行为测试；本批仅改测试，不改生产视觉资源。SDK8.0.423 隔离 WPF 定向组6通过、0失败、1项既有系统指针 opt-in 跳过；源码校验通过，WPF静态审查42 XAML/0错误/26既有warning/179 info。TRX与SHA见 `docs/design/reviews/ui-polish-round5-20261027/evidence/U07-03-06/BASELINE.md`。当前测试与文档工作树未提交，双SDK CI待精确提交后查询；实体IME、混合DPI跨屏和Playnite宿主未验。

## 2026-10-28 Round5 U09-01/02/03 当前回归审查

当前生产源码基线 `d4c4357c4e0c256b4fe0248d7efc25ce5badfd18`。U09 Render QA 报告为双主题×11个窗口尺寸、394 PNG、0 PROBLEM、逻辑DPI 1.00；当前存档页Light/Dark 1040×700图已保留。U00基线与当前图使用运行时UTC假时间且页文案/工具栏已变化，不能当作同数据前后图；逐批前后截图缺口仍在。

SDK8.0.423键盘/焦点/UIA候选组合16/16、0 skip；修复R07测试夹具缺少SaveTabIndex导致页面未选中的问题，未动生产页面或业务命令。对MoveFocus合成遍历、控件AutomationPeer的测试不等于实体键盘、IME、外部UIA客户端或Playnite宿主验收。精确修复提交和双SDKCI待推送/核对，CI前一SHA `d4c4357c` 的baseline/SDK10 job都因该夹具问题失败，原始身份见Round5 `evidence/U09-01-03/ci-failure.json`。

证据：[U09-01/02/03](../design/reviews/ui-polish-round5-20261027/evidence/U09-01-03/BASELINE.md)。接续先提交并精确SHA重跑，再推进同数据截图夹具与U09-04动画测量；不等待旧S/T宿主或权限状态。完整中文安装启动链和独立`scripts/build.cmd`仍是整轮最终门禁。

## 2026-10-28 Round5 U08-07/08 局部推进

基线 `9181700f`：通知筛选无结果增加清除搜索/级别/时间并回焦搜索的入口，空态文字复用共享标题与说明样式，保留已读和导航命令。八类页面完成共享资源引用首轮盘点；静态计数不能替代状态和视觉比较。SDK8.0.423 定向 15/15、0 skip；源码/XAML/WPF 静态及双主题 394 图离屏回归通过，基于未提交工作树。真实跳转、键盘/UIA、跨页逐控件及宿主待验，两项仍在进行；见 `docs/design/reviews/ui-polish-round5-20261027/evidence/U08-07-08/BASELINE.md`。新提交 CI 和整轮完整构建安装启动待验。

## 2026-10-28 Round5 U08-05/06 与概览 CI 断言修正

基线 `fd2d1316`：工具进程不可追踪时允许禁用控件显示原因并提供 UIA；维护诊断复制入口说明复制完整摘要。用户规则和命令不变，两项仍在进行，来源/启动、细节展开及短窗宿主待验。前一提交 run `40657403310` 双 SDK 因概览旧眉题断言失败，已按控件名验证新文字并保留其他断言；run/job/artifact 身份和原始诊断见 `docs/design/reviews/ui-polish-round5-20261027/evidence/U08-05-06/ci-failure.json`。SDK8 定向组合 48/48、0 skip，源码/XAML/WPF 静态及双主题 394 图离屏通过，属于未提交工作树；证据见 `docs/design/reviews/ui-polish-round5-20261027/evidence/U08-05-06/BASELINE.md`。修复提交 CI 与整轮完整构建安装启动待验。

## 2026-10-28 Round5 U08-01/02 局部推进

基线 `18481e6d`：概览优先状态眉题由固定“待处理”改为“当前状态”，存档历史摘要增加当前游戏名称和完整身份提示；原命令与恢复前置检查保留。SDK8.0.423 组合 31 通过、1 既有旧架构 skip、0 失败；源码/XAML/WPF 静态和双主题 394 图离屏回归通过，基于未提交工作树。三次夹具绑定失败与修正留存；整页阅读路径、窄窗及真实宿主待验，两项仍进行中。详见 `docs/design/reviews/ui-polish-round5-20261027/evidence/U08-01-02/BASELINE.md`。新提交 CI 及整轮完整构建安装启动待验。

## 2026-10-28 Round5 U08-03/04 局部推进

基线 `c5b03e57`：任务长文本的 UIA HelpText 保留完整内容；修复媒体预览占位文字属性未连接到实际 TextBlock，缺图/损坏/视频等状态不再空白。原固定槽、裁切、异步解码、任务行高和虚拟化保留。SDK8.0.423 定向 6/6、0 skip；源码/XAML/WPF 静态与双主题 394 图离屏渲染通过，基于未提交工作树。运行中行高、媒体选中边界及真实宿主待验，两项仍在进行；证据见 `docs/design/reviews/ui-polish-round5-20261027/evidence/U08-03-04/BASELINE.md`。新提交 CI 和整轮完整构建安装启动待验。

## 2026-10-28 Round5 U07-07/08 局部推进

基线 `a76de89f`：巡检间隔旁新增开关依赖与关闭保留值说明；任务重试提示明确按任务类型和对象提交，并提醒确认上次结果。保留设置错误定位、取消和现有命令。SDK8.0.423 定向 11/11、0 skip；源码/XAML/WPF 静态和双主题 394 图离屏回归通过，均为未提交工作树。其他设置依赖、权限/离线/未知结果分类和真实宿主待验，两项仍在进行；见 `docs/design/reviews/ui-polish-round5-20261027/evidence/U07-07-08/BASELINE.md`。新提交 CI 及整轮完整构建安装启动待验。

## 2026-10-28 Round5 U07-05/06 局部推进

基线 `7c4d0775`：通知详情增加相对时间；Toast 非空摘要或详情可完整查看，焦点在 Toast 内时暂停自动关闭，并给关闭/详情按钮增加 UIA 名称。原四条上限、退出动画、虚拟化表格和业务安全语义保留。SDK8.0.423 定向 5/5、0 skip；源码/XAML/WPF 静态检查与双主题 394 图离屏渲染通过，均为未提交工作树验证。多条覆盖及退出重排、来源契约、真实宿主/物理输入仍待验，两项保持进行中。见 `docs/design/reviews/ui-polish-round5-20261027/evidence/U07-05-06/BASELINE.md`；新提交 CI 和整轮完整入口链未完成。

## 2026-10-28 Round5 U07-03/04 局部推进

基线 `2aba614f`：生产文本编辑器的默认提示改为锚定控件下方，保留显式位置；表格列菜单必要列的禁用原因同时提供鼠标提示及原有 UIA HelpText。两项仍为进行中：屏幕边缘、真实 IME、弹出层宿主和快捷键/图标/子菜单及键盘路径未完成核验。SDK8.0.423 Release 5/5、0 skip，源码/XAML/WPF 静态检查和双主题 394 图离屏渲染通过；工作树渲染不是精确提交证据。前后、结果和清理边界见 `docs/design/reviews/ui-polish-round5-20261027/evidence/U07-03-04/BASELINE.md`。最终完整入口构建安装启动及双 SDK CI 仍待验。

## Round5 U04-06/08 最新本地状态（2026-10-28）

当前 `main` 已推送源码提交 `c5c86558b6232995438657c00810aeb327ed9ba5`。共享 DisclosureCard 在 Chevron 展开动画中途被禁用时移除旧 Storyboard 时钟，保持 IsExpanded 业务值，并以 IsChecked 绑定恢复合法终态；重新启用不回放旧动画。生命周期测试在同一生产实例完成100次开合，Header/Path/RotateTransform 对象身份不变，Reduced Motion移除最终时钟。Q14测试夹具现在在加载后通知选中预设变化，修复run `40578426793` 两SDK的初始化时序失败。

SDK8.0.423 精确提交 Release 聚焦测试13/13、0失败、0跳过。Source validation、XAML26/26、WPF静态37 XAML/0错误、RenderHarness Release 0 warning/0 error、双主题Render QA 394 PNG/0 PROBLEM均通过；TRX、哈希和限制见 `../design/reviews/ui-polish-round5-20261027/evidence/U04-06-08/BASELINE.md`。提交run `40583173450` 的 baseline job `122467678548` 与 SDK10 job `122467678982`目前在测试，尚无终态/artifact；状态见 `../design/reviews/ui-polish-round5-20261027/evidence/U03-08-U04-01-02/ci-runs.json`。required-checks读取曾返回403，远端额外保护仍不推断。

代码级别未直接驱动取消命令、真实窗口失活、Playnite宿主、UIA客户端或系统Reduced Motion设置；U04-06/08保持“代码完成待验收”。本批 `Remove-Item -LiteralPath -Recurse -Force` 在执行策略处被拒绝，未换命令清理；准确自有路径、磁盘快照与拒绝记录见 `../design/reviews/ui-polish-round5-20261027/evidence/U04-06-08/cleanup.json`。下一批推进U05页面/详情过渡，不等待旧S/T宿主或权限状态；整轮最终仍需中文根入口完整构建/测试/发布/打包/安装身份/启动和独立 `scripts/build.cmd` 验证。

## Round5 U04-03/04/05/07 最新本地状态（2026-10-28）

当前 `main` 为 `4225eb17139a844ede87219f02a9ad97e88a12de`。U04-04在共享工作区Tab模板加3×16 DIP选中提示条，以共享`GscMotionNormal`渐入/退出并在反向切换时互相移除故事板；焦点环/选中底色/布局分开。U04-03与U04-05复用R08/Q06/R09及GamePicker既有行为，本轮修正测试夹具跨STA全局Application依赖；U04-07在关闭动态效果时直接设终态并清除动画。同步修正旧DisclosureChevron测试对150ms字面量的假设为共享fast令牌。

SDK8.0.423组合行为15/15、0 skip，令牌回归1/1；Source validation、XAML26/26、WPF静态37 XAML/0 error、RenderHarness 0/0、双主题394 PNG Render QA/0 PROBLEM通过。详细TRX、报告SHA、磁盘变化见`../design/reviews/ui-polish-round5-20261027/evidence/U04-03-05/BASELINE.md`。精确源码run `40578426793`（baseline `122451017347`、SDK10 `122451017062`）仍运行；纯文档run `40575315327`两个SDK job也在运行。先前run `40571484630`/`40572542627`双SDK失败于过时Disclosure测试断言，已读取原始日志并记录artifact身份。required-checks API此前403，分支保护状态仍不推断。真实Playnite/UIA客户端、物理鼠标、IME与实体DPI未验；继续独立推进U04-06/08，不等待旧S/T宿主/权限状态。整轮最终候选仍须用中文根入口执行构建/测试/发布/打包/安装身份核验/启动，并独立按职责验证`scripts/build.cmd`。

## Round5 U02-05/06 最新本地状态（2026-10-28）

共享 `GeometryTokens.xaml` 提供圆角、1 DIP hairline 与分隔线令牌；`RoundedClipBehavior` 根据 Border 当前尺寸/圆角裁切并相交保留消费者原 Clip。`f59fe4c86c78cb2fc11414a431aceb49821910ee` 已推送，所有当前 Playnite 项目 XAML 的 1 DIP 边框已统一引用令牌。SDK8.0.423精确SHA聚焦组165通过、39个既有skip、0失败；source validation、26 XAML结构检查、37 XAML WPF静态审查0 errors通过。最终 Render QA 双主题334 PNG、0 PROBLEM，报告/逐图SHA见[U02-05/06证据](../design/reviews/ui-polish-round5-20261027/evidence/U02-05-06/BASELINE.md)。

run `40554157661`（初始批）与最终源码run `40555566776`的baseline/SDK10.0.401 jobs均仍在测试；身份见`evidence/U02-05-06/ci-runs.json`，尚无结果或artifact。required-checks读取此前为403，远端待验。真实Playnite、UIA/键盘/IME、物理DPI未验；中文根入口完整链和独立`scripts/build.cmd`留待整轮最终候选。旧S/T宿主/权限拒绝保持原状态，不重复探测；不触碰真实存档恢复、媒体删除或云端写入。

## Round5 U02-01～04 最新本地状态（2026-10-28）

源码提交`504562a7f0dad88454dde57b290b7862f91c93e6`已推送。U02-01/02复用生产自适应层次和对比守卫，Light/Dark达标且未重调无缺陷表面值；U02-03/04将概览普通行回归中性色，并统一成功、警告、错误、运行、排队、取消六态状态图标/前景/浅底/标签。SDK8.0.423精确身份行为回归15/15、0跳过；source validation、XAML检查25文件、WPF静态31文件0 error、Render QA双主题exit0/0 PROBLEM。报告、测试、截图与哈希见[Round5进度](../design/reviews/ui-polish-round5-20261027/PROGRESS.md)及`evidence/U02-01-04/`。

GitHub精确push run `40541712897`（SHA `504562a7`）及随后的文档run `40543778584`（SHA `0f21ac7b`）均由基线SDK与SDK10 job复现Q06过时禁用态opacity 0.72期望；实际生产控件已由U01-05改成opacity 1并使用语义Brush。两run原始作业日志已读取，run/job/artifact和摘要见`evidence/U02-01-04/ci-run-40541712897.json`及`ci-run-40543778584.json`。测试修复提交`bf7540e29a1852552784f4d2ad62c5dcd707cc60`已推送；SDK8.0.423精确SHA Release定向16/16、0 skip、source validation通过。新run `40545849867`基线job `122336135430`与SDK10.0.401 job `122336135080`仍在编译测试；目前无artifact。required-checks读取403，远端待验；不得重跑两个旧失败run。真实Playnite宿主、UIA/键盘/IME、高对比宿主表现、物理DPI、完整中文根入口链和独立`scripts/build.cmd`仍待验。U01-07/08也保持远端待验，不因当前本地结果结项。旧S/T权限/宿主阻塞保留且不重复探测。

## 2026-10-27 新授权：第五轮 UI 与动画精修 80 项

用户明确要求新增界面与动画优化任务，新增[80项计划](../design/UI_POLISH_ROUND5_80_TASKS_2026-10-27.md)与[进度账本](../design/reviews/ui-polish-round5-20261027/PROGRESS.md)。U00-01完成；U00-02/U00-03生产改动提交`58872f2c`，标题回归断言修正提交`20be8073`。精确run `40462741496`两job均因媒体收件箱宽屏测试夹具失败：CI实际排布宽1014 DIP、生产宽屏门槛1334 DIP，测试请求窗口尺寸不代表实际视口。SDK8与SDK10均Core 157/157、Worker 400/400；Playnite 1233项1214通过/1失败/18既有skip。修正夹具显式1360 DIP并断言门槛，本机SDK8该类6/6通过，等修复提交远端CI。render QA、静态审查、打包被失败门禁阻断。run/job/artifact见[CI证据](../design/reviews/ui-polish-round5-20261027/evidence/U00-02-03/ci-run-40462741496.json)。required-checks端点此前403不可读；`.tmp`清理此前遭执行策略拒绝，保留输出；旧S/T宿主/权限/物理输入状态不变。U00-04/05接续推进。

提示词明确标准中文一键入口转英文根入口的完整构建/测试/发布/打包/安装身份/启动验收，以及scripts/build.cmd职责、GitHub精确提交所有适用job（含SDK基线与SDK10）通过、每阶段安全清理自有无用产物。无远端权限不得称CI成功；不改保护/跳过测试求绿。本次仅新增本地文档，未重跑全链或查询远端CI；产品全链事实仍以90bf0405证据为准。按源码结构、80项计数、链接与diff校验交付，不创建新对话，不清理用户旧TRX/TestResults。

# GameSaveCenter 当前事实

更新：2026-10-27。历史按当时提交解释，不能覆盖本页当前结论。

## Round5 U01-05/06 最新本地状态

共享禁用按钮普通态已从整体Chrome 0.72透明改为动态禁用文字、控制填色和描边；说明文字与禁用标签的对比度通过Light/Dark层叠背景检查。U01-06沿用16/24 DIP图标系统与现有125–200% DPI回归，已满足。SDK8.0.423组合测试26/26、0 skip；源码校验、WPF静态审查和双主题render-qa均通过，render-qa报告0 PROBLEM。详情、TRX、哈希与清理结果见Round5进度`docs/design/reviews/ui-polish-round5-20261027/PROGRESS.md`的U01-05/06及其evidence。提交813adb09的GitHub Actions push run远端待验（无run/job/artifact可读；需Actions读取权限查询，详见verification.json）；真实Playnite宿主和最终根入口完整构建安装启动链/独立`scripts/build.cmd`仍待完成；未将离屏结果当作真实宿主验收。

## 2026-10-27 存档历史表格高分辨率伸缩修复与标准全链

`90bf040567f013f3a8d9ed8667028a4edb2ec524` 已推送到 `main`。在 SaveCenter 历史表格上移除固定 560 DIP 视口上限，复用 PageHost 剩余高度预算；保留现有滚动条、Recycle 虚拟化、当前游戏选框、真实命令/绑定及错误取消和恢复安全语义。没有改 Round4 S01 的密度、列管理、命名视图或断点功能。[修复与验收报告](../design/reviews/ui-history-table-4k-20261026/README.md)。

最终候选从仓库根实际运行 `cmd.exe /d /c "GameSaveCenter-一键构建安装运行.cmd < nul"`，外层退出码 0；build/install/package run 为 `20261027T010319835Z-7b052459`、`20261027T010317740Z-96e7b91b`、`20261027T012406316Z-c7e3272f`。SDK8.0.423/Release，155 步均为0，XAML25/25、solution 0 warning/0 error；1968项1910通过、58跳过、0失败，149份TRX一致，146个WPF隔离类结束。58个skip独立计数：1个真实系统指针用例默认发现期跳过，57个既有环境/架构skip。Worker win-x64发布、ZIP/PEXT一致性、安装程序集身份均通过；Playnite及Worker启动，`extensions.log`核对本次插件从安装目录加载并匹配提交身份。

同一源码第一次根链的 Worker 清理测试在删除临时 SQLite 目录时遇到文件占用，399/400；未改源码、断言或skip。该用例定向复测1/1、Worker全套400/400，之后完整标准入口再次退出0。初次失败摘要、日志与TRX已归档。RenderHarness双主题测得2560×1440逻辑DIP表格1101×899、3840×2160逻辑DIP为1741×1695，收缩再扩展通过。该证据不代表用户另一台物理4K电脑或其真实Playnite页面已验，外机确认仍待回传。

T00-01以当前90bf全链验收。清理本轮自有输出2211文件/341,640,816逻辑字节；删除清单、路径检查与C/D盘前后空间见[cleanup.json](../design/reviews/ui-history-table-4k-20261026/evidence/standard-90bf0405/cleanup.json)。32项仍有T00-07、T01-04、T01-08、T02-08、T03-05五项待权限、外机或真实宿主/输入条件；不重复探测已记录的权限阻塞。Round4仍为40已验收、2已满足、21宿主待验、1外部阻塞；本次视口修复复用既有滚动预算，不改变S01状态。当前完整记录见[专项进度账本](../design/reviews/build-ui-reliability-20261022/PROGRESS.md)。

本次标准链也为 Round4 S02-04 提供当前 SDK8 受控复核：R10 上下文行为6/6、通知收件箱2/2、Worker精确任务查询10/10、R09焦点2/2。复用T00-01的同次90bf TRX，不重复实现或构建；backlog据此更新为受控实现完成，Round4仍保留“代码完成待验收”，真实通知页面导航和返回仍待宿主验证。原始TRX与哈希见[S02-04复核清单](../design/reviews/ui-finesse-round4-20261018/evidence/S02-04/sdk8-standard-90bf0405.json)。

2026-10-27 完整进程名扫描观察到 Playnite.DesktopApp PID 39152 与 GameSaveCenter.Worker PID 42800 仍运行；先前仅按 `Playnite` 进程名查询会漏掉 DesktopApp。T03-05 隔离旧包验证不在这两个正常进程运行时启动，也不终止它们。

用户随后确认Playnite已退出；10:36+08:00再次按完整进程名扫描，目标Playnite/Worker均为0，进程冲突条件解除。隔离启动仍需通过 `Win32_Process.CommandLine` 身份预检；历史记录为Access Denied，本次遵守不重复探测规则，未启动宿主或接触正常用户设置/真实存档。最新包与旧包源码差异、身份和本次边界见T03-05 [复核证据](../design/reviews/build-ui-reliability-20261022/evidence/T03-05/source-compat-90bf0405.json)。

T01-04账本记录发现任务标题列缺失，已按计划补为“路径越界负例”；实现提交、完成状态和符号链接权限边界未变。32项表格字段现已通过结构核对。

## 2026-10-25 本机默认根入口历史成功基线（后续中文入口见末尾）

- 当前产品源码身份 b29c39a541ba92461e167f14f64917d889293161 的默认根入口 cmd.exe /d /c "GameSaveCenter-Run.cmd < nul" 已退出0：build run 20261025T152449622Z-e85af693、install run 20261025T152447580Z-d50774fa。SDK8.0.423 / Release，solution 0警告/0错误、155阶段无失败。Core157/157、Worker400/400；总计1968项1911通过/57跳过/0失败、错误、超时或中止，149份TRX一致、146个WPF隔离类全部完成。[最新完整链证据](../design/reviews/build-ui-reliability-20261022/evidence/T00-01/standard-b29c39a5/README.md)。
- 57个跳过项逐一标注为已撤销的“今日工作台 UI 架构”断言。Worker win-x64发布成功，ZIP/PEXT SHA-256均为 E6F5F836E2FA59ACBC6BE5D6EBC9351F79A72C95B4EDCFBFE4422C36E6213C77。安装清单0.6.73，六个程序集身份和SHA均与b29c39a5一致。
- Playnite PID53100启动后，extensions.log在23:52:34.784记录插件从实际扩展目录加载并观察到3个游戏；Worker PID52832从该目录下Worker路径启动。[当前安装身份](../design/reviews/build-ui-reliability-20261022/evidence/T00-01/standard-b29c39a5/install-identities.txt)。这不覆盖真实页面、浅色/窄短窗、物理输入或DPI验收。
- 用户本轮外机附件 build run 20261025T040600037Z-ec604b91 / install run 20261025T040558033Z-0b75781f在Q06 Light hover失败，未到打包/安装；附件没有同次HEAD/sourceCommit，控制台探针仍使用MouseDevice.ChangeMouseOver。该日志早于当前b29c39a5本机完整链，不能确定其精确源身份或代表当前main结果。当前main Q06 TRX本轮通过；外机快进到最新main后的默认根入口完整重跑仍待用户回传。[外机日志边界](../design/reviews/build-ui-reliability-20261022/evidence/T00-01/standard-b29c39a5/external-q06-20261025.json)。
- 外机已有stash“保存本机 GamePicker 构建前改动”，继续保留并暂不pop/drop。同步前保存所有未提交及未跟踪改动，确认clean后仅执行git pull --ff-only origin main；若fast-forward拒绝，停止回报，不reset/覆盖。快进后核对HEAD与origin/main一致，再从仓库根执行 cmd.exe /d /c "GameSaveCenter-Run.cmd < nul"，保留stash直到完整链退出0并逐项审查。
- 构建summary的workingTreeClean=false只由预存的未跟踪Round3测试证据/TRX及tests/GameSaveCenter.Playnite.Tests/TestResults/造成；跟踪源码、测试、脚本未变。用户已有Round3证据/TRX和TestResults未暂存、修改或删除。149份完整TRX及summary保留于本机artifacts/build-diagnostics/20261025T152449622Z-e85af693；包、隔离输出和安装报告也保留于artifacts。
- 同日较早默认沙箱根入口因写入Playnite用户安装目录被拒并有11项权限相关额外跳过，不计入成功结论。获准的标准根入口完整链已实际完成；最终构建、测试、打包、安装和启动均成功。
- T00-01本机当前源码完整根入口验收完成。T02-02、T02-04至T02-07的受控生产夹具和当前完整链对应WPF类均通过；它们不替代T02-08真实宿主/物理输入/DPI证据。32项计划仍有5项待验：T00-07、T01-04、T01-08、T02-08、T03-05。T01-04于2026-10-26在审核通过的提升上下文再运行，符号链接仍提示需管理员权限；13项通过、1项跳过，夹具与哨兵均安全清理，权限变化前不重试。
- T03-05在b29c39a5安装周期曾由extensions.log核实插件实际加载路径；后续b05e5696一键全链确认了安装磁盘身份和Playnite启动，但没有确认本次进程内加载路径。旧包隔离配置加载与设置往返/迁移仍未完成。
## 历史验证身份

- 此前完整验收代码：`dcbaab13`，在Q14/R08等可靠性修复后将同一checkout互斥锁扩至Windows全局命名空间。
- 此前完整标准入口成功：源码身份 `dcbaab13b9464523ffed6d88ca2748b6fb6dd647`；构建run `20261024T192816167Z-276884c0`，安装run `20261024T192814223Z-736788e1`。
- 命令：默认 `GameSaveCenter-Run.cmd`，退出0。SDK8.0.423 / Release，solution0警告/0错误；1967项1910通过、57跳过、0失败，148TRX计数一致、145WPF隔离类全部结束。Worker发布、ZIP/PEXT、安装及Playnite启动完成，六程序集磁盘版本/源码身份/SHA一致；本次未核验进程内插件加载路径。[本次完整链证据](../design/reviews/build-ui-reliability-20261022/evidence/T00-01/standard-dcbaab13/README.md)
- 最新定向验证：T01-08 `Test-BoundedProcess.ps1` 在PowerShell Core 7.6.2和Windows PowerShell 5.1受控中断均通过，精确自有子进程退出、无关进程保留、重试0；`Test-IsolatedTempCleanup.ps1` 双宿主失败/重试序列通过。该证据不覆盖真实键盘Ctrl+C。[T01-08回归证据](../design/reviews/build-ui-reliability-20261022/evidence/T01-08/README.md)
- 旧 R05 `GetCursorPos` error 5 未再中断最新完整链：`R05TooltipTimingBehaviorTests` 在 `fbd8a882` 的标准测试入口中3/3通过。该测试类结果不替代物理指针/真实宿主验收。
- 补充缺 SDK/冷 NuGet 路径：临时隔离 `dotnet` 后官方引导安装8.0.423并完整构建测试退出0；新的 NuGet 包/HTTP/CLI 缓存下还原与Release编译也退出0。此验证限本机网络，不能代替另一台电脑日志；[T00-03证据](../design/reviews/build-ui-reliability-20261022/evidence/T00-03/clean-machine-bootstrap.md)。
- `100429d0` 默认根入口run `20261024T173618477Z-d107dea1` 曾在Q14第35/145类因 `WindowAutomationPeer.GetNameCore` 读取已关闭HWND时Win32参数错误而失败；修正后的Q14定向六次均1/1，随后 `d0e11929` 标准完整链所有145个WPF类通过。失败与六次定向证据见[Q14复核](../design/reviews/build-ui-reliability-20261022/evidence/T00-01/q14-automation-drain-20261025/README.md)，完整链见[本次证据](../design/reviews/build-ui-reliability-20261022/evidence/T00-01/standard-d0e11929/README.md)。
- T00-07锁名已从`Local\`改为`Global\`。`dcbaab13`提交后的默认根入口全链退出0，SDK8.0.423、Release0/0、1967项1910通过/57跳过/0失败，发布/打包/安装/Playnite启动及六程序集身份/SHA通过。双PowerShell同会话锁回归各5/5；Task Scheduler权限拒绝跨会话实测，跨会话及跨用户ACL仍未验。失败候选隔离输出清理释放D盘约170MB，失败诊断与最新成功输出保留。[T00-07证据](../design/reviews/build-ui-reliability-20261022/evidence/T00-07/README.md)
- T03-06 清理与证据引用校验已验收：T00-07失败输出有持久清理记录，用户TRX/TestResults未动；本轮11份文档706个本地Markdown链接0失效，`validate-source.py`通过。[审计记录](../design/reviews/build-ui-reliability-20261022/evidence/T03-06/README.md)
- T03-07 与第四轮去重已验收：映射提交、证据和额外条件；Round4 64项渲染校验及7项已实现/1项外部阻塞与backlog一致。S07-03权限阻塞不变，未升级宿主待验。[映射与校验记录](../design/reviews/build-ui-reliability-20261022/evidence/T03-07/OVERLAP.md)
- 历史的T03-08初版交付检查单（2026-10-25）曾绑定dcbaab13，后续main提交只含文档/证据；当时T01-07验收后32项中有10项待权限、外机或真实宿主验收。当前清单状态以本页最新构建状态及T03-08更新版为准。[检查单](../design/reviews/build-ui-reliability-20261022/evidence/T03-08/README.md)
- T00-03恢复控制流现已在PowerShell Core 7.6.2与Windows PowerShell 5.1.22000.2538各4/4通过：中断下载、截断安装脚本、安装器退出0但仍只见SDK9.0.302均被拒绝，之后只接受global.json锁定的8.0.423；失败环境回滚和bootstrap临时目录清理通过。[双宿主证据](../design/reviews/build-ui-reliability-20261022/evidence/T00-03/recovery-20261025-verified/README.md)。这是受控故障注入，不代表真实网络/SDK压缩包损坏复验；后续 `d0e11929` 标准完整链已成功。
- e1127ac0候选的历史标准根入口run `20261024T165132695Z-aadc0bbd` 在R08悬停测试失败，未进入发布/安装；R08夹具等待初始动效完成，并延长测试副本故事板采样时间，同时保留连续性、中间态、方向、终态断言。修复后R08类连续5轮15/15、R09类2/2，见[专项证据](../design/reviews/build-ui-reliability-20261022/evidence/T00-01/r08-motion-window-20261025/README.md)；后续提交 `d0e11929` 的标准完整根入口已通过，见[本次完整证据](../design/reviews/build-ui-reliability-20261022/evidence/T00-01/standard-d0e11929/README.md)。
- T02-03定向双主题2/2、所在类11/11；真实WPF键盘焦点、选择ID/当前项和900/1400 DIP往返稳定。全链证据：[T02-03提交级完整验证](../design/reviews/build-ui-reliability-20261022/evidence/T02-03/standard-verification.json)。
- [前次标准链证据](../design/reviews/build-ui-reliability-20261022/evidence/T00-01/standard-f577fd19/README.md)绑定f577fd19；最新标准链身份见T01-08证据。后续文档提交不改变产品构建身份。
- T01-01双宿主回归与16份JSON清单证据已归档、校验；清理8个自有`.tmp`夹具目录（逻辑2,363,091字节），D盘可用空间从12,290,236,416增至12,292,726,784字节。用户旧TRX/TestResults保留。
- T01-02清理预览在f577fd19代码身份上双PowerShell各9/9；结果路径已脱敏，所有预览`CanDelete=false`，人工复核引用/占用仍是后续步骤，不存在自动删除结论。[证据](../design/reviews/build-ui-reliability-20261022/evidence/T01-02/README.md)。
- T01-06双PowerShell归档/保留夹具当前重跑均通过，303文件真实归档SHA已核对、源原件保留；f577fd19默认根入口全链成功，补足最终链验收。只认可归档后隔离清理与保留行为，不开放通用自动删除。[证据](../design/reviews/build-ui-reliability-20261022/evidence/T01-06/README.md)。
- T01-07已验收：包保留与预览回归通过；四批清理838个未引用重复PNG（113,982,425逻辑字节），无引用三份输出中的24组重复降为单份；剩余202组/571个重复路径均由当前引用源文档命中文件名/路径或目录并予以保留。索引范围为1,594张、200,160,589字节。[清理与引用复核](../design/reviews/build-ui-reliability-20261022/evidence/T01-07/README.md)。
- 本机 `c9cc5a99` 全新隔离 Release 输出编译及完整测试通过：0警告/0错误，1967项1899通过、68跳过、0失败，145个WPF类全结束，R09焦点类通过。完整安装链当时仍绑定 `dcbaab13`。外机后续已保全 stash 并从 `ea926c5e` 快进到 `d6a401af`；其标准根入口编译通过但在Q06深色hover失败，后续的测试夹具时序修复及隔离全量通过情况见本页最新状态和[Q06证据](../design/reviews/build-ui-reliability-20261022/evidence/T00-01/q06-hover-dispatcher-20261025/README.md)。

- 前次aefbddfa的第101类崩溃保留；130cd307当前同类5/5、完整链0失败，不关闭UIA或扩大skip。真实诊断已显示190bbaea插件/Worker身份及实际加载路径。

## 当前范围

- 最近一次真实首页与维护诊断截图来自190bbaea，并确认插件/Worker构建身份、插件实际加载路径与安装报告一致；Ctrl+End可达诊断摘要，未触发业务操作。130cd307随后已通过完整安装/启动，本轮未重拍UI。
- b8eedc47的存档列表/详情交叉滚动证据保留；不将它替代190bbaea的完整双主题/窄短窗矩阵。
- 51641d06安装后的Playnite存档页新增深色常规窗口滚动截图：历史列表与详情分别滚动、另一侧锚点稳定，底部校验入口可达但未触发；安装身份可见，当前进程模块枚举未暴露插件程序集路径。[T02-08索引](../design/reviews/build-ui-reliability-20261022/evidence/T02-08/host-51641d06-index.json)
- 完整双主题/正常窄短窗、命令面板宿主验收、物理键盘/IME及物理DPI仍待验。设置对话框坐标偏差后已停止主题切换，没有改配置。
- T02-03已验收：活动WPF窗口真实 `Keyboard.FocusedElement` 双主题稳定，所在类11/11，51641d06默认完整链退出0。T02-02、04～07受控回归与全链通过；T02-08实施中。T03-05已用旧/当前真实PEXT验证生产文件事务失败回退和重试，239个文件逐SHA一致；隔离Playnite配置的插件加载/设置兼容仍待验。32项整体未完成。
- [32项专项账本](../design/reviews/build-ui-reliability-20261022/PROGRESS.md)、[第四轮机器账本](../design/reviews/ui-finesse-round4-20261018/ROUND4_STATUS.json)、[待办](../AUTONOMOUS_BACKLOG.md)是任务入口。
- SDK缺失为历史；CEF/ETW与S07-03 branch-protection读取权限边界独立保留，按[能力缓存](ENVIRONMENT_CAPABILITY_CACHE.json)重试。

## 保留、清理与后续

- 前次f25标准链测试通过，但运行中改文档触发打包保护退出1；[失败保留](../design/reviews/build-ui-reliability-20261022/evidence/T03-01/README.md)。先提交再冻结完整重跑已成功，未绕过保护。
- 25个本阶段测试结果文件先归档校验，再移除六个原临时目录；原失败、当前包/构建/诊断/宿主图与用户旧Round3 TRX/TestResults保留。详情见T02-08清理记录。
- 后续源码/测试/脚本/资源改动按影响范围运行定向检查，并对当前候选执行标准根入口完整链；完整链期间不修改任何跟踪文件。纯文档运行源码/链接校验，不触发产品构建，也不为其CI通过再追加记录提交。
- 外机此前的CS8300来自 `GamePickerViewModel.cs` stash冲突标记；用户已用含未跟踪文件的stash保全改动、快进至 `d6a401af`。快进后的新根入口确实启动测试，但第29/145类Q06因Dark合成hover采样时序失败，未打包/安装。该新失败与更早R09旧断言身份缺失是不同事件；新候选的完整标准链仍待修复提交后的外机复验。
- T00-04路径与入口环境修复已验收，隔离克隆已归档后清理。外机R09附件期望值对应旧3f7d0b30测试；f13b9e05后主线改为Chrome1 DIP+独立验证描边2 DIP，130cd307当前类2/2。外机具体HEAD/DLL/PDB身份缺失，见专项复核；继续T02-08证据矩阵、SDK故障恢复与回退隔离验证，按32项账本逐项审计剩余条件；不要以受控或启动结果自动升级宿主验收。
- 旧事实原文见[截至2026-10-24历史](CURRENT_STATE_HISTORY_THROUGH_20261024.md)，更早记录仍可在Git和既有历史页追查。

## 持续有效的实现基线

- 产品版本0.6.73；共享入口为 `Themes/AcrylicProductionResources.xaml`、`DesignTokens.xaml`、`WpfUiProduction.xaml` 与 `Redesign.xaml`。
- Demo-first优先级保留；`GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design/` 当前工作区不存在，缺源时沿用生产共享样式，不伪造Demo对照。
- 保留业务命令、Binding、安全/取消/错误语义、虚拟化、UIA、键盘与Playnite兼容；游戏平台筛选以 `DropDownClosed` 提交。
- 行为矩阵入口 `scripts/e01-behavior-matrix.ps1`；`MANUAL QA REQUIRED` 项仍需对应人工证据，自动测试不替代。
- 开发启动继续阅读[PROJECT_MEMORY.md](PROJECT_MEMORY.md)、[WORKLOG.md](WORKLOG.md)、[DEVELOPMENT_HANDOFF.md](../DEVELOPMENT_HANDOFF.md)，按记忆协议推进。

## 2026-10-26 指定一键构建入口中间结果（提交后已完成）

用户日常运行的入口为仓库根目录 `GameSaveCenter-一键构建安装运行.cmd`；验收必须实际运行该包装器，不以单独 `dotnet build` 代替。R05真实鼠标测试改为默认在发现阶段跳过，只有显式设置 `GSC_RUN_INTERACTIVE_POINTER_TESTS=1` 才控制桌面指针。当前该入口第二次运行的 Release、Core/Worker 与全部146个WPF测试组通过；打包器因源码尚未提交而按身份保护退出，尚未发布、安装或启动。提交后重新运行完整入口，结果以同次 `artifacts/build-diagnostics/<runId>/summary.json` 和命令退出码为准。详见 [T00-01 R05入口证据](../design/reviews/build-ui-reliability-20261022/evidence/T00-01/r05-pointer-optin-20261026/README.md)。

## 2026-10-26 指定中文一键入口完整链通过

在提交 b05e569667925b6747f5f588d1c69ef50f84a772 上，直接运行用户指定的 GameSaveCenter-一键构建安装运行.cmd，build run 20261026T033415954Z-814046fa / install run 20261026T033414003Z-2c4989e0，完整退出0。SDK8.0.423、Release 0警告/0错误，Core157/157、Worker400/400，总计1968项1910通过、58跳过、0失败，146 WPF隔离类；Worker发布、ZIP/PEXT身份、安装验证及Playnite启动全部成功。真实鼠标Tooltip测试默认skip的原因及单独开启方式见入口修复证据。安装报告证明磁盘文件身份，不代表本次已确认进程内插件加载路径。

## 2026-10-26 外机4K存档历史表格接续

新增CLOSE-UI-01。根因是历史DataGrid剩余视口被560 DIP固定上限截断；本轮改为按当前PageHost扣除摘要/框架/详情后的剩余高度布局。复用既有UI-SCROLL-LAYOUT-REPAIR-20261020预算与滚动结构，没有修改候选表、共享滚动条、GamePicker、业务命令/绑定或恢复安全语义。

提交前SDK8.0.423：相关WPF几何类11/11；完整Render QA重跑成功，3840×2160逻辑DIP下SaveHistoryGrid 1741×1695、2560×1440下1101×899，双主题和resize恢复通过。首次Render QA仅侧栏快速切换探针未settle，保留诊断后重跑通过。该证据是离屏逻辑DPI，不等同用户另一台机器或真实Playnite。当前候选完整中文根入口链待提交后执行；目标外机物理DPI仍待复核。32项账本的T00-07、T01-04、T01-08、T02-08、T03-05边界不因本条改变。

## 2026-10-27 Round5 U01-07/08

U01-07通过既有生产按钮换行/无省略和长文案、路径、错误、复制行为覆盖去重；U01-08测试夹具升级到生产字号200%（Body 28、Caption/按钮24 DIP），覆盖输入、操作按钮、表头、可滚动技术详情及精确文本选择；SDK8.0.423专项2/2与组合21/21、0 skip。唯一源码改动在测试夹具，提交`11c1411e4dc5b673ccaa1ab1d05a1c8ae114ce5c`已推送。GitHub插件返回空workflow/status，公开Actions API超时，故SDK8/SDK10远端状态待认证查询，不按空结果判通过。真实Playnite/UIA/键盘/IME/物理DPI仍未验。证据：`docs/design/reviews/ui-polish-round5-20261027/evidence/U01-07-08/`。整轮中文根入口完整链、独立`scripts/build.cmd`和GitHub适用job仍待最终候选；不删除用户Round3 TRX或共享TestResults。

## 2026-10-28 Round5 U02-07/08

U02-07/08已去重并复核现有共享能力：阴影采用有限层级，重复行无BlurEffect，低透明模式关闭阴影/透明弹层；DynamicResource主题切换、高对比/宿主跟随和效果恢复由R09行为覆盖。SDK8.0.423专项9/9、0 skip；90秒受控WPF探针82轮/351动作/0失败，总动作P95 714.46ms，对照U00代理基线933.16ms，但不是Playnite帧率/输入延迟证据。没有生产代码缺口，本批不重复造样式；两项保持“代码完成待验收”。原始报告、TRX与界限见Round5 U02-07/08 BASELINE。U02-05/06 CI快照显示417bed51的SDK10 job成功但baseline已通过render-qa与WPF静态审查、正在生成扩展包；f59fe4c8 baseline在Render QA、SDK10仍在编译测试；文档提交的双job仍运行，required-checks权限403；真实Playnite/UIA/实体DPI继续待验。下一独立小批进入U03共享按钮状态；旧S/T宿主/权限边界不阻挡推进。

## Round5 当前进度（2026-10-28）：U03-01～03

8c927a93cff92b923a8efbedbdf28aadbc10d00b已推送。共享按钮模板现状已盘点，五态、忙碌稳定和图标目标已有生产能力；本批未改生产资源，只加生产图标动作UIA名称/命令与键盘焦点的回归断言。SDK8.0.423精确SHA聚焦13/13、0 skip；证据见docs/design/reviews/ui-polish-round5-20261027/evidence/U03-01-03/。run 40561836233的baseline 122392618285与SDK10 122392618705现均completed/success，基线15步、SDK10 8步全成功，artifact身份见Round5 `evidence/U03-01-03/ci-run-40561836233.json`。任务状态仍为“代码完成待验收”：真实Playnite、UIA客户端、物理鼠标和DPI未验。旧S/T宿主或权限待验不阻断独立控件优化。整轮中文根入口完整链与独立scripts/build.cmd仍待最终候选阶段完成。

## Round5 当前进度（2026-10-28）：U03-04～07

`d1fc1308317fb805d61d67a7dda77d71e0afff18`和`5300a2b5c70f7736f1f46617667de73749b8d883`已推送。U03-04/05改共享TextBox禁用/验证错误语义和搜索区域源码回归，SDK8.0.423隔离32/32，XAML26/26、WPF静态37文件/0 error、Render QA双主题394 PNG/0 PROBLEM。U03-06/07通过去重复用既有ComboBox与CheckBox模板，仅加长选中项/箭头分栏源码守护测试，专项13/13。精确TRX、报告与摘要在Round5 evidence/U03-04-05及U03-06-07。对应GitHub runs `40566121765`和`40567683673`的基线/SDK10 job均已完成成功；U03-04/05两job共23步，U03-06/07共23步，包和诊断artifact身份见Round5 evidence/U03-04-05及U02-05-06/ci-runs.json。U03-01/03 run `40561836233`也已双job成功。真实Playnite/实际UIA/鼠标/实体DPI待验。U03-04/05临时Render QA旧副本在2026-11-09只读复查时已不存在；没有执行删除或推断空间回收，见checks/cleanup.json。旧S/T权限待验不阻止继续独立控件批。整轮最终根入口构建安装启动、独立scripts/build.cmd和所有候选CI门禁未完成。

## 2026-10-28 Round5当前源码与验证状态

`main`已推送至`237e993f9e03541f5e8056c30cab8fdb029c0a77`，实现U05-01/02轻量页面切换和紧凑任务详情开合。SDK8.0.423相关行为5/5、0 skip；source validation通过，XAML26/26，WPF静态37 XAML/0 error，离屏Render QA精确SHA 394图/0 PROBLEM。GitHub run `40623449222`精确SHA两个适用job均成功：基线SDK8.0.423及SDK10.0.401各1982总/1924通过/0失败/58既有skip；扩展包与artifact身份见`docs/design/reviews/ui-polish-round5-20261027/evidence/U05-01-02/ci-run-40623449222.json`。required-checks权限此前403，保护状态仍“远端待验”。U05-01/02仍是“代码完成待验收”；真实Playnite、物理UIA/IME/DPI、整轮中文根入口/独立`scripts/build.cmd`仍未验。接续U05-03/04。

## 2026-10-28 Round5 U05-03/04 当前接续

本批只补充详情对象与弹层退出行为证据，生产实现继承237e993f。SDK8.0.423 Release定向9/9、0 skip，source/WPF静态通过；U05-03代码完成待验收，U05-04仍需菜单和所有抽屉实际开闭/回焦证据。失败夹具及缺构建身份诊断保留，详见docs/design/reviews/ui-polish-round5-20261027/evidence/U05-03-04/BASELINE.md。最终根入口完整链、独立scripts/build.cmd及当前提交CI仍待完成；不重复旧权限探测。

提交后复核：实现/测试提交18d73836e58946eb85c8db1c50a85519d14e1b3d已推送；冻结跟踪文件后SDK8.0.423 Release精确身份9/9、0 skip、退出0，见tests/u05-03-04-exact-18d73836.trx。GitHub run40638353451的baseline job122650472010及SDK10 job122650471976均仍在运行，无终态或artifact，见ci-run-40638353451.json。required-checks既有403不重试。

## 2026-10-28 Round5 U05-04说明弹层接续

生产键盘说明Popup由硬编码Fade/透明改用共享资源，增加打开焦点、Escape关闭和触发器回焦及UIA名称。SDK8.0.423 Release组合16/16、0 skip；source/XAML26/WPF静态0 error，scripts/render-qa.ps1退出0、双主题394 PNG/0 PROBLEM。证据见docs/design/reviews/ui-polish-round5-20261027/evidence/U05-04-help/BASELINE.md；U05-04代码完成待验收，提交后CI待查。继续U05-05/06加载与进度小批；整轮最终中文根入口完整链/独立scripts/build.cmd及真实宿主边界保留。

## 2026-10-28 Round5 U05-05/06当前接续

共享WorkspaceStatePresenter延迟180ms显示Loading视觉提示（Hidden保留测量槽），错误/内容状态立即切换，退出/隐藏/卸载停止计时器。生产进度语义去重保留，新增实际进度列未知/0/73/取消/成功五态宽度与右边界检查。SDK8.0.423 Release组合13/13、0 skip；source、XAML26、WPF静态0 error、scripts/render-qa.ps1退出0/双主题394图/0 PROBLEM。证据见docs/design/reviews/ui-polish-round5-20261027/evidence/U05-05-06/BASELINE.md。两项代码完成待验收；新提交CI待查询，旧S/T边界不变。已安全清理本批一份无引用中间成功TRX，保留最终/失败诊断和当前渲染。接续U05-07/08；整轮中文根入口完整安装启动链与独立scripts/build.cmd仍待最终候选执行。

## 2026-10-28 Round5 U05-07/08当前接续

生产Shell换页/卸载清理说明Popup、命令层和PageHost动画；TaskCenterView卸载取消详情退出代次/时钟。31次切页×主题×弹层×详情中断及Shell卸载组合通过，缓存/筛选/选中/滚动3项和隐藏/最小化进度1项通过，合计SDK8.0.423 Release7/7、0 skip。source/XAML26/WPF静态0 error、scripts/render-qa.ps1退出0/双主题394图0 PROBLEM。证据见docs/design/reviews/ui-polish-round5-20261027/evidence/U05-07-08/BASELINE.md。两项代码完成待验收，真实宿主及新提交CI待验；接续U06-01/02共享表格小批。最终中文根入口完整安装启动链与独立scripts/build.cmd仍是硬门禁。当前审计和有价值失败诊断保留，删除清单为空，未碰用户/其他阶段输出。

## 2026-10-28 Round5 U06-01/02当前接续

本批去重复用共享表格节奏与S01密度，无生产资源变更。补表头未排序/升/降序文字几何与箭头槽位、四页双主题三态叠加及失焦选择，并在302对象滚动后核对行容器实例承载不同Item、无旧状态串色；媒体Standard例外保留。初轮组合16/16，强化滚动专项1/1、0 skip；独立页夹具补真实Shell的有限视口调用，原失败保留。source/XAML26/WPF静态0 error；Render QA继承同生产源码U05-07/08，未制造截图副本。证据docs/design/reviews/ui-polish-round5-20261027/evidence/U06-01-02/BASELINE.md。两项代码完成待验收；提交后双SDK CI待查，真实Playnite/物理输入/UIA/DPI与最终中文完整链、独立scripts/build.cmd未验。接续U06-03/04行内动作与工具栏。

## 2026-10-28 Round5 U06-03/04当前接续

复用行内常显删除文字与目标绑定，补实际焦点、受控悬停列宽和安全对象检查；复核窄宽工具栏换行、筛选摘要和多选选择往返。SDK8.0.423 Release 7+9项通过、0 skip；source/XAML26/WPF静态0错误，生产视觉未变，Render QA继承同源码U05-07/08。证据docs/design/reviews/ui-polish-round5-20261027/evidence/U06-03-04/BASELINE.md。bca60704的run40639378202双SDK job成功，原始日志及3个artifact身份见该证据CI清单；required checks既有403不推断。U06-03/04代码完成待验收，接续U06-05/06。最终根入口完整链、独立scripts/build.cmd和真实宿主仍待验。

## 2026-10-28 Round5 U06-05/06当前接续

生产代码未改。存档历史非零滚动起点进入/清除40项多选保持偏移，Escape保留选择，双主题断点往返保持选择和焦点；实际任务页空/筛选无结果/错误区分，清筛选合成命令不触发刷新。SDK8.0.423 Release最终17/17、0 skip；首次锚点夹具缺有限视口失败已保留，修正后原断言通过。source/XAML26/WPF静态0错误，Render QA继承同生产代码。证据docs/design/reviews/ui-polish-round5-20261027/evidence/U06-05-06/BASELINE.md；两项代码完成待验收，真实宿主和新提交CI未验。未采集阶段开始前磁盘快照，证据中明确记录该限制；未删除其他任务输出。接续U06-07/08，最终中文根入口完整链及独立scripts/build.cmd仍待验。

2026-10-28 CI新失败：run40644355886 SDK10及run40645391789双SDK在ProcessMappingInlineActionBehaviorTests第87行受控IsMouseOver断言失败；原始job日志与artifact身份见ci-hover-failures.json。生产命令未执行失败；测试在注入状态后泵送后台消息，可能先收到真实桌面指针更新。夹具改为同步UpdateLayout后立即检查进入/退出悬停，保留可见性、焦点、列宽与目标断言。SDK8.0.423本地1/1、0 skip，TRX见tests/ci-hover-synchronous.trx；当时工作树另含U06-07/08候选，SDK10和新提交CI仍待验。假设尚待远端验证，不声称CI已修复。

## 2026-10-28 Round5 U06-07/08当前接续

候选路径表移除560 DIP上限，沿用有限剩余高度预算；同条件3840×2160逻辑DIP由1747×560增至1741×1665，窄短窗回缩/内部滚动通过。SDK8.0.423 Release组合4/4、0 skip；R18 2千/1万/2万夹具任务最多10实现行、媒体最多7，保留媒体Standard例外；实现行和变换无活动动画、模板无逐行Storyboard。source/XAML26/WPF静态0 error、双主题Render QA394图/0 PROBLEM。报告、前后图、失败诊断和安全清理见docs/design/reviews/ui-polish-round5-20261027/evidence/U06-07-08/BASELINE.md。969bfc20的run40640556766双SDK均成功、三个artifact身份已登记；U06-03受控悬停失败修正3ecacdc0的run40647037308尚在运行。真实Playnite/物理DPI和最终中文根入口完整链及独立scripts/build.cmd仍待验；下一批U07-01/02。

## 2026-10-28 Round5 U07-01/02当前接续

生产DialogOverlay动作区从水平StackPanel改为WrapPanel，非末项统一8 DIP间距；标题/真实对象与影响文本、独立正文滚动、危险确认安全策略保留。SDK8.0.423 Release10/10、0 skip，含220 DIP受控按钮换行、合成Escape取消回焦、重复确认保护；一次反射重载歧义失败已修正并保留。source/XAML26/WPF静态0 error，Render QA双主题394图/0 PROBLEM；证据docs/design/reviews/ui-polish-round5-20261027/evidence/U07-01-02/BASELINE.md。真实Playnite长正文/物理输入/UIA/DPI待验。悬停修正3ecacdc0的SDK10 job成功，baseline仍在运行，artifact身份已记录；最终完整中文根入口链、独立scripts/build.cmd和本批CI仍待验。接续U07-03/04。

## 2026-10-28 Round5 U09-01/04当前状态

`main`源码提交已推送至`8deb17c75ac91d716ec38b05f6f0a54530df2217`，新增 RenderHarness 假样本固定时钟。SDK8.0.423精确提交键盘/UIA组合16/16、0 skip；固定 UTC 的双主题×11尺寸 Render QA 394 PNG/0 PROBLEM，重复两次主样图逐像素一致；source validation、XAML26/26和WPF静态42文件/0 error通过。U00同条件操作周期P95 933.16→654.02ms，内存趋势代理下降；U00无动画逐帧及累计分配字节基线，当前离屏单次收起帧间隔P95约90ms，U09-04继续待真实宿主复核。2b6c4022 CI双job成功；8deb17c7 SDK10成功、baseline仍运行；required-checks查询403，保护状态远端待验。完整本地入口链、独立scripts/build.cmd、阶段清理与整轮审查未完成。详见`docs/design/reviews/ui-polish-round5-20261027/PROGRESS.md`。

## 2026-10-29 Round5 U08-08 / U09-01 跨页与证据复核

Trainer 工具列表删除了永久折叠、与共享空态 Presenter 重复的旧文字节点；行为源码测试守住共享状态模板及加载期间空态抑制。同步修复`d19570fa` GitHub双SDK都失败的窄窗布局测试夹具：先加载窗口并确认空闲忙碌状态折叠，再快照可见动作原边距。SDK8.0.423 Release组合16/17（16通过、0失败、1个历史架构skip），双主题间距用例2/2；Render QA双主题394图/0 PROBLEM，source validation通过、WPF静态42 XAML/0 error。证据、TRX/报告哈希和失败job/artifact身份见`docs/design/reviews/ui-polish-round5-20261027/evidence/U08-08-09-01/`。当前修正提交待推送/CI；真实Playnite、外部UIA/Narrator、实体键盘/IME/物理DPI未验。按安全边界清理了本阶段唯一原始TRX临时目录共5文件/134106字节，保留当前Render QA；cleanup.json列空间、引用和占用核查。U08-08代码完成待验收，U09-01继续进行中。
## 2026-10-29 Round5 U08-01/02 可见窗口复验

测试提交 `957ee036e9c26e6b41af620ec916aaca1f37d673` 已推送。概览优先状态与下一步命令、存档历史当前游戏身份/未选择提示均在 `1040×700 DIP` 可见 WPF 窗口验证；原命令、滚动边界和恢复前置未变。SDK8.0.423 Release精确提交2/2、0 skip，source validation通过，WPF静态42 XAML/0错误/26既有warning/179 info。仅改测试，无生产XAML/资源改动，因此本批不重复Render QA；真实Playnite、物理DPI及当前提交GitHub双SDK run/job/artifact仍待验。证据：`docs/design/reviews/ui-polish-round5-20261027/evidence/U08-01-02/BASELINE.md`。

## 2026-10-29 Round5 U07-05/06 长标题精修

`dcc288d66368b05884e47d57db0749a98ea17a36`已推送。通知收件箱与Toast标题单行省略，完整内容继续可从Tooltip/UIA及通知详情读取；列表虚拟化、四条Toast上限和焦点暂停计时保持。SDK8.0.423 Release精确SHA聚焦7/7、0 skip，source validation通过，WPF静态42 XAML/0 error；固定UTC双主题Render QA 394 PNG、0 PROBLEM，长标题溢出由行为测试验证，短截图样本未触发溢出。证据位于`docs/design/reviews/ui-polish-round5-20261027/evidence/U07-05-06-title-density/`。U07-05仍待刷新数据/来源契约/真实宿主；U07-06待Playnite叠放遮挡复核。GitHub状态为空且公开check-runs触发403限流，双SDK精确run/job/artifact标“远端待验”。阶段`.tmp`安全清理已完成，共移除1262个文件/191,875,219字节；空间与路径/占用核验见`cleanup-dcc288d6.json`。整轮根入口完整链及独立`scripts/build.cmd`仍未执行。

## 2026-10-29 Round5 U09-05 WPF测试宿主诊断

已复现授权提升完整链中的WPF `StylusWisp` 异常：列菜单 Popup 鼠标捕获时测试主机中止。仅在隔离的列菜单行为测试进程禁用该用例不覆盖的 stylus/touch 栈；SDK8.0.423 Release正式构建0警告/错误，XAML26/26，目标类5/5通过。进程 `TEMP/TMP` 使用阶段自有 D盘路径绕过C盘系统临时目录空间不足；未清理C盘或其他任务文件。证据 `docs/design/reviews/ui-polish-round5-20261027/evidence/U09-05-wpf-stylus-fix/`。完整中文根入口安装启动链、独立 `scripts/build.cmd` 和该候选SHA的GitHub双SDK job/artifact尚未验收；不据局部编译结项。

## 2026-10-30 Round5 U09-01补U05-05/06历史配对

U05加载反馈与真实进度列完成同数据、同主题 before/after RenderHarness 配对。SDK8.0.423 Release 两侧 build/capture 均 exit0；快照 `bca60704` / `969bfc20`，短 Loading 从 Visible 变 Hidden，慢加载在采样点恢复，错误立即可见；660×220 DIP槽位保留。五种进度状态的 Light/Dark 样本和值均匹配 DTO，进度条52 DIP、标签右界100 DIP不变。临时探针只实例化生产控件/资源，没有生产文件变更；真实Playnite、UIA/物理输入DPI和业务写入仍待验。清单、CSV、精选截图与探针失败/修正诊断见 `docs/design/reviews/ui-polish-round5-20261027/evidence/U05-05-06/historical-render-pair-20261030/`；U09-01其余视觉批次配对仍待盘点。

## 2026-11-02 Round5 U06-08 / U09-04 大列表与滚动复测
当前源码f1a614a8的SDK8.0.423 Release隔离构建exit0、0 warning/0 error；RenderHarness专案构建也exit0。R18大列表受控STA Window测试1/1通过、0 skip：20k Task实现10/8行，20k媒体后端限2k UI项并实现7/7行；detached scaleprobe20个跳点显示44个媒体行容器，已明确不等同宿主容器积累。两轮120秒耐久加30秒观察均exit0，Task页滚动p95为52.02/42.96ms，页面切换布局p95为463.20/439.21ms，不能归因到单一控件或动画。生产源码没有改动，临时RenderHarness标签已还原。证据docs/design/reviews/ui-polish-round5-20261027/evidence/U06-08-U09-04-scroll-followup-20261102/；U06-08仍代码完成待验收，U09-04仍进行中。

## 2026-11-02 Round5 U05-01/U06-07/U09-04 活动工作区重排

生产提交25bcbf7934575e6aa933373e1f508a1234b28b97已推送。仅对PageHost当前托管的缓存页应用响应式布局，隐藏缓存页在重新激活时使用最新尺寸；外部注入内容不会被当作缓存工作区重排。候选Release build与R08切换行为组分别0 warning/error、5/5通过且0 skip；固定UTC双主题Render QA 394图、0 PROBLEM。

受控WPF Window在1040×700 DIP下进行120秒动作与30秒稳定观察，203个页面切换布局样本p95 408.80ms、最大544.66ms，低于f1a614a8两轮463.20/439.21ms；合并计时仍包含主题变化和同步整页布局，不归因单一控件。U09-04继续进行中。U05-01、U06-07维持代码完成待验收，真实Playnite/UIA/实体输入和物理DPI尚未验证。

f1a614a8的baseline与SDK10 CI jobs均已success；文档提交de9fe37d两个适用job均已success。25bcbf79的SDK10 job成功，baseline job最近仍在编译与测试，未计通过。required-checks此前403，不重复查询。证据位于docs/design/reviews/ui-polish-round5-20261027/evidence/U05-01-U06-07-U09-04-active-layout-20261102/，构建安装启动最终门禁仍待冻结候选执行。

## 2026-11-07 Round5 U09-05 用户黑窗反馈重开

用户新截图再次显示构建期间的黑色窗口，未提供窗口句柄或进程身份。先前 `Default` 25 ms 轮询只适用于指定受控运行，无法排除短于采样间隔的闪现；U09-05 已重开。后续本机 WPF/Playnite 验收须将整个进程树放在非活动 desktop，验证 input desktop 保持 `Default`，不得据此宣称用户截图根因已确认。

`e65a719` 的 SDK10 CI #1482 在 `HostShutdownDuringReadIsDistinctFromCallerCancellation` 发生测试夹具时序竞态：已连管道即取消，但服务还未读完请求。源码测试现补 received 信号与取消前等待。SDK8.0.423 本地 Release 编译成功，目标测试受 Named Pipe 权限限制跳过（1 skip、0 pass）；不算行为验收。精确修复提交、CI 双 SDK、中文根入口完整链与独立 `scripts/build.cmd` 仍待验。U09-05、U09-04、U09-08进行中；U09-06远端待验；Round5计数为5已满足、71代码完成待验收、3进行中、1远端待验。
## 2026-11-07 U09-05 入口窗口修订接续

已确认之前在 `CodexSandboxDesktop` 观察到的 `Default` 不是用户实际输入桌面；非活动 WPF 动画测试因此出现 R08/Q10 时钟未前进失败。后续 WPF 行为验收使用 `WinSta0\Default`，并用 Win32 `CREATE_NO_WINDOW` 隐藏控制台；真实桌面 Q10 聚焦类 1/1 通过，8 个顶层显示事件均为 1% 透明、无激活、无任务栏的 testhost 窗口，未出现 Console/Playnite 窗口。

`GameSaveCenter-Run.cmd` 原成功/失败分支的 `pause` 会留下黑色命令窗口；当前工作树已删除等待，隐藏安装 PowerShell 子进程。`scripts/build.cmd` 也隐藏 PowerShell 子进程并保留 `%*`。source validation、命令行隐藏参数 smoke、未知 build 参数拒绝和 `git diff --check` 通过。精确 SHA `62a363d3` 的 CI #1483 baseline SDK8 与 SDK10 两 job 均 success，见 Round5 `evidence/U09-05-no-flash-20261107/ci-run-1483.json`。

入口修改仍待中文根完整安装启动链、独立 `scripts/build.cmd` 与全程窗口事件复核；U09-05 继续进行中，不声称已确认历史截图 HWND。状态计数仍为 5 项已满足、71 项代码完成待验收、3 项进行中、1 项远端待验。真实恢复、媒体删除和云端写入未执行。
## 2026-11-07 Round5 当前状态补充：用户黑窗仍未解决

用户再次报告闪屏。所附文件记录时间为 2026-11-06 13:59:27，无 HWND/PID/desktop 身份；本轮单次只读 Default 窗口快照没有匹配的 Playnite/GameSaveCenter 窗口，但不是连续事件监测。U09-05保持进行中，不能因 WPF 测试隔离或受控快照结项。

SDK8.0.423 Release 的 CI #1488 紧凑任务详情动画等待测试改用有界状态轮询并保留全部行为断言；本地目标用例在三个 WinSta0 私有桌面各1/1通过。该改动是测试稳定性修正，与黑窗根因无关。证据与未验边界见[Round5进度](../design/reviews/ui-polish-round5-20261027/PROGRESS.md)。

## 2026-11-08 Round5 U09-05 当前候选与桌面事件核验

当前 `main` 提交为 `92bd9de13bd40c13806fb0cba8900ada2e379fc2`，工作区跟踪源码无修改。精确提交上 `scripts/build.cmd -OutputRoot .tmp/r5-r08-final-full-20261108/output` 使用 SDK `8.0.423` 完整构建/测试 exit 0：2030 项、1962 通过、0 失败、68 个明确原因 skip、157 个 WPF 隔离类；skip 不计通过。WinEvent 监听明确绑定实际 `WinSta0\Default` input desktop，从构建启动至结束约37分钟记录 0 个顶层显示/隐藏事件。证据 `docs/design/reviews/ui-polish-round5-20261027/evidence/U09-05-no-flash-20261107/full-build-no-flash-20261108/`。

这只证明独立 build.cmd 在该次运行没有向用户交互桌面显示窗口，不证明用户黑窗来源或中文安装运行入口无闪窗。中文 `GameSaveCenter-Run.cmd` 会启动 Playnite；本轮未调用该入口。既有 Playnite PID 23488 与 Worker PID 30516 无主窗口句柄，均未结束或重复启动；未更改卷/盘符/映射。GitHub run #1490 / `42459902671` 两个适用job均success：SDK10 job `128666747381` 8/8步，baseline job `128666747608` 15/15步。预览artifact `13610836964`（90,793,668 bytes，digest `162f7b7955c8b0a2ef7c87425329afa13241c4f821247ab8881ae2e51de2e4bf`），基线诊断 `13610448945`（2,524,028 bytes，digest `89ccc21dd9929d63fdf8da5a10e6a757f41c6ad2e08d0c9e368f53bb3d82131e`），SDK10诊断 `13610475659`（1,278,945 bytes，digest `4876f23877623874671e8f2a16221d1f8302350b3d84a592832305dce2d1d196`）；记录见Round5 `evidence/U09-05-no-flash-20261107/full-build-no-flash-20261108/ci-run-1490.json`。required-checks仍受此前403限制，U09-06保持远端待验。U09-05仍进行中，不能因独立build或CI通过将未验的黑窗结项。

本阶段清理只删除本任务隔离根 `.tmp/r5-r08-final-full-20261108/`：728文件、169,300,382 bytes，路径在仓库`.tmp/`内且无重解析点/占用。证据副本哈希已核对；原始artifacts诊断/TRX保留，其他任务TestResults未触碰。D盘空闲实测11,950,292,992→12,121,559,040 bytes（+171,266,048；与删除目录大小略有差额，可能包含并发I/O），C盘前后4,907,458,560 bytes；未改卷、盘符或映射。详细清单见`evidence/U09-05-no-flash-20261107/full-build-no-flash-20261108/cleanup.json`。

### 用户再次报告闪屏：区分build与完整运行入口

只读核对当前脚本：`scripts/build.cmd` 最终只运行 `build.ps1`，不启动 Playnite；完整中文包装器转交 `GameSaveCenter-Run.cmd`，其 `dev-install-run.ps1` 在打包后关闭 Playnite/自有 Worker、安装插件，再普通启动 Playnite。重复完整链会重复重启宿主，属于潜在可见闪窗路径；但截图无 HWND/PID，不能将具体黑窗归因于它。小批只用 build.cmd；冻结候选完整安装启动另择不打扰交互桌面的验收方式。旧 Playnite PID 23488、Worker PID 30516 未操作。

## 2026-11-09 Round5 U03-06/07 下拉与勾选复核

生产 `GscWpfUiComboBoxTemplate` 和 `GscCheckBox` 覆盖长值显示、独立箭头列、Popup限高/滚动/方向键、混合态、长标签、整体命中区、Space及禁用状态。U03-06/07仅新增ComboBox源码守护测试；8份归档TRX哈希全部匹配，postcommit专项13/13、0失败/skip。GitHub run #1328两套SDK job成功，artifact见Round5 `evidence/U03-06-07/ci-run-40567683673.json`。仍待真实Playnite、Narrator/外部UIA、物理输入与实体DPI验收；不改游戏选框、DropDownClosed契约、滚动条或虚拟化。

2026-11-09 Round5 U03-08/U04-01/02：共享数值/路径控件、双主题动效和扫过行为已有精确测试/渲染证据。早期双SDK失败均已结束并保留；冻结整合SHA `5c0c57b06aed27dc46c41ef3d655ec20dafa5ec7` 的GitHub run `40900374673`/#1398双job success，artifact身份见Round5 `evidence/U09-final-5c0c57b0/github-ci.json`。三个任务仍待真实宿主/输入验收。检查/清理时C盘3,193,511,936 bytes、D盘20,195,618,816 bytes，清理前后未增加；完整根入口要求C≥4 GiB、D≥3 GiB，C不足时不运行完整安装启动链。旧U03渲染`.tmp`路径只读确认已不存在。

2026-11-09 Round5 U04-03/04/05/07：共享页签提示条修正 `388c46ea` 已包含于冻结整合候选 `5c0c57b0…`；#1333/#1334历史失败终态已核，run #1398 双 SDK 完整链成功。四项仍待真实宿主/UIA/IME/实体输入与DPI验收。历史U04测试及Render QA `.tmp`目录已不存在；最新复核未删除文件，也不认领回收空间。当前完整链预检测得C盘3,203,391,488 bytes（距4 GiB差1,091,575,808 bytes）、D盘20,194,029,568 bytes；C不足时继续独立任务，不运行安装启动链。

2026-11-09 Round5 U04-06/08：冻结候选`5c0c57b0…`的#1398双SDK完整链通过；原源码run #1335双SDKQ14 fixture失败已记录终态，失败诊断未删。四个明确属于本批的嵌套build `.tmp`目录已按PowerShell精确路径安全检查后清理，移除318文件/28,672,650 bytes；删除后C 3,217,698,816 bytes、D 20,221,902,848 bytes。隐藏文档校验日志清理前后未增加空间；最新C 3,211,915,264 bytes、D 20,221,698,048 bytes。完整中文安装启动链C盘4 GiB门槛仍差1,083,052,032 bytes；不通过清理未知数据或改卷达到门槛。真实Playnite/取消/窗口失活待验。

2026-11-09 Round5 U05-01/02：任务状态仍代码完成待验收；动态静态画面和行为证据已归档。活动页重排精确SHA `25bcbf79…` 的#1423两SDK job终态success，详情见Round5 `ci-25bcbf79-final.json`。真实Playnite仍待验。当前本地SDK盘点发现`global.json`固定8.0.423，`dotnet --list-sdks`仅列9.0.302，已知临时SDK路径不存在；最终本地链需恢复基线工具，不能降基线。

## 2026-11-09 Round5 U05-03/04 CI终态修正

18d73836/#1341、498dd3a4/#1342、bca60704/#1343及5ee7be00/#1345的基线与SDK10适用job均success，早先“运行中/待查”仅是旧快照。artifact/job身份见Round5 `evidence/U05-03-04/ci-run-40638353451-final.json`、`evidence/U05-04-help/ci-runs-final-20261109.json`。后续整合SHA `25bcbf79`/#1423也通过双SDK。U05-03/04仍代码完成待验收，宿主与外部UIA/IME/DPI未验；required-checks API既有403，继续远端待验。当前继续按PROGRESS推进U05-05/06。最终本地根链仍要求SDK8.0.423与C/D空间门槛。

## 2026-11-09 Round5 U05-05/06复核

两项生产实现来自969bfc20，加载/进度源文件之后在当前分支未变化。SDK8.0.423定向13/13、0 skip、双主题Render QA394图，以及共同源码提交e570f7b7的#1500双SDK成功已核对；当前与该共同源码的src/tests/scripts差异为0。任务仍代码完成待验收，真实Playnite和物理输入/DPI未核。旧.tmp/round5-u05-05-06已不存在；复核没有删除数据。检查时C 3,102,400,512 bytes，距4GiB约1,192,566,784 bytes；D 20,184,997,888 bytes。继续U05-07/08，最终根入口门槛未达时不运行。

## 2026-11-09 Round5 U05-07/08复核

页面离开收尾与31次导航/主题/Popup/详情压力行为保持不变；6份TRX哈希已核对，最终7/7、0 skip，三份旧失败诊断保留。精确代码/测试共同SHA e570f7b7的run #1500双SDK成功，且当前src/tests/scripts无差异；真实Playnite、外部UIA/键盘/IME/物理DPI及长期Worker仍未验。旧Render QA报告生成于969bfc20、WorkingTreeClean=False；历史 `.tmp` 截图目录现已不存在，没有本批删除。当前HEAD 819f99fc的#1514两job在编译/测试，#1513基线job打包、SDK10成功；required checks仍远端待验。R:当前无映射/挂载，未触及C/D或盘符。下一批U06-01/02专门复核时间与末列文字完整处于选框内，不能恢复旧“今日工作台”架构。复核时C空闲3,105,464,320 bytes（差4GiB约1,189,502,976），D 20,183,764,992 bytes；根入口未满足空间与SDK门槛。

## 2026-11-09 Round5 U06-01/02复核

共享样式和S01密度满足U06-01，无重复密度控件。U06-02当前U09-05 Light/Dark截图中，Overview时间戳、Save History末列和Task Grid详情均位于选框内；行为11/11、R10追补3/3，资源字典140通过、39条撤销架构断言NotExecuted不计通过，Render QA 394图/0 PROBLEM。精确共同源码e570f7b7的GitHub #1500两SDK job成功，当前代码无变化；合成离屏不等于真实Playnite、实体输入/IME/UIA/物理DPI验收。隐藏Source Validation exit 0，2份变更Markdown 1,013本地链接无缺失/越界，diff-check exit 0；本批创建的2个校验日志361 bytes已精确清理。C盘实测3,082,756,096 bytes（距4GiB差1,212,211,200），D盘20,182,212,608；最终中文入口仍受空间、准确SDK8.0.423和用户正常关闭Playnite门槛约束。CI快照#1513双job成功、#1514基线打包/SDK10成功、#1515双job测试中；下一批U06-03/04。

## 2026-11-09 Round5 U06-03/04复核

行内危险动作明确显示“删除”，保留键盘可达、UIA文字与稳定行参数；三份TRX哈希匹配、17/17、0失败/0跳过。任务工具栏101通过及11条撤销架构skip，媒体工具栏行为218通过及39条撤销架构skip；skip不计通过。详情入口、主要媒体筛选/批量动作和多选摘要保持可达，低频操作留在既有设置区/页脚，滚动条与真实命令不变。2026-10-30局部前后图不作为当前整页布局验收。当前src/tests/scripts与e570f7b7无差异，#1500双SDK全步骤成功并覆盖当前测试、Render QA和WPF静态审查；真实Playnite/UIA/实体键鼠IME/物理DPI未验。无代码改动或本地新测试，本批仍代码完成待验收。C盘3,091,267,584 bytes，差4GiB 1,203,699,712；D盘20,181,245,952。#1513双job成功，#1514基线打包/SDK10成功，#1515基线测试/SDK10成功，#1516双job测试中；下一批U06-05/06。

本批文档Source Validation exit0，6份Markdown共1,012条本地链接无缺失或越界，diff-check exit0；临时校验日志2份/361 bytes已按仓库边界清理，C/D前后分别3,092,672,512 / 20,181,188,608 bytes。

## 2026-11-09 Round5 U06-05/06复核

多选锚点最终17/17、0 skip，有限视口补测3/3；旧15通过/2失败夹具TRX保留。清筛选只清筛选、不触发刷新；Error Retry触发刷新且保留旧数据。当前代码与e570f7b7一致，#1500双SDK完整workflow、Render QA及WPF静态成功；真实Playnite、外部UIA、实体输入/IME/DPI未验。无源码改动或本地新测试。#1513成功，#1514/#1515基线打包与SDK10成功，#1516/#1517双job测试中。C盘3,087,306,752 bytes、离4GiB差1,207,660,544；D盘20,179,832,832。下一批U06-07/08。

本批文档Source Validation exit0，6份Markdown共1,010条本地链接无缺失/越界，diff-check exit0；日志2份/361 bytes按仓库边界清理，C/D前后分别3,085,688,832 / 20,179,759,104 bytes。

## 2026-11-10 Round5 U06-07/08复核

高逻辑DIP上限已移除，保留紧凑窗口收缩和可达性；20k列表受控窗口行虚拟化结果有界，Media Inbox Standard例外保留。Detached scaleprobe 44不作为生产容器/帧率结论。R18当前TRX解析1/1，当前e570f7b7的#1500双SDK全量测试、静态审查及394图Render QA成功。归档files.json记录Git LF规范化前的原始文本字节，当前有3个临时日志缺失、19项工作树字节/hash不同；不把它当当前checkout逐字节证明。无产品改动、本机不重复build/test/render；真实宿主/4K/DPI/输入和长期性能待验。#1513/#1514双job成功；#1515基线打包/SDK10成功；#1516/#1517/#1518运行中。C盘3,098,148,864 bytes、距4GiB差1,196,818,432；D盘20,177,862,656。下一批U07-01/02。

本批文档门禁Source Validation exit0；6份Markdown共1,013条本地链接无缺失/越界、diff-check exit0；2份自建日志/361 bytes已按仓库边界清理，C/D前后分别3,097,378,816 / 20,177,711,104 bytes。

## 2026-11-10 Round5 U07-01/02复核

当前共享对话框保持真实影响文本、独立滚动、可换行动作和安全确认键盘语义。10/10最终TRX hash匹配，9/10旧失败保留。当前e570f7b7由#1500双SDK全workflow覆盖；旧双主题双尺寸图非当前宿主验收。真实长正文、UIA/IME/物理键盘/DPI未验，未执行恢复/删除/云写入。#1513/#1514成功，#1515基线打包/SDK10成功，#1516-1519测试中。C盘3,113,365,504 bytes，距4GiB差1,181,601,792；D盘20,176,363,520。下一批U07-03/04。

本批文档门禁Source Validation exit0、6份Markdown相对链接缺失/越界均0、diff-check exit0；仅生成并精确清理仓库.tmp内3个校验日志共1,278 bytes。C盘校验前/后3,113,865,216 / 3,117,977,600 bytes，D盘均20,176,359,424 bytes；不声称小日志带来空间回收。

## 2026-11-10 Round5 U07-03/04复核

当前共同源码e570f7b7的Tooltip下锚/显式位置/屏幕底边翻转与菜单必要列提示/UIA/循环导航保持；现有真实WPF边缘测试6/7、另1项系统指针opt-in skip，菜单5/5。精确源码#1500基线及SDK10全workflow成功。程序化Popup焦点探针不冒充实体方向键；Playnite、Narrator、IME、混合DPI待验。无源码改动。文档提交#1520 `42653667427`双job仍运行；#1519 `42651670425`双job仍运行，#1515/16/17/18有基线job运行；required-checks远端待验。继续U07-05/06。

本批Source Validation exit0；6份Markdown 1,013条本地链接缺失/越界0，diff-check exit0。3个本批.tmp校验日志/1,278 bytes已精确清理；C/D为校验前3,109,629,952 / 20,174,999,552、校验后3,109,355,520 / 20,174,999,552 bytes；不声称释放了空间。


## 2026-11-10 Round5 U07-05/06复核

通知卡片以受限真实事件源显示来源，旧记录不猜测；标题、来源、摘要、时间层次保留，长标题完整路径为Tooltip/UIA/详情。Toast最多四条、最新优先，焦点暂停计时，关闭中间项的退出动画保持队列顺序并回收计时器。历史浅深主题图是1040×700 DIP离屏图；当前Playnite Toast遮挡关系、数据刷新、外部UIA/实体输入/DPI待验。共同e570f7b7的#1500双SDK全workflow成功，当前src/tests/scripts无差异。#1521 run `42654669423`、#1520 `42653667427`、#1519 `42651670425`仍运行，#1515-1517的SDK10成功、baseline job运行；required checks远端待验。准确SDK8未装（仅SDK9.0.302），C盘3,109,355,520 bytes差4GiB 1,185,611,776，D盘20,174,999,552。继续U07-07/08。

本批文档门禁：Source Validation exit0，6份Markdown 1,025条本地链接缺失/越界均0，git diff --check exit0；只删除仓库.tmp/r5-u07-05-06-doc-check-20261110内3个本批日志共1,278 bytes，路径边界与重解析点已核对。C/D校验前3,106,439,168 / 20,173,574,144 bytes，校验后3,106,230,272 / 20,173,574,144 bytes；不把空间变化归因于临时日志清理。#1518 SDK10 job 129271907547已成功、基线job 129271907043仍运行；#1519/#1520/#1521双job仍运行。


## 2026-11-10 Round5 U07-07/08复核

设置依赖说明常驻在自动恢复开关旁，关闭时保留参数，字段禁用不隐藏提示。错误恢复按权限/离线/传输/未知远端结果分类；未知状态须先核对提交是否被接收，不鼓励盲重试。定向恢复10/10、0 skip，当前共同源码e570f7b7的#1500双SDK完整workflow成功。无真实恢复、媒体删除、云端写入；真实Playnite/UIA/实体输入/IME/DPI待验。最新文档CI #1522双job运行、#1521/#1520/#1519双job运行；#1518基线仍运行、SDK10成功。当前只有SDK9.0.302，C 3,105,419,264 bytes距4GiB差1,189,548,032，D 20,172,206,080。继续U08-01/02。

本批文档门禁：Source Validation exit0，6份Markdown共1,019条本地链接缺失/越界均0，git diff --check exit0；仅生成后精确删除仓库.tmp/r5-u07-07-08-doc-check-20261110内3个日志共1,287 bytes，先核对仓库边界和无重解析点。C/D校验前3,107,606,528 / 20,172,136,448 bytes，校验后3,107,074,048 / 20,172,136,448 bytes，不将空间变化归因于小日志清理。最新已查询的#1522 baseline/SDK10 job仍运行，远端未通过也未失败。


## 2026-11-10 Round5 U08-01/02复核

U08-01/02旧10月30日存档页图右端受水平视口裁切，不再用来证明选框边缘。11月9日U09-05当前浅深主题截图已复看：概览任务时间、存档右侧徽标和省略文本完整位于选框内；水平滚动/列虚拟化保留。选框行为11/11；概览眉题与状态/下一步保留，不恢复“今日工作台”架构；没有恢复存档。当前e570f7b7由#1500双SDK完整workflow覆盖，真实Playnite/UIA/输入/物理DPI待验。

#1523 run `42657117177`（baseline 129298460731 / SDK10 129298460272）、#1522与#1521双job运行，#1520/#1519运行；#1518 baseline运行、SDK10成功；#1515成功。GitHub required-checks远端待验。本机只有SDK9.0.302，C 3,105,419,264 bytes差4GiB 1,189,548,032，D 20,172,206,080。继续U08-03/04。

本批文档门禁：Source Validation exit0；6份Markdown共1,027条本地链接缺失/越界均0；git diff --check exit0。只生成并精确清理仓库.tmp/r5-u08-01-02-doc-check-20261110内3个本批日志共1,278 bytes，确认仓库.tmp边界及无重解析点。C/D校验前3,101,450,240 / 20,170,543,104 bytes，校验后3,100,766,208 / 20,170,543,104 bytes，不将空间变化归因于日志清理。


## 2026-11-10 Round5 U08-03/04复核

TaskCenter状态替换期间行高稳定、阶段/进度和长名UIA有15/15行为证据；媒体缺图/录像/丢失/损坏提示与恢复清除同组覆盖，2,000项列表检查滚动/追加/替换/选择边界及三档逻辑缩放。双主题历史图占位文字可读；11月9日当前任务表选框复测确认详情省略完整在框内，水平滚动保留。共同e570f7b7 #1500双SDK全workflow成功，当前源码无差异。未删除媒体；真实Playnite/UIA/输入/DPI待验。#1524/#1523/#1522/#1521/#1520双job运行；#1519/#1518 baseline运行、SDK10成功。继续U08-05/06。

本批文档门禁：Source Validation exit0；6份Markdown 1,030条本地链接缺失/越界均0；git diff --check exit0。仅生成并按仓库.tmp边界核验后精确删除3个校验日志共1,300 bytes，路径无重解析点。C/D校验前3,100,958,720 / 20,168,994,816 bytes，校验后3,100,295,168 / 20,169,060,352 bytes；不将空间变化归因于临时日志。


## 2026-11-10 Round5 U08-05/06复核

工具启动期间有局部忙碌状态及UIA文案，成功/失败/取消后清除；不能跟踪进程的关闭选项仍禁用并说明原因。维护页保留摘要、分层详情和全摘要复制，1160×520 DIP窗口可滚到摘要/操作区；定向5/5、0 skip。历史双主题目标页8/8 OK，但其它全矩阵既有Media/侧栏PROBLEM未隐藏。当前e570f7b7 #1500全SDK工作流成功，无真实工具启动或维护写操作；Playnite/UIA/实体输入/DPI待验。#1525基线/SDK10测试中，#1522-24双job中；#1518-21基线运行而SDK10成功。继续U08-07/08。

本批文档门禁：Source Validation exit0，6份Markdown共1,031条本地链接缺失/越界均0，git diff --check exit0。仅生成并精确删除仓库.tmp/r5-u08-05-06-doc-check-20261110内3个校验日志共1,300 bytes，已核对仓库.tmp边界及无重解析点。C/D校验前3,092,197,376 / 20,167,602,176 bytes，校验后3,092,090,880 / 20,167,536,640 bytes；不把空间变化归因于校验目录清理。CI #1525 baseline/SDK10测试中；#1524/#1523/#1522双job运行；#1521/#1520/#1519/#1518基线仍运行、SDK10成功。


## 2026-11-10 Round5 U08-07/08复核

通知空态清筛选复位三种过滤并回焦搜索，不触发已读/删除；S02游戏/任务跳转保留。八页跨页复核保留业务有别的结构：设置分类/校验、通知清筛选、任务/媒体/通知共享FilterComboBox；Trainer重复且永久折叠TextBlock已删除。跨页组16 pass/0 fail/1旧架构skip（不算通过），通知筛选15/15+视图4/4。U06R5-2911旧“今日工作台”截图从当前证据撤下。当前e570f7b7 #1500双SDK全workflow通过，真实Playnite/UIA/IME/物理DPI/跳转待验。#1526双job编译测试中，#1521-25有运行中job；#1518/#1519 baseline运行/SDK10成功，#1515双job成功。下一步U09-01剩余配对。

本批文档门禁：Source Validation exit0，6份Markdown 1,038条本地链接缺失/越界均0，git diff --check exit0。只生成并精确删除仓库.tmp/r5-u08-07-08-doc-check-20261110内3个本批日志共1,281 bytes，已核对仓库.tmp边界且无重解析点。C/D校验前3,092,963,328 / 20,166,152,192 bytes，校验后3,092,529,152 / 20,166,152,192 bytes，不把变化归因于临时目录删除。

## 2026-11-10 Round5 U09-07 清理复核

用户指出应主动释放项目无用输出后，按T01只读预览复核并单独检查所有权、路径边界、重解析点、文档引用、文件占用与相关进程。仅删除已被同一源码SHA较新完整构建替代且无文档路径引用的 `artifacts/gsc-b/e570f7b7`：726文件、169,276,352 bytes；D盘实测增加171,237,376 bytes，C盘未变。最新 `e570f7b7-detached-full-20261109`、构建诊断、安装包、引用历史构建及所有用户/其他任务TRX/TestResults保留。文档门禁后测得C盘3,085,778,944 bytes，距4GiB预检还差1,209,188,352 bytes；同一校验/清理区间C盘下降643,072 bytes，未归因于位于D盘的1,208-byte校验目录。完整安装启动链仍未运行。详细记录：[U09-07 cleanup](../design/reviews/ui-polish-round5-20261027/evidence/U09-07-cleanup-20261110/cleanup.json)。

## 2026-11-10 Round5 U09-08：80项计数与闪屏入口复核

重数PROGRESS任务行：80行、80个唯一ID、0重复，当前分布6已满足、70代码完成待验收、3进行中、1远端待验；U00-01盘点去重与U09-01证据覆盖已满足。此前U09-08的5/71汇总已校正。src/tests/scripts相对生产源码e570f7b7无差异，三个实际构建入口均存在。

95b2b4cf的GitHub run #1528（42666242561）仍在运行：baseline job 129330520820位于离屏渲染QA；SDK10 job 129330521427已success并上传artifact 13698653069（1,280,732 bytes，SHA-256 5ad273bceca589e4e4f9a7dd78586c64b3b4fc9518669b89e1c1d3cc5d4fccd0）。只记录已完成部分，不宣称整轮CI通过。当前只发现SDK9.0.302；C盘空闲3,083,395,072 bytes，距4GiB还差1,211,572,224 bytes；D盘充足。C盘项目Temp只见747-byte历史benchmark文本，不足以释放空间且保留。未删除任何用户或其他任务数据。

源码检查确认三个cmd入口隐藏PowerShell/控制台辅助进程；安装脚本仅在构建安装结束后启动Playnite。截图没有HWND/PID，仍不能将黑窗归因于控制台、Playnite或具体WPF窗口，也不声称已经消除。尚未重新运行安装启动入口。精确状态和CI身份见Round5 U09-08 progress-count.json。

### Round5 SDK缓存复核（2026-11-10）

系统级`dotnet --list-sdks`只列9.0.302；仓库`global.json`要求的8.0.423仍在`scripts/sdk-baseline.ps1`默认用户缓存`%LOCALAPPDATA%/GameSaveCenter/dotnet-sdk-8.0.423`，当前可直接选择。后续不能仅凭机器级SDK列表判断8.0.423缺失。C盘可用2,908,860,416 bytes，中文完整入口要求4GiB，仍差1,386,106,880 bytes；D盘可用20,331,986,944 bytes。没有可安全清除且能补足差额的本项目C盘临时输出。最后安装身份`e42da2a5`落后于当前UI源码候选`e570f7b7`，因此候选仍待完整安装启动链与真实宿主验收；具体SDK/cache证据见`design/reviews/ui-polish-round5-20261027/evidence/U09-08-audit-20261110/sdk-cache-recheck.json`。

## 2026-11-10 Round5 最新状态：完整链已跑，宿主闪屏仍待验

用户确认释放空间后，提交`1f97beafda1895ae1b84bc453828607157e1a29b`的中文完整入口通过SDK8.0.423 build/test/package/install/start，根进程exit0；独立`scripts/build.cmd`短路径完整构建测试也exit0。根链2032项/1974 pass/0 fail/58既有skip；独立buildcmd 2032项/1964 pass/0 fail/68 skip。安装版本及zip/pext/程序集身份已核，证据在Round5 `evidence/U09-05-root-1f97beaf-20261110/`。早期沙箱安装拒绝和超长路径三测试失败均保留为失败诊断，不拿成功覆盖历史。

当前PROGRESS的80项分布为7已满足、71代码完成待验收、1进行中、1远端待验。U09-04继续分析动效性能；U09-05根链完成但真实Playnite画面、用户闪屏窗口归因、真实输入/UIA/DPI未验；U09-06远端待验；U09-08审计项满足。用户释放后根入口C/D预检已满足；机器SDK列表虽只显示9.0.302，仓库SDK bootstrap实际解析8.0.423。

95b2b4cf的run #1528 / 42666242561现已completed/success：基线job129330520820和SDK10 job129330521427所有步骤成功；artifact IDs 13701600359、13699579982、13698653069。required-checks接口此前403，仍不代表完整性。push workflow需在本批提交后按精确SHA查询。编译/检查没有关闭或强杀Playnite/Worker；桌面探针访问被拒，不宣称闪屏已经解决。

## 2026-11-13 共享工具栏按钮忙碌提示修正

用户启动截图中的按钮下沿蓝色忙碌提示对应共享样式误把父级 ViewModel 的 IsBusy 状态套给所有按钮；截图对应的运行二进制身份未单独核验。提交 b667e22445fbf57aa591f39f3d968f5c166c7cb6 移除共享隐式忙碌绑定，保留 BusyIndicator 模板和 Dashboard/Trainer 显式动作级绑定；命令 CanExecute、按钮状态、游戏选框、滚动、Binding 与恢复安全语义未改。这是独立用户反馈，不计入抽查报告16项或Round5/S/T缺陷。

同一SHA SDK8.0.423 / Release 中文一键链成功走过构建、测试、发布、打包、隔离安装及Playnite启动分支；TRX 160份、2,043项、1,985通过、0失败、58既有skip，157/157个WPF隔离类。命令日志打印完成成功分支；宿主等待包装器未返回数值，因此不把包装器状态冒充为独立捕获的cmd退出码。独立 scripts/build.cmd 实际exit 0，160份TRX同为2,043项、1,985通过、0失败、58 skip。R02BusyStateTests 两次均6/6。来源/静态检查通过；真实Playnite视觉渲染未复验。

精确SHA GitHub run 43375661710：基线job 131721452745 与 SDK10.0.401 job 131721453027 全部步骤success。required-check/ruleset名单仍为既有HTTP403，权限未变，不重试。诊断与原始证据留在本地 artifacts/build-diagnostics/20261113T060254846Z-d6b80c0d/ 和 artifacts/busy-b667e224/；CEF Chrome注册表告警保留在 console.err.log，不影响完成分支。

临时测试目录 `.tmp/busy-b667e224-tests`（32文件，12,278,656 bytes）与 `.tmp/busy-b667e224-scripttests`（15文件，6,451,200 bytes）经路径、归属、重解析点、进程和文档引用检查后，`Remove-Item -LiteralPath -Recurse -Force` 被执行策略以 `blocked by policy` 拒绝；两目录仍在，未绕过。清理尝试前C/D空闲为5,159,657,472 / 7,047,708,672 bytes，拒绝后复测为5,170,311,168 / 13,758,140,416 bytes。目录未删，期间空间变化不能归因于清理。构建输出、隔离profile、安装包和成功诊断保留；用户TRX/TestResults、配置及真实存档未触碰。

## 2026-11-13 P1收口第一批：A01/A05/A06

按Round5与S/T账本去重后，A01、A05、A06的本地可执行范围由`b667e22445fbf57aa591f39f3d968f5c166c7cb6`当前证据更新。中文完整入口与独立`scripts/build.cmd`均使用SDK8.0.423 / Release完成160份TRX、2,043项、1,985通过、0失败、58 skip及157/157个WPF隔离类；skip不计通过。安装六个程序集身份对应完整SHA，Playnite与Worker仅在一次性profile启动并已关闭。旧`c2fcc804`独立build输入桌面失败继续作为历史原始失败保留，但不再代表当前门禁状态。

A05的原始soak/定向失败与A06的合成重启、PreRestore身份、锁定/hash和终态同步测试继续有效；没有执行真实恢复、删除或云写。A03仍并入U09-04，A04仍需真实可见宿主、物理输入和呈现数据；本批不解除两项宿主阻塞。精确路径与哈希见[P1本地收口](../design/reviews/project-assessment-2026-11-06/evidence/P1-close-b667e224-20261113/README.md)。

## 2026-11-13 P1收口第二批：A07/A08/A11

A07复用RequestId ledger、重启中断、响应丢失重放和取消歧义覆盖；当前完整门禁验证相关程序集与测试，隔离Worker仅验证进程启动，不宣称真实IPC业务交互。A08的预检后磁盘满合成故障仍为1/1通过，当前完整入口未执行恢复或填充真实卷。A11的证据路径与重复大blob预检继续生效；`b667e224`的本地双入口和GitHub基线/SDK10.0.401两个job均成功，required-checks名单仍为既有403。

至此A01、A02、A05、A06、A07、A08、A11的P1本地或既有证据范围已收口。A03继续并入Round5 U09-04，A04继续等待真实可见Playnite、物理输入和呈现帧数据；不以隔离启动或离屏代理关闭。下一批可进入P2的A09/A10去重与受控验收，真实profile、恢复、删除和云写仍不在授权范围。

## 2026-11-13 P2第一批A09/A10去重

A09现有入口由维护中心环境检查/测试备份和存档页匹配、校验、readiness、恢复工作流组成；当前精确SHA的分段测试均通过，但onboarding源测试含2个既有NotExecuted且没有一次性配置下的同会话完整闭环。A10当前精确SHA已有reduced soak、2,000游戏集合、任务索引上限、历史分页、缩略图预算和选项虚拟化证据；Worker full档未在当前SHA启用，历史full不替代当前源码。

本批没有修改产品或测试，也没有启动Playnite。A09下一步先设计仅写临时目录的虚拟游戏/存档夹具；A10下一步以低优先级隐藏进程定向运行当前SHA full档，不重跑整套构建。真实宿主首次使用与代表规模仍单列待验。详见[A09/A10审查](../design/reviews/project-assessment-2026-11-06/evidence/A09-A10-audit-20261113/README.md)。

### A10 full档补验

文档HEAD`2b084c6a2b48fc2234c0a71e3e828fcd916b1715`相对产品源码`b667e224`只增加文档。SDK8.0.423 / Release、隐藏BelowNormal进程定向`SoakDataScaleTests` full档exit0、1/1通过、0 skip：2,000游戏、20,000备份、10,000任务、5,000媒体、500工具；造数138,568ms、模拟472ms，增长摘要`managedGrowth=0 MiB, handles+0, threads+0`，订阅与临时残留均0。A10后台规模范围收口，真实Playnite代表规模仍待条件。

原始TRX、控制台与结构化报告在本地`artifacts/a10-full-2b084c6a/`，hash见[结构化记录](../design/reviews/project-assessment-2026-11-06/evidence/A09-A10-audit-20261113/a10-full-result.json)。`.tmp/a10-full-2b084c6a`的17文件/5,827,456 bytes经边界、归属、链接、占用和引用检查后，指定PowerShell删除被策略拒绝；目录仍在，未绕过，空间变化不归因于清理。

### A09安全夹具设计结论

恢复侧已有`IRestoreCatalog`/`IRestoreClient`与唯一临时根，分段隔离验证可继续复用；备份侧`BackupOrchestrator`直接依赖具体`LudusaviClient`，`DashboardViewModel`又由真实插件会话构造，现有代码没有无需生产重构即可完成全链的同会话测试缝。本阶段不为测试拼接改生产DI，也不运行真实Ludusavi、profile或存档。A09保持分段覆盖已核、宿主闭环待条件；本结论不确认产品缺陷。

## 2026-11-13 A14–A16功能候选去重

A14版本详情已覆盖时间、来源、文件/大小、保护、readiness、哈希与检查时间；单版本远端/隔离恢复证明没有可靠契约，因此不拼游戏级云状态或安全分数。A15的诊断包与健康报告已覆盖生成前脱敏预览、软件/构建身份、失败/健康分段和安全导出，现有能力满足。A16的元数据包manifest/schema/SHA/entries、恢复前副本和路径迁移冲突预览已存在；没有具体跨设备失败，不新增平行向导。三项入口/缺口/收益见[A14–A16审查](../design/reviews/project-assessment-2026-11-06/evidence/A14-A16-audit-20261113/README.md)。
