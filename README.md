# SSO

## 当前阶段

当前仓库已进入阶段 8，在前几个阶段的基础上，开始补齐用户中心与后台只读管理面。

本阶段目标：

- 保持 Domain / Contracts / Application 内核稳定
- 保持认证、持久化与协议基础稳定
- 建立 Application 只读查询接口与 Infrastructure 查询实现
- 为 Web 宿主提供用户中心与后台只读管理页
- 继续通过测试固定分层边界、配置校验、管理查询与宿主集成行为

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
- Infrastructure：OpenIddict Core/Server 注册与协议基础配置
- Application：后台/用户中心查询接口
- Web：登录页、授权端点、用户中心与后台只读管理页面
- Tests：覆盖邮箱校验、客户端安全约束、会话撤销规则、应用层处理器行为、基础设施注入、仓储基础行为、Identity 管理器解析、OpenIddict 服务注册与查询层行为

## 后续阶段

后续将按增量方式推进：

1. 完成认证核心剩余交互
2. 完善审计、会话、后台与客户端接入
3. 完成联调、加固与全链路验收

