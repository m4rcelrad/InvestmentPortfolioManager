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
    /// <summary>
    /// Reprezentuje akcję spółki giełdowej.
    /// </summary>
    public class Stock : Asset
    {
        public Stock() { }
        public Stock(string name, string symbol, double quantity, double price)
            : base(name, symbol, quantity, price, volatility: 0.02) { }

        /// <inheritdoc />
        public override void SimulatePriceChange(DateTime simulationDate)
        {
            CurrentPrice = MarketSimulator.GenerateNewPrice(CurrentPrice, MeanReturn, Volatility);
            PriceHistory.Add(new PricePoint(simulationDate, CurrentPrice));
        }

        /// <inheritdoc />
        public override RiskEnum GetRiskAssessment()
        {
            return RiskEnum.High;
        }
    }

    /// <summary>
    /// Reprezentuje obligację (instrument dłużny) o stałym oprocentowaniu.
    /// </summary>
    public class Bond : Asset
    {
        private double rate;

        /// <summary>
        /// Roczna stopa oprocentowania (w formacie dziesiętnym, np. 0.05 dla 5%).
        /// </summary>
        /// <exception cref="BondRateException">Rzucany, gdy stopa jest ujemna.</exception>
        public double Rate
        {
            get => rate;
            set
            {
                if (value < 0) throw new BondRateException("Bond rate can't be lower than 0.");
                rate = value;
            }
        }

        public Bond() { }
        public Bond(string name, string symbol, double quantity, double price, double rate)
            : base(name, symbol, quantity, price, volatility: 0)
        {
            Rate = rate;
        }

        /// <summary>
        /// Symuluje wzrost wartości obligacji poprzez naliczenie dziennych odsetek.
        /// W przeciwieństwie do innych aktywów, cena obligacji rośnie deterministycznie.
        /// </summary>
        public override void SimulatePriceChange(DateTime simulationDate)
        {
            double dailyInterest = CurrentPrice * (Rate / 365.0);
            CurrentPrice += dailyInterest;

            PriceHistory.Add(new PricePoint(simulationDate, CurrentPrice));
        }

    }

    /// <summary>
    /// Reprezentuje kryptowalutę. Charakteryzuje się bardzo wysoką zmiennością i ryzykiem.
    /// </summary>
    public class Cryptocurrency : Asset
    {
        public Cryptocurrency() { }
        public Cryptocurrency(string name, string symbol, double quantity, double price)
            : base(name, symbol, quantity, price, volatility: 0.08) { }

        /// <inheritdoc />
        public override void SimulatePriceChange(DateTime simulationDate)
        {
            CurrentPrice = MarketSimulator.GenerateNewPrice(CurrentPrice, MeanReturn, Volatility);
            PriceHistory.Add(new PricePoint(simulationDate, CurrentPrice));
        }

        /// <inheritdoc />
        public override RiskEnum GetRiskAssessment()
        {
            return RiskEnum.ExtremelyHigh;
        }
    }

    /// <summary>
    /// Reprezentuje nieruchomość.
    /// Jest unikalna (nie można łączyć) i posiada szczegółowe dane adresowe.
    /// </summary>
    public class RealEstate : Asset
    {
        /// <summary>
        /// Nieruchomości nie są łączone (każda jest unikalna), dlatego zawsze zwraca false.
        /// </summary>
        public override bool IsMergeable => false;
            
        private string street = string.Empty;
        private string houseNumber = string.Empty;
        private string city = string.Empty;
        private string zipCode = string.Empty;
        private string country = string.Empty;

        public string Street
        {
            get => street;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidAddressException("Street cannot be empty.");
                street = value;
            }
        }

        public string HouseNumber
        {
            get => houseNumber;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidAddressException("House number cannot be empty.");
                houseNumber = value;
            }
        }

        public string FlatNumber { get; set; } = string.Empty;

        public string City
        {
            get => city;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidAddressException("City cannot be empty.");
                city = value;
            }
        }

        public string ZipCode
        {
            get => zipCode;
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value.Length < 3)
                    throw new InvalidZipCodeException($"Invalid ZipCode format: {value}");
                zipCode = value;
            }
        }

        public string Country
        {
            get => country;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidAddressException("Country cannot be empty.");
                country = value;
            }
        }

        public RealEstate() { }
        public RealEstate(string name, double purchasePrice, string street, string houseNumber, string city, string zipCode, string country, string flatNumber = "")
            : base(name, "PROP", quantity: 1, purchasePrice, volatility: 0.005)
        {
            Street = street;
            HouseNumber = houseNumber;
            FlatNumber = flatNumber;
            City = city;
            ZipCode = zipCode;
            Country = country;
            MeanReturn = 0.00015;
        }

        /// <summary>
        /// Symuluje zmianę wartości nieruchomości.
        /// Aktualizacja następuje tylko pierwszego dnia miesiąca ze względu na niską płynność rynku.
        /// </summary>
        public override void SimulatePriceChange(DateTime simulationDate)
        {
            if (simulationDate.Day == 1)
            {
                CurrentPrice = MarketSimulator.GenerateNewPrice(CurrentPrice, MeanReturn, Volatility);
                PriceHistory.Add(new PricePoint(simulationDate, CurrentPrice));
            }
        }

        /// <inheritdoc />
        public override RiskEnum GetRiskAssessment()
        {
            return RiskEnum.Low;
        }
    }

    /// <summary>
    /// Reprezentuje towary i surowce (np. złoto, ropa, zboża).
    /// </summary>
    public class Commodity : Asset
    {
        private UnitEnum unit;

        /// <summary>
        /// Jednostka miary towaru (np. Uncja, Baryłka).
        /// </summary>
        /// <exception cref="InvalidUnitException">Rzucany, gdy podana jednostka jest spoza definicji Enum.</exception>
        public UnitEnum Unit
        {
            get => unit;
            set
            {
                if (!Enum.IsDefined(typeof(UnitEnum), value))
                {
                    throw new InvalidUnitException($"Undefined unit type value: {value}");
                }
                unit = value;
            }
        }

        public Commodity() { }

        public Commodity(string name, string symbol, double quantity, double price, UnitEnum unit)
            : base(name, symbol, quantity, price, volatility: 0.015)
        {
            Unit = unit;
            MeanReturn = 0.0003;
        }

        /// <inheritdoc />
        public override void SimulatePriceChange(DateTime simulationDate)
        {
            CurrentPrice = MarketSimulator.GenerateNewPrice(CurrentPrice, MeanReturn, Volatility);
            PriceHistory.Add(new PricePoint(simulationDate, CurrentPrice));
        }

        /// <inheritdoc />
        public override RiskEnum GetRiskAssessment()
        {
            return RiskEnum.Medium;
        }

    }
}
