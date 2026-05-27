var builder = DistributedApplication.CreateBuilder(args);

// AppHost 负责在本地开发期统一编排网关与各个微服务。
builder.AddProject<Projects.SmartPark_Identity_Api>("identity-api")
    .WithEnvironment("ASPNETCORE_URLS", "http://localhost:5101");

builder.AddProject<Projects.SmartPark_Asset_Api>("asset-api")
    .WithEnvironment("ASPNETCORE_URLS", "http://localhost:5102");

builder.AddProject<Projects.SmartPark_IoTPlatform_Api>("iot-api")
    .WithEnvironment("ASPNETCORE_URLS", "http://localhost:5103");

builder.AddProject<Projects.SmartPark_WorkOrder_Api>("workorder-api")
    .WithEnvironment("ASPNETCORE_URLS", "http://localhost:5104");

builder.AddProject<Projects.SmartPark_Collaboration_Api>("collaboration-api")
    .WithEnvironment("ASPNETCORE_URLS", "http://localhost:5105");

builder.AddProject<Projects.SmartPark_ApiGateway>("api-gateway")
    .WithEnvironment("ASPNETCORE_URLS", "http://localhost:5100");

builder.Build().Run();
