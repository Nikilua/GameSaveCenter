# R21-02 MediaCenter 媒体列表语义与选择状态证据

日期：2026-09-21
提交：`c419fc98`（`补充媒体列表辅助语义`）

## 本批范围

- 复核生产 `MediaGrid` 已有 `SelectionMode="Extended"`、`SelectedMedia` Binding、`SelectionChanged` 事件、项目自有虚拟化面板和滚动条系统；只补列表级 `AutomationProperties.Name="当前游戏媒体列表"`。
- 不更换 `ListBox`、游戏选框、`VirtualizingWrapPanel`、ScrollViewer 或选择恢复逻辑，不改变媒体筛选、分页、命令或写入语义。
- 新增 `MediaCenterListExposesSemanticNameAndSelectionState`：使用实际 WPF `ListBox` peer 检查列表名称、`ISelectionProvider.CanSelectMultiple`、初始选中项和选中项从“媒体 A”切换到“媒体 B”。

## 证据结果

- `R21AutomationValueBehaviorTests`：`18/18`。
- R21-02 相关进度/焦点/键盘/无障碍/生产壳层套件：`66/66`。
- D 盘源码副本、显式 `GscBuildCommit=c419fc98` 的 Release 构建：Playnite `net462`、Tests `net472`，`0 errors`；仅有既有 `MediaCenterView.xaml.cs:671 CS8602` 两条 warning。
- `scripts/validate-source.py`：通过；XAML 结构检查：`24/24`；`git diff --check`：通过。
- `validate_wpf_ui.py`：`0 errors / 27 warnings / 162 info`，未见本批新增诊断。

## 边界与清理

- WPF 探针证明隔离 `ListBox` 的 UIA 名称与选择模式/变化，不代表真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现或宿主性能；不把离屏/隔离 peer 当成真实呈现帧。
- 链接工作树 WPF `_wpftmp.csproj` 仍因 `Access denied` 不可直接写入，未绕过；Demo 原目录不可用，沿用已恢复生产基线；main 工作树用户改动未碰、未合并。
- 本批 D 盘 source-copy/build 已清理。R21-02 仍不整项签收。下一可执行小批量：继续其他逐控件状态/值负例；完成公共门禁后进入 `R21-03` 错误播报。
