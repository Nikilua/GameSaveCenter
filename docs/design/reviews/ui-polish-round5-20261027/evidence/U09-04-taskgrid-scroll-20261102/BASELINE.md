# U09-04 / TaskGrid 内部滚动归属复测

## 目的

前一轮耐久探针在 Task 页选择最大可滚高度的可见 `ScrollViewer`，其 52.02 / 42.96 ms 样本属于整页滚动动作，报告没有指明控件，不能归因到 `TaskGrid`。U00 的原始耐久探针也没有单独的滚动测量。本批在两个隔离 Git 源码快照内注入字节一致的临时 RenderHarness 探针，直接从名为 `TaskGrid` 的 `DataGrid` 模板子树内找滚动器，并报告控件归属、候选数、`CanContentScroll`、视口/范围、项目数及可见实现行。临时探针没有进入生产树或提交源码。

## 同条件结果

每个源快照各运行 Light、Dark 一轮；每轮为 60 秒活动和 30 秒收尾，1040×700 DIP、透明度 0.01、窗口置于屏幕外、`FakeDashboardData(60)`、固定样本UTC `2026-11-02T00:00:00Z`、无强制GC。动作固定为 TaskGrid 模板 `ScrollViewer.ScrollToVerticalOffset` 在顶部与 75% 垂直范围间往返，随后同步 `Window.UpdateLayout()`。每轮探针退出码 0，动作失败 0；控件均为 60 项、内部候选滚动器 1 个、可见实现行 7、视口 6、范围 54，且所有运行均实际离开顶部。

| 主题 | U00 样本 / 中位数 / p95 / 最大值 (ms) | 当前样本 / 中位数 / p95 / 最大值 (ms) | p95 差值 |
| --- | --- | --- | --- |
| Light | 428 / 13.847 / 16.526 / 31.858 | 441 / 13.863 / 16.409 / 22.844 | -0.117 ms |
| Dark | 430 / 13.787 / 16.498 / 23.372 | 446 / 13.890 / 16.499 / 28.365 | +0.001 ms |

本次窄化的受控逻辑动作没有显示 TaskGrid 滚动 p95 回退信号。单轮内样本较多，但每个源/主题只有一次独立运行；这些数据不证明通用帧率、GC/累计分配、Playnite 宿主性能或物理输入延迟。此前 36.33→33.81/31.45 ms 的 `scroll_offset` 数字仍只属于“最大滚动器”页面动作，不能当作同一 TaskGrid 指标。

## 构建身份与验证

基线源为 `5d0a2448d6315af61571d0d3fbecfd902e5344fc`，当前生产源为 `f9d09cb2b7665355a74926e2cd4a801567c60baa`；Round5 文档 HEAD `2cbe584f` 与当前生产提交之间的 `src/`、`tests/`、`scripts/` 无差异。两份 RenderHarness 均用仓库 SDK 8.0.423、Release、`.NET Framework 4.7.2` 编译，restore 和 build 均退出 0、0 warning、0 error。首次单项目还原图遍历退出 1 且没有 MSB/NU 诊断；只关闭静态图仍失败。随后改用 `scripts/build.ps1` 采用的 `-m:1 -nodeReuse:false -p:NuGetAudit=false -p:MSBuildEnableWorkloadResolver=false` 参数，基线与当前快照的还原/编译均成功。失败日志、成功构建日志、RenderHarness 与生产程序集 SHA 见 [`identity.json`](identity.json)、[`build/`](build/) 和 [`diagnostics/`](diagnostics/)。U00 与当前快照注入的同一探针片段 SHA-256 均为 `0F2DB69DF22CA5AED900CE84FFA75FB8D43B56BB1272AD8B8BDCD3789426D4AF`。

运行原始报告：[U00 Light](probe/u00-light-1.txt)、[U00 Dark](probe/u00-dark-1.txt)、[当前 Light](probe/current-light-1.txt)、[当前 Dark](probe/current-dark-1.txt)。本批未改生产 UI、资源、命令或 Binding，也未运行安装启动全链；因此不需要重新运行 Render QA。没有验证实体鼠标/触控滚动、UIA、IME、物理 DPI、实际 Playnite 呈现帧或真实任务数据。没有运行恢复、媒体删除或云写入。U09-04 继续“进行中”，等待缺失的 U00 原始逐帧/累计分配证据或可接受的真实宿主复测。

## 临时输出

只清理本阶段唯一自有 `.tmp/round5-u09-04-taskgrid-20261102` 快照和构建目录。归档文件及SHA见[`files.json`](files.json)，阶段空间起点见[stage-start记录](../U09-04-taskgrid-scroll-20261102-stage-start.json)；路径边界/重解析点/进程/文档引用检查、删除清单与清理前后空间见[`cleanup.json`](cleanup.json)。