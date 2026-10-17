# GameSaveCenter 当前事实入口

## 2026-10-18 最新检查点：#1079 双 SDK 成功与 R00/R01 freshness

`main` 代码提交 `de8bdbb176bb7ae85c4ca3f488f0aeb37958bfec` 的 Actions [#1079/run `39057273073`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39057273073) 已双 SDK 终态成功。SDK8.0.423 job `117467706465`、SDK10.0.401 job `117467706337` 均 Release build `0 warning / 0 error`、Core `125/125`、Worker `362/362`、Playnite WPF 隔离 `125/125`；两 lane 实际运行 `UiFinesseFoundationTests`（第 `121/125` 类）及 R08 system-motion 类并全部成功。SDK8 另 Render QA、30-XAML 静态审查及扩展包生成/上传成功。Artifact IDs 与 digest 见 [R08 CI 证据](../design/reviews/ui-finesse-round3-20260915/evidence/CI-R08-SYSTEM-MOTION-20261018/README.md)。

R00/R01 freshness 初扫只命中 R00-03、R01-04 的 `GscMotion.cs`。全量 WPF CI 已覆盖 `UiFinesseFoundationTests`，因此把这两项 sourceCommit/evidence 更新到 `de8bdbb`；更新后为 `14 fresh / 0 stale / 0 matched source paths`，package identity `not-provided`。这只反映登记源码范围，不代表 package-host 或真实 Playnite 验收。

本机仓库 `global.json` 固定 SDK `8.0.423`，但本机只安装 SDK `9.0.302`，所以本轮从仓库目录启动的 `dotnet test` 未进入测试；此前 SDK9 定向 R08 `1/1` 仅作补充，不能替代 #1079 的 SDK8/SDK10 runner 结果。CI 成功只说明精确提交在对应 GitHub runner 输入下通过，不证明用户外机安装成功。

Round2/3 `208/192` 的最新准入复核未发现新的依赖已满足代码缺口。外机 R09 `1/2` 仍是独立真实失败，缺同次 checkout、summary/TRX 与 test/plugin DLL/PDB 身份；不要求用户重跑，拿到材料先做身份比较。下一可执行准入条件是新复现、明确功能范围或相关宿主/设备/权限前置变化。真实 Playnite、OS UIA/IME/读屏、物理 DPI/跨屏/最终呈现、ETW/宿主性能仍未验。

## 2026-10-18 最新检查点：#1077 双 SDK 终态与 Q/R 准入复核

当前 `main` HEAD `e8edbfc073d6bf18e07980a60c778577f643c6be` 对应 Actions [#1077/run `39050532641`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39050532641) 已双 SDK 终态成功。SDK8.0.423 job `117448022062` 与 SDK10.0.401 job `117448022272` 均 Release build `0 warning / 0 error`、Core `125/125`、Worker `362/362`、Playnite WPF 隔离 `125/125`；SDK8 `render-qa OK`，静态审查扫描 `30 XAML / 0 errors / 27 warnings / 177 info`，扩展包生成并上传成功。Artifacts：package `12108658169`（90,314,190 bytes，SHA-256 `c290c2dbec6af9c044d5ec42ad1ae8bd21cb0d86f5094925ae82d66e9aff8937`）；SDK8 diagnostics `12108792518`（1,628,542 bytes，`39a2d164e7224eeac7d7f7932a64d5267bbda78c3cabf6a82cd21952a5442969`）；SDK10 diagnostics `12108435363`（775,322 bytes，`d9faa653ca34c1d8c1e00f2b430450f81384df9d665e0722d9dd5cfce31c63f5`）。

本次只更新文档，产品代码基线仍为 `8fc980f`。重读 Round2/3 与 R23-08 后没有新的依赖已满足代码缺口；R00-01 至 R01-08 的受控条件仍已满足。本机当前只有 `\\.\DISPLAY1`（2560×1440），没有 Playnite/Worker；CEF `0x5` 不重试，ETW 权限不绕过，Q24-03 不把逻辑模拟当物理跨屏。用户外机 R09 失败附件 SHA-256 `F3DDDC6445E0BB1A5E4CD44EEEEC2C2C98834FC8C81482A67E456C5DB5864514` 与既有文本完全相同，仍是 `20261016T052146719Z-d9d99ac4` 的 `1/2` 失败，不是新运行；#1077 runner 通过不覆盖它。下一准入条件是外机同次身份材料、新的可复现问题/明确范围或相关宿主/设备/权限前置变化。

## 2026-10-18 最新检查点：#1076 双 SDK 终态与 Q/R 准入

文档提交 `32a62d8413f49d7f02a5909321377c9cdc09da22` 对应 Actions [#1076/run `39047011333`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39047011333) 已整体成功。SDK8.0.423 job `117437640771` 与 SDK10.0.401 job `117437640925` 均 Release build `0 warning / 0 error`、Core `125/125`、Worker `362/362`、Playnite WPF 隔离 `125/125` 通过。SDK8 `render-qa OK`、静态审查 `30 XAML / 0 errors / 27 warnings / 177 info`，扩展包生成/上传成功。Artifacts：package `12107422070`（90,314,170 bytes，SHA-256 `18310d2c4a94b85f6840006cc3ac9da77e896161ec07be066f3c58b32b54e9e0`）；SDK8 diagnostics `12107750956`（1,626,582 bytes，`70db57dfdfd00cf41ac2a46cbc65985f41ad1ea3c989e9f9ebc28fce8377bd6b`）；SDK10 diagnostics `12106670885`（772,805 bytes，`01203d80bde6bbc05a367951b966106e05b51016fb3b751a725af25f0b6a2f88`）。

本轮只同步 CI/准入文档，代码基线仍为 `8fc980f`。Round2 `208/208` 的实现列均为代码完成/已复核、最终 `203` 项未完成主要受宿主证据门槛约束；Round3 `192/192` 无待开始/实施中代码项；R00-01 至 R01-08 均已满足受控条件。当前单屏且无 Playnite/Worker 进程，R23-04/05、Q24-03 前置仍不满足。外机 R09 `1/2` 仍缺同次 checkout、summary/TRX、test/plugin DLL/PDB 身份，#1076 不覆盖该失败。下一步仅在新复现/明确范围、同次身份材料或环境条件变化时重新准入。

## 2026-10-17 最新检查点：#1075 双 SDK 终态与准入复核

文档提交 `6e308527e5040b94da1f07199f93adddf8978e4a` 对应 Actions [#1075/run `39043054832`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39043054832) 已整体成功。SDK8.0.423 job `117426249110` 与 SDK10.0.401 job `117426248974` 均 Release build `0 warning / 0 error`、Core `125/125`、Worker `362/362`、Playnite WPF 隔离 `125/125` 通过。SDK8 另 `render-qa OK`、静态审查 `30 XAML / 0 errors / 27 warnings / 177 info`，扩展包已生成并上传。Artifacts：package `12106570037`（90,314,110 bytes，SHA-256 `e0068c287926768ee8ed13b5fb1d2eee01ac33581331d74068fa0a8195e63424`）；SDK8 diagnostics `12106450704`（1,628,394 bytes，`5c8f78a322556c9c7e1c5201fce243f0d1f44070c3692fd250b5e5dfe4371c53`）；SDK10 diagnostics `12104389589`（775,652 bytes，`f606db90d3323b3d9ed997b4cddab54681c4209ef50dc0cad08c55fdbd22b3f4`）。

当前 `main` 没有相对代码基线 `8fc980f` 的源码变化；本次仅为文档提交，R00/R01 freshness 不重算。重新核对账本，R00-01 至 R01-08 均为“已满足”；R00-08 的真实 Playnite/OS 输入边界仍待验。Round2/3 最新复扫没有新的依赖已满足代码缺口。本机只枚举到 `\\.\DISPLAY1` 一块活动屏，且未运行 Playnite/Worker；Q24-03、R23-04/05 前置条件未改变。用户外机 R09 `1/2` 仍未定位，当前 CI 不代表外机成功。详见 [R23-08 当前准入复核](../design/reviews/ui-finesse-round3-20260915/evidence/R23-08-CURRENT-MAIN-ADMISSION-20260926.md)。

## 2026-10-17 最新检查点：#1074 双 SDK CI 终态

文档提交 `68c178968e535774d7acb68c8c01a67319c20804` 对应 Actions [#1074/run `39039120707`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39039120707) 已终态成功。SDK8.0.423 job `117414822833` 与 SDK10.0.401 job `117414822607` 均 Release build `0 warning / 0 error`、Core `125/125`、Worker `362/362`、Playnite/WPF 隔离 `125/125` 通过。SDK8 另 `render-qa OK`、静态审查 `30 XAML / 0 errors / 27 warnings / 177 info`，并成功生成/上传扩展包与诊断。

Artifacts：扩展包 `12104192973`（90,314,298 bytes，SHA-256 `0900b6fe7ec432716e9fc6da9a23c1c194c8dec34069c0d3b5f49848bf5b5098`）；SDK8 diagnostics `12104078221`（1,627,488 bytes，SHA-256 `b7ad514628db695804530d5e49a2b02e95e91ccdd10128a5c57264aef79ec8d9`）；SDK10 diagnostics `12104125808`（774,476 bytes，SHA-256 `7d6274eac8e4e096d13a3b074a018af3cd05c0e5e9fb3cd4c5bb3034b42b5bc6`）。此提交只更新文档，R00/R01 freshness 仍绑定代码基线 `8fc980f`；CI 结果只证明该提交和对应 runner，不代表外机验证。

Round2 `208/208`、Round3 `192/192` 与 R23-08 复扫没有发现新的依赖已满足且有明确代码缺口的 Q/R 项。外机 R09 `1/2` 仍是未定位的真实失败，缺同次 checkout、summary/TRX、test/plugin DLL/PDB 身份；不改当前 UI 或弱化断言。下一步在有同次身份材料、新复现或环境前置变化时重新准入；物理输入、真实 Playnite/CEF、OS UIA/IME/读屏、物理跨屏/呈现帧和获准系统跟踪仍未验证。

## 2026-10-17 最新检查点：#1073 双 SDK 终态与 R23-08 准入复扫

当前 `main` HEAD `199b49381598aeffa362a4154dced74f202180e9` 对应 Actions [#1073/run `39027064950`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39027064950) 已终态成功。SDK8.0.423 job `117380045515` 和 SDK10.0.401 job `117380045495` 均成功；SDK8 完成源码结构检查、诊断自测、Release build/test、Render QA、WPF 静态审查、扩展包及 artifact 上传，SDK10 完成 Release build/test 与 diagnostics 上传。该提交相对代码基线 `8fc980f` 只含文档，不重新绑定 R00/R01 freshness。

再次检查 Round2 `208/208` 与 Round3 `192/192` canonical task rows，并复核 R23-08 准入：目前没有新的依赖满足且有明确代码缺口的 Q/R 产品任务。未收口项仍卡在真实 Playnite/CEF、OS 输入与读屏、物理跨屏/呈现帧或获准系统跟踪权限。用户外机 R09 `1/2` 是独立真实失败；日志中的 Thickness 断言/行号与 main 源及符号映射不一致，缺同次 summary、TRX、test/plugin DLL/PDB 身份，根因未确认。CI 结果不能代替该机器的验证。

下一准入条件为外机同次身份材料、新的可复现问题或相关环境前置变化；先比对现有构建诊断，不要求外机重跑。此前代码基线、详细测试边界和外机材料缺口见下方 #1072 检查点记录。

## 2026-10-17 当前 main：Q06-02、#1072 双 SDK CI 与外机 R09 对照

当前 `main` 的前序文档 HEAD 为 `6c8c85b0efae4d15996a2c7382255fcbaa8d2a2d`，代码基线为 `8fc980fce5262411fbb16f0299583c34074f7f61`；代码提交 `547efd4badaa462379532c2c0c96eea5c5a5c524` 只扩展 `Q06ButtonStateSequenceBehaviorTests`。本机 SDK `8.0.423` / VSTest `17.11.1` 的 Playnite `net462`/测试 `net472` 定向 Release 测试 `1/1` 通过：双主题 Space 按下后移焦、禁用、卸载均复位 scale/PressedOverlay，fake 命令执行 `0` 次。该本机结果不是完整 solution build。证据及测试程序集/PDB SHA 见[Q06-02 按下中断复核](../design/reviews/ui-finesse-round2-20260913/evidence/Q06-02-PRESSED-RESTORE-20261017.md)。

R00/R01 freshness 绑定代码 HEAD `8fc980f`：`14` 条记录、`0` 需重跑、`0` 匹配登记源码路径，`packageIdentity=not-provided`。#1070/run `39016699991`、文档 #1071/run `39020360750`、文档 #1072/run `39023752118` 均双 SDK 终态成功。#1072 的 SDK8.0.423 job `117370558805` 与 SDK10.0.401 job `117370558697` 均通过 Release build、Core `125/125`、Worker `362/362`、Playnite source `107` 类及 WPF 隔离 `125/125` 类。SDK8 另完成 Render QA `render-qa OK`、静态审查 `30 XAML / 0 errors / 27 warnings / 177 info`、0.6.73 扩展包生成及 artifact 上传。#1071/#1072 仅更新文档，freshness 仍绑定 `8fc980f`，没有声称重跑源码基线。Artifact 身份与准确边界见[当前 main 复核](../design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-CURRENT-HEAD-RECHECK-20261017-8FC980F.md)。

用户另一台机器日志报告 R09 `1/2`、expected thickness `2` / actual `1`、源码行 `151`。该组合与历史 `e12119bf` 旧 Chrome `2 DIP` 断言相符，不与当前源码/PDB映射一致：当前 Chrome 为 `1 DIP`，ValidationStroke 为 `2 DIP`；具体外机 checkout/DLL/PDB 输入仍未知，所以这是输入身份不一致的线索，不是已确认的根因。CI 成功只适用于对应提交与 runner，不覆盖外机失败。物理鼠标移出、异步命令取消和真实 Playnite 宿主仍待验；Round2 Q06-02 最终状态未完成。13 条 MEDIUM toolbar layout risk 继续保留，CI Render QA 未重跑该审计。R23-08 准入没有发现当前依赖已满足且存在明确实现缺口的 Q/R 项。下一可执行动作是用户方便后对照外机同次诊断身份；若仍不可用，按 R23-08 重做准入，等待新复现或环境前置改变，不要求外机重跑。

## 2026-10-17 最新检查点：#1068 双 SDK CI 与 R00/R01 freshness 收口

文档提交 `b14e780daa11a074d63abc381d777c66a8d258a8` 对应 Actions [#1068](https://github.com/Nikilua/GameSaveCenter/actions/runs/39010428185) 已终态成功。SDK `10.0.401` job `117331623679` 与 SDK `8.0.423` job `117331623876` 均 Release build `0 warning / 0 error`、Core `125/125`、Worker `362/362`、Playnite source `107` 类和 WPF 隔离 `125/125` 类通过（含 R09）。SDK8 另外完成 Render QA `0 PROBLEM`、静态审查 `30 XAML / 0 errors / 27 warnings / 177 info`、扩展包生成及上传。Artifacts：SDK10 diagnostics `12092898627`（776,003 bytes，SHA-256 `fc6e02c4e6ac58e173bebb405dde9d05647b248981aba8faa6c85e9e0b52f4c7`）；SDK8 diagnostics `12093722541`（1,629,137 bytes，SHA-256 `21df9106ad02ee918ade6a759a2774ee67058e5ad1b15b2531e6045312e06725`）；package `12093881767`（90,313,946 bytes，SHA-256 `c3e24dd9cac58580fcc57fcadde26b44c3a03397740277348f256aa19afa8cc3`）。

R00/R01 freshness 对当前 HEAD `b14e780d` 为 `14` 条记录、`0` 需重跑、`0` 个登记源码路径命中、`2,963` 个比较路径，package identity `not-provided`；详见 [freshness 复核](../design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-FRESHNESS-RECHECK-20261017.md)。这是登记源码路径核对，不证明包或宿主身份。Round2/Round3 的 `192` 条 canonical task rows 已复扫；当前未收口条目对应真实宿主、系统输入、外机身份或获准跟踪权限边界，没有新的独立 Q/R 产品代码缺口。

用户另一台机器的 R09 `1/2` 与整体 exit `1` 仍是独立真实失败；当前未取得同次 `summary.json`、TRX 与 test/plugin DLL/PDB 身份。本次 #1068 的 R09 通过只代表精确 main 输入在 GitHub runner 上通过，不覆盖外机失败。下一步在获得同次身份材料、新的可复现问题或环境前置变化时重新准入并选独立小批；真实 Playnite/package-host、物理 DPI/跨屏、OS UIA/IME、最终呈现帧及 ETW/宿主性能仍未验。

## 2026-10-17 最新检查点：#1067 双 SDK CI 收口

本次诊断保护代码提交为 `26de12257748075e024c61972da16e87fd4ddfce`。其 Actions #1067/run `39006921277` 已整体终态成功：[GitHub Actions](https://github.com/Nikilua/GameSaveCenter/actions/runs/39006921277)。SDK8.0.423 job `117321161387`、SDK10.0.401 job `117321161211` 均 Release build `0 warning / 0 error`、Core `125/125`、Worker `362/362`、Playnite source `107` 类及 WPF 隔离 `125` 类通过（含 R09）。SDK8 另 Render QA `0 PROBLEM`、WPF 静态审查 `30 XAML / 0 errors / 27 warnings / 177 info`、`0.6.73.pext` 生成和上传成功。Artifacts：SDK8 diagnostics `12092696132`、SDK10 diagnostics `12091697203`、package `12092870374`。

这条提交在 GitHub Windows Server 2025 runner 上通过，不能证明用户另一台机器的安装或构建通过。本轮重复粘贴的 R09 日志 SHA-256 `F3DDDC6445E0BB1A5E4CD44EEEEC2C2C98834FC8C81482A67E456C5DB5864514` 与此前相同；外机 `1/2`、总体 exit `1` 仍是真实失败。其 line151 `Thickness(2/1)` 与当前 main 同名源 line151 Brush 断言不对应，外机的 `summary.json`、测试源、test DLL/PDB、plugin DLL 身份仍未取得。未改 R09 生产样式或放宽断言。

R00/R01 freshness 对 `26de1225` 为 `14 fresh / 0 stale / 0 matched source paths`，2,963 个比较路径，package identity `not-provided`；仅为登记源路径 freshness。当前复扫 Round2/3 与 R23-08 没发现新的依赖已满足 Q/R 产品代码项。下一可执行动作是在新外机身份材料、新可复现问题或前置环境变化时重做准入并选小批；未验真实 Playnite/package-host、物理 DPI/跨屏、OS UIA/IME、最终呈现帧、ETW/宿主性能。

## 2026-10-17 R09 诊断元数据补强

Actions #1065/run `38999877162` 对提交 `350cdcb1eb672e952991666fa77add9a207445df` 已整体终态成功：SDK10.0.401 与 SDK8.0.423 Windows Server 2025 job 均通过；Core `125/125`、Worker `362/362`、Playnite WPF 隔离 `125` 类通过（含 R09），SDK8 Render QA、静态审查、扩展包与诊断上传也成功。这证明该提交在两个干净 runner 的输入通过，不代表用户外机问题已解决。

首版 PDB 身份记录提交 `8a895807e4232cfaf8d350cc419172e7b865a30b` 对应 Actions #1066/run `39003320618` 双 SDK 终态成功。后续提交 `26de1225` 为可选 PDB 哈希读取补不可读/锁定保护：采集失败记录 unavailable reason，不覆盖原构建/测试退出语义；本机合成自测覆盖可读、锁定、缺失。该后续修改对应的 #1067/run `39006921277` 现已双 SDK 终态成功，详细结果见本页最新检查点。该工具只补诊断能力，不是用户外机成功证明；R09 外机 `1/2` 仍缺同次 summary/TRX/DLL/PDB 身份。

## 2026-10-17 补充：#1064 终态与外机诊断 ZIP 范围

文档基线修正提交 5475ba7d9744ab107b159a89ef930d535f1f95cb 对应 Actions #1064/run 38996324038 已整体终态成功：[GitHub Actions](https://github.com/Nikilua/GameSaveCenter/actions/runs/38996324038)。SDK10.0.401 job 117289578704 与 SDK8.0.423 job 117289578557 均在 Windows Server 2025 干净检出中 Release build 0 warning / 0 error、Core 125/125、Worker 362/362、Playnite WPF 隔离 125 类通过（含 R09）；SDK8 另 Render QA 0 PROBLEM、静态审查 30 XAML / 0 errors / 27 warnings / 177 info、扩展包及诊断上传成功。Artifacts：SDK10 diagnostics 12088185573、SDK8 diagnostics 12088616683、package 12088477370。该远端结果比本机验证独立，但仍不是用户外机安装输入。

用户提供的 GameSaveCenter-build-diagnostics (4).zip 与 GameSaveCenter-sdk10-build-diagnostics (1).zip 是 2026-10-13 的另一批输入，sourceCommit 均为 c9f9179d9b8d38e506dbb1402ae7030eafebea6c；前者包含 SDK8 R09 2/2 通过，后者 SDK10 失败在 Worker 取消用例 356/357 且没有 R09 子运行。它们不包含 2026-10-14 外机目录 20261014T052440239Z-3fc9f5f1；该次 summary/TRX/DLL/PDB 仍未取得。当前一键流程每次用新隔离输出目录，故不先归因旧 bin；line 151 仍指向旧断言形态，具体构建身份待同次诊断。

更新：2026-10-17；版本 `0.6.73`；当前分支 `main`；最近代码提交 `6a909cb329d88af3da83a78446050a83d24ef246`。后续文档收口提交 `ee8d6aa8d32da2861167d9d49b28e1f912063a8b` 对应 #1063/run `38992929963` 也已双 SDK终态成功。Q11-07 只在 `R08PageSwitchBehaviorTests` 增加 opt-in Light/Dark 离屏往返截图与证据，没有生产 UI/XAML、服务或 DTO 改动。本机 SDK `9.0.302`/VSTest `17.14.1` Playnite.Tests 隔离 Release `4/4`。精确代码提交的 Actions #1062/run `38988908489` 已双 SDK终态成功：SDK10.0.401 job `117266648758` 与 SDK8.0.423 job `117266649011` 均 Release `0 warning / 0 error`、Core `125/125`、Worker `362/362`、WPF 隔离 `125` 类通过，包含 R09 与 Q11-07 行为类；SDK8 另 Render QA `0 PROBLEM`、静态审查 `30 XAML / 0 errors / 27 warnings / 177 info`、包生成与上传成功。#1063 的 SDK8.0.423 job `117279033559` 与 SDK10.0.401 job `117279033715` 验证文档提交也成功。SDK9 本机、CI runner 与另一台用户机器是不同验证输入；外机 R09 `1/2` 真实失败仍未定位，不能由本机或 CI 结果覆盖。

- 当前代码提交 `1c60a2fc` 对应 Actions #1059/run `38982569941`、文档/证据提交 `d62e199f` 对应 #1060/run `38983018226` 均终态成功。详细 SDK/job 与测试结果见 Q11-06 当前 main 视觉复核包。
- Q11-07 的自动行为类 `4/4`；Task/Media/Save/Maintenance 四个缓存生产页在往返状态下各输出 Light/Dark 离屏图，共8张。Task选中 ID 由断言验证，但不在截图视口。图片不是屏幕截图或Playnite呈现。R00/R01 freshness `14 fresh / 0 stale / 0 matched` 只校验登记源码范围。#1062 精确提交的双 SDK CI 已成功，证据见 Q11-07 包；宿主/有效物理 DPI/Final 仍待验。
- R00-03 历史 #919/#920 的 SDK10.0.401 完整日志现已取得：两次均为活动侧栏过渡断言的测试前提失败；`48b78b3b` 后由测试固定系统动效输入并补偏好矩阵，当前 #1056 双 SDK 已过。详情见 [R00-03 历史失败复核](../design/reviews/ui-finesse-round3-20260915/evidence/R00-03-SIDEBAR-MOTION-CI-FAILURE-RECHECK-20261017/README.md)。
- 外机 R09 附件是先前同一文本 SHA，仍是一次真实失败，不是当前 main 的新复跑。其 `Thickness(2/1)` 第151行与旧 `3f7d0b30` 断言形态相符；当前 main 的 R09 断言在 `f13b9e05` 已拆为 Chrome 1 DIP + 独立 ValidationStroke 2 DIP。#1062 SDK8/SDK10 当前代码 runner 均通过 R09 类，但外机 HEAD、源、test DLL/PDB、plugin DLL 身份仍缺；该诊断目录在工作区不存在，已请求用户方便时附加，主线不等待。

- #1056/run `38962119739` 对提交 `4a98148caf2b0909998824acc73119272fc0cb37` 双 SDK 终态成功：[GitHub Actions](https://github.com/Nikilua/GameSaveCenter/actions/runs/38962119739)。SDK8.0.423 job `117179503173` 与 SDK10.0.401 job `117179503250` 均 Release build `0 warning / 0 error`、Core `125/125`、Worker `362/362`、Playnite source `107` 类和 WPF 隔离 `125` 类全通过。SDK8 另 `render-qa OK`、静态审查 `30 XAML / 0 errors / 27 warnings / 177 info`、扩展包与诊断产物上传成功。#1055 的取消回调夹具失败在修正后当前提交通过；历史失败保留。该 runner 结果不代表另一台机器的安装/构建输入。
- 本机 SDK `8.0.423` / MSBuild `17.11.48` / VSTest `17.11.1` 官方 Release 脚本 build `0 warning / 0 error`，Core `125/125`、Worker `361 passed + 1 NotExecuted`；修正的取消测试 `50/50`、同输出 R09 定向测试 `2/2`。完整 WPF 隔离于 R05 Tooltip 第 `50/116` 类因 `GetCursorPos` 无法读取系统初始鼠标位置停止；这不是完整 WPF 门禁通过。证据：[Worker 取消回调复核](../design/reviews/ui-finesse-round3-20260915/evidence/CI-WORKER-CANCEL-CALLBACK-RECHECK-20261017/README.md)。
- 用户外机 R09 一键日志仍为真实失败：Core `125/125`、Worker `357/357`、R09 `1/2`、整体 exit `1`。本次重复附件 SHA-256 `F3DDDC6445E0BB1A5E4CD44EEEEC2C2C98834FC8C81482A67E456C5DB5864514`，没有新的机器身份。其 line 151 `Thickness(2/1)` 与当前同名源码 line 151 Brush 颜色断言不匹配，旧 Chrome `2 DIP` 断言在 `3f7d0b30`；旧 checkout/DLL/PDB/构建输入具体哪一项仍未定位。外机及本机隔离脚本都报告 116 个 WPF 类，但 #1055 同提交 SDK8 GitHub log 为 125；计数不能单独确认根因。此前将 `116 vs 125` 当作外机独有错配证据的表述以此为准修正。

- Q24-04 当前受控行为：本机 SDK `9.0.302` / VSTest `17.14.1`，Playnite `net462`/tests `net472` 定向 Release build 后，键盘相关四类 `21/21`；非完整 solution build。加载更多可见时命令绑定保留、Tab/Shift+Tab 可达；历史耗尽时折叠且正反向不可达。证据见[Q24-04 复核包](../design/reviews/ui-finesse-round3-20260915/evidence/Q24-04-LOAD-MORE-KEYBOARD-RECHECK-20261017/README.md)。
- R00/R01 对最新源码的 freshness 已重新绑定 R00-08 证据提交 `5c81a070`：`14/14 fresh / 0 stale / 0 matched source paths`，2,918 个比较路径，package identity `not-provided`。这只校验登记源码范围。
- 在报告提交 `01efb189` 上以仓库 SDK `8.0.423` 重新跑完整 RenderHarness：Release build 与运行成功，`render-qa OK`、`0 PROBLEM`。R23-08 的旧 Task/Settings 13 个布局问题未复现；窄 Settings 最小 body viewport 为 `181.33 DIP`，Task 1040×700 为 `4/4` 可读行及可见 Inspector。freshness 更新为 `14 fresh / 0 stale / 0 matched / 2,919 compared paths`。13 MEDIUM `TOOLBAR_VERTICAL_EXPANSION` 静态风险仍保留。详情：[R00/R01 Render QA 证据](../design/reviews/ui-finesse-round3-20260915/evidence/R00-R01-CURRENT-MAIN-RENDERQA-RECHECK-20261017/README.md)。离屏逻辑 DIP 不代表 Playnite/物理 DPI/最终呈现。
- 用户外机的一键日志确实失败：Core `125/125`、Worker `357/357`，R09 `1/2`，整体 exit `1`。line 151 报 `Thickness(2)`/`Thickness(1)`，而当前同名源码 line 151 是 Brush 颜色断言；历史 `3f7d0b30` 的 line 151 才是 Chrome `2 DIP`。当前 main R09 本机 SDK9 `2/2`、旧 Actions #1047 双 SDK通过均不能覆盖外机失败。现有证据确认测试源或符号映射与当前源不一致，具体旧 checkout/DLL/PDB/脚本输入尚未识别。需要外机同次 `summary.json`、TRX 与 DLL/PDB 身份；不阻塞其它工作。
- Actions #1051/run `38927198349` 的 SDK10.0.401 job `117061655415` 已失败于 Worker `361/362`：`CancellationIsIdempotentAndPublishesSafeFinalizationBeforeCancelled` 第128行 callback count 期望1、实际0。完整日志已读取；SDK8 job `117061656455` 已终态成功，build/test、Render QA、WPF 静态审查和扩展包步骤均完成。此 SDK10 失败与外机 R09 及 Q24-04 无关；本机 SDK9 的 Worker 复核不能替代 SDK10。
- Q24-04 提交 `5c81a070` 对应 #1052/run `38932328340` 已整体成功，见[GitHub Actions](https://github.com/Nikilua/GameSaveCenter/actions/runs/38932328340)。SDK8.0.423 lane：Release build `0 warning / 0 error`，Core `125/125`、Worker `362/362`、Playnite WPF 隔离 `125` 类通过（含 R21），Render QA、WPF 静态审查、扩展包与上传成功。SDK10.0.401 lane：Release build `0 warning / 0 error`，Core `125/125`、Worker `362/362`、WPF 隔离 `125` 类通过（含 R21）；#1051 的 Worker 取消回调失败在本次 lane 通过，未复现。历史 #1051 失败保留为真实失败，不称已修复。
- R00-07 最近 audit 仍为 `168 snapshots / 116 warnings / 0 HIGH / 13 MEDIUM`，未因本轮测试改动重跑；窄布局风险仍保留。R05-03 宿主预检仍因 WMI 拒绝访问而 fail closed；Q24-03 需第二物理显示器；R23-04 需 CEF/宿主条件变化；R23-05 的 ETW 系统跟踪权限被拒，不绕过。现有 R23-08 准入盘点没有其它可立即实施的 Q/R 代码项。
- #1053/run `38937972835`（文档提交 `01efb189`，代码基线 `5c81a070`）现已整体终态成功：[GitHub Actions](https://github.com/Nikilua/GameSaveCenter/actions/runs/38937972835)。SDK8.0.423 job `117098144434` 与 SDK10.0.401 job `117098144743` 均 Release build `0 warning / 0 error`、Core `125/125`、Worker `362/362`、Playnite WPF 隔离 `125` 类通过（含 R09/R21）。SDK8 另 `render-qa OK / 0 PROBLEM`、WPF 静态检查 `0 errors / 27 warnings / 177 info`、生成 `0.6.73` 扩展包并成功上传包和诊断 artifact；SDK10 完整日志确认实际 `dotnet --version` 为 `10.0.401`。这只验证该 CI 源码输入，不覆盖外机 R09 的真实失败。
- #1054/run `38944728502` 对当前文档 HEAD `79748216` 已整体终态成功：[GitHub Actions](https://github.com/Nikilua/GameSaveCenter/actions/runs/38944728502)。SDK8.0.423 job `117120854146` 与 SDK10.0.401 job `117120854512` 均 Release build `0 warning / 0 error`、Core `125/125`、Worker `362/362`、全部 `125` 个 Playnite WPF 隔离类通过，R09 在两 lane 均通过。SDK8 还完成 `render-qa OK`、WPF 静态审查 `30 XAML / 0 errors / 27 warnings / 177 info`、生成 `0.6.73` 扩展包并上传；SDK8 诊断 artifact `12068781572`、SDK10 诊断 artifact `12066891114`。这仍只验证该 CI 输入，不证明外机 R09 失败已解决。
- 当前阶段已收口：Worker 测试夹具修正及 #1056 双 SDK CI 均通过。R12-04 相关生产/测试文件 SHA 与 2026-10-16 证据一致，状态保持“已满足”。本轮重扫 Round2 208 项与 Round3 192 项后没有新增可独立实施的 Q/R 代码项；只读桌面快照为无可见 Playnite/Worker 进程、单活动屏。下一执行点是等待可用的外机 R09 同次诊断身份 artifact、环境前置变化或任务表出现具体证据缺口，再选相应小批。外机 R09 `1/2` 仍是真实失败；当前文本 SHA 与旧附件相同，summary/TRX/DLL/PDB/plugin DLL 身份仍缺。真实 Playnite、物理 DPI/跨屏、OS UIA/IME 与最终呈现仍待验。

- Q11-06 视觉证据受控通过；host/物理 DPI/最终屏幕呈现仍待验，Final 未完成。当前 main 的 R00/R01 freshness 复扫未发现 stale 项；下一执行点是在 Round2/3 ledger 中挑选另一个依赖已满足且有明确证据缺口的 Q/R 小批。外机 R09 的真实失败继续单独保留。
## 当前判断

核心功能和六个工作区已实现，当前属于功能较完整、尚未完成发布验收的开发预览版。不能用测试数量、Round3 的“已满足”或旧评估的 95%/98% 推导正式稳定版完成度。详见 [完成度评估](../FEATURE_COMPLETION_ASSESSMENT.md)。

| 维度 | 当前可核查事实 |
| --- | --- |
| 最近完整 Release/CI | #1056/run `38962119739` 对 `4a98148c` 双 lane 终态成功；SDK8.0.423 与 SDK10.0.401 Release build `0/0`、Core125、Worker362、Playnite WPF 隔离125类；SDK8 另完成 Render QA、静态审查、扩展包和上传 |
| 最近完整脚本测试 | #1056 两 SDK lane 的 Core/Worker/WPF 全套通过；SDK8 Render QA `0 PROBLEM`。本机 SDK `8.0.423` 取消测试复跑 `50/50` 是本机证据；两者都不证明另一台机器的失败输入 |
| 当前实现阶段 | Q24-04 键盘正/负行为补证已在 `5c81a070` 完成；R00/R01 当前 main Render QA 与 freshness 已复核，旧 Task/Settings 问题未复现。Worker 取消夹具修正已由 #1056 两 SDK CI 收口；R12-04 文件身份与既有证据一致。真实 Playnite、物理键盘、完整逐页顺序、OS UIA/Narrator 与页面视觉仍待验；外机 R09 `1/2` 失败未解决 |
| R02-03 当前复验 | 生产实现未变；加强后的 STA 提示焦点/UIA peer行为与Save紧凑布局定向 `5/5`。本机 SDK `8.0.423` Release `0 warning / 0 error`、XAML `24/24`；WPF 静态审查 `0 errors/29 warnings/177 info`；RenderHarness 通用双主题 `render-qa OK`，未作为禁用提示截图证明 |
| R02-04 当前复验 | 当前 main Release `0/0`、XAML `24/24`；生产资源 STA 几何 `7/7`，16/20 DIP 图标间距各 `8 DIP`、中心差 `0`；Light/Dark RenderHarness `372` 图 `render-qa OK`，已人工对照并归档 Save 1040×700 两主题截图。未改生产 UI |
| R02-05 当前复验 | 当前 main Release `0/0`、XAML `24/24`；生产资源 STA 命中/间距/82 DIP 换行 `2/2`；Light/Dark RenderHarness `372` 图 `render-qa OK`，已检查并归档 Media 1040×700 两主题截图。未改生产 UI |
| R03-01 当前复核 | main `8e71feb4`；RenderHarness Release `0 warning / 0 error`、`TypographyDiagnosticsTests 11/11`、双主题 `finesseprobe` exit 0；中文/英文/数字/𠮷/组合重音均有实际 GlyphRun。报告 `WorkingTreeClean=False` 是证据目录已在工作树的真实记录；自 `4d166f0` 以来 src/tests 无变化。截图/报告与 identity negative 见[R03-01 当前复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R03-01-CURRENT-MAIN-RECHECK-20261012/README.md) |
| R04-02 当前复核 | main 源码/测试身份 `32c0e1e2`；Playnite net472 定向行为 `5/5`，覆盖摘要链接跨 Tab 后字段可见、键盘焦点、HelpText/滚动，以及禁用巡检字段旧非法值和路径负例。结构检查仅作补充；没有执行真实 Playnite 设置保存/Worker 应用。详见[R04-02 当前复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R04-02-CURRENT-MAIN-RECHECK-20261012/README.md) |
| R04-03 当前复核 | `e952c414` 工作树上的插件 net462 / 测试 net472 定向构建，隔离 SDK `8.0.423`，`SettingsDraftLifecycleBehaviorTests + PortableSettingsTests + SettingsValidationSourceTests` 为 `12/12`；关闭事件实际选择“否”保留草稿并恢复焦点、“是”后才关闭/回滚。真实 Playnite 原生确认框、宿主保存和 Worker 应用仍待验。详见[R04-03 当前 main 补充证据](../design/reviews/ui-finesse-round3-20260915/evidence/R04-03-CURRENT-MAIN-RECHECK-20261012/README.md) |
| R05-01/02 当前 main | commit `1c39e909856faa736b32224ee196ef0bac6ca460`，隔离 SDK `8.0.423`，Playnite 插件 `net462`、测试 `net472`；R05Focus/R05Option/R21 定向合计 `11/11`。TRX SHA/MVID、插件 SHA/MVID 已归档。Dashboard 受控 STA fixture 覆盖焦点进入、Tab 环绕、即时/动画关闭和取消回焦；XML 解析断言核对真实 XAML Click 接线。完整 Dashboard XAML/Playnite 宿主未加载；VSTest 收尾有三条 TextServicesHost `InvalidComObjectException` 消息但 exit0，原因未知 |
| R05-03 当前 main | 同一 commit 和隔离 SDK 下 `R05PopupBoundaryBehaviorTests + R05OptionVirtualizationBehaviorTests 7/7`。短窗 Picker 边界、当前桌面 ComboBox Popup 翻转后的工作区几何/MaxHeight/滚动/选定项可见通过。隔离 Playnite bootstrap 于插件安装前因 graceful shutdown 未完成而被保护策略阻止，未取得 popup/呈现证据；“翻转无闪屏”、420×220 负例、真实 Playnite/多屏仍未验 |
| R05-04 当前工作树 | 基于 main `903525be`，测试源 SHA `B6D804AD...`；本机 SDK `8.0.423` Release solution `0/0`，`R05MultiSelectionSummaryBehaviorTests 3/3`。验证 0/1/2 摘要、隐藏 ID 例外、清空“已忽略”选择同时保留“待归类”选择并保持模式；未改生产视觉。身份/边界见当前复核包 |
| R05-05 当前 main | `fbf504d2a7aabcd322c3c735959a31c7e1679fc9`；媒体批量选择用 Extended DataGrid + `SelectedItems`；唯一显式三态样例在开发校对页。XAML `24/24`、Release `0/0`、源契约 `24/24`；源测试为字符串断言，不覆盖 Space/UIA/集合行为。维持不适用，见[复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R05-05-CURRENT-MAIN-RECHECK-20261013/README.md) |
| R05-06 当前 main | 生产基线 `0fcb44f8`，只补同值重复赋值负例；隔离 SDK `8.0.423`、官方 Release build `0/0`、XAML `24/24`，定向 `28/28`。当前行为、TRX 与源码/程序集身份见[复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R05-06-CURRENT-MAIN-RECHECK-20261013/README.md) |
| R05-07 当前 main | 生产 Tooltip 行为未改；测试夹具在 main 4572118f 置顶/激活临时窗口，并先确认 WPF hover 再计驻留。本机 SDK 8.0.423 Release 0/0、XAML 24/24；Tooltip 2/2 两轮。#952 于“编译与测试”失败，12m07s；job 日志要求登录，artifact 11774606363（556 KB，SHA-256 9686cc633488c860c9dd8021690986abfff69129f1420ced7b433ff062490a3a）内容不可读，实际 runner SDK/根因未知；不凭摘要盲改。详情见[复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R05-07-CURRENT-MAIN-RECHECK-20261013/README.md) |
| R06-01 当前 main | 同一 `afade3cd` 最新 main 重建 SDK `8.0.423` Release `0/0`、XAML `24/24`；`R06ColumnWidthPersistenceBehaviorTests 6/6`，含 Task 列键/生产列定义一致性；当前代码和程序集 SHA、TRX 见[复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R06-01-CURRENT-MAIN-RECHECK-20261013/README.md)。旧双主题截图不绑定当前 commit |
| R06-02 当前 main | `90454586` 隔离 SDK `8.0.423` Release `0/0`、XAML `24/24`；`R06SortingBehaviorTests 7/7`。实际 WPF 列头双向点击/箭头、乱序重加稳定次键、未知值和失效视图负例见[复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R06-02-CURRENT-MAIN-RECHECK-20261013/README.md)。历史 RenderHarness 图绑定 `c577afc5`，未当作当前截图 |
| R06-03 当前 main | 生产实现沿用 `d83c7378`；本轮以 `fdeeec8` 生产源加单项测试差异构建，SDK `8.0.423` Release `0/0`、XAML `24/24`；R06Selection `2/2`、四表双主题状态几何 `1/1`、Task 双主题受控 hover/错误徽章 InputHitTest/Failed 终态禁用与 Running 取消按钮 UIA Invoke `1/1`、共享资源契约 `2/2`。测试仅证明隔离 WPF 状态与逻辑 DIP，不是实际 OS 指针或 Playnite 呈现。详情见[复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R06-03-CURRENT-MAIN-RECHECK-20261013/README.md) |
| R06-04 当前 main | 复用 afe4aa55 已有 DataGridClipboardBehavior 和五类 profile；生产行为复核 5/5，复制反馈 2/2。本机 SDK 8.0.423 Release 0/0、XAML 24/24，Q14 1/1、侧栏 22/22、R06Clipboard 5/5、R22 2/2。#952 完整日志显示其失败发生在旧 R07 resize 初始断言；本轮修正尚无 CI 结果，R06-04 不因本机定向通过签收。详情见[当前复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R06-04-CURRENT-MAIN-RECHECK-20261013/README.md) |
| R06-05 当前 main | 复用现有共享列头说明、排序与单位格式；本机 SDK `8.0.423` Release `0/0`、XAML `24/24`、`R06ColumnHeaderHelpTests 2/2`。当前身份 RenderHarness `372 PNG`、`render-qa OK`；代表图和 header contract 见[复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R06-05-CURRENT-MAIN-RECHECK-20261013/README.md)。本轮未改生产 UI；受控 STA 与离屏逻辑 DIP 不等价真实 Playnite/OS 输入 |
| R06-06 当前 main | 复用 `TaskStatusDto`、`TaskIndexedCollection`、现有事件合批与 `TaskSummary`；SDK `8.0.423` Release `0/0`、XAML `24/24`，Playnite 隔离 `34/34`、Worker 合成 SQLite `9/9`；真实 DataGrid 验证 80 行更新只发生 `Replace`、选择/行数/滚动偏移保持，未知/0/钳制进度、取消状态与服务端分页总数各有行为证据。TRX 哈希/程序集身份见[R06-06 当前 main 复核包](../design/reviews/ui-finesse-round3-20260915/evidence/R06-06-CURRENT-MAIN-RECHECK-20261013/README.md)。本轮无当前身份截图，旧图仍绑定 `72fd6d9a`。混合非隔离试次 `22/23` 的单一失败在滚动 viewer 查找，已披露且未混入通过数。#961 是旧 run；当前代码检查点 #968 的 SDK8/SDK10 全门禁已成功；真实 Playnite/取消竞争、UIA、物理跨屏、presented frame、ETW 和宿主性能未验 |
| R07-08 / R08 当前 main | #959 CI 的 R07 resize `1/1` 通过，R08 fixture 的系统动效前提已在 `d60e472d` 修正；本机 SDK `8.0.423` Release `0/0`、XAML `24/24`、R07 `1/1`、R08 `2/2`、native motion policy `22/22`。#960/run `38327521241` 已确认 terminal failure、job `115069541561` “编译与测试” exit 1；完整日志 endpoint 404/API 403，SDK/根因未知。#968 当前源码自动门禁成功；真实宿主仍待验。历史日志/证据见[证据包](../design/reviews/ui-finesse-round3-20260915/evidence/CI-R07-08-RESIZE-FAILURE-20261013/README.md) |
| Q23-08 当前 main | 复用现有 ResetCatalog、导入预览/确认、Playnite Save/Cancel；SDK `8.0.423` 本机 build `0/0`、行为 `12/12`、XAML `24/24`、source validation 通过；Light/Dark × `1040×700`/`720×640 DIP` 的四场景动作可达性通过。视觉栏通过，宿主外部阻塞、最终未完成。证据见[复核包](../design/reviews/ui-finesse-round3-20260915/evidence/Q23-08-CURRENT-MAIN-RECHECK-20261013/README.md) |
| Q24-01 当前 main | `3ca41ec9` 新增受控分数 DPI 行为验证；SDK `8.0.423`，Release 定向测试 `1/1`。生产资源字体/图标/1 DIP 描边在 RenderTargetBitmap 的 96/120/144/168/192 DPI 目标栅格可见，负例覆盖裁切和永久缩放；受控离屏视觉通过，物理 DPI/Playnite Host 仍待验，最终未完成。详见[证据包](../design/reviews/ui-finesse-round3-20260915/evidence/Q24-01-CURRENT-MAIN-LOGICAL-DPI-RECHECK-20261013/README.md) |
| Q24-02 当前 main | `5fcd6856` 记录生产 PageHost 实际宽高；`74815b2b` 修复 Tooltip/Popup roots 跨 Dispatcher 遍历并增加双 STA 回归。SDK `8.0.423` 定向 `9/9`，覆盖 PageHost 窄/短/宽恢复、2,000 条合成收件箱首尾可达/虚拟化及 Tooltip 行为；扩展 Popup/焦点组 `8/16`，失败边界未验收。真实 Playnite、物理跨屏和最终呈现未验。详见[证据包](../design/reviews/ui-finesse-round3-20260915/evidence/Q24-02-CURRENT-MAIN-PAGEHOST-RECHECK-20261014/README.md) |
| Q24-04 当前 main | `87070909` 加强 R21 键盘轨迹测试；SDK `8.0.423`、Playnite `net462`/tests `net472` 定向 `8/8`。八入口 90 个当前可见交互控件双向可达、反向顺序互逆，选框行为 6/6。没有完整业务行态或真实 Playnite/OS 键盘；视觉/宿主/最终仍未验。详见[证据包](../design/reviews/ui-finesse-round3-20260915/evidence/Q24-04-CURRENT-MAIN-KEYBOARD-RECHECK-20261014/README.md) |
| Q24-05 当前 main | 复用 b20f1a4b 及现有状态模板；隔离 SDK 8.0.423 Release 构建 0/0，焦点/选中/当前态与选框关闭回焦行为 7/7。选中与键盘焦点区分、双主题及禁用/失焦负例通过；真实 Playnite、OS UIA/读屏、物理焦点像素仍待验。详见[Q24-05 证据包](../design/reviews/ui-finesse-round3-20260915/evidence/Q24-05-CURRENT-MAIN-FOCUS-RECHECK-20261014/README.md) |
| Q24-06 当前 main | `af8ed430` 更新 11 个生产 DataGrid 的 Name/HelpText；进程内 peer 验证 Grid/Table/Selection pattern 与选中行变化，Task 图标按钮名称、进度 Off live setting/焦点负例通过。Release 0/0，R21Automation 24/24，R00-06 freshness 行为 13/13，freshness 14/14。实际 Narrator/Playnite/OS UIA 未验，因此代码完成待验收。详见[Q24-06 证据包](../design/reviews/ui-finesse-round3-20260915/evidence/Q24-06-CURRENT-MAIN-UI-AUTOMATION-20261014/README.md) |
| Q24-07 当前 main | `0243c108` 将高对比与有效动效状态沿视觉树继承；系统语义色、禁用/焦点/选中/Popup、活动动画时钟清理与恢复行为通过。SDK `8.0.423`，Release `0/0`，XAML `24/24`，WPF `13/13`。代码待 CI/外部验收。真实 Playnite、Windows 高对比切换、OS 键鼠/UIA/Narrator、物理 DPI/跨屏和最终呈现未验。R05 Tooltip 相邻类 `2/3` 的失败在 GetCursorPos 前置阶段，TRX 单列。详见[Q24-07 证据包](../design/reviews/ui-finesse-round3-20260915/evidence/Q24-07-CURRENT-MAIN-HIGH-CONTRAST-RECHECK-20261014/README.md) |
| Q24-08 当前 main | 复用现有长文本、TextComposition、键盘与复制实现；本机 SDK `8.0.423` Release `0/0`、XAML `24/24`、定向 `15/15`。双主题长标签/AutomationPeer/Space command、合成中文 composition 和 82 DIP 长值复制负例通过；无生产 UI 改动。真实 Windows IME、Playnite/Narrator、系统剪贴板及运行时 Tooltip 仍待验。详见[Q24-08 证据包](../design/reviews/ui-finesse-round3-20260915/evidence/Q24-08-CURRENT-MAIN-LONG-TEXT-ASSISTIVE-INPUT-RECHECK-20261014/README.md) |
| Q25-01 当前 main | 复用既有搜索/可见列表实现；SDK `8.0.423` Release `0/0`、XAML `24/24`、4 类 `13/13`。2000项5次预热+30个不同查询 p50/p95/max=`45/60/60ms`，受控 Window 20次输入到可见容器=`46.214/75.782/76.334ms`，GamePicker 键盘路径6/6。受控行为可满足；不等于真实 Playnite/物理输入/呈现帧。详见[Q25-01 证据包](../design/reviews/ui-finesse-round3-20260915/evidence/Q25-01-CURRENT-MAIN-HOT-RESPONSE-RECHECK-20261014/README.md) |
| 远端 CI | 当前 HEAD `e30a2976` 的 Actions #1046/run `38902753483` 已终态成功：SDK8.0.423 job `116978745271` 与 SDK10.0.401 job `116978744884` 均通过；Core `125/125`、Worker `362/362`、Playnite source `107` 类/WPF `125` 类全通过。SDK8 Render QA、WPF 静态审查、扩展包生成与诊断上传成功。该 CI 不覆盖外机身份未明的 R09 失败。|
| R00/R01 证据 | HEAD `5c81a070` freshness `14/14 fresh / 0 stale / 0 matched source paths`，2,918 比较路径，package identity `not-provided`；R00-07 最近审计仍 `168 snapshots / 116 warnings / 0 HIGH / 13 MEDIUM`，未重跑且风险保持开放 |
| 真实宿主 | 未启动真实 Playnite；Media Inbox 同进程滚动、物理 DPI/跨屏、IME、Narrator/UIA、ETW 与最终呈现帧均未验，`MANUAL QA REQUIRED` |

仓库锁定 SDK `8.0.423` 已由官方安装脚本安装到 `D:\GameSaveCenterTools\dotnet-sdk-8.0.423`；通过临时进程 PATH 前缀选择，系统 SDK/PATH 未改。历史 SDK/CI/IPC 诊断见[SDK阶段证据](evidence/close-sdk-20261007/README.md)、[CI证据](evidence/close-ci-20261007/README.md)和[IPC证据](evidence/close-ipc-20261005/README.md)。R05 新确定性负例与修复正例已记录；旧的第三轮失败保留为归因证据，不能用旧clean DLL独立通过覆盖。run930、run931、run932、#934、#935 与 #936 的失败原因均未取得完整日志；访问受限期间不盲修。

## 当前执行顺序

用户明确要求持续推进 Round3，不需要逐阶段确认。外机 R09 身份材料待有条件时再核对；缺少的外机输入不阻断其他任务。

1. #1067 的 PDB 锁定/不可读诊断保护已通过双 SDK Actions；文档收口后跟踪新 run 至终态。
2. R23-08 最近复扫未发现新的依赖已满足 Q/R 产品代码项；不为凑进度新建功能。
3. 收到外机身份材料、新的可复现问题或环境前置变化后，重新做准入并按小批推进；外机故障与本机/CI 结果分账。

本轮本机仅 SDK9.0.302 / VSTest17.14.1，测试只证明明确 SHA 的隔离程序集所含项目范围；不是完整 solution build，也不替代 SDK8/SDK10 CI。真实 Playnite/package-host/通知、真实网络与远端、OS UIA/Narrator/IME、物理 DPI/跨屏、最终呈现帧、ETW/宿主性能仍未验。Demo 原目录不可用；视觉沿用已恢复生产基线。

## 架构与生产 UI

- Playnite 插件 `net462`，Core/Contracts `netstandard2.0`，Worker `net8.0-windows10.0.19041.0`，Playnite 测试 `net472`。插件与 Worker 经 Named Pipe 协作，SQLite 由 Worker 持久化。
- 生产外壳为 `AcrylicProductionShellView`；局部共享入口 `AcrylicProductionResources.xaml`，配合 `DesignTokens.xaml`、`WpfUiProduction.xaml`、`Redesign.xaml` 等资源。六工作区：Overview、Save、Trainer、Media、Task、Maintenance；另有 Settings。
- Demo-first 唯一主要视觉基准仍为 `GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design/`；该目录当前工作区不存在。按既有授权沿用已恢复的生产基线，不伪造 Demo 对比结果。WPF 技能只负责质量检查。
- 游戏状态/排序选择沿用已修复的 `DropDownClosed` 提交路径；动态选项、用户选择、真实命令、取消/错误与滚动/虚拟化契约必须保留。页面结构可按当前 AGENTS 授权重构，不沿用旧历史记录中的页面冻结禁令。
- `DashboardViewModel` 主文件约 6480 行，已存在 16 个 partial 文件；这是维护风险，不是未实现功能或已测出的性能瓶颈。

## 验证与交付规则

- 行为矩阵入口 `scripts/e01-behavior-matrix.ps1`；构建 `scripts/build.ps1 -Configuration Release`，可用 `-OutputRoot` 隔离；XAML/source/freshness/RenderHarness 各自只证明其覆盖范围。
- 不把离屏 logical DIP、合成 WPF、进程内 UIA provider 当成 Playnite、物理 DPI、OS 输入或最终呈现帧。
- 每个独立阶段同步文档、记录构建/测试实际结果，单独 commit 并 push 当前远端分支；本次不安装到用户真实 Extensions，不操作真实存档、媒体或云端。
- 后续依次阅读 [PROJECT_MEMORY.md](PROJECT_MEMORY.md)、[WORKLOG.md](WORKLOG.md)、[DEVELOPMENT_HANDOFF.md](../DEVELOPMENT_HANDOFF.md) 与 Git 状态。只在追溯具体问题时读历史大文件。

## 历史：2026-10-13 SDK 10.0.401 CI 门禁实现（后续由 #968 收口）

- #965 用户诊断包确认 CI SDK `8.0.423`；布局类 54 项中 4 项失败，均已在 `5eaa12bf` 以显式逻辑 Grid 宿主修正。#966/run `38352437420` 对该提交已终态成功；其 SDK 仍为 `8.0.423`。
- 已新增 SDK `10.0.401` 独立 Actions job 和隔离 worktree 验证脚本。仓库 `global.json` 与原 SDK8 job 保持不变。本机 SDK `9.0.302` 全量 Release 编译成功；Core `125/125`、Worker `357/357`、Playnite `1566` 通过/0失败；另有 57 条旧 UI 架构断言以既有规则 NotExecuted。布局类 `54/54`。
- 该门禁后来在 #968 对 SDK8 `8.0.423` 与 SDK10 `10.0.401` 两个 job 验证成功；#967 SDK8 源码结构检查的 BOM 修正随 `4aa1ec7` 提交。此处 SDK9 本机记录只作历史交叉检查；当前执行顺序见本页上方。
- 保留其他任务 R04/R05/R07 未跟踪证据。真实 Playnite、OS UIA/IME、物理 DPI/跨屏、presented frame、ETW及宿主性能未验；Demo 原目录不可用，沿用恢复的生产基线。

## 2026-10-15 当前恢复点：R10-03 本机行为通过，Actions 待复核

- `main` 源码基线 `95109c221ec85a52c61c51c0a119a2f3d97efacf`。R10-03 使用已存在的搜索策略、Dashboard 路由、Shell 命令和工作区搜索框映射；只提取可测路由/目标解析入口并增加 WPF routed-event 行为用例。SDK `8.0.423` / VSTest `17.11.1`，Playnite `net462` / tests `net472` Release 定向 build `0/0`，R10 相关组 `13/13`。当前媒体页焦点实际移到 `MediaSearchTextBox`；弹层占用和编辑按键负例保留焦点。
- 当前 R09 外机断言对照在本机 `2/2` 通过；#967 附件的 SDK10.0.401/#983fee5 包也含 R09 `2/2`，但都不能确认外机所用源码/DLL/PDB。旧源或产物错配仍是推断，见[R09 复核](../design/reviews/ui-finesse-round3-20260915/evidence/CI-R09-OTHER-MACHINE-FAILURE-RECHECK-20261014/README.md)。
- `95109c2` 的 run `38645600070` 上次观察为 In progress；本轮 API 连接被拒绝、`gh` CLI 不存在、Actions 浏览器读取超时，当前终态未确认。新阶段提交推送后要继续跟踪两条 CI lane 至明确终态；SDK8 本机结果不替代 SDK10.0.401。下一项 R10-04 快捷键帮助。


## 2026-10-15 当前恢复点：CLOSE-WRAP / SDK8 全量门禁

- 当前 main 基线为 b04660e0af2d17daae8c7dec426598e90de738b2。唯一 tracked 源改动是 WorkspaceTreeGovernanceTests 中 Trainer/Media 的旧 typed getter 断言，已更新为通用 workspace registry 路由；Production PageHost、缓存导航和焦点路由由行为测试覆盖，定向 8/8。没有生产 UI 改动。
- 隔离 SDK 8.0.423 / VSTest 17.11.1：完整 Release solution build 0 warning / 0 error、XAML 24/24；Core 125/125，Worker 356 passed / 1 skipped，Playnite source 455 passed / 27 skipped。WPF 122 类 648 passed / 39 skipped / 1 failed；Q14 654 DIP 行距当前 1/1。详见 CLOSE-WRAP 当前 main 证据包。
- 唯一失败是 R05TooltipTimingBehaviorTests.QuickMousePassesCancelTooltipAndAStationaryDwellOpensIt 在窗口显示前调用 GetCursorPos 返回 false；该类其余 2/2 通过。全体合计 1584 passed / 67 skipped / 1 failed；本机门禁未全绿。系统输入限制保持未验，不伪装为通过。
- 新提供的 #962/#963/#965/#967/#970/#971 ZIP 与既有归档 SHA/提交/SDK 记录相符；其中旧测试修正有单独证据，不替代当前提交 Actions。推送后分别追踪 SDK8 与 SDK10.0.401 到终态。下一步重查当前 main 的 R00/R01 freshness，再推进 CLOSE-OPTICAL-01。真实 Playnite、物理指针/DPI/跨屏、呈现帧、ETW/宿主性能仍未验。

## 2026-10-16 恢复点：R11 测试宿主隔离修复，待新 CI

- 当前分支 `main` 推送 HEAD `ea1e1aead8ffd8c89d09cd753519a522f327f91b`；R13-07 当前小批未提交。Actions #1041/#1042 双 SDK 均成功；#1043 SDK10成功、SDK8在 R11 历史筛选测试失败；#1044/run `38894241925` SDK10 job `116950776709` 成功，SDK8 job `116950777158` 编译/测试成功、离屏渲染 QA 仍运行。
- 已根据 #1043 完整日志定位测试夹具问题：合成测试使用未初始化 Playnite Plugin，生产 SaveCenterView 的列宽 DispatcherTimer 回调进入 `SavePluginSettings/GetPluginUserDataPath` 后空引用。SaveCenterView 增加 internal 行为测试保存回调；生产默认继续走原设置保存路径，R11 的 fake host 用空 callback。R06/Q13 列宽控制器行为另有独立测试。
- 本机 SDK9.0.302 / VSTest17.14.1 fresh isolated Release build：R11 `6/6`、R09 `2/2`、R06 `6/6`、单独进程 Q13 `1/1`；`validate-source.py` 通过。freshness `14/14 FRESH / 0 STALE / 0 source matches`，`package not-provided`。这不是完整 solution build，也不替代 SDK8.0.423 或 SDK10.0.401。
- 用户新贴 R09 外机日志的 `Expected Thickness(2)` 出现在旧测试形态的第151行；当前 main 第151行检查错误色，第154行才检查独立 ValidationStroke 的 2 DIP。当前工作区没有该次外机 `summary.json`；分支/dirty paths/编译源/DLL身份仍未确认。日志文件路径与采集字段见 R09 复核 README。没有放宽断言或改生产模板。
- 当前 R13-07 小批本机 SDK9.0.302 / VSTest17.14.1 Playnite `16/16`、Worker `14/14`，source validation/diff check 通过；证据和 SHA 见 R13-07 复核包。它不覆盖 #1044，也不替代该小批 push 后的新 SDK8/SDK10.0.401 Actions。先收口这两批 CI，再推进 R13-08。外机 R09 输入身份仍缺同次 `summary.json`/DLL/PDB；材料到手前继续主线，不盲改。
