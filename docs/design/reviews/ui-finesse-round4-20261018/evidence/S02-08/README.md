# S02-08 通知体验验收

本项核对通知收件箱离页返回、Worker 重启、筛选结果批量已读、高对比状态与辅助通知频率。已有生产实现覆盖这些行为，本批没有改生产代码，也没有新增字符串镜像测试。实现与本次测试输入身份见[验证清单](verification-identity.json)。

## 受控验证

- Playnite 定向测试 `20/20`：通知收件箱 WPF 命令绑定与辅助说明、已读操作只作用于筛选后的未读项、工作区缓存页离开再返回、共享控件高对比状态、任务进度 UI Automation 语义及通知去重。TRX 与日志见 [tests](tests/)。
- Worker `NotificationRecordPersistenceTests` `12/12`：覆盖通知和重试身份跨 store/Worker 重启、已读变更、筛选清理边界、事件幂等与次数归并。TRX 与日志见 [tests](tests/)。
- 两个测试项目均用 SDK `9.0.302` restore，并在 `dotnet test` 中完成 Release 构建；`global.json` 要求的 SDK `8.0.423` 未用于本次验证。未跑完整解决方案构建或完整 WPF 隔离循环。
- Playnite WPF 测试结束后日志出现 `TextServicesContext.StopTransitoryExtension` 的 `InvalidComObjectException` 清理输出；VSTest 退出码为 `0`，TRX 汇总 `20/20` 且无失败用例。

## 现有实现依据

- `AcrylicProductionShellView` 缓存工作区页面；返回通知页使用原视图实例。
- Worker 通知记录持久化；批量已读只把当前过滤结果中的未读通知 ID 交给对应 Worker 命令。
- `TaskNotificationDeduper` 不为运行中进度快照领取屏幕通知；进度 UI Automation 状态不是 live region，进度更新保持键盘焦点。
- 高对比覆盖测试证明共享控件状态与语义色令牌行为。它只覆盖受控 WPF palette，不代表 Windows 高对比设置或 Playnite 宿主呈现。

## 边界

没有运行真实 Playnite、Windows 系统高对比切换或实际读屏器。受控 WPF Automation 检查不等同于真实读屏器公告；这些宿主证据仍待具备对应环境后验证。本批没有新增外发通知、诊断或真实数据写入。
