# e6e7f3b9 主题资源优化的完整门禁

冻结提交e6e7f3b931ec767aea353d0998bc12800df16a21、SDK8.0.423，中文根入口转英文根入口的构建/测试/发布/打包/安装/启动链退出0；独立scripts/build.cmd也退出0。跟踪文件在两次运行期间保持不变。各158份TRX，各2019项：1961 pass、0 fail、58既有skip（57条撤销架构断言、1条显式启用的物理指针探针），skip不计通过。原始路径、日志和TRX的原始/脱敏SHA在files.json；独立诊断实际位于sta-final旧前缀下的新run3a674686，不能与此前2d失败根链混淆。

PEXT 45737323 bytes，SHA256 ebf2ca9b1a745a1de0d2673a90eb6f2172d803ea90361b4b08608b9b722e0f07；六个安装DLL与PEXT对应条目逐字节一致，磁盘InformationalVersion均0.6.73+e6e7f3b9…；不推断运行时模块SHA。Playnite PID26600于2026-10-29 18:08:43启动，18:08:48.906宿主日志记录加载GameSaveCenter0.6.73，未冒充真实页面、外部UIA、实体键鼠/IME或物理DPI验证。

同协议两轮70周期/297操作的重建补测，活动期托管分配均约8.88GB，相比优化前12.52GB下降29.1%，相比历史前快照11.12GB下降20.2%；Rendering回调间隔p95为286.317/305.010ms。原始U00逐帧/累计分配基线缺失，以上为补建对比，不是物理呈现帧或实体输入延迟，也不保证每个单项操作均无回退。

[GitHub run40851428895](https://github.com/Nikilua/GameSaveCenter/actions/runs/40851428895)最终success；SDK8 baseline job123371589585和SDK10.0.401 job123371590197均success。三个artifact的head_sha均为该提交，ID/digest和全部step保留于github-ci.json。required-check/branch-protection接口此前403，未重复探测，仍需有权限维护者执行gh api repos/Nikilua/GameSaveCenter/branches/main/protection/required_status_checks核对要求。

本记录只证明e6e7f3b9。随后U09-01历史配对发现页签指示条与标题重叠，正在修正共享模板；该新候选不得引用本记录冒充自己的冻结全链。旧失败历史保留，真实恢复、媒体删除、云写入未用于视觉测试。
