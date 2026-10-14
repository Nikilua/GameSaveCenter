# Q25-01 当前 main 热态响应复核

## 结论

旧报告绑定 `1a58e4b`。当前 main 已在后续实现中加强 2,000 项搜索基准；本次按精确提交 `a78c46b59836bb4dfb408e4bdd467b6e1597e1e4` 重跑现有实现，没有修改性能或生产代码。Q25-01 的本机受控响应条件已满足；Round2 账本记“已满足”。没有视觉改动，因此视觉栏不适用；真实 Playnite、物理键鼠到屏幕的响应和屏幕呈现帧仍待验，宿主外部阻塞、最终结论未完成。

## 构建与身份

- 分支 `main`，精确提交 `a78c46b59836bb4dfb408e4bdd467b6e1597e1e4`。
- 本机隔离 SDK `8.0.423`，MSBuild `17.11.48`，VSTest `17.11.1`，Windows `10.0.22000`；插件 `net462`、测试 `net472`。
- 官方命令：`scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot .tmp/q25-01-main-a78c46b5`；XAML `24/24`，Release solution `0 warning / 0 error`。
- 插件 DLL SHA-256 `12340716F8BF00C959BD0561F2EF85E87EE15FD58A5C6C84FD1F15ACEE5762B0`，MVID `4855726a-6da3-4730-b4ca-e0dc6a09a831`。
- 测试 DLL SHA-256 `C002C447D042B83CAC590C59E37573C6DD5491D8F0EBE4EF9537D363BABC0554`，MVID `a47102e0-5afa-4553-91d1-9777e9e50c6e`。
- 相关源 SHA-256：`LargeLibraryPerformanceTests.cs` `1BA710F71F7CB4629690374591D32AA1AB2FEDE30C9DE38594B28DC0D7EE54F2`；`R18ContinuousInputBenchmarkTests.cs` `82ABB103C349A36AC236005BE56F144E257AC7F7744D285BB7878BDE6887CBE9`；`R18DispatcherVisibilityBenchmarkTests.cs` `B59913875F64B61CAA9B17636B82CD7AA54EB4AE780AFC4D933CDB58EEC55319`；`GamePickerKeyboardBehaviorTests.cs` `D930DE267A6903DDA3280C13C2247391418587EC6DBFDD83B0E3A497AAC0319D`；被测 `GamePickerViewModel.cs` `FFB9DDB390490C67E6DFE210F8C496FF55F02947D63BA5DF3C5A1111DEA4548B`。
- 构建时没有已跟踪的未提交改动；仓库中其他任务的未跟踪证据仍保留且未纳入本包或提交。

## 自动验证与测量

四类隔离测试进程共 `13 passed / 0 failed / 0 skipped`。

| 测试类 | 结果 | TRX SHA-256 |
| --- | ---: | --- |
| `LargeLibraryPerformanceTests` | 5/5 | `67F49424B01B7732A830436B077C8CC16E6705A7AF23068E0276E8C5EA7CE7A4` |
| `R18ContinuousInputBenchmarkTests` | 1/1 | `E36E589A8F5080782E29AD97DEAD5908E8921615D29588B99AF7368F60FED8BA` |
| `R18DispatcherVisibilityBenchmarkTests` | 1/1 | `38E4F218DA2645962E0332229D092E4916D870B60EAF67B1162CA3583BE6E5E9` |
| `GamePickerKeyboardBehaviorTests` | 6/6 | `022697DAB630BAB2EC3AE5D203C2630D0755ED05FF18F47A5418E91F6146A5AE` |

2000 项 GamePicker 热搜索先预热 5 次，再测 30 个不同查询；每个查询都得到对应单项结果，30/30 次实际改变可见 ID 集合。测量为 `SearchText -> FilteredCount`，p50/p95/max=`45/60/60 ms`，测试固定 p95 `≤100 ms` 门禁；原始查询与 30 个样本在[基准文件](benchmarks/large-library.txt)。超时负例在 75 ms 预期窗口内退出，collection 同值不重复通知和变化时只 Reset 也通过。

同一当前身份下的 R18 连续输入试次另外测 2,000/10,000 项各 30 次同步 `RefreshNow`：p95/max 分别为 `2.466/7.788 ms` 和 `7.009/11.059 ms`；总过滤评估 `60,000/300,000`。`GC.GetTotalMemory(false)` 差值峰值 `305,288/1,448,112 bytes` 只是托管堆代理，不是分配栈或 ETW 数据。

R18 Dispatcher 测试用真实 WPF `Window`/`ListBox`、2000 条合成游戏和 20 个不同搜索样本，分别记录 `SearchText` 到 ViewModel 完成、到首个可见 `ListBoxItem` 的时间。将同一 20 个配对原始数组相加，得到输入到可见容器 p50/p95/max=`46.214/75.782/76.334 ms`；VM 部分=`33.261/49.719/60.663 ms`，容器更新增量=`13.532/28.763/30.328 ms`。20/20 项有正高度/宽度的可见容器，几何为 `476×19.24 DIP`。这组是受控 WPF 布局反馈，不是 30 样本，不能混作主 30 样本门禁，也不是 Playnite 或屏幕呈现帧。

配套键盘行为 `6/6` 覆盖可见候选 Enter、Esc 和清除后的焦点返回、无结果 Enter 保留既选项、IMEProcessed/方向键路由，以及合成中文 TextComposition 提交。没有物理键盘或 Windows 输入法证据。

## 边界与 CI

- 所有列表/查询均为合成内存数据；未读取用户库、未写系统剪贴板或用户配置。
- 结果只证明当前受控 testhost/STA WPF 窗口响应；不证明真实 Playnite 宿主、物理键鼠输入、DWM/屏幕 presented frame、60 Hz、低 Tier、ETW 或真实用户库性能。没有生产视觉变更，不生成截图。
- 本机 SDK `8.0.423` 不能替代 CI SDK `10.0.401`。最近公开 Actions 列表仍显示前一文档提交 #979 排队，尚未显示 `a78c46b5` 对应的 run；本包只记录本机结果，等待确认本次两个 CI lane 的实际状态。历史 #970/#971 诊断原因及当前 main SDK9 定向复核另见[CI 诊断包](../CI-970-971-DIAGNOSTIC-TRIAGE-20261014/README.md)。
