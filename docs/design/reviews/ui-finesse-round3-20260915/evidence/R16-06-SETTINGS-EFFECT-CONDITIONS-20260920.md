# R16-06 生效条件说明（2026-09-20）

## 结论

R16-06 已实现，状态为“已实现，待环境验证”。当前代码提交为 `83e7c745`，已推送到 `origin/codex/ui-finesse-round2`。

## 现状核对与改动

- 现有链路为 Playnite `GameSaveCenterSettings.EndEdit` 先保存设置并触发 `NotifyVisualSettingsChanged`，再调用 `ApplySettingsAsync`；插件通过 `settings.update` 发送 `ToWorkerSettings`，Worker 的 `UpdateSettings` 调用 `WorkerOptions.Apply(..., persist: true)` 并重算健康巡检计划。
- 设置页四个分类标题旁补充真实生效条件：外观在设置页即时预览，保存后已打开页面即时重建；工具/目录与备份参数由 Worker 接收并从下一次新任务读取，进行中的任务不切换；自动化轮询/队列/健康计划按下一轮边界读取；随 Playnite 启动 Worker、下次以安全模式启动只影响下一次 Playnite 启动判断。
- 没有把所有字段笼统标为“需要重启”；当前审计没有发现普通设置必须重启 Playnite 的消费点。保留现有云端时段“下一轮 Worker 检查生效”、安全模式“下次启动”说明。

## 证据

- `R16SettingsEffectSourceTests` 两项源链路/负例测试，加上 R16-05 路径与既有路径回归定向测试：当前提交重新编译后 `8/8` 通过。
- 外部隔离 Release solution：`0 errors / 2 warnings`；两条均为既有 `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs:664` 的 CS8602。当前测试构建显式绑定 `GscBuildCommit=83e7c745`。
- `python scripts/validate-source.py` 通过；`scripts/check-xaml.ps1`：XAML `24/24`；`git diff --check` 通过；WPF 静态检查：`0 errors / 28 warnings / 162 info`。

## 未验证边界

- 未运行真实 Playnite/package-host 观察保存后页面、关闭页面后下次打开、Worker 重启和定时轮询的呈现；未验最终浅深主题、DPI/跨屏、UIA/读屏、IME、RenderHarness presented frame、ETW 或宿主性能。
- 证据只读取代码消费链并使用合成/隔离构建，不写真实存档、媒体、云端或用户配置；Demo 原目录不可用，继续以恢复生产资源基线为视觉依据。
- main 工作树用户改动、`src.zip` 和未跟踪对话框文件未碰、未合并；本阶段 `.tmp/r16-06-*` 输出在提交文档前清理。

下一可执行任务：`R16-07 配置导入预览`，先复用现有 `ImportPortableJson`/导入报告能力，核对版本、未知字段、凭据和失败回退边界。
