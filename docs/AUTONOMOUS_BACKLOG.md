# 无人值守开发积压清单

## 2026-10-22 S00-06 审阅转交已受控复核

源码30e32794新增恢复/保留清理审阅项，仅导航到既有流程并复核捕获游戏/导航可用性；受控WPF回车只导航一次，危险/预览命令哨兵0次，版本对象与锁/PreRestore不变。SDK8.0.423 Release编译0/0，7个独立测试进程36/36、0 fail/skip，原始与脱敏TRX/SHA已归档。[证据](design/reviews/ui-finesse-round4-20261018/evidence/S00-06/README.md)。两个空临时目录在成功进程查询后清理，保留最新构建和引用证据，用户旧TRX/TestResults未动。

Round4现为40已验收、2已满足、19代码完成待验收、2待开始、1外部阻塞；S00-06真实页面/物理输入待验。S00-07排版独立READY，S00-08随后。最新默认完整链仍是截图修复4f40e016的1908项/0失败；S00-06～08最终候选须重新跑默认.cmd，不把旧全链或受控输入冒充新源版本/真实宿主证明。

## 2026-10-22 用户截图修复与标准全链通过

共享DataGridCellsPresenter纵向补偿改为Both，消除文字相对选中行累计漂移；任务表格填满有限父Grid，筛选水平整组换行，表头单行省略；任务/存档/媒体详情固定圆角外框、480DIP工作区宽度，媒体宽详情独立于左页滚动。并修复详情加宽后的批量工具栏错排及摘要居中。

源码`4f40e016`默认中文入口→根.cmd完整退出0（SDK8.0.423/Release）：XAML25/25、solution0/0、1908项（1851 pass、57原有skip、0 fail/error/timeout/abort），140个WPF类、143份TRX一致；发布、打包、安装版本/四个关键DLL SHA、Playnite PID51232与Worker PID31176启动均已核对。同提交独立RenderHarness退出0、0 PROBLEM；原失败保留。[表格详情完整证据](design/reviews/UI-TABLE-DETAILS-REPAIR-20261022/STANDARD_BUILD.md)。

归档固定工作树前保留并校验全部诊断、最新构建/安装包和当前审计；清理本阶段39个旧目录，逻辑501429814字节，工作树逻辑1629890777字节另由托管归档回收；物理空间见证据JSON。用户旧TRX/TestResults未动。真实页面交互/物理输入仍未验；Round4计数40/2/18/3/1不变，T00-01复用本次证据记已满足。下一独立实现S00-06危险动作只转交审阅流程，S00-07/08随后；真实宿主待验不阻止已授权独立实现。

## 2026-10-22 后续专项计划：构建成功与安全清理

按用户要求新增[32 项后续任务](design/BUILD_UI_RELIABILITY_32_TASKS_2026-10-22.md)及[专项账本](design/reviews/build-ui-reliability-20261022/PROGRESS.md)。规划审阅基线 a5c96c8a；第四轮仍为 40 已验收、2 已满足、18 待验收、3 待开始、1 外部阻塞，不以新计划覆盖旧状态。T00-01已复用本次4f40e016完整默认链证据记已满足；其余T项仍为PROPOSED，按现有实现去重后逐项准入，不重复本次全链记录。

可复制提示词已明确：标准入口退出码/测试/包安装身份与启动验证；每阶段归档证据后检查路径、所有权、进程及引用再清理 artifacts/.tmp 无用产物，保留用户旧 TRX/TestResults。此次只更新文档，运行任务ID/数量/链接及源码校验，不执行产品构建、不宣称当前全链已通过，不为纯文档 CI 成功追加提交。


## 2026-10-18 第四轮执行状态

### 最新状态（2026-10-22）

当前 main 源码提交 `5974742f4bea03d4975ff3a447eabe13d52a0e0e` 的根目录 `GameSaveCenter-Run.cmd` 已完整退出 `0`：SDK8.0.423/Release、solution 0 warning/0 error、1,906 项（1,849 pass、57 skip、0 fail/error/timeout/abort）、143 份 TRX 计数一致、140 个 WPF 隔离类完成、R08 3/3、R09 2/2；Worker 发布、构建身份、PEXT/ZIP、安装和 Playnite 启动均成功。诊断 run `20261022T012309056Z-3ccfe8e5`，包含首次 R08 testhost 挂起与完整复跑记录，见 [S00-05 标准验收](design/reviews/ui-finesse-round4-20261018/evidence/S00-05/STANDARD_BUILD_20261022.md)。本次验证不改变 Round4 host verification 状态。

2026-10-22 S00-05 上下文预览已在 `5974742f` 实现并受控验证：打开面板捕获游戏/任务稳定 ID，选择变化或对象失效时禁用旧命令；命令目录定向测试 `21/21` 通过。SDK8.0.423/Release 根目录 `GameSaveCenter-Run.cmd` 完整退出码 `0`，XAML `25/25`、solution `0/0`、Core `157/157`、Worker `400/400`、Playnite source `567`（549 pass、18 skip）、WPF 隔离 `782`（743 pass、39 skip，140 类），总计 `1,906` 项（1,849 pass、57 skip、0 fail/error/timeout/abort）。R08 `3/3`、R09 `2/2`，Worker 发布、ZIP/PEXT、扩展安装身份及 Playnite 启动成功。首次完整尝试在 R08 testhost 挂起；同一测试 DLL 独立复跑两次及之后的完整根命令通过，首次 run 未覆盖。诊断 `20261022T012309056Z-3ccfe8e5`、产物哈希和原始日志位置见 [S00-05 标准验收](design/reviews/ui-finesse-round4-20261018/evidence/S00-05/STANDARD_BUILD_20261022.md)。真实 Playnite 页面交互、物理键盘和 IME 候选仍待宿主验收；S00-06～08 尚未开始，Round4 整体未完成。

Round4 S05-01 至 S05-08、S06-01 至 S06-08 已受控验收。S06-06 `fb33cd11` 按稳定 ID 刷新当前索引摘要；用户滚动截图后的 `a1a86e41` 让有效页面 ScrollViewer 测量高度限制表格预算。S06-07 `96a3eab3` 增加只读脱敏待办预览，SDK9 Release solution `0/0`、行为/UI/复制链 `13/13`。S06-08 `09ec3c84` 补原生 Worklists JSON 重载、旧字段默认和损坏清单恢复回归，Playnite.Tests Release Rebuild `0/0`、跨会话/既有设置回归 `27/27`。证据见 Round4 S06-07/S06-08 页面。SDK8.0.423 已用于当前提交的完整 Release 标准链；S00-03、S00-04 均已完成代码实现和受控测试。此前 R05 `GetCursorPos` Win32 error 5 是旧独立 UI 门禁记录，不代表本次全链结果；当前 `GameSaveCenter-Run.cmd` 已完成 140 个 WPF 隔离类、安装验证和 Playnite 启动。真实 Playnite 页面操作/视觉与物理键盘、IME 候选仍待验。S07-03 仍因 required-check/branch-protection 读取权限不足而阻塞；S02-04 至 S02-08 的 SDK8 复核不改变其他宿主验收边界。

2026-10-21 当前源码 SDK8 复核：官方 `scripts/build.ps1 -SkipTests` restore、XAML 25/25、Release solution 0/0；S02-04 至 S02-08 的通知/上下文定向测试通过，详见[共享证据](design/reviews/ui-finesse-round4-20261018/evidence/SDK8-S02-RECHECK-20261021/README.md)。S02-04 的 `controlledVerificationState` 已更新为 passed，S02-04 至 S02-08 的任务状态仍为“代码完成待验收”，hostVerification 仍 pending。用户滚动区域当前源码几何复核 7/7；Q14 紧凑搜索行断点调整为 1500 DIP，维护中心相对时间回归 32/32，详见[本地 UI 复核](design/reviews/ui-finesse-round4-20261018/evidence/LOCAL-UI-RECHECK-20261021/README.md)。离屏 STA/WPF 不是真实 Playnite 屏幕证据。

S04-05 `4d0bfd3c` 已受控验收并推送：锁定 A，只在当前已加载且经过搜索/筛选的有限媒体结果中切换 B，保留详情焦点；坏图会被跳过，复用 `AsyncThumbnailImage` 的迟到回调保护。S04-06 `792b1a2f`、S04-07 `14925e5e`、S04-08 `c9da7c76` 已完成受控验收：比较会话意见可撤销，转交使用稳定 ID 并复核，缩略图沿用 2048 px 解码/64 MiB 缓存边界。S04-08 SDK8.0.423 Release solution `0/0`，缩略图及生产比较页行为 `19/19`。真实 Playnite、真实媒体和物理 DPI 未验；详细证据见各项 Round4 页面。另按用户新反馈修正工作区滚动视口预算，SDK8 Release solution `0/0`、布局回归 `6/6`；2026-10-21 依三张宿主截图又修正根裁切、表格扣除页面工具区及小窗放大后的高度恢复，SDK9 Release solution `0 error`、布局回归 `6/6`、RenderHarness `render-qa OK`。证据见 `design/reviews/UI-SCROLL-LAYOUT-REPAIR-20261020/README.md`。

用户已明确要求完成[第四轮 64 项](design/UI_FINESSE_ROUND4_64_TASKS_2026-10-18.md)整体计划；本清单只用于挑选一个可执行子任务，`PROPOSED` 表示能力/依赖待审，不要求重复授权计划内任务。交付与退出条件见[协议](design/reviews/ui-finesse-round4-20261018/DELIVERY_AND_EXIT.md)。状态表见[第四轮账本](design/reviews/ui-finesse-round4-20261018/ROUND4_PROGRESS.md)。S00-01 已满足；S00-02 SDK8 Release build 通过，但完整 WPF 循环在 R05 系统鼠标位置读取权限处停止，安装/版本核验未到达。S01-01 至 S01-08、S02-01 至 S02-08 已完成受控验证；S02-04 至 S02-08 原始证据使用 SDK9，本轮以当前工作树 SDK8 Release 构建和定向测试复核通过，详细结果见 SDK8-S02 证据页；真实宿主仍待验。S02-08 通知体验受控测试当前 SDK8 子集 20/20、Worker 12/12；全量 WPF、Windows 系统高对比、真实读屏器和真实 Playnite未验。S03-01 来源关系模型 SDK9 Core `156/156` 已验收；S03-02 `fd59dcc1` 版本关系视图 SDK9 Release solution `0/0`、Playnite `12/12`、Worker `2/2`、RenderHarness `render-qa OK` 已受控验收；S03-03 冲突原因对照已完成 SDK8 受控验证；S03-04 恢复方案草稿已完成 SDK8 build、定向验证、隔离安装与双主题渲染，完整 WPF 循环和真实 Playnite仍待验；S03-05 方案差异比较已完成 SDK8 build、定向测试、双主题渲染和精确提交隔离 no-start 安装；S03-06 `7366d8cf` 方案失效规则已完成 SDK8 构建、14/14 定向测试、双主题渲染和精确提交隔离 no-start 安装。S03-04 至 S03-06 的完整 WPF 循环与真实 Playnite仍待验。S03-07 `eeedd003` 脱敏只读导出已完成 SDK8 构建、18/18 定向测试、双主题 Render QA 和精确提交隔离 no-start 安装。S03-04 至 S03-07 的完整 WPF 循环与真实 Playnite仍待验。S03-08 来源与方案验收已受控完成：SDK9 Release solution 0/0，Core 10/10、Playnite 15/15（含 R09 2/2）；SDK8.0.423 已安装并用于本轮其他 S02 测试，但 S03-08 未在本轮重跑。S04-01 至 S04-04 已受控验收，S04-03 复用 `f51e5c9e` 同步缩放/平移，S04-04 `9e0af12b` 加入擦除对照、棋盘透明底纹和键盘调整；S04-05 已核实依赖满足并准入。S07-01/02/04/05/06/07/08 已完成，S07-03 仍受 branch protection 读取权限阻塞。原有 Q/R 环境边界不变。

| ID/范围 | 状态 | 下一步与准入条件 |
| --- | --- | --- |
| S00-01 | SATISFIED | `dd38f8ca` 与实现/证据已在 origin/main；稳定 ID、双语别名、作用域、不可用原因复用真实命令。原 7/7 与当前 SDK8 行为类 2/2 均有记录；无需重复实现。 |
| S00-02 | BLOCKED_ENVIRONMENT | `29dc09b2`/`a58dd582` 实现 Shell 内搜索 overlay；21/21 定向测试与 SDK8 Release build 通过。全量 WPF 隔离在 R05 `GetCursorPos` Win32 error 5 处停止，安装/版本核验与真实宿主呈现未运行。 |
| S04-01 | IMPLEMENTED | `f911cba4` 比较对象集合最多保留两张有效截图，按稳定媒体 ID 存储；视频/缺失文件有原因，筛选变化不替换对象。SDK8 Release solution 0/0、行为测试 5/5、双主题 11 尺寸 RenderHarness `render-qa OK / 0 PROBLEM`。证据见 Round4 S04-01；下一项 S04-02 双图工作区。 |
| S04-02 | IMPLEMENTED | `825e58e9` 双图工作区展示两张同比例截图、原图像素尺寸、大小、来源与游戏 ID；窄宽度上下排列，短宿主默认保留媒体列表并可手动展开。SDK8 Release solution 0/0、WPF 7/7、双主题 11 尺寸 RenderHarness `render-qa OK / 0 PROBLEM`。证据见 Round4 S04-02；其后 S04-03 已由 `f51e5c9e` 实现并验证。 |
| S04-03 | SATISFIED | `f51e5c9e` 已实现同步/独立缩放、适应窗口、100% 查看、归一化中心坐标平移、12.5%–400% 缩放上限与 2048px/64MiB 缩略图预算。SDK8 一键全链内状态 `4/4`、生产 WPF 面板行为 `1/1`、缩略图预算 `3/3`；当前 RenderHarness 双主题 `render-qa OK / 0 PROBLEM`。证据见 Round4 S04-03；真实 Playnite 页面呈现未验。 |
| S04-04 | IMPLEMENTED | `9e0af12b` 在现有媒体比较视图加入擦除分隔线、A 左/B 右、方向键/Home/End 调整和透明棋盘底纹；复用现有异步解码图像源。SDK8.0.423 Release solution 0/0，生产 WPF 行为 2/2，截图及 TRX 见 Round4 S04-04 证据。仅作人工视觉比较，未做内容一致性校验或自动删除。 |
| S04-05 | IMPLEMENTED | `4d0bfd3c` 锁定左侧 A，在当前 `MediaView` 已加载的筛选结果里切换右侧 B；详情选择/焦点保持，坏图跳过并可继续双向浏览，复用缩略图代际保护。SDK8.0.423 Release solution 0/0，定向行为 `6/6`。证据见 Round4 S04-05；真实 Playnite、真实媒体、物理输入未验。 |
| S04-06 | IMPLEMENTED | `792b1a2f` 比较页意见标记仅在当前会话内按稳定媒体 ID 保存并可撤销；不改变正式收藏/备注、忽略状态或文件。SDK8 Release 与生产 WPF 意见/撤销行为 `11/11`，证据见 Round4 S04-06。 |
| S04-07 | IMPLEMENTED | `14925e5e` 将已标记对象按稳定 ID 转交现有归类/忽略预览，并由 Worker 复核当前对象；关闭比较面板不执行操作。Playnite `7/7`、Worker `1/1`，SDK8 Release solution `0/0`，证据见 Round4 S04-07。 |
| S04-08 | IMPLEMENTED | `c9da7c76` 复用现有 2048 px/64 MiB/3 路解码预算，补充 GIF、非支持格式、损坏图及卸载时迟到回调的行为验收；SDK8 Release solution `0/0`，缩略图/生产比较页 `19/19`。合成 WPF 测试不代表真实 Playnite、真实媒体或物理 DPI；证据见 Round4 S04-08。 |
| S05-01 | IMPLEMENTED | `990758b6` 为维护中心加入会话内候选保留规则/增长范围输入，字段错误和页面滚动行为使用生产 WPF 验收；SDK8.0.423 Release 一键隔离构建/安装 `0/0`、定向行为 `6/6`、双主题 11 尺寸 RenderHarness `0 PROBLEM`。真实 Playnite与物理 DPI 未验；证据见 Round4 S05-01。 |
| S05-02 | IMPLEMENTED | `5f1431f2` 审计确认旧全局预览只规划当前逐游戏策略；新增同一 SQLite 数据快照/UTC 截止时点的当前策略与统一候选策略对照，复用 `RetentionPlanner`，增长范围只作为明确标注的假设，不创建清理授权。SDK8 Release solution `0/0`、Worker 对账 `14/14`、生产 WPF Light/Dark `2/2`、精确提交隔离 no-start 安装通过、RenderHarness `0 PROBLEM`。真实 Playnite、完整 WPF 循环与物理 DPI 未验；证据见 Round4 S05-02。之后 S05-03、S05-04 已完成。 |
| S05-03 | IMPLEMENTED | `e6279b7d` 将历史逻辑增长显示为基于可用样本的观察范围/未知，分开用户假设、本地卷和远端容量边界，不输出确定耗尽日期。SDK8 Release solution `0/0`、Worker `5/5`、维护页 Light/Dark `2/2`；真实 Playnite/完整 WPF/物理 DPI 未验。证据见 Round4 S05-03。 |
| S05-04 | IMPLEMENTED | `cf50643c` 为本地存档、第二镜像、远端恢复暂存和媒体目录分别统计文件长度/文件数及可用逻辑索引量，不生成总计；空目录、未启用、路径不可用/读取不完整分别说明，并标记重叠路径。SDK8 Release solution `0/0`，Worker `8/8`、生产维护页 Light/Dark 与存储导航 `4/4`，source validation 通过。文件长度不等于卷簇实占；真实 Playnite/物理 DPI/完整 WPF 未验。证据见 Round4 S05-04。 |
| S05-05 | IMPLEMENTED | `2fd0aa04` 将双策略总量比较下钻到逐游戏计数、变化版本和保护原因；只读检查通过既有归档路径安全边界的候选，不创建清理预览句柄。SDK8 Release solution `0/0`、Worker `2/2`、生产维护页 Light/Dark 外层/内层滚动 `2/2`、RenderHarness `0 PROBLEM`、source validation 通过。缺失/不可访问/不安全路径不计作现存归档；真实 Playnite/物理 DPI/宿主滚动未验。证据见 Round4 S05-05；之后 S05-06 和 S05-07 已验收。 |
| S05-06 | IMPLEMENTED | `313fabc1` 为保留情景新增有界、版本化的标量设置持久化；维护页可选择并载入方案、保存/更新、复制、重命名和删除，快照时间随同一输入的成功比较更新。旧配置缺少字段时为空集合；不缓存比较结果、不改生效策略。SDK8 Release solution `0/0`、XAML `25/25`、定向测试 `13/13`、Light/Dark RenderHarness `0 PROBLEM`、source validation 通过。真实 Playnite/完整 WPF/物理 DPI 未验；证据见 Round4 S05-06。随后 S05-07 已验收。 |
| S05-06 | IMPLEMENTED | `313fabc1` 为保留情景新增有界、版本化的标量设置持久化；维护页可选择并载入方案、保存/更新、复制、重命名和删除，快照时间随同一输入的成功比较更新。旧配置缺少字段时为空集合；不缓存比较结果、不改生效策略。SDK8 Release solution `0/0`、XAML `25/25`、定向测试 `13/13`、Light/Dark RenderHarness `0 PROBLEM`、source validation 通过。真实 Playnite/完整 WPF/物理 DPI 未验；证据见 Round4 S05-06。 |
| S05-07 | IMPLEMENTED | `6807ae84` 新增维护页情景转交策略编辑入口：拒绝覆盖未保存情景/模板/游戏策略草稿，只映射四项保留期限到未保存模板副本，清空旧比较并跳至现有策略差异页；无自动保存、Apply 或清理。SDK9 Release solution `0/0`、生产维护页 Light/Dark 与策略映射 `4/4`、XAML `25/25`、source validation 通过。SDK8.0.423 未安装；真实 Playnite、完整 WPF/物理 DPI 未验。清理仍须当前 Worker PreviewId。证据见 Round4 S05-07。 |
| S05-08 | IMPLEMENTED | `dc696fb0` 为情景比较增加取消命令、10,000 个索引版本上限（查询限取 10,001）及结束前版本/有效策略重验；超限或过期均不展示旧明细、不清理。SDK9 Release solution `0 error`，Worker `29/29`（含 1,200 版本、零/极端增长、并发改变、上限和取消），维护页 Light/Dark `2/2`；SDK8/真实宿主/完整 WPF/物理 DPI 未验。证据见 Round4 S05-08。 |
| S06-01 | IMPLEMENTED | `40a82b15` 新增有界的清单/条目设置模型；按对象种类与稳定 ID 去重，克隆隔离，限制清单、条目、名称、ID 和备注大小，不存 DTO、路径或操作授权。SDK9.0.302 Playnite.Tests Release 0 warning/0 error，行为 4/4，源码护栏通过。SDK8.0.423 未安装；证据见 Round4 S06-01。 |
| S06-02 | IMPLEMENTED | `85400645` 为存档版本、媒体、后台任务、校验问题详情加入确认式待办入口；确认后只存稳定 ID，不执行源业务操作。SDK9 Release solution `0/0`、Playnite `8/8`、Worker `2/2`，source validation 通过。真实 Playnite 未验；证据见 Round4 S06-02。 |
| S06-03 | IMPLEMENTED | `0fc43773` 已交付清单浏览/搜索/分组、键盘重排、确认移除和按稳定 ID 解析引用状态。SDK9 Release solution 0/0、Worker 2/2、Playnite 8/8。真实 Playnite未验；RenderHarness 因 NETSDK1047 未运行。证据见 Round4 S06-03。 |
| S06-04 | IMPLEMENTED | `2af5841f` 已交付稳定 ID 续接存档版本、媒体、后台任务和校验问题；任务返回时恢复当前会话清单上下文。不猜名称/路径、不恢复对话框或执行命令；缺失时可移除。SDK9 Release solution 重编译 `0/0`、Worker `2/2`、Playnite `10/10`。SDK8.0.423 与真实 Playnite未验；证据见 Round4 S06-04。 |
| S06-05 | IMPLEMENTED | `0aa80547` 新增唯一选中条目的人工完成/撤销动作；状态不代表业务成功，续接及业务命令结果不自动切换。保存后按对象种类/稳定 ID 保留所有已有引用解析状态。SDK9 Release solution `0/0`、Playnite `17/17`、source validation、XAML `25/25`、Round4 台账 `5/5`。真实 Playnite与 SDK8 未验；审计和证据见 Round4 S06-05。 |
| S06-06 | IMPLEMENTED | `fb33cd11` 按稳定 ID 刷新并展示当前对象/父游戏摘要；摘要仅为临时 UI 投影，不写清单设置，不按名称匹配。Worker `3/3`；最终 SDK9 Release solution `0/0`、Playnite 页面/表格几何与入口 `23/23`。滚动视口边界修复为 `a1a86e41`。OS ACL/物理可读性未知。证据见 Round4 S06-06。 |
| S06-07 | IMPLEMENTED | `96a3eab3` 增加只读脱敏人工待办预览与显式复制；默认包含未手动完成项、失效/未检查/不可检查引用和 UTC 时间，不含稳定 ID、完整路径、内容、凭据或命令。SDK9 Release solution `0/0`、定向行为/UI/复制反馈 `13/13`、source/XAML/Round4 ledger 通过。真实 Playnite/系统剪贴板/物理 DPI 未验；证据见 Round4 S06-07。 |
| S06-08 | IMPLEMENTED | `09ec3c84` 增加原生设置 Worklists 跨会话 JSON 重载、旧字段默认和损坏清单恢复/原文件备份行为回归；无源对象操作。SDK9 Release Playnite.Tests Rebuild `0/0`、设置与清单回归 `27/27`、source validation 与 Round4 ledger 通过。真实 Playnite 启停和宿主磁盘原子性未验；证据见 Round4 S06-08。 |
| S01-01 | IMPLEMENTED | `25bbe005` 实现版本化工作视图配置并接入设置复制/重置；Json.NET 集合替换/归一化由 SDK8 测试复核，目标项目构建通过、ViewConfigurationDefinition 与 R09 合计 7/7。证据见 Round4 S01-01。 |
| S01-02 | IMPLEMENTED | `ad7bd563` 新增舒适/紧凑全局密度设置与共享令牌，四行下限按行/表头高度计算；SDK8 solution build 0/0，密度/设置资源14/14、R09 2/2、MediaInbox几何3/3、R23双主题资源9/9。真实 Playnite 验收待 UI gate；证据见 Round4 S01-02。 |
| S01-03 | IMPLEMENTED | `8cdf5253` 为存档历史、候选路径和任务表格加入可访问的列管理；SDK8.0.423 Release build 0/0，列显隐/列宽/视图配置/R09 共 17/17。真实 Playnite 设置保存与呈现未验；证据见 Round4 S01-03。 |
| S01-04 | IMPLEMENTED | `c6eb8d62` 为任务、存档历史和候选路径表格加入保存/复制/重命名/删除命名视图；SDK8.0.423 Release build 0/0，视图/列/R09/R10/Q14 隔离测试 29/29，Render QA OK。真实 Playnite 设置保存与呈现未验；证据见 Round4 S01-04。 |
| S01-05 | IMPLEMENTED | `dcfca235` 标记命名视图的临时筛选/列/排序/密度差异并支持恢复；SDK8.0.423 Release build 0/0，视图/列/R09/R10/Q14 隔离测试 29/29，Render QA OK。真实 Playnite 设置保存与呈现未验；证据见 Round4 S01-05。 |
| S01-06 | IMPLEMENTED | `9ec0096b` 增加表格/工作视图重置影响预览；SDK8.0.423 Release build 0/0，定向测试 39 passed / 0 failed / 11 skipped，Render QA OK。完整 WPF/安装门禁及真实 Playnite 未验；用户外机 R09 身份材料缺失，不能追溯。证据见 Round4 S01-06。 |
| S01-07 | IMPLEMENTED | `87520a92` 为共享表格列宽加入窗口窄时临时压缩、放宽后恢复；偏好宽度保持用于设置保存。SDK8.0.423 Release build 0/0，六个隔离测试类 20/20，Render QA OK。完整 WPF/安装门禁及真实 Playnite 未验；外机 R09 旧断言与 `f13b9e05` 前源码吻合，但同次程序集身份缺失。证据见 Round4 S01-07。 |
| S01-08 | IMPLEMENTED | `d1005e33` 完成视图配置迁移回退；SDK8.0.423 Release solution build 0/0，五个隔离测试类 21/21（含 R09 2/2），source validation 与 diff check 通过。真实 Playnite 设置保存/加载、双主题宿主呈现及安装门禁未验；用户旧 R09 身份材料缺失。证据见 Round4 S01-08。 |
| S02-01 | IMPLEMENTED | `83194c50` 建立本地通知记录 DTO 与 Worker SQLite 去重/保留接口；SDK8 解决方案构建 0/0，新增通知持久化测试 3/3，Worker 364 passed/1 skipped。通知 producer 留给 S02-02。证据见 Round4 S02-01。 |
| S02-02 | IMPLEMENTED | `8810d9e6` 接入通知记录生产者和收件箱；SDK8.0.423 Release solution build 0/0，Playnite 定向 15/15（R09 2/2）、Worker persistence 3/3、双主题 Render QA 22/22。真实 Playnite/安装门禁未运行。证据见 Round4 S02-02。 |
| S02-03 | IMPLEMENTED | 6d3a6f43 加入单条/当前筛选结果已读与确认清除已读记录；SDK8.0.423 Release build 0/0，Playnite 定向 18/18（R09 2/2）、Worker SQLite 7/7、双主题 11 尺寸 Render QA 22/22。真实 Playnite/安装门禁未运行；外机旧 R09 缺同次身份材料。证据见 Round4 S02-03。 |
| S02-04 | CODE_COMPLETE_PENDING | `1b17e4a0` 增加通知到游戏/任务的稳定 ID 上下文跳转；SDK9 Release solution build 0/0，通知/R10 + Worker 查询 + R09 `20/20`，双主题 11 尺寸 Render QA OK。SDK8 正式基线、完整 WPF 隔离、Playnite 安装/宿主未验；保存的一键日志停于 R05 指针读取。证据见 Round4 S02-04。 |
| S02-05 | IMPLEMENTED | `2aa3fe3c` 增加 UTC 临时静音、跨午夜本地安静时段、可发现状态和解除入口；SDK9 Release build 0/0，策略 20/20、设置交互/持久化 15/15、独立 R09 2/2，浅/深主题 × 两尺寸探针 4/4。SDK8、完整 WPF 循环、安装和真实宿主待验；旧 R09 输入身份未知。证据见 Round4 S02-05。 |
| S02-06 | IMPLEMENTED | `a6153219` 明确已知警告/错误级别突破静音；SDK9 策略 22/22、持久化 7/7、R09 2/2。SDK8、完整 WPF 循环和真实宿主未验。证据见 Round4 S02-06。 |
| S02-07 | IMPLEMENTED | `9ed6b4e5`/`c4fb56a0` 归并明确重试链并保留首末时间、次数和最后错误；SDK9 通知/重试/R09 定向测试 19/19。SDK8、完整 WPF 循环和真实宿主未验。证据见 Round4 S02-07。 |
| S02-08 | IMPLEMENTED | 复用已有通知体验行为；SDK9 Playnite 定向测试 20/20、Worker 持久化 12/12。完整 WPF 循环、SDK8、系统高对比、读屏器与真实 Playnite 未验。证据见 Round4 S02-08。 |
| S03-01 | IMPLEMENTED | `5e05aa73` 新增 Core 来源关系模型；SDK9 Release 构建成功，Core 全套 `156/156`。可报告缺父、重复/歧义 ID、环、缺失 ID 和未知关系，最多读取 500 条并做一次截断探测；不按时间戳推断线性历史。证据见 Round4 S03-01。 |
| S03-02 | IMPLEMENTED | `fd59dcc1` 为历史接入 Worker 时间键+备份 ID 稳定分页与 Playnite 来源关系文字；SDK9 Release solution `0/0`、Playnite `12/12`（R09 `2/2`）、Worker `2/2`、RenderHarness `render-qa OK / 0 PROBLEM`。证据见 Round4 S03-02；SDK8 与真实 Playnite待验。 |
| S07-01 | IMPLEMENTED | `4ea5e694` 将 `CURRENT_STATE.md` 从 254 行压缩为 32 行并保留完整历史快照；source validation、diff check 与归档一致性检查通过。证据见 Round4 S07-01。 |
| S07-02 | IMPLEMENTED | 新增 `ROUND4_STATUS.json` 与渲染/校验脚本；账本生成块接入 `validate-source.py`，保留外部 Q/R 状态。证据见 Round4 S07-02。 |
| S07-04 | IMPLEMENTED | 已在仓库 `AGENTS.md` 明确 CI 结果记录条件与禁止纯文档通过结果自循环的规则。保留实际 run/job/artifact 身份，不更改 workflow 或 required checks。证据见 Round4 S07-04。 |
| S07-05 | IMPLEMENTED | 复用现有 diagnostics collector 补充环境身份，新增本机只读 summary/TRX 预览；旧 TRX 可在本机复算。证据见 Round4 S07-05。 |
| S07-06 | IMPLEMENTED | 诊断 `summary.json` 加入原始 TRX 的测试用例通过/失败/跳过汇总、每文件原计数对照、套件类别和 WPF 隔离类运行数；保留逐文件路径便于复算。证据见 Round4 S07-06。 |
| S07-07 | IMPLEMENTED | `ENVIRONMENT_CAPABILITY_CACHE.json` 记录 SDK/Playnite/CEF/ETW 当前能力状态、已知检查与重试条件；validator 纳入 source validation。证据见 Round4 S07-07。 |
| S07-03 | BLOCKED_ENVIRONMENT | workflow 当前所有路径走完整双 SDK 流程；branch protection API 返回 403，required check 名称不可见。未改 workflow；取得配置读取权限或 required-check 清单后继续。证据见 Round4 S07-03。 |
| S00-03 | IMPLEMENTED | `0968309c` 补方向键、Enter、IME composition 防护和插件级 `Ctrl+Shift+P`；SDK8 Release 定向回归 `12/12`。`22cec597` 标准一键链退出 `0`，1896 项 0 失败、140 WPF 类通过，安装并启动 Playnite。物理键盘/IME 候选/最终交互画面仍 pending。详见 S00-03 标准验收记录。 |
| S07-08 | IMPLEMENTED | 在[交付协议](design/reviews/ui-finesse-round4-20261018/DELIVERY_AND_EXIT.md)明确单次批次快照、64 项总体验收条件及无可执行项的报告方式；总目标仍未完成。 |
| S00-04 | IMPLEMENTED | `f8b3144e`：独立稳定 ID 固定/最近命令；SDK8 完整标准链 1,904 项、0 fail，打包安装和 Playnite 启动通过。真实页面交互/视觉与输入仍待验，见 [S00-04 证据](design/reviews/ui-finesse-round4-20261018/evidence/S00-04/README.md)。 |
| S00-06 | IMPLEMENTED | 30e32794仅转交恢复/保留审阅页；SDK8编译0/0，7类36/36。真实宿主与S00-06～08最终默认链待验。 |
| S00-07 | IMPLEMENTED | S00-06实现/受控依赖已具备；细化名称、类别、作用对象、真实快捷键、不可用原因及长文本/双主题/窄窗检查。 |
| S00-08 | READY | S00-07后处理真实登记命令的行为验收，保留宿主边界。 |
| S03-03 | IMPLEMENTED | `3268565c` 冲突详情并排显示双方版本/设备证据、共同父版本、指纹关系与 readiness 校验状态/时间；SDK8 Release solution `0/0`，Playnite `4/4`、Worker `2/2`、Core `7/7`、R09 `2/2`、Render QA OK。无时间戳覆盖建议；完整 WPF 隔离、安装与真实 Playnite未验。证据见 Round4 S03-03。 |
| S03-04 | IMPLEMENTED | `1c7083e2` 实现只读恢复方案草稿；SDK8 Release solution `0/0`，Core `156/156`、Worker `380/1 skipped`、Playnite 非 WPF `475/27 skipped`、定向 `31/31`，隔离安装与 RenderHarness 双主题复跑通过。当前 main R09 `2/2`；完整 WPF 一键复跑至 `48/133` 时因 R05 系统指针读取失败中断（178 通过/1 失败，余 85 类未跑）；真实 Playnite未启动。用户旧 R09 输入身份缺失。证据见 Round4 S03-04。 |
| S03-05 | IMPLEMENTED | `b2c1e3de` 增加 A → B 恢复草稿差异比较，以 DPAPI 保护的 HMAC 映射指纹区分同备份版本的映射变化；未知指纹/大小显式保留未知。SDK8 Release build 0/0，Playnite 7/7、Worker 4/4，双主题 Render QA 与精确提交隔离 no-start 安装通过。完整 WPF 循环和真实 Playnite未验；证据见 Round4 S03-05。 |
| S03-06 | IMPLEMENTED | `7366d8cf` 为恢复草稿记录对象/内容/映射/readiness 变化导致的需重验状态；保留现有 RestoreConfirmationGuard 与最新安全确认。SDK8 Release solution 成功（还原的漏洞数据源产生 17 条 NU1900 警告），定向测试 14/14、双主题 Render QA、XAML 检查和精确提交隔离 no-start 安装通过。完整 WPF 循环和真实 Playnite未验；旧 R09 输入身份未知。证据见 Round4 S03-06。 |
| S03-07 | IMPLEMENTED | `eeedd003` 新增草稿只读 TXT 预览/导出，按字段白名单生成摘要，隐藏本地路径、认证参数与内部指纹；草稿变化时刷新预览并要求再次确认。SDK8 Release build 成功（17 条离线 NuGet audit `NU1900` 警告），定向测试 18/18、Render QA 双主题、XAML 检查和精确提交隔离 no-start 安装通过。全量 WPF/真实 Playnite及文件对话框交互未验；Demo 目录不在 checkout。证据见 Round4 S03-07。 |
| S03-08 | IMPLEMENTED | `37f42cc2` 用两设备合成分叉、缺父、同指纹、时钟偏差、41 天 readiness 和只读文件夹具验收来源/恢复方案；SDK9 Release solution `0/0`，Core `10/10`、Playnite `15/15`（含 R09 `2/2`）。SDK8.0.423 未安装，完整 WPF 与真实 Playnite未验；证据见 Round4 S03-08。 |

不自动启动新线程；不重跑无变化的外部权限探测；不把真实存档、媒体或云端写入当规划验证。


本清单是无人值守开发代理选择单一工作项的唯一来源。任务状态只能使用：`PROPOSED`、`READY`、`IN_PROGRESS`、`IMPLEMENTED`、`BLOCKED_ENVIRONMENT`、`BLOCKED_USER_DECISION`。

> 当前 UI 方向声明（2026-08-17）：用户已授权整页 UI 重构。下方 UI 条目中的“保留现有页面/控件实现”“避免改变信息架构”等，是对应历史阶段的范围或验收记录，不再作为当前页面冻结禁令；新的 UI 任务可以重做布局、导航、控件和共享模板，同时必须通过本文件及 UI 门禁要求的功能、性能、可访问性和 Playnite 验证。

## 2026-10-18 最新检查点：#1086 双 SDK CI 已终态

- Actions [#1086/run `39077533150`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39077533150) 对文档提交 `5eb0d1fd9a06acedaa2bbdb52e3b810a37eb9341` 已 success。SDK8.0.423、SDK10.0.401 两 lane Release build/test 均通过，Core `125/125`、Worker `362/362`、WPF `125/125`；SDK8 Render QA、静态审查和扩展包成功。Artifacts ID/SHA 记于 CURRENT_STATE / WORKLOG。
- 当前 main 产品基线 `be314f59` 与 R00/R01 freshness `14 fresh / 0 stale / 0 matched source paths` 未变。复扫 Round2/3、质量审查与 R23-08 未找到新增的、前置已满足且有明确代码缺口的 Q/R 项；不添加空转功能。外机 R09 仍是用户提供的独立真实失败，已有文本未附该次身份包，根因未定位；按用户要求不等待，继续主线。

## 2026-10-17 当前阶段：Q11-07 双主题往返证据与双 SDK CI 已收口

- 状态：`IN_PROGRESS`（受控行为/视觉与双 SDK CI 已通过；真实宿主/Final 尚待验，故任务最终未完成）。以 `6a909cb329d88af3da83a78446050a83d24ef246` 为当前证据提交；仅在 `R08PageSwitchBehaviorTests` 增加 opt-in Light/Dark 离屏截图/几何报告，无生产 UI/XAML、服务、DTO、命令或导航改动。四个缓存生产页往返行为类 `4/4`。
- 本机 SDK `9.0.302`/VSTest `17.14.1` 定向 Release `4/4`。精确提交 Actions #1062/run `38988908489` 的 SDK8.0.423 job `117266649011` 与 SDK10.0.401 job `117266648758` 终态成功：Release `0/0`、Core `125/125`、Worker `362/362`、WPF 隔离 `125` 类通过；SDK8 还完成 Render QA、静态审查、包生成与上传。新代码 CI 结果已记录到 Q11-07 证据包；本机和 runner 各自证明自己的输入。
- 8 张图是 96-DPI `RenderTargetBitmap`。任务选中 ID 由行为断言检查，但不在18 DIP的截图视口；图像不代表 Playnite、物理 DPI 或屏幕呈现。宿主/Final仍待验。Round2/3 与 R23-08 复扫未找到可独立实施的现存代码缺口；下一准入点是用户外机 R09 同次身份 artifact、新可复现问题或环境前置变化。外机 R09 `1/2` 仍是真实失败，不由本机或 CI 通过覆盖。

## 2026-10-05 收口队列（当前执行入口）

本节优先于下方历史候选及旧 Q/R 文档的“下一项”。用户本轮授权审阅和调整任务；这里只把明确可执行的工程缺口标 READY，不据此启动无限开发。实现证据、宿主验收和用户故障分别记录；`IMPLEMENTED` 不等于发布通过。

### 2026-10-17 R00-03 历史 SDK10 失败复核与外机输入校正

GitHub 连接现可读取 #919/run `36675309812` job `109758888214` 与 #920/run `36677346317` job `109765067482` 完整日志。两份日志均使用 SDK `10.0.401`，在 `ProductionShellChromeSourceTests` 的活动侧栏过渡前提处各失败 2/13；系统偏好可合法关闭动效，而测试只固定了应用开关。`48b78b3b` 的实例级系统动效测试 seam、策略矩阵与默认系统偏好测试校正了输入；修正后 #1056 SDK8/SDK10 全量 WPF 均通过，当前 main #1084/run `39070369704` 和 #1085/run `39074063525` 的 SDK8.0.423 与 SDK10.0.401 两 lane 也均完成 125/125 WPF 类（目标类是第17类）。历史失败仍保留，不作为当前 main 失败。详情见 [R00-03 失败复核](design/reviews/ui-finesse-round3-20260915/evidence/R00-03-SIDEBAR-MOTION-CI-FAILURE-RECHECK-20261017/README.md)。

外机 R09 附件 SHA `F3DDDC...4514` 与已记录文本相同，诊断 ID 未变；`Thickness(2/1)` 与第151行是旧测试形态，不能当成当前 main 新复跑。若新失败 artifact 到手，再按其 summary/TRX/DLL/PDB 身份定位，不用本机或 GitHub runner 的通过覆盖外机失败。

R02-06、R05-03、R23-05 当前仍分别依赖真实 Playnite 菜单、Popup 实际呈现帧和获准系统跟踪环境；不绕过权限。R23-08 无新的依赖已满足产品代码项。本轮只校正证据与表格重复项，不新增产品功能；下一次在新缺陷证据或环境前置变化时重新准入。

### 2026-10-17 Worker 取消回调 CI 与 R12-04 复核（阶段已收口）

取消回调测试夹具修正已在提交 `4a98148caf2b0909998824acc73119272fc0cb37` 完成。Actions #1056/run `38962119739` 两 lane 均终态成功：SDK8.0.423 job `117179503173`、SDK10.0.401 job `117179503250`，各自 Release build `0 warning / 0 error`、Core `125/125`、Worker `362/362`、Playnite source `107` 类和 WPF 隔离 `125` 类全通过；SDK8 另 Render QA、WPF 静态审查和扩展包上传成功。#1055 的 SDK8 package 重跑失败是已修正测试夹具的历史真实失败。本机完整 WPF 门禁仍在 R05 Tooltip `GetCursorPos` 前置阶段停止，R09 定向 `2/2` 不代表本机全套通过。详见[Worker 取消回调与双 SDK 证据](design/reviews/ui-finesse-round3-20260915/evidence/CI-WORKER-CANCEL-CALLBACK-RECHECK-20261017/README.md)。

R12-04 的四个生产/测试文件 SHA 与当前 main 既有复核证据一致；完整 Worker/Playnite CI groups 通过，无需重建服务或重复局部测试，状态维持“已满足（当前 main 复核）”。本轮按 R23-08 准入记录复扫后没有发现新的可独立实施项；后续等外部诊断身份 artifact、环境前置变化或任务表出现具体缺口时，再按小批继续。

外机 R09 日志真实 exit `1` 保留。第151行 Thickness 断言不匹配当前源码同一行的颜色断言，具体 checkout/DLL/PDB/脚本输入未知。本机与外机发现116个 WPF 类，但 #1055 同提交 SDK8 CI 发现125类；类数不能单独作为根因证据。重复附件与既有日志 SHA 相同，不阻塞其它工作。

### 2026-10-17 Q24-04 / R00-08 当前续跑点

**本机代码与行为：** 已提交推送 `5c81a07056d64482767d546efe043012abc1e6fc`；只为生产 `TaskCenterView` 现有“加载更多”状态/命令增加实际 STA 键盘正负轨迹，没有改生产 UI、服务、DTO 或命令。有历史时按钮显示且双向可达；耗尽时折叠且双向不可达。本机 SDK `9.0.302`/VSTest `17.14.1` 定向 Release build，四类键盘测试 `21/21`；非完整 solution build。`R00-08` evidence sourceCommit 更新后，R00/R01 freshness `14/14 fresh / 0 stale / 0 matched source paths`、2,918 路径、package identity `not-provided`。证据：[Q24-04 加载更多键盘与 freshness](design/reviews/ui-finesse-round3-20260915/evidence/Q24-04-LOAD-MORE-KEYBOARD-RECHECK-20261017/README.md)。Round2 Q24-04 仍未完成：真实 Playnite/OS 键盘、完整阅读顺序、UIA/Narrator 与视觉待验。

**外机 R09 单独保留为真实失败：** 用户日志 Core `125/125`、Worker `357/357`、R09 `1/2`、整体 exit `1`。`line 151` 的 `Thickness(2/1)` 对不上当前源码 line151 的 Brush 颜色断言，旧 Chrome 2 DIP 断言在 `3f7d0b30`；目前只能确认测试源码或 PDB 映射与当前源不一致，具体旧 checkout/DLL/PDB/脚本输入仍未确认。本机 R09 `2/2` 和历史 #1047 双 SDK通过都不能覆盖外机失败。失败同次 `summary.json`、TRX 与程序集身份是下一步定位材料，不阻塞独立任务。

**Actions 门禁已收口：** #1051/run `38927198349` 的 SDK10.0.401 job `117061655415` 曾终态失败于 Worker 取消回调测试，361/362，line128 expected1/actual0；完整 log 已读，SDK8 job `117061656455` 成功。#1052/run `38932328340` 与 #1053/run `38937972835` 已双 SDK整体成功。最新 #1054/run `38944728502` 对文档 HEAD `79748216` 也双 SDK整体终态成功：[Actions](https://github.com/Nikilua/GameSaveCenter/actions/runs/38944728502)。SDK8.0.423 job `117120854146` 与 SDK10.0.401 job `117120854512` 均 Release `0 warning / 0 error`、Core `125/125`、Worker `362/362`、125个WPF隔离类通过（含R09）；SDK8 另 `render-qa OK`、静态审查 `30 XAML / 0 errors / 27 warnings / 177 info`、扩展包与上传成功，诊断 artifact 分别为 `12068781572` / `12066891114`。这些结果只证明对应 GitHub runner 的构建输入通过，不证明外机 R09 失败已解决；#1051历史失败仍保留，不称已修复。

本次重新扫描 Round2 的 208 项与 Round3 的 192 项后，没有发现新的可独立实施 Q/R 代码缺口；Worker 取消测试与双 SDK Actions 已收口，R12-04 当前 main 身份复核完成。只读桌面快照没有可见 Playnite/Worker 进程且仅有一块活动屏，Q24-03 仍无法做真跨屏；R05-03 的 WMI、R23-04 的 CEF/隔离宿主、R23-05 的系统跟踪边界不绕过。下一步在外机 R09 同次诊断 artifact 可用、环境前置改变或任务表出现具体证据缺口时，再领取对应小批；目前用户粘贴文本仍是已有 SHA，缺 test DLL/PDB/plugin DLL 身份，不阻塞独立任务，也不将本机/GitHub runner 通过解释成外机成功。

### 2026-10-17 R00/R01 当前 main Render QA 校正

以当前代码基线 `5c81a070` 的后继文档提交 `01efb189` 运行仓库隔离 SDK `8.0.423` 的 Release RenderHarness build 与完整 `scripts/render-qa.ps1`，报告 `render-qa OK`、`0 PROBLEM`。旧准入表记录的 Task/Settings 13 个布局问题未在当前完整矩阵复现：窄 Settings 的最小主体 viewport `181.33 DIP`，Task 1040×700 为 `4/4` 可读行及可见 Inspector。R00/R01 freshness `14 fresh / 0 stale / 0 source-path matches`、2,919 paths；package identity `not-provided`。详细范围见[R00/R01 当前 Render QA 复核](design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-CURRENT-MAIN-RENDERQA-RECHECK-20261017/README.md)。

2026-10-15 静态 audit 的 13 MEDIUM `TOOLBAR_VERTICAL_EXPANSION` warnings 仍单独保留为布局风险；#1053 与 #1054 的 WPF 静态审查通过（30 XAML files，0 errors / 27 warnings / 177 info），但不清除此独立 audit 风险。未发现可据当前 Render QA 新增生产代码改动。#1053/run `38937972835` 与 #1054/run `38944728502` 均双 lane 终态成功；#1054 SDK8.0.423 job `117120854146`、SDK10.0.401 job `117120854512` 均 Release `0/0`、Core125、Worker362、WPF隔离125类通过（含 R09）；SDK8 另 Render QA `0 PROBLEM`、生成 `0.6.73` 扩展包并上传 package/diagnostics artifact。外机 R09 `1/2` 仍是真实独立失败，不由本机或 CI 通过覆盖；外机诊断目录 `D:\workplace\Github\GameSaveCenter\artifacts\build-diagnostics\20261016T052146719Z-d9d99ac4` 不在当前工作区，待取得该次 `summary.json`/TRX 做身份比对，不要求重跑。下一步重扫 R23-08 准入表，不发明功能。

### 2026-10-16 R13-08 失败帮助 CI 收口续跑点

**本阶段已收口：** `R13-08` 复用既有服务、DTO、映射和生产 `MaintenanceView`；只扩展测试。在现有 STA 真实生产页验证已识别认证失败与未知错误码正负行为：帮助文案/可见性、`Expander` 的实际 ExpandCollapse 状态、原始诊断展开与收起。本机 SDK `9.0.302` / VSTest `17.14.1` 定向 Release build 后 `16/16` 通过，source validation 与 diff check 通过。证据和源码/test DLL/PDB/plugin DLL 身份见[R13-08 证据包](design/reviews/ui-finesse-round3-20260915/evidence/R13-08-CURRENT-MAIN-FAILURE-HELP-RECHECK-20261016/README.md)。Actions #1047/run `38916251153` 对提交 `e5b27cde` 终态成功：SDK8.0.423 job `117024368753` 与 SDK10.0.401 job `117024368994` 均通过；两边 Core `125/125`、Worker `362/362`、Playnite source `107` 类、WPF 隔离 `125/125` 类通过。SDK8 另完成 Release build、`render-qa OK`、WPF 静态审查、扩展包生成和产物上传。SDK9 定向项目构建与两条 CI SDK 分开记录，不混为一项结果。

R00/R01 对本次唯一测试文件的显式 freshness 复核为 `14/14 fresh / 0 stale / 0 source-path matches`；当前 HEAD `d4559f3b` 的全基线 freshness 复核为 `14/14 fresh / 0 stale / 0 matched paths`（2,911 changed paths），package identity 未提供。R00-07 最近 audit 仍 `168 snapshots / 116 warnings / 0 HIGH / 13 MEDIUM`，13 条 toolbar wrap 风险保持打开，本轮未重跑 audit。外机 R09 一键流程确实 exit `1`：Core `125/125`、Worker `357/357`，R09 `1/2`。其 line151 映射到 `Thickness(2)`，但当前源 line151 是 Brush 颜色比较，历史 `3f7d0b30` line151 是旧 Chrome `2 DIP` 断言；由此确认测试源或符号行映射不与当前源匹配，尚不能区分旧 DLL/PDB/checkout/脚本输入。#1047 当前 main 双 SDK R09 通过不覆盖外机失败；外机同次 diagnostics `summary.json` / TRX / DLL-PDB identity 仍待补，不阻塞主线。R05-03 fail-closed 预检已尝试，读取 `Win32_Process.CommandLine` 返回“拒绝访问”；未启动 Playnite、安装扩展或创建 profile/output，不绕过 ENV-001。Q24-02 旧 `8/16` 是 `74815b2b` 历史结果；当前 main 重建后隔离类为 `15/16`，唯一失败是 `GetCursorPos` 前置条件，组合进程诊断 `11/16` 不作门禁。Actions #1047 SDK8/SDK10 两 lane 的 `125` 个 WPF 类全量测试成功。Q24-02 仍缺真实 Playnite/presented-frame。下一可执行项：Q24-04，先查现有焦点遍历与 fake 数据夹具，再补可见导航/筛选/表格/详情/页尾控件正反向完整轨迹。Demo 原目录不可用时继续用恢复生产基线；真实 Playnite/package-host、UIA/IME、物理 DPI/跨屏、presented frame、ETW/宿主性能仍未验。

### 2026-10-16 R13-04 当前续跑点（历史快照）

**最新续跑事实（`main` 当前候选 `a81e7abd219e136cf784359ac48174e154f157be`，与 `origin/main` 一致；代码变更在父提交 `ccdbacb683afe6b0a745640d545cc9876332391c`）：** R13-04 只增加隔离目录的 WorkerOptions 持久化/重载行为测试，没有改生产代码/XAML。本机锁定 SDK `8.0.423` / MSBuild `17.11.48` / VSTest `17.11.1` 定向结果为 Worker `4/4`、Core `1/1`、Playnite R13/设置迁移 `24/24`，同一隔离 Playnite 输出 R09 `2/2`；这不是本机完整 solution 或完整 Playnite 全类通过。GitHub Actions #1039/run `38872163664` 对当前候选终态成功：SDK8 job `116878566799` 与 SDK `10.0.401` job `116878566530` 均通过；完整 WPF 隔离入口 `124/124` 类通过，Core `125`、Worker `362`，SDK8 另完成 Render QA、静态审查和扩展包生成。故 CLOSE-WRAP-01、CLOSE-OPTICAL-01 与 CLOSE-FOCUS-01 的当前自动化门禁已收口；真实 Playnite/物理呈现仍是环境边界。本机全类扫描仍因 `GetCursorPos` Win32 error 5 中止，不绕过权限。R00/R01 freshness 当前文档 HEAD 为 `14/14` 无需重跑、0 登记源码路径命中，package identity `not-provided`。

外机本次 R09 附件显示 source classes `111` / WPF classes `116`，line 151 仍断言 `Chrome.BorderThickness == 2 DIP`，与当前 main line 151 错误色、line 152 Chrome `1 DIP`、line 154 `ValidationStroke 2 DIP` 不同；该旧断言形态见历史 `3f7d0b30`，`f13b9e05` 已拆分。附件中的 `artifacts/gsc-b/fd7672ef` 是随机构建目录，不是 commit；粘贴文本没有 sourceCommit/dirty paths/DLL/PDB SHA/插件 DLL 身份，因此只可确认测试输入版本不同，不能归因具体是旧 checkout、旧程序集还是安装包身份。#1039 当前候选通过不覆盖该失败。详细差异已记入 R13-04 证据包；旧 run #1038 只对应 `e6b77ade`。

### 2026-10-15 当前续跑点（覆盖下方历史下一项）

**最新续跑事实（从已推送 `676be4f3` 继续）：** R08 夹具第二次修正已完成本机 SDK8.0.423/VSTest17.11.1 Release test project build `0/0`，应用设置、系统偏好行为各 `1/1`；R09 高对比类本机复测 `3/3`，source validation/diff check 通过。本次修正先 drain Loaded/Dispatcher 并完成布局，再覆盖测试时长和触发入场动画，避免 Hosted runner 的 Loaded 回调与断言竞态；不改生产 UI。#1025/run `38687483121` 已终态 Failure：SDK10 job `116265550450` 在 R08SystemMotion 入场 opacity 动画断言失败；SDK8 job `116265550259` 在 R09HighContrast 行为矩阵第195行 Assert.True 失败，本机同类 `3/3` 通过，根因未证实。完整日志已由 GitHub 插件读取；对应诊断 artifact `11951470215` SHA-256 `7c328cf7b16c0d641dc0bdc61e6095d24e33b48ae69d0c9b4f1f5ddb812e4893` 与 `11951095969` SHA-256 `8d928e1b58657bb058b243aeb96de0cf8e5906b5bed46a9ffc90606181dd34cd`。修复补丁待提交推送；新 Actions 必须跟到终态。CI 收口后按用户顺序在新 source HEAD 重核 R00/R01 freshness，再推进 CLOSE-OPTICAL-01；真实 Playnite、物理 DPI/跨屏、呈现帧、UIA/读屏和 ETW/宿主性能仍未验。

- R00/R01 freshness 在 source HEAD `4eb778e9` 为 14/14 fresh / 0 stale；R00-01..07、R01-01..08 维持已满足，R00-08 受控行为满足、真实 OS/宿主输入待验。R07-04 当前 main `8/8`；R07-05 `18/18`；R07-06 `18/18`、XAML `24/24`、引用 Release build `0/0`。R07-07 在源码 `2b423737` 上 SDK8.0.423/VSTest17.11.1 `21/21`；R07-08 当前 main `d190f7ea` 同 SDK定向复测 `9/9`、项目引用 build `0/0`、XAML `24/24`。证据：[R07-07](design/reviews/ui-finesse-round3-20260915/evidence/R07-07-CURRENT-MAIN-RECHECK-20261015/README.md)、[R07-08](design/reviews/ui-finesse-round3-20260915/evidence/R07-08-CURRENT-MAIN-RECHECK-20261015/README.md)。这些不是完整 solution build或SDK10 CI。受控视觉/行为通过，真实触控板/拖窗/Playnite host仍待验；R10-02 当前 main 复核为 SDK8 `32/32`，见 [R10-02](design/reviews/ui-finesse-round3-20260915/evidence/R10-02-CURRENT-MAIN-RECHECK-20261015/README.md)；R10-03 至 R10-05 已有受控复核证据；下一步先在当前 main b04660e0 重查 R00/R01 freshness，再推进 CLOSE-OPTICAL-01。
- CI：#1006–#1009 已失败且完整日志受限；#1010/run `38636778462` 与 #1011/run `38638331563` 均 Failure。#1010 jobs `116101841201/116101841930`，artifact IDs `11929597360/11928647581`，SHA-256 `3c2bd19e16b0cef0dc54ba1c57e19b71a8be664e84dfa222e0d496e11a81660b` / `a9fb5a96607f5625c1699bf29a1447eb7616f1e424ac4dcb8bf8e4e0254fe746`；#1011 jobs `116107139673/116107139925`，artifact IDs `11928784541/11928968878`，SHA-256 `88170a28f0a19480eff989216674cbc0e3637d2bb80e35dfdbe2ea3e6bb352ac` / `f7abb468be15448673b2ba31f38ae3e48f69af2cfc6114fec4a9e9737cb4f1c5`。公开 job 只显示 exit 1，匿名日志 API 403；#1012/run `38640644812`（`2b423737`）上次看到 In progress，当前状态访问失败。`d190f7ea` 已推送，但其 Actions run ID/终态尚未从本机通道读到。CI SDK8.0.423/SDK10.0.401 分开记录；根因未知前不盲改。
- 外机 R09 用户称从 Git 拉取最新，仍报 `Expected 2 / Actual 1`、stack line151；片段不含外机 HEAD/工作树/source/DLL/PDB identity。当前 main line151 是错误色断言，line152 是 Chrome `1 DIP`，line154 是独立 `ValidationStroke 2 DIP`；锁定 SDK8 当前 main 重建同类 `2/2`，程序集哈希见[R09复核包](design/reviews/ui-finesse-round3-20260915/evidence/CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md)。外机根因仍未确认；libpng warning不在断言栈，COM异常是失败后的 WPF 收尾。未改生产模板或降低断言。
- 2026-10-15 CLOSE-WRAP 当前 main 复核：过期 `WorkspaceTreeGovernanceTests` getter 断言已改为当前通用页面注册路由，定向行为 `8/8`；Q14 当前 `1/1`，覆盖 654 DIP 单行无额外行距。SDK8.0.423 Release solution build `0/0`、XAML `24/24`；Core `125/125`、Worker `356/357`（1 skipped）、Playnite source `455 passed/27 skipped`、WPF `648 passed/39 skipped/1 failed`。唯一失败 `R05TooltipTimingBehaviorTests.QuickMousePassesCancelTooltipAndAStationaryDwellOpensIt` 在窗口显示前 `GetCursorPos` 返回 false；未改生产 UI或把失败记作通过。证据见[CLOSE-WRAP复核包](design/reviews/ui-finesse-round3-20260915/evidence/CLOSE-WRAP-CURRENT-MAIN-RECHECK-20261015/README.md)。本机测试门禁未全绿；当前提交 Actions 尚待 push 后查。
- 2026-10-15 在当前 main 源码检查点 `91bc88ad` 用本机 SDK9.0.302/VSTest17.14.1 重跑 R09 `2/2`、R05 Popup 边界与虚拟化 `7/7`；仓库固定 SDK8.0.423 未安装，本机结果不替代 SDK8/SDK10 CI。R05-03 无 presented-frame 证据，仍部分满足。详情见[R09 当前复核](design/reviews/ui-finesse-round3-20260915/evidence/CI-R09-CURRENT-MAIN-RECHECK-20261015/README.md)与[R05-03 复核](design/reviews/ui-finesse-round3-20260915/evidence/R05-03-CURRENT-MAIN-RECHECK-20261015/README.md)。
- R11-06 代码 `40af39c8` 与证据文档 `55ed3a17` 已推送：SDK8.0.423 / VSTest17.11.1，Playnite `4/4`、Worker `2/2`、XAML `24/24`、source validation 通过；只改测试，未完整构建 solution。证据：[R11-06 当前 main 复核包](design/reviews/ui-finesse-round3-20260915/evidence/R11-06-CURRENT-MAIN-RECHECK-20261015/README.md)。Actions 终态未确认。
- R00-08 旧 source `55ed3a17` 的搜索/焦点类 `13/13` 证据保留；本轮仅登记键盘源 R21 改变并复跑 `3/3`，合并行为结果 `14/14`。baseline source `e221feb3`、freshness `14 fresh / 0 stale`；package identity 未提供。详见[R00-08 当前 main 复核包](design/reviews/ui-finesse-round3-20260915/evidence/R00-08-CURRENT-MAIN-RECHECK-20261015/README.md)与[R00/R01 freshness 复核](design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-FRESHNESS-RECHECK-20261015/README.md)。
- R12-01 既有四阶段实现已复核，`3f6b355f` 只增加真实生产 `SaveCenterView` 绑定状态行为：失败阶段/执行待写入、活动 PreRestore 与集合更新均实际可见。SDK8.0.423/VSTest17.11.1 主类 `8/8`、冲突 `4/4`、重验 `2/2`；定向 Release 项目构建 `0/0`，XAML `24/24`。完整范围与未验边界：[R12-01 当前 main 复核包](design/reviews/ui-finesse-round3-20260915/evidence/R12-01-CURRENT-MAIN-RECHECK-20261015/README.md)。Actions 终态待确认。
- R12-02 复用 `6cc3a618` 的 readiness service/DTO/显示映射；`d6640887` 只补实际生产详情卡运行时行为。当前主机 SDK9.0.302/VSTest17.14.1：Worker `15/15`、Core `40/40`、Playnite `11/11`，合计 `66/66`；XAML `24/24`、source validation/diff check 通过。证据：[R12-02 当前 main 复核包](design/reviews/ui-finesse-round3-20260915/evidence/R12-02-CURRENT-MAIN-RECHECK-20261015/README.md)。不是完整 solution build；CI SDK8/SDK10.0.401 另行追踪。
- R12-03 既有 `c0adb1b7` 生产实现复核满足；当前 main 的 `MaintenanceView` 行为测试 Playnite `3/3`、Worker `4/4`、XAML `24/24`，SDK9.0.302/VSTest17.14.1，非完整 solution build。长路径完整选择/Copy 可执行状态、80 项 Recycling 有界视口、异盘/相似根负例和索引/目标无写入见[R12-03 证据包](design/reviews/ui-finesse-round3-20260915/evidence/R12-03-CURRENT-MAIN-RECHECK-20261015/README.md)。不写系统剪贴板或用户目录。
- CI 检查点 #920/run `36677346317`（commit `77de7f4`）与 #919/run `36675309812` 已分别 Failure（7m04s、13m02s）；两个 SDK `10.0.401` 完整日志已从既有归档读取。它们在 `ProductionShellChromeSourceTests` 动画正例中因旧夹具未控制系统动效策略而失败；CI runner 关闭系统动画后，生产代码无活动时钟是预期结果。`48b78b3b` 已为相关夹具加入系统策略输入，未改生产策略。归档和哈希见 R07-08 证据包；不能将旧 run 计作当前提交 CI 通过。
- 外机 R09 再次报同一 `Expected 2 / Actual 1`、stack line151；用户表示已拉取最新 Git，但故障片段不含机器 HEAD/工作树/source/DLL/PDB identity。旧断言来源与当前源码行号仍不一致，旧测试程序集/符号映射是待证推断，根因未确认。用户暂时无法补充信息，不阻塞独立任务；详见[R09复核](design/reviews/ui-finesse-round3-20260915/evidence/CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md)。

### 2026-10-14 当前续跑点（历史记录）

- 用户附加的 #970/#971 三个 diagnostics ZIP SHA 与已审副本一致：#970 SDK10.0.401 是取消测试竞态，#971 所附 SDK8.0.423 summary 是 Q14 fixture 前提错误；两者都已由 `599d2fd3` 修正。当前 main 在 SDK9.0.302 定向重跑各 `1/1`，详见[复核包](design/reviews/ui-finesse-round3-20260915/evidence/CI-970-971-DIAGNOSTIC-TRIAGE-20261014/README.md)。#970 Actions 已失败；#971 官方 run 仍显示 In progress，无 SDK10 原始日志。SDK9 结果不替代 CI SDK8/10。
- R00/R01 受 Q24-07 共享资源改动影响的四条 freshness 已在 `ae8f3be9` 上复核；本机 SDK `8.0.423` Release `0/0`、XAML `24/24`、相关 WPF `163 passed / 39 skipped / 0 failed`，baseline `14 fresh / 0 stale`。证据：[当前复核包](design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-Q24-07-CURRENT-MAIN-RECHECK-20261014/README.md)。
- #978/run `38509928015` 已失败：SDK8 命名 job `115675846585` 与 SDK10.0.401 命名 job `115675846886` 均在编译/测试失败；完整日志页面要求登录，当前无包内容，runner SDK 与根因未知。保持不盲改；SDK8 本机证据不替代 SDK10。
- Q24-08 长文案/辅助输入当前 main 受控行为 `15/15`，见[证据包](design/reviews/ui-finesse-round3-20260915/evidence/Q24-08-CURRENT-MAIN-LONG-TEXT-ASSISTIVE-INPUT-RECHECK-20261014/README.md)；真实 Windows 中文 IME、Playnite/Narrator/系统剪贴板与 Tooltip 视觉仍待验。
- Q25-01 当前 main 热态响应 `13/13`，SDK `8.0.423` Release `0/0`、XAML `24/24`；2000项预热5+30样本，p50/p95/max=`45/60/60ms`，输入到受控可见容器20样本=`46.214/75.782/76.334ms`。本机自动条件满足、无视觉改动、Host external/Final incomplete，见[当前 main 证据包](design/reviews/ui-finesse-round3-20260915/evidence/Q25-01-CURRENT-MAIN-HOT-RESPONSE-RECHECK-20261014/README.md)。
- Actions #990/run `38553608412` 对应 main `8822e32c`，已失败：SDK8 默认 job `115817208147` 的“编译与测试”及 SDK10.0.401 job `115817208303` 的“隔离工作树并运行 Release 编译与测试”均 exit 1。公开 job 页只有通用摘要，日志 API 返回 403 rate limit，页面没有诊断 ZIP 下载入口；根因未知，不凭摘要或 SDK9 结果修复。SDK9 结果不替代 SDK8/SDK10.0.401。
- R09-05 外机失败复核：当前 main `8822e32c` 带提交身份的 SDK9 定向构建 2/2；最新测试源码与 965fe1b2 一致。外机第151行与历史 Chrome 2 DIP 断言特征吻合，但 HEAD/DLL/PDB 未知，错配仅为合理推断。证据：[R09 复核包](design/reviews/ui-finesse-round3-20260915/evidence/CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md)。
- R08-05 页面切换和 R07-08 resize 的当前 main 受控行为证据已完成；R08-05 为 SDK9 44/44，R07-08 为 1/1。真实 Playnite、物理 DPI/跨屏、最终呈现、UIA/读屏、ETW/宿主性能仍待外部验收；SDK8/SDK10.0.401 CI 另行追踪。
- R10-02 当前 main 复核：`6eac7b90` 源码基线，锁定 SDK `8.0.423` / VSTest `17.11.1`，Playnite/Tests Release 定向 build `0/0`，行为 `32/32`、XAML `24/24`；包含生产 Shell 在同名游戏中实际选择指定 `PlayniteId` 的 STA 行为测试。未改生产实现。Actions run ID/终态待查，SDK8 不替代 SDK10.0.401。证据：[R10-02 当前 main 复核包](design/reviews/ui-finesse-round3-20260915/evidence/R10-02-CURRENT-MAIN-RECHECK-20261015/README.md)。
- R10-03 搜索快捷键已有实现复核通过：最新 main SDK `9.0.302`、VSTest `17.14.1`，筛选相关行为与接线 `12/12`；隔离 testhost 中复核 Ctrl+F 作用域和 Ctrl+Z/C/未修饰 F 负例。未验证真实 Playnite、物理输入/IME、UIA 或呈现。证据：[R10-03 当前 main 包](design/reviews/ui-finesse-round3-20260915/evidence/R10-03-CURRENT-MAIN-RECHECK-20261014/README.md)。
- 2026-10-15：R10-03 当前 main 搜索路由复核已包含在 `6cf27923` 全解方案 SDK9 build；快捷键策略/实际焦点及相邻回归 `13/13`，R09 `2/2`。本机无仓库固定 SDK8.0.423，SDK9 结果不替代 SDK8/SDK10.0.401 CI；Actions 状态仍需查询。证据：[R10-03 current main](design/reviews/ui-finesse-round3-20260915/evidence/R10-03-CURRENT-MAIN-RECHECK-20261015/README.md)。
- R10-04 当前 main `6cf27923` 的帮助功能已满足受控行为条件：生产 Shell Popup 启闭与当前媒体页 Ctrl+F 正例，实际 Shell 命令禁用后的空列表负例；R10 邻接行为 `18/18`、R09 `2/2`，SDK9 全解方案 Release build `0/0`。本机缺 SDK8.0.423，当前 Actions run ID/终态未知，不能签收远端 CI 门禁。证据：[R10-04 当前 main 包](design/reviews/ui-finesse-round3-20260915/evidence/R10-04-CURRENT-MAIN-RECHECK-20261015/README.md)。下一项 R10-05 筛选预设。
- R10-05 当前 main 复核：补实际生产 DashboardViewModel 筛选快照/应用方法行为，验证标量状态恢复和动态选项过期回退“全部”；生产重命名/删除取消保护、非法旧值/重复 ID 回退也通过。SDK9.0.302 全解方案 Release build `0/0`，预设 `6/6`、响应式 `8/8`、工具栏 `1/1`，合计 `15/15`。SDK8/SDK10.0.401 Actions 终态待推送后查；没有访问真实用户设置。证据：[R10-05 当前 main 包](design/reviews/ui-finesse-round3-20260915/evidence/R10-05-CURRENT-MAIN-RECHECK-20261015/README.md)。下一项 R10-06。
- R11-04 当前 main 本机行为证据：复用备注元数据、解析、SQLite upsert 和稳定 BackupId；SDK8 Playnite 4/4、Worker 1/1，SDK9 同类 5/5。没有真实存档或 Playnite Host 写入。R09 新外机日志仍缺 HEAD/DLL/PDB；当前 SDK8 `40b0a980` 本机对照 2/2，日志仍不能确诊外机根因。见[R11-04 证据包](design/reviews/ui-finesse-round3-20260915/evidence/R11-04-CURRENT-MAIN-RECHECK-20261014/README.md)和[R09 外机复核](design/reviews/ui-finesse-round3-20260915/evidence/CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md)。
- 历史 R11-05/R11-06 已完成；本轮下一执行项为 R12-04 当前 main 复核（在 Worker 测试修正的 SDK8/SDK10 CI 终态成功后）。此前 CI 状态和日志访问限制仅作历史记录。

| ID | 优先级 | 状态 | 负责角色 | 依赖/关联 |
| --- | --- | --- | --- | --- |
| GOV-003 | P1 | IMPLEMENTED | 当前审阅 Agent | 本次文档/账本审阅；证据见下 |
| CLOSE-IPC-01 | P0 | IMPLEMENTED | 开发/测试 Agent | 取消 IO/EOF 分类与夹具释放已修复；R19-08/R01-08 |
| CLOSE-CI-01 | P0 | IMPLEMENTED | 构建维护 Agent | #968 当前源码 SDK8/SDK10 构建、测试、Render QA、静态审查、扩展包和诊断上传均成功；历史不可读失败仍单列，不猜测根因 |
| CLOSE-SDK-01 | P0 | IMPLEMENTED | 构建维护 Agent | #968 分别验证仓库 SDK8 `8.0.423` 与 SDK10 `10.0.401` job；本地 helper 失败由源码门禁复现后补 BOM |
| CLOSE-WRAP-01 | P1 | IMPLEMENTED | UI 开发/测试 Agent | Q14 `1/1` 验证 654 DIP 单行边距；当前候选 Actions #1039 SDK8/SDK10 双 lane 全量 WPF 通过，SDK8 Render QA 通过。真实交互桌面上的指针用例仍因 GetCursorPos error 5 未验 |
| CLOSE-OPTICAL-01 | P1 | IMPLEMENTED | UI 开发/测试 Agent | 当前 main R02 几何 `7/7`、间距 8 DIP/中心差 0；Actions #1039 双 SDK 全量 WPF 与 SDK8 Render QA 成功收口 |
| CLOSE-FOCUS-01 | P1 | IMPLEMENTED | UI 开发/测试 Agent | R05 可见 ComboBox 获焦/离焦、Popup 关闭及紧凑布局负例已覆盖；Actions #1039 两 SDK 全量 WPF `124/124` 类通过。外机 R09 的旧测试源映射差异单列，非本任务复现 |
| CLOSE-PICKER-01 | P1 | IMPLEMENTED | 开发/测试 Agent | Dispatcher 负例与修复正例、本机完整 Release/WPF 门禁已通过；run `38222818374` 终态失败，根因并入 CLOSE-CI-01 |
| CLOSE-EVID-01 | P1 | IMPLEMENTED | UI 验证 Agent | main `599d2fd` freshness 14/14；package identity 未提供并继续单独待验，四条受影响基线记录已按当前证据校正，见 R00/R01 当前 main 复核包 |
| CLOSE-HOST-01 | P1 | BLOCKED_ENVIRONMENT | 宿主验证 Agent | ENV-001；R00-06/R23-04；Media Inbox |
| CLOSE-REG-01 | P1 | BLOCKED_ENVIRONMENT | 回归验证 Agent | 用户原 Q06/R08 日志、断言和 DLL 身份 |
| CLOSE-REL-01 | P1 | BLOCKED_ENVIRONMENT | 发布验证 Agent | CI 绿、ENV-001、独立可丢弃存档/测试远端、当前候选包 |
| CLOSE-UX-01 | P2 | PROPOSED | UI 开发 Agent | 本轮没有新增可复现页面缺陷；按需准入 |
| CLOSE-ARCH-01 | P2 | PROPOSED | 维护 Agent | 实际修改痛点/热点证据，避免大重写 |

### 用户持续授权的 Round3 Q/R 执行线（2026-10-13）

用户已明确要求持续推进 `UI_FINESSE_ROUND3_192_TASKS_2026-09-15.md`，每阶段无需再次询问。R04-03 当前 main 关闭分支补证已完成；R04-04 至 R04-08 已有满足证据。R05-01/02 `11/11`；R05-03 `7/7`（无闪屏呈现帧，部分满足）；R05-04 `3/3`；R05-05 不适用；R05-06 `28/28`；R05-07 当前 main `34/34`；R06-01 `6/6`；R06-02 `7/7`；R06-03 当前 main 行为复核 `6/6`；R06-04 当前 main `R06ClipboardBehaviorTests 5/5`、R22 复制反馈 `2/2`。[R05-07 证据](design/reviews/ui-finesse-round3-20260915/evidence/R05-07-CURRENT-MAIN-RECHECK-20261013/README.md)；[R06-01 证据](design/reviews/ui-finesse-round3-20260915/evidence/R06-01-CURRENT-MAIN-RECHECK-20261013/README.md)；[R06-02 证据](design/reviews/ui-finesse-round3-20260915/evidence/R06-02-CURRENT-MAIN-RECHECK-20261013/README.md)；[R06-03 证据](design/reviews/ui-finesse-round3-20260915/evidence/R06-03-CURRENT-MAIN-RECHECK-20261013/README.md)；[R06-04 证据](design/reviews/ui-finesse-round3-20260915/evidence/R06-04-CURRENT-MAIN-RECHECK-20261013/README.md)。R06-03 的受控 hover 不是 OS 指针证据；R06-04 的 WPF routed event 与 fake clipboard 不是实际键盘/系统剪贴板证据。R06-05 当前 main 本机 `2/2` 与双主题 `372 PNG` 已复核，#961 Actions 仍 `In progress`；R06-06 当前 main Playnite `34/34`、Worker `9/9`，见当前复核包。下一项先跟 #961 到终态，再复核已实现的 R06-07。Actions #947 (`38295749414`, `90454586`) 与 #948 (`38298023983`, `fdeeec8`) 截至2026-10-12 20:11 UTC 仍 `In progress`；`e277a804` commit checks 于20:04 UTC启动，截至20:11仍显示 `Re-running jobs...`，没有可见终态/run ID；匿名 REST API限流、日志需登录。用户指定的 #919/#920 (`36675309812`/`36677346317`) 完整日志 ZIP 已在 R07-08 证据包并按 SHA 校验；两者 CI SDK `10.0.401`，旧源码两项侧栏 transition 活动时钟断言失败。#960 较早页面曾显示 `In progress`；2026-10-13 后续已确认 terminal failure，job `115069541561` 日志仍不可读，见下方状态校正。前一 #946 已终态 failure，公开摘要“编译与测试”exit1、artifact 239 KB / SHA-256 `083b3f72bcacb1fdd36c413517d075fe5f27a921d1fb79024ad13882e3cc2cf7`；完整日志与 artifact内容不可读，匿名 API HTTP403，失败根因/CI SDK未知，不盲改。本机 SDK `8.0.423` 不代替目标 CI SDK `10.0.401`。

### 2026-10-13 R00/R01 当前 main 证据校正

R00-03/R00-04 在 main `5762c874` 上定向行为 `25/25`，更新 baseline 后 freshness `14 fresh / 0 stale`；其余 12 条源路径未命中，R01 既有证据不重跑。package identity 未提供，真实 Playnite/物理呈现仍未验。详情：[R00-03/R00-04 证据](design/reviews/ui-finesse-round3-20260915/evidence/R00-03-04-CURRENT-MAIN-RECHECK-20261013/README.md)。下一项按用户持续授权检查 R06-05 已有列头能力与依赖。

### 2026-10-13 #968 当前 main 复核（覆盖上述历史“下一项”）

代码提交 `4aa1ec7` 对应 #968 / run `38366158855` 已成功，SDK8 `8.0.423` 与 SDK10 `10.0.401` 两 job 分开验证。SDK8 完成 Render QA、WPF 静态审查和扩展包门禁；两个版本的 build/test 均成功。R00/R01 freshness summary 在该 source identity 下为 `14 fresh / 0 stale`。本阶段不重跑按类 suite，也没有包身份；历史 run 的不可读失败根因继续保留为未知，不由新绿 run 反推。完整范围、artifact hash 和边界见 [复核包](design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-CURRENT-MAIN-RECHECK-20261013/README.md)。

R05-03 已按既有隔离 Playnite 流程执行 fail-closed 预检，bootstrap 在插件安装前因进程未优雅退出而停止，未采集真实 Popup 帧。下一项转至 Round2 Q23-06 目录编辑器视觉复核：使用 RenderHarness、生产基线与合成路径/状态；离屏图只作为逻辑 DIP 证据。R00-08 OS IME/真实输入、package identity、实际 Playnite 呈现、物理 DPI/跨屏、UIA/Narrator、ETW/宿主性能仍是独立边界。

### 2026-10-13 #969 后续状态校正

文档 HEAD `d56d23e` 对应 #969/run `38377673615` 已成功，代码基线 `4aa1ec7` 对应 #968 的 SDK8 `8.0.423` 与 SDK10 `10.0.401` jobs 也都成功。用户提供的 #967 ZIP 是 SDK10 job `115178885908`、artifact `11795654027`，不是失败的 SDK8 job `115178885652`；其独立 R08-01 TRX 为 `2/2`，但用户机报告 `1/2` 仍待原始失败日志。#960/run `38327521241` 当前已确认 terminal failure：13m54s，job `115069541561` “编译与测试” exit 1；日志 endpoint 404、REST 403，SDK/根因未知。R05-03 isolated bootstrap blocker 与仍存活的 PID `34512`/4 个 CefSharp children 见[预检记录](design/reviews/ui-finesse-round3-20260915/evidence/R05-03-CURRENT-MAIN-RECHECK-20261013/README.md)；保持 profile，不强杀或重跑。下一项 Round2 Q23-06，使用生产基线、RenderHarness 和合成目录路径/只读/错误状态完成离屏双主题核查；不把离屏结果表述成宿主验收。

### 2026-10-13 Q23-06 当前 main 复核

Q23-06 已完成本机代码、测试与视觉核查：Settings ComboBox 的选中路径显示修复；RenderHarness 双主题/两尺寸/两路径状态 `8/8`；Playnite 定向结果 `170 passed / 0 failed`（39 skipped）；XAML、source、WPF 静态与 full Render QA 均已记录。视觉列更新为通过，真实 Playnite/物理 DPI/OS 输入与 ACL/网络共享仍未验证，宿主 external、最终 incomplete。证据：[Q23-06 复核包](design/reviews/ui-finesse-round2-20260913/evidence/Q23-06-CURRENT-MAIN-RECHECK-20261013/README.md)。前置 #970/run `38386326043` 于 2026-10-13 11:44 UTC 仍显示 In progress；本阶段尚待 push/新 Actions。下一可执行任务 Q23-08：先核对设置底部动作栏既有命令、短窗可达和危险/导入迁移确认，不假设计划表所列功能缺失。

### 2026-10-13 Q23-08 当前 main 复核

Q23-08 代码提交 `c377ab218dad54d81d7dd22202f0bf283f250f97` 复用现有 `SettingsResetCatalog`、portable import preview/apply 和 Playnite Save/Cancel 宿主契约。没有重复绘制 Save/Cancel，也没有重写 DTO、服务或迁移流程；只为现有 Reset All 按钮增加测试定位名，并通过内部测试 delegate 驱动原 Yes/No Warning 分支。Reset All 实际 Click：No 不改 fingerprint，Yes 只改安全设置草稿并保留路径/云端目标，之后 `CancelEdit` 恢复既有草稿。导入确认 No/Yes 与兼容预览由行为测试覆盖，handler 的 preview→confirm→apply 顺序由源/预览测试核对。

本机 SDK `8.0.423`，Playnite `net462` / tests `net472` Release build `0/0`；Q23 confirmation/import/reset filter `10/10`，Settings draft-cancel `1/1`，合计 `12/12`；XAML `24/24`、`validate-source.py` 通过，WPF 静态审查 `0 errors / 29 warnings / 177 info`。RenderHarness Light/Dark × `1040×700`/`720×640 DIP` 检查短窗默认折叠、展开后的全部/单字段重置、四分类动作经滚动可达及迁移页导入/导出可达。报告与图在[复核包](design/reviews/ui-finesse-round3-20260915/evidence/Q23-08-CURRENT-MAIN-RECHECK-20261013/README.md)。视觉栏“通过”；真实 Playnite host Save/Cancel、文件对话框、物理 DPI/OS UIA/IME/presented frame 未验证，Host “外部阻塞”，Final “未完成”。Demo 原目录不可用，沿用恢复生产基线。

用户提供了 #970 与 #971 的 diagnostics ZIP。#970 SDK10 (`10.0.401`) 绑定源码 `c9f9179`，Release build `0/0`，Worker 取消终态用例因测试未等待 token callback 注册而出现 `expected 1, actual 0`；测试已加 registration-ready 握手，生产协调器未变。#970 SDK8 (`8.0.423`) 同源 solution build summaries 通过。#971 SDK8 (`8.0.423`) 绑定源码 `5a6647c`，Q14 toolbar 测试把 `TaskFilterPreset` DTO 换成字符串，导致 `DisplayMemberPath=Name` 无选中文本并测到隐藏 presenter；测试已恢复真实 DTO、选中态和可见/有尺寸的 presenter，未改生产布局。ZIP SHA、栈与本机复核见[诊断复核包](design/reviews/ui-finesse-round3-20260915/evidence/CI-RUN-970-971-TEST-RECHECK-20261013/README.md)。#971 SDK10 diagnostics ZIP 仍未提供，workflow 当前终态也未从可读日志确认；不据 SDK8 结果推断。提交 `c377ab2` 与 `599d2fd` 当前尚待 push，推送后须分别跟踪 SDK8/SDK10 Actions 至终态。本机 SDK8 不替代 SDK10。

本阶段同步证据的当前 main 身份为 `599d2fd38dd5c9af523a0890a33e73fc78ac0bfa`。R00/R01 中受 `WpfUiProduction.xaml` 与 RenderHarness 源变更影响的四条 baseline 已在定向行为和审计复核后更新；freshness `14/14`，审计仍有 `13 MEDIUM` 窄布局提示。详情及未验边界见[R00/R01 当前 main 证据](design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-CURRENT-MAIN-RECHECK-20261013/README.md)。

下一可执行小批 Q24-02：复核当前 main 生产 PageHost 在常见短窗下的实际内容几何、列表首尾可达和恢复宽窗；使用内容区实测值，不拿外层请求尺寸替代。Q24-01 的物理 DPI/Playnite 实机部分仍待验，不阻塞独立任务。

### 2026-10-14 Q24-01 当前 main 分数 DPI 复核

代码提交 `3ca41ec9a5a3f3c6888db72ba3a6254b046dcd40` 新增受控 WPF 行为夹具，没有生产 UI 改动。隔离 SDK `8.0.423` Release 定向测试 `1/1`；生产资源下以 RenderTargetBitmap 指定 96/120/144/168/192 DPI，核对中英文字 fit、图标/1 DIP 边线像素及裁切/永久缩放负例。五张目标尺寸 PNG 已人工查看，受控视觉列记为“受控通过”。具体 TRX、PNG、产物 SHA 和测试范围见[Q24-01 证据包](design/reviews/ui-finesse-round3-20260915/evidence/Q24-01-CURRENT-MAIN-LOGICAL-DPI-RECHECK-20261013/README.md)。

该证据仅是目标 DPI 离屏模拟：WPF Visual 继承本机系统 `DpiScale=1.5`，系统 DPI 为 144，只有 `DISPLAY1`；未改变物理显示设置、移动真实窗口或启动 Playnite。物理 DPI/跨屏、真实宿主/Popup、最终呈现帧仍待验，Q24-01 Final 保持“未完成”。#972/run `38421480496`（前置 SHA `8546acab`）已终态成功；SDK8 `8.0.423` 与 SDK10 `10.0.401` 两 jobs 均通过。Q24 代码 `3ca41ec9` 尚未包含于 #972，推送后需跟新 SDK8/SDK10 Actions 终态。

### 2026-10-14 Q24-05 焦点语义当前 main 复核

- Q24-05 在受控 WPF 行为范围内已满足，复用 b20f1a4b 与现有状态模板，没有改生产代码。隔离 SDK 8.0.423 Release 构建 0/0，R21 picker、R09 focus outline、R23 导航/页签/生产表格状态共 7/7；覆盖 Focus/Selected/Current 区分、双主题、失焦/禁用负例和关闭选择层后回到可见触发器。
- 测试宿主与程序集身份、TRX、真实未验边界见 [Q24-05 证据包](design/reviews/ui-finesse-round3-20260915/evidence/Q24-05-CURRENT-MAIN-FOCUS-RECHECK-20261014/README.md)。Q24-05 控制行为记为已满足；真实 Playnite、OS UIA/读屏输入、焦点环像素、物理 DPI/呈现帧仍待验。
- #970/#971 三个用户 ZIP 为已审 SHA 副本，未增加 #971 SDK10 日志。#973/run 38429051152 仍 In progress；#974/run 38436999880 的两个 job 页仍为 Re-running jobs、日志需登录、runner SDK 未核实。跟 CI 至成功或失败终态，不以本机 SDK8 替代 SDK10。
- 下一项 Q24-06：当前 main UIA Name/HelpText/LabeledBy/native Pattern 行为复核；真实 Narrator/Playnite 外部验收另列。Q24-03 需要第二台物理显示器。

### 2026-10-14 Q24-06 UI Automation 当前 main 复核

- `af8ed430` 更新 11 个生产 DataGrid 的直接 Name，并补齐缺失的维护表 HelpText；R21Automation 的实际生产 peer 与 Selection 行切换测试 `24/24`。R00-06 关联媒体几何/锚点 `13/13`，baseline 更新后 R00/R01 freshness `14/14`。
- Q24-06 当前为 `IMPLEMENTED`：进程内 WPF peer 的 Name/HelpText/Grid/Table/Selection、生产图标按钮语义和进度 LiveSetting/焦点负例已验。实际 Narrator/Playnite 口播仍待隔离宿主验收；不可将 peer 测试描述成真实读屏。
- 证据：[Q24-06 当前 main 复核包](design/reviews/ui-finesse-round3-20260915/evidence/Q24-06-CURRENT-MAIN-UI-AUTOMATION-20261014/README.md)。本机 SDK `8.0.423`；推送后的 SDK8/SDK10 Actions 仍待终态，#971 SDK10 日志缺失不阻塞独立任务。
- 下一项 Q24-07 高对比组合；Q24-06 实际读屏待验，Q24-03 需第二物理显示器。
### GOV-003：完成度与活动入口校准

- **范围/结果**：重新统计 R192/Q208，R08-01 用户故障单列；撤下旧无分母百分比；三份当前入口精简并完整归档；补当前 CI、本地 IPC 失败、SDK 漂移与 freshness。
- **验收**：历史快照字节哈希一致，链接和统计校验通过，记录本轮实际构建/测试（包括失败）；生产代码、版本、单项 Q/R 状态不变。
- **证据**：[本轮审阅包](ai/evidence/completion-review-20261005/README.md)、[完成度评估](FEATURE_COMPLETION_ASSESSMENT.md)。本任务完成仅代表审阅文档完成。

### CLOSE-IPC-01：Named Pipe 取消/重放行为失败

- **2026-10-05 完成**：握手/独立管道/IAsyncLifetime 排除夹具干扰后仍复现取消引发的 IOException/EOF 竞争，生产分类现保留 host/caller/timeout 与未知提交语义；20 轮专项 200/200，source 470 passed/18 原有 skipped/0 failed，Core125/Worker357。全量 WPF 在第24类 Q14 失败，不能称整体通过。修复前后 TRX、DLL SHA/MVID、负例和限制见 [证据](ai/evidence/close-ipc-20261005/README.md)。下方保留原任务范围与完成定义。
- **范围**：复核 `WorkerIpcClientBehaviorTests` 两个失败方法，区分测试服务时序/共享管道/失败清理与生产取消分类；同一 DLL 独立复测，并在失败路径确保测试服务被释放。
- **证据**：本轮 SDK8 全量 source 465/2/18；一条预期 `WorkerIpcCancellationException` 实得 `WorkerRequestException`，另一条“所有的管道范例都在使用中”。这是观测事实，尚非生产根因。
- **完成条件**：保留 RequestId、MayHaveBeenAccepted、有限重放和 caller/host 取消的正负例；问题能由受控触发或清晰时序解释，修复后全量 source 和相关套件通过。一次复跑变绿不能抹除初始失败。
- **非目标**：不放宽异常类型断言、不改 skip、不盲增 timeout；不触碰生产固定管道或用户进程。若只属测试夹具，生产代码不改。

### CLOSE-CI-01：失败诊断产物与 CI 恢复

- **范围**：为 build/source/WPF 分类阶段保存实际退出码、SDK/源码/DLL 身份、console 与 TRX；CI 使用失败时也执行的上传步骤。构建失败时无需伪造不存在的 TRX；日志需脱敏。
- **前置/最新诊断**：旧 run 日志 HTTP403 是历史事实；本轮取得 run37329446647 / 226bce6b 的完整失败步骤。实际 SDK10.0.401；Core/Worker通过，两条 ProductionShellChromeSourceTests 活动动画断言失败。先核对系统动画偏好与测试前提，再补失败产物；不归因于 IPC。[日志](ai/evidence/close-ipc-20261005/ci-log-37329446647.txt)。
- **源码提交复核**：d57a613f的新run37335412450最终仍失败于同两条侧栏动画断言；IPC source已通过，不回退已修复的IPC任务。[新日志](ai/evidence/close-ipc-20261005/ci-log-37335412450.txt)。
- **完成条件**：受控失败也能取得完整方法/断言/堆栈与非零退出码；每个 WPF 类有独立结果、不相互覆盖；成功路径 artifact 正常；修复实际根因后当前候选 CI 编译/测试/渲染/打包均通过。单做上传只算部分完成。
- **2026-10-07阶段**：已接入console/TRX/实际退出码/SDK/源码与DLL身份及always上传，PS5.1/7受控失败与启动错误自测通过；侧栏22/22、source470/18、WPF前23类通过，第24类Q14两次先失败于错误态高度36→37 DIP，后89类未执行。全CI恢复未验，保持IN_PROGRESS。[证据](ai/evidence/close-ci-20261007/README.md)。
- **提交后**：48b78b3b已推送main，clean构建0/0与侧栏22/22；run37565955357记录时编译/测试进行中，先取该run终态与产物再统一SDK。freshness实算6/8，未更新baseline。
- **终态复核（SDK阶段）**：run37565955357与37566483862失败时均实际上传且已下载诊断；SDK10侧栏22/22通过，native ClientAreaAnimation=false有TRX记录；均在Q14错误态36→37 DIP失败，后89类未执行。补修诊断器多PATH匹配拼接，PS5.1/7正负例通过。完整CI仍未恢复。[SDK阶段](ai/evidence/close-sdk-20261007/README.md)。
- **入口补修**：91bfb6e1/run37569191845自测断言通过后仍退出非零，后续全部未执行/实际artifact0；同Actions包装本地复现。自测在finally后显式exit0、日志纳入artifact并只上传JSON/TXT/TRX；PS5.1/7包装退出0且负例保留。新CI待验，不提前关闭。
- **入口实际runner复核**：c35d9047/run37570629705两项自测步骤success，实际选择SDK8.0.423，预装SDK9/10未改变基线；失败artifact已下载，只有JSON/TXT/TRX。Core125/Worker357/source470通过，仍在修复前Q14错误态高度失败，CI整体尚未恢复。见[行距阶段证据](ai/evidence/close-wrap-20261007/README.md)。
- **2026-10-12 最新 Actions**：main `f13b9e05` 对应 run `38222818374` 已失败，失败步骤为“编译与测试”。下载 workflow logs 返回 HTTP403 `Must have admin rights to Repository.`；诊断 artifact 可列出但匿名下载 HTTP401。没有完整日志，CI 失败根因未知；不得按 exit code 猜测或修改代码。
- **2026-10-12 后续 run**：文档提交 `da6388e8` 对应 run `38226955461`（run931）也终态 failure 于“编译与测试”。Jobs API 未提供更细根因；完整日志仍 HTTP403，唯一诊断 artifact `11735869371`（488,867 bytes）下载 HTTP401。该提交不含生产代码改动；CI 根因仍未知，待取得完整日志后处理。
- **2026-10-12 最新 run**：提交 `a21440834865551cdb6894830548af51ad1d8e78` 对应 run `38244963620` 终态 failure 于“编译与测试”。GitHub 页面只显示 `Process completed with exit code 1`；未认证 jobs/log API 受 rate limit HTTP403，job 页面完整日志需登录。artifact `11745496487`（478 KB，SHA-256 `11de81935473ce3da60739d481c768eba2af589d2c76cc819cf3977fd4daea1e`）可从页面确认元数据，下载路由404。无完整日志，根因未知，不猜测修复。
- **2026-10-12 后续 run**：提交 `90099a09104677708a5829cf7e6a5d22b6b414ff` 对应 run `38247748056` 终态 failure 于“编译与测试”；其他结构、SDK和诊断自测步骤成功，render/static/package跳过。页面 annotation仅 `Process completed with exit code 1`；未认证Jobs/log API rate-limit HTTP403。artifact `11747310690`（479 KB，SHA-256 `8586bcace066ca779c72dbf9fbea1843c19a49133d619b9a6000e6e8dc6e79c5`）路由404。失败命令/断言未知，不盲修。
- **2026-10-12 后续 run**：文档/证据提交 `3df542af` 对应 run `38249983091`（#934）终态 failure，job `114813463440`。SDK安装、源码结构、诊断自测和 SDK 基线步骤成功；“编译与测试”失败，后续渲染/静态审查/打包跳过。CI SDK10.0.401；页面仅 `Process completed with exit code 1`。Jobs/artifact 元数据API可读，完整日志HTTP403；artifact `11747469284`（488,428 bytes，SHA-256 `662aba958e53e7a6d8ccd837a319fd93e8df91d2309f7977b0af1a84935f79fa`）下载HTTP401。失败命令/断言/堆栈未知，不猜修。
- **2026-10-12 后续 run**：`4d166f017dcba8fbb07980ec21b690267b9ff377` 对应 Actions run `38253621689`（#935，job `114825925174`）已终态 failure，耗时 `9m39s`，失败步骤“编译与测试”exit `1`。CI SDK10.0.401；job logs HTTP403；artifact `11750360097`（478 KB，SHA-256 `805b8eff2797816cf4b8f0fded68de024da80f3dc585993c02373be29e8ef46c`）下载HTTP401。失败命令/断言/堆栈未知，不猜修复。
- **2026-10-12 后续 run**：提交 `8e71feb41aeb20738aa80f0cbb5d05d0c6aeb0af` 对应 Actions run `38262296773`（#936，job `114855606547`）已终态 failure，耗时 `6m19s`，失败步骤“编译与测试”exit `1`。CI SDK10.0.401；logs API HTTP403（rate limit）、job 页面要求登录；artifact `11752913387`（476 KB，SHA-256 `d78fb86cee5d81520fa81cebe65434cded562c18bd6fda46af0a91b8081bb021`）匿名 suite 下载HTTP404。失败命令/断言/堆栈未知，不猜修复。
- **2026-10-12 后续 run**：R03 证据提交 `32c0e1e21d1fbfe0b8e46d765b4d2bd3161c46ed` 对应 Actions run `38265390606`（#937，job `114866141338`）终态 failure，8m06s 后“编译与测试”exit1。Node.js 20→24 弃用警告；artifact `GameSaveCenter-build-diagnostics` 478 KB，SHA-256 `31fa1b2bf29661b435ede1c749f854dc358e0e2c2279eb0c72d0a51eb50cd47e`。Run/job logs 与 artifacts API均HTTP403，job页要求登录，未获 artifact ID/完整日志，失败根因未知，不盲改。当前 workflow 指向 global.json8.0.423；任务补充的CI SDK10.0.401与当前声明有待核差，本run实际SDK未证实。
- **2026-10-12 后续 run**：R04-01 证据提交 `8d0e2f66` 对应 Actions run `38268914513`（#938，job `114878000993`）终态 failure，7m54s 后“编译与测试”exit1。Node.js 20→24 弃用告警；artifact `GameSaveCenter-build-diagnostics` 477 KB、SHA-256 `473d9f10342ba88b696d884576e6fcdeeef3a81f8e54c0102419ea0ea17687dd`。Run logs、job logs、artifact 列表 API均HTTP403 rate limit，job页要求登录；未获 artifact ID/完整日志，失败根因未知，不盲改。当前 workflow 指向 global.json8.0.423，任务补充的CI SDK10.0.401与当前声明有待核差，本run实际SDK未证实。
- **本机 SDK 恢复（2026-10-12）**：通过微软官方 `dotnet-install.ps1` 将 SDK `8.0.423` 安装到隔离目录 `D:\GameSaveCenterTools\dotnet-sdk-8.0.423`，运行时临时前置进程 PATH，系统 SDK/PATH 未改。当前 clean main `4d166f01` Release 全量 `1560 passed / 57 skipped / 0 failed`、0 warning/error，RenderHarness `372` 图 `render-qa OK`；其中 Playnite source `470/18 skipped`、WPF `608/39 skipped`。这满足本机重验，不替代 SDK10 CI 及包门禁。
- **历史本机 SDK 缺失**：在安装前 `global.json` 固定 `8.0.423` 且 `rollForward=disable`，机器当时只发现 `9.0.302`；对 `3df542af` 的 build 于 `sdk-baseline.ps1` 预检退出，未编译/测试；`validate-source.py` 通过。没有以 SDK9 替代。
- **后续身份补口**：独立scripts/render-qa.ps1未主动注入GSC_BUILD_COMMIT；实际只读MSBuild查询（未注入env）得到GscBuildCommit=unknown、InformationalVersion=0.6.73+unknown。本轮render显式传入c35身份且DLL已核对，不能据此声称独立脚本/CI同样绑定；当前UI全量结束并提交后，再修脚本环境保存/恢复与同HEAD身份验证。
- **非目标**：不授予宽泛仓库权限、不隐藏失败、不重复无限触发 CI。
- **2026-10-13 当前候选收口**：`4aa1ec7` 对应 #968/run `38366158855` 终态成功。SDK8 job 实际 `8.0.423`，SDK10 job 实际 `10.0.401`；build/test 成功，SDK8 的 Render QA、WPF 静态审查、扩展包及诊断上传也成功。#967 SDK8 原始日志仍不可读；其源码结构失败由本机同 guard 复现后修复 BOM。新 run 证明当前候选门禁恢复，不反向解释旧 run 的未知根因。详见 [R00/R01 当前 main 与 CI 复核](design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-CURRENT-MAIN-RECHECK-20261013/README.md)。

### CLOSE-SDK-01：明确 SDK 与编译器基线

- **2026-10-07工程阶段**：global.json/CI精确8.0.423、disable、C#12.0，build/独立runner校验实际版本；PS5.1/7真实缺失旧SDK与原latestMajor对照通过。Release0/0、Core125/Worker357/source470通过；当前runner与完整包门禁待签收，保持IN_PROGRESS。暂不新增SDK10兼容通道，未来升级须显式修改并完整验证。[证据](ai/evidence/close-sdk-20261007/README.md)。
- **范围**：统一 `global.json` 与 `setup-dotnet` 的 SDK 选择，记录实际 `dotnet --version`；明确是否需要独立的更新 SDK 兼容通道；评估 `LangVersion=latest` 与选定 SDK 的关系。
- **完成条件**：干净机器与 runner 使用同一已声明基线，完整 Release 和包门禁通过；另一预装大版本不能静默改变基线；缺 SDK 时给可理解错误。目标框架仍保持现有 Playnite/net462 与 Worker/net8 合同。
- **非目标**：不把“固定 SDK 后通过”当成已解释所有历史失败，不顺便升级运行时或所有 NuGet 包。
- **2026-10-13 收口**：#968 的 SDK8 `8.0.423` job 采用仓库锁定基线并完成 Release/test/包门禁；独立 SDK10 job 实际选择 `10.0.401` 并通过 Release/test。没有改变 `global.json`、目标框架或生产 SDK 声明；SDK9 本机结果未用于替代任一 CI job。工作流结果与 helper BOM 修复见 [复核包](design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-CURRENT-MAIN-RECHECK-20261013/README.md)。

### CLOSE-EVID-01：精确补齐当前受影响证据

- **范围**：R00-04/06/07/08、R01-05 的 shell、游戏选框、Media、Trainer 受影响行为和负例；复用现有测试/toolbarprobe/render 入口，不重建审计系统。
- **2026-10-07追加**：本阶段GscMotion与build脚本修改还命中R00-03、R01-01、R01-04。补齐动效完成/重入/清钟、源码根身份及对应负例；提交后按实际HEAD重算，不沿用9/5或只更新SHA。
- **共享输入模板影响**：CLOSE-WRAP-01改动WpfUiProduction.xaml另命中R00-01-02/R00-05；candidate逐项合并历史与待提交路径后为4 fresh/10 stale。补双主题对比/变换及上下文禁用状态，原baseline未改；不得只把原8项改SHA转绿。
- **2026-10-12 当前复核**：在推送身份 `f13b9e05de0cf502ac61e97b0fbad8433b339b3f`、本机 SDK `8.0.423` 下，受影响 WPF/source 测试 `40/40`，对比/搜索/媒体几何/工具栏受控探针通过；原始 30 搜索样本和 9 份 TRX 已归档。10 条 stale 记录更新到当前证据，freshness 为 `14 fresh / 0 stale`；另 4 条无匹配源码变化，保留原证据与身份。包未生成，package identity 仍未提供；远端 CI failure 日志受 403/401 限制，CLOSE-EVID 与 CLOSE-CI 不合并签收。[R00/R01复核包](design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-CURRENT-RECHECK-20261012/README.md)。
- **2026-10-13 当前 main 校正**：source/test identity `599d2fd38dd5c9af523a0890a33e73fc78ac0bfa` 下重跑受影响行为类、RenderHarness probes 和 source audit；针对 `WpfUiProduction.xaml` 与 `RenderHarness/Program.cs` 命中的四项 stale baseline 逐条更换证据后，freshness `14/14`。完整测试计数、审计中保留的 13 条 MEDIUM 与 physical/host/package 边界见[当前复核包](design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-CURRENT-MAIN-RECHECK-20261013/README.md)。
- **状态**：本任务的源码证据复核、负例、身份和 freshness 更新已完成。未提供的 package identity、真实宿主验收与远端 CI 根因分别留在发布/宿主/CI 队列，不阻止本项收口。
- **完成条件**：记录源码身份、测试 DLL SHA/MVID、TRX、具体测量、负例与失败边界；重跑通过后更新原 baseline，freshness 14/14 且 package 身份仍独立核对。
- **非目标**：不仅改 baseline SHA，不把本任务变成整轮视觉或真实宿主验收。
- **2026-10-13 当前 main 复核**：代码检查点 `4aa1ec7` 的 freshness summary 为 `14 fresh / 0 stale`；本次 SDK helper BOM 变更没有命中登记的 R00/R01 源路径，因此没有重复跑按类测试或只改 baseline。package identity `not-provided`，真实宿主与呈现仍独立待验。详见 [当前复核包](design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-CURRENT-MAIN-RECHECK-20261013/README.md)。

### R02-03：当前 main 焦点与 UIA peer 复验

- **2026-10-12**：已实现的禁用原因提示继续满足受控条件。测试只强化可达性证据：STA `Focus()` 后确认 `Keyboard.FocusedElement`；从 UIA peer 读取 Name/HelpText；与生产 Save 紧凑布局提示移入检查器行为合计 `5/5`。没有生产 UI/命令变更。本机 SDK8.0.423 隔离 Release `0 warning / 0 error`、XAML `24/24`；完整 RenderHarness 双主题通用布局 QA `render-qa OK`，不作为禁用状态直接截图证明。[当前复验](design/reviews/ui-finesse-round3-20260915/evidence/R02-03-CURRENT-RECHECK-20261012/README.md)
- **CI 边界**：推送 `da6388e8` 的 run931 已失败于“编译与测试”；其完整日志403、artifact `11735869371` 下载401。后续 `a2144083` 的 run932以及 `90099a09` 的run `38247748056` 也失败于该步骤；均未取得完整日志，API受限。根因未知，不做推断。R02-04与R02-05当前 main 复验已完成。

### R02-04：当前 main 图文光学对齐复验

- **2026-10-12 当前 main `a21440834865551cdb6894830548af51ad1d8e78`**：沿用既有共享按钮修复，没有改生产 UI；Release0/0、XAML24/24、`R02OpticalAlignmentTests 7/7`。真实生产资源 STA 几何输出：16/20 DIP 图标与数字 gap 各8 DIP、中心差各0；中英文基线差≤0.5 DIP，对齐切换及文本/视觉内容往返通过。
- **主题视觉**：当前身份 RenderHarness双主题全量 `render-qa OK`，372 PNG、logical DPI1.00；已核对 Save 1040×700 Light/Dark，并归档两张图。合成离屏数据不证明Playnite宿主、物理DPI或最终呈现。源码门禁通过，WPF静态审查36 XAML/0 errors/29 warnings/177 info。证据及身份见[当前复验包](design/reviews/ui-finesse-round3-20260915/evidence/R02-04-CURRENT-RECHECK-20261012/README.md)。
- **状态**：R02-04继续“已满足”。CLOSE-OPTICAL-01仍 `IN_PROGRESS`，因为整体共享回归/Actions需收口；run932与后续run `38247748056` 均只知“编译与测试”step exit1，完整日志不可读，不能归因该定向测试。R02-05当前 main 复验已完成。

### R02-05：当前 main 命中区与间距复验

- **2026-10-12 当前 main `90099a09104677708a5829cf7e6a5d22b6b414ff`**：原有生产资源行为测试当前复验 `2/2`；按钮中心命中独立实例、空隙命中负例、34 DIP 命中区/6 DIP间距及82 DIP WrapPanel同排/换行/无重叠均有实际 STA WPF几何和 `VisualTreeHelper.HitTest` 断言。未改生产代码。
- **双主题与门禁**：Release `0/0`、XAML24/24、双主题RenderHarness全量 `372 PNG / render-qa OK`；Media1040×700浅/深截图已核对，源码校验通过，WPF静态审查36 XAML/0 errors/29 warnings/177 info。截图为合成数据离屏布局，不代表真实OS输入或Playnite。
- **当前边界**：R02-05保持“已满足”，未操作真实复制/删除命令；package-host、真实鼠标/触控、物理DPI仍未验。run `38247748056` 失败日志不可读，不能归因该定向测试。下一项R02-06先检查菜单的插件/宿主所有权；外部宿主验收若仍受ENV-001阻塞，继续独立R02-07。

### R02-06 / R02-07：当前 main 菜单边界复核

- 当前 `main` `3df542af` 相对最近成功本机源码构建 `90099a09` 是 docs/evidence-only。`GetGameMenuItems`、`GameMenuActionContext` 与菜单测试自 `c55f7a9` 的 R02 menu `4/4` 证据以来没有源文件变化；该历史行为结果不冒称为本轮新运行。
- R02-06 的插件契约继续有空选择0项和合成多选有序6项、Action非空的既有测试；Playnite拥有菜单 visual tree 与键盘/翻转/关闭行为，ENV-001 的 WMI访问拒绝与CEF0x5前置未变，本轮不重试宿主。R02-06 保持“外部阻塞”；R02-07 插件可控身份快照保持“已满足”。边界、CI和本机SDK限制见[当前复核](design/reviews/ui-finesse-round3-20260915/evidence/R02-06-07-CURRENT-MAIN-REVIEW-20261012/README.md)。

### CLOSE-OPTICAL-01：复合按钮内容拉伸与固定图标列间距

- **发现**：CLOSE-WRAP全量首次跨过Q14后，第29类R02OpticalAlignmentTests 2 passed/2 failed，16/20 DIP图标两项严格8±0.5 DIP间距都实测9；其后85类未执行。独立旧clean c35 DLL也复现同两项，属于既有缺口，不归因本轮输入/行距修复。
- **根因与实现**：实际testhost中内容desired32/36 DIP却被共享presenter拉伸到134，图标固定列16/20变17/21；合成夹具只改变presenter为Center后列宽恢复16/20、间距8，原失败断言保留。共享按钮现在跟踪HasVisualContent并让真实UIElement/FrameworkContentElement遵循HorizontalContentAlignment，文本保持Stretch有限测量。联合候选原4项及新增3项动态对齐/文本-视觉内容往返7/7；连同Q14/控制器/跨页专项共64/64，完整门禁待验。
- **阶段终态**：R02 7/7、联合专项64/64、完整render/source/XAML通过；第三轮全量本类7/7，后续第37类删除回退失败另案，完整门禁仍未过。
- **完成条件**：原16/20尺寸8 DIP严格间距/中心与36 DIP高度通过，Left/Center/Right动态切换和文本有限宽度不回归；保留命令/绑定/忙碌/焦点/禁用与ellipsis，双主题全量WPF/render/source门禁通过。与CLOSE-WRAP分别记录，不放宽容差。
- **证据**：[共享UI阶段](ai/evidence/close-wrap-20261007/README.md)保留首次全量失败、旧DLL对照、STA干预及修复后TRX/源码和DLL身份；整体CI/发布未签收。
- **2026-10-12 当前 main 复验**：上方历史“间距9 DIP”对应修复前的失败身份；当前确切 main 用真实生产 ResourceDictionary 重建后，R02 optical类7/7，16/20 DIP实际间距8、中心差0；Release0/0、XAML24/24、双主题全量render-qa 372图通过。R02任务行为与视觉条件当前满足。整体队列仍 `IN_PROGRESS`；run932与后续run `38247748056` 的“编译与测试”exit1均无可读日志，不猜成此项回归。
- **2026-10-16 收口**：当前候选 Actions #1039 的 SDK8/SDK10.0.401 全量 WPF 隔离入口均成功，SDK8 `render-qa OK`。当前自动化条件已满足；真实 Playnite 和物理 DPI/最终呈现仍未验。

### CLOSE-FOCUS-01：ComboBox Popup离开焦点后的关闭路径

- **发现**：联合候选全量source470/18、前34类WPF含Q14/R02通过，第35类R05 2 passed/1 failed，后79类未执行。GamePickerStatusComboBox在MoveFocus(Next)后IsDropDownOpen仍true（147行），原生exit1；COM收尾文本不替代该实际断言。
- **当前对照**：旧clean c35 DLL独立3/3；同联合candidate DLL独立再复现2/1。新增日志保留原失败，证明260×180中筛选行Collapsed、Focus()=false；900×640可见且获焦后原断言通过。最终强制前置断言并补900×320紧凑负例4/4，生产Popup未改；第三轮全量本类4/4，后续第37类删除回退另案，完整门禁仍未过。
- **完成条件**：保留真实ComboBox/方向选择、Tab/Shift+Tab回表单/overlay边界、DropDownClosed提交语义与焦点恢复；稳定复现后根因修复及正负例，完整114类/source/render无回归。不能以直接关闭夹具Popup、放宽断言或skip代替修复。
- **证据**：[联合UI证据](ai/evidence/close-wrap-20261007/README.md)记录全量失败、clean旧DLL与同candidate独立结果及源码/DLL SHA/MVID。任务不等同真实Playnite输入/物理DPI验收。
- **2026-10-16 收口**：已确认全尺寸与紧凑可见性前提后，保留真实 ComboBox、方向选择、Tab 离焦关闭、DropDownClosed 提交和焦点回到 overlay 的行为/负例；当前 Actions #1039 双 SDK 完整 WPF `124/124` 类通过，SDK8 Render QA 通过。真实 Playnite 输入未验。

### CLOSE-PICKER-01：删除回退与防抖刷新时序

- **发现与确定性复现**：第三轮完整入口原在第37类删除回退期望 `Game 0000`、实际 `Game 0020`。旧三项独立测试没有实例化生产视图/没有线程归属断言，不能排除 Dispatcher 竞态。新增无 SynchronizationContext 的 STA WPF 负例在旧实现中确定复现：刷新线程不是 owner Dispatcher。
- **实现**：STA 创建的 picker 捕获所属 Dispatcher；缺少 SynchronizationContext 时以 DataBind 优先级回投刷新。有正常同步上下文仍沿用原路径。立即刷新、游戏状态刷新和 Dispose 会取消并释放 pending debounce；保留20ms合并、选择回退、过滤和虚拟化语义。
- **本机门禁**：SDK8.0.423 Release 0 warnings/0 errors、XAML24/24；Core125/125、Worker356 passed/1 environment skip、Playnite source461 passed/27 skipped、WPF 114 classes 608 passed/39 skipped，无失败；picker 类5/5，R09焦点类2/2。完整证据和各项 skip/身份见[本阶段证据](ai/evidence/close-picker-20261012/README.md)。`IMPLEMENTED` 仅表示本机代码与自动化门禁，不代表 CI、真实 Playnite 或发布通过。
- **后续边界**：推送后检查新 Actions run 的终态；随后 CLOSE-EVID-01 在修复提交后的 `main` 上重算 freshness，逐条补 R00/R01 stale 项。旧 run `36677346317` 无法读取；run `36675309812` 完整日志仍为403，不能猜测远端结果。Demo原目录不可用，不更换视觉体系。
- **非目标**：不盲增 timeout、放宽名称断言、skip 或改变真实选择语义；不触碰用户存档/媒体/云端，不冒称已验证真实宿主、物理 DPI、IME、UIA、ETW 或最终呈现帧。
### CLOSE-WRAP-01：任务预设单行残留折行边距

- **范围**：Q14ToolbarAlignmentBehaviorTests 在 Light/654 DIP 实测为单行，但七个控件 Margin.Bottom 仍20 DIP；原全量和同DLL独立复核均失败。定位 WrapPanel 测量、DPI边界、响应式更新和行距恢复时序，修复共享路径；不放宽断言或固定跳过654 DIP。
- **追加失败（2026-10-07）**：当前独立及全量Q14先在验证错误态TextBox高度36→37 DIP失败，未到654 DIP。需同时修复共享错误态测量与原单行边距；不能把未执行的后半段计为通过。
- **SDK阶段定位**：STA保持Validation.HasError=true，参与测量的边框2→1 DIP后高度37→36；生产尚未改。调用同DLL既有私有边距夹具，Light/Dark654 DIP七项单行margin20均失败，620/660通过。须共享模板及真实折行状态修复，不写死654或放宽容差。
- **实现与在途验证**：共享TextBox用独立2 DIP错误覆盖描边，内容测量不变；行距控制器按实际Arrange slot多行判定，单行恢复作者margin，并处理resize/内容/可见性/卸载重入。Release0/0、XAML24/24；Q14 1/1含60组双主题几何、控制器2/2、跨页54/54与完整render-qa通过。全量source470/18通过，WPF前28类（含Q14）通过，第29类R02两项失败，其后85类未执行；CLOSE-OPTICAL-01另行修复，任务保持IN_PROGRESS。[修复前后身份与证据](ai/evidence/close-wrap-20261007/README.md)。
- **阶段终态**：第三轮全量Q14与控制器通过，Core125/Worker357/source470/18及联合render通过；第37类删除回退失败另案，完整门禁仍未过，本阶段源码/证据单独提交。
- **完成条件**：保留620→660→620往返、边界宽度和Light/Dark真实STA布局；单行恢复作者margin，折行保持20 DIP；关联Task/Save/Media/Trainer共享行距无回归。补修复前后DLL身份/TRX/几何，并完成全量门禁。
- **依赖/证据**：CLOSE-EVID-01先完成此回归修复再更新baseline；[本轮失败与复核](ai/evidence/close-ipc-20261005/README.md)。这是新发现的受控失败，不替代用户Q06/R08或真实Playnite故障。

### CLOSE-HOST-01：Media Inbox 真实滚动诊断

- **范围**：复用 ENV-001 runner 的 fail-closed 预检，隔离合成游戏/媒体；同 PID/DLL/MVID 下记录主题、DIP/DPI、列头/Presenter/首行/外层偏移的滚动前中后值。
- **完成条件**：复现或明确排除具体条件；若复现，按几何差异定位并补修复前后证据；表尾可达、选择/锚点/虚拟化/窄窗和双主题无回归。恢复环境本身不代表本任务完成。
- **阻塞**：历史 WMI/CEF/单实例隔离边界尚无解除证据。条件变化才重试预检；不反复跑离屏替代宿主。

### CLOSE-REG-01：用户 Q06/R08 原失败归因

- **范围**：取得失败方法、断言/堆栈、TRX、源码/运行 DLL 身份和调用入口；与同身份复跑比对，区分用户故障、夹具和 COM 收尾噪声。
- **完成条件**：明确根因与回归证据，或经原失败环境验证可关闭；本机未复现不能关闭。与本轮新 IPC 失败分开处理。

### CLOSE-REL-01：同一候选身份的发布矩阵

- **范围**：复用 WINDOWS_TEST_PLAN、现有 package/install/rollback、故障注入与 soak。分小批：隔离备份→校验→PreRestore→恢复→失败回滚→Undo；Worker 重启/取消/断连；独立测试远端上传/失败重试；媒体/工具；宿主主题/DPI/键盘；安装升级/回退。
- **完成条件**：每个场景记录 candidate commit、包 hash、安装/加载 DLL 与工具版本、输入、预期、结果和残留；零未解释 P0/P1；所有 skip/硬件/权限缺口列清。物理跨屏、读屏和 presented frame 分账，不由 STA 代替。
- **阻塞/边界**：ENV-001、当前绿色 CI、可丢弃夹具和独立远端未齐；远端凭据不进仓库，真实用户存档不作为夹具。正式公开发布单独处理。

### CLOSE-UX-01：有限的说明密度与空态核对（候选）

- **范围**：只记录当前可见且有复现的重复说明/不可达操作；优先复用现有数据态/Tooltip/HelpText，保留风险和取消语义。
- **准入/完成条件**：每条有页面、状态、尺寸、实际问题及前后验证；无新缺陷则不领取，不继续为凑数量增加 UI 微调。

### CLOSE-ARCH-01：按维护痛点提取职责（候选）

- **范围**：先度量 DashboardViewModel 现有 16 个 partial 的耦合和实际改动成本；只提取影响当前任务的职责或测试接口。
- **准入/完成条件**：具体调用/测试痛点、有限依赖边界、行为回归证明；不因 6480 行主文件就全量重写，也不把大文件等同性能缺陷。

## 历史任务记录（状态字段优先，不按章节名领取）

### GOV-002：校准积压清单与交付证据状态

- **优先级**：P1
- **状态**：IMPLEMENTED
- **发现日期**：2026-08-01
- **根因与影响**：本轮只读审计发现积压清单仍将 DOC-001 标记为 `PROPOSED`，但 `792186f` 已完成该文档一致性修正；部分 UI 条目的自动化测试数量也停留在早期计数。状态和证据漂移会使无人值守代理错误判断可领取任务、重复实现已完成工作，或把源码自动化误当作真机验证。
- **建议范围**：逐项核对积压条目与提交、`PROJECT_MEMORY.md`、`DEVELOPMENT_PROGRESS.md`、`KNOWN_ISSUES.md` 和 `WINDOWS_TEST_PLAN.md`；仅校准任务状态、当前证据、依赖和阻塞条件，保留可追溯的历史证据；明确 `ENV-001` 是真实 Playnite 回归的唯一环境前置。
- **非目标**：不修改插件、Worker、XAML、版本、安装包、用户数据或 Git 历史；不因文档校准而解除 `BLOCKED_ENVIRONMENT`，也不把自动化结果写成真机完成。
- **验收标准**：每个条目均有唯一且当前的状态；已实现项目不再保持 `PROPOSED`；测试计数与最新可复查结果一致；所有真实 Playnite/DPI/主题/云端/恢复缺口仍明确指向隔离环境或相应测试计划。

### GOV-001：补齐无人值守开发治理文档

- **优先级**：P0
- **状态**：IMPLEMENTED
- **发现日期**：2026-07-31
- **根因与影响**：仓库缺少 `docs/AUTONOMOUS_DEVELOPMENT_RULES.md`、`docs/QUALITY_GATES.md`，且此前没有此积压清单。虽然现有 `AGENTS.md`、设计门禁和 Windows 测试计划包含部分约束，但代理无法从单一来源确定 READY 工作项、状态流转与最低验收证据，容易导致无人值守运行越过范围或重复领取工作。
- **建议范围**：建立两份缺失的治理文档；将任务状态、领取规则、最小验证集、环境阻塞记录格式和提交要求集中化；引用既有 `AGENTS.md`、`WINDOWS_TEST_PLAN.md` 与 UI 门禁，避免复制并漂移安全约束；把当前仍需真实 Windows/Playnite 验证的事项按优先级拆分为后续候选任务。
- **非目标**：不修改插件业务行为、WPF/XAML、版本、安装脚本或发布流程；不把需要真实设备、Rclone 或 Playnite 证据的事项标记为已验证。
- **验收标准**：三份治理文档均存在且互相链接；每个可领取条目有唯一 ID、优先级、状态、范围、验收证据和阻塞条件；至少一个经过审查的非破坏性工作项可标记为 `READY`。

### UI-001：WPF-UI 4.3.0 兼容性 POC 与迁移基线

- **优先级**：P0
- **状态**：BLOCKED_ENVIRONMENT
- **前置条件**：GOV-001 已完成；当前工作树干净；不得修改 Playnite 宿主 Chrome、插件 ID、业务命令或 Worker。
- **根因与影响**：现有 Apple-inspired WPF 样式虽已覆盖基础主题与控件，但尚未验证用户指定的 WPF-UI 4.3.0 是否能在 net462 / Playnite 10 宿主中进行资源隔离且不污染宿主。未经 POC 直接全量替换会有资源键冲突、程序集加载和 XAML 解析崩溃风险。
- **范围**：核验目标框架、WPF-UI 许可/依赖与 Playnite 兼容性；以作用域资源字典完成最小可编译 POC；保留现有 DesignTokens、动态主题、虚拟化与命令绑定；明确回退策略。只在满足门禁后创建后续迁移任务。
- **非目标**：不替换业务层；不更改版本或安装流程；不改变真实存档、数据库、云端或用户配置；不声明完成全量 UI 迁移。
- **验收标准**：Release 构建与全量单元测试通过；`validate-source.py`、XAML/UI 静态检查、`git diff --check` 和 `git fsck --full` 通过；Playnite 加载 POC 后不存在新增资源/绑定/跨线程异常；WPF-UI 资源不能全局污染宿主。
- **阻塞条件**：WPF-UI 或其依赖不兼容 net462/Playnite，或 POC 无法保证资源隔离时，必须记录 `BLOCKED_ENVIRONMENT` 并继续使用现有共享令牌方案。
- **当前证据**：2026-08-01 已确认 NuGet 的 WPF-UI 4.3.0 含 net462 程序集；插件以局部 `UserControl.Resources` 合并主题与控件字典，Release 构建、50 个单元测试、源码/XAML 门禁及打包通过，PEXT 含 Wpf.Ui、Abstractions 与 net462 运行依赖。Dashboard 不在 XAML 解析时创建探针；维护中心的显式加载入口会在构造/资源解析失败时保留页面与重试入口。现有 Playnite/Worker 是用户正在使用的实例；未获隔离实例授权前不得关闭进程或覆盖其插件目录，故实际加载、Dialog/Snackbar 交互、主题/DPI 与宿主污染验证尚未执行。

### ENV-001：可审计的隔离 Playnite 真机测试环境

- **优先级**：P0
- **状态**：BLOCKED_ENVIRONMENT
- **前置条件**：不得停止、关闭或覆盖用户正在使用的 Playnite/Worker；不得把 `.tmp/` 或测试库纳入 Git。
- **根因与影响**：当前工作区的 `.tmp/playnite-ui-test/Playnite` 仅含 `DatabasePath: library` 和独立扩展文件，尚不能以可审计证据证明 Playnite 会使用独立配置、扩展数据根和单独进程，而非复用用户 `%AppData%\\Playnite` 或向现有单实例转发。启动它可能影响用户实例，因而不能作为 UI-001 真机验收环境。
- **范围**：只建立并验证一个独立 Playnite 数据根、空测试库、独立扩展目录和可记录的启动 PID 边界；证明不读取/写入用户 AppData、现有库或现有扩展目录后，才允许执行后续 UI/功能 smoke test。
- **非目标**：不启动现有用户 Playnite；不删除用户数据；不安装到用户扩展目录；不执行备份、恢复、云端镜像或媒体删除。
- **验收标准**：启动方式与官方 portable/独立数据机制有可复查依据；启动前后均能记录测试 PID 与数据根；用户实例 PID/路径未变；测试扩展与 Worker（如启动）均从测试目录加载；测试后日志能证明无用户 AppData/库写入。
- **阻塞条件**：无法证明 Playnite 数据隔离或无法获得安全的独立启动方式时，保持 `BLOCKED_ENVIRONMENT`，不得以 `--hidesplashscreen`、复制安装目录或强制结束进程代替证据。
- **阶段结果（2026-09-26）**：隔离 runner 已补 fail-closed 守卫：要求显式 `.tmp` profile 与 `Playnite.DesktopApp.exe`；核对没有 Playnite/Worker 冲突进程；启动前验证进程命令行可观察；标记并拒绝非空未标记 profile；递归拒绝 reparse point；数据库路径限制在 profile；拒绝覆盖已有 audit output；隔离安装分支跳过停止用户 Playnite/Worker；脚本不读取用户 AppData 主题/日志路径。相应 PowerShell 与源码契约测试已加入。Release solution `--no-restore` build `0/0`、XAML `24/24`、Core `125/125`、Worker `357/357`；Playnite 源码测试 `465 passed/18 skipped`，105 个 WPF 测试类分进程通过。R 表仍是 192 项，未新增、重分类或签收任何 R ID。
- **2026-09-26 启动身份稳定性复核**：`--userdatadir` 必须唯一、带引号且规范化完整路径相等；首次快照后 500 ms 再确认同一 PID/exe/命令行仍存活，并拒绝快照前后退出。兄弟前缀、重复参数及快照后退出负例通过；Release solution `--no-restore`、Core `125/125`、Worker `357/357`、`DiagnosticsEvidenceSourceTests 8/8`、helper 通过。Playnite source-only group 111 类通过；本阶段未完成 WPF 全量回归。见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/ENV-001-PROCESS-START-IDENTITY-20260926.md`。TOCTOU 与 WMI/CEF 阻塞仍在，ENV-001 不改状态。
- **2026-09-26 profile marker 复核**：marker schema 升至 `2` 并绑定规范化 `ProfilePath`，拒绝复制来的跨目录 marker 与旧 schema，不迁移/覆盖既有数据；PowerShell helper tests 通过，Release `DiagnosticsEvidenceSourceTests 8/8`。证据：`docs/design/reviews/ui-finesse-round3-20260915/evidence/ENV-001-PROFILE-MARKER-PATH-BINDING-20260926.md`。该修复不解除 WMI/CEF 环境阻塞。
- **当前阻塞**：本机已发现有效签名的 `D:\software\Playnite\Playnite\Playnite.DesktopApp.exe`，但当前工具身份对 `Win32_Process` 命令行查询返回“拒绝访问”；runner 现会在创建目录/profile 或启动宿主之前拒绝。主机记录还明确证明同一 Playnite 在 2026-09-24 的 fresh isolated bootstrap 遇到 CEF `platform_channel 0x5`，相同系统/权限状态不重复启动；未下载、安装或启动 Playnite。2026-09-26 复核的[官方命令行文档](https://api.playnite.link/docs/manual/advanced/cmdlineArguments.html)只说明 `--userdatadir` 重定向数据目录；未列出独立并行实例参数，而 `--shutdown` 会关闭已有实例，不作为隔离方案。故实际宿主进程、AppData/库写入边界、扩展加载、UIA 与页面验收尚未完成，维持 `BLOCKED_ENVIRONMENT`。证据见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/ENV-001-ISOLATED-RUNNER-20260926.md`。

### DOC-001：完成度评估版本与证据一致性复核

- **优先级**：P2
- **状态**：IMPLEMENTED
- **前置条件**：不得提升程序集、插件清单或正式版本；必须以当前提交中的可复查证据更新评估。
- **根因与影响**：`docs/FEATURE_COMPLETION_ASSESSMENT.md` 仍标记 `0.6.20-development-preview`，而当前程序集、插件清单、`DEVELOPMENT_PROGRESS.md` 和 `PROJECT_MEMORY.md` 已记录 `0.6.22`。版本漂移会使完成度、真机验证范围和交接判断失真。
- **范围**：核对版本来源、当前源码覆盖和真实 Windows/Playnite 证据；只更新评估文档的版本、日期、百分比说明和未验证边界，不能把静态检查写成真机结论。
- **非目标**：不修改产品版本、程序集、`extension.yaml`、发布流程或功能实现。
- **验收标准**：评估版本与现有清单/程序集一致；每个百分比有可追踪来源；明确区分 0.6.22 已通过的自动化测试与仍被隔离环境阻塞的 UI/真实云端/恢复回归。

### UI-002：共享设计令牌与控件模板全量复审

- **优先级**：P0
- **状态**：IMPLEMENTED
- **前置条件**：UI-001 结论已记录；若 POC 不兼容，则使用现有资源体系的等价实现。
- **范围**：统一 Button、TextBox、数值输入、ComboBox、CheckBox、ScrollBar、TabControl、DataGrid、ListBox、Popup、Tooltip、Toast 和焦点环；补齐浅/深/跟随 Playnite、高对比度、关闭透明和关闭动画的安全降级。
- **验收标准**：所有共享控件 Normal/Hover/Pressed/Disabled/Focus 状态可用；大列表保持 Recycling 虚拟化；不出现页面级硬编码主题色、默认宿主白色控件、冻结 Transform 动画或大型区域 BlurEffect。
- **当前证据**：2026-08-01 已将设置卡片收敛至共享 `GscSurface`；共享令牌覆盖主/普通按钮、数值/文本输入、ComboBox、CheckBox、Slider、滚动条、Tooltip、ProgressBar、焦点环和进度不确定状态。新增 `validate-source.py` 共享控件门禁并实际执行。Release 构建为 0 警告/0 错误，Core 13、Worker 21、Playnite 16 项测试通过；打包产生 0.6.22 ZIP/PEXT，Worker smoke 为 0.6.22.0。UI Skill 静态审查为 0 errors（项目现有的布局提示和未跟踪 `.tmp/` 中的宿主主题提示未隐藏）。

### UI-003：Dashboard 与 Settings 自适应布局、可访问性及真机视觉回归

- **优先级**：P0
- **状态**：BLOCKED_ENVIRONMENT
- **前置条件**：UI-002 已实现并通过自动化门禁。
- **范围**：修复所有工作区的裁切、长文本 Tooltip、窄窗口重排、数值输入完整编辑、表格/页签/弹层对齐与真实状态反馈；在隔离目录验证 100%–200% DPI、1600×900 至 980×640、键盘导航、主题及减少动画。
- **验收标准**：真实 Playnite 加载、页面打开关闭与缩放均无 XAML/绑定/Dispatcher 异常；所有真机未覆盖条件在 Windows 测试计划中明确记录，不伪造完成。
- **当前证据与阻塞**：2026-08-01 已完成源代码层修复：侧栏导航获得有限高度内的键盘可达滚动；紧凑模式把顶部操作压缩为带 Automation Name/Tooltip 的图标；Settings 加入横向访问通道、窄屏边距/副标题降级、滑块 Automation Name；环境光与焦点环不再用负 Margin。源码门禁、Release 构建（0 警告/错误）、50 项测试与 UI Skill 审查（0 errors）通过。任务仍要求真实 Playnite 的加载/主题/DPI/键盘验证；`.tmp/playnite-ui-test` 不能证明独立 AppData/单实例边界，且用户实例正在运行，故不得启动它或覆盖用户扩展。需先完成 ENV-001。

### UI-004：WPF-UI 成熟控件的生产迁移与宿主回归

- **优先级**：P0
- **状态**：BLOCKED_ENVIRONMENT
- **前置条件**：用户已明确授权继续源码层生产迁移；真实 Playnite 验收仍以 ENV-001 证明独立数据根和进程边界为前置。必须保留插件 ID、业务命令、绑定、虚拟化和共享主题令牌。
- **根因与影响**：基线中的 WPF-UI 4.3.0 仅以惰性“界面探针”和少量顶部控件验证程序集、资源隔离与异常恢复，生产页面仍有大量重复模板。源码迁移现已完成，但未经 Windows 编译与宿主验证仍可能存在 API/XAML 兼容、资源键冲突或 Playnite 默认主题泄漏，因此不能宣称真机完成。
- **范围**：将适用的输入、下拉、信息卡、动作按钮、对话框和通知迁移到 WPF-UI 的局部资源体系；主导航/Tab、DataGrid、游戏列表、数值校验和性能敏感虚拟化区经审查保留现有 WPF 模板，避免改变信息架构、列行为、键盘路径或 Recycling。每类控件保留原有命令、主题、错误/取消状态与宿主回退。
- **非目标**：不注入 Playnite 全局资源；不修改窗口 Chrome；不以探针或截图替代生产交互；不删除现有安全错误处理、数据或备份。
- **验收标准**：真实隔离 Playnite 中 Dashboard、Settings、Dialog/Snackbar 正常加载；Light/Dark/Follow/高对比度、关闭透明/动画、100%–200% DPI 和 1600×900–980×640 无新增资源、绑定或 Dispatcher 异常；大库 ListBox/DataGrid 保持 Recycling；框架控件创建失败时页面可恢复且不污染宿主。
- **阻塞条件**：无法证明 WPF-UI 局部资源在 Playnite 10/net462 中稳定、可回退且无宿主污染时，保持自定义共享令牌方案并改为 `BLOCKED_ENVIRONMENT`，不得强行替换生产控件。
- **当前证据**：2026-08-01 已完成可在源码层安全落地的生产迁移：新增视图局部 `WpfUiProduction.xaml` 适配层；Dashboard 的 6 个指标卡、59 个生产动作按钮、14 个策略/工具/媒体开关、10 个普通文本输入和 13 个下拉选择，以及 Settings 的 5 个设置卡、2 个动作按钮、14 个开关、6 个路径输入和 3 个下拉选择均使用 WPF-UI 模板。数值校验输入、高密度 DataGrid/ListBox、搜索清除按钮和本地兜底浮层继续保留原生 WPF；59 个 Command、全部 Binding、Recycling 虚拟化和业务事件数量与基线一致。Dashboard/Settings 的 Snackbar 使用框架局部适配；确认和设置导入报告分别使用插件内 Dialog 与 MessageBox，因为真实日志证明 WPF-UI `ContentDialogHost` 是 Playnite Window 级单例，不能放入嵌入式页面。文件导入导出的磁盘读取/写入移出 UI 线程，未引入新的 `async void`。针对真实 `Wpf.Ui.Controls.Button`、`GscSoftShadowColor` 和 `ContentDialogHost.RegisterHost` 崩溃，已分别增加资源作用域/主题令牌/窗口 Host 回归门禁。Windows Release build 为 0 警告/错误，Core 13、Worker 21、Playnite 19 项测试通过；真实 Playnite、DPI、主题、键盘及宿主污染验证仍被 ENV-001 阻塞。

### UI-005：全功能页面视觉与响应式信息架构重构

- **优先级**：P0
- **状态**：BLOCKED_ENVIRONMENT
- **发现日期**：2026-08-02
- **前置条件**：UI-004 的局部 WPF-UI 资源隔离和崩溃门禁保留；不得删除现有 Command、Binding、业务事件、虚拟化、恢复安全确认、用户数据或插件 ID；真实 Playnite 验收仍以 ENV-001 为前置。
- **根因与影响**：此前 UI-002～UI-004 主要完成共享样式、框架控件迁移和宿主安全收口，页面信息架构与旧版接近，用户无法直观看到“重新设计”。早期 HTML 原型还暴露 Standard 标题/操作重叠、Compact 图标与状态卡越界等问题，若直接翻译为 XAML 会把原型缺陷带入生产页面。
- **范围**：在现有 net462 WPF/Playnite 页面内重构 Dashboard 六个工作区和 Settings；建立局部 `Redesign.xaml`，统一玻璃表面、圆角、指标卡、全局游戏上下文和设置分类；使用 1280/980/880 DIP 四档布局，明确标题、操作、游戏切换器、游戏库、详情区和紧凑侧栏的独立测量槽；保留现有 DataGrid/ListBox Recycling、命令、绑定、错误/空状态和安全回退。
- **非目标**：不把 HTML、WebView、Electron、WinUI、Avalonia 或浏览器壳引入插件；不伪造真正 Acrylic/Mica；不修改备份、恢复、云端、媒体、修改器、SQLite 或进程业务逻辑；不以截图或静态检查替代 Windows 构建和 Playnite 真机回归。
- **源码实现证据**：新增页面局部 `Themes/Redesign.xaml`；Dashboard 增加全局游戏切换器、可显式展开的完整游戏库、重构首页/存档历史/任务/维护摘要并统一修改器与媒体信息层；Settings 改为分类导航与单分类内容区。Compact/Narrow 侧栏使用 48×48 导航、48×50 Worker/Ludusavi 状态卡、裁切边界及模板级居中；Standard/Compact/Narrow 将标题、游戏选择和操作区放入独立 Grid 行。与 `HEAD` 比对，Dashboard 旧有 56 个唯一 Command 无删除，236 个唯一 Binding 无删除；Settings 25 个唯一 Binding 无删除，业务事件无删除。
- **当前验证**：`scripts/validate-source.py`、三份 XAML XML 解析、UI Skill 静态审查（0 errors）、`git diff --check` 通过；新增 4 项 UI 合约测试，当前测试源码估算 Core 13 + Worker 21 + Playnite 53 = 87 项，但本 Linux 环境没有 dotnet/MSBuild/Playnite，新增测试和 Release build 尚未执行。
- **阻塞与最终验收**：必须在 Windows 运行 restore/build/87 项测试/package/Worker smoke，并在可审计隔离 Playnite 中逐页验证 1600×900、1366×768、1280×720、1100×700、980×640、880 DIP 附近和 100%/125%/150%/200% DPI；检查 Light/Dark/Follow/高对比度、关闭玻璃/动画、键盘、Popup、侧栏居中、状态卡边界、游戏切换和页面反复开关。完成前不得宣称零崩溃或正式完成。

## 本次审计证据

- 2026-08-01：GOV-002 完成台账校准。`792186f` 已将 `FEATURE_COMPLETION_ASSESSMENT.md` 校准至 `0.6.22-development-preview`，故 DOC-001 从 `PROPOSED` 改为 `IMPLEMENTED`；当前可复查自动化基线为源码门禁通过、Release 0 warning/0 error、Core 13 + Worker 21 + Playnite UI 49 = 83 项测试通过、UI Skill 静态审查 0 errors、`git diff --check`/`git fsck --full`、PEXT 打包和 Worker `0.6.22.0` smoke 通过。历史条目中的较低测试数保留为当时提交的证据，不能误写成当前基线。`ENV-001` 仍是实际 Playnite 加载、多主题、DPI、键盘和滚动验收的唯一环境前置；没有启动 `.tmp` 或用户实例。

- 2026-08-01：本次只读审计确认用户进程仍为 `D:\\software\\Playnite\\Playnite\\Playnite.DesktopApp.exe`（PID 28708）及用户 AppData 扩展下的 Worker（PID 31444）。工作区 `.tmp/playnite-ui-test/Playnite` 仅声明 `DatabasePath: library`，未提供可审计的 portable/独立配置根证明；其扩展目录已含本次测试插件。已登记 ENV-001，未启动该副本，`.tmp/` 保持未跟踪。
- 2026-08-01：为验证 GSC-083/GSC-084，向 `.tmp/playnite-ui-test/Playnite/Extensions/66e9f2d7-67bb-43ef-b62a-b8e60734fcec` 复制了本次已打包的测试文件；启动副本后，桌面自动化只识别到 `D:\\software\\Playnite\\Playnite\\Playnite.DesktopApp.exe` 的“数据备份错误”窗口和该路径的 PID。未点击、关闭、终止或写入该用户实例，未操作其 Worker、备份、恢复或云端。此现象进一步证明复制目录不能建立 ENV-001 的单实例/数据根边界；真机验收继续保持 `BLOCKED_ENVIRONMENT`，`.tmp/` 不纳入 Git。
- 2026-08-01：继续使用官方 `--userdatadir D:\\workplace\\Github\\GameSaveCenter\\.tmp\\playnite-isolated-data` 及唯一启动 PID 测试隔离；该数据根只创建了 `Extensions`、`JITProfiles` 与 `playnite.log`，日志原样记录 `Application already running, shutting down.`，测试进程立即退出。没有获得独立可交互窗口，未触碰用户 AppData、用户 Worker、备份、恢复或云端。现有 Playnite 的全局单实例行为仍阻塞 ENV-001，后续不得以再次启动或结束用户实例绕过。
- 2026-08-01：只读审计确认 `docs/FEATURE_COMPLETION_ASSESSMENT.md` 仍为 `0.6.20-development-preview`，与当前 0.6.22 交接文档不一致；已登记 DOC-001，不在没有 READY 条目的本次运行中直接改写评估。

- 2026-08-01：UI-002 完成共享 WPF 控件和主题令牌复审。使用 Codex 附带 Python 运行 `scripts/validate-source.py` 退出码 0；UI Skill 审查退出码 0（0 errors、32 warnings、111 info，其中 `.tmp/` 的复制宿主占 17 条警告，其余为既有 Dashboard/环境光布局审查项）；`dotnet restore`、Release build（0 警告/错误）和 50 项测试通过。`scripts/package.ps1 -Configuration Release -SkipBuild` 生成 0.6.22 PEXT/ZIP，`scripts/verify.ps1 -Worker <打包 Worker>` 报告 0.6.22.0；`git diff --check`、`git fsck --full` 均为退出码 0，后者仅报告既有 dangling 对象。未启动 `.tmp/` 副本或用户 Playnite，隔离视觉回归移交 UI-003/ENV-001。

- 2026-08-01：UI-003 已完成可自动验证的 Dashboard/Settings 响应式与可访问性收口：项目自身 UI Skill 告警从 15 降至 11（总数从 32 降至 28；17 条仍来自未跟踪 `.tmp/` 的 Playnite 复制主题）。剩余项目提示均需真机在有限 `Grid` 行中复查，并不等同于已经确认的布局缺陷。源码门禁、Release build 和 50 项测试均通过；实际 Playnite 验收被 ENV-001 阻塞。

- 2026-08-01：无 READY 项时的只读审计确认，现有 WPF-UI 仅为惰性 POC，尚未迁移生产控件；已登记 UI-004。由于 UI-001/UI-003/ENV-001 尚无独立 Playnite 证据，本次没有修改 XAML、程序集、安装或用户数据。

- 2026-08-01：GOV-001 已补齐 `AUTONOMOUS_DEVELOPMENT_RULES.md` 与 `QUALITY_GATES.md`，并建立了 UI-001/002/003 的依赖、范围、非目标、验收条件和阻塞规则。UI-001 曾按依赖顺序登记为 `READY`，现因真机隔离证据不足转为 `BLOCKED_ENVIRONMENT`；不会推送或合并 `origin/main`。
- 2026-08-01：使用 Codex 附带解释器 `C:\\Users\\lopmatuse\\.cache\\codex-runtimes\\codex-primary-runtime\\dependencies\\python\\python.exe` 执行 `scripts/validate-source.py`，退出码为 `1`，原样输出为：`XAML GameSaveCenter resource missing: ...GameSaveCenterSettingsView.xaml -> GscButtonBase`、`...DesignTokens.xaml -> GscErrorTintBrush`、`Numeric input must commit complete values on LostFocus: DashboardView.xaml DuringPlayIntervalMinutes`。这是 GOV-001 发现的 UI-001 基线缺陷，未在治理任务中修改代码。`git diff --check` 通过；`git fsck --full` 仅报告既有 dangling tree，不是对象损坏。

- 2026-07-31：仓库根目录为 Git 工作树，启动时 `main` 无未提交修改；`rg --files` 未找到 GOV-001 所列三份治理文档。
- 2026-07-31：`scripts/build.ps1 -Configuration Release` 通过：Release 构建 0 警告/0 错误，Core 13、Worker 20、Playnite 设置迁移 11 项测试均通过。
- 2026-07-31：`scripts/package.ps1 -Configuration Release -SkipBuild` 成功生成 `0.6.21` ZIP 与 PEXT；`scripts/verify.ps1` 确认打包 Worker 文件版本为 `0.6.21.0`。该 smoke 检查未启动、停止或替换已安装的 Playnite/Worker。
- 2026-07-31：`python scripts/validate-source.py` 未能运行，退出码 9009；PATH 中的 `python.exe` 是 Microsoft Store 占位符，系统未安装 Python 解释器。此项必须在配置 Python 3 后重跑，不能以 XAML 结构检查替代。
