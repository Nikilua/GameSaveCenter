# WPF 测试宿主关闭诊断对照

标准 run `20261022T092852858Z-03eed7b9` 的 source 组全部通过，但控制台在测试结束后输出两次 `TextServicesContext.Uninitialize(Boolean appDomainShutdown)` / `InvalidComObjectException`。没有把退出 0 等同于没有诊断异常。

使用同一 `9d8ea563` 测试 DLL、SDK8.0.423、相同 source filter，以及与标准构建相同的授权环境，仅增加 `-- RunConfiguration.DisableAppDomain=true` 后：567 项仍为 549 pass / 18 原有 skip / 0 fail；按测试名逐项比较结果完全相同，COM 诊断从 2 次变成 0 次。见[对账与程序集/原始控制台 SHA](comparison.json)、[TRX](source-no-extra-appdomain.trx)、[原控制台](baseline-source-console.txt)、[对照控制台](source-no-extra-appdomain-console.txt)。归档控制台只规范化行尾空格，原始字节哈希记录在对账 JSON 中。

依据 [VSTest 官方配置文档](https://github.com/microsoft/vstest/blob/main/docs/configure.md)，该设置要求适配器不创建额外测试 AppDomain。当前脚本已经对每组/每个 WPF 类启动独立进程，保留该进程隔离；额外 AppDomain 不是本项目所需的边界。对照结果说明诊断与额外域卸载有关，并不声称定位了系统 COM 实现内部的具体释放顺序。

本地 `run-playnite-tests-isolated.ps1` 对每次实际执行附加该设置，保持原来的 SDK、发现、filter、140 个 WPF 类、超时、TRX 和失败传播。没有修改 CI workflow、required checks、测试断言或 skip 规则。发现步骤不变。后续默认 .cmd 全链必须验证此设置下所有 WPF 类、打包安装和启动。

诊断时另有一次普通沙箱 source 对照，9 个 Named Pipe 用例因已有权限检测而跳过（540 pass / 27 skip）；该次不作为验收依据，也没有用于改变 skip。随后相同授权环境的完整逐项对照恢复 549 pass / 18 skip。单独 R19 两项与五个模型类 41 项探针未复现 COM，不能据此归因给这些类。
