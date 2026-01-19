using InvestmentPortfolioManager.Core.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace InvestmentPortfolioManager.Core.Models
{
    /// <summary>
    /// Reprezentuje portfel inwestycyjny użytkownika. 
    /// Zarządza listą aktywów, oblicza statystyki i koordynuje aktualizacje cen rynkowych.
    /// </summary>
    public class InvestmentPortfolio : ICloneable
    {
        /// <summary>Unikalny identyfikator portfela.</summary>
        [Key] public Guid InvestmentPortfolioId { get; set; } = Guid.NewGuid();

        /// <summary>Kolekcja aktywów przypisanych do tego portfela.</summary>
        public virtual ObservableCollection<Asset> Assets { get; set; } = [];

        string owner = string.Empty;

        /// <summary>Nazwa własna portfela (np. "Emerytura", "Spekulacyjny").</summary>
        public string Name { get; set; } = "New portfolio";


        /// <summary>
        /// Imię i nazwisko właściciela portfela.
        /// </summary>
        /// <exception cref="InvalidOwnerException">
        /// Wyrzucany, gdy imię i nazwisko nie spełnia wymagań formatu (np. brak wielkich liter, niedozwolone znaki).
        /// </exception>
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

        /// <summary>
        /// Słownik przechowujący zagregowane podsumowania dla grup aktywów (np. suma wszystkich akcji Apple).
        /// Kluczem jest symbol aktywa lub unikalny identyfikator dla aktywów niełączonych.
        /// </summary>
        [XmlIgnore]
        public Dictionary<string, LiveAssetSummary> PortfolioSummaries { get; } = [];

        public InvestmentPortfolio() { }

        /// <summary>
        /// Dodaje nowe aktywo do portfela i rejestruje subskrypcję zdarzeń zmiany ceny.
        /// </summary>
        /// <param name="asset">Obiekt aktywa do dodania.</param>
        public void AddNewAsset(Asset asset)
        {
            ArgumentNullException.ThrowIfNull(asset);

            Assets.Add(asset);
            asset.PropertyChanged += OnAssetPropertyChanged;
            UpdateSummary(asset);
        }

        /// <summary>
        /// Usuwa aktywo z portfela i wyrejestrowuje subskrypcję zdarzeń.
        /// </summary>
        /// <param name="asset">Obiekt aktywa do usunięcia.</param>
        /// <returns>True, jeśli usunięcie powiodło się.</returns>
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

        /// <summary>
        /// Obsługuje aktualizację podsumowań, gdy zmieni się cena lub ilość dowolnego aktywa w portfelu.
        /// </summary>
        private void OnAssetPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is Asset asset && (e.PropertyName == nameof(Asset.CurrentPrice) || e.PropertyName == nameof(Asset.Quantity)))
            {
                UpdateSummary(asset);
            }
        }

        /// <summary>
        /// Przelicza dane w <see cref="PortfolioSummaries"/> dla konkretnego aktywa.
        /// Jeśli aktywo jest oznaczane jako <see cref="Asset.IsMergeable"/>, grupuje je z innymi o tym samym symbolu.
        /// </summary>
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

        /// <summary>Usuwa aktywo na podstawie jego unikalnego identyfikatora <see cref="Guid"/>.</summary> 
        public bool RemoveAsset(Guid id)
        {
            Asset? toRemove = Assets.FirstOrDefault(x => x.Asset_id == id);
            if (toRemove != null)
            {
                return Assets.Remove(toRemove);
            }
            return false;
        }

        /// <summary>Oblicza całkowitą bieżącą wartość rynkową wszystkich aktywów w portfelu.</summary>
        public double CalculateSum() => Assets.Sum(x => x.Value);

        /// <summary>
        /// Przeprowadza symulację rynkową dla wszystkich aktywów w portfelu.
        /// Grupuje aktywa według symbolu, aby zapewnić tę samą nową cenę dla takich samych instrumentów.
        /// </summary>
        /// <param name="simulationDate">Data wirtualna, dla której generowane są ceny.</param>
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

        /// <summary>
        /// Oblicza procentową alokację portfela w podziale na typy aktywów (np. Akcje: 40%, Crypto: 60%).
        /// </summary>
        /// <returns>Słownik, gdzie kluczem jest nazwa typu, a wartością udział (0.0 - 1.0).</returns>
        public Dictionary<string, double> GetAssetAllocation()
        {
            double total = CalculateSum();
            if (total == 0) return [];

            return Assets
                .GroupBy(a => a.GetType().Name)
                .ToDictionary(g => g.Key, g => g.Sum(a => a.Value) / total);
        }

        /// <summary>Zwraca listę aktywów, które odnotowały największy procentowy wzrost od ceny zakupu.</summary>
        public IEnumerable<Asset> GetTopMovers(int count)
        {
            return Assets.OrderByDescending(a => (a.CurrentPrice - a.PurchasePrice) / a.PurchasePrice).Take(count);
        }

        /// <inheritdoc cref="Clone(string)"/>
        public object Clone()
        {
            return this.Clone(this.Name + " - Clone");
        }

        /// <summary>Tworzy głęboką kopię portfela wraz ze wszystkimi aktywami i nadaje mu nową nazwę.</summary>
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

        /// <summary>Oblicza całkowity zysk lub stratę portfela (suma zysków ze wszystkich aktywów).</summary>
        public double CalculateTotalProfit()
        {
            if (Assets == null || !Assets.Any()) return 0.0;

            return Assets.Sum(asset => (asset.CurrentPrice - asset.PurchasePrice) * asset.Quantity);
        }

        /// <summary>Wyszukuje aktywa spełniające określone kryterium.</summary>
        /// <param name="predicate">Funkcja filtrująca.</param>
        public IEnumerable<Asset> FindAssets(Func<Asset, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            return Assets.Where(predicate);
        }

    }
}
