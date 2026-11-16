using System.Security.Cryptography;

namespace UrbanBurger;
public static class Senhas
{
    public static string Criar(string senha)
    {
        if (senha.Length < 8 || senha.Length > 128)
            throw new InvalidDataException("A senha deve ter de 8 a 128 caracteres.");
        byte[] sal = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(senha, sal, 600000, HashAlgorithmName.SHA256, 32);
        return Convert.ToBase64String(sal) + ":" + Convert.ToBase64String(hash);
    }

    public static bool Conferir(string senha, string valorSalvo)
    {
        try
        {
            string[] partes = valorSalvo.Split(':');
            if (partes.Length != 2) return false;
            byte[] sal = Convert.FromBase64String(partes[0]);
            byte[] esperado = Convert.FromBase64String(partes[1]);
            if (sal.Length != 16 || esperado.Length != 32) return false;
            byte[] recebido = Rfc2898DeriveBytes.Pbkdf2(senha, sal, 600000, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(esperado, recebido);
        }
        catch (FormatException) { return false; }
    }
}
