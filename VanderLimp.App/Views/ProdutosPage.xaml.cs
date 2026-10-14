using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using VanderLimp.App.Dados;
using VanderLimp.App.Servicos;

namespace VanderLimp.App.Views;

public partial class ProdutosPage : ContentPage
{
    private readonly ObservableCollection<ProdutoSelecaoDados> _produtos = new();
    private readonly ProdutoApiServico _produtoServico;

    private static readonly CultureInfo CulturaBrasil =
        CultureInfo.GetCultureInfo("pt-BR");

    private bool _carregando;

    public ProdutosPage(ProdutoApiServico produtoServico)
    {
        InitializeComponent();

        _produtoServico = produtoServico;
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

    private void OnAumentarClicked(object? sender, EventArgs e)
    {
        if (_carregando)
            return;

        if (sender is Button button &&
            button.CommandParameter is ProdutoSelecaoDados produto &&
            produto.Quantidade < int.MaxValue)
        {
            produto.Quantidade++;
        }
    }

    private void OnDiminuirClicked(object? sender, EventArgs e)
    {
        if (_carregando)
            return;

        if (sender is Button button &&
            button.CommandParameter is ProdutoSelecaoDados produto &&
            produto.Quantidade > 0)
        {
            produto.Quantidade--;
        }
    }

    private void OnProdutoAlterado(
        object? sender,
        PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProdutoSelecaoDados.Quantidade))
        {
            AtualizarTotal();
        }
    }

    private void AtualizarTotal()
    {
        decimal total = _produtos.Sum(produto => produto.Subtotal);

        TotalLabel.Text =
            $"Total dos produtos: {total.ToString("C2", CulturaBrasil)}";
    }

    private async Task CarregarProdutosAsync()
    {
        if (_carregando)
            return;

        _carregando = true;

        AtualizarButton.IsEnabled = false;
        ProdutosCollection.IsEnabled = false;
        CarregandoIndicator.IsVisible = true;
        CarregandoIndicator.IsRunning = true;
        MensagemLabel.IsVisible = false;

        // Guarda as quantidades para os produtos que continuarem ativos.
        var quantidades = _produtos.ToDictionary(
            produto => produto.Id,
            produto => produto.Quantidade);

        foreach (var produto in _produtos)
        {
            produto.PropertyChanged -= OnProdutoAlterado;
        }

        _produtos.Clear();
        AtualizarTotal();

        try
        {
            var produtos = await _produtoServico.ListarAsync();

            foreach (var produto in produtos)
            {
                var selecao = new ProdutoSelecaoDados(produto);

                if (quantidades.TryGetValue(produto.Id, out int quantidade))
                {
                    selecao.Quantidade = quantidade;
                }

                selecao.PropertyChanged += OnProdutoAlterado;
                _produtos.Add(selecao);
            }

            AtualizarTotal();
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
            ProdutosCollection.IsEnabled = true;
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