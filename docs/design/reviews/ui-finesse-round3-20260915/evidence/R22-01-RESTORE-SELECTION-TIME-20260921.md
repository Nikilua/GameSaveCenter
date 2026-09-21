# R22-01 恢复流程选择版本时间（2026-09-21）

## 本子批次结论

提交 `391d28b8` 收口 SaveCenter 恢复四步流程第一步的时间显示。`R22-01` 整体仍为“部分满足，待继续”；本批只处理实际显示 `BackupVersionDto.CreatedUtc` 的“选择版本”步骤，没有扩大到比较下拉、历史跳转消息或复制列。

## 复用与实现

- 复用 `BackupVersionDto.CreatedRelativeDisplay`、`CreatedFullDisplay` 和其中包含的原始 UTC 值；没有新造恢复时间格式化器。
- `RestoreWorkflowProgress` 保留原四步状态机、选择/可恢复性检查/目标核对/执行顺序和安全文案；第一步 `Detail` 改用相对时间，`DetailFullDisplay` 提供完整本地时区与 round-trip UTC。其他步骤未提供完整值时回退到原 Detail。
- SaveCenter 恢复流程模板保留现有 ItemsControl、有限页面滚动、PreRestore 保护、取消/错误与 Worker 写入边界；只为 Detail 增加 Tooltip 和 Automation HelpText。

## 实际验证

- `R22TimeDisplayBehaviorTests` `21/21`：选择版本步骤实际由 `RestoreWorkflowProgress.Build` 生成相对正文，完整证据包含原始 UTC；未选择版本的提示和 DetailFullDisplay 回退保持。
- `R12RestoreWorkflowBehaviorTests` `7/7`：四步状态、可恢复性警告、目标失败、PreRestore 失败、回滚失败和成功收口保持；定向命令合计 `28/28`。
- 精确提交身份 Release 隔离构建：XAML `24/24`；Playnite `net462`、Playnite Tests `net472` 均 `0 errors`，保留已有 `MediaCenterView.xaml.cs:671` 两条 CS8602 warning；`validate-source.py`、`git diff --check` 通过；WPF 静态检查 `0 errors / 27 warnings / 177 info`。

## 未验边界

- 本批使用合成 BackupVersionDto、恢复状态夹具和隔离构建目录；没有真实恢复、PreRestore、存档写入、用户目录修改、媒体/云端写入或外发诊断。Demo 原目录不可用，继续以恢复生产基线为准。
- 未声称真实 Playnite/package-host、Windows UIA/读屏、真实剪贴板、系统时钟跳变/跨系统启动周期、DPI/跨屏、最终呈现、ETW 或宿主性能已验证；本批没有新增真实 STA 视觉树签收。main 用户改动未碰、未合并。

## 下一步

继续 R22-01：核对 SaveCenter 比较下拉的 `ComparisonDisplay` 和历史跳转状态消息，先区分用户可见绑定、复制数据与仅内部排序/日志格式，再按边界分批迁移。
