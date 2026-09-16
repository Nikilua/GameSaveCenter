# R02-07 异步菜单上下文证据

日期：2026-09-17  
状态：已满足（插件可控部分；Playnite 宿主实际触发仍未验）  
代码提交：`4d4bb93`（`codex/ui-finesse-round2`，干净工作树）

## 任务边界

任务要求菜单打开后固定操作对象身份，列表刷新或对象删除时不得因底层选中行变化而改操作另一对象；目标失效时必须明确提示并中止。先核对当前生产入口：本插件把菜单交给 Playnite 的 `GameMenuItem`，并不存在可由本工作区控制的 WPF `ContextMenu/MenuItem`。本阶段只收口插件动作在异步执行前的身份解析，不把 R02-06 的宿主菜单视觉和输入条件冒充为已验。

## 实现与负例

- 新增 `GameMenuActionContext`，菜单生成时按目标顺序保存 `Guid` 快照，不保存可能被 Playnite 刷新的可变 `Game` 引用。
- 立即备份、同步媒体、查看备份历史、验证最新恢复点和游戏工具五个目标相关 Action，在启动 Worker 或执行 `UpsertGames` 前，使用当前 `PlayniteApi.Database.Games` 按快照 ID 重新解析，并保持原菜单顺序；“打开设置”没有游戏目标，维持宿主设置入口。
- 任一快照 ID 已从当前列表消失时，解析返回失败，Action 在 Worker/Upsert 前返回，并通过既有通知路径提示“上下文菜单目标已失效……未执行操作。请重新打开菜单后重试。”；无法确认当前列表时同样记录日志并提示中止，不降级为当前选中行。
- `R02MenuActionContextTests` 不是字符串签收：刷新正例使用替换后的新对象和重排列表，断言按原 ID 顺序解析到新引用；删除负例断言缺失 ID、空解析结果和 `false`。

## 验证结果

1. `dotnet vstest .tmp\\r02-07-build-final3\\bin\\GameSaveCenter.Playnite.Tests\\Release\\net472\\GameSaveCenter.Playnite.Tests.dll --TestCaseFilter:"FullyQualifiedName~R02Menu" --logger "console;verbosity=minimal"`：R02-07 菜单上下文与宿主契约共 `4/4`，`0` 失败、`0` 跳过。
2. `scripts/build.ps1 -Configuration Release -OutputRoot .tmp\\r02-07-build-final3`：XAML `24/24`；解决方案构建 `0 warning / 0 error`；Core `83/83`；Worker `311/311`；Playnite `541` 通过、`57` 跳过、`0` 失败，总计 Playnite `598`。
3. `python scripts/validate-source.py`：通过，覆盖 JSON/XML/YAML、XAML 语义与资源、C# 分隔符、解决方案、IPC 常量、交付保护、媒体/游戏工具 SQLite 保护、大库性能保护和 Windows 启动器。

首轮全量复跑曾暴露一条随参数重命名而过期的 `QuickActionSourceTests` 源码契约断言（仍期待 `SyncMediaFromQuickActionAsync(games)`）；已在独立提交 `4d4bb93` 校正为 `context`，随后以该干净提交完整复跑通过。这个过程事实保留，不把首轮失败隐藏成产品回归。

本阶段没有生产 XAML/样式变更，因此不重复运行 RenderHarness；R02-05 的双主题渲染报告仍独立绑定其代码提交，不能作为本阶段宿主菜单呈现证据。测试使用合成 `Game`、隔离 Release 程序集和现有插件契约，不执行菜单 Action，不启动真实 Worker，不写真实存档、媒体、用户云端或诊断数据。

## 未验边界与下一步

尚未启动真实 Playnite 会话，未用 OS 鼠标/键盘打开菜单或观测宿主在菜单打开后刷新/删除对象的事件时序；因此 Playnite 菜单的禁用、勾选、危险、子菜单、快捷键列、键盘导航、翻转、点外关闭、焦点返回，以及物理 DPI、IME、屏幕阅读器、presented frame、ETW 和宿主性能仍未验。下一可执行任务为 R02-08 动作文案动词化。
