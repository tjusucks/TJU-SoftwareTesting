# member3 说明文档

## 1. 当前目录下各文件和子文件夹的作用

### 1.1 根目录文件

- `generated-blackbox-output-v1.md`
  - member3 的黑盒测试用例输入文件。
  - 当前实际执行与保留的交付物聚焦于 RealWorld API 相关测试。

- `execution-summary.md`
  - 测试资产生成与执行方式的摘要说明。
  - 记录了输入校验、环境检查和 API 测试映射策略。

- `test-report.md`
  - 实际执行后的测试报告。
  - 记录了 RealWorld API 测试的通过/失败情况，以及当前基线实现中的已发现问题。

### 1.2 `newman/` 子文件夹

该目录存放 RealWorld API 测试相关资产。

- `newman/api-generated.postman_collection.json`
  - API 测试的主 collection 文件。
  - 由黑盒测试用例转换而来，包含请求、断言和必要的变量传递逻辑。

- `newman/api-generated.environment.json`
  - Newman 运行时使用的环境变量文件。
  - 包括 `APIURL`、测试用户信息、token、slug、commentId 等变量。

- `newman/run-api-tests.sh`
  - API 一键测试脚本。
  - 会调用 Newman 执行 `api-generated.postman_collection.json`，并输出结果到 `api-run.json`。

- `newman/api-run.json`
  - API 测试的原始执行结果文件。
  - 可用于后续分析或复核断言结果。

### 1.3 `report-materials/` 子文件夹

该目录存放为提交与老师审阅准备的精简材料。

- `newman/api-generated.postman_collection.json`
  - API 测试的主 collection 文件。
  - 由黑盒测试用例转换而来，包含请求、断言和必要的变量传递逻辑。

- `newman/api-generated.environment.json`
  - Newman 运行时使用的环境变量文件。
  - 包括 `APIURL`、测试用户信息、token、slug、commentId 等变量。

- `newman/run-api-tests.sh`
  - API 一键测试脚本。
  - 会调用 Newman 执行 `api-generated.postman_collection.json`，并输出结果到 `api-run.json`。

- `newman/api-run.json`
  - API 测试的原始执行结果文件。
  - 可用于后续分析或复核断言结果。

- `report-materials/execution-summary.md`
  - 执行摘要的审阅副本。

- `report-materials/test-report.md`
  - 测试报告的审阅副本。

---

## 2. 如何搭建测试环境，以及如何通过 sh 脚本一键测试

### 2.1 环境准备

建议在仓库根目录先完成以下准备工作：

#### 第一步：初始化 submodule

RealWorld 实现代码位于 `codebases/realworld/`，如果仓库是首次 clone，需要先初始化子模块：

```bash
git submodule update --init --recursive
```

#### 第二步：准备基础环境

需要具备以下环境：

- Node.js
- Newman
- Go

已验证通过的关键工具包括：

- `node`
- `newman`

### 2.2 启动 RealWorld 后端（API 测试前必须完成）

API 测试目标为：

```text
http://localhost:8080/api
```

需要先启动 RealWorld 的 `golang-gin` 后端。推荐步骤如下：

```bash
cd "../../codebases/realworld/implementations/golang-gin"
cp .env.example .env
```

然后根据本地环境修改 `.env` 中数据库等配置，再启动服务：

```bash
go run hello.go
```

启动成功后，后端默认监听 `http://localhost:8080`。

### 2.3 一键运行 API 测试

回到 `docs/member3/` 目录，执行：

```bash
bash ./newman/run-api-tests.sh
```

如需显式指定接口地址，可使用：

```bash
APIURL=http://localhost:8080/api bash ./newman/run-api-tests.sh
```

执行结果主要输出到：

- `newman/api-run.json`
- `test-report.md`

### 2.4 推荐的完整测试顺序

建议按以下顺序执行：

1. 在仓库根目录初始化 submodule。
2. 启动 `codebases/realworld/implementations/golang-gin` 后端。
3. 在 `docs/member3/` 执行 API 测试：
   ```bash
   bash ./newman/run-api-tests.sh
   ```

---

## 3. 当前保留结果的说明

当前 `member3` 目录只保留与 RealWorld API 相关的交付物。

保留原因是：
- 仓库 `codebases/` 中并没有 TodoMVC 项目本体；
- 因此此前为执行 UI 测试而额外补加的 TodoMVC 本体代码、UI 测试代码和运行资产已移除；
- 当前目录中的有效交付物聚焦于 RealWorld API 的黑盒测试生成、执行与审阅材料。

## 4. 简短总结

member3 当前已经完成了 RealWorld API 测试资产生成、Newman 执行脚本编写、测试运行和审阅材料整理。

这套 skill 在多类项目上都有良好的适配表现，但当前仓库的 codebase 只保留了 RealWorld 相关交付物。
