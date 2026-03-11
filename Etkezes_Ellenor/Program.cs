using Etkezes_Ellenor.Components;
using Etkezes_Ellenor.Data;
using Etkezes_Ellenor.Services;

using FingerPrintService;

using Microsoft.AspNetCore;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<EtkezesDBcontext>( ServiceLifetime.Singleton);

builder.Services.AddHttpClient();
ZkfpNative.Initialize();
builder.Services.AddFluentUIComponents();
builder.Services.AddSingleton<IFPService, FPService>();
builder.Services.AddHostedService<BgService>();
builder.Services.AddScoped<LoginUserService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<DataService>();
builder.Services.AddSingleton<ApiHelper>();
builder.Services.AddSingleton<SyncService>();
builder.WebHost.ConfigureKestrel(options =>
{
    //options.Listen(System.Net.IPAddress.Parse("192.168.10.90"), 5001);
    options.ListenAnyIP(5001);
    options.ListenLocalhost(5000);
});
var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MigrationDb();

app.Run();
