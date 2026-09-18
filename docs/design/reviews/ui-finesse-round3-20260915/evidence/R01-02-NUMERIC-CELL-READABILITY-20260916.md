# R01-02 数字单元格裁切证据

日期：2026-09-16；当前提交复核：2026-09-18
分支：`codex/ui-finesse-round2`  
实现提交：`acfe1eab48572e538c9d17cbb45b7bdc116942ed`

## 对照基线

质量审查 F07 指出，Finesse 夹具的首行数字 `1,024 / 99,999` 在数值列边界处裁掉末位，但原报告只检查四行垂直布局和独立文字对比度，仍可能输出 `finesse-fixture OK`。旧夹具数值列为 `110 DIP`。这证明原有夹具/门禁缺少水平文字完整性检查，不足以推断所有生产表格均存在裁切。

## 实现

- 新增 `src/GameSaveCenter.Playnite/Infrastructure/NumericCellReadability.cs`。它遍历实际 realized `DataGridCell`，按表头找到数值列，测量非约束文本宽度，并以 `cell.ActualWidth - Padding.Left - Padding.Right` 计算可用内容宽度；同时记录文本高度、单元格高度、换行和 trimming。
- `HorizontalFit`、`VerticalFit` 和 `IsReadable` 分开判断，并保留 `0.5 DIP` 浮点容差。这样“行高足够但列宽不足”的长负数会明确失败，而不是被 TextBlock 自适应的 `ActualWidth` 掩盖。
- `UiFrameworkProbeView` 的数值列调整为 `Width/MinWidth=160 DIP`，样本固定为 `1,024 / 99,999`、`-9,999,999,999`、`512 GiB`、`1.25 TiB`，数值文本显式 `NoWrap`/无 trimming。
- `RenderHarness finesseprobe` 和 Playnite 测试均复用同一个 helper；没有修改生产业务命令、绑定、游戏选框、滚动条、取消/错误、恢复保护、有限列表或 `net462` 业务契约。

## 当前提交补充

- `a5219c09` 只修复 R01-01 的测试源码根绑定；本批没有新增或重建生产数值列实现，R01-02 继续复用 `acfe1ea` 的 `NumericCellReadability` 与现有校对夹具。
- 使用 `.tmp\r01-01-02-build-clean-a5219c09` 的当前隔离构建，RenderHarness 独立 restore/build 为 `0 warning / 0 error`；运行报告的 `Commit` 为完整 `a5219c09...`，`WorkingTreeClean=True`。

## 行为验证

1. 实际 STA WPF 定向测试 `NumericCellReadabilityTests`：`2/2`。四个生产校对样本的横向与纵向 fit 均为 true；`56 DIP` 数值列中的 `-99,999,999,999,999` 负例为 `VerticalFit=True`、`HorizontalFit=False`、`IsReadable=False`。
2. clean-tree `finesseprobe` Light/Dark 各通过：`expected=4`、`realized=4`、`horizontalFit=4`、`verticalFit=4`、`allReadable=True`。四个样本的关键测量如下，单位为逻辑 DIP：

   | 文本 | 文本宽度 | 可用宽度 | 文本高度 | 单元格高度 | 横向/纵向 |
   | --- | ---: | ---: | ---: | ---: | --- |
   | `1,024 / 99,999` | 88.58 | 136.00 | 18.67 | 52.00 | true / true |
   | `-9,999,999,999` | 93.89 | 136.00 | 18.67 | 52.00 | true / true |
   | `512 GiB` | 46.93 | 136.00 | 18.67 | 52.00 | true / true |
   | `1.25 TiB` | 48.27 | 136.00 | 18.67 | 52.00 | true / true |

3. 同一探针的窄列负例测得文本宽度 `115.26`、可用宽度 `56.00`、文本高度 `15.33`、单元格高度 `52.00`，因此 `HorizontalFit=False`、`VerticalFit=True`，并报告 `must-fail=passed`。Light/Dark 报告的 contrast violation 均为 `0`，离屏图像已人工检查为末位完整。
4. Release 全流程 `scripts/build.ps1 -Configuration Release -OutputRoot .tmp\r01-02-isolated`：XAML 结构校验 `24/24`，解决方案构建 `0 warning / 0 error`，Core `83/83`，Worker `311/311`，Playnite `520` 通过、`57` 跳过、`0` 失败。

5. 当前提交干净隔离复核：`NumericCellReadabilityTests` `2/2`；Light 报告为 `.tmp\r01-02-finesse-clean-a5219c09-light\finesse-fixture-report.txt`，Dark 报告为 `.tmp\r01-02-finesse-clean-a5219c09-dark\finesse-fixture-report.txt`。两主题均为 `expected=4 realized=4 horizontalFit=4 verticalFit=4 allReadable=True`、contrast violations `0`；窄列负例均为 textWidth=`115.26`、availableWidth=`56`、`HorizontalFit=False`、`VerticalFit=True`、`must-fail=passed`。

## 边界与证据口径

- 探针使用合成 `ProbeRow`、实际 WPF `DataGrid`/`DataGridCell` 和隔离 Window，DPI 记录为 `1.00` 的 offscreen logical DIP；它不等价真实 Playnite 嵌入 Dashboard、物理 DPI、用户输入/IME、presented frame、ETW 或宿主大库帧性能。
- 本阶段是夹具和可读性检测门禁校正，不是对所有生产表格的全量视觉签收。生产业务状态、命令/绑定、安全与恢复语义未改。
- 旧基线与最终 Light/Dark PNG 仅用于本阶段受控比较，已在文档完成后清理临时输出；持久证据以本文件、源代码和测试为准。

## 下一步

R01-02 代码、负例和双主题受控证据已完成，当前提交复核未改变生产契约；下一可执行小批量为 R01-03“每项证据直达”：先核对账本行与证据链接是否逐项可达，再补断链/错链负例和报告入口校验。
