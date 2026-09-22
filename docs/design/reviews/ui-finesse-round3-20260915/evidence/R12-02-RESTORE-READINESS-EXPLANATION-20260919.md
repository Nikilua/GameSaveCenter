# R12-02 恢复校验结果解释

日期：2026-09-19
实现提交：6cc3a618（补齐恢复校验结果说明）
分支：codex/ui-finesse-round2

## 实际实现

- 先复用现有 RestoreReadinessDto、RestoreReadinessService、SQLite JSON 持久化和 SaveCenter 详情卡，没有新增服务、IPC 或另一套状态系统。
- Worker 现在记录 Manifest 的哈希覆盖数/可覆盖总数，并明确区分 NotAvailable、Partial、Validated、Failed。
- 有效 Manifest 但没有文件哈希时返回 Warning，摘要明确“这不等于校验成功”；部分覆盖时返回 Warning/Partial，显示覆盖比例并说明未覆盖文件不能视为已校验。
- 详情卡增加哈希状态和覆盖量；已检查超过一天的结果显示“结果较旧，建议重新验证”。原有状态、摘要、文件/大小、检查时间、验证命令和页面滚动保持。
- 原本断言完整成功的健康检查合成夹具补入正确 SHA-256；损坏、缺失、取消和隔离目录失败夹具仍保持负例。

## 行为与构建证据

- RestoreReadinessTests：13/13 通过，覆盖无哈希负例、部分哈希覆盖、哈希不匹配、Manifest 无效、取消、隔离目录清理和持久化重启。
- UiDisplayMappingTests：19/19 通过，覆盖未提供哈希、部分覆盖和旧结果文案。
- 隔离 scripts/build.ps1 -Configuration Release -OutputRoot .tmp/r12-02-full-build-v2：XAML 24/24；Release solution 0 warning / 0 error；Core 85/85；Worker 324/324；Playnite source 类组和 84 个 WPF 类隔离进程全部返回 0。
- 同一 commit 身份下，用户安装日志涉及的 WpfUiResourceDictionaryTests 为 137 passed / 39 skipped / 0 failed，总计 176；原 SaveWorkspaceKeepsAllPrimaryCommandsReachableAtHighDpi 未复现。
- validate-source.py、XAML 检查和 git diff --check 通过；WPF 技能静态审查为 0 errors / 24 warnings / 177 info。warning/info 是既有外层滚动、Canvas/颜色令牌提示，本批未新增 error。

## 事实边界

- 证据使用合成 ZIP/Manifest、fake/隔离 SQLite、隔离 STA WPF 和 .tmp 构建目录；没有读写真实存档、媒体、云端或外发诊断。
- Demo 原目录不可用，视觉检查继续沿用恢复生产基线；没有把离屏/STA 结果写成真实 Playnite 嵌入、物理 DPI、跨屏、presented frame、ETW 或宿主性能通过。
- main 的用户未提交文件未触碰；main 上既有 DEV-INSTALL-008 的 73 failed / 588 passed / 57 skipped 与安装器退出 1 仍是独立基线，尚未在 main 上覆盖或重写。

下一可执行任务：R12-03 目标路径核对。继续推进前先复用现有目标解析、路径映射和恢复保护能力。
