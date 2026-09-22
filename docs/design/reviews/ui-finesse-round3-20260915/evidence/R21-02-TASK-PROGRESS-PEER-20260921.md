# R21-02 任务进度控件 UIA 值与状态证据

日期：2026-09-21  
代码提交：`6b56a467`（`补充任务进度控件值证据`）  
分支：`codex/ui-finesse-round2`

## 本批范围

本批先核对已有 `TaskStatusDto.ProgressValue`、`ProgressDisplay` 和 TaskCenter 生产 XAML 绑定；没有重建 DTO、服务、命令或进度投影。新增 `TaskProgressPeerExposesBoundValueAndUnknownStatus`，用合成 `TaskStatusDto` 驱动实际 WPF `ProgressBar` OneWay Binding，并通过 `AutomationPeer` 检查：

- 正常运行值 `42`：UIA `RangeValue` 为 `42`，范围为 `0–100`，`HelpText` 为 `42%`；
- 未知值 `-1`：UIA 值按既有 `ProgressValue` 归一为 `0`，但 `HelpText` 保持 `—`，不把未知状态播报成有效进度；
- 越界值 `120`：UIA 值钳制为 `100`，`HelpText` 为 `100%`；
- 进度条的 UIA 名称为 `任务进度`，不以图形或百分号文本替代动作/对象名称。

这补充了既有 R06 的任务集合更新、未知/排队/运行零值、终态和进度投影测试，也复用此前 `74b559e2` 对 TaskCenter 行进度与所选任务详情生产 Binding/名称/HelpText 的核对；本批没有生产代码变更。

## 验证结果

- `R21AutomationValueBehaviorTests`：`20/20` 通过。
- 相关定向筛选 `R21AutomationValueBehaviorTests | R06TaskProgressBehaviorTests | R03NumericAlignmentTests`：`34/34` 通过。
- 提交身份 `GscBuildCommit=6b56a467a3143285b680122e82e6cc5d26c2678d` 的 D 盘源码副本 Release 构建目标为 Playnite `net462`、Tests `net472`：`0 errors`；仅有 `MediaCenterView.xaml.cs:671` 的 2 条既有 `CS8602` warning。
- `python scripts/validate-source.py`：通过；`scripts/check-xaml.ps1`：`24/24`；`git diff --check`：通过。
- WPF 静态检查：`0 errors / 27 warnings / 162 info`，与本批前基线一致，未见本批新增诊断。

## 证据边界

证据来自生产 TaskCenter XAML 契约、现有 DTO、合成数据、实际 WPF `ProgressBar`/`AutomationPeer`、fake/隔离 testhost 和 D 盘 source-copy。没有运行真实 Playnite/package-host、Windows UIA/读屏、OS 输入、IME、物理 DPI/跨屏、最终 presented frame 或宿主性能验证；没有用离屏截图或代理性能替代这些事实。链接工作树 `_wpftmp.csproj` 仍受 `Access denied` 限制，未绕过。Demo 原目录不可用，沿用已恢复的生产基线；main 用户改动未碰、未合并。

下一可执行任务：继续 R21-02 剩余复合选择器及逐控件状态/值负例；公共门禁完成后进入 R21-03 验证错误播报。真实宿主 UIA、呈现、DPI/IME、性能仍是未验边界。
