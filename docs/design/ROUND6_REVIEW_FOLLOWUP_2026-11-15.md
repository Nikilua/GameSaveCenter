# 第六轮交付复核与四项收尾（2026-11-15）

审阅HEAD c7bdeb16；产品候选9880c81b。结论：页面和动效实现、受控效果交付与构建门禁已有实质成果；“28项全部无保留完成”不成立，V20/V25/V26/V28自己仍列未验或受阻条件。无需重新做第七轮大规模改造，先完成以下有限收尾。

## 实际复核

读取启动文档、28项计划和V21–V28交付索引；比较9880c81与HEAD的src/tests/scripts无差异。11张推广对比图的SHA全部匹配manifest；中文根链与独立build的原始summary SHA均匹配本地门禁JSON。归档结果为两入口直接exit0、各2051项/1993通过/0失败/58 skip；GitHub双SDK通过为归档记录，本轮未实时查询CI或重跑构建。

实际查看任务页1600×900与概览1040×700前后图：任务表空间扩大、游戏名和详情列可见性改善；概览主操作与摘要横向组织后腾出首屏高度。这些是可见的改造，不只是改文档。但任务对比图详情“耗时”左36小时、右38.2小时，证明至少该字段并非固定同一时间状态。尚未定位是测试时钟、刷新逻辑还是数据生成的原因，不能直接认定生产计时有bug。

V20交付的是带时间戳的WPF属性采样及播放器，符合计划允许的替代形式，但详情IsVisible=false，不能证明屏幕上的流畅程度。V25只有1.00逻辑DIP的对照，不涵盖全部缩放；V26明确managed allocations未测。以上不应被总括为“全验收完成”。V28清理被策略拒绝，保留受阻事实，不绕过。

## 仅增加四项明确收尾

| ID | 对应旧项 | 任务与完成标准 | 初始状态 |
| --- | --- | --- | --- |
| C01 | V04/V12/V21 | 修正任务页对比的时间一致性：定位Elapsed展示的数据源，固定所有动态时间和进度，重拍同主题/同尺寸前后图；每次两版的样本字段快照应相同。保留旧图作为已知限制，不改生产计时只为让截图一致。顺便检查“执行中/阶段已完成/任务已完成”的合成样本组合是否有业务解释，无解释时改合成数据。 | 受控证据与`aa590a1b`精确SHA双job CI均完成；身份见[最终门禁记录](ui-redesign-round6-20261113/evidence/final-candidate-gates-20261115/README.md) |
| C02 | V26 | 在现有受控Harness补页面操作managed-allocation配对：同机、同数据、同操作数、预热后正反序，记录总分配/操作、样本与噪声；区分冷启动和稳定交互。若重复显示回退，定位后再修；不把GC保留内存或Rendering间隔冒充分配量/真实帧。复用已有探针，禁止无限跑数。 | 受控分配量配对与`e5bf3fce`精确SHA双job CI均完成；身份见[最终门禁记录](ui-redesign-round6-20261113/evidence/final-candidate-gates-20261115/README.md) |
| C03 | V25 | 补受影响控件的125/150/200%缩放可读性与几何矩阵，优先任务详情/概览CTA/存档选框/弹层；使用真正对应的WPF DPI环境，若仅逻辑模拟必须标注。能做的受控检查先做，真实跨屏仍独立待验，不因物理条件缺失将已做证据抹掉。 | 受控WPF矩阵和三个精确SHA双job CI均完成；真实跨屏/Playnite现场与物理输入仍待验，见[C03证据索引](ui-redesign-round6-20261113/evidence/C03-dpi-matrix-20261115/README.md) |
| C04 | V20/V28 | 最终体验交付：在可安全使用的可见隔离WPF或Playnite窗口中录制切页/详情反转/Toast短片；明确运行身份、数据、输入方式。无可见环境则保留受控采样和待验原因，不用HTML轨迹回放冒充录屏。同步清理受阻路径清单与真实状态，不绕过既有策略；用户可直接打开效果索引看到差异。 | 条件性待验 |

## C01执行记录（2026-11-15）

初始差异来自合成运行任务缺少`ElapsedSeconds`：`TaskStatusDto.DurationDisplay`因而回退到`FinishedUtc ?? DateTime.UtcNow`。运行样本还混用了“执行中”与“已完成”消息，且时间与进度不够完整。修正在RenderHarness新增独立的8条任务对比夹具：运行/成功/失败/取消各2条，所有时间戳、耗时和进度固定，状态、阶段、消息、终止时间和错误字段保持一致。`ElapsedSeconds`已明确提供，生产计时与生产XAML未改；原始旧图保留。

同机、同SDK 8.0.423/Release、同样本时刻`2026-10-15T12:00:00Z`、深色主题、1040×700及1600×900 DIP，对比`8678e163a92278a74645559f95789c1348941398`基线与`54d7a6efa1d7438575ca5ca0ff29d098654e6754`候选。两个RenderHarness Release构建均0 warning/0 error，两次渲染直接exit 0；均运行在各自`WinSta0`私有桌面，前后输入桌面保持`Default`。任务字段快照SHA-256均为`979BD8EA59BB948F039059D8F4E920DDB289AA355A8A45741BB0A8A7392D5BBE`，两图都显示选中任务T-0001耗时36小时、48%进度、校验中。报告/原图/并排图和manifest在[C01证据索引](ui-redesign-round6-20261113/evidence/C01-task-time-20261115/README.md)。该证据只代表受控WPF离屏布局，不是真实Playnite、物理DPI或实体输入验收。

源码提交`54d7a6efa1d7438575ca5ca0ff29d098654e6754`已推送；其Actions run #1582（`43706944794`）因新PowerShell脚本缺UTF-8 BOM在源码结构检查失败，依赖步骤按workflow正常跳过，未扩大skip。缺陷已在`aa590a1b8c710e0788098b27b128719001aeead3`修复。精确SHA Actions run #1583（`43707721313`）主job `132792318458`和SDK10 job `132792318643`均已`completed/success`、全部步骤完成；artifact身份与digest见[CI与最终候选门禁记录](ui-redesign-round6-20261113/evidence/final-candidate-gates-20261115/README.md)。

清理复核只针对本阶段独有的基线工作树、候选渲染目录及成功烟测目录（分别10,332文件/552,230,312 bytes、5文件/164,264 bytes、5文件/164,261 bytes）；三者均规范化在仓库`.tmp`内，无重解析点、Git跟踪引用或占用进程。准确的PowerShell `Remove-Item -LiteralPath -Recurse -Force` 调用被执行策略拒绝，未换其他删除方式；目录保留。首次滚动控件选择错误的失败烟测诊断保留在`.tmp/round6-c01-smoke-20261115`。用户TRX/TestResults、当前包、历史证据未触碰。

## C02执行记录（2026-11-15）

`e5bf3fce3e0e410f97fde251647189aa71dc23c6`只改RenderHarness与场景入口脚本。对比前态生产源码`8678e163a92278a74645559f95789c1348941398`与产品候选`9880c81be09365d02b9335a633be95506095c8d9`；基线树只覆盖共享Harness/合成夹具/运行脚本，候选相对9880的`src/`差异为零。SDK8.0.423/Release、固定UTC `2026-11-13T00:00:00Z`、960×640 DIP、深色主题、8条同一合成任务、同机各跑24对操作，顺序B-C-C-B。每次构建0 warning/0 error、直接构建和探针exit 0；各自WinSta0私有桌面，输入桌面前后均为Default。

首次页面创建平均分配从35,679,704降至35,601,744 bytes（−0.22%）。合并两次运行、48个独立开/合样本，详情打开均值3,344,974→3,278,218 bytes，收起3,441,514→3,380,695 bytes，开合配对均值6,786,488→6,658,913 bytes（−127,575 / −1.88%）。配对样本标准差为880/1,303 bytes、P95为6,787,560/6,660,184 bytes；重复方向一致，没有看到候选分配回退。固定操作含路由Button.Click和同步`Window.UpdateLayout()`，进程启动不计入首次页面；CLR自发GC次数有记录，无手动强制GC。

详情、逐样本TSV、每次运行身份与SHA在[C02证据索引](ui-redesign-round6-20261113/evidence/C02-task-allocation-20261115/README.md)。受控按钮事件不等于实体输入；1.00 logical DIP、离开屏幕的隔离桌面不证明真实Playnite呈现帧、DWM、物理DPI或键盘/UIA体验。真实profile、游戏配置/存档和云端均未访问。首次net472编译问题、按钮视觉树初始化和正反序列终态的早期失败诊断均保留；均已修正，未跳过测试或扩大skip。

GitHub run #1585（`43711239891`，精确SHA `e5bf3fce3e0e410f97fde251647189aa71dc23c6`）的主job `132803031587`和SDK 10.0.401 job `132803031328`均已`completed/success`、全部步骤成功。C01/C02/C03五个源码SHA及artifact身份汇总在[CI与最终候选门禁记录](ui-redesign-round6-20261113/evidence/final-candidate-gates-20261115/README.md)。未探测旧required-check 403。

## C03执行记录（2026-11-15）

C03在最终RenderHarness源码`15f82b650fde8cbe370a48f8fbd6ba8978bf4dc3`上完成任务详情、概览优先CTA、存档策略游戏选框、任务状态ComboBox/Popup的双主题采样。修正提交依次为`5a6a7c192ea102338b3f339d0b3a8cbba3b86eaa`（DPI矩阵）、`159ea800becb985e9c8a8d2a3bb8dae3b112cd48`（页面捕获稳定）、`15f82b650fde8cbe370a48f8fbd6ba8978bf4dc3`（控件裁切）；均只改Harness/证据脚本，没有改生产`src/`。权威目录含38张PNG、几何TSV、报告、runner身份和hash manifest；首次裁切诊断及中间修正目录保留。细节、图片链接和hash见[C03证据索引](ui-redesign-round6-20261113/evidence/C03-dpi-matrix-20261115/README.md)。

SDK8.0.423 Release下，进程和UI线程均为PMv2；当前`DISPLAY10`实际144 DPI/150% WPF窗口样本与125/200%栅格模拟分开标注。125/200不是对应的系统/WPF DPI上下文。数据为8个合成任务/游戏，输入程序化设置、测试运行于私有WinSta0桌面，前后桌面为Default。该矩阵不代表真实Playnite视觉、跨屏、物理输入或UIA客户端验收。

## 最终候选入口门禁（2026-11-15）

在SDK8.0.423/Release、源码SHA`15f82b650fde8cbe370a48f8fbd6ba8978bf4dc3`上，中文一键入口`GameSaveCenter-一键构建安装运行.cmd`转`GameSaveCenter-Run.cmd`完整链直接退出码0；独立`scripts/build.cmd`直接退出码0。两次均160份TRX、2,051项、1,993通过、0失败、58跳过、0其他，157/157个WPF隔离类完成。根链安装六个程序集与包hash一致；Playnite只在新隔离profile启动，没有打开插件页作视觉验收，随后通过Playnite `--shutdown`正常退出。安装身份、ZIP/PEXT和六个程序集hash、原始summary SHA及第一次辅助包装器失败记录见[最终候选门禁记录](ui-redesign-round6-20261113/evidence/final-candidate-gates-20261115/README.md)。没有关闭/扩大skip，没有访问真实profile、配置、存档或云端。

四项都沿用旧计划，不另造平行功能。C01是本次明确发现的证据质量问题；C02/C03补原验收缺口；C04是交付和环境条件，不承诺由纯代码消除。C01–C03受控证据及CI门禁已收口；C04要求的可见隔离宿主短片和Playnite现场/物理输入复核仍未完成。本次整理既有运行结果，没有重跑完整链或执行现场视觉验收。

## 执行要求

读取AGENTS、当前状态及第六轮计划，先C01，再C02/C03的独立部分，每批1～2项。使用仓库WPF技能作质量依据，保持Demo-first、游戏选框/滚动条、命令Binding、虚拟化、键盘/UIA和恢复安全语义。建立每项提交/证据/状态记录；受控、真实宿主、物理输入严格分开。

源码/测试/脚本/资源改动按影响范围验证，最终候选实际中文一键入口转GameSaveCenter-Run.cmd完整链及独立scripts/build.cmd成功，记录直接退出码、SDK、源码和安装身份；核验精确提交GitHub所有适用job。不得跳过测试、扩大skip、关闭检查求绿，旧required-check403不重复探测。纯文档只做静态/链接检查，不循环追加CI成功记录。

每阶段在归属、规范化根内路径、链接、引用及占用核实后，用PowerShell literal路径清理artifacts/.tmp自有无用输出；保留当前必要包、失败诊断和引用证据，不清用户TRX/TestResults及未知内容，策略拒绝不得绕过。中文commit并push。此次只做复核、证据索引和文档计划收口；没有操作真实配置/存档/云端，没有重跑完整链或执行现场视觉验收。

## 2026-11-16 独立复核与C05定点检查

当前HEAD d58250a5。C01快照hash与记录相符，实际查看1600×900新对照图，两侧详情均36小时/48%/校验中；原时间问题已修正。两入口原始summary hash匹配，均2051项/1993通过/0失败/58跳过，160份TRX、157个WPF类。当前生产src相对9880c81无差异，src/tests/scripts/tools相对最终门禁15f82b6无差异。本轮只读复核，没有重跑全链或实时查询CI。

C02已有受控分配量结果；C03为真实150%与125/200栅格模拟，不能扩大为全部物理DPI验收。C04仍条件性待验，清理策略限制也仍保留。因此“现有受控实现基本收口”成立，“全部验收完成”不成立。

| ID | 任务 | 依据与验收 | 状态 |
| --- | --- | --- | --- |
| C05 | 任务表进度百分比裁切定点复核 | C01的task-dark-1600x900-before-after.png候选侧中，进度条右侧数字呈局部字符（成功行类似只露出10），详情区48%完整。先在原尺寸图和生产WPF视觉树确认是否为百分比裁切、列宽/模板问题或截图因素，不能未经确认声称根因。若可复现，修共享单元格布局：0/9/48/99/100%完整可读，窄列下可明确隐藏数字并保留完整可访问文本，禁止残留半个数字；不改进度值、选框或滚动系统。覆盖双主题、常用宽度及100/150%适用环境，同数据前后图并验证横向滚动。无生产问题则记录排除证据，不制造改动。 | 已完成；提交 `1b462d73` 两种本地完整入口和 GitHub 双SDK jobs通过；C04真实宿主与物理验收仍待条件 |

C05沿用上方构建、精确CI、隔离数据和安全清理门禁，现已完成。C04保持原编号，不重复派发；后续以实际使用反馈驱动改造，不继续按数量加任务。

## 2026-11-16 C05实施预记录

生产任务表在受控 WPF 视觉树中证实裁切，不是截图缩放误差。基线进度列 `Width=112/MinWidth=100 DIP`；单元格水平内边距合计24 DIP、模板水平Margin合计16 DIP，进度条与数字列最小宽度合计92 DIP，基线最小总需求约132 DIP。1040 DIP 测试窗里 `0%` TextBlock 边界为 `[96,112]`，可用内容是 `[12,100]`，右侧超出12 DIP；0/9/48/99/100均有越界。截图、基线几何和输入桌面身份见[C05基线证据](ui-redesign-round6-20261113/evidence/C05-task-progress-20261116/README.md)。

修复把列默认宽度设为152 DIP、最小宽度148 DIP；数字预留44 DIP，进度条预留64 DIP。新增生产视图回归检查并扩展RenderHarness定点场景。SDK8.0.423/Release预提交下，RenderHarness build 0 warning/0 error；修复前矩阵按预期直接exit1，双主题的800/1040/1366/1600 DIP共16图，并报告12 DIP越界；修复后矩阵exit0，五种百分比全部位于单元格填充边界内，UIA HelpText逐项等于完整显示值。固定样本SHA `51B8F8DA60DB737D04560CB69D79D9B2C538E9CEBAA1C67EBBDCF7DE8F413E4A` 前后相同。150%来自真实当前显示器WPF Window的144 DPI读数；PNG栅格为96 DPI逻辑渲染，未将其说成真实100% DPI。800 DIP下横向滚动到末尾成功，选中任务保持，行/列虚拟化保持启用。

定向 `R06TaskProgressBehaviorTests` 修复前 `6 passed / 1 failed / 0 not executed`，失败正是新边界断言；修复后 `7 passed / 0 failed / 0 not executed`，WinSta0私有桌面，Default输入桌面前后不变。Source Validation通过；XAML结构校验27份通过；WPF静态审查0 error、26 warnings、164 info（与生产项目既有审查基线一致）。脚本和XAML以外的游戏选框、滚动系统、绑定/命令、恢复安全语义均未改。以上为预提交实施记录；精确提交后的最终状态见下方。

## 2026-11-16 C05精确提交完成记录

源码提交 `1b462d7302d90d5bdbd3195fbf64d7a2e2d22c06` 已推送。修复后受控矩阵双主题×四窗口通过，五种百分比均处于单元格填充区内，UIA HelpText完整，800 DIP横向滚动、选中稳定及行列虚拟化均保持。定向R06为7/7、0失败、0跳过；实际WPF Window是144 DPI/150%，无真实100%显示器。图像、runner、报告、固定夹具和TRX的hash见[C05证据索引](ui-redesign-round6-20261113/evidence/C05-task-progress-20261116/README.md)。

精确源码SHA的中文入口→`GameSaveCenter-Run.cmd`完整链与独立`scripts/build.cmd`均使用SDK8.0.423/Release直接exit 0；各2052项、1994通过、0失败、58跳过、160份TRX、157个WPF隔离类。安装版本0.6.73，六个程序集与打包目录SHA相同。GitHub Actions run #1592 (`43787032807`) 主Windows job `133036868868`与SDK10 job `133036869124`均completed/success，详见[candidate-gates.json](ui-redesign-round6-20261113/evidence/C05-task-progress-20261116/candidate-gates.json)。没有扩大skip或关闭检查。

完整链只在本仓库新建隔离profile启动Playnite，未打开插件页；Host/Worker仍运行，CloseMainWindow返回false，故保留其profile且不强杀。另有12个本轮自有scratch目录通过路径/重解析点/引用/占用预检，但指定PowerShell `Remove-Item -LiteralPath -Recurse -Force` 在shell启动前被策略拒绝，未绕行，目录保留；清理记录及逐项路径见[candidate-gates.json](ui-redesign-round6-20261113/evidence/C05-task-progress-20261116/candidate-gates.json)。未操作真实profile、游戏库、配置、存档、云端、实体输入或真实屏幕呈现。C04继续条件性待验；这一受控结论不代表真实Playnite视觉或物理输入通过。
