using InvestmentPortfolioManager.Core.Interfaces;
using InvestmentPortfolioManager.WPF.MVVM;
using System.Windows;

namespace InvestmentPortfolioManager.WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Tworzymy implementację IDataService, np. do testów możesz użyć mocka
            IDataService dataService = new FileDataService(); // lub inna implementacja
            DataContext = new MainViewModel(dataService);
        }
    }
}
