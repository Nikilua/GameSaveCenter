# GameSaveCenter UI Typography + Responsive Closure Report

## 基线

- 开始：`9eb12e1`（`main`，与 `origin/main` 同步）
- 分支：`main`
- 工作区：提交前仅包含本轮 UI/测试/文档改动

## 字体根因与修复

### 根因

- `GscButtonBase`、`GscWpfUiButton`、`GscWpfUiToggleSwitch`、Dashboard 根元素/隐式 TextBlock、Overview 胶囊等硬编码 `Segoe UI Variable Text, Segoe UI`，没有中文 fallback。
- 中文进入 WPF 系统级 CJK fallback，与英文 Segoe 系列字重/字形不一致。
- 通用按钮默认 `SemiBold`，中文系统字体无同名 Semibold 时被映射得更重。

### 修复

- `DesignTokens.xaml` 新增 `GscUiFontFamily`（`Segoe UI Variable Text, Segoe UI, Microsoft YaHei UI`）与 `GscCodeFontFamily`（`Consolas, Microsoft YaHei UI`）。
- Dashboard 根、隐式 TextBlock、本地 `GscButtonBase`、WPF-UI Button/ToggleSwitch、Overview 胶囊全部改用 `{DynamicResource GscUiFontFamily}`。
- `Segoe MDL2 Assets` 图标字体全部保留；Maintenance 诊断 `Consolas` 等宽字体保留。
- 通用按钮默认 `FontWeight` 从 `SemiBold` 改为 `Medium`；`GscWpfUiPrimaryButton` 单独保留 `SemiBold`。

## UI 修复

- Settings Compact/Narrow：新增 `SettingsIntroDescription`，Compact 隐藏长说明；Narrow/矮窗隐藏副标题与保存提示；icon/Padding/MinHeight/Margin 收紧。render-qa 探针显示 760×560 下正文 viewport 300 DIP，880×560 下 285 DIP。
- Save Compare Narrow：主比较区 `MinHeight=240`、`MaxHeight=max(300, height*0.52)`；保留策略预览保持 Auto 行。render-qa 1040×700 下 `SaveCompareMainScrollViewer` viewport 234 DIP（原约 35-53 DIP），retention 246 DIP。
- Compact Inspector：Save/Trainer/Media/Task 五个“查看详情”按钮全部移入表格下方独立 `Grid.Row=1` 操作行，不再覆盖状态文字。
- Media 待归类底栏：`WrapPanel` 增加 `Margin="12,10,12,0"`，与表格内容左边缘对齐，窄窗继续换行。

## 未修改项

- Overview 仅替换字体硬编码，未改页面结构、信息顺序或卡片布局。
- 未改 Worker、IPC、Ludusavi、备份/恢复、媒体同步、任务执行等业务逻辑。
- 未改 DataGrid Row Style、`Item` scroll unit、虚拟化或共享滚动骨架。

## 测试结果

- Release 构建：0 warning / 0 error。
- Core：59/59。
- Worker：190/190。
- Playnite：266/266（含 3 条新增回归）。
- `validate-source.py`：通过。
- `check-xaml.ps1`：13 个 XAML 全部通过。
- 技能静态审查：0 errors。
- render-qa：11 档窗口 + 56 主题场景 + 7 Resize 全部通过。
- UI Audit：0 HIGH / 0 MEDIUM / 0 失败路由；运行时警告 65（INFO/EXPECTED 级）。
- 最终 Audit ZIP：`artifacts/GameSaveCenter-ui-audit.zip`。

## 风险

- 真实 Playnite 宿主、第三方主题、DPI 真机、连续缩放仍为 `MANUAL QA REQUIRED`；离屏 render/audit 不代表真实宿主人工验收。
