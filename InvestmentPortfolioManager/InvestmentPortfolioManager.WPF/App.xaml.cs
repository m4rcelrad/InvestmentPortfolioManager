using InvestmentPortfolioManager.Data;
using InvestmentPortfolioManager.WPF.MVVM;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System;

namespace InvestmentPortfolioManager.WPF
{
    /// <summary>
    /// Główna klasa aplikacji WPF. 
    /// Odpowiada za cykl życia programu, inicjalizację bazy danych oraz konfigurację głównego okna.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Metoda wywoływana podczas uruchamiania aplikacji.
        /// Realizuje sekwencję startową: migrację bazy danych, inicjalizację głównego modelu widoku 
        /// oraz powiązanie go z oknem głównym.
        /// </summary>
        /// <param name="e">Argumenty zdarzenia startowego.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                using (var context = new InvestmentPortfolioDbContext())
                {
                    context.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas tworzenia bazy danych: {ex.Message}\n\n" +
                                "Upewnij się, że masz zainstalowany SQL Server LocalDB " +
                                "(standardowy składnik Visual Studio).",
                                "Błąd Bazy Danych", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(); 
                return;
            }

            MainViewModel mainVM = new MainViewModel();
            MainWindow window = new MainWindow();
            window.DataContext = mainVM;
            window.Show();
        }
    }
}