# R10-06 筛选来源提示证据（2026-09-19）

## 结论

R10-06 在当前续作分支的受控验证范围内已满足。实现提交为 `5403d797`（`补充筛选来源提示与清除操作`），后续文档同步提交会单独记录本证据。先复用 R10-01 已有的会话导航目标、任务筛选属性和查询失效路径，没有新增服务、DTO、导航栈或设计体系。

## 实际实现

- 任务导航已有 `taskNavigationGameId/taskNavigationGameName` 会作为临时游戏条件叠加到任务服务查询和本地 `ICollectionView`。本阶段公开其状态摘要，并在任务筛选工具栏下方用现有 `GscDiagnosticHintBubble` 显示“已带入游戏”的来源条件。
- 来源提示有真实 `HasTaskNavigationTarget` 正/负可见性：没有带入条件时整块折叠，有条件时展示游戏名、保留其他筛选的说明，以及带有 UI Automation 名称的“清除带入条件”按钮。
- `ClearTaskNavigationContextCommand` 只清除临时导航游戏 ID/名称，然后沿用现有 `RefreshTasksView`、任务请求失效/取消和有限列表刷新路径；搜索、状态、游戏下拉、类型、历史范围等编辑草稿不被改写。普通“清除全部任务筛选”则一并清除该临时条件，避免清空后仍被隐式限制。
- 命令通过现有 `RelayCommand`/`CanExecute` 接入，未改游戏选框、滚动条系统、命令绑定、取消/错误语义、恢复保护、Playnite/net462 或有限列表性能。

## 验证

- `R10FilterSourceBehaviorTests`：`2/2`。真实构造 `TaskCenterView` 和隔离 STA `Window`，正例检查来源提示、实际绑定文本、按钮 `CanExecute/Execute` 与保留搜索/状态草稿；负例切换无来源状态后检查提示折叠且命令不可执行。源码行为检查确认专用清除方法没有写入其他筛选字段。
- 完整 R10 集合：`16/16`；此前 R10-01～05 行为仍通过，新增 R10-06 正/负绑定行为没有引入回归。
- 最终隔离 `scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot .tmp/r10-06-build-20260919`：Playnite `net462`，solution `0 warning / 0 error`，XAML `24/24`。最后布局微调后的构建与完整 R10 `16/16` 已重跑；`validate-source.py`、`check-xaml.ps1`、`git diff --check` 通过。
- 另一次按协议执行的完整 `scripts/build.ps1` 在进入测试前因 C: 磁盘空间耗尽失败，日志是 Worker/生成的既有依赖和 XAML 中间文件无法复制/写入；这次没有获得新的全量 Core/Worker/Playnite 测试计数，不把该环境失败混写成 R10-06 失败。R10-05 已记录的既有全量 Playnite testhost `84 failed / 610 passed / 57 skipped` 仍是独立边界。

## 边界与未验事实

- 验证使用合成绑定状态、隔离 STA WPF Window、net472 testhost 和仓库源码接线；没有读取或修改真实存档、媒体、云端、用户配置，也没有对外发送诊断。
- 未在真实 Playnite 中验证诊断跳转后的最终呈现、清除按钮点击、真实任务服务返回、UI Automation/读屏、IME、物理 DPI/跨屏、presented frame、ETW、宿主性能或 package-host 安装。Demo 原目录不可用，仍以恢复生产基线为视觉依据。
- 用户提供的 DEV-INSTALL-008 main 日志继续单独记录：编译 `0/0`、Core `83/83`、Worker `311/311`、Playnite `73 failed / 588 passed / 57 skipped`，安装器退出 `1`；本阶段没有在 dirty main 上覆盖、合并或重跑安装器。
- `.tmp/r10-06-full-20260919` 已删除；`.tmp/r10-06-build-20260919` 的可删除部分已清理，仅 `test-temp/VBCSCompiler/AnalyzerAssemblyLoader` 下被长驻编译器锁定的 DLL 和父目录仍在，未强杀未知进程。临时产物未提交。

下一可执行任务：R10-07 侧栏信息密度。真实 Playnite 清除交互、呈现帧、物理 DPI/跨屏、UIA/读屏、IME、ETW、宿主性能和 package-host 仍未完成验证。
