# R10-03 搜索快捷键当前 main 复核

更新：2026-10-14；源码基线 `8822e32c0966794e715aaa943e19b802cf6f75ca`，分支 `main`。

## 结论

R10-03 已有实现满足当前可控行为条件，本轮复用 `563e6862` 引入的 `SearchShortcutPolicy`、Dashboard `PreviewKeyDown` 路由和各工作区现有搜索框，没有重建搜索或导航能力，也没有生产代码变更。

`DashboardView.OnPreviewKeyDown` 只在 Ctrl+F 且无对话框、游戏选框和紧凑浏览器占用输入时执行现有 `FocusWorkspaceSearchCommand` 并设置 `e.Handled`；其余按键不由该路由处理。当前夹具直接验证策略正负例，辅助源码夹具核对生产事件路由和自动化命名。

## 当前 main 验证

- 本机 SDK `9.0.302`，VSTest `17.14.1`；插件目标 `net462`，测试目标 `net472`。仓库 SDK 基线 `8.0.423`；本机 SDK9 结果不替代 SDK8 或 CI SDK `10.0.401`。
- Release 测试项目及项目引用构建成功，筛选 `R10SearchShortcutBehaviorTests|AccessibilitySourceTests|R10ContextGameBehaviorTests|R10NavigationBehaviorTests` 共 `12/12` 通过、0 失败、0 跳过。
- 搜索路由正例：无弹层占用时 Ctrl+F 返回聚焦搜索策略。负例：对话框、游戏选框、紧凑浏览器打开时 Ctrl+F 不路由；Ctrl+Z、Ctrl+C 与无 Ctrl 的 F 不进入搜索聚焦路径。
- 没有 WPF 视觉变更；未运行真实 Playnite、物理键盘/IME、OS UIA/读屏或最终呈现验证。
- NuGet 漏洞数据源在本次 restore 不可达，构建输出有 `NU1900` 源不可用提示；没有 C# 编译错误，测试执行成功。
- 脱敏 TRX：[`R10-03-current-main-sdk9302.trx`](tests/R10-03-current-main-sdk9302.trx)。测试源码 SHA-256：`245ED68698BEB39EAA655A5DC1D7EC6E5D81C5CDF892A6C98198F6C36A891788`；策略源码 SHA-256：`A14CD33D339074DB3A0998C997BC86C548B9886190236DC339B22BE952747D2D`；Dashboard 源码 SHA-256：`DC3A8675610595A370E7FB4BCB7A0C49C78C5A46BDEB84B516DA96B36B6C4C7C`。
- 产物身份：插件 DLL ProductVersion `0.6.73+8822e32c0966794e715aaa943e19b802cf6f75ca`、SHA-256 `325C765A15C2E1D014045C78DA17AE2130C980CD7AA6C5975CD31C4452D2941F`、MVID `1d756dc0-4380-4241-8bdb-c4a4c97f975c`；测试 DLL SHA-256 `C8984DE19675B01D848E6F101EF667CB6666BF5B7A7E57AAFEF3BF105BB8CDE5`、PDB SHA-256 `78A6D9E91390C7887D51ED45EA8D203D3E51F9CBC25E9F25DA3454FDDDB15F39`、MVID `815d1e78-aad4-42f4-bc1c-22adb7b48556`。

## 边界与下一步

这是隔离 net472 testhost 下对生产策略和源码接线的复核，不证明真实 Playnite 全局快捷键协作、IME/物理输入、UIA、物理 DPI/跨屏、presented frame、ETW 或宿主性能。Demo 原目录当前不可用，按项目规则沿用恢复生产基线；未变更游戏选框或滚动条系统。

下一项：R10-04 快捷键帮助，先复核当前命令元数据及禁用命令过滤，再补当前 main 行为证据。
