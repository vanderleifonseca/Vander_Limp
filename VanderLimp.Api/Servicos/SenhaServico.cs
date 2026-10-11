using Microsoft.AspNetCore.Identity;

namespace VanderLimp.Api.Servicos;

public class SenhaServico
{
    private readonly PasswordHasher<object> _hasher = new();
    private readonly object _usuario = new();

    public string GerarHash(string senha)
    {
        return _hasher.HashPassword(_usuario, senha);
    }

    public bool VerificarSenha(string senhaHash, string senha)
    {
        var resultado = _hasher.VerifyHashedPassword(
            _usuario,
            senhaHash,
            senha);

        return resultado != PasswordVerificationResult.Failed;
    }
}