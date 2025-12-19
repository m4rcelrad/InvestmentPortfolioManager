using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace InvestmentPortfolioManager.WPF.MVVM
{
    public class RelayCommand(Action<object> execute, Predicate<object>? canExecute = null) : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter) => canExecute == null || canExecute(parameter!);

        public void Execute(object? parameter) => execute(parameter!);
    }
}
