# R07-07 当前 main 复核（2026-10-15）

复核源码身份：`2b42373771e950d087f393c0894380530d5eb227`（main）。本次只复跑既有行为，不改生产代码或测试源码；共享滚轮样式与 Task 响应式源在旧证据后发生过变化，因此按当前 production controls 复测。R07-07 的验收要求是连续细滚、反向快速滚动、加速边界转移、末端稳定，并记录布局更新和可见容器数据。

## 本机验证

- 使用仓库锁定 SDK `8.0.423`、VSTest `17.11.1`，测试宿主 .NET Framework `4.8.9181.0`；Playnite 目标 `net462`，测试目标 `net472`。隔离 restore 与 Playnite 测试项目及引用 Release build 成功，`0 warning / 0 error`。这不是完整 solution build，也不能代替独立 GitHub Actions lane 中的 SDK `10.0.401`。
- `scripts/check-xaml.ps1` 结构检查 `24/24`。
- 六个隔离 WPF 测试进程共 `21/21`，`0 failed / 0 skipped`：`R07FineScrollBehaviorTests 2/2`、`R07ScrollOwnershipBehaviorTests 2/2`、`MediaInboxGeometryTests 3/3`、`ResponsiveLayoutCoordinatorTests 5/5`、`TaskCenterViewResponsiveTests 8/8`、`DataGridHorizontalWheelBehaviorTests 1/1`。每类的脱敏 TRX 与 SHA-256 在 [`tests/`](tests/) 中。

## 受控滚动数据

- 生产 DataGrid 的 160 行合成数据、虚拟化内部 ScrollViewer、隔离 STA Window：12 个合成 `-30` routed wheel 样本令 offset 单调从 `0` 到 `36 DIP`，每次 `+3 DIP`；3 个 `+120` 反向样本为 `33 → 30 → 27`。
- 滚动末端 `154/154 DIP` 稳定。20 个 routed wheel 事件共记录 `17` 次 `LayoutUpdated`；末端 5 个无位移事件新增布局 `0`。观测到的可见 DataGrid 行容器为 `6–9`。
- 嵌套 ScrollViewer 的单个 `-360` 合成事件将外层从 `32` 推进到 `80/432 DIP`，内层保持 `628 DIP`；记录 `1` 次布局更新、`2` 个可见内容容器，没有越过外层末端。
- 测试已有行为断言覆盖偏移单调、反向、精确末端、可见行非零、布局预算、末端 no-op 布局增量及加速边界不越界；本次未以文本包含断言替代行为验证。数字来自当前测试 TRX，不是主观描述。

这些事件由测试构造 `MouseWheelEventArgs` 并路由到真实 WPF 控件；不是物理触控板轨迹或操作系统输入。逻辑 DIP / `LayoutUpdated` / 容器观测也不代表 Playnite 宿主帧率、呈现帧或设备性能。未启动真实 Playnite，未使用 ETW，未触碰真实存档、媒体或云端。

## 程序集身份

- Playnite 测试 DLL SHA-256：`A5D5C72BF7103505E548720EDD4C65A6DB04945A41033D276AC291D172FF000E`；PDB：`9074BFECE11AFC107AEC6BD8AB86D91C338C7E52257B7040766084742CCF02C5`。
- Playnite 插件 DLL SHA-256：`B3EF985AD75168D1BD0379EC831C5485E8EF90424465196274C5D82BF8224D51`；PDB：`BE17BC36AF81DBA10826623F9A5A274316FC7D836515C581AFB14285DFD8E81B`。
- 六个相关测试源：`R07FineScrollBehaviorTests.cs` `EBA6E609887FDD1B0231EEA387560F9E654AC62091463D3581FB549B426CE128`；`R07ScrollOwnershipBehaviorTests.cs` `481DAD73EF10D39AB5732B24B385035712D3BA591EA7A259F4A1860E9ABBAF72`；`MediaInboxGeometryTests.cs` `0BF806E6373853BA8E67EC783F7428510E64A47F75610804111C5091F5A8AF94`；`ResponsiveLayoutCoordinatorTests.cs` `0D16F4B8D8B73D7FAA2E3FB8F74F9C408FC31D3110266C144EC9DDB6B09C2C21`；`TaskCenterViewResponsiveTests.cs` `F1A481EA5264CCF82A91A58A3A3F528D076C70296D4841D37478F1501F1D12CB`；`DataGridHorizontalWheelBehaviorTests.cs` `1B026231C4721E90F091A4EF63C0A64D5A7E227B8594EFDA2280CA2B6BBA3C46`。

## CI 与下一步

该证据提交之前的 #1012/run `38640644812`（commit `2b423737`）上次观察为 `In progress`。本次公开页面读取遇到缓存未命中，匿名 API 又受本机网络代理拒绝，无法确认最新终态；不能把本机 SDK8 结果记成 CI 成功。#1010、#1011 的 SDK8/SDK10 job 失败日志仍受访问限制，诊断 ZIP 的 ID、SHA 与阻塞情况记录在当前交接中。新证据提交推送后仍须跟踪对应 Actions 到成功或失败，并在失败时先取得完整日志。

**R07-07：当前受控 WPF 验收数据已复核通过，CI 尚待确认；真实触控板、OS 输入和 Playnite 宿主仍待验。下一项：R07-08 当前 main resize 压力序列复核。**
