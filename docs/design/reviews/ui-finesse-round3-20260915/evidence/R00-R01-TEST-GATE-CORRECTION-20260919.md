# R00/R01 合并后门禁纠偏证据

日期：2026-09-19  
实现提交：`c975e16d`（`收口Playnite测试隔离与断言漂移`）  
前置分支提交：`c61b6b5f`

## 触发事实

- 用户提供的 main `artifacts/one-click-install.log`（DEV-INSTALL-008）显示 Release 构建 `0 warning / 0 error`、Core `83/83`、Worker `311/311`，但 Playnite 为 `73 failed / 588 passed / 57 skipped`，安装器退出 `1`，因此没有进入打包或安装。
- 首个失败为 `WpfUiResourceDictionaryTests.SaveWorkspaceKeepsAllPrimaryCommandsReachableAtHighDpi`。失败断言把 `Header`、帮助描述属性和 `Binding` 当作固定连续字符串；当前 XAML 在列之间插入 `infra:DataGridColumnHeaderHelpBehavior.Description` 后，断言失效。这只能证明源码形状断言过期，不能证明命令真的不可达。

## 本阶段修正

- 将时间列、可信度列、过滤器数量与任务预设等门禁改为读取 XAML 元素/属性关系；不以新增宽泛 `Assert.Contains` 代替交互验证。
- 修正 Save/R02、布局、字体、工作区状态等与最新代码契约漂移的测试；保留正常文本、图标语义、负例和有限列表约束。
- 新增 `scripts/run-playnite-tests-isolated.ps1`：发现 65 个 source 类和 84 个 WPF 类，source 组一次运行，每个 WPF 类使用独立 testhost；当提供 `OutputRoot` 时，测试临时目录固定在输出目录下，避免共享 STA/Dispatcher 和 C 盘临时目录污染。`scripts/build.ps1` 已接入该门禁。
- `AcrylicProductionShellView` 的导航返回按钮增加早期 namescope 空值保护；R08 页面切换夹具显式初始化由无构造器对象跳过的导航栈。动效测试改用有界 Dispatcher 状态等待，固定睡眠仅保留为负例边界。

## 验证

验证使用 continuation worktree 的 D 盘隔离源码副本 `D:\workplace\github\GameSaveCenter\.tmp\r00-r01-source` 和 `build3` 输出，没有触碰 dirty main：

- `scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot ...`：XAML `24/24`；Contracts/Core/Worker/Playnite `net462`/测试 `net472` Release solution `0 warning / 0 error`。
- `run-playnite-tests-isolated.ps1`：source `65` 类组和 WPF `84` 个独立进程全部返回 `0`；其中 `WpfUiResourceDictionaryTests` 为 `137 passed / 39 skipped / 0 failed`，总计 `176`，原 `SaveWorkspace...` 已通过。
- Core：`84 passed / 0 skipped / 0 failed`；Worker：`322 passed / 1 skipped / 0 failed`，总计 `323`。
- 当前 continuation worktree 的 `python scripts/validate-source.py`、`scripts/check-xaml.ps1` 和 `git diff --check` 均通过；PowerShell 门禁脚本已补齐 Windows PowerShell 5.1 所需 UTF-8 BOM。
- 提交 `c975e16d` 已推送到 `origin/codex/ui-finesse-round2`。

## 真实边界

- 本阶段没有执行 package/Playnite host 安装，也没有把 WPF 独立 testhost、离屏逻辑 DIP 或测试进程隔离写成真实宿主呈现、物理 DPI/跨屏、UIA/读屏、IME、presented frame 或宿主性能证据。
- 未执行真实 Ludusavi/rclone/Worker IPC，未读取或写入真实存档、媒体、用户云端，也未对外发送诊断；未绕过被拒绝的 ETW/系统跟踪权限。Demo 原目录不可用，仍沿用恢复生产资源基线。
- main 的 `DashboardView.xaml.cs`、`src.zip`、Dialog/R08 用户文件保持未触碰；main 的失败日志仍按上述事实独立保留，不能用本分支验证反写为 main 已安装通过。

下一可执行任务：先核对最新实现与依赖，再推进 R11-08 历史时间导航；保持真实数据隔离和当前游戏选框、滚动条、命令绑定、取消/错误/恢复保护及 net462 兼容边界。
