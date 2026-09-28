# Q13-03 进程映射行内删除目标（2026-09-29）

## 已确认的问题与修复

维护页“进程映射”表格的行内“删除”按钮此前没有传入当前行 DTO；`DeleteProcessMappingCommand` 则读取 `SelectedProcessMapping`。因此当点击行与单独选中的详情行不一致时，删除请求可能落到旧选中项。

提交 `256a40f2` 将行绑定 DTO 设为命令参数，并提供基于 EXE 名的 Automation Name/HelpText。DashboardViewModel 命令只接受明确的 `ProcessMappingDto`，忙碌态或缺少参数时不可执行；实际请求使用传入 DTO 的 `ExecutableName`，并在显示详情处同步该目标。复用现有 Worker IPC 消息和 `ProcessMappingDto`，没有增加服务或 DTO。

## 行为与构建证据

- `ProcessMappingInlineActionBehaviorTests 1/1` 使用真实生产 `MaintenanceView`、生产 DataGrid、生产行内 Button 与两条合成 DTO，在 Light/Dark 下执行。测试保持 A 行选中，取得 B 行实际容器和按钮，在按钮中心做 WPF HitTest，再通过该 Button 的 AutomationPeer Invoke；捕获到的命令参数只为 B，详情选中项仍为 A。随后 Inspector 删除仍将 A 传入。空参数在此行为夹具的命令上为不可执行负例。
- `WpfUiResourceDictionaryTests.MaintenanceProcessTableSpansFullWidthUntilAMappingIsSelected 1/1` 通过，覆盖生产行绑定及既有 Inspector 目标绑定的来源契约。
- 精确提交身份 Release 构建：XAML `24/24`，solution `0 warning / 0 error`。`scripts/validate-source.py` 与 `git diff --check` 通过。
- 隔离 Playnite 测试程序集 SHA-256：`B6AB42968F40F40FE5AC1FF8F4798774DD86734090B822047CC47E211D3E64CA`。
- 定向结果保存在本目录的 [`inline-action.trx`](inline-action.trx) 与 [`source-contract.trx`](source-contract.trx)。

## 尚未验证

该 STA WPF 回归没有启动真实 Playnite、发送 OS 鼠标输入或执行 Worker 删除请求；真实桌面上的鼠标命中仍待隔离宿主验证。没有访问真实存档、媒体、云端或外发诊断。因此 Q13-03 可控实现/行为证据已具备，最终宿主状态仍未完成。

下一项检查 Q13-04 列宽调整命中热区；先复用共享 DataGrid 模板和既有测试。
