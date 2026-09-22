# R18-07 长时资源曲线证据

日期：2026-09-20  
任务：`R18-07` 长时资源曲线  
代码：`56d8e1b7`（探针资源字段）+ `c5c33e18`（停止输入后的 30 秒静置观察）  
原始报告：[`enduranceprobe-report.txt`](../../../../../../.tmp/r18-07-endurance-final/enduranceprobe-report.txt)

## 结论

R18-07 在当前可控范围内已满足：既有 `RunEnduranceProbe` 被扩展为保留原始时间序列，并同时记录托管堆、私有字节、工作集、线程、句柄、探针可见的活动计时器、反射可见的托管事件委托、动画属性持有者和缩略图缓存诊断；达到 30 分钟操作目标后停止输入，再观察 30 秒，确认活动计数停止且资源曲线回落或稳定。

这不是真实 Playnite 宿主、用户屏幕呈现、物理 DPI/跨屏、UIA/读屏、ETW 或数学意义上的无界泄漏证明。报告生成时工作树包含尚未提交的静置观察改动，元数据因此为 `WorkingTreeClean: False`；随后该改动已由 `c5c33e18` 独立提交固化。原始报告未被改写。

## 受控运行

- `DurationTargetSeconds=1800`，`PostActionSettleSeconds=30`，实际 `duration_s=1830.2/1830`。
- 隔离 WPF STA Window，`Opacity=0.01`，`1040×700 DIP`，Light/Dark，六个工作区，样本间隔约 10 秒；场景包括页面导航、Media 预览、选中详情/检查器和主题切换。
- `cycles=2769`，`completedActions=8537`，`samples=177`，`actionFailures=0`，最终 `enduranceprobe OK`。
- 原始样本全部保留在报告中；没有强制 GC。`DpiScale=1.00` 是离屏逻辑 DIP，不能推导真实宿主 DPI。

## 资源序列摘要

| 字段 | 全程范围 | 首样本 | 最终样本 | 解释 |
| --- | ---: | ---: | ---: | --- |
| `managed` | 23,940,032–48,872,096 | 23,940,032 | 36,684,488 | `GC.GetTotalMemory(false)`，托管堆代理 |
| `private` | 162,054,144–342,069,248 | 162,054,144 | 268,775,424 | 进程私有字节 |
| `workingSet` | 183,939,072–384,303,104 | 183,939,072 | 298,184,704 | 进程工作集 |
| `threads` | 22–42 | 22 | 22 | 进程线程数 |
| `handles` | 1,170–1,547 | 1,170 | 1,521 | 进程句柄数 |
| `timers` | 0–1 | 0 | 0 | 探针实例字段及动作计时器 |
| `subscriptions` | 1 | 1 | 1 | 反射可见托管事件委托；不是全局 WPF 订阅总数 |
| `animated_owners` | 0 | 0 | 0 | `HasAnimatedProperties` 的 UIElement 代理，不是精确动画时钟数 |
| `thumb_cache` | 0/96 | 0/96 | 0/96 | `AsyncThumbnailLoader` 缓存诊断 |
| `thumb_active` | 0 | 0 | 0 | 活动缩略图解码数 |

报告摘要还给出：私有字节首/尾窗口均值 `299,524,915/306,700,288`，有限窗口斜率 `145,747.3 bytes/min`；托管堆首/尾窗口均值 `37,308,647/38,072,986`，有限窗口斜率 `28,152.48 bytes/min`。这些是本次受控窗口的趋势字段，不是跨运行或无界增长结论。

## 停止输入后的证据

达到 1800 秒目标后动作计时器停止，周期计数不再增加。静置尾段原始样本如下：

| elapsed | cycle | private | working set | managed | threads | handles | timers | final |
| ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| 1802.1s | 2769 | 316,137,472 | 345,202,688 | 34,418,208 | 32 | 1,525 | 0 | 否 |
| 1813.1s | 2769 | 269,156,352 | 298,373,120 | 35,448,976 | 30 | 1,526 | 0 | 否 |
| 1823.2s | 2769 | 268,775,424 | 298,184,704 | 36,070,600 | 22 | 1,521 | 0 | 否 |
| 1830.2s | 2769 | 268,775,424 | 298,184,704 | 36,684,488 | 22 | 1,521 | 0 | 是 |

`subscriptions=1` 在静置段保持不变，`thumb_cache=0/96` 与 `thumb_active=0` 也保持不变。该场景没有实际触发缩略图解码；缩略图解码/缓存上限应引用 R18-03 证据，不能从本次 endurance 样本推导。

## 质量与边界

- 相关 Release 构建通过，`0 errors`，仅保留既有 `MediaCenterView.xaml.cs:671` 的 2 条 nullable warning。
- `UiFinesseRound2ControlSourceTests`、`WorkspaceRevisitLoadGateTests`、`R18WorkspaceRevisitSourceTests` 定向测试 `31/31`；`validate-source.py` 与 `git diff --check` 通过。
- 探针使用合成数据、隔离 Window 和现有 fake/loader 诊断，不读取或修改真实存档、媒体、用户云端或外部诊断系统；没有绕过 ETW/系统跟踪权限。
- 保留现有游戏选框、滚动条、命令绑定、取消/错误语义、恢复保护、有限列表性能和 Playnite/net462 兼容路径；本项没有替换视觉体系或新增真实业务写入。
- Demo 原目录不可用，视觉基准继续沿用恢复的生产基线。未执行 Playnite package-host、真实长时输入、真实屏幕帧、物理跨屏、UIA/读屏或 ETW 验证。

下一可执行任务：`R18-08 低性能降级触发`。先盘点现有无玻璃/无动画回退、渲染 Tier 和低性能探针，再在真实低 Tier 或明确模拟条件下验证功能与文本可读性不降级。
