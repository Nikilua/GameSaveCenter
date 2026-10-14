# R06-07/R06-08、Q24-06 与 R09 当前 main 行为复核

日期：2026-10-15。源码提交：`cd27c8c0aba8616331867a58af13376f44e958f0`。本批复核已有实现，没有修改生产代码、XAML、命令、DTO 或滚动系统。

## 构建与测试身份

- 本机 SDK `9.0.302`、VSTest `17.14.1`；Playnite 插件 `net462`、测试 `net472`。
- 从仓库外启动 SDK 9，使用独立 `GscBuildOutputRoot`、精确 `GscBuildCommit=cd27c8c0aba8616331867a58af13376f44e958f0` 和 `GscSourceRoot`。Playnite 测试项目及引用 Release build：`0 warning / 0 error`。这是定向项目构建，不是完整 solution build。
- 将当前提交内的隔离测试程序集逐类运行，并将 TRX 中的仓库绝对路径、用户名和主机名脱敏。共 `57 passed / 0 failed / 0 skipped`。

| 任务/套件 | 结果 | 当前行为覆盖 |
| --- | ---: | --- |
| R06-07：`R06EmptyStateBehaviorTests` | 2/2 | 生产 Save 表保留历史 7 列和候选 4 列；Loading、首次无数据、完成与失败状态区分；失败不伪装成空数据；检查状态驱动重试接线。 |
| R06-07：`WorkspaceStatePresenterBehaviorTests` | 8/8 | 重试命令、覆盖层输入阻断、Enter/Space/按钮点击、禁用负例。 |
| R06-07：`MediaWorkspaceStateCacheTests` | 7/7 | 上下文隔离、旧缓存、取消与失败状态、时间文案和 Tooltip 边界。 |
| R06-08：`R06DetailsBudgetBehaviorTests` | 2/2 | 选择项切换后详情对象/错误码同步，旧项不残留；长诊断在独立滚动详情中展开，行高不超过 60 DIP、至少四行完整可见；Save 选择恢复按 `PlayniteId + Path` 优先。 |
| R06-08：`MediaInboxGeometryTests` | 3/3 | 四行不足时页面滚动回退可达，核对表头、行、滚动条与窄窗预算。 |
| R06-08：`TaskCenterViewResponsiveTests` + `DetailsDisclosureSourceTests` | 9/9 | 当前视口、详情开关、筛选/失败状态、表格预算和详情对象关闭行为。 |
| Q24-06：`R21AutomationValueBehaviorTests` | 24/24 | Task/Media/Save/Maintenance 生产 DataGrid peer Name/HelpText 及 Grid/Table/Selection pattern；合成媒体选择读回；任务关闭详情按钮名称；进度未知/越界、LiveSetting、焦点保持。 |
| 外机 R09 交叉复核：`R09FocusOutlineBehaviorTests` | 2/2 | 当前共享 Primary Button 焦点覆盖层和错误 TextBox 的 Chrome/独立 ValidationStroke 行为。 |

TRX 位于本目录 `tests/`，每份均经脱敏。关键源码及二进制身份：

| 脱敏 TRX | SHA-256 |
| --- | --- |
| `R06EmptyStateBehaviorTests.trx` | `47E2A8A8BD16C4BB7BF561211CCFA6791163534F02C746A02B201372E73EC7FF` |
| `WorkspaceStatePresenterBehaviorTests.trx` | `1CAF0C87EB629941457C33C1C0DFBF46CDCD219EA18ECAD40B906E698EA0794A` |
| `MediaWorkspaceStateCacheTests.trx` | `0A51FF24DA7ADD706E9EDCE24D621EDC58FA576FB15423982D1484DDC37BDC8D` |
| `R06DetailsBudgetBehaviorTests.trx` | `D0E161A2948E1A6146BF37D86C9DD06CC0CDCA9FC23D885CF48A87334042B913` |
| `MediaInboxGeometryTests.trx` | `A6D91A76896398F446AFE052F5ECD5E327152E26AA3DBEBEFA407248B637C4B8` |
| `TaskCenterViewResponsiveTests.trx` | `C354815AF851BA56BE05861E144D86CDBC442C41EDCF3DCAADD9C276A44FDF17` |
| `DetailsDisclosureSourceTests.trx` | `177D5C6A7182DEBCDA7A847620BE630D6B65789AB0E559507066E884078AD38D` |
| `R21AutomationValueBehaviorTests.trx` | `E9C3D3E19B25BAEB72A76C430CCF2BBBC459E5FC80F1C1CBAFD8F0428A33A059` |
| `R09FocusOutlineBehaviorTests.trx` | `DE3AFDE8A1A92AFB7665E00C13400C738E2F068A7E21B214DCA473AC8D6DDBC3` |

| 产物 | SHA-256 / 身份 |
| --- | --- |
| `SaveCenterView.xaml` | `4A86836BB5FB4CDA36DD2E4085F43A345EB9F2ABDBFDD1C7C96CE5CA8DC718F9` |
| `TaskCenterView.xaml` | `00EBAC27AB45A858BBF9107DE60C64C28DA8F95C70CB3FB8425A6ACE5D65C0E2` |
| `R06EmptyStateBehaviorTests.cs` | `5BE2033173409EFFAFE9C2E7264A6BC66E486CD23E87CC295C57A0059170EE04` |
| `R06DetailsBudgetBehaviorTests.cs` | `0F43B3C14E9941A24B4A9212F2FF656E7A9308FC3B98333F29525A2D7B8C16E8` |
| `R21AutomationValueBehaviorTests.cs` | `B2CEDE58630CDB9AAAE198388D754B70A80DFB90A02C8A01D8228414328462E5` |
| `R09FocusOutlineBehaviorTests.cs` | `A5AAB9E90C340F93301D8424FF1E30D98B160D39C1265172AEE7A6B01B2CBC98` |
| Playnite plugin DLL | SHA `F1D9DB7DF8B9CE8DCD7341522CA4803BA8E9A6F74B8376484B7AA88CF6A4FEFF`; ProductVersion `0.6.73+cd27c8c0aba8616331867a58af13376f44e958f0` |
| Playnite test DLL | SHA `30D268A626863AB69138B09D68E72ACE19117184438104C52687768BC6E0A1B9`; ProductVersion `0.6.73+cd27c8c0aba8616331867a58af13376f44e958f0` |
| Plugin PDB / test PDB | `9358690450C0768BF6A822AFD9C34CE0DE278707B9403E4DB200A710FECE7CB0` / `886FC7ED59806CA0A8C74004044E9414D841FC320A1FDF327FF220715DBD7579` |

## 结论与边界

- R06-07 与 R06-08 的当前 main 自动行为条件已复核通过。此前证据绑定的提交早于 Save/Task 视图后续变化；本次按 `cd27c8c0` 重建测试项目，并在当前程序集上复跑相关类别。
- Q24-06 的受控 WPF peer/pattern 条件仍通过；这是进程内 UI AutomationPeer，不是 Narrator/Inspect/Accessibility Insights 对真实 Playnite 的验收。
- 外机 R09 日志仍为 Expected `2,2,2,2` / Actual `1,1,1,1`、stack line 151；用户称该机已从 Git 拉取最新，但没有给出故障机实际 HEAD、工作树、测试源码、DLL/PDB 身份。当前 main 的同名测试 line 151 核对错误色、line 152 核对 Chrome `1 DIP`、line 154 才核对独立 `ValidationStroke` `2 DIP`；因此外机报告行号与当前源码行号不同，且 `2` 对 `1` 的值形态对应厚度断言。历史提交 `3f7d0b30` 曾要求 Chrome 为 `2 DIP`，后由 `f13b9e05` 改为 Chrome `1 DIP` + 独立验证描边 `2 DIP`。当前证据无法证明外机实际执行的是哪版源码/程序集，也不能排除跨机运行差异；旧源/符号错配仅是可能解释。未改生产模板或降低断言。`libpng iCCP` warning 不在断言栈中；COM 清理异常发生在断言失败之后。

## GitHub Actions 当前终态（2026-10-15）

- 代码提交 `e221feb35a4f7ca796dc348c2c5cde247a5b58ed` 的 #1006/run `38624032938` 已 Failure：SDK8.0.423 job `116058522011` 失败于“编译与测试”；SDK10.0.401 job `116058522588` 失败于“隔离工作树并运行 Release 编译与测试”。
- 文档提交 `cd27c8c0aba8616331867a58af13376f44e958f0` 的 #1007/run `38625232841` 也已 Failure：SDK8.0.423 job `116062580362` 失败于“编译与测试”；SDK10.0.401 job `116062580995` 失败于“隔离工作树并运行 Release 编译与测试”。
- GitHub 公共 run 页面列出四个诊断 artifact，但匿名 REST API 返回 HTTP 403 rate limit，step-log URL 返回 404，artifact 下载路由没有给出 ZIP（返回 HTML/404）。当前只有失败步骤摘要，没有完整日志或包内容；CI 根因未知，不凭本机 SDK9 结果猜修。
- artifact 元数据：#1006 SDK8 `11923653496`，580 KB，SHA-256 `7b661814a691c227be5a75e341c7f7403d48fbb2e36dbe4f1c18ed8884bbd490`；SDK10 `11923262904`，506 KB，SHA-256 `83e8c3254bee080a2512209d28980d9bca02402fcd109c6c46a95d696155d974`。#1007 SDK8 `11923653920`，577 KB，SHA-256 `14b8ca190f64001d33ddae4f1d7a3524cfdceaf65771aab748d566ad5a7dbe73`；SDK10 `11924133752`，505 KB，SHA-256 `1afa24887514cab7dc5c6e1d3e7c40d70bfb9c2e7fcd1fca31dc7ac886bb90ec`。
- 未运行真实 Playnite；没有截图、RenderHarness 或 presented-frame 证据。真实 Narrator/外部 UIA、OS 输入/IME、物理 DPI/跨屏、ETW 与宿主性能仍待验。没有访问用户存档、媒体或云端。原始 Demo 目录不可用，沿用恢复生产基线。

本机 SDK 9 结果不替代 CI SDK `8.0.423` 或 `10.0.401`。#1006 `38624032938` 与 #1007 `38625232841` 已分别在两条 SDK lane 失败；完整日志和诊断 ZIP 内容尚不可读，根因未确认。可用的 artifact ID、SHA 与下载限制见上节。下一项按用户指定先复核 R00/R01 freshness/问题记录，再推进已满足依赖的 Q/R；不得把本机 SDK9 结果记作 CI 通过。
