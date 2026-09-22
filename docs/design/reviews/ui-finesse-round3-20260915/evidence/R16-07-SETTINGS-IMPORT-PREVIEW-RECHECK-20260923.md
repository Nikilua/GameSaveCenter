# R16-07 配置导入预览定向复核

日期：2026-09-23

复核提交：`01e83d8a`（`codex/ui-finesse-round2`，D 盘工作区）

## 结论

R16-07 的受控实现条件已满足，账本状态校正为“已满足，待环境验证”。本批没有新增生产代码，复用 `451195ad` 的 portable settings 架构 v1、detached 预览、确认后应用、设备身份保护和异常回滚路径。

## 实际复测

- `R16SettingsImportPreviewBehaviorTests`：`4/4` 通过，覆盖预览不修改草稿、确认后应用、未知字段忽略、旧架构/坏值拒绝、失败不改变原设置，以及导出不包含凭据和设备身份。
- `R16SettingsImportPreviewSourceTests`：`1/1` 通过，确认 UI 顺序为 `PreviewPortableJson → ConfirmSettingsImport → ApplyPortableJson`，没有绕过预览直接写入；确认文案包含未知字段忽略、凭据和设备身份保护。
- 既有 `PortableSettingsTests`：`10/10` 通过，覆盖非敏感字段往返、旧包默认值、无效值/未知 schema/枚举/大小上限、缺失路径报告不建目录、取消草稿和设备身份边界；定向合计 `15/15`。
- 当前提交在 `.tmp/r16-07-recheck-20260923` 完成隔离 Release solution 构建：`0 errors / 2 existing warnings`，两条均为 `MediaCenterView.xaml.cs:706` 的既有 `CS8602`；源码程序集身份与 `01e83d8a` 一致。
- `python scripts/validate-source.py`、XAML 结构检查 `24/24`、`git diff --check` 通过；WPF 技能静态检查为 `0 errors / 28 warnings / 162 info`，未见本批新增错误。

## 保留能力与边界

- 继续保留 Playnite 保存/取消、命令/Binding、错误/取消/恢复保护、路径校验、游戏选框、滚动条、有限列表性能和 net462；导入只改当前草稿，确认取消、版本不支持、坏值和异常不会写入真实配置。
- 测试使用合成 JSON、detached settings 和隔离目录，没有读取或写入真实用户配置、存档、媒体、云端或诊断数据；Demo 原目录不可用，沿用已恢复生产基线。
- 未验真实 Playnite/package-host 文件选择器、MessageBox、保存/取消、最终浅深主题、UIA/读屏/IME、DPI/跨屏、RenderHarness presented frame、ETW 或宿主性能；自动化结果不替代这些边界。

下一可执行任务：`R16-08 保存冲突处理`。先核对 Playnite 编辑基线、后台更新通知和字段级合并/拒绝边界，禁止静默覆盖用户最后修改；当前隔离复测目录待本阶段提交后清理。
