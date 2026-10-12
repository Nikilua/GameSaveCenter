# R06-04 当前 main 快捷键行为复核

日期：2026-10-13（Asia/Shanghai）

结论：复用 `afe4aa55` 已实现的 `DataGridClipboardBehavior`、五类生产表格 profile、DTO 和脱敏策略；当前 main 的原行为测试能证明格式化值和表格接线，但没有实际触发快捷键路由。本轮只增加一个内部 modifiers 测试 seam 和一个受控 WPF `PreviewKeyDown` 行为测试，没有改生产快捷键或剪贴板行为。

## 构建与测试

- 构建源码基线：main `e277a804bd4774cd32ad0bde0dfe2b8f76bc3308`，本轮工作树仅改测试 seam 和测试。Pinned SDK `8.0.423` 位于隔离目录；系统默认 dotnet 只发现 `9.0.302`，没有用它替代项目要求的 SDK。
- 官方 `scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot ... -DiagnosticsRoot ...`：Release `0 warnings / 0 errors`，XAML `24/24`；Playnite `net462`、tests `net472`。隔离输出与诊断目录均在仓库外的可再生目录。
- `R06ClipboardBehaviorTests`：`5 passed / 0 failed / 0 skipped`。包括既有格式化/多选显示顺序/ID 去重/完整路径/凭据脱敏/五类 profile 接线，以及本轮实际对生产 `DataGrid` 触发 `PreviewKeyDown` 路由的 Ctrl+C 行复制、Ctrl+Shift+C 单元格复制。负例覆盖 Ctrl+Alt+C、Ctrl+X、解绑后的 Ctrl+C 与没有选中行；未复制时保持 clipboard seam 内容不变，无选择时呈现失败提示且不吞事件。
- `R22CopyFeedbackBehaviorTests`：`2 passed / 0 failed / 0 skipped`，复核剪贴板占用失败提示和重试反馈。
- `python scripts/validate-source.py` 与 `git diff --check` 通过。测试 TRX 去掉本机路径、用户名、机器名、run name 和 deployment root，并规范为 LF。

## 身份

- `DataGridClipboardBehavior.cs` SHA-256：`A16229D66427BA77F2BF2F44D841D0D026AB0A018D9082E40F4F73D7D0208A5A7`。
- `R06ClipboardBehaviorTests.cs` SHA-256：`99381AD3D4147881DC6C02DCE72F61831A1A073072BA407F755B89D0DD55FA79`。
- Playnite DLL SHA-256 / MVID：`63FA4310CC8295EA58E76B79EDEF2B76CD32046152A91705A529A11CB50265EA` / `d1ab9447-edbe-434a-9d4d-98300a29848b`。
- Playnite tests DLL SHA-256 / MVID：`6DA620E1ED0E749D7CF5ABC4BDE27BBDC7DA519AB0909E2993C613881CA042BE` / `c3e76dff-fedb-47f8-9cfc-4db12727e098`。

| TRX | 结果 | SHA-256 |
| --- | --- | --- |
| [R06-04-clipboard-route.trx](tests/R06-04-clipboard-route.trx) | 5/5 | `56D98AA6D7C9161D9969F8D6CA7346FAB90405DB2B0E308927D1C147D020E3F4` |
| [R06-04-copy-feedback-regression.trx](tests/R06-04-copy-feedback-regression.trx) | 2/2 | `8D39BA678E2A169FA367ED321024C287F255FAEA585D89C6C8D3FCEEB030A49B` |
| [Q14ToolbarAlignmentBehaviorTests-sdk8423-current-main.trx](tests/Q14ToolbarAlignmentBehaviorTests-sdk8423-current-main.trx) | 1/1 | `3CBD5118E065B57F32A53730B5760ACE959AA26E221B7C970CDC7A8F27A8F016` |
| [Q14ToolbarAlignmentBehaviorTests-sdk8423-main2f546-recheck.trx](tests/Q14ToolbarAlignmentBehaviorTests-sdk8423-main2f546-recheck.trx) | 1/1 | `25787A2B2962847E2FB33C6AB94BCD84423B7F16EEFD668ED554F4D6EA6EC79F` |
| [ProductionShellChromeSourceTests-current-main-sdk8423.trx](tests/ProductionShellChromeSourceTests-current-main-sdk8423.trx) | 22/22 | `EDCBB94F2D46A1E76A7FF8FAE71BD7CC62EFB83F3FA57DCC491717B74A92B8FE` |
| [R06ClipboardBehaviorTests-sdk8423-current-main.trx](tests/R06ClipboardBehaviorTests-sdk8423-current-main.trx) | 5/5 | `AF531439D36DACE30632601A257795114A2B9240A1FF6EA787A9741060F0B051` |
| [R22CopyFeedbackBehaviorTests-sdk8423-current-main.trx](tests/R22CopyFeedbackBehaviorTests-sdk8423-current-main.trx) | 2/2 | `1567374318B576B7DCA51A92ACB9C291A311C873BD7B10E88E39DD2B722A33F7` |

## Actions 失败复核与当前 main 校正

- #919 (`36675309812`, `a6b6dfc`) 与 #920 (`36677346317`, `77de7f4`) 的完整日志已取回；二者实际 SDK `10.0.401`，均显示 `ProductionShellChromeSourceTests.ReducedMotionCancelsAnActiveSidebarTransitionWithoutLateClockWrites` 和 `SidebarTransitionReleasesClocksOnCompletionAndUnloadInAnActualWpfWindow` 的 transition-running `Assert.True` 失败（实际为 `false`）。日志 SHA-256：#919 `69AC5DF7900AA0122B3D55E02436AFF9515C8FD829C15240D4014AE86649B25E`；#920 `8EB5989E08ED940D7C9E9CB1CF870A20F91FD94DB933704A220D490D80C08FFF`。当前 main 相对该检查点已有侧栏代码与测试变化；当前测试类在本机 SDK `8.0.423` `22/22`，不能视作 SDK10/CI 重跑。
- #947 (`90454586`)、#948 (`fdeeec8`)、#949 (`e277a804`) 与 #950 (`0e031b12`) 的 CI job log 均取回。#949 SHA-256 `9E4332A7DE8D73F9735AE6AF5CAEE684DC56FED534AF6402643C1B9741468A32`；#950 SHA-256 `8C9C3EE938A2F3181B7ACF1DFD308E1B4D375CF8D37A36E702903E05805C2E2A`。#949/#950 实际 SDK `8.0.423`，相同失败为 Q14 `ProductionTaskToolbarsKeepMixedControlGeometryAndTextCenters` 的 compact 断言：测试按原请求宽度决定预期，随后却把窗口实际 `ActualWidth` 传给第二次布局。Expected False / Actual True 与两者分处 1216 DIP 断点两侧相符；但旧断言没有记录请求/实际宽度或 DPI，无法恢复具体测量值，也不能断定 DPI 是根因。
- 校正仅触及测试：保存第二次 `ApplyResponsiveLayout` 的 `appliedWidth`，用其判断 compact，并在断言失败信息中记录 requested/applied/arranged width 与 DPI。没有改生产布局或断点。
- 本机 Windows `10.0.22000`，Pinned SDK `8.0.423`。校正后的 Q14 `1/1`、`ProductionShellChromeSourceTests 22/22`、R06Clipboard `5/5`、R22 `2/2`；完整 Release build `0 warnings / 0 errors`、XAML `24/24`，source validation/diff check 通过。VSTest 为 .NET Framework 4.8.9345.0。进程有 TextServices `InvalidComObjectException` teardown 输出，但各 TRX 和进程退出码均通过。
- 校正构建的源码身份为 main `0e031b12` 加 Q14 测试工作树差异；Q14 测试源 SHA-256 `5FAE90542DDBC41A96F56C77B06905B352115CC192666BD36D581B88BCBAC8C7`，侧栏测试源 SHA-256 `33879158CDE26DDB40451D5751DC55DCF93064CDD86BE1C062B47D1721B7CB06`。Playnite DLL SHA/MVID `9737C86C62373FF5586BB8266745FA7480EE5B255D05372A284F6005F03F4C87` / `5368efe3-c19a-4278-9adf-2479358bcb90`；tests DLL SHA/MVID `AFF10C272E772BB7011EE2DA3941974FFE770873F9BB37491B37DBDF092B8DD8` / `2d754f5c-3683-4ed0-90b0-ab599c481331`。
- #951（run `38306094571`，commit `2f546d06`，job `115000392485`）已终态失败；CI SDK `8.0.423`、Windows `10.0.26100`。完整 log SHA-256 `96BCAC239C2E6BB25DA54E725319FA9AB6EC6964C3662D2C4AFAAB0A266406DE`（128,002 bytes / 1,015 lines）。隔离序列从 `[24/114] Q14ToolbarAlignmentBehaviorTests` 继续到 `[25/114] R02ActionAvailabilityHintTests`，据此 Q14 类已通过；后续失败位于 `R05TooltipTimingBehaviorTests.QuickMousePassesCancelTooltipAndAStationaryDwellOpensIt`，预期 Tooltip 开启计数 `1`、实测 `0`，不是 Q14 断点回归。
- Artifact `11773198841` 页面摘要 SHA-256 `fd4cdd6972f4b5d80c606b14ffdb3d3c259d4dcc8f1e9c7c3b24ebb76b5fa222`；匿名 artifact 下载 HTTP 401，未声称检查产物内容。失败测试夹具与本机诊断见[R05-07 复核包](../R05-07-CURRENT-MAIN-RECHECK-20261013/README.md)。
- 当前 Q14 本机复跑 `1/1`，TRX 文件名、测试计数和 SHA 见上表；最新隔离程序集包含 R05 测试夹具修正，Playnite SHA/MVID `A602B8F77EA4B983C28BB9CA4840D87DD94DEE2ACEEF7581F58A232CB99140A8` / `6bba8de6-5002-4886-ba04-9650d01eca0b`，tests SHA/MVID `2AAEB335A9ED08C1B8BDD5142121DCCAECDA239E651BD06809A82E5E379B794B` / `0cd8cbed-538c-4f4d-bffd-7f5533f9647e`。
- R05 测试夹具修正已提交至 main `4572118f3c671981c3d6f93a66820910bc3b94b2` 并推送。对应 Actions #952（run `38309632982`，job `115011915461`）已失败：公开页面标记“编译与测试”exit1，job 用时 `12m 7s`，未显示具体测试/命令。诊断 artifact `11774606363`（556 KB，SHA-256 `9686cc633488c860c9dd8021690986abfff69129f1420ced7b433ff062490a3a`）；job 日志要求登录，匿名 artifact API HTTP403 rate limit，匿名下载路由 HTTP404，未读取 artifact 内容。仓库 `global.json` 配置 SDK `8.0.423`，本次 runner 实际 SDK 未确认。该 run 未提供 Q14 是否通过的可读日志；不以 #951 的通过结果推断 #952。R06-04 与 R05-07 暂不收口，取得完整日志前不改代码；本机 TRX 不替代 CI 门禁。

## 验证边界与下一步

快捷键测试在显示的隔离 STA 窗口中对真实生产 DataGrid 触发 WPF routed event；modifier 通过仅供测试使用的内部 seam 提供，clipboard 写入使用 fake setter。它不代表真实 OS 键盘、系统剪贴板或 Playnite 宿主输入。没有视觉布局改动或当前 RenderHarness 截图；双主题渲染证据仍绑定原实现 `afe4aa55`。真实 Playnite、系统剪贴板占用、物理 DPI/跨屏、IME、UIA/Narrator 和宿主性能仍未验，也未触碰真实存档、媒体、云端或诊断数据。

旧阶段的 CI 结果与 SDK差异、完整日志哈希和本轮校正过程见上方“Actions 失败复核与当前 main 校正”；#952 的 CI 失败根因受日志访问限制尚未知，R06-04 暂不收口。

按用户顺序先复核 R00/R01 问题与证据，再推进依赖已满足的 Q/R 项；R06-05 列头说明已有满足证据，届时按当前 main 复核是否需要补充。R05-07/R06-04 的 CI 失败根因仍待有权限日志，不因继续独立任务而收口。
