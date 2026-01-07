using InvestmentPortfolioManager.Core.Enums;
using InvestmentPortfolioManager.Core.Interfaces;
using InvestmentPortfolioManager.Core.Models;
using InvestmentPortfolioManager.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace InvestmentPortfolioManager.WPF.MVVM
{
    // --- DASHBOARD VIEW MODEL ---
    public class DashboardViewModel : ViewModelBase
    {
        private readonly InvestmentPortfolio _portfolio;

        // Monitoring "Live" - Zagregowane dane ze słownika w Portfolio
        public IEnumerable<LiveAssetSummary> AssetSummaries => _portfolio.PortfolioSummaries.Values;

        public double TotalValue => _portfolio.CalculateSum();

        public Dictionary<string, double> AssetAllocation =>
            TotalValue == 0 ? new Dictionary<string, double>() :
            _portfolio.Assets
                .GroupBy(a => a.GetType().Name)
                .ToDictionary(g => g.Key, g => g.Sum(a => a.Value));

        public IEnumerable<Asset> TopValuedAssets => _portfolio.Assets
            .OrderByDescending(a => a.Value)
            .Take(3)
            .ToList();

        public string PortfolioRiskLevel
        {
            get
            {
                double total = TotalValue;
                if (total <= 0) return "Brak aktywów";

                double weightedRisk = _portfolio.Assets
                    .Sum(a => GetRiskWeight(a.GetRiskAssessment()) * a.Value);

                return (weightedRisk / total) switch
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
            // Odświeżanie przy zmianach w kolekcji (dodanie/usunięcie aktywa)
            ((INotifyCollectionChanged)_portfolio.Assets).CollectionChanged += (s, e) => RefreshData();
        }

        public void RefreshData()
        {
            OnPropertyChanged(nameof(TotalValue));
            OnPropertyChanged(nameof(AssetSummaries));
            OnPropertyChanged(nameof(AssetAllocation));
            OnPropertyChanged(nameof(TopValuedAssets));
            OnPropertyChanged(nameof(PortfolioRiskLevel));
        }

        private int GetRiskWeight(RiskEnum risk) => risk switch
        {
            RiskEnum.Low => 1,
            RiskEnum.Medium => 2,
            RiskEnum.High => 3,
            RiskEnum.ExtremelyHigh => 4,
            _ => 0
        };
    }

    // --- PORTFOLIO VIEW MODEL ---
    public class PortfolioViewModel : ViewModelBase
    {
        private readonly InvestmentPortfolio _portfolio;
        private readonly ICollectionView _assetsView;
        public ICollectionView AssetsView => _assetsView;

        private string _filterText = string.Empty;
        public string FilterText
        {
            get => _filterText;
            set
            {
                _filterText = value;
                OnPropertyChanged();
                _assetsView.Refresh();
            }
        }

        private Asset? _selectedAsset;
        public Asset? SelectedAsset
        {
            get => _selectedAsset;
            set
            {
                _selectedAsset = value;
                OnPropertyChanged();
                // Powiadomienie o zmianie SelectedAsset pozwala wykresowi w GUI na odświeżenie danych z PriceHistory
                CommandManager.InvalidateRequerySuggested();
            }
        }

        // Komendy
        public ICommand AddTestAssetCommand { get; }
        public ICommand RemoveAssetCommand { get; }
        public ICommand FilterHighRiskCommand { get; }
        public ICommand SortByValueCommand { get; }
        public ICommand ResetFilterCommand { get; }

        public PortfolioViewModel(InvestmentPortfolio portfolio)
        {
            _portfolio = portfolio;
            _assetsView = CollectionViewSource.GetDefaultView(_portfolio.Assets);

            // Podstawowa logika filtrowania tekstowego
            _assetsView.Filter = o =>
            {
                if (string.IsNullOrWhiteSpace(FilterText)) return true;
                if (o is Asset a)
                {
                    return a.AssetName.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
                           a.AssetSymbol.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
                }
                return false;
            };

            // Inicjalizacja komend
            AddTestAssetCommand = new RelayCommand(ExecuteAddTestAsset);
            RemoveAssetCommand = new RelayCommand(ExecuteRemoveAsset, o => SelectedAsset != null);

            // 1. Filtrowanie po ryzyku (Punkt 3 Twojej listy)
            FilterHighRiskCommand = new RelayCommand(o =>
            {
                FilterText = string.Empty;
                _assetsView.Filter = item => item is Asset a &&
                    (a.GetRiskAssessment() == RiskEnum.High || a.GetRiskAssessment() == RiskEnum.ExtremelyHigh);
            });

            // 2. Sortowanie po wartości (Punkt 3 Twojej listy)
            SortByValueCommand = new RelayCommand(o =>
            {
                _assetsView.SortDescriptions.Clear();
                _assetsView.SortDescriptions.Add(new SortDescription(nameof(Asset.Value), ListSortDirection.Descending));
            });

            // 3. Resetowanie widoku
            ResetFilterCommand = new RelayCommand(o =>
            {
                _assetsView.Filter = null; // Reset filtra (powrót do pełnej listy)
                _assetsView.SortDescriptions.Clear();
                FilterText = string.Empty;
            });
        }

        private void ExecuteAddTestAsset(object obj)
        {
            var r = new Random();
            double price = r.Next(100, 1000);
            var asset = new Stock($"Akcja {r.Next(1, 100)}", "TEST", 10, price)
            {
                LowPriceThreshold = price * 0.90
            };
            _portfolio.AddNewAsset(asset);
        }

        private void ExecuteRemoveAsset(object obj)
        {
            if (SelectedAsset != null && MessageBox.Show($"Usunąć {SelectedAsset.AssetName}?", "Potwierdzenie", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                _portfolio.RemoveAsset(SelectedAsset);
        }
    }

    // --- MAIN VIEW MODEL ---
    public class MainViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        public InvestmentPortfolio Portfolio { get; } = new();

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
        public ICommand SimulatePricesCommand { get; }
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

            SimulatePricesCommand = new RelayCommand(o => {
                Portfolio.UpdateMarketPrices(DateTime.Now);
                // Po symulacji musimy odświeżyć Dashboard (TotalValue, Summaries itp.)
                _dashboardVM.RefreshData();
            });

            ((INotifyCollectionChanged)Portfolio.Assets).CollectionChanged += OnAssetsCollectionChanged;

            LoadPortfolio();
        }

        private void OnAssetsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (Asset asset in e.NewItems)
                    asset.OnCriticalDrop += HandleCriticalDrop;

            if (e.OldItems != null)
                foreach (Asset asset in e.OldItems)
                    asset.OnCriticalDrop -= HandleCriticalDrop;
        }

        private void HandleCriticalDrop(string symbol, double price, string message)
        {
            MessageBox.Show($"ALARM: {symbol} zaliczył krytyczny spadek!\nCena: {price:C}\nInfo: {message}",
                            "Krytyczna zmiana ceny", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void LoadPortfolio()
        {
            try
            {
                var loaded = _dataService.LoadPortfolio();
                if (loaded != null)
                {
                    var existing = Portfolio.Assets.ToList();
                    foreach (var a in existing) Portfolio.RemoveAsset(a);
                    foreach (var a in loaded) Portfolio.AddNewAsset(a);
                }
            }
            catch (Exception ex) { MessageBox.Show($"Błąd odczytu: {ex.Message}"); }
        }

        private void SavePortfolio()
        {
            try
            {
                _dataService.SavePortfolio(Portfolio.Assets.ToList());
                MessageBox.Show("Dane zapisane pomyślnie.");
            }
            catch (Exception ex) { MessageBox.Show($"Błąd zapisu: {ex.Message}"); }
        }
    }
}