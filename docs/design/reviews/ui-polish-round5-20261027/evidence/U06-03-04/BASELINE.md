# U06-03/04 行内动作与工具栏

2026-10-28，基线e7528faa。本批只加强行为测试，生产UI、命令、绑定和密度资源无变化。

U06-03：维护页进程映射的行内动作原本常显“删除”，UIA名称包含EXE，HelpText说明删除映射；不依赖模糊图标。新增实际WPF按钮键盘焦点、受控悬停前后列宽不变、移开悬停仍可见的检查。双主题下UIA Invoke触发合成命令，确认目标是点击行而不是另一选中行；无参数不能执行。没有删除真实映射。

U06-04：复用生产工具栏换行/筛选摘要与多选布局，按窄宽往返检查媒体收件箱、维护、存档/概览动作组、任务预设及任务摘要；另验证多选摘要和存档选择在断点往返中保留。合成夹具覆盖既有行为，不新建溢出菜单或隐藏筛选条件。

SDK8.0.423 Release：行内动作与工具栏7/7、多选/筛选和断点9/9，均0失败/0跳过。脱敏TRX和hash在tests/。悬停由MouseDevice受控状态驱动，焦点由STA窗口实际控件获取，不代表物理鼠标、系统键盘/IME或真实Playnite。

Source validation通过、XAML26/26、WPF静态37 XAML/0 errors/26既有warnings/179 info。生产视觉未改，沿用相同生产源码的[U05-07/08双主题394图Render QA](../U05-07-08/BASELINE.md)，本批未生成新截图或宣称新提交完整安装链。

此前弹层源码提交bca60704的Windows Actions [run 40639378202](https://github.com/Nikilua/GameSaveCenter/actions/runs/40639378202)两个适用job现已成功：SDK8.0.423基线122653683590、SDK10.0.401复核122653683330；原始日志中分别确认基线SDK及SDK10 Release build/tests通过，基线render-qa OK。扩展包与两个诊断artifact身份及SHA见[CI清单](ci-run-40639378202.json)。required-checks接口既有403，不能由这两个job推断分支保护完整性。此结论不替代本批新提交CI。

两项保持代码完成待验收；真实Playnite、物理输入/UIA/实体DPI及新提交CI待验。最终中文根入口完整构建/测试/发布/打包/安装身份/启动与独立scripts/build.cmd仍需最终候选执行。disk-before/after记录空间，本阶段仅保留当前TRX及脱敏证据，无包或截图副本；没有已确认无用目录，删除清单为空，其他任务TRX/TestResults未动。

2026-10-28 CI新失败：run40644355886 SDK10及run40645391789双SDK在ProcessMappingInlineActionBehaviorTests第87行受控IsMouseOver断言失败；原始job日志与artifact身份见ci-hover-failures.json。生产命令未执行失败；测试在注入状态后泵送后台消息，可能先收到真实桌面指针更新。夹具改为同步UpdateLayout后立即检查进入/退出悬停，保留可见性、焦点、列宽与目标断言。SDK8.0.423本地1/1、0 skip，TRX见tests/ci-hover-synchronous.trx；当时工作树另含U06-07/08候选，SDK10和新提交CI仍待验。假设尚待远端验证，不声称CI已修复。

## 2026-11-02 修正候选远端验证

修正后的夹具提交`3ecacdc0dc5376e45a6460b2af2668a1df57f328`在GitHub run [40647037308](https://github.com/Nikilua/GameSaveCenter/actions/runs/40647037308)（#1349）上已completed/success。基线SDK job `122678147577`与SDK10.0.401 job `122678147262`的所有适用步骤均完成并成功，三份artifact的head SHA、ID、尺寸和digest见[`ci-run-40647037308.json`](ci-run-40647037308.json)。这验证了同步布局夹具修正，旧失败run仍保留历史；不把离屏/STA合成鼠标状态说成物理鼠标或Playnite宿主验收。当前`ProcessMappingInlineActionBehaviorTests.cs`此后未再修改。

## 2026-11-09 当前共同源码复核

当前main的src/tests/scripts相对完整验证共同SHA e570f7b7无差异。U06-03的ci-hover-synchronous、u06-actions-final和u06-summary-breakpoints三份TRX哈希全部匹配清单，计数分别为1/1、7/7、9/9，均0失败、0 skip。测试确认危险操作有明确文字，键盘可达；受控悬停不改变列宽，UIA Invoke与行对象绑定，不会误取另一选择行，无参数时不执行命令。这里没有调用真实删除命令。

U06-04任务页选定类合计101通过、0失败、11条既有skip；媒体页选择类合计218通过、0失败、39条既有skip。skip均针对已撤销“今日工作台”架构，不计通过。媒体前后图以及任务工具栏前后图是2026-10-30相应小批的局部对照，不能代表当前整页构图或作为恢复旧架构的依据。当前共同源码e570f7b7的GitHub #1500基线SDK和SDK10 job全部步骤成功，含完整Release测试、394图Render QA/0 PROBLEM及WPF静态审查；这仅覆盖代码与合成离屏检查。真实Playnite宿主、外部UIA、实体键盘/IME及物理DPI仍待验。

没有更改产品代码，也未重跑本地编译/行为测试；本批做证据去重与当前SHA覆盖核对。U06-03/04继续代码完成待验收。整轮最终中文入口和独立scripts/build.cmd仍待冻结候选；当前C盘3,091,267,584 bytes、距4GiB门槛差1,203,699,712，D盘20,181,245,952 bytes，准确SDK8.0.423尚未就绪，不以其他SDK或旧构建替代。

文档检查：隐藏Source Validation exit 0；6份变更Markdown 1,012条本地链接全部存在、缺失/越界0；git diff --check exit 0。只删除本批校验日志2份、共361 bytes；仓库边界、无重解析点、无文档引用通过，C/D清理前后均3,092,672,512 / 20,181,188,608 bytes。
