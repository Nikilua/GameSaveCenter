## 2026-10-29 U03-04禁用原生文字修正候选

历史同状态对照发现共享TextBox只改变PART_ContentHost附加Foreground，原生编辑文字仍读取控件Foreground；新增实际属性断言在修改前准确失败（预期#FF9AA4B5、实际#FFF2F4F8）。共享模板改为设置TextBox本身Foreground，沿原TemplateBinding传给内容宿主；补重新启用恢复主文字色断言，错误描边/HelpText和尺寸语义保留。

SDK8.0.423 Release定向8/8、0 skip；首次扩展运行因未传构建身份导致6个源码测试被门禁拦截，补真实HEAD元数据后通过，失败TRX保留。双主题同状态图片已检查；源码校验、XAML26/26、WPF静态42文件/0 errors（既有26 warning/179 info）及完整Render QA394 PNG/0 PROBLEM/exit0通过。当前为源码候选；提交冻结后还须本候选中文根入口完整链、独立build.cmd及双SDK CI，不能沿用cd062531通过结果。真实Playnite、实体输入/IME/UIA及物理DPI继续待验。

证据：[验证、失败分类与前后图](validation.json)、[空间预检清理](preflight-cleanup.json)。
