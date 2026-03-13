# SSO

## 当前阶段

当前仓库已完成阶段 1 的初始化骨架，用于后续按阶段实现基于 .NET 10 的单点登录系统。

本阶段目标：

- 建立 Clean Architecture 解决方案结构
- 建立最小项目引用关系
- 保持 Web、Api、示例客户端可启动
- 为后续 Identity、OpenIddict、PostgreSQL 接入预留清晰边界

本阶段不包含：

- 数据库接入
- EF Core DbContext
- ASP.NET Core Identity
- OpenIddict
- 注册、登录、注销
- 审计、会话、后台管理

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
- Contracts：跨项目共享契约
- Application：用例、接口、DTO 与应用层协调
- Infrastructure：技术实现层，后续承接 EF Core、Identity、OpenIddict、PostgreSQL
- Web：认证站点与授权交互宿主
- Api：受保护资源服务器
- Sample.ClientWeb：独立示例 Web 客户端

## 后续阶段

后续将按增量方式推进：

1. 建立 Domain / Contracts / Application 核心骨架
2. 接入 Infrastructure 基础能力
3. 集成 OpenIddict
4. 实现认证核心
5. 完善审计、会话、后台与客户端接入

