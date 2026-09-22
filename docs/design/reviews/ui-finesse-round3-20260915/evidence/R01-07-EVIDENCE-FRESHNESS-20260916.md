# R01-07 基线失效规则证据

## 结论

R01-07 已满足。新增 check-ui-evidence-freshness.ps1 和版本化 UI_EVIDENCE_BASELINE.json，按每条证据自己的源码提交比较 Git 变更，并用 sourcePaths/scopes 标记受影响页面。脚本分别输出当前源码身份和包身份；纯文档变更不要求重跑或重装，包身份不一致时只在包绑定证据上要求重装。

## 2026-09-18 当前分支扫描

- R00-01-02、R00-03、R00-04、R00-05、R00-06、R00-07、R00-08 和 R01-01、R01-02、R01-03 已用当前证据重跑，因此将 baseline 的 `sourceCommit` 重新绑定到各自可复核的当前隔离证据身份：`89aa27e1`、`e5a12ffa`、`e8fe1aab`、`4f585024`、`a5219c09`、`c2399d7b`（按证据条目对应，完整值见 JSON）。没有把未重跑的记录强行标 fresh。
- 以源码 HEAD `c3e67cb4bc13da7865a28dc8fa6067b7035d662b` 扫描 14 条记录：`14` 条 fresh、`0` 条 stale；当前包身份 `not-provided`，没有真实 package-host 安装或重装结论。R01-06 已将当前审计身份写入 baseline，随后只提交文档与 baseline 变更，按规则不需因文档-only 变化重跑。
- 本次没有隐藏 stale 或把 `package=not-provided` 写成宿主安装通过；14 条记录的关联 sourcePaths 均未命中新的非文档变更。

## 当前分支扫描

报告：[历史 freshness-report](R01-07-freshness-report-20260916.json)；[当前 2026-09-18 freshness-report](R01-07-freshness-report-20260918.json)

| 项目 | 结果 |
| --- | --- |
| 扫描时源码身份 | `c3e67cb4bc13da7865a28dc8fa6067b7035d662b` |
| 当前包身份 | not-provided；本轮没有真实包或宿主安装 |
| 扫描记录 | 14 条；14 条 fresh，0 条 stale |
| 需要重跑 | 无 |
| 保持新鲜 | 全部 14 条；R00/R01 当前证据和 R01-06 归档身份均已重新绑定 |

需要重跑只表示证据提交后其关联源码或测试路径发生变化，不表示扫描发现产品缺陷。命中的页面/范围和具体路径保存在 JSON 的 scopes、matchedSourcePaths 中。

## 行为验证

可复跑测试：

~~~powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\test-ui-evidence-freshness.ps1
~~~

三类实际结果：

| 场景 | 结果 |
| --- | --- |
| 纯文档路径变更 | documentationOnly=True；14 条均不需重跑；0 条要求重装 |
| 共享主题 Redesign.xaml 变更 | R00-01-02 和 R00-05 命中 shared-controls/all-pages；无关 R00-06 不被标记 |
| 合成包身份不匹配 | packageStatus=mismatch、reinstallRequired=True；currentSourceCommit 与 currentPackageCommit 保持独立 |

测试使用隔离 JSON fixture 和路径输入，不构建、安装或修改真实包；代码提交 e1324fe 已先通过 scripts/validate-source.py 和三分支 smoke。

## 边界

- 当前基线中的 R00/R01 证据均为 offscreen 或 infrastructure，packageBaselineCommit 为 not-applicable；因此本次扫描不会凭空声明包已验。
- 真实 package-host 记录应在取得真实包身份后填写 packageCommit 和 runtimeKind=package-host；在此之前 package 身份显示 not-provided 是阻断信息，不是通过。
- 关联规则基于版本化路径映射；新增页面或共享资源目录时必须补 sourcePaths/scopes，否则脚本会保守地不扩大影响范围。
- 未执行真实 Playnite 嵌入、用户主题、物理 DPI、OS 输入/IME、presented frame、ETW 或宿主性能验证；生产命令、绑定、picker、滚动条、取消/错误、恢复保护和有限列表契约未改。

## 下一步

R01-07 当前 baseline 与 freshness 校验已满足；下一可执行小批量为 R01-08“跳过测试说明”，继续区分本机可验证 gated 能力、历史 skip 和未启动真实 Playnite/package-host 的边界。

## 2026-09-19 当前提交修正复核

上一节“2026-09-18 当前分支扫描”中的 `c3e67cb4` 是历史文档扫描身份，不能作为当前续作分支的最新身份。该段保留用于审计历史，但已由本节和新的 [R00/R01 当前复核](R00-R01-CURRENT-RECHECK-20260919.md) supersede。

- 当前源码身份：`3354fd82400df6659165a688b8fcb1eb87116ca4`。
- 当前包身份：`not-provided`；没有真实 package-host 安装或重装结论。
- 新扫描报告：[2026-09-19 freshness-report](R01-07-freshness-report-20260919.json)；14 条记录均为 `FRESH`，0 条 stale。
- 复核同时修正了隔离 RenderHarness/UiAuditRunner 的源码根目录身份解析，以及仍指向旧实现的 R00/R01 源码断言。
- R00-07、R01-01、R01-07 在当前路径规则下本来就是 fresh，因此没有把未重跑条目伪装成同一批构建产物；R00/R01 的实际构建、测试、审计与边界见当前复核证据。

## 2026-09-23 当前分支扫描

- 当前源码身份为 `b5c7a6d423a4bf23004c3b080e133b3b0b065fa5`，包身份仍为 `not-provided`；没有真实 package-host 安装或重装结论。
- 本次将 R00/R01 已实际刷新或复核的记录绑定到当前完整身份，并生成 [2026-09-23 freshness-report](R01-07-freshness-report-20260923.json)。扫描结果为 `14` 条 fresh、`0` 条 stale；文档变更没有被写成需要安装包。
- R00-03 的当前事实不是“探针全通过”：`UiFinesseFoundationTests 9/9` 已通过，但 `motionreentryprobe`/`motionhotprobe` 重跑未稳定通过，失败样本已写入 R00-03/R01-04 证据；freshness 表示证据已绑定当前源码，不表示所有运行时探针通过。
- 审计索引当前 `20/20` 可追溯，但 summary 真实包含 `7` 条 HIGH、`4` 条 MEDIUM；R01-03/R01-06 文档已改为事实口径。当前扫描的 `package=not-provided`、Playnite/UIA/物理呈现/ETW/宿主性能边界均保留。
