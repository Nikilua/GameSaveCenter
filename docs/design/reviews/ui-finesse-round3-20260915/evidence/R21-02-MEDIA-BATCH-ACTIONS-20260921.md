# R21-02 MediaCenter 批量动作名称与 Invoke 证据

日期：2026-09-21  
提交：`9fd7223c`（`补充媒体批量动作语义证据`）

## 本批范围

- 复用 MediaCenter 现有 `FavoriteSelectedMediaCommand`、`UnfavoriteSelectedMediaCommand`、`CommentSelectedMediaCommand` 及两个现有批量操作呈现位置；没有新增服务、DTO、命令、选择模型或媒体写入语义。
- 两处批量操作条的三类按钮分别补稳定 Automation 名称：“收藏所选媒体”“取消收藏所选媒体”“为所选媒体应用当前备注”，保留原 Content、Command、CommandParameter、样式和布局。
- 新增 `MediaCenterBatchActionsExposeStableNamesAndInvokeChannels`：来源契约确认三个名称各出现两次；实际 WPF Peer 验证三个名称，并用 `IInvokeProvider` 调用三个隔离按钮处理器。

## 证据结果

- `R21AutomationValueBehaviorTests`：`15/15`。
- R21-02 相关进度/焦点/键盘/无障碍/生产壳层套件：`63/63`。
- D 盘源码副本、显式 `GscBuildCommit=9fd7223c` 的 Release 构建：Playnite `net462`、Tests `net472`，`0 errors`；仅有既有 `MediaCenterView.xaml.cs:671 CS8602` 两条 warning。
- `scripts/validate-source.py`：通过；XAML 结构检查：`24/24`；`git diff --check`：通过。
- `validate_wpf_ui.py`：`0 errors / 27 warnings / 177 info`，未见本批新增诊断。

## 边界与清理

- Invoke 验证的是隔离 WPF Button 的 UIA 调用通道，不宣称真实 `ICommand` 执行、批量选择集合或媒体写入已完成；生产 Command/CommandParameter 保持原样并由源码契约保留。
- 只验证合成 WPF Peer、fake/隔离 testhost 和源码副本；未宣称真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现或宿主性能已验证。
- 链接工作树 WPF `_wpftmp.csproj` 仍因 `Access denied` 不可直接写入，未绕过权限；本批 D 盘 source-copy/build 已清理。
- Demo 原目录不可用，沿用已恢复的生产基线；main 工作树用户改动未碰、未合并。
- R21-02 仍不整项签收。下一可执行小批量：继续核对批量动作的禁用/空选择负例及其他逐控件状态/值边界；完成公共门禁后再进入 `R21-03` 错误播报。
