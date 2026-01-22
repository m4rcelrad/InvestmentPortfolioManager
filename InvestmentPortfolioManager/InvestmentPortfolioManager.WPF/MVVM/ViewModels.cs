using InvestmentPortfolioManager.Core.Enums;
using InvestmentPortfolioManager.Core.Exceptions;
using InvestmentPortfolioManager.Core.Models;
using InvestmentPortfolioManager.Data;
using InvestmentPortfolioManager.WPF.Views;
using System;
using InvestmentPortfolioManager.Core.Comparers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace InvestmentPortfolioManager.WPF.MVVM
{
    /// <summary>
    /// Model widoku dla panelu kontrolnego (Dashboard).
    /// Agreguje dane z portfela w celu wyświetlenia podsumowań finansowych i statystyk aktywów.
    /// </summary>
    public class DashboardViewModel : ViewModelBase
    {
        private InvestmentPortfolio? _portfolio;

        /// <summary>Pobiera całkowitą wartość rynkową aktualnego portfela.</summary>   
        public double TotalValue => _portfolio?.CalculateSum() ?? 0;

        /// <summary>Pobiera całkowity zysk/stratę wygenerowaną przez portfel.</summary>
        public double TotalProfit => _portfolio?.CalculateTotalProfit() ?? 0;

        /// <summary>Zwraca informację, czy portfel jest obecnie na plusie.</summary>
        public bool IsProfitPositive => TotalProfit >= 0;

        /// <summary>Kolekcja aktywów o największych zmianach wartości (Top Movers).</summary>
        public ObservableCollection<Asset> TopMovers { get; set; } = [];

        /// <summary>Słownik reprezentujący alokację kapitału (Typ aktywa -> Wartość).</summary>
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

        /// <summary>
        /// Odświeża wszystkie statystyki obliczeniowe i powiadamia widok o zmianach.
        /// Wywoływane po każdej aktualizacji cen w symulacji.
        /// </summary>
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

    /// <summary>
    /// Model widoku odpowiedzialny za zarządzanie widokiem portfela inwestycyjnego.
    /// Obsługuje wyświetlanie listy aktywów, zaawansowane filtrowanie, wyszukiwanie 
    /// oraz interakcję z użytkownikiem w zakresie dodawania i usuwania składników portfela.
    /// </summary>
    public class PortfolioViewModel : ViewModelBase
    {
        /// <summary>Serwis odpowiedzialny za komunikację z bazą danych SQL.</summary>
        private readonly SqlDatabaseService _dataService;

        /// <summary>Referencja do obecnie obsługiwanego modelu portfela.</summary>
        private InvestmentPortfolio? _portfolio;

        private ObservableCollection<Asset> _assets = [];

        /// <summary>
        /// Kolekcja aktywów wyświetlana w interfejsie użytkownika.
        /// Automatycznie powiadamia widok o zmianie całej kolekcji.
        /// </summary>
        public ObservableCollection<Asset> Assets
        {
            get => _assets;
            set { _assets = value; OnPropertyChanged(); }
        }

        /// <summary>Tekst wprowadzony przez użytkownika w polu wyszukiwania (szuka po nazwie lub symbolu).</summary>
        public string SearchText { get; set; } = string.Empty;

        /// <summary>Filtr minimalnej ceny aktywa. Jeśli null, filtr jest nieaktywny.</summary>
        public double? MinPriceFilter { get; set; }

        /// <summary>Filtr maksymalnej ceny aktywa. Jeśli null, filtr jest nieaktywny.</summary>
        public double? MaxPriceFilter { get; set; }

        /// <summary>Wybrany z listy rozwijanej poziom ryzyka jako kryterium filtrowania.</summary>
        public RiskEnum? SelectedRiskFilter { get; set; }

        /// <summary>Pobiera listę wszystkich dostępnych poziomów ryzyka zdefiniowanych w <see cref="RiskEnum"/>.</summary>
        public IEnumerable<RiskEnum> RiskLevels => Enum.GetValues(typeof(RiskEnum)).Cast<RiskEnum>();

        /// <summary>Polecenie wyzwalające proces filtrowania kolekcji aktywów.</summary>   
        public ICommand FilterCommand { get; }

        public ICommand SortByRiskCommand { get; }
        /// <summary>Polecenie resetujące wszystkie filtry do wartości domyślnych.</summary>
        public ICommand ClearFiltersCommand { get; }

        /// <summary>
        /// Inicjalizuje nową instancję klasy <see cref="PortfolioViewModel"/>.
        /// Konfiguruje serwisy oraz przypisuje akcje do odpowiednich komend.
        /// </summary>
        /// <param name="portfolio">Instancja portfela, która ma być zarządzana przez ten ViewModel.</param>
        public PortfolioViewModel(InvestmentPortfolio portfolio)
        {
            _dataService = new SqlDatabaseService();
            _portfolio = portfolio;

            UpdatePortfolio(portfolio);

            FilterCommand = new RelayCommand(o => ApplyFilters());
            SortByRiskCommand = new RelayCommand(o => SortByRisk());
            ClearFiltersCommand = new RelayCommand(o => ClearFilters());

            AddAssetCommand = new RelayCommand(o => AddAsset());
            RemoveAssetCommand = new RelayCommand(o => RemoveAsset());
        }

        /// <summary>
        /// Aktualizuje powiązany portfel i resetuje widok filtrów.
        /// </summary>
        /// <param name="newPortfolio">Nowa instancja portfela do załadowania.</param>
        public void UpdatePortfolio(InvestmentPortfolio? newPortfolio)
        {
            _portfolio = newPortfolio;
            ClearFilters();
        }

        /// <summary>
        /// Przeprowadza filtrowanie kolekcji aktywów na podstawie wprowadzonych kryteriów.
        /// Wykorzystuje wyrażenie lambda przekazywane do metody <see cref="InvestmentPortfolio.FindAssets"/>.
        /// </summary>
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

        /// <summary>
        /// Czyści wszystkie parametry filtrowania i przywraca pełną listę aktywów z portfela.
        /// </summary>
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

        /// <summary>
        /// Wymusza odświeżenie widoku kolekcji. Przydatne przy aktualizacji cen przez symulację.
        /// </summary>
        public void RefreshView()
        {
            if (Assets != null)
            {
                CollectionViewSource.GetDefaultView(Assets).Refresh();
            }
        }

        private Asset? _selectedAsset;

        /// <summary>Obecnie zaznaczone aktywo na liście w interfejsie użytkownika.</summary>   
        public Asset? SelectedAsset
        {
            get => _selectedAsset;
            set { _selectedAsset = value; OnPropertyChanged(); }
        }

        /// <summary>Polecenie otwierające okno dodawania nowego aktywa.</summary>
        public ICommand AddAssetCommand { get; }

        /// <summary>Polecenie usuwające zaznaczone aktywo z portfela.</summary>
        public ICommand RemoveAssetCommand { get; }

        /// <summary>
        /// Obsługuje proces dodawania nowego aktywa poprzez otwarcie okna <see cref="AddAssetWindow"/>.
        /// Jeśli użytkownik potwierdzi wybór, aktywo jest dodawane do modelu portfela.
        /// </summary>
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

        /// <summary>
        /// Usuwa zaznaczone aktywo z portfela po uprzednim potwierdzeniu operacji przez użytkownika.
        /// </summary>
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
        private void SortByRisk()
        {
            if (Assets == null || !Assets.Any()) return;

            // Pobieramy obecne aktywa do listy
            var assetList = Assets.ToList();

            // UŻYCIE KOMPARATORA: To jest kluczowy moment dla Twojego sprawozdania
            assetList.Sort(new AssetRiskComparer());

            // Odświeżamy listę w GUI
            Assets = new ObservableCollection<Asset>(assetList);
        }
    }

    /// <summary>
    /// Główny model widoku aplikacji (Shell ViewModel).
    /// Zarządza nawigacją między widokami, koordynuje pracę podrzędnych modeli widoku (Dashboard i Portfolio)
    /// oraz kontroluje silnik symulacji zmian cen rynkowych.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>Serwis dostępu do danych SQL.</summary>
        private readonly SqlDatabaseService _dataService;
        /// <summary>Przechowuje aktualnie wyświetlany widok (ViewModel).</summary>
        private object _currentView;

        /// <summary>Timer sterujący cykliczną aktualizacją cen w symulacji.</summary>
        private DispatcherTimer _simulationTimer;
        /// <summary>Określa, czy symulacja rynkowa jest obecnie uruchomiona.</summary>
        private bool _isSimulationRunning;
        /// <summary>Aktualna data wewnątrz symulacji.</summary>
        private DateTime _simulationDate = DateTime.Now;

        /// <summary>Kolekcja wszystkich portfeli dostępnych w systemie.</summary>
        public ObservableCollection<InvestmentPortfolio> AllPortfolios { get; set; } = [];

        private InvestmentPortfolio _selectedPortfolio = null!;

        /// <summary>
        /// Pobiera lub ustawia imię i nazwisko właściciela wybranego portfela.
        /// Zawiera logikę walidacji i obsługę wyjątku <see cref="InvalidOwnerException"/>.
        /// </summary>
        public string PortfolioOwner
        {
            get => SelectedPortfolio?.Owner ?? string.Empty;
            set
            {
                if (SelectedPortfolio == null) return;

                try
                {
                    SelectedPortfolio.Owner = value;
                }
                catch (InvalidOwnerException ex)
                {
                    MessageBox.Show(ex.Message, "Błąd formatu", MessageBoxButton.OK, MessageBoxImage.Warning);

                    OnPropertyChanged();
                    return;
                }
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Obecnie wybrany i aktywny portfel inwestycyjny.
        /// Setter odpowiada za bezpieczne zarządzanie subskrypcjami zdarzeń (Events), 
        /// co zapobiega wyciekom pamięci oraz zapewnia aktualizację podrzędnych widoków.
        /// </summary>
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
                    OnPropertyChanged(nameof(PortfolioOwner));

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

        /// <summary>
        /// Reaguje na zmiany w kolekcji aktywów (dodanie/usunięcie), 
        /// odpowiednio zarządzając subskrypcjami zdarzeń cenowych.
        /// </summary>
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

        /// <summary>
        /// Obsługuje zdarzenie krytycznego spadku ceny aktywa.
        /// Wyświetla ostrzeżenie i wstrzymuje symulację w celach bezpieczeństwa.
        /// </summary>
        private void HandleCriticalDrop(string symbol, double price, string message)
        {
            if (_isSimulationRunning)
            {
                ToggleSimulation();
            }

            MessageBox.Show(message, $"ALERT: {symbol}", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>Model widoku dla panelu statystyk.</summary>
        public DashboardViewModel DashboardVM { get; set; } = null!;
        /// <summary>Model widoku dla zarządzania listą aktywów.</summary>
        public PortfolioViewModel PortfolioVM { get; set; } = null!;
        /// <summary>Aktualnie wybrany widok bindowany do ContentControl w MainWindow.</summary>
        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        /// <summary>Tekst wyświetlany na przycisku symulacji, zależny od jej stanu.</summary>
        public string SimulationButtonText => _isSimulationRunning ? "Stop Simulation" : "Start Simulation";
        /// <summary>Kolor przycisku symulacji (HEX), zmieniający się w zależności od aktywności.</summary>
        public string SimulationButtonColor => _isSimulationRunning ? "#E74C3C" : "#3498DB";
        /// <summary>Polecenie zmiany widoku głównego.</summary>
        public ICommand SwitchViewCommand { get; }
        /// <summary>Polecenie zapisu wszystkich portfeli do bazy danych.</summary>
        public ICommand SaveCommand { get; }
        /// <summary>Polecenie przełączające stan symulacji (Start/Stop).</summary>
        public ICommand ToggleSimulationCommand { get; }
        /// <summary>Polecenie tworzenia nowego, pustego portfela.</summary>
        public ICommand CreatePortfolioCommand { get; }
        /// <summary>Polecenie klonowania obecnego portfela przy użyciu wzorca Prototype.</summary>
        public ICommand ClonePortfolioCommand { get; }
        /// <summary>Polecenie usuwania wybranego portfela.</summary>
        public ICommand DeletePortfolioCommand { get; }

        /// <summary>
        /// Inicjalizuje nową instancję <see cref="MainViewModel"/>.
        /// Ładuje dane z bazy, konfiguruje startowy portfel oraz inicjalizuje komendy UI.
        /// </summary>
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
                if (_currentEvent != null)
                {
                    EndCurrentEvent();
                }
                _dataService.SavePortfolios(new List<InvestmentPortfolio>(AllPortfolios));
                MessageBox.Show("Saved Successfully", "Success");
            });

            CreatePortfolioCommand = new RelayCommand(o => CreatePortfolio());
            ClonePortfolioCommand = new RelayCommand(o => ClonePortfolio());
            DeletePortfolioCommand = new RelayCommand(o => DeletePortfolio());

            ToggleSimulationCommand = new RelayCommand(o => ToggleSimulation());
        }

        /// <summary>
        /// Konfiguruje parametry timera symulacji.
        /// </summary>
        private void InitializeSimulation()
        {
            _simulationTimer.Interval = TimeSpan.FromSeconds(5);
            _simulationTimer.Tick += SimulationTimer_Tick;
        }

        /// <summary>
        /// Uruchamia lub zatrzymuje proces symulacji rynkowej.
        /// </summary>
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

        /// <summary>
        /// Metoda wywoływana przy każdym tick'u zegara symulacji.
        /// Aktualizuje ceny rynkowe i wymusza odświeżenie statystyk w UI.
        /// </summary>
        private void SimulationTimer_Tick(object? sender, EventArgs? e)
        {
            if (SelectedPortfolio == null) return;

            _simulationDate = _simulationDate.AddDays(1);

            ProcessMarketEvents();

            SelectedPortfolio.UpdateMarketPrices(_simulationDate);

            DashboardVM.RecalculateStats();
            PortfolioVM.RefreshView();
        }

        /// <summary>
        /// Tworzy nowy, pusty portfel inwestycyjny z domyślnymi parametrami.
        /// Nowy portfel otrzymuje unikalną nazwę opartą na aktualnej liczbie portfeli
        /// i zostaje automatycznie ustawiony jako wybrany portfel w aplikacji.
        /// </summary>
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

        /// <summary>
        /// Tworzy głęboką kopię aktualnie wybranego portfela.
        /// Wykorzystuje mechanizm klonowania zdefiniowany w klasie <see cref="InvestmentPortfolio"/>.
        /// Pozwala użytkownikowi na szybkie tworzenie wariantów tego samego portfela 
        /// do celów porównawczych lub testowych w symulacji.
        /// </summary>
        private void ClonePortfolio()
        {
            if (SelectedPortfolio == null) return;

            var clone = (InvestmentPortfolio)SelectedPortfolio.Clone();

            AllPortfolios.Add(clone);
            SelectedPortfolio = clone;
        }

        /// <summary>Usuwa aktywny portfel i przełącza widok na inny dostępny element.</summary>
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

        /// <summary>
        /// Przechowuje aktualnie trwające zdarzenie rynkowe. Jeśli null, rynek jest w stanie stabilnym.
        /// </summary>
        private MarketEvent? _currentEvent;

        /// <summary>
        /// Liczba cykli symulacji (tików) pozostała do zakończenia obecnego zdarzenia.
        /// </summary>
        private int _eventTimeRemaining;

        /// <summary>
        /// Słownik służący do zapisu oryginalnego stanu aktywów (zmienność i średni zwrot) przed wystąpieniem zdarzenia.
        /// Kluczem jest identyfikator aktywa (Guid). Pozwala to na przywrócenie parametrów po zakończeniu zdarzenia.
        /// </summary>
        private Dictionary<Guid, (double Volatility, double MeanReturn)> _backupState = new();

        /// <summary>
        /// Prywatne pole przechowujące treść aktualnej wiadomości rynkowej.
        /// </summary>
        private string _newsMessage = "Market is stable.";

        /// <summary>
        /// Publiczna właściwość udostępniająca komunikat o stanie rynku dla widoku (WPF Binding).
        /// </summary>
        public string NewsMessage
        {
            get => _newsMessage;
            set { _newsMessage = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Lista predefiniowanych scenariuszy rynkowych, które mogą zostać wylosowane podczas symulacji.
        /// </summary>
        private readonly List<MarketEvent> _possibleEvents =
        [
            new MarketEvent
        {
            Title = "CRYPTO CRASH",
            Description = "Bitcoin and Altcoins are plunging! Panic in the market.",
            DurationTicks = 5,
            TargetPredicate = a => a is Cryptocurrency,
            VolatilityMultiplier = 3.0,
            MeanReturnModifier = -0.05
        },
        new MarketEvent
        {
            Title = "REAL ESTATE BOOM",
            Description = "Housing prices are rising due to low interest rates.",
            DurationTicks = 8,
            TargetPredicate = a => a is RealEstate,
            VolatilityMultiplier = 1.0,
            MeanReturnModifier = 0.02
        },
        new MarketEvent
        {
            Title = "GEOPOLITICAL UNCERTAINTY",
            Description = "Investors are fleeing to gold. Stocks are unstable.",
            DurationTicks = 6,
            TargetPredicate = a => a is Stock || a is Commodity,
            VolatilityMultiplier = 2.5,
            MeanReturnModifier = -0.005
        },
        new MarketEvent
        {
            Title = "MARKET STABILIZATION",
            Description = "The market is calming down after recent events.",
            DurationTicks = 3,
            TargetPredicate = a => true,
            VolatilityMultiplier = 0.5,
            MeanReturnModifier = 0.0
        }
        ];

        /// <summary>
        /// Główna metoda zarządzająca cyklem życia zdarzeń rynkowych.
        /// Wywoływana w każdej turze symulacji. Odpowiada za odliczanie czasu trwania zdarzenia
        /// lub losowanie nowego zdarzenia, jeśli żadne aktualnie nie występuje.
        /// </summary>
        private void ProcessMarketEvents()
        {
            if (_currentEvent != null)
            {
                _eventTimeRemaining--;

                if (_eventTimeRemaining <= 0)
                {
                    EndCurrentEvent();
                }
                return;
            }

            // 10% szansy na wystąpienie nowego zdarzenia w każdej turze
            if (Random.Shared.NextDouble() < 0.10)
            {
                StartRandomEvent();
            }
            else
            {
                NewsMessage = "Market is stable. No new reports.";
            }
        }

        /// <summary>
        /// Rozpoczyna losowe zdarzenie z listy dostępnych scenariuszy.
        /// Zapisuje obecny stan aktywów, aplikuje modyfikatory (zmiana zmienności i zwrotu)
        /// oraz aktualizuje komunikat dla użytkownika.
        /// </summary>
        private void StartRandomEvent()
        {
            if (SelectedPortfolio == null) return;

            var randomEvent = _possibleEvents[Random.Shared.Next(_possibleEvents.Count)];
            _currentEvent = randomEvent;
            _eventTimeRemaining = randomEvent.DurationTicks;

            NewsMessage = $"{randomEvent.Title}: {randomEvent.Description}";

            _backupState.Clear();

            foreach (var asset in SelectedPortfolio.Assets)
            {
                // Sprawdzenie, czy zdarzenie dotyczy danego typu aktywa
                if (randomEvent.TargetPredicate(asset))
                {
                    _backupState[asset.Asset_id] = (asset.Volatility, asset.MeanReturn);

                    asset.Volatility *= randomEvent.VolatilityMultiplier;
                    asset.MeanReturn += randomEvent.MeanReturnModifier;
                }
            }
        }

        /// <summary>
        /// Kończy aktualnie trwające zdarzenie rynkowe.
        /// Przywraca oryginalne wartości parametrów (zmienność, średni zwrot) dla wszystkich zmodyfikowanych aktywów
        /// i czyści stan tymczasowy.
        /// </summary>
        private void EndCurrentEvent()
        {
            if (SelectedPortfolio == null || _currentEvent == null) return;

            foreach (var asset in SelectedPortfolio.Assets)
            {
                if (_backupState.ContainsKey(asset.Asset_id))
                {
                    var original = _backupState[asset.Asset_id];
                    asset.Volatility = original.Volatility;
                    asset.MeanReturn = original.MeanReturn;
                }
            }

            _currentEvent = null;
            _backupState.Clear();
            NewsMessage = "Market event ended. Returning to normal.";
        }
    }
}