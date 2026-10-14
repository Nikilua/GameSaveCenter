# R12-03 目标路径核对：当前 main 行为复核

日期：2026-10-15

生产能力已在 main 的 `c0adb1b7` 实现。本次没有重建路径服务或改生产 XAML；只给现有 `PathRemapService` 与 `MaintenanceView` 增加当前主线实际行为证据。代码测试与本文、TRX 一起提交在本阶段提交中；测试运行时生产基线为 `750349f3`。

## 本机验证

- SDK `9.0.302`，VSTest `17.14.1`；Playnite 插件目标 `net462`，测试程序集 `net472`，testhost 为 .NET Framework `4.8.9181.0`。Worker 目标 `net8.0-windows10.0.19041.0`，测试运行时 `.NET 8.0.18`。
- Playnite `R12PathRemapBehaviorTests`：`3/3`。测试实际创建生产 `MaintenanceView` 并在隔离 STA Window 加载：显式选择诊断概览并展开“更多维护操作”，然后核对实际路径预览网格。
- 使用合成的 80 条预览项及含 210 字符段的 C 盘原路径/D 盘目标路径。两个生产只读 `TextBox` 均保持完整长字符串、NoWrap；焦点进入后 `SelectAll` 的 `SelectedText` 与完整原/目标路径严格相等，标准 Copy 命令对选区可执行。测试不写系统剪贴板。
- 生产 `DataGrid` 实际高度在 `60–261 DIP` 范围，`MaxHeight=260`、虚拟化和 `Recycling` 生效；80 项存在，末项行容器未实例化，覆盖有限视口而非仅检查 XAML 字符串。
- Worker `PathRemapServiceTests`：`4/4`。合成隔离 SQLite 负例覆盖不同盘符、相似根排除、长路径保留、缺失目标默认跳过和未确认执行拒绝；目标归档仍不存在，索引仍指向原归档路径。没有访问或写入用户目录。
- 两个定向 Release `dotnet test` 均构建对应测试项目及引用并通过；非完整 solution build。`scripts/check-xaml.ps1` `24/24`、`python scripts/validate-source.py`、`git diff --check` 通过。restore 使用独立输出根、本机 NuGet 缓存和串行 MSBuild；未更改仓库 SDK 或全局 PATH。

## 文件身份

| 文件 | SHA-256 | 说明 |
| --- | --- | --- |
| `tests/GameSaveCenter.Playnite.Tests/R12PathRemapBehaviorTests.cs` | `2F68CD4908A9F52268DCE077D1B77263ED5763254E1BADEA23EAE8D2F65FE753` | 当前 STA 生产视图行为夹具 |
| `src/GameSaveCenter.Playnite/Views/MaintenanceView.xaml` | `932C3364F133EF7AFAEDE4113BC2C784420A0AAAEA16D7545D752CAD290AB282` | 生产预览布局；未修改 |
| `src/GameSaveCenter.Worker/Services/PathRemapService.cs` | `484E80F2A56FD7DA68B19294F75893F3A352049ABE10864C914F0AEB85CB0C7F` | 生产预览/映射服务；未修改 |
| `src/GameSaveCenter.Worker/Persistence/SqliteStateStore.PathRemap.cs` | `8FD9BC3617E8D7F472A5F254AFE639B26506B69641DD7172251B519D2FF8926B` | 隔离索引路径更新；未修改 |
| `src/GameSaveCenter.Contracts/PathRemapDtos.cs` | `8AB8DBD484376704DEB78534BD248339C60644CC4B916348FD5B0F2F603869A0` | 路径预览 DTO；未修改 |
| Playnite `GameSaveCenter.Playnite.Tests.dll` | `980C5C27E3A21CA902D1D58D5C033FE1C445DD85DD473D683DBA39C3DA798CAA` | MVID `6e771dd8-a4df-4624-b6c1-6fe5d4b0b7f8`，隔离 Release 输出 |
| Worker `GameSaveCenter.Worker.Tests.dll` | `CF460A292DCBC23D151BD99159DC45AACA0130937AFCF3CD4B12173B7B8354F3` | MVID `776c9a4b-40b8-4a75-9100-ac2f4fe97eff`，隔离 Release 输出 |

| TRX | 结果 | SHA-256 |
| --- | --- | --- |
| [Playnite R12 path remap behavior](tests/R12PathRemapBehaviorTests-sdk9302-main.trx) | `3/3` | `6DD8607DCFF76B6841A2EDF803F8C93174AB12F53216CA8DD8771E3D00188759` |
| [Worker PathRemapService](tests/PathRemapServiceTests-sdk9302-main.trx) | `4/4` | `2FA25AFFD310DC3DEBB5FEAA24AC7D9B1A0F75D8733403096E866A0B8D10EA9C` |

## 状态与未验边界

R12-03 的代码与自动行为条件在当前 main 复核满足；本次没有改生产服务、DTO、索引格式、命令、安全策略或视觉体系。受控 WPF Window 证明逻辑布局与键盘选择行为，不代表真实 Playnite 呈现、系统剪贴板结果、UIA/IME、物理 DPI/跨屏、最终呈现帧、ETW 或宿主性能。未运行路径迁移命令；未创建真实目标文件、修改用户索引、访问存档/媒体/云端。

提交 `750349f3` 推送后，GitHub Actions 页面仍停留在 #979（`ae3c565`）Queued 且内容加载失败；没有出现 `750349f3` 的 run ID。直接 commit/checks 页返回 cache miss，匿名 Actions API 为内部错误，因此该提交 CI 终态未知。此次证据提交后的 CI 也须跟到明确终态；SDK8 与 SDK10.0.401 分开记录。本机 SDK9 不能替代任一 CI SDK。

下一项：Q24-04 键盘次序的完整业务状态行为补证。现有 90 个默认状态目标遍历测试已经通过；先检查测试未覆盖的六页筛选、表格、详情和页尾数据状态，再补一项真实焦点路径/负例。Q24-03 的第二物理显示器，以及 Q24-04 的真实 Playnite/OS 键盘与视觉走查仍单独待验。
