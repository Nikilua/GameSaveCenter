# GameSaveCenter 持续维护交接与开发入口

> 这是 GameSaveCenter 的跨电脑、跨模型持续维护入口。任何新的 agent、模型或开发者接手前，先完整读取本文件，再读取项目记忆、开发进度和 UI 规则。不要只依赖聊天记录。

## 接手时的最短指令

以后可以直接对新的 agent 说：

```text
请先读取 GameSaveCenter 项目的 docs/DEVELOPMENT_HANDOFF.md，按照其中的读取顺序、不可丢失约束、开发流程和验证要求继续维护，不要重置或覆盖已有改动。先检查 git status，再根据文件中的当前基线继续下一项 UI 工作；完成后更新项目记忆、开发进度并提交 commit。
```

## 必须读取的资料

按以下顺序读取：

1. `AGENTS.md`
2. `docs/DEVELOPMENT_HANDOFF.md`（本文件）
3. `docs/PROJECT_MEMORY.md`：长期不可丢失约束、已完成 UI 决策和性能边界
4. `docs/DEVELOPMENT_PROGRESS.md`：按 UI 编号排列的实施历史和下一步线索
5. `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md`：总体设计方向
6. `docs/design/UI_CHANGE_GATE.md`：每次 UI 变更的门禁与验收标准
7. `C:\Users\lopmatuse\.codex\attachments\1b6b382f-30ed-44c7-a9ce-6c580fefbe83\pasted-text.txt`：用户提供的完整任务提示词附件；如果新电脑不存在该路径，以本文件和仓库内文档为准
8. `D:\workplace\Github\GameSaveCenter.WpfUiDemo.v3.1`：WPF Demo 模板，比较布局层级、节奏、控件尺寸和交互表面，不复制 Demo 假数据或业务实现；当前本机实际可用副本为 `D:\workplace\VSCode\GameSaveCenter.WpfUiDemo.v3.1`

如果附件路径发生变化，先在当前对话的附件中找到同一份完整提示词；不能因为附件不可用而跳过仓库内的规则和约束。

## 项目目标

这是已有的 Playnite 插件项目 GameSaveCenter 的持续 UI 重构，不是新建功能，也不是只改某一个 `Margin` 或颜色。

生产 UI 使用现有的 WPF/C# 技术栈，最终视觉和信息层级应接近 `GameSaveCenter.WpfUiDemo.v3.1`。Demo 只是 UI 模板；生产页面必须继续使用真实数据、真实命令和真实状态。

必须长期保持：

- 不改插件 ID、业务服务、Worker、IPC、数据库、持久化和任务协议，除非用户明确要求并单独批准。
- 保留现有 ViewModel、命令、Binding、`x:Name`、真实状态流、错误反馈和 Playnite 生命周期。
- 修复共享资源、样式、ControlTemplate 和页面结构，不用局部补丁掩盖共享控件问题。
- 保留大型列表的有限测量、内部滚动、键盘访问、UI Automation 和虚拟化。
- 不使用 HTML、WebView、Electron、Avalonia、WinUI 或截图替代原生 WPF。
- 不使用 Demo 假数据，不用 `Task.Delay` 模拟业务成功，不把视觉状态伪造成业务结果。

## 用户明确提出的 UI 细节

这些细节不是可选的视觉偏好，而是持续验收标准：

- 同类按钮的宽度/高度、文字方向、文字垂直和水平位置必须一致。
- 文本不能因为共享模板硬编码对齐而偏离调用方意图；需要修复共享模板的 `TemplateBinding`。
- 下拉框初始必须显示明确的默认值。例如初始筛选是“全部”，控件就必须显示“全部”。
- 对于依赖真实上下文的动态下拉框，空值可能代表“等待真实上下文”，不能为了视觉而强行选择第一项。
- 页面在 100%/125%/150%/175%/200% DPI、不同窗口尺寸、Light/Dark/Follow Playnite/高对比度下不能出现重叠、裁切、文字不可读或操作入口消失。

## 每次开发的固定流程

1. 先执行 `git status`、`git branch --show-current` 和最近提交检查；不得 `reset --hard`、`checkout --` 或覆盖别的电脑留下的未提交改动。
2. 读取本文件、`PROJECT_MEMORY.md`、`DEVELOPMENT_PROGRESS.md` 中与目标页面相关的最新条目。
3. 搜索目标控件的全部共享样式、模板、资源和调用点，先判断根因属于信息架构、布局测量、模板状态、可读性、可访问性、性能还是宿主兼容性。
4. 按 UI Change Gate 实施小范围、可验证的 UI 改动；优先共享资源和结构修复，保持业务合同不变。
5. 至少运行适用的静态校验、`git diff --check`、源码验证、WPF 结构测试、Debug/Release 构建和相关单元测试。
6. 真实 Playnite、主题、DPI、键盘或宿主渲染没有实际运行时，必须明确写“尚未验证”，不能声称已经验证。
7. 完成一轮后同步更新：
   - `docs/PROJECT_MEMORY.md`：新增不可丢失的结构/行为约束
   - `docs/DEVELOPMENT_PROGRESS.md`：记录 UI 编号、修改文件、保留的命令/绑定、验证结果和未完成的宿主验证
   - 本文件的“当前交接基线”和“下一步方向”
8. 每次有实际开发改动都必须创建一个清晰的 Git commit。提交前确认工作区没有意外文件。

## 合并后当前交接基线（2026-08-11）

- 分支：`main`
- 当前 UI 交接基线：`f11e9b7`（`ui: keep demo shell at common minimum width`）；Demo 最小常用窗口 1040×700 DIP 的带文字侧栏/单行顶栏、Media 四卡、Task 2×2 摘要和 Maintenance 两列健康卡响应式节奏已固化，记忆与进度文档随后单独提交
- 上一合并提交：`e87e2af`（`merge: reconcile local and cross-machine UI migration`）
- 合并共同基线：`9cdd975`；本机 UI-173～UI-181 与 `origin/main` 的 UI-181～UI-183、交接文档线均已保留，没有删除任一方共同基线后的提交。
- 本机额外 WIP 已先由 `e61d0fc` 固化后纳入合并；本机的长期约束已追加到 `docs/PROJECT_MEMORY.md` 的 `MERGE-001`，远端既有记忆条目保持原文。
- UI-184 已将 Overview 的 Demo 层级落地为“今日工作台动作卡 → Hero/当前游戏双列（受限宽度堆叠）→ 六项指标 → 最近活动”，具体约束见 `docs/PROJECT_MEMORY.md` 与 `docs/DEVELOPMENT_PROGRESS.md`。
- UI-185 已将 SaveCenter 的候选路径页补齐为“当前规则与校验 → 候选表/Inspector”，页签改为“路径与校验”；共享 Dashboard 游戏上下文不重复渲染，真实 `SelectedGame` 状态和扫描/校验/刷新命令保持绑定，具体约束见 `docs/PROJECT_MEMORY.md` 与 `docs/DEVELOPMENT_PROGRESS.md`。
- UI-186 已将 TaskCenter 摘要卡改为 Demo 的“任务总数 / 运行中 / 需要重试 / 已完成”四项真实任务状态计数，宽屏四列、窄屏按两列/单列收缩；任务筛选、全局视角、详情 Inspector 和恢复命令未改变。
- UI-187 已将 Maintenance 诊断页顶部摘要改为 Demo 的六项真实健康卡（Worker、Ludusavi、Rclone、数据与备份目录、媒体目录、设备状态），并将响应式列数收口为宽屏 3 列、中屏 2 列、窄屏 1 列；诊断操作、表格、Inspector、审计、完整摘要和原有命令/绑定均保留。
- UI-188 已将 TaskCenter 任务队列补齐 Demo 的搜索输入框，真实搜索任务 ID、类型、游戏、详情和错误，并与状态/游戏/类型筛选叠加；未新增 Worker/IPC 请求，任务计数、Inspector、恢复命令和虚拟化保持不变。
- UI-189 已将 TaskCenter 顶部四项任务摘要改为 Demo 的“标题 → 30px 数值 → 副文案”三行阅读卡，仍绑定真实任务计数并保持四列/两列/单列响应式逻辑；搜索、筛选、Inspector、命令和虚拟化未改变。
- UI-190 已修复常用窗口尺寸下 Overview 底部内容被截断、Media 表格只剩一行和 Maintenance 诊断下方内容不可达的问题：Overview 只滚动上方工作台内容并让最近活动保持有限 Grid/ListBox 视口；Media 与 Maintenance 使用明确命名的页面滚动面承载下方内容，主表/主列表由 code-behind 保持 236–460 DIP 有限高度和内部虚拟化滚动。真实命令、Binding、Inspector 和业务层未改变。
- UI-191 已修复 SaveCenter/TrainerCenter 窄宽度或低高度堆叠 Inspector 时主列表被挤成一行的问题：历史版本/候选路径 DataGrid、已安装工具/FLiNG 搜索结果/可下载版本 ListBox 区域保持 236 DIP 最小视口；Inspector 高度按实际布局剩余空间计算并继续使用自身滚动。真实命令、Binding、选中项、导入确认、虚拟化和 Recycling 未改变。
- UI-192 已修复 TaskCenter 窄宽度或低高度堆叠详情 Inspector 时任务主表被挤压的问题：任务队列保留 236 DIP 最小视口，详情高度按摘要区、筛选区和主表剩余空间计算，搜索/筛选、真实计数、取消/重试/复制命令和虚拟化未改变。
- UI-193 已修复 Maintenance 设备状态、异常审计和进程映射页窄宽度/低高度堆叠 Inspector 时主表被挤压的问题：诊断、设备、审计发现和进程映射表统一保留 236 DIP 最小视口；设备/审计 Inspector 按实际布局剩余空间限高并继续内部滚动，进程映射表接入显式结构标识和共享表头主题加载，命令、Binding、选中项、审计日志和虚拟化未改变。
- UI-194 已将共享页面级滚动契约与 Demo 对齐：`GscPageScrollViewer` 默认垂直 `Auto`，设置页、存档策略、维护保留策略、侧栏导航和 Overview 辅助页面不再用 `Hidden` 掩盖溢出；Overview 宽屏/堆叠的有限内部滚动仍由 code-behind 分工控制，表格/列表虚拟化未改变。
- UI-195 已补齐 Maintenance“设备状态 / 异常与审计 / 进程映射”三个 Tab 的命名页面滚动面；对应三张主表由 code-behind 使用 236–460 DIP 有限 Height，继续保留 DataGrid 内部滚动、虚拟化、Inspector 滚动、真实命令和 Binding，源码门禁同步识别该结构。
- UI-196 已补齐 TaskCenter 的命名页面滚动面 `TaskPageScrollSurface`；任务表由 code-behind 使用 236–460 DIP 有限 Height，堆叠 Inspector 按页面实际视口计算剩余高度，摘要、筛选、任务表、详情和底部恢复操作在短高度常用窗口下均可通过明确滚动访问，真实搜索/筛选/计数/Binding/命令/虚拟化未改变。
- UI-197 已修复截图暴露的常用窗口可见性问题：TaskCenter 动态游戏/类型筛选的 WPF 集合刷新空选中在 DataBind 优先级恢复为 `全部`，MediaGrid 通过共享 ListBox 顶部内容契约避免少量媒体卡沉到有限视口底部，Overview 工作台按钮在窄主列第二行横向自动换行，宽屏右侧摘要/风险列保留有限 Auto 滚动以确保“打开维护中心”可达；真实命令、Binding、Worker、IPC、数据库、持久化、虚拟化、键盘访问和 Automation 未改变。
- UI-198 已修复 Overview 在常用窗口和窄窗口下的主工作区被 sibling 行挤压问题：工作台、Hero/当前游戏、六项指标和最近活动统一进入 `OverviewPrimaryScrollSurface`，窄布局再由 `OverviewStackScrollSurface` 统一承载主列与右侧摘要，避免 980 DIP 下主列高度变成 0；宽布局仍保留主列/摘要列独立有限滚动。`OverviewActivityList` 继续使用有限高度、ListBox Recycling 和自身滚动，真实命令、Binding、SelectedTask、键盘访问和 Automation 未改变。已按 1600/1366/1280/1100/980 DIP 与 900/768/720/700/640 DIP 运行隔离生产离屏渲染，源码验证通过，生产插件 Release 构建 0 警告/0 错误，隔离测试 149/149 通过；未运行真实 Playnite 宿主、主题切换、DPI 真机和连续缩放流畅性验证。
- UI-199（代码提交 `5cbd512`）已修复工作区由程序化导航、恢复状态或离屏渲染直接切换时顶栏仍显示“首页”的语义不同步：`DashboardView.UpdateWorkspacePresentation()` 与侧栏点击共同调用 `UpdateWorkspaceHeader`，媒体/维护/任务等页面标题和副标题始终跟随当前可见工作区。MediaCenter 的摘要卡响应式断点改为逻辑 DIP 的 `>=760` 四列、`>=520` 两列、其余单列，使 Dashboard 在常用 1080p/2K/4K 窗口下保持 Demo 四卡横排并为主表保留可见行；表格有限视口、内部滚动、虚拟化、Inspector、真实命令和 Binding 未改变。源码验证通过；生产插件 Release 构建 0 警告/0 错误；隔离 WPF 测试 150/150 通过；生产离屏渲染覆盖 1600/1366/1280/1100/980 DIP 与 900/768/720/700/640 DIP 并返回 `render-prod OK`。Render harness 自身仍有 3 个 FakeApi 未使用事件警告；真实 Playnite 宿主、主题切换、DPI 真机和连续缩放流畅性尚未验证。
- UI-200（代码提交 `f11e9b7`）已将 Demo 的 `MinWidth=1040`、`MinHeight=700` DIP 固化为生产外壳的常用最小窗口：Dashboard `>=1040` 保留带文字侧栏和单行顶栏，低于该值才进入图标紧凑壳；同时按外壳扣除侧栏后的约 700 DIP 页面宽度校准 Media `>=700` 四列、Task `>=900` 四列/`>=680` 两列、Maintenance `>=980` 三列/`>=680` 两列。1040×700 离屏结果为 Media 四卡并显示两行表格、Task 2×2 摘要并显示队列、Maintenance 两列健康卡；1366×768 仍为完整多列，页面级滚动、表格/列表有限视口、内部滚动、虚拟化、真实命令/Binding 和业务层未改。源码验证通过；生产插件 Release 构建 0 警告/0 错误；隔离 WPF 测试 151/151 通过；生产离屏渲染覆盖 1600/1366/1280/1100/1040/980 DIP 与 900/768/720/700/640 DIP 并返回 `render-prod OK`。Render harness 自身仍有 3 个 FakeApi 未使用事件警告；真实 Playnite 宿主、主题切换、DPI 真机和连续缩放流畅性尚未验证。
- 新增的响应式门禁要求：1080p、2K、4K 不能只按物理分辨率判断，必须按 DPI 换算后的逻辑 DIP 尺寸检查全屏、窗口化和最大化；常用窗口下首屏下方真实内容不得被页脚或工作区边界遮住，主表/主列表应保留约四行可读视口，页面滚动与列表内部虚拟化滚动必须分工明确。具体门禁见 `docs/design/UI_CHANGE_GATE.md`。
- 本轮工作区：合并提交完成时干净；后续 agent 仍须先运行 `git status`、`git log -5 --oneline --decorate` 和 `git branch --show-current`。
- 验证：源码验证通过；生产插件隔离 Release 构建 0 警告/0 错误；隔离测试输出 151/151 通过；生产离屏 render harness 返回 `render-prod OK`，自身有 3 个 FakeApi 未使用事件警告。覆盖尺寸包含 1600/1366/1280/1100/1040/980 DIP 与 900/768/720/700/640 DIP。由于本机只有 .NET 9 SDK 且仓库 `global.json` 以 .NET 8 为基线，测试使用隔离输出验证，未覆盖真实 Playnite 宿主、主题、DPI、窗口化截图和连续缩放运行时渲染。

以下原有的远端交接基线保留为历史记录，便于追溯另一台机器的 UI-183 上下文：

## 原远端交接基线（2026-08-10）

- 分支：`main`
- 当前 UI 基线：`2db4336`（`重构：维护中心诊断操作区对齐 Demo`，UI-183）
- 版本：`0.6.70-development-preview`
- 当前工作区：本轮交接文档提交完成后应干净
- 相对 `origin/main`：请运行 `git rev-list --count origin/main..HEAD` 实时确认；UI-183 实现提交为 `2db4336`，后续交接文档提交不改变生产 UI
- 最近已完成的重点：
  - UI-177：修改器中心无选中工具时释放空 Inspector 和固定右栏
  - UI-178：媒体中心无选中媒体时释放空 Inspector 和堆叠行
  - UI-179：设置页窄屏标题提示换行，以及 ZIP/Zstandard/跟随 Playnite 默认值显示
  - UI-180：首页阅读顺序调整为“工作台/今日状态 → 当前游戏 → 六项指标 → 最近活动”
  - UI-181：维护中心五张 DataGrid 的首列显式使用 `MaintenanceFirstColumnHeader`，末列继续使用显式维护表头主题
  - UI-182：维护中心进程映射编辑器宽屏使用 EXE `*`、目标游戏 240 DIP、绑定按钮的 Demo 对齐 Grid，窄于 720 DIP 时目标和按钮换到第二行
  - UI-183：维护中心诊断页顶部改为 Demo 式“诊断操作”阅读卡，刷新诊断提升为主操作，其余五个只读入口保留在第二行操作带
- 当前已完成的自动化基线：Core 13、Worker 23、Playnite 142 测试通过；Release 构建 0 警告/0 错误；XAML 结构检查 13/13；WPF 源码验证通过。静态测试不等同于真实 Playnite 宿主、主题和 DPI 渲染验证。

## 下一步开发方向

继续以 Demo 对齐为目标，对生产页面做页面级收口和真实宿主验收，优先顺序如下：

1. 逐页检查 Overview、SaveCenter、TrainerCenter、TaskCenter、Maintenance、MediaCenter、Settings 的层级、按钮尺寸、文字对齐、默认选择和空状态。
2. 检查共享 `Button`、`ComboBox`、`TextBox`、`ListBox`、`DataGrid`、Tab 和 Inspector 资源，发现同类问题时修共享模板。
3. 按 980/1040/1100/1280/1366/1600 DIP 宽度、640/720/900/1080 DIP 高，以及 1080p/2K/4K 在 100%/125%/150%/175%/200% DPI 下的常用窗口化逻辑尺寸，复核窄屏堆叠、首屏内容可见性、有限滚动和长文本；不把 4K 通过当作 1080p 通过。
4. 在可用环境中运行 Playnite 宿主，验证 Light/Dark/Follow Playnite/高对比度、键盘焦点、真实数据加载和窗口关闭生命周期；若环境不可用，保留明确的手工验收清单。
5. 发现问题后继续使用新的 UI 编号记录，不要删除历史记录或把未验证事项标成完成。

## 跨电脑、跨模型规则

代码和文档是交接的真实来源，模型记忆不是。切换电脑前应先把当前提交推送到远端：

```powershell
git push origin main
```

切换后应先拉取同一分支，再执行：

```powershell
git status
git log -5 --oneline --decorate
git branch --show-current
```

如果另一台电脑有未 push 的提交或未提交改动，先比较 `git status`、分支和提交历史，再合并；禁止直接覆盖。DeepSeek、Claude 或其他模型可以根据本文件、源码和测试继续工作，但不会自动继承原聊天中的隐含上下文、工具状态或审批状态，因此必须先读取本文件并按流程重新建立上下文。

## 用户原话（必须保留）

> 在继续之前你最好是能够搞一个文件，能够指引去哪里读取获得开发方向等等。这样我直接说你读取xx文件就可以了，他就知道后续怎么开发了。连我这段话你也要放进去，省得我每次都说了（这样每次开发他们都会维护这个项目）。

这句话代表长期维护要求：后续每次开发都必须继续维护本项目，并同步维护本交接文件、项目记忆、开发进度和 Git commit。
