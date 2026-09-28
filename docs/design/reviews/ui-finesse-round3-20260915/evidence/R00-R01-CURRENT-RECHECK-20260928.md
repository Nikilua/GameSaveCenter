# R00/R01 当前源码身份复核（2026-09-28）

## 发现与修正

从远端 `main` 快进后，freshness 扫描在旧证据身份 `3a1dadd8` / `38d5b7b2` 下发现 7 条 R00/R01 记录的关联源码路径已经变化。定向复核期间，`59afd8f4` 的首次完整 RenderHarness 报告出现 4 条 SettingsState 问题：正常和 dirty 夹具都误报 Worker 校验错误。

首次扫描命中的记录和路径为：R00-01-02 `WpfUiProduction.xaml`；R00-05 同一主题字典及 `WpfUiResourceDictionaryTests.cs`；R00-06 `MediaCenterView.xaml` / `.xaml.cs`；R00-07 `UiAuditSourceTests.cs`；R01-01 `GameSaveCenter.Playnite.Tests.csproj`；R01-03 `UiAuditSourceTests.cs` 与 `GameSaveCenter.RenderHarness/Program.cs`；R01-06 `Program.cs`。刷新后 14 条记录中 `needsRerun=0`、`matchedSourcePaths=0`；R00-01-02/05/06 锚定于各自完整测试输出身份 `53e61175`，其后唯一代码改动为不匹配这三项 sourcePaths 的 RenderHarness 根路径修正。

根因是 `RunSettingsStateProbes` 按 `AppContext.BaseDirectory` 固定回退五层。隔离输出位于仓库 `.tmp` 时，该路径落到 `.tmp` 本身，进而从不存在的 `.tmp/src/GameSaveCenter.Worker/...` 查找 Worker；夹具日志中的 `workerPath=known` 只反映 fixture 标记，不代表文件实际存在。该探针改为复用 RenderHarness 已有的 `RepositoryRoot`。没有修改 Settings 产品校验、用户路径、XAML 或业务代码。

修正后在干净源码身份 `59afd8f4bd81039e1e8150d0ccc35e608b357113` 重跑完整双主题 RenderHarness：373 个输出文件，其中 312 张 PNG；`WorkingTreeClean=True`、`render-qa OK`。1040×700 合成设置视图中，normal 显示“已保存”、dirty 显示“有未保存更改”，invalid 显示校验错误并保留摘要。初次 4 条失败是夹具根路径误算，修正后复跑通过；不将失败记录删去。

## R00/R01 证据

- Release solution build：`0 warning / 0 error`；XAML 结构检查 `24/24`。RenderHarness Release build：`0 warning / 0 error`。
- `UiAuditSourceTests 6/6`、`RepositoryIdentityTests 2/2`、`BuildIdentityTests 3/3`，均在源码身份 `59afd8f4` clean exit。
- R00-01/02：`UiFinesseFoundationTests 9/9`、`UiDiagnosticsExporterTests 11/11`，Light/Dark finesseprobe 均通过；证据源身份为 `53e61175`。`53e61175..59afd8f4` 的唯一非文档差异是 RenderHarness 根目录定位，未匹配这两项的 `sourcePaths`。
- R00-05：当前 WPF 资源类 `139 passed / 39 skipped / 0 failed`；39 项为既有显式 skip，证据源身份 `53e61175`，其后没有命中该项 `sourcePaths` 的变化。
- R00-06：媒体几何 `3/3`、媒体窗口锚点 `10/10`、R18 专测 `1/1`；四个关联行为类精确组成 `6+10+3+4=23/23`，证据源身份 `53e61175`。该身份之后没有命中 R00-06 的媒体 XAML、代码或测试路径变化。
- R00-07：当前 `UiAuditSourceTests 6/6`；受控 audit 为 168 个运行时快照、103 条警告、0 HIGH、0 MEDIUM、0 Fidelity 警告、0 失败路由。20 个证据索引样本的引用/身份/样本/边界均 `20/20`，验证脚本通过。
- R01-01：当前身份测试 `RepositoryIdentity 2/2`、`BuildIdentity 3/3`。
- R01-03 / R01-06：当前身份 `UiAuditSourceTests 6/6`；证据索引门禁 `20/20`；当前受控审计结果见 [R01-06 归档](R01-06-controlled-audit-20260928/README.md)。
- freshness 基线将上述记录锚定到各自实际复核身份（R00-01/02、R00-05、R00-06 为 `53e61175`；R00-07、R01-01、R01-03、R01-06 为 `59afd8f4`）。扫描结果 `0` 条需要重跑、`0` 条源码路径匹配；package identity 为 `not-provided`。脚本的 docs-only/shared-control/package-identity 回归测试通过。

完整 RenderHarness 文本、代表性合成页面图、三份身份/审计 TRX、受控审计摘要、索引和报告均在 [当前 R01-06 归档](R01-06-controlled-audit-20260928/README.md)。

## 未验边界与后续

RenderHarness 和 WPF 审计均为合成数据、离屏逻辑 DIP；没有启动 Playnite、安装包或触碰真实存档、媒体、云端。当前 0 HIGH/0 MEDIUM 不等同所有 103 条审计警告都消失，也不证明实际宿主主题/DPI、物理跨屏、UIA/IME、presented frame、ETW 或宿主性能。package identity 尚未提供。R 台账仍 192 个唯一 ID，状态 `106/83/1/1/1`；R23-08 当前没有 READY/IN_PROGRESS 产品代码项。下一步只在新用户复现或相应宿主/系统环境门槛变化后领取对应工作，不从本轮审计自行造功能。
