# R12-01 恢复流程分步摘要证据

日期：2026-09-19  
分支：`codex/ui-finesse-round2`  
实现提交：`e1a8da0c`（补齐恢复流程分步状态）

## 完成内容

- 先复用已有 `BackupVersionDto.RestoreReadiness`、`TaskStatusDto`、`ValidateRestoreReadinessCommand` 和 `RestoreCommand`，没有新增恢复服务、DTO、Worker IPC 或另一套安全检查。
- 新增固定四阶段状态模型：选择版本、可恢复性检查、目标核对、执行结果。每一阶段都有 pending/active/complete/warning/failed/cancelled 状态、用户可读说明和当前阶段标记；固定四项是有限状态列表，不改变现有滚动条和列表性能策略。
- 真实恢复调用仍沿用原来的确认框、请求 DTO、超时、取消和错误传播；开始写入前的 `PreRestore` 创建/锁定边界在执行阶段说明中可见。失败时保留当前阶段、任务状态、错误信息和回滚提示，不用成功通知覆盖失败。
- 选择版本变化或版本离开当前集合时重置摘要；同一版本的详情刷新不重建来源集合，也不改变 A/B 比较选择、命令绑定或游戏选框。
- `SaveCenterView.xaml` 只在现有恢复操作附近增加分步摘要卡，复用现有主题资源和页面 `ScrollViewer`，没有改成新的设计体系。Demo 原目录不可用，视觉判断沿用恢复生产基线。

## 行为验证

- 新增 `R12RestoreWorkflowBehaviorTests` 6/6：无检查时执行保持 pending；可恢复性 warning 与执行中明确显示 `PreRestore` 安全边界；游戏运行时目标核对失败会停在目标阶段；恢复后校验失败保留执行失败/回滚详情；成功任务完成四阶段；XAML 同时保留原恢复命令和四阶段 `ItemsControl`。
- 负例不是 `Assert.Contains` 签收：测试分别检查阶段状态、当前阶段、失败阶段、错误/回滚文本及目标阶段未进入执行；输入均为合成 DTO/fake 和隔离目录，没有读取或写入真实存档、媒体、云端或对外发送诊断。
- 相邻回归：`R11HistoryTimeNavigationBehaviorTests`、`R06SortingBehaviorTests`、`R11VersionSummaryBehaviorTests`、`R11ProtectionBehaviorTests` 与 R12 合计 `17 passed / 0 failed / 0 skipped`。
- `WpfUiResourceDictionaryTests`：`137 passed / 39 skipped / 0 failed`，总计 176；这直接覆盖此前安装日志中报错的测试类，当前分支没有再现该失败。
- 当前提交的隔离 Release solution build：`0 warning / 0 error`；`scripts/validate-source.py`、`scripts/check-xaml.ps1`、`git diff --check` 和 WPF 静态审查均通过。WPF 静态审查结果为 `0 error / 24 warning / 177 info`，提醒是既有外层布局/共享颜色规则。

## 离屏视觉门禁与边界

- 当前提交 `e1a8da0c` 绑定的 `scripts/render-qa.ps1` 已完成双主题、多尺寸和滚动/缩放探针，但脚本真实退出 `1`。报告中的失败是既有离屏基线：Overview 空列表 2 DIP、Task/Save 小窗口可读行数、Settings 状态夹具，以及 Shell header/Media 几何；未出现恢复流程卡片、恢复命令或本阶段新增的断链项。
- 渲染报告明确写入 `EvidenceSource=OffscreenRenderHarness`、`WorkingTreeClean=True`、`DpiScale=1.00 (offscreen logical DIP; real host DPI is not inferred)`。它只作布局/资源回归参考，不代表真实 Playnite 嵌入、物理 DPI/跨屏、呈现帧、UIA/读屏、IME、ETW、宿主性能或安装成功。
- 之前主工作区的 DEV-INSTALL-008 事实保持独立：Release `0/0`，Core `83/83`，Worker `311/311`，Playnite `73 failed / 588 passed / 57 skipped`，安装器退出 `1`，未进入打包/安装；首个资源字典失败来自过期源码字符串断言，不据此宣称命令实际不可达。
- 当前续作分支的代码已推送；main 中的用户改动未触碰、未覆盖。隔离 worktree 的 WPF 临时文件写入限制仍存在，完整构建和 harness 使用 `.tmp` 源副本完成；没有绕过 ETW/系统跟踪权限。

## 下一项

下一可执行小批量：R12-02“校验结果解释”。先核对已有恢复 readiness DTO 和错误映射，补足校验失败/警告的分层文案与行为负例；保留本证据列出的真实 Playnite 安装、物理呈现、ETW 和离屏基线未验边界。
