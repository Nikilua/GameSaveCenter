# R09-02 图标语义统一

## 结论

R09-02 已满足当前可控实现条件。现有 `ThemeAwareIcon` 继续使用共享 `Geometry` 和 `Path`，本阶段没有引入字体图标、PNG 背景或新的图标体系；补齐了备份、恢复、上传、校验、归类、忽略六类动作的语义映射，并在生产动作按钮上统一复用。

共享资源位于 `Themes/GscIconPack.xaml`：

- `GscIconActionBackup` / `GscIconActionRestore`：备份与恢复。
- `GscIconActionUpload` / `GscIconActionVerify`：上传与只读校验。
- `GscIconActionCategorize` / `GscIconActionIgnore`：媒体归类与忽略。
- `GscActionIcon` 基于 `GscLineIcon`，统一动作图标为 `16x16`，保持主题前景色继承和 6 DIP 文本间距。

生产使用已覆盖 Dashboard/Overview/Save Center 的备份、校验、恢复，Maintenance 的远端校验/上传，以及 Media Center 的归类、忽略、恢复。命令、绑定、`CommandParameter`、危险恢复样式和禁用/安全提示未改变；只把按钮内容改为“矢量图标 + 原有中文文案”。

## 行为验证

- 新增 `R09IconSemanticBehaviorTests`：从当前 checkout 的 XAML 文件流加载图标字典，验证六个语义资源都是非空、可测量的 `Geometry`，并验证 `GscActionIcon` 统一为 `16x16`。
- 同一夹具解析生产 XAML 中的真实 `ThemeAwareIcon.IconData` 用法，逐项确认六个语义在对应页面出现；再把实际 `ThemeAwareIcon` 放进禁用的 WPF Button，验证 `IconData`、可见性及 `ActualWidth/ActualHeight` 仍保留，覆盖“禁用状态仍可辨识”的非负例。
- 提交 `c7c7ae0d` 的精确隔离验证：XAML `24/24`；解决方案 Release `0 warning / 0 error`，目标包含 Playnite `net462`；R09-02 新夹具 `2/2`，相邻图标/动作回归 `3/3`，合计 `5/5`；`python scripts/validate-source.py`、`git diff --check` 均通过。

## 边界

- 本阶段只验证 Geometry 资源解析、实际 WPF 控件布局和禁用 Button 的语义数据保留；没有把离屏逻辑 DIP 或自动测试写成真实 Playnite 呈现、物理 DPI、跨屏、读屏/UIA、输入法、presented frame、ETW 或宿主性能通过。
- Demo 原始目录不可用，继续以恢复的生产资源基线作为视觉依据；没有创建新的设计体系。
- 夹具使用合成控件和隔离资源读取，不读取或修改真实存档、媒体、云端或用户 Playnite；package-host 身份仍为 `not-provided`。
- 用户提供的 `DEV-INSTALL-008` 日志显示其 main checkout 在全量 Playnite.Tests 阶段失败（73/718）；该日志发生在本续作分支精确构建之外，且包含旧/不同 checkout 的测试身份与 WPF testhost 条件，不能用来宣称本阶段的真实安装通过。后续发布收口需在合入后的单一 checkout 中重新执行完整安装器。

## 下一步

R09-02 当前可控条件已满足；下一可执行小批量为 R09-03“一像素描边”，先核对 100/125/150/175/200% 的现有边框、分隔线和选中指示资源，再选择可模拟与必须真实宿主验收的范围。
