# R01-07 基线失效规则证据

## 结论

R01-07 已满足。新增 check-ui-evidence-freshness.ps1 和版本化 UI_EVIDENCE_BASELINE.json，按每条证据自己的源码提交比较 Git 变更，并用 sourcePaths/scopes 标记受影响页面。脚本分别输出当前源码身份和包身份；纯文档变更不要求重跑或重装，包身份不一致时只在包绑定证据上要求重装。

## 2026-09-18 当前分支扫描

- R00-01-02、R00-03、R00-04、R00-05、R00-06、R00-07、R00-08 和 R01-01、R01-02、R01-03 已用当前证据重跑，因此将 baseline 的 `sourceCommit` 重新绑定到各自可复核的当前隔离证据身份：`89aa27e1`、`e5a12ffa`、`e8fe1aab`、`4f585024`、`a5219c09`、`c2399d7b`（按证据条目对应，完整值见 JSON）。没有把未重跑的记录强行标 fresh。
- 以源码 HEAD `989abec46173475a32ee9934c8f3093d63fd685f` 扫描 14 条记录：`12` 条 fresh、`2` 条 stale；当前包身份 `not-provided`，没有真实 package-host 安装或重装结论。随后只提交了文档与 baseline 变更，按规则不需因文档-only 变化重跑。
- 仍需重跑的是 R01-05（命中 `AdaptiveThemePalette.cs`、`AcrylicProductionShellView.xaml.cs`）与 R01-06（命中 `RenderHarness/Program.cs`）。这表示其历史证据与当前源码路径之间存在变更，不表示工具已经发现产品缺陷；下一小批量先处理 R01-05，再处理 R01-06。

## 当前分支扫描

报告：[历史 freshness-report](R01-07-freshness-report-20260916.json)；[当前 2026-09-18 freshness-report](R01-07-freshness-report-20260918.json)

| 项目 | 结果 |
| --- | --- |
| 扫描时源码身份 | `989abec46173475a32ee9934c8f3093d63fd685f` |
| 当前包身份 | not-provided；本轮没有真实包或宿主安装 |
| 扫描记录 | 14 条；12 条 fresh，2 条 stale |
| 需要重跑 | R01-05、R01-06 |
| 保持新鲜 | 其余 12 条，包括本批已重跑的 R00/R01 证据 |

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

R01-07 已满足；下一可执行小批量为 R01-05“负例注册表”当前证据重跑，随后处理 R01-06 宿主证据保全。R01-08 仍保留既有 skip 盘点，不把当前包身份缺失写成已安装宿主验证。
