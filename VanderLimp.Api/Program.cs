using VanderLimp.Api.Servicos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<SenhaServico>();
builder.Services.AddScoped<SessaoServico>();
builder.Services.AddScoped<ProdutoServico>();
builder.Services.AddScoped<ClienteAdminServico>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();