# U06-07/08 候选表高窗与大列表

2026-10-28。候选路径表此前把可用高度截断在560 DIP，本批沿用历史表的有限视口预算和最小四行保障，移除该上限；宽布局详情同步使用表格高度，紧凑布局依旧独立滚动。命令、绑定、滚动条及选框未改。

同条件Render QA 3840×2160逻辑DIP，候选表由1747×560变为1741×1665；对照[before](render/before.png)和[after](render/after.png)，完整报告及394张图哈希见render/。1040×700仍有5行可读。RenderHarness Release 0 warning/0 error，scripts/render-qa.ps1退出0、双主题394图/0 PROBLEM；报告源码基线e4285c05、工作树含本批未提交候选，不称作最终提交精确SHA。实际物理4K及Playnite未验。

SDK8.0.423 Release最终组合4/4、0 skip，包括高窗扩展与900×600回缩/内部滚动、1040×700可达性、R18的2000/10000/20000项任务与媒体夹具、CI悬停修正。任务表视口8行最多实现10行，媒体后端2万项时UI保留2000、视口7行最多实现7行；任务Recycling、媒体Standard既有例外保持。检查实现行及其RenderTransform无活动动画属性，并核对共享行模板无逐行Storyboard/Animation；不是实际宿主内存或呈现帧率测量。

首轮候选表高窗测试因独立夹具未加载到窗口，候选页IsVisible=false而取到历史页旧视口；已在真实加载的WPF窗口中断言候选页可见并重测，原始失败保留。中途测试窗口代码插入错误导致一次编译失败，已修正并重新编译；没有删断言、扩skip或替换SDK。TRX和hash见tests/。

Source validation、XAML26/26、WPF静态37 XAML/0 error/26既有warnings/179 info通过。阶段空间及精确自有中间TRX清理清单见disk-before/after和cleanup.json；当前截图与失败诊断保留，其他任务TRX/TestResults、真实存档和配置未动。

本批保持代码完成待验收：真实Playnite、物理DPI/键鼠、UIA客户端、长期宿主性能未验；提交后GitHub双SDK job及整轮最终中文根入口完整构建/测试/发布/打包/安装身份/启动和独立scripts/build.cmd仍待验。

此前969bfc20的[run 40640556766](https://github.com/Nikilua/GameSaveCenter/actions/runs/40640556766)基线SDK8.0.423与SDK10.0.401 job均成功，各1986总/1928通过/58既有skip/0失败；三个artifact身份见[CI清单](ci-run-40640556766.json)。required-checks此前403仍不推断。U06-03悬停CI失败及单独修正见[失败清单](../U06-03-04/ci-hover-failures.json)；修正提交3ecacdc0的双SDK run 40647037308仍运行，不能提前判绿。
