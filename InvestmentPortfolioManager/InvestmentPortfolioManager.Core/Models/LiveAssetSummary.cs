using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolioManager.Core.Models
{
    /// <summary>
    /// Klasa pomocnicza reprezentująca zagregowane dane dla grupy takich samych aktywów.
    /// Używana głównie do powiązań (Data Binding) w interfejsie użytkownika.
    /// </summary>
    public class LiveAssetSummary
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private double totalValue;

        /// <summary>Symbol giełdowy (np. BTC, AAPL).</summary>
        public string AssetSymbol { get; set; } = string.Empty;

        /// <summary>Pełna nazwa aktywa.</summary>
        public string AssetName { get; set; } = string.Empty;

        /// <summary>Łączna ilość posiadanych jednostek.</summary>
        public double TotalQuantity { get; set; }

        /// <summary>Średnia ważona cena zakupu dla całej pozycji.</summary>
        public double AveragePurchasePrice { get; set; }

        /// <summary>Łączny koszt zakupu (Suma Quantity * PurchasePrice).</summary>
        public double TotalCost { get; set; }

        /// <summary>
        /// Łączna bieżąca wartość rynkowa. Zmiana tej wartości powiadamia UI o zmianie zysku.
        /// </summary>
        public double TotalValue
        {
            get => totalValue;
            set
            {
                totalValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalProfit));}
        }

        /// <summary>Bieżący zysk lub strata na całej pozycji (Wartość - Koszt).</summary>
        public double TotalProfit => TotalValue - TotalCost;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
