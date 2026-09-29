# 用户报告的表格排序崩溃与选中行几何复核

日期：2026-09-29。只补证，不改生产源码、测试源码或 192 项任务状态。

## 用户日志与当前代码

- `supplied-sort-crash-stack.txt` 是用户提供 `crash.zip` 的脱敏最小栈摘要。日志记录 `ListCollectionView` 刷新过程中的 `NullReferenceException`，经 `DataGridStableSortController.ApplyCurrentSort/ToggleSort/OnSorting` 到真实 `DataGridColumnHeader.OnClick`。附件没有插件 DLL、SHA 或 MVID，不能确认崩溃实例的二进制身份。
- 已有修复 `f8a82469` 为排序切换期间视图被移除时避免刷新已分离的 CollectionView；2026-09-28 的 clean-main 复核确认生产控制器/排序测试从修复到当时 HEAD 未变，真实列头升降序及 detached-view 负例 `7/7`。本次在当前 HEAD `10626efc1d90db849bb297293cafc8aabfbd4997` 上重新构建并复跑。

## 当前 HEAD 验证

- Release solution build：`0 warning / 0 error`；XAML 检查 `24/24`；`scripts/validate-source.py`、`git diff --check` 通过。
- `R06SortingBehaviorTests`：`7/7`，包括实际 DataGrid 列头升/降序和集合视图分离后点击的“不抛异常且排序状态不变”负例。
- `R06SelectionStateBehaviorTests`：`2/2`，实际 STA WPF DataGrid 比较未选、失焦选中和键盘焦点选中的 cell/TextBlock 几何，布局变化容差 `0.25 DIP`，并覆盖失败色与选择装饰。
- `R23ProductionResourceStateBehaviorTests.EachProductionPageGridKeepsSelectedFocusAndDisabledRowStatesAcrossThemes`：`1/1`，覆盖 Save、Task、Media Inbox、Maintenance 四张生产表格的 Light/Dark 状态及内容几何。
- 三个独立 VSTest 进程合计 `10/10`，0 失败、0 跳过。TRX：[`sorting`](r06-sorting-current-head.trx)、[`selection`](r06-selection-current-head.trx)、[`production grid state`](r23-row-state-current-head.trx)。
- 测试程序集：ProductVersion `0.6.73+10626efc1d90db849bb297293cafc8aabfbd4997`；SHA-256 `F3140726D4E37546E80FA3D57287004E1B5E77A0DFB450E87D50D6E54250EC18`；MVID `3457eddd-8525-4f57-9610-722ce0a6729b`。
- 插件程序集：相同 ProductVersion；SHA-256 `E3B4934970DE84DF7D3E51D4DD2411333831D6C55A75EB1C4B2B50B90B4F9D38`；MVID `364352e7-b059-4537-a21b-6c7df0d32af1`。
- R00/R01 freshness 检查仍为 `14/14`；本阶段没有改动证据源路径。源码/测试文件在历史排序与选中几何证据之后未改变，当前 TRX 则绑定本次精确 Release 程序集。

## 边界

本次为合成数据、STA WPF 逻辑布局回归，没有启动 Playnite，也没有复现用户 ZIP 对应的原始 DLL、用户主题、物理 DPI 或真实点击时序。故当前测试证明已知排序回归路径在当前 HEAD 通过，不证明附件所对应的未知构建已加载该修复。选中行几何为 WPF 控件测量，不替代用户屏幕帧。没有改动服务、DTO、命令、虚拟化或恢复/取消/错误语义。
