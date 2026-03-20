# Checklist

## SuspendingWindow 浮窗窗口
- [x] SuspendingWindow.axaml 文件存在于 `ImmersingPicker/Views/` 目录
- [x] SuspendingWindow 窗口具有圆角半透明竖形矩形外观
- [x] SuspendingWindow 窗口无标题栏
- [x] SuspendingWindow 窗口始终置顶显示
- [x] SuspendingWindow 包含打开主窗口的按钮

## MovingControlButton 拖动控件
- [x] MovingControlButton.axaml 文件存在于 `ImmersingPicker/Views/suspendingWindow/` 目录
- [x] 拖动 MovingControlButton 可以移动浮窗窗口
- [x] 窗口不能移动到距离屏幕边界 5px 以内

## 位置持久化
- [x] 拖动完成后坐标保存到 `ipicker/Sundry/SuspendingWindowPos.json`
- [x] JSON 文件格式正确（包含 PosX 和 PosY 字段）
- [x] 启动时正确读取并恢复浮窗位置

## 应用程序集成
- [x] 浮窗随程序启动而启动
- [x] 主窗口关闭时浮窗不消失
- [x] 点击浮窗按钮可以打开主窗口
