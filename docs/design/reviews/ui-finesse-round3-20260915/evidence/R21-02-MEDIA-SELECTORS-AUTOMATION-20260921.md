# R21-02 MediaCenter 选择器名称与值证据

日期：2026-09-21  
代码提交：`da16043d`（`补充媒体选择器名称`）  
分支：`codex/ui-finesse-round2`

## 本批范围

本批只处理 MediaCenter 三个已有复合选择器：

- 收件箱确认归类区域的 `InboxTargetGame`：`媒体收件箱归类目标游戏`。
- 当前游戏媒体区域的 `MediaFilter`：`媒体类型筛选`。
- 所选媒体详情区域的 `MediaTargetGame`：`重新归类目标游戏`。

三个控件继续使用原有 Games/MediaFilterOptions 数据源、SelectedItem Binding 和后续命令；没有改变媒体归类、筛选、移动或文件语义。

## 验证结果

- `R21AutomationValueBehaviorTests`：`7/7` 通过；新增行为测试实际创建三个 WPF ComboBox AutomationPeer，检查三个语义名称，并验证选中值分别切换到第二个合成选项。
- 相关定向筛选：`55 passed / 0 failed / 0 skipped / 55 total`。
- 生产源码契约确认三个名称均附着于目标 Binding；没有只用字符串断言签收选择器行为。
- 当前 source-copy Release 构建目标为 Playnite `net462`、Tests `net472`：`0 errors`；仅保留 `MediaCenterView.xaml.cs:671` 的 2 条既有 `CS8602` warning。
- `python scripts/validate-source.py`：通过；`scripts/check-xaml.ps1`：`24/24` 通过；`git diff --check`：通过。
- WPF 静态检查：`0 errors / 27 warnings / 177 info`，与既有基线一致，未见本批新增诊断。

## 证据边界

证据来自生产 MediaCenter XAML、合成 WPF AutomationPeer、fake/隔离 testhost 和 source-copy。没有运行真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终 presented frame 或宿主性能验证；没有以离屏截图或代理性能替代这些事实。Demo 原目录不可用，沿用已恢复的生产基线。main 工作区仍有用户改动，本批未碰、未合并。

下一可执行任务：继续盘点其他复合选择器和逐控件状态/值负例；R21-02 公共门禁完成后再进入 R21-03 错误播报。
