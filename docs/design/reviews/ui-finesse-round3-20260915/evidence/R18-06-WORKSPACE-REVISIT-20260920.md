# R18-06 页面重访成本证据（2026-09-20）

## 结论

R18-06 在 `1b19fd4c` 已满足受控验证条件，真实宿主耗时仍待验证：生产壳层已有的页面实例注册表继续复用六个页面；本阶段为页面级读取增加了有界热态重访门，并为加载、绑定和布局分别留下来源明确的耗时日志。

- `WorkspaceRevisitLoadGateTests` 与 `R18WorkspaceRevisitSourceTests`：`7/7`。
- 页面生命周期、状态 presenter、请求协调和忙状态相邻回归：`31/31`。
- `python scripts/validate-source.py`：通过；XAML 结构门禁：`24/24`；`git diff --check`：通过。
- Release 隔离 solution：`0 errors / 2 warnings`；两条均为既有 `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs:671` nullable warning；本阶段 `.tmp/r18-06-*` 输出已清理。

## 实现与行为

### 页面实例与绑定/布局

- `AcrylicProductionShellView.CreatePages()` 只创建一次 Overview、Saves、Trainers、Media、Tasks、Maintenance 页面，并把同一个 Dashboard VM 绑定到页面实例。
- `NavigateTo` 只有在 `PageHost.Content` 不是目标实例时才重新赋值；同页重访保持原视觉树和嵌套滚动 owner，不重复绑定或内容转场。
- 新增 `[PERF] WorkspacePages` 与 `[PERF] WorkspaceActivation` 日志：分别标识一次页面创建/绑定耗时，以及 workspace 的 `first/revisit`、`attach/reuse` 和同步 layout 耗时。日志不把 WPF 同步布局时间写成 presented frame。

### 页面读取与失效边界

- `RequestWorkspaceLoad` 仍只调用既有的 Media、Maintenance 或选中游戏详情读取，不触发整库 `RefreshDashboard` 或 `Synchronize`；显式 `LoadDetailsCommand` 继续绕过重访门执行强制读取。
- `WorkspaceRevisitLoadGate` 以 `15` 秒为热态窗口，key 按页面和数据上下文隔离：Saves/Trainers 纳入 `PlayniteId`；Media 纳入 `PlayniteId`、媒体筛选、搜索和收件箱模式；Maintenance 单独成 key。不同游戏或筛选不会复用上一个上下文的新鲜度。
- 成功才记录新鲜度；失败、取消、上下文切换和 Dashboard 卸载的 `CancelDeferredUiWork` 不生成新鲜缓存。已失效请求晚返回时，门不允许它重新发布成功时间戳。
- 既有 `WorkspaceDataState`、`LastSuccessUtc`、Stale/Error/Offline presenter 和 generation/cancellation 保护保持不变，因此热态跳过不会把旧对象重新标成当前确认结果；显式失败仍沿用既有 stale/error 语义。

## 原始受控样本

| 场景 | 结果 |
|---|---:|
| 同一页面/上下文在 `14s` 内重访 | 跳过读取 |
| 同一页面/上下文到 `15s` | 允许重新读取 |
| 不同游戏上下文 | 立即允许读取 |
| 失败或取消后再次请求 | 不继承新鲜度 |
| 关闭/失效后旧请求晚返回 | 不发布新鲜度 |
| 同一上下文并发重复请求 | 第二次被拒绝 |

## 门禁与未验边界

- 测试使用确定时间、合成上下文 key、源码契约和隔离 net472/WPF testhost；没有修改真实存档、媒体、用户云端、诊断目录或 Playnite 用户数据。
- 日志是生产代码内的 Stopwatch/同步壳层记录，`WorkspaceLoad` 覆盖实际页面读取委托的耗时，`WorkspaceActivation` 覆盖页面绑定/Content 复用/同步布局路径；本轮没有运行真实 Playnite 宿主并采集一组可代表用户机器的首次/重访数值，因此不宣称真实宿主、DWM、presented frame、物理 DPI/跨屏、60fps、UIA/读屏、ETW 或宿主性能。
- Demo 原目录不可用；本轮没有改变 Demo 视觉体系、游戏选框、滚动条、命令绑定、取消/错误/恢复保护或 Playnite `net462` 契约。
- 下一可执行任务：`R18-07 长时资源曲线`，先核对既有耐久脚本和可观测句柄/订阅/时钟/工作集/缓存指标，再决定可在隔离宿主执行的范围。
