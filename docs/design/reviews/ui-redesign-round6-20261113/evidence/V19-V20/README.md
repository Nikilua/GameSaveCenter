# V19–V20 · 中断降级与动态样片

## 行为覆盖

- 页面导航测试覆盖连续切页/反向以及窗口卸载时关闭浮层、撤销时钟和保留最终导航焦点。
- 存档详情测试现在在开合中切换为系统Reduced Motion，断言活动Opacity/Translate时钟立即清除、详情回到当前打开终态；随后关闭立即折叠。既有偏好测试覆盖启用界面动画开关中止当前进入动效且重新启用不重播旧动画。
- 缩略图卸载测试覆盖正在解码的请求取消、过期结果不发布；Toast/媒体偏好关闭沿用`Normalize`路径。
- `RuntimeMotionEvidence`只有在指定`GSC_MOTION_EVIDENCE_PATH`时才订阅`CompositionTarget.Rendering`并写CSV；普通测试运行不会写文件。数据包含UTC采集ID、phase、WPF实际elapsed milliseconds、frame序号、Opacity、Transform、Visibility和有效可见性。

## 首轮受控采样

SDK8.0.423 Release过滤运行3/3通过：快速详情反向、页面导航及反向、Toast卡片重排。首次采样输出139行：页面27个渲染事件、详情75个、Toast26个，来自实际WPF `CompositionTarget.Rendering`回调，不是预设关键帧或静态截图。采样时测试宿主为受控WPF窗口；Toast与页面对象可见，详情对象的`Visibility=Visible`但`IsVisible=false`，因此详情轨迹只代表实际动画属性时钟，不能证明屏幕上绘制了详情像素。测试窗口使用自动化夹具，不是Playnite宿主。

精确提交后的复采将保存为同目录`motion-frames.csv`，并附带可交互属性回放页；回放会逐行使用实际时间戳与Opacity/位移数据。它不是视频录屏，不表示DWM/物理显示帧率。每段的长帧间隔保留在CSV，不据此声称60fps。

## 未验边界

真实Playnite前台、真实桌面录屏、物理DPI与用户系统Reduced Motion设置未运行。焦点保持由现有自动化断言覆盖，但详情采样窗口不用于真实键盘/屏幕阅读器验收。宿主与权限待验边界继续保留。
