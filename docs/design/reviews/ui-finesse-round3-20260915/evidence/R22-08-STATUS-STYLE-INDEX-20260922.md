# R22-08 状态样式一致索引

日期：2026-09-22  
状态：已满足，待环境验证  
代码提交：`d919179a`（补齐状态提示映射）

## 本批核对与实现

- 先核对现有能力：成功、失败、需关注已有共享 `StatusGlyphConverter`，TaskCenter 与 Maintenance 的状态文本入口已复用它；主题资源已有 Success/Warning/Error/Info 与对应图标几何，不重建状态服务或替换颜色体系。
- 缺口限定为共享文本线索：`执行中`、`运行中`、`进行中`、`加载中`、`同步中`、`上传中`、`下载中`、`处理中`、`校验中`、`检查中`、`验证中`统一返回 `ℹ`；`暂停`统一返回 `⚠`。原成功 `✓`、错误 `×` 和警告 `⚠` 词汇保持。
- 未知状态仍返回警示线索而不是成功线索；没有把 Unknown 当作完成、可用或健康，也没有改变业务状态值、DataGrid/列表绑定、选框、滚动条、命令和性能路径。

## 证据

- `R22StatusStyleIndexBehaviorTests 7/7`：实际调用共享转换器覆盖成功、警告、错误、运行、上传中、暂停六个桶；未知状态负例确认不含 `✓`，并核对 TaskCenter/Maintenance 均引用同一转换器。
- 状态/图标/Workspace 回归合计 `32 passed / 1 skipped`：`StatusGlyphConverterTests`、`R09IconSemanticBehaviorTests`、`WorkspaceStatePresenterBehaviorTests`、`WorkspaceStateSourceTests` 等定向集合；唯一 skip 为既有 legacy source skip，未改写。
- `scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot .tmp/r22-08-status-build-20260922`（提交身份 `d919179a`）：XAML `24/24`；Contracts/Playnite `net462`、Tests `net472`、Worker 构建 `0 errors`；保留既有 `MediaCenterView.xaml.cs:699 CS8602` 两条 warning。
- `python scripts/validate-source.py`、`git diff --check` 通过；WPF 静态检查 `0 errors / 27 warnings / 177 info`，与既有基线一致。

## 边界

- 证据使用合成状态字符串、现有 DTO/转换器和隔离 Release testhost；未把离屏/静态检查写成真实跨页颜色、图标呈现、Windows UIA/读屏、Playnite/package-host、OS 输入/IME、DPI/物理跨屏、呈现帧、ETW 或宿主性能证据。
- Demo 原目录不可用，继续沿用已恢复生产基线；未触碰 main 工作区改动，未读写真实存档、媒体、云端或外发诊断。

## 下一步

下一可执行任务进入 R23 发布收口，优先先核对依赖明确的 `R23-01` 每组可审阅交付：只整理本轮已完成项的事实、测试、边界和下一步，不把待验宿主能力写成通过。
