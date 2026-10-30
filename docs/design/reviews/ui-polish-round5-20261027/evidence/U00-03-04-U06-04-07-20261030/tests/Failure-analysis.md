# 首次测量宿主失败说明

最初新增的可见性测试只调用测量宿主，没有把视图连接到 WPF `PresentationSource`。因此 `IsVisible` 断言受未挂载的测试宿主影响；失败不代表生产页面展开后不可见。

测试现在使用低透明度 `Window` 承载页面，并刷新窗口布局；单测通过，随后 `CompactWorkspaceTableViewportBehaviorTests` 全类以 14/14、0 skip 通过。原始首次失败结果保留在 [TRX](initial-measure-only-host-failure.trx)，最终回归结果见 [TRX](compact-workspace-final.trx)。
