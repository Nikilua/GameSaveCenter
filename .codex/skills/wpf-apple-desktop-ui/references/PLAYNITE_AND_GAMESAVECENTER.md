# Playnite 与 GameSaveCenter 专项

## 双层导航

Playnite 已有宿主导航。插件内部：Wide 显示完整侧栏；Medium/Compact 只显示图标和 Tooltip；不要重复宿主入口。

## 窗口控制区

刷新、全部备份、同步媒体等必须预留 `PageTopSafeRight`，不能覆盖最小化、最大化和关闭。

## 宿主主题覆盖

关键控件使用项目命名的显式 Style Key，并检查嵌套 Popup/ScrollViewer 是否真正应用；Light/Dark/Follow Playnite 都测试；不能依赖默认 ComboBox、DataGrid、ScrollBar。

## 模块边界

一级模块：首页、存档、修改器、媒体、任务、维护。

局部 Tab：

- 存档：历史、候选路径、恢复规则
- 修改器：已安装、FLiNG 在线、Cheat Table
- 媒体：待归类、当前游戏媒体、来源与规则
- 任务：全部、运行中、失败、已完成
- 维护：诊断、设备状态、异常与日志

`设备状态` 只能在维护中心可见。

## 大型库

数百/上千游戏：列表虚拟化；启动读缓存；不在 UI 线程全量扫描；ScrollBar Thumb 保持可拖动最小长度；搜索筛选不重建复杂视觉树；缩略图和状态按需加载。

## 反馈

Toast：退出后备份、游玩中备份、无变化、媒体同步、下载完成。

Dialog：恢复、覆盖、删除/解绑、忽略并保留副本、取消任务。

## 仓库规范

若存在，必须读取：

- `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md`
- `docs/design/UI_CHANGE_GATE.md`
