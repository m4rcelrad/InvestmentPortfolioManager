using InvestmentPortfolioManager.Core.Enums;
using InvestmentPortfolioManager.Core.Models;
using InvestmentPortfolioManager.Data;
using InvestmentPortfolioManager.WPF.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace InvestmentPortfolioManager.WPF.MVVM
{
    public class DashboardViewModel : ViewModelBase
    {
        private InvestmentPortfolio? _portfolio;

        public double TotalValue => _portfolio?.CalculateSum() ?? 0;
        public double TotalProfit => _portfolio?.CalculateTotalProfit() ?? 0;

        public bool IsProfitPositive => TotalProfit >= 0;

        public ObservableCollection<Asset> TopMovers { get; set; } = [];

        public Dictionary<string, double> Allocation { get; set; } = [];

        public DashboardViewModel(InvestmentPortfolio portfolio)
        {
            UpdatePortfolio(portfolio);
        }

        public void UpdatePortfolio(InvestmentPortfolio? newPortfolio)
        {
            _portfolio = newPortfolio;
            RecalculateStats();
        }

        public void RecalculateStats()
        {
            if (_portfolio == null) return;

            OnPropertyChanged(nameof(TotalValue));
            OnPropertyChanged(nameof(TotalProfit));
            OnPropertyChanged(nameof(IsProfitPositive));

            var movers = _portfolio.GetTopMovers(3);
            TopMovers = new ObservableCollection<Asset>(movers);
            OnPropertyChanged(nameof(TopMovers));

            Allocation = _portfolio.GetAssetAllocation();
            OnPropertyChanged(nameof(Allocation));
        }
    }

    public class PortfolioViewModel : ViewModelBase
    {
        private readonly SqlDatabaseService _dataService;

        private InvestmentPortfolio? _portfolio;

        private ObservableCollection<Asset> _assets = [];
        public ObservableCollection<Asset> Assets
        {
            get => _assets;
            set { _assets = value; OnPropertyChanged(); }
        }

        public string SearchText { get; set; } = string.Empty;

        public double? MinPriceFilter { get; set; }
        public double? MaxPriceFilter { get; set; }
        public RiskEnum? SelectedRiskFilter { get; set; }

        public IEnumerable<RiskEnum> RiskLevels => Enum.GetValues(typeof(RiskEnum)).Cast<RiskEnum>();

        public ICommand FilterCommand { get; }
        public ICommand ClearFiltersCommand { get; }

        public PortfolioViewModel(InvestmentPortfolio portfolio)
        {
            _dataService = new SqlDatabaseService();
            _portfolio = portfolio;

            UpdatePortfolio(portfolio);

            FilterCommand = new RelayCommand(o => ApplyFilters());
            ClearFiltersCommand = new RelayCommand(o => ClearFilters());

            AddAssetCommand = new RelayCommand(o => AddAsset());
            RemoveAssetCommand = new RelayCommand(o => RemoveAsset());
        }

        public void UpdatePortfolio(InvestmentPortfolio? newPortfolio)
        {
            _portfolio = newPortfolio;
            ClearFilters();
        }

        private void ApplyFilters()
        {
            if (_portfolio == null) return;

            Func<Asset, bool> filterPredicate = asset =>
            {
                bool matchesSearch = string.IsNullOrWhiteSpace(SearchText) ||
                             asset.AssetName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                             asset.AssetSymbol.Contains(SearchText, StringComparison.OrdinalIgnoreCase);

                bool matchesMinPrice = !MinPriceFilter.HasValue || asset.CurrentPrice >= MinPriceFilter.Value;

                bool matchesMaxPrice = !MaxPriceFilter.HasValue || asset.CurrentPrice <= MaxPriceFilter.Value;

                bool matchesRisk = !SelectedRiskFilter.HasValue || asset.GetRiskAssessment() == SelectedRiskFilter.Value;

                return matchesSearch && matchesMinPrice && matchesMaxPrice && matchesRisk;
            };

            var filtered = _portfolio.FindAssets(filterPredicate);

            Assets = new ObservableCollection<Asset>(filtered);
        }

        private void ClearFilters()
        {
            SearchText = string.Empty;
            MinPriceFilter = null;
            MaxPriceFilter = null;
            SelectedRiskFilter = null;

            OnPropertyChanged(nameof(SearchText));
            OnPropertyChanged(nameof(MinPriceFilter));
            OnPropertyChanged(nameof(MaxPriceFilter));
            OnPropertyChanged(nameof(SelectedRiskFilter));

            if (_portfolio != null)
            {
                Assets = new ObservableCollection<Asset>(_portfolio.Assets);
            }
        }

        public void RefreshView()
        {
            if (Assets != null)
            {
                CollectionViewSource.GetDefaultView(Assets).Refresh();
            }
        }

        private Asset? _selectedAsset;
        public Asset? SelectedAsset
        {
            get => _selectedAsset;
            set { _selectedAsset = value; OnPropertyChanged(); }
        }

        public ICommand AddAssetCommand { get; }
        public ICommand RemoveAssetCommand { get; }

        private void AddAsset()
        {
            if (_portfolio == null) return;

            var addWindow = new AddAssetWindow();
            if (addWindow.ShowDialog() == true && addWindow.CreatedAsset != null)
            {
                _portfolio.AddNewAsset(addWindow.CreatedAsset);
                ApplyFilters();
            }
        }

        private void RemoveAsset()
        {
            if (_portfolio == null || SelectedAsset == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete {SelectedAsset.AssetName}?",
                                         "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _portfolio.RemoveAsset(SelectedAsset);
                ApplyFilters();
            }
        }

    }

    public class MainViewModel : ViewModelBase
    {
        private readonly SqlDatabaseService _dataService;
        private object _currentView;

        private DispatcherTimer _simulationTimer;
        private bool _isSimulationRunning;
        private DateTime _simulationDate = DateTime.Now;

        public ObservableCollection<InvestmentPortfolio> AllPortfolios { get; set; } = [];

        private InvestmentPortfolio _selectedPortfolio = null!;

        public InvestmentPortfolio SelectedPortfolio
        {
            get => _selectedPortfolio;
            set
            {
                if (_selectedPortfolio != value)
                {
                    if (_selectedPortfolio != null)
                    {
                        foreach (var asset in _selectedPortfolio.Assets)
                        {
                            asset.OnCriticalDrop -= HandleCriticalDrop;
                        }
                        _selectedPortfolio.Assets.CollectionChanged -= OnAssetsCollectionChanged;
                    }

                    _selectedPortfolio = value;
                    OnPropertyChanged();

                    if (_selectedPortfolio != null)
                    {
                        foreach (var asset in _selectedPortfolio.Assets)
                        {
                            asset.OnCriticalDrop += HandleCriticalDrop;
                        }
                        _selectedPortfolio.Assets.CollectionChanged += OnAssetsCollectionChanged;
                    }

                    DashboardVM.UpdatePortfolio(_selectedPortfolio);
                    PortfolioVM.UpdatePortfolio(_selectedPortfolio);
                }
            }
        }

        private void OnAssetsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Asset asset in e.NewItems)
                    asset.OnCriticalDrop += HandleCriticalDrop;
            }

            if (e.OldItems != null)
            {
                foreach (Asset asset in e.OldItems)
                    asset.OnCriticalDrop -= HandleCriticalDrop;
            }
        }

        private void HandleCriticalDrop(string symbol, double price, string message)
        {
            if (_isSimulationRunning)
            {
                ToggleSimulation();
            }

            MessageBox.Show(message, $"ALERT: {symbol}", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public DashboardViewModel DashboardVM { get; set; } = null!;
        public PortfolioViewModel PortfolioVM { get; set; } = null!;

        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public string SimulationButtonText => _isSimulationRunning ? "Stop Simulation" : "Start Simulation";
        public string SimulationButtonColor => _isSimulationRunning ? "#E74C3C" : "#3498DB";

        public ICommand SwitchViewCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ToggleSimulationCommand { get; }

        public ICommand CreatePortfolioCommand { get; }
        public ICommand ClonePortfolioCommand { get; }
        public ICommand DeletePortfolioCommand { get; }

        public MainViewModel()
        {
            _dataService = new SqlDatabaseService();

            _simulationTimer = new DispatcherTimer();
            InitializeSimulation();

            var loadedPortfolios = _dataService.LoadAllPortfolios();

            AllPortfolios = new ObservableCollection<InvestmentPortfolio>(loadedPortfolios);

            if (AllPortfolios.Count == 0)
            {
                var defaultPortfolio = new InvestmentPortfolio
                {
                    Name = "Default Portfolio",
                    Owner = "Admin User"
                };
                AllPortfolios.Add(defaultPortfolio);

                _dataService.SavePortfolios(new List<InvestmentPortfolio> { defaultPortfolio });
            }

            var initialPortfolio = AllPortfolios.First();
            DashboardVM = new DashboardViewModel(initialPortfolio);
            PortfolioVM = new PortfolioViewModel(initialPortfolio);
            SelectedPortfolio = initialPortfolio;

            _currentView = DashboardVM;
            CurrentView = DashboardVM;

            SwitchViewCommand = new RelayCommand(o =>
            {
                if (o is string viewName)
                {
                    if (viewName == "Dashboard") CurrentView = DashboardVM;
                    else if (viewName == "Portfolio") CurrentView = PortfolioVM;
                }
            });

            SaveCommand = new RelayCommand(o =>
            {
                _dataService.SavePortfolios(new List<InvestmentPortfolio>(AllPortfolios));
                MessageBox.Show("Saved Successfully", "Success");
            });

            CreatePortfolioCommand = new RelayCommand(o => CreatePortfolio());
            ClonePortfolioCommand = new RelayCommand(o => ClonePortfolio());
            DeletePortfolioCommand = new RelayCommand(o => DeletePortfolio());

            ToggleSimulationCommand = new RelayCommand(o => ToggleSimulation());
        }

        private void InitializeSimulation()
        {
            _simulationTimer.Interval = TimeSpan.FromSeconds(5);
            _simulationTimer.Tick += SimulationTimer_Tick;
        }

        private void ToggleSimulation()
        {
            if (_isSimulationRunning)
            {
                _simulationTimer.Stop();
                _isSimulationRunning = false;
            }
            else
            {
                _simulationTimer.Start();
                _isSimulationRunning = true;
                SimulationTimer_Tick(null, null);
            }

            OnPropertyChanged(nameof(SimulationButtonText));
            OnPropertyChanged(nameof(SimulationButtonColor));
        }

        private void SimulationTimer_Tick(object? sender, EventArgs? e)
        {
            if (SelectedPortfolio == null) return;

            _simulationDate = _simulationDate.AddDays(1);

            SelectedPortfolio.UpdateMarketPrices(_simulationDate);

            DashboardVM.RecalculateStats();
            PortfolioVM.RefreshView();
        }

        private void CreatePortfolio()
        {
            var newPortfolio = new InvestmentPortfolio
            {
                Name = $"Portfolio {AllPortfolios.Count + 1}",
                Owner = "New Investor"
            };

            AllPortfolios.Add(newPortfolio);
            SelectedPortfolio = newPortfolio;
        }

        private void ClonePortfolio()
        {
            if (SelectedPortfolio == null) return;

            var clone = (InvestmentPortfolio)SelectedPortfolio.Clone();

            AllPortfolios.Add(clone);
            SelectedPortfolio = clone;
        }

        private void DeletePortfolio()
        {
            if (SelectedPortfolio == null) return;

            var toRemove = SelectedPortfolio;

            if (AllPortfolios.Count > 1)
            {
                var index = AllPortfolios.IndexOf(toRemove);
                SelectedPortfolio = index > 0 ? AllPortfolios[index - 1] : AllPortfolios[index + 1];
            }
            else
            {
                CreatePortfolio();
            }

            AllPortfolios.Remove(toRemove);
        }
    }
}