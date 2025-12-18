using InvestmentPortfolioManager.Core.Enums;
using InvestmentPortfolioManager.Core.Exceptions;
using InvestmentPortfolioManager.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolioManager.Core.Models
{
    public class Stock : Asset
    {
        public Stock(string name, string symbol, double quantity, double price)
            : base(name, symbol, quantity, price, volatility: 0.02) { }

        public override void SimulatePriceChange(DateTime simulationDate)
        {
            CurrentPrice = MarketSimulator.GenerateNewPrice(CurrentPrice, MeanReturn, Volatility);
            PriceHistory.Add(new PricePoint(simulationDate, CurrentPrice));
        }

        public override RiskEnum GetRiskAssessment()
        {
            return RiskEnum.High;
        }
    }

    public class Bond : Asset
    {
        private double rate;
        public double Rate
        {
            get => rate;
            set
            {
                if (value < 0) throw new BondRateException("Bond rate can't be lower than 0.");
                rate = value;
            }
        }

        public Bond(string name, string symbol, double quantity, double price, double rate)
            : base(name, symbol, quantity, price, volatility: 0)
        {
            Rate = rate;
        }

        public override void SimulatePriceChange(DateTime simulationDate)
        {
            double dailyInterest = CurrentPrice * (Rate / 365.0);
            CurrentPrice += dailyInterest;

            PriceHistory.Add(new PricePoint(simulationDate, CurrentPrice));
        }

    }
    
    public class Cryptocurrency : Asset
    {
        public Cryptocurrency(string name, string symbol, double quantity, double price)
            : base(name, symbol, quantity, price, volatility: 0.08) { }

        public override void SimulatePriceChange(DateTime simulationDate)
        {
            CurrentPrice = MarketSimulator.GenerateNewPrice(CurrentPrice, MeanReturn, Volatility);
            PriceHistory.Add(new PricePoint(simulationDate, CurrentPrice));
        }

        public override RiskEnum GetRiskAssessment()
        {
            return RiskEnum.ExtremelyHigh;
        }
    }
}
