# Q10-07 设置强度滑杆：轨道命中与键盘步进

日期：2026-09-28。实施提交 `e007a33992c98a04dbb1113b451948a56302baea`；生产 Playnite DLL `0.6.73+e007a33992c98a04dbb1113b451948a56302baea`，SHA-256 `1C2EE0EABF62FB40E3E6872AFE83E856ACF3234EF6AFF710BD2220D6B18A6CA1`。Demo 原目录在本工作区不可用，沿用已恢复的生产基线，没有另换设计体系。

## 已有能力与复现

实际消费点是 `Settings/GameSaveCenterSettingsView.xaml` 的 `GlassStrengthSlider`：范围 20–100，百分比 TextBlock 绑定同一 Slider.Value，数据双向绑定 `GlassEffectStrength`；共享样式为 `Themes/DesignTokens.xaml` 的 `GscSlider`。旧 Q04–Q12 开发探针只证明 150 DIP 滑杆可见、模板存在，**没有**证明轨道上下命中或键盘改变时标签/源值同步。

在真实生产设置视图、浅/深主题的 WPF Window 中，新行为用例对旧共享模板先运行，结果 `0/2`：轨道按钮中心线以上 7 DIP 处 `VisualTreeHelper.HitTest` 为 `none`（示例 `point=76,4,hit=none`）。旧模板的 RepeatButton 根就是 4 DIP 高的 Border，这证实了可见细线同时限定了命中高度。旧样式也没有规定 SmallChange/LargeChange，使每次键盘步进与显示的整数百分比缺少明确契约。

## 修复与自动行为

- 共享 `GscSlider` 仍画 4 DIP 轨道和 18 DIP 圆形滑块，但 RepeatButton 用透明 Grid 承载细线；整个 32 DIP 高轨道区域可命中。Thumb 模板保留 18 DIP 可见圆形，控件命中面积扩至 `32×32 DIP`。
- 显式设 `SmallChange=1`、`LargeChange=10`。未改设置 DTO、保存/取消、主题资源颜色或其他业务命令。
- 精确提交 Release solution（Playnite `net462`、测试 `net472`）`0 warning / 0 error`，XAML structural `24/24`，`validate-source.py` 通过，WPF 静态审查 `0 errors / 30 warnings / 177 info`。静态警告为既有通用布局/资源提示，本次没有新增 error。
- 新用例在生产设置视图的 Light、Dark、FollowPlaynite 各 `1/1`：沿轨道两侧的上/中/下三个点命中各自 RepeatButton；受控 `OnClick` 使 `78→68→78`，实际 `Key.Right/PageUp/End/Home` 使 `78→79→89→100→20`，双向源值和百分比标签同步。UIA RangeValue 可读；禁用后拒绝焦点和 UIA 写入。`1100×700` 与 `560×640 DIP` 下仍有可用宽度，短窗通过生产设置滚动面 `BringIntoView` 完整到达。
- 当前机器 WPF 测试宿主实际 DPI scale `1.5`；窄/短窗口是逻辑尺寸变化，不等于物理 125%/200% 或跨屏。当前末态量得控件 `316×32 DIP`、Thumb `32×32 DIP`、百分比标签独立无重叠。

精确身份的类隔离回归分别为 `ReportedWorkspaceLayoutBehaviorTests 13/13`（含 Q10-07 新用例 `3/3`）、`R09ShadowBudgetBehaviorTests 3/3`、`UiFinesseRound2ControlSourceTests 24/24`，合计三个**独立 testhost** `40/40`，0 fail/skip。此前把三类混在一个 testhost 的尝试得到 `39 passed / 1 failed`：R09 阴影用例在全局资源状态被其他 WPF 类改变后预期的 `DropShadowEffect` 缺失；R09 随即单类复测 `3/3`，精确提交身份也单类 `3/3`。这是隔离策略的必要性，不删去混跑失败事实，也不把三次分进程结果写成单次全绿。

`GameSaveCenter.RenderHarness.exe settingsthemeprobe` 在同一提交下对生产设置页 `1040×700 DIP` 生成浅/深两张选中外观页原图；滑杆、圆形 Thumb 和右侧 `78%` 标签可见且无重叠，报告 `popupOpen=True`、`tooltipOpen=True`，exit 0。原始 TRX/图片只保留本机忽略目录 `artifacts/q10-07-slider-20260928/`；其路径可能带本机环境信息，未提交。

## 状态与后续

Q10-07 的当前受控 WPF 交互与双主题组件视觉已补证；真实 Playnite/package-host 的物理鼠标、键盘、UIA/读屏、125%/200% DPI、最终呈现帧仍未验，Round2 最终状态保持“未完成”。没有安装到用户扩展目录或读写真实设置/存档/媒体/云端。第三轮 R 台账 192 项计数不变。

下一独立小批量为 `Q10-08`：先核对设置页密集 Toggle/CheckBox 组在窄短窗口中的真实换行和帮助文字归属，已有能力满足时补行为/几何证据，不因本项顺手重做视觉。用户 Media Inbox 滚动空白仍优先等待真实隔离宿主日志，不能从滑杆或离屏设置图推断修复。
