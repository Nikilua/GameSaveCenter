# Tooltip 真实指针驱动时序

118ce65c默认根入口run `20261025T061639270Z-46bad3c2`、安装run `20261025T061637300Z-c7a2a240`：SDK8.0.423 Release0/0、Core157/157、Worker400/400、Compact与Q06通过。第55/146类Tooltip快速掠过用例预期0次打开，实际1次；其余两项通过，未发布安装。失败摘要、TRX、console已保留。

测试驱动原使用Background DispatcherTimer安排80ms停留，布局/自动化队列可延后离开动作；MoveCursorTo仅检查父容器IsMouseOver，不能证明已离开其中的按钮。现驱动定时器使用Send优先级，并要求Mouse.DirectlyOver匹配目标点InputHitTest。产品Tooltip时序及350ms延迟未变；快速掠过0打开、停留1打开、切换关闭等断言完整保留。

SDK8.0.423隔离Release0警告/0错误。同标准根入口桌面上下文真实指针测试五轮各3/3，合计15/15、0跳过。逐轮TRX/console已脱敏归档。定向结果仍需新提交的默认根入口完整链验证。
