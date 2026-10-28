# U05-05/06 加载与真实进度

日期2026-10-28，基线bca60704，本批候选源码/资源与测试差异。

U05-05：共享WorkspaceStatePresenter此前在Loading可见时立即绘制提示卡；现在以180ms一次性DispatcherTimer延迟视觉提示，模板StateSurface用Hidden保留原测量槽位。State和业务读取不延迟，快速响应不展示整卡；慢响应正常展示标题与原有下一步说明。Error/Empty等状态立即可见，不延迟错误或重试。状态退出、不可见、卸载停止并解除计时器回调；重挂重新计算，后台任务和取消语义不变。不使用动画伪造成功，也不强制最短Loading时长。

U05-06：去重复用TaskStatusDto.ProgressValue/ProgressDisplay、固定进度列和独立任务生命周期/取消字段，不增加平滑补到100的逻辑。新增实际生产TaskCenterView表格单元格回归：未知、0%、73%、取消73%、成功100%连续替换同一任务时，进度条宽度与标签右边界不变，显示真实进度及取消状态。

SDK8.0.423 Release最终组合13/13、0失败、0 skip、退出0，含WorkspaceLoadingFeedbackBehaviorTests、R06TaskProgressBehaviorTests、R06EmptyStateBehaviorTests及R07StatusBannerBudgetBehaviorTests。初始12项通过；加入几何检查后12通过/1失败，因为测试错误要求破折号与百分比文字自然宽度相同（12与16 DIP）。修正为固定列中的右边界和进度条宽度，无生产进度逻辑变更；失败及最终通过TRX均保留于tests/，hash见tests/hashes.json。

Source validation通过；XAML26/26；WPF静态37 XAML、0错误、26既有warnings/179 info。scripts/render-qa.ps1实际退出0，RenderHarness Release0 warning/0 error、双主题394 PNG/0 PROBLEM。候选报告明确Commit=bca60704、WorkingTreeClean=False，不称精确新提交结果。报告、命令日志、逐图hash在本目录；全量当前截图仅保留`.tmp/round5-u05-05-06/render`一份。终态画面不代表180ms中间过程，该行为由真实WPF模板可见性/时序检查覆盖。

前后对照：加载卡由立即Visible变为180ms内Hidden，保留同尺寸；到期恢复Visible；短响应Empty及Error不被旧计时器覆盖。状态文字、标题、命令、滚动条、选框、列表虚拟化与进度数值契约保留。

状态为代码完成待验收，真实Playnite/系统输入/IME/UIA客户端/物理DPI未验。提交后查询精确SHA双SDK CI；不由本地或父提交结果推断远端绿色。整轮最终候选仍需中文根入口完整构建/测试/发布/打包/安装身份/启动与独立scripts/build.cmd。真实存档恢复、媒体删除、云端写入未执行。

disk-before/after记录空间。本阶段当前截图、命令输出、失败诊断与最终TRX仍在使用；报告和hash已归档。用户和其他任务TestResults/TRX、存档和配置均未动。没有创建包副本或一次性脚手架；清理结果见cleanup.json。
