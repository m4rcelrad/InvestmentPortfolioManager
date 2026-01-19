using InvestmentPortfolioManager.Core.Enums;
using InvestmentPortfolioManager.Core.Interfaces;
using InvestmentPortfolioManager.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Serialization;

namespace InvestmentPortfolioManager.WPF.MVVM
{
    /// <summary>
    /// Serwis odpowiedzialny za trwałość danych przy wykorzystaniu plików lokalnych.
    /// Wykorzystuje format XML do serializacji i deserializacji obiektów portfela.
    /// </summary>
    public class FileDataService : IDataService
    {
        private const string FilePath = "user_portfolios.xml";

        /// <summary>
        /// Serializuje listę portfeli do pliku XML.
        /// </summary>
        public void SavePortfolios(List<InvestmentPortfolio> portfolios)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<InvestmentPortfolio>));

                using (StreamWriter writer = new StreamWriter(FilePath))
                {
                    serializer.Serialize(writer, portfolios);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"XML save error: {ex.Message}");
            }
        }

        /// <summary>
        /// Wczytuje portfele z pliku XML. Jeśli plik nie istnieje, generuje dane testowe.
        /// </summary>
        /// <returns>Lista załadowanych portfeli.</returns>
        public List<InvestmentPortfolio> LoadAllPortfolios()
        {
            if (File.Exists(FilePath))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<InvestmentPortfolio>));
                    using (StreamReader reader = new StreamReader(FilePath))
                    {
                        var loadedData = serializer.Deserialize(reader) as List<InvestmentPortfolio>;
                        if (loadedData != null && loadedData.Count > 0)
                        {
                            return loadedData;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"XML read error (loading defaults): {ex.Message}");
                }
            }

            return GenerateMockData();
        }

        private List<InvestmentPortfolio> GenerateMockData()
        {
            var portfolios = new List<InvestmentPortfolio>();

            var p1 = new InvestmentPortfolio
            {
                Name = "Main",
                Owner = "Warren Buffet",
                InvestmentPortfolioId = Guid.NewGuid()
            };
            p1.AddNewAsset(new Stock("Apple Inc.", "AAPL", 10, 150.0));
            p1.AddNewAsset(new Cryptocurrency("Bitcoin", "BTC", 0.5, 30000.0));
            portfolios.Add(p1);

            var p2 = new InvestmentPortfolio
            {
                Name = "Retirement",
                Owner = "Bill Gates",
                InvestmentPortfolioId = Guid.NewGuid()
            };
            p2.AddNewAsset(new Commodity("Gold", "GOLD", 10, 2000.0, UnitEnum.Ounce));
            p2.AddNewAsset(new Stock("Apple Inc.", "AAPL", 10, 130.0));
            portfolios.Add(p2);

            SavePortfolios(portfolios);

            return portfolios;
        }

    }
}