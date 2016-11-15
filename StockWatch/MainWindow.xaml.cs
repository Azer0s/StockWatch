using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace StockWatch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<Configuration> _stockList = new List<Configuration>();

        public MainWindow()
        {
            InitializeComponent();

            string[] args = Environment.GetCommandLineArgs();

            try
            {
                if (args.Length > 0)
                {
                    var s = new Stock(args[1]);
                    WindowState = WindowState.Minimized;
                    s.Show();
                }
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                _stockList = JsonConvert.DeserializeObject<List<Configuration>>(File.ReadAllText(new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).DirectoryName + "\\stocks.json"));

                foreach (var variable in _stockList)
                {
                    stocks.Items.Add(variable.Name);
                }
            }
            catch (Exception)
            {
                // ignored
            }

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
                        _stockList.Add(new Configuration(stock.Text,stock.Text,"1d",0,false));
                        // ReSharper disable once AssignNullToNotNullAttribute
                        File.WriteAllText(new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).DirectoryName + "\\stocks.json", JsonConvert.SerializeObject(_stockList));
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

            foreach (var variable in _stockList)
            {
                if (variable.Name == stock.ToString())
                {
                    _stockList.Remove(variable);
                    File.WriteAllText(new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).DirectoryName + "\\stocks.json", JsonConvert.SerializeObject(_stockList));
                    break;
                }
            }
        }

        public void OpenStock(object sender, RoutedEventArgs e)
        {
            if (stocks.SelectedIndex == -1) return;

            var stock = stocks.Items[stocks.SelectedIndex].ToString();

            foreach (var variable in _stockList)
            {
                if (variable.Name != stock) continue;
                var s = new Stock(variable.Stock,variable.Time,variable.Precision,variable.Animations,false);
                s.Show();
                break;
            }
        }

        private void ChangeOptions(object sender, RoutedEventArgs e)
        {
            if (stocks.SelectedIndex == -1) return;

            foreach (var variable in _stockList)
            {
                if (variable.Name == stocks.SelectedItem.ToString())
                {
                    var options = new Options(variable.Name,variable.Stock,variable.Time,variable.Precision,variable.Animations);

                    if (options.ShowDialog() == true)
                    {
                        _stockList[_stockList.IndexOf(variable)].Name = options.Name;
                        _stockList[_stockList.IndexOf(variable)].Stock = options.Stock;
                        _stockList[_stockList.IndexOf(variable)].Time = options.Time;
                        _stockList[_stockList.IndexOf(variable)].Precision = options.Precision;
                        _stockList[_stockList.IndexOf(variable)].Animations = options.Animations;

                        // ReSharper disable once AssignNullToNotNullAttribute
                        File.WriteAllText(new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).DirectoryName + "\\stocks.json", JsonConvert.SerializeObject(_stockList));

                        stocks.Items.Clear();
                        foreach (var var in _stockList)
                        {
                            stocks.Items.Add(var.Name);
                        }

                        break;
                    }
                }
            }
        }

        private void GetSnaphot(object sender, RoutedEventArgs e)
        {
            if (stocks.SelectedIndex == -1) return;

            var saveFileDialog1 = new SaveFileDialog
            {
                Filter = "StockWatch Snapshot (*.swsnapshot)|*.swsnapshot|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (saveFileDialog1.ShowDialog() != true) return;
            var path = saveFileDialog1.FileName;
            foreach (var variable in _stockList)
            {
                if (variable.Name != stocks.SelectedItem.ToString()) continue;
                var s = new Stock(variable.Stock, variable.Time, variable.Precision,variable.Animations,true);
                Task.Run(() => s.SaveSnapshot(path));
                break;
            }
            
        }
    }
}
