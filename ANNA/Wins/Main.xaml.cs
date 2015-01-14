using System;
using System.Collections.Generic;
using System.IO;
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
using Encog.App.Analyst;

namespace ANNA.Wins
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Window
    {
        public Main()
        {
            var testss = NetworkConfiguration.GetFromFile();

            InitializeComponent();
            ConfigDataGrid.ItemsSource = testss;
        }



        private void DuzenleLink_Click(object sender, RoutedEventArgs e)
        {
            var cf = new Learn(((Hyperlink)sender).DataContext as NetworkConfiguration);
            cf.Show();
        }

        private void SilLink_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult mbr = MessageBox.Show("Konfigürasyon silinecek, devam etmek istiyor musunuz?", "Dikkat", MessageBoxButton.OKCancel);
            if (mbr == MessageBoxResult.OK)
            {
                var networkConfiguration = ((Hyperlink)sender).DataContext as NetworkConfiguration;
                if (networkConfiguration != null && networkConfiguration.FilePath != null)
                {
                    File.Delete(networkConfiguration.FilePath);
                    File.Delete(networkConfiguration.AnalystFilePath);
                    File.Delete(networkConfiguration.EvalNormPath);
                    File.Delete(networkConfiguration.TrainedNetworkFilePath);
                    File.Delete(networkConfiguration.TrainedNormPath);
                    var testss = NetworkConfiguration.GetFromFile();
                    ConfigDataGrid.ItemsSource = null;
                    ConfigDataGrid.ItemsSource = testss;
                }
            }
        }

        private void KullanLink_Click(object sender, RoutedEventArgs e)
        {
            //var nc = ((Hyperlink)sender).DataContext as NetworkConfiguration;
            //FileInfo fi = new FileInfo(nc.AnalystFilePath);
            //string newline;
            //using (StreamReader sr = new StreamReader(nc.AnalystFilePath))
            //{
            //    String line = sr.ReadToEnd();
            //     newline = line.Replace("\r\nNCE]\r\n[BALANCE:CONFIG]", "\r\n[BALANCE]\r\n[BALANCE:CONFIG]")
            //                    .Replace("\r\nin]\r\ntrain", "\r\n[TASKS:task-train]\r\ntrain");
             
            //}

            //File.WriteAllText(nc.AnalystFilePath, newline);

            //var analyst = new EncogAnalyst();
            //analyst.Load(fi);
            var cf = new Analyze(((Hyperlink)sender).DataContext as NetworkConfiguration);

            cf.Show();
        }

        private void NewConfigButton_Click(object sender, RoutedEventArgs e)
        {
            var cf = new Learn();
            cf.Show();
        }

        private void Window_Activated(object sender, EventArgs e)
        {

        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var testss = NetworkConfiguration.GetFromFile();
            ConfigDataGrid.ItemsSource = null;
            ConfigDataGrid.ItemsSource = testss;
        }
    }
}
