# Q24-08 当前 main 长文案与辅助输入复核

## 结论

复核了现有生产复选框、游戏选择输入、DataGrid 复制行为和共享长文本样式，再补强两项行为夹具；没有改生产代码、服务、DTO、命令绑定或视觉资源。本机受控行为 `15/15` 通过。Q24-08 的自动行为证据更新为通过；视觉仍待验、宿主受外部条件阻塞、最终结论未完成。

## 构建与身份

- 分支 `main`；构建源提交 `ae3c565ed9cee48c2b8bbdf19bb20610c3a7d068`，加本批两份未提交测试源改动。构建时其他任务已有的未跟踪证据和临时文件未纳入。
- 本机隔离 SDK `8.0.423`，MSBuild `17.11.48`，Windows `10.0.22000`；Playnite `net462`、测试 `net472`，VSTest `17.11.1`。
- 官方命令：`scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot .tmp/q24-08-main-ae3c565-build`；XAML 结构门禁 `24/24`，Release solution `0 warning / 0 error`。插件与测试 DLL 的 ProductVersion 为 `0.6.73+ae3c565ed9cee48c2b8bbdf19bb20610c3a7d068`。
- 插件 DLL SHA-256 `BF7D8F75E63D938AA2DCB9DDC2ED19AECEE83949F2D400D8F118DDAC084B5665`，MVID `4eb6545a-1341-48fb-950e-374f236d62c3`。
- 测试 DLL SHA-256 `C8FE451E818E282D840AED5600EE1DE1812FCF4D411615F23ED8F661F2D20BFA`，MVID `e6609259-84d0-4306-84bb-ee434bd67e9c`。
- 本批测试源 SHA-256：`Q10CheckBoxHitBehaviorTests.cs` `E9CF2A5DC00DB574574447F0D3612D9A4B2FEAEF46B4114D68596FBB96702951`；`R06ClipboardBehaviorTests.cs` `832DF15025BF41871FB0DB0D4C64B3D5DB334AA0F06A77C2EF4994416AD01A2B`。
- `python scripts/validate-source.py` 与 `git diff --check` 均通过。

## 定向行为

四类隔离 testhost 共 `15 passed / 0 failed / 0 skipped`。TRX 均已脱敏并保留 SHA-256。

| 测试类 | 结果 | TRX SHA-256 |
| --- | ---: | --- |
| `Q10CheckBoxHitBehaviorTests` | 4/4 | `069073DFC06FE76463A37AF96E7D69E32A565CD281982C01D227D87A50EF2375` |
| `GamePickerKeyboardBehaviorTests` | 6/6 | `6CF80BFD7076AC2E6DC7D51A8E2832250DFF943076AF2BA2FAC4248845F8922E` |
| `R06ClipboardBehaviorTests` | 5/5 | `B76F425AB476F472469CCDA5612C02457C739C88A29471F365409F9BA2E8C899` |
| `WpfUiResourceDictionaryTests.DenseGridLongTextUsesTheSharedEllipsisAndTooltipStyle` | 1/1 | `77018D6CAAEA8F58D2AB46437402FCA4FC90B83B83F8C9060A922235204456AD` |

TRX：[复选框](tests/Q10CheckBoxHitBehaviorTests.trx)、[游戏选择输入](tests/GamePickerKeyboardBehaviorTests.trx)、[路由复制](tests/R06ClipboardBehaviorTests.trx)、[共享长文本样式](tests/WpfUiResourceDictionaryTests-LongText.trx)。

## 覆盖范围与边界

- Light/Dark 的双倍以上合成长标签实际换行，WPF AutomationPeer 返回完整 Name/HelpText；Space 两次分别只翻转一次并派发一次测试命令，参数保持不变。禁用态 UIA Toggle 被拒绝且状态和命令计数不变。命令为 `RecordingCommand` 测试替身，只证明 WPF `Command`/`CommandParameter` 接线语义，不调用生产业务动作。
- 游戏选择输入已有 WPF 合成 `TextComposition` 路径：组合尚未提交时 Enter 不关闭选择器/不改变选中项，提交“中”后再由 Enter 接受候选。它不是 Windows 中文 IME 的真实输入证据；其他纯键盘路由和焦点负例仍由同一测试类覆盖。
- 路由 Ctrl+C / Ctrl+Shift+C 以 82 DIP 列宽、中文和技术路径合成值及 fake clipboard 验证：复制完整原值且没有显示省略号；不支持的修饰键/按键、行为禁用和无选中项均不改写剪贴板内容。没有写系统剪贴板。
- 共享样式测试确认现有 DataGrid 长文本省略/Tooltip 资源契约；没有把它当作运行时 tooltip 几何或截图证明。
- 本轮无生产 UI/视觉改动，不生成截图或 RenderHarness 结果；没有真实 Playnite、Windows IME、Narrator/OS UIA、实际系统剪贴板或物理 DPI 验证。只使用合成值和隔离 STA 窗口，未读取或更改真实配置、存档、媒体、云端。
- #979 是之前文档提交 `ae3c565` 的 Actions 检查点，与本证据的代码/测试源不同；本证据对应代码推送后的 SDK8/SDK10 CI 需另行查到终态。SDK8 本机结果不替代 CI SDK `10.0.401`。
