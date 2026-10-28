# U07-01/02 对话框阅读与动作间距

2026-10-28，基线ac0afe72。生产危险/普通/三选项对话框共用Dashboard DialogOverlay。此前动作StackPanel的取消/三选项按钮没有相邻间距；改为WrapPanel，非末项8 DIP右/下间距，确认按钮保持末项。窄空间长按钮可换行，不隐藏操作。对话标题、说明、对象及后果沿用真实请求文本；长正文保留独立ScrollViewer。恢复请求仍包含游戏、版本、可恢复性、PreRestore及关闭游戏/启动器提示。

SDK8.0.423 Release最终组合10/10、0失败、0 skip：生产XAML结构检查长正文滚动、按钮顺序和间距；采用生产按钮样式与XAML间距的220 DIP受控WrapPanel检查长确认文案无重叠或越界；危险确认不将确认设为Enter默认，实际生产Escape处理器在合成KeyEvent下取消并回焦，关闭中重复确认及重叠请求安全取消。没有执行真实恢复、删除或云写入。首次Escape测试反射匹配到继承重载引发AmbiguousMatchException；限定Dashboard自身声明后保留全部断言并通过，失败及最终TRX在tests/。

前后：生产取消/确认和三选项相邻间距由0变为8 DIP，动作容器由水平StackPanel变为WrapPanel；标题、正文和安全策略未被删除。RenderHarness Release0 warning/0 error，scripts/render-qa.ps1退出0、双主题394图/0 PROBLEM，报告和逐图SHA见render/；报告Commit=ac0afe72且WorkingTreeClean=False，不称为新提交精确身份。Render QA是离屏页面回归，不能冒称真实生产对话框宿主截图；动作几何来自受控WPF夹具，长正文实际宿主滚动仍待验。

Source validation、XAML26/26、WPF静态37 XAML/0 error/26既有warnings/179 info通过。disk-before/after及cleanup.json记录本阶段空间和安全清理三份已被最终结果替代的临时成功TRX；保留最终/失败诊断和当前394图，其他任务TRX/TestResults、存档和配置未动。

旧CI悬停失败修正3ecacdc0的[run 40647037308](https://github.com/Nikilua/GameSaveCenter/actions/runs/40647037308) SDK10 job 122678147262已成功，1987总/1929通过/58既有skip/0失败；artifact身份见[清单](ci-hover-fix-sdk10.json)。基线job122678147577仍在运行，不能称整run成功。required-checks接口此前403不推断保护状态。

U07-01/02代码完成待验收；真实Playnite、物理键盘/IME/UIA、实体DPI和长正文实际宿主交互未验。本批新提交双SDK CI及整轮最终中文根入口完整构建/测试/发布/打包/安装身份/启动、独立scripts/build.cmd仍待验证。
