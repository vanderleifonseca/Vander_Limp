using System.ComponentModel.DataAnnotations;

namespace VanderLimp.Admin.Dados;

public class LoginAdminDados
{
    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha.")]
    [StringLength(128)]
    public string Senha { get; set; } = string.Empty;
}