using MySql.Data.MySqlClient;

namespace UrbanBurger;
public static class Tela
{
    public static TextBox Campo(FlowLayoutPanel painel, string rotulo, int limite, int largura = 180)
    {
        var grupo = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, WrapContents = false, Margin = new Padding(8) };
        grupo.Controls.Add(new Label { Text = rotulo, AutoSize = true });
        var campo = new TextBox { Width = largura, MaxLength = limite };
        grupo.Controls.Add(campo);
        painel.Controls.Add(grupo);
        return campo;
    }

    public static void Botao(FlowLayoutPanel painel, string texto, EventHandler acao)
    {
        var botao = new Button { Text = texto, AutoSize = true, MinimumSize = new Size(120, 36), Margin = new Padding(8), BackColor = Color.FromArgb(185, 28, 28), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        botao.Click += acao;
        painel.Controls.Add(botao);
    }

    public static void Erro(Exception erro)
    {
        string mensagem = "Não foi possível concluir a operação. Confira a conexão e o banco conforme o manual.";
        if (erro is InvalidDataException || erro is InvalidOperationException) mensagem = erro.Message;
        if (erro is System.Text.Json.JsonException) mensagem = "JSON inválido ou com campos de tipos incorretos.";
        if (erro is IOException || erro is UnauthorizedAccessException) mensagem = "Não foi possível ler o arquivo selecionado.";
        if (erro is MySqlException mysql)
        {
            if (mysql.Number == 1062) mensagem = "Registro duplicado: código do pedido, e-mail, CPF ou gerente já cadastrado.";
            if (mysql.Number == 1451) mensagem = "Não é possível excluir: existem registros vinculados.";
        }
        MessageBox.Show(mensagem, "Urban Burger", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    public static int Id(DataGridView grade)
    {
        if (grade.CurrentRow == null) throw new InvalidDataException("Selecione uma linha da tabela.");
        return Convert.ToInt32(grade.CurrentRow.Cells["id"].Value);
    }

    public static bool Confirmar(string texto)
    {
        return MessageBox.Show(texto, "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }
}
