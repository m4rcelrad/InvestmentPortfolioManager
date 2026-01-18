using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolioManager.Core.Enums
{
    /// <summary>
    /// Określa poziom ryzyka inwestycyjnego dla danego aktywa.
    /// </summary>
    public enum RiskEnum
    {
        /// <summary>
        /// Niskie ryzyko (np. obligacje skarbowe, stabilne fundusze).
        /// </summary>
        Low,

        /// <summary>
        /// Średnie ryzyko (np. akcje blue chip, zrównoważone portfele).
        /// </summary>
        Medium,

        /// <summary>
        /// Wysokie ryzyko (np. akcje rynków wschodzących, agresywne fundusze).
        /// </summary>
        High,

        /// <summary>
        /// Ekstremalnie wysokie ryzyko (np. kryptowaluty, derywaty, penny stocks).
        /// </summary>
        ExtremelyHigh
    }

    /// <summary>
    /// Jednostki miary używane dla surowców i towarów (Commodities).
    /// </summary>
    public enum UnitEnum
    {
        /// <summary>
        /// Uncja (np. dla złota, srebra).
        /// </summary>
        Ounce,

        /// <summary>
        /// Baryłka (np. dla ropy naftowej).
        /// </summary>
        Barrel,

        /// <summary>
        /// Tona (np. dla węgla, stali).
        /// </summary>
        Ton,

        /// <summary>
        /// Kilogram.
        /// </summary>
        Kilogram,

        /// <summary>
        /// Gram.
        /// </summary>
        Gram,

        /// <summary>
        /// Buszel (np. dla zbóż: pszenica, kukurydza).
        /// </summary>
        Bushel,

        /// <summary>
        /// Litr.
        /// </summary>
        Liter,

        /// <summary>
        /// Megawatogodzina (np. dla energii elektrycznej).
        /// </summary>
        MWh
    }
}
