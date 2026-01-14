using Microsoft.VisualStudio.TestTools.UnitTesting;
using InvestmentPortfolioManager.Core;
using InvestmentPortfolioManager.Core.Models;
using System;

namespace InvestmentPortfolioManager.Tests
{
    [TestClass]
    public class SimulationTests
    {
        private InvestmentPortfolio _portfolio;

        [TestInitialize]
        public void Setup()
        {
            _portfolio = new InvestmentPortfolio();
        }

        [TestMethod]
        public void UpdateMarketPrices_Bond_ShouldIncreaseDeterministically()
        {
            double initialPrice = 100.0;
            var bond = new Bond("Gov Bond", "US", 1, initialPrice, 0.05);
            _portfolio.AddNewAsset(bond);

            _portfolio.UpdateMarketPrices(DateTime.Now.AddDays(1));

            Assert.IsTrue(bond.CurrentPrice > initialPrice, "Cena obligacji powinna wzrosnąć.");
            Assert.AreNotEqual(initialPrice, bond.CurrentPrice);
        }

        [TestMethod]
        public void UpdateMarketPrices_Stock_ShouldNotBeNaN_Or_Infinity()
        {
            var stock = new Stock("Tech Corp", "TCH", 1, 100.0);
            _portfolio.AddNewAsset(stock);
            for (int i = 0; i < 10; i++)
            {
                _portfolio.UpdateMarketPrices(DateTime.Now.AddDays(i));
            }
            Assert.IsFalse(double.IsNaN(stock.CurrentPrice), "Cena akcji nie może być NaN.");
            Assert.IsFalse(double.IsInfinity(stock.CurrentPrice), "Cena akcji nie może być nieskończonością.");
            Assert.IsTrue(stock.CurrentPrice >= 0, "Cena akcji nie może być ujemna.");
        }
    }
}