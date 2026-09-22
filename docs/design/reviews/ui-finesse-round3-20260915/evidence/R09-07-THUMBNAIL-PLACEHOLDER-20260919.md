# R09-07 缩略图占位一致证据

日期：2026-09-19  
实现提交：`72a1a07b` 统一媒体缩略图占位状态

## 结论

R09-07 已满足。现有 `AsyncThumbnailImage` 的后台解码、取消、过期请求保护、冻结 `BitmapSource`、缓存和并发上限继续复用；本阶段只补充状态语义与共享的 `MediaThumbnailPreview` 固定槽位，不改 DTO、命令绑定、媒体归类、游戏选框、滚动条或列表虚拟化策略。

媒体卡片继续使用 `164 x 154` 的固定项和 `96 DIP` 预览行，操作区留在第二行 `58 DIP`。截图的加载、无图、文件缺失、损坏/无法读取和成功状态分别显示文字；录像显示“录像预览”，未知媒体类型显示“未知媒体类型”。成功缩略图迟到时只替换同一 `96 x 96` 槽位，不改变卡片行高，也不进入操作区。详情预览的状态文字同样在截图类型下生效，录像继续交给已有 `MediaElement`，不会被缩略图占位文字覆盖。

## 可复核验证

- `R09ThumbnailPlaceholderBehaviorTests`：`1/1`。真实 STA WPF `Window` 中实例化生产控件，依次验证无图、录像、缺失文件、损坏文件、有效 PNG；检查状态文字、占位可见性、`96 x 96` 实际尺寸和操作行不被挤占。
- `AsyncThumbnailImageTests`：`2/2`；`AsyncThumbnailLoaderTests`：`6/6`；`MediaThumbnailConverterTests`：`1/1`。
- R09 定向回归：`12/12`。
- `scripts/build.ps1 -SkipTests`：XAML `24/24`，Release solution `0 warning / 0 error`，包含 Playnite `net462` 与测试程序集。
- `python scripts/validate-source.py`、`scripts/check-xaml.ps1`、`git diff --check`：通过。

测试使用隔离临时目录和合成 PNG/损坏文件，不读取或修改真实存档、真实媒体、用户云端或外发诊断。

## 边界与未验项

Demo 原目录不可用，本阶段沿用恢复生产资源基线；STA/offscreen logical DIP 行为证据不等于真实 Playnite presented frame、真实物理 DPI/跨屏、UIA/读屏、IME、ETW、宿主性能或 package-host 安装通过。没有把离屏结果写成真实呈现验证。

用户提供的 `DEV-INSTALL-008` 合并后安装日志仍单列：源码编译 `0 warning / 0 error`、Core `83/83`、Worker `311/311`，但 dirty main 的 Playnite.Tests 全量为 `73 failed / 588 passed / 57 skipped`，混合 DataGrid 行生成、WPF Dispatcher/视觉资源污染、输入源为空和测试身份/时序问题。本阶段没有覆盖 main 用户改动、没有在 dirty main 上重跑安装器，也没有把当前分支的隔离通过改写成安装通过。

下一可执行任务：R09-08 主题背景压力，先核对不同明暗宿主背景下透明控件的实际合成和最差背景文字门槛。
