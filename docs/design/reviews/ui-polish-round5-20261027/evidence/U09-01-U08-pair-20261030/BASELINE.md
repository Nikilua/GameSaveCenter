# U09-01：U08-01/02 同数据历史配对

## 生产改动

对照快照为 `18481e6d8d3415ffc31362764980614e76445848` → `fd2d1316be9ff2d65b0851ac57c45e40064e50ac`。生产差异只有两处：概览 Hero 眉题从“待处理”改为“当前状态”；存档历史摘要增加所选游戏名称，并保留完整身份 Tooltip 与 UIA HelpText。原命令、历史列表、恢复对象、校验与滚动行为没有变化。行为测试文件也在 after 快照中新增，未改生产业务逻辑。

两侧均由对应提交的 Git archive 重建。archive SHA、截图及构建身份见 [pair.json](pair.json)。渲染报告中的 `Commit` 字段为 `cc05729c…`，是 archive 位于仓库 `.tmp` 下时父仓库返回的 HEAD，不能用于辨认快照；源码身份以这里记录的提交与 archive SHA 为准。

## 构建和回归

Before、After 各自用 SDK 8.0.423 / Release 编译真实 Worker 与 RenderHarness，均为 0 warning、0 error。Worker.exe 来自对应快照源码，未使用占位文件。每侧完整 `scripts/render-qa.ps1` 均退出 0，生成 394 张 PNG、0 `PROBLEM`；双主题、1040×700 DIP 目标图已逐张视觉检查。完整报告和原始入口日志见 [Before 报告](before-render-qa-report.txt)、[After 报告](after-render-qa-report.txt)、[构建与捕获日志](before-render-qa.log) 和 [After 日志](after-render-qa.log)。

样本时间固定为 `2026-10-28T09:00:00Z`。两份隔离 RenderHarness 假数据中的 UTC 时间替换为同一 FakeSampleClock；运行中任务的 `ElapsedSeconds` 也固定为 126720 秒，避免 DTO 对“无累计时长”的真实 `UtcNow` 回退造成非目标截图漂移。三份 test-only 文件在两侧 SHA 完全一致；生产文件保持原始 archive 内容。38 张差异图全部位于概览或存档历史目标视图，另外 356 张像素 SHA 相同，任务页时长截图无差异。

首次 before 捕获未编译 Worker，SettingsState 探针报告 4 个错误；随后从 before 和 after 源码分别编译 Worker，原有探针恢复通过。失败报告保留于 [首次失败诊断](initial-worker-missing-render-failure.txt)，没有改断言或跳过探针。

after 快照的 `Round5OverviewSaveContextTests` 行为回归：SDK 8.0.423、net472，1/1 通过、0 失败、0 skip、退出 0；[TRX](Round5OverviewSaveContextTests.trx) 与 [运行日志](behavior-test.log)。首次测试运行遇到 .NET Framework 260 字符路径限制，最终改用仓库 `.tmp` 内短 junction 和临时目录运行，源码、筛选和断言均未改。

WPF 技能静态审查针对当前生产项目目录（该快照是当前 HEAD 的祖先）：26 个 XAML、0 error、26 warning、164 info，完整输出见 [静态审查日志](wpf-static-review.txt)。`OverviewView.xaml` 两条 StackPanel 警告由审查器的 2500 字符文本邻近启发式触发；对应容器是 Grid 中只有标题和说明文字的表头 StackPanel，长列表位于同级 Grid 行内受限的 ScrollViewer，不是 StackPanel 测量列表。双侧完整 Render QA 的 394 张图无 `PROBLEM`。本次在 `SaveCenterView.xaml` 无警告。技能规则指定的 `GameSaveCenter.AcrylicFork/.../Design` Demo 文件树不在此工作树中，因而不能声称本次完成直接 Demo 视觉对照；本证据批次未改生产 UI，只核对了其实际生产资源、快照差异与渲染。

## 目标截图

| 页面 | 主题 | Before | After |
| --- | --- | --- | --- |
| 概览 | 浅色 | [截图](overview-light-before.png) | [截图](overview-light-after.png) |
| 概览 | 深色 | [截图](overview-dark-before.png) | [截图](overview-dark-after.png) |
| 存档历史 | 浅色 | [截图](save-light-before.png) | [截图](save-light-after.png) |
| 存档历史 | 深色 | [截图](save-dark-before.png) | [截图](save-dark-after.png) |

运行环境为 RenderHarness 离屏 WPF、逻辑 DPI 1.00；未观察真实 Playnite 宿主，也未执行真实存档恢复、媒体删除或云端写入。U08-01/02 继续标为“代码完成待验收”；U09-01 继续进行，尚有其它视觉批次待盘点。

本批证据文件SHA-256清单见 [SHA256SUMS.txt](SHA256SUMS.txt)；临时目录与磁盘空间清理记录见 [cleanup.json](cleanup.json)。
