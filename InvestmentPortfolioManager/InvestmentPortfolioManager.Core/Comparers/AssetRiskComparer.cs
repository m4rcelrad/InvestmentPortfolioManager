using InvestmentPortfolioManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolioManager.Core.Comparers
{
    /// <summary>
    /// Komparator służący do porównywania aktywów na podstawie ich oceny ryzyka.
    /// Umożliwia sortowanie list aktywów od najmniej do najbardziej ryzykownych.
    /// </summary>
    public class AssetRiskComparer : IComparer<Asset>
    {
        /// <summary>
        /// Porównuje dwa obiekty typu <see cref="Asset"/> na podstawie ich poziomu ryzyka.
        /// </summary>
        /// <param name="x">Pierwsze aktywo do porównania.</param>
        /// <param name="y">Drugie aktywo do porównania.</param>
        /// <returns>
        /// Wartość mniejszą od zera, jeśli x jest mniej ryzykowne niż y.
        /// Zero, jeśli ryzyko jest równe lub obiekty są nullem.
        /// Wartość większą od zera, jeśli x jest bardziej ryzykowne niż y.
        /// </returns>
        public int Compare(Asset? x, Asset? y)
        {
            if (x == null || y == null) return 0;

            return x.GetRiskAssessment().CompareTo(y.GetRiskAssessment());
        }
    }
}
