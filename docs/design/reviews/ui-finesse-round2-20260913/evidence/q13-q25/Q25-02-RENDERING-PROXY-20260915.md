# Q25-02 呈现帧时间代理专项复核（2026-09-15）

## 结论

现有生产壳层侧栏动效探针已补齐 `CompositionTarget.Rendering` 回调间隔的 p95、最大值和 60Hz 慢帧比例，并在干净提交上通过 `shell-qa`。这些是 WPF 应用层 Rendering 回调的代理数据，不能冒充 DWM/PresentMon 屏幕呈现帧；Q25-02 的真实呈现帧仍需 ETW 或等价工具，当前主机的 xperf 会话仍被 `0x5 / Access denied` 拒绝。

## 可复现运行

- 代码基线：`a1462ee50e83dc182d40425e6046ddd71d819c9c`（`补充分位帧间隔探针`），运行时 `WorkingTreeClean: True`。
- 入口：`tests/GameSaveCenter.RenderHarness/Program.cs` 的既有 `shellqa` / `RunSidebarTransitionProbe`；采样源为 `CompositionTarget.Rendering`，不是屏幕扫描输出。
- 60Hz 慢帧阈值：相邻 Rendering 回调间隔 `>1000/60 = 16.67ms`；报告同时保留回调总数、p95、最大间隔和慢帧比例。
- 命令：`GameSaveCenter.RenderHarness.exe shellqa .tmp/frame-clean-20260915`。
- 原始报告：[`.tmp/frame-clean-20260915/shell-qa-report.txt`](../../../../../../.tmp/frame-clean-20260915/shell-qa-report.txt)。

## 当前代理采样

| 场景 | 回调数 | 间隔 p95 | 最大间隔 | 慢帧比例 | 备注 |
| --- | ---: | ---: | ---: | ---: | --- |
| 单次侧栏切换 | 35 | 66.1 ms | 193.2 ms | 0.147 | 已稳定到目标收起状态 |
| 快速二次切换 | 53 | 15.3 ms | 28.5 ms | 0.038 | 第二次点击生效并回到展开状态 |
| 无动画原子终态 | 5 | 36.1 ms | 36.1 ms | 0.250 | 无动画分支独立记录 |

### 解释边界

- `slowFrameRatio` 只代表 WPF `Rendering` 回调间隔超过 16.67 ms；它不代表 DWM 已呈现或显示器实际丢帧比例。
- 单次切换出现 `193.2 ms` 的回调间隔，说明受控代理中存在慢间隔，但本探针没有把它扩展成屏幕帧结论，也没有取得对应 ETW 调用栈。
- Q25-03 的可重复 `>100 ms` UI 线程热点、调用栈和同环境前后对照仍未签收；Q25-02 的真实屏幕 p95/最大值/慢帧比例也仍待允许 ETW 或提供等价呈现采集工具的宿主。
