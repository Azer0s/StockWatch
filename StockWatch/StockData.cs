using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LiveCharts;

namespace StockWatch
{
    class StockData
    {
        private readonly string _stockName;
        private readonly Stock _controlller;
        public StockData(string stock, Stock controller)
        {
            _stockName = stock;
            _controlller = controller;

            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(500);
                _controlller.ChartValues.Add(i);
            }
        }
        public void Run()
        {
        }
    }
}
