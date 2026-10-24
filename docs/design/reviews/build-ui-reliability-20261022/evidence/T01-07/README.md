# T01-07 包与截图去重状态

源码身份 `f577fd19`；包归属、只读重复预览和相关测试脚本自该提交以来未变。默认根入口在该身份上完成 Release 构建、测试、打包、安装及 Playnite 启动；[提交级全链与包 SHA](../T00-01/standard-f577fd19/README.md)。

## 包归属与去重

- PowerShell Core `7.6.2` 与 Windows PowerShell `5.1.22000.2538` 的 `Test-DuplicateArtifactPreview.ps1` 均退出 `0`；真实 hash 分组、重复清单路径去重、过期 hash 排除、引用截图保护、未知截图排除和文件未变断言通过。结果见 [`duplicates-Core.json`](duplicates-Core.json) 与 [`duplicates-Desktop.json`](duplicates-Desktop.json)。
- 两个宿主运行 `Test-PackageRetention.ps1` 均退出 `0`；旧回退包和未知包 SHA 未变，ZIP/PEXT 内容一致。结果见 [`retention-Core.json`](retention-Core.json) 与 [`retention-Desktop.json`](retention-Desktop.json)。
- 当前只读命令检查了14份包归属清单、39个产物记录；只发现一个同 hash 组，即当前交付 ZIP 与 PEXT，SHA-256 均为 `19AF45FC199A9245320BE3216584465B0AB2A71E02D07EDA3A3C68FACC597340`。这是两种必需交付格式，均保留。其余24条为排除项；所有预览均未授权删除。汇总见 [`package-preview-f577fd19.json`](package-preview-f577fd19.json)。

## 旧构建输出

移除两个已被 `f577fd19` 成功全链替代、由已退出构建创建、没有文档路径引用且没有链接/文件占用的输出目录：`gsc-b/1be2abbc` 与 `gsc-b/28e3be46`。对应 `20261024T105946705Z-156e96f5` 和 `20261024T120819535Z-9df7edc5` 的完整诊断目录仍保留，删除前后 summary SHA 未变。共移除 1,756 个文件、逻辑大小 455,648,120 字节；D盘可用空间从12,289,482,752增至12,749,279,232字节。最新构建输出 `gsc-b/229ce3b1`、ZIP/PEXT、安装报告及诊断均保留；详见 [`old-build-cleanup-f577fd19.json`](old-build-cleanup-f577fd19.json)。

## 真实截图库存与边界

初始只读索引统计2,432张图片、314,143,014字节和237组重复hash；这是清理前快照。整个库存里有766个保守目录/文件名匹配，因此不能按hash直接清理。初始索引见 [`actual-screenshot-audit.json`](actual-screenshot-audit.json)。

### 2026-10-25 离屏截图副本清理

- 三个未被文档引用的旧 `ui-width-regression-20261023` 输出目录均有 `render-qa-report.txt`，报告将输出标记为 `OffscreenRenderHarness`，记录源码提交及生成时间。报告均注明 `WorkingTreeClean=False`，因此只用它们追溯输出目录，不把记录的提交说成干净源码运行。
- 在 `render/`、`render-final/`、`render-ratio-final/` 中删除428个PNG副本（200个不同SHA-256，逻辑大小58,403,926字节）。逐个核对文件与索引哈希，并在文档引用的 `save-summary-reflow-20261023/render-final/`、`ui-layout-fix/render-serial-final/` 或 `ui-width-regression-20261023/render-clean-commit/` 中验证保留同哈希文件。所有唯一图片、带文档引用或保守引用的图片、渲染报告及其他构建产物均保留。
- 选中副本在初始索引中没有精确或保守引用；清理前对当前 `docs/` 的190个候选文件名做固定字符串扫描，0命中，候选子目录路径扫描也为0。三个渲染报告不包含PNG路径。清理前没有dotnet、RenderHarness或testhost运行；每个删除目标解析后都在仓库 `artifacts/` 内，不是reparse point，也未被Git跟踪。
- D盘可用空间从11,262,414,848增至11,321,716,736字节，增加59,301,888字节。按初始库存扣除删除清单，现有索引范围为2,004张、255,739,088字节；237组重复hash仍存在（1016个副本）。删除路径、SHA、保留副本、运行报告身份和空间前后值见 [`screenshot-dedupe-20261025.json`](screenshot-dedupe-20261025.json)。该清单中的路径是历史删除记录，不是当前证据链接。

T01-07仍在实施中：包预览和两个旧构建目录清理已复核；截图清理只覆盖有明确运行报告、无引用且有字节级保留副本的428个文件。剩余重复组仍位于被引用或未证明可清理的输出中，继续保留；不按hash相同推断其他图片可删。
