using InvestmentPortfolioManager.Core.Enums;
using InvestmentPortfolioManager.Core.Exceptions;
using InvestmentPortfolioManager.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolioManager.Core.Models
{
 
    public struct PricePoint
    {
        public DateTime Date { get; set; }
        public double Price { get; set; }

        public PricePoint(DateTime date, double price)
        {
            Date = date;
            Price = price;
        }
    }

    public abstract class Asset : IComparable<Asset>, IAsset
    {
        private double quantity;
        private double currentPrice;

        public Guid Asset_id { get; init; }
        public string AssetName {  get; set; } = string.Empty;
        public string AssetSymbol { get; set; } = string.Empty;

        public double PurchasePrice { get; init; }

        public double Quantity
        {
            get => quantity;
            set
            {
                if (value <= 0)
                {
                    throw new InvalidQuantityException("Quantity must be greater than 0");
                }
                else quantity = value;
            }
        }

        public double CurrentPrice
        {
            get => currentPrice;
            protected set
            {
                if (value < 0)
                {
                    throw new InvalidPriceException("Price can't be lower than 0");
                }

                currentPrice = value;
            }
        }
        public double Volatility { get; set; }
        public double MeanReturn { get; set; }

        public ObservableCollection<PricePoint> PriceHistory { get; private set; } = [];

        public Asset()
        {
            Asset_id = Guid.NewGuid();
        }

        protected Asset(string name, string symbol, double quantity, double purchasePrice, double volatility) : this()
        {
            if (string.IsNullOrWhiteSpace(name)) throw new AssetNameException("Asset name can't be empty");
            if (string.IsNullOrWhiteSpace(symbol)) throw new AssetSymbolException("Asset symbol can't be empty");

            AssetName = name;
            AssetSymbol = symbol.ToUpper();
            Quantity = quantity;
            PurchasePrice = purchasePrice;
            CurrentPrice = purchasePrice;
            Volatility = volatility;
            MeanReturn = 0.0002;
        }

        public double CalculateValue()
        {
            return quantity * currentPrice;
        }     

        public int CompareTo(Asset? other)
        {
            if (other == null) return 1;

            return CalculateValue().CompareTo(other.CalculateValue());
        }

        public virtual RiskEnum GetRiskAssessment()
        {
            return RiskEnum.Medium;
        }

        public abstract void SimulatePriceChange(DateTime simulationDate);
    }
}
