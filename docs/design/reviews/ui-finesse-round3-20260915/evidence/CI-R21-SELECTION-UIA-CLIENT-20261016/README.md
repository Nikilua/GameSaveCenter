# R21 媒体收件箱 UIA 选择客户端复核（2026-10-16）

## 前序 CI 失败

推送基线 `2252fad7c0e1d682d00f7314efbba296f6699292` 对应 [Actions #1031 / run `38834172655`](https://github.com/Nikilua/GameSaveCenter/actions/runs/38834172655)，已终态失败。SDK10.0.401 job `116759417854` 和 SDK8.0.423 job `116759418158` 都在 `R21AutomationValueBehaviorTests.ProductionMediaInboxSelectionPatternTracksTheSelectedRow` 失败。两个完整 job log 已用 GitHub 插件取得。

日志显示 Release build `0 warning / 0 error`，Core `125/125`、Worker `357/357`；R21 为 `23 passed / 1 failed`。失败位于测试第 496 行：父级 `ISelectionProvider.GetSelection()` 返回空集合。之前 `SelectedItem`、唯一 `SelectedItems`、已生成的 DataGridRow，以及行 peer `ISelectionItemProvider.IsSelected` 检查已经通过。断言失败之后的 `InvalidComObjectException` 是 WPF testhost 收尾输出，不是断言原因。根因仍待新的 SDK8/SDK10.0.401 run 验证。

| CI SDK | Job ID | diagnostics artifact ID | ZIP SHA-256 |
|---|---:|---:|---|
| 8.0.423 | `116759418158` | `12018828064` | `4c93196d2bde09dd0ebd2f5e4cdb73dc0cdf56cbfc7593f3675dd6c1508a0819` |
| 10.0.401 | `116759417854` | `12018638993` | `0220f97a7480ae3d2b7ec820290b06530de7abc7980deff9e6327aadbb1950e1` |

## 测试校正与本机结果

此前测试直接调用 WPF peer 的父级 Selection provider。此次保留生产 `DataGrid.SelectionUnit=FullRow`、`SelectedItem`、`SelectedItems` 和行 peer 状态检查，改用 `AutomationElement.FromHandle` 获取窗口中的“媒体收件箱”，再通过 `SelectionPattern.Current.GetSelection()` 查询 UIA 客户端可读到的选中行。夹具验证首行为“媒体 A”，切换后旧选择项变为未选中、新选择项为“媒体 B”且仍为选中，并检查当前 DataGrid 选择对象。生产 XAML、绑定、命令和选择模型未改。

- 本机 SDK `8.0.423`、MSBuild `17.11.48`、VSTest `17.11.1`；WPF testhost 为 .NET Framework `4.8.9181.0`。
- Playnite `net462` 测试项目 `net472` Release build `0 warning / 0 error`；UIA 选择单项 `1/1`，完整 `R21AutomationValueBehaviorTests` `24/24`。
- `scripts/validate-source.py` 与 `git diff --check` 通过。首次默认并行的 `dotnet test` 两分钟无输出且未创建结果目录；按用户此前授权结束该次 SDK8 调用，再以 `-m:1` 构建、运行，结果稳定通过。该停滞调用没有可计入的测试结果。
- 本机 testhost 与 CI `.NET Framework 4.8.9345.0` 不同；本机 SDK8 结果不能替代新提交的两条 CI lane，也不代表真实 Playnite、独立进程 UIA client、Narrator 或最终呈现。
- 测试源 SHA-256：`1878915B46AFDA3889D687FFE4B60279A94CDD93F0368B9B1CB74BB2760CD943`。
- Playnite 测试 DLL SHA-256：`9AB0FBEE289F00A00E5F7E6A5B43C9C0019372A160F7849AF455CF2CF2EAACF4`；MVID `ccd66d2b-8cf6-4950-ac6c-98ad44f29000`；ProductVersion `0.6.73+2252fad7c0e1d682d00f7314efbba296f6699292`（本机组建的 GSC_BUILD_COMMIT 是父提交）。
- TRX SHA-256：`0D006E025F47476F2163B5AEE31EF0104F6CC7D93E5124D979D6CD3D23F89797`。

| TRX | 结果 |
|---|---|
| [R21 UIA 客户端选择行为](tests/R21AutomationValueBehaviorTests-sdk8423-uiaclient.trx) | 24 passed, 0 failed, 0 skipped |

本次测试夹具已于 `b1e7b652c938463f2f50ac74cb8157c4899daa1d` 用中文提交并推送至 `main`。Actions #1032/run `38838015414` 已终态成功，总耗时 `46m 33s`：SDK8.0.423 job `116771387686` 成功（`46m 29s`，含 Playnite 扩展打包和构建产物上传），SDK10.0.401 job `116771387396` 成功（`21m 42s`）。两个 Release build/test lane 均通过，故修正后的 R21 行为进入两条完整测试门禁。GitHub Actions 页面还列出 diagnostics 与 Windows preview artifacts；这不代表安装或真实 Playnite 宿主验收。

真实 Playnite、Windows 独立 UIA/Narrator、OS 键盘/IME、物理 DPI/跨屏、presented frame、ETW 与宿主性能仍待验。
