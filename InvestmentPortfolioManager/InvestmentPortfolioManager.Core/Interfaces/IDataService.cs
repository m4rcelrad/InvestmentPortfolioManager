using InvestmentPortfolioManager.Core.Enums;
using InvestmentPortfolioManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestmentPortfolioManager.Core.Interfaces
{
    /// <summary>
    /// Interfejs serwisu odpowiedzialnego za zapis i odczyt danych portfela.
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// Zapisuje listę portfeli inwestycyjnych do trwałego magazynu danych.
        /// </summary>
        /// <param name="portfolios">Lista portfeli do zapisania.</param>
        void SavePortfolios(List<InvestmentPortfolio> portfolios);

        /// <summary>
        /// Wczytuje wszystkie zapisane portfele inwestycyjne.
        /// </summary>
        /// <returns>Lista wczytanych portfeli lub pusta lista, jeśli brak danych.</returns>
        List<InvestmentPortfolio> LoadAllPortfolios();

    }
}
