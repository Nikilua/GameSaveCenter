# 用户报告 bug 回归：当前 main Core/Worker 与定向 Playnite 复核

日期：2026-09-27；当前测试构建身份：`d9d1f563a8cf87585cf69ef83dac2773e2d18b14`

## 当前身份与构建

- 当前 `main` 的 `src/` 与 `tests/` 相对完整 Playnite 隔离 runner 已通过的代码身份 `b382fb02cde41f65d8daff5166934161eb956e2b` 无差异；`b382` 之后仅有记忆、交接和证据文档提交。本阶段仍在精确当前 HEAD `d9d1f563` 上重建完整 Release solution，结果 `0 warnings / 0 errors`，程序集绑定当前完整 Git 身份。
- 当前身份 Core.Tests `125/125`、Worker.Tests `357/357` clean exit；另单独运行 `ExternalProcessRunnerTests 5/5`。Playnite XAML 检查 `24/24`。
- 与用户报告的备份编码、失败详情与复制反馈相关类逐类隔离通过：`R15TaskFailureCopyTests 6/6`、`R06ClipboardBehaviorTests 4/4`、`R22CopyFeedbackBehaviorTests 2/2`、`R12RestoreReportBehaviorTests 2/2`、`TaskCenterViewResponsiveTests 8/8`。
- 与慢启动/缩略图反馈相关类逐类隔离通过：`LargeLibraryPerformanceTests 5/5`、`AsyncThumbnailLoaderTests 6/6`、`AsyncThumbnailImageTests 2/2`、`R18ThumbnailBudgetTests 1/1`、`R09ThumbnailPlaceholderBehaviorTests 2/2`。
- 当前身份 `RepositoryIdentityTests 2/2`；`WpfUiResourceDictionaryTests 139 passed / 39 skipped / 0 failed`，39 项为测试类中既有显式 skip。完整 Playnite 官方 runner 在 `b382fb02` 上已 clean exit，111 个 source 类组及 105 个 WPF 隔离类全通过；因 `src/`、`tests/` 与当前 `d9d1f563` 完全相同，该全量代码覆盖仍适用于当前 main。没有在文档提交后的不同身份上重复耗时的 105 进程全量 runner。

## 结论与边界

本阶段是回归验证，没有新的生产代码变更，也未更改 R 行或 backlog 状态。已登记的备份 UTF-8/错误详情/剪贴板反馈及慢启动/异步缩略图修复在当前主线仍有覆盖，没有发现新的源码回归。测试使用 fake、合成数据和隔离 testhost；没有启动真实 Playnite、访问用户 profile/存档/媒体/剪贴板，也没有测量冷启动、presented frame、ETW 或物理 DPI。

R 台账仍为 192 个唯一 ID、状态 `106/83/1/1/1`。backlog 无 READY/IN_PROGRESS 产品项；ENV-001、Q24-03、R23-05 的既有真实宿主/跨屏/呈现环境门槛未变化。未生成需保留的 `.tmp` 或 `artifacts` 证据产物。
