# U00-03/U00-04/U06-04/U06-07 存档页首屏与工具栏调整

日期：2026-10-30
实现提交：文档提交后补录
测试源码基线：`4f3f82d31cfb9c1208adb5b8841db45afbbae34b` 加本目录 `source-diff.patch`（SHA-256 见 `build-run.json`）。

## 改动

- 将存档历史视图的名称、保存、复制、重命名、删除、恢复和重置操作放进现有 `GscDisclosureCard` 展开区，默认收起。历史范围、列设置和主要存档操作仍留在工具栏；展开区说明只管理表格视图，不修改存档数据。
- 将状态列默认放到时间、类型之后，确保恢复可用状态不会排在默认首屏以外；备注列继续承担最后一列表头样式。
- 保留原有控件名称、事件接线、筛选和业务命令。没有改动游戏选框、滚动条系统或数据恢复安全语义。

## 前后观察

- 变更前引用 U00-04-05 的同样本截图：浅色 [截图](../U00-04-05/save-history-light-1040x700-Ready.png)，深色 [截图](../U00-04-05/save-history-dark-1040x700-Ready.png)。1040×700 DIP 下视图管理动作独占第三行，状态列处于默认横向视口的最右侧。
- 变更后截图：浅色 [截图](after-save-history-light-1040x700-Ready.png)，深色 [截图](after-save-history-dark-1040x700-Ready.png)。视图管理折叠后仅保留两行工具栏；时间、类型、状态依次出现，状态列首屏可读。原有横向滚动仍能到达文件数、大小、设备和备注列。
- 同一固定 UTC `2026-10-28T09:00:00Z`。截图窗口规格为 1040×700 DIP；产物实际位图为 728×516 像素。变更前后像素尺寸及 SHA-256 见 `screenshots.json`。

## 验证

- SDK 8.0.423 / Release 经 `scripts/build.cmd` 编译：8 个项目，0 warning、0 error，exit 0。此批次编译命令有意使用 `-SkipTests`，行为回归单独运行。
- `CompactWorkspaceTableViewportBehaviorTests`：14/14 passed、0 failed、0 skipped，exit 0。新增断言检查时间/类型/状态显示顺序与首屏宽度、默认折叠、UIA 名称，以及展开后名称输入框、保存/复制/重命名/删除/重置操作均可见。
- `scripts/validate-source.py` 通过。WPF 静态审查扫描 10 个生产 View XAML：0 errors；23 条既有滚动容器提示和 4 条信息均在其它视图，修改的 `SaveCenterView.xaml` 无静态提示。
- Render QA：固定 UTC 双主题，334 张图、0 `PROBLEM`，1040×700 DIP 存档页表格有 4/4 可读行。报告含离屏逻辑 DPI，不作为真实 Playnite 或物理 DPI 验收。
- 状态夹具命令通过，共生成 216 张隔离假数据图；覆盖 Ready、Empty、Loading、Error、Stale、Offline 和长备注多选。这里只归档两张 Ready 代表图与摘要，其它一次性图在核验后清理。
- 首次行为类测试 13/14：新测试把视图放在仅测量的宿主里，未建立 PresentationSource，导致可见性断言失败。夹具改为低透明度 WPF Window 并刷新布局后，单测和全类 14/14 均通过；原失败 TRX 与解释保存在 `tests/`。

## 状态与未验边界

U00-03、U00-04、U06-04、U06-07 继续标为“代码完成待验收”。本批没有在真实 Playnite 宿主、外部 UIA、实体键盘/IME 或物理 DPI 环境验收；Demo 资源在当前工作树中不可用，未声称与 Demo 逐像素一致。没有执行真实存档恢复、媒体删除或云端写入。80 项总状态保持 7 项已满足、71 项代码完成待验收、1 项进行中（U09-04）、1 项远端待验（U09-06）。

命令、退出码、SDK、测试结果和证据文件哈希见 [build-run.json](build-run.json)；空间与清理记录在本批清理后补录。
