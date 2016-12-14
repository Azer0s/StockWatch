using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlServerCe;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LiveCharts;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace StockWatch
{
    /// <summary>
    /// Interaction logic for Stock.xaml
    /// </summary>
    public partial class Stock : Window, INotifyPropertyChanged
    {
        private int Precision { get; }
        private string Timespan { get; }
        private string StockName { get; }
        public ChartValues<double> Values1 { get; set; }
        public ChartValues<double> Values2 { get; set; }
        public List<string> Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }
        private bool IsSet { get; set; }
        private bool ShouldRun { get; set; }

        public Stock(string stock, string timeSpan, int prec,bool animations, bool snapshot)
        {
            InitializeComponent();

            Timespan = timeSpan;
            StockName = stock;
            Precision = prec;
            YFormatter = value => Math.Round(value,2) + "$";
            Labels = new List<string>();
            ShouldRun = true;

            this.Closing += delegate
            {
                ShouldRun = false;
            };

            Chart.DisableAnimations = !animations;

            if (snapshot)
            {
                return;
            }

            Task.Run(() =>
            {
                if (timeSpan.Contains("y") || timeSpan.Contains("m"))
                {
                    GetStockDataMonthYear();
                }
                if (timeSpan.Contains("d"))
                {
                    if (timeSpan == "1d")
                    {
                        while (ShouldRun)
                        {
                            GetStockDataDay();
                            Application.Current.Dispatcher.Invoke(() => {
                                Application.Current.MainWindow = this;
                                if (WindowState == WindowState.Maximized)
                                {
                                    var width = Application.Current.MainWindow.Width;
                                    WindowState = WindowState.Normal;
                                    Application.Current.MainWindow.Width = width-1;
                                    WindowState = WindowState.Maximized;
                                    Application.Current.MainWindow.Width = width;
                                }
                                else
                                {
                                    Application.Current.MainWindow.Height -= 1;
                                    Application.Current.MainWindow.Height += 1;
                                }
                            });
                            
                            Thread.Sleep(15000);
                        }
                    }
                    else
                    {
                        GetStockDataDay();
                    }
                }
            });
        }

        public Stock(string path)
        {
            InitializeComponent();
            ShowSnapshot(path);
        }

        public StockWatchMonthYear.Data DownloadmyData()
        {
            using (var client = new HttpClient())
            {
                var result = client.GetAsync($"http://chartapi.finance.yahoo.com/instrument/1.0/{StockName}/chartdata;type=quote;range={Timespan}/json");
                var resultAsString = result.Result.Content.ReadAsStringAsync().Result.Replace("finance_charts_json_callback( ", "");
                return JsonConvert.DeserializeObject<StockWatchMonthYear.Data>(resultAsString.Remove(resultAsString.Length - 1).Replace("Company-Name", "Company_Name").Replace("Exchange-Name", "Exchange_Name"));
            }
        }

        public StockWatchDay.Data DownloaddData()
        {
            using (var client = new HttpClient())
            {
                var result = client.GetAsync($"http://chartapi.finance.yahoo.com/instrument/1.0/{StockName}/chartdata;type=quote;range={Timespan}/json");
                var resultAsString = result.Result.Content.ReadAsStringAsync().Result.Replace("finance_charts_json_callback( ", "");
                return JsonConvert.DeserializeObject<StockWatchDay.Data>(resultAsString.Remove(resultAsString.Length - 1).Replace("Company-Name", "Company_Name").Replace("Exchange-Name", "Exchange_Name"));
            }
        }

        public void GetStockDataMonthYear()
        {
            try
            {                 
                var data = DownloadmyData();

                int j = 0;
                var labels = new List<string>();
                foreach (var variable in data.series)
                {
                    if (j == Precision)
                    {
                        labels.Add(TimeNumToDateTime(variable.Date).ToString("d"));
                        j = 0;
                    }
                    else
                    {
                        j++;
                    }
                }

                Application.Current.Dispatcher.Invoke(() => {
                    Title = data.meta.Company_Name + " " + TimeNumToDateTime(data.Date.min).ToString("g") + " - " + TimeNumToDateTime(data.Date.max).ToString("g");
                    Labels = labels;
                    DataContext = this;
                });

                Values1 = new ChartValues<double>();
                Values2 = new ChartValues<double>();

                int i = 0;
                foreach (var variable in data.series)
                {
                    if (i == Precision)
                    {
                        Values2.Add(variable.high);
                        Values1.Add(variable.low);
                        i = 0;
                    }
                    else
                    {
                        i++;
                    }
                }
            }
            catch (Exception)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Title = "There was an error!";
                });
            }
        }

        public void GetStockDataDay()
        {
            try
            {                
                var data = DownloaddData();

                if (!IsSet)
                {
                    if (data.meta.currency == "EUR")
                    {
                        Application.Current.Dispatcher.Invoke(() => {
                            YFormatter = value => Math.Round(value, 2) + "€";
                        });
                    }
                    IsSet = true;
                }

                int j = 0;
                Labels.Clear();
                foreach (var variable in data.series)
                {
                    if (j == Precision)
                    {
                        Labels.Add(UnixTimeStampToDateTime(variable.Timestamp).ToString("g"));
                        j = 0;
                    }
                    else
                    {
                        j++;
                    }
                }

                Application.Current.Dispatcher.Invoke(() => {
                    Title = data.meta.Company_Name + " " + UnixTimeStampToDateTime(data.Timestamp.min).ToString("g") + " - " + UnixTimeStampToDateTime(data.Timestamp.max).ToString("g");
                    DataContext = this;
                });

                Values1 = new ChartValues<double>();
                Values2 = new ChartValues<double>();

                int i = 0;
                foreach (var variable in data.series)
                {
                    if (i == Precision)
                    {
                        Values2.Add(variable.high);
                        Values1.Add(variable.low);
                        i = 0;
                    }
                    else
                    {
                        i++;
                    }
                }
#if DEBUG
                Console.WriteLine(UnixTimeStampToDateTime(data.series[data.series.Count - 1].Timestamp));
#endif
            }
            catch (Exception)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Title = "There was an error!";
                });
            }          
        }

        public static DateTime UnixTimeStampToDateTime(int unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static DateTime TimeNumToDateTime(int timeNum)
        {
            return DateTime.ParseExact(timeNum.ToString(),"yyyyMMdd",CultureInfo.InvariantCulture,DateTimeStyles.None);
        }
        public void SaveSnapshot(string path)
        {
            SaveData dataToSnapshot = null;
            if (Timespan.Contains("y") || Timespan.Contains("m"))
            {
                var myData = DownloadmyData();

                dataToSnapshot = new SaveData
                {
                    CompanyName = myData.meta.Company_Name,
                    Currency = myData.meta.currency,
                    Exchangename = myData.meta.Exchange_Name,
                    TickerName = myData.meta.ticker,
                    Uri = myData.meta.uri,
                    Title = myData.meta.Company_Name + " " + TimeNumToDateTime(myData.Date.min).ToString("d") + " - " + TimeNumToDateTime(myData.Date.max).ToString("d"),
                    Values = new Dictionary<DateTime, Value>()
                };

                foreach (var variable in myData.series)
                {
                    dataToSnapshot.Values.Add(TimeNumToDateTime(variable.Date),new Value(variable.high,variable.low,variable.close,variable.open,variable.volume));
                }
            }
            if (Timespan.Contains("d"))
            {
                var dData = DownloaddData();
                dataToSnapshot = new SaveData
                {
                    CompanyName = dData.meta.Company_Name,
                    Currency = dData.meta.currency,
                    Exchangename = dData.meta.Exchange_Name,
                    TickerName = dData.meta.ticker,
                    Uri = dData.meta.uri,
                    Title = dData.meta.Company_Name + " " + UnixTimeStampToDateTime(dData.Timestamp.min).ToString("t") + " - " + UnixTimeStampToDateTime(dData.Timestamp.max).ToString("t"),
                    Values = new Dictionary<DateTime, Value>()
                };

                foreach (var variable in dData.series)
                {
                    dataToSnapshot.Values.Add(UnixTimeStampToDateTime(variable.Timestamp), new Value(variable.high, variable.low, variable.close, variable.open, variable.volume));
                }
            }

            if (dataToSnapshot == null) return;
            {
                if (File.Exists(path))
                    File.Delete(path);

                var connStr = $"Data Source={path};Max Database Size=4091";
                var engine = new SqlCeEngine(connStr);
                engine.CreateDatabase();

                SqlCeConnection conn = null;

                try
                {
                    conn = new SqlCeConnection(connStr);
                    conn.Open();

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "CREATE TABLE [Values] ([Id] datetime NOT NULL, [High] float NOT NULL, [Low] float NOT NULL, [Close] float NOT NULL, [Open] float NOT NULL, [Volume] float NOT NULL);";
                    cmd.ExecuteNonQuery();

                    cmd = conn.CreateCommand();
                    cmd.CommandText = "ALTER TABLE [Values] ADD CONSTRAINT [PK_Values] PRIMARY KEY ([Id]);";
                    cmd.ExecuteNonQuery();

                    cmd = conn.CreateCommand();
                    cmd.CommandText = "CREATE TABLE[Meta]([CompanyName] NVARCHAR(255) NOT NULL,[Currency] NVARCHAR(255) NOT NULL,[Exchangename] NVARCHAR(255) NOT NULL,[TickerName] NVARCHAR(255) NOT NULL,[Uri] NVARCHAR(255) NOT NULL,[Title] NVARCHAR(500));";
                    cmd.ExecuteNonQuery();
                    
                    cmd = conn.CreateCommand();
                    cmd.CommandText = "ALTER TABLE [Meta] ADD CONSTRAINT [PK_Values] PRIMARY KEY ([CompanyName]);";
                    cmd.ExecuteNonQuery();

                    cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO [Meta]([CompanyName],[Currency],[Exchangename],[TickerName],[Uri],[Title])VALUES(@companyName,@currency,@exchangename,@tickerName,@uri,@title);";
                    cmd.Parameters.AddWithValue("@companyName", dataToSnapshot.CompanyName);
                    cmd.Parameters.AddWithValue("@currency", dataToSnapshot.Currency);
                    cmd.Parameters.AddWithValue("@exchangename", dataToSnapshot.Exchangename);
                    cmd.Parameters.AddWithValue("@tickerName", dataToSnapshot.TickerName);
                    cmd.Parameters.AddWithValue("@uri", dataToSnapshot.Uri);
                    cmd.Parameters.AddWithValue("@title", dataToSnapshot.Title);
                    cmd.ExecuteNonQuery();

                    foreach (var variable in dataToSnapshot.Values)
                    {
                        cmd = conn.CreateCommand();
                        cmd.CommandText = "INSERT INTO [Values]([Id],[High],[Low],[Close],[Open],[Volume])VALUES(@id,@high,@low,@close,@open,@volume);";
                        cmd.Parameters.AddWithValue("@id", variable.Key);
                        cmd.Parameters.AddWithValue("@high", variable.Value.High);
                        cmd.Parameters.AddWithValue("@low", variable.Value.Low);
                        cmd.Parameters.AddWithValue("@close", variable.Value.Close);
                        cmd.Parameters.AddWithValue("@open", variable.Value.Open);
                        cmd.Parameters.AddWithValue("@volume", variable.Value.Volume);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
                finally
                {
                    conn?.Close();
                }
            }
        }

        public void ShowSnapshot(string path)
        {
            YFormatter = value => value.ToString("C");

            SqlCeConnection conn = null;
            try
            {
                var connStr = $"Data Source={path};Max Database Size=4091";
                conn = new SqlCeConnection(connStr);

                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT* FROM[Meta];";
                conn.Open();
                var reader = cmd.ExecuteReader();

                if(reader.Read())
                {
                    Title = reader.GetString(5);
                }

                cmd.CommandText = "SELECT* FROM[Values];";
                reader = cmd.ExecuteReader();

                Labels = new List<string>();
                Values1 = new ChartValues<double>();
                Values2 = new ChartValues<double>();
                while (reader.Read())
                {
                    Labels.Add(reader.GetDateTime(0).ToString("g"));
                    Values2.Add(reader.GetDouble(1));
                    Values1.Add(reader.GetDouble(2));
                }
                DataContext = this;
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                conn?.Close();
            }              
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Snapshot_OnClick(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                var saveFileDialog1 = new SaveFileDialog
                {
                    Filter = "StockWatch Snapshot (*.swsnapshot)|*.swsnapshot|All files (*.*)|*.*",
                    FilterIndex = 1,
                    RestoreDirectory = true
                };

                if (saveFileDialog1.ShowDialog() != true) return;
                var path = saveFileDialog1.FileName;
                SaveSnapshot(path);
            });            
        }
    }
}
