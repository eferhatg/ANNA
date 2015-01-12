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
using Encog.App.Analyst.CSV.Normalize;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.Neural.Networks;
using Encog.Persist;
using Encog.Util.Arrayutil;
using Encog.Util.CSV;
using Encog.Util.Simple;

namespace ANNA.Wins
{
    /// <summary>
    /// Interaction logic for Analyze.xaml
    /// </summary>
    public partial class Analyze : Window
    {
        private List<Delimeter> DelimeterList;
        private bool _hasHeader;
        private EvalViewModel evalViewModel;
        private IMLDataSet evalSet;
        private int _epoch;

        public void InitWindow()
        {

            DelimeterList = new List<Delimeter>()
            {
                new Delimeter(){Char = ",",Name = "Virgül"},
                new Delimeter(){Char = "\t",Name = "Tab"},
                new Delimeter(){Char = ";",Name = "Noktalı Virgül"},
                  new Delimeter(){Char = " ",Name = "Boşluk"}
            };
            ComboBoxDelimeter.SelectedValuePath = "Char";
            ComboBoxDelimeter.DisplayMemberPath = "Name";
            ComboBoxDelimeter.ItemsSource = DelimeterList;
            ComboBoxDelimeter.SelectedIndex = 0;
        }

        private NetworkConfiguration _nc;
        private bool _plotstart;
        private BasicNetwork network;
        private EncogAnalyst analyst;

        public Analyze(NetworkConfiguration nc)
        {
            _nc = nc;
            InitializeComponent();
            InitWindow();
        }

        private void ButtonLoadSourceFile_Click_1(object sender, RoutedEventArgs e)
        {
            if (ButtonSelectSourceFile.Tag == null)
            {
                MessageBox.Show("Lütfen önce bir kaynak dosya seçiniz");
                return;
            }
            var delimeter = ComboBoxDelimeter.SelectedValue.ToString();

            var fi = new FileInfo(ButtonSelectSourceFile.Tag.ToString());
            fi.CopyTo(Config.EveluateFile.FullName, true);
            Normalize();
        }

        private void ButtonSelectSourceFile_Click(object sender, RoutedEventArgs e)
        {
            FileInfo fi = Helper.OpenFileDialog();
            if (fi == null) return;
            ButtonSelectSourceFile.Content = fi.Name;
            ButtonSelectSourceFile.Tag = fi.FullName;
            ButtonSelectSourceFile.FontWeight = FontWeights.Bold;
            ButtonSelectSourceFile.Foreground = Brushes.Blue;

        }

        private void Normalize()
        {
            var analyst = new EncogAnalyst();
            analyst.Load(new FileInfo(_nc.AnalystFilePath));
            var norm = new AnalystNormalizeCSV { ProduceOutputHeaders = false };
            norm.Analyze(Config.EveluateFile, _nc.HasHeaders, CSVFormat.English, analyst);
            norm.Normalize(Config.NormalizedEveluateFile);
            Config.NormalizedEveluateFile.CopyTo(_nc.EvalNormPath, true);
        }

        private void ButtonAnalyze_Click(object sender, RoutedEventArgs e)
        {

            AnalyzeData();
        }

        private void AnalyzeData()
        {
            network = (BasicNetwork)EncogDirectoryPersistence.LoadObject(new FileInfo(_nc.TrainedNetworkFilePath));
            analyst = new EncogAnalyst();
            analyst.Load(Config.AnalystFile);
            evalSet = EncogUtility.LoadCSV2Memory(_nc.EvalNormPath, network.InputCount, network.OutputCount, false, CSVFormat.English, false);
            _epoch = 0;
            ListValues.Items.Clear();

            CompositionTarget.Rendering += CompositionTargetRendering;
            evalViewModel = new EvalViewModel();
            Plot1.DataContext = evalViewModel;
            _plotstart = true;
            Plot1.InvalidatePlot(true);
        }

        private void CompositionTargetRendering(object sender, EventArgs e)
        {
            if (_plotstart)
            {
                if (evalSet.Count == _epoch)
                {
                    _plotstart = false;
                    CompositionTarget.Rendering -= CompositionTargetRendering;
                    return;
                }

                using (var file = new StreamWriter(Config.ValidationResults.ToString()))
                {

                    var item = evalSet[_epoch];

                    double actualOutPut;
                    double idealOutput;
                    var normalizedActualOutput = (BasicMLData)network.Compute(item.Input);
                    var normActions = _nc.NormalizationActions.Where(d => d != NormalizationAction.Ignore);
                    var OutputNormalizationAction = normActions.ToArray()[normActions.Count() - 1];


                    if (OutputNormalizationAction == NormalizationAction.PassThrough)
                    {

                        actualOutPut = normalizedActualOutput.Data[0];
                        idealOutput = item.Ideal[0];
                    }
                    else if (OutputNormalizationAction == NormalizationAction.Equilateral)
                    {
                        int classCount = analyst.Script.Normalize.NormalizedFields[normActions.Count() - 1].Classes.Count;
                        double normalizationHigh = analyst.Script.Normalize.NormalizedFields[normActions.Count() - 1].NormalizedHigh;
                        double normalizationLow = analyst.Script.Normalize.NormalizedFields[normActions.Count() - 1].NormalizedLow;

                        var eq = new Encog.MathUtil.Equilateral(classCount, normalizationHigh, normalizationLow);
                        actualOutPut = eq.Decode(normalizedActualOutput);
                        idealOutput = item.Ideal[0];
                    }
                    else
                    {
                        actualOutPut = analyst.Script.Normalize.NormalizedFields[network.InputCount].DeNormalize(normalizedActualOutput.Data[0]);
                        idealOutput = analyst.Script.Normalize.NormalizedFields[network.InputCount].DeNormalize(item.Ideal[0]);

                    }




                    double[] input = new double[normActions.Count() - 1];

                    for (int i = 0; i <= normActions.Count() - 2; i++)
                    {

                        if (normActions.ToArray()[i] == NormalizationAction.Normalize)
                        {
                            input[i] = analyst.Script.Normalize.NormalizedFields[network.InputCount].DeNormalize(item.Input[i]);

                        }
                        else
                        {
                            input[i] = item.Input[i];

                        }
                    }
                    ListBoxItem lbi = new ListBoxItem();
                    lbi.Content = _epoch + ") Girdiler: [" + string.Join(",", input) + "] İdeal: " + idealOutput +
                                  " Gerçek: " + actualOutPut + " Hata: " + Math.Abs(idealOutput - actualOutPut);
                    ;
                    ListValues.Items.Add(lbi);

                    evalViewModel.DrawData(new EvalData()
                    {
                        EvalType = "ideal",
                        TypeIndex = 0,
                        Iteration = _epoch,
                        Value = idealOutput,
                        InputArray = input

                    });
                    evalViewModel.DrawData(new EvalData()
                    {
                        EvalType = "actual",
                        TypeIndex = 1,
                        Iteration = _epoch,
                        Value = actualOutPut,
                        InputArray = input
                    });
                    Plot1.InvalidatePlot(true);

                    _epoch++;


                }
            }
        }
    }
}
