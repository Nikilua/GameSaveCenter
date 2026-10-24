# GameSaveCenter 当前事实

更新：2026-10-24。历史按当时提交解释，不能覆盖本页当前结论。

## 验证身份

- 当前完整验收代码：`f577fd19`，包含命名WPF测试窗口、真实键盘焦点回归、显式失败阶段诊断与安装暂存/回滚artifact身份记录。
- 最后完整标准入口成功：源码身份 `f577fd192709d5a312c7bc72fd92249b6b5e31ab`；构建run `20261024T135653993Z-90e04544`，安装run `20261024T135651674Z-ade787f3`。
- 命令：默认 `GameSaveCenter-Run.cmd`，退出0。SDK8.0.423 / Release，solution0警告/0错误；1967项1910通过、57跳过、0失败，148TRX计数一致、145WPF隔离类全部结束。Worker发布、ZIP/PEXT、安装及Playnite启动完成，六程序集磁盘版本/完整身份/SHA一致；本次未核验进程内插件加载路径。
- 补充缺 SDK/冷 NuGet 路径：临时隔离 `dotnet` 后官方引导安装8.0.423并完整构建测试退出0；新的 NuGet 包/HTTP/CLI 缓存下还原与Release编译也退出0。此验证限本机网络，不能代替另一台电脑日志；[T00-03证据](../design/reviews/build-ui-reliability-20261022/evidence/T00-03/clean-machine-bootstrap.md)。
- T02-03定向双主题2/2、所在类11/11；真实WPF键盘焦点、选择ID/当前项和900/1400 DIP往返稳定。全链证据：[T02-03提交级完整验证](../design/reviews/build-ui-reliability-20261022/evidence/T02-03/standard-verification.json)。
- [当前标准链证据与边界](../design/reviews/build-ui-reliability-20261022/evidence/T00-01/standard-f577fd19/README.md)绑定f577fd19；后续文档提交不改变该产品构建身份。
- T01-01双宿主回归与16份JSON清单证据已归档、校验；清理8个自有`.tmp`夹具目录（逻辑2,363,091字节），D盘可用空间从12,290,236,416增至12,292,726,784字节。用户旧TRX/TestResults保留。
- T01-02清理预览在f577fd19代码身份上双PowerShell各9/9；结果路径已脱敏，所有预览`CanDelete=false`，人工复核引用/占用仍是后续步骤，不存在自动删除结论。[证据](../design/reviews/build-ui-reliability-20261022/evidence/T01-02/README.md)。
- T01-06双PowerShell归档/保留夹具当前重跑均通过，303文件真实归档SHA已核对、源原件保留；f577fd19默认根入口全链成功，补足最终链验收。只认可归档后隔离清理与保留行为，不开放通用自动删除。[证据](../design/reviews/build-ui-reliability-20261022/evidence/T01-06/README.md)。

- 前次aefbddfa的第101类崩溃保留；130cd307当前同类5/5、完整链0失败，不关闭UIA或扩大skip。真实诊断已显示190bbaea插件/Worker身份及实际加载路径。

## 当前范围

- 最近一次真实首页与维护诊断截图来自190bbaea，并确认插件/Worker构建身份、插件实际加载路径与安装报告一致；Ctrl+End可达诊断摘要，未触发业务操作。130cd307随后已通过完整安装/启动，本轮未重拍UI。
- b8eedc47的存档列表/详情交叉滚动证据保留；不将它替代190bbaea的完整双主题/窄短窗矩阵。
- 51641d06安装后的Playnite存档页新增深色常规窗口滚动截图：历史列表与详情分别滚动、另一侧锚点稳定，底部校验入口可达但未触发；安装身份可见，当前进程模块枚举未暴露插件程序集路径。[T02-08索引](../design/reviews/build-ui-reliability-20261022/evidence/T02-08/host-51641d06-index.json)
- 完整双主题/正常窄短窗、命令面板宿主验收、物理键盘/IME及物理DPI仍待验。设置对话框坐标偏差后已停止主题切换，没有改配置。
- T02-03已验收：活动WPF窗口真实 `Keyboard.FocusedElement` 双主题稳定，所在类11/11，51641d06默认完整链退出0。T02-02、04～07受控回归与全链通过；T02-08实施中。T03已登记共享能力、安装报告缺口、回退候选及证据链接修复，32项整体未完成。
- [32项专项账本](../design/reviews/build-ui-reliability-20261022/PROGRESS.md)、[第四轮机器账本](../design/reviews/ui-finesse-round4-20261018/ROUND4_STATUS.json)、[待办](../AUTONOMOUS_BACKLOG.md)是任务入口。
- SDK缺失为历史；CEF/ETW与S07-03 branch-protection读取权限边界独立保留，按[能力缓存](ENVIRONMENT_CAPABILITY_CACHE.json)重试。

## 保留、清理与后续

- 前次f25标准链测试通过，但运行中改文档触发打包保护退出1；[失败保留](../design/reviews/build-ui-reliability-20261022/evidence/T03-01/README.md)。先提交再冻结完整重跑已成功，未绕过保护。
- 25个本阶段测试结果文件先归档校验，再移除六个原临时目录；原失败、当前包/构建/诊断/宿主图与用户旧Round3 TRX/TestResults保留。详情见T02-08清理记录。
- 后续源码/测试/脚本/资源改动按影响运行定向检查与标准链；完整链期间不修改任何跟踪文件。纯文档按引用/源码校验，无需为其CI通过再追加记录提交。
- 另一台电脑日志的HEAD为ea926c5e，`main`落后`origin/main`两个提交，并存在多个本地修改/未跟踪文件；`GamePickerViewModel.cs` 的 `Updated upstream` / `Stashed changes` 冲突标记导致CS8300。后续远端提交76eab05c/f577fd19未改该文件。已取得部分diff，完整冲突diff与 `git stash list` 仍待提供；保留本地所有代码、测试和文档改动，不reset/覆盖。干净main构建可在完整stash（包含未跟踪文件）后快进并运行默认入口，再将本地功能按完整diff合入独立分支。
- T00-04路径与入口环境修复已验收，隔离克隆已归档后清理。外机R09附件期望值对应旧3f7d0b30测试；f13b9e05后主线改为Chrome1 DIP+独立验证描边2 DIP，130cd307当前类2/2。外机具体HEAD/DLL/PDB身份缺失，见专项复核；继续T02-08证据矩阵、SDK故障恢复与回退隔离验证，按32项账本逐项审计剩余条件；不要以受控或启动结果自动升级宿主验收。
- 旧事实原文见[截至2026-10-24历史](CURRENT_STATE_HISTORY_THROUGH_20261024.md)，更早记录仍可在Git和既有历史页追查。

## 持续有效的实现基线

- 产品版本0.6.73；共享入口为 `Themes/AcrylicProductionResources.xaml`、`DesignTokens.xaml`、`WpfUiProduction.xaml` 与 `Redesign.xaml`。
- Demo-first优先级保留；`GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design/` 当前工作区不存在，缺源时沿用生产共享样式，不伪造Demo对照。
- 保留业务命令、Binding、安全/取消/错误语义、虚拟化、UIA、键盘与Playnite兼容；游戏平台筛选以 `DropDownClosed` 提交。
- 行为矩阵入口 `scripts/e01-behavior-matrix.ps1`；`MANUAL QA REQUIRED` 项仍需对应人工证据，自动测试不替代。
- 开发启动继续阅读[PROJECT_MEMORY.md](PROJECT_MEMORY.md)、[WORKLOG.md](WORKLOG.md)、[DEVELOPMENT_HANDOFF.md](../DEVELOPMENT_HANDOFF.md)，按记忆协议推进。
