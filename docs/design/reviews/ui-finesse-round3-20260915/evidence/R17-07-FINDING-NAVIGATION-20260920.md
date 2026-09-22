# R17-07 检查项一键定位

日期：2026-09-20  
代码提交：`e8d581c6`（`补齐诊断版本精确定位`）  
分支：`codex/ui-finesse-round2`

## 本阶段事实

先复核了现有 Finding/Health/Task 导航能力：存档路径问题已经按稳定 `PlayniteId` 进入 Saves，任务问题已经保留任务筛选、选择和滚动返回，云端问题已经进入云队列；`WorkspaceNavigationStack` 已保存维护页标签、选中诊断项、维护滚动、任务筛选和任务滚动。因此没有重建已有导航服务或返回栈。

本阶段只补健康巡检问题的稳定版本链：

- `ValidationFindingDto` 增加 `BackupId`，SQLite `findings` 表增加兼容迁移列；健康巡检写入稳定 `PlayniteId + BackupId`，`LATEST_BACKUP_EMPTY` 也保留对应版本 ID。
- 版本诊断新增独立 `BackupVersion` 路由，加载后只按稳定 ID 选择目标版本；版本不存在时保留诊断并显示“未选择其他版本”，不邻近回退。
- 历史没有 `backup_id` 的健康诊断兼容既有标题前缀 `备份恢复校验需关注：{BackupId}`；健康诊断缺少版本身份时保持无可用定位，不误导到失败任务。
- 维护诊断选择键纳入 `BackupId`，避免同一游戏多个问题版本刷新后选错；回到维护中心继续复用原返回栈和滚动恢复。

## 行为与负例证据

- Playnite 实际导航解析测试 `FindingNavigationResolverTests` 覆盖健康诊断的直接 `BackupId`、历史标题前缀、找不到精确版本和无版本身份四种路径；精确版本返回 `BackupVersion`，缺失版本返回空目标，不选择邻居，无身份不回落到任务中心。
- Playnite R17 全量定向 `15/15`，包含现有维护、存储、账本、诊断包回归和本阶段导航接线源契约；构建产物为 `net462` Playnite 插件与 `net472` 测试。
- Worker 迁移、健康巡检和 Finding 持久化定向 `18/18`：真实隔离 SQLite 旧 `findings` 表在初始化时补出 `backup_id`，既有行默认空值仍可读取；新健康行携带 `backup-1`，resolve 后退出开放队列。
- `validate-source.py` 通过；XAML 结构检查 `24/24`；`git diff --check` 通过。

## 构建与质量门禁

项目隔离 Release solution 构建成功：`0 errors`，仅保留既有 `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs:664` 的 2 条 `CS8602` warning。WPF 质量扫描为 `0 errors / 27 warnings / 162 info`；警告仍是既有 Canvas、有限视口和滚动容器提示，本阶段没有新增页面视觉体系。

## 边界与未验项

以下内容没有被本阶段测试伪装成通过：

- 未运行真实 Playnite/package-host，因此未宣称实际宿主中点击后的最终呈现、Light/Dark、DPI、UIA、IME、键盘焦点和物理滚动通过。
- 未使用真实用户存档、媒体、云端或诊断目录；行为使用合成 DTO、fake 服务和隔离 SQLite/临时目录。
- 未绕过 ETW/系统跟踪权限，也未将离屏逻辑 DIP 或代理测试写成 presented frame、物理跨屏或宿主性能证据。
- Demo 原目录当前不可用，本阶段沿用已恢复的生产基线。`main` 存在用户改动、`src.zip` 和未跟踪对话框文件，均未触碰、未合并。

临时隔离构建目录 `.tmp/r17-07-solution` 在文档提交前清理。下一可执行任务：`R17-08 维护报告可读性`，先盘点现有报告 DTO/导出服务和脱敏器，重点核对摘要、待处理、已验证、未知分组、软件身份、时间/计数一致性以及 URL 参数和 Windows 用户路径负例。
