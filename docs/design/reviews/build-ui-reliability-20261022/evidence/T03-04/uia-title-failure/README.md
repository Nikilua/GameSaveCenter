# 标准链中的未命名测试窗口标题读取崩溃

`aefbddfa` 标准根入口run `20261024T023430859Z-655cef77` 退出1，Release0警告/0错误、Core157/157、Worker400/400；前100个WPF类结束，第101/145类R12RemoteStageProgressBehaviorTests在UI窗口用例中testhost崩溃。剩余44类未执行，未发布/打包/安装/启动。原摘要、类控制台、TRX与脱敏根控制台在本目录；原诊断和编译目录仍保留。TRX中没有failed用例不表示该进程成功。

堆栈指向WindowAutomationPeer.GetNameCore读取原生窗口标题；原生错误码未记录，乱码消息不用于推断具体错误码。相同原DLL独立复跑5/5，说明本次不能声称确定性复现。完整链失败不被后续小范围通过覆盖。

本机PresentationFramework4.8.9181.0的只读IL检查见framework-get-name-il.txt：先调用FrameworkElementAutomationPeer.GetNameCore；空名时进入原生GetWindowText/GetWindowTextNoThrow。较新的[微软WPF实现](https://github.com/dotnet/wpf/blob/main/src/Microsoft.DotNet.Wpf/src/PresentationFramework/System/Windows/Automation/Peers/WindowAutomationPeer.cs)也保留显式名称优先及原生标题回退，不能将新版的异常处理推断为本机4.8行为。

本次只给该夹具窗口设置明确Title和AutomationProperties.Name，并在Show后断言仍为正常WindowAutomationPeer、GetName等于语义名称。没有禁用UIA、替换空peer、忽略Win32异常、删除业务断言或扩大skip。编译0警告/0错误，命名后连续三次独立5/5，原始及三次TRX分别保留。

当前仅证明夹具的显式命名路径及局部运行通过；不能据此宣布全局UIA权限问题解决或完整安装成功。新候选须提交后再执行默认标准全链，原失败保持失败。
