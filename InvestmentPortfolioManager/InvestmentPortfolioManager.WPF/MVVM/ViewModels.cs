using InvestmentPortfolioManager.Core.Enums;
using InvestmentPortfolioManager.Core.Interfaces;
using InvestmentPortfolioManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace InvestmentPortfolioManager.WPF.MVVM
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly InvestmentPortfolio _portfolio;

        public double TotalValue => _portfolio.CalculateSum();

        public Dictionary<string, double> AssetAllocation
        {
            get
            {
                var total = TotalValue;
                if (total == 0) return new Dictionary<string, double>();

                return _portfolio.Assets
                    .GroupBy(a => a.GetType().Name)
                    .ToDictionary(g => g.Key, g => g.Sum(a => a.Value));
            }
        }

        public IEnumerable<Asset> TopValuedAssets => _portfolio.Assets
            .OrderByDescending(a => a.Value)
            .Take(3);

        public string PortfolioRiskLevel
        {
            get
            {
                double totalValue = TotalValue;
                if (totalValue == 0) return "Brak aktywów (N/A)";

                double weightedRiskSum = 0;

                foreach (var asset in _portfolio.Assets)
                {
                    int riskWeight = GetRiskWeight(asset.GetRiskAssessment());
                    weightedRiskSum += riskWeight * asset.Value;
                }

                double averageRiskScore = weightedRiskSum / totalValue;

                return averageRiskScore switch
                {
                    <= 1.5 => "NISKIE",
                    <= 2.5 => "ŚREDNIE",
                    <= 3.5 => "WYSOKIE",
                    _ => "BARDZO WYSOKIE"
                };
            }
        }

        public DashboardViewModel(InvestmentPortfolio portfolio)
        {
            _portfolio = portfolio;

            if (_portfolio.Assets is INotifyCollectionChanged collectionChanged)
            {
                collectionChanged.CollectionChanged += (s, e) => RefreshData();
            }
        }

        private void RefreshData()
        {
            OnPropertyChanged(nameof(TotalValue));
            OnPropertyChanged(nameof(AssetAllocation));
            OnPropertyChanged(nameof(TopValuedAssets));
            OnPropertyChanged(nameof(PortfolioRiskLevel));
        }

        private int GetRiskWeight(RiskEnum risk)
        {
            return risk switch
            {
                RiskEnum.Low => 1,
                RiskEnum.Medium => 2,
                RiskEnum.High => 3,
                RiskEnum.ExtremelyHigh => 4,
                _ => 0
            };
        }
    }

    public class PortfolioViewModel : ViewModelBase
    {
        private readonly InvestmentPortfolio _portfolio;

        public ReadOnlyObservableCollection<Asset> Assets => _portfolio.Assets;

        private Asset? _selectedAsset;
        public Asset? SelectedAsset
        {
            get => _selectedAsset;
            set
            {
                _selectedAsset = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand AddTestAssetCommand { get; }
        public ICommand RemoveAssetCommand { get; }

        public PortfolioViewModel(InvestmentPortfolio portfolio)
        {
            _portfolio = portfolio;

            AddTestAssetCommand = new RelayCommand(ExecuteAddTestAsset);
            RemoveAssetCommand = new RelayCommand(ExecuteRemoveAsset, CanRemoveAsset);
        }

        private void ExecuteAddTestAsset(object obj)
        {
            // Test
            var random = new Random();
            var price = random.Next(50, 500);
            var asset = new Stock($"Test Stock {random.Next(1, 100)}", "TST", 10, price);

            try
            {
                _portfolio.AddNewAsset(asset);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd dodawania: {ex.Message}");
            }
        }

        private void ExecuteRemoveAsset(object obj)
        {
            if (SelectedAsset != null)
            {
                var result = MessageBox.Show($"Czy na pewno usunąć {SelectedAsset.AssetName}?", "Potwierdzenie", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    _portfolio.RemoveAsset(SelectedAsset);
                }
            }
        }

        private bool CanRemoveAsset(object obj) => SelectedAsset != null;
    }

    public class MainViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;

        public InvestmentPortfolio Portfolio { get; private set; } = new();

        private object _currentView;

        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        private readonly DashboardViewModel _dashboardVM;
        private readonly PortfolioViewModel _portfolioVM;

        public ICommand ShowDashboardCommand { get; }
        public ICommand ShowPortfolioCommand { get; }
        public ICommand LoadDataCommand { get; }
        public ICommand SaveDataCommand { get; }

        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;

            _dashboardVM = new DashboardViewModel(Portfolio);
            _portfolioVM = new PortfolioViewModel(Portfolio);

            _currentView = _dashboardVM;

            ShowDashboardCommand = new RelayCommand(o => CurrentView = _dashboardVM);
            ShowPortfolioCommand = new RelayCommand(o => CurrentView = _portfolioVM);

            SaveDataCommand = new RelayCommand(o => SavePortfolio());
            LoadDataCommand = new RelayCommand(o => LoadPortfolio());

            LoadPortfolio();
        }

        private void LoadPortfolio()
        {
            try
            {
                var loadedAssets = _dataService.LoadPortfolio();

                if (loadedAssets == null)
                {
                    throw new Exception("Błąd ładowania danych");
                }

                var assetsToRemove = Portfolio.Assets.ToList();
                foreach (var asset in assetsToRemove)
                {
                    Portfolio.RemoveAsset(asset);
                }

                foreach (var asset in loadedAssets)
                {
                    Portfolio.AddNewAsset(asset);
                }

                // Opcjonalnie do testowania na początku - jak będzie działać końcowa aplikacja do usunięcia
                //MessageBox.Show($"Pomyślnie załadowano {loadedAssets.Count} aktywów.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Wystąpił błąd podczas ładowania portfela:\n{ex.Message}",
                    "Błąd Danych",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SavePortfolio()
        {
            try
            {
                _dataService.SavePortfolio(Portfolio.Assets.ToList());
                MessageBox.Show("Zapisano stan portfela!", "Zapis", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Nie udało się zapisać danych:\n{ex.Message}", "Błąd Zapisu", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
