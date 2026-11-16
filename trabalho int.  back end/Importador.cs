using MySql.Data.MySqlClient;

namespace UrbanBurger;

public static class Importador
{
    public static void Importar(PedidoJson pedido)
    {
        LeituraPedido.Validar(pedido);
        using var conexao = Conexao.Abrir();
        using var transacao = conexao.BeginTransaction();
        using var comando = new MySqlCommand();
        comando.Connection = conexao;
        comando.Transaction = transacao;
        try
        {
            comando.CommandText = "INSERT INTO importacoes (codigo) VALUES (@codigo)";
            comando.Parameters.AddWithValue("@codigo", pedido.Codigo);
            comando.ExecuteNonQuery();

            foreach (ItemJson item in pedido.Itens)
            {
                comando.Parameters.Clear();
                comando.CommandText = "SELECT preco FROM produtos WHERE id=@id FOR UPDATE";
                comando.Parameters.AddWithValue("@id", item.ProdutoId);
                object? preco = comando.ExecuteScalar();
                if (preco == null || Convert.ToDecimal(preco) != item.PrecoUnitario)
                    throw new InvalidDataException("Produto " + item.ProdutoId + ": ausente ou com preço diferente. Atualize o site e gere outro arquivo.");
            }

            comando.Parameters.Clear();
            comando.CommandText = "SELECT id FROM clientes WHERE cpf=@cpf FOR UPDATE";
            comando.Parameters.AddWithValue("@cpf", pedido.Cliente.Cpf);
            object? existente = comando.ExecuteScalar();
            long clienteId;
            if (existente != null)
            {
                clienteId = Convert.ToInt64(existente);
                comando.CommandText = "SELECT email FROM clientes WHERE id=@id";
                comando.Parameters.Clear();
                comando.Parameters.AddWithValue("@id", clienteId);
                string email = Convert.ToString(comando.ExecuteScalar()) ?? "";
                if (!email.Equals(pedido.Cliente.Email, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("CPF já cadastrado com outro e-mail. Confira o cliente antes de importar.");
            }
            else
            {
                comando.CommandText = "INSERT INTO clientes (nome,email,cpf,telefone,endereco) VALUES (@nome,@email,@cpf,@telefone,@endereco)";
                comando.Parameters.Clear();
                comando.Parameters.AddWithValue("@nome", pedido.Cliente.Nome);
                comando.Parameters.AddWithValue("@email", pedido.Cliente.Email);
                comando.Parameters.AddWithValue("@cpf", pedido.Cliente.Cpf);
                comando.Parameters.AddWithValue("@telefone", pedido.Cliente.Telefone);
                comando.Parameters.AddWithValue("@endereco", pedido.Cliente.Endereco);
                comando.ExecuteNonQuery();
                clienteId = comando.LastInsertedId;
            }

            comando.Parameters.Clear();
            comando.CommandText = "INSERT INTO pedidos (cliente_id,codigo_origem,endereco_entrega,valor_total) VALUES (@cliente,@codigo,@endereco,@total)";
            comando.Parameters.AddWithValue("@cliente", clienteId);
            comando.Parameters.AddWithValue("@codigo", pedido.Codigo);
            comando.Parameters.AddWithValue("@endereco", pedido.EnderecoEntrega);
            comando.Parameters.AddWithValue("@total", pedido.ValorTotal);
            comando.ExecuteNonQuery();
            long pedidoId = comando.LastInsertedId;

            foreach (ItemJson item in pedido.Itens)
            {
                comando.Parameters.Clear();
                comando.CommandText = "INSERT INTO itens_pedido (pedido_id,produto_id,quantidade,preco_unitario) VALUES (@pedido,@produto,@quantidade,@preco)";
                comando.Parameters.AddWithValue("@pedido", pedidoId);
                comando.Parameters.AddWithValue("@produto", item.ProdutoId);
                comando.Parameters.AddWithValue("@quantidade", item.Quantidade);
                comando.Parameters.AddWithValue("@preco", item.PrecoUnitario);
                comando.ExecuteNonQuery();
            }
            transacao.Commit();
        }
        catch
        {
            transacao.Rollback();
            throw;
        }
    }

    public static void Cancelar(int id)
    {
        using var conexao = Conexao.Abrir();
        using var transacao = conexao.BeginTransaction();
        using var comando = new MySqlCommand("SELECT status FROM pedidos WHERE id=@id FOR UPDATE", conexao, transacao);
        comando.Parameters.AddWithValue("@id", id);
        object? status = comando.ExecuteScalar();
        if (status == null) throw new InvalidDataException("Pedido não encontrado.");
        if (Convert.ToString(status) == "Entregue") throw new InvalidDataException("Pedido entregue não pode ser cancelado.");
        comando.CommandText = "DELETE FROM itens_pedido WHERE pedido_id=@id";
        comando.ExecuteNonQuery();
        comando.CommandText = "DELETE FROM pedidos WHERE id=@id";
        comando.ExecuteNonQuery();
        transacao.Commit();
    }
}
