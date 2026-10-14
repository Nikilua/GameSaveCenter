# R07-04 当前 main 横向输入与端点复核

日期：2026-10-15
分支：`main`
源码起点：`62672c94`；本轮行为改动在提交前按下文列出的工作树源构建和测试，提交后将再核对干净提交身份。
本机 SDK：`9.0.302`；VSTest：`17.14.1`。仓库 `global.json` 固定 SDK `8.0.423`，本机未安装该版本；测试命令从临时目录启动，所以只记录为本机 SDK9 结果，不代替 SDK8/SDK10 CI。

## 结论

原 `R07-04-HORIZONTAL-ENDPOINT-20260918.md` 的八组合探针已经覆盖真实生产 `DataGrid` 模板、端点夹断、末列/单元格可见、滚动条几何和左右往返；但它只用 `ScrollToHorizontalOffset` 驱动端点，并明确把 Ctrl/Shift 修饰输入与触控板留作宿主检查。因此旧账本“已满足”没有完整覆盖任务表的输入条件。

本轮补上共享 `DataGridHorizontalWheelBehavior`：Shift+垂直滚轮映射到水平偏移；Control 优先，Ctrl 与 Ctrl+Shift 不被该转换拦截；收到 `WM_MOUSEHWHEEL` 时，按屏幕指针位置只路由到命中的可见 DataGrid。使用现有 `DG_ScrollViewer` 和滚动条，不改列定义、命令、游戏选框或 Media 虚拟化。

## 验证事实

- Playnite 测试项目及引用项目 Release build：`net462`/`net472`，`0 warning / 0 error`。
- RenderHarness Release build：`0 warning / 0 error`；XAML 结构检查 `24/24`；`scripts/validate-source.py` 与 `git diff --check` 通过。
- 最终合并 STA WPF 行为运行共 `8/8`：`DataGridHorizontalWheelBehaviorTests` `1/1`、相邻 `MediaInboxScrollBehaviorTests` 与 `R07FineScrollBehaviorTests` `7/7`。新用例加载生产 Media 表格与模板，使用 2,000 条合成记录，验证 Shift 路径 `0→48→0 DIP`；Control、Control+Shift 和无修饰键的水平转换负例；合成 `WM_MOUSEHWHEEL` 小增量 `4.8 DIP`、表格内命中/表格外不处理、右端夹断及左端归零、再次越界不漂移。Media 行虚拟化保持 `Standard/Item`、列虚拟化关闭，2,000 项只实现 6 行容器。相邻用例覆盖滚动条几何、末行/末单元格、滚动锚点、虚拟化、反向精细滚动和边界传递。
- 重新运行 `horizontalprobe`：Light/Dark × Save/Task/Media/Maintenance 共 `8/8`。每个组合按真实生产 DataGrid 的 `DG_ScrollViewer` 检查负/超最大夹断、末列和末单元格在 viewport 内、横向条独立占位、回左端 offset 与首列几何无漂移。Media 仍报告 `Standard/Item/column=False`。输出来自离屏 logical DIP；构建时工作树未提交，故报告 `WorkingTreeClean=False`，提交后另行记录干净提交身份。
- [当前横向输入与相邻滚动回归 TRX](tests/R07-04-current-main-behavior-and-scroll-regressions.trx)：`8/8`，SHA-256 `2FE4DF5C2F27580590D93CA0E7BD697153749BC9BFEBF891D6A2959FCBCBB888`；已移除本阶段先前的过期分拆 TRX，文件仅保留本次最终合并运行。

测试将 `WM_MOUSEHWHEEL` 参数传给生产 hook 使用的同一处理函数，并在真实 WPF DataGrid/ScrollViewer 上观察 offset；没有用系统注入模拟物理触控板或 OS 窗口消息分发。故不宣称真实 Precision Touchpad、鼠标硬件、Playnite 宿主、UIA/读屏、物理 DPI/跨屏、最终呈现帧或 ETW/宿主性能已验证。未访问真实存档、媒体、云端或诊断数据。

## 下一步

截至 2026-10-15，旧 Actions #1008/run `38630493740` 与 #1009/run `38631425713` 均已 Failure；公开 job 页只显示状态，匿名 log API 返回 403、step-log 路由 404，主机未安装 `gh` CLI。用户附加的 #970/#971 diagnostics ZIP 来自更早 source commits，不是这两次 run 的日志，故失败根因未知。R07-04 提交 `aa49a569` 新触发的 #1010/run `38636778462` 含 job `116101841201`、`116101841930`，当前为 In progress，需继续跟 SDK8/SDK10 至终态；若失败先取得完整 job log，不凭摘要猜因。下一项 R07-06 当前 main 复核。
