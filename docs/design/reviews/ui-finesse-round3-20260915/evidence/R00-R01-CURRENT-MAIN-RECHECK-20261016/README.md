# R00/R01 当前 main freshness 复核

日期：2026-10-16。只读复核 source HEAD `5157d8719110514aec857f2c0397ec4943b926e4`。

```powershell
.\scripts\check-ui-evidence-freshness.ps1 -HeadCommit HEAD
```

结果：`14 FRESH / 0 STALE`、`0 source-path matches`，baseline 中的 14 条记录共比较 `2,883` 条路径；package identity 为 `not-provided`。这表示目前没有登记的源码路径变化要求重跑这些历史证据，不表示已重建安装包、安装 Playnite 插件或验证真实宿主。

本次仅检查 evidence baseline 与 Git 源码差异；没有修改 baseline，也没有重跑对应测试、视觉或宿主场景。R00/R01 的既有已满足结论与真实 OS IME、Playnite 输入、UIA/读屏、物理 DPI/跨屏、最终呈现帧、ETW/宿主性能边界保持不变。R11-08 的新 push Actions 尚未取得 run ID/终态；该 freshness 结果不替代其 SDK8/SDK10.0.401 CI 门禁。

## 既有复核记录（较早 source HEAD）

较早的只读检查在源码 `b1e7b652c938463f2f50ac74cb8157c4899daa1d` 上为 `14 fresh / 0 stale / 0 登记源路径命中`，比较 2,848 条变更路径。机器可读结果保留在 [旧 freshness JSON](freshness-20261016.json)，SHA-256 `3A56927FDA0DE261F9AB5E3981F5530ECF835D779B111AADD3505DC976978058`。该 JSON 的 source HEAD 与本次 `5157d871` 不同，不能当成本次命令的原始输出。

截至已归档的 2026-10-15 audit（源码 `88b8eb57`），审计摘要仍为 `168 snapshots / 116 warnings / 0 HIGH / 13 MEDIUM / 0 Fidelity / 0 failed routes`，索引 `20/20`。13 条 MEDIUM 工具栏自然换行风险保持打开；`ROUND3_PROGRESS.md` 中 R00-07 当前值已按该审计更正为 `116 warnings / 13 MEDIUM`。R00/R01 任务状态未因 freshness 检查改变。

Demo 原目录在当前工作区不可用，视觉基准继续使用恢复的生产基线。真实 Playnite、UIA/Narrator、OS 输入/IME、物理 DPI/跨屏、presented frame、ETW 与宿主性能仍待验。先前记录的 R12-04 作为下一项已被当前 R11-08 续跑安排覆盖，不是当前下一任务。
