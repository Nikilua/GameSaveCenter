# R17-06 存储分析导航定向复核

日期：2026-09-23  
当前复核提交：`1077a7ee`（`校正R17-05隔离账本证据`）  
实现提交：`51cae6b9`（`完善存储分析导航`）  
分支：`codex/ui-finesse-round2`

## 结论

R17-06 在当前可控范围内继续记为“已满足，待环境验证”。本次没有新增生产代码，按当前检出版本重新构建并复核既有实现；旧证据将 Playnite R17-06 写成 `4/4`，本次按源码实际的三个测试更正为 `3/3`，不把表格条件数量当作测试数量。

- Worker `StorageAnalysisServiceTests`：`4/4`。覆盖逻辑索引体积与实际目录扫描分离、失联归档路径计数、最新版本 ID、不可用备份目录、增长预测和取消。
- Playnite R17-06 定向：`3/3`。覆盖失联路径非零提示、稳定游戏/版本 ID 精确解析及缺失负例、Demo 卡片的逻辑/物理指标与两条命令接线。
- Playnite 完整 R17 筛选：`15/15`，包含 R17-01 至 R17-06 当前已登记的行为、来源和隔离账本回归。

## 构建与质量门禁

- 当前检出版本隔离 Release solution：`0 errors / 2 warnings`；两条均为既有 `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs:706` 的 `CS8602`，不是本阶段新增。
- 目标 Playnite 产物仍为 `net462`，构建输出为本阶段隔离目录；没有覆盖 `main` 的旧实现或用户文件。
- `validate-source.py`：通过。
- XAML 结构校验：`24/24`。
- `git diff --check`：通过。
- `validate_wpf_ui.py src/GameSaveCenter.Playnite`：`0 errors / 28 warnings / 162 info`。警告/信息是已有 Canvas、StackPanel/ScrollViewer、主题资源等审查提示；本阶段未把静态扫描当作最终视觉呈现验证。

## 行为与边界证据

- 维护页 Demo 卡片继续区分 SQLite 逻辑索引体积、备份目录文件的磁盘实测和卷剩余空间。
- 归档路径为空或失联时保留逻辑体积并单独计数，提示未计入磁盘实测且不代表占用为 0；备份目录不可用时提示路径状态未知，不能解释成 0。
- TopGames 复用有限排行，按稳定 `PlayniteId` 与最新稳定 `BackupId` 精确打开游戏或版本；目标缺失不回退到同名、当前首项或邻近版本。版本消失时只打开已解析的游戏并报告版本不可用，返回维护中心沿用现有导航目标。
- 当前证据仍保留游戏选框、页面/详情滚动、命令绑定、取消/错误语义、恢复保护、有限列表和 Playnite/net462 兼容约束；测试只使用合成 DTO、fake 服务、隔离 SQLite/目录和隔离构建输出。

## 未验边界

Demo 原目录不可用，本阶段沿用已恢复的生产基线。未运行真实 Playnite/package-host、Explorer/实际权限、最终浅深主题呈现、DPI/UIA/IME/焦点滚动、presented frame、真实文件系统占用时序、ETW/系统跟踪或宿主性能测试，因此不宣称物理跨屏、真实输入、最终屏幕效果或宿主性能已通过。没有写真实存档、媒体、云端或诊断数据，也没有绕过被拒绝的系统跟踪权限。

## 下一步

下一可执行任务为 `R17-07 检查项一键定位`：先核对已有稳定 Finding/Health/Task 来源、导航返回目标和 Save/Media/Settings 入口，再决定可复用的精确定位范围；继续补行为与缺失目标负例，不以静态按钮接线签收交互。
