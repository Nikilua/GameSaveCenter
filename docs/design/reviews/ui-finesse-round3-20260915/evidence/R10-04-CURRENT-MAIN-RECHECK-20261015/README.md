# R10-04 快捷键帮助当前 main 复核

更新：2026-10-15；分支 `main`；源码基线 `6cf279238d55fe3d139fa2cb0eb7b09229a7478a`。

## 结论

快捷键帮助已沿用既有 `KeyboardShortcutHelpCatalog`、`FocusWorkspaceSearchCommand` 和实际 `ICommand.CanExecute(null)`。本轮没有重建功能或修改生产代码；在当前 main 上重建并复跑真实 Shell Popup 行为后，R10-04 的受控行为条件已满足。Actions 终态仍待查，因此不据此关闭远端 CI 门禁。

## 现有能力与行为

- 页头帮助按钮点击后实际打开 Popup，显示当前媒体工作区的 `Ctrl+F` 说明；再次点击关闭。
- 当生产 Shell 的搜索命令 `CanExecute(null)` 为 false 时，实际点击帮助按钮仍打开 Popup，但快捷键列表为空。另有 fake command 的启用/禁用目录过滤正负例。
- 当前工作区文案、命令接线和已存在的搜索路由负例继续通过。

## 当前 main 验证

- 本机只安装 .NET SDK `9.0.302` / VSTest `17.14.1`；仓库 `global.json` 固定 SDK `8.0.423`，本机没有该 SDK。本轮从仓库外启动 dotnet 并以隔离输出运行，故这些结果明确标为 SDK9，不能替代 SDK8 基线或 CI SDK `10.0.401`。
- 全解方案 Release restore/build 成功，`0 warning / 0 error`；Playnite `net462`、Playnite tests `net472`、Worker `net8.0-windows` 均在隔离输出目录构建。
- 定向行为合计 `18/18`：快捷键帮助 `5/5`、搜索策略 `2/2`、生产 routed-event 焦点 `1/1`、无障碍源码回归 `3/3`、当前游戏路由 `5/5`、返回导航 `2/2`。各 WPF 类分别进程运行。
- 邻接 R09 焦点轮廓类 `2/2`。这只证明当前这份同源隔离构建通过，不能识别或解释另一台机器的测试源、DLL、PDB 或资源身份。
- XAML 结构检查 `24/24`，`scripts/validate-source.py` 与 `git diff --check` 通过。

脱敏 TRX：

- [快捷键帮助](tests/R10KeyboardShortcutHelpBehaviorTests-sdk9302-6cf27923.trx) — SHA-256 `FD033696BB85F2893BB5B27913EFB919770819B7F72E4C8FDD982C4A717C1BD4`
- [搜索策略](tests/R10SearchShortcutBehaviorTests-sdk9302-6cf27923.trx) — SHA-256 `AC1D7020C79BAECDA5B9E0DA3484781AFA2EEDF1E7D6286883FBBF5394E2FA34`
- [生产焦点路由](tests/R10SearchShortcutFocusBehaviorTests-sdk9302-6cf27923.trx) — SHA-256 `02F9ED23A4DDE3DB93BC8C759AF415E7E776A7FAB373721E58BCA3548C8800AB`
- [无障碍源码回归](tests/AccessibilitySourceTests-sdk9302-6cf27923.trx) — SHA-256 `D9397D9A6EE551511E9CC4DBA8B2D996114DD0253196B9EF98E87BAE16774C8F`
- [当前游戏路由](tests/R10ContextGameBehaviorTests-sdk9302-6cf27923.trx) — SHA-256 `F149840F6770AAF6EBA5ED005AA38A029923C3EDF0C7CAD4879874008A03D05C`
- [返回导航](tests/R10NavigationBehaviorTests-sdk9302-6cf27923.trx) — SHA-256 `4542E7FC819AF89261A64BFC64243328D894B0531A5562AA991541A7F04C2667`
- [R09 焦点轮廓对照](../R09-FOCUS-CURRENT-MAIN-RECHECK-20261015/tests/R09FocusOutlineBehaviorTests-sdk9302-6cf27923.trx) — SHA-256 `9E53FDA1DDE8A0627D6BA51E89D289CCB17C36443C8ABEC0BA8433C7EC283D3C`

构建身份：Playnite DLL SHA-256 `DADDC7807254337358D27FD77ADBA99E83DD8C9C7092C97AEDE165DC618E2B3D`、MVID `969a369a-9fba-49bc-99ce-365bbaf49a30`、PDB SHA-256 `2618959D11B6D7CEE6528B7B4F8FE9488398B9F1C1895DD0EE3E3908D2A174D6`；测试 DLL SHA-256 `A23CD935417BC2C99AF658A3D1350434AD4630D289CDC56ABC7A415DDA968CF4`、MVID `9f5ba334-5c94-4d2b-b55c-456d5ecdd15d`、PDB SHA-256 `A355D305EA8EBE54BDB3172BEFA78BED030ED953DED81936A55D6F041AD4902B`。ProductVersion 均为 `0.6.73+6cf279238d55fe3d139fa2cb0eb7b09229a7478a`。

源码身份：`R10KeyboardShortcutHelpBehaviorTests.cs` SHA-256 `E4E0A118D72D0075B9AFFD9EEE91F335695124EA095297EE846E777F0AC4CFF2`；`KeyboardShortcutHelpCatalog.cs` `0B0F38BFD6C999D80BC21B3742B8276B44968BB1D1266168719ECF842656BE23`；Shell code-behind `736C1554D06C8B67E753F68FBFB0E2F63935F007414BA9F45BB1E29D35AE39C7`；Shell XAML `B9E7A777B84F4AC3DD08FEF69643D7F64D7632466F3DC0271239F8792B756089`。

## 边界与后续

WPF 行为运行于隔离 STA testhost，未运行真实 Playnite；真实宿主 presented frame、物理 DPI/跨屏、UIA/读屏、IME、全局快捷键协作、ETW 和宿主性能仍未验证。没有视觉改动，因此没有把离屏测试写作真实呈现验证。

当前 Actions run ID/终态未能确认：本机代理拒绝匿名 API 请求，已知 run 页面返回 cache miss。SDK9 本机结果不替代 SDK8 或 SDK10.0.401 CI。下一项 R10-05：先检查已有筛选预设模型、持久化和确认/取消行为，再补当前 main 行为证据。
