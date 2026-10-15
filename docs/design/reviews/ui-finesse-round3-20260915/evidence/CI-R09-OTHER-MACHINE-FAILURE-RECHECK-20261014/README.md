# R09 外机焦点轮廓失败复核

更新：2026-10-14；先前复核提交 `965fe1b2f8c8a6c840f4864d1ca47873e64d843c`；最新复核提交 `8822e32c0966794e715aaa943e19b802cf6f75ca`。

## 外机失败

用户提供的测试输出显示 SelectedTabAndInvalidTextBoxKeepFocusRingOutsideStateChromeAndRecoverAfterError 失败，Expected 2,2,2,2、Actual 1,1,1,1，堆栈报告源文件第 151 行。日志同时有 libpng iCCP 警告与 WPF TextServicesContext 收尾时的 COM 异常。

## 当前实现与断言

生产模板 WpfUiProduction.xaml 的输入 Chrome 始终为 1 DIP；独立 ValidationStroke 为 2 DIP、不可命中，只在 Validation.HasError 时显示。错误清除后该描边隐藏，错误态不改变内容测量。

历史提交 3f7d0b30 的第 151 行断言曾要求 errorChrome.BorderThickness 为 2，和外机的期望/实际值吻合。修正提交 f13b9e05 后，当前测试分别验证 Chrome 为 1 DIP、可见 ValidationStroke 为 2 DIP。当前 main 第 151 行检查错误色，第 154 行检查 ValidationStroke 厚度。基于行号和值，外机执行的断言/符号与当前文件不一致；旧测试产物或源码/PDB 错配最符合现有证据，但外机提交、源文件和 DLL/PDB 身份缺失，具体根因仍未确认。

libpng 警告不在失败调用栈内；TextServicesContext 的 COM 异常在断言失败后的 WPF 收尾中出现，不能解释厚度断言。

## 当前 main 复核

在当前提交 965fe1b2 上，用隔离 SDK 9.0.302 与显式 GSC_BUILD_COMMIT/GSC_SOURCE_ROOT 构建 Playnite 测试项目及其引用项目，定向执行 R09FocusOutlineBehaviorTests：2/2 通过，退出码 0；Playnite 目标 net462，测试目标 net472，VSTest 17.14.1。此轮是测试项目和引用项目构建，不是完整解决方案构建。代码与测试相对 552b8168 没有变化；552b8168 的完整 SDK9 Release 解决方案构建此前为 0 warning / 0 error，同一 R09 类为 2/2。

本次 test DLL 的 ProductVersion 为 0.6.73+965fe1b2f8c8a6c840f4864d1ca47873e64d843c；测试源 SHA-256 为 A5AAB9E90C340F93301D8424FF1E30D98B160D39C1265172AEE7A6B01B2CBC98。程序集 SHA、MVID、PDB SHA、XAML SHA 和脱敏 TRX 见本目录 tests 下的 assemblies-current-main-965fe1b2.json、hashes-current-main-965fe1b2.txt 和 R09FocusOutlineBehaviorTests-sdk9302-main-965fe1b2.trx。

## 8822e32c 当前 main 复核

用户再次提供相同断言特征的外机日志后，在最新 main `8822e32c0966794e715aaa943e19b802cf6f75ca` 上重跑 `R09FocusOutlineBehaviorTests`：本机 SDK `9.0.302`、VSTest `17.14.1`，Release 测试项目与引用项目构建，`2/2` 通过、0 失败、0 跳过。该次不是完整 solution build；NuGet 漏洞源不可达产生 `NU1900` 提示。脱敏 TRX 为 [`R09FocusOutlineBehaviorTests-sdk9302-main-8822e32c.trx`](tests/R09FocusOutlineBehaviorTests-sdk9302-main-8822e32c.trx)。

当前测试源码 SHA-256 仍为 `A5AAB9E90C340F93301D8424FF1E30D98B160D39C1265172AEE7A6B01B2CBC98`，与 `965fe1b2` 证据一致；`965fe1b2..8822e32c` 间 R09 测试源与 `WpfUiProduction.xaml` 均无差异。当前测试 DLL ProductVersion `0.6.73+8822e32c0966794e715aaa943e19b802cf6f75ca`、SHA-256 `C8984DE19675B01D848E6F101EF667CB6666BF5B7A7E57AAFEF3BF105BB8CDE5`、MVID `815d1e78-aad4-42f4-bc1c-22adb7b48556`；插件 DLL SHA-256 `325C765A15C2E1D014045C78DA17AE2130C980CD7AA6C5975CD31C4452D2941F`、MVID `1d756dc0-4380-4241-8bdb-c4a4c97f975c`。测试 PDB SHA-256 `78A6D9E91390C7887D51ED45EA8D203D3E51F9CBC25E9F25DA3454FDDDB15F39`。

当前源码第 151 行是错误态边框颜色断言，第 152 行才检查 Chrome 的 `1 DIP`；独立 `ValidationStroke` 的 `2 DIP` 在第 154 行。外机报告的期望/实际值仍与历史 `3f7d0b30` 中“Chrome 应为 2 DIP”的旧断言吻合。过期测试源/测试 DLL/PDB 不一致是现有线索下的合理推断，但外机 HEAD、实际源文件、DLL 和 PDB 身份均未提供，根因仍未确认。没有更改生产模板或弱化断言。

此前用户提供的 SDK10.0.401 诊断包在提交 983fee5 下、SDK8.0.423 诊断包在提交 c9f9179 下，R09 类也各有 2/2 通过记录；这些旧提交上的运行不替代当前 Actions 结果。相关 ZIP 不代表当前外机失败所用的测试程序集身份。

## a2fdb4f 当前 main 复核

2026-10-14 再次在当前 main 的父提交 `a2fdb4f647552d31def33aeee641917a049a77d2` 构建 Playnite 测试项目与引用项目，并单独串行运行 R09 类：本机 SDK `9.0.302`、VSTest `17.14.1`、Release、`2/2` 通过，0 失败/跳过。当前 `R09FocusOutlineBehaviorTests.cs` SHA-256 仍为 `A5AAB9E90C340F93301D8424FF1E30D98B160D39C1265172AEE7A6B01B2CBC98`；`WpfUiProduction.xaml` SHA-256 为 `7CFCBEE2720F3C8960FA9B54D8B86562C87D17D0E44F679FCF846F05ACDCEB2B`。测试 DLL SHA-256 `2990F7BD310999879237D65D1BAC687394C72E2E8F6C5B05EAC503E02F6D5E94`、MVID `abbb878a-622d-4920-96a1-5f1bf66fb609`；插件 DLL SHA-256 `8DACAD7EBD415A9A7DD02D40DD2BE80FA9F2F548193032194DC3CC20C94C85C2`、MVID `239c14b9-4b7b-40b9-bcb2-2e070d4e91c2`。脱敏 TRX 见[当前 R10-06/R09 复核包](../R10-06-CURRENT-MAIN-RECHECK-20261014/tests/R09FocusOutlineBehaviorTests-current-main.trx)。

该运行再次证明仓库当前测试断言通过，不证明外机使用了同一源、DLL、PDB 或分支；外机身份缺失，旧断言/二进制错配的解释仍未证实。本机 SDK9 结果不替代 SDK8/CI SDK10。

## 结论与下一步

没有改生产样式，也没有放宽断言；当前 main SDK9 复核未复现失败。外机所用 HEAD、工作树状态、R09 源码第 145–160 行及测试 DLL/PDB 身份仍待用户可回复后核实。libpng 警告不在失败调用栈内；TextServicesContext 的 COM 异常发生于断言失败后的 WPF 收尾，不能解释边框厚度值。SDK9 本机结果不替代 SDK8.0.423 或 CI SDK10.0.401；真实 Playnite、OS 输入、物理 DPI、读屏和最终呈现未在本次验证。当前主线本地已推进 R10-06，下一可执行项为 R10-07。

## e0a72c9 当前 main 再复核

用户于 2026-10-14 再次提供同一特征的外机失败输出：`SelectedTabAndInvalidTextBoxKeepFocusRingOutsideStateChromeAndRecoverAfterError`，期望 `2,2,2,2`、实际 `1,1,1,1`、栈指向第 151 行。当前 main 对应行仍是错误边框颜色；主 Chrome `1 DIP` 在第 152 行，独立 ValidationStroke `2 DIP` 在第 154 行。本机 Release 重建后串行执行 `R09FocusOutlineBehaviorTests` 为 `2/2`，0 失败、0 跳过；SDK `9.0.302`、VSTest `17.14.1`，测试目标 `net472`、插件目标 `net462`。

当前测试源 SHA-256 仍为 `A5AAB9E90C340F93301D8424FF1E30D98B160D39C1265172AEE7A6B01B2CBC98`，生产 `WpfUiProduction.xaml` SHA-256 仍为 `7CFCBEE2720F3C8960FA9B54D8B86562C87D17D0E44F679FCF846F05ACDCEB2B`。构建元数据 ProductVersion `0.6.73+e0a72c93774610110100d7ead0ab59108689d1eb`；测试 DLL SHA `39DD3668B86A3782AA6C76DF01305D45108F39FDAE5D6AEAC685123034DFAF4B`、MVID `eeee7482-78a5-4357-93b5-2ad1f85b1057`、PDB SHA `296F1E8F2932DA3AA4F9A972C8D5DD9A7BBEC0114010D55275B2CDD6AE561A80`；插件 DLL SHA `D42A07426F001B5FE20F6168B00E6C51DCD1AF5838AC158CDB91B92E15FE61A4`、MVID `40cdb240-f202-47c8-a438-60820e0e78c3`、PDB SHA `9BD887A95DA0EA83336092D05F698DB877815CD71F4CA86AC0B35AA412197C65`。脱敏 TRX 见[当前 R10-08 复核包](../R10-08-CURRENT-MAIN-RECHECK-20261014/tests/R09FocusOutlineBehaviorTests-current-main.trx)。

外机当前 HEAD、源文件内容、工作树状态和测试 DLL/PDB 身份仍未提供，因此“旧测试源码/程序集或 PDB 错配”仍是与历史断言吻合的推断，未确认。`libpng iCCP` 不在断言调用栈；`TextServicesContext` COM 异常出现在断言失败后的 WPF 收尾，不能解释厚度值。没有更改生产样式或弱化断言。用户到 2026-10-15 09:00 前无法回复，外机身份信息继续作为待验事实，不阻塞独立任务。

## 3a2d1163 当前 main 再复核

2026-10-14 在当前 `main` `3a2d116353945dc41e026ca993c59ec5276bd2d3` 下，按 Contracts、Core、HostAuditSeeder、Playnite、Tests 顺序重新 Release 编译；本机 SDK `9.0.302`、VSTest `17.14.1`，插件目标 `net462`、测试目标 `net472`，每个项目均 `0 warning / 0 error`。`R09FocusOutlineBehaviorTests` 独立串行 `2/2`，0 失败/跳过。脱敏 TRX：[`R09FocusOutlineBehaviorTests-sdk9302-main-3a2d1163.trx`](tests/R09FocusOutlineBehaviorTests-sdk9302-main-3a2d1163.trx)，SHA-256 `97A9E5416C389B319C928974FAC0F3D86ECB40CC8BF7C17D68FE48DED6262848`。

当前测试 DLL ProductVersion `0.6.73+3a2d116353945dc41e026ca993c59ec5276bd2d3`、SHA-256 `51E2FBF104D2C32B473E85989BBE897EF41A3A7D67B575E2D5CFD1277A176881`，测试 PDB SHA-256 `4F079B19B153591A6EC95963EDD7721E1FB89F601EC76913292537E45EC57749`；插件 DLL SHA-256 `11D0D875657355094B69DA24C7DFA11279434255A82DC62F066A514E6D565B79`，PDB SHA-256 `9BF4849E61CBEB31A87204D2CBFAB464543CDF2EA93FF0EA22B0C7C8601D4623`。测试源 SHA-256 `A5AAB9E90C340F93301D8424FF1E30D98B160D39C1265172AEE7A6B01B2CBC98`，`WpfUiProduction.xaml` SHA-256 `7CFCBEE2720F3C8960FA9B54D8B86562C87D17D0E44F679FCF846F05ACDCEB2B`。

这次只证明当前 main 本机重建和运行未复现外机断言失败。当前第 151 行是错误色断言；外机的 `Expected 2 / Actual 1` 与旧提交 `3f7d0b30` 的 Chrome 两 DIP 断言一致，旧源或二进制/PDB 错配仍只是推断。外机实际 HEAD、源码和 DLL/PDB 身份尚缺；CI SDK10 结果也不由本机 SDK9 替代。

## fd853380 当前 main 再复核

2026-10-14 在 `fd853380ec888ef94c112e255996b53106a0ddea` 当前 main 源码上，以本机 SDK `9.0.302`、VSTest `17.14.1` Release 构建 Playnite `net462` 和测试 `net472` 后串行执行本类：`2/2` 通过、0 失败、0 跳过。此构建使用已验证的依赖输出，是定向项目构建，不是完整 solution build。脱敏 TRX：[`R09FocusOutlineBehaviorTests-sdk9302-main-fd853380.trx`](tests/R09FocusOutlineBehaviorTests-sdk9302-main-fd853380.trx)，SHA-256 `E7A8B85AED7188AC8EEE985A04F863F5615B335DC039C2D89FAB766FFA2329C0`。

测试源 SHA-256 仍为 `A5AAB9E90C340F93301D8424FF1E30D98B160D39C1265172AEE7A6B01B2CBC98`；生产 `WpfUiProduction.xaml` SHA-256 为 `7CFCBEE2720F3C8960FA9B54D8B86562C87D17D0E44F679FCF846F05ACDCEB2B`。测试程序集 ProductVersion `0.6.73+fd853380ec888ef94c112e255996b53106a0ddea`、SHA-256 `EFB50B83FCC5FA0E999209B6ED0C9AABD4ED5F2231E5E7652B1466E2CEA179B0`；MVID及 PDB 身份见[当前 R11-03 复核包](../R11-03-CURRENT-MAIN-RECHECK-20261014/README.md)。插件 DLL SHA-256 `D71DD6CDC3B96A9A3B9E89BABC5ACBED66F17F73CB5BB02A9FCFB4E9145357C2`。

用户最新外机日志仍是 Expected `2,2,2,2` / Actual `1,1,1,1`，stack line151。当前 main 的同名测试第151行断言错误边框颜色，第152行才要求 Chrome `1 DIP`，第154行要求独立 `ValidationStroke 2 DIP`。数值与历史 Chrome `2 DIP` 断言相同，但外机 HEAD、实际源、DLL 和 PDB 未提供；旧源码/程序集或符号错配仅为待验证假设。`libpng iCCP` 不在失败调用栈，TextServicesContext COM exception 出现在断言失败后的 teardown，均不能解释边框厚度不符。未改模板或弱化断言。本机 SDK9结果不替代SDK8/CI SDK10.0.401。

## 2026-10-14 新日志与 SDK8 当前 main 对照

用户又提供另一台机器的一键构建/安装失败片段：同一测试名，Expected `2,2,2,2`、Actual `1,1,1,1`，堆栈源文件第151行；随后输出 `libpng iCCP` warning 和 `TextServicesContext.StopTransitoryExtension` 的 `InvalidComObjectException`。逐一对照源历史后，`3f7d0b30` 的第151行正是 `Assert.Equal(new Thickness(2), errorChrome.BorderThickness)`；`f13b9e05` 已将错误态厚度拆为 Chrome `1 DIP` 与 ValidationStroke `2 DIP`。当前 main 第151行是错误色断言，第152/154行分别断言 Chrome `1 DIP` 与 ValidationStroke `2 DIP`。

在当前 main 基线 `40b0a980` 上用本机 SDK `8.0.423` / VSTest `17.11.1` 重新 Release 构建 Playnite `net462` 和 Tests `net472`，R09 类 `2/2`，exit 0；插件 DLL ProductVersion `0.6.73+40b0a980`，测试 DLL SHA-256 `544BC3C5C124538B79D6C1178C84B2C9B41470259EE10DA42B316B68573D336B`、MVID `28b50be7-adfd-400c-bb7d-ebd709f2741c`，PDB SHA-256 `CFB5CC4EC4B3EF53FF1768AD3BC11BD59E8A371D12C63E01E5E054EB3EAD32D2`。脱敏 TRX 为 [`R09FocusOutlineBehaviorTests-sdk8423-main-40b0a980.trx`](tests/R09FocusOutlineBehaviorTests-sdk8423-main-40b0a980.trx)，SHA-256 `D886C0967EBE969BAC7F384AF1C8FB1EDECB2312636280374AA425BE33499D53`。

这次 SDK8 当前 main 对照仍不能确认外机实际 checkout、工作树、测试 DLL 或 PDB。日志数值/行号与旧断言吻合，可能来自旧测试源/程序集或符号映射，但不是已证实的根因；在取得外机身份前不改生产模板或放宽断言。`libpng iCCP` 不在断言调用栈；COM 错误出现在失败后的 WPF 收尾，不能解释边框厚度断言。SDK8 本机结果不替代 CI SDK10.0.401。

## 2026-10-15 新一键构建失败与最新 main 对照

用户再次从另一台机器提供相同失败：Expected `2,2,2,2`、Actual `1,1,1,1`，栈指向第 151 行。当前 main `884e826e` 的第 151 行仍是错误边框颜色断言；Chrome `1 DIP` 在第 152 行，独立 `ValidationStroke 2 DIP` 在第 154 行。测试源 SHA-256 `A5AAB9E90C340F93301D8424FF1E30D98B160D39C1265172AEE7A6B01B2CBC98`，与此前复核一致；当前提交 SDK8.0.423 / VSTest 17.11.1 从同一 Release 测试程序集执行该类 `2/2` 通过。脱敏 TRX：[`R09FocusOutlineBehaviorTests-sdk8423-main-884e826e.trx`](tests/R09FocusOutlineBehaviorTests-sdk8423-main-884e826e.trx)，SHA-256 `78AEA96E3D734F69B742B84ECE0726633A7ED298D2CA07D19FD2B29A9107BCDE`。测试 DLL SHA-256 `922B3F64028BC2B2086B87190DC6D105E887A143AC1A22DC9590E8077D335511`、MVID `7aa94665-4e61-4124-a6ca-2e88cfbea5e2`、PDB SHA-256 `5F461BCB7524F481C62562CF8446FB4D9D2AA6605D3B92D583C39887238FB7FB`；插件 DLL SHA-256 `93F1DB4077141F7FBCC079BC4AA4335713F229D36D9E3F643153950704C481F5`、MVID `dafdeff7-e58d-4237-8830-80d976456775`。

同时检查了当前仓库一键脚本：`dev-install-run.ps1` 默认每次生成新的 `artifacts/gsc-b/<token>` 输出根目录；`build.ps1` 先把解决方案编译到该根目录，再将同一个根目录交给 `run-playnite-tests-isolated.ps1`，后者用 `--no-build` 从该次输出执行。因此，若外机实际运行的是这套当前脚本，单纯复用上一次随机输出目录不能解释失败。旧 checkout/旧脚本或实际源码、DLL、PDB 不一致仍是可能方向，但外机身份未提供，根因没有确认。

若下次仍失败，优先附加该次 `artifacts/build-diagnostics/<run>/` 目录；最低限度需要 `summary.json` 与 `tests/wpf.R09FocusOutlineBehaviorTests/` 下的 console/TRX，以比较源码哈希、ProductVersion、DLL/PDB 和实际断言。当前没有更改生产模板，也没有放宽断言。`libpng iCCP` 不在厚度断言的调用栈；COM 异常仍只出现在失败后的 WPF 收尾。

用户随后说明这次外机已从 Git 拉取最新版本，并再次贴出同一失败片段。该信息保留为用户报告；片段仍显示旧测试断言对应的 `2` 对 `1` 和第 151 行，但未包含机器实际 `HEAD`、工作树状态、诊断目录、测试源 SHA、DLL/PDB identity，因此不能据此断定 Git checkout 确实对应本轮最新 main，也不能确认根因。当前仍无依据改动生产模板或放宽断言；问题不阻塞独立 Q/R 任务。

## 2026-10-15 最新 main `88b8eb57` 同输出 SDK8 复核

本机从当前 main `88b8eb57f13750b4f0a43c1f94a7dc1d59ae6acc` 执行完整 Release solution build（SDK `8.0.423`、VSTest `17.11.1`、Playnite `net462`、测试 `net472`）为 `0 warning / 0 error`。使用该次完整解决方案构建的同一隔离输出单独运行 `R09FocusOutlineBehaviorTests`：`2/2` 通过。脱敏 TRX 位于 [当前 main R00/R01 复核包](../R00-R01-CURRENT-MAIN-RECHECK-20261015/tests/R09FocusOutlineBehaviorTests.trx)。该测试程序集 SHA-256 `BAF74B8A177F498BED63F3F3F47E1BDFB7E85DDFD48D0811590CCF8E7ACB17CB`、MVID `54831945-8a8b-42f8-b602-07abd8258a9c`、PDB SHA-256 `22EC1B1BD010FB4445230ECFA70308CCFA0B87E44A1DB6C97097E988296D5A47`；插件 DLL SHA-256 `CB6D7DA5613A36065BD8541AABF2BD68318A4B9D312666A0B73DF049273F8D97`、MVID `24135592-de2e-4198-8ac6-930f120226ea`。

当前 main 第 151 行是错误色断言，第 154 行才是 `ValidationStroke` 2 DIP 断言。该同输出复跑进一步说明本机不能复现，但仍不能识别外机那次实际测试源、DLL/PDB 或样式资源身份；行号和值的差异仍只是 source/symbol/build-output 不一致线索，没有确认根因。未改生产模板或放宽测试。外机如再次出现，需要同一失败运行的 `summary.json`、R09 console/TRX、HEAD/status 和实际 DLL/PDB 哈希；用户无法回复期间继续其他独立工作。

## 2026-10-15 用户附加 #970/#971 diagnostics 对照

用户附加了 SDK10 与常规 build diagnostics ZIP，并说明来自 #970/#971。按 ZIP 内 `summary.json`、TRX 和 console 原文复核：

- `GameSaveCenter-build-diagnostics (4).zip` 包含 source commit `c9f9179d9b8d38e506dbb1402ae7030eafebea6c`、SDK `8.0.423`、干净工作树的 Release solution build 记录；两个完整 solution 运行中的 `R09FocusOutlineBehaviorTests` 均为 `2/2`。两者记录的 `R09FocusOutlineBehaviorTests.cs` SHA-256 都是 `A5AAB9E90C340F93301D8424FF1E30D98B160D39C1265172AEE7A6B01B2CBC98`，与当前 main 相同。该测试源码第 151 行仍是错误边框颜色，第 152 行是 Chrome `1 DIP`，第 154 行是独立 ValidationStroke `2 DIP`。ZIP SHA-256：`598C2CA619F0CAC37E3E6877B5CBC0EE45AEFC3371DAAACA0F122AE0BC64A8B6`。
- `GameSaveCenter-sdk10-build-diagnostics (1).zip` 的 source commit 同为 `c9f9179`，SDK `10.0.401`，失败发生在 Worker 集成测试：357 项中 356 通过；`TaskCoordinatorFailureTests.CancellationIsIdempotentAndPublishesSafeFinalizationBeforeCancelled` 在第 128 行期望 `1`、实际 `0`。包内没有 R09 WPF 测试运行记录，因此不能用它解释外机 R09 失败。ZIP SHA-256：`155612D3A083821531E04D07F57CCAE2CF403647741996395B335FD98AE43EAE`。
- `GameSaveCenter-build-diagnostics (3).zip` 的 source commit 为 `5a6647cbf8b40b30bc11bf8c6c32015545896967`；SDK8.0.423 WPF 运行失败于 `Q14ToolbarAlignmentBehaviorTests`，Light 980×640 的 preset text centers 为 `6,18,18,18,18,18`。该运行也不是外机所报的 R09 测试。ZIP SHA-256：`6AC19F269A3B2FDF3618BF9153C313245AE742EADB6E389A5DACDAEE486ADA4C`。

这些附件建立了“相同 R09 源码曾在 SDK8 CI 通过”的证据，也显示 SDK10 包内的失败属于 Worker 测试；它们没有外机本次失败对应的 HEAD、工作树、测试 DLL 或 PDB，因而不确认具体故障原因。用户片段的 `libpng iCCP` warning 与失败断言调用栈无关；WPF COM 异常是在失败后的 teardown 中报告。没有更改生产模板或弱化断言。独立 UI 任务继续进行，待用户可回复后再按需核对外机身份。

## 2026-10-15 当前 main SDK8 对照（`2b423737`）

针对用户新贴的外机输出，再以仓库锁定 SDK `8.0.423` / VSTest `17.11.1` 隔离 Release 重建 Playnite `net462` 与测试 `net472` 后，单独串行运行 `R09FocusOutlineBehaviorTests`：`2/2` 通过、0 失败、0 跳过。脱敏 TRX：[R09FocusOutlineBehaviorTests-sdk8423-main-2b423737.trx](tests/R09FocusOutlineBehaviorTests-sdk8423-main-2b423737.trx)，SHA-256 `5DBE53D9A91CD3503137E611A75343670B174429A0BADA7A21410C6E82DD77E7`。这只是定向项目构建，不是完整 solution build。

本次测试 DLL SHA-256 `A5D5C72BF7103505E548720EDD4C65A6DB04945A41033D276AC291D172FF000E`、PDB SHA-256 `9074BFECE11AFC107AEC6BD8AB86D91C338C7E52257B7040766084742CCF02C5`；插件 DLL SHA-256 `B3EF985AD75168D1BD0379EC831C5485E8EF90424465196274C5D82BF8224D51`、PDB SHA-256 `BE17BC36AF81DBA10826623F9A5A274316FC7D836515C581AFB14285DFD8E81B`。测试源 SHA-256 `A5AAB9E90C340F93301D8424FF1E30D98B160D39C1265172AEE7A6B01B2CBC98`；生产 `WpfUiProduction.xaml` SHA-256 `FD212D59A0244DF8CABF33A3926FB5CE80CF930E4A0B587692D2DC9D3B15EC6E`。当前源码第 151 行检查错误态边框颜色，第 152 行检查 Chrome `1 DIP`，第 154 行检查独立 ValidationStroke `2 DIP`。

本机当前 main 对照仍不能识别外机实际 checkout、工作树、DLL 或 PDB。外机报告的 `Expected 2 / Actual 1` 与旧厚度断言形态相符，但错配原因尚未证实；没有依据修改生产模板或削弱断言。`libpng iCCP` warning 不在失败调用栈中，TextServices COM 清理异常发生在断言失败之后，二者都不能解释厚度比较结果。SDK8 本机复核不替代 SDK10.0.401 CI。

## 2026-10-15 当前 main 对照（`95109c22`）与 #967 附件

用户最新外机日志仍报告相同测试、`Expected 2,2,2,2 / Actual 1,1,1,1` 和第 151 行。当前 main 第 151 行是错误边框颜色，第 152 行检查 Chrome `1 DIP`，第 154 行检查独立 `ValidationStroke 2 DIP`；历史 `3f7d0b30` 的第 151 行曾断言 Chrome 厚度应为 `2 DIP`。这支持旧测试源/测试 DLL/PDB 与插件资源不一致的推断，但外机实际 HEAD、源文件、DLL 和 PDB 仍未知，根因未确认。

重新用 SDK `8.0.423` / VSTest `17.11.1` 对当前 main 工作树构建 Playnite `net462` 与测试 `net472`，Release 定向 build `0 warning / 0 error`；R09 类单独执行 `2/2` 通过、0 失败、0 跳过。脱敏 TRX：[R09FocusOutlineBehaviorTests-sdk8423-main-95109c22.trx](tests/R09FocusOutlineBehaviorTests-sdk8423-main-95109c22.trx)，SHA-256 `AB238844DBB77270BE90149315ADAE165530996D7A7E543400F8B9FA5B45F434`。测试源 SHA-256 `A5AAB9E90C340F93301D8424FF1E30D98B160D39C1265172AEE7A6B01B2CBC98`；测试 DLL SHA-256 `82F254D315FC321149660F564A291EE105D476657E34F73D93AADFE08B388FF4`、MVID `87452ef3-7a89-44c1-81a4-afabda485a79`、PDB SHA-256 `1AD27C066FDDFD5A8988057D34F7656AA6E4CA1787825F01BF172957C716D7C9`；插件 DLL SHA-256 `9393C5258058D684B330048351172C7EC0202E5755721B62231BF3F9EC2F75BB`、MVID `d05bfaa3-0633-47a7-98c6-fdc07c8302c1`、PDB SHA-256 `36528A42E8A7425D01397BD0811C4A273DB2A6DB997222B3706295D9203C06C8`。该构建的 ProductVersion 元数据是 `0.6.73+95109c221ec85a52c61c51c0a119a2f3d97efacf`；源码 SHA 和二进制哈希共同区分含本阶段工作树改动的构建。

用户附加的 `GameSaveCenter-sdk10-build-diagnostics.zip` 对应 #967/source commit `983fee5cae51f0f9fbe4b100620ab156e3a723f1`、SDK `10.0.401`，包内 R09 测试也是 `2/2` 通过。它证明该历史 SDK10 运行没有此失败，但不能确认本次外机使用了同一提交或程序集。`libpng iCCP` 不在失败断言调用栈；TextServicesContext COM 异常发生在断言失败后的 WPF 收尾，也不能解释厚度比较。

若外机后续能复现，请从同一失败运行提供 `artifacts/build-diagnostics/<run>/summary.json` 与 `tests/wpf.R09FocusOutlineBehaviorTests/` 下的完整 console/TRX，并记录 `git rev-parse HEAD`、`git status --short`、测试源码第 145–160 行及实际加载的测试 DLL/PDB 和插件 DLL SHA-256。先保留现有文件，不通过更改断言或模板来掩盖程序集身份差异。

## 2026-10-15 当前 main SDK9 隔离对照

当前 main 基线 6cf279238d55fe3d139fa2cb0eb7b09229a7478a 的隔离 Release 全解方案 build 为 0 warning / 0 error；R09FocusOutlineBehaviorTests 在同一输出目录中 2/2 通过。脱敏结果、程序集 hashes 与 MVID 见 [本轮 R09 对照](../R09-FOCUS-CURRENT-MAIN-RECHECK-20261015/README.md)。本机只有 SDK9.0.302/VSTest17.14.1，仓库固定 SDK8.0.423 不可用，结果不替代 SDK8/SDK10.0.401。

该结果只能证明当前同源隔离构建通过，无法识别用户另一台机器的实际 HEAD、测试源、PDB、测试程序集或资源程序集。当前源码行 151 检查错误色，line 152 检查 Chrome 1 DIP，line 154 检查 ValidationStroke 2 DIP；旧断言/符号映射错配仍未证实。没有生产 UI 改动。

## 2026-10-15 当前 main `db073461` SDK8 复跑

用户随后贴出另一台机器的构建日志：`SelectedTabAndInvalidTextBoxKeepFocusRingOutsideStateChromeAndRecoverAfterError` 在第 151 行期望 `2,2,2,2`、实际 `1,1,1,1`。当前 `db073461` 测试源第 151 行检查错误边框颜色、第 152 行断言布局 Chrome 为 `1 DIP`、第 154 行断言独立 `ValidationStroke` 覆盖层为 `2 DIP`。这是测试源/行映射差异的证据，但没有外机 HEAD、status、测试源、DLL/PDB 和被加载插件资源身份，不能确认具体错配原因。

使用仓库锁定 SDK `8.0.423`、MSBuild `17.11.48`、VSTest `17.11.1`，从现有 `project.assets.json`（`--no-restore`，本地依赖缓存）隔离 Release 重建 Playnite `net462`、测试 `net472` 及引用项目，构建为 `0 warning / 0 error`；同一新建输出串行运行 R09 类为 `2/2`，0 失败、0 跳过。此为定向项目构建/行为复跑，不是完整 solution build、安装包或真实 Playnite 验证。测试源 SHA-256 `A5AAB9E90C340F93301D8424FF1E30D98B160D39C1265172AEE7A6B01B2CBC98`；生产 `WpfUiProduction.xaml` SHA-256 `FD212D59A0244DF8CABF33A3926FB5CE80CF930E4A0B587692D2DC9D3B15EC6E`。

本次测试 DLL SHA-256 `2EBD1981D34C61C2BA64921E31240432E5BDDBCA8AE5FB8232A51CA10530FCA4`、MVID `0856eb2c-128e-4170-9219-3ddd69568d56`；插件 DLL SHA-256 `690B730D9C98458A4E76BA70143C1F2519290EA1D13CC2DD5C3EF46143CE8546`、MVID `62ec4b19-30c0-4d37-8260-d69aa99b1379`；两者 ProductVersion 均标记 `0.6.73+db073461994062d6b1849948f105f97f3ff15f53`。PDB、TRX 与 console/build log 哈希见[身份清单](tests/hashes-current-main-db073461-sdk8423.txt)；脱敏结果分别见 [TRX](tests/R09FocusOutlineBehaviorTests-sdk8423-main-db073461.trx)、[console log](tests/R09FocusOutlineBehaviorTests-sdk8423-main-db073461-console.txt) 和 [build log](tests/R09FocusOutlineBehaviorTests-sdk8423-main-db073461-build.txt)。

外机日志里的 `libpng iCCP` warning 不在断言调用栈中；`TextServicesContext` 的 COM 异常出现在断言失败之后的 WPF teardown，现有材料不能把它们解释为厚度比较失败原因。没有改生产模板或放宽断言。确认外机结果仍需失败同次运行的源码与 DLL/PDB/资源身份。

## 2026-10-15 当前推送提交 `6b946540` 的同输出复跑

在刚推送的 `main` 提交 `6b946540e634eacde4c64e50a5b187e8e085e9bc` 上，用固定 SDK `8.0.423` 重建完整 Release solution（MSBuild `17.11.48`、XAML `24/24`、`0 warning / 0 error`），然后从该隔离输出单独运行 `R09FocusOutlineBehaviorTests`：`2/2` 通过。该提交只更新文档；生产源基线仍为 `88b8eb57`。脱敏 TRX：[当前 6b946540 R09 测试](tests/R09FocusOutlineBehaviorTests-sdk8423-main-6b946540.trx)，SHA-256 `AC026DE2B6BE77A5B9F944526765B33D7AA0B96F2857FE5D71F912C1D26F60F6`。源、插件/测试 DLL、测试 PDB 的 SHA-256 与 DLL MVID 见[6b946540 身份清单](tests/hashes-current-main-6b946540.txt)。

这再次确认当前 main 的同一构建输出不能复现外机报告，不会推断外机根因。SDK8 本机结果不替代 GitHub Actions 的 SDK10.0.401 lane；`6b946540` 对应 Actions run ID/终态暂不可核实（公开页加载错误且显示不同 SHA 的缓存项，匿名 API 返回 HTTP `403` rate limit）。

清理检查（2026-10-15）：源文件、程序集 SHA/MVID、脱敏 TRX 与 build/test logs 已保存在本证据目录。`.tmp/r09-current-main-retry-20261015` 的构建缓存暂未删除：检查时发现来源不明的活动 `dotnet` 进程，无法确认缓存未被占用；DLL/PDB 不作为持久证据。
