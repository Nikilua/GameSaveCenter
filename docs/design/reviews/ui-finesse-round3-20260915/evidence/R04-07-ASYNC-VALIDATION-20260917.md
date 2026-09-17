# R04-07 异步校验竞态（2026-09-17）

## 结论

R04-07 在当前设置页路径校验范围内已满足：编辑中的路径可用性检查使用不可变字段快照并在后台执行；每次新字段版本都会取消并替换旧请求，旧请求即使忽略取消并晚返回也不能覆盖新结果；设置页卸载、数据源替换或编辑提交/回滚会使旧结果失效。后台失败只回写可修复的校验提示，不在 UI 线程同步等待文件系统。

## 现状核对与实现

- 当前生产模型原有 `GameSaveCenterSettings.VerifySettings` 同时包含路径可用性和数值范围检查。本阶段保留完整同步方法，供保存状态/最终安全校验继续复用；新增 `VerifySettingsWithoutPathAvailability` 只负责编辑期的快速数值校验，没有重建设置 DTO、保存流程或引入第二套业务规则。
- `SettingsPathValidationSnapshot` 在一次请求开始时复制 Worker、Ludusavi、Rclone、存档目录、媒体目录和可选本地镜像路径；`SettingsPathValidationService` 在 `Task.Run` 中复用原有 Worker 文件名、环境变量、目录/文件、缺失叶目录、不可达磁盘/共享和权限错误规则。
- `LatestAsyncValidationCoordinator` 为每次请求分配递增版本，取消前一个 `CancellationTokenSource`，只对仍为当前版本且未取消的成功/失败回调放行。底层文件系统调用无法被强制中断时，旧任务仍可完成，但其结果不会进入设置页。
- `GameSaveCenterSettingsView` 将逐字段事件合并到 Dispatcher 后读取最新设置快照；结果回到 UI Dispatcher 前再次检查页面已加载和字段版本。`DataContextChanged`、`SettingsCommitted`、`SettingsReverted` 与 `Unloaded` 均使旧请求失效。校验进行中保存提示显示“正在校验路径 · 保存前请稍候”，异常显示“设置路径校验失败，请稍后重试”。

## 验证证据

- 代码提交：`b1d5b68f`（补齐异步设置校验竞态）；RenderHarness 反射兼容修复：`df6f8083`（修复设置校验反射兼容）。两次提交均已推送至 `origin/codex/ui-finesse-round2`。
- `python scripts/validate-source.py` 通过，`git diff --check` 通过。
- clean Release 构建：`scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot .tmp/r04-07-build-final`，XAML 结构 `24/24`，构建 `0 warnings / 0 errors`。
- clean 构建产物中的 R04-07 定向测试 `7/7`：慢 A 请求晚于快 B 请求返回不能覆盖 B；页面取消后完成的请求不回写；真实隔离目录上的后台路径服务保持原目录指向文件负例；既有路径校验和源码接线门禁通过。
- clean RenderHarness 报告：提交完整 SHA `df6f8083ebf4a40d466a69dc78653e6862d14bcc`、`WorkingTreeClean=True`、Light/Dark、offscreen logical DIP、滚动探针 `50/400/2000/4468`、`297` 张 PNG、`render-qa OK`。设置 normal/dirty/invalid 三态均通过；人工抽查 `Settings-state-invalid-1040x700.png` 与 `Settings-state-dirty-1040x700.png`，验证摘要、保存提示、字段和既有滚动区域无本阶段引入的裁切。首轮 RenderHarness 暴露了新增重载与无参数反射入口冲突，已在 `df6f8083` 改为内部 Core 方法并复跑通过。

## 证据边界与后续

行为测试使用合成设置、fake/隔离路径和任务完成源，不写真实存档、媒体、云端或用户配置。RenderHarness 是 offscreen logical DIP，不等价真实 Playnite 嵌入或 presented frame。真实 Playnite 宿主字段输入时序、系统剪贴板/IME、屏幕阅读器、物理 DPI/跨屏、真实网络共享慢调用、ETW、宿主线程/帧率性能仍未验；取消对不可中断的单次 OS 文件系统调用只能做到结果失效，不能强制终止该调用。下一可执行任务为 R04-08 保存反馈闭环。
