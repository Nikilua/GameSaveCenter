# GameSaveCenter 当前事实入口

更新：2026-10-07；本阶段修复共享输入测量/折行，并定位修复共享按钮对齐；当前在途门禁与身份见[共享UI证据](evidence/close-wrap-20261007/README.md)，分支 `main`，版本 `0.6.73-development-preview`。

本页只保留当前结论与下一步，不再累加阶段日志。9 月 30 日及以前完整原文移至 [历史快照](CURRENT_STATE_HISTORY_THROUGH_20260930.md)，旧记录中的“当前”“下一项”只代表当时状态。

## 当前判断

核心功能和六个工作区已实现，当前属于功能较完整、尚未完成发布验收的开发预览版。不能用测试数量、Round3 的“已满足”或旧评估的 95%/98% 推导正式稳定版完成度。详见 [完成度评估](../FEATURE_COMPLETION_ASSESSMENT.md)。

| 维度 | 当前可核查事实 |
| --- | --- |
| 本轮构建 | SDK `8.0.423`，Release `0 warning / 0 error`，XAML `24/24` |
| 本轮测试 | 联合专项64/64，焦点4/4；当前Core125/Worker357/source470 passed/18原有skip；完整render/source/XAML通过。第三轮WPF前36类含Q14/R02/R05焦点通过，第37类删除回退2 passed/1 failed、后77类未执行，完整门禁未通过 |
| 远端 CI | 前阶段两条SDK10侧栏22/22通过，Q14失败且产物已下载；91bfb6e1/run37569191845在自测成功日志后的外层退出码失败，后续未执行/artifact0；c35/run37570629705两项自测success，SDK8.0.423及失败artifact已实际验证，仍停修复前Q14，联合候选新CI待验 |
| SDK契约 | global.json/CI精确8.0.423、rollForward=disable、C#12.0；PS5.1/7真实缺失旧SDK与latestMajor对照通过；当前runner确选8.0.423且预装9/10未漂移；完整门禁/包待验 |
| R00/R01 证据 | 联合候选实际影响4 fresh/10 stale：原8项加R00-01-02/R00-05；baseline未改。本地包仍为历史d57a613f，未安装 |
| 真实宿主 | 历史存在隔离 EmbeddedPlaynite 样本，但当前候选的 Media Inbox 滚动、主题/DPI/输入等未闭环，`MANUAL QA REQUIRED` |

SDK与远端结果在[本阶段证据](evidence/close-sdk-20261007/README.md)；[CI诊断阶段](evidence/close-ci-20261007/README.md)、[IPC证据](evidence/close-ipc-20261005/README.md)与[原审阅](evidence/completion-review-20261005/README.md)保留历史。SDK10的真实ClientAreaAnimation=false路径已验证，侧栏通过；Q14现已通过定向及首轮全量入口：双主题654 DIP单行margin恢复0，错误态几何稳定，60组测量及往返通过；初轮完整render通过。随后发现R02内容拉伸导致图标间距9，独立旧DLL复现；共享按钮修复后原4项及新增内容切换3项通过，最终联合render和静态门禁通过，第二轮全量第35类R05失败；日志证明隐藏ComboBox/Focus失败的夹具前提，修正并补紧凑负例4/4。第三轮source470/18、WPF前36类通过，第37类删除回退实际Game 0020而期望Game 0000，后77类未执行；旧clean与同candidate独立均3/3，CLOSE-PICKER-01需要受控归因，不能抹除失败。整体门禁尚未通过。

## 当前执行顺序

唯一活动队列为 [AUTONOMOUS_BACKLOG.md](../AUTONOMOUS_BACKLOG.md) 的“2026-10-05 收口队列”，不是历史文档散落的下一项。

1. P0 `CLOSE-CI-01` / `CLOSE-SDK-01`：工程基线与失败上传已接入；按当前源码runner实际结果签收，完整门禁通过前保持IN_PROGRESS。
2. P1 `CLOSE-WRAP-01`：输入/行距修复已通过专项及Q14全量入口；CLOSE-OPTICAL-01及CLOSE-FOCUS-01专项与全量入口已通过，第三轮新CLOSE-PICKER-01优先定位；完整114类门禁通过才签收。
3. P1 `CLOSE-EVID-01`：上述回归修复后按10项sourcePaths精确补证，不能只改哈希转绿。
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
