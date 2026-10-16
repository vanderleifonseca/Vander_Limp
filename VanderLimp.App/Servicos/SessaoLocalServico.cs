using Microsoft.Maui.Storage;

namespace VanderLimp.App.Servicos;

public class SessaoLocalServico
{
    private const string ChaveToken = "vander_limp_token";

    public async Task SalvarTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException(
                "O token da sessão não pode estar vazio.",
                nameof(token));
        }

        await SecureStorage.Default.SetAsync(ChaveToken, token);
    }

    public async Task<string?> ObterTokenAsync()
    {
        return await SecureStorage.Default.GetAsync(ChaveToken);
    }

    public void EncerrarSessaoLocal()
    {
        SecureStorage.Default.Remove(ChaveToken);
    }
}