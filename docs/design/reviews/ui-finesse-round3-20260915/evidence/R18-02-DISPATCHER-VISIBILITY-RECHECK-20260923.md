# R18-02 真实 Dispatcher 基准定向复核

日期：2026-09-23  
当前复核提交：`23d70d65`（`校正R18-01连续输入证据`）  
实现提交：`59468b37`（真实 Dispatcher 受控窗口基准）  
来源标注校正：`5b28b0c3`  
分支：`codex/ui-finesse-round2`

## 结论

R18-02 在当前可控范围内继续记为“已满足，待环境验证”。本次没有新增生产代码，按当前检出版本在真实 STA WPF `Window` 中重跑已有两段测量：SearchText 到生产 `GamePickerPerformanceDiagnostics` 的 VM 完成点，再到受控窗口第一个可见 `ListBox` 容器；没有用 `FilteredCount` 代替可见反馈。

- `R18DispatcherVisibilityBenchmarkTests`：`1/1`。
- 本次 20 次原始样本：VM p95/最大 `38.259/64.967 ms`；VM 完成到可见容器条件成立的增量 p95/最大 `29.245/37.615 ms`。
- `visible_counts` 为 `20/20` 个 1；容器尺寸为 `476 × 19.24 DIP`，20 次稳定。
- 同一当前生产测试集的 `GamePicker|DebouncedRefreshTests` 宽筛选为 `50/50`；该口径包含 R18-01、R18-02 及键盘焦点/闭环/大库相邻类，因而不把宽筛选数量单独当作某一个 R18 项签收。

## 测量来源

- VM 阶段从设置一个不同搜索文本开始，直到 `RefreshCount` 增长且 `LastSearchText` 等于该查询；夹具显式安装 `DispatcherSynchronizationContext`，通过真实 Dispatcher 投递收敛。
- 可见阶段在已 `Show()` 且 `IsVisible` 的受控 `Window` 中，使用 `ListBox.ItemContainerGenerator.ContainerFromIndex(0)`、容器 `IsVisible`、`ActualWidth`、`ActualHeight` 和 `UpdateLayout()` 判定。
- 窗口使用 `Opacity=0.01`、不激活、不进任务栏，只作为受控行为夹具，不是用户屏幕截图或 presented frame 证据。首次未安装 Dispatcher 上下文的失败负例保留为夹具边界，不计入通过样本。

## 构建与质量门禁

- 当前检出版本隔离 Release solution：`0 errors / 2 warnings`；两条均为既有 `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs:706` 的 `CS8602`，不是本阶段新增。
- 目标 Playnite 产物仍为 `net462`，测试程序集为 `net472`；没有覆盖 `main` 的旧实现或用户文件。
- `validate-source.py`：通过；XAML 结构校验：`24/24`；`git diff --check`：通过。
- `validate_wpf_ui.py src/GameSaveCenter.Playnite`：`0 errors / 28 warnings / 162 info`。警告/信息是已有 Canvas、StackPanel/ScrollViewer 和主题资源审查提示；本阶段没有修改 XAML、主题资源或生产视觉树。

## 未验边界

Demo 原目录不可用，本阶段沿用已恢复的生产基线。受控 Window/布局可见性不等于 Windows DWM presented frame、物理屏幕帧、60fps、Playnite/package-host 嵌入、物理 DPI/跨屏、UIA/读屏、真实 OS IME、ETW/系统跟踪或宿主性能；没有绕过被拒绝的跟踪权限。测试只使用合成 DTO、隔离 STA testhost 和受控窗口，没有读写真实存档、媒体、云端或诊断目录。

## 下一步

下一可执行任务为 `R18-03 缩略图滚动预算`：先核对 `AsyncThumbnailLoader` 的活动请求上限、取消、缓存上限和迟到结果保护，再决定是否需要代码或只补证据。
