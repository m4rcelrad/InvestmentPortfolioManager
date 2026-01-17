using Microsoft.VisualStudio.TestTools.UnitTesting;
using InvestmentPortfolioManager.Core.Models;
using InvestmentPortfolioManager.Core.Exceptions;
using System;

namespace InvestmentPortfolioManager.Tests
{
    [TestClass]
    public class AssetTests
    {
        private InvestmentPortfolio _portfolio;

        [TestInitialize]
        public void Setup()
        {
            _portfolio = new InvestmentPortfolio();
        }

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

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenQuantityIsNegative()
        {
            Assert.ThrowsException<InvalidQuantityException>(() => new Stock("Apple", "AAPL", -5, 100));
        }

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenNameIsEmpty()
        {
            Assert.ThrowsException<AssetNameException>(() => new Stock("", "AAPL", 10, 100));
        }

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
        /*[TestMethod]
        public void FindAssets_ShouldReturnOnlyMatchingAssets()
        {
            _portfolio.AddNewAsset(new Stock("Cheap", "C", 1, 10));
            _portfolio.AddNewAsset(new Stock("Expensive", "E", 1, 1000));

            var result = _portfolio.FindAssets(a => a.CurrentPrice > 500);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("E", result.First().AssetSymbol);
        }*/
    }
}