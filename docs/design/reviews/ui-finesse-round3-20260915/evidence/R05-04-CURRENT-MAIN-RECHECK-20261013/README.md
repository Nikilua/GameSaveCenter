# R05-04 当前 main 多选摘要复核

日期：2026-10-13（Asia/Shanghai）

代码基线：`903525becdb679e9a4aae7bb04e73c98c1cb25dc`。本次只调整现有行为测试夹具，没有修改生产 UI、ViewModel、命令或数据契约。测试源码 SHA-256：`B6D804ADFA3479FBFD765F58F3485E00DC328355C0688571A63725015D476A85`。

## 现有能力与覆盖范围

复核确认继续复用 `MediaCenterView` 已有的 Extended DataGrid 选择、按收件箱模式保存的稳定媒体 ID、窗口追加后的选择恢复，以及提交前的去重/无效 ID 和错误处理。没有重建媒体服务或 DTO。

本次核对了 `R05MultiSelectionSummaryBehaviorTests` 的原始断言。原清空测试只检查 ComboBox 文本；fixture 没有给生产处理器提供当前 `DashboardViewModel.MediaInboxMode`，也没有断言其他模式的选择集合保持不变。现将测试扩为以下实际行为：

1. 生产 `MediaCenterView` 在 STA WPF 窗口中显示 0、1、2 项摘要，并核对清空按钮折叠/显示状态。
2. 稳定 ID 集合含 1 个不可见项时，摘要将总选择 3 项与当前窗口可操作的 2 项分开表达。
3. 处于“已忽略”模式时，点击生产清空按钮会清空该模式的可见和不可见选择、清空 DataGrid 选择并复位摘要；“待归类”模式保留的 ID 不变，ViewModel 与 ComboBox 仍保持“已忽略”。

隔离 fixture 只为生产处理器提供 `MediaInboxMode` 和网格诊断所需主题值，通过未初始化对象避免启动 Playnite 插件服务或后台任务。所有 `MediaItemDto` 使用合成 ID 和隔离路径；没有访问真实媒体、存档、云端或用户配置。

## 本机验证与程序集身份

- 本机隔离 SDK：`.NET SDK 8.0.423`（显式选择 `D:\GameSaveCenterTools\dotnet-sdk-8.0.423`）；仓库 `global.json` 固定同一 SDK。它不替代用户要求单独跟踪的 CI SDK `10.0.401`。
- `dotnet build GameSaveCenter.sln --configuration Release --no-restore -m:1 -nodeReuse:false -p:NuGetAudit=false -p:MSBuildEnableWorkloadResolver=false`：`0 warning / 0 error`；Playnite 目标 `net462`、测试目标 `net472` 均生成成功。
- `dotnet test tests/GameSaveCenter.Playnite.Tests/GameSaveCenter.Playnite.Tests.csproj --configuration Release --no-build --no-restore --filter FullyQualifiedName~R05MultiSelectionSummaryBehaviorTests`：`3 passed / 0 failed / 0 skipped`。脱敏 TRX：[R05-04-current-main.trx](tests/R05-04-current-main.trx)，SHA-256 `DAD4A106C61943A47CFDFC5E7BA27EC50C77EB58BB83F2D634A297F5CC4DA5F4`。
- `python scripts/validate-source.py` 通过；`git diff --check` 通过。
- 测试程序集 SHA-256 `0F53B784279FD7CAAAD2F35E50B939C2294E5B983233641F4F9538D8C405A9A4`，MVID `609344b4-9035-4277-bb15-5398e877bdfc`；插件程序集 SHA-256 `678B1A558CC393557B7F04C8EBC8BAB6CEA0BFBF5A2B06D7B8B046744F913AAB`，MVID `ceb94fb1-0e45-43c2-ac49-d9b1cf76dbf0`。二者 ProductVersion 为 `0.6.73+903525becdb679e9a4aae7bb04e73c98c1cb25dc`，即构建时工作树父提交；源文件身份另由上方测试源码 SHA 明确绑定。

没有生产 XAML/视觉变更，因此本阶段不运行 RenderHarness；历史双主题截图仍是 `52900815` 阶段的证据，不冒充当前 main 截图或真实屏幕呈现。

## 外部状态与未验证边界

- 当前公开 Actions 页面显示 #941（run `38279606113`，提交 `1c39e909`）终态失败、耗时 `8m31s`；#942（run `38281199307`，提交 `903525be`）终态失败、耗时 `9m00s`。job 页面显示需要登录后查看日志。匿名 logs 与 artifacts API 均返回 HTTP 403 rate limit；失败步骤、CI 实际 SDK 和根因仍未取得，不能依据摘要猜修。
- R05-04 的本机行为证据不覆盖真实 Playnite、物理键鼠、OS UIA/Narrator/IME、物理 DPI/多屏、最终呈现帧、ETW 或宿主性能。没有对用户文件/远端数据执行写操作。
- 下一项：对 R05-05 复核当前 main 是否仍无批量三态复选框消费点；若继续不适用，再推进 R05-06 开关保存语义。
