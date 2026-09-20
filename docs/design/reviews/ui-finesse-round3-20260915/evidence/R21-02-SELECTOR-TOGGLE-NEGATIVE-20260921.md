# R21-02 选择器无选中与开关三态证据

日期：2026-09-21  
代码提交：`efb42b7b`（`补充选择器与开关状态证据`）  
分支：`codex/ui-finesse-round2`

## 本批范围

本批继续复用生产已有 ComboBox、`ToggleSwitch` 和共享 `DesignTokens` 状态模板，没有改变生产 XAML、Binding、命令或设置语义。新增 `SelectorAndTogglePeersExposeEmptyAndIndeterminateStates`，用实际 WPF peer 验证两个容易被静态名称断言漏掉的边界：

- ComboBox 名称为 `任务状态筛选`，没有选中项时 `ISelectionProvider.CanSelectMultiple` 为 `false` 且 `GetSelection()` 返回 `null`；选择“失败”后变为单个可读选中项；
- 三态 `ToggleSwitch` 名称为 `启用备份策略`，`IsChecked=null` 时 UIA 为 `Indeterminate`，随后 `false` 为 `Off`、`true` 为 `On`。

这里把 `null` 作为 UIA 无选中负例保留，避免把“没有选中项”错误记录为空字符串或伪造选项；三态测试只验证共享 WPF 控件/模板能正确承载状态，不宣称当前业务 Toggle Binding 会产生 Indeterminate。

## 验证结果

- `R21AutomationValueBehaviorTests`：`21/21` 通过。
- 相关定向筛选 `R21AutomationValueBehaviorTests | R06TaskProgressBehaviorTests | R03NumericAlignmentTests`：`35/35` 通过。
- 提交身份 `GscBuildCommit=efb42b7b3dd6f259954cb3c8d170e97776428c9d` 的 D 盘源码副本 Release 构建目标为 Playnite `net462`、Tests `net472`：`0 errors`；仅有 `MediaCenterView.xaml.cs:671` 的 2 条既有 `CS8602` warning。
- `python scripts/validate-source.py`：通过；`scripts/check-xaml.ps1`：`24/24`；`git diff --check`：通过。
- WPF 静态检查：`0 errors / 27 warnings / 162 info`，warning/info 与既有基线一致，未见本批新增诊断。

## 证据边界

证据来自生产控件类型与共享模板、合成选项、实际 WPF `ComboBox`/`ToggleSwitch` AutomationPeer、fake/隔离 testhost 和 D 盘 source-copy。没有运行真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终 presented frame 或宿主性能验证；没有用离屏截图或代理性能替代这些事实。链接工作树 `_wpftmp.csproj` 仍受 `Access denied` 限制，未绕过。Demo 原目录不可用，沿用已恢复的生产基线；main 用户改动未碰、未合并。

下一可执行任务：继续 R21-02 剩余复合选择器及逐控件状态/值负例；公共门禁完成后进入 R21-03 验证错误播报。真实宿主 UIA、呈现、DPI/IME、性能仍是未验边界。
