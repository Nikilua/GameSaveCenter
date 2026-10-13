# R06-08 当前 main 详情与行高复核

日期：2026-10-13

任务：`R06-08 | 详情与行高预算`

结论：现有详情区、选中对象同步和四行门禁在当前 main 通过隔离 STA WPF 行为验证；本阶段仅加强测试夹具，没有改生产 XAML、布局或滚动系统。CI 对本次提交尚待触发/查询。

## 基线与构建

- 构建时 main HEAD：`dc8f26ee972c08976ca8cfbe7531e5c68c5a0559`。生产代码来自该提交；唯一已跟踪源差异为本阶段修改的 `tests/GameSaveCenter.Playnite.Tests/R06DetailsBudgetBehaviorTests.cs`。其构建输入 SHA-256 为 `0F43B3C14E9941A24B4A9212F2FF656E7A9308FC3B98333F29525A2D7B8C16E8`。其余未跟踪文件是其他任务证据，没有加入本次构建或提交。
- 按仓库脚本运行 `scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot .tmp/r06-05-main-20261013 -DiagnosticsRoot .tmp/r06-05-main-20261013/r06-08-build-diagnostics`。隔离本机 SDK `8.0.423`、MSBuild `17.11.48`；Release solution `0 warning / 0 error`，XAML `24/24`。随后按套件运行行为测试，目标框架 `net472`、VSTest `17.11.1`。
- 构建诊断记录 `workingTreeClean=false`（本阶段测试源改动及保留的 R04/R05/R07 用户证据）。不能把本次构建描述为 clean checkout。程序集 SHA-256/MVID：Playnite `60049D52AC153585788DC2FEFF4BF181F35F631F71081BE722988E7209144E8E` / `024c9ee7-3540-4d27-8198-e4548c8ae181`；Playnite.Tests `DECF7344C2B02EDE146140B81B12AE8B7A66E5E8194C847D4F4D8A8490D2A2F4` / `fa1d780b-3279-4eea-8b82-d816564bc8c4`。

## 行为结果

| 套件 | 结果 | 实际覆盖 |
|---|---:|---|
| `R06DetailsBudgetBehaviorTests` | 2/2 | Save 候选刷新按 `PlayniteId + Path`（忽略路径大小写）优先恢复原选中对象，不存在才回退 Pending。生产 `TaskCenterView` 在 STA 窗口从首项切换到失败任务，含 160 段长诊断；详情对象随选择更新，错误码正确、旧任务不残留，技术详情展开、独立 ScrollViewer 可滚动、行高不超过 60 DIP。 |
| `MediaInboxGeometryTests` | 3/3 | 生产 Media Inbox 测量表头/行/滚动条与容器边框预算；数据不足四个完整行时必须仍可由页面滚动到达；窄窗页滚动通道保留。 |
| `TaskCenterViewResponsiveTests` | 8/8 | 当前源身份下重跑短窗摘要/表格高度、实测页面 viewport、筛选空与 stale 恢复源绑定、失败详情、紧凑详情开关和表格高度门禁等。 |
| `DetailsDisclosureSourceTests` | 1/1 | 当前源身份下核对紧凑详情切换关闭前一详情对象。 |

合计 `14 passed / 0 failed / 0 skipped`。关键补强：原长详情夹具只有两条任务，`readableRows >= 2` 不能证明四行门禁。本轮保留首项/失败项两条并追加八条合成队列项，在 1120×700 STA WPF 窗口展开 160 段诊断后改为实测 `readableRows >= 4`；现有选择对象、详情滚动和行高断言继续有效。新夹具没有访问真实存档/任务数据。

脱敏 TRX SHA-256：

- `R06DetailsBudgetBehaviorTests-current-main.trx`：`636A0B6DA62888B7C078B0B0935FA6102EE7DC49874AD837090787A6A05D1336`
- `MediaInboxGeometryTests-current-main.trx`：`D200F3F5DC303516838D7C566212B9B10B45D3A46EE6C0285446A83B314C52EA`
- `TaskCenterViewResponsiveTests-current-main.trx`：`16EDD0253703C215895B7D46639D6479BDCD05E606CA964AEE76ACC21DAFBE76`
- `DetailsDisclosureSourceTests-current-main.trx`：`70490A9244A772DA95BE58ACBD11157C1A97AC67CF803A06B2052DB7F7D359DB`

`python scripts/validate-source.py` 通过；`git diff --check` 通过。旧 `.tmp/r06-08-render-final` 当前不存在，旧 RenderHarness 报告和截图绑定实现提交 `07376adb`，不是当前 main 像素证据。本轮用实际 STA WPF 几何/绑定行为验证任务条件，没有另生成整套截图矩阵。

## 边界与下一步

当前源 checkout 仍不含 Demo 原始 `DesignShellView.xaml`/`Pages`；沿用恢复生产基线，不切换设计体系。没有启动真实 Playnite/Worker，未验宿主 UIA/读屏、OS 输入/IME、物理 DPI/跨屏、presented frame、ETW、宿主性能或真实服务失败时序；没有操作真实存档、媒体、云端或诊断数据。

用户提供了 #962/#963 诊断包：两次 Release build 都通过，实际 CI SDK `8.0.423`、VSTest `17.11.1`；失败步骤是无关的 R08 数字动效夹具，反馈计数实际 `0`、期望 `1`。原始包摘要和测试输出、失败解释见 [R08-06 当前 main / CI 门控复核](../R08-06-CURRENT-MAIN-RECHECK-20261013/README.md)。#964/run `38339686245`（job `115107636301`）在 R06-08 提交 `83fbee97` 上也终态失败于“编译与测试”；匿名 artifact partial 返回 404，尚无该 run 的完整日志。已按 #962/#963 的实际断言修正数字动效测试输入；本次修正提交推送后仍须跟踪新 Actions 至终态。

下一项仍为 `R07-01 滚动所有权` 当前 main 复核：先核对页面/表格/详情/弹层多层 ScrollViewer 的所有权及边界传递测试，复用现有实现。真实 Playnite、UIA/读屏、OS 输入/IME、物理 DPI/跨屏、presented frame、ETW/宿主性能与真实服务失败时序仍未验。
