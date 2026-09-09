# L30 候选安装包与升级/回退清单

## 候选包

- 源码公共版本：`0.6.73`，没有为开发阶段递增版本号。
- 构建提交：`fa9af0a`。
- 完整程序集身份：`0.6.73+fa9af0ad36db07551bd1c2985258d72eecb9c8e4`。插件、Worker、Core、Contracts 的六份程序集身份由 `scripts/package.ps1` 逐一校验并一致；本包包含滚动锚点、浅色主题背景/云端筛选文字修复、首页活动内容和云端整卡导航修复，以及实际 WPF 点击行为测试。
- Worker 发布：`win-x64`、self-contained；包内存在 `GameSaveCenter.Worker.exe/.dll`、`runtimeconfig.json`、`hostfxr.dll`、`hostpolicy.dll`、`coreclr.dll` 和 `includedFrameworks` 标记。
- manifest：`src/GameSaveCenter.Playnite/extension.yaml` 的 `Id` 为 `66e9f2d7-67bb-43ef-b62a-b8e60734fcec`，`Version` 为 `0.6.73`，`Module` 为 `GameSaveCenter.Playnite.dll`。

候选文件：

- [GameSaveCenter-0.6.73.pext](../../artifacts/GameSaveCenter-0.6.73.pext)
- [GameSaveCenter-0.6.73-playnite.zip](../../artifacts/GameSaveCenter-0.6.73-playnite.zip)

两个文件大小均为 `43,837,868` 字节，SHA-256 均为：

`631615AB7695C46F943D9546A53369C69ADA49F347C4F7CE33D96A33C3831249`

包脚本完成了 Release 构建、Core `76/76`、Worker `310/311`（1 skip）、Playnite `427/490`（63 skip）测试、Worker `win-x64` 发布、必需文件检查、manifest 版本检查、程序集身份校验和 self-contained 检查。没有安装到真实 Playnite。

## 数据库兼容边界

- `SqliteStateStore.InitializeAsync` 对旧库执行幂等增量列/表初始化；`backup_versions` 主键结构变化时会在事务内重建表并保留已有数据。迁移夹具覆盖较旧列集合、重复初始化和无效旧结构失败报告。
- 本项目没有通用的“降级迁移”或把新库自动变回旧 schema 的承诺。新包启动并完成迁移后，不能把旧 Worker/旧插件直接指向同一份已升级状态库并声称兼容。
- L30 针对 `DatabaseMigrationHarnessTests` 与 `CloudRetryPersistenceTests` 的升级/重启相关测试为 `14/14`；测试只操作隔离临时目录。

## 升级步骤

1. 关闭 Playnite，并确认本插件持有的 Worker 已退出；不要停止或删除其他插件的 Worker。
2. 复制完整插件数据目录/状态库和插件设置到隔离备份目录，保留备份时间与当前包 SHA-256。
3. 在隔离 Playnite 或明确的扩展目录安装 `.pext`，启动后核对插件 manifest、插件程序集身份和 Worker 握手身份均为同一版本/提交。
4. 先读取状态、迁移检查和分页/刷新等只读路径，再进行需要写入的操作；异常时保留原目录和日志，不覆盖备份。

## 回退步骤

1. 停止 Playnite 和本插件 Worker，保留失败后的状态目录副本用于诊断。
2. 将升级前的完整配置/状态库备份恢复到原隔离目录；不要只恢复单个 SQLite 文件而遗漏配套设置或目录。
3. 安装原先的旧 `.pext`，先核对旧包的 manifest、程序集和 Worker 身份，再启动旧版本。
4. 如果没有升级前副本，不执行“旧包直接打开新库”的自动回退；应导出诊断并人工决定是否从备份恢复。任何迁移失败都应显示为失败，不把空库当成成功回退。

## 验证边界

这是候选包与隔离迁移证据，不是真实 Playnite 安装证据。真实 Playnite 加载、FusionX/用户主题、DPI、Worker 进程回收和视频操作仍属于 L31 宿主矩阵。
