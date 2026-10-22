# 首次标准入口失败与契约修正

- 实施提交：`52f74b1ec54bab5c01c61971e18cbba977020468`。入口为根中文 `.cmd`，实际调用 `GameSaveCenter-Run.cmd` 的默认链；run `20261022T090201519Z-9bbd87c3`，SDK `8.0.423` / Release。
- XAML 25/25、solution 0 warning / 0 error；Core 157/157、Worker 400/400。Playnite source 567 项中 548 pass、18 原有 skip、1 fail；合计 1,124 项、1,105 pass、18 skip、1 fail、0 error/timeout/abort，尚未进入 140 个 WPF 隔离类、打包、安装或启动。
- 唯一失败是 `UiAuditSourceTests.MediaInboxKeepsFiniteGridAndCompactInspectorContracts` 仍断言旧 `useInboxPageFallbackScroll` 字符串与主布局 MinHeight。新分栏已由媒体 6/6 和 RenderHarness 验证；该源码断言现在检查页面与详情框是兄弟、页面确实包含表格、详情不在页面滚动内、窄窗重挂接与有限表格/四行框架预算。没有删除测试、增加 skip 或取消有限视口检查。[定向复验](tests/audit-source-final.trx) 6/6。
- 根命令与中文入口均传播实际退出码 `1`；本次失败不记录成一键成功。源测试输出还带有 TextServicesContext 关闭时的 COM 诊断文本，后续完整复跑继续核对退出状态与结果。
- [完整控制台](first-standard-failure/console.txt)、[诊断摘要与源码身份](first-standard-failure/summary.json)、[原始 source TRX](first-standard-failure/playnite-source.trx)、[失败构建程序集 SHA](first-standard-failure/assemblies.json)。原始诊断目录仍保留于 `artifacts/build-diagnostics/20261022T090201519Z-9bbd87c3`。
- 清理了本次独有的失败输出 `artifacts/gsc-b/0e560d94`；根命令和定向测试 exec 均已确认结束。清理命令中的 CIM 引用查询被拒，且非终止错误未阻止后续删除，这是检查缺口，不能把拒绝查询记作零引用；随后使用可用的 Get-Process 核对相关进程，并将实际快照与后续必须在检查失败时停止的规则写入[清理记录](first-standard-failure/cleanup.json)。用户原有产物及此前最新成功包保留。最终标准入口必须从后续精确提交重新完整执行。

归档控制台仅移除行尾空格以通过 diff 检查；原始字节仍保留在 `artifacts/ui-layout-standard-console.log`，[SHA](first-standard-failure/raw-console-sha256.txt) 已保留。失败文本与 TRX 未改写。
