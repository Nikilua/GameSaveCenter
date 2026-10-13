# R01-03 / R01-06 当前 main 审计证据

## 运行身份与结果

- 源码/审计 identity：`599d2fd38dd5c9af523a0890a33e73fc78ac0bfa`；本机隔离 SDK `8.0.423`，Playnite `net462`、RenderHarness/test `net472`。RenderHarness Release build `0 warning / 0 error`。
- RenderHarness 离屏 audit：`168` runtime snapshots、`116` warnings、`0 HIGH`、`13 MEDIUM`、`0 Fidelity`、`0 failed routes`。13 条 MEDIUM 是动作栏在窄布局中自然换行/垂直扩展；风险保留并列在 [`AUDIT_SUMMARY.md`](AUDIT_SUMMARY.md)。没有将它们归为失败或隐藏。
- E01–E20 索引校验 `20/20` 引用、身份、样本和边界通过。`UiAuditSourceTests` `6/6` 通过；三类受影响 Playnite 测试与摘要见[测试 TRX](../tests/R00-R01-shared-audit-tests-599d2fd.trx)。
- `audit-metadata.json` 保留了生成时间、完整 commit SHA、窗口尺寸与逻辑 DPI；仅将机器专属输出根目录改成仓库相对说明并去掉未生成 ZIP 的路径。

## 精简归档

本目录保留完整摘要、证据索引、manifest、route map、布局/可见矩阵、脱敏运行文本，以及五张代表图。完整 168 个场景的临时截图和 visual-tree dump 已用于生成摘要与索引，没有复制进证据包，以控制仓库体积；验证产物可按当前 main 与 README 中的脚本命令重建。

| 图 | 条件 | 文件 |
| --- | --- | --- |
| Shell | `1040×700 DIP` | [PNG](screenshots/Shell-1040x700.png) |
| Shell | `1440×900 DIP` | [PNG](screenshots/Shell-1440x900.png) |
| Save / History | `1440×900 DIP` | [PNG](screenshots/Save-1440x900.png) |
| Media / Inbox | `1440×900 DIP` | [PNG](screenshots/Media-1440x900.png) |
| Task Center | `1440×900 DIP` | [PNG](screenshots/Task-1440x900.png) |

## 可复核方法与边界

```powershell
$env:GSC_SOURCE_ROOT = (Get-Location).Path
$env:GSC_UI_AUDIT_COMMIT = (git rev-parse HEAD).Trim()
dotnet run --project tests/GameSaveCenter.RenderHarness/GameSaveCenter.RenderHarness.csproj -c Release -- audit .tmp/r01-06-audit
./scripts/validate-ui-evidence-index.ps1 -AuditRoot .tmp/r01-06-audit
```

图像和几何来自受控 WPF synthetic data、离屏逻辑 DIP `1.0`，不是 Playnite 最终呈现或物理 DPI。真实宿主、OS UIA/IME、物理跨屏、presented frame、ETW 与宿主性能仍待各自验证。没有执行备份、恢复、删除、迁移、云端或设置保存操作。
