# S04-04 差异查看辅助

## 交付

媒体比较工作区现在可切换“擦除对照”。分隔线左侧显示 B、右侧显示 A；拖动分隔线可改变比例，聚焦分隔线后可用左右方向键以 2% 调整、Shift 加方向键以 10% 调整，Home/End 可移到边缘。擦除模式明确标注“仅供人工视觉比较，不是内容一致性校验”。两图视口和擦除视口均铺设棋盘格，以便看见透明区域。

擦除层直接绑定既有 `AsyncThumbnailImage.Source`，沿用媒体比较已有的后台解码、2048 px 解码长边和 64 MiB 缓存上限；不复制解码、不读额外目录。擦除模式关闭并排卡片中的缩放控件，避免按钮看起来可用但只作用于被遮住的图像。

## 受控验证

- SDK `8.0.423` Release 全解决方案构建：`0 warning / 0 error`。原始构建记录位于 `build-console.txt`，SDK 选择记录位于 `sdk-version.txt`；诊断 ID：`20261020T030957068Z-96f3ebaf`，构建源提交为 `a054ddf230a148d0661975ec22428814c1e901aa`，当时包含本轮三个未提交源码文件。
- `MediaComparisonViewportBehaviorTests`：`2/2` 通过。新用例打开真实生产 `MediaCenterView`，加载两张有透明带的合成图片，核对擦除层复用同一 `BitmapSource`、分隔宽度、A 左/B 右的像素颜色、方向键/Home/End、棋盘格实际渲染及退出模式后的并排控件恢复。TRX SHA-256：`F039CE9F3980FCBADD7F33557FEF640D7BA52701F31AB37B9831F267AFA2F4BF`。
- 合成截图 [media-wipe-comparison.png](media-wipe-comparison.png) 来自测试中对生产擦除卡片的 WPF `RenderTargetBitmap` 渲染，能看到左侧 A、右侧 B、透明带棋盘底纹和中间分隔线。SHA-256：`19F2DFFA4886226563A364B779F42E417A4C89827A9F718A3389A0D2B1802714`。
- `scripts/check-xaml.ps1` 检查 25 个 XAML 文件通过；`python scripts/validate-source.py .` 与 `git diff --check` 通过。

构建产物身份：`GameSaveCenter.Playnite.dll` SHA-256 `D41B4BBB15C016A0F9FD392AE7B66A9D6983AAD8CBCC7F1795373B4744AF2128`；测试程序集 SHA-256 `5AF6CE5D4482F0752C192462AE23785A5B97DE8A962E508054EA241DC2007F13`；测试 PDB SHA-256 `E31A4F9A9F0EFDA8C34BC75D2B8E9E99162A13C5A00B8490E1AC04738A0710DE`。输入源码 SHA-256：XAML `6F0D9F7626D90DE6F5763BF69F0EC7AEEF672BAA8D012A4F06700002009A4FB7`，代码后置 `D494A78EE09985E4818D29DF00D67E40D2F84963E8D43DB9C4FF5D92CE42E16D`，行为测试 `5EDFE53A8919699B10D62103AE357CC214ED065E32A2D6EF694FDF2470EA18CC`。

## 边界

测试退出码为 0，TRX 无失败。VSTest 关闭 WPF 测试进程时，控制台仍打印 `TextServicesContext` / `TextServicesHost` 的 `InvalidComObjectException` 清理输出；该输出未改变进程退出码或用例结果，本轮未绕过或隐藏它。截图和文件均为合成夹具与离屏逻辑 DPI；未运行真实 Playnite 窗口、真实媒体或物理鼠标拖动，也不构成内容一致性结论。
