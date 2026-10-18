# S07-05 跨机器诊断入口

## 现有能力与补充

复核 `scripts/build-diagnostics.ps1`：已有 collector 已记录 source commit、dirty paths、已登记源码哈希、SDK、build/test steps、程序集 DLL SHA-256/MVID、同目录 PDB 身份，以及原始 TRX；CI 已上传 summary/log/TRX。无需再写一套采集器。

本阶段在现有 `summary.json` 增加脱敏的环境身份：OS version/bitness、PowerShell version/edition、进程 bitness 与 .NET runtime version；不记录用户名、绝对仓库/用户目录、凭据或用户数据。新增 `scripts/preview-build-diagnostics.ps1`，从现有 diagnostics summary 生成本机只读预览，列出源码提交/脏路径、SDK/环境、失败步骤、DLL/PDB SHA/MVID、测试状态与原始 TRX 可用情况。新摘要直接读取已保存的 `testSummary`；旧摘要没有此字段时可从同目录原始 TRX 只读重算。脚本不启动构建/测试、不联网、不写文件或访问 Playnite/用户存档。

## 验证

- `pwsh -NoProfile -File scripts/tests/Test-BuildDiagnostics.ps1 -OutputRoot .tmp/gsc-s07-05-test`：通过，包含环境身份断言、既有 PDB/脱敏用例，以及 preview 正/负例。
- `pwsh -NoProfile -File scripts/preview-build-diagnostics.ps1 -Path docs/ai/evidence/close-sdk-20261007/local-sdk8-full/summary.json`：成功只读预览历史 SDK8 诊断；现场由 27 个 TRX 复算 1,073 用例（1,054 Passed、1 Failed、18 Skipped），WPF isolated 为 24 类/103 用例。原始记录的 Q14 失败仍清晰显示为失败；旧摘要缺环境信息时明确标记未记录，不从当前主机补推。
- preview 合成用例验证错误 step、DLL/PDB、WPF 类/用例、TRX 文件缺失提示、旧格式字段缺失与敏感字符串不显示。
- 测试临时目录限定于仓库 `.tmp/gsc-s07-05-test` 并已删除；没有完整产品 build、宿主访问或安装包采集。

## 使用

```powershell
pwsh -NoProfile -File scripts/preview-build-diagnostics.ps1 -Path artifacts/build-diagnostics
```

参数可指向单次 run 的 `summary.json` 或 diagnostics 根目录（会选择最近更新的一份摘要）。预览依赖现存本地/CI artifact；缺项会说明它未被记录、测试尚未开始或原始 TRX 不在当前目录。若需要核对另一机器上实际加载的插件 DLL，仍需该机器提供其安装包/加载路径身份；本入口不会检查运行中 Playnite。
