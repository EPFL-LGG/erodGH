using System;
using System.Linq;
using Plotly.NET;
using Plotly.NET.LayoutObjects;
using Plotly.NET.TraceObjects;
namespace ErodModelLib.Utils
{
	public static class GraphPlotter
	{
        public static void PolarChart(string title, double[] r, double[] theta, bool toDegrees = true)
        {
            double[] thetaDeg = new double[theta.Length];
            if (toDegrees)
            {
                for (int i = 0; i < theta.Length; i++)
                {
                    thetaDeg[i] = theta[i] * 180 / Math.PI;
                }
            }
            else thetaDeg = theta;

            GenericChart.GenericChart chart = ChartPolar.Chart.SplinePolar<double, double, double>(r, thetaDeg);
            chart.WithTitle(title);
            chart.Show();
        }

        public static void HistogramAngles(double[] angleInRadians, bool toDegrees = true)
        {
            int numJoints = angleInRadians.Length;
            double[] angles = new double[numJoints];
            string[] jointIndexes = new string[numJoints];
            string txt = toDegrees ? "(in degrees)" : "(in radians)";
            for (int i = 0; i < numJoints; i++)
            {
                angles[i] = toDegrees ? (angleInRadians[i] * 180 / Math.PI) : angleInRadians[i];
                jointIndexes[i] = i.ToString();
            }

            Defaults.DefaultTemplate = ChartTemplates.lightMirrored;
            int maxNumGroups = 20;
            double maxBnd = toDegrees ? 180 : Math.PI;
            var xbins = Bins.init(0.0, maxBnd, maxBnd / maxNumGroups);
            var chart1 = Chart2D.Chart.Histogram<double, double, string>(
                X: angles, MultiText: jointIndexes, MarkerColor: Color.fromKeyword(ColorKeyword.CadetBlue),
                Opacity: 0.75, HistFunc: StyleParam.HistFunc.Avg, HistNorm: StyleParam.HistNorm.None, XBins: xbins
            );
            var chart2 = Chart2D.Chart.Column<double, string, string, string, string>(values: angles);

            LinearAxis xaxis = LinearAxis.init<double, double, double, string, double, double>(
                AutoRange: StyleParam.AutoRange.False,
                Title: Title.init("Angle"),
                Range: StyleParam.Range.ofMinMax(0.0, maxBnd)
            );
            chart1.WithXAxis(xaxis);

            var minmax = new Tuple<IConvertible, IConvertible>(0.0, maxBnd);
            var domain = new Tuple<IConvertible, IConvertible>(0.0, maxBnd);
            chart1.WithXAxisStyle(Title.init("Angle"), MinMax: minmax, Domain: domain);
            chart1.WithTitle("CM " + txt);
            chart2.WithTitle("Joint angles " + txt);
            chart1.Show();
            chart2.Show();
        }

        public static void HistogramAreas(double[] areas, bool useAspect=false)
        {
            int numJoints = areas.Length;
            string[] quadIndexes = new string[numJoints];
            for (int i = 0; i < numJoints; i++) quadIndexes[i] = i.ToString();

            Defaults.DefaultTemplate = ChartTemplates.lightMirrored;
            int maxNumGroups = 10;
            var chart1 = Chart2D.Chart.Histogram<double, double, string>(X: areas, MultiText: quadIndexes, MarkerColor: Color.fromKeyword(ColorKeyword.CadetBlue), Opacity: 0.75, HistFunc: StyleParam.HistFunc.Avg, HistNorm: StyleParam.HistNorm.None, NBinsX: maxNumGroups);
            var chart2 = Chart2D.Chart.Column<double, string, string, string, string>(values: areas);

            chart1.WithTitle(useAspect ? "Quad aspect-ration distribution" : "Quad area distribution");
            chart2.WithTitle(useAspect ? "Quad aspect-rations" : "Quad areas");
            chart1.Show();
            chart2.Show();
        }
    }
}

