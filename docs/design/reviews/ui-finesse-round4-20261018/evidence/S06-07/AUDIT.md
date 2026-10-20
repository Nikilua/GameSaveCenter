# S06-07 清单导出预览等价能力审计

日期：2026-10-21
审计基线：`a1a86e41`
结论：未找到满足计划要求的清单导出预览；依赖 S06-01 至 S06-06 已满足，准入 READY。

## 核对范围

- `TaskCenterView.xaml` 的 `WorklistBrowserPanel`：目前支持选择清单、搜索/分组、刷新引用、续接对象、手工完成/撤销、排序和移除；没有预览或导出入口。`WorklistBrowserGrid` 未挂接 `DataGridClipboardBehavior`。
- `DashboardViewModel.Worklists.cs`、`DashboardViewModel.WorklistBrowser.cs` 与 `WorklistBrowserEntry`：提供清单浏览、人工整理和稳定 ID 引用解析，字段含类型、当前摘要、引用状态、备注和人工状态；没有清单文本导出命令。
- `DataGridClipboardFormatter`：剪贴板表格格式显式允许 SaveHistory、SaveCandidate、Task、MediaInbox、Finding，不包含 Worklist。直接让 DataGrid 复制会违背其显式 allowlist 设计。
- `RestorePlanDraftExportFormatter`：已有只读摘要、字段 allowlist、本地路径与维护报告脱敏示例，但只处理恢复草稿，不读取或导出工作清单。
- `ClipboardFeedback` 与剪贴板 setter 处理：可复用复制成功/剪贴板占用失败的非模态反馈，不需要另建剪贴板机制。

## S06-07 差异与准入范围

本项需提供独立的只读文本预览，再由用户明确选择复制。默认包含所有未手动标记完成的条目，其中也包括索引状态为失效、未检查或暂不可检查的条目；人工完成状态只是用户整理状态。导出时间使用 UTC。

导出只使用经允许的对象类型、受控摘要、父游戏摘要、引用状态和人工备注。文本不得包含稳定对象 ID、完整本机路径、原始文件内容、认证信息或可执行命令；自由文本经控制字符清理、路径屏蔽和既有 secret/report redactor 处理。预览明确写明“人工整理记录，不代表业务成功；不能导入或执行”。此动作不写入文件，也不触发源对象操作。

无设备、真实宿主、管理员权限或 SDK8 前置依赖。依赖满足；不重复实现清单浏览、状态刷新、续接、剪贴板失败反馈或恢复草稿导出器。

## 证据边界

本次只读审计，没有修改产品代码或运行构建/测试。实现阶段需以 formatter 内容行为和实际 WPF 预览/复制交互验证，而非只检查 UI 文案。
