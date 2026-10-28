# U07-05/06 通知详情与 Toast 完整查看

2026-10-28，基线 `7c4d07753ec63b32964ebf3bb1f6b66ea7a8724f`，两项均为局部推进。通知收件箱沿用虚拟化表格、筛选、真实关联 ID 和已读安全语义，详情标题旁由仅级别变为级别与相对时间；完整本地时间仍可由提示/UIA及原详情时间字段查看。通知契约没有来源字段，不虚构来源。

Toast 此前只有错误或有不同详情时提供按钮，长摘要与详情相同时只能悬停提示；现在非空内容均可打开原有结果对话框查看完整文本，详情为空时回退摘要。关闭/详情按钮增加 UIA 名称；鼠标或键盘焦点在卡片内时暂停计时，离开后恢复，已移除的计时器不会被焦点事件重新启动。自动弹出不请求焦点；原有四条上限、退出动画和清理不变。多个 Toast 实际覆盖与退出重排仍未完成验证，不能把 U07-06 结项。

SDK 8.0.423 Release 定向 5/5、0 失败、0 skip，包含生产 Toast 创建、长文本完整查看、键盘焦点暂停与关闭清理、收件箱时间绑定，以及既有异步通知焦点测试。[脱敏 TRX](tests-redacted.trx)，原始 SHA256 `698D66FFD91802740D1A048D2B5C01D8AF51B6820A2B2321A1D95C59C3FA4A39`。测试基线身份含未提交本批修改。源码检查通过、XAML 26/26、WPF 静态审查 37 文件/0 错误/26 既有警告/179 info。RenderHarness 0 警告/0 错误，双主题 render-qa exit 0、394 PNG、0 PROBLEM；报告 SHA256 `5F409A046175AF529DD4EF36F29D2BE4637B2EA43E1B834DBDFC8451C6BF834C`，报告工作树未清洁且 Commit 为基线；不代表真实宿主弹层截图。

清理：当前阶段 `.tmp/round5-u07-05-06/` 最终 TRX、日志与 53,232,510 字节当前渲染目录保留用于审计；无本批旧副本可删除。未触碰其他任务、用户 TRX/TestResults、存档、配置。C/D 可用空间由 3,209,289,728 / 14,614,654,976 字节变为 3,205,894,144 / 14,558,396,416 字节。真实 Playnite、物理键盘/鼠标、UIA、DPI、消息来源与多条堆叠待验；新提交双 SDK CI 和整轮完整构建安装启动仍待验。

## 2026-10-29 长标题密度增量（U07-05/U07-06）

源码提交 `dcc288d66368b05884e47d57db0749a98ea17a36` 已推送到 `main`。收件箱通知标题在固定行高内单行省略，鼠标 Tooltip 与 UIA Name 保留完整标题；Row/Column virtualization 未改。Toast 标题统一单行省略并可悬停查看全文，卡片 UIA Name 与原详情对话框继续提供完整标题/正文。通知契约没有来源字段，本批未猜测来源，也未更改任务/已读/恢复安全语义。

SDK 8.0.423 Release / net472 定向 `NotificationInboxViewBehaviorTests|Round5ToastBehaviorTests` 精确SHA `dcc288d6`：7/7通过、0失败、0 skip，退出0；[脱敏TRX](../U07-05-06-title-density/tests/U07-notification-focused-dcc288d6.redacted.trx) SHA-256 `DD4FD6B8BB405407FBAF4D1707276DFA16726003176A94FA4D3433B016C5A2BA`。首个新增用例因夹具在DataGrid视觉树尚未暴露元素而失败1项；改用生产 `DataGridTextColumn.GetCellContent(row)` 后通过。初次问题及已脱敏诊断保留于[首轮失败TRX](../U07-05-06-title-density/tests/U07-notification-first-attempt-failure.redacted.trx)，SHA-256 `916E1B9311A09C17F288CC87C004397E31ADF333706226C2B3F9D536A36AAD19`。测试启动时NuGet漏洞数据源返回NU1900不可达警告，测试仍成功；未关闭或跳过任何断言。

`validate-source.py`通过；WPF静态42 XAML / 0 error / 26既有warning / 179 info。`scripts/render-qa.ps1`固定样本 `2026-10-27T00:00:00Z`，双主题和11个逻辑窗口共394 PNG、0 PROBLEM、退出0；RenderHarness Release编译0警告/0错误。[前后通知页截图及SHA](../U07-05-06-title-density/render-pair-dcc288d6.json)，[提交后Render QA报告](../U07-05-06-title-density/render/render-qa-dcc288d6-report.txt) SHA-256 `FB85ADECFB9A5E5AD859EB03033CE92A6B0510E2AEF6D8F01B142BBFCC00B93F`。前后1040×700 DIP通知页样本未含溢出标题，故截图用于尺寸/主题无回归核对；长标题行为由WPF测试覆盖。渲染是1.00逻辑DPI离屏Harness，不代表物理DPI或真实Playnite弹层；toast遮挡关键操作的真实宿主关系仍待验。Demo目录本地副本缺失，本批只调整真实生产通知/Toast资源，不声称完成Demo对照。

源码验证、测试、截图和CI状态的结构化清单见[本批验证记录](../U07-05-06-title-density/validation-dcc288d6.json)、[CI查询记录](../U07-05-06-title-density/ci-progress-dcc288d6.json)。代码已推送，但GitHub连接对该SHA combined status为空、PR-only workflow-run查询为空；公开check-runs API返回403 rate limit。无可核run/job/artifact身份，标为“远端待验”，不按本地绿灯推断CI通过。CI恢复后的精确查询步骤见CI记录。
