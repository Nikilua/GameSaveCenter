# R04-08 保存反馈闭环（2026-09-17）

## 结论

当前设置保存链在可控范围内已经形成稳定反馈闭环：设置页能区分保存中、已写入 Playnite 但正在应用 Worker、Worker 应用失败、保存失败、校验中、校验错误、脏草稿和已保存；保存/应用进行中重复触发 `EndEdit` 会被抑制。应用失败不会伪报“已保存”，本地写入失败会保留编辑克隆和修复入口。

当前生产设置字段均走 Playnite 持久化后 Worker live apply，没有现存的“部分生效需重启”字段，因此本阶段不虚构重启提示。未来若加入仅重启生效的字段，需要在同一反馈模型中增加真实来源驱动的重启态。

## 现状核对与实现

- 原有 `SettingsSaveHintText` 只有已保存、未保存更改和校验错误三类提示，`ISettings.EndEdit` 触发保存后不能把 Worker 异步应用结果反馈给设置页。
- `GameSaveCenterSettings` 现在以 `Interlocked.CompareExchange` 防止同一实例并发保存，提供 `SettingsSaveStarted`、`SettingsApplyStarted`、`SettingsApplyCompleted` 和 `SettingsSaveFailed` 事件；本地写入成功后才清除编辑克隆。
- `GameSaveCenterPlugin.ApplySettingsAsync(Action<Exception?>?)` 将既有异步应用流程的完成/异常回调接回设置页，同时保留已有后台日志与错误处理。Worker 失败明确标记为“已写入 Playnite · Worker 应用失败”；本地写入异常明确标记为保存失败并保留当前草稿。
- `SettingsSaveFeedbackState` 将保存/应用/失败转换为可测试状态机；编辑任意字段会清除旧失败态，成功应用恢复已保存提示。状态文字和 Tooltip 共用设置页稳定提示面，不依赖一闪即过的 Toast。

## 验证证据

- 实现提交：`c735905aa1092f409bc7ddf9de1272f1d77dc084`（`补齐设置保存反馈闭环`），提交后已推送 `origin/codex/ui-finesse-round2`，RenderHarness 报告记录 `WorkingTreeClean: True`。
- `scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot .tmp/r04-08-build-compile`：XAML `24/24`，构建 `0 warning / 0 error`；`python scripts/validate-source.py` 和 `git diff --check` 通过。
- R04-08 相关选择集 `SettingsSaveFeedbackTests|SettingsValidationSourceTests|SettingsDraftLifecycleBehaviorTests|PortableSettingsTests|SettingsPathValidationTests|SettingsAsyncValidationTests`：`20/20` 通过。
- `scripts/render-qa.ps1 -Configuration Release -Output .tmp/r04-08-render-final`：绑定上述完整 SHA，Light/Dark，offscreen logical DIP `1.00`，297 张 PNG，`render-qa OK`。报告还包含 50/400/2000/4468 数据量滚动探针；Settings normal/dirty/invalid `1040×700` 图已人工检查，未见本阶段引入的裁切或状态面溢出。

## 边界与下一步

- 尚未在真实 Playnite 宿主中注入 `SavePluginSettings` 本地写入异常或 Worker pipe 应用失败，也未自动驱动真实宿主的保存/取消/关闭时序；20/20 包含状态机、源接线和既有设置生命周期行为，不冒充真实宿主故障注入。
- 渲染证据是合成设置/fake 服务、隔离目录、STA WPF 和 offscreen logical DIP；真实 Playnite 嵌入、系统剪贴板/IME、屏幕阅读器、物理 DPI/跨屏、presented frame、ETW、宿主线程/帧率性能仍未验。未写真实存档、媒体或云端。
- 下一可执行任务为 R05-01 弹层焦点范围；优先核对现有游戏选框/弹层焦点路由和对应 Q 项，继续保留当前选框与滚动条系统。
