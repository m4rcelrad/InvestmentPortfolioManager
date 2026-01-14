using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using InvestmentPortfolioManager.Core.Models;
{
    
}

namespace InvestmentPortfolioManager.Data
{
    public class InvestmentPortfolioDbContext : DbContext
    {
        public DbSet<InvestmentPortfolio> Portfolios { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Bond> Bonds { get; set; }
        public DbSet<Cryptocurrency> Cryptocurrencies { get; set; }
        public DbSet<RealEstate> RealEstates { get; set; }
        public DbSet<Commodity> Commodities { get; set; }

    }
}
