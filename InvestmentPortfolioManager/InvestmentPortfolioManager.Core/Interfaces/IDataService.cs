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
        void SavePortfolios(List<InvestmentPortfolio> portfolios);

        List<InvestmentPortfolio> LoadAllPortfolios();

        List<Asset> GetFilteredAssets(InvestmentPortfolio portfolio, double? minPrice, double? maxPrice, RiskEnum? riskLevel, string? nameFragment)

    }
}
