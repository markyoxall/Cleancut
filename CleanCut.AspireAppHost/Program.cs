var builder = DistributedApplication.CreateBuilder(args);

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin()
    .WithExternalHttpEndpoints();

var redis = builder.AddRedis("redis")
    .WithRedisInsight();

var identity = builder.AddProject(
        name: "identity",
        projectPath: @"..\src\Infrastructure\CleanCut.Infrastructure.Identity\CleanCut.Infrastructure.Identity.csproj")
    .WithExternalHttpEndpoints();

var api = builder.AddProject(
        name: "api",
        projectPath: @"..\src\Presentation\CleanCut.API\CleanCut.API.csproj")
    .WithExternalHttpEndpoints();

builder.AddProject(
        name: "productexport",
        projectPath: @"..\src\Applications\CleanCut.ProductExportHost\CleanCut.ProductExportHost.csproj")
    .WithReference(rabbitmq)
    .WithReference(redis);

builder.AddProject(
        name: "blazorwebapp",
        projectPath: @"..\src\Presentation\CleanCut.BlazorWebApp\CleanCut.BlazorWebApp.csproj")
    .WithExternalHttpEndpoints()
    .WithReference(identity)
    .WithReference(api)
    .WithReference(redis)
    .WithReference(rabbitmq);

builder.Build().Run();
