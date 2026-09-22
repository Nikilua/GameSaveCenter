# R21-02 MediaCenter 批量动作空选择负例证据

日期：2026-09-21
提交：`f7b664f1`（`补充媒体批量空选保护证据`）

## 本批范围

- 先复核现有生产实现：三个 MediaCenter 批量命令仍复用既有 `UpdateMediaMetadataBatchAsync`；方法在读取参数后先去重筛选媒体项，`null` 或空 `IList` 直接抛出“请先在媒体列表中选择一个或多个项目。”，不会取得游戏写入上下文或发出 IPC。
- 不改生产命令、Binding、取消/错误语义或媒体写入路径；新增测试只验证这条已有安全保护，避免把可 Invoke 的 UIA 通道误签成空选择可执行。
- `MediaCenterBatchActionsRejectEmptySelectionBeforeMetadataWrite` 通过反射调用真实生产私有异步方法，分别传入 `null` 与空 `ArrayList`，实际等待任务并断言同一错误消息。

## 证据结果

- `R21AutomationValueBehaviorTests`：`16/16`。
- R21-02 相关进度/焦点/键盘/无障碍/生产壳层套件：`64/64`。
- D 盘源码副本、显式 `GscBuildCommit=f7b664f1` 的 Release 构建：Playnite `net462`、Tests `net472`，`0 errors`；仅有既有 `MediaCenterView.xaml.cs:671 CS8602` 两条 warning。
- `scripts/validate-source.py`：通过；XAML 结构检查：`24/24`；`git diff --check`：通过。
- `validate_wpf_ui.py`：`0 errors / 27 warnings / 177 info`，未见本批新增诊断。

## 边界与清理

- 该证据覆盖空选择/null 的错误保护和写入前中止，不覆盖真实 `ICommand.CanExecute` 在 Playnite 绑定后的 UI Enabled 呈现；也不宣称真实媒体批量写入成功。
- 只验证合成参数、生产方法和隔离 source-copy/testhost；未宣称真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终呈现或宿主性能已验证。
- 链接工作树 WPF `_wpftmp.csproj` 仍因 `Access denied` 不可直接写入，未绕过权限；本批 D 盘 source-copy/build 已清理。
- Demo 原目录不可用，沿用已恢复的生产基线；main 工作树用户改动未碰、未合并。
- R21-02 仍不整项签收。下一可执行小批量：继续核对批量动作忙碌态/CanExecute 与其他逐控件状态值边界；完成公共门禁后再进入 `R21-03` 错误播报。
