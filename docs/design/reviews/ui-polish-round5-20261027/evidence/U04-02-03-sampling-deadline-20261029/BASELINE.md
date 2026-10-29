## 2026-10-29 U04-02/03采样截止修正候选

acebe07d中文根链SDK8.0.423退出0，157份TRX为1955 pass/58既有skip，六个安装DLL与PEXT逐字节相同；新宿主进程68964及14:07:12日志确认加载GameSaveCenter0.6.73，但未验真实页面或运行时模块SHA。独立build.cmd在第75类R08 hover重新进入未采到中间帧失败（局部1463项：1443 pass/1 fail/19 skip），完整交付门禁未通过；原始诊断归档，未追绿重跑旧链。

新增有限Render队列复现：旧1ms采样截止等到40次Render全部结束才返回，准确失败。仅将测试采样截止计时器改为Send优先级，保留生产时长、所有原断言、超时和skip。三轮直接运行12/12断言通过，但额外Test AppDomain有既有COM卸载异常，日志保留、不记为无异常门禁；按仓库正式DisableAppDomain=true复核4/4、0 skip且无该异常。此复现证明采样器可被饿死，旧hover失败未记录迟到耗时，不宣称回溯了唯一根因。

生产资源未改。新候选提交后须重新完整构建安装链、独立build.cmd和双SDK CI；D盘空间不足时以同SHA的C盘隔离工作树执行，不降低空间预算或替换SDK，不重试旧权限拒绝。当前5项已满足、71项代码完成待验收、3项进行中、1项远端待验；U09-05重新进行中，U09-01其它配对及U09-04重建补测继续。required-check名单仍按既有403远端待验，不复探。

证据：[本批验证](validation.json)、[旧候选根链与失败build.cmd](../U03-disabled-native-text-20261029/final-gates/local.json)。
