using Common.KinectUtils;
using OxyPlot;

namespace Server.Models
{
    public class Graphics
    {
        private AnglePlot ankle;
        public AnglePlot Ankle
        {
            get { return ankle; }
            set { ankle = value; }
        }

        private AnglePlot knee;
        public AnglePlot Knee
        {
            get { return knee; }
            set { knee = value; }
        }

        private AnglePlot hip;
        public AnglePlot Hip
        {
            get { return hip; }
            set { hip = value; }
        }

        private int Counter { get; set; } = 0;
        private int XSize = 45;

        public void Add(Gait gait)
        {
            if (Counter == XSize)
            {
                Clear();
                Counter = 0;
            }
            else
            {
                Counter++;
            }

            Ankle.AngularValues.Points.Add(new DataPoint(Counter, gait.CurrentAnkleFlexion));
            Knee.AngularValues.Points.Add(new DataPoint(Counter, gait.CurrentKneeFlexion));
            Hip.AngularValues.Points.Add(new DataPoint(Counter, gait.CurrentHipFlexion));

            Ankle.UpdateMinMax(gait.CurrentAnkleFlexion);
            Knee.UpdateMinMax(gait.CurrentKneeFlexion);
            Hip.UpdateMinMax(gait.CurrentHipFlexion);
        }

        public void Refresh()
        {
            Ankle.PlotModel.InvalidatePlot(true);
            Knee.PlotModel.InvalidatePlot(true);
            Hip.PlotModel.InvalidatePlot(true);
        }

        public void Clear()
        {
            Ankle.AngularValues.Points.Clear();
            Knee.AngularValues.Points.Clear();
            Hip.AngularValues.Points.Clear();
        }

        public Graphics()
        {
            Ankle = new AnglePlot(XSize, -40, 40, "Ankle Flexion Deg.");
            Knee = new AnglePlot(XSize, 0, 90, "Knee Flexion Deg.");
            Hip = new AnglePlot(XSize, 0, 70, "Hip Flexion Deg.");
        }
    }
}
