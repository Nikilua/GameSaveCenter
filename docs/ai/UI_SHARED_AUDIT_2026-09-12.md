# U12-00 共享 UI 审计与 Demo 基准定位

日期：2026-09-12。范围：生产 WPF 表现层的资源入口、控件复用和 Demo-first 可追溯性；不修改 ViewModel、命令、Worker 或数据契约。

## 结论

当前实现已有一套可用的共享设计系统。本阶段不新增平行的 Glass、Button、字体、图标或动效资源；后续页面任务必须补齐既有资源的使用点，并保持当前资源合并顺序。唯一发现的页面级硬编码颜色位于媒体预览图片的遮罩渐变（透明黑到 35% 黑），它是图片可读性局部效果，不承担主题色，不应迁移为全局色令牌。

## 运行时资源链

| 层级 | 入口 | 职责 |
| --- | --- | --- |
| 基础令牌 | `Themes/DesignTokens.xaml` | 主题颜色、间距、材质与基础资源；先合并 Typography、MotionTokens |
| 字体 | `Themes/Typography.xaml` | `GscUiFontFamily`、显示/数值/代码字体及 22/16/14/12 级字阶 |
| 动效 | `Themes/MotionTokens.xaml` 与 `Infrastructure/GscMotion.cs` | 统一 Fast/Press/Normal/Slow 时长、缓动及高对比度/关闭动画降级 |
| 控件 | `Themes/WpfUiProduction.xaml`、`Themes/ButtonStyles.xaml`、`Themes/Redesign.xaml` | 输入、选择、表格、按钮、Tab、卡片与虚拟化契约 |
| 图标 | `Themes/GscIconPack.xaml`、`Themes/GscIconButtonPack.xaml` | 主题感知线稿；低歧义上下文操作才使用纯图标按钮 |
| 页面 | `Views/*.xaml`、`Settings/GameSaveCenterSettingsView.xaml` | 只合并共享字典并应用命名 Style；保留真实绑定、命令、自动化名称和虚拟化 |

生产工作区页面均以 `DesignTokens → WpfUiProduction → Redesign` 顺序合并。壳和设置另合并 `AcrylicProductionResources.xaml` / Acrylic 参考资源，以维持既有 Playnite 壳兼容性；后续不要改变顺序或从页面复制控件模板。

## 审计清单

| 项目 | 证据 | 处置 |
| --- | --- | --- |
| 主题色硬编码 | 页面与设置层仅发现 `MediaCenterView.xaml` 图片预览遮罩的 `#00000000`/`#59000000` | 保留：局部图片遮罩，不参与主题；其余主题色走 DynamicResource |
| 字体漂移 | 业务页根级使用 `GscUiFontFamily`；代码/诊断使用 `GscCodeFontFamily` | 保留 `Segoe MDL2 Assets`：Dashboard 遗留宿主 glyph，后续图标迁移时单独替换，不与 U12-00 混改 |
| 默认控件泄漏 | TextBox、ComboBox、DataGrid 与业务按钮由 `GscWpfUi*`、`GscRedesignWorkspaceDataGrid` 或 `GscWpfUiButton` 覆盖 | 后续新增控件不得使用无样式宿主默认模板 |
| 重复 Border/Glass | 卡片、表格框、Hero、弹层分别收口在 `Redesign.xaml` 与 `DesignTokens.xaml` | 不新增第二套 GlassSurface；按语义选已有资源 |
| 动效漂移 | 共享令牌与 `GscMotion` 已覆盖生产动效路径 | 后续只复用 token，不给行/列表添加动画 |
| 表格性能 | `GscRedesignWorkspaceDataGrid` 契约启用 Recycling、项滚动和行/列虚拟化 | 页面改动不得关闭虚拟化或用无限高度容器包住表格 |

## Demo-first 基准的可追溯来源

原始 `GameSaveCenter.AcrylicFork/.../Design/` 文件目前不在工作树，不能作为运行时或构建依赖恢复。当前可追溯的基准锚点为：

- 提交 [`3c12b2c`](../../.git)（`恢复 AcrylicFork 页面基线`，2026-08-18），记录将 AcrylicFork 的页面结构恢复到生产层的变更。
- `tests/GameSaveCenter.Playnite.Tests/RestoredAcrylicForkBaselineTests.cs`：锁定首页、侧栏、存档、媒体、任务、表格、反馈与设置入口的结构/行为边界。
- `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md`：明确当前 Demo-first 优先级；若以后找回原始资产，应先核对提交来源与该测试，再更新本节，不能以另一套风格替代。

这使后续工作可在不依赖丢失目录的前提下追踪已恢复的 Demo 结构，同时避免伪造未验证的原始资源。

## 后续实施约束

U12-02～U12-09 先修改页面布局/状态，再按需补共享样式；若同类缺陷跨页出现，优先修共享模板。每项仍必须运行 XAML、源码、静态 WPF 审查、构建/测试与 RenderHarness，并把真实 Playnite、DPI 和宿主主题写为实际验证或手工边界。
