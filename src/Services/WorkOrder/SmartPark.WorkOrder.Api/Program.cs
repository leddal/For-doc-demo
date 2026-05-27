using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SmartPark.SharedContracts.Auth;
using SmartPark.WorkOrder.Api;
using SmartPark.WorkOrder.Application;
using SmartPark.WorkOrder.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 接入 Aspire 默认能力，保持工单服务与其他服务的启动方式一致。
builder.AddServiceDefaults();

// 暴露 OpenAPI，便于直接调试工单流转接口。
builder.Services.AddOpenApi();
builder.Services.AddAuthorization();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

// 工单服务只校验 JWT，令牌统一由身份服务签发。
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

builder.Services.AddWorkOrderApplication();
builder.Services.AddWorkOrderInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapWorkOrderEndpoints();
app.MapDefaultEndpoints();

// 启动时自动初始化工单库与示例数据，方便本地验证闭环流程。
await app.UseWorkOrderInfrastructureAsync();
app.Run();
