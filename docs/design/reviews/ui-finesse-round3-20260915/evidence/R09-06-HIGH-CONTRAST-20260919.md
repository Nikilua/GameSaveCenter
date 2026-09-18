# R09-06 高对比真实配色证据

日期：2026-09-19  
实现提交：`1f2eac4a`（补齐高对比主题语义资源回退）  
分支：`codex/ui-finesse-round2`

## 结论

R09-06 在当前可控 WPF/资源条件下已满足。现有 `AdaptiveThemePalette` 已有高对比入口，本阶段没有另建主题服务或控件体系；补齐的是可验证的状态传递与资源回退：

- `AdaptiveThemePalette.IsHighContrast` 记录实际 `SystemParameters.HighContrast`，并提供仅测试使用的 override，不修改 Windows 全局设置。
- 高对比 palette 使用 `SystemColors.Window/WindowText/Control/ControlDark/Highlight/HighlightText/GrayText/HotTrack` 语义色；禁用文字不再沿用普通主题的透明度算法。
- 进度轨道使用 `ControlDarkColor`，进度填充使用 `HighlightColor`；选中文字、行悬停和图标底色沿用高对比语义资源。
- 高对比时 `GlassEnabled=false`，阴影和游戏背景模糊为真实 `null`，Popup 不透明且动画为 `None`，环境洗色透明；按钮渐变 stop 保持不透明。
- 普通主题重新应用到同一个 `ResourceDictionary` 后，ambient wash 与按钮半透明 stop 恢复，进度轨道离开高对比的 `ControlDarkColor`；没有把恢复写成真实系统主题切换。

## 行为证据

`R09HighContrastBehaviorTests.HighContrastUsesSystemSemanticTokensAndRestoresMaterialAfterRecovery` 在隔离 STA WPF 中：

1. 用 `highContrastOverride=true` 创建 palette，并把生产 `ApplyRuntimeThemeResources` 应用于实际 `Grid` 资源字典。
2. 通过 `DynamicResource` 绑定的实际 `ProgressBar`、`Path` 图标和 `TextBlock` 读取运行时值，验证进度填充、图标和禁用文字，而非只检查源码字符串。
3. 验证窗口/文字/禁用语义色、选中/悬停资源、玻璃 stop alpha、阴影、Popup transparency/animation、游戏背景 opacity 和 ambient wash。
4. 在同一资源字典重新应用 `highContrastOverride=false`，验证普通主题 material 恢复。

正式 Release 身份下结果：

- R09-06 新夹具及 R09 相邻行为回归、两个高对比源码门禁：`13/13`。
- `scripts/build.ps1 -SkipTests`：XAML `24/24`，solution `0 warning / 0 error`，Playnite `net462` 构建成功。
- `python scripts/validate-source.py`：通过。
- `scripts/check-xaml.ps1`：`24/24`。
- `git diff --check`：通过。

## 真实边界

本阶段没有切换真实 Windows High Contrast，也没有写系统设置；因此不能宣称真实 OS 主题通知、用户自定义高对比方案或真实 Playnite presented frame 已验证。隔离 STA/offscreen logical DIP 也不替代真实物理 DPI/跨屏、UIA/读屏、IME、ETW、宿主性能或 package-host 安装验证。

Demo 原目录不可用，继续使用已恢复的生产资源基线。用户提供的 `DEV-INSTALL-008` main 合并后安装失败仍保留为发布边界：本阶段没有在 dirty main 上覆盖、合并或重跑安装器，也没有把当前分支隔离通过改写成安装通过。

下一可执行任务：R09-07 缩略图占位一致；先盘点加载、失败、无图、视频和损坏文件的现有占位入口及行高约束。
