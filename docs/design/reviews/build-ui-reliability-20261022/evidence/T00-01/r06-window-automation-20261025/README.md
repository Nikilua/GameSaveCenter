# R06 详情窗口 UI Automation 收尾

提交0832aca8默认根入口构建run `20261025T054600655Z-8ab0ccc5`、安装run `20261025T054558590Z-04e1f10d`：SDK8.0.423、Release0警告/0错误、Core157/157、Worker400/400、Q06通过。第59/145类R06深色1400 DIP场景在UpdateLayout中因WindowAutomationPeer调用GetWindowTextLength触发Win32参数错误，累计1395项1376通过/18跳过/1失败；未进入发布/打包/安装。

夹具为Window设置明确AutomationProperties.Name，避免自动化名称退回原生标题读取，并在Close后等待Dispatcher的ApplicationIdle回调。UIA保持开启，原有双主题、1120/1400 DIP长详情边界、四行可读、独立滚动及选择更新断言保留。

修正源码SHA256 `4B63CEAB18C058B5CBE67DE85944863AE1BBED1B978C69A72427796A089CEA3E`。SDK8.0.423隔离Release0警告/0错误，五次独立R06测试进程各5/5，共25/25通过；源码校验通过。失败摘要/TRX/console及五轮修正后TRX/console已脱敏归档。新候选仍须执行默认根入口完整链。
