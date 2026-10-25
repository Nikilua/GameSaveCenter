# 外机 Q06 合成悬停被重置

用户新日志run `20261025T040600037Z-ec604b91`，安装run `20261025T040558033Z-0b75781f`，SDK8.0.423、PowerShell7.6.7、Windows10.0.26200。Release编译0警告/0错误；第29/145类Q06在Light hover采样为mouseOver=False、opacity0，1299项1280通过/18跳过/1失败，未发布安装。日志未附HEAD，不能猜测具体提交。失败摘录与原附件SHA见verification.json。

这次输入状态已被覆盖，单纯等待动画不足。测试现有界等待时持续检查合成hover，只在状态被替换时重新施加输入；同时每个主题都在Dispatcher.Render注入一次明确的输入重置，要求该回调执行并恢复到原有opacity阈值后才采样。生产样式、动画、原状态断言与阈值均保留。

SDK8.0.423隔离Release0警告/0错误，五次独立进程5/5通过；双主题十个hover报告均mouseOver=True且opacity>0.9。每轮仍覆盖完整十状态。源码校验通过。五轮TRX/console/状态报告及一组hover截图已归档。新候选须重新运行默认根入口完整链；[576b689b的完整成功](../standard-576b689b/README.md)只能证明之前的代码，不能覆盖本次新修正。
