namespace UrbanBurger;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        bool primeiroAcesso = args.Length == 1 && args[0] == "--criar-gerente";
        using var login = new LoginForm(primeiroAcesso);
        if (login.ShowDialog() == DialogResult.OK && !primeiroAcesso)
            Application.Run(new PrincipalForm());
    }
}
