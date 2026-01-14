using InvestmentPortfolioManager.Core.Interfaces;
using InvestmentPortfolioManager.Core;
using InvestmentPortfolioManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvestmentPortfolioManager.Core.Enums;

namespace InvestmentPortfolioManager.Data
{
    public class SqlDatabaseService : IDataService
    {

        public List<Asset> GetFilteredAssets(double? minPrice, double? maxPrice, RiskEnum? riskLevel, string? nameFragment)
        {
            throw new NotImplementedException();
        }

        public List<Asset> LoadPortfolio()
        {
            throw new NotImplementedException();
        }

        public void SavePortfolio(List<Asset> assets)
        {
            throw new NotImplementedException();
        }
    }
}
