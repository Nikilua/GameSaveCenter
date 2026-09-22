# R14-06 批量目标防误选定向复核

日期：2026-09-24
工作区：`D:\\workplace\\github\\GameSaveCenter`
分支：`codex/ui-finesse-round2`
当前复核代码身份：`89f07445`
实现提交：`cfbb1279`（本批没有生产代码变更）

## 结论

R14-06 已按受控实现和定向行为结果收口为“已满足，待环境验证”。本批没有重建游戏选框、目标服务或命令链；只补一条真实行为夹具并复跑当前 Release/net462 产物，确认同名目标靠稳定 Playnite ID 区分，过滤不会把已选目标悄悄换成可见项。

## 受控行为验证

- `GamePickerViewModelTests 22/22` 通过。新增 `SameNameItemsRemainDistinctAndSelectionUsesPlayniteId`：两个显示名均为“同名游戏”的 DTO 产生不同 `IdentityDisplay`，并以 `same-name-id-b` 作为 preferred ID 时仍选中第二个对象；已有过滤隐藏选择和“显示当前游戏”行为继续通过。
- `R14ClassificationSelectionTests 4/4` 通过。目标下拉、批量归类、预览覆盖和重新归类入口继续使用 `SelectedItem` 或 `TargetPlayniteId`，不使用 `SelectedIndex`；模板字段包含图标、平台和稳定身份。
- `GamePickerKeyboardBehaviorTests 6/6` 通过，保留现有 Enter、无结果、输入法组合态和方向键行为门禁。
- `scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot artifacts/gsc-b/r14-06-recheck-20260924` 通过；XAML `24/24`、解决方案 `0 error/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning，Playnite 目标 `net462`。本批只新增测试夹具，既有 nullable 边界未改。
- `python scripts/validate-source.py`、`scripts/check-xaml.ps1 -ProjectRoot .` 和 `git diff --check` 在本批文档更新后通过。

## 受控视觉证据

- 人工检查既有 clean RenderHarness 截图 `artifacts/ui-qa-r13-r14-clean-20260922/Shell-Media-1040x700.png`：全局游戏选框显示图标/平台、游戏名和 `Playnite ID`；媒体批量目标卡片显示平台占位、名称和稳定 ID。该截图来自合成数据和 offscreen WPF，不包含两个同名目标的真实下拉交互，因此只作为视觉存在性证据，不写成真实宿主呈现或重名运行时验收。
- 同一 RenderHarness 报告全局仍因 Overview/Settings/Task/Save 既有基线以 `FAILED` 结束；本批不扩大为全局 `render-qa` 通过，也不把离屏截图写成物理 DPI、最终 presented frame 或性能证据。

## 语义与边界

- `IconPath` 继续只解析 Playnite 已有本地图标引用；缺失、远端或异常回退为空，不下载图标，不读取或写入真实存档/媒体。
- 只使用合成 DTO、fake/隔离 testhost、隔离构建输出和既有 RenderHarness；没有真实媒体、云端、用户存档或外发诊断。
- 真实 Playnite/package-host、UIA/读屏/IME、物理 DPI/跨屏、最终呈现帧、ETW、宿主性能和超大真实媒体库仍未验；Demo 原目录不可用，视觉基准沿用恢复生产基线。

下一可执行小批量：推进 `R14-07 媒体详情浏览`，先复用当前 `SelectedMedia`、分页和稳定 `MediaId`，再按同样门禁补行为与呈现边界证据。
