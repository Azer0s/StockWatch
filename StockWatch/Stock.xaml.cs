using System.Collections.Generic;
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
        public string StockTitle;
        public LineSeries ChangeControl;
        public LineSeries CloseControl;
        public bool ShouldStop;
        public Stock(string stock)
        {
            InitializeComponent();

            StockTitle = stock;
            Title = stock.Split(':')[1] + "@" + stock.Split(':')[0];

            ChartValues = new ChartValues<double> {0,0,0,0,0,0,0,0,0};
            YAxis.Labels = new [] {""};

            ChangeControl = new LineSeries
            {
                Title = "",
                Values = ChartValues
            };

            CloseControl = new LineSeries()
            {
                Title = ""
            };

            Chart.Series = new SeriesCollection
            {
                ChangeControl,
                CloseControl
            };

            ShouldStop = false;
            var stockUpdater = new Thread(() =>
            {
                var s = new StockData(stock,this, SynchronizationContext.Current);
                s.Run();
            });
            stockUpdater.Start();

            this.Closing += delegate
            {
                ShouldStop = true;
            };
        }
    }
}
