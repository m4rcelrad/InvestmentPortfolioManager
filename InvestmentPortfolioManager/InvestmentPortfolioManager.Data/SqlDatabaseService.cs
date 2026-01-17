using InvestmentPortfolioManager.Core;
using InvestmentPortfolioManager.Core.Enums;
using InvestmentPortfolioManager.Core.Interfaces;
using InvestmentPortfolioManager.Core.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace InvestmentPortfolioManager.Data
{
    public class SqlDatabaseService : IDataService
    {
        public void SavePortfolios(List<InvestmentPortfolio> portfolios)
        {
            using var db = new InvestmentPortfolioDbContext();

            foreach (var portfolio in portfolios)
            {
                db.Portfolios.Update(portfolio);
            }

            db.SaveChanges();
        }

        public List<InvestmentPortfolio> LoadAllPortfolios()
        {
            using var db = new InvestmentPortfolioDbContext();

            return db.Portfolios
                .Include(p => p.Assets)
                .ToList();
        }

        public List<Asset> GetFilteredAssets(
            InvestmentPortfolio portfolio,
            double? minPrice,
            double? maxPrice,
            RiskEnum? riskLevel,
            string? nameFragment)
        {
            using var db = new InvestmentPortfolioDbContext();

            var query = db.Assets
                .Where(a => a.InvestmentPortfolioId == portfolio.InvestmentPortfolioId)
                .AsQueryable();

            if (minPrice.HasValue)
                query = query.Where(a => a.CurrentPrice >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(a => a.CurrentPrice <= maxPrice.Value);

            if (!string.IsNullOrWhiteSpace(nameFragment))
                query = query.Where(a =>
                    a.AssetName.Contains(nameFragment) ||
                    a.AssetSymbol.Contains(nameFragment));

            var assets = query.ToList();

            if (riskLevel.HasValue)
                assets = assets
                    .Where(a => a.GetRiskAssessment() == riskLevel.Value)
                    .ToList();

            return assets;
        }
    }
}

