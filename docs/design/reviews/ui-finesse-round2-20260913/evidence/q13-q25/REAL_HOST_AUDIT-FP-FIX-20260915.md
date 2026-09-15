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
- 随后进行了一次仅用于隔离根因的启动诊断，额外传入 `--no-sandbox --disable-gpu`；Playnite 仍在主窗口出现前退出，并再次写入同类 CEF `platform_channel` 拒绝访问。该参数没有进入生产审计脚本或插件配置，因此不能被解释为生产宿主修复。
- `scripts/real-host-audit.ps1` 现会保留启动进程句柄；若进程在主窗口前退出且尾部日志命中该类 CEF/启动标记，则在输出根目录写入 `host-startup-blocker.json`。报告明确包含 `VisualEvidenceCaptured=false`、`CountsAsVisualPass=false`，且不写入 `gates/`，不会污染视觉通过或阻断门禁统计。
- 用户扩展目录中的旧 Worker PID `23304` 未被结束；审计使用了独立 Worker 数据目录和唯一 Pipe/EventPipe。当前机器仍只有 `DISPLAY1`，Q24-03 物理跨屏保持外部阻塞。

本次结构化改动的源契约回归 `DiagnosticsEvidenceSourceTests.RenderAndHostAuditEntriesDeclareEvidenceBoundariesAndTimingFields` 为 `1/1`，PowerShell AST 解析通过；既有 `cc63523` Release 全量门禁结果仍作为本轮生产代码基线，不因该次宿主失败被改写。

## `37f92f7` 当前提交成功重跑

- `scripts/real-host-audit.ps1 -Configuration Release` 使用全新隔离 UserData、隔离扩展目录、隔离 Worker 数据目录和唯一 Pipe/EventPipe 成功完成构建、测试、打包、安装和真实 Playnite 宿主捕获；安装与程序集身份均为 `0.6.73+37f92f7f107880bb5a33f61c82eee11fe1344874`。
- 产物为 `artifacts/ui-host-audit-round2-fp-final6-20260915`。`summary.json` 明确记录 `EmbeddedDashboardCaptured=true`、`EmbeddedSettingsCaptured=true`、两者 `Origin=EmbeddedPlaynite`、`ProductionVisualSourceOfTruthAvailable=true`、`HighGateCount=0`。
- `capture-manifest.json` 记录 29 个 Dashboard 视口、2 个完整滚动面和 1 个 Settings 视口，均为 `EmbeddedPlaynite`、`CompletenessValidated=true`；输出目录另保留 4 张滚动回放端点图，总计 34 张 PNG。Dashboard 视口为 `1298.67×900 DIP`、150% DPI、1948×1350 像素；Settings 为 `1278×762 DIP`、150% DPI、1917×1143 像素。
- 资源与视觉树快照记录 Settings 的 `GscPrimaryTextBrush=#FFF2F4F8`、`GscSecondaryTextBrush=#FFB9C0CC`、`SettingsScroller` 和 `TabChrome`；Dashboard/Settings metadata 均绑定当前完整 SHA。代表性的 Overview、维护诊断概览和 Settings PNG 已人工检查，中文标题、深色前景、主按钮、空态、表头和设置表单保持可读。
- `gates/` 仍只有 `overflow-classification.json`，其 `AuditFalsePositive=[]`；该分类文件按 `f1ea52a` 规则不计阻断门禁。审计结束后使用 Playnite 官方 `--shutdown --userdatadir` 关闭本轮隔离宿主；用户扩展目录旧 Worker PID `23304` 保持未触碰。
- 本次成功证据仍不覆盖第二物理屏、低于 560 DIP 的短窗、IME/读屏、真实鼠标/键盘组合输入、ETW 呈现帧和真实宿主耐久；`runner-metadata.json` 明确记录当前仅有 `DISPLAY1`，Q24-03 继续外部阻塞。

## 当前文档 HEAD 的非空隔离库重跑边界

- 为补强 Q20 首页数据态，曾在当前文档 HEAD `0fb597e42d59f3e0b1371276731437178dda03e1` 上，把隔离基线中的 `library`（包含 3 个游戏）注入到不带旧运行态标记的全新 UserData，再执行 `scripts/real-host-audit.ps1 -Configuration Release`。
- 第一次整目录复制错误地带入旧 `safestart.flag`、CEF 缓存和日志，窗口标题为 `Startup Error`；按官方 `--shutdown --userdatadir` 关闭后未结束用户 Worker `23304`。第二次只复制成功启动过的干净配置、Theme/ExtensionsData 和非空 `library`，仍在主窗口前触发 `cef.log` 的 `mojo platform_channel ... Access denied (0x5)`，产物为 `artifacts/ui-host-audit-round2-fp-data2-20260915/host-startup-blocker.json`。
- 两次重跑均没有 `summary.json`、Embedded Dashboard 或 Embedded Settings；结构化报告固定为 `VisualEvidenceCaptured=false`、`CountsAsVisualPass=false`，不升级 Q20-01～Q20-08，也不覆盖 `37f92f7` 已有的空库 Embedded 证据。非空数据的真实首页视觉仍需宿主能在当前机器正常启动后再取证。

## 交付解释

Q00 的共享前景修复、审计计数修复、宿主启动阻断结构化和自动回归已完成；`37f92f7` 已补齐当前提交的真实 Embedded Dashboard/Settings、SHA、资源和门禁证据，Q00-07 可据此完成证据身份收口，Q00-08 及 Q25-07/Q25-08 继续保留交付基线与当前重跑双重证据。此前 `cc63523` 的 CEF 早退记录仍作为失败边界保留，不覆盖本次成功捕获，也不把历史 `HighGateCount=1` 旧摘要改称当前通过。
