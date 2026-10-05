# GameSaveCenter 当前事实入口

更新：2026-10-05；审阅源码：`77de7f450431d267026ce2169aa30525b18bf528`，分支 `main`，版本 `0.6.73-development-preview`。

本页只保留当前结论与下一步，不再累加阶段日志。9 月 30 日及以前完整原文移至 [历史快照](CURRENT_STATE_HISTORY_THROUGH_20260930.md)，旧记录中的“当前”“下一项”只代表当时状态。

## 当前判断

核心功能和六个工作区已实现，当前属于功能较完整、尚未完成发布验收的开发预览版。不能用测试数量、Round3 的“已满足”或旧评估的 95%/98% 推导正式稳定版完成度。详见 [完成度评估](../FEATURE_COMPLETION_ASSESSMENT.md)。

| 维度 | 当前可核查事实 |
| --- | --- |
| 本轮构建 | SDK `8.0.423`，Release `0 warning / 0 error`，XAML `24/24` |
| 本轮测试 | Core `125/125`、Worker `357/357`；Playnite source `465 passed / 2 failed / 18 skipped`，全脚本失败，WPF 类阶段未执行 |
| 远端 CI | 审阅时最新 run `36677346317`（源码 77de7f45）失败于“编译与测试”；渲染和打包被跳过；日志下载 HTTP 403，实际失败方法未知 |
| R00/R01 证据 | 2026-10-04 当前身份检查为 `9 FRESH / 5 STALE`；过期项 R00-04/06/07/08、R01-05；未提供候选包身份 |
| 真实宿主 | 历史存在隔离 EmbeddedPlaynite 样本，但当前候选的 Media Inbox 滚动、主题/DPI/输入等未闭环，`MANUAL QA REQUIRED` |

本轮构建日志、失败摘要、统计与复核结论统一在 [审阅证据](evidence/completion-review-20261005/README.md)。9 月 30 日 SDK9 全量通过是历史结果，不能覆盖本轮失败；本轮 SDK8 也不能替代 SDK10 CI。

## 当前执行顺序

唯一活动队列为 [AUTONOMOUS_BACKLOG.md](../AUTONOMOUS_BACKLOG.md) 的“2026-10-05 收口队列”，不是历史文档散落的下一项。

1. P0 `CLOSE-IPC-01`：定位本轮两条 Named Pipe 行为失败，核对时序、同名管道和失败清理，保留真实取消/未知提交语义。
2. P0 `CLOSE-CI-01` / `CLOSE-SDK-01`：保存 CI 失败诊断，统一明确的 SDK 选择；依据日志定位远端失败，不猜测与本机相同。
3. P1 `CLOSE-EVID-01`：按 5 项 sourcePaths 重跑对应行为/负例并更新 baseline，不能只改哈希使报告转绿。
4. `ENV-001` 条件满足后执行 `CLOSE-HOST-01`：同一隔离 Playnite 进程记录 Media Inbox 滚动前后几何与 DLL 身份。
5. `CLOSE-REG-01` / `CLOSE-REL-01`：用户失败证据、恢复/撤销恢复和当前候选发布矩阵；未满足前不扩充纯视觉任务。

## 架构与生产 UI

- Playnite 插件 `net462`，Core/Contracts `netstandard2.0`，Worker `net8.0-windows10.0.19041.0`，Playnite 测试 `net472`。插件与 Worker 经 Named Pipe 协作，SQLite 由 Worker 持久化。
- 生产外壳为 `AcrylicProductionShellView`；局部共享入口 `AcrylicProductionResources.xaml`，配合 `DesignTokens.xaml`、`WpfUiProduction.xaml`、`Redesign.xaml` 等资源。六工作区：Overview、Save、Trainer、Media、Task、Maintenance；另有 Settings。
- Demo-first 唯一主要视觉基准仍为 `GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design/`；该目录当前工作区不存在。按既有授权沿用已恢复的生产基线，不伪造 Demo 对比结果。WPF 技能只负责质量检查。
- 游戏状态/排序选择沿用已修复的 `DropDownClosed` 提交路径；动态选项、用户选择、真实命令、取消/错误与滚动/虚拟化契约必须保留。页面结构可按当前 AGENTS 授权重构，不沿用旧历史记录中的页面冻结禁令。
- `DashboardViewModel` 主文件约 6480 行，已存在 16 个 partial 文件；这是维护风险，不是未实现功能或已测出的性能瓶颈。

## 验证与交付规则

- 行为矩阵入口 `scripts/e01-behavior-matrix.ps1`；构建 `scripts/build.ps1 -Configuration Release`，可用 `-OutputRoot` 隔离；XAML/source/freshness/RenderHarness 各自只证明其覆盖范围。
- 不把离屏 logical DIP、合成 WPF、进程内 UIA provider 当成 Playnite、物理 DPI、OS 输入或最终呈现帧。
- 每个独立阶段同步文档、记录构建/测试实际结果，单独 commit 并 push 当前远端分支；本次不安装到用户真实 Extensions，不操作真实存档、媒体或云端。
- 后续依次阅读 [PROJECT_MEMORY.md](PROJECT_MEMORY.md)、[WORKLOG.md](WORKLOG.md)、[DEVELOPMENT_HANDOFF.md](../DEVELOPMENT_HANDOFF.md) 与 Git 状态。只在追溯具体问题时读历史大文件。
