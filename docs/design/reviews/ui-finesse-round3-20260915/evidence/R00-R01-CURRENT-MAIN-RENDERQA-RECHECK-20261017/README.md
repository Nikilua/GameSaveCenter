# R00/R01 当前 main Render QA 复核

日期：2026-10-17。运行源码身份：`01efb189488e6d6323d9386f7ff68057c0030cbe`（报告 HEAD 与 `git rev-parse HEAD` 相同）。本次只复核当前 Render QA 与证据 freshness，没有产品代码修改。

## 结果

- 使用仓库隔离 SDK `8.0.423`，`scripts/render-qa.ps1` 内 Release RenderHarness build 与执行均成功；Harness 结束标记为 `render-qa OK`，报告中 `PROBLEM` 行数为 `0`。主题覆盖 Light/Dark；报告的 `DpiScale=1.00` 仅是离屏逻辑 DIP。
- 复核旧 R23-08 admission 中“Task/Settings 仍有 13 个 Render QA PROBLEM”的历史结论：当前基线未复现。SettingsLayout probe 覆盖 15 个尺寸案例，报告 45 行头部/分类/body 几何读数；窄宽（760/880/920 DIP）在 560/700/900 DIP 高度下均保留 5 个可测类别、类别末项滚动可达。width<=920 的最小主体 viewport 为 181.33 DIP，高于 Harness 160 DIP 门槛。完整 Render QA 覆盖 Light/Dark。Task 1040×700 shell geometry 为 4/4 readable rows，200 DIP grid viewport，Inspector Visible；任务队列行可读 4/4。
- `check-ui-evidence-freshness.ps1 -HeadCommit HEAD -AsJson`：14 fresh / 0 stale / 0 matched source paths / 2919 compared paths；package identity `not-provided`。精简逐记录结果见 [freshness-summary.json](freshness-summary.json)，SHA-256：`9FC89AC9E4F4829A203F5DE733296EADAAF01B6C681FF10DF0296FC6AC354966`。源码 freshness 只核对登记源路径是否变化，不替代 package/host 验收。
- 最新独立 UI 静态审计的 13 MEDIUM `TOOLBAR_VERTICAL_EXPANSION` warnings 仍单独保留；本次 Render QA 通过不将其改记为缺陷已修复，也没有清掉静态审查风险。

## 可复查摘要与边界

- [筛选后的 Render QA 结果](render-qa-summary.txt)，含报告身份、全部 SettingsLayout 行、关键 Task/Settings 几何和 0 个 PROBLEM。原始报告 SHA-256：`A5A08C2008B2F43F091345EBFD840D696089E238D34700403639915CBCB401B9`；摘要 SHA-256：`228C0ADA56E62A2AA458C40628D49F7D4BD421CC72FAA5C3B79D17EAA40EBC1D`。
- RenderHarness 输出记录 `WorkingTreeClean=False`：当时共享工作区有其他任务留下的未跟踪 TRX/TestResults；`git diff HEAD` 无 tracked changes。本次不纳入、不删除这些文件。
- 报告中插件 DLL 标记 `0.6.73+unknown`；没有 package identity。结果来自本机离屏 Harness 的合成数据，未运行 Playnite host、真实 UIA/IME/Narrator、物理 DPI/跨屏或最终呈现帧。
- 原 .tmp/r00-r01-renderqa-current-main-20261017 仅含本次生成的离屏 PNG/主题缓存和完整报告，摘要已归档后精确删除；未清理任何其他任务目录。

## 双 SDK CI 交叉验证

文档提交 `01efb189` 对应 Actions [#1053](https://github.com/Nikilua/GameSaveCenter/actions/runs/38937972835) 已终态成功。SDK `8.0.423` job `117098144434` 与 SDK `10.0.401` job `117098144743` 均 Release build `0 warning / 0 error`、Core `125/125`、Worker `362/362`、Playnite WPF 隔离 `125` 类通过。完整 SDK10 日志确认实际选择 `10.0.401`。SDK8 还完成 `render-qa OK / 0 PROBLEM`、WPF 静态检查（30 个 XAML：0 errors / 27 warnings / 177 info）、`0.6.73` 扩展包生成及包/诊断 artifact 上传。

该 CI 验证的是当前主线代码输入，不会覆盖另一台机器 R09 `1/2`、整体 exit `1` 的实际失败。其断言行和 Thickness 值与当前 R09 测试源不对应；失败同次 `summary.json`/TRX 与测试 DLL/PDB 身份仍缺，具体构建输入根因未定。
