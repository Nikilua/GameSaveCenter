# S05-08 情景性能验证能力审计

日期：2026-10-21  
审计基线：`479a03c3`（S05-07 受控验收）

## 已有等价能力

- 情景比较通过 `RetentionSimulationService.CompareScenarioAsync` 读取一条 SQLite statement snapshot，并对两侧使用同一 `generatedUtc` 截止时间；增长范围仅回显为假设，不参与保留计数、逻辑体积或清理授权。现有 `ScenarioComparisonUsesOnePlannerCutoffAndDoesNotCreateCleanupPreview` 对账了两侧计数、保护项、缺失归档和 25–75 MiB 假设。
- Worker 的查询、逐游戏和逐版本循环接收 `CancellationToken`；输入无效有 Worker 端校验。缺口是页面没有用户可点的取消操作，比较读取/展开的版本数也没有硬上限。
- 输入参数变化会清空旧结果；但 SQLite 版本或策略在比较期间变化时，Worker 不会重新核对基线，页面可能展示已过期结果。
- 磁盘/远端可用容量未知已有独立存储分析表现。`StorageAnalysisServiceTests.AnalyzeReportsObservedGrowthRangeWithoutClaimingConfidenceOrExpiry` 已断言容量未知时不输出耗尽日期；情景增长假设与磁盘容量仍应分开。
- 统计与执行已分离：比较 DTO 不含清理预览 ID；S05-05/07 证据记录了清理继续使用新的 Worker 预览及执行前保护、版本、策略和归档身份复核。

## 准入结论：READY

S05-08 仍有可见且可独立验收的缺口，准入范围限定为维护中心保留情景比较：增加用户取消；为情景快照查询和返回结果设置可见的硬版本上限；比较结束时再次检查版本与有效保留策略是否与开始时一致，不一致则返回明确过期状态并清空明细；受控验证大库、0 增长、极端增长、并发新增版本、取消和超限行为。未知容量沿用 S05-03 的现有实现与测试，不复制其产品逻辑。所有结果继续只读，不生成或复用清理句柄。

初定情景比较上限为 10,000 个索引版本；存储查询最多物化上限加一条用于拒绝超限，避免先读取整库再判断。范围未包含真实宿主、真实远端卷容量或物理 DPI 证明。完整解决方案 SDK8.0.423 Release 构建环境当前缺失，若继续以 SDK9 验证将如实记录该边界。
