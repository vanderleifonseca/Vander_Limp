using Microsoft.Extensions.DependencyInjection;
using VanderLimp.App.Dados;
using VanderLimp.App.Servicos;

namespace VanderLimp.App.Views;

public partial class LoginPage : ContentPage
{
    private bool _entrando;

    public LoginPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_entrando)
    return;

        _entrando = true;

        EntrarButton.Text = "Entrar";
        SenhaEntry.Text = string.Empty;

        EntrarButton.IsEnabled = false;
        MensagemLabel.IsVisible = false;
        CarregandoIndicator.IsVisible = true;
        CarregandoIndicator.IsRunning = true;

        try
        {
            var servicos = Handler?.MauiContext?.Services;

            var autenticacao = servicos?
                .GetService<AutenticacaoApiServico>();

            var sessaoLocal = servicos?
                .GetService<SessaoLocalServico>();

            if (autenticacao is null || sessaoLocal is null)
            {
                MostrarErro("O serviço de sessão não está disponível.");
                return;
            }

            string? token = await sessaoLocal.ObterTokenAsync();

            // Sem token salvo, permanece na tela de login.
            if (string.IsNullOrWhiteSpace(token))
                return;

            var sessao = await autenticacao.ConsultarSessaoAsync(token);

            if (sessao is null)
            {
                // A API recusou a sessão com HTTP 401.
                sessaoLocal.EncerrarSessaoLocal();

                MostrarErro(
                    "Sua sessão terminou. Informe o e-mail e a senha.");
                return;
            }

            await Shell.Current.GoToAsync(
                "//PedidosPage",
                new Dictionary<string, object>
                {
                    ["nome"] = sessao.Nome
                });
        }
        catch (HttpRequestException)
        {
            MostrarErro(
                "Não foi possível conectar à API para verificar sua sessão. " +
                "Confira a conexão. Sua sessão salva foi mantida.");
        }
        catch (OperationCanceledException)
        {
            MostrarErro(
                "O tempo de verificação terminou. " +
                "Sua sessão salva foi mantida.");
        }
        catch (InvalidOperationException ex)
        {
            MostrarErro(ex.Message);
        }
        catch (Exception)
        {
            MostrarErro(
                "Não foi possível acessar ou verificar a sessão salva. " +
                "Você pode tentar entrar com e-mail e senha.");
        }
        finally
        {
            _entrando = false;
            EntrarButton.IsEnabled = true;
            CarregandoIndicator.IsRunning = false;
            CarregandoIndicator.IsVisible = false;
        }
    }

    private async void OnEntrarClicked(object? sender, EventArgs e)
    {
        if (_entrando)
            return;

        MensagemLabel.IsVisible = false;

        if (string.IsNullOrWhiteSpace(EmailEntry.Text) ||
            string.IsNullOrWhiteSpace(SenhaEntry.Text))
        {
            MostrarErro("Informe o e-mail e a senha.");
            return;
        }

        var servicos = Handler?.MauiContext?.Services;

        var autenticacao = servicos?
            .GetService<AutenticacaoApiServico>();

        var sessaoLocal = servicos?
            .GetService<SessaoLocalServico>();

        if (autenticacao is null || sessaoLocal is null)
        {
            MostrarErro("O serviço de login não está disponível.");
            return;
        }

        var dados = new LoginDados
        {
            Email = EmailEntry.Text.Trim(),
            Senha = SenhaEntry.Text ?? string.Empty
        };

        _entrando = true;
        EntrarButton.IsEnabled = false;
        CarregandoIndicator.IsVisible = true;
        CarregandoIndicator.IsRunning = true;

        bool tokenSalvo = false;

        try
        {
            var resultado = await autenticacao.LoginAsync(dados);

            await sessaoLocal.SalvarTokenAsync(resultado.Token);
            tokenSalvo = true;

            SenhaEntry.Text = string.Empty;

        await Shell.Current.GoToAsync(
            "//PedidosPage",
            new Dictionary<string, object>
            {
                ["nome"] = resultado.Nome
            });
        }
        catch (HttpRequestException)
        {
            MostrarErro(
                "Não foi possível conectar à API. " +
                "Confira a conexão e se ela está rodando.");
        }
        catch (OperationCanceledException)
        {
            MostrarErro(
                "O tempo de espera terminou. Tente novamente.");
        }
        catch (InvalidOperationException ex)
        {
            MostrarErro(ex.Message);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);

            MostrarErro(
                $"Falha no login: {ex.GetType().Name}\n{ex.Message}");
        }
        finally
        {
            _entrando = false;
            CarregandoIndicator.IsRunning = false;
            CarregandoIndicator.IsVisible = false;
            EntrarButton.IsEnabled = !tokenSalvo;

            if (tokenSalvo)
            {
                EntrarButton.Text = "Login realizado";
            }
        }
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
        await Shell.Current.GoToAsync("//CadastroClientePage");
    }
}