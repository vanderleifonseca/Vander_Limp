using System.Collections.ObjectModel;
using System.Globalization;
using VanderLimp.App.Dados;
using VanderLimp.App.Servicos;

namespace VanderLimp.App.Views;

public partial class RevisarPedidoPage : ContentPage, IQueryAttributable
{
    private readonly ObservableCollection<ItemPedidoDados> _itens = new();
    private readonly EnderecoEntregaApiServico _enderecoServico;

    private bool _carregandoEnderecos;
    private EnderecoEntregaDados? _enderecoSelecionado;
    private FormaPagamentoDados? _formaPagamentoSelecionada;

    public RevisarPedidoPage(
        EnderecoEntregaApiServico enderecoServico)
    {
        InitializeComponent();

        _enderecoServico = enderecoServico;

        ItensCollection.ItemsSource = _itens;

        EnderecoPicker.ItemDisplayBinding =
            new Binding(nameof(EnderecoEntregaDados.Resumo));

        FormaPagamentoPicker.ItemDisplayBinding =
            new Binding(nameof(FormaPagamentoDados.Descricao));

        FormaPagamentoPicker.ItemsSource =
            new List<FormaPagamentoDados>
            {
                new()
                {
                    Codigo = "PIX",
                    Descricao = "PIX"
                },
                new()
                {
                    Codigo = "CARTAO",
                    Descricao = "Cartão na entrega"
                },
                new()
                {
                    Codigo = "DINHEIRO",
                    Descricao = "Dinheiro na entrega"
                }
            };
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _itens.Clear();

        if (query.TryGetValue("itens", out var valor) &&
            valor is IEnumerable<ItemPedidoDados> itens)
        {
            foreach (var item in itens)
            {
                _itens.Add(item);
            }
        }

        decimal total = _itens.Sum(item => item.Subtotal);

        TotalLabel.Text =
            "Total dos produtos: " +
            total.ToString("C2", CultureInfo.GetCultureInfo("pt-BR"));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await CarregarEnderecosAsync();
    }

    private async void OnAtualizarEnderecosClicked(
        object? sender,
        EventArgs e)
    {
        await CarregarEnderecosAsync();
    }

    private async Task CarregarEnderecosAsync()
    {
        if (_carregandoEnderecos)
            return;

        int? enderecoAnteriorId = _enderecoSelecionado?.Id;

        _carregandoEnderecos = true;

        AtualizarEnderecosButton.IsEnabled = false;
        EnderecoPicker.IsEnabled = false;

        CarregandoEnderecosIndicator.IsVisible = true;
        CarregandoEnderecosIndicator.IsRunning = true;

        MensagemEnderecoLabel.IsVisible = false;
        DetalhesEnderecoLabel.IsVisible = false;

        EnderecoPicker.SelectedItem = null;
        EnderecoPicker.ItemsSource = null;
        _enderecoSelecionado = null;

        try
        {
            var enderecos = await _enderecoServico.ListarAsync();

            EnderecoPicker.ItemsSource = enderecos;

            if (enderecos.Count == 0)
            {
                MostrarErroEndereco(
                    "Você ainda não possui um endereço ativo " +
                    "disponível para entrega em Foz do Iguaçu.");

                return;
            }

            // Mantém o endereço escolhido se ele ainda estiver disponível.
            var selecionado = enderecos.FirstOrDefault(
                endereco => endereco.Id == enderecoAnteriorId);

            // Se não houver seleção anterior, prioriza o endereço padrão.
            selecionado ??= enderecos.FirstOrDefault(
                endereco => endereco.EhEntregaPadrao);

            EnderecoPicker.SelectedItem = selecionado;
        }
        catch (UnauthorizedAccessException)
        {
            try
            {
                await DisplayAlertAsync(
                    "Sessão encerrada",
                    "Entre novamente para consultar seus endereços.",
                    "OK");

                await Shell.Current.GoToAsync("//LoginPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

                MostrarErroEndereco(
                    "Sua sessão terminou. Feche e abra o aplicativo.");
            }
        }
        catch (HttpRequestException)
        {
            MostrarErroEndereco(
                "Não foi possível conectar à API. " +
                "Confira a conexão e atualize os endereços.");
        }
        catch (OperationCanceledException)
        {
            MostrarErroEndereco(
                "O tempo de espera terminou. Tente atualizar novamente.");
        }
        catch (InvalidOperationException ex)
        {
            MostrarErroEndereco(ex.Message);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);

            MostrarErroEndereco(
                "Não foi possível carregar os endereços.");
        }
        finally
        {
            _carregandoEnderecos = false;

            AtualizarEnderecosButton.IsEnabled = true;

            EnderecoPicker.IsEnabled =
                EnderecoPicker.ItemsSource?.Count > 0;

            CarregandoEnderecosIndicator.IsRunning = false;
            CarregandoEnderecosIndicator.IsVisible = false;
        }
    }

    private void OnEnderecoChanged(object? sender, EventArgs e)
    {
        _enderecoSelecionado =
            EnderecoPicker.SelectedItem as EnderecoEntregaDados;

        if (_enderecoSelecionado is null)
        {
            DetalhesEnderecoLabel.Text = string.Empty;
            DetalhesEnderecoLabel.IsVisible = false;
            return;
        }

        var endereco = _enderecoSelecionado;

        var linhas = new List<string>
        {
            endereco.Resumo,
            $"CEP: {endereco.CEP}"
        };

        if (!string.IsNullOrWhiteSpace(endereco.Complemento))
        {
            linhas.Add($"Complemento: {endereco.Complemento}");
        }

        if (!string.IsNullOrWhiteSpace(endereco.PontoReferencia))
        {
            linhas.Add($"Referência: {endereco.PontoReferencia}");
        }

        if (endereco.EhEntregaPadrao)
        {
            linhas.Add("Endereço de entrega padrão.");
        }

        DetalhesEnderecoLabel.Text =
            string.Join(Environment.NewLine, linhas);

        DetalhesEnderecoLabel.IsVisible = true;
    }

    private void OnFormaPagamentoChanged(object? sender, EventArgs e)
    {
        _formaPagamentoSelecionada =
            FormaPagamentoPicker.SelectedItem as FormaPagamentoDados;

        if (_formaPagamentoSelecionada is null)
        {
            OrientacaoPagamentoLabel.Text = string.Empty;
            OrientacaoPagamentoLabel.IsVisible = false;
            return;
        }

        OrientacaoPagamentoLabel.Text =
            _formaPagamentoSelecionada.Codigo switch
            {
                "PIX" =>
                    "O pagamento será feito por QR Code ou PIX copia e cola " +
                    "na finalização. A cobrança ainda não está disponível " +
                    "nesta versão de testes.",

                "CARTAO" =>
                    "O pagamento com cartão será realizado na entrega.",

                "DINHEIRO" =>
                    "O pagamento em dinheiro será realizado na entrega.",

                _ => string.Empty
            };

        OrientacaoPagamentoLabel.IsVisible =
            !string.IsNullOrWhiteSpace(OrientacaoPagamentoLabel.Text);
    }

    private void MostrarErroEndereco(string mensagem)
    {
        MensagemEnderecoLabel.Text = mensagem;
        MensagemEnderecoLabel.IsVisible = true;
    }

    private async void OnAlterarProdutosClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}