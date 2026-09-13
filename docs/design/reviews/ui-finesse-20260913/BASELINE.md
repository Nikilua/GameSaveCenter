# UI 精修基线（2026-09-13）

## 基线身份与证据边界

- 生产源码 HEAD：`fd573e4`（`main`，与 `origin/main` 一致）。
- 插件版本：`0.6.73`，版本入口为 `src/GameSaveCenter.Playnite/extension.yaml` 与 `Directory.Build.props`。
- 工作区：仅有用户现有未跟踪文件 `src.zip`，本轮不删除、不提交。
- 静态基线：`scripts/validate-source.py`、`scripts/check-xaml.ps1`（24 个 XAML）和 `git diff --check` 通过。
- WPF 静态基线：0 error、22 warning、175 info。warning 主要是既有有限滚动审查提示；info 主要来自资源字典硬编码颜色和参考控件。
- 离屏基线：`.tmp/ui-prompt-qa-final2-20260913/render-qa-report.txt` 的报告为旧提交 `108ae0f`，不能直接充当本 HEAD 的新证据；P00 另以 `.tmp/ui-finesse-probe-20260913-dark/` 与 `...-light/` 保存双主题校对夹具。
- 真实宿主边界：`artifacts/ui-host-audit-current-20260913/` 有受控窗口证据，但 `MainWindowHandle=0`、未生成 `summary.json`；不得写成真实嵌入 Dashboard、物理 DPI、键盘/滚轮或宿主主题已验收。

## 生产资源生效链

| 类别 | 唯一共享入口 | 真实消费 | 当前覆盖/风险 |
| --- | --- | --- | --- |
| 字体与字阶 | `Themes/Typography.xaml` | `DesignTokens.xaml` 合并后由六页、壳和设置页的 DynamicResource/隐式样式消费 | 优选字体名已声明，但目标机实际字形命中尚未测量；`GscTypographyCaption` 还有 `Opacity=0.65` |
| 颜色、间距、材质 | `Themes/DesignTokens.xaml` + `AdaptiveThemePaletteFactory` | `ApplyRuntimeThemeResources` 写入 Dashboard、Shell 和各 workspace 的资源字典 | 运行时色板会覆盖静态 fallback；透明度合成对比尚未按每种文字语义完整输出 |
| WPF-UI 控件 | `Themes/WpfUiBase.xaml`、`Themes/WpfUiProduction.xaml` | `ui:Button`、Toggle、TextBox、ComboBox、ScrollBar 和共享焦点样式 | 生产按钮/图标按钮已有五态入口；需继续验证按压恢复、禁用 Tooltip 和各主题 |
| 页面/表格 | `Themes/Redesign.xaml` | 六页 DataGrid/ListBox/Tab/Inspector 通过显式样式消费 | 表格保持有限视口、Recycling、行列虚拟化；页面局部字号和紧凑断点仍需同夹具对照 |
| 图标 | `Controls/ThemeAwareIcon.cs`、`Themes/GscIconPack.xaml`、`GscIconButtonPack.xaml` | 壳、状态、页面和图标按钮 | 颜色由主题资源提供；不能退回 emoji/字形图标 |
| 动效 | `Themes/MotionTokens.xaml` + `Infrastructure/GscMotion.cs` | 按钮/Toggle XAML 与 Dashboard/Shell/Overview/设置 C# | 源码确认 Normal 为 220/200ms、Slow 为 300/320ms 双来源；需统一并测试减动效/高对比度 |
| 反馈 | `WorkspaceStatePresenter`、内置 Toast/Dialog/Inspector | 六页和设置的真实状态字段与命令 | 状态、失败、取消、复制诊断和页面滚动已有 U12 基线；本轮只补证据和共享缺陷 |

## 代表性资源追踪

1. `OverviewView` 的 `TextBlock` 默认字体来自 `DesignTokens` → `Typography` 的隐式样式；页面标题显式使用 `GscTypographyPageTitle`/Demo 别名，运行时颜色来自 `GscPrimaryTextBrush`。
2. `AcrylicProductionShellView` 的顶栏刷新按钮使用 `GscIconOnlyToolbarButton`，其基础模板是 `WpfUiProduction.xaml` 中的 `GscIconOnlyButtonBase` → `GscWpfUiButton`，Tooltip/AutomationProperties 保留。
3. `TaskCenterView` 的 `TaskGrid` 使用 `GscRedesignWorkspaceDataGrid`，行高来自 `GscTableRowHeight`（52 DIP），表头来自 `GscTableHeaderHeight`（42 DIP），滚动为有限视口 + Item scrolling。
4. 设置页先调用 `AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources`，再调用 `ApplySettingsMaterialResources`；这是独立设置窗口中主题资源不漂移的实际入口。
5. C# 动效调用集中在 `GscMotion` 和 Dashboard/Shell/Overview/设置的有限根元素；未发现列表行逐项动画或大面积 BlurEffect。

## 初始问题台账

| 分级 | 任务 | 页面/共享入口 | 事实 | 验收条件 |
| --- | --- | --- | --- | --- |
| 可读性 | P01-01/P01-04 | Typography | 字体链存在但实际命中未测；Caption 同时使用 Muted 色和 0.65 透明度 | 固定混排样本有字体命中/缺字报告；Caption 无二次变淡，普通小字按最终合成测量 |
| 可读性 | P02-02 | AdaptiveThemePaletteContrastGuard | SecondaryText 仅按 3.0 检查 | 普通辅助文字、选中、按钮、placeholder、错误、焦点分别记录最终合成比值和阈值 |
| 交互一致性 | P03-01 | WpfUiProduction | 生产共享模板已覆盖状态，但需要五态及卸载/失焦恢复证据 | 不改布局，按压 0.97 后回 1，禁用 Tooltip 和命令次数正确 |
| 体感 | P04-01 | MotionTokens/GscMotion | Normal/Slow 双来源 | C# 与 XAML 由同一可验证策略取值，缺字典和关闭动画安全 |
| 性能 | P00-03/P06/P10 | RenderHarness/真实宿主 | 现有帧探针是代理；真实宿主窗口不可枚举 | 记录环境、原始样本、p50/p95/max；不能把代理数据写成屏幕帧率达标 |

## 固定校对样本

审计夹具使用任务包第 4.1 节的中英、数字、路径、诊断和状态样本，数据为合成内容，不注入真实业务数据：

`存档中心  Save Center`、`媒体归类  Media Inbox`、`备份完成，云端校验失败。可以稍后重试。`、`赛博朋克 2077：往日之影 / Cyberpunk 2077: Phantom Liberty`、`AgjQy  Il1 O0  0123456789  1,024 / 99,999  9% → 100%`、`D:\Games\中文目录\Cyberpunk 2077\一个非常长的备份文件名.zip`、`任务部分完成  FLING_DOWNLOAD_FORBIDDEN  ✓ × ⚠`、`罕见字：𠮷；外文标题：NieR Replicant / Pokémon / 龍が如く`。

## P00 夹具证据

- RenderHarness 命令：`dotnet run --project tests/GameSaveCenter.RenderHarness/GameSaveCenter.RenderHarness.csproj -c Release --no-build -- finesseprobe <output> <dark|light>`。
- Dark 与 Light 均在 1120×980 DIP 完成：`finesse-fixture OK`、4 行 `DataGrid`、22 个按钮、`CaptionOpacity: 1`；截图和报告分别位于 `.tmp/ui-finesse-probe-20260913-dark/`、`.tmp/ui-finesse-probe-20260913-light/`。
- 夹具只挂在 `UiFrameworkProbeView.UserControl.Resources`，不写入 Playnite 全局资源；截图中的混排/路径/错误码/状态数据均为合成校对数据。正常、Hover、Pressed、Disabled、Keyboard Focus 由共享模板覆盖，但 Pressed/Focus 的物理行为仍需行为探针或真实宿主复核。
