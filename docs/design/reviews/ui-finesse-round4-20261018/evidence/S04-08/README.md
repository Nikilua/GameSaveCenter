# S04-08 大图资源验收

本项先复核现有 `AsyncThumbnailLoader` / `AsyncThumbnailImage` 能力，并只补此前缺少的 GIF/不支持扩展与卸载期间迟到发布行为用例。比较视图继续复用同一异步解码链。

## 当前资源边界

- 解码长边最多 `2048 px`，并发解码最多 `3` 个；LRU 缓存最多 `96` 项、总计 `64 MiB`。超出单张缓存预算的图像不会进入缓存。
- 文件探测、源尺寸读取和解码在后台执行；图像冻结后交给 WPF。比较预览保留原始尺寸说明，但不解码超出尺寸上限的完整源图。
- 缺失、损坏和未列入支持扩展的文件返回不可用状态；生产候选浏览会略过缺失/解码失败图像。
- 控件变为不可见或卸载时取消当前请求、递增代次并清除 `Source` 与尺寸文本。卸载前已完成解码的结果在 UI 发布回调再次核对取消令牌和代次。

## 验证

- SDK `8.0.423` Release 全解决方案构建 `0 warning / 0 error`；诊断 ID `20261020T074347195Z-e0d59102`。构建控制台：[build-console.txt](build-console.txt)。
- 缩略图与媒体比较定向测试 `19/19` 通过：[原始 TRX](tests/s04-08-thumbnail-regression.trx)，SHA-256 `F65D4EF055F87B8DA9C59C9AF5B8A0BAEF9AA3902D63554C4625FE3773DE94D8`。
  - `R18ThumbnailBudgetTests` `3/3`：十个连续滚动窗口、120 次请求、并发/缓存/内存上限、取消、超限源图与源尺寸说明。
  - `AsyncThumbnailLoaderTests` `7/7`：GIF 解码、`.tiff` 不支持扩展返回空、损坏图返回空且可删除、缺失文件、缓存命中、并发窗口、预取消请求。
  - `AsyncThumbnailImageTests` `3/3`：新路径替换旧图、可见后才启动加载，以及在 UI 发布前关闭宿主时丢弃已完成解码结果并清空控件图像引用。
  - 媒体候选导航与生产比较页 `6/6`：有限筛选结果、坏图跳过、比较对象保持、退出擦除视图，以及人工意见行为。
- 构建 DLL SHA-256：`098607BC5C7E16D0D269382A5B71BED71E2012F7086AEE6BDBC97DAAE726D3B9`；测试 DLL SHA-256：`CBD0229E663A9F935B99D98E02BC658041A6F06CDA7743A39664FBF794E9704D`。
- `scripts/validate-source.py`、25 个 XAML 结构检查通过；最终滚动区几何用例另有 [6/6 TRX](../../../UI-SCROLL-VIEWPORT-20261020/ui-scroll-viewport-regression-final.trx)。

定向 WPF 用例在退出时仍会打印已存在的 `TextServicesContext.InvalidComObjectException` 清理输出；VSTest 退出码为 `0`，19 项均通过。GIF 使用测试生成的合成图片，卸载行为用受控 WPF 窗口与同步门验证。没有读取用户媒体，没有在真实 Playnite/物理 DPI 上验收，也没有宣称帧率结果。
