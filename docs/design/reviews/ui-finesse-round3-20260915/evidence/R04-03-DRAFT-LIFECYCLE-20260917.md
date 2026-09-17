# R04-03 未保存离开保护证据

采集日期：2026-09-17（Asia/Shanghai）。代码提交：`58e1734f0bf84b82d0fd95077327122e9b99cbf3`（`补齐设置草稿离开保护`），已推送 `codex/ui-finesse-round2`。

## 结论

R04-03 在当前受控范围内已满足。设置编辑继续由 Playnite `ISettings` 生命周期拥有：`BeginEdit` 捕获基线，用户输入只改变当前编辑对象，明确取消才恢复克隆，成功提交后清空编辑缓冲。设置页关闭前检测当前编辑会话的脏状态，复用现有原生消息框路径；“是”放弃并允许关闭，“否”取消关闭并恢复原字段焦点。确认 UI 不可用时采用保守路径，保留窗口和草稿，不静默撤销。

## 先查已有能力

- `GameSaveCenterSettings` 原已有 `BeginEdit`、`CancelEdit`、`EndEdit`、`SettingsCommitted`、`SettingsReverted` 和 `CreateSettingsFingerprint`，因此没有引入第二套设置保存服务或绕过 Playnite 保存按钮。
- `GameSaveCenterSettingsView` 原已有三态保存提示、按编辑指纹判断脏状态和取消回滚通知；本阶段只把 Playnite 编辑克隆基线暴露给重建后的视图，并将编辑会话结束时的缓冲清理明确化。
- 设置分类仍是现有 `SettingsSectionTabs` + `SettingsScroller`，游戏选框、滚动条、命令/Binding、错误阻断、恢复保护、有限列表和 Playnite/net462 目标均未改动。

## 实现与行为

### 设置模型

- `HasPendingEdit` 只反映 Playnite 当前是否持有编辑克隆。
- `GetEditBaselineFingerprint()` 返回当前编辑克隆的指纹；设置视图卸载后重新绑定同一设置对象时，脏草稿不会被新视图初始化为“已保存”。
- `CancelEdit()` 先取得克隆、清空编辑缓冲，再恢复值并发出一次 `SettingsReverted`；重复取消不会再次撤销或重复通知。
- `EndEdit()` 只有在 Playnite 写入、视觉设置通知和应用设置调用成功后才清空编辑缓冲并发出 `SettingsCommitted`；失败仍保留草稿供后续处理。

### 设置视图

- Loaded 时挂接宿主 `Window.Closing`，卸载时解除挂接，避免页面生命周期结束后保留宿主事件引用。
- 有待提交且指纹发生变化时显示原生确认：选择“是”调用现有 `CancelEdit()` 后关闭；选择“否”设置 `Cancel=true`，保留编辑对象并将焦点恢复到离开前的字段和分类。
- 关闭确认异常时同样 `Cancel=true`，记录日志并保留草稿，避免对话框不可用造成静默丢失。
- 临时卸载/重载期间记录 `Keyboard.FocusedElement`、分类和草稿状态；重新 Loaded 后使用已有分类滚动入口、`BringIntoView` 和 `Keyboard.Focus` 恢复编辑位置。

## 自动验证

1. clean Release 构建：XAML 结构 `24/24`；全解决方案 `0 warning / 0 error`；`python scripts/validate-source.py` 通过。
2. clean Release `GameSaveCenter.Playnite.Tests.dll` 定向测试：`SettingsDraftLifecycleBehaviorTests`、`PortableSettingsTests`、`SettingsValidationSourceTests` 合计 `12/12`，其中实际 STA WPF Window 测试模拟设置页临时分离/重挂，确认同一编辑对象的草稿值、分类和原字段焦点均保留；随后显式 `CancelEdit` 恢复默认值并清空编辑缓冲。
3. clean commit RenderHarness：完整 SHA `58e1734f0bf84b82d0fd95077327122e9b99cbf3`，`WorkingTreeClean=True`，Light/Dark、多尺寸、设置 normal/dirty/invalid、现有滚动/虚拟化、Shell/resize 均 `render-qa OK`；报告统计 `357` 张 PNG。设置 dirty 代表图和深色设置状态图已人工抽查，保存状态和错误层级未回归。

## 边界

行为测试使用合成设置、fake Worker 文件、隔离目录、STA WPF Window；没有读取或修改真实存档、删除真实媒体、写用户云端或发送诊断。RenderHarness 是离屏 logical DIP，不能证明真实桌面呈现帧、物理 DPI/跨屏、宿主字体或屏幕阅读器。

关闭确认的代码路径已由编译、源码门禁和安全分支审查覆盖，但本轮没有自动点击真实 Playnite 设置窗口的原生关闭按钮/确认框，不能把 `MessageBox` 的实际宿主 UIA、Playnite 关闭时序或真实用户输入写成已验。真实 Playnite 保存/取消/关闭回调和窗口重建仍需在稳定宿主中补测；确认 API 异常时的保守保留路径已由代码分支固定。

下一可执行任务：R04-04 粘贴标准化。
