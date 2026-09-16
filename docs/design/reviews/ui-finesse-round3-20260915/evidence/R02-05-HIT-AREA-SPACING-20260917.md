# R02-05 命中区与间距证据

日期：2026-09-17

状态：已满足。当前生产实现已经具备共享按钮密度链，本阶段没有重建生产控件或修改业务入口，只增加实际 WPF 命中几何门禁并完成双主题复核。证据绑定提交：`f03b4dd34902f93f04b3f2c2121ba7f4cfc70e87`（`codex/ui-finesse-round2`）。

## 1. 最新实现核对

- `WpfUiProduction.xaml` 的 `GscIconOnlyButtonBase` 继承共享 `GscWpfUiButton`，保留统一焦点、悬停、按下和禁用模板；实际按钮为 `34×34 DIP`、最小宽高同为 `34 DIP`、`Padding=0`，相邻逻辑间隔为 `6 DIP`。复制、重试、取消和删除均通过该共享链或其危险变体呈现，不是每页重造 hit target。
- `GscIconOnlyToolbarButton` 继承图标基类并将按钮提高到 `36×36 DIP`；常规文字/工具条动作继续通过 `GscButtonHeight=36`，紧凑文字动作通过 `GscCompactButtonHeight=30`。`GscWpfUiActionButton`、`GscWpfUiContextButton` 和媒体批量按钮继续复用既有 8 DIP 动作节奏及共享模板。
- `TaskCenterView.xaml` 的复制/重试/取消行、`SaveCenterView.xaml` 的策略模板删除行使用图标按钮；`MediaCenterView.xaml` 的收件箱批量动作使用独立 `WrapPanel` 和 `GscWpfUiMediaBatchButton`，没有改变游戏选框、滚动条、命令绑定或安全门禁。

## 2. 实际行为门禁

新增 `R02HitAreaSpacingTests`，不是 `Assert.Contains` 字符串签收；测试加载生产 `DesignTokens.xaml`、`WpfUiProduction.xaml`、`Redesign.xaml`，在隔离 STA WPF Window 中完成 Measure/Arrange、真实视觉树命中和几何测量。

- `AdjacentCopyAndDeleteButtonsHaveIndependentHitTargets`：复制与删除实际矩形分别为 `(0,2.67,34×34)` 和 `(40,2.67,34×34)`；两者中心命中各自的 `Button` 实例，中心之间的 6 DIP 间隔不命中任一按钮，矩形无正面积重叠。
- `NarrowWrapKeepsCopyAndDeleteInOneActionRow`：宽度 `82 DIP` 的窄 `WrapPanel` 实际排布为 `(0,0,34×34)`、`(40,0,34×34)`、`(0,34,34×34)`；复制/删除保持同一操作行，第三动作换到下一行，三个中心命中各自实例。边界刚好相接时按严格正面积判断，不把“接触但未重叠”误判成 overlap。
- R02-05 定向 Release 测试：`2/2` 通过。

## 3. 视觉与完整验证

- RenderHarness 当前报告身份为 `f03b4dd34902f93f04b3f2c2121ba7f4cfc70e87`，`WorkingTreeClean=True`，`DpiScale=1.00` 明确为 offscreen logical DIP；Light/Dark 的 56 个视图/窗口场景均 `OK`，Light/Dark Media `1040×700` 均 `OK`，报告末尾为 `render-qa OK`。
- 人工对照了 Light/Dark `Media-1040x700.png`：批量处理栏、归类主动作和预览动作在生产窄尺寸中仍保持一组，列表及页面滚动未被本项改变。
- 完整 Release 脚本：XAML `24/24`；编译 `0 warning / 0 error`；Core `83/83`；Worker `311/311`；Playnite `537 passed / 57 skipped / 0 failed`（总计 `594`）；源码校验通过。

## 4. 边界与下一步

本证据使用合成按钮/图标、真实生产资源、隔离 STA Window、offscreen logical DIP 和 `VisualTreeHelper.HitTest`。没有启动真实 Playnite 嵌入界面，没有执行真实鼠标/OS 输入、屏幕阅读器、物理 DPI/跨屏、IME、presented frame、ETW 或宿主性能验证；没有写入真实存档、媒体、用户云端，也没有发送诊断。6 DIP 是当前共享图标按钮的生产间隔契约，34 DIP 是紧凑图标动作例外，不应外推为所有主动作的 36 DIP 结论。

下一可执行任务：R02-06 菜单状态完整。重点核对当前菜单资源与键盘上下/左右/Enter/Esc、危险/禁用/勾选/子菜单和长标签的实际行为。
