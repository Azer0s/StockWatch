using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using Microsoft.Win32;

namespace StockWatch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly int _precision;

        public MainWindow()
        {
            InitializeComponent();

            string[] args = Environment.GetCommandLineArgs();

            try
            {
                if (args.Length > 0)
                {
                    WindowState = WindowState.Minimized;
                    var s = new Stock(args[1]);
                    s.Show();
                }
            }
            catch (Exception)
            {
            }       

            _precision = int.Parse(ConfigurationManager.AppSettings["precision"]);

            stock.GotFocus += delegate
            {
                if (stock.Text == "Enter stock:")
                {
                    stock.Text = "";
                }
            };

            stock.LostFocus += delegate
            {
                if (stock.Text == "")
                {
                    stock.Text = "Enter stock:";
                }
            };

            addStock.Click += delegate
            {
                if (stock.Text != "" && stock.Text != "Enter stock:")
                {
                    if (!stocks.Items.Contains(stock.Text))
                    {
                        stocks.Items.Add(stock.Text);
                        stock.Text = "Enter stock:";
                    }
                }
            };

            Closing += delegate
            {
                Environment.Exit(0);
            };
        }

        public void DeleteStock(object sender, RoutedEventArgs e)
        {
            if (stocks.SelectedIndex == -1) return;

            var stock = stocks.Items[stocks.SelectedIndex];
            stocks.Items.Remove(stock);
        }

        public void OpenStock(object sender, RoutedEventArgs e)
        {
            if (stocks.SelectedIndex == -1) return;

            var stock = stocks.Items[stocks.SelectedIndex];
            var s = new Stock(stock.ToString(),"1y", _precision);
            s.Show();
        }

        private void ChangeOptions(object sender, RoutedEventArgs e)
        {

        }

        private void GetSnaphot(object sender, RoutedEventArgs e)
        {
            var saveFileDialog1 = new SaveFileDialog
            {
                Filter = "StockWatch Snapshot (*.swsnapshot)|*.swsnapshot|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (saveFileDialog1.ShowDialog() != true) return;
            var path = saveFileDialog1.FileName;
            var s = new Stock(stocks.SelectedItem.ToString(),"1y",_precision);
            Task.Run(()=>s.SaveSnapshot(path));
        }
    }
}
