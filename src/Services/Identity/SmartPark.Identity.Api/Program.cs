using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SmartPark.Identity.Api;
using SmartPark.Identity.Application;
using SmartPark.Identity.Infrastructure;
using SmartPark.SharedContracts.Auth;

var builder = WebApplication.CreateBuilder(args);

// 接入 Aspire 默认能力，保持所有服务的启动约定一致。
builder.AddServiceDefaults();

// 暴露 OpenAPI，便于调试登录与用户查询接口。
builder.Services.AddOpenApi();
builder.Services.AddAuthorization();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

// 身份服务负责签发 JWT，其他服务也复用同一套签名规则进行校验。
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey))
        };
    });

builder.Services.AddIdentityApplication();
builder.Services.AddIdentityInfrastructure(builder.Configuration);

var app = builder.Build();

// 身份服务同样使用统一异常处理中间件，保证登录失败和系统失败能被明确区分。
app.UseSmartParkExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapIdentityEndpoints();
app.MapDefaultEndpoints();

// 启动时自动完成身份库初始化与种子账号注入。
await app.UseIdentityInfrastructureAsync();
app.Run();
