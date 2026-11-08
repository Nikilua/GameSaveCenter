# Round5 U09-05：完整本地构建与交互桌面窗口监测

## 结果

在精确源码提交 `92bd9de13bd40c13806fb0cba8900ada2e379fc2` 上运行独立入口 `scripts/build.cmd`，SDK `8.0.423`，退出码 `0`。TRX 汇总为 2030 项：1962 通过、0 失败、68 跳过；157 个 WPF 测试类均隔离进程运行。跳过项按测试自身既有说明保留，不计为通过：57 项针对已撤销的“今日工作台”结构，10 项受当前 Named Pipe 权限限制，1 项会移动系统鼠标指针。

运行开始于 `2026-11-08T07:49:30.8814879Z`，结束于 `2026-11-08T08:26:34.8312303Z`。WinEvent 监听线程明确绑定 `WinSta0\Default`，即 input desktop `Default`。整个构建/测试区间记录到 **0 个顶层窗口显示或隐藏事件**，匹配窗口事件列表为空。构建在 D 盘仓库隔离输出目录中执行；未启动中文安装运行入口、GameSaveCenter 或 Playnite，因此该证据说明 `scripts/build.cmd` 这次没有造成桌面闪窗，但不能据此判定此前截图的黑窗根因，也不代表安装运行入口已通过。

运行期间只读进程快照仍发现旧 Playnite PID `23488` 与 Worker PID `30516`，两者无主窗口句柄；本轮没有结束或重复启动它们。没有更改卷、盘符或映射。

## 可复核证据

- `run-summary.json`：精确提交、SDK、退出码、TRX 汇总、skip 原因、桌面监测与磁盘空间记录。
- `window-monitor-and-build.json`：原始监听和构建运行身份，SHA-256 `04154750660F17296FD2E076A3AF9B5AFD405D890622194C578343FA5312CDAB`。
- `trx-manifest.csv`：160 个原始 TRX 的相对路径、大小和 SHA-256，清单 SHA-256 `379BC12F3EE4D734720BA586AED9153B83BE4D58B5149D629D84BC7824E8545E`。
- `r08-motion-reverse.trx`：R08 按压/悬停反转行为类的完整构建 TRX，SHA-256 `541784B55326FBF9037B229FEFAD3B0DA2D4BF8D35A435797E6C13071F90550F`。
- `build-console.txt`：入口摘要，SHA-256 `588031E7EB77D795BBABB649DD71153682A16B5BBDA189178D2DE5DBEA10B359`。
- 原始完整日志及160个TRX保留在本机 `artifacts/build-diagnostics/20261108T074929337Z-a6ae1e37/`；`summary.json` SHA-256 为 `1E460C09BD4DE0271D1F6F638842333CF41E3FF5D004A49836BDFAC9AE64E775`。
- [`cleanup.json`](cleanup.json)：证据副本校验后，仅清理本阶段 `.tmp/r5-r08-final-full-20261108/`（728个文件、169,300,382 bytes），诊断/TRX和证据目录保留。D盘空闲实测增加171,266,048 bytes，C盘空闲不变；未改卷、盘符、映射或其他任务 TestResults。

本证据不代替中文安装运行入口的安装身份及宿主启动验收。当前不把用户黑窗报告标记为已解决。

## GitHub CI #1490

精确同一 SHA 的 `.github/workflows/windows-build.yml` run [#1490](https://github.com/Nikilua/GameSaveCenter/actions/runs/42459902671) 于 `2026-11-08T08:49:09Z` 完成 `success`。SDK10.0.401 job `128666747381` 的8步和基线 job `128666747608` 的15步全部成功；基线 job 包含 Release 编译/测试、Render QA、WPF 静态审查、扩展打包和上传。

三个 run artifact 均绑定 SHA `92bd9de13bd40c13806fb0cba8900ada2e379fc2`：预览包 `13610836964`，90,793,668 bytes，SHA-256 `162f7b7955c8b0a2ef7c87425329afa13241c4f821247ab8881ae2e51de2e4bf`；基线诊断 `13610448945`，2,524,028 bytes，SHA-256 `89ccc21dd9929d63fdf8da5a10e6a757f41c6ad2e08d0c9e368f53bb3d82131e`；SDK10诊断 `13610475659`，1,278,945 bytes，SHA-256 `4876f23877623874671e8f2a16221d1f8302350b3d84a592832305dce2d1d196`。job、artifact和required-check读取边界见 [`ci-run-1490.json`](ci-run-1490.json)。

工作流 jobs 和产物已通过，分支保护的 required-checks 接口此前返回403，因此这一独立状态仍标记为“远端待验”；需维护者提供只读读取权限或当前 required-checks 列表后再核对。

本阶段临时构建输出已完成归档核对并清理；原始160份TRX及诊断日志仍留在 `artifacts/build-diagnostics/20261108T074929337Z-a6ae1e37/`，清理记录包含精确D/C盘前后空间和唯一删除路径。
