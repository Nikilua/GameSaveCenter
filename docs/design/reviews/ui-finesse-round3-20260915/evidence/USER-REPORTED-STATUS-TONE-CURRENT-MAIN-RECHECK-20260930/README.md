# 活跃状态颜色语义复核（2026-09-30）

## 范围

生产提交 `e27eab0465714dbfc9b75aa5006916460bfcfce7` 将 Overview 云端队列、Maintenance 环境检查和 Save Center 比较质量中误用的通用 Info 蓝按数据含义映射到主题 Neutral、Info、Success、Warning、Error 资源。普通等待/暂停和“精确”比较属性保持中性；信息仍由文字/图标表达，不依赖颜色单独传递。没有更改 DTO、命令、业务流程或 R03/R22 整组状态。

该配色取向参考 [Microsoft Windows Color](https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/color) 对颜色层级、语义和节制强调的说明，以及 [WCAG 2.2 Use of Color](https://www.w3.org/WAI/WCAG22/Understanding/use-of-color) 关于不能只靠颜色传递信息的要求。

## 构建与行为结果

- `scripts/build.ps1 -Configuration Release -SkipTests`：XAML `24/24`，解决方案 `0 warning / 0 error`。
- `ReportedWorkspaceLayoutBehaviorTests`：`44/44`，Light/Dark；包括云端队列语义状态、环境检查状态和备份比较质量的实际 WPF 前景/背景。
- `StatusToneResolverTests` 与 `TypographyDiagnosticsTests`：`13/13`。
- `python scripts/validate-source.py` 通过；WPF UI 静态检查为 `0 errors / 30 warnings / 177 info`，与本批之前扫描计数一致。
- R00/R01 freshness 检查 `14/14 FRESH`；freshness 自测通过（docs-only、shared-control、package-identity）。

测试记录：[ReportedWorkspaceLayoutBehaviorTests.trx](ReportedWorkspaceLayoutBehaviorTests.trx)、[StatusToneAndTypographyTests.trx](StatusToneAndTypographyTests.trx)。TRX 中机器仓库路径已替换为 `REPO`。

## 离屏视觉抽查

[render-qa-report.txt](render-qa-report.txt) 记录 `render-qa OK`、提交 `e27eab0465714dbfc9b75aa5006916460bfcfce7`、`WorkingTreeClean=True`、Light/Dark 页面矩阵及工作区夹具。渲染进程使用 `GSC_BUILD_COMMIT=e27eab0465714dbfc9b75aa5006916460bfcfce7` 构建；报告中 `DpiScale=1.00` 是离屏逻辑 DIP，不能代表实际 Playnite 窗口或物理 DPI。

精选截图：

- [Overview-1366x768.png](Overview-1366x768.png)：带有进行中、成功、取消、失败状态的合成任务。
- [Maintenance-1366x768-tab0.png](Maintenance-1366x768-tab0.png)：问题列表的警告与错误状态。
- [Maintenance-1366x768-tab1.png](Maintenance-1366x768-tab1.png)：云端传输状态及对应状态文字。

这些图片来自 `OffscreenRenderHarness`，仅供受控布局和颜色复核；没有启动 Playnite 或读取用户安装 DLL。

## 程序集身份

所有程序集均为 Release，`InformationalVersion=0.6.73+e27eab0465714dbfc9b75aa5006916460bfcfce7`。测试和渲染加载的插件副本 SHA256/MVID 一致。

| 程序集 | SHA256 | MVID |
|---|---|---|
| `GameSaveCenter.Playnite.dll` | `3AA1B31707FF9E7F3A6A03DBA44D753E998796E90E076532AD95F5374317CEB5` | `e2d04840-b200-401c-8d30-9b63f0f7e720` |
| `GameSaveCenter.Playnite.Tests.dll` | `2A98CBB91711C1EC937CCFD9C639B8F857C055184ABC4429860F7A85F4AA472F` | `da3267c8-953f-4c31-9484-368255706c78` |
| `GameSaveCenter.RenderHarness.exe` | `DF21871676A9C8573E54B242A02626CD38EA95231C26A6FE4A20D9365CAD51B2` | `9721b443-3ff2-41cf-817d-da54d58d11bd` |

插件副本由源码 Release 构建输出、Playnite 测试输出和 RenderHarness 输出读取，三者插件 SHA256 均为 `3AA1…7CEB5`，InformationalVersion 均指向本提交。

## 未覆盖范围与下一步

未验证真实 Playnite 宿主、用户安装 DLL、物理 125%/150% DPI、最终屏幕帧或跨屏呈现。下一项继续审查有数据时重复显示的辅助说明。Media Inbox 真实滚动偏移仍需同一 Playnite 进程的 `[GSC-GRID-DIAGNOSTIC]` 前后数据；R08-01 用户报告的 `1/2` 失败仍需失败方法、断言/堆栈和原始 DLL 身份。
