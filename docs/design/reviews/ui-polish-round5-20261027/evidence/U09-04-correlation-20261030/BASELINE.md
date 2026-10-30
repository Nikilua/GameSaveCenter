# U09-04 布局尾部统计与补充探针诊断

## 已有三轮数据复核

本记录只分析当前候选已有的三轮原始数据，源码候选为 5c0c57b06aed27dc46c41ef3d655ec20dafa5ec7；当前工作树 HEAD=4e0fb814f7ec2040383a9f34e197790468d6f599 的生产与测试树和该候选一致。原始协议、SDK、固定周期和限制见[当前固定轮次记录](../U09-04-currentperf-20261030/BASELINE.md)。下面逐布局样本分析来自该证据目录中的 parts.csv 与 performance-frames.csv，逐项数据与原始文件 SHA 见 [analysis.json](analysis.json)，复算脚本为 [analyze-currentperf.py](analyze-currentperf.py)。

| 当前轮次 | 布局样本 | p50 | p95 | 最大值 | >300 ms | 布局时长/分配 Pearson r | Rendering 间隔 p95 | >100 ms 回调间隔 |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 1 | 70 | 46.88 ms | 472.10 ms | 767.25 ms | 4 | 0.318 | 354.448 ms | 139 |
| 2 | 70 | 47.48 ms | 649.76 ms | 741.18 ms | 7 | 0.204 | 338.073 ms | 136 |
| 3 | 70 | 47.76 ms | 620.42 ms | 753.11 ms | 6 | 0.153 | 360.335 ms | 132 |

210 次布局中有 17 次超过 300 ms，分布在七个页面和 Light/Dark 两个主题；SaveCenter 4 次、TrainerCenter 6 次，其余五页合计 7 次。各轮的慢样本页面与主题有变化。布局分配与耗时的 Pearson r 为 0.153–0.318；慢样本分配量约 2.63–9.89 MiB。这个相关性仅是描述统计：数据不支持把所有尾部解释为分配量，也不能排除分配或停顿参与其中。原记录没有布局起止时间和 GC 计数，无法把慢布局同长 Rendering 间隔或 GC 事件对齐。

上一轮对照中，当前活动期托管分配为 8.8775–8.8779 GB，e6 为 8.8767–8.8769 GB；合成搜索 p95 为 6.17–7.14 ms，对照 6.38–6.96 ms；滚动 p95 为 30.90–33.18 ms，对照 31.06–31.50 ms。工作区切换中位数接近，p95 与 Rendering 回调尾部高于 e6。当前 p95 统计基于每轮 70 个布局样本；e6 两轮的逐页样本数为 20，当前为 30。e6 样本 pinned=False、当前 pinned=True，因此不能声称两组为完全相同的测量条件。Rendering 回调不是物理呈现/VSync 帧；搜索更新是程序化 WPF 逻辑代理，不是实体键盘或 IME 响应。

## 时间戳与 GC 补充探针未能构建

为查明尾部是否发生在布局调用期间并与 GC 同步，本批在隔离的临时 RenderHarness 副本准备了每个布局操作的 Stopwatch 起止时间及 GC.CollectionCount(0..2) 差值记录。源补丁和探针源码在 [probe/](probe/)；它们尚未编译，也没有产生新测量数据，不计通过。

使用先前成功编译当前候选的 SDK 8.0.423 路径后：

- 隔离快照的 dotnet restore 返回 1。详细日志确认 MSB4276：便携 SDK 缺少 Microsoft.NET.SDK.WorkloadAutoImportPropsLocator 与 Microsoft.NET.SDK.WorkloadManifestTargetsLocator 两个目录。
- 用当前工作树复制的旧 project.assets.json 做 --no-restore 编译也返回 1，NETSDK1064 指向该文件记录的已清理 NuGet 路径；所需 Newtonsoft.Json 13.0.3 在用户级缓存中存在。重新指定该缓存路径后，restore 仍因上述 SDK locator 错误失败。
- 原始文本日志、二进制日志、资产路径清单与未编译补丁均留在本目录。未更换 SDK、补造 SDK 目录、改依赖版本、删断言或调整门禁；生产源码与项目依赖未修改。

## 结论与后续

U09-04 保持“进行中”。现有数据确认了重复的离屏长尾信号，但无法归因到特定页面模板、页签动画、GC 或外部调度；因此本批不改动画时长来掩盖测量结果，也不据此宣称回归已经定位或验收通过。

可执行的下一步是取得同版本且包含必要 SDK locator 的 SDK 8.0.423 环境，并使用正确的 NuGet 资产路径重新构建临时探针；构建后按原协议再跑三轮，核对布局时间戳、GC 增量和 Rendering 间隔的重合。若 U00 原始逐帧/累计分配、真实 Playnite 呈现、实体键盘/IME 或物理 DPI 证据仍不可得，继续明确保留这些验收边界。全链、scripts/build.cmd 与 GitHub 双 SDK 门禁的既有冻结证据仍只绑定 5c0c57b0，本诊断不替代新源码候选门禁。
