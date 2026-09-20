# R21-02 MediaCenter 批量动作忙碌态 CanExecute 证据

日期：2026-09-21
提交：`e0805624`（`修复媒体批量动作忙碌态刷新`）

## 本批范围

- 复核现有生产实现：`FavoriteSelectedMediaCommand`、`UnfavoriteSelectedMediaCommand`、`CommentSelectedMediaCommand` 的 `CanExecute` 均依赖 `!IsBusy`，但原 `RaiseCommandStatesCore` 刷新集合遗漏这三项。
- 复用既有 `IsBusy`、`RelayCommand`、命令绑定和批量选择参数；只把三个已存在命令加入刷新列表，没有新增服务/DTO，也没有改变执行、取消/错误或媒体写入语义。
- 新增 `MediaBatchCommandsRefreshBusyCanExecuteState`：先检查生产刷新列表，再以真实 WPF `Button.Command` 绑定 `RelayCommand`，实际验证按钮状态从“可用”切到“忙碌禁用”并回到“可用”。

## 证据结果

- `R21AutomationValueBehaviorTests`：`17/17`。
- R21-02 相关进度/焦点/键盘/无障碍/生产壳层套件：`65/65`。
- D 盘源码副本、显式 `GscBuildCommit=e0805624` 的 Release 构建：Playnite `net462`、Tests `net472`，`0 errors`；仅有既有 `MediaCenterView.xaml.cs:671 CS8602` 两条 warning。
- `scripts/validate-source.py`：通过；XAML 结构检查：`24/24`；`git diff --check`：通过。
- `validate_wpf_ui.py`：`0 errors / 27 warnings / 162 info`，未见本批新增诊断。

## 边界与清理

- WPF 探针证明真实控件的 `Command`/`CanExecute` 回切通道和生产刷新列表接线，不代表真实 Playnite/package-host、媒体 IPC/写入或物理呈现；空选择 null/空集合保护由上一批证据继续覆盖。
- 未宣称真实 Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现或宿主性能已验证；不绕过链接工作树 WPF `_wpftmp.csproj` 的 `Access denied`。
- Demo 原目录不可用，沿用已恢复生产基线；main 工作树用户改动未碰、未合并。本批 D 盘 source-copy/build 已清理。
- R21-02 仍不整项签收。下一可执行小批量：继续其他逐控件状态/值边界，完成公共门禁后进入 `R21-03` 错误播报。
