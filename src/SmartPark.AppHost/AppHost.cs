var builder = DistributedApplication.CreateBuilder(args);

// AppHost 负责在本地开发期统一编排网关与各个微服务。
builder.AddProject<Projects.SmartPark_Identity_Api>("identity-api")
    .WithHttpEndpoint(targetPort: 5101, port: 5101, env: "ASPNETCORE_HTTP_PORTS", isProxied: false);

builder.AddProject<Projects.SmartPark_Asset_Api>("asset-api")
    .WithHttpEndpoint(targetPort: 5102, port: 5102, env: "ASPNETCORE_HTTP_PORTS", isProxied: false);

builder.AddProject<Projects.SmartPark_IoTPlatform_Api>("iot-api")
    .WithHttpEndpoint(targetPort: 5103, port: 5103, env: "ASPNETCORE_HTTP_PORTS", isProxied: false);

builder.AddProject<Projects.SmartPark_WorkOrder_Api>("workorder-api")
    .WithHttpEndpoint(targetPort: 5104, port: 5104, env: "ASPNETCORE_HTTP_PORTS", isProxied: false);

builder.AddProject<Projects.SmartPark_Collaboration_Api>("collaboration-api")
    .WithHttpEndpoint(targetPort: 5105, port: 5105, env: "ASPNETCORE_HTTP_PORTS", isProxied: false);

builder.AddProject<Projects.SmartPark_ApiGateway>("api-gateway")
    .WithHttpEndpoint(targetPort: 5100, port: 5100, env: "ASPNETCORE_HTTP_PORTS", isProxied: false);

builder.Build().Run();
