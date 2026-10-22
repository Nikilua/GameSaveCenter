# S00-08 真实登记命令行为验收

## 等价能力审计

既有目录测试已覆盖上下文快照、部分失效对象、固定/最近兼容和搜索；S00-06真实目录搜索→生产Shell回车路由证明恢复与清理两项只导航一次，S00-03键盘测试验证合成路由与IME防护。缺口是逐项验证当前全部13个真实登记项，而不是再次验证合成目录字符串。

## 本次补充

新增CommandDirectoryRegisteredRouteTests：13项真实factory注册→英文别名大小写搜索→同一个登记对象→TryExecute，恰好仅所选底层路由一次，核对导航参数与审阅Tab；另13项逐个在搜索后撤销底层CanExecute，同时验证TryExecute与ICommand.Execute均拒绝，且不改变Tab或发出Executed事件。注册表完整性检查保证未来新增路由必须加入矩阵；保留默认8项顺序和无结果搜索。其余9项覆盖捕获游戏移除、搜索/游戏选择器所在工作区变更、所选任务移除/关联游戏变化/忙碌。

全部使用合成游戏/任务和RecordingCommand，危险命令哨兵0次，已选版本与锁/PreRestore保持。底层命令可用性模拟不是操作系统权限验证；未操作真实存档或执行恢复/清理。

SDK8.0.423 Release编译0警告0错误，新增36/36、0 skip/fail。[候选身份](candidate-identity.json)、[TRX](registered-candidate.trx)、[编译](candidate-build-console.txt)。产品源码保持70e42163；候选基线d3dfad89加新测试哈希。提交后由最终默认.cmd复核精确提交，不能以候选结果或旧4f40e016标准链冒充本次新提交全链。

真实Playnite页面、物理键盘/IME/物理DPI未验；受控结果不替代宿主验收。原有S00-03/04/05/06/07证据继续保留，无删断言或新增跳过。用户旧TRX/TestResults未动。

## 默认全链首次结果

首次精确提交f08b85be在测试发现阶段路径过长退出1；同DLL禁用额外AppDomain发现1390用例。已将发现设置与执行阶段对齐，新提交完整链待跑。[失败与对照](discovery-failure/README.md)。
