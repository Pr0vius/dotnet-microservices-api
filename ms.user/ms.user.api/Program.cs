using Microsoft.EntityFrameworkCore;
using ms.rabbitmq.Consumer;
using ms.rabbitmq.Middlewares;
using ms.user.api.Consumers;
using ms.user.api.Middlewares;
using ms.user.application.Queries;
using ms.user.domain.Interfaces;
using ms.user.infrastructure.Data;
using ms.user.infrastructure.Repositories;
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<IConsumer, UsersConsumer>();

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Transient);

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
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error appliying the migrations");
    }
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseRabbitConsumer();

app.UseAuthorization();

app.MapControllers();

app.Run();
