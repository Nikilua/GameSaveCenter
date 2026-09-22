# R18-03 缩略图滚动预算定向复核

日期：2026-09-23  
当前复核提交：`93b115f4`（`校正R18-02调度器证据`）  
实现提交：`e54d514e`（缩略图滚动预算）  
原始样本校正：`18c5073f`  
分支：`codex/ui-finesse-round2`

## 结论

R18-03 在当前可控范围内继续记为“已满足，待环境验证”。本次没有新增生产代码，复用现有 `AsyncThumbnailLoader` 的最多 3 路后台解码、96 项进程内 LRU、取消计数和诊断，以及 `AsyncThumbnailImage` 的不可见/卸载取消和 generation 迟到结果保护。

- `R18ThumbnailBudgetTests`：`1/1`。
- `AsyncThumbnailLoaderTests + AsyncThumbnailImageTests`：`8/8`。
- 本阶段合计：`9/9`。

## 本次原始样本

使用隔离临时目录生成 120 个 64×64 合成 PNG，按 10 个连续窗口、每窗口 12 张图片回放：

| 指标 | 本次结果 | 门禁 |
| --- | ---: | --- |
| 请求 / 解码开始 / 解码成功 | 120 / 120 / 120 | 三者一致 |
| 峰值活动解码 | 3 | 不超过 3 |
| 每轮窗口结束活动解码 | `0,0,0,0,0,0,0,0,0,0` | 全部归零 |
| 缓存条目序列 | `12,24,36,48,60,72,84,96,96,96` | 不超过 `96/96` |
| 最大托管堆增量代理 | 112,456 bytes | 测试上限 64 MiB |
| 预取消请求 | 1 次取消 | 活动解码仍为 0 |
| 替换后最终状态 | `Ready` | 不出现旧 `Missing/Failed` |

本次 testhost 输出的 `managed_delta_bytes` 为 `0,112456,23160,67736,68288,9624,38488,18696,7592,43200`；活动数和缓存序列按上表保留。数值使用 net472 可用的 `GC.GetTotalMemory(false)` 前后非负差值，未强制 GC；它是合成夹具的托管堆变化代理，不是 ETW、私有字节、显存或物理呈现测量。

迟到错误负例在真实 STA WPF `Window` 中先提交不存在的旧路径，再立即替换为有效图片；新图片达到 `Ready` 后继续泵 Dispatcher，最终状态仍为 `Ready`，观察序列不含 `Missing/Failed`，且当前显示尺寸为 `96×96 px`。这证明旧请求不会回写复用后的图片行，但不等价真实宿主快速滚动。

## 构建与质量门禁

- 当前检出版本隔离 Release solution：`0 errors / 2 warnings`；两条均为既有 `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs:706` 的 `CS8602`，不是本阶段新增。
- 目标 Playnite 产物仍为 `net462`，测试程序集为 `net472`；没有覆盖 `main` 的旧实现或用户文件。
- `validate-source.py`：通过；XAML 结构校验：`24/24`；`git diff --check`：通过。
- `validate_wpf_ui.py src/GameSaveCenter.Playnite`：`0 errors / 28 warnings / 162 info`；本批无 XAML、主题资源或页面视觉树改动。静态警告/信息不替代运行时视觉验证。

## 未验边界

Demo 原目录不可用，本阶段沿用已恢复的生产基线。未验真实 Playnite/package-host 快速滚动、真实媒体尺寸分布、DPI/跨屏、presented frame、显存、UIA/读屏、ETW 或宿主性能；没有把离屏/受控窗口写成物理跨屏或真实帧率证据。测试只用合成图片、隔离临时目录、后台 loader 和受控 STA WPF 窗口，没有读写真实媒体、存档、用户云端或诊断目录。

## 下一步

下一可执行任务为 `R18-04 表格容器预算`：先复用现有 DataGrid/ListBox 虚拟化和 RenderHarness 容器计数，在 2k/10k/20k 数据下记录真实容器上限与滚动更新成本。
