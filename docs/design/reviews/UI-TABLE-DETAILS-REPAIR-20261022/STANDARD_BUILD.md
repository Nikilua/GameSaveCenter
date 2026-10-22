# 截图修复后的标准一键全链

## 输入与终态

- 精确源码：`4f40e01670ddfb90a10d5dedc7d59bee4adc318f`，固定工作树启动时 tracked/untracked 均干净；SDK `8.0.423`、Release。
- 实际命令：`cmd /d /c 'call "GameSaveCenter-一键构建安装运行.cmd" < NUL'`。中文入口调用根 `GameSaveCenter-Run.cmd`；根调用默认安装脚本，测试、打包、安装、启动均未跳过。
- 整链 `2026-10-22 13:37:04Z` 至 `13:59:53Z`，根退出码 **0**，launcherError 为 null；[原始终态](standard-build/root-result.json)。
- Build run：`20261022T133707856Z-62f4c447`，[完整摘要](standard-build/summary.json)、[控制台](standard-build/console.txt)、[编译日志](standard-build/build-console.txt)。原始字节 SHA 保留，Git 归档控制台只去除行尾空格。

## 构建与测试

XAML `25/25`；solution **0 warning / 0 error**。

| 分组 | 总数 | 通过 | 原有跳过 | 失败 |
| --- | ---: | ---: | ---: | ---: |
| Core | 157 | 157 | 0 | 0 |
| Worker | 400 | 400 | 0 | 0 |
| Playnite source | 567 | 549 | 18 | 0 |
| WPF，140个隔离类 | 784 | 745 | 39 | 0 |
| 合计 | **1908** | **1851** | **57** | **0** |

143份TRX逐项独立复算与SDK诊断一致，counterMismatch/parseError/error/timeout/abort均为0；[逐文件SHA与计数](standard-build/trx-manifest.json)。57项延续旧工作台退役相关skip，未扩大skip、删除行为断言或放宽源码身份门禁。R09焦点2/2、R07缩放与断点、R11/R12恢复详情、R18六组大数据预算及ReportedWorkspaceLayout54项均在完整链中通过，关键TRX在standard-build目录。所有诊断控制台未检出本批关注的TextServicesContext/InvalidComObjectException/未处理异常，[扫描记录](standard-build/diagnostic-scan.json)。

## 包、安装与启动

Worker发布、同源程序集身份、ZIP/PEXT内部校验及扩展原子安装成功。清单`0.6.73`、DLL文件版本`0.6.73.0`，产品版本`0.6.73+4f40e01670ddfb90a10d5dedc7d59bee4adc318f`。

- 插件构建/打包/已安装DLL SHA：`97E8012F421802D8842CC6D2BCEEE1C8FB1F7A94F22A3C7A536FF9C72B5295A6`。
- ZIP、PEXT各45,725,946字节，SHA均为`793212C47F26BF48C87A78C5E7E90663A3B8E03E6E4ECF14AC3E5860E1A4B44E`。
- Core、Contracts、Worker的产品版本与已安装SHA也逐一匹配打包目录；[完整身份记录](standard-build/delivery-identity.json)、[安装报告](standard-build/last-dev-install.txt)。
- Playnite PID `51232`，启动本地时间21:59:52；已安装Worker PID `31176`，启动22:00:03，读取身份时均在运行。这里证明安装和进程启动，不代替真实页面操作或物理输入验收。

## 离屏与原失败

同提交独立RenderHarness退出0，`render-qa OK`、0 PROBLEM；[报告](render-qa-report.txt)，代表截图在screenshots目录。此前53c9b14c并行离屏运行的侧边栏rapid-toggle未收敛，[失败报告](render-concurrent-failure-report.txt)保留；本次没有修改侧边栏或等待阈值，不把并行压力记为已证实根因。此前默认链的失败/中断均在[FOLLOWUP](FOLLOWUP_STANDARD_DIAGNOSTICS.md)、[首次](FIRST_STANDARD_FAILURE.md)、[第二次](SECOND_STANDARD_FAILURE.md)中保留，没有用此次通过覆盖。

## 归档、清理与边界

固定工作树已归档，可恢复的attachment身份见[归档确认](standard-build/worktree-archive-confirmed.json)；原检出已不存在。归档前复制并校验最新1277个构建/诊断/打包文件、两种安装包，以及全部6个工作树run的1212个诊断文件。原C路径到保留D路径见[映射](standard-build/preservation-map.json)，最新原始产物在`artifacts/ui-layout-standard-10/`，历史失败原始诊断在`artifacts/ui-layout-standard-8/prior-diagnostics/`，当前完整审计在`artifacts/ui-layout-fix/render-serial-final/`。

通过SDK8正常关闭编译服务器，再成功查询确认没有构建/测试/渲染进程后，规范化并核对artifacts前缀，删除本阶段39个旧目录（逻辑501,429,814字节）；包括两次旧失败构建、一次定向构建、两套旧离屏图及34个已完成测试临时目录。使用PowerShell LiteralPath，未跨shell删除。另归档工作树含逻辑1,629,890,777字节；逻辑量不等于物理释放量，卷前后空间分别保留在[清理](standard-build/cleanup.json)、[归档前](standard-build/pre-archive.json)和[归档后](standard-build/post-archive.json)。保留最新构建/包/审计、引用证据和用户旧Round3 TRX/TestResults。

真实Playnite页面交互、物理DPI/键盘/IME未冒充验收；Round4仍为40已验收、2已满足、18代码完成待验收、3待开始、1外部阻塞。T00-01复用本次完整链记已满足；其余专项条目保持未准入状态。本次证据文档提交只做源码/链接/账本校验，不为记录它触发的通过再制造CI记录循环。
