using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using ErodDataLib.Types;
using ErodDataLib.Utils;
using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Types.Transforms;
using Plotly.NET;
using Plotly.NET.LayoutObjects;
using Plotly.NET.TraceObjects;
using Rhino.Render;
using Rhino.Runtime;
using static ErodModelLib.Metrics.JointMetrics;
using static Plotly.NET.StyleParam;

namespace ErodModelLib.Utils
{
    public class GraphPlotterOptions : IGH_Goo
    {
        public int Width { get; set; }
        public int Length { get; set; }
        public string Title { get; set; }

        public GraphPlotterOptions(int width = 800, int length = 800, string title = "")
        {
            Width = width;
            Length = length;
            Title = title;
        }

        public GraphPlotterOptions()
        {
            Width = 800;
            Length = 800;
            Title = "";
        }

        #region GH_Methods
        public bool IsValid => true;

        public string IsValidWhyNot => "";

        public string TypeName => "PlotSettings";

        public string TypeDescription => "";

        public IGH_Goo Duplicate()
        {
            return (IGH_Goo)this.MemberwiseClone();
        }

        public IGH_GooProxy EmitProxy()
        {
            return null;
        }

        public bool CastFrom(object source)
        {
            return false;
        }

        public bool CastTo<T>(out T target)
        {
            target = default(T);
            return false;
        }

        public object ScriptVariable()
        {
            return null;
        }

        public bool Write(GH_IWriter writer)
        {
            return false;
        }

        public bool Read(GH_IReader reader)
        {
            return false;
        }
        #endregion
    }

    public static class GraphPlotter
	{
        public enum ColorScales { Blackbody, Bluered, Cividis, Earth, Electric, Greens, Greys, Hot, Jet, Picnic, Portland, Rainbow, RdBu, Viridis, YIGnBu, YIOrRd }

        public enum HistogramNormalization { None, Density, Percent, Probability, ProbabilityDensity }

        public enum HistogramFunction { Count, Average, Max, Min, Sum }

        public static StyleParam.Colorscale GetColorScale(ColorScales colorScale)
        {
            switch (colorScale)
            {
                case ColorScales.Blackbody:
                    return StyleParam.Colorscale.Blackbody;
                case ColorScales.Bluered:
                    return StyleParam.Colorscale.Bluered;
                case ColorScales.Cividis:
                    return StyleParam.Colorscale.Cividis;
                case ColorScales.Earth:
                    return StyleParam.Colorscale.Earth;
                case ColorScales.Electric:
                    return StyleParam.Colorscale.Electric;
                case ColorScales.Greens:
                    return StyleParam.Colorscale.Greens;
                case ColorScales.Greys:
                    return StyleParam.Colorscale.Greys;
                case ColorScales.Hot:
                    return StyleParam.Colorscale.Hot;
                case ColorScales.Jet:
                    return StyleParam.Colorscale.Jet;
                case ColorScales.Picnic:
                    return StyleParam.Colorscale.Blackbody;
                case ColorScales.Portland:
                    return StyleParam.Colorscale.Portland;
                case ColorScales.Rainbow:
                    return StyleParam.Colorscale.Rainbow;
                case ColorScales.RdBu:
                    return StyleParam.Colorscale.RdBu;
                case ColorScales.Viridis:
                    return StyleParam.Colorscale.Viridis;
                case ColorScales.YIGnBu:
                    return StyleParam.Colorscale.YIGnBu;
                case ColorScales.YIOrRd:
                    return StyleParam.Colorscale.YIOrRd;
                default:
                    return StyleParam.Colorscale.Viridis;
            }
        }

        public static StyleParam.HistNorm GetHistogramNormalization(HistogramNormalization norm)
        {
            switch (norm)
            {
                case HistogramNormalization.Density:
                    return StyleParam.HistNorm.Density;
                case HistogramNormalization.Percent:
                    return StyleParam.HistNorm.Percent;
                case HistogramNormalization.None:
                    return StyleParam.HistNorm.None;
                case HistogramNormalization.Probability:
                    return StyleParam.HistNorm.Probability;
                case HistogramNormalization.ProbabilityDensity:
                    return StyleParam.HistNorm.ProbabilityDensity;
                default:
                    return StyleParam.HistNorm.None;
            }
        }

        public static StyleParam.HistFunc GetHistogramFunction(HistogramFunction func)
        {
            switch (func)
            {
                case HistogramFunction.Average:
                    return StyleParam.HistFunc.Avg;
                case HistogramFunction.Count:
                    return StyleParam.HistFunc.Count;
                case HistogramFunction.Max:
                    return StyleParam.HistFunc.Max;
                case HistogramFunction.Min:
                    return StyleParam.HistFunc.Min;
                case HistogramFunction.Sum:
                    return StyleParam.HistFunc.Sum;
                default:
                    return StyleParam.HistFunc.Count;
            }
        }

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
            string[] multiText = Enumerable.Range(1, maxNumGroups + 1).Select(i => i.ToString()).ToArray();
            double maxBnd = Math.PI;
            var xbins = Bins.init(0.0, maxBnd, maxBnd / maxNumGroups);
            var chart1 = Chart2D.Chart.Histogram<double, double, string>(
                X: angles, MultiText: multiText, MarkerColor: Color.fromKeyword(ColorKeyword.CadetBlue),
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
            chart1.WithTitle("Joint Distribution (" + jType.ToString() +")");
            chart2.WithTitle("Joint (" + jType.ToString() + ")");
            chart1.Show();
            chart2.Show();
        }

        public static void HistogramAreas(double[] areas, string quadType)
        {
            Defaults.DefaultTemplate = ChartTemplates.lightMirrored;
            int maxNumGroups = 10;
            string[] multiText = Enumerable.Range(1, maxNumGroups+1).Select( i => i.ToString()).ToArray();
            var chart1 = Chart2D.Chart.Histogram<double, double, string>(X: areas, MultiText: multiText, MarkerColor: Color.fromKeyword(ColorKeyword.CadetBlue), Opacity: 0.75, HistFunc: StyleParam.HistFunc.Avg, HistNorm: StyleParam.HistNorm.None, NBinsX: maxNumGroups);
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
            string[] multiText = Enumerable.Range(1, maxNumGroups + 1).Select(i => i.ToString()).ToArray();
            var chart1 = Chart2D.Chart.Histogram<double, double, string>(X: data, MultiText: multiText, MarkerColor: Color.fromKeyword(ColorKeyword.CadetBlue), Opacity: 0.75, HistFunc: StyleParam.HistFunc.Avg, HistNorm: StyleParam.HistNorm.None, NBinsX: maxNumGroups);
            var chart2 = Chart2D.Chart.Column<double, string, string, string, string>(values: data);

            chart1.WithTitle("Segments " + segmentType + " Distribution");
            chart2.WithTitle("Segments " + segmentType);
            chart1.Show();
            chart2.Show();
        }

        public static void HistogramLinkagesScalarFields(double[] data, string linkageType)
        {
            Defaults.DefaultTemplate = ChartTemplates.darkMirrored;
            int maxNumGroups = 10;
            string[] multiText = Enumerable.Range(1, maxNumGroups + 1).Select(i => i.ToString()).ToArray();
            var chart1 = Chart2D.Chart.Histogram<double, double, string>(X: data, MultiText: multiText, MarkerColor: Color.fromKeyword(ColorKeyword.CadetBlue), Opacity: 0.75, HistFunc: StyleParam.HistFunc.Avg, HistNorm: StyleParam.HistNorm.None, NBinsX: maxNumGroups);

            chart1.WithTitle(linkageType + " Distribution");
            chart1.Show();
        }

        public static void RangePlots(GraphPlotterOptions options, double[] dataX, double[] dataY, double[] upperY, double[] lowerY, string[] dataLabels, string[] upperLabels, string[] lowerLabels, bool showLegend = true)
        {
            if (dataX.Length != dataY.Length) throw new Exception("Invalid data count.");
            int count = dataY.Length;
            if (dataLabels.Length == 0) dataLabels = null;
            if (lowerLabels.Length == 0) lowerLabels = null;
            if (upperLabels.Length == 0) upperLabels = null;

            Tuple<double, double>[] xy = new Tuple<double, double>[count];
            for (int i = 0; i < count; i++) xy[i] = new Tuple<double, double>(dataX[i], dataY[i]);

            var chart = Chart2D.Chart.Range<double,double,double,double,string,string,string>(xy, upper: upperY, lower: lowerY, mode: StyleParam.Mode.Lines_Markers,
                TextPosition: StyleParam.TextPosition.TopCenter, MultiText: dataLabels, MultiLowerText: lowerLabels, MultiUpperText: upperLabels,
                MarkerColor : Color.fromString("grey"), RangeColor: Color.fromString("lightblue"), ShowLegend: showLegend);
            chart.WithTitle(options.Title);
            chart.WithSize(options.Width, options.Length);

            chart.Show();
        }

        public static void Histogram(GraphPlotterOptions options, double[] data, string[] groupNames, int maxNumGroups, HistogramNormalization normalization = HistogramNormalization.None, HistogramFunction function = HistogramFunction.Count, bool showLegend=true)
        {
            var norm = GetHistogramNormalization(normalization);
            var func = GetHistogramFunction(function);
            if (groupNames.Length == 0) groupNames = null;

            var chart = Chart2D.Chart.Histogram<double, double, string>(X: data, MultiText: groupNames, Opacity: 0.75, HistFunc: func, HistNorm: norm, NBinsX: maxNumGroups, ShowLegend: showLegend, MarkerOutline: Plotly.NET.Line.init(Color: Color.fromKeyword(ColorKeyword.Black), Width: 1));
            chart.WithTitle(options.Title);
            chart.WithSize(options.Width, options.Length);

            chart.Show();
        }

        public static void PointDensity(GraphPlotterOptions options, double[] dataX, double[] dataY, string scaleLabel, ColorScales colorScale=ColorScales.Viridis, HistogramNormalization normalization = HistogramNormalization.None, bool showContourLines=true, int numberContours=5)
        {
            var cScale = GetColorScale(colorScale);
            var norm = GetHistogramNormalization(normalization);

            var chart = Chart2D.Chart.PointDensity<double, double>(x: dataX, y: dataY,
                PointOpacity: 1.0, PointMarkerColor: Color.fromKeyword(ColorKeyword.White), PointMarkerSymbol: StyleParam.MarkerSymbol.CircleCross,
                PointMarkerSize: 5, ContourLineColor: Color.fromKeyword(ColorKeyword.White), ContourLineSmoothing: 100.0,
                ColorBar: ColorBar.init<double, double>(Title: Title.init(scaleLabel)), ColorScale: cScale, ShowScale: true,
                ShowContourLabels: false, ContourLineWidth: 0.5, ShowContourLines: showContourLines, ContourColoring: StyleParam.ContourColoring.Heatmap,
                NContours: numberContours, HistNorm: norm, ContourOpacity: 1.0, UseDefaults: true);
            chart.WithTitle(options.Title);
            chart.WithSize(options.Width, options.Length);

            chart.Show();
        }

        public static void Point3DChart(GraphPlotterOptions options, double[] dataX, double[] dataY, double[] dataZ, string[] names, string labelX, string labelY, string labelZ)
        {
            StyleParam.SubPlotId scene = StyleParam.SubPlotId.Scene.NewScene(1);
            StyleParam.TextPosition[] textPos = dataX.Select(idx => StyleParam.TextPosition.MiddleRight).ToArray();

            if (names.Length == 0) names = null;
            var chart = Chart3D.Chart.Point3D<double, double, double, string>(dataX, dataY, dataZ, MultiText: names, UseDefaults: true, MultiTextPosition: textPos);
            chart.WithXAxisStyle(Title.init(labelX), Id: scene);
            chart.WithYAxisStyle(Title.init(labelY), Id: scene);
            chart.WithZAxisStyle(Title.init(labelZ));
            chart.WithTitle(options.Title);
            chart.WithSize(options.Width, options.Length);

            chart.Show();
        }

        public static void Line3DChart(GraphPlotterOptions options, double[] dataX, double[] dataY, double[] dataZ, double[] dataW, string[] names, string labelX, string labelY, string labelZ, double lineWidth=10, ColorScales colorScale = ColorScales.Viridis, bool showMarkers=true)
        {
            if (names.Length == 0) names = null;
            var cScale = GetColorScale(colorScale);
            StyleParam.SubPlotId scene = StyleParam.SubPlotId.Scene.NewScene(1);
            StyleParam.TextPosition[] textPos = dataX.Select(idx => StyleParam.TextPosition.MiddleRight).ToArray();

            var chart = Chart3D.Chart.Line3D<double, double, double, string>(dataX, dataY, dataZ, MultiText: names, ShowMarkers: showMarkers, LineWidth: lineWidth, MultiTextPosition: textPos, LineColorScale: cScale, LineColor: Color.fromColorScaleValues(dataW), UseDefaults: true);
            chart.WithXAxisStyle(Title.init(labelX), Id: scene);
            chart.WithYAxisStyle(Title.init(labelY), Id: scene);
            chart.WithZAxisStyle(Title.init(labelZ));
            chart.WithTitle(options.Title);
            chart.WithSize(options.Width, options.Length);

            chart.Show();
        }
    }
}

