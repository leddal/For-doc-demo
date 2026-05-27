using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SmartPark.Asset.Api;
using SmartPark.Asset.Application;
using SmartPark.Asset.Infrastructure;
using SmartPark.SharedContracts.Auth;

var builder = WebApplication.CreateBuilder(args);

// 接入 Aspire 默认能力，保持资产服务与其他服务的启动约定一致。
builder.AddServiceDefaults();

// 暴露 OpenAPI，便于调试资产建档、查询和更新接口。
builder.Services.AddOpenApi();
builder.Services.AddAuthorization();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

// 资产服务只做令牌校验，用户身份由 Identity 服务统一签发。
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

builder.Services.AddAssetApplication();
builder.Services.AddAssetInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapAssetEndpoints();
app.MapDefaultEndpoints();

// 启动时自动准备示例资产数据，便于联调工单和协同场景。
await app.UseAssetInfrastructureAsync();
app.Run();
