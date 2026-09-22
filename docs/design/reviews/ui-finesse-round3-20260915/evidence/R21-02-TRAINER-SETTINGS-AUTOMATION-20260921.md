# R21-02 TrainerCenter 工具设置名称与状态证据

日期：2026-09-21  
代码提交：`5d9cb81c`（`补充Trainer设置控件名称`）  
分支：`codex/ui-finesse-round2`

## 本批范围

本批只处理 TrainerCenter 已有工具设置区域的复合选择器与开关：

- `SelectedGameTool.Versions` 选择器：`工具版本`。
- `GameToolIfAlreadyRunningOptions` 选择器：`已有实例时处理方式`。
- `GameToolRiskCategoryOptions` 选择器：`工具风险类别`。
- `SelectedGameTool.Enabled`、`AutoStart`、`CloseOnGameExit`、`RequiresAdmin` 四个 ToggleSwitch：分别补 `启用所选工具`、`随游戏启动所选工具`、`退出游戏后关闭所选工具`、`管理员权限启动所选工具`。

继续复用原 Binding、`CanTrackProcess` 禁用条件和 ToggleSwitch 实现，没有改变工具启动、进程跟踪、保存或风险判定。

## 验证结果

- `R21AutomationValueBehaviorTests`：`6/6` 通过；新增行为测试实际创建三个 WPF ComboBox peer 和四个 ToggleSwitch peer，检查语义名称、Toggle `Off → On` 和版本选中值变化。
- 相关定向筛选：`54 passed / 0 failed / 0 skipped / 54 total`。
- 生产源码契约确认七个新名称各自落在目标 XAML 控件上；没有把仅有的字符串 `Assert.Contains` 当作交互完成定义。
- 当前 source-copy Release 构建目标为 Playnite `net462`、Tests `net472`：`0 errors`；仅保留 `MediaCenterView.xaml.cs:671` 的 2 条既有 `CS8602` warning。
- `python scripts/validate-source.py`：通过；`scripts/check-xaml.ps1`：`24/24` 通过；`git diff --check`：通过。
- WPF 静态检查：`0 errors / 27 warnings / 177 info`，与既有基线一致，未见本批新增诊断。

## 证据边界

证据来自生产 TrainerCenter XAML、合成 WPF AutomationPeer、fake/隔离 testhost 和 source-copy。没有运行真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终 presented frame 或宿主性能验证；没有以离屏截图或代理性能替代这些事实。Demo 原目录不可用，沿用已恢复的生产基线。main 工作区仍有用户改动，本批未碰、未合并。

下一可执行任务：继续盘点其他复合选择器和逐控件状态/值负例；R21-02 公共门禁完成后再进入 R21-03 错误播报。真实宿主 UIA、呈现、DPI/IME、性能仍是未验边界。
