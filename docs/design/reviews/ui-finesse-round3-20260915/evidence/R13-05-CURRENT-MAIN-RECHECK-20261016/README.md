# R13-05 当前 main 远端证据复核

- 复核日期：2026-10-16
- main 文档 HEAD：`72b2cbb77e4be53e9550983fd685d82ca9a31f88`
- 最近包含产品代码的验证 HEAD：`964f325d3bed8e273de98e6d8532b1c5e5065fc7`

## 复核依据

R13-05 原实现 `e280cf1c` 后，云传输 DTO、维护页、Worker 投影和相关测试都随 R13-01/02/03/04 继续演进，因此仅引用 9 月原始实现证据不足。当前 R13-05 相关的六个文件（`CloudTransferDtos.cs`、`MaintenanceView.xaml`、`CloudTransferStateService.cs`、`UiDisplayMappingTests.cs`、`R13CloudTransferStageBehaviorTests.cs`、`CloudTransferStateTests.cs`）在 `964f325d` 到当前 `main` 之间没有变化；`964f325d` 是 `72b2cbb7` 的代码祖先，后续提交只改 R11 源码或文档。

当前受控行为由 [R13-03 当前 main 复核](../R13-03-CURRENT-MAIN-RECHECK-20261016/README.md) 中的现有测试覆盖，未重建服务、DTO 或命令：

- `CloudRemoteEvidenceKeepsUnknownFieldsUnknownAndRedactsCredentials` 与 `CloudRemoteDisplayCombinesRelativePathWithoutExposingQueryCredentials` 覆盖未知字段、URI 用户信息、query token 与 Bearer 凭据脱敏。
- `CloudStatusProjectsRemoteEvidenceAndKeepsHistoricalVerificationUnknown` 覆盖 `RemoteVerified` 成功校验时间与离开该状态后“未知”的保守语义。
- `MaintenanceInspectorShowsRemoteEvidenceAndKeepsDiagnosticCopyRedacted` 验证生产 `MaintenanceView` 的四项详情绑定和现有诊断复制 sanitizer；所属 `R13CloudTransferStageBehaviorTests` `14/14`。

## CI 与本机边界

[Actions #1040 / run `38880099277`](https://github.com/Nikilua/GameSaveCenter/actions/runs/38880099277) 在源码 HEAD `964f325d` 上双 SDK 终态成功：SDK8 job `116904483831`（SDK `8.0.423`）与 SDK10 job `116904484037`（SDK `10.0.401`）。两条 lane 的完整 Release build/test 成功；主 lane 的 WPF 隔离套件 `124/124` 类通过。该 run 对 R13-05 的证明有效，因为上述 R13-05 源文件自该验证 HEAD 到当前 main 未变；它不覆盖 R11-08 新代码。

当前执行环境只安装 SDK `9.0.302`，而仓库 `global.json` 锁定 SDK `8.0.423` 并禁用 roll-forward，所以本次没有用 SDK9 冒充本机仓库基线。没有修改 XAML 或生产代码，也没有另行生成截图/临时产物。自动化行为证据不是实际 Playnite/package-host 或最终呈现验证。

## 结论与未验边界

R13-05 继续标记为“已满足”。真实 Playnite、rclone 远端、物理 DPI/跨屏、独立 OS UIA/IME、presented frame、ETW 与宿主性能仍待外部验证；未接触真实存档、媒体、云端或用户数据。

下一可执行项为 R13-06：先核对当前 main 离线恢复状态与原有证据是否受后续改动影响；如果相关源路径未变化，沿用现有能力并补当前身份，不重建功能。
