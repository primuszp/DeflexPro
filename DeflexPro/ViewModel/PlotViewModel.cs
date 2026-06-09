using System;
using System.Collections.ObjectModel;
using System.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using DeflexPro.Model;
using DeflexPro.Localization;

namespace DeflexPro.ViewModel
{
    public class PlotViewModel : ViewModelBase
    {
        private PlotModel model;

        // Látható paletta sötét háttéren
        private static readonly OxyColor[] Palette =
        [
            OxyColor.FromRgb(38,  198, 163),   // teal
            OxyColor.FromRgb(255, 183,  77),   // amber
            OxyColor.FromRgb(129, 199, 255),   // sky blue
            OxyColor.FromRgb(255, 102, 133),   // coral
            OxyColor.FromRgb(178, 235, 100),   // lime
            OxyColor.FromRgb(206, 147, 216),   // lavender
            OxyColor.FromRgb(255, 224, 130),   // yellow
            OxyColor.FromRgb( 77, 208, 225),   // cyan
        ];

        private readonly OxyColor gridColor   = OxyColor.FromRgb(38, 55, 84);
        private readonly OxyColor borderColor = OxyColor.FromRgb(46, 68, 112);

        [CLSCompliantAttribute(false)]
        public PlotModel Model
        {
            get => model;
            set { if (model != value) { model = value; RaisePropertyChanged("Model"); } }
        }

        public PlotViewModel()
        {
            model = CreateEmptyModel();
        }

        private PlotModel CreateEmptyModel()
        {
            var m = new PlotModel
            {
                Background           = OxyColors.Transparent,
                PlotAreaBackground   = OxyColor.FromArgb(40, 10, 20, 48),
                PlotAreaBorderColor  = borderColor,
                PlotAreaBorderThickness = new OxyThickness(1),
                TextColor            = OxyColor.FromRgb(224, 232, 242),
                TitleColor           = OxyColor.FromRgb(240, 246, 255),
                TitleFontSize        = 13,
                DefaultFont          = "Segoe UI",
            };

            m.Legends.Add(new Legend
            {
                LegendPosition        = LegendPosition.TopRight,
                LegendPlacement       = LegendPlacement.Inside,
                LegendOrientation     = LegendOrientation.Vertical,
                LegendBackground      = OxyColor.FromArgb(200, 11, 18, 32),
                LegendBorder          = borderColor,
                LegendBorderThickness = 1,
                LegendTextColor       = OxyColor.FromRgb(224, 232, 242),
                LegendFontSize        = 11,
            });

            m.Axes.Add(new LinearAxis
            {
                Title                = Localizer.Get("DeflectionAxis", "Deflection (μm)"),
                Position             = AxisPosition.Left,
                StartPosition        = 1,   // tengelyfordítás: nagy értékek lent
                EndPosition          = 0,
                MinimumPadding       = 0.08,
                MaximumPadding       = 0.08,
                TicklineColor        = gridColor,
                MajorGridlineStyle   = LineStyle.Solid,
                MajorGridlineColor   = OxyColor.FromArgb(60, 38, 55, 84),
                MinorGridlineStyle   = LineStyle.Dot,
                MinorGridlineColor   = OxyColor.FromArgb(30, 38, 55, 84),
                TickStyle            = TickStyle.Outside,
                AxislineStyle        = LineStyle.Solid,
                AxislineColor        = borderColor,
                TitleFontSize        = 12,
                FontSize             = 11,
            });

            m.Axes.Add(new LinearAxis
            {
                Title                = Localizer.Get("SensorDistanceAxis", "Sensor distance (mm)"),
                Position             = AxisPosition.Bottom,
                MinimumPadding       = 0.05,
                MaximumPadding       = 0.05,
                TicklineColor        = gridColor,
                MajorGridlineStyle   = LineStyle.Solid,
                MajorGridlineColor   = OxyColor.FromArgb(60, 38, 55, 84),
                MinorGridlineStyle   = LineStyle.Dot,
                MinorGridlineColor   = OxyColor.FromArgb(30, 38, 55, 84),
                TickStyle            = TickStyle.Outside,
                AxislineStyle        = LineStyle.Solid,
                AxislineColor        = borderColor,
                TitleFontSize        = 12,
                FontSize             = 11,
            });

            return m;
        }

        // Iteratív illesztés – rendezett deflekciók alapján, d0 = center sensor
        private double[] Iterative(double d0, System.Collections.Generic.List<Deflection> sortedDeflections)
        {
            double bestRmse = double.MaxValue;
            double bestL = 500, bestN = 0.5, bestC = 2.0;

            for (double l = 80; l <= 1800; l += 20)
            {
                for (double n = 0.2; n <= 1.5; n += 0.1)
                {
                    for (double c = 1.5; c <= 3.0; c += 0.15)
                    {
                        double rmse = 0;
                        int count = 0;

                        foreach (var def in sortedDeflections)
                        {
                            if (def.Measure == 0.0 || double.IsNaN(def.Measure)) continue;
                            double dc = FittBasin.Deflection2(def.Sensor.X, d0, n, l, c);
                            rmse += Math.Pow((dc - def.Measure) / def.Measure, 2);
                            count++;
                        }

                        if (count == 0) continue;
                        rmse = Math.Pow(rmse / count, 0.5) * 100;

                        if (rmse < bestRmse)
                        {
                            bestRmse = rmse;
                            bestL = l; bestN = n; bestC = c;
                        }
                    }
                }
            }

            return [bestL, bestN, bestC, bestRmse];
        }

        public void FillData(ObservableCollection<DropDetailsViewModel> drops)
        {
            model.Series.Clear();

            int colorIdx = 0;
            foreach (var drop in drops)
            {
                if (drop.Deflections == null || drop.Deflections.Count == 0) continue;

                // Csak pozitív X szenzorokat ábrázolunk (radiális elrendezés)
                var deflections = drop.Deflections
                    .Where(d => d.Sensor.X >= 0 && !double.IsNaN(d.Measure) && !double.IsNaN(d.Sensor.X))
                    .OrderBy(d => d.Sensor.X)
                    .ToList();

                if (deflections.Count == 0) continue;

                // d0 = legkisebb X offsetű szenzor (center)
                double d0 = deflections[0].Measure;
                if (d0 == 0 || double.IsNaN(d0)) continue;

                double[] parameters = Iterative(d0, deflections);
                double lp   = parameters[0];
                double np   = parameters[1];
                double cp   = parameters[2];
                double rmse = parameters[3];

                OxyColor color = Palette[colorIdx % Palette.Length];
                colorIdx++;

                string dropLabel = string.Format(
                    Localizer.Get("DropRmseFormat", "Drop #{0}  (RMSE: {1:0.1}%)"),
                    drop.ImpNumber,
                    rmse);

                // Illesztett görbe
                var fitLine = new LineSeries
                {
                    Title           = dropLabel,
                    Color           = color,
                    StrokeThickness = 2.2,
                    LineStyle       = LineStyle.Solid,
                    RenderInLegend  = true,
                };

                double xMax = deflections[deflections.Count - 1].Sensor.X;
                int steps = 200;
                for (int i = 0; i <= steps; i++)
                {
                    double x = xMax * i / steps;
                    fitLine.Points.Add(new DataPoint(x, FittBasin.Deflection2(x, d0, np, lp, cp)));
                }

                // Mért pontok – ugyanolyan szín, de nem jelenik meg a legendben
                var measured = new ScatterSeries
                {
                    Title          = null,
                    MarkerType     = MarkerType.Diamond,
                    MarkerSize     = 4,
                    MarkerFill     = OxyColors.Transparent,
                    MarkerStroke   = color,
                    MarkerStrokeThickness = 1.5,
                    RenderInLegend = false,
                };

                foreach (var def in deflections)
                    measured.Points.Add(new ScatterPoint(def.Sensor.X, def.Measure));

                model.Series.Add(fitLine);
                model.Series.Add(measured);
            }

            model.InvalidatePlot(true);
            RaisePropertyChanged("Model");
            SeriesChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler? SeriesChanged;
    }
}
