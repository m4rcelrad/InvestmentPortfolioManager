using System;
using InvestmentPortfolioManager.Core.Models;

namespace InvestmentPortfolioManager.Core.Models
{
    /// <summary>
    /// Reprezentuje zdarzenie rynkowe, które tymczasowo modyfikuje parametry symulacji aktywów.
    /// </summary>
    public class MarketEvent
    {
        /// <summary>
        /// Tytuł zdarzenia wyświetlany w pasku wiadomości.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Szczegółowy opis zdarzenia widoczny dla użytkownika.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Czas trwania zdarzenia wyrażony w liczbie cykli (tików) zegara symulacji.
        /// </summary>
        public int DurationTicks { get; set; }

        /// <summary>
        /// Funkcja określająca, na które konkretnie aktywa wpływa to zdarzenie.
        /// Zwraca true, jeśli parametry danego aktywa powinny zostać zmodyfikowane.
        /// </summary>
        public Func<Asset, bool> TargetPredicate { get; set; } = _ => false;

        /// <summary>
        /// Mnożnik zmienności. 
        /// Wartość > 1.0 zwiększa amplitudy wahań cen (ryzyko), wartość < 1.0 stabilizuje rynek.
        /// </summary>
        public double VolatilityMultiplier { get; set; } = 1.0;

        /// <summary>
        /// Wartość dodawana do średniego zwrotu. 
        /// Ujemna wartość wymusza trend spadkowy, dodatnia wzrostowy.
        /// </summary>
        public double MeanReturnModifier { get; set; } = 0.0;
    }
}