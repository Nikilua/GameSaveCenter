# R23-06 当前 main 安装与回退复核

日期：2026-09-26
分支：`main`
最终代码身份：`4f778e9bd7e305cc878e83671f372b6b954b32e8` (`0.6.73+4f778e9bd7e305cc878e83671f372b6b954b32e8`)
任务：R23-06「安装与回退可核查」当前身份复核

## 结论

当前 main 的 Release 候选通过隔离构建、当前身份测试、包身份与内容门禁、合成 profile 安装及回退。安装/回退只证明合成 profile 的文件级可核查性，不代表真实 Playnite 宿主启动、UIA 或最终屏幕呈现已验证。

- `scripts/build.ps1 -Configuration Release -SkipTests`：XAML structural check `24/24`，Release solution `0 errors`；保留两条既有 `MediaCenterView.xaml.cs:703 CS8602` warnings。
- 当前身份测试：Core `125/125`；Playnite 受影响隔离类 `179 passed / 40 skipped / 0 failed`（R14 classification `4/4`、R21 automation `21/21`、Workspace source `9 passed / 1 skipped`、WPF resources `137 passed / 39 skipped`、reported layout `8/8`）；Worker 非进程级 `356/356`。
- Playnite 合并筛选 testhost 超过 11 分钟无结果后停止，之后按测试类分离执行并通过。`WorkerProcessRestartTests` 未包含在本轮 Worker 非进程级命令中；它需要真实进程/Named Pipe 夹具，仍单独待环境复验，不计为通过或失败。
- bundled Python 的 `scripts/validate-source.py` 全部检查通过；WPF 静态 validator 覆盖 30 个 XAML，`0 errors / 28 warnings / 177 info`。warnings 是静态启发式提示，不等于本轮新增问题或运行时失败。
- 新生成 `GameSaveCenter-0.6.73-playnite.zip` 与 `.pext` 各 `45,482,846` bytes，SHA-256 相同：`82A615DA72E55527266E9CA7A7B1A67A5926472485FEC0E57DD234F8F6B24DF1`。包内容门禁通过，manifest 为 `0.6.73`；插件、共享库和 Worker 所需文件齐备，Worker 为 self-contained 发布。
- 六份插件/Worker 主程序集及共享程序集的 `ProductVersion` 均为 `0.6.73+4f778e9bd7e305cc878e83671f372b6b954b32e8`。

## 合成 profile 安装与回退

所有暂存、构建和合成 profile 均限定在仓库 `.tmp/r23-06-current-4f778e9b/` 下，真实用户 Playnite profile 与真实 Extensions 目录没有写入。

1. 将原根级发布包及已 stage 扩展备份到审计临时目录；旧 zip SHA-256 为 `9A585AA75D49C733DBEFCD6C31D7CB444E236E6B8835FEF8FD0A0A3E1D27027A`，旧候选 identity 为 `0.6.73+74159b1a4aee3bebfe53d3533afcad5bda9ed7c8`。
2. `package.ps1` 输出当前 main zip/pext 与 stage；在新建 synthetic profile 上运行 `install-dev.ps1 -PlayniteExtensionsPath`，核对 manifest、六份 DLL identity 与 Worker hostfxr/hostpolicy/coreclr、`System.Private.CoreLib`、runtimeconfig 等文件。
3. 在确认 Playnite/Worker 进程均未运行且目标仍在审计 `.tmp` 下之后，将当前候选留存在临时审计目录并恢复旧 stage。恢复后六份 DLL 均与备份逐文件 SHA-256 一致，ProductVersion 回到 `0.6.73+74159b1a4aee3bebfe53d3533afcad5bda9ed7c8`，manifest 为 `0.6.73`。

## 边界与后续

- 未启动真实 Playnite；未触碰真实用户 profile、存档、媒体或云端；未声称真实宿主截图、UIA/读屏、物理 DPI/跨屏、presented frame、ETW 或宿主性能通过。
- R23-04 仍受已记录的 CEF `platform_channel` 拒绝访问 `0x5` 阻挡；系统状态未变化前不重复相同宿主尝试，也不绕过权限。
- 审计临时目录在证据收录后清理；仓库 `artifacts/` 根级当前包与 stage 保留为最新可再生交付物，不提交到 Git。
- R 总基线仍为 192 项唯一任务；本次只更新 R23-06 当前身份的可追溯证据，不改任务统计。
- 后续按 R23-08 准入清单选择依赖已满足的 Q/R 小批量；R23-04、R23-05 真实宿主/呈现以及 R02-06 Playnite 菜单等外部边界保持明确待验。
