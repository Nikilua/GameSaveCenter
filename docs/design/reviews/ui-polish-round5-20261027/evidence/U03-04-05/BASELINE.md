# U03-04/05 输入框状态、搜索和清除

## 实现

生产资源提交：`d1fc1308317fb805d61d67a7dda77d71e0afff18`（`完善输入框禁用与错误反馈`）。

- 移除共享 WPF-UI TextBox 禁用时整控件 `Opacity=0.45` 的做法，改用语义化禁用文字、填色和描边；验证错误优先级高于禁用态，错误框仍有明确描边。
- 将首条验证错误绑定到 `AutomationProperties.HelpText`，不挤占输入内容区域。
- 任务与媒体搜索原有输入绑定、清除按钮及结果摘要分区得到源码回归约束；清除入口仍位于编辑框外层，不覆盖输入区域，数量仍在独立摘要区。
- 不改变任何命令、Binding、错误/取消业务流程、IME/键盘契约或 Playnite 页面结构。生产 TextBox 当前没有对应的忙碌业务操作，因此没有新造 TextBox 忙碌态；现有忙碌命令继续由按钮状态承载。

## 验证

使用仓库本地 SDK `8.0.423`、Release。精确源码提交 `d1fc1308317fb805d61d67a7dda77d71e0afff18` 的隔离重跑共 `32/32` 通过、0 skip、0 fail：

| 验证 | 通过数 |
| --- | ---: |
| `AccessibilitySourceTests` | 5/5 |
| `R09FocusOutlineBehaviorTests` | 2/2 |
| `CommandDirectoryKeyboardBehaviorTests` | 7/7 |
| `GamePickerKeyboardBehaviorTests` | 7/7 |
| `R10SearchShortcutFocusBehaviorTests` | 1/1 |
| `R16SettingsSearchBehaviorTests` | 1/1 |
| `TaskCenterViewResponsiveTests` | 8/8 |
| `TaskSearchTextKeepsVisibleViewportAtExistingFieldHeight` | 1/1 |

每份 TRX 与 SHA-256 见 [`tests-manifest.json`](tests-manifest.json)。初次混合类运行留下的 [`precommit-focused.trx`](tests/U03-04-05-precommit-focused.trx) 保留 29 pass / 3 fail 诊断：一项新源码断言读错了 `Grid.Row` 所在父元素，已修正；两项 WPF 焦点测试在混合STA窗口进程里拿到非激活窗口、`actual=0x0`，其后独立隔离重跑 `R09FocusOutlineBehaviorTests` 为2/2通过。该失败结果保留，不计入通过数。`MediaSearchBoxUsesStretchableColumn` 的唯一既有 skip 明确属于已撤销的“今日工作台”架构，不计通过。

源码结构检查通过；`check-xaml.ps1` 通过26/26；WPF静态检查扫描37个XAML，0 error、26 warning、179 info（既有基线）。RenderHarness Release 编译0 warning/0 error。精确源码SHA的双主题 Render QA 退出码0，394张PNG、0 `PROBLEM`；报告的源码身份为 `d1fc1308317fb805d61d67a7dda77d71e0afff18`。报告写有 `WorkingTreeClean: False`，原因是仓库中另有未跟踪的历史/其他任务证据和TRX；运行时已核对本批产品源码对应 d1fc 提交。完整报告与浅/深主题任务页代表图已归档；报告SHA-256为 `DEF766366B22BC75FE28FE171CECF29187BBC1314A338B6E697D55D072167396`。

![Light theme task page](render/Task-1040x700-light.png)

![Dark theme task page](render/Task-1040x700-dark.png)

## 远端与未验边界

GitHub workflow `.github/workflows/windows-build.yml` 对精确提交 `d1fc1308317fb805d61d67a7dda77d71e0afff18` 的 run `40566121765`（#1327）已完成：baseline job `122407721274` 的15个步骤和 SDK10.0.401 job `122407720730` 的8个步骤全部成功，包括编译测试、离屏渲染QA、WPF静态审查与打包。扩展包 artifact `12766582766`（90,778,364 bytes，SHA-256 `4acb67990818e5ef0954e19c109f6716e80c30d7ea17631659acdac10ca4c249`）；baseline诊断 `12767536172`（1,971,299 bytes，SHA-256 `298c3d1fc561300a11bba67193566d0fcb903846cc6a6e61501e1faef4b75c19`）；SDK10诊断 `12765271805`（941,795 bytes，SHA-256 `ec728e240dab67faaec654c41514c3ed7b1704c91dcc2125443d20db5ee2ee35`）。完整身份见 [`ci-run-40566121765.json`](ci-run-40566121765.json) 与 [`CI 清单`](../U02-05-06/ci-runs.json)。required-checks接口先前返回HTTP 403，保护规则状态仍“远端待验”；这不影响两个适用workflow job的成功结论。

Render QA 是离屏WPF证据，不代表真实Playnite渲染、Narrator/实际UIA客户端、物理鼠标或实体DPI。未执行真实存档恢复、媒体删除或云端写入。历史清理命令曾被执行策略拦截；2026-11-09只读复查时，该临时目录已不存在，本次未运行删除命令且不推断其消失原因或回收空间。复查记录见 [`cleanup.json`](checks/cleanup.json)。
