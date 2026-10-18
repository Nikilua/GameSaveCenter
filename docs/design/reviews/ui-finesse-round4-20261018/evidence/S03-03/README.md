# S03-03 冲突原因对照

## 实现

设备冲突详情现在把检测原因和两侧备份证据放在同一只读对照卡中：本地/远端备份 ID、设备、创建时间、大小、文件数、共同父版本、内容指纹是否可比较/相同，以及各自最近一次 readiness 校验状态和检查时间。校验时间同时显示相对年龄与绝对时间；未知字段明确显示未知。侧边栏不暴露清单路径或指纹原文。

各已知检测原因都提供中文说明；`UnknownDivergence` 根据指纹证据区分内容不同与无法比较。证据卡明确说明结果只反映已记录的检查时间，不按“最新”时间推断哪一侧应覆盖另一侧。保留原来的冲突处理命令与隔离远端恢复安全链。

## 受控验证

- 私有仓库 SDK `8.0.423` Release 全解决方案构建：`0 warning / 0 error`。独立输出目录、完整构建日志见 [solution-build.log](solution-build.log)。
- 设备冲突展示与绑定：Playnite `4/4`；readiness 摘要投影：Worker `2/2`；既有 Core 冲突检测器：`7/7`。原始 TRX 和当次程序集、PDB、源码 SHA-256 见 [identity.json](identity.json) 与 [tests](tests/)。
- 与用户旧日志同名的 R09 焦点描边测试，在同次 SDK8 输出中独立 `2/2` 通过。
- RenderHarness 完整渲染矩阵：`render-qa OK`，`0 PROBLEM`，本次输出 334 张 PNG。代表性 1366×768 设备状态页截图见 [screenshots/Maintenance-1366x768-tab2.png](screenshots/Maintenance-1366x768-tab2.png)。
- XAML 结构检查：25 个文件通过；`validate-source.py`、Round4 账本校验和 `git diff --check` 通过。

## 边界

本次只验证了受控构建、定向测试和合成渲染；未运行完整 WPF 逐类隔离循环、安装包或真实 Playnite 宿主，也没有执行云同步或真实存档恢复。旧版本 sidecar 未带 readiness 字段时明确显示未知；界面呈现实际检查时间，但不定义过期阈值，也不自动选择冲突胜方。

用户先前 R09 日志对应的诊断目录及同次 `summary.json`、TRX、DLL/PDB 身份目前不可用，因此那次 `Thickness(2)` 对 `Thickness(1)` 失败的实际运行输入仍无法定位。当前构建的 R09 独立复验结果若通过，也只证明本证据包身份中的当前测试输入。
