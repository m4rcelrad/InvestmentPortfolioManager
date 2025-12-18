using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolioManager.Core.Exceptions
{
    public class InvalidOwnerException(string message) : Exception(message) { }
}
