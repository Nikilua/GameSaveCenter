# Q25 ETW / 呈现工具边界

采集日期：2026-09-14（Asia/Shanghai）。本文件只记录工具可用性与失败原因，不把代理数据写成 Q25-02/Q25-03 通过。

## 工具盘点

- 本机可定位 `xperf.exe`、`wpr.exe`、`wpa.exe`、`wpaexporter.exe`，并能通过 `wevtutil gp Microsoft-Windows-Dwm-Core /ge:true` 查询 DWM provider；provider metadata 包含 `SCHEDULE_RENDER`、`SCHEDULE_PRESENT`、`SCHEDULE_GETPRESENTSTATS` 等任务。
- 本机未定位 `PresentMon`、`dotnet-trace` 或 `PerfView`。

## 冒烟采集结果

- 在 `.tmp` 目标上执行 5 秒 DWM ETW 会话，命令等价于 `xperf -start GscDwmSmoke -on Microsoft-Windows-Dwm-Core:0xFFFFFFFF:5 -f .tmp/gsc-dwm-smoke.etl`，随后计划用 `xperf -i ... -a dumper` 解析。
- `xperf` 在启动会话阶段直接返回 `0x5 / Access denied`；没有建立会话、没有生成可解析 ETL，也没有产生可用于统计的事件样本。失败后确认没有残留 ETW 会话或 `.tmp` trace 文件。

## 对任务的影响

- 当前仍没有 60Hz 实际呈现帧的 p95、最大间隔、慢帧比例，也没有可重复的 UI 线程 `>100ms` 调用栈；Playnite/Worker 日志中的 `[PERF]` 刷新耗时仅记录业务刷新，不替代 DWM/ETW。
- Q25-02、Q25-03 继续未完成；Q25-04 的 30 分钟真实窗口时间序列和 Q25-05 的低性能 Tier 实测也不能由这次工具拒绝推导。后续需要允许 ETW 会话的主机策略或可用的 PresentMon/等价采集工具。
