# U09-05：Render QA 隔离桌面复核（2026-11-09）

## 变更

提交 `0258ab3513c7d1bc95b8b622eead42a0fab30a76` 将交互式 `scripts/render-qa.ps1` 的 WPF RenderHarness 放到 `WinSta0\GscRenderQA_<guid>` 私有非输入桌面。运行前后读取输入桌面，并核对宿主 station、目标 desktop、父子输入 desktop 一致；超时只结束该次已确认拥有的子进程。服务/CI 无交互 `WinSta0` 时保留原执行路径。脚本初始化已有的 SDK baseline，不改 global.json、SDK pin 或 UI 代码。`scripts/validate-source.py` 加入对应的源码守卫。

此前逐类 WPF 测试已有私有桌面隔离；此次补齐 UI_CHANGE_GATE 的 `render-qa.ps1` 路径。旧 `capture-v*` 脚本不属于本次 UI_CHANGE_GATE 调用路径，也未运行。

## 验证

- 精确源码提交 `0258ab35`；SDK `8.0.423`；Release。
- `scripts/build.cmd` 退出码 0：160 份 TRX，2030 项；1962 通过、0 失败、68 跳过；157 个 WPF 类逐类隔离。Skip 保留原原因，未计为通过。完整组别及源文件身份在 [build-summary.json](build-summary.json)，控制台日志在 [scripts-build-console.log](scripts-build-console.log)；逐项 TRX 留在本机诊断目录 `.tmp/round5-renderqa-private-20261109/build-diagnostics-0258ab35/`。
- `scripts/render-qa.ps1` 退出码 0，SDK 8.0.423 的 RenderHarness 编译 0 warning/0 error；递归输出 394 张 PNG，报告无 `PROBLEM`、`FAILED`、`ERROR`，尾行为 `render-qa OK`。报告 SHA-256：`3625C3992BF17CB84D7BA3F3776BF0808C84C8C9623D7252B217CDC176419751`。
- 对 157 份 WPF 逐类测试隔离结果逐份复核：157 个唯一 GscWpfTest_<guid> 私有桌面、station 均为 WinSta0、输入桌面前后均为 Default、157 个退出码均为 0、异常记录 0。完整逐类清单见 [wpf-desktop-identities.json](wpf-desktop-identities.json)；这证明测试宿主桌面身份，不识别用户所见黑窗，也不是泛化的绝不闪屏保证。
- Render QA 子进程为 `WinSta0\GscRenderQA_da5c2ca1379f46eba408b9618c1f69bf`，输入桌面前后是 `Default`。Render QA 运行期间，另一个明确位于 `WinSta0\Default` 的 WinEvent 监听器记录目标程序窗口 SHOW 0 次、HIDE 0 次、退出码 0，见 [render-qa-default-window-events.log](render-qa-default-window-events.log)。
- 完整 `scripts/build.cmd` 的 WPF 隔离测试中后段（监听器就绪时测试已开始，最早几个类未覆盖）由 `WinSta0\Default` 监听器覆盖至最后一个 WPF 类：SHOW 0 次、HIDE 0 次、输入桌面保持 `Default`，见 [build-default-window-events.log](build-default-window-events.log)。该监听不是全 Windows 全进程视觉录屏；Core/Worker 阶段开始早于监听器。
- Source Validation、PowerShell 语法和 `git diff --check` 通过。焦点 WPF 静态审查为 26 XAML、0 errors；本批没有改 XAML，因此没有新的生产控件/宿主视觉声称。

## 结论边界

本次附件文件最后写入时间为 2026-11-06 13:59:27（+08:00），SHA-256 与先前附件完全相同，且无 HWND/PID/desktop 信息。它不能证明 11 月 9 日的新运行，也无法识别历史黑窗。Render QA 直接启动渲染宿主曾是一个可控的闪屏来源，已隔离；本批监听证明受控的 Render QA 和被监听的 WPF 测试中后段没有在输入 Default 显示目标窗口，但不等于用户手动启动、真实 Playnite 页面或所有运行时段都已验收。

U09-05 的中文→英文根入口安装/启动完整链仍是本轮 80 项最终候选的硬门禁；它会关闭并重启 Playnite，只在最终冻结候选上于隔离桌面执行一次。该结果和新 SHA 双 SDK CI 尚待记录。U09-05 保持进行中。没有改动盘符/卷、存档、媒体或云端数据。

## GitHub run #1494

精确源码 SHA `0258ab3513c7d1bc95b8b622eead42a0fab30a76` 的 [windows-build.yml run #1494](https://github.com/Nikilua/GameSaveCenter/actions/runs/42513349534) 已 `completed/success`。基线 job `128830946846` 和 SDK 10.0.401 job `128830946943` 均 `completed/success`；run / job / artifact 身份、大小、SHA-256 见 [ci-run-1494.json](ci-run-1494.json)。这只覆盖提交 `0258ab35`，不代表后续未提交改动。required-checks/ruleset仍因既往403记为“远端待验”，未重复请求。
## 本阶段临时文件清理

归档核对后，仅删除本阶段 `.tmp/round5-renderqa-private-20261109/` 内的预提交构建副本、其控制台副本及已停止监听器的源码/二进制/停止标志；具体清单见 [cleanup.json](cleanup.json)。保留精确提交 `0258ab35` 的当前构建诊断/TRX与当前渲染审计。D 盘可用空间从 10,823,155,712 增至 10,831,659,008 bytes（释放 8,503,296 bytes）；C 盘因系统并发占用变化 4,096 bytes。没有清理其他任务的 `.tmp`、`artifacts`、TRX/TestResults、配置或存档。
