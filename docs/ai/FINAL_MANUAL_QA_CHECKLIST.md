# GameSaveCenter 最终人工验收清单

> 适用版本：0.6.70（当前 `main`，Final Code Gap Closure 已合入）
> 目的：把 `PRODUCT_HARDENING_EPIC_FINAL_AUDIT.md` 中的 MANUAL QA 项目变成可逐项打勾的真实场景清单。
> 每项完成后记录：日期、结果、复现步骤、日志/截图位置。全部通过后，整体 Epic 才能从 `PARTIALLY COMPLETED / MANUAL QA REQUIRED` 推进到 `COMPLETED`。

## 1. Restore 与 Undo

- [ ] 选择真实游戏 A，创建版本 A1。
- [ ] 修改存档后创建版本 A2。
- [ ] 从 A1 执行安全恢复，确认存档内容回到 A1。
- [ ] 执行撤销最近恢复，确认存档内容回到 A2。
- [ ] 恢复过程中断/失败时，确认 PreRestore 快照被回滚且没有半成品。

## 2. Rclone 云端

- [ ] 真实 remote 配置成功，备份后自动上传完成。
- [ ] 断网后执行上传，确认错误分类、退避重试和手动重试行为正确。
- [ ] 凭据失效后执行上传，确认不会无限重试且本地备份仍完整。
- [ ] staging 下载后执行 Rclone check 与 Ludusavi 版本校验，确认拒绝不匹配版本。

## 3. 多设备

- [ ] 两台物理设备建立不同 `DeviceId`。
- [ ] 制造 V1 → A2/B2 → A3/B3 分叉，确认不自动选择胜者。
- [ ] 三种决策 PreferLocal / PreferRemote / KeepBoth 均真实生效。
- [ ] 远端恢复只进入 staging → check → 校验 → 安全恢复链。

## 4. Local Mirror

- [ ] 外置盘未连接时状态为 `Unavailable`，不是系统错误。
- [ ] 同步后镜像文件与源 SHA256 一致。
- [ ] 手工把镜像中某个同大小文件改坏，再次同步会重新复制并修复。
- [ ] 镜像中多余文件不会被删除。

## 5. 启动项与反作弊

- [ ] EXE / LNK / BAT / CMD / PS1 均能真实启动和追踪。
- [ ] 同名不同路径的 EXE 不会被误关闭。
- [ ] 普通辅助工具在反作弊游戏下可自动启动；Trainer/CT/GameModification 被阻止。
- [ ] Unknown 工具在反作弊游戏下只有显式授权后才能自动启动。

## 6. Playnite UI 工作流

- [ ] 游戏右键快捷操作：立即备份、查看历史、验证恢复点、游戏工具、打开设置。
- [ ] 修改器中心拖拽 EXE/CT/LNK/BAT/CMD/PS1，未选游戏时被拒绝。
- [ ] 维护中心复制/导出健康报告，TXT 与 Markdown 内容完整。
- [ ] 元数据灾备导出包含 SQLite、Worker 设置与 Playnite 插件设置；恢复后设置生效。
- [ ] 重建备份索引在空/新 SQLite 下仍能恢复历史，二次重建不重复。

## 7. 页面状态

- [ ] 存档历史加载时显示统一 Loading，空历史显示统一 Empty。
- [ ] 修改器工具加载/空状态使用统一 Presenter。
- [ ] Worker 离线时媒体页显示 Offline。
- [ ] 启用云端但 Rclone 不可用时维护页显示 Degraded。

## 8. 主题、DPI 与键盘

- [ ] Light / Dark / Follow Playnite 三种模式均清晰可读。
- [ ] 高对比度与关闭透明效果时不依赖毛玻璃仍可读。
- [ ] 100% / 125% / 150% / 175% / 200% DPI 无裁切、错位和发虚。
- [ ] 连续窗口缩放时列表虚拟化、滚动和 Inspector 不回归。
- [ ] Tab、Enter、Escape、Ctrl+F 键盘路径可用，焦点可见。

### UI Refactor v1 专项

- [ ] 在 2560×1440、1366×768、1100×720、1040×700 四种窗口下逐一打开首页、存档中心、修改器中心、媒体中心、任务中心、维护中心，确认无重叠、裁切、控件伸出窗口。
- [ ] 每种窗口下确认 Dashboard 顶部 GamePicker 常驻，搜索、筛选、排序、当前运行游戏定位和上次选择持久化不丢。
- [ ] 将窗口从 2560×1440 缩到 1100×720 再拉回，确认 Save/Task/Trainer Inspector、DataGrid/List 视口和页面滚动布局恢复。
- [ ] Light / Dark / Follow Playnite / 高对比度下检查 Button Hover/Pressed/Disabled、ComboBox Popup、DataGrid Header/Row/Selection、ScrollBar 与 Warning/Error/Info 对比度。
- [ ] 使用真实大游戏库检查 Dashboard、GamePicker 和六个工作区的大列表滚动流畅性。
- [ ] 完成后把结果回填到 `docs/ai/UI_REFACTOR_ACCEPTANCE_AUDIT.md` 第 13 节，并附截图或日志位置。

## 9. 大库与长时间运行

- [ ] 真实 900～2000 游戏库下 Dashboard 打开、搜索、筛选、切换 Workspace 流畅。
- [ ] 长时间挂机 24 小时以上无内存/句柄/线程近似线性增长。
- [ ] 中断 Worker 后任务被正确协调，Restore 不会自动重试。

## 10. 提交验收

- [ ] `git status` 干净。
- [ ] `origin/main` 与本地 HEAD 同步。
- [ ] Release 构建 0 warnings / 0 errors。
- [ ] Core / Worker / Playnite 全绿。
- [ ] `validate-source.py`、XAML/WPF 静态门禁、`render-qa` 通过。
- [ ] `playnite.log`、`extensions.log`、`worker-launch.log` 留下加载证据。
