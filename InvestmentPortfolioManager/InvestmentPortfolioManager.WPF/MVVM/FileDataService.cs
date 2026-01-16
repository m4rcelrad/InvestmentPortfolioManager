using InvestmentPortfolioManager.Core.Enums;
using InvestmentPortfolioManager.Core.Interfaces;
using InvestmentPortfolioManager.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Serialization;

namespace InvestmentPortfolioManager.WPF.MVVM
{
    public class FileDataService : IDataService
    {
        private const string FilePath = "user_portfolios.xml";

        public void SavePortfolios(List<InvestmentPortfolio> portfolios)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<InvestmentPortfolio>));

                using (StreamWriter writer = new StreamWriter(FilePath))
                {
                    serializer.Serialize(writer, portfolios);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"XML save error: {ex.Message}");
            }
        }

        public List<InvestmentPortfolio> LoadAllPortfolios()
        {
            if (File.Exists(FilePath))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<InvestmentPortfolio>));
                    using (StreamReader reader = new StreamReader(FilePath))
                    {
                        var loadedData = serializer.Deserialize(reader) as List<InvestmentPortfolio>;
                        if (loadedData != null && loadedData.Count > 0)
                        {
                            return loadedData;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"XML read error (loading defaults): {ex.Message}");
                }
            }

            return GenerateMockData();
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
                Owner = "John Doe",
                InvestmentPortfolioId = Guid.NewGuid()
            };
            p2.AddNewAsset(new Commodity("Gold", "GOLD", 10, 2000.0, UnitEnum.Ounce));
            p2.AddNewAsset(new Stock("Apple Inc.", "AAPL", 10, 130.0));
            portfolios.Add(p2);

            SavePortfolios(portfolios);

            return portfolios;
        }

        public List<Asset> GetFilteredAssets(InvestmentPortfolio portfolio, double? minPrice, double? maxPrice, RiskEnum? riskLevel, string? nameFragment)
        {
            if (portfolio == null || portfolio.Assets == null)
                return new List<Asset>();

            var query = portfolio.Assets.AsEnumerable();

            if (minPrice.HasValue)
                query = query.Where(a => a.CurrentPrice >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(a => a.CurrentPrice <= maxPrice.Value);

            if (riskLevel.HasValue)
                query = query.Where(a => a.GetRiskAssessment() == riskLevel.Value);

            if (!string.IsNullOrWhiteSpace(nameFragment))
            {
                query = query.Where(a =>
                    a.AssetName.Contains(nameFragment, StringComparison.OrdinalIgnoreCase) ||
                    a.AssetSymbol.Contains(nameFragment, StringComparison.OrdinalIgnoreCase));
            }

            return query.ToList();
        }
    }
}