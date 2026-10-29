## 2026-10-29 U09-01补U01四项历史对照

重建0447237a→7e61bba3→813adb09三个源码快照，分别补U01-01/02标题/混排及U01-05/06禁用文字/图标批次。Fake时间同为2026-10-28T09:00:00Z，生产wall-clock未改；src逐文件与原始ZIP核对未改。三侧SDK8.0.423 Release Worker/Harness编译均0警告0错误，各394 PNG；标题和禁用文字两侧完整Render QA exit0。最早Before因Sidebar rapid-toggle未收敛exit1，失败报告保留，未重跑追绿；它只用于静态前图，不记整体验证通过。

六次原有finesseprobe双主题exit0；Shell标题和控件禁用态样图已视觉检查。42个页面/控件主题引用以内容哈希去重保存，包含存档、任务、媒体、工具、通知及Shell；两组变更边界分开记录。仓库Git报告会读取父仓库，历史身份以ZIP注释/哈希及源码核对证明。本批不改当前生产源码，不新增真实恢复/媒体删除/云写入；实际Playnite、物理DPI、缺失可选字体、实体输入/IME/UIA仍待验。U09-01其它视觉批次对照仍缺，保持进行中。

证据：[两组配对与去重图](pairs.json)、[Before完整失败报告](before-render-report.txt)、[标题后报告](titles-render-report.txt)、[禁用文字后报告](disabled-render-report.txt)、[生产提交盘点（文件类型启发式，不等于已验视觉批次）](visual-commit-inventory.json)。
