﻿
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace StockAnalyzer.CandleStick
{
    public interface IAnalyzerMethods
    {
        string Name { get; }

        string Description { get; }

        Task<bool> Run(DataTable dtPrices, DataTable rootTable);
    }
}