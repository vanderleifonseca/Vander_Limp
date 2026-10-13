using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using VanderLimp.App.Dados;
using VanderLimp.App.Servicos;

namespace VanderLimp.App.Views;

public partial class ProdutosPage : ContentPage
{
    private readonly ObservableCollection<ProdutoDados> _produtos = new();

    private bool _carregando;

    public ProdutosPage()
    {
        InitializeComponent();

        ProdutosCollection.ItemsSource = _produtos;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await CarregarProdutosAsync();
    }

    private async void OnAtualizarClicked(object? sender, EventArgs e)
    {
        await CarregarProdutosAsync();
    }

    private async Task CarregarProdutosAsync()
    {
        if (_carregando)
            return;

        _carregando = true;

        AtualizarButton.IsEnabled = false;
        CarregandoIndicator.IsVisible = true;
        CarregandoIndicator.IsRunning = true;
        MensagemLabel.IsVisible = false;

        // Evita manter na tela uma lista antiga se a atualização falhar.
        _produtos.Clear();

        try
        {
            var servico = Handler?.MauiContext?.Services
                .GetService<ProdutoApiServico>();

            if (servico is null)
            {
                MostrarErro("O serviço de produtos não está disponível.");
                return;
            }

            var produtos = await servico.ListarAsync();

            foreach (var produto in produtos)
            {
                _produtos.Add(produto);
            }
        }
        catch (UnauthorizedAccessException)
        {
            try
            {
                await DisplayAlertAsync(
                    "Sessão encerrada",
                    "Entre novamente para consultar os produtos.",
                    "OK");

                await Shell.Current.GoToAsync("//LoginPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                MostrarErro(
                    "Sua sessão terminou. Feche e abra o aplicativo.");
            }
        }
        catch (HttpRequestException)
        {
            MostrarErro(
                "Não foi possível conectar à API. " +
                "Confira a conexão e tente atualizar novamente.");
        }
        catch (OperationCanceledException)
        {
            MostrarErro(
                "O tempo de espera terminou. Tente atualizar novamente.");
        }
        catch (InvalidOperationException ex)
        {
            MostrarErro(ex.Message);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);

            MostrarErro("Não foi possível carregar os produtos.");
        }
        finally
        {
            _carregando = false;
            AtualizarButton.IsEnabled = true;
            CarregandoIndicator.IsRunning = false;
            CarregandoIndicator.IsVisible = false;
        }
    }

    private void MostrarErro(string mensagem)
    {
        MensagemLabel.Text = mensagem;
        MensagemLabel.IsVisible = true;
    }
}