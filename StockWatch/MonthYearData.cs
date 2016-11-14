using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWatchMonthYear
{
    public class Meta
    {
        public string uri { get; set; }
        public string ticker { get; set; }
        public string Company_Name { get; set; }
        public string Exchange_Name { get; set; }
        public string unit { get; set; }
        public string timestamp { get; set; }
        public string currency { get; set; }
    }

    public class Date
    {
        public int min { get; set; }
        public int max { get; set; }
    }

    public class Close
    {
        public double min { get; set; }
        public double max { get; set; }
    }

    public class High
    {
        public double min { get; set; }
        public double max { get; set; }
    }

    public class Low
    {
        public double min { get; set; }
        public double max { get; set; }
    }

    public class Open
    {
        public double min { get; set; }
        public double max { get; set; }
    }

    public class Volume
    {
        public int min { get; set; }
        public int max { get; set; }
    }

    public class Ranges
    {
        public Close close { get; set; }
        public High high { get; set; }
        public Low low { get; set; }
        public Open open { get; set; }
        public Volume volume { get; set; }
    }

    public class Series
    {
        public int Date { get; set; }
        public double close { get; set; }
        public double high { get; set; }
        public double low { get; set; }
        public double open { get; set; }
        public int volume { get; set; }
    }

    public class Data
    {
        public Meta meta { get; set; }
        public Date Date { get; set; }
        public List<int> labels { get; set; }
        public Ranges ranges { get; set; }
        public List<Series> series { get; set; }
    }
}
