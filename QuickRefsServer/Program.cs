using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using QuickRefsServer.Models;

var builder = WebApplication.CreateBuilder(args);
string PostgresConnStr = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");
if(PostgresConnStr == null)
    PostgresConnStr = "Host=localhost;Port=5432;UserId=quickrefs;Password=quickrefs;Database=quickrefs";

builder.Services.AddControllers();
builder.Services.AddDbContext<QuickRefsDbContext>(opt =>
{
    opt.UseNpgsql(PostgresConnStr);
});


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.WithOrigins("http://localhost:3000")
                .AllowAnyMethod()
                .AllowAnyHeader();
            builder.WithOrigins("http://192.168.2.54:3000")
                .AllowAnyMethod()
                .AllowAnyHeader();
            builder.WithOrigins("http://nodejs.svr:3000")
                .AllowAnyMethod()
                .AllowAnyHeader();
            builder.WithOrigins("https://quickrefs.netlify.app")
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

builder.Services.AddStackExchangeRedisCache(opt =>
{
    opt.Configuration = Environment.GetEnvironmentVariable("REDIS_CONNECTION");
});

builder.Services.Add(ServiceDescriptor.Singleton<IDistributedCache, RedisCache>());

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
