using System.Globalization;

namespace UrbanBurger;

public class PrincipalForm : Form
{
    private readonly DataGridView pedidos = new DataGridView();
    private readonly DataGridView produtos = new DataGridView();
    private readonly DataGridView clientes = new DataGridView();
    private readonly DataGridView categorias = new DataGridView();
    private readonly ComboBox status = new ComboBox();
    private readonly TextBox preco;
    private readonly TextBox nome, email, cpf, telefone, endereco, categoria;
    private readonly TabControl abas = new TabControl();

    public PrincipalForm()
    {
        Text = "Urban Burger — Gestão do delivery";
        Size = new Size(1150, 760);
        MinimumSize = new Size(900, 650);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10);
        abas.Dock = DockStyle.Fill;
        Controls.Add(abas);

        var barraPedidos = CriarAba("Pedidos", pedidos);
        Tela.Botao(barraPedidos, "Importar JSON", Importar);
        status.DropDownStyle = ComboBoxStyle.DropDownList;
        status.Items.AddRange(new object[] { "Em preparação", "Saiu para entrega", "Entregue" });
        status.SelectedIndex = 0;
        status.Width = 190;
        status.Margin = new Padding(8, 12, 8, 8);
        barraPedidos.Controls.Add(status);
        Tela.Botao(barraPedidos, "Alterar status", AlterarStatus);
        Tela.Botao(barraPedidos, "Cancelar e excluir", ExcluirPedido);
        Tela.Botao(barraPedidos, "Ver itens", VerItens);

        var barraProdutos = CriarAba("Produtos", produtos);
        preco = Tela.Campo(barraProdutos, "Novo preço (ex.: 28,90)", 12);
        Tela.Botao(barraProdutos, "Alterar preço", AlterarPreco);
        Tela.Botao(barraProdutos, "Excluir produto", ExcluirProduto);

        var barraClientes = CriarAba("Clientes", clientes);
        nome = Tela.Campo(barraClientes, "Nome", 80);
        email = Tela.Campo(barraClientes, "E-mail", 100, 220);
        cpf = Tela.Campo(barraClientes, "CPF", 14);
        telefone = Tela.Campo(barraClientes, "Celular com DDD", 15);
        endereco = Tela.Campo(barraClientes, "Endereço", 200, 330);
        Tela.Botao(barraClientes, "Limpar campos", LimparCliente);
        Tela.Botao(barraClientes, "Cadastrar cliente", InserirCliente);
        Tela.Botao(barraClientes, "Salvar selecionado", AlterarCliente);
        Tela.Botao(barraClientes, "Excluir cliente", ExcluirCliente);
        clientes.SelectionChanged += SelecionarCliente;

        var barraCategorias = CriarAba("Categorias", categorias);
        categoria = Tela.Campo(barraCategorias, "Nome da categoria", 40, 250);
        Tela.Botao(barraCategorias, "Cadastrar categoria", InserirCategoria);
        Tela.Botao(barraCategorias, "Salvar selecionada", AlterarCategoria);
        Tela.Botao(barraCategorias, "Excluir categoria", ExcluirCategoria);
        categorias.SelectionChanged += SelecionarCategoria;
        Shown += Atualizar;
    }

    private FlowLayoutPanel CriarAba(string titulo, DataGridView grade)
    {
        var pagina = new TabPage(titulo);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var barra = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, Padding = new Padding(6) };
        Tela.Botao(barra, "Atualizar listas", Atualizar);
        grade.Dock = DockStyle.Fill;
        grade.ReadOnly = true;
        grade.AllowUserToAddRows = false;
        grade.AllowUserToDeleteRows = false;
        grade.MultiSelect = false;
        grade.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grade.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        layout.Controls.Add(barra, 0, 0);
        layout.Controls.Add(grade, 0, 1);
        pagina.Controls.Add(layout);
        abas.TabPages.Add(pagina);
        return barra;
    }

    private void Atualizar(object? sender, EventArgs e)
    {
        try
        {
            pedidos.DataSource = Conexao.Consultar("SELECT p.id,p.codigo_origem,c.nome AS cliente,p.data_pedido,p.status,p.endereco_entrega,p.valor_total FROM pedidos p JOIN clientes c ON c.id=p.cliente_id ORDER BY p.id DESC");
            produtos.DataSource = Conexao.Consultar("SELECT id,nome,categoria_id,preco FROM produtos ORDER BY id");
            clientes.DataSource = Conexao.Consultar("SELECT id,nome,email,cpf,telefone,endereco FROM clientes ORDER BY nome");
            categorias.DataSource = Conexao.Consultar("SELECT id,nome FROM categorias ORDER BY id");
        }
        catch (Exception erro) { Tela.Erro(erro); }
    }

    private void Importar(object? sender, EventArgs e)
    {
        using var janela = new OpenFileDialog { Filter = "Pedido JSON (*.json)|*.json", Multiselect = false };
        if (janela.ShowDialog() != DialogResult.OK) return;
        try
        {
            PedidoJson pedido = LeituraPedido.Ler(janela.FileName);
            if (!Tela.Confirmar("Importar " + pedido.Codigo + " de " + pedido.Cliente.Nome + "? Total: " + pedido.ValorTotal.ToString("C", CultureInfo.GetCultureInfo("pt-BR")))) return;
            Importador.Importar(pedido);
            Atualizar(sender, e);
            MessageBox.Show("Pedido importado com sucesso.");
        }
        catch (Exception erro) { Tela.Erro(erro); }
    }

    private void AlterarStatus(object? sender, EventArgs e)
    {
        try
        {
            Conexao.Executar("UPDATE pedidos SET status=@status WHERE id=@id",
                Conexao.Parametro("@status", status.Text), Conexao.Parametro("@id", Tela.Id(pedidos)));
            Atualizar(sender, e);
        }
        catch (Exception erro) { Tela.Erro(erro); }
    }

    private void ExcluirPedido(object? sender, EventArgs e)
    {
        try
        {
            int id = Tela.Id(pedidos);
            if (!Tela.Confirmar("Cancelar e excluir o pedido " + id + " e seus itens?")) return;
            Importador.Cancelar(id);
            Atualizar(sender, e);
        }
        catch (Exception erro) { Tela.Erro(erro); }
    }

    private void VerItens(object? sender, EventArgs e)
    {
        try
        {
            var dados = Conexao.Consultar("SELECT p.nome,i.quantidade,i.preco_unitario,i.quantidade*i.preco_unitario AS subtotal FROM itens_pedido i JOIN produtos p ON p.id=i.produto_id WHERE i.pedido_id=@id", Conexao.Parametro("@id", Tela.Id(pedidos)));
            using var janela = new Form { Text = "Itens do pedido", Size = new Size(700, 400), StartPosition = FormStartPosition.CenterParent };
            janela.Controls.Add(new DataGridView { Dock = DockStyle.Fill, DataSource = dados, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill });
            janela.ShowDialog(this);
        }
        catch (Exception erro) { Tela.Erro(erro); }
    }

    private void AlterarPreco(object? sender, EventArgs e)
    {
        try
        {
            if (!decimal.TryParse(preco.Text, NumberStyles.AllowDecimalPoint, CultureInfo.GetCultureInfo("pt-BR"), out decimal valor) || valor <= 0 || valor > 99999999.99m || decimal.Round(valor, 2) != valor)
                throw new InvalidDataException("Informe um preço positivo com até duas casas, usando vírgula.");
            Conexao.Executar("UPDATE produtos SET preco=@preco WHERE id=@id", Conexao.Parametro("@preco", valor), Conexao.Parametro("@id", Tela.Id(produtos)));
            Atualizar(sender, e);
            MessageBox.Show("Preço salvo no banco. Atualize também o cardápio e o JavaScript do site antes de gerar novos pedidos.");
        }
        catch (Exception erro) { Tela.Erro(erro); }
    }

    private void ExcluirProduto(object? sender, EventArgs e)
    {
        try
        {
            int id = Tela.Id(produtos);
            if (!Tela.Confirmar("Excluir produto " + id + "? Produtos já usados em pedidos não podem ser excluídos.")) return;
            Conexao.Executar("DELETE FROM produtos WHERE id=@id", Conexao.Parametro("@id", id));
            Atualizar(sender, e);
        }
        catch (Exception erro) { Tela.Erro(erro); }
    }

    private void SelecionarCliente(object? sender, EventArgs e)
    {
        if (clientes.CurrentRow == null) return;
        var linha = clientes.CurrentRow;
        nome.Text = Convert.ToString(linha.Cells["nome"].Value);
        email.Text = Convert.ToString(linha.Cells["email"].Value);
        cpf.Text = Convert.ToString(linha.Cells["cpf"].Value);
        telefone.Text = Convert.ToString(linha.Cells["telefone"].Value);
        endereco.Text = Convert.ToString(linha.Cells["endereco"].Value);
    }
    private void LimparCliente(object? sender, EventArgs e)
    {
        nome.Clear(); email.Clear(); cpf.Clear(); telefone.Clear(); endereco.Clear();
    }
    private void InserirCliente(object? sender, EventArgs e) { SalvarCliente(false); }
    private void AlterarCliente(object? sender, EventArgs e) { SalvarCliente(true); }
    private void SalvarCliente(bool editar)
    {
        try
        {
            var cliente = new ClienteJson { Nome = nome.Text.Trim(), Email = email.Text.Trim(), Cpf = cpf.Text.Trim(), Telefone = telefone.Text.Trim(), Endereco = endereco.Text.Trim() };
            LeituraPedido.ValidarCliente(cliente);
            string sql = "INSERT INTO clientes (nome,email,cpf,telefone,endereco) VALUES (@nome,@email,@cpf,@telefone,@endereco)";
            int id = 0;
            if (editar)
            {
                id = Tela.Id(clientes);
                sql = "UPDATE clientes SET nome=@nome,email=@email,cpf=@cpf,telefone=@telefone,endereco=@endereco WHERE id=@id";
            }
            Conexao.Executar(sql, Conexao.Parametro("@nome", cliente.Nome), Conexao.Parametro("@email", cliente.Email),
                Conexao.Parametro("@cpf", cliente.Cpf), Conexao.Parametro("@telefone", cliente.Telefone), Conexao.Parametro("@endereco", cliente.Endereco), Conexao.Parametro("@id", id));
            Atualizar(null, EventArgs.Empty);
        }
        catch (Exception erro) { Tela.Erro(erro); }
    }
    private void ExcluirCliente(object? sender, EventArgs e)
    {
        try
        {
            int id = Tela.Id(clientes);
            if (!Tela.Confirmar("Excluir cliente " + id + "? Clientes com pedidos não podem ser excluídos.")) return;
            Conexao.Executar("DELETE FROM clientes WHERE id=@id", Conexao.Parametro("@id", id));
            Atualizar(sender, e);
        }
        catch (Exception erro) { Tela.Erro(erro); }
    }
    private void SelecionarCategoria(object? sender, EventArgs e)
    {
        if (categorias.CurrentRow != null) categoria.Text = Convert.ToString(categorias.CurrentRow.Cells["nome"].Value);
    }
    private void InserirCategoria(object? sender, EventArgs e) { SalvarCategoria(false); }
    private void AlterarCategoria(object? sender, EventArgs e) { SalvarCategoria(true); }
    private void SalvarCategoria(bool editar)
    {
        try
        {
            LeituraPedido.Texto(categoria.Text.Trim(), "Categoria", 40);
            if (editar) Conexao.Executar("UPDATE categorias SET nome=@nome WHERE id=@id", Conexao.Parametro("@nome", categoria.Text.Trim()), Conexao.Parametro("@id", Tela.Id(categorias)));
            else Conexao.Executar("INSERT INTO categorias (nome) VALUES (@nome)", Conexao.Parametro("@nome", categoria.Text.Trim()));
            Atualizar(null, EventArgs.Empty);
        }
        catch (Exception erro) { Tela.Erro(erro); }
    }
    private void ExcluirCategoria(object? sender, EventArgs e)
    {
        try
        {
            int id = Tela.Id(categorias);
            if (!Tela.Confirmar("Excluir categoria " + id + "? Categorias com produtos não podem ser excluídas.")) return;
            Conexao.Executar("DELETE FROM categorias WHERE id=@id", Conexao.Parametro("@id", id));
            Atualizar(sender, e);
        }
        catch (Exception erro) { Tela.Erro(erro); }
    }
}
