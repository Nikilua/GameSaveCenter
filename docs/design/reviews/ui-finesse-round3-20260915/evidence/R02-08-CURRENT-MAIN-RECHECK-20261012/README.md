# R02-08 当前 main 动作文案复核

日期：2026-10-12
状态：已满足（当前 main 构建与行为证据复核；真实 Playnite 宿主仍未验）
当前源码身份：`4d166f017dcba8fbb07980ec21b690267b9ff377`（main；工作树干净）

## 当前实现与测试

先复用现有生产命令、DTO 与状态消息，没有增加服务、命令或业务协议。原实现提交 `43ea8436` 已在 main 历史中；相关生产入口后来经历布局与状态展示改动，但下列动作词在 `43ea8436..4d166f01` 中未被替换。当前源码可直接复查：

- Dashboard 的“重新校验”“重新加载详情”以及 Overview/Save Center 重新加载入口，仍绑定原 `ValidateCommand` / `LoadDetailsCommand`。
- Maintenance 的“校验远端内容”“重试上传”“下载到隔离区并校验”“快照并恢复”仍分别对应原验证、上传重试、隔离下载和恢复命令。
- `CloudTransferStatusDto` 与 Worker 失败/暂存失效消息仍使用“远端校验”，不混淆上传、下载和恢复结果。

当前 `R02ActionCopyTests` 保留 4 个针对生产 XAML/DTO/消息的检查。`c975e16d` 曾把按钮内容改为读取嵌套 `TextBlock`，并明确排除“重试”按钮后再统计 4 个重新加载入口；本轮在当前 main 的 Release source 测试组中实际通过 `4/4`。脱敏 TRX：[playnite-source.trx](playnite-source.trx) 保留整个 source 组的实际结果：`470 passed / 18 skipped / 0 failed`，其中含本项四个通过结果。

## 当前 main 验证

- 官方 `dotnet-install.ps1` 在隔离目录安装并确认 SDK `8.0.423`；系统 SDK / 全局 PATH 未改动。当前干净 main 身份 Release 脚本成功：XAML `24/24`、solution `0 warning / 0 error`、Core `125/125`、Worker `357/357`、Playnite source `470 passed / 18 skipped`、WPF `608 passed / 39 skipped`；合计 `1560 passed / 57 skipped / 0 failed`。18 项 source skip 与 39 项 WPF skip 按 TRX 的 `NotExecuted` 记录，未计入通过。
- `python scripts/validate-source.py` 通过。
- 当前身份 RenderHarness 构建 `0 warning / 0 error`；Light/Dark `render-qa OK`，报告计 `372` 张图并绑定完整 SHA、`WorkingTreeClean=True`、`DpiScale=1.00`（offscreen logical DIP）。人工检查 Save 1040×700 两主题图，“重新校验”“重新加载详情”清晰可见。Maintenance CloudQueue 两主题图也留存作页面上下文；图中没有展示远端验证/上传重试按钮，因此这些按钮的精确词仍以当前 `R02ActionCopyTests 4/4` 为证。
- 插件 Release 程序集 SHA-256：`B1B51B3609DD5E1639555E750AC8133E94B42B743AA34C1F76CB76E9A137F504`；MVID：`416ebff9-5a31-4a0d-94f3-f65954c16b93`；InformationalVersion：`0.6.73+4d166f017dcba8fbb07980ec21b690267b9ff377`。

精选截图：

- [Save Light 1040×700](screenshots/Save-Light-1040x700.png)，SHA-256 `94AEEA97957514FC609EAE01ACDF36470809560471A78E6B1F9DE0986014F026`
- [Save Dark 1040×700](screenshots/Save-Dark-1040x700.png)，SHA-256 `4F4656339DC4C2CAA59B79E62F608DBD731E328A11CA9CB78333A6E96F40C7C1`
- [CloudQueue Light 1040×700](screenshots/CloudQueue-Light-1040x700.png)，SHA-256 `9A1184A138CEDA0E31D46EB745E6FD2F900008F052AAC4781348A15DA34E451A`
- [CloudQueue Dark 1040×700](screenshots/CloudQueue-Dark-1040x700.png)，SHA-256 `0A43FA8914FADEB210C027A40C3BF047C170D377B24FD5621709F86C8D6892AA`

## 未验证边界与 CI

没有启动真实 Playnite，也没有执行真实云端校验、上传、下载、恢复或存档操作；没有生成或安装包，package identity 未提供。截图不代表真实宿主字体、物理 DPI、OS 输入/UIA、最终 presented frame、ETW 或宿主性能；Demo 原目录在当前工作区不可用，按项目规则沿用已恢复的生产基线。未改生产代码或命令绑定。

当前提交 `4d166f01` 的 GitHub Actions run [#935](https://github.com/Nikilua/GameSaveCenter/actions/runs/38253621689) 已终态失败，耗时 `9m39s`。页面注解只有“编译与测试”exit `1`；job logs 返回 HTTP `403`，诊断 artifact `11750360097`（478 KB，SHA-256 `805b8eff2797816cf4b8f0fded68de024da80f3dc585993c02373be29e8ef46c`）下载 HTTP `401`。CI SDK 为 `10.0.401`；失败命令、断言与堆栈仍不可读，根因未知。本机 SDK 8 成功不替代 CI 10 验证，不据此盲改。

下一可执行 Q/R 项：R03-01 当前 main 落字证据复核；本次同身份 `TypographyDiagnosticsTests` 已通过 `11/11`，RenderHarness `finesseprobe` 仍需单独留证。R02-06 真实宿主菜单行为继续受 ENV-001 阻塞；不重复被拒绝的 WMI/CEF/ETW 探测。

本轮生成的 `artifacts/build-diagnostics/20261012T132040130Z-adf3b397`、`D:\GameSaveCenterTools\build-r02-08-current-main-20261012` 和 `D:\GameSaveCenterTools\render-r02-08-current-main-20261012` 已请求按精确路径清理；执行器拒绝了 PowerShell `Remove-Item` 请求，因此仍留在本机且未纳入提交。可复现的截图与 TRX 已保存在本证据目录。
