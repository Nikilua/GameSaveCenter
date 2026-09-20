# R17-02 诊断包预览证据

日期：2026-09-20  
状态：已实现，待真实宿主验证  
实现提交：`2b6e9051`（`增加诊断包生成前预览`）

## 现状核对与实现

- 既有 `DiagnosticsPackageService.CreateAsync` 已提供有限 ZIP、2 MiB 上限、日志尾部上限和 `DiagnosticRedactor`；现有生成结果 DTO 已包含文件路径、大小、生成时间和文件数，本批复用这些能力。
- 新增只读 `Preview` IPC，生成前列出 `README/system/worker/dependencies/database 摘要/recent-tasks/health/settings/audit/logs` 类别、任务/审计上限、日志上限和每类脱敏范围。
- 预览明确排除真实存档/备份归档、媒体文件、SQLite 数据库文件及表内容、Rclone 配置/凭据/令牌/密码和自动上传；`database.json` 仅是 schema、大小和只读完整性探针摘要。
- Playnite 维护入口先请求预览并通过现有确认语义显示清单；取消只更新状态，不调用生成。确认后调用原创建 IPC，生成后状态和通知显示完整位置与大小，并沿用原打开路径动作。

## 验证

- Playnite R17 定向测试：`7/7`，包含 R17-01 回归、预览先于生成、确认/取消接线和结果路径/大小行为；源码身份绑定最终提交 `2b6e9051`。
- Worker R17 定向测试：`3/3`，包含实际隔离服务预览：限制值归一化、类别/排除边界、日志可选性、无目录/文件写入，以及既有真实生成 ZIP 的脱敏/上限测试。
- 完整 `GameSaveCenter.sln` Release 构建：`0 errors / 2 warnings`；两条均为既有 `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs:664` nullable warning。
- `validate-source.py`：通过；XAML 结构：`24/24`；`git diff --check`：通过；WPF 静态质量检查：`0 errors / 28 warnings / 162 info`，无新增错误。
- 预览/生成测试使用合成请求、fake/隔离 SQLite 和临时目录；未上传、未修改真实存档/媒体/数据库/用户配置，也未向外发送诊断。

## 未验证边界

- 未启动真实 Playnite/package-host，未把 `ConfirmAsync` 当作真实宿主确认框、最终浅深主题、DPI/UIA/IME、焦点、presented frame、ETW 或宿主性能通过。
- 未在真实用户数据目录生成诊断包，未验证真实路径权限、Explorer 打开动作和日志文件并发写入；保留既有安全上限与异常清理路径。
- Demo 原目录不可用，沿用已恢复生产基线；主分支用户改动、`src.zip` 和未跟踪对话框文件未触碰、未合并。

下一可执行任务：`R17-03 检查进度预算`，先核对健康巡检已有范围、暂停/延后原因、最近成功时间、下轮计划和取消/结束状态。
