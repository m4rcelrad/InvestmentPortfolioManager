using InvestmentPortfolioManager.Core;
using InvestmentPortfolioManager.Core.Exceptions;
using InvestmentPortfolioManager.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace InvestmentPortfolioManager.Tests
{
    [TestClass]
    public class InvestmentPortfolioTests
    {
        private InvestmentPortfolio _portfolio;

        [TestInitialize]
        public void Setup()
        {
            _portfolio = new InvestmentPortfolio();
        }

        [TestMethod]
        public void AddNewAsset_ShouldIncreaseAssetCount()
        {
            var stock = new Stock("Tesla", "TSLA", 1, 200);

            _portfolio.AddNewAsset(stock);

            Assert.AreEqual(1, _portfolio.Assets.Count);
            Assert.IsTrue(_portfolio.Assets.Contains(stock));
        }

        [TestMethod]
        public void CalculateSum_ShouldReturnTotalValueOfAllAssets()
        {
            _portfolio.AddNewAsset(new Stock("A", "A", 2, 100));
            _portfolio.AddNewAsset(new Bond("B", "B", 10, 10, 0.05)); 

            var sum = _portfolio.CalculateSum();

            Assert.AreEqual(300.0, sum, 0.001);
        }

        [TestMethod]
        [DataRow("Jan Kowalski", true)]      
        [DataRow("jan kowalski", false)]     
        [DataRow("Jan", false)]              
        [DataRow("Jan123", false)]           
        public void Owner_ValidationRegex_ShouldWorkCorrectly(string ownerName, bool isValid)
        {
            if (isValid)
            {
                _portfolio.Owner = ownerName;
                Assert.AreEqual(ownerName, _portfolio.Owner);
            }
            else
            {
                Assert.ThrowsException<InvalidOwnerException>(() => _portfolio.Owner = ownerName);
            }
        }
    }
}