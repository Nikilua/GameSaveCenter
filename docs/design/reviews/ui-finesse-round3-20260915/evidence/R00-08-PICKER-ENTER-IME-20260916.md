# R00-08 搜索框 Enter/IME 证据

日期：2026-09-16  
分支：`codex/ui-finesse-round2`  
实现提交：`8435d80302c64908e0ec9722e87af3226894d57a`

## 对照基线

质量审查指出，生产壳层原先在 `PreviewKeyDown` 中只要存在旧的 `SelectedGame` 就关闭选框；而 `GamePickerViewModel` 会保留被当前筛选隐藏的旧游戏。R00-08 要求先核查这一状态链，再覆盖无结果 Enter、中文候选确认、方向键、Enter、Esc 和焦点返回，不把源码分支存在当作物理输入已复现。

## 实现

- `OnPickerPreviewKeyDown` 先放行 `Key.ImeProcessed` 及带 `ImeProcessedKey` 的事件，让搜索框和输入法处理候选确认。
- Escape 保持现有关闭与 `GameContextButton` 焦点返回；方向键和其他未声明按键不在 Shell 预览处理器中拦截。
- Enter 不再检查旧的 `viewModel.SelectedGame`，而是读取 `PickerList.SelectedItem`，并要求候选仍被共享 `GamePicker.ItemsView` 包含；无候选或候选已被筛选隐藏时保持弹层和旧游戏不变。
- 有效可见候选仍通过现有 `DashboardViewModel.SelectedGame` / `GamePicker` 写入链确认，再复用 `ClosePickerAndRestoreFocus`；没有改动选框、滚动、命令、取消/错误或恢复安全语义。

## 行为验证

命令：

```text
dotnet test tests\GameSaveCenter.Playnite.Tests\GameSaveCenter.Playnite.Tests.csproj --configuration Release --no-restore --filter "FullyQualifiedName~GamePickerKeyboardBehaviorTests|FullyQualifiedName~KeyboardFocusSourceTests|FullyQualifiedName~GamePickerViewModelTests"
```

结果：Playnite `net462` 生产程序集构建 `0 warning / 0 error`；指定测试 `28/28` 通过。

实际 STA WPF 路由覆盖：

1. 合成旧游戏仍为 `SelectedGame`、当前 `ItemsView` 无结果时，Enter 未处理，弹层保持可见，旧游戏对象保持不变。
2. `Key.ImeProcessed` 以及 Up/Down/Left/Right 均未处理，弹层保持可见，旧游戏对象保持不变。
3. 当前 `PickerList` 可见候选的 Enter 被处理，弹层关闭，候选写回共享选择状态，键盘焦点返回 `GameContextButton`。
4. 既有生产 Shell Escape STA 测试同时通过，确认关闭、Handled 和焦点返回没有回归。

## 2026-09-18 当前分支复核

- 当前 HEAD `4f5850247d1775fe0f9ffe252b727f06eea0699e` 使用隔离 OutputRoot `.tmp/r00-07-08-build-clean` 重建；`GamePickerKeyboardBehaviorTests|KeyboardFocusSourceTests|GamePickerViewModelTests` 当前通过 `31/31`，其中包含活动 text composition 未提交时 Enter 保持弹层、提交后才确认的实际 WPF 路由负例。
- 运行覆盖仍是生产 `AcrylicProductionShellView`、真实 WPF `Window`、`Keyboard.Focus` 和 PreviewKeyDown/TextComposition 路由：无结果 Enter 不处理且旧选择不变；IME/方向键不关闭；可见候选 Enter 关闭并回焦点；Esc 和清除按钮焦点行为保持。
- 当前输出来自 clean-tree 隔离构建，未启动 Playnite/Worker，也未把 `Key.ImeProcessed` 夹具等同于 Windows 中文输入法候选窗口；真实 OS IME 时序、物理键盘/DPI、presented frame、ETW 和宿主性能仍是边界。

## 边界

- 测试通过真实生产 `AcrylicProductionShellView`、真实 WPF `Window`、控件路由和 `Keyboard.Focus` 验证行为；游戏数据为合成 DTO，Dashboard 仅以未初始化的最小测试承载注入 `GamePicker` 字段，未启动 Worker、Playnite 或任何真实文件操作。
- `Key.ImeProcessed` 是 WPF 路由层的 IME 事件夹具，不等价于 Windows 真实中文输入法候选窗口、物理键盘时序或宿主呈现帧；真实 Playnite 嵌入、物理 DPI、屏幕像素、ETW 和 presented frame 仍未验。
- 本阶段没有将无真实 OS IME/Playnite 输入写成缺陷已在宿主复现，也没有修改用户存档、媒体、云端或诊断外发数据。

## 下一步

R00-08 代码与受控行为证据已完成，下一可执行小批量为 R01-01“测试源码根绑定”：修复隔离 `OutputRoot` 下源码测试可能向错误 checkout 回溯的问题，并补清晰的错根诊断。
