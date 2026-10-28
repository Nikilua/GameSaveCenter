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

8c927a93cff92b923a8efbedbdf28aadbc10d00b已推送。共享按钮模板现状已盘点，五态、忙碌稳定和图标目标已有生产能力；本批未改生产资源，只加生产图标动作UIA名称/命令与键盘焦点的回归断言。SDK8.0.423精确SHA聚焦13/13、0 skip；证据见docs/design/reviews/ui-polish-round5-20261027/evidence/U03-01-03/。run 40561836233的baseline 122392618285与SDK10 122392618705仍在编译测试，任务状态为“代码完成待验收”，不可记为CI通过。旧S/T宿主或权限待验不阻断接续U03-04/05。真实Playnite/UIA/物理DPI未验；整轮中文根入口完整链、独立scripts/build.cmd和最终CI仍待候选提交后完成。

## Round5 当前进度（2026-10-28）：U03-04～07

`d1fc1308317fb805d61d67a7dda77d71e0afff18`和`5300a2b5c70f7736f1f46617667de73749b8d883`已推送。U03-04/05改共享TextBox禁用/验证错误语义和搜索区域源码回归，SDK8.0.423隔离32/32，XAML26/26、WPF静态37文件/0 error、Render QA双主题394 PNG/0 PROBLEM。U03-06/07通过去重复用既有ComboBox与CheckBox模板，仅加长选中项/箭头分栏源码守护测试，专项13/13。精确TRX、报告与摘要在Round5 evidence/U03-04-05及U03-06-07。CI run `40566121765`和`40567683673`截至19:21 UTC均未完结；更早run `40561836233`baseline仍在打包，SDK10成功。真实Playnite/实际UIA/鼠标/实体DPI待验。临时Render QA删除请求被执行策略拦截，未换Shell，见checks/cleanup.json。旧S/T权限待验不阻止继续独立控件批。整轮最终根入口构建安装启动、独立scripts/build.cmd和所有候选CI门禁未完成。

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
