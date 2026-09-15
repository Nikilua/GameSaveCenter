# Q00 / 真实宿主审计门禁假阳性修复（2026-09-15）

本记录只说明审计门禁统计修复和最新重跑边界，不把未捕获 Dashboard 的宿主启动尝试写成视觉通过。

## 统计修复

- `f1ea52a` 在 `RealHostUiAuditService.CountBlockingGateFiles` 中只统计 `gates/*.json` 的阻断门禁，并排除 `overflow-classification.json` 诊断分类报告。
- `UiAuditTruthfulnessTests.OverflowClassificationReportIsNotCountedAsBlockingGate` 在当前 Release 二进制下 `1/1` 通过。
- 既有隔离真实宿主产物 `artifacts/ui-host-audit-round2-ipc-final-20260915` 的实际 `gates/` 目录只有 `overflow-classification.json`：原始 `summary.json` 是旧统计逻辑生成的 `HighGateCount=1`，按修复后的规则重新盘点为 `BlockingGateFiles=0`。原始摘要不被篡改，也不被改写成当前提交的宿主新通过证据。

## 当前提交自动门禁

提交 `cc63523902c225e96c0d8ef9dcd058448fabaa22`（`cc63523`）包含上述修复及 Playnite WPF 测试串行隔离。Release 门禁结果：XAML `24/24`、构建 `0 warning/0 error`、Core `83/83`、Worker `310/311`（1 skip）、Playnite `495/558`（63 skip），失败 `0`；定向假阳性回归 `1/1` 通过。

## 最新真实宿主重跑边界

- `scripts/real-host-audit.ps1 -Configuration Release` 使用全新隔离 UserData 和唯一 IPC 对运行至打包、安装验证及全量测试完成，程序集身份均为 `0.6.73+cc63523...`。
- Playnite 启动阶段在创建主窗口/加载插件前退出；隔离目录的 `playnite.log` 只到 `PlayniteApplication:Application started`，`cef.log` 记录 CEF `mojo platform_channel` `Access denied (0x5)`，没有 `summary.json`、Embedded Dashboard 或 Embedded Settings 捕获。因此该次产物 `artifacts/ui-host-audit-round2-fp-final5-20260915` 只作为宿主启动阻断记录，不进入视觉通过计数。
- 用户扩展目录中的旧 Worker PID `23304` 未被结束；审计使用了独立 Worker 数据目录和唯一 Pipe/EventPipe。当前机器仍只有 `DISPLAY1`，Q24-03 物理跨屏保持外部阻塞。

## 交付解释

Q00 的共享前景修复、审计计数修复和自动回归已完成；Q00-07/Q00-08 及 Q25-07/Q25-08 的状态仍按账本既有证据处理。真实宿主的最新一次重跑未捕获像素，不能覆盖历史 `69e1f84` 已签收的隔离宿主证据，也不能把 `HighGateCount=1` 的旧摘要改称当前提交通过。
