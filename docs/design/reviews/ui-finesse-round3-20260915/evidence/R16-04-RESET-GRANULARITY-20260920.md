# R16-04 恢复默认粒度证据

日期：2026-09-20  
分支：`codex/ui-finesse-round2`  
代码提交：`2b194461`（补齐设置恢复默认粒度）

## 现状核对与实现

- 最新设置页原来只有 Playnite 整体保存/取消、首次加载 `EnsureDefaults` 和设置导入校验，没有可操作的单字段、单分类或全部默认入口；没有把首次路径补全误记为用户恢复默认。
- 新增 `SettingsResetCatalog`，以四个明确分类登记安全标量字段和默认值；字段选择器只暴露这些字段，不包含 Worker/Ludusavi/Rclone 可执行文件、存档/媒体/镜像路径、Rclone 云端目标或设备身份。
- 设置页新增单字段恢复、四个分类恢复和全部恢复。每个操作先以确认文案列出范围；全部恢复还明确会清理筛选预设、列宽和最近访问等本地界面偏好。
- 重置写入当前 `GameSaveCenterSettings` 编辑对象，再以 `DataContext = null` / 同一对象重绑刷新自动属性；不调用 `EndEdit`、不直接保存、不启动 Worker。Playnite 的取消路径仍能用 `editingClone` 恢复重置前的草稿。

## 自动化证据

- 外部隔离源码副本 Release solution：`0 errors / 2 warnings`。两条 warning 均为既有 `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs:664` 的 `CS8602`。
- 定向测试：`R16SettingsDefaultsBehaviorTests` `2/2`；`R16SettingsDefaultsSourceTests` `1/1`。行为测试实际检查连接字段保留、全部默认值和 Playnite 取消恢复原草稿；源码测试检查三个范围接线和重置方法体不结束编辑。
- `python scripts/validate-source.py` 通过；XAML 结构门禁 `24/24`；`git diff --check` 通过；WPF 技能静态扫描 `0 errors / 28 warnings / 177 info`，没有新增静态错误。
- 构建采用已授权隔离副本的现有 `obj` 资产和 `--no-restore`；源码测试使用复制到临时副本的 linked worktree `.git` 身份文件确认程序集/源码一致。未宣称 fresh restore 通过，临时副本已清理。

## 公共门禁与未验证边界

- 保留 Playnite 保存/取消、命令/Binding、路径校验、取消/错误语义、恢复保护、有限列表和 net462 兼容；重置不会清空工具路径、存档/媒体/镜像路径、云端目标、设备身份或真实存档/云端数据。
- 测试使用普通 settings DTO、隔离目录和源码契约；没有执行真实 Playnite 设置窗口点击、最终主题呈现、UIA/读屏、IME 或物理 DPI 验证。
- 未验真实 Playnite/package-host、最终浅深主题、DPI/跨屏、UIA/IME、RenderHarness presented frame、ETW 或宿主性能；Demo 原目录不可用，继续以恢复生产基线为视觉依据。
- main 工作树现有用户改动和 `src.zip` 未碰、未合并；分支已推送至 `origin/codex/ui-finesse-round2`。

下一可执行任务：`R16-05 路径编辑一致`。先检查设置页已有路径浏览/校验/打开/复制入口、`SettingsPathValidationService` 和权限/网络/不存在负例，再决定复用或补实现。真实宿主呈现、DPI/UIA/IME、ETW 和宿主性能仍未验证。
