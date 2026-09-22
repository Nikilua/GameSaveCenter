# R14-08 来源规则试运行定向复核

日期：2026-09-24
工作区：`D:\\workplace\\github\\GameSaveCenter`
分支：`codex/ui-finesse-round2`
当前复核代码身份：`6d426f6e`
实现提交：`89141528`（本批没有生产代码变更）

## 结论

R14-08 已按当前实现和受控行为/视觉证据收口为“已满足，待环境验证”。试运行继续复用现有来源规则匹配和媒体扩展名能力，只读枚举合成/隔离目录，不保存规则、不创建媒体记录、不移动文件；本批没有新建第二套服务或设计体系。

## 受控行为验证

- Worker `MediaSyncServiceTests 20/20` 通过，其中 `MediaSourcePreviewIsBoundedReadOnlyAndExplainsMatchesAndExclusions` 覆盖完整扫描的命中/排除原因、大小/样本、扫描数量预算截断，并确认来源表为空、样本文件仍在原位置。
- 同一 Worker 夹具验证 `MaxScannedEntries=1` 返回 `Partial`、`ScanTruncated=true`、扫描数 `1` 和有限样本；生产服务仍将样本上限限制在 `1–200`、扫描上限 `1–5000`、时间限制 `100–5000ms`，超时/取消沿现有 token 链收敛。
- Playnite `R14SourceRulePreviewTests 1/1` 通过，覆盖试运行命令/摘要/样本绑定、预算字段、IPC dispatcher、linked cancellation、`ScanTruncated` 和不调用 `AddMediaSourceAsync` 的源码契约。
- 当前隔离 Release 构建通过：XAML `24/24`、solution `0 error/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning，Playnite 目标 `net462`。
- `python scripts/validate-source.py`、`scripts/check-xaml.ps1 -ProjectRoot .` 和 `git diff --check` 在本批文档更新后通过。

## 受控视觉证据

- 人工检查既有 clean RenderHarness 截图 `artifacts/ui-qa-r13-r14-clean-20260922/Media-1600x900-tab3.png`：来源规则页显示“试运行（只读）”摘要，合成结果为扫描 `4` 项、命中 `2`、排除 `2`，样本行显示命中/排除标签、文件名、模式原因、大小和隔离路径；列表在有限视口内保留滚动条。
- 该 RenderHarness 报告全局仍因 Overview/Settings/Task/Save 基线以 `FAILED` 结束；截图是 offscreen 合成数据，不证明真实来源权限拒绝、超大目录耗时、Playnite 宿主最终呈现、物理 DPI/跨屏、UIA/IME、presented frame、ETW 或宿主性能。

## 安全与未验边界

- 只使用合成文件、fake/隔离 SQLite/目录；未修改真实存档、媒体、用户云端或发送诊断。试运行不写来源规则、不入库、不移动文件。
- 真实 Playnite 加载来源页、权限拒绝目录、超大目录耗时、真实视频/图片宿主呈现、DPI/跨屏、UIA/IME、presented frame、ETW 和宿主性能仍未验；没有绕过被拒绝的系统跟踪权限。Demo 原目录不可用，沿用恢复生产基线。

下一可执行小批量：推进 `R15-01 任务阶段可读`，先核对现有 TaskCoordinator/任务事件阶段 DTO 和未知阶段/终态错误边界。
