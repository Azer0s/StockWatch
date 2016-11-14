using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWatch
{
    public class SaveData
    {
        public string CompanyName { get; set; }
        public string TickerName { get; set; }
        public string Exchangename { get; set; }
        public string Currency { get; set; }
        public string Uri { get; set; }
        public Dictionary<DateTime,Value> Values { get; set; }
        public string Title { get; set; }
    }

    public class Value
    {
        public Value(double high, double low, double close, double open, double volume)
        {
            High = high;
            Low = low;
            Close = close;
            Open = open;
            Volume = volume;
        }

        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Open { get; set; }
        public double Volume { get; set; }
    }


}
