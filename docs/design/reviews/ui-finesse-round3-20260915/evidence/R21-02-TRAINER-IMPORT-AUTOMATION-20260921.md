# R21-02 TrainerCenter 导入控件名称证据

日期：2026-09-21  
代码提交：`c327f92a`（`补充Trainer导入控件名称`）  
分支：`codex/ui-finesse-round2`

## 本批范围

本批只处理 TrainerCenter 现有“导入确认”状态的复合选择器和动作：

- 紧凑确认卡与“导入确认”页中的两个已有 `ImportEntryCandidates` ComboBox，统一暴露 `选择待导入修改器主程序`。
- 两个现有 `ConfirmGameToolImportCommand` 按钮统一暴露 `确认导入游戏工具`。
- 两个现有 `CancelGameToolImportCommand` 按钮统一暴露 `取消导入游戏工具`。

命令、Binding、导入候选值、确认/取消语义和可见性没有改变，没有新增服务或文件操作。

## 验证结果

- `R21AutomationValueBehaviorTests`：`5/5` 通过；新增行为测试实际创建 WPF ComboBox/Button AutomationPeer，检查三个名称，并把候选项从 A 切换到 B 后确认选中值变化。
- 相关定向筛选：`53 passed / 0 failed / 0 skipped / 53 total`。
- 生产源码契约检查确认两处现有呈现路径各自保留同一语义名称，未只验证合成控件。
- 当前 source-copy Release 构建目标为 Playnite `net462`、Tests `net472`：`0 errors`；仅保留 `MediaCenterView.xaml.cs:671` 的 2 条既有 `CS8602` warning。
- `python scripts/validate-source.py`：通过；`scripts/check-xaml.ps1`：`24/24` 通过；`git diff --check`：通过。
- WPF 静态检查：`0 errors / 27 warnings / 177 info`，与既有基线一致，未见本批新增诊断。

## 证据边界

证据来自生产 TrainerCenter XAML、合成 WPF AutomationPeer、fake/隔离 testhost 和 source-copy。没有运行真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终 presented frame 或宿主性能验证；没有以离屏截图或代理性能替代这些事实。Demo 原目录不可用，沿用已恢复的生产基线。main 工作区仍有用户改动，本批未碰、未合并。

下一可执行任务：继续 R21-02 剩余 Trainer 工具设置复合选择器/开关及其他逐控件状态值负例；完成公共门禁后再进入 R21-03 错误播报。
