# R11-05 保护操作解释证据

日期：2026-09-19

分支：`codex/ui-finesse-round2`
范围：锁定、PreRestore、健康恢复点的保护状态解释，以及解除锁定后重新进入保留评估的边界

## 结论

R11-05 已满足当前可控验收条件。

先核对最新实现：Core `RetentionPlanner` 与 Worker `RetentionSimulationService` 已有真实保护规则，保留预览和应用重检都会跳过锁定、PreRestore 与健康恢复点。本阶段没有重建保留服务；只补齐 UI 状态与规则之间的可读连接，并把健康恢复点的显示判断与 Worker 的严重异常边界对齐：必须为 `Ready` 且文件数、总字节数均为正。

## 实际变更

- `BackupVersionDto` 增加 `IsRetentionProtected`、保护状态 glyph、保护类型和解除条件说明；健康恢复点只在 `Ready` 且存在有效内容时显示为受保护，避免空内容的 Ready 状态误导用户。
- `SaveCenterView` 历史行复用真实 DTO 状态显示 `✓`/`⚠`，状态说明通过 ToolTip 绑定；版本详情的锁定区显示当前草稿与“取消锁定并保存后下一次预览才重新评估”的边界，保留既有游戏选框、滚动条、命令和编辑语义。
- `DashboardViewModel` 为锁定草稿提供动态解释，并在锁定状态变化时通知绑定更新。
- 复用已有 Worker 预览/应用实现和 Core 规划器，没有新增真实存档、媒体、云端或诊断写入。

## 实际验证

- `R11ProtectionBehaviorTests`：`2/2`。
  - 合成 DTO 正例：锁定、PreRestore、有效健康恢复点分别显示 `✓` 与对应解释。
  - 负例：普通版本和 `Ready` 但文件数/字节数为零的版本显示 `⚠`，进入当前策略评估。
  - 真实 `SaveCenterView` 的 DataGrid 行加载合成版本集合，验证行绑定使用保护 glyph/解释属性；该夹具不宣称最终像素渲染证明。
- Core `RetentionPlannerTests`：`3/3`；解除旧版本锁定后，该版本重新成为候选，其他保护类型仍不成为候选。
- Worker `RetentionSimulationServiceTests` 保护相关夹具：`2/2`；预览统计锁定、PreRestore、健康保护数量，应用只处理未保护 zip 候选。
- R11-01/R11-02/R11-03/R11-04/R11-05 SaveCenter WPF 夹具串行组合：`11/11`。
- 存档页 R06 空态、排序、列宽相邻回归：`11/11`。
- D 盘隔离 Release solution 构建：`0 warning / 0 error`；Playnite `net462`、Worker、测试程序集均从同一隔离输出生成。
- `scripts/check-xaml.ps1`：`24/24`；`python scripts/validate-source.py`：通过；`git diff --check`：通过。

## 边界

证据使用合成 `BackupVersionDto`、隔离 SQLite/Worker 夹具、真实生产 `SaveCenterView` 和隔离 STA Window；没有读取或修改真实存档、媒体、用户云端或对外诊断数据。SaveCenter 夹具验证绑定状态与解释契约，不等价于 Playnite 宿主最终 presented frame、物理 DPI/跨屏、UIA/读屏、真实键盘/IME、ETW 或宿主性能验证。Worker 夹具使用合成归档和隔离目录，不等价于真实 Ludusavi 进程重启后的 IPC/归档读取。Demo 原目录不可用，继续沿用恢复生产基线。

本次 WPF 临时工程在非提升构建路径曾遇到创建临时 `wpftmp` 工程的 Access denied；按项目既有 `GscBuildOutputRoot` 使用 D 盘隔离输出完成同一构建与测试，未绕过 ETW/系统跟踪权限，也未覆盖 main。

用户提供的 main 合并后安装事实继续独立保留：编译 `0 warning / 0 error`，Core `83/83`，Worker `311/311`，Playnite `73 failed / 588 passed / 57 skipped`，安装器退出 1；本阶段未触碰 main，也未在 dirty main 上安装或覆盖。

## 下一步

下一可执行任务：R11-06 备份前变更摘要。R11-05 之外的真实 Worker/Ludusavi IPC、Playnite 宿主安装呈现和上述系统级边界仍未验。
