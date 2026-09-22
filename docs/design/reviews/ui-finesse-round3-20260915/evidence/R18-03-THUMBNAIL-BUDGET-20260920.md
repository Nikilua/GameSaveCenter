# R18-03 缩略图滚动预算（2026-09-20）

## 结论

R18-03 在当前分支 `codex/ui-finesse-round2` 的证据提交 `e54d514e`、原始样本校正 `18c5073f` 已满足受控完成条件。复核确认现有 `AsyncThumbnailLoader` 已提供最多 3 路后台解码、96 项进程内 LRU、取消计数和活动解码诊断；`AsyncThumbnailImage` 在不可见/卸载时取消，并用 generation 在成功和失败回调两侧拒绝迟到结果。本阶段没有重建加载器或修改生产 UI。

## 快速滚动、取消与内存结果

使用隔离临时目录生成 120 个合成 PNG，按 10 个连续窗口、每窗口 12 张图片回放：

| 指标 | 实测结果 | 门禁 |
| --- | ---: | --- |
| 请求 / 解码开始 / 解码成功 | 120 / 120 / 120 | 三者一致 |
| 峰值活动解码 | 3 | 不超过 3 |
| 每轮窗口结束活动解码 | `0,0,0,0,0,0,0,0,0,0` | 全部归零 |
| 缓存条目序列 | `12,24,36,48,60,72,84,96,96,96` | 不超过 `96/96` |
| 最大托管堆增量代理 | 90,072 bytes | 测试上限 64 MiB |
| 预取消请求 | 1 次取消 | 仍无活动解码 |
| 替换后最终状态 | `Ready` | 不出现旧 `Missing/Failed` |

R18-03 基准 `1/1`；既有 `AsyncThumbnailLoader/Image` 回归与本项合计 `9/9`。原始托管堆差值为：

```text
managed_delta_bytes=0,59376,63160,60960,0,30904,90072,27128,65008,43552
active_after_windows=0,0,0,0,0,0,0,0,0,0
cache_samples=12,24,36,48,60,72,84,96,96,96
```

托管堆值使用 net472 可用的 `GC.GetTotalMemory(false)` 前后非负差值，未强制 GC；64 MiB 是隔离合成夹具的回归上限，不是全宿主内存证明，也不替代 ETW/私有字节/显存或物理呈现测量。

## 迟到错误结果负例

真实 STA WPF `Window` 中创建生产 `AsyncThumbnailImage`，先提交不存在的旧路径，立即替换为有效的 64×64 合成图片，等待新图片 `Ready` 后继续泵 Dispatcher 150ms。最终状态保持 `Ready`，观察状态序列不含 `Missing` 或 `Failed`，说明旧请求的失败回调没有写入已复用的新图片行。既有 `ReplacingPathNeverLeavesTheOldImageVisible` 同时验证新路径像素内容；本轮将 c17 引入的 `64×64 px` 旧断言校正为当前 `PreviewWidth=96`/`DecodePixelWidth=96` 的实际 `96×96 px`，没有改生产加载器。

## 门禁与边界

- `R18ThumbnailBudgetTests` + `AsyncThumbnailLoaderTests` + `AsyncThumbnailImageTests`：`9/9`。
- `scripts/validate-source.py`：通过；`scripts/check-xaml.ps1`：`24/24`；`git diff --check`：通过。
- Release 隔离 solution（生产实现未变，基于 `e54d514e`）：`0 errors / 2 existing warnings`，警告为既有 `MediaCenterView.xaml.cs:664` nullable；Playnite 目标仍为 `net462`；`.tmp/r18-03-solution` 已清理。
- 本阶段无 XAML、主题资源或页面视觉树改动，WPF 静态质量基线沿用 `0 errors / 27 warnings / 162 info`。
- 只用合成图片、隔离临时目录、后台 loader 和受控 STA WPF 窗口；未读取真实媒体、存档、用户云端或诊断目录。Demo 原目录不可用，沿用恢复生产基线。
- 未验真实 Playnite/package-host 快速滚动、真实媒体尺寸分布、DPI/跨屏、presented frame、显存、UIA/读屏、ETW 或宿主性能；不把离屏/受控窗口写成物理跨屏或真实帧率证据。

下一可执行小批量：R18-04“表格容器预算”，先复用现有 DataGrid/ListBox 虚拟化和 RenderHarness 容器计数，在 2k/10k/20k 数据下记录真实容器上限与滚动更新成本。
