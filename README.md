# SubsCheck-Win-GUI

一个功能强大的节点测速与订阅转换工具，为Windows用户提供便捷的图形化界面。

## 项目简介

SubsCheck-Win-GUI 是基于 [subs-check](https://github.com/beck-8/subs-check) 和 [BestSub](https://github.com/bestruirui/BestSub) 开发的图形化界面工具，旨在简化节点测速、订阅管理和流媒体解锁检测的操作流程。

本项目仅供学习、研究与安全测试使用，请勿用于任何非法活动。

## 主要功能

- **节点测速**：支持并发测速，快速检测节点的下载速度和延迟
- **流媒体解锁检测**：检测 Netflix、Disney+、OpenAI 等流媒体服务的解锁情况
- **订阅管理**：支持多种订阅格式（Link、Base64、Clash）
- **多种保存方式**：支持本地、Cloudflare R2、GitHub Gist、WebDAV 等多种测速结果保存方式
- **Sub-Store 集成**：内置 Sub-Store，支持订阅转换和规则管理
- **定时任务**：支持定时自动测速，支持 cron 表达式
- **开机启动**：支持开机自动运行，后台静默测速
- **系统托盘**：最小化到系统托盘，不占用任务栏空间

## 系统要求

- **操作系统**: Windows 10/11 (32位/64位)
- **.NET 框架**: .NET Framework 4.7.2 或更高版本

> 注意：本软件不支持 Windows 7 及更早版本。

## 快速开始

### 安装

1. 下载最新版本的压缩包
2. 解压到任意目录
3. 双击运行 `subs-check.win.gui.exe`

### 使用说明

1. **添加订阅链接**：在"节点池订阅地址"文本框中输入订阅链接，支持多行输入
2. **配置测速参数**：
   - 并发线程数：推荐设置为宽带峰值/50M
   - 超时时间：节点的最大延迟
   - 下载测试时间：与下载链接大小相关
3. **选择测速地址**：从下拉菜单中选择测速地址
4. **点击启动**：点击"▶️ 启动"按钮开始测速
5. **查看结果**：测速完成后，结果将保存在 output 文件夹中

## 配置说明

### 测速结果保存方式

支持以下几种保存方式：

- **本地**：将结果保存到本地的 output 文件夹
- **r2**：将结果保存到 Cloudflare R2 存储桶
- **gist**：将结果保存到 GitHub Gist
- **webdav**：将结果保存到 WebDAV 服务器

### 高级功能

- **流媒体检测**：勾选"流媒体检测"可检测 Netflix、Disney+、OpenAI 等流媒体服务的解锁情况
- **节点重命名**：勾选"节点地址查询"可根据节点IP位置重命名节点
- **Sub-Store**：内置 Sub-Store，支持订阅转换和规则管理
- **定时任务**：支持定时自动测速，可使用分钟倒计时或 cron 表达式
- **开机启动**：勾选"开机启动"可设置开机自动运行

## 文件结构

```
SubsCheck-Win-GUI/
├── subs-check.win.gui.exe       # GUI 本体
├── subs-check.exe               # subs-check 内核 (32位)
├── config/
│   ├── config.yaml              # 配置文件
│   └── more.yaml                # 补充YAML参数配置
├── output/
│   ├── all.yaml                 # YAML格式测试结果
│   ├── base64.txt               # Base64格式测试结果
│   └── ...                      # 其他输出文件
└── README.md                    # 项目说明文档
```

## 开发与构建

### 开发环境

- Visual Studio 2019 或更高版本
- .NET Framework 4.7.2
- NuGet 包管理器

### 依赖项

- Newtonsoft.Json 13.0.3
- YamlDotNet 16.3.0

### 构建步骤

1. 使用 Visual Studio 打开 `subs-check.win.gui.sln`
2. 选择 Release 或 Debug 配置
3. 点击"生成解决方案"

## 致谢

本项目基于以下开源项目开发：

- [subs-check](https://github.com/beck-8/subs-check) - 节点测速工具核心
- [BestSub](https://github.com/bestruirui/BestSub) - 原始项目
- [Sub-Store](https://github.com/sub-store-org/Sub-Store) - 订阅转换工具
- [ACL4SSR](https://github.com/ACL4SSR/ACL4SSR) - Clash 规则集

## 免责声明

本项目仅供学习、研究与安全测试使用，请勿用于任何非法活动。使用前请确保您已了解并遵守所在地的法律法规。

### 使用条款

- **教育与研究用途**：本软件仅可用于网络技术和编程领域的学习、研究和安全测试
- **禁止非法使用**：严禁将本软件用于任何非法活动或违反使用者所在地区法律法规的行为
- **使用时限**：基于学习和研究目的，建议用户在完成研究或学习后，或在安装后的 24 小时内删除本软件及所有相关文件
- **免责声明**：本软件的创建者和贡献者不对因使用或滥用本软件而导致的任何损害或法律问题负责
- **用户责任**：用户对使用本软件的方式以及由此产生的任何后果完全负责
- **无技术支持**：本软件的创建者不提供任何技术支持或使用协助

使用本软件即表示您已阅读并理解本免责声明，并同意受其条款的约束。

## 许可证

本项目遵循相关开源项目的许可证。

## 联系方式

如有问题或建议，欢迎通过以下方式联系：

- GitHub Issues: https://github.com/lza6/SubsCheck-Win-GUI/issues

---

**请记住**：本软件的主要目的是促进学习、研究和安全测试。作者不支持或认可任何其他用途。使用者应当在合法和负责任的前提下使用本工具。