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
