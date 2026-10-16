using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using VanderLimp.Api.Servicos;

var configuracao = new ConfigurationBuilder()
    .AddUserSecrets(typeof(SenhaServico).Assembly, optional: true)
    .Build();

string? connectionString =
    configuracao.GetConnectionString("VanderLimp");

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine(
        "A conexão VanderLimp não foi encontrada nos segredos da API.");

    return;
}

Console.WriteLine("Configuração do administrador Vander Limp");
Console.WriteLine();

Console.Write("E-mail: ");
string email = (Console.ReadLine() ?? string.Empty)
    .Trim()
    .ToLowerInvariant();

if (email.Length > 254 ||
    !new EmailAddressAttribute().IsValid(email))
{
    Console.WriteLine("Informe um e-mail válido.");
    return;
}

Console.Write("Senha: ");
string senha = LerSenha();

if (senha.Length < 8 || senha.Length > 128)
{
    Console.WriteLine("A senha deve ter entre 8 e 128 caracteres.");
    return;
}

Console.Write("Confirme a senha: ");
string confirmacao = LerSenha();

if (senha != confirmacao)
{
    Console.WriteLine("As senhas não conferem.");
    return;
}

try
{
    await using var conexao =
        new MySqlConnection(connectionString);

    await conexao.OpenAsync();

    const string consulta = """
        SELECT COUNT(*)
        FROM Usuarios
        WHERE Email = @Email;
        """;

    await using (var verificar =
        new MySqlCommand(consulta, conexao))
    {
        verificar.Parameters.AddWithValue("@Email", email);

        long quantidade = Convert.ToInt64(
            await verificar.ExecuteScalarAsync());

        if (quantidade > 0)
        {
            Console.WriteLine(
                "Esse e-mail já está cadastrado. " +
                "Nenhum usuário foi alterado.");

            return;
        }
    }

    var senhaServico = new SenhaServico();
    string senhaHash = senhaServico.GerarHash(senha);

    const string inserir = """
        INSERT INTO Usuarios
            (Email, SenhaHash, TipoAcesso, Ativo, EmailConfirmado)
        VALUES
            (@Email, @SenhaHash, 'Administrador', 1, 0);
        """;

    await using var comando =
        new MySqlCommand(inserir, conexao);

    comando.Parameters.AddWithValue("@Email", email);
    comando.Parameters.AddWithValue("@SenhaHash", senhaHash);

    await comando.ExecuteNonQueryAsync();

    Console.WriteLine();
    Console.WriteLine("Administrador cadastrado com sucesso.");
    Console.WriteLine($"E-mail: {email}");
    Console.WriteLine($"ID: {comando.LastInsertedId}");
}
catch (MySqlException ex) when (ex.Number == 1062)
{
    Console.WriteLine(
        "Esse e-mail já está cadastrado. Nenhum usuário foi alterado.");
}
catch (MySqlException ex)
{
    Console.WriteLine(
        $"Não foi possível cadastrar o administrador. " +
        $"Código MySQL: {ex.Number}");
}
finally
{
    senha = string.Empty;
    confirmacao = string.Empty;
}

static string LerSenha()
{
    var caracteres = new List<char>();

    while (true)
    {
        ConsoleKeyInfo tecla = Console.ReadKey(intercept: true);

        if (tecla.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            return new string(caracteres.ToArray());
        }

        if (tecla.Key == ConsoleKey.Backspace)
        {
            if (caracteres.Count > 0)
            {
                caracteres.RemoveAt(caracteres.Count - 1);
            }

            continue;
        }

        if (!char.IsControl(tecla.KeyChar) &&
            caracteres.Count < 129)
        {
            caracteres.Add(tecla.KeyChar);
        }
    }
}