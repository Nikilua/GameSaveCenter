# R21 DataGrid UIA 选择项 peer 子树复核（2026-10-15）

## Actions #1030 失败证据

上一测试提交 `dfac9ea4dd22c2fe471994d4c4f37b02d2b0efce` 对应 [Actions #1030 / run `38712426496`](https://github.com/Nikilua/GameSaveCenter/actions/runs/38712426496)。SDK8 job `116346829496` 与 SDK10.0.401 job `116346829591` 均在 `R21AutomationValueBehaviorTests.ProductionMediaInboxSelectionPatternTracksTheSelectedRow` 失败；完整 job logs 已通过 GitHub 插件读取。

两个 runner 的 Release 构建都是 `0 warning / 0 error`；Core `125/125`、Worker `357/357` 通过。R21 类其余 `23/24` 通过，唯一失败为源第 490 行 `ISelectionProvider.GetSelection()` 返回空集合。新增的 `SelectedItem`、唯一 `SelectedItems` 和真实生成 `DataGridRow` 前置断言均通过，因此这次日志不能再归因于普通 WPF 选择状态未设置或行容器未实现。CI 仍没有记录 AutomationPeer 子树/row peer 的状态；根因尚未确认。

| CI SDK | Job ID | diagnostics artifact | SHA-256 |
|---|---:|---:|---|
| 8.0.423 | `116346829496` | `11962675763` | `654e8a41ad88440d2788b56db8ed9d8ece22838c3a1b868ac5d119e248328a5b` |
| 10.0.401 | `116346829591` | `11961594639` | `84ab470bc26575f7932a8c46d82e174326265f262e417948d38c3737e5f6aac9` |

两个 diagnostics ZIP 留在 GitHub Actions artifact 中；没有复制到仓库或 `.tmp`。完整日志表明失败发生在首次父级 selection 查询，`TextServicesContext` 的 COM teardown 异常出现在 xUnit 失败之后，不是失败断言的根因。

## 夹具补强与本机验证

生产 `DataGrid` 的 `AutomationPeer.GetChildren()` 会暴露已实现的行 peer。当前夹具先断言 XAML 的 `SelectionUnit.FullRow`，再从真实生产 grid peer 子树按名称取得 `DataGridItemAutomationPeer`，以 `ISelectionItemProvider.IsSelected` 核对首行；切换选择后核对旧行变为未选中、新行变为选中，并保留父级 `ISelectionProvider.GetSelection()` 项数及名称检查。测试没有修改生产 UI、绑定、命令或 DataGrid 选择模型。

- 本机隔离 SDK `8.0.423`、MSBuild `17.11.48`、VSTest `17.11.1`；Playnite 插件 `net462`、测试 `net472`、testhost .NET Framework `4.8.9181.0`。
- Release 测试项目与引用构建成功，`0 warning / 0 error`；`R21AutomationValueBehaviorTests` `24/24`。
- `python scripts/validate-source.py` 与 `git diff --check` 通过。
- 本地验证仅构建测试项目和引用，不是完整 solution build；未运行真实 Playnite。SDK8 本机结果不替代新提交的 SDK8/SDK10.0.401 Actions。
- 测试源 SHA-256：`E13552DA73AF1FE8D72715CE0D9A0C6D5FC1B36DB0564C359701C58226D77D5C`。
- 测试 DLL SHA-256：`840ABF60DE81DEC74744168F78397085D4CD9B2B3A3FE7AABFE1F3C12EE67CB4`。
- TRX 已按 `scripts/build-diagnostics.ps1` 脱敏；SHA-256：`E33A440A00897E7DC830CA35232B67B0441170F03D81643980E63242EC330550`。
- 首轮直接测试因未传入 `GSC_BUILD_COMMIT` 有 11 项源码身份检查失败，不作为产品结果；按仓库构建身份约定重跑后，24 项全通过。

| TRX | 结果 |
|---|---|
| [R21 Automation Value 行选择 peer 子树](tests/R21AutomationValueBehaviorTests-sdk8423-peer-enumeration.trx) | 24 passed, 0 failed, 0 skipped |

## 收口条件与边界

新夹具是否解决 CI 的父级 `GetSelection()` 空集合，必须等代码提交后的 SDK8 和 SDK10.0.401 Actions 到终态并读取日志确认；本机通过不能作为 CI 已解决的证据。仍未验证真实 Playnite/Windows UIA、Narrator、物理输入、物理 DPI/跨屏、最终呈现帧或宿主性能。
