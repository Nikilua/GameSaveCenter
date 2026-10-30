# U09-01 生产 UI 变更覆盖清查

日期：2026-10-30。此证据只关闭对比资料盘点，不代表 Playnite 宿主验收。

## 清查结论

U00-01 的基线与去重盘点已先行完成。以 U00 记录的产品源码提交 `90bf040567f013f3a8d9ed8667028a4edb2ec524` 为起点，以冻结候选 `5c0c57b06aed27dc46c41ef3d655ec20dafa5ec7` 为终点，对 `src/GameSaveCenter.Playnite` 中的 XAML、View、Theme、Design 与交互动效文件历史进行复核，共找到 37 个 UI 源码变更提交。当前 HEAD `f6836930fc67f32a691e9ac26966ca55d80c0062` 与该候选的 `src/`、`tests/` 树逐文件相同；之后只有文档变化。

U01–U08 的可视差异批次均能映射到已有的同数据配对图；动效/生命周期批次另有真实 Rendering 中间值、时间采样或有序行为序列。对没有像素变化的令牌收敛和行为修正保留其测试结论，不制造虚假差异图。逐提交清单与对应文件在 [UI 源码提交审计](ui-source-commit-audit.json)，配对/时序依据包括：

- U01、U02、U03： [U01](../U09-01-U01-pair-reconstruction-20261029/BASELINE.md)、[U02主题](../U09-01-U02-pair-reconstruction-20261029/BASELINE.md)、[U02几何](../U09-01-U02-geometry-pairs-20261029/BASELINE.md)、[U03输入](../U09-01-U03-input-pair-20261029/BASELINE.md)、[U03禁用原生文字修正](../U03-disabled-native-text-20261029/BASELINE.md)。
- U04、U05、U06： [U04选择与动效](../U09-01-U04-pairs-20261029/BASELINE.md)、[U04中断采样](../U09-01-U04-interrupt-20261029/BASELINE.md)、[U04提示条修正](../U04-indicator-gap-20261029/BASELINE.md)、[U05静态配对](../U09-01-U05-pair-reconstruction-20261029/BASELINE.md)、[U05时间样本](../U09-01-U05-pair-reconstruction-20261029/motion/BASELINE.md)、[U05帮助弹层](../U05-04-help/BASELINE.md)、[U05离页序列](../U05-07-08/BASELINE.md)、[U06候选表](../U09-01-U06-pair-reconstruction-20261029/BASELINE.md)。
- U07、U08： [U07-01～04](../U09-01-U07-01-04-pair-20261030/BASELINE.md)、[U07标题密度](../U07-05-06-title-density/render-pair-dcc288d6.json)、[通知来源](../U07-05-notification-source-20261029/render-pair.json)、[设置依赖/任务恢复](../U07-07-08/historical-render-pair-20261030/BASELINE.md)、[U08概览与存档](../U09-01-U08-pair-20261030/BASELINE.md)、[U08媒体](../U09-01-U08-03-04-pair-20261030/BASELINE.md)、[U08工具与维护](../U09-01-U08-05-06-pair-20261030/BASELINE.md)、[U08通知与Trainer](../U09-01-U08-07-08-pair-20261030/BASELINE.md)。

这次核对没有发现未映射的生产 UI 变更批次，U09-01 的比较证据最小集达到清单要求。U04/U05 动态证据只代表受控 WPF Rendering/合成事件；真实 Playnite、实体键盘与 IME、物理 DPI 和外部 UIA 仍按原任务边界待验。

## Demo-first 边界

仓库规定的 `GameSaveCenter.AcrylicFork/.../DesignShellView.xaml` 在当前工作树和旁边的旧 main 工作树都不存在。U00-01 基线截图因此是生产 RenderHarness 样本，不是 Demo 对照。本批没有新增生产 UI 设计或样式，不能据此声称 Demo 视觉已对齐；在 Demo 文件重新可用前，后续视觉改动仍需先取得该基准。
