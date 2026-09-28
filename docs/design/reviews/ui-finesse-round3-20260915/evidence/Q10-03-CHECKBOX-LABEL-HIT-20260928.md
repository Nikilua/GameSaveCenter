# Q10-03 复选标签命中与身份错配复核

日期：2026-09-28。实现提交：`11a513efca32cca5d1e13630e7db4b5b811ba36f`。

## 共享模板调整

- `Themes/DesignTokens.xaml` 的生产 `GscCheckBox` 模板为字符串内容提供 `AccessText` 数据模板，允许长标签在剩余宽度自然换行，同时保留 WPF access-key 解析。
- 模板根 Grid 增加透明背景，使复选框图形、文本与中间留白都属于同一个 hit-test surface。
- 不改命令、绑定、ViewModel、设置/存档业务语义或 Playnite 兼容目标。

## 行为证据

干净身份下的 Release 隔离测试 [Q10-03 TRX](Q10-03-CHECKBOX-LABEL-HIT-20260928.trx)：`4/4` 通过、0 失败/跳过，覆盖以下实际测试方法（每个方法分别运行 Light 与 Dark）：

- `SharedCheckboxWrapsLongLabelsAndKeepsBoxTextAndGapInOneActivationSurface`：加载生产 `GscCheckBox` 样式和运行时 Light/Dark palette。长中文标签在 220-DIP 控件宽度内实际换为超过两行；使用 WPF `VisualTreeHelper.HitTest` 检查方框、标签及两者之间的留白均落在同一模板根内；随后调用实际 `ButtonBase.OnClick`，逐点确认每次只切换一次并只发出一个 Click。
- `SpaceTogglesOncePerKeyPressAndDisabledCheckboxRejectsTheInput`：STA 窗口中派发合成 Space KeyDown/KeyUp，两次按键各产生一次状态变化与 Click；禁用后不能获取键盘焦点，UI Automation Toggle provider 抛出 `ElementNotEnabledException`，状态和 Click 计数保持不变。

点击部分是 WPF 命中几何加 `ButtonBase.OnClick` 派发终点探针，并非 OS 鼠标事件；键盘由合成 WPF routed events 驱动；Toggle provider 是进程内 UIA provider 调用，不是 Inspect/Narrator 或系统级 UIA。没有在当前 Playnite/package-host、物理 DPI 或呈现帧中复核。长标签测量也只是受控 WPF 逻辑 DIP 证据。

同身份 Release solution build `0 warning / 0 error`；XAML 结构检查 `24/24`；`validate-source.py` 通过。代码只改共享模板并新增行为测试。

## 用户提供的一键构建日志

2026-09-28 13:03 的 `for run` checkout 日志中，Core `125/125`、Worker `357/357` 和 Release 编译通过；Playnite `KeyboardFocusSourceTests` 有 3 条失败、2 条通过。失败为：

- `CompactInspectorsDeclareEscapeAndFocusReturnContract`
- `ProductionGamePickerClosesWithEscapeOrEnterAndReturnsFocus`
- `ProductionHeaderAndPickerActionsHaveStableAutomationNames`

这三条都先经 `TestRepositoryContext.Root` 读取源码；失败点为源码身份门，不是对应源码断言。测试程序集元数据 `GscBuildCommit=6618de227b888203cf3441b96d24eef7fd2d56c2`，它指向的 `GscSourceRoot` 当时 `HEAD=8a7a56b56a67dd86c2d1567e63c8913b8a8d03c7`。该类另外两条不读源码的隔离 WPF 行为测试通过。用当前 checkout 重新隔离构建并运行后，[KeyboardFocusSourceTests clean recheck TRX](REMOTE-IDENTITY-MISMATCH-KEYBOARD-FOCUS-CLEAN-RECHECK-20260928.trx) 为 `5/5`、0 失败/跳过。

因此已确认原日志是程序集与源码 checkout 身份不一致导致的测试门失败，不是这三条产品行为断言回归。具体为何旧 commit 身份程序集进入当次输出路径尚未确定；身份门保持 fail-closed，本次没有放宽校验或改写测试。脚本使用新建隔离输出目录的当次复跑仍待验证，以确认其他机器上是否可复现。

## 安全与未验边界

测试只使用合成标签和隔离 STA Window；没有读写真实存档、媒体、用户设置、云端或诊断，也未启动 Playnite。当前代码不改变 `Q10-03` 的“宿主外部阻塞/最终未完成”状态：仍需真实窗口中的鼠标命中与标签视觉确认。未验证 Narrator/Inspect、触屏/OS 输入、Playnite 包身份、物理 DPI 或最终呈现。
