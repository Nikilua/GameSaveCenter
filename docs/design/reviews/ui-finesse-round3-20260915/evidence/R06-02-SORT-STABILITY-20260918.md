# R06-02 排序提示与稳定性证据

日期：2026-09-18  
实现提交：`c577afc587a63acd7c8fbe73183812f574008f7d`（`补齐表格稳定排序规则`）

## 范围与实现

先核对最新生产代码：共享 `WpfUiProduction.xaml` 已有 `SortDirection` Ascending/Descending 触发器和 `CanUserSortColumns=True`，缺口是生产 Save/Task/Media 表格没有统一的原始值、未知值和稳定次键契约。本批复用现有 DataGrid、绑定、滚动条、DTO 和命令链，新增 `DataGridStableSortController` 与四组生产 profile：

- Save History：时间默认降序；文件数、容量按数值；设备、备注按文本；`BackupId` 为稳定次键。
- Save Candidates：置信度默认降序，NaN/Infinity 视为未知；状态、路径、原因按原始字符串；路径与 PlayniteId 组成稳定次键。
- Task Queue：本地时间默认降序；任务、游戏、详情按原始文本；状态按枚举值；进度按数值，排队中 0 和负值为未知；`TaskId` 为稳定次键。
- Media Inbox：捕获时间默认降序；类型/来源按枚举值，未知枚举末尾；文件名、原因按原始文本；`MediaId` 为稳定次键。

同一列再次触发时升降序切换，切换列时使用该列默认方向；控制器同步 `DataGridColumn.SortDirection`，因此共享箭头与真实 CollectionView 排序共用一条状态路径。未知值在升、降序均置于末尾，避免降序把缺失数据推到首屏。Maintenance 表格保留已有共享列头契约，本批不新增业务排序 profile。

## 自动化与行为证据

- `R06SortingBehaviorTests`：`4/4`。使用真实 WPF `ListCollectionView` 与隔离 STA DataGrid，覆盖时间/整数/容量的原始值排序、稳定次键、未知值末尾、同列升→降→升切换、箭头状态，以及清空后以乱序重新加入仍得到相同顺序。
- Save History 测试明确验证同一时间的 `a,b` 由 `BackupId` 稳定排序；文件数切换后仍为数值顺序，不是字符串顺序。
- Task 测试明确验证 `2` 在 `10` 之前，负值和排队中 0 均在已知进度之后。
- Media 测试明确验证来源升序为已知值后未知值，降序仍保持未知值末尾。
- `R06ColumnWidthPersistenceBehaviorTests` 在独立测试进程中 `5/5`，确认排序接入未破坏 R06-01 列宽/窄窗滚动边界。
- `python scripts/validate-source.py`、`check-xaml.ps1`：XAML `24/24`；`git diff --check` 通过。
- `scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot .tmp/r06-02-build-final`：Release 全解决方案编译 `0 warnings / 0 errors`，Playnite 目标保持 `net462`。

曾尝试把 R06-01 与 R05 多组 WPF 行为合并到一个 vstest 进程；该组合批次为 `33 passed / 7 failed`，失败包括同一 AppDomain 重复创建 `System.Windows.Application`、隐藏 Window 尚未布局和焦点/虚拟化夹具未得到可视元素，是仓库既知的并行/生命周期隔离问题，不作为本项通过证据。R05 相邻项此前均按独立进程完成定向通过；本项只把可复现的独立结果写入上文。

## 视觉与布局证据

最终命令：`scripts/render-qa.ps1 -Configuration Release -Output .tmp/r06-02-render-final3`。

- 报告：`.tmp/r06-02-render-final3/render-qa-report.txt`。
- `Commit: c577afc587a63acd7c8fbe73183812f574008f7d`，`WorkingTreeClean: True`。
- `Themes: light,dark`，`DpiScale: 1.00`（仅离屏逻辑 DIP），357 张 PNG，报告结尾 `render-qa OK`。
- Save History `headers=7`、Task `headers=6`、Media Inbox `headers=5` 均报告 `resize=true sort-arrow=visible`；共享箭头契约在浅/深主题都保留。
- Light/Dark 的 Save、Task、Media `1040×700` viewport probe 均为 `OK`；报告覆盖 50/400/2000/4468 数据量、滚动探针，以及 `2560×1440 -> 1100×720 -> 2560×1440` 恢复。
- 已人工查看 `Save-1040x700-tab0.png`、`Task-1040x700.png`、`Media-1040x700-tab0.png`：列头、状态徽章、进度条、危险/恢复按钮和既有滚动条没有被排序接入挤压或吞色。

## 边界与下一步

行为证据使用合成 DTO、隔离 CollectionView、STA WPF 和隔离构建目录；没有读写真实存档、媒体、用户云端或诊断数据。没有把离屏图写成真实呈现帧、真实 Playnite 点击、UIA/读屏、OS 输入/IME、物理 DPI/跨屏、ETW 或宿主性能证据。R06-02 的“稳定”覆盖本批生产表格的刷新重排与稳定次键，不等价真实宿主多线程刷新录像。Maintenance 表格排序 profile 未在本批扩展。

下一项：R06-03 选中焦点区分。先盘点共享 DataGrid/ListBox 的 selected、keyboard focus、hover、失焦和错误行资源，再用浅/深主题中的正负行为夹具验证选中底色不吞徽章与危险动作。
