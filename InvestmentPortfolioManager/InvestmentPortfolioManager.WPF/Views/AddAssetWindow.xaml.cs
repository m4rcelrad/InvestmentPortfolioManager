using InvestmentPortfolioManager.Core.Enums;
using InvestmentPortfolioManager.Core.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace InvestmentPortfolioManager.WPF.Views
{
    public partial class AddAssetWindow : Window
    {
        public Asset? CreatedAsset { get; private set; }

        public AddAssetWindow()
        {
            InitializeComponent();
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RealEstateFields == null || StandardAssetFields == null) return;

            string? type = (TypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (type == "RealEstate")
            {
                RealEstateFields.Visibility = Visibility.Visible;
                StandardAssetFields.Visibility = Visibility.Collapsed;
            }
            else
            {
                RealEstateFields.Visibility = Visibility.Collapsed;
                StandardAssetFields.Visibility = Visibility.Visible;
            }
        }

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
                            CreatedAsset = new Commodity(name, symbol, quantity, price, UnitEnum.Ounce);
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}