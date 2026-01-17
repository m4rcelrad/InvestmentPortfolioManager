using InvestmentPortfolioManager.Core.Enums;
using InvestmentPortfolioManager.Core.Interfaces;
using InvestmentPortfolioManager.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvestmentPortfolioManager.Data
{
    public class SqlDatabaseService : IDataService
    {
        public void SavePortfolios(List<InvestmentPortfolio> portfolios)
        {
            using var db = new InvestmentPortfolioDbContext();

            foreach (var portfolio in portfolios)
            {
                var existingPortfolio = db.Portfolios
                    .Include(p => p.Assets)
                    .FirstOrDefault(p => p.InvestmentPortfolioId == portfolio.InvestmentPortfolioId);

                if (existingPortfolio == null)
                {
                    foreach (var asset in portfolio.Assets)
                    {
                        asset.InvestmentPortfolioId = portfolio.InvestmentPortfolioId;
                        asset.InvestmentPortfolio = null;
                    }
                    db.Portfolios.Add(portfolio);
                }
                else
                {
                    db.Entry(existingPortfolio).CurrentValues.SetValues(portfolio);

                    var uiAssetIds = portfolio.Assets.Select(a => a.Asset_id).ToList();

                    var assetsToDelete = existingPortfolio.Assets
                        .Where(a => !uiAssetIds.Contains(a.Asset_id))
                        .ToList();

                    foreach (var assetToDelete in assetsToDelete)
                    {
                        db.Assets.Remove(assetToDelete);
                    }

                    foreach (var uiAsset in portfolio.Assets)
                    {
                        var existingAsset = existingPortfolio.Assets
                            .FirstOrDefault(a => a.Asset_id == uiAsset.Asset_id);

                        if (existingAsset != null)
                        {
                            db.Entry(existingAsset).CurrentValues.SetValues(uiAsset);
                        }
                        else
                        {
                            uiAsset.InvestmentPortfolioId = existingPortfolio.InvestmentPortfolioId;
                            uiAsset.InvestmentPortfolio = null;
                            existingPortfolio.Assets.Add(uiAsset);
                        }
                    }
                }
            }
            db.SaveChanges();
        }

        public List<InvestmentPortfolio> LoadAllPortfolios()
        {
            using var db = new InvestmentPortfolioDbContext();

            var portfolios = db.Portfolios
                .Include(p => p.Assets)
                .ToList();

            if (portfolios.Count == 0)
            {
                portfolios = GenerateMockData();

                SavePortfolios(portfolios);
            }

            return portfolios;
        }

        private List<InvestmentPortfolio> GenerateMockData()
        {
            var portfolios = new List<InvestmentPortfolio>();

            var p1 = new InvestmentPortfolio
            {
                Name = "Main",
                Owner = "Warren Buffet",
                InvestmentPortfolioId = Guid.NewGuid()
            };
            p1.AddNewAsset(new Stock("Apple Inc.", "AAPL", 10, 150.0));
            p1.AddNewAsset(new Cryptocurrency("Bitcoin", "BTC", 0.5, 30000.0));
            portfolios.Add(p1);

            var p2 = new InvestmentPortfolio
            {
                Name = "Retirement",
                Owner = "Bill Gates",
                InvestmentPortfolioId = Guid.NewGuid()
            };
            p2.AddNewAsset(new Commodity("Gold", "GOLD", 10, 2000.0, UnitEnum.Ounce));
            p2.AddNewAsset(new Stock("Apple Inc.", "AAPL", 10, 130.0));
            portfolios.Add(p2);

            return portfolios;
        }
    }
}