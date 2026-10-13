# Q24-04 当前 main 键盘遍历复核

日期：2026-10-14。结果是受控 WPF 输入/焦点行为复核，不代表真实 Playnite 嵌入或物理键盘走查。

## 本机验证

- 当前代码身份：`8707090901a3ee1e424c8bced9c79a170b9631bc`（`87070909`）。测试源 `R21KeyboardNavigationTraceTests.cs` SHA-256：`E788AD7C559955E83B5ABAC1131009A3231F892EF98BAB3837365CC7B406280E`。
- Windows 11 `10.0.22000.0`；隔离 .NET SDK `8.0.423`；VSTest `17.11.1`；Playnite 插件 `net462`、测试 `net472`。SDK8 只代表本机，不替代后续 SDK10 CI。
- 精确提交身份 Release 测试：`GamePickerKeyboardBehaviorTests` 与 `R21KeyboardNavigationTraceTests`，`8 passed / 0 failed / 0 skipped`。TRX：[`Q24-04-KeyboardTrace-87070909.trx`](tests/Q24-04-KeyboardTrace-87070909.trx)，经脱敏后 SHA-256 `27F2F51B2AF524D842FE4E639A66C44445145E9F2CF854CD3BFB4B8B5BE36223`。
- 测试程序集 SHA-256 `58F0D04F0E03B594075A533377F5C153E6F43BDFE1A2CCCBD74A920DB57C4483`；插件程序集 SHA-256 `769FC49458C68625AECAE7377835CBC9B7BFF754117D44D32932C85F827B0199`。两者 ProductVersion 均为 `0.6.73+8707090901a3ee1e424c8bced9c79a170b9631bc`。
- 因测试路径 `tests/GameSaveCenter.Playnite.Tests/*Keyboard*.cs` 登记在 R00-08 freshness scope，本次只将 R00-08 baseline 更新到实际复跑身份 `87070909`。用该 HEAD 重新生成的 freshness summary 有 14 条、`0 needsRerun`；package identity 为 `not-provided`。完整 summary 与说明在[R00/R01 当前复核包](../R00-R01-CURRENT-MAIN-RECHECK-20261013/README.md)。

## 覆盖与新增门禁

- 选框 6 项行为测试覆盖：没有可见结果时 Enter 保留旧游戏且不关闭；IME composition 未提交时 Enter 不确认；`ImeProcessed` 和方向键不被选框提交逻辑吞掉；选中候选 Enter 后关闭并还焦到当前游戏按钮；Escape 关闭并还焦；搜索清空后焦点回到搜索框。
- 生产焦点轨迹实际加载 Dashboard Shell、Overview、Save、Trainer、Media、Task、Maintenance、Settings 八个入口，使用 WPF `MoveFocus(Next/Previous)` 遍历。测试现会逐个确认当前窗口内所有可见、启用且可 Tab 的 Button、TextBox 与 Selector 都出现在前向和反向轨迹；反向交互控件序列须与前向顺序互逆，并把两条成功轨迹写入 TRX。
- 八个测试视图当前状态共记录 90 个交互目标：Shell 15、Overview 7、Save 9、Trainer 4、Media 21、Task 14、Maintenance 1、Settings 19。所有目标在两方向均可达；遍历没有离开各自入口。轨迹名称、顺序与目标列表在 TRX 内可查。
- 先前断言仅检查每个入口至少两个焦点点、前向轨迹包含至少一个预期名称并且焦点不离开页面；该条件不足以证明所有可见控件可达或 Shift+Tab 顺序正确。本次已在同一生产控件夹具上加了逐目标的正向/反向可达与反向序列断言。

## 未完成条件

- 轨迹来自隔离 STA `Window`；没有启动 Playnite，也没有用 OS 级 Tab/Shift+Tab 注入。各视图没有构造完整业务数据集，行级菜单、动态详情和特定启用状态并非每一种都在夹具中出现；当前证据只覆盖这些默认窗口实际显示的控件。
- 测试验证反向顺序与正向轨迹互逆，但没有按真实屏幕逐页人工核对阅读语义、所有数据态可见命令或六页生产宿主的焦点路线。没有截图，不把视觉列改为通过。Q24-04 自动行为在受控样例下通过；完整页面/Playnite 走查仍未验收，最终状态保持未完成。
- Q24-04 代码提交 `87070909` 及 Q24-02 的 `5fcd6856`、`74815b2b` 尚未推送。#973/run `38429051152` 对旧提交 `bee29749` 在本次页面检查仍显示 `In progress`；待推送后须分别跟踪 SDK8/SDK10 CI 至终态。
