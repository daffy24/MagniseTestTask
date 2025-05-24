using MagniseTestTask.DatabaseContext;
using MagniseTestTask.Interfaces;
using MagniseTestTask.Repositories;
using MagniseTestTask.Services;
using MagniseTestTask.Token;
using MagniseTestTask.Validators;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IAssetRepository, AssetRepository>();
builder.Services.AddScoped<IWebSocketValidator, WebSocketStartValidator>();

builder.Services.AddSingleton<ITokenManager, TokenManager>();
builder.Services.AddSingleton<IWebSocketService, WebSocketService>();

builder.Services.AddHostedService<AssetBackgroundService>();    

    
builder.Services.AddControllers();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();

app.UseAuthorization();

app.Run();
