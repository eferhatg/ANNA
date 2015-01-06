using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;

namespace ANNA
{
    class Helper
    {
        public static string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }
        public static readonly List<OxyColor> ColorSet = new List<OxyColor>
                                            {
                                                OxyColors.Green,
                                                OxyColors.IndianRed,
                                                OxyColors.Coral,
                                                OxyColors.Chartreuse,
                                                OxyColors.Azure
                                            };
        public static FileInfo OpenFileDialog()
        {
            FileInfo fi = null;
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = Environment.CurrentDirectory;
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                fi = new FileInfo(dlg.FileName);


            }
            return fi;
        }
    }
}
