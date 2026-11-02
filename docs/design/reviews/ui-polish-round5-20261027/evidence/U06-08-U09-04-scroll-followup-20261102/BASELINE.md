# U06-08 / U09-04 大列表与滚动复测

## 范围与源码身份

本阶段只扩展大列表和滚动诊断，没有修改生产 UI 或行为。为把耐久报告中的滚动动作按页面拆分，只对 RenderHarness 临时加了带页面名的滚动测量标签；补丁见 [instrumentation.patch](probe/instrumentation.patch)，原始文件 SHA 和程序集身份见 [identity.json](identity.json)。探针结束后已恢复 tests/GameSaveCenter.RenderHarness/Program.cs，工作树在文档变更前恢复干净，文件 SHA 回到 9e349c9e… 。

源码提交为 f1a614a8fb4babffaf8cc2b1f7b6d1c1cb773582，SDK 为仓库要求的 8.0.423。scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot .tmp/round5-u09-04-scroll-followup-20261102/solution-build 退出码 0，0 warning / 0 error。RenderHarness 不在解决方案项目列表内，因此另用相同 SDK、MSBuildEnableWorkloadResolver=false 参数还原并编译其项目，均退出码 0，0 warning / 0 error。完整构建摘要见 [solution-build-summary.json](build/solution-build-summary.json)，SDK 与编译日志见 [build/](build/)，程序集 SHA 见 [identity.json](identity.json)。

## 大列表观察（U06-08）

scaleprobe 退出码 0，覆盖 Task 与 Media Inbox 的后端 1k / 5k / 20k。20k Task 表在滚动端点可见 7–8 行、实现 7–8 行；缩小窗口时实现 9 行。20k Media Inbox 后端通过既有业务容量限制显示 2k UI 项；未显示窗口的 detached scale fixture 在 20 次直接偏移跳转后达到 44 个已实现 DataGridRow，当时可见 4–5 行，随后改变窗口尺寸后实现数回到 6。20k 媒体卡片列表保留 2k UI 项，虚拟换行面板的顶部、底部和回到顶部各实现 10 个卡片容器。完整行和滚动诊断见 [scaleprobe 原始报告](probe/scaleprobe-report.txt)。

同一阶段的 R18 受控 WPF Window 测试退出码 0，1 passed / 0 failed / 0 skipped。20k Task 后端实现 10 行、视口 8 行；20k Media Inbox 后端保留 2k UI 项，实现 7 行、视口 7 行，Standard 模式、Item 滚动和行虚拟化均符合既有断言。该测试在显示窗口中采样 8 个滚动位置，而 scaleprobe 的 detached host 直接更新了 20 个偏移；44 与 7 的差别说明两种夹具不能直接等同，不能据 detached 数值声称生产容器逐步累积。保留 Media Inbox 的 Standard 行模式与关闭列虚拟化设置，因为已有代码注释记录了星号宽文本列与回收行组合的真实大数据布局问题；本阶段没有改动该安全例外。测试 TRX 及逐项输出见 probe/tests/r18-scale-isolated.trx。

## 滚动耐久复测（U09-04）

两轮均用受控 STA WPF 测试窗口（透明度 0.01）、1040×700 DIP、固定样本时钟、无强制 GC；每轮活动目标 120 秒，之后稳定观察 30 秒。第一轮 196 个完整周期，12 个资源样本，退出码 0；Task 页滚动动作 28 个样本，中位数 30.92 ms，p95 52.02 ms，最大 57.42 ms。第二轮 200 个周期、12 个资源样本，退出码 0；Task 页滚动 28 个样本，中位数 29.87 ms，p95 42.96 ms，最大 51.90 ms。逐轮完整报告见[第一轮](probe/perf/run-1/enduranceprobe-report.txt)和[第二轮](probe/perf/run-2/enduranceprobe-report.txt)。

耐久探针选择当前页面中 ScrollableHeight 最大的可见 ScrollViewer；测量动作把 ScrollToVerticalOffset 和同步 UpdateLayout() 合在一起，报告没有记下被选中控件的名称，因此此数值归属 Task 页面滚动动作，不能单独归因于 TaskGrid。页面切换布局 p95 分别为 463.20 ms 和 439.21 ms、最大值为 694.28 ms 和 585.26 ms；这些动作也包含整页切换与同步布局。本轮没有建立它们与历史布局长尾的因果关系，也没有定位动画或 GC 根因。U09-04 保持“进行中”。

## 运行器诊断与边界

一次未带仓库构建参数的直接 restore 失败，随后用仓库脚本现有的 MSBuildEnableWorkloadResolver=false 完成解决方案和 RenderHarness 编译。R18 测试曾两次在用例执行前被 .NET Framework xUnit 加载器挡住：较长隔离输出路径触发 MAX_PATH；缩短输出路径但遗漏仓库要求的 RunConfiguration.DisableAppDomain=true 后仍无法加载 adapter。按 scripts/run-playnite-tests-isolated.ps1 的运行参数重跑后，R18 用例通过。失败日志随本目录保留，均不是测试用例失败，见 [长路径失败](probe/diagnostics/r18-long-path-runner-failure.txt)和[AppDomain设置失败](probe/diagnostics/r18-short-path-runner-failure.txt)。

报告中的 RenderHarness 临时源码使 WorkingTreeClean=False；原始文件已恢复并核对 SHA。离屏/受控 WPF 结果不代表真实 Playnite/FusionX 宿主、UI Automation 客户端、实体键鼠/IME、物理 DPI 或呈现/VSync 帧。无真实宿主验证；U06-08 保持“代码完成待验收”，U09-04 保持“进行中”。存档恢复、媒体删除和云端写入未操作。

## 阶段清理

清理前，本阶段拥有的 .tmp/round5-u09-04-scroll-followup-20261102 为 990 个文件 / 184,690,404 字节，.tmp/r5 为 324 个文件 / 29,278,288 字节；本阶段 artifacts/build-diagnostics/20261102T012741568Z-eb41e9d5 为 6 个文件 / 162,296 字节。清单与前后磁盘空间、边界检查、保留项和删除明细见 cleanup.json。只清理这三个本阶段自有目录；用户和其他任务的 TRX、TestResults、存档、配置及 SDK 不在删除范围。
