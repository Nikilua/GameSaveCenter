# R21-02 Maintenance 进程映射选择器名称与值证据

日期：2026-09-21  
代码提交：`915ac77c`（`补充进程映射选择器名称`）  
分支：`codex/ui-finesse-round2`

## 本批范围

本批只处理 Maintenance 映射编辑器中已有的 `ProcessMappingTargetGameComboBox`：

- 为“进程映射目标游戏”选择器补稳定的 Automation 名称。
- 保留原有 `Games` 数据源、`ProcessMappingTargetGame` `SelectedItem` Binding、游戏名称模板和“绑定”命令；没有改变进程映射保存或删除语义。

## 验证结果

- `R21AutomationValueBehaviorTests`：`9/9` 通过；新增行为测试实际创建 WPF ComboBox AutomationPeer，检查“进程映射目标游戏”名称并验证合成选项从“游戏 A”切换到“游戏 B”。
- 相关定向筛选：`57 passed / 0 failed / 0 skipped / 57 total`。
- 生产源码契约确认名称附着在 `ProcessMappingTargetGame` 选择器；没有只用字符串断言签收选择器行为。
- 当前 source-copy Release 构建目标为 Playnite `net462`、Tests `net472`：`0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning。
- `python scripts/validate-source.py`：通过；`scripts/check-xaml.ps1`：`24/24` 通过；`git diff --check`：通过。
- WPF 静态检查：`0 errors / 27 warnings / 177 info`，与既有基线一致，未见本批新增诊断。

## 证据边界

证据来自生产 Maintenance XAML、合成 WPF AutomationPeer、fake/隔离 testhost 和 source-copy。没有运行真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终 presented frame 或宿主性能验证；没有以离屏截图或代理性能替代这些事实。Demo 原目录不可用，沿用已恢复的生产基线。main 工作区仍有用户改动，本批未碰、未合并。

下一可执行任务：继续盘点其他复合选择器和逐控件状态/值负例；R21-02 公共门禁完成后再进入 R21-03 错误播报。
