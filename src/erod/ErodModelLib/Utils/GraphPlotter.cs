using System;
using System.Linq;
using Plotly.NET;
using Plotly.NET.LayoutObjects;
using Plotly.NET.TraceObjects;
using static ErodModelLib.Metrics.JointMetrics;

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

        public static void HistogramAngles(double[] angles, JointMetricTypes jType)
        {
            Defaults.DefaultTemplate = ChartTemplates.lightMirrored;
            int maxNumGroups = 20;
            double maxBnd = Math.PI;
            var xbins = Bins.init(0.0, maxBnd, maxBnd / maxNumGroups);
            var chart1 = Chart2D.Chart.Histogram<double, double, string>(
                X: angles, MarkerColor: Color.fromKeyword(ColorKeyword.CadetBlue),
                Opacity: 0.75, HistFunc: StyleParam.HistFunc.Avg, HistNorm: StyleParam.HistNorm.None, XBins: xbins
            );
            var chart2 = Chart2D.Chart.Column<double, string, string, string, string>(values: angles);

            LinearAxis xaxis = LinearAxis.init<double, double, double, string, double, double>(
                AutoRange: StyleParam.AutoRange.False,
                Title: Title.init(jType.ToString()),
                Range: StyleParam.Range.ofMinMax(0.0, maxBnd)
            );
            chart1.WithXAxis(xaxis);

            var minmax = new Tuple<IConvertible, IConvertible>(0.0, maxBnd);
            var domain = new Tuple<IConvertible, IConvertible>(0.0, maxBnd);
            chart1.WithXAxisStyle(Title.init("Angle"), MinMax: minmax, Domain: domain);
            chart1.WithTitle("CM (" + jType.ToString() +")");
            chart2.WithTitle("Joint (" + jType.ToString() + ")");
            chart1.Show();
            chart2.Show();
        }

        public static void HistogramAreas(double[] areas, string quadType)
        {
            Defaults.DefaultTemplate = ChartTemplates.lightMirrored;
            int maxNumGroups = 10;
            var chart1 = Chart2D.Chart.Histogram<double, double, string>(X: areas, MarkerColor: Color.fromKeyword(ColorKeyword.CadetBlue), Opacity: 0.75, HistFunc: StyleParam.HistFunc.Avg, HistNorm: StyleParam.HistNorm.None, NBinsX: maxNumGroups);
            var chart2 = Chart2D.Chart.Column<double, string, string, string, string>(values: areas);

            chart1.WithTitle("Quads " + quadType + " Distribution");
            chart2.WithTitle("Quads " + quadType);
            chart1.Show();
            chart2.Show();
        }

        public static void HistogramSegments(double[] data, string segmentType)
        {
            Defaults.DefaultTemplate = ChartTemplates.lightMirrored;
            int maxNumGroups = 10;
            var chart1 = Chart2D.Chart.Histogram<double, double, string>(X: data, MarkerColor: Color.fromKeyword(ColorKeyword.CadetBlue), Opacity: 0.75, HistFunc: StyleParam.HistFunc.Avg, HistNorm: StyleParam.HistNorm.None, NBinsX: maxNumGroups);
            var chart2 = Chart2D.Chart.Column<double, string, string, string, string>(values: data);

            chart1.WithTitle("Segments " + segmentType + " Distribution");
            chart2.WithTitle("Segments " + segmentType);
            chart1.Show();
            chart2.Show();
        }
    }
}

