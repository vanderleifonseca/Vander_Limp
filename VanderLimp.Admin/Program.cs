using VanderLimp.Admin.Components;
using VanderLimp.Admin.Servicos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<AutenticacaoAdminApiServico>(cliente =>
{
    cliente.BaseAddress = new Uri("http://localhost:5176/");
    cliente.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<SessaoAdminServico>();


builder.Services.AddHttpClient<ProdutoAdminApiServico>(cliente =>
{
    cliente.BaseAddress = new Uri("http://localhost:5176/");
    cliente.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<ClienteAdminApiServico>(cliente =>
{
    cliente.BaseAddress = new Uri("http://localhost:5176/");
    cliente.Timeout = TimeSpan.FromSeconds(30);
});


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute(
    "/not-found",
    createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();