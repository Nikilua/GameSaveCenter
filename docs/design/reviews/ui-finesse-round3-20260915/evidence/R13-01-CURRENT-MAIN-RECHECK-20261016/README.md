# R13-01 当前 main 队列阶段复核

日期：2026-10-16。生产代码基线：main `241bfb1605ef54c7a3d36247f3b2923f797d1073`；本批只增加当前 main 行为证据和测试，不改云端状态机、DTO、XAML 或生产命令。既有实现源自 `803470b8`，任务状态维持“已满足”。本次测试源码与当前构建源由下方 SHA-256 标识。

## 实际行为验证

- 本机 SDK `8.0.423`、MSBuild `17.11.48`、VSTest `17.11.1`；测试目标 Core/Worker `net8.0`（Worker 为 Windows TFM）、Playnite tests `net472`，本机 .NET Framework testhost `4.8.9181.0`。`scripts/build.ps1 -Configuration Release -SkipTests` 完整 solution build `0 warning / 0 error`，XAML `24/24`。
- Core `UiDisplayMappingTests` `40/40`：验证待队列、网络等待、普通重试、上传、校验中、上传成功等待校验、远端已验证等状态映射；认证失败不能映射成网络等待。
- Worker `CloudTransferStateTests` `14/14`：新增隔离 SQLite 状态机路径 `StartNew → MarkUploaded → MarkRemoteVerified`；上传后 DTO 是 `Uploaded` / “等待验证”、`LastSuccessfulVerificationUtc=null`；只有完成远端校验后才是 `RemoteVerified` / “已验证”并带校验时间。未连接真实远端或执行 rclone。
- Playnite `R13CloudTransferStageBehaviorTests + MaintenanceCloudTransferResolverTests` `21/21`。新增用例实际加载生产 `MaintenanceView`，选择“云端队列”页，在隔离 STA 窗口中通过真实 DataGrid 单元格、行选择和详情绑定验证：上传成功显示 `⚠ 等待验证` / `上传命令成功，尚未远端校验`；校验成功显示 `已验证` / `远端校验成功`。使用合成 DTO，不启动 DashboardViewModel 服务或任何命令。
- 首次独立 STA 尝试在窗口展示前因 testhost 缺少 Playnite 宿主提供的 `BaseTextBlockStyle` 资源而解析失败。按 RenderHarness 的中性资源 fallback 为测试 Application 补齐后，最终 Playnite 类 `21/21` 通过；这是夹具依赖修复，不是生产 XAML 或阶段行为失败。
- `python scripts/validate-source.py`、`git diff --check` 通过；WPF 静态审查 `0 errors / 27 warnings / 177 info`，均为现存全局审查项。没有 XAML 改动，不运行页面 Render QA。

## 证据身份

脱敏 TRX：

| 范围 | 结果 | 文件 | SHA-256 |
| --- | ---: | --- | --- |
| Core 映射 | 40/40 | [TRX](tests/R13-01-core-sdk8.trx) | `5C024B2D30DA1BCA6DB9A2CA98728D7C19FB9FAC1EACABC5C1A4A58A1D7727F2` |
| Worker 状态机 | 14/14 | [TRX](tests/R13-01-worker-sdk8.trx) | `5D14F48F4EBBBB453167527C3D66A4D6A8485981BE2EF2C6FBDFC8CC84212EEB` |
| Playnite 维护页 | 21/21 | [TRX](tests/R13-01-playnite-sdk8.trx) | `7F753A81F998060A6B11B4976C6B5F78E52EC46CA7ADEE3AFAF12466B52103C0` |

源码和构建程序集 SHA-256：

| 文件 | SHA-256 |
| --- | --- |
| `CloudTransferDtos.cs` | `E1D7C24385F37AE367A7C3C80B24ECF67E10B81DFE3D9F4AE8EA112E91E9684C` |
| `CloudTransferStateService.cs` | `58D7C06D7598743DB8CB37D04C908C951B70E463AF6B15EB5399EE9CD628D6CF` |
| `MaintenanceView.xaml` | `932C3364F133EF7AFAEDE4113BC2C784420A0AAAEA16D7545D752CAD290AB282` |
| `StatusGlyphConverter.cs` | `012C4F74980C44C635BAAEA0F29ECFD3957103795999432091E5EC9F3910645D` |
| `UiDisplayMappingTests.cs` | `0AC6DDF8BA38E343C319273AB7ED4385233EC49DC97627D7C5C18FBDE88947C8` |
| `CloudTransferStateTests.cs` | `B41F94E4B35130EC295C243C3CA24A1991AEC8FA072DD4F0B8CB00B9E4A8BDF8` |
| `R13CloudTransferStageBehaviorTests.cs` | `67E5030CB8AAA98D1F0FD662298F49D13CBAECA6E7FB5CD7DC68B50D9874796C` |
| `MaintenanceCloudTransferResolverTests.cs` | `9C545C7423FC3C6656AD7E145CA088775A9B47C212D9ED83E65695E1E17EC4A4` |
| Core test DLL | `B5C8F8A43BAEF5000CDF031C87B0B0552A1A9794B2CE7F091B796EB16C21BD56` |
| Worker test DLL | `42B60FBBC0CBB81AF1AA54EB7EC28222C1C25BB97BE2E843CFFE1488C0AC0CDE` |
| Playnite test DLL | `4E1FDA1CCE4FF4C579CE7F8046C91916AB64890574FF2821BFCE878E9DEAC20A` |

R12-05 生产投影修正提交 `241bfb16` 上重跑的 R00/R01 freshness 摘要为 `14/14 fresh`、`0 stale`、`0` 个登记源路径命中，比较 `2,856` 个路径；package identity 为 `not-provided`。精简机器结果见 [freshness JSON](../R12-05-CURRENT-MAIN-RECHECK-20261016/R00-R01-freshness-241bfb16.json)，SHA-256 `51C7AEC2C92C47B2A138F7FC94D9A4F6921120DF205309F6FB447C43C2468D92`。这次 freshness 只核到提交 `241bfb16`，后续测试提交后需再检查一次。

## 边界与下一步

TRX 内本机用户名、计算机名和绝对仓库路径已脱敏。数据只来自合成 DTO 和隔离 SQLite；未访问真实存档、用户媒体、真实云端或 rclone/Ludusavi。WPF 测试使用逻辑 `1440×1000 DIP` 隔离窗口，不是 Playnite/package-host 运行或最终屏幕帧，也不证明物理 DPI/跨屏、独立 OS UIA/读屏/IME、ETW 或宿主性能。Demo 原目录不可用，继续沿用恢复生产基线。下一项先跟踪已推送 R12-05 的 Actions，再核对 R13-02 下次重试时间已有实现和计时器生命周期。
