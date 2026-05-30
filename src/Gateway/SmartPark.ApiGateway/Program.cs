var builder = WebApplication.CreateBuilder(args);

// 网关复用 Aspire 默认能力，便于统一暴露健康检查、观测能力和异常响应格式。
builder.AddServiceDefaults();

// 前端以独立开发服务器运行，开发期直接放开跨域限制便于联调。
builder.Services.AddCors(options =>
{
    options.AddPolicy("default", policy =>
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
});

// YARP 根据配置把请求转发到各个后端微服务。
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// 网关也统一使用全局异常处理中间件，保证转发前后的错误结构一致。
app.UseSmartParkExceptionHandling();
app.UseCors("default");
app.MapGet("/", () => Results.Ok(new { name = "SmartPark Gateway", version = "1.0.0" }));
app.MapReverseProxy();
app.MapDefaultEndpoints();
app.Run();
