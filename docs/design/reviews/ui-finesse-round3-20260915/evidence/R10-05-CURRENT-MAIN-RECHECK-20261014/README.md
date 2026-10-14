# R10-05 筛选预设当前 main 复核

更新：2026-10-14；分支 `main`。测试构建 `GscBuildCommit`=`0b4e64a72e2ec6d282fd935745accec5544551f4`；本轮源码由下文 SHA-256 固定，包含于本阶段提交。

## 已有能力与本轮补强

筛选预设已由 `005dc2c5` 引入。当前实现复用 `FilterPresetDefinition` 标量 DTO 和 `GameSaveCenterSettings.FilterPresets` 规范化：仅持久化 ID、名称、工作区与字符串筛选值；非法旧值回退/丢弃、重复 ID 去重、数量限制为 32。任务/媒体页已有保存、应用、重命名、删除入口；应用复用原有筛选属性、刷新与分页逻辑。任务工具栏的实际按钮绑定和窄窗 `GridLength.Auto` 也有既有行为覆盖。

审查历史测试后发现，重命名/删除确认只有源码字符串断言，没有执行取消分支。本轮加入内部 `FilterPresetConfirmationOverrideForAudit` 接缝；默认路径仍调用 `plugin.ConfirmAsync`。新负例调用生产 VM 的实际重命名和删除方法，使用隔离合成集合并返回“取消”，随后验证预设对象、顺序、名称、所选项及名称草稿均未改变；删除请求仍标记为危险操作。测试未设置插件实例，取消路径也没有进入设置持久化。

没有改 XAML、视觉样式、游戏选框、滚动条或任务查询/分页实现。

## 本机验证

- 本机只有 SDK `9.0.302`；仓库 `global.json` 锁定 SDK `8.0.423`，该版本未安装。测试从仓库外目录运行，使用 SDK9 和已有 `project.assets.json`，`--no-restore`；未改 SDK pin 或依赖。CI SDK `10.0.401` 由 Actions 单独验证。
- Release 构建成功：插件 `net462`、测试宿主 `net472`，Contracts/Core/HostAuditSeeder 引用项目均构建；捕获输出无编译警告/错误。VSTest `17.14.1`。
- `R10FilterPresetBehaviorTests`：`5/5`，0 失败、0 跳过；包含非法旧配置回退、标量 JSON 往返、实际 TaskCenterView 按钮命令和本轮重命名/删除取消负例。
- `R10FilterPresetBehaviorTests|TaskCenterViewResponsiveTests|Q14ToolbarAlignmentBehaviorTests`：`14/14`，0 失败、0 跳过。
- `scripts/validate-source.py` 与 `git diff --check` 通过。
- 脱敏 TRX：[`R10-05-current-main-sdk9302.trx`](tests/R10-05-current-main-sdk9302.trx)、[`R10-05-related-sdk9302.trx`](tests/R10-05-related-sdk9302.trx)。本机 `computerName` 与仓库绝对路径已脱敏。

## 构建身份

测试程序集元数据 `GscBuildCommit`=`0b4e64a72e2ec6d282fd935745accec5544551f4`；测试与插件程序集版本均为 `0.6.73.0`。

| 文件 | SHA-256 | MVID |
| --- | --- | --- |
| `DashboardViewModel.FilterPresets.cs` | `0C9F9F6C1AE0B17443BF460CD56432D3ABCDEEE7C7EECB5D86CBDBB4B3A57A87` | — |
| `FilterPresetDefinition.cs` | `41CA85AC24D002A0996EC75E6D00AFF2665FC9965C09D25185A3927E1CC237A8` | — |
| `GameSaveCenterSettings.cs` | `4C1D01668403FEBF21D2C55CB0F955A251FA34B44943873E3AC67077D721AEB0` | — |
| `TaskCenterView.xaml` | `00EBAC27AB45A858BBF9107DE60C64C28DA8F95C70CB3FB8425A6ACE5D65C0E2` | — |
| `R10FilterPresetBehaviorTests.cs` | `5AD3DFD9123E3A0660267196929270FB92576838AEBCC6674A5FD10FAEB7F419` | — |
| `GameSaveCenter.Playnite.dll` | `4A99A2B9B55F0FA218E8953C8FD1903C4A06EB1DEE224898DDABE872E752FBC2` | `a9aaf971-b748-4b66-a853-e9368a083210` |
| `GameSaveCenter.Playnite.Tests.dll` | `84B8967CD2CDAC00D4F356B1FAB8FDDC19CD5AE94E9F2330F35AA6D814555800` | `c9c75c6f-aa65-4e46-8a3a-b2df492e7cbf` |
| `GameSaveCenter.Playnite.Tests.pdb` | `C7898A1585905CA594FCEA086A48F967EA6A71EEC0AAA7AF18611630AD1D4A58` | — |

## CI 与外部边界

前置提交 `e3a127534f9418204be9f4716e7d085a5403dfa9` 的 run #991 (`38556833749`) 已终态失败：默认 SDK8 job `115827968644` 与 SDK10 job `115827968247` 的“编译与测试”均 exit 1。公开页面只显示通用 exit code；job logs 和 artifacts API 返回 HTTP 403 rate limit。Actions 页面列出的两个 artifact 为：

- `GameSaveCenter-build-diagnostics`，ID `11891870507`，582 KB，SHA-256 `3e7c7f32b58ee82b3ffbe088b52d7ac8831cf765c1ba1add721857c605693d19`。
- `GameSaveCenter-sdk10-build-diagnostics`，ID `11890810960`，505 KB，SHA-256 `4db5f65e1af318547d8c0184e6b0ef126ad7c2d3dda9c514c13c61bb2b85e49c`。

匿名网页显示 artifact 元信息，但下载路由返回 404；目前没有 #991 完整日志或包内容，失败根因未知。没有据通用 exit code 修改代码。上述 R10-05 本机验证不能替代 SDK8.0.423 或 SDK10.0.401 CI，也不能算作本轮代码已有远端 CI 结果。

测试仅使用合成设置、合成预设集合与隔离 net472/WPF 测试宿主；没有访问真实 Playnite 设置、存档、媒体或云端。未验证真实 Playnite 确认界面、UIA/读屏、真实键盘/IME、物理 DPI/跨屏、最终呈现帧、ETW 或宿主性能。此阶段无视觉变化，无需截图。Demo 原目录不可用，沿用恢复生产基线。

下一项：本阶段中文提交并推送后跟踪对应 Actions 至终态，再继续 R10-06 筛选来源提示的当前 main 适用性核查。#991 前序失败的完整日志继续尝试获取；取得后按完整日志复现和修复，不据通用 exit code 推断。真实 Playnite 与上述外部边界仍待验。
