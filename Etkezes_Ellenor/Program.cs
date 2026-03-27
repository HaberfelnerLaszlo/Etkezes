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
//ZkfpNative.Initialize();
builder.Services.AddFluentUIComponents();
if (OperatingSystem.IsLinux())
{
  builder.Services.AddSingleton<IFPService, FPServiceLinux>();
}
else
{
    builder.Services.AddSingleton<IFPService, FPServiceWindows>();
}
builder.Services.AddHostedService<BgService>();
builder.Services.AddSingleton<LoginUserService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddScoped<DataService>();
builder.Services.AddSingleton<ApiHelper>();
builder.Services.AddSingleton<EtkezesService>();
builder.Services.AddSingleton<SyncService>();
//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.ListenAnyIP(6001);
//    options.ListenLocalhost(6000);
//});
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
