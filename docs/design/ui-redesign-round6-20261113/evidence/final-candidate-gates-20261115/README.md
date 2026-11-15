# C01–C03 最终候选门禁与 CI 证据（2026-11-15）

## 源码身份和 GitHub Actions

RenderHarness/证据链最终提交为 `15f82b650fde8cbe370a48f8fbd6ba8978bf4dc3`；生产候选页面仍为 `9880c81be09365d02b9335a633be95506095c8d9`，C01–C03 没有修改产品 `src/`。截至2026-11-15，下面五个精确源码提交的两个适用 job 均已 `completed / success`，job 内所有步骤也成功。artifact ID、所属 run 与 GitHub digest 一并列出，便于直接查原始证据。

| 收尾项 / 提交 | 精确 Actions run | 基线编译、测试与打包 job | SDK 10.0.401 job | 原始 artifact（ID；GitHub SHA-256 digest） |
| --- | --- | --- | --- | --- |
| C01 修正 `aa590a1b8c710e0788098b27b128719001aeead3` | [#1583 / 43707721313](https://github.com/Nikilua/GameSaveCenter/actions/runs/43707721313) | `132792318458` success | `132792318643` success | diagnostics `14161716894` `78d9ad5f74238b1e4c288d382fb476ece1be39e34eab61e653d4c0726f528766`; preview `14161062564` `9e45da2f417fa675fbc93a0346301b0a047e689268652b04678f66756470c37c`; SDK10 diagnostics `14160301448` `36f5b06f876a7d3ad836a17abff6df0076fbfb63b6cb0a13a3330226dd93d04` |
| C02 探针 `e5bf3fce3e0e410f97fde251647189aa71dc23c6` | [#1585 / 43711239891](https://github.com/Nikilua/GameSaveCenter/actions/runs/43711239891) | `132803031587` success | `132803031328` success | diagnostics `14163265966` `7834c04ea8661c451627af56d8cc25edc6605a2f5c8a38ed1184beb0732b55c7`; preview `14162659916` `d1a30d33dcbe74f07cfece913d811209421302eced13c4045c7670ff90a9bdd2`; SDK10 diagnostics `14161208170` `e89985a085b263ea75846a969cd234b89195dc42ec41593840964015f22b0ed8` |
| C03 DPI 采样 `5a6a7c192ea102338b3f339d0b3a8cbba3b86eaa` | [#1587 / 43715294494](https://github.com/Nikilua/GameSaveCenter/actions/runs/43715294494) | `132815398885` success | `132815399153` success | diagnostics `14164866130` `56fc987160cfee1d8196cd58b99a8c82702898b324e3f1365d72453d1fdc92b2`; preview `14164841285` `0dc586f7c686fd7d8ebf818c4b3f94c85bad007b0c978f76da047ad4a9d141de`; SDK10 diagnostics `14162209414` `46248bd220404de0d8c161cf6befc04a0df1db1d7ce2fc08b36c97d3e76ee232` |
| C03 页面捕获修正 `159ea800becb985e9c8a8d2a3bb8dae3b112cd48` | [#1588 / 43715603362](https://github.com/Nikilua/GameSaveCenter/actions/runs/43715603362) | `132816343814` success | `132816344085` success | diagnostics `14163943905` `3ea7d0f7f6e3394aa0133f49bd1c37169915ce58425d8a6d89afede0ca5bd448`; preview `14163684962` `1ee4dad7fbd6046b9cb4152b232816127477783c54853f56aa7ea5b334112689`; SDK10 diagnostics `14162674040` `2913be19535bcedc2b6ee8a06fd045e6f25bf99de5b75b11f88e187004cd4c68` |
| C03 控件裁切修正 `15f82b650fde8cbe370a48f8fbd6ba8978bf4dc3` | [#1589 / 43715904549](https://github.com/Nikilua/GameSaveCenter/actions/runs/43715904549) | `132817261608` success | `132817261448` success | preview `14164981791` `f1dba0374fe3804848255c9244a81f5429a232df691cc7363225ff3daf3d4d77`; diagnostics `14164643641` `1b0391dc20844ff2c1632b0b61e5b52c5defa2f4d74366302e41dae171967810`; SDK10 diagnostics `14163372919` `378429a01d56dfa7f31f49cd0b29574c687a63cd7cda1f20763d732530d5da6a` |

C01 前一提交 `54d7a6efa1d7438575ca5ca0ff29d098654e6754` 的 run #1582 因新 PowerShell composer 脚本缺 UTF-8 BOM 在源码结构检查失败，依赖步骤按 workflow 正常跳过；没有扩大 skip。BOM 已在 `aa590a1b` 修复。失败记录保留在 C01 证据目录；最终通过身份以本表的 #1583 为准。旧 required-checks 403 没有重复探测，Actions job 成功不代表已读到分支保护名单。

## 最终候选本地完整门禁

本机已缓存 SDK 8.0.423；两次 Release 门禁的源码身份均为 `15f82b650fde8cbe370a48f8fbd6ba8978bf4dc3`，该 Harness/证据提交相对产品候选 `9880c81` 的 `src/` 无差异。

C01–C03没有改生产页面，因此当前游戏选框、滚动系统、真实命令与 Binding、列表虚拟化、键盘/UIA行为及恢复安全语义未被本批变更替换。C03记录了绑定路径和控件几何；没有将它表述为键盘、UIA、真实Playnite或存档恢复验收。

| 入口 | 直接退出码 | 测试与隔离结果 | 原始摘要 |
| --- | --- | --- | --- |
| `GameSaveCenter-一键构建安装运行.cmd` → `GameSaveCenter-Run.cmd` 完整构建、测试、打包、安装及隔离启动链 | `0` | SDK `8.0.423` / Release；160份 TRX、2,051项、1,993通过、0失败、58跳过、0其他；157/157个 WPF 隔离类完成。测试数来自根链 `summary.json.testSummary.totals`，修正早期记录解析字段误把58个 skip记作0的问题；没有关闭或扩大 skip。 | `artifacts/build-diagnostics/20261115T133650175Z-15fbf887/summary.json`，SHA-256 `07FD3FC7E9BC59F2581ED0815FBFE571624BD15EB6713B6B52A316063CAF30DA`；结构化入口记录 `artifacts/round6-final-oneclick-result.json` |
| 独立 `scripts/build.cmd -Configuration Release` | `0` | SDK `8.0.423` / Release；同为160份 TRX、2,051项、1,993通过、0失败、58跳过、0其他；157/157个 WPF 隔离类完成。 | `artifacts/round6-final-build-diagnostics-20261115/20261115T141529581Z-2d1f833f/summary.json`，SHA-256 `E23100661BBDB398EFC43C86C8216154EB7BDE60F2D58B7463F084964972AB71`；结构化入口记录 `artifacts/round6-final-build-result.json` |

两组诊断各有157份独立 WPF 桌面结果；均为测试进程 exit 0、未超时、预期与实际桌面一致，输入桌面前后均为 `Default`。这是程序化测试隔离，不是用户实体键鼠或 UIA 客户端验证。独立 build 的 stdout SHA-256 为 `CF18A9959D17D397D279DE605E300ABF1E79F466092093B1D1A7F89C51C84B9A`，stderr 为空（SHA-256 `E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855`）。

## 安装身份与安全边界

中文完整链生成的 ZIP 与 PEXT 均为 45,744,349 bytes，SHA-256 均为 `076C3728C7A8733BE9237A0128AC334A0A2428424C8A1FCD9A9904D6BC53D337`。隔离安装 RunId 为 `20261115T133647720Z-6786949d`，安装报告 `artifacts/round6-final-install-report.txt` SHA-256 为 `D7865A6B36168E2867E82A96798E77F6FE04B63EA4F7A538603C5213F368856F`。六个安装程序集均与包内对应文件 hash 一致，版本信息绑定源 SHA `15f82b650fde8cbe370a48f8fbd6ba8978bf4dc3`：

| 安装程序集 | SHA-256 |
| --- | --- |
| `GameSaveCenter.Playnite.dll` | `AA370FEC02C404918D0A8BA4905AAC9C4827E70175F9CF7849CAB009B9BE4699` |
| `GameSaveCenter.Core.dll` | `AE3F5E9B968A4ADDEAA3F6AF4702765103C1E2571945E42A52C439E3B7188F26` |
| `GameSaveCenter.Contracts.dll` | `193B13ED71B1A4B8DB9AF55B48348937E9DCBBCFD18677DA1C7F39E6ED162657` |
| `Worker/GameSaveCenter.Worker.dll` | `74B3E5EEDD4300E3F041C3561CF3AE8C2F68CEDEB0BBBCC32599845BBC4796EC` |
| `Worker/GameSaveCenter.Core.dll` | `3FE50270DC3DB4AD83C0D8FC363CA863D5DD5246423D88E450DA8F197F2FB729` |
| `Worker/GameSaveCenter.Contracts.dll` | `5FA762F7932C61D2536900E38A2B246F84B9C8E023FA4B082B05B0D784640CA3` |

Playnite 只在新建的仓库 `.tmp` 隔离 profile 启动；进程退出使用 Playnite 的 `--shutdown`，退出码0。插件页面没有打开或作现场视觉验收。未读写真实用户 profile、游戏库、配置、存档或云端；隔离 Worker 的云上传开关为关闭。未发起物理鼠标、键盘或 UIA 输入。C03 的受控 WPF DPI 截图范围单列于[C03矩阵索引](../C03-dpi-matrix-20261115/README.md)。

独立 build 的第一次辅助包装器启动返回1且没有生成构建诊断；该记录 `artifacts/round6-final-build-attempt1.json` 保留。改为直接隐藏启动 `cmd.exe` 后，`scripts/build.cmd` 本身实际完成并直接 exit 0。未把首次包装器错误改写为脚本失败。原始摘要、stdout/stderr、安装报告、当前包及失败诊断保留于本机 `artifacts/`；未将生成物提交到 Git。

清理预检针对旧包备份 `.tmp/round6-final-prerun-package-20261115`：规范化路径在仓库 `.tmp/` 内，ACL owner 为当前用户 `Sult\lopmatu`，242个文件共203,299,385 bytes；根及子项均无重解析点，无Git跟踪内容、文档/脚本引用、命令行进程引用或独占打开锁。目录只含本轮生成的旧包stage、旧ZIP/PEXT和manifest；新包已在 `artifacts/` 独立校验保留。PowerShell `Remove-Item -LiteralPath -Recurse -Force` 被执行策略拒绝，目录保留；未换其他删除方式。用户TRX/TestResults和其他未知目录未动。
