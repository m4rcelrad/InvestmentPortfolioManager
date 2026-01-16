using InvestmentPortfolioManager.Core.Interfaces;
using InvestmentPortfolioManager.Core;
using InvestmentPortfolioManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvestmentPortfolioManager.Core.Enums;



namespace InvestmentPortfolioManager.Data
{
    public class SqlDatabaseService : IDataService
    {
        public List<Asset> GetFilteredAssets(InvestmentPortfolio portfolio, double? minPrice, double? maxPrice, RiskEnum? riskLevel, string? nameFragment)
        {
            throw new NotImplementedException();
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
            using (var db = new InvestmentPortfolioDbContext())
            {
                Console.WriteLine("Przygotowywanie portfeli do zapisania do bazy danych...");
                db.Portfolios.AddRange(portfolios);
                db.SaveChanges();
                Console.WriteLine("Portfele zostały zapisane do bazy danych.");
            }
        }
    }
}
