# S06-07 清单脱敏导出预览交付

## 实现

提交 `96a3eab3` 为任务中心的工作清单增加“预览导出”。用户先查看只读文本，再明确复制。文本只允许清单名、对象类型、当前摘要、所属游戏摘要、引用状态和人工备注；控制字符经单行化，本机路径、URL 参数与认证字段沿用共享清理器处理。默认列出所有未手动完成项，保留失效、待刷新和暂不可检查的引用，并记录 UTC 时间。手动完成只影响人工跟进列表，不表示业务成功。

预览不写文件、不含稳定对象编号、完整本机路径、原始文件内容、认证信息或可执行命令，并明确标为不可导入或执行。复制通过现有剪贴板重试与非模态反馈链；换清单、刷新或关闭浏览器时会关闭旧预览。

## 验证

- SDK `9.0.302`，Release 全解决方案 Rebuild：`0 warning / 0 error`。
- Playnite 定向回归 `13/13`：新增预览/脱敏/命令与生产只读 UI、预览层开关和复制反馈行为；同时覆盖清单归一化、重复引用和状态投影。
- `scripts/validate-source.py .` 通过；XAML 25 文件结构检查通过；Round4 进度账本 `5/5`；`git diff --check` 通过。
- 本地构建与测试命令从 `C:\Windows\Temp` 启动，使用可用 SDK 9，未改仓库锁定 SDK。完整构建日志和 TRX 为本提交的原始证据。

## 原始证据与 SHA-256

| 文件 | SHA-256 |
| --- | --- |
| [Release 全解决方案 Rebuild](build/S06-07-release-rebuild-96a3eab3.txt) | `063EC072EA65EF9D47DDBD9BAD7B1F1C7384EAF403459C786F671DC4E0893990` |
| [清单回归 TRX](tests/S06-07-worklist-regression-96a3eab3.trx) | `0B766CCF0487EEAB673FC72A11A887053A0C4E7AA5E924FFD0432572E3D8DC84` |

定向测试筛选 `WorklistExportPreviewBehaviorTests`、`WorklistDefinitionTests` 与 `WorklistBrowserEntryTests`。首次 UI 自动化名称检查发现断言与实际名称不一致，已对齐；另外补测并修复预览层样式触发器被本地可见性值压住的问题。最终 TRX 为修正后的成功结果。

## 边界

未启动真实 Playnite，也未访问系统剪贴板；复制成功与占用失败通过生产复制/重试/反馈代码的注入 setter 验证。预览 WPF 控件和开合样式触发器在 STA 离屏窗口中检查，不代表真实宿主窗口、物理屏幕或 DPI 验收。此前未跟踪的 Round3 TRX 与 `tests/GameSaveCenter.Playnite.Tests/TestResults/` 未纳入提交。
