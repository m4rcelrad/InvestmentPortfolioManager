using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolioManager.Core.Models
{
    public class LiveAssetSummary
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private double totalValue;

        public string AssetSymbol { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;

        public double TotalQuantity { get; set; }
        public double AveragePurchasePrice { get; set; }
        public double TotalCost { get; set; }

        public double TotalValue
        {
            get => totalValue;
            set
            {
                totalValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalProfit));}
        }

        public double TotalProfit => TotalValue - TotalCost;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
