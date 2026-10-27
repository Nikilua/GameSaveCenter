# U02-01～04：主题层次与语义状态色

## 去重与本批增量

- U02-01/02 复用 U00-01 的生产页双主题基线与 AdaptiveThemePaletteContrastGuard。现有守卫已分别量测 `Surface vs Background`、`ControlFill vs Surface` 和 `ControlStroke vs Surface`；本批的 SDK8 回归在 Light、Dark、FollowPlaynite 三种模式均通过。渲染审查未发现需要重调页面/卡片/输入层的证据，因此没有为完成任务而改写既有表面色。
- U02-03 概览“最近访问”普通行图标不再使用强调色；任务状态不借用强调色。当前选中行保留选框的强调色。
- U02-04 概览任务行覆盖成功、警告、错误、进行中、排队、取消六种状态；图标形状、前景色、底色和结果标签使用同一语义。普通语义文字与页面及自身浅色底的目标对比度为 4.5:1。高对比模式使用系统控件底色和系统语义前景色。
- 共享 Palette 将语义资源应用放在 Demo 核心资源之后，防止主题资源覆盖自适应状态色；同时同步旧别名资源，保留 AcrylicReferenceControls 使用契约。

## 验证

- SDK 8.0.423，Release 定向测试 15/15 通过、0 失败、0 跳过。覆盖 Light/Dark 概览任务行、Adaptive Palette Light/Dark/FollowPlaynite 对比守卫、R10 最近访问键盘可达性，以及状态字形映射。
- `python scripts/validate-source.py` 通过；`scripts/check-xaml.ps1` 检查 25 个 XAML 文件通过；WPF 静态审查扫描 31 个 XAML 文件，0 error、26 warning、179 info（与现有基线一致）。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/round5-20261028-u02-01-04` 退出码 0，报告 `0 PROBLEM`。覆盖 Light/Dark、1040×700 至 3840×2160 窗口样本、生产 Shell 页和侧栏转场探针。摘录与完整报告见 `render-qa-report.txt`；审计输出保留在 `artifacts/ui-qa/round5-20261028-u02-01-04`。
- 代表性概览渲染见 `screenshots/Overview-1100x720.png`。离屏报告的 DPI 为逻辑 DIP 1.00，不代表物理 DPI 或 Playnite 宿主。
- 第一次定向编译因新增测试文件缺少 `GameSaveCenter.Playnite.ViewModels` 命名空间引用而失败；补入引用后同一测试批通过。原失败属于测试夹具编译错误，保留说明，不计通过或产品回归。

## 验收边界

本地代码与离屏视觉行为完成，状态为“代码完成待验收”。真实 Playnite 宿主、UIA/键盘/IME、物理 DPI 与 GitHub 当前提交工作流尚待验；不据本地通过推断远端 CI 通过。完整根入口安装启动链及独立 `scripts/build.cmd` 属整轮最终门禁。没有触及存档恢复、媒体删除或云端写入。
