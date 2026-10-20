# S05-01 只读保留情景输入

实现提交：`990758b62c9adab2633a34e2fb0204004bdd1443`（`增加保留策略情景参数输入`）。

## 能力差异与实现

原有保留策略可编辑当前游戏/策略模板，并能预览 Worker 对当前有效策略的建议；它没有仅供比较的候选规则和用户给定的增长范围，因此不等价于 S05-01。本批新增 `RetentionScenarioDraft`，使用现有 `RetentionPolicy` 类型生成独立候选值，允许输入全量保留小时、每日天数、每周周数、每月月数，以及每 30 天逻辑备份体积增长上下限。

输入位于维护中心“保留策略”页，复用现有阅读卡、表单字段与主题资源。字段有范围和跨字段错误，输入网格按窗口宽度切换为三列、两列或单列；生产 `MaintenanceView` 的两主题行为用例确认情景卡可见、由页面滚动容器承载，错误可见且修改值只影响会话草稿。增长明确是用户假设，不是磁盘测量或预测。

草稿只挂在当前 `DashboardViewModel` 实例中；它没有设置/模板持久化入口、Worker 请求或清理命令连接。`CandidatePolicy` 每次返回新 `RetentionPolicy`，不会修改当前生效策略。S05-02 才负责同一数据快照上的双方案规划比较。

## 验证

- 精确提交 `990758b6`，SDK `8.0.423` Release 一键隔离构建、Worker 发布、包身份检查和 no-start 隔离安装通过；构建诊断 `20261020T085843483Z-ea5f6d76`，`0 warning / 0 error`。隔离安装清单 `0.6.73`、插件 DLL `0.6.73.0`；隔离安装副本与构建 DLL SHA-256 均为 `B56E5C15F252FB58CB4D24DE08BAE0B1A405AF7C72D434BD1E6875EA27316CA5`。安装报告见 [install-report.txt](build/install-report.txt)，原始构建日志见 [build-console.txt](build/build-console.txt)，完整诊断见 [summary.json](build/summary.json)。
- 针对同一隔离构建程序集运行 `RetentionScenarioDraftTests` 与生产 `MaintenanceView` 的 Light/Dark 行为测试，`6/6` 通过：[原始 TRX](tests/s05-01-retention-scenario-990758b6.trx)，SHA-256 `57EDABCE095F1B81DDA3B717BF16BAC710F122ABE436107F06FDA627B078897D`。测试 DLL SHA-256：`C3EF10EEA8AB516C825B9AD534E0DC20A5507991FDF9F7A2B60327F564F5886A`；插件 DLL SHA-256：`B56E5C15F252FB58CB4D24DE08BAE0B1A405AF7C72D434BD1E6875EA27316CA5`。
- SDK8.0.423 RenderHarness Release 构建 `0 warning / 0 error`。精确提交后的双主题 11 尺寸渲染 `render-qa OK`、`0 PROBLEM`：[原始报告](render/render-qa-report.txt)，SHA-256 `D4926062106836E11E0082F373F01F645828A76129BF1731C1665080C7AC6249`。报告记录 `Commit: 990758b6…`、逻辑 DPI `1.00`；`WorkingTreeClean: False` 来自保留的用户 Round3 未跟踪 TRX/TestResults。
- `validate-source.py`、25 文件 XAML 结构检查、`git diff --check` 通过。WPF 静态审查 `0 error / 26 warning / 179 info`；警告是仓库全局既有滚动结构提示，本批新增输入区未引入 error。

未运行完整 WPF 独立进程测试，也没有启动真实 Playnite。UI 和测试使用受控生产 WPF/离屏 RenderHarness；未声称是真实桌面、物理 DPI 或宿主渲染。仓库指定的 AcrylicFork Demo 文件当前不在 checkout，新增输入沿用生产共享样式和主题令牌。
