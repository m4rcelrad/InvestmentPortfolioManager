using InvestmentPortfolioManager.Core.Enums;
using InvestmentPortfolioManager.Core.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using System.Collections.Generic;

namespace InvestmentPortfolioManager.WPF.MVVM
{
    public class DashboardViewModel : ViewModelBase
    {
        private InvestmentPortfolio _portfolio;

        public double TotalValue => _portfolio?.CalculateSum() ?? 0;

        public ObservableCollection<Asset> TopMovers { get; set; }

        public Dictionary<string, double> Allocation { get; set; }

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
        private InvestmentPortfolio _portfolio;

        private ObservableCollection<Asset> _assets;
        public ObservableCollection<Asset> Assets
        {
            get => _assets;
            set { _assets = value; OnPropertyChanged(); }
        }

        public string SearchText { get; set; }
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
    }

    public class MainViewModel : ViewModelBase
    {
        private readonly FileDataService _dataService;
        private object _currentView;

        public ObservableCollection<InvestmentPortfolio> AllPortfolios { get; set; }

        private InvestmentPortfolio _selectedPortfolio;
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

        public DashboardViewModel DashboardVM { get; set; }
        public PortfolioViewModel PortfolioVM { get; set; }

        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand SwitchViewCommand { get; }
        public ICommand SaveCommand { get; }

        public MainViewModel()
        {
            _dataService = new FileDataService();

            var loadedPortfolios = _dataService.LoadAllPortfolios();
            AllPortfolios = new ObservableCollection<InvestmentPortfolio>(loadedPortfolios);

            _selectedPortfolio = AllPortfolios.FirstOrDefault();

            DashboardVM = new DashboardViewModel(_selectedPortfolio);
            PortfolioVM = new PortfolioViewModel(_selectedPortfolio);

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
                MessageBox.Show("Wszystkie portfele zostały zapisane", "Zapis zakończony");
            });
        }
    }
}