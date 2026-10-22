# 后续标准入口诊断

默认中文入口始终执行完整链，没有使用 SkipTests，也未改动 required checks、skip 或身份门禁。

| 尝试 | 源码与 run | 结果 |
| --- | --- | --- |
| 3 | a5c96c8a / 20261022T101945607Z-c4d5d102 | 外部执行会话消失；诊断仍为 running，没有根命令终态退出码。仅记录为中断，不归为产品失败或通过。 |
| 4 | a5c96c8a / 20261022T103327001Z-ab3127e3 | 运行期间主检出被另一笔纯文档提交推进到5ced8abb；source身份门禁拒绝旧程序集/新HEAD组合，根退出1。未放宽门禁，改用固定工作树。 |
| 5 | 5ced8abb / 20261022T104511986Z-6ba82645 | 固定工作树编译/Core/Worker/source通过；第62个WPF类R07DetailsBreakpointBehaviorTests失败，累计1344 pass/18 skip/1 fail，根退出1。 |

第五次唯一失败是旧结构断言读取内层滚动容器局部行0，预期工作区行4；生产结构已经改为固定圆角外框承载工作区行号。修正断言先确认命名Border和原ScrollViewer子对象，再检查外框行4/列0；选择、焦点和滚动位置断言保留且在旧失败中已通过。修复提交 a57159cf 的完整链另行记录。

原始证据：[中断观察](interrupted-attempt/observation.json)、[中断摘要](interrupted-attempt/summary.json)、[HEAD变化运行](moving-head-attempt/summary.json)、[根退出码](moving-head-attempt/observation.json)、[固定检出摘要](fixed-checkout-failure/summary.json)、[固定检出退出码](fixed-checkout-failure/result.json)、[R07失败TRX](fixed-checkout-failure/R07DetailsBreakpointBehaviorTests.trx)。各目录保留控制台及原始SHA；归档文本只去除行尾空格。

[真实媒体视图绑定探针](binding-probe/result.json) 使用5ced8abb的Release DLL，宽窄往返1400→860→1400→860→1400；已选合成批次始终为synthetic-b，空选择写入0次。只加载合成对象，没有真实媒体操作；探针源码与程序集SHA一并保留。

第六次：a57159cf / run 20261022T110212727Z-fcf7cf8b，R07详情断点已通过；第64类缩放压力测试仍要求子控件固定MaxHeight（旧表格上限、旧宽详情420上限），与有限父Grid伸展分配冲突。累计1347 pass/18 skip/1 fail，根退出1。现验收实际命名页面Grid的有限Height、边框内预算与表格填满、宽详情填满、紧凑136上限、PageHost上限及缩放恢复，保留选择/焦点/抽屉路由/真实SizeChanged检查。[原始失败](resize-contract-failure/resize.trx)、[摘要](resize-contract-failure/summary.json)、[退出码](resize-contract-failure/result.json)。新增探针首跑遗漏宽详情Infinity相减导致NaN，已改为比较真实详情视口高度；该探针失败保留本地artifacts/ui-layout-fix/tests/r07-final，未作为通过证据。

第七次：dee44e98 / run 20261022T112558956Z-428a4aa6，R07断点与缩放、R09焦点2/2均通过，第95类版本备注测试独立宿主没有调用生产壳层的ApplyResponsiveLayout，新固定外框仍折叠，视觉树找不到备注编辑器。累计1437 pass/18 skip/1 fail，根退出1。[失败TRX](version-note-fixture-failure/version-note.trx)、[摘要](version-note-fixture-failure/summary.json)、[退出码](version-note-fixture-failure/result.json)。修复测试宿主接线，并扩展1040/860宽窄两种：调用生产布局，窄窗使用真实详情按钮路由，要求外框可见、原备份仍选中，再执行绑定取消命令并检查原备注/锁/ID。未修改生产取消或保存逻辑。

随后主动复核相邻恢复详情fixture：R12RestoreReadinessViewBehaviorTests原1/1失败（未生成详情视觉树）；R12RestoreWorkflowBehaviorTests原7 pass/1 fail（同一遗漏）。两者均补生产布局调用并明确断言固定外框可见，原哈希未知/部分覆盖/过期/损坏说明、恢复步骤/错误/状态变化断言完整保留。原始TRX保存在tests/R12RestoreReadinessViewBehaviorTests.trx和tests/R12RestoreWorkflowBehaviorTests.trx，修复后结果使用不同文件名归档。
