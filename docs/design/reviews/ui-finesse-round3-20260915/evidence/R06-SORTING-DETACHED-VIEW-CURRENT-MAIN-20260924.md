# R06 排序在集合视图失效后崩溃：当前 main 复核

日期：2026-09-24  
代码/测试基线：`ade4b937`（测试由同一 checkout 的隔离 Release 输出运行）

## 触发原因与修复

只读检查用户提供的 `crash.zip` 中 Playnite 日志，排序列头点击路径在 `DataGridStableSortController.ApplyCurrentSort()` 结束 `DeferRefresh` 时抛出 `NullReferenceException`，栈经过 WPF `ListCollectionView.PrepareLocalArray()`、`RefreshOverride()`、`DeferHelper.Dispose()`、`ToggleSort()` 和 `OnSorting()`。结合 WPF 实现，集合视图已从 source detach 后仍尝试刷新是与该栈一致的触发条件。

修复让排序控制器在应用/切换排序前重新解析 `ItemsSource`，拒绝 `SourceCollection == null` 的失效视图；`ApplyCurrentSort` 仍同步排序箭头，但在刷新前退出。无效或过期列头事件由自定义控制器消费，避免 WPF 落入反射排序路径。集合视图失效时，排序方向和箭头维持控制器当前状态。

## 行为证据

- `R06SortingBehaviorTests`：`7/7` 通过，0 失败/跳过。
- 新增真实 WPF `DataGridColumnHeader` 点击行为：点列头后数据按生产稳定比较器升序排列，再点一次按降序排列，排序箭头同步变化。
- 新增失效视图负例：显示真实 WPF Window 及 DataGrid，调用公开 `DetachFromSourceCollection()` 后点击实际列头；验证无异常、活动列/方向不变、活动箭头恢复且被点击的非活动列没有伪箭头。
- 完整隔离 Release solution 编译成功，XAML `24/24`，0 errors；保留两条既有 `MediaCenterView.xaml.cs:703 CS8602` warning。
- TRX：[R06-SORTING-DETACHED-VIEW-CURRENT-MAIN-20260924.trx](R06-SORTING-DETACHED-VIEW-CURRENT-MAIN-20260924.trx)。

最初通过常规 `dotnet test` 运行时，项目默认 `bin` 中的旧测试程序集因 `GscBuildCommit` 与当前源码不符，被仓库身份门拒绝；这不是测试失败。改为直接运行当前 checkout 的 `.tmp/sortfix` 隔离 Release 测试程序集后，身份门通过且 `7/7` 通过。

## 范围与未验边界

本批只修复排序崩溃，没有签收整个 R06-02，也没有改列宽、排序默认值、DTO 或数据源策略。行为证据来自合成备份 DTO 和隔离 STA WPF Window；真实 Playnite/package-host 列头鼠标输入、触屏输入、当前用户库、宿主呈现帧和用户实际加载包身份仍未验。没有访问或修改用户存档、媒体、云端数据。

WPF 实现参考：[ListCollectionView.PrepareLocalArray](https://source.dot.net/PresentationFramework/System/Windows/Data/ListCollectionView.cs.html)、[CollectionView.DetachFromSourceCollection](https://source.dot.net/presentationframework/system/windows/Data/CollectionView.cs.html)。

下一可执行小批量：修共享 `GscRoundedDataGridRowTemplate` 的选中/键盘焦点轮廓，不让视觉层改变 cell/content 几何；以真实 WPF 行为比较选择前后每列内容位置。
