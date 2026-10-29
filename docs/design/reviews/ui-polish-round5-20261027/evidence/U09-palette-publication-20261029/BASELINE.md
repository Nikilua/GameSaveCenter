# U09-04/05 主题资源发布优化候选

历史前/后快照新增成本拆分均70轮、297操作、退出0。主题重应用分配前10.572GB、后11.754GB；布局前0.264GB、后0.297GB。不是实体呈现帧或真实宿主导航成本。parts目录保存仪器增量patch、原始CSV、构建和版本身份。历史报告仍会读父仓库HEAD；真实快照身份沿用前批ZIP/源码核对及identity.json，不使用报告Commit字段替代。

生产管线先调用兼容层、材质、Demo与语义覆盖，原来每层直接写入可见ResourceDictionary。新增原生DynamicResource观察者的四种主题/高对比回归，旧代码同一Info键触发2～3次通知，4/4准确失败。现将计算先归并到无宿主字典，再向原有字典按每个最终键发布一次；调用方资源和合并字典保留，不更换主题值、页面布局、业务命令或滚动/选框。

首次候选漏掉空值：WPF新字典indexer对缺失键赋null不插入键，导致高对比阴影未清除，原R09断言准确失败。已显式Add七个可空效果键，新增回归亦覆盖已有阴影的清除；保留真实null，不使用透明效果冒充无效果。可参考[WPF ResourceDictionary源码](https://github.com/dotnet/wpf/blob/main/src/Microsoft.DotNet.Wpf/src/PresentationFramework/System/Windows/ResourceDictionary.cs)，运行时行为由本机回归证明，不将main分支源码当作本机Framework二进制身份。

SDK8.0.423最终编译0警告0错误；五类行为150 pass/0 fail，39既有撤销架构skip不计通过。首次扩展测试漏GSC_BUILD_COMMIT，被身份门禁拦下120项；修正运行元数据后重新编译复核，未改门禁，原失败亦归档。源码校验通过；WPF静态0 error/78既有warning/507 info。首轮render因缺Worker的正常设置夹具状态退出1，补建真实Worker后render-qa退出0、394 PNG、0 PROBLEM。浅深色概览样图已查看；与失败轮同页图片逐字节相同。本批没有恢复旧架构，截图中的现存概览标题不代表重建已撤销的结构。

上列检查在基于16e30205的候选脏工作树运行，具体候选源文件哈希见validation.json；不冒充冻结候选全链。下一步提交候选，冻结后做同协议性能复测、中文根链、独立build.cmd与精确提交双SDK CI。当前不能宣称分配回退解决或最终门禁通过；Q06间歇失败仍未证明修复。真实Playnite页面、UIA/实体IME/物理DPI及required-check权限边界保留，真实恢复/媒体删除/云写入未执行。

本阶段清理：核对59份归档哈希、目录边界/无重解析点、所有文件独占打开且RenderHarness已退出后，只删除被render-final替代的旧render目录和重复设置图暂存目录。保留失败报告/两张样例、当前394图、TRX/构建与未知文件；删除清单、字节数及前后空间见cleanup.json。
