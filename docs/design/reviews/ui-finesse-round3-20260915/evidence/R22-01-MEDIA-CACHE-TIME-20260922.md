# R22-01 媒体缓存时间显示证据

日期：2026-09-22  
代码提交：`48db9ee5`、`2484f132`、`b1afb004`  
范围：仅收口 `DashboardViewModel.WorkspaceStates` 的媒体收件箱缓存标题，不扩展到报告、复制列、日志或其他完整时间入口。

## 本批事实

- `MediaInboxCountCaption` 复用 `TimeDisplayFormatter.Relative`，离线缓存与 stale 缓存均显示相对时间，不再直接拼接 `ToLocalTime():MM-dd HH:mm`。
- 新增 `MediaInboxCountCaptionFull`，由媒体概览摘要的 `ToolTip` 和 `AutomationProperties.HelpText` 提供完整本地时间、UTC 偏移和原始 UTC，保留可复制/回读的精确时间入口。
- 无当前上下文成功时间时仍显示“离线 · 无法读取”或“无法读取 · 尚未确认数量”，不会生成假缓存时间；加载、错误、正常状态文案和既有数量绑定保持不变。
- RenderHarness fake 与字段清单同步 `MediaInboxCountCaptionFull`，避免生产绑定新增后夹具漏字段。

## 行为验证

- `MediaWorkspaceStateCacheTests`：最终提交 `b1afb004` 下 `7/7`，覆盖离线缓存相对/完整时间、stale 缓存相对/完整时间，以及无成功时间负例。
- `WorkspaceStateSourceTests`：提交 `2484f132` 下 `9 passed / 1 skipped / 0 failed`；唯一 skip 为既有共享 presenter 环境项。覆盖生产状态源与 RenderHarness 字段契约。
- 以固定 `2026-09-22 08:00 UTC`、参考时间 `08:02 UTC` 验证“2 分钟前”；完整投影必须包含 `TimeDisplayFormatter.Full`，未知状态不得出现“缓存于”或“上次成功”。

## 构建与门禁

- `scripts/build.ps1 -Configuration Release -SkipTests`，隔离输出 `.tmp/r22-01-media-cache-build-20260922`：Contracts/Core/Worker/Playnite/Tests 成功，`0 error`；仅保留既有 `MediaCenterView.xaml.cs:699` 两条 `CS8602` warning。
- `scripts/check-xaml.ps1`：`24/24`。
- `python scripts/validate-source.py`：通过。
- `validate_wpf_ui.py .`：`0 error / 30 warning / 177 info`。warning 为既有滚动容器审查提示；本批没有新增 error。
- 普通 `dotnet test` 未设置 `GSC_BUILD_COMMIT` 时被 Playnite 源码身份门拒绝；按项目既有隔离协议设置提交身份并以单节点运行后通过，未放宽身份门禁。

## 边界

本批只使用合成 UTC、fake RenderHarness 和隔离 Release 输出，没有读写真实存档、媒体、云端或用户诊断，也未发送外部诊断。未运行真实 Playnite/package-host、Windows UIA/读屏、OS 输入/IME、物理 DPI/跨屏、最终呈现帧、ETW 或宿主性能；Demo 原目录不可用，视觉依据继续使用恢复生产基线与 Demo-first 资源链。下一步继续核对 `DashboardViewModel` 其余 stale/缓存 `ToLocalTime` 用户入口。
