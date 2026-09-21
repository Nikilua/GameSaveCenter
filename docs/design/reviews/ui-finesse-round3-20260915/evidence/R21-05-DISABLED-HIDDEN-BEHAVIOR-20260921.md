# R21-05 禁用与隐藏区别行为证据

日期：2026-09-21
分支：`codex/ui-finesse-round2`
任务：R21-05「禁用与隐藏区别」

## 验证范围

本阶段只补行为证据，没有修改生产业务代码、命令契约或共享控件实现。夹具使用实际 `SaveCenterView`、生产资源字典和合成命令，运行在隔离 `WindowHost` 中；未访问真实存档、媒体目录、云端或 Playnite 宿主。

## 已验证行为

夹具：`tests/GameSaveCenter.Playnite.Tests/R21DisabledHiddenBehaviorTests.cs`

- 维护跳转按钮在不满足前置条件时为 `Visibility.Collapsed`，不进入可见树，不能取得键盘焦点；对应的解释文本仍可聚焦，并同时读回 `AutomationProperties.Name` 与 `HelpText`。
- 满足维护前置条件后，同一实际按钮恢复可见、保持已有命令绑定，并可执行一次命令；这验证了隐藏条件与动作可用条件没有被混为一谈。
- 共享上下文动作在命令不可执行时保持可见并禁用，解释原因由同级说明文本提供；对禁用控件执行 UIA `IInvokeProvider.Invoke()` 被拒绝，命令副作用保持为零。

本项定向测试：2 通过、0 失败。邻接回归（R21DisabledHidden、R02ActionAvailabilityHint、R21AutomationValue、R21KeyboardNavigationTrace、R21AsyncCompletionAnnouncement）：31 通过、0 失败、0 跳过。

## 门禁与边界

- `scripts/validate-source.py`：通过。
- `scripts/check-xaml.ps1`：24 个 XAML 文件通过。
- R21-05 测试项目 Release 编译：0 警告、0 错误。
- 完整 Release 构建已完成编译，但 Worker 全量测试仍有一个与本项无关的既有失败：`MediaSyncServiceTests.ClassificationApplyUsesSelectedStableIdsAndValidatedTargetOverride` 在 `MediaSyncServiceTests.cs:570` 抛出 `NullReferenceException`（350 通过、1 失败、351 总计）；本阶段未修改该路径。
- 未宣称真实 Playnite 渲染、操作系统 UIA/读屏、DPI/跨屏、呈现帧、ETW 或宿主性能验证；Demo 原目录在当前工作区不可用，视觉判断沿用已恢复生产基线。

## 后续

R21-05 的实现与行为证据已具备，账本暂按“已满足，待环境验证”记录。下一项为 R21-06；继续前先核对其真实账本状态与已有能力，保持小批量推进。
