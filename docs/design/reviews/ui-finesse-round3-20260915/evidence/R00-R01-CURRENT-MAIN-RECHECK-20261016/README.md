# R00/R01 当前 main 证据复核（2026-10-16）

## Freshness

在已推送源码 `b1e7b652c938463f2f50ac74cb8157c4899daa1d` 上运行：

```powershell
./scripts/check-ui-evidence-freshness.ps1 -HeadCommit b1e7b652c938463f2f50ac74cb8157c4899daa1d -OutputPath docs/design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-CURRENT-MAIN-RECHECK-20261016/freshness-20261016.json -AsJson
```

- 14 条 R00/R01 登记证据均为 fresh，0 stale；没有条目命中登记源路径，`needsRerun=false`、`reinstallRequired=false`。
- 比较区间含 2,848 个变更路径，包含源码、文档和历史证据，不是 docs-only。R00/R01 登记源路径均未变化，因此本次仅做 freshness 检查，没有重复构建、行为测试或重跑视觉审计。
- freshness 脚本的 `packageIdentityStatus=not-provided`（本次未传 `PackageCommit`）。这不否认 #1032 的 SDK8 打包步骤成功；freshness 报告本身不关联该包身份，也不代表安装或真实 Playnite 宿主验收。
- 机器可读结果：[freshness-20261016.json](freshness-20261016.json)，SHA-256 `3A56927FDA0DE261F9AB5E3981F5530ECF835D779B111AADD3505DC976978058`。

## 当前审计摘要校正

截至最新已归档 audit（2026-10-15，源码检查点 `88b8eb57`），结果仍为 `168 snapshots / 116 warnings / 0 HIGH / 13 MEDIUM / 0 Fidelity / 0 failed routes`，索引 `20/20`。13 条 MEDIUM 工具栏自然换行风险保持打开，没有因本次 freshness 检查而清除。

`ROUND3_PROGRESS.md` 的 R00-07 当前表格曾沿用 2026-09-30 的 `110 warnings / 7 MEDIUM`；现按最新审计更正为 `116 warnings / 13 MEDIUM`，并链接到 2026-10-15 的 R01-06 审计证据。R00/R01 现有任务状态不变；此处是账本和证据入口校正，不是新的页面行为或视觉验收。

## 边界与后续

Demo 原目录在当前工作区不可用，视觉基准继续使用恢复的生产基线。真实 Playnite、UIA/Narrator、OS 输入/IME、物理 DPI/跨屏、presented frame、ETW 与宿主性能仍待验。下一项按已授权顺序复用现有恢复前保护服务/DTO，使用 fake 服务和隔离目录复核 R12-04；不得读写真实存档。
