# S02-06 重要事件例外

实现提交：`a6153219`。弹窗例外现在只由明确的 `Warning` / `Error` 级别触发；未识别的级别按常规信息通知处理，并遵守临时静音和安静时段。设置说明同步写明任务/会话警告与错误仍会显示、保存到本地收件箱，以及未知级别的回退规则。

任务和会话级别继续由真实结果决定：任务失败为错误、取消为警告、成功为成功；会话摘要按失败/警告/成功状态映射。任务与会话通知记录在弹窗策略判断之前写入本地收件箱，因此重要事件有可回看的记录。备份校验风险另由现有 Findings 持久化并显示在维护中心；静音策略只控制通知弹出，不会清除或隐藏该风险证据。

## 验证

- SDK `9.0.302` 下全量 Release solution build：`0` warning / `0` error。先在本阶段隔离输出中还原依赖，再从该输出编译；此主机没有 `global.json` 锁定的 SDK `8.0.423`。
- `NotificationPopupPolicyTests`：`22/22`，覆盖警告/错误在静音与安静时段内仍出现，以及未知级别遵循静音和安静时段。
- `NotificationRecordPersistenceTests`：`7/7`，覆盖记录跨 Worker 重启留存和未知级别归一为信息。
- `R09FocusOutlineBehaviorTests`：同次新构建隔离输出 `2/2`。构建目录中的插件 DLL 与 Playnite 测试目录实际加载副本 SHA-256 相同；当前测试没有复现用户旧日志里的 `2` 对 `1` 断言。
- `scripts/validate-source.py` 与 `git diff --check` 通过。

完整 WPF 逐类测试循环、一键安装及真实 Playnite 宿主仍未验。用户旧诊断 `20261018T012631286Z-e027df9d` 的 summary/TRX 和 DLL/PDB 身份没有保留，因此无法确认那次测试读到的具体源码与程序集；旧失败的根因仍未定位。本次 R09 通过只证明当前源码和本次匹配的构建产物。

## 文件

- [Release 构建日志](build/build.log)；[依赖还原日志](build/restore.log)
- [弹窗策略日志](tests/notification-popup-policy.log)；[TRX](tests/notification-popup-policy.trx)
- [通知持久化日志](tests/notification-record-persistence.log)；[TRX](tests/notification-record-persistence.trx)
- [R09 日志](tests/r09-focus.log)；[TRX](tests/r09-focus.trx)
- [源码与程序集身份](verification-identity.json)

## 2026-10-21 SDK8 当前源码复核

SDK `8.0.423` 官方 Release solution build 0 warning / 0 error、XAML 25/25；通知例外策略 22/22、Worker 通知记录/重试持久化 12/12、R09 2/2。证据与 TRX：[S02 SDK8 复核](../SDK8-S02-RECHECK-20261021/README.md)。完整 WPF 循环、安装和真实 Playnite 仍未验；当前桌面系统指针读取条件不满足 R05 物理鼠标用例。
