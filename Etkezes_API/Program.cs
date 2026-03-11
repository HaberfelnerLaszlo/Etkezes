using Microsoft.EntityFrameworkCore;
using Etkezes_API.Data;
using Etkezes_API.Services;
using Etkezes_API.Endpoints;
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddValidation();
builder.Services.AddDbContext<EtkezesDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));
builder.Services.AddScoped<LoginUserService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<EtkezesService>();
builder.Services.AddScoped<SyncService>();
var app = builder.Build();

app.MigrationDb();
app.MapDefaultEndpoints();
app.MapRootEndpoints();
app.MapLoginUserEndpoints();
app.MapUserEndpoints();
app.MapEtkezesEndpoint();
app.MapSyncEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
