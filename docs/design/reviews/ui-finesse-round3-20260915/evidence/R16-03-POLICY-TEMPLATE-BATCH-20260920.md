# R16-03 模板应用范围证据

日期：2026-09-20  
分支：`codex/ui-finesse-round2`  
代码提交：`52fbf5de`（补齐策略模板批量应用范围）

## 现状核对与实现

- 最新代码原来只有单目标 `ApplyPolicyTemplateDto`、单游戏命令和单目标 Worker 应用；没有把现有能力误记为批量完成。
- 本阶段复用现有 `BackupPolicyTemplateDto`、`BackupPolicyDto`、`BackupPolicyTemplateCatalog.ClonePolicy`、`BackupPolicyDiff`、游戏操作锁、策略持久化和审计服务，新增 `ApplyPolicyTemplateBatchDto`、逐项结果 DTO 和 `policy.template.batch.apply` IPC。
- `PolicyTemplateBatchPreview` 用稳定 Playnite ID 建立有界目标集合：明确勾选才进入请求，排除项、预计变更字段数和 100 个上限可见；筛选不改变选择，空选择不会回退为全部游戏，超限不会静默截断。
- Save 页面批量区使用有限高目标清单和现有滚动资源，展示名称/稳定 ID 筛选、目标/排除摘要、变更数量、确认文案及最近逐项结果。部分失败项保留“重试”，成功项从目标选择中移除；取消只保留选择和草稿，不发批量请求。
- Worker 读取一次已保存模板快照，逐个解析目标、取得现有游戏操作锁、写策略并追加审计。单目标非取消异常进入失败项并继续后续目标；`OperationCanceledException` 仍传播，避免把取消伪装成部分成功。

## 自动化证据

- 外部隔离源码副本 Release solution 构建：`0 errors / 2 warnings`。警告均为既有 `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs:664` 的 `CS8602`，没有新增错误。
- 定向测试：`PolicyTemplateBatchPreviewTests` `3/3`；`R16PolicyTemplateBatchSourceTests` `1/1`；`PolicyTemplatePersistenceTests` `2/2`。
- `python scripts/validate-source.py` 通过；XAML 结构门禁 `24/24`；`git diff --check` 通过；WPF 技能静态扫描 `0 errors / 28 warnings / 177 info`。本批没有新增 WPF 静态错误。
- fresh restore 在外部副本使用 `--ignore-failed-sources` 时无诊断退出，因此没有把“全新还原”写成通过；为完成受控源码构建，沿用已授权隔离副本的现有 `obj` 资产并用 `--no-restore` 复核。隔离构建目录已清理。

## 公共门禁与未验证边界

- 保留当前游戏选框、滚动条系统、命令/Binding、取消/错误语义、恢复保护、模板一次性复制、有限列表性能和 Playnite `net462` 兼容；请求只接受显式稳定 ID，不写真实用户存档、媒体、云端或诊断。
- 测试使用合成 DTO、fake/现有服务契约和隔离源码目录；源契约测试只证明接线和负例保护，不等价于真实 WPF 点击、动画、焦点、UIA 或最终呈现。
- 未验真实 Playnite/package-host、最终浅深主题呈现、DPI/跨屏、UIA/读屏、IME、RenderHarness presented frame、ETW、宿主性能和真实跨筛选输入；Demo 原目录不可用，继续以恢复生产基线为视觉依据。
- main 工作树现有用户改动和 `src.zip` 未碰、未合并；分支已推送至 `origin/codex/ui-finesse-round2`。

下一可执行任务：`R16-04 恢复默认粒度`。先检查设置页、设置服务与 Worker 配置是否已有单字段/单分类/全部默认入口，确认敏感连接字段不会被默认值覆盖，并为取消保留草稿补真实行为/负例证据。真实宿主呈现、DPI/UIA/IME、ETW 和宿主性能仍未验证。
