# R07-08 当前 main resize 行为复核

日期：2026-10-14。当前 main 提交 d7f6fc42；生产与测试源码和已构建源码基线 552b8168 相同，两个后续提交仅更新诊断与阶段文档，git diff 552b8168..HEAD -- src tests 为空。

## 实现与行为结果

复用了现有 TaskCenterView 自适应详情、AcrylicProductionShellView PageHost 尺寸传播、GamePicker 覆盖层及生产路由按钮；没有改生产代码、布局服务、命令或 DTO。测试使用合成失败任务 DTO 和隔离 STA WPF Window。

独立复跑 R07ResizeStressBehaviorTests.ResizeSequenceKeepsOpenTaskDetailsAndPickerFocusReachable：1 passed / 0 failed / 0 skipped，VSTest exit 0。此次使用 R08-05 当前 main 隔离 Release solution 输出；该构建对应同一 552b8168 源码基线，solution 0 warning / 0 error、XAML 24/24。SDK 为 9.0.302，VSTest 17.14.1。程序集 SHA、ProductVersion、MVID 与生产/测试源 SHA 见 assemblies.json 和 hashes.txt。

测试先在已加载生产 DataGrid 选择合成任务；窄布局从详情关闭开始，然后通过生产按钮 Click 打开详情，并检查打开/关闭按钮状态。GamePicker 覆盖层保持可见。后续依次测量 1366×900、960×700、960×560、1440×900、1366×900 DIP 的宽、窄、短、宽、恢复序列：

| 阶段 | Window 实际尺寸 | PageHost 实际尺寸 | 详情 | Search 焦点 | Picker | 详情 MaxHeight |
| --- | ---: | ---: | --- | --- | --- | ---: |
| wide | 1366×900 | 1038.667×759.333 | Visible，360×476 | 保留 | Visible | Infinity |
| narrow | 960×700 | 632.667×559.333 | Visible，632.667×136 | 保留 | Visible | 136 |
| short | 960×560 | 632.667×419.333 | Visible，632.667×136 | 保留 | Visible | 136 |
| wide-again | 1440×900 | 1112.667×759.333 | Visible，360×476 | 保留 | Visible | Infinity |
| restored | 1366×900 | 1038.667×759.333 | Visible，360×476 | 保留 | Visible | Infinity |

五阶段均断言任务表正高度、无残留有限 MaxHeight；详情和 picker 保持非空尺寸，窄/短布局下详情与 picker 获得有限高度，恢复宽窗后 PageHost 与初始尺寸差小于 1 DIP。生产 shell 收到 11 次 SizeChanged。脱敏 TRX 保存在 tests/。

## CI 和未验边界

历史 #968 两个 SDK lane 的门禁结果按既有 CI 证据记录为成功；早先 #960/run 38327521241 是终态失败，完整日志 API 403/404，内部 SDK、失败命令和根因未知，不由本机结果解释。当前 d7f6fc42 的 Actions run 在本次检查时尚未出现在公开列表（页面最新显示 #979/ae3c565）；按 head SHA 查询公开 REST API 返回 HTTP 403 rate limit，不能据此判断 run 不存在。本机 SDK9 结果不替代仓库 SDK8.0.423 或 CI SDK10.0.401，本阶段 CI 状态仍待终态。

此处是受控 STA WPF 逻辑 DIP 行为，不是 Playnite 实际拖动、物理 DPI/跨屏、UI Automation/读屏、最终呈现帧或宿主性能/布局耗时证据。窗口尺寸更新并经过 dispatcher idle 后检查恢复几何与状态；未测量真实鼠标拖动的帧时间。下一步等待本次推送的 CI 终态，再推进依赖已满足的 R10-02；R09-05 外机版本身份待用户可回复后补证，不阻塞独立工作。
