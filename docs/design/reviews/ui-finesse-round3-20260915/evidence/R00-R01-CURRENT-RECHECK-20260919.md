# R00/R01 当前提交复核

## 结论

R00/R01 现有实现与证据索引已按当前可复核提交重新校正；没有把表格中的新增方向当作代码缺口，也没有重建已经存在的服务、DTO、命令或控件实现。当前代码提交为 `3354fd82400df6659165a688b8fcb1eb87116ca4`，分支为 `codex/ui-finesse-round2`，证据包身份仍为 `not-provided`。

本次实际发现并修正两类问题：

- `R01-07-EVIDENCE-FRESHNESS-20260916.md` 的旧当前扫描段落曾写入 `c3e67cb4`，与随后实际复核的提交不一致；现保留历史段落并追加 supersede 说明。
- R00/R01 的旧源码测试仍检查已迁移的对话框动画调用、旧忙碌触发器属性和未包含关闭期保护的焦点行；修正为当前 `DialogOverlayMotion`、`IsBusyIndicatorVisible` 和 `dialogLifecycle.IsClosing` 契约。

同时修正 RenderHarness 与 UiAuditRunner 的源码身份解析：隔离 `OutputRoot` 位于仓库 `.tmp` 时优先使用构建元数据中的 `GscSourceRoot/GscBuildCommit`，不会再从临时输出目录向上误认主工作树。

## 干净提交验证

验证从 detached 的当前提交工作树进行，输出目录为临时目录，完成后清理：

- XAML 检查 `24/24`；Release solution build `0 warning / 0 error`，目标包含 Playnite `net462`。
- 受影响的 `UiFinesseFoundationTests` `8/8`、`UiFinesseRound2ControlSourceTests` `24/24`、`RepositoryIdentityTests` `2/2`、`UiNegativeFixtureRegistryTests` `1/1`、`UiDiagnosticsExporterTests` `11/11`、`LargeLibraryPerformanceTests` `5/5`、`MediaInboxGeometryTests` `2/2`、`UiAuditSourceTests` `6/6`、`GamePickerKeyboardBehaviorTests` `6/6`、`KeyboardFocusSourceTests` `5/5`、`GamePickerViewModelTests` `20/20`；共享上下文样式回归 `2/2`，壳层源码测试退出码为 `0`。
- 浅色/深色合成 finesse 报告均为当前完整提交身份；数值可读性 `expected=4 realized=4 horizontalFit=4 verticalFit=4 allReadable=True`，语义按钮对比度 `88` 个样本、`0` 个违规，负向 fixture 通过。
- 浅色/深色 motion hot-change 与 re-entry 探针均通过；动画中途状态可见，禁用动效归一化终态和重入终态均回到预期尺寸/位置。
- media geometry probe 通过。
- 当前审计为 `161` 个 runtime snapshots、`80` 个 INFO、`0` Fidelity、`0` failed routes、无 HIGH/MEDIUM；EVIDENCE_INDEX `20/20` 条引用、身份、样本和边界均通过校验。
- `check-ui-evidence-freshness.ps1` 输出 `14` 条 fresh、`0` 条 stale；`test-ui-evidence-freshness.ps1` 的 documentation-only、shared-control、package-identity 三类测试通过。

## 影响范围

本次重新绑定或复核的记录为：R00-01-02、R00-03、R00-04、R00-05、R00-06、R00-08、R01-02、R01-03、R01-04、R01-05、R01-06。R00-07、R01-01、R01-07 在当前路径规则下原本已 fresh，本次没有为了制造统一时间戳而重复构建；所有记录的当前扫描结果仍由新鲜度报告逐条给出。

## 真实未验边界

- Demo 原目录不可用；视觉结论沿用恢复的生产资源基线 `AcrylicProductionResources.xaml`，没有伪造 Demo 像素对比。
- 本轮验证是合成数据、fake/隔离服务、STA WPF 和离屏逻辑 DIP；没有宣称真实 Playnite 嵌入、物理 DPI/跨屏、真实键盘/IME、UIA/读屏、presented frame、ETW 或宿主性能通过。
- 没有修改真实存档、媒体、用户云端或对外发送诊断；没有运行会覆盖用户 Playnite 的非隔离流程。
- 包身份仍为 `not-provided`，因此不写入安装成功、包宿主呈现或真实安装回退结论。

## 下一步

R00/R01 当前证据校正完成后，下一可执行小批量为 R08-08：先检查 `GscMotion` 的共享可变/冻结 `Freezable`、变换实例归属及 R00-02 已有覆盖，再补最小行为与负例证据。
