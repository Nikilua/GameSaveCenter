# Q14-01 Task Center 工具栏同高复核

日期：2026-09-29。复用当前生产 `TaskCenterView` 与共享主题资源补充实际几何断言；没有生产 UI 改动。

## 构建身份

- 源码 HEAD：`591f07be`（本次 test-only 提交）。Release solution build 成功，`0 warnings / 0 errors`；XAML `24/24`。`scripts/validate-source.py` 通过。
- 定向用例：`Q14ToolbarAlignmentBehaviorTests.ProductionTaskToolbarsKeepMixedControlGeometryAndTextCenters`，`1/1 passed`、0 failed/skipped，VSTest exit `0`。一条 Fact 在同一 STA 窗口线程依次跑 10 个 Light/Dark × 尺寸场景。
- 测试 DLL MVID：`86c679d4-47f0-48a8-b16f-d3114279569f`；SHA-256：`DE1F1E811538B327B02D1B5933F5F3C97C0B01F81DC04225D88BB56402847493`。插件 net462 DLL SHA-256：`43B68167A8D469E0B593D2D0ECE2A7CCB4F3614C4E556CADF1C3CDA7926C296D`。
- 构建完整输出：[Release build](release-build.log)；测试输出：[TRX](Q14-01-toolbar.trx)、[console](console.log)、[10 组几何数据](geometry-report.txt)。

## 实际测量

- 覆盖窗口 DIP 尺寸 `980×640`、`1040×700`、`1280×720`、`1600×900`、`760×640`，各自 Light/Dark。使用真实生产 `TaskCenterView`、`DesignTokens.xaml`、`WpfUiProduction.xaml`、`Redesign.xaml` 和 `AcrylicProductionResources.xaml`；DataContext 是只提供合成筛选选项的夹具，没有调用真实服务或命令。
- 主筛选行中参与布局的混合控件高度均为 `36 DIP`，控件框中心差不超过 `0.75 DIP`。预设行的组合框、输入框和按钮均为 `36 DIP`，顶部对齐；内容中心实测范围 `17.33–18 DIP`，最大差 `0.67 DIP`。各主题与宽度的坐标见几何数据文件。
- 另将搜索框置为合成校验错误、键盘聚焦、禁用刷新按钮、设置刷新忙碌态；快照逐项比较边界、尺寸与内容中心，各状态相对基线变化不超过 `0.75 DIP`。
- 当前 WPF testhost 报告 DPI scale `1.5×1.5`。`760 DIP` 场景中只有搜索、状态筛选和刷新按钮留在主行；测试不将另外的筛选框位置记作已覆盖。

## 验收边界

- `console.log` 在 xUnit `Finished` 后报告 21 条 WPF `TextServicesHost` `InvalidComObjectException` 清理异常；xUnit/TRX 明确通过且 VSTest exit `0`，该清理噪声根因未知。
- 这是近乎透明的 STA WPF 测量夹具，不是 Playnite 宿主截图/呈现帧。没有实际鼠标/键盘输入，没有切换到物理 125% DPI，也没有覆盖所有页面的工具栏。真实宿主仍需验证，所以 Q14-01 保留 Round2 的外部阻塞/未完成状态。
- 下一项：Q14-02 筛选标签。开始前继续盘点现有共享标签样式和绑定，再按实际缺口补行为；Media Inbox 真实 Playnite 滚动诊断仍是独立未验边界。
