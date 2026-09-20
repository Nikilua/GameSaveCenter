# R18-02 真实 Dispatcher 基准（2026-09-20）

## 结论

R18-02 在当前分支 `codex/ui-finesse-round2` 的实现提交 `59468b37`、来源标注校正 `5b28b0c3` 已满足受控完成条件：在真实 STA WPF `Window` 中，把本地筛选完成与窗口内第一个可见列表容器分成两段测量。测试使用生产 `GamePickerViewModel` 的内部刷新诊断记录 VM 完成点，再用 `ListBox.ItemContainerGenerator.ContainerFromIndex(0)`、`IsVisible`、`ActualWidth/ActualHeight` 和 `UpdateLayout()` 作为可见反馈来源；没有使用 `FilteredCount` 代替画面延迟。

## 受控窗口结果

20 次连续查询均完成两段测量：

| 阶段 | p95 | 最大值 | 测量来源 |
| --- | ---: | ---: | --- |
| SearchText 输入 → VM `RefreshCount` 增长且 `LastSearchText` 对应 | 52.272 ms | 63.581 ms | `GamePickerPerformanceDiagnostics`，实际 Dispatcher 投递后的 `ApplyViewRefresh` |
| VM 完成 → 可见容器条件成立 | 28.343 ms | 49.942 ms | 受控窗口 `ListBox` 的容器可见性、宽高和布局更新 |

每次样本的 `visible_counts` 均为 `1`，首个容器的实测几何均为 `476 × 19.24 DIP`。测试 `R18DispatcherVisibilityBenchmarkTests` 最新标注修复后 `1/1`；同一实现逻辑的 R18/游戏选框/键盘/IME/防抖合并回归为 `47/47`。

## 原始样本（可复算）

```text
vm_ms=63.581,32.244,33.822,33.555,31.752,33.178,32.164,33.779,48.597,32.41,51.301,33.037,32.06,32.76,32.52,33.513,40.642,52.272,45.127,33.658
visible_feedback_ms=49.942,11.71,12.486,12.847,12.844,12.221,12.825,11.302,28.343,27.8,24.489,12.612,12.326,12.686,12.366,11.873,19.73,23.326,15.048,12.412
visible_counts=1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1
container_heights=19.24,19.24,19.24,19.24,19.24,19.24,19.24,19.24,19.24,19.24,19.24,19.24,19.24,19.24,19.24,19.24,19.24,19.24,19.24,19.24
container_widths=476,476,476,476,476,476,476,476,476,476,476,476,476,476,476,476,476,476,476,476
```

VM 时间从设置一个不同的搜索文本开始，到诊断刷新计数增加且 `LastSearchText` 等于该查询结束；可见反馈时间是同一 `Stopwatch` 中减去 VM 阶段后的增量。每次可见条件都在一个实际 `Window`（`Show()`、`IsVisible`）内检查，窗口使用 `Opacity=0.01` 保持受控、不作为用户屏幕截图证据。夹具显式安装 `DispatcherSynchronizationContext`；首轮未安装时真实捕获了刷新没有按宿主 Dispatcher 收敛的负例，修正后才计入本结果。

## 门禁与边界

- R18-02 最新测试 `1/1`；同一实现逻辑的 R18/游戏选框/键盘/IME/防抖合并回归 `47/47`。
- `scripts/validate-source.py`：通过；`scripts/check-xaml.ps1`：`24/24`；`git diff --check`：通过。
- Release 隔离 solution（实现提交 `59468b37`，来源字符串校正不改变生产代码）：`0 errors / 2 existing warnings`，警告为既有 `MediaCenterView.xaml.cs:664` nullable；Playnite 目标 `net462`。`.tmp/r18-02-solution` 已清理。
- 本批只新增测试夹具和证据输出，没有改 XAML、主题资源、生产视觉树或业务命令；WPF 质量基线沿用当前静态 `0 errors / 27 warnings / 162 info`。
- 这是 Dispatcher/布局可见性证据，不是 Windows DWM presented frame、物理屏幕帧、60fps、Playnite 嵌入宿主、物理 DPI/跨屏、UIA/读屏、真实 OS IME 或 ETW/分配性能证据。未绕过被拒绝的系统跟踪权限。
- 只使用合成 DTO、隔离 STA testhost 和受控窗口；未读写真实存档、媒体、云端或诊断目录。Demo 原目录不可用，继续使用恢复生产基线。

下一可执行小批量：R18-03“缩略图滚动预算”，先核对 `AsyncThumbnailLoader` 的并发请求、取消、缓存上限和迟到结果保护，再决定是否需要代码。
