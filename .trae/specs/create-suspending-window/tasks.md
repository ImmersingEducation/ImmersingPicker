# Tasks

- [x] Task 1: 创建 SuspendingWindow 浮窗窗口
  - [x] SubTask 1.1: 创建 `ImmersingPicker/Views/SuspendingWindow.axaml` 浮窗窗口 XAML 文件，设置圆角、半透明、无标题栏、置顶属性
  - [x] SubTask 1.2: 创建 `ImmersingPicker/Views/SuspendingWindow.axaml.cs` 浮窗窗口代码后台文件
  - [x] SubTask 1.3: 在浮窗中添加打开主窗口的按钮

- [x] Task 2: 创建 MovingControlButton 拖动控件
  - [x] SubTask 2.1: 创建 `ImmersingPicker/Views/suspendingWindow/` 目录
  - [x] SubTask 2.2: 创建 `MovingControlButton.axaml` 拖动控件 XAML 文件
  - [x] SubTask 2.3: 创建 `MovingControlButton.axaml.cs` 拖动控件代码后台文件，实现拖动逻辑和边界限制（距离屏幕边界5px）

- [x] Task 3: 实现位置持久化功能
  - [x] SubTask 3.1: 创建位置配置模型类（包含 PosX 和 PosY 属性）
  - [x] SubTask 3.2: 实现位置保存功能，在拖动完成后将坐标写入 `ipicker/Sundry/SuspendingWindowPos.json`
  - [x] SubTask 3.3: 实现位置读取功能，启动时从配置文件读取并设置浮窗位置

- [x] Task 4: 集成浮窗到应用程序启动流程
  - [x] SubTask 4.1: 修改 `App.axaml.cs`，在 `OnFrameworkInitializationCompleted` 中创建并显示 SuspendingWindow
  - [x] SubTask 4.2: 确保浮窗独立于主窗口生命周期（主窗口关闭时浮窗不关闭）

# Task Dependencies
- [Task 2] depends on [Task 1]
- [Task 3] depends on [Task 2]
- [Task 4] depends on [Task 1, Task 3]
