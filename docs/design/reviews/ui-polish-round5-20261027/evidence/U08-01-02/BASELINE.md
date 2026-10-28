# U08-01/02 概览状态与存档目标

2026-10-28，基线 `18481e6d8d3415ffc31362764980614e76445848`，两项局部推进，仍进行中。概览优先状态可表示健康、故障或其他情况；原固定眉题“待处理”对健康状态不准确，改为“当前状态”，原状态文本、下一步命令、主题和结构保留，没有恢复其他架构。

存档历史摘要原有版本数、保护/匹配状态和历史范围，现增加当前游戏名称及完整身份提示/UIA 帮助，未选游戏时明示；保持原列表、恢复对象、校验和命令。跨状态/窄窗的完整阅读路径及真实宿主待验。

SDK 8.0.423 Release 组合 31 通过、1 既有旧“今日工作台”架构断言 skip、0 失败；skip 不计通过。[脱敏 TRX](tests-redacted.trx)，原始 SHA256 `51A8F14C46870C7A0B9A5613FAE89ABBEF5DDEC3B4346FF18C6052E77CC2BF5B`。测试针对基线含未提交修改。首次和第二次夹具失败因未挂载 Tab 未继承上下文，第三次仍失败因未完成 WPF 绑定调度；提供同一上下文并等待 ContextIdle 后保留原断言通过，三次失败 TRX 已脱敏保留。源码检查通过、XAML 26/26、WPF 静态 37 文件/0 错误/26 既有警告/179 info。RenderHarness 0 警告/0 错误，render-qa exit 0、双主题 394 PNG/0 PROBLEM；报告 SHA256 `1B0045F62893B84C97549AE6C708C541C2E00CF8B911C891115480EB8622E579`，报告基线身份且工作树未清洁，不等同真实 Playnite 渲染。

清理：本批 `.tmp/round5-u08-01-02/` 最终/失败 TRX、日志和 53,290,115 字节当前审计渲染保留。未删除其他任务、用户 TRX/TestResults、存档和配置。C/D 可用空间由 3,170,291,712 / 15,072,129,024 字节变为 3,132,211,200 / 15,015,632,896 字节。真实宿主、物理 UIA/DPI、完整恢复流程、新提交双 SDK CI、整轮完整构建安装启动待验；未执行真实恢复。

## 2026-10-29 可见窗口补充回归

测试提交 `957ee036e9c26e6b41af620ec916aaca1f37d673`（`补充概览与存档窗口回归`）将原本脱离宿主的绑定夹具改为真实显示的 WPF `Window`，覆盖项目定义的标准最小窗口 `1040×700 DIP`。本提交只改测试，不改生产 XAML、共享样式、业务命令或恢复流程。

- 概览：按 1040 DIP 断点使用堆叠布局，确认“当前状态”、优先状态和下一步按钮均有可见排列区域；命令对象、UIA Name、Tooltip及纵向自动/横向禁用的页面滚动通道保持连接。
- 存档历史：所选游戏名称和完整身份同时出现在 Tooltip/UIA HelpText；切换为未选择后名称、Tooltip和HelpText均更新为“未选择游戏”。摘要有可见高度，历史表格保持至少236 DIP最小高度，页面纵向滚动自动、横向禁用；既有重试命令仍指向 `LoadDetailsCommand`。未执行备份或恢复。
- SDK `8.0.423`、Release、目标 `net472`：精确提交定向测试2/2、0失败、0 skip、退出码0。TRX脱敏副本 SHA-256：`BFEE915A1C42F11B0387FB8A83CC129E214C807A9F6CDA831CB82583E173BF7F`，见[精确提交TRX](tests/exact-957ee036-focused.redacted.trx)。
- 初轮编译错误为 `net472` 不含 `IsExternalInit`，已将测试记录类型改为普通类；随后首个窗口回归因视觉树存在多个同名元素而触发 `Single()`，已按 `ButtonBase` 限定查询，最终精确提交复跑通过。TRX在xUnit报告完成后仍记录了 WPF `TextServicesHost.OnUnregisterTextStore` 的STA收尾 `InvalidComObjectException`；早期隔离WPF回归也存在同类收尾诊断，当前测试退出码与TRX计数均为通过，未将诊断计作跳过或失败。
- `python scripts/validate-source.py`退出0；WPF静态审查扫描42个XAML、0 error、26既有warning、179 info。此提交没有生产视图或资源差异，未重复Render QA；前述394 PNG/0 PROBLEM报告仅作为之前生产视图候选证据，报告含dirty工作树身份，不冒充本提交精确截图或真实Playnite渲染。
- 精确提交已推送。GitHub连接查询该SHA的combined status为空，工作流run接口只返回PR触发而未给出本次push run；公开Actions页面为旧缓存。没有本提交可核的run/job/artifact身份，当前按“远端待验”处理，详情见[CI查询快照](ci-progress-957ee.json)。`windows-build.yml`中的基线和SDK10两个job均待用可列push run的连接继续核对。

清理完成：路径规范化并确认目标位于仓库 `.tmp/`、无重解析点、没有本阶段活动构建进程或文件锁、无指向该目录的Markdown链接后，使用 PowerShell `Remove-Item -LiteralPath -Recurse -Force` 删除本批自建 `.tmp/round5-u08-01-02-next` 的3个日志/TRX文件，逻辑字节13,032。D盘可用空间 `12,463,370,240 → 12,463,390,720` 字节，C盘保持 `2,406,744,064` 字节。路径、逐文件清单和检查结果见[清理记录](cleanup.json)。旧Round3/Round5结果、用户TestResults、配置和其他阶段审计均保留。
