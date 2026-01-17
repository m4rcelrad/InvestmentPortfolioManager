using System;
using Microsoft.EntityFrameworkCore;
using InvestmentPortfolioManager.Core.Models;

namespace InvestmentPortfolioManager.Data
{
    public class InvestmentPortfolioDbContext : DbContext
    {
        public DbSet<InvestmentPortfolio> Portfolios { get; set; }
        public DbSet<Asset> Assets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(
                @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=InvestmentPortfolioDb;Integrated Security=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Asset>()
                .HasDiscriminator<string>("AssetType")
                .HasValue<Stock>("Stock")
                .HasValue<Bond>("Bond")
                .HasValue<Cryptocurrency>("Cryptocurrency")
                .HasValue<RealEstate>("RealEstate")
                .HasValue<Commodity>("Commodity");
        }
    }
}
