var builder = DistributedApplication.CreateBuilder(args);

var rabbitmq = builder.AddContainer("rabbitmq", "rabbitmq", "3-management")
    .WithEnvironment("RABBITMQ_DEFAULT_USER", "guest")
    .WithEnvironment("RABBITMQ_DEFAULT_PASS", "guest")
    .WithEnvironment("RABBITMQ_SERVER_ADDITIONAL_ERL_ARGS", "-rabbit loopback_users []")
    .WithEndpoint(port: 5672, targetPort: 5672, name: "amqp")
    .WithHttpEndpoint(port: 15672, targetPort: 15672, name: "management");

var redis = builder.AddRedis("redis")
    .WithRedisInsight();

var mailhog = builder.AddContainer("mailhog", "mailhog/mailhog")
    .WithHttpEndpoint(port: 8025, targetPort: 8025, name: "http")
    .WithEndpoint(port: 1025, targetPort: 1025, name: "smtp");

var identity = builder.AddProject(
        name: "identity",
        projectPath: @"..\src\Infrastructure\CleanCut.Infrastructure.Identity\CleanCut.Infrastructure.Identity.csproj")
    .WithExternalHttpEndpoints()
    .WithReference(redis);

var api = builder.AddProject(
        name: "api",
        projectPath: @"..\src\Presentation\CleanCut.API\CleanCut.API.csproj")
    .WithExternalHttpEndpoints()
    .WithReference(redis);

builder.AddProject(
        name: "productexport",
        projectPath: @"..\src\Applications\CleanCut.ProductExportHost\CleanCut.ProductExportHost.csproj")
    .WithReference(redis);

builder.AddProject(
        name: "blazorwebapp",
        projectPath: @"..\src\Presentation\CleanCut.BlazorWebApp\CleanCut.BlazorWebApp.csproj")
    .WithExternalHttpEndpoints()
    .WithReference(identity)
    .WithReference(api)
    .WithReference(redis);

builder.Build().Run();
