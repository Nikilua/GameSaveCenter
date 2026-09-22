# R09-05 焦点轮廓合成

日期：2026-09-19  
实现/验证提交：`3f7d0b30`（补充焦点轮廓合成行为夹具）  
分支：`codex/ui-finesse-round2`

## 结论

R09-05 在当前可控条件下已满足。复核最新生产资源后没有发现需要重建焦点体系的实现缺口；本阶段补充真实生产样式的 STA WPF 行为夹具，验证焦点覆盖层、共享焦点描边、选中 Chrome、输入校验错误以及错误恢复的叠加关系。

生产实现保持 Demo-first 的恢复生产资源基线，并继续保留命令、绑定、游戏选框、滚动条、取消/错误语义、恢复保护和列表性能策略。

## 现有实现复核

- `DesignTokens.xaml` 的 `GscSharedFocusVisual` 是共享焦点样式：实际模板为 `2 DIP`、`CornerRadius=13`、`GscAccentBrush`、`Opacity=0.72` 的 Border。`WpfUiProduction.xaml`、`Redesign.xaml` 和现有页面样式继续通过该资源复用，没有用 `FocusVisualStyle={x:Null}` 取消键盘焦点反馈。
- `GscWpfUiButton` 的 `ButtonChrome` 和 `FocusOverlay` 都使用 `CornerRadius=14`；焦点覆盖层覆盖完整圆角 Chrome，`IsHitTestVisible=False`，`IsKeyboardFocusWithin=True` 时进入 `Opacity=1`，同时边框切换为 accent。内部 Clip 不会截断自身圆角层，外层共享焦点 Border 不依赖按钮内容区域。
- `GscRedesignWorkspaceTabItem` 的选中态使用 accent 背景/边框；工作区 Tab 的 Grid 和 Chrome 保持 `ClipToBounds=False`，让共享焦点描边有圆角外侧空间。
- `GscWpfUiTextBoxTemplate` 先处理键盘焦点，再由 `Validation.HasError=True` 明确覆盖错误底色、错误边框和 `2 DIP` 边框；有效值恢复后，焦点填充、accent 边框和 `1 DIP` 边框重新生效。

## 行为证据

新增 `R09FocusOutlineBehaviorTests`，使用当前 checkout 的真实生产 ResourceDictionary、合成控件、隔离 STA Window 和合成校验数据：

- `FocusedPrimaryButtonKeepsRoundedOverlayAndSharedFocusRingWithNegativeBlurredState` 实际聚焦生产 primary Button，读取实际模板中的 `FocusOverlay`、`ButtonChrome` 和共享焦点模板实例；确认完整圆角覆盖层、2 DIP 共享描边、非命中测试以及 accent 边框均存在。焦点转移到外部 TextBox 后，按钮焦点覆盖层实际回到 `Opacity=0`，作为失焦负例。
- `SelectedTabAndInvalidTextBoxKeepFocusRingOutsideStateChromeAndRecoverAfterError` 实际挂载生产 Workspace Tab 到 TabControl，确认 selected Chrome 的 accent 边框和 `ClipToBounds=False`；随后用 `IntegerRangeValidationRule(1..3)` 驱动真实生产 TextBox 从有效值变为 `9`，确认错误底色/边框/2 DIP 边界，同时焦点样式仍存在；改回 `2` 后确认错误消失并恢复焦点填充、accent 边框和 1 DIP 边界。
- 测试使用 `R09FocusWpf` xUnit collection 禁止本组 STA Window 并行，并且不创建跨测试共享的 `Application.Current`。这避免已关闭 Dispatcher 的全局 Application 污染后续窗口；该隔离措施只影响测试调度，不改变生产代码。

当前提交的精确 Release 验证：

- `scripts/build.ps1 -SkipTests`：XAML `24/24`，solution `0 warning / 0 error`，包含 Playnite `net462`。
- `R09FocusOutlineBehaviorTests`：`2/2`。
- 相邻回归：R09-02 `2/2`、R09-03 `2/2`、R09-04 `3/3`、共享焦点/列表资源 `2/2`；合计 `11/11`。
- `python scripts/validate-source.py`、`scripts/check-xaml.ps1`、`git diff --check`：通过。

## 边界

- 首轮隔离尝试确认：没有系统键盘输入源时，WPF 不会自动把 Focus Adorner 挂到测试窗口；因此本证据用实际控件焦点触发器和实际共享焦点模板实例化证明资源/模板行为，不把缺少系统输入源写成真实 Adorner 呈现通过。
- 当前夹具验证的是受控 WPF logical DIP、模板状态和负例，不等价真实 Playnite 嵌入、物理 DPI/跨屏、presented frame、UIA/读屏、IME、Windows High Contrast、ETW 或宿主性能。R09-06 的系统高对比真实配色仍单独验收。
- Demo 原始目录不可用，本阶段继续以恢复生产资源基线为视觉参照，没有自行切换新的设计体系。
- 未读取或修改真实存档、媒体、云端或用户 Playnite；`package-host` 仍为 `not-provided`。用户提供的 DEV-INSTALL-008 main 全量测试失败仍只作为合入后单一 checkout 重跑安装器的发布边界，未被本阶段证据覆盖。

## 下一步

下一可执行任务为 R09-06“高对比真实配色”：先核对系统 High Contrast 资源入口和切回普通主题的恢复路径，再做受控负例/行为证据；不把本阶段的 `glassEnabled=false` 或普通主题焦点测试代替系统高对比验收。
