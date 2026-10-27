# U00-04 / U00-05 — 存档页间距节奏与状态样张

日期：2026-10-27。目标页为生产 `SaveCenterView`。视觉结构继续沿用 Demo-first 生产资源；本批只把存档页摘要区做成一页完整样例，没有迁移共享模板。

## U00-04 间距节奏

- 摘要卡内距由 `10,4` 调为 `12,8` DIP，底部间距由 `2` 调为 `8` DIP。上下间距对齐现有 8/12 DIP 步长（`GscSpacing2` / `GscSpacing3`），仍低于 14 DIP 的页面区段间距。
- 摘要中的上下文胶囊由 `Padding=9,4 / Margin=10,0,0,0` 调为 `Padding=8,4 / Margin=12,0,0,0`；两个辅助文本行间距统一为 4 DIP；操作组上间距由 10 调为 12 DIP。
- 保持摘要内容、命令、绑定、状态、筛选和操作顺序。1040×700 DIP 与 1366×768 DIP 复核没有新增控件行、文字换行或挤压；紧凑宽度原有的三组操作行继续存在。游戏选框、滚动条、表格列和详情面板未改。
- 间距前后截图可对照 [U00-02/03 的 Light 1040](../U00-02-03/screenshots/Save-light-1040x700.png)、[Dark 1040](../U00-02-03/screenshots/Save-dark-1040x700.png)、[Light 1366](../U00-02-03/screenshots/Save-light-1366x768.png)、[Dark 1366](../U00-02-03/screenshots/Save-dark-1366x768.png) 与本目录 `save-history-*` 样张；清单含尺寸和 SHA-256，见 [screenshots.json](screenshots.json)。

## U00-05 隔离状态样张

- RenderHarness 的 `statefixtures` 模式将 `SaveHistory` 加入既有生产页状态夹具，浅/深主题 × 1040×700、1100×720、1366×768、2560×1440 DIP，覆盖 Ready、Empty、Loading、Error、Stale、Offline 与双选长文本，共生成 56 张存档页截图。
- 空、首次加载、错误、离线夹具清空假的表格集合；Stale 夹具保留 12 条上次成功数据并显示过期提示。Ready 与 Empty 的可见空态、错误/加载/离线覆盖层、Stale 旧数据保留均由 harness 断言。
- 双选长文本样张在内存假数据上选中两行，备注含中英文、版本和路径共 113 字；检查 DataGrid 保持 2 行选中、备注使用字符省略，并将完整备注保留在 ToolTip。该样张不触发命令、不访问真实数据库。
- 代表性样张：浅色 Ready / Empty / Stale、浅色双选长文本、深色 Error、深色 1366 双选长文本；清单与哈希见 [screenshots.json](screenshots.json)。完整状态报告为 [statefixtures-report.txt](statefixtures-report.txt)。

## 验证

- SDK：仓库 `global.json` 锁定的 .NET SDK 8.0.423；使用宿主已有 SDK 目录，没有修改 SDK 基线。
- RenderHarness Release 编译：0 warning / 0 error。`render-qa` exit 0，报告 `render-qa OK` 且没有 `PROBLEM`；完整报告及 SHA-256 见 [render-qa-report.txt](render-qa-report.txt) 和 [screenshots.json](screenshots.json)。
- `statefixtures` exit 0，56 张存档页输出；验证六类数据状态与长文本/多选契约。最初一次样张捕获在分段内容完成布局前查找 DataGrid，暴露 harness 时序问题；加入 `UpdateLayout` 后重跑全部通过。
- 相关行为回归 3/3 passed、0 skipped：[u00-04-05-targeted-rerun.trx](tests/u00-04-05-targeted-rerun.trx)。该定向运行环境变量 `GSC_BUILD_COMMIT=7d50aa9201c59c97bf4f016d13df3a2823a376cd`，用于满足测试仓库身份约束；它不是本次工作树的提交SHA。
- 同期 R08 同页导航修复类 4/4 passed、0 skipped：[R08-same-page-fix-confirmed.trx](tests/R08-same-page-fix-confirmed.trx)。
- `python scripts/validate-source.py` 通过；`scripts/check-xaml.ps1` 通过（25/25）；WPF 静态审查扫描 31 个 XAML，0 error、26 warning、179 info，与 U00-01 基线一致。`git diff --check` 通过。
- U00-04/05 源码候选提交及其 GitHub SDK 8 / SDK 10 双 job 尚待本批提交后核验；本证据不代表最终 `GameSaveCenter-一键构建安装运行.cmd` 全链、独立 `scripts/build.cmd`、安装身份或真实 Playnite 宿主已验收。

## 边界

截图来自隔离 WPF 离屏窗口和假数据，不代表真实 Playnite 渲染、UIA 树、物理键盘/IME、系统物理 DPI 或实际帧率。未触发存档恢复、媒体删除或云端写入。真实宿主与最终构建门禁仍待验。
