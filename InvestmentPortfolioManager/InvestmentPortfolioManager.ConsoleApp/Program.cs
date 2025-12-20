using InvestmentPortfolioManager.Core;
using InvestmentPortfolioManager.Core.Enums;
using InvestmentPortfolioManager.Core.Exceptions;
using InvestmentPortfolioManager.Core.Models;
using System.Globalization;

namespace InvestmentPortfolioManager.ConsoleApp
{
    internal class Program
    {
        static void Main()
        {
            var culture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            Console.WriteLine("==================================================");
            Console.WriteLine("        INVESTMENT PORTFOLIO MANAGER");
            Console.WriteLine("==================================================\n");

            InvestmentPortfolio portfolio = new();

            try
            {
                portfolio.Owner = "Warren Buffet";
                Console.WriteLine($"[INFO] Portfolio owner set to: {portfolio.Owner}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to set owner: {ex.Message}");
                return;
            }

            try
            {
                Console.WriteLine("[INFO] Adding initial assets...");

                var stock = new Stock("Apple Inc.", "AAPL", 10, 150.0);
                portfolio.AddNewAsset(stock);
                Console.WriteLine($"   + Added Stock: {stock.AssetSymbol} ({stock.Quantity} units @ {stock.PurchasePrice:c})");

                var bond = new Bond("US Treasury Bond", "US-GOV", 50, 100.0, 0.05);
                portfolio.AddNewAsset(bond);
                Console.WriteLine($"   + Added Bond:  {bond.AssetSymbol} ({bond.Quantity} units, Rate: {bond.Rate:p2})");

                var crypto = new Cryptocurrency("Bitcoin", "BTC", 0.5, 45000.0);
                portfolio.AddNewAsset(crypto);
                Console.WriteLine($"   + Added Crypto:{crypto.AssetSymbol} ({crypto.Quantity} units @ {crypto.PurchasePrice:c})");

                var apartment = new RealEstate("Krakow Old Town", 1250000.0, "Szewska", "67", "Krakow", "31-009", "Poland", "67");
                portfolio.AddNewAsset(apartment);
                Console.WriteLine($"   + Added RealEstate: {apartment.AssetName} ({apartment.City}, {apartment.Street})");

                var apartment2 = new RealEstate("Krakow Apartment", 800000.0, "Florianska", "67", "Krakow", "31-009", "Poland", "67");
                portfolio.AddNewAsset(apartment2);
                Console.WriteLine($"   + Added 2nd RealEstate: {apartment.AssetName} ({apartment.City}, {apartment.Street})");


                var gold = new Commodity("Gold", "XAU", 10.0, 1950.0, UnitEnum.Ounce);
                portfolio.AddNewAsset(gold);
                Console.WriteLine($"   + Added Commodity: {gold.AssetName} ({gold.Quantity} {gold.Unit})");

                Console.WriteLine("\n[TEST] Adding second Bitcoin tranche to test grouping logic...");
                var crypto2 = new Cryptocurrency("Bitcoin", "BTC", 0.5, 30000.0);
                portfolio.AddNewAsset(crypto2);
                Console.WriteLine($"   + Added 2nd BTC: {crypto2.AssetSymbol} ({crypto2.Quantity} units @ {crypto2.PurchasePrice:c}) [CHEAPER]");

            }
            catch (InvalidAddressException ex)
            {
                Console.WriteLine($"[ERROR] Invalid address data: {ex.Message}");
            }
            catch (InvalidZipCodeException ex)
            {
                Console.WriteLine($"[ERROR] ZipCode validation failed: {ex.Message}");
            }
            catch (InvalidUnitException ex)
            {
                Console.WriteLine($"[ERROR] Commodity unit error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Asset addition failed: {ex.Message}");
            }

            Console.WriteLine($"\n[INITIAL STATE] Total Value: {portfolio.CalculateSum():c}");

            Console.WriteLine("\n[DELEGATES] Attaching price listeners to Bitcoin (Multicast)...");

            var btcAsset = portfolio.Assets.FirstOrDefault(a => a.AssetSymbol == "BTC");

            if (btcAsset != null)
            {
                btcAsset.OnPriceUpdate += DisplayAlert;
                btcAsset.OnPriceUpdate += LogSimulation;

                btcAsset.LowPriceThreshold = 42000.0;
                btcAsset.OnCriticalDrop += DisplayCriticalAlert;
                Console.WriteLine($"   [CONFIG] Alert threshold for BTC set to {btcAsset.LowPriceThreshold:C2}");
            }

            uint days = 10;
            DateTime simulationStart = DateTime.Today;

            Console.WriteLine($"\n[INFO] Starting {days}-day market simulation starting from {simulationStart:d}");
            SimulationStart(portfolio, simulationStart, days);

            Console.WriteLine("\n==================================================");
            Console.WriteLine("    GROUPED PORTFOLIO PERFORMANCE");
            Console.WriteLine("==================================================");
            PrintGroupedSummary(portfolio);

            Console.WriteLine("\n[PREDICATES] Searching for high-value assets (> $1000)...");
            Func<Asset, bool> isExpensiveCriteria = asset => asset.CurrentPrice > 1000.0;

            var expensiveAssets = portfolio.FindAssets(isExpensiveCriteria);

            foreach (var asset in expensiveAssets)
            {
                Console.WriteLine($"   -> FOUND: {asset.AssetSymbol} worth {asset.CurrentPrice:C2}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static void SimulationStart(InvestmentPortfolio portfolio, DateTime simulationStart, uint days)
        {
            DateTime simulationDate = simulationStart;

            for (int day = 1; day <= days; day++)
            {
                simulationDate = simulationDate.AddDays(1);

                portfolio.UpdateMarketPrices(simulationDate);

                PrintDailyReport(portfolio, simulationDate, day);

                Thread.Sleep(800);
            }
        }

        static void PrintDailyReport(InvestmentPortfolio portfolio, DateTime date, int dayNumber)
        {
            Console.WriteLine($"\n--- DAY {dayNumber} ({date:yyyy-MM-dd}) ---");

            Console.WriteLine($"{"SYMBOL",-10} | {"TYPE",-15} | {"PRICE ($)",15} | {"CHANGE",10}");
            Console.WriteLine(new string('-', 60));

            foreach (var asset in portfolio.Assets)
            {
                string typeName = asset.GetType().Name;
                string changeIndicator = (asset.CurrentPrice >= asset.PurchasePrice) ? "(+)" : "(-)";
                Console.WriteLine($"{asset.AssetSymbol,-10} | {typeName,-15} | {asset.CurrentPrice,15:F2} | {changeIndicator,10}");
            }

            Console.WriteLine(new string('-', 60));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"TOTAL VALUE: {portfolio.CalculateSum():c}");
            Console.ResetColor();
        }

        static void PrintGroupedSummary(InvestmentPortfolio portfolio)
        {
            Console.WriteLine(new string('-', 115));

            Console.WriteLine($"{"SYMBOL",-8} | {"NAME",-18} | {"QTY",12} | {"AVG BUY",18} | {"CURR VAL",20} | {"PROFIT/LOSS",18}");
            Console.WriteLine(new string('-', 115));

            foreach (var summary in portfolio.PortfolioSummaries.Values)
            {
                ConsoleColor originalColor = Console.ForegroundColor;
                if (summary.TotalProfit > 0) Console.ForegroundColor = ConsoleColor.Green;
                else if (summary.TotalProfit < 0) Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine(
                    $"{summary.AssetSymbol,-8} | " +
                    $"{summary.AssetName,-18} | " +     
                    $"{summary.TotalQuantity,12:F4} | " + 
                    $"{summary.AveragePurchasePrice,18:C2} | " + 
                    $"{summary.TotalValue,20:C2} | " +
                    $"{summary.TotalProfit,18:C2}");

                Console.ForegroundColor = originalColor;
            }
            Console.WriteLine(new string('-', 115));
        }

        static void DisplayAlert(string symbol, double price, string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"   >>> [ALERT] {symbol} price event: {message}");
            Console.ResetColor();
        }

        static void LogSimulation(string symbol, double price, string message)
        {
            Console.WriteLine($"   [LOG] Audit record: {symbol} @ {price:F2}");
        }

        static void DisplayCriticalAlert(string symbol, double price, string message)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"   !!! {message} ({symbol} is now {price:C2}) !!!   ");
            Console.ResetColor();
            Console.WriteLine();
        }
    }
}