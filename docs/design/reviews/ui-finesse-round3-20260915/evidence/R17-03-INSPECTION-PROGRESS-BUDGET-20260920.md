# R17-03 检查进度预算证据

日期：2026-09-20  
状态：已实现，受控验证完成；真实宿主验证待进行  
实现提交：`87473bc3`（`补充恢复巡检进度边界`）

## 审计与实现

- 复用既有 `HealthInspectionService` 的持久化游标、单次时间预算、运行中会话判断、`GameOperationLock`、延后候选表和取消/失败终态，没有新增数据库迁移或替换巡检机制。
- 巡检开始时先持久化“尚未完成备份索引读取”；索引加载后在既有 `LastSummary` 通道记录索引总数、需检查数、等待延后数和选中候选数，并明确未选择归档未读取。
- 游戏运行与操作锁占用分别写出暂停原因；所有候选都在等待时也写出暂停原因。取消、单次预算耗尽和异常结束均带明确结束状态，不把延后或取消计为已检查。
- `HealthInspectionStateDto` 增加最近完成时间、当前/最近候选、进度边界和下轮计划显示；维护页健康卡与恢复巡检行动项显示本轮边界、最近成功、最近完成、下轮间隔/预算及真实摘要。原命令、Binding、游戏选框、滚动条、取消/错误/恢复保护和 net462 路径保留。

## 验证

- Worker `HealthInspectionServiceTests`：`12/12`，含真实隔离 SQLite/归档的运行中延后、游标恢复、成功/损坏、取消未读取归档和锁门禁行为。
- Playnite R17：`10/10`，包含 R17-01/R17-02 回归、R17-03 DTO 行为和维护页绑定源码检查；源码测试按当前 checkout 注入 `GscBuildCommit`/`GscSourceRoot` 后通过。
- 完整 `GameSaveCenter.sln` Release 构建：`0 errors / 2 warnings`；两条均为既有 `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs:664` nullable warning。
- `validate-source.py`：通过；XAML 结构：`24/24`；`git diff --check`：通过；WPF 静态质量：`0 errors / 28 warnings / 162 info`，未新增错误。
- 构建输出位于隔离 `.tmp/r17-03-solution`，证据确认后已清理；测试只使用合成数据、fake 会话/锁和隔离目录，没有修改真实存档、媒体、数据库、用户配置或云端数据。

## 未验证边界

- 未启动真实 Playnite/package-host，未把维护卡片最终浅深主题、DPI/UIA/IME、焦点/滚动、presented frame、ETW 或宿主性能写成通过；WPF 静态 warning/info 为既有基线。
- 未在真实游戏运行进程、真实锁竞争、真实超时归档或真实用户目录中验证宿主时序；当前证据覆盖隔离 fake/SQLite 行为和持久化摘要语义。
- Demo 原目录不可用，沿用恢复生产基线；main 上用户未提交的 `DashboardView.xaml.cs`、`src.zip`、对话框文件/测试未触碰，当前未合并 main。

下一可执行任务：`R17-04 保留预览对比`，先核对现有 `RetentionSimulationService`、保护项/隔离账本和执行前预览过期边界。
