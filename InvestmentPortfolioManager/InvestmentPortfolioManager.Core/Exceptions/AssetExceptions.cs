using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolioManager.Core.Exceptions
{
    public class InvalidQuantityException(string message) : Exception(message) { }

    public class InvalidPriceException(string message) : Exception(message) { }

    public class AssetNameException(string message) : Exception(message) { }

    public class AssetSymbolException(string message) : Exception(message) { }

    public class BondRateException(string message) : Exception(message) { }
}
