# DEV-INSTALL-009 默认一键流程验收

日期：2026-10-20（本地时间）  
源码提交：`f70b806e21277f257f5b7b0f3eb7ae572cc66768`  
入口：`GameSaveCenter-Run.cmd` → `scripts/dev-install-run.ps1`  
诊断 ID：`20261019T184725162Z-607fe456`

## 结果

默认全流程退出码 `0`。精确 SDK `8.0.423` 下 Release solution build `0 warnings / 0 errors`；随后 restore、测试、win-x64 自包含 Worker 发布、打包、扩展安装与 Playnite 启动完成。

| 测试组 | 总数 | 通过 | 跳过 | 失败/错误/超时/中断 |
| --- | ---: | ---: | ---: | ---: |
| Core | 157 | 157 | 0 | 0 |
| Worker | 383 | 383 | 0 | 0 |
| Playnite source | 514 | 496 | 18 | 0 |
| Playnite WPF isolated | 758 | 718 | 40 | 0 |
| **合计** | **1812** | **1754** | **58** | **0** |

WPF 隔离运行 `137/137` 个测试类。140 个 TRX 文件均可解析，TRX 明细与汇总计数无差异。重点复核的 `R09FocusOutlineBehaviorTests` 为 `2/2`、`R10KeyboardShortcutHelpBehaviorTests` 为 `5/5`。已按桌面输入能力预检并跳过无法从非交互隔离桌面可靠执行的指针用例。

## 安装身份

- 包：`artifacts/GameSaveCenter-0.6.73-playnite.zip` 与 `artifacts/GameSaveCenter-0.6.73.pext`
- 安装目录：`%APPDATA%\Playnite\Extensions\GameSaveCenter_66e9f2d7-67bb-43ef-b62a-b8e60734fcec`
- 清单版本：`0.6.73`；插件 DLL：`0.6.73.0`
- 构建 DLL 与安装 DLL SHA-256：`AC87FDC16F4FF7211CC00C6B0B8135711D32D20F6C338A36F09B086972535A31`
- Playnite 已启动且响应（PID `11068`）。

## 修复与边界

系统只安装 SDK `9.0.302` 时，旧安装器在编译前拒绝运行。当前 `DEV-INSTALL-009` 保留 `global.json` 的精确 SDK `8.0.423` 要求，缺失时通过微软官方安装脚本写入当前用户 SDK 缓存；首次安装需要访问微软下载源。

全量 WPF 测试此前因逐类运行产生的 shadow-copy TEMP 目录没有回收而填满 D 盘。runner 现在在每个成功类后，只删除隔离 TEMP 根目录中由本轮新建的临时项，并保留失败类现场。帮助 Popup 测试也在清理时关闭窗口；runner 为 WPF 子进程增加 2 分钟 hang 定位。

原始本地日志：`artifacts/one-click-install.log`、`artifacts/last-dev-install.txt`。完整 JSON、控制台日志及 TRX 位于 `artifacts/build-diagnostics/20261019T184725162Z-607fe456`。这些是可再生本地产物，不纳入 Git。

本次确认了源码构建、测试、包身份、安装和 Playnite 进程启动；没有进行真实窗口页面的视觉验收。干净的新机器仍须有网络访问微软 SDK 下载源。
