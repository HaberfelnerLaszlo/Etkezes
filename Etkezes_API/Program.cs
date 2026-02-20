using Microsoft.EntityFrameworkCore;
using Etkezes_API.Data;
using Etkezes_API.Services;
using Etkezes_API.Endpoints;
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<EtkezesDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));
builder.Services.AddScoped<LoginUserService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<EtkezesService>();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapRootEndpoints();
app.MapLoginUserEndpoints();
app.MapUserEndpoints();
app.MapEtkezesEndpoint();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
