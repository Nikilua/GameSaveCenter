# R02-01 动作优先级证据

## 结论

R02-01 已满足。当前分支 `codex/ui-finesse-round2` 的代码提交为 `89c9cc3`，按现有生产页面和共享控件能力收口动作角色，没有把 main 的旧页面实现覆盖到当前分支。

- 主页 `OverviewHomeToolbarActions`、`OverviewCurrentGameCard` 和媒体批量栏 `MediaInboxBatchActionRow` 各保留一个主动作角色。
- 存档详情的 `RestoreCommand` 复用共享 `GscWpfUiDangerActionButton`；媒体来源的 `DeleteMediaSourceCommand` 复用共享 `GscWpfUiContextDangerButton`；存档策略模板删除继续使用已有图标危险样式。
- Media Inbox 的待归类/已忽略状态互斥：已忽略状态隐藏 `ApplyMediaClassification` 主按钮，仅显示 `RestoreIgnoredMediaBatch`；恢复按钮默认折叠，列表和检查器两处都只在已忽略状态显示。
- 真实命令、Binding、取消/错误处理、恢复前保护与撤销语义未改；游戏选框、现有滚动条系统、有限列表路径和 net462 兼容未改。

## 实现与行为验证

### 定向测试

执行：

~~~powershell
dotnet vstest .tmp\\r02-01-build4\\bin\\GameSaveCenter.Playnite.Tests\\Release\\net472\\GameSaveCenter.Playnite.Tests.dll --TestCaseFilter:"FullyQualifiedName~R02ActionPriorityTests"
~~~

结果：`R02ActionPriorityTests` 2/2 通过、0 失败、0 跳过。

测试读取当前生产 `OverviewView.xaml`、`SaveCenterView.xaml`、`MediaCenterView.xaml` 和 `WpfUiProduction.xaml`，用 XML/XPath 检查区域主动作数量、条件可见性和危险样式角色；另以 STA WPF `ResourceDictionary` 加载真实样式并实例化生产 `Button`，验证 Primary/Danger 的 Appearance、主动作/上下文动作高度和填充资源确实不同。测试同时保留了危险样式负例区分，未只用字符串存在断言签收交互。

### Release 全量回归

执行：

~~~powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\\build.ps1 -Configuration Release -OutputRoot .tmp\\r02-01-release-final
~~~

| 检查项 | 通过 | 失败 | 跳过 | 总计 |
| --- | ---: | ---: | ---: | ---: |
| Core | 83 | 0 | 0 | 83 |
| Worker | 311 | 0 | 0 | 311 |
| Playnite | 525 | 0 | 57 | 582 |
| XAML 结构检查 | 24 | 0 | 0 | 24 |

解决方案构建为 `0 warning / 0 error`。Playnite 的 57 条跳过仍是已撤销 UI 基线，沿用 R01-08 的分类，不为本项强行改写。

### 受控 RenderHarness 证据

RenderHarness 在提交 `89c9cc3` 上构建为 `0 warning / 0 error`，审计结果为 161 个运行时快照、0 Fidelity、0 失败路由、0 HIGH、0 MEDIUM、73 条已分类的预期 INFO。代表性受控视图检查确认：

- Overview 工具栏只有“全部备份”主动作，当前游戏卡片只有“立即备份”主动作。
- Save History 检查器将“安全恢复”显示为红色危险动作，并与普通保存、对比和撤销动作区分。
- Media Inbox 待归类状态保留“归类所选”主动作；来源规则中的“移除”使用红色危险上下文动作，与“添加来源”区分。

截图位于本轮隔离 `.tmp\\r02-01-audit-final\\screenshots`，仅用于本轮人工检查，不提交为长期证据；可在固定 checkout 上按上面的构建命令和以下命令重现：

~~~powershell
$env:GSC_UI_AUDIT_COMMIT = "89c9cc3"
& ".\\tests\\GameSaveCenter.RenderHarness\\.tmp\\r02-01-harness-final\\bin\\GameSaveCenter.RenderHarness\\Release\\net472\\GameSaveCenter.RenderHarness.exe" audit ".tmp\\r02-01-audit-final"
~~~

源码校验 `python scripts/validate-source.py` 通过。`wpf-apple-desktop-ui` 质量检查扫描 770 个 XAML、0 errors；343 warnings/1106 info 为全仓扫描中既有的临时历史/宿主资源和共享资源提示，不能解释为本项新增缺陷。

## 证据边界

以上截图和运行时样本均为隔离数据、真实生产 WPF 视图/样式和 offscreen logical DIP；不是实际 Playnite 嵌入 Dashboard 的截图，也没有验证用户主题、物理 DPI/跨屏、真实 OS 键盘/IME、屏幕 presented frame、屏幕阅读器、ETW 或宿主帧率/大库现场。已忽略状态的互斥显示由结构化条件测试覆盖，但本轮截图重点是待归类页和来源规则；真实宿主复核仍按环境可用性单独进行。

本轮未启动真实 Playnite、未写真实存档/媒体/云端、未删除真实文件、未外发诊断；临时输出须在本证据同步后清理。

## 下一步

下一可执行小批量为 R02-02“忙碌宽度稳定”，先核对现有忙碌状态模板和按钮最小宽度能力，再补状态切换/负例行为证据。
