

Projekt testowy MSTest jest już podpięty do rozwiązania i ma referencję do `Core`.

1. CO TRZEBA PRZETESTOWAĆ?

A. Modele (Asset.cs i dziedziczące):
   - Czy właściwość `Value` poprawnie liczy iloczyn `Quantity * Price`?
   - Czy walidacja w setterach działa (np. ujemna ilość, pusta nazwa, itd)?

B. Portfel (InvestmentPortfolio.cs):
   - Dodawanie i usuwanie aktywów.
   - Sumowanie wartości całego portfela (`CalculateSum`).
   - Walidacja właściciela (`Owner`).

C. Symulacja (Szczególne przypadki):
   - `Bond` (Obligacja) - tu symulacja jest nielosowa (wzrost o stały %), więc łatwo to przetestować.
   - Dla akcji (`Stock`) i krypto wynik jest losowy, więc testujemy tylko czy cena się *zmieniła* lub czy nie jest ujemna.

- Nie testuj metody `MarketSimulator.GenerateNewPrice` na konkretne wartości, bo jest losowa. Testuj ją tylko pod kątem tego, czy nie zwraca NaN lub nieskończoności.
- Pamiętaj o sprawdzeniu walidacji Regex w `InvestmentPortfolio.Owner` (np. czy odrzuca "jan kowalski" małymi literami).

D. Rzeczy zrobione przez Emmanuela i Pawła ( częściowo kodem, częściowo manualnie)
