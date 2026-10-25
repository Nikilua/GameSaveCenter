# GameSaveCenter 当前事实

更新：2026-10-25。历史按当时提交解释，不能覆盖本页当前结论。

## 最新根入口复验（2026-10-25）

- c7640d1f根入口第8类Compact再次触发原生UIA标题读取失败。现测试程序集统一为未显式命名的测试Window提供稳定名称，标准Peer保持开启；六类三轮78/78通过，新候选完整链待跑。证据：design/reviews/build-ui-reliability-20261022/evidence/T00-01/window-automation-lifecycle-20261025/README.md

## 前次R06根入口复验

- 0832aca8标准根入口run 20261025T054600655Z-8ab0ccc5编译0/0且Q06通过，但第59/145类R06因WindowAutomationPeer原生标题读取参数错误退出1，未发布安装。明确窗口UIA名称和关闭后排空后定向五轮25/25；新提交标准全链待重跑。证据：design/reviews/build-ui-reliability-20261022/evidence/T00-01/r06-window-automation-20261025/README.md

## 前次R08根入口复验

- 8e517cb4默认标准根入口Release0/0、Core157/157、Worker400/400且Q06通过；第74/145类R08合成hover被真实指针同步替换，根入口退出1，未发布安装。R08夹具保持所请求的输入状态后五轮15/15通过，新提交标准根入口待重跑。[失败和修正证据](../design/reviews/build-ui-reliability-20261022/evidence/T00-01/r08-input-state-20261025/README.md)

## 前次构建状态（2026-10-25）

- 最近一次成功的标准根入口完整链仍是 `dcbaab13`，见下方历史身份记录。用户另一台电脑在快进到 `d6a401af` 后的完整根入口中，Release编译0警告/0错误，但第29/145个WPF类 `Q06ButtonStateSequenceBehaviorTests` 失败；1299项中1280通过、18跳过、1失败，未进入发布、安装或Playnite启动。
- Q06失败是合成悬停后立即读取120 ms动画属性的测试采样竞态。提交 `ccfe4cbf546c0688f171ad5a24d35cc2d4308921` 仅在测试夹具读取前推进Dispatcher 180 ms，生产样式与断言不变。SDK8.0.423下定向类连续3/3通过；完整隔离Release构建0警告/0错误，1967项1899通过、68跳过、0失败，148份TRX计数一致，145个WPF类全部完成。[失败、修正和证据](../design/reviews/build-ui-reliability-20261022/evidence/T00-01/q06-hover-dispatcher-20261025/README.md)
- 当前本机Playnite Desktop及Worker正在运行，未执行会安装/启动宿主的根入口。修复候选仍需在提交后由外机运行默认 `GameSaveCenter-Run.cmd` 完整链；成功结果未收到前，不把T00-01恢复为已验收。外机stash已保存并且工作树快进至 `d6a401af`，保持stash不pop。

## 验证身份

- 当前完整验收代码：`dcbaab13`，在Q14/R08等可靠性修复后将同一checkout互斥锁扩至Windows全局命名空间。
- 最后完整标准入口成功：源码身份 `dcbaab13b9464523ffed6d88ca2748b6fb6dd647`；构建run `20261024T192816167Z-276884c0`，安装run `20261024T192814223Z-736788e1`。
- 命令：默认 `GameSaveCenter-Run.cmd`，退出0。SDK8.0.423 / Release，solution0警告/0错误；1967项1910通过、57跳过、0失败，148TRX计数一致、145WPF隔离类全部结束。Worker发布、ZIP/PEXT、安装及Playnite启动完成，六程序集磁盘版本/源码身份/SHA一致；本次未核验进程内插件加载路径。[本次完整链证据](../design/reviews/build-ui-reliability-20261022/evidence/T00-01/standard-dcbaab13/README.md)
- 最新定向验证：T01-08 `Test-BoundedProcess.ps1` 在PowerShell Core 7.6.2和Windows PowerShell 5.1受控中断均通过，精确自有子进程退出、无关进程保留、重试0；`Test-IsolatedTempCleanup.ps1` 双宿主失败/重试序列通过。该证据不覆盖真实键盘Ctrl+C。[T01-08回归证据](../design/reviews/build-ui-reliability-20261022/evidence/T01-08/README.md)
- 旧 R05 `GetCursorPos` error 5 未再中断最新完整链：`R05TooltipTimingBehaviorTests` 在 `fbd8a882` 的标准测试入口中3/3通过。该测试类结果不替代物理指针/真实宿主验收。
- 补充缺 SDK/冷 NuGet 路径：临时隔离 `dotnet` 后官方引导安装8.0.423并完整构建测试退出0；新的 NuGet 包/HTTP/CLI 缓存下还原与Release编译也退出0。此验证限本机网络，不能代替另一台电脑日志；[T00-03证据](../design/reviews/build-ui-reliability-20261022/evidence/T00-03/clean-machine-bootstrap.md)。
- `100429d0` 默认根入口run `20261024T173618477Z-d107dea1` 曾在Q14第35/145类因 `WindowAutomationPeer.GetNameCore` 读取已关闭HWND时Win32参数错误而失败；修正后的Q14定向六次均1/1，随后 `d0e11929` 标准完整链所有145个WPF类通过。失败与六次定向证据见[Q14复核](../design/reviews/build-ui-reliability-20261022/evidence/T00-01/q14-automation-drain-20261025/README.md)，完整链见[本次证据](../design/reviews/build-ui-reliability-20261022/evidence/T00-01/standard-d0e11929/README.md)。
- T00-07锁名已从`Local\`改为`Global\`。`dcbaab13`提交后的默认根入口全链退出0，SDK8.0.423、Release0/0、1967项1910通过/57跳过/0失败，发布/打包/安装/Playnite启动及六程序集身份/SHA通过。双PowerShell同会话锁回归各5/5；Task Scheduler权限拒绝跨会话实测，跨会话及跨用户ACL仍未验。失败候选隔离输出清理释放D盘约170MB，失败诊断与最新成功输出保留。[T00-07证据](../design/reviews/build-ui-reliability-20261022/evidence/T00-07/README.md)
- T03-06 清理与证据引用校验已验收：T00-07失败输出有持久清理记录，用户TRX/TestResults未动；本轮11份文档706个本地Markdown链接0失效，`validate-source.py`通过。[审计记录](../design/reviews/build-ui-reliability-20261022/evidence/T03-06/README.md)
- T03-07 与第四轮去重已验收：映射提交、证据和额外条件；Round4 64项渲染校验及7项已实现/1项外部阻塞与backlog一致。S07-03权限阻塞不变，未升级宿主待验。[映射与校验记录](../design/reviews/build-ui-reliability-20261022/evidence/T03-07/OVERLAP.md)
- T03-08最终交付检查单已验收：完整链绑定dcbaab13；后续main提交只含文档/证据。T01-07验收后32项中仍有10项待权限、外机或真实宿主验收，计划未整体完成。[检查单](../design/reviews/build-ui-reliability-20261022/evidence/T03-08/README.md)
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
