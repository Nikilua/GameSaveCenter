# U06-07/08 候选表高窗与大列表

2026-10-28。候选路径表此前把可用高度截断在560 DIP，本批沿用历史表的有限视口预算和最小四行保障，移除该上限；宽布局详情同步使用表格高度，紧凑布局依旧独立滚动。命令、绑定、滚动条及选框未改。

同条件Render QA 3840×2160逻辑DIP，候选表由1747×560变为1741×1665；对照[before](render/before.png)和[after](render/after.png)，完整报告及394张图哈希见render/。1040×700仍有5行可读。RenderHarness Release 0 warning/0 error，scripts/render-qa.ps1退出0、双主题394图/0 PROBLEM；报告源码基线e4285c05、工作树含本批未提交候选，不称作最终提交精确SHA。实际物理4K及Playnite未验。

SDK8.0.423 Release最终组合4/4、0 skip，包括高窗扩展与900×600回缩/内部滚动、1040×700可达性、R18的2000/10000/20000项任务与媒体夹具、CI悬停修正。任务表视口8行最多实现10行，媒体后端2万项时UI保留2000、视口7行最多实现7行；任务Recycling、媒体Standard既有例外保持。检查实现行及其RenderTransform无活动动画属性，并核对共享行模板无逐行Storyboard/Animation；不是实际宿主内存或呈现帧率测量。

首轮候选表高窗测试因独立夹具未加载到窗口，候选页IsVisible=false而取到历史页旧视口；已在真实加载的WPF窗口中断言候选页可见并重测，原始失败保留。中途测试窗口代码插入错误导致一次编译失败，已修正并重新编译；没有删断言、扩skip或替换SDK。TRX和hash见tests/。

Source validation、XAML26/26、WPF静态37 XAML/0 error/26既有warnings/179 info通过。阶段空间及精确自有中间TRX清理清单见disk-before/after和cleanup.json；当前截图与失败诊断保留，其他任务TRX/TestResults、真实存档和配置未动。

本批保持代码完成待验收：真实Playnite、物理DPI/键鼠、UIA客户端、长期宿主性能未验；提交后GitHub双SDK job及整轮最终中文根入口完整构建/测试/发布/打包/安装身份/启动和独立scripts/build.cmd仍待验。

此前969bfc20的[run 40640556766](https://github.com/Nikilua/GameSaveCenter/actions/runs/40640556766)基线SDK8.0.423与SDK10.0.401 job均成功，各1986总/1928通过/58既有skip/0失败；三个artifact身份见[CI清单](ci-run-40640556766.json)。required-checks此前403仍不推断。U06-03悬停CI失败及单独修正见[失败清单](../U06-03-04/ci-hover-failures.json)；修正提交3ecacdc0的双SDK run 40647037308仍运行，不能提前判绿。

## 2026-11-10 当前源码与归档身份复核

当前src/tests/scripts相对共同SHA e570f7b7没有差异；GitHub #1500基线job 129033708852和SDK10 job 129033708730全流程成功，覆盖当前全量测试、WPF静态审查及Render QA双主题394图/0 PROBLEM。原3840×2160和1040×700对照来自历史候选；当前活动页样张见U05-01-U06-07-U09-04复测。合成逻辑DIP不代表实体显示器或Playnite宿主。

U06-08 follow-up的R18 TRX当前可解析为1/1通过；其同目录files.json是原始采集字节清单，不等于Git checkout后的当前字节清单。24项中有3个临时编译日志未归档，21项仍在；其中19项当前大小/哈希与原清单不同。仓库text=auto/eol=lf会规范化文本；当前8字节LF的sdk-version-console.txt转成9字节CRLF后匹配原清单SHA，确认至少这类差异来自原始字节与规范化checkout的口径不同。保留旧清单和缺失日志的历史记录，不再声称它们当前存在或对当前工作树逐字节匹配。当前e570f7b7全量构建与#1500两SDK成功为共享源码的较新覆盖。

受控窗口与detached scaleprobe不能混为真实生产测量：R18受控窗口容器结果用于回归虚拟化；detached媒体峰值44不等价真实滚动容器累积。Media Inbox保留Standard行模式及关闭列虚拟化的既有安全例外。本批未改产品源码、未运行本机build/test/render；U06-07/08仍待真实Playnite、4K/DPI、实体输入及容器/帧率验收。

本批文档门禁：隐藏Source Validation exit 0；6份变更Markdown 1,013条本地链接无缺失/越界；git diff --check exit 0。精确移除本批日志2份/361 bytes，已核实仓库边界、无重解析点和无文档引用；C/D清理前后分别3,097,378,816 / 20,177,711,104 bytes，未观察到空间回收。
