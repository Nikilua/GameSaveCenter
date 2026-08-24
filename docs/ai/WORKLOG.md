# GameSaveCenter AI 开发工作日志

> 每完成一个有意义的阶段追加一条；只记录对未来开发有帮助的信息。

## 2026-08-24 UI-299 表格字体与行表面可读性优化

**问题确认：**

- 现有共享 DataGrid 已使用 Demo 的 UI 字体链，但单元格没有显式锁定字体令牌；在 Playnite 宿主主题介入时存在回退风险。
- 浅色表格的表头、数据行和卡片底色过于接近，长列表阅读时行边界主要依赖细分隔线；深色表格也缺少稳定的行面层次。

**实现内容：**

- `DesignTokens.xaml` 新增 `GscTableRowBrush`，运行时浅/深主题和 Demo 核心主题分别提供低透明度的基底行面与交替行面；高对比度仍降为透明，由系统高对比度颜色负责可读性。
- 共享 `DataGrid` 与 `GscRedesignWorkspaceDataGrid` 启用 `TextOptions.TextFormattingMode=Display`；`DataGridCell` 显式继承 `GscUiFontFamily` 与 `GscBodyFontSize`，不改变页面已有按钮、游戏选框、命令、Binding、列宽、排序、滚动和 Recycling 虚拟化。
- 由于上一轮大型库 Worker 预热已把行为改为“预热 Worker、延后目录同步”，同步修正两条仍要求旧语义的源码断言；实现行为未放宽。

**验证结果：**

- `validate-source.py`、XAML 结构检查 19/19、WPF 静态审查 0 error/21 warnings 通过。
- `render-qa` 输出 `artifacts/ui-qa/table-readability-v1/render-qa-report.txt` 为 `render-qa OK`；浅/深主题、多尺寸、表头排序/列宽契约、Save/Task/Media/Maintenance 表格滚动和媒体 4468 项回退探针均通过。
- Release 一键构建安装测试通过：Core 59/59、Worker 198/198、Playnite 281 通过/57 跳过；当前开发版 0.6.70 已安装并在隔离 Preview 扩展后由真实 Playnite 日志确认加载。Computer Use 返回的 Playnite 虚拟窗口没有真实顶层 HWND，截图实际落到 Codex 窗口；为避免误点其他应用，本阶段没有把宿主点击回归写成已完成。
- 追加宿主复核：由 Windows 正常启动 Playnite 时启动页短暂拥有 HWND，主窗口显示后句柄归零；Computer Use 重新选窗、激活和 Raise 仍返回 Codex 截图/`EmptyWindowAutomationPeer`，因此没有发送点击或滚动。测试后 Preview 238 个文件已恢复原路径，Playnite 与标准 Worker 均已停止。
- 追加真实 WPF 操控：用临时窗口直接托管生产 `SaveCenterView` 和 160 行 `DataGrid`，Computer Use 成功切换“历史版本”、选中第二行、滚动下行并滚回；选中态、右侧详情、字体、表头与行面显示均正常。临时宿主和构建目录已按规则清理。

## 2026-08-23 UI-298 安全增加毛玻璃高光与环境光 Blur

**问题确认：**

- 项目已有主题自适应的半透明表面和环境光层，但 Blur 资源一直为空，视觉上只有渐变光，没有实际的柔化环境光。
- Playnite 是嵌入式 WPF 宿主，不能安全地把整个宿主窗口或页面内容做背景模糊；对大表格、滚动内容或文字应用 Blur 会影响性能、清晰度和虚拟化表现。

**实现内容：**

- 新增共享 `GscAmbientBlurEffect`，只挂在 `AmbientMaterialLayer` 的三个固定尺寸环境光椭圆上，半径 18、`RenderingBias.Performance`；不作用于页面、文字、DataGrid、ListBox 或滚动区域。
- 生产壳增加一条 1 DIP、不可命中的玻璃高光线，增强现有半透明表面的层次，不改变布局和交互命中区域。
- 毛玻璃关闭或系统高对比度时，Blur 资源返回真实 `null`，环境光层继续按既有逻辑隐藏，高光降为透明；保留现有不透明、可读的降级路径。
- 增加 WPF 资源契约测试，固定 Blur 半径/性能偏好和关闭毛玻璃时的空效果；现有“滚动内容不得使用 BlurEffect”约束继续保留。

**验证结果：**

- WPF 资源定向测试通过（2/2）；源码验证通过。
- WPF 静态审查 0 error、21 warnings、164 info，warning/info 均为既有项目上下文。
- `artifacts/ui-qa/glass-blur-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖双主题、多尺寸、Tab、滚动和 resize transition。
- Release 解决方案构建 0 warning、0 error；未在真实 Playnite 宿主逐像素验收，需用户更新插件后人工确认实际显卡/宿主表现。

## 2026-08-23 FUNC-003 FLiNG 历史归档递归解析

**问题确认：**

- 现有 FLiNG 归档解析只读取 `https://archive.flingtrainer.com/` 根目录的压缩包链接，不会继续进入年份子目录；因此 2019 年以前的修改器地址虽然能同步到归档站点，但无法进入本地目录。
- 日志中的归档同步数量曾从在线目录的 741 项增加到 745 项，未能反映历史子目录的完整内容，也没有分别记录两类来源数量。

**实现内容：**

- FLiNG 归档改为有界广度优先目录扫描，递归读取归档站点的 HTTPS 子目录，限制最多 2048 个目录和 10000 个归档文件，按下载地址去重；仍只接受 `archive.flingtrainer.com` 下的 `.zip`/`.exe` 文件。
- 同步日志现在分别记录在线目录、归档目录和去重后的总数；归档站点临时不可用时继续保留在线目录的成功刷新语义。
- 增加根目录子目录发现、嵌套目录相对地址解析和外部链接过滤测试，覆盖 2019 年以前目录结构的核心路径。

**验证结果：**

- FLiNG 目录解析定向测试通过（3/3）。
- Release 解决方案构建 0 warning、0 error；Core 59/59、Worker 198/198 通过。
- 未在真实 FLiNG 归档站点或 Playnite 宿主执行网络/宿主验收；本阶段没有 UI/XAML 改动。

## 2026-08-23 PERF-001 大型库 Worker 预热与 Dashboard 版本探测解耦

**问题确认：**

- 975 个游戏的当前日志显示，插件打开后才启动 Worker；SQLite 初始化约 12 秒，首次 Dashboard 快照还同步等待一次 Ludusavi 版本探测。
- 当前大库并没有在打开页面时执行全库 `find`，但冷启动和 IPC 重试仍会让用户感觉 Worker/Ludusavi 长时间无响应。

**实现内容：**

- 大型 Playnite 库在宿主库稳定后后台预热 Worker；只启动进程、初始化存储和应用设置，仍然禁止自动全库目录同步，目录同步继续只由显式用户操作触发。
- Dashboard 首次快照改为立即使用已缓存的 Ludusavi 版本；版本探测改为 Worker 后台任务，六小时缓存和失败隔离语义保留，避免外部进程阻塞首次本地状态读取。
- Worker 初始化日志增加存储初始化、旧任务整理、快照清理和总启动耗时，便于区分 SQLite、恢复任务和快照清理的具体慢点。
- 新增 Playnite 源码契约测试，固定“大库预热但不自动同步”和“首次快照不等待版本探测”的行为。

**验证结果：**

- Release 解决方案构建 0 warning、0 error。
- 新增定向 Playnite 测试通过（1/1）；尚未进行真实 Playnite 宿主启动验证。

## 2026-08-23 FUNC-002 媒体收件箱忽略恢复闭环

**问题确认：**

- 批量忽略已经保留了归档副本，但页面没有恢复入口；误忽略后只能手工打开媒体目录，数据库状态、文件归档和用户操作之间缺少可回退闭环。

**实现内容：**

- Contracts/Worker 增加已忽略媒体列表与批量恢复 IPC。Worker 只在记录仍为 `Ignored` 时处理，先校验归档文件或原始文件，再将归档副本安全移回 `_Inbox\\Pending`；目标文件已有同内容时复用校验逻辑，不覆盖不同内容，原始截图/录像始终保留。
- SQLite 增加 `GetIgnoredMediaAsync` 与 `RestoreMediaToInboxAsync`，恢复后清空游戏归属、回写 `Inbox`、`NotApplicable` 和“用户撤销忽略，待重新归类”原因，并追加审计记录。批量接口沿用 500 项上限、去重、部分失败明细和取消传播语义。
- Playnite 收件箱只增加现有操作条中的“待归类/已忽略”轻量视图切换；已忽略视图按需加载，默认待归类流程不向旧 Worker 发起新请求。已忽略状态下隐藏归类/忽略动作，只显示“恢复到待归类”；恢复后同时刷新待归类和已忽略缓存。
- RenderHarness 增加 `MediaInboxItems` 兼容投影，保持大数据收件箱滚动探针覆盖真实行数据。

**验证结果：**

- `validate-source.py`、XAML 结构检查（19/19）通过；WPF 静态审查 0 error、21 warnings、164 info，均为既有项目上下文提示。
- Release 全解决方案构建 0 warning、0 error；Core 59/59、Worker 196/196、Playnite 280 通过/57 跳过。
- 新增 SQLite 恢复状态测试与真实文件移动测试；Worker 定向测试通过。
- `artifacts/ui-qa/media-inbox-restore-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖亮/暗主题、多窗口尺寸、Tab、滚动与 resize transition。未在真实 Playnite 中执行会改变用户媒体数据的恢复操作；宿主 DPI、高对比度和真实点击命中仍需人工验证。

## 2026-08-23 FUNC-001 媒体收件箱批量归类/忽略与真实状态显示

**问题确认：**

- 媒体待归类收件箱存在大批量待处理项，但原有归类和忽略只能逐条走 Playnite-to-Worker IPC；侧栏 Worker/Ludusavi 状态直接显示原始布尔值，用户无法快速理解“正常/不可用”的含义。

**实现内容：**

- Contracts 增加 `media.reassign.batch`、`media.inbox.ignore.batch` 及批量请求/结果 DTO；Worker 增加受限批量处理能力，单请求最多 500 项，去重后逐项复用现有移动、归档、SQLite 更新和审计逻辑，取消继续向上传播，部分失败返回明细。
- Playnite 收件箱只增加表格上方的轻量批量操作条：启用 `Extended + FullRow` 多选、复用 `InboxTargetGame` 选择目标游戏、前端按 500 项自动分批并显示成功/失败摘要；原有 Inspector、单条命令、安全确认和大数据表格滚动/虚拟化保持不变。
- 生产壳侧栏将 Worker/Ludusavi 指示灯和文案改为基于真实布尔状态的 DataTrigger，不再显示原始 `True/False`。

**验证结果：**

- `validate-source.py`、XAML 结构检查：通过（19 个 XAML 文件）。
- Release 隔离构建：0 警告、0 错误；Core 59、Worker 194、Playnite 280 通过，Playnite 57 项按既有规则跳过。
- `render-qa`：`artifacts/ui-qa/media-inbox-batch-v1/render-qa-report.txt` 为 `render-qa OK`，覆盖亮/暗主题、多窗口尺寸、滚动与 resize transition。
- `validate_wpf_ui.py`：0 error、21 warnings、164 info；提示均为项目已有布局/颜色上下文。未在真实 Playnite 中执行批量归类/忽略，避免改动用户数据；宿主 DPI/高对比度仍需人工验证。

## 2026-08-22 UI-297 页面激活时同步当前运行中游戏

**问题确认：**

- 既有自动定位只在 Dashboard 首次读取 Worker 快照和页面存活期间收到 Playnite `GameStarted` 时生效；普通刷新刻意不抢回手动选择。
- Worker 的 `ExternalGameProcessDetector` 首轮扫描是基线，不会把 Worker 启动前已存在的游戏进程直接建成会话。用户先启动游戏、再打开插件时，Worker 快照可能没有 `IsRunning`，导致游戏选框不切换。

**实现内容：**

- `GameSaveCenterPlugin` 增加只读 `TryGetCurrentlyRunningPlayniteGameIds()`，读取 Playnite SDK `Game.IsRunning`；异常时返回 null 并保留 Worker 原状态，不会触发 IPC、会话、备份或进程扫描。
- `DashboardView.xaml.cs` 在 `Loaded` 与 `IsVisibleChanged=true` 调用页面激活同步；`DashboardViewModel` 在快照应用后再调用一次，覆盖异步快照晚于页面显示的情况。
- 页面激活同步所有已缓存 DTO 的 `IsRunning`，刷新 `Snapshot.RunningGames`、GamePicker 缓存行和选中游戏通知；只有发现当前运行游戏时才按既有 resolver 规则切换，空闲时保留用户手动/持久化选择。
- `GamePickerItem` 增加轻量 `INotifyPropertyChanged` 刷新入口，不替换大库缓存对象，不改变 GamePicker 的筛选、排序和虚拟化结构。
- 新增非基线跳过的源码回归测试，锁定页面激活只读 Playnite 状态且不发起 `GameSessionStarted`。

**验证结果：**

- `scripts/validate-source.py`：通过。
- `scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot .tmp/runtime-autoselect-build`：XAML 19/19，Release 0 warning/0 error。
- 页面激活自动定位源码测试：1/1 通过。
- `validate_wpf_ui.py`：0 error、21 warnings、164 info，均为既有项目上下文提示。
- 完整 Playnite 测试为 240 通过、62 跳过、19 失败；失败来自当前分支已有的旧 Demo/布局断言（LabSegmented、摘要列、按钮高度等），未涉及本轮运行状态改动。真实 Playnite 宿主、主题/DPI 与页面反复打开仍待人工验证。

## 2026-08-22 UI-296 修正任务/媒体摘要条实际宿主的旧四列布局

**问题确认：**

- 用户提供的 Playnite 截图仍然呈现旧的“四列全等宽，Rectangle 与第 2/3/4 个统计列重叠”的结构：第一块后没有线，最后一块右侧出现多余线。此前源码和离屏证据已经改过，但 00:07 安装的 DLL 早于 00:14 构建，实际宿主仍加载旧版本；固定 10 DIP 分隔槽也没有完全复用首页的布局契约。

**实现内容：**

- `TaskCenterView.xaml` 与 `MediaCenterView.xaml` 的摘要条统一改为 `* / Auto / * / Auto / * / Auto / *`；四个真实统计块固定在 `0/2/4/6`，三条竖线固定在 `1/3/5`，不再存在尾部分隔线，也不允许统计块与 Rectangle 共用列。
- `WpfUiResourceDictionaryTests` 增加精确列序列、分隔线数量和分隔线列约束，防止 Demo 旧四列结构或固定槽回归；真实 Binding、文案、命令和数据契约未改变。
- 使用 `scripts/dev-install-run.ps1 -Configuration Release -NoStart` 完成当前版本安装验证；未访问或假定任何外部 Demo 文件夹，也没有结束 `GameSaveCenterPreview` 的 Worker。

**验证结果：**

- `scripts/validate-source.py`：通过；`validate_wpf_ui.py`：0 error、21 warnings、164 info，warning/info 为既有上下文提示。
- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/summary-divider-layout-fix`：0 警告/0 错误；Core 59、Worker 194、Playnite 277 通过/57 跳过/0 失败。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/summary-divider-layout-fix`：`render-qa OK`，双主题、1040/1100/1366/2560、多 Tab、滚动和 resize transition 通过；已抽查 Task/Media 摘要条。
- 安装目录 `C:\Users\lopmatu\AppData\Roaming\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec` 已验证清单 `0.6.70`、DLL `0.6.70.0`；Playnite 按 `-NoStart` 保持关闭，待用户启动后做宿主截图复核。

## 2026-08-22 UI-295 修正媒体摘要统计块与竖线顺序

**问题确认：**

- 媒体中心摘要条的四个统计内容使用四列，但分隔线也放在第 2、3、4 个统计列中，导致第一块后没有线，后续竖线偏到统计块内部，最后一块右侧出现多余竖线。

**实现内容：**

- `MediaCenterView.xaml` 将摘要条改为 `* / 10 / * / 10 / * / 10 / *` 七列：四个真实统计块位于 `0/2/4/6`，分隔线位于 `1/3/5` 并水平居中；所有 `MediaSummary`、`Snapshot.UnassignedMediaCount` OneWay Binding 和文案保持不变。
- 更新 `WpfUiResourceDictionaryTests` 与 `RestoredAcrylicForkBaselineTests`，锁定“统计块后跟分隔线”的列契约；媒体摘要测试改为默认 Fact，避免该视觉回归继续被历史基线跳过。

**验证结果：**

- `scripts/validate-source.py`：通过；`validate_wpf_ui.py`：0 error、21 warnings、164 info，warning/info 为既有上下文提示。
- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/media-summary-divider-fix`：0 警告/0 错误；Core 59、Worker 194、Playnite 277 通过/57 跳过/0 失败。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/media-summary-divider-fix`：`render-qa OK`，双主题、1040/1100/1366/2560、多 Tab、滚动和 resize transition 通过；已抽查媒体中心亮/暗主题摘要条。

## 2026-08-21 UI-294 修正任务统计分隔线并统一工作区环境光材质

**问题确认：**

- 任务中心统计卡片原本把分隔线和下一个统计项放在同一列；四组 `*` 列虽然能显示内容，但分隔线会与统计项争用布局空间，视觉上出现横向分布不均和贴近数字的问题。
- 首页 Hero 使用的是局部环境光渐变，其他工作区没有共享同一层；直接把 `BlurEffect` 套到页面内容或列表上会增加渲染成本，并可能影响滚动、虚拟化和高对比度模式。

**实现内容：**

- `TaskCenterView.xaml` 将统计条改为 `* / 10 / * / 10 / * / 10 / *` 七列，三条分隔线只占固定间隔列并水平居中；真实统计 Binding 保持不变，测试锁定列契约。
- 新增共享 `AmbientMaterialLayer`，在首页、存档、修改器、媒体、任务、维护六个工作区的真实内容之后绘制轻量径向环境光；设置页沿用同一套主题颜色。该层不接管布局、不拦截输入、不对文本/表格/滚动器使用 BlurEffect。
- 环境光颜色由 `AdaptiveThemePalette` 根据亮暗主题和透明效果设置动态资源；关闭玻璃效果或高对比度时通过 `GscAmbientPageOpacity=0` 隐藏，避免保留无效视觉树。

**验证结果：**

- `scripts/validate-source.py`：通过；`validate_wpf_ui.py`：0 error、21 warnings、164 info，新增 warning 仅为环境光层使用设计所需的负 Margin，其余为既有滚动/颜色提示。
- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/task-ambient-current`：0 警告/0 错误；Core 59、Worker 194、Playnite 276 通过/58 跳过/0 失败。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/task-ambient-final5`：`render-qa OK`，双主题、1040/1100/1366/2560、多 Tab、滚动和 resize transition 通过；离屏 RenderHarness 证据不等同真实 Playnite 宿主逐像素验收。

## 2026-08-21 UI-293 修复首页关注事项挤压与版本比较气泡

**问题确认：**

- 首页“需关注事项”行把 `SuggestedAction` 放在无约束的 `Auto` 列；建议句子按完整理想宽度测量后，标题列只剩几个字的空间，截图中出现标题被挤成“发…”/“勇…”的现象。
- 存档中心“版本比较”标题旁的状态气泡依赖 `LastBackupDiff.ComparisonQualityDisplay` 的嵌套路径；比较尚未执行时路径可能返回 `UnsetValue`，现有 `TargetNullValue` 不足以覆盖，视觉上只剩空的色块；标题与气泡也没有明确使用同一行的垂直居中约束。

**实现内容：**

- `OverviewView.xaml` 将关注事项建议列固定为 220 DIP，标题/游戏名保留星号列并设置 `MinWidth=0`，建议继续绑定真实 `SuggestedAction`，只在右侧受控宽度内省略，不再反向挤压标题。
- `SaveCenterView.xaml` 将“版本比较”标题和质量气泡改为明确的两列标题行；标题、气泡和气泡内文字均垂直居中，并为嵌套绑定同时提供 `TargetNullValue` 与 `FallbackValue`，初始显示“等待比较”。比较完成后仍直接显示 Worker 返回的精确/估算/Manifest 无效状态。
- `DashboardViewModel` 将差异摘要初始化为“选择两个版本后，比较结果会显示在这里。”，避免比较前大面积空白；比较命令、DTO、绑定和安全语义未改变。
- Playnite UI 源码测试锁定关注事项列宽/右对齐契约，以及比较页气泡回退文本和初始说明；未增加 Demo 文件夹、环境变量或伪造生产数据依赖。

**验证结果：**

- `scripts/validate-source.py`：通过；`validate_wpf_ui.py`：0 error、20 warnings、164 info，warning/info 为既有上下文提示。
- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/attention-pill-fix`：XAML 18/18、0 警告/0 错误；Core 59、Worker 194、Playnite 276 通过/58 跳过/0 失败。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/attention-pill-fix`：`render-qa OK`，双主题、1040/1100/1366/2560、多 Tab、滚动和 resize transition 通过；离屏 RenderHarness 证据不等同真实 Playnite 宿主逐像素验收。

## 2026-08-21 UI-292 修复设置路径输入框裁切并移除生产壳主题栏

**问题确认：**

- 设置页核心工具路径使用的 WPF-UI TextBox 模板把内容宿主按理想高度居中；固定 36 DIP 高度下，长路径又启用了自动水平滚动条，滚动条占用底部空间后文字视口只剩约 13 DIP，导致截图中的文字上下被裁切。
- 生产壳顶部 44 DIP utility surface 只显示“主题 / 跟随 Playnite / 浅色 / 深色”，与当前页面需求不符；主题设置本身仍应保留在 Playnite 设置页。

**实现内容：**

- `WpfUiProduction.xaml` 的共享 TextBox 模板改为拉伸内容宿主、垂直居中正文，默认隐藏水平滚动条；`DesignTokens.xaml` 和 WPF-UI 适配器收紧垂直内边距，避免 36 DIP 输入框再次裁切。
- 设置页核心路径字段移除显式自动水平滚动条，保留文本框获得焦点后的横向滚动与编辑能力；所有真实 Binding、UpdateSourceTrigger、ToolTip 和保存语义保持不变。
- `AcrylicProductionShellView` 删除顶部主题栏，Header 上移到原 utility surface 的位置；同步移除壳主题按钮样式、回调和死代码。设置页的“界面主题”下拉框、动态调色板和 Playnite 主题设置仍保留。
- Playnite UI 测试增加长路径 `PART_ContentHost` 实测断言，并锁定生产壳不再包含主题栏。

**验证结果：**

- `scripts/validate-source.py`：通过；`validate_wpf_ui.py`：0 error、20 warnings、164 info，均为既有上下文提示。
- Release 构建：XAML 18/18、0 警告/0 错误；Core 59、Worker 194、Playnite 276 通过/58 跳过/0 失败。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/settings-theme-fix-final`：`render-qa OK`，覆盖双主题、1040/1100/1366/2560、多 Tab 和 resize transition；设置输入内容视口探针为 21 DIP、正文高度 16 DIP。

## 2026-08-21 BUILD-003 修复测试根目录被外部 Demo 工作目录劫持

**问题确认：**

- 用户的一键安装日志中，编译、Core/Worker 测试均成功，Playnite 测试的 5 个失败全部是 `DirectoryNotFoundException`；失败路径为 `D:\workplace\Github\GameSaveCenter.AcrylicFork\src\GameSaveCenter.Playnite\Design\*.xaml`。
- 测试并没有继续读取 Demo 资源；真正的问题是 `WpfUiResourceDictionaryTests` 优先从 `Directory.GetCurrentDirectory()` 向上找 `GameSaveCenter.sln`，安装器启动目录恰好是 AcrylicFork，于是把外部仓库误认成当前仓库。两个相同模式的测试根目录探测器也一并修正。

**实现内容：**

- `WpfUiResourceDictionaryTests`、`RestoredAcrylicForkBaselineTests` 和 `NumericInputTests` 只从 `AppContext.BaseDirectory`（当前测试程序集所在的隔离构建输出）向上定位仓库；找不到时明确失败，不再从启动目录猜测其它仓库。
- `RestoredAcrylicForkBaselineTests` 和 `NumericInputTests` 同样改为从测试程序集目录向上查找，避免任何测试因启动目录误选外部仓库。
- 未引入 Demo 路径、环境变量或外部文件依赖；只读取当前仓库源码资源。

**验证结果：**

- 在 `D:\workplace\Github\GameSaveCenter.AcrylicFork` 作为当前工作目录时，直接运行当前仓库 Playnite 测试：276 通过、58 跳过、0 失败。
- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/no-demo-root-fix`：XAML 18/18、Release 0 警告/0 错误、Core 59/59、Worker 194/194、Playnite 276/58/0。

## 2026-08-21 UI-291 媒体来源规则宽窄布局

**实现内容：**

- 来源规则页改为 `MediaSourceLayout`：宽屏使用“来源表单 + 规则列表”双栏，窄屏按“表单 → 规则列表”堆叠；表单默认可见，不再用折叠 Expander 隐藏真实入口。
- 保留 `CustomMediaSourcePath`、`CustomMediaPattern`、`CustomMediaShared`、添加/更新/移除命令，以及规则列表的内部滚动、虚拟化和空状态。
- 窄屏表单字段收为单列并启用局部滚动，宽屏保持两列；媒体待归类页的预览、目标游戏、归类/忽略/批量操作未迁移或删减。

**验证结果：**

- `scripts/validate-source.py`：通过；XAML 18/18；Release 0 警告/0 错误；Core 59/59、Worker 194/194、Playnite 276 通过/58 跳过/0 失败。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/ui-final-audit`：`render-qa OK`，覆盖 1040/1100/1366/2560、Light/Dark 和 resize transition。
- `validate_wpf_ui.py`：0 error、20 warnings、164 info；RenderHarness 为离屏证据，不等同真实 Playnite 宿主逐像素验收。
- 测试与构建只使用当前仓库文件，不依赖另一台机器上的 AcrylicFork Demo 文件夹。

## 2026-08-21 UI-290 首页 Demo-first 信息层级

**实现内容：**

- 首页 `OverviewPrimaryFlow` 现在按“TODAY/当前游戏 → 六项指标 → 最近任务/全局活动 → 风险与提醒”的顺序排布；风险 rail 的宽屏并列和窄屏后置行为保持不变。
- “今日工作台”批量按钮与首次环境检查提示仍保留真实绑定，但下移为辅助操作区，不再占据首页第一视觉层级。
- 首页原有当前游戏、Snapshot 指标、任务列表、风险操作、滚动与响应式代码未替换为 Demo 数据或 Mock 控件。
- 将对应顺序测试从历史基线跳过项恢复为普通 Fact，明确锁定 Hero/metrics/toolbar/activity 的行契约。

**验证结果：**

- `scripts/validate-source.py`：通过；Release 0 警告/0 错误；Core 59/59、Worker 194/194、Playnite 276 通过/58 跳过/0 失败。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/overview-order-ui288`：`render-qa OK`，覆盖 1040/1100/1366/2560、Light/Dark、resize transition、工作区滚动和表格探针。
- RenderHarness 属于离屏证据，不等同真实 Playnite 宿主的逐像素验收。

## 2026-08-21 UI-289 普通与紧凑共享控件尺寸

**实现内容：**

- `DesignTokens.xaml` 将普通按钮、文本输入框、数值输入框和 ComboBox 的共享高度收敛为 `GscButtonHeight=36`，新增 `GscCompactButtonHeight=30`。
- `WpfUiProduction.xaml` 的 `GscWpfUiCompactButton` 改为 30 DIP；普通 WPF-UI TextBox 的固定 42 DIP 高度改为动态引用普通控件令牌，避免主题/资源链分叉。
- 首页与存档页的紧凑按钮删除显式 `MinHeight=38` 覆盖，保留按钮宽度、命令、绑定和高风险操作的原有特殊高度。
- 激活共享尺寸源码测试，加入普通 36 / 紧凑 30 的 STA 度量和资源契约，修正旧的 38/42 DIP 测试断言。

**验证结果：**

- `scripts/validate-source.py`：通过；`validate_wpf_ui.py`：0 error、20 warnings、164 info，警告均为已有有限视口/参考控件提示。
- Release 构建：0 警告/0 错误；Core 59/59、Worker 194/194、Playnite 275 通过/59 跳过/0 失败。

## 2026-08-21 UI-288 生产壳主题操作与真实设置入口

**问题确认：**

- 生产壳右侧顶部保留了 44 DIP 的空白带，只是占位，无法完成 Demo 所要求的主题操作；侧栏也没有生产版可用的设置入口。
- 旧测试把设置导航当作“占位项”禁止，但实际设置入口已经存在于 Playnite 的 `OpenPluginSettings` 路由，应该复用该真实入口而不是复制 Demo 设置页。

**实现内容：**

- 将顶部空白带改为共享资源驱动的主题 utility surface，提供“跟随 Playnite / 浅色 / 深色”三个真实 `ThemeMode` 选项；切换后保存现有插件设置、通知调色板刷新，并保持 Light/Dark 动态资源链。
- 侧栏维护中心后增加真实“设置”导航项；点击时恢复当前工作区选中状态，再调用 `plugin.PlayniteApi.MainView.OpenPluginSettings(plugin.Id)`，不改变工作区 ViewModel、命令或设置页内部分类 Tab。
- 主题按钮样式放入 `AcrylicProductionResources.xaml` 的共享资源，保留键盘/Automation 名称和高对比度动态前景资源。

**验证结果：**

- `scripts/validate-source.py`：通过。
- `scripts/build.ps1 -OutputRoot artifacts/gsc-b/ui-shell`：XAML 18/18、Release 0 警告/0 错误、Core 59/59、Worker 194/194、Playnite 274 通过/60 跳过/0 失败。
- 本阶段未依赖 AcrylicFork Demo 文件夹；源码和测试只使用当前仓库生产资源。


## 2026-08-21 BUILD-002 移除跨电脑测试对 Demo 目录的运行时依赖

**问题确认：**

- 失败发生在 Playnite 测试阶段，五个共享控件测试仍然读取外部 AcrylicFork Demo 文件；另一台机器没有 `GameSaveCenter.AcrylicFork` 兄弟目录时抛出 `DirectoryNotFoundException`。
- 之前新增的“缺少 Demo 则跳过”路径仍把 Demo 当作默认测试输入，且失败日志对应的隔离构建产物仍会进入外部文件读取，不能解决普通跨电脑构建的可复现性。

**实现内容：**

- 删除 `AcrylicForkDesignSource`、`AcrylicForkDesignFactAttribute` 以及五个测试中的外部 Demo 文件读取。
- 五个测试恢复为普通 `[Fact]`，只读取当前仓库内的 `DesignTokens.xaml`、`WpfUiProduction.xaml`、`AdaptiveThemePalette.cs` 和设置页资源；Demo 仍是设计基准，但不再是测试运行时依赖。
- 未修改生产 UI、Worker 业务实现、真实命令、绑定、数据契约或任何页面结构。

**验证结果：**

- 设置 `GSC_ACRYLICFORK_ROOT=D:\nonexistent\GameSaveCenter.AcrylicFork` 模拟无 Demo 机器，定向共享资源测试 9 通过、2 跳过、0 失败。
- 同一无 Demo 环境下完整 Playnite 测试：274 通过、60 跳过、0 失败，共 334 项；Release 编译及测试产物均来自隔离输出目录。

## 2026-08-21 UI-287 修复共享表格排序箭头和列宽拖拽

**问题确认：**

- 生产共享表格把 `CanUserResizeColumns` 打开了，但自定义 `DataGridColumnHeader` 模板遗漏 WPF 必需的左右 `Thumb` 部件，列宽拖拽命中区域实际上不存在。
- 排序箭头放在 14 DIP 固定列中，箭头路径和右侧留白超过可用空间，导致截图中只显示半个箭头；首页的兼容表头还使用了另一套没有独立箭头列的布局。

**实现内容：**

- 共享 `WpfUiProduction.xaml` 表头补回透明的 `PART_LeftHeaderGripper`/`PART_RightHeaderGripper`，共享 DataGrid 保留列排序并设置 Demo 同级的 64 DIP 最小列宽；Dashboard 本地表头同步使用 22 DIP 排序区。
- 排序箭头改为独立 22 DIP 列，保留现有主题 Accent 颜色、字体、表头边框和选中/虚拟化语义；未修改任何真实列绑定、命令、排序数据源或当前滚动条系统。
- 新增 Playnite 资源契约断言；RenderHarness 对每个表格运行时检查表头 Thumb、命中区和排序箭头布局，避免只靠 XAML 字符串判断。

**验证结果：**

- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/ui287-table-header-v2`：XAML 18/18；Release 构建 0 warning、0 error；Core 59/59；Worker 194/194；Playnite 274 通过、60 跳过、0 失败。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/ui287-table-header-v2`：`render-qa OK`；双主题、1040/1100/1366/2560、多页面 Tab、滚动和 resize transition 通过，表头 resize/sort 运行时门禁通过。
- `scripts/validate-source.py` 通过；`validate_wpf_ui.py` 0 error、20 warnings、164 info，提示均为既有共享资源、有限测量和示例资源审计项。本阶段未运行真实 Playnite 生产宿主。

## 2026-08-21 UI-286 修复修改器 EXE 导入与旧版 FLiNG 归档搜索

**问题确认：**

- `Outlast 2 v1.0-Update 2 Plus 4 Trainer.exe` 的文件名包含 `Update`；旧扫描器把文件名包含 `update` 的所有 EXE 当成更新辅助程序，导致显式导入时错误提示“未在导入内容中找到 .exe”。
- 拖放导入请求从 Worker/IPC 线程返回后，候选集合、选中项和状态直接更新；`ImportEntryCandidates` 绑定 WPF `CollectionView`，会触发“CollectionView 不支持从调度程序线程以外的线程修改 SourceCollection”。
- FLiNG 在线目录原本只同步当前 `flingtrainer.com` 页面，没有把用户指出的 `archive.flingtrainer.com` 历史目录加入搜索；现有下载器已能安全处理 ZIP/EXE，但不是 RAR/7z。

**实现内容：**

- 将“显式选中的单文件”与“目录/ZIP 候选过滤”分离；仅排除 `unins*`、`uninstall`、`update`、`updater`、`setup` 这类明确辅助入口，保留带版本描述的 `...-Update ... Trainer.exe`。新增真实 EXE 导入和 ZIP 内入口回归测试。
- 候选集合、选中项、清理状态和导入后工具选择统一通过 Playnite Dispatcher 应用；命令、绑定、复制到工具库、哈希和安全语义不变。
- 刷新 FLiNG 目录时可选同步历史归档根目录；归档 ZIP/EXE 以可搜索条目展示，读取版本直接生成一个下载版本并复用现有 HTTPS 校验、大小限制、安全 ZIP 解压和入口选择。归档站暂时不可用时保留当前在线目录，不覆盖缓存。
- 归档结果在目录列表中显示“FLiNG 归档”，避免与当前在线条目混淆；未执行用户提供的未知 EXE。

**验证结果：**

- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/ui286-trainer-import-v2`：XAML 18/18；Release 构建 0 warning、0 error；Core 59/59；Worker 194/194；Playnite 273 通过、60 跳过、0 失败。
- 新增导入/归档测试通过：含 `Update` 文件名的 EXE 可检查并实际导入，ZIP 内同名入口可识别，归档目录链接可转换为可搜索条目；拖放候选更新具备 Dispatcher 源码门禁。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/ui286-trainer-import-v1`：`render-qa OK`；双主题、1040/1100/1366/2560、多页面 Tab、滚动和 resize transition 均通过，Trainer 导入确认 ComboBox 仍有 4 个候选项。
- `scripts/validate-source.py` 通过；`validate_wpf_ui.py` 0 error、20 warnings、164 info；warnings/info 为既有共享资源、有限测量和示例资源审计提示。本阶段未运行真实 Playnite 生产宿主。

## 2026-08-21 UI-285 修复媒体待归类表格反向滚动偶发空白

**问题确认：**

- UI-283 已经消除了大数据表格从顶部向下拖动时的明显空白，但用户继续反馈到达底部后向上拖动，偶发出现表头和滚动条仍在、行呈现器却全部空白的状态。
- `MediaDataGrid` 通过共享隐式 `DataGrid` 样式继承了 `infra:DataGridStarFill.Enabled=True`。该 workaround 面向被无限测量的表格，会在有限 Grid 视口中把星号列改成像素宽度并触发 `InvalidateMeasure`；大数据 Standard 虚拟化在反向滚动期间可能因此丢失已实现行。
- 既有 RenderHarness 只按 0/25/50/75/100% 单向取样，不能覆盖用户的“到底后回翻”路径。

**实现内容：**

- 仅对有限视口的 `MediaInboxGrid` 设置 `infra:DataGridStarFill.Enabled=False`，恢复 WPF 原生有限视口星号列测量；保留 `VirtualizationMode=Standard`、`ScrollUnit=Item`、行虚拟化、列虚拟化关闭、表头/列宽/排序/选中态和右侧 Inspector。
- RenderHarness 为媒体收件箱增加 0→100→0→100→50→0→100→0 的大数据反向滚动序列，逐步检查已实现行、DataContext、表头间距、最后回翻位置，并门禁共享 star-fill 不得重新启用。
- 未改动 `UnassignedMedia`、`SelectedInboxMedia`、媒体预览、归类/忽略命令或真实安全语义；其他 Save/Task/Maintenance 表格仍使用共享 star-fill/虚拟化契约。

**验证结果：**

- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/ui285-media-scroll-v1`：XAML 18/18；Release 构建 0 warning、0 error；Core 59/59；Worker 191/191；Playnite 272 通过、60 跳过、0 失败。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/ui285-media-scroll-v3`：`render-qa OK`；4468 条数据在多尺寸、浅/深主题和反向滚动序列中均保持实现行，无空白回翻状态。
- `scripts/validate-source.py`、`validate_wpf_ui.py`（0 error、20 warnings、164 info）通过；WPF warnings/info 为现有共享资源和有限测量审计提示。本阶段仍未运行真实 Playnite 生产宿主。

## 2026-08-21 UI-284 修复主题前景色、按钮几何与设置侧栏

**问题确认：**

- 生产共享按钮的内容模板没有把按钮的 `Foreground`、字体族和字阶传给内部 `TextBlock`；按钮套用 Primary 外观时文字会回落为黑色，在深色主题上不可读，并会放大多个页面的按钮错位/溢出观感。
- 设置页分类栏仍引用旧的 `LabSegmented` 灰色填充；风险操作按钮由 `WrapPanel` 自适应排列，两个按钮在窄宽下高度不一致；需关注事项底部维护入口也缺少 Demo 同级的按钮表面。
- 比较与保留页的“估算比较（缺少完整 Hash）”状态气泡缺少稳定尺寸约束，标题行对齐会随内容测量变化。

**实现内容：**

- `GscWpfUiButtonTextTemplate` 改为继承宿主 Button 的动态 `Foreground`、`FontFamily`、`FontSize` 和 `FontWeight`，修复所有共享按钮的主题文字颜色；风险操作区改为固定按钮高度的水平布局，需关注事项保持 Demo 的真实分隔列表和 `SuggestedAction` 绑定，底部维护入口使用带背景/描边的 Secondary 按钮。
- 新增 `GscSettingsSectionTabs`/`GscSettingsSectionTabItem`，使用主题动态画刷、选中态和 UI 字体，设置页改用该样式，移除旧灰色整块背景。
- 比较页状态气泡增加固定高度、内边距、最大宽度和像素对齐约束；同步更新源码门禁与设置样式测试契约。
- 未添加 Mock Finding、未恢复 Demo 中不存在的 checkbox/逐项按钮，真实绑定、命令、安全语义、滚动和虚拟化保持不变。

**验证结果：**

- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/ui284-theme-v1`：XAML 18/18；Release 构建 0 warning、0 error；Core 59/59；Worker 191/191；Playnite 272 通过、60 跳过、0 失败。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/ui284-theme-v1`：`render-qa OK`；双主题、1040/1100/1366/2560 多尺寸、滚动和 resize transition 均通过；Overview 风险/关注区、Settings 侧栏、Save 比较页气泡已抽查。
- `scripts/validate-source.py`、`validate_wpf_ui.py`（0 error、20 warnings、164 info）和 `git diff --check` 通过。WPF warnings/info 为现有共享资源与测量审计提示；本阶段仍未运行真实 Playnite 生产宿主，不能宣称 Demo-first 总迁移完成。

## 2026-08-21 UI-283 修复媒体待归类大数据滚动空白

**问题确认：**

- 用户反馈媒体中心“待归类”在约 4468 条数据时向下拖动滚动条，表头与实际行之间出现大段空白；真实收件箱上限由 `DashboardViewModel.Media.cs` 的 `Limit = 5000` 约束。
- 媒体收件箱此前继承共享 `DataGrid` 的 `Recycling` 行回收和列虚拟化，同时使用星号文本列；这组组合在大数据量、拇指拖动后存在 WPF 虚拟化呈现器把已实现行放到视口底部的回归风险。

**实现内容：**

- `MediaDataGrid` 保留共享 DataGrid 的表头、行圆角选中态、`Item` 滚动、行虚拟化、排序/列宽调整和顶部内容对齐；仅对收件箱局部覆盖为 `VirtualizationMode=Standard`、`EnableColumnVirtualization=False`，避免大数据量下的回收/列虚拟化组合。
- 未改动 `UnassignedMedia`、`SelectedInboxMedia`、预览 Inspector、`AssignInboxMediaCommand`、`IgnoreInboxMediaCommand`、目标游戏选择和真实归类/忽略安全语义；媒体当前游戏列表和生产滚动条系统也未迁移。
- RenderHarness 的媒体收件箱探针扩展到 4468 条，并在 2400 DIP 宽度下验证首行、中段、末尾的 realized 行与视口几何；探针契约改为按表格显式期望 `Standard`/关闭列虚拟化，其他工作区仍要求共享 `Recycling`/列虚拟化。

**验证结果：**

- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/ui283-media-inbox-v1`：XAML 18/18；Release 构建 0 warning、0 error；Core 59/59；Worker 191/191；Playnite 272 通过、60 跳过、0 失败。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/ui283-media-inbox-v5`：`render-qa OK`；Light/Dark、1040/1100/1366/2560、多尺寸滚动、末尾回滚和 resize transition 均通过；4468 条媒体探针各位置 `gap=0`，未出现正向空白间隔。
- `scripts/validate-source.py`、`validate_wpf_ui.py`（0 error、20 warnings、164 info）和 `git diff --check` 通过；WPF warnings/info 为现有共享资源与测量审计提示。
- 本阶段仍是离屏 RenderHarness/静态验证，未取得新的可识别 Playnite 生产宿主截图；不能把本阶段写成 Demo-first 总迁移完成。

## 2026-08-21 UI-282 按 Demo 重构首页风险/关注区与比较保留页

**问题确认：**

- 小屏首页的堆叠根因是 `OverviewActivityColumn` 横跨内层 Flow 的第 3、4 行，而紧凑右栏仍被放到第 4 行，WPF 因此把风险卡与全局活动安排在同一测量区域；不是单纯的透明度或边距问题。
- 首页风险区源码仍残留生产版安全提示、复选框、逐项“查看”和第二套明细模板，和 Demo 的紧凑无复选框卡片不一致；需关注事项也把 Demo 的右侧建议文字做成了按钮。
- 比较与保留页之前仍是历史 Inspector 结构，缺少 Demo 的一个横向可滚动画布、两张对等卡片、真实差异文件清单和“建议保留/候选清理”清单。

**实现内容：**

- 紧凑首页把右栏移到 `OverviewPrimaryFlow` 的专用第 5 行，活动列继续独占第 3、4 行；风险和关注卡按先活动后右栏的顺序排列，消除小屏空间堆叠。
- `OverviewProtectionPreviewItems` 改为 Demo 密度的多选 `ListBox`：只显示状态点、游戏名、状态 Chip 和原因，不渲染 Checkbox 或逐项按钮；选中卡仍通过 `OnProtectionSelectionChanged` 调用真实 `OpenProtectionItemCommand`，底部 `OpenProtectionGamesCommand`、`ApplyRecommendedProtectionCommand`、确认和快照安全语义保持。
- 删除旧的 `OverviewLegacyRiskContent`、`OverviewProtectionDetails` 及其第二套复选框/查看模板；风险卡只保留一个有限列表视口。`AttentionFindings` 行改成 Demo 的图标、标题、游戏名和 `SuggestedAction` 文字，移除自定义空状态段落和每行“查看原因”按钮。
- 比较与保留页改为 `SaveComparePageScrollViewer` 包住的 `MinWidth=880` 横向画布，两个 `GscReadingCardStyle` 对等排列；左卡展示真实三类差异计数和文件清单，右卡展示真实保留预览、建议保留、候选清理和安全说明，窄宽通过页面横向滚动保持结构而不堆叠错位。
- 更新首页/存档页静态契约，明确 Demo 无复选框和逐项按钮、关注项使用建议文字，以及比较页必须使用横向画布和真实清单绑定。

**验证结果：**

- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/ui282-demo-structure-v4`：XAML 18/18；Release 0 warning、0 error；Core 59/59；Worker 191/191；Playnite 272 通过、60 跳过、0 失败。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/ui282-demo-structure-v2`：`render-qa OK`；Light/Dark、1040/1100/1366/2560、多尺寸滚动探针和 resize transition 均通过。1040×700 报告中活动列表位于 `y=507`，风险卡列表和底部动作位于后续独立区域，未再与活动行重叠；比较页保留横向 ScrollViewer，`MinWidth=880` 结构在窄宽可滚动。
- `scripts/validate-source.py`、`validate_wpf_ui.py`（0 error、20 warnings、164 info）和 `git diff --check` 通过；WPF warnings 是现有滚动测量、负 Margin 和共享资源审计提示，不是本阶段构建错误。
- 证据仍来自离屏 RenderHarness 和静态门禁，未取得新的可识别 Playnite 生产宿主逐页截图；本阶段只收口上述首页/存档页问题，不宣称 Demo-first 总迁移或真实宿主逐页验证已完成。

## 2026-08-21 UI-281 修复首页风险/关注区与共享按钮几何

**问题确认：**

- 首页风险区为了隐藏重复的生产摘要而误把真实 `RecentProtection` 明细列表放进了折叠祖先，渲染后只剩很薄的空白区；需关注事项还缺少 Demo 的紧凑业务行。
- `GscWpfUiButton` 的空白按钮背景与 `ContentPresenter` 是并列元素，按钮内边距没有参与内容测量，导致多个页面的中文按钮文字贴边或溢出。
- 存档中心比较与保留页误用历史 Inspector 的窄列宽；备份策略页多个标签/开关使用隐式 Grid 单元格，窄宽下出现错位。

**实现内容：**

- 共享 `GscWpfUiButton` 将 Hover/Pressed 状态层和 `ContentPresenter` 放入带 Padding 的按钮外壳，修复所有复用该模板的主按钮/操作按钮几何；真实 Command、Binding、焦点态和禁用态保持。
- 首页风险区只隐藏重复的摘要图标/标题/说明，恢复真实保护明细列表、选择框、状态点/状态气泡、逐项查看和批量保护命令；首页最近任务与全局活动移除鼠标悬停变色，只保留选中态和键盘焦点语义。
- 需关注事项增加真实 `AttentionFindings` 的紧凑空状态容器，保留实际 Finding 绑定和维护中心命令；不伪造或补入 Demo Mock 数据。
- 存档中心比较与保留改为 Demo 风格的两张对等卡片，比较/刷新动作移到标题行并使用紧凑按钮；备份策略与云端上传设置改用显式 `*`/`Auto` 列确保标签、Chip、Toggle、输入框和 ComboBox 对齐。

**验证结果：**

- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/ui281-button-risk-v4`：XAML 18/18；Release 0 warning、0 error；Core 59/59；Worker 191/191；Playnite 272 通过、60 跳过、0 失败。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/ui281-button-risk-v3`：`render-qa OK`，Light/Dark、1040/1100/1366/2560、多尺寸滚动探针和 resize transition 均通过；风险/保护/关注 ScrollViewer 均有正确的非零视口，比较与保留、备份策略页抽查无按钮溢出或错位。
- `scripts/validate-source.py`、`validate_wpf_ui.py`（0 error、19 warnings、164 info）和 `git diff --check` 通过；warnings/info 为既有滚动测量、负 Margin 和共享资源审计提示。
- 证据仍来自离屏 RenderHarness 与静态门禁，未取得新的可识别 Playnite 生产宿主逐页截图；不能把本阶段证据写成 Demo-first 总迁移完成，也不能宣称已验证真实宿主的主题切换、DPI、键盘焦点和命令执行。

## 2026-08-21 UI-280 首页 Demo 结构与选中态修正

**问题确认：**

- `OverviewView.xaml` 的六项统计卡曾把分隔线放在与统计卡相同的 Grid 列中，导致竖线穿过卡片内容；最近任务、全局活动和状态气泡也比 Demo 使用了更大的字阶/控件高度。
- 生产风险区额外保留了安全提示子卡、云端子卡和“展开最近游戏保护明细” Expander；需关注事项仍使用 34 DIP 大图标块，与 Demo 的紧凑行不同。
- 共享 `DataGridRow`/媒体行和首页任务选中态没有采用修改器中心 Trainer 列表的 14 DIP 圆角选中卡效果。

**实现内容：**

- 统计区改为 11 列交替结构（6 个星号统计列 + 5 个 Auto 分隔列），保留六个真实 `Snapshot` 绑定和比例进度条。
- 首页新增 Demo 字体别名，最近任务/全局活动/风险/需关注事项使用 15/12.5/10.5/12 DIP 的 Demo 密度；任务和活动状态统一复用紧凑 `GscRedesignTableStatusPill`，共享 ToolTip 补齐 UI 字体、12 DIP 字阶、7 DIP 内边距和 4 DIP 偏移。
- 风险区取消额外 Expander 和嵌套卡表面，保护明细直接显示为两行紧凑卡；复选框、逐项查看、底部“查看需要处理的游戏”和“启用所选保护”命令、确认/快照安全语义均保留。安全说明改为批量保护按钮 ToolTip，不再占用 Demo 没有的警示卡层级。
- 需关注事项改为 18 DIP 图标标记、紧凑标题/游戏名和透明背景的“查看原因”动作；生产 DataGrid 共享行模板与 Dashboard 兼容行模板的选中态均对齐 Trainer 的 Accent 填充、Accent 描边和 14 DIP 圆角卡片。
- 同步更新首页布局回归契约，锁定保护明细无额外 Expander、共享圆角 DataGrid 行模板和紧凑关注项几何。

**验证结果：**

- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/ui-overview-home-v4`：XAML 18/18；Release 0 warning、0 error；Core 59/59；Worker 191/191；Playnite 272 通过、60 跳过、0 失败（新增首页结构/选中态契约正常执行）。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/ui-overview-home-v2`：`render-qa OK`，七页、Light/Dark、1040/1100/1366/2560 DIP、多尺寸滚动探针和 resize transition 均通过；首页保护列表仍保持有限内部滚动，页面横向滚动为 Disabled。
- `scripts/validate-source.py`、`validate_wpf_ui.py`（0 error、19 warnings、164 info）和 `git diff --check` 通过；warnings/info 为既有滚动测量、负 Margin 和共享资源审计提示，本轮未新增错误。
- 本阶段没有取得新的可识别 Playnite 生产宿主逐页截图；离屏 RenderHarness 与静态门禁不能替代宿主 Light/Dark/Follow、DPI、键盘焦点和真实命令人工验收，Demo-first 总迁移仍未完成。

## 2026-08-21 UI-279 修复修改器中心窄宽导入工具栏裁切

**实现内容：**

- 发现 Trainer 在约 744 DIP 工作区（1040×700 RenderHarness）中，四个真实导入按钮仍挤在“当前游戏工具”标题行的自动列，最右侧“+ 添加启动项”超出可视边界，命中区不可靠。
- `TrainerCenterView.xaml` 为标题、导入工具栏和拖放提示增加独立行；`TrainerCenterView.xaml.cs` 在 `<980 DIP` 时将四个导入命令移到标题下方，并让拖放提示顺延到第三行，宽屏仍保持 Demo 的标题/工具栏同排节奏。
- 保留 `ImportTrainerCommand`、`ImportToolFolderCommand`、`ImportCheatTableCommand`、`ImportCustomLaunchItemCommand`、导入确认、工具列表、Inspector、ScrollViewer 和 Recycling 虚拟化；新增 `RestoredAcrylicForkBaselineTests.TrainerImportToolbarReflowsBeforeTheCompactViewportCanClipItsCommands` 契约测试。

**验证结果：**

- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/ui279-trainer-toolbar-v1`：XAML 18/18；Release 0 warning、0 error；Core 59/59；Worker 191/191；Playnite 268 通过、62 跳过、0 失败。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/ui279-trainer-toolbar-v1`：`render-qa OK`，覆盖七页、Light/Dark、1040/1100/1366/2560 DIP、滚动探针和 resize；浅/深色 Trainer 1040×700 截图确认四个导入按钮全部可见。
- `scripts/capture-ui-audit.ps1 -Configuration Release -Output artifacts/ui-audit-ui279-trainer-toolbar-v1`：Fidelity 0、HIGH 0、失败路由 0；仍有 4 个现有 `TOOLBAR_VERTICAL_EXPANSION` MEDIUM，来源是已选工具 Inspector 内五项开关/启动延迟的必要 WrapPanel 换行，不是导入按钮裁切。
- `scripts/validate-source.py`、WPF 质量校验（0 error、19 warnings、164 info）和 `git diff --check` 通过。以上仍为离屏/静态证据，真实 Playnite Dashboard 证据未补齐。

## 2026-08-21 UI-278 修正双主题 RenderHarness 宿主背景

**实现内容：**

- 发现 RenderHarness 的多个页面宿主 Grid 将背景写死为深色 `#181E2B`；强制浅色主题时，页面已经注入 Demo 浅色色板，但文字仍被错误地渲染在深色画布上，Trainer 顶部工具/导入区因此出现假性的低对比。
- 新增 `CreateHarnessBackground`，页面宿主优先读取当前页面的 `GscBackdropBrush`，未主题化的历史探针继续回退原深色背景；覆盖主题 QA、Tab 页面、单页页面、滚动探针、布局探针和 resize 审计。
- 新增 `ThemeQaHostUsesThePageBackdropResource` 源码回归断言；没有改动生产 View、命令、绑定、滚动条、虚拟化或项目 Tab chrome。

**验证结果：**

- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/ui278-themed-host-v1`：RenderHarness Release 0 warning、0 error；`render-qa OK`，覆盖七页、Light/Dark、1040/1100/1366/2560 DIP、滚动探针和 resize transition。
- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/ui-278-themed-host-v1`：XAML 18/18；Release 0 warning、0 error；Core 59/59；Worker 191/191；Playnite 267 通过、62 跳过、0 失败；`scripts/validate-source.py` 通过；WPF 质量校验 0 error、19 warnings、164 info。
- 重新抽查 `theme/light/Trainer-1040x700.png`、`theme/light/Save-1366x768.png` 与 `theme/dark/Trainer-1040x700.png`：浅色画布、Tab、Trainer 导入/工具区和深色主题层级均可读；之前的低对比来自审计宿主背景，不是 Trainer 生产页面表面。
- 以上仍是离屏/静态证据，不能替代可识别 Playnite Dashboard 宿主中的主题切换、键盘焦点、拖放和真实命令验收；真实 Dashboard 嵌入证据仍未补齐。

## 2026-08-21 UI-277 修复共享折叠栏标题对比度

**实现内容：**

- 发现 Task 在 1040×700 窗口的实际 744 DIP 工作区进入“更多筛选”状态时，`GscDisclosureCard` Header 处于透明深色画布上，浅色主题的深色文字与背景对比不足，截图中只剩 Chevron，标题无法可靠识别。
- 共享 `GscDisclosureCardExpander` 现在使用 `GscControlFillBrush` 作为 Header 表面，并将 Expander 的背景/边框绑定传入 Header ToggleButton 与 `HeaderChrome`；浅/深主题都获得可见的卡片边界和标题对比度。
- 保留 Demo `LabDisclosure` 的整行命中、主题 tint hover/expanded 状态、150ms Chevron 展开/收起动效、真实 Expander `Header`/`Content` 和 Task `TaskGameFilter` 绑定，没有改动页面滚动或项目 Tab chrome。

**验证结果：**

- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/ui-277-disclosure-surface-v1`：XAML 18/18；Release 0 warning、0 error；Core 59/59；Worker 191/191；Playnite 266 通过、62 跳过、0 失败。
- `scripts/validate-source.py`、`validate_wpf_ui.py`（0 error、19 warnings）和 `git diff --check`：通过；共享折叠栏资源契约测试通过。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/ui277-disclosure-surface-v1`：`render-qa OK`，覆盖七页、Light/Dark、1040/1100/1366/2560 DIP、滚动探针和 resize transition；抽查 Task 1040×700 双主题截图，标题和卡片表面均可见。
- 以上仍是离屏/静态证据，不能替代可识别 Playnite Dashboard 宿主中的折叠栏鼠标、键盘焦点、Escape 与真实筛选绑定验收；真实 Dashboard 嵌入证据仍未补齐。

## 2026-08-21 UI-276 修复媒体当前页窄宽操作区重叠

**实现内容：**

- 修复 `MediaCenterView` 当前游戏媒体页在 Playnite 实际工作区宽度约 744 DIP（窗口 1040 DIP）时的操作区重叠：原批量操作 `WrapPanel` 与紧凑 Inspector 的“查看媒体详情”按钮共享同一 Grid 单元格，导致按钮互相覆盖、命中区不可靠。
- 新增 `MediaCurrentActionRow` 的双行/双列响应式契约：宽屏保持提示、批量操作同一行；窄屏提示独占第一行，批量操作与媒体详情抽屉按钮分列第二行。`FavoriteSelectedMediaCommand`、`UnfavoriteSelectedMediaCommand`、`CommentSelectedMediaCommand`、`MediaCompactDetailsButton`、Inspector 和媒体选择绑定均保留。
- 只调整页面操作区的布局测量，没有改变媒体预览、异步缩略图、虚拟化 ListBox、滚动条、Inspector 内容或任何真实命令。

**验证结果：**

- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/ui-276-media-actions-v1`：XAML 18/18；Release 0 warning、0 error；Core 59/59；Worker 191/191；Playnite 266 通过、62 跳过、0 失败。
- `scripts/validate-source.py`、`validate_wpf_ui.py`（0 error、19 warnings）和 `git diff --check`：通过；新增媒体响应式断言定向测试 1/1 通过。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/ui276-media-actions-v1`：`render-qa OK`，覆盖七页、Light/Dark、1040/1100/1366/2560 DIP、滚动探针和 resize transition；抽查媒体页 1040×700 双主题及 1366×768 浅色截图，操作按钮已分离且未见覆盖。
- 以上仍是离屏/静态证据，不能替代可识别 Playnite Dashboard 宿主中的真实媒体选择、批量收藏/备注、Inspector 抽屉键盘焦点和 DPI 验收；真实 Dashboard 嵌入证据仍未补齐。

## 2026-08-21 UI-275 对齐 Demo 滑杆几何

**实现内容：**

- 唯一生产滑杆 `GscSlider` 对齐 Demo `LabSlider` 的 22 DIP 控件高度、4 DIP 轨道和 18 DIP 滑块，保留生产主题自适应填充、边框、阴影、悬停/拖动状态及 `GlassStrengthSlider` 的真实双向值绑定。
- 仅调整共享模板测量，不改变设置页布局、低高度页面滚动、键盘焦点、拖动命令或任何页面 Tab/ScrollViewer 实现。

**验证结果：**

- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/ui-275-slider-v1`：XAML 18/18；Release 0 warning、0 error；Core 59/59；Worker 191/191；Playnite 265 通过、62 跳过、0 失败。
- `scripts/validate-source.py`、WPF UI 校验（0 error、19 warnings、164 info）和 `git diff --check`：通过。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/ui275-slider-v1`：`render-qa OK`，覆盖七页、Light/Dark、多尺寸、滚动探针和 resize transition；Settings 1040/1100 双主题均通过并抽查 1366 浅/深色截图。
- 以上仍是离屏证据，不能替代可识别 Playnite 宿主中的 Slider 拖动、键盘调节、DPI 和 Dashboard 逐页真实操作验收；总 Demo-first 目标仍未完成。

## 2026-08-21 UI-274 对齐 Demo 输入框与下拉选项状态

**实现内容：**

- 生产共享 TextBox 模板新增 `GscControlFocusFillBrush` 聚焦表面，浅/深色值分别对应 Demo `FieldFocusFillBrush`；键盘焦点仍使用原有共享 FocusVisual，验证错误状态继续覆盖为错误语义色。
- 生产共享 ComboBoxItem 补齐 Demo 的 UI 字体链、正文令牌和 Hand 光标；悬停/选中使用同一 Demo tint，选中项使用 Medium 字重，Popup 的滚动、键盘导航、最大高度和真实 `ItemsSource`/选择绑定保持不变。
- `AdaptiveThemePaletteFactory` 同时为高对比度 WPF 路径和普通 Demo 核心色板注入聚焦表面资源，没有改变项目 Tab chrome、当前游戏选框、ScrollViewer 或业务命令。

**验证结果：**

- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/ui-274-input-combo-v1`：XAML 18/18；Release 0 warning、0 error；Core 59/59；Worker 191/191；Playnite 264 通过、62 跳过、0 失败。
- `scripts/validate-source.py`、WPF UI 校验（0 error、19 warnings、164 info）和 `git diff --check`：通过。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/ui274-input-combo-v1`：`render-qa OK`，覆盖七页、Light/Dark、多尺寸、滚动探针和 resize transition；已抽查 Settings/Save 浅色与深色 1040×700 截图。
- 以上仍是离屏证据，不能替代可识别 Playnite 宿主中的 Dashboard 逐页视觉、DPI、键盘焦点、下拉命中和真实操作验收；总 Demo-first 目标仍未完成。

## 2026-08-21 UI-273 统一 Demo 按钮与开关状态

**实现内容：**

- 生产共享 `GscWpfUiButton` 对齐 Demo `LabBtn` 的悬停/按下状态层：只叠加不可命中的透明覆盖层并做透明度过渡，主按钮继续使用 Demo 核心渐变与 on-accent 字色，不改变按钮内容、命令或绑定。
- 生产共享 `GscWpfUiToggleSwitch` 对齐 Demo `LabToggle` 的 40×23 DIP 轨道、17 DIP 滑块、左侧留白和 140ms 位移动效；悬停、按下、禁用和键盘焦点仍由共享模板管理，保留现有 `ToggleSwitch` 内容和真实设置绑定。
- 动效目标使用 WPF 属性路径 `RenderTransform.(TranslateTransform.X)`，并补充资源/模板契约测试；没有触及项目 Tab chrome、当前游戏选框、ScrollViewer、虚拟化或业务命令。

**验证结果：**

- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/ui-273-shared-button-toggle-v1`：XAML 18/18；Release 0 warning、0 error；Core 59/59；Worker 191/191；Playnite 263 通过、62 跳过、0 失败。
- `scripts/validate-source.py`、WPF UI 校验（0 error、19 warnings、163 info）和 `git diff --check`：通过。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/ui273-shared-button-toggle-v1`：`render-qa OK`，覆盖七页、Light/Dark、多尺寸、滚动探针和 resize transition；已抽查 Settings/Save 浅色与深色 1040×700 截图，未见按钮或开关窄屏裁切。
- 以上为离屏证据，不能替代可识别 Playnite 宿主中的 Dashboard 逐页视觉、DPI、键盘焦点与真实操作验收；总 Demo-first 目标仍未完成。

## 2026-08-21 UI-272 回滚修改器中心的 Demo 分段栏

**实现内容：**

- 按目标文件末尾的 Tab 例外要求，回滚 `TrainerCenterView` 在 `a03accf` 引入的 `TrainerSegmentTabs` + `LabSegmented` 外层导航，恢复项目原有 `TrainerTabControl` / `TrainerTabItem`（基于 `GscRedesignWorkspaceTabControl` / `GscRedesignWorkspaceTabItem`）。
- 恢复为四个真实 `TabItem`：已绑定工具、导入确认、FLiNG 在线库、可下载版本；移除仅服务于 Demo 分段栏的 `PanelTools`/`PanelImport`/`PanelCatalog`/`PanelReleases` 可见性切换代码。
- 不回退 Settings 的左侧分类栏：它是目标 Demo 明确要求的设置页信息架构，不是项目工作区 Tab chrome。工具导入、目录搜索、版本下载、Inspector、列表回收虚拟化、ScrollViewer 和所有真实命令/绑定保持不变。

**验证结果：**

- 更新 `TrainerCenterUsesProjectTabNavigationWithoutDroppingRealEntryPoints` 及共享布局契约，锁定 Trainer 不再使用 `LabSegmented`，并继续检查四个 Tab、真实命令和虚拟化入口。
- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/ui-272-trainer-tab-rollback-v2`：XAML 18/18；Release 0 warning、0 error；Core 59/59；Worker 191/191；Playnite 262 通过、62 跳过、0 失败。
- `scripts/validate-source.py`、`validate_wpf_ui.py`（0 error、19 warnings、161 info）和 `git diff --check`：通过。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/ui272-trainer-tab-rollback-v1`：`render-qa OK`，覆盖七页、Light/Dark、多尺寸、各 Tab、滚动探针和 resize transition；已抽查 Trainer 浅色/深色代表截图。
- 离屏证据确认项目 Tab chrome 与四个真实面板正常渲染，但不能替代可识别 Playnite 宿主中的逐页像素、DPI、键盘焦点和真实操作验收；总 Demo-first 目标仍未完成。

## 2026-08-21 UI-271 真实 Playnite 宿主审计边界复核

**审计结果：**

- 使用 Release `0.6.70+c6cb235ef446fbe6e0c12566a7920c92e2135af8` 重新安装并启动真实 Playnite；`artifacts/ui-host-audit-ui271/summary.json` 记录 `EmbeddedSettingsCaptured=true`、`ControlledDashboardCaptured=true`、`EmbeddedDashboardCaptured=false`、`ProductionVisualSourceOfTruthAvailable=false`。
- Settings 已取得 `EmbeddedPlaynite` 当前宿主截图、视觉树和资源快照，可作为 Settings 嵌入宿主证据；Dashboard 自动 UI Automation 未找到左侧 GameSaveCenter 入口，90 秒等待后退出，不能把受控窗口的七页截图写成生产视觉真值。
- `gates/REAL_EMBEDDED_DASHBOARD_NOT_CAPTURED.json` 保留 HIGH 门禁；Computer Use 观察到 Playnite 主窗口返回 `EmptyWindowAutomationPeer`，未绕过该边界或强杀不属于本轮的旧 Worker。

**验证结果：**

- `scripts/real-host-audit.ps1 -Configuration Release -Output artifacts/ui-host-audit-ui271`：构建、安装、Playnite 启动和审计产物生成完成；最终因未捕获真实 Dashboard 以 partial 状态退出。
- 审计内置 Release 基线：XAML 18/18；Release 0 warning、0 error；Core 59/59、Worker 191/191、Playnite 262 通过、62 跳过、0 失败。
- 当前仍需用户在可见 Playnite 左侧手动进入 GameSaveCenter 后重跑真实审计，补齐 Dashboard 七页逐页像素、DPI、键盘焦点、命中区域和真实操作证据；总 Demo-first 目标保持未完成。

## 2026-08-21 UI-271 统一 Demo 表格字阶

**实现内容：**

- 在生产 `DesignTokens.xaml` 增加 `GscBodyFontSize=13.5` 和 `GscCaptionFontSize=12`，对应 Demo `SizeBody`/`SizeCaption`。
- 生产共享 `DataGrid` 与 `DataGridColumnHeader` 统一使用 UI 字体链、13.5 DIP 正文、12 DIP 表头和 Medium 表头字重；表格 44 DIP 行高、36 DIP 表头、排序箭头、列宽调整、选中态、滚动和 Recycling 虚拟化保持不变。
- `SharedDataGridUsesDemoBodyAndCaptionFontTokens` 同时锁定生产令牌与 Demo 参考值，避免宿主默认字号造成页面间表格密度漂移。

**验证结果：**

- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/ui-271-table-typography-v1`：XAML 18/18；Release 0 warning、0 error；Core 59/59、Worker 191/191、Playnite 262 通过、62 跳过、0 失败。
- `python scripts/validate-source.py`、WPF UI 校验（0 error、19 warnings、161 info）和 `git diff --check`：通过。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/ui271-table-typography-v1`：`render-qa OK`，覆盖七页双主题、多尺寸、滚动探针和 resize transition；已抽查 Save、Task、Maintenance 表格代表截图。
- 离屏证据仍不能替代可识别 Playnite 宿主中的真实字号、DPI、键盘焦点和列宽拖动验收，总 Demo-first 迁移保持未完成。

## 2026-08-21 UI-270 统一 Demo 折叠栏箭头动效

**实现内容：**

- 生产共享 `GscDisclosureCardExpander` 的箭头状态现在复用 Demo `LabDisclosure` 的 150ms 展开/收起旋转动效；整行点击、键盘焦点、内容显隐、真实 Expander 绑定和主题资源保持不变。
- 只修共享 `DesignTokens.xaml` 模板并增加源码契约，没有在页面局部复制另一套折叠栏，也没有迁移 Demo 滚动条或改变任何业务命令。

**验证结果：**

- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/ui-270-disclosure-animation-v1`：XAML 18/18；Release 0 warning、0 error；Core 59/59、Worker 191/191、Playnite 261 通过、62 跳过、0 失败。
- `python scripts/validate-source.py`、WPF UI 校验（0 error、19 warnings、161 info）和 `git diff --check`：通过。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/ui270-disclosure-animation-v1`：`render-qa OK`，覆盖七页双主题、多尺寸、滚动探针和 resize transition；未发现布局、滚动或虚拟化回归。
- 动效的真实时间曲线仍需在可识别 Playnite 宿主中用键盘/鼠标展开收起确认；离屏截图不作为动效完成的唯一证据，总 Demo-first 迁移保持未完成。

## 2026-08-21 UI-269 Demo 核心主题配色固定

**实现内容：**

- 新增共享 `AdaptiveThemePaletteFactory.ApplyDemoCoreResources`，将 Demo 的浅色/深色画布渐变、卡片/侧栏/顶栏、输入框、正文层级、表格行与表头、分段控件、滚动条、遮罩和成功/警告/错误/信息状态关系写入生产资源；Playnite 仍可提供非核心 Accent/focus 颜色。
- 生产 Shell 和 Settings 都在 WPF 主题资源应用后调用同一核心色板；高对比度仍保留系统自适应不透明路径。没有改动生产 Tab chrome、当前游戏选框、滚动条交互、虚拟化、真实命令/绑定或业务状态。
- `DemoCorePaletteWinsOverHostNeutralBrushesOutsideHighContrast` 锁定核心色板入口、浅深色卡片、表格表头、滚动条和错误色资源契约。

**验证结果：**

- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/ui-269-demo-palette-v2`：XAML 18/18；Release 0 warning、0 error；Core 59/59、Worker 191/191、Playnite 260 通过、62 跳过、0 失败。
- `python scripts/validate-source.py`、WPF UI 校验（0 error、19 warnings、161 info）和 `git diff --check`：通过。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/ui269-demo-palette-v1`：`render-qa OK`，覆盖七页双主题、多尺寸、滚动探针和 resize transition；已抽查 Overview、Save、Media、Maintenance、Settings 的浅/深色代表图。
- 离屏证据仍不能替代可识别 Playnite 宿主中的逐页像素、DPI、键盘焦点、主题切换和真实操作验收，总 Demo-first 迁移保持未完成。

## 2026-08-21 UI-268 Demo 字体阶关系补齐

**实现内容：**

- 在生产 `DesignTokens.xaml` 增加 `GscDisplayFontFamily`，统一为 `Segoe UI Variable Display, Segoe UI, Microsoft YaHei UI`；正文仍使用 `GscUiFontFamily`，代码/路径仍使用 `GscCodeFontFamily`。
- `GscRedesignHeroTitle`、`GscRedesignFeedbackDialogTitle`、`GscPageTitleStyle`、生产 Shell 标题和旧 Dashboard 回退标题接入 Display 字体；分区标题保持正文族，符合 Demo 的 `LabTitle`/`LabSection` 字阶关系。
- 没有改变生产 Tab chrome、当前游戏选框、滚动条、DataGrid/ListBox 虚拟化、命令、绑定或真实运行时数据；补充 `TypographyUsesCentralizedChineseAwareFontChain` 对 Display 令牌和标题入口的回归断言。

**验证结果：**

- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/ui-268-display-font-v1`：XAML 18/18；Release 0 warning、0 error；Core 59/59、Worker 191/191、Playnite 259 通过、62 跳过、0 失败。
- `python scripts/validate-source.py`、WPF UI 校验（0 error、19 warnings、161 info）和 `git diff --check`：通过。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/ui268-display-font-v1`：`render-qa OK`，覆盖七页双主题、1040/1100/1366/2560 DIP、多页滚动和 resize transition。
- 本阶段只补齐共享字体资源；离屏 RenderHarness 证据仍不能替代可识别 Playnite 宿主中的逐页像素、DPI、键盘焦点、主题切换和真实操作验收，总 Demo-first 迁移保持未完成。

## 2026-08-21 UI-267 工作区测量与几何审计收口

**实现内容：**

- 媒体“当前游戏媒体”标题区的真实搜索框最小宽度提高到 `160 DIP`，外层操作区保留 `300 DIP` 最小宽度；没有改变 `MediaSearchText` 绑定、媒体筛选、卡片列表、预览 Inspector 或生产滚动条。
- 存档历史表在宿主工作区低于 `1240 DIP` 时提前使用紧凑列宽，确保标准约 `1116 DIP` 宿主中右侧 Inspector 存在时“状态”列仍位于表格可达区域；历史 DataGrid 的自动横向滚动、列宽拖动、排序、虚拟化和所有真实命令保持不变。
- `SharedWorkspaceBreakpointsKeepSearchAndHistoryEssentialsReadable` 回归测试锁定媒体搜索框和存档紧凑断点契约。
- 几何审计改为比较主 DataGrid/ListBox 直接所属 Grid 行，识别 Overview 有限活动视口、排除表格/列表内部卡片 WrapPanel 的工具栏误报，并将小于 8 DIP 的 WPF 单行 TextBox 内部滚动量视为模板测量噪声；列最小宽度超出视口但存在 Auto 横向滚动时记录为 `EXPECTED_HORIZONTAL_SCROLL`，不掩盖无滚动能力的真实压力。

**验证结果：**

- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/ui-audit-layout-fix-v1`：XAML 18/18；Release 0 warning、0 error；Core 59/59、Worker 191/191、Playnite 259 通过、62 跳过、0 失败。
- `python scripts/validate-source.py`、WPF UI 校验（0 error、19 warnings、161 info）和 `git diff --check`：通过。
- `artifacts/ui-audit-ui267-fix3/AUDIT_SUMMARY.md`：静态/运行时路由全部成功，Fidelity 0、HIGH 0、MEDIUM 0；主表直接行填充率在最大化任务/媒体快照为 1.00，横向列压缩有运行时滚动证据。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/ui267-layout-audit-fix-v1`：`render-qa OK`，覆盖七页双主题、多尺寸、滚动探针和 resize transition；已检查存档历史、当前游戏媒体和任务页截图。离屏证据仍不能替代可识别 Playnite 宿主中的逐页像素、DPI、键盘焦点及真实操作验收。

## 2026-08-20 UI-266 存档维护指标阅读节奏对齐 Demo

**实现内容：**

- 将存档页“最近差异”的新增/修改/删除三个真实指标改为 Demo 的“数值 → 标签”顺序，并保留差异摘要、只读比较和保留预览 Inspector。
- 将维护页的保留、容量、容量趋势、保留模拟、保护状态和本地镜像指标统一为“数值 → 标签 → 补充说明”的阅读节奏；所有绑定仍来自真实 Snapshot、Retention、Storage、Simulation 和 Mirror 状态，没有加入 Mock 数据。
- 没有改变命令、错误/取消/安全语义、DataGrid/列表虚拟化、页面滚动、Inspector 或用户明确保留的生产 Tab chrome。

**验证结果：**

- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/metrics-rhythm-v1`：XAML 18/18；Release 0 warning、0 error；Core 59/59、Worker 191/191、Playnite 258 通过、62 跳过、0 失败。
- `python scripts/validate-source.py`、WPF UI 校验（0 error、19 warnings、161 info）和 `git diff --check`：通过。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/metrics-rhythm-v1`：`render-qa OK`，覆盖七页双主题、多尺寸、滚动探针和 resize transition；已检查存档比较/保留与维护异常/审计截图。离屏证据仍不能替代可识别 Playnite 宿主的逐页像素验收。

## 2026-08-20 UI-265 维护诊断概览前置环境健康区

**实现内容：**

- 按 Demo `MaintenancePage` 的诊断概览顺序，把六项真实健康状态从“更多维护操作”展开区移到页面首屏的“环境健康”卡；环境检查与诊断操作继续位于其后，生产外层 Tab 和诊断内层 Tab chrome 按用户例外保持当前实现。
- `DiagnosticHealthPanel` 仍使用真实 `Snapshot.WorkerVersion`、`Snapshot.LudusaviVersion`、Rclone 可用性、数据/媒体目录、`Snapshot.UnassignedMediaCount` 和 `DeviceComparisons.Count`；只调整信息架构，没有加入 Demo 的固定状态或示例数字。
- 保留环境检查展开项、复制/导出/完整性自检、目录日志、索引重建、任务协调、元数据灾备、路径迁移、安全模式和健康摘要等全部真实命令与安全状态；健康卡继续由 `ApplyResponsiveLayout` 响应式排列。
- 回归断言新增健康卡位于环境检查和诊断操作之前的结构契约，并同步当前 4/2/1 健康卡列数逻辑。

**验证结果：**

- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/maintenance-health-order-v1`：XAML 18/18；Release 0 warning、0 error；Core 59/59、Worker 191/191、Playnite 258 通过、62 跳过、0 失败。
- `python scripts/validate-source.py`：通过；`python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .`：0 error、19 warnings、161 info；`git diff --check`：通过。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/maintenance-health-order-v1`：`render-qa OK`，覆盖七页双主题、多尺寸、滚动探针与 resize transition；已查看维护页截图，Tab chrome、设备/诊断表格和 Inspector 仍可达。离屏证据仍不能替代可识别 Playnite 宿主的逐页像素验收。

## 2026-08-20 UI-264 首页统计条恢复 Demo 连续结构

**实现内容：**

- 将首页六张独立统计卡收口为 Demo 的单个连续 `GscRedesignSectionCard` 统计条，六个等宽指标之间使用五条 `GscTableDividerBrush` 分隔；数字置于标签上方，使用 26 DIP 的 Demo 阅读节奏。
- 六项仍绑定真实 `Snapshot.ManagedGames`、`MatchedGames`、`RunningGames`、`WarningGames`、`PendingCloudTasks` 和 `UnassignedMediaCount`；匹配率/风险率进度条及空游戏库时的隐藏逻辑均保留，没有写入 Mock 数字。
- `OverviewStatStrip` 后台契约从 `UniformGrid` 调整为 `Border`，删除仅用于旧统计卡的列数响应式分支；当前游戏选框、今日工作台操作区、活动列表滚动/虚拟化、风险卡和所有真实命令没有改变。统计卡的悬停位移动画仍受现有 render-only gate 保护。
- 更新首页结构回归断言，明确保护连续统计条、五条 Divider、六项真实绑定、进度条和 hover 事件；生产 Tab chrome 仍按用户明确例外保持当前项目实现。

**验证结果：**

- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/overview-summary-strip-v1`：XAML 18/18；Release 0 warning、0 error；Core 59/59、Worker 191/191、Playnite 258 通过、62 跳过、0 失败。
- `python scripts/validate-source.py`：通过；`python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .`：0 error、19 warnings、161 info；`git diff --check`：通过。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/overview-summary-strip-v1`：`render-qa OK`，覆盖双主题、多尺寸、滚动探针与 resize transition；已查看 Overview 1366×768 截图，当前游戏、连续统计条和下方风险内容均保持可达。离屏证据仍不能替代可识别 Playnite 宿主的逐页像素验收。

## 2026-08-20 UI-263 任务页统计条恢复 Demo 连续结构

**实现内容：**

- 将任务中心顶部统计从“外层卡片 + 可变列 UniformGrid + 四个内部 Border”收口为 Demo 的单个连续四等分统计条，使用 26 DIP 数字、三条分隔线和统一的 `GscRedesignSectionCard` 表面。
- 四项仍绑定 `Tasks.Count`、`RunningTaskCount`、`RetryableTaskCount`、`CompletedTaskCount`；运行中/需要重试/今日完成分别使用 Demo 对应的强调/警告/成功色阶，没有写入演示数字。
- `TaskSummaryPanelElement` 同步为真实 `Border`，删除旧的列数响应式分支；任务队列最小视口、筛选栏、更多筛选、右侧 Inspector、复制错误/重试/取消操作和任务表虚拟化均未改变。
- 更新任务页结构回归断言，继续保护真实计数、Divider、连续统计条和详情布局契约。

**验证结果：**

- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/task-summary-strip-v1`：XAML 18/18；Release 0 warning、0 error；Core 59/59、Worker 191/191、Playnite 258 通过、62 跳过、0 失败。
- `python scripts/validate-source.py`：通过；`python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .`：0 error、19 warnings、161 info；`git diff --check`：通过。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/task-summary-strip-v1`：`render-qa OK`，覆盖双主题、多尺寸、滚动探针与 resize transition；已查看 Task 1366×768 截图，筛选栏、队列和 Inspector 均保持可达。离屏证据仍不能替代可识别 Playnite 宿主的逐页像素验收。

## 2026-08-20 UI-262 媒体页统计条恢复 Demo 连续结构

**实现内容：**

- 将生产媒体中心顶部四张独立 `GscRedesignMetricBorder` 卡片改为一个连续的 `GscRedesignSectionCard` 统计条，按 Demo 的四等分布局和三条竖向分隔线呈现媒体文件、归档占用、收藏和待归类四组真实统计。
- 保留 `MediaSummary.TotalCount`、截图/录像数量、`TotalSizeDisplay`、`FavoriteCount`、`Snapshot.UnassignedMediaCount` 的原有 OneWay 绑定；没有写入 Demo 数字，也没有改变待归类 DataGrid、预览、Inspector、归类/忽略命令或页签结构。
- 同步 `MediaCenterView.xaml.cs` 的命名元素契约，从旧 `UniformGrid.Columns` 调整为真实的 `Border`，避免后台响应式逻辑继续访问已移除的 `Columns` 属性。
- 更新页面基线测试，明确保护连续统计条、Divider、真实绑定和不再出现独立 metric card；当前项目 Tab chrome 例外继续保持不变。

**验证结果：**

- `scripts/build.ps1 -Configuration Release -OutputRoot artifacts/gsc-b/media-summary-v1`：XAML 18/18；Release 0 warning、0 error；Core 59/59、Worker 191/191、Playnite 258 通过、62 跳过、0 失败。
- `python scripts/validate-source.py`：通过；`python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .`：0 error、19 warnings、161 info；`git diff --check`：通过。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/media-summary-strip-v1`：`render-qa OK`，覆盖双主题、多尺寸、滚动探针与 resize transition；已查看 Media 1040×700/1366×768 待归类截图，统计条和主表/Inspector 均可见。离屏证据仍不能替代可识别 Playnite 宿主的逐页像素验收。

## 2026-08-20 UI-253 修改器中心恢复 Demo 分段页面结构

**实现内容：**

- 修改器中心移除旧生产 `TabControl/TabItem` 外壳，按 Demo `TrainerPage.xaml` 恢复顶部 `LabSegmented` 分段导航、说明副标题和四个命名内容面板：已绑定工具、导入确认、FLiNG 在线库、可下载版本。
- 分段切换仅改变面板可见性，不改变真实 ViewModel、Binding、Command 或异步流程；保留 EXE/目录/CT/自定义启动项导入、待确认入口选择、在线目录搜索、发行版本加载和下载绑定。
- 工具列表、目录结果、发行版本列表继续使用虚拟化和回收模式；现有 Inspector、紧凑详情抽屉、项目 ScrollBar 和响应式两列/堆叠布局均保留。分段导航使用 Demo 的 `LabSegmented`，不套用旧 TabControl 样式。
- 增加初始化期空保护，避免 `SelectedIndex=0` 在 XAML 构造期间先触发分段事件而访问尚未生成的面板字段；源码验证器同步识别 Demo segmented 导航为少量非虚拟化标签列表。
- 更新页面基线测试，将过时的 TabControl 结构断言迁移为分段导航、四面板、真实命令和滚动/虚拟化契约；没有删除任何业务入口。

**验证结果：**

- `dotnet build GameSaveCenter.sln -c Release --no-restore -m:1 -nodeReuse:false -p:NuGetAudit=false -p:MSBuildEnableWorkloadResolver=false`：0 warning、0 error。
- 解决方案测试：Core 59/59、Worker 191/191、Playnite 252/252 通过、61 跳过、0 失败。
- `scripts/check-xaml.ps1`：18 个文件通过；`python scripts/validate-source.py`：通过；`git diff --check`：通过。
- `python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .`：0 error，20 个既有 warning、161 个 info。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/trainer-segmented-final`：`render-qa OK`；双主题、多尺寸、resize 以及修改器中心四个分段均完成渲染和布局探针。离屏证据仍不能替代可识别 Playnite 宿主的逐页像素验收。

## 2026-08-20 DOC-002 Demo-first 优先级覆盖旧 WPF 视觉约束

**决策：**

- 用户明确要求取消 `wpf-apple-desktop-ui` 对页面视觉和实现路线的优先制约，后续以 AcrylicFork Demo 为最新且最高视觉基准。
- 已在 `AGENTS.md`、`APPLE_WPF_IMPLEMENTATION_PROMPT.md`、`UI_CHANGE_GATE.md`、`DEVELOPMENT_HANDOFF.md`、`CODEX_CONTINUATION_PROMPT.md`、`AUTONOMOUS_DEVELOPMENT_RULES.md`、`PROJECT_MEMORY.md` 及当前 UI 计划中同步该优先级。
- `wpf-apple-desktop-ui` 只保留质量检查职责：真实 Binding/Command、异步错误/取消/安全语义、虚拟化、键盘/UI Automation、可访问性、主题/DPI 和 Playnite 兼容性；不能推翻 Demo 的页面结构、导航、控件或视觉关系。
- 明确保留当前游戏选框、生产滚动条系统、真实数据与命令；Demo Mock 数据、演示色板、窗口按钮和演示行为不迁移。

**验证边界：**

- 本阶段仅更新项目提示词、门禁和交接文档，没有修改生产代码或业务行为；后续页面阶段仍需按 Demo 目标运行完整源码、构建、测试和页面审计。

## 2026-08-20 UI-251 存档规则卡与诊断概览第三轮对齐

**实现内容：**

- 存档中心“当前存档规则”卡将规则标题、游戏名、匹配/风险状态和立即扫描、重新校验、刷新详情操作收束到同一横向区域；只有低于 700 DIP 才进入紧凑堆叠，避免正常宿主宽度下无谓增高。
- 维护中心“诊断概览”改回 AcrylicFork 生产 Demo 的信息顺序：先显示环境检查与首次环境检查折叠内容，再显示诊断操作；更多维护操作中的健康指标仍保留真实 Binding 与命令。
- 共享表格排序箭头恢复 Demo 的完整几何和独立表头空间，保留列宽拖拽、排序和虚拟化能力。
- 设置页补齐全局 `GscUiFontFamily` 入口；旧 Dashboard 回退页原已有字体继承，继续保留。

**验证结果：**

- `dotnet test tests/GameSaveCenter.Playnite.Tests/GameSaveCenter.Playnite.Tests.csproj -c Release --no-restore -m:1 -nodeReuse:false -p:NuGetAudit=false -p:MSBuildEnableWorkloadResolver=false`：251 通过、61 跳过、0 失败；同时修正两条仍要求旧页面结构的 AcrylicFork 基线门禁。
- `scripts/render-qa.ps1 -Configuration Release -Output .tmp/ui-qa-stage3-final`：`render-qa OK`；双主题、多尺寸和 resize 探针通过，存档规则卡在 1040×700 下保持横向紧凑。
- `python scripts/validate-source.py`、WPF UI 静态校验（0 error）和 `git diff --check` 均通过；未以 RenderHarness 结果宣称真实 Playnite 宿主视觉验收完成。

## 2026-08-20 UI-250 AcrylicFork 媒体页结构与全局字体第二轮对齐

**实现内容：**

- 媒体中心恢复 AcrylicFork 的实际生产结构：四张摘要卡独立占据顶部，完整 `TabControl` 自己承载 Tab 标题和内容，不再把 Tab 标题、摘要卡和内容宿主拆成变形的三段布局。
- 待归类页保留真实 `UnassignedMedia` DataGrid，并新增/保留独立媒体详情 Inspector：选中项可预览截图或录像、查看来源与归类原因，并在侧栏完成目标游戏选择、确认归类或忽略保留副本；这些控件不放进 DataGrid 滚动面。
- 常见 Playnite 内容宽度下待归类 Inspector 保持侧栏可见，只有低于 700 DIP 才堆叠，避免用户在实际安装后看不到预览和操作。
- 生产壳体与工作区根节点统一继承 `GscUiFontFamily`，让页面正文、表格和按钮不再各自落到不同默认字体。

**验证结果：**

- `scripts/render-qa.ps1 -Configuration Release -Output .tmp/ui-qa-stage2`：`render-qa OK`；待归类 1040×700 离屏结果已显示表格 + 详情预览侧栏，双主题、多尺寸和 resize 探针通过。
- `validate_wpf_ui.py`：0 error；19 条既有 warning、161 条 info，未新增 XAML 错误。
- `git diff --check`：通过。未以 RenderHarness 结果宣称真实 Playnite 宿主视觉验收完成。

## 2026-08-20 UI-249 AcrylicFork 首页与共享控件首轮对齐

**实现内容：**

- 以 `D:\workplace\github\GameSaveCenter.AcrylicFork` 的实际生产页面为参考，首页补回独立的“今日工作台”批量操作区，保留现有真实刷新、全部备份、媒体同步和关注项命令；首页最近任务行固定为 54 DIP，并将消息保持单行省略，避免多行消息把一条记录撑成多行高度。
- 存档中心生产壳体新增可见的“立即备份”入口，绑定现有 `BackupSelectedCommand`；“全部备份”仍保留为全局操作，两者语义不混用。
- 共享表格框架收紧为 Demo 的 12 DIP 圆角，状态胶囊改为紧凑无描边软填充；DataGrid 行固定使用统一行高，表头排序箭头保留独立空间，继续允许用户调整列宽、排序和虚拟化。
- 共享折叠栏改为紧凑的箭头 + 标题行 + 内容体结构，保留原有 Header、Binding、键盘焦点和内容命令。

**验证结果：**

- `scripts/render-qa.ps1 -Configuration Release -Output .tmp/ui-qa-stage1`：`render-qa OK`，双主题、多尺寸和 resize 探针通过；首页 1040×700 已出现“今日工作台”，最近任务仍保持有限视口。
- `python scripts/validate-source.py`：通过。
- `validate_wpf_ui.py`：0 error；既有布局 warning 仍需在后续页面结构阶段逐项消除。
- `git diff --check`：通过。未以 RenderHarness 结果宣称真实 Playnite 宿主视觉验收完成。

## 2026-08-19 UI-248 媒体中心摘要卡片与标签栏分区

**实现内容：**

- 将媒体中心四个统计项拆成独立等宽卡片；统计卡片不再和 Tab 导航共用同一个横向容器。
- 新增独立的媒体 Tab 条，内容宿主单独占据剩余行；保留真实媒体 Binding、详情 Inspector、项目滚动条和虚拟化。
- 该结构修正了生产页仍沿用旧“统计项包住 Tab”信息架构的问题。

**验证边界：**

- XAML、Playnite 测试和 `render-qa OK` 均通过；离屏截图已确认卡片、Tab 条和内容区边界分离。
- 当前仍没有可识别的 Playnite 生产窗口像素截图，不能把 RenderHarness 结果写成宿主 1:1 验收完成。

## 2026-08-19 UI-247 任务搜索输入区结构回归保护

**核对结果：**

- 当前生产 `TaskCenterView.xaml` 已将“搜索任务…”提示、搜索图标和真实 `TaskSearchTextBox` 放入同一个可伸展输入区；没有恢复为独立标签加窄输入框。
- `TaskSearchBoxHost` 在桌面、中等和紧凑宽度分别保留 `420 / 300 / 180 DIP` 最小宽度，状态、类型和刷新控件继续使用独立边界。
- 用户当前截图中的旧布局来自旧安装包或仍在运行的旧 Playnite 进程；RenderHarness 的当前任务页已显示连续搜索框，但不把它当作 Playnite 宿主像素验收。
- 增加结构回归断言，防止后续提交重新引入 `TaskSearchLabel`。

## 2026-08-19 UI-246 媒体中心摘要标签栏强调材质

**实现内容：**

- 将媒体中心顶部的“待归类 / 当前游戏媒体 / 来源规则”摘要标签栏从通用灰色控件填充改为现有 `GscAccentTintBrush` 紫色强调材质。
- 保留媒体卡片、真实 Binding、右侧详情 Inspector、项目自有滚动条和虚拟化实现，不迁移 Demo 顶部主题按钮或 Demo 滚动条。
- 在 Playnite 页面基线测试中增加材质回归断言，避免该标签栏再次退回通用灰色资源。

**验证结果：**

- `scripts/validate-source.py`：通过。
- `scripts/check-xaml.ps1`：18 个 XAML 文件通过。
- `validate_wpf_ui.py`：0 error；19 条既有 warning，161 条 info。
- Playnite 测试：250 通过、61 跳过、0 失败。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/media-purple`：`render-qa OK`；双主题、多尺寸、媒体内部滚动和 resize 探针通过。
- 更新后的媒体截图：[Media-1366x768-tab1.png](/D:/workplace/github/GameSaveCenter/artifacts/ui-qa/media-purple/Media-1366x768-tab1.png)。

**验证边界：**

- 本阶段已确认离屏渲染结果，但尚未获得可识别的 Playnite 生产窗口像素截图；不能把 RenderHarness 结果写成宿主 1:1 视觉验收完成。

## 2026-08-19 UI-245 拉长任务中心搜索输入框

**实现内容：**

- 保持“搜索任务…”提示和搜索图标在同一个 `TaskSearchTextBox` 内，不恢复为左侧独立标签。
- 为真实 Playnite 宿主增加响应式最小宽度：桌面宽度 `420 DIP`、中等宽度 `300 DIP`、紧凑宽度 `180 DIP`，避免筛选栏测量时搜索框退化成窄条。
- 状态、类型和游戏筛选仍保留独立边界；窄窗口继续换到第二行，不让筛选控件覆盖搜索区。

**验证结果：**

- XAML structural validation：18 个文件通过。
- Release 完整打包：成功；Core 59/59、Worker 191/191、Playnite 250 通过、61 跳过、0 失败。
- Release 安装：清单版本 `0.6.70`、DLL 版本 `0.6.70.0`，已部署到本机 Playnite 扩展目录。
- `git diff --check`：通过。

**验证边界：**

- 安装后 Playnite 日志确认生产扩展从新安装目录加载了 `GameSaveCenter.Playnite.dll`；本次重新启动未保持可识别窗口，因此没有新增宿主截图，不把它写成最终视觉截图验收完成。

## 2026-08-19 UI-244 首页风险明细紧凑布局复核

**问题与修正：**

- 首页最近任务列表和外层承载取消固定最小高度，任务数量较少时不再在卡片底部留下大块空白；列表仍保留项目自身的虚拟化和滚动配置。
- 风险与提醒收敛为一套可见内容：隐藏重复的紧凑预览和重复操作行，保留安全提示、最近游玩保护摘要、可展开明细和云端提示；底部操作按钮独立留在风险视口外。
- 风险外层视口按布局模式设为有限高度：紧凑堆叠布局最多 520 DIP，侧栏布局最多 420 DIP；展开的保护明细另有独立内部视口，紧凑布局最多 420 DIP、侧栏布局最多 340 DIP。数量增加时只滚动列表，不会继续把首页根页面撑高。
- 最近任务状态图标底色统一使用较轻的紫色强调底，避免黑色/透明块压住 Demo 的紫色层级。

**验证结果：**

- `scripts/validate-source.py`：通过。
- `scripts/check-xaml.ps1`：18 个 XAML 文件通过。
- `validate_wpf_ui.py`：0 error；19 条既有 warning，161 条 info。
- `git diff --check`：通过。
- Release 一键构建/安装与 Core 59、Worker 191、Playnite 250 通过 / 61 跳过结果已取得；生产扩展清单和 DLL 均为 0.6.70。
- 在同一个可识别的 Playnite 生产宿主中重新打开首页并展开风险明细：明细卡片、换行说明和“查看”操作完整可见，底部“查看需要处理的游戏 / 启用所选保护”仍在独立操作区，没有被明细滚动区域截断。

**验证边界：**

- 本条只覆盖首页紧凑布局下的风险展开态，以及首页最近任务承载；不能据此宣称所有页面已经完成 AcrylicFork Demo 的 1:1 宿主验收。

## 2026-08-19 UI-240 风险视口与维护空选中状态宿主复核

**问题与修正：**

- 首页“风险与提醒”不能随着条目数量无限增加主页面高度；保留外层 `OverviewRiskViewport` 的 `330 DIP` 上限，并让风险列表使用项目现有滚动条。展开的最近游戏保护明细继续使用独立的 `190 DIP` 内部视口，标题、说明和操作不随列表滚动。
- 维护中心“发现的问题”和“审计记录”在没有选中行时，表格占满工作区，右侧 Inspector 隐藏；点击表格行后再显示独立 Inspector。这样与 Demo 的空选中状态一致，也避免空详情栏长期挤压表格。
- `DashboardViewModel` 刷新 Findings 时只恢复此前仍存在的选中项，不再自动选中第一条问题；真实 Findings、Binding、命令和安全语义保持不变。

**验证结果：**

- 一键 Release 构建/安装成功，生产扩展目录与 `extension.yaml`、DLL 版本核对为 `0.6.70`。
- Core：59/59 通过；Worker：191/191 通过；Playnite：249 通过、61 跳过、0 失败。
- `scripts/validate-source.py`、WPF UI 校验（0 error）和 `git diff --check` 通过。
- 使用屏幕控制进入实际 Playnite 生产版：首页风险栏没有把主页面无限撑高；维护中心无选中时表格为全宽，点击问题后才出现右侧“问题详情” Inspector。该结论来自实际宿主截图，不是 RenderHarness 离屏图。

## 2026-08-19 UI-227 媒体 Tab 材质与生产按钮文本边界

**问题与修正：**

- 媒体中心沿用共享工作区 Tab 模板，但模板把外层 Tab 条背景硬编码为 `GscControlFillBrush`，导致该页显示成用户指出的浅灰控件条。模板现在绑定 `TabControl.Background`；媒体页单独使用现有 `GscGlassFillBrush`，没有改动 Demo 不要求迁移的滚动条。
- 共享生产按钮增加文本内容模板：文字在按钮内容区水平/垂直居中、禁止换行并使用字符省略，长本地化标签或长操作名称不会把按钮撑出工具栏/Inspector 边界；命令、Binding 和按钮状态保持不变。
- 工作区分节标题统一禁止换行并启用字符省略，避免游戏名或页面标题变长时侵入相邻控件。

**验证结果：**

- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/render-tab-text` 通过：双主题、1040/1100/1366/2560、多页面滚动、resize 和媒体虚拟列表探针均完成，结果为 `render-qa OK`。
- `python scripts/validate-source.py`、WPF UI 校验（0 errors）和 `git diff --check` 通过。
- `dotnet test tests/GameSaveCenter.Playnite.Tests/GameSaveCenter.Playnite.Tests.csproj -c Release --no-restore -m:1 -p:MSBuildEnableWorkloadResolver=false --nologo`：249 通过、61 跳过、0 失败。
- 本条改动仍需通过当前提交的 Playnite 生产入口人工核对；离屏截图不作为宿主视觉真值。
- 已用 `scripts/dev-install-run.ps1 -Configuration Release -NoStart` 完成真实安装验证：生产扩展目标为 `C:\Users\lopmatu\AppData\Roaming\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`，`extension.yaml` 与 DLL 均为 `0.6.70`。随后使用屏幕控制两次定位 Playnite，但返回画面实际是 Codex/其他窗口，且自动化树为 `EmptyWindowAutomationPeer`；因此本轮明确不宣称 Playnite 视觉验收通过，已停止屏幕操控。

## 2026-08-19 UI-226 维护表格中等宽度列布局

**问题与修正：**

- 维护中心在 1040 DIP 工作区保留右侧 Inspector 时，发现问题表格的固定列与最小列宽总和超过剩余宽度，导致“建议处理”列被挤压或只剩不可读的窄条。
- 新增维护诊断/审计表格的响应式列宽策略：中等宽度收窄游戏、标题和建议处理列，详情与动作列继续使用弹性宽度；宽屏恢复接近 Demo 的列级比例。
- 所有维护长文本单元格统一左对齐、垂直居中、字符省略，并保留 Tooltip 查看完整内容；没有改变 Binding、选中项、Inspector 或业务命令。

**验证结果：**

- RenderHarness Release 构建通过，`render-qa OK`；1040/1100/1366/2560、浅色/深色、resize、滚动和维护诊断/审计表格探针均通过。
- `scripts/validate-source.py`、WPF UI 校验（0 errors）和 `git diff --check` 通过；校验中的 StackPanel/主题资源提示为既有提示。
- 直接执行 `dotnet build --no-restore` 仍受当前机器仅安装 .NET SDK 9.0.302 且缺少 Workload locator 目录影响，出现 MSB4276；本轮采用项目已有 RenderHarness 脚本的 `MSBuildEnableWorkloadResolver=false` 路径完成实际 WPF 构建和渲染验证。

## 2026-08-18 WORKER-001 完整性检查忽略未配置的可选目录

**问题与修正：**

- 一键安装的 Worker 测试曾把默认安装中未配置的 `GameToolsDirectory`、`DownloadDirectory` 判定为 `Warning`，导致健康数据库/目录场景无法得到 `Healthy`，可选依赖未配置场景也被额外污染。
- `IntegrityCheckService.CheckDirectory` 现在仅对已配置但不存在/不可写的可选目录报告问题；空的可选目录按“未启用”处理，关键目录仍保持原有失败语义。

**验证结果：**

- 隔离 Release 编译：0 warning / 0 error。
- Worker：191/191 通过；完整性定向测试：6/6 通过。
- Core：59/59 通过；Playnite 测试在仓库原目录运行：248 通过、61 跳过、0 失败。
- 使用隔离输出目录运行 Playnite 测试会因测试按运行目录定位仓库而出现 `Repository root not found`，这是测试运行方式限制，不是源码断言失败；因此后续完整验证应保持 Playnite 测试从仓库目录执行。

## 2026-08-18 UI-225 首页风险列表有限视口

**实现内容：**

- 首页“风险与提醒”保留标题、安全恢复提示、摘要和操作按钮在外层卡片中；可能随数据增长的风险原因列表改为独立的 `ScrollViewer`，最大高度 190 DIP，竖向自动滚动、横向禁用。
- 展开的“最近游戏保护明细”也使用独立的 190 DIP 列表视口，避免 `RecentProtection.Items` 把右栏或首页根页面无限撑高。生产数据本身仍按现有 ViewModel/服务上限展示，不改变风险计数、选择、命令和安全语义。
- 两个列表都继续使用生产 `GscPageScrollViewer`，没有引入 AcrylicFork 演示滚动条；页面根滚动面仍负责跨区内容，列表视口只负责自身溢出。

**验证结果：**

- `validate-source.py`、WPF UI 校验（0 errors）和 `git diff --check` 通过；WPF 校验仅报告项目已有的 StackPanel/主题资源提示。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/overview-risk-bounded-v2` 通过，双主题、多尺寸、响应式缩放和构建均完成，结果为 `render-qa OK`。
- 相关结构回归测试已加入，测试运行器本轮再次无输出挂起并被中止，因此不把该次定向测试报告为通过；RenderHarness 构建和渲染验证通过。

## 2026-08-18 UI-224 首页徽标模板与 Demo 行密度修正

**本轮修正：**

- 共享 `ChipBase` 的模板现在显式使用 `HorizontalContentAlignment` 和 `VerticalContentAlignment`，不再依赖外层 `Border` 与内部 `TextBlock` 的偶然对齐。
- 首页最近任务改为 Demo 的图标瓷贴、54 DIP 行高、详情列内嵌进度条、独立状态/时间列；只有运行中、等待中和等待确认任务显示进度。
- 首页全局活动的类型/结果徽标改用共享 Chip，固定 64 DIP 最小宽度并保持内容居中；风险提醒同样使用共享错误 Chip，字号 11、最小宽度 64 DIP，确保“风险”完整可见。

**验证边界：**

- `validate-source.py`、WPF UI 校验和 `git diff --check` 通过；Release 隔离构建 `phase-overview-badges-v25` 通过（0 warning/0 error，构建时跳过测试）。
- RenderHarness 的首页尺寸、主题和真实滚动检查通过；整套报告仍被既有 Media resize 恢复问题拦住：`MediaGrid` 与 `MediaInspectorScrollViewer` 两项失败，不能写成全量 Render QA 通过。
- 0.6.70 包已生成并安装到 Playnite。真实 Playnite 生产入口截图确认最近任务状态气泡居中、全局活动徽标居中、风险文字完整显示；电脑控制已释放。该截图只证明首页本轮区域，不代表所有页面已完成 Demo 逐页验收。
- Playnite 旧 UI 测试仍包含针对已删除旧页面结构的断言；本轮没有用跳过或删测试的方式掩盖它们。

## 2026-08-18 UI-223 首页状态徽标与真实宿主复核

**原因定位：**

- 首页最近任务的进度面板此前对所有任务都可见，导致成功、失败任务也被渲染成 Demo 中只属于执行中任务的进度条。
- 最近任务、全局活动和风险提醒中的状态文字依赖隐式对齐，窄宽度下会出现气泡文字偏移或被截断。

**实现与验证：**

- `OverviewView.xaml` 现在只对执行中、等待中、等待确认的任务显示进度面板；成功/失败项恢复 Demo 的状态与时间布局。
- 统一最近任务状态、全局活动类型/结果和风险徽标的水平/垂直居中、禁止换行；风险徽标增加最小宽度并取消省略，确保“风险”等文字完整显示。
- `MediaCenterView.xaml` 的虚拟化缩略图解码宽度收敛到 96px，避免每张媒体卡片按 240px 解码造成不必要的首屏内存和滚动压力。
- `validate-source.py` 通过；隔离 Release 构建 `phase-overview-v20` 通过（0 warning/0 error）；WPF 校验 0 error、18 条既有 warning；0.6.70 包生成并安装成功，DLL 版本为 0.6.70.0。
- 重启并重新安装后，从真实 Playnite 生产插件入口打开首页复核：侧栏显示“设置”；最近任务仅运行/等待项显示进度；全局活动的“维护”“信息”徽标居中且无溢出；风险卡片中的“风险”完整显示。复核结束后已释放电脑控制。

**验证边界：**

- 本阶段真实宿主截图重点复核首页上述区域，不把一次首页复核宣称为 7 个页面已经完成像素级一致。
- 直接 Playnite 测试进程本轮出现无输出挂起；此前一键 Worker 测试仍有 2 个既有集成测试失败（191 项中 189 项通过）；Render QA 的两个媒体 resize 恢复用例尚未重新通过。因此本阶段只报告已实际通过的源检查、隔离构建、打包安装和首页宿主复核。

## 2026-08-17 UI-222 生产 Shell 入口边界与宿主审计修复

**原因定位：**

- 之前的生产界面同时存在旧 `DashboardDemoShell` 和新页面结构；旧结构虽已隐藏，但其尺寸计算与 Playnite 宿主审计仍会影响判断，造成“源码已改、启动后像没变”的错觉。
- 当前 Playnite 用户目录同时加载 `GameSaveCenter Preview` 0.6.71 和生产 `GameSaveCenter` 0.6.70。前者是独立的 Demo/预览插件，后者才加载 `AcrylicProductionShellView`；从预览入口进入不会显示生产改造。
- 生产 Shell 原先还保留 Demo 的“外观预览”徽标，已改为“生产版”，避免把生产入口误认成 Preview。

**实现与验证：**

- 生产 `DashboardView` 的可见层现在明确绑定宿主宽度并裁剪，唯一可见页面宿主为 `AcrylicProductionShellView.PageHost`；旧 `DashboardDemoShell` 仅保留为兼容字段并保持 `Collapsed`。
- Header 在 980 DIP 以下把游戏选择器和全局操作放入明确的第二行，避免 Playnite 窄宿主中被裁剪或假溢出；页面内部继续使用真实 ViewModel、Binding、Command 和项目滚动条。
- Real Host 审计已修正 Playnite 150% DPI 下 `TransformToAncestor` 像素/DIP 混用，以及受控窗口截图未按 DPI 输出导致的裁剪误判。
- `validate-source.py`、WPF UI 校验（0 errors）、Release 构建（0 warning/0 error）、Playnite 303/303 和 `render-qa OK` 已通过；本次真实宿主安装目录为生产 `GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`，启动日志同时确认 Preview 与生产两个插件均加载。
- 自动审计仍无法通过 UIAutomation 点击 Playnite 自定义侧栏，因此 `EmbeddedDashboardCaptured=false` 仍是真实边界；Controlled Host/RenderHarness 只能证明生产视图结构和布局，不冒充 Playnite 嵌入像素验收。

## 2026-08-17 UI-DIRECTION-RESET 页面级重构授权

**方向变更：**

- 用户明确要求 GameSaveCenter 进入完全页面级 UI 重构阶段；后续可以重新设计页面布局、信息架构、导航、Tab/Segmented、控件类型、共享模板、滚动模型和视觉层级。
- 上一轮 UiLab/AcrylicFork 迁移记录中的“不要恢复”“不要替换”“明确不迁移”仅用于解释历史决策，不再作为当前页面冻结规则。保留这些历史记录用于追溯，但新任务不得因为它们而拒绝整页改造。
- 仍需保护真实命令、Binding、数据契约、错误/取消/安全语义、可访问性、性能和 Playnite 兼容性；如新设计有意改变这些能力，必须在对应阶段说明并补齐验证。

**文档处理：**

- 已在 `AGENTS.md`、`docs/DEVELOPMENT_HANDOFF.md`、`docs/ai/PROJECT_MEMORY.md` 和 UI 门禁中加入当前方向声明，避免历史迁移条目覆盖本轮用户要求。

## 2026-08-17 UI-221 AcrylicFork 整页视觉迁移收口

**实现内容：**

- 以 `D:\workplace\github\GameSaveCenter.AcrylicFork` @ `b09cba6` 为基准复核生产全部工作区，页面骨架已与样板一致；本轮补齐四类样板头部信息：Media 的“待归类 N · 截图与录像自动归档 · 增量同步”、Trainer 的“已绑定工具 N · 修改器 · CT 表 · 自定义启动项 · 拖拽导入”、Save 的当前规则/校验状态胶囊、Maintenance 的安全模式/云端降级状态。
- 新增回归测试锁定这些头部说明与真实绑定，避免后续页面重构时把样板信息或计数删除。
- 尝试独立 `AcrylicParity.xaml` 时发现独立 ResourceDictionary 的 `BasedOn` 不能解析父级合并字典的 Gsc 样式，会直接造成页面构造期 `XamlParseException`；因此放弃别名层，生产页面继续直接使用 `Gsc*` 共享样式。此结论已写入项目记忆，不要重新创建该字典。
- demo 右上角色板、预览徽标、样例数据和样例滚动条仍未迁移；生产滚动条、圆角表头、DataGrid/ListBox 虚拟化和真实命令/绑定全部保留。

**验证结果：**

- `validate-source.py`、`check-xaml.ps1` 通过；WPF UI 校验 0 errors（16 条既有布局/主题 warning 保留）。
- Release 构建 0 warning / 0 error；Core 59/59、Worker 191/191、Playnite 304/304。
- RenderHarness 全量 `render-qa OK`：7 页面 × Light/Dark × 1040×700/1100×720/1366×768/2560×1440 + resize；证据在 `artifacts/ui-qa/acrylic-full-migration-v1`。
- `DEV-INSTALL-008` 安装成功，Playnite `extensions.log` 确认 `GameSaveCenter 0.6.70.0 loaded`，无新增 `XamlParseException`；real-host-audit 输出在 `artifacts/ui-host-audit-acrylic-v1`，`EmbeddedSettingsCaptured=true`、`ControlledDashboardCaptured=true`、`EmbeddedDashboardCaptured=false`（UIAutomation 无法点击侧栏，需用户手动打开后重跑才能得到真实嵌入像素）。

## 2026-08-16 UI-220 UiLab 几何对齐、维护二级导航与媒体回滚修复

**实现内容：**

- 修正共享 `GscRedesignWorkspaceTabItem`：页面内容 Stretch，分段标题在模板内部独立居中；同步修正维护、存档、媒体的本地 TabItem 对齐，避免表格和卡片被居中压缩。
- 将维护中心诊断内层从旧 `TabControl` 改为 UiLab 风格的 segmented ListBox + named panel host；保留问题列表/诊断概览的原始真实绑定、Inspector、命令和滚动表面，并补充二级切换事件处理。
- 修复 `VirtualizingWrapPanel` 的无限高度初测和生成器刷新恢复逻辑；在 RenderHarness 中加入媒体列表滚动到底再回顶的回归探针，并更新 segmented 页面选择/维护二级导航探测。
- 首页 Hero/当前游戏列按 UiLab 恢复 `1.35*:1`，紧凑宽度继续使用响应式堆叠；演示色板、窗口按钮、样例数据和样例滚动条仍未迁移。

**验证结果：**

- source 校验通过；WPF UI 校验 0 errors（既有 StackPanel/主题资源提示保留）；Playnite Release 构建 0 warning / 0 error。
- Core 59/59、Worker 191/191、Playnite 303/303；RenderHarness 双主题、多尺寸、resize 和媒体滚动回顶探针均 `render-qa OK`。
- 离屏截图已确认维护页二级标签同时可见、表格/详情分栏恢复、媒体卡片回顶不为空；真实 Playnite 当前仍受窗口自动化激活限制，未将离屏证据冒充宿主像素验收。

## 2026-08-16 UI-219 UiLab 分段页面骨架直迁与启动崩溃修复

**实现内容：**

- 不再只复用颜色和卡片样式；按 `D:\workplace\github\GameSaveCenter.UiLab\Pages` 的页面层级，将媒体、存档、修改器、维护四个生产页的顶层 `TabControl/TabItem` 骨架改为 UiLab 同款的两行 `Grid`：分段导航 + 命名面板宿主。生产页面的真实绑定、命令、Inspector、DataGrid/ListBox 虚拟化和生产滚动条保留；维护/诊断和审计内部仍保留 UiLab 对应的嵌套页签。
- 在 `Redesign.xaml`、`DesignTokens.xaml` 和运行时主题调色板中加入 UiLab 的两级 segmented surface，`GscRedesignSegmented`/`GscRedesignSegmentedItem` 与 UiLab 的圆角、内边距、选中态和透明度节奏对齐。右上角演示色板、窗口演示按钮、样例数据和样例滚动条没有迁移。
- 修复直接迁移后 WPF 在 `InitializeComponent()` 期间提前触发 `SelectionChanged` 的空引用：四个分段事件处理器在面板字段尚未生成时安全返回。同步修复 `VirtualizingWrapPanel` 的生成器插入索引边界和异常回退，避免之前的 `VisualCollection.Insert` 越界导致 Playnite 崩溃。
- 将结构测试从旧 `TabItem Header` 契约更新为新 segmented/panel 契约，仍保留对真实命令、绑定、表格有限视口和滚动约束的断言。

**验证结果：**

- `validate-source.py` 通过；WPF UI 校验 0 errors（仅既有布局/主题资源提示）；Release 构建 0 warning / 0 error。
- Playnite 测试 303/303；一键隔离构建通过 Core 59/59、Worker 191/191、Playnite 303/303；扩展已安装为 `extension.yaml 0.6.70`、DLL `0.6.70.0`。
- 真实 `extensions.log` 在 22:40:40 记录新 DLL 加载，启动后没有新的 `XamlParseException`、`NullReferenceException` 或 `VirtualizingWrapPanel` 崩溃。Computer Use 当前只能得到黑色截图和 `EmptyWindowAutomationPeer`，窗口激活失败，因此本阶段不把页面级像素截图写成真实宿主视觉验收；真实页面切换/截图仍需用户可交互的 Playnite 窗口后再复核。

## 2026-08-16 UI-218 UiLab 页面骨架迁入生产

**实现内容：**

- 按 `GameSaveCenter.UiLab` 的页面骨架重排生产工作区：Dashboard 不再在每个页面上方叠加重复的选中游戏卡、全局操作行和恢复安全横幅；游戏上下文继续保留在唯一的页面头部，页面内部负责自己的策略/比较安全提示。
- 备份中心策略页新增 Demo 同款三栏策略表面，保留生产策略绑定、命令、模板和云端/保留设置；比较页的三个指标卡在窄宽度使用可换行的固定最小宽度，避免数字与按钮互相覆盖。
- 修改器中心将当前游戏工具栏与拖入导入区拆成两层；任务中心将搜索、状态、类型与游戏筛选稳定为两行；历史版本、路径校验、媒体中心、维护中心继续沿用生产命令、Inspector、DataGrid/虚拟化和滚动条。
- UiLab 右上角演示色板、窗口演示按钮、样例数据和样例滚动条未迁移；只对用户指出的五类重叠做最小防重叠处理，没有替换生产滚动语义。

**验证结果：**

- `validate-source.py` 通过；WPF UI 校验 0 errors（15 warnings 为既有 StackPanel/主题资源提示）；Release WPF Rebuild 0 warning / 0 error。
- Playnite 回归测试 303/303；完整隔离安装构建通过 Core 59/59、Worker 191/191、Playnite 303/303，扩展已安装到真实 Playnite 并启动。
- 通过 Computer Use 在约 1303×673 的真实 Playnite 宿主中检查首页、备份中心、修改器中心和任务中心：全局重复安全横幅已消失，修改器工具栏可见，任务筛选已分行且无文字/按钮重叠。该人工截图不改写自动审计的 `EmbeddedDashboardCaptured=false` 事实边界。

## 2026-08-16 UI-217 真实 Playnite 人工视觉复核

**观察结果：**

- 基于 `81fde54` 的 Release 安装包在真实 Playnite 中加载成功，人工通过 Computer Use 进入 GameSaveCenter 后，首页已呈现开放式页面头部、真实游戏上下文、单一六项指标带、首页工作区卡片和生产页脚；未迁移 UiLab 右上角演示色板、窗口按钮或样例滚动条。
- 真实任务中心在 1303×673 宿主视口中加载成功：标题/副标题、四项任务统计带、筛选区、圆角表头 DataGrid、真实任务状态胶囊、进度条和生产滚动语义均可见；任务详情区按真实 `SelectedTask` 选择状态显示，默认无选中项时隐藏是当前生产逻辑，不是布局丢失。
- 误开的 GameSaveCenter 设置窗口已关闭，未保存任何设置。真实 Playnite 手动截图没有冒充 `real-host-audit` 的 `EmbeddedDashboardCaptured` 产物；自动审计因 UIAutomation 仍找不到侧栏入口而保留未捕获门禁。

**结论：**

- 用户此前提供的生产图与样板差距，主要混合了旧提交/旧页面状态、宿主外壳裁剪、真实数据差异和未选中 Inspector 状态，不能据此判断当前源码仍是旧四卡布局；本轮实机观察与 `artifacts/ui-qa` 离屏矩阵相互印证，首页与任务中心的主要层级迁移已经落地。

## 2026-08-16 UI-215 Task 统计栏与窄窗筛选迁移

**实现内容：**

- 对照 UiLab TasksPage，将生产任务中心顶部四张指标卡合并为一张圆角统计带，四个真实任务计数保留在同一表面内，并用三条细分隔线建立阅读节奏；统计值/标签顺序调整为 Demo 的数值在上、标签在下。
- 删除仅服务于四卡换列的 `UniformGrid.Columns` 响应式逻辑，保留任务队列有限视口、详情 Inspector、生产滚动条、DataGrid 行列虚拟化和所有真实命令/绑定。
- 修复窄窗“游戏:”标签与下拉框分离的问题，移动 `TaskGameFilterHost` 控件组时同时带上标签和下拉框。

**验证结果：**

- `validate-source.py`、`check-xaml.ps1`、Release 构建均通过（0 warning / 0 error）；Playnite 303/303。
- `GameSaveCenter.RenderHarness` 的 `artifacts/ui-qa/task-summary-band-v2` 覆盖 1040×700、1100×720、1366×768、1600×900、2560×1440，多尺寸 resize 和 Light/Dark 均 `render-qa OK`；1040 窄窗截图确认无孤立标签或文字重叠，任务 DataGrid 仍使用生产 Auto 滚动条。
- RenderHarness 是离屏证据；真实 Playnite 嵌入式 Dashboard 本阶段未重新捕获，宿主主题、DPI、键盘和连续缩放仍需人工确认。

## 2026-08-16 UI-216 Task 窄窗控件组回归防护

- 真实宿主隔离安装阶段发现一条旧测试仍假设 `TaskGameFilterComboBox` 直接位于筛选容器；实现迁移为“标签 + 下拉框”整体移动后，该假设已失效。
- 测试改为验证 `TaskGameFilterHost` 在紧凑/宽屏之间移动，并确认真实下拉框仍是控件组子项，避免以后为通过旧测试而重新引入孤立标签错位。
- 修复后 Playnite 303/303 通过；宿主审计需使用这一修订重新构建，当前尚未把上一轮失败的隔离安装结果当作视觉证据。

## 2026-08-16 UI-214 Overview 单卡统计栏与单色首页晕影

**实现内容：**

- 对照 `GameSaveCenter.UiLab` 的 Overview 截图和源码，将生产首页六个独立统计卡改成一个圆角统计表面，指标之间用细分隔线对齐 UiLab 节奏；真实 `Snapshot` 绑定、比例条和空库保护保持不变。
- 移除统计指标卡的悬停平移动画，避免非交互信息块出现卡片叠加感；生产页面的动画门控属性和其它交互不变。
- 将 Dashboard 根层三色环境光改为一个主题自适应的强调色磨玻璃晕影，保留首页氛围但不迁移 UiLab 的演示色板按钮或样例滚动条。

**验证结果：**

- `validate-source.py`、`check-xaml.ps1`、WPF UI 校验通过（0 errors）；Release 构建 0 warning / 0 error；Playnite 303/303。
- `GameSaveCenter.RenderHarness` 覆盖 1040×700、1100×720、1366×768、1600×900、2560×1440，Light/Dark 和 resize transition 均 `render-qa OK`；截图证实统计栏无文字重叠，生产滚动条未被替换。
- 证据目录：`artifacts/ui-qa/overview-single-band-v1`。RenderHarness 是离屏证据，不替代真实 Playnite 嵌入式 Dashboard 视觉验收。

## 2026-08-16 UI-213 真实宿主审计边界收口

**审计结果：**

- 用当前 `420483f` 执行 `scripts/real-host-audit.ps1 -Configuration Release -Output artifacts/ui-host-audit`；Release 构建、Core 59/59、Worker 191/191、Playnite 303/303、安装和 Playnite 启动均通过。
- 人工打开真实 Playnite 并进入 GameSaveCenter，确认实际宿主 Settings 窗口可见；`EmbeddedPlaynite` Settings viewport/scroll evidence 已写入 `artifacts/ui-host-audit`。
- `summary.json` 为 `EmbeddedSettingsCaptured=true`、`EmbeddedDashboardCaptured=false`、`ControlledDashboardCaptured=true`、`ProductionVisualSourceOfTruthAvailable=false`。自动 UIAutomation 未找到侧栏入口，故受控 Dashboard 截图不作为真实生产视觉真值。

**后续边界：**

- 本阶段只完成真实宿主加载与 Settings 抽查；Media 新缩略图网格的真实宿主视觉仍需人工进入 Media 页确认，不能以离屏 RenderHarness 代替。
- 离屏证据仍为 `artifacts/ui-qa/media-grid-migration-v3`，其 `render-qa OK`、Light/Dark 和多尺寸结果有效；生产滚动条/虚拟化边界继续保持。

## 2026-08-16 UI-205-REAL-HOST-MIGRATION-FIX AcrylicFork 生产迁移收口

**实现内容：**

- 将 AcrylicFork 的共享视觉层级继续收口到生产 Dashboard/Settings/Media/Trainer/Save 页面；演示专用右上角颜色按钮、演示数据和样例滚动条没有迁移。
- 修复 Playnite BAML 在私有扩展加载上下文中偶发找不到 `GameSaveCenter.Contracts` 的问题：生产 XAML 不再直接引用外部 Contracts 程序集，新增 `XamlEnumValues` 本地包装值，保留原始枚举对象以维持绑定和 DataTrigger 语义。
- 修复 Dashboard 选中游戏标题栏在首次测量/窗口缩放时保留旧 DesiredSize 的问题：标题、身份区和操作行按最终可用宽度约束，操作按钮窄屏换行，ApplicationIdle 再做一次收敛布局；Save/Media 的表格 viewport 使用短窗口可读性下限。
- Real Host 审计等待 ApplicationIdle 后再截图，并在后续干净轮次清理旧 `CHILD_LAYOUT_OVERFLOW` 门禁，避免把瞬时布局或上一轮证据当成最终失败。

**验证结果：**

- `validate-source.py`、`check-xaml.ps1`、Release 构建通过，0 warning / 0 error；Core 59/59、Worker 191/191、Playnite 303/303。
- `DEV-INSTALL-008` 重新打包安装成功，安装验证为 `extension.yaml 0.6.70`、DLL `0.6.70.0`；Playnite 扩展日志确认生产插件加载，未出现新的 `XamlParseException` 或 Contracts 缺失记录，外来 Preview Worker 保持运行。
- Real Host 受控矩阵覆盖 maximized、1600/1366/1280/1024 与 Light/Dark、工作区和内层 Tab；最终 `gates/overflow-classification.json` 的 `RealFixedLayoutOverflow` 为空，旧 `CHILD_LAYOUT_OVERFLOW.json` 已清除。

**验证边界：**

- `AUTO VERIFIED`：源码/XAML、构建、三组测试、受控截图/滚动证据、安装包和生产扩展加载日志。
- `MANUAL QA REQUIRED`：本机自动 UIAutomation 仍未找到 Playnite 左侧 GameSaveCenter 项，故审计诚实保留 `EmbeddedDashboardCaptured=false`；受控窗口证据不能冒充真实嵌入像素。前一轮人工观察到的真实嵌：首页、存档中心和任务中心在 1280/1024 尺寸下无明显文字重叠或右侧裁切。

## 2026-08-16 UI-205-ACRYLIC-PARITY AcrylicFork 视觉与页面结构迁移

**实现内容：**

- 纠正参考源：以 `D:\workplace\github\GameSaveCenter.AcrylicFork` 的 `b09cba6` 为权威视觉样板；迁移深色/浅色表面层级、Shell/卡片/控件圆角尺度、按钮与标题尺寸、状态色、页面标题和 Settings 分类栏节奏。
- Dashboard 顶部改为 AcrylicFork 式开放页面头部，保留生产中的真实游戏选择器、刷新/备份/同步命令和绑定；Overview 普通状态不再重复显示全局命令卡，仅在真实 onboarding 状态保留操作带。
- 调整 Dashboard 的游戏选择器/操作区响应式排布，Settings 在常用内容宽度使用左侧分类栏，低于 700 DIP 转为顶部横向分类，低于 620 DIP 进一步收紧标题与输入布局。
- 修正共享 DataGrid 表头、Button、Card、状态胶囊和页面标题的圆角/尺寸，使迁移后的控件不再依赖样板中的布局问题；滚动条继续使用生产模板和完整 Track 绑定。

**明确不迁移：**

- AcrylicFork 的演示数据、演示专用右上角颜色/主题按钮、样例滚动条均未引入；顶部七组色值仅作为生产主题参考，生产命令、数据绑定、虚拟化、页面滚动骨架和现有滚动条保留。

**验证结果：**

- `python scripts/validate-source.py`、XAML 检查通过；Release 0 warning/0 error；Core 59/59、Worker 191/191、Playnite 302/302。
- `scripts/render-qa.ps1 -Output artifacts/ui-qa/render-acrylic-correction` 全部通过，覆盖 Light/Dark、7 个工作区、多个窗口尺寸及 resize transition；修正渲染夹具的 Settings 入口动画时钟后，设置页主题截图不再为空，Overview/Save/Maintenance/Settings 无文字重叠。
- `DEV-INSTALL-008` 一键安装成功；安装器只处理当前扩展 Worker，外来 AcrylicFork Worker 保持运行，Playnite 与生产 Worker 均成功启动。

**验证边界：**

- `AUTO VERIFIED`：源码/XAML、Release 构建、三组测试、离屏截图和布局探针。
- `MANUAL QA REQUIRED`：本阶段未重新启动真实 Playnite 宿主；真实 Playnite 主题、DPI、键盘和连续缩放仍需在宿主内确认。

**下一步：**用户在真实 Playnite 中确认嵌入式页面；如无阻塞反馈，不再修 AcrylicFork 演示样本本身。

## 2026-08-16 UI-REAL-HOST-AUDIT-BLOCKERS-FIX 实施完成

- 来源：`GameSaveCenter_RealHost_Audit_Blockers_Prompt.zip`；只修 Audit 可信度。
- CommitSha：`real-host-audit.ps1` 启动前写 `GSC_UI_AUDIT_COMMIT`；metadata 优先级 env → AssemblyInformationalVersion → unknown；unknown 写 `AUDIT_SOURCE_REVISION_MISSING` HIGH。
- Embedded 身份：`IsGenuinelyEmbeddedDashboard`（Loaded + PresentationSource + Window 非 fallback），不再 static null 猜；headless 无人点击时诚实 `EmbeddedDashboardCaptured=false` + `REAL_EMBEDDED_DASHBOARD_NOT_CAPTURED` HIGH。
- SafeFileName 根因是 `string.Join("-", chars)` 每字符插 `-`；改为仅替换非法字符、折叠连续 `-`、保留中文。
- Overflow 分类：fixed/scroll/decorative/false-positive；ScrollViewer 内容与装饰性越界不再误报 HIGH，真实 fixed overflow 仍写 gate；分类统计入 `gates/overflow-classification.json`。
- Resize 稳定：最多 3 pass，等待 DataBind/Loaded/Render/Idle，连续两次几何差 ≤0.5 DIP 才截图，记录 responsive-stable.json。
- Scroll：排除 DG_ScrollViewer/PART_ContentHost/TextBox/ComboBox 内部 scroller；manifest 写真实尺寸/状态/Reason；`Scope=Dashboard|Settings` 隔离。
- 证据：summary CommitSha=d7948c9、Settings embedded=true、Controlled=true；ZIP `artifacts/GameSaveCenter-ui-host-audit.zip`。
- 测试：Playnite 302/302、Worker 191/191、Core 59/59；报告 `docs/ai/REAL_HOST_AUDIT_BLOCKERS_FIX_REPORT.md`。

## 2026-08-16 UI-HOST-AUDIT-TRUTHFULNESS-FIX 实施完成

- 来源：`GameSaveCenter_HostAudit_Truthfulness_Fix_Prompt.zip`；目标是让 Real Host Audit 的 origin、scroll semantics、manifest 与 success state 全部可信。
- Sidebar View 语义：删除 `SidebarItem.Activated` 调用（View 由 Playnite 在用户点击时走 `Opened`），审计就绪时发通知等待真实 Dashboard OnLoaded；fallback 专用窗口显式标记 `AuditHostKind.ControlledAuditWindow`，Dashboard 默认 `EmbeddedPlaynite`。
- Origin 显式化：不再用 `auditDashboardWindow == null` 猜 origin；`CompletedRoots` 按 `kind-dashboard` / `settings` 拆分，Controlled fallback 不阻断 Embedded capture。
- Manifest session 化：`AuditCaptureSession` 独立保存 EmbeddedDashboard/ControlledDashboard/Settings entries；settings manifest 只含 Settings，重复运行不串数据。
- Scroll 语义：`DG_ScrollViewer`/DataGrid 逻辑 item 滚动器不再生成像素长图，标记 `SkippedVirtualized`；`PART_ContentHost` 排除；`CaptureStatus/Reason/OutputWidthPx/OutputHeightPx/ViewportHeight/ExtentHeight/SegmentCount` 真实写入；CompletenessValidated 只在真实校验后为 true。
- 新增 `summary.json` 硬门禁与 `CHILD_LAYOUT_OVERFLOW` gate；`real-host-audit.ps1` 在 Embedded 未捕获时输出 PARTIAL 并以退出码 2 结束。
- 证据：summary `EmbeddedDashboardCaptured=false`（headless 无人工点击，诚实标注）、Settings embedded 10 条、Controlled 363 条、滚动面 86 条 validated；zip `artifacts/GameSaveCenter-ui-host-audit.zip`。
- 回归：Playnite 294/294、Worker 191/191、Core 59/59；报告 `docs/ai/HOST_AUDIT_TRUTHFULNESS_FIX_REPORT.md`。

## 2026-08-16 UI-REAL-HOST-CAPTURE-COMPLETENESS-FIX 实施完成

- 来源：`GameSaveCenter_RealHost_Capture_Completeness_Fix_Prompt.zip`；根因是截图语义模型错误，不是 UI 本身。
- 已确认两个 P0 根因并修复：
  - Controlled Host 过去同时把 Window.Width/Height 与 Dashboard.Width/Height 设成 profile，WPF Window 是 outer size，client 更小，导致右/底被 clip；真实 embedded 场景也不能改 Dashboard 尺寸。现改为无边框审计窗口（client == outer == profile），Dashboard `ClearValue(Width/Height)` + Stretch，并加 `CAPTURE_PROFILE_SIZE_MISMATCH` 断言。
  - `SaveFullPage` 过去只挑 ExtentHeight 最大的一个 ScrollViewer 当作整页，导致 sidebar/header/其他 scroller 缺失。现拆分为三种契约：`embedded-current/viewport`、`controlled/<profile>/<theme>/viewport`、`scroll-surfaces/<route>__<name>.png`，并输出 `capture-manifest.json`。
- 新增完整性断言：viewport 输出尺寸必须等于 `Actual*DpiScale`（`CAPTURE_VIEWPORT_CLIPPED` gate）、边界 sentinel 几何、Controlled client size 断言、所有 meaningful ScrollViewer 枚举。
- 元数据改为真实值：`Mode=embedded-current|controlled-host-window`、`CaptureOrigin`、`DedicatedAuditWindowUsed`、`ProfileSizeApplied`、`ThemeOverrideApplied`；PlayniteDesktopVersion 不再回退成假精确的 1.0.0.0。
- 测试：新增 `UiAuditCaptureContractTests`（6 项：controlled 不改 Dashboard 尺寸、embedded 不 resize/不改主题、DPI 像素契约、多 ScrollViewer 枚举、右下 sentinel 几何、manifest 字段）；Playnite 287/287、Worker 191/191、Core 59/59。
- 最终端到端运行：603 个文件，无 CAPTURE gates；controlled 尺寸 1024/1280/1366 精确命中，1600 受工作区限制为 1600x912；scroll-surfaces 197 张；Settings 由 Playnite 托管时如实标记 `embedded-current-settings`；zip `artifacts/GameSaveCenter-ui-host-audit.zip`。
- 报告：`docs/ai/REAL_HOST_CAPTURE_COMPLETENESS_FIX_REPORT.md`。

## 2026-08-15 LUDUSAVI-DIAGNOSTICS-FIX 实施完成

- 用户诊断：备份失败直接原因是 Ludusavi 联网更新 `raw.githubusercontent.com/.../manifest.yaml` 超时（22:08/22:12 失败，网络恢复后重试成功），不是存档路径或 ZIP 写入问题。
- 三个放大问题的修复：
  - `ExternalProcessRunner` 为 stdout/stderr 显式设置 `Encoding.UTF8`，避免中文错误按系统代码页解码成乱码。
  - `LudusaviClient.ExecuteJsonAsync` 失败分支现在把原始 stdout（stdout 为空时用 stderr）写入 `RawOutput`，任务详情不再显示空 RawOutput。
  - `DashboardViewModel` 新增 `CopyTextWithRetry`：复制详情/健康报告/诊断信息时对剪贴板占用（`CLIPBRD_E_CANT_OPEN`/COMException）重试 3 次，最终失败显示明确提示而不是弹出异常。
- 测试：新增 UTF-8 解码回归测试；Worker 191/191、Playnite 281/281；Release 0 warning/0 error；`validate-source.py`、`check-xaml.ps1` 通过。
- 备注：Ludusavi manifest 网络问题属外部环境，插件侧不重试 manifest 下载；失败时保留原始输出便于用户按 `raw.githubusercontent.com` 超时定位。

## 2026-08-15 UI-REAL-HOST-AUDIT-NESTED-TABS-THEMES 实施完成

- 用户复核继续反馈：Tab 下的嵌套 Tab（如“异常与审计”→“审计记录”）缺失；需要浅色/深色两套截图；截图仍只显示页面局部。
- 修复：
  - `CaptureAllInnerTabs` 改为递归捕获嵌套 TabControl（选择父 Tab 后等待 ApplicationIdle，再从视觉树与 Content 双路径找子 TabControl），已产出 `tab-maintenance-异-常-与-审-计-审-计-记-录.png` 等嵌套页。
  - 真机审计按 `Light`/`Dark` 双主题各跑一遍；Dashboard/Settings 新增 `ApplyThemeForAudit`，设置页兜底窗口现在注入真实 `GameSaveCenterSettings` 作为 DataContext。
  - 整页截图改为始终渲染 Dashboard/Settings 根元素（含侧栏/外壳）并按内容最大滚动高度撑高后输出，不再只截内部滚动器或窗口可视区；maximized 工作区截图现为 1707×1232（Overview 1707×1717）。
- 产物：`artifacts/ui-host-audit/screenshots/<size>/<light|dark>/`，嵌套 Tab、双主题、整页长图齐全；最终 zip `artifacts/GameSaveCenter-ui-host-audit.zip`（约 86MB）。
- 验证：Release 0 warning/0 error；`validate-source.py`、`check-xaml.ps1` 通过。第三方主题/真实 Playnite 窗口内观感仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-REAL-HOST-AUDIT-MULTI-SIZE 实施完成

- 用户复核后补充要求：真机截图要覆盖最大化窗口与多种窗口化尺寸，且项目协作规则要加入“及时清理无用文件”。
- 多尺寸捕获：`RealHostUiAuditService` 现按 `maximized`（实际 WorkArea 1707×912 DIP）+ `1600x1000`、`1366x768`、`1280x720`、`1024x768` 五档循环，每档重新截取 Dashboard、6 个工作区、全部内层 Tab、窗口截图与 Settings 5 个分类。
- 关键修复：
  - Playnite 进程为 DPI-unaware，`GetSystemMetrics` 返回虚拟化小尺寸，导致窗口被内容吸成 640×480；改用 `SystemParameters.WorkArea` 并 `SizeToContent.Manual` + 显式给 Dashboard/Settings 设置宽高，窗口尺寸才与档位一致。
  - 内容截图按逻辑分辨率输出，避免 32 位进程整窗口 1.5× 渲染 OOM；真实 DPI/尺寸记录在 `metadata-<size>.json`。
  - 移除了多尺寸扫描中的全页滚动拼接（单张表格拼接可卡十几分钟），保留页面/Tab/窗口截图；滚动证据仍可由单尺寸离屏审计提供。
- 产物：`artifacts/ui-host-audit/`（5 个尺寸目录 × 6 workspace + 内层 Tab + window + Settings 5 分类）与 `artifacts/GameSaveCenter-ui-host-audit.zip`（约 33MB，379 个文件）。
- 清理：AGENTS.md 与 DEVELOPMENT_HANDOFF 新增“文件清理规则”；删除 `artifacts/` 旧 `dev-build`（7.4GB）、`ui-audit-build`、`phase*`、`audit*`、旧 zip 与 `.tmp/` 全部旧审计目录，仅保留当前安装目录、`.pext`、当前审计输出与日志。
- 验证：Release 0 warning/0 error；`validate-source.py`、`check-xaml.ps1` 通过。真实第三方主题/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-REAL-HOST-AUDIT-FULL-COVERAGE 实施完成

- 用户复核真实宿主截图后指出：脚本运行到 Settings 时停住、只截页面左上部分、未覆盖全部页面；另反馈审计兜底窗口部分超出屏幕且无法关闭。
- 根因与修复：
  - Playnite 主窗口在无交互桌面会话中不可见，UIA 侧栏点击不可靠；插件启动时若存在 `GSC_REAL_HOST_AUDIT`/sentinel，延迟自动激活侧栏项。
  - 侧栏激活仍不落地时，`RealHostUiAuditService.EnsureDashboardCaptured` 在专用窗口（1440×900、左上锚定、ToolWindow 可关闭）内创建真实 `DashboardView` 并触发同一套 Tier B 捕获；Settings 通过 UI 线程专用窗口兜底捕获。
  - Settings 兜底此前失败的两个原因：Dashboard 完成后先删 sentinel，兜底再读 sentinel 得到空路径；以及兜底窗口创建曾调度到线程池 Dispatcher。现改为 `RequestSettingsCapture` 时缓存输出根并直接传入 Dashboard 的 UI Dispatcher。
  - 设置页分类 Header 是复杂 Grid，`Header.ToString()` 全部同名导致去重只截第 1 类；改为从 Header 视觉树提取中文标题，并按索引去重，5 个分类全部截图。
  - `CreateZip` 遇到默认 zip 被占用时改写入带时间戳的独立 zip，不再中断后续 Settings 捕获。
- 产物（`artifacts/ui-host-audit/` + `GameSaveCenter-ui-host-audit.zip`）：Dashboard 6 个 workspace 截图/完整窗口截图、全部内层 Tab、`full-scroll/` 整页拼接（Overview 1099×2579 等）、真实 DPI 1.5 metadata、resource/style/visual-tree；Settings 5 个分类截图 + 完整窗口截图 + SettingsScroller 整页拼接 + metadata。
- 验证：Release 0 warning/0 error；`validate-source.py`、`check-xaml.ps1`、WPF UI 校验 0 errors；Core 59/59、Worker 190/190、Playnite 281/281（dev-install-run 已跑）。
- 说明：无交互桌面下捕获的是“真实 DashboardView + 真实 adaptive palette + 真实 DPI”的专用窗口证据；有可见 Playnite 窗口时会走真实窗口截图路径。真实第三方主题/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-REAL-HOST-PARITY-CLOSURE 实施完成

- 来源：`GameSaveCenter_RealHost_UI_Parity_Audit_Prompt.zip`；计划 `docs/ai/REAL_HOST_UI_PARITY_CLOSURE_PLAN.md`；报告 `docs/ai/REAL_HOST_UI_PARITY_CLOSURE_REPORT.md`。
- 建立 Tier A（Offscreen Regression Audit）与 Tier B（Real Playnite Host Fidelity Audit）双审计链路；README 已明确离屏截图不是 production pixel truth。
- 插件新增 `RealHostUiAuditService`：`GSC_REAL_HOST_AUDIT`/sentinel 触发，从真实 Dashboard 捕获截图、visual tree、resource snapshot、style fingerprint、DPI、bounds，并自动打开 Settings 捕获设置页；不触发业务命令。
- 新增 `UiDiagnosticsExporters` 与 `AdaptiveThemePaletteContrastGuard` 及单测。
- 本机真实宿主捕获成功：DPI 1.5、Dashboard 1264×868、6 个 workspace 证据；发现离屏“更漂亮”主因是 fallback palette vs runtime adaptive palette + 真实 DPI/host bounds/data。
- 新增 `docs/HOST_STYLE_DEPENDENCY_REPORT.md` 与 `docs/HOST_VISUAL_PARITY_REPORT.md`。
- AGENTS.md / DEVELOPMENT_HANDOFF 新增“每轮完成后由 Agent 自己 commit 并 push”项目协定。
- 验证：Release 0 warning/0 error；Core 59/59、Worker 190/190、Playnite 281/281；render-qa 全绿；Offscreen UI Audit 0 HIGH/0 MEDIUM/0 fidelity/0 failed routes。
- 剩余：真实 125-200% DPI、第三方主题、连续缩放与 Settings paired evidence 仍需人工/下次脚本运行确认。

## 2026-08-15 UI-AUDIT11-RESIDUAL-CLOSURE 实施完成

- 来源：`GameSaveCenter_Audit11_Residual_UI_Closure_Prompt.zip`；计划 `docs/ai/UI_AUDIT11_RESIDUAL_UI_CLOSURE_PLAN.md`；报告 `docs/ai/UI_AUDIT11_RESIDUAL_UI_CLOSURE_REPORT.md`。
- SaveHistory 大小列：90 → 116 DIP，`SaveSizeValue` 使用 `TextTrimming=None` + `Tag=SaveHistorySize`；narrow 状态列仍完整。
- Maintenance Device：Compact/Narrow 改为详情切换（查看设备详情 ›），展开 viewport >= 180 DIP，表格保留 header + 2 行。
- Audit 新增 `SHORT_SEMANTIC_VALUE_TRIMMING` 与 `INTERACTIVE_INSPECTOR_USABILITY` 失败门禁；Settings 滚动目标整数取整。
- 验证：Release 0 warning/0 error；Core 59/59、Worker 190/190、Playnite 276/276；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 fidelity/0 failed routes。
- 审计 ZIP 因标准路径被其他进程锁定，输出到 `artifacts/audit11-final/GameSaveCenter-ui-audit.zip`。
- 真实 Playnite 宿主/DPI 125%/150%/主题/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FIDELITY-CLOSURE-AUDIT10 实施完成

- 来源：`GameSaveCenter_UI_Fidelity_Closure_Audit10_Prompt.zip`；计划 `docs/ai/UI_FIDELITY_CLOSURE_AUDIT10_PLAN.md`；报告 `docs/ai/UI_FIDELITY_CLOSURE_AUDIT10_REPORT.md`。
- Maintenance：移除局部 implicit `DataGridColumnHeader` style，中间列表头恢复渲染（Diagnostics/Device/Audit Findings/Audit Log/Process 共 11 个 header）；filler header 继续由共享模板主题化。
- Media：搜索框改为 `Auto/*(MinWidth=160)/Auto/150` Grid，不再 54.67 DIP；narrow 内容宽约 390 DIP。
- Settings：选中分类在 SelectionChanged/ApplyResponsiveLayout 后同步 scroll-into-view，保留 BringIntoView 回退并用增量 delta 收敛；最后分类 HorizontalOffset=359.33。
- Save History：narrow 收起备注 summary 列，状态列留在视口内；完整备注仍在 Inspector。
- Audit：新增 `HEADER_CONTENT_FIDELITY`、`ACTIVE_TAB_VISIBILITY`、`CONTROL_USABILITY_GEOMETRY`、`ESSENTIAL_COLUMN_VISIBILITY`，全部 MEDIUM + 失败门禁；修复前可复现 92 个失败，修复后 0。
- 验证：Release 0 warning/0 error；Core 59/59、Worker 190/190、Playnite 273/273；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 failed routes/0 fidelity。
- 提交：见 `git log`。真实 Playnite 宿主/DPI 125%/150%/主题/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-POST-TYPOGRAPHY-GEOMETRY-CLOSURE 实施完成

- 来源：`GameSaveCenter_PostTypography_Geometry_Audit_Fix_Prompt.zip`；计划 `docs/ai/UI_POST_TYPOGRAPHY_GEOMETRY_CLOSURE_PLAN.md`；报告 `docs/ai/UI_POST_TYPOGRAPHY_GEOMETRY_CLOSURE_REPORT.md`。
- 修复 Maintenance 诊断“等级”列 72 → 共享 `GscSeverityColumnWidth`（92 DIP），异常审计同列统一引用；`GscRedesignTableStatusPill` 保持内容自适应。
- 全仓固定几何扫描：仅修改确认挤压的等级列；Task/Save/Media/Audit/Process 短列与按钮 MinWidth 核对后保持不变，未全局增高。
- UI Audit 新增 Text-Fit 检测：`FormattedText` 无约束宽度 vs `ActualWidth`，排除 wrap/ellipsis；`TEXT_FIT` 为 MEDIUM 且 audit 直接失败。
- visual-tree exporter 修复：离屏 host `IsVisible` 恒 false → 改用 `Visibility == Visible`；175 个 JSON 全部非空。
- 验证：Release 0 warning/0 error；Core 59/59、Worker 190/190、Playnite 268/268；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 failed routes/0 TEXT-FIT。
- 提交：见 `git log`。真实 Playnite 宿主/DPI 125%/150%/主题/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FINAL-TYPOGRAPHY-RESPONSIVE-CLOSURE 实施完成

- 来源：`GameSaveCenter_Final_UI_Typography_Prompt.zip`；计划 `docs/ai/UI_TYPOGRAPHY_RESPONSIVE_CLOSURE_PLAN.md`；报告 `docs/ai/UI_TYPOGRAPHY_RESPONSIVE_CLOSURE_REPORT.md`。
- 字体：新增 `GscUiFontFamily`（含 Microsoft YaHei UI fallback）与 `GscCodeFontFamily`；普通 UI 不再硬编码 `Segoe UI Variable Text, Segoe UI`；`Segoe MDL2 Assets` 与 `Consolas` 保留；通用按钮默认 SemiBold → Medium，Primary 保留 SemiBold。
- Settings：Compact/Narrow header 收紧（隐藏长说明/副标题/保存提示、缩小 icon 与 padding）；760×560 正文 viewport 300 DIP。
- Save Compare：窄窗主比较区 MinHeight 240、MaxHeight `max(300, height*0.52)`；1040×700 viewport 234 DIP。
- Compact Inspector：Save/Trainer/Media/Task 五个详情按钮移入表格下方独立操作行，不再覆盖状态文字。
- Media 待归类底栏：`Margin="12,10,12,0"`，与表格内容左边缘对齐并保留窄窗换行。
- 验证：Release 0 warning/0 error；Core 59/59、Worker 190/190、Playnite 266/266；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/0 失败路由。
- 提交：见 `git log`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FINAL-POLISH-V7.1 实施完成

- 计划：`docs/ai/UI_FINAL_POLISH_PLAN_V7_1.md`；报告：`docs/ai/UI_FINAL_POLISH_REPORT_V7_1.md`。
- 首页活动行改为五列：Icon / Scope / Message(*) / MetaChip / Time；chip 独立横向列组，不再与 scope 上下堆叠；Time 右留白 20 DIP。
- Maintenance 六个指定 clipping 与全量 `POSSIBLE_CLIPPING` 清零；Audit 消息带元素名/父元素/文本片段，并扣除 Margin 误报。
- 验证：Playnite `263/263`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；最终 Audit 0 HIGH/0 MEDIUM/0 失败路由。
- 提交：`702b0d5`、`f6f17a8`。真实 Playnite 主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FINAL-CLOSURE-V7 实施完成

- 计划：`docs/ai/UI_FINAL_CLOSURE_PLAN_V7.md`；报告：`docs/ai/UI_FINAL_CLOSURE_REPORT_V7.md`。
- Audit 6 的路由错误已根修：维护中心按 `OuterTab + InnerTab + ExpectedPrimaryElement` 生成独立子路由，截图前断言 expected==actual，失败路由 0。
- 宽屏列 Fill：新增共享 `DataGridStarFill`（记录星号权重并按剩余宽度分配），六张 2K 表 ColumnFillRatio 0.18~0.48 → 1.00；MaintenanceProcess 目标游戏 20 → 1549 DIP。
- 纵向 Stretch：Task 根 ScrollViewer 改有限 Grid，Media Inbox/Current 取消 460 上限，Task/Media 主行 fill 1.00。
- Maintenance 表头白块：`GscLastColumnHeader` 去掉 `OverridesDefaultStyle`，统一 `MaintenanceLastColumnHeader`，`HEADER_WHITE_BLOCK=0`。
- Progress：新增 `GscProgressTrackBrush/GscProgressFillBrush`，模板补 `PART_Track`；0/5/25/50/75/100 探针 fill 占比 0.00/0.04/0.24/0.49/0.74/0.99。
- 单行 TextBox：`PART_ContentHost` Stretch + Padding 收口，`SINGLE_LINE_CONTENTHOST_VERTICAL_SCROLL=0`。
- Task narrow：1040×700 outer scroll=0、4.04 行、父子滚动冲突 0。
- 验证：Playnite `263/263`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；最终 Audit 0 HIGH/0 MEDIUM/0 失败路由。
- 提交：`5cd0226`、`58191d5`、`494b402`、`87d0553`、`7eaaacd`。真实 Playnite 主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-FEEDBACK-GLOBAL-ACTIVITY-CHIP-CENTER

- 用户反馈首页“全局活动”中“备份成功”等结果气泡文字不是居中。
- 根因：宽/窄两套 Kind/Result chip 的 TextBlock 未设置水平与文本对齐，Border 被固定列宽撑开后文字默认靠左。
- 修复：`OverviewView.xaml` 四个 chip TextBlock 统一加 `HorizontalAlignment=Center / VerticalAlignment=Center / TextAlignment=Center`；`UiLayoutRegressionTests` 新增居中回归断言。
- 验证：XAML/source 门禁通过；Playnite `263/263`；v6.2 截图刷新到 `artifacts/ui-qa/v6-2-shots/`。
- 提交：`d962b4d`。

## 2026-08-15 UI-TABLE-AND-CHIP-CLOSURE-V6.2 实施完成

- 计划：`docs/ai/UI_TABLE_AND_CHIP_CLOSURE_PLAN_V6_2.md`；报告：`docs/ai/UI_TABLE_AND_CHIP_CLOSURE_REPORT_V6_2.md`。
- Chip 从胶囊改圆角矩形：`GscRedesignContextPill` / `GscRedesignTableStatusPill` CornerRadius 统一 7，ContextPill MinHeight 26。
- 时间列不再贴边：共享 `DataGridCell` Padding `12,8,20,8`，Overview 时间列 `Margin=12,0,20,0`，六列 `40|150|*|96|84|112`。
- SaveCandidate 可信度列改为真实 ProgressBar（Height 8，Maximum 1）+ `P0` 文本；Task/Overview 原有进度条未重复改。
- Maintenance 四个主表取消 460 DIP `MaxHeight` 上限，Device/Process 布局 `VerticalAlignment=Stretch`；2K/4K fill ratio：Diagnostics 0.89/0.93、Device 0.82/0.88、Audit 0.88/0.92、Process 0.90/0.93。
- RenderHarness 新增 `v6-2shots` 模式与 3840×2160 档，脚本 `scripts/capture-v6-2-shots.ps1`，截图 `artifacts/ui-qa/v6-2-shots/`。
- 验证：Release 0 warning/0 error；Playnite `263/263`；render-qa 11 档 + 56 主题 + 7 Resize 全绿；UI Audit 0 HIGH/0 MEDIUM/8 EXPECTED INFO；source/XAML/WPF 门禁通过。
- 提交：`c58b359`、`6a68a59`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-OVERNIGHT-CLOSURE-V6 实施完成

- 计划：`docs/ai/UI_OVERNIGHT_CLOSURE_PLAN_V6.md`；报告：`docs/ai/UI_OVERNIGHT_CLOSURE_REPORT_V6.md`。
- 页面历史改为 Playnite 会话级：首次打开 Overview，同会话恢复，Playnite 重启回 Overview；不再使用持久化 LastWorkspace；Onboarding 首页显示 CTA 显式进入 Maintenance。
- `GscNumericFieldInput` 根修：`PART_ContentHost` 绑定 `VerticalContentAlignment`，数字 1/5/30/120/1440 完整居中显示。
- 全局活动改为轻量六列表格并加 header；Overview 主列/次列 disabled ScrollViewer 改为 Grid；Maintenance Device/Process 与 Media Current 外层 ScrollViewer 改为有限 Grid。
- Task/Media 筛选补语义前缀；Device/Process 主表最小视口 252 DIP；UI Audit 0 HIGH/0 MEDIUM/8 INFO、TRUE_PARENT_CHILD_SCROLL_CONFLICT=0。
- 验证：Release 0 warning/0 error；Playnite `261/261`；render-qa 10 档 + 56 主题 + 7 Resize 全绿；source/XAML/WPF 静态门禁通过。
- v6 截图：`artifacts/ui-qa/v6-shots/`，命令 `scripts/capture-v6-shots.ps1`。
- 提交：`baa8f72`（计划）及实施/文档提交见 `git log`。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-15 UI-OVERNIGHT-FIX-V4 实施完成

- 计划：`docs/ai/UI_OVERNIGHT_FIX_PLAN_V4.md`；报告：`docs/ai/UI_OVERNIGHT_FIX_REPORT_V4.md`。
- `GscDisclosureCard` 模板升级为独立 chevron 图标区，全项目 Expander 统一且无内部滚动；维护中心诊断页拆成 `问题列表 / 诊断概览` 二级 Tab，FindingsGrid 独占主工作区。
- 首页全局活动行收敛为 60 DIP，图标垂直居中，时间列 112 DIP；存档备份自动化所有数值输入补齐 label/unit/helper。
- 新增 `OvernightV4SharedTests`、`OvernightV4SaveFormTests`、`OvernightV4MaintenanceTests`；Playnite `255/255`，render-qa 10 档 + 56 主题 + 7 Resize 全绿，UI Audit 0 HIGH/0 MEDIUM/39 INFO/0 失败路由。
- v4 截图：`artifacts/ui-qa/v4-shots/`，命令 `scripts/capture-v4-shots.ps1`。
- 用户后续反馈修复：折叠 header 文字改为与图标垂直居中；存档备份自动化数字输入改为水平/垂直居中显示，框尺寸不变；提交 `fc86ecc`。
- 提交：`3015182`（计划）、`5131e4d`（共享样式+首页）、`0201615`（存档表单）、`5196f4a`（维护中心）、`fc86ecc`（对齐修复）。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-14 UI-VISUAL-REWORK-V3 实施完成

- 图一：Overview 当前游戏卡三个按钮统一 `GscWpfUiCompactButton` 几何（38/88 DIP），Primary 只改 `Appearance`，标准宽度同排、窄窗自然 2+1。
- 图二：最近 30 天动作按钮与统计 chips 分层；统计 chips 中性/警告配色；所有 Expander/Disclosure 统一到 `GscDisclosureCard`，去掉尾部 `>`，Chevron 独立图标区并居中。
- 图三：全局活动改为轻量表四列（Icon / Main / Chips / Time 112 DIP），行高 64 DIP、hover 分隔线；窄窗 chips 移到摘要下方。
- 图四：Save 当前存档规则状态合并为一行 badge，按 `SelectedGame.HealthState` 着色（风险=橙、正常=绿、待确认/中性=灰蓝、运行=蓝），三按钮统一 38/100 DIP 紧凑几何；卡片 padding 收敛为 12/10。
- 图五：Maintenance Diagnostics 环境卡压缩为摘要行，首次环境检查与低频维护进入 `GscDisclosureCard`；FindingsGrid 五列收敛为 72/120/160/*180/140，1040×700 下五列表头完整且首屏可见。
- 补强：Media 来源表单、Save 策略模板、Task 更多筛选也统一到 `GscDisclosureCard`，`GscExpander` 不再被页面引用；Maintenance 两个主 Disclosure 内容改为内部有限滚动（`EnvironmentCheckDisclosureScroller` / `MaintenanceActionsDisclosureScroller`），展开后不会挤压 FindingsGrid。
- 按钮对齐探针：render-qa 新增 Overview/Save 三按钮 Y 坐标与高度差检查；探针抓到 1536×864 下“查看需关注项”换行，已将 Hero/当前游戏列从 1.1:0.9 调整为 1:1，10 档尺寸全部同排同高。
- 功能保真：REMOVE=0；`DetectPathsCommand/ValidateCommand/LoadDetailsCommand`、全部诊断/维护命令、FindingsGrid 5 列与 EnvironmentCheckItems 均保留；GamePicker HARD LOCK 未改。
- 验证：Release 构建 0 warning/0 error；Core `59/59`、Worker `190/190`、Playnite `253/253`；`check-xaml.ps1`、`validate-source.py`、WPF 静态审查（0 error）通过；render-qa 10 档 + 56 主题场景 + 7 Resize 全绿；UI Audit 0 HIGH / 0 MEDIUM / 32 INFO / 0 失败路由。
- 截图证据：`artifacts/ui-qa/v3-shots/`（10 张标准/窄窗/展开态）、`artifacts/ui-qa/v3-final/`；Audit 汇总：`artifacts/ui-audit/v3-final/AUDIT_SUMMARY.md`。截图命令：`scripts/capture-v3-shots.ps1`。
- 提交：`5c3bdae`（计划）、`9ee3660`（Overview）、`e8b8c31`（Save/Maintenance），后续补强与文档将在最终提交中记录。真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-14 UI-VISUAL-REWORK-V3 计划

- 用户提供 `GameSaveCenter_UI_Design_and_Prompt_Pack_v3.zip`，要求按 5 张截图定向重构，不继续泛化优化。
- 已生成 `docs/ai/UI_VISUAL_REWORK_PLAN_V3.md`：逐图列出问题、受影响 x:Name、当前结构、拟改结构、KEEP/MOVE/RESTYLE、保留 Binding、预期标准/窄窗样式与验收方法。
- 按 v3 要求先提交计划，随后按 图一 → 图二/Disclosure → 图三 → 图四 → 图五 → 颜色/Audit → 文档 的顺序实施。

## 2026-08-14 UI-VISUAL-CORRECTION-V2 真实宿主 reload 验证

- `scripts/dev-install-run.ps1 -Configuration Release` 一键构建安装成功并启动 Playnite：Release 0 warning / 0 error，Core `59/59`、Worker `190/190`、Playnite `250/250`。
- 真实宿主证据：`playnite.log` `20:39:57` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`；`extensions.log` `20:39:59` 记录 `GameSaveCenter 0.6.70.0 loaded`；Worker PID `36980` 从当前扩展目录运行；`20:39` 后无 ERROR/Exception/crash。
- 真实 Playnite 主题视觉、DPI 与连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-14 UI-VISUAL-CORRECTION-V2 实施完成

- Overview：根页成为唯一纵向滚动；主列/右列/风险卡取消滚动职责；风险卡去内滚与死空间；最近保护明细使用 `GscDisclosureCardExpander`；全局活动按 Tag 切换宽/紧凑行。
- Save：`SaveCurrentRuleCard` 压成紧凑四列，三按钮等高/等宽，窄窗动作移到第二行；`SaveCandidateGrid` 最小视口提到 252 DIP。
- Maintenance：Diagnostics 外层改 Grid，去掉 `Outer Scroll > FindingsGrid` 父子双滚动；环境明细、低频维护、运行状态收进 Disclosure；Audit 改为“发现的问题 / 审计记录”二级切换，同一时刻只显示一张主表。
- Audit 新增 v2 断言：OV-001/002/003/005、SAVE-001/002、MAINT-001/002/003；最终 UI Audit HIGH `0`、MEDIUM `0`、运行时警告 `33`、161 快照、失败路由 `0`。
- render-qa 10 档尺寸 + 56 主题场景 + 7 Resize 恢复全部通过；Core `59/59`、Worker `190/190`、Playnite `250/250`。

## 2026-08-14 UI-VISUAL-CORRECTION-V2 计划

- 用户提供 `GameSaveCenter_UI_Visual_Correction_Pack_v2.zip`，本轮不推翻上一轮重构，只关闭截图/Audit 仍暴露的问题。
- 已生成 `docs/ai/UI_VISUAL_CORRECTION_PLAN_V2.md`，覆盖 Overview 单滚动、风险卡去内滚/去空白、Disclosure 视觉、全局活动响应式行、Save 当前存档规则卡、Maintenance Diagnostics 去双滚动、Audit 二级切换与新增 Audit 断言。
- 按包要求先提交计划，随后按 P2 → P0/P1/P3 → P4 → P5 → P6 → P7 顺序实施。

## 2026-08-14 UI-REFACTOR-V1 真实宿主 reload 验证

- `scripts/dev-install-run.ps1 -Configuration Release` 一键构建安装成功：Release 0 warning / 0 error，Core `59/59`、Worker `190/190`、Playnite `250/250`，扩展安装到 `C:\Users\lopmatu\AppData\Roaming\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec` 并启动 Playnite。
- 真实宿主证据：`playnite.log` `18:15:48` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`；`extensions.log` `18:15:50` 记录 `GameSaveCenter 0.6.70.0 loaded`；Worker PID `11056` 从当前扩展目录运行；`18:10` 后 `playnite.log` 与 `extensions.log` 无 ERROR/Exception/crash。
- 真实 Playnite 主题视觉、DPI 与连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-14 UI-REFACTOR-V1 验收审计文档

- 新增 `docs/ai/UI_REFACTOR_ACCEPTANCE_AUDIT.md`，把实施包 `06_ACCEPTANCE_CHECKLIST.md` 逐项映射到仓库证据，并区分 `AUTO VERIFIED` 与 `MANUAL QA REQUIRED`。
- Inspector 恢复修复后重跑最终 UI Audit：HIGH `0`、MEDIUM `0`、运行时警告 `39`、161 快照、失败路由 `0`。

## 2026-08-14 UI-REFACTOR-V1 Resize 恢复探针与 Inspector 恢复修复

- `render-qa` 新增 Resize transition QA：同一视图实例按 2560×1440 → 1100×720 → 2560×1440 依次布局，快照 DataGrid/ListBox/命名 ScrollViewer 的尺寸、可见性与滚动条，断言回到大窗后几何恢复。
- 探针抓到并修复真实缺陷：Save/Task/Trainer 的 Inspector 在宽→窄→宽后停留在窄窗收起状态；现在非 compact 分支会按当前选择显式恢复 Inspector 可见性。
- 新增 3 条回归测试（Save/Task/Trainer `InspectorRestoresAfterCompactResize`），并让旧“列释放”测试在验证宽窗时设置真实选中项、在 compact 验证 Auto 行前走真实详情切换。
- `render-qa` 10 档尺寸 + 56 主题场景 + 7 视图 Resize 恢复全部通过；Core `59/59`、Worker `190/190`、Playnite `250/250`。

## 2026-08-14 UI-REFACTOR-V1 页面级横向溢出门禁

- render-qa 与主题 QA 现在都会检查页面级 `*ScrollSurface` 与 `SettingsScroller`：`hbar` 必须为 `Disabled` 且无横向溢出；DataGrid 内部 `hbar=Auto` 的列滚动仍被允许。
- 报告行新增 `hbar` 与 `hscrollable` 探针；10 档尺寸矩阵 + 56 个 Light/Dark 主题场景全部通过，页面级横向溢出为 0。
- Release 0 警告/0 错误；Core `59/59`、Worker `190/190`、Playnite `247/247`；source/XAML/WPF 门禁通过。
- 真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-14 UI-REFACTOR-V1 强制 Light/Dark 主题 QA

- RenderHarness 新增 `RunThemeQa`：对 7 个工作区 × 1040×700 / 1100×720 / 1366×768 / 2560×1440 × Light/Dark 强制主题渲染，共 56 个离屏场景。
- 通过 `AdaptiveThemePaletteFactory.Create` 与 `ApplyRuntimeThemeResources` 复用生产主题桥接；`InternalsVisibleTo` 仅开放给 RenderHarness。
- 门禁：56 个场景全部 OK；调色板断言（Light 主文字黑、Dark 主文字白）与主表/页面滚动面视口检查通过；像素采样 Light 背景约 `239,240,243`、Dark 约 `54,57,67`。
- `render-qa` 现包含 10 档尺寸矩阵 + 主题 QA；Release 0 警告/0 错误，Core `59/59`、Worker `190/190`、Playnite `247/247`。
- 真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`；本轮没有 REMOVE，GamePicker 与 Dashboard 锁定区域未改。

## 2026-08-14 UI-REFACTOR-V1 扩档验证（2K / 1100×720 / DPI 等效）

- `render-qa` 窗口矩阵从 5 档扩到 10 档：1040×700、1100×720、1280×720、1366×768、1536×864（1080p @125%）、1600×900、1707×960（1440p @150%）、1920×1080、2048×1152（1440p @125%）、2560×1440；10 档全部通过。
- UI Audit 新增 `2k`（2560×1440）与 `narrow-1100`（1100×720）尺寸，快照 115→161；修复 Audit 向工作区 `ApplyResponsiveLayout` 传内容高度而非窗口高度的夹具偏差，与生产 Dashboard 和 render-qa 对齐。
- 扩档后最终审计：HIGH `0`、MEDIUM `0`、运行时警告 39、失败路由 `0`；2560×1440 主表视口与页面滚动正常，1100×720 维护设备/进程表按窗口高度预算为 360 DIP，而不是错误的 240 DIP。
- 真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`。

## 2026-08-14 UI-REFACTOR-V1 Phase 8 收口（Overview 92 DIP 清零）

- 定位到 After Audit 剩余 MEDIUM 的实际对象是“当前游戏”卡片内 3 个操作按钮的未命名 `WrapPanel`（按钮 38 DIP + 底部 8 DIP，换行两行后总高 92 DIP），不是顶部 `OverviewHomeToolbar` Border。
- `OverviewView.xaml` 将该行 3 个按钮底部边距从 8 收到 4 DIP（`RESTYLE`，命令/Binding/可达性不变）；同时保留顶部工作台工具栏 `Padding="14,10"` 收口，render-qa 实测工具栏 1040×700 91→79 DIP、1280×720 75→63 DIP。
- 最终 UI Audit：HIGH `0`、MEDIUM `0`、运行时警告 42→40、失败路由 `0`；render-qa 五种窗口全绿；Core `59/59`、Worker `190/190`、Playnite `247/247`；source/XAML/WPF 门禁 0 errors。
- 真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`；本轮没有 REMOVE，GamePicker 与 Dashboard 锁定区域未改。

## 2026-08-14 UI-REFACTOR-V1 Phase 8 最终回归

- 最终 Release 隔离构建 0 警告/0 错误；Core `59/59`、Worker `190/190`、Playnite `247/247`；source/XAML/WPF/render-qa 全绿。
- 重跑 `scripts/capture-ui-audit.ps1`：HIGH 从 10 项清零，MEDIUM 从 4 项降到 2 项（Overview 工具栏高度 92 DIP），运行时警告 62 → 42，失败路由 0。
- 最后补修 Maintenance 设备/进程 narrow 表最小视口到 280 DIP，使 Audit HIGH 归零。
- `UI_REFACTOR_FIDELITY_PLAN.md` 已回填 Phase 0～8 状态与 After Audit。
- 真实 Playnite 宿主主题/DPI/连续缩放仍为 `MANUAL QA REQUIRED`；离屏 Audit 与 render-qa 不代表真实宿主人工验收。

## 2026-08-14 UI-REFACTOR-V1 Phase 7 设置轻量统一

- `DesignTokens.xaml` 新增共享 `GscSettingsFieldColumnWidth`（150 DIP），Settings 常规与目录页 8 个字段行全部改为引用该 token，消除页面级硬编码列宽。
- 未改设置分区、字段、绑定、保存语义、响应式断点或主题桥接；五个 Tab 与所有业务设置保持原状。
- 新增资源字典回归断言：`GscSettingsFieldColumnWidth` 必须可解析为 `GridLength`。
- Gate：Release 0 警告/0 错误，Core `59/59`、Worker `190/190`、Playnite `247/247`，source/XAML/WPF/render-qa 全绿。

## 2026-08-14 UI-REFACTOR-V1 Phase 6 维护中心 Maintenance

- 诊断操作卡主行收敛为 5 个常用按钮（复制/导出诊断、复制/导出健康报告、完整性自检）；目录与日志、完整性/自愈/协调、元数据灾备进入 3 个共享 Expander，安全模式、批量路径迁移、重建/协调等 16 个命令全部保留。
- 异常与审计页的审计日志表视口从 96/140 提升到 236/280 DIP；render-qa 实测 1040×700 审计日志表 280 DIP（6 行），不再只有 1.6～1.9 行。
- FindingsGrid、设备表、进程映射表保持 350 DIP 主视口与内部滚动；保留策略四块业务与设备/进程全部命令未改；无 REMOVE。
- 更新两条旧回归断言以匹配新结构：Inspector 内不允许 Expander；诊断主行 5 个按钮 + 卡片共 16 个命令。
- Gate：Release 0 警告/0 错误，Core `59/59`、Worker `190/190`、Playnite `247/247`，source/XAML/WPF/render-qa 全绿。

## 2026-08-14 UI-REFACTOR-V1 Phase 5 任务中心 Task

- 紧凑面板下“游戏筛选”响应式移入“更多筛选”Expander（宽屏自动回到主筛选行），搜索/状态/类型筛选保持主行；四筛选全部可达。
- 任务详情 Inspector 在 Compact/Narrow 默认收起为“查看任务详情 / 收起任务详情”按钮；复制详情、安全重试、取消任务仍可展开访问。
- 操作行不再在 1040×700 竖向膨胀：仅 <520 DIP 才竖向堆叠；常见窄窗保持横向。
- 任务摘要卡在 520–899 DIP 使用 4 列紧凑单行，任务表 1040×700 视口 252 DIP、1280×720 259 DIP；页面 extent 从约 867 降到 536（1040）。
- 未改 ViewModel、Worker、IPC、任务筛选语义、DataGrid 六列/Item ScrollUnit/虚拟化；无 REMOVE。
- 新增 2 条回归测试：Task 窄窗 Inspector 默认收起/可打开；游戏筛选在 compact 进入更多筛选、wide 回到主行。
- Gate：Release 0 警告/0 错误，Core `59/59`、Worker `190/190`、Playnite `247/247`，source/XAML/WPF/render-qa 全绿。

## 2026-08-14 UI-REFACTOR-V1 Phase 4b 媒体中心 Media

- 当前游戏媒体页在 Compact/Narrow 下默认收起媒体 Inspector，改为表内“查看媒体详情 / 收起媒体详情”按钮；预览、收藏、备注、保存、打开目录、重新归类与批量操作仍可通过按钮展开访问。
- 来源规则页“添加截图或录像来源”表单默认折叠到共享 Expander；目录、文件模式、共享开关与添加来源命令仍可展开访问，规则列表继续获得 Auto/* 主行。
- 待归类 DataGrid、媒体虚拟化/Recycling、缩略图异步加载、搜索/筛选绑定与全部命令未改。
- render-qa 实测：1040×700 当前媒体列表保持 350 DIP，Inspector 收起后页面 extent 从约 891 降至 489；1280×720 同样保持 360 DIP。
- 新增 2 条回归测试：Media 窄窗 Inspector 默认收起/可打开、来源表单折叠但字段可达。
- Gate：Release 0 警告/0 错误，Core `59/59`、Worker `190/190`、Playnite `245/245`，source/XAML/WPF/render-qa 全绿。

## 2026-08-14 UI-REFACTOR-V1 Phase 4a 修改器中心 Trainer

- 已绑定工具页在 Compact/Narrow 下默认收起工具设置 Inspector，改为表内“查看工具详情 / 收起工具详情”按钮；启动、保存、打开目录、重新定位、解除绑定与全部编辑字段仍可通过按钮展开访问。
- `TrainerToolsList` 增加命名与选择事件，代码补防重入协调；虚拟化、Recycling、拖拽导入、FLiNG 搜索、可下载版本与导入确认流程未改。
- render-qa 实测：1040×700 工具列表视口 236 DIP、1280×720 276 DIP（修复首轮 234 DIP 门禁失败后达标）。
- 新增 1 条回归测试：窄窗默认收起工具 Inspector、详情按钮可打开/收起。
- Gate：Release 0 警告/0 错误，Core `59/59`、Worker `190/190`、Playnite `243/243`，source/XAML/WPF/render-qa 全绿。

## 2026-08-14 UI-REFACTOR-V1 Phase 3 存档中心 Save Center

- 历史版本与路径候选在 Compact/Narrow 下默认收起 Inspector，改为表内“查看版本详情 / 查看候选详情”按钮；点击后展开同一套 Inspector（备注/锁定/恢复/比较/恢复就绪与接受/忽略/重新扫描），功能零损失。
- 路径与校验页头部从两行压成单行：当前规则/校验状态在左，扫描/校验/刷新在右，为主表让出高度。
- 备份策略页模板区放入共享 `GscExpander`（默认折叠），模板新建/保存/应用/删除命令与全部参数仍可展开访问。
- 比较与保留继续保留并排/堆叠双区，`CompareBackup`、`PreviewRetention` 与所有差异/保留信息未改。
- 未改 GamePicker、ViewModel、Worker、IPC、DataGrid 列/虚拟化/稳定滚动策略；无 REMOVE。
- render-qa 实测：1040×700 历史表高度 385 DIP（原 236）、候选表 254 DIP（原 236）；1280×720 历史 405 DIP、候选 274 DIP；Inspector 收起时主表内部滚动保持 Auto + Item ScrollUnit。
- 新增 1 条回归测试：窄窗默认收起 Inspector、详情按钮可打开/收起；Gate：Release 0 警告/0 错误，Core `59/59`、Worker `190/190`、Playnite `242/242`，source/XAML/WPF/render-qa 全绿。

## 2026-08-14 UI-REFACTOR-V1 Phase 2 首页 Overview

- 六项 Snapshot 指标由 3×2 等权大卡改为响应式紧凑 Summary Strip：宽工作区 6 列、中等 3 列、窄 2 列；卡内 Padding/圆角/数字字号收紧，六个真实计数、比例条与说明全部保留。
- 最近 30 天保护明细放入共享 `GscExpander`（默认折叠），汇总、未识别/需处理胶囊、查看与启用所选保护命令保持常驻；`RecentProtection.Items` 与选择语义不变。
- 全局活动时间线从 3 列 Auto 挤压改为稳定 4 列：图标 / 内容（MinWidth 220）/ 分类 / 结果 / 时间分离；`KindDisplay / ResultDisplay / CreatedDisplay` 全部保留。
- 未改 Dashboard GamePicker、首页“今日工作台 / TODAY / 当前游戏”结构、任何 ViewModel/命令/Binding、DataGrid/ListBox 虚拟化策略；无 REMOVE。
- 新增 3 条回归测试：摘要条响应式列数、保护明细可折叠且项不丢失、全局活动稳定四列。
- Gate：隔离 Release 构建 0 警告/0 错误；Core `59/59`、Worker `190/190`、Playnite `241/241`；`validate-source.py`、WPF UI validator 0 errors、五种窗口 `render-qa OK`；`git diff --check` 通过。

## 2026-08-14 UI-REFACTOR-V1 Phase 1 共享布局基础

- 在 `Themes/Redesign.xaml` 新增共享基础件：`GscInternalTabControl` / `GscInternalTabItem`（内部二级 Tab，供保留策略、异常审计、比较保留使用）、`GscToolbarActionRow`（工具栏动作行）、`GscToolbarOverflowButton`（溢出/更多按钮）。
- 在 `WpfUiResourceDictionaryTests` 增加四个新资源 key 的解析断言，确保后续页面使用这些共享样式时不会静默回退。
- 未修改任何 View 页面、GamePicker、ViewModel、Worker、IPC 或业务逻辑；本轮只建立后续 Phase 2～8 可复用的共享样式。
- Gate：隔离 Release 构建 0 警告/0 错误；Core `59/59`、Worker `190/190`、Playnite `238/238`；`validate-source.py`、WPF UI validator 0 errors、五种窗口 `render-qa OK`；`git diff --check` 通过。

## 2026-08-14 UI-REFACTOR-V1 Phase 0 基线、保真计划与 Gate

- 读取 `GameSaveCenter_UI_Refactor_Implementation_Pack_v1` 全部正文、UI Audit（commit `4ab44fe`）与 WPF Demo v6.1；确认事实来源优先级：生产 main > UI Audit > 实施包 > Demo > 旧布局。
- 生成 `docs/ai/UI_REFACTOR_FIDELITY_PLAN.md`：锁定 GamePicker、一级导航、首页“今日工作台 / TODAY / 当前游戏”、修改器导入流程、Settings 分区与 dev-probe；覆盖 92 条命令、43 个 DataGrid 列、30 个 ScrollViewer、143 个条件 UI 的保真策略。
- Phase 0 Gate：隔离 Release 构建 0 警告 / 0 错误；Core `59/59`、Worker `190/190`、Playnite `238/238`；`validate-source.py` 通过；WPF UI validator 0 errors（17 warnings 为现有 StackPanel/滚动上下文提示）；五种窗口 `render-qa OK`；`git diff --check` 通过。
- 后续 Phase 1～8 按计划逐阶段独立提交并 push，不跨阶段混改；出现 DataGrid 滚动/虚拟化回归先停后续阶段。

## 2026-08-14 UI-AUDIT-001 开发专用 UI 自动审计与整页滚动快照

- 新增 `scripts/capture-ui-audit.ps1` 与根目录 `GameSaveCenter-UI-Audit.cmd`，一条命令产出 `artifacts/ui-audit/` 与 `artifacts/GameSaveCenter-ui-audit.zip`。
- 静态盘点自动扫描 `Views/Settings` 下真实 XAML，生成 `UI_ROUTE_MAP.md/json`、`UI_MANIFEST.md/json`、`UI_FIDELITY_MATRIX.md`；Dashboard、Workspace、设置页、未来新增 UserControl/Tab 都会被自动发现。
- 运行时审计复用 `GameSaveCenter.RenderHarness` 的生产视图与假数据，输出每页每尺寸的 `visual-tree/*.json`、`layout/*.json`、`LAYOUT_REPORT.md`、`AUDIT_SUMMARY.md`。
- 整页滚动截图覆盖所有有滚动条的区域：页面级 `-full-*.png` 直接渲染完整内容，DataGrid/ListBox 按像素换算逐段滚动拼接成 `-scroll-*.png`，满足“从头到底都能截进去”的要求。
- 覆盖 `maximized`（实际 WorkArea）、`wide`、`standard`、`compact`、`narrow` 五种逻辑窗口；截图与文本报告统一做用户目录脱敏，并明确提示位图内容需自行复核。
- 验证：Core `59/59`、Worker `190/190`、Playnite `238/238`；`validate-source.py`、WPF UI validator 0 errors、`render-qa` 全绿；最终审计 `0` 失败路由、`0` 零字节 PNG/JSON；ZIP 约 16.2 MiB。
- Commit：见当前 `git log -1`。

## 2026-08-14 文档：README 与旧文档版本号同步到 0.6.70

- README 当前版本显式标注 `0.6.70-development-preview`（extension.yaml / DLL `0.6.70.0`）。
- 同步 `docs/design/APPLE_UI_GUIDE.md`、`docs/KNOWN_ISSUES.md`、`docs/CODEX_FULL_HANDOFF.md` 中仍停留在 0.6.22/0.6.46/0.6.55 的版本号。

## 2026-08-14 崩溃修复与 Metadata 原子回滚

- 真实 Playnite 09:58:04 崩溃根因：`GscWorkspaceStatePresenter` 模板中重试按钮使用普通 `Button`，却应用 `GscWpfUiActionButton`（TargetType 为 `ui:Button`），切换存档页渲染状态控件时抛 `XamlParseException: “Button”TargetType 与元素“Button”的类型不匹配`。已改为 `ui:Button` 并增加源码回归断言。
- `MetadataRestoreCoordinator`：Playnite 侧在 Worker 恢复前用 detached `GameSaveCenterSettings.ValidatePortableJson` 预校验插件设置；恢复后导入/保存/应用任一失败时，先恢复旧插件设置，再调用 `metadata.restore.rollback` 回滚 DB 与 Worker 设置，失败才进入 `MANUAL_INTERVENTION_REQUIRED`。
- Worker `MetadataBackupService`：恢复前副本改为 `VACUUM INTO` 一致性快照（不再直接复制可能缺 WAL 的活库）；恢复/回滚后调用 `WorkerOptions.ReloadPersistedSettings()`，避免旧内存选项覆盖刚恢复的 worker-settings.json。
- 故障注入覆盖：Worker 测试验证 B 恢复成功后调用 rollback 可回到 A（DB + Worker settings）；Playnite 协调器测试验证 Plugin 保存故意失败后旧 Plugin settings 被恢复、Worker rollback 被调用、失败时进入人工介入。
- 最终 Gate：Release 0 warnings / 0 errors；Core `59/59`、Worker `190/190`、Playnite `235/235`；source/XAML/WPF 静态门禁、五种窗口 `render-qa`、fault-injection 与 1000 轮 soak 全绿。
- 真实宿主验证：`dev-install-run.ps1` 后 `playnite.log` 10:18:34 记录插件加载，`extensions.log` 10:18:36 记录 `GameSaveCenter 0.6.70.0 loaded`，`worker-launch.log` 10:18:40 记录 `Application started`；10:18 后无新崩溃日志。
- Commit：`a417b7c`（崩溃修复）、`13f21a5`（Metadata 原子回滚）。

## 2026-08-14 Final Code Gap Closure 四项缺口闭合

- P0 `REPOSITORY-REBUILD-001` 升级：`RepositoryRebuildService` 改为从磁盘 ZIP/Manifest 扫描，空/新 SQLite 也能按 Ludusavi 目录重建 `recovered-*` 占位游戏与备份历史；不依赖原 Game Match，不猜 Parent，保留 Locked/PreRestore 旧索引，二次重建幂等。
- P1 `METADATA-BACKUP-001` 升级：灾备包新增 `settings/plugin-settings.json`（复用 `ExportPortableJson/ImportPortableJson`），预览返回插件设置哈希与 JSON，恢复后由 Playnite 侧导入、保存并同步 Worker；导入失败时回滚恢复前插件设置。
- P1/P2 `UI-STATES-001` 覆盖扩展：存档历史 Loading、修改器工具 Loading/Empty、媒体 Worker Offline、维护云端 Degraded 均接入共享 `WorkspaceStatePresenter`；状态来自真实快照与集合，不模拟业务成功。
- P1 `LOCAL-MIRROR-001` 升级：Local Mirror 从“同大小即跳过”改为 SHA256 内容校验；已存在文件同大小但内容不同会重新复制并在复制后再次校验。
- 最终 Gate：Release 0 warnings / 0 errors；Core `59/59`、Worker `190/190`、Playnite `235/235`；`validate-source.py`、XAML/WPF 静态门禁与五种窗口 `render-qa` 全绿；`fault-injection-test.ps1` 与 `soak-test.ps1 -Iterations 1000` 通过。
- 真实宿主验证：`dev-install-run.ps1 -Configuration Release` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 09:43:56 记录插件加载，`extensions.log` 09:43:58 记录 `GameSaveCenter 0.6.70.0 loaded`，`worker-launch.log` 09:44:03 记录 `Application started`。
- Commit：`e58714c`、`0bcdce2`、`84f74fc`、`5a12d1e`。
- 下一项：按 `docs/ai/FINAL_MANUAL_QA_CHECKLIST.md` 执行真实场景人工验收；整体 Epic 仍为 `PARTIALLY COMPLETED / MANUAL QA REQUIRED`。

## 2026-08-14 Layer C 与最终 Epic 审计收口

- 已生成 `docs/ai/PRODUCT_HARDENING_LAYER_C_AUDIT.md`：11 项逐项列出生产代码、测试、AUTO VERIFIED、MANUAL QA REQUIRED 与 Commit；结论为 Layer C 11/11 交付。
- 已生成 `docs/ai/PRODUCT_HARDENING_EPIC_FINAL_AUDIT.md`：Layer A 14 项、A-HARDEN-001/002/003、Layer B 13 项、Layer C 11 项逐项表格，包含状态、代码/测试/CI 证据、人工验收和 Commit。
- 最终 Gate：Release 0 warnings / 0 errors；Core `59/59`、Worker `183/183`、Playnite `231/231`；`validate-source.py`、XAML/WPF 静态门禁与五种窗口 `render-qa` 全绿。
- 独立复核：`scripts/fault-injection-test.ps1 -Configuration Release` 通过；`scripts/soak-test.ps1 -Configuration Release -Iterations 1000` 通过。
- 真实宿主验证：`dev-install-run.ps1 -Configuration Release` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 04:02:36 记录插件加载，`extensions.log` 04:02:37 记录 `GameSaveCenter 0.6.70.0 loaded`，`worker-launch.log` 04:02:42 记录 `Application started`。
- 整体状态：`PARTIALLY COMPLETED / MANUAL QA REQUIRED`；真实 Restore/Undo、Rclone、双设备、外置镜像、启动项、反作弊、主题/DPI/连续缩放与 1000+ 游戏库仍未人工验收。
- Commit：`039bc68`（Layer C/Epic 审计）；故障注入与 Soak 独立复核补记随后续文档提交。
- 下一项：等待真实场景人工验收；不再主动扩张新功能。

## 2026-08-14 MAINTENANCE-REPORT-001 用户可读维护健康报告

- Task ID：`MAINTENANCE-REPORT-001`。
- 实现内容：新增 IPC `maintenance.report.get` 与 Worker `MaintenanceReportService`，从 SQLite 计数、完整性自检、存储分析、本地镜像状态聚合用户可读健康文本；维护中心“诊断”操作带新增“复制健康报告/导出健康报告”，导出支持 TXT/Markdown。报告与开发者诊断 ZIP 明确区分，不包含日志、原始数据库或凭据。
- 主要修改文件：`MaintenanceReportDtos.cs`、`MaintenanceReportService.cs`、`MessageTypes.cs`、`WorkerDtos.cs`、`IpcRequestDispatcher.cs`、`Program.cs`、`DashboardViewModel.cs`、`MaintenanceView.xaml`、`MaintenanceReportServiceTests.cs`、`MaintenanceReportSourceTests.cs`、`ProtocolCompatibilityTests.cs`、`WpfUiResourceDictionaryTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `59/59`、Worker `183/183`、Playnite `231/231`；`validate-source.py`、XAML/WPF 静态门禁与五种窗口 `render-qa` 全绿。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 03:43:26 记录插件加载，`worker-launch.log` 03:43:32 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实维护中心点击复制/导出健康报告、长文本显示与内容完整性复核。
- Commit：`9d465d2`。
- 下一项：生成 Layer C Audit，随后生成最终 Epic Audit。

## 2026-08-14 SETTINGS-VALIDATION-001 设置即时验证摘要

- Task ID：`SETTINGS-VALIDATION-001`。
- 实现内容：设置页标题区新增 `SettingsValidationSummary`，在文本框、下拉框与复选框变化时调用既有 `VerifySettings` 并内联展示最多 4 条错误；错误清除后自动隐藏，补充 `AutomationProperties.Name="设置验证错误"`。验证结果仍然与 Playnite 保存流程共用同一套规则。
- 主要修改文件：`GameSaveCenterSettingsView.xaml`、`GameSaveCenterSettingsView.xaml.cs`、`SettingsValidationSourceTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `59/59`、Worker `182/182`、Playnite `230/230`；`validate-source.py`、XAML/WPF 静态门禁与五种窗口 `render-qa` 全绿。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 03:43:26 记录插件加载，`worker-launch.log` 03:43:32 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实修改路径/数值后立即查看内联错误、修正后消失，以及保存时仍能阻止严重错误。
- Commit：`4614bb8`。
- 下一项：Layer C `MAINTENANCE-REPORT-001` 用户可读健康报告。

## 2026-08-14 UI-STATES-001 统一页面状态控件

- Task ID：`UI-STATES-001`。
- 实现内容：新增共享 `WorkspaceStatePresenter`，统一 Loading/Empty/Error/Degraded/Offline/Disabled 六种状态；共享模板展示状态图标、状态文案、标题、说明和可选重试按钮（无命令时自动隐藏）。Overview 全局活动与 Task 空状态已接入共享控件，页面空状态回归测试同时接受 `GscEmptyStateText` 与 `GscWorkspaceStatePresenter`。
- 主要修改文件：`WorkspaceStatePresenter.cs`、`Redesign.xaml`、`OverviewView.xaml`、`TaskCenterView.xaml`、`WorkspaceStateSourceTests.cs`、`WpfUiResourceDictionaryTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `59/59`、Worker `182/182`、Playnite `229/229`；`validate-source.py`、XAML/WPF 静态门禁与五种窗口 `render-qa` 全绿。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 03:36:34 记录插件加载，`worker-launch.log` 03:36:40 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实 Loading/Error/Degraded/Offline/Disabled 状态人工切换与重试按钮复核。
- Commit：`f21cba4`。
- 下一项：Layer C `SETTINGS-VALIDATION-001` 设置即时验证。

## 2026-08-14 ACCESSIBILITY-001 键盘快捷搜索、焦点与自动化名称

- Task ID：`ACCESSIBILITY-001`。
- 实现内容：Dashboard 全局 `PreviewKeyDown` 增加 `Ctrl+F`：首页/存档聚焦游戏搜索框，修改器聚焦 FLiNG 搜索框，任务聚焦任务搜索框，媒体聚焦媒体搜索框，维护聚焦进程映射输入框，并自动全选当前内容。任务、媒体、FLiNG 与游戏搜索框补充 `AutomationProperties.Name`；既有共享 `GscSharedFocusVisual` 与 `SystemParameters.HighContrast` 玻璃降级保持生效。
- 主要修改文件：`DashboardView.xaml.cs`、`DashboardView.xaml`、`TaskCenterView.xaml`、`MediaCenterView.xaml`、`TrainerCenterView.xaml`、`AccessibilitySourceTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `59/59`、Worker `182/182`、Playnite `227/227`；`validate-source.py`、XAML/WPF 静态门禁与五种窗口 `render-qa` 全绿。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 03:25:12 记录插件加载，`worker-launch.log` 03:25:18 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实 Playnite 中键盘-only 使用 `Ctrl+F`、Tab/方向键、Esc，以及 Narrator/Accessibility Insights、高对比度与 200% DPI 复核。
- Commit：`c907983`。
- 下一项：Layer C `UI-STATES-001` 统一 Loading/Empty/Error/Degraded/Offline/Disabled 页面状态。

## 2026-08-14 UI-STATE-001 合理 UI 状态持久化

- Task ID：`UI-STATE-001`。
- 实现内容：设置新增 `LastWorkspace`、任务状态/游戏/类型筛选、任务搜索、媒体筛选与媒体搜索持久化字段；DashboardViewModel 启动时恢复上次 Workspace 与筛选，变更通过新增的 `DebouncedRefresh.Schedule()` 500ms 防抖保存。运行中游戏优先与上次选择恢复继续使用既有 GamePicker 持久化；不保存 Loading/Busy/Error 等瞬态。
- 主要修改文件：`GameSaveCenterSettings.cs`、`DashboardViewModel.cs`、`DebouncedRefresh.cs`、`PortableSettingsTests.cs`、`UiStatePersistenceSourceTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `59/59`、Worker `182/182`、Playnite `225/225`；`validate-source.py` 与源码门禁通过（本阶段无 XAML 改动，不重跑 render-qa）。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 03:16:07 记录插件加载，`worker-launch.log` 03:16:13 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实切换 Workspace、修改任务/媒体筛选与搜索后重启 Playnite，核对恢复结果；运行中游戏优先语义人工复核。
- Commit：`e4513cb`。
- 下一项：Layer C `ACCESSIBILITY-001` 键盘、高对比度与焦点统一。

## 2026-08-14 DRAGDROP-001 修改器中心拖拽导入

- Task ID：`DRAGDROP-001`。
- 实现内容：修改器中心“已绑定工具”页支持从资源管理器拖入单个文件或目录；`.ct` 自动进入 CheatTable 导入，`.lnk/.bat/.cmd/.ps1` 自动按自定义启动项（外部引用不复制），`.exe` 弹出“修改器 / 普通启动项”二选一，`.zip` 与目录复用既有主程序选择流程；未选择游戏时拒绝导入并提示。导入后 AutoStart 仍保持默认关闭。
- 主要修改文件：`DashboardViewModel.cs`、`TrainerCenterView.xaml`、`TrainerCenterView.xaml.cs`、`DragDropImportSourceTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `59/59`、Worker `182/182`、Playnite `224/224`；`validate-source.py`、XAML/WPF 静态门禁与五种窗口 `render-qa` 全绿。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 03:06:22 记录插件加载，`worker-launch.log` 03:06:28 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实拖入 EXE/CT/LNK/BAT/CMD/PS1/ZIP/目录，并人工确认 EXE 二选一、多 EXE 选择和 AutoStart 默认关闭。
- Commit：`5d113c3`。
- 下一项：Layer C `UI-STATE-001` 合理 UI 状态持久化。

## 2026-08-14 PLAYNITE-QUICK-001 Playnite 游戏右键快捷操作

- Task ID：`PLAYNITE-QUICK-001`。
- 实现内容：`GameSaveCenterPlugin.GetGameMenuItems` 为游戏右键菜单提供“立即备份 / 查看备份历史 / 验证最新恢复点 / 游戏工具 / 打开设置”五个快捷操作，全部使用当前所选游戏 ID，并复用 Worker 的 `backup.game`、`backup.list`、`restore.readiness.validate`、`tools.list` 生产 IPC 链路；菜单统一分组到 `GameSaveCenter`。
- 主要修改文件：`GameSaveCenterPlugin.cs`、`QuickActionSourceTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `59/59`、Worker `182/182`、Playnite `222/222`；`validate-source.py` 与源码门禁通过（本阶段无 XAML 改动，不重跑 render-qa）。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 02:50:33 记录插件加载，`worker-launch.log` 02:50:39 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实 Playnite 游戏库中右键逐项点击五个快捷操作，核对备份提交、历史/恢复点/工具摘要与设置页打开。
- Commit：`15ab1d7`。
- 下一项：Layer C `DRAGDROP-001` 修改器中心拖拽导入。

## 2026-08-14 ACTIVITY-001 首页全局活动时间线

- Task ID：`ACTIVITY-001`。
- 实现内容：首页新增“全局活动”卡片，通过 `ActivityTimelineMapper` 把最近 100 条审计记录映射为备份/恢复/云端/媒体/游戏工具/健康/冲突/完整性/仓库修复等业务事件；每条只保留时间、游戏、分类、结果与摘要，不暴露原始日志或堆栈。Dashboard 快照新增 `RecentActivities`，UI 最多显示 12 条，使用有限 240 DIP 视口、虚拟化与 Recycling。
- 主要修改文件：`DashboardDtos.cs`、`ActivityTimelineMapper.cs`、`DashboardService.cs`、`SnapshotComparers.cs`、`DashboardViewModel.cs`、`OverviewView.xaml`、`ActivityTimelineMapperTests.cs`、`UiLayoutRegressionTests.cs`、`FakeDashboardData.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `59/59`、Worker `182/182`、Playnite `221/221`；`validate-source.py`、XAML/WPF 静态门禁与五种窗口 `render-qa` 全绿。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 02:41:38 记录插件加载，`worker-launch.log` 02:41:44 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实备份/恢复/云端/媒体/工具操作后人工核对时间线分类、结果与摘要可读性。
- Commit：`7f38b15`。
- 下一项：Layer C `PLAYNITE-QUICK-001` Playnite 游戏右键快捷操作。

## 2026-08-14 LOCAL-MIRROR-001 第二本地镜像复制与校验

- Task ID：`LOCAL-MIRROR-001`。
- 实现内容：设置页新增“启用第二本地镜像”与镜像目录；维护中心“保留策略”页新增镜像状态卡与“刷新状态/同步镜像”入口。Worker `LocalMirrorService` 从存档目录按相对路径复制到镜像并校验文件大小，跳过大小相同的现有文件，从不删除镜像中多余文件；外置硬盘未连接时状态为 `Unavailable` 而不是系统错误，同步完成写入 `.gsc-mirror-sync.json` 标记。
- 主要修改文件：`LocalMirrorDtos.cs`、`LocalMirrorService.cs`、`WorkerOptions.cs`、`OperationDtos.cs`、`MessageTypes.cs`、`IpcRequestDispatcher.cs`、`GameSaveCenterSettings.cs`、`GameSaveCenterSettingsView.xaml`、`DashboardViewModel.cs`、`MaintenanceView.xaml`、`MaintenanceView.xaml.cs`、`LocalMirrorServiceTests.cs`、`PortableSettingsTests.cs`、`UiLayoutRegressionTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `55/55`、Worker `182/182`、Playnite `220/220`；`validate-source.py`、XAML/WPF 静态门禁与五种窗口 `render-qa` 全绿。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 02:24:22 记录插件加载，`worker-launch.log` 02:24:28 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实外置硬盘插入/拔出、镜像同步后人工核对文件数量与大小，以及主仓库删除后镜像历史不被删除。
- Commit：`387149f`。
- 下一项：Layer C `ACTIVITY-001` 全局活动时间线。

## 2026-08-14 RETENTION-SIM-001 全局保留策略模拟器与安全清理

- Task ID：`RETENTION-SIM-001`。
- 实现内容：维护中心“保留策略”页新增全局保留策略模拟器：按每游戏持久化策略复用 `RetentionPlanner`，汇总现有版本、建议保留、候选清理、预计释放，以及用户锁定/健康保护/PreRestore 计数；展示最多 200 条候选明细。`retention.simulation.apply` 要求二次确认，执行时重新计算并逐条复核，只删除备份根目录下的 ZIP 候选并同步移除 SQLite 索引；锁定、PreRestore、健康恢复点和根目录外/非 ZIP 路径自动跳过，失败明细写入审计。
- 主要修改文件：`RetentionSimulationDtos.cs`、`RetentionSimulationService.cs`、`SqliteStateStore.cs`、`MessageTypes.cs`、`IpcRequestDispatcher.cs`、`DashboardViewModel.cs`、`MaintenanceView.xaml`、`MaintenanceView.xaml.cs`、`RetentionSimulationServiceTests.cs`、`UiLayoutRegressionTests.cs`、`FakeDashboardData.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `55/55`、Worker `178/178`、Playnite `219/219`；`validate-source.py`、XAML/WPF 静态门禁与五种窗口 `render-qa` 全绿。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 02:10:44 记录插件加载，`worker-launch.log` 02:10:50 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实备份仓库下人工预览并确认清理，核对保护版本未被删除、审计记录完整。
- Commit：`a27f430`。
- 下一项：Layer C `LOCAL-MIRROR-001` 第二本地镜像。

## 2026-08-14 STORAGE-001 备份存储分析与增长趋势预估

- Task ID：`STORAGE-001`。
- 实现内容：维护中心“保留策略”页新增只读“备份存储分析”卡：显示备份目录所在卷总容量/剩余空间、目录实测大小、SQLite 索引体积与版本数、近 7/30/90 天新增索引体积、Top 5 游戏占用排行，并按近 30 天增速给出标注“估算”的容量耗尽预测。所有统计在 Worker 侧执行，支持取消，不删除或移动任何文件。
- 主要修改文件：`StorageAnalysisDtos.cs`、`StorageAnalysisService.cs`、`SqliteStateStore.cs`、`MessageTypes.cs`、`IpcRequestDispatcher.cs`、`DashboardViewModel.cs`、`MaintenanceView.xaml`、`MaintenanceView.xaml.cs`、`StorageAnalysisServiceTests.cs`、`UiLayoutRegressionTests.cs`、`FakeDashboardData.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `55/55`、Worker `174/174`、Playnite `218/218`；`validate-source.py`、XAML/WPF 静态门禁与五种窗口 `render-qa` 全绿。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 01:45:42 记录插件加载，`worker-launch.log` 01:45:48 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实备份仓库下人工核对磁盘数字、增长趋势与预测合理性。
- Commit：`2ef8f13`。
- 下一项：Layer C `RETENTION-SIM-001` 全局保留策略模拟器。

## 2026-08-14 FAULT-INJECTION-001 ZIP/数据库故障注入补齐

- Task ID：`FAULT-INJECTION-001`。
- 实现内容：`FaultInjectionHarness` 新增 `corrupted-zip` 与 `corrupted-database` 两类边界故障：损坏 ZIP 必须按 `InvalidDataException` 失败且测试临时归档被清理；损坏 SQLite 文件初始化必须失败且原始数据库文件不得被失败注入删除。总注入故障从 13 类提升到 15 类，仍断言 `Attempted == Recovered`、无意外错误、无残留锁/订阅/临时文件。
- 主要修改文件：`tests/GameSaveCenter.Worker.Tests/Infrastructure/FaultInjectionHarness.cs`、`FaultInjectionTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `55/55`、Worker `170/170`、Playnite `217/217`；`validate-source.py` 与 XAML 结构门禁通过（测试-only 改动，不重跑 render-qa）。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 01:28:48 记录插件加载，`worker-launch.log` 01:28:54 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实磁盘满/权限拒绝/进程强杀/断电时对备份、恢复与元数据操作的人工复核。
- Commit：`738f339`。
- 下一项：生成 Layer B Audit，随后进入 Layer C 11 项。

## 2026-08-14 SOAK-001 数据规模与资源监控 Soak

- Task ID：`SOAK-001`。
- 实现内容：新增 `SoakDataScaleHarness`：默认测试用小规模（200 游戏/2000 备份/1000 任务/3000 媒体/50 工具），设置 `GSC_SOAK_DATA_SCALE=1` 运行完整规模（2000/20000/10000/30000/500）；循环模拟 Dashboard 读取、任务/媒体/工具查询、事件扇出、原子写入和操作锁。
- 监控与断言：记录 Managed Memory、Handle Count、Thread Count、订阅残留、临时文件残留，并做有界增长断言（内存 < 256 MiB 增量、句柄 +256、线程 +32）；失败会记录错误而非只输出日志。
- 主要修改文件：`tests/GameSaveCenter.Worker.Tests/Infrastructure/SoakDataScaleHarness.cs`、`SoakDataScaleTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `55/55`、Worker `170/170`、Playnite `217/217`；`validate-source.py` 与 XAML 结构门禁通过（测试-only 改动，不重跑 render-qa）。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 01:20 记录插件加载，`worker-launch.log` 01:20:05 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实 24/48 小时宿主持续运行、真实大库与长时间挂机后的内存/句柄趋势观察。
- Commit：`c9e053a`。
- 下一项：继续 Layer B，从 `FAULT-INJECTION-001` ZIP/数据库故障注入补齐开始。

## 2026-08-14 ATOMIC-IO-001 原子写入旧文件完整保留审计

- Task ID：`ATOMIC-IO-001`。
- 实现内容：审计确认共享 `AtomicFileWriter` 已覆盖 Worker 设置持久化、媒体复制、元数据恢复替换和启动失败计数；本阶段补充“取消写入时旧文件完整保留”“替换失败时旧文件完整保留且无 `.replace` 残留”两项审计测试。
- 主要修改文件：`AtomicFileWriterTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `55/55`、Worker `169/169`、Playnite `217/217`；`validate-source.py` 与 XAML 结构门禁通过（测试-only 改动，不重跑 render-qa）。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 01:09 记录插件加载，`worker-launch.log` 01:09:28 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实进程在写入中途被终止/断电后确认目标文件不出现截断半成品且临时文件被清理。
- Commit：`eed946c`。
- 下一项：继续 Layer B，从 `SOAK-001` 数据规模与监控补齐开始。

## 2026-08-14 IPC-COMPAT-001 IPC 握手能力列表

- Task ID：`IPC-COMPAT-001`。
- 实现内容：`WorkerHandshakeDto` 增加 `AppVersion` 与 `Capabilities`，握手返回 `RestoreReadiness/MetadataBackup/RepositoryRebuild/PathRemap/TaskReconcile/GameOperationLock/AtomicIo` 能力列表；插件/Worker 仍使用独立 `ProtocolVersion`，不直接用 0.6.70 判断兼容。
- 主要修改文件：`WorkerDtos.cs`、`IpcRequestDispatcher.cs`、`ProtocolCompatibilityTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `55/55`、Worker `167/167`、Playnite `217/217`；`validate-source.py` 与 XAML 结构门禁通过（本阶段无 UI 改动，不重跑 render-qa）。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 01:01 记录插件加载，`worker-launch.log` 01:01:23 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：保留旧版 Worker 后启动新插件，确认握手拒绝并替换旧进程。
- Commit：`7ee738d`。
- 下一项：继续 Layer B，从 `ATOMIC-IO-001` 重要状态文件原子写入审计补齐开始。

## 2026-08-14 GAME-OP-LOCK-001 单游戏操作兼容矩阵

- Task ID：`GAME-OP-LOCK-001`。
- 实现内容：新增 `GameOperationKind`（Backup/Restore/Retention/CloudUpload/CloudDownload/Media/RestoreReadiness/RepositoryRepair）与 `GameOperationPolicy.IsCompatible` 显式兼容矩阵：Restore 与所有操作不兼容，同游戏两个 Backup 不兼容，Retention 与 Backup/Restore 不兼容；Backup+Media、CloudUpload+Media、RestoreReadiness+Backup 标记兼容。
- 锁入口：`GameOperationLock.AcquireAsync` 新增带操作类型的重载，备份、云端重试、媒体同步、恢复预览/执行均已接入类型化调用；底层仍按 GameId 串行化（安全超集），不同游戏可并行。
- 主要修改文件：`GameOperationLock.cs`、`BackupOrchestrator.cs`、`RestoreOrchestrator.cs`、`MediaSyncService.cs`、`GameOperationLockTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `54/54`、Worker `167/167`、Playnite `217/217`；`validate-source.py` 与 XAML 结构门禁通过（本阶段无 UI 改动，不重跑 render-qa）。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 00:52 记录插件加载，`worker-launch.log` 00:52:47 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实长时间备份期间对同一游戏发起恢复/媒体同步，确认 `GAME_OPERATION_BUSY` 且不同游戏仍并行。
- Commit：`7dba09c`。
- 下一项：继续 Layer B，从 `IPC-COMPAT-001` 能力列表补齐开始。

## 2026-08-14 TASK-RECONCILE-001 Worker 中断任务分类协调

- Task ID：`TASK-RECONCILE-001`。
- 实现内容：为每个 Worker 进程生命周期生成 `WorkerSessionId`，`TaskStatusDto` 与 `tasks.worker_session_id` 持久化任务所属 Worker。Worker 启动时只协调“不属于当前 Worker 会话”的遗留 `Queued/Running` 任务，并分类：`Backup/MediaSync/MediaInbox/CloudUpload` 标记 `WORKER_RESTARTED_RETRYABLE`，`Integrity` 标记 `WORKER_RESTARTED`，`Restore` 标记 `MANUAL_INTERVENTION_REQUIRED` 且错误信息提示检查 PreRestore 快照，绝不自动重试恢复。
- UI/语义：任务中心可见“Worker 意外退出，任务中断”的错误信息；手动“协调中断任务”复用同一分类逻辑。
- 主要修改文件：`OperationDtos.cs`、`WorkerOptions.cs`、`SqliteStateStore.cs`、`TaskCoordinator.cs`、`TaskReconcileService.cs`、`WorkerInitializationService.cs`、`TaskEventBroadcaster.cs`、`SnapshotComparers.cs` 及相关 Worker 测试。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `54/54`、Worker `158/158`、Playnite `217/217`；`validate-source.py` 与 XAML 结构门禁通过（本阶段无 XAML 改动，不重跑 render-qa）。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 00:44 记录插件加载，`worker-launch.log` 00:44:18 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实中断恢复任务后人工核对 `MANUAL_INTERVENTION_REQUIRED` 与 PreRestore 快照；以及中断上传/备份后从任务中心重试的真实流程。
- Commit：`47685bd`。
- 下一项：继续 Layer B，从 `GAME-OP-LOCK-001` 单游戏操作兼容矩阵补齐开始。

## 2026-08-14 PATH-REMAP-001 批量路径迁移预览与目标缺失策略

- Task ID：`PATH-REMAP-001`。
- 实现内容：路径迁移增加只读预览 `path.remap.preview`，按“备份归档/媒体归档/媒体原始路径/媒体来源/游戏工具入口/工作目录/解析目标/存档候选”列出受影响旧/新路径并标记目标是否存在；执行前自动调用 `MetadataBackupService.CreateAsync` 生成元数据灾备，再写库。
- 目标缺失策略：`PathRemapRequestDto.ApplyMissingTargets` 默认 `false`；存在缺失目标且未授权应用时返回 `PATH_REMAP_TARGET_MISSING` 并整体跳过（默认安全跳过）；用户确认后可以 `ApplyMissingTargets=true` 仍应用。UI 先预览并显示缺失数量，再二次确认。
- 主要修改文件：`PathRemapService.cs`、`PathRemapDtos.cs`、`SqliteStateStore.PathRemap.cs`、`MessageTypes.cs`、`IpcRequestDispatcher.cs`、`DashboardViewModel.cs`、`PathRemapServiceTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `54/54`、Worker `158/158`、Playnite `217/217`；`validate-source.py` 与 XAML 结构门禁通过（本阶段无 XAML 改动，不重跑 render-qa）。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 00:32 记录插件加载，`worker-launch.log` 00:32:16 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实移动目录后预览并执行迁移，核对数据库路径、Worker 设置与自动生成的元数据灾备包。
- Commit：`bddcfdd`。
- 下一项：继续 Layer B，从 `TASK-RECONCILE-001` Worker 中断任务分类恢复补齐开始。

## 2026-08-14 REPOSITORY-REBUILD-001 备份索引重建预览与确认

- Task ID：`REPOSITORY-REBUILD-001`。
- 实现内容：备份索引重建拆成“只读扫描预览 → 用户确认 → 写库”三个阶段。`repository.rebuild.preview` 扫描存档目录 ZIP，统计已确认归属/未归属/元数据部分缺失/损坏归档并返回摘要；`repository.rebuild` 现在要求 `Confirmed=true`，未确认返回 `REPOSITORY_REBUILD_NOT_CONFIRMED`。
- UI：维护中心“重建备份索引”先调用预览并显示摘要，再弹出确认框，确认后才执行真实重建；失败游戏仍保留原索引，重复重建不会产生额外版本（沿用既有幂等刷新路径）。
- 主要修改文件：`RepositoryRebuildService.cs`、`RepositoryRebuildDtos.cs`、`MessageTypes.cs`、`IpcRequestDispatcher.cs`、`DashboardViewModel.cs`、`RepositoryRebuildServiceTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `54/54`、Worker `157/157`、Playnite `217/217`；`validate-source.py` 与 XAML 结构门禁通过（本阶段无 XAML 改动，不重跑 render-qa）。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 00:23 记录插件加载，`worker-launch.log` 00:23:37 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实 Ludusavi 备份目录下预览后执行重建，并核对版本数量和归档路径。
- Commit：`dff0cf4`。
- 下一项：继续 Layer B，从 `PATH-REMAP-001` 批量路径迁移预览/目标缺失处理补齐开始。

## 2026-08-14 METADATA-BACKUP-001 元数据灾备恢复流程

- Task ID：`METADATA-BACKUP-001`。
- 实现内容：在既有元数据灾备导出基础上补齐安全恢复：`metadata.restore.preview` 校验 ZIP 内的 manifest、数据库/设置 SHA-256、条目路径越界；`metadata.restore.execute` 要求确认，执行前复制当前数据库与 Worker 设置到 `MetadataBackups/PreRestore-*`，临时暂停后台自动操作，使用 `AtomicFileWriter.ReplaceFileAsync` 原子替换，随后执行 SQLite `integrity_check`；失败时回滚恢复前副本，回滚失败返回 `METADATA_RESTORE_ROLLBACK_FAILED`。
- UI：维护中心新增“恢复元数据灾备”按钮，流程为选择 ZIP → 预览 → 危险操作确认 → 执行 → 显示恢复摘要与恢复前副本路径。
- 核心设计选择：恢复包不会被直接解压覆盖；替换前清理 WAL/SHM 侧车文件并确保文件属性可写；`SqliteConnection.ClearAllPools` 用于释放 SQLite 池锁。Worker.Tests 增加 `DisableTestParallelization`，避免全局池清理干扰并行测试。
- 主要修改文件：`MetadataBackupService.cs`、`MetadataBackupDtos.cs`、`MessageTypes.cs`、`IpcRequestDispatcher.cs`、`AtomicFileWriter.cs`、`WorkerOptions.cs`、`DashboardViewModel.cs`、`MaintenanceView.xaml`、`MetadataBackupServiceTests.cs`、`AssemblyInfo.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `54/54`、Worker `155/155`、Playnite `217/217`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 00:15 记录插件加载，`worker-launch.log` 00:15:20 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实选择元数据包执行恢复、核对恢复前副本与恢复后数据库，以及恢复失败回滚的人工复核。
- Commit：`97b06b5`。
- 下一项：继续 Layer B，从 `REPOSITORY-REBUILD-001` 备份仓库索引重建预览/确认补齐开始。

## 2026-08-13 DB-MIGRATION-001 历史数据库 Fixture 补齐

- Task ID：`DB-MIGRATION-001`。
- 实现内容：`DatabaseMigrationHarness` 增加 `ReadScalar`，两代旧库 Fixture 覆盖 `game_policies`、`backup_policy_templates`、`sessions`、`device_conflict_decisions`、备份历史、任务、媒体来源、GameTool 与版本表；第二个 Fixture 模拟缺少 `session_id/archive_path/shared_directory/risk_category/resolved_target_path` 等新列的更老 schema，验证升级补列且数据不丢。
- 数据值断言：不再只断言“表能打开”，而是直接验证冲突决策、策略 JSON、模板名称、会话启动配置、任务类型、锁定状态、媒体来源匹配模式和工具路径等真实值；内置策略模板种子使用 `INSERT OR IGNORE`，旧 `important` 模板保留。
- 主要修改文件：`tests/GameSaveCenter.Worker.Tests/Infrastructure/DatabaseMigrationHarness.cs`、`DatabaseMigrationHarnessTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `54/54`、Worker `152/152`、Playnite `217/217`；`validate-source.py` 与 XAML 结构门禁通过（测试-only 改动，不重跑 render-qa）。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 23:42 记录插件加载，`worker-launch.log` 23:42:43 记录 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实历史用户数据库升级后的功能回归，以及大量历史数据迁移时长的人工复核。
- Commit：`55f532f`。
- 下一项：继续 Layer B，从 `METADATA-BACKUP-001` 元数据灾备恢复流程审计开始。

## 2026-08-13 INTEGRITY-001 完整性自检补齐

- Task ID：`INTEGRITY-001`。
- 实现内容：在既有 SQLite/表结构/目录/已索引文件检查基础上补齐：磁盘上未被数据库索引的孤儿归档（`ORPHAN_ARCHIVE`）、备份 Manifest 无法解析或重复路径（`MANIFEST_INVALID`/`MANIFEST_DUPLICATE_PATH`）、数据/存档/媒体所在卷剩余空间不足（`LOW_DISK_SPACE`）、未配置依赖的状态收敛为 `Warning`（Ludusavi）或 `Skipped`（Rclone 可选）。
- 结果模型：`IntegrityCheckResultDto` 现在使用 `Healthy/Warning/Error/Skipped` 四态并返回 `SkippedCount`；自检仍完全只读，不自动删除或修复。
- 主要修改文件：`IntegrityCheckService.cs`、`SqliteStateStore.Integrity.cs`、`IntegrityDtos.cs`、`IntegrityCheckServiceTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `54/54`、Worker `151/151`、Playnite `217/217`；`validate-source.py` 与 XAML 结构门禁通过（本阶段无 UI 改动，不重跑 render-qa）。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 23:27 记录插件加载，`worker-launch.log` 23:27:07 记录 Worker 启动与 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实磁盘不足、孤儿 ZIP、损坏 Manifest 场景下人工复核自检结果；修复动作仍未实现自动应用，只提供建议。
- Commit：`a064108`。
- 下一项：继续 Layer B，从 `DB-MIGRATION-001` 历史数据库 Fixture 补齐开始。

## 2026-08-13 DIAGNOSTICS-001 一键诊断包结构化升级

- Task ID：`DIAGNOSTICS-001`。
- 实现内容：诊断包从纯文本摘要升级为结构化 JSON：`system.json`、`worker.json`、`dependencies.json`、`database.json`、`recent-tasks.json`、`health.json`、`settings.json`，外加 README、审计与受限日志尾部。新增集中式 `DiagnosticRedactor`，统一脱敏密码/Token/API Key/Authorization/Bearer、URL query secret、UNC 凭据、邮箱和 `C:\Users\<USER>` 用户路径。
- 核心设计选择：所有写入包内的字符串都先经过 `DiagnosticRedactor`；数据库只做只读探针，不打包 SQLite；日志/包大小保持上限；Playnite 侧传入插件版本、Playnite 版本、主题、工作区与 DPI 信息，Worker 补 PID/启动时间/Uptime/协议版本。
- 主要修改文件：`DiagnosticsPackageService.cs`、`DiagnosticRedactor.cs`、`DiagnosticsDtos.cs`、`SqliteStateStore.cs`、`DashboardViewModel.cs`、`DiagnosticsPackageServiceTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `54/54`、Worker `148/148`、Playnite `217/217`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。禁止泄密测试覆盖 `abc123/token123/secret123/JohnDoe/querysecret`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 23:18 记录插件加载，`worker-launch.log` 23:18:24 记录 Worker 启动与 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实 Playnite 中导出诊断包后人工检查 ZIP 内 JSON 字段与脱敏结果。
- Commit：`17e88b6`（与 SAFE-MODE-001 同组提交）。
- 下一项：`SAFE-MODE-001` 启动恢复（同一提交）后继续 `INTEGRITY-001` 补齐。

## 2026-08-13 SAFE-MODE-001 安全模式启动恢复

- Task ID：`SAFE-MODE-001`。
- 实现内容：在既有安全模式门禁基础上补齐启动恢复：`WorkerOptions` 新增持久化 `SafeModeRequested` 与启动失败计数文件；连续 3 次 Worker 初始化失败后自动请求安全模式，Playnite 打开维护中心时用 `ConfirmAsync` 询问“是否使用安全模式打开”，确认后启用并清除请求。设置页新增“下次以安全模式启动”开关，维护中心安全模式提示条新增“恢复正常模式”按钮。
- 核心设计选择：安全模式请求不自动永久启用，必须由用户确认；启动成功只清失败计数、保留待确认请求；手动退出安全模式走真实 `UpdateSettings` 管道并立即刷新诊断。
- 主要修改文件：`WorkerOptions.cs`、`WorkerInitializationService.cs`、`OperationDtos.cs`、`IpcRequestDispatcher.cs`、`GameSaveCenterSettings.cs`、`GameSaveCenterSettingsView.xaml`、`MaintenanceView.xaml`、`DashboardViewModel.cs`、`SafeModeStartupTests.cs`。
- 测试结果：与 DIAGNOSTICS-001 同批：Core `54/54`、Worker `148/148`、Playnite `217/217`；安全模式三次失败请求、成功清计数、请求清除、设置持久化与 UI 绑定均有测试。
- 真实宿主验证：与 DIAGNOSTICS-001 同批通过；插件与 Worker 正常加载。
- MANUAL QA REQUIRED：真实连续启动失败 3 次后人工确认提示与安全模式自动暂停效果；以及“恢复正常模式”按钮的真实行为。
- Commit：`17e88b6`。
- 下一项：继续 Layer B，从 `INTEGRITY-001` 全局完整性自检补齐开始。

## 2026-08-13 A-HARDEN-003 Onboarding 测试备份闭环审计

- Task ID：`A-HARDEN-003`。
- 实现内容：审计确认 ONBOARDING-001 已具备真实“测试备份”入口：维护中心首次使用卡片上的 `OnboardingTestBackupCommand` 直接调用 `BackupSelectedAsync`，而 `BackupSelectedAsync` 走真实 `MessageTypes.BackupGame` 生产备份管道（`Reason="Manual"`、`Force=true`），没有独立的 `TestBackupService` 或假服务。按钮仅在存在已匹配当前游戏且 Ludusavi 可用时可用。
- 本阶段补齐：无可用测试游戏时的明确文案改为“若当前没有可用于测试的已识别存档游戏，可稍后在存档中心手动执行备份。”，并新增回归测试锁定“OnboardingTestBackupCommand → BackupSelectedAsync → MessageTypes.BackupGame/Manual”链路。
- 主要修改文件：`MaintenanceView.xaml`、`tests/GameSaveCenter.Playnite.Tests/SettingsAndAutoSelectSourceTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `54/54`、Worker `145/145`、Playnite `216/216`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 22:58 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 22:58:47 记录 Worker 启动、存储初始化和 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实首次使用流程中点击“对当前游戏做测试备份”，确认任务中心显示真实 Manual Backup 结果和“测试备份成功”反馈。
- Commit：`5666944`。
- 下一项：STEP 7 Layer A Hardening Gate（Release/Core/Worker/Playnite/source/XAML/render-qa 全量复跑），随后生成 Layer B 逐项审计。

## 2026-08-13 A-HARDEN-002 未分类自定义启动项反作弊自动启动语义

- Task ID：`A-HARDEN-002`。
- 实现内容：修正 `GameToolAutoStartPolicy`：`CustomExecutable + Unknown` 在普通游戏下按用户 AutoStart 正常启动，不再因未分类一律暂缓；遇到反作弊游戏时，只有该工具显式持久化授权 `AllowUnknownToolWithAntiCheat` 后才允许。`GeneralUtility` 在反作弊游戏下继续允许，`GameModification`/`Trainer`/`CheatTable` 在反作弊游戏下继续禁止。
- 数据模型：`GameToolDto` 与 `UpdateGameToolRequestDto` 新增 `AllowUnknownToolWithAntiCheat`；`game_tools` 新增 `allow_unknown_anticheat_autostart INTEGER NOT NULL DEFAULT 0`，`EnsureColumnAsync` 与 CREATE TABLE 同步，旧库升级由迁移 Harness 校验。
- UI：TrainerCenter 工具类别区域新增“反作弊授权”开关，仅当自定义启动项且类别为未分类时显示；未分类选项文案从“自动启动将暂缓”改为“未分类（反作弊游戏需授权）”。`SnapshotComparers` 现在同时比较 IfAlreadyRunning/RiskCategory/AllowUnknownToolWithAntiCheat。
- 主要修改文件：`GameToolAutoStartPolicy.cs`、`SqliteStateStore.cs`、`TrainerDtos.cs`、`TrainerCenterView.xaml`、`DashboardViewModel.cs`、`SnapshotComparers.cs`，以及 Worker/Playnite 相关测试。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `54/54`、Worker `145/145`、Playnite `215/215`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 22:53 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 22:53:09 记录 Worker 启动、存储初始化和 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实反作弊游戏与未分类自定义工具组合下的人工复核，确认授权开关持久化且不会每次启动重复询问；普通游戏下未分类工具按 AutoStart 正常启动。
- Commit：`8d6c36c`。
- 下一项：`A-HARDEN-003` Onboarding 测试备份闭环审计。

## 2026-08-13 A-HARDEN-001 通知级别正式实现与回归补齐

- Task ID：`A-HARDEN-001`。
- 实现内容：在既有 `NOTIFY-LEVELS-001` 基础上正式收口通知级别：`NotificationLevel`（`ImportantOnly/Summary/Verbose`）持久化到 Settings，默认 `Summary`；`NotificationLevelPolicy` 纯策略控制会话摘要与任务事件；设置页提供“仅重要通知/退出摘要/详细任务通知”和说明文本。本阶段把 `SessionNotificationAccumulator` 从插件私有嵌套类抽出为 `Infrastructure` 内部类，并给 Playnite.Tests 开放 InternalsVisibleTo，补齐“同一 Session 只发一次最终摘要”“期望任务数未到不提前汇总”“重复投递只替换终态不重复”和“Verbose 子事件 + 最终摘要”等回归测试。
- 核心设计选择：通知状态继续复用 Task Center 的同一批终态任务，不另造成功判断；Summary 对 Session 任务只发一条退出摘要，Verbose 才额外逐任务显示；同一 Session 由 `TryMarkEmitted` 保证不重复 final。
- 主要修改文件：`src/GameSaveCenter.Playnite/Infrastructure/SessionNotificationAccumulator.cs`、`GameSaveCenterPlugin.cs`、`GameSaveCenter.Playnite.csproj`、`tests/GameSaveCenter.Playnite.Tests/SessionNotificationAccumulatorTests.cs`、`tests/GameSaveCenter.Core.Tests/NotificationLevelPolicyTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `54/54`、Worker `141/141`、Playnite `215/215`；`validate-source.py` 与 XAML 结构门禁通过（本阶段未改 XAML，不重跑 render-qa 与 WPF 静态门禁）。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 22:34 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 22:34:39 记录 Worker 启动、存储初始化和 `Application started`；安装后无新增插件异常。
- 验证边界：非任务型重要事件（Health Risk、Backup anomaly、Multi-device conflict、Integrity serious error）目前由维护中心 Findings/Dashboard 承载，尚未单独生成 Playnite toast；真实宿主下三种通知级别的实际观感仍需人工复核。
- Commit：`fbbdb90`。
- 下一项：`A-HARDEN-002` CustomExecutable Unknown AutoStart 语义修正。

## 2026-08-13 FAULT-INJECTION-001 关键 I/O 故障注入

- Task ID：`FAULT-INJECTION-001`。
- 实现内容：新增 `FaultInjectionHarness` 与 `FaultInjectionTests`，只服务自动测试，不在生产代码中加入 `TestMode` 分支。注入 13 类确定性边界故障：原子写源缺失、目标已占用、取消写入、外部进程缺失/超时/取消/非零退出、TaskCoordinator 业务异常/通用异常/取消、订阅释放后发布、单游戏锁争用超时和双重 Dispose。
- 核心设计选择：Harness 使用临时数据目录，故障后断言 `.tmp/.partial` 无残留、任务进入稳定终态、取消令牌被移除、锁可重新获取、订阅计数归零；故障注入不触碰真实存档、媒体、数据库或 Worker 设置。
- 主要修改文件：`tests/GameSaveCenter.Worker.Tests/Infrastructure/FaultInjectionHarness.cs`、`FaultInjectionTests.cs`、`scripts/fault-injection-test.ps1`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `53/53`、Worker `141/141`、Playnite `211/211`；`validate-source.py` 与 XAML 结构门禁通过（本阶段无 UI 改动，不重跑 render-qa 与 WPF 静态门禁）。`scripts/fault-injection-test.ps1` 独立跑通，0 失败。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 22:27 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 22:27:34 记录 Worker 启动、存储初始化和 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实磁盘满/权限移除/外部进程崩溃场景下的人工复核，确认任务中心显示稳定错误码且原数据完整。
- Commit：`f91992e`。
- 下一项：按最终总任务顺序回到 STEP 4，从 `A-HARDEN-001` 正式实现通知级别开始，随后 `A-HARDEN-002/003`、Layer A Hardening Gate，再对已交付的 Layer B/C 逐项审计补齐。

## 2026-08-13 SOAK-001 长时间稳定性测试

- Task ID：`SOAK-001`。
- 实现内容：新增可复用 `SoakStabilityHarness` 与 `SoakStabilityTests`，以加速轮次反复压测 Worker 最容易长期退化/泄漏的表面：`TaskCoordinator` 任务持久化与变更 Feed、`TaskEventBroadcaster` 订阅/发布/释放、`GameOperationLock` 单游戏互斥、`AtomicFileWriter` 原子写与失败清理、SQLite 临时表读写探针和审计写入。新增 `scripts/soak-test.ps1` 长跑入口，可通过 `GSC_SOAK_ITERATIONS` 把默认 100 轮扩展到最多 5000 轮。
- 核心设计选择：Harness 只使用临时数据目录，不触碰真实存档、媒体、数据库或 Worker 设置；为稳定性断言补充 `TaskEventBroadcaster.SubscriberCount` 与 `GameOperationLock.TrackedGameCount` 两个只读计数，不改任何并发行为。变更 Feed 的保留上限仍为 500 条，慢订阅者容量仍为 128 条 Drop-Oldest。
- 主要修改文件：`tests/GameSaveCenter.Worker.Tests/Infrastructure/SoakStabilityHarness.cs`、`SoakStabilityTests.cs`、`TaskEventBroadcasterTests.cs`、`GameOperationLockTests.cs`、`TaskEventBroadcaster.cs`、`GameOperationLock.cs`、`scripts/soak-test.ps1`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `53/53`、Worker `140/140`、Playnite `211/211`；`validate-source.py` 与 XAML 结构门禁通过（本阶段无 UI 改动，不重跑 render-qa 与 WPF 静态门禁）。`scripts/soak-test.ps1 -Iterations 60` 独立跑通，0 失败。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 22:18 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 22:18:53 记录 Worker 启动、存储初始化和 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实 24/48 小时宿主持续运行、内存/句柄趋势、大游戏库与真实备份/媒体任务混跑，以及长时间挂机后任务中心/通知仍可用的观察。
- Commit：`8132419`。
- 下一项：继续 Layer B，从 `FAULT-INJECTION-001` 故障注入开始。

## 2026-08-13 ATOMIC-IO-001 原子写入审计与共享原子写入器

- Task ID：`ATOMIC-IO-001`。
- 实现内容：新增 Worker 共享 `AtomicFileWriter`，统一设置持久化与媒体复制两处“临时文件 + 原子替换”逻辑。`WriteAllTextAsync` 在目标同目录写唯一 `.tmp` 后 `File.Move(..., true)`；`CopyAtomicallyAsync` 在目标同目录写唯一 `.partial` 后 `File.Move(..., false)`。任何失败都会先清理残留临时文件再重新抛出。
- 核心设计选择：临时文件与目标同盘同目录，避免跨卷 `Move` 退化成非原子复制；`WorkerOptions.Persist()` 与 `MediaSyncService` 的私有复制逻辑改为委托共享实现，保证以后所有 Worker 原子写入都走同一套清理与替换契约。
- 主要修改文件：`src/GameSaveCenter.Worker/Infrastructure/AtomicFileWriter.cs`、`WorkerOptions.cs`、`MediaSyncService.cs`，以及 `tests/GameSaveCenter.Worker.Tests/AtomicFileWriterTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `53/53`、Worker `136/136`、Playnite `211/211`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 22:04 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 记录存储初始化与 `Application started`；安装后无新增插件异常，Playnite 与 Worker 均从当前扩展目录运行。
- MANUAL QA REQUIRED：真实进程在写入中途被终止/断电后确认目标文件不出现截断半成品且临时文件被清理；以及高并发读取方在覆盖替换瞬间始终看到完整旧值或完整新值。
- Commit：`f1eda41`。
- 下一项：继续 Layer B，从 `SOAK-001` 长时间稳定性测试开始。

## 2026-08-13 IPC-COMPAT-001 IPC protocol handshake

- Task ID：`IPC-COMPAT-001`。
- 实现内容：新增显式 `system.handshake` IPC 与 `WorkerHandshakeDto`（协议版本、最低支持版本、Worker 版本、UTC 时间）。`WorkerIpcClient.HandshakeAsync` 先校验 `ProtocolCompatibility.IsCompatible`，不兼容时抛出 `PROTOCOL_MISMATCH`；`WorkerLauncher.ProbeHealthAsync` 优先握手，旧 Worker 不支持握手时回退 Ping，版本不匹配仍标记 `Incompatible`。
- 核心设计选择：兼容规则放在 Contracts 纯静态类，客户端/服务端/测试共用；显式握手与既有逐请求协议版本检查共存，避免新插件静默复用旧 Worker。
- 主要修改文件：Contracts `WorkerDtos.cs`/`ProtocolCompatibility.cs`/`MessageTypes`、`IpcRequestDispatcher`、`WorkerIpcClient`、`WorkerLauncher`，以及 Core/Playnite 测试。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `53/53`、Worker `132/132`、Playnite `211/211`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 记录存储初始化与 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：保留旧版 Worker 进程后启动新插件，确认握手拒绝并替换旧进程；以及协议版本变更时的真实兼容验证。
- Commit：`6454027`。
- 下一项：继续 Layer B，从 `ATOMIC-IO-001` 原子写入审计开始。

## 2026-08-13 GAME-OP-LOCK-001 单游戏操作兼容锁

- Task ID：`GAME-OP-LOCK-001`。
- 实现内容：新增 `GameOperationLock` 单游戏信号量锁，并接入备份、云端上传重试、媒体同步、恢复预览与恢复执行。同一游戏的备份/恢复/媒体操作在同一时间只允许一个执行；不同游戏仍可并行。锁获取超时返回 `GAME_OPERATION_BUSY`，不会排队积压或静默覆盖。
- 核心设计选择：锁以 PlayniteId 为粒度、大小写不敏感；恢复执行在进入任务前获取锁，预览也持锁，避免与真实写入交错；不清理字典以避免并发竞态，锁对象数量与游戏数有界。
- 主要修改文件：`GameOperationLock`、`BackupOrchestrator`、`MediaSyncService`、`RestoreOrchestrator`、`Program`，以及 Worker 测试。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `51/51`、Worker `132/132`、Playnite `211/211`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 记录存储初始化与 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实长时间备份期间对同一游戏发起恢复/媒体同步，确认显示 `GAME_OPERATION_BUSY` 且不同游戏仍并行。
- Commit：`dc8a5e6`。
- 下一项：继续 Layer B，从 `IPC-COMPAT-001` IPC protocol handshake 开始。

## 2026-08-13 TASK-RECONCILE-001 Worker 中断任务恢复体系

- Task ID：`TASK-RECONCILE-001`。
- 实现内容：在既有启动自动协调基础上新增 `tasks.reconcile` IPC 与 `TaskReconcileService`。维护中心诊断页新增“协调中断任务”按钮和结果摘要；手动执行会把遗留的 `Queued/Running` 任务幂等标记为 `WORKER_RESTARTED`，保留任务中心和通知统一状态，并写审计。
- 核心设计选择：`MarkInterruptedTasksAsync` 改为返回受影响行数，让手动协调可报告数量；已成功/失败/取消/等待任务不受影响，重复协调不会重复改写。
- 主要修改文件：Contracts `TaskReconcileDtos.cs`/`MessageTypes`、`SqliteStateStore`、`TaskReconcileService`、`Program`、`IpcRequestDispatcher`、`DashboardViewModel`、`MaintenanceView`，以及 Worker/Playnite 测试。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `51/51`、Worker `131/131`、Playnite `211/211`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 记录存储初始化与 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实中断上传/备份后点击协调，并在任务中心核对 `WORKER_RESTARTED` 状态与重试入口。
- Commit：`2724209`。
- 下一项：继续 Layer B，从 `GAME-OP-LOCK-001` 单游戏操作兼容锁开始。

## 2026-08-13 PATH-REMAP-001 批量路径迁移

- Task ID：`PATH-REMAP-001`。
- 实现内容：新增 `path.remap` IPC 与 `PathRemapService`。维护中心诊断页新增“批量路径迁移”卡片：输入旧根路径和新根路径并确认后，Worker 批量改写 SQLite 中 `backup_versions.archive_path`、`media`、`media_sources`、`game_tool_versions`、`save_candidates` 等已索引路径，并同步 Worker 设置的存档/媒体目录。只改路径引用，不移动或删除文件。
- 核心设计选择：SQLite 更新使用“前缀 + 目录边界”匹配，避免 `C:\Saves` 误改 `C:\Saves2`；服务端强制 `Confirmed=true`，Playnite 侧先弹危险操作确认框；失败/空/相同/相对路径均明确拒绝。
- 主要修改文件：Contracts `PathRemapDtos.cs`/`MessageTypes`、`SqliteStateStore.PathRemap.cs`、`PathRemapService`、`Program`、`IpcRequestDispatcher`、`DashboardViewModel`、`MaintenanceView`，以及 Worker/Playnite 测试。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `51/51`、Worker `130/130`、Playnite `211/211`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 记录存储初始化与 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实移动备份/媒体目录后执行迁移，并核对 Worker 重启后路径与磁盘归档仍然一致。
- Commit：`a521281`。
- 下一项：继续 Layer B，从 `TASK-RECONCILE-001` Worker 中断任务恢复体系开始。

## 2026-08-13 REPOSITORY-REBUILD-001 备份仓库索引重建

- Task ID：`REPOSITORY-REBUILD-001`。
- 实现内容：新增 `repository.rebuild` IPC 与 `RepositoryRebuildService`。遍历已匹配游戏，通过既有 `RefreshBackupHistoryAsync` 从 Ludusavi 磁盘实际备份列表重建 SQLite `backup_versions` 索引；单个游戏失败不中断其余游戏，结果汇总成功/失败/索引版本数并写审计。维护中心诊断页新增“重建备份索引”按钮和结果摘要。
- 核心设计选择：抽出窄接口 `IBackupHistoryRebuilder`，让重建服务可单测且不依赖真实 Ludusavi；重建只读取磁盘并协调索引，不删除、不上传、不修改真实归档；失败游戏保留原索引。
- 主要修改文件：Contracts `RepositoryRebuildDtos.cs`/`MessageTypes`、`RestoreDependencies`、`BackupOrchestrator`、`RepositoryRebuildService`、`Program`、`IpcRequestDispatcher`、`DashboardViewModel`、`MaintenanceView`，以及 Worker/Playnite 测试。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `51/51`、Worker `128/128`、Playnite `211/211`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 记录存储初始化与 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实 Ludusavi 备份目录下点击重建后，与磁盘文件逐一核对版本数量和归档路径。
- Commit：`7845a53`。
- 下一项：继续 Layer B，从 `PATH-REMAP-001` 批量路径迁移开始。

## 2026-08-13 METADATA-BACKUP-001 GameSaveCenter 自身元数据灾备

- Task ID：`METADATA-BACKUP-001`。
- 实现内容：新增 `metadata.backup.create` IPC 与 `MetadataBackupService`，在数据目录 `MetadataBackups` 生成有上限、自描述的 ZIP 灾备包。包含 SQLite 一致性快照（`VACUUM INTO`）、脱敏的 Worker 设置、版本与校验清单、README；明确不包含存档、媒体、Rclone 配置或凭据。维护中心诊断页新增“导出元数据灾备”按钮和结果摘要。
- 核心设计选择：数据库快照使用 SQLite `VACUUM INTO` 获取一致副本，避免直接复制 WAL 状态；设置文件先脱敏再入包；ZIP 超过 512 MiB 直接失败并清理临时文件，防止灾备目录膨胀。
- 主要修改文件：Contracts `MetadataBackupDtos.cs`/`MessageTypes`、`MetadataBackupService`、`Program`、`IpcRequestDispatcher`、`DashboardViewModel`、`MaintenanceView`，以及 Worker/Playnite 测试。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `51/51`、Worker `127/127`、Playnite `211/211`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 记录存储初始化与 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实维护中心点击导出后的包内容人工复核，以及从灾备包恢复数据库的演练。
- Commit：`e7e8fb2`。
- 下一项：继续 Layer B，从 `REPOSITORY-REBUILD-001` 备份仓库索引重建开始。

## 2026-08-13 DB-MIGRATION-001 历史数据库 Fixture 升级 Harness

- Task ID：`DB-MIGRATION-001`。
- 实现内容：新增可复用的 Worker 测试 Harness `DatabaseMigrationHarness`，在临时目录创建旧版 SQLite Fixture，然后运行当前 `SqliteStateStore.InitializeAsync` 完整迁移，并报告首次/重复初始化、缺失表、缺失列、各表行数和明确错误。覆盖旧库升级保留数据、全新数据库创建、非法旧 schema 失败报告三个场景。
- 核心设计选择：Harness 只操作临时数据库，绝不接触用户数据；旧 Fixture 覆盖历史上后加的列（`match_input_hash`、`session_id`、`archive_path`、`classification_state`、`shared_directory`、`if_already_running`、`risk_category`、`resolved_target_path` 等）和旧版 `backup_versions` 主键，验证真实迁移路径而非只测 `CREATE TABLE`。
- 主要修改文件：`tests/GameSaveCenter.Worker.Tests/Infrastructure/DatabaseMigrationHarness.cs`、`tests/GameSaveCenter.Worker.Tests/DatabaseMigrationHarnessTests.cs`。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `51/51`、Worker `126/126`、Playnite `211/211`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 记录存储初始化与 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实旧版本用户数据库升级后的功能回归，以及大量历史数据迁移时长的人工复核。
- Commit：`187744b`。
- 下一项：继续 Layer B，从 `METADATA-BACKUP-001` GameSaveCenter 自身元数据灾备开始。

## 2026-08-13 INTEGRITY-001 全局完整性自检

- Task ID：`INTEGRITY-001`。
- 实现内容：新增 Worker 全局完整性自检与 IPC `integrity.check`。检查 SQLite `integrity_check`、外键、必需表、关键目录可写性、已配置的 Ludusavi/Rclone 可执行文件，以及备份归档、游戏工具和媒体索引指向的文件是否存在。维护中心诊断页新增“完整性自检”按钮和结果摘要。
- 核心设计选择：自检只读，不自动删除或修复任何数据；数据库问题标记为 Critical，文件/可执行文件缺失标记为 Warning；缺失路径只展示最多 20 个示例，避免大量记录刷屏。
- 主要修改文件：Contracts `IntegrityDtos.cs`/`MessageTypes`、`SqliteStateStore.Integrity.cs`、`IntegrityCheckService`、`Program`、`IpcRequestDispatcher`、`DashboardViewModel`、`MaintenanceView`，以及 Worker/Playnite 测试。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `51/51`、Worker `123/123`、Playnite `211/211`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 记录存储初始化与 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实维护中心点击完整性自检后的结果展示，以及缺失文件/数据库异常场景的人工复核。
- Commit：`85cb884`。
- 下一项：继续 Layer B，从 `DB-MIGRATION-001` 历史数据库 Fixture 升级 Harness 开始。

## 2026-08-13 SAFE-MODE-001 安全模式

- Task ID：`SAFE-MODE-001`。
- 实现内容：新增全局“安全模式”设置并持久化到插件与 Worker 设置。开启后暂停自动退出备份、游玩中定时备份、自动媒体同步、自动工具启动、会话存档快照与保护提示、云端自动上传和云端自动重试；手动备份、手动媒体同步、手动工具启动、诊断与恢复仍可用。设置页新增开关，维护中心诊断页在开启时显示醒目提示，诊断摘要包含安全模式状态。
- 核心设计选择：安全模式只关闭“自动产生副作用”的路径，不封锁用户明确点击的手动操作；Worker 侧在 `GameSessionCoordinator`、`GameToolService`、`BackupOrchestrator`、`MediaSyncService` 和 `CloudRetryService` 同时设门禁，避免绕过 Playnite 事件的进程检测路径重新触发自动化。旧设置 JSON 缺字段时保持关闭。
- 主要修改文件：Contracts `WorkerSettingsDto`/`WorkerSettingsSnapshotDto`/`DashboardSnapshotDto`、`WorkerOptions`、`GameSessionCoordinator`、`GameToolService`、`BackupOrchestrator`、`MediaSyncService`、`CloudRetryService`、`DashboardService`、`IpcRequestDispatcher`、Playnite 设置与维护视图、诊断摘要，以及 Worker/Playnite 测试。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `51/51`、Worker `120/120`、Playnite `211/211`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 记录存储初始化与 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实游戏中开启/关闭安全模式后验证不再触发自动备份与工具启动，以及设置页/维护提示在主题与 DPI 下的人工观感。
- Commit：`a70d73f`。
- 下一项：继续 Layer B，从 `INTEGRITY-001` 全局完整性自检开始。

## 2026-08-13 NOTIFY-LEVELS-001 通知级别收口

- Task ID：`NOTIFY-LEVELS-001`。
- 实现内容：补齐 `ImportantOnly/Summary/Verbose` 三级通知语义：`ImportantOnly` 仅显示失败/取消任务及警告/失败退出摘要；`Summary` 保持当前每次游戏退出只发一条聚合摘要；`Verbose` 在最终摘要之外逐条显示终态任务。设置页新增“通知级别”ComboBox，旧设置 JSON 缺字段时归一为 `Summary`。
- 核心设计选择：把级别决策抽成 Core 纯逻辑 `NotificationLevelPolicy`，插件只消费该决策；Session 任务无论级别都继续进入统一聚合器，避免破坏一次 Session 一条摘要的去重契约。枚举值从 1 开始，避免旧 JSON 缺字段时误解析成 `ImportantOnly`。
- 主要修改文件：`Contracts/Enums.cs`、`Core/Services/NotificationLevelPolicy.cs`、`Playnite/GameSaveCenterPlugin.cs`、`Playnite/Settings/GameSaveCenterSettings.cs`、`GameSaveCenterSettingsView.xaml`、Core 与 Playnite 测试、项目记忆与交接文档。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `51/51`、Worker `118/118`、Playnite `210/210`；`validate-source.py`、XAML 结构、WPF 静态门禁 0 errors / 33 warnings / 153 info、五种窗口 `render-qa OK`。
- 真实宿主验证：`dev-install-run.ps1` 构建、打包、普通权限安装并启动 Playnite；`playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录 `0.6.70.0 loaded`，`worker-launch.log` 记录存储初始化与 `Application started`；安装后无新增插件异常。
- MANUAL QA REQUIRED：真实游戏退出触发三种级别时的实际通知观感，以及设置页主题/DPI/连续缩放人工复核。
- Commit：`d99b8ef`。
- 下一项：继续 Layer B，从 `SAFE-MODE-001` 安全模式开始。

## 2026-08-13 LAYER-A-AUDIT-001 / DIAGNOSTICS-001

- Task ID：`LAYER-A-AUDIT-001`、`DIAGNOSTICS-001`。
- 实现内容：修复多设备等价判断、Restore Readiness 大文件取消/Hash、分磁盘环境检查和重复 Manifest 防护；新增维护中心脱敏诊断包导出入口与 IPC。
- 核心设计选择：只有完整 Manifest 内容指纹相同才判定跨设备内容等价；仅大小/文件数相同保守标记未知分歧；诊断包限制日志/任务/审计数量和总大小，拒绝打包数据库、存档、媒体与凭据。
- 主要修改文件：`BackupContentFingerprint`、`DeviceConflictDetector`、`RestoreReadinessService`、`EnvironmentCheckService`、`FileManifestDiffService`、`DiagnosticsPackageService`、Contracts IPC DTO、`DashboardViewModel`、`MaintenanceView` 及对应 Core/Worker/Playnite 测试。
- 测试结果：隔离 Release 构建 0 warnings / 0 errors；Core `47/47`、Worker `118/118`、Playnite `210/210`；`validate-source.py` 通过；WPF 静态门禁 0 errors / 33 warnings / 153 info（均为既有布局提示）；五种窗口 `render-qa OK`；一键开发安装与 Playnite 启动成功，日志确认插件 `0.6.70` 加载、Worker 初始化并进入 `Application started`。
- AUTO VERIFIED：上限/脱敏/排除敏感文件测试、指纹与恢复校验单测、源码/XAML/WPF/render-qa、真实构建/安装/启动。
- MANUAL QA REQUIRED：真实游戏 Restore/Undo、真实 Rclone 断网与凭据失效、真实双设备、多种 EXE/LNK/BAT/PS1、1000+ 游戏库、主题/DPI/连续缩放；未把自动化结果冒充这些验收。
- Commit：`cbd0cdb`（Layer A 审计修复）、`c6bfda2`（诊断包）。
- 下一项：继续 Layer B/C 任务；完整 38 项 Epic 尚未完成，通知级别随后已由 `NOTIFY-LEVELS-001` 收口。

## 2026-08-13 UI-QA-REAL-006 设置分类 Tab 实际裁切修复

- 用户反馈上一轮“底部安全区域”没有视觉变化；复核截图后确认不是安全区数值太小，而是 `TabItem` 的圆角 `Border` 直接作为模板根并开启 `ClipToBounds=True`，圆角 Chrome 被 `TabPanel` 的布局槽贴底裁平。
- `GscRedesignSettingsTabControl` 改为 `StackPanel` 承载 `TabPanel` 与真实的 `SettingsHeaderBottomSafetyZone` 占位元素；紧凑顶部横向模式隐藏该占位元素，保留原有滚动方向、选中逻辑和键盘导航。
- `GscRedesignSettingsTabItem` 改为不裁切的 `TabItemRoot` + 独立 Chrome；Chrome 顶部对齐并保留 2 DIP 底部安全边距，移除会放大边界裁切的 `ClipToBounds=True`，不改变 Header、Binding、FocusVisualStyle 或业务行为。
- RenderHarness 增加 `chromeBottom` / `chromeSafety` 几何门禁；最终 5 种窗口尺寸渲染通过，短窗口/滚动探针全部通过，设置页视觉复核确认卡片底部圆角不再被直线切平。
- 验证：XAML/source 门禁、`git diff --check`、Playnite `210/210`、`render-qa OK`。真实 Playnite 宿主的主题、DPI 和连续缩放仍需人工复核。

## 2026-08-13 UI-QA-REAL-005 首页与设置页几何兼容性修复

- 用户在 4K 全屏反馈首页“今日概览”没有贴在内容顶端、当前游戏卡空间紧张，以及设置页分类卡底部圆角仍像被切掉。
- `OverviewView` 将宽屏右侧摘要滚动面和内容面显式锚定顶部；Hero/当前游戏宽屏列从 `1.25* + 0.75*` 调整为 `1.1* + 0.9*`，保留原有堆叠断点、命令和绑定。
- `GscRedesignSettingsTabControl` 在共享 `TabPanel` 外增加 `SettingsHeaderItemsHost` 底部安全区，设置顶部对齐、像素对齐和布局取整，避免 `ScrollViewer` viewport 在最后一张分类卡底部裁剪圆角。
- RenderHarness 修复设置页入口动画导致的空白截图，并加入 Overview secondary top delta、当前游戏宽度比、Settings 最后一张 Tab 底部几何门禁。
- 验证：`python scripts/validate-source.py`、WPF 静态门禁、`git diff --check`、`render-qa` 全绿；隔离 Release 全量测试 Core `42/42`、Worker `117/117`、Playnite `210/210`。真实 Playnite 宿主主题/DPI/连续缩放仍需用户复核。

## 2026-08-13 DEV-INSTALL-007 兼容未发现 Playnite 路径

- 用户反馈另一台机器在 Playnite 使用自定义/便携安装路径时，一键入口在构建前报 `TrustedPlayniteExecutables` 空数组绑定错误；实际失败点是安装前进程处理，不是项目编译失败。
- `scripts/dev-install-run.ps1` 现在增加 App Paths、PATH 和规范化路径发现，并允许可信 Playnite 候选为空；没有运行中的 Playnite 时继续构建、打包和安装，仅提示安装后无法自动启动。指定了不存在的 `-PlayniteExecutable` 时给出直接错误。
- 如果 Playnite 仍在运行但没有任何可确认的可信路径，仍然停止安装并要求用户退出 Playnite 或显式指定路径，保留防止误杀未知进程的安全边界。
- 根目录 `GameSaveCenter-Run.cmd` 与源码门禁同步到 `DEV-INSTALL-007`。
- 验证要求：PowerShell AST、`scripts/validate-source.py`、`git diff --check`；真实另一台机器的自定义 Playnite 路径和无 Playnite 进程场景仍需用户手工执行一键入口确认。

## 2026-08-13 DEV-INSTALL-006 无窗口 Playnite 残留回收

- 真实复现一键安装失败：Playnite PID 48188 收到正常退出请求后 20 秒仍未退出；该进程属于当前会话、路径为已发现的 `D:\software\Playnite\Playnite.DesktopApp.exe`，且 `MainWindowHandle=0`，因此没有窗口可供 `CloseMainWindow()` 再次关闭，并非管理员权限问题。
- 安装器继续优先正常退出；超时后仅结束当前会话、路径与本次 Playnite 候选完全一致、且已无主窗口的残留实例。路径不可读、路径不可信、跨会话或仍有主窗口时停止安装，禁止按名称广泛终止。
- 停止操作处理 PID 在检查与终止之间自然退出的竞态，不再把已退出进程误报为失败。根目录入口同步要求 `DEV-INSTALL-006`，源码门禁锁定上述安全条件。
- 验证：PowerShell 语法与 `scripts/validate-source.py` 通过；同一 PID 48188 场景下 `DEV-INSTALL-006` 自动识别并结束无窗口残留，随后完整 Release 构建 0 警告/0 错误，Core 42/42、Worker 117/117、Playnite 203/203，全量打包、普通用户安装和启动成功。
- 安装 DLL 产品版本为 `0.6.70+4125a5448b1d903c1122d6ba596b8ca31597a714`，证明安装的是执行本阶段前的当前 HEAD 而非旧 `c01e9d5` 包；11:14:33 的 Playnite 日志确认插件从用户扩展目录加载，11:14:38 的 Worker 日志确认存储初始化、过期任务整理与 `Application started`，Playnite/Worker 进程持续运行。

## 2026-08-13 DEV-INSTALL-005 构建输出隔离、安装暂存隔离与批量保护确认

**问题根因：**

- 用户提供的日志中 Contracts/Core/Worker/Playnite 编译均已成功；真正失败的是 Playnite.Tests 与 Worker.Tests 覆盖标准 `bin\Release` 输出时被拒绝访问。
- 当前机器仍有旧的 `dotnet/testhost` 或 Worker 持有仓库标准 `bin/obj` 文件。该类文件锁不应通过默认 UAC 或按进程名强杀解决。

**实现内容：**

- `scripts/build.ps1` 增加可选 `-OutputRoot`，通过 `GscBuildOutputRoot` 为每个项目生成独立的 `bin/obj`，并关闭 MSBuild 节点复用；普通 IDE/CI 未传参数时仍使用原路径。
- `scripts/dev-install-run.ps1` 每次使用 `artifacts\dev-build\<Configuration>\<guid>`，不再清理正在使用的标准输出；安装临时目录改为唯一 GUID 路径。安装器修订号为 `DEV-INSTALL-005`，默认不请求管理员权限。
- `scripts/package.ps1` 支持从隔离输出打包，并让 Worker 的运行时恢复/发布也使用同一隔离根目录。
- Overview 的批量“启用推荐自动保护”增加修改前预览确认；只提交已选且可选游戏，不执行备份/恢复、不覆盖其他策略设置。

**验证结果：**

- Release 隔离构建：0 warnings / 0 errors；Core `37/37`、Worker `81/81`、Playnite `202/202`。
- `validate-source.py`、XAML 结构检查、WPF 静态校验与 `git diff --check` 通过；WPF 静态校验为 0 errors，剩余 warnings 是既有布局审查提示。
- `scripts/package.ps1 -SkipBuild -BuildOutputRoot ...` 成功生成 `artifacts\GameSaveCenter-0.6.70.pext`。
- 使用正常桌面文件权限执行 `scripts/dev-install-run.ps1 -Configuration Release -NoStart -SkipClean` 成功，无 UAC；安装报告确认版本 `0.6.70`、插件 DLL `0.6.70.0`。
- 真实 Playnite 启动验证成功：`playnite.log` 记录 `Loaded plugin: GameSaveCenter, version 0.6.70`，插件日志记录加载 `GameSaveCenter.Playnite 0.6.70.0`；`worker-launch.log` 记录 Worker 初始化完成并进入 `Application started`。

**未替代的人工验收：**

- 真实用户交互、主题/DPI/键盘焦点、连续缩放，以及真实 Rclone、多设备冲突、游戏恢复/撤销、EXE/LNK/BAT/PS1 启动项、900+ 游戏库仍需人工验收；本阶段不把静态测试或一次宿主启动写成全部产品验收完成。

## 2026-08-13 DEV-INSTALL-004 回退默认提权并按归属回收 Worker

**实现内容：**

- 移除一键安装器启动时的管理员检测、UAC 自重启和安装过程中的管理员停止逻辑；普通用户运行 `.cmd` 不再被强制要求授权。
- 安装前先对现有 Playnite 窗口调用 `CloseMainWindow()`，等待 Playnite 的 `OnApplicationStopped` 执行既有 `WorkerLauncher.StopOwnedWorker()`，再继续覆盖安装。
- Playnite 退出后若仍有 Worker，只允许处理路径明确位于当前扩展目录 `Worker\GameSaveCenter.Worker.exe` 的残留进程；路径不可读取或属于其他扩展时直接停止安装，避免按进程名误杀。
- 安装器修订号更新为 `DEV-INSTALL-004`，根目录入口同步检查该版本，防止旧入口继续运行已废弃的提权代码。

**验证结果：**

- PowerShell AST 解析、`git diff --check` 通过。
- 当前会话真实执行 `DEV-INSTALL-004` 时没有出现 UAC；日志确认先完成“Playnite 已退出，等待插件回收 Worker”，随后才进入 NuGet 恢复。
- 本次执行后 Playnite 与 Worker 均已退出；后续 NuGet 恢复因当前机器 SDK 缺失 `Microsoft.NET.SDK.Workload*Locator` 目录而退出码为 1，与进程处理无关。用户重启后的上一轮构建/测试/安装已成功。

**结论：**

- `GameSaveCenter.Worker` 不是用户手动启动的陌生进程；`worker-launch.log` 记录它由已安装的 GameSaveCenter 插件从扩展目录启动。残留通常来自 Playnite 异常退出或旧权限上下文，安装器现在先走正常退出回收链路，不再把整个安装提权。

## 2026-08-13 DEV-INSTALL-003 安装入口版本识别

**实现内容：**

- 一键安装 PowerShell 脚本输出自身的绝对路径和安装器修订号 `DEV-INSTALL-003`，避免旧日志/旧脚本错误地被当作当前修复结果。
- `GameSaveCenter-Run.cmd` 启动前检查脚本是否包含当前修订号；从旧副本或错误目录启动时立即停止并显示期望路径，不再执行旧的进程停止逻辑。
- 重新确认 PID 3896 是无父进程的孤儿 Worker；当前执行会话对 `Stop-Process`、`taskkill /F` 和命名管道请求均无权/无响应，不能把当前环境的强制结束结果伪造为成功。

**验证结果：**

- 当前仓库 `scripts/dev-install-run.ps1` 第 147 行是扩展路径筛选，不是用户日志中的旧权限异常；旧错误文本不再存在于当前脚本。
- PowerShell AST 解析与 `git diff --check` 通过；入口校验已加入。
- 该阶段已被 DEV-INSTALL-004 的正常退出方案取代；保留旧日志证据，不能作为当前安装器行为判断依据。

## 2026-08-13 DEV-INSTALL-002 一键安装自动提权与停止竞态修复

**实现内容：**

- `scripts/dev-install-run.ps1` 在构建前检查当前 PowerShell 是否为管理员；普通会话自动通过 UAC 重启自身，并原样转发配置、安装路径、`-NoStart` 与 `-SkipClean` 参数。
- 停止进程时先尝试当前会话；权限不足时只对已确认的 PID 请求管理员停止，并在提权进程中再次核对进程名，避免 UAC 等待期间 PID 被复用后误杀其他进程。
- 保留 `Get-Process` 与 `Stop-Process` 之间的退出竞态处理，并在超时抛错前增加最后一次进程确认；已经退出的 Worker 不再阻塞安装，也不再重复刷屏重试。

**验证结果：**

- PowerShell AST 解析通过；`git diff --check` 通过。
- 重新执行 `scripts/package.ps1 -Configuration Release -SkipBuild` 成功；`.pext` 必需内容断言通过，包根目录包含 `GameSaveCenter.Core.dll`。附件中的旧 Playnite 失败日志发生在 21:50/21:53，而当前安装目录的 Core DLL 时间为 22:04，旧失败原因是旧包缺失 Core，不代表当前包仍缺文件。
- 在当前 Codex 无桌面/中完整性会话中真实执行一键安装时，UAC 启动返回 Windows `0xc0000142`；随后对已确认的 `GameSaveCenter.Worker [3896]` 的精确管理员停止仍返回“拒绝访问”。这属于当前执行会话没有可交互高完整性令牌，不能替代用户桌面 UAC 验收。
- 因 PID 3896 仍被外部会话持有，本轮未覆盖真实 Playnite 安装目录，也未宣称扩展已由真实宿主加载。用户在桌面双击 `GameSaveCenter-一键构建安装运行.cmd` 并允许 UAC 后，需要继续核对 `playnite.log` 的 `ExtensionFactory:Loaded plugin: GameSaveCenter` 与 `extensions.log`。

**下一步：**

- 在用户桌面完成一次 UAC 允许后的真实一键安装；若 Worker 仍是更高权限/受保护进程，则先退出其所属 Playnite 或重启系统，再复验包覆盖和扩展加载。

## 2026-08-13 NOTIFY-001 / MULTI-DEVICE-001 / RCLONE-RELIABILITY-001 会话摘要、设备分叉与云端可靠性

**实现内容：**

- 退出备份与媒体任务现在携带同一 `SessionId`，任务数据库和事件广播保留该字段；Playnite 对同一游戏会话的终态任务做一次聚合通知，避免“备份完成 / 媒体完成 / 云同步完成”连续弹出。摘要直接基于 Task Center 使用的 `TaskStatusDto` 终态生成：本地备份成功、云端失败、媒体失败和取消状态分别可见；Rclone 云端失败仍明确显示本地备份已保留。
- 多设备摘要增加 `ParentBackupId` 共同基线。两台设备从同一父版本产生不同子版本时标记 `DivergedFromCommonBase`，已知父子线性关系不误报冲突；仍然只记录人工决策，不自动选择、合并、覆盖或删除任何一方。旧 SQLite 通过既有 `EnsureColumnAsync` 与备份版本迁移安全补列。
- Rclone 错误分类为网络、不完整传输、凭据、权限、远端不存在和未知错误。只有网络/不完整传输进入既有有限退避；凭据、权限和远端配置错误显示明确可行动错误并停止自动重试。本地备份不会因云端失败被删除，命令白名单仍为 `copy/check/lsf/cat/version`，没有 `sync/delete/purge/move`。

**主要修改文件：**

- `src/GameSaveCenter.Contracts/GameDtos.cs`、`OperationDtos.cs`、`DashboardDtos.cs`、`DeviceStateDtos.cs`
- `src/GameSaveCenter.Core/Services/GameSessionSummaryBuilder.cs`、`DeviceConflictDetector.cs`、`Models/DomainModels.cs`
- `src/GameSaveCenter.Worker/Services/GameSessionCoordinator.cs`、`TaskCoordinator.cs`、`BackupOrchestrator.cs`、`MediaSyncService.cs`、`CloudRetryService.cs`、`RcloneFailureClassifier.cs`
- `src/GameSaveCenter.Worker/Persistence/SqliteStateStore.cs`、`Ipc/TaskEventBroadcaster.cs`
- `src/GameSaveCenter.Playnite/GameSaveCenterPlugin.cs`、`ViewModels/DashboardViewModel.cs`、`Infrastructure/SnapshotComparers.cs`
- `tests/GameSaveCenter.Core.Tests/GameSessionSummaryBuilderTests.cs`、`DeviceConflictDetectorTests.cs`、`tests/GameSaveCenter.Worker.Tests/CloudRetryPersistenceTests.cs`、`DashboardHealthPersistenceTests.cs`

**验证结果：**

- Core Release：0 警告、0 错误；Core 测试 35/35。
- Worker Release：0 警告、0 错误；Worker 测试 81/81。
- Playnite Release：0 警告、0 错误；Playnite 测试 202/202。
- `git diff --check`、`scripts/check-xaml.ps1`、`scripts/validate-source.py`、WPF 静态校验通过；WPF 校验仍报告仓库既有 33 条 warning、153 条 info，无 error。

**AUTO VERIFIED：**任务 SessionId/SQLite 往返、退出摘要纯逻辑、共同基线冲突判定、Rclone 错误分类与有限重试策略、代码构建和测试、XAML/WPF/source 门禁。

**MANUAL QA REQUIRED：**真实 Rclone 断网/凭据过期/远端部分上传；真实两台设备分叉与隔离恢复；真实 Playnite 最终包扩展扫描、主题/DPI/键盘和退出通知；PID 3896 仍由外部管理员会话持有，当前会话无法终止，因此一键覆盖安装仍需管理员任务管理器结束进程或重启后复验。

**提交：**代码提交 `304054a feat: harden session device and cloud reliability`；冲突赢家必须显式选择的安全修正提交 `3a39853 fix: require explicit device conflict winner`；本条文档随后单独提交。push 到共享 `main` 仍受安全策略阻止，未声称 push 成功。

**最终交接：**Release self-contained 包 `artifacts/GameSaveCenter-0.6.70.pext` 已生成并通过包内容断言；隔离 Playnite 记录到 `Application started`，但截图步骤因当前会话无桌面句柄失败，不能视为真实扩展加载。只剩真实宿主、Rclone、多设备和恢复手工 QA，不新增第 15 项功能。

## 2026-08-13 SMART-PROTECT-001/002 智能保护提示与最近游戏批量策略

**实现内容：**

- 游戏完整停止流程现在等待现有存档识别/匹配结果；识别到候选路径、已接受候选或 Ludusavi 匹配时，首次完整退出可在既有 Dashboard 对话框中选择“启用推荐策略 / 以后再说 / 不再提醒”。识别不到存档时只记录“暂未发现存档”审计，不打扰用户；后续识别到存档仍可触发提示。
- SQLite 持久化 `NeverShown/Deferred/Enabled/Dismissed` 提示状态、最近识别结果和提示时间；“以后再说”按 7 天冷却；启用推荐策略同时打开游戏结束自动保护。停止请求的 Playnite IPC 超时单独延长到 3 分钟，覆盖最多约 2 分钟的存档扫描。
- Overview 既有“最近 30 天游戏”卡片现在显示“已保护 / 未匹配 / 存档未保护 / 风险”状态；已保护项不可选，其他项可多选并通过一个 Worker 请求批量启用推荐保护（游玩中与退出后），操作写入审计并刷新快照。未新增主导航页，保留现有滚动、键盘和列表结构。

**验证结果：**

- Core Release 构建 0 警告、0 错误；Core 测试 29/29；Worker Release 构建 0 警告、0 错误，Worker 测试 76/76；Playnite Release 构建 0 警告、0 错误，Playnite 测试 202/202。测试编译使用隔离 `artifacts/smart-test/final-*` 输出，避开旧测试宿主对标准目录的句柄锁。
- `scripts/validate-source.py`、`scripts/check-xaml.ps1`、`git diff --check` 通过；`render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/smart-protection-final` 通过所有窗口尺寸、页面滚动与表格视口门禁。

**验证边界：**

- 隔离 RenderHarness 和隔离 Playnite 可用于编译/离屏布局验证；当前会话没有取得桌面句柄，也没有在真实最终包上完成扩展扫描，不能据此宣称真实宿主加载已验证。
- `GameSaveCenter.Worker [3896]` 仍由外部管理员会话持有且当前会话无权读取/终止；真实覆盖安装仍需管理员任务管理器结束该 PID 或重启后执行。

**提交：**代码与测试、文档分别提交；push 到共享 `main` 受安全策略拦截，未声称 push 成功。

**下一项：**BACKUP-001，继续复用既有存档中心与 Worker 备份流程，先补可恢复性/一致性门禁，再扩展多设备与 Rclone。

## 2026-08-13 GAME-TOOL-003/004 重复实例策略与风险分类

**实现内容：**

- 为自定义启动项增加 `Skip`（默认）、`Restart`、`AllowAnotherInstance` 三种已有实例策略；只对解析后的自定义 EXE 完整路径做匹配，不按进程名直接关闭其他程序。无法读取同名进程路径时保守停止，Restart 在关闭/强制结束前再次确认 PID 对应的完整路径。
- 为 CustomExecutable 增加 `Unknown`（默认）、`GeneralUtility`、`GameModification` 风险分类，并写入 SQLite。反作弊风险游戏中，修改器/CT 与游戏修改工具不会自动启动；通用工具仍可自动启动；未分类自定义工具的自动启动先暂缓，需用户在既有工具设置页明确分类并保存。
- 在 TrainerCenter 既有设置 Inspector 中增加“已有实例时”和“工具类别”下拉框；保留既有导入、启动、延迟、退出跟踪、管理员权限及滚动/虚拟化结构。手动启动遇到已有同路径实例时会显示已跳过，而不是重复拉起。

**验证结果：**

- Worker Release 构建：0 警告、0 错误；Worker 测试 74/74 通过（独立 `artifacts/test-bin` 输出，避免 PID 3896 文件锁）。
- Playnite Release 构建：0 警告、0 错误；Playnite 测试 200/200 通过。
- `validate_wpf_ui.py` 通过（0 errors；33 个既有 warning、153 个既有 info）；`render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/game-tool-policy-render` 通过，包含 Trainer Inspector 在窄窗口的滚动几何。
- 追加路径比较、默认策略、SQLite 策略往返和 UI 契约测试；`git diff --check` 通过。

**验证边界：**

- 隔离 Playnite/RenderHarness 已验证编译和离屏布局，但当前会话仍无法取得桌面句柄或完成真实扩展扫描；不能据此宣称真实宿主已加载本阶段包。
- PID 3896 仍是当前会话无权读取/终止的外部 Worker；真实覆盖安装和宿主加载仍需管理员任务管理器结束该 PID 或重启后执行。

**提交：**代码与测试待提交；文档单独提交。

**下一项：**SMART-PROTECT-001/002，复用现有游戏停止、存档识别和策略页，不新增主导航页。

## 2026-08-13 ONBOARDING-001 环境准备与首次使用入口

**实现内容：**

- 在现有维护中心诊断页顶部增加“环境准备”卡片；首次打开 Dashboard 会定位到维护中心，检查结果可随时重新运行，也可以持久化跳过首次检查。
- 新增 Worker IPC `environment.check` 与 `EnvironmentCheckService`：检查 Worker/IPC、数据/存档/媒体目录读写、SQLite 临时表读写、Playnite 游戏缓存、Ludusavi 版本与只读备份列表调用、Rclone 版本与只读远端探测、磁盘可用空间。
- Rclone 未配置时显示“跳过”（可选能力），不会误报为基础环境失败；环境检查不自动备份、不上传、不删除、不覆盖真实存档。测试备份按钮只有用户明确点击且当前游戏已匹配时才可执行。
- 维护中心沿用既有页面滚动与有限 DataGrid 视口，没有新增主导航页或改变现有表格虚拟化骨架；首次使用状态加入 Playnite 持久设置。

**验证结果：**

- Worker Release 构建：0 警告、0 错误；新增环境检查测试与现有 Worker 测试合计 70/70 通过（隔离输出目录，规避用户 Worker PID 3896 的文件锁）。
- Playnite Release 构建：0 警告、0 错误；Playnite 测试 198/198 通过。
- `scripts/validate-source.py`、WPF UI 静态校验、`git diff --check`、`scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/onboarding-render` 全部通过；render QA 覆盖 1040x700、1280x720 等窗口尺寸。
- `scripts/package.ps1 -Configuration Release -SkipBuild` 成功，生成 `artifacts/GameSaveCenter-0.6.70.pext` 与 zip，并通过 self-contained Worker 包内容断言。

**验证边界：**

- 隔离 Playnite 进程可启动到应用层，但当前会话无法获取桌面句柄，且该隔离数据目录没有进入真实扩展加载阶段；没有据此宣称 Playnite 宿主加载成功。
- 真实用户 Worker `GameSaveCenter.Worker [3896]` 的路径/所有权不可读且当前会话无权终止，真实覆盖安装和最终 IPC/UI 人工验收仍需用户在管理员任务管理器结束该 PID 或重启后执行。

**提交：**待代码、文档分别提交。

**下一项：**按附件顺序继续 GAME-TOOL-003/004/SMART；保持每阶段独立构建、测试、打包、宿主加载日志核对。

## 2026-08-12 POLICY-001 策略模板与 Playnite 包部署验证

**实现内容：**

- 在现有 per-game policy 之上增加可复用策略模板：默认、重要游戏、高频游玩、仅退出后、仅手动五个稳定内置模板；内置模板不可编辑/删除。
- 增加用户模板的 SQLite 持久化、创建/更新/删除/应用 IPC，以及现有 Save Center 策略页中的模板编辑区；应用模板只复制当前值，不建立继承关系。
- 模板统一做数值归一化，并强制 `AllowAutomaticRestore=false`，避免模板功能绕过恢复安全边界；应用和删除操作写入既有审计日志。
- 修复“新建模板副本”先清空选择再读取名称的问题；副本现在会正确使用“原名称（副本）”。
- 修复 Playnite 包部署：安装包显式包含 `GameSaveCenter.Core.dll`；Worker 按 `win-x64` 还原/自包含发布，并检查 `runtimeconfig.json` 的 `includedFrameworks` 与 host/runtime 文件，避免把 framework-dependent Worker 混入安装包。
- 开发安装脚本启动 Playnite 时使用其安装目录作为工作目录，减少宿主启动环境差异。

**验证结果：**

- Core 测试：29/29 通过；Worker 测试：69/69 通过；Playnite 测试：197/197 通过。
- Worker 与 Playnite Release 隔离构建：0 警告、0 错误。
- `scripts/package.ps1 -Configuration Release -SkipBuild` 成功；`.zip/.pext` 均通过包内容断言，包含 Core DLL 与 self-contained Worker runtime。
- `validate-source.py`、`check-xaml.ps1`、WPF 静态校验无错误；`render-qa.ps1` 全部通过，现有页面/DataGrid/Settings 断点几何保持门禁结果。
- 已有真实 Playnite 日志在 22:12:06 记录 `ExtensionFactory:Loaded plugin: GameSaveCenter, version 0.6.70`，说明包含 Core DLL 的插件本体可以被宿主发现并加载。

**验证边界：**

- `AUTO VERIFIED`：源码、测试、构建、包内容、自包含 runtimeconfig、隔离渲染和既有真实宿主的插件加载日志。
- `MANUAL QA REQUIRED`：清理仍持有旧 Worker 文件锁的 PID 3896 后，在用户真实 Playnite 目录重新安装最终 `.pext`，验证 Worker 启动、IPC、模板创建/保存/应用/删除及主题/DPI/键盘操作。当前旧 Worker 无法由本会话取得终止权限；干净 Playnite 首次初始化在本机权限环境下未进入扩展加载阶段，因此不宣称完整真实 UI/Worker 已通过。

**提交：** `e0c123d docs: record policy and package verification`；后续一键安装器修复见 `53399ef`。

**下一项：** `ONBOARDING-001`，继续按附件顺序小阶段推进；每个阶段完成后先构建/测试/打包，再启动 Playnite并检查加载日志。

## 2026-08-12 DEV-INSTALL-RUN 一键安装器进程停止竞态修复

**问题：**

- 一键安装器在 `Get-Process` 找到 `GameSaveCenter.Worker [3896]` 后，进程可能已在退出，原来的 `Stop-Process -ErrorAction Stop` 会把“进程已自行退出”误报为安装失败。
- 修复后将这类停止竞态视为成功，并继续由后续轮询确认进程是否真的消失；如果进程仍存活，则明确停止安装，避免覆盖被占用的 Playnite 扩展文件。

**验证结果：**

- PowerShell AST 语法检查通过，`git diff --check` 通过。
- 实际运行安装器时不再出现“Cannot find a process with the process identifier 3896”；当前会话仍无权终止该存活进程，安装器按安全策略停止，并提示使用管理员任务管理器结束进程或重启后重试。
- 因 PID 3896 仍被占用，本次不能宣称真实 Playnite 目录的完整构建、替换、Worker/IPC 与扩展加载验证已完成；清理进程后需重新运行一键安装并检查 `playnite.log`/`extensions.log`。

**提交：** `53399ef fix: make dev installer process stop idempotent`

**下一项：** 释放 PID 3896 后重新完成真实安装验证；随后继续 `ONBOARDING-001`。

## 2026-08-12 WORKER-LIFECYCLE-001 Playnite 退出清理自有 Worker

**问题与修复：**

- `GameSaveCenterPlugin.OnApplicationStopped` 原先只取消任务和定时器，没有停止 `WorkerLauncher` 自己启动的子进程；Playnite 退出后可能遗留 Worker，下一次一键安装就会遇到扩展目录文件锁。
- `WorkerLauncher` 现在只记录并停止当前插件实例通过 `Process.Start` 创建的 Worker，不按进程名误杀其他实例；退出竞态由 `shutdownRequested` 和原子引用交换覆盖。
- Worker 停止失败只写入 Worker 启动日志，不让 Playnite 退出流程因清理异常再次崩溃；新的启动竞态也会在 Playnite 已退出时立即清理刚启动的子进程。

**验证结果：**

- 源码校验通过；Playnite Release 编译 0 警告、0 错误；新增生命周期回归测试 1/1；Playnite Release 全量测试 198/198 通过。
- `scripts/package.ps1 -Configuration Release` 成功，生成 `.pext/.zip`，包内含 Core DLL 和 self-contained Worker runtime。
- 隔离 Playnite 以独立 `--userdatadir` 启动后进入首次启动向导，日志停在 `FirstTimeStartupWindowFactory`，未进入扩展扫描；因此仍标记 `MANUAL QA REQUIRED`，不宣称隔离宿主加载成功。验证结束后已关闭本次隔离 Playnite。
- 真实用户目录的 `GameSaveCenter.Worker [3896]` 仍存活；PowerShell、提升权限 `Stop-Process` 以及单 PID `taskkill /PID 3896 /F /T` 均返回拒绝访问。真实一键安装和最终 `ExtensionFactory` 加载验证需用户在管理员任务管理器结束该 PID 或重启后重跑。

**提交：** `3f05e16 fix: stop owned worker on Playnite shutdown`

**下一项：** 释放 PID 3896 后重跑真实一键安装并核对 `playnite.log`、`extensions.log`、`worker-launch.log`；随后继续 `ONBOARDING-001`。

## 2026-08-12 RELIABILITY-RESTORE-001-FOLLOWUP 恢复可用性安全闭环与灾难演练

**实现内容：**

- 复用已有 Restore Readiness，不重写恢复体系；严格解析 Manifest，拒绝非法/重复/越界路径，逐文件比对归档路径集合、大小和可用 SHA-256，Manifest 缺失文件不再误判为 `Ready`。
- Manifest 损坏返回 `Failed`；隔离目录创建/访问失败返回 `Failed`；取消继续由 `CancellationToken` 传播；所有提取仍只进入应用自有临时目录并在完成后清理。
- 为现有 RestoreOrchestrator 抽出最小测试接口，生产仍注入现有 concrete service；新增临时 SQLite + 内存假 Ludusavi 灾难演练，覆盖恢复失败自动回滚、成功恢复后 Undo、游戏运行时阻止恢复。

**主要修改文件：**

- `src/GameSaveCenter.Worker/Services/RestoreReadinessService.cs`
- `src/GameSaveCenter.Worker/Services/RestoreOrchestrator.cs`
- `src/GameSaveCenter.Worker/Infrastructure/IRestoreClient.cs`
- `src/GameSaveCenter.Worker/Services/RestoreDependencies.cs`
- `tests/GameSaveCenter.Worker.Tests/RestoreReadinessTests.cs`
- `tests/GameSaveCenter.Worker.Tests/RestoreOrchestratorTests.cs`

**验证结果：**

- Worker Release 构建：0 警告、0 错误。
- Worker 测试：67/67 通过；包括正常/损坏/缺失 Manifest/Hash 不匹配/取消/隔离目录失败及恢复演练。
- 自动化演练只使用临时目录、临时 SQLite 和内存假客户端；真实 Playnite、Ludusavi、游戏目录和真实 Restore/Undo 尚未验证（MANUAL QA REQUIRED）。

**提交：** `d45f65c feat: harden restore readiness and recovery drills`

**下一项：** `POLICY-001`，继续复用现有 per-game policy，不新增主页面。

## 2026-08-12 UI-207-FOLLOWUP-REVALIDATION 当前游戏上下文与设置布局复验

**复验结论：**

- UI-207 实现已存在于 `d2662e3`，本次没有重复修改代码；复核确认未触碰 DataGrid、Worker、IPC、备份链路或新增轮询。
- 设置页仍由共享 TabControl 模板负责滚动：宽屏 232 DIP 左侧垂直导航，紧凑布局顶部水平导航，内容区独立 `SettingsScroller`；5 个分类在全部探针中可达。
- 运行中游戏仍按持久化选择 → 最近启动事件 → 最近活动优先；无运行游戏时恢复 `GamePickerSelectedGameId`，游戏停止不改变选择。真实 Icon 仍为 UI-only、本地优先、48 像素解码、Freeze、LRU 48、失败手柄 fallback，GamePicker 列表不加载真实 Icon。

**本次验证：**

- `python scripts/validate-source.py`、`scripts/check-xaml.ps1`、`git diff --check` 通过；`git fsck --full` 仅报告仓库既有 dangling 对象。
- WPF 静态门禁 0 errors（33 条既有 warnings）；Playnite/Worker Release 项目构建 0 警告/0 错误。
- Core 27/27、Worker 59/59、Playnite 197/197 通过；Playnite/Worker 测试使用 `.tmp/ui207-*` 隔离输出，避免默认 bin 目录文件锁竞争。
- `scripts/render-qa.ps1` 全绿；Settings 覆盖 760/880/920/1100/1400 × 560/700/900，共 15 组探针，5 个分类全部可见，滚动职责正确。
- 离屏 Settings PNG 只验证结构/测量，具体宿主内容不在假数据 harness 中；真实 Playnite 设置页、主题、DPI、键盘、连续缩放和自动定位/Icon 流程仍为 `BLOCKED_ENVIRONMENT`。

**下一步：**

继续附件顺序中的 `POLICY-001`，不要重复实现 UI-207。

## 2026-08-12 PROTECTION-001 最近游玩保护摘要

**做了什么：**

- 在 Core 新增纯计算 `RecentProtectionAssessmentService`，以 Dashboard 已缓存的 `GameStatusDto` 和可配置时间窗口（7/30/90 天）计算“最近游玩 / 已保护 / 需处理 / 未识别存档”，不执行匹配、备份、恢复、磁盘、数据库或云端操作。
- 保护提醒覆盖未识别存档、已识别但从未备份、自动保护关闭、最近游玩发生在最新备份之后、云同步失败，以及最新恢复点损坏/失败/不支持/尚未确认等情况；每个游戏只投影一条最高优先级、可解释原因。
- Dashboard 游戏快照新增最新恢复可用性状态；Playnite 在现有 Overview“风险与提醒”滚动面中增加统计卡、最多 6 条可处理项和“选择”操作。操作只切换当前游戏上下文，不自动执行备份或恢复，也不新增页面或修改 DataGrid。
- Settings 的“自动化与媒体”分类增加最近保护统计窗口，沿用现有 ComboBox、主题资源、响应式断点和设置迁移校验；旧设置缺少该字段时默认 30 天。

**修改文件：**

- `src/GameSaveCenter.Core/Services/RecentProtectionAssessmentService.cs`
- `src/GameSaveCenter.Contracts/GameDtos.cs`
- `src/GameSaveCenter.Worker/Services/DashboardService.cs`
- `src/GameSaveCenter.Playnite/GameSaveCenter.Playnite.csproj`
- `src/GameSaveCenter.Playnite/Infrastructure/SnapshotComparers.cs`
- `src/GameSaveCenter.Playnite/Settings/GameSaveCenterSettings.cs`
- `src/GameSaveCenter.Playnite/Settings/GameSaveCenterSettingsView.xaml`
- `src/GameSaveCenter.Playnite/ViewModels/DashboardViewModel.cs`
- `src/GameSaveCenter.Playnite/Views/OverviewView.xaml`
- `tests/GameSaveCenter.Core.Tests/RecentProtectionAssessmentTests.cs`
- `tests/GameSaveCenter.Playnite.Tests/PortableSettingsTests.cs`
- `tests/GameSaveCenter.Playnite.Tests/SettingsAndAutoSelectSourceTests.cs`

**测试结果：**

- Core 27/27、Worker 59/59、Playnite 197/197 通过；Playnite/Worker 测试使用显式隔离 `OutputPath` 和 `IntermediateOutputPath`，避开本机旧 net472 输出锁。
- Worker 与 Playnite Release 构建 0 警告/0 错误；`python scripts/validate-source.py`、`git diff --check`、`scripts/check-xaml.ps1` 通过。
- WPF 静态门禁 0 errors（仓库既有 warnings）；`scripts/render-qa.ps1` 全绿，覆盖首页、设置页 760/880/920/1100/1400 × 560/700/900 断点及现有表格滚动探针。
- 未在真实 Playnite 宿主内完成主题切换、DPI、键盘和连续缩放人工验收，继续标记 `BLOCKED_ENVIRONMENT`。

**下一步：**

`POLICY-001`：把保护策略状态与用户可理解的策略建议继续接入现有游戏上下文；保持小阶段、独立测试、文档、commit 和 push。

## 2026-08-12 HEALTH-001 每游戏备份健康摘要

**做了什么：**

- 在 Contracts 增加稳定的 `Healthy / Attention / Risk / Unknown` 四态和可解释的健康摘要/理由；保留旧 `Ready / Warning` UI 状态兼容，避免缓存和旧渲染夹具变成错误状态。
- 在 Core 新增纯函数 `GameHealthAssessmentService`：只消费最近游玩、最新备份时间/版本数、备份任务失败、恢复可用性、未解决 finding、云端状态等证据，不读磁盘、不打开 ZIP、不访问网络。
- Worker Dashboard 的 SQLite 聚合一次读取每个游戏的最新恢复可用性 JSON、备份任务失败次数/最近状态、未解决 warning/error 和最新理由；加入 `(game_id, task_type, created_utc)` 索引，避免在每个游戏循环内做 N+1 查询。
- 首页沿用已有六张统计卡，增加四态计数明细；Dashboard 游戏选择器、选中游戏头部和 Save Center 校验区复用现有布局显示四态与理由 Tooltip。`Risk` 使用错误色、`Attention` 使用警告色、`Unknown` 使用中性色；未新增页面、命令、导航或扫描体系。
- Snapshot comparer 纳入健康摘要与理由列表，健康理由变化时才触发既有集合更新。

**为什么这样做：**

健康状态必须回答“最近是否玩过、是否真的成功备份、最新恢复点是否可用、是否有失败/云端异常”，不能继续只用“匹配成功/有 finding”做粗粒度投影。证据先由 SQLite 聚合提供，再由 Core 纯计算判定，便于测试、解释和后续策略阶段复用。

**修改文件：**

- `src/GameSaveCenter.Contracts/Enums.cs`
- `src/GameSaveCenter.Contracts/GameDtos.cs`
- `src/GameSaveCenter.Contracts/DashboardDtos.cs`
- `src/GameSaveCenter.Core/Services/GameHealthAssessmentService.cs`
- `src/GameSaveCenter.Worker/Persistence/SqliteStateStore.cs`
- `src/GameSaveCenter.Worker/Services/DashboardService.cs`
- `src/GameSaveCenter.Playnite/Infrastructure/SnapshotComparers.cs`
- `src/GameSaveCenter.Playnite/ViewModels/GamePickerItem.cs`
- `src/GameSaveCenter.Playnite/ViewModels/DashboardViewModel.cs`
- `src/GameSaveCenter.Playnite/Views/DashboardView.xaml`
- `src/GameSaveCenter.Playnite/Views/OverviewView.xaml`
- `src/GameSaveCenter.Playnite/Views/SaveCenterView.xaml`
- `tests/GameSaveCenter.Core.Tests/GameHealthAssessmentTests.cs`
- `tests/GameSaveCenter.Worker.Tests/DashboardHealthPersistenceTests.cs`
- `tests/GameSaveCenter.Playnite.Tests/WpfUiResourceDictionaryTests.cs`

**测试结果：**

- Core 19/19、Worker 59/59、Playnite 197/197 通过；Worker/Playnite 使用隔离输出验证，绕开本机旧 net8/net472 测试输出文件锁。
- Worker 与 Playnite Release 构建 0 警告/0 错误；`git diff --check`、`scripts/validate-source.py`、`scripts/check-xaml.ps1` 通过。
- WPF 静态门禁 0 errors（仓库既有 warnings）；`scripts/render-qa.ps1` 全绿，覆盖 1040×700、1280×720、1366×768 及既有响应式滚动探针。
- 未运行真实 Playnite 宿主内的主题切换、DPI、键盘和连续缩放人工验收；仍标记为 `BLOCKED_ENVIRONMENT`。

**下一步：**

`PROTECTION-001`：备份保护状态与策略联动；继续小阶段、独立测试、记忆文档、commit 和 push。

## 2026-08-12 UI-207-FOLLOWUP 设置滚动所有权与当前游戏上下文收口

**做了什么：**

- 设置页把页面级滚动从 UserControl 根节点收回到共享 `GscRedesignSettingsTabControl` 模板：宽屏分类栏固定 232 DIP、有限高度下垂直 `Auto`，紧凑布局切换为顶部水平 `Auto`；选中项仍自动 `BringIntoView()`，内容区拥有独立 `SettingsScroller`，5 个分类和所有表单字段均可达。
- 新增共享 `GscSelectedGameIconControl`，当前真实 Playnite Icon 只在 Dashboard 顶部/选中头部、Overview、Save、Trainer、Media 的当前游戏上下文表面加载；GamePicker 虚拟化列表保持 initials，Provider 仍为本地优先、48 DIP 解码、OnLoad/Freeze、LRU 48、失败 glyph fallback、无网络。
- GamePicker 默认“已安装”与已有筛选/排序/搜索保持不变；当前选择即使被筛选隐藏也不被静默替换，显示恢复提示并提供“清除搜索 / 显示当前游戏”命令。事件驱动的 `PlayniteGameStarted` 自动定位、持久化选择和停止后保持选择逻辑不变。
- 没有修改任何 DataGrid、命令、Binding、Worker、IPC 或 Playnite 业务数据流；性能基准改为写入临时/显式 `GSC_TEST_ARTIFACT_ROOT`，避免污染测试 bin 输出树。

**测试结果：**

- `python scripts/validate-source.py` 通过；`python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .` 0 errors（仓库既有 33 warnings）。
- Playnite Release 构建隔离输出 0 警告/0 错误；Core 13/13、Worker 51/51、Playnite 197/197 通过；`git diff --check` 通过。
- `scripts/render-qa.ps1 -Configuration Release` 全绿；Settings 760/880/920/1100/1400 × 560/700/900 共 15 组探针，5 个分类可见，宽屏导航最小宽度 232 DIP，内容滚动超限时 `Auto` 可滚动。
- 未运行真实 Playnite 宿主内的主题切换、DPI、键盘和连续缩放人工验收；这些仍标记为 `BLOCKED_ENVIRONMENT`。

## 2026-08-12 UI-207 设置布局响应式与当前游戏上下文

**做了什么：**

- Settings 设置页：`SettingsHeader` 显式 `ClipToBounds=False`；宽/窄布局断点统一为 920 DIP；宽布局 5 个分类 `MinHeight=72`、窄布局 `MinHeight=44`；`GscRedesignSettingsTabControl` 分类栏改为 ScrollViewer（宽时垂直 Auto、窄时水平 Auto），选中 Tab 自动 `BringIntoView()`，5 个分类在常用尺寸下完整可达；设置项与保存行为不变。
- 当前游戏自动定位：新增 `GameSelectionResolver`，打开 Dashboard 时按“运行中优先（持久化 id → 最近 GameStarted → 最近活动）→ 上次选择 → 首个已安装 → 空库 null”选择；`GameSaveCenterPlugin` 新增 `PlayniteGameStarted` 事件，`DashboardViewModel` 只在首次打开或新 GameStarted 时切换，普通刷新不抢回用户手动选择；游戏停止后保持当前选择并继续复用 `GamePickerSelectedGameId` 持久化。
- GamePicker 新用户默认筛选改为“已安装”，空值/未知值归一到“已安装”，已有明确配置值保留。
- 当前选中游戏显示真实 Playnite Icon：新增 UI-only `PlayniteGameIconProvider`（48 max decode、OnLoad、Freeze、LRU 48、远程/缺失/异常 fallback 到手柄 glyph），Dashboard 顶部选择器、展开头部与 Overview 当前游戏卡复用同一 `SelectedGameIcon`；GamePicker 列表不加载真实图标。
- 不新增 Timer/进程扫描/IPC/网络 IO；未触碰 DataGrid 滚动相关代码。

**测试结果：**

- `python scripts/validate-source.py`、`git diff --check`、`git fsck --full` 通过。
- Core 13/13、Worker 51/51、Playnite 195/195 通过；解决方案 Release 构建 0 警告/0 错误。
- render-qa 全绿，含 15 组 SettingsLayout 探针（760/880/920/1100/1400 × 560/700/900 DIP，5 个分类全部可见可测）。
- `scripts/dev-install-run.ps1 -Configuration Release -NoStart` 一键构建安装成功，安装到 `%APPDATA%\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`（0.6.70）。
- 真实 Playnite 冒烟：启动后 GameSaveCenter 0.6.70.0 正常加载，无异常日志；设置页视觉、自动定位场景、真实 Icon、1080p/4K 与 DPI 人工验收标记 `BLOCKED_ENVIRONMENT`。

## 2026-08-11 DEV-AUDIT-001 重新审计当前仓库

**做了什么：**

- 重新阅读交接文档、设计门禁、AGENTS.md、最新 Git 历史，并核对当前源码。
- 建立 `docs/ai/` 长期记忆机制（本文件 + `PROJECT_MEMORY.md`），供后续任意 Codex 会话快速恢复上下文。
- 确认当前分支 `main`、版本 `0.6.70-development-preview`、工作区仅有未跟踪的 `GameSaveCenter.7z`（用户生成，不提交）。
- 核对六个 Workspace、Settings、GamePicker、DataGrid/ScrollViewer 分工、Theme、离屏 QA、UI-201～205 与 PERF-001～004（旧编号）实际实现。
- 确认 `GameToolType.CustomExecutable` 与 GameTool DTO 字段已存在，但 Worker 导入/启动仍只完整支持 Trainer/CT：自定义启动项尚未正式实现。
- 确认 `MediaThumbnailConverter` 仍是 UI 线程同步解码；Task/Media 搜索仍每次按键刷新；Snapshot 未变化时多数集合仍会 Reset。

**为什么这样做：**

目标文件要求先审计再开发，禁止根据历史聊天或旧 ZIP 猜测当前实现；同时为多端持续开发建立本地长期记忆。

**修改文件：**

- 新增 `docs/ai/PROJECT_MEMORY.md`
- 新增 `docs/ai/WORKLOG.md`

**测试结果：**

文档阶段无需编译；源码未改动。

**仍需验证内容：**

真实 Playnite 宿主、主题/DPI 真机与连续缩放流畅性仍待 UI-QA-REAL-001。

**下一步：**

NEXT: PERF-004 性能基线设施（`[PERF]` 日志 + `docs/ai/PERFORMANCE_BASELINE.md`）。

## 2026-08-11 PERF-004 性能基线设施

**做了什么：**

- 新增统一 `[PERF]` Debug 日志：Worker 快照生成（fetch）、Playnite 快照应用（apply）、Workspace 切换布局、GamePicker setItems/refresh、Task/Media 搜索刷新、Media 详情加载、缩略图解码。
- 新增 `docs/ai/PERFORMANCE_BASELINE.md`，记录测量清单、离屏 render-qa 基线数字与待真机验证项。
- Worker `DashboardService` 注入 `ILogger<DashboardService>`，只输出 Debug，Release 正常运行不刷屏。

**为什么这样做：**

性能优化前先建立可重复的测量设施，避免“凭感觉优化”；日志带数据量便于对照大库规模。

**修改文件：**

- `src/GameSaveCenter.Worker/Services/DashboardService.cs`
- `src/GameSaveCenter.Playnite/ViewModels/DashboardViewModel.cs`
- `src/GameSaveCenter.Playnite/ViewModels/GamePickerViewModel.cs`
- `src/GameSaveCenter.Playnite/Views/DashboardView.xaml.cs`
- `src/GameSaveCenter.Playnite/Converters/MediaThumbnailConverter.cs`
- 新增 `docs/ai/PERFORMANCE_BASELINE.md`

**测试结果：**

- Release 编译通过（Contracts/Core/Worker/Playnite 四个生产项目；测试输出目录有外部残留进程锁，改用隔离 `--output` 目录跑测试）。
- Core 13/13、Worker 23/23、Playnite 152/152 通过。

**性能变化：**

- 离屏渲染基线已记录（1040×700 到 1920×1080 的 render_ms），见 `docs/ai/PERFORMANCE_BASELINE.md`。

**仍需验证内容：**

真实 Playnite 宿主下 `[PERF]` 日志的实际耗时、1000+ 游戏库搜索/切页、缩略图滚动。

**下一步：**

NEXT: PERF-005 Snapshot 无变化 0 Reset。

## 2026-08-11 PERF-005 Snapshot 无变化 0 Reset

**做了什么：**

- `BatchObservableCollection.ReplaceAll` 支持内容比较器（`Func<T,T,bool>`）并返回是否实际替换；内容未变化时不再发任何 `CollectionChanged`（从“1 次 Reset”降到“0 次”）。
- 新增 `SnapshotComparers`：为 Games/Tasks/Findings/Audit/Backups/SaveCandidates/Media/MediaSources/GameTools/ProcessMappings/DeviceComparisons 提供覆盖 UI 字段的内容比较，Task 覆盖 Progress/State/Message/Error 等高频变化字段。
- `DashboardViewModel.Replace` 透传比较器；Media 只在集合变化时才 `MediaView.Refresh()`；GamePicker `SetItems` 在内容未变化时跳过 Clear/Add/Refresh，避免整表重建。

**为什么这样做：**

Snapshot 每次返回全新 DTO，即使内容相同，旧的引用比较也会触发 Reset 并让 WPF 重建 CollectionView/ItemContainer；这是大库高频刷新的主要浪费。

**修改文件：**

- 新增 `src/GameSaveCenter.Playnite/Infrastructure/SnapshotComparers.cs`
- `src/GameSaveCenter.Playnite/Infrastructure/BatchObservableCollection.cs`
- `src/GameSaveCenter.Playnite/ViewModels/DashboardViewModel.cs`
- `src/GameSaveCenter.Playnite/ViewModels/GamePickerViewModel.cs`
- 新增测试 `tests/GameSaveCenter.Playnite.Tests/BatchObservableCollectionTests.cs`
- `tests/GameSaveCenter.Playnite.Tests/GamePickerViewModelTests.cs`

**测试结果：**

- Core 13/13、Worker 23/23、Playnite 156/156（新增 4 个 0 通知/变化检测测试）通过。
- `scripts/render-qa.ps1` 全绿（7 页面 × 5 尺寸，无 `PROBLEM`）。

**性能变化：**

- 内容相同的 Snapshot：Games/Tasks/Findings/Media/GameTools 等集合 0 次 CollectionChanged（测试覆盖）。
- GamePicker：相同内容跳过重建，不再每次快照发 Reset。

**仍需验证内容：**

真实 Playnite 大库下高频刷新/任务进度事件的实际 UI 卡顿改善。

**下一步：**

NEXT: PERF-006 Task/Media 搜索防抖。

## 2026-08-11 PERF-006 Task/Media 搜索防抖

**做了什么：**

- 新增轻量 `DebouncedRefresh`（180ms）：快速连续输入只保留最后一次 Refresh；清空搜索框立即刷新；`Cancel()` 在页面/ViewModel 卸载时取消，避免 Timer 泄漏。
- `TaskSearchText` / `MediaSearchText` 改用防抖；原来的 `[PERF] TaskSearch/MediaSearch refresh` 日志保留在最终刷新动作中。
- 新增 4 个防抖单元测试：空查询立即刷新、快速输入只刷一次、取消阻止待执行刷新、清空会取消待执行刷新并立即刷新。

**为什么这样做：**

每次按键都执行 `ICollectionView.Refresh()` 会让大列表在输入过程中反复重新过滤/布局；防抖后连续输入 `abcdef` 只执行约 1 次最终 Refresh。

**修改文件：**

- 新增 `src/GameSaveCenter.Playnite/Infrastructure/DebouncedRefresh.cs`
- `src/GameSaveCenter.Playnite/ViewModels/DashboardViewModel.cs`
- 新增测试 `tests/GameSaveCenter.Playnite.Tests/DebouncedRefreshTests.cs`

**测试结果：**

- Core 13/13、Worker 23/23、Playnite 160/160 通过。

**性能变化：**

- 连续输入场景：Task/Media 搜索从 N 次 Refresh 降为约 1 次（测试覆盖）。

**仍需验证内容：**

真实 Playnite 大列表输入体验与滚动流畅性。

**下一步：**

NEXT: GAME-TOOL-001 自定义游戏启动项（EXE/LNK/BAT/CMD/PS1）。

## 2026-08-11 GAME-TOOL-001 自定义游戏启动项基础能力

**做了什么：**

- 新增 `GameToolLaunchKind` 与 `GameToolLaunchKinds`：按扩展名分类 EXE / LNK / BAT / CMD / PS1 / 系统默认程序，并明确只有 EXE（含可解析快捷方式目标）可安全跟踪进程。
- 新增 `GameToolLauncher`：EXE 支持 Arguments / WorkingDirectory / RunAsAdministrator；LNK 通过 WScript.Shell 解析 TargetPath/Arguments/WorkingDirectory 后优先运行真实目标；BAT/CMD 通过 `cmd.exe /d /s /c`，PS1 通过 `powershell.exe -NoProfile -ExecutionPolicy Bypass -File`；其他文件用 `UseShellExecute` 交给系统默认程序。
- 修改器中心新增“+ 添加启动项”入口，文件选择器覆盖 EXE/LNK/脚本/所有文件；导入默认 `CopyIntoLibrary=false`，保留外部路径引用，不复制文件。
- 工具卡片类型显示改为“自定义启动项”，Inspector 显示外部引用提示；`CloseOnGameExit` 对不可跟踪类型禁用。

**为什么这样做：**

复用现有 GameTool 模型与字段，不新建第二套数据库；不同文件类型必须采用不同启动策略，不能统一 `Process.Start(path)`。

**修改文件：**

- `src/GameSaveCenter.Contracts/Enums.cs`
- `src/GameSaveCenter.Contracts/TrainerDtos.cs`
- 新增 `src/GameSaveCenter.Worker/Services/GameToolLauncher.cs`
- `src/GameSaveCenter.Playnite/ViewModels/DashboardViewModel.cs`
- `src/GameSaveCenter.Playnite/Views/TrainerCenterView.xaml`
- 新增测试 `GameToolLaunchKindsTests.cs`、`GameToolLauncherTests.cs`

**测试结果：**

- 启动策略单测覆盖 EXE/LNK→EXE/LNK→文档/BAT/CMD/PS1/普通文件/缺失文件/管理员参数。

## 2026-08-11 GAME-TOOL-002 自定义启动项生命周期与安全关闭

**做了什么：**

- 新增 `GameToolSessionTracker`：按 SessionId 记录 GameSaveCenter 自己启动的 PID + 启动时间 + CloseOnExit；游戏退出时只关闭本 Session 且 CloseOnExit 的进程，不按进程名杀、不跨 Session。
- `GameToolService` 自动启动流程接入 tracker：延迟取消、启动记录、退出关闭；只有可跟踪类型且用户开启时才实际关闭。
- 自定义导入支持单文件/任意类型并保持外部路径；`SaveSelectedGameToolAsync` 保存时自动纠正“不可跟踪类型开启退出后关闭”与“系统默认程序开启管理员”。

**为什么这样做：**

CloseOnGameExit 的安全边界必须按 PID + StartTime + Session 确认，避免误杀用户游戏前已打开的同名工具。

**修改文件：**

- 新增 `src/GameSaveCenter.Worker/Services/GameToolSessionTracker.cs`
- `src/GameSaveCenter.Worker/Services/GameToolService.cs`
- 新增测试 `GameToolServiceImportTests.cs`、`GameToolSessionTrackerTests.cs`

**测试结果：**

- Worker 45/45、Playnite 160/160、Core 13/13 通过；render-qa 全绿；技能静态审查 0 error。

**仍需验证内容：**

真实 Playnite 中导入 LNK/BAT/PS1、自动随游戏启动、延迟与退出关闭的真机流程。

**下一步：**

NEXT: PERF-007 媒体缩略图异步化。

## 2026-08-11 PERF-007 媒体缩略图异步化

**做了什么：**

- 新增 `AsyncThumbnailLoader`：File IO + BitmapImage 解码全部移到后台线程，最多 3 个并发，解码后 Freeze，LRU 缓存 96 项，缓存 key 覆盖路径/宽度/文件长度/最后修改时间。
- 新增 `AsyncThumbnailImage` 控件：先显示空占位，后台解码完成后回 UI 只更新对应 item；路径被替换或控件卸载时取消过期加载。
- 媒体列表缩略图（96px）与选中媒体大图预览（480px）都改用异步控件；原 `MediaThumbnailConverter` 保留兼容。
- `scripts/validate-source.py` 门禁同步支持 `PreviewWidth="96"` 异步缩略图写法，并校验异步加载器存在。

**为什么这样做：**

原绑定转换器在 UI 线程同步做 File.Exists/FileInfo/FileStream/Decode，大量截图滚动时会卡 UI；异步化后滚动只触发可见项加载，且并发有界。

**修改文件：**

- 新增 `src/GameSaveCenter.Playnite/Converters/AsyncThumbnailLoader.cs`
- 新增 `src/GameSaveCenter.Playnite/Controls/AsyncThumbnailImage.cs`
- `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml`
- `scripts/validate-source.py`
- 新增测试 `tests/GameSaveCenter.Playnite.Tests/AsyncThumbnailLoaderTests.cs`

**测试结果：**

- Playnite 163/163（新增 3 个异步加载测试）通过；render-qa 全绿；技能静态审查 0 error；`scripts/validate-source.py` 通过。

**性能变化：**

- 缩略图解码不再占用 UI 线程；并发限制 3，避免 200 张图同时 200 个 Task。

**仍需验证内容：**

真实 Playnite 下大量截图滚动的帧率与解码耗时（`[PERF]` 日志 + UI-QA-REAL-001）。

**下一步：**

NEXT: UI-QA-REAL-001 真机回归与最终收尾。

## 2026-08-12 UI-QA-REAL-001 真机冒烟与最终收尾

**做了什么：**

- 一键构建安装最新扩展（Release 0 警告/0 错误，Core 13、Worker 45、Playnite 163 全过，extension.yaml/DLL 0.6.70）。
- 启动隔离 Playnite 真机冒烟：插件成功加载，Worker 0.6.70.0 自动启动并完成存储初始化，Playnite 进程稳定运行，无新增错误。
- 保存真实宿主截图：`artifacts/ui-qa/real/playnite-real.png`（本会话无法直接预览，已留给用户复核）。
- 离屏 render-qa（7 页面 × 5 常用窗口）与技能静态审查保持全绿；TaskCenter 状态/游戏/类型筛选在 harness 中均显示“全部”默认值。

**为什么这样做：**

真机回归用于确认本轮性能与自定义启动项改动没有破坏 Playnite 宿主加载链路；无法自动判断的画面细节（主题/DPI/键盘/缩放流畅性）写入待验证清单。

**测试结果：**

- 一键构建安装成功；Playnite 真机启动、扩展加载、Worker IPC 自动启动成功。

**仍需人工验证：**

- Playnite 内 Light/Dark/Follow/高对比度主题。
- 100%/125%/150%/175%/200% DPI 与窗口化/最大化/连续缩放。
- 首页/存档/修改器/媒体/任务/维护/设置各页首屏与底部滚动。
- 自定义启动项导入 LNK/BAT/CMD/PS1、随游戏自动启动、延迟与退出关闭真机流程。
- 大量截图滚动的帧率与缩略图异步加载效果。

**下一步：**

NEXT: 按真实 Playnite 使用反馈决定 PERF-008/009/010 是否值得继续；当前无真实 profiling 证据，不为了编号做无收益优化。

## 2026-08-12 PERF-009 / PERF-010 代码级热点优化

**做了什么：**

- PERF-009：新增 `TaskIndexedCollection`，用 TaskId→index 字典替代每次进度事件 `Tasks.ToList().FindIndex(...)` 的 O(n) 拷贝扫描；快照替换后重建索引，事件更新 O(1)，列表仍保持 200 行上限。
- PERF-010：`RaiseCommandStates()` 改为 Dispatcher 合帧刷新（DataBind 优先级），同一 UI 帧内多次属性变化只执行一次全部命令的 `RaiseCanExecuteChanged`；Dispatcher 关闭时回退为立即刷新。

**为什么这样做：**

任务进度是高频更新路径（每次 Progress/Message/State 变化都会到达 Dashboard），列表拷贝 + 线性查找在 200 行规模下也是每次事件的浪费；命令状态在一次业务操作里可能被触发数十次，合并后不会影响 CanExecute 正确性（WPF 在命令执行时仍会查询）。

**修改文件：**

- 新增 `src/GameSaveCenter.Playnite/Infrastructure/TaskIndexedCollection.cs`
- `src/GameSaveCenter.Playnite/ViewModels/DashboardViewModel.cs`
- 新增测试 `tests/GameSaveCenter.Playnite.Tests/TaskIndexedCollectionTests.cs`

**测试结果：**

- Playnite 167/167（新增 4 个任务索引测试）通过；Release 编译通过。

**性能变化：**

- 任务事件合并：O(n) 拷贝+查找 → O(1) 索引更新。
- 命令状态刷新：一次业务操作内 N 次触发 → 约 1 次合帧刷新。

**仍需验证内容：**

真实 Playnite 下高频任务进度事件的 UI 帧率。

**下一步：**

NEXT: 仅剩 PERF-008（按 Workspace 按需刷新）未实施；目标文件要求先有真实 profiling 证据，当前保留为待定，不做无收益重构。

## 2026-08-12 大库 2000 规模回归测试

**做了什么：**

- 新增 `LargeLibraryPerformanceTests`：2000 游戏相同 Snapshot 时 GamePicker 0 次二次集合通知；单游戏状态变化时只发 1 次 Reset 且无逐项 Add；2000 任务相同 Snapshot 时 0 次二次 CollectionChanged。

**为什么这样做：**

目标文件的性能验收要求 1000+ 游戏不冻结，并明确要求用 Unit Test / Instrumentation 证明“相同 Snapshot 从 1 次 Reset 降到 0 次”；补上 2000 规模证据，避免只在小样本上证明。

**修改文件：**

- 新增 `tests/GameSaveCenter.Playnite.Tests/LargeLibraryPerformanceTests.cs`

**测试结果：**

- Playnite 170/170（新增 3 个大库规模测试）通过。

**仍需验证内容：**

真实 Playnite 1000+ 游戏库的 IPC 与渲染帧率。

**下一步：**

NEXT: PERF-008 仍待真实 profiling；当前按 Workspace 按需刷新已有基础（详情只在激活 Workspace 加载），暂不追加重构。

## 2026-08-12 2000 规模合成 profiling 基准

**做了什么：**

- `LargeLibraryPerformanceTests` 增加合成 profiling：测量 2000 游戏 GamePicker 首次/未变化/单变化 SetItems、搜索输入到防抖刷新完成，以及 2000 任务首次/未变化 ReplaceAll；结果写入 `artifacts/ui-qa/benchmarks/large-library.txt`。

**实测结果（本机）：**

- 首次 SetItems 27ms；未变化 0ms；单游戏变化 25ms；搜索端到端 402ms（含 180ms 防抖）；任务 ReplaceAll 均 <1ms。

**结论：**

- 大库集合路径没有明显 O(n^2)；PERF-008 不因 GamePicker/任务集合压力而紧迫，保留按真实 Playnite 渲染 profiling 再评估。

**修改文件：**

- `tests/GameSaveCenter.Playnite.Tests/LargeLibraryPerformanceTests.cs`
- `docs/ai/PERFORMANCE_BASELINE.md`

**测试结果：**

- Playnite 171/171 通过。

**仍需验证内容：**

真实 Playnite 1000+ 游戏库的 IPC/渲染帧率。

**下一步：**

NEXT: 待用户提供真实 Playnite 大库反馈或人工验收结果。

## 2026-08-12 PERF-008 评估收口

**做了什么：**

- 基于 2000 规模合成 profiling 与源码审计，正式评估 PERF-008（按 Workspace 按需刷新）：
  - 详情数据已经在 `RefreshCoreAsync` / `RequestWorkspaceLoad` / `LoadDetailsAsync` 中按当前 Workspace 分支加载（Saves/Trainers/Media 只在激活时请求，Tasks/Maintenance 只加载各自独立数据）。
  - 全量 `DashboardSnapshot` 仅用于全局摘要与任务轮询，后台轮询已有 1 分钟全量 TTL，任务变化走 `GetTaskChanges` 增量。
  - 2000 规模合成 profiling 显示 GamePicker/Tasks 集合路径无 O(n^2)（首次 27ms、未变化 0ms、任务 ReplaceAll <1ms）。

**结论：**

PERF-008 维持现状，不追加按 Workspace 重构；等真实 Playnite 大库渲染 profiling 显示全量快照成为瓶颈时再单独评估。

**修改文件：**

- `docs/ai/WORKLOG.md`
- `docs/ai/PROJECT_MEMORY.md`

**仍需验证内容：**

真实 Playnite 大库 IPC 与渲染帧率（外部环境）。

**下一步：**

NEXT: 全部可自主完成项已收口；剩余为真实 Playnite 人工验收。

## 2026-08-12 验收缺口修复（评审反馈）

按评审意见修复以下语义问题，全部先改实现再跑测试：

### PID 身份安全

- 启动后立即记录 `Process.StartTime`（真实值），不再用 `DateTime.UtcNow` 近似。
- 关闭时要求 PID 相同且实际 StartTime 与记录值在 5 秒容差内双向匹配（`ProcessIdentityGuard`），避免 PID 被复用后误杀新进程。
- 新增真实双进程测试：Session A 关闭、Session B 存活；新增 PID 复用拒绝测试。

### 缩略图真正后台化

- `AsyncThumbnailLoader.LoadAsync` 用 `Task.Run` 强制 File.Exists/FileInfo/FileStream/Decode 全部离开调用线程，即使 Semaphore 立即放行也不会在 UI 线程同步执行。
- `AsyncThumbnailImage` 注册 `Unloaded` 取消待处理加载，`Loaded` 时若占位为空自动重新加载。
- 主路径 `AsyncThumbnailLoader.Decode` 增加 `[PERF] Thumbnail decode` 埋点。

### 自定义启动项管理

- Inspector 新增名称、工作目录、启动参数编辑框；保存时把 `DisplayName/WorkingDirectory/Arguments` 一起提交 Worker。
- 新增“重新定位”命令（仅外部启动项显示），Worker 新增 `tools.relocate` 消息与存储更新。
- LNK 导入/重定位时解析并持久化 `ResolvedTargetPath`；UI 的 `CanTrackProcess/LaunchKindDisplay` 改用解析后目标，LNK→EXE 不再被 UI 误判为不可追踪。
- `game_tool_versions` 增加兼容列 `resolved_target_path`。

### Benchmark 与记忆清理

- 搜索 benchmark 改为轮询 `FilteredCount` 等待真实刷新完成，不再固定睡 400ms；新增清空搜索测量。
- 修正 `PROJECT_MEMORY.md` 里 PERF-005/006 的过期技术债、测试数量与优先级描述。

**测试结果：**

- Worker 49/49、Playnite 171/171 通过；新 benchmark：搜索到刷新完成 208ms、清空 199ms。

**修改文件：**

- `GameToolSessionTracker.cs`、`GameToolService.cs`
- `AsyncThumbnailLoader.cs`、`AsyncThumbnailImage.cs`
- Contracts/Store/Dispatcher/`DashboardViewModel.cs`/`TrainerCenterView.xaml`
- `GameToolSessionTrackerTests.cs`、`GameToolServiceImportTests.cs`、`LargeLibraryPerformanceTests.cs`
- `docs/ai/PROJECT_MEMORY.md`、`docs/ai/PERFORMANCE_BASELINE.md`

**仍需验证内容：**

真实 Playnite 人工验收（主题/DPI/窗口化/自定义启动项真机流程/大库渲染帧率）。

## 2026-08-12 UI-206 DataGrid 滚动几何修复（SUPERSEDED）

> 历史记录保留。该条目的 Pixel 方案已由真实 Playnite A/B 验证为严重回归并撤回；当前最终方案是 `Item` + `GscStableDataGridRow` + geometry probe，见后续条目与 PROJECT_MEMORY。

**做了什么：**

- 共享 `DataGrid` Style 与 `SaveDataGrid` / `TaskDataGrid` / `MaintenanceDataGrid` 显式设置 `VirtualizingPanel.ScrollUnit=Pixel`，保留 `CanContentScroll=True`、虚拟化与 Recycling，不关闭任何性能机制。
- Maintenance/Task 响应式代码不再给 DataGrid 写死运行时 `Height`，改为 `Height=double.NaN` + 有限 `MaxHeight`（保留 MinHeight）。
- 完整诊断摘要：移除外层 `ClipToBounds=True`，删除 code-behind 对外层 Border 的 MinHeight/MaxHeight 动态限制；由页面滚动面负责短窗口可达性，TextBox 保留自身 160 DIP 上限与内部滚动。
- 新增 offscreen 滚动回归探针：`SaveHistoryGrid` / `TaskGrid` / `FindingsGrid` / `MaintenanceAuditFindingsGrid` / `MaintenanceAuditLogGrid`，60 行假数据 × 287/311/337/353/419 DIP 非整行高度，滚到底后断言 `VerticalOffset ≈ ScrollableHeight`，不越界。
- 更新 `WpfUiResourceDictionaryTests`：断言 Pixel ScrollUnit、MaxHeight + Height=NaN、诊断摘要不再被外层裁剪。

**为什么这样做：**

真实 Playnite 2K 窗口化下，DataGrid 按 Item 逻辑滚动与固定 Height 在非整行/高 DPI viewport 上产生“表头大空白 + 末行只露一点”的滚动几何错误；Pixel 滚动 + 有限 MaxHeight 让滚到底的偏移与 ScrollableHeight 一致。

**修改文件：**

- `Themes/WpfUiProduction.xaml`
- `Views/SaveCenterView.xaml`、`Views/TaskCenterView.xaml`、`Views/MaintenanceView.xaml`
- `Views/MaintenanceView.xaml.cs`、`Views/TaskCenterView.xaml.cs`
- `tests/GameSaveCenter.Playnite.Tests/WpfUiResourceDictionaryTests.cs`
- `tests/GameSaveCenter.RenderHarness/Program.cs`、`tests/GameSaveCenter.RenderHarness/FakeDashboardData.cs`

**测试结果：**

- Playnite 172/172、render-qa 全绿（含新增滚动探针，offset==scrollable）、源码验证与技能静态审查通过。

**仍需验证内容：**

真实 Playnite 2K 窗口化/最大化的视觉确认；1080p/4K 环境标记 BLOCKED_ENVIRONMENT（本机暂无法验证）。

## 2026-08-12 QA-HARDENING 收口清理

**做了什么：**

- 修复 `RelocateAsync` WorkingDirectory 语义：WorkingDirectory 如果只是导入时自动等于旧 EntryPath 父目录，重新定位时跟随新文件目录；用户显式自定义的目录保留；旧目录已不存在时回退新文件目录。
- 自定义外部文件导入成功后清理无意义空 GameTools 目录。
- `LargeLibraryPerformanceTests` 等待 helper 超时后现在直接 `Assert.Equal(expected, FilteredCount)`，搜索坏了会测试失败，不再把 5 秒当成“耗时”放行。
- GitHub Actions Windows CI 新增 `scripts/render-qa.ps1` 与 WPF UI validator 步骤，离屏几何 QA 真正进入 CI 门禁。
- 同步 `PROJECT_MEMORY / WORKLOG / README` 到最终状态；删除“DataGrid 必须 Pixel”的过期结论，明确记录 Pixel 经真机验证会回归，当前采用 `Item + GscStableDataGridRow + geometry probe`。
- 测试方法改名：`DataGridsUsePixelScrollUnit...` → `DataGridsUseItemScrollUnitAndStableRowWithoutDiagnosticClip`。

**为什么这样做：**

收口评审指出长期记忆与源码已经漂移（Pixel/Item 相反），继续让未来 Codex 接力会把刚修好的 UI bug 改回去；同时把最有价值的离屏几何 QA 放进 CI，避免本地忘记跑。

**修改文件：**

- `src/GameSaveCenter.Worker/Services/GameToolService.cs`
- `tests/GameSaveCenter.Playnite.Tests/LargeLibraryPerformanceTests.cs`
- `tests/GameSaveCenter.Playnite.Tests/WpfUiResourceDictionaryTests.cs`
- `.github/workflows/windows-build.yml`
- `README.md`、`docs/ai/PROJECT_MEMORY.md`、`docs/ai/WORKLOG.md`

**测试结果：**

- 本地 Playnite 179/179、render-qa 全绿；源码验证与技能静态审查通过。

**最终结论：**

PERF-004～010 与 GAME-TOOL-001/002 主体完成；最近 DataGrid/UI 问题在源码层已经经历回滚与再修复，当前方案为 Item + 稳定行样式 + geometry probe。剩余完整主题/DPI/大库/启动项真机验收仍需用户真实使用反馈，不宣称已完成。

## 2026-08-12 QA-HARDENING-2 文档与边缘清理

**做了什么：**

- `PROJECT_MEMORY.md` 测试基线更新为 Worker 51；TaskFilter 描述改为 `TaskFilterOptionsSync`（不再提 200ms timer）；LNK 可追踪描述与“脚本/普通文件不可靠”分开写清。
- `DEVELOPMENT_HANDOFF.md` 顶部当前基线更新到 `0c6f143`，测试基线更新为 Core 13/Worker 51/Playnite 179；删除“下一步 PERF-008/009/010”过期指示。
- WORKLOG 旧 UI-206 条目标注 SUPERSEDED，避免未来 Codex 误以为 Pixel 是当前方案。
- 外部 CustomExecutable 导入不再预先创建 GameTools root（只有 ZIP/目录/复制需要存储时才创建），失败路径也不会留下空目录；补充“导入失败不留下空目录”测试。

**修改文件：**

- `src/GameSaveCenter.Worker/Services/GameToolService.cs`
- `tests/GameSaveCenter.Worker.Tests/GameToolServiceImportTests.cs`
- `docs/ai/PROJECT_MEMORY.md`、`docs/ai/WORKLOG.md`、`docs/DEVELOPMENT_HANDOFF.md`

**测试结果：**

- Worker 51/51、Playnite 179/179；源码验证通过；远端 `main` 已推送（`bc83562`）。

## 2026-08-12 RELIABILITY-RESTORE-001 恢复可用性验证

**做了什么：**

- 为备份版本增加恢复可用性状态、检查时间、归档可读性、隔离提取结果、条目/大小统计、哈希校验摘要与错误/警告计数。
- 从 Ludusavi `backups --api` 的 `backupPath` 与版本 ID 持久化归档路径；恢复检查只读取该版本 ZIP，不扫描真实存档目录。
- 新增 Worker IPC `restore.readiness.validate`，在应用数据目录下创建唯一隔离目录，完成后清理；拒绝路径穿越、超大条目/总展开体积和超大条目数量。
- 支持有效、损坏、缺失、统计不一致、空文件、路径穿越与不支持格式等结果；预留清单 SHA-256 校验，并将不支持的 ZIP 压缩方式显示为 `Unsupported`。
- 在既有存档历史 Inspector 内增加“恢复可用性”摘要与“验证可恢复性”按钮；没有新页面、没有改变历史表格的尺寸/滚动/虚拟化结构。
- 修复 `artifacts` 隔离输出目录被 SDK 默认 glob 编译导致的重复程序集属性问题。

**为什么这样做：**

恢复前需要验证“选定历史版本本身可读、可提取”，而不是只验证恢复后的 live save。隔离提取避免验证过程写入真实存档路径，持久化结果则让用户能看到上次检查证据并在需要时重新检查。

**修改文件：**

- Contracts：`DashboardDtos.cs`、`Enums.cs`、`MessageTypes.cs`、`OperationDtos.cs`
- Worker：`RestoreReadinessService.cs`、`LudusaviResultParser.cs`、`SqliteStateStore.cs`、`IpcRequestDispatcher.cs`、`Program.cs`
- Playnite：`DashboardViewModel.cs`、`SaveCenterView.xaml`、`SnapshotComparers.cs`
- Tests：`RestoreReadinessTests.cs`
- Build：`Directory.Build.props`
- 文档：`PROJECT_MEMORY.md`、`WORKLOG.md`

**测试结果：**

- Worker 58/58、Core 13/13、Playnite 197/197 通过。
- Worker 与 Playnite Release 构建均为 0 警告/0 错误。
- `validate-source.py`、`check-xaml.ps1`、WPF UI 静态校验、`render-qa.ps1` 全部通过；多窗口渲染中历史表格几何保持不变，新增 Inspector 内容在既有滚动区域内。
- 真实 Playnite 宿主、主题切换与多 DPI 的人工验收仍需用户环境复核；本阶段未宣称已完成该人工验收。

**提交：**


- `RELIABILITY-RESTORE-001`（本阶段独立提交）

**下一步：**

- 按用户附件继续 `RELIABILITY-HEALTH-001`：建立按游戏的备份健康摘要，复用当前历史版本与诊断数据，不新增重复页面。
# 2026-08-13 可靠性闭环最终验收与缺口修复

**Task IDs：** RELIABILITY-RESTORE-001、HEALTH-001、PROTECTION-001、POLICY-001、ONBOARDING-001、GAME-TOOL-003/004、SMART-PROTECT-001/002、BACKUP-DIFF-001、BACKUP-GUARD-001、NOTIFY-001、MULTI-DEVICE-001、RCLONE-RELIABILITY-001

**实现与修复：**

- 恢复编排在部分写入、写后异常和 post-validation 失败时都恢复锁定 PreRestore；回滚失败输出稳定错误并进入人工介入状态。灾难演练增至 10 个临时目录用例。
- 多设备从机器名迁移到稳定不透明 DeviceId；保留旧 sidecar/旧决策兼容，便携设置不会复制设备身份，旧设置首次加载会生成并保存新身份。
- 备份异常保护加入 Off/Normal/Strict，重要游戏模板默认为 Strict；持久化 Manifest 的大量文件删除参与保护判定。
- GameTool 将已有实例决策抽成纯策略并补真实测试自有 EXE 演练；反作弊自动启动分类独立可测。
- Rclone 增加执行级白名单，完整命令行不再进入日志；补中断任务、取消子进程和禁止危险命令测试。
- 补健康云开关/异常、A3/B3 分叉、三种人工冲突决策、设置设备身份迁移等验收测试。
- 新增 `docs/ai/RELIABILITY_CLOSURE_AUDIT.md`，逐项列出 AUTO VERIFIED 与 MANUAL QA REQUIRED。

**核心设计选择：**

- 不新增第 15 项功能，不重写 Worker/恢复/GameTool；所有修补沿用现有 DTO、SQLite、Task Center 和恢复链。
- 云端目录身份与展示机器名分离；用户可迁移显示名，但同一安装保持稳定 ID。
- 异常备份允许保存，Retention 保护用户 Lock、PreRestore 和已验证健康点；仍不自动执行删除。
- UI 只在既有 Save Center 策略区加入共享样式 ComboBox，并保留 Automation 名称、键盘访问与响应式滚动结构。

**自动验证：**

- Core 42/42、Worker 117/117、Playnite 203/203。
- Release build 0 warnings / 0 errors；source、XAML、diff、WPF render-qa 全通过。
- 一键脚本普通权限完成构建、测试、打包、安装、版本校验并启动 Playnite；宿主日志确认 0.6.70 加载，Worker 运行并完成 IPC。

**MANUAL QA REQUIRED：**

真实游戏 Restore/Undo、真实 Rclone 断网/凭据/部分上传、真实两台设备、真实启动项、1000+ 游戏库、完整主题/DPI/键盘/连续缩放。详见最终审计文档。

**提交：** `dd39229`（可靠性安全缺口）；本审计文档提交见紧随其后的 docs commit。

**下一项：** 本轮无第 15 项；只接受人工 QA 反馈或明确的新任务。

## 2026-08-13 UI-QA-REAL-002 2K 首页状态区与设置/维护布局修复

**实现内容：**

- 修复首页“今日工作台”在 2K 最大化宽度下状态灯被挤到窄列、只剩竖向圆点的问题；状态胶囊改为标题下方的全宽第二行，并保留响应式内边距。
- 维护中心首次使用环境检查改为响应式 `UniformGrid`：宽窗口 3 列、中等窗口 2 列、窄窗口 1 列；统一卡片高度和横向拉伸，消除最后一行参差不齐的胶囊排列。
- 修复共享文本框内容宿主的垂直对齐和高度，统一为居中、42 DIP；设置页分类卡片增加底部安全间距并收敛圆角，避免窗口底部裁切和“被削掉”的视觉效果。
- 补充渲染夹具中的环境检查样例数据，并新增 UI 布局回归测试，确保后续修改不会复现这三类问题。

**核心设计选择：**

- 只修复现有共享布局和模板，不新增页面、不改变绑定/命令、滚动骨架或 DataGrid 虚拟化。
- 首页状态区使用显式 Grid 行而非依赖窄列 WrapPanel；维护卡片使用可随可用宽度变化的 UniformGrid；设置输入框继续复用现有共享模板。

**主要修改文件：**

- `src/GameSaveCenter.Playnite/Views/OverviewView.xaml`、`OverviewView.xaml.cs`
- `src/GameSaveCenter.Playnite/Views/MaintenanceView.xaml`、`MaintenanceView.xaml.cs`
- `src/GameSaveCenter.Playnite/Themes/WpfUiProduction.xaml`、`DesignTokens.xaml`、`Redesign.xaml`
- `tests/GameSaveCenter.RenderHarness/FakeDashboardData.cs`
- `tests/GameSaveCenter.Playnite.Tests/UiLayoutRegressionTests.cs`
- `docs/ai/PROJECT_MEMORY.md`、`docs/ai/WORKLOG.md`

**测试结果：**

- XAML 结构校验通过；Release 构建 0 warning / 0 error。
- Core 42/42、Worker 117/117、Playnite 206/206。
- `render-qa.ps1` 在 1040×700、1280×720、1366×768、1600×900、1920×1080 全部通过，包含首页与维护中心首次使用样例渲染。
- `dev-install-run.ps1 -Configuration Release` 构建、打包、安装校验成功；真实 Playnite 扩展日志确认 `GameSaveCenter 0.6.70.0` 加载，Worker 进程正常运行。

**验证边界：**

- `AUTO VERIFIED`：源码/XAML、Release 构建、Core/Worker/Playnite 测试、五种窗口尺寸渲染、开发安装、真实宿主插件日志和 Worker 启动。
- `MANUAL QA REQUIRED`：用户实际 2K 最大化窗口的视觉确认、Settings 全页滚动、主题/DPI 切换和连续缩放；本阶段没有伪造“已完成人工点击验收”。

**提交：** `a2c0f8d`（UI 修复、回归测试与长期记忆更新）。本条 hash 回填由随后独立的文档提交完成。

**下一项：**等待用户人工 QA 反馈；不新增本轮范围外功能。

## 2026-08-13 UI-QA-REAL-003 首页、工具设置与媒体列表布局补充

**实现内容：**

- 首页“最近 30 天玩过的游戏”卡片将两个操作按钮移到摘要下方的独立响应式行，避免右侧窄栏与标题/统计文字争抢空间。
- 修改器中心为启动延迟字段补充“启动延迟 / 秒”标签，保留原有 `LaunchDelaySeconds` 绑定，并使用 34 DIP 紧凑输入框，消除单独显示数字 `8` 的歧义和过大的输入框观感。
- 媒体中心当前媒体列表显式设置内容水平/垂直拉伸、顶部对齐，并使用顶部对齐的虚拟化面板，修复表头下方到第一行之间的大段空白。
- 补充三项 UI 布局回归测试，覆盖首页操作行、媒体列表首行定位和启动延迟单位/紧凑高度。

**核心设计选择：**

- 只调整既有卡片内部布局与共享控件尺寸，不新增页面，不改变命令、绑定、业务流程或媒体列表虚拟化骨架。
- 媒体列表仍使用 `ListBox` 与 `VirtualizingStackPanel`，只修正内容对齐，避免以固定高度或外层嵌套滚动器掩盖布局问题。

**主要修改文件：**

- `src/GameSaveCenter.Playnite/Views/OverviewView.xaml`
- `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml`
- `src/GameSaveCenter.Playnite/Views/TrainerCenterView.xaml`
- `tests/GameSaveCenter.Playnite.Tests/UiLayoutRegressionTests.cs`

**测试结果：**

- `check-xaml.ps1` 通过（13 个 XAML 文件）。
- Release 构建 0 warning / 0 error；Core 42/42、Worker 117/117、Playnite 209/209。
- `render-qa.ps1` 在 1040×700、1280×720、1366×768、1600×900、1920×1080 全部通过；渲染证据确认首页操作区不再拥挤、媒体首行紧贴列表顶部、工具设置显示带单位的紧凑延迟字段。
- `dev-install-run.ps1 -Configuration Release` 普通权限完成构建、打包、安装校验并启动 Playnite；扩展日志确认 `GameSaveCenter 0.6.70.0` 加载，Worker 从当前安装目录正常运行。

**验证边界：**

- `AUTO VERIFIED`：源码/XAML、Release 构建、Core/Worker/Playnite 测试、五种窗口尺寸渲染、开发安装、真实宿主扩展日志和 Worker 进程。
- `MANUAL QA REQUIRED`：用户实际 2K 最大化窗口、主题/DPI 切换、Settings 全页连续缩放，以及真实媒体数据量下的滚动体验；未将自动渲染冒充为人工验收。

**提交：**`3657145`（代码、回归测试与长期记忆）；本条 hash 回填由随后独立的文档提交完成。

**下一步：**等待用户人工视觉反馈；不引入本轮范围外功能。

## 2026-08-13 UI-QA-REAL-004 设置页分类卡圆角裁切修复

**问题根因：**

- 设置页左侧分类卡并不是圆角值失效，而是固定 232 DIP 的 `SettingsHeaderScroller` 在启用纵向滚动条后，实际内容 viewport 变窄；`TabItem` 仍占满原宽度，卡片右侧圆角被滚动区域直接裁掉，视觉上像底部或边缘被削平。

**实现内容：**

- 左侧分类滚动槽从 232 DIP 扩展到 248 DIP，为滚动条和卡片圆角保留安全宽度；分类卡自身继续使用共享 14 DIP 圆角。
- 分类卡增加短高度响应式：可用高度低于 760 DIP 时收紧高度和底部间距，避免五个分类卡在设置宿主 viewport 中被挤压。
- 左右设置滚动面增加明确的底部安全留白，顶部布局和现有设置内容不变；卡片 Chrome 开启自身边界裁剪，避免内容越界破坏圆角。
- 新增设置页圆角/短高度布局回归测试，并更新已有设置布局断言。

**主要修改文件：**

- `src/GameSaveCenter.Playnite/Themes/Redesign.xaml`
- `src/GameSaveCenter.Playnite/Settings/GameSaveCenterSettingsView.xaml.cs`
- `tests/GameSaveCenter.Playnite.Tests/SettingsAndAutoSelectSourceTests.cs`
- `tests/GameSaveCenter.Playnite.Tests/UiLayoutRegressionTests.cs`

**测试结果：**

- `check-xaml.ps1` 通过（13 个 XAML 文件）。
- Release 构建 0 warning / 0 error；Core 42/42、Worker 117/117、Playnite 210/210。
- `render-qa.ps1` 在 1040×700、1280×720、1366×768、1600×900、1920×1080 全部通过；设置布局探针确认左侧滚动槽为 248 DIP，短高度分类卡进入 60 DIP 档位，页面滚动仍为 Auto。
- `dev-install-run.ps1 -Configuration Release` 构建、打包、安装校验成功并启动 Playnite；真实扩展日志确认 `GameSaveCenter 0.6.70.0` 加载，Worker 从当前安装目录正常运行。

**验证边界：**

- `AUTO VERIFIED`：源码/XAML、Release 构建、Core/Worker/Playnite 测试、五种窗口尺寸渲染、开发安装、真实宿主扩展日志和 Worker 进程。
- `MANUAL QA REQUIRED`：用户实际 Playnite 设置页在 2K/DPI 缩放下的最终视觉确认，以及不同主题下滚动条与圆角之间的观感；本阶段不把离屏探针冒充为人工点击验收。

**提交：**`a9d7e58`（代码、回归测试与长期记忆）；本条 hash 回填由随后独立的文档提交完成。

**下一项：**等待用户人工视觉反馈；不引入本轮范围外功能。

## 2026-08-16 UI-206 Overview UiLab 页面级迁移

**实现内容：**

- 将生产首页重排为 UiLab 的真实阅读层级：顶部 Hero/当前游戏横向区域与六项指标占满工作区，下方才进入“最近活动 + 全局活动 / 风险与提醒 + 需关注事项”双栏。
- 右侧下栏采用 330 DIP 固定关注栏，窄窗口自动下移；保留生产页真实 Snapshot、Protection、AttentionFindings、命令、绑定和页面级滚动，不引入 UiLab 演示用顶部颜色按钮或演示滚动条。
- 统一 Overview 阅读卡、活动容器、全局活动圆角表头和低对比度分隔线；共享 `DataGridColumnHeader` 增加圆角和安全内边距，降低宿主截图中的尖锐边框感。
- 修复新关注行模板的 DataTemplate 触发器层级，并补充页面骨架/固定侧栏的源码回归断言；渲染 harness 改为验证侧栏与下方活动行对齐。

**验证结果：**

- `validate-source.py`、`check-xaml.ps1`、WPF 静态审查：通过，无新增错误。
- Playnite Release 构建：0 warning / 0 error；Playnite 测试 303/303 通过。
- 离屏 Overview 在 1040×700 至 3840×2160、多主题 Light/Dark 与 resize transition 下完成渲染；最终报告确认宽屏侧栏相对最近活动行偏移 0 DIP、无 Overview 新问题。
- 全量 render harness 仍报告 Save/Media 窄尺寸表格视口低于 236 DIP 的既有门禁项；这些问题不属于本阶段 Overview 改动，未伪报为全量通过。

**验证边界：**

- `AUTO VERIFIED`：源码/XAML、生产编译、Playnite 测试、Overview 多尺寸/双主题离屏渲染、布局对齐探针。
- `MANUAL QA REQUIRED`：真实 Playnite 宿主中的最终 DPI、Follow/高对比度主题、键盘焦点和连续缩放；本阶段未将离屏渲染冒充为人工验收。

**提交：**本阶段代码、测试与文档需由 Agent 独立 commit 并 push 到 `main`。

## 2026-08-16 UI-208 Overview 全局活动业务列表对齐 UiLab

**问题根因：**

- UiLab 的“全局活动”是业务阅读列表：左侧类型胶囊，中间对象/事件两行，右侧结果和时间；生产首页之前仍保留了额外的 DataGrid 式表头和图标列，所以即使卡片外观接近，页面信息层级仍明显不同。

**实现内容：**

- 移除 Overview 全局活动专用表头，改为 UiLab 同款四列业务行；`Activities`、`KindDisplay`、`ResultDisplay`、虚拟化 ItemsControl 和页面级滚动保持不变。
- 类型与结果胶囊继续按真实 `Result` 使用成功、警告、失败语义色；窄窗口只收紧类型/时间列，不引入 demo 内部滚动条。
- 更新布局回归测试，确保四列、中文摘要截断、状态胶囊和时间列间距不会回退到旧表格式。

**验证结果：**

- `validate-source.py`、`check-xaml.ps1`、WPF 静态审查通过（无新增错误）。
- Playnite Release 构建 0 warning / 0 error；Playnite 测试 303/303 通过。
- RenderHarness v6/v6.2 首页宽/窄截图通过；全局活动业务行无文字重叠，页面滚动和真实绑定未改变。

**验证边界：**

- `AUTO VERIFIED`：源码/XAML、生产编译、Playnite 测试、Overview 宽/窄离屏截图。
- `MANUAL QA REQUIRED`：真实 Playnite 宿主中的 2K/DPI、Follow/高对比度主题、键盘焦点和连续缩放；本阶段未把离屏截图冒充宿主像素真值。

## 2026-08-16 UI-209 共享 DataGrid 表头对齐 UiLab

**问题根因：**

- UiLab 的 `LabGridColumnHeader` 以透明表头容器承载弱分隔线；生产虽然已有圆角列头模板，但 `DataGridColumnHeadersPresenter` 仍给整行连续铺设表头底色，列头之间的圆角间隙因此不可见，视觉上接近尖锐矩形条。

**实现内容：**

- 将生产共享 `DataGridColumnHeadersPresenter` 背景改为透明，保留每个 `DataGridColumnHeader` 自身的圆角、低对比度填充和分隔线。
- 不改变表头高度、列宽、排序 glyph、`VirtualizingPanel.ScrollUnit=Item`、行/列虚拟化、页面滚动条或页内真实绑定。

**验证结果：**

- `validate-source.py`、`check-xaml.ps1` 通过。
- Playnite Release 构建 0 warning / 0 error；Playnite 测试 303/303 通过。
- RenderHarness v6/v6.2 表格及首页宽/窄截图通过；Maintenance 表头圆角间隙可见，无白块或文字重叠。

**验证边界：**

- `AUTO VERIFIED`：源码/XAML、生产编译、Playnite 测试、离屏截图。
- `MANUAL QA REQUIRED`：真实 Playnite 宿主最终 DPI、Follow/高对比度主题、键盘焦点和连续缩放。

## 2026-08-16 UI-210 Media 统计带与响应式布局对齐 UiLab

**问题根因：**

- UiLab Media 顶部是一个连续圆角统计带，默认阅读当前游戏媒体；生产之前是四张分离统计卡且默认落在待归类页，首屏结构差异明显。
- Media 在短窗/缩放时把列表视口压到 180-200 DIP；从窄窗恢复宽窗后，Inspector 还可能保持折叠状态，造成宽屏右侧详情消失。

**实现内容：**

- 将 Media 四项统计改为单一 `GscRedesignSectionCard` 内的四段 value → caption → supporting line 结构，保留真实 `MediaSummary`/`Snapshot` 绑定。
- 默认选择“当前游戏媒体”标签，使首屏阅读顺序与 UiLab 一致；待归类表格和归类命令仍可直接切换。
- 常规 700-720 DIP 窗口将媒体/收件箱视口维持在 236 DIP；极短窗口才使用 180 DIP 紧凑下限。
- 宽屏恢复时重新显示已选媒体 Inspector；窄屏仍使用现有“查看媒体详情”折叠按钮，未引入 UiLab 滚动条或关闭 ListBox/DataGrid 虚拟化。

**验证结果：**

- `validate-source.py`、`check-xaml.ps1` 通过。
- Playnite Release 构建 0 warning / 0 error；Playnite 测试 303/303 通过。
- 重建 RenderHarness 后，Media Light/Dark、1040/1100/1366/2560 多尺寸和 2560→1100→2560 resize transition 通过；Media 不再产生视口不足或 Inspector 恢复失败门禁。
- 全量 render-qa 仍只剩 Save 候选表窄窗 `<236 DIP` 的既有门禁，未把它误记为本阶段已解决。

**验证边界：**

- `AUTO VERIFIED`：源码/XAML、生产编译、Playnite 测试、Media 多主题/多尺寸离屏渲染。
- `MANUAL QA REQUIRED`：真实 Playnite 宿主中的最终 DPI、Follow/高对比度主题、键盘焦点、连续缩放和大媒体库滚动。

## 2026-08-16 UI-211 Save 候选表窄窗视口收口

**问题根因：**

- Save 的响应式高度公式沿用了过大的 `height - 520` 扣减，常规 700-720 DIP 窗口把候选表压到 205-225 DIP，触发全量 render-qa 的 `<236 DIP` 门禁。

**实现内容：**

- 将 Save 历史/候选表的常规视口公式收敛到 `Math.Max(180d, Math.Min(252d, height - 464d))`；700-720 DIP 保持 236 DIP，真正短窗才降至 180 DIP。
- 保留 Save 的生产 DataGrid、Item scrolling、行/列虚拟化、Inspector 抽屉和页面命令，不迁移 UiLab 的滚动条模板。

**验证结果：**

- `validate-source.py`、`check-xaml.ps1` 通过。
- Playnite Release 构建 0 warning / 0 error；Playnite 测试 303/303 通过。
- 全量 RenderHarness `render-qa OK`：7 页面、Light/Dark、1040/1100/1366/2560、多尺寸和 2560→1100→2560 resize transition 全部通过。

**验证边界：**

- `AUTO VERIFIED`：源码/XAML、生产编译、Playnite 测试、全量离屏渲染。
- `MANUAL QA REQUIRED`：真实 Playnite 宿主最终 DPI、Follow/高对比度主题、键盘焦点和连续缩放。

## 2026-08-16 UI-212 Media 缩略图网格迁移并保留生产虚拟化

**问题根因：**

- UiLab 当前媒体页是固定 164 DIP 卡片、96 DIP 缩略图的网格；生产之前是横向三列信息行，因此即使统计带、Inspector 和页签已经接近 Demo，媒体主体仍明显不像样板。
- Demo 使用普通 `WrapPanel`，直接复制会一次性生成大媒体库的所有缩略图，也会把 Demo 的滚动条模板带入生产，和项目的大列表性能/宿主兼容约束冲突。

**实现内容：**

- 新增 `Controls/VirtualizingWrapPanel`：按固定卡片尺寸计算列/行，只生成可见区附近的 ListBoxItem，实现 `IScrollInfo` 的行、页、鼠标滚轮和 `MakeVisible`，并兼容 Recycling generator。
- Media 当前游戏列表改为该虚拟化网格；卡片按 Demo 迁移缩略图、底部渐变、录像标识、收藏标识、文件名、拍摄时间、云端状态和圆角选中描边。
- 继续使用生产 `ListBox`/`ScrollViewer`/`ScrollBar`，保留 `MediaView`、Extended selection、Inspector 抽屉、真实打开/收藏/备注/归类命令和窄窗详情按钮。
- 为首次挂载时生成器尚未就绪的 WPF 测量时序增加保护；空集合、Reset 和宽窄窗口切换不会抛异常。

**验证结果：**

- `validate-source.py`、`check-xaml.ps1`、WPF 静态审查通过（0 error；已有滚动测量提示和硬编码颜色 info 保持历史基线）。
- Playnite Release 构建 0 warning / 0 error；Playnite 测试 303/303 通过。
- RenderHarness 全量 `render-qa OK`：7 页面、Light/Dark、1040/1100/1366/2560、多尺寸和 2560→1100→2560 resize transition 全部通过；Media 网格截图已输出到 `artifacts/ui-qa/media-grid-migration-v3`。

**验证边界：**

- `AUTO VERIFIED`：源码/XAML、生产编译、Playnite 测试、离屏卡片网格、生产滚动条和响应式过渡。

## 2026-08-17 UI-213 AcrylicFork 页面级全量结构收口

**问题根因：**

- 之前生产页面仍保留旧的 Dashboard 外壳、旧的页面上下文卡和局部圆角表格模板；它们与 AcrylicFork Demo 的信息架构不是同一棵视觉树，因此局部调色或修 Margin 无法达到 Demo 观感。
- 维护中心默认进入“问题列表”也让健康概览在首屏不可见，造成用户看到的不是 Demo 结构；RenderHarness 的旧探针同样依赖这个默认值。

**实现内容：**

- Dashboard 外壳收口为共享 Header/GameSwitcher + 单一 PageHost；隐藏重复的 SelectedGameHeader、旧安全提示和旧策略包装，保留真实导航、Command、Binding 和 Playnite 集成。
- 首页按 Demo 重排为 Hero/当前游戏、六项指标、最近任务、全局活动、风险与提醒、需关注事项；全局活动使用四列业务行，活动字段显式 OneWay，关注入口补充可访问说明，零分母进度条自动折叠。
- 维护中心诊断概览按 Demo 顺序调整为六项健康卡、环境检查、诊断操作、完整摘要；问题表格仍独立保留并由 RenderHarness 显式选择验证。
- 共享 DataGrid 表头/单元格/行改为 Demo 的透明表头、底部分隔线和稳定行节奏；继续使用项目自身滚动条和 Recycling/Item 虚拟化，不迁移 Demo 顶部颜色按钮或滚动条。
- Media 保持真实媒体绑定并继续使用生产虚拟化滚动；RenderHarness 的窄窗问题表探针同步到新的维护默认 Tab。

**验证结果：**

- python scripts/validate-source.py 通过。
- python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .：0 errors，13 条既有布局提示。
- 修复 `scripts/package.ps1` 在未指定 `BuildOutputRoot` 时传入空 `dotnet` 参数的问题；无构建输出根目录时已成功生成并校验 `.pext`，安装包包含 `GameSaveCenter.Contracts.dll`。
- Playnite Debug 构建 0 warning / 0 error；Playnite 测试 303/303 通过。
- RenderHarness Release 构建 0 warning / 0 error；全量 render-qa OK，覆盖 7 页面、浅色/深色、1040/1100/1366/2560、多尺寸及 2560→1100→2560。
- Solution 全量测试中 Core 59/59、Playnite 303/303 通过；Worker 仍有 2 个依赖本机可选环境状态的既有失败（健康检查期望 Healthy/Skipped，当前返回 Warning），未由本次 UI 改动触发。

**验证边界：**

- AUTO VERIFIED：源码/XAML、生产编译、回归测试、离屏视觉和布局探针。
- MANUAL QA REQUIRED：真实 Playnite 宿主安装目录、最终 DPI/Follow/高对比度主题、键盘焦点和真实大库滚动；本轮没有把 RenderHarness 截图冒充真实宿主验收。

## 2026-08-17 BUILD-001 一键安装低磁盘测试隔离

**原因定位：**

- 一键安装使用隔离构建目录在 D 盘，但 Worker 完整性测试仍通过系统 `TEMP/TMP` 在 C 盘创建临时根目录。
- 当前机器 C 盘剩余空间低于完整性检查阈值 512 MiB，`IntegrityCheckService` 正确产生 `LOW_DISK_SPACE` 警告，导致健康和可选依赖测试从 `Healthy/Skipped` 变成 `Warning`。

**实现与验证：**

- `scripts/build.ps1` 在指定 `OutputRoot` 时将测试临时目录切换到该隔离输出根目录，生产完整性检查语义不变。
- `scripts/dev-install-run.ps1` 将隔离构建根目录缩短为 `artifacts/gsc-b/<guid>`，避免 .NET Framework Playnite 测试适配器在深路径下加载失败。
- 隔离 Release 构建验证：Core 59/59、Worker 191/191、Playnite 303/303，构建 0 warning / 0 error。
- 完整一键安装（`-NoStart`）已验证 Worker 发布、`.pext` 打包、安装目录和 `GameSaveCenter.Contracts.dll` 均成功。

## 2026-08-18 UI-214 首页状态徽标可读性收口

**用户反馈：**

- 首页最近任务、全局活动的“成功/信息”等状态气泡文字没有明确居中。
- 风险与提醒中的“风险”徽标受旧 `ContentControl` 模板约束，存在文字裁切风险。
- 任务图标仍引用未统一的旧主题资源键。

**实现内容：**

- `OverviewView.xaml` 为最近任务状态、全局活动类型和结果徽标补充明确的水平/垂直居中约束。
- 风险徽标改为独立 `Border + TextBlock`，设置最小宽度、内边距、文本省略 Tooltip，并按健康/警告/风险/未知状态同步背景、边框和文字颜色。
- 任务图标统一使用生产 `GscAccentBrush`，避免 Demo 资源键在宿主中回退为黑色。

**验证结果：**

- `git diff --check` 通过。
- XAML structural validation 通过；Release 构建 `artifacts/phase-overview-v18` 为 0 warning / 0 error。
- RenderHarness 的首页 Overview 在浅色/深色、1040/1100/1366/2560 尺寸探针通过，生成的 PNG 中风险徽标文字完整且状态颜色正确。
- 全量 RenderHarness 仍未通过：已有 Media resize recovery 两项失败（`MediaGrid` 尺寸未恢复、`MediaInspectorScrollViewer` 状态未恢复），不能把本轮写成全量视觉回归通过。
- `scripts/validate-source.py` 仍报告已有媒体缩略图 96px bounded decode 规则缺失；WPF UI 静态扫描无 error，仅保留既有布局提示。
- 本轮重复执行 Playnite 测试命令后长时间无输出，已停止这批明确由该命令启动的 30 个空闲 `dotnet` 进程；未将这次无结果的运行计为通过，沿用上一阶段已记录的 Playnite 回归基线。

## 2026-08-18 UI-226 首页 Demo 主题别名同步与生产页面资源注入

**用户反馈：**

- 首页最近任务的图标和进度条显示为黑色/透明，右上“全部”链接不明显。
- 全局活动分类和结果气泡没有使用 Demo 的紫色/语义色层级。
- 风险与提醒中的“启用所选保护”没有呈现为紫色主按钮。

**根因：**

- `AcrylicReferenceControls.xaml` 的共享控件仍读取 Demo 使用的短资源键（`AccentBrush`、`AccentTintBrush`、`AccentStrokeBrush` 等）。
- 生产 `AdaptiveThemePalette.ApplyAccentResources` 只更新了 `Gsc*` 资源键，没有同步这些短别名；在 Playnite 宿主中，未解析的短键回退到了宿主主题的黑色/透明资源，因此页面结构虽然已改，颜色层级仍与 Demo 不一致。

**实现内容：**

- 在生产页面局部 `ResourceDictionary` 中同步写入 Demo 的短主题别名，同时保留现有 `Gsc*` 资源和项目滚动条实现。
- 增加资源值和源码映射回归断言，确认别名跟随计算后的 Playnite 宿主强调色，而不是静态捕获某个主题。
- 未迁移 Demo 顶部彩色主题按钮；只复用其主题色令牌。

**验证结果：**

- `scripts/validate-source.py`：通过。
- `validate_wpf_ui.py`：0 error，19 条既有 warning。
- 隔离 Release 构建（XAML、Contracts/Core/Worker、Playnite 插件及测试程序集）：0 warning / 0 error。
- 本次补充生产 PageHost 资源注入断言后重跑针对性 Playnite 单测，命令长时间无输出并被停止，未将其记为通过；Release 构建已确认测试程序集可编译。
- 全量 `WpfUiResourceDictionaryTests` 仍有 43 个迁移前结构断言失败；这些断言检查旧 WrapPanel、旧列定义和旧字号，未将其记为本轮通过，也未因本次主题资源修复改动业务代码。
- 发现上一阶段只给旧的隐藏页面树注入了运行时主题资源，生产 `AcrylicProductionShellView.PageHost` 中的真实页面没有收到 Demo 短资源别名；已在 `DashboardView.ApplyAdaptiveTheme()` 同时给生产 Shell 和其实际页面实例注入运行时资源。
- `scripts/validate-source.py`：通过；`validate_wpf_ui.py`：0 error、19 条既有 warning；隔离 Release 构建、打包和开发安装通过。
- RenderHarness 的深色首页 PNG 已实际检查：最近任务图标、进度条、`全部` 链接、全局活动分类文字、风险主按钮均呈现紫色或对应语义色；这证明离屏夹具中的实际资源解析已修复。
- 全量 RenderHarness 仍未通过：已有 Media resize recovery 两项失败（`MediaGrid` 尺寸未恢复、`MediaInspectorScrollViewer` 状态未恢复），不能把本轮写成全量视觉回归通过。

**验证边界：**

- 本阶段没有把 RenderHarness 全量结果写成通过；此前 Media resize recovery 失败仍需后续处理。
- Playnite 日志确认新 DLL 已加载，但 Computer Use 返回 `EmptyWindowAutomationPeer`、`MainWindowHandle=0`，截图实际是桌面其他窗口；因此真实宿主视觉仍是 `MANUAL QA REQUIRED`，不能把离屏 PNG 当成 Playnite 截图。
- 本阶段只修正首页生产页面的主题资源注入，不代表存档、媒体、任务、修改器和维护中心的全量 Demo 对照已经完成。

**验证边界：**

- 本轮没有重新启动或控制 Playnite 宿主；上一次屏幕控制已由用户物理 Escape 停止，因此没有把离屏 PNG 冒充真实宿主验收。

## 2026-08-18 UI-225 首页徽标实际宿主复核

**用户反馈：**

- 首页最近任务中的“成功”状态徽标文字没有可靠居中。
- 首页全局活动中的“信息/成功”徽标文字没有可靠居中。
- 风险与提醒中的“风险”徽标文字可能被截断。

**实现内容：**

- `OverviewView.xaml` 新增仅用于纯文本状态徽标的居中 `DataTemplate`，避免修改共享 Chip 模板后破坏 Worker/Ludusavi 等复杂内容徽标。
- 最近任务、全局活动类型/结果徽标统一设置水平/垂直内容居中、零内边距和足够的最小宽度。
- 风险徽标从横向 `StackPanel` 改为 `Grid` 的独立 `Auto` 列；游戏名称使用省略和 Tooltip，风险文字使用 72 DIP 最小宽度并保持完整显示。

**验证结果：**

- `scripts/validate-source.py` 通过。
- `validate_wpf_ui.py`：0 error，19 条既有布局 warning，未新增 XAML 错误。
- Release 编译：0 warning / 0 error。
- Worker 集成测试 191 项全部通过；Playnite 旧结构断言仍有 61 项失败，原因是测试仍检查迁移前的 Overview/Media/Maintenance/Task/Saves 结构，未将其误报为本次 UI 修改通过。
- RenderHarness 首页主题/尺寸检查通过；全量 RenderHarness 仍被既有 Media resize recovery 两项失败拦截。
- 最新包已安装到 Playnite 扩展目录并重启真实 Playnite。宿主截图确认最近任务“成功”徽标居中、风险徽标完整显示、侧栏末项显示为“设置”。

**验证边界：**

- 本条只确认首页徽标这一独立修复和真实宿主截图，不代表 AcrylicFork 全量页面迁移已经完成。
-
## 2026-08-18 UI-224 首页活动/风险徽标测量修正与真实宿主验证边界

**用户反馈：**

- 首页最近任务和全局活动中的“成功/信息”等气泡文字没有可靠居中。
- 风险与提醒中的“风险”气泡在实际宿主中有中文裁切风险。

**实现内容：**

- `OverviewView.xaml` 的最近任务、全局活动类型/结果徽标改为固定宽度、零内边距，并让内部 `TextBlock` 使用 `HorizontalAlignment=Center`、`TextAlignment=Center`，避免旧 `LabSubCard` 默认测量影响文字位置。
- 风险徽标宽度调整为 70 DIP，同样取消内边距并显式居中；继续保留 Tooltip 和各健康状态的背景、边框、文字颜色绑定。

**验证结果：**

- XAML structural validation：18 个文件通过。
- `scripts/validate-source.py`：通过。
- `validate_wpf_ui.py`：0 error，19 条已有布局 warning，未新增 XAML 错误。
- Release 编译：0 warning / 0 error；打包与开发安装成功，安装目录中的生产 DLL 为 `0.6.70.0`。
- Playnite 日志确认真实宿主加载了 `GameSaveCenter, version 0.6.70`。

**验证边界：**

- 电脑控制工具当前为 Playnite 返回了 `EmptyWindowAutomationPeer`，但截图实际显示的是桌面上的其他窗口，且 Playnite 进程 `MainWindowHandle=0`。因此本轮只能确认“生产 DLL 已被宿主加载”，不能确认“真实 Playnite 页面视觉通过”；该截图不计入视觉验收证据。
- Worker 集成测试仍有 2 个已有环境状态失败（期望 `Healthy/Skipped`，实际 `Warning`）；全量 RenderHarness 仍有 Media resize recovery 两项失败。两者均未被本轮徽标改动伪装成通过。

## 2026-08-18 UI-227 媒体中心模式栏主题修正

**用户反馈：**

- 媒体中心顶部模式/Tab 卡片栏仍使用 Demo 旧的浅灰透明背景，与其它生产页面的深色 Tab 外壳不一致。

**实现内容：**

- `MediaCenterView.xaml` 的 `MediaModeStrip` 改用生产深色 `GscGlassStrongBrush`、`GscControlStrokeBrush` 和现有圆角/裁剪规则，消除媒体页顶部灰色透明条。
- 当前游戏媒体、待归类、来源规则三个模式仍使用原有 RadioButton、Binding 和命令，仅替换外层承载视觉。
- 选中项改用 `GscAccentTintStrongBrush`、`GscAccentBrush` 和 `GscSelectionTextBrush`，并让悬停状态不覆盖选中状态；未迁移 Demo 顶部颜色按钮，也未替换项目滚动条。

**验证结果：**

- `git diff --check`：通过。
- `scripts/validate-source.py`：通过。
- `validate_wpf_ui.py`：0 error，19 条既有 warning。
- Release 隔离构建：0 warning / 0 error。
- RenderHarness 编译成功并输出了各尺寸/主题结果，但全量回归仍被 Media resize recovery 两项失败拦截：`MediaGrid` 在 2560→1100→2560 回弹后尺寸不一致，`MediaInspectorScrollViewer` 在回弹后从可见变为折叠。不能记为全量通过。

**验证边界：**

- 本阶段没有把 RenderHarness PNG 当作 Playnite 真机截图；真实宿主视觉仍需在 Computer Use 能识别 Playnite 窗口后复核。

## 2026-08-18 UI-228 恢复被错误覆盖的 AcrylicFork 页面基线

**问题确认：**

- `be5707d` 将生产页面覆盖成另一套“今日工作台 / 最近活动”页面树；两台电脑同时出现旧界面，是因为都拉取了同一个提交，不是 WPF 缓存或主题缓存。
- 该提交的页面源码与用户此前确认的 AcrylicFork 基线不一致，故使用其父提交的页面与主题资源作为恢复目标，不修改业务命令、Binding、数据契约或 Playnite 插件入口。

**恢复内容：**

- 恢复首页“最近任务 / 全局活动 / 风险与提醒”页面结构。
- 恢复存档、媒体、任务、修改器和维护页面在该基线下的 XAML、代码后置和共享主题资源。
- 将针对已撤销“今日工作台”架构的 61 条源码契约断言明确标记为跳过，并新增当前基线断言，校验首页、存档、媒体和任务的真实页面入口；不使用伪造控件迎合旧测试。

**验证结果：**

- `scripts/validate-source.py`：通过。
- `validate_wpf_ui.py`：0 error，19 条既有 warning。
- Release 编译：0 warning / 0 error。
- Core：59/59 通过；Worker：191/191 通过。
- Playnite：246 通过、61 跳过、0 失败（跳过项均明确标注为已撤销架构断言；新增 1 条媒体模式栏颜色基线断言）。

**验证边界：**

- 本轮没有把离屏 PNG 当作真实 Playnite 宿主截图；真实宿主视觉仍需在可识别的 Playnite 窗口中复核。

## 2026-08-18 UI-230 首页风险明细固定视口与旧工具栏清理

**本阶段内容：**

- 首页“风险与提醒”中的需关注列表、展开后的最近游戏保护明细分别使用独立的项目 `GscPageScrollViewer`。
- 两个列表均设置 `MaxHeight=190`、垂直滚动 `Auto`、水平滚动 `Disabled`；数据未超出时不显示滚动条，超出时只在风险区域内部滚动，不再无限增加首页高度。
- 保留页面级滚动负责首页其它内容，未引入 Demo 滚动条，也未改变真实 Binding、Command 或保护业务语义。
- 清理首页残留的“今日工作台”旧工具栏及对应代码后置响应式逻辑，恢复 Hero、概览指标、最近任务、全局活动的页面阅读顺序。
- 媒体中心模式栏改用生产深色控件底色和紫色选中底色，避免灰色透明条覆盖页面主题。

**验证结果：**

- `scripts/validate-source.py`：通过。
- `validate_wpf_ui.py`：0 error，20 条既有 warning。
- WPF Release 编译：0 warning / 0 error。
- Playnite 测试：248 通过、61 跳过、0 失败。
- RenderHarness：`render-qa OK`；1040×700 报告确认风险区域受 190 DIP 上限约束，首页根滚动面仍可滚动。
- `git diff --check`：通过。

**验证边界：**

- 默认 RenderHarness fixture 的需关注项没有超过 190 DIP，因此报告中该列表为 `scrollable=false` 是“当前数据未溢出”的正常结果，不代表没有配置滚动；超量时由同一 `ScrollViewer` 自动出现滚动条。
- 本阶段没有把离屏 PNG 当作新的真实 Playnite 宿主截图；真实宿主视觉仍需在可识别的 Playnite 窗口中复核。

## 2026-08-19 UI-231 首页高数量风险项的高度边界

**本阶段内容：**

- 首页风险区域不让明细数量直接决定主页面高度。
- “最近游戏保护明细”和“需关注事项”分别使用独立的 `GscPageScrollViewer`，均限制为 `MaxHeight=190`，超出后只在对应卡片内部显示项目现有滚动条。
- 页面根滚动继续负责首页整体内容；风险卡片没有再叠加第二层整卡滚动，避免出现无意义的双滚动条。
- 风险明细仍来自真实 `RecentProtection.Items`、`AttentionFindings` 绑定，滚动限制只是视口边界，不是业务数据截断。
- 同步保留首页最近任务的独立进度/状态/时间列、全局活动语义色徽标，以及媒体中心紫色模式栏样式修正。

**验证边界：**

- Release 编译、核心/Worker/Playnite 测试和安装包生成已通过。
- 静态契约确认两个风险列表均存在 190 DIP 内部视口。
- 本轮真实 Playnite 视觉截图未能完成：Playnite 进程日志显示插件已加载，但屏幕控制接口返回 `EmptyWindowAutomationPeer`，截图实际指向 Codex 窗口，因此未将其冒充为视觉验收通过。

## 2026-08-19 UI-232 首页风险区域布局回归

**本阶段内容：**

- 首页“风险与提醒”的需关注列表和展开后的最近游戏保护明细继续使用独立 `GscPageScrollViewer`，`MaxHeight=190`、垂直滚动 `Auto`、水平滚动 `Disabled`；条目增加时只在列表内部滚动，不再无限撑高首页。
- 修正首页宽布局右侧栏的测量行：风险与提醒现在与最近任务处于同一行，并跨越最近任务/全局活动两行；窄布局则落到同一个显式的整行，不再落入隐式行或与全局活动错位。
- RenderHarness 的对齐检查改为直接比较右侧栏与 `OverviewRecentActivityCard`，不再拿整页顶部滚动面误判合法布局。
- 媒体中心模式栏使用生产深色表面，选中项使用明确的紫色强调底色；未迁移 Demo 顶部颜色按钮，也未替换项目滚动条。

**验证结果：**

- `git diff --check`：通过。
- `scripts/validate-source.py`：通过。
- `validate_wpf_ui.py`：0 error；20 条既有 warning，无新增错误。
- RenderHarness：`render-qa OK`；1040/1100 DIP 风险列表均为 `MaxHeight=190` 且溢出探针报告 `scrollable=True`；1536/1600/1707 DIP 右侧栏与最近任务卡片 `OverviewSecondaryTopDelta=0 DIP`。
- 本轮 Playnite 测试命令启动后长时间无输出且子进程低 CPU 空转，已停止；没有把它记为通过。真实 Playnite 宿主视觉截图仍未完成。

## 2026-08-19 UI-233 首页风险视口与首页行结构回归

**本阶段内容：**

- 确认首页“风险与提醒”采用列表级有限视口：`OverviewAttentionScrollViewer` 与 `OverviewProtectionItemsScrollViewer` 均使用 `GscPageScrollViewer`，`MaxHeight=190`，垂直滚动 `Auto`，水平滚动 `Disabled`。风险数量增加时只在列表内部滚动，风险卡片和首页根内容不会无限增长，也没有新增整卡滚动造成双滚动条。
- 最近任务恢复为 Demo 行节奏：任务类型与游戏名同一标题行，详情与进度条同一副标题行，结果徽标和时间独立对齐；任务图标使用紫色强调底色。
- 全局活动移除生产页面多余的表头行和图标列，改为分类徽标、游戏/摘要、结果和时间四列，保留真实活动绑定。
- 修改器中心已安装工具顶部改为显式 Grid 行，导入按钮和拖拽提示不再与列表首行共享 WrapPanel 测量空间。
- 媒体中心模式栏使用 `GscAccentTintBrush` 紫色主题表面，选中 RadioButton 保留更强的紫色状态；未迁移 Demo 顶部颜色按钮或 Demo 滚动条。
- `scripts/build.ps1` 与 `scripts/render-qa.ps1` 关闭不必要的 SDK Workload Resolver，兼容仅安装裸 .NET SDK 的构建机；该项目目标为 .NET Framework/WPF，不依赖 SDK Workload。

**验证结果：**

- `scripts/validate-source.py`：通过。
- `validate_wpf_ui.py`：0 error，20 条既有 warning，161 条 info。
- `dotnet build` Playnite 与 RenderHarness：0 warning / 0 error。
- `dotnet test tests/GameSaveCenter.Playnite.Tests/GameSaveCenter.Playnite.Tests.csproj`：248 通过、61 跳过、0 失败。
- `scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot artifacts/build-check-20260819`：还原和解决方案 Release 编译通过，0 warning / 0 error。
- RenderHarness：`render-qa OK`；1040×700、1100×720 的首页风险列表视口均为 190 DIP，溢出探针 `scrollable=True`；首页根滚动面保持独立；浅色/深色、多尺寸和缩放过渡探针通过。
- `git diff --check`：通过。

**验证边界：**

- 本轮证据来自源码、构建、测试和 RenderHarness 离屏渲染；当前工具上下文没有可用的 Playnite 窗口控制接口，因此没有把离屏 PNG 冒充真实 Playnite 宿主视觉验收。真实宿主仍需在可识别的 Playnite 页面中复核。

## 2026-08-19 UI-234 首页风险列表取消静默截断

**本阶段内容：**

- `DashboardViewModel.AttentionFindings` 不再使用 `.Take(4)` 静默丢弃第 5 条之后的风险；真实严重度筛选结果全部交给首页绑定。
- 首页仍由 `OverviewAttentionScrollViewer` 和 `OverviewProtectionItemsScrollViewer` 各自承担 190 DIP 的有限视口，超出部分只在项目现有滚动条内滚动，不给整张风险卡片再叠加滚动上下文。
- “打开维护中心”继续作为完整问题处理入口；首页保留摘要式卡片，不承担无限高度的完整问题列表。

**验证结果：**

- `git diff --check`：通过。
- `scripts/validate-source.py`：通过。
- `validate_wpf_ui.py`：0 error，20 条既有 warning，161 条 info。
- 本轮 Playnite 资源测试命令启动后长时间无输出且子进程低 CPU 空转，未将其记为通过；后续需用可完成的测试入口重新验证。

## 2026-08-19 UI-235 首页紫色状态层级增强

**本阶段内容：**

- 首页最近任务图标与全局活动类型徽标由低透明度 `GscAccentTintBrush` 调整为 `GscAccentTintStrongBrush`。
- 保留成功、失败、警告、信息等结果状态的原有语义色、Binding 和 Command；本次只增强紫色基础表面的可见度，避免与页面背景混成黑色透明层。
- 首页“启用所选保护”等主要操作继续使用项目现有紫色主按钮资源，未新增 Demo 顶部主题按钮。

**验证结果：**

- `git diff --check`：通过。
- `scripts/validate-source.py`：通过。
- `validate_wpf_ui.py`：0 error，20 条既有 warning，161 条 info。
- `scripts/build.ps1 -Configuration Debug -SkipTests -OutputRoot artifacts/build-check-v235`：解决方案编译通过，0 warning / 0 error。

## 2026-08-19 UI-236 风险视口与媒体紫色状态回归

**本阶段内容：**

- 首页风险与提醒继续采用列表级有限视口，风险数量很多时只在列表内部滚动，不再无限撑高首页；真实 `AttentionFindings` 不做静默截断。
- 首页最近任务游戏名、修改器下载版本号和媒体来源状态增加有限测量、截断和 Tooltip，避免长文本挤压按钮或破坏行结构。
- 媒体中心模式栏改为更明确的生产紫色强调表面；未迁移 Demo 顶部颜色按钮，也未替换项目滚动条。
- 同步更新媒体模式栏的源码契约测试，使测试描述当前的强紫色状态而不是已废弃的旧弱色资源。

**验证结果：**

- Playnite 测试重新编译后：248 通过、61 跳过、0 失败；旧的 38 项失败确认为旧测试二进制造成的假象。
- `scripts/validate-source.py`：通过。
- `validate_wpf_ui.py`：0 error，20 条既有 warning，161 条 info。
- `git diff --check`：通过。
- RenderHarness `render-current3`：`render-qa OK`；1040×700、1100×720 的风险视口均固定为 190 DIP 并通过溢出滚动探针；浅色/深色、多尺寸和缩放过渡均通过。

**验证边界：**

- RenderHarness 是离屏验证，不是 Playnite 真机截图；真实宿主视觉仍需在可识别的 Playnite 窗口中复核，不能用错误窗口截图替代。

## 2026-08-19 UI-237 Release 安装与宿主截图边界复核

**本阶段内容：**

- 使用当前 `main` 提交 `37ab9a6` 完成 Release 一键安装；安装前保持 Playnite 退出，未修改扩展目录 ACL，也未停止另一套无关的 Preview Worker。
- 安装目录已替换为 `C:\Users\lopmatu\AppData\Roaming\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`，`extension.yaml` 为 `0.6.70`，生产 DLL 文件版本为 `0.6.70.0`。
- 首页风险与提醒继续使用列表级 `MaxHeight=190` 内部滚动；数量很多时不会无限撑高 Dashboard，首页根滚动与风险列表滚动职责分离。

**验证结果：**

- Release 编译通过，0 warning / 0 error。
- Core 测试：59/59 通过。
- Worker 集成测试：191/191 通过。
- Playnite 测试：248 通过、61 跳过、0 失败。
- 安装报告：`artifacts/last-dev-install.txt`，清单与 DLL 版本核对通过。

**验证边界：**

- 本轮尝试启动 Playnite 并选择唯一返回的 Playnite 窗口，但 `get_window_state` 返回的图像内容实际是其他桌面窗口；因此真实 Playnite 页面视觉验收仍记录为“阻塞”，不能用该截图或 RenderHarness 离屏图冒充宿主通过。

## 2026-08-19 UI-238 首页风险与提醒总视口收口

**本阶段内容：**

- 首页风险与提醒区域新增独立的 `OverviewRiskViewport`，使用项目现有 `GscPageScrollViewer`，`MaxHeight=330`、垂直滚动 `Auto`、水平滚动 `Disabled`；风险条目数量很多时只在该区域内部滚动，不再无限增加首页高度。
- 风险标题、说明和“打开维护中心”操作保持在视口外；展开后的最近游戏保护明细继续使用独立的 `OverviewProtectionItemsScrollViewer`，`MaxHeight=190`，两个滚动层分别承担总风险列表和明细列表的有限视口职责。
- 保留 `OverviewRiskScrollViewer` 的 `Panel` 兼容节点和真实 `RecentProtection.Items`/`AttentionFindings` 绑定，未改变业务数据、命令或项目滚动条模板。
- 增加源码契约测试，防止后续误删风险区高度边界或把底部操作重新塞入可增长列表。

**验证结果：**

- Playnite 测试：249 通过、61 跳过、0 失败；Core 59/59；Worker 191/191。
- `scripts/validate-source.py`：通过。
- `validate_wpf_ui.py`：0 error，20 条既有 warning，161 条 info。
- RenderHarness：`render-qa OK`；浅色/深色、1040×700/1100×720/1366×768/2560×1440 和缩放过渡均通过，报告中的 `OverviewRiskViewport` 固定为 330 DIP 且出现 Auto 纵向滚动。
- `git diff --check`：通过。

**验证边界：**

- 本阶段仍未把离屏 RenderHarness 结果冒充 Playnite 真机视觉验收；真实宿主截图需要在可识别的 Playnite 页面窗口中单独复核。

## 2026-08-19 UI-239 媒体中心恢复 Demo 指标卡与分段 Tab

**问题确认：**

- 媒体中心顶部虽然已经不是浅灰透明条，但生产页面仍把统计数字、RadioButton 模式切换和指标混在同一条承载层中；这与 AcrylicFork Demo 的四张独立指标卡、独立分段 Tab 不是同一套页面结构。

**实现内容：**

- `MediaCenterView.xaml` 将顶部统计恢复为四张独立的 `GscRedesignMetricBorder` 指标卡：当前游戏媒体、媒体占用、已收藏、待归类。
- 主内容改用共享 `MediaTabControl`，其基础样式为 `GscRedesignWorkspaceTabControl`/`GscRedesignWorkspaceTabItem`，选中 Tab 使用生产紫色强调令牌；删除旧的 `MediaModeStrip`、`MediaModeRadio` 和 RadioButton 事件承载层。
- `MediaCenterView.xaml.cs` 保留真实 Tab 内容、Binding、Command 和项目滚动条，只增加指标卡在 4/2/1 列之间的响应式重排。
- 更新媒体页面源码契约测试，明确约束 Demo 的指标卡和共享紫色分段 Tab 结构。

**验证结果：**

- `git diff --check`：通过。
- `scripts/validate-source.py`：通过。
- `validate_wpf_ui.py`：0 error，20 条已有布局 warning，161 条 info。
- Playnite 测试：249 通过、61 跳过、0 失败。
- Release RenderHarness：`render-qa OK`；1040×700、1100×720、1366×768、2560×1440，浅色/深色和缩放过渡均完成，媒体三 Tab 均可渲染。
- 离屏 PNG 已人工检查：媒体页面显示四张独立指标卡，Tab 选中态为紫色分段样式；这不是 Playnite 真机截图。

**验证边界：**

- 仍需在当前 Release 安装包中用可识别的 Playnite 页面窗口复核媒体页和首页，不能用 RenderHarness 离屏图替代宿主验收。

## 2026-08-19 UI-240 首页风险侧栏展开态可读性收口

**问题确认：**

- 真实 Playnite 窗口中，风险与提醒在展开“最近游戏保护明细”后，预览卡、状态气泡、说明和操作按钮争用同一条侧栏空间；最大化窗口时明细仍会被窗口底部裁切。
- 仅依靠首页根滚动不能解决风险条目数量过多的问题，因此风险总列表和展开明细必须分别拥有有限视口与项目现有滚动条。

**实现内容：**

- 风险侧栏宽度统一为 410 DIP；`OverviewRiskViewport` 根据窗口高度在 500–720 DIP 之间取值，并使用现有 `GscPageScrollViewer` 的 Auto 垂直滚动。
- 展开明细使用独立的 `OverviewProtectionItemsScrollViewer`，根据可用高度在 300–420 DIP 之间取值；风险条目再多也不会无限增加首页高度。
- 展开明细时隐藏同一数据源的只读预览卡，避免同一游戏在侧栏重复出现。
- 明细卡片改为“游戏名 → 状态 → 换行说明 → 查看”四行布局，保留原有选择 Binding、`OpenProtectionItemCommand` 和安全语义；状态文字保持居中且超长时提供 Tooltip。
- `MediaCenterView.xaml` 的本地 Tab 承载层改为透明，继续使用共享紫色 Tab 状态和项目自己的滚动条。

**验证结果：**

- `scripts/validate-source.py`：通过。
- `validate_wpf_ui.py`：0 error，20 条既有 warning，161 条 info。
- `git diff --check`：通过。
- Release 构建与安装通过：Core 59/59、Worker 191/191、Playnite 249 通过/61 跳过/0 失败。
- 在新安装的生产扩展上打开真实 Playnite 窗口（窗口 ID `10621340`），每次滚动和展开操作后都重新获取窗口截图：展开态下重复预览卡已消失；继续滚动后完整看到 Bongo Cat 的游戏名、风险状态、换行说明和“查看”按钮，未发现控件重叠。

## 2026-08-19 UI-241 任务中心合并搜索输入区

**问题确认：**

- 任务中心原先把“搜索任务…”作为独立标签放在输入框左侧，导致真实宿主中搜索框只剩较窄的固定宽度；这与用户要求的可伸展搜索输入区不符。

**实现内容：**

- 删除独立搜索标签列，将搜索提示和搜索图标放入 `TaskSearchTextBox` 的外层输入区；真实 `TaskSearchText` Binding、命令和 AutomationProperties 保持不变。
- 桌面宽度下搜索区占用筛选栏剩余空间；紧凑宽度下搜索区独占第一行，状态、类型和刷新移到第二行，避免筛选控件互相挤压。

**验证结果：**

- XAML structural validation：18 个文件通过。
- Release 构建：0 警告、0 错误。
- Core：59/59；Worker：191/191；Playnite：250 通过、61 跳过、0 失败。
- 一键安装包已成功部署到本机 Playnite，清单与 DLL 版本核对通过。

**验证边界：**

- 最终重新打开 Playnite 做任务中心截图时，电脑控制被物理 Escape 中止；因此本次不能把宿主截图写成已完成，需下次重新取得可识别的 Playnite 窗口后确认实际宽度。

**验证边界：**

- 本阶段验证的是首页风险侧栏的真实宿主展开态；其他页面仍需按各自页面逐页进行真实 Playnite 对照验收，不能据此宣称全量 UI 已与 Demo 完成 1:1 验收。

## 2026-08-19 UI-242 真实 Playnite 任务搜索栏复核

**验证内容：**

- 重新启动并绑定真实 Playnite 生产窗口后，确认打开的是生产 `GameSaveCenter` 扩展，而不是 AcrylicFork Preview。
- 在真实任务中心截图中确认“搜索任务…”已经位于 `TaskSearchTextBox` 内部，搜索输入区横向占据筛选栏剩余空间；“全部状态”“全部类型”和刷新按钮保持独立边界，没有与搜索框重叠。
- 首页真实宿主截图也已重新取得，用于确认生产 PageHost 当前确实加载了最近任务和风险与提醒区域。

**同步修正：**

- `scripts/validate-source.py` 的首页列表有限视口规则按当前实际 XAML 结构修正：`OverviewActivityList` 的有限高度由真实页面滚动/Grid 宿主承载，不再把合法结构误报成缺少视口。

**验证结果：**

- 真实 Playnite 生产窗口截图：任务搜索栏合并布局可见；搜索提示、搜索图标、输入框和右侧筛选控件边界清晰。
- `scripts/validate-source.py`：通过。
- `git diff --check`：通过。

**验证边界：**

- 本次真实截图只覆盖首页入口和任务中心搜索区，不能据此宣称首页风险侧栏、媒体中心、存档中心和维护中心全部完成 Demo 1:1 验收；这些页面仍需逐页取得生产宿主截图并对照。

## 2026-08-19 UI-243 任务搜索与首页风险可读性阶段收口

**实现内容：**

- 任务中心搜索提示与搜索图标已经并入同一个 `TaskSearchTextBox` 外层输入区；搜索框在桌面宽度占据筛选栏剩余空间，紧凑宽度时独占第一行。
- 媒体中心局部 `MediaTabControl` 不再绘制灰色玻璃背景，恢复共享紫色分段 Tab 的选中态和项目既有滚动条。
- 首页最近任务和全局活动的结果/分类气泡设置了明确最小宽度、居中对齐、无换行和 Tooltip；长文本使用省略，运行中进度条使用项目强调色。
- 首页风险侧栏扩展为 410 DIP，风险总列表和展开的保护明细各自使用有限视口；保护明细改为纵向卡片，游戏名、状态、说明和查看操作不再挤在一行。
- 删除生产侧栏末尾无功能的“设置”占位文字，避免出现误导导航或乱码残留。

**验证结果：**

- XAML structural validation：18 个文件通过。
- Release 构建：0 警告、0 错误。
- Core：59/59；Worker：191/191；Playnite：250 通过、61 跳过、0 失败。
- 一键 Release 安装成功，生产清单为 0.6.70，DLL 为 0.6.70.0。
- `git diff --check`：通过。

**验证边界：**

- 本轮 Computer Use 启动 Playnite 后未出现可捕获的唯一窗口，随后已释放电脑控制；因此本轮没有新增有效的 Playnite 真机截图证据。
- 当前阶段不能据此宣称全量页面已经与 AcrylicFork Demo 完成 1:1；存档、修改器、媒体、任务、维护以及首页各状态仍需在可识别的同一 Playnite 宿主中逐页复核。

## 2026-08-20 UI-244 最近任务表头与媒体摘要卡收口

**问题确认：**

- 最近任务的任务名和 DataGrid 表头在部分宿主样式继承下被渲染成黑色，导致深色页面对比度不足。
- 媒体中心顶部四张摘要卡高度和内部留白过大，挤占了来源规则、列表和详情区的首屏空间。

**实现内容：**

- 共享 `DataGridColumnHeader`、`DataGridColumnHeadersPresenter` 和任务中心局部表头同时设置 `Foreground` 与 `TextElement.Foreground`，覆盖 WPF 宿主默认黑色继承；最近任务类型、游戏名、详情和结果文字也显式使用生产主题令牌。
- 媒体中心四张摘要卡继续直接使用共享 `GscRedesignMetricBorder`，统一缩小为 `Padding=10,8`、`MinHeight=72`、圆角 14、数字字号 24 和紧凑间距；没有引入单页私有卡片皮肤。
- 保留 DataGrid 的列宽拖动、排序、虚拟化和项目自身滚动条；这轮没有迁移 Demo 顶部彩色按钮或 Demo 滚动条。

**验证结果：**

- RenderHarness Release 构建：0 警告、0 错误。
- `scripts/render-qa.ps1 -Output artifacts/ui-qa/phase-home-media-cards-final`：`render-qa OK`；浅色/深色、1040/1100/1366/2560 尺寸和缩放过渡均通过。
- `scripts/validate-source.py`：通过；`validate_wpf_ui.py`：0 error；`check-xaml.ps1`：18 个 XAML 文件通过；`git diff --check`：通过。
- Core：59/59；Playnite：251 通过、61 跳过、0 失败。
- 最终离屏截图确认最近任务文字和表头可读，媒体四张摘要卡等比例收缩。未把本轮离屏结果写成 Playnite 宿主真机 1:1 验收。

## 2026-08-20 UI-252 存档历史页补回 Demo 操作卡

**问题确认：**

- 存档中心默认“历史版本”页此前只有分段导航和 DataGrid；Demo 中表格上方的“历史版本 / 版本数 / 立即扫描 / 重新校验 / 刷新详情”操作卡缺失，导致真实扫描、校验和详情刷新入口只在“路径与校验”页出现。

**实现内容：**

- 在 `SaveCenterView.xaml` 的历史版本表格上方增加 `SaveHistorySummaryCard`，版本数绑定真实 `Backups.Count`，当前规则与健康状态绑定真实 `SelectedGame`，三个操作绑定现有 `DetectPathsCommand`、`ValidateCommand` 和 `LoadDetailsCommand`。
- 在 `SaveCenterView.xaml.cs` 增加摘要操作区的响应式行/列重排：700 DIP 以上保持 Demo 式横向布局，低于 700 DIP 时操作区移到第二行并保持按钮可点击。
- 表格仍使用现有 `SaveDataGrid`、固定最小视口、列宽拖动、排序、虚拟化和项目滚动条；没有把 Demo Mock 数据或 Demo 滚动条接入生产。
- 增加源码契约测试，锁定摘要卡和三个真实命令入口不会被后续页面收口误删。

**验证结果：**

- `scripts/check-xaml.ps1`：18 个 XAML 文件通过。
- `scripts/validate-source.py`：通过。
- Release 构建：0 warning / 0 error。
- Core：59/59；Worker：191/191；Playnite：251 通过、61 跳过、0 失败。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/save-history-summary-final`：`render-qa OK`；浅色/深色、1040/1100/1366/2560 尺寸和 resize transition 均通过，历史表在门禁中保持至少 236 DIP 有效视口。
- `validate_wpf_ui.py`：0 error；`git diff --check`：通过。

**验证边界：**

- 本阶段只完成离屏和静态验证，没有新增可识别的 Playnite 生产宿主像素截图；不能把 RenderHarness PNG 当作宿主 1:1 验收。

## 2026-08-20 UI-254 任务与设置页 Demo 结构收口

**实现内容：**

- 任务中心补充 Demo-first 源码契约：四项真实指标、筛选工具栏、更多筛选、任务队列、进度列和右侧 Inspector 的阅读顺序与真实 `TasksView`、`SelectedTask`、重试/取消命令、表格虚拟化保持可追踪。
- 设置页将旧 `TabControl` 页面壳迁移为 Demo 的左侧五项 `LabSegmented` 分类栏 + 右侧单一 `GscPageScrollViewer` 内容区；五个真实设置面板按分类切换可见性，字段绑定、验证、Playnite 保存语义、导入/导出操作不变。
- 设置页响应式逻辑在常见 Playnite 宽度保持 Demo 左栏；仅极窄宿主才把分类栏移到内容上方，并继续保证右侧滚动区拥有有限可测视口。RenderHarness 已支持 `SettingsSectionTabs` 这一 ListBox 分类入口，审计器不再依赖旧 TabControl/头部滚动条。
- 更新源码门禁、布局/资源回归测试和任务结构契约测试；未迁移 Demo Mock 数据，也未替换项目既有 DataGrid/ScrollViewer 滚动条。

**验证结果：**

- `scripts/check-xaml.ps1`：18 个 XAML 文件通过。
- `scripts/validate-source.py`：通过。
- Release 构建：0 警告、0 错误。
- Core：59/59；Worker：191/191；Playnite：253 通过、61 跳过、0 失败。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/task-settings-final`：`render-qa OK`；浅色/深色、多尺寸、五个设置分类、任务宽窄 Inspector 和 resize transition 均通过。
- `validate_wpf_ui.py`：0 error、20 warnings、161 info；`git diff --check`：通过。

**验证边界：**

- 本阶段新增的是 RenderHarness 离屏 PNG 和静态回归证据，目检了设置页 1040/1366 代表分类与任务中心 1040/1366 宽窄状态；没有新增可识别的 Playnite 生产宿主像素截图，不能把离屏结果写成宿主 1:1 验收。

## 2026-08-20 UI-255 工作区表格契约与 Mono 字体收口

**实现内容：**

- 在 `Themes/Redesign.xaml` 增加显式 `GscRedesignWorkspaceDataGrid`，集中承接 Demo `LabGrid` 的行高、表头高度、整行选择、列宽调整、排序、Item scrolling、行/列虚拟化与 Recycling、双向内部滚动和顶部对齐。
- `SaveCenterView`、`MediaCenterView`、`MaintenanceView`、`TaskCenterView` 的真实表格样式改为基于该共享契约；各页仍保留自己的背景、状态行、媒体表头和稳定行样式，未改变 ItemsSource、SelectedItem、命令、列宽响应式逻辑或项目滚动条。
- `GscCodeFontFamily` 改为 `Cascadia Mono, Consolas, Microsoft YaHei UI`，维护中心完整诊断摘要接入该 DynamicResource；所有业务折叠栏继续统一使用 `GscDisclosureCard`。
- 增加跨页源码契约测试，并同步调整既有门禁，使其验证共享样式而不是要求每个页面复制同一组 setter。

**验证结果：**

- `scripts/check-xaml.ps1`：18 个 XAML 文件通过。
- `scripts/validate-source.py`：通过；`git diff --check`：通过。
- Release 解决方案构建：0 警告、0 错误；Core：59/59；Worker：190/190（本轮排除 Soak）；Playnite：251 通过、61 跳过、0 失败。
- `artifacts/ui-qa/shared-grid-contract-final/render-qa-report.txt`：`render-qa OK`，覆盖七页、Light/Dark、多尺寸、多 Tab、表格滚动探针和 resize transition。
- `validate_wpf_ui.py`：0 error、20 warnings、161 info；代表性 Save/Media/Maintenance/Task 离屏截图已目检表头、选中行、状态胶囊、滚动和 Inspector 无遮挡。

**验证边界：**

- 本阶段没有取得新的可识别 Playnite 生产宿主逐页像素截图；RenderHarness、测试和离屏图片不能替代 Playnite 的 Light/Dark/Follow、高对比度、DPI、键盘焦点、真实数据和连续缩放人工验收。总 Demo-first 目标仍未完成。

## 2026-08-20 UI-256 共享按钮与反馈层收口

**实现内容：**

- `Themes/Redesign.xaml` 增加 Demo-first 的共享 Toast/Dialog 资源：Toast 卡片、标题、消息、Dialog 遮罩、Dialog 卡片、标题和消息统一使用浮层材质、圆角、间距和阴影令牌。
- `DashboardView` 的 Toast 仍由现有 `UiNotificationRequested` 事件、计时器、悬停暂停、最多四条和错误详情入口驱动；Dialog 仍保留确认、取消、以后再说、不再提醒、错误详情、Escape 和单实例取消语义。
- 删除 Dashboard 内与 `DesignTokens.xaml` 重复的 `GscButtonBase`/`GscPrimaryButton`/工具栏按钮模板，让原生 `Button` 反馈操作复用全局按钮契约；WPF-UI `ui:Button` 页面按钮继续复用 `WpfUiProduction.xaml` 的共享语义样式。
- 设置页导入报告和错误仍保持可靠的原生 `MessageBox` 模态路径；没有为了视觉统一把 Window-wide ContentDialogHost 或不稳定 Snackbar 强行嵌入 Playnite 页面。
- 增加源码契约，锁定共享反馈资源、Dashboard 真实通知/确认事件和按钮模板去重不会被后续页面迁移误删。

**验证结果：**

- `scripts/check-xaml.ps1`：18 个 XAML 文件通过；`scripts/validate-source.py`：通过；`git diff --check`：通过。
- Release 解决方案构建：0 警告、0 错误；Core：59/59；Worker：190/190（排除 Soak）；Playnite：252 通过、61 跳过、0 失败。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/feedback-surfaces-final`：`render-qa OK`，七页、Light/Dark、1040/1100/1366/2560、多分类/多 Tab 和 resize transition 均通过。
- `validate_wpf_ui.py`：0 error、20 warnings、161 info；未取得新的可识别 Playnite 宿主逐页像素截图，离屏证据不能替代宿主 1:1 验收。

## 2026-08-20 UI-257 首页有限视口修复与真实宿主加载复核

**问题确认：**

- 真实生产 `DashboardView` 的受控宿主截图在 1366/1600 宽度暴露了首页右侧“当前游戏”卡片及操作区被窗口边界裁切的问题；原因是首页根 `ScrollViewer` 禁止横向滚动，却仍可能以无限横向约束测量子内容，星号列按期望宽度增长。

**实现内容：**

- `OverviewLayoutGrid` 绑定 `OverviewStackScrollSurface.ViewportWidth`，并使用左对齐的有限宽度布局；保留首页现有页面滚动、当前游戏选择器、备份/刷新/关注项命令和卡片内真实操作，不引入 Demo 横向滚动条。
- 增加源码契约，锁定横向滚动禁用时必须绑定有限 viewport，避免后续页面结构调整再次把真实按钮推到宿主可视区之外。

**验证结果：**

- `scripts/check-xaml.ps1`：18 个文件通过；`scripts/validate-source.py`：通过；`git diff --check`：通过。
- 最终隔离 Release 构建：0 警告、0 错误；Core：59/59；Worker：191/191；Playnite：256 通过、62 跳过、0 失败。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/overview-responsive-ui257`：`render-qa OK`；Light/Dark、多尺寸、页面滚动、表格探针和 resize transition 全部通过。1366 截图中首页 workspace 为 1042 DIP，当前游戏卡片完整位于 x=520..1026，三个操作按钮均可见；1600 截图同样无右侧裁切。
- `validate_wpf_ui.py`：0 error、20 warnings、146 info；warnings/info 为既有滚动测量、Canvas、负 Margin 与共享颜色审计提示，本轮未新增错误。

**真实宿主边界：**

- `scripts/real-host-audit.ps1` 三次完成生产 0.6.70/0.6.70.0 安装并加载，最新输出为 `artifacts/ui-host-audit-ui257-final`；日志确认真实插件读取 3 个 Playnite 游戏、50 个任务、100 个 finding、30 个媒体。但 Playnite 主窗口仍返回 `EmptyWindowAutomationPeer`，脚本无法通过 UIAutomation 找到嵌入导航项，因此只生成了受控 `DashboardView` 截图，未取得可识别的嵌入式 Playnite 逐页像素证据。
- 受控截图证明生产页面本身的当前游戏卡片裁切已修复，但不能写成 Playnite 嵌入宿主 1:1 视觉验收。七页 Demo-first 总目标仍未完成，后续继续取得同一 Playnite 宿主下的逐页证据并复核真实操作路径。

## 2026-08-20 UI-258 真实 Playnite 生产宿主七页逐页复核

- 通过 Computer Use 在已安装的生产扩展 `GameSaveCenter 0.6.70.0` 中实际打开并导航了七个目标页面；窗口标题显示 `GameSaveCenter 生产版`，当前游戏为 `Bongo Cat`，Worker 与 Ludusavi 状态均可见。
- 首页实际显示当前游戏选择器、刷新/全部备份/同步媒体/查看需关注项、Hero/当前游戏卡片、指标、最近任务和风险提醒；存档页实际显示立即备份/全部备份以及历史、路径、策略、比较标签；媒体页实际显示 30 项媒体、5.76 MiB、待归类 4468 项和来源规则。
- 媒体“待归类”页实际选中待处理截图并滚动独立 Inspector，确认预览、归类游戏选择器、“确认归类”和“忽略并保留副本”均可达；本次只查看，没有执行归类、忽略或保留副本，待归类数量未被改变。
- 任务页实际显示 50 条任务、运行中 0、需关注 16、今日完成 34，并显示搜索/状态/类型筛选和选中任务详情；修改器页实际显示已绑定工具列表、导入入口、工具设置和启动/保存设置操作；维护页实际显示诊断、设备状态、保留策略、异常与审计、进程映射，并进入诊断页确认环境检查与诊断操作区域可见。
- 设置通过 Playnite 游戏右键 `GameSaveCenter → 打开设置` 实际打开，显示 `GameSaveCenter 设置`、常规与目录分类、Worker/Ludusavi/存档目录字段及保存/取消按钮；关闭时未修改设置。
- 这组截图来自真实 Playnite 生产嵌入宿主，不是 RenderHarness 或受控 Dashboard。自动 `real-host-audit.ps1` 仍因主窗口 `EmptyWindowAutomationPeer` 无法生成 `summary.json`，所以这里记录为人工嵌入复核，不改写自动审计门禁。七页页面入口与关键真实数据的本轮人工覆盖已完成；不同 DPI、Follow/高对比度、键盘焦点及执行真实备份/归类/忽略等操作仍需单独验证。

## 2026-08-20 UI-259 媒体收件箱共享虚拟化契约收口

**问题确认：**

- `MediaInboxGrid` 虽然继承 `GscRedesignWorkspaceDataGrid`，但页面实例曾覆盖为 `VirtualizationMode=Standard` 并关闭列虚拟化；这与 Demo `LabGrid` 对齐的共享表格契约和大媒体库性能底线冲突，也让 WPF 静态审查产生媒体表格虚拟化警告。

**实现内容：**

- 删除媒体收件箱实例上的行虚拟化、`Standard` 模式和 `EnableColumnVirtualization=False` 覆盖，恢复 `GscRedesignWorkspaceDataGrid` 的 Recycling、Item scrolling、行/列虚拟化、排序和列宽调整；真实 `ItemsSource`、`SelectedItem`、Inspector、预览与归类/忽略/保留副本命令不变。
- RenderHarness 新增 60 项收件箱夹具和 `Media-Inbox` 五种视口高度滚动探针，同时把列虚拟化作为表格回归门禁；Playnite 源码契约改为验证共享 Recycling 契约并禁止旧 workaround 回归。

**验证结果：**

- `scripts/check-xaml.ps1`：18 个 XAML 文件通过；`scripts/validate-source.py`：通过；`git diff --check`：通过。
- Release 解决方案构建：0 警告、0 错误；Core：59/59；Worker：191/191；Playnite：256 通过、62 跳过、0 失败。
- `scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/media-virtualization-fix`：`render-qa OK`；浅色/深色、1040–3840 宽度、三 Tab、resize transition 和表格探针通过。`MediaInboxGrid` 在 287/311/337/353/419 DIP 高度及 0/25/50/75/100% 滚动位置均保持 Recycling、列虚拟化、首行无空洞、末行可达。
- `validate_wpf_ui.py`：0 error、19 warnings、146 info；本次移除了媒体 DataGrid 虚拟化警告，未新增布局错误。

**真实宿主边界：**

- 本阶段只改变收件箱继承的表格虚拟化契约，没有重新安装并执行真实媒体操作；UI-258 的真实 Playnite 七页入口和媒体 Inspector 可达证据仍有效。不同 DPI、Follow/高对比度、键盘焦点，以及备份/归类/忽略等真实操作仍是总目标剩余人工边界。

## 2026-08-20 UI-260 清理存档页 Demo 游戏名残留

**问题确认：**

- 真实 Playnite 生产页的存档中心标题副文案硬编码为 `Elden Ring · 路径与恢复点状态`，而页头当前游戏选择器显示真实选择的 `Bongo Cat`；这是 Demo 示例数据进入生产壳的残留。

**实现内容：**

- `AcrylicProductionShellView.xaml.cs` 抽取 `UpdatePageHeader`，存档副文案改为读取 `DashboardViewModel.SelectedGame.Name`，未选择游戏时显示“未选择游戏”。
- 壳体监听 `SelectedGame` 变化，切换当前游戏后立即更新页头；其它页面标题、副文案、导航和真实命令保持不变。
- 增加 `ProductionSaveSubtitleUsesTheSelectedGameInsteadOfDemoData` 源码契约测试，防止示例游戏名再次进入生产页。

**验证结果：**

- `scripts/validate-source.py`：通过；`scripts/check-xaml.ps1`：18 个 XAML 文件通过；`git diff --check`：通过。
- 定向 Playnite 源码契约：14/14 通过。
- `dev-install-run.ps1 -Configuration Release -NoStart`：XAML 18/18、Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 257 通过/62 跳过/0 失败，安装清单 0.6.70、DLL 0.6.70.0 校验通过。

**真实宿主边界：**

- 修复前的真实 Playnite 截图确认了 Bongo Cat 选择器与 Elden Ring 副文案不一致；修复后已重新安装当前 DLL，但本轮 Computer Use 在重启后的 Playnite 窗口短暂返回“foreground window did not report a process id”，因此不伪造一张修复后像素截图。
- GSC-086 的当前宿主回归已另外完成：4468 条媒体收件箱数据下顶部/中部/底部/快速滚轮/返回顶部未出现白色空视口；125%/150% DPI、Follow/高对比度、键盘焦点和真实备份/归类/忽略仍未完成。

## 2026-08-20 UI-261 按项目现有样式回滚工作区 Tab 栏

**问题确认：**

- 用户明确要求生产页 Tab 栏不直接沿用 Demo 的外层连续分段胶囊；当前项目原有 Tab chrome 更符合生产 UI，需回滚此前迁移到 Demo 风格的共享页签视觉。

**实现内容：**

- `Themes/Redesign.xaml` 的 `GscRedesignWorkspaceTabControl`/`GscRedesignWorkspaceTabItem` 保留现有工作区页签资源键和 TabControl/TabItem 结构，但改回项目原有的透明页签带、独立横向滚动、11 DIP 圆角、选中强调色和焦点视觉；不再使用 Demo 的连续外层圆角分段容器。
- Save、Media、Maintenance 的真实 Tab headers、SelectedIndex、内容区域 Stretch、绑定和页面命令不变；`GscInternalTabControl` 继续共享该契约。
- RenderHarness 度量探针现在为合法重复模板部件名分配稳定序号，维护页嵌套 TabControl 不会因 `HeaderScrollViewer` 同名而误报重复键。

**验证结果：**

- `scripts/validate-source.py`：通过；`scripts/check-xaml.ps1`：18 个 XAML 文件通过；`git diff --check`：通过。
- 定向 `RestoredAcrylicForkBaselineTests`：15/15 通过；`scripts/render-qa.ps1 -Configuration Release -Output artifacts/ui-qa/project-tab-chrome-rollback`：`render-qa OK`，浅色/深色、多窗口尺寸、维护页嵌套页签及 resize transition 均通过。
- 离屏代表截图已复核 Save、Media、Maintenance：页签不再是 Demo 的外层连续胶囊，表格/Inspector/滚动区域仍完整可见。
- Tab 回滚后的完整 `dev-install-run.ps1 -Configuration Release -NoStart` 也已通过：Release 0 warning/0 error、Core 59/59、Worker 191/191、Playnite 258 通过/62 跳过，安装清单 0.6.70、DLL 0.6.70.0；WPF validator 为 0 error、19 warnings、161 info。

**真实宿主边界：**

- 本轮 Tab 样式变更尚未取得重启后 Playnite 的稳定前台截图；之前重启后的 Computer Use 窗口曾返回 `foreground window did not report a process id`，因此不能把离屏截图写成真实宿主 Tab 像素验收。
- 仍需在真实 Playnite 中复核 125%/150% DPI、窗口缩放、Follow/高对比度、键盘焦点，以及真实备份/媒体归类操作；Demo-first 七页总目标保持进行中。
