# <img src="docs/assets/icon.png" height="32" width="32"> ImmersingPicker - 简单，公平，有趣

> **简单，公平，有趣** - 一款基于座位表的课堂随机抽选软件

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Avalonia UI](https://img.shields.io/badge/Avalonia-11.3-0078D4?logo=nuget)](https://avaloniaui.net/)
[![Version](https://img.shields.io/badge/version-0.3.0--Beta-ff69b4)](https://github.com/ImmersingEducation/ImmersingPicker/releases)

---

## 📖 简介

ImmersingPicker 是一款专为课堂设计的**随机抽选软件**，通过可视化的座位表界面，让课堂提问更加公平、高效、有趣。

### ✨ 主要特性

- 🎯 **可视化座位表** - 直观展示学生座位布局，支持自定义行列排列
- ⚖️ **公平抽选算法** - 基于权重的智能算法，确保每个学生被选中的机会均等
- 📊 **历史记录** - 完整记录每次抽选结果，支持查看和导出
- 🎨 **现代化界面** - 采用 Fluent Design 设计语言，支持亮色/暗色主题切换
- 🔒 **安全保护** - 支持密码保护、USB 设备安全检查
- 📱 **悬浮窗口** - 桌面悬浮按钮，快速访问，支持左右停靠
- 🔄 **自动保存** - 定时保存数据，防止意外丢失

---

## 🚀 快速开始

### 系统要求

- **操作系统**: Windows 10/11 x86、Linux x86（建议 X11 桌面环境）
- **.NET 运行时**: .NET 10.0 或更高版本
- **内存**: 最低 512MB RAM
- **磁盘空间**: 100MB 可用空间

### 安装步骤

#### 方法一：下载预编译版本（推荐）

1. 访问 [Releases 页面](https://github.com/ImmersingEducation/ImmersingPicker/releases)
2. 下载对应平台的安装包
3. 运行安装程序，按照提示完成安装

#### 方法二：从源代码编译

**前置条件：**
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Git

**编译步骤：**

```bash
# 克隆仓库
git clone https://github.com/ImmersingEducation/ImmersingPicker.git
cd ImmersingPicker

# 还原依赖
dotnet restore

# 编译 Release 版本
dotnet build -c Release

# 发布应用（可选）
dotnet publish -c Release -r win-x64 --self-contained
```

编译产物位于 `ImmersingPicker/bin/Release/net10.0/` 目录。

---

## 📖 使用指南

### 首次启动向导

首次启动 ImmersingPicker 时，会进入欢迎向导：

1. **欢迎页面** - 了解软件基本信息
2. **许可协议** - 阅读并同意 GPL v3 开源协议
3. **班级初始化** - 创建第一个班级
4. **外观设置** - 选择主题颜色和显示模式

### 创建班级

1. 点击左侧导航栏的 **编辑器** 图标
2. 点击 **"+ 新建班级"** 按钮
3. 输入班级名称（如"三年级一班"）
4. 点击确认创建

### 添加学生

#### 方式一：手动添加

1. 进入 **编辑器** 页面
2. 选择要编辑的班级
3. 点击 **"+ 新建学生"** 按钮
4. 填写学生信息：
   - **姓名**：学生姓名
   - **学号**：唯一标识符
   - **座位**：格式为"行，列"（如"3,5"表示第 3 行第 5 列）
5. 点击确认保存

#### 方式二：表格编辑

在编辑器中使用表格视图批量编辑学生信息，支持：
- 直接修改姓名、学号、座位
- 删除学生（右键菜单）
- 拖拽调整座位位置

### 开始抽选

1. 返回 **首页**
2. 从下拉列表选择班级
3. 使用 **"+"** 和 **"-"** 按钮调整抽选人数
4. 点击 **"开始抽选！"** 按钮
5. 观看动画效果后查看结果

抽选过程中，座位表上的选中学生会以**绿色边框**高亮显示。

### 查看历史记录

1. 点击左侧导航栏的 **历史** 图标
2. 浏览历史抽选记录
3. 每条记录包含：
   - 抽选时间
   - 使用的抽选器
   - 被选中的学生列表

### 自定义设置

点击左侧导航栏的 **设置** 图标，可以访问：

#### 基本设置
- 主题模式（系统/亮色/暗色）
- 主题颜色
- 开机自启动
- 语言设置

#### 抽选器设置
- 权重计算参数（10 个可调参数）
- 抽选算法配置
- 公平性调整

#### 首页设置
- 动画播放次数
- 动画延迟
- 座位表排列方式（从上到下/从下到上，从左到右/从右到左）

#### 悬浮窗口设置
- 启用/禁用悬浮窗口
- 停靠位置（左侧/右侧）
- 垂直位置调整

#### 安全与隐私
- 密码保护（访问编辑器和设置需要验证）
- USB 设备安全检查
- 隐私模式

---

## 🔧 高级功能

### 密码保护

启用密码保护后，访问以下功能需要验证：
- 编辑器（添加/修改学生）
- 设置页面

**设置密码：**
1. 进入 **设置 > 安全与隐私**
2. 启用 **密码保护**
3. 设置密码（建议使用强密码）

### USB 安全检查

启用后，只有在插入授权的 USB 设备时才能运行软件，适合教室电脑等公共环境。

### 悬浮窗口

悬浮窗口提供快速访问入口：
- 主窗口失去焦点时自动显示
- 支持拖拽调整位置
- 双击打开主窗口

---

## 📸 界面预览

### 主界面
![主界面](docs/assets/screenshot-home.png)

### 座位表抽选
![抽选](docs/assets/screenshot-picking.png)

### 编辑器
![编辑器](docs/assets/screenshot-editor.png)

### 设置页面
![设置](docs/assets/screenshot-settings.png)

---

## 🏗️ 技术架构

### 项目结构

```
ImmersingPicker/
├── ImmersingPicker/           # 主程序（UI 层）
│   ├── Views/                 # 窗口和页面
│   │   ├── MainPages/         # 主页面（首页、历史）
│   │   ├── SettingsPages/     # 设置页面
│   │   ├── EditorPages/       # 编辑器页面
│   │   └── WelcomePages/      # 欢迎向导页面
│   ├── Controls/              # 自定义控件
│   ├── Services/              # 导航服务
│   └── Helpers/               # 辅助类
├── ImmersingPicker.Core/      # 核心层（领域模型）
│   ├── Models/                # 数据模型
│   ├── Abstractions/          # 抽象接口
│   └── Exceptions/            # 自定义异常
└── ImmersingPicker.Services/  # 服务层
    ├── Services/
    │   ├── Picker/            # 抽选器实现
    │   ├── Storage/           # 存储服务
    │   └── Security/          # 安全服务
    └── Helper/                # 工具类
```

### 技术栈

- **.NET 10** - 跨平台运行时
- **Avalonia UI 11.3** - 跨平台 UI 框架
- **FluentAvaloniaUI 2.4** - Fluent Design 控件库
- **Serilog** - 结构化日志记录
- **EPPlus** - Excel 文件处理
- **ClassIsland.IPC** - 进程间通信

---

## 🤝 贡献指南

欢迎贡献代码、报告问题或提出建议！

### 开发环境搭建

```bash
# 克隆仓库
git clone https://github.com/ImmersingEducation/ImmersingPicker.git
cd ImmersingPicker

# 还原 NuGet 包
dotnet restore

# 运行调试版本
dotnet run --project ImmersingPicker
```

或者，使用 Jetbrains Rider 快速搭建环境！

### 提交代码

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

### 代码规范

- 遵循 C# 命名规范
- 使用可空引用类型（nullable reference types）
- 编写单元测试覆盖核心逻辑
- 保持代码简洁、可读

---

## 📄 开源协议

本项目采用 **GNU General Public License v3.0** 协议开源。

- ✅ 允许商业使用
- ✅ 允许修改和分发
- ✅ 允许专利使用
- ❌ 不提供担保
- ⚠️ 修改后必须开源
- ⚠️ 必须保留版权和许可声明

详见 [LICENSE](LICENSE) 文件。

---

## 🙏 致谢

- [Avalonia UI](https://avaloniaui.net/) - 跨平台 UI 框架
- [FluentAvalonia](https://github.com/amwx/FluentAvalonia) - Fluent Design 控件
- [ClassIsland](https://github.com/ClassIsland/ClassIsland) - 课程表软件
- [Serilog](https://serilog.net/) - 日志框架
- [EPPlus](https://epplussoftware.com/) - Excel 处理库

感谢所有贡献者和用户的支持！

---

## 📬 联系方式

- **项目地址**: https://github.com/ImmersingEducation/ImmersingPicker
- **问题反馈**: https://github.com/ImmersingEducation/ImmersingPicker/issues

---


<div align="center">

**Made by ImmersingEducation**

⭐ 如果这个项目对你有帮助，请给一个 Star 支持！

</div>
