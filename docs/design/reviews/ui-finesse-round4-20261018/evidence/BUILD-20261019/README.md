# 2026-10-19 当前源码构建失败复核

本次使用仓库锁定的 SDK `8.0.423` 和新的隔离输出目录运行 `scripts/build.ps1`。完整解决方案编译成功（0 警告、0 错误），Core `147/147`、Worker `374` 通过 / `1` 跳过、Playnite 非 WPF 分组 `468/468`。逐类 WPF 测试在 `NumericInputTests` 停止：设置页已经有 12 个数字范围校验，而测试仍断言 10 个。

多出的两项来自通知安静时段的开始/结束分钟字段；两项都使用 `IntegerRangeValidationRule`。将现有断言更新为 12 后，SDK `8.0.423` 下 `NumericInputTests` 为 `12/12`，`R09FocusOutlineBehaviorTests` 为 `2/2`。修正后只定向运行了这两个类，未重跑其余 WPF 类。

## 结果边界

- 首次正式脚本运行在 132 个 WPF 测试类中的第 16 类遇到上述旧计数，完整 WPF 隔离循环未完成。
- 解决方案编译、Core/Worker、Playnite 非 WPF 分组和修正后的两个目标类均有 TRX 记录；结果与二进制身份见 [verification-identity.json](verification-identity.json)。
- 本次没有打包、安装或启动 Playnite，也没有声称验证真实 Playnite 宿主。
- 用户此前提供的 R09 失败诊断目录没有保留。本次当前源码下 R09 `2/2` 通过，不能识别旧运行使用的源码、DLL 或 PDB。
