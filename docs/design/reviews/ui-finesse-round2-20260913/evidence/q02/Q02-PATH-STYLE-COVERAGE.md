# Q02-06 技术路径样式覆盖证据

采集日期：2026-09-14（Asia/Shanghai）。代码基线：`d97d87b`（统一技术路径显示样式）。本证据只证明源码入口与资源链，不替代真实宿主的截断、Tooltip、复制、IME 或物理 DPI 验收。

## 共享资源

- `Themes/Typography.xaml` 新增 `GscPathText`，基于 `GscTypographyCode`，统一代码字体、单行显示和 `CharacterEllipsis`。
- `Themes/WpfUiProduction.xaml` 新增 `GscWpfUiPathTextBox`，基于 `GscWpfUiTextBox`，只替换技术路径输入的字体，不改变原生选择、光标、校验和编辑契约。

## 已接入入口

- 设置页的 Worker、Ludusavi、存档目录、Rclone、云端目标、媒体目录、镜像目录共 7 个编辑框使用 `GscWpfUiPathTextBox`。
- Trainer 设置的工作目录编辑框使用 `GscWpfUiPathTextBox`；已选版本路径使用 `TrainerDiagnosticPath`，并保留 `EntryPath` Tooltip。
- 存档候选表和候选详情使用 `SavePathText`/`GscPathText`，保留原始 `Path` Tooltip。
- 媒体文件列、收件箱/媒体详情归档路径、媒体来源 `RootPath` 使用 `MediaPathText`；文件列仍将 `OriginalPath` 作为完整 Tooltip。
- 维护页 Worker/Ludusavi/Rclone/数据/媒体目录、归档路径、镜像路径和进程映射 EXE 使用 `MaintenancePathText`，保留各自原值 Tooltip。

## 自动门禁

`TypographyDiagnosticsTests.ProductionColumnsKeepNumericAndPathSemantics` 锁定共享路径样式、7 个设置编辑框、Trainer 工作目录以及 Media/Maintenance/Save 的入口挂接；WPF 定向测试为 7/7，RenderHarness 深浅主题夹具均退出 0。实际宿主复制和 Tooltip 呈现仍保持 Q02-06 未完成边界。
