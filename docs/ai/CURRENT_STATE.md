# GameSaveCenter 当前事实入口

更新：2026-10-07；本阶段验证基线`0e92f82b`，CI诊断/侧栏策略输入的dirty源码与DLL身份见[本阶段证据](evidence/close-ci-20261007/README.md)，分支 `main`，版本 `0.6.73-development-preview`。IPC源码d57a613f已交付。

本页只保留当前结论与下一步，不再累加阶段日志。9 月 30 日及以前完整原文移至 [历史快照](CURRENT_STATE_HISTORY_THROUGH_20260930.md)，旧记录中的“当前”“下一项”只代表当时状态。

## 当前判断

核心功能和六个工作区已实现，当前属于功能较完整、尚未完成发布验收的开发预览版。不能用测试数量、Round3 的“已满足”或旧评估的 95%/98% 推导正式稳定版完成度。详见 [完成度评估](../FEATURE_COMPLETION_ASSESSMENT.md)。

| 维度 | 当前可核查事实 |
| --- | --- |
| 本轮构建 | SDK `8.0.423`，Release `0 warning / 0 error`，XAML `24/24` |
| 本轮测试 | Core125/125、Worker357/357；source470 passed/18原有skipped/0 failed；侧栏22/22；WPF前23类通过，第24类Q14错误态高度36→37 DIP失败，后89类未执行；IPC历史200/200单独保留 |
| 远端 CI | 基线 run37336829173（0e92f82b / SDK10.0.401）仍两条活动动画断言失败；本阶段新增实例内偏好测试输入与失败诊断上传，提交后远端结果另记，不宣称恢复 |
| R00/R01 证据 | 基线9 fresh/5 stale；本阶段另命中R00-03、R01-01、R01-04，待提交后按实际HEAD复算。baseline未改；本地包仍为历史d57a613f，未安装 |
| 真实宿主 | 历史存在隔离 EmbeddedPlaynite 样本，但当前候选的 Media Inbox 滚动、主题/DPI/输入等未闭环，`MANUAL QA REQUIRED` |

当前CI诊断、PowerShell5.1/7自测、侧栏22项及Q14失败在[10月7日证据](evidence/close-ci-20261007/README.md)；[IPC证据](evidence/close-ipc-20261005/README.md)与[原完成度审阅](evidence/completion-review-20261005/README.md)保持历史。整体门禁仍失败；SDK8不能替代SDK10 CI。Q14本次先失败于错误态高度，原654 DIP边距失败仍未解决。

## 当前执行顺序

唯一活动队列为 [AUTONOMOUS_BACKLOG.md](../AUTONOMOUS_BACKLOG.md) 的“2026-10-05 收口队列”，不是历史文档散落的下一项。

1. P0 `CLOSE-CI-01` / `CLOSE-SDK-01`：依据已取得的侧栏动画断言核对系统偏好/测试前提，保存失败产物并统一明确SDK。
2. P1 `CLOSE-WRAP-01`：修复已两次复现的Q14/654 DIP单行20 DIP边距残留，保留折行和往返负例。
3. P1 `CLOSE-EVID-01`：上述回归修复后按5项sourcePaths精确补证，不能只改哈希转绿。
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
