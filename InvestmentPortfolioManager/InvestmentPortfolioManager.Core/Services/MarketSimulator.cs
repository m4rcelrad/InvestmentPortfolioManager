using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolioManager.Core.Services
{
    public static class MarketSimulator
    {
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
