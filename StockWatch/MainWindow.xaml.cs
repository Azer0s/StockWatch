using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Runtime.Remoting.Contexts;
using System.Threading;
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
            string[] args = Environment.GetCommandLineArgs();

            try
            {
                if (args.Length > 0)
                {
                    if (args[1] == "-c")
                    {
                        ConsoleManager.Show();
                        ConsoleModeStart();
                        return;
                    }
                    else
                    {
                        var s = new Stock(args[1]);
                        WindowState = WindowState.Minimized;
                        s.Show();
                    }                    
                }
            }
            catch (Exception)
            {
                // ignored
            }

            InitializeComponent();

            try
            {
                if (!new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.StockWatch\\stocks.json").Exists)
                {
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.StockWatch");
                    File.Create(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.StockWatch\\stocks.json");
                }
                // ReSharper disable once AssignNullToNotNullAttribute
                _stockList = JsonConvert.DeserializeObject<List<Configuration>>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.StockWatch\\stocks.json"));

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
                        _stockList.Add(new Configuration(stock.Text, stock.Text, "1d", 0, false));
                        try
                        {
                            // ReSharper disable once AssignNullToNotNullAttribute
                            File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.StockWatch\\stocks.json", JsonConvert.SerializeObject(_stockList));
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        stock.Text = "Enter stock:";
                    }
                }
            };

            Closing += delegate
            {
                Environment.Exit(0);
            };
        }

        private void ConsoleModeStart()
        {
            var list = JsonConvert.DeserializeObject<List<Configuration>>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.StockWatch\\stocks.json"));
            this.Close();
            Console.Title = @"StockWatch Console Mode";

            while (true)
            {
                MainView:
                Console.Clear();
                Console.WriteLine(@"StockWatch - Console Mode");
                Console.WriteLine(@"-------------------------");
                Console.WriteLine(@"[0] Add new stock");

                var i = 1;
                foreach (var variable in list)
                {
                    Console.WriteLine($@"[{i}] {variable.Name}");
                    i++;
                }

                Console.WriteLine(@"[Q] Exit program");
                Console.WriteLine(@"-------------------------");

                var res = Console.ReadLine();

                if (res.ToLower() == "q")
                {
                    Environment.Exit(0);
                }

                if (res == "0")
                {
                    Console.WriteLine(@"-------------------------");
                    Console.WriteLine(@"Stock name:");
                    var name = Console.ReadLine();
                    Console.WriteLine();
                    Console.WriteLine(@"Stock ticker:");
                    var ticker = Console.ReadLine();

                    list.Add(new Configuration(name, ticker, "1d", 0, false));
                    try
                    {
                        File.WriteAllText(
                            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) +
                            "\\.StockWatch\\stocks.json", JsonConvert.SerializeObject(list));
                    }
                    catch
                    {
                        // ignored
                    }
                    Console.WriteLine(@"-------------------------");
                }

                int n;
                if (int.TryParse(res, out n))
                {
                    StockConsole:
                    try
                    {
                        Console.Clear();
                        Console.WriteLine();
                        Console.WriteLine(list[n-1].Name);
                        Console.WriteLine(@"-------------------------");
                        Console.WriteLine(@"[0] Open live-feed");
                        Console.WriteLine(@"[1] Settings");
                        Console.WriteLine(@"[B] Back");
                        Console.WriteLine(@"-------------------------");
                        var stockRes = Console.ReadKey();
                        Console.Write("\b");

                        //TODO

                        switch (stockRes.Key)
                        {
                            case ConsoleKey.B:
                                goto MainView;

                            case ConsoleKey.D0:
                            case ConsoleKey.NumPad0:
                                var s = new Stock(list[n-1].Name,"1d",0,false,false);
                                Console.WriteLine(@"Press q to quit");
                                Console.WriteLine("\n");

                                var shouldquit = false;
                                while (!shouldquit)
                                {
                                    try
                                    {
                                        if (Reader.ReadKey(500).Key == ConsoleKey.Q)
                                        {
                                            shouldquit = true;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        //ignored
                                    }

                                    var dataArray = s.DownloaddData().series;

                                    if (dataArray == null)
                                    {
                                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                                        ClearCurrentConsoleLine();
                                        Console.WriteLine(@"No data available!");
                                        Console.ReadKey();
                                        goto MainView;
                                    }

                                    var lastData = dataArray[dataArray.Count - 1];
                                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                                    ClearCurrentConsoleLine();
                                    Console.WriteLine(Stock.UnixTimeStampToDateTime(lastData.Timestamp) + @" | High: " + lastData.high + @" | Low: " + lastData.low);
                                }
                                continue;
                            case ConsoleKey.D1:
                            case ConsoleKey.NumPad1:
                                while (true)
                                {
                                    Console.Clear();
                                    Console.WriteLine(@"-------------------------");
                                    Console.WriteLine(@"[0] Name");
                                    Console.WriteLine(@"[1] Stock ticker");
                                    Console.WriteLine(@"[2] Time");
                                    Console.WriteLine(@"[B] Go back");
                                    Console.WriteLine(@"-------------------------");

                                    var result = Console.ReadKey();

                                    switch (result.Key)
                                    {
                                        case ConsoleKey.B:
                                            goto StockConsole;
                                        case ConsoleKey.NumPad0:
                                        case ConsoleKey.D0:
                                            Console.WriteLine();
                                            Console.WriteLine(@"Enter new name:");
                                            var newName = Console.ReadLine();
                                            list[list.IndexOf(list[n - 1])].Name = newName;
                                            File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.StockWatch\\stocks.json", JsonConvert.SerializeObject(_stockList));
                                            break;
                                        case ConsoleKey.NumPad1:
                                        case ConsoleKey.D1:
                                            Console.WriteLine();
                                            Console.WriteLine(@"Enter new ticker:");
                                            var newTicker = Console.ReadLine();
                                            list[list.IndexOf(list[n - 1])].Stock = newTicker;
                                            File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.StockWatch\\stocks.json", JsonConvert.SerializeObject(_stockList));
                                            break;
                                        case ConsoleKey.NumPad2:
                                        case ConsoleKey.D2:
                                            Console.WriteLine();
                                            Console.WriteLine(@"Enter new timespand:");
                                            var newTime = Console.ReadLine();
                                            list[list.IndexOf(list[n - 1])].Time = newTime;
                                            File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.StockWatch\\stocks.json", JsonConvert.SerializeObject(_stockList));
                                            break;
                                    }
                                }  
                                break;
                            default:
                                Console.WriteLine(@"Invalid input!");
                                Console.ReadKey();
                                Console.Clear();
                                goto StockConsole;
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine(@"Invalid input!");
                        Console.ReadKey();
                        Console.Clear();
                        goto StockConsole;
                    }
                }
                
            }
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
                    // ReSharper disable once AssignNullToNotNullAttribute
                    File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.StockWatch\\stocks.json", JsonConvert.SerializeObject(_stockList));
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
                try
                {
                    var s = new Stock(variable.Stock, variable.Time, variable.Precision, variable.Animations, false);
                    s.Show();
                }
                catch (Exception)
                {
                    //ignored
                }      
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
                        File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.StockWatch\\stocks.json", JsonConvert.SerializeObject(_stockList));

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

        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }
    }
}
