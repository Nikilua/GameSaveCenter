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

## 尚未验证

本机 SDK8 的结果不替代修正提交的 SDK8/SDK10.0.401 Actions。真实 Playnite、物理键盘/鼠标、UI Automation/读屏、IME、物理 DPI/跨屏、最终呈现帧、ETW 和宿主性能仍未验。
