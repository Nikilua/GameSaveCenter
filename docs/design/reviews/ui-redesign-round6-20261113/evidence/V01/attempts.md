# V01–V04 采样诊断

这些是开发期的采样/夹具缺陷，均已修正；失败输出没有被计为通过，也没有放宽断言。

| 尝试 | 观察到的失败 | 修正 |
|---|---|---|
| 首次 Harness 编译 | `CS0103: GetCommitSha` 不存在于 RenderHarness.Program。 | 源提交身份由隔离运行脚本的 `runner-metadata.json` 记录，Harness 报告引用该文件。 |
| 首次基线运行 | `SelectTab` 在宿主完成 Measure/Arrange 前遍历视觉树，找不到 SaveCenterView 的 TabControl。 | 先将生产视图放入固定客户区并执行布局，再选择标签。 |
| 首次详情状态断言 | 1600×900 选择一条记录时生产布局会自动显示侧栏详情；紧凑详情按钮不控制宽屏侧栏，导致把默认宽屏误称为“详情关闭”。 | 宽屏闭合样本使用真实无选择状态，详情样本使用同一数据中的选中行；不强行设置生产页不能呈现的状态。 |
| 状态预期复核 | 宽屏闭合样本无选中项，原先通用检查却仍要求有选择。 | 断言按窄/宽屏与详情状态分别验证选择，状态夹具通过。 |
| Source Validation | 新增 PowerShell 5.1 脚本缺少 UTF-8 BOM，源码校验拒绝。 | 用 UTF-8 BOM 重写该文件；后续 Source Validation exit 0。 |

保存的 `round6-baseline-report.txt`、`runner-metadata.json` 和状态夹具报告均是修正后的最终成功样本。上述失败只用于说明实现与质量门禁如何调整，没有修改生产 XAML。
