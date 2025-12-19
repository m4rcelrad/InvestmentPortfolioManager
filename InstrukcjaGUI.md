1. GŁÓWNE ŹRÓDŁO DANYCH: KLASA InvestmentPortfolio

Twoim głównym modelem w ViewModelu (np. MainViewModel) powinna być klasa `InvestmentPortfolio`.
Nie musisz tworzyć oddzielnych list czy kolekcji "ObservableCollection" w ViewModelu, ponieważ model udostępnia gotową kolekcję.

-> Właściwość do bindowania:
   public ReadOnlyObservableCollection<Asset> Assets { get; }

-> Dlaczego to ważne?
   Typ `ReadOnlyObservableCollection` sprawia, że widok (ListView/DataGrid) AUTOMATYCZNIE dowiaduje się o dodaniu lub usunięciu aktywa z portfela. Nie musisz ręcznie odświeżać listy.

-> Właściwości elementu listy (klasa Asset):
   Każdy obiekt w tej liście ma gotowe właściwości obsługujące odświeżanie (INotifyPropertyChanged):
   - `AssetName` (string)
   - `AssetSymbol` (string)
   - `CurrentPrice` (double) -> Ta właściwość sama odświeża widok, gdy zmienia się cena w symulacji!
   - `Value` (double) -> Wyliczana dynamicznie (Ilość * Cena), też odświeża się sama.
   - `Quantity` (double)

   Przykład XAML:
   ```
   <ListView ItemsSource="{Binding Portfolio.Assets}">
       ...
       <GridViewColumn Header="Cena" DisplayMemberBinding="{Binding CurrentPrice, StringFormat=C2}"/>
       ...
   </ListView>
   ```

---

2. OBSŁUGA RÓŻNYCH TYPÓW AKTYWÓW (DataTemplates)

Lista `Assets` przechowuje ogólne obiekty `Asset`, ale fizycznie są to instancje klas: `Stock`, `Bond`, `RealEstate`, `Cryptocurrency`.
Aby wyświetlić specyficzne dane (np. adres dla nieruchomości, oprocentowanie dla obligacji), użyj DataTemplates w XAML. System sam dobierze wygląd do typu obiektu.

Przykład:
```
<DataTemplate DataType="{x:Type models:RealEstate}">
    <TextBlock Text="{Binding City}" /> </DataTemplate>
```

---

3. SYMULACJA RYNKU

Zaimplementowałem mechanizm symulacji cen. Aby ceny zaczęły się zmieniać, musisz wywołać jedną metodę w modelu.

-> Instrukcja:
   1. W ViewModelu stwórz komendę, np. `NextDayCommand`.
   2. W jej ciele wywołaj: 
      portfolio.UpdateMarketPrices(newDate);

-> Efekt:
   Wszystkie ceny (`CurrentPrice`) i wartości (`Value`) w interfejsie zaktualizują się same. Nie musisz nic przeładowywać.

---

4. NARZĘDZIA MVVM

W folderze `MVVM` masz gotowe klasy pomocnicze:

-> `ViewModelBase`:
   Dziedzicz po niej swoje ViewModele. Używaj metody `OnPropertyChanged(nameof(Pole))`, jeśli dodajesz własne właściwości sterujące widokiem (np. wybrany element listy).

-> `RelayCommand`:
   Używaj do obsługi przycisków.
   Przykład: 
   Public ICommand SimulateCommand { get; }
   SimulateCommand = new RelayCommand(execute => RunSimulation());

---

5. MOCKOWANIE DANYCH (SZYBKI START)

Zanim Paweł zrobi obsługę danych, możesz w konstruktorze `MainViewModel` wrzucić kilka testowych danych, żeby widzieć cokolwiek na ekranie:

```public MainViewModel()
{
    Portfolio = new InvestmentPortfolio();
    
    // DANE TESTOWE
    Portfolio.AddNewAsset(new Stock("Apple", "AAPL", 10, 150.0));
    Portfolio.AddNewAsset(new Bond("Obligacje", "BOND1", 5, 1000.0, 0.05));
    Portfolio.AddNewAsset(new Cryptocurrency("Bitcoin", "BTC", 0.5, 45000.0));
}
```
