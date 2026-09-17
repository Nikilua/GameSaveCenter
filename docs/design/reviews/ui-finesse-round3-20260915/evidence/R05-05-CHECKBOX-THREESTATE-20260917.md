# R05-05 复选框三态边界证据

核对日期：2026-09-17（Asia/Shanghai）  
代码基线：`52900815c1fb8a16550446b9ee8d5318b8238a25`；当前文档 HEAD 为其后的文档提交 `a3c4a67cba1207e06e8454c539cad6b6bf9f3e1f`。本阶段没有生产代码变更。

## 结论

R05-05 对当前产品记为“不适用”。任务要求的是面向批量结果的三态复选框，并且要明确“当前页”或“全部结果”的作用域；当前生产媒体批量交互没有这类复选框消费点，实际模型是 `MediaCenterView` 的 `DataGrid.SelectionMode=Extended` 与 `SelectedItems` 命令参数。R05-04 已对这个真实模型补齐数量、当前窗口可操作数和暂不可见例外摘要，因此不新增一个没有业务消费者的全选/半选模型。

## 生产实现核对

- `MediaCenterView.xaml` 的媒体收件箱使用 Extended + FullRow DataGrid 选择，批量命令消费 `MediaInboxGrid.SelectedItems`；`MediaCenterView.xaml.cs` 已有按收件箱模式保留媒体 ID、加载更多恢复和批量提交前去重/无效项统计。
- 当前生产 `CheckBox` 仅用于设置项“本地镜像”和存档中心的“锁定”等标量布尔值，不能代表媒体收件箱的当前页/全部结果集合；`MediaCenterView` 没有批量 Select All CheckBox 或 `IsThreeState` 消费点。
- `UiFrameworkProbeView.xaml` 的显式三态 CheckBox 是开发校对夹具，不是生产批量功能。共享 `GscCheckBox` 与 `GscDataGridCheckBox` 已有独立 `IndeterminateMark`，`IsChecked=null` 时显示短横线；这与 Q10-02 既有证据的 `indeterminate=True mark=visible` 一致。

## 自动证据

- 按当前 HEAD 重新执行标准 `scripts/build.ps1 -Configuration Release -SkipTests`：XAML structural validation `24/24`，Release 构建 `0` 警告、`0` 错误。
- 重新生成当前 HEAD 程序集身份后执行 `UiFinesseRound2ControlSourceTests`：`24/24` 通过。此前文档提交后直接复跑出现的 `24/24` 失败是构建程序集仍绑定代码提交 `52900815`、源码根 HEAD 已变为 `a3c4a67` 的身份不一致，不是产品或 Q10-02 失败。
- Q10-02 的既有共享控件证据仍有效：Light/Dark 受控夹具记录 `GscCheckBox`/`GscDataGridCheckBox` 半选标记可见；该夹具不含真实媒体批量集合，也不把视觉半选状态写成批量语义。

## 门禁与边界

R05-05 的 Space 切换、UIA 三态状态和分页追加/部分失败后的复选框集合，在当前生产没有对应控件，故标记为不适用，不用静态 `Assert.Contains` 伪造交互验收。媒体批量集合、分页保留 ID、不可见例外和清空语义由 R05-04 的真实生产 DataGrid 行为证据覆盖。

本阶段未启动真实 Playnite，未验证真实 OS Space、屏幕阅读器/UIA、IME、物理 DPI/跨屏、presented frame、ETW 或宿主性能；共享控件证据来自 STA WPF/RenderHarness 的 offscreen logical DIP。未写真实存档、媒体或云端。

若未来新增“当前页/全部结果”复选框，应重新打开本项，先固定作用域与集合来源，再补全选/半选/空选、筛选切换、分页追加、部分失败、Space 和 UIA 状态的真实行为测试。

下一项：R05-06 开关保存语义。
