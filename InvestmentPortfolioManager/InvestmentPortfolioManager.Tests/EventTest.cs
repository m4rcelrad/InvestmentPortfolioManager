using Microsoft.VisualStudio.TestTools.UnitTesting;
using InvestmentPortfolioManager.Core.Models;

namespace InvestmentPortfolioManager.Tests
{
    /// <summary>
    /// Testy weryfikujące poprawność działania mechanizmu zdarzeń (Events) i delegatów.
    /// </summary>
    [TestClass]
    public class EventTests
    {
        /// <summary>
        /// Sprawdza, czy zdarzenie <see cref="Asset.OnCriticalDrop"/> jest wyzwalane,
        /// gdy cena spadnie poniżej ustalonego progu bezpieczeństwa.
        /// </summary>
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