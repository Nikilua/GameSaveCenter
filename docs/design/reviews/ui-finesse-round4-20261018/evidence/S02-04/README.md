# S02-04 上下文跳转

实现提交：`1b17e4a0e14f7de2f9ba5fe51595d51a26099e56`。通知详情提供“打开关联游戏”和“打开关联任务”入口。游戏按 Playnite 稳定 ID 定位，任务查询由 SQLite 使用精确 `task_id` 条件筛选，不按名称、消息文本或相似 ID 猜测。关联对象已删除或当前库中不可见时保留通知和摘要并说明原因；任务入口进入现有任务详情/来源导航，可继续查看已有版本、媒体或问题信息。返回动作回到通知收件箱。已读仍只代表查看。

## 提交后验证

- 使用 SDK `9.0.302` 对提交 `1b17e4a0` 做 Release 全解方案构建：`0 warning / 0 error`。为绕过该 SDK 安装中缺少的 workload resolver locator，从 `C:\Windows\Temp` 运行，使用 `--no-restore -m:1 -nodeReuse:false -p:NuGetAudit=false -p:MSBuildEnableWorkloadResolver=false`。构建日志见 [build.log](build.log)。这不是仓库 `global.json` 指定的 SDK `8.0.423` 门禁。
- 通知视图与 R10 上下文解析 `8/8`，Worker 精确任务查询 `10/10`，R09 焦点描边 `2/2`；均为提交后构建的当前提交程序集，TRX 分别见 [通知/R10](tests/notification-context.trx)、[精确任务 ID](tests/exact-task-id-query.trx) 和 [R09](tests/R09-focus-outline.trx)。
- 提交后 Offscreen Render Harness 报告 `render-qa OK`，报告无 `PROBLEM`；通知列表在浅色/深色各 11 个窗口尺寸都显示 `3/3` 行。代表截图：[浅色](render/Notifications-light-1366x768.png)、[深色](render/Notifications-dark-1366x768.png)。详情栏可滚动；按钮可见，说明文本可在详情区域滚动查看。Harness 只证明离屏布局，不证明物理 DPI 或真实 Playnite 窗口。
- 解决方案、Playnite 插件、测试程序集、Worker 与 Render Harness 的 SHA-256，以及 TRX/截图/源码 blob 身份见 [verification-identity.json](verification-identity.json)。源码提交身份是 `1b17e4a0`。

## 尚未完成的门禁

- 2026-10-21 已使用 SDK `8.0.423` 通过官方 `scripts/build.ps1 -SkipTests` 完成 restore、25 个 XAML 结构检查与完整 Release solution build，`0 warnings / 0 errors`；S02-04 定向测试 20/20 通过。完整 WPF 独立循环、一键安装和真实宿主呈现仍未完成，因此状态仍为“代码完成待验收”。复核证据：[S02-04 至 S02-08 SDK8 当前源码复核](../SDK8-S02-RECHECK-20261021/README.md)。
- 当前 SDK8 的 R05 `QuickMousePassesCancelTooltipAndAStationaryDwellOpensIt` 依设计跳过：此 Windows 会话不允许读取系统鼠标位置；同类另外 2 项通过。未绕过该权限，也未把测试 skip 当作真实桌面验收。这与用户旧 R09 `Thickness(2)`/`Thickness(1)` 断言失败是不同运行、不同失败。
- 用户确认暂时没有诊断 `20261018T012631286Z-e027df9d` 的 `summary.json` 和 TRX。该日志缺少同次源码、测试程序集和 PDB 身份；本次 R09 `2/2` 通过不能解释旧运行实际输入。没有修改共享焦点模板或放宽 R09 断言。

## 2026-10-27 SDK8 标准全链复核（复用 T00-01）

- 实现提交 `1b17e4a0e14f7de2f9ba5fe51595d51a26099e56` 是本次完整标准链源码 `90bf040567f013f3a8d9ed8667028a4edb2ec524` 的祖先。仓库根 `GameSaveCenter-一键构建安装运行.cmd` 以 global.json 锁定的 SDK `8.0.423` 完成全链，外层退出码 `0`；build/install/package run 分别为 `20261027T010319835Z-7b052459`、`20261027T010317740Z-96e7b91b`、`20261027T012406316Z-c7e3272f`。
- 复用该 T00-01 全链中本来就执行的原始 TRX：R10 上下文行为 `6/6`、通知收件箱 `2/2`、Worker 精确任务查询 `10/10`、R09 焦点描边 `2/2`。全链合计 `1968` 项、`1910` 通过、`58` 跳过、`0` 失败；与本项相关的测试均通过。原始 TRX、运行身份及 SHA-256 见 [复核清单](sdk8-standard-90bf0405.json) 和 [T00-01 完整链报告](../../../ui-history-table-4k-20261026/evidence/standard-90bf0405/README.md)。没有重复实现上下文跳转，也没有重复运行构建。
- 同一全链验证了安装程序集身份、Playnite 启动及插件从安装目录实际加载。它不覆盖真实通知详情页的游戏/任务导航、返回和失效对象交互；因此 `state` 仍为“代码完成待验收”，真实 Playnite 页面验证仍 pending。

## 文件索引

- [Release build 日志](build.log)
- [通知与任务上下文 TRX](tests/notification-context.trx)
- [精确任务 ID 查询 TRX](tests/exact-task-id-query.trx)
- [R09 TRX](tests/R09-focus-outline.trx)
- [Render QA 报告](render/render-qa-report.txt)
- [构建、源码、程序集与证据哈希](verification-identity.json)
