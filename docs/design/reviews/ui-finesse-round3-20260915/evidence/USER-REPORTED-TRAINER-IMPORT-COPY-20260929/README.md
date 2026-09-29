# Trainer 导入确认空态文案

日期：2026-09-29  
源码提交：`98800f1a7cb27bcb6f92c190c7feb5cf3d388c94`。本证据只覆盖生产 `TrainerCenterView` 的“导入确认”页空态提示及其操作工具栏。

## 行为与布局

- 当没有待确认项目时，保留简短的导入入口提示；`HasPendingGameToolEntrySelection` 变为 `true` 后折叠该提示，避免已有数据时继续显示“暂无项目”类辅助文字。
- 生产选择器、确认/取消按钮、当前选择和两个 `ICommand` 绑定保持可用；Light/Dark 状态切换都由生产视图与合成可通知状态对象验证。
- R00-07 离屏几何 JSON：compact `904×520 DIP`、wide `1596×840 DIP`；操作 WrapPanel 为 `44 DIP` 高，3 个子元素（2 个动作），无横向溢出、可达且无该页布局警告。审计生成时空态处于可见状态；待确认状态下提示折叠由行为测试覆盖。
- R00-07 当前完整审计为 `168` 个快照、`110` 条既有警告、`0 HIGH`、`0 路由失败`；MEDIUM 中 3 条来自外壳动作区、4 条来自 Media Inbox 批量动作区，Trainer 导入确认没有新 warning。本次没有改变审计规则或媒体滚动行为。

## 验证身份

- 提交后 Release 隔离构建：XAML `24/24`；解决方案 `0 warnings / 0 errors`。
- 精确 Release `net472` 测试程序集：导入提示与操作绑定行为 `3/3`；`UiAuditSourceTests` `6/6`；均为 `0 failed / 0 skipped`。
- 测试 DLL ProductVersion：`0.6.73+98800f1a7cb27bcb6f92c190c7feb5cf3d388c94`；SHA-256 `143613A420F50C376D3CC4B62A9896CDB6840E494B40D88835DC093E58C9765C`；MVID `38414e45-d62d-4610-811d-ef4280315cf1`。
- 插件 DLL ProductVersion 同为 `0.6.73+98800f1a7cb27bcb6f92c190c7feb5cf3d388c94`；SHA-256 `6DF65AE2EC6307DE664D63FAAE63977C6EE48E047029C56B1B0CAB081697F458`；MVID `d038dbc6-bb4a-4162-b4fc-9465b09ceceb`。
- `scripts/validate-source.py` 通过；WPF 静态检查 `0 errors / 30 warnings / 177 info`，没有新增 error。RenderHarness Light/Dark 全矩阵在提交前以相同源码内容运行并返回 `render-qa OK`；完整离屏审计与三种 toolbar 分类负例见本次 `.tmp` 审计记录摘要。

## 未验证边界

RenderHarness 和受控 STA WPF 均为离屏/合成逻辑 DIP 验证；没有启动真实 Playnite，没有验证 125%/150% 物理 DPI、OS 输入或宿主最终呈现帧。Media Inbox 真实滚动空白问题仍需安全同进程宿主诊断；本批未改其虚拟化、滚动锚点或共享 DataGrid 模板。

## 记录

- [`trainer-import-copy-state.trx`](trainer-import-copy-state.trx)
- [`trainer-copy-ui-audit-source.trx`](trainer-copy-ui-audit-source.trx)
- [`trainer-center-import-compact.json`](trainer-center-import-compact.json)
- [`trainer-center-import-wide.json`](trainer-center-import-wide.json)
- [`toolbar-probe-report.txt`](toolbar-probe-report.txt)