# Host Style Dependency Report

维护时间：2026-08-15

## 目的

列出 GameSaveCenter 生产 XAML 中所有 `BasedOn="{StaticResource {x:Type X}}"` 的样式，区分：

- A. Intentional host integration：刻意跟随 Playnite。
- B. Plugin-owned visual：外观必须由 GSC 自己控制，不能依赖宿主 implicit style 提供视觉。
- C. Accessibility / behavior inheritance only：只继承 framework 行为，不继承宿主视觉。

离屏 RenderHarness 与真实 Playnite 的 `{x:Type X}` implicit style 解析可能不同；真实宿主审计会通过 `style-fingerprints.json` 对比 effective 值确认是否有视觉泄漏。

## 当前扫描结果（BasedOn `{x:Type ...}`）

| 文件 | 样式 | 目标 | 分类 |
|---|---|---|---|
| Themes/WpfUiProduction.xaml | `GscDataGridColumnHeaderStyle` | DataGridColumnHeader | B/C |
| Themes/WpfUiProduction.xaml | `GscWpfUiTextBox` | TextBox | B/C |
| Themes/WpfUiProduction.xaml | `GscWpfUiComboBox` | ComboBox | B/C |
| Views/TrainerCenterView.xaml | implicit ListBox | ListBox | B/C |
| Views/MediaCenterView.xaml | `MediaInboxColumnHeaderStyle` | DataGridColumnHeader | B/C |
| Views/MediaCenterView.xaml | `MediaDataGrid` | DataGrid | B/C |
| Views/SaveCenterView.xaml | `SaveDataGrid` | DataGrid | B/C |
| Views/SaveCenterView.xaml | `SaveFirstHeader` / `SaveLastHeader` | DataGridColumnHeader | B/C |
| Views/MaintenanceView.xaml | `MaintenanceDataGrid` | DataGrid | B/C |
| Views/TaskCenterView.xaml | `TaskDataGrid` | DataGrid | B/C |
| Views/OverviewView.xaml | implicit ProgressBar（两处） | ProgressBar | B/C |
| Views/DashboardView.xaml | `GscFirst/Last/OnlyColumnHeader` | DataGridColumnHeader | B/C |
| Views/DashboardView.xaml | `GscLeft/StretchDataGridCell` | DataGridCell | B/C |

## 判断

- 当前没有任何样式声明为“刻意跟随 Playnite 主题”（A 类）。
- 其余均为 B/C 类：这些样式大多设置 `OverridesDefaultStyle=True` 并提供自包含 Template/Setter，目的是只从 framework base 继承行为；真实宿主中如果 `{x:Type X}` 解析到 Playnite theme style，仍可能把 font/margin/trigger 等 Setter 带入 BasedOn chain。
- 结论：不能静态认定“没有泄漏”，必须用 Tier B 的 `style-fingerprints.json` 与离屏 fingerprint 对比确认。

## 验证方式

```powershell
.\scripts\real-host-audit.ps1
```

然后对比：

```text
artifacts\ui-audit\...（Tier A 离屏）
artifacts\ui-host-audit\style-fingerprints.json（Tier B 真实宿主）
```

重点核查 `GscDataGridColumnHeaderStyle`、`GscWpfUiTextBox`、`GscWpfUiComboBox` 的 FontFamily/FontSize/FontWeight/Background/BorderBrush/Effect 是否被宿主改写。
