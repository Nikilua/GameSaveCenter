# R10-03 搜索快捷键证据（2026-09-19）

## 结论

R10-03 在当前续作分支的可控验证范围内已满足。实现提交为 `563e6862`（`补齐搜索快捷键作用域保护`）。本阶段复用现有搜索框、Dashboard 预览键路由和 Shell 游戏选框，没有另建导航或输入体系。

## 实际实现

- `DashboardView.OnPreviewKeyDown` 继续把 Ctrl+F 路由到当前工作区的已有搜索框：Trainers、Tasks、Media、Maintenance 和默认游戏搜索框均沿用既有 `FocusWorkspaceSearch` 路径及控件自动化名称。
- 新增 `SearchShortcutPolicy` 作为最小作用域判断：对话框、Shell 游戏选框或紧凑游戏浏览器打开时不抢占 Ctrl+F，让当前弹层/输入上下文继续拥有键盘焦点。
- Shell 只暴露现有 `PickerOverlay` 的可见状态；没有改变 `OnPickerPreviewKeyDown` 的 IME、方向键、Page/Home/End、Enter、Esc 处理，也没有修改 Playnite 全局键绑定。
- 策略保留编辑语义：Ctrl+Z、Ctrl+C 和不带 Ctrl 的 F 都不会进入搜索聚焦路由。

## 验证

- `R10SearchShortcutBehaviorTests` `2/2`：无占用层时 Ctrl+F 正例；对话框、游戏选框、紧凑浏览器分别为负例；Ctrl+Z/C 和无 Ctrl 的 F 为负例。
- 包含 R10-01/R10-02 与无障碍源码接线的定向回归 `9/9`：测试命令筛选 `R10SearchShortcutBehaviorTests|AccessibilitySourceTests|R10ContextGameBehaviorTests|R10NavigationBehaviorTests`。
- `scripts/build.ps1 -SkipTests -OutputRoot .tmp/r10-03-build`：Release solution `0 warning / 0 error`，Playnite `net462`；XAML `24/24`。`validate-source.py`、`check-xaml.ps1`、`git diff --check` 通过。

## 边界与未验事实

- 证据使用隔离 net472 testhost、真实生产源码接线和纯策略正/负例；没有读取或修改真实存档、媒体、云端、用户配置或诊断数据。
- 尚未在真实 Playnite 中验证全局快捷键协作、真实键盘/IME 输入、弹层焦点转移、UIA/屏幕阅读器、最终 presented frame、物理 DPI/跨屏、ETW、宿主性能或 package-host；没有把离屏/代理结果写成真实呈现或物理跨屏通过。Demo 原目录不可用，沿用恢复生产基线。
- 用户提供的 `DEV-INSTALL-008` main checkout 日志仍独立记录为编译 `0/0`、Core `83/83`、Worker `311/311`，Playnite 全量 `73 failed / 588 passed / 57 skipped`；本阶段未在 dirty main 上覆盖或重跑安装器，也不把该混合宿主失败改写成 R10-03 隔离失败。
- `.tmp/r10-03-build` 已按仓库规则尝试清理，但部分 `bin/obj/test-temp` 文件仍返回 Access denied，疑似被现有 dotnet/testhost 进程占用；未强制终止未知归属进程。该临时目录不纳入提交。

下一可执行任务：R10-04 快捷键帮助；真实 Playnite 输入、弹层焦点、呈现和安装器边界仍未验。
