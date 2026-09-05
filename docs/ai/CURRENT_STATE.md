# GameSaveCenter 当前事实入口

> 更新时间：2026-09-05。本文是新一轮开发的短入口；历史细节仍保留在 [`PROJECT_MEMORY.md`](PROJECT_MEMORY.md)、[`WORKLOG.md`](WORKLOG.md) 和 [`DEVELOPMENT_HANDOFF.md`](../DEVELOPMENT_HANDOFF.md)，但与本文冲突时以本文和最新代码为准。

## 版本与生产入口

- 当前分支：`main`；当前版本：`0.6.73`，版本来源为 `Directory.Build.props`、`src/GameSaveCenter.Playnite/extension.yaml` 和生产侧栏版本文本。
- 生产可见宿主是 `src/GameSaveCenter.Playnite/Views/DashboardView.xaml` 中承载的 `AcrylicProductionShellView`；页面工作区位于 `Views/OverviewView.xaml`、`SaveCenterView.xaml`、`TrainerCenterView.xaml`、`TaskCenterView.xaml`、`MediaCenterView.xaml` 和 `MaintenanceView.xaml`。`DashboardView` 的兼容壳仍保留真实 Binding/命令，不应把兼容路径误当成第二套业务实现。
- Worker 入口为 `src/GameSaveCenter.Worker/Program.cs`；Playnite 与 Worker 的契约集中在 `src/GameSaveCenter.Contracts`，请求分派入口为 `src/GameSaveCenter.Worker/Ipc/IpcRequestDispatcher.cs`，持久化入口为 `Persistence/SqliteStateStore*.cs`。

## 当前视觉来源与有效例外

- 页面迁移遵循 Demo-first 规则；计划引用的 `GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design/` 在当前工作区不存在，不能作为本机测试输入，也不能声称完成与该目录的逐像素比对。当前可追溯的生产资源入口是 `src/GameSaveCenter.Playnite/Themes/AcrylicProductionResources.xaml`，其核心资源为 `DesignTokens.xaml`、`WpfUiProduction.xaml` 和 `Redesign.xaml`，并按兼容需要合并 AcrylicReference 资源。
- Demo 的 Mock 数据、演示行为和窗口按钮不进入生产。当前游戏选择器、项目现有滚动条系统、真实运行时数据以及目标文件明确要求的安全/确认语义是有效例外；页面可以重构信息架构，但必须保留命令、Binding、数据契约、错误/取消语义、虚拟化、键盘/UI Automation 和 Playnite 兼容性。
- 游戏筛选 ComboBox 使用 OneWay 显示绑定、`UiFilterSelection.Synchronize` 恢复共享状态，并以 `DropDownClosed` 作为用户写回入口；不要恢复静态 `SelectedIndex` 与双向写回竞态。工作区列表应保留有限视口、内部滚动和 Recycling 虚拟化。

## 当前已完成的功能阶段

- R01～R07：危险清理隔离/账本、任务统计历史、游戏筛选行为、媒体分页、IPC 取消/重放。
- U01～U03：任务状态视口、侧栏动画终态、游戏目录来源诊断。
- F01～F03：备份健康巡检与隔离恢复、云端队列与传输策略、媒体归类建议与可撤销批次；E02：当前事实入口与交接边界治理。
- E01 正在补证据矩阵：`scripts/e01-behavior-matrix.ps1` 可在隔离 `.tmp` 输出中分开记录业务、IPC、WPF/STA、故障/Soak 和可选 RenderHarness 结果；真实宿主项仍不会被脚本伪造为通过。
- F03 的媒体归类只基于来源规则、会话、进程映射和文件名等本地证据；低置信度不自动归类，批次快照冲突不覆盖用户修改，原始媒体和真实存档不删除。应用/撤销只移动可恢复的归档副本。

## 验证边界

- 自动基线：Release 构建 0 warning/0 error；Core `65/65`、Worker `275/275`、Playnite `338/400`（62 跳过）、XAML `19/19`；`scripts/validate-source.py`、WPF 静态审计和 RenderHarness 多尺寸/双主题/resize/侧栏探针通过。
- 离屏 RenderHarness、静态源码检查和沙箱测试不等同真实 Playnite 宿主证据。已对当前用户 Worker 完成一次只读 `system.ping` 的真实 Named Pipe 连通性验证；真实 Playnite 逐页像素、主题/高对比度、DPI、键盘焦点、媒体大库、真实云端凭据/断网、Worker 硬重启和长时多进程并发仍标记为 `MANUAL QA REQUIRED`。
- 用户可操作的验收应使用隔离 Playnite 安装、独立数据目录和明确进程边界；在这些条件未提供前，继续做安全的源码/Worker/离屏验证，但不要安装插件、写入用户数据或伪造宿主通过结论。

## 新任务启动顺序

1. 先读本文，再读 `PROJECT_MEMORY.md`、`WORKLOG.md`、`DEVELOPMENT_HANDOFF.md` 和任务相关设计门禁。
2. 先确认代码事实与本文一致；若历史条目冲突，在本文补充当前覆盖关系，不删除历史证据。
3. 每个独立阶段只改一个功能边界，补行为测试，运行 Release/门禁/渲染验证，同步三份交接文档后用中文提交并推送。
