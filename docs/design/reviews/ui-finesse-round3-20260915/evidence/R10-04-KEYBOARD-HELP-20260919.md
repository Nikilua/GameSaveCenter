# R10-04 快捷键帮助证据（2026-09-19）

## 结论

R10-04 在当前续作分支的可控验证范围内已满足。实现提交为 `19be9f12`（`补充快捷键帮助入口`）。本阶段复用现有 `RelayCommand`、`CanExecute`、当前工作区和 R10-03 的搜索路由，没有虚构未接线的全局快捷键。

## 实际实现

- Shell 页头新增带 Tooltip 和 Automation Name 的“键盘操作”信息入口；点击后打开现有 Demo-first 浮层样式的 Popup，再次点击关闭，保留键盘焦点和选框系统的既有路由。
- `KeyboardShortcutHelpCatalog` 以真实 Shell `FocusWorkspaceSearchCommand` 为命令元数据来源，按 `DashboardViewModel.CurrentWorkspace` 生成当前页说明。当前实现只公布已经接线的 `Ctrl+F`，不把 F5、Playnite 全局键或未实现的动作写成可用快捷键。
- 目录在呈现前调用 `ICommand.CanExecute(null)`；禁用命令直接不生成条目。Ctrl+F 的执行仍委托给既有 Dashboard `FocusWorkspaceSearch`，各页搜索框与 R10-03 的对话框/选框/紧凑浏览器作用域保护保持不变。

## 验证

- `R10KeyboardShortcutHelpBehaviorTests`：目录按 `CanExecute` 过滤的正/负例、当前工作区文案和生产接线检查；真实 STA WPF Window 点击 Shell 帮助按钮后，Popup 实际打开并显示“媒体中心 / Ctrl+F”，再次点击关闭。
- R10-03/R10-02/R10-01 相邻定向回归合计 `13/13`：R10 帮助 `4/4`、R10-03 `2/2`、Accessibility 源码回归 `3/3`、R10-02 `2/2`、R10-01 `2/2`。
- `scripts/build.ps1 -SkipTests -OutputRoot D:\workplace\github\GameSaveCenter\.tmp\continuation-r10-04-build-final-20260919`：Playnite `net462`，solution `0 warning / 0 error`；XAML `24/24`。`validate-source.py`、`check-xaml.ps1`、`git diff --check` 通过。D: 隔离输出已按精确路径清理。

## 边界与未验事实

- 交互证据使用隔离 STA Window、真实生产 Shell/XAML 和合成的未初始化 ViewModel 工作区状态；没有读取或修改真实存档、媒体、云端、用户配置或诊断数据。
- 尚未在真实 Playnite 中验证 Popup 的最终 presented frame、物理 DPI/跨屏、UIA/屏幕阅读器、真实键盘/IME、全局快捷键协作、ETW、宿主性能或 package-host；没有把逻辑/离屏 STA 结果写成真实呈现或物理跨屏通过。Demo 原目录不可用，沿用恢复生产基线。
- 用户提供的 `DEV-INSTALL-008` main checkout 日志仍独立记录为编译 `0/0`、Core `83/83`、Worker `311/311`，Playnite 全量 `73 failed / 588 passed / 57 skipped`；本阶段未在 dirty main 上覆盖或重跑安装器，也不把该混合宿主失败改写成 R10-04 隔离失败。
- C: 盘曾仅剩约 `0.21 GB`，导致一次外部隔离构建空间不足；最终构建改用 D: 盘并成功。旧 VBCSCompiler 临时文件仍可能被长驻 dotnet 进程锁定，未强制终止未知进程，未将临时输出纳入提交。

下一可执行任务：R10-05 筛选预设；真实 Playnite 帮助交互、呈现和安装器边界仍未验。
