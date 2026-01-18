using InvestmentPortfolioManager.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolioManager.Core.Interfaces
{
    /// <summary>
    /// Definiuje podstawowe zachowania dla każdego aktywa w systemie.
    /// </summary>
    internal interface IAsset
    {
        /// <summary>
        /// Symuluje zmianę ceny aktywa w czasie na podstawie zdefiniowanej zmienności i trendu.
        /// </summary>
        /// <param name="simulationDate">Data, dla której generowana jest nowa cena.</param>
        void SimulatePriceChange(DateTime simulationDate);

        /// <summary>
        /// Zwraca ocenę ryzyka inwestycyjnego dla danego typu aktywa.
        /// </summary>
        /// <returns>Wartość z enum <see cref="RiskEnum"/> określająca poziom ryzyka.</returns>
        RiskEnum GetRiskAssessment();
    }
}
