# R16-07 配置导入预览（2026-09-20）

## 结论

R16-07 已实现，状态为“已实现，待环境验证”。代码提交为 `451195ad`，已推送到 `origin/codex/ui-finesse-round2`。

## 现状核对与改动

- 既有能力为 `GameSaveCenterSettings.ExportPortableJson` / `ImportPortableJson`、架构版本 `1`、缺失路径报告和 detached 校验；导出已把 `DeviceId` 清空，当前设置 DTO 没有凭据字段（既有测试同时锁定 `RclonePassword` 不进入 JSON）。
- 新增 `PreviewPortableJson`：在 detached settings 上解析并校验版本和值范围，不修改当前草稿；预览列出架构版本、兼容性、实际发生变化的可写字段、未知根/Settings 字段和安全说明。未知字段继续由 Newtonsoft 忽略，并明确写入“不破坏当前配置”。设备身份不列为可覆盖字段。
- 新增 `ApplyPortableJson`：UI 先用原生 MessageBox 展示预览并要求“是/否”确认，确认后才复制到当前草稿；应用前保留序列化快照，复制或后续报告失败时恢复原配置。取消确认、版本不支持和值无效均不写入。
- 保留原 `ImportPortableJson` API，改为 `PreviewPortableJson` 后走同一应用/回滚路径；没有新建配置源、凭据存储、真实存档/媒体/云端写入或新的 UI 体系。

## 证据

- 最终 HEAD `451195ad` 绑定源码身份的定向测试 `15/15`：R16-07 行为与源码测试、既有 `PortableSettingsTests` 全通过；覆盖预览不改草稿、确认应用、未知字段、旧架构/坏值负例、凭据与设备身份导出保护。
- 最终提交完整 Release/net462 solution 构建：`0 errors / 2 warnings`；两条均为既有 `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs:664` 的 CS8602。输出在隔离 `.tmp/r16-07-final-solution`。
- `python scripts/validate-source.py` 通过；`scripts/check-xaml.ps1` 为 XAML `24/24`；`git diff --check` 通过；WPF 静态检查为 `0 errors / 28 warnings / 162 info`。输出仅作本地验证，不纳入 Git。

## 未验证边界

- 未运行真实 Playnite/package-host 的文件选择器、MessageBox、设置保存/取消和最终呈现；未验浅深主题、DPI/跨屏、UIA/读屏、IME、RenderHarness presented frame、ETW 或宿主性能。Demo 原目录不可用，继续沿用已恢复的生产基线。
- 证据使用合成 JSON、detached settings 和隔离构建目录；没有读取或写入真实用户配置、存档、媒体、云端或系统诊断，也没有把离屏/代理结果写成真实呈现或物理跨屏。
- main 工作树的用户改动、`src.zip` 和未跟踪对话框文件未碰，当前不合并 main；`.tmp/r16-07-*` 仅为本阶段可再生输出，文档提交前清理。

下一可执行任务：`R16-08 保存冲突处理`，先核对设置编辑基线、后台更新通知和字段级合并/拒绝边界，禁止静默覆盖用户最后修改。
