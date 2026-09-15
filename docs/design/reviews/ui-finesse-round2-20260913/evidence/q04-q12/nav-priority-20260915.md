# Q06-05 选中导航悬停优先级证据

## 本轮结果

- 代码提交：`513ac5f`（`修复选中导航悬停状态优先级`）。
- 生产入口：`src/GameSaveCenter.Playnite/Themes/AcrylicProductionResources.xaml` 的共享 `AcrylicNavItem` 模板。
- 修复前，`IsChecked=True` 的强选中背景先应用，后续 `IsMouseOver=True` 又写入普通 `GscAccentTintBrush`，当前导航项悬停时会丢失强选中层级。
- 现在使用最后执行的 `MultiTrigger(IsChecked=True, IsMouseOver=True)` 恢复 `GscAccentTintStrongBrush`、强边框和 `GscSelectionTextBrush`；选中态优先级不再依赖触发器偶然顺序。

## 自动证据

- `UiFinesseRound2ControlSourceTests`：`20/20` 通过，新增 `SelectedAcrylicNavigationKeepsItsStrongStateWhenHovered`。
- `python scripts/validate-source.py`：通过。
- `scripts/check-xaml.ps1`：`24` 个 XAML 通过。
- Release RenderHarness：`render-qa OK`，提交身份 `513ac5f2528e0706fde194d977f03fd8bd798e5d`，`WorkingTreeClean=True`，浅色/深色均完成全量页面、滚动和资源检查，0 warning/0 error。

## 验收边界

本证据关闭了 Q06-05 的共享模板代码缺口和自动检查；它没有把真实 Playnite 中“保持鼠标悬停、切换焦点/禁用、按压并观察最终像素”的序列写成通过。真实宿主的该输入序列仍保留在账本的视觉/宿主待验列。
