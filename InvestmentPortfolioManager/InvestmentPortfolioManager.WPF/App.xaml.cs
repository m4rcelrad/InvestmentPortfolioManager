using InvestmentPortfolioManager.WPF.MVVM;
using System.Windows;

namespace InvestmentPortfolioManager.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
           
            MainViewModel mainVM = new MainViewModel();

            MainWindow window = new MainWindow();

            window.DataContext = mainVM;

            window.Show();
        }
    }
}