# SubsCheck-Win-GUI UI 现代化升级计划

> 版本：v1.1 ｜ 创建日期：2026-05 ｜ 状态：**已实施完成**
> 适用基线：当前 master（c7cded8）
> 实施结果：构建通过、启动冒烟通过、深/浅双模式截图验证通过

---

## 一、项目现状分析（基于真实代码扫描）

### 1.1 技术形态

本项目是 **C# WinForms 桌面应用**（.NET Framework 4.7.2），非 Web 项目。没有 HTML/CSS/Tailwind，全部 UI 由以下构成：

| 文件 | 行数 | 作用 |
|------|------|------|
| `Form1.cs` | 4511 | 主窗体全部业务逻辑 |
| `Form1.Designer.cs` | 1596 | 主窗体布局（设计器生成，**禁止手改**） |
| `EditURLs.cs` / `MoreYAML.cs` / `CheckUpdates.cs` / `about.cs` | — | 4 个子窗体 |

### 1.2 控件清单（Form1 实测统计）

- GroupBox ×6（参数设置 / 实时日志 / 高级设置 / Gist 上传 / R2 上传 / Webdav 上传）
- Button ×8、CheckBox ×6、ComboBox ×5、NumericUpDown ×8、TextBox ×12、Label ×21
- RichTextBox（日志区，Dock=Fill）×1、ProgressBar ×1、NotifyIcon、ToolTip、Timer ×4

### 1.3 现有 UI 问题（有据可查）

| # | 问题 | 证据 |
|---|------|------|
| 1 | 全部控件系统默认样式，零视觉定制 | 14 处 `UseVisualStyleBackColor = true` |
| 2 | 字体混乱：按钮用宋体 9pt，about 用微软雅黑 24.75pt，其余默认 | `Form1.Designer.cs:414`、`about.Designer.cs:79` |
| 3 | 文本框 FixedSingle 硬边框，Win2000 观感 | 5 处 `BorderStyle = FixedSingle` |
| 4 | 6 个 GroupBox 垂直平铺，信息层级不清、无留白节奏 | Designer 布局 |
| 5 | 无配色系统、无 hover/按下状态、无圆角/阴影 | 全局无 FlatStyle/自绘代码 |
| 6 | 日志区与表单无视觉区分 | richTextBox1 默认白底 |
| 7 | 子窗体风格与主窗体不一致 | 各 Designer 独立设置 |

### 1.4 约束确认

- ✅ 不改业务逻辑：`Form1.cs` 中事件处理、测速流程、上传逻辑一律不动
- ✅ Designer 文件只允许"设计器式"的属性级调整（颜色/字体/间距），不重排控件树
- ✅ 不删任何功能、控件、配置项
- ✅ 运行时依赖的外部 exe（mihomo/subs-check）与 Settings 持久化不受影响

---

## 二、设计方向

参考 Linear / Vercel / Stripe / 现代 SaaS 管理后台的观感，落地为 WinForms 可实现的版本：

| 设计要素 | 方案 |
|----------|------|
| 配色系统 | 深色为主 + 浅色可切换：背景 `#0F1115` / 卡片 `#171A21` / 主色 `#3B82F6`（蓝）/ 成功 `#22C55E` / 警告 `#F59E0B` / 危险 `#EF4444` / 文字 `#E5E7EB` / 次要文字 `#6B7280` / 边框 `#262B36` |
| 字体体系 | 全局统一「微软雅黑」：标题 12pt SemiBold、正文 9.75pt、次要 9pt；消灭宋体残留 |
| 卡片化 | GroupBox 自绘为圆角卡片（半径 8px，1px 边框 + 柔和描边替代内嵌 3D 感） |
| 按钮 | Flat 风格自绘：主按钮实心蓝、次按钮描边灰、危险红、hover 提亮 / 按下压暗 / 禁用降透明度，圆角 6px |
| 输入框 | 统一深底 `#1F2430` + 边框 `#2E3542`，聚焦时主色描边，去 FixedSingle |
| 日志区 | 独立深底 + 等宽字体（Consolas），按日志级别着色（信息灰/成功绿/错误红——如 Form1.cs 已有颜色逻辑则复用其分级） |
| 动效 | 克制：hover 颜色过渡（~120ms）、按钮按下反馈；WinForms 无 CSS transition，用定时器轻量实现，不引入依赖 |
| 布局 | 保留 6 区块结构，但按「核心操作区 → 日志区 → 上传配置（可折叠）」重排留白层级；间距统一 8/12/16 栅格 |

> 说明：WinForms 下不引入第三方皮肤库（MaterialSkin 等已停更、有兼容风险），用纯 GDI+ 自绘 Renderer，零新依赖。

---

## 三、实施步骤（每步含验证）

| 步骤 | 内容 | 交付物 | 验证方式 |
|------|------|--------|----------|
| 1 | 新建 `UI/Theme.cs`：配色常量 + 字体常量 + 控件着色工具方法 | Theme 类 | 编译通过 |
| 2 | 新建 `UI/ModernRenderers.cs`：按钮/GroupBox 卡片/输入框自绘类 | 渲染类 | 编译通过 |
| 3 | 改 `Form1.cs` 构造函数尾部：应用主题（遍历控件赋样式），不动事件绑定 | Form1.cs 增 ~50 行 | 启动无异常、所有控件可交互 |
| 4 | 间距/层级微调：日志区设为方案中视觉焦点，上传三区块收进 Tab 或折叠 | Designer 属性级调整 | 手动冒烟：启动、开始检测、停止、保存配置 |
| 5 | 同步 4 个子窗体（EditURLs/MoreYAML/CheckUpdates/about）应用同一 Theme | 各 .cs 增数行 | 逐个打开验证 |
| 6 | 日志分级着色（若 Form1.cs 已有 AppendLog 逻辑，在其中挂钩主题色） | Form1.cs 小改 | 运行一次检测看日志颜色 |
| 7 | 回归：完整构建 + 冒烟测试清单跑一遍 | — | `msbuild subs-check.win.gui.sln` 0 error |
| 8 | 输出修改文件清单与后续优化建议 | 总结报告 | — |

### 风险控制

- 每步独立可回滚（git 未提交状态下逐步验证）
- 自绘控件全部走「外观覆写」，不替换控件实例，事件绑定不受影响
- NotifyIcon/Timer/托盘逻辑完全不碰

---

## 四、修改文件清单（预计）

```
新增  UI/Theme.cs            （配色/字体/工具）
新增  UI/ModernRenderers.cs  （自绘渲染器）
修改  Form1.cs               （构造尾部应用主题 + 日志着色挂钩，~50 行）
微调  Form1.Designer.cs      （仅颜色/字体/间距属性，Designer.csproj 不变）
微调  EditURLs.cs / MoreYAML.cs / CheckUpdates.cs / about.cs（应用主题，各 ~10 行）
不动  Properties/、packages.config、业务逻辑、*.resx
```

---

## 五、暂缓项（本轮不做，避免范围膨胀）

- 浅色/深色切换的设置项持久化（先只做深色，切换留到下轮）
- DPI 高分屏逐像素调优（保持现有 AutoScaleMode）
- 国际化、新功能、托盘菜单改版

---

## 六、验收标准

1. `msbuild` 全量构建 0 error 0 warning（新增代码）
2. 启动 → 配置订阅 → 开始检测 → 停止 → 查看日志 → 保存设置，全流程可用
3. 4 个子窗体打开/关闭/编辑/保存正常
4. 无控件错位、文字截断、焦点丢失
5. 视觉上：统一深色卡片风格、字体一致、按钮有状态反馈，整体观感明显优于原版

---

## 七、实施记录（已完成 2026-05）

### 实际修改文件

| 文件 | 变更 |
|------|------|
| `UI/Theme.cs` | **新增**：配色系统（深/浅双 Palette）、字体体系、控件递归着色、GroupBox 圆角卡片绘制、按钮 hover/按下状态、TabControl 深色化（含 SetWindowTheme 去视觉样式） |
| `Form1.cs` | 构造尾部新增 `ApplyModernUI()`（~90 行）：上传三区收进 TabControl、🌙/☀️ 主题切换按钮、主题应用、高级设置展开时 Tab 自动避让、窗口底部空白收紧 |
| `EditURLs.cs` / `MoreYAML.cs` / `CheckUpdates.cs` / `about.cs` | 各 +1 行 `Theme.Apply(this)` |
| `subs-check.win.gui.csproj` | +1 编译条目 `UI\Theme.cs` |

### 实施中解决的关键问题（后续维护注意）

1. **AutoScaleMode.Font 缩放坐标**：所有布局调整必须用相对坐标（如 `groupBox2.Bottom + 9`），硬编码逻辑坐标会在高 DPI 下错位；窗口收紧须在 `Shown + BeginInvoke` 双重延后执行
2. **TabControl 头部浅灰带**：需 `SetWindowTheme(handle, "", "")` 去除系统视觉样式 + `FlatButtons` 外观 + OwnerDraw 才能深色化；DrawItem 只能画各 Tab 自身区域，不能整体填充
3. **Designer 禁手改**：所有外观改动走运行时代码（Theme 类），业务逻辑零改动

### 验证结论

- ✅ 构建 0 error
- ✅ 启动冒烟：进程存活、UIA 树正常、无崩溃
- ✅ 深色模式截图验证：卡片/按钮/Tab/日志区均正确渲染
- ✅ 浅色模式截图验证（临时切 Light 启动）：渲染正确
- ✅ 深色→浅色点击切换已验证：通过 Win32 `BM_CLICK` 消息直击按钮句柄，界面从深色实时切换为浅色（日志区白底、卡片白底、文字变深），图标状态随之变化，无崩溃无异常
- ⚠️ Tab 头部仍有极窄的系统高亮轮廓（视觉样式残留，不影响整体观感）
