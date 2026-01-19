using System;
using Microsoft.EntityFrameworkCore;
using InvestmentPortfolioManager.Core.Models;

namespace InvestmentPortfolioManager.Data
{
    /// <summary>
    /// Kontekst bazy danych Entity Framework Core dla aplikacji menedżera portfela.
    /// Definiuje tabele oraz reguły mapowania obiektowo-relacyjnego (ORM).
    /// </summary>
    public class InvestmentPortfolioDbContext : DbContext
    {
        /// <summary>Tabela przechowująca nagłówki portfeli.</summary>
        public DbSet<InvestmentPortfolio> Portfolios { get; set; }

        /// <summary>Tabela przechowująca wszystkie aktywa (Stocks, Crypto, etc.).</summary>
        public DbSet<Asset> Assets { get; set; }

        /// <summary>Tabela przechowująca historię zmian cen dla wszystkich aktywów.</summary>
        public DbSet<PricePoint> PricePoints { get; set; }

        /// <summary>
        /// Konfiguruje połączenie z bazą danych. 
        /// Domyślnie używa lokalnej instancji SQL Server (LocalDB).
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(
                @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=InvestmentPortfolioDb;Integrated Security=True");
        }

        /// <summary>
        /// Definiuje zaawansowane mapowania modelu, w tym strategię dziedziczenia.
        /// </summary>
        /// <remarks>
        /// Zastosowano strategię TPH (Table-Per-Hierarchy), gdzie wszystkie typy aktywów 
        /// znajdują się w jednej tabeli "Assets", a rozróżnia je kolumna "AssetType" (Discriminator).
        /// </remarks>
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
