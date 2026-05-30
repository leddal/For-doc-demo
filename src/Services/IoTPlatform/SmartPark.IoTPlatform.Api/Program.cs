using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SmartPark.IoTPlatform.Api;
using SmartPark.IoTPlatform.Application;
using SmartPark.IoTPlatform.Infrastructure;
using SmartPark.SharedContracts.Auth;

var builder = WebApplication.CreateBuilder(args);

// 接入 Aspire 默认能力，便于本地统一接入健康检查与观测能力。
builder.AddServiceDefaults();

// 暴露 OpenAPI，便于调试监测点、告警和灌溉控制接口。
builder.Services.AddOpenApi();
builder.Services.AddAuthorization();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

// 物联服务信任身份服务签发的 JWT，用于保护监测与控制接口。
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

builder.Services.AddIoTPlatformApplication();
builder.Services.AddIoTPlatformInfrastructure(builder.Configuration);

var app = builder.Build();

// 统一异常处理中间件让监测、告警和控制接口都输出稳定错误结构。
app.UseSmartParkExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapIoTEndpoints();
app.MapDefaultEndpoints();

// 启动时自动准备默认监测点和告警样例，便于验证闭环链路。
await app.UseIoTPlatformInfrastructureAsync();
app.Run();
