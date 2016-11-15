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
using System.Windows.Shapes;

namespace StockWatch
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        public string Name { get; set; }
        public string Stock { get; set; }
        public string Time { get; set; }
        public int Precision { get; set; }
        public bool Animations { get; set; }

        public Options(string name, string stock, string time, int precision,bool animations)
        {
            InitializeComponent();

            comboBox.Items.Add("1d");
            comboBox.Items.Add("5d");
            comboBox.Items.Add("1m");
            comboBox.Items.Add("3m");
            comboBox.Items.Add("1y");
            comboBox.Items.Add("5y");
            comboBox.Items.Add("max");

            comboBox1.Items.Add("0");
            comboBox1.Items.Add("1");
            comboBox1.Items.Add("2");
            comboBox1.Items.Add("3");
            comboBox1.Items.Add("4");

            checkBox.IsChecked = animations;

            textBox.Text = name;
            textBox1.Text = stock;
            comboBox.SelectedItem = time;
            comboBox1.SelectedItem = precision.ToString();

            button.Click += delegate
            {
                if(string.IsNullOrEmpty(textBox.Text) || string.IsNullOrEmpty(textBox1.Text))return;

                Name = textBox.Text;
                Stock = textBox1.Text;
                Time = comboBox.SelectedItem.ToString();
                Precision = int.Parse(comboBox1.SelectedItem.ToString());
                Animations = (bool) checkBox.IsChecked;

                DialogResult = true;
            };
        }
    }
}
