using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OxyPlot;

namespace StockWatch
{
    /// <summary>
    /// Interaction logic for Stock.xaml
    /// </summary>
    public partial class Stock : Window
    {
        public IList<DataPoint> Points { get; private set; }

        public Stock(string stock)
        {
            InitializeComponent();
            Title = "Stockwatch: " + stock;
        }
    }
}
