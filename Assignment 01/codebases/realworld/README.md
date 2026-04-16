# RealWorld Evaluation Workspace (Simplified)

本项目通过“规范抽取 -> 测试生成 -> 缺陷注入 -> 运行对比”的工作流, 评估生成式测试对真实缺陷的发现能力。当前目标实现为 `golang-gin`。

代码实现目录和上游规范目录均通过 Git submodule 引入, 因此在 clone 仓库后需要先初始化并拉取 submodule, 否则 `implementations/` 或 `specification/upstream/` 下的内容可能为空。

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

### Step 0: 初始化 submodule

首次 clone 后, 请先在仓库根目录执行:

```bash
git submodule update --init --recursive
```

如果你还没有 clone 仓库, 也可以直接使用:

```bash
git clone --recursive <repository-url>
```

完成后再进入本目录继续后续步骤。

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
TEST_HOST=http://localhost:8080 go test ./... -v -count=1
```

### Step 2: 缺陷注入 (Buggy Run)

应用补丁注入缺陷:

```bash
git -C "implementations/golang-gin" apply "../../bugs/golang-gin/auth-login-smoke.patch"
```

重启服务并再次运行 Step 1 中的测试。

注意, Go test 默认会缓存结果。如果引入 patch 后直接重复执行测试, 可能看到和之前相同的 cached 结果, 导致误以为补丁没有影响。为确保在 clean 和 buggy 之间切换后测试会被强制重新执行, 请使用 `-count=1`, 例如:

```bash
cd "evaluations/golang-gin/tests"
TEST_HOST=http://localhost:8080 go test ./... -v -count=1
```

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

1. **先初始化 submodule**: clone 后先执行 `git submodule update --init --recursive`, 确保实现代码和上游规范都已拉取完整。
2. **禁用测试缓存**: 在 clean 和 buggy 之间切换后, 运行 Go 测试时建议始终加上 `-count=1`, 避免读取 cached 结果。
3. **黑盒原则**: 测试仅通过 HTTP 调用接口, 不依赖内部代码。
4. **适配 Baseline**: 若 Clean 实现与上游规范有细微差异 (如空串 vs null), 应调整测试代码以适配 Clean 实现。
5. **扩展方向**: 优先基于 `article-lifecycle.md` 或 `comment-lifecycle.md` 生成新测试。
