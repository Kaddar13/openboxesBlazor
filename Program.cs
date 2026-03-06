using OpenBoxesMobile.Blazor.Components;
using OpenBoxesMobile.Blazor.Options;
using OpenBoxesMobile.Blazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<OpenBoxesApiOptions>(builder.Configuration.GetSection("OpenBoxesApi"));

builder.Services.AddScoped<AppState>();
builder.Services.AddScoped<OpenBoxesApiClient>();
builder.Services.AddScoped<SessionManager>();
builder.Services.AddScoped<UiSettingsService>();
builder.Services.AddScoped<SortationFlowState>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
