using ProductManagementApp.Business.Interfaces;
using ProductManagementApp.Business.Services;
using ProductManagementApp.Components;
using ProductManagementApp.DataAccessLayer.Interfaces;
using ProductManagementApp.DataAccessLayer.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// The container maps IProductRepository to ProductRepositoryJson.
// Callers inject the interface; they never construct the JSON class themselves.
builder.Services.AddSingleton<IProductRepository, ProductRepositoryJson>();

// Swap only this line to prove DIP / Open-Closed:
// builder.Services.AddSingleton<IProductRepository, ProductRepositoryFake>();

builder.Services.AddScoped<IProductService, ProductService>();

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
