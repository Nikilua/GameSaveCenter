# Q13-08 分页锚点与实际总数复核

日期：2026-09-29（Asia/Shanghai）；代码身份：2333361ecc38b16d00559a8fb91e551c89e0a17c。
分支：main；复核前 HEAD 与 origin/main 一致。此阶段未改生产代码或测试断言，只归档既有能力的当前身份证据。

## 构建身份

- Release solution build：成功，0 warnings / 0 errors；XAML structural checks：24/24。构建日志：[release-build.log](release-build.log)。
- Plugin informational version：0.6.73+2333361ecc38b16d00559a8fb91e551c89e0a17c；程序集版本：0.6.73.0；MVID：14e68a62-3439-4baf-bffc-23088d0f0b5f。
- GameSaveCenter.Playnite.dll SHA256：919B97E3B93DB9197BD597FE4C74B644806FBB502F63864E2B2FD6AB97A8666F。
- GameSaveCenter.Playnite.Tests.dll SHA256：78DA4425E8B8462445A747055C42C4C891CC1D2ED9FE2E17CD3A90DCFCD44F29。
- GameSaveCenter.Worker.Tests.dll SHA256：2D98E080046E0DD00878115C94525D5B81A72D6C39BB1A1B3B7C519AFB7D8999。

## 当前身份测试结果

五个独立 VSTest 进程均 exit 0，合计 29/29 passed、0 failed、0 skipped。逐类 TRX 和 console：

| 测试类 | 结果 | 覆盖 |
| --- | ---: | --- |
| MediaInboxScrollBehaviorTests | 5/5 | 生产 MediaCenterView 与 MediaPageAccumulator 追加分页；稳定锚点行在有界窗口头部裁剪后索引前移 50，视口内首行位置仍在 1.5 device pixel 容差内；顶部/中段/底部、往返、窗口尺寸变化、末页操作可达；保留并断言行虚拟化、Item ScrollUnit 和现有 VirtualizationMode.Standard。 |
| MediaWindowAnchorContractTests | 10/10 | 分页锚点捕获/恢复接线、负责滚动容器、代次失效和锚点淘汰后的反馈等生产契约。 |
| R07SelectionAnchorBehaviorTests | 4/4 | 稳定 ID 优先于旧行索引；ID 不存在时回退到邻行而非首行；真实 WPF DataGrid 刷新后的邻行选择。 |
| MediaPageAccumulatorTests | 6/6 | 250 页×200 条仍限制为 2,000 项并保留选中项；重叠页按 MediaId 去重更新；200/2,000/10,000 后端规模的有界窗口。 |
| MediaQueryPersistenceTests | 4/4 | fake service 与 GUID 隔离 SQLite；同时间戳的稳定游标、跨页无重复、TotalCount/HasMore、收件箱/忽略总数和筛选搜索。 |

每类保存了 TRX 和原始 console，例如 [MediaInboxScrollBehaviorTests.trx](MediaInboxScrollBehaviorTests.trx) 与 [console](MediaInboxScrollBehaviorTests-console.log)。测试使用合成条目、fake 服务和隔离临时数据库，没有访问用户媒体、存档或云端。

## Q13-08 条件映射与边界

- 已验证的能力：加载更多追加时按稳定 ID 还原可见行及其视口位置；头部裁剪后的锚点与选中行回退到稳定邻项；缓存保持 2,000 条上限并以 ID 去重；Worker 分页在同时间戳下使用稳定游标并保留实际匹配总数。Media Inbox 仍启用行虚拟化、Item ScrollUnit 与 VirtualizationMode.Standard，未改变此组表格例外。
- “返回最新”是明确操作，产品语义要求回到最新页/顶部，不记作意外跳顶。
- 这些测试组合覆盖了视口、缓存和后端分页组件，但没有端到端驱动真实 DashboardViewModel/fake service 并断言刷新或删除后绑定的总数文本；也没有在真实 Playnite 中完成追加、刷新、删项及排序/筛选操作。该闭环仍待补验。
- 当前 WPF 夹具报告的有效 DPI 为 150%；测试中的 1.25/1.5 RenderTransform 是合成缩放，不当作物理 125%/150% DPI。没有把 STA 合成窗口称为 Playnite 最终呈现。
- 因用户要求的真实宿主分页操作和完整绑定总数闭环尚未验证，Round2 状态继续为外部阻塞/未完成。没有新增生产实现或声称签收 Q13-08。

## 下一步

推进依赖已满足的 Q14-01 工具栏同高：先审已有共享样式和实测断言，按现有能力补缺证；Media Inbox 的真实表格滚动错位仍需安全运行宿主下同一坐标系的滚动前后诊断日志。R08 用户侧 1/2 失败仍缺失败方法、断言/堆栈及原始构建身份。
