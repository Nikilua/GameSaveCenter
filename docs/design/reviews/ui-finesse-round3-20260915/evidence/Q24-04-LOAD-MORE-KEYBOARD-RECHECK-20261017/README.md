# Q24-04 加载更多键盘路径与 R00-08 freshness 复核

日期：2026-10-17。当前 `main` 父提交：`cbdc29ad1913b759a64d4835df5da04e01142fc0`。本批只修改 `R21KeyboardNavigationTraceTests.cs`，不改生产 XAML、ViewModel、服务、DTO 或命令。

## 已有能力与补充点

`TaskCenterView` 已将“加载更多”绑定到 `LoadMoreTasksCommand`，并由 `TaskHistoryHasMore` 控制显示。原失败任务状态键盘轨迹覆盖了筛选、DataGrid、选中行技术详情和重试命令，但夹具固定 `TaskHistoryHasMore=false`，因此隐藏状态下没有验证可见页尾命令是否能通过 Tab 到达。

本批继续复用生产 View 与现有命令契约，新增正负两种合成状态：

- `TaskHistoryHasMore=true`：确认按钮可见、命令引用保持绑定、Tab 与 Shift+Tab 均能到达，并位于 DataGrid 之前；失败任务的既有详情/重试路径仍按正反向顺序检查。
- `TaskHistoryHasMore=false`：确认按钮折叠、命令绑定保留、按钮不进入正向或反向焦点轨迹，也不出现在当前可见交互目标中；焦点不离开 TaskCenter 测试宿主。
- 新旧命令均为 no-op 合成命令，没有执行加载、重试、取消或外部 I/O。

## 本机验证

使用当前工作区隔离构建输出，SDK `9.0.302`、VSTest `17.14.1`、Playnite `net462`、测试 `net472`；由测试项目及引用项目构建，非完整 solution build。WPF 类按独立 testhost 逐类运行：

| 测试类 | 结果 |
| --- | ---: |
| `R21KeyboardNavigationTraceTests` | 4/4 |
| `KeyboardFocusSourceTests` | 5/5 |
| `GamePickerKeyboardBehaviorTests` | 7/7 |
| `R10KeyboardShortcutHelpBehaviorTests` | 5/5 |
| 总计 | 21/21 |

全部 21 项通过，0 失败、0 跳过。原始 TRX 已去除机器名/用户名，保存在 [`tests/`](tests/)；这些文件同时复核了 R00-08 登记的 `*Keyboard*.cs` 源码范围。源码结构检查与 `git diff --check` 通过。

构建程序集包含父提交 `cbdc29ad` 的 `GscBuildCommit` 元数据；编译时 R21 测试文件尚未提交，内容 SHA-256 为 `789B56AFEDF343DFBE6BC98B14E126D20F3CC057D27D2BC49018F8E611C33D11`。测试 DLL SHA-256 `A6520D3449BABD303DCD0984005240A1974DDB806E4A94AB2E2E476127B3929B`，PDB SHA-256 `C08E22B44E82839D2D4513280CA908F6C19204D11150F99BE44355E798DB6C4B`，MVID `b599b620-0d58-4a40-b385-2368ef80a88d`。生产插件 DLL SHA-256 `57369BAE11768B1AE5ADE13AAACEC9CFBE00ECD4EB9BB3BFED480E8E11F1EA2F`，PDB SHA-256 `588D8CE4A23A87E22A1D87885738F0838CFDCD6611D8363FD871031DA11422DC`。其余源码、DLL 与 TRX SHA 见[身份清单](hashes-sdk9302-cbdc29ad.txt)。

这些 STA WPF 自动化结果只证明合成内容下的 `MoveFocus` 路径；没有运行真实 Playnite、物理 Tab/Shift+Tab、Narrator/UIA 客户端或视觉验收，因此 Q24-04 与 Round2 总体仍未完成。
