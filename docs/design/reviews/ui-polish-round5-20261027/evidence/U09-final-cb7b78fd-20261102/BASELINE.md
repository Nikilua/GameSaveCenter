# U09-05 / U09-08 当前候选最终门禁

日期：2026-11-02。生产源码提交 `cb7b78fd514e7d6eaa2a34e341340d5d06610508`（`修正列表选中框内容偏移`）已推送。最终文档冻结候选为其文档后继 `0b61ee51ff0d78bf2f0f7c428cf358721753e558`；两提交间没有源码、测试、脚本或资源差异。旧冻结候选 `5c0c57b0` 的门禁只作为历史记录。U09-05 与 U09-08 已满足本轮验收要求。

## 实际入口与冻结规则

- `GameSaveCenter-一键构建安装运行.cmd` 实际存在，只调用同目录 `GameSaveCenter-Run.cmd` 并传递退出码；后者运行 `scripts/dev-install-run.ps1 -Configuration Release`，执行SDK基线检查、完整构建与测试、发布/打包、安装身份核验和Playnite启动。通过 `< nul` 禁止包装器的结尾 pause 阻塞，不改入口实现、不添加别名。
- `scripts/build.cmd` 独立存在，调用 `scripts/build.ps1` 且不传 `-SkipTests`，按其Release完整解决方案构建/测试职责单独验证；不替代安装启动链。
- 生产源码SHA为 `cb7b78fd...`，本地完整链以文档冻结候选 `0b61ee51...` 注入构建身份。完整日志、158份TRX哈希清单、包/安装身份、SDK与退出码见[`local-gates.json`](local-gates.json)。

## 已完成的候选局部检查

- SDK 8.0.423 Release解决方案编译：0 warning / 0 error；`R10RecentAccessBehaviorTests`：3/3，0失败、0 skip。
- Source Validation通过；WPF静态审查218 XAML / 0 errors；双主题 Render QA 精确cb7 SHA为394 PNG / 0 `PROBLEM`。
- 选中行图标位置回归及成对图证据见[U06-02批次](../U06-02-selection-overlay-20261102/BASELINE.md)。

## 生产源码候选 GitHub CI

run [41392841712](https://github.com/Nikilua/GameSaveCenter/actions/runs/41392841712)（#1425）对应精确head `cb7b78fd514e7d6eaa2a34e341340d5d06610508`，`windows-build.yml` 两个适用job最终均completed/success且全部步骤完成：SDK8基线 `125107766820`，SDK10.0.401 `125107767536`。Preview artifact `13120801010`（90,791,456 bytes，`sha256:dffc823d7dd43f9ea543d8219d62045b46ca3e23662308af2d0e6ad031d0aafe`）、基线诊断`13119839919`（2,031,384 bytes，`sha256:f0f18a937adfa1c3bd399c1c50191d3e695ba1a10b03bcef4df77ec6dfcd1209`）、SDK10诊断`13118807409`（971,692 bytes，`sha256:7ab7284a14834d66b72c9442d799657584db71ba363c39c24a0fec307ef9aecc`）均精确绑定该SHA。活动job时日志接口曾返回GitHub `BlobNotFound`；待job结束后通过步骤与artifact重新核实终态，未重跑workflow。具体身份见 [`ci-run-41392841712.json`](ci-run-41392841712.json)。

## 最终文档冻结候选完整链与独立构建

冻结提交 `0b61ee51ff0d78bf2f0f7c428cf358721753e558` 上，实际存在的中文入口 `GameSaveCenter-一键构建安装运行.cmd` 经 `GameSaveCenter-Run.cmd` 执行发布、测试、打包、安装身份核验和Playnite启动，退出码0；没有创建或改名入口。随后按其自身职责运行 `scripts/build.cmd -OutputRoot artifacts\gsc-b\1c0856c2`，退出码0，不以它替代完整链。两入口均使用仓库固定SDK 8.0.423、Release，分别有158份TRX；每次总计2025项，1967通过、0失败、58条既有skip，skip不计通过；Core 157/157、Worker 401/401，155个WPF隔离类全部完成。两个构建诊断ID、控制台日志SHA及逐份TRX SHA在[`local-gates.json`](local-gates.json)。

安装清单为0.6.73，程序集身份 `0.6.73+0b61ee51ff0d78bf2f0f7c428cf358721753e558`。ZIP与PEXT均为45,737,895 bytes、SHA-256 `CC3F08165F76AE32CFB48E8EC4A8A4930B2F59B10D1D69021420A5614055D82C`。安装目录为 `%APPDATA%\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`；报告确认磁盘程序集身份，Playnite进程 `D:\software\Playnite\Playnite.DesktopApp.exe` 已启动。此链验证安装和进程启动，不表示已人工观察真实插件页面或完成实体输入/UIA/DPI验收。

run [41403537450](https://github.com/Nikilua/GameSaveCenter/actions/runs/41403537450)（#1427）与文档冻结SHA精确绑定；SDK基线job `125140372775`（15步）和SDK 10.0.401 job `125140372862`（8步）均completed/success。preview、基线诊断、SDK10诊断artifact分别为 `13124907150`、`13124607962`、`13124035463`，digest及SHA绑定见[`ci-run-41403537450.json`](ci-run-41403537450.json)。本地包、安装记录和完整根链已与精确SHA对应；required-check名单仍沿用U09-06既有403权限限制，记作“远端待验”，未重复请求。纯文档最终记录不再触发本机完整链。

## 空间与本阶段输出

阶段开始快照见[`stage-start.json`](stage-start.json)。本地WMI磁盘枚举被系统拒绝访问；改用.NET `DriveInfo` 成功记录。起始C盘空闲4,986,994,688 bytes、D盘15,580,450,816 bytes。包装器默认将输出与测试TEMP放在C盘，配置预留输出2 GiB、测试TEMP 1 GiB、包1 GiB及系统TEMP 1 GiB，默认环境需C盘5 GiB，高于起始空闲约381,714,432 bytes。已确认仓库 `.tmp` 实际位于C盘，不能降低C盘预算；`D:\Temp` 是已存在、非重解析点目录且有足够空间。本次使用独立目录 `D:\Temp\gsc-r5-u09-final-90196aa2` 仅承载TEMP/TMP，原预算、SDK与门禁保持不变。阶段开始时保留既有 `artifacts/`（7,132文件/1,678,403,921 bytes）、`.tmp/`（14,139文件/1,069,404,099 bytes）、旧安装报告/日志和已有诊断，不将其误认成本阶段输出或清理。初始相关Playnite进程为0。

完整链结束前，仅清理本阶段自有外置临时目录 `D:\Temp\gsc-r5-u09-final-90196aa2`：先核对父目录、规范绝对路径、重解析点、259个文件独占读取、进程及文档链接，再用PowerShell `Remove-Item -LiteralPath -Recurse -Force` 删除；共移除208,671,025 bytes。C盘清理前后空闲4,586,995,712→4,586,995,712 bytes，D盘13,725,048,832→13,934,358,528 bytes。清理详情及保留的最新包/诊断输出见[`cleanup.json`](cleanup.json)；既有`.tmp/`和其它历史输出未删。未执行真实存档恢复、媒体删除或云写入；真实插件页面、外部UIA、实体输入/IME、物理DPI仍待验。
