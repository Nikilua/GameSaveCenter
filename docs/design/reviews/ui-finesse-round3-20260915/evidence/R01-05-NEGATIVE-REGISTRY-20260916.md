# R01-05 负例注册表证据

## 结论

R01-05 已满足。`5e6d64aea3a64bc3fcde85627a84ce7cff5759e2` 在测试项目新增测试侧注册表 `UiNegativeFixtureRegistryTests`，为对比度、裁切、焦点、层级和状态各保留一个有意不合格夹具，并实际调用对应检测器；没有把夹具或注册入口放进生产项目。

这里的 `expected-failure` 指夹具必须被检测器拒绝或标记为异常；健康实现下注册表测试本身应通过，不把“检测到问题”误写成产品测试失败。

## 注册表与实际结果

| ID | 类别 | 检测器 | expected-failure 夹具 | 实际检测结果 |
| --- | --- | --- | --- | --- |
| N01 | 对比度 | `AdaptiveThemePaletteContrastGuard.ValidateTextContrast` | 黑字 `Colors.Black` / 暗底 `37,42,52`，最低 `4.5` | `violations=1`，命中 `negative-black-on-dark` |
| N02 | 裁切 | `NumericCellReadability.Measure` | `56 DIP` 数值列、长负数，行高仍为 `52 DIP` | `horizontalFit=False`、`verticalFit=True`、`isReadable=False` |
| N03 | 焦点 | `AcrylicProductionShellView.OnPickerPreviewKeyDown` | 旧游戏仍选中但搜索无可见结果，发送 Enter | `handled=False`、`overlay=Visible`、`selectionPreserved=True` |
| N04 | 层级 | `RealHostUiAuditService.CheckChildLayoutOverflow` | `100 DIP` 父级中放入 `200 DIP` 子级 | `CHILD_LAYOUT_OVERFLOW.json` 已写入，`gateExists=True` |
| N05 | 状态 | `WorkspaceStatePresenter` 实际命中测试 | Loading 状态叠在底层按钮上 | `retry=Collapsed`、`underlyingHit=False` |

五项由 `UiNegativeFixtureRegistryTests.RegisteredNegativeFixturesAreRejectedByTheirDetectors` 一次注册表测试执行；每项的 probe 都返回 `detected=True` 才能通过。N02/N03/N05 使用受控真实 WPF `Window` 与视觉树，N04 的 gate 写入使用随机隔离临时目录并在 probe 结束后删除。

## 可复核验证

- `scripts/build.ps1 -Configuration Release -OutputRoot .tmp\r01-05-final-build`：XAML `24/24`；构建 `0 warning / 0 error`；Core `83/83`；Worker `311/311`；Playnite `523` 通过、`57` 跳过、`0` 失败，总计 `580`。
- R01-05 注册表定向测试：`1/1` 通过；源码校验 `python scripts/validate-source.py` 通过。
- 代码提交 `5e6d64a` 已推送到 `origin/codex/ui-finesse-round2`；该提交只新增测试项目文件，未修改生产入口。

## 2026-09-18 当前分支复核

- 以当前提交 `810114e2` 新建 `.tmp\r01-05-build-810114e2`：XAML `24/24`，solution Release `0 warning / 0 error`；测试程序集通过构建绑定身份校验，没有复用旧默认 bin。
- `UiNegativeFixtureRegistryTests.RegisteredNegativeFixturesAreRejectedByTheirDetectors` 当前 `1/1`。运行时逐项输出均为 `detected=True`：N01 对比度 `violations=1`；N02 长负数 `horizontalFit=False / verticalFit=True / isReadable=False`；N03 `handled=False / overlay=Visible / selectionPreserved=True`；N04 隔离 `CHILD_LAYOUT_OVERFLOW` gate `gateExists=True`；N05 `retry=Collapsed / underlyingHit=False / presenterHitTestVisible=True`。
- `python scripts/validate-source.py` 当前通过。此次没有修改生产 UI、服务/DTO、命令/绑定、游戏选框、滚动条或安全语义；只是用当前源码重新执行既有测试侧注册表。

## 边界

- 夹具使用合成颜色、DTO、WPF 控件、隔离 Window 和 offscreen logical DIP；不等价真实 Playnite 嵌入、OS 输入/IME、物理 DPI、presented frame、ETW 或宿主性能。
- 未执行备份、恢复、删除、迁移、下载、真实媒体写入、云端写入或诊断外发；当前游戏选框、滚动条、命令/绑定、取消/错误、恢复保护、有限列表和 `net462` 契约未改。
- 注册表证明检测器能抓住指定的五类错误，不代表所有页面或所有状态已经完成视觉/宿主验收。

## 下一步

R01-05 当前证据已满足；下一可执行小批量为 R01-07 freshness baseline 的增量更新，然后处理 R01-06“宿主证据保全”，只归档可由 clone 后复核的身份、manifest、summary 和精选证据，不保留无引用临时构建物。
