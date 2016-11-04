using System;
using System.Collections.Generic;
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

namespace StockWatch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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

            market.GotFocus += delegate
            {
                if (market.Text == "Enter market:")
                {
                    market.Text = "";
                }
            };

            market.LostFocus += delegate
            {
                if (market.Text == "")
                {
                    market.Text = "Enter market:";
                }
            };

            addStock.Click += delegate
            {
                if (market.Text != "Enter market:" && market.Text != "" && stock.Text != "" && stock.Text != "Enter stock:")
                {
                    var stockText = market.Text + ":" + stock.Text;
                    if (!stocks.Items.Contains(stockText))
                    {
                        stocks.Items.Add(stockText);
                        market.Text = "Enter market:";
                        stock.Text = "Enter stock:";
                    }
                }
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
            Stock s = new Stock(stock.ToString());
            s.Show();
        }
    }
}
