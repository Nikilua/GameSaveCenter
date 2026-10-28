# U06-03/04 行内动作与工具栏

2026-10-28，基线e7528faa。本批只加强行为测试，生产UI、命令、绑定和密度资源无变化。

U06-03：维护页进程映射的行内动作原本常显“删除”，UIA名称包含EXE，HelpText说明删除映射；不依赖模糊图标。新增实际WPF按钮键盘焦点、受控悬停前后列宽不变、移开悬停仍可见的检查。双主题下UIA Invoke触发合成命令，确认目标是点击行而不是另一选中行；无参数不能执行。没有删除真实映射。

U06-04：复用生产工具栏换行/筛选摘要与多选布局，按窄宽往返检查媒体收件箱、维护、存档/概览动作组、任务预设及任务摘要；另验证多选摘要和存档选择在断点往返中保留。合成夹具覆盖既有行为，不新建溢出菜单或隐藏筛选条件。

SDK8.0.423 Release：行内动作与工具栏7/7、多选/筛选和断点9/9，均0失败/0跳过。脱敏TRX和hash在tests/。悬停由MouseDevice受控状态驱动，焦点由STA窗口实际控件获取，不代表物理鼠标、系统键盘/IME或真实Playnite。

Source validation通过、XAML26/26、WPF静态37 XAML/0 errors/26既有warnings/179 info。生产视觉未改，沿用相同生产源码的[U05-07/08双主题394图Render QA](../U05-07-08/BASELINE.md)，本批未生成新截图或宣称新提交完整安装链。

此前弹层源码提交bca60704的Windows Actions [run 40639378202](https://github.com/Nikilua/GameSaveCenter/actions/runs/40639378202)两个适用job现已成功：SDK8.0.423基线122653683590、SDK10.0.401复核122653683330；原始日志中分别确认基线SDK及SDK10 Release build/tests通过，基线render-qa OK。扩展包与两个诊断artifact身份及SHA见[CI清单](ci-run-40639378202.json)。required-checks接口既有403，不能由这两个job推断分支保护完整性。此结论不替代本批新提交CI。

两项保持代码完成待验收；真实Playnite、物理输入/UIA/实体DPI及新提交CI待验。最终中文根入口完整构建/测试/发布/打包/安装身份/启动与独立scripts/build.cmd仍需最终候选执行。disk-before/after记录空间，本阶段仅保留当前TRX及脱敏证据，无包或截图副本；没有已确认无用目录，删除清单为空，其他任务TRX/TestResults未动。

2026-10-28 CI新失败：run40644355886 SDK10及run40645391789双SDK在ProcessMappingInlineActionBehaviorTests第87行受控IsMouseOver断言失败；原始job日志与artifact身份见ci-hover-failures.json。生产命令未执行失败；测试在注入状态后泵送后台消息，可能先收到真实桌面指针更新。夹具改为同步UpdateLayout后立即检查进入/退出悬停，保留可见性、焦点、列宽与目标断言。SDK8.0.423本地1/1、0 skip，TRX见tests/ci-hover-synchronous.trx；当时工作树另含U06-07/08候选，SDK10和新提交CI仍待验。假设尚待远端验证，不声称CI已修复。
