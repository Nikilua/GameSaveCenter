# R02-04 图文光学居中证据

## 结论

R02-04 已满足。当前 `codex/ui-finesse-round2` 的提交为 `d296ce081810acd8e4d34c8538d1e9c3f10e49ab`，本阶段没有重建生产按钮体系：最新生产代码已经有共享文字模板、统一内容居中和复合图标内容模板豁免，本阶段只补真实 WPF 几何门禁。

- 同一共享文字按钮样式下，中文四字和英文双词使用相同的内容模板与 36 DIP 高度，实际基线差小于 `0.5 DIP`。
- 图标加数字复合内容分别以 16/20 DIP 图标测量，图标与数字保持 `8 DIP` 间距，垂直中心差不超过 `1.5 DIP`，没有挤字。
- Light/Dark 受控截图对照通过；没有修改游戏选框、滚动条、命令绑定、取消/错误语义、恢复保护或 net462 兼容。

## 既有能力核对

核对当前生产资源后确认：

1. `WpfUiProduction.xaml` 的 `GscWpfUiButtonTextTemplate` 使用 `TextAlignment="Center"`、`VerticalAlignment="Center"`、不换行和受边界约束的省略，`GscWpfUiButton` 使用统一 `HorizontalContentAlignment/VerticalContentAlignment=Center`。
2. `Redesign.xaml` 的 Header 按钮继承共享生产按钮高度和居中属性；带图标/标签 `Grid` 的 Header Visual 按钮明确设置 `ContentTemplate={x:Null}`，因此复合内容不会被文字模板转成控件类型名称。
3. 真实生产页面已有 18/20 DIP 图标、图标加标签和图标加数字的内容组合；本阶段未另造图标或改变按钮命令。

## 本阶段补强

`tests/GameSaveCenter.Playnite.Tests/R02OpticalAlignmentTests.cs` 新增两个真实 STA WPF 测试：

- `SharedTextButtonsKeepTheSameBaselineForChineseAndEnglishLabels` 从生产 ResourceDictionary 创建两个同样式、同高按钮，遍历实际视觉树中的 `TextBlock`，用 `BaselineOffset` 和相对按钮坐标比较基线，不是 `Assert.Contains` 源码断言。
- `CompositeIconAndCountButtonsKeepAnEightDipGapAndSharedCenter` 对 16 和 20 DIP 两种图标尺寸创建生产 Header Visual 按钮，测量实际图标/数字边界、间距和中心差，两个理论场景均须通过。

## 行为与视觉验证

### R02 定向行为测试

在 `d296ce0` 身份下执行 R02-04 定向 Release 编译和测试：

```powershell
dotnet build tests/GameSaveCenter.Playnite.Tests/GameSaveCenter.Playnite.Tests.csproj -c Release --no-restore -m:1 -p:GscBuildCommit=d296ce0 -p:NuGetAudit=false -p:MSBuildEnableWorkloadResolver=false -v:minimal
dotnet test tests/GameSaveCenter.Playnite.Tests/GameSaveCenter.Playnite.Tests.csproj -c Release --no-build -m:1 -p:GscBuildCommit=d296ce0 -p:NuGetAudit=false -p:MSBuildEnableWorkloadResolver=false --filter FullyQualifiedName~R02OpticalAlignmentTests --logger "console;verbosity=minimal"
```

结果：编译 `0 warning / 0 error`；定向测试 `3` 通过、`0` 失败、`0` 跳过。测试覆盖同样式中英文基线以及 16/20 DIP 图标与数字的实际边界，而不是只校验资源键存在。

### Release 构建与全量回归

执行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\build.ps1 -Configuration Release -OutputRoot .tmp\r02-04-build-final2
```

第一次同阶段全量运行在既有 `WorkerIpcClientBehaviorTests.CallerCancellationDuringReplayWaitStopsWithAmbiguousOutcome` 出现一次 IPC 时序失败；同一 `d296ce0` 隔离输出立即复跑该单项为 `1/1` 通过，随后完整脚本复跑成功。最终结果：XAML `24/24`；解决方案构建 `0 warning / 0 error`；Core `83/83/0`、Worker `311/311/0`、Playnite `535` 通过、`57` 跳过、`0` 失败，总计 Playnite `592`。未隐藏或重写这次环境时序事实。

`python scripts/validate-source.py` 通过。57 条跳过沿用 R01-08 的 LegacyProductionUiBaseline 分类，没有为 R02-04 改写测试状态。

### Light/Dark 受控 RenderHarness

执行 `scripts\render-qa.ps1 -Configuration Release -Output .tmp\r02-04-render-final`，报告头部确认：

```text
EvidenceSource: OffscreenRenderHarness
Commit: d296ce081810acd8e4d34c8538d1e9c3f10e49ab
WorkingTreeClean: True
```

Light/Dark 各覆盖 Overview、Save、Trainer、Media、Maintenance、Task、Settings 的 `1040x700`、`1100x720`、`1366x768`、`2560x1440`，共 56 个视图/尺寸场景，均为 `OK`，最终 `render-qa OK`。人工对照 `theme/light/Save-1040x700.png` 和 `theme/dark/Save-1040x700.png`：同高按钮的图文排列、表格行和底部动作栏均完整；没有因按钮内容模板或图标尺寸变化产生明显上下漂移或裁切。

## 证据边界与安全范围

行为测试使用生产 WPF 资源、合成图标 Geometry、隔离 STA Window 和 logical DIP；RenderHarness 使用合成页面数据。没有启动真实 Playnite，没有写真实存档、媒体、用户云端或外部诊断，也没有删除真实文件。`.tmp` 下的构建/截图是本阶段临时输出，文档同步后清理。

已验证的是受控 WPF 视觉树的实际基线、边界间距、中心差、双主题离屏布局和有限列表/滚动回归；未验证真实 Playnite 嵌入、物理 DPI/跨屏、OS 键盘/IME、屏幕阅读器朗读、presented frame、ETW、真实 Worker 时序或宿主大库物理性能。不能把离屏截图写成真实屏幕呈现或系统跟踪结论。

## 下一步

下一可执行小批量为 R02-05“命中区与间距”：先盘点紧凑工具条及复制/删除等行内动作的实际命中矩形，再用相邻按钮负例和窄窗换行行为校验共享密度令牌；不改滚动条系统或真实危险命令语义。
