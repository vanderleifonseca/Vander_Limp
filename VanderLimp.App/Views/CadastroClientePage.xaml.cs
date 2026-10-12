using Microsoft.Extensions.DependencyInjection;
using VanderLimp.App.Dados;
using VanderLimp.App.Servicos;

namespace VanderLimp.App.Views;

public partial class CadastroClientePage : ContentPage
{
    private bool _cadastrando;

    public CadastroClientePage()
    {
        InitializeComponent();

        TipoPessoaPicker.SelectedIndex = 0;
    }

    private void OnTipoPessoaChanged(object? sender, EventArgs e)
    {
        bool pessoaJuridica = TipoPessoaPicker.SelectedIndex == 1;

        DadosEmpresaLayout.IsVisible = pessoaJuridica;

        NomeLabel.Text = pessoaJuridica
            ? "Razão social"
            : "Nome completo";

        NomeEntry.Placeholder = pessoaJuridica
            ? "Informe a razão social"
            : "Informe seu nome completo";

        DocumentoLabel.Text = pessoaJuridica ? "CNPJ" : "CPF";

        DocumentoEntry.Placeholder = pessoaJuridica
            ? "CNPJ sem pontuação, com letras maiúsculas"
            : "CPF sem pontuação";

        DocumentoEntry.Keyboard = pessoaJuridica
            ? Keyboard.Default
            : Keyboard.Numeric;

        DocumentoEntry.MaxLength = pessoaJuridica ? 14 : 11;
        DocumentoEntry.Text = string.Empty;
    }

    private void OnSemNumeroChanged(
        object? sender,
        CheckedChangedEventArgs e)
    {
        NumeroEntry.IsEnabled = !e.Value;

        if (e.Value)
        {
            NumeroEntry.Text = string.Empty;
        }
    }

    private async void OnCadastrarClicked(object? sender, EventArgs e)
    {
        if (_cadastrando)
            return;

        MensagemLabel.IsVisible = false;

        if (TipoPessoaPicker.SelectedIndex < 0)
        {
            MostrarErro("Selecione o tipo de pessoa.");
            return;
        }

        if (string.IsNullOrWhiteSpace(SenhaEntry.Text) ||
            SenhaEntry.Text.Length < 8)
        {
            MostrarErro("Informe uma senha com pelo menos 8 caracteres.");
            return;
        }

        if (SenhaEntry.Text != ConfirmacaoSenhaEntry.Text)
        {
            MostrarErro("As senhas não conferem.");
            return;
        }

        var servico = Handler?.MauiContext?.Services
            .GetService<ClienteApiServico>();

        if (servico is null)
        {
            MostrarErro("O serviço de cadastro não está disponível.");
            return;
        }

        bool pessoaJuridica = TipoPessoaPicker.SelectedIndex == 1;

        var dados = new ClienteCadastroDados
        {
            TipoPessoa = pessoaJuridica ? "PJ" : "PF",
            NomeRazaoSocial = Texto(NomeEntry),
            Documento = Texto(DocumentoEntry).ToUpperInvariant(),
            NomeFantasia = pessoaJuridica
                ? TextoOpcional(NomeFantasiaEntry)
                : null,
            NomeResponsavel = pessoaJuridica
                ? TextoOpcional(NomeResponsavelEntry)
                : null,
            Telefone = Texto(TelefoneEntry),
            Email = Texto(EmailEntry),
            Senha = SenhaEntry.Text ?? string.Empty,
            ConfirmacaoSenha = ConfirmacaoSenhaEntry.Text ?? string.Empty,

            Endereco = new EnderecoCadastroDados
            {
                CEP = Texto(CepEntry),
                Logradouro = Texto(LogradouroEntry),
                Numero = SemNumeroCheckBox.IsChecked
                    ? null
                    : TextoOpcional(NumeroEntry),
                SemNumero = SemNumeroCheckBox.IsChecked,
                Complemento = TextoOpcional(ComplementoEntry),
                Bairro = Texto(BairroEntry),
                Cidade = Texto(CidadeEntry),
                UF = Texto(UfEntry).ToUpperInvariant(),
                CodigoMunicipioIBGE = Texto(CodigoMunicipioEntry),
                PontoReferencia = TextoOpcional(PontoReferenciaEntry)
            }
        };

        _cadastrando = true;
        CadastrarButton.IsEnabled = false;
        CarregandoIndicator.IsVisible = true;
        CarregandoIndicator.IsRunning = true;

        bool cadastroConcluido = false;

        try
        {
            var resultado = await servico.CadastrarAsync(dados);
            cadastroConcluido = true;

            SenhaEntry.Text = string.Empty;
            ConfirmacaoSenhaEntry.Text = string.Empty;

            await DisplayAlertAsync(
                "Cadastro concluído",
                resultado.Mensagem,
                "OK");
        }
        catch (HttpRequestException)
        {
            MostrarErro(
                "Não foi possível conectar à API. " +
                "Confira se ela está rodando no computador.");
        }
        catch (OperationCanceledException)
        {
            MostrarErro(
                "O tempo de espera terminou. Antes de tentar novamente, " +
                "confira se o cadastro foi realizado.");
        }
        catch (InvalidOperationException ex)
        {
            MostrarErro(ex.Message);
        }
        catch (Exception)
        {
            MostrarErro(
                "Ocorreu um erro inesperado. Confira o cadastro " +
                "antes de tentar novamente.");
        }
        finally
        {
            _cadastrando = false;
            CarregandoIndicator.IsRunning = false;
            CarregandoIndicator.IsVisible = false;

            // Evita reenviar um cadastro já confirmado nesta tela.
            CadastrarButton.IsEnabled = !cadastroConcluido;

            if (cadastroConcluido)
            {
                CadastrarButton.Text = "Cadastro realizado";
            }
        }
    }

    private void MostrarErro(string mensagem)
    {
        MensagemLabel.Text = mensagem;
        MensagemLabel.IsVisible = true;
    }

    private static string Texto(Entry campo)
    {
        return campo.Text?.Trim() ?? string.Empty;
    }

    private static string? TextoOpcional(Entry campo)
    {
        string texto = Texto(campo);

        return string.IsNullOrWhiteSpace(texto) ? null : texto;
    }
}