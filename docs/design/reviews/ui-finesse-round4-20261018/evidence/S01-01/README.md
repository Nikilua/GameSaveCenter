# S01-01 版本化工作视图配置模型

日期：2026-10-18。实现提交：`25bbe005`（`新增版本化工作视图配置模型`）。

## 实现

新增 `ViewConfigurationDefinition`，按 schema version 1 组合筛选预设 ID、工作区键、排序列/方向、可见列键、列宽和密度。模型只暴露整数、字符串、字符串列表及列键到像素宽度的映射，不持有 DTO、文件路径或文件内容。归一化限制配置数量、列键数量、宽度值和文本长度；未知 schema 版本、空名称/工作区及无效值退回安全默认或忽略。

`GameSaveCenterSettings.ViewConfigurations` 在旧设置缺少字段时保持空列表；未知 JSON 成员由 Json.NET 默认忽略。既有 `FilterPresets` 和 `DataGridColumnWidths` 结构保持兼容。设置编辑快照通过现有 JSON clone 路径深复制视图配置，重置设置会清空新集合。

## 验证状态

- `python scripts/validate-source.py .`：通过；包含 JSON/XML/YAML、XAML 语义/资源、C# 分隔符、Round4 账本与环境缓存检查。
- `git diff --check`：通过。
- SDK `8.0.423` Release 构建 Playnite 测试项目成功（0 warning / 0 error）。`ViewConfigurationDefinitionTests` 与 R09 回归类共 `7/7` 通过；TRX：[S01-01-and-R09-sdk8423.trx](tests/S01-01-and-R09-sdk8423.trx)，SHA-256 `67A87BA7EB9D418AD790A6EEBCB5285CB20E83C385A371717A5167E4D7707A69`。
- 测试发现 Json.NET 默认填充初始化集合，导致设置反序列化绕过 `ViewConfigurations` 的归一化 setter。已为该属性指定 `ObjectCreationHandling.Replace`，保证加载时执行替换和归一化；旧设置缺字段行为仍由测试覆盖。
- 未修改 XAML；真实 Playnite 设置加载/保存与宿主行为未验证。

## 边界与下一步

受控验证现已通过，S01-01 可更新为已验收；真实 Playnite 设置加载/保存仍未运行。该模型只建立持久化契约，尚未提供页面密度、列选择或视图保存交互。
