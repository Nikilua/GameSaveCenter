# R11-07 备份结果分层证据

日期：2026-09-19  
实现提交：`02860571`（`补齐备份结果分层与失败证据校正`）

## 实施事实

- 先复用已有 `CloudTransferStatusDto`、云端状态服务、`RetryCloudUpload` IPC 和云端重试队列，没有新增第二套上传服务。
- 新增 `BackupResultDto`，把本地结果与云端后续状态分开：本地成功可以同时处于云端排队、镜像失败、认证待处理、传输中、已上传待远端校验或远端已校验。
- `BackupOrchestrator` 在本地历史版本已经索引并持久化后发布本地成功结果；云端失败仍保留任务失败语义，但任务携带 `HasPartialSuccess` 和对应云端状态。`RetryCloudUploadAsync` 只复制已保留的本地备份，不重新创建 Ludusavi 本地版本。
- `TaskCoordinator`、实时 `TaskEventBroadcaster` 和任务终态复制均保留分层 DTO。Playnite 只忽略“带本地成功结果的云端失败”这一种失败提示，仍对普通失败和取消保持原有错误语义；刷新历史后本地版本继续可见。
- SaveCenter 结果卡片显示本地成功与云端状态；排队/失败/认证状态提供“单独重试云端上传”，`Uploaded` 明确是“待远端校验”，不会误报为已校验。

## 验证

验证均在 continuation worktree 的 D 盘可写源码副本和隔离输出完成；没有触碰 dirty main。结果如下：

- `scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot ...`：XAML `24/24`，Contracts/Core/Worker/Playnite net462/测试程序集 Release `0 warning / 0 error`。
- Worker 分层状态、部分成功终态和实时事件克隆：`9/9`；加上既有云传输状态相邻回归：`20/20`。
- 真实 `SaveCenterView` STA Window 夹具：R11-07 排队/已上传负例/结果卡片 `3/3`；完整 R11 相关 Playnite 定向回归 `14/14`。
- main 日志中被首个失败命中的 `SaveWorkspaceKeepsAllPrimaryCommandsReachableAtHighDpi`、同类大小列源码门禁和 busy 指示器源码门禁在当前分支结构化校正后 `6/6`；校正按 XAML 元素与属性关系核验，不是增加宽泛 `Assert.Contains`。
- `python scripts/validate-source.py`、`scripts/check-xaml.ps1` 和 `git diff --check` 均通过。

## main 安装失败边界

用户提供的 DEV-INSTALL-008 日志显示 Release 构建 `0 warning / 0 error`、Core `83/83`、Worker `311/311`，但 Playnite 为 `73 failed / 588 passed / 57 skipped`，安装器退出 `1`，因此没有进入打包或安装。首个 `SaveWorkspace...` 失败是 `SaveCenterView.xaml` 新增列帮助属性后，旧断言仍要求 `Header` 与 `Binding` 连续排列；这不是主命令不可达的证据。其余失败还包含 STA/AppDomain、离屏视觉树和其他源码形状门禁，不能由本阶段一条修复推断全部消失。该修正只在当前 continuation 分支，未写入 dirty main。

## 证据边界

测试使用合成 DTO、fake TaskCoordinator、隔离 STA Window 和隔离输出目录；未执行真实 Ludusavi/rclone/Worker IPC、真实云端、真实存档、Playnite package-host 安装或宿主呈现。未宣称物理 DPI/跨屏、UIA/读屏、IME、ETW、呈现帧或宿主性能；Demo 原目录不可用，继续沿用恢复生产基线。未绕过被拒绝的系统跟踪权限。

下一可执行任务：按用户要求进入 R00/R01 小批量问题修复与证据校正；在此之后再推进依赖已满足的 R11-08。 
