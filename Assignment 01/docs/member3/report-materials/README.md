# 审阅说明

## 1. 本目录的作用

本目录保存的是 `docs/member3/newman/` 下 RealWorld API 执行资产，以及对应的执行摘要与测试报告。

当前目录中的主要文件如下：

- `api-generated.postman_collection.json`
  - Newman / Postman 使用的主测试集合。
  - 按黑盒测试用例生成，包含 API 请求、断言和必要的变量传递逻辑。

- `api-generated.environment.json`
  - Newman 运行所需的环境变量文件。
  - 包括 `APIURL`、测试用户、token、slug、commentId` 等变量。

- `run-api-tests.sh`
  - 一键执行脚本。
  - 运行后会调用 Newman 执行 collection，并输出 `api-run.json`。

- `api-run.json`
  - Newman 的原始执行结果。
  - 便于教师或后续成员复核每个请求与断言的运行情况。

- `execution-summary.md`
  - 执行摘要。
  - 说明输入校验、环境检查和测试资产映射方式。

- `test-report.md`
  - 测试运行报告。
  - 汇总 API 测试的通过情况、失败项和当前基线实现暴露出的问题。

## 2. 如何搭建环境并运行本目录中的测试

### 2.1 前置准备

建议先在仓库根目录执行：

```bash
git submodule update --init --recursive
```

需要具备以下环境：

- Node.js
- Newman
- Go
- 可用的数据库环境（供 RealWorld `golang-gin` 后端使用）

本目录对应的 API 目标地址默认为：

```text
http://localhost:8080/api
```

### 2.2 启动 RealWorld 后端

进入后端目录：

```bash
cd "../../codebases/realworld/implementations/golang-gin"
cp .env.example .env
```

然后根据本地数据库配置修改 `.env`，再启动服务：

```bash
go run hello.go
```

启动成功后，后端默认监听 `http://localhost:8080`。

### 2.3 在本目录中一键执行 API 测试

回到当前 `report-materials/` 目录后，执行：

```bash
bash ./run-api-tests.sh
```

如需显式指定接口地址，可执行：

```bash
APIURL=http://localhost:8080/api bash ./run-api-tests.sh
```

主要输出结果为：

- `api-run.json`
- 终端中的 Newman 执行结果

如需查看已整理好的执行结论，可直接阅读：

- `execution-summary.md`
- `test-report.md`

## 3. 如何使用 Claude 插件

本仓库中的 Claude 插件位于：

- `blackbox-testing/`

其中包含两个可直接调用的 skill：

- `blackbox-testing`
  - 根据需求文档生成黑盒测试用例。
- `blackbox-execution`
  - 将黑盒测试输出转换为可执行资产，并生成环境检查与执行报告。

对应说明文件为：

- `blackbox-testing/skills/SKILL.md`
- `blackbox-testing/skills/SKILL2.md`

### 3.1 在 Claude Code 中的典型使用方式

如果已经在仓库根目录打开 Claude Code，可以直接使用这两个 skill：

```text
/blackbox-testing
```

适合输入需求文档或规格说明，让插件生成结构化黑盒测试用例。

然后可继续使用：

```text
/blackbox-execution
```

适合输入前一步生成的黑盒测试结果，让插件继续生成 Newman / Postman 执行资产、检查环境，并在可执行时生成测试报告。

### 3.2 本目录材料与插件的关系

本目录中的文件是基于以下流程形成的：

1. 使用 `blackbox-testing` 从需求中生成黑盒测试用例；
2. 使用 `blackbox-execution` 将测试用例转成 Newman 可执行资产；
3. 对 RealWorld API 目标执行测试；
4. 保留可审阅的执行资产副本、执行摘要和测试报告。

## 5. 说明

当前 `member3` 保留的交付物聚焦于 RealWorld API，但该 skill 在多类项目上都有良好的适配表现。
