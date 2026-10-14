# R10-02 当前 main 复核（2026-10-14）

## 结论

复用 `TaskSourceNavigationResolver`、`TaskStatusDto`、`GamePickerViewModel` 和 Shell 当前游戏选框。R10-02 没有新增按名称导航或第二套选框；本轮补齐生产任务命令的执行/失效负例，并直接验证重名选框不会错选。代码提交：`13704e75234e7f8d943e15ae8380a728a277375e`（基线 `a94819b91f5ac796d8dd99d5320b9b79bf73780c`）。

`DashboardViewModel.RequestWorkspaceLoad` 增加默认空值的内部审计回调，测试可记录路由后请求了哪个工作区，而不启动 Worker、读取用户数据或改动正常生产调度。生产任务命令本身未改。

## 行为验证

- `R10ContextGameBehaviorTests`：`5/5`。实际 `OpenSelectedTaskGameCommand` 在两个同名游戏中按任务的 `GameId` 选择正确对象、切到 Saves 并记录“返回任务”目标；目标 ID 已从快照移除时保留当前游戏/工作区，不增加返回历史、不请求新工作区并显示刷新提示。真实选框 ViewModel 的正例按稳定 ID 选中目标；负例在同名游戏的目标 ID 缺失时保持未选中。
- 相邻导航组：`25/25`，0 失败、0 跳过：R10 context `5`、R10 navigation `2`、purpose navigation `4`、finding navigation resolver `11`、R15 task source navigation `3`。
- R10 context 中 Media/Shell 的源接线检查仅作补充；本轮没有声称运行真实媒体后端或真实 Playnite 视觉界面。
- `scripts/validate-source.py`、`git diff --check` 通过。没有 XAML 或视觉变更，因此不生成截图。

## 构建身份

- 本机隔离 SDK：`9.0.302`；VSTest：`17.14.1`。仓库 `global.json` 锁定的 `8.0.423` 未安装。本次从仓库外工作目录运行 dotnet，并以 `--no-restore`、隔离 `GscBuildOutputRoot` 构建，避免改动仓库 SDK 配置。
- Playnite 项目目标 `net462`；测试项目目标 `net472`。这不是 CI SDK `10.0.401` 的替代结果。
- 生产程序集：SHA-256 `947EABD0C9B2EA3F89C31B866553512EDB86F01C7CD9926457A5B88751E6E7F8`，MVID `942d6cca-0462-49ba-a604-37155a0505e8`。
- 测试程序集：SHA-256 `6FDCCA3DFD92B8D9822E5EC603C8D14CFE920F340E17944674B5AE86A13C56AD`，MVID `e34d67a5-6dec-4a09-9cb0-a97f29abf1c8`。ProductVersion 带基线 `a94819b`，因此源码和程序集分别用 SHA/MVID 标识。
- 关键源码 SHA-256：`DashboardViewModel.cs` `6C9A40DDC8AD762491CB3DD36FEA955AA957E69C1B6EA35E87FF350259377544`；`R10ContextGameBehaviorTests.cs` `A21B41C3B5726CA971225D9F88A2F60B5F2E402C74540DE0B90F181F8383E0F7`。构建后它们已包含于 `13704e75`。
- 脱敏 TRX SHA-256：定向 `F8B2717CD85FE04BB20B1EF075FD510A2A09A1DF78BC38B0356F3F5FBFBDE204`；相邻 `25CFF99E71AC839B67CFF081AA10E8425034F17F3DE400A43B720B6430745E9B`。保留用例结果与计数，机器、用户、绝对根目录已替换为通用值。

## 边界和后续

测试使用合成 `GameStatusDto`/`TaskStatusDto`、隔离 testhost 与路由回调；未读取或修改真实存档、媒体、云端、配置或诊断数据。未运行真实 Playnite，未验证最终 presented frame、物理 DPI/跨屏、OS UIA/读屏或宿主性能。Demo 原目录不可用；本轮没有视觉改动，沿用恢复生产基线。

本机验证完成，阶段仍等待本次推送对应的 SDK8/SDK10.0.401 Actions 终态；若 CI 失败，先取完整 job log 再按实际错误复现。此前指定 run `36677346317`（#920，提交 `77de7f4`）现在显示 Failure、总耗时 7m04s、无 artifacts，页面要求登录查看 logs；公开信息只有构建/测试/打包步骤 exit 1，根因和实际 runner SDK 未知，不以此猜改当前代码。

CI 收口后下一项为 R10-03 搜索快捷键：先查现有命令与焦点/弹层作用域行为，复用已有实现并补真实行为正反例。
