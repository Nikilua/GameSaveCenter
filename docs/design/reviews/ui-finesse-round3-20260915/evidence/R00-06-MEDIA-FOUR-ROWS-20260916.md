# R00-06 媒体四行门禁证据

日期：2026-09-16  
分支：`codex/ui-finesse-round2`  
实现提交：`7d57575`（媒体几何门禁）；`db5d483`（探针身份元数据）

## 对照基线

质量审查 F06 指出，生产表格原来的 `MinHeight=212 DIP` 不能证明“42 DIP 表头 + 4×52 DIP 完整行”，也没有计入边框和水平滚动条。R00-06 要求使用实际行的可见交集，同时覆盖不同密度、水平条、短窗回退和父级裁剪。

当前实现复用 `Infrastructure/MediaInboxGeometry.cs` 的公式：

- 表头、行高和水平条均优先取实际 WPF 测量值；水平条尚未实现时使用系统 DIP 回退。
- 默认密度且有水平条时，所需主表高度为 `42 + 4×52 + 12 = 262 DIP`，表格框还要加 `24 DIP` padding 和 `2 DIP` border，即 `288 DIP`。
- `UiLayoutAnalyzer` 按 `DataGridColumnHeadersPresenter`、实际 `DataGridRow` 和有效裁剪区域计算完整行数；没有把固定常量或 `ActualHeight` 单独当成可读性证明。
- 生产 `MediaCenterView` 在运行时设置可读高度 floor，保留已有游戏选框、滚动条系统、命令/绑定、取消/错误语义和 `net462` 目标。

## 自动与行为验证

当前报告绑定：完整提交号 `db5d483d8ac6460ac7c3a07fe64cec5c7fe417d3`，`WorkingTreeClean=True`，`DpiScale=1.00`（离屏逻辑 DIP）。

1. `dotnet build tests\GameSaveCenter.Playnite.Tests\GameSaveCenter.Playnite.Tests.csproj --no-restore -c Release -m:1 /p:UseSharedCompilation=false -v:minimal`：`0 warning / 0 error`。
2. Media 几何、审计源契约和 R00-06 控件源契约定向测试：`4/4`。
3. `dotnet build tests\GameSaveCenter.RenderHarness\GameSaveCenter.RenderHarness.csproj --no-restore -c Release -m:1 /p:UseSharedCompilation=false -v:minimal`：`0 warning / 0 error`。
4. `RenderHarness.exe mediageometryprobe .tmp\r00-06-geometry-final`：Light/Dark 各 5 场景均通过，最终输出 `mediageometryprobe OK`。
5. `RenderHarness.exe audit .tmp\r00-06-audit-final`：`161` 个运行时快照、`0` 个 Fidelity 警告、`0` 个失败路由、`0 HIGH / 0 MEDIUM`；现有嵌套滚动和页级滚动可达性只保留为 INFO。
6. `RenderHarness.exe shellqa .tmp\r00-06-shellqa-final`：exit 0，最终输出 `shell-qa OK`；生产壳层 Media 在 `1040×700`、`1100×720`、`1366×768 DIP` 均保留四行表格和页尾区域的滚动可达性。

几何探针的关键样本如下（`grid=visible/layout`，单位 DIP）：

| 主题/场景 | 实际表头 | 行高/完整行 | 水平条 | 所需主表/表格框 | 结果 |
| --- | ---: | ---: | ---: | ---: | --- |
| Light/Dark normal | 42/42 | 52 / 6/4 | 0 | 250/276 | readable |
| Light/Dark horizontal-scroll | 42/42 | 52 / 5/4 | 12 | 262/288 | readable；PRIMARY_SCROLL_ACCESS 为 INFO |
| Light/Dark alternate-density | 36/36 | 44 / 6/4 | 0 | 212/238 | readable |
| Light/Dark short-fallback | 42/42 | 52 / 1/4 | 0 | 250/276 | short-window-page-fallback；页级回退为 INFO |
| Light/Dark blocked-parent | 42/42 | 52 / 1/4 | 0 | 250/276 | `pageScroll=False`，PRIMARY_VIEWPORT_TOO_SHORT 和 PRIMARY_VIEWPORT_UNREACHABLE 为 HIGH |

其中 `alternate-density` 证明公式跟随实际密度变化，不能被误读为恢复旧的 `212 DIP` 固定 floor；`blocked-parent` 是父级不可达的明确失败负例。

## 证据边界

- 这是使用真实生产 WPF 视图、合成 20 条媒体数据和隔离输出目录的 STA/offscreen 行为证据；不写入真实存档、媒体、云端或用户配置。
- `DpiScale=1.00` 是受控离屏逻辑 DIP，不是物理 DPI 或真实显示器像素；没有取得 Playnite 嵌入 Dashboard、真实鼠标/滚轮、人工键盘输入、ETW 或 presented-frame 证据。
- 完整审计中的 INFO 不等于缺陷清零；短窗口能否在真实宿主中用物理滚轮顺利到达仍待宿主条件。未把代理性能、离屏截图或 `shellqa` 写成真实宿主帧率验收。

## 下一步

下一可执行小批次为 R00-07“审计排除项收窄”：先核对 `AnalyzeToolbars` 的现有语义/几何分类，再为正常长表单和同祖先超宽、不可达工具条分别建立正例与失败负例，避免按 `TrainerToolsSettingsScrollViewer` 整棵子树静默排除。
