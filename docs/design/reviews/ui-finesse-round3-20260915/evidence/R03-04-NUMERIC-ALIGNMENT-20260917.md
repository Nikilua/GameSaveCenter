# R03-04 数字列对齐验证

日期：2026-09-17  
代码提交：`9fa68ef276e6c55ad65ad1860b6b0e6b16dcf1a1`  
任务：计数、容量、百分比和时间按列语义对齐；9→10、99→100 保持列锚点；未知值统一占位且不显示为零。

## 现状核对与实现

- 现有 `GscTypographyNumeric` 已使用 WPF `Typography.NumeralAlignment=Tabular`，Save 计数/容量已有右对齐，但 Save/Media/Maintenance/Task 的时间单元格和 Task 百分比仍分别复用普通文本或直接格式化 `ProgressPercent`。
- 本阶段新增共享 `GscTypographyNumericCell`、`GscTypographyTimeCell`、`GscTypographyPercentCell`。数字单元格统一 `HorizontalAlignment=Right`、`TextAlignment=Right`、`NoWrap`、`TextTrimming=None`；时间沿用右锚点并允许字符省略；百分比沿用右锚点和 Caption 层级。
- Save 历史的时间/计数/容量、Media 拍摄时间、Maintenance 审计时间和 Task 创建时间接入共享语义样式。Task 表格与详情卡使用新增显示层 `ProgressValue`/`ProgressDisplay`；原始 `ProgressPercent`、状态、命令、绑定和安全流程未删除或改写。
- `ProgressDisplay` 对排队状态的默认 0 和负数显示 `—`；有效值显示百分比，超出 0～100 的值只在显示/进度条层钳制到边界，不把未知写成 0。DTO 的值类型和既有非空计数/容量语义保持不变。

## 自动验证

- `R03NumericAlignmentTests`：`10/10`。
  - 真实 STA WPF 生产资源中用 120 DIP 列测量 `9`、`10`、`99`、`100`；四个 TextBlock 的右边界均落在 `119.5～120.5 DIP`，且 10 比 9、100 比 99 的内容测量宽度更大，证明位数变化不会移动列右锚点。
  - 8 个合成 DTO 边界用例覆盖排队 0、负数、运行中 0/9/10/99/100/120；占位、正常显示与 100% 钳制均通过。
  - 生产源码契约确认四个页面时间列和 Task 百分比使用共享样式/显示属性，并拒绝旧的 `ProgressPercent` 直接百分比格式化。
- 干净隔离 Release：XAML `24/24`；构建 `0 warning / 0 error`；Core `83/83`；Worker `311/311`；Playnite `558 passed / 57 skipped / 0 failed`，总计 `615`；源码校验通过。
- RenderHarness Release 构建 `0/0`，报告绑定上述 commit 且 `WorkingTreeClean=True`；生产 Light/Dark `1040×700` 的 Task、Save、Media、Maintenance 等页面均 `render-qa OK`。人工抽查 Light Task、Dark Task、Light Save、Dark Media 截图，数字/时间列无新增裁切或主题对比异常。
- `python scripts/validate-source.py`：通过。RenderHarness 报告仅作为受控 WPF/offscreen logical DIP 证据。

## 门禁校正记录

第一次全量运行暴露了两项测试声明问题：既有 Save 尺寸样式源码断言需要保留局部 `TextTrimming=None`，新锚点夹具需要显式 120 DIP Grid 列且应测量 TextBlock 右边界而非把内容宽误当作列宽。修正后重新执行干净隔离构建，最终结果为上述 `0/0` 和 `10/10`；没有通过修改 skip 或放宽生产门禁来掩盖失败。

## 未验边界

测试使用合成 DTO、真实生产 WPF ResourceDictionary、隔离 STA Window 和 offscreen logical DIP；未启动真实 Playnite，未验证宿主最终 frame、用户字体替换、物理 DPI/跨屏、OS 键盘/IME、屏幕阅读器、ETW 或宿主帧率/性能。未写真实存档、媒体、云端或用户设置。当前游戏选框、滚动条系统、命令/Binding、取消/错误、恢复保护、有限列表和 net462 契约保持不变。

复现入口：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build.ps1 -Configuration Release -OutputRoot .tmp/r03-04-build-final
dotnet vstest .tmp/r03-04-build-final/bin/GameSaveCenter.Playnite.Tests/Release/net472/GameSaveCenter.Playnite.Tests.dll --TestCaseFilter:"FullyQualifiedName~R03NumericAlignmentTests"
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/render-qa.ps1 -Configuration Release -Output .tmp/r03-04-render-final
```
