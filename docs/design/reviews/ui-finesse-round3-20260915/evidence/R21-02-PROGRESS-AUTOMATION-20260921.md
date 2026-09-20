# R21-02 进度控件名称与值证据

日期：2026-09-21  
代码提交：`74b559e2`（`补充R21任务与远端进度名称`）  
分支：`codex/ui-finesse-round2`

## 本批范围

本批只处理三个已有进度呈现点：

- TaskCenter DataGrid 行进度：继续绑定 `TaskStatusDto.ProgressValue`，补 `任务进度` 名称和 `ProgressDisplay` HelpText。
- TaskCenter 所选任务详情进度：继续绑定 `SelectedTask.ProgressValue`，补 `所选任务进度` 名称和 `SelectedTask.ProgressDisplay` HelpText。
- Maintenance 远端隔离下载进度：继续绑定 `RemoteBackupStageProgress`，保留 `IsRemoteBackupStageActive` 的可见性，补 `远端备份隔离下载进度` 名称和百分比 HelpText。

没有新增服务、DTO、状态投影、命令、取消逻辑或恢复保护；当前游戏选框、滚动条和有限列表实现未改。

## 验证结果

- `R21AutomationValueBehaviorTests`：`4/4` 通过。
- 相关定向筛选：`52 passed / 0 failed / 0 skipped / 52 total`。筛选包含 R21 键盘/无障碍/生产壳层、R05 焦点边界、GamePicker 键盘、R06 任务进度和 R12 远端阶段回归。
- 负例没有由新增源码 `Assert.Contains` 代替：既有 R06 行为覆盖未知/排队、运行中零值、越界钳制、取消与成功终态；既有 R12 行为覆盖远端下载/校验、成功后等待恢复确认、取消/失败以及非远端阶段不受影响。本批只把这些真实状态已有的可读值接到对应 HelpText。
- 当前 source-copy 的 Release 构建目标为 Playnite `net462`、Tests `net472`：`0 errors`；仅保留 `MediaCenterView.xaml.cs:671` 的 2 条既有 `CS8602` warning。
- `python scripts/validate-source.py`：通过。
- `scripts/check-xaml.ps1`：`24/24` 通过。
- `git diff --check`：通过。
- WPF 静态检查：`0 errors / 27 warnings / 177 info`；warning/info 与既有基线一致，未见本批新增诊断。

## 证据边界

证据来自生产 XAML、已有 DTO/远端阶段投影、合成 WPF AutomationPeer、fake/隔离 testhost 和 source-copy。没有运行真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终 presented frame 或宿主性能验证；没有使用代理性能或离屏截图替代这些事实。Demo 原目录不可用，沿用已恢复的生产基线。main 工作区仍有用户改动，本批未碰、未合并。

下一可执行任务：继续 R21-02 剩余复合选择器及逐控件状态/值负例；完成后再进入 R21-03 错误播报。真实宿主 UIA、呈现、DPI/IME、性能仍是未验边界。
