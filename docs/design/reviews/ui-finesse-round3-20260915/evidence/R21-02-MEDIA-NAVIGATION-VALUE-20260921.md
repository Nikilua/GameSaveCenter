# R21-02 MediaCenter 媒体详情位置值与导航边界证据

日期：2026-09-21  
提交：`806d5a27`（`补充媒体详情位置证据`）

## 本批范围

- 先复用生产 `DashboardViewModel.MediaDetailNavigationDisplay`、`CanNavigatePreviousMedia` 和 `CanNavigateNextMedia`，没有重建服务、DTO、命令或 Binding。
- 新增 `MediaDetailNavigationValueExposesSelectionBoundaries`：用合成 `MediaItemDto` 和隔离的 `BatchObservableCollection` 直接验证未选择、首项 `1 / 2`、末项 `2 / 2` 三态，以及前后导航可用性边界。
- 同时检查生产 `MediaCenterView.xaml` 的 `MediaDetailNavigationDisplay` Binding 和“媒体详情位置”语义名，并在实际 WPF `TextBlock` peer 上验证名称与当前显示值。

## 证据结果

- `R21AutomationValueBehaviorTests`：`19/19`，包含新增媒体详情位置三态和导航负例。
- 本批使用的相关组合筛选：`22/22`，`0 failed / 0 skipped`。
- 提交后以 `GscBuildCommit=806d5a27` 的 D 盘源码副本完成 Release 构建：Playnite `net462`、Tests `net472`，`0 errors`；仅保留既有 `MediaCenterView.xaml.cs:671 CS8602` 两条 warning。
- `scripts/validate-source.py`：通过；XAML 结构检查：`24/24`；`git diff --check`：通过。
- `validate_wpf_ui.py`：`0 errors / 27 warnings / 162 info`，未见本批新增诊断。

## 边界与清理

- ViewModel 证据使用合成 DTO、反射设置隔离字段和最小 WPF peer；XAML Binding/名称做了生产源码核对，但未启动真实 Playnite/package-host，也不代表 Windows UIA/读屏、OS 键盘输入、IME、物理 DPI/跨屏、最终呈现或宿主性能。
- TextBlock peer 证明隔离控件的可读名称和值承载；不把离屏/隔离控件当作真实呈现帧，也不把该测试写成真实媒体详情写入或 IPC 证据。
- 链接工作树 WPF `_wpftmp.csproj` 仍因 `Access denied` 不可直接写入，未绕过；Demo 原目录不可用，沿用已恢复生产基线；main 工作树用户改动未碰、未合并。
- 本批 D 盘 source-copy/build 已清理。R21-02 仍不整项签收。下一可执行小批量：继续盘点其他逐控件状态/值负例，公共门禁完成后进入 `R21-03` 错误播报。
