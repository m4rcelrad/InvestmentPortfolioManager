using InvestmentPortfolioManager.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InvestmentPortfolioManager.Core.Models
{
    public class InvestmentPortfolio
    {
        private readonly ObservableCollection<Asset> _assets = [];
        public ReadOnlyObservableCollection<Asset> Assets { get; }

        string owner = string.Empty;

        public string Owner
        {
            get => owner;
            set
            {
                string pattern = @"^[A-ZĄĆĘŁŃÓŚŹŻ][a-ząćęłńóśźż]+(?:\s[A-ZĄĆĘŁŃÓŚŹŻ][a-ząćęłńóśźż]+)?\s[A-ZĄĆĘŁŃÓŚŹŻ][a-ząćęłńóśźż]+(?:-[A-ZĄĆĘŁŃÓŚŹŻ][a-ząćęłńóśźż]+)?$";

                if (string.IsNullOrEmpty(value) || !Regex.IsMatch(value, pattern))
                {
                    throw new InvalidOwnerException("Please enter a valid owner name");
                }

                owner = value;
            }
        }

        public Asset? this[string symbol]
        {
            get => _assets.FirstOrDefault(a => a.AssetSymbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));
        }

        public InvestmentPortfolio()
        {
            Assets = new ReadOnlyObservableCollection<Asset>(_assets);
        }

        public void AddNewAsset(Asset asset)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));

            _assets.Add(asset);
        }

        public bool RemoveAsset(Asset asset)
        {
            return _assets.Remove(asset);
        }

        public bool RemoveAsset(Guid id)
        {
            Asset? toRemove = _assets.FirstOrDefault(x => x.Asset_id == id);
            if (toRemove != null)
            {
                return _assets.Remove(toRemove);
            }
            return false;
        }

        public double CalculateSum() => _assets.Sum(x => x.Value);

        public void UpdateMarketPrices(DateTime simulationDate)
        {
            foreach (var asset in _assets)
            {
                asset.SimulatePriceChange(simulationDate);
            }
        }

        public IEnumerable<Asset> FindAssets(Func<Asset, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return _assets.Where(predicate);
        }

        public Dictionary<string, double> GetAssetAllocation()
        {
            double total = CalculateSum();
            if (total == 0) return [];

            return _assets
                .GroupBy(a => a.GetType().Name)
                .ToDictionary(g => g.Key, g => g.Sum(a => a.Value) / total * 100);
        }

        public IEnumerable<Asset> GetTopMovers(int count)
        {
            return _assets.OrderByDescending(a => (a.CurrentPrice - a.PurchasePrice) / a.PurchasePrice).Take(count);
        }
    }
}
