# R22-01 本地镜像时间摘要校正

日期：2026-09-22  
工作区：`D:\workplace\github\GameSaveCenter`  
分支：`codex/ui-finesse-round2`  
代码提交：`b45e31dbd4ba1977f15d4ef09802594eabe16a5b`  
任务：R22-01「时间显示统一」的本地镜像残余入口

## 本批范围

- 复用已有 `LocalMirrorStatusDto.LastSyncRelativeDisplay` 与 `LastSyncFullDisplay`，将 `LocalMirrorService.StatusAsync` 的用户可见摘要从旧的 `LastSyncDisplay` 固定本地日期改为相对时间。
- 维护页紧凑镜像卡片的“最近同步”行直接绑定相对时间，Tooltip 和 UI Automation HelpText 绑定完整本地时区时间；保留镜像同步命令、只复制/校验、不删除镜像多余文件和现有滚动布局。
- `LastSyncDisplay`、`LastSyncRawUtcDisplay` 等兼容/复制所需投影未删除或改写；本批不扩大报告、日志和真实剪贴板语义。

## 行为与构建证据

- `LocalMirrorServiceTests`：`6/6`。新增合成 marker 场景：相对时间进入状态摘要，固定本地日期不再进入摘要，完整时间仍单独可读；禁用、目录不可用、复制校验、同尺寸哈希差异、取消和镜像多余文件保护均通过。
- `R22TimeDisplayBehaviorTests`：`30/30`。确认生产维护页不再把 `LocalMirrorStatus.Message` 作为“已同步”时间行，改用相对时间和完整时间帮助信息；未知时间仍为“尚未同步”。
- Release solution：XAML `24/24`，`0 error`，保留既有 `MediaCenterView.xaml.cs:699` 两条 `CS8602` warning；`git diff --check` 通过。

## 视觉/几何证据

- clean `shellqa` 报告：`artifacts/ui-qa-r22-01-localmirror-clean-20260922/shell-qa-report.txt`，`Commit=b45e31db…`、`WorkingTreeClean=True`、Light/Dark、offscreen logical DIP `1.00`；Shell `720/960/980/1040` 头部几何通过，Maintenance `1040/1100/1366` 紧凑检查均通过。
- clean 全量 RenderHarness 报告：`artifacts/ui-qa-r22-01-localmirror-full-clean-20260922/render-qa-report.txt`，同一提交身份和 clean 状态；Light/Dark Maintenance 1040/1100/1366/2560 的页面探针均无本批新增问题。
- 全量报告仍以 `FAILED` 结束，既有失败为 Overview 最近访问空列表、Settings 窄布局/状态夹具、Task/Save 窄视口行数门禁；不能写成全局 `render-qa OK`，也不能把离屏 logical DIP 写成真实 Playnite 呈现、DPI 或性能证据。

## 未验边界

本批只使用合成 marker、fake/RenderHarness 和隔离构建目录，没有读写真实存档、媒体、云端或用户诊断。真实 Playnite/package-host、Windows UIA/读屏、OS 输入/IME、物理 DPI/跨屏、最终 presented frame、ETW 和宿主性能仍未验；Demo 原目录不可用，继续参考已恢复生产基线。下一可执行任务回到 R23-04 UIA/Controlled host；宿主仍阻塞时推进其他独立 Q/R 小批量。
