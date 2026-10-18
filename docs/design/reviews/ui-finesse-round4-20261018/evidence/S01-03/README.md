# S01-03 可选列管理

实现提交：`8cdf5253`（`实现表格可选列管理`）

## 实现

- 在存档历史、存档候选路径和任务表格提供键盘可操作的原生列选择菜单；状态/身份必需列固定显示，可选列按稳定键保存。
- 设置只记录用户隐藏的可选列，采用 `v1/<view-key>` 范围；未知旧列键忽略，新版本新增列默认显示。恢复默认只清除当前表格的显隐偏好并保留列宽。
- 共享菜单样式使用主题动态资源。媒体收件箱表格和其虚拟化配置未改。

## 受控验证

- Windows x64，SDK `8.0.423` / MSBuild `17.11.48` / VSTest `17.11.1`。
- 提交后 `scripts/build.ps1 -Configuration Release -OutputRoot .tmp/s01-03-postcommit-20261018 -SkipTests`：完整解决方案构建成功，0 警告、0 错误；XAML 结构检查 24 文件通过。原始诊断目录为 `artifacts/build-diagnostics/20261018T062934190Z-b5d44179`。
- 分进程运行 Playnite WPF 测试：列显隐 `4/4`、列宽 `6/6`、视图配置 `5/5`、R09 焦点视觉 `2/2`，合计 `17/17`。TRX SHA-256：

| 测试类 | 结果 | TRX SHA-256 |
| --- | --- | --- |
| `DataGridColumnVisibilityBehaviorTests` | 4/4 | `AA1B72F5E5D6238CD4EC4E6F5B29B7A51B23CF558B6FE4336B0C4ACE2415AAC5` |
| `R06ColumnWidthPersistenceBehaviorTests` | 6/6 | `496D85484CA906854011CDC94BCE8B7B18F7A1A5FB65BEFC03460CAC38FE218A` |
| `ViewConfigurationDefinitionTests` | 5/5 | `65E1A57238342AF5A2617944209A968866286E91D77859E7B1BCF0E09F24F4AC` |
| `R09FocusOutlineBehaviorTests` | 2/2 | `3A011A1156D8874220B71BB142D29550683314CF02E9A7A0CC55E5F35B607EF7` |

四份原始 TRX 保存在本目录 `tests/`。

- `python scripts/validate-source.py .` 通过。WPF 静态检查扫描 35 个 XAML 文件，0 errors、28 warnings、177 info；告警为已有布局启发式提示。
- 外机提供的 R09 日志报告第 151 行期望 `2,2,2,2`、实际 `1,1,1,1`。当前提交该测试第 151 行检查验证状态边框颜色，厚度断言位于第 154 行；当前提交独立 R09 运行 `2/2`。因此不能将外机堆栈直接对应到当前测试源码，实际 checkout/DLL/PDB 身份未知。

## 未验证边界

本地受控 WPF 测试不代表 Playnite 宿主呈现。未验证 Playnite 中的设置实际保存、双主题视觉、UI Automation/读屏、物理 DPI 或真实键盘交互。完整安装/UI 门禁此前仍在 R05 `GetCursorPos` Win32 error 5 处中止；不得据此声称宿主验收通过。
