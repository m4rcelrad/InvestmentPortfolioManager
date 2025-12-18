using InvestmentPortfolioManager.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolioManager.Core.Interfaces
{
    internal interface IAsset
    {
        void SimulatePriceChange(DateTime simulationDate);
        RiskEnum GetRiskAssessment();
    }
}
