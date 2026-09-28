# Q13-04 表格列宽调整热区（2026-09-29）

## 现状与修复

检查了生产共享 DataGrid/Header 模板、列宽布局控制器及当前排序和列宽持久化测试。生产表格已启用列宽调整/排序，复用两侧原生 8 DIP `Thumb` 热区，最小宽度为 64 DIP；本阶段没有另造控件或改表格视觉模板。

行为回归发现一个已有缺陷：用户双击生产表头 resize gripper 后，WPF 将列宽恢复为 `Auto`，但布局设置中旧的 Pixel 覆盖仍保留；下一次布局恢复会把刚恢复的 Auto 宽度再次改回 Pixel。测试在修复前以断言“旧 Pixel 宽度键必须被移除”稳定失败，见 [`before-fix-stale-pixel.trx`](before-fix-stale-pixel.trx)。

提交 `c513be3f` 修复该行为：列宽变为 Auto 时，仅移除当前视图/当前列的已保存宽度，并复用既有延迟保存；Pixel 宽度仍按原机制保存。没有改变其他视图、其他列、命令绑定、排序语义或持久化 DTO。

## 验证

- 精确提交 `c513be3f` 的 Release solution 构建成功：XAML `24/24`，`0` warning、`0` error。隔离构建输出身份：插件 DLL SHA-256 `1B8F248A9C844A9114B6C3B850E4B423B9E11D8730EFE9077D8D3843125DA252`；Playnite 测试 DLL SHA-256 `137E5D6FC5827525F177F641AAF0B88E52126439C0ABB6023DCCD0625B58C1CE`。
- 生产 Header `Thumb` 的双主题 STA WPF 行为 `Q13ColumnResizeBehaviorTests 1/1`：两侧 grip hit-test、拖动增减宽度、64 DIP 下限、拖动不触发表格排序、双击恢复 Auto、后续布局不重新应用旧 Pixel 宽度，以及关闭 resize 时隐藏 gripper。结果：[column-resize.trx](column-resize.trx)。
- 既有 `R06ColumnWidthPersistenceBehaviorTests 6/6` 通过，含视图隔离、窄窗口最小列可达、重置边界和生产列键；结果：[column-width-persistence.trx](column-width-persistence.trx)。
- 既有 `R06SortingBehaviorTests 7/7` 通过，含真实 DataGridColumnHeader 排序双向行为、分离 CollectionView 负例、数值/未知项排序和生产箭头契约；结果：[sorting-regression.trx](sorting-regression.trx)。
- `scripts/validate-source.py` 与 `git diff --check` 通过。构建和回归均使用隔离 `.tmp/q13-04-c513be3f` 输出；设置由合成夹具提供。

构建命令：`scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot .tmp/q13-04-c513be3f/build`。三组测试分别通过 `dotnet test tests/GameSaveCenter.Playnite.Tests/GameSaveCenter.Playnite.Tests.csproj -c Release --no-build --no-restore` 按上述类过滤器运行；TRX 先写到隔离构建目录，再归档到本目录。WPF 类分进程串行执行。

## 未验证边界

测试通过真实 WPF Header/Thumb 和 STA 窗口路由拖动事件，不是 OS 物理鼠标输入；本阶段未启动 Playnite，也未验证真实宿主中的指针命中、最终呈现像素或有效物理 DPI。因此 Q13-04 行为代码和自动证据已具备，真实宿主栏及最终签收仍未完成。用户报告的 R08 one-click `1/2` 失败也仍缺完整失败方法/断言/堆栈/TRX；当前 main 的隔离复跑 `2/2` 通过不能代替定位用户侧失败。

下一项可执行任务为 Q13-05 ScrollBar Thumb hover 状态复核；若真实 Playnite 启动/诊断环境恢复，优先收集用户报告的 Media Inbox 滚动问题所需同进程 `[GSC-GRID-DIAGNOSTIC]` 几何日志。不得通过 ETW/系统跟踪权限绕过、OS 全局输入或离屏截图推断实际宿主呈现。
