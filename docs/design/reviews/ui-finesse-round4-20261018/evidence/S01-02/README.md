# S01-02 舒适紧凑表格密度

日期：2026-10-18。实现提交：`ad7bd563`（`实现舒适紧凑表格密度`）。

## 实现

外观设置新增“数据表密度”，保存为 `DefaultTableDensityMode`，默认舒适模式；未知值归一化为舒适，编辑取消、JSON 往返与外观重置都保留正确状态。舒适/紧凑模式分别使用行高 `52/44 DIP`、表头 `42/38 DIP`、单元格横向留白 `12/10 DIP`，不改正文字号。

共享 DataGrid 样式与 Dashboard 本地兼容样式都读取相同令牌。页面将令牌覆盖写入自己的资源范围，不修改 Playnite/Application 级资源。保存设置后重新应用已打开工作区布局；四行最小高度按当前表头和行高计算，并为可能的水平滚动条与像素取整保留空间：舒适 `274 DIP`、紧凑 `238 DIP`。Media Inbox 继续使用实际测得的表格和滚动条尺寸。

## 验证

- SDK `8.0.423` Release 全解方案构建：0 warning、0 error。构建前设置 `GSC_BUILD_COMMIT=de3d881d` 与当前源码根目录；提交 `ad7bd563` 包含构建所用的相同源码树。
- 新增密度策略、设置持久化/重置与资源绑定定向测试 `14/14` 通过：[S01-02-targeted-verified.trx](tests/S01-02-targeted-verified.trx)，SHA-256 `C920F04752E234F8BB0C9F58C3FA7AC5F2C1F512C974AB404F9B54DE1162700E`。
- R09 当前源码复核 `2/2` 通过：[R09FocusOutlineBehaviorTests-final.trx](tests/R09FocusOutlineBehaviorTests-final.trx)，SHA-256 `FD724C1BBDF636121D9C1576022D9286B56FB68930458666E09F35AC1B262950`。
- Media Inbox 几何 `3/3` 通过：[MediaInboxGeometryTests.trx](tests/MediaInboxGeometryTests.trx)，SHA-256 `4CE9AD14630A2DE4022A0AAD5A5220120B35927CDA8AC88DEAA0087A3F9DAD2C`。
- 生产资源状态与双主题行为 `9/9` 通过：[R23ProductionResourceStateBehaviorTests.trx](tests/R23ProductionResourceStateBehaviorTests.trx)，SHA-256 `77D56CBABFEF100344F23105761ADA5561A88281152F42CA6BD60ABB6DCA00E4`。
- 测试程序集 SHA-256：`77E9CB1CDC4B941EFB1326D19DDAE85943021EFA9641E068967B61E58DC2CA8E`。
- `python scripts/validate-source.py .` 与 `git diff --check`：通过。

## 边界

受控编译和定向行为验证通过；真实 Playnite 设置加载/保存、实际宿主呈现及屏幕字号/点击体验未运行。当前全量 WPF/安装门禁仍受独立 R05 Tooltip 测试的系统指针读取权限错误阻挡；该测试没有改成跳过，安装和版本核验未到达。此前外机 R09 的 `Expected 2 DIP / Actual 1 DIP` 来自旧断言形态；这次用当前源码构建的 R09 为 `2/2`。详见 [R09 当前源码复核](../../../ui-finesse-round3-20260915/evidence/R09-FOCUS-CURRENT-MAIN-RECHECK-20261015/README.md)。
