using InvestmentPortfolioManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolioManager.Core.Comparers
{
    internal class AssetRiskComparer : IComparer<Asset>
    {
        public int Compare(Asset? x, Asset? y)
        {
            if (x == null || y == null) return 0;

            return x.GetRiskAssessment().CompareTo(y.GetRiskAssessment());
        }
    }
}
