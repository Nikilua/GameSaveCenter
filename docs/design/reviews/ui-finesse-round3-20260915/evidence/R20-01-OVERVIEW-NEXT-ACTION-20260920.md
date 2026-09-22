# R20-01 概览下一步证据

日期：2026-09-20  
分支：`codex/ui-finesse-round2`  
结论：已满足，待环境验证

## 1. 任务条件与现有能力

R20-01 要求概览只突出当前最需要处理的一项，并将动作连接到真实解决入口；无游戏、未匹配、可备份和失败待处理必须分别正确，优先级变化不能因无关数字变化而反复跳动。

本项先复用现有 DTO、`OverviewPriorityResolver`、`GamePickerViewModel`、生产 Shell 和任务工作区，没有新增概览服务或批量写入入口：

- 无游戏继续使用现有 `Empty` 状态和刷新入口。
- 有失败任务时使用 `Tasks` 状态，`DashboardViewModel` 设置真实任务筛选“失败”、取消待执行的搜索/历史防抖刷新，并显式加载任务页；不会把失败数字只显示成不可操作的提示。
- 有未匹配游戏时使用 `Unmatched` 状态，动作打开现有游戏选框并设置“未匹配”筛选。
- Ludusavi 可用且有已匹配但从未有本地备份的游戏时使用 `Backupable` 状态，动作打开同一游戏选框并设置新增的“可备份”筛选；不直接调用现有全库 `BackupAllCommand`，保留用户选择目标后再执行真实备份的边界。
- 其余媒体/通用提醒/健康状态沿用原有顺序。失败、空库、未匹配、可备份均由快照中的稳定字段计算；无关提醒数量变化不会改变同一优先级结果。

## 2. 实际证据

- `OverviewPriorityResolverTests` 与 `GamePickerViewModelTests` 定向组合 `32 passed / 0 failed / 0 skipped / 32 total`。覆盖未匹配优先于可备份、可备份不启动全库写入、失败任务入口，以及无关快照变化保持同一状态；同时验证“可备份”筛选的正例和排除项。
- `OverviewInteractionTests` 与 `GamePickerShellSourceTests` 定向组合 `5 passed / 0 failed / 0 skipped / 5 total`，覆盖真实 Overview ButtonBase 命令交互和 Shell 选框入口源码约束。
- 使用隔离 source-copy 与独立 Release 输出编译 Playnite `net462`、Tests `net472` 并运行上述测试；通过。输出仅保留既有 `MediaCenterView.xaml.cs:671` `CS8602` 警告，没有新增编译错误。
- `scripts/validate-source.py` 通过；`scripts/check-xaml.ps1` 检查 `24/24`；`git diff --check` 通过。未依赖 `Assert.Contains` 单独签收动画、焦点或性能；本项的行为证据覆盖命令、筛选、优先级负例，Shell 打开仍复用现有焦点/IME 保护。
- 直接在链接工作树构建时，MSBuild 写入 `src/GameSaveCenter.Contracts/obj/...AssemblyInfoInputs.cache` 和 WPF 临时项目遇到 `Access denied`；没有绕过权限，也没有修改 main。改用 source-copy 只为获得可审阅的隔离编译证据，构建身份仍指向本分支 `codex/ui-finesse-round2`。

## 3. 未确认边界与安全语义

- 未运行真实 Playnite/package-host、Worker pipe、Ludusavi 或真实存档备份；只使用合成 DTO、fake 服务、隔离测试宿主和隔离目录，未写真实存档、媒体、云端或诊断。
- 未宣称最终 presented frame、物理 DPI/跨屏、UI Automation/读屏、真实键盘/IME、ETW 或宿主性能结果；Demo 原目录不可用，沿用已恢复的生产基线。
- “可备份”只在工具可用、游戏已匹配且没有版本数与最近备份时间时计入；R20-02 的指标统计范围/更新时间、R20-03 首次配置引导和后续状态任务尚未处理。
- main 仍保留用户未提交的 `DashboardView.xaml.cs`、`src.zip` 和对话框相关未跟踪文件；本项没有覆盖、移动或合并它们。

## 4. 下一步

下一可执行任务为 `R20-02 指标统计范围`：先盘点 Overview 快照中的各数字、来源字段与更新时间，再补“当前游戏/全库/未知未加载”标识和点击后列表的可解释来源；继续沿用合成数据、fake 服务和隔离目录。
