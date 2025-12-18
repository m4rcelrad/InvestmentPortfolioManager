using InvestmentPortfolioManager.Core;
using InvestmentPortfolioManager.Core.Exceptions;
using InvestmentPortfolioManager.Core.Models;
using InvestmentPortfolioManager.Core.Services;
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Asset addition failed: {ex.Message}");
            }

            Console.WriteLine($"\n[INITIAL STATE] Total Value: {portfolio.CalculateSum():c}");

            uint days = 10;
            DateTime simulationStart = DateTime.Today;

            Console.WriteLine($"\n[INFO] Starting {days}-day market simulation starting form {simulationStart:d}");
            SimulationStart(portfolio, simulationStart, days);
        }

        static void SimulationStart(InvestmentPortfolio portfolio, DateTime simulationStart, uint days)
        {
            DateTime simulationDate = simulationStart;

            for (int day = 1; day <= days; day++)
            {
                simulationDate = simulationDate.AddDays(1);

                portfolio.UpdateMarketPrices(simulationDate);

                PrintDailyReport(portfolio, simulationDate, day);

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
    }
}
