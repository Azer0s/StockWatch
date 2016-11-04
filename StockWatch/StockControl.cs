using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OxyPlot;

namespace StockWatch
{
    class StockControl
    {
        public StockControl()
        {
            this.Plot.Title = stock.Split(':')[1] + "@" + stock.Split(':')[0];

            Points = new List<DataPoint>()
            {
                new DataPoint(0, 4),
                new DataPoint(10, 13),
                new DataPoint(20, 15),
                new DataPoint(30, 16),
                new DataPoint(40, 12),
                new DataPoint(50, 12)
            };

            Thread stockUpdater = new Thread(() =>
            {
                StockData s = new StockData(stock);
                s.Run();
            });
            stockUpdater.Start();
        }
    }
}
