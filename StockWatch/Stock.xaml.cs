using System.Threading;
using System.Windows;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts.Base;

namespace StockWatch
{
    /// <summary>
    /// Interaction logic for Stock.xaml
    /// </summary>
    public partial class Stock : Window
    {
        public ChartValues<double> ChartValues;
        public Stock(string stock)
        {
            InitializeComponent();

            Title = stock.Split(':')[1] + "@" + stock.Split(':')[0];

            ChartValues = new ChartValues<double> {4, 6, 5, 2, 4};

            Chart.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Price",
                    Values = ChartValues
                }
            };

            var stockUpdater = new Thread(() =>
            {
                var s = new StockData(stock,this);
                s.Run();
            });
            stockUpdater.Start();
        }
    }
}
