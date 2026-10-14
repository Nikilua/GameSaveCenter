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

## 结论与下一步

没有改生产样式，也没有放宽断言；当前 main SDK9 复核未复现失败。外机所用 HEAD、工作树状态、R09 源码第 145–160 行及测试 DLL/PDB 身份仍待用户可回复后核实。libpng 警告不在失败调用栈内；TextServicesContext 的 COM 异常发生于断言失败后的 WPF 收尾，不能解释边框厚度值。SDK9 本机结果不替代 SDK8.0.423 或 CI SDK10.0.401；真实 Playnite、OS 输入、物理 DPI、读屏和最终呈现未在本次验证。下一项 Round3 任务为 R10-04。
