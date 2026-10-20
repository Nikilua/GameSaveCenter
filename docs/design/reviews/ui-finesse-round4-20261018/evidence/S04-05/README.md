# S04-05 比较候选切换

## 交付

媒体比较工作区新增“上一候选 / 下一候选”。操作只替换右侧 B，左侧 A 始终锁定；详情区的 `SelectedMedia` 不变。候选来自当前 `MediaView` 已加载且经当前筛选/搜索后的有限结果，排除录像和 A，不自动请求下一页。右侧图片下方显示当前筛选结果中的位置。

候选切换沿用生产 `AsyncThumbnailImage` 的后台解码、取消和代际检查。图片加载失败/缺失时，本次比较会话将该稳定 `MediaId` 标为不可用，优先沿当前浏览方向跳到下一张有效图片；没有后续时回到反方向有效图片。当前筛选结果刷新、清空或重新添加比较对象时会重置失败标记。若文件在探测与替换之间消失，也会在同一有限结果范围重试。

## 受控验证

- SDK `8.0.423` Release 全解决方案构建成功：`0 warning / 0 error`。命令输出和 SDK 选择见 `build-console.txt`、`sdk-version.txt`。
- 定向测试 `6/6`：候选导航边界/缺失跳过 `2/2`；生产 WPF `MediaCenterView` 比较视图行为 `3/3`；既有 `AsyncThumbnailImageTests.ReplacingPathNeverLeavesTheOldImageVisible` `1/1`。TRX 位于 `tests/s04-05-media-comparison.trx`。
- WPF 行为用例显示生产页面并使用合成 PNG 和一个损坏图片，验证筛选隐藏项和视频不会成为候选、焦点留在导航按钮、详情选择及 A 不变、坏图被跳过后仍可前后浏览；同时渲染候选位置、按钮和最终 A/B 图。截图 [media-candidate-navigation.png](media-candidate-navigation.png) 是离屏 WPF 合成夹具，不是 Playnite 或真实屏幕。
- `python scripts/validate-source.py .`、`scripts/check-xaml.ps1`（25 个文件）和 `git diff --check` 通过。

## 输入与产物身份

构建时 HEAD 为 `3b8e52b9147d6b7bedff10fddbbd3c1d03bcd3a8`；本批源码在该提交之上。构建的 Playnite DLL SHA-256：`D46B717C1C3207C45FA0B84A97215A0F0AA79B3F09F3142345408B96230A0E95`；测试 DLL：`F7B9A62825F2512758A8BF5D563B7C8FC84F366EAFC9EF69D8C92A334F6215AD`；测试 PDB：`4FFAB69ECA0D0B125B7C87FD4669A5723C2CD7D9A763D191B8194F52BA09B5B4`。

TRX SHA-256：`A9ABA3E9F1117F0713EEDFB5A2817CBF9A53083E2D067D842BD5CD2E7B9132DD`。截图 SHA-256：`44348D5935FB8B4810695D1CC1470B1C77ECF26A7792AF15A7743EB9BD62928A`。生产源码和两份测试源码 SHA-256：

| 文件 | SHA-256 |
| --- | --- |
| `Infrastructure/MediaComparisonCandidateNavigator.cs` | `14EC91C290F33C6178D5F98BF2C5A77B9CA68BE4DB9696D19860495772F274FB` |
| `Infrastructure/MediaComparisonSelection.cs` | `DFA3EDF7FD71297DA8241787261F4E101682B201AD7FC11EC5BFCD63A5A05E7C` |
| `ViewModels/DashboardViewModel.cs` | `9A67323FFA8533352EFC681432280C884D267B28D883D63160911E69150C70B1` |
| `ViewModels/DashboardViewModel.Media.cs` | `AE37ADF8EB9EF4F7B83F800943793E5D87140EB9733F15AD737CCD08D14832E8` |
| `Views/MediaCenterView.xaml` | `14AC9401DE77E3FDDC0A484D6A9E9B0B8DEB8A64B38C4D09EB8104A1621CD074` |
| `Views/MediaCenterView.xaml.cs` | `68D9F8C607F46C490040232CC20D4845BB98EE470C33044B2142F0977E85ECA6` |
| `MediaComparisonCandidateNavigatorTests.cs` | `1FA8F67974B71EB9C07E345E6800C7DEEA15C371CAA85FDE9770AFB8B6E9748E` |
| `MediaComparisonViewportBehaviorTests.cs` | `D16EBF01A9453C08AD9576227C144F757CAD0CB196941A3EB6CF9A692F06667C` |

## 边界

没有运行真实 Playnite 页面、真实媒体设备、物理 DPI 或完整 WPF 独立循环。候选范围只覆盖当前已加载结果窗口；用户可用既有媒体列表分页操作载入更多结果。真实鼠标与键盘设备输入未测试；受控用例直接执行按钮命令并核对 WPF 焦点状态。图片像素内容未用于自动一致性判断。
