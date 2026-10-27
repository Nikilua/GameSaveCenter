# U04-03/04/05/07 按压、选择与输入反馈

日期：2026-10-28。生产代码/测试提交：`4225eb17139a844ede87219f02a9ad97e88a12de`。

## 去重与本轮改动

- **U04-03 按压释放接续：**既有 R08 已从真实渲染值检查主按钮按压、移出/松开与快速重新按下的连续性；Q06 覆盖焦点、禁用、卸载和 `CanExecute=false` 时不误执行命令。本轮复用这些测试，没有另造按压动画。
- **U04-04 选择指示过渡：**共享 `GscRedesignWorkspaceTabItem` 增加 3×16 DIP 强调色条，只对 opacity 做 `GscMotionNormal`（220 ms）渐入/渐出并使用共享 ease-out。入场先移除退场时钟，反向亦然；选中底色、键盘焦点环和页签内容布局不变。共享样式覆盖存档、媒体、维护和训练中心工作区页签。
- **U04-05 输入反馈时序：**现有共享 TextBox 焦点环、即时校验边框和错误 HelpText 均不等待动画；GamePicker 清除操作立即清空文本并把焦点还给搜索框。本轮没有增加延迟或改输入契约。
- **U04-07 降低动态效果：**关闭 `MotionEnabledActive` 时通过模板 setter 直接呈现选中/未选中终态，并移除进入、退出两种 storyboard。无缩放/位移，没有延迟操作时机。
- 原始 SDK8.0.423 与 GitHub 日志复核还发现 `SharedDisclosureChevronUsesDemoRotationTiming` 仍断言旧的 150 ms 字面量；生产实现已映射到 120 ms `GscMotionFast`。本轮更新测试为验证共享令牌。R08/Q14 测试夹具移除对跨 STA 全局 `Application` 写入的依赖，保留原动画、命令和几何断言。

## 本地验证

- SDK：8.0.423；Release，隔离输出根 `.tmp/round5-u04-03-05-build-06d4e854`。当前候选精确提交为 `4225eb17…`；聚焦 TRX 文件名保留之前的构建身份 `06d4e854`，用于追溯测试产生时源码父提交；代码测试修正随后由当前提交收录。
- 动效/输入/按钮组合：15/15 通过、0 失败、0 skip。TRX：[`U04-03-05-focused-sdk8-06d4e854.trx`](tests/focused-suite-final/U04-03-05-focused-sdk8-06d4e854.trx)，SHA-256 `678820FAABC9F038B5492EF647C9627715306917A87534148C95BFCB2F948B5B`。包括新页签双主题入退场、中途反选、终值稳定、焦点分离和无动效即时状态；R08、Q06、GamePicker 键盘、R09 焦点/验证及 Q14 68 组工具栏几何。
- U04-01 共享 DisclosureCard 时长契约：1/1 通过、0 skip；TRX `tests/u04-01-ci-fix/U04-01-shared-motion-token-sdk8-06d4e854.trx`，SHA-256 `7BE611DFBFA4FB2B366091B570F366087DC88E341DFE988341C5AAF67C7ADD74`。
- `python scripts/validate-source.py` exit 0；`scripts/check-xaml.ps1` 检查 26 个 XAML；WPF 静态审查 37 XAML、0 error、26 warning、179 info；RenderHarness Release 0 warning/0 error；`scripts/render-qa.ps1` exit 0，394 PNG、0 `PROBLEM`/`ERROR`。报告位于本机 `.tmp/round5-u04-03-05-render-06d4e854/render-qa-report.txt`，SHA-256 `DB1A7AF41DF0B511A83105C3BF802C5DB6FCF9A1CAC3E906627E09336DB97FB9`；report 记录 OffscreenRenderHarness/DPI 1.00，不代表真实 Playnite 或实体 DPI。
- 首轮失败的选择指示测试与组合回归也保留在本地 `tests/selection-*`、`tests/focused-suite*` 目录，用于说明测试夹具的动画预热、HoldEnd 终值及 STA 资源归属修正；最终成功 TRX 为上述 `focused-suite-final` 和 `u04-01-ci-fix`。

## GitHub CI 与边界

- 精确修复提交 `4225eb17139a844ede87219f02a9ad97e88a12de` 已推送；run `40578426793`（#1334）baseline job `122451017347`、SDK10.0.401 job `122451017062`在运行。纯文档提交 `06d4e854` 的 run `40575315327`（#1333）baseline `122440055009`、SDK10 `122440055220`也在运行。run/job/artifact快照见同目录上层 `../U03-08-U04-01-02/ci-runs.json`。
- 旧失败 run `40571484630` 与 `40572542627` 的两套 SDK 原始 job 日志已读取：唯一失败是 `SharedDisclosureChevronUsesDemoRotationTiming` 仍要求 `To="-90" Duration="0:0:0.15"`；对应 CI artifact ID、尺寸、SHA-256 在 `ci-runs.json`。较早 run `40570767550` 的 R08 跨 STA `Application.set_ShutdownMode` 故障也按测试宿主问题修正；这些旧失败保留为历史，不重跑。
- required-checks API 曾返回 403 `Resource not accessible by integration`，因此仍不能确认 branch protection 要求。真实 Playnite 主机、实际 UIA 客户端、Narrator、物理鼠标/IME、实体 DPI 和外部显示器均未验；只做离屏与合成输入。真实存档恢复、媒体删除和云写入没有执行。最终候选根入口全链及独立 `scripts/build.cmd` 尚未开始。

## 输出与清理

- Render QA 当前输出：395 文件/53,246,317 bytes；隔离测试 build 输出：330 文件/28,891,017 bytes。两者均为本批自有 `.tmp` 目录，保留当前审计与可复现输出；没有重复保存 PNG。
- 运行前空闲：C `1,478,320,128` bytes、D `6,165,086,208` bytes；完成后 C `1,473,875,968`、D `4,474,048,512`。本批可计量输出约 82,137,334 bytes；D 盘另有约 1.61 GB 空间变化无法归因到本批，未据此清理其他目录。已记录的旧 `.tmp` 删除尝试曾被执行策略拒绝，本轮没有更换删除方式，也未删除任何其他任务的 TRX、TestResults、存档、配置或未知文件。
