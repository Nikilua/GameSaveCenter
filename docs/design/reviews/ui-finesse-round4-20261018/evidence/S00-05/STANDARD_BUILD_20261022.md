# S00-05 标准根命令全链验收

日期：2026-10-22

## 验收输入

- 源码提交：`5974742f4bea03d4975ff3a447eabe13d52a0e0e`（`main`）。
- 命令：仓库根目录 `cmd.exe /d /c "GameSaveCenter-Run.cmd < NUL"`。
- SDK：`8.0.423`；Configuration：`Release`；Windows x64。
- 最终诊断：`artifacts/build-diagnostics/20261022T012309056Z-3ccfe8e5/summary.json`，`outcome=passed`，完整运行时段 `2026-10-22 01:23:09Z` 至 `02:23:23Z`。

## 结果

- XAML `25/25`；Release solution build `0 warning / 0 error`。
- Core `157/157`，Worker `400/400`，Playnite source `567` 项（`549` pass、`18` skip、`0` fail）。
- WPF 隔离测试 `140/140` 类结束，`782` 项（`743` pass、`39` skip、`0` fail）。总计 `1,906` 项（`1,849` pass、`57` skip、`0` fail/error/timeout/abort）；143 份 TRX 计数一致，`counterMismatchCount=0`。
- `CommandDirectoryBehaviorTests` `3/3`、`CommandDirectoryKeyboardBehaviorTests` `7/7`；R08 反向动效 `3/3`、R09 焦点轮廓 `2/2`。
- Worker 发布、程序集构建身份校验、ZIP/PEXT 打包、Playnite 扩展安装核验和 Playnite 进程启动均通过。清单 `0.6.73`、插件 DLL `0.6.73.0`。
- Release 插件 DLL SHA-256：`FE7CAF678FC294A05A054412CBCE2C588A39C2F07D10FCA58EF2EB015469DE7F`；已安装 DLL SHA-256 相同。ZIP 与 PEXT SHA-256 均为 `2B8ECC49574B1DE4744EA4880447F8FD249A4544377B7C1BB87958DB2831E7A1`。R09 TRX SHA-256：`18B713C98A0ED4B68A3594AB59AE3764C91BC74F532B2F77299CF2FFDE1BD13C`。

## 首次全链运行与复核

- 同一源码提交的首次完整尝试诊断为 `20261022T004049092Z-839ad6f9`，在 WPF `R08MotionReverseBehaviorTests` 隔离进程结束等待超时，根命令未进入打包/安装。该类已完成测试没有失败结果，TRX 显示首项通过、第二项未完成；测试宿主挂起并生成约 `634.8 MB` hang dump。不能把这次 run 记为通过。
- 使用首次构建的同一测试程序集独立复跑该类两次，各为 `3/3`、退出码 `0`；随后从根目录重跑完整标准命令，最终诊断如上，R08 与其余 `139` 个隔离类全部正常结束。首次挂起保留作诊断，不以复跑结果覆盖原始记录。
- 最终摘要 `workingTreeClean=false` 仅列出此前已存在的 Round3 未跟踪 TRX 与 `tests/GameSaveCenter.Playnite.Tests/TestResults/`；这些用户文件没有修改或暂存。

## 验收边界

根命令已验证编译、受控测试、打包、扩展安装和 Playnite 进程启动。没有把进程启动表述为真实页面交互/视觉验收；物理键盘、真实 IME 候选和当前桌面 UI 呈现仍待宿主操作验证。
