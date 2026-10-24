# T02-03 批量选择收窄与实际键盘焦点

本阶段更新 `CompactWorkspaceTableViewportBehaviorTests.BulkHistorySelectionSurvivesInspectorBreakpointRoundTrips`。每种主题都创建真实可见并激活的窗口，先验证 `Keyboard.Focus(grid)` 返回该 DataGrid 且它是 `Keyboard.FocusedElement`，选择40/120条历史项并固定当前行，再经历三轮 1400→900→1400 DIP 断点往返。每次缩放都检查选中ID、当前行、同一键盘焦点对象、DataGrid焦点范围及动作栏可见性。测试不再程序化设置逻辑焦点，也没有改变业务页面。

测试程序集以源码父提交 `b711a820b726c25053596ece18988c720078bbf3` 为基线，只修改了本项测试；`CompactWorkspaceTableViewportBehaviorTests.cs` SHA-256 `10FA947B00606AC39B85E201C1D37943BF8FF8EEBE6835F044594D983119222A`。SDK `8.0.423`、MSBuild `17.11.48`、VSTest `17.11.1`、Playnite测试目标 net472，Windows .NET Framework `4.8.9181.0`。测试DLL SHA-256 `F27B2E65B96BD3D1059BF5EF542F5B12FCCC57C86E7D9B0219E942982E6FA22F`，ProductVersion `0.6.73+b711a820b726c25053596ece18988c720078bbf3`。定向输出已在归档结果后清理；提交级默认全链输出保留在 `artifacts/gsc-b/74cedf9c`。

- [`keyboard-focus-light-dark.trx`](keyboard-focus-light-dark.trx)：两个主题2/2，`RunConfiguration.DisableAppDomain=true`，进程退出0。
- [`keyboard-focus-class.trx`](keyboard-focus-class.trx)：整个生产视口测试类11/11，0失败、0跳过，进程退出0。
- [`initial-focus-scope-failure.trx`](initial-focus-scope-failure.trx)：首个尝试错误地要求 `FocusManager.GetFocusedElement(view)` 必须是 DataGrid；实际结果为 `DataGridCell`。随后移除逻辑焦点设置，改为直接保持并验证真实键盘焦点。保留了这次失败，没有改弱业务断言。

定向结果证明该夹具在本机WPF上实际取得并保持键盘焦点；不代表另一设备键盘、IME、Playnite宿主或物理DPI测试。默认完整根 `.cmd` 的提交级结果见下节。

Build identity is in [build-identity.json](build-identity.json). It binds the test DLL/PDB to the sole modified fixture source. The new TRXs and the initial failed attempt are retained here.

## 提交级默认完整链通过

提交 `51641d06dc17e0d2638a5fafe25d8f68660515bc` 使用仓库默认 `GameSaveCenter-Run.cmd` 完整退出0，run `20261024T084629122Z-63a925f0`，SDK `8.0.423`。Release solution build 为0警告/0错误。全套1967项为1910通过、57跳过、0失败/错误/超时/中断；148份TRX计数一致，145个WPF隔离类全部结束，本项视口类11/11通过。

Worker发布、ZIP/PEXT打包、原子安装及Playnite启动均由默认入口报告成功。安装清单/DLL版本为0.6.73/0.6.73.0，6个程序集的ProductVersion与源码提交一致；ZIP与PEXT SHA-256相同。完整摘要、包哈希、安装程序集身份及本机原始诊断/控制台SHA-256记录在 [standard-verification.json](standard-verification.json)。原始诊断保留在本机 `artifacts/build-diagnostics/20261024T084629122Z-63a925f0/`，没有复制进Git。

第一次受限沙箱运行同一提交时，solution build和测试均无失败，但11个环境门控用例被跳过，且安装复制到用户Playnite目录时被沙箱拒绝，根入口退出1。该次摘要和控制台保留在本机 `artifacts/build-diagnostics/20261024T080951054Z-f0a47fb7/` 与 `artifacts/standard-51641d06-console.txt`。随后在获准的本机执行上下文重跑默认入口，11个门控用例实际执行且全链退出0；源码提交未改变。细节见 [standard-verification.json](standard-verification.json)。

本证据确认受控WPF真实键盘焦点和完整构建安装链；不声称已验证物理键盘输入、IME、宿主中插件实际加载路径或物理DPI。
