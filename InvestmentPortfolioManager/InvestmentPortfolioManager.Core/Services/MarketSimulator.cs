using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolioManager.Core.Services
{
    /// <summary>
    /// Statyczny serwis dostarczający metody do symulacji zachowań rynku finansowego.
    /// </summary>
    public static class MarketSimulator
    {
        /// <summary>
        /// Generuje nową cenę aktywa przy użyciu modelu Geometrycznego Ruchu Browna (GBM).
        /// </summary>
        /// <param name="currentPrice">Ostatnia znana cena aktywa.</param>
        /// <param name="meanReturn">Oczekiwana średnia stopa zwrotu (dryf).</param>
        /// <param name="volatility">Współczynnik zmienności (ryzyko/odchylenie standardowe).</param>
        /// <returns>Nowa cena po uwzględnieniu losowego szoku rynkowego.</returns>
        /// <remarks>
        /// Metoda implementuje transformację Boxa-Mullera w celu wygenerowania rozkładu normalnego,
        /// który jest następnie używany do obliczenia wykładniczego wzrostu ceny.
        /// </remarks>
        public static double GenerateNewPrice(double currentPrice, double meanReturn, double volatility)
        {
            double u1 = 1.0 - Random.Shared.NextDouble();
            double u2 = 1.0 - Random.Shared.NextDouble();

            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            double drift = meanReturn - (0.5 * Math.Pow(volatility, 2));
            double shock = volatility * randStdNormal;

            return currentPrice * Math.Exp(drift + shock);
        }

    }
}
