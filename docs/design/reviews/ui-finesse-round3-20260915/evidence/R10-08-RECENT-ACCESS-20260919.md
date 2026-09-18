# R10-08 最近操作续接证据

日期：2026-09-19  
分支：`codex/ui-finesse-round2`  
范围：有限最近访问入口、稳定 ID 持久化、当前快照清理与 Overview 行为验证

## 结论

R10-08 已满足当前可控验收条件。

现有“最近任务”和“全局活动”保持原有数据契约与入口语义；新增的“最近访问”是独立卡片和独立命令。记录只保存 `PlayniteId`、工作区、TabIndex 和 UTC 时间，不保存游戏名称、归档路径、媒体路径或 DTO 引用。当前快照应用后，找不到的稳定 ID 会被清理；展示名称从当前 `Games` 快照解析，避免历史名称或本地绝对路径泄漏到标题。

## 实现事实

- `RecentAccessRecord` 在设置层提供最多 8 条记录，按稳定 ID 不区分大小写去重，按最近 UTC 时间排序，工作区和 TabIndex 做白名单/范围归一化。
- `GameSaveCenterSettings.RecentAccess` 只序列化上述标量字段；`DashboardViewModel` 在游戏选择或工作区变化时通过既有 `uiStateSave` 防抖保存，不写真实存档、媒体、云端或诊断数据。
- Dashboard 快照替换 `Games` 后立即清理不再存在的 ID，并将记录重新投影为当前游戏名称；`OpenRecentAccessCommand` 找不到对象时只清理记录并显示解释，不改选其他游戏。
- `OverviewView.xaml` 新增有限 `ListBox`：最大高度 `280 DIP`、Recycling 虚拟化、局部滚动和可聚焦命令按钮；“最近任务”仍使用原 `OverviewTasks` 列表，未合并两个概念。
- `scripts/validate-source.py` 增加该有限视口的明确白名单，防止 Overview 页级无限测量绕过大列表门禁。

## 实际验证

- `R10RecentAccessBehaviorTests`：`2/2`。
  - 合成 10+ 条记录验证 8 条上限、稳定 ID 去重、非法工作区/TabIndex 归一化、已移除 ID 清理和 JSON 不含 `GameName`/`Path`/`Archive`。
  - 真实 `OverviewView` + STA Window 视觉树验证“打开最近访问对象”按钮绑定到专用命令，命令参数保留稳定 ID，并通过 WPF `ButtonBase.OnClick` 执行一次。
- R10 组合测试：`18/18`（包含 R10-01～R10-08 相关行为与相邻回归）。
- 定向 Release 构建通过：`GameSaveCenter.Contracts`、`GameSaveCenter.Core`、`GameSaveCenter.Playnite`（`net462`）及测试项目（`net472`）均成功编译，测试输出无 warning/error。
- `python scripts/validate-source.py`：通过。
- `scripts/check-xaml.ps1`：`24/24`。
- `git diff --check`：通过。

## 边界

证据使用 synthetic DTO、隔离设置对象、隔离 STA WPF Window 和当前工作区快照；没有读取或修改真实存档、媒体、用户云端、用户配置或对外诊断。未启动真实 Playnite/package-host，未宣称最终 presented frame、物理 DPI/跨屏、UIA/读屏、真实键盘/IME、ETW 或宿主性能通过。Demo 原目录不可用，继续沿用已恢复生产基线。

用户提供的 main 合并后安装事实继续独立保留：编译/Core/Worker 成功，但 Playnite 全量为 `73 failed / 588 passed / 57 skipped`，安装器退出 1；本阶段未覆盖 dirty main、未把该混合宿主失败改写为 R10-08 证据。

## 下一步

下一可执行任务：R11-01 版本信息摘要。R10-08 之外的真实 Playnite 安装复验和上述宿主边界仍未验。
