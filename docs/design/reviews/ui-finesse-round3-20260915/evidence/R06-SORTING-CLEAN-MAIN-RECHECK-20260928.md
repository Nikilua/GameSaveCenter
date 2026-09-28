# R06 排序崩溃修复：干净 main 身份复核

日期：2026-09-28
当前代码/测试身份：`3d83d38f9592c01522240fe7f462bf2e52ff0cb3`
排序修复提交：`f8a82469c0eb3c4c197b4272972056778dee29ea`

## 身份校正

原 [排序崩溃复核](R06-SORTING-DETACHED-VIEW-CURRENT-MAIN-20260924.md) 的 TRX 在 2026-09-24 14:33 运行；代码提交 `f8a82469` 于 14:38 创建，提交中同时包含排序控制器改动和新增的两个行为用例。因此原 TRX 的 Git 身份 `ade4b937` 指向父提交，不能单独作为已提交干净代码的身份依据。保留原 TRX 和运行记录；本复核在当前干净 `main` 重新构建、运行，以补足这一身份缺口。

`f8a82469..3d83d38f` 间 `DataGridStableSortController.cs` 和 `R06SortingBehaviorTests.cs` 没有变化。本次构建开始与测试结束时工作树均干净，隔离 Release 构建由 `scripts/build.ps1` 从当前完整 HEAD 写入程序集身份。

## 构建与行为结果

- 完整 Release solution build：`0 warning / 0 error`；XAML 结构检查 `24/24`。
- 当前提交 `R06SortingBehaviorTests`：`7 passed / 0 failed / 0 skipped`，VSTest exit `0`；[原始结果（环境字段已脱敏）](R06-SORTING-CLEAN-MAIN-20260928.trx)。
- 排序数据行为覆盖存档时间/数值、任务进度/阶段、媒体来源、未知值升降序与稳定次键；真实生产 `DataGridColumnHeader` 的受控 WPF Click 路径验证升序、再次点击降序和箭头同步。
- 失效视图负例在显示的 STA WPF Window 中调用实际列头 Click：先 `DetachFromSourceCollection()`，再点击；断言无异常、当前活动列/方向不变、非活动列不显示伪箭头。`ProductionTablesAttachStableProfilesAndKeepSharedArrowContract` 同时调用 `TestRepositoryContext.AssertAssemblyMatchesSource()`，将测试程序集绑定到当前源码根身份。

## 未验边界

这是合成备份 DTO、隔离 STA WPF 窗口和逻辑 DIP 的回归，不模拟 OS 鼠标/触屏输入。真实 Playnite/package-host、用户当前加载包、当前用户库、物理 DPI、呈现帧和宿主性能仍未验；未读取或修改真实存档、媒体或云端数据。桌面截图与日志中“点击表头”问题的代码路径有受控行为覆盖，但不能据此宣称用户安装包已在宿主中复测。
