# 第二次标准入口与双图比较布局契约

- 提交 `9d8ea563cc6e75d523ad215b40443f2170469980`，中文默认入口转交根命令，run `20261022T092852858Z-03eed7b9`，SDK8.0.423 / Release，退出 `1`。
- solution 0 warning / 0 error；Core 157/157、Worker 400/400、Playnite source 549 pass / 18 原有 skip / 0 fail。已执行 12 个 WPF 隔离类；累计 1,178 项、1,159 pass、18 skip、1 fail、0 error/timeout/abort。尚未打包安装或启动。
- 唯一失败为 `MediaComparisonSelectionTests.ProductionButtonsKeepSelectedPairAcrossFilterChangesAndClearItOnRequest`。480 DIP 详情列使 1040 DIP 窗口的双图区域小于既有 560 DIP 阈值，生产代码正确使用上下排列；旧测试仍把该窗口作为双列场景。宽场景改为 1280 DIP，并显式要求内部宽度至少 560；500 DIP 内部窄场景仍严格断言上下排列，恢复后仍断言双列。没有放宽比较对象、单次执行、取消、清空或布局断言。断言失败路径也会在 finally 关闭测试 Dispatcher。
- [比较选择最终复验](tests/comparison-selection-final.trx) 5/5、[比较视口最终复验](tests/comparison-viewport-final.trx) 4/4。完整标准链仍须在后续精确提交运行。
- [诊断与源码身份](second-standard-failure/summary.json)、[原始失败 TRX](second-standard-failure/comparison.trx)、[完整控制台](second-standard-failure/console.txt)。控制台归档只移除行尾空格；原始字节仍在 `artifacts/ui-layout-standard-2-console.log`，[SHA](second-standard-failure/raw-console-sha256.txt) 保留。原始 run 目录保留。
- source 组虽退出 0，原始日志仍出现 `TextServicesContext` 在测试 AppDomain 关闭时的 COM 诊断；[同一 DLL、相同授权环境的对照](com-lifecycle/README.md) 保持 549 pass / 18 skip、逐项结果一致，关闭额外测试 AppDomain 后诊断从 2 次降为 0。已调整本地测试执行参数，后续完整标准链仍待验。
