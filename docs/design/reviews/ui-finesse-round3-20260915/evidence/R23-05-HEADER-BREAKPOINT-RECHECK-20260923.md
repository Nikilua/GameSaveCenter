# R23-05 壳层断点几何复测

日期：2026-09-23  
分支：`codex/ui-finesse-round2`  
提交：`b5e7fca0ff12ffcefac99a7cffca039fc29eb286`  
任务：R23-05「帧性能证据闭环」的独立几何小批量

## 本批结论

本批没有新增生产 UI、服务、DTO、命令或绑定实现，只扩展 RenderHarness 的实际 WPF 壳层断点探针。R23-05 已有的壳层/媒体几何修复在提交后的 clean tree 上继续成立；R23-05 仍保持“部分满足，待宿主性能验收”，因为真实 presented frame、ETW/WPR/xperf 和 Playnite 宿主端到端性能尚未取得。

## 实际行为证据

- 隔离 Release RenderHarness `net472` 构建：`0` errors，保留 `MediaCenterView.xaml.cs:706` 两条既有 `CS8602` warning。
- `shellqa` exit `0`，报告身份为 `b5e7fca0`，`WorkingTreeClean=True`，Light/Dark、`DpiScale=1.00` offscreen logical DIP。
- 真实生产 `AcrylicProductionShellView` 在断点两侧均通过：`1200/1279` 为 compact，header `101.3 DIP`；`1280/1366` 为 expanded，header `68.0 DIP`。标题与动作区在 compact 态垂直分行，在 expanded 态水平不重叠，动作区保持在 `HeaderSurface` 内。
- 既有 Media Inbox 几何也在同次 clean probe 通过：`1040/1100/1366` 的 `gridTopGap=142 DIP`，footer、历史和次级动作可通过页级滚动到达。
- 定向 Playnite 行为套件 `ResponsiveLayoutCoordinatorTests` + `MediaInboxGeometryTests`：`8/8` 通过。

## 代码与证据分账

本批只验证以下真实关系，不以静态字符串或 `Assert.Contains` 签收：

1. WPF `RowDefinition.Height` 与共享 `ResponsiveLayoutCoordinator` 的 compact/expanded 结果一致。
2. compact 真实布局中标题与动作面板不发生纵向重叠；expanded 真实布局中两者不发生横向重叠。
3. 动作面板的实际变换边界没有越过 `HeaderSurface`，并继续覆盖 `1200→1279→1280→1366` 断点序列。

本批没有运行真实 Playnite/package-host、Windows UIA/读屏、OS 输入/IME、物理 DPI/跨屏或屏幕呈现采集；不把离屏 WPF、`CompositionTarget.Rendering`、Stopwatch 或逻辑 DIP 写成 DWM/PresentMon/物理刷新率结论。Demo 原目录不可用，继续沿用恢复生产基线；没有读写真实存档、删除真实媒体、写用户云端或外发诊断。

原始报告在本地临时目录：`artifacts/gsc-b/r23-05-boundary-20260923/shellqa-clean/shell-qa-report.txt`。该目录属于本批可再生输出，提交前清理，不进入 Git。

## 下一步

R23-04 正常可枚举 Playnite 会话仍受 CEF `platform_channel ... 拒绝访问 (0x5)` 阻塞；R23-05 的真实 presented frame/宿主性能仍需系统权限允许的 ETW/WPR/xperf/等价工具或正常宿主会话。下一可执行小批量转向依赖已满足且不依赖上述条件的 Q/R 行为复核，不重复当前几何探针。
