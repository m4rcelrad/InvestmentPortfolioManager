using InvestmentPortfolioManager.Core.Enums;
using InvestmentPortfolioManager.Core.Exceptions;
using InvestmentPortfolioManager.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

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

    public delegate void AssetPriceChangedHandler(string symbol, double newPrice, string message);

    [XmlInclude(typeof(Stock))]
    [XmlInclude(typeof(Bond))]
    [XmlInclude(typeof(Cryptocurrency))]
    public abstract class Asset : IAsset, IComparable<Asset>, ICloneable
    {
        public event AssetPriceChangedHandler? OnPriceUpdate;
        public event AssetPriceChangedHandler? OnCriticalDrop;

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

                bool hasChanged = Math.Abs(currentPrice - value) > 0.0001;
                double oldPrice = currentPrice;
                currentPrice = value;

                if (hasChanged && OnPriceUpdate != null)
                {
                    string movement = value > oldPrice ? "rose" : "dropped";
                    string msg = $"Price {movement} by {Math.Abs(value - oldPrice):c}";

                    OnPriceUpdate.Invoke(AssetSymbol, currentPrice, msg);
                }

                if (LowPriceThreshold.HasValue && currentPrice < LowPriceThreshold.Value)
                {
                    string alertMsg = $"CRITICAL WARNING: Price dropped below {LowPriceThreshold.Value:c}!";
                    OnCriticalDrop?.Invoke(AssetSymbol, currentPrice, alertMsg);
                }
            }
        }
        public double Volatility { get; set; }
        public double MeanReturn { get; set; }

        public double? LowPriceThreshold { get; set; }

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

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
