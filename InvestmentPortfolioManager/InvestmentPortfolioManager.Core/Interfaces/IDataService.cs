using InvestmentPortfolioManager.Core.Enums;
using InvestmentPortfolioManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolioManager.Core.Interfaces
{
    public interface IDataService
    {
        void SavePortfolio(List<Asset> assets);
        
        List<Asset> LoadPortfolio();

        List<Asset> GetFilteredAssets(double? minPrice, double? maxPrice, RiskEnum? riskLevel, string? nameFragment);

    }
}
