using Microsoft.VisualStudio.TestTools.UnitTesting;
using InvestmentPortfolioManager.Core.Models;
using InvestmentPortfolioManager.Core.Exceptions;
using System;

namespace InvestmentPortfolioManager.Tests
{
    /// <summary>
    /// Testy jednostkowe dla klas pochodnych klasy <see cref="Asset"/>.
    /// Weryfikują poprawność obliczeń wartości rynkowej oraz działanie walidatorów w konstruktorach.
    /// </summary>
    [TestClass]
    public class AssetTests
    {
        private InvestmentPortfolio _portfolio;

        /// <summary>Inicjalizacja środowiska testowego przed każdym testem.</summary>
        [TestInitialize]
        public void Setup()
        {
            _portfolio = new InvestmentPortfolio();
        }

        /// <summary>Sprawdza, czy właściwość Value poprawnie mnoży ilość przez aktualną cenę.</summary>
        [TestMethod]
        public void Value_ShouldCalculateCorrectly_WhenQuantityAndPriceAreSet()
        {
            double quantity = 10;
            double price = 150.0;
            var stock = new Stock("Apple", "AAPL", quantity, price);

            double expectedValue = 1500.0;
            double actualValue = stock.Value; 

            Assert.AreEqual(expectedValue, actualValue, 0.001, "Wartość aktywa (Value) jest niepoprawna.");
        }

        /// <summary>Weryfikuje, czy próba utworzenia aktywa z ujemną ilością wyrzuca dedykowany wyjątek.</summary>
        [TestMethod]
        public void Constructor_ShouldThrowException_WhenQuantityIsNegative()
        {
            Assert.ThrowsException<InvalidQuantityException>(() => new Stock("Apple", "AAPL", -5, 100));
        }

        /// <summary>Sprawdza, czy system blokuje tworzenie aktywów bez nazwy.</summary>
        [TestMethod]
        public void Constructor_ShouldThrowException_WhenNameIsEmpty()
        {
            Assert.ThrowsException<AssetNameException>(() => new Stock("", "AAPL", 10, 100));
        }

        /// <summary>
        /// Testuje logikę grupowania w <see cref="LiveAssetSummary"/>. 
        /// Dwa zakupy tej samej waluty powinny zostać złączone w jedną pozycję ze średnią ceną.
        /// </summary>
        [TestMethod]
        public void AddNewAsset_ShouldGroupSameAssetsInSummary()
        {
            var btc1 = new Cryptocurrency("Bitcoin", "BTC", 0.5, 40000);
            var btc2 = new Cryptocurrency("Bitcoin", "BTC", 0.5, 30000);

            _portfolio.AddNewAsset(btc1);
            _portfolio.AddNewAsset(btc2);

            Assert.IsTrue(_portfolio.PortfolioSummaries.ContainsKey("BTC"));

            var summary = _portfolio.PortfolioSummaries["BTC"];
            Assert.AreEqual(1.0, summary.TotalQuantity, "Ilość powinna się zsumować (0.5 + 0.5).");
            Assert.AreEqual(35000, summary.AveragePurchasePrice, 0.001, "Średnia cena zakupu źle policzona.");
        }

        /// <summary>
        /// Weryfikuje działanie metody <see cref="InvestmentPortfolio.FindAssets"/>.
        /// Test sprawdza, czy przekazany predykat (wyrażenie lambda) poprawnie filtruje kolekcję
        /// i zwraca tylko te aktywa, które spełniają określony warunek cenowy.
        /// </summary>
        [TestMethod]
        public void FindAssets_ShouldReturnOnlyMatchingAssets()
        {
            _portfolio.AddNewAsset(new Stock("Cheap", "C", 1, 10));
            _portfolio.AddNewAsset(new Stock("Expensive", "E", 1, 1000));

            var result = _portfolio.FindAssets(a => a.CurrentPrice > 500);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("E", result.First().AssetSymbol);
        }
    }
}