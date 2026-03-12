using LiveCommerce.Infrastructure;
using LiveCommerce.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<CommentConsumerWorker>();

var host = builder.Build();
host.Run();
