# 用户 Playnite 慢启动与缩略图卡顿：当前 main 定向复核

日期：2026-09-26

代码基线：`a1544da2810f34e0f4c6445b92a577abaa4ec131` (`main`)
范围：复核历史启动/首屏诊断对应的当前实现与自动回归；不改产品代码、不启动 Playnite。

## 历史问题与当前实现

原始诊断记录的 Playnite 冷启动约 `39.22 s`，GameSaveCenter 插件初始化窗口约 `0.63 s`；打开 Dashboard 的 IPC snapshot 约 `977 ms`、UI apply 约 `45 ms`，旧缩略图 decode 样本约 `200–534 ms`。这些测量说明冷启动总时长不能归因给插件初始化，但首次 Dashboard 和缩略图加载仍可能造成可感知等待。

当前源码已经包含以下修复，均在此轮前合入：

- `GameSaveCenterPlugin` 对大型库后台预热 Worker；库未就绪时延后启动，目录同步不因打开扩展而自动执行，等待显式用户意图。
- `DashboardService.GetAsync` 首帧使用已缓存的 Ludusavi 版本，不等待外部进程；缺失/过期版本由有并发门控的后台任务探测，六小时缓存与失败隔离保留。
- 媒体卡片使用 `MediaThumbnailPreview`/`AsyncThumbnailImage`；详情预览也用 `AsyncThumbnailImage`。`AsyncThumbnailLoader` 将文件探测和解码置于后台，最大 3 路并发、96 项 LRU 缓存、有限解码宽度，图片冻结后交给 UI，并传播取消。
- `DashboardView.xaml` 与 `MediaCenterView.xaml` 仍声明 `MediaThumbnailConverter` 资源，但当前检索未发现任何绑定使用该 converter；媒体 XAML 的实际缩略图入口是上述异步控件。

## 当前基线验证

先以当前 HEAD 身份构建 `GameSaveCenter.Playnite.Tests` Release/no-restore：`0` error，保留两条已有 `MediaCenterView.xaml.cs:703 CS8602` warning。随后在身份匹配的程序集上，分进程运行以下定向回归：

| 回归范围 | 通过 | 失败 | 跳过 |
| --- | ---: | ---: | ---: |
| 大型库预热、首次 Dashboard/版本探测与 Worker 忙碌保护（6 个用例） | 6 | 0 | 0 |
| `AsyncThumbnailLoaderTests` | 6 | 0 | 0 |
| `AsyncThumbnailImageTests` | 2 | 0 | 0 |
| `R18ThumbnailBudgetTests` | 1 | 0 | 0 |
| `R09ThumbnailPlaceholderBehaviorTests` | 2 | 0 | 0 |
| 合计 | **17** | **0** | **0** |

这组结果复核现有实现，不是实际 Playnite 冷启动时间或 displayed-frame/ETW 掉帧测量。未启动 Playnite，未访问用户 profile/存档/媒体，也未重试已有 CEF `platform_channel 0x5` 状态。R 表总基线仍为 192 个唯一 ID，状态计数不变。

## 结论与后续

历史用户问题对应的代码修复和当前自动回归均存在，当前没有由本轮证据支持的新增产品代码缺口，因此不重复造轮子或改写 R 状态。真实宿主启动与呈现性能仍须等到 R23-04/R23-05 环境前置变化后验证；若用户能提供新复现、当前日志或新的性能采样，再按新增证据开产品修复阶段。
