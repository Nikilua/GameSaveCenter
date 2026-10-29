# U09-01 / U08-03·04 同数据双主题历史配对

## 对照范围

历史生产快照为 `c5b03e576edddfe7a5f06172e081407eb9a5ee62` → `18481e6d8d3415ffc31362764980614e76445848`。目标生产差异只有两处：`MediaThumbnailPreview` 的说明文字绑定到实际 `TextBlock.Text`，以及 `TaskCenterView` 将任务长文本绑定为完整的 UI Automation `HelpText`。任务名仍按原样省略，表格布局、命令、列表虚拟化、媒体解码和业务状态未改。

Render Harness 在两份快照中使用相同的测试专用注入。固定样本时间为 `2026-10-28T09:00:00Z`，任务游戏名及运行时长固定；媒体样本有4张指向不存在文件的截图和2段录像。`FakeSampleClock.cs`、`FakeDashboardData.cs`、`U08PairMediaProbe.cs` 与 RenderHarness 项目文件在两侧SHA256相同。完整仓库快照归档未复制到证据目录；其SHA及大小列于 `pair.json`。

## 视觉与行为结果

两侧以 SDK 8.0.423、Release 分别构建对应 Worker 和 RenderHarness，构建均为0警告、0错误。`scripts/render-qa.ps1` 双侧均退出0，各生成394张PNG且0 `PROBLEM`。逐图比较为390张相同、4张变化；变化仅出现在 Media tab1 的1920×1080、2048×1152、2560×1440和3840×2160输出。详细完整报告及SHA见本目录。

专用探针在浅色、深色各捕获6张缩略卡。修复前4张缺失截图与2段录像的预览文本均为空，修复后分别显示“媒体文件不存在”和“录像预览”；两侧状态仍为 `Missing` / `Video`。运行检查也通过了真实 `MediaCenterView` 中同样的状态。双主题截图：

| 主题 | 修复前 | 修复后 |
| --- | --- | --- |
| 浅色媒体缩略卡 | [before](media-thumbnail-cards-before-light.png) | [after](media-thumbnail-cards-after-light.png) |
| 深色媒体缩略卡 | [before](media-thumbnail-cards-before-dark.png) | [after](media-thumbnail-cards-after-dark.png) |

任务页以相同长标题和样本时间捕获。两主题 before/after 图像SHA完全相同，因为 UIA HelpText 不改变像素；原省略样式保留。任务长文本及媒体状态的后续主线行为证据为 [U08-03/04 当前主线回归](../U08-03-04-current-main-8ac477b9/BASELINE.md)，其中15/15通过、0失败、0 skip；`18481e6d` 到该测试基线的两个目标生产文件没有变化。

| 主题 | 修复前 | 修复后 |
| --- | --- | --- |
| 浅色任务页 | [before](task-page-before-light-1366x768.png) | [after](task-page-after-light-1366x768.png) |
| 深色任务页 | [before](task-page-before-dark-1366x768.png) | [after](task-page-after-dark-1366x768.png) |

按仓库 WPF 质量清单对当前生产子树执行 `validate_wpf_ui.py src/GameSaveCenter.Playnite`，退出0：26 XAML、0 error、26 warning、164 info。媒体大列表保持有限视口及 Recycling 虚拟化（如 `MediaDuplicateGroupsList` 的 `MaxHeight=430`、`MediaSourcePreviewItems` 的 `MaxHeight=240`）；静态扫描对页面内 StackPanel 与滚动控件的邻近关系仍报启发式 warning。本轮不改页面层级、共享模板或列表滚动模型。

首轮修复前完整 Render QA 报告出现 SettingsState hint/summary 断言失败。失败报告和原始控制台日志已保留；分别构建两快照自身的 Worker 后，再运行的两侧完整 Render QA 均通过，未调整断言或跳过门禁。报告中内嵌的 `Commit=d9b750...` 来自承载快照的父工作树，不是历史源码身份；本证据以固定 Git SHA 与快照归档SHA为准。

## 验收边界

该对照是离屏 WPF 结果。仓库指定的 `GameSaveCenter.AcrylicFork/.../Design` Demo 树不在本工作树，未声称完成直接 Demo 视觉比对；真实 Playnite 页面、外部 UIA/Narrator、实体键盘/IME、物理 DPI 与跨屏显示未验。没有运行真实存档恢复、媒体删除或云端写入。U08-03/04 继续“代码完成待验收”，U09-01 继续盘点其余批次。

源快照、构建程序集、Render QA/探针结果身份见 [pair.json](pair.json)；自有临时目录核验、删除及磁盘空间变化见 [cleanup.json](cleanup.json)。
