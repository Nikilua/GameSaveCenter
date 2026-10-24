# T00-03 实际 SDK 引导与冷 NuGet 还原

本补充验证针对“另一台电脑没有预装所需 SDK”的路径，来源提交为 `64492e5b197352087d22a00239efa02fc204a762`，产品代码仍是 `51641d06dc17e0d2638a5fafe25d8f68660515bc`。

第一次运行先从进程 `PATH` 中移除 `dotnet`，再通过仓库的官方 Microsoft 安装器路径把精确 SDK `8.0.423` 安装到仓库 `.tmp` 临时目录；没有改系统级 SDK。SDK 引导、NuGet 还原、Release 编译以及核心、Worker、Playnite 测试均退出 0：1967 项中 1909 通过、58 跳过、0 失败，148 份 TRX 计数一致，145 个 WPF 隔离类完成。比最近标准入口多跳过的 Tooltip 鼠标交互用例，TRX 说明当前桌面不向测试 WPF 窗口分发鼠标事件。

第二次运行把 NuGet 包缓存、HTTP 缓存和 dotnet CLI home 全部指向新的 `.tmp` 目录。全新依赖还原和 Release 编译均退出 0、0 警告、0 错误；此轮按范围未运行测试。

脱敏后保留的两份结构化诊断摘要为 [完整 SDK/测试运行](sdk-bootstrap-full-run.json) 与 [冷 NuGet Release 编译](cold-nuget-build-run.json)，文件哈希和运行身份汇总在 [clean-machine-bootstrap.json](clean-machine-bootstrap.json)。完整标准安装/启动仍以 [T02-03 标准验证](../T02-03/standard-verification.json) 为准。

已按归档后的保留策略清理本轮8个专属 `.tmp` 目录（SDK、两套构建/诊断输出、NuGet包/HTTP缓存和CLI home）：9730个文件、1,978,685,641字节。删除前确认路径在仓库 `.tmp` 内、无重解析点且没有进程从这些目录运行；D盘可用空间从约10.75 GiB升至约12.61 GiB。保留了脱敏诊断摘要、最新标准包/构建与用户原有TRX/TestResults，未触碰用户旧结果。

这两次运行证明本机在未预置 SDK、空 NuGet 缓存时可完成相应路径；不能代替对方电脑的网络/代理、安全软件、磁盘或 Playnite 条件。对方电脑的首个失败阶段仍需其 `artifacts/one-click-install.log` 与最新 `build-diagnostics/summary.json` 确认。T00-03 的下载中断与损坏包仍为受控注入结果，没有据此把该任务提升为已验收。
