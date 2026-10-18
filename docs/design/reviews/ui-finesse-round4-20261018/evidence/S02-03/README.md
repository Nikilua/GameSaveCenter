# S02-03 已读语义

实现提交：`6d3a6f437f1d51ec337e18e8f767b02afca17681`。通知收件箱现支持标记所选未读通知、仅标记当前筛选结果里的未读通知，以及在二次确认后清除已读通知记录。已读只表示查看状态；清理只删除 `notification_records` 中已读行，不改变任务状态，也不删除任务、审计日志或备份/恢复证据。

## 受控验证

- SDK `8.0.423` Release 全解方案构建通过：`0 warning / 0 error`。
- Playnite 定向测试 `18/18` 通过，覆盖读操作、页面命令绑定、筛选、R08 页面切换和 R09 焦点行为；R09 为 `2/2`。
- Worker SQLite `NotificationRecordPersistenceTests` 为 `7/7`，覆盖指定 ID 的未读更新、只清理已读项、重启持久化、保留任务/审计/备份证据和写请求 replay protection。
- Render QA `render-qa OK`。通知页双主题覆盖 11 种窗口尺寸，共 `22/22`；`1040×700` 浅色/深色均完整显示 `3/3` 行。全量报告及代表截图见 [render](render/render-qa-report.txt)。这是 OffscreenRenderHarness 的逻辑 DIP 证据，不代表 Playnite 宿主、物理 DPI 或实际屏幕像素。
- `python scripts/validate-source.py .` 与 `git diff --check` 通过；WPF 静态审查扫描 36 个 XAML，`0 errors / 26 warnings / 177 info`。
- DLL、PDB、源文件、TRX 与渲染输出 SHA-256 见 [verification-identity.json](verification-identity.json)；不声称 MVID。

## 边界与外机 R09 日志

完整 WPF 独立进程循环、真实 Playnite 安装/启动和宿主呈现未运行。

用户给出的诊断 `20261018T012631286Z-e027df9d` 显示 R09 测试断言失败，因此该日志不是编译错误。当前共享 TextBox 模板保留 `Chrome=1 DIP`，错误态另绘 `ValidationStroke=2 DIP`；本次提交后构建的 R09 为 `2/2`。用户暂时没有旧运行的 `summary.json`、TRX、源码版本和 DLL/PDB 身份，故不能确认旧运行实际输入；没有据该日志改粗生产边框或放宽断言。失败后的 `TextServicesContext` COM 清理异常不改变断言结论。
