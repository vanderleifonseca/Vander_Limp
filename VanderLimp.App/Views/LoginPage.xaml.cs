using Microsoft.Extensions.DependencyInjection;
using VanderLimp.App.Dados;
using VanderLimp.App.Servicos;

namespace VanderLimp.App.Views;

public partial class LoginPage : ContentPage
{
    private bool _entrando;
    private bool _temTokenSalvo;

    public LoginPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await EntrarAsync(somenteSessaoSalva: true);
    }

    private async void OnEntrarClicked(object? sender, EventArgs e)
    {
        await EntrarAsync(somenteSessaoSalva: false);
    }

    private async Task EntrarAsync(bool somenteSessaoSalva)
    {
        if (_entrando)
            return;

        _entrando = true;

        MensagemLabel.IsVisible = false;
        EntrarButton.IsEnabled = false;
        CarregandoIndicator.IsVisible = true;
        CarregandoIndicator.IsRunning = true;

        try
        {
            var servicos =
                Application.Current?.Handler?.MauiContext?.Services;

            var autenticacao = servicos?
                .GetService<AutenticacaoApiServico>();

            var sessaoLocal = servicos?
                .GetService<SessaoLocalServico>();

            if (autenticacao is null || sessaoLocal is null)
            {
                MostrarErro("O serviço de login não está disponível.");
                return;
            }

            string? token = await sessaoLocal.ObterTokenAsync();

            _temTokenSalvo = !string.IsNullOrWhiteSpace(token);

            if (_temTokenSalvo)
            {
                EmailEntry.IsVisible = false;
                SenhaEntry.IsVisible = false;
                SenhaEntry.Text = string.Empty;

                var sessao =
                    await autenticacao.ConsultarSessaoAsync(token!);

                if (sessao is null)
                {
                    sessaoLocal.EncerrarSessaoLocal();
                    _temTokenSalvo = false;

                    MostrarErro(
                        "A sessão foi encerrada ou expirou. " +
                        "Entre uma vez para salvar uma nova sessão.");

                    return;
                }

                await AbrirPedidosAsync(sessao.Nome);
                return;
            }

            if (somenteSessaoSalva)
                return;

            if (string.IsNullOrWhiteSpace(EmailEntry.Text) ||
                string.IsNullOrWhiteSpace(SenhaEntry.Text))
            {
                MostrarErro("Informe o e-mail e a senha.");
                return;
            }

            var dados = new LoginDados
            {
                Email = EmailEntry.Text.Trim(),
                Senha = SenhaEntry.Text ?? string.Empty
            };

            try
            {
                var resultado = await autenticacao.LoginAsync(dados);

                await sessaoLocal.SalvarTokenAsync(resultado.Token);

                _temTokenSalvo = true;
                SenhaEntry.Text = string.Empty;

                await AbrirPedidosAsync(resultado.Nome);
            }
            finally
            {
                dados.Senha = string.Empty;
            }
        }
        catch (HttpRequestException)
        {
            MostrarErro(
                _temTokenSalvo
                    ? "Confira se a API está ligada e a conexão USB está ativa. " +
                      "Sua sessão foi mantida. Toque em Tentar novamente."
                    : "Não foi possível conectar à API. " +
                      "Confira se ela está executando.");
        }
        catch (OperationCanceledException)
        {
            MostrarErro(
                _temTokenSalvo
                    ? "A verificação demorou. Sua sessão foi mantida. " +
                      "Toque em Tentar novamente."
                    : "O tempo de espera terminou. Tente novamente.");
        }
        catch (InvalidOperationException ex)
        {
            MostrarErro(ex.Message);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);

            MostrarErro(
                "Não foi possível concluir a entrada. Tente novamente.");
        }
        finally
        {
            _entrando = false;

            EmailEntry.IsVisible = !_temTokenSalvo;
            SenhaEntry.IsVisible = !_temTokenSalvo;

            EntrarButton.Text = _temTokenSalvo
                ? "Tentar novamente"
                : "Entrar";

            EntrarButton.IsEnabled = true;

            CarregandoIndicator.IsRunning = false;
            CarregandoIndicator.IsVisible = false;
        }
    }

    private static async Task AbrirPedidosAsync(string nome)
    {
        await Shell.Current.GoToAsync(
            "//PedidosPage",
            new Dictionary<string, object>
            {
                ["nome"] = nome
            });
    }

    private void MostrarErro(string mensagem)
    {
        MensagemLabel.Text = mensagem;
        MensagemLabel.IsVisible = true;
    }

    private async void OnCriarCadastroClicked(
        object? sender,
        EventArgs e)
    {
        if (_entrando)
            return;

        await Shell.Current.GoToAsync("//CadastroClientePage");
    }
}