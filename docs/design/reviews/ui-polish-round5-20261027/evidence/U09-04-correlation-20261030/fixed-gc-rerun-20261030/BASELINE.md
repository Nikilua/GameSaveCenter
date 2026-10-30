# U09-04 固定时钟 GC/时间戳补充复测

## 范围与身份

首次探针构建未带仓库构建脚本已有的 MSBuildEnableWorkloadResolver=false 参数；按同版本 SDK 8.0.423 和该既有参数重试后，RenderHarness 项目还原及 Release 编译成功，exit 0、0 warning、0 error。该操作只编译隔离副本中的 GameSaveCenter.RenderHarness 及其项目引用，不是发布安装全链。

受测 src/ 与 tests/ 精确等同冻结源码候选 5c0c57b06aed27dc46c41ef3d655ec20dafa5ec7；测量时 HEAD 为 4e214368a310ed0ef892b5e3eebb5ed46fc7adf9，本批没有修改生产或测试源码。原始 Program、仪表化 Program、探针源码与补丁 SHA、SDK路径、完整命令、构建日志、二进制身份及每轮退出码均见 [build-run.json](build-run.json)。三轮固定 GSC_RENDER_SAMPLE_UTC=2026-10-28T09:00:00Z，各70周期、297动作、30秒静置采样、exit 0；每轮stdout、报告、CSV原件及哈希见 [sha256.json](sha256.json)。

## 分段结果

每轮70个“theme”区间记录 ApplyThemePalette(shell) 与 ApplyThemePalette(page)；70个“layout”区间记录页面内容赋值、响应式布局和 window.UpdateLayout()。完整原始数据及可复算脚本见本目录。

| 区间（210个样本） | p50 | p95 | 最大值 | >300 ms | 观测 |
| --- | ---: | ---: | ---: | ---: | --- |
| 主题应用 | 232.36 ms | 325.97 ms | 361.38 ms | 21 | 210/210区间有Gen 0计数增量；分配与耗时Pearson r=0.974 |
| 内容布局 | 46.12 ms | 97.49 ms | 225.14 ms | 0 | 123/210有任意代GC计数增量；9/210超过100 ms |
| 主题应用+内容布局时间戳包络 | 281.75 ms | 398.32 ms | 434.67 ms | 67 | 用相邻两段绝对时间戳差重建；不与旧“layout”列直接等同 |

主题应用超过300 ms的21个样本均同时包含Gen 0计数增量，并与超过300 ms的Rendering回调间隔时间重叠。这个关联只定位到受控探针中两个显式 ApplyThemePalette 调用所在的时间段；它不证明GC是原因，也不代表生产环境每次导航都会重新应用主题。

三个活动期Rendering回调区间合计2831个，p95为286.91 ms，超过100 ms的有416个，超过300 ms的有124个。旧三轮内容布局曾有17/210次超过300 ms（p95分别472.10、649.76、620.42 ms）；本次同样固定样本时钟、同源码候选的新三轮内容布局没有复现这些>300 ms样本，Rendering回调长间隔仍存在。旧、新测量的仪表化细节及调度环境不完全相同，不能把未复现当作回归修复，也不能把回调间隔当作物理帧。

Pearson相关仅作描述统计；GC.CollectionCount是操作窗口增量而非事件时间。WPF窗口为受控STA窗口，透明度0.01；FakeDashboardData与程序化文本更新不能验证Playnite真实宿主、实体输入、IME、物理DPI或VSync。U00原始逐帧及累计分配基线仍缺失，故 U09-04 保持“进行中”，等待可核的基线与宿主证据；本次没有延长动画或改生产资源。