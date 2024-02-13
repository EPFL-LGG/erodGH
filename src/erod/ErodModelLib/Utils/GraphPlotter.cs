using System;
using Plotly.NET;


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

        public static void HistogramAngles(string title, double[] theta, bool toDegrees = true)
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

            var chart = Chart2D.Chart.Histogram<int, int, int>(X: new int[] { 1, 2, 2, 2, 3, 4, 5, 5 },
                            MultiText: new int[] { 1, 2, 3, 4, 5, 6, 7 },
                            Name: "histogram");

            chart.WithTitle(title);
            chart.Show();
        } 
    }
}

