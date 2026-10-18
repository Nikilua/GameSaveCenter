# S03-06 方案失效规则受控验证

## 实现

实现提交 `7366d8cfe76702350a4c397f8de8aeed8e482b38`。恢复方案草稿持久化 `Current` / `NeedsRevalidation` 状态和失效原因，并在详情或备份历史重新载入时对游戏/备份身份、文件数量与大小、内容指纹、readiness 检查结果和映射指纹重新比较。缺失目标仅在备份历史完整时标记失效。内容与 readiness 身份通过域分离 HMAC 保存，不持久化原始路径、内容指纹或自由文本摘要。草稿需重验时显示状态说明；实际恢复仍受现有 `RestoreConfirmationGuard` 和最新安全确认保护。

## 构建与验证

- 精确提交 `7366d8c` 的 SDK `8.0.423` Release solution build 成功，`0` errors。还原期间因漏洞数据源 `127.0.0.1:9` 不可连接产生 `17` 条 `NU1900` 警告；Playnite、Core、Worker 与全部测试项目均成功生成。
- 定向 Playnite 测试 `14/14`：`RestorePlanDraftRevalidationTests`、`R12RestorePlanDraftBehaviorTests`、`R12RestoreRevalidationBehaviorTests` 和 `R09FocusOutlineBehaviorTests`。TRX 保存在 [`tests/S03-06-targeted.trx`](tests/S03-06-targeted.trx)。因此当前精确提交的 R09 测试为 `2/2` 通过；这不能识别或解释用户旧失败运行中的源码/DLL/PDB，因为旧诊断材料不可用。
- `python scripts/validate-source.py .` 和 `scripts/check-xaml.ps1` 均通过（25 个 XAML 文件）。
- 精确提交的 RenderHarness 输出 `render-qa OK`，报告身份为 `7366d8cfe76702350a4c397f8de8aeed8e482b38`、`WorkingTreeClean: True`，覆盖浅色/深色和多个窗口尺寸。它是离屏渲染结果，不代表真实 Playnite 宿主验证。
- `scripts/dev-install-run.ps1 -NoStart` 隔离安装通过，版本 `0.6.73` / DLL `0.6.73.0`；没有启动 Playnite。安装摘要在 [`install-report.txt`](install-report.txt)。安装插件 DLL SHA-256：`2815DC4AB28C6765F1404455CF8F6CDB7723085B07C7F0AC07533151199F9867`；Worker DLL SHA-256：`541D410D7220D3B0776E11E6BD0F39E494D1346A5C4E04758FCA02440F71A5CD`。

## 边界

未运行完整 WPF 逐类隔离循环，也未启动真实 Playnite。用户提供的旧日志是测试断言失败（预期 `2,2,2,2`，实际 `1,1,1,1`），不是编译器错误；用户暂时没有对应诊断目录、`summary.json` 或 TRX，旧运行的源码与程序集身份无法确认。当前提交的 R09 `2/2` 通过不反推旧运行输入。工作区原有未跟踪 TRX 和 `TestResults` 未纳入证据包。
