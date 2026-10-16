# R13-07 当前 main 队列筛选与分页复核

复核日期：2026-10-16。构建输入基于 `main` 提交 `ea1e1aead8ffd8c89d09cd753519a522f327f91b`，包含本次未提交的 R13-07 源码改动；源文件和编译产物 SHA-256 见 [hashes-sdk9302.txt](hashes-sdk9302.txt)。

## 复用现有能力与本次改动

- 已有的 `CloudTransferStatusRequestDto`、Worker 状态查询、维护页筛选控件、全局/筛选汇总、分页 consistency token 和 `SelectionAnchorResolver` 均继续复用；没有新增服务、DTO、命令或筛选体系。
- `DashboardViewModel.LoadCloudTransferPageAsync` 现在调用内部分页追加 helper。合成重叠页验证 TransferKey 大小写不敏感、保留已有对象、页内重复不重复加入；只有全重复页时不发空的集合 Reset 通知。
- 同一生产路径调用的选择恢复 helper 覆盖刷新后按稳定 key（忽略大小写）恢复新 DTO、选中项仍在后页时保留待恢复 key，以及目标项消失且无后页时按原位置边界回退。
- 在实际 `MaintenanceView` 的隔离 STA 窗口中操作状态、类型、时间 ComboBox 与游戏/设备 TextBox，核对五个绑定值变化；汇总文本通过生产 `FormatCloudTransferScopeSummary` 计算。正向核对“当前筛选 5/8 项”和“全局 8 项”，空结果负例为“当前筛选 0/8 项”。该视图夹具使用合成 `INotifyPropertyChanged` DataContext，不执行真实插件 IPC 命令。
- Worker 的合成状态查询覆盖游戏、来源设备、更新时间筛选和全局计数；错误设备负例返回筛选计数 0、全局计数 3、空列表。`CloudTransferStateTests` 还覆盖状态查询、全量汇总/分页明细和过期 consistency token。

## 本机验证

仓库 `global.json` 锁定 SDK `8.0.423` 且禁止 roll-forward；本机只有 SDK `9.0.302`。通过 `.tmp` 内临时 SDK 9 runner 和隔离 `GscBuildOutputRoot` 执行定向 Release 构建，VSTest 为 `17.14.1`：

| 验证 | 结果 | 范围 |
| --- | ---: | --- |
| Playnite R13 行为 | `16/16` | Playnite 插件 `net462`、测试 `net472`；含真实生产 `MaintenanceView` WPF STA 控件、分页去重和选中恢复 |
| Worker 云队列状态 | `14/14` | `net8.0-windows10.0.19041.0`；合成过滤、汇总、分页与一致性 token |
| 源码校验、`git diff --check` | 通过 | 当前工作树 |
| R00/R01 freshness | `14` 条记录、`0` 需重跑、`0` 匹配源路径 | package identity `not-provided`；JSON 不代替运行时或宿主验证 |

Playnite R13 测试项目与引用项目在隔离输出中编译成功；这不是完整 solution build，也不是 SDK `8.0.423` 或 CI SDK `10.0.401` 结果。首次把筛选 UI 放入独立 STA 测试后，发现它提前关闭 `Application.Current` 的 Dispatcher，影响同类 WPF 测试；随后将筛选操作并入原有维护页 STA 用例，最终整类稳定通过 `16/16`。只保留最终成功 TRX。

## CI 与未验证边界

#1044/run `38894241925`（提交 `ea1e1aea`）的 SDK10.0.401 job `116950776709` 已成功。SDK8 job `116950777158` 的编译/测试步骤成功，当前在离屏渲染 QA；job 尚未终态。该 run 不包含本次 R13-07 源码改动。R13-07 推送后产生的新 Actions run 才会验证本次代码，必须等 SDK8 与 SDK10.0.401 两条 lane 都到终态并读取失败日志，之后才能收口本阶段。

WPF 筛选控件在这里验证的是生产视图绑定；未在实际 Playnite 插件宿主中执行过滤刷新/IPC，也未运行 RenderHarness、真实 Playnite/package-host、独立 UIA/读屏/IME、物理 DPI/跨屏、最终呈现帧、ETW 或宿主性能。仅用合成队列数据，未读写真实存档、媒体或云端。下一项为 `R13-08 失败分类帮助`，在本阶段两条 CI lane 终态后推进。
