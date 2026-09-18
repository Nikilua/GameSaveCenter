# R10-05 筛选预设证据（2026-09-19）

## 结论

R10-05 在当前续作分支的受控验证范围内已满足。实现提交为 `005dc2c5`（`补充筛选预设与确认操作`），已推送到 `origin/codex/ui-finesse-round2`。本阶段先核对了现有能力：`PolicyTemplates` 是备份策略模板，不等同于筛选预设；任务和媒体页已有筛选状态、分页/取消路径与收件箱模式，因此只补预设存储和页面入口，没有重建服务或 DTO。

## 实际实现

- 新增 `FilterPresetDefinition`，仅持久化预设 ID、名称、工作区和字符串筛选值：任务搜索/状态/游戏/类型/历史范围，媒体搜索/类型/收件箱模式。没有 `TaskStatusDto`、`GameStatusDto`、`MediaItemDto` 或临时 ViewModel 对象引用。
- `GameSaveCenterSettings.FilterPresets` 在赋值和 JSON 往返时做防御性规范化：空名称、未知工作区、重复/非法 ID、非法状态/范围/媒体值回退或丢弃；总量上限为 32 条，文本有长度上限。`CopyFrom` 走同一列表规范化，便携设置不会把 live object 带入配置。
- 任务页和媒体页复用现有工具栏/收件箱布局，新增保存、应用、重命名、删除入口。重命名与删除调用既有 `plugin.ConfirmAsync`；同名保存也先确认覆盖。应用仍通过现有筛选属性触发原有刷新、分页、取消和错误语义。
- 任务页响应式重排原本每次把第二行设为 `0` 高度；已修正为 `GridLength.Auto`，否则紧凑窗口会静默隐藏预设入口。游戏选框、滚动条系统、有限列表虚拟化、Playnite/net462、命令绑定和安全语义未改。

## 验证

- `R10FilterPresetBehaviorTests`：`4/4`。覆盖非法旧配置回退、重复 ID、标量 JSON 往返、排除 live DTO 字段、真实构造的 TaskCenterView 预设 ComboBox/按钮绑定与紧凑响应式行保持可测量，以及重命名/删除必须确认的接线负例。
- R10 相邻定向回归：`14/14`；包含 R10-01 到 R10-04 的返回、定位、搜索作用域和帮助行为。
- 与本阶段直接相关的设置、状态、可访问性、任务响应式与预设回归合计：`25/25`。
- 最终隔离 `scripts/build.ps1 -SkipTests -OutputRoot D:\workplace\github\GameSaveCenter\.tmp\continuation-r10-05-build-final-20260919`：Playnite `net462`，solution `0 warning / 0 error`，XAML `24/24`；`validate-source.py`、`check-xaml.ps1`、`git diff --check` 通过。
- 全量阶段门禁曾在同一 net472 testhost 得到 Core `83/83`、Worker `311/311`，Playnite `84 failed / 610 passed / 57 skipped`。失败栈主要为既有 PresentationSource/STA WPF 资源与动画时序、DataGrid 生成和旧源码契约问题；本阶段不把该全量结果改写成通过，R10-05 的可交付结论只引用上面的直接相关 `25/25` 与 R10 `14/14`。

## 边界与未验事实

- 验证使用合成标量设置、fake binding、隔离 STA WPF Window 和临时 testhost；没有读取或修改真实存档、媒体、用户云端、真实 Playnite 配置或对外发送诊断。
- 预设中的任务游戏筛选沿用现有查询契约的字符串游戏名；它不是 live `GameStatusDto` 引用。后续若把任务游戏筛选升级为稳定 PlayniteId，需要单独迁移现有查询和 UI 选项，不能在本项证据中提前宣称。
- 未在真实 Playnite 中验证保存/确认 Popup、最终 presented frame、物理 DPI/跨屏、UIA/读屏、真实键盘/IME、ETW、宿主性能或 package-host 安装。Demo 原目录不可用，沿用恢复生产基线。
- 用户提供的 `DEV-INSTALL-008` main 日志仍独立记录为编译 `0/0`、Core `83/83`、Worker `311/311`、Playnite `73 failed / 588 passed / 57 skipped`、安装器退出 1；本阶段未在 dirty main 上覆盖、合并或重跑安装器。
- D: 本阶段 5 个隔离目录中 4 个已按精确路径删除；`continuation-r10-05-build-20260919` 的部分 VBCSCompiler analyzer DLL 仍被锁，未强杀未知进程，也未把临时产物提交。

下一可执行任务：R10-06 筛选来源提示。真实 Playnite 保存/确认交互与 main 合并后单 checkout 安装器复验仍未完成。
