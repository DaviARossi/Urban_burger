using System.Data;
using MySql.Data.MySqlClient;

namespace UrbanBurger;
public static class Conexao
{
    public static MySqlConnection Abrir()
    {
        string? texto = Environment.GetEnvironmentVariable("URBAN_MYSQL");
        if (string.IsNullOrWhiteSpace(texto))
            throw new InvalidOperationException("Configure URBAN_MYSQL conforme o LEIA-ME.");
        var conexao = new MySqlConnection(texto);
        try { conexao.Open(); return conexao; }
        catch { conexao.Dispose(); throw; }
    }

    public static MySqlParameter Parametro(string nome, object? valor)
    {
        return new MySqlParameter(nome, valor ?? DBNull.Value);
    }

    public static DataTable Consultar(string sql, params MySqlParameter[] parametros)
    {
        using var conexao = Abrir();
        using var comando = new MySqlCommand(sql, conexao);
        comando.Parameters.AddRange(parametros);
        using var adaptador = new MySqlDataAdapter(comando);
        var tabela = new DataTable();
        adaptador.Fill(tabela);
        return tabela;
    }

    public static void Executar(string sql, params MySqlParameter[] parametros)
    {
        using var conexao = Abrir();
        using var comando = new MySqlCommand(sql, conexao);
        comando.Parameters.AddRange(parametros);
        comando.ExecuteNonQuery();
    }
}
