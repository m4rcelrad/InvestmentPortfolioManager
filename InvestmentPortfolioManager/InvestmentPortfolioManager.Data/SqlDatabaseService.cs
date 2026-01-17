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
                foreach (var asset in portfolio.Assets)
                {
                    asset.InvestmentPortfolioId = portfolio.InvestmentPortfolioId;
                }

                var exists = db.Portfolios
                    .AsNoTracking()
                    .Any(p => p.InvestmentPortfolioId == portfolio.InvestmentPortfolioId);

                if (!exists)
                {
                    // INSERT
                    db.Portfolios.Add(portfolio);
                }
                else
                {
                    // UPDATE
                    db.Portfolios.Update(portfolio);
                }
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

    }
}

