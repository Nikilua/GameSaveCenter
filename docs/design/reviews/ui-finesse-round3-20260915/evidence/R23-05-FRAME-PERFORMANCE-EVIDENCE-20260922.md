# R23-05 帧性能证据闭环

日期：2026-09-22  
分支：`codex/ui-finesse-round2`  
探针构建身份：`fef68005d86c5daa801850afa04d01b3eb113209`  
任务：R23-05「帧性能证据闭环」

## 结论

现有 `shellqa` 已在当前提交重新运行并取得 Rendering 回调代理样本；本批不宣称真实显示器呈现帧通过。探针最终为 `shell-qa FAILED`，失败来自同一报告中的 5 个几何门禁，不应被性能数字掩盖。

- Rendering 回调：已取得，来源是 WPF `CompositionTarget.Rendering`，记录回调数、`Stopwatch` 间隔、p95、最大间隔和超过 60Hz 阈值的比例。
- Stopwatch：已取得，记录受控侧栏切换的持续时间、布局/测量/排列次数和间隔样本。
- 真实呈现帧：未取得。当前数据是 offscreen logical DIP / 受控 WPF 窗口代理，不是 DWM、PresentMon 或 ETW 的屏幕扫描输出。
- ETW 边界：沿用 Q25 的真实事实，`xperf`/WPR 系统跟踪曾被权限策略拒绝；没有绕过拒绝，也没有虚构 ETL、调用栈或显示器丢帧率。

## 当前提交与运行

单独构建 `tests/GameSaveCenter.RenderHarness/GameSaveCenter.RenderHarness.csproj` 到 `D:\gsc-r23-05-build-20260922` 后运行：

```text
GameSaveCenter.RenderHarness.exe shellqa artifacts/r23-05-shellqa-20260922
```

报告记录：

- `EvidenceSource=OffscreenRenderHarness`。
- `Commit=fef68005d86c5daa801850afa04d01b3eb113209`。
- `WorkingTreeClean=True`。
- Light/Dark 主题、合成生产壳层夹具、`DpiScale=1.00` logical DIP；真实宿主 DPI 未由该探针推断。
- Release 构建 `0 errors`，仅既有 `MediaCenterView.xaml.cs:699` 两条 `CS8602` warning。
- XAML 结构 `24/24`；source validation、XAML 检查和 `git diff --check` 已通过。

原始报告：`artifacts/r23-05-shellqa-20260922/shell-qa-report.txt`。同目录保留该次探针生成的截图和主题/页面夹具，直到本批文档完成引用后再按清理规则处理。

## Rendering 代理样本

报告中的侧栏切换样本如下：

| 场景 | Rendering 回调 | 间隔 p95 | 最大间隔 | 慢帧比例（>16.67ms） | 终态 |
| --- | ---: | ---: | ---: | ---: | --- |
| 单次切换 | 29 | 74.5ms | 234.6ms | 0.143 | 收起，稳定 |
| 快速二次切换 | 41 | 18.5ms | 35.2ms | 0.100 | 展开，稳定 |
| 无动画原子终态 | 4 | 21.4ms | 21.4ms | 0.333 | 收起，稳定 |

对应 Stopwatch 持续时间为 `522.6ms`、`468.8ms`、`53.9ms`。这些数字只描述受控 WPF 窗口收到 Rendering 回调时的间隔和探针动作时间；不能转换为“显示器实际丢了多少帧”。

## 同次运行的失败门禁

报告最终列出 5 个几何问题：

1. `Shell 980x640` header actions 超出 `HeaderSurface` 边界。
2. `Shell 1040x700` header actions 超出 `HeaderSurface` 边界。
3. Media `1040x700` inbox grid 与 batch row 的顶部间距为 `233 DIP`。
4. Media `1100x720` inbox grid 与 batch row 的顶部间距为 `233 DIP`。
5. Media `1366x768` inbox grid 与 batch row 的顶部间距为 `269 DIP`。

本批不把这 5 项归类为性能通过，也不直接改写几何实现；它们是当前 `shellqa` 的真实失败输出，后续应作为独立的小批量几何复核/修复入口。R23-05 的性能数据保留为“代理已取得、报告未全绿”。

## 证据分账与未验边界

| 证据层 | 本批结果 | 可以说明什么 | 不能说明什么 |
| --- | --- | --- | --- |
| WPF `CompositionTarget.Rendering` | 已运行 | 应用层回调间隔代理 | DWM/显示器呈现帧、真实刷新率或物理掉帧 |
| `Stopwatch` | 已运行 | 受控动作/布局测量的单调耗时 | Playnite 宿主真实输入到屏幕的端到端延迟 |
| 离屏截图/逻辑 DIP | 同次生成 | 受控布局和几何报告的样本 | 真实宿主 DPI、物理跨屏、presented frame |
| ETW/PresentMon | 未取得 | — | p95/最大真实呈现间隔、系统调用栈、GPU/DWM 结论 |

Demo 原目录不可用，继续参考已恢复生产基线；本批没有读取或写入真实存档、删除真实媒体、写用户云端或外发诊断。游戏选框、滚动条系统、命令绑定、取消/错误语义、恢复保护、有限列表性能和 Playnite/net462 未改。

下一可执行任务：保留 R23-05 的几何失败待验入口，推进 R23-06 安装与回退身份核查；真实呈现帧仍需获得系统允许的 ETW/PresentMon 等价工具后另行复测。
