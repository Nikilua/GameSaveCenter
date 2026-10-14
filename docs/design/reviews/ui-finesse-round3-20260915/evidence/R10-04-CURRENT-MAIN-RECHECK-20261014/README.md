# R10-04 快捷键帮助当前 main 复核

更新：2026-10-14；代码基线 `e3a127534f9418204be9f4716e7d085a5403dfa9`，分支 `main`。

## 已有实现与本轮补强

生产功能自 `19be9f12` 起已存在：帮助目录使用 Shell 实际的 `FocusWorkspaceSearchCommand` 和 `ICommand.CanExecute(null)`，依当前 `DashboardViewModel.CurrentWorkspace` 生成已接线的 Ctrl+F 条目。页头按钮会刷新目录并实际开关 Popup。当前 Shell/XAML 后续有布局等变更，但 R10-04 的目录、命令及 Popup 源文件沿用既有实现。

审阅历史夹具后，发现“命令不可执行不显示”原先分别覆盖了目录 helper 的 fake-command 负例和 Shell Popup 的启用正例，缺少由实际 Shell 命令禁用状态穿过 Popup 更新的负例。本轮只新增一项行为测试：保留 Shell 搜索回调为 null，先确认真实 `FocusWorkspaceSearchCommand.CanExecute(null)` 为 false，再点击帮助按钮，验证 Popup 打开但条目列表为空；生产代码与 XAML 未改动。

## 当前 main 验证

- 本机 SDK `9.0.302`、VSTest `17.14.1`；Release 插件 `net462`、测试宿主 `net472` 和项目引用构建成功，0 warning / 0 error。
- 定向筛选 `R10KeyboardShortcutHelpBehaviorTests|R10SearchShortcutBehaviorTests|AccessibilitySourceTests|R10ContextGameBehaviorTests|R10NavigationBehaviorTests`：`17/17` 通过、0 失败、0 跳过。分组：帮助 `5/5`、快捷键策略 `2/2`、无障碍源码回归 `3/3`、当前游戏路由 `5/5`、返回导航 `2/2`。
- 实际 STA WPF Shell 正例点击帮助按钮后 Popup 打开并展示当前媒体页的 Ctrl+F，再次点击关闭；禁用负例使用生产 Shell 命令回调的真实不可执行状态，确认 Popup 不显示 Ctrl+F。目录 fake ICommand 正/负例与各工作区说明也通过。
- 脱敏 TRX：[`R10-04-current-main-sdk9302.trx`](tests/R10-04-current-main-sdk9302.trx)。新增测试文件 SHA-256：`E4E0A118D72D0075B9AFFD9EEE91F335695124EA095297EE846E777F0AC4CFF2`；目录源码 SHA-256：`0B0F38BFD6C999D80BC21B3742B8276B44968BB1D1266168719ECF842656BE23`；Shell 源码 SHA-256：`736C1554D06C8B67E753F68FBFB0E2F63935F007414BA9F45BB1E29D35AE39C7`；Shell XAML SHA-256：`B9E7A777B84F4AC3DD08FEF69643D7F64D7632466F3DC0271239F8792B756089`。
- 测试 DLL 编译时 `GSC_BUILD_COMMIT` 为基线 `e3a12753`；新增测试源的上述 SHA-256 固定了本次工作树输入。插件 DLL SHA-256 `5F6ED6F46DDB711BA11F003724218541068C2AAAE44AC1387451FF7EDC250074`、MVID `474830e8-c142-4c9b-a312-edc7d9f94f05`；测试 DLL SHA-256 `5E247493395848E21B205B255985AE6A3457E07DF0C023AA4BCDC9D30F467428`、MVID `819ba8c5-5197-4b16-a0be-73bf3acaf7fd`；测试 PDB SHA-256 `02692F74011EA366B4A9A730DDD4333A51C7CDC4D78BBFEA3CAC09A4F6EE4EB2`。
- 无生产视觉改动；本次只验证隔离 STA 控件行为与真实生产 Shell/XAML 接线，没有运行真实 Playnite。

## 边界与下一步

隔离 net472 testhost 和 STA Window 不证明真实 Playnite 最终 presented frame、物理 DPI/跨屏、UIA/读屏、真实键盘/IME、全局快捷键协作、ETW 或宿主性能。Demo 原目录不可用，沿用恢复生产基线；没有改动游戏选框或滚动条系统。SDK9 本地结果不替代仓库 SDK8.0.423 或 CI SDK10.0.401。

下一项：R10-05 筛选预设；先复核已有持久化模型与确认/撤销行为，再补当前 main 行为证据。
