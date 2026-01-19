using InvestmentPortfolioManager.Core.Enums;
using InvestmentPortfolioManager.Core.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace InvestmentPortfolioManager.WPF.Views
{
    /// <summary>
    /// Logika interakcji dla okna dialogowego AddAssetWindow.xaml.
    /// Okno służy do zbierania danych od użytkownika i tworzenia nowych instancji 
    /// klas pochodnych typu <see cref="Asset"/>.
    /// </summary>
    public partial class AddAssetWindow : Window
    {
        /// <summary>
        /// Przechowuje nowo utworzony obiekt aktywa. 
        /// Właściwość ta jest odczytywana przez ViewModel po zamknięciu okna z wynikiem pozytywnym.
        /// </summary>
        public Asset? CreatedAsset { get; private set; }

        /// <summary>
        /// Inicjalizuje nową instancję okna <see cref="AddAssetWindow"/>.
        /// </summary>
        public AddAssetWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Obsługuje zmianę wybranego typu aktywa w rozwijanej liście.
        /// Dynamicznie przełącza widoczność paneli (<see cref="StandardAssetFields"/> vs <see cref="RealEstateFields"/>),
        /// dostosowując formularz do wymagań konkretnego typu danych (np. adres dla nieruchomości).
        /// </summary>
        /// <param name="sender">Źródło zdarzenia (ComboBox).</param>
        /// <param name="e">Dane zdarzenia zmiany wyboru.</param>
        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Sprawdzamy czy kontrolki są już załadowane
            if (RealEstateFields == null || StandardAssetFields == null || CommodityFields == null) return;

            string? type = (TypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (type == "RealEstate")
            {
                RealEstateFields.Visibility = Visibility.Visible;
                StandardAssetFields.Visibility = Visibility.Collapsed;
                CommodityFields.Visibility = Visibility.Collapsed;
            }
            else
            {
                RealEstateFields.Visibility = Visibility.Collapsed;
                StandardAssetFields.Visibility = Visibility.Visible;

                // Pokaż pole jednostki tylko jeśli wybrano Commodity
                CommodityFields.Visibility = (type == "Commodity") ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Główna logika zatwierdzania formularza. 
        /// Przeprowadza walidację danych wejściowych, parsuje wartości liczbowe 
        /// i tworzy odpowiedni obiekt klasy (Stock, Bond, Cryptocurrency, Commodity lub RealEstate).
        /// </summary>
        /// <remarks>
        /// W przypadku błędnych danych (puste pola, zły format liczb), metoda przechwytuje wyjątek 
        /// i wyświetla komunikat błędu użytkownikowi, nie zamykając okna.
        /// </remarks>
        /// <param name="sender">Przycisk "Add".</param>
        /// <param name="e">Argumenty zdarzenia.</param>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = NameBox.Text;
                if (string.IsNullOrWhiteSpace(name)) throw new Exception("Name is required.");

                if (!double.TryParse(PriceBox.Text, out double price))
                    throw new Exception("Invalid Price format.");

                string type = (TypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Stock";

                if (type == "RealEstate")
                {
                    string street = StreetBox.Text;
                    string houseNumber = HouseNumberBox.Text;
                    string flatNumber = FlatNumberBox.Text;
                    string city = CityBox.Text;
                    string zipCode = ZipCodeBox.Text;
                    string country = CountryBox.Text;

                    CreatedAsset = new RealEstate(name, price, street, houseNumber, city, zipCode, country, flatNumber);
                }
                else
                {
                    string symbol = SymbolBox.Text;
                    if (string.IsNullOrWhiteSpace(symbol)) throw new Exception("Symbol is required.");

                    if (!double.TryParse(QuantityBox.Text, out double quantity))
                        throw new Exception("Invalid Quantity format.");

                    switch (type)
                    {
                        case "Stock":
                            CreatedAsset = new Stock(name, symbol, quantity, price);
                            break;
                        case "Bond":
                            CreatedAsset = new Bond(name, symbol, quantity, price, 3.0);
                            break;
                        case "Cryptocurrency":
                            CreatedAsset = new Cryptocurrency(name, symbol, quantity, price);
                            break;
                        case "Commodity":
                            // Pobieramy tekst z wybranej pozycji w ComboBox
                            string unitString = (UnitComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Ounce";

                            // Zamieniamy tekst na odpowiednią wartość z Twojego Enuma
                            if (!Enum.TryParse(unitString, out UnitEnum selectedUnit))
                            {
                                selectedUnit = UnitEnum.Ounce; // Wartość domyślna
                            }

                            CreatedAsset = new Commodity(name, symbol, quantity, price, selectedUnit);
                            break;
                        default:
                            CreatedAsset = new Stock(name, symbol, quantity, price);
                            break;
                    }
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating asset:\n{ex.Message}", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Zamyka okno bez zapisywania zmian.
        /// Ustawia <see cref="Window.DialogResult"/> na false.
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}