# S04-06 保留意见标记

实现提交：`792b1a2f`（`增加截图比较意见标记`）。

比较页为当前会话中的最多两张媒体对象提供“待保留 / 待检查”意见，可按稳定媒体 ID 替换和撤销；清空比较对象时同步清除意见。标记不写入正式媒体收藏/备注，不执行 IgnoreMedia、归类或文件删除，也不跨重启持久化。

## 验证

- SDK `8.0.423` Release 全解决方案构建成功，`0 warning / 0 error`；完整控制台记录为 [build-console.txt](build-console.txt)。
- 比较对象、意见标记、撤销与生产 WPF 页面行为测试 `11/11` 通过：[TRX](tests/s04-06-media-comparison.trx)。
- `scripts/validate-source.py` 通过：[source-validation.txt](source-validation.txt)。WPF 静态扫描记录在 [wpf-ui-static-review.txt](wpf-ui-static-review.txt)。
- 合成媒体比较页面截图：[media-comparison-opinion.png](media-comparison-opinion.png)。

早期测试夹具曾以 `4/5` 结束，暴露生产按钮测试没有准备好命令执行上下文；修正夹具后，以当前提交构建的 `11/11` 结果为验收依据。早期结果保留于 [pre-fix-selection-fixture.trx](tests/pre-fix-selection-fixture.trx)。

截图及 WPF 行为使用合成媒体/离屏宿主，不代表真实 Playnite 窗口或用户媒体。真实 Playnite 呈现与物理输入未验。
