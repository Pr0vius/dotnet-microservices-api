using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
using ms.rabbitmq.Consumer;
using ms.rabbitmq.Middlewares;
using ms.user.api.Consumers;
using ms.user.api.Middlewares;
using ms.user.application.Queries;
using ms.user.domain.Interfaces;
using ms.user.infrastructure.Data;
using ms.user.infrastructure.Repositories;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    logger.Info("Starting Users Microservice...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    builder.Services.AddSingleton<IConsumer, UsersConsumer>();

    builder.Services.AddDbContext<UserDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")),
        ServiceLifetime.Transient);

    builder.Services.AddTransient<IUserRepository, UserRepository>();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssemblies(typeof(GetAllUsersQuery).Assembly);
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        logger.Info("App in dev mode");
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<UserDbContext>();
            context.Database.Migrate();
            logger.Info("Database migration completed succesfully");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error applying migrations");
        }
    }

    app.UseMiddleware<RequestLoggingMiddleware>();
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.UseRabbitConsumer();

    app.UseAuthorization();

    app.MapControllers();

    logger.Info("Application started sucessfully");
    app.Run();
}
catch (Exception ex)
{
    logger.Fatal(ex, "Critical Error on application startup");
    throw;
}
finally
{
    LogManager.Shutdown();
}
