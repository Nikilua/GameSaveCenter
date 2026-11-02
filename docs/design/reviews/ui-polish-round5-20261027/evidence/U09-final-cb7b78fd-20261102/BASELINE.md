# U09-05 / U09-08 当前候选最终门禁

日期：2026-11-02。生产候选提交 `cb7b78fd514e7d6eaa2a34e341340d5d06610508`（`修正列表选中框内容偏移`）。该候选已先提交并推送；旧冻结候选 `5c0c57b0` 的根入口结果仅保留历史，不替代当前代码。U09-05 与 U09-08 暂重新标记进行中，等待本候选完整门禁和状态复核。

## 实际入口与冻结规则

- `GameSaveCenter-一键构建安装运行.cmd` 实际存在，只调用同目录 `GameSaveCenter-Run.cmd` 并传递退出码；后者运行 `scripts/dev-install-run.ps1 -Configuration Release`，执行SDK基线检查、完整构建与测试、发布/打包、安装身份核验和Playnite启动。通过 `< nul` 禁止包装器的结尾 pause 阻塞，不改入口实现、不添加别名。
- `scripts/build.cmd` 独立存在，调用 `scripts/build.ps1` 且不传 `-SkipTests`，按其Release完整解决方案构建/测试职责单独验证；不替代安装启动链。
- 当前生产源码候选SHA为 `cb7b78fd...`。最终文档冻结提交SHA、完整链日志、TRX、包/安装身份、SDK与退出码将在本文件续记。候选源码未经变更；纯文档证据提交不代表生产源码变化。

## 已完成的候选局部检查

- SDK 8.0.423 Release解决方案编译：0 warning / 0 error；`R10RecentAccessBehaviorTests`：3/3，0失败、0 skip。
- Source Validation通过；WPF静态审查218 XAML / 0 errors；双主题 Render QA 精确cb7 SHA为394 PNG / 0 `PROBLEM`。
- 选中行图标位置回归及成对图证据见[U06-02批次](../U06-02-selection-overlay-20261102/BASELINE.md)。

## GitHub 当前候选状态（记录时）

run [41392841712](https://github.com/Nikilua/GameSaveCenter/actions/runs/41392841712)（#1425）对应精确head `cb7b78fd514e7d6eaa2a34e341340d5d06610508`，`windows-build.yml` 两个适用job最终均completed/success且全部步骤完成：SDK8基线 `125107766820`，SDK10.0.401 `125107767536`。Preview artifact `13120801010`（90,791,456 bytes，`sha256:dffc823d7dd43f9ea543d8219d62045b46ca3e23662308af2d0e6ad031d0aafe`）、基线诊断`13119839919`（2,031,384 bytes，`sha256:f0f18a937adfa1c3bd399c1c50191d3e695ba1a10b03bcef4df77ec6dfcd1209`）、SDK10诊断`13118807409`（971,692 bytes，`sha256:7ab7284a14834d66b72c9442d799657584db71ba363c39c24a0fec307ef9aecc`）均精确绑定该SHA。活动job时日志接口曾返回GitHub `BlobNotFound`；待job结束后通过步骤与artifact重新核实终态，未重跑workflow。具体身份见 [`ci-run-41392841712.json`](ci-run-41392841712.json)。

## 空间与本阶段输出

阶段开始快照见[`stage-start.json`](stage-start.json)。本地WMI磁盘枚举被系统拒绝访问；改用.NET `DriveInfo` 成功记录。起始C盘空闲4,986,994,688 bytes、D盘15,580,450,816 bytes。包装器默认将输出与测试TEMP放在C盘，配置预留输出2 GiB、测试TEMP 1 GiB、包1 GiB及系统TEMP 1 GiB，默认环境需C盘5 GiB，高于起始空闲约381,714,432 bytes。已确认仓库 `.tmp` 实际位于C盘，不能降低C盘预算；`D:\Temp` 是已存在、非重解析点目录且有足够空间。执行前将申请创建独立 `D:\Temp\gsc-r5-u09-final-90196aa2` 并仅为本链设置 `TEMP`/`TMP`，保持原预算、SDK和门禁。阶段开始时保留既有 `artifacts/`（7,132文件/1,678,403,921 bytes）、`.tmp/`（14,139文件/1,069,404,099 bytes）、旧安装报告/日志和已有诊断，不将其误认成本阶段输出或清理。初始相关Playnite进程为0。

完整根链、独立构建、安装身份、日志/TRX身份、阶段结束空间与仅本阶段输出的安全清理结论将在运行结束后补记。未执行真实存档恢复、媒体删除或云写入。真实Playnite窗口可视内容、外部UIA/实体输入/IME、物理DPI需继续保留验收边界。
