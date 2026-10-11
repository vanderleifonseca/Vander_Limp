using System.ComponentModel.DataAnnotations;
using VanderLimp.Api.Validacoes;

namespace VanderLimp.Api.Dados;

public class ClienteCadastroDto : IValidatableObject
{
    [Required]
    [RegularExpression(@"^(PF|PJ)$",
        ErrorMessage = "Informe PF ou PJ.")]
    public string TipoPessoa { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string NomeRazaoSocial { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^([0-9]{11}|[A-Z0-9]{12}[0-9]{2})$",
        ErrorMessage = "Informe CPF ou CNPJ sem pontuação. Use letras maiúsculas no CNPJ.")]
    public string Documento { get; set; } = string.Empty;

    [StringLength(200)]
    public string? NomeFantasia { get; set; }

    [StringLength(200)]
    public string? NomeResponsavel { get; set; }

    [Required]
    [StringLength(20)]
    public string Telefone { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 8)]
    public string Senha { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Senha),
        ErrorMessage = "As senhas não conferem.")]
    public string ConfirmacaoSenha { get; set; } = string.Empty;

    [Required]
    public EnderecoCadastroDto? Endereco { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (TipoPessoa == "PF")
        {
            if (Documento?.Length != 11)
            {
                yield return new ValidationResult(
                    "Pessoa física deve informar CPF com 11 números.",
                    new[] { nameof(Documento) });
            }
            else if (!DocumentoValidacao.CpfValido(Documento))
            {
                yield return new ValidationResult(
                    "O CPF informado é inválido.",
                    new[] { nameof(Documento) });
            }
        }

        if (TipoPessoa == "PJ")
        {
            if (!DocumentoValidacao.CnpjValido(Documento))
            {
                yield return new ValidationResult(
                    "O CNPJ informado é inválido.",
                    new[] { nameof(Documento) });
            }

            if (string.IsNullOrWhiteSpace(NomeResponsavel))
            {
                yield return new ValidationResult(
                    "Informe o responsável pela empresa.",
                    new[] { nameof(NomeResponsavel) });
            }
        }

        if (string.IsNullOrWhiteSpace(NomeRazaoSocial))
        {
            yield return new ValidationResult(
                "Informe o nome ou a razão social.",
                new[] { nameof(NomeRazaoSocial) });
        }

        if (string.IsNullOrWhiteSpace(Telefone))
        {
            yield return new ValidationResult(
                "Informe o telefone.",
                new[] { nameof(Telefone) });
        }

        if (Endereco is not null)
        {
            if (Endereco.SemNumero &&
                !string.IsNullOrWhiteSpace(Endereco.Numero))
            {
                yield return new ValidationResult(
                    "Deixe o número vazio ao selecionar Sem número.",
                    new[] { "Endereco.Numero" });
            }

            if (!Endereco.SemNumero &&
                string.IsNullOrWhiteSpace(Endereco.Numero))
            {
                yield return new ValidationResult(
                    "Informe o número ou selecione Sem número.",
                    new[] { "Endereco.Numero" });
            }
        }
    }
}