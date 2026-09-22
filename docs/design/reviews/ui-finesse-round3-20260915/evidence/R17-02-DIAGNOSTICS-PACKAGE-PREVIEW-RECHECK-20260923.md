# R17-02 诊断包预览定向复核

日期：2026-09-23  
复核提交：`837528bc`（`codex/ui-finesse-round2`，D 盘工作区 `D:\workplace\github\GameSaveCenter`）  
结论：现有实现满足受控代码与行为门槛，账本状态校正为“已满足，待环境验证”；本批没有新增生产代码，复用实现提交 `2b6e9051`。

## 实际复测

- Playnite R17 定向测试：`7/7`，包含 R17-01 triage 回归 `5/5`、预览请求先于生成、确认/取消接线、取消不写入诊断 ZIP、生成结果路径/大小显示和维护页入口。
- Worker 诊断包测试：`2/2`。`R17DiagnosticsPackagePreviewTests` 使用隔离 Worker 配置验证限制值归一化、类别/脱敏/明确排除项、可选日志和 Preview 不创建目录/文件；既有 `DiagnosticsPackageServiceTests` 使用隔离 SQLite/日志生成真实 ZIP，验证 2 MiB 上限、条目范围、敏感文本脱敏和不包含 `.db`。
- R17-01 Worker SQLite 回归：`2/2`；相关 Playnite/Worker 合计 `11/11`。
- 从当前 checkout 生成的隔离 Release solution：`0 errors`，`2` 条既有 warning，均为 `MediaCenterView.xaml.cs:706` 的 `CS8602`。
- `validate-source.py`、XAML `24/24`、`git diff --check` 通过；WPF 静态检查 `0 errors / 28 warnings / 162 info`。warning/info 属于现有共享布局、主题与颜色提示，不作为最终呈现通过的依据。

## 保留能力与边界

预览只读列出 README、system、worker、dependencies、database 摘要、recent tasks、health、settings、audit 和可选日志尾部；明确排除真实存档/备份归档、媒体、SQLite 文件及表内容、Rclone 凭据/令牌/密码、自动上传和云端写入。确认前不调用创建 IPC；确认后沿用现有创建、脱敏、大小上限、结果路径和打开路径动作。保留当前游戏选框、滚动条、命令/Binding、错误/取消/恢复保护、有限列表性能和 Playnite/net462 兼容。测试没有使用真实用户配置、存档、媒体、云端或对外诊断发送。

本批尚未证明真实 Playnite/package-host 确认框、Explorer 路径权限、真实日志并发、最终浅深主题、DPI/UIA/IME、焦点、RenderHarness presented frame、ETW、物理跨屏或宿主性能；`database.json` 只是 schema/大小/只读完整性摘要，不等于真实数据库内容。Demo 原目录不可用，继续以已恢复生产基线为参考；没有绕过系统跟踪权限，也没有把离屏结果写成真实呈现结论。

下一可执行任务：`R17-03 检查进度预算`，先核对巡检范围、暂停/延后原因、最近成功时间、下轮计划和取消/结束状态。
