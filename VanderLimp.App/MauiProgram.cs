using Microsoft.Extensions.Logging;
using VanderLimp.App.Servicos;

namespace VanderLimp.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont(
                    "OpenSans-Regular.ttf",
                    "OpenSansRegular");

                fonts.AddFont(
                    "OpenSans-Semibold.ttf",
                    "OpenSansSemibold");
            });

        // Endereço usado neste estágio de testes no emulador Android.
        builder.Services.AddSingleton<HttpClient>(_ => new HttpClient
        {
            BaseAddress = new Uri(DeviceInfo.Current.DeviceType == DeviceType.Virtual
				? "http://10.0.2.2:5176/"
				: "http://127.0.0.1:5176/"),
            Timeout = TimeSpan.FromSeconds(30)
        });

        builder.Services.AddTransient<ClienteApiServico>();
		builder.Services.AddTransient<AutenticacaoApiServico>();
		builder.Services.AddSingleton<SessaoLocalServico>();
		builder.Services.AddTransient<ProdutoApiServico>();
		builder.Services.AddTransient<VanderLimp.App.Views.ProdutosPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
builder.Services.AddTransient<EnderecoEntregaApiServico>();

        return builder.Build();
    }
}