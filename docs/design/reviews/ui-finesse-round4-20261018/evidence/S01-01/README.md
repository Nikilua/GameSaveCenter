# S01-01 版本化工作视图配置模型

日期：2026-10-18。实现提交：`25bbe005`（`新增版本化工作视图配置模型`）。

## 实现

新增 `ViewConfigurationDefinition`，按 schema version 1 组合筛选预设 ID、工作区键、排序列/方向、可见列键、列宽和密度。模型只暴露整数、字符串、字符串列表及列键到像素宽度的映射，不持有 DTO、文件路径或文件内容。归一化限制配置数量、列键数量、宽度值和文本长度；未知 schema 版本、空名称/工作区及无效值退回安全默认或忽略。

`GameSaveCenterSettings.ViewConfigurations` 在旧设置缺少字段时保持空列表；未知 JSON 成员由 Json.NET 默认忽略。既有 `FilterPresets` 和 `DataGridColumnWidths` 结构保持兼容。设置编辑快照通过现有 JSON clone 路径深复制视图配置，重置设置会清空新集合。

## 验证状态

- `python scripts/validate-source.py .`：通过；包含 JSON/XML/YAML、XAML 语义/资源、C# 分隔符、Round4 账本与环境缓存检查。
- `git diff --check`：通过。
- 新增 `ViewConfigurationDefinitionTests` 覆盖归一化、数量上限、未知 JSON 成员、旧设置字段缺失、未来 schema 忽略、设置往返与编辑取消后的独立集合；尚未运行这些 C# 测试或编译。
- 当前主机缺仓库要求的精确 SDK `8.0.423`，因此本阶段未运行 .NET build/test；不以 SDK9 代替。
- 未修改 XAML；真实 Playnite 设置加载/保存与宿主行为未验证。

## 边界与下一步

`controlledVerificationState` 保持 `not_run`，`hostVerificationState` 保持 `not_run`。SDK8.0.423 可用后先构建 Playnite 测试项目并运行 `ViewConfigurationDefinitionTests`；通过后再将 S01-01 更新为已验收。该模型只建立持久化契约，尚未提供页面密度、列选择或视图保存交互。
