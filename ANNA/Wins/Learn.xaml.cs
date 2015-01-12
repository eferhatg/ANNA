using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Encog.App.Analyst;
using Encog.App.Analyst.CSV.Normalize;
using Encog.App.Analyst.CSV.Segregate;
using Encog.App.Analyst.CSV.Shuffle;
using Encog.App.Analyst.Script.Normalize;
using Encog.App.Analyst.Wizard;
using Encog.Engine.Network.Activation;
using Encog.ML.Data;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using Encog.Neural.Networks.Training.Lma;
using Encog.Neural.Networks.Training.Propagation;
using Encog.Neural.Networks.Training.Propagation.Back;
using Encog.Neural.Networks.Training.Propagation.Manhattan;
using Encog.Neural.Networks.Training.Propagation.Quick;
using Encog.Neural.Networks.Training.Propagation.Resilient;
using Encog.Neural.Networks.Training.Propagation.SCG;
using Encog.Persist;
using Encog.Util.Arrayutil;
using Encog.Util.CSV;
using Encog.Util.Normalize;
using Encog.Util.Normalize.Input;
using Encog.Util.Simple;
using OxyPlot;
using FontWeights = System.Windows.FontWeights;

namespace ANNA.Wins
{

    public partial class Learn : Window
    {
        private NetworkConfiguration _nc;
        private bool _justRendered = false;
        private bool _loadTag;
        private List<NormalizationParams> _nrmParams;
        public List<Delimeter> DelimeterList;
        private ErrorViewModel errorViewModel;
        public Learn()
        {

            _nc = new NetworkConfiguration();
            _nc.NetworkLayers = new List<NetworkLayer>();
            InitializeComponent();
            InitWindow();

        }
        public Learn(NetworkConfiguration nc)
        {
            _loadTag = true;
            _nc = nc;
            InitializeComponent();
            InitWindow();
            BindControls();

        }

        public void BindControls()
        {

            UpDownHiddenLayer.Value = _nc.NetworkLayers.Count;

            DataGridHiddenLayers.ItemsSource = null;
            DataGridHiddenLayers.ItemsSource = _nc.NetworkLayers;
            ComboboxLearningAlgorithm.SelectedValue = _nc.LearnConfig.Algorithm;
            if (_nc.NetworkType == "MLP")
            {
                ComboboxNetworkType.SelectedIndex = 0;
            }
            else
            {
                ComboboxNetworkType.SelectedIndex = 1;
            }

            UpDownLearningRate.Value = Convert.ToDecimal(_nc.LearnConfig.LearningRate);
            UpDownMaximumError.Value = Convert.ToDecimal(_nc.LearnConfig.MaximumError);
            UpDownIteration.Value = _nc.LearnConfig.IterationCount;
            UpDownMomentum.Value = Convert.ToDecimal(_nc.LearnConfig.Momentum);
            UpDownAvarageError.Value = Convert.ToDecimal(_nc.LearnConfig.AvarageError);
            CheckBoxLimitedWithAvarageError.IsChecked = _nc.LearnConfig.LimitedWithAvarageError;
            CheckBoxLimitedWithMaximumError.IsChecked = _nc.LearnConfig.LimitedWithMaximumError;

            _loadTag = false;
        }



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


        private void UpDownHiddenLayer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_loadTag)
            {
                return;
            }
            var newValue = Convert.ToInt32(e.NewValue);
            var oldValue = Convert.ToInt32(e.OldValue);
            if (newValue > oldValue)
            {
                for (int i = 0; i < (newValue - oldValue); i++)
                {
                    _nc.NetworkLayers.Add(new NetworkLayer()
                    {
                        ActivationFunction = "Sigmoid",
                        Name = "Katman" + (DataGridHiddenLayers == null ? 0 : DataGridHiddenLayers.Items.Count),
                        NeuronCount = 10
                    });
                }
            }
            else
            {
                if (_nc.NetworkLayers.Count == 0) return;

                var differ = oldValue - newValue;

                _nc.NetworkLayers.RemoveRange(_nc.NetworkLayers.Count - 1 - differ, differ);
            }
            if (DataGridHiddenLayers != null)
            {
                DataGridHiddenLayers.ItemsSource = null;
                DataGridHiddenLayers.ItemsSource = _nc.NetworkLayers;
            }
        }

        private void ComboboxNetworkType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (e.AddedItems.Count > 0)
            {
                var comboBoxItem = e.AddedItems[0] as ComboBoxItem;
                if (comboBoxItem == null || DataGridHiddenLayers == null || UpDownHiddenLayer == null)
                {
                    return;
                }
                string tag = comboBoxItem.Tag as string;
                if (tag == "RBF")
                {
                    _nc.NetworkLayers.Clear();
                    UpDownHiddenLayer.Value = 0;
                    UpDownHiddenLayer.IsEnabled = false;

                    DataGridHiddenLayers.ItemsSource = null;
                    _justRendered = true;
                }
                else
                {
                    UpDownHiddenLayer.IsEnabled = true;
                }
            }


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

        private bool _hasHeader;
        private string outputname;
        private void ButtonLoadSourceFile_Click(object sender, RoutedEventArgs e)
        {
            if (ButtonSelectSourceFile.Tag == null)
            {
                MessageBox.Show("Lütfen önce bir kaynak dosya seçiniz");
                return;
            }
            var delimeter = ComboBoxDelimeter.SelectedValue.ToString();
            _hasHeader = ComboBoxHasHeaders.SelectionBoxItem.ToString() == "Var";
            var filepath = ButtonSelectSourceFile.Tag.ToString();

            //Kaynak dosya uygulama klasörüne kopyalanıyor.

            var fi = new FileInfo(filepath);
            File.Delete(Config.BaseFile.FullName);
            File.Delete(Config.TrainingFile.FullName);
            fi.CopyTo(Config.BaseFile.FullName);
            fi.CopyTo(Config.TrainingFile.FullName);

            //İlk satıra bakılarak kolon isimleri bulunuyor Grid dolduruluyor.
            _nrmParams = new List<NormalizationParams>();
            using (var reader = new StreamReader(Config.TrainingFile.FullName))
            {
                var headerLine = reader.ReadLine();
                if (headerLine != null)
                {
                    var colnames = headerLine.Split(delimeter.ToCharArray());
                    _nrmParams.AddRange(colnames.Select((t, i) => new NormalizationParams
                    {
                        ColName = _hasHeader ? t : "Sütun" + (i + 1),
                        ColType = "Normalizasyon",
                        DataType = "Giriş"
                    }));
                }
                _nrmParams[_nrmParams.Count - 1].DataType = "Çıkış";
                outputname = _nrmParams[_nrmParams.Count - 1].ColName;
            }
            DataGridNormalization.ItemsSource = null;
            DataGridNormalization.ItemsSource = _nrmParams;

            //  Sütun başlığı varsa, siliniyor.

            var lines = File.ReadAllLines(Config.TrainingFile.FullName);
            File.WriteAllLines(Config.TrainingFile.FullName, lines);

            ButtonNormilizeFile.IsEnabled = true;

        }

        private void UpDownLearningPercentage_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (UpDownTestPercentage != null && Convert.ToInt32(e.NewValue) <= 100)
                UpDownTestPercentage.Value = (100 - Convert.ToInt32(e.NewValue));
        }

        private void UpDownTestPercentage_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            if (UpDownLearningPercentage != null && Convert.ToInt32(e.NewValue) <= 100)
                UpDownLearningPercentage.Value = (100 - Convert.ToInt32(e.NewValue));
        }

        private FileInfo _segregatefileInfo;
        private void ButtonSelectSegregateFile_Click(object sender, RoutedEventArgs e)
        {
            _segregatefileInfo = Helper.OpenFileDialog();
            if (_segregatefileInfo == null) return;
            ButtonSelectSegregateFile.Content = _segregatefileInfo.Name;
            ButtonSelectSegregateFile.Tag = _segregatefileInfo.FullName;
            ButtonSelectSegregateFile.FontWeight = FontWeights.Bold;
            ButtonSelectSegregateFile.Foreground = Brushes.Blue;
        }

        private void ButtonSegregateFile_Click(object sender, RoutedEventArgs e)
        {
            if (ButtonSelectSegregateFile.Tag == null)
            {
                MessageBox.Show("Lütfen önce bir kaynak dosya seçiniz");
                return;
            }
            SegregateData(_segregatefileInfo, ComboBoxHasHeaders.SelectedValue.ToString() == "Evet");
            MessageBox.Show("Belirttiğiniz dosya belirttiğiniz oranlarda ayrıştırıldı. Dosya ile aynı klasöre eğitim ve test dataları oluşturuldu.");
        }

        private bool _isReadyToLearn;

        private void ButtonNormilizeFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NormalizeData();
                Text t = new Text(Config.NormalizedTrainingFile);
                t.Show();
                ButtonOpenLearningTab.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region encogfuncs
        private static void ShuffleData()
        {
            var shuffle = new ShuffleCSV();
            shuffle.Analyze(Config.TrainingFile, false, CSVFormat.English);
            shuffle.ProduceOutputHeaders = false;
            shuffle.Process(Config.TrainingFile);
        }
        private void SegregateData(FileInfo baseFi, bool hasheaders)
        {
            var trainFile =
                new FileInfo(baseFi.DirectoryName + @"\" + Path.GetFileNameWithoutExtension(baseFi.FullName) + "_egitim" +
                             baseFi.Extension);
            var testFile =
               new FileInfo(baseFi.DirectoryName + @"\" + Path.GetFileNameWithoutExtension(baseFi.FullName) + "_test" +
                            baseFi.Extension);

            var segregate = new SegregateCSV();
            segregate.Targets.Add(new SegregateTargetPercent(trainFile, Convert.ToInt32(UpDownLearningPercentage.Value)));
            segregate.Targets.Add(new SegregateTargetPercent(testFile, Convert.ToInt32(UpDownTestPercentage.Value)));
            segregate.ProduceOutputHeaders = false;
            segregate.Analyze(baseFi, hasheaders, CSVFormat.English);
            segregate.Process();
        }

        private void NormalizeData()
        {
            var cikisnrm = _nrmParams.Where(d => d.DataType == "Çıkış");
            var normalizationParamses = cikisnrm as NormalizationParams[] ?? cikisnrm.ToArray();
            if (normalizationParamses.Count() != 1 || _nrmParams.IndexOf(normalizationParamses.First()) != _nrmParams.Count - 1)
            {
                MessageBox.Show("Son sütun 'Çıkış' tipinde olmalıdır.");
                return;
            }
            var shuffle = ComboBoxShuffle.SelectionBoxItem.ToString() == "Evet";
            if (shuffle)
            {
                ShuffleData();
            }

            //Normalizasyon yapılıyor.
            var analyst = new EncogAnalyst();
            var wizard = new AnalystWizard(analyst);
            if (_hasHeader)
            {
                wizard.TargetFieldName = outputname;
            }
          
            wizard.Wizard(Config.TrainingFile, _hasHeader, AnalystFileFormat.DecpntComma);
             _nc.NormalizationActions= new NormalizationAction[_nrmParams.Count];
            for (var i = 0; i < _nrmParams.Count; i++)
            {
                _nc.NormalizationActions[i] = NormalizationAction.Ignore;
                NormalizationAction na;
                if (_nrmParams[i].DataType != "Yoksay")
                {
                    switch (_nrmParams[i].ColType)
                    {

                        case "Olduğu Gibi":
                            na = NormalizationAction.PassThrough;
                            break;
                        case "Normalizasyon":
                            na = NormalizationAction.Normalize;
                            break;
                        case "Sınıflandırma":
                            na = NormalizationAction.Equilateral;
                            break;
                        default:
                            na = NormalizationAction.Ignore;
                            break;
                    }
                  
                        _nc.NormalizationActions[i] = na;
                    

                }
                else
                {
                    na = NormalizationAction.Ignore;

                }
                analyst.Script.Normalize.NormalizedFields[i].Action = na;
            }
            var norm = new AnalystNormalizeCSV { ProduceOutputHeaders = false };
            norm.Analyze(Config.TrainingFile, _hasHeader, CSVFormat.English, analyst);
            norm.Normalize(Config.NormalizedTrainingFile);
            analyst.Save(Config.AnalystFile);

        }
        private void CreateNetwork()
        {
            var network = new BasicNetwork();
            network.AddLayer(new BasicLayer(null, true, _nrmParams.Count(d => d.DataType.Equals("Giriş"))));

            foreach (var networkLayer in _nc.NetworkLayers)
            {
                switch (networkLayer.ActivationFunction)
                {

                    case "Bipolar":
                        network.AddLayer(new BasicLayer(new ActivationBiPolar(), true, networkLayer.NeuronCount));
                        break;
                    case "Competitive":
                        network.AddLayer(new BasicLayer(new ActivationCompetitive(), true, networkLayer.NeuronCount));
                        break;
                    case "Gaussian":
                        network.AddLayer(new BasicLayer(new ActivationGaussian(), true, networkLayer.NeuronCount));
                        break;
                    case "Linear":
                        network.AddLayer(new BasicLayer(new ActivationLinear(), true, networkLayer.NeuronCount));
                        break;
                    case "Logarithmic":
                        network.AddLayer(new BasicLayer(new ActivationLOG(), true, networkLayer.NeuronCount));
                        break;
                    case "Sinusodial":
                        network.AddLayer(new BasicLayer(new ActivationSIN(), true, networkLayer.NeuronCount));
                        break;
                    case "SoftMax":
                        network.AddLayer(new BasicLayer(new ActivationSoftMax(), true, networkLayer.NeuronCount));
                        break;
                    case "Hyperbolic Tangent":
                        network.AddLayer(new BasicLayer(new ActivationTANH(), true, networkLayer.NeuronCount));
                        break;
                    default:
                    case "Sigmoid":
                        network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, networkLayer.NeuronCount));
                        break;
                }

            }
            network.AddLayer(new BasicLayer(new ActivationLinear(), false, 1));
            network.Structure.FinalizeStructure();
            network.Reset();
            EncogDirectoryPersistence.SaveObject(Config.TrainedNetworkFile, (BasicNetwork)network);
        }

        private void SaveNetworkConfiguration()
        {

            _nc.LearnConfig = new LearnConfig();
            _nc.NetworkType = ((ComboBoxItem)ComboboxNetworkType.SelectedItem).Tag.ToString();

            _nc.LearnConfig = new LearnConfig()
            {
                Algorithm = ComboboxLearningAlgorithm.SelectedValue.ToString(),
                AvarageError = float.Parse(UpDownAvarageError.Value.ToString()),
                IterationCount = Convert.ToInt32(UpDownIteration.Value),
                LearningRate = float.Parse(UpDownLearningRate.Value.ToString()),
                MaximumError = float.Parse(UpDownMaximumError.Value.ToString()),
                Momentum = float.Parse(UpDownMomentum.Value.ToString()),
                LimitedWithAvarageError =
                    (CheckBoxLimitedWithAvarageError.IsChecked.HasValue && CheckBoxLimitedWithAvarageError.IsChecked.Value) &&
                    CheckBoxLimitedWithAvarageError.IsChecked.Value,
                LimitedWithMaximumError =
                    (CheckBoxLimitedWithMaximumError.IsChecked.HasValue && CheckBoxLimitedWithMaximumError.IsChecked.Value) &&
                    CheckBoxLimitedWithMaximumError.IsChecked.Value

            };
            _nc.NormalizationParams = _nrmParams;
            _nc.HasHeaders = _hasHeader;
            CreateNetwork();




        }

        private Propagation train;
        private BasicNetwork network;
        private void TrainNetwork()
        {
            network = (BasicNetwork)EncogDirectoryPersistence.LoadObject(Config.TrainedNetworkFile);
            IMLDataSet trainingSet = EncogUtility.LoadCSV2Memory(Config.NormalizedTrainingFile.ToString(), network.InputCount, network.OutputCount, false, CSVFormat.English, false);

            switch (_nc.LearnConfig.Algorithm)
            {
                case "Backpropagation":
                    train = new Backpropagation(network, trainingSet, _nc.LearnConfig.LearningRate, _nc.LearnConfig.Momentum);
                    break;
                case "Manhattan Update":
                    train = new ManhattanPropagation(network, trainingSet, _nc.LearnConfig.LearningRate);
                    break;
                case "Quickpropagation":
                    train = new QuickPropagation(network, trainingSet, _nc.LearnConfig.LearningRate);
                    break;
                case "Scaled Conjugate Gradient":
                    train = new ScaledConjugateGradient(network, trainingSet);
                    break;
                default:
                    train = new ResilientPropagation(network, trainingSet);
                    break;
            }

        }
        #endregion

        private void TabControlLearn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (e.Source is TabControl)
            {
                if (TabItemLearning.IsSelected && !_isReadyToLearn)
                {
                    MessageBox.Show("Lütfen önce ağ özelliklerini tamamlayınız.");
                    TabControlLearn.SelectedIndex = 0;
                }
            }
        }

        private void ButtonOpenLearningTab_Click(object sender, RoutedEventArgs e)
        {
            SaveNetworkConfiguration();
            _isReadyToLearn = true;
            TabControlLearn.SelectedIndex = 1;
        
        }
        private int _epoch = 0;
        private double _maxError = 0;
        private double _avgError = 0;
        private double _totalError = 0;
        private double _mst = 0;
        private double _mse = 0;
        bool _plotstart;



        private void StartLearning_Click(object sender, RoutedEventArgs e)
        {
            TrainNetwork();

            _epoch = 0;
            _maxError = 0;
            _avgError = 0;
            _totalError = 0;
            _mst = 0;
            _mse = 0;
            LstAvgError.Items.Clear();
            LstError.Items.Clear();
            LstMsError.Items.Clear();
            StackListLayer.Items.Clear();

            WriteNetworkDetails();


            CompositionTarget.Rendering += CompositionTargetRendering;
            errorViewModel = new ErrorViewModel();
            Plot1.DataContext = errorViewModel;

            _plotstart = true;
            Plot1.InvalidatePlot(true);

        }

        private void WriteNetworkDetails()
        {
            var layerCount = train.Network.Flat.LayerCounts.Count();
            for (int i = (layerCount - 1); i >= 0; i--)
            {
                var LayerName = "Gizli";
                if (layerCount == (i + 1))
                {
                    LayerName = "Giriş";
                }
                if (i == 0)
                {
                    LayerName = "Çıkış";
                }
                var neuronCounts = train.Network.Flat.LayerCounts[i];
                var activationFunc = train.Network.Flat.ActivationFunctions[i].ToString().Split('.')[train.Network.Flat.ActivationFunctions[i].ToString().Split('.').Count()-1];
                GroupBox gp = new GroupBox();
                gp.Header = LayerName;

                TextBlock tb = new TextBlock();
                tb.TextWrapping = TextWrapping.Wrap;
                tb.Width = 200;
                tb.Text += "Nöron Sayısı:" + neuronCounts + "\nFonksiyon:" + activationFunc;
                gp.Content = tb;

                ListBoxItem lbi= new ListBoxItem();
                lbi.Content = gp;
                StackListLayer.Items.Add(lbi);
  
          
            }







        }


        private void CompositionTargetRendering(object sender, EventArgs e)
        {
            if (_plotstart && train != null)
            {
                errorViewModel.DrawData(new ErrorData()
                {
                    ErrorType = "avg",
                    TypeIndex = 0,
                    Iteration = _epoch,
                    Value = _avgError
                });
                errorViewModel.DrawData(new ErrorData()
                {
                    ErrorType = "error",
                    TypeIndex = 1,
                    Iteration = _epoch,
                    Value = train.Error
                });
                errorViewModel.DrawData(new ErrorData()
                {
                    ErrorType = "MSE",
                    TypeIndex = 2,
                    Iteration = _epoch,
                    Value = _mse
                });
                Plot1.InvalidatePlot(true);
                train.Iteration();
                _epoch++;
                _mst += Math.Pow(train.Error, 2);
                _mse = _mst / _epoch;

                _maxError = train.Error;
                _totalError += train.Error;
                _avgError = _totalError / _epoch;
                LstError.Items.Add(_epoch.ToString() + ") " + train.Error);
                LstAvgError.Items.Add(_epoch.ToString() + ") " + _avgError);
                LstMsError.Items.Add(_epoch.ToString() + ") " + _mse);

                if (_nc.LearnConfig.LimitedWithMaximumError && _maxError < _nc.LearnConfig.MaximumError) FinishTrain(train);
                if (_nc.LearnConfig.LimitedWithAvarageError && _avgError < _nc.LearnConfig.AvarageError) FinishTrain(train);
                if (_epoch >= _nc.LearnConfig.IterationCount) FinishTrain(train);

            }


        }

        public void FinishTrain(Propagation trainProp)
        {

            CompositionTarget.Rendering -= CompositionTargetRendering;
            _plotstart = false;
            trainProp.FinishTraining();
            var dialog = new Prompt("Kayıt için bir isim giriniz:");
            if (dialog.ShowDialog() == true)
            {

                _nc.Name = dialog.ResponseText;
                EncogDirectoryPersistence.SaveObject(Config.TrainedNetworkFile, (BasicNetwork)network);
              
                _nc.WriteToFile();
            }

        }



    }
}
