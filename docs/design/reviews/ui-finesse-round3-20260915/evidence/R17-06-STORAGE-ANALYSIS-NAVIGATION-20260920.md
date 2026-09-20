# R17-06 存储分析导航证据

日期：2026-09-20  
实现提交：`51cae6b9`（`完善存储分析导航`）  
分支：`codex/ui-finesse-round2`

## 结论

R17-06 在当前可控范围内已满足，状态保持“待环境验证”。本阶段先核对了现有 `StorageAnalysisService`、`StorageAnalysisDto`、TopGames 排行、`TaskSourceNavigationResolver`、维护页 Demo 卡片和 Save 工作区加载链，没有重建已有服务或引入新的导航体系。

- 维护页 Demo 卡片明确区分 SQLite 逻辑索引体积、备份目录文件的磁盘实测和卷剩余空间。
- 索引行的归档路径为空或失联时单独计数并保留逻辑体积，提示“未计入磁盘实测，不代表占用为 0”，备份目录不可用时提示路径状态未知，不能按 0 解释。
- TopGames 复用现有有限排行，补充最新稳定 `BackupId`；“查看游戏”按稳定 `PlayniteId` 精确打开存档中心，“查看版本”按稳定游戏/版本 ID 精确选中版本。
- 游戏或版本不存在时不回退到同名或当前首项；版本消失时只打开已解析的游戏并报告版本不可用。返回维护中心继续复用已有导航返回目标。
- 保留现有游戏选框、页面/详情滚动、命令绑定、取消/错误语义、恢复保护、有限列表和 Playnite/net462 兼容；没有写真实存档、媒体、云端或诊断数据。

## 自动证据

- Worker `StorageAnalysisServiceTests`：`4/4`。覆盖逻辑体积与实际目录扫描分离、失联归档路径计数、最新版本 ID、不可用备份目录、增长预测和取消。
- Playnite R17-06 定向：`4/4`。覆盖失联路径非零提示、稳定游戏/版本 ID 精确解析及缺失负例、Demo 卡片指标和两条命令接线。
- Playnite R17 全量筛选：`15/15`。
- 外部隔离 Release solution：`0 errors / 2 warnings`，两条为既有 `MediaCenterView.xaml.cs:664` nullable warning；目标仍为 Playnite `net462`。
- `validate-source.py`：通过；XAML 结构校验：`24/24`；`git diff --check`：通过。
- `validate_wpf_ui.py src/GameSaveCenter.Playnite`：`0 errors / 27 warnings / 162 info`。警告为既有 Canvas、StackPanel/ScrollViewer 审查提示；没有新增错误。本阶段未以该扫描替代运行时视觉验证。
- 本阶段隔离 Release 构建目录 `.tmp/r17-06-solution` 已在校验后清理。

## 视觉、宿主与边界

原 Demo 目录不可用，本阶段沿用已恢复的生产基线，仅调整当前可见 Demo-first 存储卡片的事实标签、诊断说明、有限排行和导航入口；未启用旧的隐藏存储卡片，也未替换当前选框或滚动条系统。没有运行真实 Playnite/package-host、Explorer/实际权限、最终浅深主题呈现、DPI/UIA/IME/焦点滚动、presented frame、ETW/系统跟踪或宿主性能测试，因此不宣称物理跨屏、真实输入、真实文件系统占用时序或最终屏幕效果已通过。测试只使用合成 DTO、fake 服务、隔离 SQLite/目录和隔离构建输出。

main 当前仍有用户修改（Dashboard R08 文件、`src.zip`、未跟踪对话框文件），本阶段未触碰、未合并。

## 下一步

下一可执行任务为 `R17-07 检查项一键定位`：先核对维护行动项已有稳定 Finding/Health/Task 来源、导航返回目标和 Save/Media/Settings 入口，再决定可复用的精确定位范围；继续先做小批量和负例，不把“点击按钮”静态接线当作交互签收。
