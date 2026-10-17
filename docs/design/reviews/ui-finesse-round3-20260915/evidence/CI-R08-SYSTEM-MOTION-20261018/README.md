# R08 system-motion CI 时长夹具复核（2026-10-18）

## 基线 run #1078

提交 `5241add2fd93ca04ca65b95242a783239eb7e65e` 对应 [Actions #1078/run `39054894478`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39054894478)。两条 job 均已终态，但 run 整体为失败：

| Lane | Job | 结果 |
| --- | --- | --- |
| SDK 8.0.423 | `117460748837` | Release build `0 warning / 0 error`；Core `125/125`；Worker `362/362`；WPF 隔离在 `[66/125]` 的 R08 system-motion 测试失败 |
| SDK 10.0.401 | `117460748676` | Release build 与完整测试成功；Core `125/125`、Worker `362/362`、WPF 隔离 `125` 类 |

SDK8 完整 job log 指向 `R08SystemMotionHotChangeBehaviorTests.SystemAnimationPreferenceChangeEndsEntranceAndReenableDoesNotReplayIt` 第58行。启动入场时钟的正向断言失败，日志值 `opacity=1; loaded=True`，`DependencyPropertyHelper` 未观察到动画时钟。TRX 位于 CI 诊断目录 `20261017T174255023Z-77415c26/tests/wpf.R08SystemMotionHotChangeBehaviorTests/`。测试退出时出现的 WPF TextServices `InvalidComObjectException` 在该断言和 `Test Run Failed` 后，日志没有证据表明它是失败原因。

SDK8 诊断 artifact：ID `12109790764`，609,140 bytes，SHA-256 `edb89c8b722baeaff502cc113df97f7f252bc4a4f44ed67ba66b862caf18373f`。SDK10 artifact：ID `12109567791`，776,313 bytes，SHA-256 `26d32ae0062ff13e94801656e0694c8b7e61b35a7f6914cc298720c52abc33de`。完整 job logs 已通过 GitHub connector 读取。SDK10 的成功不覆盖 SDK8 失败；本次 CI 也不代表用户外机 R09 失败已解决。

## 夹具修正

系统动效热切换测试需要先确立一个正在运行的 entrance clock，再由真实 `GameSaveCenterSettingsView.OnSystemParametersChanged` 回调触发关闭和恢复检查。原夹具在窗口加载期间改写资源字典时长，但 SDK8 runner 未能在动画启动断言观察到活动时钟。现有日志没有提供启动瞬间的实际资源时长，故不把具体系统参数或 Dispatcher 覆写路径记为已证实根因。

共享 `GscMotion.AnimateEntrance(element, offset)` 生产入口仍按元素资源读取 normal/slow 时长，再委托给新增的显式时长重载。该行为测试直接传入一秒时长，消除 fixture 对宿主资源刷新时序的依赖，但仍通过同一个 `Track`、动画建立和完成回调。测试保留动画实际启动正向断言；关掉系统策略后仍检查两个时钟释放、Opacity/Y=`1/0`；重新打开并经过原时长后仍检查不重播。没有弱化交互或负例，也没有更改生产默认策略。

## 本机复核与下一步

- 本机 Windows：SDK `9.0.302`、VSTest `17.14.1`。修改后的 Playnite `net462` / tests `net472` Release 相关项目编译，目标行为测试 `1/1` 通过。
- `python scripts/validate-source.py .` 通过，`git diff --check` 通过。无视觉变化，不适用截图验收；未启动真实 Playnite，未改 Windows 动效设置。
- 本机 SDK9 结果不替代 SDK8.0.423 或 SDK10.0.401。阶段提交后的新 Actions 结果见下节。

未验证边界：真实 Playnite 系统偏好通知、系统高对比度环境中的用户界面行为、OS UIA/读屏、物理 DPI/跨屏、最终呈现帧和宿主性能。外机 R09 同次 checkout/test DLL/PDB/plugin DLL 身份继续缺失，与本次 R08 CI 结果无关。

## #1079 双 SDK 收口与 R00/R01 freshness

代码提交 `de8bdbb176bb7ae85c4ca3f488f0aeb37958bfec` 对应 [Actions #1079/run `39057273073`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39057273073)，两条 job 均终态成功。完整日志确认 SDK8.0.423 job `117467706465` 和 SDK10.0.401 job `117467706337` 都运行了完整 Playnite WPF 隔离清单 `125/125`；其中 `UiFinesseFoundationTests` 是第 `121/125` 类，随后 R08 system-motion 类也运行，最终均报告全部 Playnite 测试成功。两 lane Release build 均 `0 warning / 0 error`，Core `125/125`、Worker `362/362`。

SDK8 另完成 `render-qa OK`、WPF 静态审查（30 XAML、0 errors、27 warnings、177 info）、扩展包生成和上传。Artifacts：SDK10 diagnostics `12110214194`（775,251 bytes，SHA-256 `fa95154a4b93daedd4d57f26547a0b02959bb53c40c222a2e0f3b803d992dc2e`）；SDK8 diagnostics `12110638801`（1,628,891 bytes，SHA-256 `2fb041e67a8e5f5b39caedcea4adabf9cddd65e7d5aba1e3057fa07b7839141b`）；SDK8 package `12111535634`（90,314,202 bytes，SHA-256 `dd45ec28d148d19202a80acdb1b917f59ade012c24dca0a7ec621b34eec59381`）。

R00/R01 freshness 初扫的 14 条记录中，只有 R00-03 与 R01-04 命中 `GscMotion.cs`。#1079 全量隔离测试在两种 SDK lane 中覆盖这两个记录共用的 `UiFinesseFoundationTests`，因此将这两条登记证据更新到精确代码提交 `de8bdbb`；复核脚本在更新后的基线返回 `14 fresh / 0 stale / 0 matched source paths`，package identity `not-provided`。这只刷新已登记源码范围，不代表包安装或真实 Playnite 呈现。完整 job log 已读取；CI 通过只说明相同提交在对应 GitHub runner 输入下通过。

R00-03 与 R01-04 继续标记“已满足（受控 WPF 行为）”。真实 Playnite 生命周期/系统偏好通知、OS UIA/读屏、物理 DPI/跨屏、最终呈现帧和宿主性能未验。用户外机 R09 的 `1/2` 失败仍是独立真实问题，当前同次 checkout、TRX、test/plugin DLL/PDB 身份仍缺；本次 CI 不覆盖该失败。

## #1080 SDK10 新发现：应用动效切换夹具

文档提交 `7b5ea6b1861ca813495fc12d06fe9893bc541e48` 对应 [Actions #1080/run `39061487759`](https://github.com/Nikilua/GameSaveCenter/actions/runs/39061487759)。SDK10.0.401 job `117480203788` 的 Release solution build 成功（0 warning / 0 error），Core `125/125`、Worker `362/362`；WPF 隔离第 `61/125` 类 `R08MotionHotChangeBehaviorTests` 在正向动画启动前置失败：`opacity=1; loaded=True`，期望存在动画时钟但没有。完整 job log 已读取。失败后的 TextServices `InvalidComObjectException` 出现在断言之后，不是日志所示的断言原因。SDK10 diagnostics artifact `12112485036`，518,122 bytes，SHA-256 `31a556e3a6ab9777f4512b13bd73d649ebe5be5b058a71a27a67d9eb38c6bc9d`。

该 app-preference 测试此前在窗口 `Show()` 后写入 `GscMotionNormal/Slow` 资源，再通过默认入口启动动画。改为显式一秒时长，避免启动正例依赖活动窗口资源刷新；调用仍经过共享 `GscMotion` 的真实跟踪与取消逻辑，现有动画启动、关闭后清钟、重新启用不重播断言都保留。没有改生产动画策略或放宽断言。

本机 SDK `9.0.302` / VSTest `17.14.1`，Playnite `net462` / tests `net472` Release 定向构建及 `R08MotionHotChangeBehaviorTests` `1/1` 通过；source validation 与 diff check 通过。源文件 SHA-256 `EE03FED5B0528AE2629260E4FAD7F62401694E702E59E6C3658E6CFEAE4351FA`，测试 DLL SHA-256 `4A0F4BFF94EAFCEC152DEB31F1DBEDD82F138C96B878CDEA3F89217316EB8015`。脱敏 TRX：[R08MotionHotChangeBehaviorTests-sdk9302-local.trx](tests/R08MotionHotChangeBehaviorTests-sdk9302-local.trx)，SHA-256 `BC1793C693679F8D17813488A014503ACE9A260E938639DD477EBFCE87AE6799`，7,356 bytes。本机 SDK9 不替代 GitHub SDK8/SDK10。

记录时 SDK8.0.423 job `117480204000` 仍在编译与测试阶段，#1080 不是终态，不能据此结束 follow-up。修正提交还需独立 CI 双 SDK 复核。当前真实 Playnite、OS 输入/UIA、物理 DPI/跨屏、最终呈现帧和宿主性能仍未验。

## be314f59 修正提交与 R09 外机断言版本差异

R08 夹具修正已提交并推送至 main：代码 SHA be314f59c6a3a0b93d890cf39ee4c6c26ff2c523，提交说明“固定应用动效测试时长”。Actions #1081/run 39063179675 的 SDK8 job 117485212854 和 SDK10 job 117485212614 记录时仍在编译/测试；不据此报告 CI 通过。前一 #1080 SDK8 job 117480204000 记录时在生成扩展包，SDK10 job 117480203788 已失败。状态以 GitHub run 为准。

修正提交对应的 R00/R01 freshness 脚本输出为 14 fresh / 0 stale / 0 matched source paths，package identity not-provided。测试夹具修改没有命中已登记的 R00/R01 source paths。

用户外机 R09 文本报告 Chrome.BorderThickness 期望 2,2,2,2、实际 1,1,1,1。仓库历史显示：初始测试提交 3f7d0b3027c968620e412d38394c69e060fc0564 直接断言错误态 Chrome 为 2 DIP；提交 f13b9e05de0cf502ac61e97b0fbad8433b339b3f 同步修改生产模板和测试，将内容 Chrome 固定为 1 DIP、独立错误 ValidationStroke 设为 2 DIP，并新增恢复状态检查。当前 main 用后一形态，#1079 精确代码 WPF 全量 125/125 通过。外机日志测试形态与当前源码不同，可能涉及旧/混合测试程序集或 checkout；没有同次源码、TRX、test/plugin DLL/PDB 身份前，无法确定根因或确认外机已修复。

本机 SDK9 1/1、GitHub #1079 全量测试和当前代码行为分别是有限证据，均不证明用户外机实际加载了同一 test/plugin 产物。真实 Playnite、外机程序集身份与呈现仍待验证。
## #1081 代码修正与 #1082 文档提交双 SDK 终态

代码提交 be314f59c6a3a0b93d890cf39ee4c6c26ff2c523 的 Actions #1081/run 39063179675 已整体 success。SDK10 job 117485212614、SDK8 job 117485212854 均 Release build 0 warning/0 error、Core 125/125、Worker 362/362、Playnite WPF 隔离 125/125；日志明确覆盖 R08MotionHotChangeBehaviorTests（第 61/125 类）、UiFinesseFoundationTests（第 121/125 类），随后报告全部 Playnite 测试成功。SDK8 另 Render QA OK、30 个 XAML 静态审查 0 errors / 27 warnings / 177 info、扩展包生成及上传成功。

#1081 artifacts：SDK10 diagnostics 12112518432（774,065 bytes，SHA-256 b253a85499ae31178037772d42d7831a0d2e45dba9ea89b5b93b57189f600f08）；SDK8 diagnostics 12113496075（1,623,054 bytes，SHA-256 ac35ce2c95dba9c00568ea5ae4c605d0e620076d026b9a329432093db0d9bcac）；package 12113301806（90,314,050 bytes，SHA-256 20a2eeccb8e358096ee181cc0d735061282c858680549d5bc370cd5efa46a390）。

文档提交 b9eb697152f6def41faeca5753e91e1053d3f829 对应 #1082/run 39063783824 也整体 success。SDK10 job 117487049149、SDK8 job 117487049347 均 Release 0/0、Core 125/125、Worker 362/362、WPF 125/125；SDK8 Render QA、WPF 静态审查与扩展包均通过。Artifacts：SDK10 diagnostics 12112952511（774,340 bytes，SHA-256 3f0a28ab24a073ef89627c191d8176d838172b493e911eae3a2b4776fe439d0d）；SDK8 diagnostics 12113009402（1,625,081 bytes，SHA-256 f15f0ade7391f9e8ed62f92d3e7244fffeab881eb11828fc24d783c958954e33）；package 12112844773（90,313,972 bytes，SHA-256 efb9999efd7cee64ac9b4aa4bced0a52ec104bb47366801f2d98d42d24c4a2d5）。

作为修正前对照，#1080/run 39061487759 整体 failure。SDK8 job 117480204000 的 Release build、Core 125/125、Worker 362/362、WPF 125/125、Render QA、静态审查与包上传全部成功；SDK10 job 117480203788 在 R08 活动动画时钟正例失败。SDK8 artifacts：diagnostics 12113205878（1,627,137 bytes，SHA-256 adbd95a4357a40729a158f41bda0f2b66dd3cb54106a26d4f300fdef229b481e）；package 12112444661（90,313,860 bytes，SHA-256 4c8a8f41c47d966fa9105883ac59a1dbedec4bb2599214960261a9b4a26e913c）。SDK10 diagnostics 12112485036（518,122 bytes，SHA-256 31a556e3a6ab9777f4512b13bd73d649ebe5b058a71a27a67d9eb38c6bc9d）。

本机 SDK9.0.302 / VSTest17.14.1 定向构建与 R08 类 1/1 通过只作补充；SDK8 与 SDK10 CI 分开记录。对用户“本机成功是否代表外机成功”的答复是否定的：clean-runner CI 可验证精确 commit 的构建/测试，不证明外机实际加载 DLL 或 Playnite 呈现。

## R09 外机断言与当前源码版本差异

外机粘贴日志期待 Chrome.BorderThickness=2、实际 1。历史测试提交 3f7d0b3027c968620e412d38394c69e060fc0564 直接检查错误态 Chrome=2 DIP；提交 f13b9e05de0cf502ac61e97b0fbad8433b339b3f 起生产模板和测试改成 Chrome=1 DIP、独立 ValidationStroke=2 DIP，另有错误恢复状态断言。当前 main 使用后一版，#1079、#1081、#1082 的完整 WPF 125 类均通过。这个源码形态差异提示外机可能使用旧/混合 test 产物或 checkout，但外机同次 checkout、TRX、test/plugin DLL/PDB 身份仍缺；不推断成已确认根因或已修复。

## #1083 文档检查点双 SDK 终态

历史文档 HEAD `0bde982d49700465498bd0732651e21c10cebe43` 对应 Actions #1083/run `39066560072` 已由 GitHub API 核验终态成功。SDK8.0.423 job `117495353783` 和 SDK10.0.401 job `117495353928` 均成功。两 lane 的 Release build/test、Core `125/125`、Worker `362/362`、WPF 隔离 `125/125` 通过；SDK8 Render QA、30-XAML 静态审查及 package 上传成功。产品代码基线为 `be314f59c6a3a0b93d890cf39ee4c6c26ff2c523`，该 run 仅验证文档 HEAD 所含的精确代码/runner 输入，不能证明外机安装或加载程序集。

Artifacts：SDK10 diagnostics `12113568013`，775,150 bytes，SHA-256 `1e4fd2c98056804044c60021324632621039a0454267d8a7f364a69d108d7127`；SDK8 diagnostics `12114328619`，1,627,808 bytes，`1e331115c26317bab230f999674eb534cfaeab219f6c9d8538ad414586b42dfb`；package `12114328615`，90,313,962 bytes，`785878fa1ecd78fa051fd3b39e4effdd530e2b1af4352b498722341faa70924e`。

本轮再次附加的文本 SHA-256 `F3DDDC6445E0BB1A5E4CD44EEEEC2C2C98834FC8C81482A67E456C5DB5864514` 与已登记外机日志完全一致，仍是 `20261016T052146719Z-d9d99ac4`，不是外机新运行。当前源 line 151 为 Brush 断言，而该日志在 line 151 报 Thickness 2/1；保留失败事实并等待该次 `summary.json`、TRX、test/plugin DLL/PDB 身份以区分外机输入，不能将本机或 CI 通过写成外机通过。
