using InvestmentPortfolioManager.Core.Enums;
using InvestmentPortfolioManager.Core.Models;
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

        public void UpdatePortfolio(InvestmentPortfolio newPortfolio)
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
        private readonly FileDataService _dataService;

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

        public ICommand FilterCommand { get; }
        public ICommand ClearFiltersCommand { get; }

        public PortfolioViewModel(InvestmentPortfolio portfolio)
        {
            _dataService = new FileDataService();
            _portfolio = portfolio;

            UpdatePortfolio(portfolio);

            FilterCommand = new RelayCommand(o => ApplyFilters());
            ClearFiltersCommand = new RelayCommand(o => ClearFilters());
        }

        public void UpdatePortfolio(InvestmentPortfolio newPortfolio)
        {
            _portfolio = newPortfolio;
            ClearFilters();
        }

        private void ApplyFilters()
        {
            if (_portfolio == null) return;

            var filtered = _dataService.GetFilteredAssets(
                _portfolio,
                MinPriceFilter,
                MaxPriceFilter,
                SelectedRiskFilter,
                SearchText);

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
    }

    public class MainViewModel : ViewModelBase
    {
        private readonly FileDataService _dataService;
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
                    _selectedPortfolio = value;
                    OnPropertyChanged();

                    DashboardVM.UpdatePortfolio(_selectedPortfolio);
                    PortfolioVM.UpdatePortfolio(_selectedPortfolio);
                }
            }
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
            _dataService = new FileDataService();

            _simulationTimer = new DispatcherTimer();
            InitializeSimulation();

            var loadedPortfolios = _dataService.LoadAllPortfolios();

            AllPortfolios = new ObservableCollection<InvestmentPortfolio>(loadedPortfolios);

            _selectedPortfolio = AllPortfolios.First();

            DashboardVM = new DashboardViewModel(_selectedPortfolio);
            PortfolioVM = new PortfolioViewModel(_selectedPortfolio);

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