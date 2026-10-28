# U09-01/02/03 回归证据核对

2026-10-28。本次对照 U00-01 基线截图、Round5 已归档前后图、键盘/焦点测试覆盖与当前生产页渲染。当前生产源码提交为 `d4c4357c4e0c256b4fe0248d7efc25ce5badfd18`；本次修正只改键盘回归测试夹具，不改生产 XAML 或控件。

## 当前主题与尺寸样图

以中文 Windows 包装链之外的独立 `scripts/render-qa.ps1 -Configuration Release -Output .tmp/round5-u09-01-03/render` 生成当前 RenderHarness 报告。进程退出码 0，`render-qa OK`，394 张 PNG、0 `PROBLEM`；Harness Release 编译 0 warning / 0 error。覆盖 Light/Dark 与 1040×700、1100×720、1280×720、1366×768、1536×864、1600×900、1707×960、1920×1080、2048×1152、2560×1440、3840×2160 DIP。报告 SHA-256 为 `4E5F449D5FAC94FBEB1A43B22B75C0A9F566B8601F7D57EFEB2355E55D8A3118`（1,159,857 bytes），本地完整报告留在 `.tmp/round5-u09-01-03/render/render-qa-report.txt`。

本证据只归档 1040×700 存档页双主题当前样图：[Light](current-save-light-1040x700.png)、[Dark](current-save-dark-1040x700.png)。哈希、基线配对和生成时间见 [screenshots.json](screenshots.json)。U00-01 的前一组图仍见 [U00-01 基线](../U00-01/BASELINE.md)。

这些图**不能标为同数据前后对照**：旧图来自 `f5cddfe9`，当前图来自 `d4c4357c`；Harness 假数据中的相对时间由运行时 `DateTime.UtcNow` 生成。两次图的样本时间从“昨天 11:28”变成“昨天 17:00”，当前页还呈现新的游戏身份与工具栏次序。像素哈希不同同时包含预期 UI 改动和样本数据变化，不能单独当作回归。已核对历史上有明确前后图的是 [U06-07/08 候选表](../U06-07-08/BASELINE.md)，该配对覆盖候选表高窗调整；其余批次的逐批、同数据双主题前后图需要再补齐。U09-01 保持进行中。

## 键盘与 UI Automation 覆盖

SDK 8.0.423 Release 候选工作树定向组合 16/16 通过、0 skip：`R21KeyboardNavigationTraceTests`、`R09FocusOutlineBehaviorTests`、异步完成提示、通知筛选与安全说明，以及 `R07StatusBannerBudgetBehaviorTests`。候选原始TRX在 `.tmp/round5-u09-01-03/combined/u09-02-combined.trx`，[脱敏副本](tests-candidate-redacted.trx)已归档。该次测试包含相对 `d4c4357c` 尚未提交的夹具改动；修复提交后还要以新提交 SHA 重跑并归档精确身份。

检查了测试实现：Tab/Shift+Tab 用 WPF `MoveFocus` 追踪八个生产入口的前后焦点次序；UIA 覆盖控件 Automation Name、共享焦点环和 `IInvokeProvider`。不能据此声称实体键盘输入、真实输入法/IME、外部 UIA 客户端或 Playnite 内焦点已验收。Round5 真实宿主及权限待验项保持原状态，不重复探测。U09-02 保持进行中。

CI run [40660226413](https://github.com/Nikilua/GameSaveCenter/actions/runs/40660226413) 对 SHA `d4c4357c4e0c256b4fe0248d7efc25ce5badfd18` 的 baseline 与 SDK10 job 都在 `R07StatusBannerBudgetBehaviorTests.SaveStaleBannerKeepsRetryAndHistoryTableFloor` 失败。原始 job 日志显示 `InvokeButton` 未收到 Click；离屏诊断确认测试 `SaveBannerContext` 未提供 `SaveTabIndex`，TabControl 没有选中历史页，按钮 `IsVisible=false`。仅补测试夹具默认页签并增加可见/排列断言，没有删断言或改业务命令；修正后本地该类 5/5，组合 16/16。原始 run/job/artifact 身份见 [ci-failure.json](ci-failure.json)。本轮修复提交的远端重验仍待完成。U09-06 保持进行中。

## 尺寸、主题与 DPI 边界

当前 Render QA 在逻辑 DPI 1.00 的离屏 WPF 窗口覆盖上述 11 个尺寸和两种主题，无 `PROBLEM`。3840×2160 是逻辑 DIP 测量，不是实体 4K 显示器验证；真实多屏/物理 DPI 切换、Playnite 窗口缩放及外壳主题交叉尚未测。U09-03 保持进行中。

本轮 RenderHarness 产品源码与当前 `d4c4357c` 一致；界面源文件没有本轮改动，最近的 WPF 静态审查见 [U08-07/08](../U08-07-08/BASELINE.md)。本次通过 `scripts/validate-source.py` 与 `git diff --check`；它们不替代完整构建或真实宿主验收。

## 输出与清理

本阶段 `.tmp/round5-u09-01-03` 当前自有输出为 405 个文件、54,641,150 bytes，包含当前 394 图报告、定向测试与初始失败诊断；现阶段报告和失败诊断仍用于验收。开工快照 C/D 可用空间分别为 3,073,388,544 / 14,878,277,632 bytes；本次记录时为 3,138,142,208 / 13,700,571,136 bytes。两次空间差异不归因于本阶段；没有删除文件，也不声称释放空间。旧 Round3 的用户 TRX/TestResults 及其他任务未改动。清理边界、运行占用、引用检查及最终删除清单留给 U09-07 完成前核对。
