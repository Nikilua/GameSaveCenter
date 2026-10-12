# R05-06 当前 main 开关保存语义复核

日期：2026-10-13（Asia/Shanghai）
生产代码基线：main `0fcb44f851257504d867401dab92a74b3807899a`。本轮没有改生产设置、保存协议、服务或 DTO；只补了既有 STA WPF 行为用例的“同值重复赋值不重复通知”负例。修改后的测试源码 SHA-256 为 `7C84BDBD0FAB4550F972B7D76E3E2B482BE200AB6C31C11C32B0664B476BE7A7`。

## 现有实现与本轮覆盖

继续复用 `GameSaveCenterSettings`、生产 `GameSaveCenterSettingsView`、现有 `ToggleSwitch` 模板、Portable JSON 和保存反馈状态机。本轮没有重建即时/保存边界，也没有增加新的设置服务。

当前 main 的 `R05TogglePersistenceBehaviorTests` 通过 STA WPF Window 加载生产设置视图，使用隔离临时目录和合成设置：

1. 把“启用界面动画”切为关闭后，模型值、实际 ToggleSwitch `IsChecked`、生产模板 `Track` 的关闭态 Brush、dirty 保存提示和 `ExportPortableJson` 隔离快照一致。
2. 将同一个 `false` 值再次写入后，`PropertyChanged` 计数仍为 `1`，Track 与“未保存更改”提示不变。这个负例覆盖了同值更新不会重复触发绑定刷新。
3. 调用 `CancelEdit` 后，模型、开关和 Track 回到开启态，计数只因一次真实值变化增至 `2`，dirty 状态清除，提示回到“已保存”。
4. 媒体同步、云端上传、任务通知和健康巡检开关会禁用其依赖面板；回滚后四个值和两个依赖面板恢复。
5. `SettingsSaveFeedbackTests` 区分 Playnite 设置写入失败与 Worker 应用失败，验证原因阶段和保留当前编辑的反馈；成功应用会回到 Idle 并清空失败信息。
6. `SettingsDraftLifecycleBehaviorTests` 验证编辑草稿经过设置视图 detach/reattach 和分类切换仍保留；“否”路径保留草稿与焦点，“是”路径才关闭并回滚。

## 本机验证与身份

- 本机隔离 SDK `8.0.423`；官方 `scripts/build.ps1 -Configuration Release -SkipTests`：XAML `24/24`，Release solution `0 warning / 0 error`。Playnite 目标 `net462`，测试目标 `net472`。
- 当前定向测试合计 `28 passed / 0 failed / 0 skipped`：`R05TogglePersistenceBehaviorTests 1/1`、`SettingsSaveFeedbackTests 2/2`、`SettingsDraftLifecycleBehaviorTests 1/1`、`UiFinesseRound2ControlSourceTests 24/24`。
- 脱敏 TRX：[`R05-06-toggle.trx`](tests/R05-06-toggle.trx) SHA-256 `D8BA6C2A234966F8C891A5A34E58CEBFBECEAE30592C678D3055C8F16C0AB2CA`；[`R05-06-save-feedback.trx`](tests/R05-06-save-feedback.trx) `68F04DAE2060952601C9C9E64446E31D885536B2BB3DCB50F8E460A6141CDA9C`；[`R05-06-draft-lifecycle.trx`](tests/R05-06-draft-lifecycle.trx) `AE01E1AA8D459553D2873271DF74011D214EF7D4B6DBB7CB7C6B05A1D3CB1D8D`；[`R05-06-source-contracts.trx`](tests/R05-06-source-contracts.trx) `22F6CDA4BF099CB35E69705643848E84CA602891E4FFAAD70A90414EE1D0464F`。
- 改动测试源 SHA-256 如上；其余测试源 SHA：save feedback `28801A2047EEE70325736BF24447F4D24594A4AB537CD41B2503158A24387454`，draft lifecycle `9B67A3E4FAB710FEB1C6C6576EB4B80ED809B88D689D6CA21C012304B281C6DB`，共享源契约 `B2717D805C550ADB789F77D4E47BF130ABB0458FBCB41A977DEF3637D6FB557C`。当前插件 DLL SHA-256 `3CF5A200D867E1A12B59189853CC05B3ACD7385E6B1D0B758A8A4C3BF867494B`；测试 DLL `9EA9D0B7C97D0E3FC2AC8AA0FF4351C91AEE7B29F3556BCD9EC2B1F1F1B117EE`。构建程序集元数据绑定父提交 `0fcb44f8`；测试源码 SHA 独立绑定本轮负例变更。
- 相关当前源 SHA-256：`GameSaveCenterSettingsView.xaml` `36075E7639A01133EB415CBBD0E56359CEB67AFB3898C43F9F8369AB94070C72`；`GameSaveCenterSettingsView.xaml.cs` `6298F45B7434BDCA434E16DDAD885DF84D58ACF2CFAF2B816C0E52FBA4FF43C5`；`GameSaveCenterSettings.cs` `4C1D01668403FEBF21D2C55CB0F955A251FA34B44943873E3AC67077D721AEB0`；`DesignTokens.xaml` `2FB7931D4F5C7E741484F5F7D59A68785CCD9F7F69C85BC9D2C8AE26CC4EE31B`。

## 视觉与外部边界

本次没有生产视觉改动；当前行为夹具检查真实 ToggleSwitch 模板的 Brush 状态，但不生成像素或最终呈现帧证据。历史双主题 RenderHarness 报告绑定 `cf9a250f`，而设置页与共享资源后来已有变化，因此没有把那组旧截图冒充当前 main，也没有重跑完整 297 图 RenderHarness。

设置 DTO、Portable JSON 和文件路径均为合成数据/隔离临时目录；没有改真实 Playnite 配置或触碰真实存档、媒体、云端。窗口关闭分支通过测试 seam 驱动，不等价于真实 Playnite 原生对话框；未验真实 Worker 应用故障注入、OS 输入/IME、UIA/Narrator、物理 DPI/多屏、ETW 或宿主性能。

下一项：R05-07 Tooltip 时序当前 main 适用性与实际行为复核。
