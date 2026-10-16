# R13-02 当前 main 下次重试时间复核

日期：2026-10-16
源码提交：`1e7ff4d8d121f46869c87d8315d466056344a1ae`（仅增加行为测试）
状态：**已满足**；既有实现来自 `6fd22892`，本轮不重建生产逻辑。

## 行为复核

- 当前 `CloudTransferStatusDto` 已复用 `NextAttemptUtc/NextAttemptLocal`，提供相对重试时间、带本地绝对时间的详情提示，以及无自动重试/已到期的明确文案。生产 `MaintenanceView` 将相对时间显示在选中队列记录详情中，并把完整本地/UTC时间放在 Tooltip 和 Automation HelpText；失败原因保留在同一条记录的原始诊断区。
- 新增测试接入现有隔离 STA 的生产 `MaintenanceView` 行为用例，复用一个页面/窗口验证上传、已校验和重试三条合成记录。选择网络失败重试项后，读取实际详情控件的相对文案、完整时间 Tooltip、Automation HelpText 和展开后的错误原因。将合成 `NextAttemptUtc` 改为已过期并重新选择后，控件显示“可立即重试”，不出现负倒计时。
- 时钟边界用“到期时间已过去”的合成状态验证属性重新求值后的钳制；没有调用操作系统改时 API，因此不证明系统时钟事件会令正在显示的 WPF 绑定立即重绘。重试展示路径没有逐行 DispatcherTimer：测试检查维护页代码后置与 DTO 源码没有计时器，并通过实际维护页详情行为复核绑定。

## 构建与测试

- 本机 SDK `8.0.423`，使用仓库保留的隔离 SDK 目录；MSBuild `17.11.48`，VSTest `17.11.1`。Core 测试目标 `net8.0` / runtime `.NET 8.0.29`；Playnite 插件目标 `net462`、测试 `net472`，本机测试宿主 `.NET Framework 4.8.9181.0`。
- Core `UiDisplayMappingTests.CloudRetryTiming*`：`2/2`；Playnite `R13CloudTransferStageBehaviorTests`：`14/14`。Release 测试构建同时生成当前 Playnite `net462`、测试 `net472` 及引用项目；输出隔离在本地 `.tmp`，不是完整 solution build。
- `python scripts/validate-source.py` 与 `git diff --check` 通过。没有改 XAML/布局，因此不运行 RenderHarness；没有声称真实 Playnite 宿主或最终屏幕呈现通过。

## 证据身份

脱敏 TRX：

| 范围 | 结果 | 文件 | SHA-256 |
| --- | ---: | --- | --- |
| Core 重试边界 | 2/2 | [TRX](tests/R13-02-core-retry-sdk8.trx) | `AF6286819C370F79ABF2BEEA24768D83B6D6F3006ED7FBCF8D3320D9581A9BF7` |
| Playnite 队列与维护页行为 | 14/14 | [TRX](tests/R13-02-playnite-behavior-sdk8.trx) | `52D1F4178D6332D3D7BAA7C39379ED829251AE2A1FD4296F74E1CF8CD554C347` |

| 文件 | SHA-256 |
| --- | --- |
| `R13CloudTransferStageBehaviorTests.cs` | `337F1F5276A121AE698FC50508D2854046D8A3BE18A272CEED965B537A60D9D5` |
| `CloudTransferDtos.cs` | `E1D7C24385F37AE367A7C3C80B24ECF67E10B81DFE3D9F4AE8EA112E91E9684C` |
| `MaintenanceView.xaml` | `932C3364F133EF7AFAEDE4113BC2C784420A0AAAEA16D7545D752CAD290AB282` |
| `MaintenanceView.xaml.cs` | `264CC3F4AAD92F44D4C32BCCCD83BD3D07FBAFB7C6B09CC47AC74042F96AEF9B` |
| Playnite test assembly | `C09C10BCE8E7B594473194761AD2CEEB7CB9870B89850C4E4A1291DBF67385C4` |
| Core test assembly | `125DE512623C9D61403C3DB170F4696068F0C591E24A78AF6B417EB8BFD9B542` |

R00/R01 freshness 在提交 `1e7ff4d8d121f46869c87d8315d466056344a1ae` 上为 `14/14 fresh`、`0 stale`、`0` 个登记源路径命中，比较 `2,861` 个路径；package identity 为 `not-provided`。精简结果见 [freshness JSON](R00-R01-freshness-1e7ff4d8.json)，SHA-256 `E628BBE2B4A7EAC6B3C51EFBB342E9C9C797624DD3E2CB6FE6263981F7FC23F`。该结果只说明登记源范围未要求重跑，不证明包身份或真实宿主运行。

## 边界与后续

只使用合成 DTO、隔离 STA 和隔离构建输出，没有访问真实存档、用户媒体、云端或外发诊断。隔离窗口为逻辑 `1440×1000 DIP`，不是 Playnite/package-host、物理 DPI/跨屏或最终 presented frame 证据；OS UIA/读屏/IME、真实系统时钟变更通知、ETW 与宿主性能仍未验。Demo 原目录不可用，沿用恢复生产基线。下一项为 `R13-03 手动重试范围`，先盘点已存在的重试入口、幂等身份和部分成功语义。
