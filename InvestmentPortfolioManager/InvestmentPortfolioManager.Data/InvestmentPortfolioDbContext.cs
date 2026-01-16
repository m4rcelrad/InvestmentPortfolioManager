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
        public InvestmentPortfolioDbContext()
            : base("name=InvestmentPortfolioDb")
        {
        }

        public DbSet<InvestmentPortfolio> Portfolios { get; set; }
        public DbSet<Asset> Assets { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Asset>()
                .Map<Stock>(m => m.Requires("AssetType").HasValue("Stock"))
                .Map<Bond>(m => m.Requires("AssetType").HasValue("Bond"))
                .Map<Cryptocurrency>(m => m.Requires("AssetType").HasValue("Cryptocurrency"))
                .Map<RealEstate>(m => m.Requires("AssetType").HasValue("RealEstate"))
                .Map<Commodity>(m => m.Requires("AssetType").HasValue("Commodity"));

            base.OnModelCreating(modelBuilder);
        }
    }
}
