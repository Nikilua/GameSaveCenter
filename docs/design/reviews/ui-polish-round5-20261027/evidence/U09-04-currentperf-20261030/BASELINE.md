# U09-04 当前源码固定轮次性能复测：仍进行中

当前冻结候选的生产文件与RenderHarness源码等同于`5c0c57b06aed27dc46c41ef3d655ec20dafa5ec7`；当前记录提交`c6af13d64c2166ee95f0170df111197e38c71beb`之后只有文档变化。与e6候选相比，生产源码唯一差异是共享`Themes/Redesign.xaml`页签模板。当前SDK8.0.423 Release测量探针构建exit0（0 warning/0 error），三个固定轮次都完成70周期、297操作和30秒settle、exit0。探针只临时改动RenderHarness，已归档[`Program.cs`补丁包](probe/Program.cs.instrumentation.zip)、辅助探针源码与哈希；解压后的补丁对原始文件`git apply --check`通过，生产源码未改。

协议为受控透明度0.01的WPF STA窗口、七页面、1040×700 DIP、双主题、无强制GC；每轮活动约47.4–48.8秒。当前三轮显式固定`GSC_RENDER_SAMPLE_UTC=2026-10-28T09:00:00Z`，报告为`pinned=True`。此前e6报告显示同一时间值，但记录为`pinned=False`；时间字段一致、固定状态标记不同，比较时保留此差异。尝试从e6提交重建同一探针未能生成可运行文件：便携SDK8.0.423缺少`Microsoft.NET.SDK.WorkloadAutoImportPropsLocator`和`Microsoft.NET.SDK.WorkloadManifestTargetsLocator`目录，e6重建exit1，原始MSB4276诊断已归档；未换SDK、未关闭检查。

| 指标 | e6历史两轮 | 当前候选三轮 | 观察 |
| --- | --- | --- | --- |
| 活动期托管分配 | 8.8767–8.8769 GB | 8.8775–8.8779 GB | 差约0.011%，基本持平 |
| Rendering回调间隔p95 | 286.317、305.010 ms | 354.448、338.073、360.335 ms | 当前样本均较高 |
| Rendering间隔>33.3 ms | 144/998（14.4%）、143/863（16.6%） | 143/710（20.1%）、143/697（20.5%）、135/657（20.5%） | 当前比例较高；不是实际呈现帧计数 |
| 工作区切换逻辑布局中位数 | 282.96、285.54 ms | 293.48、287.52、292.48 ms | 中位数接近 |
| 工作区切换逻辑布局p95 | 388.30、413.79 ms | 709.49、832.56、854.35 ms | 当前尾部在三轮中重复偏高 |
| 合成搜索文本更新p95 | 6.96、6.38 ms | 6.31、7.14、6.17 ms | 接近 |
| 滚动偏移逻辑更新p95 | 31.06、31.50 ms | 33.18、31.33、30.90 ms | 接近 |
| 详情打开/关闭p95 | 打开39.04、38.07；关闭18.91、24.29 ms | 打开42.08、39.62、37.75；关闭20.34、19.01、20.52 ms | 接近 |

逐页拆分见[`page-breakdown.csv`](page-breakdown.csv)，原始每轮探针CSV与报告位于`measurement-*`；三份原始stdout保存在[`console-logs.zip`](console-logs.zip)，压缩前逐文件SHA见[`console-log-hashes.json`](console-log-hashes.json)。各页面布局中位数接近：e6为38.00–69.28 ms，当前为39.55–70.48 ms；当前逐页最大布局样本为649.76–767.25 ms（每页30个样本），e6为54.52–220.22 ms（每页20个样本）。这里列最大值而非逐页p95，因为每页每轮只有10个样本，逐页nearest-rank p95等于最大值。测量提示出当前候选存在重复的逻辑布局尾部差异，但不能据此把变化归因于页签模板或动画；外部调度、GC和WPF布局耗时没有被单独隔离。

[`comparison.csv`](comparison.csv)列出每轮的全部延迟和分配数据；[`preparation.json`](preparation.json)、各`identity.json`与[`files.json`](files.json)记录提交、SDK、程序集和SHA。e6原始测量见[冻结记录](../U09-final-e6e7f3b9/BASELINE.md)。

此探针在离屏受控WPF窗口运行，不是Playnite宿主；`CompositionTarget.Rendering`回调间隔不等于合成器/VSync/物理呈现帧，程序化文本更新不等于实体键盘或IME输入。U00原始逐帧及累计分配基线仍缺失，真实宿主渲染、物理输入与DPI边界仍待验。U09-04保持“进行中”，当前结果是离屏性能证据和待归因的尾部信号，不代表性能验收完成。
