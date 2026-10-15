# R09 高对比度按压状态复核（2026-10-15）

## GitHub CI 失败

提交 `5862856e508fdb0abf2478fbbc54c34b34075692` 对应 [Actions #1027 / run `38698959702`](https://github.com/Nikilua/GameSaveCenter/actions/runs/38698959702)；后续 R00/R01 证据提交 `c0a98906d74c6ce9399035cf6920a59befc5c781` 对应 [Actions #1028 / run `38701408503`](https://github.com/Nikilua/GameSaveCenter/actions/runs/38701408503)。两次运行的 SDK `8.0.423` 与 `10.0.401` job 都在 `R09HighContrastBehaviorTests.HighContrastCombinationsKeepDisabledSelectedFocusAndPopupReadableAndRecoverMotion` 报告 `Assert.True()` 的 `Expected: True / Actual: False`，栈落在测试源 line 195；Core `125/125`、Worker `357/357` 已通过。

GitHub 插件已取得 #1027 和 #1028 的完整 SDK8/SDK10 job logs。失败发生在测试通过 `RaiseSpaceKey` 构造并路由合成 `KeyDown` 之后；该测试附近唯一未带状态消息的 pressed-state 断言是 `focusButton.IsPressed`。因此失败点是夹具没有可靠地产生所需的按压状态。后续 `TextServicesContext.StopTransitoryExtension()` 的 `InvalidComObjectException` 出现在测试失败后的 STA 清理；本机测试通过时也会出现相同清理输出，它不是 xUnit 失败原因。

| Run | SDK | Job ID | 诊断 artifact | SHA-256 |
|---|---|---:|---|---|
| #1027 | 8.0.423 | `116302196246` | `11955722725` | `379e45ac5d5624ade572ee5d5545f9e8cec02b1d5c5f54d992975fdfd060e25b` |
| #1027 | 10.0.401 | `116302195979` | `11956201028` | `f6f36077418aa63be90cb5923a6048ae86ef956f0699ac2fa0192a44a2a8d4da` |
| #1028 | SDK8 baseline | `116310228972` | `11956852742` | `0bda4f3c7c308cbe79fe39d3bf2025f54bb9abfdd677190c5939e8f9ad5cfbc4` |
| #1028 | 10.0.401 | `116310229860` | `11956693825` | `4e94f3afab875ae5d7618f630922273b08f38bdcfd1ff4292e898ab1ed2a8db4` |

## 夹具修正与本机验证

只改 `R09HighContrastBehaviorTests` 夹具，没有更改生产按钮样式、主题资源、命令或绑定。高对比度状态矩阵现通过已有 `HighContrastStateProbeButton` 设置 `IsPressed`，随后仍对真实生产模板检查按下 overlay、focus ring、主题状态、弹出层、动效时钟和释放后的负例；恢复普通主题后的 press/motion 状态也使用同一确定输入。单独的 `Q06ButtonStateSequenceBehaviorTests` 仍覆盖合成 routed Space 路径。两类都是受控 WPF 输入/逻辑 DIP 验证，不代表物理键盘输入。

- 本机 SDK `8.0.423`、MSBuild `17.11.48`、VSTest `17.11.1`；Playnite `net462`、测试 `net472`、测试宿主 .NET Framework `4.8.9181.0`。
- Release 测试项目及引用构建成功；`R09HighContrastBehaviorTests` `3/3`，`Q06ButtonStateSequenceBehaviorTests` `1/1`。
- 本轮未运行完整 solution build、完整 Playnite 测试或真实 Playnite 宿主；SDK8 结果不替代推送后 SDK `10.0.401` CI。
- 日志与 TRX 只保留在 GitHub run 和本证据目录；未把两份诊断 ZIP 或生成 DLL/PDB 留在仓库，避免堆积临时产物。

| 文件 | SHA-256 |
|---|---|
| `R09HighContrastBehaviorTests.cs` | `c7893c86884ba41a0b701f874bce6ddf87842403a997c1a44517402e61706951` |
| `GameSaveCenter.Playnite.Tests.dll`（本机临时产物） | `cee68914e6a12b8edf35b2ab3155d1ce4ab8e835c7e12c3ba0e9bc2f2518738a` |
| `R09HighContrastBehaviorTests-sdk8423-after-fixture-fix.trx` | `b33dd922c4e6ed211451fc0d6811c9751479b07d3e27bc081550cef651387983` |
| `Q06ButtonStateSequenceBehaviorTests-sdk8423.trx` | `68be8f16fc2c6b56623bce94bba76e48dc0107230a164071b91fd74996fbd881` |

| TRX | 结果 |
|---|---|
| [R09 high contrast](tests/R09HighContrastBehaviorTests-sdk8423-after-fixture-fix.trx) | 3 passed, 0 failed, 0 skipped |
| [Q06 button state sequence](tests/Q06ButtonStateSequenceBehaviorTests-sdk8423.trx) | 1 passed, 0 failed, 0 skipped |

## Actions #1029 失败与 UIA 行选择夹具补强

修正提交 `171ac4a6e6327dd6f163fb9112928f1c271d7e50` 对应 [Actions #1029 / run `38706481548`](https://github.com/Nikilua/GameSaveCenter/actions/runs/38706481548)。SDK8 job `116327043161` 和 SDK10.0.401 job `116327042757` 都在 `R21AutomationValueBehaviorTests.ProductionMediaInboxSelectionPatternTracksTheSelectedRow` 失败，失败点是第 485 行 `Assert.Single(selection.GetSelection())` 收到空集合。两个 CI testhost 都是 .NET Framework `4.8.9345.0`；其余该类 23 项通过，Worker `357/357`、Core `125/125` 通过。失败后的 TextServices COM shutdown 输出发生在 xUnit 报错之后。

完整日志证实的现象是：夹具设置 `grid.SelectedIndex = 0` 并执行一次 `UpdateLayout` 后，UIA Selection pattern 尚未给出所选行。日志没有记录 `DataGrid.SelectedItems` 和生成容器状态，因而不能仅凭它确认是实际选择丢失、行容器尚未生成还是 .NET Framework peer 行为差异。本机 .NET Framework `4.8.9181.0` 首次单测通过；原始测试类在正确嵌入 `GscBuildCommit=171ac4a6…` 的重建后也 `24/24` 通过，不能复现 CI 状态。

为让 UIA 查询前提可核实，测试夹具现在显式 `ScrollIntoView` 并重新布局目标行，随后断言 WPF 实际 `SelectedItem`、唯一 `SelectedItems` 和已生成的 `DataGridRow`，再保留原有 UIA Selection pattern 项数与行名称断言。切到第二行时重复相同前置验证；没有更改生产控件、XAML、命令或绑定。

- 本机 SDK `8.0.423` / MSBuild `17.11.48` / VSTest `17.11.1`，Playnite `net462` / tests `net472`；Release 测试项目及引用构建 `0 warning / 0 error`。
- 修改后的 `R21AutomationValueBehaviorTests`：`24/24`；TRX：[本机行容器复核](tests/R21AutomationValueBehaviorTests-sdk8423-after-row-realization.trx)，SHA-256 `D6EE57973E4C6F5E60D1C45240B2AFCB5D551AA0AAF0B9230395DC936D7807F3`。
- 测试源码 SHA-256 `B61ADBE1C37FBE2B232967BC649B378A7BD0CFA2D1322B8DF8ED6D6EB0A792A2`；测试 DLL `7FC1FDC6F533C8FAD16861F1CC9EAD62915D1A0CCB5DCAB0CBF03650866A3749`；生产插件 DLL `31B52F2151F35EA37A95CE5A73D10300CD838190F0994864EBF1147C609C49C3`。构建提交身份为 `171ac4a6`，测试源码 SHA 单独记录以区分未提交工作树内容。

| #1029 lane | Job ID | 诊断 artifact | SHA-256 |
|---|---:|---:|---|
| SDK8.0.423 | `116327043161` | `11959620980` | `95c4a4b14e47423fc0334afd535c30eb9b9de38d33206ea3d7ef82e7c1d69763` |
| SDK10.0.401 | `116327042757` | `11959885981` | `a40c20e5ac9bd966d205ed6c008f9c691d419071a84c3b8aae541694e33fd9c4` |

上述夹具调整是否消除 SDK8/SDK10.0.401 的 CI 失败，须以新提交 Actions 终态确认；目前根因仍未被本机 .NET Framework 复现。

## 尚未验证

本机 SDK8 的结果不替代修正提交的 SDK8/SDK10.0.401 Actions。真实 Playnite、物理键盘/鼠标、UI Automation/读屏、IME、物理 DPI/跨屏、最终呈现帧、ETW 和宿主性能仍未验。
