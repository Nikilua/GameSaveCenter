# R20-06 部分可用状态证据

日期：2026-09-21  
状态：已满足，待真实宿主与外部依赖环境验证  
提交：`e32ed599`

## 范围与现有能力

R20-06 要求 Worker、云端或外部工具部分不可用时，保留独立可用区；受影响动作准确禁用并解释原因，单点故障不能把整页变成不可用错误页。本阶段先核对最新实现，没有重建状态模型：

- `ActionAvailabilityHints` 已按恢复、媒体收件箱、云端队列和隔离远端恢复分别计算前置条件。Worker 离线、未选择对象、缺少目标、Ludusavi/Rclone 不可用、认证/记录状态不满足或正在执行时，相关命令保持禁用并返回用户可读说明；不相关工作区命令不共用一个全局“整页失败”开关。
- `WorkspaceStatePresenter` 已提供 Loading、Empty、Error、Degraded、Offline 等独立状态投影；Dashboard 的游戏/媒体/维护/云端区域各自保留状态内容、旧数据和恢复入口，局部读取失败不覆盖其他区域。
- R02 的真实行为测试已经覆盖危险恢复动作、媒体/云端选择与运行前置条件、远端恢复必须先隔离校验，以及可聚焦/可读的可用性说明。R07 的横幅测试覆盖空任务失败、维护过期、安全模式和保留旧行时的重试/表格可达性。

## 本阶段修正

首次复跑 R07 时发现 Maintenance 云端过期横幅把 `DynamicResource` 用在 `Style.BasedOn` 上，真实 WPF 加载抛出 `XamlParseException`，因此不能签收。已将该横幅的 `BasedOn` 改为可在样式声明中解析的 `StaticResource`，不改变命令、绑定、状态条件或滚动系统。修正提交为 `e32ed599`。

## 验证结果

- 隔离 Release solution 构建：使用当前分支 `GscBuildCommit=36dbc3f9`、外部 source-copy 和仓库内短路径 `.tmp/r20-06-build`，Playnite `net462`、Playnite.Tests `net472` 均产出；`0 errors / 2 warnings`。两条 warning 都是既有 `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs:671` 的 `CS8602`。
- `R02ActionAvailabilityHintTests|WorkspaceStateSourceTests|R07StatusBannerBudgetBehaviorTests`：`18 total / 17 passed / 0 failed / 1 skipped`。跳过项是已有 `LegacyProductionUiBaselineFact`：`SharedWorkspaceStatePresenterExistsAndIsUsedAcrossPages`，其断言针对已撤销的旧今日工作台架构。测试最终退出成功；WPF testhost 关闭阶段输出了已知 TextServices COM 清理噪声，但未转化为测试失败。
- `scripts/validate-source.py` 通过；`scripts/check-xaml.ps1` 为 `24/24`；`git diff --check` 通过。

## 证据边界

本阶段使用合成状态、fake/隔离 testhost、隔离 source-copy 和隔离构建目录，没有读写真实存档、媒体、用户云端或诊断数据。未运行真实 Playnite/package-host、真实 Worker/Named Pipe、Ludusavi/Rclone/云端、真实外部工具故障时序，也未宣称最终 presented frame、物理 DPI/跨屏、UIA/读屏、OS 输入/IME、ETW 或宿主性能已验证。Demo 原目录不可用，沿用已恢复生产基线；main 的用户改动未碰、未合并。

阶段临时目录在文档提交前按精确路径清理；下一可执行任务为 `R20-07 最近活动密度`，先核对现有任务事件摘要、重复进度合并、完成/失败展开详情和真实计数来源。
