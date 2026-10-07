# CLOSE-CI-01 诊断与侧栏测试前提（2026-10-07）

本阶段从 main `0e92f82b397da81d98156009ff9fef0d7190ca36` 开始。版本仍为 `0.6.73-development-preview`，没有用户安装或正式发布。提交前测试使用 dirty build；InformationalVersion 中的基线提交不能代替本阶段源码身份。最终源码逐文件 SHA 与测试 DLL SHA/MVID 见 [身份快照](final-source-and-dll-identity.json)。

## 本阶段变更

- 两条活动侧栏动画测试原先只设置应用开关，仍依赖机器的 `HighContrast` / `ClientAreaAnimation`。增加实例内、internal 的系统偏好测试输入；生产默认继续读真实 Windows 偏好，没有全局测试开关、OS 设置修改、跳过或容差放宽。八种应用/高对比度/系统动画组合及默认真实偏好路径有独立验证。
- build 与独立 Playnite runner 为每个 native 步骤保存真实退出码、开始/结束时间、console；Core/Worker/source 与每个 WPF 类保存独立 TRX。摘要记录 SDK、基线提交、dirty 路径、源码 hash 和程序集 hash/MVID；构建失败不会伪造 TRX。`runType` / `testsRequested` 区分构建与测试。
- CI 在成功或失败后上传诊断；权限仍为 contents/read。日志去除当前 repo/profile 路径、TRX 主机名称和常见 URL 凭据。原始断言/堆栈、失败状态、跳过数保持。
- PowerShell 5.1 的 native stderr 不能中断退出码采集；启动不存在的程序明确记录 `-1` 与 launchError，不继承上一进程的 `0`。诊断写入即使失败也会恢复 TEMP/TMP、构建身份环境和当前位置。

## 实际验证与限制

| 验证 | 结果 |
| --- | --- |
| SDK8.0.423 Release | XAML24/24；0 warning / 0 error；PowerShell5.1与7实际构建成功，TEMP/TMP恢复 |
| 诊断自测，PowerShell5.1 / 7 | 两者通过；受控进程退出7/0、stderr、方法/断言/堆栈、缺失程序、JSON/TRX脱敏 |
| Core | 125/125 |
| 侧栏类独立回归 | 22/22，原两条断言及八种策略组合均通过 |
| Playnite source | 470 passed / 18 原有 skipped / 0 failed |
| WPF 全量尝试 | 前23类通过，第24类Q14失败，后89类未执行；runner退出1 |
| Worker | 本阶段357/357，exit0，TRX/console独立归档；实际22.535分钟 |

本阶段两次 Q14 都首先失败于验证错误状态：TextBox几何组件3（高度）`36 → 37 DIP`，超过原有`0.75 DIP`门限；方法、断言和堆栈见 [独立失败](verification/q14-before-console.txt) 与 [全量失败](full-playnite/wpf.Q14ToolbarAlignmentBehaviorTests-console.txt)。本次没有执行到654 DIP单行边距检查；10月5日的20 DIP残留失败仍保留，不能据此撤销。共享TextBox模板错误态增加BorderThickness的测量影响需下一布局阶段验证。

基线最新CI run `37336829173` 仍为SDK10.0.401、两条活动动画断言失败，见 [原日志](baseline-ci-37336829173.txt)。本机22/22不能证明远端根因或完整CI已恢复；提交后的远端产物另记。CLOSE-CI-01保持IN_PROGRESS，SDK/布局/渲染/包门禁没有被本阶段签收。

源码结构检查最终通过；PowerShell AST与git diff --check通过，git fsck --full退出0，仅既有dangling对象。首次BOM失败已按仓库PS5.1规则修复，未隐藏失败或修改检查器。

目录中的早期摘要来自当时加载的helper版本，可能没有后来新增的runType/sourceFiles；最终身份快照和final-build-diagnostics是最终脚本版本。全部标为dirty验证，不能称clean候选发布。额外COM清理输出独立保留，不解释此前断言。

## 后续

先处理CLOSE-SDK-01与CLOSE-WRAP-01，再补证。当前修改还命中R00-03、R01-01、R01-04，除原5项stale外需精确重验这3项；baseline未改，提交后按实际HEAD重新计算。受控WPF不代表真实Playnite、物理DPI、OS输入或最终呈现帧；宿主Media Inbox、用户Q06/R08及恢复/发布矩阵继续开放。
