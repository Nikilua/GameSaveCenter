# R23-05 帧性能证据闭环

日期：2026-09-22  
分支：`codex/ui-finesse-round2`  
探针构建身份：`130ba48ae287f029cf180d15784d21f3225ba8fe`
任务：R23-05「帧性能证据闭环」

## 结论

本批先复核了上一版 `shellqa` 的 5 个真实几何失败，再修复共享壳层阈值和 Media Inbox footer 行位；当前提交重新运行 `shellqa` 已通过。Rendering/Stopwatch 仍只是应用层代理，本批不宣称真实显示器呈现帧通过。

- Rendering 回调：已取得，来源是 WPF `CompositionTarget.Rendering`，记录回调数、`Stopwatch` 间隔、p95、最大间隔和超过 60Hz 阈值的比例。
- 几何门禁：已修复并通过；Shell 头部在 980/1040 宽度进入显式第二行，Media 1040/1100/1366 的 `gridTopGap` 均为 `142 DIP`。
- Stopwatch：已取得，记录受控侧栏切换的持续时间、布局/测量/排列次数和间隔样本。
- 真实呈现帧：未取得。当前数据是 offscreen logical DIP / 受控 WPF 窗口代理，不是 DWM、PresentMon 或 ETW 的屏幕扫描输出。
- ETW 边界：沿用 Q25 的真实事实，`xperf`/WPR 系统跟踪曾被权限策略拒绝；没有绕过拒绝，也没有虚构 ETL、调用栈或显示器丢帧率。

## 当前提交与运行

在 D: 隔离源码副本中构建 `tests/GameSaveCenter.RenderHarness/GameSaveCenter.RenderHarness.csproj`，并在提交后的 clean tree 运行：

```text
GameSaveCenter.RenderHarness.exe shellqa D:\gsc-r23-05-shellqa-clean-20260922
```

报告记录：

- `EvidenceSource=OffscreenRenderHarness`。
- `Commit=130ba48ae287f029cf180d15784d21f3225ba8fe`。
- `WorkingTreeClean=True`。
- Light/Dark 主题、合成生产壳层夹具、`DpiScale=1.00` logical DIP；真实宿主 DPI 未由该探针推断。
- XAML `24/24`；solution Release 构建 `0 errors`，仅既有 `MediaCenterView.xaml.cs:699` 两条 `CS8602` warning；响应式边界 `5/5`、Media 动作/筛选回归 `6/6`、`WpfUiResourceDictionaryTests 137 passed / 39 skipped / 0 failed`。
- source validation、XAML 检查和 `git diff --check` 已通过。

原始报告：`D:\gsc-r23-05-shellqa-clean-20260922\shell-qa-report.txt`。D: 临时目录只用于本批复核，文档提交前按规则清理；仓库不提交截图或构建物。

## 几何修复结果

- `ResponsiveLayoutCoordinator.IsCompactShellHeader` 从仅覆盖 `<980` 调整为覆盖 `<1280`；不改变 960/1040/1280 的页面 `LayoutMode`，只让头部动作在内容预算不足时进入现有第二行布局。响应式边界测试同步覆盖 980、1040、1200、1279 与 1280 的正负边界。
- Media Inbox 保留原批量选择行、DataGrid、页面级 ScrollViewer、选框、命令和绑定；将筛选预设与可用性提示移到既有 `MediaInboxFooter` 的前两行，加载摘要、次级动作和失败带顺延，不隐藏或删除入口。
- clean `shellqa` 结果：Shell `720/960/980/1040` 均无头部越界；Media `1040/1100/1366` 均为 `gridTopGap=142 DIP`；三种尺寸的 footer、历史和次级动作在页面末端均位于 viewport 内；Light/Dark 共用同一生产几何探针并通过。

## Rendering 代理样本

报告中的侧栏切换样本如下：

| 场景 | Rendering 回调 | 间隔 p95 | 最大间隔 | 慢帧比例（>16.67ms） | 终态 |
| --- | ---: | ---: | ---: | ---: | --- |
| 单次切换 | 27 | 97.5ms | 213.8ms | 0.154 | 收起，稳定 |
| 快速二次切换 | 44 | 16.3ms | 28.2ms | 0.047 | 展开，稳定 |
| 无动画原子终态 | 4 | 29.0ms | 29.0ms | 0.333 | 收起，稳定 |

对应 Stopwatch 持续时间为 `540.9ms`、`459.4ms`、`55.1ms`。这些数字只描述受控 WPF 窗口收到 Rendering 回调时的间隔和探针动作时间；不能转换为“显示器实际丢了多少帧”。

## 修复前基线

修复前 `fef68005` 报告列出 5 个几何问题：

1. `Shell 980x640` header actions 超出 `HeaderSurface` 边界。
2. `Shell 1040x700` header actions 超出 `HeaderSurface` 边界。
3. Media `1040x700` inbox grid 与 batch row 的顶部间距为 `233 DIP`。
4. Media `1100x720` inbox grid 与 batch row 的顶部间距为 `233 DIP`。
5. Media `1366x768` inbox grid 与 batch row 的顶部间距为 `269 DIP`。

以上 5 项已由当前 `130ba48a` 的 clean `shellqa` 复核收口；它们不再作为当前失败门禁。R23-05 的性能数据仍保留为“应用层代理已取得”，真实 presented frame/宿主性能不因几何通过而自动完成。

## 证据分账与未验边界

| 证据层 | 本批结果 | 可以说明什么 | 不能说明什么 |
| --- | --- | --- | --- |
| WPF `CompositionTarget.Rendering` | 已运行 | 应用层回调间隔代理 | DWM/显示器呈现帧、真实刷新率或物理掉帧 |
| `Stopwatch` | 已运行 | 受控动作/布局测量的单调耗时 | Playnite 宿主真实输入到屏幕的端到端延迟 |
| 离屏截图/逻辑 DIP | 同次生成 | 受控布局和几何报告的样本 | 真实宿主 DPI、物理跨屏、presented frame |
| ETW/PresentMon | 未取得 | — | p95/最大真实呈现间隔、系统调用栈、GPU/DWM 结论 |

Demo 原目录不可用，继续参考已恢复生产基线；本批没有读取或写入真实存档、删除真实媒体、写用户云端或外发诊断。游戏选框、滚动条系统、命令绑定、取消/错误语义、恢复保护、有限列表性能和 Playnite/net462 未改。

下一可执行任务：推进 R23-06 当前候选安装与回退身份核查；R23-04 UIA/Controlled host 仍按真实宿主边界推进，真实呈现帧仍需获得系统允许的 ETW/PresentMon 等价工具后另行复测。
