global using auth_sevice.Src.Utils;
using auth_sevice.Src.Data;
using auth_sevice.Src.Services;
using Microsoft.EntityFrameworkCore;

DotEnv.Init();
ServerInfo.Init();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DataContext>(options =>
{
  options.UseNpgsql(ServerInfo.DB_CONNECT);
}, ServiceLifetime.Transient);

// SERVICES DEPENDENCY INJECTION
builder.Services.AddScoped<ITokenManager, TokenManager>();
builder.Services.AddScoped<IBlacklistTokenManager, BlacklistTokenManager>();
builder.Services.AddHttpContextAccessor();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
  var services = scope.ServiceProvider;

  var context = services.GetRequiredService<DataContext>();
  if (context.Database.GetPendingMigrations().Any())
  {
    context.Database.Migrate();
  }
}

app.Run();
