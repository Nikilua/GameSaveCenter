# GameSaveCenter 当前交接

## 2026-10-21 当前进展：滚动视口修正、S06-08 完成

用户截图指出存档历史、路径校验和任务队列表格内部滚动条进入下方详情区域，部分数据被遮挡。此前 `43b869d1` 收拢紧凑工具区并移出表格框架内的详情入口；后续 `a1a86e41` 修复页面高度输入，已测量的页面 ScrollViewer 视口优先限制表格空间。SDK9.0.302 Release solution `0 warning / 0 error`；页面/表格几何与清单入口测试 `23/23`。证据：[滚动视口修复](docs/design/reviews/UI-SCROLL-VIEWPORT-REPAIR-20261021/README.md)。测试为离屏 WPF，未在真实 Playnite 或物理 DPI 下验屏。

S06-06 `fb33cd11` 按稳定 ID 刷新清单当前对象和父游戏摘要，只保存在瞬时 UI 投影中；Worker `3/3`。完整证据：[S06-06](docs/design/reviews/ui-finesse-round4-20261018/evidence/S06-06/README.md)。源文件 ACL/物理可读性仍不从 Worker 索引状态推断。S06-07 `96a3eab3` 提供脱敏只读清单预览和显式复制；SDK9 Release solution `0/0`，定向测试 `13/13`。证据：[S06-07](docs/design/reviews/ui-finesse-round4-20261018/evidence/S06-07/README.md)。S06-08 `09ec3c84` 补原生清单 JSON 跨会话与损坏字段恢复回归；Playnite.Tests Rebuild `0/0`、设置/清单定向测试 `27/27`。证据：[S06-08](docs/design/reviews/ui-finesse-round4-20261018/evidence/S06-08/README.md)。SDK8.0.423 未安装，真实 Playnite 启停及宿主磁盘原子性未验证；原 Round3 未跟踪 TRX/TestResults 保留。S07-03 仍受 required-check 读取权限阻塞，计划内目前无其他 READY 项。

## 2026-10-21 Round4 S05-08 情景性能验证已验收

提交 `313fabc18b9a7f3b627da748cc4dd6c3bd6f0cda` 为维护中心保留策略页增加有界命名情景设置；支持选择载入、保存/更新、复制、重命名、删除和最近成功快照时间。它与生效策略分离；结果与清理预览句柄不缓存。旧设置缺字段仍可读取，便携设置包含命名情景。精确提交 SDK `8.0.423` Release solution `0/0`、XAML `25/25`、定向测试 `13/13`、双主题 RenderHarness `0 PROBLEM`、source validation/diff check 通过。完整证据：[S05-06](docs/design/reviews/ui-finesse-round4-20261018/evidence/S05-06/README.md)。真实 Playnite/完整 WPF 隔离/物理 DPI/CEF 未验；保留旧 Round3 TRX/TestResults。

提交 `dc696fb0` 加入维护页情景比较的取消入口、10,000 版本硬上限和结束前版本/有效策略重验；超限明确拒绝，过期返回空结果并提示重算，不触碰清理 Apply。SDK9 Worker 29/29、维护页 Light/Dark 2/2、Release solution 0 error。详情、TRX 和构建边界：[S05-08](docs/design/reviews/ui-finesse-round4-20261018/evidence/S05-08/README.md)。本机没有 SDK8.0.423；真实 Playnite/完整 WPF 隔离/物理 DPI/CEF 未验。

## 历史：2026-10-21 Round4 S06-01/S06-02 已验收；S06-03 当时准入

`40a82b15` 完成 S06-01 有界清单契约；`85400645` 为存档版本、媒体、任务和校验问题详情接入确认后保存稳定引用的待办入口。Release solution `0 warning/0 error`，Playnite `8/8`、Worker `2/2`，source validation 通过，WPF 静态审查 `0 errors`。完整证据：[S06-02](docs/design/reviews/ui-finesse-round4-20261018/evidence/S06-02/README.md)。真实 Playnite 未运行；现有 CEF/权限边界未重试或绕过。

当时 S06-03 审计确认最近访问、筛选预设、校验 triage 和各页面队列不是异构人工清单；该项后续已由提交 `0fc43773` 完成，当前验收与下一项见本交接顶部。

## 历史：2026-10-21 Round4 S05-03 至 S05-05 已验收

S05-03 `e6279b7d` 将历史逻辑增长作为样本观察范围/未知，分开用户假设、本地卷与未知远端容量，不推算耗尽日期。S05-04 `cf50643c` 在维护中心当前可见的存储卡分别展示本地存档、镜像、恢复 staging、媒体目录的文件长度/文件数和可用 SQLite 逻辑量；无合计，空目录为 0、不可用为未知、重复路径提示勿相加。S05-05 `2fd0aa04` 将当前/候选策略总量比较下钻到逐游戏和变化版本，展示锁定、PreRestore、健康恢复点保护；只读确认符合归档路径安全条件的文件，不生成清理句柄。SDK `8.0.423` 精确提交 Release solution `0/0`，Worker `2/2`、生产维护页 Light/Dark 双层滚动行为 `2/2`、RenderHarness 无问题。证据：[S05-03](docs/design/reviews/ui-finesse-round4-20261018/evidence/S05-03/README.md)、[S05-04](docs/design/reviews/ui-finesse-round4-20261018/evidence/S05-04/README.md)、[S05-05](docs/design/reviews/ui-finesse-round4-20261018/evidence/S05-05/README.md)。真实 Playnite、完整 WPF 隔离循环、物理 DPI/卷簇实占与宿主滚动未验；保留用户的 Round3 TRX/TestResults。随后 S05-06 已验收，当前下一步审计 S05-07。

## 2026-10-20 用户反馈：页面滚动区域回归修复

存档历史、路径候选与任务队列的整页根容器不再用旧 `ActualHeight` 固定高度；表格和详情继续使用有限内部滚动，额外页面内容由 PageHost 外层承载。SDK8 Release solution `0/0`，几何回归 `5/5`，资源/源码契约 `4 pass / 1 historical skip`。证据：[滚动修复](docs/design/reviews/UI-SCROLL-LAYOUT-REPAIR-20261020/README.md)。验证为离屏 STA/WPF，不代表真实 Playnite 或物理 DPI。后续已完成 S05-03 至 S05-05。

## 2026-10-20 Round4 S05-02 已受控验收

提交 `5f1431f2` 为维护中心保留策略页增加当前逐游戏策略与统一候选策略的只读对比。审计确认既有全局清理模拟器只有当前策略，存储趋势没有候选规则侧。版本和当前策略经一条 SQLite 查询读取，两边使用同一快照与 UTC 截止时间，复用 `RetentionPlanner`；增长范围标为用户假设，不参与保留计数，对比没有清理预览或 Apply 授权。SDK `8.0.423` 精确提交 Release 一键隔离 no-start 构建/Worker 发布/安装 `0/0`，Worker 对账 `14/14`、生产 WPF 双主题行为 `2/2`、RenderHarness `0 PROBLEM`。完整证据见 [S05-02](docs/design/reviews/ui-finesse-round4-20261018/evidence/S05-02/README.md)。实际 Playnite 页面、物理 DPI、完整 WPF 隔离循环未验。之后 S05-03 与 S05-04 已验收；旧 Round3 未跟踪 TRX 和 TestResults 保留。

## 2026-10-20 一键构建、安装、启动默认流程已验证通过

此前 `DEV-INSTALL-008` 在只有 SDK `9.0.302` 的机器上因仓库精确锁定 SDK `8.0.423` 而失败于编译前。当前 `DEV-INSTALL-009` 会在关闭 Playnite 前，通过微软官方安装脚本准备缺少的准确 SDK（新机器首次执行需能访问微软下载源）。

本机从默认入口 `GameSaveCenter-Run.cmd` 跑完默认全链，退出码 `0`。SDK `8.0.423`，Release solution `0 warning / 0 error`；Core `157/157`、Worker `383/383`、Playnite `1812` 项中 `1754` 通过、`58` 跳过、0 失败/错误/超时/中断；WPF 隔离 `137/137` 类完成。R09 `2/2`、R10 `5/5`。自包含 Worker 发布、包生成、扩展安装和 Playnite 启动成功；安装 manifest `0.6.73`、插件 DLL `0.6.73.0`，安装 DLL 与本次构建哈希一致，Playnite PID `11068` 已启动且响应。

此前全量流程还暴露出隔离 WPF 测试的 shadow-copy 临时目录累积并耗尽磁盘，以及帮助 Popup 阻止测试进程退出；前者按类清理本轮新增的隔离 TEMP 项，后者在清理阶段关闭 Popup 并设置 2 分钟挂起诊断。诊断 ID `20261019T184725162Z-607fe456`，运行提交 `f70b806e21277f257f5b7b0f3eb7ae572cc66768`；分组结果和本地日志位置见 [DEV-INSTALL-009 全链验收记录](docs/design/reviews/DEV-INSTALL-009-20261020/README.md)。此次验证了构建、测试、安装与进程启动；没有声称已检查真实 Playnite 窗口的页面视觉效果。

## 2026-10-20 用户反馈：页面与表格滚动区修复

存档历史、候选路径、任务中心和当前媒体页采用 PageHost 视口内的页面滚动，加上限高的表格/列表内部滚动。数据仍可通过内层表格滚动查看，页面向下滚动可到达详情；收件箱尺寸算法保持原状。SDK8 Release solution `0/0`，定向 WPF `9/9`，全尺寸双主题 RenderHarness `0 PROBLEM`；合成截图和几何/TRX 见 [滚动区证据](docs/design/reviews/UI-SCROLL-VIEWPORT-20261020/README.md)。RenderHarness 为合成数据、逻辑 DPI 1.0，不代表真实 Playnite 呈现。滚动修复后续接 S04-04 和 S04-05；当前下一项 READY 为 S04-06。

## 2026-10-20 Round4 S04-05 已验收，下一项 S04-06

S00-01 已由当前账本和既有行为证据标为满足；S00-02 的完整 UI 门禁仍受 R05 `GetCursorPos` 权限边界阻挡。S04-03 使用既有 `f51e5c9e` 同步缩放/平移能力，详细受控证据见 [S04-03](docs/design/reviews/ui-finesse-round4-20261018/evidence/S04-03/README.md)。

S04-04 提交 `9e0af12b` 加入图片擦除对照、A 左/B 右、透明棋盘背景和键盘位置调整；复用现有异步缩略图源。SDK8.0.423 Release solution `0/0`，媒体比较生产 WPF 行为 `2/2`；证据、TRX、合成渲染截图和哈希见 [S04-04](docs/design/reviews/ui-finesse-round4-20261018/evidence/S04-04/README.md)。VSTest 成功，但 WPF 关闭阶段输出 `TextServicesContext.InvalidComObjectException`。真实 Playnite、真实媒体、物理 DPI 和物理鼠标拖动未验证；此功能只供人工视觉比较，不判断内容一致性。

S04-05 后续已在 `4d0bfd3c` 完成并推送：比较页新增上一/下一候选与当前位置，固定 A，只在已加载筛选结果内切换 B，保持详情选择和按钮焦点；损坏/缺失图跳过并按边界回退，沿用现有缩略图取消和 generation guard。SDK `8.0.423` Release solution `0/0`，候选导航、生产 WPF 与缩略图迟到行为 `6/6`；受控证据见 [S04-05](docs/design/reviews/ui-finesse-round4-20261018/evidence/S04-05/README.md)。真实 Playnite/物理输入和完整 WPF 隔离循环未验。

S04-06 已审查为 READY：生产代码只有正式收藏/备注和正式忽略/归类，没有比较会话内可撤销的人工意见标记。建议本轮标记限于比较会话，清空/关闭后不持久化，也不改媒体元数据或文件。

## 2026-10-19 用户反馈：窗口布局与弹出样式

已实现五项 UI 调整：头部 `1200 DIP` 起同行；列选择 ContextMenu 使用主题玻璃与圆角模板并播放可关闭的淡入位移动画；存档详情展开保留四行历史表格；媒体详情展开保留一整行卡片；通知操作按钮等高，空状态限制在表格列内居中并复用共享面板样式。SDK8 Release 解决方案编译 `0/0`，定向 WPF `15/15`，全尺寸 RenderHarness `render-qa OK`，最终 shell QA 与源码校验通过。

本次未在真实 Playnite 安装/展示新 UI，也未执行完整 WPF 隔离循环。RenderHarness 离屏 DPI 为 1.0 DIP，不代表用户物理屏幕呈现。当前验证时工作树含用户未提交的 S04-01 媒体比较更改；本轮提交需排除其 ViewModel、`MediaCenterView.xaml`、FakeDashboardData 和对应测试。下一步可让用户用 `DEV-INSTALL-009` 一键复测，再根据实际 Playnite 窗口尺寸校准细节。

## 2026-10-19 一键构建 SDK 修复进度

用户最新 `DEV-INSTALL-008` 日志在编译前失败：`global.json` 要求精确 SDK `8.0.423`，机器只有 `9.0.302`。`DEV-INSTALL-009` 保留精确锁定并通过官方安装脚本把缺失 SDK 安装到当前用户 LocalAppData；SDK 准备和后续完整检查均在关闭 Playnite/Worker 前进行。复跑发现 Windows PowerShell 5.1 会把 dotnet 的预期 stderr 提升为终止错误，现已修正。系统仅有 SDK9.0.302 的本机已成功自动安装并选中 SDK8.0.423，`Test-SdkBaseline.ps1` 通过，诊断 `20261019T014732824Z-823abe5a`。干净提交 `9a44c50e` 的 `-SkipTests` 一键 build/publish/install/start 流程也通过，版本 `0.6.73` / DLL `0.6.73.0`，Playnite 与 Worker 已运行。

当前默认全测试一键流程仍在验证中。此前另一次全量 WPF 隔离循环发现 133 类、前 5 类通过，第 6 类时受新用户输入控制中断（诊断 `20261019T010804009Z-d25e5350`），不是测试失败；其输入含未提交 S04-01 源码。首次 SDK 下载已写入用户 LocalAppData；另一台新环境首次使用仍需能访问 Microsoft 下载源。用户保留的未跟踪 TRX/TestResults 未改动。

## 2026-10-19 Round4 S03-08 来源与方案验收

验收测试提交 `37f42cc2e9920574d43d13cf8018ad6dcf114158` 已受控完成，没有生产代码变化。SDK9.0.302 Release solution build `0/0`，Core 来源图 `10/10`，Playnite `15/15`（含 R09 `2/2`）；覆盖明确父 ID、两设备分叉、同指纹、时钟偏差、41 天 readiness、只读合成文件、草稿重验与原 Restore guard。source validation 通过。当前机器缺少 global.json 要求的 SDK8.0.423，SDK9 结果不替代 SDK8 正式基线；完整 WPF 循环、真实 Playnite与真实恢复未运行。证据：[S03-08](docs/design/reviews/ui-finesse-round4-20261018/evidence/S03-08/README.md)。下一项核对 S04-01。

## 2026-10-19 Round4 S03-07 脱敏方案导出

实现与受控验证提交 `eeedd003bb91a0e562529b45b15d4cde67f18db8`。恢复草稿支持先预览脱敏 TXT 摘要，再明确保存；只导出白名单字段，不含存档、路径、认证参数、内部 ID/HMAC/密钥或自动执行脚本。保存前发现预览对应草稿已变化时先更新预览并要求再次操作。SDK8 Release solution build 成功、0 errors（17 条 NuGet audit 源不可达警告），定向测试 `18/18`，source/XAML/WPF 静态审查、双主题 RenderHarness 和精确提交隔离 no-start 安装通过；版本 `0.6.73` / DLL `0.6.73.0`。未跑完整 WPF/真实 Playnite或导出文件对话框交互；Demo 目录不在 checkout。证据：[S03-07](design/reviews/ui-finesse-round4-20261018/evidence/S03-07/README.md)。其后 S03-08 已验收；当前核对 S04-01。

## 2026-10-19 Round4 S03-06 方案失效规则

实现与受控验证提交 `7366d8cfe76702350a4c397f8de8aeed8e482b38`。恢复草稿现记录对象、内容、映射或 readiness 变化引起的 `NeedsRevalidation` 状态；不会持久化原始路径/内容指纹/自由文本摘要，重验前也不会绕过 `RestoreConfirmationGuard`。SDK `8.0.423` 精确提交 Release solution build 成功、0 errors（还原阶段 17 条 NuGet audit 数据源连接警告）；定向 Playnite `14/14`（包含 R09 `2/2`）、XAML 25 文件检查、RenderHarness 双主题、隔离 no-start 安装通过，版本 `0.6.73` / DLL `0.6.73.0`。未跑完整 WPF 循环或真实 Playnite。旧 R09 运行仍缺 `summary.json`/TRX/DLL/PDB，不能识别旧输入。证据：[S03-06](design/reviews/ui-finesse-round4-20261018/evidence/S03-06/README.md)。其后 S03-07、S03-08 均已完成；当前核对 S04-01。

## 2026-10-19 Round4 S03-05 方案差异比较

实现与受控验收提交 `b2c1e3de5fa822bcd8f2b04ddfe8b66d41411457`。恢复草稿比较支持明确 A → B 方向，通过 DPAPI 保护的 HMAC-SHA256 映射指纹区分同版本的映射变化，不保存原始路径或密钥；无法确认的指纹和大小保持未知。SDK `8.0.423` Release solution build `0/0`，Playnite `7/7`、Worker `4/4`，RenderHarness 双主题 `render-qa OK`，精确提交的隔离 no-start 安装通过（版本 `0.6.73` / DLL `0.6.73.0`）。未运行完整 WPF 循环或真实 Playnite。用户旧 R09 `summary.json`、TRX、DLL/PDB 暂不可用，旧运行输入仍未知。证据：[S03-05](design/reviews/ui-finesse-round4-20261018/evidence/S03-05/README.md)。下一项按计划处理 S03-06 方案失效规则。

## 2026-10-19 Round4 S03-04 恢复方案草稿

实现提交 `1c7083e2e1f6b4e6a7c9881794e1f722e96148c0`；功能和隔离安装验证基于 `b5bb5158960e05a4c948124d9e29540f4a05812a`，最新完整一键复跑源提交 `d4068e6562b7d78a9893211a522059d5d1ce3c9f`。SDK `8.0.423` Release solution build `0/0`；Core `156/156`、Worker `380/1 skipped`、Playnite 非 WPF `475/27 skipped`，当前 main R09 `2/2` 与恢复草稿 `4/4` 通过。WPF 隔离跑到 `48/133` 类时，R05 Tooltip 用例无法读取启动前系统指针而失败；178 项通过、1 项失败、85 个类未运行。独立隔离安装、双主题 RenderHarness 复跑、source validation 和 XAML `25/25` 通过。首次 RenderHarness 快速切换时序探针未稳定，同一已构建 Harness 复跑通过。真实 Playnite 未启动。

用户粘贴的旧 R09 结果属于测试断言失败，不是编译错误；2 DIP→1 DIP 与历史 Chrome 厚度断言相符，该断言已在 `f13b9e05` 拆分为 Chrome 1 DIP、ValidationStroke 2 DIP。用户暂时没有该次 summary/TRX/DLL/PDB，无法确认旧运行是否用了该测试版本。当前 main R09 `2/2`；本机完整一键测试另在 R05 系统指针读取失败处中断，该边界不应与旧 R09 混为一谈。详细 TRX、隔离安装报告、渲染报告、截图和二进制哈希见 [S03-04 证据包](design/reviews/ui-finesse-round4-20261018/evidence/S03-04/README.md)。下一项按计划审查 S03-05 方案差异比较。

## 2026-10-19 Round4 S03-03 冲突原因对照

实现与证据已提交 `3268565cfe02bd8e8b8e6d059e6ff2ffbcc84720`，证据包：[S03-03](design/reviews/ui-finesse-round4-20261018/evidence/S03-03/README.md)。设备冲突详情现将检测原因、左右备份和设备信息、共同父版本、指纹可比性、两侧 readiness 状态及检查时间放在同一只读对照卡内；旧 sidecar 没有检查状态时显示未知。面板清楚区分新鲜度信息与分支胜负，不按最新时间建议覆盖。

全解方案 build `0/0`；Playnite `4/4`、Worker `2/2`、Core `7/7`、R09 `2/2`；RenderHarness `render-qa OK`。未运行完整 WPF 隔离循环、安装或真实 Playnite。用户确认旧 R09 诊断目录与 summary/TRX 不在手上；当前匹配 SDK8 测试程序集通过，但不能识别旧失败实际源码/DLL/PDB。继续按 64 项计划审查 S03-04 恢复方案草稿。

## 2026-10-19 Round4 S03-02 版本关系视图

提交 `fd59dcc1`。版本详情现展示来源父版本、分叉、内容相同版本、设备与不确定关系；历史通过时间键加备份 ID 稳定分页，每页最多 100 条。SDK `9.0.302` Release solution build `0/0`，Playnite 定向 `12/12`（R09 `2/2`），Worker `2/2`，RenderHarness `render-qa OK / 0 PROBLEM`。证据包：[S03-02](design/reviews/ui-finesse-round4-20261018/evidence/S03-02/README.md)。当前未启动真实 Playnite；本机只有 SDK9.0.302，而仓库要求 SDK8.0.423。

用户之前的 R09 失败缺同次 `summary.json`、TRX、DLL/PDB，旧运行输入仍无法确认；本轮当前匹配程序集 R09 通过不解释原失败。S03-02 已交付，继续按 64 项计划审查 S03-03 冲突原因对照的既有能力与依赖。

## 2026-10-19 当前构建失败复核

当前测试源已修复设置页数字范围校验数量断言：设置页现有 12 项（包含通知安静时段的开始/结束分钟），原断言仍为 10。SDK `8.0.423` 下解决方案编译 `0 warning / 0 error`；Core `147/147`、Worker `374 passed / 1 skipped`、Playnite 非 WPF 分组 `468/468`；修正后的 `NumericInputTests` `12/12`、R09 `2/2`。完整 Playnite WPF 132 类隔离循环未完成，因此不能称一键构建/测试/安装整链已通过；未安装或启动 Playnite。

用户此前的 R09 `Thickness(2) → Thickness(1)` 运行缺同次 summary/TRX/DLL/PDB。当前源码与新构建 DLL 下该类 `2/2` 通过，但旧运行输入仍无法确认。详细 TRX、build 日志和程序集哈希：[BUILD-20261019](design/reviews/ui-finesse-round4-20261018/evidence/BUILD-20261019/README.md)。Round4 继续 S03-02。

## 2026-10-19 Round4 S02-08 通知体验受控验收

核对并复用现有通知收件箱、Worker 持久化、过滤批量已读、页面缓存、通知去重和进度 UI Automation 行为；没有新增生产实现。SDK `9.0.302` Playnite 定向测试 `20/20`，Worker 持久化 `12/12`。VSTest 退出码为 0 且 TRX 通过；WPF 清理输出 `TextServicesContext.StopTransitoryExtension()` `InvalidComObjectException`。SDK8、完整 WPF 隔离、真实 Playnite、Windows 系统高对比及实际读屏器未验。证据和二进制身份：[S02-08](design/reviews/ui-finesse-round4-20261018/evidence/S02-08/README.md)。

## 2026-10-19 Round4 S03-01 来源关系模型

提交 `5e05aa73` 新增 Core 来源关系模型和构建器。父边根据唯一的 `ParentBackupId` 建立，等价内容根据内容指纹标记；报告缺父、重复/缺失 ID、歧义父引用、环和未知关系，不用时间戳推断安全线性历史。输入限 500 条并额外探测 1 条判断截断。SDK `9.0.302` Release Core 构建与全套测试 `156/156` 通过。SDK8、全解方案和真实宿主未验；没有 Worker/Playnite 接入，也没有读写存档。证据：[S03-01](design/reviews/ui-finesse-round4-20261018/evidence/S03-01/README.md)。现有 SaveCenter 仅有版本表/详情/日期范围，没有来源关系文字列表或分叉表达；下一项 S03-02 版本关系视图。

## 2026-10-18 最新进展：S02-06 重要通知例外受控验证通过

实现提交 `a6153219` 明确只有已知警告/错误通知突破临时静音与安静时段；未知级别按信息处理。设置说明任务/会话警告与错误仍显示并保存在收件箱，备份校验 Findings 继续由现有持久化列表承载。SDK9.0.302 Release solution restore/build `0/0`，弹窗策略 `22/22`、通知记录持久化 `7/7`、同次隔离输出 R09 `2/2`；source validation 通过。R09 运行目录中插件 DLL 与测试加载副本 SHA-256 一致。证据见 `docs/design/reviews/ui-finesse-round4-20261018/evidence/S02-06/README.md`。

当前主机只有 SDK9.0.302，仓库锁定 SDK8.0.423；正式 SDK8 基线、完整 WPF 逐类循环、一键安装和真实 Playnite 宿主仍未验证。用户旧 R09 诊断 `20261018T012631286Z-e027df9d` 的 summary/TRX/DLL/PDB 没有保留，所以旧失败具体输入仍未知；当前通过不能映射旧运行。

## 2026-10-18 进展回顾：S02-04 上下文跳转已实现，正式基线待验

提交 `1b17e4a0` 增加通知详情按稳定游戏 ID 打开游戏、按精确任务 ID 打开任务；缺失对象时保留摘要并解释，不按名称误跳。提交后 SDK `9.0.302` Release solution build `0 warning / 0 error`；通知/R10 `8/8`、Worker 精确任务查询 `10/10`、R09 `2/2`；双主题 11 尺寸 Offscreen Render QA OK。详见 `docs/design/reviews/ui-finesse-round4-20261018/evidence/S02-04/README.md`。

当前 shell 只找到 SDK9.0.302，仓库 `global.json` 要求 SDK8.0.423；本次从 `C:\Windows\Temp` 使用 workaround 参数且未 restore。完整 WPF 独立循环、真实 Playnite 安装/宿主呈现未运行。工作区保存的一键日志在 R05 测试读取系统鼠标位置时退出，未进入安装；这是与用户旧 R09 Thickness 断言失败不同的运行。用户确认没有旧 R09 的 summary/TRX/DLL/PDB，因此旧测试输入仍不可追溯。S02-05 和 S02-06 已在后续完成，当前下一项 S02-07。

## 2026-10-18 最新进展：S02-03 已读语义受控验证通过

实现提交 6d3a6f43 支持标记所选未读通知、只标记当前筛选结果内的未读项，以及确认后清除已读记录；清理不解决关联任务，也不删除任务、审计或备份/恢复证据。提交后 SDK 8.0.423 Release solution build 0 warning / 0 error，Playnite 定向 18/18（R09 2/2），Worker SQLite 7/7，Render QA 22/22，source validation 通过。WPF 静态审查 0 errors / 26 warnings / 177 info。全部报告、TRX、截图和 SHA-256 身份见 docs/design/reviews/ui-finesse-round4-20261018/evidence/S02-03/README.md。

首次布局探针发现 1040×700 的通知列表只完整显示 2/3 行；收紧摘要卡与操作按钮后，最终浅色/深色各 11 尺寸均通过，窗口列表为 3/3。完整 WPF 独立进程循环、真实 Playnite 安装/启动和宿主呈现未运行。

用户所贴诊断 20261018T012631286Z-e027df9d 是 R09 测试断言失败，不是编译错误。当前模板保留 Chrome 1 DIP 与独立错误描边 2 DIP；本次提交后 R09 2/2。旧运行缺同次 summary/TRX/DLL/PDB，实际输入仍未知。下一项按计划审查 S02-04。

## 前序进展：S02-02 通知收件箱受控验证通过

提交 `8810d9e6` 已实现通知本地记录生产者与收件箱入口。SDK `8.0.423` Release solution build `0 warning / 0 error`，Playnite 相关定向测试 `15/15`（R09 `2/2`），Worker 持久化 `3/3`，Render QA `22/22`，source validation 通过。测试 TRX、程序集/PDB SHA、Render QA 报告和截图见 `docs/design/reviews/ui-finesse-round4-20261018/evidence/S02-02/README.md`。完整 WPF 独立进程循环和真实 Playnite 安装/宿主呈现未运行。

用户贴出的外机日志是在测试阶段 R09 断言失败，不能称为编译失败。当前源码的 Chrome 边框是 `1 DIP`，单独 ValidationStroke 是 `2 DIP`；这次当前提交版重跑通过。用户目前没有旧诊断 `20261018T012631286Z-e027df9d` 的 summary/TRX/DLL/PDB，所以旧运行实际源码和程序集仍无法确认。S02-03 已在后续批次完成。

## 前序交接记录：S01-08 视图迁移受控验收通过

`d1005e33` 已提交视图配置迁移回退。SDK `8.0.423` Release solution build `0 warning / 0 error`，五个隔离测试类 `21/21`，包含 R09 `2/2`；source validation 与 diff check 通过。诊断 ID `20261018T110922448Z-da48bc13`，构建及测试身份见 `docs/design/reviews/ui-finesse-round4-20261018/evidence/S01-08/README.md`。真实 Playnite 设置读写、宿主双主题呈现和安装门禁未运行。用户旧 R09 失败缺 summary/TRX/DLL/PDB，因此实际输入仍未知；当前 R09 通过不能反推旧运行。

S01-08 后按 64 项计划审查 S02-02 收件箱入口的依赖与现有能力；S02-01 通知记录契约已完成。继续按单个小批实现、验证、证据、中文提交并推送。

## 前序交接记录：S01-07 已提交，SDK8 Release 与定向验证通过

本地 `main` 实现提交 `87520a92` 为共享 DataGrid 列宽控制器增加窄窗口临时压缩和放宽后恢复；设置、命名视图捕获使用偏好宽度，临时宽度不持久化。提交后 SDK `8.0.423` Release 解决方案构建 `0 warning / 0 error`；R06 列宽 `7/7`、真实窗口 resize `1/1`、R07 断点/缩放 `2/2`、TaskCenter `8/8`、R09 `2/2`，共 `20/20`。Render QA `OK`，Task/候选路径 `1040×700` 均为 `4/4` 可读行。摘要、TRX、DLL/PDB 身份和截图见 `docs/design/reviews/ui-finesse-round4-20261018/evidence/S01-07/README.md`。

用户外机 R09 输出是在测试阶段因断言失败而退出，不是编译错误。当前代码提交版使用全新隔离输出后 R09 `2/2` 通过。Git 历史显示 `f13b9e05` 已在 2026-10-12 把错误主边框断言从 `2 DIP` 改为 `1 DIP`，并增加独立 `2 DIP ValidationStroke`；外机日志的厚度值与旧断言吻合。但用户暂时没有该次 `summary.json`/TRX/DLL/PDB，无法确认实际运行的测试源码/程序集身份，不能把旧 DLL 或 checkout 写成定论。完整 WPF 隔离循环、安装/版本门禁和真实 Playnite 设置保存/双主题宿主呈现仍未验证；R05 `GetCursorPos` Win32 error 5 权限边界保留。S01-08 已完成受控验证，后续按计划审查 S02-02。

## 2026-10-18 最新续接：S01-05 已提交，SDK8 Release 与定向测试通过

本地 `main` 实现提交 `dcfca235` 为任务与存档表格命名视图添加临时差异提示和恢复操作。提交后 SDK `8.0.423` Release solution build `0 warning / 0 error`；视图配置 `9/9`、交互 `1/1`、列显隐 `4/4`、列宽 `6/6`、R09 `2/2`、R10 `6/6`、Q14 `1/1`，总计 `29/29`。Render QA `OK`，Task `1040×700` 为 `4/4` 可读行。用户提供的外机 R09 日志仍缺少其 summary/TRX 与测试/插件 DLL/PDB 身份；用户表示暂时没有这些文件。该日志行号/断言与当前源不一致，本机当前 R09 通过不能代替外机输入，未改焦点样式。完整 WPF/安装门禁和真实 Playnite 设置保存/宿主呈现仍未验证；此前 R05 `GetCursorPos` Win32 error 5 的环境边界保留。下一项按 Round4 计划为 S01-06。详见 `docs/design/reviews/ui-finesse-round4-20261018/evidence/S01-05/README.md`。

## 2026-10-18 当前 Round4 SDK8 门禁复核

S00-01 已满足；S01-01 至 S01-05 与 S02-01 均完成 SDK8 受控验证，详见各自 evidence README。S01-02 的密度/设置资源 `14/14`、R09 `2/2`、MediaInbox 几何 `3/3`、R23 双主题生产资源 `9/9`；S01-05 构建 `0 warning / 0 error`、定向隔离测试 `29/29`。完整 WPF 隔离此前在第 45/126 类的 R05 `GetCursorPos` 系统指针读取 Win32 error 5 处停止；剩余 WPF 类及安装/版本核验未运行。不要绕过指针权限；前置条件变化后继续 UI 门禁。

用户最新 R09 日志所示 `Expected 2 DIP / Actual 1 DIP` 与旧测试 `3f7d0b30` 相符；当前 `origin/main` 已含 `f13b9e05` 的 Chrome 1 DIP / ValidationStroke 2 DIP 修正，本机 SDK8 提交 `dcfca235` 当前 R09 `2/2`。用户表示暂时没有外机同次 summary/TRX，不能区分旧 checkout、旧测试 DLL 或旧 PDB 源映射；身份对照详见 [S01-05](docs/design/reviews/ui-finesse-round4-20261018/evidence/S01-05/verification-identity.json)。

## 2026-10-18 Round4 S02-01 通知记录代码完成

`83194c50` 新增通知记录 DTO、severity/规范化策略和 Worker SQLite 持久化 API。去重键唯一且同事件重放不覆盖首条；保留 30 天、最多 500 条，任务状态仍以原任务表为准。SDK8.0.423 Release 解决方案构建 0/0，新增 Worker SQLite 测试 `3/3`，完整 Worker `364` passed / `1` skipped。通知生产者尚未接入，S02-02 接续；未改 XAML/外发通知。证据：`docs/design/reviews/ui-finesse-round4-20261018/evidence/S02-01/README.md`。

## 2026-10-18 外机 R09 最新失败输入

诊断 ID `20261018T012631286Z-e027df9d` 的用户日志为 R09 `1/2`，行151期望 Chrome `2 DIP`、实际 `1 DIP`；该断言与旧 `3f7d0b30` 测试一致。`f13b9e05` 已将 Chrome 改为 1 DIP、独立 ValidationStroke 维持 2 DIP，当前 `origin/main` 包含该修正。本机 SDK8.0.423 当前隔离输出 R09 `2/2` 通过（TRX、测试 DLL/PDB SHA 已归档）。外机同次 summary/TRX/source HEAD/DLL/PDB 不在当前工作区，无法区分旧 checkout、旧测试 DLL 或 PDB；这不是编译错误。不要改模板或放宽断言。见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R09-FOCUS-CURRENT-MAIN-RECHECK-20261015/README.md`。

## 2026-10-18 最新续接：S01-01 视图配置模型代码完成

`25bbe005` 新增版本化工作视图配置模型并接入设置复制/重置；只存标量与稳定列键，旧设置缺字段和未知 JSON 成员兼容。SDK8.0.423 Release 测试项目构建 0/0，`ViewConfigurationDefinitionTests` 与 R09 合计 7/7。验证发现 Json.NET 默认填充初始集合会绕过归一化 setter，现用 `ObjectCreationHandling.Replace` 修正。未改 XAML、未验 Playnite host。证据见 `docs/design/reviews/ui-finesse-round4-20261018/evidence/S01-01/README.md`。

下一步按计划继续 S01-06 布局重置预览；S00-02 的真实安装/版本门禁仍未通过。S01-02/03/04/05 的设置加载保存和页面呈现均未在真实 Playnite 中验证。

## 2026-10-18 最新续接：S07-03 分支检查读取受限

Windows workflow 对纯文档与混合改动都执行 SDK8/SDK10 全流程。GitHub `GET /branches/main/protection` 返回 403，rulesets 查询为空但不能证明 classic required checks 配置；设置页不可用，`gh` CLI 不在本机。未改 workflow。取到 required-check 策略后再继续 S07-03。审查记录与重试条件见 `docs/design/reviews/ui-finesse-round4-20261018/evidence/S07-03/README.md` 和 `docs/ai/ENVIRONMENT_CAPABILITY_CACHE.json`。

S07-03 之外，Round4 继续处理依赖独立且可验证的小批；S00-02 的 SDK8/真实 Playnite 门禁不绕过，也不因本项阻塞而把总目标判完成。

## 2026-10-18 最新续接：S07-08 已定义整体交付边界

完整 Round4 64 项计划已由用户授权；`PROPOSED` 仅表示该项的能力/依赖审查尚未完成。单批交付时统一更新机器状态、生成账本、证据、验证边界与下一项。只有 64 个任务全部达到终态且所需宿主/设备/外部证据齐备时才能宣告计划完成。规则见 `docs/design/reviews/ui-finesse-round4-20261018/DELIVERY_AND_EXIT.md`。

S00-02 UI 门禁仍未通过；S07-03 是下一维护候选，需先核对 GitHub 实际 required checks 与混合路径行为。Round4 其他未开始任务仍在原计划范围内，继续按依赖推进。

## 2026-10-18 最新续接：S07-07 环境能力缓存已验收

环境准入缓存位于 `docs/ai/ENVIRONMENT_CAPABILITY_CACHE.json`。SDK8 缺失、Playnite 真实宿主未检查、CEF `0x5` 与 ETW 权限拒绝分别记录状态、已知检查日期、证据与重试条件；没有在无前置变化时重复探测。校验器已接入 `scripts/validate-source.py`，验证与证据见 `docs/design/reviews/ui-finesse-round4-20261018/evidence/S07-07/README.md`。

S00-02 仍未通过真实 Playnite UI 门禁；仓库缺精确 SDK8.0.423 时不启动后续宿主步骤，含该 XAML 改动的分支不推送。S07-03 的 CI 分流仍需先核对 actual required checks 与混合路径行为。

## 2026-10-18 最新续接：S07-05 已验收，S00-02 仍待 UI 门禁

现有 `build-diagnostics.ps1` collector 的 summary 现在带 OS/PowerShell/bitness/.NET runtime 身份；运行 `pwsh -NoProfile -File scripts/preview-build-diagnostics.ps1 -Path artifacts/build-diagnostics` 可在本机只读查看 commit/dirty paths、SDK、DLL/PDB 与测试/TRX 计数。旧 summary 会在本机从 raw TRX 重算，旧环境字段保持“未记录”。该入口不访问网络或 Playnite，也不采集用户存档/凭据。证据：`docs/design/reviews/ui-finesse-round4-20261018/evidence/S07-05/README.md`。

S07-01/02/04/06 已完成。S00-02 仍缺 SDK8.0.423 与真实 Playnite UI 验证；含该 XAML 改动的本地 `main` 仍不推送。

## 2026-10-18 最新续接：S07-02 已验收，S00-02 仍待 UI 门禁

Round4 64 项的结构化状态源为 `docs/design/reviews/ui-finesse-round4-20261018/ROUND4_STATUS.json`。更新 JSON 后运行 `python scripts/render-round4-progress.py --write`；验证使用 `--check`，同时已接入 `scripts/validate-source.py`。完成/受控/宿主状态与证据索引分开记录，旧 Q/R 项与外部阻塞不由该索引清除。证据：`docs/design/reviews/ui-finesse-round4-20261018/evidence/S07-02/README.md`。

S07-01/04/06 已完成。S00-02 仍缺 SDK8.0.423 与真实 Playnite UI 验证；本地分支包含该未准入 XAML 改动，继续不推送 `main`。后续继续按剩余独立项准入。

## 2026-10-18 最新续接：S07-06 已验收，S00-02 仍待 UI 门禁

诊断器现在从原始 TRX 生成 `summary.json.testSummary`，分别报告测试用例状态、各 suite 的 case/class 数，以及 WPF 隔离类运行数，并逐文件对照 TRX Counters。定向诊断测试通过；历史 SDK8 `c7f2a368` 的 27 个 TRX 复算为 1,073 个用例（1,054通过、1失败、18跳过），WPF 为24个隔离类/103个用例。历史 Q14 失败未被改写。证据见 `docs/design/reviews/ui-finesse-round4-20261018/evidence/S07-06/README.md`。

S07-01/04 已完成。S00-02 仍缺 SDK8.0.423 与真实 Playnite UI 验证；不要将含该 UI 改动的 `main` 推送。后续继续从 Round4 独立可执行项准入。

## 2026-10-18 最新续接：S07-04 已验收，S00-02 仍待 UI 门禁

根 `AGENTS.md` 新增 CI 结果记录规则：仅新失败/修复、发布或验收状态改变时提交新结论；不为纯文档提交触发的通过结果继续追加提交。run/job/artifact 身份仍须可追溯。本次只审查 workflow 和历史记录，未改 workflow/required checks；相关 path 分流属独立 S07-03。证据位于 `docs/design/reviews/ui-finesse-round4-20261018/evidence/S07-04/README.md`。

S07-01 的压缩/归档已完成。S00-02 门禁仍缺 SDK8.0.423 与真实 Playnite 验证；本地分支含该未准入 XAML 改动，继续不推送 `main`，并推进依赖独立的任务。

## 2026-10-18 最新续接：S07-01 已验收，S00-02 仍待 UI 门禁

S07-01 将当前事实入口从 254 行压缩到 32 行，并把完整旧正文保存在 `docs/ai/CURRENT_STATE_HISTORY_THROUGH_20261018.md`；归档正文与提交 `4ea5e694` 的父提交原文一致。证据位于 `docs/design/reviews/ui-finesse-round4-20261018/evidence/S07-01/README.md`。source validation、diff check 通过；本项不需要产品构建。

本机 SDK8.0.423 与真实 Playnite UI 门禁仍未满足，S00-02 继续保持代码完成待验收。当前本地 `main` 的提交包含这些尚未准入远端的 XAML 改动，因此不推送该分支；改走依赖独立或符合计划准入条件的工作项。保留的历史 TRX 和 `tests/GameSaveCenter.Playnite.Tests/TestResults/` 未修改。

## 2026-10-18 当前阶段：Round4 S00-02 代码完成，等待 UI 门禁

本轮没有复记旧 Q/R CI。S00-01 代码提交 `dd38f8caec8cc6c63080b2d7e1eb587e14e9cde8` 加入只读命令目录，复用现有工作区导航、设置回调、游戏选择、Ctrl+F 搜索和任务关联游戏命令；稳定 ID / 双语别名 / 分类 / 作用域 / 不可用原因齐备，缺少当前游戏/任务时阻止执行。详细证据和 TRX/DLL SHA 见[证据页](design/reviews/ui-finesse-round4-20261018/evidence/S00-01/README.md)。

S00-01 证据见上一节。S00-02 的实现已在本地提交 `29dc09b2` 和 `a58dd582`：Shell 内搜索 overlay，名称/别名/分类过滤，空查询限八项，显示作用范围/可用状态/禁用原因并虚拟化；Escape 保留 IME 组合语义。按最终提交 Release 目标构建成功，21/21 定向测试通过，200 项本地过滤 `0.554 ms`，source validation 通过；静态 WPF 审查 0 errors。

`GameSaveCenter-Run.cmd` 被实际调用，但本机只有 SDK9.0.302，仓库 `global.json` 要求精确 SDK8.0.423，故脚本在安装与版本核验前退出。Playnite 未进行插件启动、真实渲染、主题或 DPI 核验。遵守 UI_CHANGE_GATE，不把未通过门禁的 XAML 改动推送到 `origin/main`；本地 `main` 暂时领先远端两个产品提交和后续账本提交。补齐 SDK8 后先重新运行指定脚本，再依据真实结果验收并推送，之后才准入 S00-03。历史未跟踪 TRX/TestResults 原样保留。

## 2026-10-18 新明确范围：第四轮 64 项增量计划

用户要求继续补充有价值的功能/UI，并直接落地本地文档。本轮已核对 `4d55a7dd` 的源码与第三轮主账本：192 个主条目无精确“待开始/实施中”，但宿主/IME/跨屏/呈现等边界仍未全验。本轮只做规划，不新增产品代码，不重新声称 CI 或宿主通过。

新增[第四轮 64 项计划](design/UI_FINESSE_ROUND4_64_TASKS_2026-10-18.md)及[账本](design/reviews/ui-finesse-round4-20261018/ROUND4_PROGRESS.md)：按依赖推进；S00-01 已验收，S00-02 等待本机 SDK/Playnite UI 门禁。旧 Q/R 环境边界仍保留。


## 2026-10-18 最新交接：#1086 双 SDK 终态成功，外机 R09 未定位

当前分支 `main`，Actions [#1086/run `39077533150`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39077533150) 对文档提交 `5eb0d1fd9a06acedaa2bbdb52e3b810a37eb9341` 已整体终态 success。SDK8.0.423 job `117527685827`、SDK10.0.401 job `117527686008` 均通过 Release build/test、Core `125/125`、Worker `362/362` 和 Playnite WPF `125/125`；`ProductionShellChromeSourceTests` 为第 `17/125` 类。SDK8 Render QA、WPF 静态审查 `30 XAML / 0 errors / 27 warnings / 177 info`、扩展包及 diagnostics 上传成功。Artifacts 和 SHA 见 CURRENT_STATE / WORKLOG；产品代码基线为 `be314f59`。

#919/#920 失败的完整 SDK10 日志已表明旧动画测试夹具遗漏系统动效偏好输入；`48b78b3b` 已修正测试前提。#1056、#1084、#1085、#1086 的 SDK8/SDK10 全量 WPF 均通过，R00-03 受控条件已满足，历史失败保留。

用户当前外机 R09 文本 SHA-256 `F3DDDC6445E0BB1A5E4CD44EEEEC2C2C98834FC8C81482A67E456C5DB5864514` 与已有文本相同，仍是 `20261016T052146719Z-d9d99ac4` 的 `1/2` 失败。Thickness 2/1 堆栈与当前同名源文件第151行的 Brush 断言不一致；因暂无同次 summary/TRX/test/plugin DLL/PDB identity，根因未定位。用户说暂时没有 ZIP、继续主线，因此不等该输入。CI 不能代表外机实际 checkout、程序集加载或 Playnite 呈现。

R00/R01 freshness 对 `be314f59` 仍为 14 fresh / 0 stale / 0 matched paths。Round2/3 和 R23-08 当前无新的依赖满足且有具体缺口的 Q/R 小批；总目标未完成。下一可执行条件：新身份材料、新复现/范围，或宿主/设备/权限前置变化。保留既有未跟踪 TRX/TestResults；本轮没有新增 `.tmp`/artifacts。

## 2026-10-18 历史交接：#1084 双 SDK 终态，R00-03 已归因，外机 R09 仍未定位

当前分支 `main`、HEAD `4cde34ce465a99787ba3ef969a57bacf9e606550`。Actions [#1084/run `39070369704`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39070369704) 已终态 success：SDK8.0.423 job `117506536858` 与 SDK10.0.401 job `117506537093` 的 Release build/test、Core `125/125`、Worker `362/362`、WPF 隔离 `125/125` 均通过。`ProductionShellChromeSourceTests` 在第 `17/125` 类，两 lane 完成全组；SDK8 Render QA、XAML 静态审查和扩展包生成/上传成功。本次文档提交不改产品代码，最近产品代码基线为 `be314f59c6a3a0b93d890cf39ee4c6c26ff2c523`。Artifacts ID/SHA 见 CURRENT_STATE 与 WORKLOG。

已取得并检查 #919/#920 完整 SDK10.0.401 job log：两者同一测试类均 `11/13`，失败是活动侧栏过渡夹具未固定系统动效偏好；`48b78b3b` 增加测试实例级系统策略输入与矩阵用例，未改变生产策略。#1056 和当前 #1084 双 SDK 全量 WPF 均通过，R00-03 受控条件保持已满足；历史失败保留原样。

用户附加的外机日志 SHA-256 `F3DDDC6445E0BB1A5E4CD44EEEEC2C2C98834FC8C81482A67E456C5DB5864514` 与旧文本相同，仍为 `20261016T052146719Z-d9d99ac4` 的 R09 `1/2` 失败。Thickness 第151行不对应当前源码的 Brush 第151行；无同次 summary/TRX/test DLL/PDB/plugin DLL 身份，不能定根因或称外机已成功。用户暂时没有诊断 ZIP，依其要求不等待，外机问题保持独立未解。

R00/R01 freshness 对产品代码 `be314f59` 为 `14 fresh / 0 stale / 0 matched source paths`。重新读质量审查、第三轮账本与 R23-08 后没有新的依赖已满足且有明确代码缺口的 Q/R 小批。下一可执行条件是外机身份材料、新复现/明确范围，或相关宿主、设备、权限前置变化；总目标未完成。真实 Playnite、OS UIA/IME/读屏、物理 DPI/跨屏/呈现帧、ETW/宿主性能仍未验。保留工作树既有未跟踪 TRX/TestResults，不生成临时产物。

## 2026-10-18 历史交接：#1083 双 SDK 终态，外机 R09 未定位

本轮开始时分支为 `main`、HEAD 为文档提交 `0bde982d49700465498bd0732651e21c10cebe43`；它对应 Actions #1083/run `39066560072` 已终态成功，SDK8.0.423 job `117495353783` 与 SDK10.0.401 job `117495353928` 均成功。Release build/test、Core 125/125、Worker 362/362、Playnite WPF 125/125 通过；SDK8 Render QA、静态审查和扩展包上传也成功。产品代码基线为 `be314f59c6a3a0b93d890cf39ee4c6c26ff2c523`。这些 runner 结果不代表外机 checkout 或实际加载的程序集。

本轮外机粘贴文本 SHA-256 `F3DDDC6445E0BB1A5E4CD44EEEEC2C2C98834FC8C81482A67E456C5DB5864514` 与之前附件相同，仍是诊断目录 `20261016T052146719Z-d9d99ac4` 的 R09 `1/2` 失败。它的测试第 151 行报告 Chrome 厚度 2/1，而当前 main 第 151 行检查错误色 Brush，第 152 行检查 Chrome `1 DIP`，第 154 行检查 `ValidationStroke=2 DIP`。不据此改生产样式或放宽测试；缺同次 `summary.json`、TRX、test/plugin DLL/PDB identity，外机根因仍未知。本机 SDK9 与 GitHub SDK8/SDK10 不能替外机作结论。

R00/R01 freshness 对 `be314f59` 为 `14 fresh / 0 stale / 0 matched source paths`。重新核对 Round2/3、质量审查与 R23-08 后，当前没有依赖已满足且有具体缺口的 Q/R 产品代码小批；总目标仍未完成。下一步：外机同次身份材料到达时直接比对；如果没有材料，则有新复现、明确功能范围或宿主/设备/权限前置可观察变化时重做任务准入。真实 Playnite、OS UIA/IME/读屏、物理 DPI/跨屏/最终呈现、ETW/宿主性能保持未验。用户先前说明无法即时回复，因此本交接不等待其回复，也不要求重跑。

## 2026-10-18 历史交接：R08 修正双 SDK 成功（#1082）

代码提交 be314f59c6a3a0b93d890cf39ee4c6c26ff2c523 和文档提交 b9eb697152f6def41faeca5753e91e1053d3f829 均已推送 main。代码 Actions #1081/run 39063179675、文档 Actions #1082/run 39063783824 都双 SDK 终态成功。SDK8.0.423/SDK10.0.401 两边 Release build 0 warning/0 error、Core 125/125、Worker 362/362、Playnite WPF 125/125；SDK8 Render QA OK、静态审查 30 XAML / 0 errors / 27 warnings / 177 info、包生成及上传成功。Artifacts 的 ID、大小、digest 见 R08 evidence README。

R08 app-motion 夹具只改用 GscMotion 显式时长重载，原有动画启动、取消清钟、重启不重播断言保留。SDK9.0.302 / VSTest17.14.1 本机定向类 1/1 通过；它不代替 CI。前一 #1080 SDK10 失败于活动动画时钟正例，SDK8 lane 成功；修正后的 #1081 SDK8/SDK10 都通过。

外机 R09 日志的 Chrome 2 DIP 断言是旧测试形态（提交 3f7d0b30）；f13b9e05 起改为 Chrome 1 DIP + 独立 ValidationStroke 2 DIP。当前 main 与干净 CI 使用新形态，#1079/#1081/#1082 全量 WPF 通过；外机当时的 checkout 和 DLL/PDB 身份仍缺，不能确认旧/混合产物推测，也不能称外机成功。下一步是新身份材料到达后直接对照，不要求用户重跑。Round2/3/R23-08 当前没有新满足前置的 Q/R 代码缺口；物理 Playnite/输入/跨屏/最终呈现/ETW 边界仍未验证。

## 2026-10-18 #1077 终态与下一准入条件

`main` HEAD `e8edbfc073d6bf18e07980a60c778577f643c6be` 对应 Actions [#1077/run `39050532641`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39050532641) 已双 SDK 终态成功：SDK8.0.423 job `117448022062` 与 SDK10.0.401 job `117448022272` 的 Release build `0/0`、Core `125/125`、Worker `362/362`、Playnite WPF 隔离 `125/125` 均通过；SDK8 Render QA、静态审查和扩展包也通过。Artifact ID/SHA 见 `docs/ai/WORKLOG.md` 的 #1077 记录。本提交仅同步证据，产品代码基线仍为 `8fc980f`。

最新准入复核没有新的依赖已满足代码缺口；R00-01 至 R01-08 受控条件保持已满足。实时环境仍是单屏、无 Playnite/Worker；CEF `0x5` 不重试，ETW 权限不绕过。外机 R09 文本 SHA-256 与既有输入相同，仍是 `1/2` 失败，不是新运行；当前 main/CI 结果不覆盖它。下一可执行条件是取得该次诊断身份材料、新的具体复现/功能范围或相关环境前置变化；若有身份材料，先对照，不要求重跑。

## 2026-10-18 #1076 终态与当前可执行边界

`main` HEAD `32a62d8413f49d7f02a5909321377c9cdc09da22` 对应 Actions [#1076/run `39047011333`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39047011333) 已双 SDK 终态成功。SDK8.0.423、SDK10.0.401 均 Release build `0/0`、Core `125/125`、Worker `362/362`、Playnite WPF 隔离 `125/125`；SDK8 Render QA、30-XAML 静态审查和扩展包通过。Artifact ID/SHA 见 `docs/ai/WORKLOG.md` 的 #1076 记录。本次无产品代码变化，R00/R01 freshness 仍绑定 `8fc980f`。

当前 Round2/3 无新的依赖满足代码缺口；R00-01 至 R01-08 受控条件已满足。单屏、无 Playnite/Worker；CEF `0x5` 不重试，ETW 权限不绕过，物理跨屏不模拟。用户外机 R09 `1/2` 仍未定位，缺失败当次源、summary/TRX 与测试/插件 DLL/PDB 身份。下一执行条件：外机身份材料、新复现缺陷/明确功能范围，或相关宿主/硬件/权限前置可观察变化；材料到手先做身份比对，不要求重跑。

## 2026-10-17 #1075 终态与下一准入条件

当前 `main` HEAD `6e308527e5040b94da1f07199f93adddf8978e4a` 对应 Actions [#1075/run `39043054832`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39043054832) 已双 SDK 终态成功。SDK8.0.423 和 SDK10.0.401 均 Release build `0/0`、Core `125/125`、Worker `362/362`、Playnite WPF 隔离 `125/125`；SDK8 Render QA、静态审查与扩展包通过。Artifact ID/SHA 见 `docs/ai/WORKLOG.md` 的 #1075 记录。代码基线仍是 `8fc980f`。

重新核对后 R00-01 至 R01-08 均已满足；R00-08 的受控行为已覆盖，真实 Playnite/OS 输入待验。Q/R 当前没有新的依赖满足代码缺口。本机单屏 `\\.\DISPLAY1`、无 Playnite/Worker 进程；R23-04 不在既有 CEF `0x5` 状态重试，R23-05 不绕过 ETW 权限，Q24-03 等第二块真实活动屏。外机 R09 `1/2` 仍缺失败当次源、TRX、test/plugin DLL/PDB 身份；不要求用户重跑，取得材料后先做身份比对。下一步只在这些前置条件变化、收到新的可复现缺陷或明确功能范围时再领代码小批。

## 2026-10-17 #1074 双 SDK 终态与下一准入点

文档提交 `68c178968e535774d7acb68c8c01a67319c20804` 对应 Actions [#1074/run `39039120707`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39039120707) 已终态成功：SDK8.0.423 / SDK10.0.401 两 job 均完成 Release build/test，Core `125/125`、Worker `362/362`、Playnite/WPF 隔离 `125/125` 通过；SDK8 Render QA 和 WPF 静态审查通过，扩展包及双 SDK 诊断已上传。Artifacts 和 SHA 见 `docs/ai/WORKLOG.md` 的 #1074 记录。该文档提交没有改源码，R00/R01 代码 freshness 基线仍为 `8fc980f`。

外机 R09 `1/2` 仍是独立未定位失败；没有同次 checkout、summary/TRX、test/plugin DLL/PDB 身份，不能用本机或 CI 结果覆盖。Round2/3 与 R23-08 最新复扫没有新的依赖满足代码缺口。下一可执行准入条件是外机同次身份材料、新的复现问题或相关环境前置变化；不要要求重跑。真实 Playnite/CEF、OS UIA/IME/读屏、物理 DPI/跨屏、最终呈现帧及获准 ETW/宿主性能仍未验证。

## 2026-10-17 #1073 终态与当前准入复扫

当前 `main` HEAD `199b49381598aeffa362a4154dced74f202180e9` 对应 Actions [#1073/run `39027064950`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39027064950) 已终态成功。SDK8.0.423 job `117380045515` 与 SDK10.0.401 job `117380045495` 均成功；SDK8 完成 Release build/test、Render QA、静态审查与扩展包，SDK10 完成 Release build/test。该提交仅验证文档；R00/R01 freshness 仍绑定代码基线 `8fc980f`。

重新扫描 Round2 `208/208`、Round3 `192/192` 与 R23-08 后，没有新的依赖满足且有明确实现缺口的 Q/R 产品代码项。用户外机 R09 `1/2` 继续作为未定位的真实失败记录：缺该次 summary/TRX/test DLL/PDB/plugin DLL 身份，CI 不能替代外机结果。下一步仅在身份 artifact、新复现或环境条件变化时重新准入；获取 artifact 后先做身份比对，不要求重跑。真实 Playnite/CEF、OS UIA/IME/读屏、物理 DPI/跨屏、最终呈现、ETW/宿主性能仍待验。用户工作树中的既有未跟踪 TRX/TestResults 保留。

## 2026-10-17 #1072 双 SDK 文档检查点终态与外机 R09 断言映射

当前文档检查点的前序 `main` HEAD 为 `6c8c85b0efae4d15996a2c7382255fcbaa8d2a2d`，代码基线为 `8fc980fce5262411fbb16f0299583c34074f7f61`。代码提交 `547efd4badaa462379532c2c0c96eea5c5a5c524` 只扩展 Q06 按钮状态测试；本机 SDK `8.0.423` / VSTest `17.11.1` 的 Playnite 定向 Release `1/1` 通过，不是完整 solution build。#1070/#1071/#1072 对各自精确提交的 Actions 均双 SDK 终态成功。#1072/run `39023752118` 对提交 `6c8c85b0`：SDK8.0.423 job `117370558805` 与 SDK10.0.401 job `117370558697` 的 Release build、Core `125/125`、Worker `362/362`、Playnite source `107` 类与 WPF 隔离 `125/125` 类均通过；SDK8 另有 Render QA、WPF 静态审查、扩展包与 artifact 上传。artifact 身份和外机边界见[R00/R01 当前 main 复核](design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-CURRENT-HEAD-RECHECK-20261017-8FC980F.md)。

用户外机日志仍报告 R09 `1/2`、总体 exit `1`。其中 `Expected Thickness(2) / Actual Thickness(1)`、源第 151 行与历史提交 `e12119bf` 的旧 Chrome `2 DIP` 断言吻合；当前测试已经拆为 Chrome `1 DIP`、ValidationStroke `2 DIP`，源第 151 行是颜色比较。由此可确认外机日志的源码/PDB映射不等于当前测试形态，但外机 HEAD、dirty paths、测试 DLL/PDB 和 plugin DLL 身份缺失，具体根因未确认。#1070 runner 通过不能覆盖这次外机失败；不凭该日志改当前模板或弱化断言。详情见[外机 R09 复核记录](design/reviews/ui-finesse-round3-20260915/evidence/CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md)。

R00/R01 freshness 绑定代码提交 `8fc980f`：`14` 条、`0` 需重跑、`0` 个匹配源码路径，`packageIdentity=not-provided`。后续提交 `9feb88c0` 与 `6c8c85b0` 仅改文档，按 R01-07 不需重建源码 freshness。13 条 MEDIUM toolbar layout risk 仍保留；R23-08 重新准入未发现依赖已满足且有明确代码缺口的 Q/R 项。下一可执行动作是用户方便时比对外机失败同次 `summary.json`、R09 TRX/console 和 test/plugin DLL/PDB 身份；无需重跑。如果仍无法取得，按 R23-08 重做准入，等待新复现或环境前置改变后再选 Q/R。物理鼠标移出、异步命令取消、真实 Playnite、OS UIA/IME、物理 DPI/跨屏、最终呈现帧、ETW/宿主性能仍待验。工作树里既有未跟踪 TRX/TestResults 保留未动。

## 2026-10-17 #1068 终态与下一准入点

`main` 上文档提交 `b14e780daa11a074d63abc381d777c66a8d258a8` 对应 Actions [#1068/run `39010428185`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39010428185) 已终态成功。SDK8.0.423 job `117331623876` 与 SDK10.0.401 job `117331623679` 的 Release build、Core `125/125`、Worker `362/362`、Playnite source `107` 类、WPF 隔离 `125` 类（含 R09）均通过；SDK8 Render QA `0 PROBLEM`、静态审查 `30 XAML / 0 errors / 27 warnings / 177 info`、package/diagnostics 上传成功。Artifact IDs：SDK10 diagnostics `12092898627`、SDK8 diagnostics `12093722541`、package `12093881767`。

R00/R01 freshness 对 `b14e780d` 为 `14` 条、`0` 需重跑、`0` 源路径命中、`2,963` 比较路径，package identity `not-provided`。Round2/3 canonical `192` 项已复扫，当前没有新且依赖满足的产品代码缺口。下一步：有外机 R09 同次身份材料时先比对 summary/TRX/test DLL/PDB/plugin DLL；若无新外部输入，则遇到新可复现问题或环境前置变化时重做准入并选最小 Q/R 小批，不要求用户重跑或回复。用户外机 R09 的 `1/2` 是独立真实失败，不能由 #1068 通过覆盖。真实 Playnite/package-host、物理 DPI/跨屏、OS UIA/IME、最终呈现帧、ETW/宿主性能仍待验。

## 2026-10-17 #1067 终态与本机/CI/外机验证边界

诊断保护代码提交 `26de12257748075e024c61972da16e87fd4ddfce` 对应 Actions #1067/run `39006921277` 已整体终态成功：[GitHub Actions](https://github.com/Nikilua/GameSaveCenter/actions/runs/39006921277)。SDK8.0.423 job `117321161387`、SDK10.0.401 job `117321161211` 均 Release build `0 warning / 0 error`、Core `125/125`、Worker `362/362`、Playnite source `107` 类及 WPF 隔离 `125` 类通过（含 R09）。SDK8 Render QA `0 PROBLEM`；WPF 静态审查 `30 XAML / 0 errors / 27 warnings / 177 info`；`0.6.73.pext`、包和诊断 artifacts 生成/上传成功。Artifact IDs：SDK8 diagnostics `12092696132`、SDK10 diagnostics `12091697203`、package `12092870374`。

本机 build、精确提交的 GitHub runner、用户另一台电脑的实际安装/构建是不同证据。#1067 只证明该 commit 在两种 CI SDK/runner 输入下通过，不代表外机成功。用户本轮再次粘贴的 R09 文本 SHA-256 `F3DDDC6445E0BB1A5E4CD44EEEEC2C2C98834FC8C81482A67E456C5DB5864514` 与已有日志相同；外机 `R09 1/2` 和整体 exit `1` 仍是真实失败。line151 的 `Thickness(2/1)` 不匹配当前 main 同名源 line151 的 Brush 断言；外机同次 `summary.json`、test DLL/PDB 与 plugin DLL identity 仍未获得，故保持未定位，不改焦点样式或断言。详见[R09 外机复核证据](design/reviews/ui-finesse-round3-20260915/evidence/CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md)。

R00/R01 freshness 对 `26de1225` 为 `14 fresh / 0 stale / 0 matched source paths`、2,963 paths、package identity `not-provided`；只检查登记源码路径。R23-08 最新复扫未发现新的依赖已满足 Q/R 产品代码项。下一步在外机身份材料、新可复现问题或环境前置变化时重做准入并挑依赖满足的小批。真实 Playnite/package-host、物理 DPI/跨屏、OS UIA/IME、最终呈现帧、ETW/宿主性能仍待验。

## 2026-10-17 R09 诊断脚本补强（#1067 已收口）

在 `350cdcb1` 基线上，`build-diagnostics.ps1` 尽力把每个 DLL 的相邻 PDB SHA-256/大小/脱敏路径写入 `summary.json`；缺失时为 `null`，被锁/不可读时记录错误类型且保留构建结果。首版提交 `8a895807` 对应 Actions #1066/run `39003320618` 已终态成功。后续提交 `26de1225` 为锁定/不可读 PDB 增加非阻断保护，并由本机合成自测及 #1067/run `39006921277` 双 SDK CI 收口。此工具只增加诊断字段，不定位或覆盖外机真实 R09 失败；仍需同次 summary/TRX/DLL/PDB/plugin 身份。缺失的 `20261014T052440239Z-3fc9f5f1` 文件仍未取得。

## 2026-10-17 最新 CI 状态与待验输入

文档纠偏提交 5475ba7d9744ab107b159a89ef930d535f1f95cb 的 #1064/run 38996324038 已终态成功：SDK10.0.401 job 117289578704、SDK8.0.423 job 117289578557 均在 Windows Server 2025 干净检出上完成 Release build/test；两边 Core 125/125、Worker 362/362、WPF 隔离 125 类通过（包含 R09），SDK8 Render QA、WPF 静态审查、扩展包和 artifact 上传成功。artifacts：SDK10 diagnostics 12088185573、SDK8 diagnostics 12088616683、package 12088477370。它不能证明用户外机的安装/构建输入成功。

用户给出的 GameSaveCenter-build-diagnostics (4).zip 和 GameSaveCenter-sdk10-build-diagnostics (1).zip 是 2026-10-13、sourceCommit c9f9179d9b8d38e506dbb1402ae7030eafebea6c 的不同运行：SDK8 R09 2/2；SDK10 失败在 Worker 取消测试 356/357，未运行 R09 子类。它们不是 2026-10-14 外机目录 20261014T052440239Z-3fc9f5f1。外机错误 line 151 的 Thickness 2/1 是历史测试源码形态，具体 HEAD、dirty paths、test DLL/PDB/plugin DLL 尚缺。当前一键脚本使用每次独立输出目录，因此不先将原因归为旧 bin。待该次 summary.json、R09 TRX/console 与程序集身份可用后再定位；主线继续独立任务，不改生产焦点样式或放宽断言。

## 2026-10-17 当前续跑：Q11-07 双 SDK CI 已收口

Q11-07 代码与证据提交为 `6a909cb329d88af3da83a78446050a83d24ef246`；只在 `R08PageSwitchBehaviorTests` 增加环境变量 opt-in 的 Light/Dark 四个生产缓存页往返离屏截图/几何输出，生产 UI、服务、DTO、命令和导航未改。本机 SDK `9.0.302`/VSTest `17.14.1` 隔离 Release 行为类 `4/4`。测试/插件 DLL/PDB 身份见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/Q11-07-CURRENT-MAIN-VISUAL-RECHECK-20261017/README.md`；本机结果只适用于本机 SDK 和该源输入。

#1061/run `38986851956`（旧提交 `57023ac`）与 #1062/run `38988908489`（精确代码提交 `6a909cb`）均已终态成功。#1062 SDK10.0.401 job `117266648758`、SDK8.0.423 job `117266649011` 均 Release `0 warning / 0 error`、Core `125/125`、Worker `362/362`、WPF 隔离 `125` 类通过；SDK8 另 Render QA `0 PROBLEM`、静态审查 `30 XAML / 0 errors / 27 warnings / 177 info`、扩展包/诊断 artifacts 成功上传。详细日志、artifact ID 和准确边界见 Q11-07 证据包。

文档收口提交 `ee8d6aa8d32da2861167d9d49b28e1f912063a8b` 的 #1063/run `38992929963` 也终态成功：SDK10 job `117279033715` 与 SDK8 job `117279033559` 均通过；SDK8 再次完成 Render QA、静态审查及扩展包上传。#1063 不改变 Q11-07 代码基线。

8 张图是 96-DPI `RenderTargetBitmap`，不是桌面截图、Playnite host、物理 DPI 或 presented frame。Task 选中项不在当前 18-DIP 截图视口；选中 ID 由测试行为断言验证。外机 R09 `1/2` 仍是真实独立失败；缺该次 `summary.json`、HEAD、源和 test/plugin DLL/PDB 身份。本机和 CI 都不能代替它。

阶段状态：自动行为、受控双主题图像与精确提交双 SDK CI 均通过；Q11-07 最终仍因真实 Playnite、有效物理 DPI和用户屏幕呈现待验而未完成。Round2/3 与 R23-08 复扫未发现新的独立代码缺口；下一步在外机 R09 同次诊断 ZIP 可用、新复现问题或环境前置变化时重新准入。用户外机 R09 `1/2` 是独立真实失败，日志目录当前工作区不存在；已请用户方便时附加该目录，主线不等待。未访问真实存档/媒体/云端。

## 2026-10-17 当前 CI 与外机故障边界

当前分支为 `main`。已推送文档检查点 `01e99b6e41c05519a78dfae42a4ad97c6fcf9f1b` 的 #1057/run `38969111355` 终态成功；最后代码提交 `4a98148caf2b0909998824acc73119272fc0cb37` 的 #1056 SDK8.0.423 / SDK10.0.401 两 lane 也成功。它们验证各自 runner 输入，不是用户另一台机器的安装或本机状态证明。

历史 #919/#920 的 SDK10 完整日志已取回：两者各在 `ProductionShellChromeSourceTests` 13 项中 11 通过、2 个活动过渡断言失败；原因是测试只固定应用设置，没有固定可由 Windows 关闭的系统动效偏好。`48b78b3b` 后增加实例级测试 seam 与系统偏好矩阵，生产仍读取真实系统偏好；当前 #1056 双 SDK 通过。详见 [R00-03 历史失败复核](design/reviews/ui-finesse-round3-20260915/evidence/R00-03-SIDEBAR-MOTION-CI-FAILURE-RECHECK-20261017/README.md)。

用户外机 R09 文本 SHA 与既有附件相同：Expected `Thickness(2)` / Actual `Thickness(1)`、第151行是旧 `3f7d0b30` 断言形态，当前 main 在 `f13b9e05` 已拆为 Chrome 1 DIP 与 ValidationStroke 2 DIP。它仍是那次真实失败，不是新复跑；外机源/程序集身份未证实。若得到新失败目录，先比对 `summary.json`、R09 TRX/console、test DLL/PDB 和 plugin DLL 身份，再决定是否改代码。

下一可执行项：在新 R/Q 问题证据或环境前置变化时重做 R23-08 准入并挑依赖已满足的小批。现有边界为 R02-06 需要 Playnite 宿主菜单，R05-03 需要真实翻转呈现帧，R23-05 需要允许的系统跟踪输入；继续遵守 fail-closed，不以离屏/代理数据代替宿主验收。

更新：2026-10-17；分支 `main`；代码提交 `4a98148caf2b0909998824acc73119272fc0cb37`。本轮只修复 Worker 取消回调测试夹具时序，无生产代码变化。本机 SDK8.0.423 官方 Release build `0/0`、Core `125/125`、Worker `361 passed + 1 NotExecuted`、目标取消测试 `50/50`；R09 定向 `2/2`。本机全量 WPF 隔离在 R05 Tooltip `GetCursorPos` 前置条件失败处停止。其后 #1056/run `38962119739` 的 SDK8.0.423 与 SDK10.0.401 Actions 两 lane 均成功，完整 Release build/Core/Worker/Playnite WPF 组通过；SDK8 另 Render QA、静态审查和扩展包成功。证据见[Worker 取消回调与双 SDK 复核包](design/reviews/ui-finesse-round3-20260915/evidence/CI-WORKER-CANCEL-CALLBACK-RECHECK-20261017/README.md)。Q24-04/R00-R01 事实仍见[Q24-04复核包](design/reviews/ui-finesse-round3-20260915/evidence/Q24-04-LOAD-MORE-KEYBOARD-RECHECK-20261017/README.md)。

**当前 CI 结论：** #1056/run `38962119739` 对提交 `4a98148c` 已整体终态成功。SDK8 job `117179503173`、SDK10.0.401 job `117179503250` 均 Release `0/0`、Core `125/125`、Worker `362/362`、Playnite source `107` 类和 WPF 隔离 `125` 类通过；SDK8 另 `render-qa OK`、静态审查 `30 XAML / 0 errors / 27 warnings / 177 info`、扩展包生成与上传成功。#1055 的 SDK8 package 重跑失败在后续提交复现并修正；#1051 历史失败及 #1052–#1056 的各自输入仍按历史事实保留。该 CI 结果只证明 GitHub runner 构建输入通过，不能覆盖另一台机器的失败。

用户外机 R09 失败仍真实存在：Core `125/125`、Worker `357/357`、R09 `1/2`、整流程 exit `1`。line151 的 `Thickness(2/1)` 与当前源码 line151 颜色断言不匹配，说明测试源或配套 PDB/程序集输入至少一项不同；具体 checkout/DLL/PDB/脚本来源仍未知。外机、本机官方隔离都报告116个 WPF 类，但 #1055 同提交 SDK8 GitHub log 报125，类计数差不能单独定位根因。COM 收尾异常发生在断言失败之后，不解释厚度值。SDK9 本机定向和 #1047/#1053/#1054 通过均不覆盖外机错误。外机诊断目录不在当前工作区，取得同次 `summary.json`/R09 TRX/DLL-PDB 身份后直接分析，不要求用户重跑。

下一可执行动作：Round2 208 项、Round3 192 项和 R23-08 准入已重新扫描，没有发现新的可独立实施 Q/R 代码缺口；当前只读快照无可见 Playnite/Worker 进程且只有一块活动屏。用户外机 R09 的摘要文本与先前附件 SHA 相同，仍需那次诊断 artifact 中的 summary/TRX/test DLL/PDB/plugin DLL 身份才能定位；若 artifact 后续可用，先做身份比对，不要求重跑。否则等环境前置变化或任务表出现新的具体证据缺口，再按小批继续。真实 Playnite/package-host、物理键盘/屏/读屏、完整页面阅读顺序与视觉仍未验；CEF 权限、ETW/真实呈现保留外部边界。未绕过权限或修改真实存档/媒体/云端。
## 2026-10-15 R12-03 当前 main 行为复核

- 现有 `PathRemapService`、DTO、隔离索引与生产 `MaintenanceView` 已满足目标路径核对方向；没有重建业务服务或改生产 XAML。
- Playnite 测试实际加载生产 `MaintenanceView`：展开父级维护操作后验证 80 条列表、有限高度、Recycling、末行未实例化，以及长原/目标路径完整选择和 Copy `CanExecute`。Worker `PathRemapServiceTests 4/4` 覆盖异盘/相似根、确认门禁、缺失目标及索引/文件无写入。
- 自动证据与具体 SHA、TRX、未验边界见[R12-03 复核包](design/reviews/ui-finesse-round3-20260915/evidence/R12-03-CURRENT-MAIN-RECHECK-20261015/README.md)。本次 WPF 测试初跑发现夹具未展开“更多维护操作”Expander，修正的是测试选择状态，未改生产实现。最终 `3/3` 与 `4/4` 通过。
- 下一项 Q24-04 键盘次序完整业务状态行为补证；Q24-03 第二显示器及真实 Playnite/OS 输入仍保持外部待验。

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

1. 保持 `main`；保留其他任务的 R04/R05/R07 未跟踪文件，显式暂存本阶段的代码/证据/文档路径，不能 `git add .`。
2. 收口 R13-08：本机 SDK `9.0.302` 定向 Release build 后行为类 `16/16`；`validate-source.py` 和 `git diff --check` 已通过。提交前归档成功 TRX/哈希并清理本阶段生成、且未被证据引用的 `.tmp` 和失败 TRX。
3. 按中文历史风格提交并 push `main`。通过 GitHub 插件查新 Actions，SDK `8.0.423` 和 `10.0.401` 分别追踪到明确终态；CI 运行中不得签收。若失败，先取得完整 job log，再按实际报错复现和修改。
4. R13-08 CI 关闭后按账本尝试 R05-03 的 fail-closed 隔离宿主预检；若 ENV-001 前置仍拒绝，不绕过权限，记录限制并转另一个依赖满足的 Q/R。
5. 外机 R09 已证实运行代码/符号行映射与当前测试源不一致，但具体是旧测试 DLL、PDB、checkout 或实际脚本尚待同次 `summary.json`/TRX/哈希。等待用户能提供时再定位，不暂停其它独立工作。
6. 保留真实 Playnite/package-host、OS UIA/IME、物理 DPI/跨屏、presented frame、ETW/宿主性能等未验边界；Demo 原目录缺失时沿用恢复生产基线。

## 验证边界

R13-08 行为测试使用合成 DTO、fake context 和隔离 STA；未启动真实 Playnite、连接真实云端、触碰存档/媒体或安装用户 Extensions。本机 SDK9 不是 SDK8 或 SDK10.0.401 CI 替代；R13-08 新改动的新 CI 尚待追踪。没有验证真实 UIA/读屏/IME、物理 DPI/跨屏或最终呈现帧。

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

## 2026-10-14 R11-03 恢复点

- 当前 checkout `main`，阶段基线/程序集源码版本为 `fd853380ec888ef94c112e255996b53106a0ddea`。R11-03 生产实现已存在，只增加生产 DashboardViewModel 筛选/分页/摘要行为测试与证据；R11 定向 7/7、同一 Release 测试程序集 R09 定向 2/2。SDK9.0.302/VSTest17.14.1，本机 Playnite net462/Tests net472 单项 Release 构建 0 warning / 0 error，不是完整 solution build；SDK9不得替代SDK8或CI SDK10.0.401。
- 当前阶段变更包括 `tests/GameSaveCenter.Playnite.Tests/R11DiffListSearchBehaviorTests.cs`、R11-03证据目录、R09复核记录和当前状态/记忆/日志。文档同步并提交后只纳入上述本阶段文件，保留 status 中所有其它未跟踪证据和 WPF 临时项目。
- 外机用户日志：Expected/Actual均为四边厚度2/1，stack line151；当前 main line151为颜色检查，line152 Chrome厚度1，line154 ValidationStroke厚度2。旧测试源或DLL/PDB错配只是推断；缺外机HEAD、源码和程序集身份。libpng warning与断言无关，WPF COM错误在失败后收尾阶段。当前本机 R09 2/2，未更改模板或弱化断言。
- #992/#993 失败原因未知，job日志/diagnostics API受403/登录权限阻挡。检查 R11-03 推送后的当前提交 SDK8/SDK10.0.401 Actions 并持续到终态；CI未明确成功/失败前不能宣称阶段完成。旧 Actions 列表快照不应当作新提交状态。
- 下一可执行项 R11-04：检查现有备注机制、取消及错误语义、重启读回和稳定 BackupId 选择；合成数据/隔离目录，禁止动真实存档。用户明日09:00前无法回复，不等待其外机信息，先推进独立任务。

## 2026-10-14 当前恢复点：R11-04 本机完成，待推送/CI

- 当前 `main` HEAD `40b0a980`；本阶段只改 `R11VersionNoteBehaviorTests.cs`、`R11VersionMetadataPersistenceTests.cs` 和对应证据/记忆文档。生产备注、锁定、Ludusavi metadata update、历史刷新、SQLite upsert 原有能力已确认。
- SDK `8.0.423`/VSTest `17.11.1`：Playnite `net462` 与 Tests `net472` 定向 Release 构建 `0 warning / 0 error`，R11 行为 `4/4`；Worker Tests `net8.0-windows10.0.19041.0` `1/1`。SDK `9.0.302`/VSTest `17.14.1` 相同测试 `4/4 + 1/1`。SDK8 重新构建当前 `40b0a980` 后 R09 焦点类 `2/2`。这些都是本机 SDK结果，不替代 CI SDK8/SDK10.0.401。
- R11-04 证据、TRX和程序集身份见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R11-04-CURRENT-MAIN-RECHECK-20261014/README.md`；R09 用户新日志和 SDK8 对照追加在 `evidence/CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md`。无 XAML/生产 UI 改动，不做截图/Render QA。
- 用户外机日志的行号/值匹配旧 `3f7d0b30` Chrome 2 DIP 断言；当前第151行是错误色，第152/154行检查 Chrome 1 DIP/ValidationStroke 2 DIP。缺外机 HEAD、源码与测试 DLL/PDB，不能确诊旧源/符号错配；libpng warning 不在断言调用栈，COM 异常在断言失败后的 WPF cleanup。不要为此改生产样式或放宽断言；用户不可回复前也不等待身份材料。
- R11-04 推送后检查新提交的 SDK8 和 SDK10.0.401 Actions 到明确终态；日志不可读时记录权限限制，继续其它已满足依赖任务。下一项 R11-05：检查保护 DTO、RetentionPlanner 与删除执行器/刷新顺序，使用合成数据和隔离目录证明保护版本总被跳过并有失效 ID 负例。
- 保留其它任务的未跟踪证据和 `TestResults` 内容；本轮自己生成的 TRX 只保留脱敏且 README 引用的证据副本。本轮 `.tmp/r11-04-current-main-sdk9302` 在确认副本后按绝对路径清理。

## 2026-10-15 当前恢复点：R10-02 本机复核完成，Actions 待确认

- main 源码基线 `6eac7b90bbbc0511ef3ed942422ef0b10b54a03b`；生产导航与游戏选框未改，本阶段只增加生产 Shell 同名游戏选择交互测试及其证据/状态文档。
- 本机 SDK `8.0.423`、VSTest `17.11.1`；Playnite `net462` 和测试 `net472` 定向 Release build `0 warning / 0 error`。R10 context `5/5`、导航 `20/20`、Shell GamePicker STA `7/7`，共 `32/32`；XAML `24/24`、source validation、diff check 通过。完整 TRX、身份和边界见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R10-02-CURRENT-MAIN-RECHECK-20261015/README.md`。
- 本机受控窗口使用合成 DTO；真实 Playnite、媒体后端、物理输入/IME、UIA/读屏、物理 DPI/跨屏、最终呈现与宿主性能未验证。外机 R09 断言仍无实际 HEAD、源码、DLL/PDB 身份，根因未确认。
- SDK8 不替代 CI SDK10.0.401。此前 #1010/#1011 的日志 API 为 403、根因未知；#1012 上次观察 In progress；`6eac7b90` 和文档提交的 run ID/终态待查。
- 下一项 R10-03：复核搜索快捷键在输入框、当前游戏选框、紧凑浏览器和对话框中的焦点/弹层作用域，先复用已有实现并补实际行为正负例。用户至明早 09:00 无法回复，不等待外机信息。

## 2026-10-15 当前恢复点：R10-03 本机通过，CI 终态待确认

- 基线 `main` / `95109c221ec85a52c61c51c0a119a2f3d97efacf`。R10-03 保留现有快捷键策略和生产命令；新增 routed-event 行为夹具验证 Media 工作区 Ctrl+F 真实移动键盘焦点到 `MediaSearchTextBox`，并验证弹层/编辑键负例。SDK `8.0.423` / VSTest `17.11.1`，Playnite `net462` / Tests `net472` Release 定向 build `0/0`，R10 邻接组 `13/13`，R09 类 `2/2`。证据 `docs/design/reviews/ui-finesse-round3-20260915/evidence/R10-03-CURRENT-MAIN-RECHECK-20261015/README.md`。
- 用户外机 R09 栈指向 line151、厚度 `Expected 2 / Actual 1`；当前源码第151行是错误色，第152/154行是 Chrome 1 DIP 与外置验证环 2 DIP。历史旧断言吻合，但外机提交、工作树、测试源/DLL/PDB 仍未知，不能确认错配根因。#967 SDK10 附件对应 `983fee5`，R09 `2/2`；本机当前对照也 `2/2`。不改生产模板/测试断言。
- `95109c2` 的 Actions run `38645600070` 上次 In progress；API 请求被本机代理拒绝，`gh` CLI 缺失，Actions 页读取超时，当前状态未知。本阶段新提交的 run 也要单独跟到成功或失败；SDK8 不替代 SDK10.0.401。真实 Playnite 全局热键、物理键/IME、UIA/读屏、最终呈现和物理 DPI/跨屏未验。
- 下一项可执行任务 R10-04：先核对快捷键帮助现有命令元数据及禁用过滤，再补真实命令可用/禁用行为。外机 R09 下一次可提供的最小证据为该次 `summary.json`、`tests/wpf.R09FocusOutlineBehaviorTests/` console/TRX、HEAD/status 和实际 DLL/PDB hashes；其余工作不等待回复。

## 2026-10-15 R10-04 当前 main 快捷键帮助复核

- R10-04 生产帮助目录沿用真实 FocusWorkspaceSearchCommand、CanExecute 与当前工作区文案。本轮只复核现有实现；Shell Popup 实际点击开闭、媒体工作区 Ctrl+F 条目和生产命令 CanExecute=false 时空列表都通过，没有生产 UI/XAML 改动。
- main 基线 6cf279238d55fe3d139fa2cb0eb7b09229a7478a。本机仅 SDK9.0.302/VSTest17.14.1，global.json 固定 SDK8.0.423；用隔离输出全解方案 Release restore/build 0 warning / 0 error。R10 帮助/邻接行为 18/18，R09 焦点类 2/2；XAML 24/24、validate-source.py、diff check 通过。SDK9 不能替代 SDK8 或 CI SDK10.0.401。
- 脱敏 TRX、DLL/PDB SHA/MVID 与未验范围见 design/reviews/ui-finesse-round3-20260915/evidence/R10-04-CURRENT-MAIN-RECHECK-20261015/README.md。
- 外机 R09 仍报 Expected 2 / Actual 1、line 151。本机同源隔离输出 2/2；当前源码 line 151 是错误边框颜色，line 152 为 Chrome 1 DIP，line 154 为独立 ValidationStroke 2 DIP。外机 HEAD、工作树、测试源码、DLL/PDB/资源未知，因此旧断言/符号错配仍只是推断，未更改模板或断言。iCCP 警告不在断言栈，COM teardown 异常发生在断言失败之后。外机复核见 design/reviews/ui-finesse-round3-20260915/evidence/R09-FOCUS-CURRENT-MAIN-RECHECK-20261015/README.md。
- 当前提交对应 Actions run ID/终态未知；本机代理拒绝匿名 API，已知 run 页面 cache miss。未关闭 CI 门禁。真实 Playnite、UIA/读屏、IME、物理 DPI/跨屏、最终呈现帧、ETW/宿主性能仍未验。下一项 R10-05：盘点既有筛选预设 DTO、持久化和删除/重命名确认取消路径，再补行为。


## 2026-10-15 当前恢复点：CLOSE-WRAP 已复测，单项系统指针环境失败

- 分支 main，基线 b04660e0af2d17daae8c7dec426598e90de738b2。只改 WorkspaceTreeGovernanceTests 的过时 registry 源码契约；定向 WorkspaceTree/R08 PageHost/R10 Search focus 共8/8，生产UI未改。Q14 654 DIP 行距测试1/1通过。
- SDK8.0.423、VSTest17.11.1 的完整 Release build 0/0、XAML24/24、Core125/125、Worker356 passed/1 skipped、Playnite source455 passed/27 skipped。122 WPF类总648 passed/39 skipped/1 failed；R05TooltipTiming系统指针用例在window显示前GetCursorPos返回false，另外2/3通过。全套1584 passed/67 skipped/1 failed，本机门禁未绿。
- 完整证据与TRX在 docs/design/reviews/ui-finesse-round3-20260915/evidence/CLOSE-WRAP-CURRENT-MAIN-RECHECK-20261015/README.md。外机R09的Expected2/Actual1仍因实际HEAD、source、DLL/PDB身份缺失而未定因。用户所附CI ZIP与#962/#963/#965/#967/#970/#971现有复核包一致。
- 提交推送本阶段后查询准确Actions runs，分别跟SDK8和SDK10.0.401到success/failure；CI日志不完整时不猜根因。随后重查当前main的R00/R01 freshness并只校正有证据的条目，再推进CLOSE-OPTICAL-01。未验：交互式系统指针、真实Playnite、物理DPI/跨屏、最终呈现帧、ETW/宿主性能。

## 2026-10-15 当前恢复点：R08 动效测试夹具已本机修正，Actions 待收口

- 从 `main` / `c30b89e2053e509c24ebd4135b3641b8d152867d` 开始。#1024/run `38677622085` 已终态失败：SDK8 job `116234717488` 和 SDK10.0.401 job `116234717135` 都在编译/测试步骤遇到同一 R08 opacity 动画未启动断言。GitHub 插件已取得两份完整日志。初始主题应用按真实 Windows motion policy 写入零时长，测试在 `Show` 前设置的 1 秒资源随后被覆盖。
- 只改两个 R08 测试夹具，在 `Show`/`UpdateLayout` 完成首轮主题应用后再写入测试时长；没有生产 UI 改动。SDK8.0.423 Release 测试项目 build `0 warning / 0 error`，应用设置类 `1/1`、系统策略类 `1/1`，source validation 与 `git diff --check` 通过。TRX、源码/程序集 SHA 与 CI 失败摘要见 `docs/design/reviews/ui-finesse-round3-20260915/evidence/CI-R08-MOTION-POLICY-FIX-20261015/README.md`。
- 本阶段提交 push 后须查询两条 Actions lane，并追到明确终态。之后按用户要求在新 source checkout 重查 R00/R01 freshness，接着推进 CLOSE-OPTICAL-01。外机 R09 的 `Expected 2 / Actual 1` 仍缺同次 HEAD、测试 DLL/PDB 身份；不能据本机 `2/2` 推断外机根因。真实 Playnite、系统偏好通知、物理 DPI/跨屏、呈现帧、UIA/读屏、ETW/宿主性能未验。

## 2026-10-15 当前恢复点：Actions #1026 Worker 取消竞态

- 分支 `main`，当前推送基线 `8e6513856500505d4608f76ef098236c64e1b5b4`。已查询 #1026/run `38690846810`：SDK8.0.423 job `116276140253` 与 SDK10.0.401 job `116276140402` 均已终态失败，完整日志已从 GitHub 插件取得。
- SDK10 日志确认为 `TaskCoordinator.CancelAsync` 在 `SemaphoreSlim.Release` 遇到已 Dispose 的 Gate。当前未提交代码不 Dispose runtime Gate，并在状态终态检查前保留 `CancellationRequested` 快速返回。该修复保留取消、终态和错误语义。
- 本机 SDK8.0.423 完整 Release build `0/0`、Core `125/125`、Worker `356 passed / 1 Named Pipe 环境 skip`、目标取消用例 `1/1`、R09 高对比 `3/3`。Playnite 全量扫测中断在 122 类中的 6 类，不能宣称全量通过。证据见 [CI-WORKER-CANCEL-RACE-FIX-20261015](design/reviews/ui-finesse-round3-20260915/evidence/CI-WORKER-CANCEL-RACE-FIX-20261015/README.md)。
- `.tmp` 清理已释放约 588 MB；SDK8.0.423 必须保留供后续验证。未跟踪的其他 Q/R TRX、临时 WPF 项目和 TestResults 保持原样。
- 下一步：同步最终事实后中文 commit/push 到 `main`，通过 GitHub 插件跟踪新 SDK8 与 SDK10.0.401 Actions 至明确终态；如果失败先取完整日志。CI 阶段收口后重跑 R00/R01 freshness，再处理其行为缺口。R09 高对比 CI 断言在本机未复现，真实 Playnite、物理跨屏/呈现帧/ETW仍未验。

## 2026-10-15 当前恢复点：R09 高对比按压态夹具修正

- 当前分支 `main`；前一推送 `c0a98906` 对应 #1028/run `38701408503` 失败。#1027/run `38698959702` 与 #1028 的 SDK8.0.423 和 SDK10.0.401 jobs 都在 R09 high-contrast matrix 的合成 Space KeyDown 后报 bare `Assert.True` Expected True/Actual False。两轮完整 logs 已通过 GitHub 插件读取；diagnostics artifact ID/SHA 和本机复核见 `design/reviews/ui-finesse-round3-20260915/evidence/CI-R09-PRESSED-STATE-RECHECK-20261015/README.md`。
- 当前代码阶段只修测试夹具：用现有 `HighContrastStateProbeButton` 稳定改变 `IsPressed`，保留生产模板 press/release、overlay、focus、动效/高对比组合断言；没有改生产 UI。SDK8.0.423 / MSBuild17.11.48 / VSTest17.11.1 本机 Release 测试项目 build 成功，R09 `3/3`，Q06 routed Space state `1/1`。TextServices COM 清理输出在本机 pass 后仍可见，故不归为测试失败根因。
- 当前阶段提交/推送后，先用 GitHub 插件定位新 run，检查 SDK8 与 SDK10.0.401 jobs；只有明确终态才结束阶段。失败时读取完整 log，不能用本机 SDK8 代替 SDK10。
- CI 收口后继续 R12-04 复核：`ROUND3_PROGRESS.md` 已有“已满足”行和 2026-09-19 行为证据；先确认当前源/测试/DTO 与证据基线，再复用已有 fake/隔离夹具做需要的当前 main 复验。不得访问真实存档。后续未验证边界：真实 Playnite/package-host、物理键鼠、UIA/IME、物理 DPI/跨屏、presented frame、ETW/宿主性能。
- 临时复测 TRX 已复制进证据目录；本阶段结束时删除自己创建的 `.tmp/r09-highcontrast-ci-fix-20261015`。保留已跟踪证据和其他任务未跟踪文件。

## 2026-10-15 当前恢复点：5862856e / R00-R01 fresh

- `main` 最新推送 `5862856e508fdb0abf2478fbbc54c34b34075692`。#1026/run `38690846810` 旧提交两条 CI lane 均失败；SDK10 日志根因为取消期间释放已 Dispose 的 Gate，已修复并本机 SDK8.0.423 验证。该新提交 Actions 尚无可读终态：GitHub 插件 combined status 返回空 checks，公开列表缓存只到 #979 并报加载错误，run ID 未取得，不判断为成功或失败。
- R00/R01 current-head freshness `14/14 fresh`、`0 stale`、`0 matched paths`，package `not-provided`；这是失效检查，不是宿主验收。任务表既有行为证据保持有效。
- 下一项 R12-04：查现有 PreRestore 保护备份、DTO/锁/编排；用 fake service 和隔离目录验证备份失败阻断恢复、重试/取消按同游戏锁和最新状态，不触碰真实存档。
- 本轮 `.tmp` 净释放约 588 MB；保留 SDK8.0.423 以及被历史证据引用的输出。其他未跟踪证据与临时项目保持原样。

## 2026-10-16 当前恢复点：R13-06 行为补证与外机 R09 输入身份

- 起始推送 HEAD `2341ca59e8bca692c9e778c2e31e62ba59016768`，分支 `main`。本轮唯一代码修改是 `tests/GameSaveCenter.Playnite.Tests/R13CloudTransferStageBehaviorTests.cs`，扩展真实生产 `MaintenanceView` STA 行为；新增两种 DTO/状态只用于合成 fixture，没有改生产 UI/DTO/Worker/通知语义。
- 本机 SDK9.0.302/VSTest17.14.1，新隔离构建的 R13 WPF 类 `14/14`、Worker retry `12/12`、Core recovery `1/1`、通知 deduper `6/6`、R09 `2/2`。source validation 与 diff check 通过。TRX/SHA 已归档；本轮自建 `.tmp/r09-r13-repro-20261016`、`.tmp/r13-06-recheck-20261016` 已在确认路径位于仓库 `.tmp` 后清理，释放 `178.1 MiB`；其他 `.tmp`、用户文件、其他任务未跟踪文件保留。
- 外机 R09 新日志 run `20261016T052146719Z-d9d99ac4`、输出 `artifacts/gsc-b/fd7672ef`、WPF classes 116；期望/实际与历史旧 Chrome 2 DIP 断言一致。main 及祖先 `983fee5` 的 line151/152/154 已分别是颜色断言、Chrome 1 DIP、ValidationStroke 2 DIP。本机当前源 R09 2/2 通过。外机没有提供 HEAD/status、源码/DLL/PDB/plugin DLL SHA，根因不能确定；README 已列明从同次 run 取身份的 PowerShell 命令。
- 当前已知 CI：#1041 SDK10 成功，SDK8仍在 package；#1042 SDK10成功、SDK8仍在编译/测试；#1043 两条 lane 均在编译/测试。新代码提交仍待 commit/push，随后查新 run SDK8与SDK10.0.401至终态；旧 run 状态不得混成新阶段结果。`#1040`成功不覆盖本轮代码。
- 下一可执行任务 R13-07 队列筛选/汇总：先核对状态筛选、全局计数、分页 consistency token、过期选择恢复和现有命令；仅用合成数据/fake/隔离状态。真实 network/rclone、Playnite host/通知、OS UIA/IME、物理 DPI/跨屏、最终帧、ETW/宿主性能未验。

## 2026-10-16 最近恢复点：R11 fake host 修复待双 SDK CI

当前 `main` 起始 HEAD 为 `2341ca59e8bca692c9e778c2e31e62ba59016768`。#1041 与 #1042 的 SDK8/SDK10 jobs 均成功；#1043 SDK10 成功、SDK8 在 R11 WPF 行为测试失败。失败根因来自 synthetic host：宽度 debounce timer 触发未初始化 Playnite Plugin 的 `SavePluginSettings/GetPluginUserDataPath`。已为 SaveCenterView 行为测试增加内部 persistence callback seam，生产默认 callback 保持原样；R11 fake host 使用安全 no-op。SDK9.0.302 本机新隔离 build 下 R11 6/6、R09 2/2、R06 6/6、Q13 单类进程 1/1，source validation 与 R00/R01 freshness 均通过；不替代 SDK8/SDK10 CI。

最近外机 R09 日志的 Expected2/Actual1 与旧版第151行断言吻合；当前源码第151行是颜色、第154行才是 ValidationStroke 2 DIP。没有该次诊断 `summary.json`，外机 HEAD/status/source/DLL 身份仍未知。下一步提交并推送本批，跟踪新 SDK8 与 SDK10.0.401 job 直到终态；若失败先读完整日志。两条 lane 成功后继续 R13-07 队列筛选与汇总，并在同次诊断 `summary.json` 可用时复核外机来源。没有验真实 Playnite host、OS UIA/IME、真实 DPI/最终呈现、ETW 或宿主性能；未访问真实存档、媒体与云端。

## 2026-10-17 当前恢复点：R00/R01 证据校正与外机 R09 失败

- 当前 `main` 代码基线 `5c81a07056d64482767d546efe043012abc1e6fc`；Render QA 报告 commit `01efb189488e6d6323d9386f7ff68057c0030cbe`。R00/R01 full RenderHarness 在仓库 SDK `8.0.423` Release build/运行通过，`render-qa OK / 0 PROBLEM`、14 fresh / 0 stale / 0 matched source paths / 2,919 compared paths。旧 Task/Settings 13 个问题未复现；13 MEDIUM toolbar audit 风险仍保留。
- #1053/run `38937972835` 对 `01efb189` 已终态 success。SDK8.0.423 job `117098144434`、SDK10.0.401 job `117098144743` 均 Release `0/0`、Core125、Worker362、WPF隔离125类成功（含 R09/R21）；SDK10 完整日志确认 `dotnet --version 10.0.401`。SDK8 Render QA `0 PROBLEM`，WPF 静态审查 30 XAML 为 `0 errors / 27 warnings / 177 info`，`0.6.73` 包与诊断 artifact 上传成功。当前仍有未提交 R00/R01 文档更新和精简证据，提交只纳入本阶段明确文件；status 中其余 Q/R TRX 与 TestResults 属其它任务，保留。
- 外机失败是真实且独立：日志 R09 `1/2`、整体 exit `1`，WPF 类 `116`。第151行 `Thickness(2/1)` 与历史旧断言吻合，不对应当前源码 line151 颜色断言；测试源码或其配套符号/程序集输入至少一项与当前 main 不一致。具体是旧 checkout、旧测试 DLL、旧 PDB 或其他构建输入尚未确认。本机 SDK9 `2/2`、#1047、#1053 双 SDK成功都不覆盖外机失败。确切失败诊断目录 `D:\workplace\Github\GameSaveCenter\artifacts\build-diagnostics\20261016T052146719Z-d9d99ac4` 不在本工作区；无需重跑，明日用户方便时直接附加此目录（优先 `summary.json` 与 R09 TRX/console）。采集器已有 sourceCommit/dirtyPaths/sourceFiles/SDK/test DLL hash/MVID 信息；PDB SHA 可从失败输出下读取。
- 通过 `scripts/validate-source.py` 与 `git diff --check`；本批仅文档与小型证据摘要，无产品代码变化。R23-08 当前准入复核未发现新代码缺口；下一步中文 commit/push 本次文档校正，查询对应 Actions run 并双 SDK 追至终态，再重扫 ledger 选择首个依赖已满足的现有 Q/R 项。若没有可独立执行项则记录明确环境门禁，不发明实现。
- 未验证：外机的真实 checkout/test DLL/PDB/plugin DLL 身份、外机安装结果、真实 Playnite/UIA/IME/读屏、物理 DPI/跨屏/最终呈现、ETW/宿主性能。合成数据/隔离目录之外的真实存档、媒体和云端未访问。已经清理本任务生成的 RenderHarness `.tmp`；保留 SDK、R08/R21 其它任务目录及其他未跟踪证据。
