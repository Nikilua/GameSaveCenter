# U02-07/08 阴影预算与主题切换连续性

## 去重结论与本轮增量

`U02-07` 和 `U02-08` 的生产能力已由共享主题资源及 R09 行为回归覆盖，本批没有发现需要改动的样式或主题状态缺口，因此不重复实现视觉体系。代码基线为 `f59fe4c86c78cb2fc11414a431aceb49821910ee`；性能报告写入时 HEAD 是文档提交 `31dc41d947abb8a221d09753181f40888a7fb87b`，核对确认两者之间 `src/GameSaveCenter.Playnite`、Playnite 测试及 RenderHarness 源码没有变化。

- 阴影只由有限共享层级提供：表面 14/2 DIP、侧栏 24/3、弹层 20/5、对话框 34/8、主按钮 18/0、滑块拇指 6/1；列表行和重复卡片不叠加阴影。可见的 `BlurEffect` 仅用于受主题控制的游戏背景与设置环境层，未用于重复列表行。
- 关闭玻璃/透明预算时，共享阴影资源为 `null`，弹层改为不透明，环境色洗白层透明度归零。
- 主题替换在下一次 Render 前更新弹层边框、图标和占位符的动态资源；宿主主题跟随、高对比系统语义色、透明/效果降级和恢复均有行为覆盖。

本批新证据是上述能力的专项复核与性能探针；生产源码和测试源码均未改变。既有双主题 render-qa 矩阵继承自 [U02-05/06](../U02-05-06/BASELINE.md)，该批 334 张 PNG、0 `PROBLEM`，截图 SHA 索引见 [manifest](../U02-05-06/screenshots.json)。

## 行为验证

SDK 8.0.423 Release 定向运行 `R09ShadowBudgetBehaviorTests`、`R09ThemeSwitchBehaviorTests`、`R09HighContrastBehaviorTests` 和 `R09BackgroundPressureBehaviorTests`：9/9 passed，0 failed，0 skipped。原始 TRX：[`U02-07-08-r09-focused.trx`](tests/U02-07-08-r09-focused.trx)，SHA-256 `3CC9A7165F8E31DFBAF1573CE96659076D5A491373831A4C2EDAABA64DF7A3C8`。

阴影预算覆盖 Light/Dark 有限效果层级与低成本模式、透明弹层降级、滚动视口不因卡片阴影扩张；主题/高对比覆盖切换前后动态资源、宿主跟随、系统语义色和效果恢复。没有修改真实命令、Binding、UIA、输入法或业务状态。

## 受控性能探针

报告：[`enduranceprobe-report.txt`](performance/enduranceprobe-report.txt)，SHA-256 `B0F40FB1A79A38B7ED0B7EC9661DBD467EDF27C249B98567541E8BD2F2A88971`。RenderHarness WPF STA 窗口运行 60 秒动作窗口及 30 秒稳定观察，Light/Dark、7 个工作区、1040×700 DIP；82 轮、351 个动作、10 个资源样本、0 动作失败。动作总 P95 714.46 ms、最大 777.4 ms；切页布局代理 P95 550.09 ms，详情打开 P95 51.24 ms，滚动 P95 52.7 ms，合成搜索输入 P95 7.19 ms。

U00 基线报告记录 68 轮/290 个动作/0 失败，总动作 P95 933.16 ms、最大 1138.2 ms。当前代理数值较低，未显示本轮共享效果层引入的退化信号；两次采样跨提交、合成数据和受控离屏窗口，耗时含布局/探针采集并不是呈现帧、物理输入或 Playnite 宿主基准，不能单独证明无性能回退。两个报告分别保留原始计数与限制：[本轮](performance/enduranceprobe-report.txt)、[U00 基线](../U00-06-07/enduranceprobe-report.txt)。

## GitHub 与验收边界

截至 2026-10-28 02:24 +08:00，`windows-build.yml` 适用的 baseline 与 SDK 10.0.401 job 状态见 [`ci-runs.json`](../U02-05-06/ci-runs.json)：提交 `417bed51` 的 SDK10 job成功，baseline已通过 Render QA 和 WPF 静态审查、正在生成扩展包；实现候选 `f59fe4c8` 的 baseline 正在 Render QA、SDK10 正在编译/测试；纯文档提交 `31dc41d9` 的两 job仍运行。SDK10 成功产物身份为 artifact `12758905811`（`GameSaveCenter-sdk10-build-diagnostics`，942058 bytes，SHA-256 `9f953138148f07da4fab4f5d1f10aaec81c0c46d0e0d5bb3c0f171991c81f466`）。required-checks 端点此前返回403，分支保护状态仍为“远端待验”。

D 盘观测可用空间为 `7,025,397,760` 字节。本批保留报告和 TRX 共 53,253 字节；没有删除文件，也未清理 `.tmp/`、`artifacts/`、其他任务的 TRX/TestResults。工作树中已有文档链接和历史审计材料均保留。

状态保持“代码完成待验收”：真实 Playnite 宿主、UIA/键盘/IME、实体 DPI 尚未验证；本批也没有触碰存档恢复、媒体删除或云端写入。不要把受控 WPF 性能代理或未完成的远端 job 记作完整验收。
