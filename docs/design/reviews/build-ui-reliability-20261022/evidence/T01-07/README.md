# T01-07 包与截图去重状态

源码身份 `f577fd19`；包归属、只读重复预览和相关测试脚本自该提交以来未变。默认根入口在该身份上完成 Release 构建、测试、打包、安装及 Playnite 启动；[提交级全链与包 SHA](../T00-01/standard-f577fd19/README.md)。

## 包归属与去重

- PowerShell Core `7.6.2` 与 Windows PowerShell `5.1.22000.2538` 的 `Test-DuplicateArtifactPreview.ps1` 均退出 `0`；真实 hash 分组、重复清单路径去重、过期 hash 排除、引用截图保护、未知截图排除和文件未变断言通过。结果见 [`duplicates-Core.json`](duplicates-Core.json) 与 [`duplicates-Desktop.json`](duplicates-Desktop.json)。
- 两个宿主运行 `Test-PackageRetention.ps1` 均退出 `0`；旧回退包和未知包 SHA 未变，ZIP/PEXT 内容一致。结果见 [`retention-Core.json`](retention-Core.json) 与 [`retention-Desktop.json`](retention-Desktop.json)。
- 当前只读命令检查了14份包归属清单、39个产物记录；只发现一个同 hash 组，即当前交付 ZIP 与 PEXT，SHA-256 均为 `19AF45FC199A9245320BE3216584465B0AB2A71E02D07EDA3A3C68FACC597340`。这是两种必需交付格式，均保留。其余24条为排除项；所有预览均未授权删除。汇总见 [`package-preview-f577fd19.json`](package-preview-f577fd19.json)。

## 旧构建输出

移除两个已被 `f577fd19` 成功全链替代、由已退出构建创建、没有文档路径引用且没有链接/文件占用的输出目录：`gsc-b/1be2abbc` 与 `gsc-b/28e3be46`。对应 `20261024T105946705Z-156e96f5` 和 `20261024T120819535Z-9df7edc5` 的完整诊断目录仍保留，删除前后 summary SHA 未变。共移除 1,756 个文件、逻辑大小 455,648,120 字节；D盘可用空间从12,289,482,752增至12,749,279,232字节。最新构建输出 `gsc-b/229ce3b1`、ZIP/PEXT、安装报告及诊断均保留；详见 [`old-build-cleanup-f577fd19.json`](old-build-cleanup-f577fd19.json)。

## 真实截图库存与边界

只读索引统计现有审计目录中的2,432张图片、314,143,014字节和237组重复 hash。索引标明这些旧审计输出未被本阶段认领；精确文档路径引用为0，但有766个保守目录/文件名匹配。所有重复组都标为保留，本次未删除截图。记录见 [`actual-screenshot-audit.json`](actual-screenshot-audit.json)。

T01-07仍在实施中：包预览、旧构建安全清理及截图重复库存已经复核；历史截图的所有权尚未建立，不能据 hash 相同或没有精确引用就删除。后续需先把具体截图目录与创建阶段/归档证据对应，再决定是否保留一个副本；当前不触碰这批未知审计图。
