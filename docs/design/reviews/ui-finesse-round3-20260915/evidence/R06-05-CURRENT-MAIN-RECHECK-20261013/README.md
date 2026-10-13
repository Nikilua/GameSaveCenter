# R06-05 列头说明：当前 main 复核

日期：2026-10-13。源码基线：`d60e472de8c1f52c9eedc348f0274562c06d7e53`（main）。生产 UI/视图模型/命令/DTO 未改；本轮收口的是此前未提交的 `R06ColumnHeaderHelpTests.cs` 行为覆盖，并补当前生产页面的离屏截图证据。

## 现有能力与本轮验证

- 复用 `DataGridColumnHeaderHelpBehavior`、`GscDataGridHeaderTextTemplate` 和生产排序路径。列头说明仍由 `Tooltip` 与 `AutomationProperties.HelpText` 提供；没有在 header 中加入按钮或改路由。
- 本机 Windows `10.0.22000`，隔离 .NET SDK `8.0.423` / MSBuild `17.11.48`；Release solution `0 warning / 0 error`，XAML `24/24`。Playnite 插件 `net462`，测试 `net472`。
- `R06ColumnHeaderHelpTests 2/2`，0 失败/跳过。实际生成生产样式的 `DataGridColumnHeader`，验证 Tooltip/HelpText、字符串 Header、排序和重排能力、没有嵌套 `ButtonBase`、左右 resize `Thumb` 存在；长表头测得 `Wrap`、`TextTrimming.None` 且高度超过一行；通过实际 `DataGridColumnHeader.OnClick` 两次，检查升/降序及 `ListCollectionView` 行顺序。XAML 源契约只作补充，不单独代替交互测试。
- 容量语义沿用现有 `B / KiB / MiB / GiB`、1024 进制格式化。对当前 Save、Task、Media、Maintenance 生产表格核对列键和宽度；首屏列均有任务相关信息，没有发现无值占位列。短列按内容定宽，长文本/详情列使用星号空间或详情面板。

## 当前视觉复核

- 当前源码身份的 `scripts/render-qa.ps1 -Configuration Release` 通过，Light/Dark 共 372 张 PNG，覆盖多尺寸、滚动、50/400/2000/4468 条合成数据及 resize。完整本地报告当时 `WorkingTreeClean=False`，当前 tracked source diff 为本项测试文件，且工作树另有未跟踪 R04/R05/R07 证据；该字段如实保留。报告源码 commit `d60e472...`，SHA-256 `C76F2CE67A5A6034588349AA1E00A77ECD851D735189349C690EC06F298F2728`。
- 当前证据包只保留人工检查过的 8 张 `1040×700` 代表图。Save、Task、Media、Maintenance 的列头在两主题中可见；Save 的“文件数”在窄列中换行。报告的代表表头契约：Save 7 列、Task 7 列、Media 5 列、Maintenance Findings 3 列、Audit Findings 4 列、Audit Log 3 列，均为 `resize=true`、`sort-arrow=visible`。Save/Task/Media/Audit 在 1040×700 至少 4 行可读；Maintenance Findings 为 5 行可读。
- 代表图： [Save 浅色](Save-1040x700-light.png)、[Save 深色](Save-1040x700-dark.png)、[Task 浅色](Task-1040x700-light.png)、[Task 深色](Task-1040x700-dark.png)、[Media 浅色](Media-1040x700-light.png)、[Media 深色](Media-1040x700-dark.png)、[Maintenance 浅色](Maintenance-1040x700-light.png)、[Maintenance 深色](Maintenance-1040x700-dark.png)。摘要报告与 SHA 见 [`render-qa-summary.txt`](render-qa-summary.txt)。

## 文件身份

- 测试源码 `R06ColumnHeaderHelpTests.cs` SHA-256：`9391298E79FF74132E2C3E4226F5C39F2AE96B610CD5BDD035C32DB3DD362882`。
- 插件 DLL SHA-256 / MVID：`79EB1D1BAE01E03ED6800530F2872B5180BCCBE9553A28A6AA14C385D984FEAE` / `5bcf9b95-26ca-4817-b4a6-bcb8df9672e1`。
- Tests DLL SHA-256 / MVID：`343E0B285E0FB6DFF717CBC8C22E9FAC0D5C71AB26AF2854B0F16588F133564F` / `c79bbbe7-4b2a-41d4-8cf9-fbac75ea7304`。
- 脱敏 TRX：[`R06ColumnHeaderHelpTests-current-main.trx`](tests/R06ColumnHeaderHelpTests-current-main.trx)，SHA-256 `C8F63922D0F3C919AD74525AF9149F626AE975D7D619C0BE3527716000BC5024`。

## CI 与边界

- R08 修正提交 `d60e472` 的 Actions [#960 / run `38327521241`](https://github.com/Nikilua/GameSaveCenter/actions/runs/38327521241) 已终态 Failure；摘要仅显示“编译、测试与打包”exit 1，诊断附件 `11781543273`、580 KB、SHA-256 `113893ceea28078586cad28dc98fff77bcb402d08457a979b862d4a227bdfc51`。公开作业页要求登录查看完整日志；匿名 GitHub API 返回 HTTP 403 rate limit，附件页面返回 HTTP 404。CI SDK、失败命令与根因未知，未据此改代码。
- R00/R01 证据校正提交 `a2f8883` 的 Actions [#954 / run `38313448857`](https://github.com/Nikilua/GameSaveCenter/actions/runs/38313448857) 已终态 Failure。完整日志已由前序复核归档在[R07-08证据包](../CI-R07-08-RESIZE-FAILURE-20261013/run-38313448857-complete-logs.zip)，SHA-256 `CAB266AD735E3E145B4A31C4AA50A641F2FC4DD4371EDA30AF29972A7AE82807`；日志确认 CI SDK `8.0.423`，失败是旧 R07 `IsVisible` 初始断言，且没有记录可定位的可见性/祖先/尺寸值。它不能推断 R00/R01 有问题，也不能把整轮 CI 写成通过。该 run 的 diagnostics artifact `11775967579`、557 KB、SHA-256 `2de2ab3f1140c692f62ffc1fadb7c2c57a85f4059627f556fd65aed38e5b11a9`；当前匿名附件页不可读。
- RenderHarness 使用合成数据、离屏逻辑 DIP（`DpiScale=1.00`），不是 Playnite 窗口、物理 DPI 或最终屏幕帧。受控 WPF 测试不证明真实鼠标悬停、键盘输入、OS UIA/读屏、拖拽列宽或 IME。没有访问真实存档、媒体或云端。

本机 R06-05 行为和视觉复核已满足当前可控验收；代码阶段仍需随本提交推送并跟踪对应 Actions 终态。后续继续 R06-06 行内进度稳定，先复用 `TaskStatusDto.ProgressPercent/ProgressValue/ProgressDisplay` 与现有任务刷新/选择/滚动路径。
