# R10-03 当前 main 搜索快捷键行为复核

更新：2026-10-15。本机源码基线 `95109c221ec85a52c61c51c0a119a2f3d97efacf`（构建期间含本阶段工作树改动），分支 `main`。

## 结论

现有 `SearchShortcutPolicy`、Dashboard `PreviewKeyDown` 和 `FocusWorkspaceSearchCommand` 已覆盖快捷键策略与工作区搜索目标。本阶段发现此前只有策略正负例及源码接线断言，没有直接观察生产路由执行命令后的 WPF 键盘焦点；因此把同一生产路由提取为 `DashboardView.TryHandleWorkspaceSearchShortcut`，并把既有工作区搜索目标解析放入 `FocusWorkspaceSearchTarget`，没有改变快捷键条件、命令、焦点映射或页面视觉。

新增 `R10SearchShortcutFocusBehaviorTests.CtrlFFocusesTheSearchTargetAndDoesNotConsumeOverlayOrEditingInput` 使用实际 `MediaCenterView` 的“当前游戏媒体”页签和 `MediaSearchTextBox`，创建 STA WPF Window 并派发 `PreviewKeyDown` routed event。正例验证 Ctrl+F 执行生产 Shell 的 `FocusWorkspaceSearchCommand`，焦点从触发按钮移到当前工作区搜索框，事件标记为 Handled。六类负例覆盖对话框、游戏选框、紧凑浏览器占用，以及 Ctrl+Z、Ctrl+C、未修饰 F；每项均要求命令不执行、事件未处理、焦点保留在触发按钮。其他工作区的搜索目标名称沿用原有解析映射；本次实际焦点行为选取 Media 页面验证，其余映射由既有源码门禁覆盖。

## 验证

- 本机 SDK `8.0.423`、VSTest `17.11.1`；Playnite `net462`、测试 `net472`。测试项目及引用 Release 构建 `0 warning / 0 error`；本阶段 R10 相关行为组 `13/13`，R09 焦点轮廓独立回归 `2/2`。分项结果见 [`R10-03-current-main-sdk8423.trx`](tests/R10-03-current-main-sdk8423.trx) 和 [R09 当前 main TRX](../CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/tests/R09FocusOutlineBehaviorTests-sdk8423-main-95109c22.trx)。这不是完整 solution build，也不替代 CI SDK `10.0.401`。
- `scripts/validate-source.py` 通过；`git diff --check` 通过。WPF 静态扫描为 `0 errors / 29 warnings / 177 info`；报告项来自仓库现有 XAML 与其他 `.tmp` 页面，本阶段没有修改 XAML、共享样式或资源字典。
- 测试程序集 ProductVersion `0.6.73+95109c221ec85a52c61c51c0a119a2f3d97efacf`、SHA-256 `82F254D315FC321149660F564A291EE105D476657E34F73D93AADFE08B388FF4`、MVID `87452ef3-7a89-44c1-81a4-afabda485a79`；PDB SHA-256 `1AD27C066FDDFD5A8988057D34F7656AA6E4CA1787825F01BF172957C716D7C9`。
- 插件程序集 SHA-256 `9393C5258058D684B330048351172C7EC0202E5755721B62231BF3F9EC2F75BB`、MVID `d05bfaa3-0633-47a7-98c6-fdc07c8302c1`；PDB SHA-256 `36528A42E8A7425D01397BD0811C4A273DB2A6DB997222B3706295D9203C06C8`。
- 新测试源 SHA-256 `8CD034A2FF467B9E45581E139D4AC27B47FA6A1BFFCF643E2FF6CB025CA5080E`；Dashboard 源 SHA-256 `DA7C53B9910A25B7A81AA8173E6B8F5C91B36D9D7D2B00E22D43748FD7ED3DDA`；未改策略源 SHA-256 `A14CD33D339074DB3A0998C997BC86C548B9886190236DC339B22BE952747D2D`。

## 未验边界与继续点

这是合成按键修饰符、隔离服务和受控 STA WPF Window 的行为验证，不代表物理 Ctrl 键/IME、Playnite 全局快捷键协作、UIA/读屏、Playnite 最终呈现、物理 DPI/跨屏、ETW 或宿主性能。没有访问真实游戏存档、媒体或云端。Demo 原目录不可用；本阶段没有视觉改动，继续沿用恢复生产基线。

提交 `95109c2` 的 Actions run `38645600070` 上次观察为 In progress；本轮复查受网络限制：GitHub API 连接被拒绝（本机代理 `127.0.0.1:9`），`gh` CLI 不存在，Actions 浏览器页读取超时。该 run 当前终态以及本阶段推送后新 run 的终态都待核实；SDK8 本机通过不能签收 CI 门禁。下一项：R10-04 快捷键帮助，复用当前命令元数据并补命令可用/禁用行为。
