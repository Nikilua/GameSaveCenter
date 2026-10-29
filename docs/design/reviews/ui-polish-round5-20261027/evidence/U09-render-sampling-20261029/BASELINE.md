# U09-05 R08 hover渲染采样修正候选

392854b3中文根链已通过对话框类，随后在R08MotionReverseBehaviorTests首次hover中间态断言失败，退出1。原输出midpoint=0、exit=0/0，reenter=0.238/0；按压反转和两主题48次兄弟扫过成功。局部1445 pass/1 fail/19 skip，独立build.cmd未启动。完整原始诊断保留，失败类及root冻结/SDK/源码身份已归档。

仅调整测试采样：进入/退出/重入前先注册CompositionTarget.Rendering观察器，在同一渲染回调内判定原条件并退出dispatcher frame；保留同一readonly IsMouseOver状态驱动和原始全部断言，500/250ms超时不变，现有fixture400ms采样时长不变，未延长生产动画。观察器与截止timer均在finally移除，窗口补明确UIA名称，正常peer保留。终态、按压、48次扫过仍执行原检查。

SDK8.0.423定向类4/4、0 skip。实际进入/退出/重入采到2/2/3个渲染帧，耗时28.374/14.558/30.307ms，本轮合成状态重置均0；不将本轮数据当作旧失败唯一根因。原来的轮询断言在两次泵送之间可能漏观测，当前改为对实际计算帧取样，不用人工Seek/伪造中间值。生产源码、时长、SDK、workflow及skip都未改。新测试候选必须提交冻结后重新跑中文根链、独立build.cmd与双SDK CI，定向通过不算整链通过。

本阶段未产生新包或截图，保留失败构建和日志/TRX以便继续定位，未清理未知产物；真实宿主页面/实体输入DPI与required-check403边界不变，真实恢复/媒体删除/云写入未执行。
