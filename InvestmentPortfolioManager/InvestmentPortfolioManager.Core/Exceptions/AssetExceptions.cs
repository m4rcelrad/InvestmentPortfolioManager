using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolioManager.Core.Exceptions
{
    /// <summary>
    /// Wyjątek wyrzucany, gdy podana ilość aktywa jest nieprawidłowa (ujemna).
    /// </summary>
    public class InvalidQuantityException(string message) : Exception(message) { }

    /// <summary>
    /// Wyjątek wyrzucany, gdy cena aktywa jest nieprawidłowa (ujemna).
    /// </summary>
    public class InvalidPriceException(string message) : Exception(message) { }

    /// <summary>
    /// Wyjątek wyrzucany, gdy nazwa aktywa jest pusta.
    /// </summary>
    public class AssetNameException(string message) : Exception(message) { }

    /// <summary>
    /// Wyjątek wyrzucany, gdy symbol giełdowy (Ticker) jest pusty.
    /// </summary>
    public class AssetSymbolException(string message) : Exception(message) { }

    /// <summary>
    /// Wyjątek wyrzucany, gdy oprocentowanie obligacji jest ujemne.
    /// </summary>
    public class BondRateException(string message) : Exception(message) { }

    /// <summary>
    /// Wyjątek wyrzucany, gdy podany adres jest nieprawidłowy.
    /// </summary>
    public class InvalidAddressException(string message) : Exception(message) { }

    /// <summary>
    /// Wyjątek wyrzucany, gdy kod pocztowy ma nieprawidłowy format.
    /// </summary>
    public class InvalidZipCodeException(string message) : Exception(message) { }

    /// <summary>
    /// Wyjątek rzucany, gdy wybrana jednostka miary nie pasuje do określonych typów.
    /// </summary>
    public class InvalidUnitException(string message) : Exception(message) { }
}
