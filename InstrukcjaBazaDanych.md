Zrobiłem tak, żeby program nie wiedział, w jaki sposób dane są zapisywane. Możesz dowolnie wymieniać źródła danych (XML, SQL) bez psucia aplikacji.

**Uwaga: nie dodałem chyba zależności ConsoleApp - Data, więc musisz to zrobić jak będziesz chciał coś testować w aplikacji konsolowej**


Poniżej instrukcja, jak wpiąć swoją implementację bazy danych.


---

1. INTERFEJS IDataService

W projekcie `Core` znajduje się interfejs `IDataService`. 

Wymagane metody do zaimplementowania:
- void SavePortfolio(List<Asset> assets);
- List<Asset> LoadPortfolio();
- List<Asset> GetFilteredAssets(...);

---

2. GDZIE PISAĆ KOD? (PROJEKT DATA)

Stworzyłem osobny projekt: `InvestmentPortfolioManager.Data`.
To tutaj powinieneś zainstalować biblioteki do bazy danych.

Twoim zadaniem jest stworzenie klasy w tym projekcie, która implementuje `IDataService`.

Przykład struktury klasy:
```
namespace InvestmentPortfolioManager.Data
{
    public class SqlDatabaseService : IDataService
    {
        private string _connectionString = "...";

        public void SavePortfolio(List<Asset> assets)
        {
            // Tu wpisz kod zapisujący listę do tabeli
        }

        public List<Asset> LoadPortfolio()
        {
            // Tu wpisz kod pobierający dane (SELECT *)
        }
        
        // ... reszta metod
    }
}
```

---

3. JAK "WŁĄCZYĆ" SWOJĄ BAZĘ DANYCH? (DEPENDENCY INJECTION)

W tej chwili aplikacja używa danych testowych albo pustej implementacji. Aby aplikacja zaczęła korzystać z klasy `SqlDatabaseService`, musisz ją dodać przy starcie aplikacji.
---

4. WAŻNE UWAGI

-> Mapowanie typów:
  Przy odczycie z bazy danych (`LoadPortfolio`) musisz rozpoznać typ aktywa (np. kolumna 'Type': 'Stock') i utworzyć odpowiednią klasę z Core (`new Stock(...)`), a nie generyczny Asset.

-> Filtrowanie (`GetFilteredAssets`):
   Metoda przyjmuje parametry opcjonalne (nullable `double?`). Jeśli parametr jest nullem, ignoruj go w zapytaniu WHERE.
   Przykład logiczny: `SELECT * FROM Assets WHERE (@minPrice IS NULL OR Price >= @minPrice)`

"""
