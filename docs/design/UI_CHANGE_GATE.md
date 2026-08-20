# UI 变更强制门禁

更新时间：2026-08-11

> 当前方向声明（2026-08-17）：本文件是质量、兼容性和验证门禁，不冻结页面信息架构或控件实现。用户已授权整页 UI 重构，因此页面布局、导航、控件类型、共享模板和滚动模型可以重做；下方门禁用于验证新方案仍然可用、可访问、可扩展且兼容 Playnite。

## 最新视觉优先级：Demo-first（2026-08-20）

本门禁的视觉基准已明确改为 Demo 优先：

- 主要基准为 `GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design/DesignShellView.xaml`、`Pages/*.xaml`、`DesignTokens.xaml`、`DesignColorsLight.xaml`、`DesignColorsDark.xaml` 和 `DesignControls.xaml`。
- Demo 与生产旧页面、UiLab、历史交接记录或 `wpf-apple-desktop-ui` 的通用视觉建议冲突时，以 Demo 的整体结构和视觉关系为准。
- `wpf-apple-desktop-ui` 只负责质量、可访问性、绑定、虚拟化、主题/DPI 和 Playnite 兼容性检查，不得以其 Apple-inspired 偏好阻止或改变 Demo 迁移。
- 当前游戏选框、生产滚动条系统、真实命令/绑定、异步错误/取消/安全语义和真实运行时数据仍按项目目标保留。

所有新增或修改 GameSaveCenter UI 的提交，必须先阅读：

- `.codex/skills/wpf-apple-desktop-ui/SKILL.md`（随仓库提交；本机同时安装于 `%USERPROFILE%\.codex\skills\wpf-apple-desktop-ui`），仅作为质量检查依据；并按任务需要读取其 `references/` 中对应的响应式、Playnite、控件、可访问性或回归清单文档
- `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md`
- `docs/design/APPLE_UI_GUIDE.md`
- `docs/KNOWN_ISSUES.md` 中与主题、DPI、动画和安装有关的条目

## 不可违反的原则

1. 真实 WPF/XAML 实现，不引入 HTML、WebView、Electron、Avalonia、WinUI 3 或 MAUI 外壳。
2. 不改插件 ID，不重写备份、恢复、Ludusavi、媒体、云端和任务编排业务层。
3. Playnite 内嵌页面不伪造 macOS 红黄绿窗口按钮，不修改宿主窗口 Chrome。
4. Apple-inspired 指的是层级、留白、材质、圆角、克制状态色和自然动效，不是复制 Apple 资产。
5. 不分散硬编码主题色。可切换颜色必须使用共享设计令牌和 `DynamicResource`。
6. 普通内容卡片以清晰、近不透明表面为主；毛玻璃只用于侧栏、浮层、提示和少量结构层。
7. 文字、列表行和大型滚动区域禁止应用 `BlurEffect`。
8. 动画优先使用 `Opacity`、`TranslateTransform`、`ScaleTransform` 和颜色；避免动画 Width/Height/Margin。
9. Style 中禁止放置会被代码动画修改的共享 `Freezable` Transform；动画对象必须是元素独占且可变的实例。
10. 文字必须启用像素对齐与高 DPI 友好渲染；禁止通过整个 TextBlock 的 `Opacity` 制造次级文本。
11. 主题至少支持“跟随 Playnite、浅色、深色”，并在高对比度、关闭透明和关闭动画时安全降级。
12. 状态不能只依赖颜色，必须同时有文本、图标或状态点。
13. 所有 Loading、Succeeded、Failed、Cancelled 状态必须来自真实任务，不用 `Task.Delay` 模拟业务成功。
14. 新增按钮必须有正常、Hover、Pressed、Disabled、Keyboard Focus 状态。
15. 大型游戏库列表必须保持虚拟化，不能让动画和容器模板关闭虚拟化。
16. 修改 XAML 后必须运行 `scripts/validate-source.py`；Windows 上还必须通过 `GameSaveCenter-Run.cmd` 的真实构建、安装和版本核验。
17. 修改 WPF/XAML 后应运行技能静态审查 `python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .`，处理其中 error 级发现；warning/info 按上下文说明保留原因。
18. 页面级布局/响应式改动必须运行 `scripts/render-qa.ps1`（C 盘满时先设 `TEMP/TMP` 到仓库 `.tmp/qa-temp`），报告不得出现 `PROBLEM`。

## 常用显示尺寸与内容可见性门禁

“适配 4K”不能推导出“适配 1080p”。WPF 按 DIP 而不是物理像素排版：4K 在 150%/200% DPI 下的逻辑工作区可能与 2K 或 1080p 相同甚至更小；1080p 窗口化后也可能只剩 1280–1600 DIP 宽。因此每轮页面改动都必须同时考虑以下三类屏幕及其窗口化状态：

- 1080p：全屏与常用窗口化，重点检查约 1280–1920 DIP 宽、640–900 DIP 高。
- 2K/1440p：100%/125%/150% DPI 下的全屏与窗口化，重点检查约 1280–2560 DIP 宽。
- 4K/2160p：150%/175%/200% DPI 下的全屏与窗口化，重点检查缩放后约 1280–2560 DIP 宽。

仓库内 `GameSaveCenter.WpfUiDemo.v3.1` 的 `MainWindow` 将 `1040×700 DIP` 声明为常用最小窗口；生产外壳和页面响应式断点必须把这个尺寸作为明确回归点。低于该尺寸可以进入紧凑/窄布局，但不能把 1040×700 这种 Demo 已支持的正常窗口提前当作极端小窗口处理。

验收重点不是让极端小窗口保持完整，而是保证常用窗口化、最大化和跨 DPI 状态下：

- 首屏下方的真实内容不会被固定页脚、宿主边界或相邻卡片遮住；看不到的内容必须有明确可操作的纵向滚动通道。
- 页面级滚动只承载有限测量的页面内容；DataGrid/ListBox 等大型列表仍必须拥有自己的有限视口、内部滚动和虚拟化，不能用无限测量的外层 ScrollViewer 掩盖布局问题。
- 以表格/列表为主的页面，在常用高度下至少保留约四行可读内容；高度不足时优先压缩次级信息或让页面滚动，不能把主表挤成一行后宣称已适配。
- 需要堆叠的卡片、按钮和 Inspector 必须自然换行或转为单列，不能依赖裁切、负 Margin、全页面缩放或隐藏滚动条来“适配”。
- 复核应以逻辑尺寸为准，至少覆盖 980/1040/1100/1280/1366/1600 DIP 宽和 640/720/900/1080 DIP 高，并记录尚未实际运行的 Playnite/主题/DPI 验证。

## UI 提交检查表

- [ ] 颜色、圆角、间距和阴影是否来自共享令牌或局部可解释资源？
- [ ] 跟随 Playnite、强制浅色、强制深色下是否均可读？
- [ ] 100%、125%、150%、175%、200% DPI 是否无裁剪、发虚和错位？
- [ ] 透明效果关闭、高对比度开启时是否仍完整可用？
- [ ] 空状态、错误状态、禁用原因和下一步操作是否清楚？
- [ ] 长名称是否使用省略和 Tooltip？
- [ ] 任务和日志是否按需加载，不在 UI 线程读取大文件？
- [ ] 1080p、2K、4K 的全屏与常用窗口化逻辑尺寸下，首屏内容是否可见，底部内容是否有明确滚动通道？
- [ ] 主表/主列表是否仍保留约四行可读高度、内部滚动和虚拟化，未被上方 Auto 行挤成一行？
- [ ] 动画是否只改变渲染属性，且关闭页面后停止计时器和订阅？
- [ ] 是否保留真实命令、错误传播、取消和业务状态？
- [ ] `extension.yaml`、程序集版本、安装包名和已安装 DLL 是否一致？
- [ ] 是否运行了 `python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .` 并处理 error 级发现？
- [ ] 是否运行了 `scripts/render-qa.ps1`（7 页面 × 5 种常用窗口）且报告无 `PROBLEM`？

违反本门禁的 UI 改动不应进入 `main`。
