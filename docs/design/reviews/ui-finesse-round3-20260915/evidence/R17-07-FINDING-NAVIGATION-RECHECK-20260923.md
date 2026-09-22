# R17-07 检查项一键定位定向复核

日期：2026-09-23  
当前复核提交：`280c839e`（`校正R17-06存储分析证据`）  
实现提交：`e8d581c6`（`补齐诊断版本精确定位`）  
分支：`codex/ui-finesse-round2`

## 结论

R17-07 在当前可控范围内继续记为“已满足，待环境验证”。本次没有新增生产代码，按当前检出版本重新构建并复核已有 Finding/Health/Task 来源、精确版本路由和维护返回语义。

- Worker 迁移、健康巡检和 Finding 持久化定向：`18/18`。包含 4 个迁移夹具、12 个健康巡检行为和 2 个 Finding 持久化行为；隔离旧 `findings` 表会补出兼容 `backup_id`，旧行可读取，新健康行携带稳定版本 ID，resolve 后退出开放队列。
- Playnite `FindingNavigationResolverTests`：`11/11`。覆盖存档路径、云队列、健康诊断直接 `BackupId`、历史标题前缀、缺失精确版本、无版本身份、任务提示、缺失游戏和任务目标负例。
- Playnite 完整 R17 筛选：`15/15`，包含 R17-01 至 R17-06 已登记的行为、来源、存储和隔离账本回归。

## 构建与质量门禁

- 当前检出版本隔离 Release solution：`0 errors / 2 warnings`；两条均为既有 `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs:706` 的 `CS8602`，不是本阶段新增。
- 目标 Playnite 产物仍为 `net462`，测试程序集为 `net472`；没有覆盖 `main` 的旧实现或用户文件。
- `validate-source.py`：通过。
- XAML 结构校验：`24/24`。
- `git diff --check`：通过。
- `validate_wpf_ui.py src/GameSaveCenter.Playnite`：`0 errors / 28 warnings / 162 info`。警告/信息是已有 Canvas、StackPanel/ScrollViewer 和主题资源审查提示；本阶段未以静态扫描替代运行时视觉验证。

## 行为与负例证据

- 健康巡检问题写入并沿用稳定 `PlayniteId + BackupId`；历史没有 `backup_id` 的条目按标题前缀兼容解析。
- 精确版本存在时进入对应 `BackupVersion`；版本消失时返回空精确目标，不选择邻近版本。健康诊断缺少版本身份时保持诊断可见但不回落到失败任务。
- 存档路径问题按稳定游戏 ID进入 Save；云端问题优先进入云队列；任务问题保留任务筛选、选中项和滚动恢复。移除游戏时不跳同名游戏，也不改用当前游戏选择。
- 维护页返回继续使用已有 `WorkspaceNavigationStack`，保留维护页标签、选中诊断项、维护滚动、任务筛选和任务滚动；没有重建导航体系。
- 保留当前游戏选框、滚动条、命令绑定、取消/错误语义、恢复保护、有限列表和 Playnite/net462 兼容；测试只使用合成 DTO、fake 服务、隔离 SQLite/临时目录。

## 未验边界

Demo 原目录不可用，本阶段沿用已恢复的生产基线。未运行真实 Playnite/package-host，因此未宣称宿主点击后的最终呈现、Light/Dark、DPI、UIA、IME、键盘焦点和物理滚动通过；未验真实文件权限、重启时序、presented frame、ETW/系统跟踪或宿主性能，也没有绕过被拒绝的系统跟踪权限。没有写真实存档、媒体、云端或诊断数据。

## 下一步

下一可执行任务为 `R17-08 维护报告可读性`：先盘点现有报告 DTO、采集/导出服务、复制入口和脱敏器，核对摘要、待处理、已验证、未知分组、软件身份、时间/计数一致性及 URL 参数和 Windows 用户路径负例。
