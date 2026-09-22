# R16-03 模板应用范围定向复核

日期：2026-09-23

复核提交：`1f968819`（`codex/ui-finesse-round2`，D 盘工作区）

## 结论

R16-03 的受控实现条件已满足，账本状态校正为“已满足，待环境验证”。本批没有新增生产代码，复用了 `52fbf5de` 的批量模板预览、稳定 ID 目标集合、逐项 Worker 执行和失败重试入口，仅复测当前提交上的 Core/Worker/Playnite 行为与 net462 构建。

## 实际复测

- Core `PolicyTemplateBatchPreviewTests`：`3/3` 通过，覆盖明确勾选、排除项、空选择不回退全部、稳定 Playnite ID、变更字段数和 100 个目标上限；超限不静默截断。
- Worker `PolicyTemplatePersistenceTests`：`2/2` 通过，批量模板执行的策略持久化契约保持有效。
- Playnite `R16PolicyTemplateBatchSourceTests`：`1/1` 通过，确认批量命令使用显式稳定 ID、保留筛选后的选择，成功/失败结果和取消入口接线没有把源契约测试冒充真实点击证据。
- Playnite `net462` 定向构建实际完成，无新增错误；保留 `MediaCenterView.xaml.cs:706` 两条既有 `CS8602` warning。
- `python scripts/validate-source.py`、XAML 结构检查 `24/24`、`git diff --check` 通过。

## 保留能力与边界

- 继续复用 `BackupPolicyTemplateDto`、`BackupPolicyDto`、模板 clone/归一化、游戏操作锁、策略持久化、审计、游戏选框、滚动条、命令绑定、取消/错误/恢复保护和 Playnite/net462；取消不发送批量请求，单项异常继续后续目标，取消异常保持取消语义。
- 测试使用合成 DTO、fake/现有服务契约和隔离构建目录，没有写真实存档、媒体、云端、用户诊断或配置。
- 未验真实 Playnite/package-host 的批量点击、筛选输入、焦点/UIA、最终浅深主题呈现、DPI/跨屏、IME、presented frame、ETW 或宿主性能；Demo 原目录不可用，源契约结果不冒充真实呈现。

下一可执行任务：`R16-04 恢复默认粒度`。先核对单字段/单分类/全部默认入口，并确认路径、云端目标和设备身份不会被全部默认覆盖。
