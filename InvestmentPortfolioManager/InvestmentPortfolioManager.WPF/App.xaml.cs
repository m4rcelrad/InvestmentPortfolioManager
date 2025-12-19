using InvestmentPortfolioManager.Core.Enums;
using InvestmentPortfolioManager.Core.Interfaces;
using InvestmentPortfolioManager.Core.Models;
using InvestmentPortfolioManager.WPF.MVVM;
using System.Configuration;
using System.Data;
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

            IDataService testDataService = new MockDataService();

            MainViewModel mainVM = new MainViewModel(testDataService);

            MainWindow window = new MainWindow();

            window.DataContext = mainVM;

            window.Show();
        }
    }

    // Do podmiany jak będzie gotowa baza danych
    public class MockDataService : IDataService
    {
        public void SavePortfolio(List<Asset> assets) { }

        public List<Asset> LoadPortfolio()
        {
            return new List<Asset>();
        }

        public List<Asset> GetFilteredAssets(double? min, double? max, RiskEnum? risk, string? name)
        {
            return new List<Asset>();
        }
    }

}
