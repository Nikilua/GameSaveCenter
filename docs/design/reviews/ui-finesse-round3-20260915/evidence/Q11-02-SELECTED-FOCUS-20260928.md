# Q11-02 当前导航项与键盘焦点叠加行为（2026-09-28）

## 当前结果

本阶段没有修改生产导航样式。共享 `AcrylicNavItem` 已有选中填充、悬停填充、选中+悬停优先级、键盘焦点描边和禁用外观；原行为用例只分别观察“选中”和“未选中但有键盘焦点”，缺少两状态同时成立的实际 WPF 断言。

新用例 `AcrylicNavigationKeepsKeyboardFocusOutlineVisibleWhileSelectedInBothThemes` 使用生产资源和 STA `Window`，在浅/深主题分别执行以下状态序列：

- 当前页选中且键盘聚焦：选中填充保留，同时显示强调描边及 `2 DIP` 描边宽度。
- 焦点移出但保留当前页：选中填充仍在，边框回到选中描边和 `1 DIP`。
- 未选中但键盘聚焦：背景透明，焦点强调描边和 `2 DIP` 仍可见。
- 控件禁用：焦点进入被拒绝，禁用透明度为 `0.46`。

这证明了生产 `ControlTemplate` 在上述 WPF 状态组合下的资源和几何行为，避免依赖轻微颜色差辨认焦点。它没有驱动真实鼠标悬停、OS Tab 输入、真实 Playnite 窗口或物理屏幕呈现，因此 Q11-02 仍未最终完成。

## 构建和执行身份

- 测试代码提交：`9987dbb10f14519f3da2614215ab298410320149`。
- Release `GameSaveCenter.Playnite.Tests` 构建成功；输出无编译警告或错误行。
- `R23ProductionResourceStateBehaviorTests`：`4/4` 通过、0 失败、0 跳过，VSTest exit `0`；TRX：`artifacts/q11-02-nav-focus-20260928/q11-02-r23-resource-state-9987dbb1.trx`。
- 用例结束未见 `InvalidComObjectException` 清理噪声。

