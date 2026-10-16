# R05-03 当前 main 弹层边缘复核

日期：2026-10-13

状态：部分满足。当前 main 上生产 Picker/ComboBox 的边界、滚动和选定项可见行为通过；表格条件中的“翻转无闪屏”没有呈现帧证据，不能据几何稳定或离屏测试签收。

## 当前身份与验证

- 源码/测试 commit：`1c39e909856faa736b32224ee196ef0bac6ca460`。本轮仅复核现有实现，没有改生产 XAML、模板、Popup、滚动条或命令。
- 使用隔离 .NET SDK `8.0.423`，Release 构建插件 `net462` 和测试 `net472`；测试宿主 .NET Framework `4.8.9181.0`。
- 定向运行 `R05PopupBoundaryBehaviorTests` 与 `R05OptionVirtualizationBehaviorTests`，`7 passed / 0 failed / 0 skipped`。脱敏 TRX：[R05-03-boundary-postcommit.trx](tests/R05-03-boundary-postcommit.trx)，SHA-256 `B3BF918708BF18D6AA8B15A6DDE8962953D2E1084892CF8DA261198C93459427`。程序集身份见 [R05-01/02 当前 main 复核](../R05-01-CURRENT-MAIN-RECHECK-20261013/README.md)。

## 已验证行为

- 生产 Shell 在 720×360 DIP 短窗中加载 200 个合成游戏，选中最后一项后验证 PickerPanel 不越出 overlay 宽高、列表有 Auto 滚动、所选行位于实际列表视口；紧凑时筛选行隐藏、`Padding=8`；窗口扩到 900 DIP 后筛选行恢复，缩回 360 DIP 后再次隐藏，选中项保持可见。
- 使用生产 `GscWpfUiComboBox` 模板，在当前桌面工作区右下边缘打开 100 项 ComboBox，选中第 100 项。通过 `PointToScreen` 和当前窗口 `TransformToDevice` 换算验证 Popup 起止坐标在当前 `SystemParameters.WorkArea` 内；Popup 高度受 `MaxDropDownHeight=120` 约束，ScrollViewer 有可滚动范围且滚动条为 Auto，选定项在滚动视口可见。
- R05-02 2000 项选择/虚拟化回归一并运行 `5/5`。没有为本轮生产页面或共享模板生成新 RenderHarness 截图，因为没有布局或资源变更。

## 未验证边界

- 当前测试读取 Popup 打开后的几何，不采集呈现帧或眼见动画；“翻转无闪屏”仍未验。不能把稳定的 `PointToScreen` 坐标、RenderHarness 图片或逻辑 DPI 当作无闪烁证明。
- ComboBox 只覆盖当前桌面工作区，不覆盖真实 Playnite 嵌入、多物理屏、物理 DPI 跨屏、OS IME/UIA/Narrator、ETW 或宿主帧率/性能。720×360 测试没有重新验证历史 420×220 极端壳层；旧证据记录该尺寸约只有 92×92 DIP 的有效区，不能容纳完整选框内容。
- CI #941（commit 同为 `1c39e909`）仍在运行，当前本机 SDK `8.0.423` 测试不代表 CI SDK `10.0.401` 门禁通过。

原始实现记录：[R05-03 弹层边缘适配](../R05-03-POPUP-EDGE-20260917.md)。“翻转无闪屏”保留为待实际呈现帧验证的 R05-03 条件。

## 2026-10-13 隔离 Playnite bootstrap 预检

- 对 d56d23e 文档 HEAD 执行了既有 `scripts/real-host-audit.ps1` fail-closed 流程。启动前检查通过：没有已运行的 Playnite/Worker，进程命令行检查可用，`D:\software\Playnite\Playnite.DesktopApp.exe` 签名有效。当前只有一个 `2560×1440` 显示器（工作区 `2560×1368`）；这限制物理跨屏，不阻止本项当前屏幕边缘场景。
- Runner 在安装扩展前初始化隔离 profile 时退出码 `1`，分类为 `isolated-profile-bootstrap-failed-before-host-install`。Playnite 已在该隔离目录创建 `config.json`，但 bootstrap 未在关闭请求后 20 秒内退出，runner 按保护策略拒绝强制终止进程。原因没有被证明；当前日志里没有 `platform_channel`/access-denied 命中。100 条合成库仅被请求，未安装 seeder、未生成库，插件 Worker 未启动。
- PID `34512` 的 exe 与 `--userdatadir` 均验证属于本次 `.tmp/r05-03-popup-profile-20261013`。我请求过一次正常关闭；截至 `2026-10-13 10:35:50 UTC`，进程仍响应、主窗口句柄为 0；其进程树有 4 个使用同一隔离 profile 的 CefSharp 子进程。未强杀或重试。该隔离 profile 因进程仍在运行而保留。后续先等待该进程树退出；确认 Playnite/Worker 全部无冲突后，才可用同一 marker-bound profile 和新的唯一输出目录重跑。若仍无法优雅关闭，保留阻塞并继续其他任务，不绕过保护。
- 本次没有安装当前插件、执行弹层交互或采集屏幕呈现序列；真实 Playnite Popup 与“翻转无闪屏”仍未验。Runner 原始 blocker 与 metadata 位于本机 `artifacts/ui-host-audit-r05-03-popup-20261013/`；脱敏摘要见 [host-preflight-20261013.json](host-preflight-20261013.json)。原始输出只含本次隔离 profile 的日志，不触及用户库、真实存档、媒体或云端。
- 本记录提交时 d56d23e 对应 Actions run `38377673615` 已成功，耗时 `43m27s`。R08-01 已用 #967 SDK10 单类 TRX 校正当前 main 自动证据；用户机器的原始失败仍待提供。下一项 Round2 Q23-06 目录编辑器视觉复核，使用合成路径与隔离 RenderHarness，不依赖被阻塞的 Playnite profile。

## 2026-10-16 fail-closed 宿主预检复核

- 在当前 `main` 工作树用既有 `scripts/PlayniteHostIsolation.ps1` 执行启动前预检。冲突进程枚举没有报告 Playnite/Worker；随后 `Assert-GscProcessCommandLineInspectionAvailable` 读取 `Win32_Process.CommandLine` 时返回“拒绝访问”，安全预检因此 fail closed。权限拒绝意味着无法据这次枚举证明宿主进程完整不存在。
- 本次没有启动 Playnite/Worker、安装扩展、创建新隔离 profile/output、重试相同 bootstrap、强终止进程或绕过 WMI 权限。10-13 留下的隔离 profile 未触碰；本轮没有生成 artifact 或呈现帧。
- R05-03 现有 Popup 工作区几何、列表边界、滚动及选定项测试仍通过；“翻转无闪屏”没有真实呈现帧，本次预检也没有推进该条件。ENV-001 `BLOCKED_ENVIRONMENT` 保持。下一步转 Q24-02 证据校正；如果之后权限/宿主环境变化，再按原保护流程复核，不从离屏图推断真实呈现。
