using Microsoft.VisualStudio.TestTools.UnitTesting;
using InvestmentPortfolioManager.Core.Models;

namespace InvestmentPortfolioManager.Tests
{
    [TestClass]
    public class EventTests
    {
        [TestMethod]
        public void OnCriticalDrop_ShouldFire_WhenPriceDropsBelowThreshold()
        {
            var crypto = new Cryptocurrency("Bitcoin", "BTC", 1, 50000);
            crypto.LowPriceThreshold = 40000; 
            bool eventFired = false;

            crypto.OnCriticalDrop += (sym, price, msg) => { eventFired = true; };

            crypto.UpdatePrice(30000);

            Assert.IsTrue(eventFired, "Zdarzenie OnCriticalDrop nie zostało wywołane po spadku ceny!");
        }
    }
}