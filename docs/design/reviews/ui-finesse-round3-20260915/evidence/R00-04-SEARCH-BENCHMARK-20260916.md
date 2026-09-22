# R00-04 搜索基准真实性证据

日期：2026-09-16  
代码提交：`df884b0`（`校正搜索基准样本`）  
分支：`codex/ui-finesse-round2`

## 结论

R00-04 的代码与受控合成数据验证已完成，账本保持“代码完成待验收”。搜索基准现在为每个正式样本使用不同的 `SearchText`，等待条件同时检查 `FilteredCount` 和真实可见 `PlayniteId` 集合；等待超时使用每次调用新建且正在运行的 `Stopwatch`。错误期望的负例在有限时间内返回失败，不会因调用方传入的已停止计时器而无限等待。

## 验证结果

- 当前隔离 worktree 的 Playnite WPF Release 构建：`0` warning / `0` error。
- R00-04 定向测试：`2/2` 通过。
  - `GamePicker2000_Benchmark_WritesMeasuredTimings`
  - `GamePickerBenchmarkWaitTimesOutOnAnImpossibleExpectedCount`
- 基准使用 2,000 个合成游戏，正式样本 `30` 次；`search_changed_result_sets=30`。
- 每次正式输入的原始查询序列：
  `Game 1999|Game 1998|Game 1997|Game 1996|Game 1995|Game 1994|Game 1993|Game 1992|Game 1991|Game 1990|Game 1989|Game 1988|Game 1987|Game 1986|Game 1985|Game 1984|Game 1983|Game 1982|Game 1981|Game 1980|Game 1979|Game 1978|Game 1977|Game 1976|Game 1975|Game 1974|Game 1973|Game 1972|Game 1971|Game 1970`
- 原始时延样本（ms）：
  `44,45,60,44,44,45,45,44,45,45,45,60,44,45,44,45,46,45,45,44,45,46,45,45,45,45,45,45,45,45`
- 统计结果：`p50=45ms`、`p95=60ms`、`max=60ms`；基准门禁的 p95 上限为 `100ms`。
- 负例使用 1 个项目等待不可能的 0 个结果，`75ms` 超时后返回 `False`，测试总等待保持在 `1s` 以内。

## 纠偏记录

第一次定向运行明确暴露了旧修正仍不足：只等待 `FilteredCount == 1` 时，后续查询会在过滤刷新前提前返回，30 次中仅观察到 `1` 次可见集合变化。`df884b0` 改为等待当前查询对应的可见 ID 集合后重跑，结果才达到 `30/30`。这条失败记录保留在证据中，说明门禁覆盖的是实际集合变化而非单一计数或字符串存在。

## 2026-09-18 当前分支复核

- 当前 HEAD `e5a12ffaea138c3a3efccaeb82c58d339ae7d494` 的 R00-04 定向测试 `2/2` 通过；结果写入隔离 `.tmp/r00-03-04-test-artifacts/ui-qa/benchmarks/large-library.txt`，没有使用真实游戏库。
- 当前原始测量为 `search_measured_samples=30`、`search_changed_result_sets=30`、`p50=46ms`、`p95=47ms`、`max=48ms`；30 个查询从 `Game 1999` 递减至 `Game 1970`，每次输入均改变可见 ID 集合。`first/unchanged/changed_set_ms=46/1/12`，任务集合首替换/不变替换为 `1/0ms`。
- 当前不可达结果负例仍在约 `75ms` 超时内返回 `False`，测试总耗时约 `84ms`；每次等待使用运行中的独立 `Stopwatch`，不会把已停止计时器当作超时依据。
- 因此 R00-04 的当前可控样本真实性、原始序列和有限时间负例已满足；R18-01 仍独立覆盖连续输入、IME、20ms debounce 的过滤次数/分配，真实宿主输入和物理呈现不由本批次签收。

## 范围与边界

该验证使用 `GamePickerViewModel`、fake DTO 和合成 2,000 项数据，结果属于隔离测试进程中的 WPF 逻辑/Dispatcher 等待证据，不是 Playnite 真机输入、IME 候选确认、连续打字节奏、物理 DPI、屏幕呈现帧或 ETW 性能证据。R18-01 仍需独立覆盖连续输入、IME、20ms debounce 的过滤次数/分配；真实宿主性能边界不由本批次升级。

## 2026-09-23 当前身份复核

- 当前 `b5c7a6d423a4bf23004c3b080e133b3b0b065fa5` 隔离 testhost 的 `LargeLibraryPerformanceTests` 为 `5/5`；2000 条合成游戏的正式样本仍为 `30` 次，`search_changed_result_sets=30`，原始摘要为 `p50=46ms`、`p95=48ms`、`max=48ms`。
- 不可能结果的有限超时负例仍通过；每次等待使用运行中的独立 `Stopwatch`。本次只刷新了合成数据和隔离 testhost 证据，未写真实游戏库、未把搜索逻辑样本升级为 IME 或 Playnite 宿主性能结论。
