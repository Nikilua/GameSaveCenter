# R16-02 策略差异预览定向复核

日期：2026-09-23

复核提交：`e63c62c7`（`codex/ui-finesse-round2`，D 盘工作区）

## 结论

R16-02 的受控实现条件已满足，账本状态校正为“已满足，待环境验证”。本批没有新增生产代码，复用了 `b327d5ef` 的策略 DTO、模板目录、差异比较和保存/取消链，仅复测当前提交上的 Core 行为、Playnite 源契约和 net462 构建。

## 实际复测

- Core `BackupPolicyTemplateCatalogTests`：`5/5` 通过，覆盖 13 个策略字段的已保存/显式值差异、模板复制回退和字段变更通知。
- Playnite `R16PolicyDiffSourceTests` + `R16PolicyTemplateBatchSourceTests`：`2/2` 通过。源码负例确认取消未保存修改的方法体不调用 `RequestAsync`，已有游戏草稿未保存时模板应用命令受保护，页面继续绑定差异列表和一次性模板覆盖语义。
- Playnite `net462` 定向构建实际完成，无新增错误；保留 `MediaCenterView.xaml.cs:706` 两条既有 `CS8602` warning。
- `python scripts/validate-source.py`、XAML 结构检查 `24/24`、`git diff --check` 通过。

## 保留能力与边界

- 继续复用 `BackupPolicyDto`、`BackupPolicyTemplateDto`、归一化规则、原保存/取消命令、游戏选框、滚动条、命令绑定、取消/错误/恢复保护和 Playnite/net462；模板是一次性复制，不建立隐藏继承关系。
- 测试使用合成 DTO、fake/现有服务契约和隔离构建目录，没有修改真实存档、媒体、云端、用户诊断或配置文件。
- 未验真实 Playnite/package-host 的最终差异卡片、浅深主题、UIA/读屏、IME、DPI/跨屏、presented frame、ETW 或宿主性能；Demo 原目录不可用，离屏/契约结果不冒充真实呈现。

下一可执行任务：`R16-03 模板应用范围`。先核对现有模板应用命令、目标筛选、上限和逐项失败重试边界。
