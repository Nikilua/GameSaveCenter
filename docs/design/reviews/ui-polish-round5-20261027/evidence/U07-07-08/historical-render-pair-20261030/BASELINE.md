# U07-07 / U07-08 历史前后渲染配对

本批为 U09-01 补充历史证据，不改生产源码或资源，也不改变两项验收状态。设置页对照 `a76de89f932cd10e912b478616524050ba868c79` → `c5b03e576edddfe7a5f06172e081407eb9a5ee62`；任务页对照 `17d883abd084e3a2a646c1879f45ecbacd6d9dee` → `0aff7f32aab4fb31940fd791c1486c67d36ebdbd`。四个历史快照使用同一个离屏探针，源身份、探针、RenderHarness 程序集和每个文件的 SHA-256/字节数见 [run.json](run.json)。

## 对照结果

- U07-07：自动巡检保持关闭、巡检间隔字段保持禁用。前版没有依赖说明；后版浅色和深色都显示“开启自动巡检后，可调整巡检间隔和重新验证有效期；关闭时保留当前数值。”探针验证说明完整落在设置滚动视口内，并把同一间隔字段对齐到相同位置。
- U07-08：18 条固定样本任务中选中同一条云端上传失败任务，错误码为 `RCLONE_PERMISSION_DENIED`。前版没有重试指引卡；后版浅色和深色都显示权限检查/先核对远端结果的指引。探针同时核对卡片可见及 UI Automation HelpText 与正文完全一致。
- 八张终态图已逐对视觉检查：设置页 `1600×900 DIP`，任务页 `1366×768 DIP`。任务行的全部时间字段由探针固定在同一个 UTC 样本，避免相邻行因两次进程启动时间不同而漂移。

## 构建与捕获

四个历史 RenderHarness 快照均以 .NET SDK `8.0.423`、Release、`--no-restore` 编译，build exit code 均为 `0`；每次有 4 条已记录的 NU1900 漏洞源可用性警告、0 个错误。各版分别运行一次 `settings` 或 `task` 捕获，四次 render exit code 均为 `0`。原始构建日志、运行日志、探针检查输出与固定探针源码均随本证据保存。由于本地 NuGet 漏洞源不可用，保留了警告，没有关闭审核或改 SDK。

首轮 `1040×700` 设置截取发生裁切并捕获到尚未收敛的入场动画，不作为比较证据。最终设置探针关闭该离屏夹具的入场动效，并在前后两版使用相同控件锚点和相同视口位置；无效首轮截图未归档。RenderHarness 使用只读合成数据，不加载用户配置，也不触发设置写入。

最终证据归档并复核后，清理本阶段唯一临时目录 `.tmp/u07-visual-pairs-20261030`：2,708 个文件、125,652,193 字节。目录边界、重解析点、文档引用、进程/文件占用及21项归档资产SHA-256均通过预检；只删除该目录，清单和空间前后数据见 [cleanup.json](cleanup.json)。

## 图像

| 页面/主题 | Before | After |
| --- | --- | --- |
| 设置 / Light | [settings-light-before.png](settings-light-before.png) | [settings-light-after.png](settings-light-after.png) |
| 设置 / Dark | [settings-dark-before.png](settings-dark-before.png) | [settings-dark-after.png](settings-dark-after.png) |
| 任务 / Light | [task-light-before.png](task-light-before.png) | [task-light-after.png](task-light-after.png) |
| 任务 / Dark | [task-dark-before.png](task-dark-before.png) | [task-dark-after.png](task-dark-after.png) |

## 边界

这是 96 DPI 的离屏 WPF 渲染，不代表 Playnite 宿主呈现、实体键盘/IME、外部 UIA/Narrator、物理 DPI 或多屏验证。恢复存档、删除媒体、云端写入均未运行；任务指引只做可见性与 UIA 文本检查，没有执行重试命令。U07-07 与 U07-08 继续保持“代码完成待验收”。
