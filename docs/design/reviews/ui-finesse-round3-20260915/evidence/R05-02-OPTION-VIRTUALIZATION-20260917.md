# R05-02 选项虚拟化焦点（2026-09-17）

## 结论

R05-02 在当前可控生产范围内已满足。生产游戏选框继续使用 `ICollectionView` 过滤、WPF `ListBox` 的有限视口和 Recycling 虚拟化；2000 项合成数据下，方向键、`PageDown`、`Home`、`End` 改变键盘活动项并将活动项保持在可见视口，导航不会关闭选框。过滤隐藏时保留仍有效的当前选择并提供已有“显示当前游戏”恢复命令；数据刷新删除当前选中项时回退到首个有效可见项。

## 现状核对与负例

现有 `PickerList` 已有 `ScrollViewer.VerticalScrollBarVisibility=Auto`、`VirtualizingPanel.IsVirtualizing=True` 和 `VirtualizationMode=Recycling`；`GamePickerViewModel` 已保留过滤隐藏的选择、`SelectedGameHiddenByFilter` 和 `ShowSelectedGameCommand`，`SetItems` 已按当前视图首项提供刷新后的回退。实际补测前，生产 Shell 的 `OnPickerSelectionChanged` 会把 ListBox 键盘移动误当作鼠标选择，第一次 `Down` 后立即关闭选框；该负例不是字符串推断，而是实际 STA WPF 路由观测。

## 实现与证据

- 代码提交：`7a4ede84667d916ad61d382302b742cca8fd4704`，已推送到 `origin/codex/ui-finesse-round2`。
- `OnPickerPreviewKeyDown` 对 Up/Down/Left/Right/PageUp/PageDown/Home/End 建立短生命周期键盘导航保护，允许 ListBox 更新活动项而不触发关闭；`PreviewMouseLeftButtonDown` 会先清除保护，保留原有鼠标选择即提交并关闭的语义；卸载时清理代际状态。
- `R05OptionVirtualizationBehaviorTests` 实际生产 Shell + 隔离 STA WPF：2000 项列表初始只实现有限窗口；Down 后选中索引前进且选框保持打开，PageDown、End、Home 后活动项均在实际 DIP 可见范围；过滤到 0 项仍保留隐藏选择，恢复命令恢复全列表，删除选中项回退 `Game 0000`。定向结果 `3/3`。
- clean Release 构建：XAML `24/24`，构建 `0` 警告、`0` 错误；clean artifact 的 R05-02 `3/3`、R05-01 回归 `3/3`、既有 `GamePickerKeyboardBehaviorTests 6/6`。
- clean RenderHarness：`.tmp/r05-02-render-clean`，绑定完整 SHA，`WorkingTreeClean=True`，Light/Dark，297 张 PNG，`render-qa OK`；`Shell-1040x700`、`Settings-state-normal-1040x700` 和 `Task-1040x700` 已人工抽查。报告的滚动夹具含 50/400/2000/4468 数据量。

## 边界

键盘测试是在真实生产 Shell 视觉树中的隔离 STA WPF Window 上合成 PreviewKeyDown/KeyDown 路由，不等价真实 Playnite 宿主、物理键盘、Windows IME 候选 UI、屏幕阅读器或 UIA Pattern。RenderHarness 是离屏 logical DIP（`DpiScale=1.00`），不代表物理 DPI/跨屏、presented frame、ETW 或宿主帧率/性能；没有写真实存档、媒体、云端或发送诊断。

下一可执行任务：R05-03 弹层边缘适配。
