# Round 6 · V01–V04 存档页基线与设计复核

本阶段先固定真实生产视图的基线，再确定页面改造范围和后续复用的合成数据。仓库当前分支没有计划所列的 `GameSaveCenter.AcrylicFork/.../Design/` Demo 源文件；因此按 Round 6 计划的缺省规则，以当前生产 WPF 资源为事实基线，没有伪造 Demo 参考。

## V01 · 基线截图

来源是 `src/GameSaveCenter.Playnite/Views/SaveCenterView.xaml`，通过 `GameSaveCenter.RenderHarness` 在隔离 WPF 桌面上运行。相同样本时间为 `2026-11-13T00:00:00Z`，合成游戏为 Baldur's Gate 3，8 条合成历史记录；浅色/深色主题各覆盖 1040×700 和 1600×900 DIP，常规/详情打开状态均有样图。

截图保存的是存档视图客户区。客户区尺寸由生产外壳布局推导：1040×700 窗口对应 744×460 DIP；1600×900 窗口对应 1276×660 DIP。渲染缩放为 1.0，字形解析、字体候选和运行桌面记在 [基线报告](evidence/V01/round6-baseline-report.txt) 与 [SHA-256 清单](evidence/sha256-manifest.json)。这组图是受控 WPF 渲染，不是 Playnite 真实前台画面，也不代表物理 DPI。

| 主题 / 窗口 | 常规状态 | 详情打开 |
|---|---|---|
| 浅色 · 1040×700 | [查看基线](evidence/V01/baseline/save-history-light-1040x700-details-closed.png) | [查看基线](evidence/V01/baseline/save-history-light-1040x700-details-open.png) |
| 深色 · 1040×700 | [查看基线](evidence/V01/baseline/save-history-dark-1040x700-details-closed.png) | [查看基线](evidence/V01/baseline/save-history-dark-1040x700-details-open.png) |
| 浅色 · 1600×900 | [查看基线](evidence/V01/baseline/save-history-light-1600x900-details-closed.png) | [查看基线](evidence/V01/baseline/save-history-light-1600x900-details-open.png) |
| 深色 · 1600×900 | [查看基线](evidence/V01/baseline/save-history-dark-1600x900-details-closed.png) | [查看基线](evidence/V01/baseline/save-history-dark-1600x900-details-open.png) |

窄窗打开详情后，页面按生产行为滚到详情区域；宽窗在未选中行时收起详情、选中时显示侧栏。没有通过强制模板状态制造生产界面不支持的组合。主表当前 1040×700 基线在四行可读底线内。

## V02 · 从截图确认的差距

以下观察来自上表基线和同一轮 408 张图的完整 Render QA，不把未观察的猜测列成缺陷。

1. **页面内标题与对象上下文挤在同一小块。**“存档清单”与版本数在左上，游戏名、匹配状态和健康状态退到数行辅助文字；主对象和保护结论没有形成稳定的首屏焦点。
2. **摘要区占高较多。**1040×700 客户区中，摘要与两行操作区后才到表头；完整 Render QA 记录主表 274 DIP 高、4 行可读，页面滚动内容比可见视口多 112 DIP。
3. **操作优先级分散。**“立即扫描”是主操作，但备份、重新校验、刷新、时间范围、清空范围、版本筛选、列显示和视图管理仍以相近按钮轮廓连续排列。窄宽时第二行还要再断成第三段。
4. **控件家族的描边权重不统一。**浅色基线中多个次要按钮有明显深色外轮廓；深色基线又以亮线显示，强调色与边框的层次需回到共享样式统一处理。
5. **详情的窄宽路径不够直接。**1040 窗口先滚动表格，再滚动到下方详情；宽屏则常驻右栏。详情信息在紧凑布局中首屏不可见，开合的视觉连续性也没有明确提示。
6. **表格数据密度已有可用底线，但对齐和标签可再精修。**四行可读、列虚拟化及现有滚动均正常；时间、类型、状态、数量、容量、设备和备注需要更稳定的基线、数字宽度与行内留白。
7. **详情把摘要、恢复可用性和技术字段排成一条长流。**备份 ID、加入待办、来源关系、校验结果、技术信息与恢复草稿操作需要分组，且继续明确“校验通过不等于路径映射完成或真实恢复已演练”。

没有在本阶段删除操作或改动滚动/选择行为。完整矩阵报告为 `render-qa OK`、0 个 `PROBLEM`；其几何记录用于约束后续改造。

## V03 · 可实现的页面规格

```text
存档页签
┌ 当前游戏 + 明确的匹配 / 保护状态 + 版本数与时间范围 ─────┐
├ 主要动作：扫描/备份             次要动作：校验/刷新 ────┤
├ 搜索 ─ 历史范围/版本导航 ─ 列与保存视图 ──────────────┤
├ 历史表格（有限视口、虚拟化、内部滚动） │ 版本详情面板   ┤
└ 状态/可用性提示                    │ 恢复安全与技术项 ┘
```

- **对象区：**把当前游戏名设为视觉锚点；同一行呈现可读的匹配、健康/保护状态和版本数量，日期范围作为辅助信息。状态保留文字和语义标记，不能只用颜色。
- **操作区：**扫描和备份作为清楚的主要操作组；重新校验、重载详情作为次级工具；搜索、日期范围、最近/较早/加载更早、列显隐和已保存视图收拢到同一工具栏家族。当前没有存档关键字搜索绑定，V06 将它作为仅作用于已载入历史视图的客户端筛选实现，不触发 Worker、备份或存档 I/O，并补上清除、键盘和选择保持覆盖。1040 窗口允许两行，但每组内部保持稳定，不盖住当前条件。
- **表格区：**保留时间、类型、文件数、大小、设备、备注、保护/恢复状态和现有列配置；表头约 38 DIP、数据行 44–48 DIP，数量/大小统一数字对齐。维持至少四行可读、当前游戏选框、横向访问与生产滚动条实现。
- **详情区：**先显示版本身份与关键摘要，再分组呈现来源关系、隔离校验、恢复可用性和技术字段；长路径/错误保留可复制或 Tooltip 的完整值。恢复草稿仍是只读授权前置，按钮和文案不得暗示已执行恢复。
- **比例与响应：**1600×900 让表格占约 62–66%，详情占 34–38%；1040×700 使用紧凑抽屉/下方详情，主表仍先保留四行并由页面滚动访问完整详情。切换视口不清选择、不重置表格滚动锚点。
- **类型与主题：**以共享字体回退为准；游戏名约 20–22 DIP、分区标题 15–16、正文 13–14、辅助 11–12；时间与容量使用现有数字/时间样式。强调色只突出主要操作、选择和焦点，按钮、输入、徽标和表面统一由共享令牌驱动。

此规格给 V05–V12 实施用；真正效果仍以后续生产 XAML 前后对照和内部复核为准。

## V04 · 同数据状态夹具

扩展了 `FakeDashboardData` 的长文本样本：游戏名为“星海远征·终焉回响典藏版 — Starfall Exodus Complete Collection Edition”，备注长 172 字符并包含英文构建信息和 `D:\Games\Starfall Exodus Complete Collection\Saved\Profiles\VeryLongFolderName\Manual-Checkpoint\slot-01.sav`，设备名也故意加长。长文本行同时验证选择两条记录、文本省略和完整 Tooltip；相关检查通过。

存档页 Ready、Empty、Loading、Error、Stale、Offline 六种状态均由内存夹具渲染；Ready 与 Stale 保留行，初载错误/离线不伪造记录。双主题、1040×700 和 1600×900 的 28 张必要样图见 [V04 状态样本](evidence/V04/state-fixtures/)，完整矩阵运行记录见 [statefixtures 报告](evidence/V04/statefixtures-report.txt)。未启动 Worker、Playnite、真实游戏库或任何存档/云端写操作。

## 运行与内部质量复核

- SDK 8.0.423 / Release 构建 RenderHarness：0 warning、0 error。
- 专用基线采样与全状态夹具均运行在 `WinSta0\GscRound6_*` 私有桌面；运行前后输入桌面保持 `Default`，脚本退出码 0。源提交和运行环境见两个 `runner-metadata.json`。
- 完整 Render QA：408 张 PNG、`render-qa OK`、0 个 `PROBLEM`。WPF 静态审查扫描 32 个 XAML：0 error、26 warnings、179 info；本阶段未改生产 XAML，因此保留原有 warning 作为后续有目标处理的信号，没有通过关闭检查消去。
- `python scripts/validate-source.py` 与 `git diff --check` 均通过。首轮采样遇到 Harness 编译符号、初始化时机和宽屏默认详情状态问题，已按生产视图实际行为修正；静态校验发现的 PowerShell UTF-8 BOM 也已补齐。尝试与修正摘要记在 [采样诊断](evidence/V01/attempts.md)。
- 质量复核确认：主表与四行可读底线在本阶段都没有回退；后续可改动页面层级、留白、按钮和详情排版。受控离屏截图不证明 Playnite 前台帧率、物理输入、宿主主题/DPI 或 UI Automation 客户端体验。

## V05-V08: 存档页首屏与详情示范

生产实现提交为 `6a7b01b3e7fa5862b49cfcb1ff87f701be9b308e`（基线 `c543790fcb6277f994f86bc58d0acd40631f9ed6`）。下列截图使用相同的 RenderHarness 与 8 条 `FakeDashboardData(Ready)` 记录，固定采样时间 `2026-11-13T00:00:00Z`、双主题、1.0 离屏 DIP 缩放和相同的客户区（窄窗 744×460 DIP、宽窗 1276×660 DIP）。`before`/`after` 原图保留在证据目录；并排图在上方标出对应提交短 SHA。详细运行身份、尺寸、样本和退出码见 [渲染记录](evidence/V05-V08/reports/before-render-report.txt)、[基线运行元数据](evidence/V05-V08/reports/before-runner-metadata.json) 与 [候选运行元数据](evidence/V05-V08/reports/after-runner-metadata.json)。两次采样都在 WinSta0 隔离桌面运行、脚本退出码 0，且输入桌面前后均为 `Default`。

| 状态 | 浅色对比 | 深色对比 |
| --- | --- | --- |
| 1040×700，常规 | [查看并排图](evidence/V05-V08/comparison/save-history-light-1040x700-details-closed-before-after.png) | [查看并排图](evidence/V05-V08/comparison/save-history-dark-1040x700-details-closed-before-after.png) |
| 1040×700，详情打开 | [查看并排图](evidence/V05-V08/comparison/save-history-light-1040x700-details-open-before-after.png) | [查看并排图](evidence/V05-V08/comparison/save-history-dark-1040x700-details-open-before-after.png) |
| 1600×900，常规 | [查看并排图](evidence/V05-V08/comparison/save-history-light-1600x900-details-closed-before-after.png) | [查看并排图](evidence/V05-V08/comparison/save-history-dark-1600x900-details-closed-before-after.png) |
| 1600×900，详情打开 | [查看并排图](evidence/V05-V08/comparison/save-history-light-1600x900-details-open-before-after.png) | [查看并排图](evidence/V05-V08/comparison/save-history-dark-1600x900-details-open-before-after.png) |

原始图：[基线 before](evidence/V05-V08/before/)、[生产候选 after](evidence/V05-V08/after/)。这些是生产 WPF 视图在受控 Harness 中的采样，并排图没有缩放两侧的内容区域；不是 Playnite 前台截屏，也没有将离屏缩放解释成物理 DPI 验证。

### 可见变化与复核结论

- 游戏名成为页面首屏锚点，匹配/保护状态与版本数紧邻呈现；扫描/备份形成主操作组，校验/刷新退为次级操作。
- 搜索、历史范围和版本导航、列与保存视图按关系归组，窄宽下工具栏有稳定的第二行；搜索只过滤已载入元数据。
- 宽窗把历史记录和详情安排成清楚的 2:1 两栏；详情先给创建时间、触发方式、文件数、容量等摘要，再列来源关系、恢复可用性与技术字段。
- 表格行的信息基线与状态标签更清楚，详情里的来源/安全说明继续保留完整语义。快速阅读时主要动作和当前对象更容易区分。
- 1040×700 紧凑布局仍沿用生产页面的滚动职责：详情在表格之后访问，受限视口不会同时显示完整详情。历史表格的长文本横向访问方式也仍由现有滚动系统提供。此点在截图中可见，未将窄窗描述成无滚动抽屉。
- 当前游戏选框、业务命令/Binding、版本选择、虚拟化、键盘路径、恢复安全说明和生产滚动条系统均保留。客户端搜索没有读取归档或调用 Worker。

内部评审复看了双主题宽屏详情和紧凑详情状态。层级与工具栏分组有可见改善，表格/详情分界明确；紧凑详情需要页面纵向滚动是已知边界。没有发现本批引入的选框越界或状态语义丢失。变化足以满足 V05–V08 与 V12 的页面示范验收，继续 V09–V11 共享原件与后续动效。

SDK 8.0.423 / Release 的本地完整阶段回归：构建 0 warning / 0 error；Core 157/157、Worker 407/407、Playnite 2,044 项中 1,986 通过、0 失败、58 个既有 skip，157/157 个 WPF 隔离类。`python scripts/validate-source.py` 和 `git diff --check` 通过。生产 WPF 静态审查为 26 个 XAML、0 errors / 26 warnings / 164 info；本批没有关闭审查规则。精确候选全量 Render QA 与本地验证记录位于 `evidence/V05-V08/reports/`，并列入 [SHA-256 清单](evidence/sha256-manifest.json)。GitHub run [#1570](https://github.com/Nikilua/GameSaveCenter/actions/runs/43478597342) 正在精确提交 `6a7b01b3e7fa5862b49cfcb1ff87f701be9b308e` 上运行；基线和 SDK 10.0.401 两个适用 job 当前均为 in progress，V27 仍会对最终候选重新核验。真实 Playnite 前台呈现、宿主缩放与 UIA 客户端体验仍按旧边界待验。
