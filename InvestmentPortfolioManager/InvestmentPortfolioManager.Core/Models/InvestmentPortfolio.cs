using InvestmentPortfolioManager.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InvestmentPortfolioManager.Core.Models
{
    public class InvestmentPortfolio
    {
        private List<Asset> assets = [];
        string owner = string.Empty;

        public IEnumerable<Asset> Assets => assets;
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

        public void AddNewAsset(Asset asset)
        {
            assets.Add(asset);
        }

        public bool RemoveAsset(Asset asset)
        {
            return assets.Remove(asset);
        }

        void RemoveAsset(Guid id)
        {
            Asset? to_remove = assets.FirstOrDefault(x => x.Asset_id == id);

            if (to_remove != null)
            {
                assets.Remove(to_remove);
            }
        }

        public double CalculateSum()
        {
            return assets.Sum(x => x.CurrentPrice * x.Quantity);
        }

        public void UpdateMarketPrices(DateTime simulationDate)
        {
            foreach (var asset in assets)
            {
                asset.SimulatePriceChange(simulationDate);
            }
        }
    }
}
