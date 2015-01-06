using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ANNA.Annotations;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Training.Propagation;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ANNA
{
    public class ErrorViewModel : INotifyPropertyChanged
    {
        private PlotModel _errorPlotModel;
        public PlotModel ErrorPlotModel
        {
            get { return _errorPlotModel; }
            set { _errorPlotModel = value; OnPropertyChanged("ErrorPlotModel"); }
        }

        private PlotModel _neuronModel;
        public PlotModel NeuronPlotModel
        {
            get { return _neuronModel; }
            set { _neuronModel = value; OnPropertyChanged("NeuronPlotModel"); }
        }

        public ErrorViewModel()
        {
            ErrorPlotModel = new PlotModel();
            NeuronPlotModel = new PlotModel();
            SetUpModel();
            SetUpNeuronModel();
        }

        private void SetUpModel()
        {
     

            ErrorPlotModel.LegendTitle = "Hata";
            ErrorPlotModel.LegendOrientation = LegendOrientation.Horizontal;
            ErrorPlotModel.LegendPlacement = LegendPlacement.Outside;
            ErrorPlotModel.LegendPosition = LegendPosition.TopRight;
            ErrorPlotModel.LegendBackground = OxyColor.FromAColor(200, OxyColors.White);
            ErrorPlotModel.LegendBorder = OxyColors.Black;
            var yAxis = new  LinearAxis(AxisPosition.Left, 0) { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, Title = "Değer" };
            var xAxis = new LinearAxis(AxisPosition.Bottom, 0) { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, Title = "İterasyon" };
            ErrorPlotModel.Axes.Add(yAxis);
            ErrorPlotModel.Axes.Add(xAxis);
    
        }
        private void SetUpNeuronModel()
        {


           
            NeuronPlotModel.LegendOrientation = LegendOrientation.Horizontal;
            NeuronPlotModel.LegendPlacement = LegendPlacement.Outside;
            NeuronPlotModel.LegendPosition = LegendPosition.TopRight;
            NeuronPlotModel.LegendBackground = OxyColor.FromAColor(200, OxyColors.White);
            NeuronPlotModel.LegendBorder = OxyColors.Black;
            var yAxis = new LinearAxis(AxisPosition.Left, 0) { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, Title = "Nöron Dizilimi" };
            var xAxis = new LinearAxis(AxisPosition.Bottom, 0) { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, Title = "" };
            NeuronPlotModel.Axes.Add(yAxis);
            NeuronPlotModel.Axes.Add(xAxis);

        }

        public void DrawData(ErrorData errorData)
        {
            LineSeries lineSerie;
            if (ErrorPlotModel.Series.Count <= errorData.TypeIndex)
            {
                Random r = new Random();

                Array mvalues = Enum.GetValues(typeof(MarkerType));
                MarkerType randomMarker = (MarkerType)mvalues.GetValue(r.Next(mvalues.Length));
;
                
                lineSerie = new LineSeries
                {
                    StrokeThickness = 2,
                    MarkerSize = 3,
                    MarkerStroke = Helper.ColorSet[r.Next(0,5)],
                    MarkerType =randomMarker,
                    CanTrackerInterpolatePoints = false,
                    Title = string.Format("Hata {0}", errorData.ErrorType),
                    Smooth = false,
                };
                ErrorPlotModel.Series.Add(lineSerie);
            }
            else
            {
                lineSerie = ErrorPlotModel.Series[errorData.TypeIndex] as LineSeries;
            }
            lineSerie.Points.Add(new DataPoint(errorData.Iteration, errorData.Value));
         
        }
        public void DrawNeurons(Propagation train)
        {
            if(train==null)return;
            ScatterSeries scatterSeries;
            if (NeuronPlotModel.Series.Count == 0)
            {
                scatterSeries = new ScatterSeries();
            }
            else
            {
                scatterSeries = NeuronPlotModel.Series[0] as ScatterSeries;
            }
            var counter = 0;
            for (int i =train.Network.Flat.LayerCounts.Length-1; i >=0 ; i--)
            {

                var multiplier = train.Network.Flat.LayerCounts[i]/2;
                for (int j = 0; j < train.Network.Flat.LayerCounts[i]; j++)
                {

                    scatterSeries.Points.Add(new ScatterPoint(counter, j - multiplier) { Value = 15 });
                }
                counter++;

            }
            NeuronPlotModel.Series.Add(scatterSeries);
        }
    
 

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class ErrorData
    {
        public string ErrorType { get; set; }
        public int TypeIndex { get; set; }
        public Double Value { get; set; }
        public int Iteration { get; set; }
    }
}
