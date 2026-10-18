# S00-01 只读命令目录

日期：2026-10-18。实现提交：[`dd38f8ca`](../../../../../../src/GameSaveCenter.Playnite/Views/CommandDirectoryCatalog.cs)。

## 现有能力核对

生产 Shell 已有 `KeyboardShortcutHelpCatalog`，当前只展示 Ctrl+F 和当前页面搜索说明；工作区导航主要由 RadioButton 的 `Checked` 事件执行，设置通过已有 `SettingsRequested` 回调打开。项目此前没有统一稳定 ID、双语别名、作用域与动态不可用原因目录。因此新增目录契约，不重复建设业务命令。

目录登记的安全动作是工作区导航、打开插件设置、选择当前游戏、定位当前页面搜索、打开所选任务关联游戏。它们分别复用 Shell 导航命令/设置回调、现有游戏选择入口、`FocusWorkspaceSearchCommand` 和 `OpenSelectedTaskGameCommand`。恢复、删除、清理等动作不在本批目录中。

工作区和任务命令使用稳定 ID。Saves、Trainers、Media 标记为当前游戏作用域；未选择游戏时返回“请先选择当前游戏，再打开此工作区。”并阻止执行。任务关联游戏命令检查当前位于任务中心、有选中任务、任务携带游戏 ID 且该稳定 ID 仍在加载列表中；失败时解释原因，不调用原命令。底层 `ICommand.CanExecute` 在转发前再次检查。

## 验证

- 代码提交：`dd38f8caec8cc6c63080b2d7e1eb587e14e9cde8`（中文提交：`加入统一只读命令目录`）。
- 环境：SDK `9.0.302`、VSTest `17.14.1`；仓库固定 SDK `8.0.423`，本机未安装该 SDK。下列结果不替代 SDK8/SDK10 CI。
- Playnite Tests Release 目标构建成功，未输出编译 warning/error。构建程序集使用 `GscBuildCommit=dd38f8ca`。
- `CommandDirectoryBehaviorTests` 与既有 `R10ContextGameBehaviorTests`：`7/7` 通过，0 失败、0 跳过。覆盖目录稳定 ID/别名/作用域、缺少游戏/任务时阻止执行、绑定参数只转发一次，以及既有任务导航精确稳定 ID / 失效游戏负例。
- `python scripts/validate-source.py .` 通过；`git diff --check` 通过。
- 测试记录：[S00-01-command-directory.trx](tests/S00-01-command-directory.trx)，SHA-256 `EED8C2552F5AAB0F68ABB6BA7D730D27404783ABD1B8CCB2FE958E8A2BD6C677`。
- 测试程序集 SHA-256 `399123919710231FD0E0FCF04DDDCCD27D694655F33FA9E7CFEF78C9EFD81809`。

当前源复核：仓库 SDK `8.0.423` 隔离 Release 解决方案构建成功（0 warning / 0 error）；本轮 `CommandDirectoryBehaviorTests` 独立类 `2/2` 通过。记录见 [SDK8 TRX](tests/command-directory-sdk8423.trx)，SHA-256 `BD2520D0C7BA56CC3FD1428E713B1F66B728FDCC4487FE92FF3D3C710F2653AC`。原 7/7 结果仍是 SDK9 旧证据，未与这次类计数混合。

本批没有修改 XAML 或布局，因此不以离屏图像或静态资源检查声称 Playnite 屏幕呈现通过。命令目录目前是可被后续入口调用的真实数据契约，用户可见搜索面板属于下一项 S00-02；真实 Playnite 宿主运行未验证。
