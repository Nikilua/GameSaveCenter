# R10-02 当前 main 行为复核（2026-10-15）

## 结果

复用现有 `DashboardViewModel.OpenSelectedTaskGameCommand`、`TaskSourceNavigationResolver`、`GamePickerViewModel` 和 `AcrylicProductionShellView` 顶部游戏选框。生产实现未改；本轮仅增加一条 STA WPF 行为测试，验证生产 Shell 按钮打开选框、两个同名游戏中选择指定对象、选框关闭并将焦点交还入口按钮，最终游戏仍是目标 `PlayniteId`。

锁定 SDK `8.0.423` / VSTest `17.11.1`，Release 构建 Playnite 测试项目及其引用项目成功，`0 warning / 0 error`。定向运行 `GamePickerKeyboardBehaviorTests` `7/7`、R10 context `5/5`、相邻导航/来源 `20/20`，共 `32/32`，0 failed / 0 skipped。XAML 结构检查 `24/24`、`validate-source.py` 与 `git diff --check` 通过。这是隔离测试项目与引用构建，不是完整 solution build，也不是 CI SDK10.0.401 结果。

## 行为覆盖

- 生产 `OpenSelectedTaskGameCommand` 在两个同名游戏中按任务 `GameId` 定位准确对象、切换 Saves 工作区、建立“返回任务”目标并请求一次工作区加载。
- 任务游戏 ID 已从当前快照移除时，保留当前选中游戏和 Tasks 工作区，不建返回历史、不加载新工作区，并显示刷新提示。
- 生产 Shell 的当前游戏按钮打开选框并把焦点送到搜索框；选中第二个同名游戏后，选框关闭、焦点返回按钮，`SelectedGame.PlayniteId` 和对象引用均指向该目标，当前游戏名绑定显示“同名游戏”。
- `GamePickerViewModel` 正例按稳定 ID 选中同名目标；目标 ID 缺失时保持未选中，不能回退到同名对象。
- 相邻测试覆盖导航快照恢复、无效滚动值、查找目标缺失、任务来源/备份身份精确解析和 Shell/页面来源接线。字符串接线断言只作补充，不替代以上行为用例。

## 构建身份

- 测试源码基线 HEAD：`6eac7b90bbbc0511ef3ed942422ef0b10b54a03b`；本轮只改测试源码，R10 生产导航、GamePicker、Shell XAML 与相关生产文件相对 `13704e75` 未变。
- 测试项目目标 `net472`；Playnite 目标 `net462`；测试运行时为 .NET Framework `4.8.9181.0`。
- 测试程序集 ProductVersion：`0.6.73+6eac7b90bbbc0511ef3ed942422ef0b10b54a03b`；SHA-256 `43DE0B8B37CB5B590D7948B36D8601282028BA7B1B217ED7B8D11480E71DCAFB`；MVID `c688ebe6-0e46-4458-a7b6-49a098c3fda8`；PDB SHA-256 `3A5E771DD478674866C75A7BD994BE6BB35855BD78F725E688BE34B8FD94CD25`。
- Playnite 程序集 SHA-256 `0B120490D7173C7EFA0D3A46C37338301BE751D89667C61BE80333DE6DA89758`；MVID `3246aa28-4d34-4126-9a85-37d74de69c57`；PDB SHA-256 `E596E5A69AAC8B4D29BCE7CBA2C5FCE2C0CDD04DCEA2AA119D4ADBD0F3B6B9DF`。
- 关键源码 SHA-256：`R10ContextGameBehaviorTests.cs` `A21B41C3B5726CA971225D9F88A2F60B5F2E402C74540DE0B90F181F8383E0F7`；`GamePickerKeyboardBehaviorTests.cs` `0598FA9AF18685A97EC725C74D858826CEAEFAD8CEC74D6049391233EE0915C6`；`DashboardViewModel.Navigation.cs` `C95DCC3A544320CAC91DF6688EEF42E0C0A4A8A02657621616CB3F7BAE5CCD62`；`GamePickerViewModel.cs` `FFB9DDB390490C67E6DFE210F8C496FF55F02947D63BA5DF3C5A1111DEA4548B`；`AcrylicProductionShellView.xaml` `B9E7A777B84F4AC3DD08FEF69643D7F64D7632466F3DC0271239F8792B756089`。

脱敏 TRX 位于本目录 `tests/`：R10 context `5`、导航快照 `2`、来源接线 `4`、Finding resolver `11`、R15 task source `3`、生产 GamePicker 键盘/UI `7`。TRX 已移除机器名、用户名和部署路径，保留用例名与结果。

TRX SHA-256：`R10ContextGameBehaviorTests.trx` `8BA3926544944498C2A25ACAEF96462471DBD5DA560C6A3E1B9EC774F0B89C7D`；`R10NavigationBehaviorTests.trx` `7906086BEFE4C96CDBFE1E8F524A63DC70D5EB227CD7FB391ECA5C9041CA6FBE`；`PurposeNavigationSourceTests.trx` `FC9AA4830A20E5415B4AB67D2D298C29F00055E7F26B8694EEB16BFD801B9916`；`FindingNavigationResolverTests.trx` `8611A043AF6A1CE88D132FEC8B25FEA62E76FDD77A61187EE1567D713B165874`；`R15TaskSourceNavigationTests.trx` `F01EF09E0312AA591BB85EB32C7D8BCFCCEBA63D1A5C1F1F355C2A8B15BE0433`；`GamePickerKeyboardBehaviorTests.trx` `42A53B36E5299A5DCE5B0A47E094CBCA3D7E501FD3AE138DEB287EFD8135771F`。

## 未验证边界与 CI

选框点击/列表选择是在隔离 STA WPF 窗口中合成触发；未运行真实 Playnite、真实媒体后端、真实鼠标/键盘/IME、外部 UIA/读屏、物理 DPI/跨屏或最终呈现帧。任务命令通过隔离工作区加载回调验证，没有启动 Worker，也没有读取真实用户数据。

本机 SDK8 的 `32/32` 不能替代 SDK10.0.401 CI。此前 #990 对提交 `8822e32c` 的两个 CI lane 曾失败，公开日志 API 返回 403，根因未知；那不是本轮 `6eac7b90` 测试身份的 CI 结果。`6eac7b90` 及本轮证据提交对应的 Actions run ID/终态仍需查询，未把 CI 标为通过。下一项 R10-03 搜索快捷键：复核当前 main 的焦点与弹层作用域，并沿用现有实现补当前身份行为证据。
