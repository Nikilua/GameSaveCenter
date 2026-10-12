# R02-06 / R02-07 当前 main 源码边界复核

日期：2026-10-12。当前 `main` 为 `3df542af20089ca91733f93d814df5b4c6373169`。该提交相对最近成功本机 Release 复验的源码身份 `90099a09104677708a5829cf7e6a5d22b6b414ff` 只改动文档和证据，生产源码与测试文件没有变化。

## R02-06 插件契约与宿主所有权

- 当前入口仍为 `GameSaveCenterPlugin.GetGameMenuItems`，向 Playnite 返回六个 `GameMenuItem`。插件可控测试覆盖空选择返回 0 项，以及合成多选按固定顺序返回 6 项、统一 `GameSaveCenter` 分组且 Action 非空；测试不会执行这些 Action。
- 现存 `R02MenuHostContractTests` 结果为 `2/2`。在 `c55f7a9f16276fe651f3e9ae845a8e5687cb21e9` 的菜单异步上下文提交上，R02 menu 合并定向结果为 `4/4`；该结果包括上述两项宿主契约测试。当前 `GetGameMenuItems` 相关代码、`GameMenuActionContext` 及两组测试自该提交以来未变。
- 禁用、勾选、危险样式、子菜单、快捷键列、菜单定位/翻转、键盘输入和关闭/焦点返回属于 Playnite 宿主 visual tree。当前插件没有自有 WPF 菜单，不能以插件契约测试冒充完整 R02-06 视觉和输入验收，状态继续为“外部阻塞”。
- ENV-001 最近事实仍是隔离 runner 对 `Win32_Process.CommandLine` 查询被拒绝，且已有 Playnite CEF `platform_channel 0x5` 启动失败记录。条件未变，本轮没有再启动或绕过权限。

## R02-07 插件可控的异步目标身份

既有行为测试证明菜单建立时捕获有序 Guid 快照；列表刷新后以新对象引用按原 ID 顺序重新解析；任一目标被删除时返回缺失 ID、清空解析结果并中止。五个目标相关 Action 在 Worker/Upsert 前走该解析路径，不回退到当前选中行。此受控插件功能保持“已满足”；宿主打开菜单后的刷新/删除时序和实际输入仍未验。依据为 [R02-07 原证据](../R02-07-MENU-ASYNC-CONTEXT-20260917.md)。

上述结果是复用已有测试与当前源码未变核验，不是本轮对当前 `3df542af` 新运行的 `4/4`。本轮没有执行任何菜单 Action 或触碰真实存档、媒体、用户云端。

## 本轮执行限制

- 仓库 `global.json` 锁定 SDK `8.0.423` 且 `rollForward=disable`；系统当前只发现 `9.0.302`。`scripts/build.ps1 -Configuration Release -OutputRoot .tmp\r02-menu-current-main-20261012` 在 `sdk-baseline.ps1` 预检时退出，未开始编译或测试。没有用 SDK9 替代。
- `python scripts/validate-source.py` 在当前 `3df542af` 通过。当前提交未改源码，不把此源码校验写成本机 Release build 或 R02Menu 新测试结果。
- 推送提交 `3df542af` 的 Actions run [#934](https://github.com/Nikilua/GameSaveCenter/actions/runs/38249983091) 已在 CI SDK `10.0.401` 下终态失败：SDK 安装、结构检查、诊断自测和 SDK 基线步骤成功；“编译与测试”失败，离屏渲染、WPF 静态审查和打包跳过。run ID 为 `38249983091`、job ID 为 `114813463440`。页面仅给出 exit code 1，未取得失败命令、断言或堆栈。
- Jobs/log API 的 jobs 与 artifact 元数据可读，但日志 endpoint HTTP `403`；artifact `11747469284`（`GameSaveCenter-build-diagnostics`，488,428 bytes，SHA-256 `662aba958e53e7a6d8ccd837a319fd93e8df91d2309f7977b0af1a84935f79fa`）压缩包下载 HTTP `401`。根因未知，不推断或盲修。

## 下一步

R02-06 只有在 ENV-001 前置变化后才重跑 fail-closed 隔离宿主预检；成功后用隔离 profile 与合成游戏观察菜单长标签/快捷键列、上下左右导航、翻转和 Escape/焦点返回。若验证 Enter，只用隔离 profile 的“打开设置”非写入项；不要触发备份、同步或恢复 Action。R02-07 继续沿用现有行为证据。当前本机需先恢复仓库要求的 SDK `8.0.423`，之后才能补跑 R02Menu 测试与可执行的 CLOSE-WRAP-01 复测。
