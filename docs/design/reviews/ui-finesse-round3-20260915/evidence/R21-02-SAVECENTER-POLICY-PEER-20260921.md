# R21-02 SaveCenter 策略控件外置标签与状态证据

日期：2026-09-21  
证据提交：`e1fadaab`（`补充存档策略控件行为证据`）  
分支：`codex/ui-finesse-round2`

## 本批范围

本批不修改生产 XAML，只为 SaveCenter 已有策略控件补实际 WPF peer 证据：

- “启用备份策略”“游戏退出后自动备份”“游玩中定期备份”“备份后自动上传云端”四个外置文本标签对应的 ToggleSwitch。
- “异常保护等级”和“选择策略模板”两个 ComboBox。
- “锁定所选版本” CheckBox。

这些控件继续使用现有 `SelectedGame.Policy.*`、`SelectedPolicyTemplate`、`LockSelectedBackup` Binding；没有新增服务、DTO、命令、标签体系或存档写入语义。

## 验证结果

- `R21AutomationValueBehaviorTests`：`12/12` 通过；新增行为测试实际创建七个 WPF 控件 peer，检查四个 ToggleSwitch 和 CheckBox 的语义名称与 `Off → On`，并验证两个 ComboBox 的选值变化。
- 相关定向筛选：`60 passed / 0 failed / 0 skipped / 60 total`。
- 原有源码契约仍确认外置标签文本、Binding 和 `AutomationProperties.Name` 处于同一策略行；本批没有只用 `Assert.Contains` 签收控件状态交互。
- 提交身份 `GscBuildCommit=e1fadaab` 的 D 盘源码副本 Release 构建目标为 Playnite `net462`、Tests `net472`：`0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning。链接工作树直接触发 WPF `_wpftmp.csproj` 仍遇 `Access denied`，改用项目已有 D 盘源码副本流程完成同一身份验证，没有绕过权限。
- `python scripts/validate-source.py`：通过；`scripts/check-xaml.ps1`：`24/24` 通过；`git diff --check`：通过。
- WPF 静态检查：`0 errors / 27 warnings / 177 info`，警告和信息均为既有基线，未见本批新增诊断。
- 本批 D 盘源码副本和隔离构建目录已清理。

## 证据边界

证据来自已有生产 SaveCenter XAML、合成 WPF 控件 peer、fake/隔离 testhost 和隔离源码副本；它证明 Automation 名称、Toggle/CheckBox 状态和 ComboBox 选值在受控控件上可读，不等于真实 Playnite UIA/读屏或最终呈现验证。没有运行真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终 presented frame 或宿主性能验证；没有写真实存档、媒体、云端或诊断。Demo 原目录不可用，沿用已恢复的生产资源基线。main 工作区仍有用户改动，本批未碰、未合并。

下一可执行任务：继续盘点剩余复合选择器和逐控件状态/值负例；R21-02 公共门禁完成后再进入 R21-03 错误播报。
