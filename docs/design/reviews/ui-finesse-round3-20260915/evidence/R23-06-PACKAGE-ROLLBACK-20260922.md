# R23-06 安装与回退可核查

日期：2026-09-22  
分支：`codex/ui-finesse-round2`  
最终代码提交：`102b74b08e0f04f2b334e3d75ecec161fd13103a`（当前分支文档收口；候选代码改动来自 `130ba48a`）
任务：R23-06「安装与回退可核查」

## 结论

当前 Release 候选已完成隔离编译、Core/Worker 测试、发布包身份校验、合成 profile 安装和回退演练；Playnite WPF 隔离首次在动效类出现一次时序失败，精确复跑 `9/9` 通过，最后 4 个未执行类为 `27 passed / 11 skipped`；没有使用真实存档执行恢复，也没有触碰真实 Playnite Extensions。

- 隔离门禁结果：XAML `24/24`、Core `125/125`、Worker `355/355`、Playnite source `111` 类；Release 编译 `0 errors`，保留既有 `MediaCenterView.xaml.cs:699` 两条 `CS8602` warning。WPF 类隔离首次跑到 `98/101` 时由 `UiFinesseFoundationTests.EntranceMotionReentryKeepsTheCurrentDispatcherValue` 抖动退出，单类复跑 `9/9`，剩余 UI 类 `27 passed / 11 skipped`，未改弱测试。
- 六份程序集构建身份一致：`0.6.73+102b74b08e0f04f2b334e3d75ecec161fd13103a`。
- `GameSaveCenter-0.6.73-playnite.zip` 与 `.pext` 均生成并由 `package.ps1` 内容门禁通过；两者 SHA-256 相同：`C84C7CAB59CC44845DF1FF1629FA6544A19B410EF444D672F4407E252D30117F`，大小均为 `44,060,877` bytes。
- 当前可审阅包保留在 `D:\gsc-r23-06-package-20260922`；仓库不提交二进制产物。
- 当前候选实际安装到新的 D: 合成 profile 后，清单为 `0.6.73`、DLL `ProductVersion=0.6.73+102b74b0…`、必需插件/Worker 文件齐全；随后恢复旧 synthetic 候选 `0.6.73+9026f4a2…`，回退身份复核成功。

## 发现并修复的隔离构建门禁问题

首次将 package 输出放在 D 盘时，Worker 源码测试中的 8 个失败是因为测试程序集仍从 `AppContext.BaseDirectory` 向上寻找 `GameSaveCenter.sln`，外置输出自然无法找到仓库；当时结果为 `347/355`，不是业务断言失败。

本批在 Worker 测试项目复用 Playnite 已有的身份绑定方式：

- `GameSaveCenter.Worker.Tests.csproj` 写入 `GscSourceRoot` 与 `GscBuildCommit` 程序集元数据。
- 新增 `TestRepositoryContext`，验证源码根存在、程序集 commit 与源码 HEAD 一致，再提供源测试读取路径。
- 5 个旧的目录向上寻根方法改为复用该上下文；没有改 Worker 业务、IPC、DTO 或数据存储。

修复提交后，D 盘外置输出完整 package 门禁最终取得 Worker `355/355`，证明安装/发布候选可以脱离仓库 `bin/obj` 目录复现。

## 六份程序集身份

`package.ps1` 的 ECMA-335 元数据读取对以下六份文件逐一校验，实际输出均为同一身份：

| 类别 | 文件 |
| --- | --- |
| 插件 | `GameSaveCenter.Playnite.dll` |
| Worker | `Worker/GameSaveCenter.Worker.dll` |
| 插件共享 | `GameSaveCenter.Contracts.dll`、`GameSaveCenter.Core.dll` |
| Worker 共享 | `Worker/GameSaveCenter.Contracts.dll`、`Worker/GameSaveCenter.Core.dll` |

包内容门禁同时确认 `extension.yaml`、插件 DLL、共享 DLL、Worker DLL 和 `Worker/GameSaveCenter.Worker.runtimeconfig.json` 存在，并确认 Worker self-contained runtime 内容和清单版本 `0.6.73`。

## 合成 profile 安装与回退

本次演练范围：

- Playnite profile：`D:\gsc-r23-06-synthetic-profile-20260922`。
- 安装目标：该 profile 的 `Extensions/GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`。
- 旧版本备份：`D:\gsc-r23-06-rollback-backup-20260922`，安装前读取到 `0.6.73+9026f4a2812c5c534a46abb1bf79aed7afd7988d`。

实际顺序：

1. 备份隔离 profile 中上一轮扩展目录，并核对清单 `0.6.73`、DLL `0.6.73.0` 和旧 ProductVersion。
2. 用 `scripts/install-dev.ps1 -PlayniteExtensionsPath` 安装当前 `102b74b0` 候选；核对清单版本、文件版本、ProductVersion 和 Worker 文件存在。
3. 在无 Playnite/Worker 进程的前提下，仅删除该隔离目标目录并复制已核验备份；回退后 ProductVersion 恢复为 `0.6.73+9026f4a2…`，清单仍为 `0.6.73`。

安装器的暂存目录/移动流程和版本校验由现有脚本执行；回退演练只操作明确的合成 profile 目标，未调用真实恢复命令、未删除真实媒体或写用户云端。

## 边界

- 本批证明的是发布候选身份、包内容、隔离安装和目录级回退；不证明真实 Playnite 当前屏幕呈现、UIA/读屏、物理 DPI/跨屏、ETW、宿主帧性能或真实存档恢复。
- R23-04 的当前真实嵌入宿主截图绑定的是 `9026f4a2`，不能把本批 `102b74b0` 的包安装事实扩大成当前候选已完成真实宿主复测；R23-04 runner 的 UIA/summary 边界仍保留。
- Demo 原目录不可用，继续使用恢复生产基线；游戏选框、滚动条系统、命令绑定、取消/错误语义、恢复保护和有限列表性能未被本批改变。

下一可执行任务：回到 R23-04 runner 的 UIA/Controlled host 收口；R23-05 几何已由 `130ba48a` clean `shellqa` 通过，真实 presented frame/宿主性能仍按权限边界待验。
