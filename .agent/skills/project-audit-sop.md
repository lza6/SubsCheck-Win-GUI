# 技能：项目审计 SOP（SubsCheck-Win-GUI 版）

## 适用场景
对本项目做终局审计、版本发布前检查、或大改动后的回归确认。

## 前置读取
1. `workflow_status.md`（任务台账与遗留）
2. `.wolf/cerebrum.md` + `.wolf/buglog.json`（跨会话教训）
3. `git log --oneline -10` + 未提交 diff

## 步骤（含本项目特有坑）
1. **构建**：必须用 VS MSBuild（`C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`）。Framework MSBuild 仅 C#5 必失败（CS1056）。构建前 `Stop-Process -Name subs-check.win.gui`（文件锁）。
2. **跨进程契约检查**：改 `config/config.yaml` 任何字段前，先确认外部 subs-check.exe 内核是否消费该字段（查上游 README/源码）。**凭据字段必须明文**（内核 Go 程序读取）。
3. **null 路径排查**：改字段初值时，grep 该字段全部消费点（本项目 WebUIapiKey 教训：timer4 轮询 X-API-Key）。
4. **UI 改动**：运行时建控件前查 Designer 坐标防重叠；建后必须 PrintWindow 截图目视验证（枚举到句柄 ≠ 可见）。
5. **验证命令**：
   - 构建：`MSBuild.exe subs-check.win.gui.csproj /t:Rebuild /p:Configuration=Release /v:minimal`
   - 冒烟：启动 exe → `Get-Process subs-check.win.gui` 存活 + MainWindowTitle 正常
   - 门禁：`rg "CMLiussss" *.cs`（仅注释可残留）

## 证据要求
每项结论附真实命令输出；UI 改动附截图；未实测项标"待验证"。

## 回滚方式
每修复一个 commit，独立 `git revert`。

## 常见坑
- exe 文件锁 → 构建前 Stop-Process
- WinForms 类名是 `WindowsForms10.BUTTON.app.xxx` 不是 `Button`
- AutoScaleMode.Font 高 DPI 缩放 → 布局用相对坐标
- 独立验证脚本编译时注意 csc 默认 C#5（不能 lambda/out var）

## 增补（2026-10-26 B1-B6 轮）
- MSTest 命令行运行必须带 /TestAdapterPath:"packages\MSTest.TestAdapter\build\_common"，否则报"没有可用测试"
- YamlDotNet Dictionary<string,object> 序列化 bool 会带类型标签；本方案写读同栈无碍，但若内核侧严格 YAML 解析需抽查（本轮冒烟通过）
- 未知行告警必须去噪：内核每秒多行输出，逐行 Log 会淹没日志；用聚合计数+首次提示
- .NET Framework 4.7.2 无 coverlet，覆盖率标 null 并在 limitations 说明，禁止编造
