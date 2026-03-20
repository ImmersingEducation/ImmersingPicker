# SuspendingWindow（浮窗功能）Spec

## Why
提供一个快速访问主窗口的浮窗入口，用户可以通过桌面上的小型悬浮窗口快速打开主程序，无需通过系统托盘操作，提升用户体验和操作便捷性。

## What Changes
- 新增 `SuspendingWindow` 浮窗窗口，具有圆角半透明竖形矩形外观
- 新增 `MovingControlButton` 控件用于实现窗口拖动功能
- 新增位置持久化功能，保存和恢复浮窗位置
- 修改 `App.axaml.cs` 启动逻辑，随程序启动浮窗

## Impact
- Affected specs: 无
- Affected code:
  - `ImmersingPicker/App.axaml.cs` - 添加浮窗启动逻辑
  - `ImmersingPicker/Views/` - 新增 `SuspendingWindow.axaml` 和 `SuspendingWindow.axaml.cs`
  - `ImmersingPicker/Views/suspendingWindow/` - 新增 `MovingControlButton.axaml` 和 `MovingControlButton.axaml.cs`

## ADDED Requirements

### Requirement: SuspendingWindow 浮窗窗口
系统应提供一个独立的浮窗窗口，具有以下特性：
- 圆角半透明竖形矩形外观
- 无标题栏
- 始终置顶显示
- 随程序启动而启动
- 主窗口关闭时不消失

#### Scenario: 浮窗启动
- **WHEN** 应用程序启动时
- **THEN** 浮窗窗口自动显示

#### Scenario: 主窗口关闭
- **WHEN** 用户关闭主窗口
- **THEN** 浮窗窗口保持显示

### Requirement: MovingControlButton 拖动控件
系统应提供一个拖动控件用于移动浮窗窗口：
- 通过拖动按钮移动整个浮窗窗口
- 限制窗口不能移出屏幕边界（距离边界5px时阻止继续移动）

#### Scenario: 正常拖动
- **WHEN** 用户拖动 MovingControlButton
- **THEN** 浮窗窗口跟随移动

#### Scenario: 边界限制
- **WHEN** 用户拖动浮窗到距离屏幕边界5px以内
- **THEN** 阻止窗口继续向该方向移动

### Requirement: 打开主窗口按钮
浮窗内应包含一个按钮，点击后打开程序主窗口。

#### Scenario: 打开主窗口
- **WHEN** 用户点击浮窗内的按钮
- **THEN** 主窗口显示并激活

### Requirement: 位置持久化
系统应保存和恢复浮窗窗口的位置：
- 位置数据存储在 `ipicker/Sundry/SuspendingWindowPos.json` 文件中
- JSON 格式包含 `PosX` 和 `PosY` 字段

#### Scenario: 保存位置
- **WHEN** 用户完成拖动浮窗
- **THEN** 当前坐标写入到 `ipicker/Sundry/SuspendingWindowPos.json`

#### Scenario: 恢复位置
- **WHEN** 应用程序启动且存在位置配置文件
- **THEN** 浮窗显示在保存的位置

### Requirement: 文件结构
浮窗相关代码应放置在指定目录：
- `ImmersingPicker/Views/SuspendingWindow.axaml` - 浮窗窗口 XAML
- `ImmersingPicker/Views/SuspendingWindow.axaml.cs` - 浮窗窗口代码
- `ImmersingPicker/Views/suspendingWindow/MovingControlButton.axaml` - 拖动控件 XAML
- `ImmersingPicker/Views/suspendingWindow/MovingControlButton.axaml.cs` - 拖动控件代码
