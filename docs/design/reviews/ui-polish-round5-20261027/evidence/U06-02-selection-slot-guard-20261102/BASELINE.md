# U06-02 / U08-08 选中行容器槽位稳定回归

日期：2026-11-02。用户补充指出，首页最近活动在选中时可能整行右下偏移，导致状态图标被裁，并强调可保留蓝色圆角框，但不要为框线移动行或内容。本批核对生产Overview局部 `OverviewRecentTaskItemStyle` 和共享隐式 `ListBoxItem`：`cb7b78fd` 已把选框拆为独立覆盖层。当前批只补 WPF 回归断言，没有修改生产 XAML、主题资源、命令或 Binding；当前源码与既有生产提交 `cb7b78fd514e7d6eaa2a34e341340d5d06610508` 的 `src/` 完全一致。

## 几何合同

扩展 `SelectedOverviewAndSharedListItemsKeepTheirContentPosition`：在 Overview 专用模板和共享隐式模板上，分别对状态图标 tile 与 `ListBoxItem` 容器测量未选中、选中和键盘焦点状态。除了既有 34×34 DIP 图标坐标/尺寸≤0.01 DIP 的断言，现增加整行容器相对 `ListBox` 的 bounds、`Margin`、`RenderTransform` 矩阵和 `LayoutTransform` 矩阵不变断言。边框仍须可见；Overview 圆角保持14 DIP，公共列表8 DIP。这样可以发现整行被挪动或缩放的回归，同时允许只移动边框覆盖层。

生产视觉修改仍由 [`U06-02 选框叠层基线`](../U06-02-selection-overlay-20261102/BASELINE.md) 证明：浅/深主题同数据前后图中蓝色圆角框保留、图标恢复行内原位。本批生产 XAML/资源无差异，所以没有重跑静态 WPF 审查或 render-qa；这里的新增证据是实际 WPF 容器几何行为。

## 编译与行为结果

- SDK：仓库要求的 .NET SDK 8.0.423；编译 Release `GameSaveCenter.Playnite.Tests` 项目及 Contracts、Core、Playnite、HostAuditSeeder 依赖。目标方法 TRX 为 [`selection-container-slot.trx`](selection-method/selection-container-slot.trx)，1/1 通过、0失败；类隔离 TRX 为 [`R10RecentAccessBehaviorTests.trx`](r10-class-tests/R10RecentAccessBehaviorTests.trx)，3/3通过、0失败、0 skip。类隔离使用与仓库脚本一致的 `RunConfiguration.DisableAppDomain=true`、`-m:1`、`-nodeReuse:false` 设置，日志没有 COM 收尾异常。
- 首次使用独立 `--artifacts-path` 的还原停在“确定要还原的项目”，用户态取消，exit 1，测试未启动；下一次不隔离并行度的目标运行启动多个空闲 dotnet 节点且无输出，也被取消。没有把两次取消记作测试通过；原始还原日志与逐次决策见 [`attempts.json`](attempts.json)。
- 单节点编译后的目标方法 1/1 通过、exit 0，但无隔离 AppDomain 的 TestHost 在收尾时输出 `InvalidComObjectException`；原日志已保存，不当作干净门禁。随后使用仓库正式 Playnite 隔离设置运行整个 R10 类，获得干净 3/3，作为本批行为结论。
- 测试源码 SHA-256 `5035642259941313D59A2645AF3C237790CB29C8B8D412B79BD1820FF645F812`；编译的测试程序集 SHA-256 `18395732D765F5DA1F67DED6AE1BFCEBDF3163A8BC74474882394AD248CA2573`。两次原始 TRX 和日志哈希列于 [`runs.json`](runs.json)。Source Validation exit 0，`git diff --check` 无错误。

这是代码回归，不是 Playnite 宿主签收。U06-02/U08-08 继续“代码完成待验收”；真实 Playnite、外部 UIA、实体键鼠/IME、物理 DPI 仍未验。当前 test-only 源候选之后还须按本轮要求完成冻结候选中文根入口完整链、独立 `scripts/build.cmd` 和双 SDK GitHub CI；不得用本报告的定向测试替代完整交付门禁。
