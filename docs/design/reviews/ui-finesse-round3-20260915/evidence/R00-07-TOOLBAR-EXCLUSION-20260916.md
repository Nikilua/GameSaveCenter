# R00-07 审计排除项收窄证据

日期：2026-09-16  
分支：`codex/ui-finesse-round2`  
实现提交：`42f9dca61eb23b92e1cf80a615b764e844d5a1d7`

## 对照基线

质量审查 F07 指出，`AnalyzeToolbars` 原先以 `TrainerToolsSettingsScrollViewer` 的祖先名称整棵排除，正常长表单与未来真正异常工具栏无法区分。R00-07 要求按工具栏用途和滚动可达性分类，同时保留同一祖先下超宽、不可达工具栏的失败负例。

## 实现

- 删除 `!IsInsideNamedAncestor(panel, root, "TrainerToolsSettingsScrollViewer")` 整棵排除。
- 根据显式 Toolbar/ActionRow 语义、命令按钮数量、输入控件数量区分 `action-toolbar`、`settings-form` 和 `content-flow`；非动作项保留在报告中，并记录 `ExclusionReason`。
- 报告记录动作/输入控件数、布局/期望宽度、可用宽度、可见交集、横向溢出、可达性和最近滚动祖先。
- 横向溢出只比较有效需求宽度与可用宽度，避免把当前页面滚动位置导致的 `Rect.Empty`（`-∞` 宽度）误判为横向溢出；隐藏状态父级记录为状态隐藏并不生成动作栏告警。
- 保留现有生产视图、命令/绑定、滚动系统、取消/错误/安全语义和 `net462` 兼容；本阶段只修改审计模型、分析器、报告和隔离探针。

## 行为验证

当前 clean-tree 探针绑定 `42f9dca61eb23b92e1cf80a615b764e844d5a1d7`，报告为 `WorkingTreeClean=True`、`DpiScale=1.00`（offscreen logical DIP）。

1. `dotnet build tests\GameSaveCenter.RenderHarness\GameSaveCenter.RenderHarness.csproj --no-restore -c Release -m:1 /p:UseSharedCompilation=false -v:minimal`：`0 warning / 0 error`。
2. `dotnet test tests\GameSaveCenter.Playnite.Tests\GameSaveCenter.Playnite.Tests.csproj --no-restore -c Release -m:1 /p:UseSharedCompilation=false --no-build --filter "FullyQualifiedName~UiAuditSourceTests|FullyQualifiedName~UiFinesseRound2ControlSourceTests"`：`29/29`。
3. `RenderHarness.exe toolbarprobe .tmp\r00-07-toolbarprobe-final`：`toolbarprobe OK`。
   - `normal-form`：真实输入行分类为 `settings-form`，`excluded=True`，理由为“表单输入流：包含输入控件但没有命令按钮”，无 `TOOLBAR_*` 告警。
   - `wide-toolbar`：同一命名 `TrainerToolsSettingsScrollViewer` 下，`desiredWidth=700`、`availableWidth=360`，分类为 `action-toolbar`，命中 `TOOLBAR_HORIZONTAL_OVERFLOW`。
   - `unreachable-toolbar`：同一命名祖先下垂直滚动被禁用，分类为 `action-toolbar`、`reachable=False`，命中 `TOOLBAR_UNREACHABLE`；同时保留横向几何数据。
4. `RenderHarness.exe audit .tmp\r00-07-audit-final`：`161` 个运行时快照、`73` 个运行时警告、`0` Fidelity、`0` 失败路由；HIGH 和 MEDIUM 均为“无”，既有合法嵌套/页级滚动保持 INFO。

## 校准记录与边界

- 首轮全量审计曾因 `Rect.Empty.Width=-∞` 把页面当前滚动位置外的合法动作栏误报为横向溢出；最终判定已收窄为有效需求宽度比较，并用 clean-tree 全量审计复核为 0 HIGH/0 MEDIUM。这是检测器校准事实，不是生产页面缺陷。
- 证据使用合成 WPF 面板、真实生产视图审计和隔离输出目录；没有写入真实存档、媒体、云端或用户配置。
- `DpiScale=1.00` 是逻辑 DIP，不代表物理 DPI 或显示器像素；探针和全量审计不替代真实 Playnite 嵌入 Dashboard、用户主题、鼠标/键盘/滚轮、ETW、presented frame 或宿主帧率。

## 下一步

下一可执行小批量为 R00-08“搜索框 Enter/IME”：先查现有 `OnPickerPreviewKeyDown`、选框焦点回退和搜索无结果状态，再用已有事件/命令契约建立无结果 Enter、IME 候选确认、方向键、Enter、Esc 和焦点返回的行为负例，不凭字符串断言签收真实输入。
