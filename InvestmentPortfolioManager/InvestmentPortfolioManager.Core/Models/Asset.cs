using InvestmentPortfolioManager.Core.Enums;
using InvestmentPortfolioManager.Core.Exceptions;
using InvestmentPortfolioManager.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
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
    [XmlInclude(typeof(RealEstate))]
    [XmlInclude(typeof(Commodity))]
    public abstract class Asset : IAsset, IComparable<Asset>, ICloneable, INotifyPropertyChanged, IEquatable<Asset>
    {
        public Guid InvestmentPortfolioId { get; set; }
        public virtual InvestmentPortfolio? InvestmentPortfolio { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public event AssetPriceChangedHandler? OnPriceUpdate;
        public event AssetPriceChangedHandler? OnCriticalDrop;

        private double quantity;
        private double currentPrice;
        private string assetName = string.Empty;
        private string assetSymbol = string.Empty;

        public virtual bool IsMergeable => true;

        public Guid Asset_id { get; set; } = Guid.NewGuid();
        
        public double PurchasePrice { get; init; }
        public double Volatility { get; set; }
        public double MeanReturn { get; set; }
        public double? LowPriceThreshold { get; set; }

        public ObservableCollection<PricePoint> PriceHistory { get; private set; } = [];

        public string AssetName
        {
            get => assetName;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new AssetNameException("Asset name can't be empty");
                assetName = value;
            }
        }

        public string AssetSymbol
        {
            get => assetSymbol;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new AssetSymbolException("Asset symbol can't be empty");
                assetSymbol = value.ToUpper();
            }
        }

        public double Quantity
        {
            get => quantity;
            set
            {
                if (value <= 0) throw new InvalidQuantityException("Quantity must be greater than 0");
                quantity = value;

                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(Value));
            }
        }

        public double CurrentPrice
        {
            get => currentPrice;
            protected set
            {
                if (value < 0) throw new InvalidPriceException("Price can't be lower than 0");

                bool hasChanged = Math.Abs(currentPrice - value) > 0.0001;

                if (hasChanged)
                {
                    double oldPrice = currentPrice;
                    currentPrice = value;

                    OnPropertyChanged(nameof(CurrentPrice));
                    OnPropertyChanged(nameof(Value));

                    if (OnPriceUpdate != null)
                    {
                        string movement = value > oldPrice ? "rose" : "dropped";
                        string msg = $"Price {movement} by {Math.Abs(value - oldPrice):c}";
                        OnPriceUpdate.Invoke(AssetSymbol, currentPrice, msg);
                    }

                    if (LowPriceThreshold.HasValue && currentPrice < LowPriceThreshold.Value)
                    {
                        OnCriticalDrop?.Invoke(AssetSymbol, currentPrice, $"CRITICAL: Below {LowPriceThreshold.Value:c}");
                    }
                }
            }
        }
        
        public double Value => Quantity * CurrentPrice;

        public Asset() { }

        protected Asset(string name, string symbol, double quantity, double purchasePrice, double volatility) : this()
        {         
            AssetName = name;
            AssetSymbol = symbol;
            Quantity = quantity;
            PurchasePrice = purchasePrice;
            CurrentPrice = purchasePrice;
            Volatility = volatility;
            MeanReturn = 0.0002;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int CompareTo(Asset? other) => other == null ? 1 : Value.CompareTo(other.Value);

        public virtual RiskEnum GetRiskAssessment() => RiskEnum.Medium;

        public abstract void SimulatePriceChange(DateTime simulationDate);

        public object Clone()
        {
            var clone = (Asset)this.MemberwiseClone();
            clone.Asset_id = Guid.NewGuid();
            clone.InvestmentPortfolioId = Guid.Empty;
            clone.InvestmentPortfolio = null;
            clone.PriceHistory = new ObservableCollection<PricePoint>(this.PriceHistory);

            return clone;
        }

        public bool Equals(Asset? other)
        {
            if (other == null) return false;
            return this.Asset_id == other.Asset_id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Asset asset)
                return Equals(asset);
            return false;
        }

        public override int GetHashCode()
        {
            return Asset_id.GetHashCode();
        }

        public void UpdatePrice(double newPrice)
        {
            CurrentPrice = newPrice;

            if (CurrentPrice < LowPriceThreshold)
            {
                OnCriticalDrop?.Invoke(AssetSymbol, CurrentPrice, "Price fell below threshold!");
            }
        }
    }
}
