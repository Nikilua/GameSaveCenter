# U09-05 WPF 菜单测试宿主稳定性

## 现象与调整

基线 `cb0a11cbe32d19afc9a65f2b4c92209c0965c3d0` 的第二次完整入口尝试在第 10/154 个隔离 WPF 类中止。`DataGridColumnVisibilityBehaviorTests` 的前 4 项通过；打开列菜单的键盘导航用例期间，WPF `StylusWisp.WispStylusDevice.ChangeStylusCapture` 在 `Popup` 鼠标捕获切换时抛出未处理的 `NullReferenceException`，测试主机随之退出。第一次同 SHA 完整入口运行中该类 5/5 通过，说明失败与宿主输入栈有关，不能把该次中止记作成功。

此前沙箱完整入口已完成编译、Release测试、发布与打包，但在插件安装暂存目录写入阶段被系统拒绝。精确源码身份`cb0a11c...`、SDK8.0.423，2012项中1944通过、0失败、68跳过。跳过原因拆分为：57项已撤销“今日工作台”架构断言、1项需要显式交互opt-in的指针测试、9项在受限沙箱内无法建立的本地Named Pipe客户端测试、1项无法建立的Worker进程Named Pipe测试；跳过不计为通过。包身份是`0.6.73+cb0a11c...`。失败安装尝试ID `20261028T201041211Z-7b36f78d`：目标扩展目录未移动、`.new`暂存目录不存在、Playnite没有重启。该链以退出码1结束，不构成安装/启动门禁通过。统计、逐项skip缘由、完整控制台及安装记录见本目录`sandbox-root-*`文件。

该类只检查列菜单顺序、键盘循环导航与 UI Automation，不覆盖触控或手写输入。测试辅助器现在在创建 STA WPF 内容前，为隔离测试进程设置 `Switch.System.Windows.Input.Stylus.DisableStylusAndTouchSupport`。隔离脚本会为每个 WPF 测试类启动独立进程；生产程序集、生产 WPF 资源和宿主触控设置均未修改。该 AppContext 开关的说明见 [Microsoft .NET Framework 4.7 变更记录](https://github.com/microsoft/dotnet/blob/main/releases/net47/dotnet47-changes.md)。

## 验证

- SDK 8.0.423、Release；正式 `scripts/build.ps1 -SkipTests` 编译解决方案成功，0 warnings / 0 errors；XAML 结构检查 26/26。
- `DataGridColumnVisibilityBehaviorTests` 隔离定向 5/5 通过、0 失败、0 skip；含完整生产资源源码身份断言与菜单键盘/UIA用例。
- 只调整测试宿主；未改生产 XAML、主题、控件、命令或业务状态，不需要重做 Render QA。
- 第一次直接调用 `dotnet test` 的整类结果因未注入 `GscBuildCommit` 而有 1 项源码身份设置失败，随后使用正式构建脚本编译并重跑整类通过；该初次调用不计为产品测试失败或通过。
- 初次正式脚本空间预检因 C 盘临时目录仅余 605,229,056 bytes（要求 1,073,741,824 bytes）而停止。未清理 C 盘或其他任务数据；后续将 `TEMP`/`TMP` 指向本阶段 D 盘隔离目录，脚本预检在 D 盘通过（可用 13,300,178,944 bytes，预算 4,294,967,296 bytes）。

## 未完成边界

此记录只证明隔离键盘/UIA测试稳定。完整中文入口构建、测试、发布、打包、安装身份与 Playnite 启动尚待本轮候选提交验证；独立 `scripts/build.cmd`、新提交 GitHub baseline/SDK10 job 与 artifact 也尚待核验。此测试设置明确不提供 stylus、touch、IME 或真实 Playnite 宿主验收。

日志、TRX、测试序列、构建摘要与磁盘快照分别保存在本目录；所有仓库绝对路径已替换为 `[repo]`。
