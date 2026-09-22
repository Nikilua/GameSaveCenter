# R22-03 复制反馈轻量（2026-09-21）

## 结论

本批按“已满足，待环境验证”收口。实现提交为 `5a21e18e`（`补齐复制结果反馈`），已推送到 `origin/codex/ui-finesse-round2`。

## 现状核对与实现

- 先核对最新实现：DataGrid 已有按 `Profile` 的 TSV 白名单格式化和剪贴板重试入口，但复制失败原先静默；设置页路径复制已有本地状态文本；Dashboard 的任务详情、路径、诊断、维护报告和结果详情复制入口已存在，但部分只走全局通知或直接写剪贴板。
- 新增 `Infrastructure/ClipboardFeedback.cs`，复用现有 `FeedbackToast` 自动化 Peer 和 `GscRedesignFeedbackToast*` 主题资源，在发起控件下方显示短暂 Popup。Popup 不进入布局、不获取焦点；同一目标复用同一 Popup，全局活动状态会关闭前一条，因此连续复制不会堆积反馈卡片。
- DataGrid 的 `Ctrl+C`/`Ctrl+Shift+C` 成功、空内容和剪贴板占用失败均经过同一反馈；失败文案明确“请稍后重试”。生产表的现有 Profile、选框、滚动条、虚拟化和脱敏格式保持不变。
- 设置页完整路径复制与通知详情复制接入同一反馈；设置页状态 TextBlock 同步更新 Automation HelpText/Name。Dashboard 复制通知增加 `IsCopyFeedback` 标记：有焦点源控件时定位到原控件附近，没有焦点的程序化调用才回退到单一可合并 Toast；不新增模态窗口。
- 既有 `ClipboardRetry`、命令绑定、取消/错误、恢复保护和 Playnite `net462` 目标保持；没有修改真实存档、媒体、用户云端或诊断发送路径。

## 验证证据

- `R22CopyFeedbackBehaviorTests 2/2`：实际 STA Window 中让 DataGrid 成功复制、连续复制和 fake 剪贴板占用失败；确认 Popup 仍是同一实例、PlacementTarget 为原 DataGrid、目标宽高不变、Popup 不可聚焦；失败结果通过 Automation Name/HelpText 回读并包含重试信息。
- 相邻定向回归：`R06ClipboardBehaviorTests 4/4`、`R15TaskFailureCopyTests 6/6`、`R16SettingsPathEditorSourceTests 1/1`；R21 两个独立 STA 场景 `1/1 + 1/1`。
- 使用提交身份 `5a21e18e` 的隔离 Release 编译：Playnite `net462`、Tests `net472` 无错误；保留既有 `MediaCenterView.xaml.cs:671 CS8602` 两条 warning。`validate-source.py` 通过，`git diff --check` 通过，WPF 检查为 `0 errors / 27 warnings / 177 info`，warning/info 与此前基线一致。
- 使用 fake 剪贴板 setter、合成任务 DTO 和隔离隐藏 Window；未以 `Assert.Contains` 单独签收交互，测试实际检查 Popup 复用、定位、无布局变化、焦点和失败负例。

## 未验边界

没有启动真实 Playnite/package-host，没有把隔离 Window、Popup 几何或代理结果写成最终呈现证据；真实 Windows 剪贴板、UIA/读屏、键盘/IME、物理 DPI/跨屏 Popup、主题切换后的真实呈现帧、ETW 和宿主性能仍待验。Demo 原目录不可用，视觉继续沿用已恢复生产基线。未修改 dirty main，也未访问真实存档、媒体目录、用户云端或外发诊断。

## 下一步

下一可执行项为 `R22-05` 批量数量防歧义：先盘点现有跨筛选选择、当前结果数和隐藏选择提示，再以行为负例推进，不改变选框、滚动条和批量命令安全门控。
