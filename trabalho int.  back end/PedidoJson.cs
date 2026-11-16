using System.Net.Mail;
using System.Text.Json;

namespace UrbanBurger;
public class ClienteJson
{
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
    public string Cpf { get; set; } = "";
    public string Telefone { get; set; } = "";
    public string Endereco { get; set; } = "";
}
public class ItemJson
{
    public int ProdutoId { get; set; }
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
}
public class PedidoJson
{
    public string Codigo { get; set; } = "";
    public ClienteJson Cliente { get; set; } = new ClienteJson();
    public string EnderecoEntrega { get; set; } = "";
    public List<ItemJson> Itens { get; set; } = new List<ItemJson>();
    public decimal ValorTotal { get; set; }
}

public static class LeituraPedido
{
    public static void Texto(string? valor, string campo, int tamanho)
    {
        if (string.IsNullOrWhiteSpace(valor) || valor.Length > tamanho)
            throw new InvalidDataException(campo + ": preencha até " + tamanho + " caracteres.");
    }

    public static void ValidarCliente(ClienteJson cliente)
    {
        if (cliente == null) throw new InvalidDataException("Cliente ausente.");
        Texto(cliente.Nome, "Nome", 80);
        Texto(cliente.Email, "E-mail", 100);
        if (!MailAddress.TryCreate(cliente.Email, out var email) || email.Address != cliente.Email)
            throw new InvalidDataException("E-mail inválido.");
        Texto(cliente.Cpf, "CPF", 14);
        Texto(cliente.Telefone, "Telefone", 15);
        Texto(cliente.Endereco, "Endereço", 200);
        string cpf = cliente.Cpf.Replace(".", "").Replace("-", "");
        string telefone = cliente.Telefone.Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", "");
        if (!SoDigitos(cpf, 11) || !SoDigitos(telefone, 11))
            throw new InvalidDataException("CPF e celular devem conter 11 dígitos.");
        cliente.Cpf = cpf.Substring(0, 3) + "." + cpf.Substring(3, 3) + "." + cpf.Substring(6, 3) + "-" + cpf.Substring(9);
        cliente.Email = cliente.Email.Trim().ToLowerInvariant();
    }

    private static bool SoDigitos(string texto, int tamanho)
    {
        if (texto.Length != tamanho) return false;
        foreach (char letra in texto)
            if (letra < '0' || letra > '9') return false;
        return true;
    }

    public static PedidoJson Ler(string arquivo)
    {
        if (new FileInfo(arquivo).Length > 1024 * 1024)
            throw new InvalidDataException("Arquivo muito grande. Limite: 1 MB.");
        string texto = File.ReadAllText(arquivo);
        var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, MaxDepth = 16 };
        PedidoJson? pedido = JsonSerializer.Deserialize<PedidoJson>(texto, opcoes);
        if (pedido == null) throw new InvalidDataException("Arquivo sem pedido.");
        Validar(pedido);
        return pedido;
    }

    public static void Validar(PedidoJson pedido)
    {
        Texto(pedido.Codigo, "Código", 64);
        ValidarCliente(pedido.Cliente);
        Texto(pedido.EnderecoEntrega, "Endereço de entrega", 200);
        if (pedido.Itens == null || pedido.Itens.Count < 1 || pedido.Itens.Count > 100)
            throw new InvalidDataException("O pedido deve ter entre 1 e 100 itens.");
        decimal total = 0;
        var ids = new List<int>();
        foreach (ItemJson item in pedido.Itens)
        {
            if (item == null || item.ProdutoId <= 0 || ids.Contains(item.ProdutoId) ||
                item.Quantidade < 1 || item.Quantidade > 99 || item.PrecoUnitario <= 0 ||
                item.PrecoUnitario > 99999999.99m || decimal.Round(item.PrecoUnitario, 2) != item.PrecoUnitario)
                throw new InvalidDataException("Item inválido ou produto repetido.");
            ids.Add(item.ProdutoId);
            total += item.PrecoUnitario * item.Quantidade;
        }
        if (total > 99999999.99m || total != pedido.ValorTotal)
            throw new InvalidDataException("O total não corresponde aos itens.");
    }
}
