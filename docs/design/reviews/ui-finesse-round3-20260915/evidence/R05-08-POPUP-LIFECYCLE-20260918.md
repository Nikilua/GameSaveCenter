# R05-08 弹层资源热切换与生命周期证据

日期：2026-09-18（Asia/Shanghai）  
任务：`R05-08 | 弹层资源热切换`  
实现提交：`599a8fd96f2808f5879f7eee7dd1d564e054db8a`（`收口弹层主题与生命周期`）  
测试夹具提交：`bebde2fed18f3ad891d6fe4123c6b5a6757b96fd`（`稳固弹层生命周期测试夹具`）

## 结论

当前生产 Settings 页的 Popup/Tooltip 资源热切换与父窗隐藏收口到当前可控范围：弹层打开时从 Light 切到 Dark，两个实际附属层都保持打开并切换到活动主题资源；父窗 `Hide()` 后视图不可见，Popup 与 Tooltip 均关闭。局部行为没有修改全局静态资源，也没有留下跨页面的静态订阅；显式 Tooltip 的事件处理器在根节点卸载时解除。

本轮先核对了现有 `GameSaveCenterSettingsView`、`AdaptiveThemePalette`、共享 Popup/Tooltip 样式和 R05-07 的局部 `GscToolTipBehavior`，只补真实资源链与生命周期缺口。保留当前游戏选框、滚动条、命令/Binding、取消/错误、安全/恢复语义、有限列表性能和 Playnite/net462 兼容。

## 实际生产行为

`R05PopupLifecycleBehaviorTests` 在真实 `GameSaveCenterSettingsView`、生产资源、STA WPF `Window` 和隔离目录中使用合成设置数据验证：

1. 打开实际外观页 ComboBox 的 `PART_Popup`，同时打开绑定在设置提示上的生产样式 Tooltip；Light 主题下两个附属层均有实际背景。
2. 调用生产 `ApplyThemeForAudit(Dark)` 后，ComboBox Popup 和 Tooltip 仍为打开状态，主题选择器显示 Dark，且两者背景颜色均从 Light 资源切换到 Dark 资源；这覆盖了 Tooltip 脱离页面视觉树后回退静态深色资源的负例。
3. 隐藏承载窗口后，Settings 视图不可见，Popup 与 Tooltip 都关闭；测试没有写入真实设置或用户数据。

实现侧由 `GscToolTipBehavior` 以附加范围管理瞬时层：加载时注册范围内显式 Tooltip，打开/主题变化时把当前作用域资源同步到脱离视觉树的 Tooltip/Popup，卸载时关闭附属层并解除显式 Tooltip 处理器。`AdaptiveThemePalette` 同时为实际生产 `AcrylicReferenceControls.xaml` 仍使用的旧 Acrylic 资源键提供活动 Demo 主题别名，避免静态字典掩盖主题切换结果。没有增加 Application 级事件、全局弱引用表或无法回收的 scope delegate。

## 构建与回归

最终标准产物由：

```text
scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot .tmp/r05-08-build
```

生成，XAML structural validation `24/24`，Release 编译 `0 warning / 0 error`，并绑定最终测试夹具提交 `bebde2fe...`。从该产物分进程定向运行的结果为：

- `R05PopupLifecycleBehaviorTests 1/1`；
- `R05TooltipTimingBehaviorTests 1/1`；
- `R05PopupBoundaryBehaviorTests 2/2`；
- `R05FocusBoundaryBehaviorTests 3/3`；
- `R05TogglePersistenceBehaviorTests 1/1`；
- `R05OptionVirtualizationBehaviorTests 3/3`；
- `UiFinesseRound2ControlSourceTests 24/24`。

在实现提交 `599a8fd9` 上运行的标准全量测试记录为 Core `83/83`、Worker `311/311`、Playnite `573/665` 通过、`57` 跳过、`35` 失败。失败集中在并行 WPF `Application`/隐藏窗口/布局夹具的既有环境假设；最终夹具提交后上述 R05-08 及相邻回归均已按隔离进程复跑通过，因此不把该全量并行结果写成 R05-08 的产品失败，也不把它写成全量绿色。

## 视觉证据

执行：

```text
scripts/render-qa.ps1 -Configuration Release -Output .tmp/r05-08-render-clean
```

报告 `.tmp/r05-08-render-clean/render-qa-report.txt` 绑定完整提交 `bebde2fed18f3ad891d6fe4123c6b5a6757b96fd`，记录：

- `WorkingTreeClean=True`、Light/Dark、357 张 PNG；
- `DpiScale=1.00`，明确仅为离屏 logical DIP，不推断真实宿主物理 DPI；
- Settings 1040×700 开面探针 `popupOpen=True tooltipOpen=True`，主资源从 `#F21B1F27` 切换为 `#FFF2F4F8`，共 6 张开面截图；
- 主题、尺寸、滚动与 resize transition 探针完成，最终 `render-qa OK`。

已人工抽查：

- `Settings-theme-switch-light-open-1040x700.png`；
- `Settings-theme-switch-dark-open-1040x700.png`；
- `Settings-theme-switch-light-tooltip.png`；
- `Settings-theme-switch-dark-tooltip.png`。

四张图中的 Settings 页面层级、Popup/Tooltip 形态和双主题对比正常，未见本轮资源别名与生命周期改动造成的页面裁切、横向溢出或旧深色资源残留。

## 边界

行为证据是隔离目录、合成设置数据、生产 WPF 视觉树和离屏/隐藏窗口测试，不触碰真实存档、媒体、云端或用户诊断数据。`Window.Hide()` 覆盖的是当前夹具的父窗隐藏语义，不等价于真实 Playnite 宿主关闭、停靠、重建或跨扩展窗口生命周期。

当前没有真实 Playnite 宿主中的主题切换录像、物理 DPI/跨屏 Popup、呈现帧无残影、OS 输入/IME、读屏/UIA、泄漏 profiler、ETW 或宿主性能证据；离屏 `DpiScale=1.00` 也不代表物理屏幕像素。全量 WPF 并行测试的 35 条失败尚未在真实宿主环境重跑收口，不以代理测试替代这些边界。

下一项：`R06-01` 列宽用户记忆。
