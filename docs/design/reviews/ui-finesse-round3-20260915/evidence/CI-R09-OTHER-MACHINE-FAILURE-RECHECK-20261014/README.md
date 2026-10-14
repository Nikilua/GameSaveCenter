# R09FocusOutlineBehaviorTests 外机失败复核

日期：2026-10-14  
当前 main：`066b9a6c01057c044ddc0d7c63ff70ce3dd7a6b3`  
本机 SDK：`9.0.302`；Playnite `net462`，测试 `net472`  
测试源 SHA-256：`A5AAB9E90C340F93301D8424FF1E30D98B160D39C1265172AEE7A6B01B2CBC98`  
生产资源 SHA-256：`7CFCBEE2720F3C8960FA9B54D8B86562C87D17D0E44F679FCF846F05ACDCEB2B`

## 用户提供的失败

另一台机器的 `R09FocusOutlineBehaviorTests.SelectedTabAndInvalidTextBoxKeepFocusRingOutsideStateChromeAndRecoverAfterError` 失败，日志为 `Expected: 2,2,2,2; Actual: 1,1,1,1`。同次输出中的 `libpng warning: iCCP` 不在这条断言的调用栈中。用户提供片段没有附该机器的 commit、测试源/DLL/PDB 身份或 diagnostics 包。

## 当前 main 对照

当前生产 `GscWpfUiTextBoxTemplate` 有两层：`Chrome` 始终保留 `BorderThickness=1`；独立 `ValidationStroke` 设置 `BorderThickness=2`、`IsHitTestVisible=false`，仅在 `Validation.HasError` 时显示。验证错误边框不参与内容测量，错误恢复后该层应归零。

历史提交 `3f7d0b30` 中的测试确实断言 `errorChrome.BorderThickness == 2`。提交 `f13b9e05` 已按生产模板改为分别断言 Chrome 为 1、可见 ValidationStroke 为 2 且不可命中，并检查错误恢复后描边隐藏。用户日志中的 `2` 对 `1` 与这条旧 Chrome 断言相符；结合栈中行号与当前源文件布局不一致，当前证据倾向于该机器仍运行旧测试源码/程序集、旧分支或源码与 PDB 不匹配。由于没有那台机器的 HEAD 和二进制身份，不能区分具体是哪一种，也不将其写成已确认原因。

逐行核对后，用户日志里的第 151 行正好对应历史 `3f7d0b30` 测试的 `Assert.Equal(new Thickness(2), errorChrome.BorderThickness)`；而当前 main 第 151 行断言的是错误边框颜色，2 DIP 断言已移到第 154 行并读取 `validationStroke.BorderThickness`。因此失败日志运行的测试断言不是当前 main 的版本：可能是旧分支/旧 DLL，或旧 PDB 与新源码错配；没有外机 HEAD/DLL/PDB 身份，无法再区分。

当前 main 的 `R09FocusOutlineBehaviorTests` 使用隔离 Release 输出复跑 `2/2` 通过。构建基于 `066b9a6`，期间只有 R08-01 测试文件有工作树测试增量；本机 SDK9 结果不替代 SDK8/CI SDK10。脱敏 TRX 和当前 build DLL SHA/MVID 在同目录 `tests/`。

## 结论与下一步

当前 main 的生产层级和测试断言相互一致，本机复核未复现外机失败；没有改生产 UI，也没有放宽测试。`f13b9e05` 的历史修正已解释最吻合的断言差异，但外机版本身份仍待确认。用户恢复可回复后，如仍需确认，请只补该机 `git rev-parse HEAD`、`git status --short`、`R09FocusOutlineBehaviorTests.cs` 第 145–160 行及测试 DLL/PDB 时间或 SHA；不需要再传整仓库。

该复核不涉及视觉变化，未生成截图；未运行真实 Playnite或物理输入。R08-01 小批仍按独立行为证据继续；这条 R09 外机记录单独保留，不阻塞其他依赖已满足的 Q/R 工作。

## 2026-10-14 当前 main 与用户诊断包复核

- 当前 main `552b8168f5ace1051cce9ebe7d39ee971c2d97e0` 的隔离 Release solution build 成功，SDK `9.0.302`、`0 warning / 0 error`；Playnite `net462`、测试 `net472`。R09FocusOutlineBehaviorTests `2/2` 通过。脱敏 TRX、当前程序集 SHA/MVID/ProductVersion 和源文件 SHA 见本目录 `tests/` 下 `R09FocusOutlineBehaviorTests-sdk9302-main-552b8168.trx`、`assemblies-current-main-552b8168.json`、`hashes-current-main-552b8168.txt`。
- 用户提供的 SDK10.0.401 诊断 ZIP 在提交 `983fee5` 下也记录了 R09FocusOutlineBehaviorTests `2/2`；`GameSaveCenter-build-diagnostics (4).zip` 的 SDK8.0.423 产物在 `c9f9179` 下有两次 R09 `2/2`。对应测试源码 SHA 均为 `A5AAB9E…B2CBC98`。SDK10 当前 ZIP 的整包构建失败发生在 Worker 用例，不是 R09；含提交 `5a6647c` 的另一 ZIP 没有 R09 TRX。以上历史运行不能代替当前 CI 终态，但没有复现用户贴出的 R09 断言。
- 用户日志中的第 151 行与历史 `3f7d0b30` 的 `errorChrome.BorderThickness == 2` 断言精确对应；当前 main 的第 151 行是错误色断言，第 154 行才检查独立 ValidationStroke 的 2 DIP。基于行号与期望/实际值，最符合旧测试源码/程序集或源码与 PDB 混用；因为缺外机 HEAD、DLL/PDB SHA，这仍是推断，未写成确定根因。当前 `dev-install-run.ps1` 默认每次生成新的隔离构建输出目录，但用户是否运行了该版本脚本、是否传入自定义输出路径仍未知。
- `libpng warning: iCCP` 不在断言调用栈。TextServicesContext/COM 异常出现在断言失败后的 WPF 收尾，不能解释边框厚度为何为 1。
- Actions 当前公开快照：#971/run `38393317198` 页面仍为 In progress，日志要求登录；#978/run `38509928015` 为 Failure，页面只有通用 exit code；列表只到 #979（提交 `ae3c565`），未显示当前 HEAD 的 run。SDK9 本机通过不替代 SDK8 或 CI SDK10.0.401。

下一步：用户恢复可回复后，只需补该机 `git rev-parse HEAD`、`git status --short`、测试 DLL 的 ProductVersion/SHA，以及 R09 源码 145–160 行；在此之前不改模板或降低断言。该诊断不阻塞独立 Q/R 任务。
