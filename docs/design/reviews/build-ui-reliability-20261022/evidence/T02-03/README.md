# T02-03 批量选择收窄与实际键盘焦点

本阶段更新 `CompactWorkspaceTableViewportBehaviorTests.BulkHistorySelectionSurvivesInspectorBreakpointRoundTrips`。每种主题都创建真实可见并激活的窗口，先验证 `Keyboard.Focus(grid)` 返回该 DataGrid 且它是 `Keyboard.FocusedElement`，选择40/120条历史项并固定当前行，再经历三轮 1400→900→1400 DIP 断点往返。每次缩放都检查选中ID、当前行、同一键盘焦点对象、DataGrid焦点范围及动作栏可见性。测试不再程序化设置逻辑焦点，也没有改变业务页面。

测试程序集以源码父提交 `b711a820b726c25053596ece18988c720078bbf3` 为基线，只修改了本项测试；`CompactWorkspaceTableViewportBehaviorTests.cs` SHA-256 `10FA947B00606AC39B85E201C1D37943BF8FF8EEBE6835F044594D983119222A`。SDK `8.0.423`、MSBuild `17.11.48`、VSTest `17.11.1`、Playnite测试目标 net472，Windows .NET Framework `4.8.9181.0`。测试DLL SHA-256 `F27B2E65B96BD3D1059BF5EF542F5B12FCCC57C86E7D9B0219E942982E6FA22F`，ProductVersion `0.6.73+b711a820b726c25053596ece18988c720078bbf3`；独立输出在 `artifacts/gsc-b/t02-03-keyboard-focus`。

- [`keyboard-focus-light-dark.trx`](keyboard-focus-light-dark.trx)：两个主题2/2，`RunConfiguration.DisableAppDomain=true`，进程退出0。
- [`keyboard-focus-class.trx`](keyboard-focus-class.trx)：整个生产视口测试类11/11，0失败、0跳过，进程退出0。
- [`initial-focus-scope-failure.trx`](initial-focus-scope-failure.trx)：首个尝试错误地要求 `FocusManager.GetFocusedElement(view)` 必须是 DataGrid；实际结果为 `DataGridCell`。随后移除逻辑焦点设置，改为直接保持并验证真实键盘焦点。保留了这次失败，没有改弱业务断言。

定向结果证明该夹具在本机WPF上实际取得并保持键盘焦点；不代表另一设备键盘、IME、Playnite宿主或物理DPI测试。代码提交后的默认完整根 `.cmd` 尚待执行。

Build identity is in [build-identity.json](build-identity.json). It binds the test DLL/PDB to the sole modified fixture source. The new TRXs and the initial failed attempt are retained here.
