# S02-05 临时静音与时段

实现提交：`2aa3fe3c`。设置页新增 15 分钟、1 小时和 4 小时临时静音，提供解除入口、当前状态说明，以及按本地时间运行的每日安静时段。安静时段支持跨午夜；相同起止分钟表示关闭该时段。临时静音到期点按 UTC 保存，重启后仍可正确到期；系统时钟或时区变化后，每条新通知都按当前本地时间重新判断。

静音只抑制信息和成功类弹出。生成的通知记录仍异步写入本地收件箱；警告和错误在 S02-05 中继续弹出。更改作为 Playnite 设置草稿提交，保存后生效；状态提示每分钟刷新，解除按钮在有效静音期间可用。新增配置参与设置复制、取消回滚、默认重置、便携设置导入/导出和正常 JSON 序列化。

## 提交后验证

- SDK `9.0.302` Release 全解方案构建：`0 warning / 0 error`。命令和完整日志见 [build.log](build/build.log)。为使用现有还原资产并绕过 SDK9 安装缺失的 workload resolver locator，从 `C:\Windows\Temp` 执行 `dotnet build GameSaveCenter.sln --configuration Release --no-restore --nologo -m:1 -nodeReuse:false -p:NuGetAudit=false -p:MSBuildEnableWorkloadResolver=false`，并显式传入提交与源码根身份。当前机器只安装 SDK `9.0.302`；仓库 `global.json` 锁定 `8.0.423`，因此这不是 SDK8 正式门禁，也未执行 restore。
- `NotificationPopupPolicyTests`：`20/20`，覆盖 UTC 到期边界、警告/错误例外、跨午夜和同日区间边界、系统本地时间变化、无效区间及 UTC 归一化。TRX：[notification-popup-policy.trx](tests/core-policy/notification-popup-policy.trx)。
- 设置持久化、取消回滚、便携导入导出与 WPF 设置页交互：`15/15`。包含 JSON 重启往返后静音到期、预设和解除按钮、安静时段开关及分钟输入绑定。TRX：[s02-05-settings.trx](tests/playnite-settings/s02-05-settings.trx)。
- 独立进程运行当前提交的 `R09FocusOutlineBehaviorTests`：`2/2`。TRX：[R09-focus-outline.trx](tests/r09-focus/R09-focus-outline.trx)。旧诊断 `20261018T012631286Z-e027df9d` 的 `summary.json`、TRX 与同次 DLL/PDB 身份仍未取得，所以当前通过结果不能反推出旧运行输入。与新增 WPF 设置页测试放入同一筛选进程时，窗口焦点前置条件曾失败；按仓库的逐类隔离方式运行 R09 后通过。
- 新增 `notificationsettingsprobe` 在浅色/深色主题及 `1040×700`、`720×640` DIP 下检查设置搜索、状态、四个动作和输入是否可见，以及滚动后是否可达；`4/4` 组合通过。报告与截图见 [render](render/notification-settings-probe-report.txt)。截图：[浅色 1040](render/notification-schedule-light-1040x700.png)、[深色 1040](render/notification-schedule-dark-1040x700.png)、[浅色 720](render/notification-schedule-light-720x640.png)、[深色 720](render/notification-schedule-dark-720x640.png)。该离屏探针不证明真实 Playnite 宿主或物理 DPI。
- XAML 结构检查覆盖 `25` 个文件并通过；`validate-source.py` 和 `git diff --check` 通过。日志分别见 [xaml-validation.log](build/xaml-validation.log) 与 [source-validation.log](build/source-validation.log)。构建、测试、渲染报告、源码及程序集的 SHA-256 见 [verification-identity.json](verification-identity.json)。

## 未覆盖范围

完整 Playnite WPF 逐类测试循环、SDK8.0.423 正式构建、一键安装和真实 Playnite 宿主未在本次运行。仓库保存的上一轮一键日志曾在 R05 `GetCursorPos` 系统鼠标位置读取处退出，未到安装；该运行与用户贴出的旧 R09 断言失败不同。本次提交验证的是 SDK9 受控构建及定向测试，任务维持“代码完成待验收”。
