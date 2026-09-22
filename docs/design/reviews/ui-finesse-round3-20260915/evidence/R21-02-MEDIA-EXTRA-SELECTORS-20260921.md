# R21-02 MediaCenter 额外选择器名称与值证据

日期：2026-09-21  
证据提交：`eac4276f`（`补充媒体选择器行为证据`）  
分支：`codex/ui-finesse-round2`

## 本批范围

本批不修改生产 XAML，只为 MediaCenter 已有语义名补受控 WPF 行为证据：

- 媒体收件箱视图：`媒体收件箱视图`。
- 已保存媒体筛选组合：`媒体筛选预设`。
- 归类批次状态：`媒体归类批次状态筛选`。
- 收件箱归类建议的目标游戏：`调整归类建议目标`。

四个 ComboBox 继续使用原有选项来源、SelectedItem/SelectedValue Binding 和相关命令；没有新增服务、DTO、命令或业务语义。

## 验证结果

- `R21AutomationValueBehaviorTests`：`10/10` 通过；新增行为测试实际创建四个 WPF ComboBox AutomationPeer，检查语义名称并验证四个选择值从第一项切换到第二项。
- 相关定向筛选：`58 passed / 0 failed / 0 skipped / 58 total`。
- 生产源码契约确认四个名称仍附着于对应 MediaCenter 选择器；本批没有只用字符串断言签收交互。
- 当前 source-copy Release 构建目标为 Playnite `net462`、Tests `net472`：`0 errors / 2` 条既有 `MediaCenterView.xaml.cs:671 CS8602` warning。
- `python scripts/validate-source.py`：通过；`scripts/check-xaml.ps1`：`24/24` 通过；`git diff --check`：通过。
- WPF 静态检查：`0 errors / 27 warnings / 177 info`，与既有基线一致，未见本批新增诊断。

## 证据边界

证据来自已有生产 MediaCenter XAML、合成 WPF AutomationPeer、fake/隔离 testhost 和 source-copy。没有运行真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终 presented frame 或宿主性能验证；没有以离屏截图或代理性能替代这些事实。Demo 原目录不可用，沿用已恢复的生产基线。main 工作区仍有用户改动，本批未碰、未合并。

下一可执行任务：继续盘点其他复合选择器和逐控件状态/值负例；R21-02 公共门禁完成后再进入 R21-03 错误播报。
