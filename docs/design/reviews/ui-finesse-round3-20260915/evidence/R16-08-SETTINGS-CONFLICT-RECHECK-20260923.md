# R16-08 保存冲突处理定向复核

日期：2026-09-23  
复核提交：`afe80186`（`codex/ui-finesse-round2`，D 盘工作区 `D:\workplace\github\GameSaveCenter`）  
结论：现有实现满足受控代码与行为门槛，账本状态校正为“已满足，待环境验证”；本批没有新增生产代码，复用实现提交 `ee6b37c9`。

## 实际复测

- `R16SettingsConflictBehaviorTests`：`3/3`。非冲突持久化字段合入、同字段冲突保留草稿且不部分覆盖、字段摘要与 `SettingsConflictException` 均通过。
- `R16SettingsConflictSourceTests`：`1/1`。确认 `EndEdit` 读取最新持久化值、触发冲突事件并抛出异常，设置页显示提示，合并器存在且不存在静默 `CopyFrom(persisted)`。
- `SettingsDraftLifecycleBehaviorTests`：`1/1`。合成 STA WPF `Window` 中，脱离视图仍保留脏草稿和焦点字段，取消恢复原始编辑基线。
- `SettingsSaveFeedbackTests`：`2/2`。持久化写入失败与 Worker 应用失败分别反馈，成功应用回到 idle。
- `PortableSettingsTests`：`10/10`；本批定向合计 `17/17`。
- 从当前 checkout 生成的隔离 Release solution：`0 errors`，`2` 条既有 warning，均为 `MediaCenterView.xaml.cs:706` 的 `CS8602`。
- `validate-source.py`、XAML `24/24`、`git diff --check` 通过；WPF 静态检查 `0 errors / 28 warnings / 162 info`。这些 warning/info 是现有共享布局、主题与硬编码颜色提示，不作为最终呈现通过的依据。

## 保留能力与边界

保留当前游戏选框、滚动条系统、命令与 Binding、保存/取消、错误/取消/恢复保护、有限列表性能和 Playnite/net462 兼容；冲突时不静默覆盖，非冲突字段可合入，冲突字段保留草稿并阻断保存。复测只使用合成数据、detached settings、fake 服务和隔离输出目录，没有修改真实配置、存档、媒体、云端或发送诊断。

本批尚未证明真实 Playnite 双设置窗口或共享 settings 对象竞态、真实宿主错误呈现、最终浅深主题、UIA/读屏/IME、DPI/跨屏、RenderHarness presented frame、ETW 或宿主性能。Demo 原目录不可用，继续以已恢复的生产基线为参考；没有绕过系统跟踪权限，也没有把代理性能或离屏结果写成真实呈现结论。

下一可执行任务：`R17-01 健康结果分层`，先核对现有健康结果、已解决项和跨来源去重时间证据。
