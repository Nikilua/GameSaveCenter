## 2026-10-29 U09-02/03媒体STA清理候选

f1eb8f8f在C盘真实中文根链退出1：MediaInboxScrollBehaviorTests的WPF TextStore窗口移动回调跨STA访问导致宿主崩溃。该类没有发布用例结果，Sequence标记outputScale1.25未完成；已完成部分1227 pass/18 skip不能代表完整通过，独立build.cmd未启动。诊断及Sequence已用短路径归档。

新增保留IME启用的真实TextBox生命周期回归，旧夹具因Dispatcher未关闭准确失败。仅在共享RunSta finally中于所有者线程InvokeShutdown，清理异常亦返回失败，未关闭IME或改滚动/锚点/虚拟化断言、三种输出缩放或skip。按正式隔离配置连续三轮7/7，共21/21、0 skip。缺失清理已被直接证明，旧崩溃唯一因果不作过度归因；新候选仍须中文根链、独立build.cmd及双SDK CI，U09-05继续进行中。

证据：[验证清单](validation.json)。WPF文本服务按Dispatcher管理并监听关闭事件的架构参考：[TextServicesHost源码](https://github.com/dotnet/wpf/blob/main/src/Microsoft.DotNet.Wpf/src/PresentationFramework/System/Windows/Documents/TextServicesHost.cs)；本地实际验证运行时为.NET Framework4.8，不以main源码代替本地证据。
