# R06-03 选中/焦点状态：当前 main 行为复核

日期：2026-10-13（Asia/Shanghai）
生产实现仍来自 `d83c73785c15b72af11c6b8897f359129ba66c26`。本轮生产源码基线为 main `fdeeec8a9d3a7742a63ccbee21c55a50752326c8`；新增行为测试在该 checkout 的工作树中编译，源码 SHA 与 DLL 身份见下。没有改生产代码、DTO、命令或绑定。

## 构建与测试

- 本机隔离 SDK：`8.0.423`，位于 `D:\GameSaveCenterTools\dotnet-sdk-8.0.423`；目标 CI SDK `10.0.401` 独立跟踪，本机结果不替代 CI。
- 官方 `scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot ... -DiagnosticsRoot ...`：Release `0 warnings / 0 errors`、XAML `24/24`；Playnite `net462`、Playnite tests `net472`。隔离输出位于 `D:\GameSaveCenterTools\R06-03-current-main-20261013`；最后构建诊断目录 `20261012T195656918Z-37f15de1`。
- `R06SelectionStateBehaviorTests`：`2 passed / 0 failed / 0 skipped`。失败行失焦选中仍保留 inactive selection surface，不套 error row chrome；成功行键盘焦点使用 `2 DIP` accent border；失焦选中切换到 muted chrome；几何探针比较 cell 与文字布局。
- `R23ProductionResourceStateBehaviorTests.EachProductionPageGridKeepsSelectedFocusAndDisabledRowStatesAcrossThemes`：`1/1`。Task、Media Inbox、Save、Maintenance 四个生产表在 Light/Dark 检查 inactive selection、键盘焦点与 disabled 状态，比较行内 cell/text 几何。
- 新增 `TaskSelectionKeepsFailureBadgeAndCancelActionUsableAcrossThemes`：`1/1`。真实 `TaskCenterView` 生产 XAML、合成 Failed/Running DTO 与 fake Cancel 命令，在两主题检查 hover 进入/离开时共享行底色变化；失败选中行的 Error pill 颜色、可见性和 `InputHitTest` 命中；Failed 终态取消按钮保持禁用；选中 Running 行并聚焦后，取消按钮可用，AutomationPeer `Invoke` 实际 Raise `Click` 并执行 fake 命令。
- `WpfUiResourceDictionaryTests.SharedDataGridSelectionUsesTrainerRoundedCardChrome` 与 `SharedListBoxItemsStayRoundedAndKeyboardFocusable`：`2/2`，用于补充共享模板结构契约，不替代上述运行时行为。
- 总计 `6 passed / 0 failed / 0 skipped`，测试类按独立 VSTest 进程运行。`python scripts/validate-source.py` 通过；`git diff --check` 在文档收尾时复核。

## 程序集与证据身份

- 测试源码 `R23ProductionResourceStateBehaviorTests.cs` SHA-256：`663FF65E6EDF569D05D51D002C45C74D4D125AA2F281617F02BD8081AA84B3FC`。
- 共享资源 `WpfUiProduction.xaml` SHA-256：`8BB716E48B3BC1DE4CC8CA82C7A05A7320072AD856C8318BDC8603DF874D2EA9`。
- Playnite DLL SHA-256 / MVID：`A486B9B34CFB5614E24797F39E2D09CEBFAE51E251299F0F4C5EC8A67AA1BF50` / `354dc807-4006-4a7d-b9ef-59dc034f3420`。
- Playnite tests DLL SHA-256 / MVID：`CB3F3FB068EDAF7E880F146AF892769D4384448F34AEB0DA1AB62FD3AAC0B148` / `d3eadb88-2bc6-470c-8ab5-6d0053b8c5fd`。

脱敏 TRX 保留测试方法、结果与 GUID，去掉本机用户名、计算机名和绝对路径；文件按仓库约定使用 LF，表中哈希对应归档文件字节：

| TRX | 结果 | SHA-256 |
| --- | --- | --- |
| [R06-03-selection.trx](tests/R06-03-selection.trx) | 2/2 | `36E78FD0884125DAFC7F72D4004A1847E81C92D0ADC6D1FDFA6F590911D48B30` |
| [R06-03-grid-themes.trx](tests/R06-03-grid-themes.trx) | 1/1 | `1A6612F8CC7882F7FB823F9BF30462DE77FC5BEF47A867B0F5A6054C08058EA3` |
| [R06-03-state-badge-action.trx](tests/R06-03-state-badge-action.trx) | 1/1 | `52D1A2EC0FD6E2B9FF891E081FE96D06A56450F694BB20688E90200C68D89AC5` |
| [R06-03-resource-contracts.trx](tests/R06-03-resource-contracts.trx) | 2/2 | `814B76D40B4946CFE8F7D29BB2B3737B00CBCC53BFB725D9EA80B920C183A82A` |

## 边界与下一步

hover 通过反射调用 WPF `MouseDevice.ChangeMouseOver` 控制 MouseDevice 状态并触发 WPF hover/leave 路径；这是受控 WPF 行为，不代表真实 OS 鼠标或 Playnite 宿主输入。一次 `SetCursorPos` 尝试没有使测试行进入 `IsMouseOver`，没有计入通过，具体原因未确定。取消操作只执行 fake 命令，不触碰真实任务。

本轮没有启动 Playnite、RenderHarness 或生成当前截图。历史双主题截图仍绑定 `d83c7378`，没有当作当前图像证据。真实 Playnite/package-host、OS 指针、UIA/Narrator、物理 DPI/跨屏、IME、最终呈现帧、ETW 与宿主性能仍待验；没有读取或写入真实存档、媒体、云端或诊断数据。

Actions #947（run `38295749414`，commit `90454586`）与 #948（run `38298023983`，commit `fdeeec8`）截至 2026-10-12 20:00 UTC 仍显示 `In progress`。GitHub 日志需要登录，实际 CI SDK 与步骤不可见。用户指定检查点 #920（run `36677346317`，commit `77de7f4`）现为 `Failure`；公开页面仅显示编译/测试步骤 exit 1，完整日志需登录，失败根因和 runner SDK未知。#946 的完整日志/artifact 内容也不可读，不能据摘要猜根因。本机 SDK `8.0.423` 与目标 CI SDK `10.0.401` 分开记录。

下一可执行小批量：R06-04 复制单元格与整行，先复用现有 `DataGridClipboardBehavior`、配置与已脱敏字段策略，再补行/单元格事件及负例证据。
