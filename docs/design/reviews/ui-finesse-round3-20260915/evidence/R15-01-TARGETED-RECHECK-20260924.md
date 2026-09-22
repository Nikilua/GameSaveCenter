# R15-01 任务阶段可读定向复核

日期：2026-09-24
工作区：`D:\\workplace\\github\\GameSaveCenter`
分支：`codex/ui-finesse-round2`
当前复核代码身份：`9a63e7e2`
实现提交：`4e7ac33a`（本批没有生产代码变更）

## 结论

R15-01 已按当前实现和隔离测试结果收口为“已满足，待环境验证”。本批复用现有 `TaskCoordinator`、任务 DTO、SQLite 查询和 Task Center 命令链，只补当前身份的运行证据，没有重建任务服务或改变取消/错误/恢复语义。

## 受控行为验证

- Worker `TaskCoordinatorFailureTests` 与 `TaskQueryPersistenceTests` 合计 `16/16` 通过，覆盖任务阶段写入/失败保留、迁移后的 `stage_message` 读写、新增/更新/最近任务/分页查询和旧库空值兼容。
- Playnite `R15TaskStageTests 2/2` 通过：真实阶段事件映射到扫描、校验、索引、上传、下载、恢复、清理等可读阶段；运行中未知总量显示 `—`，失败/取消保留最后阶段并独立显示终态错误/取消语义。
- 当前隔离 Release 构建通过：XAML `24/24`、solution `0 error/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning，Playnite 目标 `net462`。
- `python scripts/validate-source.py`、`scripts/check-xaml.ps1 -ProjectRoot .` 和 `git diff --check` 在本批文档更新后通过。

## 受控视觉证据

- 人工检查既有 clean RenderHarness 截图 `artifacts/ui-qa-r13-r14-clean-20260922/Task-1600x900.png`：任务列表显示“阶段”列，合成行包含正常阶段与“阶段未知”负例；右侧详情区保留状态、进度、技术详情和当前阶段区域，任务列表仍是有限视口。
- 该截图为合成 offscreen 数据，不能证明真实 Worker 阶段事件覆盖、Playnite 最终呈现、物理 DPI/跨屏、UIA/IME、presented frame、ETW 或宿主性能；RenderHarness 全报告的其他页面基线失败仍保留。

## 语义与边界

- `TaskStatusDto.StageMessage` 继续独立保存最后真实阶段，`Message`/错误详情保留终态语义；`TaskStageResolver` 不根据未知阶段猜测百分比。
- 只使用合成 DTO、fake/隔离持久层、隔离 testhost 和隔离构建输出；未写真实存档、媒体、云端或真实用户任务历史。
- 真实宿主阶段事件全覆盖、重启后真实任务历史、Playnite/package-host、DPI/跨屏、UIA/读屏/IME、最终呈现帧、ETW 和宿主性能仍未验；Demo 原目录不可用，沿用恢复生产基线。

下一可执行小批量：推进 `R15-02 取消过程展示`，先核对现有取消请求、取消中状态及成功/取消竞争的终态收敛。
