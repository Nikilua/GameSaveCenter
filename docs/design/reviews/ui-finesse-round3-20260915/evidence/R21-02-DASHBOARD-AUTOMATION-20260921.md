# R21-02 Dashboard 选框与策略控件名称/状态证据

日期：2026-09-21  
代码提交：`6a8cae16`（`补充Dashboard控件名称`）  
分支：`codex/ui-finesse-round2`

## 本批范围

本批只处理 Dashboard 已有游戏选框和当前游戏内联策略编辑器：

- 游戏状态、平台、排序三个 ComboBox，补充稳定的 Automation 名称。
- 当前游戏自动任务、退出/游玩时备份、退出/游玩时归档媒体、备份后上传云端六个策略 ToggleSwitch，及保存当前游戏策略按钮，补充稳定的 Automation 名称。

原有游戏选框筛选、排序、同步和策略 `Binding`、`SavePolicyCommand` 均保留；没有新增服务、DTO、命令或业务语义。

## 验证结果

- `R21AutomationValueBehaviorTests`：`8/8` 通过；新增行为测试实际创建三个 WPF ComboBox AutomationPeer、六个 ToggleSwitch peer 和保存按钮，检查语义名称，验证三个选项切换以及 Toggle `Off→On`。
- 相关定向筛选：`56 passed / 0 failed / 0 skipped / 56 total`。
- 生产源码契约确认 Dashboard 三个筛选器和策略控件名称均附着在目标控件；没有只用字符串断言签收选择器、开关或保存动作行为。
- 当前 source-copy Release 构建目标为 Playnite `net462`、Tests `net472`：`0 errors / 0 warnings`。
- `python scripts/validate-source.py`：通过；`scripts/check-xaml.ps1`：`24/24` 通过；`git diff --check`：通过。
- WPF 静态检查：`0 errors / 27 warnings / 177 info`，为当前基线，未见本批新增诊断。

## 证据边界

证据来自生产 Dashboard XAML、合成 WPF AutomationPeer、fake/隔离 testhost 和 source-copy。没有运行真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终 presented frame 或宿主性能验证；没有以离屏截图或代理性能替代这些事实。Demo 原目录不可用，沿用已恢复的生产基线。main 工作区仍有用户改动，本批未碰、未合并。

下一可执行任务：继续盘点其他复合选择器和逐控件状态/值负例；R21-02 公共门禁完成后再进入 R21-03 错误播报。
