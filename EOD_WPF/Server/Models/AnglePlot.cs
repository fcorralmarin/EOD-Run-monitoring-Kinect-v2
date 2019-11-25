using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    public class AnglePlot
    {
        private int max;
        public int Max
        {
            get { return max; }
            set
            {
                max = (value > max) ? value : max;
            }
        }
        private int min;
        public int Min
        {
            get { return min; }
            set
            {
                min = (value < min) ? value : min;
            }
        }

        public int ROM
        {
            get { return Max - Min; }
        }

        private PlotModel plotModel;
        public PlotModel PlotModel
        {
            get { return plotModel; }
            set { plotModel = value; }
        }

        private LineSeries angularValues;
        public LineSeries AngularValues
        {
            get { return angularValues; }
            set { angularValues = value; }
        }

        public void UpdateMinMax(double angle)
        {
            Min = Convert.ToInt32(Math.Ceiling(angle + 0.5));
            Max = Convert.ToInt32(Math.Ceiling(angle + 0.5));
        }

        public AnglePlot(int x, int ymin, int ymax, string tittle)
        {
            Max = -360;
            Min = 360;

            PlotModel = new PlotModel()
            {
                PlotType = PlotType.XY
            };

            plotModel.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Maximum = x,
                Key = "H"
            });

            plotModel.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Left,
                Minimum = ymin,
                Maximum = ymax,
                Key = "V"
            });

            AngularValues = new LineSeries();

            AngularValues.XAxisKey = "H";
            AngularValues.YAxisKey = "V";

            AngularValues.Color = OxyColor.FromArgb(180,0, 255, 0);

            PlotModel.Title = tittle;

            if (PlotModel.Series.Any())
            {
                PlotModel.Series.Clear();
            }
            PlotModel.Series.Add(AngularValues);
        }
    }
}
