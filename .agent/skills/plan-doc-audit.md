# Skill: plan-doc-audit — 计划文档闭环审计与清理

## 适用场景
清理已完成使命的计划/指南/清单类 md；防止半成品计划散落。

## 前置读取
- `docs/archive/plans/*/archive-manifest.md`（已有归档，勿重复）
- 各候选文档全文

## 步骤
1. 扫描：`find . -name "*.md"` 排除 README/归档/依赖/记忆目录，按文件名含"计划|指南|清单|待办|路线图"筛选
2. 逐文档判定：
   - 全部任务已闭环/过时/不适用 → 可归档删除
   - 含未闭环任务或待用户决策 → 保留
3. 归档：复制到 `docs/archive/plans/YYYYMMDD/`（保留原文件名）
4. 更新 `archive-manifest.md`（原路径/任务数/闭环数/删除时间/回滚方式）
5. 确认归档文件存在后删除原文件

## 验收标准
manifest 与实际归档文件一一对应；保留文档有明确理由。

## 回滚
从归档目录复制回原路径。

## 常见坑
- 文档标注"已完成"不可直接采信——抽查实现与证据（T-100.json 等）
- 长期维护文档（weekly.md、workflow_status.md、policy.md、test_ledger.md）永不删除
