using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolioManager.WPF.MVVM
{
    /// <summary>
    /// Klasa bazowa dla wszystkich modeli widoku (ViewModels).
    /// Implementuje interfejs <see cref="INotifyPropertyChanged"/>, umożliwiając 
    /// automatyczne odświeżanie powiązań (bindings) w interfejsie użytkownika.
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Wywołuje powiadomienie o zmianie właściwości.
        /// Wykorzystuje atrybut [CallerMemberName], aby automatycznie pobrać nazwę wywołującej właściwości.
        /// </summary>
        /// <param name="name">Nazwa zmienionej właściwości.</param>
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
