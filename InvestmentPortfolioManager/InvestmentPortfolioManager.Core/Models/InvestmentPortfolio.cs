using InvestmentPortfolioManager.Core.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace InvestmentPortfolioManager.Core.Models
{
    public class InvestmentPortfolio : IEnumerable<Asset>, ICloneable
    {
        [Key] public Guid InvestmentPortfolioId { get; set; } = Guid.NewGuid();
        public virtual ObservableCollection<Asset> Assets { get; set; } = [];

        string owner = string.Empty;
        string Name { get; set; } = "New portfolio";

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

        public Dictionary<string, LiveAssetSummary> PortfolioSummaries { get; } = [];

        public IEnumerable<Asset> this[string symbol]
        {
            get => Assets.Where(a => a.AssetSymbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));
        }

        public InvestmentPortfolio() { }

        public void AddNewAsset(Asset asset)
        {
            ArgumentNullException.ThrowIfNull(asset);

            Assets.Add(asset);
            asset.PropertyChanged += OnAssetPropertyChanged;
            UpdateSummary(asset);
        }

        public bool RemoveAsset(Asset asset)
        {
            if (Assets.Remove(asset))
            {
                asset.PropertyChanged -= OnAssetPropertyChanged;
                UpdateSummary(asset);
                return true;
            }
            return false;
        }

        private void OnAssetPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is Asset asset && (e.PropertyName == nameof(Asset.CurrentPrice) || e.PropertyName == nameof(Asset.Quantity)))
            {
                UpdateSummary(asset);
            }
        }

        private void UpdateSummary(Asset asset)
        {
            if (asset.IsMergeable)
            {
                string symbol = asset.AssetSymbol;
                var assetsOfSymbol = Assets.Where(a => a.AssetSymbol == symbol).ToList();

                if (assetsOfSymbol.Count == 0)
                {
                    PortfolioSummaries.Remove(symbol);
                    return;
                }

                var totalQuantity = assetsOfSymbol.Sum(a => a.Quantity);
                var totalCost = assetsOfSymbol.Sum(a => a.PurchasePrice * a.Quantity);
                var totalValue = assetsOfSymbol.Sum(a => a.Value);

                if (!PortfolioSummaries.TryGetValue(symbol, out var summary))
                {
                    summary = new LiveAssetSummary { AssetSymbol = symbol, AssetName = assetsOfSymbol.First().AssetName };
                    PortfolioSummaries.Add(symbol, summary);
                }

                summary.TotalQuantity = totalQuantity;
                summary.TotalCost = totalCost;
                summary.TotalValue = totalValue;
                summary.AveragePurchasePrice = totalQuantity > 0 ? totalCost / totalQuantity : 0;
            }
            else
            {
                string uniqueKey = $"{asset.AssetSymbol}_{asset.Asset_id}";
                bool exists = Assets.Contains(asset);

                if (!exists)
                {
                    PortfolioSummaries.Remove(uniqueKey);
                    return;
                }

                if (!PortfolioSummaries.TryGetValue(uniqueKey, out var summary))
                {
                    summary = new LiveAssetSummary
                    {
                        AssetSymbol = asset.AssetSymbol,
                        AssetName = asset.AssetName
                    };
                    PortfolioSummaries.Add(uniqueKey, summary);
                }

                summary.TotalQuantity = asset.Quantity;
                summary.TotalCost = asset.PurchasePrice * asset.Quantity;
                summary.TotalValue = asset.Value;
                summary.AveragePurchasePrice = asset.PurchasePrice;
            }
        }

        public bool RemoveAsset(Guid id)
        {
            Asset? toRemove = Assets.FirstOrDefault(x => x.Asset_id == id);
            if (toRemove != null)
            {
                return Assets.Remove(toRemove);
            }
            return false;
        }

        public double CalculateSum() => Assets.Sum(x => x.Value);

        public void UpdateMarketPrices(DateTime simulationDate)
        {
            var groupedAssets = Assets.GroupBy(asset =>
            {
                if (asset is Commodity commodity)
                {
                    return $"{commodity.AssetSymbol}_{commodity.Unit}";
                }

                if (asset is RealEstate realEstate)
                {
                    return $"{realEstate.AssetSymbol}_{realEstate.AssetName}";
                }

                return asset.AssetSymbol;
            });

            foreach (var group in groupedAssets)
            {
                var leader = group.First();

                leader.SimulatePriceChange(simulationDate);

                double newMarketPrice = leader.CurrentPrice;

                foreach (var asset in group)
                {
                    if (asset == leader) continue;

                    asset.UpdatePrice(newMarketPrice);

                    asset.PriceHistory.Add(new PricePoint(simulationDate, newMarketPrice));
                }
            }
        }

        public IEnumerable<Asset> FindAssets(Func<Asset, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return Assets.Where(predicate);
        }

        public Dictionary<string, double> GetAssetAllocation()
        {
            double total = CalculateSum();
            if (total == 0) return [];

            return Assets
                .GroupBy(a => a.GetType().Name)
                .ToDictionary(g => g.Key, g => g.Sum(a => a.Value) / total * 100);
        }

        public IEnumerable<Asset> GetTopMovers(int count)
        {
            return Assets.OrderByDescending(a => (a.CurrentPrice - a.PurchasePrice) / a.PurchasePrice).Take(count);
        }

        public IEnumerator<Asset> GetEnumerator()
        {
            return Assets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public object Clone()
        {
            return this.Clone(this.Name + " - Clone");
        }

        public object Clone(string newName)
        {
            var clone = new InvestmentPortfolio
            {
                InvestmentPortfolioId = Guid.NewGuid(),
                Name = newName,
                Owner = this.Owner
            };

            foreach (var asset in this.Assets)
            {
                var assetClone = (Asset)asset.Clone();
                clone.AddNewAsset(assetClone);
            }

            return clone;
        }
    }
}
