# Maintenance 诊断空态文案精简

日期：2026-09-29  
源码提交：`4a8341350c2340c479375bb1a6563c221a14009f`

## 改动与行为

- 维护页“发现的问题”和“异常与审计 → 发现的问题”共用短空态“暂无待处理诊断项。”，删除了原先推断备份与媒体状态正常的第二句话。零条诊断仍显示空态；有诊断时两处提示均折叠。没有改诊断服务、刷新命令、错误/离线状态、取消语义或危险操作。
- 生产 `MaintenanceView` Light/Dark STA WPF 测试实际装载两个页面：主列表按 `Findings.Count` 显示提示，审计列表按 `MaintenanceState == Empty` 显示提示。合成诊断项加入与清空时，两处都呈现 `Visible → Collapsed → Visible`。

## 构建与测试

- 当前提交隔离 Release solution 构建成功：`0 warnings / 0 errors`；XAML 结构验证 `24/24`。
- `ReportedWorkspaceLayoutBehaviorTests.MaintenanceDiagnosticEmptyCopyIsAccurateAndCollapsesWithData` 浅/深主题 `2/2`，VSTest 失败 `0`、跳过 `0`、退出码 `0`。实际测试窗口 `1100×720 DIP`，夹具仅用合成诊断，不访问用户数据。
- `scripts/validate-source.py` 与 `git diff --check` 通过；R00/R01 freshness `14/14`，本次未命中登记源路径。
- 测试 DLL ProductVersion `0.6.73+4a8341350c2340c479375bb1a6563c221a14009f`；SHA-256 `A578914DE4AD9E426B198D46012784261F57DDBB8F65487DC8765111C1A7A8CC`；MVID `788a51b7-80be-4b22-a195-2daae5e89cf1`。
- Playnite 插件 DLL ProductVersion 同上；SHA-256 `19A22FE32D67008DC0389CE336F3F6E539669A2C0E4E74AC47CF98CB077E4B1C`；MVID `6427b665-d1e6-4947-a158-ca51dcad84d4`。

## 未验证边界

本证据是受控 STA WPF 合成数据和逻辑 DIP 行为，没有启动真实 Playnite，没有核对用户截图对应 DLL、物理 125%/150% DPI 或屏幕呈现。没有将此局部空态精简写成所有维护/存档说明均已审计。存档恢复保护、远端校验、隔离恢复和取消语义文案仍保留待逐项审核。Demo 原目录在当前 checkout 不可用，视觉继续沿用恢复的生产基线。

记录：[`maintenance-empty-state.trx`](maintenance-empty-state.trx)
