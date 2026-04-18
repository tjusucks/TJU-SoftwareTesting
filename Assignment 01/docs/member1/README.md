# Member1 使用说明（中文）

本目录是成员1（Prompt负责人）的完整交付物，目标是让组员可以快速理解并复现实验流程。

## 1. 目录结构说明

- `prompts/`
  - `prompt-v1.txt`：基线版，强调结构稳定
  - `prompt-v2.txt`：强调需求映射完整性
  - `prompt-v3.txt`：强调边界/异常覆盖与去重
- `templates/`
  - `requirement-input-template.md`：输入模板（RequirementInputV1）
  - `generated-output-template.md`：输出模板（7段结构 + 用例字段）
- `experiments/`
  - `real-input-pack.md`：真实输入包（RealWorld + TodoMVC）
  - `experiment-protocol.md`：实验协议（同输入、同口径）
  - `run-v1.md` / `run-v2.md` / `run-v3.md`：三轮记录
  - `metrics-summary.md`：指标汇总与增量对比
- 其他文档
  - `01-constraints-and-prompt-v1.md`：约束与 v1 设计
  - `02-io-templates-freeze.md`：I/O 契约冻结
  - `handoff-member2-codebase.md`：给成员2的交接
  - `handoff-member3-execution.md`：给成员3的交接
  - `handoff-member4-evaluation.md`：给成员4的交接
  - `final-checklist.md`：最终提交前检查项

## 2. 运行前准备

### 必需环境

1. 已安装 `claude` CLI
2. 已安装并可用 `ccr`（claude-code-router）
3. 已配置可用 API key（通过环境变量注入）

### 快速检查

```bash
source ~/.zshrc
ccr status
claude --version
```

如果 `ccr status` 显示未运行，先执行：

```bash
ccr start
```

## 3. 复现方式（推荐）

### 方式A：使用 skill 命令

在 Claude Code 中调用你们安装的 blackbox skill，然后输入 `experiments/real-input-pack.md` 中的需求内容。

### 方式B：直接用 prompt 文件

按 v1/v2/v3 分别运行，保持同一输入文本与同一评估口径，输出保存到对应实验记录。

最低要求：
- 使用相同输入包（`real-input-pack.md`）
- 对照 `experiment-protocol.md` 的指标定义
- 更新 `run-v1/v2/v3.md` 与 `metrics-summary.md`

## 4. 输出必须满足的格式

生成结果必须保持以下 7 个 section（顺序固定）：

1. Feature Summary
2. Requirements Extracted
3. Test Design Strategy
4. Test Scenarios
5. Detailed Test Cases
6. Coverage Summary
7. Ambiguities / Missing Information / Assumptions

并且 `Detailed Test Cases` 至少包含字段：

- Test Case ID
- Title
- Requirement Reference
- Preconditions
- Test Data
- Steps
- Expected Result
- Priority
- Risk/Notes

## 5. 常见问题排查

### Q1: 401 / 鉴权失败

- 检查 API key 是否有效
- 检查 `~/.zshrc` 是否已加载：`source ~/.zshrc`
- 重启 router：`ccr restart`

### Q2: 输出格式不稳定

- 先用 `prompt-v1` 验证结构
- 再切到 `prompt-v2/v3` 做覆盖增强
- 对照 `templates/generated-output-template.md` 做结构校验

### Q3: 指标无法对比

- 确认三轮使用同一个输入包
- 确认公式与口径按 `experiment-protocol.md` 一致

## 6. 与组员协作的最短路径

- 成员2（输入整理）：先看 `handoff-member2-codebase.md`
- 成员3（执行脚本）：先看 `handoff-member3-execution.md`
- 成员4（评估统计）：先看 `handoff-member4-evaluation.md`
- 报告负责人：直接复用 `report-materials/` 下三份文档

## 7. 提交前最后检查

按 `final-checklist.md` 逐项确认，重点关注：

- prompts/model/analysis 是否齐全
- 真实输入来源是否可追溯
- 报告内容是否与当前真实输入实验一致
