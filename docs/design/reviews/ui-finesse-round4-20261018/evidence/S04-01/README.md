# S04-01 比较对象集合

## 实现

媒体中心增加一个与当前筛选结果独立的比较对象集合，最多保存两张有效截图 DTO，并以 `MediaId` 保持稳定身份。加入时只检查用户选中的截图 `ArchivePath` 是否存在；视频、路径缺失、文件缺失、重复 ID 和超过两张上限都会在页面状态中说明。切换媒体筛选不会替换集合，清空命令会释放当前对象。未扫描其他目录，也未读取图片内容。

真实 `MediaCenterView` 的加入与清空按钮绑定到生产 ViewModel 命令。行为测试覆盖当前多选、状态摘要、筛选后保留、清空和不可执行状态；测试文件只在唯一临时目录中创建两个空 PNG 占位文件来验证存在性，不触碰真实媒体库。

## 构建与验证

- 固定 SDK `8.0.423`，`scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot .tmp/round4-s04-01-validation/build`：Release 全解决方案构建通过，`0 warnings / 0 errors`；XAML 结构检查 `25/25`。
- `MediaComparisonSelectionTests`：`5/5` 通过，包含真实页面按钮绑定/命令路径。TRX：[S04-01-media-comparison.trx](tests/S04-01-media-comparison.trx)。Playnite 测试目标为 `net472`，运行时 CLR `4.8.9181.0`。
- RenderHarness 在相同隔离输出下成功构建；浅色/深色、11 个窗口尺寸（`1040×700` 至 `3840×2160`）均完成渲染，`render-qa OK`、`0 PROBLEM`。摘要：[render-summary.txt](render-summary.txt)。[离屏截图](render/Media-2560x1440-tab1.png)展示深色主题当前游戏媒体页中的比较对象入口；它是合成夹具，不代表真实 Playnite 屏幕。
- `python scripts/validate-source.py .` 与 `git diff --check` 通过。

## 输入身份

实现提交为 `f911cba4`。构建与测试使用其父提交 `44c650df27d4c0b46c5481a2f6e2974c3381030b` 上包含本项改动的工作树；以下源文件哈希与提交内容一致。构建 DLL、测试 DLL/PDB、TRX 和离屏截图均按 SHA-256 固定如下。RenderHarness 报告的 `WorkingTreeClean` 为 `False`，其源码身份以本表哈希为准。

| 文件 | SHA-256 |
| --- | --- |
| `MediaComparisonSelection.cs` | `766F6973990F97E9ACC446F12F650F80DC23D6F3B12C47997F91791A89BCAE53` |
| `DashboardViewModel.Media.cs` | `73185F5950043A0D32EF142B1C06E462B05479B4B40239E2A2E527881C1A08E3` |
| `DashboardViewModel.cs` | `7A3AA9E0286EED7CACE9717DEBC65EC8EC19A5784DBF71FE2CBCE9FB3B248542` |
| `MediaCenterView.xaml` | `E41902E0A166647F4F661022B8702620999F452F500AD2F1FED9F5F3AA4B22B9` |
| `MediaComparisonSelectionTests.cs` | `8C88CAB27053DFAE730085251C1B02B8654FF37F5728713D56902AF07B7F67DF` |
| `FakeDashboardData.cs` | `E353DB2D0B5C2C66032EB5E9B884B34829A99965395A2C1133D1D914EBFA0AAA` |
| Playnite plugin DLL | `10F0AB6802320EA1A5F060CE85BEEB4A9E3DC610BFCF5E76E37FB04924E1FC6F` |
| Playnite test DLL | `1E6668C0C715BEC746D8289DA36F6EE346B25FA4C2037A2D22E4542981C49B13` |
| Playnite test PDB | `5EF43771C7EAB6B04AD971A80DA56E96B50FF291A5D53391CA3C65EA35558B6B` |
| TRX | `49F1DFB181B6DBDB6D52B289E8B3AB3AE1723E296F6461F8378692CA991BD453` |
| Render report (temporary; summary retained) | `101322FFE600D6C76C2666C764418AE595D5C30EA8DC7EA950E903E7DCD992EE` |
| Offscreen screenshot | `864911DC3631745A4C7D3640CD876BBDDC1164811DF9559C48F04BB6E44C8D3B` |

## 边界

本项完成受控构建、行为测试和 OffscreenRenderHarness 验证；没有运行完整 WPF 逐类隔离循环或真实 Playnite，也没有用合成渲染冒充真实屏幕。图片解码、并排工作区与缩放属于后续 S04-02/S04-03 范围。
