namespace VanderLimp.Api.Validacoes;

public static class DocumentoValidacao
{
    public static bool CpfValido(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf) ||
            cpf.Length != 11 ||
            cpf.Any(c => c < '0' || c > '9'))
        {
            return false;
        }

        if (cpf.All(c => c == cpf[0]))
            return false;

        for (int quantidade = 9; quantidade <= 10; quantidade++)
        {
            int soma = 0;

            for (int i = 0; i < quantidade; i++)
            {
                soma += (cpf[i] - '0') * (quantidade + 1 - i);
            }

            int resto = soma % 11;
            int digito = resto < 2 ? 0 : 11 - resto;

            if (cpf[quantidade] - '0' != digito)
                return false;
        }

        return true;
    }

    public static bool CnpjValido(string? cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj) || cnpj.Length != 14)
            return false;

        // Primeiras 12 posições: números ou letras maiúsculas.
        for (int i = 0; i < 12; i++)
        {
            char caractere = cnpj[i];

            bool ehNumero = caractere >= '0' && caractere <= '9';
            bool ehLetra = caractere >= 'A' && caractere <= 'Z';

            if (!ehNumero && !ehLetra)
                return false;
        }

        // Últimas duas posições: somente números.
        if (cnpj[12] < '0' || cnpj[12] > '9' ||
            cnpj[13] < '0' || cnpj[13] > '9')
        {
            return false;
        }

        if (cnpj.All(c => c == cnpj[0]))
            return false;

        int[] pesos = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        for (int quantidade = 12; quantidade <= 13; quantidade++)
        {
            int soma = 0;
            int inicio = 13 - quantidade;

            for (int i = 0; i < quantidade; i++)
            {
                soma += (cnpj[i] - '0') * pesos[inicio + i];
            }

            int resto = soma % 11;
            int digito = resto < 2 ? 0 : 11 - resto;

            if (cnpj[quantidade] - '0' != digito)
                return false;
        }

        return true;
    }
}