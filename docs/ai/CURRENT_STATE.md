# GameSaveCenter 当前事实入口

更新：2026-10-12；本地已收口 picker Dispatcher 刷新与 R09 错误描边测试校正，完整 Release/WPF 隔离门禁通过。阶段证据：[CLOSE-PICKER-01](evidence/close-picker-20261012/README.md)。当前分支 `main`，版本 `0.6.73-development-preview`。

本页只保留当前结论与下一步，不再累加阶段日志。9 月 30 日及以前完整原文移至 [历史快照](CURRENT_STATE_HISTORY_THROUGH_20260930.md)，旧记录中的“当前”“下一项”只代表当时状态。

## 当前判断

核心功能和六个工作区已实现，当前属于功能较完整、尚未完成发布验收的开发预览版。不能用测试数量、Round3 的“已满足”或旧评估的 95%/98% 推导正式稳定版完成度。详见 [完成度评估](../FEATURE_COMPLETION_ASSESSMENT.md)。

| 维度 | 当前可核查事实 |
| --- | --- |
| 本轮构建 | 本机 SDK `8.0.423`；Release `0 warning / 0 error`；XAML `24/24`。SDK10.0.401 属于远端 Actions，单独核验 |
| 本轮测试 | Core `125/125`；Worker `356 passed / 1 skipped`；Playnite source `461 passed / 27 skipped`；WPF `114 classes, 608 passed / 39 skipped`；合计 `1,550 passed / 67 skipped / 0 failed`，完整脚本 exit `0` |
| 当前实现阶段 | `CLOSE-PICKER-01` 本机代码和自动化门禁已实现；新增 STA Dispatcher 线程负例证明旧实现线程错误，删除回退/虚拟化全量通过。R09 测试已校正为检查独立 `ValidationStroke` 两 DIP 覆盖层，不错误断言 Chrome 改为两 DIP |
| 远端 CI | 旧检查点 run `36677346317` 本次无法读取，状态未知；run `36675309812` 仍失败且完整日志 API 返回403。当前修改提交推送后要检查新 Actions run 直至终态；CI SDK10.0.401 不以本机 SDK8结果替代 |
| R00/R01 证据 | e121 基线 freshness 上次为 `4 fresh / 10 stale`；当前阶段源码尚未提交后的 freshness 尚未重算，baseline 未更新。CLOSE-EVID-01 下一步按受影响路径逐项补行为/负例证据 |
| 真实宿主 | 未启动真实 Playnite；Media Inbox 同进程滚动、物理 DPI/跨屏、IME、Narrator/UIA、ETW 与最终呈现帧均未验，`MANUAL QA REQUIRED` |

历史 SDK/CI/IPC 诊断见[SDK阶段证据](evidence/close-sdk-20261007/README.md)、[CI证据](evidence/close-ci-20261007/README.md)和[IPC证据](evidence/close-ipc-20261005/README.md)。本地完整运行现已通过，R05 新确定性负例与修复正例已记录；旧的第三轮失败保留为归因证据，不能用旧clean DLL独立通过覆盖。完整候选仍需远端 Actions 终态和后续 R00/R01 freshness 校正。

## 当前执行顺序

唯一活动队列为 [AUTONOMOUS_BACKLOG.md](../AUTONOMOUS_BACKLOG.md) 的“2026-10-05 收口队列”，不是历史文档散落的下一项。

1. 推送本阶段 main 提交并核对对应 Actions；状态未知/运行中时不宣称阶段完成，SDK10.0.401 结果单列。
2. P0 `CLOSE-CI-01` / `CLOSE-SDK-01`：取得可用的当前 Actions 终态与完整日志；旧 run 403/页面不可读时记录限制，不盲修。
3. P1 `CLOSE-EVID-01`：在修复提交后重算 freshness，针对 R00/R01 的10条 stale sourcePaths 补行为、负例与身份，不能只改哈希转绿。
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
