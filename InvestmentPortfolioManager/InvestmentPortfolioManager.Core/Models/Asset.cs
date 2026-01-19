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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestmentPortfolioManager.Core.Models
{
    /// <summary>
    /// Klasa reprezentująca pojedynczy wpis w historii cen aktywa.
    /// Jest to encja mapowana do bazy danych, przechowująca datę i wartość notowania.
    /// </summary>
    public class PricePoint
    {
        [Key]
        public Guid PricePointId { get; set; } = Guid.NewGuid();

        public DateTime Date { get; set; }
        public double Price { get; set; }

        public Guid AssetId { get; set; }

        [ForeignKey("AssetId")]
        public virtual Asset? Asset { get; set; }

        public PricePoint() { }

        public PricePoint(DateTime date, double price)
        {
            Date = date;
            Price = price;
        }
    }

    /// <summary>
    /// Delegat obsługujący zdarzenia zmiany ceny aktywa.
    /// </summary>
    public delegate void AssetPriceChangedHandler(string symbol, double newPrice, string message);

    /// <summary>
    /// Abstrakcyjna klasa bazowa dla wszystkich instrumentów finansowych.
    /// Zawiera wspólną logikę dla cen, ilości, walidacji oraz historii notowań.
    /// </summary>  
    [XmlInclude(typeof(Stock))]
    [XmlInclude(typeof(Bond))]
    [XmlInclude(typeof(Cryptocurrency))]
    [XmlInclude(typeof(RealEstate))]
    [XmlInclude(typeof(Commodity))]
    public abstract class Asset : IAsset, IComparable<Asset>, ICloneable, INotifyPropertyChanged, IEquatable<Asset>
    {

        public Guid InvestmentPortfolioId { get; set; }
        [XmlIgnore]
        public virtual InvestmentPortfolio? InvestmentPortfolio { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Zdarzenie wywoływane przy każdej znaczącej zmianie ceny.
        /// </summary>
        public event AssetPriceChangedHandler? OnPriceUpdate;

        /// <summary>
        /// Zdarzenie wywoływane, gdy cena spadnie poniżej zdefiniowanego progu <see cref="LowPriceThreshold"/>.
        /// </summary>
        public event AssetPriceChangedHandler? OnCriticalDrop;

        private double quantity;
        private double currentPrice;
        private string assetName = string.Empty;
        private string assetSymbol = string.Empty;

        /// <summary>
        /// Określa, czy aktywa tego samego typu mogą być łączone w jedną pozycję (np. akcje tak, nieruchomości nie).
        /// </summary>
        public virtual bool IsMergeable => true;


        [Key] public Guid Asset_id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Cena zakupu (używana do obliczania zysku/straty).
        /// </summary>
        public double PurchasePrice { get; set; }

        /// <summary>
        /// Współczynnik zmienności ceny (używany w symulacjach).
        /// </summary>
        public double Volatility { get; set; }

        /// <summary>
        /// Średni oczekiwany zwrot (używany w symulacjach).
        /// </summary>
        public double MeanReturn { get; set; } = 0.0002;

        private double? lowPriceThreshold;

        /// <summary>
        /// Próg ceny, poniżej którego zostanie wywołany alarm (event <see cref="OnCriticalDrop"/>).
        /// </summary>
        public double? LowPriceThreshold
        {
            get => lowPriceThreshold;
            set
            {
                if (lowPriceThreshold != value)
                {
                    lowPriceThreshold = value;
                    OnPropertyChanged();
                }
            }
        }

        public virtual ObservableCollection<PricePoint> PriceHistory { get; set; } = new();

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

        [NotMapped]
        public string AssetTypeName => this.GetType().Name;

        /// <summary>
        /// Ilość posiadanych jednostek aktywa.
        /// </summary>
        /// <exception cref="InvalidQuantityException">Rzucany, gdy wartość jest mniejsza lub równa 0.</exception>
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

        /// <summary>
        /// Aktualna cena rynkowa pojedynczej jednostki.
        /// Zmiana tej właściwości może wywołać zdarzenia <see cref="OnPriceUpdate"/> oraz <see cref="OnCriticalDrop"/>.
        /// </summary>
        /// <exception cref="InvalidPriceException">Rzucany, gdy cena jest ujemna.</exception>
        public double CurrentPrice
        {
            get => currentPrice;
            set
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

        /// <summary>
        /// Całkowita wartość pozycji (Ilość * Cena Aktualna).
        /// </summary>
        public double Value => Quantity * CurrentPrice;
        [XmlIgnore]
        public double ValueChange => (CurrentPrice - PurchasePrice) * Quantity;
        [XmlIgnore]
        public bool HasPositiveChange => ValueChange >= 0;

        public Asset() { }

        protected Asset(string name, string symbol, double quantity, double purchasePrice, double volatility) : this()
        {
            AssetName = name;
            AssetSymbol = symbol;
            Quantity = quantity;
            PurchasePrice = purchasePrice;
            CurrentPrice = purchasePrice;
            Volatility = volatility;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int CompareTo(Asset? other) => other == null ? 1 : Value.CompareTo(other.Value);

        /// <inheritdoc />
        public virtual RiskEnum GetRiskAssessment() => RiskEnum.Medium;

        /// <inheritdoc />
        public abstract void SimulatePriceChange(DateTime simulationDate);

        /// <summary>
        /// Tworzy głęboką kopię obiektu aktywa z nowym ID i wyczyszczonym powiązaniem do portfela.
        /// </summary>
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

        /// <summary>
        /// Ręcznie aktualizuje cenę aktywa i sprawdza progi alarmowe.
        /// </summary>
        public void UpdatePrice(double newPrice)
        {
            CurrentPrice = newPrice;

            if (CurrentPrice < LowPriceThreshold)
            {
                OnCriticalDrop?.Invoke(AssetSymbol, CurrentPrice, "Price fell below threshold!");
            }
        }

        [NotMapped]
        public double MinPriceHistory => PriceHistory != null && PriceHistory.Count > 0 ? PriceHistory.Min(p => p.Price) : CurrentPrice;

        [NotMapped]
        public double MaxPriceHistory => PriceHistory != null && PriceHistory.Count > 0 ? PriceHistory.Max(p => p.Price) : CurrentPrice;
    }
}
