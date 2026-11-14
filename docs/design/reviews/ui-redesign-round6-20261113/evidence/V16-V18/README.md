# V16–V18 · 详情切换、通知重排与加载交接

## 本批可见改进

- 任务详情选择变化按 `DispatcherPriority.Render` 合并；回调读取最新 `SelectedTask`，不让旧对象的过渡排队。媒体选择同样只对最新绑定对象淡入。两个页面的业务选择和命令仍来自原有 ViewModel/Binding。
- Toast 新增时旧卡片平滑让位，移除时只移动删除项下方的卡片。按卡片代次使旧完成回调失效，连续变化从当前可见偏移重新定向；不抢焦点，保留最多4条、原计时和关闭命令。重排不调用同步 `UpdateLayout()`。
- 缩略图仍立即进入真实 `Loading` 状态；只有持续超过180ms且控件仍可见/已加载才出现“正在载入缩略图”。快响应不会短暂闪提示。图像到达后使用共享transition淡入；取消、换源、隐藏、卸载和错误都会撤掉等待状态，解码和业务完成时间没有人为延迟。

V13–V15中查出的共享按钮断言仍要求旧的 `GscMotionFast` hover 和100ms press，而Demo生产样式使用 `GscMotionHover` 与90ms press。本批同步更新三处契约测试，没有调整生产token迎合旧断言，也没有删测或增加skip。

## 本地验证

| 检查 | 结果 |
| --- | --- |
| SDK 8.0.423 Release完整 `scripts/build.ps1` | 编译0 warning/0 error；Core 157/157、Worker 407/407；Playnite隔离测试2048项、1990通过、0失败、58个既有skip，157个WPF类进程全部完成 |
| Toast + 缩略图定向测试（最终Toast重排修正后） | 7/7通过，0失败；包含插入/移除时真实CompositionTarget渲染样本、焦点保持、加载延迟提示与快响应隐藏 |
| `python scripts/validate-source.py` | 通过 |
| `scripts/check-xaml.ps1` | 27/27通过 |
| `validate_wpf_ui.py src/GameSaveCenter.Playnite` | 0 errors / 26 warnings / 164 info；均为已有静态提醒 |
| `git diff --check` | 通过 |

完整Release在本轮最后两处Toast布局优化之前完成；这些布局优化随后以生产项目和测试项目重新编译，并由7项Toast/缩略图测试通过。候选SHA的完整构建、CI和渲染QA仍以最终候选后的记录为准。

## GitHub首次查询与修复

提交 `399f048b41bb9fdcdd3dd7c19a15342ff65ba194` 的run #1574 / `43579451033` 曾因三个过期动效断言失败：`UiFinesseRound2ControlSourceTests`、`UiFinesseFoundationTests`、`WpfUiResourceDictionaryTests`。本提交同步修正到当前生产令牌；旧失败保留作诊断，不通过扩大skip或关闭检查掩盖。该run适用jobs为 `132400836237` 与 `132400836498`，诊断artifact `14108115621`。本节后续补记新候选的完整run/job/artifact身份。

## 宿主边界

动画行为取样来自受控WPF测试窗口和 `CompositionTarget.Rendering`，不是实际Playnite前台录屏；真实Playnite宿主呈现、用户桌面帧率、物理DPI与Narrator尚未验证。V20另提供带时间戳的实际WPF采样和可播放轨迹，并清楚标注该差异。没有访问真实游戏库、存档、恢复、配置或云数据。
