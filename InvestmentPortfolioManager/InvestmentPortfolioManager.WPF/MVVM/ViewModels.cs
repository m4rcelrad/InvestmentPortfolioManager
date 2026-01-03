using InvestmentPortfolioManager.Core.Enums;
using InvestmentPortfolioManager.Core.Interfaces;
using InvestmentPortfolioManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace InvestmentPortfolioManager.WPF.MVVM
{
    // --- DASHBOARD ---
    public class DashboardViewModel : ViewModelBase
    {
        private readonly InvestmentPortfolio _portfolio;

        // Bindowanie bezpośrednio do Portfolio z modelu
        public InvestmentPortfolio Portfolio => _portfolio;

        public double TotalValue => _portfolio.CalculateSum();

        public string PortfolioRiskLevel
        {
            get
            {
                double totalValue = TotalValue;
                if (totalValue == 0) return "Brak danych";
                double weightedRiskSum = 0;
                foreach (var asset in _portfolio.Assets)
                {
                    int riskWeight = asset.GetRiskAssessment() switch
                    {
                        RiskEnum.Low => 1,
                        RiskEnum.Medium => 2,
                        RiskEnum.High => 3,
                        RiskEnum.ExtremelyHigh => 4,
                        _ => 0
                    };
                    weightedRiskSum += riskWeight * asset.Value;
                }
                double score = weightedRiskSum / totalValue;
                return score <= 1.5 ? "NISKIE" : score <= 2.5 ? "ŚREDNIE" : score <= 3.5 ? "WYSOKIE" : "BARDZO WYSOKIE";
            }
        }

        public DashboardViewModel(InvestmentPortfolio portfolio)
        {
            _portfolio = portfolio;
            if (_portfolio.Assets is INotifyCollectionChanged collection)
                collection.CollectionChanged += (s, e) => RefreshCalculatedFields();
        }

        public void RefreshCalculatedFields()
        {
            OnPropertyChanged(nameof(TotalValue));
            OnPropertyChanged(nameof(PortfolioRiskLevel));
        }
    }

    // --- PORTFOLIO ---
    public class PortfolioViewModel : ViewModelBase
    {
        private readonly InvestmentPortfolio _portfolio;

        public ReadOnlyObservableCollection<Asset> Assets => _portfolio.Assets;

        private Asset? _selectedAsset;
        public Asset? SelectedAsset { get => _selectedAsset; set { _selectedAsset = value; OnPropertyChanged(); } }

        public ICommand RemoveAssetCommand { get; }

        public PortfolioViewModel(InvestmentPortfolio portfolio)
        {
            _portfolio = portfolio;
            RemoveAssetCommand = new RelayCommand(o => {
                if (SelectedAsset != null) _portfolio.RemoveAsset(SelectedAsset);
            }, o => SelectedAsset != null);
        }
    }

    // --- MAIN ---
    public class MainViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        public InvestmentPortfolio Portfolio { get; } = new();

        private object _currentView;
        public object CurrentView { get => _currentView; set { _currentView = value; OnPropertyChanged(); } }

        private readonly DashboardViewModel _dashboardVM;
        private readonly PortfolioViewModel _portfolioVM;

        public ICommand ShowDashboardCommand { get; }
        public ICommand ShowPortfolioCommand { get; }
        public ICommand SimulatePricesCommand { get; }
        public ICommand SaveDataCommand { get; }
        public ICommand LoadDataCommand { get; }

        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;
            _dashboardVM = new DashboardViewModel(Portfolio);
            _portfolioVM = new PortfolioViewModel(Portfolio);
            _currentView = _dashboardVM;

            ShowDashboardCommand = new RelayCommand(o => CurrentView = _dashboardVM);
            ShowPortfolioCommand = new RelayCommand(o => CurrentView = _portfolioVM);
            SaveDataCommand = new RelayCommand(o => _dataService.SavePortfolio(Portfolio.Assets.ToList()));
            LoadDataCommand = new RelayCommand(o => LoadPortfolio());

            // Zgodnie z instrukcją: wywołujemy UpdateMarketPrices
            SimulatePricesCommand = new RelayCommand(o => {
                Portfolio.UpdateMarketPrices(DateTime.Now);
                // Musimy powiadomić VM, że suma portfela mogła się zmienić
                _dashboardVM.RefreshCalculatedFields();
            });

            // MOCKOWANIE DANYCH (SZYBKI START) - zgodnie z instrukcją
            Portfolio.AddNewAsset(new Stock("Apple", "AAPL", 10, 150.0));
            Portfolio.AddNewAsset(new Bond("Obligacje", "BOND1", 5, 1000.0, 0.05));
            Portfolio.AddNewAsset(new Cryptocurrency("Bitcoin", "BTC", 0.5, 45000.0));

            LoadPortfolio();
        }

        private void LoadPortfolio()
        {
            try
            {
                var assets = _dataService.LoadPortfolio();
                if (assets != null && assets.Any())
                {
                    var toRemove = Portfolio.Assets.ToList();
                    foreach (var a in toRemove) Portfolio.RemoveAsset(a);
                    foreach (var a in assets) Portfolio.AddNewAsset(a);
                    _dashboardVM.RefreshCalculatedFields();
                }
            }
            catch { /* Ciche ładowanie jeśli plik nie istnieje */ }
        }
    }
}