# R00/R01 动效探针时序复核（2026-09-23）

## 结论

本批修复的是 RenderHarness 的采样夹具，不是生产动效实现：

- reentry 探针在建立深色主题资源后先完成响应式布局，再等待有界的活动中间态；
- hot-change 探针保留 `700 ms` 审计时钟，只给首个渲染片留出有限沉降窗口；
- reentry 完成态等待 `270 DIP`、无活动时钟、位移归零三项同时满足，并保留超时失败；
- 输出增加实际观察值，便于区分“生产状态失败”和“Dispatcher 采样过早”。

没有修改生产 XAML、`GscMotion`、命令、Binding、游戏选框、滚动条、取消/错误/恢复保护或 Playnite `net462` 路径。当前代码身份为 `0e468873d8f851a39c4a04672a78628b51d78bb1`。

## 实际验证

- RenderHarness Release 构建：`0 error`；Playnite 目标 `net462`；保留 `MediaCenterView.xaml.cs:706` 两条既有 `CS8602` warning。
- `motionprobe`：Light/Dark 均观察到活动中间帧，终态 `72 DIP`、`Opacity=1`、`X=0`、无活动动画时钟，exit `0`。
- `motionreentryprobe`：Light `229.91 → 257.43 → 270 DIP`，Dark `238.40 → 267.92 → 270 DIP`；两主题立即接管当前渲染宽度，终态 `finalAnimated=False`，exit `0`。
- `motionhotprobe`：Light/Dark 均 `duringAnimated=True`；关闭动效后 `72 DIP`、`Opacity=1`、`X=0`，禁用重入立即回到 `270 DIP`，exit `0`。
- `motioncycleprobe`：Light/Dark 各 `100` 次 Loaded/Unloaded，事件计数 `101/101`，无残留过渡或动画时钟，exit `0`。
- `UiFinesseFoundationTests`：`9/9` 通过，包含完成态清钟、当前渲染值重入、reduced-motion 取消和动效资源合同。

这些是同一生产壳层的受控 STA WPF Window、Light/Dark、offscreen logical DIP 证据，不是 `Assert.Contains` 形式的字符串签收；门禁仍会在有界时间内报告活动态或完成态缺失。

## 边界

未把受控窗口、离屏截图或 Dispatcher 探针升级为真实 Playnite 输入、Windows 偏好通知、UIA/读屏、物理 DPI/跨屏、presented frame、ETW 或宿主性能通过。未读写真实存档、媒体、云端或外发诊断；Demo 原目录不可用，继续沿用恢复生产基线。

## 下一项

R00-03 与 R01-04 的当前受控动效证据按“已满足”收口；下一可执行项仍为 R23-04 正常可枚举 Playnite 主窗体/UIA 会话，若 CEF/窗口暴露继续阻塞则推进依赖已满足的独立 Q/R 小批量。
