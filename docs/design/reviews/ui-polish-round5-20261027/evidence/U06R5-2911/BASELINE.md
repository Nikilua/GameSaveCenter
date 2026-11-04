# 选中框右缘完整包裹复查：292f3bc6

## 用户反馈与修复

用户提供的三张图指出：最近任务的 `10-20 10:36` 时间、存档历史行右侧设备/备注文字、任务队列的状态与详情文字越过蓝色圆角选框右缘。新候选保留蓝色选择框、键盘焦点/悬停状态、行内容位置和现有滚动条体系；共享行内容在行视口内裁切，选框几何根据可用滚动视口对齐。`ViewportSelectionFrameMarginConverter` 在祖先滚动绑定不可用时回退到配置内边距；Overview 继续与列表内容对齐，避免边缘压到时间或滚动轨道。

源码分三次中文提交：`4ded0f52`（共享裁切/框线几何与 R06/R23 回归）、`3e98148e`（缺少滚动绑定时的内边距回退）、`292f3bc6`（Overview 与行内容对齐）。精确修订为 `292f3bc63a8ab08c57505d50ac5ba68b6c0d431a`，已推送 `main`。未改命令、Binding、数据契约、危险操作/错误/取消语义，也未更换游戏选框或滚动条。

## 精确候选验证

- Source Validation、`scripts/check-xaml.ps1` 和 WPF 静态审查退出码均为 0；扫描 26 个 XAML，0 错误、26 warnings、164 info。静态提示不等于宿主验收。
- 精确候选的 focused TRX：R06 selection state 3/3、Save History 1/1、TaskGrid 1/1，R10 recent access 3/3，R23 resource state 12/12 均通过，索引见 [focused-results.json](focused-results.json)。独立 `scripts/build.cmd` 退出码 0：160 份 TRX、2030 项、1962 通过、0 失败、68 skip。skip 原因保留，57 条为已撤销架构断言，1 条系统鼠标 opt-in，10 条 Named Pipe 受限环境用例，不计通过。完整 TRX 与映射、summary 在本目录。
- 精确候选完整 Render QA 退出码 0：双主题多尺寸共 394 PNG、0 PROBLEM；本批六张关键页图固定 1040×700、Light/Dark；离屏 logical DPI scale 1.00。关键截图：[Overview Light](screens/overview-light.png)、[Overview Dark](screens/overview-dark.png)、[Save Light](screens/save-light.png)、[Save Dark](screens/save-dark.png)、[Task Light](screens/task-light.png)、[Task Dark](screens/task-dark.png)。
- 独立 `scripts/build.cmd` 按其编译/测试职责验收，不代表安装启动链已通过。实际中文入口 `GameSaveCenter-一键构建安装运行.cmd` 转交 `GameSaveCenter-Run.cmd`；本候选整链尝试在构建开始前的 C 盘空间预检退出，exit 1：可用 `3,473,227,776` bytes，门槛 `4,294,967,296` bytes，短 `821,739,520` bytes。没有本候选的完整编译/发布/打包、安装身份核验或 Playnite 启动记录；不可用旧候选成功结果替代。
- 为该次 Render QA 与独立构建归档必要报告、六张关键图、完整 TRX 并校验 SHA 后，清理记录见 [cleanup.json](cleanup.json)。清理后当前 C 盘空间仍少于根入口门槛，故不重复启动必然失败的同一预检。
- GitHub 精确源码候选 run [#1454](https://github.com/Nikilua/GameSaveCenter/actions/runs/41742480073)仍在进行：SDK 10.0.401 job `126287170119` 已 completed/success（8/8）；基线 job `126287170658` 的编译/测试、离屏 Render QA、WPF 静态审查均已成功，当前生成扩展包，上传与诊断 artifact 尚待完成。步骤快照见 [CI 身份](ci-run-41742480073.json)。required-check/ruleset读取沿用之前403，不重复试探。

## 未验边界与本轮状态

源码实现完成待验收：Playnite 插件实际页面没有观察；外部 UIA、实体键盘/IME、物理跨屏 DPI 未验。没有执行真实存档恢复、媒体删除或云端写入。当前 80 项计数为 5 已满足、71 项代码完成待验收、U09-04/U09-05/U09-08 进行中、U09-06 远端待验。
