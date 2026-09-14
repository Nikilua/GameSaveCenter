# Q02-02 存档历史数值列对齐证据

采集日期：2026-09-15（Asia/Shanghai）。来源为当前提交 `94dc203c8bb9b05537098d5ca43ade3807b24443` 的 `RenderHarness audit`，报告身份为 `Commit=94dc203`、`WorkingTreeClean=True`、`DpiScale=1.00`；standard 逻辑内容区为 1116×660 DIP。

## 生产页截图

[SaveCenter 历史版本 standard 截图](Q02-SAVE-HISTORY-ALIGNMENT-20260915.png)

截图中的 `文件数`、`大小` 两列使用右锚点数字文本；`时间`、`类型`、`设备`、`备注` 和 `状态` 保持各自文本/胶囊语义，没有把所有列机械右对齐。

## 实际视觉树测量

standard `save-center-历史版本` 视觉树中，8 行文件数与大小单元格共 16 个可见数值 `TextBlock`：

- 文件数：`121`–`128`，8/8 的 `HorizontalAlignment=Right`，实际文本宽度均为 `23.33 DIP`。
- 大小：`24.6 MiB`、`25.56 MiB`、`26.51 MiB`、`27.47 MiB`、`28.42 MiB`、`29.37 MiB`、`30.33 MiB`、`31.28 MiB`，8/8 的 `HorizontalAlignment=Right`，实际文本宽度为 `55.33–63.33 DIP`。
- `SaveHistoryGrid` 实际宽度为 `739.33 DIP`，数值列实际宽度分别为文件数 `64 DIP`、大小 `78 DIP`；audit 没有 `SHORT_SEMANTIC_VALUE_TRIMMING`、`ESSENTIAL_COLUMN_VISIBILITY` 或 Fidelity 警告。

该证据签收离屏生产视图中的列锚点、最坏样本未裁切和双列语义分离；真实 Playnite 字体/DPI、鼠标调整列宽与排序点击仍属于宿主边界。
