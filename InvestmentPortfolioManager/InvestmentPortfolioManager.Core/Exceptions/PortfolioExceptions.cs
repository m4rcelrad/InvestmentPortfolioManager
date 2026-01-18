using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolioManager.Core.Exceptions
{
    /// <summary>
    /// Wyjątek rzucany, gdy dane (imie i nazwisko) właściciela portfela są niepoprawne.
    /// </summary>
    public class InvalidOwnerException(string message) : Exception(message) { }
}
