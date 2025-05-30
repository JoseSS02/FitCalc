using FitCalc.Pages;

namespace FitCalc;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("login", typeof(Login));

    }
}