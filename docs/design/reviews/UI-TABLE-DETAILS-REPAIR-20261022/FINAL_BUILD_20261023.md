# 存档操作区留白修复：构建与安装记录

验证源码：`0a91c39962b373c2170b31fbe3fd363887465fc2`，版本 `0.6.73`，Windows、SDK `8.0.423`、Release。后续纯文档提交不改变本记录绑定的源码身份。

## 问题与修复

历史操作栏移入摘要右侧Auto列时按1604 DIP测量，摘要实际宽度变为0 DIP，卡片高达5754 DIP。旧测试只检查1400 DIP下的行号，没有检查宽窄往返后摘要宽度和留白。

摘要与操作栏现在各占一个Auto行，并共用有限卡片宽度；历史范围标签/选择器成组，其他历史导航按钮独立换行。回归覆盖860～2100 DIP、详情栏断点往返，验证摘要拿到完整内容宽度、内容外留白有界、每个按钮不超出操作栏，以及足够宽时单行恢复原始行距。原长提示场景保留，不靠隐藏真实错误/预览/结果信息压缩高度。

## 当前源码验证

| 项目 | 结果 |
| --- | --- |
| 官方`build.ps1 -Configuration Release -SkipTests`解决方案构建 | 0 warning、0 error；XAML25/25 |
| 提交/源码身份 | 2/2 |
| WPF资源完整类 | 140通过、39既有跳过、0失败 |
| 报告布局，双主题 | 54/54 |
| 窗口缩放压力 | 1/1 |
| 紧凑表格视口 | 7/7 |
| 历史时间导航 | 6/6 |
| 上述6类合计，每类独立进程 | 249项：210通过、39既有跳过、0失败 |
| 最终RenderHarness | render-qa OK、0 PROBLEM；浅/深主题、多尺寸与生产PageHost |
| 源码结构验证 | validate-source.py通过 |

本轮先用官方入口编译，再对其输出运行相关类；没有把`-SkipTests`的构建记录本身当作测试通过。上述249项是本次变更的定向检查；此前f3ac4c74的1954项全量结果只属于前一源码。

本机原始证据：

- 官方构建：`artifacts/build-diagnostics/20261023T064813849Z-3a7c85a4`。
- TRX、计数及几何：`artifacts/save-summary-reflow-20261023/official-tests/`、`official-test-summary.json`、`official-geometry.txt`。跳过数按实际UnitTestResult的NotExecuted统计，避免TRX Counters.notExecuted为0造成误记。
- 修复前失败、初次缺少构建身份的资源检查及后续恢复均保留在同目录的`before/`、`final-regression/`；没有删除失败证据或放宽身份门禁。
- 最终截图/报告：同目录`render-final/`；已人工查看存档1100×720、1366×768、1600×900，以及浅色1366×768。它们是离屏WPF证据，不是物理DPI/真实输入证据。

## 打包与安装

插件、Worker及四个共享程序集全部为 `0.6.73+0a91c39962b373c2170b31fbe3fd363887465fc2`。`package.ps1 -SkipBuild`复用已验证的`artifacts/gsc-b/a91c3991`，发布、身份校验、ZIP和PEXT生成成功。

PEXT SHA256：`2B91020A0E8122059D27CA37748DB2AF4F11D90ECBEFAF01F518B5E893B91ABB`。

安装使用标准安装器的原有函数，替换前核对绝对目标路径，确认宿主/Worker未运行。239个文件逐一SHA256一致，清单`0.6.73`、DLL`0.6.73.0`。已启动`D:\software\Playnite\Playnite.DesktopApp.exe`，记录PID36620、主窗口句柄9047204；原始记录为`artifacts/save-summary-reflow-20261023/install.json`和`install.log`。

Computer Use随后尝试真实页面捕获：先返回窗口最小化，按工具要求恢复后返回`foreground window did not report a process id`，未取得页面截图。`host-page-attempt.json`明确`pageRenderingVerified=false`，没有把进程/窗口启动扩大成真实页面交互通过，也不要求用户补充真实Playnite反馈。

## 清理

清理本阶段不再需要的5个旧构建目录`8cba50a1`、`6f4df87e`、`34e8fa34`、`80d62df3`、`cb34d3a5`；删除前验证全部位于仓库`artifacts/gsc-b`，保留当前`a91c3991`、最新包、离屏报告、各阶段原始诊断和TRX。用户旧Round3 TRX/TestResults没有加入提交、改写或删除。
