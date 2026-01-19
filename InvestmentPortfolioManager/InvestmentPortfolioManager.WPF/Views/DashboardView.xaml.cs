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
    /// Klasa definiująca logikę interakcji dla widoku DashboardView.xaml.
    /// Pełni rolę panelu podsumowania (Dashboard), prezentując kluczowe wskaźniki efektywności 
    /// portfela, takie jak całkowita wartość, zagregowany zysk/strata oraz alokacja aktywów.
    /// </summary>
    /// <remarks>
    /// Jako <see cref="UserControl"/>, ten komponent jest dynamicznie wstrzykiwany do głównego okna aplikacji 
    /// poprzez mechanizm <see cref="ContentControl"/> i bindowanie właściwości CurrentView w MainViewModel.
    /// </remarks>
    public partial class DashboardView : UserControl
    {
        /// <summary>
        /// Inicjalizuje nową instancję klasy <see cref="DashboardView"/>.
        /// Powoduje załadowanie definicji interfejsu z pliku XAML, 
        /// w tym stylów wizualnych dla kart podsumowania i wyzwalaczy (Triggers) kolorów zysku/straty.
        /// </summary>  
        public DashboardView()
        {
            InitializeComponent();
        }
    }
}
