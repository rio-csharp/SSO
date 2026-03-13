# SSO

## 当前阶段

当前仓库已进入阶段 2，在阶段 1 解决方案骨架基础上，继续建立 SSO 系统的核心内核。

本阶段目标：

- 建立 Domain 核心实体、值对象与安全约束
- 建立 Contracts 共享结果模型、分页模型与核心读取模型
- 建立 Application 端口接口、异常模型与最小用例处理器
- 通过单元测试固定关键安全规则与分层边界

本阶段不包含：

- 数据库接入
- EF Core DbContext
- ASP.NET Core Identity
- OpenIddict
- 实际令牌签发与验证
- Web / Api 宿主中的业务界面与控制器实现

## 解决方案结构

```text
SSO.slnx
src/
	MySso.Domain/
	MySso.Contracts/
	MySso.Application/
	MySso.Infrastructure/
	MySso.Web/
	MySso.Api/
samples/
	MySso.Sample.ClientWeb/
tests/
	MySso.Domain.Tests/
	MySso.Application.Tests/
	MySso.IntegrationTests/
```

## 分层边界

- Domain：领域模型与基础规则，不依赖框架
- Contracts：跨项目共享契约与读取模型
- Application：用例、接口、DTO 与应用层协调，不依赖宿主与基础设施实现
- Infrastructure：技术实现层，后续承接 EF Core、Identity、OpenIddict、PostgreSQL
- Web：认证站点与授权交互宿主
- Api：受保护资源服务器
- Sample.ClientWeb：独立示例 Web 客户端

## 当前内核内容

- Domain：`IdentityUser`、`Role`、`RegisteredClient`、`UserSession`、`AuditLog`
- Domain：`EmailAddress`、`PersonName` 等值对象，以及 PKCE / 重定向地址 / 会话撤销等基础安全约束
- Contracts：统一操作结果、分页模型、用户 / 角色 / 客户端 / 会话 / 审计读取模型
- Application：仓储端口、时间与当前用户上下文端口、最小用例处理器
- Tests：覆盖邮箱校验、客户端安全约束、会话撤销规则、应用层处理器行为

## 后续阶段

后续将按增量方式推进：

1. 接入 Infrastructure 基础能力
2. 集成 OpenIddict 与身份系统
3. 实现认证核心
4. 完善审计、会话、后台与客户端接入
5. 完成联调、加固与全链路验收

