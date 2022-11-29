global using auth_sevice.src.Utils;
using auth_sevice.src.Data;
using auth_sevice.src.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
// using StackExchange.Redis;
using Enyim.Caching.Configuration;

DotEnv.Init();
ServerInfo.Init();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
  option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
  option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    In = ParameterLocation.Header,
    Description = "Please enter a valid token",
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    BearerFormat = "JWT",
    Scheme = "Bearer"
  });
  option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

// SERVICES DEPENDENCY INJECTION
builder.Services.AddDbContext<DataContext>(options =>
{
  options.UseNpgsql(ServerInfo.DB_CONNECT);
}, ServiceLifetime.Transient);

// REDIS
// builder.Services.AddSingleton<IConnectionMultiplexer>(
//   ConnectionMultiplexer.Connect(ServerInfo.REDIS_CONNECT)
// );

// MEMCACHED
builder.Services.AddEnyimMemcached(o => o.Servers = new List<Server>{
  new Server{
    Address = Environment.GetEnvironmentVariable("MEMCACHED_HOST"),
    Port  = int.Parse(Environment.GetEnvironmentVariable("MEMCACHED_PORT")!)
  }
});

builder.Services.AddScoped<ITokenManager, TokenManager>();
// builder.Services.AddScoped<IBlacklistTokenManager, BlacklistTokenManager>();
// builder.Services.AddSingleton<IBlacklistTokenManager, RedisBlacklistTokenManager>();
builder.Services.AddSingleton<IBlacklistTokenManager, MemcBlacklistTokenManager>();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.MapControllers();

app.UseCors(corsPolicyBuilder => corsPolicyBuilder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader());

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
