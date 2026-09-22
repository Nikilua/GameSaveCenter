# R12-03 目标路径核对证据

日期：2026-09-19。实现分支：`codex/ui-finesse-round2`。实现提交：`c0adb1b7`。

## 完成范围

- 先核对并复用已有 `PathRemapService.PreviewAsync`、路径预览 DTO、IPC 和设置写回能力，没有新增文件移动或删除链路。
- Maintenance 页面现在以对照表展示原路径、重映射路径和目标状态；完整路径保留在只读 TextBox 中，可直接选择复制。旧根或新根改变时清除旧预览，避免展示过期目标。
- 预览表限制 `MaxHeight=260`，标记 `FiniteViewport`，启用 Recycling 行/列虚拟化；说明文字明确确认只会写索引与 Worker 设置，不会写用户目录、移动文件或删除文件。

## 行为与构建证据

- Worker `PathRemapServiceTests`：`4/4`。合成长路径、不同盘符、相似旧根负例和目标目录未写入均通过；隔离 SQLite 内容保持不变。
- Playnite `R12PathRemapBehaviorTests`：`2/2`。验证 DTO 完整文本/目标状态，以及真实 Maintenance XAML 的绑定、有限视口、虚拟化、只读复制单元格和无文件移动调用。
- `UiFinesseRound2ControlSourceTests`：`24/24`。其中包含当前续接分支对危险确认框初始焦点源码契约的校正；这组证据不等于 main 已完成合并。
- `WpfUiResourceDictionaryTests`：`137 passed / 39 skipped / 0 failed`，总计 `176`。
- 完整隔离 `scripts/build.ps1 -Configuration Release -OutputRoot .tmp/r12-03-full-build`：XAML `24/24`；Release solution `0 warning / 0 error`；Core `85/85`；Worker `325/325`；Playnite source `68` 类、WPF `84` 类逐类隔离进程全部返回 `0`，脚本最终输出 `All Playnite tests passed with WPF classes isolated by process.`
- 最终 `python scripts/validate-source.py`、`scripts/check-xaml.ps1`、`git diff --check` 通过。WPF 静态审查为 `0 error / 25 warning / 177 info`；warning/info 是既有外层布局、Canvas 和颜色令牌提示，没有新增 error。

## 边界与主线集成事实

- 业务验证只使用合成长路径、fake 服务、隔离目录和隔离 SQLite；没有写真实存档、媒体、用户目录、云端或外发诊断。完整路径可读性由 DTO/行为测试证明，不冒充物理剪贴板验证。
- 证据来自隔离 STA WPF/offscreen logical DIP 和源码/构建门禁，不等于真实 Playnite/package-host 呈现、物理 DPI/跨屏、presented frame、UIA/IME、ETW 或宿主性能通过。Demo 原目录不可用，本轮沿用恢复生产基线。
- 用户在 main 上运行 `GameSaveCenter-一键构建安装运行.cmd` 的最新 `DEV-INSTALL-008` 日志在安装前失败：Playnite 源码组 `273 passed / 18 skipped / 1 failed / 292 total`，失败为 `UiFinesseRound2ControlSourceTests.DangerousConfirmationKeepsCancelAsTheInitialFocusTarget`。main dirty `DashboardView.xaml.cs` 已包含 `!dialogLifecycle.IsClosing`，但 main 跟踪测试仍期待旧的 `if (IsLoaded && DialogOverlay.Visibility == Visibility.Visible) initialFocus.Focus();` 字符串；本分支对应测试已通过。
- main 当前的 R08 用户改动、`src.zip` 及未跟踪 R08 文件均未覆盖或清理。本阶段只推送续接分支，未宣称 main 一键安装通过；需要在 main 用户 R08 变更形成可审阅提交后再做安全集成。

## 下一步

下一可执行小批量为 `R12-04 恢复保护备份`：继续先查已有元数据备份/回滚能力，再补独立的成功与失败负例。main 的焦点源断言集成仍是独立边界，不以路径预览证据替代解决。
