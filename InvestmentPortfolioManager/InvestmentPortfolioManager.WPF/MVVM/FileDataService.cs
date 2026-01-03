using InvestmentPortfolioManager.Core.Enums;
using InvestmentPortfolioManager.Core.Interfaces;
using InvestmentPortfolioManager.Core.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace InvestmentPortfolioManager.WPF.MVVM
{
    public class FileDataService : IDataService
    {
        private readonly string _filePath = "portfolio.json";

        public List<Asset> LoadPortfolio()
        {
            if (!File.Exists(_filePath))
                return new List<Asset>();

            string json = File.ReadAllText(_filePath);
            var assets = JsonSerializer.Deserialize<List<Asset>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return assets ?? new List<Asset>();
        }

        public void SavePortfolio(List<Asset> assets)
        {
            string json = JsonSerializer.Serialize(assets,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }

        // IMPLEMENTACJA brakującej metody z IDataService
        public List<Asset> GetFilteredAssets(double? minValue, double? maxValue, RiskEnum? risk, string? name)
        {
            var assets = LoadPortfolio();

            if (minValue.HasValue)
                assets = assets.Where(a => a.Value >= minValue.Value).ToList();

            if (maxValue.HasValue)
                assets = assets.Where(a => a.Value <= maxValue.Value).ToList();

            if (risk.HasValue)
                assets = assets.Where(a => a.GetRiskAssessment() == risk.Value).ToList();

            if (!string.IsNullOrWhiteSpace(name))
                assets = assets.Where(a => a.AssetName.Contains(name, System.StringComparison.OrdinalIgnoreCase)).ToList();

            return assets;
        }
    }
}
