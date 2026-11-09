# U04-06 / U04-08 本地验证

日期：2026-10-28。源码提交 `c5c86558b6232995438657c00810aeb327ed9ba5` 已推送。

## 改动与覆盖

- U04-06：共享 `GscDisclosureCardExpander` 的 Chevron 在打开转场中禁用时立即移除 Storyboard 时钟，保留 `IsExpanded` 业务值，并通过 `IsChecked` 绑定恢复合法角度。重新启用不会回放旧的打开动画；之后用户再次切换仍会正常收起。Light/Dark 各验证一次。
- U04-08：在同一生产 Expander 实例执行100次展开/收起；每次都核对 Header、Path、RotateTransform 对象身份不变。结束后切换 Reduced Motion，核对角度处于关闭终值且活动动画时钟已移除。
- Q14 CI 回归：测试夹具通知 `SelectedTaskFilterPreset` 属性变化，并在窗口加载后模拟真实 ViewModel 的绑定更新，避免误把加载时序问题归因于生产工具栏。

本批不调用存档恢复、媒体删除、云写入；不改变业务命令、Binding、安全取消语义、游戏选框或滚动条。CanExecute/取消流程、真实窗口失活、Playnite宿主及实际系统动画设置未由本批自动化直接驱动，保持待验收边界。

## 验证

- SDK `8.0.423`，精确提交 Release 聚焦组合：13/13通过、0失败、0跳过；TRX `tests/commit-c5c86558-exact/U04-06-08-focused-sdk8-c5c86558.trx`，SHA-256 `833588BF1FAAE0DFEE552BD805FD279A1EE5FD59B15EC917645B1C1005C4C602`。该运行包括两主题禁用中断、100次对象生命周期、Q14与共享Chevron结构断言。首次提交后尝试错误传入缩写身份并中断，日志保留在 `tests/commit-c5c86558/test-run.txt`，因程序集身份保护失败、不计通过；正确完整SHA运行通过。
- Q14隔离回归：1/1通过；TRX `tests/Q14/U04-06-08-Q14-isolated-sdk8-4225.trx`，SHA-256 `F6F8798D7A84FE453B7DE2E5B04E6A2840C9AB6DD2F9A59103B791CE5F12D64A`；覆盖68个布局几何场景。
- `scripts/validate-source.py` 通过；输出 `source-validation.txt`，SHA-256 `30760E7082311AC27AA2D981C07638FC2A4B11D039C265AFB40365B1F98FBCA6`。
- `scripts/check-xaml.ps1` 通过：26个XAML；输出 `xaml-check.txt`。
- `.codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .` 通过：37个XAML、0错误、26条现存警告、179条信息；输出 `wpf-static.txt`，SHA-256 `4B55D55220B148B5588CE01E369738030700C4E9AD0850862548019D29435334`。
- `scripts/render-qa.ps1 -Configuration Release -Output .tmp/round5-u04-06-08-render-4225` 当时由RenderHarness Release编译0 warning/0 error，394 PNG、`render-qa OK`、0 PROBLEM。完整日志归档于本目录 `render-qa.log`，SHA-256 `434A0E29E4BD289BDC977A1283BBF18A3C8BF540D70473C0055056F02909B165`；原始报告当时在 `.tmp`，SHA-256 `003C824E733019D273A827BE0157AC3BAAC5291A6F79048A8807F86101ED7B0A`。2026-11-09复核该`.tmp`目录已不存在，不能声称本机原始报告或PNG仍可读取；后续冻结整合候选的完整渲染/打包门禁见下文。

## 远端与验收边界

父提交 `4225eb17` 的 workflow run `40578426793` baseline job `122451017347` 和 SDK10 job `122451017062` 均失败于 Q14 ComboBox.Text 初始化预期；随后源码 run `40583173450`（#1335）两job `122467678548` / `122467678982` 也因同一夹具预期错误终态 failure，诊断artifact `12770991617`（622,156 bytes）和 `12770842930`（529,495 bytes）仍可读取。失败原因与修正历史见同目录上层 `../U03-08-U04-01-02/ci-runs.json`，不把旧失败记作通过。之后测试夹具修正进入冻结整合候选 `5c0c57b06aed27dc46c41ef3d655ec20dafa5ec7`，该精确 SHA 的 GitHub run `40900374673`（#1398）基线 job `123540700192` 与 SDK10 job `123540699717` 全部 success；中文根入口和独立 `scripts/build.cmd` 也分别完成职责验证，具体本地构建、安装身份、artifact和CI证据见[冻结门禁](../U09-final-5c0c57b0/BASELINE.md)。required-checks API此前403，不能据此推断全部分支保护要求。

当前测试 TRX、静态审查日志与历史Render QA摘要为本批审计证据；不声称真实 Playnite / UIA 客户端 / 系统 Reduced Motion 宿主验收通过。
