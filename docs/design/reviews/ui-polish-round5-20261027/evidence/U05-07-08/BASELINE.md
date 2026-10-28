# U05-07/08 页面离开与组合压力

2026-10-28，基线969bfc20。本批生产改动：真正换页前关闭键盘说明与命令层并归一化旧PageHost时钟；Shell卸载时同样收尾；TaskCenterView卸载时取消详情过渡代次与动画，撤销控件自己的命中锁。没有调用Worker任务取消或改变业务订阅契约；返回页面仍沿原缓存及最新数据绑定。

前后：原Shell卸载仅停止侧栏动画，原任务页卸载仅释放列表/配置订阅，可能保留说明Popup或详情退出回调。本批将这些视觉对象一并收尾。终态页面布局、游戏选框和滚动条不变。

U05-08复现序列：真实生产Shell初始Overview，交替Tasks/Overview共31次，每次切Light/Dark并打开说明Popup后切页；Tasks时打开详情，随即关闭并在退出中导航离开；返回Tasks再次检查实际可见的命中能力。最后主动开始1秒PageHost过渡、打开说明Popup，卸载Shell，检查Popup关闭、时钟撤销、Opacity=1和真实导航状态未改。使用合成页面DataContext和任务对象，不连接真实Worker、用户配置、存档、媒体或云。

SDK8.0.423 Release：最终组合3/3（31次组合序列、详情反向/锚点、加载反馈生命周期）；缓存导航/筛选/选择/滚动与源码守卫另3/3，全部0 skip。原始脱敏TRX和SHA见tests/。

失败诊断保留：首轮重复route连接处包含同一已选RadioButton，无Checked事件也没有真正离页；改为31次实际切换，同时保留独立同页缓存复用检查。组合后对隐藏/分离页面的有效IsHitTestVisible断言失败；最终按WPF异步Unloaded时序，离开后检查本地锁与closing状态、无活动时钟，下一次真正可见时再次检查有效命中。没有移除命中检查或放宽生产安全语义，三份失败TRX可回溯。

Source validation通过，XAML26/26；WPF静态37 XAML、0 errors、26既有warnings/179 info。scripts/render-qa.ps1实际退出0，RenderHarness Release0 warning/0 error，双主题394 PNG、0 PROBLEM。报告/命令日志/逐图hash已归档，候选报告Commit=969bfc20、WorkingTreeClean=False，不称为精确新提交结果；完整当前截图仅保留.tmp/round5-u05-07-08/render一份。

真实Playnite宿主、系统键盘/IME、UIA客户端、物理DPI和长期后台业务任务未实测；不能用本批合成序列推断。任务状态保持代码完成待验收；精确提交双SDK CI仍需提交后查询，required-checks既有403不重复试探。最终中文根入口完整安装启动链和独立scripts/build.cmd仍待整轮候选执行。

本批仅一份当前Render输出，无包/截图复制。当前失败诊断、最终行为TRX及审计仍需使用，暂无已确认无用的自有输出，删除清单为空；磁盘空间见disk-before/after。保留用户和其他阶段的TRX/TestResults、存档与配置。

独立R08OffscreenMotionBehaviorTests为1/1、0 skip，覆盖隐藏/最小化暂停及返回恢复，IsIndeterminate真实busy属性始终保留。本批总计7项通过；未连接Worker，不能据此称长期真实业务任务已实测。
