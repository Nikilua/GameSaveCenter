# R22-07 确认框信息结构

日期：2026-09-22  
状态：已满足，待环境验证  
代码提交：`3ac6d16a`（收紧危险确认框键盘语义）

## 本批核对与实现

- 先按真实调用链核对恢复、元数据恢复、保留清理、隔离账本协调、媒体批量和任务取消等确认内容。恢复正文已经实际列出游戏/版本或批次对象、作用范围、后果以及 PreRestore/文件保留/停止条件；按钮由调用方传入“开始安全恢复”“清理候选版本”“继续协调”等动作文案，不依赖“是/否”。
- 确认卡沿用 Demo 基线的标题、正文和独立 `ScrollViewer` 详情区（长技术信息可在详情区滚动阅读），没有重建另一套模态服务；现有遮罩焦点范围、返回焦点、Escape 取消和关闭生命周期保持。
- 新增共享 `DialogConfirmationPolicy`：危险确认的确认按钮显式 `IsDefault=false`，取消按钮显式 `IsCancel=true` 且不是默认按钮；普通确认、三选一和结果关闭分别应用各自明确的键盘策略。危险确认仍把初始焦点放在取消按钮，并补充 Automation HelpText。
- 本地恢复、撤销恢复、已校验远端恢复补上 `isDangerous: true`，使这些会替换当前状态的动作也不能被 Enter 默认确认；PreRestore 快照、当前选择复核、取消返回和 Worker 请求语义未改。
- Playnite 无嵌入页面处理器时的 native fallback 仍受 `ShowMessage(YesNo)` API 限制，保守返回 No；本批不把该 fallback 宣称为可自定义按钮文案的嵌入确认 UI。

## 证据

- `R22ConfirmationStructureBehaviorTests 4/4`：实际 WPF STA `Button` 验证危险/普通/三选一/结果策略；`UiConfirmationEventArgs` 验证动作文案和未完成的安全默认；源码边界验证三条恢复路径均标为危险确认。
- 相关对话框、焦点和恢复回归合计 `34/34`：`R08DialogOverlayBehaviorTests`、`R05FocusBoundaryBehaviorTests`、`R22RestoreConfirmationTimeBehaviorTests`、`R12RestoreWorkflowBehaviorTests`、`R12RestoreRevalidationBehaviorTests`、`R13CloudTransferStageBehaviorTests` 与本项测试均通过。
- `scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot .tmp/r22-07-confirm-build-20260922`（提交身份 `3ac6d16a`）：XAML `24/24`；Contracts/Playnite `net462`、Tests `net472`、Worker 构建 `0 errors`；保留既有 `MediaCenterView.xaml.cs:699 CS8602` 两条 warning。
- `python scripts/validate-source.py`、`git diff --check` 通过；WPF 静态检查 `0 errors / 27 warnings / 177 info`，与基线一致。

## 边界

- 证据使用合成确认请求、隔离 STA WPF、fake/合成恢复 DTO 和隔离构建目录；未启动真实 Playnite/package-host，未验证真实 Windows UIA/读屏、OS 输入/IME、最终呈现帧、DPI/物理跨屏、ETW 或宿主性能。
- Demo 原目录不可用，视觉依据仍为已恢复生产基线；未触碰 main 工作区的用户改动，未读写真实存档、媒体、云端、用户存档或外发诊断。
- 本批没有把 native fallback 的 Yes/No 主机 API 限制、真实宿主呈现或读屏结果写成已通过；现有 ScrollViewer 只证明详情可滚动阅读，不替代真实宿主可用性验证。

## 下一步

下一可执行任务为 `R22-08` 状态样式一致索引：先盘点成功、警告、错误、运行、暂停、未知的现有状态资源/图标/文案映射，再做一个边界明确的小批量行为证据；保留当前选框、滚动条、命令绑定、取消/错误和恢复保护。
