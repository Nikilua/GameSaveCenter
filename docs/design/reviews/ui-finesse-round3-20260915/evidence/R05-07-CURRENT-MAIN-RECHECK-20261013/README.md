# R05-07 当前 main Tooltip 行为复核

日期：2026-10-13（Asia/Shanghai）。实现提交为 `afade3cd39eab31de97f204aa76e9dc590ba3083`，父提交 `2662cdbb3ab635e19b364d6797db1d7506fef615`；构建输入绑定本轮列出的六个源码 SHA，未声称 clean checkout。Playnite 目标仍为 `net462`，测试目标为 `net472`。

## 现有实现与本轮修正

已有 `GscToolTipBehavior` 负责壳层/设置页 Esc 关闭；已有生产 Tooltip 样式负责 `Focusable=False`、`Placement=Mouse`、`MaxWidth=420`、长说明换行。检查发现 WPF 的 `ToolTipService.InitialShowDelay` / `BetweenShowDelay` 是 owner 上的附加属性，设置 UserControl 根节点并不会让任意子控件自动取得该值；且 `BetweenShowDelay=100` 允许刚显示过的 tooltip 在快速移到下一 owner 时立即再开。

本轮在共享 `GscToolTipBehavior` 中增加 inheritable timing scope：将根节点 350ms 初始延迟和 0ms 连续显示延迟应用到默认值的后代 owner；显式 Style/Local 值保持原样。壳层、设置页和 Dashboard 根值统一为 `350/0`。未新增服务、DTO 或命令。

实际 STA WPF 行为测试用合成按钮、真实系统指针移动和隔离 Window 验证：

1. 连续快速划过四个 owner 后停留 420ms，未打开 tooltip；在一个 owner 停留 420ms，恰好打开一个。
2. 首个 tooltip 已打开后快速移动至另外两个 owner，再离开 420ms，`Opened` 总数仍为 1，所有 tooltip 均关闭，覆盖“上一泡显示后快速划过”的负例。
3. Window 加载后动态添加的 owner 取得 `350/0`；显式设置 `900/250` 的 owner 保持该值。
4. 长合成路径的实际 Tooltip 文本布局为 Wrap、无 trimming，文本高度超过一行且宽度不超过 420 DIP；`Esc` 关闭 tooltip 并保持原键盘焦点。
5. 指针位置在夹具开始时读取，并在 `finally` 恢复；没有访问真实存档或媒体路径。

## 本机验证与产物身份

- 本机隔离 SDK `8.0.423`（`D:\GameSaveCenterTools\dotnet-sdk-8.0.423`）。官方 `scripts/build.ps1 -Configuration Release -SkipTests`：XAML `24/24`，Release solution `0 warning / 0 error`。本次工具目录中的 Release 插件程序集为 `net462`，测试程序集为 `net472`。
- 定向行为/回归 `34 passed / 0 failed / 0 skipped`：Tooltip `2/2`、Popup lifecycle `1/1`、Popup boundary `2/2`、focus boundary `5/5`、Round2 source contracts `24/24`。这不是完整测试套件通过声明；一次较早的全量 WPF 尝试在第 `19/114` 类时停止，不计为通过。
- `python scripts/validate-source.py` 通过；WPF 静态审查 `36 XAML / 0 errors / 29 warnings / 177 info`；`git diff --check` 通过。
- 插件 DLL SHA-256：`CAB1A9F5D8FA3BE0E9E0B25BA6C6EA66E6E8720C527D7892A806A0FFA4CFD278`；测试 DLL SHA-256：`36FB96003B2AEE5DAA7E29DE6EC39C162F769EFA58F1E4B685C6B076B707F1F5`。
- 本轮源码 SHA-256：`GscToolTipBehavior.cs` `71704B589F71274493BEFDB639DA80FA04AFCE660C7F62AEF20C1D1153A52699`；`AcrylicProductionShellView.xaml` `B9E7A777B84F4AC3DD08FEF69643D7F64D7632466F3DC0271239F8792B756089`；`GameSaveCenterSettingsView.xaml` `A095AFFCA0AA9B9A154D8147E7DCC7462DC8FBA10C83B72E13BA77C67E56078F`；`DashboardView.xaml` `278DE939D67F6AF247E7DD0623DA65B8FF8DF1E28E716C510B2B3D4369C64DE6`；`R05TooltipTimingBehaviorTests.cs` `D233248B04BE314B8BBB0FA3DBD736DF030D63C91092E4997D184A9C547AC608`；`UiFinesseRound2ControlSourceTests.cs` `F77BF80A4D94C91E0F64754F74BA87B652785AE4AED128E84D7AAD9F09D56F44`。
- 脱敏 TRX：[`Tooltip 2/2`](tests/R05-07-tooltip-timing.trx) SHA-256 `B7CB4EA4FEA1CD8F6B26F985B6C1755FC21D3E7C7208B1C414D77B9347150B9E`；[`Popup lifecycle 1/1`](tests/R05-07-popup-lifecycle.trx) `6BF5975AAFB41BD0FD2B5B65B34A89BB64EDB08BBED4108C6048EC7F3707DB28`；[`Popup boundary 2/2`](tests/R05-07-popup-boundary.trx) `86607E7E1D0A59BA66D4EFDD9E62BAA2F5EF59216FEC0E82B0F1F0EE120DA457`；[`focus boundary 5/5`](tests/R05-07-focus-boundary.trx) `AB1C37F05CF57BADD5648D012246076008FB62B6EEF21531A59FA0CE63A84D99`；[`source contracts 24/24`](tests/R05-07-source-contracts.trx) `5C30CEDBE65E33762F4579475EA1C1887667E0C1709C8C9367CB507E0B462275`。
- Actions run #946（`38294361061`，提交 `afade3cd`）终态 failure，4m18s；公开 annotation 标记“编译与测试”exit1及Node.js弃用warning。诊断 artifact 239 KB，SHA-256 `083b3f72bcacb1fdd36c413517d075fe5f27a921d1fb79024ad13882e3cc2cf7`；匿名完整日志和artifact API HTTP403，artifact内容不可读。失败命令、测试断言、实际CI SDK与根因未知，不凭exit code推断。

## 2026-10-13 Actions #951 Tooltip hover 到达复核

- Actions #951（run `38306094571`，commit `2f546d06`，job `115000392485`）已终态失败；CI SDK `8.0.423`、Windows `10.0.26100`。完整 job log SHA-256 `96BCAC239C2E6BB25DA54E725319FA9AB6EC6964C3662D2C4AFAAB0A266406DE`，128,002 bytes / 1,015 lines。失败方法为 `QuickMousePassesCancelTooltipAndAStationaryDwellOpensIt`：420ms 驻留后预期打开计数 `1`、实测 `0`；长路径 Wrap/Esc 用例通过。隔离序列从 Q14 `[24/114]` 继续到 R02 `[25/114]`，因此 Q14 类已完成。Artifact `11773198841` 页面摘要 SHA-256 `fd4cdd6972f4b5d80c606b14ffdb3d3c259d4dcc8f1e9c7c3b24ebb76b5fa222`；匿名 artifact 下载返回 HTTP 401，未声称读取其内容。
- CI 未记录指针到达或 WPF 命中状态，不能从 `Opened=0` 推断 Tooltip 生产计时根因。本机旧非置顶夹具的一次诊断复跑显示系统光标已到目标屏幕坐标，但 `IsMouseOver=False`、`Mouse.DirectlyOver=null`（DPI `1.5x1.5`）；这表明夹具缺少 hover 到达前置确认，不证明 CI 发生了同一情况。
- 仅修正测试夹具：短暂 Window 置顶并调用 `Activate()`；移动系统指针后最多等待 2 秒，直到 WPF `IsMouseOver` 与系统坐标同时匹配；超时输出命中窗口、前台窗口、窗口边界、目标句柄及 DPI。快速扫过取消与驻留开启断言保留，生产 Tooltip 行为/延时未改。当前修正版在本机 SDK `8.0.423`、Windows `10.0.22000` 连续两次 `2/2`；Release solution `0 warning / 0 error`、XAML `24/24`，Q14 `1/1`，`validate-source.py` 与 `git diff --check` 通过。新 CI 仍待推送后复核。
- 当前测试源码 SHA-256 `F527025A86BD105A7D8E960FF42DEB33C5FD8CF4E2A111D7BC2102CC35FA5B53`；Playnite DLL SHA/MVID `A602B8F77EA4B983C28BB9CA4840D87DD94DEE2ACEEF7581F58A232CB99140A8` / `6bba8de6-5002-4886-ba04-9650d01eca0b`；tests DLL SHA/MVID `2AAEB335A9ED08C1B8BDD5142121DCCAECDA239E651BD06809A82E5E379B794B` / `0cd8cbed-538c-4f4d-bffd-7f5533f9647e`。
- 本轮脱敏 TRX：[首次 Tooltip 复跑 `2/2`](tests/R05-07-tooltip-ci951-local-recheck.trx)，SHA-256 `76174A2255E8B76AA61E6E77B08B72BA02DD06A60B3D439A9061E81C81DC2A9B`；[重复 Tooltip 复跑 `2/2`](tests/R05-07-tooltip-ci951-repeat.trx)，SHA-256 `EC97F5284ACE187105457E6ED923A361A6C4F579788896DDB4BF4949E3A1F369`。

## 2026-10-13 Actions #952 终态与日志限制

- Actions #952（run `38309632982`，commit `4572118f3c671981c3d6f93a66820910bc3b94b2`，job `115011915461`）已失败；公开 job 页面显示用时 `12m 7s`，失败步骤为“编译与测试”，annotation 仅为 `Process completed with exit code 1`。页面另有 Node.js 20→24 弃用 warning。此摘要未说明实际失败命令、测试方法、断言或堆栈。
- 诊断 artifact `11774606363`（556 KB），页面记录 SHA-256 `9686cc633488c860c9dd8021690986abfff69129f1420ced7b433ff062490a3a`。本次无法读取内容：job 页日志入口要求登录；匿名 artifact API 请求被 GitHub 限流为 HTTP 403；`/actions/runs/38309632982/artifacts/11774606363` 跳转后的匿名路由返回 HTTP 404。没有声称下载或检查 artifact。
- 仓库 `global.json` 配置 SDK `8.0.423`，但因 run 日志不可读，本次 runner 实际 SDK 未确认；本机 `8.0.423` Tooltip 两轮通过不能替代 CI。失败根因保持未知，不据摘要或耗时改代码。待取得有权限的完整日志后再按真实失败复现；在此期间可继续不依赖该失败的 R00/R01 复核。

## 未验证边界

行为夹具加载生产 Tooltip 样式和行为代码，但没有启动 Playnite 宿主或完整应用 Dashboard 页面；没有验证 OS UI Automation/Narrator、真实物理 DPI/多屏边缘放置、操作系统字体渲染或已呈现帧、ETW 和宿主性能。未运行全量测试。系统指针仅在短时受控夹具期间移动并已恢复。#952 的 runner SDK 尚未由日志确认；global.json 配置为 `8.0.423`，用户指定的 `10.0.401` 历史结果与本次运行分开记录。本机结果不替代远端 CI 门禁。

下一项：R05-07/R06-04 的 CI 失败原因尚待完整日志，保持未收口；依用户执行顺序先复核 R00/R01 freshness 命中项，再继续依赖满足的 Q/R 小批量。
