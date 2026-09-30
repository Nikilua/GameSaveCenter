# 完整 Release 构建与测试复核（2026-09-30）

## 完整脚本结果

最终源树在 `c5aad677` 代码提交身份执行：

```powershell
./scripts/build.ps1 -Configuration Release
```

脚本以退出码 `0` 完成，并报告 `构建与测试全部成功`：

- .NET SDK `9.0.302`；XAML 结构检查 `24/24`。
- Release solution 编译 `0 warnings / 0 errors`。
- Core：`125/125` 通过。
- Worker：`356/357` 通过，`WorkerProcessRestartTests.HardRestartReconcilesDurableIncompleteTask` 跳过 1 项、失败 0 项。
- Playnite 隔离流程：112 个 source test classes 与 113 个 WPF test classes 均完成；脚本报告 `All Playnite tests passed with WPF classes isolated by process.`

## 最终 RenderHarness 复核

- 当前代码提交 `c5aad677521f70d4daf617c53c66a3e8423bde8e` 的 clean-tree 报告为 `render-qa OK`，覆盖 Light/Dark、50/400/2000/4468 项数据量和紧凑窗口场景。报告中 `DpiScale=1.00` 明确是离屏逻辑 DIP，不能代表真实宿主 DPI。
- 检视 [媒体中心紧凑窗口截图](Shell-Media-1040x700.png) 与 [720×640 DIP shell 截图](Shell-720x640.png)：这两个离屏场景中未发现控件互相覆盖；只作为 RenderHarness 视觉检查，不作 Playnite/物理 DPI 验收。
- `python scripts/validate-source.py` 通过；WPF 静态检查 `0 errors / 29 warnings / 177 info`。扫描包含既有参考控件/临时审计警告，未据此关闭相关风险项。

## R08 页面缓存测试夹具校正

完整脚本首轮通过编译/Core/Worker 后，在 `R08PageSwitchBehaviorTests.CachedWorkspacePagesRestoreTabsAndTaskFiltersWithoutRefreshing` 失败。精确诊断为 `TaskTypeFilter` source 为 `null`、控件按生产 XAML 的 `TargetNullValue` 显示“全部”。生产 `DashboardViewModel.TaskTypeFilter` setter 将空/空白输入归一为“全部”；原测试自动属性不具备这个生产合同。现在 state-only fixture 对任务搜索、状态/类型/游戏筛选、范围和时间范围应用相同默认归一，仍覆盖页面切换前后的实际控件状态/列表选择/滚动与不触发刷新。

[R08 定向行为 TRX](R08PageSwitchFixture.trx) 为 `1/1 passed`。夹具源 SHA-256：`6114F75DA1107D2FCDC5E600DFB20CFDBC235AE139D0953C6D600738F327A918`。

## 限制

本机 SDK 只有 `9.0.302`；没有在本机重放用户 CI 的 SDK `10.0.401` / C# 14。SDK 10 `field` 关键字冲突的源码修复和原始诊断另见 [C# 14 编译修复证据](../SDK10-COMPILE-IDENTIFIER-FIX-20260930/README.md)。这里的 WPF 测试运行在隔离 testhost；不是 Playnite 安装实例或 125%/150% 物理 DPI 呈现验证。新的 GitHub Actions 运行需在推送当前检查点后确认。
