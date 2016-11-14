using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWatch
{
    public class Configuration
    {
        public Configuration(string name, string stock, string time, int precision,bool animations)
        {
            Name = name;
            Stock = stock;
            Time = time;
            Precision = precision;
            Animations = animations;
        }

        public string Name { get; set; }
        public string Stock { get; set; }
        public string Time { get; set; }
        public int Precision { get; set; }
        public bool Animations { get; set; }
    }
}
