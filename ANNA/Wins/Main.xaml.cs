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
                if (networkConfiguration != null)
                    File.Delete(networkConfiguration.FilePath);
            }
        }

        private void KullanLink_Click(object sender, RoutedEventArgs e)
        {
            var cf = new Analyze();
            cf.Show();
        }

        private void NewConfigButton_Click(object sender, RoutedEventArgs e)
        {
            var cf = new Learn();
            cf.Show();
        }
    }
}
