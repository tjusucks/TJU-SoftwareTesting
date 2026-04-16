# RealWorld Evaluation Workspace (Simplified)

本项目通过“规范抽取 -> 测试生成 -> 缺陷注入 -> 运行对比”的工作流, 评估生成式测试对真实缺陷的发现能力。当前目标实现为 `golang-gin`。

## 1. Claude Code 使用指南

推荐使用外部免费 API 降低测试成本, 支持国内手机号注册。

### 1.1 推荐平台

- **智谱/iflow**: `https://platform.iflow.cn/models`
- **NVIDIA Build**: `https://build.nvidia.com/explore`

### 1.2 转换与配置

由于 Claude Code 不直接支持 OpenAI Compatible 协议, 需使用转换工具:

- `https://github.com/farion1231/cc-switch`
- `https://github.com/router-for-me/CLIProxyAPI`

安装后可通过环境变量或配置文件指定 API 地址。若安装了对应 skill, 可直接使用 `/skillname` 调用。

## 2. 核心评估流程 (Clean vs Buggy)

### Step 1: 运行 Clean Baseline

启动原始实现, 确保环境干净:

```bash
cd "implementations/golang-gin"
cp .env.example .env
```

在 `.env` 中配置数据库连接等必要参数后, 启动服务:

```bash
go run hello.go
```

运行生成测试, 验证是否通过 (Pass):

```bash
cd "evaluations/golang-gin/tests"
TEST_HOST=http://localhost:8080 go test ./... -v
```

### Step 2: 缺陷注入 (Buggy Run)

应用补丁注入缺陷:

```bash
git -C "implementations/golang-gin" apply "../../bugs/golang-gin/auth-login-smoke.patch"
```

重启服务并再次运行 Step 1 中的测试。

### Step 3: 恢复环境

测试完成后务必撤销补丁:

```bash
git -C "implementations/golang-gin" apply -R "../../bugs/golang-gin/auth-login-smoke.patch"
```

## 3. 结果判定逻辑

只有在 Clean 环境下 Pass 的测试才具有评估意义。

| Clean | Buggy | 结论                                    |
| ----- | ----- | --------------------------------------- |
| Pass  | Fail  | Bug Revealed (成功揭示缺陷)             |
| Pass  | Pass  | Bug Not Revealed (测试覆盖不足)         |
| Fail  | Fail  | Invalid Test (测试本身有误或不适配实现) |

## 4. 目录结构概览

- `specification/features/`: 抽取出的功能规格 (Input).
- `evaluations/golang-gin/tests/`: 可执行的黑盒 Go 测试 (Artifact).
- `bugs/golang-gin/`: 缺陷补丁 (Injectors).

## 5. 快速上手建议

1. **黑盒原则**: 测试仅通过 HTTP 调用接口, 不依赖内部代码。
2. **适配 Baseline**: 若 Clean 实现与上游规范有细微差异 (如空串 vs null), 应调整测试代码以适配 Clean 实现。
3. **扩展方向**: 优先基于 `article-lifecycle.md` 或 `comment-lifecycle.md` 生成新测试。
