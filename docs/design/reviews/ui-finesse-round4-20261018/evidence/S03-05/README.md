# S03-05 方案差异比较受控验证

## 结果

实现提交 `b2c1e3de5fa822bcd8f2b04ddfe8b66d41411457`。恢复草稿现在可以按明确的 A → B 方向比较来源、映射摘要、预计影响、准备状态和未知项。Worker 用 DPAPI 保护的本机密钥计算 HMAC-SHA256 映射指纹；草稿只保存指纹、版本和密钥 ID，不保存路径或密钥。密钥丢失、轮换或旧草稿缺少指纹时结果为未知；未知大小不会作为零值处理。相同备份版本的映射发生变化时会显示为不同映射。比较仅展示只读草稿，不授予或触发恢复执行。

## 测试、构建与安装

- SDK `8.0.423` Release 全解方案构建：`0 warning / 0 error`。
- `R12RestorePlanDraftBehaviorTests`：`7/7`；覆盖比较方向、同版本映射变化、未知状态和选择行为。
- `RestoreMappingPreviewBuilderTests`：`4/4`；覆盖规范化指纹、映射变化、未知大小以及序列化不包含原始路径或 HMAC 密钥。
- `python scripts/validate-source.py .` 通过；WPF 静态审查 `0 errors / 26 warnings / 177 info`。
- RenderHarness 浅色/深色完整矩阵 `render-qa OK`，包含目标尺寸 `1040×700` 和 `1100×720`。
- 精确提交 `b2c1e3de` 的干净临时 worktree 使用 `dev-install-run.ps1 -NoStart` 完成隔离安装：构建 `0/0`、XAML 结构检查 `25/25`、清单版本 `0.6.73`、插件 DLL 版本 `0.6.73.0`。流程未启动或更改用户的 Playnite。
- 安装插件 DLL SHA-256：`A54463FD3CF096935FDF46C800AE99C51DC1BCDCD3D2D33FD179E4F7D4A46F72`。
- 安装 Worker DLL SHA-256：`BA1CCDED8E3D14FEDCC57D5C226F8FF0DC232CA9F25B85D3A6886C495C74CEB3`。

## 边界

未运行完整 WPF 逐类隔离循环，也未启动真实 Playnite；RenderHarness 仅证明离屏渲染，不代表宿主呈现。用户粘贴的旧 R09 `2,2,2,2 → 1,1,1,1` 是测试断言失败，不是编译错误；用户暂时没有对应诊断目录、`summary.json` 或 TRX，旧运行的源码、测试程序集和 DLL/PDB 身份仍无法确认。当前工作区中的其它未跟踪 TRX 与测试结果目录属于用户既有文件，未纳入本证据包。
