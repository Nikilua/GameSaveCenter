# R07-04 横向滚动端点验收证据

日期：2026-09-18  
分支：`codex/ui-finesse-round2`  
代码提交：`8f682fcb814a648de497728b17ebc870a13187aa`

## 结论

R07-04 在当前可控范围内已满足。核对最新生产页面和共享 DataGrid 模板后，本阶段没有改动生产 XAML、滚动条系统、游戏选框、命令绑定或业务服务；只在隔离 RenderHarness 中增加横向端点行为探针。探针使用真实生产页面、实际模板内 `DG_ScrollViewer` 和合成 fake 数据，证明横向滚动的负端点、超最大端点、末列/末单元格可见、横向条不遮挡首行以及回到左端无漂移。

## 探针范围与行为

- `horizontalprobe` 在 Light/Dark 两主题和 Save/Task/Media/Maintenance 四类生产页面上运行，共 8 个组合；每页使用合成长表并把现有列扩展到可横向滚动的隔离夹具尺寸。
- 每个组合先验证 `ScrollToHorizontalOffset(-120)` 被夹到 0，再验证超过 `ScrollableWidth` 的请求被夹到最大值；到右端时检查最后一个实际列头和最后一个实际单元格完整落在表格 viewport 内。
- 检查实际横向滚动条存在且不覆盖第一行；随后回到左端，确认首列头左边界恢复到初始位置，没有像素/DIP 漂移。检查读取的是实际 `ActualWidth/ActualHeight`、列头/单元格矩形和模板内 ScrollViewer 状态，不是字符串断言。
- Media 同时保留现有有限列表契约：`VirtualizationMode=Standard`、`ScrollUnit=Item`、`EnableColumnVirtualization=False`，没有为了端点探针关闭行虚拟化。

## 原始输出与抽查

报告：`.tmp/r07-04-horizontal-clean/horizontalprobe-report.txt`  
报告身份：`WorkingTreeClean: True`、`DpiScale: 1.00`（隔离离屏逻辑 DIP，不推断真实宿主 DPI）；末尾为 `horizontalprobe OK`。Light/Dark×Save/Task/Media/Maintenance 的端点、viewport、末列/单元格和滚动条几何均记录在报告中，横向最大值分别为 Save `987.33`、Task `1152`、Media `707.33`、Maintenance `619.33` DIP。

已抽查实际右端截图：`.tmp/r07-04-horizontal-clean/light-Task-right.png`、`.tmp/r07-04-horizontal-clean/dark-Media-right.png`。前者可见 Task 右侧“详情”列和底部横向条，后者可见 Media 右侧“文件/原因”列；截图在设置右端 offset 后、回到左端前保存，只作离屏布局抽查，不冒充真实 Playnite 呈现或物理跨屏证据。

## 自动回归

- `scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot .tmp/r07-04-build-clean`：XAML 结构 `24/24`，解决方案 Release 编译 `0 warning / 0 error`。
- RenderHarness Release 编译：`0 warning / 0 error`；运行 `horizontalprobe`：Light/Dark×四页 `8/8`，均 `horizontalprobe OK`。
- 每个组合均覆盖负/超最大端点夹断、末列/末单元格完整、横向条不遮挡第一行和左右端点往返无漂移；Media 虚拟化契约保持。

## 适用边界与下一步

证据使用合成 DTO、fake 服务、隔离 STA WPF 和 offscreen logical DIP；Demo 原始 `DesignShellView.xaml`/`Pages` 目录在当前 checkout 仍不存在，视觉基准继续沿用已恢复的生产布局。未验真实 Playnite 嵌入宿主、鼠标/触控板物理输入、Ctrl/Shift/IME 路径、UIA/读屏、物理 DPI/跨屏、presented frame、ETW 或宿主性能；未写真实存档、媒体、云端或诊断数据。

账本下一布局项为 R07-05「详情断点稳定」；按本轮用户指定顺序，下一实际执行批次先转入 R00/R01 的现有实现复核、问题修复与证据校正。
