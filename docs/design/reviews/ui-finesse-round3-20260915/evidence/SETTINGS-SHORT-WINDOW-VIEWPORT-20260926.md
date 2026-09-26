# Settings short-window viewport repair — 2026-09-26

## 问题与修复

Task 阶段之后完整 RenderHarness 还剩 8 个 Settings findings，均在 560 DIP 高度：category rail 的 6 条越界告警，以及 760/880/920 宽度的主体 viewport 128/128/144 DIP。

category ListBox 开启 `CanContentScroll`，其 `ScrollViewer.ViewportHeight` 返回逻辑项目单位（4 或 5），不是 DIP。修正 RenderHarness 为相对实际 `ScrollContentPresenter` 测量和比较 DIP；保留末项滚动至末端的可达性检查，消除单位误用造成的 6 条误报。

实际空间问题来自 Settings 顶部始终展开的恢复默认说明/操作卡。保留原有安全说明、单字段/全部默认真实命令和当前草稿语义；将内容放入共享 `GscExpander` disclosure，在宿主高度 `<760 DIP` 时自动折叠，短高下恢复时腾出正文空间。跨越短高阈值时自动恢复正常高度状态，同时在同一布局模式内尊重用户手动展开/折叠。

## 当前源码实测

RenderHarness Release 使用 checkout `b85e53ed1e7e97cba9351fd0db8ab06caafee2f0` 构建，报告标记 `WorkingTreeClean=False`、DPI scale `1.00`（离屏逻辑 DIP）。完整双主题、多尺寸、页面与壳层运行生成 372 张临时 PNG，退出码 0，报告结尾 `render-qa OK`，没有 `PROBLEM`。

Settings 560 DIP / 宽 760、880、920、1100、1400 样本的 category `ScrollContentPresenter` 为 174.4 DIP；末项类别滚动至末端后底边为 165.6 DIP（逻辑 `ViewportHeight=4`，不再与 DIP 混比）。同组主体 viewport 为 185.6 DIP。Settings 布局探针在 700/900 DIP 高度亦无类别可达性问题。

Release 隔离输出构建（复用已有 restore assets、`--no-restore`）结果：RenderHarness `0 warnings / 0 errors`；Playnite.Tests 与 Playnite 依赖 `0 warnings / 0 errors`。定向 WPF 短窗行为用例 760/920 两个 case 均报告通过；设置标题/路径布局 Light/Dark `2/2`；带正确 `GSC_BUILD_COMMIT`/`GSC_SOURCE_ROOT` 的 R16 恢复默认源码契约 `1/1`。Source validation、XAML `24/24`、`git diff --check` 通过。

普通 solution restore 被当前用户配置文件 ACL 阻止读取 `%AppData%\NuGet\NuGet.Config`；没有改权限或重试提升操作。使用项目已有资产及 `.tmp` 下独立 `BaseOutputPath` 完成本阶段 Release 构建，避免覆盖被其他进程占用的默认 `bin`。一次未附 build identity 的源码测试按设计拒绝读取源码，补齐 checkout identity 后该测试通过。

扩展运行 `ReportedWorkspaceLayoutBehaviorTests` 还独立暴露 SaveHistory 窗口化间距断言：Light/Dark 实测动作与内容间距均为 `1.6 DIP`，测试期望 `8–14 DIP`。单独运行仍可复现；Settings 和 SaveHistory 源码彼此独立，此问题不计入 RenderHarness finding，作为后续独立阶段诊断/修复，不在本阶段混改。

## 验证边界

RenderHarness 与 WPF 用例证明当前 checkout 的离屏布局/逻辑 DIP，不证明 Playnite 宿主、用户安装包、物理 DPI/跨屏、UIA/读屏或最终 presented frame。未启动 Playnite；CEF `platform_channel 0x5` 与真实宿主边界不变。R 台账仍为 192 项，状态 `106/83/1/1/1`，本阶段不改账本状态。
