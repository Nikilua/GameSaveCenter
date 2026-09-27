# GSC-058 首页旧交互兼容回归契约校准

日期：2026-09-27；测试最终代码身份：`b62baef525459831ffe6590996feea76985d3993`

## 结果

- 根据当前 GSC-058 首页层级，依次校准三个陈旧测试契约：`6d6d7507` 更新 `OverviewInteractionTests`，反映默认折叠的统计条/全局活动区及其兼容命令边界；`76bc8a0d` 删除 Overview 边界夹具里已经不可达的 820-DIP 双语专用视口分支，并更新 `R03BilingualLengthTests`；`b62baef5` 更新 `R10RecentAccessBehaviorTests`，验证最近访问默认折叠、与首页任务集合隔离，并在测试中临时展开兼容卡验证命令。
- 最终身份 Release solution build：`0 warnings / 0 errors`；XAML 结构检查 `24/24`。最终身份下隔离 Playnite 非 WPF/source 测试组 `465 passed / 18 skipped / 0 failed`。
- 三个受影响行为类在最终身份 `b62baef5` 分别通过：`OverviewInteractionTests 4/4`、`R03BilingualLengthTests 2/2`、`R10RecentAccessBehaviorTests 2/2`；`R08MotionReverseBehaviorTests 2/2` 也在最终身份单独 clean exit。`R08` 在前一代码身份 `76bc8a0d` 另连续三次详细隔离复跑均为 `2/2`。
- Playnite WPF 的 105 个测试类通过分进程方式覆盖：类 1–50、52–65 在 `76bc8a0d` 成功，类 67–105 在 `b62baef5` 成功；受变更影响的类及 `R08`/`R10` 在 `b62baef5` 另行重跑。不是一次完整、单一提交身份的 WPF runner 全绿：一次官方隔离顺序运行在 `R08MotionReverseBehaviorTests` 出现间歇失败，之后该类的重复运行通过。不得把这批分段证据表述成完整 WPF 套件单次全绿。
- RenderHarness Release build 在 `76bc8a0d` 为 `0 warnings / 0 errors`。干净工作树、精确提交身份下运行 Overview 边界矩阵：3 个 profile（empty-activity、many-risks、offline）× 2 个主题 × 2 个尺寸（1040×700、1600×900），共 `12/12`，`WorkingTreeClean=True`、`overviewedges OK`、无 PROBLEM。该 Harness 源文件在后续 `b62baef5` 未修改。

## 范围与边界

本阶段只改 Playnite 测试和 RenderHarness 测试/夹具，不改生产 XAML、C#、绑定、命令或业务逻辑。GSC-058 的产品行为没有回滚；此次变更是让回归契约与现有行为一致。R 账本保持 192 个唯一 ID、`106/83/1/1/1`，没有新增、重分类或签收条目；backlog 没有 READY/IN_PROGRESS 产品项。没有启动真实 Playnite，也没有由离屏逻辑 DIP 推断宿主、物理 DPI 或 UIA 通过。

本轮 `.tmp` 隔离测试输出已在确认所有进程结束后，按仓库内逐项核验的精确路径清理；没有触碰其他设备遗留目录或生成物。当前未解决项仅是 `R08MotionReverseBehaviorTests` 的一次间歇性 runner 失败；最终身份隔离测试通过，尚无证据指向生产动画回归。
