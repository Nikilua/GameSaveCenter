# U00-01 — Round 5 视觉基线与去重

日期：2026-10-27。RenderHarness 报告时间：11:26:17（Asia/Shanghai）。

## 基线身份与边界

- 当前仓库提交：`f5cddfe960668fbb06653d553c0a2bf316ce137d`；可执行产品源码仍为 `90bf040567f013f3a8d9ed8667028a4edb2ec524`，本次提交相对产品候选只有计划文档。
- SDK：`global.json` 精确锁定的 `8.0.423`，从当前用户 GameSaveCenter SDK 缓存选择。
- `scripts/render-qa.ps1 -Configuration Release -Output .tmp/ui-polish-round5-u00-01`：RenderHarness Release 编译 0 warning / 0 error；进程退出码 0；报告 `render-qa OK`、0 个 `PROBLEM`。
- 受控窗口：1040×700 和 1366×768 DIP；Light/Dark；离屏 logical DPI 1.00。使用 RenderHarness 合成数据和隔离 WPF 窗口，不代表真实 Playnite 页面、物理 DPI、系统输入或屏幕实际帧。
- 页面源码来自生产 `Views/` 与生产设置视图。截图未覆盖真实宿主外壳及其当前游戏选框；选框和滚动条系统保持保护项。
- 当前 checkout 中没有 `GameSaveCenter.AcrylicFork/.../DesignShellView.xaml`、`Design/Pages`，也没有 `GameSaveCenter.WpfUiDemo.v3.1`。因此本次不制作虚构的 Demo 对照；实施沿用当前生产资源，Demo 源重新出现后再按 Demo-first 比较。
- 设置截图来自 RenderHarness 的“存在 3 项校验错误”状态样本，不是干净默认设置样本。错误提示属于真实验证语义，不能当作装饰移除；本轮未拍干净配置或真实 Playnite 设置。

截图清单及 SHA-256 在 [screenshots.json](screenshots.json)。下列截图均使用相同隔离数据和受控主题；每项观察优先引用 1040×700，并以 1366×768 检查宽度变化。

## 逐页观察（候选差异，不等同 20 个已确认缺陷）

这些条目只记录截图或几何报告可以直接支持的现象，并区分需要进一步验证的候选；下一批先处理共享根因，不把测试样本限制误判成产品缺陷。

### 存档中心

| 排序 | 级别 | 具体观察 | 证据与核验边界 |
| --- | --- | --- | --- |
| 1 | P1 | 历史筛选、扫描/校验、加载/列设置、视图管理分成 3–4 行；在 1040 窗口中操作区占据主表上方的大块首屏。 | [Light 1040](Save-light-1040x700.png)、[Dark 1040](Save-dark-1040x700.png)、[Light 1366](Save-light-1366x768.png)。先理清主次与分组，不删任何动作。 |
| 2 | P1 | 历史表在窄视口启用自身横向滚动，默认首屏右侧状态/备注列无法同时看到。 | 同上；Render QA 报告 `SaveHistoryGrid` 725×274 DIP、4/4 可读行，内部横/纵滚动均存在。保留滚动体系，检查默认列优先级和完整查看路径。 |
| 3 | P2 | 选中的页签“历史版本”和内容卡标题“历史版本”连续重复。 | [Light 1040](Save-light-1040x700.png)、[Dark 1366](Save-dark-1366x768.png)。核对统一页标题层级，不改变页面导航。 |
| 4 | P2 | 宽布局的版本详情面板有独立纵向滚动，详情操作与下方来源/可用性信息分布在长面板中。 | [Light 1366](Save-light-1366x768.png)、[Dark 1366](Save-dark-1366x768.png)。确认键盘滚动、焦点和选中锚点后再调整。 |
| 5 | P2 | 1040 时详情入口移到历史标题旁，1366 时详情常驻右栏；同一操作在断点两侧位置变化明显。 | [Light 1040](Save-light-1040x700.png)、[Light 1366](Save-light-1366x768.png)。验证切换/返回时选择和焦点保持。 |

### 任务中心

| 排序 | 级别 | 具体观察 | 证据与核验边界 |
| --- | --- | --- | --- |
| 1 | P1 | 1040 下队列操作挤成两行，“查看任务详情”独占下一行，重置按钮标题被省略。 | [Light 1040](Task-light-1040x700.png)、[Dark 1040](Task-dark-1040x700.png)。不移除任务操作；比较共享工具栏分级/溢出方案。 |
| 2 | P1 | 主表宽度超过 1040 窗口可见宽度，状态、进度和详情列需横向滚动查看。 | 同上；Render QA 报告 `TaskGrid` 711×274 DIP、4/4 可读行且启用横向滚动。横向滚动保持可达。 |
| 3 | P2 | 顶部搜索/状态筛选与“概览与更多筛选”折叠条形成两层固定区，压缩队列首屏高度。 | [Light 1040](Task-light-1040x700.png)、[Light 1366](Task-light-1366x768.png)。不取消筛选；核对首次扫描效率和紧凑高度。 |
| 4 | P2 | 长游戏名在行内显示省略，例如 `Baldur's Gat...`。 | [Light 1040](Task-light-1040x700.png)、[Light 1366](Task-light-1366x768.png)。截图看不到 Tooltip 是否可由键盘发现，故列为待验而非确认丢失内容。 |
| 5 | P2 | 宽布局详情栏内部滚动并分成任务摘要、完整详情、当前阶段等卡片；右侧操作图标组较依赖图标识别。 | [Light 1366](Task-light-1366x768.png)、[Dark 1366](Task-dark-1366x768.png)。下一批静态核查 UIA 名称和焦点；不改真实任务/取消语义。 |

### 媒体中心

| 排序 | 级别 | 具体观察 | 证据与核验边界 |
| --- | --- | --- | --- |
| 1 | P1 | 1040 下批量操作和归类目标分为两行；表格起点随工具栏换行向下移动。 | [Light 1040](Media-light-1040x700.png)、[Dark 1040](Media-dark-1040x700.png)；对比 [Light 1366](Media-light-1366x768.png)。不得隐藏当前筛选、多选数量或必要动作。 |
| 2 | P1 | 页面顶部先展示四个同权重统计值，再展示四个工作区页签和批量操作；主要待办表格的首屏优先级靠后。 | 同上。Render QA 在 1040 下报告媒体表 681×267 DIP、4/4 可读行；是否减少统计占位需用同样数据复核。 |
| 3 | P2 | “文件/原因”列在窄布局接近可用右边界，长原因文案首屏容易贴边。 | [Light 1040](Media-light-1040x700.png)、[Dark 1040](Media-dark-1040x700.png)。检查 Tooltip、Inspector 和键盘查看全文路径后再判定是否裁切。 |
| 4 | P2 | 过滤预设、应用/保存/重命名/删除控件位于列表之后，需要页面纵向滚动才能到达。 | [Light 1040](Media-light-1040x700.png)、[Light 1366](Media-light-1366x768.png)。属于当前滚动布局观察，不在本轮撤掉页面或表格滚动。 |
| 5 | P2 | 统计带、页签带、批量处理卡片各自使用独立表面，层级边界较多。 | [Light 1366](Media-light-1366x768.png)、[Dark 1366](Media-dark-1366x768.png)。视觉精简时保持统计语义、待归类计数和危险操作隔离。 |

### 设置

| 排序 | 级别 | 具体观察 | 证据与核验边界 |
| --- | --- | --- | --- |
| 1 | P1（待复核） | 当前截图处于全局校验错误状态；顶部错误/修复区在两种窗口中都占较大垂直空间。 | [Light 1040](Settings-light-1040x700.png)、[Dark 1366](Settings-dark-1366x768.png)。错误语义必须保留；未拍无错误配置，尚不能判断此占用是否只发生在错误态。 |
| 2 | P1（待复核） | 1040 样本的设置内容滚动区只有 81 DIP viewport，而内容 extent 为 849 DIP（所选设置页随数据不同）。 | 同一报告的 `SettingsScroller` 几何及 [Light 1040](Settings-light-1040x700.png)。由错误区/恢复区等真实状态造成的预算需与干净设置样本对照。 |
| 3 | P2 | 左侧分类列表与右侧设置内容各自有独立滚动条，短窗中需要识别当前滚动目标。 | [Light 1040](Settings-light-1040x700.png)、[Dark 1040](Settings-dark-1040x700.png)。保持键盘滚动和分类定位。 |
| 4 | P2 | 1040 首屏仅露出前两类设置；后续分类依赖左栏滚动，右侧内容也需单独滚动。 | 同上。需结合真实导航焦点轨迹核对，不把未显示等同不可达。 |
| 5 | P2（样本文案） | 标题使用 `SETTINGS` 英文小标签加中文产品标题；与其下的中文分类标题形成中英层级混排。 | [Light 1366](Settings-light-1366x768.png)、[Dark 1366](Settings-dark-1366x768.png)。这是排版统一候选，不应为了统一移除产品名或设置上下文。 |

设置页面所有发现仍受“校验错误”样本影响；本轮没有创建、保存或修改任何用户配置。下一次设置视觉核对应使用隔离且有效的默认配置，以及单独的错误状态图。

## 旧能力映射与去重

| Round 5 范围 | 已有能力 / 旧证据 | 本轮保留的增量边界 |
| --- | --- | --- |
| U06-07 高窗/窄窗比例 | `90bf0405` 已移除 `SaveHistoryGrid` 的 560 DIP 上限；2560×1440、3840×2160 Light/Dark 及收缩恢复通过，见[历史表格专项](../../../ui-history-table-4k-20261026/README.md)。当前 Render QA 在 1040×700 仍报告历史表 4/4 可读行。 | 不重复修复已关闭的高窗留白；U06-07 仅继续审查任务/媒体等其他页面及真实宿主待验边界。 |
| U06-01/U03 密度、列与表格 | Round4 S01-01 至 S01-07 已有视图配置、密度、可选列、保存/重置预览和断点记忆实现，状态及真实宿主边界见[Round4账本](../../../ui-finesse-round4-20261018/ROUND4_PROGRESS.md)。 | 不另造列管理/视图持久化；仅基于当前图像审查共享层级和易读性。 |
| U07/U08 媒体能力 | Round4 S04 已有媒体比较、筛选结果导航、意见撤销与缩略图预算；S02-04 通知上下文跳转已受控验证。 | 本轮只做视觉和交互反馈，不触发真实媒体删除、存档恢复或云写入。 |
| U01/U02/U04 字体、主题、动效 | 生产已存在 `Typography.xaml`、`DesignTokens.xaml`、`MotionTokens.xaml`、`WpfUiProduction.xaml`、`Redesign.xaml`；Round3 已有字体/主题/动画回归。 | 不造第二套令牌或动画服务；仅修复本轮截图与测量支持的缺口，并保留降低动态效果路径。 |
| Demo-first | 计划、CURRENT_STATE 和门禁均优先 Demo；本 checkout 没有 AcrylicFork Design 或 WpfUiDemo v3.1 源目录。 | 缺源期间以可验证生产资源为基线；不得称本轮已和 Demo 对齐。 |

## 基线质量与性能边界

- 技能静态审查扫描 31 个 XAML：0 errors、26 warnings、179 info。任务/存档页没有该次输出的 StackPanel 警告；媒体页有 8 条“可滚动控件可能位于 StackPanel”的 warning，须逐个结合 Grid 几何复查；维护/概览等既有 warning 不自动纳入本轮修复。
- `render-qa` 已采集侧栏开合代理数据：2000×1100 离屏场景下，单次开合 p95 frame-gap 30.6 ms、最大 143.7 ms；快速往返 p95 22.1 ms、最大 125.5 ms；关闭动画时最终状态在 109.4 ms 到达。该探针不等于 Playnite 屏幕呈现帧，且此次无 `PROBLEM`。完整输入/切页/详情/列表滚动性能基线仍留给 U00-06，不能用这组侧栏数据替代。
- U00-06 未完成：本轮没有测量 Playnite 内真实输入延迟、呈现帧、ETW 或宿主性能；不从离屏 RenderHarness 推断 60 FPS。

## 归档与清理

Render QA 临时输出共 395 个文件（394 PNG + 1,141,754 字节报告），合计 53,094,868 字节。为本基线留存 16 张同条件截图，共 1,474,206 字节；每张 SHA-256 见 [screenshots.json](screenshots.json)。完整 RenderHarness 报告未复制进 Git，`render-qa-report.txt` SHA-256 和输出/清理空间记录见 [run-metadata.json](run-metadata.json)。

清理前已核对本阶段 `.tmp` 输出的规范化路径、reparse point、活动 RenderHarness 进程、文件占用和已归档截图 SHA-256。随后执行 `Remove-Item -LiteralPath ... -Recurse -Force` 被执行策略拒绝；目录仍保留，未尝试其他删除方式。清理状态记为待执行，不把未发生的空间回收写成完成。其他 `.tmp`、artifacts、bin/obj、Round 3 用户 TRX/TestResults 全部保留。清单、拒绝结果和前后空间观察见 [cleanup.json](cleanup.json)。

U00-01 的离屏视觉基线和历史任务去重已完成；临时输出清理待执行，真实 Playnite、有效默认设置截图、物理 DPI、UIA/IME 和所呈现动画帧不在本证据范围。远端 CI 终态核对后再更新账本状态。
