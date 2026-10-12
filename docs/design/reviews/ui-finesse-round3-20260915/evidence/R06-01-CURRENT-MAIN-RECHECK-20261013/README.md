# R06-01 列宽用户记忆：当前 main 复核

日期：2026-10-13（Asia/Shanghai）。复核提交：`afade3cd39eab31de97f204aa76e9dc590ba3083`。本轮只核对当前 main 已有实现并补证；没有新增或重建列宽服务、DTO、命令或设置通道。

## 当前实现核对

继续复用 `GameSaveCenterSettings.DataGridColumnWidths`、`DataGridColumnLayoutController` 和现有 Playnite 设置保存链。Save History、Save Candidates、Task Queue、Media Inbox 通过稳定的视图/列键分别保存用户 Pixel 宽度；版本/列键校验、初始化/响应式布局抑制、最小宽度、当前视图重置、卸载 flush 与既有横向滚动仍由原实现负责。

当前提交的 `R06ColumnWidthPersistenceBehaviorTests` 有 6 条行为用例，均在 STA WPF/合成设置环境通过：

1. 用户 Pixel 宽度在控制器重建后恢复，不串到其他视图。
2. 旧版本键和未知列键被忽略，存储宽度受当前最小宽度约束。
3. 重置回捕获的响应式默认布局，只删除当前视图键，并调用持久化回调。
4. 窄窗口保持主列可达，既有横向滚动条仍可见。
5. 生产表格有稳定视图区键和可达的重置动作。
6. Task Queue 的持久化列键与生产 DataGrid 列定义逐项一致。

## 当前 main 构建与身份

- 隔离本机 SDK `8.0.423`，输出目录 `D:\GameSaveCenterTools\R06-01-current-main-20261013`。运行官方 `scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot ...`：XAML `24/24`，Release solution `0 warning / 0 error`；Playnite `net462`，测试 `net472`。
- 对该输出程序集执行 `dotnet vstest`，筛选 `FullyQualifiedName~R06ColumnWidthPersistenceBehaviorTests`：`6 passed / 0 failed / 0 skipped`。本次定向脱敏 [TRX](tests/R06-01-current-main.trx) SHA-256：`CF964D93CB84511D6F261505B7B7DF87354002EE810D1B1914E9FBC30481AD0A`。
- 当前行为测试源码 SHA-256：`CB41E6A518AE63F4D35D8BC345CCCF2D2A2C02F9860DE6B7E547D1C90E64BC18`；控制器 `AD070507AD7BB8F512AED3E42130A757D703778AEA21732882D1FF2123D9D958`；设置模型 `4C1D01668403FEBF21D2C55CB0F955A251FA34B44943873E3AC67077D721AEB0`。
- 当前 Release 插件 DLL SHA-256：`BBEE3EB3845F3F59FCDDEE99233F06CE04DAC0C68B11C086AB41A17D84FC3E80`；测试 DLL SHA-256：`BA45E24B351188A61F06156985F1A3FB2A5AE113D3EAE912A8431CD25C8466BE`。
- 本次没有改生产代码或测试。R06-01 原有[实现与视觉证据](../R06-01-COLUMN-WIDTH-PERSISTENCE-20260918.md)继续保留；其中双主题 RenderHarness 报告/截图绑定原实现提交 `75a6e6d8`。本轮未重跑 RenderHarness，因此不把旧图或离屏逻辑 DIP 当作当前 `afade3cd` 的新视觉证据。

## 边界与结论

行为仍由合成 DataGrid、隔离设置对象和控制器销毁/重建验证；这不等价真实 Playnite 进程重启、用户拖拽录像或宿主设置迁移。未验真实 Playnite、物理 DPI/跨屏、UIA/Narrator、presented frame、ETW 或宿主性能；没有写入真实配置、存档、媒体或云端。实现仍满足 R06-01 的当前可控行为条件，账本状态保持“已满足”。

下一项：R06-02 排序提示与稳定性；先确认当前 main 在已有排序复核提交之后没有改变相关行为，再按需补当前身份证据。
