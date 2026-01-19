using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InvestmentPortfolioManager.WPF.Views
{
    /// <summary>
    /// Logika interakcji dla komponentu PortfolioView.xaml.
    /// Jest to główny widok zarządzania aktywami, umożliwiający przeglądanie, 
    /// filtrowanie oraz usuwanie składników portfela inwestycyjnego.
    /// </summary>
    /// <remarks>
    /// Widok ten ściśle współpracuje z <see cref="InvestmentPortfolioManager.WPF.MVVM.PortfolioViewModel"/>.
    /// Zawiera zaawansowany interfejs DataGrid z warunkowym formatowaniem kolorystycznym 
    /// dla zysków i strat oraz pola do konfiguracji progów alarmowych (Alert Price).
    /// </remarks>
    public partial class PortfolioView : UserControl
    {
        /// <summary>
        /// Inicjalizuje nową instancję klasy <see cref="PortfolioView"/>.
        /// Ładuje zasoby XAML, w tym style dla nagłówków kolumn DataGrid oraz 
        /// szablony komórek (CellTemplates) odpowiedzialne za dynamiczne kolorowanie zmian wartości.
        /// </summary>
        public PortfolioView()
        {
            InitializeComponent();
        }
    }
}
