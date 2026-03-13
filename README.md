# SSO

## 当前阶段

当前仓库已进入阶段 5，在前几个阶段的基础上，开始接入 ASP.NET Core Identity 基础。

本阶段目标：

- 保持 Domain / Contracts / Application 内核稳定
- 在 Infrastructure 中接入 ASP.NET Core Identity 基础实体与管理器
- 将 Identity 与 EF Core 持久化统一到同一个 DbContext
- 继续通过测试固定分层边界、配置校验、持久化与身份基础行为

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
- Infrastructure：时间服务、当前用户上下文、基础选项绑定与依赖注入入口
- Infrastructure：`MySsoDbContext`、实体映射、EF Core 仓储、设计时工厂
- Infrastructure：`SsoIdentityUser`、`SsoIdentityRole` 与 Identity 管理器注册
- Tests：覆盖邮箱校验、客户端安全约束、会话撤销规则、应用层处理器行为、基础设施注入、仓储基础行为与 Identity 管理器解析

## 后续阶段

后续将按增量方式推进：

1. 集成 OpenIddict
2. 实现认证核心
3. 完善审计、会话、后台与客户端接入
4. 完成联调、加固与全链路验收

