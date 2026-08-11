# 设计原则

## Apple-inspired，不是 macOS 仿制

目标是借鉴可迁移的品质：清晰层级、足够负空间、克制材质、明确反馈、适应窗口变化；同时保留 Windows 桌面习惯：Segoe UI、键盘焦点、滚轮、列宽调整、UI Automation、高对比度和 DPI。

## 视觉优先级

1. 当前任务和核心数据
2. 主操作
3. 当前上下文和状态
4. 次级操作
5. 技术详情

边框、阴影、渐变不能抢过内容。

## 分组手段优先级

1. 空间和对齐
2. 背景层级
3. 标题和字体
4. 轻分隔线
5. 阴影
6. 颜色

不要用卡片套卡片制造层级。

## 三层表面

- Window/Host Background
- Primary Surface：主工作区
- Secondary/Floating Surface：表单、Popup、Inspector、Dialog、Toast

普通数据行不使用独立阴影或毛玻璃。

## 动作层级

局部区域通常只有一个 Primary 按钮。Secondary、Ghost、Destructive 必须视觉区分。不要让所有按钮都使用高饱和强调色。

## 状态表达

状态至少由文本加图标/圆点构成，不只靠颜色。

## 简化的正确方式

- 拆分模块
- 折叠次要详情
- 使用 Inspector/Drawer/Details pane
- 只显示当前上下文相关操作

错误方式是缩小字体、强制裁剪、删除标签、把所有操作塞在一行。
