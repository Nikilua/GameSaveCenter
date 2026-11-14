# V19–V20 · 中断降级与动态样片

## 行为覆盖

- 页面导航测试覆盖连续切页/反向以及窗口卸载时关闭浮层、撤销时钟和保留最终导航焦点。
- 存档详情测试现在在开合中切换为系统Reduced Motion，断言活动Opacity/Translate时钟立即清除、详情回到当前打开终态；随后关闭立即折叠。既有偏好测试覆盖启用界面动画开关中止当前进入动效且重新启用不重播旧动画。
- 缩略图卸载测试覆盖正在解码的请求取消、过期结果不发布；Toast/媒体偏好关闭沿用`Normalize`路径。
- `RuntimeMotionEvidence`只有在指定`GSC_MOTION_EVIDENCE_PATH`时才订阅`CompositionTarget.Rendering`并写CSV；普通测试运行不会写文件。数据包含UTC采集ID、phase、WPF实际elapsed milliseconds、frame序号、Opacity、Transform、Visibility和有效可见性。

## 精确候选采样与回放

提交 `9097e6c86a8d5a8808eaab03b8b92c1a318b6ece`（2026-11-14）上SDK8.0.423 Release定向运行3/3通过，0失败/0跳过；TRX归档为[`motion-frames-9097e6c8.trx`](motion-frames-9097e6c8.trx)。CSV共147条记录：136条实际`CompositionTarget.Rendering`回调、11条交互状态快照。

| 采集ID（UTC） | 回调/状态 | 实测范围 | 最大相邻回调间隔 |
| --- | ---: | --- | ---: |
| `page-navigation-20261114T110943733Z` | 36 / 3 | PageHost Opacity 0–1，Y 0 DIP；包括快速反向到首页 | 167.7 ms |
| `save-details-20261114T110939003Z` | 75 / 6 | Opacity 0–1，Y −6 至 8 DIP；覆盖开合、反向和Reduced Motion终态快照 | 280.1 ms |
| `toast-stack-20261114T110942318Z` | 25 / 2 | 受影响卡片Y −114.67 至 0 DIP；插入后回到0 | 38.1 ms |

详情测试对象在所有采样帧`Visibility=Visible`时仍有`IsVisible=false`（受控测试窗口内的有效可见性状态），因此详情回放只证明属性时钟确实变化，不证明其像素出现在屏幕。PageHost和Toast轨迹的有效`IsVisible`为true。最大的帧间隔原样写在CSV，页面和详情场景明确出现长间隔；这份数据不构成60fps或DWM流畅度证明。

可直接打开[`motion-player.html`](motion-player.html)播放上述实际采样：有速度选择、播放/暂停、帧滑杆及Opacity/Y DIP时间曲线。播放器使用采集时的`elapsed_ms`和属性值，不插帧；有效不可见或Collapsed的行显示为空画面并保留状态读数。`motion-player-preview.png`是播放器界面截图，只供检查播放器布局，不是动画证据。原始[`motion-frames.csv`](motion-frames.csv) SHA-256：`E35876367ABEC6F4EEDDD5AAF661A90FB67899E52EE66D1EFA4640B2C77DE771`；播放器 SHA-256：`5D69310A5FEB7BFCFB8ED541281D2E4C6C5E5B0B61065A03A2AAD350C87C99B1`；TRX SHA-256：`702EF289C534AA9D41A87D691A2B624AE040D073F7E3F14CA8BDAFB4D9DF4836`。

使用Microsoft Edge无头实例打开播放器并交互检查：3个场景出现、Toast采样播放时间/帧滑杆推进、详情不可见提示正确、浏览器控制台0错误。界面预览SHA-256：`8F9F259A589959B1B14F2B6258A3666DD72DC6C9D7324E4D5944C522189F0273`。

## 未验边界

真实Playnite前台、真实桌面录屏、物理DPI与用户系统Reduced Motion设置未运行。焦点保持由现有自动化断言覆盖，但详情采样窗口不用于真实键盘/屏幕阅读器验收。宿主与权限待验边界继续保留。
