using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SmartPark.Collaboration.Api;
using SmartPark.Collaboration.Application;
using SmartPark.Collaboration.Infrastructure;
using SmartPark.SharedContracts.Auth;

var builder = WebApplication.CreateBuilder(args);

// 接入 Aspire 默认能力，保持协同服务与其他服务的运行约定一致。
builder.AddServiceDefaults();

// 暴露 OpenAPI，便于调试事件台账、公告和事件转工单接口。
builder.Services.AddOpenApi();
builder.Services.AddAuthorization();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

// 协同服务校验统一 JWT，并据此控制事件与公告的操作权限。
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

builder.Services.AddCollaborationApplication();
builder.Services.AddCollaborationInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapCollaborationEndpoints();
app.MapDefaultEndpoints();

// 启动时自动准备事件和公告种子数据，便于直接演示协同闭环。
await app.UseCollaborationInfrastructureAsync();
app.Run();
