# S00-04 固定与最近命令

## 交付

实现提交 `f8b3144e36d190663f760a98aa0602bec6bda2f8` 为命令目录加入有界固定与最近使用偏好。固定和最近命令使用独立、稳定的 ID 保存，不复用“最近访问游戏”；最多固定 5 项、保留 20 项最近命令。已移除或未知 ID 在加载/保存时清理，命令只在成功执行后进入最近列表。固定/取消固定、顺序、设置重置、冲突合并和搜索结果点击后的选择状态均有回归覆盖。

实现和定向回归见 [IMPLEMENTATION.md](IMPLEMENTATION.md)。完整标准构建、测试、打包、安装与启动结果见 [STANDARD_BUILD_20261022.md](STANDARD_BUILD_20261022.md)，机器可读身份见 [verification-identity.json](verification-identity.json)。

## 当前验收状态

当前提交的 SDK 8.0.423 `GameSaveCenter-Run.cmd` 完整退出码为 `0`；Release solution 编译、全量 Core/Worker/Playnite 测试、Worker 发布、包身份校验、ZIP/PEXT 打包、扩展安装校验和 Playnite 启动均成功。R09 焦点轮廓用例 `2/2`。

运行器启动 Playnite 并确认安装版本/DLL 身份，不包含真实页面操作、视觉验收、物理键盘或 IME 候选验收。因此 S00-04 为“代码完成待验收”，host verification 保持 pending。Round4 尚未整体完成。
