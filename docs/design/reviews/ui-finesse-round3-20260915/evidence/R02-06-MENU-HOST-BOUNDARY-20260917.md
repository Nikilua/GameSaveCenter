# R02-06 菜单状态完整证据

日期：2026-09-17

状态：外部宿主阻塞（插件可控契约已满足，Playnite 菜单呈现部分不可在当前工作区签收）。证据绑定提交：`8e48ac817b19a179721436a86837433b58d4babd`（`codex/ui-finesse-round2`）。

## 1. 入口核对

当前生产代码没有 WPF `ContextMenu`/`MenuItem` 资源或页面菜单树。插件唯一相关入口是 `GameSaveCenterPlugin.GetGameMenuItems(GetGameMenuItemsArgs args)`，把动作交给 Playnite 宿主渲染：

- 空游戏选择直接返回空序列，不创建无目标菜单项。
- 有游戏选择时按固定顺序提供“立即备份”“同步媒体”“查看备份历史”“验证最新恢复点”“游戏工具”“打开设置”六项，统一 `MenuSection="GameSaveCenter"`，每项都有 Action 委托。
- Action 委托没有在本阶段测试中执行；因此不会启动 Worker、写存档/媒体/云端或发出诊断。

这意味着禁用、勾选、危险色、子菜单箭头、快捷键列、键盘上下/左右/Enter/Esc、菜单边缘翻转和点外关闭的视觉/事件语义由 Playnite 宿主控制，不在本插件的 WPF visual tree 中。当前入口没有可安全复用的本地菜单服务或 DTO，未新增替代菜单体系。

## 2. 插件可控行为门禁

新增 `R02MenuHostContractTests`，用未初始化的插件对象和合成 `Game`，只枚举菜单描述，不触发 Action：

- `EmptyGameSelectionProducesNoHostMenuItems`：空选择 `0` 项。
- `SelectedGamesProduceOrderedHostOwnedQuickActionsWithoutExecutingThem`：两项合成游戏得到有序 `6` 项；每项的 `Description`、`MenuSection` 和非空 Action 均正确。

R02-06 定向 Release 测试 `2/2` 通过。测试是实际插件方法和 SDK 类型行为，不是对 `GameSaveCenterPlugin.cs` 做 `Assert.Contains`。

## 3. 构建验证与未验项目

- 完整隔离 Release：XAML `24/24`；编译 `0 warning / 0 error`；Core `83/83`；Worker `311/311`；Playnite `539 passed / 57 skipped / 0 failed`（总计 `596`）；源码校验通过。
- 本阶段没有生产 XAML 或样式修改，因此不重复运行 R02-05 的双主题 RenderHarness；最近一次生产资源渲染报告仍绑定前一项 `f03b4dd`，不能冒充本阶段菜单宿主证据。
- 由于当前工作区没有 Playnite 菜单 visual tree，不能签收 R02-06 的完整条件：禁用/勾选/危险/子菜单状态、长标签与快捷键列布局、上下/左右/Enter/Esc、菜单翻转、点外关闭和焦点返回。需要在用户明确允许的隔离 Playnite 宿主会话中安装当前包、选取合成或专用隔离游戏并人工/自动操作宿主菜单后补验；本轮不绕过宿主权限，也不触碰真实用户库。

证据范围是 Playnite SDK `GameMenuItem` 生成契约、合成选择和不执行 Action 的插件枚举。没有启动真实 Playnite，没有执行真实 OS 输入/屏幕阅读器/物理 DPI/IME/presented frame/ETW/宿主性能验证，也没有真实数据写入。下一可执行任务为 R02-07 异步菜单上下文，先核对 Action 捕获的游戏对象身份和失效中止语义。
