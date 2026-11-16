using System.Net.Mail;

namespace UrbanBurger;

public class LoginForm : Form
{
    private readonly TextBox email;
    private readonly TextBox senha;
    private readonly bool criar;

    public LoginForm(bool primeiroAcesso)
    {
        criar = primeiroAcesso;
        Text = "Urban Burger — Gerente";
        ClientSize = new Size(460, 330);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        var painel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), FlowDirection = FlowDirection.TopDown, WrapContents = false };
        Controls.Add(painel);
        string titulo = "Acesso do gerente";
        if (criar) titulo = "Criar primeiro gerente (somente uma vez)";
        painel.Controls.Add(new Label { Text = titulo, AutoSize = true, ForeColor = Color.FromArgb(185, 28, 28) });
        email = Tela.Campo(painel, "E-mail", 100, 370);
        senha = Tela.Campo(painel, "Senha", 128, 370);
        senha.UseSystemPasswordChar = true;
        Tela.Botao(painel, "Confirmar", Confirmar);
    }

    private void Confirmar(object? sender, EventArgs e)
    {
        try
        {
            string endereco = email.Text.Trim().ToLowerInvariant();
            if (!MailAddress.TryCreate(endereco, out var valido) || valido.Address != endereco)
                throw new InvalidDataException("Informe um e-mail válido.");
            if (criar)
            {
                Conexao.Executar("INSERT INTO gerentes (id,email,senha_hash) VALUES (1,@email,@senha)",
                    Conexao.Parametro("@email", endereco), Conexao.Parametro("@senha", Senhas.Criar(senha.Text)));
                MessageBox.Show("Gerente criado. Abra o aplicativo normalmente para entrar.");
                DialogResult = DialogResult.OK;
                return;
            }
            var dados = Conexao.Consultar("SELECT senha_hash FROM gerentes WHERE email=@email",
                Conexao.Parametro("@email", endereco));
            if (dados.Rows.Count != 1 || !Senhas.Conferir(senha.Text, Convert.ToString(dados.Rows[0][0]) ?? ""))
                throw new InvalidDataException("E-mail ou senha incorretos.");
            DialogResult = DialogResult.OK;
        }
        catch (Exception erro) { Tela.Erro(erro); }
        finally { senha.Clear(); }
    }
}
