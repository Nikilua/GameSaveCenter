# R21-02 SaveCenter 控件名称与值（2026-09-21，续作小批量）

## 范围与事实

- 先复用 SaveCenter 现有的 `ToggleSwitch`、`ComboBox`、`CheckBox`、命令和 Binding。本批没有新增服务、DTO、命令、数据值或新的控件模板。
- 提交 `efa9614f` 为四个存档策略开关补语义 Automation 名称：启用备份策略、游戏退出后自动备份、游玩中定期备份、备份后自动上传云端；同时为异常保护等级、策略模板、锁定所选版本和两个策略动作补名称。
- 新增的生产源码契约将控件名称与对应的 `IsChecked`/`SelectedValue`/`SelectedItem` Binding 一起核对，避免只看到孤立标签或 ToolTip 就签收关联关系。

## 验证结果

- `R21AutomationValueBehaviorTests`：`4 passed / 0 failed / 0 skipped`。其中受控 STA WPF `AutomationPeer` 实际验证四个自定义 ToggleSwitch 的名称和 `Off → On` 状态，以及异常保护 ComboBox 的名称和选中值变化；glyph 动作名、ProgressBar RangeValue 和上一批生产契约继续覆盖。
- R21 相关键盘、焦点、无障碍和生产壳层回归：`35 passed / 0 failed / 0 skipped`。
- 隔离 source-copy Release 编译：Playnite `net462`、Tests `net472` 无错误；首次编译只出现 `MediaCenterView.xaml.cs:671` 的 2 条既有 `CS8602` warning，后续 no-restore build summary 为 `0 warning / 0 error`。`validate-source.py`、XAML `24/24`、`git diff --check` 通过；WPF 静态检查 `0 errors / 27 warnings / 177 info`，没有新增对应警告。

## 未收口边界

- R21-02 仍为“实现中，待继续”：TaskCenter DataGrid 内重复进度条、Maintenance 远端备份进度，以及其余页面的复合选择器/外置标签和逐控件状态/值负例还未逐项签收。
- 当前行为证据是受控 WPF 与生产 XAML 接线，不代表真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现或宿主性能；没有调用真实命令，也没有写真实存档、媒体、云端或诊断数据。Demo 原目录不可用，main 用户改动未碰、未合并。
- 本阶段 source-copy/build 目录已在提交文档前按精确路径清理。
- 下一可执行小批量仍为 `R21-02`：先处理 TaskCenter/Maintenance 的进度条 UIA 名称和值与负例；完成后再进入 `R21-03` 验证错误播报。
