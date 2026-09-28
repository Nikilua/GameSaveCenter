# R01-06 受控审计与 RenderHarness 归档（2026-09-28）

## 身份与结果

- 源码身份：`59afd8f4bd81039e1e8150d0ccc35e608b357113`；隔离 Release solution 与 RenderHarness 均为 `0 warning / 0 error`，XAML `24/24`。
- 完整 RenderHarness：Light/Dark，11 个逻辑窗口尺寸；373 个输出文件（312 张 PNG）；`WorkingTreeClean=True`、`render-qa OK`。Settings normal/dirty/invalid 三态分别显示已保存、未保存更改和错误摘要。
- UI Audit：168 个运行时快照、103 条警告、0 HIGH、0 MEDIUM、0 Fidelity 警告、0 失败路由；索引 20 项，引用/身份/样本/未验边界均 `20/20`，`validate-ui-evidence-index.ps1` 通过。
- UI Audit 默认只记短 SHA；本轮通过 `GSC_UI_AUDIT_COMMIT` 传入完整当前 SHA 后生成索引。归档 metadata 的 `OutputRoot`/`ZipPath` 已换成说明文字，没有保留机器临时目录。
- `UiAuditSourceTests 6/6`、`RepositoryIdentityTests 2/2`、`BuildIdentityTests 3/3` 的 TRX 保留在本目录。

## 文件

- `AUDIT_SUMMARY.md`、`EVIDENCE_INDEX.md`、`LAYOUT_REPORT.md`、`UI_FIDELITY_MATRIX.md`、manifest 与 route map：当前受控审计结果。
- `render-qa-report.txt`：完整多尺寸/双主题 RenderHarness 文本报告。
- `screenshots/`：Overview、Save History、Media Inbox、Task、Maintenance 五张 1040×700 synthetic WPF 页面图。截图不包含真实用户库或真实宿主状态。
- `audit-metadata.json`：当前审计信息；临时输出目录与 ZIP 绝对路径已去除。
- `UI_MANIFEST.json` / `UI_ROUTE_MAP.json`：对机器仓库根和合成长文件路径作了可识别占位替换；布局/来源数据未改。
- 三份 TRX 保留结果与方法列表；仅将本机 `.tmp` test assembly `codeBase` 改为相对程序集名。

生成的其余 88 MB 原始布局/visual-tree/PNG/zip 只在本机 `.tmp/` 临时存在，没有整包复制进 Git。完整原始集可从当前源码重建。

## 边界

截图和几何来自合成 DTO、离屏 WPF 与 logical DIP 1.0；没有启动 Playnite 或加载当前用户安装包，不代表真实宿主呈现、物理 DPI/跨屏、UIA/读屏、IME、DWM presented frame、ETW 或宿主性能。审计剩余 103 条警告仍需按原始报告分类阅读，不能因 HIGH/MEDIUM 为 0 宣称无风险。
