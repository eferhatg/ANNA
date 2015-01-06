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
    /// Interaction logic for Text.xaml
    /// </summary>
    public partial class Text : Window
    {
        public Text(string text)
        {
            InitializeComponent();
            TextBlockInfo.Text = text;
        }
        public Text(FileInfo textFile)
        {
            InitializeComponent();
            using (var reader = new StreamReader(textFile.FullName))
            {
                var text=reader.ReadToEnd();
                TextBlockInfo.Text = text;
            }
          
        }
    }
}
