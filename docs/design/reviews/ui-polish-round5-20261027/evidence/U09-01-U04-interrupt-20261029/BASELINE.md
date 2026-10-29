# U09-01补齐U04-06/08历史对照

历史生产源码4225eb17139a844ede87219f02a9ad97e88a12de → c5c86558b6232995438657c00810aeb327ed9ba5。两份Git ZIP注释、SHA与356/357个生产src文件逐字节核对未改；仅RenderHarness加入同一探针，Fake UTC固定。SDK8.0.423 Release单节点编译两侧退出0，NU1900漏洞信息源不可达警告保留，未关闭审计；本次快照未注入历史Assembly提交版本，身份按ZIP、源码字节和已记录DLL SHA核对，不冒充历史安装包。

采用真实MaintenanceView.Resources的GscDisclosureCard，双主题同一只读中文内容。实际Rendering中观察箭头中间帧后禁用：旧版两个主题仍IsAnimated=true、角度在-90与0之间，250ms后才走到0且旧时钟仍在；新版立即归0、IsAnimated=false，重新启用未重放旧时钟。原始CSV保留实际经过时间，图片只证明所拍状态，不声称实体输入或物理呈现帧。两侧各10张最新PNG，禁用中间帧和终态可对照。

首版将主题资源重应用也计入100次复用，发现其替换Transform；此现象前后都有，不归因于100次开合。v2明确记录reduced-theme-applied，并在主题更新完成后取对象基准，再连续100次切换；Header、Path、RotateTransform均保持同一实例，两主题前后都成立，最终Reduced Motion无动画时钟。v3仅增加禁用即时PNG，不改状态驱动或观察谓词；三轮原始CSV和构建日志均保留。这项是等价能力证据，不虚构对象复用的新改善。

历史行为13/13等验收继续引用../U04-06-08/BASELINE.md；本批不改生产资源，不等于当前源码完整Render QA、安装链或真实Playnite验收。真实取消命令、窗口失活、实体输入/DPI及长期内存增长待验，U04-06/08保留代码完成待验收，U09-01继续进行中。

已核对归属、文档引用、规范化路径、重解析点边界和独占打开；删除本阶段32张旧PNG，保留最新20图、三轮CSV、全部日志/TRX、快照源码和未知文件。前后空间及逐项SHA/字节见cleanup.json。
