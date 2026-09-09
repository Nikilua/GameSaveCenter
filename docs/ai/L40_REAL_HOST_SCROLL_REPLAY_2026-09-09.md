# L40 真实 Playnite 回放复核与证据边界

日期：2026-09-09  
代码提交：`c84107b`  
构建身份：`0.6.73+c84107b2f79870133707ed01abd22c521e87e070`  
证据目录：[`artifacts/ui-host-audit-isolated-l40`](../../artifacts/ui-host-audit-isolated-l40)

## 本轮实际执行

- 使用隔离 Playnite 数据目录启动 `D:\software\Playnite\Playnite.DesktopApp.exe`，安装 c84107b 包和对应 Worker；没有修改用户 FusionX 文件、Playnite 全局样式或正常用户数据目录。
- 发现复制的隔离配置仍指向上一轮 L36 Worker 绝对路径，先在隔离目录内修正为 L40 Worker 路径，再重新启动。日志确认 Worker 来自本轮 L40 安装目录，避免把旧包身份冲突误判为表格问题。
- L40 的最终 `summary.json` 为：`EmbeddedDashboardCaptured=false`、`EmbeddedSettingsCaptured=true`、`ControlledDashboardCaptured=true`、`ProductionVisualSourceOfTruthAvailable=false`，并生成 `REAL_EMBEDDED_DASHBOARD_NOT_CAPTURED` 门禁。
- 因真实嵌入 Dashboard 没有被打开，本轮没有生成新的 `scroll-replay/*/replay.json`。受控专用窗口的截图不能冒充真实宿主表格回放，故不把 L40 记作滚动通过。

## 与滚动条自动化路径相关的结果

- L40 前一轮 b100913 运行时曾在真实 DataGrid 的 WPF `RangeValueProvider.SetValue` 内抛出 `NullReferenceException`，位置在开发审计的“滑块等效”路径；这不是生产 DataGrid 运行路径，也不是表格正文异常的证据。
- c84107b 将该开发审计调用包在异常隔离中：RangeValue 不可用时记录“滑块等效不可用”并继续审计，不再把宿主 UI Automation provider 的异常扩大成整轮回放失败。该变更只影响开发审计，不改变生产滚动、绑定、选择或虚拟化。
- 由于 L40 没有真实嵌入 Dashboard，本次没有宣称 c84107b 已完成物理滑块验证；仍不能替代鼠标拖动或视频录屏。

## 当前有效的真实嵌入滚动证据

L39 的真实 `EmbeddedPlaynite` 回放仍是当前可复核的宿主端点证据，详见 [`L39_REAL_HOST_SCROLL_REPLAY_2026-09-09.md`](L39_REAL_HOST_SCROLL_REPLAY_2026-09-09.md)：

- `MediaInboxGrid` 400 条，底部 `394/394`，实际 Presenter `0,36,604×279.33`，末项 `399@256..300`，cell/visual/text 为 `5/5/5`。
- `TaskGrid` 50 条，底部 `40/40`，实际 Presenter `0,36,637.33×456`，末项 `49@432..476`，cell/visual/text 为 `6/6/6`。
- 两表各 47 个样本、底部各 21 个、上下端点往返 20 次；空正文、大块间隙、末端水平条覆盖、选中内容缺失和尾项不完整均为 `0`。
- 该回放使用真实负责行滚动的 `ScrollViewer`，并保留 `CanContentScroll=True`、`ScrollUnit=Item`、行列虚拟化和 FusionX 宿主模板链。

## 回归与结论

- Release 构建：`0 warning / 0 error`。
- Core：`76/76`；Worker：`310/311`（`1` skip）；Playnite：`425/488`（`63` skip）。
- 源码校验、XAML 校验和滚动审计契约测试均通过；没有关闭虚拟化、加入固定底部补偿、刷新 Items、定时布局或强制回顶。
- 当前仍不能写“视频问题已解决”：L39 只证明真实宿主程序化端点和末项视口正确，L40 还证明了真实嵌入捕获缺失；物理滑块、滚轮、PageUp/PageDown、Ctrl+End、主题/DPI 矩阵和视频录屏继续标记为“宿主人工待验收”。
