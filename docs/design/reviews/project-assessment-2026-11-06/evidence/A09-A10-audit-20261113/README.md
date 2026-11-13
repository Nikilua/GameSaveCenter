# A09/A10 去重审查（2026-11-13）

## 范围与源码身份

- 审查源码：`b667e22445fbf57aa591f39f3d968f5c166c7cb6`。后续仅有文档提交，产品源码未变化。
- 本批是入口、覆盖和缺口审查，没有修改源码、测试、资源或脚本，没有启动Playnite，也没有执行真实恢复、删除或云端写入。
- 抽查报告的16项仍是计划候选，A09/A10不因本记录成为已确认产品缺陷。

## A09：首次使用到隔离恢复闭环

### 当前入口

1. 维护中心 onboarding 卡片通过 `RunEnvironmentCheckCommand` 运行真实 `CheckEnvironment`，可跳过或在环境检查通过后完成首次设置。
2. `OnboardingTestBackupCommand` 直接复用生产 `BackupSelectedAsync` → `BackupGame` 管线；没有独立的模拟成功服务。
3. 游戏匹配由现有 catalog upsert/matcher 管线完成；存档页继续提供 `ValidateGame`、`ValidateRestoreReadiness` 与恢复工作流，保留确认、PreRestore、运行中拒绝、失败回滚和人工介入语义。

### 当前证据

精确SHA完整入口TRX中的相关类：

| 测试类 | 结果 |
| --- | --- |
| `EnvironmentCheckServiceTests` | 1/1通过 |
| `GameMatcherTests` | 2/2通过 |
| `SettingsAndAutoSelectSourceTests` | 4通过、2个既有`NotExecuted`；其中onboarding真实备份管线用例通过，旧生产UI基线项不计通过 |
| `BackupPreviewBehaviorTests` | Worker 2/2通过 |
| `R11BackupPreviewBehaviorTests` | WPF 4/4通过 |
| `RestoreReadinessTests` | 15/15通过 |
| `RestoreOrchestratorTests` | 16/16通过 |
| `R12RestoreWorkflowBehaviorTests` | WPF 8/8通过 |

### 缺口与收益

- 缺口：现有覆盖是分段服务/行为测试，没有一个一次性Playnite配置把“首次检查→匹配→备份→校验→恢复预览→隔离恢复”串成同一用户会话。onboarding卡片本身也不承担后四步，因此不能声称首次使用闭环已验。
- 缺口：`b667e224`的一次性profile验证了安装及Playnite/Worker进程启动，但没有导入虚拟游戏库，也没有完成修复后的真实可见宿主交互。
- 收益：在不接触真实profile和存档的前提下，完整链可发现术语、跨页导航、命令前置条件与重复步骤的实际阻碍，避免重写已有引导。
- 夹具设计结论：恢复侧已有 `IRestoreCatalog` / `IRestoreClient` 测试缝和唯一临时根，能够安全验证隔离恢复；备份侧 `BackupOrchestrator` 仍直接依赖具体 `LudusaviClient`，现有测试没有可替换的备份客户端接口。`DashboardViewModel` 又直接由真实插件会话构造，因此当前没有一条不改生产依赖关系就能串起全部步骤的同会话测试缝。
- 决策：不为拼接一次性测试而重构生产依赖注入，也不使用真实 Ludusavi、真实profile或真实存档。A09保持“分段安全覆盖已核、同会话宿主闭环待条件”，不是已确认缺陷。
- 下一步：仅在后续业务需求本身引入可替换备份客户端，或可用一次性Playnite profile/虚拟游戏且不占用用户桌面时继续；服务级隔离恢复不能替代真实宿主可用性。

## A10：代表规模与有界性

### 当前入口

- Worker `SoakDataScaleHarness` 提供 reduced、full、stress 三档。当前full为2,000游戏、20,000备份、10,000任务、5,000媒体、500工具；stress为10,000游戏、20,000备份、10,000任务、50,000媒体、500工具。
- Playnite侧使用现有游戏选择器、任务索引集合、备份历史分页、选项虚拟化和异步缩略图预算，不新增平行列表实现。

### 当前精确SHA证据

| 测试类 | 结果与范围 |
| --- | --- |
| `SoakDataScaleTests` | 1/1通过；本次为reduced：40游戏、120备份、200任务、600媒体、20工具；查询与模拟后managed/handle/thread增长均0 |
| `LargeLibraryPerformanceTests` | 5/5通过；覆盖2,000游戏选择器集合通知与基准等待边界 |
| `TaskIndexedCollectionTests` | 4/4通过；更新、头插、200行上限与索引重建 |
| `BackupHistoryPagingTests` | 1/1通过；稳定时间+ID游标、插入后不移位、响应不泄露manifest路径 |
| `R18ThumbnailBudgetTests` | WPF 3/3通过；快速窗口、取消、缓存/解码预算及陈旧结果拒绝 |
| `R05OptionVirtualizationBehaviorTests` | WPF 5/5通过；选项虚拟化行为 |

### 缺口与收益

- 当前补验：文档HEAD `2b084c6a2b48fc2234c0a71e3e828fcd916b1715`仅在产品源码`b667e22445fbf57aa591f39f3d968f5c166c7cb6`之后增加文档。该HEAD使用SDK8.0.423 / Release、`GSC_SOAK_DATA_SCALE=1`定向运行full档，exit 0、1/1通过、0失败、0 skip。
- full结果：2,000游戏、20,000备份、10,000任务、5,000媒体、500工具；造数138,568ms，模拟472ms；首次/热游戏查询20/13ms，任务页100/3/4ms，任务搜索5ms，媒体页3/0/0ms；`managedGrowth=0 MiB, handles+0, threads+0`，订阅与临时残留均0。
- 缺口：现有本机真实Playnite库只有少量游戏；自动测试的集合/STA窗口结果不能证明真实宿主在代表规模下的滚动、搜索、取消和缩略图体验。
- 收益：当前源码的SQLite查询、分页、增长与临时输出有界性已有full档证据；宿主矩阵另列，避免把后台规模通过误写成UI帧率通过。
- 下一步：A10后台范围收口，真实宿主代表规模仍保持条件待验；不因本结果生成平行列表实现或重跑整套构建。

## 原始证据身份

| 文件 | SHA-256 |
| --- | --- |
| `artifacts/build-diagnostics/20261113T060254846Z-d6b80c0d/tests/core/core.trx` | `D423A8B52D1643288437B000A44ECDAC0B0A26C6BDE3B84AC2186858E3EE46FA` |
| `artifacts/build-diagnostics/20261113T060254846Z-d6b80c0d/tests/playnite-source/playnite-source.trx` | `8694809B8E35E85B4DB7934254E46EC7D28F44CF04A1670CFE00255874D486EE` |
| `artifacts/build-diagnostics/20261113T060254846Z-d6b80c0d/tests/worker/worker.trx` | `72E22DFA7B4A4D1F5AD80729D1695ACC559FCA8805EBCEF444D157DF7421A274` |
| `artifacts/build-diagnostics/20261113T060254846Z-d6b80c0d/tests/wpf.R11BackupPreviewBehaviorTests/wpf.R11BackupPreviewBehaviorTests.trx` | `69C3B1D0EA27D98E9C99CA0915F5E39E41E9AB0F6009CEB5537F467B67F46622` |
| `artifacts/build-diagnostics/20261113T060254846Z-d6b80c0d/tests/wpf.R12RestoreWorkflowBehaviorTests/wpf.R12RestoreWorkflowBehaviorTests.trx` | `63F6448A0761228D6124E7F7DF3D0B8063DB3B1A1D8A49CAA1A5EBB1832687A4` |
| `artifacts/build-diagnostics/20261113T060254846Z-d6b80c0d/tests/wpf.R18ThumbnailBudgetTests/wpf.R18ThumbnailBudgetTests.trx` | `5E6E09D79C2700EE06E1F3EC169F4E96DBBA964E256080A65DA8C1A47019E91F` |
| `artifacts/build-diagnostics/20261113T060254846Z-d6b80c0d/tests/wpf.R05OptionVirtualizationBehaviorTests/wpf.R05OptionVirtualizationBehaviorTests.trx` | `C90395223A54A71DCB16910E1B25B5FB55B36788395E17EA5CC2648DC60E691D` |

相关测试源在精确SHA的Git blob：`SettingsAndAutoSelectSourceTests` `efca3a64`、`EnvironmentCheckServiceTests` `9d7b3a70`、`GameMatcherTests` `8cf14fcf`、`BackupPreviewBehaviorTests` `9dbd8d97`、`RestoreReadinessTests` `4604efc5`、`RestoreOrchestratorTests` `829eab0a`、`R11BackupPreviewBehaviorTests` `95ddd4f9`、`R12RestoreWorkflowBehaviorTests` `f2eb7473`、`SoakDataScaleTests` `d66076fb`、`LargeLibraryPerformanceTests` `62cb0363`、`R18ThumbnailBudgetTests` `f68f0275`、`BackupHistoryPagingTests` `714776f8`、`TaskIndexedCollectionTests` `17fbda20`、`R05OptionVirtualizationBehaviorTests` `3a0856af`。

full档的结构化结果、TRX/日志hash与清理记录见[`a10-full-result.json`](a10-full-result.json)。原始本地证据保留在`artifacts/a10-full-2b084c6a/`。测试临时目录`.tmp/a10-full-2b084c6a`共17文件、5,827,456 bytes，路径、归属、重解析点、进程和文档引用检查通过；指定`Remove-Item -LiteralPath -Recurse -Force`被执行策略以`blocked by policy`拒绝，目录未删除，未绕过。
