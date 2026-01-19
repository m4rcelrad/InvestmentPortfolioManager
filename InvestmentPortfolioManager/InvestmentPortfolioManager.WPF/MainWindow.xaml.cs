using System.Windows;

namespace InvestmentPortfolioManager.WPF
{
    /// <summary>
    /// Logika interakcji dla głównego okna aplikacji MainWindow.xaml.
    /// W architekturze MVVM klasa ta pełni rolę "widoku" (View) i odpowiada 
    /// wyłącznie za warstwę prezentacji oraz inicjalizację kontrolek interfejsu.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Inicjalizuje nową instancję klasy <see cref="MainWindow"/>.
        /// Metoda <see cref="InitializeComponent"/> ładuje definicję interfejsu z pliku XAML
        /// oraz łączy zdefiniowane w nim zdarzenia z metodami w kodzie.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Obsługuje zdarzenie zaznaczenia przycisku RadioButton.
        /// Obecnie metoda jest pusta, ponieważ nawigacja między widokami 
        /// jest realizowana przez bindowanie komend w <see cref="MVVM.MainViewModel"/>.
        /// </summary>
        /// <param name="sender">Obiekt, który wywołał zdarzenie.</param>
        /// <param name="e">Dane dotyczące zdarzenia zmiany stanu przycisku.</param>
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}