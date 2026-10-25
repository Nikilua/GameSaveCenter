# R08 动画反向测试输入与采样边界

推送提交 `7525f8c98da6e8b6794127c0f96731fc209a1f26` 的默认根入口运行 `20261025T084235278Z-4336836a` 在 WPF 第 74/146 类 R08 失败。SDK 8.0.423 Release 编译为 0 警告、0 错误，Core 157/157、Worker 400/400；当时 TRX 共 1437 项、1418 通过、18 跳过、1 失败。Q06 已在第 29 类通过。失败发生在发布/打包/安装之前，Playnite 与 Worker 进程未改变。

失败样本把 `ScaleX=0.998` 的刚开始按压当作中点，之后又要求回弹超过 0.002；该起始值没有足够的动画区间。夹具现在等到 `ScaleX <= 0.99` 才释放。另将 R08 hover 探针从全局 `MouseDevice.ChangeMouseOver` 改为设置 WPF 只读 `IsMouseOverPropertyKey`，并把测试窗口根节点设为不可命中测试。这样仍触发真实生产共享模板的 Hover/Pressed 故事板和反向采样断言，且测试不再受桌面指针同步影响。生产资源、默认动画时长及业务行为均未修改；视觉状态探针不模拟物理鼠标。

候选源码 SDK 8.0.423 / Release 解决方案构建 0 警告、0 错误。`R08MotionReverseBehaviorTests` 五次独立进程各 3/3，共 15/15；所有回合的悬停退出/重入采样、按压释放/重按连续性、中间态方向和最终值断言均通过。

测试源码 SHA-256：`90A71A1C2044BE445654E1BFABD2A333D12ED5094C297A0E49E14AA839975EA8`。候选测试程序集 SHA-256：`E9D74E78A30B3BBE99B7D6088A5C2C12FAE29F2DF568B5C767EF83FD7A533783`。首次根入口失败的脱敏 TRX/console、五轮定向 TRX/console 和身份摘要见本目录；摘要文件为 [verification.json](verification.json)，校验清单为 [SHA256SUMS.txt](SHA256SUMS.txt)。

该证据只覆盖定向 R08 与候选解决方案构建。修复提交后的默认根入口构建、全套测试、发布、打包、安装身份和启动仍待验证。
