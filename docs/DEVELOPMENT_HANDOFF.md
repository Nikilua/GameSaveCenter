# GameSaveCenter 当前交接

更新：2026-10-07；SDK工程阶段、远端终态与当前源码/DLL身份见[SDK阶段证据](ai/evidence/close-sdk-20261007/README.md)，版本0.6.73。当前结果见 [CURRENT_STATE](ai/CURRENT_STATE.md)，持久规则见 [PROJECT_MEMORY](ai/PROJECT_MEMORY.md)。旧记录完整保留在 [历史交接](DEVELOPMENT_HANDOFF_HISTORY_THROUGH_20260930.md)。

## 本轮完成

- global.json与CI固定SDK8.0.423/disable，C#12.0，构建和独立runner验证实际选择；PS5.1/7真实缺失旧SDK失败、原latestMajor对照返回8.0.423通过。Release0/0、Core125/Worker357/source470通过；完整WPF、runner与包门禁仍须按最新终态签收，SDK/CI任务不关闭。
- run37565955357/37566483862已取得终态与always上传产物；SDK10侧栏22/22，默认偏好ClientAreaAnimation=false；实际Q14错误态36→37 DIP失败。STA测量确认错误边框参与布局，独立双主题654 DIP边距失败和620/660正例均已取得。
- 补修诊断器多个PATH同名程序拼接导致启动失败；两个不同原生程序夹具在PS5.1/7验证首路径优先级，finally恢复PATH。前后诊断保留；无业务/UI修复或版本变化。

## 前阶段已记录事实（历史）

- CI诊断已接入实际native退出码、独立console/TRX、SDK/源码hash/DLL身份及always上传；PS5.1/7受控失败、stderr、缺失程序和脱敏自测通过。生产默认系统动效策略保留，实例内测试输入覆盖8组合；侧栏22/22，source470/18，Core125/125。
- 本轮WPF前23类通过，第24类Q14先在错误态高度36→37 DIP失败，后89类未执行；没有运行到原654 DIP边距断言。CI恢复仍未验，不关闭CLOSE-CI-01。新增受影响R00-03/R01-01/R01-04须跟原5项一并补证。
- CLOSE-IPC-01已完成：独立管道、握手与失败清理先排除夹具干扰，再受控复现生产取消dispose的IOException/EOF竞争；修复取消分类，保留RequestId、有限重放和未知提交语义。
- 同一最终DLL专项20轮200/200；source470通过/18原有跳过/零失败，Core125/Worker357通过，SDK8 Release 0 warning/error、XAML24/24。源码/DLL SHA、MVID、复现和负例见 [本轮证据](ai/evidence/close-ipc-20261005/README.md)。提交前dirty构建身份明确分账。
- 全量WPF在第24类Q14失败：654 DIP单行预设控件残留20 DIP bottom margin，同DLL独立复核再失败。前23类通过，后89类未执行；新增CLOSE-WRAP-01，整体门禁不通过。
- 取得最新CI run37329446647完整失败步骤：实际SDK10.0.401，两条ProductionShellChromeSourceTests活动动画断言失败，与IPC分开；旧run日志403仍是历史事实。

## 下一项如何开始

1. fetch main并核对工作树；活动任务以 [AUTONOMOUS_BACKLOG](AUTONOMOUS_BACKLOG.md) 为唯一入口。
   两条前阶段CI终态及诊断已经归档，不再重复查询；SDK阶段隔离build身份是c7f2a368加dirty源码hash，后续提交需重建。freshness6/8，baseline未改；不要复用旧DLL假称当前候选。
2. 继续CLOSE-CI-01/CLOSE-SDK-01：当前源码runner须实际选择8.0.423；Q14修复后完整CI编译/测试/渲染/包门禁通过才关闭。 SDK10侧栏实际通过，不再视为当前失败，不改OS偏好或skip。
3. CLOSE-WRAP-01：先核对共享TextBox错误态BorderThickness的测量变化，同时解决原654 DIP单行残留20 DIP margin；保留620→660→620往返和Light/Dark，不放宽几何容差。修好后补原5项及R00-03/R01-01/R01-04。
4. ENV-001门禁满足后做Media Inbox同PID/DLL/MVID滚动几何；用户Q06/R08仍需原始日志与运行身份；最后执行同候选恢复/回滚/Undo与发布矩阵。

## 验证边界

本轮没有启动真实Playnite、改变Windows动画偏好、安装到用户Extensions或触及真实存档/媒体/云端；没有验证物理DPI/读屏/最终呈现帧。SDK8本机与SDK10 CI分别记录。测试期间只终止已核对命令行归属本轮隔离目录的测试进程；被终止的运行不记为通过。

每独立阶段编译、测试、同步文档并commit/push当前main。历史d57a613f包的clean构建/IPC/结构及六个程序集身份已记录，未安装；不能签收当前源码。当前freshness6/8，baseline未改，完整SDK/CI门禁仍受Q14阻塞。本地包不冒称稳定发布，当前阶段中间产物归档后按仓库规则清理。
