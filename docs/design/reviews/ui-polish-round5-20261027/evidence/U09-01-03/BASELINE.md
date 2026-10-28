# U09-01/02/03 回归证据

更新：2026-10-28。源码提交 `8deb17c75ac91d716ec38b05f6f0a54530df2217`。本批只更新 RenderHarness 假数据时钟与 `scripts/render-qa.ps1`；没有改生产 XAML、真实业务命令或主题资源。

## U09-01 固定样本与截图

RenderHarness 使用 `GSC_RENDER_SAMPLE_UTC` 固定假数据中的相对时间；未指定时同一进程只采样一次当前 UTC。`scripts/render-qa.ps1` 接受 `-SampleUtc`，只向本次 Harness 子进程传值并在结束后恢复环境变量。当前样本为 `2026-10-28T09:00:00Z`。

精确提交 `8deb17c7` 上连续执行两次固定样本的完整 Render QA。两次 1040×700 存档页双主题图逐像素稳定：Light 像素 SHA-256 `4D195AEDF62103567E07DCCB3CAE37AEF1527A415DF4933E4002D422056FDF94`；Dark 像素 SHA-256 `27E0EE2569A281E5B039382A5E80FCF0E520E62FCA2C795AA7260D77137C613F`。第二次报告生成于本地 21:56:50，退出码 0，394 张 PNG、0 `PROBLEM`，逻辑 DPI 1.00，覆盖 Light/Dark 与 11 个尺寸。原始报告 SHA-256 `1647F2E948328526829EAB5F9F441612A36044F8B77A8BEEF3519E4FFBAEF0BF`（1,141,900 bytes）；脱敏归档为 [Render QA 报告](render-qa-report-redacted.txt)，SHA-256 `5DB61ED68576FB2261766059509A41105AFC7A0F9E0183A657D11A1375A81EDA`。

归档的 1040×700 存档页样图为 [Light](current-save-light-1040x700.png) 与 [Dark](current-save-dark-1040x700.png)，文件 SHA 和像素 SHA 见 [截图清单](screenshots.json)。U00-01 原始图在 [U00-01 基线](../U00-01/BASELINE.md)。当前图与 U00 图仍不是同数据前后对照：U00 的页面状态、样本时间和生产界面均不同；逐批配对缺口保留，不能由时钟夹具自动补成历史证据。已存在的有效前后图见 [U06-07/08 候选表](../U06-07-08/BASELINE.md)。

报告记录 `WorkingTreeClean=False`，因为仓库已有未跟踪的其他阶段证据/输出。精确渲染时 `git status --untracked-files=no` 为空，即本轮跟踪源码已冻结；未清理未知输出。

## U09-02 键盘与 UI Automation

SDK 8.0.423、Release、精确提交 `8deb17c7` 的定向组合 16/16 通过，0 失败、0 skip。覆盖 `R21KeyboardNavigationTraceTests`、`R21AsyncCompletionAnnouncementBehaviorTests`、`NotificationInboxViewBehaviorTests`、`R07StatusBannerBudgetBehaviorTests`、`R09FocusOutlineBehaviorTests`。原始 TRX SHA-256 `EA398B74E2907CD430A7A48503E28E50105651E384888369E0251C1E861996FE`（49,643 bytes）；脱敏副本为 [精确提交 TRX](tests-candidate-redacted.trx)，SHA-256 `CC6A821ACBB2E29A348819A74C537DCFAA11883EEA2A2A0224A82AFD100F8428`。

测试用 WPF `MoveFocus` 检查导航顺序，用 UIA Peer/Invoke 验证名称与命令入口。它不等同于实体键盘、真实 IME、外部 UIA 客户端或 Playnite 内焦点验收；这些边界保留。

该阶段本机排查留下的9份失败TRX（R07状态横幅与列菜单焦点，共10个失败结果）已做路径脱敏并归档：[失败TRX压缩包](historical-failed-local-trx-redacted.zip)、[逐项SHA和失败用例清单](historical-failed-local-trx-manifest.json)。单份历史TRX没有完整源码SHA字段，因此只作故障调查记录；精确当前候选的成功TRX见[U09-05最终门禁](../U09-05-final-5f0eb620/BASELINE.md)，旧结果不覆盖新源码结论。

## U09-03 尺寸、主题与 DPI

精确提交 Render QA 覆盖 1040×700、1100×720、1280×720、1366×768、1536×864、1600×900、1707×960、1920×1080、2048×1152、2560×1440、3840×2160 DIP，双主题共 394 张 PNG、0 `PROBLEM`。报告 SHA/尺寸清单见 [截图清单](screenshots.json)。3840×2160 是逻辑 DIP 离屏测量，不是实体 4K 屏幕。物理 DPI、多屏切换、Playnite 外壳尺寸与主题交叉仍待真实宿主。

质量检查：`python scripts/validate-source.py` 通过；`scripts/check-xaml.ps1` 26/26 文件通过；WPF 静态审查扫描 42 个 XAML，0 errors、26 warnings、179 info。警告为现有资源/布局候选；本批没有生产 XAML 改动，静态审查不代替 Playnite 渲染。Render QA 内含 RenderHarness Release 构建，脚本和全场景退出码均为 0。

## 历史 CI 与待验边界

R07 测试夹具缺少 `SaveTabIndex` 的失败历史仍保留在 [ci-failure.json](ci-failure.json)：提交 `d4c4357c` 的 run `40660226413` 两个 job 均失败于“未选中存档页导致 UIA Invoke 不可见”。修复仅补齐测试夹具并增加可见/排列/UIA Invoke 断言，没有删除断言或改生产命令。

修复提交 `2b6c4022b374d7ca8d2aae1914bb322cbca8c547` 的 run `40688405018`（run #1359）baseline job `122816641999` 与 SDK10 job `122816642269` 均成功。产物：Windows preview artifact `12820255378`（90,784,388 bytes，`sha256:25a34f460cbd48bd7986cf0ab0920e4be0d10ccb5a7b616b30728852ece3cf85`）；baseline diagnostics `12819921352`（1,999,228 bytes，`sha256:6342a3394a6b9b94bf585d01a596a3e90fc02a6a7adfed017727cdf2b108db3e`）；SDK10 diagnostics `12816647710`（955,213 bytes，`sha256:57d5be1d7d135df8a30db8dced32f6b3d557c42403597eb8cded4e4205a31a5f`）。

新源码提交 `8deb17c75ac91d716ec38b05f6f0a54530df2217` 的 run `40699146599` 中，SDK10 job `122853896050` 已成功，artifact `12822401732`（954,683 bytes，`sha256:0e3f437dc24d0ba32734b3052aa752c441f2bee56afedf04232ab041415adae4`）；baseline job `122853896172` 在本次记录时仍运行，不能据此结项。required-checks 接口曾返回 403 `Resource not accessible by integration`，规则集读取为 `[]`；不推断保护状态，待有权限的仓库维护者核对 `main` 的 required status checks。既有 S/T 宿主与权限待验项保持原样，不重复探测。

本文件建立时，最终中文入口、独立 `scripts/build.cmd`、安装身份/启动链和产物清理尚未执行；这些门禁后续已在[精确候选U09-05最终证据](../U09-05-final-5f0eb620/BASELINE.md)完成。U09-04逐帧/分配基线与真实宿主复核边界仍见[性能证据](../U09-04/PERFORMANCE.md)。
