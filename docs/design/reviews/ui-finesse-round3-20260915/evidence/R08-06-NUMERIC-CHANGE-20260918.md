# R08-06 数字变化动效（2026-09-18）

## 结论

R08-06 已满足。概览页只对六个有价值的摘要计数提供短促、渲染层级的缩放反馈；高频刷新受节流保护，减动效时立即更新，任务进度与技术文本保持稳定、可复制，`99→100` 不改变相邻指标的位置。

## 实现与身份

- `acbfe7e0`（`补充概览数字变化反馈`）复用现有 `GscMotion`，新增 `NumericChangeFeedback` 附加行为和 render-only `ScaleTransform` pulse；不重建服务、DTO、命令或页面数据链。
- 六个 Overview 摘要计数显式启用行为，固定 `Width="96"`；`OverviewTaskProgressBar` 的百分比和 `GscTypographyTimeCell` 技术时间文本不启用行为。
- 计数行为只接受整数值变化；首次已存在值作为基线；反馈峰值为局部 `1.04`，最短反馈间隔 `420ms`，使用现有 Motion token。`MotionEnabled=false` 或系统动画关闭时不创建动画，文本仍即时更新。
- `e264acff`（`校正数字动效行为取样`）只校正测试的 WPF 渲染队列取样：先泵送 `DispatcherPriority.Render`，再检查真实 `ScaleTransform.ScaleX` 动画源，避免把“动画已调用”误写成“动画已呈现”。
- 最终隔离源码根：`r08-06-source-e264acff`，HEAD=`e264acff2d740b91509ea3eb807154f170626c41`。
- 最终隔离构建根：`r08-06-build-e264acff`；solution Release `0 warning / 0 error`，Playnite 目标 `net462`，XAML `24/24`。
- `python scripts/validate-source.py`、`check-xaml.ps1`、`git diff --check` 通过；WPF 静态审查为 `0 errors / 21 warnings / 177 infos`，提示为仓库既有 Canvas/有限滚动/颜色令牌检查，本项未新增 error。

## 实际行为证据

最终隔离程序集串行运行：

- `R08NumericChangeBehaviorTests`：`3/3`。真实 STA `Window` 中验证 `99→100` 的反馈计数、实际 `ScaleTransform.ScaleX` 动画时钟、`101→102` 高频刷新仍只反馈一次；`NormalizeAll` 后缩放回到 `1`，相邻控件的 X 坐标稳定。减动效负例即时落字、无动画；源码范围验证六个概览计数启用且进度/技术文本排除。
- 相邻 `R08PageSwitchBehaviorTests`：`2/2`；`R08BusinessFeedbackBehaviorTests`：`4/4`。均为独立 `dotnet test` 进程，未把之前并行合跑的 WPF 时序抖动写成通过。

## 离屏视觉证据

- 最终 `render-qa.ps1 -Configuration Release` 绑定 `e264acff`，报告 `WorkingTreeClean=True`，生成 `357` 张双主题、多尺寸 PNG；`Overview-1040x700.png` 与 `Overview-1600x900.png` 已抽查，六个指标列保持对齐，指标槽位固定，未见本项新增的邻项挤压。
- 完整 RenderHarness 退出码为 `1`，报告明确只列出既有 resize 场景 `Save/step1:1100x720` 仅 `3/4` 行完整可读、`Task/step1:1100x720` 仅 `3/4` 行完整可读；这两项不属于本次数字计数动效改动，未改写为全量 `render-qa OK`。
- 该报告来自 offscreen logical DIP（`DpiScale=1.00`），不等价真实 Playnite 嵌入、物理 DPI/跨屏或用户屏幕帧。

## 边界

证据使用合成数据、隔离 STA WPF Window、fake/生产页面资源和隔离输出目录；没有修改真实存档、删除真实媒体、写用户云端或发送诊断。Demo 原始目录不可用，本阶段沿用恢复生产基线。未验证真实 Playnite/Worker 高频刷新、宿主呈现帧、UIA/读屏、物理 DPI/跨屏、ETW 或宿主 60fps 性能；这些仍是后续宿主准入边界。下一项为 R08-07 对话框遮罩同步。
