# R14-07 媒体详情浏览定向复核

日期：2026-09-24
工作区：`D:\\workplace\\github\\GameSaveCenter`
分支：`codex/ui-finesse-round2`
当前复核代码身份：`89f07445`
实现提交：`c17d9bc7`（本批没有生产代码变更）

## 结论

R14-07 已按当前实现和隔离测试结果收口为“已满足，待环境验证”。本批复用现有 `Media` 窗口、`SelectedMedia`、`MediaId` 锚点、缩略图加载和视频回退链，只补定向运行证据，不扩展成跨页详情或新的媒体服务。

## 受控行为验证

- 选定 Playnite 套件 `46/46` 通过：`R14ClassificationSelectionTests`、`MediaWindowAnchorContractTests`、`AsyncThumbnailLoaderTests`、`AsyncThumbnailImageTests`、`R21AutomationValueBehaviorTests`、`R19AsyncContextSourceTests` 和 `R19DraftRefreshBehaviorTests`。
- 详情导航行为由 `PreviousMediaCommand`/`NextMediaCommand` 和当前已加载窗口边界覆盖；位置摘要按当前窗口显示，列表恢复继续通过 `MediaGrid.ScrollIntoView`/`BringIntoView`，窄布局关闭详情仍回焦原媒体行。
- 图片详情复用解码后的 `PixelWidth × PixelHeight`；缺失路径、不可用媒体、视频格式/`MediaFailed` 和 generation/cancellation 回退保持，不把失败升级为列表级错误。
- 当前提交隔离 Release 构建通过：XAML `24/24`、solution `0 error/2` 条既有 `MediaCenterView.xaml.cs:706 CS8602` warning，Playnite 目标 `net462`。
- `python scripts/validate-source.py`、`scripts/check-xaml.ps1 -ProjectRoot .` 和 `git diff --check` 在本批文档更新后通过。

## 受控视觉证据

- 人工检查既有 clean RenderHarness 截图 `artifacts/ui-qa-r13-r14-clean-20260922/Media-1040x700-tab1.png`：当前游戏媒体网格、有限视口/滚动条、选中项和“查看媒体详情”入口均可见；该截图没有打开详情面板，因此不把它写成详情内容、视频解码、焦点回归或最终呈现证据。
- 既有 RenderHarness 全报告仍有其他页面基线失败；本批不写成全局 `render-qa` 通过，也不把 offscreen 逻辑 DIP 当作物理 DPI、跨屏、presented frame 或宿主性能。

## 语义与边界

- 上一项/下一项只在当前已加载媒体窗口移动；跨页导航、真实视频编解码器和真实文件权限仍未验。
- 只使用合成 DTO、fake/隔离目录、隔离 WPF testhost 和既有 RenderHarness；未读取或写入真实存档、媒体、云端或外发诊断。
- 真实 Playnite/package-host、UIA/读屏/IME、物理 DPI/跨屏、最终呈现帧、ETW、宿主性能和超大真实媒体库仍未验；Demo 原目录不可用，视觉基准沿用恢复生产基线。

下一可执行小批量：核对并复核 `R14-08 来源规则试运行` 的现有实现与只读/限额/取消边界，再决定是否进入 `R15-01`。
