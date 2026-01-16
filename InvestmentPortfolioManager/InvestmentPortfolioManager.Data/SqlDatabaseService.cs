using InvestmentPortfolioManager.Core;
using InvestmentPortfolioManager.Core.Enums;
using InvestmentPortfolioManager.Core.Interfaces;
using InvestmentPortfolioManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace InvestmentPortfolioManager.Data
{
    public class SqlDatabaseService : IDataService
    {
        public List<Asset> GetFilteredAssets(InvestmentPortfolio portfolio, double? minPrice, double? maxPrice, RiskEnum? riskLevel, string? nameFragment)
        {
            if (portfolio == null)
                throw new ArgumentNullException(nameof(portfolio));

            using (var db = new InvestmentPortfolioDbContext())
            {
                // aktywa z bazy danych bez poziomu ryzyka (to co da się przefiltrować w SQL)
                var query = db.Portfolios
                    .Where(p => p.InvestmentPortfolioId == portfolio.InvestmentPortfolioId)
                    .SelectMany(p => p.Assets);

                if (minPrice.HasValue)
                    query = query.Where(a => a.CurrentPrice >= minPrice.Value);

                if (maxPrice.HasValue)
                    query = query.Where(a => a.CurrentPrice <= maxPrice.Value);

                if (!string.IsNullOrWhiteSpace(nameFragment))
                    query = query.Where(a =>
                        a.AssetName.Contains(nameFragment) ||
                        a.AssetSymbol.Contains(nameFragment));

                // przejscie do pamięci, bo ryzyko trzeba filtrować tam - osobna metoda GetRiskAssessment()
                var assets = query.ToList();

                if (riskLevel.HasValue)
                    assets = assets
                        .Where(a => a.GetRiskAssessment() == riskLevel.Value)
                        .ToList();

                return assets;
            }
        }

        public List<InvestmentPortfolio> LoadAllPortfolios()
        {
            using (var db = new InvestmentPortfolioDbContext())
            {
                Console.WriteLine("Ładowanie portfeli z bazy danych...");
                var portfolios = db.Portfolios
                    .Include("Assets")
                    .ToList();

                Console.WriteLine($"Załadowano {portfolios.Count} portfeli z bazy danych.");
                return portfolios;
            }
        }

        public void SavePortfolios(List<InvestmentPortfolio> portfolios)
        {
            Console.WriteLine("Zapisywanie portfeli do bazy danych...");
            using (var db = new InvestmentPortfolioDbContext())
            {
                foreach (var portfolio in portfolios)
                {
                    db.Portfolios.AddOrUpdate(portfolio);
                }

                db.SaveChanges();
            }

            Console.WriteLine("Portfele zostały zapisane do bazy danych.");
        }
    }
}
