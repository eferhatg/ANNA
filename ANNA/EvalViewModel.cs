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
    public class EvalViewModel : INotifyPropertyChanged
    {
        private PlotModel _evalPlotModel;
        public PlotModel EvalPlotModel
        {
            get { return _evalPlotModel; }
            set { _evalPlotModel = value; OnPropertyChanged("EvalPlotModel"); }
        }

      

        public EvalViewModel()
        {
            EvalPlotModel = new PlotModel();
       
            SetUpModel();
        
        }

        private void SetUpModel()
        {


            EvalPlotModel.LegendTitle = "Çıktı";
            EvalPlotModel.LegendOrientation = LegendOrientation.Horizontal;
            EvalPlotModel.LegendPlacement = LegendPlacement.Outside;
            EvalPlotModel.LegendPosition = LegendPosition.TopRight;
            EvalPlotModel.LegendBackground = OxyColor.FromAColor(200, OxyColors.White);
            EvalPlotModel.LegendBorder = OxyColors.Black;
            var yAxis = new LinearAxis(AxisPosition.Left, 0) { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, Title = "Değer" };
            var xAxis = new LinearAxis(AxisPosition.Bottom, 0) { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, Title = "İterasyon" };
            EvalPlotModel.Axes.Add(yAxis);
            EvalPlotModel.Axes.Add(xAxis);

        }
        

        public void DrawData(EvalData evalData)
        {
            LineSeries lineSerie;
            if (EvalPlotModel.Series.Count <= evalData.TypeIndex)
            {
                Random r = new Random();

                Array mvalues = Enum.GetValues(typeof(MarkerType));
                MarkerType randomMarker = (MarkerType)mvalues.GetValue(r.Next(mvalues.Length));
                ;

                lineSerie = new LineSeries
                {
                    StrokeThickness = 2,
                    MarkerSize = 3,
                    MarkerStroke = Helper.ColorSet[r.Next(0, 5)],
                    MarkerType = randomMarker,
                    CanTrackerInterpolatePoints = false,
                    Title = string.Format("{0}", evalData.EvalType),
                    Smooth = false,
                };
                EvalPlotModel.Series.Add(lineSerie);
            }
            else
            {
                lineSerie = EvalPlotModel.Series[evalData.TypeIndex] as LineSeries;
            }
            lineSerie.Points.Add(new DataPoint(evalData.Iteration, evalData.Value));

        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class EvalData
    {
        public string EvalType { get; set; }
        public int TypeIndex { get; set; }
        public Double Value { get; set; }
        public int Iteration { get; set; }
        public double[] InputArray { get; set; }
    }
}
