using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace InvestmentPortfolioManager.WPF.MVVM
{
    /// <summary>
    /// Uniwersalna implementacja interfejsu <see cref="ICommand"/>.
    /// Pozwala na bindowanie akcji z widoku bezpośrednio do metod w ViewModelu za pomocą delegatów.
    /// </summary>
    public class RelayCommand(Action<object> execute, Predicate<object>? canExecute = null) : ICommand
    {
        /// <summary>
        /// Zdarzenie wywoływane, gdy zmieniają się warunki określające, czy polecenie może zostać wykonane.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter) => canExecute == null || canExecute(parameter!);

        public void Execute(object? parameter) => execute(parameter!);
    }
}
