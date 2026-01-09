var builder = DistributedApplication.CreateBuilder(args);

// RabbitMQ with persistence for queues and messages
var rabbitmq = builder.AddContainer("rabbitmq", "rabbitmq", "3-management")
    .WithEnvironment("RABBITMQ_DEFAULT_USER", "guest")
    .WithEnvironment("RABBITMQ_DEFAULT_PASS", "guest")
    .WithEnvironment("RABBITMQ_SERVER_ADDITIONAL_ERL_ARGS", "-rabbit loopback_users []")
    .WithEndpoint(port: 5672, targetPort: 5672, name: "amqp")
    .WithHttpEndpoint(port: 15672, targetPort: 15672, name: "management")
    .WithBindMount("rabbitmq-data", "/var/lib/rabbitmq");

// Plain Redis container on standard port 6379 - no Aspire magic, just simple Redis
// Redis data persisted to survive container restarts
var redis = builder.AddContainer("redis", "redis", "latest")
    .WithEndpoint(port: 6379, targetPort: 6379, name: "tcp")
    .WithBindMount("redis-data", "/data");

// Add RedisInsight for GUI management of Redis
// Persist RedisInsight data (EULA acceptance, saved connections) across container restarts
builder.AddContainer("redis-insight", "redis/redisinsight", "latest")
    .WithHttpEndpoint(port: 5540, targetPort: 5540, name: "http")
    .WithBindMount("redis-insight-data", "/data");

// MailHog with persistence for captured emails
var mailhog = builder.AddContainer("mailhog", "mailhog/mailhog")
    .WithHttpEndpoint(port: 8025, targetPort: 8025, name: "http")
    .WithEndpoint(port: 1025, targetPort: 1025, name: "smtp")
    .WithBindMount("mailhog-data", "/maildir");

// Remove .WithReference(redis) from all projects - they'll use appsettings.json instead
var identity = builder.AddProject(
        name: "identity",
        projectPath: @"..\src\Infrastructure\CleanCut.Infrastructure.Identity\CleanCut.Infrastructure.Identity.csproj")
    .WithExternalHttpEndpoints();

var api = builder.AddProject(
        name: "api",
        projectPath: @"..\src\Presentation\CleanCut.API\CleanCut.API.csproj")
    .WithExternalHttpEndpoints();

builder.AddProject(
        name: "orderprocessing",
        projectPath: @"..\src\Applications\CleanCut.OrderProcessingHost\CleanCut.OrderProcessingHost.csproj")
    .WaitFor(rabbitmq);  // Wait for RabbitMQ to be ready before starting

builder.AddProject(
        name: "blazorwebapp",
        projectPath: @"..\src\Presentation\CleanCut.BlazorWebApp\CleanCut.BlazorWebApp.csproj")
    .WithExternalHttpEndpoints()
    .WithReference(identity)
    .WithReference(api);

builder.Build().Run();
