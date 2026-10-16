# R13-08 当前 main 失败帮助行为复核

日期：2026-10-16。源码基线为已推送 `e30a29767a903cfe42d16885dc5ed15c3885f05c`。本阶段只补测试，不改生产服务、DTO、ViewModel、XAML 或命令。

## 覆盖内容

扩展 `R13CloudTransferStageBehaviorTests.ProductionMaintenanceQueueRendersUploadedAndVerifiedAsDifferentStates`，在既有隔离 STA 窗口中操作真实生产 `MaintenanceView` 和 `DataGrid`，使用合成记录：

- 选中已识别的认证失败后，检查失败类别、下一步文案和帮助容器可见；通过实际 `Expander` 的 `IExpandCollapseProvider` 展开/收起原始诊断，核对原始错误码和错误文本随展开状态显示/隐藏。
- 再选中未知错误码 `RCLONE_PROVIDER_NEW_FAILURE`，检查帮助容器收起、分类与下一步为空，同时原始错误码/诊断文本仍可展开读取；收起后原始诊断不可见。
- 所有数据由测试 fixture 创建，没有连接真实云端、读写用户数据或调用外部命令。

这覆盖 R13-08“只显示已识别原因；可展开原始错误码，未知错误保留诊断而非猜测”的受控生产视图行为；旧的源码 hook 检查已改名为 `MaintenanceInspectorFailureHelpSourceHooksRemainWired`，不会冒充 UI 行为证明。

## 本机验证

- SDK `9.0.302`、VSTest `17.14.1`；Playnite `net462` / 测试 `net472` 的 Release 项目与引用构建成功，定向类 `16/16` 通过。
- 成功 TRX：[R13CloudTransferStageBehaviorTests-sdk9302.trx](tests/R13CloudTransferStageBehaviorTests-sdk9302.trx)，SHA-256 `865C934956B5309D8F50CF002B2A93E9B23267BC709FC1ED69C77AF0DF410BAB`。本机 test DLL SHA-256 `C09A11771620AAF44D2D611C8650186E1576C6846E4103DE9263D5457FA9CFBC`，PDB SHA-256 `7D132E3E2F8E2521889EF61463E470F0490A716D8C2FD156D05A40CAC190B90E`；生产插件 DLL SHA-256 `AD4F6B9B4751E4718BCF74DA7B7EE2286D1F11E70F2936038132C0EF5C49D42A`。
- 测试源 SHA-256 `0520D4C631160E035EA0AEFCB336637485142CE1A4A592C34DD7EDBB40C64359`。`python scripts/validate-source.py` 通过，`git diff --check` 通过。
- R00/R01 对本次唯一变更测试路径执行显式 `ChangedPath` freshness 检查，仍为 `14/14 FRESH / 0 STALE / 0 matched source paths`；JSON 见 [freshness-e30a2976-r13-08-test-change.json](../R00-R01-CURRENT-MAIN-RECHECK-20261016/freshness-e30a2976-r13-08-test-change.json)，SHA-256 `CBD7AED99C7F75AACB064030453E83A021427048B15843B73DA9EC06D1C8E5C9`。

这是本机 SDK9 的定向项目验证，不是完整 solution build 或完整 Playnite 类扫描。Actions #1046 对父提交 `e30a2976` 的 SDK8/SDK10.0.401 成功不包含本次测试改动；代码推送后仍须分别追踪两条 CI lane 到终态。没有新截图或 XAML 变化；真实 Playnite/package-host、UIA/读屏、网络/rclone 与最终呈现仍未验。

## 外机 R09 错误与验证范围

用户另一台机器在独立一键安装流程中 Core `125/125`、Worker `357/357` 通过，但 `R09FocusOutlineBehaviorTests` 为 `1/2`，导致整个流程 exit `1`。因此本机通过不能被当作该机安装成功；这项外机失败仍未解决。

该次异常报告 `Expected Thickness(2)` / `Actual Thickness(1)`，PDB 堆栈指向 R09 测试源 line 151。当前 main line 151 比较 Brush 颜色，不可能产生 Thickness 值；历史提交 `3f7d0b30` 的同一行正是 Chrome `2 DIP` 断言。当前 `ValidationStroke` 的 `2 DIP` 断言位于 line 154，且当前生产模板仍将 Chrome 固定为 `1 DIP`、ValidationStroke 固定为 `2 DIP`。所以该次测试代码或符号行映射与当前测试源不一致；具体是旧测试程序集、旧 PDB、错误 checkout 或其他构建输入，仍需该次 diagnostics `summary.json`、TRX、DLL/PDB 身份才能区分。当前 CI 的 R09 通过不覆盖这台机器的实际失败。

当前 `dev-install-run.ps1` 默认会为一次安装新建随机 `artifacts/gsc-b/<token>` 输出目录，构建和 `--no-build` 测试共用该目录，降低旧 bin/obj 被复用的机会。但用户日志没有记录实际执行的脚本修订和同次 `summary.json`，不能据此推断那台机器采用了当前脚本。
