# R01-01 测试源码根绑定证据

日期：2026-09-16；当前提交复核：2026-09-18
分支：`codex/ui-finesse-round2`  
实现提交：`abb5589acee4ee1bca99ae2e483670d81617904e`；本批测试基础设施修复：`a5219c0906c6302752c026f734900f7cdbdf770b`

## 对照基线

质量审查 F10 记录过一次真实错根：隔离构建输出放在 main 仓库 `.tmp` 时，部分 Playnite 源码测试从 `AppContext.BaseDirectory` 向上找到 main 的 `.sln`，导致新程序集配旧源码形成无效比较。R01-01 要求源码根与程序集构建身份绑定，隔离输出不能悄悄读另一个 checkout，错根要有清晰诊断。

## 实现

- 测试项目嵌入 `AssemblyMetadata`：`GscSourceRoot` 和 `GscBuildCommit`；`GscSourceRoot` 默认来自测试项目所属 checkout，构建脚本显式传播当前源根。
- `TestRepositoryContext.Root` 不再从测试输出目录回溯；它校验元数据根包含 `GameSaveCenter.sln` 和 Playnite 源码目录，读取该根的 Git HEAD，并要求程序集 commit 与源码 HEAD 相等或为安全的短 SHA 前缀。
- 缺失元数据、未知 commit、无效源码根和 checkout/程序集不一致均抛出包含程序集路径、源码根和两端 commit 的可诊断异常；所有 37 个重复 `FindRepositoryRoot` 及 6 个直接回溯的源码 reader 均转向该 helper。
- `scripts/build.ps1` 设置并恢复 `GSC_SOURCE_ROOT`；没有改变生产 UI、命令/绑定、取消/错误、恢复保护、滚动或 `net462` 业务契约。

## 当前提交补充

- 首次使用旧默认 `bin\Release` 的定向复跑被身份门禁正确拒绝：程序集仍为 `447ac07e`，源码根已是当前 checkout；这不是产品回归，也没有绕过校验。
- 在当前隔离构建中，身份筛查进一步发现 `R06EmptyStateBehaviorTests.cs` 仍有一个独立的 `FindRepositoryRoot()` 从 `Environment.CurrentDirectory` 回溯 `.sln`。`a5219c09` 将其收敛到 `TestRepositoryContext.Root`；没有改生产代码或空表业务语义。
- 修复后使用当前提交新建 `.tmp\r01-01-02-build-clean-a5219c09`，重新编译 solution 与 RenderHarness 均为 `0 warning / 0 error`；XAML 结构校验 `24/24`。

## 行为验证

1. 当前 checkout 的身份/代表性源码测试：当前提交 Release 构建 `0 warning / 0 error`；`RepositoryIdentityTests`、`UiFinesseFoundationTests`、`UiAuditSourceTests` 定向 `16/16`，其中包含错根扫描和真实 WPF 代表性行为。
2. 当前 worktree 隔离输出：`scripts/build.ps1 -Configuration Release -OutputRoot .tmp\r01-01-isolated`；XAML 结构校验 24 个文件通过，解决方案构建 `0/0`，Core `83/83`，Worker `311/311`，Playnite `518` 通过、`57` 跳过、`0` 失败。该目录已在关闭构建服务器后删除。
3. 跨 checkout 输出复跑：同一当前 worktree 源码将输出放到 `D:\workplace\github\GameSaveCenter\.tmp\r01-01-cross-checkout-20260916`（main checkout 的临时目录），结果仍为构建 `0/0`、Core `83/83`、Worker `311/311`、Playnite `518` 通过、`57` 跳过、`0` 失败；源码 reader 未回读 main 源码。该目录已删除，main 源码和用户 `src.zip` 未修改。
4. 首次定向测试使用 7 位 `GscBuildCommit` 时，严格全字符串比较按预期报告程序集/源码身份不一致；随后改为支持 7～40 位安全前缀匹配并重跑 `14/14`，保留了不一致时的阻断。
5. `a5219c09` 修复后的干净隔离输出再次报告程序集 commit=`a5219c09...`、源码根为当前 worktree、`WorkingTreeClean=True`；R01-01 当前可控条件已满足。

## 边界

- 证据来自隔离构建、真实 .NET testhost、源码元数据和 Git HEAD；不涉及 Playnite 宿主启动、真实存档/媒体/云端或对外诊断发送。
- 跨 checkout 验证只在 main 的 `.tmp` 写入可再生构建输出，未覆盖或合并 main 源码；输出已清理。`57` 个 skip 独立记录，不能当作通过。
- 这是源码测试基础设施的 checkout 身份门禁，不代表真实 Playnite UI、物理 DPI、IME、屏幕呈现帧或宿主性能已验。

## 下一步

R01-01 代码与隔离/跨 checkout 证据已完成，当前补充修复后的身份测试也已通过；下一可执行小批量为 R01-03“每项证据直达”，核对共享索引、报告入口和断链/错链负例。
