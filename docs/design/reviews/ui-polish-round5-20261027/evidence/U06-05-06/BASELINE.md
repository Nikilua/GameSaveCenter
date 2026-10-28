# U06-05/06 多选锚点与空表状态

2026-10-28，基线39c6ad80；本批只修改测试，无生产UI或业务差异。

U06-05复用生产多选摘要/清除和断点行为：存档历史的120行合成夹具先将有限视口滚动至第20行，再进入40项多选，检查偏移不变；双主题下三次窄宽往返保持选择与焦点，Escape保留选择，显式清除后偏移不变。媒体收件箱既有摘要测试核对当前窗口/隐藏选择数量和按模式清除，不操作真实媒体。物理键盘和真实Playnite未验。

U06-06新增生产TaskCenterView三态行为：Empty、FilterEmpty、Error分别显示相应说明；筛选无结果的按钮执行合成ClearTaskFiltersCommand一次，RefreshCommand为零；错误时重试调用RefreshCommand。既有存档测试覆盖加载、空、失败、旧数据及可执行重试。

SDK8.0.423 Release最终组合17/17、0失败、0 skip；source validation、XAML26/26及WPF静态37 XAML/0 error/26既有warnings/179 info。首次加锚点检查时独立夹具未应用Shell的有限视口且未等待滚动调度，偏移为零导致2项失败；补生产ApplyResponsiveLayout及调度后保留非零起点、精确偏移及选择断言，最终17/17通过。失败、定向修正和最终TRX与SHA见tests/。

生产视觉未改，Render QA继承同生产源码的[U05-07/08证据](../U05-07-08/BASELINE.md)：双主题394图/0 PROBLEM；本批没有重新生成截图或执行完整安装链。两项代码完成待验收，真实Playnite、物理键盘/IME、UIA客户端、实体DPI及新提交双SDK CI待验；最终中文根入口完整链和独立scripts/build.cmd仍为整轮门禁。

磁盘快照在本批测试后、归档前记录于disk-before.json，阶段开始前未采集独立快照，不能称作开始前空间；disk-after.json记录归档后空间。本批仅阶段TRX及脱敏证据，没有截图/包副本；保留失败诊断和最终测试，删除清单为空。其他任务TRX/TestResults、存档和配置未动。
