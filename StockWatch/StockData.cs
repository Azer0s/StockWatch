using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using LiveCharts;
using Newtonsoft.Json;

namespace StockWatch
{
    class StockData
    {
        private readonly string _stockName;
        private readonly Stock _controlller;
        private readonly string _url;
        private readonly SynchronizationContext _context;

        public StockData(string stock, Stock controller, SynchronizationContext context)
        {
            _stockName = stock;
            _controlller = controller;
            _url = "https://www.google.com/finance/info?q=" + _controlller.StockTitle;
            _context = context;
        }

        public void Run()
        {
            while (!_controlller.ShouldStop)
            {
                try
                {
                    Thread.Sleep(1000);

                    using (var client = new HttpClient())
                    {
                        var result = client.GetAsync(_url);
                        var resultAsString = result.Result.Content.ReadAsStringAsync().Result.TrimStart('/').Replace("[", "").Replace("]", "");
                        var resAsDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(resultAsString);
                        var value = double.Parse(resAsDict["l"].Replace(".", ","));

                        var time = resAsDict["lt_dts"];
                        Application.Current.Dispatcher.BeginInvoke(new Action(() => _controlller.CControl.Title = resAsDict["c"] + "%"));

                        //TODO Implement time

                        _controlller.ChartValues.Add(value);

                        if (_controlller.ChartValues.Count > 10)
                        {
                            _controlller.ChartValues.RemoveAt(0);
                        }
                    }
                }
                catch (Exception)
                {
                    _controlller.ChartValues.Clear();
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => _controlller.YAxis.Title = "Something went wrong!"));
                }
            }
        }
    }
}
