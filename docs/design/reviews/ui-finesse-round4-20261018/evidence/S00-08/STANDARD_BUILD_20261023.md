# S00-06～08 最终标准一键全链

## 精确输入与退出结果

源码`02119729651ae687f2f1e5316ba4e48a3da69045`，固定托管工作树启动时tracked/untracked均干净。SDK8.0.423 / Release，实际调用中文`GameSaveCenter-一键构建安装运行.cmd`→根`GameSaveCenter-Run.cmd`→默认开发安装脚本；无SkipTests/NoStart/SkipClean/SkipPackageArchives。标准`scripts/build.cmd`仍转交build.ps1，根入口默认完成构建测试后再发布、打包、安装和启动。

根命令从2026-10-22 16:46:51Z至17:12:45Z，**退出0**、launcherError null。[原始终态](standard-build/root-result.json)，run **20261022T164654106Z-9af98dc7**，[SDK/步骤/源身份](standard-build/summary.json)、[控制台](standard-build/console.txt)。

## 构建与测试

XAML25/25；解决方案**0警告、0错误**，[编译日志](standard-build/build-console.txt)。

| 分组 | 总数 | 通过 | 原有跳过 | 失败 |
| --- | ---: | ---: | ---: | ---: |
| Core | 157 | 157 | 0 | 0 |
| Worker | 400 | 400 | 0 | 0 |
| Playnite source | 567 | 549 | 18 | 0 |
| WPF，143个隔离类 | 827 | 788 | 39 | 0 |
| 合计 | **1951** | **1894** | **57** | **0** |

146份TRX按逐个UnitTestResult独立复算，计数一致、无error/timeout/abort；57项延续旧UI架构退役skip，无新增跳过。[原始TRX SHA与计数](standard-build/trx-manifest.json)。真实登记路由36/36、排版矩阵Fact1/1（12组）、R09焦点2/2、截图问题工作区布局54/54，以及全部R07/R11/R12/R18均在完整链通过。选定TRX在standard-build/tests，原始与脱敏SHA见[身份清单](standard-build/selected-trx-identities.json)。所有诊断控制台未命中TextServicesContext/InvalidComObjectException/Unhandled exception/未处理异常，[扫描](standard-build/diagnostic-scan.json)。

## 发布、安装和启动

Worker win-x64发布、插件与Worker同源身份、ZIP/PEXT内部校验、安装版本校验通过。清单0.6.73、文件版本0.6.73.0；四个关键DLL产品版本均`0.6.73+02119729651ae687f2f1e5316ba4e48a3da69045`，stage/已安装SHA逐项一致，插件构建DLL也一致。插件SHA`DBD516B5484E8127E8845F8E266CBD58598B72B2C80B7D4796931DA1FF15635B`；ZIP/PEXT各45728893字节、SHA均`70E12F2E4F9CE5CCEB4E2AE11D5BF3FCE525A5AC8B8D149FF9DB28F30F196318`。[完整交付身份](standard-build/delivery-identity.json)、[安装报告](standard-build/last-dev-install.txt)。

安装器按原流程请求旧Playnite退出、结束无窗口残留和旧Worker，再安装启动；新Playnite PID12948在01:12:48、Worker PID85492在01:12:58启动，路径与安装位置匹配，采样时均存活。这只证明进程启动，不证明真实页面、物理输入或物理DPI交互。

## 失败保留与清理

此前f08b85be发现阶段AppDomain shadow-copy路径过长退出1；同DLL对照发现1390用例后，由02119729对齐发现/执行设置，零用例拒绝保持。[发现失败与修复](discovery-failure/README.md)。02119729首次Worker临时数据库清理IOException退出1，断言完成后Dispose失败；同DLL类连续三次12/12和本次完整链通过，未更改清理逻辑或测试限制，未证明偶发占用根因修复。[原始失败与复核](worker-cleanup-failure/README.md)。启动器编码错误也独立保留，不覆盖这些失败。

归档前将全部3次产品运行诊断、最新bin、安装包与报告共1308文件复制并逐个SHA验证。当前原始证据在`artifacts/round4-final-standard/diagnostics/`；最新构建和包分别在其`latest-build/`、`package/`。[映射](standard-build/preservation-map.json)。工作树归档身份01a1649c-4363-7230-890e-4355f7201cea，目录已不存在，[确认](standard-build/worktree-archive-confirmed.json)。

成功进程查询确认无构建/测试/渲染后，清理7个本任务旧目录（含S00-06/07旧构建、S00-08空TEMP、旧原型图、4f40e016旧bin/包），逻辑524855827字节。当前最新包和全部历史诊断、SHA与选定截图保留，旧bin/包路径由本次新交付替代。固定工作树逻辑1041870303字节另由托管归档回收；逻辑量不等于物理释放。[清理范围与卷前后空间](standard-build/cleanup.json)。用户旧Round3 TRX/TestResults未动。

## 当前状态

S00-06实现30e32794、S00-07实现70e42163、S00-08实现f08b85be及构建修复02119729均完成受控验证，仍为代码完成待验收。Round4共40已验收、2已满足、21代码完成待验收、1外部阻塞，全部独立实现已完成，真实Playnite页面/物理键盘/IME/跨屏DPI等宿主证据未齐；S07-03 required checks/分支保护读取权限仍阻塞，未重试被拒探测、未改workflow/保护。Round4不能标整体完成。
