# U07-05 / U08-07 通知焦点回归

日期：2026-10-29。生产代码基线为提交 `7304a58e0767465b9af1c9c93fa46387f0ac304c`；该次验证仅修改测试夹具，`GscBuildCommit` 与 `GscSourceRoot` 均指向此基线。

## 覆盖与结果

- `NotificationInboxViewBehaviorTests` 使用生产 `NotificationInboxView`、`DataGrid` 和已显示的 WPF 窗口；SDK 8.0.423 / Release 定向组4/4通过、0失败、0跳过。归档 TRX：[`notification-inbox-focus.trx`](tests/notification-inbox-focus.trx)，SHA-256 `469AA52E2F28A77206200BBA1462EFAFA2B960469E181AF654454F41074D5406`。
- U08-07 用例把真实无结果空态置为可见，在清除按钮获焦时触发清除，验证搜索/级别/时间筛选复位且 WPF 键盘焦点返回搜索框；同时检查通知记录与已读命令计数未变。
- U07-05 用例在搜索框持有焦点、旧通知选中时向集合首部插入新通知，验证焦点仍在搜索框、旧对象仍选中。
- 首次新增焦点断言时测试夹具把 `NotificationInboxHasNoFilterResults` 固定为 `false`，清除按钮所在空态折叠，因而不可聚焦。将夹具改为无结果状态后重跑通过。失败诊断保留为 [`clear-filter-focus-fixture-diagnostic.trx`](tests/clear-filter-focus-fixture-diagnostic.trx)，SHA-256 `C0160E7D83D11E23410766D82476C969328C432AFA5E9FA90EC1AB0B44339A1B`；该失败不代表生产按钮故障。

## 检查与边界

`python scripts/validate-source.py` 退出0；WPF静态审查扫描42个XAML、0错误、26项既有警告、179项info；`git diff --check` 退出0。本批没有生产XAML/资源变化，因此没有重跑Render QA；引用既有 [`U08-07/08基线`](../U08-07-08/BASELINE.md) 的双主题394 PNG/0 PROBLEM，不将旧图说成本批截图。

`Focus()` / `Keyboard.Focus()` 与代码触发的按钮Click只验证WPF合成窗口行为，不代表物理键盘、鼠标、外部UIA或Playnite宿主输入。测试也没有覆盖数据库/Worker通知刷新或实际游戏/任务跳转；这些边界保持待验。`7304a58e` 的GitHub run `40726534942` 查询时仍在运行：baseline job `122951221975` 与SDK10 job `122951222525` 均未终态，尚无artifact；本测试证据随下一提交重新触发精确SHA双SDK CI。
